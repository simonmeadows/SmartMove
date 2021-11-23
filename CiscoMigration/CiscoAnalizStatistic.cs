﻿/********************************************************************
Copyright (c) 2017, Check Point Software Technologies Ltd.
All rights reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
********************************************************************/

namespace CiscoMigration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CheckPointObjects;
    using MigrationBase;

    namespace CiscoMigration
    {
        class CiscoAnalizStatistic : VendorAnalizStatistic
        {
            List<CheckPoint_Package> cpPackages { get; set; }
            List<CheckPoint_NAT_Rule> cpNatRules { get; set; }

            public override void CalculateNetworks(List<CheckPoint_Network> _cpNetworks,
                                                   List<CheckPoint_NetworkGroup> _cpNetworkGroups,
                                                   List<CheckPoint_Host> _cpHosts,
                                                   List<CheckPoint_Range> _cpRanges)
            {
                _totalNetworkObjectsCount = _cpNetworks.Count + _cpHosts.Count + _cpNetworkGroups.Count + _cpRanges.Count;
                List<string> vs = new List<string>();
                foreach (var item in _cpNetworkGroups) { vs.AddRange(item.Members); }
                _nestedNetworkGroupsCount = vs.Distinct().Count();

                foreach (var item in _cpNetworks)
                {
                    if (_cpNetworks.Where(nt => nt.Netmask == item.Netmask & nt.Subnet == nt.Subnet).Count() > 1) { _duplicateServicesObjectsCount++; }
                }
                foreach (var item in _cpHosts)
                {
                    if (_cpHosts.Where(nt => nt.IpAddress == item.IpAddress ).Count() > 1) { _duplicateServicesObjectsCount++; }
                }
                foreach (var item in _cpRanges)
                {
                    if (_cpRanges.Where(nt => nt.RangeFrom == item.RangeFrom & nt.RangeTo == nt.RangeTo).Count() > 1) { _duplicateServicesObjectsCount++; }
                }
                foreach (var item in cpNatRules)
                {
                    vs.RemoveAll(vv => item.Service != null ? (vv == item.Service.Name) : false || item.Source != null ? (vv == item.Source.Name) : false || item.Destination != null ? (vv == item.Destination.Name) : false);
                }
                _unusedNetworkObjectsCount = vs.Count();
            }

            public override void CalculateRules(List<CheckPoint_Package> _cpPackages,
                                                List<CheckPoint_NAT_Rule> _cpNatRules)
            {
                cpPackages = _cpPackages;
                cpNatRules = _cpNatRules;
                _totalServicesRulesCount = _cpNatRules.Count;
                _rulesServicesutilizingServicesAnyCount = 0;
                foreach (var rule in _cpNatRules)
                {
                    bool first = true;
                    if (!rule.Enabled) { _disabledServicesRulesCount++; }
                    if (rule.Comments == "") { _uncommentedServicesRulesCount++; }
                    if (rule.Name == "") { _unnamedServicesRulesCount++; }
                    if (rule.Source != null && (rule.Source.Name == "any" | rule.Source.Name == "ANY")) { _rulesServicesutilizingServicesAnySourceCount++; if (first) { _rulesServicesutilizingServicesAnyCount++; first = false; } }
                    if (rule.Destination != null && (rule.Destination.Name == "any" | rule.Destination.Name == "ANY")) { _rulesServicesutilizingServicesAnyDestinationCount++; if (first) { _rulesServicesutilizingServicesAnyCount++; first = false; } }
                    if (rule.Service != null && (rule.Service.Name == "any" | rule.Service.Name == "ANY")) { _rulesServicesutilizingServicesAnyServiceCount++; if (first) { _rulesServicesutilizingServicesAnyCount++; first = false; } }
                }
                foreach (var package in _cpPackages)
                {
                    _totalServicesRulesCount += package.ParentLayer.Rules.Count;
                    foreach (var rule in package.ParentLayer.Rules)
                    {
                        bool first = true;
                        if (!rule.Enabled) { _disabledServicesRulesCount++; }
                        if (rule.Comments == "") { _uncommentedServicesRulesCount++; }
                        if (rule.Name == "") { _unnamedServicesRulesCount++; }
                        if (rule.Track == CheckPoint_Rule.TrackTypes.None) { _nonServicesLoggingServicesRulesCount++; }
                        if (rule.Source.Where(sr => sr.Name == "any" || sr.Name == "ANY").Count() != 0) { _rulesServicesutilizingServicesAnySourceCount++; if (first) { _rulesServicesutilizingServicesAnyCount++; first = false; } }
                        if (rule.Destination.Where(dst => dst.Name == "any" || dst.Name == "ANY").Count() != 0) { _rulesServicesutilizingServicesAnyDestinationCount++; if (first) { _rulesServicesutilizingServicesAnyCount++; first = false; } }
                        if (rule.Service.Where(srv => srv.Name == "any" || srv.Name == "ANY").Count() != 0) { _rulesServicesutilizingServicesAnyServiceCount++; if (first) { _rulesServicesutilizingServicesAnyCount++; first = false; } }
                        if (rule.Time.Count() != 0) { _timesServicesRulesCount++; }
                        if (rule.IsCleanupRule()) { _cleanupServicesRuleCount++; }
                    }
                    foreach (var subPolicy in package.SubPolicies)
                    {
                        _totalServicesRulesCount += subPolicy.Rules.Count;
                        foreach (var subRule in subPolicy.Rules)
                        {
                            bool first = true;
                            if (!subRule.Enabled) { _disabledServicesRulesCount++; }
                            if (subRule.Comments == "") { _uncommentedServicesRulesCount++; }
                            if (subRule.Name == "") { _unnamedServicesRulesCount++; }
                            if (subRule.Track == CheckPoint_Rule.TrackTypes.None) { _nonServicesLoggingServicesRulesCount++; }
                            if (subRule.Source.Where(sr => sr.Name == "any" || sr.Name == "ANY").Count() != 0) { _rulesServicesutilizingServicesAnySourceCount++; if (first) { _rulesServicesutilizingServicesAnyCount++; first = false; } }
                            if (subRule.Destination.Where(dst => dst.Name == "any" || dst.Name == "ANY").Count() != 0) { _rulesServicesutilizingServicesAnyDestinationCount++; if (first) { _rulesServicesutilizingServicesAnyCount++; first = false; } }
                            if (subRule.Service.Where(srv => srv.Name == "any" || srv.Name == "ANY").Count() != 0) { _rulesServicesutilizingServicesAnyServiceCount++; if (first) { _rulesServicesutilizingServicesAnyCount++; first = false; } }
                            if (subRule.Time.Count() != 0) { _timesServicesRulesCount++; }
                            if (subRule.IsCleanupRule()) { _cleanupServicesRuleCount++; }
                        }
                    }
                }
                _optimizationServicesPotentialCount = _disabledServicesRulesCount +
                                                      _unnamedServicesRulesCount +
                                                      _timesServicesRulesCount +
                                                      _nonServicesLoggingServicesRulesCount;

            }

            public override void CalculateServices(List<CheckPoint_TcpService> _cpTcpServices,
                                                   List<CheckPoint_UdpService> _cpUdpServices,
                                                   List<CheckPoint_SctpService> _cpSctpServices,
                                                   List<CheckPoint_IcmpService> _cpIcmpServices,
                                                   List<CheckPoint_DceRpcService> _cpDceRpcServices,
                                                   List<CheckPoint_OtherService> _cpOtherServices,
                                                   List<CheckPoint_ServiceGroup> _cpServiceGroups)
            {
                _totalServicesObjectsCount = _cpTcpServices.Count + _cpUdpServices.Count + _cpSctpServices.Count + _cpDceRpcServices.Count + _cpOtherServices.Count + _cpIcmpServices.Count + _cpServiceGroups.Count;
                List<string> vs = new List<string>();
                foreach (var item in _cpServiceGroups) { vs.AddRange(item.Members); _totalServicesObjectsCount += item.Members.Count; }
                _nestedServicesGroupsCount = vs.Distinct().Count();
                vs = new List<string>();

                foreach (var service in _cpTcpServices)
                {
                    if (service.Name == "") { _unnamedServicesRulesCount++; }
                    if (_cpTcpServices.Where(sr => sr.Name == service.Name & sr.Port == service.Port & sr.SourcePort == service.SourcePort).Count() > 1) { _duplicateServicesObjectsCount++; }

                }
                foreach (var service in _cpUdpServices)
                {
                    if (service.Name == "") { _unnamedServicesRulesCount++; }
                    if (_cpTcpServices.Where(sr => sr.Name == service.Name & sr.Port == service.Port & sr.SourcePort == service.SourcePort).Count() > 1) { _duplicateServicesObjectsCount++; }
                }
                foreach (var service in _cpSctpServices)
                {
                    if (service.Name == "") { _unnamedServicesRulesCount++; }
                    if (_cpTcpServices.Where(sr => sr.Name == service.Name & sr.Port == service.Port & sr.SourcePort == service.SourcePort).Count() > 1) { _duplicateServicesObjectsCount++; }
                }
                foreach (var service in _cpIcmpServices)
                {
                    if (service.Name == "") { _unnamedServicesRulesCount++; }
                    if (_cpTcpServices.Where(sr => sr.Name == service.Name).Count() > 1) { _duplicateServicesObjectsCount++; }
                }
                foreach (var service in _cpDceRpcServices)
                {
                    if (service.Name == "") { _unnamedServicesRulesCount++; }
                    if (_cpTcpServices.Where(sr => sr.Name == service.Name).Count() > 1) { _duplicateServicesObjectsCount++; }
                }
                foreach (var service in _cpOtherServices)
                {
                    if (service.Name == "") { _unnamedServicesRulesCount++; }
                    if (_cpTcpServices.Where(sr => sr.Name == service.Name).Count() > 1) { _duplicateServicesObjectsCount++; }
                }

                foreach (var item in cpNatRules)
                {
                    vs.RemoveAll(vv => item.Service != null ? (vv == item.Service.Name) : false || item.Service != null ? (vv == item.Source.Name) : false || item.Service != null ? (vv == item.Destination.Name) : false);
                }
                foreach (var item in cpPackages)
                {
                    foreach (var rule in item.ParentLayer.Rules)
                    {
                        vs.RemoveAll(vv => rule.Service != null ? (rule.Service.Where(sr => sr.Name == vv).Count() > 0) : false || rule.Source != null ? (rule.Source.Where(sr => sr.Name == vv).Count() > 0) : false || rule.Service != null ? (rule.Destination.Where(ds => ds.Name == vv).Count() > 0) : false);
                    }
                    foreach (var subPol in item.SubPolicies)
                    {
                        foreach (var rule in subPol.Rules)
                        {
                            vs.RemoveAll(vv => rule.Service != null ? (rule.Service.Where(sr => sr.Name == vv).Count() > 0) : false || rule.Source != null ? (rule.Source.Where(sr => sr.Name == vv).Count() > 0) : false || rule.Service != null ? (rule.Destination.Where(ds => ds.Name == vv).Count() > 0) : false);
                        }
                    }
                }
                _unusedServicesObjectsCount = vs.Count();
            }
        }
    }

}
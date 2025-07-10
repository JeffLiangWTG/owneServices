using System.Diagnostics.CodeAnalysis;
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#", Scope = "member", Target = "Enterprise.Customs.CA.Business.MessageManagers.CAEManifestForwarderMessageManager+DefineActionCodeDelegate.#Invoke(Enterprise.Customs.Common.MessageBuilders.MessageSubTypes&)")]
[assembly: SuppressMessage("CargoWiseOne", "CW1123:new CachedValue", Scope = "namespaceanddescendants", Target = "~N:Enterprise.Customs.CA.Business", Justification = "Properties using CachedValue should use CachedValueHelper.")]
[assembly: SuppressMessage("CargoWiseOne", "CW1124:new CachedProperty", Scope = "namespaceanddescendants", Target = "~N:Enterprise.Customs.CA.Business", Justification = "Properties using CachedProperty should use CachedValueHelper.")]
[assembly: SuppressMessage("CargoWiseOne", "EDI001:Resource String Static Reference Rule", Justification = "Baseline WI00613594", Scope = "member", Target = "~F:Enterprise.Customs.CA.Business.CusCALPCO.AllLPCOFields")] // Enterprise/Product/Operations/Customs/CA/Business/Business/CusCALPCO/CusCALPCO.cs:544,47
[assembly: SuppressMessage("CargoWiseOne", "EDI003:Business Object Property Max Length Validation", Justification = "Baseline WI00752625", Scope = "member", Target = "~M:Enterprise.Customs.CA.Business.DutyAndTaxManager.AddGeneralClassificationDutyRate")] // Enterprise/Product/Operations/Customs/CA/Business/Business/JobComInvoiceLine/Tariff/DutyAndTax/DutyAndTaxManager.cs:1092,29
[assembly: SuppressMessage("CargoWiseOne", "CW1194:Usafe BusinessObjectCollection Creation", Justification = "Baseline WI00842925", Scope = "member", Target = "~P:Enterprise.Customs.CA.Business.CusCAeMHHouse.MessagesForDisplay")]
[assembly: SuppressMessage("CargoWiseOne", "CW1194:Usafe BusinessObjectCollection Creation", Justification = "Baseline WI00842925", Scope = "member", Target = "~P:Enterprise.Customs.CA.Business.CusCAeMHMaster.MessagesForDisplay")]
[assembly: SuppressMessage("CargoWiseOne", "CW1194:Usafe BusinessObjectCollection Creation", Justification = "Baseline WI00842925", Scope = "member", Target = "~P:Enterprise.Customs.CA.Business.RefTariffSynchronizeProcessor.HarmonizedTariffList")]
[assembly: SuppressMessage("CargoWiseOne", "CW1194:Usafe BusinessObjectCollection Creation", Justification = "Baseline WI00842925", Scope = "member", Target = "~P:Enterprise.Customs.CA.Business.RNSMessagingBO.MessagesForDisplay")]

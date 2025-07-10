// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("CargoWiseOne", "CW1124:new CachedProperty", Scope = "namespaceanddescendants", Target = "~N:Enterprise.Customs.KR.Business", Justification = "Properties using CachedProperty should use CachedValueHelper.")]
[assembly: SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Baseline WI00605045", Scope = "member", Target = "~F:Enterprise.Customs.KR.Business.IncotermList.incotermsFreightExcludedList")] // Enterprise/Product/Operations/Customs/KR/Business/CodeDescriptionPairList/IncotermListPartial.cs:16,18
[assembly: SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Baseline WI00605045", Scope = "member", Target = "~F:Enterprise.Customs.KR.Business.IncotermList.incotermsInsuranceExcludedList")] // Enterprise/Product/Operations/Customs/KR/Business/CodeDescriptionPairList/IncotermListPartial.cs:17,18
[assembly: SuppressMessage("CargoWiseOne", "EDI003:Business Object Property Max Length Validation", Justification = "Baseline WI00752625", Scope = "member", Target = "~M:Enterprise.Customs.KR.Business.JobDeclaration.SetBondedAreaCode")] // Enterprise/Product/Operations/Customs/KR/Business/Business/JobDeclaration/JobDeclaration.cs:1652,36
[assembly: SuppressMessage("CargoWiseOne", "EDI003:Business Object Property Max Length Validation", Justification = "Baseline WI00752625", Scope = "member", Target = "~M:Enterprise.Customs.KR.Business.JobComInvoiceLine.SetTariffEtcDataFromProductsPivotCore(Enterprise.Customs.Business.BaseCusClassPartPivot)")] // Enterprise/Product/Operations/Customs/KR/Business/Business/JobComInvoiceLine/JobComInvoiceLine.cs:298,19

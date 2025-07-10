// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("CargoWiseOne", "CW1124:new CachedProperty", Scope = "namespaceanddescendants", Target = "~N:Enterprise.Customs.IT.Business", Justification = "Properties using CachedProperty should use CachedValueHelper.")]
[assembly: SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Baseline WI00605045", Scope = "member", Target = "~F:Enterprise.Customs.IT.Business.ITEDIMessage.TypeDecider")] // Enterprise/Product/Operations/Customs/IT/Core/Business/EDIMessage/ITEDIMessage.cs:23,53
[assembly: SuppressMessage("CargoWiseOne", "EDI003:Business Object Property Max Length Validation", Justification = "Baseline WI00752625", Scope = "member", Target = "~M:Enterprise.Customs.IT.Business.InventorySelectionHeaderPreviousDocumentFiller.AddPreviousDocumentForImportNotHavingDashInPreviousEntryNumber")] // Enterprise/Product/Operations/Customs/IT/Core/Business/WarehouseIntegration/InventorySelectionHeaderPreviousDocumentFiller.cs:94,42
[assembly: SuppressMessage("CargoWiseOne", "EDI003:Business Object Property Max Length Validation", Justification = "Baseline WI00752625", Scope = "member", Target = "~M:Enterprise.Customs.IT.Business.InventorySelectionHeaderPreviousDocumentFiller.AddPreviousDocumentForImportHavingDashInPreviousEntryNumber")] // Enterprise/Product/Operations/Customs/IT/Core/Business/WarehouseIntegration/InventorySelectionHeaderPreviousDocumentFiller.cs:73,35

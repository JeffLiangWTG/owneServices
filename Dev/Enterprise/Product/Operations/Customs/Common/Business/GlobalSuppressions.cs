// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("CargoWiseOne", "CW1124:new CachedProperty", Scope = "namespaceanddescendants", Target = "~N:Enterprise.Customs.Common", Justification = "Properties using CachedProperty should use CachedValueHelper.")]
[assembly: SuppressMessage("CargoWiseOne", "CW1121:Do Not Include Column Values Or Names In Error Reporter Keys", Justification = "Auto generated baseline suppressions - WI00545660", Scope = "member", Target = "~P:Enterprise.Customs.Common.CusEntryNumber.CE_EntryNum")] // C:\git\wtg\CargoWise\Dev\Enterprise\Product\Operations\Customs\Common\Business\CusEntryNum\CusEntryNumber.cs:539:30
[assembly: SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Baseline WI00605045", Scope = "member", Target = "~F:Enterprise.Customs.Common.US.SEBillProcessingResultList.Multiple")] // Enterprise/Product/Operations/Customs/Common/Business/US/SEBillProcessingResultListPartial.cs:20,23
[assembly: SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Baseline WI00605045", Scope = "member", Target = "~F:Enterprise.Customs.Common.US.SEBillProcessingResultList.BillStatusHoldOrExam")] // Enterprise/Product/Operations/Customs/Common/Business/US/SEBillProcessingResultListPartial.cs:21,23

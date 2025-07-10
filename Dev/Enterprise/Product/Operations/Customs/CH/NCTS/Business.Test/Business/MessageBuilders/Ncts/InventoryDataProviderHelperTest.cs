using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class InventoryDataProviderHelperTest : TestCase
{
	public void TestIsUnloadingStateMISorDIF() => CombineAssertions(() =>
	{
		AssertEquals("NEW", false, InventoryDataProviderHelper.IsUnloadingStateMISorDIF(NctsUnloadedStateList.Codes.NEW));
		AssertEquals("MIS", true, InventoryDataProviderHelper.IsUnloadingStateMISorDIF(NctsUnloadedStateList.Codes.MIS));
		AssertEquals("DIF", true, InventoryDataProviderHelper.IsUnloadingStateMISorDIF(NctsUnloadedStateList.Codes.DIF));
		AssertEquals("DAM", false, InventoryDataProviderHelper.IsUnloadingStateMISorDIF(NctsUnloadedStateList.Codes.DAM));
		AssertEquals("DEC", false, InventoryDataProviderHelper.IsUnloadingStateMISorDIF(NctsUnloadedStateList.Codes.DEC));
		AssertEquals("empty", false, InventoryDataProviderHelper.IsUnloadingStateMISorDIF(ZString.Empty));
	});

	public void TestIsUnloadingStateNEWorMISorDIF() => CombineAssertions(() =>
	{
		AssertEquals("NEW", true, InventoryDataProviderHelper.IsUnloadingStateNEWorMISorDIF(NctsUnloadedStateList.Codes.NEW));
		AssertEquals("MIS", true, InventoryDataProviderHelper.IsUnloadingStateNEWorMISorDIF(NctsUnloadedStateList.Codes.MIS));
		AssertEquals("DIF", true, InventoryDataProviderHelper.IsUnloadingStateNEWorMISorDIF(NctsUnloadedStateList.Codes.DIF));
		AssertEquals("DAM", false, InventoryDataProviderHelper.IsUnloadingStateNEWorMISorDIF(NctsUnloadedStateList.Codes.DAM));
		AssertEquals("DEC", false, InventoryDataProviderHelper.IsUnloadingStateNEWorMISorDIF(NctsUnloadedStateList.Codes.DEC));
		AssertEquals("empty", false, InventoryDataProviderHelper.IsUnloadingStateNEWorMISorDIF(ZString.Empty));
	});

	public void TestIsUnloadingStateDECorMISorDIF() => CombineAssertions(() =>
	{
		AssertEquals("NEW", false, InventoryDataProviderHelper.IsUnloadingStateDECorMISorDIF(NctsUnloadedStateList.Codes.NEW));
		AssertEquals("MIS", true, InventoryDataProviderHelper.IsUnloadingStateDECorMISorDIF(NctsUnloadedStateList.Codes.MIS));
		AssertEquals("DIF", true, InventoryDataProviderHelper.IsUnloadingStateDECorMISorDIF(NctsUnloadedStateList.Codes.DIF));
		AssertEquals("DAM", false, InventoryDataProviderHelper.IsUnloadingStateDECorMISorDIF(NctsUnloadedStateList.Codes.DAM));
		AssertEquals("DEC", true, InventoryDataProviderHelper.IsUnloadingStateDECorMISorDIF(NctsUnloadedStateList.Codes.DEC));
		AssertEquals("empty", false, InventoryDataProviderHelper.IsUnloadingStateDECorMISorDIF(ZString.Empty));
	});

	public void TestIsUnloadingStateDECorNEWorDIForDAM() => CombineAssertions(() =>
	{
		AssertEquals("NEW", true, InventoryDataProviderHelper.IsUnloadingStateDECorNEWorDIForDAM(NctsUnloadedStateList.Codes.NEW));
		AssertEquals("MIS", false, InventoryDataProviderHelper.IsUnloadingStateDECorNEWorDIForDAM(NctsUnloadedStateList.Codes.MIS));
		AssertEquals("DIF", true, InventoryDataProviderHelper.IsUnloadingStateDECorNEWorDIForDAM(NctsUnloadedStateList.Codes.DIF));
		AssertEquals("DAM", true, InventoryDataProviderHelper.IsUnloadingStateDECorNEWorDIForDAM(NctsUnloadedStateList.Codes.DAM));
		AssertEquals("DEC", true, InventoryDataProviderHelper.IsUnloadingStateDECorNEWorDIForDAM(NctsUnloadedStateList.Codes.DEC));
		AssertEquals("empty", false, InventoryDataProviderHelper.IsUnloadingStateDECorNEWorDIForDAM(ZString.Empty));
	});

	public void TestIsDifferent() => CombineAssertions(() =>
	{
		ZString declared = "declared";

		AssertIsDifferent("Empty", false, ZString.Empty);
		AssertIsDifferent("Same", false, declared);
		AssertIsDifferent("Different", true, "unloaded");

		void AssertIsDifferent(string message, bool expectedResult, ZString unloadedValue) => AssertEquals(message, expectedResult, InventoryDataProviderHelper.IsDifferent(declared, unloadedValue));
	});
}

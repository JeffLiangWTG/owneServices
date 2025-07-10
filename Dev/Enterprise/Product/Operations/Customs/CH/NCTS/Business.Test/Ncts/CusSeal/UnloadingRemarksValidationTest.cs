using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

internal class UnloadingRemarksValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCY_Code()
	{
		var codeData = Factory.New<UnloadingRemarks>();
		CombineAssertions(() =>
		{
			codeData.CY_Code = "";
			AssertNoNotifications("When empty", codeData.CY_CodeInfo);
			codeData.CY_Code = "@";
			AssertNoNotifications("When invalid code", codeData.CY_CodeInfo);
		});
	}

	public void TestCheckCY_Data()
	{
		const string messageError = "Unloading Remarks are required";
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		var seal = header.ArrivalHeaderContainers.AddNew().Seals.AddNew();
		seal.UnloadingRemarksText = ZString.Empty;
		var unloadingRemarks = new UnloadingRemarksCollection(seal).FindOrCreate();

		seal.BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
		unloadingRemarks.Validation.ValidateCY_Data();
		AssertHasMessageError("NEW", unloadingRemarks.CY_DataInfo, messageError);

		seal.BK_UnloadingState = NctsUnloadedStateList.Codes.MIS;
		unloadingRemarks.Validation.ValidateCY_Data();
		AssertHasMessageError("MIS", unloadingRemarks.CY_DataInfo, messageError);

		seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DIF;
		unloadingRemarks.Validation.ValidateCY_Data();
		AssertHasMessageError("DIF", unloadingRemarks.CY_DataInfo, messageError);

		seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;
		unloadingRemarks.Validation.ValidateCY_Data();
		AssertNoMessageError("DEC", unloadingRemarks.CY_DataInfo, messageError);
	}
}

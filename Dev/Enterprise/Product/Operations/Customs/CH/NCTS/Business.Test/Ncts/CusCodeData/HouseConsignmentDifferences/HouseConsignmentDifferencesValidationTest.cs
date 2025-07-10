using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(HouseConsignmentDifferencesValidation))]
sealed class HouseConsignmentDifferencesValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCY_Code()
	{
		HouseConsignmentDifference.Parent.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		ValidationTestHelper.AssertInvalidCodeMessageError(HouseConsignmentDifference.CY_CodeInfo, "9", UnloadingRemarkCodeList.Codes.Unknown);

		HouseConsignmentDifference.Parent.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		HouseConsignmentDifference.CY_Code = "9";
		AssertNoMessageErrorContaining($"B9_UnloadedState={HouseConsignmentDifference.Parent.MovementDetail.B9_UnloadedState}", HouseConsignmentDifference.CY_CodeInfo, ListValidation.InvalidCodeMessageError.ToString());
	}

	public void TestCheckCY_Data() => CombineAssertions(() =>
	{
		AssertNoNotifications("Initial", HouseConsignmentDifference.CY_DataInfo);

		HouseConsignmentDifference.Parent.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;

		HouseConsignmentDifference.CY_Code = UnloadingRemarkCodeList.Codes.Other;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(HouseConsignmentDifference.CY_DataInfo, PassarValidationMessages.MessageNS30004, $"When CY_Code is {HouseConsignmentDifference.CY_Code}");

		HouseConsignmentDifference.CY_Data = "Description";
		AssertNoNotifications($"When CY_Code is {HouseConsignmentDifference.CY_Code}", HouseConsignmentDifference.CY_DataInfo);

		HouseConsignmentDifference.CY_Code = UnloadingRemarkCodeList.Codes.NotShipped;
		HouseConsignmentDifference.CY_Data = ZString.Empty;
		AssertNoNotifications($"When CY_Code is {HouseConsignmentDifference.CY_Code}", HouseConsignmentDifference.CY_DataInfo);

		HouseConsignmentDifference.CY_Code = UnloadingRemarkCodeList.Codes.Unknown;
		HouseConsignmentDifference.Validation.ValidateCY_Data();
		AssertNoNotifications($"When CY_Code is {HouseConsignmentDifference.CY_Code}", HouseConsignmentDifference.CY_DataInfo);

		HouseConsignmentDifference.CY_Code = UnloadingRemarkCodeList.Codes.Stolen;
		HouseConsignmentDifference.Validation.ValidateCY_Data();
		AssertNoNotifications($"When CY_Code is {HouseConsignmentDifference.CY_Code}", HouseConsignmentDifference.CY_DataInfo);

		HouseConsignmentDifference.Parent.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		HouseConsignmentDifference.CY_Code = UnloadingRemarkCodeList.Codes.Other;
		AssertNoNotifications($"When CY_Code is {HouseConsignmentDifference.CY_Code}", HouseConsignmentDifference.CY_DataInfo);
	});

	HouseConsignmentDifferences HouseConsignmentDifference => houseConsignmentDifference ?? (houseConsignmentDifference = CreateHouseConsignmentDifference());
	HouseConsignmentDifferences houseConsignmentDifference;

	HouseConsignmentDifferences CreateHouseConsignmentDifference()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.Bills.AddNew().HouseConsignmentDifference;
	}
}

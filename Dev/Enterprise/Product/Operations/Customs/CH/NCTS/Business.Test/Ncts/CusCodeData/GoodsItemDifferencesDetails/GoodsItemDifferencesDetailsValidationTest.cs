using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(GoodsItemDifferencesDetailsValidation))]
sealed class GoodsItemDifferencesDetailsValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCY_Code() => CombineAssertions(() =>
	{
		goodsItemDifferencesDetails.Parent.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(goodsItemDifferencesDetails.CY_CodeInfo, "0", UnloadingRemarkCodeList.Codes.Unknown);

		goodsItemDifferencesDetails.Parent.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		goodsItemDifferencesDetails.CY_Code = ZString.Empty;
		AssertNoNotifications(AssertionMessage(), goodsItemDifferencesDetails.CY_CodeInfo);

		goodsItemDifferencesDetails.Parent.BY_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		goodsItemDifferencesDetails.Parent.Bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(goodsItemDifferencesDetails.CY_CodeInfo, "0", UnloadingRemarkCodeList.Codes.Unknown);
		goodsItemDifferencesDetails.Parent.Bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.DIF;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(goodsItemDifferencesDetails.CY_CodeInfo, "0", UnloadingRemarkCodeList.Codes.Unknown);

		goodsItemDifferencesDetails.Parent.Bill.MovementDetail.B9_UnloadedState = NctsUnloadedStateList.Codes.MIS;
		goodsItemDifferencesDetails.CY_Code = ZString.Empty;
		AssertNoNotifications(AssertionMessage(), goodsItemDifferencesDetails.CY_CodeInfo);

		string AssertionMessage() => $"B9_UnloadingState={goodsItemDifferencesDetails.Parent.Bill.MovementDetail.B9_UnloadedState} BY_UnloadingState={goodsItemDifferencesDetails.Parent.BY_UnloadedState}";
	});

	public void TestCheckCY_Data() => CombineAssertions(() =>
	{
		AssertNoNotifications(goodsItemDifferencesDetails.CY_DataInfo);

		goodsItemDifferencesDetails.CY_Code = UnloadingRemarkCodeList.Codes.Other;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(goodsItemDifferencesDetails.CY_DataInfo, PassarValidationMessages.MessageNS30004, $"When CY_Code is {goodsItemDifferencesDetails.CY_Code}");

		goodsItemDifferencesDetails.CY_Data = "Description";
		AssertNoNotifications($"When CY_Code is {goodsItemDifferencesDetails.CY_Code}", goodsItemDifferencesDetails.CY_DataInfo);

		goodsItemDifferencesDetails.CY_Code = UnloadingRemarkCodeList.Codes.NotShipped;
		goodsItemDifferencesDetails.CY_Data = ZString.Empty;
		AssertNoNotifications($"When CY_Code is {goodsItemDifferencesDetails.CY_Code}", goodsItemDifferencesDetails.CY_DataInfo);

		goodsItemDifferencesDetails.CY_Code = UnloadingRemarkCodeList.Codes.Unknown;
		goodsItemDifferencesDetails.Validation.ValidateCY_Data();
		AssertNoNotifications($"When CY_Code is {goodsItemDifferencesDetails.CY_Code}", goodsItemDifferencesDetails.CY_DataInfo);

		goodsItemDifferencesDetails.CY_Code = UnloadingRemarkCodeList.Codes.Stolen;
		goodsItemDifferencesDetails.Validation.ValidateCY_Data();
		AssertNoNotifications($"When CY_Code is {goodsItemDifferencesDetails.CY_Code}", goodsItemDifferencesDetails.CY_DataInfo);
	});

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		var arrivalGoodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
		goodsItemDifferencesDetails = arrivalGoodsItem.GoodsItemDifferencesDetail;
	}
	GoodsItemDifferencesDetails goodsItemDifferencesDetails;
}

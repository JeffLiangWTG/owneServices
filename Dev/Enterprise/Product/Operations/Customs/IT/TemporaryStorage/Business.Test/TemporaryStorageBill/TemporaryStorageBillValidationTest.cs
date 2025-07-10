using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStorageBillValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckABL_GoodsDescription()
	{
		const string errorMessage = "Enter a Goods Description or a valid Tariff in at least one Item.";
		var header = Factory.New<TemporaryStorageHeader>();
		var tempBill = header.Bills.AddNew();
		var packedItem1 = tempBill.PackedItems.AddNew();

		CombineAssertions(() =>
		{
			tempBill.ABL_GoodsDescription = ZString.Empty;
			packedItem1.API_FormattedTariff = ZString.Empty;
			AssertHasMessageError("When Goods Description and Formatted Tariff is empty", tempBill.ABL_GoodsDescriptionInfo, errorMessage);

			tempBill.ABL_GoodsDescription = "Test Goods Description";
			AssertNoMessageError("When Goods Description is not empty and Formatted Tariff is empty", tempBill.ABL_GoodsDescriptionInfo, errorMessage);

			packedItem1.API_FormattedTariff = "1234567890";
			tempBill.ABL_GoodsDescription = ZString.Empty;
			AssertNoMessageError("When Goods Description is empty and Formatted Tariff is not empty", tempBill.ABL_GoodsDescriptionInfo, errorMessage);
		});
	}

	public void TestCheckABL_BillNumber()
	{
		const string errorMessage = "No Packing Lines against Bill - Please enter some Packing Lines for the Bill";
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();

		bill.Validation.ValidateABL_BillNumber();
		AssertHasMessageErrorContaining(bill.ABL_BillNumberInfo, errorMessage);

		bill.Packs.AddNew();

		bill.Validation.ValidateABL_BillNumber();
		AssertNoMessageErrorContaining(bill.ABL_BillNumberInfo, errorMessage);
	}

	public void TestCheckDuplicateTypeAndNumber()
	{
		const string warningMessage = "Bill Number should be unique.";
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		bill.ABL_BillNumber = "Bill";

		var bill2 = header.Bills.AddNew();
		bill2.ABL_BillNumber = "Bill2";
		AssertNoWarning("Bill Number should not have duplicate warning message if values are not duplicate", bill2.ABL_BillNumberInfo, warningMessage);

		bill2.ABL_BillNumber = "Bill";
		AssertHasWarning("Bill Number should have duplicate warning message if values are duplicate", bill2.ABL_BillNumberInfo, warningMessage);
	}
}

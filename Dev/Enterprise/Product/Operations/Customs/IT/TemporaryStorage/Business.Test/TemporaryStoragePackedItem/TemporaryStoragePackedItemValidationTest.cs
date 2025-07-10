using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStoragePackedItemValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAPI_Tariff()
	{
		const string errorMessage = "Enter the Tariff Code or enter the Description of Goods in the Bill Details tab.";
		var header = Factory.New<TemporaryStorageHeader>();
		var tempBill = header.Bills.AddNew();
		var packedItem = tempBill.PackedItems.AddNew();

		CombineAssertions(() =>
		{
			packedItem.Bill.ABL_GoodsDescription = "Test Goods Description";
			packedItem.API_FormattedTariff = ZString.Empty;
			AssertNoMessageError("When Goods Description is not empty and Formatted Tariff is empty", packedItem.API_FormattedTariffInfo, errorMessage);
			AssertNoNotifications(packedItem.API_FormattedTariffInfo);

			packedItem.API_FormattedTariff = "1234567890";
			packedItem.Bill.ABL_GoodsDescription = ZString.Empty;
			AssertNoMessageError("When Goods Description is empty and Formatted Tariff is not empty", packedItem.API_FormattedTariffInfo, errorMessage);

			packedItem.API_GoodsDescription = ZString.Empty;
			packedItem.API_FormattedTariff = ZString.Empty;
			AssertHasMessageError("When Goods Description and Formatted Tariff is empty", packedItem.API_FormattedTariffInfo, errorMessage);
		});
	}

	public void TestCheckAPI_GoodsDescription()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var tempBill = header.Bills.AddNew();
		var packedItem = tempBill.PackedItems.AddNew();

		packedItem.API_GoodsDescription = ZString.Empty;
		AssertNoNotifications(packedItem.API_GoodsDescriptionInfo);
	}

	public void TestCheckAPI_GrossWeightUQ()
	{
		const string errorMessage = "The code you have selected is not in the list.";
		var header = Factory.New<TemporaryStorageHeader>();
		var tempBill = header.Bills.AddNew();
		var packedItem = tempBill.PackedItems.AddNew();

		CombineAssertions(() =>
		{
			packedItem.API_GrossWeightUQ = "G";
			AssertNoMessageError("When Gross Weight UQ is valid", packedItem.API_GrossWeightUQInfo, errorMessage);

			packedItem.API_GrossWeightUQ = "KG";
			AssertNoMessageError("When Gross Weight UQ is valid", packedItem.API_GrossWeightUQInfo, errorMessage);

			packedItem.API_GrossWeightUQ = "PP";
			AssertHasMessageError("When Gross Weight UQ is invalid", packedItem.API_GrossWeightUQInfo, errorMessage);
		});
	}

	public void TestCheckAPI_CustomsUQ2()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");

		helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "GHI", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));

		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
		Factory.Save();

		var header = Factory.New<TemporaryStorageHeader>();
		var tempBill = header.Bills.AddNew();
		var packedItem = tempBill.PackedItems.AddNew();

		ValidationTestHelper.AssertInvalidCodeMessageError(packedItem.API_CustomsUQ2Info, "PP", "GHI");
	}

	public void TestCheckAPI_CustomsQty2()
	{
		const decimal maxAllowedValue = 9999999999.999999m;
		var header = Factory.New<TemporaryStorageHeader>();
		var tempBill = header.Bills.AddNew();
		var packedItem = tempBill.PackedItems.AddNew();

		CombineAssertions(() =>
		{
			packedItem.API_CustomsQty2 = maxAllowedValue - 1;
			AssertNoWarnings("When Customs Qty2 is within the allowed limit", packedItem.API_CustomsQty2Info);

			packedItem.API_CustomsQty2 = maxAllowedValue + 1;
			var warningMessage = "the maximum value allowed for Supplementary quantity is 9,999,999,999.999999";
			AssertHasWarningContaining("When Customs Qty2 exceeds the allowed limit", packedItem.API_CustomsQty2Info, warningMessage);
		});
	}

	public void TestCheckAPI_ChemicalSubstanceCode()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "ECICS");

		var today = ZDateTime.Today;
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy,
			UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "ValidCode", "Valid Chemical Code", today.AddDays(-1), today.AddDays(1));
		Factory.Save();

		var header = Factory.New<TemporaryStorageHeader>();
		var tempBill = header.Bills.AddNew();
		var packedItem = tempBill.PackedItems.AddNew();

		ValidationTestHelper.AssertInvalidCodeMessageError(packedItem.API_ChemicalSubstanceCodeInfo, "000000-0", "ValidCode");
	}

	public void TestCheckSupportingDocumentIsEmpty()
	{
		var bill = Factory.New<TemporaryStorageBill>();
		var packedItem = bill.PackedItems.AddNew();
		packedItem.Validation.ValidateAll();
		AssertHasRowMessageErrorContaining(packedItem, "In Supporting Documents, you have to enter at least one row.");

		var supportingDocument = packedItem.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = "CODE1";
		supportingDocument.CSI_ReferenceNumber = "REF1";
		packedItem.Validation.ValidateAll();
		AssertNoRowMessageErrorContaining(packedItem, "In Supporting Documents, you have to enter at least one row.");
	}

	public void TestCheckPreviousDocumentIsEmpty()
	{
		var bill = Factory.New<TemporaryStorageBill>();
		var packedItem = bill.PackedItems.AddNew();

		packedItem.Validation.ValidateAll();

		AssertHasRowMessageError(packedItem, "In Previous Documents, you have to enter at least one row.");

		var previousDocument = packedItem.PreviousDocuments.AddNew();
		previousDocument.CSI_ReferenceNumber = "REF1";
		previousDocument.CSI_CustomsOffice = "OFFICE1";

		packedItem.Validation.ValidateAll();

		AssertNoRowMessageError(packedItem, "In Previous Documents, you have to enter at least one row.");
	}
}

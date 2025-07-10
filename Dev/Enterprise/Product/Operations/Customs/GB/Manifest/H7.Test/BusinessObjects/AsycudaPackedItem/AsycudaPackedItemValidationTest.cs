using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.H7.Business.Testing;

sealed class AsycudaPackedItemValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAPI_GrossWeight()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var packedItem = bill.PackedItems.AddNew();

		packedItem.API_GrossWeight = ZDecimal.Zero;
		AssertHasMessageError(packedItem.API_GrossWeightInfo, "You have not entered a Gross Weight.");

		packedItem.API_GrossWeight = 1000.000;
		AssertNoMessageErrors(packedItem.API_GrossWeightInfo);
	}

	public void TestCheckAPI_NetWeight()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var packedItem = bill.PackedItems.AddNew();

		packedItem.API_GrossWeight = 2200;
		packedItem.API_GrossWeightUQ = Weight.Grams;

		packedItem.API_NetWeight = 3;
		packedItem.API_NetWeightUQ = Weight.Kilograms;

		AssertHasMessageError(packedItem.API_NetWeightInfo, "Net mass should be less than gross mass.");

		packedItem.API_NetWeight = 2;
		AssertNoMessageErrors(packedItem.API_NetWeightInfo);
	}

	public void TestCheckAPI_FormattedTariff()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var packedItem = bill.PackedItems.AddNew();

		packedItem.API_FormattedTariff = string.Empty;
		AssertHasMessageError(packedItem.API_FormattedTariffInfo, "You have not entered a Tariff.");

		packedItem.API_Tariff = "aa123";
		AssertHasMessageError(packedItem.API_FormattedTariffInfo,
			"The Tariff value entered must be at least 6 alphanumeric characters.");

		packedItem.API_Tariff = "!#^330";
		AssertHasMessageError(packedItem.API_FormattedTariffInfo,
			"The Tariff value entered must be at least 6 alphanumeric characters.");

		var applicationBusinessProvider = packedItem.Header.ApplicationBusinessProvider;
		var dataGrouping = string.IsNullOrWhiteSpace(applicationBusinessProvider?.PackedItemTariffDataGrouping)
			? GlbCompany.CurrentCompany.GC_RN_NKCountryCode
			: applicationBusinessProvider.PackedItemTariffDataGrouping;
		var tariffTypeCode = applicationBusinessProvider?.PackedItemTariffType ?? "HSN";
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
		var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, tariffTypeCode);
		var tariffView = helper.LoadOrCreateNewTariff(dataGrouping, tariffType.PK, "12345678",
			ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		packedItem.API_Tariff = tariffView.ZZ1_TariffCode;
		AssertNoMessageErrors(packedItem.API_FormattedTariffInfo);
	}

	public void TestCheckAPI_GoodsValue()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		bill.AdditionalProcedureCodes.AddNew("4000C08");

		var packedItem1 = bill.PackedItems.AddNew();
		packedItem1.API_GoodsValue = 30;
		packedItem1.API_RX_NKGoodsValueCurrency = CurrencyCodes.UnitedKingdom;

		AssertNoMessageErrors(packedItem1.API_GoodsValueInfo);

		packedItem1.API_GoodsValue = 40;
		packedItem1.Validation.ValidateAll();

		AssertHasMessageError(packedItem1.API_GoodsValueInfo, "Sum of Intrinsic Value (Items) must not exceed GBP 39 when Add. Procedure(s) is 4000C08.");

		bill.AdditionalProcedureCodes.AddNew("4000C07");
		packedItem1.Validation.ValidateAll();
		AssertNoMessageErrors(packedItem1.API_GoodsValueInfo);

		packedItem1.API_GoodsValue = 140;
		packedItem1.Validation.ValidateAll();
		AssertHasMessageError(packedItem1.API_GoodsValueInfo, "Sum of Intrinsic Value (Items) must not exceed GBP 135 when Add. Procedure(s) contains 4000C07.");
	}
}

using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.H7.Business.Test
{
	sealed class AsycudaPackedItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAPI_CustomsQty2()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			packedItem.API_CustomsQty2 = -123;
			AssertHasError(packedItem.API_CustomsQty2Info,
				"Please enter a 'Supplementary Units' greater than or equal to 0.");

			packedItem.API_CustomsQty2 = 123;
			AssertNoNotifications(packedItem.API_CustomsQty2Info);

			var errorMessage =
				"The maximum permitted total digits are 16. Out of them, a max of six can be decimal digits.";
			packedItem.API_CustomsQty2 = 1234567890123.123456M;
			AssertHasMessageError(packedItem.API_CustomsQty2Info, errorMessage);
		}

		public void TestkAPI_CustomsUQ2()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ,
				"Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			Factory.Save();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			packedItem.API_CustomsUQ2 = "123";
			AssertHasMessageError(packedItem.API_CustomsUQ2Info, "The code you have selected is not in the list.");

			packedItem.API_CustomsUQ2 = "KG";
			AssertHasMessageError(packedItem.API_CustomsUQ2Info, "The code you have selected is not in the list.");

			packedItem.API_CustomsUQ2 = "ABC";
			AssertNoMessageError(packedItem.API_CustomsUQ2Info, "The code you have selected is not in the list.");
		}

		public void TestkAPI_CustomsUQ()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			packedItem.API_CustomsQty = 12;
			AssertNoErrors(packedItem.API_CustomsUQInfo);
		}

		public void TestCheckAPI_FormattedTariffInfo()
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

			packedItem.API_Tariff = "12345";
			AssertHasMessageError("Should error for non-even number of digits", packedItem.API_FormattedTariffInfo, "The code you have selected is not in the list.");

			packedItem.API_Tariff = "123456";
			AssertNoMessageErrors("Should not error for prefix of tariff", packedItem.API_FormattedTariffInfo);

			packedItem.API_Tariff = tariffView.ZZ1_TariffCode;
			AssertNoMessageErrors(packedItem.API_FormattedTariffInfo);
		}

		public void TestCheckAPI_GoodsDescription()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			packedItem.API_GoodsDescription = string.Empty;
			AssertHasMessageError(packedItem.API_GoodsDescriptionInfo, "You have not entered a Goods Description.");

			packedItem.API_GoodsDescription = "NormalDescription";
			AssertNoMessageErrors(packedItem.API_GoodsDescriptionInfo);
		}

		public void TestValidateGoodsValueCurrencyCore()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			packedItem.API_RX_NKGoodsValueCurrency = "SSS";
			packedItem.Validation.ValidateAll();
			AssertHasError(packedItem.API_RX_NKGoodsValueCurrencyInfo, "Enter a valid Intrinsic Value Currency.");

			packedItem.API_RX_NKGoodsValueCurrency = "BTY";
			packedItem.Validation.ValidateAll();
			AssertNoNotifications(packedItem.API_CustomsQty2Info);

			packedItem.API_RX_NKGoodsValueCurrency = "";
			packedItem.Validation.ValidateAll();
			AssertHasMessageError(packedItem.API_RX_NKGoodsValueCurrencyInfo, "You have not entered an Intrinsic Value Currency.");
		}

		public void TestAPI_GoodsValue()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			packedItem.API_GoodsValue = 0;
			packedItem.Validation.ValidateAll();
			AssertHasMessageError(packedItem.API_GoodsValueInfo, "You have not entered an Intrinsic Value.");
		}

		public void TestCheckAPI_GoodsValue()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_Procedure = EUH7AdditionalProcedureCodeList.Codes.C08;
			var packedItem1 = bill.PackedItems.AddNew();
			packedItem1.API_RX_NKGoodsValueCurrency = CurrencyCodes.EuropeanUnion;
			packedItem1.API_GoodsValue = 40;

			AssertNoMessageErrors(packedItem1.API_GoodsValueInfo);

			var packedItem2 = bill.PackedItems.AddNew();
			packedItem2.API_RX_NKGoodsValueCurrency = CurrencyCodes.EuropeanUnion;
			packedItem2.API_GoodsValue = 20;

			AssertHasMessageError(packedItem2.API_GoodsValueInfo, "Sum of Intrinsic Value (Items) must not exceed EUR 45 when Add. Procedure(s) is C08.");

			bill.ABL_Procedure = EUH7AdditionalProcedureCodeList.Codes.C07;
			var packedItem3 = bill.PackedItems.AddNew();
			packedItem3.API_RX_NKGoodsValueCurrency = CurrencyCodes.EuropeanUnion;
			packedItem3.API_GoodsValue = 100;

			AssertHasMessageError(packedItem3.API_GoodsValueInfo, "Sum of Intrinsic Value (Items) must not exceed EUR 150 when Add. Procedure(s) contains C07.");
		}
	}
}

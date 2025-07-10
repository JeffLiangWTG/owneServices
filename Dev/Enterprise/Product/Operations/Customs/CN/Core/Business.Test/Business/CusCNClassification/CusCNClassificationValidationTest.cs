using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusCNClassificationValidationTest : TestCaseWithFactory
	{
		[TestDate(2016, 5, 5)]
		public void TestCheckCNC_CIQTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var ciqTariffType = helper.CreateNewOrGetExistingTariffType("CN", "CIQ");
			var cusTariffType = helper.CreateNewOrGetExistingTariffType("CN", "HSN");
			Factory.Save();
			var ciqTariff1 = helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, "10000012001", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			var ciqTariff2 = helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, "10000012002", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			var ciqTariff3 = helper.LoadOrCreateNewTariff("CN", ciqTariffType.PK, "10000013001", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.LoadOrCreateNewTariff("CN", cusTariffType.PK, "10000012", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.LoadOrCreateNewTariff("CN", cusTariffType.PK, "10000013", new ZDateTime(2016, 5, 4), new ZDateTime(2016, 12, 1));
			helper.CreateTariffRelationship(ciqTariff1.PK, cusTariffType.PK, "10000012");
			helper.CreateTariffRelationship(ciqTariff2.PK, cusTariffType.PK, "10000012");
			helper.CreateTariffRelationship(ciqTariff3.PK, cusTariffType.PK, "10000013");
			Factory.Save();
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "10000012";
			pivot.Details.Validation.ValidateCNC_CIQTariff();
			var targetInfo = pivot.CNC_CIQTariffInfo;
			pivot.CNC_CIQTariff = "1111";
			AssertHasMessageErrorContaining(targetInfo, "not in the list");
			AssertNoMessageErrorContaining(targetInfo, "is not valid for the Customs Tariff Code");
			pivot.CNC_CIQTariff = "10000012001";
			AssertNoMessageErrors(targetInfo);
			pivot.CNC_CIQTariff = "10000013001";
			AssertHasMessageErrorContaining(targetInfo, "is not valid for the Customs Tariff Code");
			AssertNoMessageErrorContaining(targetInfo, "not in the list");
		}

		public void TestCheckCNC_EndUse()
		{
			var testItem = Factory.New<CusCNClassification>();
			var targetInfo = testItem.CNC_EndUseInfo;
			testItem.CNC_EndUse = "~";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			testItem.CNC_EndUse = "99";
			AssertNoMessageErrors(targetInfo);
		}

		[TestDate(2016, 5, 5)]
		public void TestDestrictsAndRegins()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeTypeDistrict = helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.DistrictCode, "District Codes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, codeTypeDistrict.ZZK_CodeType, "12345", "New District Code", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, codeTypeDistrict.ZZK_CodeType, "12346", "New District Code", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			var codeTypeRegion = helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CNCIQDistricts, "Region Codes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, codeTypeRegion.ZZK_CodeType, "12347", "New Region Code", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, codeTypeRegion.ZZK_CodeType, "12348", "New Region Code", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			var ciqState = helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CNCIQStates, "China Inspection and Quarantine States");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, ciqState.ZZK_CodeType, "12349", "New Region Code", new ZDateTime(2016, 1, 1), new ZDateTime(2016, 12, 1));
			Factory.Save();
			var pivot = Factory.New<CusClassPartPivot>();
			var classification = Factory.New<CusClassification>();
			pivot.CI_CC = classification.PK;
			pivot.CNC_OriginDistrict = "11111";
			pivot.CNC_OriginRegion = "11112";
			pivot.CNC_DestinationDistrict = "11113";
			pivot.CNC_DestinationRegion = "11114";
			pivot.CNC_OriginState = "11115";
			AssertHasMessageErrorContaining(pivot.CNC_OriginDistrictInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(pivot.CNC_OriginStateInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(pivot.CNC_OriginRegionInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(pivot.CNC_DestinationDistrictInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(pivot.CNC_DestinationRegionInfo, ListValidation.InvalidCodeMessageError);
			pivot.CNC_OriginDistrict = "12345";
			pivot.CNC_DestinationDistrict = "12346";
			pivot.CNC_OriginRegion = "12347";
			pivot.CNC_DestinationRegion = "12348";
			pivot.CNC_OriginState = "12349";
			AssertNoMessageErrorContaining(pivot.CNC_OriginDistrictInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(pivot.CNC_OriginStateInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(pivot.CNC_OriginRegionInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(pivot.CNC_DestinationDistrictInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(pivot.CNC_DestinationRegionInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCNC_TradeUnitQty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSUQ", "Customs Unit Quantity");
			helper.CreateNewOrGetExistingCusCodeList("CN", "CUSUQ", "047", "Quintals", ZDateTime.Today, ZDateTime.Today.AddYears(1));
			Factory.Save();
			var testItem = Factory.New<CusCNClassification>();
			var targetInfo = testItem.CNC_TradeUnitQtyInfo;
			testItem.CNC_TradeUnitQty = "XXX";
			AssertHasMessageErrorContaining(targetInfo, ListValidation.InvalidCodeMessageError);
			var pleaseEnterTradeUQ = "Please enter Trade Quantity Unit.\r\nThe system will not default the value to the invoice line if the Trade Quantity Unit is not specified.";
			testItem.CNC_TradeUnitQty = "047";
			AssertNoErrors(targetInfo);
			testItem.CNC_TradeUnitPrice = 1.23;
			testItem.CNC_TradeUnitQty = ZString.Empty;
			AssertHasMessageError(targetInfo, pleaseEnterTradeUQ);
			testItem.CNC_TradeUnitQty = "047";
			AssertNoMessageError(targetInfo, pleaseEnterTradeUQ);
		}

		public void TestCNC_TradeUnitPrice()
		{
			var testItem = Factory.New<CusCNClassification>();
			var targetInfo = testItem.CNC_TradeUnitPriceInfo;
			testItem.CNC_TradeUnitPrice = -1;
			AssertHasErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			testItem.CNC_TradeUnitPrice = 1;
			AssertNoErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckCNC_GoodsSpecModel()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200010", "00000", "00422", "99999");
			helper.CreateAdditionalElement("00000", "品名");
			helper.CreateAdditionalElement("00422", "品牌类型");
			helper.CreateAdditionalElement("99999", "其他");
			Factory.Save();
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var classification = pivot.Details;
			pivot.CI_TariffNum = "2713200010";
			classification.Validation.ValidateCNC_GoodsSpecModel();
			AssertNoMessageErrorContaining(pivot.CNC_GoodsSpecModelInfo, "Some Additional Information you entered is incorrect.");
			pivot.AdditionalInformationCodes.AddNew("00000", "111");
			pivot.AdditionalInformationCodes.AddNew("00422", "X");
			pivot.CNC_GoodsSpecModel = "X";
			classification.Validation.ValidateCNC_GoodsSpecModel();
			AssertHasMessageErrorContaining(pivot.CNC_GoodsSpecModelInfo, "Some Additional Information you entered is incorrect.");
			AssertNoWarningContaining(pivot.CNC_GoodsSpecModelInfo, "Some Additional Information is not available for the selected Tariff");
			pivot.AdditionalInformationCodes.AddNew("00352", "333");
			classification.Validation.ValidateCNC_GoodsSpecModel();
			AssertHasWarningContaining(pivot.CNC_GoodsSpecModelInfo, "Some Additional Information is not available for the selected Tariff");
		}

		public void TestCheckCNC_RX_NKTradeUnitPriceCurrency()
		{
			var testItem = Factory.New<CusCNClassification>();
			var targetInfo = testItem.CNC_RX_NKTradeUnitPriceCurrencyInfo;
			var pleaseEnterCurrency = "You should enter a currency when you have entered Trade Unit Price.";
			testItem.Validation.ValidateCNC_RX_NKTradeUnitPriceCurrency();
			AssertNoError("CNC_TradeUnitPrice 0, should not check CNC_RX_NKTradeUnitPriceCurrency.", targetInfo, pleaseEnterCurrency);
			testItem.CNC_TradeUnitPrice = 1.23;
			testItem.Validation.ValidateCNC_RX_NKTradeUnitPriceCurrency();
			AssertHasError("CNC_TradeUnitPrice not 0, should check CNC_RX_NKTradeUnitPriceCurrency.", targetInfo, pleaseEnterCurrency);
			testItem.CNC_RX_NKTradeUnitPriceCurrency = "CNY";
			AssertNoError("CNC_RX_NKTradeUnitPriceCurrency input, should not contain message error.", targetInfo, pleaseEnterCurrency);
		}
	}
}

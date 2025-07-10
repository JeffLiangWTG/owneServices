using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class GuidedDecisionMakingBasicValidationTest : BusinessObjectValidationTestCase
	{
		public void TestExportConfiguration()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			var validation = new GuidedDecisionMakingBasicValidationForTest(guidedDecisionMakingBasic);
			AssertNull(validation.exportConfiguration);

			guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, true);
			validation = new GuidedDecisionMakingBasicValidationForTest(guidedDecisionMakingBasic);
			AssertType<ExportGuidedDecisionMakingBasicValidationConfiguration>(validation.exportConfiguration);

			guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, true);
			validation = new GuidedDecisionMakingBasicValidationForTest(guidedDecisionMakingBasic);
			AssertType<ExportGuidedDecisionMakingBasicValidationConfiguration>(validation.exportConfiguration);
		}

		public void TestImportConfiguration()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			var validation = new GuidedDecisionMakingBasicValidationForTest(guidedDecisionMakingBasic);
			AssertNull(validation.importConfiguration);

			guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, false);
			validation = new GuidedDecisionMakingBasicValidationForTest(guidedDecisionMakingBasic);
			AssertType<ImportGuidedDecisionMakingBasicValidationConfiguration>(validation.importConfiguration);

			guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, true);
			validation = new GuidedDecisionMakingBasicValidationForTest(guidedDecisionMakingBasic);
			AssertType<ImportGuidedDecisionMakingBasicValidationConfiguration>(validation.importConfiguration);
		}

		public void TestCheckEffectiveDate()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			guidedDecisionMakingBasic.EffectiveDate = ZDate.Empty;
			AssertHasErrorContaining("Effective Date should not be empty => red error", guidedDecisionMakingBasic.EffectiveDateInfo, MandatoryValidation.MustBeEntered);

			guidedDecisionMakingBasic.EffectiveDate = ZDate.Invalid;
			AssertHasErrorContaining("Effective Date should not be invalid => red error", guidedDecisionMakingBasic.EffectiveDateInfo, "Enter a valid Effective Date.");

			guidedDecisionMakingBasic.EffectiveDate = ZDate.BrettsBirthday;
			AssertNoErrorContaining("Effective Date is good => no error", guidedDecisionMakingBasic.EffectiveDateInfo, "Enter a valid Effective Date.");
		}

		public void TestCheckTariffCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;

			var tradeGroupStandard = helper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date4);
			helper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Botswana, date1, date4);
			Factory.Save();

			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, "EXP");
			Factory.Save();

			var cusTariff = helper.CreateTariff(dataGrouping, hsnTariffType.PK, "1100110000", date1, date4, "dummy Description 0");
			Factory.Save();

			helper.CreateTariffUOM(cusTariff, UOMTypeList.Codes.CU2, "T", dataGrouping);
			Factory.Save();

			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			guidedDecisionMakingBasic.DataGrouping = ZString.Empty;
			guidedDecisionMakingBasic.TariffType = ZString.Empty;
			guidedDecisionMakingBasic.EffectiveDate = ZDate.Invalid;

			guidedDecisionMakingBasic.TariffCode = ZString.Empty;
			AssertHasErrorContaining("Tariff Code should not be empty => red error", guidedDecisionMakingBasic.TariffCodeInfo, MandatoryValidation.MustBeEntered);

			guidedDecisionMakingBasic.TariffCode = "10000000";
			AssertHasErrorContaining("TariffCode entered is not valid => red error", guidedDecisionMakingBasic.TariffCodeInfo, ListValidation.InvalidCodeError);

			guidedDecisionMakingBasic.TariffCode = "1100110000";
			AssertNoErrorContaining("TariffCode is good => no error", guidedDecisionMakingBasic.TariffCodeInfo, ListValidation.InvalidCodeError);

			guidedDecisionMakingBasic.FormattedTariff = "1000.00.00";
			AssertHasErrorContaining("TariffCode entered is not valid => red error", guidedDecisionMakingBasic.TariffCodeInfo, ListValidation.InvalidCodeError);

			guidedDecisionMakingBasic.FormattedTariff = "1100.11.00 00";
			AssertNoErrorContaining("TariffCode is good => no error", guidedDecisionMakingBasic.TariffCodeInfo, ListValidation.InvalidCodeError);

			guidedDecisionMakingBasic.TariffType = "DTY";
			AssertHasErrorContaining("TariffCode entered is not valid for DTY type", guidedDecisionMakingBasic.TariffCodeInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckCountryOfOrigin()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, false);
			guidedDecisionMakingBasic.DataGrouping = "FR";

			guidedDecisionMakingBasic.CountryOfOrigin = ZString.Empty;
			AssertHasErrorContaining("CountryOfOrigin should not be empty => red error", guidedDecisionMakingBasic.CountryOfOriginInfo, MandatoryValidation.MustBeEntered);

			guidedDecisionMakingBasic.CountryOfOrigin = "KO";
			AssertHasErrorContaining("CountryOfOrigin entered is not valid => red error", guidedDecisionMakingBasic.CountryOfOriginInfo, ListValidation.InvalidCodeError);

			guidedDecisionMakingBasic.CountryOfOrigin = "US";
			AssertNoErrorContaining("CountryOfOrigin is good => no error", guidedDecisionMakingBasic.CountryOfOriginInfo, ListValidation.InvalidCodeError);

			guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, true);
			guidedDecisionMakingBasic.DataGrouping = "FR";

			guidedDecisionMakingBasic.CountryOfOrigin = ZString.Empty;
			AssertNoErrorContaining("CountryOfOrigin is not mandatory for export => no error", guidedDecisionMakingBasic.CountryOfOriginInfo, MandatoryValidation.MustBeEntered);

			guidedDecisionMakingBasic.CountryOfOrigin = "KO";
			AssertHasErrorContaining("CountryOfOrigin entered is not valid => red error", guidedDecisionMakingBasic.CountryOfOriginInfo, ListValidation.InvalidCodeError);

			guidedDecisionMakingBasic.CountryOfOrigin = "US";
			AssertNoErrorContaining("CountryOfOrigin is good => no error", guidedDecisionMakingBasic.CountryOfOriginInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckCountryOfDestination()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, true);
			guidedDecisionMakingBasic.DataGrouping = "FR";

			guidedDecisionMakingBasic.CountryOfDestination = ZString.Empty;
			AssertHasErrorContaining("CountryOfDestination should not be empty => red error", guidedDecisionMakingBasic.CountryOfDestinationInfo, MandatoryValidation.MustBeEntered);

			guidedDecisionMakingBasic.CountryOfDestination = "KO";
			AssertHasErrorContaining("CountryOfDestination entered is not valid => red error", guidedDecisionMakingBasic.CountryOfDestinationInfo, ListValidation.InvalidCodeError);

			guidedDecisionMakingBasic.CountryOfDestination = "US";
			AssertNoErrorContaining("CountryOfDestination is good => no error", guidedDecisionMakingBasic.CountryOfDestinationInfo, ListValidation.InvalidCodeError);

			guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, false);
			guidedDecisionMakingBasic.DataGrouping = "FR";

			guidedDecisionMakingBasic.CountryOfDestination = ZString.Empty;
			AssertNoErrorContaining("CountryOfDestination is not mandatory for import => no error", guidedDecisionMakingBasic.CountryOfDestinationInfo, MandatoryValidation.MustBeEntered);

			guidedDecisionMakingBasic.CountryOfDestination = "KO";
			AssertHasErrorContaining("CountryOfDestination entered is not valid => red error", guidedDecisionMakingBasic.CountryOfDestinationInfo, ListValidation.InvalidCodeError);

			guidedDecisionMakingBasic.CountryOfDestination = "US";
			AssertNoErrorContaining("CountryOfDestination is good => no error", guidedDecisionMakingBasic.CountryOfDestinationInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckPreference_WhenImport()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, false);
			guidedDecisionMakingBasic.EffectiveDate = ZDate.Today;
			guidedDecisionMakingBasic.DataGrouping = "FR";
			guidedDecisionMakingBasic.CountryOfOrigin = "US";
			guidedDecisionMakingBasic.TariffCode = "1111111111";

			var list = guidedDecisionMakingBasic.Lookups.PreferenceList;

			AssertEquals("Empty DataGrouping/CountryOfOrigin/TariffCode/DutyRateTypeCode", 0, list.Count);
			guidedDecisionMakingBasic.Preference = ZString.Empty;
			AssertNoErrorContaining("List is empty and prefence is empty => no validation required for preference", guidedDecisionMakingBasic.PreferenceInfo, MandatoryValidation.MustBeEntered);

			guidedDecisionMakingBasic.Preference = "KO";
			AssertNoErrorContaining("List is empty and the value entered in preference is invalid => no validation required for preference", guidedDecisionMakingBasic.PreferenceInfo, ListValidation.InvalidCodeError);

			guidedDecisionMakingBasic.DutyRateTypeCode = "RT1";
			list = guidedDecisionMakingBasic.Lookups.PreferenceList;
			Assert("DataGrouping/CountryOfOrigin/TariffCode/DutyRateTypeCode list have value", list.Count > 0);

			guidedDecisionMakingBasic.Preference = ZString.Empty;
			AssertHasErrorContaining("List is not empty and prefence is empty => red error", guidedDecisionMakingBasic.PreferenceInfo, MandatoryValidation.MustBeEntered);

			guidedDecisionMakingBasic.Preference = "KO";
			AssertHasErrorContaining("List is not empty and the value entered in preference is invalid => red error", guidedDecisionMakingBasic.PreferenceInfo, ListValidation.InvalidCodeError);

			guidedDecisionMakingBasic.Preference = list.GetAllCodes()[0];
			AssertNoErrorContaining("List is not empty but preference entered is valid => no error", guidedDecisionMakingBasic.PreferenceInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckPreference_WhenNotImport()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, true);
			guidedDecisionMakingBasic.EffectiveDate = ZDate.Today;
			guidedDecisionMakingBasic.DataGrouping = "FR";
			guidedDecisionMakingBasic.CountryOfOrigin = "US";
			guidedDecisionMakingBasic.CountryOfDestination = "US";
			guidedDecisionMakingBasic.TariffCode = "1111111111";

			guidedDecisionMakingBasic.DutyRateTypeCode = "RT1";
			var list = guidedDecisionMakingBasic.Lookups.PreferenceList;
			Assert("DataGrouping/CountryOfOrigin/TariffCode/DutyRateTypeCode list have value", list.Count > 0);

			guidedDecisionMakingBasic.Preference = ZString.Empty;
			AssertNoErrors("List is not empty and preference is empty => no red error due to export.", guidedDecisionMakingBasic.PreferenceInfo);

			guidedDecisionMakingBasic.Preference = "KO";
			AssertNoErrors("List is not empty and the value entered in preference is invalid => no red error due to export.", guidedDecisionMakingBasic.PreferenceInfo);
		}

		public void TestCheckQuotaOrderNumber_WhenImport()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, false);
			guidedDecisionMakingBasic.DataGrouping = "FR";
			guidedDecisionMakingBasic.ParentDataGrouping = "EU";
			guidedDecisionMakingBasic.TariffType = "DTY";
			guidedDecisionMakingBasic.EffectiveDate = ZDate.Today;
			guidedDecisionMakingBasic.CountryOfOrigin = "US";
			guidedDecisionMakingBasic.TariffCode = "1111111111";

			var list = guidedDecisionMakingBasic.Lookups.QuotaOrderNumberList;

			AssertEquals("Empty DataGrouping/CountryOfOrigin/TariffCode/DutyRateTypeCode", 0, list.Count);
			guidedDecisionMakingBasic.QuotaOrderNumber = ZString.Empty;
			AssertNoErrorContaining("List is empty and QuotaOrderNumber is empty => no validation required for QuotaOrderNumber", guidedDecisionMakingBasic.QuotaOrderNumberInfo, MandatoryValidation.MustBeEntered);

			guidedDecisionMakingBasic.QuotaOrderNumber = "KO";
			AssertNoErrorContaining("List is empty and the value entered in QuotaOrderNumber is invalid => no validation required for QuotaOrderNumber", guidedDecisionMakingBasic.QuotaOrderNumberInfo, ListValidation.InvalidCodeError);

			guidedDecisionMakingBasic.DutyRateTypeCode = "RT1";
			list = guidedDecisionMakingBasic.Lookups.QuotaOrderNumberList;
			Assert("DataGrouping/CountryOfOrigin/TariffCode/DutyRateTypeCode list have value", list.Count > 0);

			guidedDecisionMakingBasic.QuotaOrderNumber = ZString.Empty;
			AssertHasErrorContaining("List is not empty and QuotaOrderNumber is empty => red error", guidedDecisionMakingBasic.QuotaOrderNumberInfo, MandatoryValidation.MustBeEntered);

			guidedDecisionMakingBasic.QuotaOrderNumber = "KO";
			AssertHasErrorContaining("List is not empty and the value entered in QuotaOrderNumber is invalid => red error", guidedDecisionMakingBasic.QuotaOrderNumberInfo, ListValidation.InvalidCodeError);

			guidedDecisionMakingBasic.QuotaOrderNumber = list.GetAllCodes()[0];
			AssertNoErrorContaining("List is not empty and the value entered in QuotaOrderNumber valid => no error", guidedDecisionMakingBasic.QuotaOrderNumberInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckQuotaOrderNumber_WhenNotImport()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, true);
			guidedDecisionMakingBasic.DataGrouping = "FR";
			guidedDecisionMakingBasic.ParentDataGrouping = "EU";
			guidedDecisionMakingBasic.TariffType = "DTY";
			guidedDecisionMakingBasic.EffectiveDate = ZDate.Today;
			guidedDecisionMakingBasic.CountryOfOrigin = "US";
			guidedDecisionMakingBasic.CountryOfDestination = "US";
			guidedDecisionMakingBasic.TariffCode = "1111111111";

			guidedDecisionMakingBasic.DutyRateTypeCode = "RT1";
			var list = guidedDecisionMakingBasic.Lookups.QuotaOrderNumberList;
			Assert("DataGrouping/CountryOfOrigin/TariffCode/DutyRateTypeCode list have value", list.Count > 0);

			guidedDecisionMakingBasic.QuotaOrderNumber = ZString.Empty;
			AssertNoErrors("List is not empty and QuotaOrderNumber is empty => no red error due to export.", guidedDecisionMakingBasic.QuotaOrderNumberInfo);

			guidedDecisionMakingBasic.QuotaOrderNumber = "KO";
			AssertNoErrors("List is not empty and the value entered in QuotaOrderNumber is invalid => no red error due to export.", guidedDecisionMakingBasic.QuotaOrderNumberInfo);
		}

		public void TestCheckCustomsFirstQuantity()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			guidedDecisionMakingBasic.CustomsFirstQuantity = 0;
			AssertHasWarningContaining("CustomsFirstQuantity can't be equal to zero => warning", guidedDecisionMakingBasic.CustomsFirstQuantityInfo, "Net Weight must be greater than zero.");

			guidedDecisionMakingBasic.CustomsFirstQuantity = -2;
			AssertHasWarningContaining("CustomsFirstQuantity can't be negative => warning", guidedDecisionMakingBasic.CustomsFirstQuantityInfo, "Net Weight must be greater than zero.");

			guidedDecisionMakingBasic.CustomsFirstQuantity = 4;
			AssertNoWarningContaining("CustomsFirstQuantity is good => no warning", guidedDecisionMakingBasic.CustomsFirstQuantityInfo, "Net Weight must be greater than zero.");
		}

		public void TestCheckCustomsSecondQuantity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var dataGrouping = Core.Constants.CountryCodes.Botswana;

			var tradeGroupStandard = helper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date4);
			helper.AddCountry(tradeGroupStandard, dataGrouping, date1, date4);
			Factory.Save();

			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, "EXP");
			Factory.Save();

			var cusTariff = helper.CreateTariff(dataGrouping, hsnTariffType.PK, "11001100", date1, date4, "dummy Description 0");
			helper.CreateTariff(dataGrouping, hsnTariffType.PK, "12121212", date1, date4, "dummy Description 0");
			Factory.Save();

			helper.CreateTariffUOM(cusTariff, UOMTypeList.Codes.CU2, "T", dataGrouping, tradeGroupStandard);
			Factory.Save();

			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			guidedDecisionMakingBasic.DataGrouping = ZString.Empty;
			guidedDecisionMakingBasic.TariffType = ZString.Empty;
			guidedDecisionMakingBasic.EffectiveDate = ZDate.Invalid;

			guidedDecisionMakingBasic.TariffCode = "12121212";

			guidedDecisionMakingBasic.CustomsSecondQuantity = 0;
			AssertNoWarnings("tariffcode doesn't need a CU2 and no validation: 0", guidedDecisionMakingBasic.CustomsSecondQuantityInfo);

			guidedDecisionMakingBasic.CustomsSecondQuantity = -2;
			AssertNoWarnings("tariffcode doesn't need a CU2 and no validation: -2", guidedDecisionMakingBasic.CustomsSecondQuantityInfo);

			guidedDecisionMakingBasic.CustomsSecondQuantity = 4;
			AssertNoWarnings("tariffcode doesn't need a CU2 and no validation: 4", guidedDecisionMakingBasic.CustomsSecondQuantityInfo);

			guidedDecisionMakingBasic.DataGrouping = "BW";
			guidedDecisionMakingBasic.TariffCode = "11001100";
			guidedDecisionMakingBasic.CustomsSecondUnitQty = "";

			var zeroWarning = "Supplementary Quantity must be greater than zero.";
			var invalidUOMWarning = "Supplementary Quantity must have a valid unit. Please select valid Country Of Origin to get valid unit.";
			guidedDecisionMakingBasic.CustomsSecondQuantity = 0;
			AssertHasWarningContaining("tariffcode need a CU2 => CustomsSecondQuantity can't be equal to zero", guidedDecisionMakingBasic.CustomsSecondQuantityInfo, zeroWarning);

			guidedDecisionMakingBasic.CustomsSecondQuantity = -2;
			AssertHasWarningContaining("tariffcode need a CU2 => CustomsSecondQuantity can't be negative", guidedDecisionMakingBasic.CustomsSecondQuantityInfo, zeroWarning);

			guidedDecisionMakingBasic.CustomsSecondQuantity = 4;
			AssertNoWarningContaining("tariffcode need a CU2 => CustomsSecondQuantity is good => no warning", guidedDecisionMakingBasic.CustomsSecondQuantityInfo, zeroWarning);

			guidedDecisionMakingBasic.CountryOfOrigin = "XX";
			AssertEquals("CustomsSecondUnitQty Cleared", ZString.Empty, guidedDecisionMakingBasic.CustomsSecondUnitQty);
			AssertEquals("CustomsSecondQuantity Cleared", 0m, guidedDecisionMakingBasic.CustomsSecondQuantity);
			guidedDecisionMakingBasic.CustomsSecondQuantity = 5;
			AssertHasWarningContaining("tariffcode need a CU2 => no valid UOM and set CustomsSecondUnitQty to empty when invalid CountryOfOrigin", guidedDecisionMakingBasic.CustomsSecondQuantityInfo, invalidUOMWarning);

			guidedDecisionMakingBasic.CountryOfOrigin = dataGrouping;
			AssertNoWarningContaining("tariffcode need a CU2 => all good", guidedDecisionMakingBasic.CustomsSecondQuantityInfo, invalidUOMWarning);
		}

		public void TestCheckCustomsThirdQuantity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var dataGrouping = Core.Constants.CountryCodes.Botswana;

			var tradeGroupStandard = helper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date4);
			helper.AddCountry(tradeGroupStandard, dataGrouping, date1, date4);
			Factory.Save();

			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, "EXP");
			Factory.Save();

			var cusTariff = helper.CreateTariff(dataGrouping, hsnTariffType.PK, "11001100", date1, date4, "dummy Description 0");
			helper.CreateTariff(dataGrouping, hsnTariffType.PK, "12121212", date1, date4, "dummy Description 0");
			Factory.Save();

			helper.CreateTariffUOM(cusTariff, UOMTypeList.Codes.CU3, "T", dataGrouping, tradeGroupStandard);
			Factory.Save();

			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);

			guidedDecisionMakingBasic.DataGrouping = ZString.Empty;
			guidedDecisionMakingBasic.TariffType = ZString.Empty;
			guidedDecisionMakingBasic.EffectiveDate = ZDate.Invalid;

			guidedDecisionMakingBasic.TariffCode = "12121212";

			guidedDecisionMakingBasic.CustomsThirdQuantity = -2;
			AssertNoWarnings("tariffcode doesn't need a CU3 and no validation: -2", guidedDecisionMakingBasic.CustomsSecondQuantityInfo);

			var lessThanZeroWarning = "Third Quantity can not be less than zero.";
			var emptyUnitWarning = "The unit must be given when Third Quantity is greater than zero.";
			var emptyQuantityWarning = "Third Quantity must be greater than zero when The unit is given.";
			guidedDecisionMakingBasic.DataGrouping = "BW";
			guidedDecisionMakingBasic.TariffCode = "11001100";
			guidedDecisionMakingBasic.CustomsSecondUnitQty = "";
			guidedDecisionMakingBasic.CustomsThirdQuantity = -2;
			AssertHasWarningContaining("CustomsThirdQuantity can't be negative => warning", guidedDecisionMakingBasic.CustomsThirdQuantityInfo, lessThanZeroWarning);

			guidedDecisionMakingBasic.CustomsThirdQuantity = 4;
			AssertNoWarningContaining("CustomsThirdQuantity is good => no warning", guidedDecisionMakingBasic.CustomsThirdQuantityInfo, lessThanZeroWarning);

			guidedDecisionMakingBasic.CustomsThirdUnitQty = "";
			AssertHasWarningContaining("Unit can not be empty When CustomsThirdQuantity has value", guidedDecisionMakingBasic.CustomsThirdQuantityInfo, emptyUnitWarning);

			guidedDecisionMakingBasic.CustomsThirdUnitQty = "LPA";
			AssertNoWarningContaining("Unit can not be empty When CustomsThirdQuantity has value", guidedDecisionMakingBasic.CustomsThirdQuantityInfo, emptyUnitWarning);

			guidedDecisionMakingBasic.CustomsThirdUnitQty = "";
			guidedDecisionMakingBasic.CustomsThirdQuantity = 0;
			AssertNoWarningContaining("Unit can not be empty When CustomsThirdQuantity has value", guidedDecisionMakingBasic.CustomsThirdQuantityInfo, emptyUnitWarning);

			guidedDecisionMakingBasic.CustomsThirdUnitQty = "LPA";
			AssertHasWarningContaining("CustomsThirdQuantity can not be 0 When unit has value", guidedDecisionMakingBasic.CustomsThirdQuantityInfo, emptyQuantityWarning);

			guidedDecisionMakingBasic.CustomsThirdQuantity = 2;
			AssertNoWarningContaining("Unit and CustomsThirdQuantity has value", guidedDecisionMakingBasic.CustomsThirdQuantityInfo, emptyUnitWarning);
		}

		public void TestCheckMeursingResult()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			guidedDecisionMakingBasic.MeursingResult = ZString.Empty;
			AssertHasWarning("Meursing Result can't be empty.", guidedDecisionMakingBasic.MeursingResultInfo, "You have not entered a Meursing Result.");

			guidedDecisionMakingBasic.MeursingResult = "INVC";
			AssertHasWarning("Meursing Result should be valid", guidedDecisionMakingBasic.MeursingResultInfo, "You have not entered a valid code.");

			guidedDecisionMakingBasic.MeursingResult = MeursingList.Codes.MeursingStarchGlucose05PercentSucrose05PercentMilkFat015PercentMilkProtein025Percent;
			AssertNoErrors("Meursing Result is valid with a value in the Meursing Result List", guidedDecisionMakingBasic.MeursingResultInfo);
		}

		public void TestCheckTariffCode_CheckClassConditions()
		{
			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsSecondQuantity = 2;
			invoiceLine.JI_CustomsSecondUnitQty = "NAR";

			var de = Core.Constants.CountryCodes.Germany;

			var anotherFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(anotherFactory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(de, "TTT");
			var classType1 = helper.CreateOrGetExistingRefCusConditionType(de, "CLASS", "TSTC1", "Test Class Condition Type 1");

			anotherFactory.Save();
			var tariff = helper.CreateTariff(de, tariffType.PK, "Tariff1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var formulaValueType = helper.CreateOrGetExistingRefCusConditionValueType(de, "FRMVT", afterCreate: t => t.ZX4_IsFormula = true);

			var testCondCtrl1 = helper.CreateOrGetExistingRefCusCondition(de, classType1.PK, tariff.PK, "Comment", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(formulaValueType.PK, testCondCtrl1.PK, "[KGM]/[NAR] < 165.001 & [KGM]/[NAR] >= 60.000");

			anotherFactory.Save();
			tariff = Factory.Load<TariffView>(tariff.PK);
			var (notMetMessageForNonInformation, notMetMessageForInformation) = tariff.CheckConditionsAreMetForConditionClass(Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Class, invoiceLine.EvaluateConditionValue, null, invoiceLine.CalcDataForConditionFormula);
			AssertMultilineASCIIEquals("notMetMessageForNonInformation should not be empty", @"The Classification condition is not satisfied
    Test Class Condition Type 1:
        Comment: ([KGM]/[NAR] < 165.001 & [KGM]/[NAR] >= 60.000)
", notMetMessageForNonInformation);

			invoiceLine.JI_CustomsQuantity = 180m;
			(notMetMessageForNonInformation, notMetMessageForInformation) = tariff.CheckConditionsAreMetForConditionClass(Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Class, invoiceLine.EvaluateConditionValue, null, invoiceLine.CalcDataForConditionFormula);
			AssertMultilineASCIIEquals("notMetMessageForNonInformation should be empty", "", notMetMessageForNonInformation);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentDataGroup = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping("FR", "France", parentDataGroup);
			helper.CreateNewOrGetExistingDataGrouping("DE", "Germany", parentDataGroup);
			helper.CreateNewOrGetExistingDataGrouping("AU", "AU");
			helper.CreateNewOrGetExistingDataGrouping("US", "US");

			var dtyType = helper.CreateTariffType("EUN", "DTY");
			var addType = helper.CreateTariffType("FR", "ADD");
			Factory.Save();

			var startDate = ZDate.Today.AddDays(-2);
			var endDate = ZDate.Today.AddDays(2);
			var tariff1 = helper.CreateTariff("FR", dtyType.PK, "1111111111", startDate, endDate);
			var tariff2 = helper.CreateTariff("EUN", dtyType.PK, "2222222222", startDate, endDate);
			var tariff3 = helper.CreateTariff("FR", dtyType.PK, "3333333333", startDate, endDate);
			var tariff4 = helper.CreateTariff("FR", addType.PK, "4444444444", startDate, endDate);

			var tradeGroup1 = helper.CreateTradeGroup("EUN", "TG1", startDate, endDate);
			var tradeGroup2 = helper.CreateTradeGroup("FR", "TG2", startDate, endDate);
			var tradeGroup3 = helper.CreateTradeGroup("AU", "TG3", startDate, endDate);
			var tradeGroup4 = helper.CreateTradeGroup("FR", "TG4", startDate, endDate);
			helper.AddCountry(tradeGroup1, "US", startDate, endDate, "America");
			helper.AddCountry(tradeGroup2, "DE", startDate, endDate, "German");
			helper.AddCountry(tradeGroup2, "AU", startDate, endDate, "Australia");
			helper.AddCountry(tradeGroup3, "NZ", startDate, endDate, "New Zealand");
			helper.AddCountry(tradeGroup4, "US", startDate, endDate, "America");

			var perference1 = helper.CreatePreferenceView("Pre1", "Preference 1", "EUN");
			var perference2 = helper.CreatePreferenceView("Pre2", "Preference 2", "FR");
			var perference3 = helper.CreatePreferenceView("Pre3", "Preference 3", "AU");
			var perference4 = helper.CreatePreferenceView("Pre4", "Preference 4", "FR");
			var rateType1 = helper.CreateCusRateType("EUN", "RT1");
			var rateType2 = helper.CreateCusRateType("FR", "RT2");
			var rateCode1 = helper.CreateCusRateCode(Factory, "RC1", rateType1.PK);
			var rateCode2 = helper.CreateCusRateCode(Factory, "RC2", rateType1.PK);
			var rateCode3 = helper.CreateCusRateCode(Factory, "RC3", rateType1.PK);
			var rateCode4 = helper.CreateCusRateCode(Factory, "RC4", rateType1.PK);
			var rate1 = helper.CreateRate(tariff1, rateCode1.PK, startDate, endDate, "0.1", perference1.PK);
			var rate2 = helper.CreateRate(tariff1, rateCode2.PK, startDate, endDate, "0.2", perference2.PK);
			var rate3 = helper.CreateRate(tariff3, rateCode3.PK, startDate, endDate, "0.3", perference3.PK);
			var rate4 = helper.CreateRate(tariff1, rateCode4.PK, startDate, endDate, "0.4", perference4.PK);
			helper.CreateCusApplicability(rate1, tradeGroup1, startDate, endDate, orderNumber: "Ord1");
			helper.CreateCusApplicability(rate2, tradeGroup1, startDate, endDate, orderNumber: "Ord2");
			helper.CreateCusApplicability(rate2, tradeGroup2, startDate, endDate, orderNumber: "Ord3");
			helper.CreateCusApplicability(rate3, tradeGroup2, startDate, endDate, orderNumber: "Ord4");
			var applicability4 = helper.CreateCusApplicability(rate4, tradeGroup4, startDate, endDate, orderNumber: "Ord5");
			helper.CreateExcludedTradeGroup(tradeGroup4, applicability4);
			Factory.Save();
		}
	}
}

using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class TemporaryStorageTestHelper : TestCaseWithFactory
	{
		public const string TestTariffCode = "0304798000";

		public static void AssertCargoDescSetting_BY_HarmonisedTariff_Synchronizes_BY_Description<T>(T packedItem)
			where T : TemporaryStoragePackedItem
		{
			CombineAssertions(() =>
			{
				packedItem.API_FormattedTariff = TestTariffCode;
				AssertEquals("When setting a valid API_FormattedTariff, API_GoodsDescription", "FISH AND CRUSTACEANS, MOLLUSCS AND OTHER AQUATIC INVERTEBRATES Fish fillets and other fish meat (whether or not minced), fresh, chilled or frozen Other", packedItem.API_GoodsDescription);

				packedItem.API_FormattedTariff = "8001100000";
				AssertEquals("When switching to another valid API_FormattedTariff, API_GoodsDescription", "TIN AND ARTICLES THEREOF Tin, not alloyed", packedItem.API_GoodsDescription);

				packedItem.API_FormattedTariff = "";
				AssertEquals("When setting an empty API_FormattedTariff, API_GoodsDescription", "", packedItem.API_GoodsDescription);

				packedItem.API_FormattedTariff = "1234567890";
				AssertEquals("When setting an invalid API_FormattedTariff, API_GoodsDescription", "", packedItem.API_GoodsDescription);
			});
		}

		public static void SetUpTariff(BusinessObjectFactory factory, string countrycode = Core.Constants.CountryCodes.Latvia, string tariffTypeCode = Universal.Constants.TariffTypes.Import)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(countrycode, "test country", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffTypeCode, nomenclatureGroupType: "CN");
			factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, TestTariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");
			helper.CreateNomenclatureGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "LIVE ANIMALS", compositeKey: "01", nomenclatureGroupType: "CN");
			helper.CreateNomenclatureGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "03", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "FISH AND CRUSTACEANS, MOLLUSCS AND OTHER AQUATIC INVERTEBRATES", compositeKey: "01.03", nomenclatureGroupType: "CN");
			helper.CreateNomenclatureGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "0304", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Fish fillets and other fish meat (whether or not minced), fresh, chilled or frozen", compositeKey: "01.03..04", nomenclatureGroupType: "CN");
			var tariff2 = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "8001100000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tin, not alloyed", compositeKey: "15.80..01.1");
			helper.CreateNomenclatureGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "80", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "TIN AND ARTICLES THEREOF", compositeKey: "15.80", nomenclatureGroupType: "CN");

			var rateType1 = helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode1 = helper.LoadOrCreateNewCusRateCode(factory, "A00", rateType1.PK);
			var preference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");
			var preference2 = helper.CreatePreferenceForCountryAndGrouping("200", "200", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");

			var rateType2 = helper.CreateNewOrGetExistingRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "SEC");
			var rateCode2 = helper.LoadOrCreateNewCusRateCode(factory, "SEC", rateType2.PK);

			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.12", preference1.PK);
			var rate2 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", preference2.PK);
			var rate3 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.08", preference1.PK);
			var rate4 = helper.CreateRefCusRate(tariff.PK, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", preference1.PK);
			var rate5 = helper.CreateRefCusRate(tariff2.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.04", preference1.PK);

			var tradeGroup = helper.LoadOrCreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");
			helper.CreateCusApplicability(rate1.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate2.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate3.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ABCD");
			helper.CreateCusApplicability(rate4.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate5.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateTaxOrFee("RID", 0.05m, countrycode);
			helper.CreateTaxOrFee("ORD", 0.2m, countrycode);
			helper.CreateNewOrGetExistingVATApplicability(tariff, countrycode, "RID");
			helper.CreateNewOrGetExistingVATApplicability(tariff, countrycode, "ORD");

			factory.Save();
		}

		public static void SetUpTariff(BusinessObjectFactory factory, Dictionary<string, string> tariffCodeRateFormula)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			factory.Save();

			helper.CreateTaxOrFee("RID", 0.05m, Core.Constants.CountryCodes.Latvia);
			helper.CreateTaxOrFee("ORD", 0.2m, Core.Constants.CountryCodes.Latvia);

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");

			var rateType = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode = helper.CreateCusRateCode(factory, "A00", rateType.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");

			foreach (var item in tariffCodeRateFormula)
			{
				var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, item.Key, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");
				var rate = helper.CreateRefCusRate(tariff.PK, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, item.Value, preference.PK);
				helper.CreateCusApplicability(rate.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Latvia, "RID");
				helper.CreateNewOrGetExistingVATApplicability(tariff, Core.Constants.CountryCodes.Latvia, "ORD");
			}

			factory.Save();
		}

		public static void AssertResourceStringData(ZPropertyInfo info, string caption, string mediumCaption, string shortCaption, string fullDescription)
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals("Caption", caption, captionResourceString.Caption);
			AssertEquals("MediumCaption", mediumCaption, captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", shortCaption, captionResourceString.ShortCaption);
			AssertEquals("FullDescription", fullDescription, captionResourceString.FullDescription);
		}

		public static void SetupC0009ForEuAndCtCountries(BusinessObjectFactory factory)
		{
			SetupC0009ForCountries(factory, factory.GetEuropeanUnionAndCtCountries().ToArray());
		}

		public static void SetupC0009ForCountries(BusinessObjectFactory factory, params string[] countries)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, "C0009 Desc");
			foreach (var country in countries)
			{
				helper.CreateNewOrGetExistingDataGrouping(country, parent: eun);
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, country, country, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			}
			factory.Save();
		}

		internal static void SetupCusCodeForGetAdditionalCodes(UniversalReferenceTestDataHelper helper, BusinessObjectFactory factory)
		{
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "Additional Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, "Default Rate", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var lastMonth = ZDateTime.Today.AddMonths(-1);
			var nextMonth = ZDateTime.Today.AddMonths(1);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "AC01", "EU AC01", lastMonth, nextMonth)
				.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "AC03", "EU AC03", lastMonth, nextMonth)
				.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty);
			factory.Save();
		}

		internal static void SetUpTariffSecondUnit(BusinessObjectFactory factory, string countrycode = "LV")
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(countrycode, "test country", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "9111100000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rateType1 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode1 = helper.CreateCusRateCode(factory, "A00", rateType1.PK);
			var reference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");
			var reference2 = helper.CreatePreferenceForCountryAndGrouping("200", "200", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");

			var rateType2 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "SEC");
			var rateCode2 = helper.CreateCusRateCode(factory, "SEC", rateType2.PK);

			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "MIN(MAX(0.500 * [NAR], VFD * 0.027), VFD * 0.046)", reference1.PK);
			var rate2 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", reference2.PK);
			var rate3 = helper.CreateRefCusRate(tariff.PK, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", reference1.PK);

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");
			helper.CreateCusApplicability(rate1.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate2.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusApplicability(rate3.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateTaxOrFee("RID", 0.05m, countrycode);
			helper.CreateTaxOrFee("ORD", 0.2m, countrycode);
			helper.CreateNewOrGetExistingVATApplicability(tariff, countrycode, "RID");
			helper.CreateNewOrGetExistingVATApplicability(tariff, countrycode, "ORD");

			factory.Save();
		}

		internal static void SetUpTariffAllUnits(BusinessObjectFactory factory, ZString countryCode)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "Latvia", parentDataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "IMP");
			factory.Save();

			var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "9111200000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var rateType1 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
			var rateCode1 = helper.CreateCusRateCode(factory, "A00", rateType1.PK);
			var reference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");

			var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0.5000 * [LTR] + 0.500 * [NAR] + 0.500 * [KGM] + 0.500 * [DTN] + 0.02 * VFD", reference1.PK);

			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(tradeGroup, "EU");
			helper.CreateCusApplicability(rate1.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateTaxOrFee("ORD", 0.21m, countryCode);
			helper.CreateTaxOrFee("BRR", 0.06m, countryCode);
			helper.CreateTaxOrFee("BRP", 0.12m, countryCode);
			helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "ORD");
			helper.CreateNewOrGetExistingVATApplicability(tariff, countryCode, "BRP");

			factory.Save();
		}

		internal static void SetupTariffAndRateForAdditionalSupplementaryCodes(BusinessObjectFactory factory)
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var testHelper = new UniversalReferenceTestDataHelper(factory);
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;

			var tradeGroupStandard = testHelper.CreateTradeGroup(currentCountry, "STANDARD", date1, date4);
			testHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Australia, date1, date4);

			var hsnTariffType = testHelper.CreateNewOrGetExistingTariffType(currentCountry, "IMP");
			var dutyRateType = testHelper.CreateNewOrGetExistingRateType(currentCountry, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = testHelper.LoadOrCreateNewCusRateCode(factory, "A00", dutyRateType.PK);
			var preferenceSTD = testHelper.CreatePreferenceForCountry("100", "100", currentCountry);

			var cusTariff = testHelper.CreateTariff(currentCountry, hsnTariffType.PK, "1234512345", date1, date4, "dummy Description 0");

			var testRate1 = testHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
			testHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "additionalcode", "ordernumber");

			testHelper.CreateCusCodeType("ADDCD", "Additional Codes");
			testHelper.CreateCusCodeList(currentCountry, "ADDCD", "additionalcode", "additionalcode", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			factory.Save();
		}

		internal sealed class TemporaryStoragePackedItemForTest : TemporaryStoragePackedItem
		{
			public TemporaryStoragePackedItemForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool CalculateAndRefreshAllDutyAmountFromTariffRatesCore => CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting;

			internal  bool CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting { get; set; }

			internal IValueSetStrategy GetValueSetStrategy_Exposed() => GetValueSetStrategy();
		}
	}
}

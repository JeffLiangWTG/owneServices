using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

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
		var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffTypeCode);
		tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
		factory.Save();

		var tariff = helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, TestTariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Other", compositeKey: "01.03..04.7.9");
		helper.CreateNomenclatureGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "LIVE ANIMALS", compositeKey: "01", nomenclatureGroupType: "CN");
		helper.CreateNomenclatureGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "03", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "FISH AND CRUSTACEANS, MOLLUSCS AND OTHER AQUATIC INVERTEBRATES", compositeKey: "01.03", nomenclatureGroupType: "CN");
		helper.CreateNomenclatureGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "0304", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "Fish fillets and other fish meat (whether or not minced), fresh, chilled or frozen", compositeKey: "01.03..04", nomenclatureGroupType: "CN");
		helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffType.PK, "8001100000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description: "Tin, not alloyed", compositeKey: "15.80..01.1");
		helper.CreateNomenclatureGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "80", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "TIN AND ARTICLES THEREOF", compositeKey: "15.80", nomenclatureGroupType: "CN");

		var rateType1 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DTY");
		var rateCode1 = helper.CreateCusRateCode(factory, "A00", rateType1.PK);
		var preference1 = helper.CreatePreferenceForCountryAndGrouping("100", "100", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");
		var preference2 = helper.CreatePreferenceForCountryAndGrouping("200", "200", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN");

		var rateType2 = helper.CreateCusRateType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "SEC");
		var rateCode2 = helper.CreateCusRateCode(factory, "SEC", rateType2.PK);

		var rate1 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.12", preference1.PK);
		var rate2 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", preference2.PK);
		var rate3 = helper.CreateRefCusRate(tariff.PK, rateCode1.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VFD * 0.08", preference1.PK);
		var rate4 = helper.CreateRefCusRate(tariff.PK, rateCode2.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "0", preference1.PK);

		var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.AddCountry(tradeGroup, "EU");
		helper.CreateCusApplicability(rate1.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusApplicability(rate2.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusApplicability(rate3.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ABCD");
		helper.CreateCusApplicability(rate4.PK, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

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
}

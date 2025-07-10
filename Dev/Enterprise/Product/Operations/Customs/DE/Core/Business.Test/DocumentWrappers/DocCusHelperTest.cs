using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCusHelper))]
	sealed class DocCusHelperTest : TestCaseWithFactory
	{
		public void TestGetGermanRateCodeDescription_WhenTranslationOnCountryLevel()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountryCode = Env.CurrentCompany.Country.Code;

			var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(currentCountryCode, parent: eunId);

			CreateRateCodeWithTranslation(helper, "350", currentCountryCode, "DE Description for current country");
			Factory.Save();

			var germanRateCodeDescriptionCurrentCountry = DocCusHelper.GetGermanRateCodeDescription(Factory, "350", currentCountryCode);
			AssertEquals("DE Description for current country", germanRateCodeDescriptionCurrentCountry);
		}

		public void TestGetGermanRateCodeDescription_WhenTranslationOnEULevel()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountryCode = Env.CurrentCompany.Country.Code;

			var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(currentCountryCode, parent: eunId);

			helper.CreateOrGetLanguage("DE", "German");

			var rateTypeCurrentCountry = helper.CreateNewOrGetExistingRateType(currentCountryCode, Enterprise.Customs.Business.UniversalReferenceConstants.RefCusRateTypes.ExportTaxes, "Export Taxes");
			// having a rate code on country level, but no translation
			helper.CreateCusRateCode(Factory, "350", rateTypeCurrentCountry.PK, description: "EN Description", countryCode: currentCountryCode);

			CreateRateCodeWithTranslation(helper, "350", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DE Description for EU");
			Factory.Save();

			var germanRateCodeDescriptionEU = DocCusHelper.GetGermanRateCodeDescription(Factory, "350", currentCountryCode);
			AssertEquals("DE Description for EU", germanRateCodeDescriptionEU);
		}

		public void TestGetGermanRateCodeDescription_WhenTranslationOnBothLevels()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountryCode = Env.CurrentCompany.Country.Code;

			var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(currentCountryCode, parent: eunId);

			CreateRateCodeWithTranslation(helper, "350", currentCountryCode, "DE Description for current country");
			CreateRateCodeWithTranslation(helper, "350", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "DE Description for EU");
			Factory.Save();

			var germanRateCodeDescriptionEU = DocCusHelper.GetGermanRateCodeDescription(Factory, "350", currentCountryCode);
			AssertEquals("Current company translation has priority over EU", "DE Description for current country", germanRateCodeDescriptionEU);
		}

		public void TestGetGermanRateCodeDescription_RateCodeB00()
		{
			var currentCountryCode = Env.CurrentCompany.Country.Code;
			CombineAssertions(() =>
			{
				var result = DocCusHelper.GetGermanRateCodeDescription(Factory, "B00", currentCountryCode);
				AssertEquals("Translation not from RefCusRateCodeLanguage", "Einfuhrumsatzsteuer (EUSt)", result);

				var helper = new UniversalReferenceTestDataHelper(Factory);
				var eunId = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
				helper.CreateNewOrGetExistingDataGrouping(currentCountryCode, parent: eunId);
				CreateRateCodeWithTranslation(helper, "B00", currentCountryCode, "DE Description for current country");
				Factory.Save();
				result = DocCusHelper.GetGermanRateCodeDescription(Factory, "B00", currentCountryCode);
				AssertEquals("Translation from RefCusRateCodeLanguage", "DE Description for current country", result);
			});
		}

		void CreateRateCodeWithTranslation(UniversalReferenceTestDataHelper helper, ZString rateCode, ZString dataGroupingCode, ZString description)
		{
			helper.CreateOrGetLanguage("DE", "German");
			var rateType = helper.CreateNewOrGetExistingRateType(dataGroupingCode, Enterprise.Customs.Business.UniversalReferenceConstants.RefCusRateTypes.ExportTaxes, "Export Taxes");
			var cusRateCode = helper.CreateCusRateCode(Factory, rateCode, rateType.PK, description: "EN Description", countryCode: dataGroupingCode);
			var rateCodeLanguage = helper.LoadOrCreateNewCusRateCodeLanguage(Factory, cusRateCode, "DE");
			rateCodeLanguage.ZXC_Description = description;
		}
	}
}

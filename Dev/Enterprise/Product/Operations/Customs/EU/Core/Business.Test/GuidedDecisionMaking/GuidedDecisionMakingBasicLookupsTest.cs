using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class GuidedDecisionMakingBasicLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsUQListWithDataGroupCode()
		{
			SetUpCustomsUQs();

			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			guidedDecisionMakingBasic.EffectiveDate = ZDateTime.Today.Date;
			AssertEquals("DG1 CustomsUQ", "GHI", guidedDecisionMakingBasic.Lookups.CustomsUQList.CodesAsString);

			guidedDecisionMakingBasic.DataGrouping = "FR";
			guidedDecisionMakingBasic.ParentDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			AssertEquals("EUN CustomsUQ", "ABC, DEF", guidedDecisionMakingBasic.Lookups.CustomsUQList.CodesAsString);

			guidedDecisionMakingBasic.EffectiveDate = ZDateTime.Today.AddMonths(2).Date;
			AssertEquals("EUN CustomsUQ with date", "DEF", guidedDecisionMakingBasic.Lookups.CustomsUQList.CodesAsString);
		}

		public void TestCustomsSecondUQList()
		{
			SetUpCustomsUQs();
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			guidedDecisionMakingBasic.DataGrouping = "FR";
			guidedDecisionMakingBasic.EffectiveDate = ZDateTime.Today.Date;
			SetUpUnitOfMeasureForTariff();

			AssertEquals("CustomsSecondUQList should contain only one UQ that is defined as CU2 in the tariff.UnitsOfMeasure.", "ABC", guidedDecisionMakingBasic.Lookups.CustomsSecondUQList.CodesAsString);

			void SetUpUnitOfMeasureForTariff()
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateTariffUOM(guidedDecisionMakingBasic.Tariff, UOMTypeList.Codes.CU2, "ABC", Core.Constants.CountryCodes.France);
				Factory.Save();
			}
		}

		void SetUpCustomsUQs()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DEF", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateCusCodeList("DG1", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "GHI", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			Factory.Save();
		}

		public void TestCountryOfOriginList()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			guidedDecisionMakingBasic.DataGrouping = "FR";
			var list = guidedDecisionMakingBasic.Lookups.CountryOfOriginList;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Contains CountryCodes relevant to current (Parent) Data Grouping that is valid at today", new[] { "US", "DE", "AU" }, list.GetAllCodes());
				AssertEquals("Description US", "America", list.GetDescriptionFromCode("US"));
				AssertEquals("Description DE", "German", list.GetDescriptionFromCode("DE"));
				AssertEquals("Description AU", "Australia", list.GetDescriptionFromCode("AU"));

				guidedDecisionMakingBasic.DataGrouping = "AU";
				list = guidedDecisionMakingBasic.Lookups.CountryOfOriginList;
				AssertContainsExactElementsInAnyOrder("Contains CountryCodes relevant to AU", new[] { "NZ" }, list.GetAllCodes());
			});
		}

		public void TestTariffs()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			guidedDecisionMakingBasic.DataGrouping = "FR";
			guidedDecisionMakingBasic.TariffType = "DTY";
			guidedDecisionMakingBasic.EffectiveDate = ZDate.Today;

			var list = guidedDecisionMakingBasic.Lookups.Tariffs;
			CombineAssertions(() =>
			{
				list.Load();
				AssertContainsExactElementsInAnyOrder("FR|DTY|Today tariffCodes", new[] { "1111111111", "2222222222", "3333333333" }, list.Select(x => x.ZZ1_TariffCode));

				guidedDecisionMakingBasic.TariffType = "ADD";
				list = guidedDecisionMakingBasic.Lookups.Tariffs;
				list.Load();
				AssertContainsExactElementsInAnyOrder("FR|ADD|Today tariffCodes", new[] { "4444444444" }, list.Select(x => x.ZZ1_TariffCode));

				guidedDecisionMakingBasic.DataGrouping = "DE";
				list = guidedDecisionMakingBasic.Lookups.Tariffs;
				list.Load();
				AssertEquals("No valid tariff for DE", 0, list.Count);

				guidedDecisionMakingBasic.EffectiveDate = ZDate.Today.AddDays(-5);
				list = guidedDecisionMakingBasic.Lookups.Tariffs;
				list.Load();
				AssertEquals("No valid tariff in the date range", 0, list.Count);
			});
		}

		public void TestPreferenceList()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, false);
			guidedDecisionMakingBasic.EffectiveDate = ZDate.Today;
			var lookups = guidedDecisionMakingBasic.Lookups;
			CombineAssertions(() =>
			{
				AssertEquals("Empty DataGrouping/CountryOfOrigin/TariffCode/DutyRateTypeCode", 0, lookups.PreferenceList.Count);

				guidedDecisionMakingBasic.DataGrouping = "FR";
				AssertEquals("Empty CountryOfOrigin/TariffCode/DutyRateTypeCode", 0, lookups.PreferenceList.Count);

				guidedDecisionMakingBasic.CountryOfOrigin = "US";
				AssertEquals("Empty TariffCode/DutyRateTypeCode", 0, lookups.PreferenceList.Count);

				guidedDecisionMakingBasic.TariffCode = "1111111111";
				AssertEquals("Empty DutyRateTypeCode", 0, lookups.PreferenceList.Count);

				guidedDecisionMakingBasic.DutyRateTypeCode = "RT1";
				guidedDecisionMakingBasic.EffectiveDate = ZDate.Invalid;
				AssertEquals("Invalid EffectiveDate", 0, lookups.PreferenceList.Count);

				guidedDecisionMakingBasic.EffectiveDate = ZDate.Today;
				guidedDecisionMakingBasic.TariffCode = "tt";
				AssertEquals("InValid tariff", 0, lookups.PreferenceList.Count);

				guidedDecisionMakingBasic.TariffCode = "1111111111";
				guidedDecisionMakingBasic.DutyRateTypeCode = "OTH";
				AssertEquals("No rate for OTH DutyRateTypeCode", 0, lookups.PreferenceList.Count);

				guidedDecisionMakingBasic.DutyRateTypeCode = "RT1";
				var list = lookups.PreferenceList;
				AssertContainsExactElementsInAnyOrder("FR|US|tariff1|RT1", new[] { "Pre1", "Pre2" }, list.GetAllCodes());
				AssertEquals("Description Pre1", "Preference 1", list.GetDescriptionFromCode("Pre1"));
				AssertEquals("Description Pre2", "Preference 2", list.GetDescriptionFromCode("Pre2"));

				guidedDecisionMakingBasic.DataGrouping = "AU";
				AssertEquals("No tariff for AU", 0, lookups.PreferenceList.Count);

				guidedDecisionMakingBasic.DataGrouping = "FR";
				guidedDecisionMakingBasic.CountryOfOrigin = "AU";
				AssertContainsExactElementsInAnyOrder("FR|AU|tariff1|RT1: AU belong to tradeGroup2 -> rate2", new[] { "Pre2" }, lookups.PreferenceList.GetAllCodes());

				guidedDecisionMakingBasic.TariffCode = "3333333333";
				AssertContainsExactElementsInAnyOrder("FR|AU|tariff3|RT1: rate3", new[] { "Pre3" }, lookups.PreferenceList.GetAllCodes());

				guidedDecisionMakingBasic.DutyRateTypeCode = "RT2";
				AssertEquals("No rate for RT2", 0, lookups.PreferenceList.Count);
			});
		}

		public void TestPreferenceRetentionDependsOnCountryOfDestinationSettingOrder()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, false);
			AssertEquals("Preference should be retained when CountryOfDestination is set before Preference", "P1", guidedDecisionMakingBasic.Preference);

			guidedDecisionMakingBasic.CountryOfDestination = "GP";
			AssertEquals("Preference should be cleared when CountryOfDestination is set after Preference", ZString.Empty, guidedDecisionMakingBasic.Preference);
		}

		public void TestQuotaOrderNumberList()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, true, false);
			guidedDecisionMakingBasic.DataGrouping = "FR";
			guidedDecisionMakingBasic.ParentDataGrouping = "EU";
			guidedDecisionMakingBasic.TariffType = "DTY";
			guidedDecisionMakingBasic.EffectiveDate = ZDate.Today;

			var lookups = guidedDecisionMakingBasic.Lookups;
			CombineAssertions(() =>
			{
				AssertEquals("Empty DataGrouping/CountryOfOrigin/TariffCode/DutyRateTypeCode", 0, lookups.QuotaOrderNumberList.Count);

				guidedDecisionMakingBasic.DataGrouping = "FR";
				AssertEquals("Empty CountryOfOrigin/TariffCode/DutyRateTypeCode", 0, lookups.QuotaOrderNumberList.Count);

				guidedDecisionMakingBasic.CountryOfOrigin = "US";
				AssertEquals("Empty TariffCode/DutyRateTypeCode", 0, lookups.QuotaOrderNumberList.Count);

				guidedDecisionMakingBasic.TariffCode = "1111111111";
				AssertEquals("Empty DutyRateTypeCode", 0, lookups.QuotaOrderNumberList.Count);

				guidedDecisionMakingBasic.DutyRateTypeCode = "OTH";
				AssertEquals("No rate for OTH DutyRateTypeCode", 0, lookups.QuotaOrderNumberList.Count);

				guidedDecisionMakingBasic.DutyRateTypeCode = "RT1";
				guidedDecisionMakingBasic.EffectiveDate = ZDate.Invalid;
				AssertEquals("invalid EffectiveDate", 0, lookups.QuotaOrderNumberList.Count);

				guidedDecisionMakingBasic.EffectiveDate = ZDate.Today;
				guidedDecisionMakingBasic.TariffCode = "TT";
				AssertEquals("invalid tariff", 0, lookups.QuotaOrderNumberList.Count);

				guidedDecisionMakingBasic.TariffCode = "1111111111";
				guidedDecisionMakingBasic.DutyRateTypeCode = "RT1";
				var list = guidedDecisionMakingBasic.Lookups.QuotaOrderNumberList;
				AssertContainsExactElementsInAnyOrder("FR|US|tariff1|RT1", new[] { "Ord1", "Ord2" }, list.GetAllCodes());
				AssertEquals("Description Ord1", "Ord1", list.GetDescriptionFromCode("Ord1"));
				AssertEquals("Description Ord2", "Ord2", list.GetDescriptionFromCode("Ord2"));

				guidedDecisionMakingBasic.Preference = "Pre1";
				AssertContainsExactElementsInAnyOrder("FR|US|tariff1|RT1|Pre1", new[] { "Ord1" }, lookups.QuotaOrderNumberList.GetAllCodes());

				guidedDecisionMakingBasic.DataGrouping = "AU";
				AssertEquals("No tariff for AU", 0, lookups.QuotaOrderNumberList.Count);

				guidedDecisionMakingBasic.DataGrouping = "FR";
				guidedDecisionMakingBasic.CountryOfOrigin = "AU";
				AssertContainsExactElementsInAnyOrder("FR|AU|tariff1|RT1|Pre1", new[] { "Ord3" }, lookups.QuotaOrderNumberList.GetAllCodes());

				var cacheKey = $"GuidedDecisionMaking_QuotaOrderNumberList_{guidedDecisionMakingBasic.DataGrouping}_{guidedDecisionMakingBasic.TariffCode}_{guidedDecisionMakingBasic.DutyRateTypeCode}_{guidedDecisionMakingBasic.EffectiveTradeGroupCountry}_{guidedDecisionMakingBasic.Preference}_{guidedDecisionMakingBasic.EffectiveDate}";
				Factory.TryGetValueFromCacheOnly<CodeDescriptionPairList>(cacheKey, out var list2);
				AssertSame("list from cacheKey", lookups.QuotaOrderNumberList, list2);

				guidedDecisionMakingBasic.TariffCode = "3333333333";
				AssertContainsExactElementsInAnyOrder("FR|AU|tariff3|RT1|Pre1", new[] { "Ord4" }, lookups.QuotaOrderNumberList.GetAllCodes());

				guidedDecisionMakingBasic.DutyRateTypeCode = "RT2";
				AssertEquals("No rate for RT2", 0, lookups.QuotaOrderNumberList.Count);
			});
		}

		public void TestMeursingResultList()
		{
			var guidedDecisionMakingBasic = GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(Factory, false, false);
			AssertType<MeursingList>(guidedDecisionMakingBasic.Lookups.MeursingResultList);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentDataGroup = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping("FR", "France", parentDataGroup);
			helper.CreateNewOrGetExistingDataGrouping("DE", "German", parentDataGroup);
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
			helper.AddCountry(tradeGroup3, "NZ", startDate, endDate, "Nealand");
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

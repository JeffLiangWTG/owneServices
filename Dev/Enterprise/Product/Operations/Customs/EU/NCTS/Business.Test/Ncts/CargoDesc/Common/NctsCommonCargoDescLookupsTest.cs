using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsCommonCargoDescLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTariffs()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var lvTariffTypePK = Universal.Testing.UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, Core.Constants.CountryCodes.Latvia, Constants.TariffTypes.Import).PK;
			var deTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Germany, lvTariffTypePK, "01345698", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var lvTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Latvia, lvTariffTypePK, "01345698", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var tariffs = line.Lookups.Tariffs;

			CombineAssertions(() =>
			{
				AssertSame("Cached", tariffs, line.Lookups.Tariffs);
				AssertEquals("Wrong DataGroupCode", false, deTariff.MatchesFilter(line.Lookups.Tariffs.CompleteFilter));
				AssertEquals("Correct DataGroupCode", true, lvTariff.MatchesFilter(line.Lookups.Tariffs.CompleteFilter));
			});
		}

		public void TestWeightUnitList()
		{
			AssertEquals(OLookUpEditType.Weight, lookupsNcts4.WeightUnitList.LookupEditType);
		}

		public void TestCusCodeListDataGroupingCode()
		{
			AssertEquals(nctsHeader.DefaultDataGroupingCode, lookupsNcts4.CusCodeListDataGroupingCode);
		}

		public void TestCusCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia");
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "CUS Codes");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "01000001", "CUSCode 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "CNCODE", "11000001");
			Factory.Save();

			line.BY_HarmonisedTariff = "11000001";
			var list = line.Lookups.CusCodeList;
			list.Load();
			CombineAssertions("When Tariff is not empty", () =>
			{
				AssertEquals("CNCODE is the default for Attribute Name", "CNCODE", list.FilterBusinessObjectDefaults["Attribute Name:Property"].Value);
				AssertEquals("Tariff codes first 6 characters is the default for Attribute Value", "110000", list.FilterBusinessObjectDefaults["Attribute Value:Property"].Value);
			});

			line.BY_HarmonisedTariff = ZString.Empty;
			list = line.Lookups.CusCodeList;
			list.Load();

			CombineAssertions("When Tariff is empty", () =>
			{
				AssertEquals("Attribute Name filter is not present", false, list.FilterBusinessObjectDefaults.ContainsDefaultFor("Attribute Name:Property"));
				AssertEquals("Attribute Value filter is not present", false, list.FilterBusinessObjectDefaults.ContainsDefaultFor("Attribute Value:Property"));
			});
		}

		public void TestCusCodeList_IsCL016CodeListFilterActive_True()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				SetupCL016List(isCL016CodeListFilterActive: true, countryCode: Core.Constants.CountryCodes.Italy, countryName: "Italy");

				var list = lookupsDeparture.CusCodeList;
				list.Load();

				AssertEquals("Count", 1, list.Count);
				AssertEquals("ZZD_Code", "0010001-6", list[0].ZZD_Code);
			}
		}

		public void TestCusCodeList_IsCL016CodeListFilterActive_False()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				SetupCL016List(isCL016CodeListFilterActive: false, countryCode: Core.Constants.CountryCodes.Latvia, countryName: "Latvia");

				var list = lookupsDeparture.CusCodeList;
				list.Load();

				AssertEquals("Count", 3, list.Count);
			}
		}

		public void TestCachedListOfAdditionalCodeDescriptions()
		{
			SetupAdditionalCodes();

			AssertCachedListOfAdditionalCodeDescriptions("Departure NCTS4 Good Item", lookupsNcts4);
			AssertCachedListOfAdditionalCodeDescriptions("Departure NCTS5 Good Item", lookupsDeparture);
			AssertCachedListOfAdditionalCodeDescriptions("Arrival Good Item", lookupsArrival);

			void AssertCachedListOfAdditionalCodeDescriptions(ZString assertionText, NctsCommonCargoDescLookups lookups)
			{
				var additionalCodesList = lookups.CachedListOfAdditionalCodeDescriptions;
				CombineAssertions(assertionText, () =>
				{
					AssertContainsExactElementsInAnyOrder("Codes List", new ZString[] { "add1", "add4" }, additionalCodesList.GetAllCodesZString());
					AssertEquals("Code List description add1", "add1 Descriptions", additionalCodesList.GetDescriptionFromCode("add1"));
				});
			}
		}

		public void TestCachedListOfAdditionalCodeDescriptions_Language()
		{
			SetupAdditionalCodes();
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.French;
			Factory.Save();

			AssertCachedListOfAdditionalCodeDescriptions_Language("Departure NCTS4 Good Item", lookupsNcts4);
			AssertCachedListOfAdditionalCodeDescriptions_Language("Departure NCTS5 Good Item", lookupsDeparture);
			AssertCachedListOfAdditionalCodeDescriptions_Language("Arrival Good Item", lookupsArrival);

			void AssertCachedListOfAdditionalCodeDescriptions_Language(ZString assertionText, NctsCommonCargoDescLookups lookups)
			{
				var additionalCodesList = lookups.CachedListOfAdditionalCodeDescriptions;
				using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				{
					CombineAssertions(assertionText, () =>
					{
						AssertEquals("French description add1", "add1 La description", additionalCodesList.GetDescriptionFromCode("add1"));
						AssertEquals("Code List description add4", "add4 Descriptions", additionalCodesList.GetDescriptionFromCode("add4"));
					});
				}
			}
		}

		public void TestCustomsUnitOfQuantityList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DEF", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Argentina, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "GHI", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "JKL", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			Factory.Save();

			AssertCustomsUnitOfQuantityList("Departure NCTS4 Good Item", lookupsNcts4);
			AssertCustomsUnitOfQuantityList("Departure NCTS5 Good Item", lookupsDeparture);
			AssertCustomsUnitOfQuantityList("Arrival Good Item", lookupsArrival);

			void AssertCustomsUnitOfQuantityList(ZString assertionText, NctsCommonCargoDescLookups lookups)
			{
				CombineAssertions(assertionText, () =>
				{
					var list = lookups.CustomsUnitOfQuantityList;
					AssertEquals("Declaration falls back to logged in company and Todays date", "ABC, DEF, JKL", list.CodesAsString);
					AssertSame("Cached", list, lookups.CustomsUnitOfQuantityList);
				});
			}
		}

		public void TestCountryOfOriginList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			AssertCountryOfOriginList("Departure NCTS4 Good Item", lookupsNcts4);
			AssertCountryOfOriginList("Departure NCTS5 Good Item", lookupsDeparture);
			AssertCountryOfOriginList("Arrival Good Item", lookupsArrival);

			void AssertCountryOfOriginList(ZString assertionText, NctsCommonCargoDescLookups lookups)
			{
				var countryOfOriginList = lookups.CountryOfOriginList;
				CombineAssertions(assertionText, () =>
				{
					AssertEquals("Codes From List", "AU, DE, FR", countryOfOriginList.CodesAsString);
					AssertSame("Cached", countryOfOriginList, lookups.CountryOfOriginList);
				});
			}
		}

		public void TestAdditionalCodesList()
		{
			SetupTariffAndRate();
			SetupAdditionalCodes();

			var nctsheader = Factory.New<NctsHeader>();
			nctsheader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsheader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var goodItem = nctsheader.Bills.AddNew().GoodsItems.AddNew();

			goodItem.BY_HarmonisedTariff = "DUMMYTRF";
			goodItem.BY_RN_NKCountryOfOrigin = Core.Constants.CountryCodes.Botswana;

			CombineAssertions("AdditionalCodesList when preference and ordernumber and rateType and rateCode is empty ", () =>
			{
				var additionalCodesList = goodItem.Lookups.AdditionalCodeList;
				AssertContainsExactElementsInAnyOrder(new ZString[] { "add11", "add12", "add21", "add22" }, additionalCodesList.GetAllCodesZString());
			});

			goodItem.BY_RN_NKCountryOfOrigin = ZString.Empty;

			CombineAssertions("AdditionalCodesList when preference and ordernumber and rateType and rateCode is empty ", () =>
			{
				var additionalCodesList = goodItem.Lookups.AdditionalCodeList;
				AssertContainsExactElementsInAnyOrder(new ZString[] { "add31" }, additionalCodesList.GetAllCodesZString());
			});
		}

		void SetupTariffAndRate(string overrideDataGrouping = "")
		{
			var date1 = new ZDate(2010, 12, 10);
			var date4 = new ZDate(2079, 06, 06);
			var dataGrouping = string.IsNullOrEmpty(overrideDataGrouping) ? GlbCompany.CurrentCompany.Country.Code : new ZString(overrideDataGrouping);

			var tradeGroupStandard = RefDataHelper.CreateTradeGroup(dataGrouping, "STANDARD", date1, date4);
			RefDataHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Botswana, date1, date4);

			var tradeGroupStandard2 = RefDataHelper.CreateTradeGroup(dataGrouping, "STANDARD2", date1, date4);
			RefDataHelper.AddCountry(tradeGroupStandard2, Core.Constants.CountryCodes.EuropeanUnion, date1, date4);
			Factory.Save();

			var hsnTariffType = RefDataHelper.CreateNewOrGetExistingTariffType(dataGrouping, Universal.Constants.TariffTypes.Export);
			Factory.Save();
			var dutyRateType = RefDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.Duty, "Duty");
			var rateCode1 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);
			var addRateType = RefDataHelper.CreateNewOrGetExistingRateType(dataGrouping, Universal.Constants.RateTypes.AntiDumping, "ADD");
			var rateCode2 = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, "AD1", addRateType.PK);
			Factory.Save();
			var preferenceSTD = RefDataHelper.CreatePreferenceForCountry("STD", "Standard", dataGrouping);
			var preferenceRED = RefDataHelper.CreatePreferenceForCountry("RED", "Reduced", dataGrouping);
			var preferenceMFN = RefDataHelper.CreatePreferenceForCountry("MFN", "Most-favored Nation Duty", dataGrouping);
			Factory.Save();

			var cusTariff = RefDataHelper.CreateTariff(dataGrouping, hsnTariffType.PK, "DUMMYTRF", date1, date4, "dummy Description 0");
			Factory.Save();

			var testRate1 = RefDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceSTD.PK);
			RefDataHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "add11", "ord11");
			RefDataHelper.CreateCusApplicability(testRate1, tradeGroupStandard, date1, date4, "add12", "ord12");

			var testRate2 = RefDataHelper.CreateRate(cusTariff, rateCode1.PK, date1, date4, "0", preferencePk: preferenceRED.PK);
			RefDataHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4, "add21", "ord21");
			RefDataHelper.CreateCusApplicability(testRate2, tradeGroupStandard, date1, date4, "add22", "ord22");

			var testRate3 = RefDataHelper.CreateRate(cusTariff, rateCode2.PK, date1, date4, "0", preferencePk: preferenceRED.PK);
			RefDataHelper.CreateCusApplicability(testRate3, tradeGroupStandard2, date1, date4, "add31", "ord31");
			Factory.Save();
		}

		void SetupAdditionalCodes()
		{
			const string additionalCodes = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes;
			const string latvia = Core.Constants.CountryCodes.Latvia;
			var lastMonth = ZDateTime.Today.AddMonths(-1);
			var nextMonth = ZDateTime.Today.AddMonths(1);
			RefDataHelper.CreateCusCodeType(additionalCodes, "Additional Codes", latvia);
			RefDataHelper.CreateCusCodeType(additionalCodes, "Additional Codes", Core.Constants.CountryCodes.Italy);
			RefDataHelper.CreateCusCodeType(additionalCodes, "Additional Codes", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			RefDataHelper.CreateOrGetLanguage(Core.Constants.CountryCodes.France, "French");
			Factory.Save();

			var addcdCodeList1 = RefDataHelper.CreateCusCodeList(latvia, additionalCodes, "add1", "add1 Descriptions", lastMonth, nextMonth);
			RefDataHelper.CreateCusCodeListLanguage(addcdCodeList1, Core.Constants.CountryCodes.France, "add1 La description");
			RefDataHelper.CreateCusCodeList(latvia, additionalCodes, "add2", "add2 Descriptions", ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(1));
			RefDataHelper.CreateCusCodeList(latvia, additionalCodes, "add3", "add3 Descriptions", ZDateTime.Today.AddDays(-4), ZDateTime.Today.AddDays(-2));
			RefDataHelper.CreateCusCodeList(latvia, additionalCodes, "add4", "add4 Descriptions", lastMonth, nextMonth);
			RefDataHelper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, additionalCodes, "add5", "add5 Descriptions", lastMonth, nextMonth);
			RefDataHelper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, additionalCodes, "add2", "addEU2 Descriptions", lastMonth, nextMonth);
			Factory.Save();
		}

		UniversalReferenceTestDataHelper SetupCL016List(bool isCL016CodeListFilterActive, string countryCode, string countryName)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(countryCode, countryName, eun);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS,
				"European Customs Inventory of Chemical Substance", EconomicGroupList.Codes.EuropeanUnion);

			helper.CreateCusCodeListsForMultipleTypesWithAttributes(
				EconomicGroupList.Codes.EuropeanUnion,
				[EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS],
				"0010001-6",
				"abietic acid, technical",
				new Dictionary<string, string[]>
				{
			{ "CNCODE", ["38061000"] },
			{ "CL016", ["Y"] }
				},
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTime
			);

			helper.CreateCusCodeListWithAttribute(
				EconomicGroupList.Codes.EuropeanUnion,
				EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS,
				"0013011-0",
				"cyclobutrifluram",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTime,
				"CL016",
				"N"
			);

			helper.CreateCusCodeListWithAttribute(
				EconomicGroupList.Codes.EuropeanUnion,
				EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS,
				"0012540-6",
				"2-(N-butylanilino)ethanol",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTime,
				"CNCODE",
				"29221900"
			);

			Factory.Save();

			if (isCL016CodeListFilterActive)
			{
				NctsConfigurationTestHelper.TemporarilySetGoodsItemsConfiguration(Factory, "IsCL016CodeListFilterActive", true);
			}

			return helper;
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			line = nctsHeader.MovementHeader.GoodsItems.AddNew();
			lookupsNcts4 = line.Lookups;

			var nctsHeaderDeparture = Factory.New<NctsHeader>();
			nctsHeaderDeparture.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeaderDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItemDeparture = nctsHeaderDeparture.Bills.AddNew().GoodsItems.AddNew();
			lookupsDeparture = (NctsDepartureCargoDescPhase5Lookups)goodsItemDeparture.Lookups;

			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var goodsItemArrival = arrivalHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
			goodsItemArrival.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			lookupsArrival = goodsItemArrival.Lookups;
		}

		NctsHeader nctsHeader;
		NctsCommonCargoDesc line;
		NctsCommonCargoDescLookups lookupsNcts4;
		NctsCommonCargoDescLookups lookupsDeparture;
		NctsCommonCargoDescLookups lookupsArrival;
		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;
	}
}

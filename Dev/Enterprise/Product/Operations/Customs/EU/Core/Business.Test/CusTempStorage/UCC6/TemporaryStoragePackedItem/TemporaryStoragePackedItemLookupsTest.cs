using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class TemporaryStoragePackedItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookupsItems()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var lookups = packedItem.Lookups;
			AssertNotNull(lookups.TariffList);
			AssertEquals(lookups.TariffList.GetType(), typeof(TariffViewCollection));
			AssertNotNull(lookups.GrossWeightUQList);
		}

		public void TestCachedListOfAdditionalCodeDescriptions()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			const string additionalCodes = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes;
			var eurpoeanUnionCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

			var eunId = helper.CreateNewOrGetExistingDataGrouping(eurpoeanUnionCode);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunId);

			helper.CreateNewOrGetExistingCusCodeType(additionalCodes, "Additional Codes");

			var addcdCodeList1 = helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, additionalCodes, "ABC", "ABC Descriptions", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateNewOrGetExistingCusCodeListLanguage(addcdCodeList1, "LT", "ABC Descriptions");
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, additionalCodes, "DEF", "DEF Descriptions", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Argentina, additionalCodes, "GHI", "GHI Descriptions", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateNewOrGetExistingCusCodeList(eurpoeanUnionCode, additionalCodes, "JKL", "JKL Descriptions", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));

			helper.CreateOrGetLanguage("LT", "Latvian");

			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = "LTV";
			Factory.Save();

			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var lookups = packedItem.Lookups;

			CombineAssertions(() =>
			{
				var additionalCodesList = lookups.CachedListOfAdditionalCodeDescriptions;
				AssertEquals("Codes List Contains ABC", true, additionalCodesList.GetAllCodesZString().Contains("ABC"));
				AssertEquals("Codes List Contains DEF", true, additionalCodesList.GetAllCodesZString().Contains("DEF"));
				AssertEquals("Codes List Contains JKL", true, additionalCodesList.GetAllCodesZString().Contains("JKL"));
				AssertEquals("Code List description ABC", "ABC Descriptions", additionalCodesList.GetDescriptionFromCode("ABC"));
			});
		}

		public void TestGrossWeightUQList()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var lookups = packedItem.Lookups;
			var weightUnits = lookups.GrossWeightUQList;
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), weightUnits);
			Assert("Weight Units", weightUnits.ContainsCode(Core.Constants.Weight.Kilograms));
		}

		public void TestNetWeightUQList()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var lookups = packedItem.Lookups;
			var weightUnits = lookups.NetWeightUQList;
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), weightUnits);
			Assert("Weight Units", weightUnits.ContainsCode(Core.Constants.Weight.Kilograms));
		}

		public void TestChemicalSubstanceCodeList_NoCodeForDataGrouping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "ECICS");

			var eunDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var lvDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunDataGrouping);
			Factory.Save();

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var codeLists1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "C", "European", yesterday, tomorrow);
			var codeLists2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "D", "Foreign", yesterday, tomorrow);
			Factory.Save();

			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var lookups = packedItem.Lookups;

			var codeList = lookups.ChemicalSubstanceCodeList;
			codeList.Load();

			AssertEquals("Prerequiste", Core.Constants.CountryCodes.Latvia, header.DataGrouping);
			AssertContainsExactElementsInAnyOrder("Only European valid codes of type ECICS should be in the ChemicalSubstanceCodeList list when no valid code is available for current data grouping.",
				new ZGuid[] { codeLists1.PK }, codeList.Select(x => x.PK));
		}

		public void TestChemicalSubstanceCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "ECICS");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "Transport Charges Method Of Payment");

			var eunDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var lvDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunDataGrouping);
			Factory.Save();

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var codeLists1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "A", "Latvian 1", yesterday, tomorrow);
			var codeLists2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "B", "Latvian 1", yesterday, tomorrow);
			var codeLists3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "C", "Bad Type", yesterday, tomorrow);
			var codeLists4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "D", "European", yesterday, tomorrow);
			var codeLists5 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "E", "Outdated", yesterday, yesterday);
			var codeLists6 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "F", "European 1", yesterday, tomorrow);
			Factory.Save();

			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var lookups = packedItem.Lookups;

			var codeList = lookups.ChemicalSubstanceCodeList;
			codeList.Load();

			AssertEquals("Prerequiste", Core.Constants.CountryCodes.Latvia, header.DataGrouping);

			AssertContainsExactElementsInAnyOrder("Matched CodeType: ECICS, dataGrouping: Latvia, date: today",
				new ZGuid[] { codeLists1.PK, codeLists2.PK }, codeList.Select(x => x.PK));
		}

		public void TestTariffLists()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentDataGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", parentDataGroup);

			var impType = helper.CreateTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff);
			var addType = helper.CreateTariffType(Core.Constants.CountryCodes.Latvia, "ADD");
			Factory.Save();

			var startDate = ZDate.Today.AddDays(-2);
			var endDate = ZDate.Today.AddDays(2);
			helper.CreateTariff(Core.Constants.CountryCodes.Latvia, impType.PK, "1111111111", startDate, endDate);
			helper.CreateTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, impType.PK, "2222222222", startDate, endDate);
			helper.CreateTariff(Core.Constants.CountryCodes.Latvia, impType.PK, "3333333333", startDate, endDate);
			helper.CreateTariff(Core.Constants.CountryCodes.Latvia, addType.PK, "4444444444", startDate, endDate);
			Factory.Save();

			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var list = packedItem.Lookups.TariffList;
			list.Load();
			AssertContainsExactElementsInAnyOrder("Latvia|IMP|Today tariffCodes", new[] { "1111111111", "2222222222", "3333333333" }, list.Select(x => x.ZZ1_TariffCode));
		}

		public void TestCustomsUnitOfQuantityList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", "ABC Descriptions", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DEF", "DEF Descriptions", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Argentina, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "GHI", "GHI Descriptions", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "JKL", "JKL Descriptions", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			Factory.Save();

			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var lookups = packedItem.Lookups;
			CombineAssertions(() =>
			{
				var list = lookups.CustomsUnitOfQuantityList;
				AssertEquals("Declaration falls back to logged in company and Todays date", "ABC, DEF, JKL", list.CodesAsString);
				AssertSame("Cached", list, lookups.CustomsUnitOfQuantityList);
			});
		}

		public void TestCurrencies()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();

			AssertNotNull(packedItem.Lookups.Currencies);
		}

		public void TestCountryOfOriginList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008;
			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var europeanUnionCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var eunId = helper.CreateNewOrGetExistingDataGrouping(europeanUnionCode);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunId);

			helper.CreateNewOrGetExistingCusCodeType(codeType, "CountryList – NCTS");
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, Core.Constants.CountryCodes.Australia, "Australien", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, Core.Constants.CountryCodes.Germany, "Deutschland", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, Core.Constants.CountryCodes.France, "Frankreich", startDate, endDate);

			Factory.Save();

			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var lookups = packedItem.Lookups;

			var countryOfOriginList = lookups.CountryOfOriginList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes From List", "AU, DE, FR", countryOfOriginList.CodesAsString);
				AssertSame("Cached", countryOfOriginList, lookups.CountryOfOriginList);
			});
		}
	}
}

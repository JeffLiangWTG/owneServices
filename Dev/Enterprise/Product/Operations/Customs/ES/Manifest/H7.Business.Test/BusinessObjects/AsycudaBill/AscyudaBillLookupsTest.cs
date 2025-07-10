using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class AscyudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestImporterIdentificationTypeList()
		{
			var asycudaBill = Factory.New<AsycudaBill>();
			var codeList = asycudaBill.Lookups.ImporterIdentificationTypeList;

			CombineAssertions(() =>
			{
				AssertEquals("There are three elements in the list", 3, codeList.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "EOR", "NIF", "TIN" }, codeList.GetAllCodes());
			});
		}

		public void TestDocumentationRequired()
		{
			var billLookups = Factory.New<AsycudaBill>().Lookups;
			AssertNotNull(billLookups.DocumentationRequired);
			AssertContainsExactElementsInAnyOrder(new[] { "Yes", "No" }, billLookups.DocumentationRequired.ToArray().Select(x => x.Description));
		}

		public void TestAdditionalProcedureCodeList()
		{
			var asycudaBill = Factory.New<AsycudaBill>();
			var codeList = asycudaBill.Lookups.AdditionalProcedureList;

			CombineAssertions(() =>
			{
				AssertEquals("There are seven elements in the list", 7, codeList.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "C07", "C07+F48", "C07+F49", "C08", "C16", "C35", "C36" }, codeList.GetAllCodes());
			});
		}

		public void TestShipperCountriesCodeList()
		{
			SetupTradeGroupsAndCountries();
			var bill = Factory.New<AsycudaBill>();

			CombineAssertions(() =>
			{
				var countryList = bill.Lookups.ShipperCountries;
				AssertSame("Cached", countryList, bill.Lookups.ShipperCountries);
				AssertEquals("ES, PL", ((CodeDescriptionPairList)countryList).CodesAsString);
			});
		}

		public void TestConsigneeCountriesCodeList()
		{
			SetupTradeGroupsAndCountries();
			var bill = Factory.New<AsycudaBill>();

			CombineAssertions(() =>
			{
				var countryList = bill.Lookups.ConsigneeCountries;
				AssertSame("Cached", countryList, bill.Lookups.ConsigneeCountries);
				AssertEquals("ES, PL", ((CodeDescriptionPairList)countryList).CodesAsString);
			});
		}

		public void TestConsigneeState_List_NotES()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "X7";
			var state1 = Factory.New<RefCountryStates>();
			var state2 = Factory.New<RefCountryStates>();
			state1.RW_RN_NKCountryCode = country.RN_Code;
			state1.RW_Code = "TP1";
			state1.RW_Description = "TP1 DESC";
			state2.RW_RN_NKCountryCode = country.RN_Code;
			state2.RW_Code = "ST2";
			state2.RW_Description = "ST2 DESC 2";

			var bill = Factory.New<AsycudaBill>();
			AssertEquals("ConsigneeState_List should have no elements.", 0, bill.Lookups.ConsigneeState_List.Count);

			bill.ABL_RN_NKConsigneeCountry = "X7";
			var list = bill.Lookups.ConsigneeState_List;
			AssertEquals("ConsigneeState_List should have 2 elements.", 2, list.Count);
			AssertEquals("ST2 DESC 2", list.GetDescriptionFromCode("ST2"));
			AssertEquals("TP1 DESC", list.GetDescriptionFromCode("TP1"));
		}

		public void TestConsigneeState_List_ES()
		{
			InitCustomsFiscalTerritories();
			var bill = Factory.New<AsycudaBill>();
			AssertEquals("ConsigneeState_List should have no elements.", 0, bill.Lookups.ConsigneeState_List.Count);

			bill.ABL_RN_NKConsigneeCountry = "ES";
			var list = bill.Lookups.ConsigneeState_List;
			AssertEquals("ConsigneeState_List should have 1 elements.", 1, list.Count);
			AssertEquals("35 Desc.", list.GetDescriptionFromCode("35"));
		}

		public void TestCustomsStatusList()
		{
			var asycudaBill = Factory.New<AsycudaBill>();

			CombineAssertions(() =>
			{
				var customStatusCodes = asycudaBill.Lookups.CustomsStatusList;
				AssertSame("Cached", customStatusCodes, asycudaBill.Lookups.CustomsStatusList);
				AssertEquals("CustomsStatusList should be the same as ESH7AISEntryStatusList", new ESH7AISEntryStatusList().CodesAsString, asycudaBill.Lookups.CustomsStatusList.CodesAsString);
			});
		}

		public void TestShipperState_List_NotES()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "X7";
			var state1 = Factory.New<RefCountryStates>();
			var state2 = Factory.New<RefCountryStates>();
			state1.RW_RN_NKCountryCode = country.RN_Code;
			state1.RW_Code = "TP1";
			state1.RW_Description = "TP1 DESC";
			state2.RW_RN_NKCountryCode = country.RN_Code;
			state2.RW_Code = "ST2";
			state2.RW_Description = "ST2 DESC 2";

			var bill = Factory.New<AsycudaBill>();
			AssertEquals("ShipperState_List should have no elements.", 0, bill.Lookups.ShipperState_List.Count);

			bill.ABL_RN_NKShipperCountry = "X7";
			var list = bill.Lookups.ShipperState_List;
			AssertEquals("ShipperState_List should have 2 elements.", 2, list.Count);
			AssertEquals("ST2 DESC 2", list.GetDescriptionFromCode("ST2"));
			AssertEquals("TP1 DESC", list.GetDescriptionFromCode("TP1"));
		}

		public void TestShipperState_List_ES()
		{
			InitCustomsFiscalTerritories();
			var bill = Factory.New<AsycudaBill>();
			AssertEquals("ShipperState_List should have no elements.", 0, bill.Lookups.ShipperState_List.Count);

			bill.ABL_RN_NKShipperCountry = "ES";
			var list = bill.Lookups.ShipperState_List;
			AssertEquals("ShipperState_List should have 1 elements.", 1, list.Count);
			AssertEquals("35 Desc.", list.GetDescriptionFromCode("35"));
		}

		public void TestSellerState_List_NotES()
		{
			var country = Factory.New<RefCountry>();
			country.RN_Code = "X7";
			var state1 = Factory.New<RefCountryStates>();
			var state2 = Factory.New<RefCountryStates>();
			state1.RW_RN_NKCountryCode = country.RN_Code;
			state1.RW_Code = "TP1";
			state1.RW_Description = "TP1 DESC";
			state2.RW_RN_NKCountryCode = country.RN_Code;
			state2.RW_Code = "ST2";
			state2.RW_Description = "ST2 DESC 2";

			var bill = Factory.New<AsycudaBill>();
			AssertEquals("SellerState_List should have no elements.", 0, bill.Lookups.SellerState_List.Count);

			bill.ABL_RN_NKSellerCountry = "X7";
			var list = bill.Lookups.SellerState_List;
			AssertEquals("SellerState_List should have 2 elements.", 2, list.Count);
			AssertEquals("ST2 DESC 2", list.GetDescriptionFromCode("ST2"));
			AssertEquals("TP1 DESC", list.GetDescriptionFromCode("TP1"));
		}

		public void TestSellerState_List_ES()
		{
			InitCustomsFiscalTerritories();
			var bill = Factory.New<AsycudaBill>();
			AssertEquals("SellerState_List should have no elements.", 0, bill.Lookups.SellerState_List.Count);

			bill.ABL_RN_NKSellerCountry = "ES";
			var list = bill.Lookups.SellerState_List;
			AssertEquals("SellerState_List should have 1 elements.", 1, list.Count);
			AssertEquals("35 Desc.", list.GetDescriptionFromCode("35"));
		}

		void SetupTradeGroupsAndCountries()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, "Spain", euDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, "Poland", euDataGrouping);
			var group1010 = helper.CreateTradeGroup(EconomicGroupList.Codes.EuropeanUnion, "1010", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(group1010, Core.Constants.CountryCodes.Poland, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.Today.AddDays(2).Date);
			helper.AddCountry(group1010, Core.Constants.CountryCodes.Spain, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.Today.AddDays(2).Date);

			Factory.Save();
		}

		void InitCustomsFiscalTerritories()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "Customs Fiscal Territories", "ESC");
			helper.CreateNewOrGetExistingCusCodeList("ESC", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "35", "35 Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			Factory.Save();
		}

		public void TestMessageStatusList()
		{
			var asycudaBill = Factory.New<AsycudaBill>();

			CombineAssertions(() =>
			{
				var messageStatuslist = asycudaBill.Lookups.MessageStatusList;
				AssertSame("Cached", messageStatuslist, asycudaBill.Lookups.MessageStatusList);
				AssertEquals("MessageStatusList should be the same as LogicalStatusList", new LogicalStatusList().CodesAsString, asycudaBill.Lookups.MessageStatusList.CodesAsString);
			});
		}
	}
}

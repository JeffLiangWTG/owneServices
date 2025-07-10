using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsArrivalCargoDescLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUnloadedStates_IsNew()
		{
			var list = lookups.UnloadedStates;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "DEC, DIF, MIS, NEW", list.CodesAsString);
				AssertSame("Cached", list, lookups.UnloadedStates);
			});
		}

		public void TestUnloadedStates()
		{
			CombineAssertions(() =>
			{
				line.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				Factory.Save();

				var list = lookups.UnloadedStates;
				AssertEquals("CodesAsString", "DEC, DIF, MIS", list.CodesAsString);
				AssertSame("Cached", list, lookups.UnloadedStates);
			});
		}

		[TestDate(2022, 09, 29)]
		public void TestCusCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.MasterFiles.Business.EconomicGroupList.Codes.EuropeanUnion);
			var latviaCountryCode = Core.Constants.CountryCodes.Latvia;
			helper.CreateNewOrGetExistingDataGrouping(latviaCountryCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "European Customs Inventory of Chemical Substance");
			helper.CreateCusCodeList(latviaCountryCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "0018137-1", "DESC1", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
			helper.CreateCusCodeList(latviaCountryCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "0018137-2", "DESC2", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
			helper.CreateCusCodeList(latviaCountryCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "0018137-3", "DESC3", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 02, 01));
			Factory.Save();

			var list = lookups.CusCodeList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Elements", new[] { "0018137-1", "0018137-2" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, lookups.CusCodeList);
			});
		}

		public void TestCusCodeList_HarmonisedTariff()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia");
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "CUS Codes");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "01000001", "CUSCode 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "CNCODE", "11000001");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "01000002", "CUSCode 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "CNCODE", "21000002");
			Factory.Save();

			line.BY_HarmonisedTariff = "11000001";
			var list = lookups.CusCodeList;
			list.Load();
			CombineAssertions(() =>
			{
				AssertSame("Tariff - Lists should be the same", list, line.Lookups.CusCodeList);
				AssertEquals("Tariff - The code linked to the commodity should be retrieved", "110000", list.FilterBusinessObjectDefaults["Attribute Value:Property"].Value);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = header.Bills.AddNew();
			line = bill.ArrivalGoodsItems.AddNew();
			lookups = new NctsArrivalCargoDescLookups(line);
		}
		NctsArrivalCargoDesc line;
		NctsArrivalCargoDescLookups lookups;
	}
}

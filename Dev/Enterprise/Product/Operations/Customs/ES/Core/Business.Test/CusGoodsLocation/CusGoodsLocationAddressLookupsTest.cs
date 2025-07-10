using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	internal class CusGoodsLocationAddressLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAuthorisationNumberListTypes()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var location = entryInstruction.GoodsLocation;
			var locationAddress = location.Address;
			var lookups = locationAddress.Lookups;

			AssertType<CusAuthorisationHeaderCollectionFiltered>(lookups.AuthorisationNumberList);

			location.CGL_Qualifier = "Y";
			location.CGL_Type = "B";
			AssertType<ZZRefCusCodeListCombinedCollection>(lookups.AuthorisationNumberList);
		}

		public void TestAuthorisationNumberList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var eunCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var spainCode = Core.Constants.CountryCodes.Spain;
			var italyCode = Core.Constants.CountryCodes.Italy;
			var grouping = helper.CreateNewOrGetExistingDataGrouping(eunCode);
			helper.CreateNewOrGetExistingDataGrouping(spainCode, parent: grouping);
			helper.CreateNewOrGetExistingDataGrouping(italyCode, parent: grouping);

			var locCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType;
			helper.CreateNewOrGetExistingCusCodeType(locCode, "Locations");
			helper.CreateNewOrGetExistingCusCodeType("AAA", "Invalid Type");
			Factory.Save();
			var refCusCodeList1 = helper.CreateCusCodeList(eunCode, locCode, "SD01", "SD01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList2 = helper.CreateCusCodeList(spainCode, locCode, "ES00010100DECO", "Test 1", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			var refCusCodeList3 = helper.CreateCusCodeList(spainCode, locCode, "ES00010101EAT", "Test 2", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			var refCusCodeList4 = helper.CreateCusCodeList(spainCode, locCode, "ES00010101GENE", "Test 3", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			var refCusCodeList5 = helper.CreateCusCodeList(italyCode, locCode, "IT00010", "Test 4", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			var refCusCodeList6 = helper.CreateCusCodeList(spainCode, "AAA", "INV02", "Invalid Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList7 = helper.CreateCusCodeList(spainCode, locCode, "INV03", "Invalid StartDate", ZDateTime.Today.AddDays(2), ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList8 = helper.CreateCusCodeList(spainCode, locCode, "INV04", "Invalid EndDate", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var entryInstruction = Factory.New<CusEntryInstruction>();
			var location = entryInstruction.GoodsLocation;
			location.CGL_Qualifier = "Y";
			location.CGL_Type = "B";
			var locationAddress = location.Address;
			var lookups = locationAddress.Lookups;
			var locationsList = lookups.AuthorisationNumberList;

			CombineAssertions(() =>
			{
				AssertType<ZZRefCusCodeListCombinedCollection>("List Type", locationsList);
				AssertSame("Cached", locationsList, lookups.AuthorisationNumberList);

				var completeFilter = ((ZZRefCusCodeListCombinedCollection)locationsList).CompleteFilter;
				AssertEquals("Unmatched EUN", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList1.PK).MatchesFilter(completeFilter));
				AssertEquals("Matched ES 1", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList2.PK).MatchesFilter(completeFilter));
				AssertEquals("Matched ES 2", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList3.PK).MatchesFilter(completeFilter));
				AssertEquals("Matched ES 3", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList4.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched DataGroupingCode", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList5.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched CodeType", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList6.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched StartDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList7.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched EndDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList8.PK).MatchesFilter(completeFilter));
			});
		}
	}
}

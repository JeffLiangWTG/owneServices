using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(PremisesLookupsImplementer))]
	sealed class PremisesLookupsImplementerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCustomsLocationList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

			var eunCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var spainCode = Core.Constants.CountryCodes.Spain;
			var italyCode = Core.Constants.CountryCodes.Italy;
			var grouping = helper.CreateNewOrGetExistingDataGrouping(eunCode);
			_ = helper.CreateNewOrGetExistingDataGrouping(spainCode, parent: grouping);
			_ = helper.CreateNewOrGetExistingDataGrouping(italyCode, parent: grouping);

			var locCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType;
			_ = helper.CreateNewOrGetExistingCusCodeType(locCode, "Locations");
			_ = helper.CreateNewOrGetExistingCusCodeType("AAA", "Invalid Type");
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
			CombineAssertions(() =>
			{
				using (MasterFiles.Business.GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
				{
					var locationsList = lookups.CustomsLocationList;
					AssertType<ZZRefCusCodeListCombinedCollection>("List Type", locationsList);
					AssertSame("Cached", locationsList, lookups.CustomsLocationList);

					var completeFilter = ((ZZRefCusCodeListCombinedCollection)locationsList).CompleteFilter;
					AssertEquals("Unmatched EUN", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList1.PK).MatchesFilter(completeFilter));
					AssertEquals("Matched ES 1", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList2.PK).MatchesFilter(completeFilter));
					AssertEquals("Matched ES 2", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList3.PK).MatchesFilter(completeFilter));
					AssertEquals("Matched ES 3", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList4.PK).MatchesFilter(completeFilter));
					AssertEquals("Unmatched DataGroupingCode", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList5.PK).MatchesFilter(completeFilter));
					AssertEquals("Unmatched CodeType", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList6.PK).MatchesFilter(completeFilter));
					AssertEquals("Unmatched StartDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList7.PK).MatchesFilter(completeFilter));
					AssertEquals("Unmatched EndDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList8.PK).MatchesFilter(completeFilter));
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = PremisesLookupsImplementer.Get(Factory);
		}
		PremisesLookupsImplementer lookups;
	}
}

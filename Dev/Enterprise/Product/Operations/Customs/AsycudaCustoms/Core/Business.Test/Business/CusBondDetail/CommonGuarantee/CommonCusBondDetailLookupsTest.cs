using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CommonCusBondDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBondTypeList()
		{
			var guarantee = Factory.New<CusBondDetail>();
			var lookups = new CommonCusBondDetailLookups(guarantee);
			var list = lookups.BondTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "CON, STB", list.CodesAsString);
				AssertSame("Cached", lookups.BondTypeList, list);
			});
		}

		public void TestGuaranteeCollection()
		{
			var valid1 = CreateCusGuaranteeHeader(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDate.Today.AddDays(-1), null);
			var valid2 = CreateCusGuaranteeHeader(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));
			var invalid1 = CreateCusGuaranteeHeader(Core.Constants.CountryCodes.China, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));
			var invalid2 = CreateCusGuaranteeHeader(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDate.Today.AddDays(-2), ZDate.Today.AddDays(-1));
			var invalid3 = CreateCusGuaranteeHeader(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDate.Today.AddDays(1), ZDate.Today.AddDays(2));

			var guarantee = Factory.New<CusBondDetail>();
			var lookups = new CommonCusBondDetailLookups(guarantee);
			var collection = lookups.GuaranteeCollection;
			CombineAssertions(() =>
			{
				AssertEquals("Empty End Date", true, valid1.MatchesFilter(collection.CompleteFilter));
				AssertEquals("Entered End Date", true, valid2.MatchesFilter(collection.CompleteFilter));
				AssertEquals("Wrong Country Code", false, invalid1.MatchesFilter(collection.CompleteFilter));
				AssertEquals("Past", false, invalid2.MatchesFilter(collection.CompleteFilter));
				AssertEquals("Future", false, invalid3.MatchesFilter(collection.CompleteFilter));
			});
		}

		CusGuaranteeHeader CreateCusGuaranteeHeader(string countryCode, ZDate start, ZDate? end)
		{
			var result = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			result.CPH_RN_NKCountryCode = countryCode;
			result.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			result.CPH_StartDate = start;
			if (end.HasValue)
			{
				result.CPH_EndDate = end.Value;
			}
			return result;
		}

		public void TestActivityCodeList()
		{
			var guarantee = Factory.New<CusBondDetail>();
			var lookups = new CommonCusBondDetailLookups(guarantee);
			var list = lookups.ActivityCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "CON, REL", list.CodesAsString);
				AssertSame("Cached", lookups.ActivityCodeList, list);
			});
		}

		public void TestStatusList()
		{
			var guarantee = Factory.New<CusBondDetail>();
			var lookups = new CommonCusBondDetailLookups(guarantee);
			var list = lookups.StatusList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "LIN, UNL", list.CodesAsString);
				AssertSame("Cached", lookups.StatusList, list);
			});
		}
	}
}

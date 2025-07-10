using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class CusESNctsHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSummaryTypeList()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			var esHeader = nctsHeader.ESNctsHeader;
			CombineAssertions(() =>
			{
				var list = esHeader.Lookups.SummaryTypeList;
				AssertType<SummaryTypeList>(list);
				AssertSame("Cached", list, esHeader.Lookups.SummaryTypeList);
				AssertEquals("Expected codes", "GG, SG, GP, SP", list.CodesAsString);
			});
		}
	}
}

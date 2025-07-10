using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CACusRulingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRulingTypeList()
		{
			var ruling = Factory.NewWithValidTestData<CACusRuling>();
			AssertEquals(9, ruling.Lookups.RulingTypeList.Count);
			Assert(ruling.Lookups.RulingTypeList.ContainsCode("2"));
			Assert(ruling.Lookups.RulingTypeList.ContainsCode("6"));
			Assert(ruling.Lookups.RulingTypeList.ContainsCode("R"));
			Assert(ruling.Lookups.RulingTypeList.ContainsCode("T"));
			Assert(ruling.Lookups.RulingTypeList.ContainsCode("W"));
			Assert(ruling.Lookups.RulingTypeList.ContainsCode("DD"));
			Assert(ruling.Lookups.RulingTypeList.ContainsCode("G"));
			Assert(ruling.Lookups.RulingTypeList.ContainsCode("SP"));
			Assert(ruling.Lookups.RulingTypeList.ContainsCode("SW"));
		}
	}
}

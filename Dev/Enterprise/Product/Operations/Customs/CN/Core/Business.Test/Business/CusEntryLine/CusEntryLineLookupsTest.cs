using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusEntryLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryLine()
		{
			var parent = Factory.New<CusEntryLine>();
			AssertEquals(parent.Lookups.EntryLine, parent);
		}
	}
}

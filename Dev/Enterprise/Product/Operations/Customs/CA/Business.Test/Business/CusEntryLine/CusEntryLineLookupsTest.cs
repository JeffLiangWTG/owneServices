using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusEntryLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryLine()
		{
			CusEntryLine parent = Factory.New<CusEntryLine>();
			AssertEquals(parent.Lookups.EntryLine, parent);
		}
	}
}

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class CusEntryLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryLine()
		{
			CusEntryLine parent = Factory.New<CusEntryLine>();
			AssertEquals(parent.Lookups.EntryLine, parent);
		}
	}
}

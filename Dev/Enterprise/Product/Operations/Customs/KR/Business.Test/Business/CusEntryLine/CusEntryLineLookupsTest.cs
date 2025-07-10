using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class CusEntryLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryLine()
		{
			var parent = Factory.New<CusEntryLine>();
			AssertEquals(parent.Lookups.EntryLine, parent);
		}
	}
}

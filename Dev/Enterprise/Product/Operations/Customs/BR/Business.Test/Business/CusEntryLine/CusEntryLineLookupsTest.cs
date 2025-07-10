using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class CusEntryLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryLine()
		{
			var entryLine = Factory.New<CusEntryLine>();
			AssertEquals(entryLine.Lookups.EntryLine, entryLine);
		}

		public void TestEntryLineStatusList()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var list = entryLine.Lookups.EntryLineStatusList;
			AssertSame(Factory.GetCachedValue<CustomsPostedStatusList>(), list);
			AssertContainsExactElementsInAnyOrder(new string[] { "ACC", "ACT", "DLT", "DPD", "UPD" }, list.GetAllCodes());
		}
	}
}

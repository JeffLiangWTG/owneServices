using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCH_EntryStatusList()
		{
			Factory.SetupEntryStatusList();
			var declaration = Factory.New<JobDeclaration>();
			var entry = Factory.New<CusEntryHeader>();
			var list = entry.Lookups.CH_EntryStatusList;
			AssertEquals("EntryList.Count = 0 if no declartion binded", 0, list.Count);
			entry.CH_JE = declaration.PK;
			list = entry.Lookups.CH_EntryStatusList;
			AssertEquals(3, list.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "ST1", "ST2", "ST3" }, list.GetAllCodes());
		}

		public void TestEntryHeader()
		{
			var parent = Factory.New<CusEntryHeader>();
			AssertEquals(parent.Lookups.EntryHeader, parent);
		}
	}
}

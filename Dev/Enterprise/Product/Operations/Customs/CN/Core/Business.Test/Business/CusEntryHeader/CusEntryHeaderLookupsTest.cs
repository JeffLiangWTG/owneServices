using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryHeader()
		{
			var parent = Factory.New<CusEntryHeader>();
			AssertEquals(parent.Lookups.EntryHeader, parent);
		}

		public void TestCH_MessageTypeList()
		{
			var parent = Factory.New<CusEntryHeader>();
			AssertType<EntryTypeList>(parent.Lookups.CH_MessageTypeList);
			AssertSame(parent.Lookups.CH_MessageTypeList, parent.Lookups.CH_MessageTypeList);
			Assert("Entry type list should not be translatable", parent.Lookups.CH_MessageTypeList is UntranslatableCodeDescriptionPairList);
		}

		public void TestMessageStatusList()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var testList = Factory.GetCachedValue<JobMessageStatusList>();
			AssertSame(testList, entryHeader.Lookups.MessageStatusList);
		}
	}
}

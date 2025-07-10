using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryHeader()
		{
			var parent = Factory.New<CusEntryHeader>();
			AssertEquals(parent.Lookups.EntryHeader, parent);
		}

		public void TestMessageStatusList()
		{
			var entry = Factory.New<CusEntryHeader>();
			AssertEquals(typeof(Common.BR.BRMessageStatusList), entry.Lookups.MessageStatusList.GetType());
		}

		public void TestMessageTypeList()
		{
			var entry = Factory.New<CusEntryHeader>();
			AssertEquals(typeof(MessageTypeList), entry.Lookups.CH_MessageTypeList.GetType());
		}

		public void TestRiskChannelList()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var list = entryHeader.Lookups.RiskChannelList;
			AssertSame(Factory.GetCachedValue<RiskChannelList>(), list);
			AssertEquals("1, 2, 3, 4, 5", list.CodesAsString);
		}

		public void TestCargoStatusList()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var list = entryHeader.Lookups.CargoStatusList;
			AssertSame(Factory.GetCachedValue<BRCargoStatusList>(), list);
			AssertEquals("1, 2, 3, 4, 5, 6, 7", list.CodesAsString);
		}

		public void TestAdministrativeStatusList()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var list = entryHeader.Lookups.AdministrativeStatusList;
			AssertSame(Factory.GetCachedValue<BRAdministrativeStatusList>(), list);
			AssertEquals("1, 2, 3, 4, 5", list.CodesAsString);
		}
	}
}

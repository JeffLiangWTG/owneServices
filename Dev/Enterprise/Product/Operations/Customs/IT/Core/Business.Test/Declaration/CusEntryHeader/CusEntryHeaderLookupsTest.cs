namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryHeaderLookupsTest : EU.Business.Declaration.Testing.CusEntryHeaderLookupsTest
{
	public new void TestMessageStatusList()
	{
		AssertType<ITMessageStatusList>(entryHeader.Lookups.MessageStatusList);
	}

	public void TestCH_EntryStatusList()
	{
		AssertType<ITEntryStatusList>(entryHeader.Lookups.CH_EntryStatusList);
	}

	public void TestCH_MessageTypeList()
	{
		AssertType<Customs.Business.DeclarationApplicationCodeList>(entryHeader.Lookups.CH_MessageTypeList);
	}

	public void TestCustomsChannelCodeList()
	{
		CombineAssertions(() =>
		{
			AssertType<CustomsChannelCodeList>("Type", entryHeader.Lookups.CustomsChannelCodeList);
			AssertSame("Cache", entryHeader.Lookups.CustomsChannelCodeList, entryHeader.Lookups.CustomsChannelCodeList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		entryHeader = Factory.New<CusEntryHeader>();
	}

	CusEntryHeader entryHeader;
}

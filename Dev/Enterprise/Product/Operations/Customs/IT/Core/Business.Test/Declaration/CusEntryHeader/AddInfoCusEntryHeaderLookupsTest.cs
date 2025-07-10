namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class AddInfoCusEntryHeaderLookupsTest : EU.Business.Declaration.Testing.AddInfoCusEntryHeaderLookupsTest
{
	public void TestAmendmentStatusList()
	{
		CombineAssertions(() =>
		{
			AssertType<AmendmentStatusList>("Type", addInfoLookups.AmendmentStatusList);
			AssertSame("Cache", addInfoLookups.AmendmentStatusList, addInfoLookups.AmendmentStatusList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		entryHeader = Factory.New<CusEntryHeader>();
		addInfoLookups = entryHeader.AddInfoLookups;
	}

	CusEntryHeader entryHeader;
	AddInfoCusEntryHeaderLookups addInfoLookups;
}

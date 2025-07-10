using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ETLineSpecialMentionGroupWrapperTest : SADSpecialMentionGroupCommonWrapperTest<ETLineSpecialMentionGroupWrapper>
{
	public void TestAdditionalInformation()
	{
		var etLineSpecialMentionGroupWrapper = new ETLineSpecialMentionGroupWrapper(entryLine);
		AssertNotNull("AdditionalInformation", etLineSpecialMentionGroupWrapper.AdditionalInformation);
		AssertType<ETLineSpecialMentionInfoAdditionalInformationWrapper>("AdditionalInformation type", etLineSpecialMentionGroupWrapper.AdditionalInformation);
	}

	protected override ETLineSpecialMentionGroupWrapper GetSpecialMentionGroupWrapper(CusEntryLine entryLine) => new ETLineSpecialMentionGroupWrapper(entryLine);
}

using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class ETLineSpecialMentionGroupWrapper : SADSpecialMentionGroupCommonWrapper, IETLineSpecialMentionGroup
{
	public ETLineSpecialMentionGroupWrapper(CusEntryLine entryLine)
		: base(entryLine)
	{
	}

	public IETLineSpecialMentionInfoAdditionalInformation AdditionalInformation => new ETLineSpecialMentionInfoAdditionalInformationWrapper();
}

using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class T2LPOUSRequestProofOperationInformationForT2LT2LFWrapper : T2LPOUSCommonProofOperationInformationForT2LT2LFWrapper, IT2LPOUSRequestProofOperationInformationForT2LT2LF
{
	public T2LPOUSRequestProofOperationInformationForT2LT2LFWrapper(CusEntryHeader cusEntryHeader) : base(cusEntryHeader, false)
	{
	}

	public ZString LRN => entryHeader.CH_BGMReference;
}

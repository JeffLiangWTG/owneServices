using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class T2LPOUSReceptionProofOperationInformationForT2LT2LFWrapper : T2LPOUSCommonProofOperationInformationForT2LT2LFWrapper, IT2LPOUSReceptionProofOperationInformationForT2LT2LF
{
	public T2LPOUSReceptionProofOperationInformationForT2LT2LFWrapper(CusEntryHeader cusEntryHeader) : base(cusEntryHeader, true)
	{
	}

	public ZString CodigoReferencia => entryHeader.MovementReferenceNumber;
}

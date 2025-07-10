using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IRequestT2LMessageDataProvider : IT2LPOUSRequestAndReceptionMessageDataProvider
{
	IT2LPOUSRequestProofOperationInformationForT2LT2LF ProofOperationInformationForT2LT2LF { get; }
}

public interface IT2LPOUSRequestProofOperationInformationForT2LT2LF : IT2LPOUSCommonProofOperationInformationForT2LT2LF
{
	ZString LRN { get; }
}

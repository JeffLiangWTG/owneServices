using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

public interface IReceptionT2LMessageDataProvider : IT2LPOUSRequestAndReceptionMessageDataProvider
{
	IT2LPOUSReceptionProofOperationInformationForT2LT2LF ProofOperationInformationForT2LT2LF { get; }
	ZString TipoAltaIndirecta { get; }
	IReadOnlyCollection<IAnnexDocCommon> EnvioDocumentos { get; }
}

public interface IT2LPOUSReceptionProofOperationInformationForT2LT2LF : IT2LPOUSCommonProofOperationInformationForT2LT2LF
{
	ZString CodigoReferencia { get; }
}

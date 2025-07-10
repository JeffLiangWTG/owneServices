using System;
using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT021ResponseMessageProcessor : BasePassarNctsMSGMessageProcessor<INT021ResponseDetail>
{
	public NT021ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"NT021 - Payload request goods declaration response";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarNctsPayloadRequestGoodsDeclarationResponse };

	protected override string MovementType => throw new NotImplementedException();

	protected override void ProcessResponseMessageCore(CHEDIMessage message, INT021ResponseDetail customsResponse, NctsHeader nctsHeader)
	{
		nctsHeader.EffectiveMessageStatus = CHLogicalStatusList.Codes.Accepted;
	}
}

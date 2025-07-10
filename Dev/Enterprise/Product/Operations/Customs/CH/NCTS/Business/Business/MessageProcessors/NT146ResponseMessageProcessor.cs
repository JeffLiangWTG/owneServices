using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NT146ResponseMessageProcessor : BasePassarNctsMSGMessageProcessor<INT146ResponseDetail>
{
	public NT146ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"NT146 - Response Information about non-arrived movement";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarNonArrivedTransitMovementInformation };

	protected override string MovementType => NctsMovementType.Codes.Departure;

	protected override BusinessObject FindLinkedObject(EDIMessage message, INT146ResponseDetail xmlObject) => FindLinkedObjectByCorrelationIdentifier(message, xmlObject);

	protected override void ProcessResponseMessageCore(CHEDIMessage message, INT146ResponseDetail customsResponse, NctsHeader nctsHeader)
	{
		nctsHeader.EffectiveMessageStatus = customsResponse.IsAccepted ? CHLogicalStatusList.Codes.Accepted : CHLogicalStatusList.Codes.Invalid;
	}
}

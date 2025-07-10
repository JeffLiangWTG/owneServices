using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class BordereauErrorResponseMessageProcessor : BaseInboundMessageProcessor<UniversalEventWrapper>
{
	public BordereauErrorResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Bordereau Error Response Message Processor";

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsEdec;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.BOR };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.Rejected, MessageSubTypeCodeList.Codes.XmlSchemaError, MessageSubTypeCodeList.Codes.RuleError };

	protected override UniversalEventWrapper DeserializeResponse(CHEDIMessage message) => message.UniversalEventData;

	protected override BusinessObject FindLinkedObject(EDIMessage message, UniversalEventWrapper xmlObject) => FindLinkedObjectByOutgoingSessionID(message);

	protected override void ProcessResponseMessage(CHEDIMessage message, UniversalEventWrapper customsResponse)
	{
	}
}

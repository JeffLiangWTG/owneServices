using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business;

public class CharteraOutputAcknowledgeMessageProcessor : BaseInboundMessageProcessor<UniversalEventWrapper>
{
	public CharteraOutputAcknowledgeMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Chartera Output Acknowledge Message Processor";

	protected override string ApplicationCodeCore => ApplicationCodes.CHCustomsCharteraOutput;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.REQ };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.Acknowledged };

	protected override UniversalEventWrapper DeserializeResponse(CHEDIMessage message) => message.UniversalEventData;

	protected override BusinessObject FindLinkedObject(EDIMessage message, UniversalEventWrapper xmlObject) => FindLinkedObjectByOutgoingSessionID(message);

	protected override void ProcessResponseMessage(CHEDIMessage message, UniversalEventWrapper customsResponse) { }
}

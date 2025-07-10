using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

sealed class SummaryProspectusResponseMessageProcessor : XmlIncomingMessageProcessor
{
	public SummaryProspectusResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override void ProcessResponse(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage, EDIMessage originalSentMessage)
	{
		receivedMessage.EM_MessageSubType = originalSentMessage.EM_MessageSubType;
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Summary Prospectus Response Message Processor";

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => [EDIMessageTypeList.Codes.SummaryProspectusRequest];
}

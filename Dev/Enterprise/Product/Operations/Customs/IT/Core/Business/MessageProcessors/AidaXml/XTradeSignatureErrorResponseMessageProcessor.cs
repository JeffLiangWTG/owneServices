using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using InterchangeTypes = Enterprise.Customs.IT.Business.MessageProcessorConstants.InterchangeTypes;

namespace Enterprise.Customs.IT.Business;

public sealed class XTradeSignatureErrorResponseMessageProcessor : XmlIncomingMessageProcessor
{
	public XTradeSignatureErrorResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"XTrade Signature Error Response Message Processor";

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { InterchangeTypes.Ucc6XTradeSignatureErrorType };

	protected override void ProcessResponse(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage, EDIMessage originalSentMessage)
	{
		if (originalSentMessage.EM_MessageType.ToString() is InterchangeTypes.Ucc6NewMessageType or InterchangeTypes.Ucc6CancellationType or InterchangeTypes.Ucc6AmendmentType)
		{
			entryAdapter.SetStatusAsFailedForTransmission();
		}
	}
}

using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public sealed class XtradeSignatureResponseMessageProcessor : XmlIncomingMessageProcessor
{
	public XtradeSignatureResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => [EDIMessageTypeList.Codes.SignatureResponse];

	protected override string MessageFriendlyNameCore => (NoResString)"XTrade Signature Message Processor";

	protected override bool ShouldSetReceivedMessageNumberEqualToOriginalMessageNumber => true;

	protected override void ProcessResponse(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage, EDIMessage originalSentMessage)
	{
		if (!originalSentMessage.EM_MessageType.In(allowedMessageTypesForProcessing))
		{
			return;
		}

		receivedMessage.EM_MessageSubType = originalSentMessage.EM_MessageSubType;
		receivedMessage.EM_ApplicationReference = originalSentMessage.EM_ApplicationReference;
	}

	readonly ImmutableArray<ZString> allowedMessageTypesForProcessing = new ZString[]
	{
		EDIMessageTypeList.Codes.NewDeclaration,
		EDIMessageTypeList.Codes.Amendment,
		EDIMessageTypeList.Codes.Cancellation,
	}.ToImmutableArray();
}

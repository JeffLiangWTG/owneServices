using System.Linq;
using System.Xml;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using static Enterprise.Customs.IT.Business.MessageProcessorConstants;

namespace Enterprise.Customs.IT.Business;

public abstract class XmlIncomingMessageProcessor : IncomingCustomsMessageProcessor<IXmlCustomsLinkedObjectAdapter>
{
	protected XmlIncomingMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected sealed override string ApplicationCodeCore => EDIMessage.ApplicationCodes.ITCustomsXTrade;

	protected sealed override IXmlCustomsLinkedObjectAdapter GetAdapter(ICustomsLinkedObjectAdapterProvider customsLinkedObjectAdapterProvider)
	{
		return customsLinkedObjectAdapterProvider.GetNewXmlCustomsLinkedObjectAdapter();
	}

	protected sealed override bool ShouldCloneMessage => false;

	protected sealed override void ProcessIncomingCustomsMessage(IncomingCustomsMessageProcessData processData)
	{
		var entryAdapter = GetOneAndOnlyEntryAdapterFromCollection(processData.EntryAdapters);
		var receivedMessage = processData.ReceivedMessage;
		var sentInterchange = processData.SentInterchange;

		try
		{
			var originalSentMessage = sentInterchange.ContainedMessages.Cast<EDIMessage>().Single();

			SetReceivedMessageNumberEqualToOriginalOneIfApplicable(originalSentMessage.EM_MessageNum, receivedMessage);
			CheckDuplicate(entryAdapter, processData.ReceivedInterchange);
			ProcessResponse(entryAdapter, receivedMessage, originalSentMessage);
		}
		catch (XmlException xmlException)
		{
			throw new UnableToInterpretInterchangeException(GetUnableToParseResponseMessage(receivedMessage.EM_MessageType), xmlException);
		}
	}

	void SetReceivedMessageNumberEqualToOriginalOneIfApplicable(ZString originalMessageNumber, EDIMessage receivedEDIMessage)
	{
		if (!ShouldSetReceivedMessageNumberEqualToOriginalMessageNumber)
		{
			return;
		}

		receivedEDIMessage.EM_MessageNum = originalMessageNumber;
	}

	void CheckDuplicate(IXmlCustomsLinkedObjectAdapter adapter, EDIInterchange receivedInterchange)
	{
		if (adapter.Messages.Any(m =>
			m.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Receive &&
			m.EM_MessageText == receivedInterchange.EI_BodyText &&
			m.EM_MessageType == InterchangeTypes.Ucc6ResponseMessageType))
		{
			throw new CustomsMessageProcessorException(Res.GetString("E5D5A0D6-4E9D-482E-9F72-278AAE43E38C", "Message discarded because duplicated in the declaration"));
		}
	}

	protected virtual bool ShouldSetReceivedMessageNumberEqualToOriginalMessageNumber => false;

	protected sealed override ZGuid GetReceivedTrackingId(EDIInterchange receivedInterchange) => receivedInterchange.EI_SessionGUID;

	protected abstract void ProcessResponse(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage, EDIMessage originalSentMessage);

	string GetUnableToParseResponseMessage(string messageType) => Res.GetString("2E8246A2-A083-4ED5-99E2-D5F3B033D009", "Unable to parse the response message [{0}].", messageType);
}

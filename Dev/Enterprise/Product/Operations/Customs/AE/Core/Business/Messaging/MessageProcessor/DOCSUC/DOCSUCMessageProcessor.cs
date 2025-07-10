using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.AE.Business;

sealed class DOCSUCMessageProcessor : BaseMessageProcessor<IDOCSUCDataProvider>
{
	protected override EDIMessage GetOutgoingMessage(EDIMessage message, IDOCSUCDataProvider dataProvider)
		=> message.Factory.GetOutboundMessage(dataProvider.OutgoingAccessReference);

	protected override void ProcessPreProcessOKMessageCore(EDIMessage message, IDOCSUCDataProvider dataProvider, LoggingInformation logger)
	{
		var factory = message.Factory;
		var outboundMessage = factory.GetOutboundMessage(dataProvider.OutgoingAccessReference);

		message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;

		if (outboundMessage.EM_LinkedObject is IMessageAttachee attachee)
		{
			attachee.EntryStatus = EDIMessageStatusList.Codes.Sent;
			attachee.MessageStatus = AEConstants.Messaging.MessageTypes.DOCSUC;
		}

		if (outboundMessage is { })
		{
			UpdateMessage(message, EDIMessageStatusList.Codes.Discarded, Res.GetString("9a479564-9249-4b97-8f41-32fc2f156b75", "Message is discarded. Cannot find corresponding sent message."));
		}
	}
}

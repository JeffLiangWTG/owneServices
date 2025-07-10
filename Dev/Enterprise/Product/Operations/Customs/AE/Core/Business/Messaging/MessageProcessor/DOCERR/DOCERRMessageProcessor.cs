using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.AE.Business;

sealed class DOCERRMessageProcessor : BaseMessageProcessor<IDOCERRDataProvider>
{
	protected override EDIMessage GetOutgoingMessage(EDIMessage message, IDOCERRDataProvider dataProvider)
		=> message.Factory.GetOutboundMessage(dataProvider.OutgoingAccessReference);

	protected override void ProcessPreProcessOKMessageCore(EDIMessage message, IDOCERRDataProvider dataProvider, LoggingInformation logger)
	{
		var factory = message.Factory;
		var outboundMessage = factory.GetOutboundMessage(dataProvider.OutgoingAccessReference);

		message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
		outboundMessage.EM_Status = EDIMessageStatusList.Codes.Failed;

		if (outboundMessage.EM_LinkedObject is IMessageAttachee attachee)
		{
			attachee.MessageStatus = AEConstants.Messaging.MessageTypes.DOCSUC;
			attachee.EntryStatus = EDIMessageStatusList.Codes.Error;
		}

		if (outboundMessage is { })
		{
			UpdateMessage(message, EDIMessageStatusList.Codes.Discarded, Res.GetString("92951543-5bf4-4150-b4c9-8267c3019a92", "Message is discarded. Cannot find corresponding sent message."));
		}
	}
}

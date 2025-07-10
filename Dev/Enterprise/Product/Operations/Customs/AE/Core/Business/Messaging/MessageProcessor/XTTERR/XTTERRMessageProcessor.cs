using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.AE.Business;

sealed class XTTERRMessageProcessor : BaseMessageProcessor<IXTTERRDataProvider>
{
	protected override EDIMessage GetOutgoingMessage(EDIMessage message, IXTTERRDataProvider dataProvider)
		=> message.Factory.GetOutboundMessage(dataProvider.OutgoingAccessReference);

	protected override void ProcessPreProcessOKMessageCore(EDIMessage message, IXTTERRDataProvider dataProvider, LoggingInformation logger)
	{
		message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;

		if (message.EM_LinkedObject is IMessageAttachee attachee)
		{
			attachee.MessageStatus = EDIMessageStatusList.Codes.Discarded;
			attachee.EntryStatus = EDIMessageStatusList.Codes.Error;
		}

		var factory = message.Factory;
		var outboundMessage = factory.GetOutboundMessage(dataProvider.OutgoingAccessReference);

		if (outboundMessage is { })
		{
			UpdateMessage(outboundMessage, EDIMessageStatusList.Codes.Failed, Res.GetString("db90c067-08b7-4bf6-bccc-81a1efad57da", "Message failed due to xT sending error."));
		}
	}
}

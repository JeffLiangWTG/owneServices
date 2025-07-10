namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class MessageStatusChangeMessageProcessorTest : CAUniversalEventMessageProcessorTest
	{
		protected override string UniversalEventFileName => "MessageStatusChange.xml";

		protected override string MessageInterpretationFileName => "MessageStatusChange.html";

		protected override string MessageTypeDescription => "Message: Status Change";

		protected override CAUniversalEventMessageProcessor GetMessageProcesser() => new MessageStatusChangeMessageProcessor(logger, UniversalEvent, Message, entryHeader);
	}
}

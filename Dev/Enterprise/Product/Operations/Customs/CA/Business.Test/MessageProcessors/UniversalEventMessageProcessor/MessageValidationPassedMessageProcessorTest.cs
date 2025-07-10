namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class MessageValidationPassedMessageProcessorTest : CAUniversalEventMessageProcessorTest
	{
		protected override string ExpectedEntryStatus => MessageValidationPassedMessageProcessor.IsOnFile;

		protected override string UniversalEventFileName => "MessageValidationPassed.xml";

		protected override string MessageInterpretationFileName => "MessageValidationPassed.html";

		protected override string MessageTypeDescription => "Message Validation Passed";

		protected override CAUniversalEventMessageProcessor GetMessageProcesser() => new MessageValidationPassedMessageProcessor(logger, UniversalEvent, Message, entryHeader);
	}
}

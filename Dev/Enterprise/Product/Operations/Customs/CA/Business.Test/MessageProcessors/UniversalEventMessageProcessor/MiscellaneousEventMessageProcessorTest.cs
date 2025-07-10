namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class MiscellaneousEventMessageProcessorTest : CAUniversalEventMessageProcessorTest
	{
		protected override string UniversalEventFileName => "MiscellaneousEvent.xml";

		protected override string MessageInterpretationFileName => "MiscellaneousEvent.html";

		protected override string MessageTypeDescription => "Miscellaneous Event";

		protected override CAUniversalEventMessageProcessor GetMessageProcesser() => new MiscellaneousEventMessageProcessor(logger, UniversalEvent, Message, entryHeader);
	}
}

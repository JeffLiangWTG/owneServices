namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class MessageAcceptedMessageProcessorTest : CAUniversalEventMessageProcessorTest
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMessageStatusShouldNotBeClearedUnlessCurrentlyAwaiting()
		{
			entryHeader.CH_Status = MessageStatusList.Codes.ClearOriginal;
			GetMessageProcesser().Process();
			AssertEquals("Message status should not be updated because it has already been cleared", MessageStatusList.Codes.ClearOriginal, entryHeader.CH_Status);
		}

		protected override string[] ExpectedStatusList => new[] { MessageStatusList.Codes.ClearOriginal, MessageStatusList.Codes.ClearChange,
			MessageStatusList.Codes.ClearReplace, MessageStatusList.Codes.ClearDelete };

		protected override string UniversalEventFileName => "MessageAccepted.xml";

		protected override string MessageInterpretationFileName => "MessageAccepted.html";

		protected override string MessageTypeDescription => "Message Accepted";

		protected override CAUniversalEventMessageProcessor GetMessageProcesser() => new MessageAcceptedMessageProcessor(logger, UniversalEvent, Message, entryHeader);
	}
}

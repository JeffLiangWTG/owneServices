using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.Testing;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class IMPMessageProcessorTest : EDIFACTMessageProcessorTest
	{
		public void TestProcessQueryMessage()
		{
			var message = Factory.New<QueryMessage>();
			AssertNoExceptionThrown(() => new IMPMessageProcessor(new()).ProcessMessage(message));
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
		}

		public void TestOverrides()
		{
			var impMessageProcessor = new IMPMessageProcessor(logger);
			AssertEquals("ApplicationCode", EDIMessage.ApplicationCodes.CAIMP, impMessageProcessor.ApplicationCode);
			AssertEquals("MessageFriendlyName", "CA Customs Import", impMessageProcessor.MessageFriendlyName);
		}

		public void TestCADMessageProcessor()
		{
			var message = Factory.New<CADMessage>();
			var impMessageProcessor = new MessageProcessorForTesting(logger);
			var processor = impMessageProcessor.GetMessageProcessor_Exposed(message);
			AssertType<CADResponseMessageProcessor>(processor);
		}

		class MessageProcessorForTesting : IMPMessageProcessor
		{
			public MessageProcessorForTesting(LoggingInformation logger) : base(logger)
			{
			}

			public CustomsMessageProcessor GetMessageProcessor_Exposed(EDIMessage ediMessage)
			{
				return base.GetMessageProcessor(ediMessage);
			}
		}
	}
}

using Enterprise.Customs.Business.MessageProcessors.Testing;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class QueryResponseMessageProcessorTest : EDIFACTMessageProcessorTest
	{
		public void TestProcessQueryMessage()
		{
			var message = Factory.New<QueryMessage>();
			AssertNoExceptionThrown(() => new QueryResponseMessageProcessor(new()).ProcessMessage(message));
			AssertEquals(EDIMessage.Status.Received, message.EM_Status);
		}
	}
}

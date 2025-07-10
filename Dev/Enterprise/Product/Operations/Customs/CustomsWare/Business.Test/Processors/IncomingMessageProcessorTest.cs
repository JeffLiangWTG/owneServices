using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	public class IncomingMessageProcessorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestList()
		{
			var messageProcessor = new IncomingMessageProcessorForTesting();
			NUnit.Framework.Assert.That(messageProcessor.MessageProcessor_Exposed.GetType(), Is.EqualTo(typeof(IncomingApplicationMessageProcessor)));
		}

		class IncomingMessageProcessorForTesting : IncomingMessageProcessor
		{
			internal ApplicationTypeMessageProcessor MessageProcessor_Exposed
			{
				get
				{
					return MessageProcessors[0];
				}
			}
		}
	}
}

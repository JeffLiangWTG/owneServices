using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;

namespace Enterprise.Messaging.MessageProcessors.Testing
{
	public class ApplicationTypeMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageFilter()
		{
			var message1 = AddNewMessage("TP1", "ST1");
			var message2 = AddNewMessage("TP2", "ST2");
			var message3 = AddNewMessage("TP3");
			var message4 = AddNewMessage("TP1");
			message4.EM_ApplicationCode = "OTH";
			Factory.Save();

			var processor = new TestApplicationTypeMessageProcessor(new LoggingInformation());
			processor.MessageTypesToIncludeExposed = System.Array.Empty<ZString>();
			processor.MessageTypesToExcludeExposed = System.Array.Empty<ZString>();
			processor.MessageSubTypesToIncludeExposed = System.Array.Empty<ZString>();
			var selectedMessages = Factory.Load<EDIMessage>(processor.MessageFilter);
			AssertEquals("3 messages selected", 3, selectedMessages.Length);
			Assert("Contains message1", selectedMessages.Contains(message1));
			Assert("Contains message2", selectedMessages.Contains(message2));
			Assert("Contains message3", selectedMessages.Contains(message3));
			Assert("Not Contains message4", !selectedMessages.Contains(message4));

			processor.MessageTypesToIncludeExposed = System.Array.Empty<ZString>();
			processor.MessageTypesToExcludeExposed = new ZString[] { "TP2", "TP3" };
			selectedMessages = Factory.Load<EDIMessage>(processor.MessageFilter);
			AssertEquals("1 message selected", 1, selectedMessages.Length);
			Assert("Contains message1", selectedMessages.Contains(message1));
			Assert("Not Contains message2", !selectedMessages.Contains(message2));
			Assert("Not Contains message3", !selectedMessages.Contains(message3));
			Assert("Not Contains message4", !selectedMessages.Contains(message4));

			processor.MessageTypesToIncludeExposed = new ZString[] { "TP1", "TP2" };
			processor.MessageTypesToExcludeExposed = System.Array.Empty<ZString>();
			selectedMessages = Factory.Load<EDIMessage>(processor.MessageFilter);
			AssertEquals("2 messages selected", 2, selectedMessages.Length);
			Assert("Contains message1", selectedMessages.Contains(message1));
			Assert("Contains message2", selectedMessages.Contains(message2));
			Assert("Not Contains message3", !selectedMessages.Contains(message3));
			Assert("Not Contains message4", !selectedMessages.Contains(message4));

			processor.MessageTypesToIncludeExposed = new ZString[] { "TP1", "TP2" };
			processor.MessageTypesToExcludeExposed = System.Array.Empty<ZString>();
			processor.MessageSubTypesToIncludeExposed = new ZString[] { "ST2" };
			selectedMessages = Factory.Load<EDIMessage>(processor.MessageFilter);
			AssertEquals("1 message selected", 1, selectedMessages.Length);
			Assert("Not Contains message1", !selectedMessages.Contains(message1));
			Assert("Contains message2", selectedMessages.Contains(message2));
			Assert("Not Contains message3", !selectedMessages.Contains(message3));
			Assert("Not Contains message4", !selectedMessages.Contains(message4));
		}

		EDIMessage AddNewMessage(ZString messageType, string messageSubType = null)
		{
			var message = EDIMessageTestFactory.New(Factory);
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationCode = "APP";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_IsActive = true;
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			return message;
		}
	}
}

using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRCRSMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageFilter()
		{
			var message1 = AddNewMessage(CMRMessage.CMRMessageTypes.CONTRL);
			var message2 = AddNewMessage(CMRMessage.CMRMessageTypes.IMD);
			var message3 = AddNewMessage(CMRMessage.CMRMessageTypes.CARST);

			var processor = new CMRCRSMessageProcessor(new LoggingInformation());
			var messageFilter = processor.MessageFilter;
			CombineAssertions(() =>
			{
				AssertEquals("Not Contains message1", false, message1.MatchesFilter(messageFilter));
				AssertEquals("Not Contains message2", false, message2.MatchesFilter(messageFilter));
				AssertEquals("Contains message3", true, message3.MatchesFilter(messageFilter));
			});
		}

		EDIMessage AddNewMessage(ZString messageType)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_IsActive = true;
			message.EM_MessageType = messageType;
			return message;
		}
	}
}

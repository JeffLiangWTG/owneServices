using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRAllMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageFilter()
		{
			var messages = new List<(EDIMessage Message, bool ShouldMatchFilter)>
			{
				(AddNewMessage(CMRMessage.CMRMessageTypes.CONTRL), false),
				(AddNewMessage(CMRMessage.CMRMessageTypes.IMD), true),
				(AddNewMessage(CMRMessage.CMRMessageTypes.CARST), false),
				(AddNewMessage(CMRMessage.CMRMessageTypes.SEI), false),
				(AddNewMessage(CMRMessage.CMRMessageTypes.UBMREQE), false),
				(AddNewMessage(CMRMessage.CMRMessageTypes.UBMREQR), false)
			};

			var processor = new CMRAllMessageProcessor(new LoggingInformation());
			var messageFilter = processor.MessageFilter;
			CombineAssertions(() =>
			{
				foreach (var pair in messages)
				{
					AssertEquals(pair.Message.EM_MessageType, pair.ShouldMatchFilter, pair.Message.MatchesFilter(messageFilter));
				}
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

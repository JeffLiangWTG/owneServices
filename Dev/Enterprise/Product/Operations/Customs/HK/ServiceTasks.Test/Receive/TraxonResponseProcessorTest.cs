using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.HK.ServiceTasks.Testing
{
	class TraxonResponseProcessorTest : DeclarationsAndShipmentsCreatedCancelledTestCase
	{
		public void TestMessageFriendlyName()
		{
			AssertEquals("ISAC Response", processor.MessageFriendlyName);
		}

		public void TestApplicationCode()
		{
			AssertEquals(EDIMessage.ApplicationCodes.Traxon, processor.ApplicationCode);
		}

		public void TestProcessMessage()
		{
			processor.ProcessMessage(message);
			AssertEquals("MessageStatus", EDIMessage.Status.Error, message.EM_Status);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var logger = new LoggingInformation();
			processor = new TraxonResponseProcessor(logger);
			message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.Traxon;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageText = "UNH+5946001+IEMFMA:D:95A:IA:IATA01+142520'NNNNNNNN + CUSEXP + AWB 125 - 53811940 UPDATED AT 1233 25MAR2004.REPLACE ?: 4, TOTAL ?: 4.++1++++ + 'UNT + 3 + 5946001'";
		}
		TraxonResponseProcessor processor;
		EDIMessage message;
	}
}

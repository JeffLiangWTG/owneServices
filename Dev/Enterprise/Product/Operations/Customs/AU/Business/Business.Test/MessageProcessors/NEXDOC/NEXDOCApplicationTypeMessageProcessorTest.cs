using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class NEXDOCApplicationTypeMessageProcessorTest : TestCaseWithFactory
	{
		public void TestApplicationCode()
		{
			AssertEquals(EDIMessage.ApplicationCodes.NEXDOCS, nexdocApplicationTypeMessageProcessor.ApplicationCode);
		}

		public void TestEmptyMessage()
		{
			message.EM_MessageText = ZString.Empty;
			nexdocApplicationTypeMessageProcessor.ProcessMessage(message);
			AssertEquals("The message is in error", EDIMessage.Status.Error, message.EM_Status);

			var logs = string.Join(" ", logger.UserLogStrings.Cast<string>());
			AssertContains("Could not process NEXDOC message #NEX_EM_001. Root element is missing.", logs);
		}

		public void TestUnrecognisedDocumentType()
		{
			message.EM_MessageText = @"<ns1:SomeUnknownResponse xmlns:ns1=""http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"" />";
			nexdocApplicationTypeMessageProcessor.ProcessMessage(message);
			AssertEquals("The message is in error", EDIMessage.Status.Error, message.EM_Status);

			var logs = string.Join(" ", logger.UserLogStrings.Cast<string>());
			AssertContains("Could not process NEXDOC message #NEX_EM_001. Unknown message type 'SomeUnknownResponse'.", logs);
		}

		public void TestGetMessageProcessor_ReadRexResponseProcessor()
		{
			var messageText = @"<ns1:ReadRexResponse xmlns:ns1=""http://agriculture.gov.au/nexdoc/ReadRexSoap_1.0"" />";
			AssertGetMessageProcessorReturnsExpectedType<ReadRexResponseProcessorRC5>(messageText);
		}

		public void TestGetMessageProcessor_RexAcknowledgeOwnershipResponseProcessor()
		{
			var messageText = @"<ns1:RexAcknowledgeOwnershipResponse xmlns:ns1=""http://agriculture.gov.au/nexdoc/RexOwnershipSoap_1.0"" />";
			AssertGetMessageProcessorReturnsExpectedType<RexAcknowledgeOwnershipResponseProcessor>(messageText);
		}

		public void TestGetMessageProcessor_RexForwardOwnershipResponseProcessor()
		{
			var messageText = @"<ns1:RexForwardOwnershipResponse xmlns:ns1=""http://agriculture.gov.au/nexdoc/ReadRexSoap_1.0"" />";
			AssertGetMessageProcessorReturnsExpectedType<RexForwardOwnershipResponseProcessor>(messageText);
		}

		public void TestGetMessageProcessor_RexTransferOwnershipResponseProcessor()
		{
			var messageText = @"<ns1:RexTransferOwnershipResponse xmlns:ns1=""http://agriculture.gov.au/nexdoc/ReadRexSoap_1.0"" />";
			AssertGetMessageProcessorReturnsExpectedType<RexTransferOwnershipResponseProcessor>(messageText);
		}

		NEXDOCApplicationTypeMessageProcessorForTest nexdocApplicationTypeMessageProcessor;
		EDIMessage message;
		LoggingInformation logger;

		protected override void SetUp()
		{
			base.SetUp();
			logger = new LoggingInformation();
			nexdocApplicationTypeMessageProcessor = new NEXDOCApplicationTypeMessageProcessorForTest(logger);
			message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.NEXDOCS;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageNum = "NEX_EM_001";
			message.EM_Status = EDIMessage.Status.Queued;
		}

		void AssertGetMessageProcessorReturnsExpectedType<T>(string messageText)
			where T : NEXDOCMessageProcessor
		{
			message.EM_MessageText = messageText;
			var processor = nexdocApplicationTypeMessageProcessor.GetMessageProcessor(message);
			AssertType<T>(processor);
			AssertEquals("The message is NOT in error", EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals("No errors reported", 0, logger.UserLogStrings.Count);
		}

		sealed class NEXDOCApplicationTypeMessageProcessorForTest : NEXDOCApplicationTypeMessageProcessor
		{
			public NEXDOCApplicationTypeMessageProcessorForTest(LoggingInformation logger)
				: base(logger)
			{
			}

			public new CustomsMessageProcessor GetMessageProcessor(EDIMessage message) => base.GetMessageProcessor(message);
		}
	}
}

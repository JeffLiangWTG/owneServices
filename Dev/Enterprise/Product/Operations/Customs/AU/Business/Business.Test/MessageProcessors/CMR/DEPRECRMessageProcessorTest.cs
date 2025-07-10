using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DEPRECRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestProcessDEPRECRRejectionResponse()
		{
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("DEPRECRRejectionMessage.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Status", Constants.CMRConsolStatus.Rejected, wrapper.GetCMRStatus(CMRMessage.CMRMessageTypes.DEPREC));
			AssertContains("Report", "Consol #: C00001308\r\n\r\nStatus: REJECTED\r\nStatus Description: The transaction has been rejected due to errors.  Please correct and re-send the message.\r\n\r\n\r\nErrors:\r\n\tThe mandatory field CDI is missing\r\n".Replace("\r\n", "<br>"), processor.SentReport.Body);
		}

		public void TestClearResponseOnJobDeclaration()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRECClear;
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearDEPRECOriginal.Code, declaration.JE_MessageStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestErrorResponseOnJobDeclaration()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRECErrors;
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.ErrorDEPRECOriginal.Code, declaration.JE_MessageStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestRejectedResponseOnJobDeclaration()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRECFails;
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.FailDEPRECOriginal.Code, declaration.JE_MessageStatus);
			AssertEquals("ErrorEmailCount", 1, processor.ErrorEmailSendCount);
		}

		public void TestAmendedResponseOnJobDeclaration()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRECReplaced;
			outgoingMessage2.EM_MessageSubType = "AMD";
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearDEPRECReplacement.Code, declaration.JE_MessageStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestAmendedErrorResponseOnJobDeclaration()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRECReplacedErrors;
			outgoingMessage2.EM_MessageSubType = "AMD";
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.ErrorDEPRECReplacement.Code, declaration.JE_MessageStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestWithdrawnResponseOnJobDeclaration()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRECWithdrawn;
			outgoingMessage2.EM_MessageSubType = "WDW";
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearDEPRECWithdrawal.Code, declaration.JE_MessageStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.DEPREC;

		protected override ZString GetExpectedMessageName() => "Depot Export Receival Notice Response (DEPRECR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001308";
			wrapper = new FreightConsolWrapper(consol);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001010";
			outgoingMessage1 = (CMRMessage)consol.Messages.AddNew(typeof(OutgoingEDIMessageTestHelper));
			outgoingMessage2 = (CMRMessage)declaration.Messages.AddNew(typeof(OutgoingEDIMessageTestHelper));
			outgoingMessage1.EM_MessageType = Enterprise.Customs.AU.Declaration.Business.CMRMessage.CMRMessageTypes.DEPREC;
			outgoingMessage2.EM_MessageType = Enterprise.Customs.AU.Declaration.Business.CMRMessage.CMRMessageTypes.DEPREC;
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			processor = new DEPRECRMessageProcessorTestHelper(logger);
		}

		protected override Type IncomingMessageType => typeof(CMRDEPRECRMessage);

		CMRMessage outgoingMessage1;
		CMRMessage outgoingMessage2;
		JobDeclaration declaration;
		ForwardingConsol consol;
		DEPRECRMessageProcessorTestHelper processor;
		FreightConsolWrapper wrapper;

		sealed class DEPRECRMessageProcessorTestHelper : DEPRECRMessageProcessor
		{
			public DEPRECRMessageProcessorTestHelper(LoggingInformation logger) : base(logger) { }
			protected override void SendReport(EmailDef email)
			{
				SentReport = email;
			}
			public EmailDef SentReport;
		}
	}
}

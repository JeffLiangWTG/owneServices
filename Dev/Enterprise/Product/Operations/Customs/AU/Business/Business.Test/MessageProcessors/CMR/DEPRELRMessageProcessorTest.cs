using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DEPRELRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestClearResponseOnJobDeclaration()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRELClear;
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearDEPRELOriginal.Code, declaration.JE_MessageStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestErrorResponseOnJobDeclaration()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRELErrors;
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.ErrorDEPRELOriginal.Code, declaration.JE_MessageStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestRejectedResponseOnJobDeclaration()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRELFails;
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.FailDEPRELOriginal.Code, declaration.JE_MessageStatus);
			AssertEquals("ErrorEmailCount", 1, processor.ErrorEmailSendCount);
		}

		public void TestAmendedResponseOnJobDeclaration()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRELReplaced;
			outgoingMessage2.EM_MessageSubType = "AMD";
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearDEPRELReplacement.Code, declaration.JE_MessageStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestAmendedErrorResponseOnJobDeclaration()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRELReplacedErrors;
			outgoingMessage2.EM_MessageSubType = "AMD";
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.ErrorDEPRELReplacement.Code, declaration.JE_MessageStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestWithdrawnResponseOnJobDeclaration()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.DEPRELWithdrawn;
			outgoingMessage2.EM_MessageSubType = "WDW";
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearDEPRELWithdrawal.Code, declaration.JE_MessageStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.DEPREL;

		protected override ZString GetExpectedMessageName() => "Depot Export Release Notice Response (DEPRELR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001308";
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001010";
			outgoingMessage1 = (CMRMessage)consol.Messages.AddNew(typeof(OutgoingEDIMessageTestHelper));
			outgoingMessage2 = (CMRMessage)declaration.Messages.AddNew(typeof(OutgoingEDIMessageTestHelper));
			outgoingMessage1.EM_MessageType = Enterprise.Customs.AU.Declaration.Business.CMRMessage.CMRMessageTypes.DEPREL;
			outgoingMessage2.EM_MessageType = Enterprise.Customs.AU.Declaration.Business.CMRMessage.CMRMessageTypes.DEPREL;
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			processor = new DEPRELRMessageProcessorTestHelper(logger);
		}

		protected override Type IncomingMessageType => typeof(CMRDEPRELRMessage);

		CMRMessage outgoingMessage1;
		CMRMessage outgoingMessage2;
		JobDeclaration declaration;
		ForwardingConsol consol;
		DEPRELRMessageProcessorTestHelper processor;

		sealed class DEPRELRMessageProcessorTestHelper : DEPRELRMessageProcessor
		{
			public DEPRELRMessageProcessorTestHelper(LoggingInformation logger) : base(logger) { }
			protected override void SendReport(EmailDef email)
			{
				SentReport = email;
			}
			public EmailDef SentReport;
		}
	}
}

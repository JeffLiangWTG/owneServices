using System;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class WARRELRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestClearResponse()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.WARRELClear;
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearWARRELOriginal.Code, declaration.JE_MessageStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestRejectedResponse()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.WARRELFails;
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.FailWARRELOriginal.Code, declaration.JE_MessageStatus);
			AssertEquals("ErrorEmailCount", 1, processor.ErrorEmailSendCount);
		}

		public void TestAmendedResponse()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.WARRELReplaced;
			outgoingMessage1.EM_MessageSubType = "AMD";
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearWARRELReplacement.Code, declaration.JE_MessageStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestWithdrawnResponse()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.WARRELWithdrawn;
			outgoingMessage1.EM_MessageSubType = "WDW";
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearWARRELWithdrawal.Code, declaration.JE_MessageStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001010";
			declaration.JE_MessageStatus = CustomsEntryStatus.AwaitingWARRELOriginal.Code;
			outgoingMessage1 = (CMRMessage)declaration.Messages.AddNew(typeof(OutgoingEDIMessageTestHelper));
			outgoingMessage1.EM_MessageType = Enterprise.Customs.AU.Declaration.Business.CMRMessage.CMRMessageTypes.WARREL;
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			processor = new WARRELRMessageProcessor(logger);
		}
		JobDeclaration declaration;
		WARRELRMessageProcessor processor;
		CMRMessage outgoingMessage1;

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.WARREL;

		protected override ZString GetExpectedMessageName() => "Warehouse Export Release Notice Response (WARRELR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => new WARRELRMessageProcessor(logger);

		protected override Type IncomingMessageType => typeof(CMRWARRELRMessage);
	}
}

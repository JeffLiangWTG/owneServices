using System;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class WARRETRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestClearResponse()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.WARRETClear;
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearWARRETOriginal.Code, declaration.JE_MessageStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestRejectedResponse()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.WARRETFails;
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.FailWARRETOriginal.Code, declaration.JE_MessageStatus);
			AssertEquals("ErrorEmailCount", 1, processor.ErrorEmailSendCount);
		}

		public void TestAmendedResponse()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRExportMessagesTestData.WARRETReplaced;
			outgoingMessage1.EM_MessageSubType = "AMD";
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearWARRETReplacement.Code, declaration.JE_MessageStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.WARRET;

		protected override ZString GetExpectedMessageName() => "Warehouse Export Return Notice Response (WARRETR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => new WARRETRMessageProcessor(logger);

		protected override Type IncomingMessageType => typeof(CMRWARRETRMessage);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001010";
			declaration.JE_MessageStatus = CustomsEntryStatus.AwaitingWARRETOriginal.Code;
			outgoingMessage1 = (CMRMessage)declaration.Messages.AddNew(typeof(OutgoingEDIMessageTestHelper));
			outgoingMessage1.EM_MessageType = Enterprise.Customs.AU.Declaration.Business.CMRMessage.CMRMessageTypes.WARRET;
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			processor = new WARRETRMessageProcessor(logger);
		}
		JobDeclaration declaration;
		WARRETRMessageProcessor processor;
		CMRMessage outgoingMessage1;
	}
}

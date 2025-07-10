using System;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DRWBCKRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestClearResponse()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRDrawbackMessageTestData.DRWBCKClear;
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.ClearOriginal.Code, declaration.JE_MessageStatus);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestRejectedResponse()
		{
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText = CMRDrawbackMessageTestData.DRWBCKFails;
			processor.ProcessMessage(incomingMessage);
			Factory.Save();
			AssertEquals("EntryStatus", CustomsEntryStatus.FailOriginal.Code, declaration.JE_MessageStatus);
			AssertEquals("ErrorEmailCount", 1, processor.ErrorEmailSendCount);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.DRWBCK;

		protected override ZString GetExpectedMessageName() => "Drawback Response (DRWBCKR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => new DRWBCKRMessageProcessor(logger);

		protected override Type IncomingMessageType => typeof(CMRDRWBCKRMessage);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001142";
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
			declaration.JE_MessageStatus = CustomsEntryStatus.AwaitingOriginal.Code;
			outgoingMessage1 = (CMRMessage)declaration.Messages.AddNew(typeof(OutgoingEDIMessageTestHelper));
			outgoingMessage1.EM_MessageType = Enterprise.Customs.AU.Declaration.Business.CMRMessage.CMRMessageTypes.DRWBCK;
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			processor = new DRWBCKRMessageProcessor(logger);
		}
		JobDeclaration declaration;
		DRWBCKRMessageProcessor processor;
		CMRMessage outgoingMessage1;
	}
}

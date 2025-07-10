using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BR.Business.Testing
{
	class DuimpCompleteConsultMessageSenderTest : TestCaseWithFactory
	{
		public void TestCanSendMessage()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_AuthorityVersion = "1";
			var sender = new DuimpCompleteConsultMessageSender(entryHeader);
			AssertNull(sender.CanSendMessage);

			entryHeader.CH_AuthorityVersion = "0";
			sender = new DuimpCompleteConsultMessageSender(entryHeader);
			AssertEquals("The selected entry cannot be consulted. The entry version is zero or empty.", sender.CanSendMessage);

			entryHeader.CH_AuthorityVersion = string.Empty;
			sender = new DuimpCompleteConsultMessageSender(entryHeader);
			AssertEquals("The selected entry cannot be consulted. The entry version is zero or empty.", sender.CanSendMessage);
		}

		public void TestSendMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("20BR0000274180");
			entryHeader.CH_AuthorityVersion = "1";
			var message = new DuimpCompleteConsultMessageSender(entryHeader).SendMessage();
			CombineAssertions(() =>
			{
				Assert("IsInDatabase should be FALSE", !entryHeader.IsInDatabase);
				AssertCompleteConsultMessage(entryHeader, message);
			});
		}

		public void TestSendMessageAndSave()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("20BR0000274180");
			entryHeader.CH_AuthorityVersion = "1";
			var messageSender = new DuimpCompleteConsultMessageSender(entryHeader);
			messageSender.SendMessageAndSave();
			AssertEquals("One EDIMessage added", 1, entryHeader.Messages.Count);
			var message = entryHeader.Messages[0];
			CombineAssertions(() =>
			{
				Assert("IsInDatabase should be TRUE", entryHeader.IsInDatabase);
				AssertCompleteConsultMessage(entryHeader, message);
			});

			entryHeader.CH_CEI_Instruction = ZGuid.NewZGuid();
			messageSender.SendMessageAndSave();
			AssertEquals("Cannot Save...", UnitTestUserNotification.Instance.PreviousMessages[0].Caption);
			AssertEquals("EDIMessage deleted after saving failed", 1, entryHeader.Messages.Count);
		}

		void AssertCompleteConsultMessage(CusEntryHeader entryHeader, EDIMessage message)
		{
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.BRCustoms, message.EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.CIH, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.CompleteConsult, message.EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", EDIInterchange.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_LinkedObject", entryHeader, message.EM_LinkedObject);
			AssertEquals("EM_ApplicationReference", "20BR0000274180|1", message.EM_ApplicationReference);
			AssertEquals("CH_Status", BRMessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
		}
	}
}

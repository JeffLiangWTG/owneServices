using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ExportCompleteConsultMessageSenderTest : TestCaseWithFactory
	{
		public void TestCanSendMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var messageSender = new ExportCompleteConsultMessageSender(entryHeader);
			AssertEquals("The selected Entry does not contain a Movement Reference Number.", messageSender.CanSendMessage);

			entryHeader.MovementReferenceNumberSetter("20BR0000274180");
			AssertEquals("No COM - Complete Consult Message has been detected.", messageSender.CanSendMessage);

			var messageCOM1 = Factory.New<EDIMessage>();
			messageCOM1.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			messageCOM1.EM_MessageType = MessageTypeList.Codes.CDE;
			messageCOM1.EM_MessageSubType = EDIMessageSubTypeList.Codes.CompleteConsult;
			messageCOM1.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			messageCOM1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddSeconds(-1);
			messageCOM1.EM_EI = Factory.New<EDIInterchange>().PK;
			messageCOM1.Interchange.EI_SessionGUID = ZGuid.NewZGuid();

			entryHeader.Messages.Add(messageCOM1);
			AssertEquals("No XER - Customs Error Message has been detected to re-trigger the request.", messageSender.CanSendMessage);

			var messageXER = Factory.New<EDIMessage>();
			messageXER.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			messageXER.EM_MessageType = MessageTypeList.Codes.XER;
			messageXER.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			messageXER.EM_SystemCreateTimeUtc = ZDateTime.Now.AddSeconds(-1);
			messageXER.EM_EI = Factory.New<EDIInterchange>().PK;
			messageXER.Interchange.EI_SessionGUID = messageCOM1.Interchange.EI_SessionGUID;

			entryHeader.Messages.Add(messageXER);
			AssertEquals("No XER - Customs Error Message has been detected to re-trigger the request.", messageSender.CanSendMessage);

			messageXER.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			messageCOM1.EM_MessageSubType = EDIMessageSubTypeList.Codes.Amend;
			AssertEquals("No COM - Complete Consult Message has been detected.", messageSender.CanSendMessage);

			messageCOM1.EM_MessageSubType = EDIMessageSubTypeList.Codes.CompleteConsult;
			AssertNull(messageSender.CanSendMessage);

			var messageCOM2 = Factory.New<EDIMessage>();
			messageCOM2.EM_ApplicationCode = EDIMessage.ApplicationCodes.BRCustoms;
			messageCOM2.EM_MessageType = MessageTypeList.Codes.CDE;
			messageCOM2.EM_MessageSubType = EDIMessageSubTypeList.Codes.CompleteConsult;
			messageCOM2.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			messageCOM2.EM_SystemCreateTimeUtc = ZDateTime.Now;
			messageCOM2.EM_EI = Factory.New<EDIInterchange>().PK;
			messageCOM2.Interchange.EI_SessionGUID = ZGuid.NewZGuid();

			entryHeader.Messages.Add(messageCOM2);
			AssertEquals("No XER - Customs Error Message has been detected to re-trigger the request.", messageSender.CanSendMessage);
		}

		public void TestSendMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var message = new ExportCompleteConsultMessageSender(entryHeader).SendMessage();
			CombineAssertions(() =>
			{
				Assert("IsInDatabase should be FALSE", !entryHeader.IsInDatabase);
				AssertCompleteConsultMessage(entryHeader, message);
			});
		}

		public void TestSendMessageAndSave()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var messageSender = new ExportCompleteConsultMessageSender(entryHeader);
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
			AssertEquals("EM_MessageType", MessageTypeList.Codes.CDE, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", EDIMessageSubTypeList.Codes.CompleteConsult, message.EM_MessageSubType);
			AssertEquals("EM_ReceiveTransmit", EDIInterchange.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_LinkedObject", entryHeader, message.EM_LinkedObject);
			AssertEquals("CH_Status", BRMessageStatusList.Codes.AwaitingResponse, entryHeader.CH_Status);
		}
	}
}

using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRRENMessageProcessorTest : XMLMessageTestHelper<GOVCBRRENMessageProcessorTest>
	{
		public void TestFunctionCodeCase08()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var incomingMessage = CreateMessageForTest("GOVCBRREN_CUS.xml");
			var entry = CreateEntryWithOutgoingMessageForExport();
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);
			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to", CustomsEntryStatusTypeList.Codes.SUP, entry.CH_EntryStatus);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("2020-06-08 08:46:33", email.Body);
			AssertContains("6N00220000052X", email.Body);
			AssertContains("통관보류", email.Body);
			AssertContains("적재지검사", email.Body);
			AssertContains("고발의뢰", email.Body);
			AssertContains("홍길동", email.Body);
			AssertContains("032-000-0000", email.Body);
		}

		public void TestFunctionCodeCase39()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var incomingMessage = CreateMessageForTest("GOVCBRREN_CUS_FuntionCode39.xml");
			var entry = CreateEntryWithOutgoingMessageForExport();
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);
			AssertEquals("entry status is updated correctly to", CustomsEntryStatusTypeList.Codes.CSP, entry.CH_EntryStatus);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("2020-06-08 08:46:33", email.Body);
			AssertContains("6N00220000052X", email.Body);
			AssertContains("통관보류해제", email.Body);
			AssertContains("적재지검사", email.Body);
			AssertContains("고발의뢰", email.Body);
			AssertContains("홍길동", email.Body);
			AssertContains("032-000-0000", email.Body);
		}

		public void TestWithConditionalElements()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var incomingMessage = CreateMessageForTest("GOVCBRREN_CUS_Empty.xml");
			var entry = CreateEntryWithOutgoingMessageForExport();
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertNoExceptionThrown("Null check should have been done", () => new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage));

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("2020-06-08 08:46:33", email.Body);
			AssertContains("6N00220000052X", email.Body);
			AssertContains("통관보류", email.Body);
			AssertContains("적재지검사", email.Body);
			AssertContains("고발의뢰", email.Body);
		}

		public void TestExportREN_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBRREN_CUS.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수출 통관보류통보]6N00220000052X 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestExportREN_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRREN_CUS.xml");
				CreateEntryForExport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 통관보류통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수출신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestExportREN_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRREN_CUS.xml");
				CreateEntryForExport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 통관보류통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "ORG";
			staff1.GS_LoginName = "Origin";
			staff1.GS_EmailAddress = "OriginalSender@wisetechglobal.com";

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "T1";
			staff1.GS_LoginName = "Test1";
			staff2.GS_EmailAddress = "ExportGroupTest@wisetechglobal.com";
			exportGroup = Factory.New<GlbGroup>();
			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = exportGroup.PK;
			link1.GK_GS = staff2.PK;

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "AG";
			staff3.GS_LoginName = "Agent";
			staff3.GS_EmailAddress = "CusAgent@wisetechglobal.com";
		}
		GlbGroup exportGroup;

		void CreateEntryForExport(bool setCusAgent)
		{
			var declaration = Factory.New<JobDeclaration>();
			if (setCusAgent)
			{
				declaration.JE_GS_NKCusAgent = "AG";
			}
			exportEntry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = exportEntry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "6N00220000052X";
			entryNumber.CE_EntryType = "EXP";
		}
		CusEntryHeader exportEntry;

		CusEntryHeader CreateEntryWithOutgoingMessageForExport()
		{
			if (exportEntry == null)
			{
				CreateEntryForExport(true);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._830;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkedObject = exportEntry;

			return exportEntry;
		}

		EDIMessage CreateMessageForTest(ZString fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBRRENMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._REN;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;

			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Incoming";
	}
}

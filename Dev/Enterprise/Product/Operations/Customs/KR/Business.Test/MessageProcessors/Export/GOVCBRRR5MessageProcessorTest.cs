using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRRR5MessageProcessorTest : XMLMessageTestHelper<GOVCBRRR5MessageProcessorTest>
	{
		public void TestStatusIsPRN()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var incomingMessage = CreateMessageForTest("GOVCBRRR5_PRN.xml");
			var entry = CreateEntryWithOutgoingMessageForExport();
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: Entry Status is Empty", ZString.Empty, entry.CH_Status);

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to PRN", "PRN", entry.CH_EntryStatus);

			entry.CustomsOfficers.Load();
			AssertEquals("entry.CustomsOfficers Data Create/Update", entry.CustomsOfficers.Count, 1);
			AssertEquals("entry.CustomsOfficess Create/Update is Success", entry.CustomsOfficers[0].CY_Data, "KCS011-홍길동");
			AssertEquals(incomingMessage.EM_MessageDateTime, exportEntry.CustomsOfficers[0].CY_Date);
			AssertEquals(CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer, exportEntry.CustomsOfficers[0].CY_Code);

			var stmAlogFilter = new ZQuery(StmALogSchema.SL_Parent, entry.PK);
			stmAlogFilter.AddToFilter(StmALogSchema.SL_Table, CusEntryHeader.Schema.TableName);
			var alog = Factory.LoadTop1<StmALog>(stmAlogFilter);
			AssertEquals("StmAlog SL_EventTime is updated", "20200810123012", alog.SL_EventTime.ToString("yyyyMMddHHmmss"));

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("2020-08-10 12:30:12", email.Body);
			AssertContains("6N002-20-000052X", email.Body);
			AssertContains("수시 반복적으로 수출하는 업체에 대한 검사생략", email.Body);

			AssertContains("결과통보", email.Body);
		}

		public void TestStatusIsCCGAndIssueDateTimeisNull()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var incomingMessage = CreateMessageForTest("GOVCBRRR5_CCG.xml");
			var entry = CreateEntryWithOutgoingMessageForExport();
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: Entry Status is Empty", ZString.Empty, entry.CH_Status);

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CCG", "CCG", entry.CH_EntryStatus);

			entry.CustomsOfficers.Load();
			AssertEquals("entry.CustomsOfficers Data Create/Update", entry.CustomsOfficers.Count, 1);
			AssertEquals("entry.CustomsOfficess Create/Update is Success", entry.CustomsOfficers[0].CY_Data, "KCS011-홍길동");
			AssertEquals(incomingMessage.EM_MessageDateTime, exportEntry.CustomsOfficers[0].CY_Date);
			AssertEquals(CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer, exportEntry.CustomsOfficers[0].CY_Code);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("2020-08-10 12:30:12", email.Body);
			AssertContains("담당자변경", email.Body);
		}

		public void TestStatusIsCCGAndAuthenticatorIDisNull()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var incomingMessage = CreateMessageForTest("GOVCBRRR5_CCG_AuthenticatorIDIsNull.xml");
			var entry = CreateEntryWithOutgoingMessageForExport();
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: Entry Status is Empty", ZString.Empty, entry.CH_Status);

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CCG", "CCG", entry.CH_EntryStatus);

			entry.CustomsOfficers.Load();
			AssertEquals("entry.CustomsOfficers Data Create/Update", entry.CustomsOfficers.Count, 1);
			AssertEquals("entry.CustomsOfficess Create/Update is Sucess When Declaration.Authenticator.ID is Empty", entry.CustomsOfficers[0].CY_Data, "홍길동");
			AssertEquals(incomingMessage.EM_MessageDateTime, exportEntry.CustomsOfficers[0].CY_Date);
			AssertEquals(CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer, exportEntry.CustomsOfficers[0].CY_Code);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("2020-08-10 12:30:12", email.Body);
			AssertContains("담당자변경", email.Body);
		}

		public void TestStatusIsBER()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var incomingMessage = CreateMessageForTest("GOVCBRRR5_BER.xml");
			var entry = CreateEntryWithOutgoingMessageForExport();
			Factory.Save();

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to BER", "BER", entry.CH_EntryStatus);
		}

		public void TestStatusIsPER()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var incomingMessage = CreateMessageForTest("GOVCBRRR5_PER.xml");
			var entry = CreateEntryWithOutgoingMessageForExport();
			Factory.Save();

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to PER", "PER", entry.CH_EntryStatus);
		}

		public void TestStatusIsEER()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var incomingMessage = CreateMessageForTest("GOVCBRRR5_EER.xml");
			var entry = CreateEntryWithOutgoingMessageForExport();
			Factory.Save();

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to EER", "EER", entry.CH_EntryStatus);
		}

		public void TestStatusIsLER()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var incomingMessage = CreateMessageForTest("GOVCBRRR5_LER.xml");
			var entry = CreateEntryWithOutgoingMessageForExport();
			Factory.Save();

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to LER", "LER", entry.CH_EntryStatus);
		}

		public void TestExportRR5_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBRRR5_PRN.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수출 처리결과통보]6N00220000052X 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestExportRR5_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRRR5_PRN.xml");
				CreateEntryForExport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 처리결과통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수출신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestExportRR5_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRRR5_PRN.xml");
				CreateEntryForExport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 처리결과통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
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

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBRRR5MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._RR5;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			Factory.Save();
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Incoming";
	}
}

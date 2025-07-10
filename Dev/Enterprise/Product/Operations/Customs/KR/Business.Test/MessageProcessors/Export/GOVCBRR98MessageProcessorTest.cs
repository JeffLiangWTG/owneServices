using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRR98MessageProcessorTest : XMLMessageTestHelper<GOVCBRR98MessageProcessorTest>
	{
		public void TestEM_MessageSubTypeIsOne()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var incomingMessage = CreateMessageForTest("GOVCBRR98_ONE.xml");
			var entry = CreateEntryWithOutgoingMessageForExport("6N00220000052X");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: Entry Status is Empty", ZString.Empty, entry.CH_EntryStatus);
			AssertEquals("EDI_Message.EM_MessageSubType is Empty", ZString.Empty, incomingMessage.EM_MessageSubType);

			AssertNotContains("GovernmentAgencyGoodsItem.Packing data is null", "<wco:Packaging><!--포장개수--><wco:QuantityQuantity>1</wco:QuantityQuantity></wco:Packaging>", incomingMessage.EM_MessageText);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			entry.Reload();
			AssertEquals("EDI_Message.EM_MessageSubType is updated to ONE", "ONE", incomingMessage.EM_MessageSubType);
			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertNullOrEmpty("entry status is not updated", entry.CH_EntryStatus);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient1 = email1.Recipients[0];

			AssertEquals("OriginalSender@wisetechglobal.com", recipient1.Email);
			AssertContains("수출신고서", email1.Body);
			AssertContains("2020-08-20 12:30:12", email1.Body);
			AssertContains("김레인", email1.Body);
			AssertContains("6N002-20-000052X", email1.Body);
			AssertContains("2020-07-20", email1.Body);
			AssertContains("2020-08-19", email1.Body);
			AssertContains("조이슨세이프티시스템스코리아", email1.Body);
			AssertContains("KNIT SOCKS", email1.Body);
			AssertContains("please see a Line 28, It is null but send email succesed.", "<td>포장개수</td><td>0</td>", email1.Body);
			AssertContains("83", email1.Body);
		}

		public void TestEM_MessageSubTypeisMUL()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var incomingMessage = CreateMessageForTest("GOVCBRR98_MUL.xml");
			var entry = CreateEntryWithOutgoingMessageForExport("6N00220000052X");
			var entry2 = CreateEntryWithOutgoingMessageForExport("6N00220000051X");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: Entry Status is Empty", ZString.Empty, entry.CH_EntryStatus);
			AssertEquals("PreCondition: Entry Status is Empty", ZString.Empty, entry2.CH_EntryStatus);
			AssertEquals("EDI_Message.EM_MessageSubType is Empty", ZString.Empty, incomingMessage.EM_MessageSubType);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			AssertEquals("EDI_Message.EM_MessageSubType is updated to MUL", "MUL", incomingMessage.EM_MessageSubType);

			entry.Reload();
			entry2.Reload();

			var messageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.KRCustoms);
			messageFilter.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			var messages = Factory.Load<EDIMessage>(messageFilter).Where(x => x.PK != incomingMessage.PK);
			AssertEquals("When EDI_Message.EM_MessageSubType is MUL, Create EDIM", 2, messages.Count());

			var message1 = messages.SingleOrDefault(x => x.EM_MessageText.Contains("6N00220000051X") && !x.EM_MessageText.Contains("6N00220000052X"));
			var message2 = messages.SingleOrDefault(x => x.EM_MessageText.Contains("6N00220000052X") && !x.EM_MessageText.Contains("6N00220000051X"));
			AssertNotNull("6N00220000051X", message1);
			AssertNotNull("6N00220000052X", message2);

			var fileReader = new TestFileReader(typeof(GOVCBRR98MessageProcessorTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBRR98_Result1.xml");
			AssertXMLEquals(testFile, message2.EM_MessageText.SubstringSafe(1));

			testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBRR98_Result2.xml");
			AssertXMLEquals(testFile, message1.EM_MessageText.SubstringSafe(1));

			AssertNullOrEmpty("entry status is updated", entry.CH_EntryStatus);
			AssertNullOrEmpty("entry status is updated", entry2.CH_EntryStatus);

			AssertEquals("entry is located", entry2.PK, message1.EM_LinkUniqueID);
			AssertEquals("entry is located", entry.PK, message2.EM_LinkUniqueID);

			AssertEquals(2, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Body.Contains("6N002-20-000051X"));
			var recipient1 = email1.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient1.Email);
			AssertContains("수출신고서", email1.Body);
			AssertContains("김레인", email1.Body);
			AssertContains("조이슨세이프티시스템스코리아", email1.Body);
			AssertContains("KNIT SOCKS", email1.Body);
			AssertContains("83", email1.Body);
			AssertContains("1", email1.Body);

			var email2 = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Body.Contains("6N002-20-000052X"));
			var recipient2 = email2.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient2.Email);
			AssertContains("수출신고서", email2.Body);
			AssertContains("2020-08-20 12:30:12", email2.Body);
			AssertContains("김레인", email2.Body);
			AssertContains("2020-08-22", email2.Body);
			AssertContains("2020-09-21", email2.Body);
			AssertContains("성진세미텍(주)", email2.Body);
			AssertContains("SHIELD", email2.Body);
			AssertContains("3", email2.Body);
			AssertContains("920", email2.Body);
		}

		public void TestExportR98_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBRR98_ONE.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수출 수리취소 예정통보]6N00220000052X 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestExportR98_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRR98_ONE.xml");
				CreateEntryForExport(false, "6N00220000052X");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 수리취소 예정통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수출신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestExportR98_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRR98_ONE.xml");
				CreateEntryForExport(true, "6N00220000052X");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 수리취소 예정통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
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

		void CreateEntryForExport(bool setCusAgent, ZString entryExportNum)
		{
			var declaration = Factory.New<JobDeclaration>();
			if (setCusAgent)
			{
				declaration.JE_GS_NKCusAgent = "AG";
			}
			exportEntry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = exportEntry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = entryExportNum;
			entryNumber.CE_EntryType = "EXP";
		}
		CusEntryHeader exportEntry;

		CusEntryHeader CreateEntryWithOutgoingMessageForExport(string entryExportNum)
		{
			CreateEntryForExport(true, entryExportNum);

			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._830;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkedObject = exportEntry;

			return exportEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBRR98MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R98;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Incoming";
	}
}

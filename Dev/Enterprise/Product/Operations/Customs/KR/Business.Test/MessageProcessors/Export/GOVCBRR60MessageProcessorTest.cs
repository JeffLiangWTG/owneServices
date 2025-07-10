using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRR60MessageProcessorTest : XMLMessageTestHelper<GOVCBRR60MessageProcessorTest>
	{
		public void TestR60()
		{
			CreateEntryWithOutgoingMessageForExport();
			var incomingMessage = CreateMessageForTest("GOVCBRR60_0.xml");
			Factory.Save();
			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", exportEntry.CH_EntryStatus.IsEmpty);

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to AMI", CustomsEntryStatusTypeList.Codes.AMI, exportEntry.CH_EntryStatus);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수출신고서", email.Body);
			AssertContains("2020-08-05 12:30:12", email.Body);
			AssertContains("6N00220000052X", email.Body);
			AssertContains("수출화주의 상호", email.Body);
			AssertContains("검증모델명", email.Body);
			AssertContains("자동보완 상세내역", email.Body);
			AssertNotContains("나머지 내역은 프로그램에서 확인 하십시오.", email.Body);

			AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"></tr></thead>" +
																   "<tr><th width=\"150\">항 목</th><th width=\"350\">내 용</th></tr>" +
																   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																   "<tr><td>보완통보일시</td><td>2020-08-05 12:30:12</td></tr>" +
																   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																   "<tr><td>수출화주</td><td>수출화주의 상호</td></tr></table>" +
																   "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead>" +
																   "<tr class=\"tableheadings\"></tr></thead>" +
																   "<tr><th colspan=\"4\">자동보완 내역</th></tr>" +
																   "<tr><td>검증 모델명</td><td>자동보완 내역</td></tr>" +
																   "<tr><td width=\"150\">검증모델명</td><td width=\"350\">자동보완 상세내역</td></tr></table>");
		}

		public void TestR60_11ReasonCodes()
		{
			CreateEntryWithOutgoingMessageForExport();
			var incomingMessage = CreateMessageForTest("GOVCBRR60_11Reasons.xml");
			Factory.Save();
			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", exportEntry.CH_EntryStatus.IsEmpty);

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("Test CargoDescription", email.Body);
			AssertContains("Test Description", email.Body);
			AssertContains("나머지 내역은 프로그램에서 확인 하십시오.", email.Body);

			AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"></tr></thead><tr><th width=\"150\">항 목</th><th width=\"350\">내 용</th></tr>" +
																	"<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																	"<tr><td>보완통보일시</td><td>2014-01-01 00:00:00</td></tr>" +
																	"<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																	"<tr><td>수출화주</td><td>수출화주의</td></tr>" +
																	"</table><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead>" +
																	"<tr class=\"tableheadings\"></tr></thead><tr><th colspan=\"4\">자동보완 내역</th></tr>" +
																	"<tr><td>검증 모델명</td><td>자동보완 내역</td></tr>" +
																	"<tr><td width=\"150\">검증모델명</td><td width=\"350\">1</td></tr>" +
																	"<tr><td width=\"150\">Test CargoDescription</td><td width=\"350\">Test Description</td></tr>" +
																	"<tr><td width=\"150\">Test CargoDescription</td><td width=\"350\">Test Description</td></tr>" +
																	"<tr><td width=\"150\">Test CargoDescription</td><td width=\"350\">Test Description</td></tr>" +
																	"<tr><td width=\"150\">Test CargoDescription</td><td width=\"350\">Test Description</td></tr>" +
																	"<tr><td width=\"150\">Test CargoDescription</td><td width=\"350\">Test Description</td></tr>" +
																	"<tr><td width=\"150\">Test CargoDescription</td><td width=\"350\">Test Description</td></tr>" +
																	"<tr><td width=\"150\">Test CargoDescription</td><td width=\"350\">Test Description</td></tr>" +
																	"<tr><td width=\"150\">Test CargoDescription</td><td width=\"350\">Test Description</td></tr>" +
																	"<tr><td width=\"150\">Test CargoDescription</td><td width=\"350\">Test Description</td></tr>" +
																	"<tr><td width=\"150\">Test CargoDescription</td><td width=\"350\">Test Description</td></tr>" +
																	"<tr><td width=\"150\">Test CargoDescription</td><td width=\"350\">Test Description</td></tr></table>");
			AssertNotContains(incomingMessage.EM_MessageInterpretation, "나머지 내역은 프로그램에서 확인 하십시오.");
		}

		public void TestEmptyGoodsShipment()
		{
			CreateEntryWithOutgoingMessageForExport();
			var incomingMessage = CreateMessageForTest("GOVCBRR60_WithoutGoodsShipment.xml");
			Factory.Save();
			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", exportEntry.CH_EntryStatus.IsEmpty);

			AssertNoExceptionThrown(() => new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage));

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("검증모델명", email.Body);
			AssertNotContains("자동보완 상세내역", email.Body);
			AssertNotContains("나머지 내역은 프로그램에서 확인 하십시오.", email.Body);

			AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"></tr></thead>" +
																   "<tr><th width=\"150\">항 목</th><th width=\"350\">내 용</th></tr>" +
																   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																   "<tr><td>보완통보일시</td><td>2020-08-05 12:30:12</td></tr>" +
																   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																   "<tr><td>수출화주</td><td>수출화주의 상호</td></tr></table>");
		}
		public void TestExportR60_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				var incomingMessage = CreateMessageForTest("GOVCBRR60_0.xml");
				Factory.Save();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수출 자동보완 통보]6N00220000052X 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);

				AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"></tr></thead>" +
													   "<tr><th width=\"150\">항 목</th><th width=\"350\">내 용</th></tr>" +
													   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
													   "<tr><td>보완통보일시</td><td>2020-08-05 12:30:12</td></tr>" +
													   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
													   "<tr><td>수출화주</td><td>수출화주의 상호</td></tr></table>" +
													   "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead>" +
													   "<tr class=\"tableheadings\"></tr></thead>" +
													   "<tr><th colspan=\"4\">자동보완 내역</th></tr>" +
													   "<tr><td>검증 모델명</td><td>자동보완 내역</td></tr>" +
													   "<tr><td width=\"150\">검증모델명</td><td width=\"350\">자동보완 상세내역</td></tr></table>");
			}
		}

		public void TestExportR60_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBRR60_0.xml");
				CreateEntryForExport(false);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 자동보완 통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수출신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);

				AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"></tr></thead>" +
													   "<tr><th width=\"150\">항 목</th><th width=\"350\">내 용</th></tr>" +
													   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
													   "<tr><td>보완통보일시</td><td>2020-08-05 12:30:12</td></tr>" +
													   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
													   "<tr><td>수출화주</td><td>수출화주의 상호</td></tr></table>" +
													   "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead>" +
													   "<tr class=\"tableheadings\"></tr></thead>" +
													   "<tr><th colspan=\"4\">자동보완 내역</th></tr>" +
													   "<tr><td>검증 모델명</td><td>자동보완 내역</td></tr>" +
													   "<tr><td width=\"150\">검증모델명</td><td width=\"350\">자동보완 상세내역</td></tr></table>");
			}
		}

		public void TestExportR60_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBRR60_0.xml");
				CreateEntryForExport(true);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 자동보완 통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);

				AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\"></tr></thead>" +
																	   "<tr><th width=\"150\">항 목</th><th width=\"350\">내 용</th></tr>" +
																	   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																	   "<tr><td>보완통보일시</td><td>2020-08-05 12:30:12</td></tr>" +
																	   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																	   "<tr><td>수출화주</td><td>수출화주의 상호</td></tr></table>" +
																	   "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead>" +
																	   "<tr class=\"tableheadings\"></tr></thead>" +
																	   "<tr><th colspan=\"4\">자동보완 내역</th></tr>" +
																	   "<tr><td>검증 모델명</td><td>자동보완 내역</td></tr>" +
																	   "<tr><td width=\"150\">검증모델명</td><td width=\"350\">자동보완 상세내역</td></tr></table>");
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
			Factory.Save();
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
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = exportEntry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
			Factory.Save();
		}
		CusEntryHeader exportEntry;

		void CreateEntryWithOutgoingMessageForExport()
		{
			if (exportEntry == null)
			{
				CreateEntryForExport(true);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._830;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = exportEntry.PK;
			outgoingMessage.EM_LinkedObject = exportEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBRR60MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R60;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Incoming";
	}
}

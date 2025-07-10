using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRR96MessageProcessorTest : XMLMessageTestHelper<GOVCBRR96MessageProcessorTest>
	{
		public void TestR96()
		{
			CreateEntryWithOutgoingMessageForExport();
			var incomingMessage = CreateMessageForTest("GOVCBRR96_CUS.xml");
			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			Factory.Save();

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);
			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to MFI", "MFI", exportEntry.CH_EntryStatus);
			TestEmailIsSent();
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("FABRIC FASHION MASK", email.Body);
			AssertContains("FABRIC FASHION MASK 900", email.Body);
			AssertContains("분류의견을 기재", email.Body);
			AssertContains("시료반환여부을 기재", email.Body);
			AssertContains("기타사항을 기재", email.Body);
			AssertContains("신고인 상호", email.Body);
			AssertContains("수출자 화주 상호", email.Body);
			AssertContains("6307.90-9000", email.Body);

			AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																   "<th>항 목</th><th>내 용</th></tr></thead>" +
																   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																   "<tr><td>안내일자</td><td>2020-08-11</td></tr>" +
																   "<tr><td>신고품명</td><td>FABRIC FASHION MASK</td></tr>" +
																   "<tr><td>모델규격</td><td>FABRIC FASHION MASK 900</td></tr>" +
																   "<tr><td>분류의견</td><td>분류의견을 기재</td></tr>" +
																   "<tr><td>참고사항1(시료반환여부)</td><td>시료반환여부을 기재</td></tr>" +
																   "<tr><td>참고사항2(기타)</td><td>기타사항을 기재</td></tr>" +
																   "<tr><td>신고인 상호</td><td>신고인 상호</td></tr>" +
																   "<tr><td>화주 상호</td><td>수출자 화주 상호</td></tr>" +
																   "<tr><td>분석회보 문서번호</td><td>9-20-12345</td></tr>" +
																   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																   "<tr><td>란번호</td><td>1</td></tr>" +
																   "<tr><td>규격번호</td><td>1</td></tr>" +
																   "<tr><td>결정세번</td><td>6307.90-9000</td></tr>" +
																   "<tr><td>신고세번</td><td>6307.90-9000</td></tr></table>");
		}
		public void Test_ElementIsEmpty()
		{
			CreateEntryWithOutgoingMessageForExport();
			var incomingMessage = CreateMessageForTest("GOVCBRR96_Empty.xml");
			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			Factory.Save();

			AssertNoExceptionThrown("When no Response.Declaration.GoodShipment, Exporter, Submitter is there, system should still proceed successfully", () =>
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage));
			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to MFI", "MFI", exportEntry.CH_EntryStatus);
			TestEmailIsSent();

			AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
													   "<th>항 목</th><th>내 용</th></tr></thead>" +
													   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
													   "<tr><td>안내일자</td><td>2020-08-11</td></tr>" +
													   "<tr><td>신고품명</td><td>&nbsp;</td></tr>" +
													   "<tr><td>모델규격</td><td>&nbsp;</td></tr>" +
													   "<tr><td>분류의견</td><td>&nbsp;</td></tr>" +
													   "<tr><td>참고사항1(시료반환여부)</td><td>&nbsp;</td></tr>" +
													   "<tr><td>참고사항2(기타)</td><td>&nbsp;</td></tr>" +
													   "<tr><td>신고인 상호</td><td>&nbsp;</td></tr>" +
													   "<tr><td>화주 상호</td><td>&nbsp;</td></tr>" +
													   "<tr><td>분석회보 문서번호</td><td>9-20-12345</td></tr>" +
													   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
													   "<tr><td>란번호</td><td>1</td></tr>" +
													   "<tr><td>규격번호</td><td>1</td></tr>" +
													   "<tr><td>결정세번</td><td>&nbsp;</td></tr>" +
													   "<tr><td>신고세번</td><td>&nbsp;</td></tr></table>");
		}

		void TestEmailIsSent()
		{
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("2020-08-11", email.Body);
			AssertContains("9-20-12345", email.Body);
			AssertContains("6N002-20-000052X", email.Body);
			AssertContains("001", email.Body);
			AssertContains("01", email.Body);
		}

		public void TestExportR96_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				var incomingMessage = CreateMessageForTest("GOVCBRR96_CUS.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수출 분석결과통보]6N00220000052X 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);

				AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
													   "<th>항 목</th><th>내 용</th></tr></thead>" +
													   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
													   "<tr><td>안내일자</td><td>2020-08-11</td></tr>" +
													   "<tr><td>신고품명</td><td>FABRIC FASHION MASK</td></tr>" +
													   "<tr><td>모델규격</td><td>FABRIC FASHION MASK 900</td></tr>" +
													   "<tr><td>분류의견</td><td>분류의견을 기재</td></tr>" +
													   "<tr><td>참고사항1(시료반환여부)</td><td>시료반환여부을 기재</td></tr>" +
													   "<tr><td>참고사항2(기타)</td><td>기타사항을 기재</td></tr>" +
													   "<tr><td>신고인 상호</td><td>신고인 상호</td></tr>" +
													   "<tr><td>화주 상호</td><td>수출자 화주 상호</td></tr>" +
													   "<tr><td>분석회보 문서번호</td><td>9-20-12345</td></tr>" +
													   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
													   "<tr><td>란번호</td><td>1</td></tr>" +
													   "<tr><td>규격번호</td><td>1</td></tr>" +
													   "<tr><td>결정세번</td><td>6307.90-9000</td></tr>" +
													   "<tr><td>신고세번</td><td>6307.90-9000</td></tr></table>");
			}
		}

		public void TestExportR96_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBRR96_CUS.xml");
				CreateEntryForExport(false);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 분석결과통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수출신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);

				AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
													   "<th>항 목</th><th>내 용</th></tr></thead>" +
													   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
													   "<tr><td>안내일자</td><td>2020-08-11</td></tr>" +
													   "<tr><td>신고품명</td><td>FABRIC FASHION MASK</td></tr>" +
													   "<tr><td>모델규격</td><td>FABRIC FASHION MASK 900</td></tr>" +
													   "<tr><td>분류의견</td><td>분류의견을 기재</td></tr>" +
													   "<tr><td>참고사항1(시료반환여부)</td><td>시료반환여부을 기재</td></tr>" +
													   "<tr><td>참고사항2(기타)</td><td>기타사항을 기재</td></tr>" +
													   "<tr><td>신고인 상호</td><td>신고인 상호</td></tr>" +
													   "<tr><td>화주 상호</td><td>수출자 화주 상호</td></tr>" +
													   "<tr><td>분석회보 문서번호</td><td>9-20-12345</td></tr>" +
													   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
													   "<tr><td>란번호</td><td>1</td></tr>" +
													   "<tr><td>규격번호</td><td>1</td></tr>" +
													   "<tr><td>결정세번</td><td>6307.90-9000</td></tr>" +
													   "<tr><td>신고세번</td><td>6307.90-9000</td></tr></table>");
			}
		}

		public void TestExportR96_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBRR96_CUS.xml");
				CreateEntryForExport(true);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 분석결과통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);

				AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
													   "<th>항 목</th><th>내 용</th></tr></thead>" +
													   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
													   "<tr><td>안내일자</td><td>2020-08-11</td></tr>" +
													   "<tr><td>신고품명</td><td>FABRIC FASHION MASK</td></tr>" +
													   "<tr><td>모델규격</td><td>FABRIC FASHION MASK 900</td></tr>" +
													   "<tr><td>분류의견</td><td>분류의견을 기재</td></tr>" +
													   "<tr><td>참고사항1(시료반환여부)</td><td>시료반환여부을 기재</td></tr>" +
													   "<tr><td>참고사항2(기타)</td><td>기타사항을 기재</td></tr>" +
													   "<tr><td>신고인 상호</td><td>신고인 상호</td></tr>" +
													   "<tr><td>화주 상호</td><td>수출자 화주 상호</td></tr>" +
													   "<tr><td>분석회보 문서번호</td><td>9-20-12345</td></tr>" +
													   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
													   "<tr><td>란번호</td><td>1</td></tr>" +
													   "<tr><td>규격번호</td><td>1</td></tr>" +
													   "<tr><td>결정세번</td><td>6307.90-9000</td></tr>" +
													   "<tr><td>신고세번</td><td>6307.90-9000</td></tr></table>");
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
			var fileReader = new TestFileReader(typeof(GOVCBRR96MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R96;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Incoming";
	}
}

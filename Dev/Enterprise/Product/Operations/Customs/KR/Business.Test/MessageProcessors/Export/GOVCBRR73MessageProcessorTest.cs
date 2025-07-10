using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRR73MessageProcessorTest : XMLMessageTestHelper<GOVCBRR73MessageProcessorTest>
	{
		public void TestR73()
		{
			CreateEntryWithOutgoingMessageForExport();
			var incomingMessage = CreateMessageForTest("GOVCBRR73_0.xml");

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", exportEntry.CH_EntryStatus.IsEmpty);
			Factory.Save();
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to NDC", CustomsEntryStatusTypeList.Codes.NDC, exportEntry.CH_EntryStatus);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("P", email.Body);
			AssertContains("수출신고서", email.Body);
			AssertContains("2014-08-06 12:30:12", email.Body);
			AssertContains("6N00220000052X", email.Body);
			AssertContains("서류제출", email.Body);
			AssertContains("기타서류제출", email.Body);
			AssertContains("[KCS011] 홍길동", email.Body);

			exportEntry.CustomsOfficers.Load();
			AssertEquals(exportEntry.CustomsOfficers.Count, 1);
			AssertEquals("CustomsOfficerID and CustomsOfficerName is KCS011-홍길동", "KCS011-홍길동", exportEntry.CustomsOfficers[0].CY_Data);
			AssertEquals(incomingMessage.EM_MessageDateTime, exportEntry.CustomsOfficers[0].CY_Date);
			AssertEquals("RCO", exportEntry.CustomsOfficers[0].CY_Code);

			AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																   "<th>항 목</th><th>내 용</th></tr></thead>" +
																   "<tr><td>선별결과구분</td><td>P</td></tr>" +
																   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																   "<tr><td>선별결과 통보일시</td><td>2014-08-06 12:30:12</td></tr>" +
																   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																   "<tr><td>선별결과</td><td>서류제출</td></tr>" +
																   "<tr><td>서류제출 내용</td><td>기타서류제출</td></tr>" +
																   "<tr><td>세관 담당자</td><td>[KCS011] 홍길동</td></tr></table>");
		}

		public void TestEmptyCustomsOfficer()
		{
			CreateEntryWithOutgoingMessageForExport();
			var incomingMessage = CreateMessageForTest("GOVCBRR73_WithoutVer.xml");

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", exportEntry.CH_EntryStatus.IsEmpty);
			Factory.Save();
			AssertNoExceptionThrown(() => new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage));
			AssertEquals("entry status is updated correctly to CLF", CustomsEntryStatusTypeList.Codes.CLF, exportEntry.CH_EntryStatus);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("화면심사", email.Body);
			AssertNotContains("기타서류제출", email.Body);
			AssertNotContains("[]", email.Body);

			AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																   "<th>항 목</th><th>내 용</th></tr></thead>" +
																   "<tr><td>선별결과구분</td><td>S</td></tr>" +
																   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																   "<tr><td>선별결과 통보일시</td><td>2014-08-06 12:30:12</td></tr>" +
																   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																   "<tr><td>선별결과</td><td>화면심사</td></tr>" +
																   "<tr><td>서류제출 내용</td><td>&nbsp;</td></tr>" +
																   "<tr><td>세관 담당자</td><td>&nbsp;</td></tr></table>");
			exportEntry.CustomsOfficers.Load();
			AssertEquals(exportEntry.CustomsOfficers.Count, 0);
		}

		public void TestNED()
		{
			CreateEntryWithOutgoingMessageForExport();
			var incomingMessage = CreateMessageForTest("GOVCBRR73_NED.xml");

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", exportEntry.CH_EntryStatus.IsEmpty);
			Factory.Save();
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry status is updated correctly to NED", CustomsEntryStatusTypeList.Codes.NED, exportEntry.CH_EntryStatus);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("전자서류제출", email.Body);

			AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																   "<th>항 목</th><th>내 용</th></tr></thead>" +
																   "<tr><td>선별결과구분</td><td>P</td></tr>" +
																   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																   "<tr><td>선별결과 통보일시</td><td>2014-08-06 12:30:12</td></tr>" +
																   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																   "<tr><td>선별결과</td><td>전자서류제출</td></tr>" +
																   "<tr><td>서류제출 내용</td><td>전자서류</td></tr>" +
																   "<tr><td>세관 담당자</td><td>[KCS011] 홍길동</td></tr></table>");
		}

		public void TestBAE()
		{
			CreateEntryWithOutgoingMessageForExport();
			var incomingMessage = CreateMessageForTest("GOVCBRR73_BAE.xml");

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", exportEntry.CH_EntryStatus.IsEmpty);
			Factory.Save();
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry status is updated correctly to BAE", CustomsEntryStatusTypeList.Codes.BAE, exportEntry.CH_EntryStatus);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("신고지검사", email.Body);

			AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																   "<th>항 목</th><th>내 용</th></tr></thead>" +
																   "<tr><td>선별결과구분</td><td>B</td></tr>" +
																   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																   "<tr><td>선별결과 통보일시</td><td>2014-08-06 12:30:12</td></tr>" +
																   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																   "<tr><td>선별결과</td><td>신고지검사</td></tr>" +
																   "<tr><td>서류제출 내용</td><td>기타서류제출</td></tr>" +
																   "<tr><td>세관 담당자</td><td>[KCS011] 홍길동</td></tr></table>");
		}

		public void TestLAE()
		{
			CreateEntryWithOutgoingMessageForExport();
			var incomingMessage = CreateMessageForTest("GOVCBRR73_LAE.xml");
			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", exportEntry.CH_EntryStatus.IsEmpty);
			Factory.Save();
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry status is updated correctly to LAE", CustomsEntryStatusTypeList.Codes.LAE, exportEntry.CH_EntryStatus);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("적재지검사", email.Body);

			AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																   "<th>항 목</th><th>내 용</th></tr></thead>" +
																   "<tr><td>선별결과구분</td><td>L</td></tr>" +
																   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																   "<tr><td>선별결과 통보일시</td><td>2014-08-06 12:30:12</td></tr>" +
																   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																   "<tr><td>선별결과</td><td>적재지검사</td></tr>" +
																   "<tr><td>서류제출 내용</td><td>기타서류제출</td></tr>" +
																   "<tr><td>세관 담당자</td><td>[KCS011] 홍길동</td></tr></table>");
		}
		public void TestExportR73_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				var incomingMessage = CreateMessageForTest("GOVCBRR73_0.xml");
				Factory.Save();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수출 선별결과 통보]6N00220000052X 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);

				AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																   "<th>항 목</th><th>내 용</th></tr></thead>" +
																   "<tr><td>선별결과구분</td><td>P</td></tr>" +
																   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																   "<tr><td>선별결과 통보일시</td><td>2014-08-06 12:30:12</td></tr>" +
																   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																   "<tr><td>선별결과</td><td>&nbsp;</td></tr>" +
																   "<tr><td>서류제출 내용</td><td>기타서류제출</td></tr>" +
																   "<tr><td>세관 담당자</td><td>[KCS011] 홍길동</td></tr></table>");
			}
		}

		public void TestExportR73_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBRR73_0.xml");
				CreateEntryForExport(false);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 선별결과 통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수출신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);

				AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																   "<th>항 목</th><th>내 용</th></tr></thead>" +
																   "<tr><td>선별결과구분</td><td>P</td></tr>" +
																   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																   "<tr><td>선별결과 통보일시</td><td>2014-08-06 12:30:12</td></tr>" +
																   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																   "<tr><td>선별결과</td><td>서류제출</td></tr>" +
																   "<tr><td>서류제출 내용</td><td>기타서류제출</td></tr>" +
																   "<tr><td>세관 담당자</td><td>[KCS011] 홍길동</td></tr></table>");
			}
		}

		public void TestExportR73_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBRR73_0.xml");
				CreateEntryForExport(true);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 선별결과 통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);

				AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																   "<th>항 목</th><th>내 용</th></tr></thead>" +
																   "<tr><td>선별결과구분</td><td>P</td></tr>" +
																   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																   "<tr><td>선별결과 통보일시</td><td>2014-08-06 12:30:12</td></tr>" +
																   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																   "<tr><td>선별결과</td><td>서류제출</td></tr>" +
																   "<tr><td>서류제출 내용</td><td>기타서류제출</td></tr>" +
																   "<tr><td>세관 담당자</td><td>[KCS011] 홍길동</td></tr></table>");
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
			var fileReader = new TestFileReader(typeof(GOVCBRR73MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R73;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;

			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Incoming";
	}
}

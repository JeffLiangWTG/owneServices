using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRR95MessageProcessorTest : XMLMessageTestHelper<GOVCBRR95MessageProcessorTest>
	{
		public void TestR95()
		{
			CreateEntryWithOutgoingMessageForExport();
			var incomingMessage = CreateMessageForTest("GOVCBRR95_CUS.xml");
			SampleCodeType();
			Factory.Save();
			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);
			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CLR", "CLR", exportEntry.CH_EntryStatus);
			TestEmailIsSent();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("무한상사", email.Body);
			AssertContains("검사담당자명", email.Body);
			AssertContains("02-5256-5441", email.Body);

			AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																   "<th>항 목</th><th>내 용</th></tr></thead>" +
																   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																   "<tr><td>검사일자</td><td>2020-08-18</td></tr>" +
																   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																   "<tr><td>검사차수</td><td>1</td></tr>" +
																   "<tr><td>신고인 상호</td><td>무한상사</td></tr>" +
																   "<tr><td>검사 세관(과)</td><td>[10010] 동해세관 통관지원(1)과</td></tr>" +
																   "<tr><td>검사 담당자명</td><td>검사담당자명</td></tr>" +
																   "<tr><td>세관 담당자 전화번호</td><td>02-5256-5441</td></tr></table>");
		}
		public void Test_AuthenticatorAndSubmitterIsEmpty()
		{
			CreateEntryWithOutgoingMessageForExport();
			var incomingMessage = CreateMessageForTest("GOVCBRR95_Empty.xml");
			SampleCodeType();
			Factory.Save();
			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertNoExceptionThrown("When no Response.Authenticator is there, system should still proceed successfully", () =>
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage));
			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CLR", "CLR", exportEntry.CH_EntryStatus);
			TestEmailIsSent();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("무한상사", email.Body);
			AssertNotContains("검사담당자명", email.Body);
			AssertNotContains("02-5256-5441", email.Body);

			AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																   "<th>항 목</th><th>내 용</th></tr></thead>" +
																   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																   "<tr><td>검사일자</td><td>2020-08-18</td></tr>" +
																   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																   "<tr><td>검사차수</td><td>1</td></tr>" +
																   "<tr><td>신고인 상호</td><td>&nbsp;</td></tr>" +
																   "<tr><td>검사 세관(과)</td><td>[10010] 동해세관 통관지원(1)과</td></tr>" +
																   "<tr><td>검사 담당자명</td><td>&nbsp;</td></tr>" +
																   "<tr><td>세관 담당자 전화번호</td><td>&nbsp;</td></tr></table>");
		}

		void TestEmailIsSent()
		{
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("2020-08-18", email.Body);
			AssertContains("6N002-20-000052X", email.Body);
			AssertContains("1", email.Body);
			AssertContains("[10010] 동해세관 통관지원(1)과", email.Body);
		}

		public void TestExportR95_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				var incomingMessage = CreateMessageForTest("GOVCBRR95_CUS.xml");
				SampleCodeType();
				Factory.Save();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수출 검사완료통보]6N00220000052X 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);

				AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																	   "<th>항 목</th><th>내 용</th></tr></thead>" +
																	   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																	   "<tr><td>검사일자</td><td>2020-08-18</td></tr>" +
																	   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																	   "<tr><td>검사차수</td><td>1</td></tr>" +
																	   "<tr><td>신고인 상호</td><td>무한상사</td></tr>" +
																	   "<tr><td>검사 세관(과)</td><td>[10010] 동해세관 통관지원(1)과</td></tr>" +
																	   "<tr><td>검사 담당자명</td><td>검사담당자명</td></tr>" +
																	   "<tr><td>세관 담당자 전화번호</td><td>02-5256-5441</td></tr></table>");
			}
		}

		public void TestExportR95_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBRR95_CUS.xml");
				CreateEntryForExport(false);
				SampleCodeType();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 검사완료통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수출신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);

				AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																	   "<th>항 목</th><th>내 용</th></tr></thead>" +
																	   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																	   "<tr><td>검사일자</td><td>2020-08-18</td></tr>" +
																	   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																	   "<tr><td>검사차수</td><td>1</td></tr>" +
																	   "<tr><td>신고인 상호</td><td>무한상사</td></tr>" +
																	   "<tr><td>검사 세관(과)</td><td>[10010] </td></tr>" +
																	   "<tr><td>검사 담당자명</td><td>검사담당자명</td></tr>" +
																	   "<tr><td>세관 담당자 전화번호</td><td>02-5256-5441</td></tr></table>");
			}
		}

		public void TestExportR95_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBRR95_CUS.xml");
				CreateEntryForExport(true);
				SampleCodeType();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 검사완료통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);

				AssertEquals(incomingMessage.EM_MessageInterpretation, "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><thead><tr class=\"tableheadings\">" +
																	   "<th>항 목</th><th>내 용</th></tr></thead>" +
																	   "<tr><td>제출문서</td><td>수출신고서</td></tr>" +
																	   "<tr><td>검사일자</td><td>2020-08-18</td></tr>" +
																	   "<tr><td>수출신고번호</td><td>6N002-20-000052X</td></tr>" +
																	   "<tr><td>검사차수</td><td>1</td></tr>" +
																	   "<tr><td>신고인 상호</td><td>무한상사</td></tr>" +
																	   "<tr><td>검사 세관(과)</td><td>[10010] </td></tr>" +
																	   "<tr><td>검사 담당자명</td><td>검사담당자명</td></tr>" +
																	   "<tr><td>세관 담당자 전화번호</td><td>02-5256-5441</td></tr></table>");
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
			var fileReader = new TestFileReader(typeof(GOVCBRR95MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R95;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}
		void SampleCodeType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "100", "동해세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "10", "통관지원(1)과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Incoming";
	}
}

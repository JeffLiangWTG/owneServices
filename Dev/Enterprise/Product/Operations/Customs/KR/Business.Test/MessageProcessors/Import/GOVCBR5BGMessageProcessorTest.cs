using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5BGMessageProcessorTest : XMLMessageTestHelper<GOVCBR5BGMessageProcessorTest>
	{
		public void Test5BG()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5BG_0.xml");
			CreateEntryWithOutgoingMessageForImport();
			SampleCodeType();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", importEntry.CH_EntryStatus.IsEmpty);
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", importEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CCL", CustomsEntryStatusTypeList.Codes.CCL, importEntry.CH_EntryStatus);
			AssertEquals("entry message status is updated correctly to CAP", CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms, importEntry.CH_Status);
			AssertEquals(CustomsEntryStatusTypeList.Codes.ANT, incomingMessage.EM_MessageOwner);
			var outgoingMessage = (EDIMessage)importEntry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5BF);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals("message subType is DeclarationType", ElectronicDocumentTypeList.Codes._5BF, incomingMessage.EM_MessageSubType);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입신고서", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("2020-09-17 17:02", email.Body);
			AssertContains("C 승인", email.Body);
			AssertContains("99 기타사유", email.Body);
			AssertContains("12 위험물,검역 검사등을 위한 장치장소(세관-과) 변경", email.Body);
			AssertContains("승인기타사유", email.Body);
			AssertContains("내용 : 기타사유", email.Body);
			AssertContains("[03083] 부산세관 신항부두통관과", email.Body);
			AssertContains("류상하", email.Body);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-17 17:02", incomingMessage.EM_MessageInterpretation);
			AssertContains("C 승인", incomingMessage.EM_MessageInterpretation);
			AssertContains("99 기타사유", incomingMessage.EM_MessageInterpretation);
			AssertContains("12 위험물,검역 검사등을 위한 장치장소(세관-과) 변경", incomingMessage.EM_MessageInterpretation);
			AssertContains("승인기타사유", incomingMessage.EM_MessageInterpretation);
			AssertContains("내용 : 기타사유", incomingMessage.EM_MessageInterpretation);
			AssertContains("[03083] 부산세관 신항부두통관과", incomingMessage.EM_MessageInterpretation);
			AssertContains("류상하", incomingMessage.EM_MessageInterpretation);
		}

		public void TestEmptyAdditionalInformationAndReasons()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5BG_WithoutData.xml");
			CreateEntryWithOutgoingMessageForImport();
			SampleCodeType();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", importEntry.CH_EntryStatus.IsEmpty);
			AssertNoExceptionThrown(() => new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage));

			AssertEquals("entry message status is updated correctly to CDC", CustomsMessageStatusTypeList.Codes.CancellationDeclined, importEntry.CH_Status);
			AssertEquals(CustomsEntryStatusTypeList.Codes.DMS, incomingMessage.EM_MessageOwner);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("99 기타사유", email.Body);
			AssertNotContains("12 위험물,검역 검사등을 위한 장치장소(세관-과) 변경", email.Body);
			AssertNotContains("승인기타사유", email.Body);
			AssertNotContains("내용 : 기타사유", email.Body);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-17 17:02", incomingMessage.EM_MessageInterpretation);
			AssertContains("E 기각", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>귀책사유부호</td><td>&nbsp;</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>취하사유부호</td><td>&nbsp;</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>기타사유</td><td>&nbsp;</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>기각사유</td><td>&nbsp;</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("[03083] 부산세관 신항부두통관과", incomingMessage.EM_MessageInterpretation);
			AssertContains("홍길동", incomingMessage.EM_MessageInterpretation);
		}

		public void TestWrongStructure()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5BG_WrongDeclarationOfficeID.xml");
			CreateEntryWithOutgoingMessageForImport();
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", importEntry.CH_EntryStatus.IsEmpty);
			AssertNoExceptionThrown(() => new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage));

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("[03083] 부산세관 신항부두통관과", email.Body);

			AssertContains("<td>처리세관(과)부호</td><td>[030831] </td>", incomingMessage.EM_MessageInterpretation);
		}

		public void Test5BGUpdateData()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5BG_WithoutData.xml");
			CreateEntryWithOutgoingMessageForImport();
			Factory.Save();

			AssertNoExceptionThrown(() => new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage));

			AssertNullOrEmpty(importEntry.CH_EntryStatus);
			var entryNum = importEntry.EntryNumbers.GetOrCreateCusEntryNum("IMP");
			AssertEquals(ZDateTime.Empty, entryNum.CE_ExpiryDate);

			incomingMessage = CreateMessageForTest("GOVCBR5BG_0.xml");
			Factory.Save();
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			importEntry.Reload();
			importEntry.EntryNumbers.Reload(true);
			AssertEquals(new ZDateTime(2020, 09, 17, 17, 02, 00), entryNum.CE_ExpiryDate);
		}

		public void TestImport5BG_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				var incomingMessage = CreateMessageForTest("GOVCBR5BG_0.xml");
				SampleCodeType();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수입 취하신청 처리결과 통보]1234520000045M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);

				AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
				AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-09-17 17:02", incomingMessage.EM_MessageInterpretation);
				AssertContains("C 승인", incomingMessage.EM_MessageInterpretation);
				AssertContains("99 기타사유", incomingMessage.EM_MessageInterpretation);
				AssertContains("12 위험물,검역 검사등을 위한 장치장소(세관-과) 변경", incomingMessage.EM_MessageInterpretation);
				AssertContains("승인기타사유", incomingMessage.EM_MessageInterpretation);
				AssertContains("내용 : 기타사유", incomingMessage.EM_MessageInterpretation);
				AssertContains("[03083] 부산세관 신항부두통관과", incomingMessage.EM_MessageInterpretation);
				AssertContains("류상하", incomingMessage.EM_MessageInterpretation);
			}
		}

		public void TestImport5BG_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBR5BG_0.xml");
				CreateEntryForImport(false);
				SampleCodeType();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입 취하신청 처리결과 통보] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수입취하 신청서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);

				AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
				AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-09-17 17:02", incomingMessage.EM_MessageInterpretation);
				AssertContains("C 승인", incomingMessage.EM_MessageInterpretation);
				AssertContains("99 기타사유", incomingMessage.EM_MessageInterpretation);
				AssertContains("12 위험물,검역 검사등을 위한 장치장소(세관-과) 변경", incomingMessage.EM_MessageInterpretation);
				AssertContains("승인기타사유", incomingMessage.EM_MessageInterpretation);
				AssertContains("내용 : 기타사유", incomingMessage.EM_MessageInterpretation);
				AssertContains("[03083] 부산세관 신항부두통관과", incomingMessage.EM_MessageInterpretation);
				AssertContains("류상하", incomingMessage.EM_MessageInterpretation);
			}
		}

		public void TestImport5BG_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBR5BG_0.xml");
				CreateEntryForImport(true);
				SampleCodeType();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입 취하신청 처리결과 통보] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);

				AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
				AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-09-17 17:02", incomingMessage.EM_MessageInterpretation);
				AssertContains("C 승인", incomingMessage.EM_MessageInterpretation);
				AssertContains("99 기타사유", incomingMessage.EM_MessageInterpretation);
				AssertContains("12 위험물,검역 검사등을 위한 장치장소(세관-과) 변경", incomingMessage.EM_MessageInterpretation);
				AssertContains("승인기타사유", incomingMessage.EM_MessageInterpretation);
				AssertContains("내용 : 기타사유", incomingMessage.EM_MessageInterpretation);
				AssertContains("[03083] 부산세관 신항부두통관과", incomingMessage.EM_MessageInterpretation);
				AssertContains("류상하", incomingMessage.EM_MessageInterpretation);
			}
		}

		void SampleCodeType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType("CUSDP", "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSOF", "030", "부산세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSDP", "83", "신항부두통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();
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
			staff2.GS_EmailAddress = "ImportGroupTest@wisetechglobal.com";
			importGroup = Factory.New<GlbGroup>();
			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = importGroup.PK;
			link1.GK_GS = staff2.PK;

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "AG";
			staff3.GS_LoginName = "Agent";
			staff3.GS_EmailAddress = "CusAgent@wisetechglobal.com";
			Factory.Save();
		}
		GlbGroup importGroup;

		void CreateEntryForImport(bool setCusAgent)
		{
			var declaration = Factory.New<JobDeclaration>();
			if (setCusAgent)
			{
				declaration.JE_GS_NKCusAgent = "AG";
			}
			importEntry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = importEntry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "1234520000045M";
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = importEntry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNumber.CE_EntryStatus = ZString.Empty;
			Factory.Save();
		}
		CusEntryHeader importEntry;

		void CreateEntryWithOutgoingMessageForImport()
		{
			if (importEntry == null)
			{
				CreateEntryForImport(true);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5BF;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = importEntry.PK;
			outgoingMessage.EM_LinkedObject = importEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5BGMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5BG;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}

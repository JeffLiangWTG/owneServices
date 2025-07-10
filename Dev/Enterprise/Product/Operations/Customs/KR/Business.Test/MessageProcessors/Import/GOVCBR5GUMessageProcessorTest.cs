using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5GUMessageProcessorTest : XMLMessageTestHelper<GOVCBR5GUMessageProcessorTest>
	{
		public void Test5GU()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5GU_0.xml");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "030", "부산세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "81", "신항수입과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-08-21", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-04", incomingMessage.EM_MessageInterpretation);
			AssertContains("[03081] 부산세관 신항수입과", incomingMessage.EM_MessageInterpretation);
			AssertContains("이지은", incomingMessage.EM_MessageInterpretation);
			AssertContains("030-81-시정-20-02218", incomingMessage.EM_MessageInterpretation);
			AssertContains("051-620-6167", incomingMessage.EM_MessageInterpretation);
			AssertContains("1", incomingMessage.EM_MessageInterpretation);
			AssertContains("04", incomingMessage.EM_MessageInterpretation);
			AssertContains("현품에 원산지 미표시", incomingMessage.EM_MessageInterpretation);
			AssertContains("표시방법 : 스티커<br>기타시정사항 : 화주의 책임하, 현품의 잘보이는곳에 'MADE IN TAIWAN' 을 쉽게 제거되지않는 스티커를 이용하여 원산지 보수작업을 하여주시기 바랍니다. <br>해당 건은 원산지 미표시 2차로 과징금이 부과되오니 차후 수입시 유의하시기 바랍니다.", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("나머지 내역은 프로그램에서 확인 하십시오.", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입신고서", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("2020-08-21", email.Body);
			AssertContains("2020-09-04", email.Body);
			AssertContains("[03081] 부산세관 신항수입과", email.Body);
			AssertContains("이지은", email.Body);
			AssertContains("030-81-시정-20-02218", email.Body);
			AssertContains("051-620-6167", email.Body);
			AssertContains("1", email.Body);
			AssertContains("04", email.Body);
			AssertContains("현품에 원산지 미표시", email.Body);
			AssertContains("표시방법 : 스티커<br>기타시정사항 : 화주의 책임하, 현품의 잘보이는곳에 'MADE IN TAIWAN' 을 쉽게 제거되지않는 스티커를 이용하여 원산지 보수작업을 하여주시기 바랍니다. <br>해당 건은 원산지 미표시 2차로 과징금이 부과되오니 차후 수입시 유의하시기 바랍니다.", email.Body);
			AssertNotContains("나머지 내역은 프로그램에서 확인 하십시오.", email.Body);
		}

		public void TestEmptyCustomsOfficer()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5GU_WithoutVer.xml");
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			entry.Reload();
			incomingMessage.Reload();

			AssertNotContains("2020-08-21", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("[03081] 부산세관 신항수입과", incomingMessage.EM_MessageInterpretation);
			AssertContains("Test11", incomingMessage.EM_MessageInterpretation);
			AssertContains("Test12", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("나머지 내역은 프로그램에서 확인 하십시오.", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertNotContains("2020-08-21", email.Body);
			AssertNotContains("[03081] 부산세관 신항수입과", email.Body);
			AssertContains("나머지 내역은 프로그램에서 확인 하십시오.", email.Body);
		}

		public void Test5GUUpdateData()
		{
			var entry = CreateEntryWithOutgoingMessageForImport();
			CreateMessageForTest("GOVCBR5GU_0.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.EntryNumbers.Reload(true);

			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5GU);
			AssertEquals("030812002218", entryNum.CE_EntryNum);
			AssertEquals(new ZDate(2020, 08, 21), entryNum.CE_IssueDate);
			AssertEquals(new ZDate(2020, 09, 04), entryNum.CE_ExpiryDate);
		}

		public void Test5GUUpdateData_RealData()
		{
			var entry = CreateEntryWithOutgoingMessageForImport();
			CreateMessageForTest("GOVCBR5GU_RealData.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.EntryNumbers.Reload(true);

			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5GU);
			AssertEquals("040C22101363", entryNum.CE_EntryNum);
			AssertEquals(new ZDate(2021, 07, 08), entryNum.CE_IssueDate);
			AssertEquals(new ZDate(2021, 07, 22), entryNum.CE_ExpiryDate);
		}

		public void TestImport5GU_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5GU_0.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [원산지 시정명령 통보]1234520000045M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImport5GU_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5GU_0.xml");
				CreateEntryForImport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[원산지 시정명령 통보] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수입신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImport5GU_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5GU_0.xml");
				CreateEntryForImport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[원산지 시정명령 통보] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
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
			staff2.GS_Code = "T2";
			staff2.GS_LoginName = "Test2";
			staff2.GS_EmailAddress = "ImportGroupTest@wisetechglobal.com";
			importGroup = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			var link2 = Factory.New<GlbGroupLink>();
			link2.GK_GG = importGroup.PK;
			link2.GK_GS = staff2.PK;

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
		}
		CusEntryHeader importEntry;

		CusEntryHeader CreateEntryWithOutgoingMessageForImport()
		{
			if (importEntry == null)
			{
				CreateEntryForImport(true);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._929;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkedObject = importEntry;

			return importEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5GUMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5GU;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}

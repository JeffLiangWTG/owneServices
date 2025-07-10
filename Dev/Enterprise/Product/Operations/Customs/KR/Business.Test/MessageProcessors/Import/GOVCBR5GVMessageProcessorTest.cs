using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5GVMessageProcessorTest : XMLMessageTestHelper<GOVCBR5GVMessageProcessorTest>
	{
		public void Test5GV()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5GV_0.xml");
			CreateEntryWithOutgoingMessageForImport();
			Factory.Save();
			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", importEntry.CH_Status.IsEmpty);
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", importEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CGD", "CGD", importEntry.CH_EntryStatus);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입신고서", email.Body);
			AssertContains("2020-08-26", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("3", email.Body);
			AssertContains("서류변경", email.Body);
			AssertContains("2020-09-10", email.Body);
			AssertContains("이영숙(16과)", email.Body);
			AssertContains("051-620-6922", email.Body);
			AssertContains("전자서류제출", email.Body);
			AssertContains("<tr><td>1</td><td>B407</td><td>관세감면분납코드</td></tr>", email.Body);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-08-26", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("3", incomingMessage.EM_MessageInterpretation);
			AssertContains("서류변경", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-10", incomingMessage.EM_MessageInterpretation);
			AssertContains("이영숙(16과)", incomingMessage.EM_MessageInterpretation);
			AssertContains("051-620-6922", incomingMessage.EM_MessageInterpretation);
			AssertContains("전자서류제출", incomingMessage.EM_MessageInterpretation);
			AssertContains("B407", incomingMessage.EM_MessageInterpretation);
			AssertContains("관세감면분납코드", incomingMessage.EM_MessageInterpretation);
		}

		public void TestWhenThereAreMoreThanTenLines()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5GVWith22GoodsShipments.xml");
			CreateEntryWithOutgoingMessageForImport();
			var entryNum = importEntry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5GV);
			Factory.Save();

			AssertNullOrEmpty(entryNum.CE_EntryNum);
			AssertEquals(ZDate.Empty, entryNum.CE_IssueDate);
			AssertEquals(ZDate.Empty, entryNum.CE_ExpiryDate);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entryNum.Reload();
			incomingMessage.Reload();

			AssertEquals("014102100013", entryNum.CE_EntryNum);
			AssertEquals(new ZDateTime(2021, 03, 26), entryNum.CE_IssueDate);
			AssertEquals(new ZDateTime(2021, 04, 10), entryNum.CE_ExpiryDate);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertNotContains("나머지 내역은 프로그램에서 확인 하십시오.", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td>1</td><td>B407</td><td>관세감면분납코드</td><td>1</td><td>B408</td><td>관세감면율</td></tr><tr>", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertContains("나머지 내역은 프로그램에서 확인 하십시오.", Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
		}

		public void TestWhenTransactionNatureCodeIs2()
		{
			CreateMessageForTest("GOVCBR5GV_TransactionNatureCode2.xml");
			CreateEntryWithOutgoingMessageForImport();
			var entryNum = importEntry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5GV);
			Factory.Save();

			AssertNullOrEmpty(entryNum.CE_EntryNum);
			AssertEquals(ZDate.Empty, entryNum.CE_IssueDate);
			AssertEquals(ZDate.Empty, entryNum.CE_ExpiryDate);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entryNum.Reload();

			AssertEquals("014102100012", entryNum.CE_EntryNum);
			AssertEquals(new ZDateTime(2021, 03, 26), entryNum.CE_IssueDate);
			AssertEquals(new ZDateTime(2021, 04, 10), entryNum.CE_ExpiryDate);
		}

		public void TestWhenTransactionNatureCodeIs3()
		{
			CreateEntryWithOutgoingMessageForImport();
			var entryNum = importEntry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5GV);
			CreateMessageForTest("GOVCBR5GV_TransactionNatureCode3.xml");
			Factory.Save();

			AssertNullOrEmpty(entryNum.CE_EntryNum);
			AssertEquals(ZDate.Empty, entryNum.CE_IssueDate);
			AssertEquals(ZDate.Empty, entryNum.CE_ExpiryDate);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entryNum.Reload();

			AssertEquals("130102000033", entryNum.CE_EntryNum);
			AssertEquals(new ZDateTime(2020, 05, 11), entryNum.CE_IssueDate);
			AssertEquals(new ZDateTime(2020, 05, 26), entryNum.CE_ExpiryDate);
		}

		public void TestImport5GV_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				var incomingMessage = CreateMessageForTest("GOVCBR5GV_0.xml");
				Factory.Save();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수입통관 보완요구서]1234520000045M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);

				AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-08-26", incomingMessage.EM_MessageInterpretation);
				AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
				AssertContains("3", incomingMessage.EM_MessageInterpretation);
				AssertContains("서류변경", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-09-10", incomingMessage.EM_MessageInterpretation);
				AssertContains("이영숙(16과)", incomingMessage.EM_MessageInterpretation);
				AssertContains("051-620-6922", incomingMessage.EM_MessageInterpretation);
				AssertContains("전자서류제출", incomingMessage.EM_MessageInterpretation);
			}
		}

		public void TestImport5GV_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBR5GV_0.xml");
				CreateEntryForImport(false);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입통관 보완요구서] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수입신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);

				AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-08-26", incomingMessage.EM_MessageInterpretation);
				AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
				AssertContains("3", incomingMessage.EM_MessageInterpretation);
				AssertContains("서류변경", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-09-10", incomingMessage.EM_MessageInterpretation);
				AssertContains("이영숙(16과)", incomingMessage.EM_MessageInterpretation);
				AssertContains("051-620-6922", incomingMessage.EM_MessageInterpretation);
				AssertContains("전자서류제출", incomingMessage.EM_MessageInterpretation);
			}
		}

		public void TestImport5GV_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBR5GV_0.xml");
				CreateEntryForImport(true);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입통관 보완요구서] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);

				AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-08-26", incomingMessage.EM_MessageInterpretation);
				AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
				AssertContains("3", incomingMessage.EM_MessageInterpretation);
				AssertContains("서류변경", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-09-10", incomingMessage.EM_MessageInterpretation);
				AssertContains("이영숙(16과)", incomingMessage.EM_MessageInterpretation);
				AssertContains("051-620-6922", incomingMessage.EM_MessageInterpretation);
				AssertContains("전자서류제출", incomingMessage.EM_MessageInterpretation);
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
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._929;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = importEntry.PK;
			outgoingMessage.EM_LinkedObject = importEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5GVMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5GV;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}

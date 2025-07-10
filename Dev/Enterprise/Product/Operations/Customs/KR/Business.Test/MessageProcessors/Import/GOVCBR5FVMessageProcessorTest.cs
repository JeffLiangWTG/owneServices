using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5FVMessageProcessorTest : XMLMessageTestHelper<GOVCBR5FVMessageProcessorTest>
	{
		public void Test5FV()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5FV_0.xml");
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
			AssertContains("030-20-10763124", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("0127030112000511999", incomingMessage.EM_MessageInterpretation);
			AssertContains("030-75-20-21999", incomingMessage.EM_MessageInterpretation);
			AssertContains("6521842299", incomingMessage.EM_MessageInterpretation);
			AssertContains("서울맥스텔레콤", incomingMessage.EM_MessageInterpretation);
			AssertContains("박종훈", incomingMessage.EM_MessageInterpretation);
			AssertContains("서울 강남구 영동대로 511,20층 2012 (삼성동)", incomingMessage.EM_MessageInterpretation);
			AssertContains("20-09-23", incomingMessage.EM_MessageInterpretation);
			AssertContains("6", incomingMessage.EM_MessageInterpretation);
			AssertContains("-2379600", incomingMessage.EM_MessageInterpretation);
			AssertContains("-237960", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-23", incomingMessage.EM_MessageInterpretation);
			AssertContains("[E] 부가세환급", incomingMessage.EM_MessageInterpretation);
			AssertContains("[02] 수입세금계산서(과세분)", incomingMessage.EM_MessageInterpretation);
			AssertContains("[A] 과오납환급", incomingMessage.EM_MessageInterpretation);
			AssertContains("Y", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입신고서", email.Body);
			AssertContains("030-20-10763124", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("0127030112000511999", email.Body);
			AssertContains("030-75-20-21999", email.Body);
			AssertContains("6521842299", email.Body);
			AssertContains("서울맥스텔레콤", email.Body);
			AssertContains("박종훈", email.Body);
			AssertContains("서울 강남구 영동대로 511,20층 2012 (삼성동)", email.Body);
			AssertContains("20-09-23", email.Body);
			AssertContains("6", email.Body);
			AssertContains("-2379600", email.Body);
			AssertContains("-237960", email.Body);
			AssertContains("2020-09-23", email.Body);
			AssertContains("[E] 부가세환급", email.Body);
			AssertContains("[02] 수입세금계산서(과세분)", email.Body);
			AssertContains("[A] 과오납환급", email.Body);
			AssertContains("Y", email.Body);
		}

		public void Test5FV_EmptyAmendmentDateTime()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5FV_1.xml");
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			incomingMessage.Reload();
			entry.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
			AssertContains("040-21-11136691", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("0127040112101222168", incomingMessage.EM_MessageInterpretation);
			AssertContains("1208773486", incomingMessage.EM_MessageInterpretation);
			AssertContains("0127040112101222168", incomingMessage.EM_MessageInterpretation);
			AssertContains("21-01-15", incomingMessage.EM_MessageInterpretation);
			AssertContains("6", incomingMessage.EM_MessageInterpretation);
			AssertContains("4801834", incomingMessage.EM_MessageInterpretation);
			AssertContains("480180", incomingMessage.EM_MessageInterpretation);
			AssertContains("[0] 당초부가세수입(세금)계산서", incomingMessage.EM_MessageInterpretation);
			AssertContains("[02] 수입세금계산서(과세분)", incomingMessage.EM_MessageInterpretation);
			AssertContains("[0] 당초부가세수입(세금)계산서", incomingMessage.EM_MessageInterpretation);
			AssertContains("N", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입신고서", email.Body);
			AssertContains("040-21-11136691", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("0127040112101222168", email.Body);
			AssertContains("1208773486", email.Body);
			AssertContains("0127040112101222168", email.Body);
			AssertContains("21-01-15", email.Body);
			AssertContains("6", email.Body);
			AssertContains("4801834", email.Body);
			AssertContains("480180", email.Body);
			AssertContains("[0] 당초부가세수입(세금)계산서", email.Body);
			AssertContains("[02] 수입세금계산서(과세분)", email.Body);
			AssertContains("[0] 당초부가세수입(세금)계산서", email.Body);
			AssertContains("N", email.Body);
		}

		public void TestImport5FV_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBR5FV_0.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수입 세금계산서(개별)]1234520000045M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImport5FV_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5FV_0.xml");
				CreateEntryForImport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입 세금계산서(개별)] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수입신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImport5FV_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5FV_0.xml");
				CreateEntryForImport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입 세금계산서(개별)] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void Test5FVStatementHeader()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5FV_0.xml");
			Factory.Save();

			ZQuery query = new ZQuery(CusStatementHeaderSchema.B2_GC, entry.Declaration.JE_GC);
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
			var statementCollection = Factory.Load<CusStatementHeader>(query);
			var beforeCount = statementCollection.Length;

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			statementCollection = Factory.Load<CusStatementHeader>(query);
			var afterCount = statementCollection.Length;

			AssertEquals(beforeCount, afterCount);

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "0127030112000511999";
			statement.B2_GC = entry.Declaration.JE_GC;
			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			Factory.Save();

			incomingMessage = CreateMessageForTest("GOVCBR5FV_0.xml");
			Factory.Save();
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			statement.Reload();

			AssertEquals(new ZDate(2020, 09, 23), statement.B2_PaymentAuthorizationDate);
			AssertEquals(StatementHeaderPaymentStatusList.Codes.PYC, statement.B2_PaymentStatus);
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
			declaration.JE_DeclarationReference = "B00001000";
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
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkedObject = importEntry;

			return importEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5FVMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5FV;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}

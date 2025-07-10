using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5WNMessageProcessorTest : XMLMessageTestHelper<GOVCBR5WNMessageProcessorTest>
	{
		public void Test5WN()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5WN_CUS.xml");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("Version number is saved", "1", incomingMessage.EM_MessageOwner);
			AssertContains("incomingMessage EM_MessageInterpretation is updated", "수입신고서", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입신고서", email.Body);
			AssertContains("2020-01-02", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("2020-09-01", email.Body);
			AssertContains("1", email.Body);
			AssertContains("2020-09-09", email.Body);
			AssertContains("(주)다스", email.Body);
			AssertContains("이상은,송현", email.Body);
			AssertContains("경북 경주시 외동읍 외동농공단지길 14", email.Body);
			AssertContains("인천세관 조사(납세)심사과", email.Body);
			AssertContains("임승현", email.Body);
			AssertContains("이상은,송현", email.Body);
			AssertContains("032-722-4062", email.Body);
			AssertContains("FTA 사후협정 정정 신청합니다", email.Body);
			AssertContains("인천세관", email.Body);
			AssertContains("4163720000011M", email.Body);
			AssertContains("-3031740", email.Body);
			AssertContains("-2756130", email.Body);
			AssertContains("-275610", email.Body);
			AssertContains("개별소비세,교통세,주세,교육세,농특세,신고지연가산세,휴대품등미신고가산세,과소신고가산세(관세),과소신고가산세(내국세),납부불성실가산세(관세),납부불성실가산세(내국세),무신고가산세,수입신고불이행가산세,재수출불이행가산세,과다환급가산금", email.Body);
		}

		public void TestEmpty()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentTable = "CusEntryHeader";
			var incomingMessage = CreateMessageForTest("GOVCBR5WN_Empty.xml");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);

			AssertNoExceptionThrown("When no Response.Authenticator is there, system should still proceed successfully", () =>
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			incomingMessage.Reload();

			AssertContains("incomingMessage EM_MessageInterpretation is updated", "수입신고서", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("2020-09-01", email.Body);
			AssertNotContains("032-722-4062", email.Body);
		}

		public void TestDutyTaxfeeIsNotZero()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentTable = "CusEntryHeader";
			var incomingMessage = CreateMessageForTest("GOVCBR5WN_DutyTaxfeeIsNotZero.xml");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);

			AssertNoExceptionThrown("When Response.Declaration/DutyTaxFee is there, system should still proceed successfully", () =>
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			incomingMessage.Reload();

			AssertContains("incomingMessage EM_MessageInterpretation is updated", "수입신고서", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("개별소비세,교통세,주세,교육세,농특세,신고지연가산세,휴대품등미신고가산세,과소신고가산세(관세),과소신고가산세(내국세),납부불성실가산세(관세),납부불성실가산세(내국세),무신고가산세,수입신고불이행가산세,재수출불이행가산세,과다환급가산금", email.Body);
		}

		public void TestDutyTaxfeeCodeError()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentTable = "CusEntryHeader";
			var incomingMessage = CreateMessageForTest("GOVCBR5WN_DutyTaxfeeCodeError.xml");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);

			AssertNoExceptionThrown("When Response.Declaration/DutyTaxFee is Error, system should still proceed successfully", () =>
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			incomingMessage.Reload();

			AssertContains("incomingMessage EM_MessageInterpretation is updated", "수입신고서", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("AAA", email.Body);
			AssertContains("BBB", email.Body);
			AssertContains("CCC", email.Body);
		}

		public void TestImport5WN_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				var incomingMessage = CreateMessageForTest("GOVCBR5WN_CUS.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				incomingMessage.Reload();

				AssertContains("incomingMessage EM_MessageInterpretation is updated", "수입신고서", incomingMessage.EM_MessageInterpretation);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [세액경정통지서]1234520000045M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImport5WN_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBR5WN_CUS.xml");
				CreateEntryForImport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				incomingMessage.Reload();

				AssertContains("incomingMessage EM_MessageInterpretation is updated", "수입신고서", incomingMessage.EM_MessageInterpretation);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[세액경정통지서] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수입신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImport5WN_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBR5WN_CUS.xml");
				CreateEntryForImport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				incomingMessage.Reload();

				AssertContains("incomingMessage EM_MessageInterpretation is updated", "수입신고서", incomingMessage.EM_MessageInterpretation);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[세액경정통지서] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestCusStatementHeader_Create()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryWithOutgoingMessageForImport();

			var incomingMessage = CreateMessageForTest("GOVCBR5WN_DutyTaxDifferenceIsNotZero.xml");
			var response = KRXmlObjectSerializer.Deserialize<CargoWise.Customs.KR.MessageDefinitions.GOVCBR5WN.Response>(incomingMessage.GetEM_MessageTextReader());
			AssertEquals("040422400000405", response.Declaration.Id.Value);
			AssertEquals("20240115", response.IssueDateTime);
			AssertEquals("20240115", response.AdditionalInformation.LimitDateTime);
			AssertEquals(563410m, response.Declaration.AdditionalDocument.AmountAmount.Value);
			AssertEquals("1", response.Declaration.VersionId.Value);
			Factory.Save();

			var statement = new CusStatementHeader.Loader(Factory).Load("040422400000405", importEntry.Declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
			AssertNull(statement);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			AssertCreatedOrUpdatedCusStatementHeader(true, incomingMessage.EM_MessageNum);
		}
		public void TestCusStatementHeader_Update()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryWithOutgoingMessageForImport();

			var incomingMessage = CreateMessageForTest("GOVCBR5WN_DutyTaxDifferenceIsNotZero.xml");
			var response = KRXmlObjectSerializer.Deserialize<CargoWise.Customs.KR.MessageDefinitions.GOVCBR5WN.Response>(incomingMessage.GetEM_MessageTextReader());
			AssertEquals("040422400000405", response.Declaration.Id.Value);
			AssertEquals("20240115", response.IssueDateTime);
			AssertEquals("20240115", response.AdditionalInformation.LimitDateTime);
			AssertEquals(563410m, response.Declaration.AdditionalDocument.AmountAmount.Value);
			AssertEquals("1", response.Declaration.VersionId.Value);

			var cusStatementHeader = Factory.New<CusStatementHeader>();
			cusStatementHeader.B2_GC = importEntry.Declaration.JE_GC;
			cusStatementHeader.B2_StatementNumber = "040422400000405";
			cusStatementHeader.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;

			var cusStatementLine = cusStatementHeader.StatementLines.AddNew();
			cusStatementLine.B3_EntryType = "XXX";
			cusStatementLine.B3_EntryNum = "1234567890";
			Factory.Save();

			var statement = new CusStatementHeader.Loader(Factory).Load("040422400000405", importEntry.Declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
			AssertNotNull(statement);
			AssertEquals(0, cusStatementHeader.Messages.Count);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			AssertCreatedOrUpdatedCusStatementHeader(false, incomingMessage.EM_MessageNum);

			var incomingMessage2 = CreateMessageForTest("GOVCBR5WN_DutyTaxDifferenceIsNotZero.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			AssertCreatedOrUpdatedCusStatementHeader(false, incomingMessage2.EM_MessageNum);
		}

		void AssertCreatedOrUpdatedCusStatementHeader(bool wasCreated, string incomingMessageNum)
		{
			var statement = new CusStatementHeader.Loader(new BusinessObjectFactory()).Load("040422400000405", importEntry.Declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
			AssertNotNull(statement);
			if (wasCreated)
			{
				AssertEquals("Attached only when CusStatementHeader is newly created.", 1, statement.Messages.Count);
				AssertEquals(ElectronicDocumentTypeList.Codes._5WN, statement.Messages[0].EM_MessageType);
				AssertEquals("RCV", statement.Messages[0].EM_Status);
			}
			else
			{
				AssertEquals(0, statement.Messages.Count);
			}

			AssertEquals(new ZDateTime("2024-01-15"), statement.B2_ProcessDate);
			AssertEquals(new ZDateTime("2024-01-15"), statement.B2_PrintDate);
			AssertEquals(new ZDateTime("2024-01-30"), statement.B2_DueDate);
			AssertEquals(incomingMessageNum, statement.B2_IncomingMessageNo);
			AssertEquals(563410m, statement.B2_StatementAmount);
			AssertEquals(importEntry.Declaration.JE_OH_DutyPayer, statement.B2_OH_Importer);
			AssertEquals("010", statement.B2_ProcessPort);
			AssertEquals("W", statement.B2_Status);
			AssertEquals("PYI", statement.B2_PaymentStatus);
			AssertEquals("1", statement.B2_RMNumber);

			AssertEquals(1, statement.StatementLines.Count);
			var statementLine = statement.StatementLines[0];
			AssertEquals("IMP", statementLine.B3_EntryType);
			AssertEquals("1234520000045M", statementLine.B3_EntryNum);
			AssertEquals("B3_CustomsFeesTotal", 563410m, statementLine.B3_CustomsFeesTotal);
			AssertEquals(1u, statementLine.B3_SequenceNumber);

			AssertEquals(3, statementLine.Charges.Count);
			AssertEquals(ChargeTypeList.Codes.Duty, statementLine.Charges[0].B4_ChargeType);
			AssertEquals("CUD deduct amount is 215500", 215500m, statementLine.Charges[0].B4_ChargeAmount);

			AssertEquals(ChargeTypeList.Codes.VAT, statementLine.Charges[1].B4_ChargeType);
			AssertEquals("VAT deduct amount is 290930", 290930m, statementLine.Charges[1].B4_ChargeAmount);

			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, statementLine.Charges[2].B4_ChargeType);
			AssertEquals("Type of deduct amount are 5AU:21550, 5AV:29090, 5AW:2700, 5AX:3640", 56980m, statementLine.Charges[2].B4_ChargeAmount);
		}

		public void TestCusStatementHeader_NotCreate()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryWithOutgoingMessageForImport();

			var incomingMessage = CreateMessageForTest("GOVCBR5WN_DutyTaxDifferenceIsZero.xml");
			var response = KRXmlObjectSerializer.Deserialize<CargoWise.Customs.KR.MessageDefinitions.GOVCBR5WN.Response>(incomingMessage.GetEM_MessageTextReader());
			AssertEquals("040422400000405", response.Declaration.Id.Value);
			AssertEquals("20240115", response.IssueDateTime);
			AssertEquals("20240115", response.AdditionalInformation.LimitDateTime);
			AssertEquals(0m, response.Declaration.AdditionalDocument.AmountAmount.Value);
			AssertEquals("1", response.Declaration.VersionId.Value);
			Factory.Save();

			var statement = new CusStatementHeader.Loader(Factory).Load("040422400000405", importEntry.Declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
			AssertNull(statement);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			statement = new CusStatementHeader.Loader(new BusinessObjectFactory()).Load("040422400000405", importEntry.Declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
			AssertNull(statement);
		}
		public void TestCusStatementHeader_NotUpdate()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryWithOutgoingMessageForImport();

			var incomingMessage = CreateMessageForTest("GOVCBR5WN_DutyTaxDifferenceIsZero.xml");
			var response = KRXmlObjectSerializer.Deserialize<CargoWise.Customs.KR.MessageDefinitions.GOVCBR5WN.Response>(incomingMessage.GetEM_MessageTextReader());
			AssertEquals("040422400000405", response.Declaration.Id.Value);
			AssertEquals("20240115", response.IssueDateTime);
			AssertEquals("20240115", response.AdditionalInformation.LimitDateTime);
			AssertEquals(0m, response.Declaration.AdditionalDocument.AmountAmount.Value);
			AssertEquals("1", response.Declaration.VersionId.Value);

			var cusStatementHeader = Factory.New<CusStatementHeader>();
			cusStatementHeader.B2_GC = importEntry.Declaration.JE_GC;
			cusStatementHeader.B2_StatementNumber = "040422400000405";
			cusStatementHeader.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;

			var cusStatementLine = cusStatementHeader.StatementLines.AddNew();
			cusStatementLine.B3_EntryType = "XXX";
			cusStatementLine.B3_EntryNum = "1234567890";
			Factory.Save();

			var statement = new CusStatementHeader.Loader(Factory).Load("040422400000405", importEntry.Declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
			AssertNotNull(statement);
			AssertEquals(0, cusStatementHeader.Messages.Count);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			statement = new CusStatementHeader.Loader(new BusinessObjectFactory()).Load("040422400000405", importEntry.Declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
			AssertNotNull(statement);

			AssertEquals(0, statement.Messages.Count);

			Assert(statement.B2_ProcessDate.IsEmpty);
			Assert(statement.B2_PrintDate.IsEmpty);
			Assert(statement.B2_DueDate.IsEmpty);
			Assert(statement.B2_IncomingMessageNo.IsEmpty);
			Assert(statement.B2_StatementAmount.IsEmpty);
			Assert(statement.B2_OH_Importer.IsEmpty);
			Assert(statement.B2_ProcessPort.IsEmpty);
			Assert(statement.B2_Status.IsEmpty);
			Assert(statement.B2_PaymentStatus.IsEmpty);
			Assert(statement.B2_RMNumber.IsEmpty);

			AssertEquals(1, statement.StatementLines.Count);
			var statementLine = statement.StatementLines[0];
			AssertEquals("XXX", statementLine.B3_EntryType);
			AssertEquals("1234567890", statementLine.B3_EntryNum);

			AssertEquals(0, statementLine.Charges.Count);
		}
		public void Test5FE()
		{
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5WN_5FE.xml");
			Factory.Save();
			var amendmentSessionalData = entry.EntryInstruction.AmendmentSessionalDataCollection.Cast<AmendmentSessionalData>().SingleOrDefault(x => x.CSI_LineNo == 2);

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: CSI_ItemNumber Empty", ZInt.Zero, amendmentSessionalData.CSI_ItemNumber);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			amendmentSessionalData.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: CSI_ItemNumber Empty", 2, amendmentSessionalData.CSI_ItemNumber);
		}

		public void TestIrrelevant5FE()
		{
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5WN_DutyTaxDifferenceIsZero.xml");
			Factory.Save();
			var amendmentSessionalData = entry.EntryInstruction.AmendmentSessionalDataCollection.Cast<AmendmentSessionalData>().SingleOrDefault(x => x.CSI_LineNo == 2);

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: CSI_ItemNumber Empty", ZInt.Zero, amendmentSessionalData.CSI_ItemNumber);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			amendmentSessionalData.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: CSI_ItemNumber Empty", ZInt.Zero, amendmentSessionalData.CSI_ItemNumber);
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
			var orgHeader = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "레디코리아");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "010";
			declaration.JE_OH_DutyPayer = orgHeader.PK;
			if (setCusAgent)
			{
				declaration.JE_GS_NKCusAgent = "AG";
			}
			importEntry = declaration.CustomsEntryHeaders.AddNew();
			importEntry.CH_VersionID = 1;
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
			outgoingMessage.EM_ApplicationReference = "1";

			CreateAmendmentSessionalData(importEntry);

			return importEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5WNMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5WN;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		void CreateAmendmentSessionalData(CusEntryHeader entry)
		{
			var instruction = entry.Declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var amendmentSessionalData = entry.EntryInstruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_LineNo = 2;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}

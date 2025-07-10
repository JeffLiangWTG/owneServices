using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5JGMessageProcessorTest : XMLMessageTestHelper<GOVCBR5JGMessageProcessorTest>
	{
		public void Test5JGWithTypeCodeIs17()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5JG_TypeCodeIs17.xml");
			StatementHeaderTest("110512000000880", incomingMessage);

			var statementHeader = new CusStatementHeader.Loader(Factory).Load("110512000000880", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.Normal);

			Factory.Save();
			AssertEquals(2, statementHeader.StatementLines.Count);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();

			var factory = new BusinessObjectFactory();
			statementHeader = factory.Load<CusStatementHeader>(statementHeader.PK);

			AssertEquals("110512000000880", statementHeader.B2_StatementNumber);
			AssertEquals("20201016", statementHeader.B2_DueDate.ToString(DateFormatType.Date));
			AssertEquals("20201001", statementHeader.B2_ProcessDate.ToString(DateFormatType.Date));
			AssertEquals(7000m, statementHeader.B2_StatementAmount);
			AssertEquals("PYI", statementHeader.B2_PaymentStatus);
			AssertEquals("U", statementHeader.B2_StatementType);
			AssertEquals("110", statementHeader.B2_ProcessPort);
			AssertEquals(true, statementHeader.B2_IsMonthlyStatement);
			AssertEquals("20200901", statementHeader.B2_PeriodStartDate.ToString(DateFormatType.Date));
			AssertEquals("20200930", statementHeader.B2_PeriodEndDate.ToString(DateFormatType.Date));
			AssertEquals("79002792961104", statementHeader.B2_AccountNo);
			AssertEquals(GlbBranch.CurrentBranch.Company.PK, statementHeader.B2_GC);

			AssertEquals(1, statementHeader.StatementLines.Count);
			AssertEquals("110512000000880", statementHeader.StatementLines[0].B3_EntryNum);
			AssertEquals("OTH", statementHeader.StatementLines[0].B3_EntryType);
			AssertEquals(7000m, statementHeader.StatementLines[0].B3_CustomsFeesTotal);
			AssertEquals(statementHeader.PK, statementHeader.StatementLines[0].B3_B2);

			var statementLineCharge = statementHeader.StatementLines[0].Charges;
			AssertEquals(2, statementHeader.StatementLines[0].Charges.Count);
			AssertEquals("DIF", statementLineCharge[0].B4_ChargeType);
			AssertEquals(3500m, statementLineCharge[0].B4_ChargeAmount);
			AssertEquals(statementHeader.StatementLines[0].PK, statementLineCharge[0].B4_B3);

			AssertEquals("PAF", statementLineCharge[1].B4_ChargeType);
			AssertEquals(3500m, statementLineCharge[1].B4_ChargeAmount);
			AssertEquals(statementHeader.StatementLines[0].PK, statementLineCharge[1].B4_B3);

			AssertEquals(statementHeader.PK, incomingMessage.EM_LinkUniqueID);

			AssertContains("(주)엠아이씨텍글로벌", statementHeader.PayerFromCustoms);
			AssertContains("경기 성남시 분당구 황새울로240번길3 (현대오피스빌딩)", statementHeader.PayerFromCustoms);
			AssertContains("쑹까오신밍", statementHeader.PayerFromCustoms);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("[수입대체경비 고지내역 통보] Response for 발행번호: 110512000000880", email.Subject);
			var recipient = email.Recipients[0];
			AssertEquals("ImportStatementEmailGroupTest@wisetechglobal.com", recipient.Email);
			AssertContains("110-51-20-00000880", email.Body);
			AssertContains("(주)엠아이씨텍글로벌", email.Body);
			AssertContains("쑹까오신밍", email.Body);
			AssertContains("경기 성남시 분당구 황새울로240번길3 (현대오피스빌딩)", email.Body);
			AssertContains("울산세관", email.Body);
			AssertContains("2020-10-01", email.Body);
			AssertContains("79002792961104", email.Body);
			AssertContains("160034", email.Body);
			AssertContains("3500", email.Body);
			AssertContains("7000", email.Body);
			AssertContains("1127-110-51-20-00000880", email.Body);
			AssertContains("41634", email.Body);

			AssertContains("110-51-20-00000880", incomingMessage.EM_MessageInterpretation);
			AssertContains("(주)엠아이씨텍글로벌", incomingMessage.EM_MessageInterpretation);
			AssertContains("쑹까오신밍", incomingMessage.EM_MessageInterpretation);
			AssertContains("경기 성남시 분당구 황새울로240번길3 (현대오피스빌딩)", incomingMessage.EM_MessageInterpretation);
			AssertContains("울산세관", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-10-01", incomingMessage.EM_MessageInterpretation);
			AssertContains("79002792961104", incomingMessage.EM_MessageInterpretation);
			AssertContains("160034", incomingMessage.EM_MessageInterpretation);
			AssertContains("3500", email.Body);
			AssertContains("7000", email.Body);
			AssertContains("1127-110-51-20-00000880", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634", incomingMessage.EM_MessageInterpretation);
		}

		public void Test5JGWithTypeCodeIs11()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5JG_TypeCodeIs11.xml");
			AssertNull(new CusStatementHeader.Loader(Factory).Load("110512000000880", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.Normal));
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			var statementHeader = new CusStatementHeader.Loader(Factory).Load("110512000000880", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.Normal);
			AssertNotNull(statementHeader);
			var statementLine = statementHeader.StatementLines;
			AssertEquals(1, statementLine.Count);
			var statementLineCharge = statementLine[0].Charges;

			AssertEquals("110512000000880", statementHeader.B2_StatementNumber);
			AssertEquals("20201016", statementHeader.B2_DueDate.ToString(DateFormatType.Date));
			AssertEquals("20201001", statementHeader.B2_ProcessDate.ToString(DateFormatType.Date));
			AssertEquals(7000m, statementHeader.B2_StatementAmount);
			AssertEquals("PYI", statementHeader.B2_PaymentStatus);
			AssertEquals("U", statementHeader.B2_StatementType);
			AssertEquals("110", statementHeader.B2_ProcessPort);
			AssertEquals(true, statementHeader.B2_IsMonthlyStatement);
			AssertEquals("79002792961104", statementHeader.B2_AccountNo);
			AssertEquals("20200901", statementHeader.B2_PeriodStartDate.ToString(DateFormatType.Date));
			AssertEquals("20200930", statementHeader.B2_PeriodEndDate.ToString(DateFormatType.Date));
			AssertEquals(GlbBranch.CurrentBranch.Company.PK, statementHeader.B2_GC);

			AssertEquals("110512000000880", statementLine[0].B3_EntryNum);
			AssertEquals("OTH", statementLine[0].B3_EntryType);
			AssertEquals(7000m, statementLine[0].B3_CustomsFeesTotal);
			AssertEquals(statementHeader.PK, statementLine[0].B3_B2);

			AssertEquals(1, statementLineCharge.Count);
			AssertEquals("TOF", statementLineCharge[0].B4_ChargeType);
			AssertEquals(7000m, statementLineCharge[0].B4_ChargeAmount);
			AssertEquals(statementLine[0].PK, statementLineCharge[0].B4_B3);

			AssertEquals(statementHeader.PK, incomingMessage.EM_LinkUniqueID);

			AssertContains("(주)엠아이씨텍글로벌", statementHeader.PayerFromCustoms);
			AssertContains("경기 성남시 분당구 황새울로240번길3 (현대오피스빌딩)", statementHeader.PayerFromCustoms);
			AssertContains("쑹까오신밍", statementHeader.PayerFromCustoms);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("[수입대체경비 고지내역 통보] Response for 발행번호: 110512000000880", email.Subject);
			var recipient = email.Recipients[0];
			AssertEquals("ImportStatementEmailGroupTest@wisetechglobal.com", recipient.Email);
			AssertContains("110-51-20-00000880", email.Body);
			AssertContains("(주)엠아이씨텍글로벌", email.Body);
			AssertContains("쑹까오신밍", email.Body);
			AssertContains("경기 성남시 분당구 황새울로240번길3 (현대오피스빌딩)", email.Body);
			AssertContains("울산세관", email.Body);
			AssertContains("2020-10-01", email.Body);
			AssertContains("타소장치 수수료", email.Body);
			AssertContains("79002792961104", email.Body);
			AssertContains("160034", email.Body);
			AssertContains("7000", email.Body);
			AssertContains("0", email.Body);
			AssertContains("1127-110-51-20-00000880", email.Body);
			AssertContains("41634", email.Body);

			AssertContains("110-51-20-00000880", incomingMessage.EM_MessageInterpretation);
			AssertContains("(주)엠아이씨텍글로벌", incomingMessage.EM_MessageInterpretation);
			AssertContains("쑹까오신밍", incomingMessage.EM_MessageInterpretation);
			AssertContains("경기 성남시 분당구 황새울로240번길3 (현대오피스빌딩)", incomingMessage.EM_MessageInterpretation);
			AssertContains("울산세관", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-10-01", incomingMessage.EM_MessageInterpretation);
			AssertContains("79002792961104", incomingMessage.EM_MessageInterpretation);
			AssertContains("160034", incomingMessage.EM_MessageInterpretation);
			AssertContains("7000", incomingMessage.EM_MessageInterpretation);
			AssertContains("0", incomingMessage.EM_MessageInterpretation);
			AssertContains("1127-110-51-20-00000880", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634", incomingMessage.EM_MessageInterpretation);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "T1";
			staff.GS_LoginName = "Test1";
			staff.GS_EmailAddress = "ImportStatementEmailGroupTest@wisetechglobal.com";
			importStatementEmailGroup = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = importStatementEmailGroup.PK;
			link1.GK_GS = staff.PK;

			Factory.Save();
		}
		GlbGroup importStatementEmailGroup;

		void StatementHeaderTest(ZString statementNumber, EDIMessage message)
		{
			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = statementNumber;
			statement.B2_StatementType = "U";
			statement.B2_GC = message.Branch.GB_GC;

			var statementLine1 = statement.StatementLines.AddNew();
			statementLine1.B3_EntryNum = "110512000000880";
			statementLine1.B3_EntryType = Constants.EntryTypeForStatementLine.OtherImportCost;
			var statementLine2 = statement.StatementLines.AddNew();
			statementLine2.B3_EntryNum = "110512000000880";
			statementLine2.B3_EntryType = Constants.EntryTypeForStatementLine.OtherImportCost;
			var statementCharge1 = statementLine1.Charges.AddNew();
			var statementCharge2 = statementLine2.Charges.AddNew();
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5JGMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5JG;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}

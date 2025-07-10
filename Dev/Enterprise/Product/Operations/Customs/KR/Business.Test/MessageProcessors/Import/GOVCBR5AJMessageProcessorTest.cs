using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5AJMessageProcessorTest : XMLMessageTestHelper<GOVCBR5AJMessageProcessorTest>
	{
		public void TestCaseIs5AC()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5AJ_5AC.xml");
			Factory.Save();

			var statement5JG = Factory.New<CusStatementHeader>();
			statement5JG.B2_StatementNumber = "030511900081007";
			statement5JG.B2_GC = GlbBranch.CurrentBranch.Company.PK;
			statement5JG.B2_StatementType = StatementHeaderTypeList.Codes.Normal;
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			statement5JG.Reload();
			AssertEquals("PYC", statement5JG.B2_PaymentStatus);
			#region StatementHeader
			var statementHeader = new CusStatementHeader.Loader(Factory).Load("030190079807", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.NormalReport);
			AssertEquals("030190079807", statementHeader.B2_StatementNumber);
			AssertEquals(39600m, statementHeader.B2_StatementAmount);
			AssertEquals("PYC", statementHeader.B2_PaymentStatus);
			AssertEquals("B", statementHeader.B2_StatementType);
			AssertEquals("030", statementHeader.B2_ProcessPort);
			AssertEquals(true, statementHeader.B2_IsMonthlyStatement);
			AssertEquals("030511900081007", statementHeader.B2_AccountNo);
			AssertEquals("", statementHeader.B2_PaymentParty);
			AssertEquals("6108500065", statementHeader.B2_ImporterCustomsID);
			AssertEquals(GlbBranch.CurrentBranch.Company.PK, statementHeader.B2_GC);
			#endregion

			#region StatementLine
			var statementLine = statementHeader.StatementLines;
			AssertEquals("1314719080103M", statementLine[0].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Export, statementLine[0].B3_EntryType);
			AssertEquals(19800m, statementLine[0].B3_CustomsFeesTotal);
			AssertEquals(statementHeader.PK, statementLine[0].B3_B2);

			AssertEquals("1314719080102M", statementLine[1].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Export, statementLine[1].B3_EntryType);
			AssertEquals(0m, statementLine[1].B3_CustomsFeesTotal);
			AssertEquals(statementHeader.PK, statementLine[1].B3_B2);

			AssertEquals("1314719080101M", statementLine[2].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Export, statementLine[2].B3_EntryType);
			AssertEquals(0m, statementLine[2].B3_CustomsFeesTotal);
			AssertEquals(statementHeader.PK, statementLine[2].B3_B2);

			AssertEquals("1314719090020M", statementLine[3].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Export, statementLine[3].B3_EntryType);
			AssertEquals(19800m, statementLine[3].B3_CustomsFeesTotal);
			AssertEquals(statementHeader.PK, statementLine[3].B3_B2);

			AssertEquals("1314719090019M", statementLine[4].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Export, statementLine[4].B3_EntryType);
			AssertEquals(0m, statementLine[4].B3_CustomsFeesTotal);
			AssertEquals(statementHeader.PK, statementLine[4].B3_B2);
			#endregion

			#region StatementLine.B3_CustomsFeesTotal is over Zero, Created a StatemeneLineCharge
			var createChareLine = statementLine.Where(x => x.B3_CustomsFeesTotal > 0).ToArray();
			AssertEquals(2, createChareLine.Length);

			var statementLineCharge1 = createChareLine[0].Charges;
			AssertEquals("DIF", statementLineCharge1[0].B4_ChargeType);
			AssertEquals(19800m, statementLineCharge1[0].B4_ChargeAmount);
			AssertEquals(createChareLine[0].PK, statementLineCharge1[0].B4_B3);

			var statementLineCharge2 = createChareLine[1].Charges;
			AssertEquals("DIF", statementLineCharge2[0].B4_ChargeType);
			AssertEquals(19800m, statementLineCharge2[0].B4_ChargeAmount);
			AssertEquals(createChareLine[1].PK, statementLineCharge2[0].B4_B3);
			#endregion

			#region Done Statement2 Test.
			AssertEquals("PYC", statement5JG.B2_PaymentStatus);
			AssertEquals("030190079807", statement5JG.B2_CheckNo);
			#endregion

			#region incomingMessage.EM_MessageInterpretation
			AssertContains("030-19-0079807", incomingMessage.EM_MessageInterpretation);
			AssertContains("13147", incomingMessage.EM_MessageInterpretation);
			AssertContains("6108500065", incomingMessage.EM_MessageInterpretation);
			AssertContains("수출신고번호", incomingMessage.EM_MessageInterpretation);
			AssertContains("보세운송 임시개청", incomingMessage.EM_MessageInterpretation);
			AssertContains("13147-19-0800001U", incomingMessage.EM_MessageInterpretation);
			AssertContains("030-51-19-00081007", incomingMessage.EM_MessageInterpretation);

			AssertContains("1314719080103M", incomingMessage.EM_MessageInterpretation);
			AssertContains("1314719080102M", incomingMessage.EM_MessageInterpretation);
			AssertContains("1314719080101M", incomingMessage.EM_MessageInterpretation);
			AssertContains("1314719090020M", incomingMessage.EM_MessageInterpretation);
			AssertContains("1314719090019M", incomingMessage.EM_MessageInterpretation);
			AssertContains("19800", incomingMessage.EM_MessageInterpretation);
			AssertContains("0", incomingMessage.EM_MessageInterpretation);
			#endregion

			#region Email Body
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("030-19-0079807", email.Body);
			AssertContains("13147", email.Body);
			AssertContains("6108500065", email.Body);
			AssertContains("수출신고번호", email.Body);
			AssertContains("보세운송 임시개청", email.Body);
			AssertContains("13147-19-0800001U", email.Body);
			AssertContains("030-51-19-00081007", email.Body);

			AssertContains("1314719080103M", email.Body);
			AssertContains("1314719080102M", email.Body);
			AssertContains("1314719080101M", email.Body);
			AssertContains("1314719090020M", email.Body);
			AssertContains("1314719090019M", email.Body);
			AssertContains("19800", email.Body);
			AssertContains("0", email.Body);
			#endregion
		}
		public void TestCaseIs5GW()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5AJ_5GW.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();

			#region StatementHeader
			var statementHeader = new CusStatementHeader.Loader(Factory).Load("030190079807", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.NormalReport);
			AssertEquals("030190079807", statementHeader.B2_StatementNumber);
			AssertEquals(39600m, statementHeader.B2_StatementAmount);
			AssertEquals("PYC", statementHeader.B2_PaymentStatus);
			AssertEquals("B", statementHeader.B2_StatementType);
			AssertEquals("030", statementHeader.B2_ProcessPort);
			AssertEquals(true, statementHeader.B2_IsMonthlyStatement);
			AssertEquals("030511900081007", statementHeader.B2_AccountNo);
			AssertEquals("", statementHeader.B2_PaymentParty);
			AssertEquals("6108500065", statementHeader.B2_ImporterCustomsID);
			AssertEquals(GlbBranch.CurrentBranch.Company.PK, statementHeader.B2_GC);
			#endregion

			#region StatementLine
			var statementLine = statementHeader.StatementLines;
			AssertEquals("1314719080103M", statementLine[0].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, statementLine[0].B3_EntryType);
			AssertEquals(19800m, statementLine[0].B3_CustomsFeesTotal);
			AssertEquals(statementHeader.PK, statementLine[0].B3_B2);

			AssertEquals("1314719080102M", statementLine[1].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, statementLine[1].B3_EntryType);
			AssertEquals(0m, statementLine[1].B3_CustomsFeesTotal);
			AssertEquals(statementHeader.PK, statementLine[1].B3_B2);

			AssertEquals("1314719080101M", statementLine[2].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, statementLine[2].B3_EntryType);
			AssertEquals(0m, statementLine[2].B3_CustomsFeesTotal);
			AssertEquals(statementHeader.PK, statementLine[2].B3_B2);

			AssertEquals("1314719090020M", statementLine[3].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, statementLine[3].B3_EntryType);
			AssertEquals(19800m, statementLine[3].B3_CustomsFeesTotal);
			AssertEquals(statementHeader.PK, statementLine[3].B3_B2);

			AssertEquals("1314719090019M", statementLine[4].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, statementLine[4].B3_EntryType);
			AssertEquals(0m, statementLine[4].B3_CustomsFeesTotal);
			AssertEquals(statementHeader.PK, statementLine[4].B3_B2);
			#endregion

			#region StatementLine.B3_CustomsFeesTotal is over Zero, Created a StatemeneLineCharge
			var createChareLine = statementLine.Where(x => x.B3_CustomsFeesTotal > 0).ToArray();
			AssertEquals(2, createChareLine.Length);

			var statementLineCharge1 = createChareLine[0].Charges;
			AssertEquals("DIF", statementLineCharge1[0].B4_ChargeType);
			AssertEquals(19800m, statementLineCharge1[0].B4_ChargeAmount);
			AssertEquals(createChareLine[0].PK, statementLineCharge1[0].B4_B3);

			var statementLineCharge2 = createChareLine[1].Charges;
			AssertEquals("DIF", statementLineCharge2[0].B4_ChargeType);
			AssertEquals(19800m, statementLineCharge2[0].B4_ChargeAmount);
			AssertEquals(createChareLine[1].PK, statementLineCharge2[0].B4_B3);
			#endregion

			#region incomingMessage.EM_MessageInterpretation
			AssertContains("030-19-0079807", incomingMessage.EM_MessageInterpretation);
			AssertContains("13147", incomingMessage.EM_MessageInterpretation);
			AssertContains("6108500065", incomingMessage.EM_MessageInterpretation);
			AssertContains("관세사용 수입대체경비", incomingMessage.EM_MessageInterpretation);
			AssertContains("13147-19-0800001U", incomingMessage.EM_MessageInterpretation);
			AssertContains("030-51-19-00081007", incomingMessage.EM_MessageInterpretation);

			AssertContains("1314719080103M", incomingMessage.EM_MessageInterpretation);
			AssertContains("1314719080102M", incomingMessage.EM_MessageInterpretation);
			AssertContains("1314719080101M", incomingMessage.EM_MessageInterpretation);
			AssertContains("1314719090020M", incomingMessage.EM_MessageInterpretation);
			AssertContains("1314719090019M", incomingMessage.EM_MessageInterpretation);
			AssertContains("19800", incomingMessage.EM_MessageInterpretation);
			AssertContains("0", incomingMessage.EM_MessageInterpretation);
			#endregion

			#region Email
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("030-19-0079807", email.Body);
			AssertContains("13147", email.Body);
			AssertContains("6108500065", email.Body);
			AssertContains("수입신고번호", email.Body);
			AssertContains("관세사용 수입대체경비", email.Body);
			AssertContains("13147-19-0800001U", email.Body);
			AssertContains("030-51-19-00081007", email.Body);

			AssertContains("1314719080103M", email.Body);
			AssertContains("1314719080102M", email.Body);
			AssertContains("1314719080101M", email.Body);
			AssertContains("1314719090020M", email.Body);
			AssertContains("1314719090019M", email.Body);
			AssertContains("19800", email.Body);
			AssertContains("0", email.Body);
			#endregion
		}

		public void Test5AJLoadOrgHeader()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5AJ_5AC.xml");
			var payer = CreatePayer("BUS", "6108500065");

			var statement5JG = Factory.New<CusStatementHeader>();
			statement5JG.B2_StatementNumber = "030190079807";
			statement5JG.B2_GC = GlbBranch.CurrentBranch.Company.PK;
			statement5JG.B2_StatementType = StatementHeaderTypeList.Codes.NormalReport;
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			statement5JG.Reload();
			var cusCodes = new OrgCusCode.Loader(Factory).Load(Core.Constants.CountryCodes.KoreaSouth, IdentificationType.BusinessRegNo, "6108500065");
			AssertEquals(cusCodes[0].OK_OH, statement5JG.B2_OH_Importer);
		}

		public void Test5AJLoadPayer()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5AJ_5AC.xml");
			CreatePayer("BUS", "6108500065");
			CreatePayer("NAT", "6108500065");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "1314719080103M";

			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "모나리자(주)");
			declaration.JE_PaidBy = "";
			declaration.JE_OH_DutyPayer = payer.PK;

			var statement5JG = Factory.New<CusStatementHeader>();
			statement5JG.B2_StatementNumber = "030190079807";
			statement5JG.B2_GC = GlbBranch.CurrentBranch.Company.PK;
			statement5JG.B2_StatementType = StatementHeaderTypeList.Codes.NormalReport;
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			statement5JG.Reload();
			AssertEquals(payer.PK, statement5JG.B2_OH_Importer);
		}

		[TestDate(2022, 03, 24)]
		public void Test5AjProcessDate()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5AJ_5AC.xml");
			var statement5JG = Factory.New<CusStatementHeader>();
			statement5JG.B2_StatementNumber = "030190079807";
			statement5JG.B2_GC = GlbBranch.CurrentBranch.Company.PK;
			statement5JG.B2_StatementType = StatementHeaderTypeList.Codes.NormalReport;
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			statement5JG.Reload();
			AssertEquals(new ZDateTime(2022, 03, 24), statement5JG.B2_ProcessDate);
		}

		public void TestCaseIsLengthOverTen()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5AJ_LengthIsOverTen.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();

			#region StatementHeader
			var statementHeader = new CusStatementHeader.Loader(Factory).Load("030190079807", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.NormalReport);
			AssertEquals("030190079807", statementHeader.B2_StatementNumber);
			AssertEquals(39600m, statementHeader.B2_StatementAmount);
			AssertEquals("PYC", statementHeader.B2_PaymentStatus);
			AssertEquals("B", statementHeader.B2_StatementType);
			AssertEquals("030", statementHeader.B2_ProcessPort);
			AssertEquals(true, statementHeader.B2_IsMonthlyStatement);
			AssertEquals("030511900081007", statementHeader.B2_AccountNo);
			AssertEquals("", statementHeader.B2_PaymentParty);
			AssertEquals("6108500065", statementHeader.B2_ImporterCustomsID);
			AssertEquals(GlbBranch.CurrentBranch.Company.PK, statementHeader.B2_GC);
			#endregion

			#region StatementLine
			var statementLine = statementHeader.StatementLines;
			AssertEquals(12, statementLine.Count);
			#endregion

			#region incomingMessage.EM_MessageInterpretation
			AssertNotContains("나머지 내역은 프로그램에서 확인 하십시오.", incomingMessage.EM_MessageInterpretation);
			#endregion

			#region Email
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("나머지 내역은 프로그램에서 확인 하십시오.", email.Body);
			#endregion
		}

		protected override void SetUp()
		{
			base.SetUp();
			importGroup = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "T1";
			staff.GS_LoginName = "Test1";
			staff.GS_EmailAddress = "OriginalSender@wisetechglobal.com";
			var link = Factory.New<GlbGroupLink>();
			link.GK_GG = importGroup.PK;
			link.GK_GS = staff.PK;

			Factory.Save();
		}
		GlbGroup importGroup;
		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5AJMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5AJ;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		OrgAddress CreatePayer(ZString oh_Code, ZString code)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = oh_Code;
			orgHeader.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, code, Core.Constants.CountryCodes.KoreaSouth);

			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "TEST";

			return address;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}

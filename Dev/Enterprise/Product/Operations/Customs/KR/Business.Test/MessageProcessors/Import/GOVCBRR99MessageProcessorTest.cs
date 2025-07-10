using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRR99MessageProcessorTest : XMLMessageTestHelper<GOVCBRR99MessageProcessorTest>
	{
		public void Test929()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._929);
			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._929);
			outgoingMessage.EM_ApplicationReference = "1";
			var incomingMessage = CreateMessageForTest("GOVCBRR99_929.xml");
			SampleCodeType();
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", entry.CH_Status.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("message subType is DeclarationType", ElectronicDocumentTypeList.Codes._929, incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to OAC", CustomsMessageStatusTypeList.Codes.OriginalAccepted, entry.CH_Status);
			AssertEquals("IMP CusEntryHeader.CH_VersionID is updated 929 EDIMessage.EM_ApplicationReference.", outgoingMessage.EM_ApplicationReference, entry.CH_VersionID.ToString());
			AssertEquals("message ApplicationReference is updated to Outgoing Message No.", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals("Current entry.CH_BGMReference is updated", "0127020112001757259", entry.CH_BGMReference);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
			AssertContains("1234520000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("3", incomingMessage.EM_MessageInterpretation);
			AssertContains("11", incomingMessage.EM_MessageInterpretation);
			AssertContains("[02012] 인천세관 수입2과", incomingMessage.EM_MessageInterpretation);
			AssertContains("[143846] 김재관", incomingMessage.EM_MessageInterpretation);
			AssertContains("20150606", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-10-14 10:41:03", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-10-14 10:41:13", incomingMessage.EM_MessageInterpretation);
			AssertContains("0127020112001757259", incomingMessage.EM_MessageInterpretation);
			AssertContains("[003] 협정관세적용신청관련 안내문 기재", incomingMessage.EM_MessageInterpretation);
			AssertContains("001,002란이 협정관세적용가능물품이니 협정관세 신청여부를 재확인 하시기 바랍니다 제001,002란은 특혜관세 적용 대상 가능성이 있으므로 재확인하시기 바랍니다", incomingMessage.EM_MessageInterpretation);

			OtherEntryNumCheck(entry);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입신고서", email.Body);
			AssertContains("1234520000045M", email.Body);
			AssertContains("3", email.Body);
			AssertContains("11", email.Body);
			AssertContains("[02012] 인천세관 수입2과", email.Body);
			AssertContains("[143846] 김재관", email.Body);
			AssertContains("20150606", email.Body);
			AssertContains("2020-10-14 10:41:03", email.Body);
			AssertContains("2020-10-14 10:41:13", email.Body);
			AssertContains("0127020112001757259", email.Body);
			AssertContains("[003] 협정관세적용신청관련 안내문 기재", email.Body);
			AssertContains("001,002란이 협정관세적용가능물품이니 협정관세 신청여부를 재확인 하시기 바랍니다 제001,002란은 특혜관세 적용 대상 가능성이 있으므로 재확인하시기 바랍니다", email.Body);
		}

		public void TestImport929_IsAcceptanceDateTimeSavedToIssueDate()
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._929);
			CreateMessageForTest("GOVCBRR99_929.xml");
			Factory.Save();
			var cusEntryNumber = LoadEntryNumber(entry, "IMP");
			Assert("PreCondition: CE_IssueDate is Empty", cusEntryNumber.CE_IssueDate.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			cusEntryNumber.Reload();
			AssertEquals("CE_IssueDate is updated correctly to Response/Declaration/AcceptanceDateTime value", new ZDateTime("2020-10-14 10:41:00"), cusEntryNumber.CE_IssueDate);
		}

		public void Test929EmptyNoticeNumber()
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._929);
			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._929);
			outgoingMessage.EM_ApplicationReference = "1";
			var incomingMessage = CreateMessageForTest("GOVCBRR99_929_EmptyNoticeNumber.xml");
			SampleCodeType();
			Factory.Save();

			ZQuery query = new ZQuery(CusStatementHeaderSchema.B2_GC, declaration.JE_GC);
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
			var statementCollection = Factory.Load<CusStatementHeader>(query);
			var beforeCount = statementCollection.Length;

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();

			statementCollection = Factory.Load<CusStatementHeader>(query);
			var afterCount = statementCollection.Length;

			AssertEquals(beforeCount, afterCount);
		}

		public void Test929CreateStatementHeader()
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._929);
			AddEntryHeaderCharge(entry, ChargeTypeList.Codes.VAT, 7000m);
			AddEntryHeaderCharge(entry, ChargeTypeList.Codes.SpecialConsumptionTax, 2000m);

			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "R99PAYER", "모나리자(주)");
			declaration.JE_PaidBy = ZString.Empty;
			declaration.JE_OH_DutyPayer = payer.PK;
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._33;
			declaration.JE_CustomsOffice = "020";
			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._929);
			outgoingMessage.EM_ApplicationReference = "1";
			var incomingMessage = CreateMessageForTest("GOVCBRR99_929_012.xml");
			incomingMessage.EM_MessageNum = "R99_929_0";
			SampleCodeType();

			var statement = loadStatement();
			AssertNull(statement);

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			import929.RoundDecimalValueRoundedWithDecimalPlaces();
			entry.Snapshots.RemoveAndDeleteAll();
			using (var stream = KRXmlObjectSerializer.Serialize(import929))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
				Factory.Save();
			}

			var newFactory = new BusinessObjectFactory();
			var payerWrapper = OrgHeaderWrapper.New(newFactory.Load<OrgHeader>(payer.PK));
			payerWrapper.ZO_VATDeferment = ZString.Empty;
			newFactory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();

			statement = loadStatement();
			AssertNotNull(statement);
			AssertEquals(1, statement.Messages.Count);
			AssertEquals(incomingMessage.EM_MessageNum, statement.B2_IncomingMessageNo);
			AssertEquals(new ZDateTime(2020, 10, 14, 10, 41, 00), statement.B2_ProcessDate);
			AssertEquals(new ZDateTime(2015, 06, 06), statement.B2_PrintDate);
			AssertEquals(new ZDateTime(2015, 06, 21), statement.B2_DueDate);
			AssertEquals(2000m, statement.B2_StatementAmount);
			AssertEquals(StatementHeaderStatusList.Codes.Z, statement.B2_Status);
			AssertEquals(StatementHeaderPaymentStatusList.Codes.PYI, statement.B2_PaymentStatus);
			AssertEquals(declaration.JE_OH_DutyPayer, statement.B2_OH_Importer);
			AssertEquals(declaration.JE_CustomsOffice, statement.B2_ProcessPort);
			AssertContains(statement.Messages[0].EM_MessageInterpretation, incomingMessage.EM_MessageInterpretation);

			var statementLine = statement.StatementLines[0];
			AssertEquals(KRJobMessageTypeList.Codes.Import, statementLine.B3_EntryType);
			AssertEquals("1234520000045M", statementLine.B3_EntryNum);
			AssertEquals(1u, statementLine.B3_SequenceNumber);

			newFactory = new BusinessObjectFactory();
			payerWrapper = OrgHeaderWrapper.New(newFactory.Load<OrgHeader>(payer.PK));
			payerWrapper.ZO_VATDeferment = "N1";
			newFactory.Save();

			incomingMessage = CreateMessageForTest("GOVCBRR99_929.xml");
			incomingMessage.EM_MessageNum = "R99_929_1";
			import929 = new ImportEntryHeaderCreator().Create(entry);
			import929.RoundDecimalValueRoundedWithDecimalPlaces();
			entry.Snapshots.RemoveAndDeleteAll();
			using (var stream = KRXmlObjectSerializer.Serialize(import929))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
				Factory.Save();
			}

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			statement = loadStatement();
			AssertEquals(2, statement.Messages.Count);
			AssertEquals(7000m, statement.B2_StatementAmount);
			AssertContains(statement.Messages[1].EM_MessageInterpretation, incomingMessage.EM_MessageInterpretation);

			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._01;
			incomingMessage = CreateMessageForTest("GOVCBRR99_929_012.xml");
			incomingMessage.EM_MessageNum = "R99_929_2";
			import929 = new ImportEntryHeaderCreator().Create(entry);
			import929.RoundDecimalValueRoundedWithDecimalPlaces();
			entry.Snapshots.RemoveAndDeleteAll();
			using (var stream = KRXmlObjectSerializer.Serialize(import929))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
				Factory.Save();
			}

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			statement = loadStatement();
			AssertEquals(3, statement.Messages.Count);
			AssertEquals(9000m, statement.B2_StatementAmount);
			AssertContains(statement.Messages[2].EM_MessageInterpretation, incomingMessage.EM_MessageInterpretation);

			CusStatementHeader loadStatement()
			{
				var newFactory = new BusinessObjectFactory();
				return new CusStatementHeader.Loader(newFactory).Load("0127020112001757259", declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
			}
		}

		public void Test929CreateStatementHeaderEmptyDeclarationDate()
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._929);
			AddEntryHeaderCharge(entry, ChargeTypeList.Codes.VAT, 7000m);
			AddEntryHeaderCharge(entry, ChargeTypeList.Codes.SpecialConsumptionTax, 2000m);

			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "R99PAYER", "모나리자(주)");
			declaration.JE_PaidBy = ZString.Empty;
			declaration.JE_OH_DutyPayer = payer.PK;
			declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._33;
			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._929);
			outgoingMessage.EM_ApplicationReference = "1";
			var incomingMessage = CreateMessageForTest("GOVCBRR99_929_012_EmptyDeclarationDate.xml");
			incomingMessage.EM_MessageNum = "R99_929_0";
			SampleCodeType();

			var statement = loadStatement();
			AssertNull(statement);

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			import929.RoundDecimalValueRoundedWithDecimalPlaces();
			entry.Snapshots.RemoveAndDeleteAll();
			using (var stream = KRXmlObjectSerializer.Serialize(import929))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
				Factory.Save();
			}

			var newFactory = new BusinessObjectFactory();
			var payerWrapper = OrgHeaderWrapper.New(newFactory.Load<OrgHeader>(payer.PK));
			payerWrapper.ZO_VATDeferment = ZString.Empty;
			newFactory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();

			statement = loadStatement();
			AssertNotNull(statement);
			AssertEquals(1, statement.Messages.Count);
			AssertEquals(incomingMessage.EM_MessageNum, statement.B2_IncomingMessageNo);
			AssertEquals(new ZDateTime(2020, 10, 14, 10, 41, 00), statement.B2_ProcessDate);
			Assert(statement.B2_PrintDate.IsEmpty);
			Assert(statement.B2_DueDate.IsEmpty);
			AssertEquals(2000m, statement.B2_StatementAmount);
			AssertEquals(StatementHeaderStatusList.Codes.Z, statement.B2_Status);
			AssertEquals(StatementHeaderPaymentStatusList.Codes.PYI, statement.B2_PaymentStatus);

			CusStatementHeader loadStatement()
			{
				var newFactory = new BusinessObjectFactory();
				return new CusStatementHeader.Loader(newFactory).Load("0127020112001757259", declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
			}
		}

		public void Test929CreateStatementLineCharge()
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._929);
			var entryLine = entry.MergedLines[0];

			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._929);
			outgoingMessage.EM_ApplicationReference = "1";
			var incomingMessage = CreateMessageForTest("GOVCBRR99_929.xml");
			SampleCodeType();

			AddEntryHeaderCharge(entry, ChargeTypeList.Codes.Duty, 10);
			AddEntryHeaderCharge(entry, ChargeTypeList.Codes.SpecialConsumptionTax, 20);
			AddEntryHeaderCharge(entry, ChargeTypeList.Codes.LiquorTax, 30);
			AddEntryHeaderCharge(entry, ChargeTypeList.Codes.TransportationTax, 40);
			AddEntryHeaderCharge(entry, ChargeTypeList.Codes.VAT, 50);
			AddEntryHeaderCharge(entry, ChargeTypeList.Codes.EducationTax, 60);
			AddEntryHeaderCharge(entry, ChargeTypeList.Codes.AgricultureTax, 70);
			AddEntryHeaderCharge(entry, ChargeTypeList.Codes.PenaltyForLateDeclaration, 80);
			AddEntryHeaderCharge(entry, ChargeTypeList.Codes.PenaltyForMissedDeclaration, 90);
			Factory.Save();

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			import929.RoundDecimalValueRoundedWithDecimalPlaces();
			using (var stream = KRXmlObjectSerializer.Serialize(import929))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
				Factory.Save();
			}

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			var statement = new CusStatementHeader.Loader(Factory).Load("0127020112001757259", declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
			AssertNotNull(statement);
			AssertEquals(1, statement.Messages.Count);

			var statementLine = statement.StatementLines[0];
			AssertEquals(KRJobMessageTypeList.Codes.Import, statementLine.B3_EntryType);
			AssertEquals("1234520000045M", statementLine.B3_EntryNum);
			AssertEquals(450m, statementLine.B3_CustomsFeesTotal);

			assertStatementLineCharge(ChargeTypeList.Codes.Duty, 10);
			assertStatementLineCharge(ChargeTypeList.Codes.SpecialConsumptionTax, 20);
			assertStatementLineCharge(ChargeTypeList.Codes.LiquorTax, 30);
			assertStatementLineCharge(ChargeTypeList.Codes.TransportationTax, 40);
			assertStatementLineCharge(ChargeTypeList.Codes.VAT, 50);
			assertStatementLineCharge(ChargeTypeList.Codes.EducationTax, 60);
			assertStatementLineCharge(ChargeTypeList.Codes.AgricultureTax, 70);
			assertStatementLineCharge(ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration, 170);

			void assertStatementLineCharge(string chargeType, decimal amount)
			{
				AssertEquals(amount, statement.StatementLines[0].Charges.Cast<CusStatementLineCharge>()?.FirstOrDefault(x => x.B4_ChargeType == chargeType).B4_ChargeAmount);
			}
		}

		public void Test929CreateStatementLineEmptyChargeType()
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._929);
			var entryLine = entry.MergedLines[0];

			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._929);
			outgoingMessage.EM_ApplicationReference = "1";
			var incomingMessage = CreateMessageForTest("GOVCBRR99_929.xml");
			SampleCodeType();

			AddEntryCharge(ChargeTypeList.Codes.Duty, 10);
			AddEntryCharge("ZZZ", 20);
			Factory.Save();

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			import929.RoundDecimalValueRoundedWithDecimalPlaces();
			using (var stream = KRXmlObjectSerializer.Serialize(import929))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
				Factory.Save();
			}

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			var statement = new CusStatementHeader.Loader(Factory).Load("0127020112001757259", declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);

			AssertEquals(1, statement.StatementLines[0].Charges.Count);
			AssertEquals(10m, statement.StatementLines[0].Charges.Cast<CusStatementLineCharge>()?.FirstOrDefault(x => x.B4_ChargeType == ChargeTypeList.Codes.Duty).B4_ChargeAmount);

			void AddEntryCharge(string chargeType, decimal amount)
			{
				var charge = entry.Charges.AddNew();
				charge.C1_ChargeType = chargeType;
				charge.C1_ChargeAmount = amount;
			}
		}

		public void Test5SM()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("5SM", ElectronicDocumentTypeList.Codes._5SM);
			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5SM);
			var incomingMessage = CreateMessageForTest("GOVCBRR99_5SM.xml");
			SampleCodeType();
			Factory.Save();

			var cusEntryNumber = LoadEntryNumber(entry, ElectronicDocumentTypeList.Codes._5SM);
			Assert("PreCondition: CE_IssueDate is Empty", cusEntryNumber.CE_IssueDate.IsEmpty);

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", entry.CH_Status.IsEmpty);

			AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			incomingMessage.Reload();
			entry.Reload();
			cusEntryNumber.Reload();

			AssertEquals("CE_IssueDate is updated correctly to Response/Declaration/AcceptanceDateTime value", new ZDateTime("2020-10-14 10:41:00"), cusEntryNumber.CE_IssueDate);

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("message subType is DeclarationType", ElectronicDocumentTypeList.Codes._5SM, incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to OAC", CustomsMessageStatusTypeList.Codes.OriginalAccepted, entry.CH_Status);
			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			AssertContains("포괄가격신고서", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("[]", incomingMessage.EM_MessageInterpretation);

			AssertContains("[001] 세관통보사항 기재1", incomingMessage.EM_MessageInterpretation);
			AssertContains("test1", incomingMessage.EM_MessageInterpretation);
			AssertContains("[002] 세관통보사항 기재2", incomingMessage.EM_MessageInterpretation);
			AssertContains("test2", incomingMessage.EM_MessageInterpretation);
			AssertContains("[003] 협정관세적용신청관련 안내문 기재", incomingMessage.EM_MessageInterpretation);
			AssertContains("test3", incomingMessage.EM_MessageInterpretation);
			AssertContains("[004] 미납사항 안내문 기재", incomingMessage.EM_MessageInterpretation);
			AssertContains("test4", incomingMessage.EM_MessageInterpretation);
			AssertContains("[005] 체납관련 정보 등을 기재", incomingMessage.EM_MessageInterpretation);
			AssertContains("test5", incomingMessage.EM_MessageInterpretation);
			AssertContains("[006] 거래품명 관련 안내문 기재", incomingMessage.EM_MessageInterpretation);
			AssertContains("test6", incomingMessage.EM_MessageInterpretation);
			AssertContains("[007] 담보제공대상 관련 안내문 기재", incomingMessage.EM_MessageInterpretation);
			AssertContains("test7", incomingMessage.EM_MessageInterpretation);
			AssertContains("[008] 해외거래처부호 관련 안내문 기재", incomingMessage.EM_MessageInterpretation);
			AssertContains("test8", incomingMessage.EM_MessageInterpretation);
			AssertContains("[009] FTA농림축산물 특별긴급관세조치 알림 메시지 기재", incomingMessage.EM_MessageInterpretation);
			AssertContains("test9", incomingMessage.EM_MessageInterpretation);
			AssertContains("[010] 수정수입세금계산서 미발급 알림메시지", incomingMessage.EM_MessageInterpretation);
			AssertContains("test10", incomingMessage.EM_MessageInterpretation);
			AssertContains("[011]", incomingMessage.EM_MessageInterpretation);
			AssertContains("test11", incomingMessage.EM_MessageInterpretation);
			AssertContains("[012]", incomingMessage.EM_MessageInterpretation);
			AssertContains("test12", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("나머지 내역은 프로그램에서 확인 하십시오.", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("포괄가격신고서", email.Body);
			AssertNotContains("[]", email.Body);

			AssertContains("[001] 세관통보사항 기재1", email.Body);
			AssertContains("test1", email.Body);
			AssertContains("[002] 세관통보사항 기재2", email.Body);
			AssertContains("test2", email.Body);
			AssertContains("[003] 협정관세적용신청관련 안내문 기재", email.Body);
			AssertContains("test3", email.Body);
			AssertContains("[004] 미납사항 안내문 기재", email.Body);
			AssertContains("test4", email.Body);
			AssertContains("[005] 체납관련 정보 등을 기재", email.Body);
			AssertContains("test5", email.Body);
			AssertContains("[006] 거래품명 관련 안내문 기재", email.Body);
			AssertContains("test6", email.Body);
			AssertContains("[007] 담보제공대상 관련 안내문 기재", email.Body);
			AssertContains("test7", email.Body);
			AssertContains("[008] 해외거래처부호 관련 안내문 기재", email.Body);
			AssertContains("test8", email.Body);
			AssertContains("[009] FTA농림축산물 특별긴급관세조치 알림 메시지 기재", email.Body);
			AssertContains("test9", email.Body);
			AssertContains("[010] 수정수입세금계산서 미발급 알림메시지", email.Body);
			AssertContains("test10", email.Body);
			AssertContains("나머지 내역은 프로그램에서 확인 하십시오.", email.Body);
		}

		public void Test5FE()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._5FE, "2");
			entry.CH_BGMReference = "0127020112001123456";

			var messageR99 = entry.Messages.AddNew();
			messageR99.EM_SystemCreateTimeUtc = new ZDateTime(2024, 07, 08);
			messageR99.EM_SystemCreateUser = "ORG";
			messageR99.EM_MessageType = ElectronicDocumentTypeList.Codes._R99;
			messageR99.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5FE;

			var instruction = entry.Declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var amendmentSessionalData = entry.EntryInstruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_LineNo = 2;
			amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.A;
			var penaltyExemptionSessionalData = amendmentSessionalData.PenaltyExemptionSessionalData;
			penaltyExemptionSessionalData.CSI_LineNo = 1;
			penaltyExemptionSessionalData.CSI_Code = DutyPenaltyExemptionCodeList.Codes.Y;

			var cusEntryNum = entry.EntryNumbers.AddNew();
			cusEntryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5UA;
			cusEntryNum.CE_EntryNum = "1234520000070M";
			cusEntryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
			cusEntryNum.CE_EntryLineReference = "1";

			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5FE);
			outgoingMessage.EM_MessageOwner = "";
			var incomingMessage = CreateMessageForTest("GOVCBRR99_5FE.xml");
			SampleCodeType();
			Factory.Save();

			amendmentSessionalData = entry.EntryInstruction.AmendmentSessionalDataCollection.Cast<AmendmentSessionalData>().SingleOrDefault(x => x.CSI_LineNo == 2);
			penaltyExemptionSessionalData = amendmentSessionalData.PenaltyExemptionSessionalData;

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", entry.CH_Status.IsEmpty);
			AssertEquals(ZShort.Zero, entry.CH_VersionID);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, cusEntryNum.CE_EntryStatus);
			Assert("PreCondition: CSI_DateOfIssue is Empty", amendmentSessionalData.CSI_DateOfIssue.IsEmpty);
			Assert("PreCondition: CSI_DateOfIssue is Empty", penaltyExemptionSessionalData.CSI_DateOfIssue.IsEmpty);

			AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			incomingMessage.Reload();
			entry.Reload();
			cusEntryNum.Reload();
			amendmentSessionalData.Reload();
			penaltyExemptionSessionalData.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("message subType is DeclarationType", ElectronicDocumentTypeList.Codes._5FE, incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to AAC", CustomsMessageStatusTypeList.Codes.AmendmentAccepted, entry.CH_Status);
			AssertEquals("IMP CusEntryHeader.CH_VersionID is updated 5FE EDIMessage.EM_ApplicationReference.", outgoingMessage.EM_ApplicationReference, entry.CH_VersionID.ToString());
			AssertEquals("message ApplicationReference is updated to Outgoing Message No.", entry.Messages.LastOutgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, cusEntryNum.CE_EntryStatus);
			AssertEquals("PreCondition: CSI_DateOfIssue is updated correctly to Response/Declaration/AcceptanceDateTime value", new ZDateTime("2020-10-14 10:41:00"), amendmentSessionalData.CSI_DateOfIssue);
			AssertEquals("PreCondition: CSI_DateOfIssue is updated correctly to Response/Declaration/AcceptanceDateTime value", new ZDateTime("2020-10-14 10:41:00"), penaltyExemptionSessionalData.CSI_DateOfIssue);

			var entryLoaded = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);
			AssertEquals("At 5FE, CH_BGMReference was not changed.", "0127020112001123456", entryLoaded.CH_BGMReference);

			AssertContains("수입정정신고서", incomingMessage.EM_MessageInterpretation);

			OtherEntryNumCheck(entry);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입정정신고서", email.Body);

			outgoingMessage.EM_MessageOwner = "1";
			incomingMessage = CreateMessageForTest("GOVCBRR99_5FE.xml");
			Factory.Save();

			AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			cusEntryNum.Reload();

			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, cusEntryNum.CE_EntryStatus);
		}

		[TestDate(2024, 07, 08)]
		public void Test5FEWhenAlready5FEIsSentInThePast()
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._5FE, "2");
			entry.CH_VersionID = 2;

			var message5FE_1 = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5FE);
			var messageR99 = entry.Messages.AddNew();
			messageR99.EM_SystemCreateTimeUtc = new ZDateTime(2024, 07, 07);
			messageR99.EM_SystemCreateUser = "ORG";
			messageR99.EM_MessageType = ElectronicDocumentTypeList.Codes._R99;
			messageR99.EM_MessageSubType = message5FE_1.EM_MessageType;

			var message5FE_2 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5FE, "3");
			var incomingMessage = CreateMessageForTest("GOVCBRR99_5FE.xml");
			Factory.Save();
			messageR99.EM_ApplicationReference = message5FE_1.EM_MessageNum;

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", entry.CH_Status.IsEmpty);
			AssertEquals("PreCondition: Next Customs Version No is 1", 1u, entry.CalculateNextCustoms5FEVersionNumber());
			AssertEquals("PreCondition: VersionID is 2", 2u, entry.CH_VersionID);
			AssertEquals("PreCondition: CW1 Version No is 3", 3u, entry.GetCW1VersionNumberFromCustoms5FEVersionNumber(entry.CalculateNextCustoms5FEVersionNumber(), ZDate.Today));

			AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			incomingMessage.Reload();
			message5FE_2.Reload();
			entry.Reload();

			AssertEquals("Next Customs Version No is 2", 2u, entry.CalculateNextCustoms5FEVersionNumber());
			AssertEquals("VersionID is 3", 3u, entry.CH_VersionID);
			AssertEquals("CW1 Version No is 4", 4u, entry.GetCW1VersionNumberFromCustoms5FEVersionNumber(entry.CalculateNextCustoms5FEVersionNumber(), ZDate.Today));
			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("message subType is DeclarationType", ElectronicDocumentTypeList.Codes._5FE, incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to AAC", CustomsMessageStatusTypeList.Codes.AmendmentAccepted, entry.CH_Status);
			AssertEquals("IMP CusEntryHeader.CH_VersionID is updated 5FE EDIMessage.EM_ApplicationReference.", message5FE_2.EM_ApplicationReference, entry.CH_VersionID.ToString());
			AssertEquals("message ApplicationReference is updated to Outgoing Message No.", message5FE_2.EM_MessageNum, incomingMessage.EM_ApplicationReference);
		}

		public void Test5BF()
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._5BF);
			var incomingMessage = CreateMessageForTest("GOVCBRR99_5BF.xml");
			SampleCodeType();
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", entry.CH_Status.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("message subType is DeclarationType", ElectronicDocumentTypeList.Codes._5BF, incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to CAC", CustomsMessageStatusTypeList.Codes.CancellationAccepted, entry.CH_Status);
			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5BF);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			OtherEntryNumCheck(entry);

			AssertContains("수입취하 신청서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입취하 신청서", email.Body);
		}

		public void Test934()
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", "XXX");
			SampleCodeType();
			var cusEntryNumber = LoadEntryNumber(entry, ElectronicDocumentTypeList.Codes._934);
			var incomingMessageWithNoOriginalMessage = CreateMessageForTest("GOVCBRR99_934.xml");

			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessageWithNoOriginalMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", cusEntryNumber.CE_EntryStatus.IsEmpty);
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessageWithNoOriginalMessage.Reload();
			entry.Reload();
			cusEntryNumber.Reload();
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals("entry is located", entry.PK, incomingMessageWithNoOriginalMessage.EM_LinkUniqueID);
			AssertEquals("CE_ExpiryDate is not updated since original message is null", ZDateTime.Empty, cusEntryNumber.CE_ExpiryDate);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, "XXX");
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._934;
			var fileReader = new TestFileReader(typeof(GOVCBRR99MessageProcessorTest));

			var incomingMessage = CreateMessageForTest("GOVCBRR99_934.xml");
			fileReader = new TestFileReader(typeof(GOVCBRR99MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText("Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing", "GOVCBR934_Empty.xml");
			outgoingMessage.EM_MessageText = messageText;
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();
			cusEntryNumber.Reload();

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("CE_ExpiryDate is not updated since outgoing message does not contain LimitDateTime", ZDateTime.Empty, cusEntryNumber.CE_ExpiryDate);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			messageText = fileReader.GetEmbeddedFileText("Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing", "GOVCBR934_TypeIsA.xml");
			outgoingMessage.EM_MessageText = messageText;
			incomingMessage = CreateMessageForTest("GOVCBRR99_934.xml");
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			cusEntryNumber.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("message subType is DeclarationType", ElectronicDocumentTypeList.Codes._934, incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to OAC", CustomsMessageStatusTypeList.Codes.OriginalAccepted, cusEntryNumber.CE_EntryStatus);
			OtherEntryNumCheck(entry);
			AssertEquals("message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals("entryNum EntryNumber is updated correctly to '0127020112001712345'", "0127020112001712345", cusEntryNumber.CE_EntryNum);
			AssertEquals("CE_IssueDate is updated correctly to Response/Declaration/AcceptanceDateTime value", new ZDateTime("2020-10-14 10:41:00"), cusEntryNumber.CE_IssueDate);
			AssertEquals("CE_ExpiryDate is updated correctly to Estimated Date of Final Price value", new ZDateTime("2020-11-15"), cusEntryNumber.CE_ExpiryDate);

			AssertContains("가격신고서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("가격신고서", email.Body);
		}

		public void Test5BA()
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._5BA, "1");
			var incomingMessage = CreateMessageForTest("GOVCBRR99_5BA.xml");
			SampleCodeType();
			var cusEntryNumber = LoadEntryNumber(entry, ElectronicDocumentTypeList.Codes._5BA);
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", cusEntryNumber.CE_EntryStatus.IsEmpty);
			Assert("PreCondition: Entry Line Reference is Empty", cusEntryNumber.CE_EntryLineReference.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();
			cusEntryNumber.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to OAC", CustomsMessageStatusTypeList.Codes.OriginalAccepted, cusEntryNumber.CE_EntryStatus);
			AssertEquals(new ZDateTime(2020, 10, 14, 10, 41, 00), cusEntryNumber.CE_IssueDate);
			AssertEquals("1", cusEntryNumber.CE_EntryLineReference);
			AssertEquals(ElectronicDocumentTypeList.Codes._5BA, incomingMessage.EM_MessageSubType);

			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5BA);
			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			OtherEntryNumCheck(entry);

			AssertContains("합의세율 신청서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("합의세율 신청서", email.Body);
		}

		public void Test5BB()
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._5BB);
			var incomingMessage = CreateMessageForTest("GOVCBRR99_5BB.xml");
			SampleCodeType();
			var cusEntryNumber_5BA = LoadEntryNumber(entry, ElectronicDocumentTypeList.Codes._5BA);
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", cusEntryNumber_5BA.CE_EntryStatus.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();
			cusEntryNumber_5BA.Reload();

			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5BB);

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to AAC", CustomsMessageStatusTypeList.Codes.AmendmentAccepted, cusEntryNumber_5BA.CE_EntryStatus);
			AssertEquals("entry line reference is updated correctly to EM_ApplicationReference", outgoingMessage.EM_ApplicationReference, cusEntryNumber_5BA.CE_EntryLineReference);
			OtherEntryNumCheck(entry);
			AssertEquals("message ApplicationReference is updated correctly to EM_MessageNum", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals("message subType is DeclarationType", ElectronicDocumentTypeList.Codes._5BB, incomingMessage.EM_MessageSubType);

			AssertContains("합의세율 정정신청서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("합의세율 정정신청서", email.Body);
		}

		public void Test5FN()
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._5FN);
			var message1 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5FN, "1");
			entry.Messages.Add(message1);
			var message2 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5UA, "2");
			entry.Messages.Add(message2);
			var message3 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5FN, "2");
			entry.Messages.Add(message3);
			var incomingMessage = CreateMessageForTest("GOVCBRR99_5FN.xml");
			SampleCodeType();
			var cusEntryNumber001 = LoadEntryNumber(entry, ElectronicDocumentTypeList.Codes._5FN);
			cusEntryNumber001.CE_EntryLineReference = "1";   //check status after receiving message
			cusEntryNumber001.CE_EntryNum = "1234520000045M";
			var cusEntryNumber002 = entry.EntryNumbers.AddNew();
			cusEntryNumber002.CE_EntryType = ElectronicDocumentTypeList.Codes._5UA;
			cusEntryNumber002.CE_EntryLineReference = "2";
			cusEntryNumber002.CE_EntryNum = "1234520000045M";
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", cusEntryNumber001.CE_EntryStatus.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", cusEntryNumber002.CE_EntryStatus.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();
			cusEntryNumber001.Reload();
			cusEntryNumber002.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			Assert("Entry Status is Empty", cusEntryNumber001.CE_EntryStatus.IsEmpty);
			Assert("Entry Status is Empty", cusEntryNumber002.CE_EntryStatus.IsEmpty);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			incomingMessage = CreateMessageForTest("GOVCBRR99_5FN.xml");
			var cusEntryNumber003 = entry.EntryNumbers.AddNew();
			cusEntryNumber003.CE_EntryType = ElectronicDocumentTypeList.Codes._5FN;
			cusEntryNumber003.CE_EntryLineReference = "2";
			cusEntryNumber003.CE_EntryNum = "1234520000045M";
			Factory.Save();
			Assert("PreCondition: Entry Status is Empty", cusEntryNumber003.CE_EntryStatus.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();
			cusEntryNumber003.Reload();

			AssertEquals("entry status is updated correctly to OAC", CustomsMessageStatusTypeList.Codes.OriginalAccepted, cusEntryNumber003.CE_EntryStatus);
			OtherEntryNumCheck(entry);

			AssertContains("감면분납용도세율", incomingMessage.EM_MessageInterpretation);
			AssertContains("1234520000045M", incomingMessage.EM_MessageInterpretation);
			AssertEquals(new ZDateTime(2020, 10, 14, 10, 41, 0), cusEntryNumber003.CE_IssueDate);
			AssertEquals(message3.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("감면분납용도세율", email.Body);
			AssertContains("1234520000045M", email.Body);
		}

		public void TestEM_MessageTypeIs5FN_GetOutGoingMessage()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._5FN);
			var entryNum = LoadEntryNumber(entry, ElectronicDocumentTypeList.Codes._5FN);
			entryNum.CE_EntryNum = "1234520000045M";
			entryNum.CE_EntryLineReference = "2";
			var message1 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5FN, "2");
			entry.Messages.Add(message1);
			var incomingMessage1 = CreateMessageForTest("GOVCBRR99_5FN.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage1.Reload();
			message1.Reload();
			AssertEquals(entry.PK, incomingMessage1.EM_LinkUniqueID);
			AssertEquals(message1.EM_MessageNum, incomingMessage1.EM_ApplicationReference);

			var message2 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5FN, "2");
			entry.Messages.Add(message2);
			var incomingMessage2 = CreateMessageForTest("GOVCBRR99_5FN.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage2.Reload();
			message2.Reload();
			AssertEquals(entry.PK, incomingMessage2.EM_LinkUniqueID);
			AssertEquals(message2.EM_MessageNum, incomingMessage2.EM_ApplicationReference);
		}

		public void Test105()
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._105, "1");
			var incomingMessage = CreateMessageForTest("GOVCBRR99_105.xml");
			SampleCodeType();
			var cusEntryNumber = LoadEntryNumber(entry, ElectronicDocumentTypeList.Codes._5SC);
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", cusEntryNumber.CE_EntryStatus.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();
			cusEntryNumber.Reload();

			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._105);

			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(ElectronicDocumentTypeList.Codes._105, incomingMessage.EM_MessageSubType);

			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, cusEntryNumber.CE_EntryStatus);
			AssertEquals("1", cusEntryNumber.CE_EntryLineReference);

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to AAC", CustomsMessageStatusTypeList.Codes.AmendmentAccepted, cusEntryNumber.CE_EntryStatus);
			OtherEntryNumCheck(entry);

			AssertContains("협정관세 정정신청서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("협정관세 정정신청서", email.Body);
		}

		public void Test5TM()
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._5TM);
			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5TM);
			var incomingMessage = CreateMessageForTest("GOVCBRR99_5TM.xml");
			SampleCodeType();
			var cusEntryNumber = LoadEntryNumber(entry, ElectronicDocumentTypeList.Codes._5TM);
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", cusEntryNumber.CE_EntryStatus.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();
			cusEntryNumber.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to OAC", CustomsMessageStatusTypeList.Codes.OriginalAccepted, cusEntryNumber.CE_EntryStatus);
			OtherEntryNumCheck(entry);

			AssertEquals("message ApplicationReference is updated correctly to EM_MessageNum", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			AssertContains("부가가치세 금거래계좌 납부신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("1234520000045M", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("부가가치세 금거래계좌 납부신청서", email.Body);
			AssertContains("1234520000045M", email.Body);
		}

		public void Test5BD()
		{
			var entry = CreateEntryWithOutgoingMessageForImport(KRJobMessageTypeList.Codes.Import, ElectronicDocumentTypeList.Codes._5BD);
			var entryNum = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5BD);
			var incomingMessage = CreateMessageForTest("GOVCBRR99_5BD.xml");
			SampleCodeType();
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", entry.CH_Status.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();
			entryNum.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("message subType is DeclarationType", ElectronicDocumentTypeList.Codes._5BD, incomingMessage.EM_MessageSubType);
			AssertEquals("entry number status is updated correctly to OAC", CustomsMessageStatusTypeList.Codes.OriginalAccepted, entryNum.CE_EntryStatus);
			AssertEquals("CE_IssueDate is updated correctly to Response/Declaration/AcceptanceDateTime value", new ZDateTime("2020-10-14 10:41:00"), entryNum.CE_IssueDate);
			OtherEntryNumCheck(entry);

			AssertContains(ElectronicDocumentTypeList.Descriptions._5BD, incomingMessage.EM_MessageInterpretation);
			var outgoingMessage = (EDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5BD);
			AssertContains(incomingMessage.EM_ApplicationReference, outgoingMessage.EM_MessageNum);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains(ElectronicDocumentTypeList.Descriptions._5BD, email.Body);
		}

		public void TestDHS()
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._DHS, "1");
			var incomingMessage = CreateMessageForTest("GOVCBRR99_DHS.xml");
			SampleCodeType();
			var cusEntryNumber = LoadEntryNumber(entry, ElectronicDocumentTypeList.Codes._DHR);
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", cusEntryNumber.CE_EntryStatus.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();
			cusEntryNumber.Reload();

			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._DHS);

			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(ElectronicDocumentTypeList.Codes._DHS, incomingMessage.EM_MessageSubType);

			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, cusEntryNumber.CE_EntryStatus);
			AssertEquals("1", cusEntryNumber.CE_EntryLineReference);

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to AAC", CustomsMessageStatusTypeList.Codes.AmendmentAccepted, cusEntryNumber.CE_EntryStatus);
			OtherEntryNumCheck(entry);

			AssertContains("협정관세적용신청 정정신청서(자료교환용)", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("협정관세적용신청 정정신청서(자료교환용)", email.Body);
		}

		public void TestImportR99_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRR99_929.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수입 접수통보]1234520000045M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImportR99_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRR99_929.xml");
				CreateEntryForImport("B00001000", false, "IMP");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입 접수통보] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수입신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}

			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRR99_5SM.xml");
				CreateEntryForImport("B00001001", false, "5SM");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입 접수통보] Response for Declaration Number: B00001001 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [포괄가격신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImportR99_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRR99_929.xml");
				CreateEntryForImport("B00001000", true, "IMP");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입 접수통보] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);
			}

			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRR99_5SM.xml");
				CreateEntryForImport("B00001001", true, "5SM");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입 접수통보] Response for Declaration Number: B00001001 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void Test5SC()
		{
			AssertUpdateIssueDateForType5SCDHR(ElectronicDocumentTypeList.Codes._5SC, "GOVCBRR99_5SC.xml");
			AssertUpdateIssueDateForType5SCDHR_NoEntryNumExist(ElectronicDocumentTypeList.Codes._5SC, "GOVCBRR99_5SC.xml");
		}

		public void TestDHR()
		{
			AssertUpdateIssueDateForType5SCDHR(ElectronicDocumentTypeList.Codes._DHR, "GOVCBRR99_DHR.xml");
			AssertUpdateIssueDateForType5SCDHR_NoEntryNumExist(ElectronicDocumentTypeList.Codes._DHR, "GOVCBRR99_DHR.xml");
		}

		void AssertUpdateIssueDateForType5SCDHR(string messageType, string fileName)
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", messageType, "1");
			var incomingMessage = CreateMessageForTest(fileName);
			var cusEntryNumber = LoadEntryNumber(entry, messageType);
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: CE_IssueDate is Empty", cusEntryNumber.CE_IssueDate.IsEmpty);
			Assert("PreCondition: CE_EntryLineReference is Empty", cusEntryNumber.CE_EntryLineReference.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();
			cusEntryNumber.Reload();

			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, messageType);

			AssertEquals("FTA Version ID is updated to Outgoing EDIMessage.EM_ApplicationReference.", outgoingMessage.EM_ApplicationReference, cusEntryNumber.CE_EntryLineReference);
			AssertEquals("CE_IssueDate is updated Response/IssueDateTime.", new ZDateTime("2020-10-14 10:41:00"), cusEntryNumber.CE_IssueDate);
			AssertEquals("message ApplicationReference is updated correctly to EM_MessageNum", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals("message subType is DeclarationType", messageType, incomingMessage.EM_MessageSubType);
			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
		}

		void AssertUpdateIssueDateForType5SCDHR_NoEntryNumExist(string messageType, string fileName)
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", messageType, "1");
			var incomingMessage = CreateMessageForTest(fileName);
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entry.Reload();

			var cusEntryNumber = LoadEntryNumber(entry, messageType);
			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
		}

		public void TestHighestFTAEntryLineNumberFor5SC()
		{
			AssertFTAEntryLineNumber("GOVCBRR99_5SC.xml");
		}
		public void TestHighestFTAEntryLineNumberFor105()
		{
			AssertFTAEntryLineNumber("GOVCBRR99_105.xml");
		}

		void AssertFTAEntryLineNumber(string fileName)
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._5SC, "1");
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5SC;

			var headerFTA = new ImportFTACreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(headerFTA))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._5SC, stream);
				Factory.Save();
			}

			CreateMessageForTest(fileName);
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 3;
			entryLine.CL_FTASequenceNumber = 3;
			JobComInvoiceLine invoiceLine = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			Factory.Save();
			AssertEquals("Highest FTASequenceNumber", (ZShort)0, entry.CH_HighestFTASequenceNumber);
			AssertEquals(EntrySnapshotStatus.Current, entry.Snapshots[0].CES_Status);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			entry.Snapshots[0].Reload();
			AssertEquals("Highest FTASequenceNumber", (ZShort)2, entry.CH_HighestFTASequenceNumber);
			AssertEquals(EntrySnapshotStatus.Lodged, entry.Snapshots[0].CES_Status);
		}
		public void TestHighestFTAEntryLineNumberForDHR()
		{
			AssertFTAEntryLineNumberDHR("GOVCBRR99_DHR.xml");
		}
		public void TestHighestFTAEntryLineNumberForDHS()
		{
			AssertFTAEntryLineNumberDHR("GOVCBRR99_DHS.xml");
		}
		void AssertFTAEntryLineNumberDHR(string fileName)
		{
			var entry = CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._DHR, "1");
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._DHR;
			entryNumber.CE_EntryNum = "1234520000045M";

			var headerDHR = new ImportDHRCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(headerDHR))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._DHR, stream);
				Factory.Save();
			}

			CreateMessageForTest(fileName);
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 3;
			entryLine.CL_FTASequenceNumber = 3;
			JobComInvoiceLine invoiceLine = declaration.Invoices[0].JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			Factory.Save();

			AssertEquals("Highest FTASequenceNumber", (ZShort)0, entry.CH_HighestFTASequenceNumber);
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			Factory.Save();
			AssertEquals(EntrySnapshotStatus.Current, entry.Snapshots[0].CES_Status);

			entry.Reload();
			entry.Snapshots[0].Reload();
			AssertEquals("Highest FTASequenceNumber", (ZShort)2, entry.CH_HighestFTASequenceNumber);
			AssertEquals(EntrySnapshotStatus.Lodged, entry.Snapshots[0].CES_Status);
		}

		public void TestVATDefermentWhenHasStatementCode012()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._929);
			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "모나리자(주)");
			declaration.JE_OH_DutyPayer = payer.PK;
			CreateMessageForTest("GOVCBRR99_929_012.xml");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var payerWrapper = OrgHeaderWrapper.New(newFactory.Load<OrgHeader>(declaration.JE_OH_DutyPayer));
			AssertEquals("VATDeferment is empty", ZString.Empty, payerWrapper.ZO_VATDeferment);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			newFactory = new BusinessObjectFactory();
			payerWrapper = OrgHeaderWrapper.New(newFactory.Load<OrgHeader>(declaration.JE_OH_DutyPayer));
			AssertEquals("VATDeferment set to Y1", "Y1", payerWrapper.ZO_VATDeferment);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("‘부가세 납부유예업체’ 정보가 없는데, 특이사항에 ‘부가세 납부유예 대상 업체’로 통보되어서,", email.Body);
			AssertContains("납세의무자 회사 정보에 ‘부가세 납부유예업체’로 변경하였습니다.", email.Body);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			payerWrapper.ZO_VATDeferment = "N1";
			newFactory.Save();
			CreateMessageForTest("GOVCBRR99_929_012.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			newFactory = new BusinessObjectFactory();
			payerWrapper = OrgHeaderWrapper.New(newFactory.Load<OrgHeader>(declaration.JE_OH_DutyPayer));
			AssertEquals("VATDeferment set to ??", "??", payerWrapper.ZO_VATDeferment);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("‘부가세 납부유예업체’가 아닌데, 특이사항에 ‘부가세 납부유예 대상 업체’로 통보되었습니다.", email.Body);
			AssertContains("납세의무자 회사 정보를 확인 바랍니다", email.Body);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			payerWrapper.ZO_VATDeferment = "N";
			newFactory.Save();
			CreateMessageForTest("GOVCBRR99_929_012.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			newFactory = new BusinessObjectFactory();
			payerWrapper = OrgHeaderWrapper.New(newFactory.Load<OrgHeader>(declaration.JE_OH_DutyPayer));
			AssertEquals("VATDeferment don't changed", "N", payerWrapper.ZO_VATDeferment);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("‘부가세 납부유예업체’가 ‘N’로 설정되어 있지만, 특이사항에 ‘부가세 납부유예 대상 업체’로 통보되었습니다.", email.Body);
			AssertContains("납세의무자 회사 정보를 확인 바랍니다.", email.Body);
		}

		public void TestVATDefermentWhenHasNotStatementCode012()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._929);
			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "모나리자(주)");
			declaration.JE_OH_DutyPayer = payer.PK;
			CreateMessageForTest("GOVCBRR99_929.xml");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var payerWrapper = OrgHeaderWrapper.New(newFactory.Load<OrgHeader>(declaration.JE_OH_DutyPayer));
			AssertEquals("VATDeferment is empty", ZString.Empty, payerWrapper.ZO_VATDeferment);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			newFactory = new BusinessObjectFactory();
			payerWrapper = OrgHeaderWrapper.New(newFactory.Load<OrgHeader>(declaration.JE_OH_DutyPayer));
			AssertEquals("VATDeferment set to N1", "N1", payerWrapper.ZO_VATDeferment);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("‘부가세 납부유예업체’ 정보가 없는데, 특이사항에 ‘부가세 납부유예 대상 업체’로 통보되어 있지 않아서", email.Body);
			AssertContains("납세의무자 회사정보에 ‘부가세 납부유예업체’가 아닌 것으로 변경하였습니다", email.Body);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			payerWrapper.ZO_VATDeferment = "Y1";
			newFactory.Save();
			CreateMessageForTest("GOVCBRR99_929.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			newFactory = new BusinessObjectFactory();
			payerWrapper = OrgHeaderWrapper.New(newFactory.Load<OrgHeader>(declaration.JE_OH_DutyPayer));
			AssertEquals("VATDeferment set to ??", "??", payerWrapper.ZO_VATDeferment);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("부가세 납부유예업체’인데, 특이사항에 ‘부가세 납부유예 대상 업체’로 통보되지 않았습니다", email.Body);
			AssertContains("납세의무자 회사 정보를 확인 바랍니다.", email.Body);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			payerWrapper.ZO_VATDeferment = "Y";
			newFactory.Save();
			CreateMessageForTest("GOVCBRR99_929.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			newFactory = new BusinessObjectFactory();
			payerWrapper = OrgHeaderWrapper.New(newFactory.Load<OrgHeader>(declaration.JE_OH_DutyPayer));
			AssertEquals("VATDeferment don't changed", "Y", payerWrapper.ZO_VATDeferment);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("부가세 납부유예업체’가 ‘Y’로 설정되어 있지만, 특이사항에 ‘부가세 납부유예 대상 업체’로 통보되지 않았습니다", email.Body);
			AssertContains("납세의무자 회사 정보를 확인 바랍니다.", email.Body);
		}

		void AddEntryHeaderCharge(CusEntryHeader entry, string chargeType, decimal amount)
		{
			var charge = entry.Charges.AddNew();
			charge.C1_ChargeType = chargeType;
			charge.C1_ChargeAmount = amount;
		}

		void SampleCodeType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType("CUSDP", "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSOF", "020", "인천세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSDP", "12", "수입2과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
		}

		CusEntryNumber LoadEntryNumber(CusEntryHeader entry, ZString typeCode)
		{
			var cusEntryNumber = entry.EntryNumbers.GetOrCreateCusEntryNum(typeCode);

			return cusEntryNumber;
		}

		void OtherEntryNumCheck(CusEntryHeader entry)
		{
			var cusEntryNumber = LoadEntryNumber(entry, "IMP");

			AssertEquals("EntryStatus is Empty when EntryType is 'IMP'", ZString.Empty, cusEntryNumber.CE_EntryStatus);
		}

		public void TestNullExceptionThrowForDutyPayer()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryWithOutgoingMessageForImport("IMP", ElectronicDocumentTypeList.Codes._929);
			declaration.JE_PaidBy = ZString.Empty;
			declaration.JE_OH_DutyPayer = Guid.Empty;
			var incomingMessage = CreateMessageForTest("GOVCBRR99_929.xml");
			Factory.Save();

			AssertNull(declaration.DutyPayer);
			AssertNull(declaration.PayerAddress);
			AssertNoExceptionThrown(() => new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage));
		}

		public void TestD72()
		{
			CreateEntryForImport("B00001000", true, "IMP");
			var entryNum_D72 = importEntry.EntryNumbers.AddNew();
			entryNum_D72.CE_EntryType = ElectronicDocumentTypeList.Codes._D72;
			var outgoingMessage1 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._D72, "2");
			var outgoingMessage2 = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._D72, "1");

			var incomingMessage = CreateMessageForTest("GOVCBRR99_D72.xml");
			SampleCodeType();
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entryNum_D72.Reload();
			AssertEquals("message ApplicationReference is updated to Outgoing Message No.", outgoingMessage2.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals("EntryNum LineReference is updated to highest Outgoing Message ApplicationReference", "1", entryNum_D72.CE_EntryLineReference);
		}

		public void TestD72_VersionNumber_1()
		{
			CreateEntryForImport("B00001000", true, "IMP");
			var entryNum_D72 = importEntry.EntryNumbers.AddNew();
			entryNum_D72.CE_EntryType = ElectronicDocumentTypeList.Codes._D72;
			entryNum_D72.CE_EntryLineReference = "2";
			var outgoingMessage = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._D72, "1");

			var incomingMessage = CreateMessageForTest("GOVCBRR99_D72.xml");
			SampleCodeType();
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entryNum_D72.Reload();
			AssertEquals("message ApplicationReference is updated to Outgoing Message No.", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals("EntryNum LineReference is updated to highest Outgoing Message ApplicationReference", "2", entryNum_D72.CE_EntryLineReference);
		}

		public void TestD72_VersionNumber_2()
		{
			CreateEntryForImport("B00001000", true, "IMP");
			var entryNum_D72 = importEntry.EntryNumbers.AddNew();
			entryNum_D72.CE_EntryType = ElectronicDocumentTypeList.Codes._D72;
			entryNum_D72.CE_EntryLineReference = "1";
			var outgoingMessage = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._D72, "2");

			var incomingMessage = CreateMessageForTest("GOVCBRR99_D72_1.xml");
			SampleCodeType();
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			entryNum_D72.Reload();
			AssertEquals("message ApplicationReference is updated to Outgoing Message No.", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals("EntryNum LineReference is updated to highest Outgoing Message ApplicationReference", "2", entryNum_D72.CE_EntryLineReference);
		}

		public void Test5UA()
		{
			CreateEntryForImport("B00001000", true, KRJobMessageTypeList.Codes.Import);
			var oldMessage5UA = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5UA, "1");
			var oldEntryNum5UA = Create5UAEntryNum("1");
			var currentMessage5UA = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5UA, "3");
			var currentEntryNum5UA = Create5UAEntryNum("3");
			oldEntryNum5UA.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			currentEntryNum5UA.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;
			var incomingMessage = CreateMessageForTest("GOVCBRR99_5UA.xml");

			var instruction = importEntry.Declaration.CustomsEntryInstructions.AddNew();
			importEntry.CH_CEI_Instruction = instruction.PK;
			var amendmentSessionalData = importEntry.EntryInstruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.A;
			var penaltyExemptionSessionalData1 = amendmentSessionalData.PenaltyExemptionSessionalData;
			penaltyExemptionSessionalData1.CSI_LineNo = 1;
			penaltyExemptionSessionalData1.CSI_Code = DutyPenaltyExemptionCodeList.Codes.Y;
			amendmentSessionalData = importEntry.EntryInstruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.A;
			var penaltyExemptionSessionalData3 = amendmentSessionalData.PenaltyExemptionSessionalData;
			penaltyExemptionSessionalData3.CSI_LineNo = 3;
			penaltyExemptionSessionalData3.CSI_Code = DutyPenaltyExemptionCodeList.Codes.Y;
			Factory.Save();

			Assert(incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert(incomingMessage.EM_ApplicationReference.IsEmpty);
			Assert(incomingMessage.EM_MessageSubType.IsEmpty);
			Assert(oldEntryNum5UA.CE_IssueDate.IsEmpty);
			Assert(currentEntryNum5UA.CE_IssueDate.IsEmpty);
			Assert(penaltyExemptionSessionalData1.CSI_DateOfIssue.IsEmpty);
			Assert(penaltyExemptionSessionalData3.CSI_DateOfIssue.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			importEntry.Reload();
			oldEntryNum5UA.Reload();
			currentEntryNum5UA.Reload();
			penaltyExemptionSessionalData1.Reload();
			penaltyExemptionSessionalData3.Reload();

			AssertEquals(importEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals(currentMessage5UA.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(ElectronicDocumentTypeList.Codes._5UA, incomingMessage.EM_MessageSubType);

			AssertValueIsEmpty(oldEntryNum5UA);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, currentEntryNum5UA.CE_EntryStatus);
			AssertEquals(new ZDateTime("2020-10-14 10:41:00"), currentEntryNum5UA.CE_IssueDate);
			Assert(penaltyExemptionSessionalData1.CSI_DateOfIssue.IsEmpty);
			AssertEquals(new ZDateTime("2020-10-14 10:41:00"), penaltyExemptionSessionalData3.CSI_DateOfIssue);

			void AssertValueIsEmpty(CusEntryNumber entryNumber)
			{
				Assert(entryNumber.CE_IssueDate.IsEmpty);
			}

			CusEntryNumber Create5UAEntryNum(string entryLineReference)
			{
				var entryNum = importEntry.EntryNumbers.AddNew();
				entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5UA;
				entryNum.CE_EntryLineReference = entryLineReference;
				return entryNum;
			}
		}

		public void TestWhenNoFoundMatched5UAEntryNumAndMessage()
		{
			CreateEntryForImport("B00001000", true, KRJobMessageTypeList.Codes.Import);
			var message5UA = CreateOutgoingMessage(ElectronicDocumentTypeList.Codes._5UA, "1");
			var entryNum5UA = importEntry.EntryNumbers.AddNew();
			entryNum5UA.CE_EntryType = ElectronicDocumentTypeList.Codes._5UA;
			entryNum5UA.CE_EntryLineReference = "1";
			var incomingMessage = CreateMessageForTest("GOVCBRR99_5UA.xml");
			Factory.Save();

			Assert(incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert(incomingMessage.EM_ApplicationReference.IsEmpty);
			Assert(incomingMessage.EM_MessageSubType.IsEmpty);
			Assert(entryNum5UA.CE_EntryStatus.IsEmpty);
			Assert(entryNum5UA.CE_IssueDate.IsEmpty);

			AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			incomingMessage.Reload();
			importEntry.Reload();
			entryNum5UA.Reload();

			AssertEquals(importEntry.PK, incomingMessage.EM_LinkUniqueID);
			Assert(incomingMessage.EM_ApplicationReference.IsEmpty);
			Assert(entryNum5UA.CE_EntryStatus.IsEmpty);
			Assert(entryNum5UA.CE_IssueDate.IsEmpty);
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

		void CreateEntryForImport(string declarationRef, bool setCusAgent, ZString entryType)
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			declaration.JE_OH_DutyPayer = declaration.BrokerAddress?.Header?.PK ?? ZGuid.Empty;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			if (setCusAgent)
			{
				declaration.JE_GS_NKCusAgent = "AG";
			}
			declaration.JE_DeclarationReference = declarationRef;
			importEntry = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			importEntry.CH_CEI_Instruction = entryInstruction.PK;

			var invoice = declaration.Invoices.AddNew();
			var entryLine = importEntry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 2;
			entryLine.CL_FTASequenceNumber = 2;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			entryLine = importEntry.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.CL_FTASequenceNumber = 1;
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var entryNumber = importEntry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "1234520000045M";
			entryNumber.CE_EntryType = entryType;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = importEntry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNumber.CE_EntryStatus = ZString.Empty;
		}
		JobDeclaration declaration;
		CusEntryHeader importEntry;

		CusEntryHeader CreateEntryWithOutgoingMessageForImport(ZString entryType, ZString em_MessageType, string applicationReference = "")
		{
			if (importEntry == null)
			{
				CreateEntryForImport("B00001000", true, entryType);
			}
			CreateOutgoingMessage(em_MessageType, applicationReference);

			return importEntry;
		}

		EDIMessage CreateOutgoingMessage(ZString em_MessageType, string applicationReference = "")
		{
			var outgoingMessage = importEntry.Messages.AddNew();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = em_MessageType;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_ApplicationReference = applicationReference;
			return outgoingMessage;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBRR99MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R99;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
		public string TestOutgoingFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}

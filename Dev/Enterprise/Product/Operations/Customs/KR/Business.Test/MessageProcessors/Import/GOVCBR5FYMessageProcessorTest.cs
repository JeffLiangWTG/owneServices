using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5FYMessageProcessorTest : XMLMessageTestHelper<GOVCBR5FYMessageProcessorTest>
	{
		public void Test5FY()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5FY_0.xml");
			CreatePayer("KRTEST");
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "1078614075", Core.Constants.CountryCodes.KoreaSouth);

			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();

			#region StatementHeader
			var statementHeader = StatementHeaderLoad(incomingMessage);

			AssertEquals("0127030012000018260", statementHeader.B2_StatementNumber);
			AssertEquals("20200930", statementHeader.B2_DueDate.ToString(DateFormatType.Date));
			AssertEquals("20200923", statementHeader.B2_ProcessDate.ToString(DateFormatType.Date));
			AssertEquals(29625339m, statementHeader.B2_StatementAmount);
			AssertEquals(StatementHeaderPaymentStatusList.Codes.PYI, statementHeader.B2_PaymentStatus);
			AssertEquals(StatementHeaderTypeList.Codes.Invoice, statementHeader.B2_StatementType);
			AssertEquals("300", statementHeader.B2_ProcessPort);
			AssertEquals(true, statementHeader.B2_IsMonthlyStatement);
			AssertEquals(PaymentPartyList.Codes.BRK, statementHeader.B2_PaymentParty);
			AssertEquals(GlbCompany.CurrentCompany.OrgProxy.PK, statementHeader.B2_OH_Importer);
			AssertEquals("1078614075", statementHeader.B2_ImporterCustomsID);
			AssertEquals("20200801", statementHeader.B2_PeriodStartDate.ToString(DateFormatType.Date));
			AssertEquals("20200831", statementHeader.B2_PeriodEndDate.ToString(DateFormatType.Date));
			AssertEquals(incomingMessage.Branch.GB_GC, statementHeader.B2_GC);
			#endregion

			#region StatementLine
			var statementLine = statementHeader.StatementLines.FirstOrDefault();

			AssertEquals("4163420017772M", statementLine.B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, statementLine.B3_EntryType);
			AssertEquals(29625339m, statementLine.B3_CustomsFeesTotal);
			AssertEquals((ZShort)1, statementLine.B3_SequenceNumber);
			AssertEquals("0127010111500000001", statementLine.B3_AssociatedEntry);
			AssertEquals(statementHeader.PK, statementLine.B3_B2);
			#endregion

			#region StatementLineCharge
			var statementLineCharges = statementLine.Charges.Cast<CusStatementLineCharge>().ToList();

			AssertEquals(ChargeTypeList.Codes.Duty, statementLineCharges[0].B4_ChargeType);
			AssertEquals(12605040m, statementLineCharges[0].B4_ChargeAmount);
			AssertEquals(ChargeTypeList.Codes.VAT, statementLineCharges[1].B4_ChargeType);
			AssertEquals(17016810m, statementLineCharges[1].B4_ChargeAmount);
			AssertEquals(ChargeTypeList.Codes.LiquorTax, statementLineCharges[2].B4_ChargeType);
			AssertEquals(123m, statementLineCharges[2].B4_ChargeAmount);
			AssertEquals(ChargeTypeList.Codes.AgricultureTax, statementLineCharges[3].B4_ChargeType);
			AssertEquals(456m, statementLineCharges[3].B4_ChargeAmount);
			AssertEquals(ChargeTypeList.Codes.SpecialConsumptionTax, statementLineCharges[4].B4_ChargeType);
			AssertEquals(789m, statementLineCharges[4].B4_ChargeAmount);
			AssertEquals(ChargeTypeList.Codes.TransportationTax, statementLineCharges[5].B4_ChargeType);
			AssertEquals(987m, statementLineCharges[5].B4_ChargeAmount);
			AssertEquals(ChargeTypeList.Codes.EducationTax, statementLineCharges[6].B4_ChargeType);
			AssertEquals(654m, statementLineCharges[6].B4_ChargeAmount);
			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, statementLineCharges[7].B4_ChargeType);
			AssertEquals(321m, statementLineCharges[7].B4_ChargeAmount);
			AssertEquals(ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration, statementLineCharges[8].B4_ChargeType);
			AssertEquals(159m, statementLineCharges[8].B4_ChargeAmount);
			AssertEquals(statementLine.PK, statementLineCharges[0].B4_B3);
			#endregion

			AssertContains("0127-030-01-20-00018260", incomingMessage.EM_MessageInterpretation);
			AssertContains("3512456849562", incomingMessage.EM_MessageInterpretation);
			AssertContains("1078614075", incomingMessage.EM_MessageInterpretation);
			AssertContains("[04] 사업자등록번호", incomingMessage.EM_MessageInterpretation);
			AssertContains("엘지전자(주)", incomingMessage.EM_MessageInterpretation);
			AssertContains("권봉석배두용", incomingMessage.EM_MessageInterpretation);
			AssertContains("서울특별시 영등포구 여의대로 128(여의도동)", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634", incomingMessage.EM_MessageInterpretation);
			AssertContains("수입징수관서", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-30", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-23", incomingMessage.EM_MessageInterpretation);
			AssertContains("12,605,040", incomingMessage.EM_MessageInterpretation);
			AssertContains("17,016,810", incomingMessage.EM_MessageInterpretation);
			AssertContains("123", incomingMessage.EM_MessageInterpretation);
			AssertContains("456", incomingMessage.EM_MessageInterpretation);
			AssertContains("789", incomingMessage.EM_MessageInterpretation);
			AssertContains("987", incomingMessage.EM_MessageInterpretation);
			AssertContains("654", incomingMessage.EM_MessageInterpretation);
			AssertContains("321", incomingMessage.EM_MessageInterpretation);
			AssertContains("159", incomingMessage.EM_MessageInterpretation);
			AssertContains("29,625,339", incomingMessage.EM_MessageInterpretation);

			AssertContains("41634-20-017772M", incomingMessage.EM_MessageInterpretation);
			AssertContains("29,625,339", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("[월별납부 고지 통보] Response for 발행번호: 0127030012000018260", email.Subject);
			var recipient = GroupSourceLocator.GetFromGroup(importGroup);
			AssertEquals("'Post Masters' (code: 'PMG')", recipient.Location.ToString());
			AssertContains("0127-030-01-20-00018260", email.Body);
			AssertContains("3512456849562", email.Body);
			AssertContains("1078614075", email.Body);
			AssertContains("[04] 사업자등록번호", email.Body);
			AssertContains("엘지전자(주)", email.Body);
			AssertContains("권봉석배두용", email.Body);
			AssertContains("서울특별시 영등포구 여의대로 128(여의도동)", email.Body);
			AssertContains("41634", email.Body);
			AssertContains("수입징수관서", email.Body);
			AssertContains("2020-09-30", email.Body);
			AssertContains("2020-09-23", email.Body);
			AssertContains("12,605,040", email.Body);
			AssertContains("17,016,810", email.Body);
			AssertContains("123", email.Body);
			AssertContains("456", email.Body);
			AssertContains("789", email.Body);
			AssertContains("987", email.Body);
			AssertContains("654", email.Body);
			AssertContains("321", email.Body);
			AssertContains("159", email.Body);
			AssertContains("29,625,339", email.Body);

			AssertContains("41634-20-017772M", email.Body);
			AssertContains("29,625,339", email.Body);
		}

		public void Test11Data()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5FY_11Data.xml");

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "4163420017772M";
			entryNumber.CE_EntryType = SharedJobMessageTypeList.Codes.Import;

			var payer1 = CreatePayer("KRTest1");
			CreatePayer("KRTest2");
			declaration.JE_PaidBy = "";
			declaration.JE_OH_DutyPayer = payer1.OA_OH;

			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementNumber = "0127030012000018260";
			header.B2_GC = incomingMessage.Branch.GB_GC;
			header.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			header.B2_DueDate = new ZDate(2020, 10, 10);
			header.B2_ProcessDate = new ZDate(2020, 10, 11);

			var line = header.StatementLines.AddNew();
			line.B3_EntryNum = "1233420017772M";
			line.B3_CustomsFeesTotal = 19283m;
			line.B3_SequenceNumber = 1;

			var charge = line.Charges.AddNew();
			charge.B4_ChargeType = ChargeTypeList.Codes.Duty;
			charge.B4_ChargeAmount = 19283m;

			var line2 = header.StatementLines.AddNew();
			line2.B3_EntryNum = "4163420017772M";
			line2.B3_CustomsFeesTotal = 19283m;
			line2.B3_SequenceNumber = 2;

			var charge1 = line2.Charges.AddNew();
			charge1.B4_ChargeType = ChargeTypeList.Codes.Duty;
			charge1.B4_ChargeAmount = 123m;

			var charge2 = line2.Charges.AddNew();
			charge2.B4_ChargeType = ChargeTypeList.Codes.VAT;
			charge2.B4_ChargeAmount = 456789m;

			Factory.Save();

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("20200930", header.B2_DueDate.ToString(DateFormatType.Date));
			AssertEquals("20200923", header.B2_ProcessDate.ToString(DateFormatType.Date));

			var statementHeader = StatementHeaderLoad(incomingMessage);
			AssertEquals(payer1.OA_OH, statementHeader.B2_OH_Importer);

			var statementLines = statementHeader.StatementLines.Cast<CusStatementLine>().ToList();
			AssertEquals(11, statementLines.Count);

			AssertEquals(true, line.IsDeleted);
			AssertEquals(true, charge.IsDeleted);

			AssertEquals(29621850m, line2.B3_CustomsFeesTotal);
			AssertEquals(29621850m, charge1.B4_ChargeAmount);
			AssertEquals(true, charge2.IsDeleted);

			AssertEquals("4163420017772M", statementLines[0].B3_EntryNum);
			AssertEquals(29621850m, statementLines[0].B3_CustomsFeesTotal);
			AssertEquals((ZShort)1, statementLines[0].B3_SequenceNumber);
			AssertEquals("4163420017704M", statementLines[1].B3_EntryNum);
			AssertEquals(123m, statementLines[1].B3_CustomsFeesTotal);
			AssertEquals((ZShort)2, statementLines[1].B3_SequenceNumber);
			AssertEquals("4163420017701M", statementLines[2].B3_EntryNum);
			AssertEquals(456m, statementLines[2].B3_CustomsFeesTotal);
			AssertEquals((ZShort)3, statementLines[2].B3_SequenceNumber);
			AssertEquals("4163420017703M", statementLines[3].B3_EntryNum);
			AssertEquals(789m, statementLines[3].B3_CustomsFeesTotal);
			AssertEquals((ZShort)4, statementLines[3].B3_SequenceNumber);
			AssertEquals("4163420017709M", statementLines[4].B3_EntryNum);
			AssertEquals(987m, statementLines[4].B3_CustomsFeesTotal);
			AssertEquals((ZShort)5, statementLines[4].B3_SequenceNumber);
			AssertEquals("4163420017708M", statementLines[5].B3_EntryNum);
			AssertEquals(654m, statementLines[5].B3_CustomsFeesTotal);
			AssertEquals((ZShort)6, statementLines[5].B3_SequenceNumber);
			AssertEquals("4163420017706M", statementLines[6].B3_EntryNum);
			AssertEquals(321m, statementLines[6].B3_CustomsFeesTotal);
			AssertEquals((ZShort)7, statementLines[6].B3_SequenceNumber);
			AssertEquals("4163420017702M", statementLines[7].B3_EntryNum);
			AssertEquals(159m, statementLines[7].B3_CustomsFeesTotal);
			AssertEquals((ZShort)8, statementLines[7].B3_SequenceNumber);
			AssertEquals("4163420017707M", statementLines[8].B3_EntryNum);
			AssertEquals(357m, statementLines[8].B3_CustomsFeesTotal);
			AssertEquals((ZShort)9, statementLines[8].B3_SequenceNumber);
			AssertEquals("4163420017705M", statementLines[9].B3_EntryNum);
			AssertEquals(951m, statementLines[9].B3_CustomsFeesTotal);
			AssertEquals((ZShort)10, statementLines[9].B3_SequenceNumber);
			AssertEquals("4163420017179M", statementLines[10].B3_EntryNum);
			AssertEquals(753m, statementLines[10].B3_CustomsFeesTotal);
			AssertEquals((ZShort)11, statementLines[10].B3_SequenceNumber);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, statementLines[0].B3_EntryType);
			AssertEquals(statementHeader.PK, statementLines[0].B3_B2);

			var statementLineCharges = statementLines[0].Charges.Where(x => x.B4_B3 == statementLines[0].PK).ToList();
			AssertEquals(1, statementLineCharges.Count);

			AssertEquals(statementLines[0].PK, statementLineCharges[0].B4_B3);
			AssertEquals(ChargeTypeList.Codes.Duty, statementLineCharges[0].B4_ChargeType);
			AssertEquals(29621850m, statementLineCharges[0].B4_ChargeAmount);

			AssertContains("41634-20-017772M", incomingMessage.EM_MessageInterpretation);
			AssertContains("29,621,850", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-20-017704M", incomingMessage.EM_MessageInterpretation);
			AssertContains("123", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-20-017701M", incomingMessage.EM_MessageInterpretation);
			AssertContains("456", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-20-017703M", incomingMessage.EM_MessageInterpretation);
			AssertContains("789", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-20-017709M", incomingMessage.EM_MessageInterpretation);
			AssertContains("987", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-20-017708M", incomingMessage.EM_MessageInterpretation);
			AssertContains("654", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-20-017706M", incomingMessage.EM_MessageInterpretation);
			AssertContains("321", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-20-017702M", incomingMessage.EM_MessageInterpretation);
			AssertContains("159", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-20-017707M", incomingMessage.EM_MessageInterpretation);
			AssertContains("357", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-20-017705M", incomingMessage.EM_MessageInterpretation);
			AssertContains("951", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("나머지 내역은 프로그램에서 확인 하십시오.", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("[월별납부 고지 통보] Response for 발행번호: 0127030012000018260", email.Subject);
			var recipient = GroupSourceLocator.GetFromGroup(importGroup);
			AssertEquals("'Post Masters' (code: 'PMG')", recipient.Location.ToString());
			AssertContains("41634-20-017772M", email.Body);
			AssertContains("29,621,850", email.Body);
			AssertContains("41634-20-017704M", email.Body);
			AssertContains("123", email.Body);
			AssertContains("41634-20-017701M", email.Body);
			AssertContains("456", email.Body);
			AssertContains("41634-20-017703M", email.Body);
			AssertContains("789", email.Body);
			AssertContains("41634-20-017709M", email.Body);
			AssertContains("987", email.Body);
			AssertContains("41634-20-017708M", email.Body);
			AssertContains("654", email.Body);
			AssertContains("41634-20-017706M", email.Body);
			AssertContains("321", email.Body);
			AssertContains("41634-20-017702M", email.Body);
			AssertContains("159", email.Body);
			AssertContains("41634-20-017707M", email.Body);
			AssertContains("357", email.Body);
			AssertContains("41634-20-017705M", email.Body);
			AssertContains("951", email.Body);
			AssertContains("나머지 내역은 프로그램에서 확인 하십시오.", email.Body);
		}

		public void TestDoneStatement6()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5FY_5Data.xml");

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "4163420017772M";
			entryNumber.CE_EntryType = SharedJobMessageTypeList.Codes.Import;

			var payer1 = CreatePayer("KRTest1");
			CreatePayer("KRTest2");
			declaration.JE_PaidBy = "";
			declaration.JE_OH_DutyPayer = payer1.OA_OH;

			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementNumber = "0127030012000018260";
			header.B2_GC = incomingMessage.Branch.GB_GC;
			header.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			header.B2_DueDate = new ZDate(2020, 10, 10);
			header.B2_ProcessDate = new ZDate(2020, 10, 11);

			var line = header.StatementLines.AddNew();
			line.B3_EntryNum = "1233420017772M";
			line.B3_CustomsFeesTotal = 19283m;
			line.B3_SequenceNumber = 1;

			var line2 = header.StatementLines.AddNew();
			line2.B3_EntryNum = "4163420017772M";
			line2.B3_CustomsFeesTotal = 19283m;
			line2.B3_SequenceNumber = 2;

			Factory.Save();

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			var statementHeader = StatementHeaderLoad(incomingMessage);
			var statementLines = statementHeader.StatementLines.Cast<CusStatementLine>().ToList();
			AssertEquals(5, statementLines.Count);

			var statementLine1 = statementLines[0];
			AssertEquals(1u, statementLine1.B3_SequenceNumber);
			AssertEquals("0127010111500000001", statementLines[0].B3_AssociatedEntry);
			AssertEquals(59243700m, statementLine1.B3_CustomsFeesTotal);
			AssertEquals(2, statementLine1.Charges.Count);
			AssertEquals("DTY", statementLine1.Charges[0].B4_ChargeType);
			AssertEquals(29621850m, statementLine1.Charges[0].B4_ChargeAmount);
			AssertEquals("PMT", statementLine1.Charges[1].B4_ChargeType);
			AssertEquals(29621850m, statementLine1.Charges[1].B4_ChargeAmount);

			var statementLine2 = statementLines[1];
			AssertEquals(2u, statementLine2.B3_SequenceNumber);
			AssertEquals("0127010111500000002", statementLine2.B3_AssociatedEntry);
			AssertEquals(246m, statementLine2.B3_CustomsFeesTotal);
			AssertEquals(2, statementLine2.Charges.Count);
			AssertEquals("DTY", statementLine2.Charges[0].B4_ChargeType);
			AssertEquals(123m, statementLine2.Charges[0].B4_ChargeAmount);
			AssertEquals("TRT", statementLine2.Charges[1].B4_ChargeType);
			AssertEquals(123m, statementLine2.Charges[1].B4_ChargeAmount);

			var statementLine3 = statementLines[2];
			AssertEquals(3u, statementLine3.B3_SequenceNumber);
			AssertEquals("0127010111500000003", statementLine3.B3_AssociatedEntry);
			AssertEquals(912m, statementLine3.B3_CustomsFeesTotal);
			AssertEquals(2, statementLine3.Charges.Count);
			AssertEquals("DTY", statementLine3.Charges[0].B4_ChargeType);
			AssertEquals(456m, statementLine3.Charges[0].B4_ChargeAmount);
			AssertEquals("EDT", statementLine3.Charges[1].B4_ChargeType);
			AssertEquals(456m, statementLine3.Charges[1].B4_ChargeAmount);

			var statementLine4 = statementLines[3];
			AssertEquals(4u, statementLine4.B3_SequenceNumber);
			AssertEquals("0127010111500000004", statementLine4.B3_AssociatedEntry);
			AssertEquals(1578m, statementLine4.B3_CustomsFeesTotal);
			AssertEquals(2, statementLine4.Charges.Count);
			AssertEquals("DTY", statementLine4.Charges[0].B4_ChargeType);
			AssertEquals(789m, statementLine4.Charges[0].B4_ChargeAmount);
			AssertEquals("PLT", statementLine4.Charges[1].B4_ChargeType);
			AssertEquals(789m, statementLine4.Charges[1].B4_ChargeAmount);

			var statementLine5 = statementLines[4];
			AssertEquals(5u, statementLine5.B3_SequenceNumber);
			AssertEquals("0127010111500000005", statementLine5.B3_AssociatedEntry);
			AssertEquals(1974m, statementLine5.B3_CustomsFeesTotal);
			AssertEquals(2, statementLine5.Charges.Count);
			AssertEquals("DTY", statementLine5.Charges[0].B4_ChargeType);
			AssertEquals(987m, statementLine5.Charges[0].B4_ChargeAmount);
			AssertEquals("VAT", statementLine5.Charges[1].B4_ChargeType);
			AssertEquals(987m, statementLine5.Charges[1].B4_ChargeAmount);
		}
		public void TestWithoutDuty()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5FY_WithoutDuty.xml");
			CreatePayer("KRTEST");
			Factory.Save();

			AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());

			incomingMessage.Reload();

			var statementHeader = StatementHeaderLoad(incomingMessage);

			var cusCode = new OrgCusCode.Loader(Factory).Load(Core.Constants.CountryCodes.KoreaSouth, IdentificationType.BusinessRegNo, "1078614075");
			AssertEquals(cusCode[0].OK_OH, statementHeader.B2_OH_Importer);

			var statementLines = statementHeader.StatementLines.Cast<CusStatementLine>();

			AssertEquals(0, statementLines.Count());

			AssertContains("0127-030-01-20-00018260", incomingMessage.EM_MessageInterpretation);
			AssertContains("3512456849562", incomingMessage.EM_MessageInterpretation);
			AssertContains("1078614075", incomingMessage.EM_MessageInterpretation);
			AssertContains("[04] 사업자등록번호", incomingMessage.EM_MessageInterpretation);
			AssertContains("엘지전자(주)", incomingMessage.EM_MessageInterpretation);
			AssertContains("권봉석배두용", incomingMessage.EM_MessageInterpretation);
			AssertContains("서울특별시 영등포구 여의대로 128(여의도동)", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634", incomingMessage.EM_MessageInterpretation);
			AssertContains("수입징수관서", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-30", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-23", incomingMessage.EM_MessageInterpretation);

			AssertNotContains("41634-20-017772M", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("29,625,339", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("[월별납부 고지 통보] Response for 발행번호: 0127030012000018260", email.Subject);
			var recipient = GroupSourceLocator.GetFromGroup(importGroup);
			AssertEquals("'Post Masters' (code: 'PMG')", recipient.Location.ToString());
			AssertContains("0127-030-01-20-00018260", email.Body);
			AssertContains("3512456849562", email.Body);
			AssertContains("1078614075", email.Body);
			AssertContains("[04] 사업자등록번호", email.Body);
			AssertContains("엘지전자(주)", email.Body);
			AssertContains("권봉석배두용", email.Body);
			AssertContains("서울특별시 영등포구 여의대로 128(여의도동)", email.Body);
			AssertContains("41634", email.Body);
			AssertContains("수입징수관서", email.Body);
			AssertContains("2020-09-30", email.Body);
			AssertContains("2020-09-23", email.Body);

			AssertNotContains("41634-20-017772M", email.Body);
			AssertNotContains("29,625,339", email.Body);
		}

		public void TestB2_PaymentPartyIsOWN()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5FY_0.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();

			#region StatementHeader
			var statementHeader = StatementHeaderLoad(incomingMessage);
			AssertEquals("If OrgProxy.CustomsCodes not equals to Response.Declaration.Payer.Id.Value, statementHeader.B2_PaymentParty is update to 'OWN'", PaymentPartyList.Codes.OWN, statementHeader.B2_PaymentParty);
			#endregion
		}

		protected override void SetUp()
		{
			base.SetUp();
			importGroup = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "T1";
			staff1.GS_LoginName = "Test1";
			staff1.GS_EmailAddress = "staff1@wisetechglobal.com";
			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = importGroup.PK;
			link1.GK_GS = staff1.PK;

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "T2";
			staff2.GS_LoginName = "Test2";
			staff2.GS_EmailAddress = "staff2@wisetechglobal.com";
			var link2 = Factory.New<GlbGroupLink>();
			link2.GK_GG = importGroup.PK;
			link2.GK_GS = staff2.PK;

			Factory.Save();
		}
		GlbGroup importGroup;

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5FYMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5FY;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		OrgAddress CreatePayer(ZString oh_Code)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = oh_Code;
			orgHeader.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "1078614075", Core.Constants.CountryCodes.KoreaSouth);

			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "TEST";

			return address;
		}

		CusStatementHeader StatementHeaderLoad(EDIMessage incomingMessage)
		{
			return new CusStatementHeader.Loader(Factory).Load("0127030012000018260", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.Invoice);
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}

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
	sealed class GOVCBR5FZMessageProcessorTest : XMLMessageTestHelper<GOVCBR5FZMessageProcessorTest>
	{
		public void Test5FZ_1_1()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5FZ_0.xml");
			CreatePayer("KRTEST", "6188116919");
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "6188116919", Core.Constants.CountryCodes.KoreaSouth);
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();

			#region StatementHeader
			var statementHeader = new CusStatementHeader.Loader(Factory).Load("0401980012158", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.IndividualCollectionReceipt);

			AssertEquals("0401980012158", statementHeader.B2_StatementNumber);
			AssertEquals("20191201", statementHeader.B2_PeriodStartDate.ToString(DateFormatType.Date));
			AssertEquals("20191231", statementHeader.B2_PeriodEndDate.ToString(DateFormatType.Date));
			AssertEquals(StatementHeaderPaymentStatusList.Codes.PYC, statementHeader.B2_PaymentStatus);
			AssertEquals(StatementHeaderTypeList.Codes.IndividualCollectionReceipt, statementHeader.B2_StatementType);
			AssertEquals("040", statementHeader.B2_ProcessPort);
			AssertEquals(true, statementHeader.B2_IsMonthlyStatement);
			AssertEquals(StatementTypeList.Codes.VEP, statementHeader.B2_PaymentType);
			AssertEquals("", statementHeader.B2_AccountNo);
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy.PK, statementHeader.B2_OH_Importer);
			AssertEquals("6188116919", statementHeader.B2_ImporterCustomsID);
			AssertEquals(PaymentPartyList.Codes.BRK, statementHeader.B2_PaymentParty);
			AssertEquals(GlbBranch.CurrentBranch.Company.PK, statementHeader.B2_GC);
			#endregion

			#region StatementLine
			var statementLine = statementHeader.StatementLines.ToList();
			AssertEquals("1234520000045M", statementLine[0].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, statementLine[0].B3_EntryType);
			AssertEquals("040111919110129", statementLine[0].B3_AssociatedEntry);
			AssertEquals("20191224", statementLine[0].B3_EntryDate.ToString(DateFormatType.Date));
			AssertEquals(0m, statementLine[0].B3_CustomsFeesTotal);
			AssertEquals((ZShort)1, statementLine[0].B3_SequenceNumber);

			AssertEquals("4163419507281M", statementLine[1].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, statementLine[1].B3_EntryType);
			AssertEquals("040111919861587", statementLine[1].B3_AssociatedEntry);
			AssertEquals("20191230", statementLine[1].B3_EntryDate.ToString(DateFormatType.Date));
			AssertEquals(0m, statementLine[1].B3_CustomsFeesTotal);
			AssertEquals((ZShort)2, statementLine[1].B3_SequenceNumber);
			#endregion

			#region StatementLineCharge
			var statementLineCharge1 = statementLine[0].Charges.FirstOrDefault();
			AssertEquals(6158162m, statementLineCharge1.B4_ChargeAmount);
			AssertEquals(ChargeTypeList.Codes.ValueForVAT, statementLineCharge1.B4_ChargeType);

			var statementLineCharge2 = statementLine[1].Charges.FirstOrDefault();
			AssertEquals(10837114m, statementLineCharge2.B4_ChargeAmount);
			AssertEquals(ChargeTypeList.Codes.ValueForVAT, statementLineCharge2.B4_ChargeType);
			#endregion

			#region incomingMessage.EM_MessageInterpretation
			AssertContains("일괄세금계산서", incomingMessage.EM_MessageInterpretation);
			AssertContains("수입계산서(면세분)", incomingMessage.EM_MessageInterpretation);
			AssertContains("040-19-80012158", incomingMessage.EM_MessageInterpretation);
			AssertContains("1218300561", incomingMessage.EM_MessageInterpretation);
			AssertContains("인천세관", incomingMessage.EM_MessageInterpretation);
			AssertContains("인천광역시 중구 서해대로 339 (항동7가)", incomingMessage.EM_MessageInterpretation);
			AssertContains("6188116919", incomingMessage.EM_MessageInterpretation);
			AssertContains("에스케이에프코리아(주)", incomingMessage.EM_MessageInterpretation);
			AssertContains("성석필", incomingMessage.EM_MessageInterpretation);
			AssertContains("부산광역시 강서구 과학산단2로19번길 99(지사동)", incomingMessage.EM_MessageInterpretation);
			AssertContains("2019-12-24", incomingMessage.EM_MessageInterpretation);
			AssertContains("5", incomingMessage.EM_MessageInterpretation);
			AssertContains("16995276", incomingMessage.EM_MessageInterpretation);
			AssertContains("0", incomingMessage.EM_MessageInterpretation);
			AssertContains("2019-12-01 ~ 2019-12-31", incomingMessage.EM_MessageInterpretation);
			AssertContains("Remark", incomingMessage.EM_MessageInterpretation);
			AssertContains("2", incomingMessage.EM_MessageInterpretation);

			AssertContains("개별세금 계산서 내역", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("040111919110129", incomingMessage.EM_MessageInterpretation);
			AssertContains("6158162", incomingMessage.EM_MessageInterpretation);
			AssertContains("0", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-19-507281M", incomingMessage.EM_MessageInterpretation);
			AssertContains("040111919861587", incomingMessage.EM_MessageInterpretation);
			AssertContains("10837114", incomingMessage.EM_MessageInterpretation);
			AssertContains("0", incomingMessage.EM_MessageInterpretation);
			#endregion

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("[수입 세금계산서(월별)] Response for 발행번호: 0401980012158", email.Subject);
			var recipient = GroupSourceLocator.GetFromGroup(importGroup);
			AssertEquals("'Post Masters' (code: 'PMG')", recipient.Location.ToString());
			AssertContains("일괄세금계산서", email.Body);
			AssertContains("수입계산서(면세분)", email.Body);
			AssertContains("040-19-80012158", email.Body);
			AssertContains("1218300561", email.Body);
			AssertContains("인천세관", email.Body);
			AssertContains("인천광역시 중구 서해대로 339 (항동7가)", email.Body);
			AssertContains("6188116919", email.Body);
			AssertContains("에스케이에프코리아(주)", email.Body);
			AssertContains("성석필", email.Body);
			AssertContains("부산광역시 강서구 과학산단2로19번길 99(지사동)", email.Body);
			AssertContains("2019-12-24", email.Body);
			AssertContains("5", email.Body);
			AssertContains("16995276", email.Body);
			AssertContains("0", email.Body);
			AssertContains("2019-12-01 ~ 2019-12-31", email.Body);
			AssertContains("Remark", email.Body);
			AssertContains("2", email.Body);

			AssertContains("개별세금 계산서 내역", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("040111919110129", email.Body);
			AssertContains("6158162", email.Body);
			AssertContains("0", email.Body);
			AssertContains("41634-19-507281M", email.Body);
			AssertContains("040111919861587", email.Body);
			AssertContains("10837114", email.Body);
			AssertContains("0", email.Body);
		}

		public void Test5FZ_1_2()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5FZ_1.xml");
			CreatePayer("KRTEST", "6188116919");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();

			#region StatementHeader
			var statementHeader = new CusStatementHeader.Loader(Factory).Load("0401980012143", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.IndividualCollectionReceipt);

			AssertEquals("0401980012143", statementHeader.B2_StatementNumber);
			AssertEquals("20191201", statementHeader.B2_PeriodStartDate.ToString(DateFormatType.Date));
			AssertEquals("20191231", statementHeader.B2_PeriodEndDate.ToString(DateFormatType.Date));
			AssertEquals(StatementHeaderPaymentStatusList.Codes.PYC, statementHeader.B2_PaymentStatus);
			AssertEquals(StatementHeaderTypeList.Codes.IndividualCollectionReceipt, statementHeader.B2_StatementType);
			AssertEquals("040", statementHeader.B2_ProcessPort);
			AssertEquals(true, statementHeader.B2_IsMonthlyStatement);
			AssertEquals(StatementTypeList.Codes.VPD, statementHeader.B2_PaymentType);
			AssertEquals("", statementHeader.B2_AccountNo);
			var cusCode = new OrgCusCode.Loader(Factory).Load(Core.Constants.CountryCodes.KoreaSouth, IdentificationType.BusinessRegNo, "6188116919")?.FirstOrDefault();
			AssertEquals(cusCode.OK_OH, statementHeader.B2_OH_Importer);
			AssertEquals("6188116919", statementHeader.B2_ImporterCustomsID);
			AssertEquals(PaymentPartyList.Codes.OWN, statementHeader.B2_PaymentParty);
			AssertEquals(GlbBranch.CurrentBranch.Company.PK, statementHeader.B2_GC);
			#endregion

			#region StatementLine
			var statementLine = statementHeader.StatementLines.ToList();

			AssertEquals("1234520000045M", statementLine[0].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, statementLine[0].B3_EntryType);
			AssertEquals("040111919110369", statementLine[0].B3_AssociatedEntry);
			AssertEquals("20191227", statementLine[0].B3_EntryDate.ToString(DateFormatType.Date));
			AssertEquals(676810m, statementLine[0].B3_CustomsFeesTotal);
			AssertEquals((ZShort)1, statementLine[0].B3_SequenceNumber);

			AssertEquals("4163419507229M", statementLine[1].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, statementLine[1].B3_EntryType);
			AssertEquals("040111919861383", statementLine[1].B3_AssociatedEntry);
			AssertEquals("20191228", statementLine[1].B3_EntryDate.ToString(DateFormatType.Date));
			AssertEquals(1083760m, statementLine[1].B3_CustomsFeesTotal);
			AssertEquals((ZShort)2, statementLine[1].B3_SequenceNumber);
			#endregion

			#region StatementLineCharge
			var statementLineCharge1 = statementLine[0].Charges.FirstOrDefault();
			AssertEquals(6768162m, statementLineCharge1.B4_ChargeAmount);
			AssertEquals(ChargeTypeList.Codes.ValueForVAT, statementLineCharge1.B4_ChargeType);

			var statementLineCharge2 = statementLine[1].Charges.FirstOrDefault();
			AssertEquals(10837629m, statementLineCharge2.B4_ChargeAmount);
			AssertEquals(ChargeTypeList.Codes.ValueForVAT, statementLineCharge2.B4_ChargeType);
			#endregion

			#region incomingMessage.EM_MessageInterpretation
			AssertContains("일괄세금계산서", incomingMessage.EM_MessageInterpretation);
			AssertContains("수입세금계산서(과세분)", incomingMessage.EM_MessageInterpretation);
			AssertContains("040-19-80012143", incomingMessage.EM_MessageInterpretation);
			AssertContains("1218300561", incomingMessage.EM_MessageInterpretation);
			AssertContains("인천세관", incomingMessage.EM_MessageInterpretation);
			AssertContains("인천광역시 중구 서해대로 339 (항동7가)", incomingMessage.EM_MessageInterpretation);
			AssertContains("6188116919", incomingMessage.EM_MessageInterpretation);
			AssertContains("에스케이에프코리아(주)", incomingMessage.EM_MessageInterpretation);
			AssertContains("성석필", incomingMessage.EM_MessageInterpretation);
			AssertContains("부산광역시 강서구 과학산단2로19번길 99(지사동)", incomingMessage.EM_MessageInterpretation);
			AssertContains("2019-12-27", incomingMessage.EM_MessageInterpretation);
			AssertContains("5", incomingMessage.EM_MessageInterpretation);
			AssertContains("17605791", incomingMessage.EM_MessageInterpretation);
			AssertContains("1760570", incomingMessage.EM_MessageInterpretation);
			AssertContains("2019-12-01 ~ 2019-12-31", incomingMessage.EM_MessageInterpretation);
			AssertContains("Remark", incomingMessage.EM_MessageInterpretation);
			AssertContains("2", incomingMessage.EM_MessageInterpretation);

			AssertContains("개별세금 계산서 내역", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("040111919110369", incomingMessage.EM_MessageInterpretation);
			AssertContains("6768162", incomingMessage.EM_MessageInterpretation);
			AssertContains("676810", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-19-507229M", incomingMessage.EM_MessageInterpretation);
			AssertContains("040111919861383", incomingMessage.EM_MessageInterpretation);
			AssertContains("10837629", incomingMessage.EM_MessageInterpretation);
			AssertContains("1083760", incomingMessage.EM_MessageInterpretation);
			#endregion

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("[수입 세금계산서(월별)] Response for 발행번호: 0401980012143", email.Subject);
			var recipient = GroupSourceLocator.GetFromGroup(importGroup);
			AssertEquals("'Post Masters' (code: 'PMG')", recipient.Location.ToString());
			AssertContains("일괄세금계산서", email.Body);
			AssertContains("수입세금계산서(과세분)", email.Body);
			AssertContains("040-19-80012143", email.Body);
			AssertContains("1218300561", email.Body);
			AssertContains("인천세관", email.Body);
			AssertContains("인천광역시 중구 서해대로 339 (항동7가)", email.Body);
			AssertContains("6188116919", email.Body);
			AssertContains("에스케이에프코리아(주)", email.Body);
			AssertContains("성석필", email.Body);
			AssertContains("부산광역시 강서구 과학산단2로19번길 99(지사동)", email.Body);
			AssertContains("2019-12-27", email.Body);
			AssertContains("5", email.Body);
			AssertContains("17605791", email.Body);
			AssertContains("1760570", email.Body);
			AssertContains("2019-12-01 ~ 2019-12-31", email.Body);
			AssertContains("Remark", email.Body);
			AssertContains("2", email.Body);

			AssertContains("개별세금 계산서 내역", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("040111919110369", email.Body);
			AssertContains("6768162", email.Body);
			AssertContains("676810", email.Body);
			AssertContains("41634-19-507229M", email.Body);
			AssertContains("040111919861383", email.Body);
			AssertContains("10837629", email.Body);
			AssertContains("1083760", email.Body);
		}

		public void Test5FZ_2_1()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5FZ_2.xml");
			CreatePayer("KRTEST", "2138514655");
			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew(IdentificationType.BusinessRegNo, "2138514655", Core.Constants.CountryCodes.KoreaSouth);
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();

			#region StatementHeader
			var statementHeader = new CusStatementHeader.Loader(Factory).Load("0302080005637", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.MonthlyReceipt);

			AssertEquals("0302080005637", statementHeader.B2_StatementNumber);
			AssertEquals("", statementHeader.B2_PeriodStartDate.ToString(DateFormatType.Date));
			AssertEquals("", statementHeader.B2_PeriodEndDate.ToString(DateFormatType.Date));
			AssertEquals(StatementHeaderPaymentStatusList.Codes.PYC, statementHeader.B2_PaymentStatus);
			AssertEquals(StatementHeaderTypeList.Codes.MonthlyReceipt, statementHeader.B2_StatementType);
			AssertEquals("030", statementHeader.B2_ProcessPort);
			AssertEquals(true, statementHeader.B2_IsMonthlyStatement);
			AssertEquals(StatementTypeList.Codes.VEP, statementHeader.B2_PaymentType);
			AssertEquals("0127030012000017941", statementHeader.B2_AccountNo);
			AssertEquals(GlbBranch.CurrentBranch.OrgProxy.PK, statementHeader.B2_OH_Importer);
			AssertEquals("2138514655", statementHeader.B2_ImporterCustomsID);
			AssertEquals(PaymentPartyList.Codes.BRK, statementHeader.B2_PaymentParty);
			AssertEquals(GlbBranch.CurrentBranch.Company.PK, statementHeader.B2_GC);
			#endregion

			#region StatementLine
			var statementLine = statementHeader.StatementLines.ToList();

			AssertEquals("1234520000045M", statementLine[0].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, statementLine[0].B3_EntryType);
			AssertEquals("030112000771025", statementLine[0].B3_AssociatedEntry);
			AssertEquals("20201005", statementLine[0].B3_EntryDate.ToString(DateFormatType.Date));
			AssertEquals(0m, statementLine[0].B3_CustomsFeesTotal);
			AssertEquals((ZShort)1, statementLine[0].B3_SequenceNumber);

			AssertEquals("4163720012182M", statementLine[1].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, statementLine[1].B3_EntryType);
			AssertEquals("030112000792759", statementLine[1].B3_AssociatedEntry);
			AssertEquals("20201005", statementLine[1].B3_EntryDate.ToString(DateFormatType.Date));
			AssertEquals(0m, statementLine[1].B3_CustomsFeesTotal);
			AssertEquals((ZShort)2, statementLine[1].B3_SequenceNumber);
			#endregion

			#region StatementLineCharge
			var statementLineCharge1 = statementLine[0].Charges.FirstOrDefault();
			AssertEquals(5622704m, statementLineCharge1.B4_ChargeAmount);
			AssertEquals(ChargeTypeList.Codes.ValueForVAT, statementLineCharge1.B4_ChargeType);

			var statementLineCharge2 = statementLine[1].Charges.FirstOrDefault();
			AssertEquals(34751570m, statementLineCharge2.B4_ChargeAmount);
			AssertEquals(ChargeTypeList.Codes.ValueForVAT, statementLineCharge2.B4_ChargeType);
			#endregion

			#region incomingMessage.EM_MessageInterpretation
			AssertContains("월별세금계산서", incomingMessage.EM_MessageInterpretation);
			AssertContains("수입계산서(면세분)", incomingMessage.EM_MessageInterpretation);
			AssertContains("030-20-80005637", incomingMessage.EM_MessageInterpretation);
			AssertContains("0127030012000017941", incomingMessage.EM_MessageInterpretation);
			AssertContains("6018300048", incomingMessage.EM_MessageInterpretation);
			AssertContains("부산세관", incomingMessage.EM_MessageInterpretation);
			AssertContains("부산광역시 중구 충장대로 20 (중앙동 4가 17)", incomingMessage.EM_MessageInterpretation);
			AssertContains("2138514655", incomingMessage.EM_MessageInterpretation);
			AssertContains("에어리퀴드어드밴스드머티어리", incomingMessage.EM_MessageInterpretation);
			AssertContains("폴카드웰버링", incomingMessage.EM_MessageInterpretation);
			AssertContains("경기도 화성시 장안면 장안공단1길 45", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-10-05", incomingMessage.EM_MessageInterpretation);
			AssertContains("5", incomingMessage.EM_MessageInterpretation);
			AssertContains("40374274", incomingMessage.EM_MessageInterpretation);
			AssertContains("0", incomingMessage.EM_MessageInterpretation);
			AssertContains("", incomingMessage.EM_MessageInterpretation);
			AssertContains("Remark", incomingMessage.EM_MessageInterpretation);
			AssertContains("2", incomingMessage.EM_MessageInterpretation);

			AssertContains("개별세금 계산서 내역", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("030112000771025", incomingMessage.EM_MessageInterpretation);
			AssertContains("5622704", incomingMessage.EM_MessageInterpretation);
			AssertContains("0", incomingMessage.EM_MessageInterpretation);
			AssertContains("41637-20-012182M", incomingMessage.EM_MessageInterpretation);
			AssertContains("030112000792759", incomingMessage.EM_MessageInterpretation);
			AssertContains("34751570", incomingMessage.EM_MessageInterpretation);
			AssertContains("0", incomingMessage.EM_MessageInterpretation);
			#endregion

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("[수입 세금계산서(월별)] Response for 발행번호: 0302080005637", email.Subject);
			var recipient = GroupSourceLocator.GetFromGroup(importGroup);
			AssertEquals("'Post Masters' (code: 'PMG')", recipient.Location.ToString());
			AssertContains("월별세금계산서", email.Body);
			AssertContains("수입계산서(면세분)", email.Body);
			AssertContains("030-20-80005637", email.Body);
			AssertContains("0127030012000017941", email.Body);
			AssertContains("6018300048", email.Body);
			AssertContains("부산세관", email.Body);
			AssertContains("부산광역시 중구 충장대로 20 (중앙동 4가 17)", email.Body);
			AssertContains("2138514655", email.Body);
			AssertContains("에어리퀴드어드밴스드머티어리", email.Body);
			AssertContains("폴카드웰버링", email.Body);
			AssertContains("경기도 화성시 장안면 장안공단1길 45", email.Body);
			AssertContains("2020-10-05", email.Body);
			AssertContains("5", email.Body);
			AssertContains("40374274", email.Body);
			AssertContains("0", email.Body);
			AssertContains("", email.Body);
			AssertContains("Remark", email.Body);
			AssertContains("2", email.Body);

			AssertContains("개별세금 계산서 내역", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("030112000771025", email.Body);
			AssertContains("5622704", email.Body);
			AssertContains("0", email.Body);
			AssertContains("41637-20-012182M", email.Body);
			AssertContains("030112000792759", email.Body);
			AssertContains("34751570", email.Body);
			AssertContains("0", email.Body);
		}

		public void Test5FZ_2_2()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5FZ_3.xml");
			CreatePayer("KRTEST", "2138514655");
			Factory.Save();

			var statement5FY = Factory.New<CusStatementHeader>();
			statement5FY.B2_StatementNumber = "0127030012000017941";
			statement5FY.B2_GC = GlbBranch.CurrentBranch.Company.PK;
			statement5FY.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			statement5FY.Reload();

			#region StatementHeader
			var statementHeader = new CusStatementHeader.Loader(Factory).Load("0302080005636", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.MonthlyReceipt);

			AssertEquals("0302080005636", statementHeader.B2_StatementNumber);
			AssertEquals("", statementHeader.B2_PeriodStartDate.ToString(DateFormatType.Date));
			AssertEquals("", statementHeader.B2_PeriodEndDate.ToString(DateFormatType.Date));
			AssertEquals(StatementHeaderPaymentStatusList.Codes.PYC, statementHeader.B2_PaymentStatus);
			AssertEquals(StatementHeaderTypeList.Codes.MonthlyReceipt, statementHeader.B2_StatementType);
			AssertEquals("030", statementHeader.B2_ProcessPort);
			AssertEquals(true, statementHeader.B2_IsMonthlyStatement);
			AssertEquals(StatementTypeList.Codes.VPD, statementHeader.B2_PaymentType);
			AssertEquals("0127030012000017941", statementHeader.B2_AccountNo);
			var cusCode = new OrgCusCode.Loader(Factory).Load(Core.Constants.CountryCodes.KoreaSouth, IdentificationType.BusinessRegNo, "2138514655")?.FirstOrDefault();
			AssertEquals(cusCode.OK_OH, statementHeader.B2_OH_Importer);
			AssertEquals("2138514655", statementHeader.B2_ImporterCustomsID);
			AssertEquals(PaymentPartyList.Codes.OWN, statementHeader.B2_PaymentParty);
			AssertEquals(GlbBranch.CurrentBranch.Company.PK, statementHeader.B2_GC);
			#endregion

			#region StatementLine
			var statementLine = statementHeader.StatementLines.ToList();

			AssertEquals("1234520000045M", statementLine[0].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, statementLine[0].B3_EntryType);
			AssertEquals("030112000771025", statementLine[0].B3_AssociatedEntry);
			AssertEquals("20201005", statementLine[0].B3_EntryDate.ToString(DateFormatType.Date));
			AssertEquals(121280080m, statementLine[0].B3_CustomsFeesTotal);
			AssertEquals((ZShort)1, statementLine[0].B3_SequenceNumber);

			AssertEquals("4163720012182M", statementLine[1].B3_EntryNum);
			AssertEquals(SharedJobMessageTypeList.Codes.Import, statementLine[1].B3_EntryType);
			AssertEquals("030112000792759", statementLine[1].B3_AssociatedEntry);
			AssertEquals("20201005", statementLine[1].B3_EntryDate.ToString(DateFormatType.Date));
			AssertEquals(15459800m, statementLine[1].B3_CustomsFeesTotal);
			AssertEquals((ZShort)2, statementLine[1].B3_SequenceNumber);
			#endregion

			#region StatementLineCharge
			var statementLineCharge1 = statementLine[0].Charges.FirstOrDefault();
			AssertEquals(1212800890m, statementLineCharge1.B4_ChargeAmount);
			AssertEquals(ChargeTypeList.Codes.ValueForVAT, statementLineCharge1.B4_ChargeType);

			var statementLineCharge2 = statementLine[1].Charges.FirstOrDefault();
			AssertEquals(154598078m, statementLineCharge2.B4_ChargeAmount);
			AssertEquals(ChargeTypeList.Codes.ValueForVAT, statementLineCharge2.B4_ChargeType);
			#endregion

			AssertEquals(StatementHeaderPaymentStatusList.Codes.PYC, statement5FY.B2_PaymentStatus);
			AssertEquals("20201005", statement5FY.B2_PaymentAuthorizationDate.ToString(DateFormatType.Date));
			AssertEquals("0302080005636", statement5FY.B2_AccountNo);

			#region incomingMessage.EM_MessageInterpretation
			AssertContains("월별세금계산서", incomingMessage.EM_MessageInterpretation);
			AssertContains("수입세금계산서(과세분)", incomingMessage.EM_MessageInterpretation);
			AssertContains("030-20-80005636", incomingMessage.EM_MessageInterpretation);
			AssertContains("0127030012000017941", incomingMessage.EM_MessageInterpretation);
			AssertContains("6018300048", incomingMessage.EM_MessageInterpretation);
			AssertContains("부산세관", incomingMessage.EM_MessageInterpretation);
			AssertContains("부산광역시 중구 충장대로 20 (중앙동 4가 17)", incomingMessage.EM_MessageInterpretation);
			AssertContains("2138514655", incomingMessage.EM_MessageInterpretation);
			AssertContains("에어리퀴드어드밴스드머티어리", incomingMessage.EM_MessageInterpretation);
			AssertContains("폴카드웰버링", incomingMessage.EM_MessageInterpretation);
			AssertContains("경기도 화성시 장안면 장안공단1길 45", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-10-05", incomingMessage.EM_MessageInterpretation);
			AssertContains("3", incomingMessage.EM_MessageInterpretation);
			AssertContains("1367398968", incomingMessage.EM_MessageInterpretation);
			AssertContains("136739880", incomingMessage.EM_MessageInterpretation);
			AssertContains("", incomingMessage.EM_MessageInterpretation);
			AssertContains("Remark", incomingMessage.EM_MessageInterpretation);
			AssertContains("2", incomingMessage.EM_MessageInterpretation);

			AssertContains("개별세금 계산서 내역", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("030112000771025", incomingMessage.EM_MessageInterpretation);
			AssertContains("1212800890", incomingMessage.EM_MessageInterpretation);
			AssertContains("121280080", incomingMessage.EM_MessageInterpretation);
			AssertContains("41637-20-012182M", incomingMessage.EM_MessageInterpretation);
			AssertContains("030112000792759", incomingMessage.EM_MessageInterpretation);
			AssertContains("154598078", incomingMessage.EM_MessageInterpretation);
			AssertContains("15459800", incomingMessage.EM_MessageInterpretation);
			#endregion

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("[수입 세금계산서(월별)] Response for 발행번호: 0302080005636", email.Subject);
			var recipient = GroupSourceLocator.GetFromGroup(importGroup);
			AssertEquals("'Post Masters' (code: 'PMG')", recipient.Location.ToString());
			AssertContains("월별세금계산서", email.Body);
			AssertContains("수입세금계산서(과세분)", email.Body);
			AssertContains("030-20-80005636", email.Body);
			AssertContains("0127030012000017941", email.Body);
			AssertContains("6018300048", email.Body);
			AssertContains("부산세관", email.Body);
			AssertContains("부산광역시 중구 충장대로 20 (중앙동 4가 17)", email.Body);
			AssertContains("2138514655", email.Body);
			AssertContains("에어리퀴드어드밴스드머티어리", email.Body);
			AssertContains("폴카드웰버링", email.Body);
			AssertContains("경기도 화성시 장안면 장안공단1길 45", email.Body);
			AssertContains("2020-10-05", email.Body);
			AssertContains("3", email.Body);
			AssertContains("1367398968", email.Body);
			AssertContains("136739880", email.Body);
			AssertContains("", email.Body);
			AssertContains("Remark", email.Body);
			AssertContains("2", email.Body);

			AssertContains("개별세금 계산서 내역", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("030112000771025", email.Body);
			AssertContains("1212800890", email.Body);
			AssertContains("121280080", email.Body);
			AssertContains("41637-20-012182M", email.Body);
			AssertContains("030112000792759", email.Body);
			AssertContains("154598078", email.Body);
			AssertContains("15459800", email.Body);
		}

		public void Test5FZ_EmptyData()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5FZ_EmptyData.xml");
			CreatePayer("KRTEST", "6188116919");
			Factory.Save();

			AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());

			incomingMessage.Reload();

			#region incomingMessage.EM_MessageInterpretation
			AssertContains("일괄세금계산서", incomingMessage.EM_MessageInterpretation);
			AssertContains("수입계산서(면세분)", incomingMessage.EM_MessageInterpretation);
			AssertContains("040-19-80012158", incomingMessage.EM_MessageInterpretation);
			AssertContains("1218300561", incomingMessage.EM_MessageInterpretation);
			AssertContains("인천세관", incomingMessage.EM_MessageInterpretation);
			AssertContains("인천광역시 중구 서해대로 339 (항동7가)", incomingMessage.EM_MessageInterpretation);
			AssertContains("6188116919", incomingMessage.EM_MessageInterpretation);
			AssertContains("2019-12-24", incomingMessage.EM_MessageInterpretation);
			AssertContains("5", incomingMessage.EM_MessageInterpretation);
			AssertContains("6158162", incomingMessage.EM_MessageInterpretation);
			AssertContains("0", incomingMessage.EM_MessageInterpretation);
			AssertContains("1", incomingMessage.EM_MessageInterpretation);

			AssertContains("개별세금 계산서 내역", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("040111919110129", incomingMessage.EM_MessageInterpretation);
			AssertContains("6158162", incomingMessage.EM_MessageInterpretation);
			AssertContains("0", incomingMessage.EM_MessageInterpretation);
			#endregion

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("[수입 세금계산서(월별)] Response for 발행번호: 0401980012158", email.Subject);
			var recipient = GroupSourceLocator.GetFromGroup(importGroup);
			AssertEquals("'Post Masters' (code: 'PMG')", recipient.Location.ToString());
			AssertContains("일괄세금계산서", email.Body);
			AssertContains("수입계산서(면세분)", email.Body);
			AssertContains("040-19-80012158", email.Body);
			AssertContains("1218300561", email.Body);
			AssertContains("인천세관", email.Body);
			AssertContains("인천광역시 중구 서해대로 339 (항동7가)", email.Body);
			AssertContains("6188116919", email.Body);
			AssertContains("2019-12-24", email.Body);
			AssertContains("5", email.Body);
			AssertContains("6158162", email.Body);
			AssertContains("0", email.Body);
			AssertContains("1", email.Body);

			AssertContains("개별세금 계산서 내역", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("040111919110129", email.Body);
			AssertContains("6158162", email.Body);
			AssertContains("0", email.Body);
		}

		public void Test5FZ_11Data()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5FZ_11Data.xml");

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = entry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "1234520000045M";
			entryNumber.CE_EntryType = SharedJobMessageTypeList.Codes.Import;

			var payer1 = CreatePayer("KRTEST1", "6188116919");
			CreatePayer("KRTEST2", "6188116919");
			declaration.JE_PaidBy = "";
			declaration.JE_OH_DutyPayer = payer1.OA_OH;

			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementNumber = "0401980012158";
			header.B2_GC = incomingMessage.Branch.GB_GC;
			header.B2_StatementType = StatementHeaderTypeList.Codes.IndividualCollectionReceipt;
			header.B2_DueDate = new ZDate(2020, 10, 10);
			header.B2_ProcessDate = new ZDate(2020, 10, 11);

			var line = header.StatementLines.AddNew();
			line.B3_EntryNum = "1233420017772M";
			line.B3_AssociatedEntry = "040111919110129";
			line.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			line.B3_EntryDate = new ZDate(2021, 05, 17);
			line.B3_CustomsFeesTotal = 19283m;
			line.B3_SequenceNumber = 2;

			var charge = line.Charges.AddNew();
			charge.B4_ChargeAmount = 12345m;
			charge.B4_ChargeType = ChargeTypeList.Codes.ValueForVAT;

			var line2 = header.StatementLines.AddNew();
			line2.B3_EntryNum = "1234520000045M";
			line2.B3_AssociatedEntry = "123451919110129";
			line2.B3_EntryType = SharedJobMessageTypeList.Codes.Import;
			line2.B3_EntryDate = new ZDate(2021, 05, 17);
			line2.B3_CustomsFeesTotal = 12345m;
			line2.B3_SequenceNumber = 1;

			Factory.Save();

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			var statementHeader = new CusStatementHeader.Loader(Factory).Load("0401980012158", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.IndividualCollectionReceipt);

			AssertEquals(true, line.IsDeleted);
			AssertEquals("040111919110129", line2.B3_AssociatedEntry);
			AssertEquals(0m, line2.B3_CustomsFeesTotal);

			AssertEquals(payer1.Header.PK, statementHeader.B2_OH_Importer);

			var statementLine = statementHeader.StatementLines.Cast<CusStatementLine>().ToList();

			AssertEquals(11, statementLine.Count);

			#region StatementLineCharge
			var statementLineCharge1 = statementLine[0].Charges.FirstOrDefault();
			AssertEquals(6158162m, statementLineCharge1.B4_ChargeAmount);
			AssertEquals(ChargeTypeList.Codes.ValueForVAT, statementLineCharge1.B4_ChargeType);
			#endregion

			#region incomingMessage.EM_MessageInterpretation
			AssertContains("개별세금 계산서 내역", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("040111919110129", incomingMessage.EM_MessageInterpretation);
			AssertContains("6158162", incomingMessage.EM_MessageInterpretation);
			AssertContains("0", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-19-507281M", incomingMessage.EM_MessageInterpretation);
			AssertContains("040111919861587", incomingMessage.EM_MessageInterpretation);
			AssertContains("10837114", incomingMessage.EM_MessageInterpretation);
			AssertContains("0", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-19-123456M", incomingMessage.EM_MessageInterpretation);
			AssertContains("040111919110001", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-19-789123M", incomingMessage.EM_MessageInterpretation);
			AssertContains("040111919861002", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-19-456123M", incomingMessage.EM_MessageInterpretation);
			AssertContains("040111919110003", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-19-741852M", incomingMessage.EM_MessageInterpretation);
			AssertContains("040111919861004", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-19-852963M", incomingMessage.EM_MessageInterpretation);
			AssertContains("040111919110005", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-19-159357M", incomingMessage.EM_MessageInterpretation);
			AssertContains("040111919861006", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-19-654852M", incomingMessage.EM_MessageInterpretation);
			AssertContains("040111919110007", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-19-845620M", incomingMessage.EM_MessageInterpretation);
			AssertContains("040111919861008", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("나머지 내역은 프로그램에서 확인 하십시오.", incomingMessage.EM_MessageInterpretation);
			#endregion

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("개별세금 계산서 내역", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("040111919110129", email.Body);
			AssertContains("6158162", email.Body);
			AssertContains("0", email.Body);
			AssertContains("41634-19-507281M", email.Body);
			AssertContains("040111919861587", email.Body);
			AssertContains("10837114", email.Body);
			AssertContains("0", email.Body);
			AssertContains("41634-19-123456M", email.Body);
			AssertContains("040111919110001", email.Body);
			AssertContains("41634-19-789123M", email.Body);
			AssertContains("040111919861002", email.Body);
			AssertContains("41634-19-456123M", email.Body);
			AssertContains("040111919110003", email.Body);
			AssertContains("41634-19-741852M", email.Body);
			AssertContains("040111919861004", email.Body);
			AssertContains("41634-19-852963M", email.Body);
			AssertContains("040111919110005", email.Body);
			AssertContains("41634-19-159357M", email.Body);
			AssertContains("040111919861006", email.Body);
			AssertContains("41634-19-654852M", email.Body);
			AssertContains("040111919110007", email.Body);
			AssertContains("41634-19-845620M", email.Body);
			AssertContains("040111919861008", email.Body);
			AssertContains("나머지 내역은 프로그램에서 확인 하십시오.", email.Body);
		}

		public void Test5FZ_B2_PaymentAuthorizationDate()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5FZ_0.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			var statementHeader = new CusStatementHeader.Loader(Factory).Load("0401980012158", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.IndividualCollectionReceipt);
			AssertEquals("20191224", statementHeader.B2_PaymentAuthorizationDate.ToString(DateFormatType.Date));
		}

		public void Test5FZ_StmNote()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5FZ_0.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			var statementHeader = new CusStatementHeader.Loader(Factory).Load("0401980012158", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.IndividualCollectionReceipt);
			AssertEquals("Remark", statementHeader.Remarks);
		}

		public void Test5FZ_B2_ProcessDate()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5FZ_3.xml");
			CreatePayer("KRTEST", "2138514655");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();

			#region StatementHeader
			var statementHeader = new CusStatementHeader.Loader(Factory).Load("0302080005636", incomingMessage.Branch.GB_GC, StatementHeaderTypeList.Codes.MonthlyReceipt);

			AssertEquals(incomingMessage.EM_MessageDateTime.ToString("yyyy-MM-dd 00:00:00"), statementHeader.B2_ProcessDate.ToString(DateFormatType.DateTimeKorean));
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
			var fileReader = new TestFileReader(typeof(GOVCBR5FZMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5FZ;
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

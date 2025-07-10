using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CSARevenueSummaryFormDocumentWrapperTest : TestCaseWithFactory
	{
		#region TestSourceIdentifierProvider

		public void TestISourceIdentifierProvider()
		{
			var rsf = Factory.New<CusStatementHeader>();
			rsf.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			rsf.B2_OH_Importer = Factory.LoadTop1<OrgHeader>(new CargoWise.EntityFramework.ZQuery()).PK;
			rsf.B2_PeriodEndDate = new ZDate(2020, 11, 30);
			Factory.Save();
			var wrapper = new CSARevenueSummaryFormDocumentWrapper(rsf);

			var supporter = wrapper as ISourceIdentifierProvider;
			AssertNotNull("CSARevenueSummaryFormDocumentWrapper should implement ISourceIdentifierProvider", supporter);
			AssertEquals("supporter.SourceIdentifier", rsf.PK, supporter?.SourceIdentifier);
		}

		#endregion

		public void TestGenerateWrapper()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";

			var dec01 = Factory.New<JobDeclaration>();
			var dec02 = Factory.New<JobDeclaration>();
			var dec03 = Factory.New<JobDeclaration>();
			var dec04 = Factory.New<JobDeclaration>();

			dec01.JE_OH_Importer = importer.PK;
			dec02.JE_OH_Importer = importer.PK;
			dec03.JE_OH_Importer = importer.PK;
			dec04.JE_OH_Importer = importer.PK;

			dec01.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec02.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			dec03.JE_MessageType = JobMessageTypeList.Codes.XTypeEntry;
			dec04.JE_MessageType = JobMessageTypeList.Codes.Import;

			dec01.JE_EntryAuthorisationDate = new ZDateTime(2020, 11, 29, 12, 12, 12);
			dec02.JE_EntryAuthorisationDate = new ZDateTime(2020, 11, 02, 12, 12, 12);
			dec03.JE_EntryAuthorisationDate = new ZDateTime(2020, 11, 02, 12, 12, 12);
			dec04.JE_EntryAuthorisationDate = new ZDateTime(2020, 11, 17, 12, 12, 12);

			dec02.CA_B2AcceptedDate = new ZDateTime(2020, 11, 03, 12, 12, 12);
			dec03.CA_B2AcceptedDate = new ZDateTime(2020, 11, 03, 12, 12, 12);

			var entry01 = dec01.ActiveEntryHeaders.AddNew();
			entry01.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entry01.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
			entry01.CH_EntryReleaseDate = new ZDateTime(2020, 12, 17, 12, 12, 12);

			var entryLine01 = entry01.MergedLines.AddNew();
			entryLine01.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 12m);
			entryLine01.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 1m);
			entryLine01.Fees.SetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount, 1.2m);
			entryLine01.Fees.SetAmount(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 0.09m);

			var entry04 = dec04.ActiveEntryHeaders.AddNew();
			entry04.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entry04.CH_EntryStatus = CADEntryStatusList.Codes.Approved;
			entry04.CH_EntryReleaseDate = new ZDateTime(2020, 11, 30, 12, 12, 12);

			var entryLine04 = entry04.MergedLines.AddNew();
			entryLine04.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 13m);
			entryLine04.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 2m);
			entryLine04.Fees.SetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount, 3.2m);
			entryLine04.Fees.SetAmount(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 0.19m);

			var entry02 = dec02.ActiveEntryHeaders.AddNew();
			entry02.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

			var entryLine02 = entry02.MergedLines.AddNew();
			entryLine02.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 12m);
			entryLine02.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 1m);
			entryLine02.Fees.SetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount, 1.2m);
			entryLine02.Fees.SetAmount(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 0.09m);

			var invoice02 = dec02.B2AsClaimedForInvoices.AddNew();
			var line02 = invoice02.AsClaimForFilteredInvoiceLines.AddNew();
			var fee021 = line02.DutiesAndTaxes.AddNew();
			fee021.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			fee021.C1_Amount = 13m;

			var fee022 = line02.DutiesAndTaxes.AddNew();
			fee022.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			fee022.C1_Amount = 1m;

			var fee023 = line02.DutiesAndTaxes.AddNew();
			fee023.C1_TaxType = DutyAndTaxTypes.Codes.SIMADuty;
			fee023.C1_Amount = 1.2m;

			var fee024 = line02.DutiesAndTaxes.AddNew();
			fee024.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			fee024.C1_Amount = 0.09m;

			var entry03 = dec03.ActiveEntryHeaders.AddNew();
			entry03.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;

			var entryLine03 = entry03.MergedLines.AddNew();
			var entryLineFee0301 = entryLine03.Fees.AddNew();
			entryLineFee0301.CF_ChargeType = EntryChargeTypeList.Codes.TotalDutyAmount;
			entryLineFee0301.CF_ChargeAmount = -12m;
			var entryLineFee0302 = entryLine03.Fees.AddNew();
			entryLineFee0302.CF_ChargeType = EntryChargeTypeList.Codes.TotalGSTAmount;
			entryLineFee0302.CF_ChargeAmount = -11m;
			entryLine03.Fees.SetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount, 1.2m);
			entryLine03.Fees.SetAmount(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 0.09m);

			var invoice03 = dec03.B2AsClaimedForInvoices.AddNew();
			var line03 = invoice03.AsClaimForFilteredInvoiceLines.AddNew();
			var fee031 = line03.DutiesAndTaxes.AddNew();
			fee031.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			fee031.C1_Amount = -12m;

			var fee032 = line03.DutiesAndTaxes.AddNew();
			fee032.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			fee032.C1_Amount = -11m;

			var fee033 = line03.DutiesAndTaxes.AddNew();
			fee033.C1_TaxType = DutyAndTaxTypes.Codes.SIMADuty;
			fee033.C1_Amount = 1.2m;

			var fee034 = line03.DutiesAndTaxes.AddNew();
			fee034.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			fee034.C1_Amount = 0.09m;

			var entryNum01 = CusEntryNumber.New(dec01, "REL", Core.Constants.CountryCodes.Canada);
			entryNum01.CE_ParentTable = JobDeclaration.Schema.TableName;
			entryNum01.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			entryNum01.CE_EntryNum = "12345XX";
			entryNum01.CE_ParentID = dec01.PK;

			var rsf = Factory.New<CusStatementHeader>();
			rsf.B2_StatementType = CusStatementHeaderTypes.Codes.RSF;
			rsf.B2_OH_Importer = importer.PK;
			rsf.B2_PeriodEndDate = new ZDate(2020, 11, 30);
			Factory.Save();

			dec02.CA_OriginalTransactionNo = entryNum01.CE_EntryNum;
			dec03.CA_OriginalTransactionNo = entryNum01.CE_EntryNum;

			var debits = rsf.Debits.Cast<CSARSFPayment>();
			var credits = rsf.Credits.Cast<CSARSFPayment>();
			var interims = rsf.InterimPayments.Cast<CSARSFPayment>();

			debits.FirstOrDefault(x => x.CodeID == CSARSFDebitCodes.Codes._49443).Amount = 12.34m;
			debits.FirstOrDefault(x => x.CodeID == CSARSFDebitCodes.Codes._49454).Amount = 23.45m;
			debits.FirstOrDefault(x => x.CodeID == CSARSFDebitCodes.Codes._49555).Amount = 34.56m;

			credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49010).Amount = 21.43m;
			credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49019).Amount = 32.54m;
			credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49121).Amount = 43.65m;
			credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49409).Amount = 0.12m;
			credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49437).Amount = 9m;
			credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49443).Amount = 100m;
			credits.FirstOrDefault(x => x.CodeID == CSARSFCreditCodes.Codes._49555).Amount = 21.43m;

			interims.FirstOrDefault(x => x.CodeID == CSARSFInterimPaymentCodes.Codes._49010).Amount = 103m;
			interims.FirstOrDefault(x => x.CodeID == CSARSFInterimPaymentCodes.Codes._49121).Amount = 9.2m;

			var assessment01 = CreateCSARSFAssessment(rsf, "TP1", "RN01", "PC01", 1.01m);
			var assessment02 = CreateCSARSFAssessment(rsf, "TP2", "RN02", "PC02", 2.02m);
			var assessment03 = CreateCSARSFAssessment(rsf, "TP3", "RN03", "PC03", 3.03m);
			var assessment04 = CreateCSARSFAssessment(rsf, "TP4", "RN04", "PC04", 4.04m);
			var assessment05 = CreateCSARSFAssessment(rsf, "TP5", "RN05", "PC05", 5.05m);
			var assessment06 = CreateCSARSFAssessment(rsf, "TP6", "RN06", "PC06", 6.06m);

			var calculator = new CSARSFCalculator(rsf);
			calculator.CalculateRSF();

			var wrapper = new CSARevenueSummaryFormDocumentWrapper(rsf);

			CombineAssertions(() =>
			{
				AssertEquals("Business Number", rsf.B2_ImporterCustomsID, wrapper.BusinessNumber);
				AssertEquals("CSA Importer Name", rsf.ImporterName, wrapper.CSAImporterName);
				AssertEquals("RSF Month", "2020/11", wrapper.RSFMonth);
				AssertEquals("Period Start Date", "2020/11/01", wrapper.PeriodStartDate);
				AssertEquals("Period End Date", "2020/11/30", wrapper.PeriodEndDate);
				AssertEquals("VFD of Current Month transactions", ZDecimal.Zero, wrapper.VFDOfCurrentMonthTransactions);
				AssertEquals("Filing ID", CACustomsDataRegistry.Instance.AccountSecurityNo.Value, wrapper.FilingID);

				AssertEquals("Net Total of Debits and Credits", -101.54m, wrapper.NetTotalOfDebitsAndCredits1stPage);

				AssertEquals("Debit 49010 Originals", 25m, wrapper.Debit49010Originals);
				AssertEquals("Debit 49010 Adjustments", 13m, wrapper.Debit49010Adjustments);
				AssertEquals("Debit 49121 Originals", 3m, wrapper.Debit49121Originals);
				AssertEquals("Debit 49121 Adjustments", 1m, wrapper.Debit49121Adjustments);
				AssertEquals("Debit 49011", 4.4m, wrapper.Debit49011);
				AssertEquals("Debit 49475", 0.28m, wrapper.Debit49475);
				AssertEquals("Debit 49443", 12.34m, wrapper.Debit49443);
				AssertEquals("Debit 49555", 34.56m, wrapper.Debit49555);
				AssertEquals("Debit 49454", 23.45m, wrapper.Debit49454);
				AssertEquals("Debit Subtotal in 1st Page", 117.03m, wrapper.DebitSubtotal1stPage);

				AssertEquals("Credit 49010", 21.43m, wrapper.Credit49010);
				AssertEquals("Credit 49121", 43.65m, wrapper.Credit49121);
				AssertEquals("Credit 49443", 100m, wrapper.Credit49443);
				AssertEquals("Credit 49017", -12m, wrapper.Credit49017);
				AssertEquals("Credit 49018", 2.4m, wrapper.Credit49018);
				AssertEquals("Credit 49019", 32.54m, wrapper.Credit49019);
				AssertEquals("Credit 49409", 0.12m, wrapper.Credit49409);
				AssertEquals("Credit 49437", 9m, wrapper.Credit49437);
				AssertEquals("Credit 49555", 21.43m, wrapper.Credit49555);
				AssertEquals("Credit Subtotal in 1st Page", 218.57m, wrapper.CreditSubtotal1stPage);

				AssertEquals("Interim Payment 49010", 103m, wrapper.Interim49010);
				AssertEquals("Interim Payment 49121", 9.2m, wrapper.Interim49121);
				AssertEquals("Interim Payments Subtotal in 1st Page", 112.2m, wrapper.InterimSubtotal1stPage);

				AssertEquals("1st Customs Assessment in 1st Page", 1.01m, wrapper.Assessment1.Amount);
				AssertEquals("2nd Customs Assessment in 1st Page", 2.02m, wrapper.Assessment2.Amount);
				AssertEquals("3rd Customs Assessment in 1st Page", 3.03m, wrapper.Assessment3.Amount);
				AssertEquals("Count of Customs Assessment in 2nd Page", 3, wrapper.CustomsAssessments2ndPage.Count);
				AssertEquals("Hide 2nd Page", false, wrapper.Hide2ndPage);
				AssertEquals("1st Customs Assessment in 2nd Page", 4.04m, wrapper.CustomsAssessments2ndPage[0].Amount);
				AssertEquals("2nd Customs Assessment in 2nd Page", 5.05m, wrapper.CustomsAssessments2ndPage[1].Amount);
				AssertEquals("3rd Customs Assessment in 2nd Page", 6.06m, wrapper.CustomsAssessments2ndPage[2].Amount);
				AssertEquals("Customs Assessments Subtotal in 1st Page", 6.06m, wrapper.CustomsAssessmentsSubtotal1stPage);
				AssertEquals("Customs Assessments Subtotal in 2nd Page", 15.15m, wrapper.CustomsAssessmentsSubtotal2ndPage);

				AssertEquals("Net Total of Current Month Revenue Distribution", -101.54m, wrapper.NetTotalCurrentMonthRevenueDistribution);
				AssertEquals("Subtotal of Interim Payments", 112.2m, wrapper.SubtotalInterimPayment);
				AssertEquals("Subtotal of Customs Assessments", 21.21m, wrapper.SubtotalCustomsAssessments);
				AssertEquals("Total Payment", 31.87m, wrapper.TotalPayment);
			});
		}

		CSARSFAssessment CreateCSARSFAssessment(CusStatementHeader rsf, ZString type, ZString referenceNumber, ZString portCode, ZDecimal amount)
		{
			var assessment = rsf.CustomsAssessments.AddNew();
			assessment.Type = type;
			assessment.ReferenceNumber = referenceNumber;
			assessment.PortCode = portCode;
			assessment.Amount = amount;
			return assessment;
		}
	}
}

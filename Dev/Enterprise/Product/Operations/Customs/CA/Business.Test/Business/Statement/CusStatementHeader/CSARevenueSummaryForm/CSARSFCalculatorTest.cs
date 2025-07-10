using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CSARSFCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculateRSF()
		{
			var dec01 = CreateTestJobDeclaration(JobMessageTypeList.Codes.Import, new ZDateTime(2020, 11, 29, 12, 12, 12), ZDateTime.Empty);
			var dec02 = CreateTestJobDeclaration(JobMessageTypeList.Codes.Import, new ZDateTime(2020, 11, 17, 12, 12, 12), ZDateTime.Empty);
			var dec03 = CreateTestJobDeclaration(JobMessageTypeList.Codes.XTypeEntry, ZDateTime.Empty, new ZDateTime(2020, 11, 15, 12, 12, 12));
			var dec04 = CreateTestJobDeclaration(JobMessageTypeList.Codes.Import, new ZDateTime(2020, 12, 02, 12, 12, 12), ZDateTime.Empty);
			var dec05 = CreateTestJobDeclaration(JobMessageTypeList.Codes.XTypeEntry, ZDateTime.Empty, new ZDateTime(2020, 12, 02, 12, 12, 12));

			var entry01 = dec01.ActiveEntryHeaders.AddNew();
			entry01.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entry01.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
			entry01.CH_EntryReleaseDate = new ZDateTime(2020, 12, 17, 12, 12, 12);

			var entryLine01 = entry01.MergedLines.AddNew();
			entryLine01.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 12m);
			entryLine01.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 1m);
			entryLine01.Fees.SetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount, 1.2m);
			entryLine01.Fees.SetAmount(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 0.09m);

			var entry02 = dec02.ActiveEntryHeaders.AddNew();
			entry02.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entry02.CH_EntryStatus = CADEntryStatusList.Codes.Approved;
			entry02.CH_EntryReleaseDate = new ZDateTime(2020, 11, 30, 12, 12, 12);

			var entryLine02 = entry02.MergedLines.AddNew();
			entryLine02.Fees.SetAmount(EntryChargeTypeList.Codes.TotalDutyAmount, 13m);
			entryLine02.Fees.SetAmount(EntryChargeTypeList.Codes.TotalGSTAmount, 2m);
			entryLine02.Fees.SetAmount(EntryChargeTypeList.Codes.TotalSIMAAmount, 2.2m);
			entryLine02.Fees.SetAmount(EntryChargeTypeList.Codes.TotalExciseTaxAmount, 1.09m);

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

			var calculator = new CSARSFCalculator(rsf);
			calculator.CalculateRSF();

			AssertEquals(3, rsf.CSARSFTransactions.Count);

			var debits = rsf.Debits.Cast<CSARSFPayment>();
			var credits = rsf.Credits.Cast<CSARSFPayment>();
			var interims = rsf.InterimPayments.Cast<CSARSFPayment>();

			CombineAssertions(() =>
			{
				AssertEquals(CSARSFDebitCodes.Codes._490101, 25m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._490101));
				AssertEquals(CSARSFDebitCodes.Codes._490102, 0m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._490102));
				AssertEquals(CSARSFDebitCodes.Codes._49011, 3.4m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._49011));
				AssertEquals(CSARSFDebitCodes.Codes._491211, 3m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._491211));
				AssertEquals(CSARSFDebitCodes.Codes._491212, 0m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._491212));
				AssertEquals(CSARSFDebitCodes.Codes._49443, 0m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._49443));
				AssertEquals(CSARSFDebitCodes.Codes._49454, 0m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._49454));
				AssertEquals(CSARSFDebitCodes.Codes._49475, 1.18m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._49475));
				AssertEquals(CSARSFDebitCodes.Codes._49555, 0m, GetPaymentAmount(debits, CSARSFDebitCodes.Codes._49555));

				AssertEquals(CSARSFCreditCodes.Codes._49010, 0m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49010));
				AssertEquals(CSARSFCreditCodes.Codes._49017, -12m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49017));
				AssertEquals(CSARSFCreditCodes.Codes._49018, 1.2m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49018));
				AssertEquals(CSARSFCreditCodes.Codes._49019, 0m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49019));
				AssertEquals(CSARSFCreditCodes.Codes._49121, 0m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49121));
				AssertEquals(CSARSFCreditCodes.Codes._49409, 0m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49409));
				AssertEquals(CSARSFCreditCodes.Codes._49437, 0m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49437));
				AssertEquals(CSARSFCreditCodes.Codes._49443, 0m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49443));
				AssertEquals(CSARSFCreditCodes.Codes._49555, 0m, GetPaymentAmount(credits, CSARSFCreditCodes.Codes._49555));

				AssertEquals(CSARSFInterimPaymentCodes.Codes._49010, 0m, GetPaymentAmount(interims, CSARSFInterimPaymentCodes.Codes._49010));
				AssertEquals(CSARSFInterimPaymentCodes.Codes._49121, 0m, GetPaymentAmount(interims, CSARSFInterimPaymentCodes.Codes._49121));
			});
		}

		JobDeclaration CreateTestJobDeclaration(ZString messageType, ZDateTime releaseDate, ZDateTime acceptedDate)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_MessageType = messageType;
			dec.JE_EntryAuthorisationDate = releaseDate;
			dec.CA_B2AcceptedDate = acceptedDate;
			return dec;
		}

		ZDecimal GetPaymentAmount(IEnumerable<CSARSFPayment> payments, ZString paymentType)
		{
			return payments.FirstOrDefault(x => x.CodeID == paymentType).Amount;
		}

		OrgHeader importer;

		protected override void SetUp()
		{
			base.SetUp();
			importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMPORTER";
		}
	}
}

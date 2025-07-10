using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class AccountingTransactionExportGetLinesTest : ScriptTest
	{
		public void TestAccountingTransactionExportGetLines()
		{
			var arInvoice = CreateARInvoice();
			var directPayment = CreateDirectPayment();
			var apInvoice = CreateAPInvoice();
			Factory.Save();

			var result = RunScript(apInvoice.PK);
			AssertDataRow(result.FirstOrDefault(), headers, valuesForAPInvoice);

			result = RunScript(directPayment.PK);
			AssertDataRow(result.FirstOrDefault(), headers, valuesForDirectPayment);

			result = RunScript(arInvoice.PK);
			AssertDataRow(result.FirstOrDefault(), headers, valuesForARInvoiceWithCharge);
		}

		public void TestAccountingTransactionExportGetLines_TaxBranch_IsNotNull()
		{
			var branch1 = TestObjectCreator.CreateBranch("Br1", "TaxBranch1", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("Br2", "TaxBranch2", GlbCompany.CurrentCompany);
			var branch3 = TestObjectCreator.CreateBranch("Br3", "TaxBranch3", GlbCompany.CurrentCompany);
			Factory.Save();

			var arInvoice = CreateARInvoice();
			arInvoice.AH_GB_TaxBranch = branch1.PK;
			arInvoice.Lines[0].AL_GB_TaxBranch = branch1.PK;
			var directPayment = CreateDirectPayment();
			directPayment.AH_GB_TaxBranch = branch2.PK;
			directPayment.Lines[0].AL_GB_TaxBranch = branch2.PK;
			var apInvoice = CreateAPInvoice();
			apInvoice.AH_GB_TaxBranch = branch3.PK;
			apInvoice.Lines[0].AL_GB_TaxBranch = branch3.PK;
			Factory.Save();

			var result = RunScript(arInvoice.PK);
			AssertEquals(branch1.PK, result.First()["AL_GB_TaxBranch"]);

			result = RunScript(directPayment.PK);
			AssertEquals(branch2.PK, result.First()["AL_GB_TaxBranch"]);

			result = RunScript(apInvoice.PK);
			AssertEquals(branch3.PK, result.First()["AL_GB_TaxBranch"]);
		}

		public void TestAccountingTransactionExportGetLines_TaxBranch_IsNull()
		{
			var arInvoice = CreateARInvoice();
			var directPayment = CreateDirectPayment();
			var apInvoice = CreateAPInvoice();
			Factory.Save();

			var result = RunScript(arInvoice.PK);
			AssertEquals(DBNull.Value, result.First()["AL_GB_TaxBranch"]);

			result = RunScript(directPayment.PK);
			AssertEquals(DBNull.Value, result.First()["AL_GB_TaxBranch"]);

			result = RunScript(apInvoice.PK);
			AssertEquals(DBNull.Value, result.First()["AL_GB_TaxBranch"]);
		}

		public void TestGetTaxRateFromLine()
		{
			var arInvoice = CreateARInvoice(false);
			var directPayment = CreateDirectPayment();
			var apInvoice = CreateAPInvoice();

			SetLineData(arInvoice, 18, 3, 6, 2);
			SetLineData(directPayment, 125, 10, 100, 10);
			SetLineData(apInvoice, 4, 1, 135, 10);
			Factory.Save();

			AssertResult(arInvoice.PK, 6M, 3M);
			AssertResult(directPayment.PK, 12.5M, 10M);
			AssertResult(apInvoice.PK, 4M, 13.5M);

			void AssertResult(ZGuid pk, decimal rate, decimal extraRate)
			{
				var result = RunScript(pk);
				AssertEquals(rate, result.FirstOrDefault().Field<decimal>("AT_Rate"));
				AssertEquals(extraRate, result.FirstOrDefault().Field<decimal>("AT_ExtraRate"));
			}

			void SetLineData(TransactionHeaderWithLines transaction, int rateNumerator, int rateDenominator, int extraRateNumerator, int extraRateDenominator)
			{
				transaction.Lines[0].AL_TaxRateNumerator = rateNumerator;
				transaction.Lines[0].AL_TaxRateDenominator = rateDenominator;
				transaction.Lines[0].AL_TaxExtraRateNumerator = extraRateNumerator;
				transaction.Lines[0].AL_TaxExtraRateDenominator = extraRateDenominator;
			}
		}

		public void TestWIPAndACR()
		{
			var charge = CreateCharge();
			charge.JR_InvoiceType = "FIN";
			charge.JR_RX_NKSellInvoiceCurrency = "USD";
			var invoice = CreateARInvoice(false);
			var apInvoice = CreateAPInvoice();
			Factory.Save();

			var batchNumber = 1;
			var sequence = 1;
			TestObjectCreator.CreateGenExportBatchSequenceWebServiceReverseLine(batchNumber, charge.WIP.PK, sequence);
			Factory.Save();
			var result = RunScript(null, batchNumber);
			AssertDataRow(result.FirstOrDefault(), headers, valuesForWIP);

			batchNumber = 2;
			sequence = 2;
			TestObjectCreator.CreateGenExportBatchSequenceWebServiceReverseLine(batchNumber, charge.Accrual.PK, sequence);
			Factory.Save();
			result = RunScript(null, batchNumber);
			AssertDataRow(result.FirstOrDefault(), headers, valuesForACR);

			batchNumber = 3;
			sequence = 3;
			TestObjectCreator.CreateGenExportBatchSequenceWebServiceReverseLine(batchNumber, invoice.Lines[0].PK, sequence);
			Factory.Save();
			result = RunScript(invoice.PK, batchNumber);
			AssertDataRow(result.FirstOrDefault(), headers, valuesForARInvoice);

			batchNumber = 4;
			sequence = 4;
			TestObjectCreator.CreateGenExportBatchSequenceWebServiceReverseLine(batchNumber, apInvoice.Lines[0].PK, sequence);
			Factory.Save();
			result = RunScript(apInvoice.PK, batchNumber);
			AssertDataRow(result.FirstOrDefault(), headers, valuesForAPInvoice);
		}

		public void TestChargeValueOnPostedAndUnposted()
		{
			var charge = CreateCharge();
			charge.JR_InvoiceType = "FIN";
			charge.JR_RX_NKSellInvoiceCurrency = "USD";
			Factory.Save();

			var batchNumber = 1;
			var sequence = 1;
			TestObjectCreator.CreateGenExportBatchSequenceWebServiceReverseLine(batchNumber, charge.WIP.PK, sequence);
			Factory.Save();
			var result = RunScript(null, batchNumber);
			AssertDataRow(result.FirstOrDefault(), headers, valuesForWIP);

			batchNumber = 2;
			sequence = 2;
			var arInvoice = CreateARInvoice(false);
			charge = TestObjectCreator.CreateCharge(arInvoice.Lines[0], TestObjectCreator.Job1, TestObjectCreator.CommentChargeCode, TestObjectCreator.AUD);
			Factory.Save();

			TestObjectCreator.GSTWithExtraRate.SetRate_ForTestOnly(16, 2);
			Factory.Save();

			result = RunScript(arInvoice.PK, batchNumber);
			AssertDataRow(result.FirstOrDefault(), headers, valuesForARInvoiceWithCharge);

			batchNumber = 3;
			sequence = 3;
			var apInvoice = CreateAPInvoice();
			charge = TestObjectCreator.CreateCharge(apInvoice.Lines[0], TestObjectCreator.Job1, TestObjectCreator.CommentChargeCode, TestObjectCreator.AUD);
			Factory.Save();

			TestObjectCreator.GST1.SetRate_ForTestOnly(17, 1);
			TestObjectCreator.GST1.SetRate_ForTestOnly(19, 1);
			Factory.Save();

			result = RunScript(apInvoice.PK, batchNumber);
			AssertDataRow(result.FirstOrDefault(), headers, valuesForAPInvoiceWithCharge);
		}

		[TestDate(2020, 6, 3)]
		public void TestMultiSubAccounts()
		{
			var salesGroup = TestObjectCreator.CreateSalesGroup("SG1");
			var staffGroup = TestObjectCreator.CreateStaffGroup("STF");
			var staff = GlbStaff.CurrentUser;

			var arInvoice = CreateARInvoice();
			var apInvoice = CreateAPInvoice();
			AssertEquals("Precondition", 1, arInvoice.Lines.Count);
			AssertEquals("Precondition", 1, apInvoice.Lines.Count);

			var arInvoiceLines = arInvoice.Lines[0];
			var apInvoiceLines = apInvoice.Lines[0];

			ScriptTestHelper.CreateMultiSubAccounts(TestObjectCreator, AccTransactionLineSubAccountSchema.Constants.TableName, arInvoiceLines.PK, OrgHeaderSchema.Constants.Prefix, TestObjectCreator.LocalClient.PK);
			ScriptTestHelper.CreateMultiSubAccounts(TestObjectCreator, AccTransactionLineSubAccountSchema.Constants.TableName, arInvoiceLines.PK, AccGroupsSchema.Constants.Prefix, salesGroup.PK);
			ScriptTestHelper.CreateMultiSubAccounts(TestObjectCreator, AccTransactionLineSubAccountSchema.Constants.TableName, arInvoiceLines.PK, GlbGroupSchema.Constants.Prefix, staffGroup.PK);

			ScriptTestHelper.CreateMultiSubAccounts(TestObjectCreator, AccTransactionLineSubAccountSchema.Constants.TableName, apInvoiceLines.PK, OrgHeaderSchema.Constants.Prefix, TestObjectCreator.LocalClient.PK);
			ScriptTestHelper.CreateMultiSubAccounts(TestObjectCreator, AccTransactionLineSubAccountSchema.Constants.TableName, apInvoiceLines.PK, AccGroupsSchema.Constants.Prefix, salesGroup.PK);
			ScriptTestHelper.CreateMultiSubAccounts(TestObjectCreator, AccTransactionLineSubAccountSchema.Constants.TableName, apInvoiceLines.PK, GlbGroupSchema.Constants.Prefix, staffGroup.PK);
			ScriptTestHelper.CreateMultiSubAccounts(TestObjectCreator, AccTransactionLineSubAccountSchema.Constants.TableName, apInvoiceLines.PK, GlbStaffSchema.Constants.Prefix, staff.PK);

			Factory.Save();

			var result = RunScript(arInvoice.PK);
			var arInvoiceResult = result.FirstOrDefault();
			AssertNotNull(arInvoiceResult);
			AssertEquals("ORG: ZLOCCLT, SEG: SG1, SGP: STF", arInvoiceResult["MultiSubAccountTypeCode"].ToString());

			result = RunScript(apInvoice.PK);
			var apInvoiceResult = result.FirstOrDefault();
			AssertNotNull(apInvoiceResult);
			AssertEquals("ORG: ZLOCCLT, SEG: SG1, STR: E, SGP: STF", apInvoiceResult["MultiSubAccountTypeCode"].ToString());
		}

		public void TestCashAdvanceReceivedAmount()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
			var arInvoiceLine = TestObjectCreator.CreateARInvoiceLine(arInvoice, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 100M);
			arInvoiceLine.AL_JH = TestObjectCreator.Job1.PK;

			var charge = TestObjectCreator.CreateJobCharge(arInvoiceLine, TestObjectCreator.Job1, TestObjectCreator.CommentChargeCode, TestObjectCreator.AUD);
			var cah = TestObjectCreator.CreateCashAdvanceRequestHeader(charge.Job as Job, TestObjectCreator.Debtor, LedgerTypes.AccountsReceivable, charge.JR_LocalSellAmt, charge.JR_OSSellAmt, charge.JR_RX_NKSellCurrency);

			var cal = TestObjectCreator.CreateCashAdvanceRequestLine(cah, charge.JR_LocalSellAmt, charge.JR_OSSellAmt);
			charge.JR_CAL_ARLine = cal.PK;
			cal.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			cal.CAL_LocalPaidAmount = 100M;
			cal.CAL_OSPaidAmount = 100M;

			Factory.Save();

			var apInvoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1.0m);
			apInvoice.AH_TransactionNum = "111";
			apInvoice.AH_OH = TestObjectCreator.AALSHI.PK;

			var apInvoiceLine = TestObjectCreator.CreateAPInvoiceLine(apInvoice, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 100M);
			apInvoiceLine.AL_JH = TestObjectCreator.Job1.PK;
			var charge1 = TestObjectCreator.CreateJobCharge(apInvoiceLine, TestObjectCreator.Job1, TestObjectCreator.CommentChargeCode, TestObjectCreator.AUD);
			var cah1 = TestObjectCreator.CreateCashAdvanceRequestHeader(charge1.Job as Job, TestObjectCreator.Debtor, LedgerTypes.AccountsPayable, charge1.JR_LocalCostAmt, charge1.JR_OSCostAmt, charge1.JR_RX_NKCostCurrency);

			var cal1 = TestObjectCreator.CreateCashAdvanceRequestLine(cah, charge1.JR_LocalCostAmt, charge1.JR_OSCostAmt);
			charge1.JR_CAL_APLine = cal1.PK;
			cal1.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;
			cal1.CAL_LocalPaidAmount = 100M;
			cal1.CAL_OSPaidAmount = 100M;

			Factory.Save();

			var arInvoiceResult = RunScript(arInvoice.PK);
			string[] cashAdvanceReceivedHeaders = { "CashAdvanceReceived" };
			object[] cashAdvanceReceivedValuesForARInvoice = { cal.CAL_LocalPaidAmount };
			AssertDataRow(arInvoiceResult.FirstOrDefault(), cashAdvanceReceivedHeaders, cashAdvanceReceivedValuesForARInvoice);

			var apInvoiceResult = RunScript(arInvoice.PK);
			object[] cashAdvanceReceivedValuesForAPInvoice = { cal1.CAL_LocalPaidAmount };
			AssertDataRow(arInvoiceResult.FirstOrDefault(), cashAdvanceReceivedHeaders, cashAdvanceReceivedValuesForAPInvoice);
		}

		static EnumerableRowCollection<DataRow> RunScript(ZGuid? transactionHeaderPK, int batchNumber = 0) =>
			DataUtils.GetDataTableFromQuery(Db.Connection,
				$"EXEC AccountingTransactionExportGetLines '{GlbCompany.CurrentCompany.GC_Code}', {batchNumber}, '{ZDate.Today}'{(transactionHeaderPK == null ? "" : $", '{transactionHeaderPK}'")}").AsEnumerable();

		Charge CreateCharge()
		{
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0m, TestObjectCreator.Agent, 1.0m);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 8M, 8M);
			charge.JR_AT_SellGSTRate = TestObjectCreator.VATSPV.PK;
			charge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			return charge;
		}

		ARInvoice CreateARInvoice(bool createJobCharge = true)
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0m, TestObjectCreator.ABIGAS);
			var arInvoiceLine = TestObjectCreator.CreateARInvoiceLine(arInvoice, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", 100.00m);
			arInvoiceLine.AL_AT = TestObjectCreator.GSTWithExtraRate.PK;
			arInvoiceLine.AL_SupplyType = "DSB";
			if (createJobCharge)
			{
				var charge = TestObjectCreator.CreateJobCharge(arInvoiceLine, TestObjectCreator.Job1, TestObjectCreator.CommentChargeCode, TestObjectCreator.AUD);
				charge.JR_InvoiceType = "FIN";
			}
			return arInvoice;
		}

		DirectPayment CreateDirectPayment()
		{
			var directPayment = TestObjectCreator.CreateDirectPayment(ZDateTime.Today, 150m, 50m, 250m, 50m);
			directPayment.Lines[0].AL_AT = TestObjectCreator.FREECAPGST.PK;
			directPayment.Lines[0].AL_SupplyType = "LOC";
			directPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			return directPayment;
		}

		APInvoice CreateAPInvoice()
		{
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 50m, 0m, 0m, 50m, 0m, 0m, TestObjectCreator.AALSHI);
			apInvoice.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			apInvoice.Lines[0].AL_SupplyType = "LOX";
			return apInvoice;
		}

		readonly string[] headers = {
"AL_LineAmount",
"AL_GSTVAT",
"AL_GSTVATExtra",
"AL_OSAmount",
"ChargeAmount",
"ChargeGST",
"ChargeAT_Code",
"ChargeAT_Description",
"ChargeAT_Type",
"ChargeAT_Rate",
"ChargeAT_ExtraTaxRateType",
"ChargeAT_ExtraRate",
"ChargeAT_RN_NKCountry",
"AT_Code",
"AT_Description",
"AT_Type",
"AT_Rate",
"AT_ExtraTaxRateType",
"AT_ExtraRate",
"AT_RN_NKCountry",
"GC_RX_NKLocalCurrency",
"chargeInvoiceType",
"chargeSellInvoiceCurrency",
"AL_SupplyType",
};

		readonly object[] valuesForAPInvoiceWithCharge = { -50M, -5M, 0M, -55M, 50M, 5M, "ZZGST1", "GST Rate 1", "RAT", 10M, string.Empty, 0M, "AU", "ZZGST1", "GST Rate 1", "RAT", 10M, string.Empty, 0M, "AU", "AUD", null, null, "LOX" };
		readonly object[] valuesForAPInvoice = { -50M, -5M, 0M, -55M, 0M, 0M, null, null, null, null, null, null, null, "ZZGST1", "GST Rate 1", "RAT", 10M, string.Empty, 0M, "AU", "AUD", null, null, "LOX" };
		readonly object[] valuesForDirectPayment = { -150M, -50M, 0M, -200M, 0M, 0M, null, null, null, null, null, null, null, "FRECAPGST", "CapitalRated", "CAP", 0M, string.Empty, 0M, "AU", "AUD", null, null, "LOC" };
		readonly object[] valuesForARInvoiceWithCharge = { 100M, 12.36M, 0M, 112.36M, 100M, 0M, "ZZGSTNEDU", "GSTANDEDU", "RAT", 12M, "EDU", 3M, "AU", "ZZGSTNEDU", "GSTANDEDU", "RAT", 12M, "EDU", 3M, "AU", "AUD", "FIN", "", "DSB" };
		readonly object[] valuesForARInvoice = { 100M, 12.36M, 0M, 112.36M, 0M, 0M, null, null, null, null, null, null, null, "ZZGSTNEDU", "GSTANDEDU", "RAT", 12M, "EDU", 3M, "AU", "AUD", null, null, "DSB" };
		readonly object[] valuesForWIP = { -8M, 0M, 0M, -8M, 8M, 0M, "ZZVATSPV", "VAT Remitted by Customer Tax", "RAT", 22M, "SPV", 0M, "IT", null, null, null, null, null, null, null, "AUD", "FIN", "USD", "" };
		readonly object[] valuesForACR = { 8M, 0M, 0M, 8M, 8M, 0M, "ZZGST1", "GST Rate 1", "RAT", 10M, "", 0M, "AU", null, null, null, null, null, null, null, "AUD", null, null, "" };
	}
}


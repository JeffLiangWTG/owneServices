

using System;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class OrgCreditControlledDocumentsApprovalBalancesTest : ScriptTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			collection.Add(CreateNewSettings(0, 0, AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly));

			AccountingMasterFilesRegistry.Instance.CreditControllerOverrideThreshold.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
		}

		AmountOrPercentageBasedThreeLevelAuthorisationRequirement CreateNewSettings(ZDecimal amount, ZDecimal percentage, ZString range, ZString auth)
		{
			var settings = new AmountOrPercentageBasedThreeLevelAuthorisationRequirement();
			settings.Amount = amount;
			settings.Percentage = percentage;
			settings.Range = range;
			settings.AuthorisationRequirement = auth;

			return settings;
		}

		public void TestOrgWithNoTransactions()
		{
			RunsScriptAndAssert("No transactions for org. Should return zeros.", 0, 0, 0, 0, 0, 0, TestObjectCreator.AALSHI.PK, GlbCompany.CurrentCompany.PK);
		}

		[TestDate(2014,03,26)]
		public void TestWithVariousDueDates_Midnight()
		{
			RunTestWithVariousDueDates();
		}

		[TestDate(2014, 03, 26, 12, 0, 0)]
		public void TestWithVariousDueDates_Midday()
		{
			RunTestWithVariousDueDates();
		}

		[TestDate(2014, 03, 26, 23, 59, 59)]
		public void TestWithVariousDueDates_JustBeforeMidnight()
		{
			RunTestWithVariousDueDates();
		}

		[TestDate(2014, 03, 26, 00, 00, 01)]
		public void TestWithVariousDueDates_JustAfterMidnight()
		{
			RunTestWithVariousDueDates();
		}

		void RunTestWithVariousDueDates()
		{
			CreateInvoice(typeof(ARInvoice), 10M, TestObjectCreator.AALSHI, 20, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			CreateInvoice(typeof(ARInvoice), 20M, TestObjectCreator.AALSHI, 50, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			CreateInvoice(typeof(ARInvoice), 100M, TestObjectCreator.AALSHI, 20, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.FinalInvoice);
			CreateInvoice(typeof(ARInvoice), 200M, TestObjectCreator.AALSHI, 50, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();

			RunsScriptAndAssert("Total outstanding amount and number of due days Passed for ALL, DSB and NOTDSB invoice type", 330M, 30M, 300M, 50, 50, 50, TestObjectCreator.AALSHI.PK, GlbCompany.CurrentCompany.PK);
		}

		[TestDate(2015, 11, 26)]
		public void TestWithMultiOrganisationsAcrossCompanies()
		{
			CreateInvoice(typeof(ARInvoice), 10M, TestObjectCreator.AALSHI, 10, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			CreateInvoice(typeof(ARInvoice), 20M, TestObjectCreator.ABIGAS, 30, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			CreateInvoice(typeof(ARInvoice), 30M, TestObjectCreator.AALSHI, 10, TestObjectCreator.NonCurrentCompany.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			CreateInvoice(typeof(ARInvoice), 100M, TestObjectCreator.AALSHI, 20, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.FinalInvoice);
			CreateInvoice(typeof(ARInvoice), 200M, TestObjectCreator.ABIGAS, 10, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.FinalInvoice);
			CreateInvoice(typeof(ARInvoice), 300M, TestObjectCreator.AALSHI, 10, TestObjectCreator.NonCurrentCompany.PK, InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();

			RunsScriptAndAssert("Should filter on the company and org.", 110M, 10M, 100M, 20, 10, 20, TestObjectCreator.AALSHI.PK, GlbCompany.CurrentCompany.PK);
			RunsScriptAndAssert("Should filter on the company and org.", 220M, 20M, 200M, 30, 30, 10, TestObjectCreator.ABIGAS.PK, GlbCompany.CurrentCompany.PK);
			RunsScriptAndAssert("Should filter on the company and org.", 330M, 30M, 300M, 10, 10, 10, TestObjectCreator.AALSHI.PK, TestObjectCreator.NonCurrentCompany.PK);
		}

		public void TestDSBvsNotDSB()
		{
			CreateInvoice(typeof(ARInvoice), 10M, TestObjectCreator.AALSHI, 20, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			CreateInvoice(typeof(ARInvoice), 100M, TestObjectCreator.AALSHI, 25, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();

			RunsScriptAndAssert("DSB And NotDSB should select different due days", 110M, 10M, 100M, 25, 20, 25, TestObjectCreator.AALSHI.PK, GlbCompany.CurrentCompany.PK);
		}

		public void TestNoDueDate()
		{
			CreateInvoice(typeof(ARInvoice), 10M, TestObjectCreator.AALSHI, 20, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			CreateInvoice(typeof(ARInvoice), 100M, TestObjectCreator.AALSHI, 20, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.FinalInvoice);
			CreateInvoice(typeof(ARInvoice), 20M, TestObjectCreator.AALSHI, null, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			CreateInvoice(typeof(ARInvoice), 200M, TestObjectCreator.AALSHI, null, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();

			RunsScriptAndAssert("Excludes the 20/200 because it has no due date", 110M, 10M, 100M, 20, 20, 20, TestObjectCreator.AALSHI.PK, GlbCompany.CurrentCompany.PK);
		}

		public void TestExcludesAP()
		{
			CreateInvoice(typeof(ARInvoice), 10M,  TestObjectCreator.AALSHI, 20, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			CreateInvoice(typeof(ARInvoice), 100M, TestObjectCreator.AALSHI, 20, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.FinalInvoice);
			CreateInvoice(typeof(APInvoice), 20M,  TestObjectCreator.AALSHI, 20, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			CreateInvoice(typeof(APInvoice), 200M, TestObjectCreator.AALSHI, 20, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.FinalInvoice);
			Factory.Save();

			RunsScriptAndAssert("Excludes the 20/200 because it is AP", 110M, 10M, 100M, 20, 20, 20, TestObjectCreator.AALSHI.PK, GlbCompany.CurrentCompany.PK);
		}

		public void TestWithFullyPaid()
		{
			InvoicingBase invoice;

			CreateInvoice(typeof(ARInvoice), 10M, TestObjectCreator.AALSHI, 20, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			CreateInvoice(typeof(ARInvoice), 10M, TestObjectCreator.AALSHI, 20, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.FinalInvoice);
			invoice = CreateInvoice(typeof(ARInvoice), 10M, TestObjectCreator.AALSHI, 20, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			FullyPayInvoice(invoice);
			invoice = CreateInvoice(typeof(ARInvoice), 10M, TestObjectCreator.AALSHI, 20, GlbCompany.CurrentCompany.PK, InvoiceTypesList.Codes.FinalInvoice);
			FullyPayInvoice(invoice);
			Factory.Save();

			RunsScriptAndAssert("Should take not full paid into account, but exclude fully paid", 20M, 10M, 10M, 20, 20, 20, TestObjectCreator.AALSHI.PK, GlbCompany.CurrentCompany.PK);
		}

		[TestDate(2014, 11, 25)]
		public void TestIncludesTransactionType()
		{
			var disbursementINV = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001001", TestObjectCreator.AUD, 1m, 10m, 0, 10m, 0, TestObjectCreator.AALSHI, TestObjectCreator.FRT.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			disbursementINV.AH_DueDate = ZDateTime.Today.AddDays(-2);
			var finalINV = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001002", TestObjectCreator.AUD, 1m, 100m, 0, 100m, 0, TestObjectCreator.AALSHI, TestObjectCreator.FRT.PK, InvoiceTypesList.Codes.FinalInvoice);
			finalINV.AH_DueDate = ZDateTime.Today.AddDays(-2);
			Factory.Save();

			RunsScriptAndAssert("Transaction type should be included " + TransactionTypes.Invoice, 110M, 10M, 100M, 2, 2, 2, TestObjectCreator.AALSHI.PK, GlbCompany.CurrentCompany.PK);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			var disbursementCRD = TestObjectCreator.CreateARCreditNoteWithLine("00001003", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "Desc CRD", job, TestObjectCreator.FRT, 50m, ZDateTime.Today, false);
			disbursementCRD.AH_TransactionCategory = InvoiceTypesList.Codes.DisbursementInvoice;
			disbursementCRD.AH_DueDate = ZDateTime.Today.AddDays(-3);
			TestObjectCreator.CreateJobCharge(disbursementCRD.Lines[0], job, TestObjectCreator.FRT, TestObjectCreator.AUD);
			var finalCRD2 = TestObjectCreator.CreateARCreditNoteWithLine("00001004", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "Desc CRD", job, TestObjectCreator.FRT, 300m, ZDateTime.Today.AddDays(-3), false);
			finalCRD2.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			finalCRD2.AH_DueDate = ZDateTime.Today.AddDays(-3);
			TestObjectCreator.CreateJobCharge(finalCRD2.Lines[0], job, TestObjectCreator.FRT, TestObjectCreator.AUD);
			Factory.Save();

			RunsScriptAndAssert("Transaction type should be included " + TransactionTypes.CreditNote, -240M, -40M, -200M, 3, 3, 3, TestObjectCreator.AALSHI.PK, GlbCompany.CurrentCompany.PK);

			var distAdjustmentNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARAdjustmentNote), "00001005", TestObjectCreator.AUD, 1m, 20m, 0, 20m, 0, TestObjectCreator.AALSHI, TestObjectCreator.FRT.PK, InvoiceTypesList.Codes.DisbursementInvoice);
			distAdjustmentNote.AH_DueDate = ZDateTime.Today.AddDays(-4);
			var finalAdjustmentNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARAdjustmentNote), "00001006", TestObjectCreator.AUD, 1m, 100m, 0, 100m, 0, TestObjectCreator.AALSHI, TestObjectCreator.FRT.PK, InvoiceTypesList.Codes.FinalInvoice);
			finalAdjustmentNote.AH_DueDate = ZDateTime.Today.AddDays(-4);
			Factory.Save();

			RunsScriptAndAssert("Transaction type should be included " + TransactionTypes.AdjustmentNote, -120M, -20M, -100M, 4, 4, 4, TestObjectCreator.AALSHI.PK, GlbCompany.CurrentCompany.PK);

			var payment = TestObjectCreator.CreateARPayment(1m, 60m, ZDateTime.Today, ZDateTime.Today, TestObjectCreator.AALSHI.PK, TestObjectCreator.AUDBankAccount.PK);
			payment.AH_DueDate = ZDateTime.Today.AddDays(-5);
			Factory.Save();

			RunsScriptAndAssert("Transaction type should be included " + TransactionTypes.Payment, -60M, -20M, -40M, 5, 4, 5, TestObjectCreator.AALSHI.PK, GlbCompany.CurrentCompany.PK);

			var receipt = TestObjectCreator.CreateARReceipt(ReceiptTypes.Cheque, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100m, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			RunsScriptAndAssert("Transaction type should be included " + TransactionTypes.Receipt, -60M, -20M, -40M, 5, 4, 5, TestObjectCreator.AALSHI.PK, GlbCompany.CurrentCompany.PK);

			var transfer = TestObjectCreator.CreateTransfer<ARTransfer>(30m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK, TestObjectCreator.AALSHI.PK);
			Factory.Save();

			RunsScriptAndAssert("Transaction type should be included " + TransactionTypes.Transfer, -30M, -20M, -10M, 5, 4, 5, TestObjectCreator.AALSHI.PK, GlbCompany.CurrentCompany.PK);

			var contra = TestObjectCreator.CreateContra(50m, ZDateTime.Today, TestObjectCreator.AALSHI.PK, TestObjectCreator.ABIGAS.PK);
			Factory.Save();

			RunsScriptAndAssert("Transaction type should be included " + TransactionTypes.Contra, -80M, -20M, -60M, 5, 4, 5, TestObjectCreator.AALSHI.PK, GlbCompany.CurrentCompany.PK);

			var journal = TestObjectCreator.CreateJournal<ARJournal>(70m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
			Factory.Save();

			RunsScriptAndAssert("Transaction type should be included " + TransactionTypes.Journal, -10M, -20M, 10M, 5, 4, 5, TestObjectCreator.AALSHI.PK, GlbCompany.CurrentCompany.PK);

			var excludeType = TestObjectCreator.CreateTransactionPendingAllocation("000010008", TestObjectCreator.AALSHI, 50m, 0m, TestObjectCreator.AUD, 1m);
			Factory.Save();

			RunsScriptAndAssert("Transaction type should not be included " + TransactionTypes.InvoicePendingAllocation, -10M, -20M, 10M, 5, 4, 5, TestObjectCreator.AALSHI.PK, GlbCompany.CurrentCompany.PK);
		}

		#region Implementation

		void FullyPayInvoice(InvoicingBase invoice)
		{
			var journalToMatch = Factory.New<AccTransactionHeader>();
			journalToMatch.AH_InvoiceDate = ZDateTime.Now.ToDateTime();
			journalToMatch.AH_Ledger = invoice.AH_Ledger;
			journalToMatch.AH_TransactionType = TransactionTypes.Journal;
			journalToMatch.AH_RX_NKTransactionCurrency = invoice.AH_RX_NKTransactionCurrency;
			journalToMatch.AH_ExchangeRate = invoice.AH_ExchangeRate;
			journalToMatch.AH_InvoiceAmount = -invoice.AH_InvoiceAmount;
			journalToMatch.AH_GSTAmount = -invoice.AH_GSTAmount;
			journalToMatch.AH_OutstandingAmount = -invoice.AH_OutstandingAmount;
			journalToMatch.AH_PostDate = invoice.AH_PostDate;
			journalToMatch.AH_GB = invoice.AH_GB;
			journalToMatch.AH_GE = invoice.AH_GE;
			journalToMatch.AH_TransactionNum = invoice.AH_TransactionNum + "J";

			var matchlinks = new TransactionMatchLinkGroup(Factory);
			var link1 = matchlinks.AddNew();
			var link2 = matchlinks.AddNew();
			link1.AP_AH = invoice.PK;
			link1.AP_Amount = invoice.AH_OutstandingAmount;
			link2.AP_AH = journalToMatch.PK;
			link2.AP_Amount = journalToMatch.AH_OutstandingAmount;
			link1.AP_MatchDate = link2.AP_MatchDate = invoice.AH_PostDate;
			link1.AP_MatchGroupNum = link2.AP_MatchGroupNum = "100100";

			journalToMatch.AH_OutstandingAmount = 0;
			invoice.AH_OutstandingAmount = 0;
		}

		int transactionNumber;

		InvoicingBase CreateInvoice(Type type, ZDecimal amount, OrgHeader org, ZInt? overdueDays, ZGuid companyPK, ZString transactionCategory, string txnType = "")
		{
			var branch = Factory.Load<GlbCompany>(companyPK).FirstActiveBranch;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var inv = TestObjectCreator.CreateInvoiceWithLine(type, (transactionNumber++).ToString(), TestObjectCreator.AUD, 1M, amount, 0M, amount, 0M, org, ZGuid.Empty);
				inv.AH_TransactionCategory = transactionCategory;
				inv.AH_DueDate = overdueDays.HasValue ? ZDateTime.Today.AddDays(-overdueDays.Value) : ZDateTime.Empty;

				AssertEquals("precondition", amount, (inv.AH_Ledger == "AP" ? -1 : 1) * inv.AH_OutstandingAmount);

				if (!string.IsNullOrEmpty(txnType))
				{
					inv.AH_TransactionType = txnType;

					if ((inv.AH_Ledger == LedgerTypes.AccountsPayable || inv.AH_Ledger == LedgerTypes.AccountsReceivable)
						&& inv.AH_TransactionType == TransactionTypes.Journal)
					{
						inv.AH_AG = TestObjectCreator.GetGLAccountFromDB("6810.00.00").PK;
					}
				}
				return inv;
			}
		}

		class ScriptResult
		{
			public ZDecimal SumAllAmount, SumDsbAmount, SumNotDsbAmount;
			public ZInt SumAllDueDays, SumDsbDueDays, SumNotDsbDueDays;
		}

		void RunsScriptAndAssert(string message, ZDecimal expectedSumAllAmount, ZDecimal expectedSumDsbAmount, ZDecimal expectedSumNotDsbAmount, ZInt expectedSumAllDueDays, ZInt expectedSumDsbDueDays, ZInt expectedSumNotDsbDueDays, ZGuid orgPK, ZGuid companyPK)
		{
			var result = RunScript(orgPK, companyPK);
			AssertEquals(message + " - expectedSumAllAmount", expectedSumAllAmount, result.SumAllAmount);
			AssertEquals(message + " - expectedSumDsbAmount", expectedSumDsbAmount, result.SumDsbAmount);
			AssertEquals(message + " - expectedSumNotDsbAmount", expectedSumNotDsbAmount, result.SumNotDsbAmount);
			AssertEquals(message + " - expectedSumAllDueDays", expectedSumAllDueDays, result.SumAllDueDays);
			AssertEquals(message + " - expectedSumDsbDueDays", expectedSumDsbDueDays, result.SumDsbDueDays);
			AssertEquals(message + " - expectedSumNotDsbDueDays", expectedSumNotDsbDueDays, result.SumNotDsbDueDays);
		}

		ScriptResult RunScript(ZGuid orgPK, ZGuid companyPK)
		{
			string sql = string.Format(@"select * From OrgCreditControlledDocumentsApprovalBalances(
				'{0}', --@OrgPK
				'{1}', --@CompanyPK
				'{2}' --@LocalDateTime
			)",
			orgPK, companyPK, ZDateTime.Now.ToISO8601String());

			var row = DataUtils.GetDataTableFromQuery(Db.Connection, sql).Rows[0];
			return new ScriptResult { SumAllAmount = new ZDecimal(row["SumAllAmount"]), SumDsbAmount = new ZDecimal(row["SumDsbAmount"]), SumNotDsbAmount = new ZDecimal(row["SumNotDsbAmount"]),
															  SumAllDueDays = new ZInt(row["SumAllDueDays"]), SumDsbDueDays = new ZInt(row["SumDsbDueDays"]), SumNotDsbDueDays = new ZInt(row["SumNotDsbDueDays"]) };
		}

		#endregion

	}
}



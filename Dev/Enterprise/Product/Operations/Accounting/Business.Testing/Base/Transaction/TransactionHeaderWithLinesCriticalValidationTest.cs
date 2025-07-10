using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CriticalValidation;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public abstract class TransactionHeaderWithLinesCriticalValidationTest : TransactionHeaderCriticalValidationTest
	{
		void SetupTransactionToCheckLineTypeErrorMessage(TransactionHeaderWithLines transaction)
		{
			ZDateTime transactionDate = new ZDateTime(2009, 11, 20);
			ZDecimal sumOfLinesAmount = 0;
			transaction.AH_PostDate = transactionDate;
			transaction.AH_TransactionNum = "123";
			transaction.AH_GC = GlbCompany.CurrentCompany.PK;
			transaction.AH_GB = GlbBranch.CurrentBranch.PK;
			transaction.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			foreach (TransactionLine line in transaction.Lines)
			{
				line.AL_PostDate = transactionDate;
				line.AL_ReverseDate = transactionDate;
				line.AL_OverseasTotal = 100M;
				line.AL_LineAmount = line.AL_OSAmount;
				//line.AL_GSTVAT = 0M;
				sumOfLinesAmount += line.AL_OSAmount;
			}
			transaction.AH_OSTotal = sumOfLinesAmount;
			transaction.AH_OSExTaxAmount = transaction.AH_OSTotalAmount;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "Baseline")]
		public void TestCheckTransactionLineBranchBelongToTransactionHeaderCompany()
		{
			ARInvoice invoice = (ARInvoice)Factory.New(typeof(ARInvoice), new Guid("aa3b5365-97c6-4d32-99ef-427825833a01"));
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.Lines.Add(Factory.New(typeof(ARInvoiceLine), new Guid("14605091-4fd3-43b4-8403-6db6a05b1b80")));
			invoice.Lines[0].AL_LineType = TransactionLineTypes.Revenue;
			SetupTransactionToCheckLineTypeErrorMessage(invoice);

			//Change AL_GB to a branch in a different company
			ZQuery getAnotherCompanyFilter = new ZQuery(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_Code);
			GlbCompany anotherCompany = Factory.LoadTop1<GlbCompany>(getAnotherCompanyFilter);
			ZQuery branchInTheOtherCompanyFilter = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, anotherCompany.PK);
			GlbBranch branchInTheOtherCompany = Factory.LoadTop1<GlbBranch>(branchInTheOtherCompanyFilter);
			invoice.Lines[0].AL_GC = anotherCompany.PK;
			invoice.Lines[0].AL_GB = branchInTheOtherCompany.PK;
			var expectedMessage =
@"Header: PK = aa3b5365-97c6-4d32-99ef-427825833a01, Ledger = AR, Transaction Type = INV, Invoice Date = , Post Date = 20-Nov-09 00:00:00, Invoice Amount = 100, GST Amount = 0, OS Total = 100, Exchange Rate = 1, Currency = AUD, Outstanding Amount = 100, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 123, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.
Line Details:
Line: PK = 14605091-4fd3-43b4-8403-6db6a05b1b80, Charge Code = , GL Account = , Type = REV, OS Amount = 100, Local Amount = 100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = 20-Nov-09 00:00:00, Reverse Date = 20-Nov-09 00:00:00, Post To GL = N, Reverse To GL = N, Header PK = aa3b5365-97c6-4d32-99ef-427825833a01, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
";
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Transaction line branch does not belong to transaction header company", true, CriticalValidationErrorType.TransactionLineBranchDoesNotBelongToTransactionHeaderCompany_2, "Transaction line branch does not belong to transaction header company", expectedMessage);
			AssertOnSavingCheck(invoice, testCase);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "Baseline")]
		public void TestCheckTransactionHeaderBranchBelongToTransactionHeaderCompany()
		{
			ARInvoice invoice = (ARInvoice)Factory.New(typeof(ARInvoice), new Guid("aa3b5365-97c6-4d32-99ef-427825833a01"));
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.Lines.Add(Factory.New(typeof(ARInvoiceLine), new Guid("14605091-4fd3-43b4-8403-6db6a05b1b80")));
			invoice.Lines[0].AL_LineType = TransactionLineTypes.Revenue;
			SetupTransactionToCheckLineTypeErrorMessage(invoice);

			//Change AH_GC to a different company - AH_GB set method resets AH_GC
			ZQuery getAnotherCompanyFilter = new ZQuery(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_Code);
			GlbCompany anotherCompany = Factory.LoadTop1<GlbCompany>(getAnotherCompanyFilter);
			invoice.AH_GC = anotherCompany.PK;
			var expectedMessage =
@"Header: PK = aa3b5365-97c6-4d32-99ef-427825833a01, Ledger = AR, Transaction Type = INV, Invoice Date = , Post Date = 20-Nov-09 00:00:00, Invoice Amount = 100, GST Amount = 0, OS Total = 100, Exchange Rate = 1, Currency = AUD, Outstanding Amount = 100, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 123, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.
Line Details:
Line: PK = 14605091-4fd3-43b4-8403-6db6a05b1b80, Charge Code = , GL Account = , Type = REV, OS Amount = 100, Local Amount = 100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = 20-Nov-09 00:00:00, Reverse Date = 20-Nov-09 00:00:00, Post To GL = N, Reverse To GL = N, Header PK = aa3b5365-97c6-4d32-99ef-427825833a01, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
";
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Transaction header branch does not belong to transaction header company", true, CriticalValidationErrorType.TransactionHeaderBranchDoesNotBelongToTransactionHeaderCompany_2, "Transaction header branch does not belong to transaction header company", expectedMessage);
			AssertOnSavingCheck(invoice, testCase);
		}

		public void TestTransactionLineAmountTotalDoesNotMatchTransactionHeaderInvoiceAmount()
		{
			var expectedUserErrorMessage =
@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Transaction Line Amount Total does not match the Transaction Header Invoice Amount. Transaction Line Amount Total is 200 but Header Invoice Amount is 100.";

			var expectedTechDetails = ZString.Empty;

			foreach (Type type in new[] { typeof(APInvoice), typeof(APCreditNote), typeof(APAdjustmentNote), typeof(ARInvoice), typeof(ARCreditNote), typeof(ARAdjustmentNote), typeof(DirectPayment), typeof(DirectReceipt) })
			{
				Type type_cachedForDelegates1 = type;

				var header = (TransactionHeaderWithLines)Factory.NewWithValidTestData(type_cachedForDelegates1);
				header.AH_GC = GlbCompany.CurrentCompany.PK;
				header.AH_GB = GlbBranch.CurrentBranch.PK;
				header.AH_RX_NKTransactionCurrency = "AUD";
				header.AH_ExchangeRate = 1M;
				header.AH_InvoiceAmount = 100m;
				header.AH_OSTotal = 150m;
				header.AH_GSTAmount = 50m;
				header.AH_OutstandingAmount = header.AH_InvoiceAmount + header.AH_GSTAmount;

				var line = (TransactionLine)header.Lines.AddNew(header.DependentTransactionLineType);
				line.AL_RX_NKTransactionCurrency = "AUD";
				line.AL_LineAmount = 200m;
				line.AL_OSAmount = 150m;
				line.AL_GSTVAT = 50m;
				expectedTechDetails = header.GetTransactionHeaderWithLinesInfo();

				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Transaction : {0}, Transaction Line Amount Total does not match the Transaction Header Invoice Amount.", type_cachedForDelegates1.Name),
					true, CriticalValidationErrorType.SumOfTransactionLineAmountsDoesNotMatchTransactionHeaderInvoiceAmount_3, expectedUserErrorMessage, expectedTechDetails);
				AssertOnSavingCheck(header, testCase);
			}
		}

		public void TestTransactionLineAmountTotalDoesNotMatchTransactionHeaderInvoiceAmount_WithPAToAPTransactionLineMonitor()
		{
			AccountingConfigurationRegistry.Instance.EnableTransactionLineMonitor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("000001", TestObjectCreator.Creditor1, 100m);
			Factory.Save();

			var (header, errorMsg) = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation);
			header.AH_GC = GlbCompany.CurrentCompany.PK;
			header.AH_GB = GlbBranch.CurrentBranch.PK;
			header.AH_RX_NKTransactionCurrency = CurrencyCodes.Australia;

			var line = header.Lines.AddNew(header.DependentTransactionLineType);
			line.AL_RX_NKTransactionCurrency = CurrencyCodes.Australia;
			line.AL_LineAmount = 100m;
			line.AL_OSAmount = 100m;
			line.AL_GSTVAT = 0m;

			header.Lines.RemoveAll();

			header.AH_ExchangeRate = 1m;
			header.AH_InvoiceAmount = 100m;
			header.AH_OSTotal = 100m;
			header.AH_GSTAmount = 0m;
			header.AH_OutstandingAmount = 100m;

			var expectedUserErrorMessage =
@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Transaction Line Amount Total does not match the Transaction Header Invoice Amount. Transaction Line Amount Total is 0 but Header Invoice Amount is 100.";

			var expectedPAToAPMonitorInfo1 =
@"Last 10 counts of lines:
CheckTransactionLineTotalsMatchTransactionHeaderAmounts: 0";

			var expectedPAToAPMonitorInfo2 =
@"Stack trace of last line remove:
   at";

			var expectedPAToAPMonitorInfo3 =
@"Total calls of adding line: 1
Total calls of removing line: 1";

			var expectedLineInfo = header.GetTransactionHeaderWithLinesInfo();

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Header has no linked lines", true,
				CriticalValidationErrorType.SumOfTransactionLineAmountsDoesNotMatchTransactionHeaderInvoiceAmount_3,
				expectedUserErrorMessage, expectedPAToAPMonitorInfo1, expectedPAToAPMonitorInfo2, expectedPAToAPMonitorInfo3, expectedLineInfo);

			AssertOnSavingCheck(header, testCase);
		}

		public void TestTransactionLineGSTTotalDoesNotMatchTransactionHeaderGSTAmount()
		{
			var expectedUserErrorMessage =
@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Transaction Line GST Total does not match the Transaction Header GST Amount. Transaction Line GST Total is 10 but Header GST Amount is 50.";

			foreach (Type type in new[] { typeof(APInvoice), typeof(APCreditNote), typeof(APAdjustmentNote), typeof(ARInvoice), typeof(ARCreditNote), typeof(ARAdjustmentNote), typeof(DirectPayment), typeof(DirectReceipt) })
			{
				Type type_cachedForDelegates1 = type;

				var header = (TransactionHeaderWithLines)Factory.NewWithValidTestData(type_cachedForDelegates1);
				header.AH_GC = GlbCompany.CurrentCompany.PK;
				header.AH_GB = GlbBranch.CurrentBranch.PK;
				header.AH_RX_NKTransactionCurrency = "AUD";
				header.AH_ExchangeRate = 1M;
				header.AH_InvoiceAmount = 100m;
				header.AH_OSTotal = 150m;
				header.AH_GSTAmount = 50m;
				header.AH_OutstandingAmount = header.AH_InvoiceAmount + header.AH_GSTAmount;

				var line = (TransactionLine)header.Lines.AddNew(header.DependentTransactionLineType);
				line.AL_RX_NKTransactionCurrency = "AUD";
				line.AL_LineAmount = 100m;
				line.AL_OSAmount = 150m;
				line.AL_GSTVAT = 10m;
				var expectedTechDetails = header.GetTransactionHeaderWithLinesInfo();

				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Transaction : {0}, Transaction Line GST Total does not match the Transaction Header GST Amount.", type_cachedForDelegates1.Name),
					true, CriticalValidationErrorType.SumOfTransactionLineGSTAmountsDoesNotMatchTransactionHeaderGSTAmount_2, expectedUserErrorMessage, expectedTechDetails);
				AssertOnSavingCheck(header, testCase);
			}
		}

		public void TestTransactionLineOSAmountTotalDoesNotMatchTransactionHeaderOSAmount()
		{
			var expectedUserErrorMessage =
@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Transaction Line OS Amount Total does not match the Transaction Header OS Amount. Transaction Line OS Amount Total is 100 but Header OS Amount is 250.";

			foreach (Type type in new[] { typeof(APInvoice), typeof(APCreditNote), typeof(APAdjustmentNote), typeof(ARInvoice), typeof(ARCreditNote), typeof(ARAdjustmentNote), typeof(DirectPayment), typeof(DirectReceipt) })
			{
				Type type_cachedForDelegates1 = type;

				var header = (TransactionHeaderWithLines)Factory.NewWithValidTestData(type_cachedForDelegates1);
				header.AH_GC = GlbCompany.CurrentCompany.PK;
				header.AH_GB = GlbBranch.CurrentBranch.PK;
				header.AH_RX_NKTransactionCurrency = "AUD";
				header.AH_ExchangeRate = 1M;
				header.AH_InvoiceAmount = 200m;
				header.AH_OSTotal = 250m;
				header.AH_GSTAmount = 50m;
				header.AH_OutstandingAmount = header.AH_InvoiceAmount + header.AH_GSTAmount;

				var line = (TransactionLine)header.Lines.AddNew(header.DependentTransactionLineType);
				line.AL_RX_NKTransactionCurrency = "AUD";
				line.AL_LineAmount = 200m;
				line.AL_OSAmount = 100m;
				line.AL_GSTVAT = 50m;
				var expectedTechDetails = header.GetTransactionHeaderWithLinesInfo();

				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Transaction : {0}, Transaction Line OS Amount Total does not match the Transaction Header OS Amount.", type_cachedForDelegates1.Name),
					true, CriticalValidationErrorType.SumOfTransactionLineOSAmountsDoesNotMatchTransactionHeaderOSTotalAmount_3, expectedUserErrorMessage, expectedTechDetails);
				AssertOnSavingCheck(header, testCase);
			}

			foreach (Type type in new[] { typeof(APInvoice), typeof(APCreditNote), typeof(APAdjustmentNote), typeof(ARInvoice), typeof(ARCreditNote), typeof(ARAdjustmentNote), typeof(DirectPayment), typeof(DirectReceipt) })
			{
				Type type_cachedForDelegates2 = type;

				var header = (TransactionHeaderWithLines)Factory.NewWithValidTestData(type_cachedForDelegates2);
				header.AH_GC = GlbCompany.CurrentCompany.PK;
				header.AH_GB = GlbBranch.CurrentBranch.PK;
				header.AH_RX_NKTransactionCurrency = "AUD";
				header.AH_InvoiceAmount = 200m;
				header.AH_OSTotal = 250m;
				header.AH_GSTAmount = 50m;
				header.AH_OutstandingAmount = header.AH_InvoiceAmount + header.AH_GSTAmount;

				var line = (TransactionLine)header.Lines.AddNew(header.DependentTransactionLineType);
				line.AL_RX_NKTransactionCurrency = "USD";
				line.AL_ExchangeRate = 2m;
				line.AL_LineAmount = 200m;
				line.AL_OSAmount = 500m;
				line.AL_GSTVAT = 50m;

				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Transaction : {0}, Should not check transaction line OS amount total against transaction header OS amount because currency is different in header and line.", type_cachedForDelegates2.Name));
				AssertOnSavingCheck(header, testCase);
			}
		}

		public void TestTransactionLineOSAmountTotalDoesNotMatchTransactionHeaderOSAmount_WithOtherTaxes()
		{
			var expectedUserErrorMessage =
@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Transaction Line OS Amount Total and Tax Transaction Amount does not match the Transaction Header OS Amount. Transaction Line OS Amount Total is 250, Tax Transaction Amount is 10, but Header OS Amount is 250.";

			foreach (Type type in new[] { typeof(APInvoice), typeof(APCreditNote), typeof(APAdjustmentNote), typeof(ARInvoice), typeof(ARCreditNote), typeof(ARAdjustmentNote), typeof(DirectPayment), typeof(DirectReceipt) })
			{
				Type type_cachedForDelegates1 = type;

				var header = (TransactionHeaderWithLines)Factory.NewWithValidTestData(type_cachedForDelegates1);
				header.AH_GC = GlbCompany.CurrentCompany.PK;
				header.AH_GB = GlbBranch.CurrentBranch.PK;
				header.AH_RX_NKTransactionCurrency = "AUD";
				header.AH_ExchangeRate = 1M;
				header.AH_InvoiceAmount = 200m;
				header.AH_OSTaxAmountOtherTaxes = 10m;
				header.AH_OSTotal = 250m;
				header.AH_LocalTaxAmountOtherTaxes = 0m;
				header.AH_GSTAmount = 50m;
				header.AH_OutstandingAmount = header.AH_InvoiceAmount + header.AH_GSTAmount;

				var line = (TransactionLine)header.Lines.AddNew(header.DependentTransactionLineType);
				line.AL_RX_NKTransactionCurrency = "AUD";
				line.AL_LineAmount = 200m;
				line.AL_OSAmount = 250m;
				line.AL_GSTVAT = 50m;

				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Transaction : {0}, lines and header total are matching, but Tax Transaction amount is different.", type_cachedForDelegates1.Name),
					true, CriticalValidationErrorType.SumOfTransactionLineOSAmountsDoesNotMatchTransactionHeaderOSTotalAmount_3, expectedUserErrorMessage);
				AssertOnSavingCheck(header, testCase);

				line.AL_OSAmount = 100m;
				testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Transaction : {0}, lines and header total are not matching as well to test that we report correct values in message.", type_cachedForDelegates1.Name),
					true, CriticalValidationErrorType.SumOfTransactionLineOSAmountsDoesNotMatchTransactionHeaderOSTotalAmount_3,
					"Transaction Line OS Amount Total is 100, Tax Transaction Amount is 10, but Header OS Amount is 250.");
				AssertOnSavingCheck(header, testCase);
			}

			foreach (Type type in new[] { typeof(APInvoice), typeof(APCreditNote), typeof(APAdjustmentNote), typeof(ARInvoice), typeof(ARCreditNote), typeof(ARAdjustmentNote), typeof(DirectPayment), typeof(DirectReceipt) })
			{
				Type type_cachedForDelegates2 = type;

				var header = (TransactionHeaderWithLines)Factory.NewWithValidTestData(type_cachedForDelegates2);
				header.AH_GC = GlbCompany.CurrentCompany.PK;
				header.AH_GB = GlbBranch.CurrentBranch.PK;
				header.AH_RX_NKTransactionCurrency = "AUD";
				header.AH_InvoiceAmount = 200m;
				header.AH_OSTaxAmountOtherTaxes = 10m;
				header.AH_OSTotal = 260m;
				header.AH_LocalTaxAmountOtherTaxes = 0m;
				header.AH_GSTAmount = 60m;
				header.AH_OutstandingAmount = header.AH_InvoiceAmount + header.AH_GSTAmount;

				var line = (TransactionLine)header.Lines.AddNew(header.DependentTransactionLineType);
				line.AL_RX_NKTransactionCurrency = "USD";
				line.AL_ExchangeRate = 2m;
				line.AL_LineAmount = 200m;
				line.AL_OSAmount = 500m;
				line.AL_GSTVAT = 60m;

				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("Transaction : {0}, Should not check transaction line OS amount total against transaction header OS amount because currency is different in header and line.", type_cachedForDelegates2.Name));
				AssertOnSavingCheck(header, testCase);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:Do Not Cast Factory Method", Justification = "Baseline")]
		public void TestSumOfGLJournalLinesEqualsToZero()
		{
			var journal = (GLJournal)Factory.New(typeof(GLJournal), new Guid("aa3b5365-97c6-4d32-99ef-427825833a01"));
			journal.AH_Ledger = LedgerTypes.General;

			journal.Lines.Add(Factory.New(typeof(GLJournalLine), new Guid("14605091-4fd3-43b4-8403-6db6a05b1b80")));
			journal.Lines.Add(Factory.New(typeof(GLJournalLine), new Guid("492fccfd-4ed6-4806-9922-eed48235cfce")));
			SetupTransactionToCheckLineTypeErrorMessage(journal);
			journal.Lines[1].AL_LineType = "GJL";
			journal.Lines[0].AL_LineType = "GJL";
			journal.Lines[0].AL_LineAmount = 10m;
			journal.Lines[1].AL_LineAmount = 10m;
			journal.AH_TransactionType = TransactionTypes.GLStandardJournal;

			var expectedTechDetails =
@"Header: PK = aa3b5365-97c6-4d32-99ef-427825833a01, Ledger = GL, Transaction Type = GJL, Invoice Date = , Post Date = 20-Nov-09 00:00:00, Invoice Amount = 200, GST Amount = 0, OS Total = 200, Exchange Rate = 1, Currency = AUD, Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 123, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.
Line Details:
Line: PK = 14605091-4fd3-43b4-8403-6db6a05b1b80, Charge Code = , GL Account = , Type = GJL, OS Amount = 100, Local Amount = 10, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = , Post Date = 20-Nov-09 00:00:00, Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = aa3b5365-97c6-4d32-99ef-427825833a01, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
Line: PK = 492fccfd-4ed6-4806-9922-eed48235cfce, Charge Code = , GL Account = , Type = GJL, OS Amount = 100, Local Amount = 10, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = , Post Date = 20-Nov-09 00:00:00, Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = aa3b5365-97c6-4d32-99ef-427825833a01, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
";

			var expectedUserErrorMessage =
@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Transaction Line Amount Total must be zero for a Header with GL ledger GJL transaction type but was 20.";

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Transaction Line Amount Total must be zero for GL Journals", true, CriticalValidationErrorType.SumOfTransactionLineAmountsDoesNotEqualToZero_6, expectedUserErrorMessage, expectedTechDetails);
			AssertOnSavingCheck(journal, testCase);

			journal.GenerateReverseTransaction(true);
			var reversedJournal = (GLJournal)(journal.ReverseTransaction);
			reversedJournal.AH_TransactionNum = "123";

			expectedTechDetails = string.Format(@"Header: PK = {0}, Ledger = GL, Transaction Type = GJL, Invoice Date = {1}, Post Date = {2}, Invoice Amount = -20, GST Amount = 0, OS Total = -20, Exchange Rate = 1, Currency = AUD, Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 123, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.
Line Details:
Line: PK = {3}, Charge Code = , GL Account = , Type = GJL, OS Amount = 0, Local Amount = -10, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = , Post Date = 20-Nov-09 00:00:00, Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {0}, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
Line: PK = {4}, Charge Code = , GL Account = , Type = GJL, OS Amount = 0, Local Amount = -10, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = , Post Date = 20-Nov-09 00:00:00, Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {0}, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
Original Transaction:
Header: PK = aa3b5365-97c6-4d32-99ef-427825833a01, Ledger = GL, Transaction Type = GJL, Invoice Date = , Post Date = 20-Nov-09 00:00:00, Invoice Amount = 200, GST Amount = 0, OS Total = 200, Exchange Rate = 1, Currency = AUD, Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 123, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.
Line Details:
Line: PK = 14605091-4fd3-43b4-8403-6db6a05b1b80, Charge Code = , GL Account = , Type = GJL, OS Amount = 100, Local Amount = 10, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = , Post Date = 20-Nov-09 00:00:00, Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = aa3b5365-97c6-4d32-99ef-427825833a01, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
Line: PK = 492fccfd-4ed6-4806-9922-eed48235cfce, Charge Code = , GL Account = , Type = GJL, OS Amount = 100, Local Amount = 10, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = , Post Date = 20-Nov-09 00:00:00, Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = aa3b5365-97c6-4d32-99ef-427825833a01, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
", reversedJournal.PK, ZDateTime.Now, ZDateTime.Today, reversedJournal.Lines[0].PK, reversedJournal.Lines[1].PK);

			expectedUserErrorMessage =
@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Transaction Line Amount Total must be zero for a Header with GL ledger GJL transaction type but was -20.";

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("Transaction Line Amount Total must be zero for GL Journals", true, CriticalValidationErrorType.SumOfTransactionLineAmountsDoesNotEqualToZero_6, expectedUserErrorMessage, expectedTechDetails);
			AssertOnSavingCheck(reversedJournal, testCase);
		}
		public void TestSumOfJRJLinesEqualsToZero()
		{
			ZDateTime invoiceDate = new ZDateTime(2009, 11, 20);

			var journal = Factory.New<JobRevenueJournal>();
			journal.AH_TransactionNum = "123";

			journal.AH_InvoiceDate = invoiceDate;
			SetupTransactionToCheckLineTypeErrorMessage(journal);

			var line1 = journal.JournalLines.AddNew();
			var line2 = journal.JournalLines.AddNew();

			line1.AL_PostDate = invoiceDate;
			line2.AL_PostDate = invoiceDate;

			line1.AL_LineAmount = 100m;
			line2.AL_LineAmount = 10m;

			var expectedTechDetails =
$@"Header: PK = {journal.PK}, Ledger = JC, Transaction Type = JRJ, Invoice Date = 20-Nov-09 00:00:00, Post Date = 20-Nov-09 00:00:00, Invoice Amount = 0, GST Amount = 0, OS Total = 0, Exchange Rate = 1, Currency = AUD, Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 123, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.
Line Details:
Line: PK = {line1.PK}, Charge Code = , GL Account = , Type = REV, OS Amount = 0, Local Amount = 100, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = 20-Nov-09 00:00:00, Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {journal.PK}, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
Line: PK = {line2.PK}, Charge Code = , GL Account = , Type = REV, OS Amount = 0, Local Amount = 10, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = 20-Nov-09 00:00:00, Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {journal.PK}, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.
";

			var expectedUserErrorMessage =
@"An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: Transaction Line Amount Total must be zero for a Header with JC ledger JRJ transaction type but was 110.";

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Transaction Line Amount Total must be zero for JR Journals", true, CriticalValidationErrorType.SumOfTransactionLineAmountsDoesNotEqualToZero_6, expectedUserErrorMessage, expectedTechDetails);
			AssertOnSavingCheck(journal, testCase);
		}

		[TestDate(2017, 07, 07, 18, 29, 17)]
		public void TestNotAllLinesAreGoingToBeSaved()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: TestObjectCreator.Creditor1);
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 10);

			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 22);
			Factory.SetContext(BusinessContext.SavingAsIncomplete);
			IncompleteInvoiceBOIsSavedByFactoryServiceProvider.RegisterInvoiceToSaveOnlyInvoiceHeader(invoice);
			Assert("Precondition: new line", !line2.IsInDatabase);
			Assert("Precondition: line will not be saved.", !line2.IsSavedByFactory);
			AssertEquals("Precondition: AH_OSTotalAmount", 35.2M, invoice.AH_OSTotalAmount);
			Assert("Precondition: invoice has changes.", invoice.HasChanges);
			Assert("Precondition: invoice will be saved.", invoice.IsSavedByFactory);

			var expectedTechDetails =
$@"Header: PK = {invoice.PK}, Ledger = AP, Transaction Type = INV, Invoice Date = 07-Jul-17 18:29:17, Post Date = 07-Jul-17 18:29:17, Invoice Amount = -32, GST Amount = -3.20, OS Total = -35.20, Exchange Rate = 1, Currency = AUD, Outstanding Amount = -35.20, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = {invoice.AH_TransactionNum}, Job PK = 00000000-0000-0000-0000-000000000000, Organization = ZCreditor1, Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = Factory Level : (SavingAsIncomplete, IncompleteInvoiceSaving).
Line Details:
Line: PK = {line1.PK}, Charge Code = , GL Account = {TestObjectCreator.GLHeader1.AG_AccountNum}, Type = CST, OS Amount = -11.0, Local Amount = -10, GST = -1.0, Tax Rate = ZZGST1, Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {invoice.PK}, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, <New or has changes, but will NOT be saved in db>, Is Final = No, Sub Accounts = , Has Changes = Yes.
Line: PK = {line2.PK}, Charge Code = , GL Account = {TestObjectCreator.GLHeader1.AG_AccountNum}, Type = CST, OS Amount = -24.20, Local Amount = -22, GST = -2.20, Tax Rate = ZZGST1, Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {invoice.PK}, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, <New or has changes, but will NOT be saved in db>, Is Final = No, Sub Accounts = , Has Changes = Yes.
";
			var expectedUserErrorMessage = @"Transaction Line Amount Total will not match the Transaction Header Invoice Amount after saving because not all lines will be saved. Number of all lines: 2, Number of lines that will not be saved despite having changed amounts: 0, Number of new lines that will not be saved: 2.";

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Transaction Line Amount Total correct, but not all lines will be saved.", true, CriticalValidationErrorType.SumOfTransactionLineAmountsWillNotMatchTransactionHeaderInvoiceAmountBecauseNotAllLinesWillBeSaved_2, expectedUserErrorMessage, expectedTechDetails);
			AssertOnSavingCheck(invoice, testCase);
		}

		public void TestLineOSAmountTotalNotMatchHeaderOSAmount_LineOSAmountIncorrect()
		{
			var header = Factory.NewWithValidTestData<ARInvoice>();
			header.AH_GC = GlbCompany.CurrentCompany.PK;
			header.AH_GB = GlbBranch.CurrentBranch.PK;
			header.AH_RX_NKTransactionCurrency = "AUD";
			header.AH_ExchangeRate = 1M;
			header.AH_InvoiceAmount = 200m;
			header.AH_OSTotal = 250m;
			header.AH_GSTAmount = 50m;
			header.AH_OutstandingAmount = header.AH_InvoiceAmount + header.AH_GSTAmount;

			var line = (TransactionLine)header.Lines.AddNew(header.DependentTransactionLineType);
			line.AL_RX_NKTransactionCurrency = "AUD";
			line.AL_LineAmount = 200m;
			line.AL_OSAmount = 100m;
			line.AL_GSTVAT = 50m;

			AssertEquals("Precondition", TransactionLineTypes.Revenue, line.AL_LineType);
			Assert("Precondition", !line.IsInDatabase);
			AssertEquals("Precondition", header.AH_RX_NKTransactionCurrency, line.AL_RX_NKTransactionCurrency);
			AssertEquals("Precondition", line.Company.GC_RX_NKLocalCurrency, line.AL_RX_NKTransactionCurrency);
			AssertNotEquals("Precondition", line.AL_OSAmount, line.AL_LineAmount + line.AL_GSTVAT);

			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("Transaction Line OS Amount Total does not match the Transaction Header OS Amount",
				true, CriticalValidationErrorType.SumOfTransactionLineOSAmountsDoesNotMatchTransactionHeaderOSTotalAmount_3,
				"Transaction Line OS Amount Total does not match the Transaction Header OS Amount.",
				"Calculate Tax at Header Level : True",
				"Header:",
				"Line:",
				"TransactionLineOSAmountIncorrectWhenPostingReceivableCharges: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.");
			AssertOnSavingCheck(header, testCase);

			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			testCase = new TestCaseDefinition_ForSeparateTestsMethods("Transaction Line OS Amount Total does not match the Transaction Header OS Amount",
				true, CriticalValidationErrorType.SumOfTransactionLineOSAmountsDoesNotMatchTransactionHeaderOSTotalAmount_3,
				"Transaction Line OS Amount Total does not match the Transaction Header OS Amount.",
				"Calculate Tax at Header Level : False",
				"Header:",
				"Line:",
				"TransactionLineOSAmountIncorrectWhenPostingReceivableCharges: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.");
			AssertOnSavingCheck(header, testCase);

			header.AH_GSTAmount = 60m;
			header.AH_OSTotal = 260m;
			line.AL_GSTVAT = 60m;
			header.AH_OutstandingAmount = header.AH_InvoiceAmount + header.AH_GSTAmount;

			AssertNotEquals("Precondition", line.AL_OSAmount, line.AL_LineAmount + line.AL_GSTVAT);

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("Transaction Line OS Amount Total does not match the Transaction Header OS Amount",
				true, CriticalValidationErrorType.SumOfTransactionLineOSAmountsDoesNotMatchTransactionHeaderOSTotalAmount_3,
				"Transaction Line OS Amount Total does not match the Transaction Header OS Amount.",
				"Header:",
				"Line:",
				"TransactionLineOSAmountIncorrectWhenPostingReceivableCharges:",
				"AL_GSTVAT = 60, AL_GSTVAT Old Value = 50, AL_LineAmount = 200, AL_OSAmount = 100",
				"at");
			AssertOnSavingCheck(header, testCase);

			header.AH_InvoiceAmount = 210m;
			header.AH_OSTotal = 270m;
			line.AL_LineAmount = 210m;
			header.AH_OutstandingAmount = header.AH_InvoiceAmount + header.AH_GSTAmount;

			AssertNotEquals("Precondition", line.AL_OSAmount, line.AL_LineAmount + line.AL_GSTVAT);

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("Transaction Line OS Amount Total does not match the Transaction Header OS Amount",
				true, CriticalValidationErrorType.SumOfTransactionLineOSAmountsDoesNotMatchTransactionHeaderOSTotalAmount_3,
				"Transaction Line OS Amount Total does not match the Transaction Header OS Amount.",
				"Header:",
				"Line:",
				"TransactionLineOSAmountIncorrectWhenPostingReceivableCharges:",
				"AL_LineAmount = 210, AL_LineAmount Old Value = 200, AL_OSAmount = 100, AL_GSTVAT = 60",
				"at");
			AssertOnSavingCheck(header, testCase);

			line.AL_OSAmount = 500m;

			AssertNotEquals("Precondition", line.AL_OSAmount, line.AL_LineAmount + line.AL_GSTVAT);

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("Transaction Line OS Amount Total does not match the Transaction Header OS Amount",
				true, CriticalValidationErrorType.SumOfTransactionLineOSAmountsDoesNotMatchTransactionHeaderOSTotalAmount_3,
				"Transaction Line OS Amount Total does not match the Transaction Header OS Amount.",
				"Header:",
				"Line:",
				"TransactionLineOSAmountIncorrectWhenPostingReceivableCharges:",
				"AL_OSAmount = 500, AL_OSAmount Old Value = 100, AL_LineAmount = 210, AL_GSTVAT = 60",
				"at");
			AssertOnSavingCheck(header, testCase);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}

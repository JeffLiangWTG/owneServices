using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingIServices;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Accounting.Business.CashBook.ExchangeDifference;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	public class TransactionHeaderCriticalValidationTest : AccTransactionHeaderCriticalValidationTest
	{
		public void TestDirectDebitBatchLocalAmountIsNotEqualToSumOfAllPaymentLocalAmounts_BatchIsInLocalCurrencyAndPaymentsAreInMixedCurrencies()
		{
			var apPayment1 = TestObjectCreator.CreateAPPayment(1m, 100m, ZDateTime.Now, ZDateTime.Now, TestObjectCreator.Creditor1.PK, TestObjectCreator.AUDBankAccount.PK);
			apPayment1.AH_ReceiptType = ReceiptTypes.DirectDebit;
			apPayment1.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
			apPayment1.AH_ExchangeRate = 0.5m;
			apPayment1.AH_OSTotal = 100m;

			var directPayment1 = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 200m, 20m, 300m, 30m, TestObjectCreator.AUDBankAccount.PK, 1m);
			directPayment1.AH_ReceiptType = ReceiptTypes.DirectDebit;
			Factory.Save();

			var batch1 = Factory.New<DirectDebitBatchHeader>();
			batch1.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			batch1.AH_TransactionNum = "00001000";
			AssertEquals("Batch is in local currency", CurrencyCodes.Australia, batch1.AH_RX_NKTransactionCurrency);
			AssertContainsExactElementsInAnyOrder(new[] { apPayment1.PK, directPayment1.PK }, batch1.Lines.Select(x => x.PK));

			AssertEquals(750m, batch1.AH_InvoiceAmount); //AP PAY AH_InvoiceAmount = 200 AUD + CB DPY AH_InvoiceAmount = 500 AUD + CB DPY AH_GSTAmount = 50 AUD

			AssertOnSavingCheck(batch1, new TestCaseDefinition_ForSeparateTestsMethods("Batch AH_InvoiceAmount is equal to sum of (AH_InvoiceAmount + AH_GSTAmount) all payments - Expect No Error."));

			batch1.AH_InvoiceAmount = 600m;

			using (new DisposableAction(() => batch1.Factory.SetContext(BusinessContext.ExcludeFromDirectDebitBatchCriticalValidation), () => batch1.Factory.RemoveContext(BusinessContext.ExcludeFromDirectDebitBatchCriticalValidation)))
			{
				AssertOnSavingCheck(batch1, new TestCaseDefinition_ForSeparateTestsMethods("Batch AH_InvoiceAmount is NOT equal to sum of (AH_InvoiceAmount + AH_GSTAmount) all payments - Should not fail because Batch has ExcludeFromDirectDebitBatchCriticalValidation context"));
			}

			AssertOnSavingCheck(batch1, new TestCaseDefinition_ForSeparateTestsMethods("Batch AH_InvoiceAmount is NOT equal to sum of (AH_InvoiceAmount + AH_GSTAmount) all payments",
					true,
				CriticalValidationErrorType.DirectDebitBatchLocalAmountIsNotEqualToSumOfAllPaymentLocalAmounts_2,
					"Direct Debit Batch Header Local Amount does not match sum of Payment Local Amount and GST Amount. Payment Local Amount Total is 700, GST Amount is 50, but Header Local Amount is 600."));
		}

		public void TestDirectDebitBatchLocalAmountIsNotEqualToSumOfAllPaymentLocalAmounts_BatchIsInLocalCurrencyAndPaymentsAreInLocalCurrency()
		{
			var apPayment1 = TestObjectCreator.CreateAPPayment(1m, 100m, ZDateTime.Now, ZDateTime.Now, TestObjectCreator.Creditor1.PK, TestObjectCreator.AUDBankAccount.PK);
			apPayment1.AH_ReceiptType = ReceiptTypes.DirectDebit;

			var directPayment1 = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 200m, 20m, 300m, 30m, TestObjectCreator.AUDBankAccount.PK, 1m);
			directPayment1.AH_ReceiptType = ReceiptTypes.DirectDebit;
			Factory.Save();

			var batch1 = Factory.New<DirectDebitBatchHeader>();
			batch1.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			batch1.AH_TransactionNum = "00001000";
			AssertEquals("Batch is in local currency", CurrencyCodes.Australia, batch1.AH_RX_NKTransactionCurrency);
			AssertContainsExactElementsInAnyOrder(new[] { apPayment1.PK, directPayment1.PK }, batch1.Lines.Select(x => x.PK));

			AssertEquals(650m, batch1.AH_InvoiceAmount); //AP PAY AH_InvoiceAmount = 100 AUD + CB DPY AH_InvoiceAmount = 500 AUD + CB DPY AH_GSTAmount = 50 AUD

			AssertOnSavingCheck(batch1, new TestCaseDefinition_ForSeparateTestsMethods("Batch AH_InvoiceAmount is equal to sum of (AH_InvoiceAmount + AH_GSTAmount) all payments - Expect No Error."));

			batch1.AH_InvoiceAmount = 600m;

			using (new DisposableAction(() => batch1.Factory.SetContext(BusinessContext.ExcludeFromDirectDebitBatchCriticalValidation), () => batch1.Factory.RemoveContext(BusinessContext.ExcludeFromDirectDebitBatchCriticalValidation)))
			{
				AssertOnSavingCheck(batch1, new TestCaseDefinition_ForSeparateTestsMethods("Batch AH_InvoiceAmount is NOT equal to sum of (AH_InvoiceAmount + AH_GSTAmount) all payments - Should not fail because Batch has ExcludeFromDirectDebitBatchCriticalValidation context"));
			}

			AssertOnSavingCheck(batch1, new TestCaseDefinition_ForSeparateTestsMethods("Batch AH_InvoiceAmount is NOT equal to sum of (AH_InvoiceAmount + AH_GSTAmount) all payments.",
				true,
				CriticalValidationErrorType.DirectDebitBatchLocalAmountIsNotEqualToSumOfAllPaymentLocalAmounts_2,
				"Direct Debit Batch Header Local Amount does not match sum of Payment Local Amount and GST Amount. Payment Local Amount Total is 600, GST Amount is 50, but Header Local Amount is 600."));
		}

		public void TestDirectDebitBatchLocalAmountIsNotEqualToSumOfAllPaymentLocalAmounts_BatchIsInForeignCurrency()
		{
			var apPayment1 = TestObjectCreator.CreateAPPayment(1m, 100m, ZDateTime.Now, ZDateTime.Now, TestObjectCreator.Creditor1.PK, TestObjectCreator.USDBankAccount.PK);
			apPayment1.AH_ReceiptType = ReceiptTypes.DirectDebit;
			apPayment1.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
			apPayment1.AH_ExchangeRate = 0.5m;
			apPayment1.AH_OSTotal = 100m;

			var directPayment1 = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 200m, 20m, 300m, 30m, TestObjectCreator.USDBankAccount.PK, 0.5m);
			directPayment1.AH_ReceiptType = ReceiptTypes.DirectDebit;
			Factory.Save();

			var batch1 = Factory.New<DirectDebitBatchHeader>();
			batch1.AH_AB = TestObjectCreator.USDBankAccount.PK;
			batch1.AH_TransactionNum = "00001000";
			AssertEquals("Batch is in foreign currency", CurrencyCodes.UnitedStates, batch1.AH_RX_NKTransactionCurrency);
			AssertContainsExactElementsInAnyOrder(new[] { apPayment1.PK, directPayment1.PK }, batch1.Lines.Select(x => x.PK));

			AssertEquals(1300m, batch1.AH_InvoiceAmount); //AP PAY AH_InvoiceAmount = 200 AUD + CB DPY AH_InvoiceAmount = 1000 AUD + CB DPY AH_GSTAmount = 100 AUD

			AssertOnSavingCheck(batch1, new TestCaseDefinition_ForSeparateTestsMethods("Batch AH_InvoiceAmount is equal to sum of (AH_InvoiceAmount + AH_GSTAmount) all payments - Expect No Error."));

			batch1.AH_InvoiceAmount = 600m;

			using (new DisposableAction(() => batch1.Factory.SetContext(BusinessContext.ExcludeFromDirectDebitBatchCriticalValidation), () => batch1.Factory.RemoveContext(BusinessContext.ExcludeFromDirectDebitBatchCriticalValidation)))
			{
				AssertOnSavingCheck(batch1, new TestCaseDefinition_ForSeparateTestsMethods("Batch AH_InvoiceAmount is NOT equal to sum of (AH_InvoiceAmount + AH_GSTAmount) all payments - Should not fail because Batch has ExcludeFromDirectDebitBatchCriticalValidation context"));
			}

			AssertOnSavingCheck(batch1, new TestCaseDefinition_ForSeparateTestsMethods("Batch AH_InvoiceAmount is NOT equal to sum of (AH_InvoiceAmount + AH_GSTAmount) all payments.",
				true,
				CriticalValidationErrorType.DirectDebitBatchLocalAmountIsNotEqualToSumOfAllPaymentLocalAmounts_2,
				"Direct Debit Batch Header Local Amount does not match sum of Payment Local Amount and GST Amount. Payment Local Amount Total is 1200, GST Amount is 100, but Header Local Amount is 600."));
		}

		public void TestDirectDebitBatchOSAmountIsNotEqualToBatchLocalAmount_BatchIsInLocalCurrencyAndPaymentsAreInMixedCurrencies()
		{
			var apPayment1 = TestObjectCreator.CreateAPPayment(1m, 100m, ZDateTime.Now, ZDateTime.Now, TestObjectCreator.Creditor1.PK, TestObjectCreator.AUDBankAccount.PK);
			apPayment1.AH_ReceiptType = ReceiptTypes.DirectDebit;
			apPayment1.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
			apPayment1.AH_ExchangeRate = 0.5m;
			apPayment1.AH_OSTotal = 100m;

			var directPayment1 = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 200m, 20m, 300m, 30m, TestObjectCreator.AUDBankAccount.PK, 1m);
			directPayment1.AH_ReceiptType = ReceiptTypes.DirectDebit;
			Factory.Save();

			var batch1 = Factory.New<DirectDebitBatchHeader>();
			batch1.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			batch1.AH_TransactionNum = "00001000";
			AssertEquals("Batch is in local currency", CurrencyCodes.Australia, batch1.AH_RX_NKTransactionCurrency);
			AssertContainsExactElementsInAnyOrder(new[] { apPayment1.PK, directPayment1.PK }, batch1.Lines.Select(x => x.PK));

			AssertEquals(750m, batch1.AH_InvoiceAmount); //AP PAY AH_InvoiceAmount = 200 AUD + CB DPY AH_InvoiceAmount = 500 AUD + CB DPY AH_GSTAmount = 50 AUD
			AssertEquals(650m, batch1.AH_OSTotal); //AP PAY AH_InvoiceAmount = 100 USD + CB DPY AH_InvoiceAmount = 500 AUD + CB DPY AH_GSTAmount = 50 AUD

			//Batch OS Amount should be equal to Batch Local Amount when Batch is in Local Currency, but in this scenario it is not.
			//However critical validation error for this scenario should not be added until Batch OS Amount calculation is fixed
			AssertOnSavingCheck(batch1, new TestCaseDefinition_ForSeparateTestsMethods("Batch OS Amount is NOT equal to Batch Local Amount when Batch is in Local Currency - Expect No Error"));
		}

		public void TestDirectDebitBatchOSAmountIsNotEqualToBatchLocalAmount_BatchIsInLocalCurrencyAndPaymentsAreInLocalCurrency()
		{
			var apPayment1 = TestObjectCreator.CreateAPPayment(1m, 100m, ZDateTime.Now, ZDateTime.Now, TestObjectCreator.Creditor1.PK, TestObjectCreator.AUDBankAccount.PK);
			apPayment1.AH_ReceiptType = ReceiptTypes.DirectDebit;

			var directPayment1 = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 200m, 20m, 300m, 30m, TestObjectCreator.AUDBankAccount.PK, 1m);
			directPayment1.AH_ReceiptType = ReceiptTypes.DirectDebit;
			Factory.Save();

			var batch1 = Factory.New<DirectDebitBatchHeader>();
			batch1.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			batch1.AH_TransactionNum = "00001000";
			AssertEquals("Batch is in local currency", CurrencyCodes.Australia, batch1.AH_RX_NKTransactionCurrency);
			AssertContainsExactElementsInAnyOrder(new[] { apPayment1.PK, directPayment1.PK }, batch1.Lines.Select(x => x.PK));

			AssertEquals(650m, batch1.AH_InvoiceAmount); //AP PAY AH_InvoiceAmount = 100 AUD + CB DPY AH_InvoiceAmount = 500 AUD + CB DPY AH_GSTAmount = 50 AUD
			AssertEquals(650m, batch1.AH_OSTotal); //AP PAY AH_OSTotal = 100 AUD + CB DPY AH_OSTotal = 500 AUD + CB DPY AH_GSTAmount = 50 AUD

			AssertOnSavingCheck(batch1, new TestCaseDefinition_ForSeparateTestsMethods("Batch OS Amount is equal to Batch Local Amount when Batch is in Local Currency - Expect No Error"));

			batch1.AH_OSTotal = 600m;

			using (new DisposableAction(() => batch1.Factory.SetContext(BusinessContext.ExcludeFromDirectDebitBatchCriticalValidation), () => batch1.Factory.RemoveContext(BusinessContext.ExcludeFromDirectDebitBatchCriticalValidation)))
			{
				AssertOnSavingCheck(batch1, new TestCaseDefinition_ForSeparateTestsMethods("Batch OS Amount is NOT equal to Batch Local Amount when Batch is in Local Currency - Should not fail because Batch has ExcludeFromDirectDebitBatchCriticalValidation context"));
			}

			AssertOnSavingCheck(batch1, new TestCaseDefinition_ForSeparateTestsMethods("Batch OS Amount is NOT equal to Batch Local Amount when Batch is in Local Currency",
				true,
				CriticalValidationErrorType.DirectDebitBatchOSAmountIsNotEqualToLocalAmountWhenBatchIsInLocalCurrency,
				"Direct Debit Batch Header OS Amount does not match Local Amount. Header OS Amount is 600, but Header Local Amount is 650."));
		}

		public void TestDirectDebitBatchOSAmountIsNotEqualToSumOfAllPaymentOSAmounts_BatchIsInForeignCurrency()
		{
			var apPayment1 = TestObjectCreator.CreateAPPayment(1m, 100m, ZDateTime.Now, ZDateTime.Now, TestObjectCreator.Creditor1.PK, TestObjectCreator.USDBankAccount.PK);
			apPayment1.AH_ReceiptType = ReceiptTypes.DirectDebit;
			apPayment1.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
			apPayment1.AH_ExchangeRate = 0.5m;
			apPayment1.AH_OSTotal = 100m;

			var directPayment1 = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 200m, 20m, 300m, 30m, TestObjectCreator.USDBankAccount.PK, 0.5m);
			directPayment1.AH_ReceiptType = ReceiptTypes.DirectDebit;
			Factory.Save();

			var batch1 = Factory.New<DirectDebitBatchHeader>();
			batch1.AH_AB = TestObjectCreator.USDBankAccount.PK;
			batch1.AH_TransactionNum = "00001000";
			AssertEquals("Batch is in foreign currency", CurrencyCodes.UnitedStates, batch1.AH_RX_NKTransactionCurrency);
			AssertContainsExactElementsInAnyOrder(new[] { apPayment1.PK, directPayment1.PK }, batch1.Lines.Select(x => x.PK));

			AssertEquals(650m, batch1.AH_OSTotal); //AP PAY AH_OSTotal = 100 USD + CB DPY AH_OSTotal = 550 USD

			AssertOnSavingCheck(batch1, new TestCaseDefinition_ForSeparateTestsMethods("Batch AH_OSTotal is equal to sum of AH_OSTotal of all payments - Expect No Error."));

			batch1.AH_OSTotal = 700m;

			using (new DisposableAction(() => batch1.Factory.SetContext(BusinessContext.ExcludeFromDirectDebitBatchCriticalValidation), () => batch1.Factory.RemoveContext(BusinessContext.ExcludeFromDirectDebitBatchCriticalValidation)))
			{
				AssertOnSavingCheck(batch1, new TestCaseDefinition_ForSeparateTestsMethods("Batch AH_OSTotal is NOT equal to sum of AH_OSTotal of all payments - Should not fail because Batch has ExcludeFromDirectDebitBatchCriticalValidation context"));
			}

			AssertOnSavingCheck(batch1, new TestCaseDefinition_ForSeparateTestsMethods("Batch AH_OSTotal is NOT equal to sum of AH_OSTotal of all payments.",
				true,
				CriticalValidationErrorType.DirectDebitBatchOSAmountIsNotEqualToSumOfAllPaymentOSAmountsWhenBatchIsInForeignCurrency,
				"Direct Debit Batch Header OS Amount does not match sum of Payment OS Amount. Payment OS Amount Total is 650, but Header OS Amount is 700."));
		}

		public void TestFullyPaidTransactionHasTaxRecordsWithEmptyRealisationDate_Case1()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1");
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_OutstandingAmountInfo.HasChanges);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS", includeInInvoiceTotal: false);
			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, TaxConfigurationLedgers.AccountsPayable.Code);
			var taxID = TestObjectCreator.CreateTaxRate("TID", "TID Desc", 6);
			AssertOnSavingCheck(invoice, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			var taxTransaction = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxSystem = taxSystem, TaxConfiguration = taxConfig, TransactionHeader = invoice, PostDate = new ZDate(2020, 3, 5), TaxId = taxID, TaxRate = (9, 8), OsTaxBaseAmount = 40, LocalTaxBaseAmount = 20, OsTaxAmount = 4, LocalTaxAmount = 2, DoesNotCreateGLMovemetsOnSaving = true });
			taxTransaction.ATT_Basis = TaxBasisList.Posting.Code;
			AssertOnSavingCheck(invoice, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			taxTransaction.ATT_Basis = TaxBasisList.Matching.Code;
			taxTransaction.ATT_AG_TaxControlAccount = TestObjectCreator.GLHeader1.PK;
			taxTransaction.ATT_AG_TaxPendingControlAccount = TestObjectCreator.GLHeader1.PK;
			AssertOnSavingCheck(invoice, new TestCaseDefinition_ForSeparateTestsMethods("MatchingBasisTaxTransactionsWithEmptyRealisationDate", true, CriticalValidationErrorType.MatchingBasisTaxTransactionsWithEmptyRealisationDate, CriticalValidationMessageTemplate.MatchingBasisTaxTransactionsWithEmptyRealisationDateErrorMessage));

			try
			{
				SuspendCriticalValidationAttribute.IsActive = true;
				Factory.Save();
			}
			finally
			{
				SuspendCriticalValidationAttribute.IsActive = false;
			}

			Assert(invoice.IsInDatabase);
			AssertOnSavingCheck(invoice, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));
		}

		public void TestFullyPaidTransactionHasTaxRecordsWithEmptyRealisationDate_Case2()
		{
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS", includeInInvoiceTotal: false);
			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, TaxConfigurationLedgers.AccountsPayable.Code);
			var taxID = TestObjectCreator.CreateTaxRate("TID", "TID Desc", 6);
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "INV2");
			invoice.AH_OutstandingAmount = 5m;

			try
			{
				SuspendCriticalValidationAttribute.IsActive = true;
				Factory.Save();
			}
			finally
			{
				SuspendCriticalValidationAttribute.IsActive = false;
			}

			Assert(invoice.IsInDatabase);
			invoice.AH_OutstandingAmount = 0m;
			Assert(invoice.AH_OutstandingAmountInfo.HasChanges);

			AssertOnSavingCheck(invoice, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			var taxTransaction = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxSystem = taxSystem, TaxConfiguration = taxConfig, TransactionHeader = invoice, PostDate = new ZDate(2020, 3, 5), TaxId = taxID, TaxRate = (9, 8), OsTaxBaseAmount = 40, LocalTaxBaseAmount = 20, OsTaxAmount = 4, LocalTaxAmount = 2 });
			taxTransaction.ATT_Basis = TaxBasisList.Posting.Code;
			AssertOnSavingCheck(invoice, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			taxTransaction.ATT_Basis = TaxBasisList.Matching.Code;
			AssertOnSavingCheck(invoice, new TestCaseDefinition_ForSeparateTestsMethods("MatchingBasisTaxTransactionsWithEmptyRealisationDate", true, CriticalValidationErrorType.MatchingBasisTaxTransactionsWithEmptyRealisationDate, CriticalValidationMessageTemplate.MatchingBasisTaxTransactionsWithEmptyRealisationDateErrorMessage));

			invoice.AH_IsCancelled = true;
			var matchLink = Factory.New<AccTransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			AssertOnSavingCheck(invoice, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));
		}

		public void TestFullyPaidTransactionHasTaxRecordsWithEmptyRealisationDate_Case3()
		{
			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("TS", includeInInvoiceTotal: false);
			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, TaxConfigurationLedgers.AccountsPayable.Code);
			var taxID = TestObjectCreator.CreateTaxRate("TID", "TID Desc", 6);
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV3");
			invoice.AH_OutstandingAmount = 5m;
			var taxTransaction = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxSystem = taxSystem, TaxConfiguration = taxConfig, TransactionHeader = invoice, PostDate = new ZDate(2020, 3, 5), TaxId = taxID, TaxRate = (9, 8), OsTaxBaseAmount = 40, LocalTaxBaseAmount = 20, OsTaxAmount = 4, LocalTaxAmount = 2, DoesNotCreateGLMovemetsOnSaving = true });
			taxTransaction.ATT_Basis = TaxBasisList.Matching.Code;

			try
			{
				SuspendCriticalValidationAttribute.IsActive = true;
				Factory.Save();
			}
			finally
			{
				SuspendCriticalValidationAttribute.IsActive = false;
			}

			Assert(invoice.IsInDatabase);
			invoice.AH_OutstandingAmount = 0m;
			Assert(invoice.AH_OutstandingAmountInfo.HasChanges);
			Assert(taxTransaction.IsInDatabase);
			AssertOnSavingCheck(invoice, new TestCaseDefinition_ForSeparateTestsMethods("MatchingBasisTaxTransactionsWithEmptyRealisationDate", true, CriticalValidationErrorType.MatchingBasisTaxTransactionsWithEmptyRealisationDate, CriticalValidationMessageTemplate.MatchingBasisTaxTransactionsWithEmptyRealisationDateErrorMessage));

			taxTransaction.ATT_RealisationDate = ZDate.Today;
			AssertOnSavingCheck(invoice, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));

			var transaction = GetNewParentForLocalInvoiceAmountEqualToForeignAmountCheck(Factory, TaxConfigurationLedgers.AccountsPayable.Code, TransactionTypes.Invoice, TestObjectCreator.USD.Code);
			Assert(!transaction.IsInDatabase);
			AssertNotEquals(0m, transaction.AH_OutstandingAmount);

			taxTransaction = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { TaxSystem = taxSystem, TaxConfiguration = taxConfig, TransactionHeader = transaction, PostDate = new ZDate(2020, 3, 5), TaxId = taxID, TaxRate = (9, 8), OsTaxBaseAmount = 40, LocalTaxBaseAmount = 20, OsTaxAmount = 4, LocalTaxAmount = 2 });
			taxTransaction.ATT_Basis = TaxBasisList.Matching.Code;
			AssertOnSavingCheck(transaction, new TestCaseDefinition_ForSeparateTestsMethods("Expect no errors."));
		}

		public void TestNumberFountainBaseDataWasChanged()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1");
			TestObjectCreator.CreateInvoiceLine(invoice, 100);
			invoice.AH_TransactionCategory = TransactionCategory.Codes.SelfBilling;
			Assert("Precondition: IsSelfBillingInvoice", invoice.IsSelfBillingInvoice);

			NumberFountainTransactionDataProvider.GetIsSelfBilling(invoice);
			AssertOnSavingCheck(invoice, new TestCaseDefinition_ForSeparateTestsMethods("Invoice was not changed after number generation."));

			invoice.AH_TransactionCategory = TransactionCategory.Codes.Standard;

			AssertOnSavingCheck(invoice, new TestCaseDefinition_ForSeparateTestsMethods("Invoice was changed after number generation.",
				true,
				CriticalValidationErrorType.NumberFountainBaseDataWasChanged_2,
				CriticalValidationMessageTemplate.NumberFountainBaseDataWasChanged,
				"Header: PK =",
				"SelfBilling: original value: True, current value: False."
				));
		}

		public void TestOSTotalAmountEqualToRoundedAmount()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);

			new InvoiceRoundingLineCreator().SetRoundingAmountCachedValue_ForTestOnly(invoice, 101M);

			AssertOnSavingCheck(invoice, new TestCaseDefinition_ForSeparateTestsMethods("OS Total Amount Is Changed After Rounding.",
				true,
				CriticalValidationErrorType.OSTotalAmountMustNotChangeAfterRoundingLineCreation,
				CriticalValidationMessageTemplate.OSTotalAmountMustNotChangeAfterRoundingLineCreationErrorMessage,
				"Header: PK ="
				));

			new InvoiceRoundingLineCreator().SetRoundingAmountCachedValue_ForTestOnly(invoice, 100M);

			AssertOnSavingCheck(invoice, new TestCaseDefinition_ForSeparateTestsMethods("OS Total Amount Is Not Changed After Rounding line creation."));
		}

		public void TestDepositBatchTransactionLineDoesNotCheckNumberFountainBaseDataIsNotChanged()
		{
			var directReceipt = Factory.NewWithValidTestData<BankReconDirectReceipt>();
			directReceipt.Lines.AddNew();
			AssertEquals("Precondition", 1, directReceipt.Lines.Count);
			directReceipt.Lines[0].FillWithValidTestData();
			directReceipt.Lines[0].AL_AT = TestObjectCreator.GST1.PK;

			var depositBatchTransactionLine = Factory.Load<DepositBatchTransactionLine>(directReceipt.PK);

			Assert("Precondition", directReceipt.IsTaxReportable);
			Assert("Precondition", !depositBatchTransactionLine.IsTaxReportable);
			NumberFountainTransactionDataProvider.GetIsTaxReported(directReceipt);
			Assert("Precondition", !depositBatchTransactionLine.IsInDatabase);
			Assert("Precondition", !depositBatchTransactionLine.HasNumberFountain);
			AssertOnSavingCheck(depositBatchTransactionLine, new TestCaseDefinition_ForSeparateTestsMethods("DepositBatchTransactionLine does not check NumberFountainBaseDataIsNotChanged."));
		}

		public void TestGetMatchLinkInfoWhenOutstandingAmountAndFullyPaidDateAreNotValid()
		{
			var invoice = GetNewParentForOutstandingAmountAndFullyPaidDateCheck(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			var devExpectedErrorMsg1 = string.Format(@"Developer Details (Critical Validation Failure): 

Invalid Fully Paid Date with respect to the outstanding amount.

Header: PK = {0}, Ledger = AR, Transaction Type = INV, Invoice Date =", invoice.PK);

			var devExpectedErrorMsg2 = "Post Date = , Invoice Amount = 5, GST Amount = 0, OS Total = 0, Exchange Rate = 1, Currency = , Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number =";
			var devExpectedErrorMsg3 = string.Format(@"Related Match Links:
Match Link: Group Number = M000971, Amount = 5, OS Amount = 0, Match Date = 20-Oct-16 00:00:00, Transaction PK = {0}, Is In DB = No, Has Changes = Yes.", invoice.PK);

			Func<TransactionHeader, TestCaseDefinition_ForSeparateTestsMethods> testCaseGetter = (transaction) =>
			{
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
@"An error has occurred. Your unsaved work must be re-entered.

			Please close the form in which you were working and re-enter the data.

			Error Message: Invalid Fully Paid Date with respect to the outstanding amount.",
					true,
					CriticalValidationErrorType.InvalidFullyPaidDateWithRespectToTheOutstandingAmount_2,
					CriticalValidationMessageTemplate.InvalidFullyPaidDateWithRespectToTheOutstandingAmountErrorMessage, new string[] { devExpectedErrorMsg1, devExpectedErrorMsg2, devExpectedErrorMsg3 });

				return testCase;
			};

			AssertOnSavingCheck(invoice, testCaseGetter(invoice));
		}

		protected override List<TestCaseDefinitionWithDelegate_Obsolete> GetTestCases()
		{
			List<TestCaseDefinitionWithDelegate_Obsolete> result = base.GetTestCases();

			result.AddRange(GetIsCancelledIsNotSetWithoutMatchLinksCases());
			result.AddRange(GetOutstandingAmountAndFullyPaidDateAreValidCases());
			result.AddRange(GetLocalInvoiceAmountEqualToForeignAmountCases());
			result.AddRange(GetClearingJournalAH_AGEmptyCases());
			result.AddRange(GetGLAccountNotSetCases());
			result.AddRange(GetMaximumJobInvoiceNumberErrorCases());
			return result;
		}

		List<TestCaseDefinitionWithDelegate_Obsolete> GetMaximumJobInvoiceNumberErrorCases()
		{
			List<TestCaseDefinitionWithDelegate_Obsolete> result = new List<TestCaseDefinitionWithDelegate_Obsolete>();
			var currentTransactionType = TransactionTypes.Invoice;
			foreach (string ledgerType in ledgerTypes)
			{
				var currentLedgerType = ledgerType;
				result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("MaximumJobInvoiceNumberError, Ledger = {0}, Type = {1}", ledgerType, currentTransactionType),
					factory => GetNewParentForJobInvoiceNumberExcceedTheMaximumNumberCheck(factory, currentLedgerType, currentTransactionType),
					true, CriticalValidationErrorType.JobInvoiceNumberExceedTheMaximumNumber, "The maximum number of invoices for a job is 703. You cannot post any more invoices for this job."));
			}

			return result;
		}

		List<TestCaseDefinitionWithDelegate_Obsolete> GetIsCancelledIsNotSetWithoutMatchLinksCases()
		{
			List<TestCaseDefinitionWithDelegate_Obsolete> result = new List<TestCaseDefinitionWithDelegate_Obsolete>();

			foreach (string ledgerType in ledgerTypes)
			{
				foreach (string transactionType in transactionTypesForIsCancelledCheck)
				{
					string ledgerType1 = ledgerType;
					string transactionType1 = transactionType;
					result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("IsCancelledIsNotSetWithoutMatchLinks, Ledger = {0}, Type = {1}", ledgerType, transactionType),
						factory => GetNewParentForIsCancelledCheck(factory, ledgerType1, transactionType1),
						true, CriticalValidationErrorType.MissingRevesingTransactionForCanceledTransaction_2, IsCancelledErrorMessage));
				}
			}

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("IsCancelledIsNotSetWithoutMatchLinks, transaction in db, not cancelled", factory => GetInvalidTransactionSavedInDb(factory)));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("IsCancelledIsNotSetWithoutMatchLinks, transaction in db, cancelled",
				delegate(BusinessObjectFactory factory)
				{
					AccTransactionHeader invalidInDb = GetInvalidTransactionSavedInDb(factory, false);
					invalidInDb.AH_IsCancelled = true;
					return invalidInDb;
				}, true, CriticalValidationErrorType.MissingRevesingTransactionForCanceledTransaction_2, IsCancelledErrorMessage));

			return result;
		}

		List<TestCaseDefinitionWithDelegate_Obsolete> GetOutstandingAmountAndFullyPaidDateAreValidCases()
		{
			List<TestCaseDefinitionWithDelegate_Obsolete> result = new List<TestCaseDefinitionWithDelegate_Obsolete>();

			foreach (string ledgerType in ledgerTypes)
			{
				foreach (string transactionType in transactionTypes)
				{
					string ledgerType1 = ledgerType;
					string transactionType1 = transactionType;
					result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("OutstandingAmountAndFullyPaidDateAreValid, Ledger = {0}, Type = {1}", ledgerType, transactionType),
						factory => GetNewParentForOutstandingAmountAndFullyPaidDateCheck(factory, ledgerType1, transactionType1),
						true, CriticalValidationErrorType.InvalidFullyPaidDateWithRespectToTheOutstandingAmount_2, OutstandingAmountAndFullyPaidDateErrorMessage));
				}
			}

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("OutstandingAmountAndFullyPaidDateAreValid, transaction in db, valid", factory => GetInvalidTransactionSavedInDb(factory)));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("OutstandingAmountAndFullyPaidDateAreValid, transaction in db, invalid(true, true)",
				delegate(BusinessObjectFactory factory)
				{
					AccTransactionHeader invalidInDb = GetInvalidTransactionSavedInDb(factory, true, true);
					invalidInDb.AH_FullyPaidDate = ZDateTime.Empty;
					return invalidInDb;
				}, true, CriticalValidationErrorType.InvalidFullyPaidDateWithRespectToTheOutstandingAmount_2, OutstandingAmountAndFullyPaidDateErrorMessage));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("OutstandingAmountAndFullyPaidDateAreValid, transaction in db, valid(false, true)", factory => GetInvalidTransactionSavedInDb(factory, false, true)));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("OutstandingAmountAndFullyPaidDateAreValid, transaction in db, invalid(false, true)",
				delegate(BusinessObjectFactory factory)
				{
					AccTransactionHeader invalidInDb = GetInvalidTransactionSavedInDb(factory, false, true);
					invalidInDb.AH_OutstandingAmount = invalidInDb.AH_InvoiceAmount + invalidInDb.AH_GSTAmount + 1;
					invalidInDb.AH_OutstandingAmount = invalidInDb.AH_OutstandingAmount - 1;
					Assert("Just reset outstanding amount so there should be changes", invalidInDb.AH_OutstandingAmountInfo.HasChanges);
					return invalidInDb;
				}, true, CriticalValidationErrorType.InvalidFullyPaidDateWithRespectToTheOutstandingAmount_2, OutstandingAmountAndFullyPaidDateErrorMessage));

			return result;
		}

		public void TestLocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1()
		{
			foreach (string ledgerType in ledgerTypes)
			{
				foreach (string transactionType in transactionTypesForLocalInvoiceAmountEqualToForeignAmount)
				{
					if (ledgerType == LedgerTypes.AccountsPayable && transactionType == TransactionTypes.InvoiceBatch)
					{
						continue; //Ledger: AP and Transaction: INB is not a valid combination
					}

					string ledgerType1 = ledgerType;
					string transactionType1 = transactionType;

					var transaction = GetNewParentForLocalInvoiceAmountEqualToForeignAmountCheck(Factory, ledgerType1, transactionType1, "AUD");
					var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("IsLocalInvoiceAmountEqualToForeignAmount, Ledger = {0}, Type = {1}, Currency = AUD", ledgerType, transactionType),
						true, CriticalValidationErrorType.LocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1_4, LocalInvoiceAmountNotEqualToForeignAmountErrorMessage("AUD", 1),
						LocalInvoiceAmountNotEqualToForeignAmountErrorMessageAdditionalInfo(ledgerType1, transactionType1));
					AssertOnSavingCheck(transaction, testCase);

					transaction = GetNewParentForLocalInvoiceAmountEqualToForeignAmountCheck(Factory, ledgerType1, transactionType1, "USD");
					testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("IsLocalInvoiceAmountEqualToForeignAmount For OSTransaction With AH_ExchangeRate 1, Ledger = {0}, Type = {1}, Currency = USD", ledgerType, transactionType));
					AssertOnSavingCheck(transaction, testCase);
				}
			}
		}

		public void TestTransactionEvaluateTransactionHeaderWithNotEqualOneExchangeRateAndLocalCurrencyWithStackTrace()
		{
			var accTransactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			accTransactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			accTransactionHeader.AH_TransactionType = TransactionTypes.Overpayment;
			accTransactionHeader.AH_OSTotal = 2550000M;
			accTransactionHeader.AH_InvoiceAmount = 20M;

			var arCRD = Factory.NewWithValidTestData<AccTransactionHeader>();
			arCRD.AH_Ledger = LedgerTypes.AccountsReceivable;
			arCRD.AH_TransactionType = TransactionTypes.Overpayment;
			arCRD.AH_InvoiceAmount = -20M;

			var matchLinkForARCRD = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLinkForARCRD.AP_MatchGroupNum = "M001";
			matchLinkForARCRD.AP_AH = arCRD.PK;
			matchLinkForARCRD.AP_Amount = -20M;

			var matchLinkForOverPayment = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			matchLinkForOverPayment.AP_MatchGroupNum = "M001";
			matchLinkForOverPayment.AP_AH = accTransactionHeader.PK;
			matchLinkForOverPayment.AP_Amount = 20M;

			Factory.SetContext(BusinessContext.CreateTransactionsBeforePostingForFactoryLevel);

			accTransactionHeader.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			accTransactionHeader.AH_ExchangeRate = 2M;

			var overPayment = Factory.Load<TransactionHeader>(accTransactionHeader.PK);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"Not one ExchangeRate and local currency.",
				true,
				CriticalValidationErrorType.LocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1_4,
				CriticalValidationMessageTemplate.LocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1ErrorMessage(
					accTransactionHeader.AH_RX_NKTransactionCurrency, accTransactionHeader.AH_ExchangeRate),
				"EvaluateTransactionHeaderWithNotEqualOneExchangeRateAndLocalCurrency: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1."
			);

			AssertOnSavingCheck(overPayment, testCase);
			overPayment.AH_ExchangeRate = 3M;
			accTransactionHeader.AH_InvoiceAmount = 20M;
			accTransactionHeader.AH_OutstandingAmount = 0M;

			var expectedCollectedInfo = CriticalValidationInfoCollectorService.GetOrCreateService(overPayment.Factory).GetInfo(overPayment.PK, CriticalValidationInfoCollectorServiceKeyType.EvaluateTransactionHeaderWithNotEqualOneExchangeRateAndLocalCurrency);

			var expectedHeaderInfo = @$"Header: PK = {accTransactionHeader.PK}, Ledger = AR, Transaction Type = OVP, Invoice Date = 04-Mar-04 00:00:00, Post Date = , Invoice Amount = 20, GST Amount = 0, OS Total = 2550000, Exchange Rate = 3, Currency = AUD, Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 0CBSU6DGVDNI2TP1RBLLN7XKNXVXACC4CNGTGR, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = Factory Level : (CreateTransactionsBeforePostingForFactoryLevel).";

			var expectedTransactionHeaderInfo = @"
Transaction Header:
- Local Amount: 20
- Foreign Currency Amount: 2550000
- Currency: AUD
- Exchange Rate: 3
- Company: EDI
- Company Local Currency: AUD";

			var testCaseWithExtraData = new TestCaseDefinition_ForSeparateTestsMethods(
	"Not one ExchangeRate and local currency.",
	true,
	CriticalValidationErrorType.LocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1_4,
	CriticalValidationMessageTemplate.LocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1ErrorMessage(overPayment.AH_RX_NKTransactionCurrency, overPayment.AH_ExchangeRate),
	expectedHeaderInfo, expectedCollectedInfo, expectedTransactionHeaderInfo);

			AssertOnSavingCheck(overPayment, testCaseWithExtraData);
		}

		public void TestLocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1_WithOtherTaxes()
		{
			foreach (string ledgerType in ledgerTypes)
			{
				foreach (string transactionType in transactionTypesForLocalInvoiceAmountEqualToForeignAmount)
				{
					if (ledgerType == LedgerTypes.AccountsPayable && transactionType == TransactionTypes.InvoiceBatch)
					{
						continue; //Ledger: AP and Transaction: INB is not a valid combination
					}

					string ledgerType1 = ledgerType;
					string transactionType1 = transactionType;

					var transaction = GetNewParentForLocalInvoiceAmountEqualToForeignAmountCheck(Factory, ledgerType1, transactionType1, "AUD");
					transaction.AH_LocalTaxAmountOtherTaxes = 6;
					transaction.AH_OutstandingAmount = 11;
					var testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("IsLocalInvoiceAmountEqualToForeignAmount, Ledger = {0}, Type = {1}, Currency = AUD", ledgerType, transactionType),
						true, CriticalValidationErrorType.LocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1_4, LocalInvoiceAmountNotEqualToForeignAmountErrorMessage("AUD", 1),
						transactionType == TransactionTypes.InvoiceBatch ? "" : "\r\nTransaction Header:\r\n- Local Amount: 11\r\n- Foreign Currency Amount: 10");
					AssertOnSavingCheck(transaction, testCase);

					transaction.AH_LocalTaxAmountOtherTaxes = 5;
					transaction.AH_OutstandingAmount = 10;
					testCase = new TestCaseDefinition_ForSeparateTestsMethods(string.Format("IsLocalInvoiceAmountEqualToForeignAmount For OSTransaction With AH_ExchangeRate 1, Ledger = {0}, Type = {1}, Currency = USD", ledgerType, transactionType));
					AssertOnSavingCheck(transaction, testCase);
				}
			}
		}

		public void TestSumOfInvoiceBatchLineInvoiceAmountEqualToInvoiceBatchHeaderInvoiceAmount()
		{
			var testHeader = GetInvoicBatchHeaderWithBatchLines();
			testHeader.AH_InvoiceAmount = 2050m;
			testHeader.AH_OutstandingAmount = 2450;
			var expectedErrorMessage = "Sum Of Invoice Batch Line Invoice Amount does not match the Invoice Batch Header Invoice Amount. Sum of Invoice Batch Line Invoice Amount is 2000 but Invoice Batch Header Invoice Amount is 2050.";

			AssertOnSavingCheck(testHeader, new TestCaseDefinition_ForSeparateTestsMethods(
					"Sum of Invoice Batch Line Invoice Amount should be equal to Invoice Batch Header Invoice Amount",
					true,
					CriticalValidationErrorType.SumOfInvoiceBatchLineAmountNotEqualToInvoiceBatchHeaderAmount,
					expectedErrorMessage));
		}

		public void TestSumOfInvoiceBatchLineLocalTotalEqualToInvoiceBatchHeaderOutstandingAmount()
		{
			var testHeader = GetInvoicBatchHeaderWithBatchLines();
			testHeader.AH_GSTAmount = 250;
			testHeader.AH_OutstandingAmount = 2450;
			var expectedErrorMessage = "Sum of Invoice Batch Line Local Total does not match the Invoice Batch Header Outstanding Amount. Sum of Invoice Batch Line Local Total is 2400 but Invoice Batch Header Outstanding Amount is 2450.";

			AssertOnSavingCheck(testHeader, new TestCaseDefinition_ForSeparateTestsMethods(
					"Sum of Invoice Batch Line Local Total should be equal to Invoice Batch Header Outstanding Amount",
					true,
					CriticalValidationErrorType.SumOfInvoiceBatchLineAmountNotEqualToInvoiceBatchHeaderAmount,
					expectedErrorMessage));
		}

		public void TestSumOfInvoiceBatchLineOSTotalEqualToInvoiceBatchHeaderOSTotal()
		{
			var testHeader = GetInvoicBatchHeaderWithBatchLines();
			testHeader.AH_OSTotal = 7250;
			var expectedErrorMessage = "Sum of Invoice Batch Line OS Total does not match the Invoice Batch Header OS Total. Sum of Invoice Batch Line OS Total is 7200 but Invoice Batch Header OS Total is 7250.";

			AssertOnSavingCheck(testHeader, new TestCaseDefinition_ForSeparateTestsMethods(
					"Sum of Invoice Batch Line OS Total should be equal to Invoice Batch Header OS Total",
					true,
					CriticalValidationErrorType.SumOfInvoiceBatchLineAmountNotEqualToInvoiceBatchHeaderAmount,
					expectedErrorMessage));
		}

		public void TestSumOfInvoiceBatchLineGSTAmountEqualToInvoiceBatchHeaderGSTAmount()
		{
			var testHeader = GetInvoicBatchHeaderWithBatchLines();
			testHeader.AH_GSTAmount = 250;
			testHeader.AH_LocalTaxAmountOtherTaxes = 150m;
			var expectedErrorMessage = "Sum of Invoice Batch Line GST Amount  does not match the Invoice Batch Header GST Amount. Sum of Invoice Batch Line GST Amount is 200 but Invoice Batch Header GST Amount is 250.";

			AssertOnSavingCheck(testHeader, new TestCaseDefinition_ForSeparateTestsMethods(
					"Sum of Invoice Batch Line GST Amount should be equal to Invoice Batch Header GST Amount",
					true,
					CriticalValidationErrorType.SumOfInvoiceBatchLineAmountNotEqualToInvoiceBatchHeaderAmount,
					expectedErrorMessage));
		}

		public void TestCancelledInvoiceBatchHeaderInvoiceAmountEqualToZero()
		{
			var testHeader = GetInvoicBatchHeaderWithBatchLines();
			testHeader.AH_InvoiceAmount = 2000m;
			testHeader.AH_OutstandingAmount = 2400;
			testHeader.IsCancelled = true;
			var expectedErrorMessage = "Canceled Invoice Batch Header Invoice Amount is non-zero";

			AssertOnSavingCheck(testHeader, new TestCaseDefinition_ForSeparateTestsMethods(
					"Canceled Invoice Batch Header Invoice Amount should be zero.",
					true,
					CriticalValidationErrorType.CancelledInvoiceBatchHeaderAmountIsNonZero,
					expectedErrorMessage));
		}

		public void TestCancelledInvoiceBatchHeaderOutstandingAmountEqualToZero()
		{
			var testHeader = GetInvoicBatchHeaderWithBatchLines();
			testHeader.AH_InvoiceAmount = 0;
			testHeader.AH_GSTAmount = 250;
			testHeader.AH_OutstandingAmount = 450;
			testHeader.IsCancelled = true;
			var expectedErrorMessage = "Canceled Invoice Batch Header Outstanding Amount is non-zero";

			AssertOnSavingCheck(testHeader, new TestCaseDefinition_ForSeparateTestsMethods(
					"Canceled Invoice Batch Header Outstanding Amount should be zero.",
					true,
					CriticalValidationErrorType.CancelledInvoiceBatchHeaderAmountIsNonZero,
					expectedErrorMessage));
		}

		public void TestCancelledInvoiceBatchHeaderOSTotalEqualToZero()
		{
			var testHeader = GetInvoicBatchHeaderWithBatchLines();
			testHeader.AH_InvoiceAmount = 0;
			testHeader.AH_OutstandingAmount = 0;
			testHeader.AH_GSTAmount = -200;
			testHeader.AH_OSTotal = 7200;
			testHeader.IsCancelled = true;
			var expectedErrorMessage = "Canceled Invoice Batch Header OS Total is non-zero";

			AssertOnSavingCheck(testHeader, new TestCaseDefinition_ForSeparateTestsMethods(
					"Canceled Invoice Batch Header OS Total should be zero.",
					true,
					CriticalValidationErrorType.CancelledInvoiceBatchHeaderAmountIsNonZero,
					expectedErrorMessage));
		}

		public void TestCancelledInvoiceBatchHeaderGSTAmountEqualToZero()
		{
			var testHeader = GetInvoicBatchHeaderWithBatchLines();
			testHeader.AH_InvoiceAmount = 0;
			testHeader.AH_OutstandingAmount = 0;
			testHeader.AH_OSTotal = 0;
			testHeader.AH_GSTAmount = 200;
			testHeader.AH_LocalTaxAmountOtherTaxes = -200m;
			testHeader.IsCancelled = true;
			var expectedErrorMessage = "Canceled Invoice Batch Header GST Amount is non-zero";

			AssertOnSavingCheck(testHeader, new TestCaseDefinition_ForSeparateTestsMethods(
					"Canceled Invoice Batch Header GST Amount should be zero.",
					true,
					CriticalValidationErrorType.CancelledInvoiceBatchHeaderAmountIsNonZero,
					expectedErrorMessage));
		}

		public void TestExchangeGainLossWhenCategoryIsRealizedExchangeGainLoss_WithLocalBankAccountAndLocalOSAmountDiffer()
		{
			var exchangeGainLossWithLocalBankAccount = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			exchangeGainLossWithLocalBankAccount.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.RealizedExchangeGainLoss;
			exchangeGainLossWithLocalBankAccount.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			exchangeGainLossWithLocalBankAccount.AH_ExchangeRate = 1m;
			exchangeGainLossWithLocalBankAccount.AH_InvoiceAmount = 10m;
			exchangeGainLossWithLocalBankAccount.AH_OSTotal = 0m;
			exchangeGainLossWithLocalBankAccount.AH_AG = TestObjectCreator.GLHeader1.PK;
			AssertOnSavingCheck(exchangeGainLossWithLocalBankAccount, new TestCaseDefinition_ForSeparateTestsMethods(
					"Should throw exception when Bank Account is Local Currency and Local,OS amount differ",
					true,
					CriticalValidationErrorType.LocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1_4,
					string.Format("Local Invoice Amount that is not equal to the Foreign Currency Invoice Amount when we use Local Currency. The Currency is '{0}' and the Exchange Rate is '{1}'.", "AUD", exchangeGainLossWithLocalBankAccount.AH_ExchangeRate)));
		}

		public void TestExchangeGainLossWhenCategoryIsRealizedExchangeGainLoss_WithLocalBankAccountAndLocalOSAmountEqual()
		{
			var exchangeGainLossWithLocalBankAccount = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			exchangeGainLossWithLocalBankAccount.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.RealizedExchangeGainLoss;
			exchangeGainLossWithLocalBankAccount.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			exchangeGainLossWithLocalBankAccount.AH_ExchangeRate = 1m;
			exchangeGainLossWithLocalBankAccount.AH_InvoiceAmount = 10m;
			exchangeGainLossWithLocalBankAccount.AH_OSTotal = 10m;
			exchangeGainLossWithLocalBankAccount.AH_AG = TestObjectCreator.GLHeader1.PK;
			AssertOnSavingCheck(exchangeGainLossWithLocalBankAccount, new TestCaseDefinition_ForSeparateTestsMethods(
					"Should not throw exception when Bank Account is Local Currency and Local,OS amount equal",
					false,
					CriticalValidationErrorType.LocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1_4,
					""));
		}

		public void TestExchangeGainLossWhenCategoryIsRealizedExchangeGainLoss_WithOSBankAccountAndLocalOSAmountDiffer()
		{
			var exchangeGainLossWithLocalBankAccount = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			exchangeGainLossWithLocalBankAccount.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.RealizedExchangeGainLoss;
			exchangeGainLossWithLocalBankAccount.AH_AB = TestObjectCreator.USDBankAccount.PK;
			exchangeGainLossWithLocalBankAccount.AH_ExchangeRate = 1m;
			exchangeGainLossWithLocalBankAccount.AH_InvoiceAmount = 10m;
			exchangeGainLossWithLocalBankAccount.AH_OSTotal = 0m;
			exchangeGainLossWithLocalBankAccount.AH_AG = TestObjectCreator.GLHeader1.PK;
			AssertOnSavingCheck(exchangeGainLossWithLocalBankAccount, new TestCaseDefinition_ForSeparateTestsMethods(
					"Should throw exception when Bank Account is OS Currency and Local,OS amount differ",
					false,
					CriticalValidationErrorType.LocalInvoiceAmountNotEqualToTheForeignCurrencyInvoiceAmountWhenExRateIs1_4,
					""));
		}

		#region GL Journal Entries Number Has Been Assigned

		public void TestGLJournalEntriesNumberHasBeenAssigned_NoGLD()
		{
			var standardJournal = Factory.NewWithValidTestData<AccTransactionHeader>();
			standardJournal.AH_Ledger = LedgerTypes.General;
			standardJournal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			standardJournal.AH_PostDate = new ZDateTime(2009, 11, 10);

			Factory.Save();
			AssertEquals(new ZDateTime(2009, 11, 10), standardJournal.AH_PostDate);

			standardJournal.AH_PostDate = new ZDateTime(2009, 11, 20);
			AssertOnSavingCheck(standardJournal, new TestCaseDefinition_ForSeparateTestsMethods("GL Journal Entries Number Has Not Been Assigned, Can change for Target Journal."));

			var generalLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			generalLedgerData.GLD_Type = "PST";
			generalLedgerData.GLD_GLAccountType = "TLG";
			generalLedgerData.GLD_PostDate = ZDateTime.Today;
			generalLedgerData.GLD_PostPeriod = 1;
			generalLedgerData.GLD_GC_Company = GlbCompany.CurrentCompany.PK;
			generalLedgerData.GLD_GB_Branch = GlbBranch.CurrentBranch.PK;
			generalLedgerData.GLD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			generalLedgerData.GLD_AH_TransactionHeader = standardJournal.PK;
			Factory.Save();
			standardJournal.AH_PostDate = new ZDateTime(2009, 11, 10);

			AssertOnSavingCheck(standardJournal, new TestCaseDefinition_ForSeparateTestsMethods("GL Journal Entries Number Has Not Been Assigned but not use GLD, Can change for Target Journal."));
		}

		public void TestGLJournalEntriesNumberHasBeenAssigned_StandardJournal()
		{
			var standardJournal = Factory.NewWithValidTestData<AccTransactionHeader>();
			standardJournal.AH_Ledger = LedgerTypes.General;
			standardJournal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			standardJournal.AH_PostDate = new ZDateTime(2009, 11, 10);
			CheckGLJournalEntriesNumberHasBeenAssigned(standardJournal);
		}

		public void TestGLJournalEntriesNumberHasBeenAssigned_NoteJournal()
		{
			var noteJournal = Factory.NewWithValidTestData<AccTransactionHeader>();
			noteJournal.AH_Ledger = LedgerTypes.General;
			noteJournal.AH_TransactionType = TransactionTypes.GLNoteJournal;
			noteJournal.AH_PostDate = new ZDateTime(2009, 11, 10);
			CheckGLJournalEntriesNumberHasBeenAssigned(noteJournal);
		}

		public void TestGLJournalEntriesNumberHasBeenAssigned_ReverseJournal()
		{
			var reverseJournal = Factory.NewWithValidTestData<AccTransactionHeader>();
			reverseJournal.AH_Ledger = LedgerTypes.General;
			reverseJournal.AH_TransactionType = TransactionTypes.GLReversingJournal;
			reverseJournal.AH_PostDate = new ZDateTime(2009, 11, 10);
			reverseJournal.AH_DueDate = new ZDateTime(2009, 11, 10);
			CheckGLJournalEntriesNumberHasBeenAssigned(reverseJournal);
		}

		public void TestGLJournalEntriesNumberHasBeenAssigned_AutoJournal()
		{
			var autoJournal = Factory.NewWithValidTestData<AccTransactionHeader>();
			autoJournal.AH_Ledger = LedgerTypes.General;
			autoJournal.AH_TransactionType = TransactionTypes.GLAutoJournal;
			autoJournal.AH_PostDate = new ZDateTime(2009, 11, 10);
			autoJournal.AH_DueDate = new ZDateTime(2009, 11, 10);
			CheckGLJournalEntriesNumberHasBeenAssigned(autoJournal);
		}

		void CheckGLJournalEntriesNumberHasBeenAssigned(AccTransactionHeader target)
		{
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Factory.Save();
				AssertEquals(new ZDateTime(2009, 11, 10), target.AH_PostDate);

				target.AH_PostDate = new ZDateTime(2009, 11, 20);
				AssertOnSavingCheck(target, new TestCaseDefinition_ForSeparateTestsMethods("GL Journal Entries Number Has Not Been Assigned, Can change for Target Journal."));

				var generalLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
				generalLedgerData.GLD_Type = "PST";
				generalLedgerData.GLD_GLAccountType = "TLG";
				generalLedgerData.GLD_PostDate = ZDateTime.Today;
				generalLedgerData.GLD_PostPeriod = 1;
				generalLedgerData.GLD_GC_Company = GlbCompany.CurrentCompany.PK;
				generalLedgerData.GLD_GB_Branch = GlbBranch.CurrentBranch.PK;
				generalLedgerData.GLD_GE_Department = GlbDepartment.CurrentDepartment.PK;
				generalLedgerData.GLD_AH_TransactionHeader = target.PK;
				generalLedgerData.GLD_JournalEntriesNumber = "test";
				generalLedgerData.GLD_JournalEntriesNumberRuleCode = "test";
				Factory.Save();
				target.AH_PostDate = new ZDateTime(2009, 11, 10);

				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
					"GL Journal Entries Number Has Been Assigned, Can Not Save For Target Journal.",
					true,
					CriticalValidationErrorType.GLJournalEntriesNumberHasBeenAssigned,
					"An unique reference number has been allocated while you were editing the GL Journal.\r\nSaving has been aborted to protect the allocated reference number.\r\nPlease reverse the GL Journal and re-enter.");

				AssertOnSavingCheck(target, testCase);
			}
		}

		#endregion

		List<TestCaseDefinitionWithDelegate_Obsolete> GetLocalInvoiceAmountEqualToForeignAmountCases()
		{
			List<TestCaseDefinitionWithDelegate_Obsolete> result = new List<TestCaseDefinitionWithDelegate_Obsolete>();

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("IsLocalInvoiceAmountEqualToForeignAmount For OSTransaction With AH_ExchangeRate 1, Ledger = CashBook, Type = DirectDebitBatch, Currency = AUD",
						factory => GetNewParentForLocalInvoiceAmountEqualToForeignAmountCheck(factory, LedgerTypes.CashBook, TransactionTypes.DDRBatch, "AUD")));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("IsLocalInvoiceAmountEqualToForeignAmount For OSTransaction With AH_ExchangeRate 1, Ledger = CashBook, Type = DirectDebitBatch, Currency = USD",
						factory => GetNewParentForLocalInvoiceAmountEqualToForeignAmountCheck(factory, LedgerTypes.CashBook, TransactionTypes.DDRBatch, "USD")));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("IsLocalInvoiceAmountEqualToForeignAmount For OSTransaction With AH_ExchangeRate 1, Ledger = CashBook, Type = ReceiptBatch, Currency = AUD",
						factory => GetNewParentForLocalInvoiceAmountEqualToForeignAmountCheck(factory, LedgerTypes.CashBook, TransactionTypes.ReceiptBatch, "AUD")));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("IsLocalInvoiceAmountEqualToForeignAmount For OSTransaction With AH_ExchangeRate 1, Ledger = CashBook, Type = ReceiptBatch, Currency = USD",
						factory => GetNewParentForLocalInvoiceAmountEqualToForeignAmountCheck(factory, LedgerTypes.CashBook, TransactionTypes.ReceiptBatch, "USD")));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("IsLocalInvoiceAmountEqualToForeignAmount, Ledger - CashBook, TransactionType - ReceiptBatch, Currency = AUD, valid",
						factory => GetNewParentForLocalInvoiceAmountEqualToForeignAmountCheck(factory, LedgerTypes.CashBook, TransactionTypes.ReceiptBatch, "AUD")));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("IsLocalInvoiceAmountEqualToForeignAmount, invalid transaction in db, modified - now fails for transaction header OS amount being changed after save",
				delegate(BusinessObjectFactory factory)
				{
					AccTransactionHeader invalidInDb = GetInvalidTransactionForOutstandingAmountAndFullyPaidDateCheckSavedInDb(factory);
					invalidInDb.AH_OSTotal = 100;
					Assert("Just reset OSTotal amount so there should be changes", invalidInDb.AH_OSTotalInfo.HasChanges);
					return invalidInDb;
				}, true, CriticalValidationErrorType.TransactionHeaderOSAmountWasModifiedAfterBeingSaved_2, CriticalValidationMessageTemplate.TransactionHeaderOSAmountWasModifiedAfterBeingSaved));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("IsLocalInvoiceAmountEqualToForeignAmount, invalid transaction in db, unmodified - passes",
				factory => GetInvalidTransactionForOutstandingAmountAndFullyPaidDateCheckSavedInDb(factory)));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("IsLocalInvoiceAmountEqualToForeignAmount, invalid transaction (Direct Debit Batch) in db, modified - fails",
				delegate(BusinessObjectFactory factory)
				{
					AccTransactionHeader invalidInDb = GetInvalidTransactionForOutstandingAmountAndFullyPaidDateCheckSavedInDb(factory, LedgerTypes.CashBook, TransactionTypes.DDRBatch, "USD");
					invalidInDb.AH_OSTotal = 100;
					Assert("Just reset OSTotal amount so there should be changes", invalidInDb.AH_OSTotalInfo.HasChanges);
					return invalidInDb;
				},
				true,
				CriticalValidationErrorType.DirectDebitBatchOSAmountIsNotEqualToSumOfAllPaymentOSAmountsWhenBatchIsInForeignCurrency,
				"Direct Debit Batch Header OS Amount does not match sum of Payment OS Amount. Payment OS Amount Total is 0, but Header OS Amount is 100."));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("IsLocalInvoiceAmountEqualToForeignAmount, invalid transaction (Direct Debit Batch) in db, unmodified - passes",
				factory => GetInvalidTransactionForOutstandingAmountAndFullyPaidDateCheckSavedInDb(factory, LedgerTypes.CashBook, TransactionTypes.DDRBatch, "USD")));

			return result;
		}

		List<TestCaseDefinitionWithDelegate_Obsolete> GetClearingJournalAH_AGEmptyCases()
		{
			List<TestCaseDefinitionWithDelegate_Obsolete> result = new List<TestCaseDefinitionWithDelegate_Obsolete>();

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("AP Clearing Journal with GL Account", factory =>
			{
				AccTransactionHeader transaction = factory.New<APJournal>();
				transaction.AH_TransactionCategory = Constants.TransactionCategory.Codes.Clearing;
				transaction.AH_AG = TestObjectCreator.GLHeader1.PK;
				transaction.AH_TransactionNum = "VALUEFORTEST";
				return transaction;
			}));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("AR Clearing Journal with GL Account", factory =>
			{
				AccTransactionHeader transaction = factory.New<ARJournal>();
				transaction.AH_TransactionCategory = Constants.TransactionCategory.Codes.Clearing;
				transaction.AH_AG = TestObjectCreator.GLHeader1.PK;
				transaction.AH_TransactionNum = "VALUEFORTEST";
				return transaction;
			}));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("AR Clearing Journal for Installments with GL Account", factory =>
			{
				AccTransactionHeader transaction = factory.New<ARJournal>();
				transaction.AH_TransactionCategory = Constants.TransactionCategory.Codes.ClearingJournal;
				transaction.AH_AG = TestObjectCreator.GLHeader1.PK;
				transaction.AH_TransactionNum = "VALUEFORTEST";
				return transaction;
			}));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("AP Clearing Journal without GL Account", factory =>
			{
				AccTransactionHeader transaction = factory.New<APJournal>();
				transaction.AH_TransactionCategory = Constants.TransactionCategory.Codes.Clearing;
				transaction.AH_AG = ZGuid.Empty;
				transaction.AH_TransactionNum = "VALUEFORTEST";
				return transaction;
			}, true, CriticalValidationErrorType.TransactionWithEmptyGLAccountField_7, "Clearing Journal with empty GL account field"));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("AR Clearing Journal without GL Account", factory =>
			{
				AccTransactionHeader transaction = factory.New<ARJournal>();
				transaction.AH_TransactionCategory = Constants.TransactionCategory.Codes.Clearing;
				transaction.AH_AG = ZGuid.Empty;
				transaction.AH_TransactionNum = "VALUEFORTEST";
				return transaction;
			}, true, CriticalValidationErrorType.TransactionWithEmptyGLAccountField_7, "Clearing Journal with empty GL account field"));

			result.Add(new TestCaseDefinitionWithDelegate_Obsolete("AR Clearing Journal for Installments without GL Account", factory =>
			{
				AccTransactionHeader transaction = factory.New<ARJournal>();
				transaction.AH_TransactionCategory = Constants.TransactionCategory.Codes.ClearingJournal;
				transaction.AH_AG = ZGuid.Empty;
				transaction.AH_TransactionNum = "VALUEFORTEST";
				return transaction;
			}, true, CriticalValidationErrorType.TransactionWithEmptyGLAccountField_7, "Clearing Journal with empty GL account field"));

			return result;
		}

		List<TestCaseDefinitionWithDelegate_Obsolete> GetGLAccountNotSetCases()
		{
			List<TestCaseDefinitionWithDelegate_Obsolete> result = new List<TestCaseDefinitionWithDelegate_Obsolete>();

			foreach (string ledgerType in ledgerTypes)
			{
				foreach (string transactionType in transactionTypesForNoGLAccountCheck)
				{
					string ledgerType1 = ledgerType;
					string transactionType1 = transactionType;
					result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("{0} {1} with valid GL account field", ledgerType, transactionType), factory =>
					{
						AccTransactionHeader transaction = GetNewParentForNoGLAccountCheck(factory, ledgerType1, transactionType1);
						transaction.AH_AG = TestObjectCreator.GLHeader1.PK;
						return transaction;
					}));
					result.Add(new TestCaseDefinitionWithDelegate_Obsolete(string.Format("{0} {1} with empty GL account field", ledgerType, transactionType), factory =>
					{
						return GetNewParentForNoGLAccountCheck(factory, ledgerType1, transactionType1);
					}, true, CriticalValidationErrorType.TransactionWithEmptyGLAccountField_7, NoGLAccountErrorMessage(ledgerType, transactionType)));
				}
			}

			return result;
		}

		#region Implementation

		readonly string[] ledgerTypes = new[] { LedgerTypes.AccountsPayable, LedgerTypes.AccountsReceivable };

		readonly string[] transactionTypesForIsCancelledCheck = new[] {
				TransactionTypes.AdjustmentNote,
				TransactionTypes.CreditNote,
				TransactionTypes.Contra,
				TransactionTypes.Discount,
				TransactionTypes.ExchangeDifference,
				TransactionTypes.Invoice,
				TransactionTypes.Journal,
				TransactionTypes.Overpayment,
				TransactionTypes.Payment,
				TransactionTypes.Receipt,
				TransactionTypes.Transfer
			};

		readonly string[] transactionTypes = new[] {
				TransactionTypes.AdjustmentNote,
				TransactionTypes.CreditNote,
				TransactionTypes.Invoice,
				TransactionTypes.Payment,
				TransactionTypes.Receipt
			};

		readonly string[] transactionTypesForLocalInvoiceAmountEqualToForeignAmount = new[] {
				TransactionTypes.AdjustmentNote,
				TransactionTypes.CreditNote,
				TransactionTypes.Invoice,
				TransactionTypes.Payment,
				TransactionTypes.Receipt,
				TransactionTypes.InvoiceBatch
			};

		readonly string[] transactionTypesForNoGLAccountCheck = new[] {
				TransactionTypes.ExchangeDifference,
				TransactionTypes.Journal,
				TransactionTypes.Discount,
				TransactionTypes.Overpayment
			};

		protected string IsCancelledErrorMessage
		{
			get
			{
				return @"Missing Reversing Transaction for this canceled transaction";
			}
		}

		protected string OutstandingAmountAndFullyPaidDateErrorMessage
		{
			get
			{
				return @"Invalid Fully Paid Date with respect to the outstanding amount.";
			}
		}

		protected string LocalInvoiceAmountNotEqualToForeignAmountErrorMessage(ZString currency, ZDecimal exchangeRate)
		{
			return String.Format("Local Invoice Amount that is not equal to the Foreign Currency Invoice Amount when we use Local Currency. The Currency is '{0}' and the Exchange Rate is '{1}'.", currency, exchangeRate);
		}

		protected string LocalInvoiceAmountNotEqualToForeignAmountErrorMessageAdditionalInfo(string ledger, string transactionType)
		{
			string message = string.Empty;
			if ((ledger == LedgerTypes.AccountsPayable || ledger == LedgerTypes.AccountsReceivable) &&
				(transactionType == TransactionTypes.Invoice || transactionType == TransactionTypes.CreditNote || transactionType == TransactionTypes.AdjustmentNote))
			{
				message = "\r\nTransaction Header:\r\n- Local Amount: 5\r\n- Foreign Currency Amount: 10\r\n- Currency: AUD\r\n- Exchange Rate: 1\r\n- Company: EDI\r\n- Company Local Currency: AUD";
			}
			return message;
		}

		protected string NoGLAccountErrorMessage(string ledgerType, string transactionType)
		{
			string message = string.Empty;
			if ((ledgerType == LedgerTypes.AccountsPayable || ledgerType == LedgerTypes.AccountsReceivable) &&
				(transactionType == TransactionTypes.Journal || transactionType == TransactionTypes.Discount || transactionType == TransactionTypes.Overpayment
				|| transactionType == TransactionTypes.ExchangeDifference))
			{
				message = string.Format("{0} {1} with an empty GL account field", ledgerType, transactionType);
			}

			message = "with an empty GL account field";

			return message;
		}

		protected virtual TransactionHeader GetNewParentForJobInvoiceNumberExcceedTheMaximumNumberCheck(BusinessObjectFactory factory, string ledgerType, string transactionType)
		{
			var header = Factory.NewWithValidTestData<ARInvoice>();
			header.AH_Ledger = ledgerType;
			header.AH_TransactionType = transactionType;

			var job = Factory.NewJobForTesting<Job>();
			job.JH_JobNum = "S00001024";
			header.AH_JH = job.PK;

			var existedInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			existedInvoice.AH_Ledger = "AR";
			existedInvoice.AH_ConsolidatedInvoiceRef = "S00001024/ZZ";
			existedInvoice.AH_JH = job.PK;

			InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(header, job);
			return header;
		}

		protected virtual TransactionHeader GetNewParentForIsCancelledCheck(BusinessObjectFactory factory, string ledgerType, string transactionType)
		{
			AccTransactionHeader header = factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = ledgerType;
			header.AH_TransactionType = transactionType;
			header.IsCancelled = true;

			TransactionHeader result = factory.Load<TransactionHeader>(header.PK);
			return result;
		}

		protected virtual TransactionHeader GetNewParentForOutstandingAmountAndFullyPaidDateCheck(BusinessObjectFactory factory, string ledgerType, string transactionType)
		{
			AccTransactionHeader header = factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = ledgerType;
			header.AH_TransactionType = transactionType;
			header.AH_InvoiceAmount = 5m;
			header.AH_OutstandingAmount = 0m;
			header.AH_FullyPaidDate = ZDateTime.Empty;

			TransactionHeader result = factory.Load<TransactionHeader>(header.PK);
			TransactionHeaderWithLines headerWithLines = result as TransactionHeaderWithLines;
			if (headerWithLines != null)
			{
				headerWithLines.Lines.AddNew().AL_LineAmount = 5m;
			}
			TransactionMatchLink match = ((IMatching)result).Matchlinks.AddNew();
			match.AP_AH = result.PK;
			match.AP_Amount = 5m;
			match.AP_MatchGroupNum = "M000971";
			match.AP_MatchDate = new ZDateTime(2016, 10, 20);
			return result;
		}

		protected virtual TransactionHeader GetNewParentForLocalInvoiceAmountEqualToForeignAmountCheck(BusinessObjectFactory factory, string ledgerType, string transactionType, string transactionCurrency)
		{
			if (ledgerType == LedgerTypes.CashBook && transactionType == TransactionTypes.DDRBatch)
			{
				if (transactionCurrency == "AUD")
				{
					var directPayment1 = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 200m, 20m, 300m, 30m, TestObjectCreator.AUDBankAccount.PK, 1m);
					directPayment1.AH_ReceiptType = ReceiptTypes.DirectDebit;

					var batch1 = Factory.New<DirectDebitBatchHeader>();
					batch1.AH_AB = TestObjectCreator.AUDBankAccount.PK;
					batch1.AH_TransactionNum = "00001000";
					return batch1;
				}
				else
				{
					var directPayment1 = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 200m, 20m, 300m, 30m, TestObjectCreator.USDBankAccount.PK, 0.5m);
					directPayment1.AH_ReceiptType = ReceiptTypes.DirectDebit;

					var batch1 = Factory.New<DirectDebitBatchHeader>();
					batch1.AH_AB = TestObjectCreator.USDBankAccount.PK;
					batch1.AH_TransactionNum = "00001000";
					return batch1;
				}
			}
			else
			{
				var header = factory.NewWithValidTestData<AccTransactionHeader>();
				header.AH_GC = GlbCompany.CurrentCompany.PK;
				header.AH_GB = GlbBranch.CurrentBranch.PK;
				header.AH_Ledger = ledgerType;
				header.AH_TransactionType = transactionType;
				header.AH_RX_NKTransactionCurrency = transactionCurrency;
				header.AH_ExchangeRate = 1M;
				header.AH_OSTotal = 10m;
				header.AH_InvoiceAmount = 5m;
				header.AH_OutstandingAmount = 5m;

				var result = factory.Load<TransactionHeader>(header.PK);
				var headerWithLines = result as TransactionHeaderWithLines;
				if (headerWithLines != null)
				{
					headerWithLines.Lines.AddNew().AL_LineAmount = 5m;
					headerWithLines.Lines[0].AL_OSAmount = 10m;
				}
				return result;
			}
		}

		AccTransactionHeader GetNewParentForNoGLAccountCheck(BusinessObjectFactory factory, string ledgerType, string transactionType)
		{
			AccTransactionHeader transaction = null;

			switch (ledgerType)
			{
				case LedgerTypes.AccountsPayable:
					switch (transactionType)
					{
						case TransactionTypes.Journal:
							transaction = TestObjectCreator.CreateJournal<APJournal>(0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
							break;
						case TransactionTypes.ExchangeDifference:
							transaction = TestObjectCreator.CreateExchangeDifference<APExchangeDifference>(0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
							break;
						case TransactionTypes.Discount:
							transaction = TestObjectCreator.CreateAPDiscount(0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
							break;
						case TransactionTypes.Overpayment:
							transaction = TestObjectCreator.CreateOverpayment<APOverpayment>(0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
							break;
					}
					break;

				case LedgerTypes.AccountsReceivable:
					switch (transactionType)
					{
						case TransactionTypes.Journal:
							transaction = TestObjectCreator.CreateJournal<ARJournal>(0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
							break;
						case TransactionTypes.ExchangeDifference:
							transaction = transaction = TestObjectCreator.CreateExchangeDifference<ARExchangeDifference>(0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
							break;
						case TransactionTypes.Discount:
							transaction = transaction = TestObjectCreator.CreateARDiscount(0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
							break;
						case TransactionTypes.Overpayment:
							transaction = TestObjectCreator.CreateOverpayment<AROverpayment>(0m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
							break;
					}
					break;

				default:
					return null;
			}
			transaction.AH_AG = ZGuid.Empty;
			return transaction;
		}

		AccTransactionHeader GetInvalidTransactionSavedInDb(BusinessObjectFactory factory)
		{
			return GetInvalidTransactionSavedInDb(factory, true, false);
		}

		AccTransactionHeader GetInvalidTransactionSavedInDb(BusinessObjectFactory factory, bool isCancelled)
		{
			return GetInvalidTransactionSavedInDb(factory, isCancelled, false);
		}

		AccTransactionHeader GetInvalidTransactionSavedInDb(BusinessObjectFactory factory, bool isCancelled, bool withFullyPaidDate)
		{
			string insertCommand = string.Format("INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_TransactionNum, AH_GB, AH_GE, AH_Ledger, AH_TransactionType, AH_InvoiceDate, " +
				(withFullyPaidDate ? "AH_FullyPaidDate," : "") +
				" AH_ExchangeRate, AH_RX_NKTransactionCurrency, AH_IsCancelled, AH_InvoiceAmount, AH_GC, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser) VALUES (NEWID(), '{0}', '{1}', '{2}', '{3}', '{4}', GETDATE()," +
				(withFullyPaidDate ? "GETDATE()," : "") +
				" 1, '{5}', {6}, 123, '{7}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
				(++transactionNum).ToString(), GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, ledgerTypes[0], transactionTypes[0], "AUD", isCancelled ? 1 : 0, GlbCompany.CurrentCompany.PK);
			Db.Connection.ExecuteNonQuery(insertCommand);

			TransactionHeader result = factory.LoadTop1<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, transactionNum.ToString()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertNotNull("Invalid Transaction should be loaded from DB", result);

			TransactionHeaderWithLines resultAsTransactionHeaderWithLines = result as TransactionHeaderWithLines;
			if (resultAsTransactionHeaderWithLines != null)
			{
				DependentTransactionLine line = resultAsTransactionHeaderWithLines.Lines.AddNew();
				line.AL_LineAmount = result.AH_InvoiceAmount;
			}

			result.AH_InvoicePrinted = true;
			Assert("Should have changes", result.HasChanges);

			return result;
		}

		AccTransactionHeader GetInvalidTransactionForOutstandingAmountAndFullyPaidDateCheckSavedInDb(BusinessObjectFactory factory)
		{
			return GetInvalidTransactionForOutstandingAmountAndFullyPaidDateCheckSavedInDb(factory, ledgerTypes[0], transactionTypes[0], "AUD");
		}

		AccTransactionHeader GetInvalidTransactionForOutstandingAmountAndFullyPaidDateCheckSavedInDb(BusinessObjectFactory factory, string ledgerType, string transactionType, string currency)
		{
			string insertCommand = string.Format("INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_TransactionNum, AH_GB, AH_GE, AH_Ledger, AH_TransactionType, AH_InvoiceDate, " +
				" AH_ExchangeRate, AH_RX_NKTransactionCurrency, AH_InvoiceAmount, AH_OutstandingAmount, AH_OSTotal, AH_GC, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser) VALUES (NEWID(), '{0}', '{1}', '{2}', '{3}', '{4}', GETDATE()," +
				" 1, '{5}', 123, 123, 456, '{6}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
				(++transactionNum).ToString(), GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, ledgerType, transactionType, currency, GlbCompany.CurrentCompany.PK.ToGuid());
			Db.Connection.ExecuteNonQuery(insertCommand);

			TransactionHeader result = factory.LoadTop1<TransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, transactionNum.ToString()).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertNotNull("Invalid Transaction should be loaded from DB", result);

			result.AH_InvoicePrinted = true;
			Assert("Should have changes", result.HasChanges);

			return result;
		}

		InvoiceBatchHeader GetInvoicBatchHeaderWithBatchLines()
		{
			var arInvoiceBatchLine1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "APINV1", TestObjectCreator.CNY, 2m, 2000m, 200m, 1000m, 100m);
			var arInvoiceBatchLine2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "APINV2", TestObjectCreator.CNY, 4m, 4000m, 400m, 1000m, 100m);
			arInvoiceBatchLine1.AH_LocalTaxAmountOtherTaxes = 100M;
			arInvoiceBatchLine2.AH_LocalTaxAmountOtherTaxes = 100M;
			arInvoiceBatchLine1.AH_OSTaxAmountOtherTaxes = 200M;
			arInvoiceBatchLine2.AH_OSTaxAmountOtherTaxes = 400M;

			Factory.Save();

			var testHeader = Factory.NewWithValidTestData<InvoiceBatchHeader>();
			testHeader.AH_RX_NKTransactionCurrency = TestObjectCreator.CNY.RX_Code;
			testHeader.AH_LocalTaxAmountOtherTaxes = 200M;
			testHeader.Line.Add(arInvoiceBatchLine1);
			testHeader.Line.Add(arInvoiceBatchLine2);

			return testHeader;
		}

		int transactionNum = 100;

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_cached ?? (TestObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_cached;

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator
		{
			get { return taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory)); }
		}
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;

		#endregion
	}
}

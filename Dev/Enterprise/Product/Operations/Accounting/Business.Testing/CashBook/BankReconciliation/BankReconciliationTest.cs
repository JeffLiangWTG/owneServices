using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.DocumentEngine.Testing;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Statement = Enterprise.Accounting.Business.Base.AccStatement.Statement;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	[TestedType(typeof(BankReconciliation))]
	public class BankReconciliationTest : NonPersistentBusinessObjectTestCase
	{
		public class MockBankReconciliation : BankReconciliation
		{
			public bool HasCashbookSummaryRan;

			public MockBankReconciliation(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected new ZDecimal GetCashBookAmount(ZGuid bank, ZDateTime postDate)
			{
				HasCashbookSummaryRan = true;
				return base.GetCashBookAmount(bank, postDate);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BankReconciliation(Factory);
		}

		public void TestBankAccountListNotificationForCashAccount()
		{
			var cashAccount = TestObjectCreator.CreateBankAccount("TST", "Test Cash Account", TestObjectCreator.AUD, TestObjectCreator.GLHeader1, AccountTypeCodeDescriptionPairList.Codes.CSH);

			var bankRecon = new BankReconciliation(Factory);
			var collection = (AccBankAccountCollection)MetaData.GetListDataSource(bankRecon, bankRecon.BankAccountPKInfo.PropertyDescriptor);
			AssertEquals("The selected Bank Account is a Cash Account. Please select a suitable Bank Account for Bank Reconciliation", collection.GetAllNotificationsWhenAdditionalFilterNotMet(cashAccount));
		}

		public void TestTextFilterForTransactionShouldNotContainColumnIfTextSearchFilterExceedsItsMaxLength()
		{
			var recon = (BankReconciliation)GetNewBusinessObject();
			var someLongText = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

			recon.TextSearchFilter = someLongText.Substring(0, AccTransactionHeaderSchema.AH_ReceiptBatchNo.MaxLength);
			var filterToTest = recon.GetTextFilterForTransaction_ForTestOnly();
			Assert("Filter contains column name", filterToTest.FilterString.Contains(AccTransactionHeaderSchema.AH_ReceiptBatchNo.Name));
			Assert("Filter contains column name", filterToTest.FilterString.Contains(AccTransactionHeaderSchema.AH_ChequeOrReference.Name));

			recon.TextSearchFilter = someLongText.Substring(0, AccTransactionHeaderSchema.AH_ReceiptBatchNo.MaxLength + 1);
			filterToTest = recon.GetTextFilterForTransaction_ForTestOnly();
			Assert("Filter does not contain column name due to exceeding length", !filterToTest.FilterString.Contains(AccTransactionHeaderSchema.AH_ReceiptBatchNo.Name));
			Assert("Filter contains column name", filterToTest.FilterString.Contains(AccTransactionHeaderSchema.AH_ChequeOrReference.Name));
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			recon.TextSearchFilter = someLongText.Substring(0, AccTransactionHeaderSchema.AH_ChequeOrReference.MaxLength);
			filterToTest = recon.GetTextFilterForTransaction_ForTestOnly();
			Assert("Filter does not contain column name due to exceeding length", !filterToTest.FilterString.Contains(AccTransactionHeaderSchema.AH_ReceiptBatchNo.Name));
			Assert("Filter contains column name", filterToTest.FilterString.Contains(AccTransactionHeaderSchema.AH_ChequeOrReference.Name));
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			int maxLength = AccTransactionHeaderSchema.AH_ChequeOrReference.MaxLength;
			try
			{
				recon.TextSearchFilter = someLongText.Substring(0, maxLength + 1);
				Fail("Should not be executed");
			}
			catch (MaxLengthExceededException e)
			{
				CombineAssertions(() =>
				{
					AssertContains($"The maximum length of '{nameof(recon.TextSearchFilter)}' has been exceeded.", e.Message);
					AssertContains($"The maximum length of this property is {maxLength} characters, but {maxLength + 1} were entered.", e.Message);
				});
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesBankReconciliation()
		{
			var recon = (BankReconciliation)GetNewBusinessObject();
			recon.BankAccountPK = TestBank.PK;
			recon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			recon.StatementDate = recon.ReconcileDate;

			var bankList = new List<string>
				{
					nameof(recon.CashBookBalance),
					nameof(recon.ClosingBalance),
					nameof(recon.ClosingBalanceReadOnly),
					nameof(recon.AmendedBankStatementBalance),
					nameof(recon.UnclearedStatementAmount),
					nameof(recon.UnclearedCashbookAmount),
					nameof(recon.CashbookTotal),
					nameof(recon.StatementTotal),
					nameof(recon.TotalDifference),
					nameof(recon.CurrentDebitTotal),
					nameof(recon.CurrentCreditTotal),
					nameof(recon.ReconError),
					nameof(recon.AmountFilter)
				};

			var tester = new DecimalPlacesAttributeTester(recon);
			tester.CheckNonLocalCurrency(bankList, nameof(recon.BankCurrencyDecimalsAsInt), nameof(recon.BankAccount.AB_RX_NKAccountCurrency), recon.BankAccount);
		}

		internal void SetupTransactions(AccBankAccount fBankAccount, BusinessObjectFactory factory)
		{
			SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 120.0m, fBankAccount, factory, "");

			SetTransaction(LedgerTypes.CashBook, TransactionTypes.DirectPayment, -120.0m, fBankAccount, factory, "");
			SetTransaction(LedgerTypes.CashBook, TransactionTypes.Transfer, 430.0m, fBankAccount, factory, "");

			AccTransactionHeader rCBatch = SetTransaction(LedgerTypes.CashBook, TransactionTypes.ReceiptBatch, 0.0m, fBankAccount, factory, "");

			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, 200.0m, fBankAccount, factory, "");
			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, 200.0m, fBankAccount, factory, rCBatch.AH_TransactionNum);

			SetTransaction(LedgerTypes.CashBook, TransactionTypes.OpeningReceipt, 320.0m, fBankAccount, factory, "");
			SetTransaction(LedgerTypes.CashBook, TransactionTypes.OpeningPayment, 500.0m, fBankAccount, factory, "");

			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 500.0m, fBankAccount, factory, "");

			AccTransactionHeader lateDDRBatch = SetTransaction(LedgerTypes.CashBook, TransactionTypes.DDRBatch, 0.0m, fBankAccount, factory, "");
			lateDDRBatch.AH_PostDate = Env.Time.CurrentLocalDateTime.AddDays(5);
			lateDDRBatch.AH_ReceiptType = ReceiptTypes.NonRolledUpBatch;

			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Payment, 700.0m, fBankAccount, factory, lateDDRBatch.AH_TransactionNum);

			AccTransactionHeader payment2 = SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 120.0m, fBankAccount, factory, "");

			SetStatement("DR", "CHQ", 130, "TEST1", fBankAccount, factory);
			SetStatement("DR", "CHQ", 140, "TEST1", fBankAccount, factory);
			SetStatement("CR", "CHQ", 150, "TEST2", fBankAccount, factory);

			payment2.AH_AB = ZGuid.Empty;
		}

		public (AccTransactionHeader unclearedTransaction, AccTransactionHeader clearedTransaction, BankReconciliation bankRecon) SetupTransactionsForExternalTests()
		{
			SetupTransactions(TestBank, Factory);
			var unclearedTransaction = SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 120.0m, TestBank, Factory, "");
			var clearedTransaction = SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 110.0m, TestBank, Factory, "");
			clearedTransaction.AH_DateClearedInCashbook = Env.Time.CurrentLocalDate;
			Factory.Save();

			var bankRecon = (BankReconciliation)GetNewBusinessObject();
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDate;
			bankRecon.StatementDate = bankRecon.ReconcileDate;
			bankRecon.ClosingBalance = 1000;
			bankRecon.ClearedFilter = true;
			bankRecon.ApplyFilter();

			return (unclearedTransaction, clearedTransaction, bankRecon);
		}

		[SuspendCriticalValidation]
		public void TestGetStatement()
		{
			SetupTransactions(TestBank, Factory);
			Factory.Save();

			var bankRecon = new MockBankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			AssertEquals("3 statements returned by GetStatements() after applying filter", 3, bankRecon.GetStatements().Count);
			bankRecon.BankAccount.GetStatements_ForTestOnly().Load();
			AssertEquals("BankRecon.BankAccount.Statements also should have 3 statements - no filter should be applied", 3, bankRecon.BankAccount.GetStatements_ForTestOnly().Count);

			var statmentToUpdateInNewFactory = new BusinessObjectFactory().Load<Statement>(bankRecon.GetStatements()[0].PK);
			statmentToUpdateInNewFactory.AS_IsCleared = true;
			statmentToUpdateInNewFactory.Factory.Save();

			bankRecon.ReloadRecords();

			AssertEquals("2 unreconciled statements remain after applying filter", 2, bankRecon.GetStatements().Count);
			bankRecon.BankAccount.GetStatements_ForTestOnly().Load();
			AssertEquals("BankRecon.BankAccount.Statements should have 3 statements - no filter should be applied", 3, bankRecon.BankAccount.GetStatements_ForTestOnly().Count);
		}

		[TestDate(2018, 12, 04, 12, 00, 00)]
		public void TestEmptyReceiptBatchNoTransactions()
		{
			var bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime.AddMinutes(5);
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, 100.0m, TestBank, Factory, "");
			var receiptCancelled1 = SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, 10.0m, TestBank, Factory, "");
			receiptCancelled1.AH_IsCancelled = true;
			var newReceipt = SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, 5.0m, TestBank, Factory, "");
			newReceipt.AH_TransactionBelongsToGroup = receiptCancelled1.PK;

			Factory.Save();
			AssertEquals("100 + 5", 105m, bankRecon.UnclearedCashbookAmount);

			SetTransaction(LedgerTypes.CashBook, TransactionTypes.DirectReceipt, 300.0m, TestBank, Factory, "");
			var directReceiptCancelled1 = SetTransaction(LedgerTypes.CashBook, TransactionTypes.DirectReceipt, 25.0m, TestBank, Factory, "");
			directReceiptCancelled1.AH_IsCancelled = true;
			var newDirectReceipt = SetTransaction(LedgerTypes.CashBook, TransactionTypes.DirectReceipt, 50.0m, TestBank, Factory, "");
			newDirectReceipt.AH_TransactionBelongsToGroup = directReceiptCancelled1.PK;

			Factory.Save();
			bankRecon.SetUnclearedAmountsFromDB_ForTestOnly(TestBank.PK, bankRecon.ReconcileDate, bankRecon.StatementDate);
			AssertEquals("300 + 50 - 100 - 5", 245m, bankRecon.UnclearedCashbookAmount);

			var receiptCancelled2 = SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, 1.0m, TestBank, Factory, "");
			var directReceiptCancelled2 = SetTransaction(LedgerTypes.CashBook, TransactionTypes.DirectReceipt, 2.0m, TestBank, Factory, "");
			receiptCancelled2.AH_IsCancelled = true;
			directReceiptCancelled2.AH_IsCancelled = true;

			Factory.Save();
			bankRecon.SetUnclearedAmountsFromDB_ForTestOnly(TestBank.PK, bankRecon.ReconcileDate, bankRecon.StatementDate);
			AssertEquals("no change", 245m, bankRecon.UnclearedCashbookAmount);
		}

		[TestDate(2018, 12, 04, 12, 00, 00)]
		public void TestReceiptNotCancelledWithBatchNotCancelled()
		{
			AssertReceiptWithBatch(false, false);
		}

		[TestDate(2018, 12, 04, 12, 00, 00)]
		public void TestReceiptCancelledWithBatchNotCancelled()
		{
			AssertReceiptWithBatch(true, false);
		}

		public void TestGetUndepositedReceiptAmount()
		{
			var testBank1 = TestObjectCreator.USDBankAccount;
			var testBank2 = TestObjectCreator.AUDBankAccount;
			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -700m, -100m, testBank1);
			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -7m, -1m, testBank2);

			Factory.Save();

			var bankRecon1 = new BankReconciliation(Factory);
			bankRecon1.BankAccountPK = testBank1.PK;
			bankRecon1.ReconcileDate = Env.Time.CurrentLocalDateTime.AddMinutes(5);
			bankRecon1.StatementDate = bankRecon1.ReconcileDate;

			AssertEquals(100m, bankRecon1.GetUndepositedReceiptAmount_ForTestOnly(testBank1.PK, ZDateTime.Today));

			var bankRecon2 = new BankReconciliation(Factory);
			bankRecon2.BankAccountPK = testBank2.PK;
			bankRecon2.ReconcileDate = Env.Time.CurrentLocalDateTime.AddMinutes(5);
			bankRecon2.StatementDate = bankRecon2.ReconcileDate;
			AssertEquals(7m, bankRecon2.GetUndepositedReceiptAmount_ForTestOnly(testBank2.PK, ZDateTime.Today));
		}

		public void TestGetLateDepositedReceiptAmount()
		{
			var testBank1 = TestObjectCreator.USDBankAccount;
			var testBank2 = TestObjectCreator.AUDBankAccount;
			SetReceiptAndBatchTransactions(testBank1, LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -700m, -100m, "Test1", "1111");
			SetReceiptAndBatchTransactions(testBank2, LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -7m, -1m, "Test2", "2222");

			var bankRecon1 = new BankReconciliation(Factory);
			bankRecon1.BankAccountPK = testBank1.PK;
			bankRecon1.ReconcileDate = Env.Time.CurrentLocalDateTime.AddMinutes(5);
			bankRecon1.StatementDate = bankRecon1.ReconcileDate;

			AssertEquals(100m, bankRecon1.GetLateDepositedReceiptAmount_ForTestOnly(testBank1.PK, ZDateTime.Today));

			var bankRecon2 = new BankReconciliation(Factory);
			bankRecon2.BankAccountPK = testBank2.PK;
			bankRecon2.ReconcileDate = Env.Time.CurrentLocalDateTime.AddMinutes(5);
			bankRecon2.StatementDate = bankRecon2.ReconcileDate;
			AssertEquals(7m, bankRecon2.GetLateDepositedReceiptAmount_ForTestOnly(testBank2.PK, ZDateTime.Today));
		}

		void SetReceiptAndBatchTransactions(AccBankAccount bank, string receiptLedger, string receiptTransactionType, decimal localAmount, decimal osAmount, string reference, string batchNumber)
		{
			var receipt = SetTransaction(receiptLedger, receiptTransactionType, localAmount, osAmount, bank, batchNumber);
			receipt.AH_ChequeOrReference = reference;
			var batch = SetTransaction(LedgerTypes.CashBook, TransactionTypes.ReceiptBatch, -70.0m, -10m, bank);
			batch.AH_TransactionNum = batchNumber;
			batch.AH_PostDate = ZDateTime.Today.AddDays(2);
			batch.AH_ChequeOrReference = reference;

			Factory.Save();
		}

		AccTransactionHeader SetTransaction(string ledger, string transactionType, decimal localAmount, decimal osAmount, AccBankAccount fBankAccount, string receiptBatchNo = "")
		{
			var transaction = Factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;
			transaction.AH_Ledger = ledger;
			transaction.AH_TransactionType = transactionType;
			transaction.AH_InvoiceAmount = localAmount;
			transaction.AH_OutstandingAmount = localAmount;
			transaction.AH_OSTotal = osAmount;
			transaction.AH_InvoiceDate = ZDateTime.Today;
			transaction.AH_PostDate = ZDateTime.Today;
			transaction.AH_GC = GlbCompany.CurrentCompany.PK;
			transaction.AH_GB = GlbBranch.CurrentBranch.PK;
			transaction.AH_GE = GlbDepartment.CurrentDepartment.PK;
			transaction.AH_TransactionNum = ledger + transactionType + localAmount;
			transaction.AH_AB = fBankAccount.PK;
			transaction.AH_ReceiptBatchNo = receiptBatchNo;

			return transaction;
		}

		void AssertReceiptWithBatch(bool receiptIsCancelled, bool batchIsCancelled)
		{
			var bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime.AddMinutes(5);
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			SetReceiptAndBatchTransactions(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, 100m, receiptIsCancelled, batchIsCancelled);
			AssertEquals("100", 100m, bankRecon.UnclearedCashbookAmount);

			SetReceiptAndBatchTransactions(LedgerTypes.CashBook, TransactionTypes.DirectReceipt, 300m, receiptIsCancelled, batchIsCancelled);
			bankRecon.SetUnclearedAmountsFromDB_ForTestOnly(TestBank.PK, bankRecon.ReconcileDate, bankRecon.StatementDate);
			AssertEquals("300 - 100", 200m, bankRecon.UnclearedCashbookAmount);
		}

		void SetReceiptAndBatchTransactions(string receiptLedger, string receiptTransactionType, decimal amount, bool receiptIsCancelled, bool batchIsCancelled)
		{
			var batchNumber = TestObjectCreator.GetRandomString(5);
			var receipt = SetTransaction(receiptLedger, receiptTransactionType, amount, TestBank, Factory, batchNumber);
			var batch = SetTransaction(LedgerTypes.CashBook, TransactionTypes.ReceiptBatch, 10.0m, TestBank, Factory, "");
			batch.AH_TransactionNum = batchNumber;
			batch.AH_PostDate = Env.Time.CurrentLocalDateTime.AddDays(2);

			if (receiptIsCancelled)
			{
				receipt.AH_IsCancelled = true;
				var newReceipt = SetTransaction(receiptLedger, receiptTransactionType, 666m, TestBank, Factory, "");
				newReceipt.AH_TransactionBelongsToGroup = receipt.PK;
				newReceipt.AH_PostDate = Env.Time.CurrentLocalDateTime.AddDays(2);
			}

			if (batchIsCancelled)
			{
				batch.AH_IsCancelled = true;

				var newBatch = SetTransaction(LedgerTypes.CashBook, TransactionTypes.ReceiptBatch, 20m, TestBank, Factory, "");
				newBatch.AH_TransactionBelongsToGroup = batch.PK;
				newBatch.AH_PostDate = Env.Time.CurrentLocalDateTime.AddDays(2);
			}

			Factory.Save();
		}

		public void TestBankCurrencyDecimals()
		{
			var currency = Factory.New<RefCurrency>();
			currency.RX_Code = "ABC";
			currency.RX_Desc = "TEST";
			currency.RX_SubUnitRatio = 1000;
			Factory.Save();

			TestBank.AB_RX_NKAccountCurrency = currency.RX_Code;
			TestBank.Factory.Save();
			var bankRecon = new BankReconciliation(Factory);
			AssertEquals("should show the default decimal places", 2, bankRecon.BankCurrencyDecimals);

			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;
			bankRecon.ReloadRecords();
			AssertNotNull("PreCondition", bankRecon.BankAccount.AccountCurrency);
			AssertEquals("should be same as the bank currency decimal places", 3, bankRecon.BankCurrencyDecimals);
		}

		public void TestAdditionalTransactions()
		{
			BankReconciliation bankRecon = new BankReconciliation(Factory);
			BankReconDirectPayment payment = Factory.NewWithValidTestData<BankReconDirectPayment>();
			bankRecon.AdditionalTransactions.Headers.Add(payment);
			AssertEquals(1, bankRecon.AdditionalTransactions.Headers.Count);
			bankRecon.ReloadRecords();
			AssertEquals(1, bankRecon.AdditionalTransactions.Headers.Count);
			Factory.Save();
			AssertEquals(0, bankRecon.AdditionalTransactions.Headers.Count);
		}

		[TestDate(2012, 01, 24, 15, 00, 00)]
		public void TestTotalCashbookUnclearedAmount()
		{
			SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 120.0m, TestBank, Factory, "");

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime.AddMinutes(5);
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			bankRecon.FUnclearedCashbookAmountFromDB_ForTestOnly = 0;
			AssertEquals("TotalUnclearedCashbookAmountFromDB must Stay unsetted because it is already Initialized", 0m, bankRecon.UnclearedCashbookAmount);

			bankRecon.FUnclearedCashbookAmountFromDB_ForTestOnly = null;
			AssertEquals("TotalUnclearedCashbookAmountFromDB must be setted because it is not Initialized yet", -120m, bankRecon.UnclearedCashbookAmount);
		}

		[TestDate(2015, 02, 27, 12, 00, 00)]
		public void TestTotalStatementUnclearedAmount()
		{
			SetStatement("DR", "CHQ", 10, "TEST1", TestBank, Factory);

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime.AddMinutes(5);
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			bankRecon.FUnclearedStatementAmountFromDB_ForTestOnly = 0;
			AssertEquals("TotalUnclearedStatementAmountFromDB must Stay unsetted because it is already Initialized", 0m, bankRecon.UnclearedStatementAmount);

			bankRecon.FUnclearedStatementAmountFromDB_ForTestOnly = null;
			AssertEquals("TotalUnclearedStatementAmountFromDB must be setted because it is not Initialized yet", 10m, bankRecon.UnclearedStatementAmount);
		}

		public void TestRecocileAndStatementDatesDontAffectEachOther()
		{
			TestBank.AB_LastReconcileDate = new ZDateTime(2007, 01, 11);
			TestBank.AB_LastStatementDate = new ZDateTime(2007, 01, 10);
			TestBank.Factory.Save();
			AssertEquals("Precondition: AB_LastReconcileDate", new ZDateTime(2007, 01, 11), TestBank.AB_LastReconcileDate);
			AssertEquals("Precondition: AB_LastStatementDate", new ZDateTime(2007, 01, 10), TestBank.AB_LastStatementDate);

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			AssertEquals("ReconcileDate", new ZDateTime(2007, 01, 11), bankRecon.ReconcileDate);
			AssertEquals("StatementDate", new ZDateTime(2007, 01, 10), bankRecon.StatementDate);

			bankRecon.ReconcileDate = new ZDateTime(2007, 01, 23);
			bankRecon.StatementDate = new ZDateTime(2007, 01, 20);
			Factory.Save();
			AssertEquals("ReconcileDate", new ZDateTime(2007, 01, 23), bankRecon.ReconcileDate);
			AssertEquals("StatementDate", new ZDateTime(2007, 01, 20), bankRecon.StatementDate);

			AccBankAccount loadedBank = new BusinessObjectFactory().Load<AccBankAccount>(TestBank.PK);
			AssertEquals("AB_LastReconcileDate", new ZDateTime(2007, 01, 23), loadedBank.AB_LastReconcileDate);
			AssertEquals("AB_LastStatementDate", new ZDateTime(2007, 01, 20), loadedBank.AB_LastStatementDate);
		}

		public void TestSavingBankReconSetsLastRecocileAndLastStatementDatesForValidation()
		{
			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			AssertEquals("Precondition: LastReconcileDate", ZDateTime.Empty, bankRecon.LastReconcileDate_ForTestOnly);
			AssertEquals("Precondition: LastStatementDate", ZDateTime.Empty, bankRecon.LastStatementDate_ForTestOnly);

			bankRecon.ReconcileDate = new ZDateTime(2007, 01, 23);
			bankRecon.StatementDate = new ZDateTime(2007, 01, 22);
			Factory.Save();
			AssertEquals("LastReconcileDate", new ZDateTime(2007, 01, 23), bankRecon.LastReconcileDate_ForTestOnly);
			AssertEquals("LastStatementDate", new ZDateTime(2007, 01, 22), bankRecon.LastStatementDate_ForTestOnly);
		}

		public void TestSettingEditablePropertiesSetsHasChanges()
		{
			BankReconciliation bankRecon = new BankReconciliation(Factory);
			AssertEquals("HasChanges", false, bankRecon.HasChanges);

			bankRecon.BankAccountPK = ZGuid.NewZGuid();
			AssertEquals("HasChanges", true, bankRecon.HasChanges);

			bankRecon.HasChanges = false;
			bankRecon.ReconcileDate = ZDateTime.Now;
			AssertEquals("HasChanges", true, bankRecon.HasChanges);

			bankRecon.HasChanges = false;
			bankRecon.StatementDate = ZDateTime.Now;
			AssertEquals("HasChanges", true, bankRecon.HasChanges);

			bankRecon.HasChanges = false;
			bankRecon.ClosingBalance = 100m;
			AssertEquals("HasChanges", true, bankRecon.HasChanges);
		}

		[SuspendCriticalValidation]
		public void TestAmountFilter()
		{
			SetupTransactions(TestBank, Factory);
			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			bankRecon.AmountFilter = 120;
			bankRecon.MergedTransactions.HasChanges = false;
			bankRecon.ApplyFilter();

			AssertEquals(2, bankRecon.MergedTransactions.Count);
		}

		[SuspendCriticalValidation]
		public void TestLocalAmountFilter()
		{
			AccTransactionHeader payment = SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 1000.0m, 900.0m, 10.0m, TestBank, Factory, "");
			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			bankRecon.AmountFilter = 900;
			bankRecon.ApplyFilter();
			AssertEquals(1, bankRecon.MergedTransactions.Count);

			bankRecon.AmountFilter = -900;
			bankRecon.ApplyFilter();
			AssertEquals(1, bankRecon.MergedTransactions.Count);

			bankRecon.AmountFilter = 0.0m;
			bankRecon.ApplyFilter();
			AssertEquals(1, bankRecon.MergedTransactions.Count);

			bankRecon.AmountFilter = 1010;
			bankRecon.ApplyFilter();
			AssertEquals(1, bankRecon.MergedTransactions.Count);

			bankRecon.AmountFilter = -1010;
			bankRecon.ApplyFilter();
			AssertEquals(1, bankRecon.MergedTransactions.Count);

			bankRecon.AmountFilter = 85;
			bankRecon.ApplyFilter();
			AssertEquals(0, bankRecon.MergedTransactions.Count);

			bankRecon.AmountFilter = -85;
			bankRecon.ApplyFilter();
			AssertEquals(0, bankRecon.MergedTransactions.Count);
		}

		[SuspendCriticalValidation]
		public void TestMethodFilter()
		{
			var directReceipt = TestObjectCreator.CreateDirectReceipt(ZDateTime.Today, 100.00M, 0, 100.00M, 0M);
			directReceipt.AH_AB = TestBank.PK;
			directReceipt.AH_ReceiptType = ReceiptTypes.DirectCredit;
			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			bankRecon.MethodFilter = ReceiptTypes.DirectCredit;
			bankRecon.MergedTransactions.HasChanges = false;
			bankRecon.ApplyFilter();

			AssertEquals(1, bankRecon.MergedTransactions.Count);
			AssertEquals(TransactionTypes.ReceiptBatch, bankRecon.MergedTransactions[0].Type);
			AssertEquals(ReceiptTypes.DirectCredit, bankRecon.MergedTransactions[0].Method);
		}

		[SuspendCriticalValidation]
		public void TestWhetherFilterTriggersCashbookSummary()
		{
			SetupTransactions(TestBank, Factory);
			Factory.Save();

			MockBankReconciliation bankRecon = new MockBankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;
			bankRecon.HasCashbookSummaryRan = false;

			bankRecon.AmountFilter = 120;
			bankRecon.MergedTransactions.HasChanges = false;
			bankRecon.HasCashbookSummaryRan = false;
			bankRecon.ApplyFilter();

			Assert(!bankRecon.HasCashbookSummaryRan);
		}

		[SuspendCriticalValidation]
		public void TestLoad()
		{
			SetupTransactions(TestBank, Factory);
			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			AssertNotNull(bankRecon);
			AssertEquals(6, bankRecon.Transactions_ForTestOnly.Count);
		}

		[SuspendCriticalValidation]
		public void TestTransactionTotal()
		{
			SetupTransactions(TestBank, Factory);
			Factory.Save();

			MockBankReconciliation bankRecon = new MockBankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			AssertEquals(9, bankRecon.MergedTransactions.Count);
			AssertEquals(6, bankRecon.Transactions_ForTestOnly.Count);

			bankRecon.Transactions_ForTestOnly[1].IsCleared = ZBool.True;
			AssertEquals(bankRecon.Transactions_ForTestOnly[1].Debit - bankRecon.Transactions_ForTestOnly[1].Credit, bankRecon.CashbookTotal);
		}

		[SuspendCriticalValidation]
		public void TestStatementTotal()
		{
			SetupTransactions(TestBank, Factory);
			Factory.Save();

			MockBankReconciliation bankRecon = new MockBankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			AssertEquals(3, bankRecon.GetStatements().Count);
			AssertEquals(0.0m, bankRecon.CashbookTotal);

			bankRecon.GetStatements()[0].AS_IsCleared = true;
			AssertEquals(-130.0m, bankRecon.StatementTotal);
		}

		[SuspendCriticalValidation]
		public void TestReconciliationError()
		{
			SetupTransactions(TestBank, Factory);
			Factory.Save();

			MockBankReconciliation bankRecon = new MockBankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			bankRecon.ClosingBalance = 1500m;

			decimal reconError = bankRecon.ClosingBalance - bankRecon.CashBookBalance + bankRecon.UnclearedStatementAmount + bankRecon.UnclearedCashbookAmount;

			AssertEquals(reconError, bankRecon.ReconError);
		}

		[SuspendCriticalValidation]
		[TestDate(2015, 02, 27, 12, 00, 00)]
		public void TestUnclearedTransaction()
		{
			SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 120.0m, TestBank, Factory, "");

			SetTransaction(LedgerTypes.CashBook, TransactionTypes.DirectPayment, -120.0m, TestBank, Factory, "");
			SetTransaction(LedgerTypes.CashBook, TransactionTypes.Transfer, 430.0m, TestBank, Factory, "");

			AccTransactionHeader receiptBatch = SetTransaction(LedgerTypes.CashBook, TransactionTypes.ReceiptBatch, 0.0m, TestBank, Factory, "");
			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -200.0m, TestBank, Factory, receiptBatch.AH_TransactionNum);
			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -150.0m, TestBank, Factory, receiptBatch.AH_TransactionNum);

			SetTransaction(LedgerTypes.CashBook, TransactionTypes.OpeningReceipt, -320.0m, TestBank, Factory, "");
			SetTransaction(LedgerTypes.CashBook, TransactionTypes.OpeningPayment, 500.0m, TestBank, Factory, "");

			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 500.0m, TestBank, Factory, "");

			SetStatement("DR", "CHQ", 10, "TEST1", TestBank, Factory);
			SetStatement("CR", "RCB", 12, "TEST1", TestBank, Factory);

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime.AddMinutes(5);
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			bankRecon.SetUnclearedAmountsFromDB_ForTestOnly(TestBank.PK, bankRecon.ReconcileDate, bankRecon.StatementDate);
			AssertEquals(360m, bankRecon.UnclearedCashbookAmount);
			AssertEquals(-2m, bankRecon.UnclearedStatementAmount);
		}

		[SuspendCriticalValidation]
		public void TestAllFieldsWithTicking()
		{
			SetupTransactions(TestBank, Factory);

			var statement = SetStatement("CR", "CHQ", 15, "TEST1", TestBank, Factory);
			var cashTransaction = SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 120.0m, TestBank, Factory, "");

			Factory.Save();

			MockBankReconciliation bankRecon = new MockBankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;
			bankRecon.ClosingBalance = 1000;

			AssertEquals(-2150m, bankRecon.UnclearedCashbookAmount);
			AssertEquals(105m, bankRecon.UnclearedStatementAmount);
			AssertEquals(-1045m, bankRecon.AmendedBankStatementBalance);
			AssertEquals(-1045m, bankRecon.ReconError);

			AssertEquals(11, bankRecon.MergedTransactions.Count);
			AssertEquals(7, bankRecon.Transactions_ForTestOnly.Count);

			var cashTran = bankRecon.Transactions_ForTestOnly.FindByPK(cashTransaction.PK) as BankReconTransaction;
			cashTran.IsCleared = true;
			var change = cashTran.Debit - cashTran.Credit;

			AssertEquals(-2150m - change, bankRecon.UnclearedCashbookAmount);
			AssertEquals(105m, bankRecon.UnclearedStatementAmount);
			AssertEquals(-1045m - change, bankRecon.AmendedBankStatementBalance);
			AssertEquals(105m, bankRecon.ReconError);

			var statement2 = bankRecon.GetStatements().FindByPK(statement.PK) as Statement;
			statement2.IsCleared = true;
			var change2 = statement2.Debit - statement2.Credit;

			AssertEquals(-2150m - change, bankRecon.UnclearedCashbookAmount);
			AssertEquals(105m + change2, bankRecon.UnclearedStatementAmount);
			AssertEquals(-1045m - change + change2, bankRecon.AmendedBankStatementBalance);
			AssertEquals(120m, bankRecon.ReconError);
		}

		[SuspendCriticalValidation]
		[TestDate(2015, 02, 27, 12, 00, 00)]
		public void TestUnclearedTransaction_WithClearedDateSet()
		{
			SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 120.0m, TestBank, Factory, "");

			SetTransaction(LedgerTypes.CashBook, TransactionTypes.DirectPayment, -120.0m, TestBank, Factory, "");
			SetTransaction(LedgerTypes.CashBook, TransactionTypes.Transfer, 430.0m, TestBank, Factory, "");

			AccTransactionHeader receiptBatch = SetTransaction(LedgerTypes.CashBook, TransactionTypes.ReceiptBatch, 0.0m, TestBank, Factory, "");
			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -200.0m, TestBank, Factory, receiptBatch.AH_TransactionNum);
			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -150.0m, TestBank, Factory, receiptBatch.AH_TransactionNum);

			SetTransaction(LedgerTypes.CashBook, TransactionTypes.OpeningReceipt, -320.0m, TestBank, Factory, "");
			SetTransaction(LedgerTypes.CashBook, TransactionTypes.OpeningPayment, 500.0m, TestBank, Factory, "");

			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 500.0m, TestBank, Factory, "");

			ZDateTime clearedDate = new ZDateTime(2006, 2, 15);

			AccTransactionHeader paymentToExclude = SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 120.0m, TestBank, Factory, "");
			AccTransactionHeader transferToExclude = SetTransaction(LedgerTypes.CashBook, TransactionTypes.Transfer, 430.0m, TestBank, Factory, "");

			paymentToExclude.AH_DateClearedInCashbook = clearedDate;
			transferToExclude.AH_DateClearedInCashbook = clearedDate;

			SetStatement("CR", "CHQ", 15, "TEST1", TestBank, Factory);
			Statement s1 = SetStatement("DR", "CHQ", 10, "TEST1", TestBank, Factory);
			s1.IsCleared = true;

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime.AddMinutes(5);
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			bankRecon.SetUnclearedAmountsFromDB_ForTestOnly(TestBank.PK, bankRecon.ReconcileDate, bankRecon.StatementDate);
			AssertEquals(360m, bankRecon.UnclearedCashbookAmount);
			AssertEquals(-15m, bankRecon.UnclearedStatementAmount);
		}

		[TestDate(2007, 1, 8)]
		[SuspendCriticalValidation]
		public void TestCalculateUnclearedAmount()
		{
			AccTransactionHeader payment = SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 120.0m, TestBank, Factory, "");

			SetTransaction(LedgerTypes.CashBook, TransactionTypes.DirectPayment, -120.0m, TestBank, Factory, "");
			AccTransactionHeader transfer = SetTransaction(LedgerTypes.CashBook, TransactionTypes.Transfer, 430.0m, TestBank, Factory, "");

			AccTransactionHeader receiptBatch = SetTransaction(LedgerTypes.CashBook, TransactionTypes.ReceiptBatch, 0.0m, TestBank, Factory, "");
			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -200.0m, TestBank, Factory, receiptBatch.AH_TransactionNum);
			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -150.0m, TestBank, Factory, receiptBatch.AH_TransactionNum);

			SetTransaction(LedgerTypes.CashBook, TransactionTypes.OpeningReceipt, -320.0m, TestBank, Factory, "");
			SetTransaction(LedgerTypes.CashBook, TransactionTypes.OpeningPayment, 500.0m, TestBank, Factory, "");

			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 500.0m, TestBank, Factory, "");

			//add statements
			SetStatement("DR", "CHQ", 10, "TEST1", TestBank, Factory);
			SetStatement("DR", "RCB", 12, "TEST1", TestBank, Factory);
			SetStatement("DR", "EFT", 10, "TEST1", TestBank, Factory);
			SetStatement("CR", "CCD", 10, "TEST1", TestBank, Factory);
			SetStatement("CR", "DDR", 10, "TEST1", TestBank, Factory);
			SetStatement("CR", "TRF", 11, "TEST1", TestBank, Factory);

			ZDateTime clearedDate = Env.Time.CurrentLocalDateTime.AddMonths(-1);
			payment.AH_DateClearedInCashbook = clearedDate;

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime.AddMinutes(5);
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			AssertEquals(480m, bankRecon.UnclearedCashbookAmount);
			AssertEquals(1m, bankRecon.UnclearedStatementAmount);
			AssertEquals(481m, bankRecon.AmendedBankStatementBalance);
			AssertEquals(481m, bankRecon.ReconError);

			bankRecon.ClearedFilter = true;
			bankRecon.ApplyFilter();

			BankReconTransaction transaction = bankRecon.Transactions_ForTestOnly.FindByPK(payment.PK) as BankReconTransaction;
			transaction.AH_DateClearedInCashbook = ZDateTime.Empty;

			AssertEquals(360m, bankRecon.UnclearedCashbookAmount);
			AssertEquals(1m, bankRecon.UnclearedStatementAmount);
			AssertEquals(361m, bankRecon.AmendedBankStatementBalance);
			AssertEquals(361m, bankRecon.ReconError);

			transaction = bankRecon.Transactions_ForTestOnly.FindByPK(transfer.PK) as BankReconTransaction;
			transaction.AH_DateClearedInCashbook = clearedDate;
			AssertEquals(-70m, bankRecon.UnclearedCashbookAmount);
			AssertEquals(1m, bankRecon.UnclearedStatementAmount);
			AssertEquals(-69m, bankRecon.AmendedBankStatementBalance);
			AssertEquals(-69m, bankRecon.ReconError);
		}

		[TestDate(2007, 1, 8)]
		[SuspendCriticalValidation]
		public void TestAmountsWithAdditionalTransactions()
		{
			AccTransactionHeader payment = SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 120.0m, TestBank, Factory, "");

			SetTransaction(LedgerTypes.CashBook, TransactionTypes.DirectPayment, -120.0m, TestBank, Factory, "");
			AccTransactionHeader transfer = SetTransaction(LedgerTypes.CashBook, TransactionTypes.Transfer, 430.0m, TestBank, Factory, "");

			AccTransactionHeader receiptBatch1 = SetTransaction(LedgerTypes.CashBook, TransactionTypes.ReceiptBatch, 0.0m, TestBank, Factory, "");
			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -200.0m, TestBank, Factory, receiptBatch1.AH_TransactionNum);
			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Receipt, -150.0m, TestBank, Factory, receiptBatch1.AH_TransactionNum);

			SetTransaction(LedgerTypes.CashBook, TransactionTypes.OpeningReceipt, -320.0m, TestBank, Factory, "");
			SetTransaction(LedgerTypes.CashBook, TransactionTypes.OpeningPayment, 500.0m, TestBank, Factory, "");

			SetTransaction(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, 500.0m, TestBank, Factory, "");

			ZDateTime clearedDate = Env.Time.CurrentLocalDateTime.AddMonths(-1);
			payment.AH_DateClearedInCashbook = clearedDate;

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime.AddMinutes(5);
			bankRecon.StatementDate = clearedDate;

			BankReconTransaction directPayment = bankRecon.AdditionalTransactions.BankReconTransactions.Factory.Load<BankReconTransaction>(
				SetTransaction(LedgerTypes.CashBook, TransactionTypes.DirectPayment, 110.0m, TestBank, bankRecon.AdditionalTransactions.BankReconTransactions.Factory, "").PK);
			directPayment.OnLoaded();
			BankReconTransaction receiptBatch = bankRecon.AdditionalTransactions.BankReconTransactions.Factory.Load<BankReconTransaction>(
				SetTransaction(LedgerTypes.CashBook, TransactionTypes.ReceiptBatch, 55.0m, TestBank, bankRecon.AdditionalTransactions.BankReconTransactions.Factory, "").PK);
			receiptBatch.OnLoaded();

			AssertEquals(480m, bankRecon.UnclearedCashbookAmount);
			AssertEquals(0m, bankRecon.CashBookBalance);
			AssertEquals(0m, bankRecon.CashbookTotal);

			bankRecon.AdditionalTransactions.BankReconTransactions.Add(directPayment);
			bankRecon.AdditionalTransactions.BankReconTransactions.Add(receiptBatch);

			AssertEquals(480m, bankRecon.UnclearedCashbookAmount);
			AssertEquals(165m, bankRecon.CashBookBalance);
			AssertEquals(0m, bankRecon.CashbookTotal);

			bankRecon.ClearedFilter = true;
			bankRecon.ApplyFilter();

			AssertEquals("Check count just generate MergedTransactions", 8, bankRecon.MergedTransactions.Count);

			AssertEquals(645m, bankRecon.UnclearedCashbookAmount);
			AssertEquals(705m, bankRecon.CashBookBalance);
			AssertEquals(-120m, bankRecon.CashbookTotal);

			directPayment.IsCleared = true;
			AssertEquals(535m, bankRecon.UnclearedCashbookAmount);
			AssertEquals(705m, bankRecon.CashBookBalance);
			AssertEquals(-10m, bankRecon.CashbookTotal);

			receiptBatch.IsCleared = true;
			AssertEquals(480m, bankRecon.UnclearedCashbookAmount);
			AssertEquals(705m, bankRecon.CashBookBalance);
			AssertEquals(45m, bankRecon.CashbookTotal);

			directPayment.IsCleared = false;
			receiptBatch.IsCleared = false;
			AssertEquals(645m, bankRecon.UnclearedCashbookAmount);
			AssertEquals(705m, bankRecon.CashBookBalance);
			AssertEquals(-120m, bankRecon.CashbookTotal);
		}

		[TestDate(2006, 11, 17, 12, 34, 53)]
		public void TestValidateReconcileDate()
		{
			ZDateTime lastRecincileDate = ZDateTime.Now.AddDays(-1);

			BankStatement bankAccount = Factory.NewWithValidTestData<BankStatement>();
			bankAccount.AB_LastReconcileDate = lastRecincileDate;
			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = bankAccount.PK;

			bankRecon.ReconcileDate = ZDateTime.Now;
			Assert("Today is a valid date", !bankRecon.ReconcileDateInfo.HasErrors());

			bankRecon.ReconcileDate = ZDateTime.Now.AddDays(-1);
			Assert("Yesterday is a valid date", !bankRecon.ReconcileDateInfo.HasErrors());

			bankRecon.ReconcileDate = ZDateTime.Now.AddDays(-2);
			Assert("Any date before the last reconciliation date is not a valid date", bankRecon.ReconcileDateInfo.HasErrors());
			string expectedErrorMessage = "Reconcile date cannot be earlier than the last reconciliation date, which is " + lastRecincileDate.ToShortDateString() + ".";
			Assert("Error Message", bankRecon.ReconcileDateInfo.GetErrors().Contains(expectedErrorMessage));

			bankRecon.ReconcileDate = ZDateTime.Now.AddDays(1);
			Assert("Tomorrow is not a valid date", bankRecon.ReconcileDateInfo.HasErrors());
			expectedErrorMessage = "Reconcile date cannot be later than today.";
			Assert("Error Message", bankRecon.ReconcileDateInfo.GetErrors().Contains(expectedErrorMessage));

			bankRecon.ReconcileDate = ZDateTime.Empty;
			Assert("Empty is not a valid date", bankRecon.ReconcileDateInfo.HasErrors());
			expectedErrorMessage = "Please enter a valid date.";
			Assert("Error Message", bankRecon.ReconcileDateInfo.GetErrors().Contains(expectedErrorMessage));
		}

		[TestDate(2006, 11, 17, 10, 23, 47)]
		public void TestValidateStatementDate()
		{
			ZDateTime lastStatementDate = ZDateTime.Now.AddDays(-1);

			BankStatement bankAccount = Factory.NewWithValidTestData<BankStatement>();
			bankAccount.AB_LastStatementDate = lastStatementDate;
			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = bankAccount.PK;

			bankRecon.StatementDate = ZDateTime.Now;
			Assert("Today is a valid date", !bankRecon.StatementDateInfo.HasErrors());

			bankRecon.StatementDate = ZDateTime.Now.AddDays(-1);
			Assert("Yesterday is a valid date", !bankRecon.StatementDateInfo.HasErrors());

			bankRecon.StatementDate = ZDateTime.Now.AddDays(-2);
			Assert("Any date before the last statement date is not a valid date", bankRecon.StatementDateInfo.HasErrors());
			string expectedErrorMessage = "Statement date cannot be earlier than the last statement date, which is " + lastStatementDate.ToShortDateString() + ".";
			Assert("Error Message", bankRecon.StatementDateInfo.GetErrors().Contains(expectedErrorMessage));

			bankRecon.StatementDate = ZDateTime.Now.AddDays(1);
			Assert("Tomorrow is not a valid date", bankRecon.StatementDateInfo.HasErrors());
			expectedErrorMessage = "Statement date cannot be later than today.";
			Assert("Error Message", bankRecon.StatementDateInfo.GetErrors().Contains(expectedErrorMessage));

			bankRecon.StatementDate = ZDateTime.Empty;
			Assert("Empty is not a valid date", bankRecon.StatementDateInfo.HasErrors());
			expectedErrorMessage = "Please enter a valid date.";
			Assert("Error Message", bankRecon.StatementDateInfo.GetErrors().Contains(expectedErrorMessage));
		}

		public void TestValidateBankAccount()
		{
			var bankAccount = Factory.NewWithValidTestData<BankStatement>();
			var bankAccountForOtherCompany = Factory.NewWithValidTestData<BankStatement>();
			bankAccountForOtherCompany.AB_GC = TestObjectCreator.NonCurrentCompany.PK;

			var cashAccount = TestObjectCreator.CreateBankAccount("TST", "Test Cash Account", TestObjectCreator.AUD, TestObjectCreator.GLHeader1, AccountTypeCodeDescriptionPairList.Codes.CSH);

			Factory.Save();

			var bankRecon = new BankReconciliation(Factory);
			AssertNoError("No error by default", bankRecon.BankAccountPKInfo, "This bank account is not valid for the current company.");

			bankRecon.BankAccountPK = bankAccount.PK;
			AssertNoError("No error for bank account of current company", bankRecon.BankAccountPKInfo, "This bank account is not valid for the current company.");

			bankRecon.BankAccountPK = bankAccountForOtherCompany.PK;
			AssertHasError("Error for bank account of other company", bankRecon.BankAccountPKInfo, "This bank account is not valid for the current company.");

			bankRecon.BankAccountPK = ZGuid.NewZGuid();
			AssertHasError("Error for invalid bank account", bankRecon.BankAccountPKInfo, "This bank account is not valid for the current company.");

			bankRecon.BankAccountPK = cashAccount.PK;
			AssertHasError(bankRecon.BankAccountPKInfo, "The selected Bank Account is a Cash Account. Please select a suitable Bank Account for Bank Reconciliation");
		}

		public void TestPreSaveValidation()
		{
			var bankRecon = new BankReconciliation(Factory);
			AssertNoError("No error by default", bankRecon.BankAccountPKInfo, "This bank account is not valid for the current company.");
			AssertNoError("No error by default", bankRecon.StatementDateInfo, "Please enter a valid date.");
			AssertNoError("No error by default", bankRecon.ReconcileDateInfo, "Please enter a valid date.");

			bankRecon.RunPreSaveValidation();
			AssertHasError("Blank bank account is not valid", bankRecon.BankAccountPKInfo, "This bank account is not valid for the current company.");
			AssertHasError("Blank statement date is not valid", bankRecon.StatementDateInfo, "Please enter a valid date.");
			AssertHasError("Blank reconcile date is not valid", bankRecon.ReconcileDateInfo, "Please enter a valid date.");
		}

		[ExpectNoExceptions()]
		public void TestNotSettingReconcileDateShouldNotCauseAnyException()
		{
			ZDateTime lastStatementDate = ZDateTime.Now;

			BankStatement bankAccount = Factory.NewWithValidTestData<BankStatement>();
			bankAccount.AB_LastStatementDate = lastStatementDate;

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = bankAccount.PK;
			bankRecon.StatementDate = lastStatementDate;
			AssertNull("Statements", bankRecon.GetStatements());
			AssertEquals("Transactions.Count", 0, bankRecon.Transactions_ForTestOnly.Count);
			AssertEquals("MergedTransactions.Count", 0, bankRecon.MergedTransactions.Count);
		}

		[TestDate(2007, 1, 8)]
		public void TestSettingBankSetsReconcileAndStatementDates()
		{
			BankReconciliation bankRecon = new BankReconciliation(Factory);

			BusinessObjectFactory tempFactory = new BusinessObjectFactory();
			BankStatement bankAccount = tempFactory.NewWithValidTestData<BankStatement>();
			bankAccount.AB_LastReconcileDate = ZDateTime.Now.AddDays(-2);
			bankAccount.AB_LastStatementDate = ZDateTime.Now.AddDays(-1);
			tempFactory.Save();

			bankRecon.BankAccountPK = bankAccount.PK;
			AssertEquals("ReconcileDate", bankAccount.AB_LastReconcileDate, bankRecon.ReconcileDate);
			AssertEquals("StatementDate", bankAccount.AB_LastStatementDate, bankRecon.StatementDate);

			bankRecon.BankAccountPK = ZGuid.Empty;
			AssertEquals("ReconcileDate", ZDateTime.Empty, bankRecon.ReconcileDate);
			AssertEquals("StatementDate", ZDateTime.Empty, bankRecon.StatementDate);
		}

		#region TestBankOrDatesGoingToChange

		[TestDate(2006, 11, 28)]
		public void TestBankOrDatesGoingToChange()
		{
			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankOrDatesGoingToChange += new EventHandler(BankRecon_BankOrDatesGoingToChange);

			try
			{
				BankOrDatesGoingToChange_Called = false;

				BusinessObjectFactory tempFactory = new BusinessObjectFactory();
				BankStatement bankAccount = tempFactory.NewWithValidTestData<BankStatement>();
				bankAccount.AB_LastStatementDate = ZDateTime.Now.AddDays(-1);
				bankAccount.AddNewStatement().FillWithValidTestData();
				tempFactory.Save();

				bankRecon.BankAccountPK = bankAccount.PK;
				AssertEquals("BankOrDatesGoingToChange should not be called", false, BankOrDatesGoingToChange_Called);

				bankRecon.ReconcileDate = ZDateTime.Now;
				AssertEquals("BankOrDatesGoingToChange should not be called", false, BankOrDatesGoingToChange_Called);

				bankRecon.StatementDate = ZDateTime.Now;
				AssertEquals("BankOrDatesGoingToChange should not be called", false, BankOrDatesGoingToChange_Called);

				bankRecon.MergedTransactions[0].IsCleared = true;

				bankRecon.BankAccountPK = ZGuid.Empty;
				AssertEquals("BankOrDatesGoingToChange should be called", true, BankOrDatesGoingToChange_Called);

				BankOrDatesGoingToChange_Called = false;
				bankRecon.ReconcileDate = ZDateTime.Empty;
				AssertEquals("BankOrDatesGoingToChange should be called", true, BankOrDatesGoingToChange_Called);

				BankOrDatesGoingToChange_Called = false;
				bankRecon.StatementDate = ZDateTime.Empty;
				AssertEquals("BankOrDatesGoingToChange should be called", true, BankOrDatesGoingToChange_Called);

				bankAccount.GetStatements_ForTestOnly().RemoveAndDeleteAll();
				tempFactory.Save();
				bankRecon.ResetFactoryAndReloadRecords_ForTestOnly();

				BankOrDatesGoingToChange_Called = false;
				bankRecon.BankAccountPK = bankAccount.PK;
				AssertEquals("BankOrDatesGoingToChange should not be called", false, BankOrDatesGoingToChange_Called);

				bankRecon.ReconcileDate = ZDateTime.Now;
				AssertEquals("BankOrDatesGoingToChange should not be called", false, BankOrDatesGoingToChange_Called);

				bankRecon.StatementDate = ZDateTime.Now;
				AssertEquals("BankOrDatesGoingToChange should not be called", false, BankOrDatesGoingToChange_Called);

				bankRecon.AdditionalTransactions.BankReconTransactions.Add(Factory.NewWithValidTestData<BankReconTransaction>());
				bankRecon.ResetCombinedTransactions_ForTestOnly();
				BankOrDatesGoingToChange_Called = false;
				bankRecon.BankAccountPK = ZGuid.Empty;
				AssertEquals("BankOrDatesGoingToChange should be called", true, BankOrDatesGoingToChange_Called);

				bankRecon.AdditionalTransactions.BankReconTransactions.Add(Factory.NewWithValidTestData<BankReconTransaction>());
				bankRecon.ResetCombinedTransactions_ForTestOnly();
				BankOrDatesGoingToChange_Called = false;
				bankRecon.ReconcileDate = ZDateTime.Empty;
				AssertEquals("BankOrDatesGoingToChange should be called", true, BankOrDatesGoingToChange_Called);

				bankRecon.AdditionalTransactions.BankReconTransactions.Add(Factory.NewWithValidTestData<BankReconTransaction>());
				bankRecon.ResetCombinedTransactions_ForTestOnly();
				BankOrDatesGoingToChange_Called = false;
				bankRecon.StatementDate = ZDateTime.Empty;
				AssertEquals("BankOrDatesGoingToChange should be called", true, BankOrDatesGoingToChange_Called);

				BankOrDatesGoingToChange_Called = false;
				bankRecon.BankAccountPK = bankAccount.PK;
				AssertEquals("BankOrDatesGoingToChange should not be called", false, BankOrDatesGoingToChange_Called);

				bankRecon.ReconcileDate = ZDateTime.Now;
				AssertEquals("BankOrDatesGoingToChange should not be called", false, BankOrDatesGoingToChange_Called);

				bankRecon.StatementDate = ZDateTime.Now;
				AssertEquals("BankOrDatesGoingToChange should not be called", false, BankOrDatesGoingToChange_Called);
			}
			finally
			{
				bankRecon.BankOrDatesGoingToChange -= new EventHandler(BankRecon_BankOrDatesGoingToChange);
			}
		}

		void BankRecon_BankOrDatesGoingToChange(object sender, EventArgs e)
		{
			((BankReconciliation.BankReconEventArgs)e).Result = false;
			BankOrDatesGoingToChange_Called = true;
		}

		bool BankOrDatesGoingToChange_Called;

		#endregion

		[SuspendCriticalValidation]
		public void TestDDLLineExcluded()
		{
			SetupTransactions(TestBank, Factory);

			APPayment dDLPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			dDLPayment.AH_ReceiptType = ReceiptTypes.DirectDebitLine;
			dDLPayment.AH_AB = TestBank.PK;

			APPayment dDRPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			dDRPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			dDRPayment.AH_AB = TestBank.PK;

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			AssertNotNull(bankRecon);
			AssertEquals(6, bankRecon.Transactions_ForTestOnly.Count);
			AssertNull(bankRecon.Transactions_ForTestOnly.FindByPK(dDLPayment.PK));
			AssertNull(bankRecon.Transactions_ForTestOnly.FindByPK(dDRPayment.PK));
		}

		[SuspendCriticalValidation]
		public void TestDDBLineIncluded()
		{
			TestBank.AB_ShowDetailsOnDirectDebits = false;
			TestBank.Factory.Save();
			SetupTransactions(TestBank, Factory);

			APPayment dDLPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			dDLPayment.AH_ReceiptType = ReceiptTypes.DirectDebitLine;
			dDLPayment.AH_AB = TestBank.PK;

			APPayment dDRPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			dDRPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			dDRPayment.AH_ReceiptBatchNo = "TESTNO";
			dDRPayment.AH_AB = TestBank.PK;

			DirectDebitBatchHeader dDRBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			dDRBatch.AH_AB = TestBank.PK;

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			AssertNotNull(bankRecon);
			AssertEquals(8, bankRecon.Transactions_ForTestOnly.Count);
			AssertNull(bankRecon.Transactions_ForTestOnly.FindByPK(dDLPayment.PK));
			AssertNotNull(bankRecon.Transactions_ForTestOnly.FindByPK(dDRPayment.PK));
			AssertNotNull(bankRecon.Transactions_ForTestOnly.FindByPK(dDRBatch.PK));
		}

		public void TestWarningForUnbatchedDDRPayments()
		{
			APPayment unbatchedDDRPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			unbatchedDDRPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			unbatchedDDRPayment.AH_AB = TestBank.PK;
			unbatchedDDRPayment.AH_PostDate = Env.Time.CurrentLocalDateTime;
			unbatchedDDRPayment.AH_OSExTaxAmount = 100M;

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			string returnWarningMsg = bankRecon.ValidateBeforeLoad();

			AssertEquals("Bank Reconciliation has found following issues." + System.Environment.NewLine + System.Environment.NewLine + " - There are some un-batched DDR Payments." + System.Environment.NewLine +
				"   You are advised to create direct debit batch for them since un-batched DDR payments do not appear on the Bank Reconciliation.", returnWarningMsg);
		}

		public void TestWarningForLateBatchedDDRPayments()
		{
			DirectDebitBatchHeader dDRBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			dDRBatch.AH_AB = TestBank.PK;
			dDRBatch.AH_PostDate = Env.Time.CurrentLocalDateTime;

			APPayment lateBatchedDDRPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			lateBatchedDDRPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			lateBatchedDDRPayment.AH_AB = TestBank.PK;
			lateBatchedDDRPayment.AH_PostDate = Env.Time.CurrentLocalDateTime.AddDays(-3);
			lateBatchedDDRPayment.AH_OSExTaxAmount = 100M;

			dDRBatch.AH_TransactionNum = "TEST1001";
			lateBatchedDDRPayment.AH_ReceiptBatchNo = dDRBatch.AH_TransactionNum;
			dDRBatch.IsManuallySetTransactionNumber_ForTestOnly = true;
			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime.AddDays(-2);
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			string returnWarningMsg = bankRecon.ValidateBeforeLoad();

			AssertEquals("Bank Reconciliation has found following issues." + System.Environment.NewLine + System.Environment.NewLine +
				" - There are some DDR Payments batched later than current statement date.", returnWarningMsg);
		}

		public void TestNoWarningForBatchedDDRPayments()
		{
			APPayment unbatchedDDRPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			unbatchedDDRPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			unbatchedDDRPayment.AH_AB = TestBank.PK;
			unbatchedDDRPayment.AH_PostDate = Env.Time.CurrentLocalDateTime;
			unbatchedDDRPayment.AH_ReceiptBatchNo = "12345";
			unbatchedDDRPayment.AH_OSExTaxAmount = 100M;

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			string returnWarningMsg = bankRecon.ValidateBeforeLoad();

			AssertEquals("", returnWarningMsg);
		}

		public void TestNoWarningForCancelledUnbatchedDDR()
		{
			APPayment unbatchedDDRPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			unbatchedDDRPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			unbatchedDDRPayment.AH_AB = TestBank.PK;
			unbatchedDDRPayment.AH_PostDate = Env.Time.CurrentLocalDateTime;
			unbatchedDDRPayment.AH_IsCancelled = ZBool.True;
			((IMatching)unbatchedDDRPayment).CurrentMatchGroup.AddNew().AP_AH = unbatchedDDRPayment.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(unbatchedDDRPayment);
			unbatchedDDRPayment.AH_OSExTaxAmount = 100M;

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			string returnWarningMsg = bankRecon.ValidateBeforeLoad();

			AssertEquals("", returnWarningMsg);
		}

		public void TestNoWarningForOpeningPaymentDDR()
		{
			OpeningPayment.OpeningPayment unbatchedDDRPayment = Factory.NewWithValidTestData(typeof(OpeningPayment.OpeningPayment)) as OpeningPayment.OpeningPayment;
			unbatchedDDRPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			unbatchedDDRPayment.AH_AB = TestBank.PK;
			unbatchedDDRPayment.AH_PostDate = Env.Time.CurrentLocalDateTime;
			unbatchedDDRPayment.AH_OSExTaxAmount = 100M;

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			string returnWarningMsg = bankRecon.ValidateBeforeLoad();

			AssertEquals("", returnWarningMsg);
		}

		public void TestNoWarningForUnbatchedDDRinOtherCompany()
		{
			APPayment unbatchedDDRPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			unbatchedDDRPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			unbatchedDDRPayment.AH_AB = TestBank.PK;
			unbatchedDDRPayment.AH_PostDate = Env.Time.CurrentLocalDateTime;
			unbatchedDDRPayment.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			unbatchedDDRPayment.AH_OSExTaxAmount = 100M;

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			string returnWarningMsg = bankRecon.ValidateBeforeLoad();

			AssertEquals("", returnWarningMsg);
		}

		public void TestNonRolledUPDirectBatchHeaderDoesNotShow()
		{
			TestObjectCreator.AUDBankAccount.AB_ShowDetailsOnDirectDebits = false;
			DirectDebitBatchHeader rolledUpDDRBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			rolledUpDDRBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			rolledUpDDRBatch.AH_TransactionNum = "TEST1001";

			APPayment dDLPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			dDLPayment.AH_ReceiptType = ReceiptTypes.DirectDebitLine;
			dDLPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			dDLPayment.AH_ReceiptBatchNo = rolledUpDDRBatch.AH_TransactionNum;
			Factory.Save();

			TestObjectCreator.AUDBankAccount.AB_ShowDetailsOnDirectDebits = true;
			DirectDebitBatchHeader nonRolledUpDDRBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			nonRolledUpDDRBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			nonRolledUpDDRBatch.AH_TransactionNum = "TEST1002";

			APPayment dDRPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			dDRPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			dDRPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			dDRPayment.AH_ReceiptBatchNo = nonRolledUpDDRBatch.AH_TransactionNum;

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestObjectCreator.AUDBankAccount.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			AssertNotNull(bankRecon.MergedTransactions);
			AssertEquals(2, bankRecon.MergedTransactions.Count);

			AssertNull(bankRecon.MergedTransactions.FindByPK(nonRolledUpDDRBatch.PK));
			AssertNull(bankRecon.MergedTransactions.FindByPK(dDLPayment.PK));
			AssertNotNull(bankRecon.MergedTransactions.FindByPK(dDRPayment.PK));
			AssertNotNull(bankRecon.MergedTransactions.FindByPK(rolledUpDDRBatch.PK));
		}

		[ExpectNoExceptions]
		public void TestInvalidFromDateFilterAndToDateFilter()
		{
			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = ZDateTime.Now;
			bankRecon.StatementDate = bankRecon.ReconcileDate;
			bankRecon.MergedTransactions.HasChanges = false;

			bankRecon.DateFilterType = " ";
			bankRecon.FromDateFilter = new ZDateTime(" ");
			bankRecon.ToDateFilter = new ZDateTime(" ");

			bankRecon.GetDateFiltersForTransaction_ForTestOnly();
		}

		public void TestCashbookClearedDateFilter()
		{
			AccBankAccount testBank = TestObjectCreator.AUDBankAccount;
			ZDateTime postDate = ZDateTime.Now;
			ZDateTime clearedDate1 = new ZDateTime(2006, 2, 15);
			ZDateTime clearedDate2 = new ZDateTime(2006, 3, 15);

			APPayment aPPaymentCleared1 = Factory.NewWithValidTestData<APPayment>();
			aPPaymentCleared1.AH_DateClearedInCashbook = clearedDate1;
			aPPaymentCleared1.AH_AB = testBank.PK;

			APPayment aPPaymentCleared2 = Factory.NewWithValidTestData<APPayment>();
			aPPaymentCleared2.AH_DateClearedInCashbook = clearedDate2;
			aPPaymentCleared2.AH_AB = testBank.PK;

			APPayment aPPaymentUncleared = Factory.NewWithValidTestData<APPayment>();
			aPPaymentUncleared.AH_AB = testBank.PK;

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = testBank.PK;
			bankRecon.ReconcileDate = ZDateTime.Now;
			bankRecon.StatementDate = bankRecon.ReconcileDate;
			bankRecon.MergedTransactions.HasChanges = false;

			bankRecon.DateFilterType = BankReconciliation.DATE_IN_STATEMENT_ForTestOnly;
			bankRecon.FromDateFilter = new ZDateTime(2006, 2, 01);
			bankRecon.ToDateFilter = new ZDateTime(2006, 3, 01);
			bankRecon.ClearedFilter = ZBool.True;

			bankRecon.ApplyFilter();
			AssertEquals(1, bankRecon.MergedTransactions.Count);
			AssertEquals(aPPaymentCleared1.PK, ((AccTransactionHeader)bankRecon.MergedTransactions[0]).PK);
		}

		public void TestUnclearedAmountCorrectForClearedRolledupBatches()
		{
			ZDateTime postDate = ZDateTime.Now;

			// Setup Rolled up DDR Batch
			DirectDebitBatchHeader dDRBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			dDRBatch.AH_DateClearedInCashbook = postDate;
			dDRBatch.AH_InvoiceAmount = 120m;
			dDRBatch.AH_OSTotal = 120m;
			dDRBatch.AH_PostDate = postDate;
			dDRBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			dDRBatch.AH_ReceiptType = ReceiptTypes.DirectDebit;

			// Setup Individual Payment
			APPayment aPPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			aPPayment.AH_ReceiptBatchNo = dDRBatch.AH_TransactionNum;
			aPPayment.AH_OSExTaxAmount = 120m;
			aPPayment.AH_PostDate = postDate;
			aPPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebitLine;

			// Setup Other Transactions
			DirectPayment.DirectPayment directPayment = Factory.NewWithValidTestData(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
			directPayment.AH_OSExTaxAmount = 50m;
			directPayment.AH_PostDate = postDate;
			directPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			directPayment.AH_DateClearedInCashbook = postDate;
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			directPayment.Lines.AddNew(directPayment.DependentTransactionLineType);
			directPayment.Lines[0].AL_OSExTaxAmount = 50m;

			APPayment nonClearedAPPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			nonClearedAPPayment.AH_ReceiptBatchNo = dDRBatch.AH_TransactionNum;
			nonClearedAPPayment.AH_OSExTaxAmount = 60m;
			nonClearedAPPayment.AH_PostDate = postDate;
			nonClearedAPPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			nonClearedAPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			APPayment nonClearedAPPayment_NonDDRType = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			nonClearedAPPayment_NonDDRType.AH_ReceiptBatchNo = dDRBatch.AH_TransactionNum;
			nonClearedAPPayment_NonDDRType.AH_OSExTaxAmount = 45m;
			nonClearedAPPayment_NonDDRType.AH_PostDate = postDate;
			nonClearedAPPayment_NonDDRType.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			nonClearedAPPayment_NonDDRType.AH_ReceiptType = ReceiptTypes.Cheque;

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = ZDateTime.Now;
			bankRecon.StatementDate = bankRecon.ReconcileDate;
			bankRecon.SetUnclearedAmountsFromDB_ForTestOnly(TestObjectCreator.AUDBankAccount.PK, postDate, postDate);

			AssertEquals(-105m, bankRecon.UnclearedCashbookAmount);
			AssertEquals(0m, bankRecon.UnclearedStatementAmount);
		}

		public void TestUnclearedAmountWithUnBatchedDDR()
		{
			ZDateTime postDate = ZDateTime.Now;

			var directPayment = Factory.NewWithValidTestData<DirectPayment.DirectPayment>();
			directPayment.AH_OSTotal = -50m;
			directPayment.AH_InvoiceAmount = -50m;
			directPayment.AH_PostDate = postDate.AddDays(-10);
			directPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			directPayment.Lines.AddNew(directPayment.DependentTransactionLineType);
			directPayment.Lines[0].AL_LineAmount = -50m;
			directPayment.Lines[0].AL_OSAmount = -50m;

			var directPaymentNotInBatch = Factory.NewWithValidTestData<DirectPayment.DirectPayment>();
			directPaymentNotInBatch.AH_OSTotal = -30m;
			directPaymentNotInBatch.AH_InvoiceAmount = -30m;
			directPaymentNotInBatch.AH_PostDate = postDate.AddDays(-10);
			directPaymentNotInBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			directPaymentNotInBatch.AH_ReceiptType = ReceiptTypes.DirectDebit;
			directPaymentNotInBatch.Lines.AddNew(directPaymentNotInBatch.DependentTransactionLineType);
			directPaymentNotInBatch.Lines[0].AL_LineAmount = -30m;
			directPaymentNotInBatch.Lines[0].AL_OSAmount = -30m;

			var directPaymentNotInTimeSpan = Factory.NewWithValidTestData<DirectPayment.DirectPayment>();
			directPaymentNotInTimeSpan.AH_OSTotal = -80m;
			directPaymentNotInTimeSpan.AH_InvoiceAmount = -80m;
			directPaymentNotInTimeSpan.AH_PostDate = postDate.AddDays(-2);
			directPaymentNotInTimeSpan.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			directPaymentNotInTimeSpan.AH_ReceiptType = ReceiptTypes.DirectDebit;
			directPaymentNotInTimeSpan.Lines.AddNew(directPaymentNotInTimeSpan.DependentTransactionLineType);
			directPaymentNotInTimeSpan.Lines[0].AL_LineAmount = -80m;
			directPaymentNotInTimeSpan.Lines[0].AL_OSAmount = -80m;

			var nonClearedAPPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			nonClearedAPPayment.AH_OSTotal = 60m;
			nonClearedAPPayment.AH_OSExTaxAmount = 60m;
			nonClearedAPPayment.AH_PostDate = postDate.AddDays(-10);
			nonClearedAPPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			nonClearedAPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			var nonClearedAPPayment_NonDDRType = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			nonClearedAPPayment_NonDDRType.AH_OSTotal = 45m;
			nonClearedAPPayment_NonDDRType.AH_OSExTaxAmount = 45m;
			nonClearedAPPayment_NonDDRType.AH_PostDate = postDate.AddDays(-10);
			nonClearedAPPayment_NonDDRType.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			nonClearedAPPayment_NonDDRType.AH_ReceiptType = ReceiptTypes.Cheque;

			Factory.Save();

			var dDRBatch = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			dDRBatch.AH_TransactionNum = ZString.Empty;
			dDRBatch.AH_PostDate = postDate;
			dDRBatch.AH_ReceiptType = ReceiptTypes.DirectDebit;
			dDRBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;

			Assert(directPayment.IncludeInTheBatch);
			Assert(directPaymentNotInTimeSpan.IncludeInTheBatch);
			Assert(nonClearedAPPayment.IncludeInTheBatch);
			Assert(!nonClearedAPPayment_NonDDRType.IncludeInTheBatch);
			directPaymentNotInBatch.IncludeInTheBatch = false;

			AssertEquals(190m, dDRBatch.AH_InvoiceAmount);
			AssertEquals(190m, dDRBatch.AH_OSTotal);

			Factory.Save();

			var bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = ZDateTime.Now;
			bankRecon.StatementDate = bankRecon.ReconcileDate;
			bankRecon.SetUnclearedAmountsFromDB_ForTestOnly(TestObjectCreator.AUDBankAccount.PK, postDate.AddDays(-5), postDate.AddDays(-5));

			AssertEquals(-185m, bankRecon.UnclearedCashbookAmount);
			AssertEquals(0m, bankRecon.UnclearedStatementAmount);
		}

		[TestDate(2015, 02, 27, 12, 00, 00)]
		public void TestUnclearedAmountWithStatemets()
		{
			SetStatement("DR", "RCB", 120, "TEST1", TestBank, Factory);
			SetStatement("CR", "RCB", 130, "TEST2", TestBank, Factory);

			SetStatement("DR", "CHQ", 140, "TEST1", TestBank, Factory);
			SetStatement("CR", "CHQ", 150, "TEST2", TestBank, Factory);

			SetStatement("DR", "EFT", 160, "TEST1", TestBank, Factory);
			SetStatement("CR", "EFT", 170, "TEST2", TestBank, Factory);

			SetStatement("DR", "CCD", 180, "TEST1", TestBank, Factory);
			SetStatement("CR", "CCD", 190, "TEST2", TestBank, Factory);

			SetStatement("DR", "DDR", 200, "TEST1", TestBank, Factory);
			SetStatement("CR", "DDR", 210, "TEST2", TestBank, Factory);

			SetStatement("DR", "TRF", 220, "TEST1", TestBank, Factory);
			SetStatement("CR", "TRF", 230, "TEST2", TestBank, Factory);

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			//We set any date as if not set both below dates become Invalid and you will not get values from BankRecon.UnclearedCashbookAmount as it only returns it if if (AreKeyFieldsValid)...
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime.AddMinutes(5);
			bankRecon.StatementDate = Env.Time.CurrentLocalDateTime;

			AssertEquals(0m, bankRecon.UnclearedCashbookAmount);
			AssertEquals(-60m, bankRecon.UnclearedStatementAmount);
			AssertEquals(-60m, bankRecon.AmendedBankStatementBalance);
			AssertEquals(-60m, bankRecon.ReconError);
		}

		[TestDate(2015, 02, 27, 12, 00, 00)]
		public void TestUnclearedAmountWithStatemets_AllTypes()
		{
			var statementTypes = new BusinessObjectFactory().New<Statement>().Lookups.AS_Type_List;
			var amountDifference = 14m;
			var random = new Random();
			foreach (CodeDescriptionPair pair in statementTypes)
			{
				var amount = random.Next();
				SetStatement("DR", pair.Code, amount, "TEST1", TestBank, Factory);
				SetStatement("CR", pair.Code, amount + amountDifference, "TEST2", TestBank, Factory);
			}
			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime.AddMinutes(5);
			bankRecon.StatementDate = Env.Time.CurrentLocalDateTime;

			AssertEquals(0m, bankRecon.UnclearedCashbookAmount);
			var expectedAmount = -amountDifference * statementTypes.Count;
			AssertEquals(expectedAmount, bankRecon.UnclearedStatementAmount);
			AssertEquals(expectedAmount, bankRecon.AmendedBankStatementBalance);
			AssertEquals(expectedAmount, bankRecon.ReconError);
		}

		public void TestClearedFitlerSetByDateTypeFilter()
		{
			BankReconciliation bankRecon = new BankReconciliation(Factory);
			Assert(!bankRecon.ClearedFilter);

			bankRecon.DateFilterType = BankReconciliation.DATE_IN_STATEMENT_ForTestOnly;
			Assert(bankRecon.ClearedFilter);
		}

		public void TestMethod_List()
		{
			BankReconciliation bankRecon = new BankReconciliation(Factory);
			AssertNotNull("Method_List", bankRecon.Method_List);
			AssertEquals("Method_List.Count", 20, bankRecon.Method_List.Count);
		}

		public void TestAdditionalTransactionsBizObj()
		{
			BankStatement bankStatement = Factory.New<BankStatement>();
			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = bankStatement.PK;
			bankRecon.StatementDate = new ZDateTime(2006, 10, 11);
			AssertNotNull("DirectTransactions", bankRecon.AdditionalTransactions);
			AssertEquals("AdditionalTransactions.StatementDate", bankRecon.StatementDate, bankRecon.AdditionalTransactions.StatementDate);
		}

		public void TestCashBookAmountWtithNextDayTransactions()
		{
			ZDateTime postDate = ZDateTime.Now.Date.AddDays(-5).AddHours(10);

			APPayment aPPaymentInTimeSpan = Factory.NewWithValidTestData<APPayment>();
			aPPaymentInTimeSpan.AH_OSExTaxAmount = 45m;
			aPPaymentInTimeSpan.AH_PostDate = postDate;
			aPPaymentInTimeSpan.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			aPPaymentInTimeSpan.AH_ReceiptType = ReceiptTypes.Cheque;

			APPayment aPPaymentNotInTimeSpan = Factory.NewWithValidTestData<APPayment>();
			aPPaymentNotInTimeSpan.AH_OSExTaxAmount = 100m;
			aPPaymentNotInTimeSpan.AH_PostDate = postDate.AddDays(1);
			aPPaymentNotInTimeSpan.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			aPPaymentNotInTimeSpan.AH_ReceiptType = ReceiptTypes.Cheque;

			ARPayment aRPaymentNotInTimeSpan = Factory.NewWithValidTestData<ARPayment>();
			aRPaymentNotInTimeSpan.AH_OSExTaxAmount = 200m;
			aRPaymentNotInTimeSpan.AH_PostDate = postDate.AddHours(23);
			aRPaymentNotInTimeSpan.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			aRPaymentNotInTimeSpan.AH_ReceiptType = ReceiptTypes.Cheque;

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = TestObjectCreator.AUDBankAccount.PK;
			bankRecon.ReconcileDate = postDate;
			bankRecon.StatementDate = postDate;
			AssertEquals(0m, bankRecon.CashBookBalance);
			BusinessObjectCollection dummyCollection = bankRecon.MergedTransactions;
			AssertEquals(-45m, bankRecon.CashBookBalance);
			bankRecon.ReloadRecords();
			AssertEquals(-45m, bankRecon.CashBookBalance);
		}

		[SuspendCriticalValidation]
		public void TestForeignCurrencyTransactionsWithLocalCurrencyBankAccount()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_RX_NKTransactionCurrency = "USD";
			invoice.AH_OSTotal = -100m;
			invoice.AH_InvoiceAmount = -45.45m;

			APPayment payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			payment.AH_RX_NKTransactionCurrency = "USD";
			payment.AH_ReceiptType = ReceiptTypes.eNettDirectDebit;
			payment.AH_OSTotal = 100m;
			payment.AH_InvoiceAmount = 45.45m;
			payment.AH_PostDate = payment.AH_FullyPaidDate = ZDateTime.Now.Date;

			TransactionMatchLinkGroup matchLinkGroup = new TransactionMatchLinkGroup(Factory);
			string matchGroupNumber = TestObjectCreator.GetRandomString(5);
			TransactionMatchLink matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = payment.PK;
			matchLink.AP_Amount = 45.45m;
			matchLink.AP_MatchGroupNum = matchGroupNumber;
			matchLinkGroup.Add(matchLink);
			matchLink = Factory.New<TransactionMatchLink>();
			matchLink.AP_AH = invoice.PK;
			matchLink.AP_Amount = -45.45m;
			matchLink.AP_MatchGroupNum = matchGroupNumber;
			matchLinkGroup.Add(matchLink);
			TestObjectCreator.SetupMatchLinkMatchDate(matchLinkGroup);

			Factory.Save();

			BankReconciliation bankReconciliation = new BankReconciliation(Factory) { BankAccountPK = TestObjectCreator.AUDBankAccount.PK, ReconcileDate = ZDateTime.Now.Date, StatementDate = ZDateTime.Now.Date };
			BusinessObjectCollection dummyCollection = bankReconciliation.MergedTransactions;
			AssertEquals(-45.45m, bankReconciliation.CashBookBalance);
		}

		public void TestApplyFilterWithChanges()
		{
			TestObjectCreator.AUDBankAccount.Factory.Save();

			BankReconciliation bankReconciliation = new BankReconciliation(Factory) { BankAccountPK = TestObjectCreator.AUDBankAccount.PK, ReconcileDate = ZDateTime.Today, StatementDate = ZDateTime.Today };
			bankReconciliation.ResetMergedTransactions_ForTestOnly();
			bankReconciliation.Transactions_ForTestOnly.Add(Factory.NewWithValidTestData<BankReconTransaction>());
			AssertEquals("MergedTransactions.Count", 1, bankReconciliation.MergedTransactions.Count);
			AssertEquals("bankReconciliation.CanReloadTransactions", false, bankReconciliation.CanReloadTransactions);
			AssertEquals("ApplyFilter", "Please save your changes before applying filter.", bankReconciliation.ApplyFilter());

			bankReconciliation.ResetMergedTransactions_ForTestOnly();
			bankReconciliation.GetStatements().Add(Factory.NewWithValidTestData<Statement>());
			AssertEquals("MergedTransactions.Count", 1, bankReconciliation.MergedTransactions.Count);
			AssertEquals("bankReconciliation.CanReloadTransactions", false, bankReconciliation.CanReloadTransactions);
			AssertEquals("ApplyFilter", "Please save your changes before applying filter.", bankReconciliation.ApplyFilter());

			bankReconciliation.ResetMergedTransactions_ForTestOnly();
			bankReconciliation.AdditionalTransactions.BankReconTransactions.Add(Factory.NewWithValidTestData<BankReconTransaction>());
			AssertEquals("MergedTransactions.Count", 1, bankReconciliation.MergedTransactions.Count);
			AssertEquals("bankReconciliation.CanReloadTransactions", true, bankReconciliation.CanReloadTransactions);
			AssertEquals("ApplyFilter", "", bankReconciliation.ApplyFilter());
		}

		public void TestAdditionalTransactionsInCollections()
		{
			BankReconciliation bankReconciliation = new BankReconciliation(Factory) { BankAccountPK = TestObjectCreator.AUDBankAccount.PK, ReconcileDate = ZDateTime.Today, StatementDate = ZDateTime.Today };
			bankReconciliation.Transactions_ForTestOnly.Add(Factory.New<BankReconTransaction>());
			AssertEquals("CombinedTransactions.Count", 1, bankReconciliation.CombinedTransactions.Count);
			AssertEquals("MergedTransactions.Count", 1, bankReconciliation.MergedTransactions.Count);

			bankReconciliation.Transactions_ForTestOnly.RemoveAll();
			bankReconciliation.ResetTransactions_ForTestOnly();
			AssertEquals("CombinedTransactions.Count", 0, bankReconciliation.CombinedTransactions.Count);
			AssertEquals("MergedTransactions.Count", 0, bankReconciliation.MergedTransactions.Count);

			bankReconciliation.AdditionalTransactions.BankReconTransactions.Add(Factory.New<BankReconTransaction>());
			AssertEquals("CombinedTransactions.Count", 0, bankReconciliation.CombinedTransactions.Count);
			AssertEquals("MergedTransactions.Count", 0, bankReconciliation.MergedTransactions.Count);

			bankReconciliation.ResetCombinedTransactions_ForTestOnly();
			AssertEquals("CombinedTransactions.Count", 1, bankReconciliation.CombinedTransactions.Count);
			AssertEquals("MergedTransactions.Count", 1, bankReconciliation.MergedTransactions.Count);

			bankReconciliation.Transactions_ForTestOnly.Add(Factory.New<BankReconTransaction>());
			AssertEquals("CombinedTransactions.Count", 1, bankReconciliation.CombinedTransactions.Count);
			AssertEquals("MergedTransactions.Count", 1, bankReconciliation.MergedTransactions.Count);

			bankReconciliation.ResetCombinedTransactions_ForTestOnly();
			AssertEquals("CombinedTransactions.Count", 2, bankReconciliation.CombinedTransactions.Count);
			AssertEquals("MergedTransactions.Count", 2, bankReconciliation.MergedTransactions.Count);
		}

		public void TestKeyFieldChangesResetFactory()
		{
			BankReconciliation bankReconciliation = new BankReconciliation(Factory);
			BusinessObjectFactory transactionFactory = bankReconciliation.TransactionsFactory_innerValue_ForTestOnly;
			AssertNotNull("TransactionFactory must be initialized from a begining.", bankReconciliation.TransactionsFactory_innerValue_ForTestOnly);
			AssertNotEquals("New TransactionsFactory must be created", bankReconciliation.Factory, bankReconciliation.TransactionsFactory_innerValue_ForTestOnly);

			bankReconciliation.BankAccountPK = TestBank.PK;

			bankReconciliation.Transactions_ForTestOnly.Add(Factory.New<BankReconTransaction>());
			bankReconciliation.AdditionalTransactions.BankReconTransactions.Add(Factory.New<BankReconTransaction>());
			bankReconciliation.GetStatements().Add(Factory.NewWithValidTestData<Statement>());
			AssertEquals("Precondition: CombinedTransactions.Count", 2, bankReconciliation.CombinedTransactions.Count);
			AssertEquals("Precondition: MergedTransactions.Count", 3, bankReconciliation.MergedTransactions.Count);
			AssertEquals("bankReconciliation.AdditionalTransactions.ReconciliationBankAccount", bankReconciliation.BankAccountPK, bankReconciliation.AdditionalTransactions.ReconciliationBankAccountPK);

			bankReconciliation.BankAccountPK = ZGuid.Empty;
			AssertEquals("bankReconciliation.AdditionalTransactions.ReconciliationBankAccount", bankReconciliation.BankAccountPK, bankReconciliation.AdditionalTransactions.ReconciliationBankAccountPK);
			bankReconciliation.BankAccountPK = TestBank.PK;
			AssertEquals("bankReconciliation.AdditionalTransactions.ReconciliationBankAccount", bankReconciliation.BankAccountPK, bankReconciliation.AdditionalTransactions.ReconciliationBankAccountPK);

			AssertNotNull("TransactionFactory must be initialized.", bankReconciliation.TransactionsFactory_innerValue_ForTestOnly);
			AssertNotEquals("New TransactionsFactory must be created", transactionFactory, bankReconciliation.TransactionsFactory_innerValue_ForTestOnly);
			transactionFactory = bankReconciliation.TransactionsFactory_innerValue_ForTestOnly;
			AssertEquals("CombinedTransactions.Count", 0, bankReconciliation.CombinedTransactions.Count);
			AssertEquals("MergedTransactions.Count", 0, bankReconciliation.MergedTransactions.Count);
			AssertEquals("BankAccount Factory must be TransactionsFactory to prevent saving statement changes after changing key fields for Reconciliation.", bankReconciliation.TransactionsFactory_ForTestOnly, bankReconciliation.BankAccount.Factory);
			AssertEquals("Transactions Factory must be TransactionsFactory to prevent saving transaction changes after changing key fields for Reconciliation.", bankReconciliation.TransactionsFactory_ForTestOnly, bankReconciliation.Transactions_ForTestOnly.Factory);
			AssertEquals("Statements Factory must be TransactionsFactory to prevent saving statement changes after changing key fields for Reconciliation.", bankReconciliation.TransactionsFactory_ForTestOnly, bankReconciliation.GetStatements().Factory);
			AssertEquals("AdditionalTransactions Factory must be TransactionsFactory to prevent saving additional transaction changes after changing key fields for Reconciliation.", bankReconciliation.TransactionsFactory_ForTestOnly, bankReconciliation.AdditionalTransactions.Factory);

			bankReconciliation.ReconcileDate = ZDateTime.Now.AddDays(-1);
			bankReconciliation.Transactions_ForTestOnly.Add(Factory.New<BankReconTransaction>());
			bankReconciliation.AdditionalTransactions.BankReconTransactions.Add(Factory.New<BankReconTransaction>());
			bankReconciliation.GetStatements().Add(Factory.NewWithValidTestData<Statement>());
			AssertEquals("Precondition: CombinedTransactions.Count", 2, bankReconciliation.CombinedTransactions.Count);
			AssertEquals("Precondition: MergedTransactions.Count", 3, bankReconciliation.MergedTransactions.Count);

			bankReconciliation.ReconcileDate = ZDateTime.Now.AddDays(-2);
			AssertNotNull("TransactionFactory must be initialized.", bankReconciliation.TransactionsFactory_innerValue_ForTestOnly);
			AssertNotEquals("New TransactionsFactory must be created", transactionFactory, bankReconciliation.TransactionsFactory_innerValue_ForTestOnly);
			transactionFactory = bankReconciliation.TransactionsFactory_innerValue_ForTestOnly;
			AssertEquals("CombinedTransactions.Count", 0, bankReconciliation.CombinedTransactions.Count);
			AssertEquals("MergedTransactions.Count", 0, bankReconciliation.MergedTransactions.Count);
			AssertEquals("BankAccount Factory must be TransactionsFactory to prevent saving statement changes after changing key fields for Reconciliation.", bankReconciliation.TransactionsFactory_ForTestOnly, bankReconciliation.BankAccount.Factory);
			AssertEquals("Transactions Factory must be TransactionsFactory to prevent saving transaction changes after changing key fields for Reconciliation.", bankReconciliation.TransactionsFactory_ForTestOnly, bankReconciliation.Transactions_ForTestOnly.Factory);
			AssertEquals("Statements Factory must be TransactionsFactory to prevent saving statement changes after changing key fields for Reconciliation.", bankReconciliation.TransactionsFactory_ForTestOnly, bankReconciliation.GetStatements().Factory);
			AssertEquals("AdditionalTransactions Factory must be TransactionsFactory to prevent saving additional transaction changes after changing key fields for Reconciliation.", bankReconciliation.TransactionsFactory_ForTestOnly, bankReconciliation.AdditionalTransactions.Factory);

			bankReconciliation.StatementDate = ZDateTime.Now.AddDays(-3);
			bankReconciliation.ResetMergedTransactions_ForTestOnly();
			bankReconciliation.Transactions_ForTestOnly.Add(Factory.New<BankReconTransaction>());
			bankReconciliation.AdditionalTransactions.BankReconTransactions.Add(Factory.New<BankReconTransaction>());
			bankReconciliation.GetStatements().Add(Factory.NewWithValidTestData<Statement>());
			AssertEquals("Precondition: CombinedTransactions.Count", 2, bankReconciliation.CombinedTransactions.Count);
			AssertEquals("Precondition: MergedTransactions.Count", 3, bankReconciliation.MergedTransactions.Count);
			AssertEquals("bankReconciliation.AdditionalTransactions.StatementDate", bankReconciliation.StatementDate, bankReconciliation.AdditionalTransactions.StatementDate);

			bankReconciliation.StatementDate = ZDateTime.Now.AddDays(-4);
			AssertNotNull("TransactionFactory must be initialized.", bankReconciliation.TransactionsFactory_innerValue_ForTestOnly);
			AssertNotEquals("New TransactionsFactory must be created", transactionFactory, bankReconciliation.TransactionsFactory_innerValue_ForTestOnly);
			transactionFactory = bankReconciliation.TransactionsFactory_innerValue_ForTestOnly;
			AssertEquals("CombinedTransactions.Count", 0, bankReconciliation.CombinedTransactions.Count);
			AssertEquals("MergedTransactions.Count", 0, bankReconciliation.MergedTransactions.Count);
			AssertEquals("BankAccount Factory must be TransactionsFactory to prevent saving statement changes after changing key fields for Reconciliation.", bankReconciliation.TransactionsFactory_ForTestOnly, bankReconciliation.BankAccount.Factory);
			AssertEquals("Transactions Factory must be TransactionsFactory to prevent saving transaction changes after changing key fields for Reconciliation.", bankReconciliation.TransactionsFactory_ForTestOnly, bankReconciliation.Transactions_ForTestOnly.Factory);
			AssertEquals("Statements Factory must be TransactionsFactory to prevent saving statement changes after changing key fields for Reconciliation.", bankReconciliation.TransactionsFactory_ForTestOnly, bankReconciliation.GetStatements().Factory);
			AssertEquals("AdditionalTransactions Factory must be TransactionsFactory to prevent saving additional transaction changes after changing key fields for Reconciliation.", bankReconciliation.TransactionsFactory_ForTestOnly, bankReconciliation.AdditionalTransactions.Factory);
			AssertEquals("bankReconciliation.AdditionalTransactions.StatementDate", bankReconciliation.StatementDate, bankReconciliation.AdditionalTransactions.StatementDate);
		}

		public void TestFactorySaveWithAdditionalTransactions()
		{
			BankReconciliation bankReconciliation = new BankReconciliation(Factory) { BankAccountPK = TestBank.PK, ReconcileDate = ZDateTime.Today, StatementDate = ZDateTime.Today };
			bankReconciliation.ResetMergedTransactions_ForTestOnly();

			AccTransactionHeader directPayment = bankReconciliation.TransactionsFactory_ForTestOnly.Load<BankReconTransaction>(
				SetTransaction(LedgerTypes.CashBook, TransactionTypes.DirectPayment, -120.0m, TestBank, bankReconciliation.TransactionsFactory_ForTestOnly, "").PK);
			directPayment.OnLoaded();
			AccTransactionHeader receiptBatch = bankReconciliation.TransactionsFactory_ForTestOnly.Load<BankReconTransaction>(
				SetTransaction(LedgerTypes.CashBook, TransactionTypes.ReceiptBatch, 200.0m, TestBank, bankReconciliation.TransactionsFactory_ForTestOnly, "").PK);
			receiptBatch.OnLoaded();
			SetTransaction(LedgerTypes.CashBook, TransactionTypes.DirectReceipt, 200.0m, TestBank, bankReconciliation.TransactionsFactory_ForTestOnly, receiptBatch.AH_TransactionNum);

			bankReconciliation.AdditionalTransactions.BankReconTransactions.Add(directPayment);
			bankReconciliation.AdditionalTransactions.BankReconTransactions.Add(receiptBatch);

			AssertEquals("bankReconciliation.AdditionalTransactions must be cleared.", 2, bankReconciliation.AdditionalTransactions.BankReconTransactions.Count);
			AssertEquals("bankReconciliation.AdditionalTransactions must be cleared.", 0, bankReconciliation.Transactions_ForTestOnly.Count);
			ZDecimal unclearedCashbookAmountBeforeSaving = bankReconciliation.UnclearedCashbookAmount;
			AssertEquals("bankReconciliation.UnclearedAmount must be calculated.", 80M, unclearedCashbookAmountBeforeSaving);
			AssertEquals("bankReconciliation.TotalUnclearedAmount must be zero.", 0M, bankReconciliation.UnclearedCashbookAmountFromDB_ForTestOnly);

			ZDecimal unclearedStatementAmountBeforeSaving = bankReconciliation.UnclearedStatementAmount;
			AssertEquals("bankReconciliation.UnclearedAmount must be calculated.", 0M, unclearedStatementAmountBeforeSaving);
			AssertEquals("bankReconciliation.TotalUnclearedAmount must be zero.", 0M, bankReconciliation.UnclearedStatementAmountFromDB_ForTestOnly);

			bankReconciliation.Factory.Save();

			AssertEquals("bankReconciliation.AdditionalTransactions must be cleared.", 0, bankReconciliation.AdditionalTransactions.BankReconTransactions.Count);
			AssertEquals("bankReconciliation.AdditionalTransactions must be cleared.", 2, bankReconciliation.Transactions_ForTestOnly.Count);
			AssertEquals("bankReconciliation.UnclearedCashbookAmount must be equal UnclearedCashbookAmount before saving.", unclearedCashbookAmountBeforeSaving, bankReconciliation.UnclearedCashbookAmount);
			AssertEquals("bankReconciliation.UnclearedCashbookAmount must be equal UnclearedCashbookAmountFromDB before saving.", unclearedCashbookAmountBeforeSaving, bankReconciliation.UnclearedCashbookAmountFromDB_ForTestOnly);

			AssertEquals("bankReconciliation.UnclearedStatementAmount must be equal UnclearedStatementAmount before saving.", unclearedStatementAmountBeforeSaving, bankReconciliation.UnclearedStatementAmount);
			AssertEquals("bankReconciliation.UnclearedStatementAmountFromDB must be equal UnclearedStatementAmount before saving.", unclearedStatementAmountBeforeSaving, bankReconciliation.UnclearedStatementAmountFromDB_ForTestOnly);
		}

		[SuspendCriticalValidation]
		[ExpectNoExceptions]
		public void TestRunReportAndAttachEDocWhenReportHasRenderException ()
		{
			SetupTransactions(TestBank, Factory);

			var statement = SetStatement("CR", "CHQ", 15, "TEST1", TestBank, Factory);
			var cashTransaction = SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 120.0m, TestBank, Factory, "");

			Factory.Save();

			var bankRecon = (BankReconciliation)GetNewBusinessObject();
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDate;
			bankRecon.StatementDate = bankRecon.ReconcileDate;
			bankRecon.ClosingBalance = 1000;

			var docManagerInfo = bankRecon.BankAccount.DocManagerInfo();
			AssertEquals("Precondition: no eDocs attached", 0, docManagerInfo.Files.Count);

			var cashTran = bankRecon.Transactions_ForTestOnly.FindByPK(cashTransaction.PK) as BankReconTransaction;
			cashTran.IsCleared = true;
			bankRecon.BankAccountPK = ZGuid.Empty;

			using (var report = new MockReportWithRenderException())
			{
				bankRecon.RunReportAndAttachEDoc(report, docManagerInfo, "filename.txt");
			}
			AssertEquals("Postcondition: no eDocs attached", 0, docManagerInfo.Files.Count);
		}

		#region Bank Reconciliation History (eDoc and snapshot)
		[SuspendCriticalValidation]
		public void TestFactorySave_AttachesEDocToBankAccount()
		{
			SetupTransactions(TestBank, Factory);

			var statement = SetStatement("CR", "CHQ", 15, "TEST1", TestBank, Factory);
			var cashTransaction = SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 120.0m, TestBank, Factory, "");

			Factory.Save();

			var bankRecon = (BankReconciliation)GetNewBusinessObject();
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDate;
			bankRecon.StatementDate = bankRecon.ReconcileDate;
			bankRecon.ClosingBalance = 1000;

			var docManagerInfo = bankRecon.BankAccount.DocManagerInfo();
			AssertEquals("Precondition: no eDocs attached", 0, docManagerInfo.Files.Count);

			var cashTran = bankRecon.Transactions_ForTestOnly.FindByPK(cashTransaction.PK) as BankReconTransaction;
			cashTran.IsCleared = true;

			bankRecon.AttachHistoryEDocForCurrentSession();     // EDoc must be created before BusinessObjectFactory.Save().
			Factory.Save();

			docManagerInfo = bankRecon.BankAccount.DocManagerInfo();
			AssertEquals("After Save(), one eDoc should be attached", 1, docManagerInfo.Files.Count);
			AssertAttachedEDocs(docManagerInfo, "Bank Reconciliation", "BRC");
			Assert("Assumption: The Bank Reconciliation eDoc should match the content of BankReconciliation", true);

			var statement2 = bankRecon.GetStatements().FindByPK(statement.PK) as Statement;
			statement2.IsCleared = true;

			bankRecon.AttachHistoryEDocForCurrentSession();
			Factory.Save();

			docManagerInfo = bankRecon.BankAccount.DocManagerInfo();
			AssertEquals("After next Save(), two eDocs should be attached", 2, docManagerInfo.Files.Count);
			AssertAttachedEDocs(docManagerInfo, "Bank Reconciliation", "BRC");

			bankRecon.AttachHistoryEDocForCurrentSession();
			Factory.Save();

			docManagerInfo = bankRecon.BankAccount.DocManagerInfo();
			AssertEquals("After Save() with no changes, no more eDocs should be attached", 2, docManagerInfo.Files.Count);
		}

		[SuspendCriticalValidation]
		[TestDate(2020, 1, 17, 13, 12, 32)]
		public void TestFactorySave_AttachedEDocsHaveUniqueNames()
		{
			SetupTransactions(TestBank, Factory);

			var statement = SetStatement("CR", "CHQ", 15, "TEST1", TestBank, Factory);
			var cashTransaction = SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 120.0m, TestBank, Factory, "");

			Factory.Save();

			var bankRecon = (BankReconciliation)GetNewBusinessObject();
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDate;
			bankRecon.StatementDate = bankRecon.ReconcileDate;
			bankRecon.ClosingBalance = 1000;

			var docManagerInfo = bankRecon.BankAccount.DocManagerInfo();
			AssertEquals("Precondition: no eDocs attached", 0, docManagerInfo.Files.Count);

			var cashTran = bankRecon.Transactions_ForTestOnly.FindByPK(cashTransaction.PK) as BankReconTransaction;
			cashTran.IsCleared = true;

			bankRecon.AttachHistoryEDocForCurrentSession();      // EDoc must be created before BusinessObjectFactory.Save().
			Factory.Save();

			docManagerInfo = bankRecon.BankAccount.DocManagerInfo();
			AssertEquals("After Save(), one eDoc should be attached", 1, docManagerInfo.Files.Count);
			AssertAttachedEDocs(docManagerInfo, "Bank Reconciliation", "BRC");
			AssertEDocsFilename(docManagerInfo, 0, "Bank Reconciliation 20200117_131232.xlsx");

			var statement2 = bankRecon.GetStatements().FindByPK(statement.PK) as Statement;
			statement2.IsCleared = true;

			bankRecon.AttachHistoryEDocForCurrentSession();
			Factory.Save();

			docManagerInfo = bankRecon.BankAccount.DocManagerInfo();
			AssertEquals("After next Save(), two eDocs should be attached", 2, docManagerInfo.Files.Count);
			AssertAttachedEDocs(docManagerInfo, "Bank Reconciliation", "BRC");
			AssertEDocsFilename(docManagerInfo, 1, "Bank Reconciliation 20200117_131232_1.xlsx");
			AssertNoDuplicateEDocsFilenames(docManagerInfo);
		}

		[SuspendCriticalValidation]
		public void TestSetBankAccount_UpdatesSnapshotObject()
		{
			SetupTransactions(TestBank, Factory);
			var bankAccount2 = CreateBank("B2", "XYZZY", "1060.10.20");
			var cashTransaction = SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 120.0m, TestBank, Factory, "");

			Factory.Save();

			var bankRecon = (BankReconciliation)GetNewBusinessObject();
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDate;
			bankRecon.StatementDate = bankRecon.ReconcileDate;
			bankRecon.ClosingBalance = 1000;

			CombineAssertions("OpeningSnapshot should reflect initial state.", () =>
			{
				AssertEquals(0m, bankRecon.OpeningSnapshot.StatementBalance);
				AssertEquals(-2150m, bankRecon.OpeningSnapshot.UnclearedCashbookAmount);
				AssertEquals(120m, bankRecon.OpeningSnapshot.UnclearedStatementAmount);
				AssertEquals(-2030m, bankRecon.OpeningSnapshot.AmendedBankStatementBalance);
				AssertEquals(0m, bankRecon.OpeningSnapshot.CashBookBalance);
				AssertEquals(-2030m, bankRecon.OpeningSnapshot.ReconError);
			});

			var cashTran = bankRecon.Transactions_ForTestOnly.FindByPK(cashTransaction.PK) as BankReconTransaction;
			cashTran.IsCleared = true;

			CombineAssertions("OpeningSnapshot should not change when transactions are cleared.", () =>
			{
				AssertEquals(0m, bankRecon.OpeningSnapshot.StatementBalance);
				AssertEquals(-2150m, bankRecon.OpeningSnapshot.UnclearedCashbookAmount);
				AssertEquals(120m, bankRecon.OpeningSnapshot.UnclearedStatementAmount);
				AssertEquals(-2030m, bankRecon.OpeningSnapshot.AmendedBankStatementBalance);
				AssertEquals(0m, bankRecon.OpeningSnapshot.CashBookBalance);
				AssertEquals(-2030m, bankRecon.OpeningSnapshot.ReconError);
			});

			bankRecon.ClosingBalance = 1200;
			CombineAssertions("OpeningSnapshot should not change when closing balance is set.", () =>
			{
				AssertEquals(0m, bankRecon.OpeningSnapshot.StatementBalance);
				AssertEquals(-2150m, bankRecon.OpeningSnapshot.UnclearedCashbookAmount);
				AssertEquals(120m, bankRecon.OpeningSnapshot.UnclearedStatementAmount);
				AssertEquals(-2030m, bankRecon.OpeningSnapshot.AmendedBankStatementBalance);
				AssertEquals(0m, bankRecon.OpeningSnapshot.CashBookBalance);
				AssertEquals(-2030m, bankRecon.OpeningSnapshot.ReconError);
			});

			bankRecon.BankAccountPK = bankAccount2.PK;

			CombineAssertions("OpeningSnapshot should change when bank account is changed.", () =>
			{
				AssertEquals(0m, bankRecon.OpeningSnapshot.StatementBalance);
				AssertEquals(0m, bankRecon.OpeningSnapshot.UnclearedCashbookAmount);
				AssertEquals(0m, bankRecon.OpeningSnapshot.UnclearedStatementAmount);
				AssertEquals(0m, bankRecon.OpeningSnapshot.AmendedBankStatementBalance);
				AssertEquals(-1030m, bankRecon.OpeningSnapshot.CashBookBalance);
				AssertEquals(1030m, bankRecon.OpeningSnapshot.ReconError);
			});
		}

		[SuspendCriticalValidation]
		public void TestSetStatementDate_UpdatesSnapshotObject()
		{
			SetupTransactions(TestBank, Factory);
			Factory.Save();

			var bankRecon = (BankReconciliation)GetNewBusinessObject();
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDate;
			bankRecon.StatementDate = bankRecon.ReconcileDate;
			bankRecon.ClosingBalance = 1000;

			CombineAssertions("OpeningSnapshot should reflect initial state.", () =>
			{
				AssertEquals(0m, bankRecon.OpeningSnapshot.StatementBalance);
				AssertEquals(-2030m, bankRecon.OpeningSnapshot.UnclearedCashbookAmount);
				AssertEquals(120m, bankRecon.OpeningSnapshot.UnclearedStatementAmount);
				AssertEquals(-1910m, bankRecon.OpeningSnapshot.AmendedBankStatementBalance);
				AssertEquals(0m, bankRecon.OpeningSnapshot.CashBookBalance);
				AssertEquals(-1910m, bankRecon.OpeningSnapshot.ReconError);
			});

			bankRecon.ClosingBalance = 1200;

			CombineAssertions("OpeningSnapshot should not change when closing balance is set.", () =>
			{
				AssertEquals(0m, bankRecon.OpeningSnapshot.StatementBalance);
				AssertEquals(-2030m, bankRecon.OpeningSnapshot.UnclearedCashbookAmount);
				AssertEquals(120m, bankRecon.OpeningSnapshot.UnclearedStatementAmount);
				AssertEquals(-1910m, bankRecon.OpeningSnapshot.AmendedBankStatementBalance);
				AssertEquals(0m, bankRecon.OpeningSnapshot.CashBookBalance);
				AssertEquals(-1910m, bankRecon.OpeningSnapshot.ReconError);
			});

			bankRecon.StatementDate = Env.Time.CurrentLocalDate.AddDays(-30);

			CombineAssertions("OpeningSnapshot should change when statement date is changed.", () =>
			{
				AssertEquals(0m, bankRecon.OpeningSnapshot.StatementBalance);
				AssertEquals(-2030m, bankRecon.OpeningSnapshot.UnclearedCashbookAmount);
				AssertEquals(0m, bankRecon.OpeningSnapshot.UnclearedStatementAmount);
				AssertEquals(-2030m, bankRecon.OpeningSnapshot.AmendedBankStatementBalance);
				AssertEquals(-910m, bankRecon.OpeningSnapshot.CashBookBalance);
				AssertEquals(-1120m, bankRecon.OpeningSnapshot.ReconError);
			});
		}

		[SuspendCriticalValidation]
		public void TestSetReconcileDate_UpdatesSnapshotObject()
		{
			SetupTransactions(TestBank, Factory);
			Factory.Save();

			var bankRecon = (BankReconciliation)GetNewBusinessObject();
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDate;
			bankRecon.StatementDate = bankRecon.ReconcileDate;
			bankRecon.ClosingBalance = 1000;

			CombineAssertions("OpeningSnapshot should reflect initial state.", () =>
			{
				AssertEquals(0m, bankRecon.OpeningSnapshot.StatementBalance);
				AssertEquals(-2030m, bankRecon.OpeningSnapshot.UnclearedCashbookAmount);
				AssertEquals(120m, bankRecon.OpeningSnapshot.UnclearedStatementAmount);
				AssertEquals(-1910m, bankRecon.OpeningSnapshot.AmendedBankStatementBalance);
				AssertEquals(0m, bankRecon.OpeningSnapshot.CashBookBalance);
				AssertEquals(-1910m, bankRecon.OpeningSnapshot.ReconError);
			});

			bankRecon.ClosingBalance = 1200;

			CombineAssertions("OpeningSnapshot should not change when closing balance is set.", () =>
			{
				AssertEquals(0m, bankRecon.OpeningSnapshot.StatementBalance);
				AssertEquals(-2030m, bankRecon.OpeningSnapshot.UnclearedCashbookAmount);
				AssertEquals(120m, bankRecon.OpeningSnapshot.UnclearedStatementAmount);
				AssertEquals(-1910m, bankRecon.OpeningSnapshot.AmendedBankStatementBalance);
				AssertEquals(0m, bankRecon.OpeningSnapshot.CashBookBalance);
				AssertEquals(-1910m, bankRecon.OpeningSnapshot.ReconError);
			});

			bankRecon.ReconcileDate = Env.Time.CurrentLocalDate.AddDays(-30);

			CombineAssertions("OpeningSnapshot should change when reconcile date is changed.", () =>
			{
				AssertEquals(0m, bankRecon.OpeningSnapshot.StatementBalance);
				AssertEquals(0m, bankRecon.OpeningSnapshot.UnclearedCashbookAmount);
				AssertEquals(120m, bankRecon.OpeningSnapshot.UnclearedStatementAmount);
				AssertEquals(120m, bankRecon.OpeningSnapshot.AmendedBankStatementBalance);
				AssertEquals(-910m, bankRecon.OpeningSnapshot.CashBookBalance);
				AssertEquals(1030m, bankRecon.OpeningSnapshot.ReconError);
			});
		}

		[SuspendCriticalValidation]
		public void TestTransactionIsCleared_UpdatesClearedInCurrentSession()
		{
			SetupTransactions(TestBank, Factory);
			var unclearedTransaction = SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 120.0m, TestBank, Factory, "");
			var clearedTransaction = SetTransaction(LedgerTypes.AccountsPayable, TransactionTypes.Payment, 110.0m, TestBank, Factory, "");
			clearedTransaction.AH_DateClearedInCashbook = Env.Time.CurrentLocalDate;
			Factory.Save();

			var bankRecon = (BankReconciliation)GetNewBusinessObject();
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDate;
			bankRecon.StatementDate = bankRecon.ReconcileDate;
			bankRecon.ClosingBalance = 1000;
			bankRecon.ClearedFilter = true;
			bankRecon.ApplyFilter();

			AssertEquals("Precondition: no cleared details available.", 0, bankRecon.TransactionsClearedInCurrentSession().Count);
			AssertEquals("Precondition: no uncleared details available.", 0, bankRecon.TransactionsUnclearedInCurrentSession().Count);

			var unclearedTran = bankRecon.Transactions_ForTestOnly.FindByPK(unclearedTransaction.PK) as BankReconTransaction;
			unclearedTran.IsCleared = true;
			AssertEquals("One transaction marked as cleared.", 1, bankRecon.TransactionsClearedInCurrentSession().Count);
			AssertEquals("No transactions marked as uncleared.", 0, bankRecon.TransactionsUnclearedInCurrentSession().Count);

			var clearedTran = bankRecon.Transactions_ForTestOnly.FindByPK(clearedTransaction.PK) as BankReconTransaction;
			clearedTran.IsCleared = false;
			AssertEquals("One transaction marked as cleared.", 1, bankRecon.TransactionsClearedInCurrentSession().Count);
			AssertEquals("One transaction marked as uncleared.", 1, bankRecon.TransactionsUnclearedInCurrentSession().Count);

			unclearedTran.IsCleared = false;
			AssertEquals("No transactions marked as cleared.", 0, bankRecon.TransactionsClearedInCurrentSession().Count);
			AssertEquals("One transaction marked as uncleared.", 1, bankRecon.TransactionsUnclearedInCurrentSession().Count);

			clearedTran.IsCleared = true;
			AssertEquals("No transactions marked as cleared.", 0, bankRecon.TransactionsClearedInCurrentSession().Count);
			AssertEquals("No transactions marked as uncleared.", 0, bankRecon.TransactionsUnclearedInCurrentSession().Count);
		}

		[SuspendCriticalValidation]
		public void TestTrackTransactionChanges_CapturesNewRecords()
		{
			SetupTransactions(TestBank, Factory);
			Factory.Save();
			var bankRecon = (BankReconciliation)GetNewBusinessObject();
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDate;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			var snapshotBefore = bankRecon.MergedTransactionsSnapshot();

			var statementTransaction = Factory.NewWithValidTestData<Statement>();
			bankRecon.MergedTransactions.Add(statementTransaction);
			var snapshotAfter = bankRecon.MergedTransactionsSnapshot();

			AssertEquals("Precondition: nothing added - snapshots", 0, bankRecon.TransactionsAddedInThisSession.Count);
			AssertEquals("Precondition: nothing added - transactions", 0, bankRecon.TransactionsAddedInCurrentSession().Count);
			AssertEquals("Precondition: nothing removed - snapshots", 0, bankRecon.TransactionsRemovedInThisSession.Count);
			AssertEquals("Precondition: nothing removed - transactions", 0, bankRecon.TransactionsRemovedInCurrentSession().Count);

			bankRecon.TrackTransactionChanges(snapshotBefore, snapshotAfter);

			AssertEquals("Statement transaction should be added - snapshot", 1, bankRecon.TransactionsAddedInThisSession.Count);
			Assert("Statement transaction PK should be added", bankRecon.TransactionsAddedInThisSession.ContainsKey(statementTransaction.PK));
			AssertEquals("Statement transaction should be added - snapshot", 1, bankRecon.TransactionsAddedInCurrentSession().Count);
			AssertEquals("Nothing should be removed - snapshots", 0, bankRecon.TransactionsRemovedInThisSession.Count);
			AssertEquals("Nothing should be removed - transactions", 0, bankRecon.TransactionsRemovedInCurrentSession().Count);
		}

		[SuspendCriticalValidation]
		public void TestTrackTransactionChanges_CapturesRemovedRecords()
		{
			SetupTransactions(TestBank, Factory);
			Factory.Save();
			var bankRecon = (BankReconciliation)GetNewBusinessObject();
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDate;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			var snapshotBefore = bankRecon.MergedTransactionsSnapshot();
			AssertNotEquals("Precondition: should be some transactions", 0, snapshotBefore.Count);
			var someTransactionPk = snapshotBefore.First().PK;

			bankRecon.MergedTransactions.Remove(someTransactionPk);
			var snapshotAfter = bankRecon.MergedTransactionsSnapshot();

			AssertEquals("Precondition: nothing added - snapshots", 0, bankRecon.TransactionsAddedInThisSession.Count);
			AssertEquals("Precondition: nothing added - transactions", 0, bankRecon.TransactionsAddedInCurrentSession().Count);
			AssertEquals("Precondition: nothing removed - snapshots", 0, bankRecon.TransactionsRemovedInThisSession.Count);
			AssertEquals("Precondition: nothing removed - transactions", 0, bankRecon.TransactionsRemovedInCurrentSession().Count);

			bankRecon.TrackTransactionChanges(snapshotBefore, snapshotAfter);

			AssertEquals("Nothing should be added - snapshots", 0, bankRecon.TransactionsAddedInThisSession.Count);
			AssertEquals("Nothing should be added - transactions", 0, bankRecon.TransactionsAddedInCurrentSession().Count);
			AssertEquals("Transaction should be removed - snapshots", 1, bankRecon.TransactionsRemovedInThisSession.Count);
			Assert("Transaction PK should be removed", bankRecon.TransactionsRemovedInThisSession.ContainsKey(someTransactionPk));
			AssertEquals("Transaction should be removed - transactions", 1, bankRecon.TransactionsRemovedInCurrentSession().Count);
		}

		[SuspendCriticalValidation]
		public void TestTrackTransactionChanges_CapturesChangedRecords()
		{
			SetupTransactions(TestBank, Factory);
			Factory.Save();
			var bankRecon = (BankReconciliation)GetNewBusinessObject();
			bankRecon.BankAccountPK = TestBank.PK;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDate;
			bankRecon.StatementDate = bankRecon.ReconcileDate;

			var snapshotBefore = bankRecon.MergedTransactionsSnapshot();
			AssertNotEquals("Precondition: should be some transactions", 0, bankRecon.GetStatements().Count);
			var someTransaction = (Statement)bankRecon.GetStatements().First();

			var originalCredit = someTransaction.Credit;
			var originalDebit = someTransaction.Debit;
			someTransaction.AS_Amount = someTransaction.AS_Amount + 1m;
			var newCredit = someTransaction.Credit;
			var newDebit = someTransaction.Debit;
			var snapshotAfter = bankRecon.MergedTransactionsSnapshot();

			AssertEquals("Precondition: nothing added - snapshots", 0, bankRecon.TransactionsAddedInThisSession.Count);
			AssertEquals("Precondition: nothing added - transactions", 0, bankRecon.TransactionsAddedInCurrentSession().Count);
			AssertEquals("Precondition: nothing removed - snapshots", 0, bankRecon.TransactionsRemovedInThisSession.Count);
			AssertEquals("Precondition: nothing removed - transactions", 0, bankRecon.TransactionsRemovedInCurrentSession().Count);

			bankRecon.TrackTransactionChanges(snapshotBefore, snapshotAfter);

			AssertEquals("Changed transaction should be added - snapshots", 1, bankRecon.TransactionsAddedInThisSession.Count);
			AssertEquals("Changed transaction should be added - transactions", 1, bankRecon.TransactionsAddedInCurrentSession().Count);
			Assert("Transaction PK should be added", bankRecon.TransactionsAddedInThisSession.ContainsKey(someTransaction.PK));
			AssertEquals("Added transaction should track new credit", newCredit, bankRecon.TransactionsAddedInThisSession[someTransaction.PK].Credit);
			AssertEquals("Added transaction should track new debit", newDebit, bankRecon.TransactionsAddedInThisSession[someTransaction.PK].Debit);

			AssertEquals("Changed transaction should be removed - snapshots", 1, bankRecon.TransactionsRemovedInThisSession.Count);
			AssertEquals("Changed transaction should be removed - transactions", 1, bankRecon.TransactionsRemovedInCurrentSession().Count);
			Assert("Transaction PK should be removed", bankRecon.TransactionsRemovedInThisSession.ContainsKey(someTransaction.PK));
			AssertEquals("Added transaction should track original credit", originalCredit, bankRecon.TransactionsRemovedInThisSession[someTransaction.PK].Credit);
			AssertEquals("Added transaction should track original debit", originalDebit, bankRecon.TransactionsRemovedInThisSession[someTransaction.PK].Debit);
		}
		#endregion

		#region Implementation

		AccBankAccount TestBank;
		readonly Random Random = new Random();

		protected override void SetUp()
		{
			base.SetUp();
			SetBank();
		}

		public void ExternalSetup() => SetUp();

		void SetBank()
		{
			TestBank = CreateBank("B1", "Bank123", "3820.00.00");
		}

		AccBankAccount CreateBank(string code, string accountNumber, string glNumber)
		{
			var b = new BusinessObjectFactory().New<AccBankAccount>();

			b.AB_Code = code;
			b.AB_AccountNum = accountNumber;
			b.AB_GC = GlbCompany.CurrentCompany.PK;
			b.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			b.AB_AG = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery(AccGLHeaderSchema.AG_AccountNum, glNumber)).PK;
			b.Factory.Save();

			return b;
		}

		AccTransactionHeader SetTransaction(string ledger, string transactionType, decimal localAmount, AccBankAccount fBankAccount, BusinessObjectFactory factory, string receiptBatchNo)
		{
			AccTransactionHeader transaction = factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;

			transaction.AH_Ledger = ledger;
			transaction.AH_TransactionType = transactionType;
			transaction.AH_InvoiceAmount = localAmount;
			transaction.AH_OutstandingAmount = localAmount;
			transaction.AH_OSTotal = localAmount;

			//Mandatory values
			transaction.AH_InvoiceDate = Env.Time.CurrentLocalDateTime;
			transaction.AH_PostDate = Env.Time.CurrentLocalDateTime;
			transaction.AH_GC = GlbCompany.CurrentCompany.PK;
			transaction.AH_GB = GlbBranch.CurrentBranch.PK;
			transaction.AH_GE = GlbDepartment.CurrentDepartment.PK;

			//Others
			transaction.AH_TransactionNum = ledger + transactionType + Random.Next(1000000000);
			transaction.AH_AB = fBankAccount.PK;
			transaction.AH_ReceiptBatchNo = receiptBatchNo;

			return transaction;
		}

		AccTransactionHeader SetTransaction(string ledger, string transactionType, decimal localAmount, decimal foreignAmount, decimal gstAmount, AccBankAccount fBankAccount, BusinessObjectFactory factory, string receiptBatchNo)
		{
			AccTransactionHeader transaction = Factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;

			transaction.AH_Ledger = ledger;
			transaction.AH_TransactionType = transactionType;
			transaction.AH_InvoiceAmount = localAmount;
			transaction.AH_OSTotal = foreignAmount;
			transaction.AH_OutstandingAmount = localAmount;
			transaction.AH_GSTAmount = gstAmount;

			//Mandatory values
			transaction.AH_InvoiceDate = Env.Time.CurrentLocalDateTime;
			transaction.AH_PostDate = Env.Time.CurrentLocalDateTime;
			transaction.AH_GC = GlbCompany.CurrentCompany.PK;
			transaction.AH_GB = GlbBranch.CurrentBranch.PK;
			transaction.AH_GE = GlbDepartment.CurrentDepartment.PK;

			//Others
			transaction.AH_TransactionNum = ledger + transactionType + Random.Next(1000000000);
			transaction.AH_AB = fBankAccount.PK;
			transaction.AH_ReceiptBatchNo = receiptBatchNo;

			return transaction;
		}

		Statement SetStatement(string debitCredit, string type, decimal amount, string reference, AccBankAccount fBankAccount, BusinessObjectFactory factory)
		{
			Statement testStatment = factory.New(typeof(Statement)) as Statement;

			testStatment.AS_AB = fBankAccount.PK;
			testStatment.AS_DebitCredit = debitCredit;
			testStatment.AS_Type = type;
			testStatment.AS_Amount = amount;
			testStatment.AS_ChequeOrReference = reference;
			testStatment.AS_StatementDate = Env.Time.CurrentLocalDateTime;

			return testStatment;
		}

		void AssertAttachedEDocs(DocManagerInfo docManagerInfo, ZString expectedStartOfFilename, ZString expectedDocType)
		{
			for (int i = 0; i < docManagerInfo.Files.Count; i++)
			{
				var storageFile = (BusinessObject)docManagerInfo.Files[i];
				var docType = (RefDocType)storageFile["DocType"];
				Assert($"StorageFile #{i} should be saved to database.", storageFile.IsInDatabase);
				var docAssertionMessage = $"StorageFile #{i} RefDocType should match '{expectedDocType}'";
				if (docType.RT_DocType.IsEmpty)
				{
					docAssertionMessage += " (did you forget to do Testing > DB Upgrade > Reload DocumentsComplete.xml)";
				}
				AssertEquals(docAssertionMessage, expectedDocType, docType.RT_DocType);
				var fileAsAttachment = (IDeliveryEmailAttachment)storageFile;
				AssertGreaterThan($"StorageFile #{i} should be larger than zero bytes.", fileAsAttachment.FileSizeInBytes, 0);
				AssertStartsWith($"StorageFile #{i} should match expected naming scheme.", expectedStartOfFilename, fileAsAttachment.FileName);
				AssertEndsWith($"StorageFile #{i} should be an Excel xlsx file.", ".xlsx", fileAsAttachment.FileName);
			}
		}

		void AssertNoDuplicateEDocsFilenames(DocManagerInfo docManagerInfo)
		{
			var duplicateNames = docManagerInfo.Files
				.Cast<IDeliveryEmailAttachment>()
				.GroupBy(f => f.FileName)
				.Where(g => g.Count() > 1);
			AssertEquals("There should be no duplicate files. Duplicates found: " + string.Join(",", duplicateNames.Select(x => x.First())), 0, duplicateNames.Count());
		}

		void AssertEDocsFilename(DocManagerInfo docManagerInfo, int idx, ZString expectedFilename)
		{
			AssertGreaterThanOrEqualTo("Document index should not exceed attached document count", docManagerInfo.Files.Count - 1, idx);
			var fileAsAttachment = (IDeliveryEmailAttachment)docManagerInfo.Files[idx];
			AssertEquals(expectedFilename, fileAsAttachment.FileName);
		}

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		#endregion
	}
}

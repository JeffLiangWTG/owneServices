using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	[TestedType(typeof(BankReconTransaction))]
	public class BankReconTransactionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesBankReconTransaction()
		{
			var transaction = Factory.NewWithValidTestData<BankReconTransaction>();
			var bankAccount = Factory.New<AccBankAccount>();
			transaction.AH_AB = bankAccount.PK;

			var bankList = new List<string>
			{
				nameof(transaction.Debit),
				nameof(transaction.Credit),
				nameof(transaction.StatementCredit),
				nameof(transaction.StatementDebit),
			};

			var exList = new List<string>
			{
				nameof(transaction.AH_ExchangeRate)
			};

			var tester = new DecimalPlacesAttributeTester(transaction);
			tester.CheckNonLocalCurrency(bankList, nameof(transaction.BankCurrencyDecimalsAsInt), nameof(transaction.BankAccount.AB_RX_NKAccountCurrency), transaction.BankAccount);
			tester.CheckExchangeRate(exList, nameof(transaction.ExchangeRateDecimalPlaces));
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestDeleteSavedBankRecon()
		{
			BankReconTransaction transaction = Factory.NewWithValidTestData<BankReconTransaction>();
			Factory.Save();
			transaction.Delete();
		}

		public void TestDeleteNotSavedBankRecon()
		{
			BankReconTransaction transaction = Factory.NewWithValidTestData<BankReconTransaction>();
			transaction.Delete();
			AssertEquals("Transaction should be deleted", true, transaction.IsDeleted);
		}

		public void TestRelatedStatementPK()
		{
			BankReconTransaction transaction = Factory.NewWithValidTestData<BankReconTransaction>();
			ZGuid testPK = ZGuid.NewZGuid();
			transaction.RelatedStatementPK = testPK;
			AssertEquals("RelatedStatementPK", testPK, transaction.RelatedStatementPK);
		}

		public void TestReceiptBatchesAlwaysDisplayOSAmount()
		{
			BusinessObjectFactory dataFactory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(dataFactory);

			DirectReceipt.DirectReceipt receipt = dataFactory.New<DirectReceipt.DirectReceipt>();
			receipt.AH_AB = creator.AUDBankAccount.PK;
			receipt.AH_ReceiptType = ReceiptTypes.DirectCredit;
			receipt.AH_ChequeOrReference = "1";
			DirectReceipt.DirectReceiptLine line = (DirectReceipt.DirectReceiptLine)receipt.Lines.AddNew();
			line.AL_AG = creator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 100m;
			line.AL_AT = creator.GST1.PK;
			line.AL_OSTaxAmount = 50m;
			dataFactory.Save();

			DirectReceipt.DirectReceipt secondReceipt = dataFactory.New<DirectReceipt.DirectReceipt>();
			secondReceipt.AH_AB = creator.AUDBankAccount.PK;
			secondReceipt.AH_ReceiptType = ReceiptTypes.Cheque;
			secondReceipt.AH_ChequeOrReference = "1";
			DirectReceipt.DirectReceiptLine secondReceiptline = (DirectReceipt.DirectReceiptLine)secondReceipt.Lines.AddNew();
			secondReceiptline.AL_AG = creator.GLHeader1.PK;
			secondReceiptline.AL_OSExTaxAmount = 100m;
			secondReceiptline.AL_AT = creator.GST1.PK;
			secondReceiptline.AL_OSTaxAmount = 50m;
			dataFactory.Save();

			DepositBatch.DepositBatch batchHeader = dataFactory.New<DepositBatch.DepositBatch>();
			batchHeader.AH_AB = creator.AUDBankAccount.PK;
			batchHeader.LoadTransactions(ZGuid.Empty);
			AssertEquals("Should have the direct receipt in the batch", 1, batchHeader.Transactions.Count);
			dataFactory.Save();

			ZQuery findBatchHeaderQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ReceiptBatch);
			findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, receipt.AH_ReceiptBatchNo);
			findBatchHeaderQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, receipt.AH_GC);
			BankReconTransaction transaction = Factory.LoadTop1<BankReconTransaction>(findBatchHeaderQuery);
			AssertEquals("Deposit batches should always display the foreign amount - because the GST value of DRC transactions isn't recorded",
				150m, transaction.Debit);
			AssertEquals(0m, transaction.Credit);

			transaction = Factory.Load<BankReconTransaction>(batchHeader.PK);
			AssertEquals("Deposit batches should always display the foreign amount - because the GST value of DRC transactions isn't recorded",
				150m, transaction.Debit);
			AssertEquals(0m, transaction.Credit);
		}

		public void TestDepositBatchCollection()
		{
			var testBank = GetTestBankAccount();

			var testHeader1 = Factory.New<AccTransactionHeader>();
			var testHeader2 = Factory.New<AccTransactionHeader>();
			var testHeader3 = Factory.New<AccTransactionHeader>();
			var testHeader4 = Factory.New<AccTransactionHeader>();

			testHeader1.AH_ReceiptBatchNo = "TST1";
			testHeader2.AH_ReceiptBatchNo = "TST2";
			testHeader3.AH_ReceiptBatchNo = "TST1";
			testHeader4.AH_ReceiptBatchNo = "TST1";

			testHeader1.AH_AB = testBank.PK;
			testHeader2.AH_AB = testBank.PK;
			testHeader3.AH_AB = testBank.PK;
			testHeader4.AH_AB = testBank.PK;

			testHeader1.AH_GB = GlbBranch.CurrentBranch.PK;
			testHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
			testHeader3.AH_GB = GlbBranch.CurrentBranch.PK;
			testHeader4.AH_GB = GlbBranch.CurrentBranch.PK;

			testHeader1.AH_TransactionType = TransactionTypes.Receipt;
			testHeader2.AH_TransactionType = TransactionTypes.DirectReceipt;
			testHeader3.AH_TransactionType = TransactionTypes.Receipt;
			testHeader4.AH_TransactionType = TransactionTypes.ReceiptBatch;

			var testTransaction = Factory.Load<BankReconTransaction>(testHeader4.PK);
			testTransaction.AH_AB = testBank.PK;
			AssertEquals(2, testTransaction.DepositBatchTransactions.Count);
			AssertEquals(0, testTransaction.DirectDebitBatchTransactions.Count);
		}

		public void TestDepositBatchCollection_WhenNoBatchNumberYet()
		{
			var testBank = GetTestBankAccount();

			var testHeader1 = Factory.New<AccTransactionHeader>();
			var testHeader2 = Factory.New<AccTransactionHeader>();
			var testHeader3 = Factory.New<AccTransactionHeader>();
			var testHeader4 = Factory.New<AccTransactionHeader>();

			testHeader1.AH_ReceiptBatchNo = "";
			testHeader2.AH_ReceiptBatchNo = "TST2";
			testHeader3.AH_ReceiptBatchNo = "";
			testHeader4.AH_ReceiptBatchNo = "";

			testHeader1.AH_AB = testBank.PK;
			testHeader2.AH_AB = testBank.PK;
			testHeader3.AH_AB = testBank.PK;
			testHeader4.AH_AB = testBank.PK;

			testHeader1.AH_GB = GlbBranch.CurrentBranch.PK;
			testHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
			testHeader3.AH_GB = GlbBranch.CurrentBranch.PK;
			testHeader4.AH_GB = GlbBranch.CurrentBranch.PK;

			testHeader1.AH_TransactionType = TransactionTypes.Receipt;
			testHeader2.AH_TransactionType = TransactionTypes.DirectReceipt;
			testHeader3.AH_TransactionType = TransactionTypes.Receipt;
			testHeader4.AH_TransactionType = TransactionTypes.ReceiptBatch;

			var testTransaction = Factory.Load<BankReconTransaction>(testHeader4.PK);
			testTransaction.AH_AB = testBank.PK;
			testTransaction.MasterBankRecon = new BankReconciliation(Factory);
			testTransaction.MasterBankRecon.AdditionalTransactions.AddToBatchToDirectReceiptPKMapping(testHeader4.PK, testHeader1.PK);
			AssertEquals("Only the transaction that has been marked as the child transaction of the batch is returned", 1, testTransaction.DepositBatchTransactions.Count);
			AssertEquals("Only the transaction that has been marked as the child transaction of the batch is returned", testHeader1.PK, testTransaction.DepositBatchTransactions[0].PK);
			AssertEquals(0, testTransaction.DirectDebitBatchTransactions.Count);
		}

		public void TestDDBCollection()
		{
			AccBankAccount testBank = GetTestBankAccount();

			AccTransactionHeader testHeader1 = Factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;
			AccTransactionHeader testHeader2 = Factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;
			AccTransactionHeader testHeader3 = Factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;
			AccTransactionHeader testHeader4 = Factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;

			testHeader1.AH_ReceiptBatchNo = "TST1";
			testHeader2.AH_ReceiptBatchNo = "TST2";
			testHeader3.AH_ReceiptBatchNo = "TST1";
			testHeader4.AH_ReceiptBatchNo = "TST1";

			testHeader1.AH_AB = testBank.PK;
			testHeader2.AH_AB = testBank.PK;
			testHeader3.AH_AB = testBank.PK;
			testHeader4.AH_AB = testBank.PK;

			testHeader1.AH_GB = GlbBranch.CurrentBranch.PK;
			testHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
			testHeader3.AH_GB = GlbBranch.CurrentBranch.PK;
			testHeader4.AH_GB = GlbBranch.CurrentBranch.PK;

			testHeader1.AH_TransactionType = TransactionTypes.Payment;
			testHeader2.AH_TransactionType = TransactionTypes.DirectPayment;
			testHeader3.AH_TransactionType = TransactionTypes.Payment;
			testHeader4.AH_TransactionType = TransactionTypes.Payment;

			BankReconTransaction testTransaction = Factory.Load(typeof(BankReconTransaction), testHeader4.PK) as BankReconTransaction;
			testTransaction.AH_AB = testBank.PK;
			AssertEquals(2, testTransaction.DirectDebitBatchTransactions.Count);
			AssertEquals(0, testTransaction.DepositBatchTransactions.Count);
		}

		public void TestDebitCreditForTransfer()
		{
			BankReconTransaction testTransaction = Factory.New(typeof(BankReconTransaction)) as BankReconTransaction;

			testTransaction.AH_TransactionType = TransactionTypes.Transfer;
			testTransaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			testTransaction.AH_OSTotal = 120m;

			testTransaction.OnLoaded();

			AssertEquals(120m, testTransaction.Debit);
			AssertEquals(0m, testTransaction.Credit);
		}

		public void TestDebitCreditForDPY()
		{
			BankReconTransaction testTransaction = Factory.New(typeof(BankReconTransaction)) as BankReconTransaction;

			testTransaction.AH_TransactionType = TransactionTypes.DirectPayment;
			testTransaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			testTransaction.AH_OSTotal = 120m;

			testTransaction.OnLoaded();

			AssertEquals(120m, testTransaction.Debit);
			AssertEquals(0m, testTransaction.Credit);
		}

		public void TestDebitCreditForPayment()
		{
			BankReconTransaction testTransaction = Factory.New(typeof(BankReconTransaction)) as BankReconTransaction;

			testTransaction.AH_TransactionType = TransactionTypes.Payment;
			testTransaction.AH_Ledger = LedgerTypes.AccountsPayable;
			testTransaction.AH_OSTotal = -120m;

			testTransaction.OnLoaded();

			AssertEquals(120m, testTransaction.Debit);
			AssertEquals(0m, testTransaction.Credit);
		}

		public void TestDifferentCompany()
		{
			GlbBranch newBranch = Factory.New(typeof(GlbBranch)) as GlbBranch;

			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			AccTransactionHeader testHeader1 = Factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;
			AccTransactionHeader testHeader2 = Factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;
			AccTransactionHeader testHeader3 = Factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;
			AccTransactionHeader testHeader4 = Factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;

			testHeader1.AH_ReceiptBatchNo = "TST1";
			testHeader2.AH_ReceiptBatchNo = "TST1";
			testHeader3.AH_ReceiptBatchNo = "TST1";
			testHeader4.AH_ReceiptBatchNo = "TST1";

			testHeader1.AH_TransactionType = TransactionTypes.Receipt;
			testHeader2.AH_TransactionType = TransactionTypes.Receipt;
			testHeader3.AH_TransactionType = TransactionTypes.DirectReceipt;
			testHeader4.AH_TransactionType = TransactionTypes.ReceiptBatch;

			testHeader1.AH_GB = newBranch.PK;
			testHeader2.AH_GB = ZGuid.NewZGuid();
			testHeader3.AH_GB = GlbBranch.CurrentBranch.PK;
			testHeader4.AH_GB = GlbBranch.CurrentBranch.PK;

			testHeader1.AH_OSTotal = 120m;
			testHeader2.AH_OSTotal = 130m;
			testHeader3.AH_OSTotal = 140m;
			testHeader4.AH_OSTotal = 20m;

			BankReconTransaction testTransaction = Factory.Load(typeof(BankReconTransaction), testHeader4.PK) as BankReconTransaction;
			testTransaction.OnLoaded(); // This is a workaround for the test - OnLoaded only called on the loading from DB

			AssertEquals(20.0m, testTransaction.Debit);
			AssertEquals(0m, testTransaction.Credit);

			testHeader1.AH_OSTotal = -120m;
			testHeader2.AH_OSTotal = -130m;
			testHeader3.AH_OSTotal = -140m;
			testHeader4.AH_OSTotal = -20m;

			testTransaction = Factory.Load(typeof(BankReconTransaction), testHeader4.PK) as BankReconTransaction;
			testTransaction.OnLoaded(); // This is a workaround for the test - OnLoaded only called on the loading from DB

			AssertEquals(0.0m, testTransaction.Debit);
			AssertEquals(20m, testTransaction.Credit);
		}

		public void TestDebitCreditForDepositBatch()
		{
			AccTransactionHeader testHeader1 = Factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;
			AccTransactionHeader testHeader2 = Factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;
			AccTransactionHeader testHeader3 = Factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;
			AccTransactionHeader testHeader4 = Factory.New(typeof(AccTransactionHeader)) as AccTransactionHeader;

			testHeader1.AH_ReceiptBatchNo = "TST1";
			testHeader2.AH_ReceiptBatchNo = "TST2";
			testHeader3.AH_ReceiptBatchNo = "TST1";
			testHeader4.AH_ReceiptBatchNo = "TST1";

			testHeader1.AH_TransactionType = TransactionTypes.Receipt;
			testHeader2.AH_TransactionType = TransactionTypes.Receipt;
			testHeader3.AH_TransactionType = TransactionTypes.DirectReceipt;
			testHeader4.AH_TransactionType = TransactionTypes.ReceiptBatch;

			testHeader1.AH_OSTotal = 120m;
			testHeader2.AH_OSTotal = 130m;
			testHeader3.AH_OSTotal = 140m;
			testHeader4.AH_OSTotal = 20m;

			testHeader1.AH_GB = GlbBranch.CurrentBranch.PK;
			testHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
			testHeader3.AH_GB = GlbBranch.CurrentBranch.PK;
			testHeader4.AH_GB = GlbBranch.CurrentBranch.PK;

			BankReconTransaction testTransaction = Factory.Load(typeof(BankReconTransaction), testHeader4.PK) as BankReconTransaction;
			testTransaction.OnLoaded(); // This is a workaround for the test - OnLoaded only called on the loading from DB

			AssertEquals(20.0m, testTransaction.Debit);
			AssertEquals(0m, testTransaction.Credit);

			testHeader1.AH_OSTotal = -120m;
			testHeader2.AH_OSTotal = -130m;
			testHeader3.AH_OSTotal = -140m;
			testHeader4.AH_OSTotal = -20m;

			testTransaction = Factory.Load(typeof(BankReconTransaction), testHeader4.PK) as BankReconTransaction;
			testTransaction.OnLoaded(); // This is a workaround for the test - OnLoaded only called on the loading from DB

			AssertEquals(0.0m, testTransaction.Debit);
			AssertEquals(20m, testTransaction.Credit);
		}

		public void TestForDDRBatch()
		{
			BankReconTransaction testTransaction = Factory.New(typeof(BankReconTransaction)) as BankReconTransaction;

			testTransaction.AH_TransactionType = TransactionTypes.DDRBatch;
			testTransaction.AH_Ledger = LedgerTypes.CashBook;
			testTransaction.AH_OSTotal = -120m;

			testTransaction.OnLoaded();

			AssertEquals(120m, testTransaction.Debit);
			AssertEquals(0m, testTransaction.Credit);
		}

		public void TestClearedDate()
		{
			ZDateTime clearedDate = new ZDateTime(2006, 2, 14);
			BankReconTransaction testTransaction = Factory.New(typeof(BankReconTransaction)) as BankReconTransaction;

			AssertEquals(ZBool.False, testTransaction.IsCleared);

			testTransaction.ClearedDate = clearedDate;
			AssertEquals(ZBool.True, testTransaction.IsCleared);
		}

		public void TestUpdateClearedDateForReceiptBatch()
		{
			AccBankAccount testBank = GetTestBankAccount();

			SetReceiptBatchWith4Transactions(testBank);

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = testBank.PK;
			bankRecon.StatementDate = Env.Time.CurrentLocalDateTime;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;

			AssertEquals(1, bankRecon.CombinedTransactions.Count);

			BankReconTransaction testTransaction = bankRecon.CombinedTransactions[0];
			testTransaction.MasterBankRecon = bankRecon;

			AssertEquals("There should be 4 transaction in the collection.", testTransaction.DepositBatchTransactions.Count, 4);

			testTransaction.IsCleared = ZBool.True;
			AssertEquals(bankRecon.StatementDate, testTransaction.DepositBatchTransactions[0].AH_DateClearedInCashbook);
			AssertEquals(bankRecon.StatementDate, testTransaction.DepositBatchTransactions[1].AH_DateClearedInCashbook);
			AssertEquals(bankRecon.StatementDate, testTransaction.DepositBatchTransactions[2].AH_DateClearedInCashbook);
			AssertEquals(bankRecon.StatementDate, testTransaction.DepositBatchTransactions[3].AH_DateClearedInCashbook);

			testTransaction.IsCleared = ZBool.False;
			AssertEquals(testTransaction.DepositBatchTransactions[0].AH_DateClearedInCashbook, ZDateTime.Empty);
			AssertEquals(testTransaction.DepositBatchTransactions[1].AH_DateClearedInCashbook, ZDateTime.Empty);
			AssertEquals(testTransaction.DepositBatchTransactions[2].AH_DateClearedInCashbook, ZDateTime.Empty);
			AssertEquals(testTransaction.DepositBatchTransactions[3].AH_DateClearedInCashbook, ZDateTime.Empty);
		}

		public void TestUpdateClearedDateForDDRBatch()
		{
			AccBankAccount testBank = GetTestBankAccount();

			SetDDRBatchWith4Transactions(testBank);

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = testBank.PK;
			bankRecon.StatementDate = Env.Time.CurrentLocalDateTime;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;

			AssertEquals(1, bankRecon.CombinedTransactions.Count);

			BankReconTransaction testTransaction = bankRecon.CombinedTransactions[0];
			testTransaction.MasterBankRecon = bankRecon;

			AssertEquals("There should be 4 transaction in the collection.", testTransaction.DirectDebitBatchTransactions.Count, 4);

			testTransaction.IsCleared = ZBool.True;
			AssertEquals(bankRecon.StatementDate, testTransaction.DirectDebitBatchTransactions[0].AH_DateClearedInCashbook);
			AssertEquals(bankRecon.StatementDate, testTransaction.DirectDebitBatchTransactions[1].AH_DateClearedInCashbook);
			AssertEquals(bankRecon.StatementDate, testTransaction.DirectDebitBatchTransactions[2].AH_DateClearedInCashbook);
			AssertEquals(bankRecon.StatementDate, testTransaction.DirectDebitBatchTransactions[3].AH_DateClearedInCashbook);

			testTransaction.IsCleared = ZBool.False;
			AssertEquals(ZDateTime.Empty, testTransaction.DirectDebitBatchTransactions[0].AH_DateClearedInCashbook);
			AssertEquals(ZDateTime.Empty, testTransaction.DirectDebitBatchTransactions[1].AH_DateClearedInCashbook);
			AssertEquals(ZDateTime.Empty, testTransaction.DirectDebitBatchTransactions[2].AH_DateClearedInCashbook);
			AssertEquals(ZDateTime.Empty, testTransaction.DirectDebitBatchTransactions[3].AH_DateClearedInCashbook);
		}

		public void TestUpdateClearedDateFor2SingleTransactions()
		{
			AccBankAccount testBank = GetTestBankAccount();

			AccTransactionHeader dDRBatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			dDRBatch.AH_Ledger = LedgerTypes.CashBook;
			dDRBatch.AH_TransactionType = TransactionTypes.DirectPayment;
			dDRBatch.AH_ReceiptType = ReceiptTypes.Cheque;
			dDRBatch.AH_GB = GlbBranch.CurrentBranch.PK;
			dDRBatch.AH_AB = testBank.PK;
			dDRBatch.AH_PostDate = Env.Time.CurrentLocalDateTime;

			AccTransactionHeader dDRBatch2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			dDRBatch2.AH_Ledger = LedgerTypes.CashBook;
			dDRBatch2.AH_TransactionType = TransactionTypes.DirectPayment;
			dDRBatch2.AH_ReceiptType = ReceiptTypes.Cheque;
			dDRBatch2.AH_GB = GlbBranch.CurrentBranch.PK;
			dDRBatch2.AH_AB = testBank.PK;
			dDRBatch2.AH_PostDate = Env.Time.CurrentLocalDateTime;

			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = testBank.PK;
			bankRecon.StatementDate = Env.Time.CurrentLocalDateTime;
			bankRecon.ReconcileDate = Env.Time.CurrentLocalDateTime;

			AssertEquals(2, bankRecon.CombinedTransactions.Count);

			BankReconTransaction testTransaction1 = bankRecon.CombinedTransactions[0];
			testTransaction1.MasterBankRecon = bankRecon;

			BankReconTransaction testTransaction2 = bankRecon.CombinedTransactions[1];
			testTransaction2.MasterBankRecon = bankRecon;

			testTransaction1.IsCleared = ZBool.True;
			AssertEquals(bankRecon.StatementDate, testTransaction1.AH_DateClearedInCashbook);
			AssertEquals(ZDateTime.Empty, testTransaction2.AH_DateClearedInCashbook);

			testTransaction2.IsCleared = ZBool.True;
			AssertEquals(bankRecon.StatementDate, testTransaction2.AH_DateClearedInCashbook);
		}

		public void TestReadOnlyProperties()
		{
			BankReconTransaction transaction = Factory.New<BankReconTransaction>();
			AssertEquals(true, transaction.TransactionDateInfo.ReadOnly);
			AssertEquals(true, transaction.InvoiceDateInfo.ReadOnly);
			AssertEquals(true, transaction.TypeInfo.ReadOnly);
			AssertEquals(true, transaction.MethodInfo.ReadOnly);
			AssertEquals(true, transaction.PayeeInfo.ReadOnly);
			AssertEquals(true, transaction.ChequeRefInfo.ReadOnly);
			AssertEquals(true, transaction.DebitInfo.ReadOnly);
			AssertEquals(true, transaction.CreditInfo.ReadOnly);
			AssertEquals(true, transaction.LineTypeInfo.ReadOnly);
			AssertEquals(false, transaction.IsClearedInfo.ReadOnly);
			AssertEquals(true, transaction.ClearedDateInfo.ReadOnly);
		}

		public void TestAmount()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			BankReconTransactionForTest transaction = Factory.New<BankReconTransactionForTest>();
			transaction.AH_AB = testObjectCreator.AUDBankAccount.PK;
			transaction.AH_OSTotal = 10;
			transaction.AH_InvoiceAmount = 3.5;
			transaction.AH_GSTAmount = 0.2;
			AssertEquals("Local currency", 3.7m, transaction.Amount);
			transaction.AH_AB = testObjectCreator.USDBankAccount.PK;
			AssertEquals("Foreign currency", 10m, transaction.Amount);
		}

		public void TestReceiptBatchesWithSingleDCRReceipt()
		{
			BusinessObjectFactory dataFactory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(dataFactory);

			string expectedChequeOrReference = "CH123456";
			string expectedReceiptType = ReceiptTypes.DirectCredit;

			APReceipt receipt = dataFactory.New<APReceipt>();
			receipt.AH_AB = creator.AUDBankAccount.PK;
			receipt.AH_ReceiptType = expectedReceiptType;
			receipt.AH_ChequeOrReference = expectedChequeOrReference;
			dataFactory.Save();

			BankReconTransaction transaction = Factory.Load<BankReconTransaction>(receipt.RelatedDepositBatch.PK);
			AssertEquals("Single DCR in a batch.", expectedReceiptType, transaction.Method);
			AssertEquals("Single DCR in a batch.", expectedChequeOrReference, transaction.ChequeRef);

			var directReceipt = creator.CreateDirectReceipt(ZDateTime.Today, 100.00M, 0, 100.00M, 0M);
			directReceipt.AH_AB = creator.AUDBankAccount.PK;
			directReceipt.AH_ReceiptType = ReceiptTypes.DirectCredit;
			directReceipt.AH_ChequeOrReference = expectedChequeOrReference;
			dataFactory.Save();

			transaction = Factory.Load<BankReconTransaction>(directReceipt.RelatedDepositBatch.PK);
			AssertEquals("Single DCR in a batch.", expectedReceiptType, transaction.Method);
			AssertEquals("Single DCR in a batch.", expectedChequeOrReference, transaction.ChequeRef);

			receipt = dataFactory.New<APReceipt>();
			receipt.AH_AB = creator.AUDBankAccount.PK;
			receipt.AH_ReceiptType = ReceiptTypes.MiscellaneousReceipt;
			receipt.AH_ChequeOrReference = expectedChequeOrReference;
			dataFactory.Save();

			transaction = Factory.Load<BankReconTransaction>(receipt.RelatedDepositBatch.PK);
			AssertEquals("Single not DCR in a batch.", "", transaction.Method);
			AssertEquals("Single not DCR in a batch.", receipt.RelatedDepositBatch.AH_TransactionNum, transaction.ChequeRef);
		}

		public void TestSetIsClearedBeforeMasterBankReconSet()
		{
			BankReconTransaction transaction = Factory.New<BankReconTransaction>();
			transaction.IsCleared = true;
			AssertEquals("Doesn't set until MasterBankRecon set.", false, transaction.IsCleared);
			BankReconciliation masterBankRecon = new BankReconciliation(Factory);
			masterBankRecon.StatementDate = ZDateTime.Now;
			transaction.MasterBankRecon = masterBankRecon;
			AssertEquals("Should set a value.", true, transaction.IsCleared);

			transaction.MasterBankRecon = null;
			transaction.ClearedDate = ZDateTime.Empty;
			AssertEquals(false, transaction.IsCleared);

			transaction.MasterBankRecon = masterBankRecon;
			AssertEquals("Should not set a value again.", false, transaction.IsCleared);

			transaction.MasterBankRecon = null;
			transaction.ClearedDate = ZDateTime.Now;
			transaction.IsCleared = false;
			AssertEquals("Doesn't set until MasterBankRecon set.", true, transaction.IsCleared);
			transaction.MasterBankRecon = masterBankRecon;
			AssertEquals("Should set a value.", false, transaction.IsCleared);

			transaction.MasterBankRecon = null;
			transaction.ClearedDate = ZDateTime.Now;
			AssertEquals(true, transaction.IsCleared);

			transaction.MasterBankRecon = masterBankRecon;
			AssertEquals("Should not set a value again.", true, transaction.IsCleared);
		}

		public void TestBranchDepartmentCombinationValidation_BankReconTransaction()
		{
			var bizObj = Factory.NewWithValidTestData<BankReconTransaction>();

			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObj(Factory,
				(branch, department) => { bizObj.AH_GB = branch; bizObj.AH_GE = department; }, bizObj.AH_GEInfo);
		}

		public void TestIsCleared_UpdatesBankRecClearedInCurrentSession()
		{
			var transaction = Factory.New<BankReconTransaction>();
			var masterBankRecon = new BankReconciliation(Factory);
			masterBankRecon.StatementDate = ZDateTime.Now;
			transaction.MasterBankRecon = masterBankRecon;
			AssertEquals("Precondition: IsCleared is not set.", false, transaction.IsCleared);
			AssertEquals("Precondition: Nothing marked as cleared", 0, masterBankRecon.TransactionIdsClearedInCurrentSession.Count);
			AssertEquals("Precondition: Nothing marked as uncleared", 0, masterBankRecon.TransactionIdsUnclearedInCurrentSession.Count);

			transaction.IsCleared = true;
			AssertCollectionContains("Transaction is marked as cleared by PK.", transaction.PK, masterBankRecon.TransactionIdsClearedInCurrentSession);

			transaction.IsCleared = false;
			AssertCollectionNotContains("Transaction is unmarked as cleared by PK.", transaction.PK, masterBankRecon.TransactionIdsClearedInCurrentSession);
			AssertCollectionNotContains("Transaction is not uncleared by PK.", transaction.PK, masterBankRecon.TransactionIdsUnclearedInCurrentSession);

			transaction.IsCleared = true;
			AssertCollectionContains("Transaction is marked as cleared by PK.", transaction.PK, masterBankRecon.TransactionIdsClearedInCurrentSession);
			AssertCollectionNotContains("Transaction is not uncleared by PK.", transaction.PK, masterBankRecon.TransactionIdsUnclearedInCurrentSession);
		}

		public void TestIsCleared_UpdatesBankRecUnclearedInCurrentSession()
		{
			var masterBankRecon = new BankReconciliation(Factory);
			masterBankRecon.StatementDate = ZDateTime.Now;
			var transaction = Factory.New<BankReconTransaction>();
			transaction.MasterBankRecon = masterBankRecon;
			transaction.ClearedDate = ZDateTime.Now;
			AssertEquals("Precondition: IsCleared is set based on ClearedDate.", true, transaction.IsCleared);
			AssertEquals("Precondition: Nothing marked as cleared", 0, masterBankRecon.TransactionIdsClearedInCurrentSession.Count);
			AssertEquals("Precondition: Nothing marked as uncleared", 0, masterBankRecon.TransactionIdsUnclearedInCurrentSession.Count);

			transaction.IsCleared = false;
			AssertCollectionContains("Transaction is marked as uncleared by PK.", transaction.PK, masterBankRecon.TransactionIdsUnclearedInCurrentSession);

			transaction.IsCleared = true;
			AssertCollectionNotContains("Transaction is unmarked as checked by PK.", transaction.PK, masterBankRecon.TransactionIdsUnclearedInCurrentSession);
			AssertCollectionNotContains("Transaction is not cleared by PK.", transaction.PK, masterBankRecon.TransactionIdsClearedInCurrentSession);

			transaction.IsCleared = false;
			AssertCollectionContains("Transaction is marked as uncleared by PK.", transaction.PK, masterBankRecon.TransactionIdsUnclearedInCurrentSession);
			AssertCollectionNotContains("Transaction is not cleared by PK.", transaction.PK, masterBankRecon.TransactionIdsClearedInCurrentSession);
		}

		#region Implementation 

		protected override bool CanPersistedObjectBeDeleted
		{
			get { return false; }
		}

		AccBankAccount GetTestBankAccount()
		{
			AccBankAccount testBank = Factory.New(typeof(AccBankAccount)) as AccBankAccount;
			testBank.AB_AccountNum = "10000000";
			testBank.AB_GC = GlbCompany.CurrentCompany.PK;
			testBank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			testBank.AB_AG = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK;
			testBank.AB_Code = "ABCBANK";
			return testBank;
		}

		void SetReceiptBatchWith4Transactions(AccBankAccount testBank)
		{
			AccTransactionHeader transactionINBatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader transactionINBatch2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader transactionINBatch3 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader transactionINBatch4 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader headerOfBatch = Factory.NewWithValidTestData<AccTransactionHeader>();

			headerOfBatch.AH_Ledger = LedgerTypes.CashBook;
			transactionINBatch.AH_Ledger = LedgerTypes.AccountsPayable;
			transactionINBatch2.AH_Ledger = LedgerTypes.AccountsPayable;
			transactionINBatch3.AH_Ledger = LedgerTypes.AccountsPayable;
			transactionINBatch4.AH_Ledger = LedgerTypes.CashBook;

			headerOfBatch.AH_TransactionType = TransactionTypes.ReceiptBatch;
			transactionINBatch.AH_TransactionType = TransactionTypes.Receipt;
			transactionINBatch2.AH_TransactionType = TransactionTypes.Receipt;
			transactionINBatch3.AH_TransactionType = TransactionTypes.Receipt;
			transactionINBatch4.AH_TransactionType = TransactionTypes.DirectReceipt;

			transactionINBatch.AH_ReceiptType = ReceiptTypes.Cheque;
			transactionINBatch2.AH_ReceiptType = ReceiptTypes.Cash;
			transactionINBatch3.AH_ReceiptType = ReceiptTypes.CreditCard;
			transactionINBatch4.AH_ReceiptType = ReceiptTypes.AccountMaintenanceFee;

			headerOfBatch.AH_ReceiptBatchNo = "155000000";
			transactionINBatch.AH_ReceiptBatchNo = headerOfBatch.AH_ReceiptBatchNo;
			transactionINBatch2.AH_ReceiptBatchNo = headerOfBatch.AH_ReceiptBatchNo;
			transactionINBatch3.AH_ReceiptBatchNo = headerOfBatch.AH_ReceiptBatchNo;
			transactionINBatch4.AH_ReceiptBatchNo = headerOfBatch.AH_ReceiptBatchNo;

			headerOfBatch.AH_GB = GlbBranch.CurrentBranch.PK;
			transactionINBatch.AH_GB = GlbBranch.CurrentBranch.PK;
			transactionINBatch2.AH_GB = GlbBranch.CurrentBranch.PK;
			transactionINBatch3.AH_GB = GlbBranch.CurrentBranch.PK;
			transactionINBatch4.AH_GB = GlbBranch.CurrentBranch.PK;

			headerOfBatch.AH_GC = GlbCompany.CurrentCompany.PK;
			transactionINBatch.AH_GC = GlbCompany.CurrentCompany.PK;
			transactionINBatch2.AH_GC = GlbCompany.CurrentCompany.PK;
			transactionINBatch3.AH_GC = GlbCompany.CurrentCompany.PK;
			transactionINBatch4.AH_GC = GlbCompany.CurrentCompany.PK;

			headerOfBatch.AH_AB = testBank.PK;
			transactionINBatch.AH_AB = testBank.PK;
			transactionINBatch2.AH_AB = testBank.PK;
			transactionINBatch3.AH_AB = testBank.PK;
			transactionINBatch4.AH_AB = testBank.PK;

			headerOfBatch.AH_PostDate = Env.Time.CurrentLocalDateTime;
			transactionINBatch.AH_PostDate = Env.Time.CurrentLocalDateTime;
			transactionINBatch2.AH_PostDate = Env.Time.CurrentLocalDateTime;
			transactionINBatch3.AH_PostDate = Env.Time.CurrentLocalDateTime;
			transactionINBatch4.AH_PostDate = Env.Time.CurrentLocalDateTime;
			Factory.Save();
		}

		void SetDDRBatchWith4Transactions(AccBankAccount testBank)
		{
			AccTransactionHeader transactionINBatch = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader transactionINBatch2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader transactionINBatch3 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader transactionINBatch4 = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionHeader headerOfBatch = Factory.NewWithValidTestData<AccTransactionHeader>();

			headerOfBatch.AH_Ledger = LedgerTypes.CashBook;
			transactionINBatch.AH_Ledger = LedgerTypes.AccountsPayable;
			transactionINBatch2.AH_Ledger = LedgerTypes.AccountsPayable;
			transactionINBatch3.AH_Ledger = LedgerTypes.AccountsPayable;
			transactionINBatch4.AH_Ledger = LedgerTypes.CashBook;

			headerOfBatch.AH_TransactionType = TransactionTypes.DDRBatch;
			transactionINBatch.AH_TransactionType = TransactionTypes.Payment;
			transactionINBatch2.AH_TransactionType = TransactionTypes.Payment;
			transactionINBatch3.AH_TransactionType = TransactionTypes.Payment;
			transactionINBatch4.AH_TransactionType = TransactionTypes.DirectPayment;

			transactionINBatch.AH_ReceiptType = ReceiptTypes.DirectDebitLine;
			transactionINBatch2.AH_ReceiptType = ReceiptTypes.DirectDebitLine;
			transactionINBatch3.AH_ReceiptType = ReceiptTypes.DirectDebitLine;
			transactionINBatch4.AH_ReceiptType = ReceiptTypes.DirectDebitLine;

			headerOfBatch.AH_ReceiptBatchNo = "155000000";
			transactionINBatch.AH_ReceiptBatchNo = headerOfBatch.AH_ReceiptBatchNo;
			transactionINBatch2.AH_ReceiptBatchNo = headerOfBatch.AH_ReceiptBatchNo;
			transactionINBatch3.AH_ReceiptBatchNo = headerOfBatch.AH_ReceiptBatchNo;
			transactionINBatch4.AH_ReceiptBatchNo = headerOfBatch.AH_ReceiptBatchNo;

			headerOfBatch.AH_GB = GlbBranch.CurrentBranch.PK;
			transactionINBatch.AH_GB = GlbBranch.CurrentBranch.PK;
			transactionINBatch2.AH_GB = GlbBranch.CurrentBranch.PK;
			transactionINBatch3.AH_GB = GlbBranch.CurrentBranch.PK;
			transactionINBatch4.AH_GB = GlbBranch.CurrentBranch.PK;

			headerOfBatch.AH_GC = GlbCompany.CurrentCompany.PK;
			transactionINBatch.AH_GC = GlbCompany.CurrentCompany.PK;
			transactionINBatch2.AH_GC = GlbCompany.CurrentCompany.PK;
			transactionINBatch3.AH_GC = GlbCompany.CurrentCompany.PK;
			transactionINBatch4.AH_GC = GlbCompany.CurrentCompany.PK;

			headerOfBatch.AH_AB = testBank.PK;
			transactionINBatch.AH_AB = testBank.PK;
			transactionINBatch2.AH_AB = testBank.PK;
			transactionINBatch3.AH_AB = testBank.PK;
			transactionINBatch4.AH_AB = testBank.PK;

			headerOfBatch.AH_PostDate = Env.Time.CurrentLocalDateTime;
			transactionINBatch.AH_PostDate = Env.Time.CurrentLocalDateTime;
			transactionINBatch2.AH_PostDate = Env.Time.CurrentLocalDateTime;
			transactionINBatch3.AH_PostDate = Env.Time.CurrentLocalDateTime;
			transactionINBatch4.AH_PostDate = Env.Time.CurrentLocalDateTime;
			Factory.Save();
		}

		class BankReconTransactionForTest : BankReconTransaction
		{
			public BankReconTransactionForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new ZDecimal Amount
			{
				get { return base.Amount; }
			}
		}

		#endregion
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	public class AutoReconcilerTest : TestCaseWithFactory
	{
		public void TestChequeReferenceMatching()
		{
			AutoReconciler reconciler = new AutoReconciler();
			AssertEquals(true, reconciler.DoesRefMatchCore_ForTestOnly("", ""));
			AssertEquals(true, reconciler.DoesRefMatchCore_ForTestOnly("1234", "1234"));

			AssertEquals(true, reconciler.DoesRefMatchCore_ForTestOnly("001234", "1234"));
			AssertEquals(true, reconciler.DoesRefMatchCore_ForTestOnly("1234", "001234"));
			AssertEquals(true, reconciler.DoesRefMatchCore_ForTestOnly("  1234", "1234"));
			AssertEquals(true, reconciler.DoesRefMatchCore_ForTestOnly("1234", "  1234"));
			AssertEquals(true, reconciler.DoesRefMatchCore_ForTestOnly("   0001234", "1234"));
			AssertEquals(true, reconciler.DoesRefMatchCore_ForTestOnly("1234", "   0001234"));

			AssertEquals(false, reconciler.DoesRefMatchCore_ForTestOnly("001234", "12340"));
			AssertEquals(false, reconciler.DoesRefMatchCore_ForTestOnly("12340", "001234"));
			AssertEquals(false, reconciler.DoesRefMatchCore_ForTestOnly("  12340", "1234"));
			AssertEquals(false, reconciler.DoesRefMatchCore_ForTestOnly("1234", "  12340"));
			AssertEquals(false, reconciler.DoesRefMatchCore_ForTestOnly("   0001234", "12340"));
			AssertEquals(false, reconciler.DoesRefMatchCore_ForTestOnly("12340", "   0001234"));
		}

		public void TestDRMatching()
		{
			BankStatement testBankStatement = TestFactory.New(typeof(BankStatement)) as BankStatement;

			ZGuid bankPK = testBankStatement.PK;

			SetStatement(testBankStatement, "DR", "CHQ", 120.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 6, 11, 10, 45));
			SetStatement(testBankStatement, "DR", "RCB", 130.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 7, 12, 23, 12));
			SetStatement(testBankStatement, "DR", "MSR", 135.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 8, 14, 12, 10));
			SetStatement(testBankStatement, "DR", "TRF", 140.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 9, 16, 30, 20));
			SetStatement(testBankStatement, "DR", "TRF", 140.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 9, 16, 30, 20));
			SetStatement(testBankStatement, "DR", "RCB", 190.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 10, 10, 42, 55));
			SetStatement(testBankStatement, "DR", "MSR", 195.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 11, 15, 35, 15));
			SetStatement(testBankStatement, "DR", "CHQ", 210.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 12, 20, 45, 27));

			BankReconTransCollection testCollection = new BankReconTransCollection(TestFactory, bankPK);

			SetReconciliation(testCollection, bankPK, TransactionTypes.Payment, 120.0m, "CR", "CHQ", testBankStatement.GetStatements_ForTestOnly()[0].AS_ChequeOrReference, ZDateTime.Now);
			SetReconciliation(testCollection, bankPK, TransactionTypes.ReceiptBatch, -130.0m, "CR", "RCB", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[1].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.ReceiptBatch, -135.0m, "CR", "MSR", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[2].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.Transfer, -140.0m, "CR", "TRF", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[1].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.Transfer, -140.0m, "CR", "TRF", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[3].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.Transfer, -140.0m, "CR", "TRF", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[3].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.Transfer, -140.0m, "CR", "TRF", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[3].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.OpeningReceipt, 190.0m, "CR", "RCB", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[5].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.OpeningReceipt, 195.0m, "CR", "MSR", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[6].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.OpeningPayment, 210.0m, "CR", "CHQ", testBankStatement.GetStatements_ForTestOnly()[7].AS_ChequeOrReference, ZDateTime.Now);

			testCollection[5].AH_Ledger = LedgerTypes.CashBook;
			testCollection[6].AH_Ledger = LedgerTypes.CashBook;
			testCollection[6].IsCleared = ZBool.True;

			LoadValues(testCollection);
			AutoReconciler testMatcher = new AutoReconciler();

			testMatcher.Match(testBankStatement.GetStatements_ForTestOnly(), testCollection);
			AssertEquals("IsCleared for Reconcilation", true, testCollection[0].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[0].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[1].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[1].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[2].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[2].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", false, testCollection[3].IsCleared);
			AssertEquals("IsCleared for Reconcilation", false, testCollection[4].IsCleared);

			AssertEquals("IsCleared for Reconcilation", false, testCollection[5].IsCleared);
			Assert("IsCleared for Statement", !testBankStatement.GetStatements_ForTestOnly()[3].AS_IsCleared);
			Assert("IsCleared for Statement", !testBankStatement.GetStatements_ForTestOnly()[4].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[6].IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[7].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[5].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[8].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[6].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[9].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[7].AS_IsCleared);

			ZDecimal balance = CalculateTransactionTotalAmount(testCollection) - CalculateStatementTotalAmount(testBankStatement);
			AssertEquals(-140.0m, balance);
		}

		public void TestDRMatchingIgnoreRef()
		{
			BankStatement testBankStatement = TestFactory.New(typeof(BankStatement)) as BankStatement;

			ZGuid bankPK = testBankStatement.PK;

			SetStatement(testBankStatement, "DR", "CHQ", 120.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 6, 11, 10, 45));
			SetStatement(testBankStatement, "DR", "RCB", 130.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 7, 12, 23, 12));
			SetStatement(testBankStatement, "DR", "MSR", 135.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 8, 14, 12, 10));
			SetStatement(testBankStatement, "DR", "TRF", 140.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 9, 16, 30, 20));
			SetStatement(testBankStatement, "DR", "TRF", 140.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 9, 16, 30, 20));
			SetStatement(testBankStatement, "DR", "RCB", 190.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 10, 10, 42, 55));
			SetStatement(testBankStatement, "DR", "MSR", 195.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 11, 15, 35, 15));
			SetStatement(testBankStatement, "DR", "CHQ", 210.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 12, 20, 45, 27));

			BankReconTransCollection testCollection = new BankReconTransCollection(TestFactory, bankPK);

			SetReconciliation(testCollection, bankPK, TransactionTypes.Payment, 120.0m, "CR", "CHQ", TestObjectCreator.GetRandomString(10), ZDateTime.Now);
			SetReconciliation(testCollection, bankPK, TransactionTypes.ReceiptBatch, -130.0m, "CR", "RCB", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[1].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.ReceiptBatch, -135.0m, "CR", "MSR", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[2].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.Transfer, -140.0m, "CR", "TRF", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[1].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.Transfer, -140.0m, "CR", "TRF", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[3].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.Transfer, -140.0m, "CR", "TRF", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[3].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.Transfer, -140.0m, "CR", "TRF", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[3].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.OpeningReceipt, 190.0m, "CR", "RCB", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[5].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.OpeningReceipt, 195.0m, "CR", "MSR", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[6].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.OpeningPayment, 210.0m, "CR", "CHQ", TestObjectCreator.GetRandomString(10), ZDateTime.Now);

			testCollection[5].AH_Ledger = LedgerTypes.CashBook;
			testCollection[6].AH_Ledger = LedgerTypes.CashBook;

			LoadValues(testCollection);
			AutoReconciler testMatcher = new AutoReconciler();

			testMatcher.Match(testBankStatement.GetStatements_ForTestOnly(), testCollection, true);
			AssertEquals("IsCleared for Reconcilation", true, testCollection[0].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[0].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[1].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[1].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[2].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[2].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", false, testCollection[3].IsCleared);
			AssertEquals("IsCleared for Reconcilation", false, testCollection[4].IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[5].IsCleared);
			AssertEquals("IsCleared for Reconcilation", true, testCollection[6].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[3].AS_IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[4].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[7].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[5].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[8].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[6].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[9].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[7].AS_IsCleared);

			ZDecimal balance = CalculateTransactionTotalAmount(testCollection) - CalculateStatementTotalAmount(testBankStatement);
			AssertEquals(0.0m, balance);
		}

		public void TestCRMatching()
		{
			BankStatement testBankStatement = TestFactory.New(typeof(BankStatement)) as BankStatement;

			ZGuid bankPK = testBankStatement.PK;

			SetStatement(testBankStatement, "CR", "CHQ", 120.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 6, 11, 10, 45));
			SetStatement(testBankStatement, "CR", "RCB", 130.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 7, 12, 23, 12));
			SetStatement(testBankStatement, "CR", "MSR", 135.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 8, 14, 12, 10));
			SetStatement(testBankStatement, "CR", "TRF", 140.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 9, 16, 30, 20));
			SetStatement(testBankStatement, "CR", "RCB", 190.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 11, 10, 42, 55));
			SetStatement(testBankStatement, "CR", "MSR", 195.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 12, 15, 35, 15));
			SetStatement(testBankStatement, "CR", "CHQ", 210.0m, TestObjectCreator.GetRandomString(10), new ZDateTime(2006, 11, 13, 20, 45, 27));

			BankReconTransCollection testCollection = new BankReconTransCollection(TestFactory, bankPK);

			SetReconciliation(testCollection, bankPK, TransactionTypes.Payment, 120.0m, "DR", "CHQ", testBankStatement.GetStatements_ForTestOnly()[0].AS_ChequeOrReference, ZDateTime.Now);
			SetReconciliation(testCollection, bankPK, TransactionTypes.ReceiptBatch, -130.0m, "DR", "RCB", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[1].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.ReceiptBatch, -135.0m, "DR", "MSR", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[2].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.Transfer, -140.0m, "DR", "TRF", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[1].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.Transfer, -140.0m, "DR", "TRF", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[3].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.Transfer, -140.0m, "DR", "TRF", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[3].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.Transfer, -140.0m, "DR", "TRF", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[3].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.OpeningReceipt, 190.0m, "DR", "RCB", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[4].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.OpeningReceipt, 195.0m, "DR", "MSR", TestObjectCreator.GetRandomString(10), testBankStatement.GetStatements_ForTestOnly()[5].AS_StatementDate);
			SetReconciliation(testCollection, bankPK, TransactionTypes.OpeningPayment, 210.0m, "DR", "CHQ", testBankStatement.GetStatements_ForTestOnly()[6].AS_ChequeOrReference, ZDateTime.Now);

			testCollection[4].AH_Ledger = LedgerTypes.CashBook;
			testCollection[5].AH_Ledger = LedgerTypes.CashBook;
			testCollection[6].AH_Ledger = LedgerTypes.CashBook;
			testCollection[6].IsCleared = ZBool.True;

			SetReconciliation(testCollection, bankPK, TransactionTypes.OpeningPayment, 120.0m, "R", "CHQ", testBankStatement.GetStatements_ForTestOnly()[0].AS_ChequeOrReference, ZDateTime.Now);

			LoadValues(testCollection);
			AutoReconciler testMatcher = new AutoReconciler();

			testMatcher.Match(testBankStatement.GetStatements_ForTestOnly(), testCollection);
			AssertEquals("IsCleared for Reconcilation", true, testCollection[0].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[0].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[1].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[1].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[2].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[2].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", false, testCollection[3].IsCleared);

			AssertEquals("IsCleared for Reconcilation", false, testCollection[4].IsCleared);
			AssertEquals("IsCleared for Reconcilation", false, testCollection[5].IsCleared);
			Assert("IsCleared for Statement", !testBankStatement.GetStatements_ForTestOnly()[3].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[6].IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[7].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[4].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[8].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[5].AS_IsCleared);

			AssertEquals("IsCleared for Reconcilation", true, testCollection[9].IsCleared);
			Assert("IsCleared for Statement", testBankStatement.GetStatements_ForTestOnly()[6].AS_IsCleared);

			ZDecimal balance = CalculateTransactionTotalAmount(testCollection) - CalculateStatementTotalAmount(testBankStatement);
			AssertEquals(140.0m, balance);
		}

		public void TestDDBMatching_DR()
		{
			BankReconTransaction debitBatchHeader = SetTransactionHeader("TEST001", 1300m, LedgerTypes.CashBook, TransactionTypes.DDRBatch, "", new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(debitBatchHeader);

			SetStatementAndAssert(transactionCollection, debitBatchHeader, ReceiptTypes.DirectDebit, "DR");
		}

		public void TestDDBMatching_CR()
		{
			BankReconTransaction debitBatchHeader = SetTransactionHeader("TEST001", -1300m, LedgerTypes.CashBook, TransactionTypes.DDRBatch, "", new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(debitBatchHeader);

			SetStatementAndAssert(transactionCollection, debitBatchHeader, ReceiptTypes.DirectDebit, "CR");
		}

		public void TestEFTMatching_DR()
		{
			BankReconTransaction eFTPayment = SetTransactionHeader("TEST001", 1300m, LedgerTypes.AccountsPayable, TransactionTypes.Payment, ReceiptTypes.EFT, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(eFTPayment);

			SetStatementAndAssert(transactionCollection, eFTPayment, ReceiptTypes.EFT, "DR");
		}

		public void TestDirectPaymentEFTMatching_DR()
		{
			BankReconTransaction eFTPayment = SetTransactionHeader("TEST001", -1300m, LedgerTypes.CashBook, TransactionTypes.DirectPayment, ReceiptTypes.EFT, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(eFTPayment);

			SetStatementAndAssert(transactionCollection, eFTPayment, ReceiptTypes.EFT, "DR");
		}

		public void TestSFTMatching_CR()
		{
			BankReconTransaction sFTPayment = SetTransactionHeader("TEST001", -1300m, LedgerTypes.AccountsPayable, TransactionTypes.Payment, ReceiptTypes.ScheduledEFT, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(sFTPayment);

			SetStatementAndAssert(transactionCollection, sFTPayment, ReceiptTypes.ScheduledEFT, "CR");
		}

		public void TestSFTMatching_DR()
		{
			BankReconTransaction sFTPayment = SetTransactionHeader("TEST001", 1300m, LedgerTypes.AccountsPayable, TransactionTypes.Payment, ReceiptTypes.ScheduledEFT, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(sFTPayment);

			SetStatementAndAssert(transactionCollection, sFTPayment, ReceiptTypes.ScheduledEFT, "DR");
		}

		public void TestCRQMatching_CR()
		{
			BankReconTransaction cRQPayment = SetTransactionHeader("TEST001", -1300m, LedgerTypes.AccountsPayable, TransactionTypes.Payment, ReceiptTypes.CollectionRequest, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(cRQPayment);

			SetStatementAndAssert(transactionCollection, cRQPayment, ReceiptTypes.CollectionRequest, "CR");
		}

		public void TestCRQMatching_DR()
		{
			BankReconTransaction cRQPayment = SetTransactionHeader("TEST001", 1300m, LedgerTypes.AccountsPayable, TransactionTypes.Payment, ReceiptTypes.CollectionRequest, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(cRQPayment);

			SetStatementAndAssert(transactionCollection, cRQPayment, ReceiptTypes.CollectionRequest, "DR");
		}

		public void TestDirectPaymentSFTMatching_CR()
		{
			BankReconTransaction sFTPayment = SetTransactionHeader("TEST001", 1300m, LedgerTypes.CashBook, TransactionTypes.DirectPayment, ReceiptTypes.ScheduledEFT, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(sFTPayment);

			SetStatementAndAssert(transactionCollection, sFTPayment, ReceiptTypes.ScheduledEFT, "CR");
		}

		public void TestDirectPaymentSFTMatching_DR()
		{
			BankReconTransaction sFTPayment = SetTransactionHeader("TEST001", -1300m, LedgerTypes.CashBook, TransactionTypes.DirectPayment, ReceiptTypes.ScheduledEFT, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(sFTPayment);

			SetStatementAndAssert(transactionCollection, sFTPayment, ReceiptTypes.ScheduledEFT, "DR");
		}

		public void TestDirectPaymentCRQMatching_CR()
		{
			BankReconTransaction cRQPayment = SetTransactionHeader("TEST001", 1300m, LedgerTypes.CashBook, TransactionTypes.DirectPayment, ReceiptTypes.CollectionRequest, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(cRQPayment);

			SetStatementAndAssert(transactionCollection, cRQPayment, ReceiptTypes.CollectionRequest, "CR");
		}

		public void TestDirectPaymentCRQMatching_DR()
		{
			BankReconTransaction cRQPayment = SetTransactionHeader("TEST001", -1300m, LedgerTypes.CashBook, TransactionTypes.DirectPayment, ReceiptTypes.CollectionRequest, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(cRQPayment);

			SetStatementAndAssert(transactionCollection, cRQPayment, ReceiptTypes.CollectionRequest, "DR");
		}

		public void TestEFTMatching_CR()
		{
			BankReconTransaction eFTPayment = SetTransactionHeader("TEST001", -1300m, LedgerTypes.AccountsPayable, TransactionTypes.Payment, ReceiptTypes.EFT, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(eFTPayment);

			SetStatementAndAssert(transactionCollection, eFTPayment, ReceiptTypes.EFT, "CR");
		}

		public void TestDirectPaymentEFTMatching_CR()
		{
			BankReconTransaction eFTPayment = SetTransactionHeader("TEST001", 1300m, LedgerTypes.CashBook, TransactionTypes.DirectPayment, ReceiptTypes.EFT, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(eFTPayment);

			SetStatementAndAssert(transactionCollection, eFTPayment, ReceiptTypes.EFT, "CR");
		}

		public void TestCCDMatching_DR()
		{
			BankReconTransaction cCDPayment = SetTransactionHeader("TEST001", 1300m, LedgerTypes.AccountsPayable, TransactionTypes.Payment, ReceiptTypes.CreditCard, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(cCDPayment);

			SetStatementAndAssert(transactionCollection, cCDPayment, ReceiptTypes.CreditCard, "DR");
		}

		public void TestCCDMatching_CR()
		{
			BankReconTransaction cCDPayment = SetTransactionHeader("TEST001", -1300m, LedgerTypes.AccountsPayable, TransactionTypes.Payment, ReceiptTypes.CreditCard, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(cCDPayment);

			SetStatementAndAssert(transactionCollection, cCDPayment, ReceiptTypes.CreditCard, "CR");
		}

		public void TestDPY_CCDMatching_DR()
		{
			BankReconTransaction cCDPayment = SetTransactionHeader("TEST001", -1300m, LedgerTypes.CashBook, TransactionTypes.DirectPayment, ReceiptTypes.CreditCard, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(cCDPayment);

			SetStatementAndAssert(transactionCollection, cCDPayment, ReceiptTypes.CreditCard, "DR");
		}

		public void TestDPY_CCDMatching_CR()
		{
			BankReconTransaction cCDPayment = SetTransactionHeader("TEST001", 1300m, LedgerTypes.CashBook, TransactionTypes.DirectPayment, ReceiptTypes.CreditCard, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(cCDPayment);

			SetStatementAndAssert(transactionCollection, cCDPayment, ReceiptTypes.CreditCard, "CR");
		}

		public void TestDDRMatching_DR()
		{
			BankReconTransaction dDRPayment = SetTransactionHeader("TEST001", 1300m, LedgerTypes.AccountsPayable, TransactionTypes.Payment, ReceiptTypes.DirectDebit, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(dDRPayment);

			SetStatementAndAssert(transactionCollection, dDRPayment, ReceiptTypes.DirectDebit, "DR");
		}

		public void TestDDRMatching_CR()
		{
			BankReconTransaction dDRPayment = SetTransactionHeader("TEST001", -1300m, LedgerTypes.AccountsPayable, TransactionTypes.Payment, ReceiptTypes.DirectDebit, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(dDRPayment);

			SetStatementAndAssert(transactionCollection, dDRPayment, ReceiptTypes.DirectDebit, "CR");
		}

		public void TestDPYDDRMatching_DR()
		{
			BankReconTransaction dDRPayment = SetTransactionHeader("TEST001", -1300m, LedgerTypes.CashBook, TransactionTypes.DirectPayment, ReceiptTypes.DirectDebit, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(dDRPayment);

			SetStatementAndAssert(transactionCollection, dDRPayment, ReceiptTypes.DirectDebit, "DR");
		}

		public void TestDPYDDRMatching_CR()
		{
			BankReconTransaction dDRPayment = SetTransactionHeader("TEST001", 1300m, LedgerTypes.CashBook, TransactionTypes.DirectPayment, ReceiptTypes.DirectDebit, new ZDateTime(2006, 11, 6, 11, 10, 45));
			BankReconTransCollection transactionCollection = SetTransactionCollection(dDRPayment);

			SetStatementAndAssert(transactionCollection, dDRPayment, ReceiptTypes.DirectDebit, "CR");
		}

		public void TestOtherBankChargeTypesMatching()
		{
			foreach (CodeDescriptionPair pair in new CodeDescriptionPairList(OLookUpEditType.BankChargeTypes))
			{
				if (pair.Code != ReceiptTypes.MiscellaneousReceipt)
				{
					BankReconTransaction transaction = SetTransactionHeader("TEST001", 1300m, "", "", pair.Code, new ZDateTime(2006, 11, 6, 11, 10, 45));
					BankReconTransCollection transactionCollection = SetTransactionCollection(transaction);

					SetStatementAndAssert(transactionCollection, transaction, pair.Code, "");
				}
			}
		}

		public void TestAmbiguousMatch()
		{
			BankStatement testBankStatement = TestFactory.New(typeof(BankStatement)) as BankStatement;

			ZGuid bankPK = testBankStatement.PK;

			SetStatement(testBankStatement, "DR", "DDR", 120.0m, "TestRef1", new ZDateTime(2006, 11, 6, 11, 10, 45));
			SetStatement(testBankStatement, "DR", "RCB", 120.0m, "TestRef1", new ZDateTime(2006, 11, 6, 11, 10, 45));
			SetStatement(testBankStatement, "DR", "MSR", 120.0m, "TestRef1", new ZDateTime(2006, 11, 6, 11, 10, 45));
			SetStatement(testBankStatement, "DR", "TRF", 120.0m, "TestRef1", new ZDateTime(2006, 11, 6, 11, 10, 45));

			BankReconTransCollection testCollection = new BankReconTransCollection(TestFactory, bankPK);

			SetReconciliation(testCollection, bankPK, TransactionTypes.Payment, 120.0m, "CR", "DDR", "TestRef1", new ZDateTime(2006, 11, 6, 11, 10, 45));
			SetReconciliation(testCollection, bankPK, TransactionTypes.DDRBatch, 120.0m, "CR", "DDR", "TestRef1", new ZDateTime(2006, 11, 6, 11, 10, 45));

			LoadValues(testCollection);
			AutoReconciler testMatcher = new AutoReconciler();

			testMatcher.Match(testBankStatement.GetStatements_ForTestOnly(), testCollection);

			Assert(testBankStatement.GetStatements_ForTestOnly()[0].AS_IsCleared);
			Assert(!testBankStatement.GetStatements_ForTestOnly()[1].AS_IsCleared);
			Assert(!testBankStatement.GetStatements_ForTestOnly()[2].AS_IsCleared);
			Assert(!testBankStatement.GetStatements_ForTestOnly()[3].AS_IsCleared);

			Assert(testCollection[0].IsCleared);
			Assert(!testCollection[1].IsCleared);
		}

		public void TestUnmatch()
		{
			BankStatement bankAccount = Factory.NewWithValidTestData<BankStatement>();
			bankAccount.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bankAccount.AB_LastReconcileDate = ZDateTime.Now;
			bankAccount.AB_LastStatementDate = bankAccount.AB_LastReconcileDate;
			SetStatement(bankAccount, true, "DR", "MSF", 100.0m, "TestRef1", bankAccount.AB_LastReconcileDate);
			Factory.Save();

			BankReconciliation bankRecon = new BankReconciliation(Factory);
			bankRecon.BankAccountPK = bankAccount.PK;
			bankRecon.ReconcileDate = bankAccount.AB_LastReconcileDate;
			bankRecon.StatementDate = bankAccount.AB_LastStatementDate;
			AssertEquals("MergedTransactions.Count", 2, bankRecon.MergedTransactions.Count);

			bankRecon.MergedTransactions[0].IsCleared = true;
			bankRecon.MergedTransactions[1].IsCleared = true;
			AssertEquals("IsCleared", true, bankRecon.MergedTransactions[0].IsCleared);
			AssertEquals("IsCleared", true, bankRecon.MergedTransactions[1].IsCleared);

			bankRecon.MergedTransactions[0].IsCleared = false;
			AssertEquals("IsCleared", false, bankRecon.MergedTransactions[0].IsCleared);
			AssertEquals("IsCleared", false, bankRecon.MergedTransactions[1].IsCleared);

			bankRecon.MergedTransactions[0].IsCleared = true;
			bankRecon.MergedTransactions[1].IsCleared = true;
			AssertEquals("IsCleared", true, bankRecon.MergedTransactions[0].IsCleared);
			AssertEquals("IsCleared", true, bankRecon.MergedTransactions[1].IsCleared);

			bankRecon.MergedTransactions[1].IsCleared = false;
			AssertEquals("IsCleared", false, bankRecon.MergedTransactions[0].IsCleared);
			AssertEquals("IsCleared", false, bankRecon.MergedTransactions[1].IsCleared);
		}

		[ExpectNoExceptions]
		public void TestMatchAcceptsNulls()
		{
			AutoReconciler reconciler = new AutoReconciler();
			reconciler.Match(null, null, false);
			reconciler.Match(null, null, true);
		}

		#region Implementation

		void SetStatementAndAssert(BankReconTransCollection transactionCollection, BankReconTransaction debitBatchHeader, ZString statementType, ZString debitCredit)
		{
			Statement matched_DDRStatement = SetStatement("TEST001", debitCredit, 1300m, statementType, new ZDateTime(2006, 11, 6, 11, 10, 0));
			Statement nonMatched_DDRStatement = SetStatement("TEST004", debitCredit, 1300m, statementType, new ZDateTime(2006, 11, 15));

			var bankAccount = (BankStatement)matched_DDRStatement.Master;
			bankAccount.AB_Desc = "AAA BANK ACCOUNT";
			bankAccount.AB_BankName = "AAA BANK";
			bankAccount.AB_BankAddress = "123 SOME STREET, SYDNEY, NSW, 2000";
			bankAccount.AB_BankAbbreviation = "AAA";
			bankAccount.StatementDateFilter = new ZDateTime(2006, 11, 6, 11, 10, 0);

			bankAccount = (BankStatement)nonMatched_DDRStatement.Master;
			bankAccount.AB_Desc = "AAA BANK ACCOUNT";
			bankAccount.AB_BankName = "AAA BANK";
			bankAccount.AB_BankAddress = "123 SOME STREET, SYDNEY, NSW, 2000";
			bankAccount.AB_BankAbbreviation = "AAA";
			bankAccount.StatementDateFilter = new ZDateTime(2006, 11, 15);

			AssertMatchingResult(matched_DDRStatement, transactionCollection, debitBatchHeader, nonMatched_DDRStatement);
		}

		void AssertMatchingResult(Statement matched_DDRStatement, BankReconTransCollection transactionCollection, BankReconTransaction debitBatchHeader, Statement nonMatched_DDRStatement)
		{
			AutoReconciler testMatcher = new AutoReconciler();

			var bankStatement = (BankStatement)matched_DDRStatement.Master;
			if (!bankStatement.GetStatements_ForTestOnly().IsLoaded)
			{ bankStatement.ApplyFilter(); }
			testMatcher.MatchStatement_ForTestOnly(matched_DDRStatement, (StatementCollection)((IBusinessObjectInternals)matched_DDRStatement).ParentCollections[0], transactionCollection, false);

			Assert(debitBatchHeader.IsCleared);
			Assert(matched_DDRStatement.AS_IsCleared);

			debitBatchHeader.IsCleared = ZBool.False;

			bankStatement = (BankStatement)nonMatched_DDRStatement.Master;
			if (!bankStatement.GetStatements_ForTestOnly().IsLoaded)
			{ bankStatement.ApplyFilter(); }
			testMatcher.MatchStatement_ForTestOnly(nonMatched_DDRStatement, (StatementCollection)((IBusinessObjectInternals)nonMatched_DDRStatement).ParentCollections[0], transactionCollection, false);

			Assert(!debitBatchHeader.IsCleared);
			Assert(!nonMatched_DDRStatement.AS_IsCleared);
		}

		BankReconTransCollection SetTransactionCollection(BankReconTransaction debitBatchHeader)
		{
			BankReconTransCollection transactionCollection = new BankReconTransCollection(Factory, ZGuid.Empty);
			transactionCollection.Add(debitBatchHeader);
			LoadValues(transactionCollection);
			return transactionCollection;
		}

		BankReconTransaction SetTransactionHeader(ZString transactionNum, ZDecimal amount, ZString ledger, ZString transactionType, ZString receiptType, ZDateTime date)
		{
			BankReconTransaction transaction = Factory.NewWithValidTestData(typeof(BankReconTransaction)) as BankReconTransaction;
			transaction.MasterBankRecon = Master;
			transaction.AH_TransactionNum = transactionNum;
			transaction.AH_ChequeOrReference = transactionNum;
			transaction.AH_OSTotal = amount;
			transaction.AH_Ledger = ledger;
			transaction.AH_TransactionType = transactionType;
			transaction.AH_ReceiptType = receiptType;
			transaction.AH_InvoiceDate = date;
			return transaction;
		}

		Statement SetStatement(ZString reference, ZString debitCredit, ZDecimal amount, ZString type, ZDateTime date)
		{
			Statement matched_DDRStatement = Factory.NewWithValidTestData(typeof(Statement)) as Statement;
			matched_DDRStatement.AS_ChequeOrReference = reference;
			matched_DDRStatement.AS_DebitCredit = debitCredit;
			matched_DDRStatement.AS_Amount = amount;
			matched_DDRStatement.AS_Type = type;
			matched_DDRStatement.AS_StatementDate = date;
			return matched_DDRStatement;
		}

		protected void LoadValues(BankReconTransCollection testCollection)
		{
			foreach (BankReconTransaction transaction in testCollection)
			{
				transaction.OnLoaded();
			}
		}

		protected ZDecimal CalculateTransactionTotalAmount(BankReconTransCollection testCollection)
		{
			ZDecimal totalDebit = 0.0m;
			ZDecimal totalCredit = 0.0m;

			foreach (BankReconTransaction transaction in testCollection)
			{
				if (transaction.IsCleared)
				{
					totalDebit += transaction.Debit;
					totalCredit += transaction.Credit;
				}
			}

			return totalDebit - totalCredit;
		}

		protected ZDecimal CalculateStatementTotalAmount(BankStatement testBankStatement)
		{
			ZDecimal totalDebit = 0.0m;
			ZDecimal totalCredit = 0.0m;

			foreach (Statement transaction in testBankStatement.GetStatements_ForTestOnly())
			{
				if (transaction.AS_IsCleared)
				{
					totalDebit += transaction.Debit;
					totalCredit += transaction.Credit;
				}
			}

			return totalDebit - totalCredit;
		}

		protected void SetStatement(BankStatement testBankStatement, ZString debitCredit, ZString type, ZDecimal amount, ZString reference, ZDateTime date)
		{
			SetStatement(testBankStatement, false, debitCredit, type, amount, reference, date);
		}

		protected void SetStatement(BankStatement testBankStatement, bool isImporting, ZString debitCredit, ZString type, ZDecimal amount, ZString reference, ZDateTime date)
		{
			Statement statement = testBankStatement.AddNewStatement();

			statement.IsImportingData = isImporting;
			statement.AS_DebitCredit = debitCredit;
			statement.AS_Type = type;
			statement.AS_Amount = amount;
			statement.AS_ChequeOrReference = reference;
			statement.AS_StatementDate = date;
		}

		protected void SetReconciliation(BankReconTransCollection testCollection, ZGuid bankPK, ZString type, ZDecimal oSTotal, ZString debitCredit, ZString receiptType, ZString chequeOrReference, ZDateTime date)
		{
			BankReconTransaction transaction = testCollection.AddNew(typeof(BankReconTransaction));

			transaction.MasterBankRecon = Master;

			transaction.AH_AB = bankPK.ToGuid();
			transaction.AH_TransactionNum = TestObjectCreator.GetRandomString(15);
			transaction.AH_TransactionType = type;
			transaction.IsCleared = ZBool.False;

			if (debitCredit == "DR")
			{
				transaction.AH_OSTotal = -oSTotal;
				transaction.AH_InvoiceAmount = -oSTotal;
			}
			else if (debitCredit == "CR")
			{
				transaction.AH_OSTotal = oSTotal;
				transaction.AH_InvoiceAmount = oSTotal;
			}

			transaction.AH_ReceiptType = receiptType;

			if (transaction.AH_TransactionType == "RCB")
			{
				transaction.AH_ReceiptBatchNo = chequeOrReference;
			}
			else
			{
				transaction.AH_ChequeOrReference = chequeOrReference;
			}

			transaction.AH_GB = GlbBranch.CurrentBranch.PK;
			transaction.AH_GE = GlbDepartment.CurrentDepartment.PK;

			if (transaction.AH_TransactionType == TransactionTypes.ReceiptBatch)
			{
				transaction.DepositBatchTransactions.AddNew(typeof(BatchTransaction));

				transaction.DepositBatchTransactions[0].AH_TransactionType = TransactionTypes.Receipt;

				if (debitCredit == "DR")
				{
					transaction.DepositBatchTransactions[0].AH_OSTotal = -oSTotal;
				}
				else if (debitCredit == "CR")
				{
					transaction.DepositBatchTransactions[0].AH_OSTotal = oSTotal;
				}

				transaction.DepositBatchTransactions[0].AH_ReceiptBatchNo = transaction.AH_ReceiptBatchNo;
			}

			transaction.AH_InvoiceDate = date;
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(TestFactory);
				}
				return fTestObjectCreator;
			}
		}
		protected TestObjectCreator fTestObjectCreator;

		protected BusinessObjectFactory TestFactory
		{
			get
			{
				if (fTestFactory == null)
				{
					fTestFactory = new BusinessObjectFactory();
				}
				return fTestFactory;
			}
		}
		protected BusinessObjectFactory fTestFactory;

		BankReconciliation Master;

		protected override void SetUp()
		{
			base.SetUp();
			Master = new BankReconciliation(Factory);
			Master.BankAccountPK = TestObjectCreator.AUDBankAccount.PK;
			Master.ReconcileDate = ZDateTime.Now;
			Master.StatementDate = Master.ReconcileDate;
		}

		#endregion
	}
}

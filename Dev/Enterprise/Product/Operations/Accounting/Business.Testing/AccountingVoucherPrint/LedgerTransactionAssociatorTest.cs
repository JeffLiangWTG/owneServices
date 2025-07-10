using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingVoucherPrint
{
	public class LedgerTransactionAssociatorTest : TestCaseWithFactory
	{
		public void TestLedgerCollection()
		{
			LedgerTransactionAssociator testAssociator = new LedgerTransactionAssociator();
			OptionalFilterCriteriaList ledgerList = testAssociator.GetLedgerList();
			AssertEquals(5, ledgerList.Count);
			AssertEquals(LedgerDescription.AccountsReceivable, ledgerList[0].Description);
			AssertEquals(LedgerDescription.AccountsPayable, ledgerList[1].Description);
			AssertEquals(LedgerDescription.Cashbook, ledgerList[2].Description);
			AssertEquals(LedgerDescription.JobCosting, ledgerList[3].Description);
			AssertEquals(LedgerDescription.GeneralLedger, ledgerList[4].Description);
		}

		public void TestLedgerAddsAR()
		{
			LedgerTransactionAssociator testAssociator = new LedgerTransactionAssociator();
			OptionalFilterCriteriaList ledgerCollection = testAssociator.GetLedgerList();
			// Select AR
			ledgerCollection[0].Enabled = true;
			OptionalFilterCriteriaList transactionTypeCollection = testAssociator.GetTransactionListByLedger(ledgerCollection, true);
			AssertEquals(11, transactionTypeCollection.Count);
		}

		public void TestLedgerAddsARAndCB()
		{
			LedgerTransactionAssociator testAssociator = new LedgerTransactionAssociator();
			OptionalFilterCriteriaList ledgerCollection = testAssociator.GetLedgerList();
			// Select AR
			ledgerCollection[0].Enabled = true;
			ledgerCollection[2].Enabled = true;
			OptionalFilterCriteriaList transactionTypeCollection = testAssociator.GetTransactionListByLedger(ledgerCollection, true);
			AssertEquals(13, transactionTypeCollection.Count);
		}

		public void TestARAPCollection()
		{
			LedgerTransactionAssociator testAssociator = new LedgerTransactionAssociator();
			OptionalFilterCriteriaList ledgerCollection = testAssociator.GetLedgerList();
			ledgerCollection[0].Enabled = true;
			OptionalFilterCriteriaList transactionTypeCollection = testAssociator.GetTransactionListByLedger(ledgerCollection, true);
			AssertEquals(11, transactionTypeCollection.Count);
			AssertEquals(TransactionDescription.Invoice, transactionTypeCollection[0].Description);
			AssertEquals(TransactionDescription.Adjustment, transactionTypeCollection[1].Description);
			AssertEquals(TransactionDescription.CreditNote, transactionTypeCollection[2].Description);
			AssertEquals(TransactionDescription.Payment, transactionTypeCollection[3].Description);
			AssertEquals(TransactionDescription.Receipt, transactionTypeCollection[4].Description);
			AssertEquals(TransactionDescription.Discount, transactionTypeCollection[5].Description);
			AssertEquals(TransactionDescription.ExchangeDiff, transactionTypeCollection[6].Description);
			AssertEquals(TransactionDescription.Overpayment, transactionTypeCollection[7].Description);
			AssertEquals(TransactionDescription.ARAPJournal, transactionTypeCollection[8].Description);
			AssertEquals(TransactionDescription.Transfer, transactionTypeCollection[9].Description);
			AssertEquals(TransactionDescription.Contra, transactionTypeCollection[10].Description);
		}

		public void TestCashbookCollection()
		{
			LedgerTransactionAssociator testAssociator = new LedgerTransactionAssociator();
			OptionalFilterCriteriaList ledgerCollection = testAssociator.GetLedgerList();
			ledgerCollection[2].Enabled = true;
			OptionalFilterCriteriaList transactionTypeCollection = testAssociator.GetTransactionListByLedger(ledgerCollection, true);
			AssertEquals(4, transactionTypeCollection.Count);
			AssertEquals(TransactionDescription.DirectReceipt, transactionTypeCollection[0].Description);
			AssertEquals(TransactionDescription.DirectPayment, transactionTypeCollection[1].Description);
			AssertEquals(TransactionDescription.ExchangeDiff, transactionTypeCollection[2].Description);
			AssertEquals(TransactionDescription.Transfer, transactionTypeCollection[3].Description);
		}

		public void TestJobCostingCollection()
		{
			LedgerTransactionAssociator testAssociator = new LedgerTransactionAssociator();
			OptionalFilterCriteriaList ledgerCollection = testAssociator.GetLedgerList();
			ledgerCollection[3].Enabled = true;
			OptionalFilterCriteriaList transactionTypeCollection = testAssociator.GetTransactionListByLedger(ledgerCollection, true);
			AssertEquals(4, transactionTypeCollection.Count);
			AssertEquals(TransactionDescription.Accrual, transactionTypeCollection[0].Description);
			AssertEquals(TransactionDescription.WIP, transactionTypeCollection[1].Description);
			AssertEquals(TransactionDescription.JobRevenueJournal, transactionTypeCollection[2].Description);
			AssertEquals(TransactionDescription.CFXJournal, transactionTypeCollection[3].Description);
		}

		public void TestGeneralLedgerCollection()
		{
			LedgerTransactionAssociator testAssociator = new LedgerTransactionAssociator();
			OptionalFilterCriteriaList ledgerCollection = testAssociator.GetLedgerList();
			ledgerCollection[4].Enabled = true;
			OptionalFilterCriteriaList transactionTypeCollection = testAssociator.GetTransactionListByLedger(ledgerCollection, true);
			AssertEquals(1, transactionTypeCollection.Count);
			AssertEquals(TransactionDescription.GeneralJournal, transactionTypeCollection[0].Description);
		}

		public void TestContraFilter()
		{
			LedgerTransactionAssociator testAssociator = new LedgerTransactionAssociator();
			OptionalFilterCriteriaList ledgerCollection = testAssociator.GetLedgerList();
			ledgerCollection[0].Enabled = true;
			OptionalFilterCriteriaList transactionTypeCollection = testAssociator.GetTransactionListByLedger(ledgerCollection, true);
			AssertEquals(11, transactionTypeCollection.Count);
			AssertEquals(TransactionDescription.Contra, transactionTypeCollection[10].Description);
			AssertEquals(testAssociator.ContraFilter_ForTestOnly.LiteralTextADO, transactionTypeCollection[10].Filter.LiteralTextADO);
		}

		public void TestContraFilterLoad()
		{
			Contra testContra = Contra.New(Factory);
			testContra.AH_Desc = "TESTContra";
			LedgerTransactionAssociator testAssociator = new LedgerTransactionAssociator();
			AccTransactionHeader result = Factory.LoadTop1(typeof(AccTransactionHeader), testAssociator.ContraFilter_ForTestOnly) as AccTransactionHeader;
			AssertEquals(LedgerTypes.AccountsPayable, result.AH_Ledger);
			AssertEquals(TransactionTypes.Contra, result.AH_TransactionType);
		}
	}
}
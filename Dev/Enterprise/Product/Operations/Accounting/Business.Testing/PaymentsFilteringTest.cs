using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing
{
	public class PaymentsFilteringTest : TestCaseWithFactory
	{
		public void TestGetPaymentsFromMixedTransctionsCollection_TransactionCreatorHashtable()
		{
			var transactions = new TransactionCreatorHashtable();
			var testPayment1 = CreatePaymentApprovalWithPayment(TestChequeBook, "1");
			var testPayment2 = CreatePaymentApprovalWithPayment(TestBookWithAutoAllocation, "2");
			var testInvoice1 = Factory.NewWithValidTestData<APInvoice>();

			transactions.AddAPInvoice(testInvoice1, "Test", "123");
			transactions.AddAPPaymentApproval(testPayment1, "Test", "TestBank", "CHQ", "123", "123");
			transactions.AddAPPaymentApproval(testPayment2, "Test", "TestBank", "CHQ", "1234", "123");

			AssertNoErrors(nameof(testPayment1), testPayment1);
			AssertNoErrors(nameof(testPayment2), testPayment2);
			Factory.Save();

			AssertNotNull(nameof(testPayment1), testPayment1.NewPayment);
			AssertNotNull(nameof(testPayment2), testPayment2.NewPayment);

			var utils = new AccountingUtils();
			var filteredTransactions = utils.GetPaymentsFromMixedTransactionsCollection_ForTestOnly(transactions, Factory);
			AssertEquals("Collection should contain only 1 transaction", 1, filteredTransactions.Count);
			Assert("Collection should contain TestPayment2", filteredTransactions.Contains(testPayment2.NewPayment));
		}

		public void TestGetPaymentsFromMixedTransctionsCollection_APPaymentApprovalWithoutAuthorisationCollection()
		{
			var transactions = new APPaymentApprovalWithoutAuthorisationCollection(Factory);
			var testApproval1 = CreatePaymentApprovalWithPayment(TestBookWithAutoAllocation);
			var testApproval2 = CreatePaymentApprovalWithPayment(TestBookWithAutoAllocation);
			var testApproval3 = CreatePaymentApprovalWithPayment(TestChequeBook);
			transactions.Add(testApproval1);
			transactions.Add(testApproval2);
			transactions.Add(testApproval3);

			var utils = new AccountingUtils();
			var filteredTransactions = utils.GetPaymentsFromMixedTransactionsCollection_ForTestOnly(transactions, Factory);
			AssertEquals("Collection should contain only 2 transactions", 2, filteredTransactions.Count);
			Assert("Collection should contain TestPayment1", filteredTransactions.Contains(testApproval1.NewPayment));
			Assert("Collection should contain TestPayment2", filteredTransactions.Contains(testApproval2.NewPayment));
		}

		public void TestGetPaymentsFromMixedTransctionsCollection_TransactionHeaderCollection()
		{
			var transactions = new TransactionHeaderCollection(Factory);
			var testPayment1 = Factory.NewWithValidTestData<APPayment>();
			transactions.Add(testPayment1);

			var utils = new AccountingUtils();
			var filteredTransactions = utils.GetPaymentsFromMixedTransactionsCollection_ForTestOnly(transactions, Factory);
			AssertEquals("Should return the same collection", transactions.Count, filteredTransactions.Count);
			AssertArrayEqualsByElements("Should return the same collection", transactions.ToArray(), filteredTransactions.ToArray());
			filteredTransactions = utils.GetPaymentsFromMixedTransactionsCollection_ForTestOnly(new PaymentApprovalCollection(Factory), Factory);
			AssertEquals("Unsupported type of collection - should return empty list ", 0, filteredTransactions.Count);
		}

		public void TestSplitTransactionBatchOnCollectionByChequeBookParameter()
		{
			var testBookWithAutoAllocation2 = GetAutoPrintChequeBook(BankAccount, 1, 3, 2);
			//Group of TestBookWithAutoAllocation
			var testPayment1 = CreatePaymentApprovalWithPayment(TestBookWithAutoAllocation);
			var testPayment2 = CreatePaymentApprovalWithPayment(TestBookWithAutoAllocation);

			//Group of TestBookWithAutoAllocation2
			var testPayment3 = CreatePaymentApprovalWithPayment(testBookWithAutoAllocation2);

			var transactions = new TransactionCreatorHashtable();
			transactions.AddAPPaymentApproval(testPayment1, "Test", "TestBank", "CHQ", "123", "123");
			transactions.AddAPPaymentApproval(testPayment2, "Test", "TestBank", "CHQ", "1234", "123");
			transactions.AddAPPaymentApproval(testPayment3, "Test", "TestBank", "CHQ", "12345", "123");
			var utils = new AccountingUtils();
			var splitTransactions = utils.SplitTransactionBatchOnCollectionByChequeBookParameter(transactions, Factory);
			AssertEquals("SplitTransactions should contain 2 groups", 2, splitTransactions.Count);

			var chequeBookGroup = GetTransactionGroupByCountOfRecords(splitTransactions, 2);
			AssertNotNull("Group #1 should contain 2 transactions", chequeBookGroup);
			Assert("Group #1 should contain TestPayment1", chequeBookGroup.Contains(testPayment1.NewPayment));
			Assert("Group #1 should contain TestPayment2", chequeBookGroup.Contains(testPayment2.NewPayment));

			chequeBookGroup = GetTransactionGroupByCountOfRecords(splitTransactions, 1);
			AssertNotNull("Group #1 should contain 1 transaction", chequeBookGroup);
			Assert("Group #1 should contain TestPayment3", chequeBookGroup.Contains(testPayment3.NewPayment));
		}

		#region Implementation

		IEnumerable<TransactionHeader> GetTransactionGroupByCountOfRecords(List<List<TransactionHeader>> splitTransactions, ZInt countOfRecords)
		{
			foreach (var transactionCollection in splitTransactions)
			{
				if (transactionCollection.Count == countOfRecords)
				{
					return transactionCollection;
				}
			}
			return null;
		}

		APPaymentApprovalWithoutAuthorisation CreatePaymentApprovalWithPayment(AccChequeBook chequeBook, string matchTransactionNum = "1")
		{
			APPaymentApprovalWithoutAuthorisation testPaymentApproval = Factory.New<APPaymentApprovalWithoutAuthorisation>();
			testPaymentApproval.AV_OH = TestOrgHeader.PK;
			testPaymentApproval.AV_AB = BankAccount.PK;
			testPaymentApproval.AV_AK = chequeBook.PK;
			testPaymentApproval.AV_Amount = 100m;
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.FillPaymentMatchTransactions(matchTransactionNum, typeof(APInvoice), TestOrgHeader, testPaymentApproval, 100m);
			testPaymentApproval.CreateNewPayment();
			AssertNotNull("New payment should be created", testPaymentApproval.NewPayment);
			Assert("New Payment should not be deleted yet", !testPaymentApproval.NewPayment.IsDeleted);

			return testPaymentApproval;
		}

		AccChequeBook GetAutoPrintChequeBook(AccBankAccount bankAccount, ZDecimal startNO, ZDecimal lastNO, ZDecimal currentNO)
		{
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNO;
			chequeBook.AK_LastNo = lastNO;
			chequeBook.AK_CurrentNo = currentNO;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			return chequeBook;
		}

		AccBankAccount BankAccount;
		AccChequeBook TestChequeBook;
		AccChequeBook TestBookWithAutoAllocation;
		OrgHeader TestOrgHeader;

		void PrepareBaseObjects()
		{
			TestOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			TestOrgHeader.OH_IsCreditor = true;
			BankAccount = Factory.NewWithValidTestData<AccBankAccount>();

			TestChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			TestChequeBook.AK_StartNo = 1;
			TestChequeBook.AK_LastNo = 3;
			TestChequeBook.AK_CurrentNo = 2;
			TestChequeBook.AK_AB = BankAccount.PK;

			TestBookWithAutoAllocation = GetAutoPrintChequeBook(BankAccount, 1, 3, 2);

			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			PrepareBaseObjects();
		}

		#endregion
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	[TestedType(typeof(ARAPJournalVoucherProivder))]
	public class ARAPJournalVoucherProivderTest : TransactionWithoutLinesVoucherProviderTest
	{
		public void TestLineNoGenerated()
		{
			AssertEquals(2, TestProvider.VoucherLines.Length);
		}

		public void TestAccountNumber()
		{
			TestTransaction.AH_AG = GLAccount1.PK;
			AssertEquals(LocalAccountNumber1, TestProvider.VoucherLines[0].AccountNumber);
		}

		public void TestVoucherNumber()
		{
			TestTransaction.AH_TransactionNum = "TESTTRAN001";
			AssertEquals(TestTransaction.AH_TransactionNum, TestProvider.VoucherLines[0].VoucherNumber);
		}

		public void TestVoucherType()
		{
			//string ExpectedVoucherType = "应收帐款-JNL";
			string expectedVoucherType = "AR-JNL";
			AssertEquals(expectedVoucherType, TestProvider.VoucherLines[0].VoucherType);
			AssertEquals(expectedVoucherType, TestProvider.VoucherLines[1].VoucherType);
		}

		public void TestVoucherAmount()
		{
			TestTransaction.AH_InvoiceAmount = -120m;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			TestTransaction.AH_InvoiceAmount = 120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			TestTransaction.AH_Ledger = LedgerTypes.AccountsPayable;
			TestTransaction.AH_InvoiceAmount = 120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			TestTransaction.AH_InvoiceAmount = -120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
		}

		public void TestVoucherAmountForControlAccount()
		{
			TestTransaction.AH_InvoiceAmount = 120m;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			TestTransaction.AH_InvoiceAmount = -120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			TestTransaction.AH_Ledger = LedgerTypes.AccountsPayable;
			TestTransaction.AH_InvoiceAmount = -120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			TestTransaction.AH_InvoiceAmount = 120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
		}

		protected override VoucherProvider GetVoucherProvider(AccTransactionHeader transaction)
		{
			return new ARAPJournalVoucherProivder(transaction);
		}

		protected override AccTransactionHeader GetTestTransaction()
		{
			return Factory.NewWithValidTestData(typeof(ARJournal)) as TransactionHeader;
		}

		protected override ZInt GetExpectedAttachementNo()
		{
			return 0;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ARAPJournalVoucherProivder(Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestProvider = GetVoucherProvider(TestTransaction);
		}
	}
}

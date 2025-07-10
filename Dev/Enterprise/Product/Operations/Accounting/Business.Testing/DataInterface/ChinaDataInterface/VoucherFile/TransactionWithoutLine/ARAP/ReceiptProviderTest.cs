using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	[TestedType(typeof(ReceiptPaymentVoucherProvider))]
	public class ReceiptProviderTest : TransactionWithoutLinesVoucherProviderTest
	{
		public void TestLineNoGenerated()
		{
			AssertEquals(2, TestProvider.VoucherLines.Length);
		}

		public void TestAccountNumber()
		{
			TestTransaction.AH_AB = Bank1.PK;
			AssertEquals(LocalAccountNumber1, TestProvider.VoucherLines[0].AccountNumber);
		}

		public void TestVoucherType()
		{
			//string ExpectedVoucherType = "应收帐款-收款单";
			string expectedVoucherType = "AR-REC";
			AssertEquals(expectedVoucherType, TestProvider.VoucherLines[0].VoucherType);
			AssertEquals(expectedVoucherType, TestProvider.VoucherLines[1].VoucherType);
		}

		public void TestVoucherNumber()
		{
			TestTransaction.AH_TransactionNum = "TESTTRAN001";
			AssertEquals(TestTransaction.AH_TransactionNum, TestProvider.VoucherLines[0].VoucherNumber);
		}

		public void TestVoucherAmountForReceipt()
		{
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_OSTotal = -120m;
			TestTransaction.AH_InvoiceAmount = -120m;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[0].ForeignCurrencyAmount);
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_OSTotal = 120m;
			TestTransaction.AH_InvoiceAmount = 120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[1].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[1].ForeignCurrencyAmount);
			TestTransaction.AH_Ledger = LedgerTypes.AccountsPayable;
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_OSTotal = 120m;
			TestTransaction.AH_InvoiceAmount = 120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[1].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[1].ForeignCurrencyAmount);
		}

		public void TestVoucherAmountForReceiptControlAccount()
		{
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_InvoiceAmount = -120m;
			TestTransaction.AH_OSTotal = -120m;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[1].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[1].ForeignCurrencyAmount);
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_InvoiceAmount = 120m;
			TestTransaction.AH_OSTotal = -120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[0].ForeignCurrencyAmount);
			TestTransaction.AH_Ledger = LedgerTypes.AccountsPayable;
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_InvoiceAmount = 120m;
			TestTransaction.AH_OSTotal = -120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[0].ForeignCurrencyAmount);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ReceiptPaymentVoucherProvider(Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
		}

		protected override VoucherProvider GetVoucherProvider(AccTransactionHeader transaction)
		{
			return new ReceiptPaymentVoucherProvider(transaction);
		}

		protected override AccTransactionHeader GetTestTransaction()
		{
			return InsertARReceipt(Factory);
		}

		protected override ZInt GetExpectedAttachementNo()
		{
			return 1;
		}
	}

	[TestedType(typeof(ReceiptPaymentVoucherProvider))]
	public class PaymentVoucherTest : VoucherProviderTestCase
	{
		public void TestVoucherTypeForPayment()
		{
			//string ExpectedVoucherType = "应付帐款-付款单";
			string expectedVoucherType = "AP-PAY";
			AssertEquals(expectedVoucherType, TestProvider.VoucherLines[0].VoucherType);
		}

		public void TestVoucherNumberForPayment()
		{
			TestTransaction.AH_TransactionNum = "TESTTRAN001";
			AssertEquals(TestTransaction.AH_TransactionNum, TestProvider.VoucherLines[0].VoucherNumber);
		}

		public void TestVoucherAmountForPayment()
		{
			TestTransaction.AH_InvoiceAmount = 120m;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			TestTransaction.AH_InvoiceAmount = -120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			TestTransaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			TestTransaction.AH_InvoiceAmount = 120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
		}

		public void TestVoucherAmountForPaymentControlAccount()
		{
			TestTransaction.AH_InvoiceAmount = 120m;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			TestTransaction.AH_InvoiceAmount = -120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			TestTransaction.AH_Ledger = LedgerTypes.AccountsReceivable;
			TestTransaction.AH_InvoiceAmount = 120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
		}

		public void TestControlAccountNumber()
		{
			var mockControlAccount = new Mock<IControlAccountProvider>(MockBehavior.Strict);
			ReceiptPaymentVoucherProvider testProvider = new ReceiptPaymentVoucherProvider(TestTransaction);
			mockControlAccount.Setup(m => m.SetTransaction(TestTransaction));
			mockControlAccount.Setup(m => m.PK).Returns(ControlAccount.PK);
			testProvider.ControlAccountProvider = mockControlAccount.Object;
			AssertEquals(ControlAccountNumber, testProvider.VoucherLines[1].AccountNumber);
		}

		protected override VoucherProvider GetVoucherProvider(AccTransactionHeader transaction)
		{
			return new ReceiptPaymentVoucherProvider(transaction);
		}

		protected override AccTransactionHeader GetTestTransaction()
		{
			return InsertAPPayment(Factory);
		}

		protected override ZInt GetExpectedAttachementNo()
		{
			return 1;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ReceiptPaymentVoucherProvider(Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
		}
	}
}

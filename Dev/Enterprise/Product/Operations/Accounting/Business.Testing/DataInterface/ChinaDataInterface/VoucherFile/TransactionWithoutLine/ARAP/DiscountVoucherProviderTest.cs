using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	[TestedType(typeof(DiscountVoucherProvider))]
	public class DiscountVoucherProviderTest : TransactionWithoutLinesVoucherProviderTest
	{
		public void TestLineNoGenerated()
		{
			AssertEquals(2, TestProvider.VoucherLines.Length);
		}

		public void TestAccountNumber()
		{
			((DiscountVoucherProvider)TestProvider).fGLAccount = GLAccount1.PK;
			AssertEquals(LocalAccountNumber1, TestProvider.VoucherLines[0].AccountNumber);
		}

		public void TestVoucherNumber()
		{
			TestTransaction.AH_TransactionNum = "TESTTRAN001";
			AssertEquals(TestTransaction.AH_TransactionNum, TestProvider.VoucherLines[0].VoucherNumber);
		}

		public void TestVoucherTypeForARDSC()
		{
			string expectedVoucherType = "AR-DSC";
			AssertEquals(expectedVoucherType, TestProvider.VoucherLines[0].VoucherType);
			AssertEquals(expectedVoucherType, TestProvider.VoucherLines[1].VoucherType);
		}

		public void TestVoucherTypeForAPDSC()
		{
			string expectedVoucherType = "AP-DSC";
			TestTransaction.AH_Ledger = LedgerTypes.AccountsPayable;
			AssertEquals(expectedVoucherType, TestProvider.VoucherLines[0].VoucherType);
			AssertEquals(expectedVoucherType, TestProvider.VoucherLines[1].VoucherType);
		}

		public void TestVoucherAmount()
		{
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_InvoiceAmount = 120m;
			TestTransaction.AH_OSTotal = 120m;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[1].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[1].ForeignCurrencyAmount);
			TestTransaction.AH_InvoiceAmount = -120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[0].ForeignCurrencyAmount);
			TestTransaction.AH_Ledger = LedgerTypes.AccountsPayable;
			TestTransaction.AH_InvoiceAmount = 120m;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[1].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[1].ForeignCurrencyAmount);
		}

		public void TestVoucherAmountForControlAccount()
		{
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_OSTotal = 120m;
			TestTransaction.AH_InvoiceAmount = 120m;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[0].ForeignCurrencyAmount);
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_OSTotal = -120m;
			TestTransaction.AH_InvoiceAmount = -120m;
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
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[0].ForeignCurrencyAmount);
		}

		public void TestGLAccount()
		{
			TestTransaction.AH_AG = TestObjectCreator.GLHeader1.PK;
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;

			AssertEquals(TestTransaction.AH_AG, TestProvider.VoucherLines[0].AccountPK);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DiscountVoucherProvider(Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestProvider = GetVoucherProvider(TestTransaction) as TransactionWithoutLinesVoucherProvider;
		}

		protected override VoucherProvider GetVoucherProvider(AccTransactionHeader transaction)
		{
			return new DiscountVoucherProvider(transaction);
		}

		protected override AccTransactionHeader GetTestTransaction()
		{
			AccTransactionHeader transaction = Factory.NewWithValidTestData(typeof(ARDiscount)) as TransactionHeader;
			return transaction;
		}

		protected override ZInt GetExpectedAttachementNo()
		{
			return 0;
		}

		protected override bool OrganisationCodeIsApplicableToThisVoucher
		{
			get
			{
				return false;
			}
		}
	}
}

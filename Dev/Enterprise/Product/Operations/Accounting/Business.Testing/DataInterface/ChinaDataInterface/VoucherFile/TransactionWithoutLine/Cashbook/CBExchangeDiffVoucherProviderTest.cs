using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	[TestedType(typeof(CBExchangeDiffVoucherProvider))]
	public class CBExchangeDiffVoucherProviderTest : TransactionWithoutLinesVoucherProviderTest
	{
		protected override void CoreTestForVoucherCurrency(RefCurrency currency)
		{
			base.CoreTestForVoucherCurrency(GlbCompany.CurrentCompany.LocalCurrency);
		}

		public void TestCurrencyCodeExchangeRate()
		{
			TestTransaction.AH_RX_NKTransactionCurrency = "USD";
			TestTransaction.AH_InvoiceAmount = 100m;
			TestTransaction.AH_ExchangeRate = 1.2m;
			TestTransaction.AH_OSTotal = 120m;
			AssertEquals(100m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			AssertEquals(100m, TestProvider.VoucherLines[0].OSDebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].OSCreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestProvider.VoucherLines[0].CurrencyCode);
			TestTransaction.AH_InvoiceAmount = -100m;
			TestTransaction.AH_ExchangeRate = 1.2m;
			TestTransaction.AH_OSTotal = -120m;
			TestProvider = GetVoucherProvider(TestTransaction) as CBExchangeDiffVoucherProvider;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(100m, TestProvider.VoucherLines[1].CreditAmount);
			AssertEquals(0m, TestProvider.VoucherLines[1].OSDebitAmount);
			AssertEquals(100m, TestProvider.VoucherLines[1].OSCreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[1].ExchangeRate);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestProvider.VoucherLines[0].CurrencyCode);
		}

		public void TestAccountNumber()
		{
			TestTransaction.AH_AB = Bank1.PK;
			AssertEquals(LocalAccountNumber1, TestProvider.VoucherLines[0].AccountNumber);
		}

		public void TestAmount()
		{
			TestTransaction.AH_InvoiceAmount = 120m;
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_OSTotal = 120m;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[0].ForeignCurrencyAmount);
			TestTransaction.AH_InvoiceAmount = -120m;
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_OSTotal = -120m;
			TestProvider = GetVoucherProvider(TestTransaction) as CBExchangeDiffVoucherProvider;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[1].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[1].ForeignCurrencyAmount);
		}

		public void TestControlAmount()
		{
			TestTransaction.AH_InvoiceAmount = -120m;
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_OSTotal = -120m;
			AssertEquals(120m, TestProvider.VoucherLines[0].DebitAmount);
			AssertEquals(0m, TestProvider.VoucherLines[0].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[0].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[0].ForeignCurrencyAmount);
			TestTransaction.AH_InvoiceAmount = 120m;
			TestTransaction.AH_ExchangeRate = 1m;
			TestTransaction.AH_OSTotal = -120m;
			TestProvider = GetVoucherProvider(TestTransaction) as CBExchangeDiffVoucherProvider;
			AssertEquals(0m, TestProvider.VoucherLines[1].DebitAmount);
			AssertEquals(120m, TestProvider.VoucherLines[1].CreditAmount);
			AssertEquals(1m, TestProvider.VoucherLines[1].ExchangeRate);
			AssertEquals(120m, TestProvider.VoucherLines[1].ForeignCurrencyAmount);
		}

		public void TestControlAccountNumber()
		{
			var mockControlAccount = new Mock<IControlAccountProvider>(MockBehavior.Strict);
			CBExchangeDiffVoucherProvider testProvider = new CBExchangeDiffVoucherProvider(TestTransaction);
			mockControlAccount.Setup(m => m.SetTransaction(TestTransaction));
			mockControlAccount.Setup(m => m.PK).Returns(ControlAccount.PK);
			testProvider.ControlAccountProvider = mockControlAccount.Object;
			AssertEquals(ControlAccountNumber, testProvider.VoucherLines[1].AccountNumber);
		}

		protected override VoucherProvider GetVoucherProvider(AccTransactionHeader transaction)
		{
			return new CBExchangeDiffVoucherProvider(transaction);
		}

		protected override AccTransactionHeader GetTestTransaction()
		{
			AccTransactionHeader headerToReturn = TestObjectCreator.InsertTransaction(TransactionTypes.ExchangeDifference, LedgerTypes.CashBook);
			headerToReturn.AH_TransactionNum = new Random().Next(100000).ToString();
			headerToReturn.AH_AG = Factory.LoadTop1<AccGLHeader>(new ZQuery()).PK;
			return headerToReturn;
		}

		protected override ZInt GetExpectedAttachementNo()
		{
			return 0;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CBExchangeDiffVoucherProvider(Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestProvider = GetVoucherProvider(TestTransaction) as CBExchangeDiffVoucherProvider;
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

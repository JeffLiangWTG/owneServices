using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.CashBook.Transfer
{
	public sealed class BankTransferFundingCurrencyProviderTest : TestCaseWithFactory
	{
		public void TestConfigureBankTransferFromPaymentBatch()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 0.5m);
			var paymentBatch = CreatePaymentBatch(TestObjectCreator.USDBankAccount.PK);
			var payment = CreatePayment();
			var approval = CreatePaymentApproval(paymentBatch.PK, payment.PK);
			var quote = CreateQuote(approval, "USD");
			AssertEquals(20m, quote.QU_FeeAmount);
			Factory.Save();

			var bankTransfer = new BankTransfer(Factory, null);
			bankTransfer.BankTransferToPK = TestObjectCreator.AUDBankAccount.PK;
			var provider = new BankTransferFundingInfoCalculator(bankTransfer);
			provider.ConfigureBankTransferFromPaymentBatch(paymentBatch, new List<Payment> { payment });
			AssertEquals("USD", provider.FundingCurrency);
			AssertEquals(420m, bankTransfer.SellAmount);
			AssertEquals(440m, bankTransfer.BuyAmount);
			AssertEquals(40m, bankTransfer.FinanceChargeOSAmount);
		}

		public void TestCalculateBankTransferWithFundingCurrency()
		{
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 0.5m);
			var paymentBatch = CreatePaymentBatch(TestObjectCreator.USDBankAccount.PK);
			var payment = CreatePayment();
			var approval = CreatePaymentApproval(paymentBatch.PK, payment.PK);
			var quote = CreateQuote(approval, "USD");
			AssertEquals(20m, quote.QU_FeeAmount);
			Factory.Save();

			var bankTransfer = new BankTransfer(Factory, null);
			bankTransfer.BankTransferToPK = TestObjectCreator.AUDBankAccount.PK;
			var provider = new BankTransferFundingInfoCalculator(bankTransfer);
			provider.ConfigureBankTransferFromPaymentBatch(paymentBatch, new List<Payment> { payment });
			AssertEquals("USD", provider.FundingCurrency);
			AssertEquals(420m, bankTransfer.SellAmount);
			AssertEquals(440m, bankTransfer.BuyAmount);
			AssertEquals(40m, bankTransfer.FinanceChargeOSAmount);

			bankTransfer.SellExchangeRate = 0.1m;
			provider.CalculateBankTransferWithFundingCurrency();
			AssertEquals(420m, bankTransfer.SellAmount);
			AssertEquals(600m, bankTransfer.BuyAmount);
			AssertEquals(200m, bankTransfer.FinanceChargeOSAmount);
		}

		APPaymentApprovalWithoutAuthorisation CreatePaymentApproval(ZGuid paymentBatchPK, ZGuid paymentPK)
		{
			var approval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval.AV_AH = paymentPK;
			approval.AV_RX_NKPaymentCurrency = "CAD";
			approval.AV_Amount = 200m;
			approval.AV_PayExRate = 0.5m;
			approval.InitializeForPaymentBatch(() => false);
			approval.AV_APB_PaymentBatch = paymentBatchPK;
			return approval;
		}

		APPayment CreatePayment()
		{
			var payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			payment.AH_GC = GlbCompany.CurrentCompany.PK;
			return payment;
		}

		APPaymentBatchPoster CreatePaymentBatch(ZGuid fundingBankAccountPK)
		{
			var paymentBatch = Factory.New<APPaymentBatchPoster>();
			paymentBatch.APB_AB_FundingBankAccount = fundingBankAccountPK;
			paymentBatch.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			return paymentBatch;
		}

		AccEPaymentQuote CreateQuote(APPaymentApprovalWithoutAuthorisation approval, ZString fromCurrency)
		{
			var deal = TestObjectCreator.CreateValidEPaymentDealForStatus(EPaymentStatusCodes.Deal.Accepted, approval);
			deal.Quote.QU_FeeAmount = 20m;
			deal.Quote.QU_FromAmount = 400m;
			deal.Quote.QU_ExchangeRate = 0.5m;
			deal.Quote.QU_ExchangeRateInverted = 2m;
			deal.Quote.QU_ToAmount = 200m;
			deal.Quote.QU_RX_NKFromCurrency = fromCurrency;
			deal.Quote.QU_RX_NKToCurrency = "CAD";
			return deal.Quote;
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}

		TestObjectCreator testObjectCreator;
	}
}

using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.CashBook.Transfer
{
	public class BankTransferFundingInfoCalculator
	{
		public BankTransferFundingInfoCalculator(BankTransfer bankTransfer)
		{
			this.bankTransfer = bankTransfer;
		}

		public void ConfigureBankTransferFromPaymentBatch(APPaymentBatchPoster paymentBatch, IEnumerable<Payment> paymentsWithTheSameBankAccount)
		{
			if (paymentBatch == null)
			{
				return;
			}

			if (paymentBatch.FundingBankAccountCurrency != Env.CurrentCompany.LocalCurrency.Code)
			{
				bankTransfer.ShouldCalculateExchangeVariance = true;
			}

			FundingCurrency = paymentBatch.FundingBankAccountCurrency;
			bankTransfer.BankTransferFromPK = paymentBatch.APB_AB_FundingBankAccount;

			CalculateBankTransferFromPayments(paymentsWithTheSameBankAccount);
			CalculateBankTransferWithFundingCurrency();
		}

		public void ConfigureBankTransferFromSinglePayments(IEnumerable<Payment> paymentsWithTheSameBankAccount)
		{
			if (!paymentsWithTheSameBankAccount.Any())
			{
				return;
			}

			var firstPayment = paymentsWithTheSameBankAccount.First();
			if (firstPayment.FundingCurrency != Env.CurrentCompany.LocalCurrency.Code)
			{
				bankTransfer.ShouldCalculateExchangeVariance = true;
			}

			FundingCurrency = firstPayment.FundingCurrency;
			bankTransfer.BankTransferFromPK = firstPayment.FundingBankAccountPK;

			CalculateBankTransferFromPayments(paymentsWithTheSameBankAccount);
			CalculateBankTransferWithFundingCurrency();
		}

		public void CalculateBankTransferWithFundingCurrency()
		{
			if (FundingCurrency != Env.CurrentCompany.LocalCurrency.Code)
			{
				bankTransfer.FinanceChargeOSAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(financeChargeInFundingCurrency, bankTransfer.SellExchangeRate);
			}
			else
			{
				bankTransfer.FinanceChargeOSAmount = financeChargeInFundingCurrency;
			}

			bankTransfer.BuyAmount = totalLocalCost + bankTransfer.FinanceChargeOSAmount;
		}

		void CalculateBankTransferFromPayments(IEnumerable<Payment> paymentsWithTheSameBankAccount)
		{
			totalLocalCost = paymentsWithTheSameBankAccount.Sum(x => x.RelatedPaymentApproval.AV_Calc_LocalAmount);
			financeChargeInFundingCurrency = paymentsWithTheSameBankAccount.Sum(x => x.DealTotalFees);
			bankTransfer.SellAmount = paymentsWithTheSameBankAccount.Sum(x => x.DealTotalCost);
			bankTransfer.BankTransferToPK_ReadOnly = true;
		}

		public ZString FundingCurrency { get; private set; }

		readonly BankTransfer bankTransfer;
		ZDecimal totalLocalCost;
		ZDecimal financeChargeInFundingCurrency;
	}
}

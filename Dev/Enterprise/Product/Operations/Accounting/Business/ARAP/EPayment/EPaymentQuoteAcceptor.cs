using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.EPayment;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public static class EPaymentQuoteAcceptor
	{
		public static (bool isQuoteAccepted, string userMessage) AcceptEPaymentQuote(EPaymentQuote quoteToAccept, PaymentApprovalBase paymentApproval, bool saveFactory = true)
		{
			Argument.NotNull(quoteToAccept, "quoteToAccept");
			Argument.NotNull(paymentApproval, "paymentApproval");

			var isQuoteAccepted = false;
			string userMessage;

			if (quoteToAccept.QU_Status == EPaymentStatusCodes.Quote.Received)
			{
				if (!quoteToAccept.QU_ProviderReference.IsEmpty)
				{
					var fundingCurrency = EPaymentFundingInfoProviderFactory.CreateProvider(paymentApproval).GetFundingCurrency();
					if (quoteToAccept.QU_RX_NKFromCurrency == fundingCurrency)
					{
						AcceptQuoteCore(quoteToAccept, paymentApproval);
						userMessage = Res.GetString("127922e6-eac0-47bf-bb79-46a66358f1ab", "Quote {0} has been accepted.", quoteToAccept.QU_InternalReference);
						isQuoteAccepted = true;
					}
					else
					{
						quoteToAccept.QU_Status = EPaymentStatusCodes.Quote.Discarded;
						userMessage = Res.GetString("8F56D1D2-E99E-40D1-ADD8-5DCB27A5CB55", "This quote is no longer valid. Funding Currency of the payment does not match quote currency.");
					}
					if (saveFactory)
					{
						try
						{
							paymentApproval.Factory.Save();
						}
						catch (ZCannotSaveException ex)
						{
							isQuoteAccepted = false;
							userMessage = ex.Message;
						}
					}
				}
				else
				{
					userMessage = Res.GetString("1d8b3ff8-49f3-4101-84f5-69a0bb321c7b", "This is indicative rate only. To request a formal quote, please select an {0} E-Payment Account as the Bank Account for this payment, and ensure you have authorized your {0} User Account.", quoteToAccept.QU_ProviderCode);
				}
			}
			else
			{
				userMessage = Res.GetString("9f8d0b48-e34f-4036-aae2-17cd5bf30d89", "This quote is not in Received status.");
			}
			return (isQuoteAccepted, userMessage);
		}

		public static string AcceptEPaymentQuoteForAcceptedDeal(EPaymentQuote quoteToAccept, PaymentApprovalBase paymentApproval)
		{
			Argument.NotNull(quoteToAccept, "quoteToAccept");
			Argument.NotNull(paymentApproval, "paymentApproval");

			var warningMessage = ZString.Empty;
			var fundingCurrency = EPaymentFundingInfoProviderFactory.CreateProvider(paymentApproval).GetFundingCurrency();
			if (quoteToAccept.QU_RX_NKFromCurrency != fundingCurrency)
			{
				warningMessage = Res.GetString("998E1CA7-2C79-44B2-95DE-77F4F4492744", "Funding Currency does not match quote currency.");
			}
			AcceptQuoteCore(quoteToAccept, paymentApproval);
			return warningMessage;
		}

		static void AcceptQuoteCore(EPaymentQuote quoteToAccept, PaymentApprovalBase paymentApproval)
		{
			ApplyQuoteDetailsToPayment(quoteToAccept, paymentApproval);
			if (quoteToAccept.QU_RX_NKFromCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				UpdateExchangeDifferenceIfRequired(paymentApproval);
			}
			DiscardAllOtherQuotesFromAllProviders(quoteToAccept, paymentApproval);
			quoteToAccept.QU_Status = EPaymentStatusCodes.Quote.Accepted;
		}

		static void ApplyQuoteDetailsToPayment(EPaymentQuote quoteToAccept, PaymentApprovalBase paymentApproval)
		{
			paymentApproval.AV_RX_NKPaymentCurrency = quoteToAccept.QU_RX_NKToCurrency;
			paymentApproval.AV_Amount = quoteToAccept.QU_ToAmount;
			if (quoteToAccept.QU_RX_NKFromCurrency == paymentApproval.AV_Calc_LocalCurrency)
			{
				paymentApproval.AV_Calc_LocalAmount = quoteToAccept.QU_FromAmount; //local amount setter will recalculate the exchange rate.
			}
		}

		static void UpdateExchangeDifferenceIfRequired(PaymentApprovalBase paymentApproval)
		{
			//If PaymentMatchingBaseObject is not already created, this will create PaymentMatchingBaseObject now.
			var matchingBase = paymentApproval.PaymentMatchingBaseObject;

			//Usually when match group form is opened, system create Exchange Difference object if AV_ExchangeDifference is not zero.
			//Here we are not opening match group form, but we still want existing Exchange Difference to be added to MatchedTransactions collection.
			//So we call CreateTemporaryTransactions method here to reuse existing code.
			//If temporary transactions are already created, matching base will not create them again.
			matchingBase.CreateTemporaryTransactions();

			//AV_ExchangeDifference is set to 0m when temporary transactions are created.
			//Hence getting previousExchangeDifference value from ExchangeDifferenceBizO.
			var previousExchangeDifference = paymentApproval.PaymentMatchingBaseObject.ExchangeDiffCurrent?.AH_InvoiceAmount ?? ZDecimal.Zero;

			var newExchangeDifference = 0m;
			if (matchingBase.MatchedTransactions.Count > 1 && !matchingBase.SessionBalancesToZero)
			{
				newExchangeDifference = previousExchangeDifference + -matchingBase.Balance;
			}

			//DeleteTemporaryTransactions because we do not want to save this Exchange Difference object to database when accepting quote.
			matchingBase.DeleteTemporaryTransactions();
			//Resetting matching base object because we used it for calculation puposes only, we did not match transactions.
			paymentApproval.ResetPaymentMatchingBaseObject();

			if (newExchangeDifference != 0m)
			{
				//DeleteTemporaryTransactions will reset AV_ExchangeDifference to previous value, hence we now set new value.
				paymentApproval.AV_ExchangeDifference = newExchangeDifference;
			}
		}

		static void DiscardAllOtherQuotesFromAllProviders(EPaymentQuote quoteToAccept, PaymentApprovalBase paymentApproval)
		{
			var quotesToDiscard = paymentApproval.PaymentQuotes.Cast<AccEPaymentQuote>().Where(q => q.PK != quoteToAccept.PK);
			if (quotesToDiscard.Any())
			{
				quotesToDiscard.ForEach(q => q.QU_Status = EPaymentStatusCodes.Quote.Discarded);
			}
		}
	}
}

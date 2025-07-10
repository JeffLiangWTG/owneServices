using Enterprise.Accounting.Business.ARAP.EPayment;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public struct QuoteCreationResult
	{
		public EPaymentQuote Quote;
		public string ErrorMessage;
		public EPaymentDealCreator.QuoteAcceptingStatus QuoteStatus;
		public QuoteCreationResult(EPaymentQuote quote, string errorMessage, EPaymentDealCreator.QuoteAcceptingStatus quoteStatus)
		{
			Quote = quote;
			ErrorMessage = errorMessage;
			QuoteStatus = quoteStatus;
		}

		public void Deconstruct(out EPaymentQuote quote, out string errorMessage, out EPaymentDealCreator.QuoteAcceptingStatus quoteStatus)
		{
			quote = Quote;
			errorMessage = ErrorMessage;
			quoteStatus = QuoteStatus;
		}
	}
}

using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public partial class PaymentBatchCurrencySummary : CurrencySummary
	{
		public PaymentBatchCurrencySummary(IMatchingCollection transactions) : base(transactions)
		{
		}
	}
}

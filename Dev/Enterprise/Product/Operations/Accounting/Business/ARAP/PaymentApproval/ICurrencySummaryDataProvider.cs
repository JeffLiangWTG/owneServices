using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public interface ICurrencySummaryDataProvider : IMatching
	{
		void SetExchangeRate(ZDecimal exchangeRate);
		bool HasActiveDeal { get; }
	}
}

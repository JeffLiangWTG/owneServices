using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing.InvoiceLineToChargeMatching
{
	public class MatchingSuggestion
	{
		public MatchingSuggestion(ChargeGroup chargeGroup, ZInt weight)
		{
			ChargeGroup = chargeGroup;
			Weight = weight;
		}
		public ChargeGroup ChargeGroup { get; }
		//public ZDecimal PercentageMatched { get; }
		public ZInt Weight { get;  }
	}
}

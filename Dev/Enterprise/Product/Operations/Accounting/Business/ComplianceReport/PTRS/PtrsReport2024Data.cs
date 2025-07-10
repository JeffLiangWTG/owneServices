using CargoWise.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public sealed class PtrsReport2024Data
	{
		public ZInt MostCommonPaymentTerm { get; set; }
		public ZInt PaymentTermMin { get; set; }
		public ZInt PaymentTermMax { get; set; }
		public ZDecimal AveragePaymentTime { get; set; }
		public ZDecimal MedianPaymentTime { get; set; }
		public ZInt PaymentTimeOf80thPercentile { get; set; }
		public ZInt PaymentTimeOf95thPercentile { get; set; }
		public ZDecimal PercentagePaidWithinTerm { get; set; }

		public ZDecimal PercentagePaidWithin30days { get; set; }

		public ZDecimal PercentagePaidBetween31And60Days { get; set; }
		public ZDecimal PercentagePaidAfter60Days { get; set; }

		public ZDecimal SmallBusinessPaymentPercentage { get; set; }
	}
}

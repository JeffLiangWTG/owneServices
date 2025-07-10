using CargoWise.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.PTRS
{
	public sealed class PtrsAllPaymentsReport2024Data
	{
		public ZDecimal SmallBusinessPartialPaymentAmount { get; set; }
		public ZDecimal SmallBusinessFullPaymentAmount { get; set; }
		public ZDecimal OthersPartialPaymentAmount { get; set; }
		public ZDecimal OthersFullPaymentAmount { get; set; }

		public ZDecimal SmallBusinessPaymentAmount => SmallBusinessPartialPaymentAmount + SmallBusinessFullPaymentAmount;
		public ZDecimal AllPaymentAmount => SmallBusinessPartialPaymentAmount + SmallBusinessFullPaymentAmount + OthersPartialPaymentAmount + OthersFullPaymentAmount;
	}
}

using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport.IL.OpenFormat
{
	public class OpenFormatInfo
	{
		public int B100Counter { get; set; }

		public int B110Counter { get; set; }

		public int C100Counter { get; set; }

		public int D110Counter { get; set; }

		public int D120Counter { get; set; }

		public int ARInvoiceCounter { get; set; }

		public ZDecimal ARInvoiceSum { get; set; }

		public ZDecimal ARGSTSum { get; set; }

		public int APInvoiceCounter { get; set; }

		public ZDecimal APInvoiceSum { get; set; }

		public ZDecimal APGSTSum { get; set; }

		public int ARPaymentCounter { get; set; }

		public ZDecimal ARPaymentSum { get; set; }

		public ZDecimal ARPaymentGSTSum { get; set; }

		public int DepositCounter { get; set; }

		public ZDecimal DepositSum { get; set; }
	}
}

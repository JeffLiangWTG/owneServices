using System;

namespace Enterprise.Accounting.Web.Business
{
	[Serializable]
	public class TransactionPaymentStatusInfo : TransactionPaymentStatusRequest, ITransactionPaymentStatusInfo
	{
		public string CurrencyCode { get; set; }
		public decimal InvoiceTotal { get; set; }
		public decimal PaidAmount { get; set; }
		public string PaymentStatus { get; set; }
		public DateTime FullyPaidDate { get; set; }
		public bool FullyPaidDateHasValue { get; set; }
	}
}

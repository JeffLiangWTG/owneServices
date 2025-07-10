using System;

namespace Enterprise.Accounting.Web.Business
{
	[Serializable]
	public class TransactionPaymentStatusResponse : ResponseBase
	{
		public TransactionPaymentStatusInfo TransactionPaymentStatus { get; set; }
	}
}

using Enterprise.Accounting.Business.ARAP.PaymentApproval;

namespace Enterprise.Accounting.Business.ChequeTransaction
{
	internal class ChequeTransactionHeaderValidation : AccPaymentBatchValidation
	{
		public ChequeTransactionHeaderValidation(ChequeTransactionHeader parent) : base(parent)
		{
		}
	}
}

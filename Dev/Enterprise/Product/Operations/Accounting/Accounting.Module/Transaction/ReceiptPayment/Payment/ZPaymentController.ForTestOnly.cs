#if DEBUG

namespace Enterprise.Accounting.Module
{
	public partial class ZPaymentController
	{
		public Business.ARAP.PaymentApproval.PaymentApprovalBase GetNewPaymentApproval_ForTestOnly()
		{
			return GetNewPaymentApproval();
		}
	}
}

#endif

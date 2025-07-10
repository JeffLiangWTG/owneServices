#if DEBUG

namespace Enterprise.Accounting.GUI
{
	public partial class AlterPaymentManager
	{
		public Business.ARAP.PaymentApproval.PaymentApprovalBase PreparePayment_ForTestOnly()
		{
			return PreparePayment();
		}
	}
}

#endif

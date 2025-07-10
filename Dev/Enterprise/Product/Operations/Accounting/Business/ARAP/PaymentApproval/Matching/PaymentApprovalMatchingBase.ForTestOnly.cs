#if DEBUG

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public partial class PaymentApprovalMatchingBase
	{
		public PaymentApprovalBase PaymentApprovalDetail_ForTestOnly
		{
			get { return PaymentApprovalDetail; }
			set { PaymentApprovalDetail = value; }
		}

		public string GetMatchedTransactionsCacheKey_ForTestOnly()
		{
			return GetMatchedTransactionsCacheKey();
		}
	}
}

#endif

#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public partial class ReceiptPaymentBase
	{
		public ZString DefaultCashFlowCategory_ForTestOnly => DefaultCashFlowCategory;

		public void PrepareReceiptPaymentForMatching_ForTestOnly()
		{
			PrepareReceiptPaymentForMatching();
		}

		public bool IsMatching_ForTestOnly
		{
			get { return IsMatching; }
			set { IsMatching = value; }
		}

		public Base.Matching.MatchingBase fMatchingBaseObject_ForTestOnly
		{
			get { return fMatchingBaseObject; }
			set { fMatchingBaseObject = value; }
		}
	}
}

#endif

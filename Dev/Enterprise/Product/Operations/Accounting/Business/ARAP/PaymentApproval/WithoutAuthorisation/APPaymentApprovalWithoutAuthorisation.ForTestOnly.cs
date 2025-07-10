#if DEBUG

using System.ComponentModel;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public partial class APPaymentApprovalWithoutAuthorisation
	{
		public PaymentApprovalMatchingBase GetNewPaymentMatchingBaseObject_ForTestOnly()
		{
			return GetNewPaymentMatchingBaseObject();
		}

		public bool GetPropertyReadonlyness_ForTestOnly(PropertyDescriptor property)
		{
			return GetPropertyReadonlyness(property);
		}

		public bool AV_PayExRate_ReadOnly_ForTestOnly => AV_PayExRate_ReadOnly;
	}
}

#endif

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class AccPaymentBatchLookups : AutoAccPaymentBatchLookups
	{
		public AccPaymentBatchLookups(AutoAccPaymentBatch parent) : base(parent)
		{
		}

		public virtual CodeDescriptionPairList PaymentTypeList => new CodeDescriptionPairList(OLookUpEditType.PaymentMethod);
	}
}

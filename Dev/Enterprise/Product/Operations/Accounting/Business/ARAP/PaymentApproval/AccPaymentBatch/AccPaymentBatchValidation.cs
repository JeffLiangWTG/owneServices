using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class AccPaymentBatchValidation : AutoAccPaymentBatchValidation
	{
		public AccPaymentBatchValidation(AutoAccPaymentBatch parent) : base(parent)
		{
		}

		new AccPaymentBatch Parent => base.Parent as AccPaymentBatch;

		protected override void CheckAPB_PaymentType()
		{
			base.CheckAPB_PaymentType();
			ListValidation.ErrorIfInvalidCode(Parent.APB_PaymentTypeInfo);
		}
	}
}

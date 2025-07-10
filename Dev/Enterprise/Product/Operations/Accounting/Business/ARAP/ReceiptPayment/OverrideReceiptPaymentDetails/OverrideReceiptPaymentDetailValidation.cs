using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public class OverrideReceiptPaymentDetailValidation : TransactionHeaderValidation
	{
		public OverrideReceiptPaymentDetailValidation(ReceiptPaymentBase parent)
			: base(parent)
		{
		}

		protected override void CheckAH_PostDate()
		{
		}
	}
}

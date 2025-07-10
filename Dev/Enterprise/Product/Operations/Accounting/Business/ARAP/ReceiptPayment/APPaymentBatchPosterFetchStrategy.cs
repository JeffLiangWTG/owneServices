using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public class APPaymentBatchPosterFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public APPaymentBatchPosterFetchStrategy(APPaymentBatchPoster header) : base(header)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			Factory.AddFetchHint(AccPaymentApprovalSchema.AV_APB_PaymentBatch, PaymentBatch.PK);
		}

		APPaymentBatchPoster PaymentBatch => BusinessObject as APPaymentBatchPoster;
	}
}

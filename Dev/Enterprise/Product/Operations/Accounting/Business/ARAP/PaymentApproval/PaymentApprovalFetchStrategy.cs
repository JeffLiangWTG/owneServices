
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	class PaymentApprovalFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public PaymentApprovalFetchStrategy(PaymentApprovalBase approval)
			: base(approval)
		{
		}

		PaymentApprovalBase PaymentApproval
		{
			get { return BusinessObject as PaymentApprovalBase; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(AccPaymentApprovalItemSchema.A2_AV, PaymentApproval.PK);
			Factory.AddFetchHint(AccTransactionHeaderSchema.PK, PaymentApproval.AV_AH);

			ZQuery query = new ZQuery(AccTransactionLinesSchema.AL_AH, PaymentApproval.AV_AH);
			query.AddToFilter(AccTransactionLinesSchema.AL_GC, PaymentApproval.Branch.Company.PK);
			Factory.AddFetchHint(AccTransactionLinesSchema.Instance, query);

			Factory.AddFetchHint(AccTransactionMatchLinkSchema.AP_AH, PaymentApproval.AV_AH);
		}
	}
}
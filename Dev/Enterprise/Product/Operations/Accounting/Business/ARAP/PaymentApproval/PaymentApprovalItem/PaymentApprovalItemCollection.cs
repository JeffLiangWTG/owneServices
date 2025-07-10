using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class PaymentApprovalItemCollection : BusinessObjectCollection<PaymentApprovalItem>
	{
		public PaymentApprovalItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public PaymentApprovalItemCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}

		public PaymentApprovalItemCollection(PaymentApprovalBase approval)
			: base(approval.Factory, GetQueryForApproval(approval))
		{
		}

		public PaymentApprovalItemCollection(PaymentApprovalBase approval, ZQuery additionalQuery)
			: base(approval.Factory, GetQueryForApproval(approval, additionalQuery))
		{
		}

		static ZQuery GetQueryForApproval(PaymentApprovalBase approval)
		{
			return new ZQuery(AccPaymentApprovalItemSchema.A2_AV, approval.PK);
		}

		static ZQuery GetQueryForApproval(PaymentApprovalBase approval, ZQuery additionalQuery)
		{
			return new ZQuery(GetQueryForApproval(approval, additionalQuery));
		}
	}
}

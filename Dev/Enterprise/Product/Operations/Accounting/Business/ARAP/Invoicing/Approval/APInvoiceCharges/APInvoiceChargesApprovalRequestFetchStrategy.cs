using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APInvoiceChargesApprovalRequestFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public APInvoiceChargesApprovalRequestFetchStrategy(EnterpriseBusinessObject businessObject) : base(businessObject)
		{
		}

		APInvoiceChargesApprovalRequest Parent => BusinessObject as APInvoiceChargesApprovalRequest;

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			if (Parent.XP_ParentTableCode == AccTransactionHeaderSchema.Constants.Prefix)
			{
				Factory.AddFetchHint(typeof(InvoicingBase), Parent.XP_ParentID);
			}
			else
			{
				Factory.AddFetchHint(typeof(GenAddOnColumn), Parent.PK);
			}
		}
	}
}
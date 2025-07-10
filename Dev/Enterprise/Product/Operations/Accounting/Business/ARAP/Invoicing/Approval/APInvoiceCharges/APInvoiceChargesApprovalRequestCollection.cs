using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APInvoiceChargesApprovalRequestCollection : TransactionApprovalRequestCollection<APInvoiceChargesApprovalRequest>
	{
		public APInvoiceChargesApprovalRequestCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public APInvoiceChargesApprovalRequestCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override string[] GetApprovalType()
		{
			return new[] { Constants.GenApprovalRequestApprovalType.APInvoiceCharges };
		}
	}
}
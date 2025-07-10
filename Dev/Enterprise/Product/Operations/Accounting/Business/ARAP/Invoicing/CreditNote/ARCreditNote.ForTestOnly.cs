#if DEBUG

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class ARCreditNote
	{
		public void AddInvoiceApprovalLog_ForTestOnly()
		{
			AddInvoiceApprovalLog();
		}
	}
}

#endif

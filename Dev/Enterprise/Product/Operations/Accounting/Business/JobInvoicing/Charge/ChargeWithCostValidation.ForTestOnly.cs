#if DEBUG

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class ChargeWithCostValidation
	{
		public bool IsInvoiceNumberApplicable_ForTestOnly()
		{
			return IsInvoiceNumberApplicable();
		}
	}
}

#endif

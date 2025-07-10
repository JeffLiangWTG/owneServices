#if DEBUG

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class OverrideInvoiceDetailsForm
	{
		public ZArchitecture.ZGrid InvoicesGrid_ForTestOnly
		{
			get { return InvoicesGrid; }
			set { InvoicesGrid = value; }
		}
	}
}

#endif

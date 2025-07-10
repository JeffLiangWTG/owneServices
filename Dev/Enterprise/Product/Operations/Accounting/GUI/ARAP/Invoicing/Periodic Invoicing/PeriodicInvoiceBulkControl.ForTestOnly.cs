#if DEBUG

namespace Enterprise.Accounting.GUI
{
	public partial class PeriodicInvoiceBulkControl
	{
		public ZArchitecture.ZGrid PeriodicInvoicesOnJobsGrid_ForTestOnly
		{
			get { return PeriodicInvoicesOnJobsGrid; }
			set { PeriodicInvoicesOnJobsGrid = value; }
		}
	}
}

#endif

#if DEBUG

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobInvoicingUserControl
	{
		public ZArchitecture.GUI.ZTemplateTabControl JobInvoicingTabControl_ForTestOnly
		{
			get { return JobInvoicingTabControl; }
			set { JobInvoicingTabControl = value; }
		}
	}
}

#endif

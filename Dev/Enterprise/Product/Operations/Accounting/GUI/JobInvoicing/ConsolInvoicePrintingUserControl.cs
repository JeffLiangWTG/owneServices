using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class ConsolInvoicePrintingUserControl : ZUserControl
	{
		public ConsolInvoicePrintingUserControl()
		{
			InitializeComponent();
			SetupControlSecurity();
			JobInvoicePrintingControl.AllowOverlap(ARJobInvoicingPrintingSecurityPanel);
			APInvoicePrintingSplitContainer.AllowOverlap(APJobInvoicingPrintingSecurityPanel);
		}

		public ConsolInvoicePrintingUserControl(SecurityCheckpoint plugInSecurity)
			: this()
		{
			this.PlugInSecurity = plugInSecurity;
			SetupControlSecurity();
		}

		void SetupControlSecurity()
		{
			JobInvoicePrintingControl.PluginSecurity = PlugInSecurity;
			APInvoicePrintingUserControl.PluginSecurity = PlugInSecurity;
		}

		readonly SecurityCheckpoint PlugInSecurity;
	}
}

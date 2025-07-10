namespace Enterprise.Customs.ES.GUI
{
	public partial class ExportInvoiceLineOrganizationsUserControl : EU.GUI.PlugIn.InvoiceLineOrganizationsUserControl
	{
		public ExportInvoiceLineOrganizationsUserControl()
		{
			InitializeComponent();
			ConsigneeAddressControl.Visible = false;
		}
	}
}

using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.BE.GUI.PlugIn;

public partial class ImportInvoiceLineOrganizationsUserControl : EU.GUI.PlugIn.ImportInvoiceLineOrganizationsUserControl
{
	public ImportInvoiceLineOrganizationsUserControl()
	{
		InitializeComponent();
		ConsignorAddressControl.CaptionResourceString = ResourceStringData.Empty;
	}
}

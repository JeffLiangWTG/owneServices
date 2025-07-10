namespace Enterprise.Customs.IT.GUI;

public class ImportInvoiceLineOrganizationsUserControl : EU.GUI.PlugIn.ImportInvoiceLineOrganizationsUserControl
{
	public ImportInvoiceLineOrganizationsUserControl()
	{
		SetConsigneeAddressControlVisibility();
	}

	void SetConsigneeAddressControlVisibility()
	{
		ConsigneeAddressControl.Visible = false;
	}
}

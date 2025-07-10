using System;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.GUI;

public sealed class ImportInvoiceLineOrganizationsUserControl : EU.GUI.PlugIn.ImportInvoiceLineOrganizationsUserControl
{
	public ImportInvoiceLineOrganizationsUserControl()
	{
		SetControlVisibility();
	}

	void SetControlVisibility()
	{
		ConsigneeAddressControl.Visible = false;
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		SetControlsVisibilityDependingOnDataItem((JobDeclaration)CurrentDataItem);
	}

	void SetControlsVisibilityDependingOnDataItem(JobDeclaration jobDeclaration)
	{
		var isUCC6 = jobDeclaration?.IsUCC6 ?? false;

		ConsignorAddressControl.Visible = isUCC6;
		BuyerDocAddressControl.Visible = isUCC6;
		SellerDocAddressControl.Visible = isUCC6;
	}
}

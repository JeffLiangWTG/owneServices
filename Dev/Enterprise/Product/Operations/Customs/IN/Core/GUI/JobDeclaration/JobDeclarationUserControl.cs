using System;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.IN.GUI;

public partial class JobDeclarationUserControl : BaseCustomsDeclarationUserControl
{
	public JobDeclarationUserControl()
	{
		InitializeComponent();
		RemoveObsoleteAddressControls();
		ReorderRightTabeControlTabs();
	}

	void RemoveObsoleteAddressControls()
	{
		Controls.Remove(ImporterOrganisationControl);
		Controls.Remove(SupplierOrganisationControl);
	}

	void ReorderRightTabeControlTabs()
	{
		RightTabControl.SuspendLayout();
		RightTabControl.TabPages.Remove(DetailsTabPage);
		RightTabControl.TabPages.Insert(DetailsTabPage, 0);
		RightTabControl.TabPages.Remove(ExportOrientedUnitsTabPage);
		RightTabControl.TabPages.Insert(ExportOrientedUnitsTabPage, 1);
		RightTabControl.ResumeLayout(false);
		RightTabControl.PerformLayout();
	}

	protected override void SetRightTabControlSelectTab()
	{
		RightTabControl.SelectedTab = DetailsTabPage;
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		DeclarationOtherDetailsLayoutPanel.UpdateLayout(new DeclarationOtherDetailsLayout());
		ExportOrientedUnitsLayoutPanel.UpdateLayout(new ExportOrientedUnitsLayout());
	}

	protected override void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
	{
		base.JE_MessageTypeInfo_ValueChanged(sender, e);
		ExportOrientedUnitsTabPage.TabVisible = JobDeclaration?.IsExport ?? false;
	}
}

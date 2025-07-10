using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.BE.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public partial class ExportInvoiceLineUserControl : EU.GUI.EUExportInvoiceLineUserControl
{
	public ExportInvoiceLineUserControl()
	{
		InitializeComponent();

		CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);

		ReorderTabPages();
	}

	protected override ResourceStringData GetAdditionalInfosTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) => EU.GUI.CaptionProvider.AdditionalDocuments;

	protected override ResourceStringData GetPreviousDocumentsTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration)
	{
		return Res.GetData("C4E37090-D903-4C2A-9BDE-373D1220A199", "[UCC 2/1] Previous documents");
	}

	protected override ResourceStringData GetSupportingDocumentsTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration)
	{
		return Res.GetData("AD048ADF-714F-4819-84B4-D76C199C8418", "[UCC 2/3] Supporting documents");
	}

	protected override ResourceStringData GetPackagesTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration)
	{
		return Res.GetData("44CF3A8C-7720-4A23-80C3-C70D3EE03D58", "Packages");
	}

	protected override ResourceStringData GetLineChargesTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration)
	{
		return Res.GetData("5843D6E8-EF03-4BAC-80A7-1EFF6DF56F59", "[UCC 4/9] Charges");
	}

	protected override Type GetPreviousDocumentsUserControlType()
	{
		return JobDeclaration is JobDeclaration declaration && declaration.IsUCC6 && declaration.JE_ApplicationCode == Customs.Business.DeclarationApplicationCodeList.Codes.Builtin ? typeof(InvoiceLinePreviousDocumentsUserControlUCC6) : typeof(InvoiceLinePreviousDocumentsUserControl);
	}

	protected override ZBool DynamicLayoutApplied => ZBool.True;
	protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ExportInvoiceLineDetailsLayout();

	protected override Type GetSupportingDocumentsUserControlType() => typeof(InvoiceLineSupportingDocumentsUserControl);

	protected override Type GetAdditionalInfosUserControlType() => typeof(InvoiceLineAdditionalInfosUserControlWithGrid);

	void ReorderTabPages()
	{
		LineDetailTabControl.TabPages.Remove(NewLineDetailsTabPage);
		LineDetailTabControl.TabPages.Insert(NewLineDetailsTabPage, 0);

		LineDetailTabControl.TabPages.Remove(LineChargesTabPage);
		LineDetailTabControl.TabPages.Insert(LineChargesTabPage, 1);

		LineDetailTabControl.TabPages.Remove(SupportingDocumentsTabPage);
		LineDetailTabControl.TabPages.Insert(SupportingDocumentsTabPage, 2);

		LineDetailTabControl.TabPages.Remove(AdditionalInfosTabPage);
		LineDetailTabControl.TabPages.Insert(AdditionalInfosTabPage, 3);

		LineDetailTabControl.TabPages.Remove(PreviousDocumentsTabPage);
		LineDetailTabControl.TabPages.Insert(PreviousDocumentsTabPage, 4);

		LineDetailTabControl.TabPages.Remove(PackagesPivotTabPage);
		LineDetailTabControl.TabPages.Insert(PackagesPivotTabPage, 5);

		LineDetailTabControl.TabPages.Remove(OrganizationsTabPage);
		LineDetailTabControl.TabPages.Insert(OrganizationsTabPage, 6);

		LineDetailTabControl.TabPages.Remove(DangerousGoodsTabPage);
		LineDetailTabControl.TabPages.Insert(DangerousGoodsTabPage, 7);

		LineDetailTabControl.TabPages.Remove(AuthorisationsTabPage);
		LineDetailTabControl.TabPages.Insert(AuthorisationsTabPage, 8);
	}

	protected override void JobDeclaration_ControlVisibilityChanged(object sender, EventArgs e)
	{
		base.JobDeclaration_ControlVisibilityChanged(sender, e);

		var previousDocumentsUserControl = (ZDynamicControlCreationUserControl)Controls.Find("PreviousDocumentsUserControl", true)[0];
		previousDocumentsUserControl.UserControlType = GetPreviousDocumentsUserControlType();

		Refresh();
	}
}

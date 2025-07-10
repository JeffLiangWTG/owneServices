using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.BE.GUI.PlugIn;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI;

public partial class ImportInvoiceLineUserControl : EU.GUI.EUImportInvoiceLineUserControl
{
	public ImportInvoiceLineUserControl()
	{
		InitializeComponent();

		CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);

		ReorderTabPages();
	}

	protected override Type GetPreviousDocumentsUserControlType()
	{
		return JobDeclaration is JobDeclaration declaration && declaration.IsUCC6 && declaration.JE_ApplicationCode == Customs.Business.DeclarationApplicationCodeList.Codes.Builtin ? typeof(ImportInvoiceLinePreviousDocumentsUserControlUCC6) : typeof(ImportInvoiceLinePreviousDocumentsUserControl);
	}

	protected override ZBool DynamicLayoutApplied => ZBool.True;

	protected override Type GetSupportingDocumentsUserControlType() => typeof(InvoiceLineSupportingDocumentsUserControl);

	protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ImportInvoiceLineDetailsLayout();

	protected override Type GetAdditionalInfosUserControlType() => typeof(InvoiceLineAdditionalInfosUserControlWithGrid);

	protected override Type GetOrganizationsUserControlType() => typeof(ImportInvoiceLineOrganizationsUserControl);

	protected override Type GetValuationIndicatorsUserControlType() => typeof(EU.GUI.InvoiceLineValuationIndicatorDropEditsUserControl);

	protected override ResourceStringData GetAdditionalInfosTabPageCaption(ICommonInvoiceDataProvider declaration) => EU.GUI.CaptionProvider.AdditionalDocuments;

	protected override ResourceStringData GetPreviousDocumentsTabPageCaption(ICommonInvoiceDataProvider declaration)
	{
		return Res.GetData("CC3C87D2-C61F-4859-A67B-C15FA1A3C457", "[UCC 2/1] Previous documents");
	}

	protected override ResourceStringData GetSupportingDocumentsTabPageCaption(ICommonInvoiceDataProvider declaration)
	{
		return Res.GetData("1862CB25-4541-44FB-B89C-FD5EEF4F34F1", "[UCC 2/3] Supporting documents");
	}

	protected override ResourceStringData GetPackagesTabPageCaption(ICommonInvoiceDataProvider declaration)
	{
		return Res.GetData("B3ED407A-0AE7-4CA0-A2D1-FD1E2678A0EA", "Packages");
	}

	protected override ResourceStringData GetLineChargesTabPageCaption(ICommonInvoiceDataProvider declaration)
	{
		return Res.GetData("8760BA5C-3616-4D33-B89A-38261E1CC103", "[UCC 4/9] Charges");
	}

	protected override ResourceStringData GetValueIndicatorsTabPageCaption(ICommonInvoiceDataProvider declaration)
	{
		return Res.GetData("FFEB8690-2937-4357-9328-F8E179981258", "[UCC 4/13] Value Indicators");
	}

	void ReorderTabPages()
	{
		LineDetailTabControl.TabPages.Clear();
		LineDetailTabControl.TabPages.Insert(NewLineDetailsTabPage, 0);
		LineDetailTabControl.TabPages.Insert(LineChargesTabPage, 1);
		LineDetailTabControl.TabPages.Insert(FiscalReferencesTabPage, 2);
		LineDetailTabControl.TabPages.Insert(SupportingDocumentsTabPage, 3);
		LineDetailTabControl.TabPages.Insert(AdditionalInfosTabPage, 4);
		LineDetailTabControl.TabPages.Insert(PreviousDocumentsTabPage, 5);
		LineDetailTabControl.TabPages.Insert(PackagesPivotTabPage, 6);
		LineDetailTabControl.TabPages.Insert(OrganizationsTabPage, 7);
		LineDetailTabControl.TabPages.Insert(AuthorisationsTabPage, 8);
		LineDetailTabControl.TabPages.Insert(ValueIndicatorsTabPage, 9);
	}

	protected override void JobDeclaration_ControlVisibilityChanged(object sender, EventArgs e)
	{
		base.JobDeclaration_ControlVisibilityChanged(sender, e);

		var previousDocumentsUserControl = (ZDynamicControlCreationUserControl)Controls.Find("PreviousDocumentsUserControl", true)[0];
		previousDocumentsUserControl.UserControlType = GetPreviousDocumentsUserControlType();

		Refresh();
	}
}

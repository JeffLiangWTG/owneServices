using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class ImportInvoiceLineUserControl : EU.GUI.EUImportInvoiceLineUserControl
{
	public ImportInvoiceLineUserControl()
	{
		InitializeComponent();
		SetSpecificControlsVisibility();
	}

	void SetSpecificControlsVisibility()
	{
		CountryOfSupplyCodeFindBox.Visible = false;
	}

	protected override Type GetSupportingDocumentsUserControlType() => typeof(InvoiceLineLayoutSupportingDocumentsUserControl);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(InvoiceLineImportLayoutPreviousDocumentsUserControl);

	protected override Type GetAdditionalInfosUserControlType() => typeof(AdditionalInfosUserControl);

	protected override Type GetOrganizationsUserControlType() => typeof(ImportInvoiceLineOrganizationsUserControl);

	protected override Type GetValuationIndicatorsUserControlType() => typeof(EU.GUI.InvoiceLineValuationIndicatorDropEditsUserControl);

	protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ImportInvoiceLineDetailsLayout();

	protected override CargoWiseOne.ResourceStrings.ResourceStringData GetAdditionalInfosTabPageCaption(EU.Business.Declaration.ICommonInvoiceDataProvider declaration) => EU.GUI.CaptionProvider.AdditionalInfo_44;

	protected override bool IsJI_ValuationMarkupVisible => false;
	protected override bool IsJI_ZZF_NKTaxTypeVisible => false;
	protected override bool IsJI_TaxOrFeeDetailVisible => true;

	protected override ZBool DynamicLayoutApplied => ZBool.True;

	protected override ZBool HasDifferentPanelLayout => ZBool.True;
}

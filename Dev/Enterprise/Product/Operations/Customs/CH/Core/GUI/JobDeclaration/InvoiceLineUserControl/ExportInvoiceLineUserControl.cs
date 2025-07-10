using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.GUI;

public partial class ExportInvoiceLineUserControl : BaseInvoiceLineUserControl
{
	public ExportInvoiceLineUserControl()
	{
		InitializeComponent();
		CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
		SetTabPagesOrderAndVisibility();
	}

	void SetTabPagesOrderAndVisibility()
	{
		PermitsTabPage.TabVisible = false;
		NonCustomsLawTabPage.TabVisible = false;
		VehiclesTabPage.TabVisible = false;
		TobaccosTabPage.TabVisible = false;
		SpecialMentionsTabPage.TabVisible = false;
		LineDetailTabControl.ReorderTabPages(NewLineDetailsTabPage, LineChargesTabPage, SupportingDocumentsTabPage, PreviousDocumentsTabPage,
			PackagesPivotTabPage, RestrictionsTabPage, InAndOutwardProcessingTabPage, AdditionalInformationTabPage, VehiclesTabPage, TobaccosTabPage, LineDetailTabControl.FindSingle<ZTabPage>("CustomFieldsTabPage"));
	}

	protected override ZBool DynamicLayoutApplied => ZBool.True;
	protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ExportInvoiceLineDetailsLayout();
	protected override ZBool HasDifferentPanelLayout => ZBool.True;
	protected override ZString UniversalTariffType => UniversalReferenceConstants.TariffTypes.ExportTariff;

	protected override IEnumerable<string> GetDefaultColumnsForGrid()
	{
		yield return JobComInvoiceLineSchema.Constants.JI_LineNo;
		yield return JobComInvoiceLine.Schema.EntryLineNumber;
		yield return JobComInvoiceLineSchema.Constants.JI_CEI;
		yield return JobComInvoiceLineSchema.Constants.JI_Procedure;
		yield return TariffColumnName;
		yield return JobComInvoiceLineSchema.Constants.JI_Description;
		yield return JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin;
		yield return JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity;
		yield return JobComInvoiceLineSchema.Constants.JI_InvoiceUQ;
		yield return JobComInvoiceLineSchema.Constants.JI_Weight;
		yield return JobComInvoiceLineSchema.Constants.JI_WeightUQ;
		yield return JobComInvoiceLineSchema.Constants.JI_NetWeight;
		yield return JobComInvoiceLineSchema.Constants.JI_NetWeightUQ;
		yield return JobComInvoiceLineSchema.Constants.JI_LinePrice;
	}
}

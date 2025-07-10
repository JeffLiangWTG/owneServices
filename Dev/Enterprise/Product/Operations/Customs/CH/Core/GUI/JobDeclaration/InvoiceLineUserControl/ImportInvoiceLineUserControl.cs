using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.GUI;

public partial class ImportInvoiceLineUserControl : BaseInvoiceLineUserControl
{
	public ImportInvoiceLineUserControl()
	{
		InitializeComponent();
		CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
		ReorderTabPages();
	}

	void ReorderTabPages()
	{
		LineDetailTabControl.ReorderTabPages(NewLineDetailsTabPage, LineChargesTabPage, SupportingDocumentsTabPage,
			PackagesPivotTabPage, TaxesAndFeesTabPage, PermitsTabPage, NonCustomsLawTabPage, InAndOutwardProcessingTabPage,
			AdditionalInformationTabPage, VehiclesTabPage, TobaccosTabPage, SpecialMentionsTabPage, LineDetailTabControl.FindSingle<ZTabPage>("CustomFieldsTabPage"));
	}

	protected override ZBool DynamicLayoutApplied => ZBool.True;
	protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ImportInvoiceLineDetailsLayout();
	protected override ZBool HasDifferentPanelLayout => ZBool.True;
	protected override ZString UniversalTariffType => UniversalReferenceConstants.TariffTypes.ImportTariff;

	protected override IEnumerable<ZGridColumnInfo> GetColumnInfosToBeAdded()
	{
		foreach (var columnInfo in base.GetColumnInfosToBeAdded())
		{
			yield return columnInfo;
		}
		yield return new ZDropEditColumnStyleInfo
		{
			ColumnName = JobComInvoiceLineSchema.Constants.JI_PrimaryPreference,
		};
	}

	protected override IEnumerable<string> GetDefaultColumnsForGrid()
	{
		yield return JobComInvoiceLineSchema.Constants.JI_LineNo;
		yield return JobComInvoiceLine.Schema.EntryLineNumber;
		yield return JobComInvoiceLineSchema.Constants.JI_CEI;
		yield return JobComInvoiceLineSchema.Constants.JI_Procedure;
		yield return TariffColumnName;
		yield return JobComInvoiceLineSchema.Constants.JI_Description;
		yield return JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin;
		yield return JobComInvoiceLineSchema.Constants.JI_PrimaryPreference;
		yield return JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity;
		yield return JobComInvoiceLineSchema.Constants.JI_InvoiceUQ;
		yield return JobComInvoiceLineSchema.Constants.JI_Weight;
		yield return JobComInvoiceLineSchema.Constants.JI_WeightUQ;
		yield return JobComInvoiceLineSchema.Constants.JI_NetWeight;
		yield return JobComInvoiceLineSchema.Constants.JI_NetWeightUQ;
		yield return JobComInvoiceLineSchema.Constants.JI_LinePrice;
	}
}

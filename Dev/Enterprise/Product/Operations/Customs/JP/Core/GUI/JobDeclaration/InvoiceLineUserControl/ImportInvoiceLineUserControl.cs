using CargoWise.Types;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class ImportInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		public ImportInvoiceLineUserControl()
		{
			InitializeComponent();
			ConfigureTariffColumn();

			DomesticConsumptionTaxesGrid.MaximumRows = CusLineTariffDetailCollection.MaxRowCount;
		}

		protected override ZBool DynamicLayoutApplied => ZBool.True;

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ImportInvoiceLineLayouts();

		void ConfigureTariffColumn()
		{
			var tariffColumn = CustomsInvoiceLinesBoundGrid.GetColumnStyle(TariffColumnName) as TariffColumnStyleInfo;
			tariffColumn.SelectNomenclatureModes = new System.Collections.Generic.List<SelectionStyle> { SelectionStyle.Tariff, SelectionStyle.Subheading };
		}

		protected override void ResetCustomsInvoiceLinesBoundGridColumns()
		{
			base.ResetCustomsInvoiceLinesBoundGridColumns();
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				ColumnName = JobComInvoiceLine.Schema.JI_StorageType,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("JPImportInvoiceLineUserControl|JI_StorageType", "Storage Type"),
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
			{
				ColumnName = nameof(JobComInvoiceLine.JI_DutyRateFormula),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				IsVisible = true,
			});

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDateEditColumnStyleInfo
			{
				ColumnName = nameof(JobComInvoiceLine.JI_BondedDate),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				IsVisible = true,
			});
		}
	}
}

using CargoWise.Types;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class ExportInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		public ExportInvoiceLineUserControl()
		{
			InitializeComponent();
			AddNewColumnsToCustomsInvoiceLinesBoundGrid();
			ConfigureTariffColumn();

			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
			OtherLawsGrid.MaximumRows = CusOtherLawReferenceCollection<CusOtherLawReferenceForInvoiceLines>.MaxRowCount;
		}

		protected override ZBool DynamicLayoutApplied => ZBool.True;

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ExportInvoiceLineLayouts();

		void ConfigureTariffColumn()
		{
			var tariffColumn = CustomsInvoiceLinesBoundGrid.GetColumnStyle(TariffColumnName) as TariffColumnStyleInfo;
			tariffColumn.SelectNomenclatureModes = new System.Collections.Generic.List<SelectionStyle> { SelectionStyle.Tariff, SelectionStyle.Heading };
		}

		void AddNewColumnsToCustomsInvoiceLinesBoundGrid()
		{
			CustomsInvoiceLinesBoundGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZDropEditColumnStyleInfo
				{
					ColumnName = JobComInvoiceLine.Schema.JI_FEFTAArticle48,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("54784D8E-EDB7-403C-98E8-16077F605E57", "FEFTA"),
					IsVisible = false
				},

				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = "FEFTAArticle48Description",
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500),
					GroupName = Enterprise.Customs.JP.GUI.Res.GetData("54784D8E-EDB7-403C-98E8-16077F605E57", "FEFTA"),
					IsReadOnly = true,
					IsVisible = false
				},
			});
		}

		protected override void ResetCustomsInvoiceLinesBoundGridColumns()
		{
			base.ResetCustomsInvoiceLinesBoundGridColumns();

			var exportControlNumberColumnStyleInfo = new ZArchitecture.ZTextBoxColumnStyleInfo();
			exportControlNumberColumnStyleInfo.ColumnName = nameof(JobComInvoiceLine.ExportControlNumber);
			exportControlNumberColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(137);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Insert(4, exportControlNumberColumnStyleInfo);
		}

		new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			CustomsInvoiceLinesBoundGrid.SetAvailability(JobDeclaration.IsExportAndSea, nameof(JobComInvoiceLine.ExportControlNumber));
		}
	}
}

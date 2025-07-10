using Enterprise.Customs.JP.Business;

namespace Enterprise.Customs.JP.GUI
{
	public partial class ImportSupplierHeaderUserControl : BaseCustomsSupplierHeaderUserControl
	{
		public ImportSupplierHeaderUserControl()
		{
			InitializeComponent();
			InitializeInvoiceArrayBoundGrid();
			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
			ComprehensiveValuationsGrid.MaximumRows = ComprehensiveValuationCollection.MaxRowCount;
		}

		void InitializeInvoiceArrayBoundGrid()
		{
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(new ZArchitecture.GUI.ZDropEditColumnStyleInfo()
			{
				ColumnName = nameof(JobComInvoiceHeader.JZ_ValuationCode),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			});

			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo()
			{
				ColumnName = nameof(JobComInvoiceHeader.JZ_ValuationDateOverride),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			});
		}
	}
}

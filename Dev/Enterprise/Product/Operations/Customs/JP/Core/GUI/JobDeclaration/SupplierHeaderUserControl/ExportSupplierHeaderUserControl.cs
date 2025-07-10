namespace Enterprise.Customs.JP.GUI
{
	public partial class ExportSupplierHeaderUserControl : BaseCustomsSupplierHeaderUserControl
	{
		public ExportSupplierHeaderUserControl()
		{
			InitializeComponent();
			InitializeNewColumns();

			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
		}

		void InitializeNewColumns()
		{
			var valuationDateOverrideDateEditColumnStyleInfo = new ZArchitecture.ZDateEditColumnStyleInfo();
			valuationDateOverrideDateEditColumnStyleInfo.ColumnName = "JZ_ValuationDateOverride";
			valuationDateOverrideDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(valuationDateOverrideDateEditColumnStyleInfo);
		}
	}
}

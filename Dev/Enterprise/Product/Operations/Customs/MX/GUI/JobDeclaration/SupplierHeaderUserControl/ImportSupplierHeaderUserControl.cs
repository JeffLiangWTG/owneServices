namespace Enterprise.Customs.MX.GUI
{
	public partial class ImportSupplierHeaderUserControl : BaseCustomsSupplierHeaderUserControl
	{
		public ImportSupplierHeaderUserControl()
		{
			InitializeComponent();

			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
		}
	}
}

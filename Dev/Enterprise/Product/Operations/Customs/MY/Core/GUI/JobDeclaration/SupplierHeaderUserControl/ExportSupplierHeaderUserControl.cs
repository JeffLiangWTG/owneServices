namespace Enterprise.Customs.MY.GUI
{
	public partial class MYExportSupplierHeaderUserControl : MYCustomsSupplierHeaderUserControl
	{
		public MYExportSupplierHeaderUserControl()
		{
			InitializeComponent();
			this.InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
		}
	}
}

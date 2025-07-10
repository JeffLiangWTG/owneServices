namespace Enterprise.Customs.MY.GUI
{
	public partial class MYImportSupplierHeaderUserControl : MYCustomsSupplierHeaderUserControl
	{
		public MYImportSupplierHeaderUserControl()
		{
			InitializeComponent();
			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
		}
	}
}

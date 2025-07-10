namespace Enterprise.Customs.MY.GUI
{
	public partial class MYImportInvoiceLineUserControl : MYInvoiceLineUserControl
	{
		public MYImportInvoiceLineUserControl()
		{
			InitializeComponent();
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
		}
	}
}

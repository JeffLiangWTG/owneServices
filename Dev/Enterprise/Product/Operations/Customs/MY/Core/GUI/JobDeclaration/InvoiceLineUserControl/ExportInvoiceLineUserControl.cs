namespace Enterprise.Customs.MY.GUI
{
	public partial class MYExportInvoiceLineUserControl : MYInvoiceLineUserControl
	{
		public MYExportInvoiceLineUserControl()
		{
			InitializeComponent();
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
		}
	}
}

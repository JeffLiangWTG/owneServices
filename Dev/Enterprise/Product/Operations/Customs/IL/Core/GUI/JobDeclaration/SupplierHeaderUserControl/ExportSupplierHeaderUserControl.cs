namespace Enterprise.Customs.IL.GUI
{
	public partial class ExportSupplierHeaderUserControl : BaseCustomsSupplierHeaderUserControl
	{
		public ExportSupplierHeaderUserControl()
		{
			InitializeComponent();

			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
		}
	}
}

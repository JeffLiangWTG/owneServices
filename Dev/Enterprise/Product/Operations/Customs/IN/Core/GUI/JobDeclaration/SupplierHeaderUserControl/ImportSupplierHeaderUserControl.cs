namespace Enterprise.Customs.IN.GUI;

public partial class ImportSupplierHeaderUserControl : BaseCustomsSupplierHeaderUserControl
{
	public ImportSupplierHeaderUserControl()
	{
		InitializeComponent();

		InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
	}
}

namespace Enterprise.Customs.IN.GUI;

public partial class ImportInvoiceLineUserControl : BaseInvoiceLineUserControl
{
	public ImportInvoiceLineUserControl()
	{
		InitializeComponent();

		CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
	}
}

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public class ExportInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		protected override Customs.GUI.InvoiceLineChargesUserControl GetInvoiceLineChargesUserControl() => new ExportInvoiceLineChargesUserControl();
	}
}

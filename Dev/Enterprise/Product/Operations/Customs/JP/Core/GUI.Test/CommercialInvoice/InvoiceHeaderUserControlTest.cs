using Enterprise.Customs.GUI;

namespace Enterprise.Customs.JP.GUI.Testing
{
	sealed class InvoiceHeaderUserControlTest : Customs.GUI.Testing.InvoiceHeaderUserControlAbstractTest
	{
		protected override CommonInvoiceHeaderUserControl GetNewInvoiceHeaderUserControl() => new InvoiceHeaderUserControl();
	}
}

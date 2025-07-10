namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class InvoiceHeaderUserControlTest : Customs.GUI.Testing.InvoiceHeaderUserControlAbstractTest
	{
		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetNewInvoiceHeaderUserControl() => new InvoiceHeaderUserControl();
	}
}

using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class ExportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestInvoiceLineChargesUserControl()
		{
			using (var userControl = new ExportInvoiceLineUserControl())
			{
				AssertType(typeof(ExportInvoiceLineChargesUserControl), userControl.InvoiceLineCharges);
			}
		}
	}
}

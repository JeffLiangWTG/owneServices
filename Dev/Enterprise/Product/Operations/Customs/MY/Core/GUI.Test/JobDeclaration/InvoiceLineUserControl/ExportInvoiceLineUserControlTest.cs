using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.MY.GUI.Testing
{
	class ExportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestGridLayoutContext()
		{
			using (MYExportInvoiceLineUserControl control = new MYExportInvoiceLineUserControl())
			{
				AssertEquals("context is set", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}
	}
}

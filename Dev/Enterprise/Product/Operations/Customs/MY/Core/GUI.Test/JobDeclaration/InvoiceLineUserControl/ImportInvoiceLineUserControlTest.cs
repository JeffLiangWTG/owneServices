using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.MY.GUI.Testing
{
	class ImportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestGridLayoutContext()
		{
			using (MYImportInvoiceLineUserControl control = new MYImportInvoiceLineUserControl())
			{
				AssertEquals("context is set", nameof(Customs.GUI.DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}
	}
}

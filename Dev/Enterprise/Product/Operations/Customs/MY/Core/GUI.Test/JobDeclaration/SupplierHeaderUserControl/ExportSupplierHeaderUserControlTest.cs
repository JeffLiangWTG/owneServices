using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.MY.GUI.Testing
{
	class ExportSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestGridLayoutContext()
		{
			using (MYExportSupplierHeaderUserControl control = new MYExportSupplierHeaderUserControl())
			{
				AssertEquals("context is set", nameof(Customs.GUI.DeclarationType.Export), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}
	}
}

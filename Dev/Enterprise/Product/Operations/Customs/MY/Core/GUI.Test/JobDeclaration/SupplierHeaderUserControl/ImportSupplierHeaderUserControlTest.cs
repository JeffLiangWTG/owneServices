using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.MY.GUI.Testing
{
	class ImportSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestGridLayoutContext()
		{
			using (MYImportSupplierHeaderUserControl control = new MYImportSupplierHeaderUserControl())
			{
				AssertEquals("context is set", nameof(Customs.GUI.DeclarationType.Import), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}
	}
}

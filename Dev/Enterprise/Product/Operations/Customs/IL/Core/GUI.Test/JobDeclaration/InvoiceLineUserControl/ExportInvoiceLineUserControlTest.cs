using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.GUI.Testing
{
	class ExportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be export", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}
	}
}

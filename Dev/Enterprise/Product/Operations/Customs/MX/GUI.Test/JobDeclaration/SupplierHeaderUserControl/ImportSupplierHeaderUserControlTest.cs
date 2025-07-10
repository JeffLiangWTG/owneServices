using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.GUI.Testing
{
	[TestedType(typeof(ImportSupplierHeaderUserControl))]
	class ImportSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContext()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				AssertEquals("Column layout context should have been set", nameof(Customs.GUI.DeclarationType.Import), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestGridId()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				AssertEquals("GridLayoutT+oYGAheR1633dT3oeP+lw==", control.InvoiceHeadersBoundGrid.GridId);
			}
		}
	}
}

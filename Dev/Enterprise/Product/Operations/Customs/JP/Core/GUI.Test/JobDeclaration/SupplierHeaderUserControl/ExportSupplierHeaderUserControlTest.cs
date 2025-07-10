using System.Collections.Generic;
using Enterprise.Core.Forms;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.JP.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ExportSupplierHeaderUserControl))]
	class ExportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ExportSupplierHeaderUserControl, JobDeclaration>
	{
		protected override IEnumerable<string> ExpectedControlList => DefaultControlList;

		public void TestColumnLayoutContext()
		{
			using (var control = new ExportSupplierHeaderUserControl())
			{
				AssertEquals("Column layout context should have been set", nameof(Customs.GUI.DeclarationType.Export), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestJobComInvoiceHeadersBoundGrid()
		{
			using (var control = new ExportSupplierHeaderUserControl())
			{
				var grid = control.JobComInvoiceHeadersBoundGrid;
				var columns = grid.ColumnStyles.ToList<ZGridColumnInfo>();
				var valuationDateColumn = columns.Find(x => x.ColumnName == "JZ_ValuationDateOverride");
				AssertNotNull(valuationDateColumn);
			}
		}
	}
}

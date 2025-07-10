using System.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class DataTableGridViewTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestNoExceptionThrownWhenAccessDeletedRow()
		{
			var view = new DataTableGridView();
			var data = new DataSet();
			var table = new DataTable("TestTable");
			table.Columns.Add("PK", typeof(ZGuid));
			data.Tables.Add(table);
			var row = table.NewRow();
			row["PK"] = new ZGuid();
			table.Rows.Add(row);
			table.AcceptChanges();
			view.InstallDataTableSafe(table, true);
			row.Delete();
			view.InstallDataTableSafe(table, true);
			view.Dispose();
		}
	}
}

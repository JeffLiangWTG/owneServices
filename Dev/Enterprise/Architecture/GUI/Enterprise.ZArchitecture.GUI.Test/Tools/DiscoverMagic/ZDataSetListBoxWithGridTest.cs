using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	internal class ZDataSetListBoxWithGridTest : ZControlBaseTestCase<ZDataSetListBoxWithGrid>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestDisposeWithLargeDataSet()
		{
			var form = new Form();
			try
			{
				var dataSet = new DataSet();
				var table1 = dataSet.Tables.Add("Table1");
				table1.Columns.Add("Col1");
				for (int i = 0; i < 10000; i++)
				{
					var row = table1.NewRow();
					row["Col1"] = $"Testval{i}";
					table1.Rows.Add(row);
				}

				var table2 = dataSet.Tables.Add("Table2");
				table2.Columns.Add("Col2");
				for (int i = 0; i < 10000; i++)
				{
					var row = table2.NewRow();
					row["Col2"] = "Testval";
					table2.Rows.Add(row);
				}

				var listGird = new ZDataSetListBoxWithGrid()
				{ Dock = DockStyle.Fill, DataSource = dataSet };
				form.Controls.Add(listGird);
				form.Show();
				listGird.GridViewForTest.CurrentCell = listGird.GridViewForTest.Rows[0].Cells[0];
				listGird.GridViewForTest.BeginEdit(true);
			}
			finally
			{
				AssertNoExceptionThrown(() => form.Dispose());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestDataSource()
		{
			var data = new DataSet(); // for testing only
			var table = data.Tables.Add("Table1");
			data.Tables.Add("Table2");
			using (var control = new ZDataSetListBoxWithGrid())
			{
				control.DataSource = data;
				AssertContainsExactElementsInAnyOrder(new[] { "Table1", "Table2" }, (string[])control.GetListBoxForTest.DataSource);
				table.Columns.Add("Column1");
				table.Columns.Add("Column2");
				control.DataSource = table;
				AssertNull(control.GetListBoxForTest.DataSource);
				AssertContainsExactElementsInAnyOrder(new[] { "Column1", "Column2" }, control.CurrentTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
			}
		}

		[DeveloperOnlyTest]
		[TestDate(2015, 01, 01)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestCopyDateValues()
		{
			var data = new DataSet();
			var table = new DataTable("TestTable");
			table.Columns.Add("ZDateTimeCol", typeof(ZDateTime));
			table.Columns.Add("DateTimeCol", typeof(DateTime));
			var row1 = table.NewRow();
			row1["ZDateTimeCol"] = new ZDateTime(2015, 12, 1, 15, 5, 10);
			row1["DateTimeCol"] = new DateTime(2015, 12, 1, 15, 5, 10);
			table.Rows.Add(row1);
			data.Tables.Add(table);
			table.AcceptChanges();
			using (var control = new ZDataSetListBoxWithGrid())
			{
				control.DataSource = data;
				control.GridViewForTest.SelectAll();
				Assert("soemthign", control.GridViewForTest.Columns.Contains("Row state"));
				control.GridViewForTest.ExtensionMenu.MenuItems.FindByText("&Copy As CSV").PerformClick();
				var expected = @"""ZDateTimeCol"", ""DateTimeCol""
""2015-12-01 15:05:10"", ""2015-12-01 15:05:10""
"""", """"";
				var actualCopiedResult = SafeClipboard.GetText();
				AssertMultilineASCIIEquals("Expected date columns to be formatted correctly, null values cause no problems", expected, actualCopiedResult);
				control.GridViewForTest.ExtensionMenu.MenuItems.FindByText("&Copy As Insert SQL").PerformClick();
				expected = @"INSERT INTO TestTable
(ZDateTimeCol, DateTimeCol)
VALUES
('2015-12-01 15:05:10', '2015-12-01 15:05:10'),
(Null, Null)";
				actualCopiedResult = SafeClipboard.GetText();
				AssertMultilineASCIIEquals("SQL should work ask well", expected, actualCopiedResult);
			}
		}

		[DeveloperOnlyTest]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestSqlBoolValues()
		{
			var data = new DataSet();
			var table = new DataTable(StmALogSchema.Constants.TableName);
			table.Columns.Add(StmALogSchema.Constants.SL_FireWorkflow, typeof(bool));
			table.Columns.Add(StmALogSchema.Constants.SL_IsEstimate, typeof(bool));
			table.Rows.Add(table.NewRow());
			table.Rows.Add(table.NewRow());
			data.Tables.Add(table);
			table.AcceptChanges();
			Factory.Save();
			using (var control = new ZDataSetListBoxWithGrid())
			{
				control.DataSource = data;
				control.GridViewForTest.Rows[0].Cells[StmALogSchema.Constants.SL_FireWorkflow].Value = "True";
				control.GridViewForTest.Rows[0].Cells[StmALogSchema.Constants.SL_IsEstimate].Value = "True";
				control.GridViewForTest.Rows[1].Cells[StmALogSchema.Constants.SL_FireWorkflow].Value = "False";
				control.GridViewForTest.Rows[1].Cells[StmALogSchema.Constants.SL_IsEstimate].Value = "False";
				control.GridViewForTest.Rows[0].Cells[0].Selected = true;
				control.GridViewForTest.Rows[1].Cells[1].Selected = true;
				var copyMenuItem = control.GridViewForTest.ExtensionMenu.MenuItems.FindByText("&Copy As Insert SQL");
				var action = new Action(() => copyMenuItem.PerformClick());
				action.Invoke();
				var expected = @"INSERT INTO dbo.StmALog
(SL_FireWorkflow, SL_IsEstimate)
VALUES
('1', '1'),
('0', '0')";
				var actualCopiedResult = ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action);
				AssertMultilineASCIIEquals("For boolean columns stored as different datacolumns in the db, we expect the SQL output to be different", expected, actualCopiedResult);
			}
		}

		[DeveloperOnlyTest]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestCsvSelection()
		{
			var data = new DataSet();
			var table = new DataTable("TestTable");
			table.Columns.Add("Something", typeof(ZString));
			table.Columns.Add("Something2", typeof(ZString));
			table.Columns.Add("NotSelected", typeof(ZString));
			var row = table.NewRow();
			row["Something"] = new ZString("String1");
			row["Something2"] = new ZString("String2");
			row["NotSelected"] = new ZString("NoExpected");
			table.Rows.Add(row);
			data.Tables.Add(table);
			table.AcceptChanges();
			using (var control = new ZDataSetListBoxWithGrid())
			{
				control.DataSource = data;
				control.GridViewForTest.Rows[0].Cells[0].Selected = true;
				control.GridViewForTest.Rows[0].Cells[1].Selected = true;
				control.GridViewForTest.ExtensionMenu.MenuItems.FindByText("&Copy As CSV").PerformClick();
				var expected = @"""Something"", ""Something2""
""String1"", ""String2""";
				var actualCopiedResult = SafeClipboard.GetText();
				AssertMultilineASCIIEquals("Only expected to include columns selected", expected, actualCopiedResult);
			}
		}

		[DeveloperOnlyTest]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestInsertSQLBlockSelection()
		{
			var data = new DataSet();
			var table = new DataTable("TestTable");
			table.Columns.Add("Something", typeof(ZString));
			table.Columns.Add("Something2", typeof(ZString));
			table.Columns.Add("Something3", typeof(ZString));
			var row = table.NewRow();
			row["Something"] = new ZString("String1");
			row["Something2"] = new ZString("String2");
			row["Something3"] = new ZString("String3");
			table.Rows.Add(row);
			data.Tables.Add(table);
			table.AcceptChanges();
			using (var control = new ZDataSetListBoxWithGrid())
			{
				control.DataSource = data;
				control.GridViewForTest.Rows[0].Cells[0].Selected = true;
				control.GridViewForTest.Rows[0].Cells[1].Selected = true;
				var menuItem = control.GridViewForTest.ExtensionMenu.MenuItems.FindByText("&Copy As Insert SQL");
				var action = new Action(() => menuItem.PerformClick());
				action.Invoke();
				var expected = @"INSERT INTO TestTable
(Something, Something2)
VALUES
('String1', 'String2')";
				var actualCopiedResult = ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action);
				AssertMultilineASCIIEquals("", expected, actualCopiedResult);
			}
		}

		[DeveloperOnlyTest]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestCopyingValuesDoesNotIncludeRowStateColumnNameOrValues()
		{
			var data = new DataSet();
			var table = new DataTable("TestTable");
			table.Columns.Add("Value1", typeof(ZString));
			var row = table.NewRow();
			row["Value1"] = new ZString("ABCD");
			table.Rows.Add(row);
			data.Tables.Add(table);
			table.AcceptChanges();
			using (var control = new ZDataSetListBoxWithGrid())
			{
				control.DataSource = data;
				control.GridViewForTest.SelectAll();
				Assert("Row state column should be added by default for this data type", control.GridViewForTest.Columns.Contains("Row state"));
				var copyMenuItem = control.GridViewForTest.ExtensionMenu.MenuItems.FindByText("&Copy As Insert SQL");
				var action = new Action(() =>
				{
					copyMenuItem.PerformClick();
					Application.DoEvents();
				});
				action.Invoke();
				var expected = @"INSERT INTO TestTable
(Value1)
VALUES
('ABCD'),
(Null)";
				var actualCopiedResult = ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action);
				AssertMultilineASCIIEquals("Should not contain any row state info", expected, actualCopiedResult);
				copyMenuItem = control.GridViewForTest.ExtensionMenu.MenuItems.FindByText("&Copy As CSV");
				action = () =>
				{
					copyMenuItem.PerformClick();
					Application.DoEvents();
				};
				action.Invoke();
				expected = @"""Value1""
""ABCD""
""""";
				actualCopiedResult = ClipboardTestHelper.RetryIfCopyOrCutFailed<string>(action);
				AssertMultilineASCIIEquals("Should not contain any row state info", expected, actualCopiedResult);
			}
		}

		protected override bool UsesControlDataBindings
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresTypeDescriptor
		{
			get
			{
				return false;
			}
		}
	}
}

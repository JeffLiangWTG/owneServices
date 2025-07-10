using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class ZDateEditColumnStyleTest : TestCaseWithDummy
	{
		public void TestTabbing()
		{
			using (var form = new ZForm())
			{
				form.Show();
				Application.DoEvents();

				var grid = new ZGrid();
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Z0_VarCharMax.Name, 20));
				grid.ColumnStyles.Add(new ZDateEditColumnStyleInfo(DummyBizoSchema.Z0_Date.Name, 20));
				grid.ColumnStyles.Add(new ZDateEditColumnStyleInfo(DummyBizoSchema.Z0_AnotherDate.Name, 20));
				form.Controls.Add(grid);
				grid.BindTo = ".";
				grid.SetDataBinding(Dummy.Collection, "");
				grid.Focus();
				KeySender.SendKeyDown(grid, grid.Handle, Keys.Tab);
				KeySender.SendKeyUp(grid, grid.Handle, Keys.Tab);
				AssertEquals("Tabbing in", 1, grid.CurrentCell.ColumnNumber);
				KeySender.SendKeyDown(grid, grid.Handle, Keys.Tab);
				KeySender.SendKeyUp(grid, grid.Handle, Keys.Tab);
				AssertEquals("Tabbing out", 2, grid.CurrentCell.ColumnNumber);
			}
		}

		public void TestShouldColumnHandleKey()
		{
			using (var style = new ZDateEditColumnStyle(Info))
			{
				INavigatingGridColumn column = style;

				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Control | Keys.Up));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Control | Keys.Down));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Alt | Keys.Up));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Alt | Keys.Down));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Control | Keys.Alt | Keys.Up));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Control | Keys.Alt | Keys.Down));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Control | Keys.Left));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Control | Keys.Right));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.F2));

				var editControl = (ZDateEdit)style.EditControl;
				editControl.Text = "ABC";

				editControl.DateTextBox.SelectionStart = 0;
				editControl.DateTextBox.SelectionLength = 3;
				AssertEquals("ShouldColumnHandleKey()", false, column.ShouldColumnHandleKey(Keys.Up));
				AssertEquals("ShouldColumnHandleKey()", false, column.ShouldColumnHandleKey(Keys.Down));
				AssertEquals("ShouldColumnHandleKey()", false, column.ShouldColumnHandleKey(Keys.Left));
				AssertEquals("ShouldColumnHandleKey()", false, column.ShouldColumnHandleKey(Keys.Right));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Shift | Keys.Left));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Shift | Keys.Right));

				editControl.DateTextBox.SelectionLength = 2;
				AssertEquals("ShouldColumnHandleKey()", false, column.ShouldColumnHandleKey(Keys.Up));
				AssertEquals("ShouldColumnHandleKey()", false, column.ShouldColumnHandleKey(Keys.Down));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Left));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Right));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Shift | Keys.Left));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Shift | Keys.Right));

				editControl.DateTextBox.SelectionLength = 0;
				AssertEquals("ShouldColumnHandleKey()", false, column.ShouldColumnHandleKey(Keys.Up));
				AssertEquals("ShouldColumnHandleKey()", false, column.ShouldColumnHandleKey(Keys.Down));
				AssertEquals("ShouldColumnHandleKey()", false, column.ShouldColumnHandleKey(Keys.Left));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Right));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Shift | Keys.Left));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Shift | Keys.Right));

				editControl.DateTextBox.SelectionStart = 1;
				AssertEquals("ShouldColumnHandleKey()", false, column.ShouldColumnHandleKey(Keys.Up));
				AssertEquals("ShouldColumnHandleKey()", false, column.ShouldColumnHandleKey(Keys.Down));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Left));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Right));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Shift | Keys.Left));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Shift | Keys.Right));

				editControl.DateTextBox.SelectionStart = 2;
				AssertEquals("ShouldColumnHandleKey()", false, column.ShouldColumnHandleKey(Keys.Up));
				AssertEquals("ShouldColumnHandleKey()", false, column.ShouldColumnHandleKey(Keys.Down));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Left));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Right));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Shift | Keys.Left));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Shift | Keys.Right));

				editControl.DateTextBox.SelectionStart = 3;
				AssertEquals("ShouldColumnHandleKey()", false, column.ShouldColumnHandleKey(Keys.Up));
				AssertEquals("ShouldColumnHandleKey()", false, column.ShouldColumnHandleKey(Keys.Down));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Left));
				AssertEquals("ShouldColumnHandleKey()", false, column.ShouldColumnHandleKey(Keys.Right));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Shift | Keys.Left));
				AssertEquals("ShouldColumnHandleKey()", true, column.ShouldColumnHandleKey(Keys.Shift | Keys.Right));
			}
		}

		public void TestFormattingInAnotherCulture()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture("zh-SG")))
			{
				var collection = new DummyBusinessObjectCollection(Factory);
				collection.SetReadOnlyIncludingChildren(true);
				var dummy = collection.AddNew();
				var dummy2 = collection.AddNew();
				dummy.Z0_Date = new ZDateTime(2008, 1, 2);
				dummy2.Z0_Date = new ZDateTime(2008, 1, 2);

				using (var form = new ZForm(collection))
				using (var grid = new ZGrid())
				{
					grid.SetBindingMember(".");
					var columnInfo = new ZDateEditColumnStyleInfo(DummyBizoSchema.Z0_Date.Name, 100);
					columnInfo.DateTimeFormat = ZDateTimePickerFormat.Long;
					grid.ColumnStyles.Add(columnInfo);
					grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Z0_VarCharMax.Name, 100));
					grid.Dock = DockStyle.Fill;
					form.Controls.Add(grid);
					form.Show();
					Application.DoEvents();

					var column = (ZDateEditColumnStyle)grid.TableStyles[0].GridColumnStyles[0];
					var text = (string)typeof(DataGridTextBoxColumn).InvokeMember("GetText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod, null, column, new object[] { dummy.Z0_Date });
					AssertEquals("02-Jan-08 00:00", text);
				}
			}
		}

		public void TestZDateCanBeProperlyDisplayed()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			collection.SetReadOnlyIncludingChildren(true);
			var dummy = collection.AddNew();
			dummy.Z0_DateOnly = new ZDate(2008, 1, 2);

			using (var form = new ZForm(collection))
			using (var grid = new ZGrid())
			{
				grid.SetBindingMember(".");
				var columnInfo = new ZDateEditColumnStyleInfo(dummy.Z0_DateOnlyInfo.Name, 100);
				grid.ColumnStyles.Add(columnInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				var column = (ZDateEditColumnStyle)grid.TableStyles[0].GridColumnStyles[0];
				var text = (string)typeof(DataGridTextBoxColumn).InvokeMember("GetText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod, null, column, new object[] { dummy.Z0_DateOnly });
				AssertEquals("02-Jan-08", text);
			}
		}

		[ExpectNoExceptions]
		public void TestZDateCanBeCommitted()
		{
			var collection = new DummyBusinessObjectCollection(Factory);

			using (var form = new ZForm(collection))
			using (var grid = new ZGrid())
			{
				grid.SetBindingMember(".");
				var columnInfo = new TestZDateEditColumnStyleInfo(DummyBizoSchema.Constants.Z0_DateOnly, 100);
				grid.ColumnStyles.Add(columnInfo);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				var columnStyle = (TestZDateEditColumnStyle)grid.Columns[0].ColumnStyle;
				columnStyle.IsEditingForTest = true;
				columnStyle.TestEditValue = new ZDateTime(2015, 3, 3);
				columnStyle.CommitForTest();
			}
		}

		public void TestCommitReadOnly()
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var item = collection.AddNew();

			using (var form = new ZForm(collection))
			{
				using (var grid = new ZGrid())
				{
					grid.SetBindingMember(".");
					var info = new TestZDateEditColumnStyleInfo(DummyBizoSchema.Constants.Z0_DateOnly, 100);
					grid.ColumnStyles.Add(info);
					grid.Dock = DockStyle.Fill;
					form.Controls.Add(grid);
					form.Show();
					Application.DoEvents();
					var style = (TestZDateEditColumnStyle)grid.Columns[0].ColumnStyle;

					style.IsEditingForTest = true;
					style.TestEditValue = new ZDateTime(2016, 02, 29);
					AssertEquals("Expecting success commit when cell is editable", true, style.Commit_DebugAccess(grid.ListManager, 0));
					AssertEquals("Expecting a value update to editable cell", new ZDate(2016, 02, 29), item.Z0_DateOnly);

					style.IsEditingForTest = true;
					style.ReadOnly = true;
					style.TestEditValue = new ZDateTime(2016, 01, 01);
					AssertEquals("Expecting success commit when cell is read-only", true, style.Commit_DebugAccess(grid.ListManager, 0));
					AssertEquals("Expecting NO value update to read-only cell", new ZDate(2016, 02, 29), item.Z0_DateOnly);
				}
			}
		}

		public void TestPrepareEditControlNeedSetDateTimeValue()
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var item = collection.AddNew();
			var calendar = CultureInfo.InvariantCulture.Calendar;
			var year = calendar.TwoDigitYearMax + 1;
			item.Z0_Date = new ZDateTime(year, 8, 7);
			var expectedDate = item.Z0_Date.ToString("yyyy-MM-dd").ToUpper();

			using (var form = new ZForm(collection))
			using (var grid = new ZGrid())
			{
				grid.SetBindingMember(".");
				var info = new TestZDateEditColumnStyleInfo(DummyBizoSchema.Constants.Z0_Date, 100);
				grid.ColumnStyles.Add(info);
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				grid.Focus();
				var style = (TestZDateEditColumnStyle)grid.Columns[0].ColumnStyle;
				var zDateEdit = (ZDateEdit)style.GridControl;
				var actualDate = zDateEdit.DateTimeValue.ToString("yyyy-MM-dd").ToUpper();

				AssertEquals(expectedDate, actualDate);
			}
		}

		class TestZDateEditColumnStyleInfo : ZDateEditColumnStyleInfo
		{
			public TestZDateEditColumnStyleInfo(string columnName, int width)
				: base(columnName, width) { }

			[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
			public override Type ColumnStyleType
			{
				get { return typeof(TestZDateEditColumnStyle); }
			}
		}

		class TestZDateEditColumnStyle : ZDateEditColumnStyle
		{
			public TestZDateEditColumnStyle(ZDateEditColumnStyleInfo columnInfo)
				: base(columnInfo)
			{
			}

			public void CommitForTest()
			{
				var context = new BindingContext();
				var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
				collection.AddNew();

				var testCurrencyManager = (CurrencyManager)context[collection];
				Commit(testCurrencyManager, 0);
			}

			protected override object EditValue
			{
				get { return TestEditValue; }
			}

			public ZDateTime TestEditValue { get; set; }

			public bool IsEditingForTest
			{
				set { IsEditing = value; }
			}
		}

		readonly ZDateEditColumnStyleInfo Info = new ZDateEditColumnStyleInfo();
	}
}

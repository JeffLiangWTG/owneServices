using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common.Testing;
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
	sealed class ZDateTimeOffsetEditColumnStyleTest : TestCaseWithDummy
	{
		public void TestInvalidDefaultValue()
		{
			Dummy.Collection.AddNew();
			Dummy.Collection[0].Z0_DateTimeOffset = ZDateTimeOffset.Invalid;
			using (var form = new ZForm())
			{
				var grid = new ZGrid();
				grid.ColumnStyles.Add(new ZDateTimeOffsetEditColumnStyleInfo(DummyBizoSchema.Z0_DateTimeOffset.Name, 20));
				form.Controls.Add(grid);
				grid.BindTo = ".";
				grid.SetDataBinding(Dummy.Collection, "");
				form.Show();
				Application.DoEvents();
				grid.Focus();
				AssertEquals("<INVALID>", ((ZDateTimeOffsetEditColumnStyle)grid.Columns[0].ColumnStyle).EditControl.Text);
			}
		}

		public void TestTabbing()
		{
			using (var form = new ZForm())
			{
				form.Show();
				Application.DoEvents();

				var grid = new ZGrid();
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Z0_VarCharMax.Name, 20));
				grid.ColumnStyles.Add(new ZDateTimeOffsetEditColumnStyleInfo(DummyBizoSchema.Z0_DateTimeOffset.Name, 20));
				form.Controls.Add(grid);
				grid.BindTo = ".";
				grid.SetDataBinding(Dummy.Collection, "");
				grid.Focus();

				KeySender.SendKeyDown(grid, grid.Handle, Keys.Tab);
				KeySender.SendKeyUp(grid, grid.Handle, Keys.Tab);
				AssertEquals("Tabbing in", 1, grid.CurrentCell.ColumnNumber);
			}
		}

		public void TestShouldColumnHandleKey()
		{
			using (var style = new ZDateTimeOffsetEditColumnStyle(Info))
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

				var editControl = (ZDateTimeOffsetEdit)style.EditControl;
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

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestFormattingInAnotherCulture()
		{
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture("zh-SG")))
			{
				var collection = new DummyBusinessObjectCollection(Factory);
				collection.SetReadOnlyIncludingChildren(true);
				var dummy = collection.AddNew();
				var dummy2 = collection.AddNew();
				dummy.Z0_DateTimeOffset = new ZDateTimeOffset(2008, 1, 2, 0, 0, 0, TimeSpan.FromHours(11));
				dummy2.Z0_DateTimeOffset = new ZDateTimeOffset(2008, 1, 2, 0, 0, 0, TimeSpan.FromHours(11));

				using (var form = new ZForm(collection))
				using (var grid = new ZGrid())
				{
					grid.SetBindingMember(".");
					var columnInfo = new ZDateTimeOffsetEditColumnStyleInfo(DummyBizoSchema.Z0_DateTimeOffset.Name, 100);
					columnInfo.DateTimeFormat = ZDateTimePickerFormat.Long;
					grid.ColumnStyles.Add(columnInfo);
					grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Z0_VarCharMax.Name, 100));
					grid.Dock = DockStyle.Fill;
					form.Controls.Add(grid);
					form.Show();
					Application.DoEvents();

					var column = (ZDateTimeOffsetEditColumnStyle)grid.TableStyles[0].GridColumnStyles[0];
					var text = (string)typeof(DataGridTextBoxColumn).InvokeMember("GetText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod, null, column, new object[] { dummy.Z0_DateTimeOffset });
					AssertEquals("02-Jan-08 00:00", text);
				}
			}
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestFormattingIncludingGMT()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = true;

			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture("en-AU")))
			using (RawDataRegistry.Instance.DisplayUtcOffset.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				{
					var collection = new DummyBusinessObjectCollection(Factory);
					var dummy = collection.AddNew();
					dummy.Z0_DateTimeOffset = new ZDateTimeOffset(2018, 1, 2, 5, 26, 0, TimeSpan.FromHours(10));

					var columnInfo = new ZDateTimeOffsetEditColumnStyleInfo(DummyBizoSchema.Z0_DateTimeOffset.Name, 100);
					columnInfo.DateTimeFormat = ZDateTimePickerFormat.Long;

					using (var columnStyle = new ZDateTimeOffsetEditColumnStyle(columnInfo))
					{
						AssertEquals("02-JAN-18 05:26 GMT+10:00", columnStyle.FormatValueObject(dummy, dummy.Z0_DateTimeOffset));
					}
				}
			}
		}

		public void TestCommitWithOffset()
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			var item = collection.AddNew();

			item.Z0_DateTimeOffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday, TimeSpan.FromHours(10));

			using (var form = new ZForm(collection))
			{
				using (var grid = new ZGrid())
				{
					grid.SetBindingMember(".");
					var info = new TestZDateTimeOffsetEditColumnStyleInfo("Z0_DateTimeOffset", 100);
					grid.ColumnStyles.Add(info);
					grid.Dock = DockStyle.Fill;
					form.Controls.Add(grid);
					form.Show();
					Application.DoEvents();
					var style = (TestZDateTimeOffsetEditColumnStyle)grid.Columns[0].ColumnStyle;

					style.IsEditingForTest = true;
					style.TestEditOffsetValue = new ZDateTimeOffset(ZDateTime.BrettsBirthday, TimeSpan.FromHours(4));
					AssertEquals("Expecting success commit when cell is editable", true, style.Commit_DebugAccess(grid.ListManager, 0));
					AssertEquals("Expecting a value update to editable cell", style.TestEditOffsetValue, item.Z0_DateTimeOffset);
				}
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
					var info = new TestZDateTimeOffsetEditColumnStyleInfo("Z0_DateTimeOffset", 100);
					grid.ColumnStyles.Add(info);
					grid.Dock = DockStyle.Fill;
					form.Controls.Add(grid);
					form.Show();
					Application.DoEvents();
					var style = (TestZDateTimeOffsetEditColumnStyle)grid.Columns[0].ColumnStyle;

					style.IsEditingForTest = true;
					style.TestEditValue = new ZDateTime(2016, 02, 29);
					AssertEquals("Expecting success commit when cell is editable", true, style.Commit_DebugAccess(grid.ListManager, 0));
					AssertEquals("Expecting a value update to editable cell", new ZDateTimeOffset(new ZDate(2016, 02, 29)), item.Z0_DateTimeOffset);

					style.IsEditingForTest = true;
					style.ReadOnly = true;
					style.TestEditValue = new ZDateTime(2016, 01, 01);
					AssertEquals("Expecting success commit when cell is read-only", true, style.Commit_DebugAccess(grid.ListManager, 0));
					AssertEquals("Expecting NO value update to read-only cell", new ZDateTimeOffset(new ZDate(2016, 02, 29)), item.Z0_DateTimeOffset);
				}
			}
		}

		class TestZDateTimeOffsetEditColumnStyleInfo : ZDateTimeOffsetEditColumnStyleInfo
		{
			public TestZDateTimeOffsetEditColumnStyleInfo(string columnName, int width)
				: base(columnName, width)
			{ }

			[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
			public override Type ColumnStyleType
			{
				get { return typeof(TestZDateTimeOffsetEditColumnStyle); }
			}
		}

		class TestZDateTimeOffsetEditColumnStyle : ZDateTimeOffsetEditColumnStyle
		{
			public TestZDateTimeOffsetEditColumnStyle(ZDateTimeOffsetEditColumnStyleInfo columnInfo)
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
				get { return TestEditOffsetValue ?? new ZDateTimeOffset(TestEditValue); }
			}

			public ZDateTime TestEditValue { get; set; }

			public ZDateTimeOffset? TestEditOffsetValue { get; set; }

			public bool IsEditingForTest
			{
				set { IsEditing = value; }
			}
		}

		readonly ZDateTimeOffsetEditColumnStyleInfo Info = new ZDateTimeOffsetEditColumnStyleInfo();
	}
}

using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class ZCalcEditColumnStyleTest : TestCaseWithDummy
	{
		[ExpectNoExceptions]
		public void TestAbort()
		{
			var columnInfo = new TestOGridCalcEditColumnInfo { ColumnName = "Column1" };

#pragma warning disable CW1108 // Do Not Use DataSet
			var data = new DataSet(); // for testing only
#pragma warning restore CW1108 // Do Not Use DataSet
			var table = data.Tables.Add("Table");
			var column = table.Columns.Add("Column1", typeof(ZInt));
			column.DefaultValue = ZInt.Zero;
			column.AllowDBNull = false;

			using (var form = new KForm())
			{
				var grid = new ZGrid();
				grid.GridId = Guid.NewGuid().ToString();
				grid.Columns.Add(columnInfo);

				form.Controls.Add(grid);
				grid.SetDataBinding(data, "Table");
				form.Show();

				((TestOGridCalcEditColumn)grid.Columns[0].ColumnStyle).AbortForTest(0);
				((TestOGridCalcEditColumn)grid.Columns[0].ColumnStyle).AbortForTest(1);
			}
		}

		class TestOGridCalcEditColumnInfo : ZCalcEditColumnStyleInfo
		{
			[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
			public override Type ColumnStyleType
			{
				get { return typeof(TestOGridCalcEditColumn); }
			}
		}

		class TestOGridCalcEditColumn : ZCalcEditColumnStyle
		{
			public TestOGridCalcEditColumn(ZCalcEditColumnStyleInfo columnInfo) : base(columnInfo) { }

			public void AbortForTest(int rowNum)
			{
				Abort(rowNum);
			}
		}

		public void TestDecimals()
		{
			using (var column = new ZCalcEditColumnStyle(new ZCalcEditColumnStyleInfo("Test", 0, 9)))
			{
				AssertEquals("0.000000000", column.Format);
			}

			using (var column = new ZCalcEditColumnStyle(new ZCalcEditColumnStyleInfo("Test", 0, 3)))
			{
				AssertEquals("0.000", column.Format);

				column.Decimals = 1;
				AssertEquals("0.0", column.Format);

				column.Decimals = 0;
				AssertEquals("0", column.Format);

				column.Decimals = -1;
				AssertEquals("0", column.Format);

				column.Decimals = 3;
				AssertEquals("0.000", column.Format);
			}
		}

		public void TestCommitWithBlankText()
		{
#pragma warning disable CW1108 // Do Not Use DataSet
			var data = new DataSet(); // for testing only
#pragma warning restore CW1108 // Do Not Use DataSet
			var table = data.Tables.Add("Table");
			table.Columns.Add("Column1", typeof(ZDecimal));

			using (var testForm = new ZForm())
			{
				var testGrid = new ZGrid();
				testGrid.GridId = Guid.NewGuid().ToString();
				testGrid.Columns.Add(new ZCalcEditColumnStyleInfo("Column1", 80, 3));

				testForm.Controls.Add(testGrid);
				testGrid.SetDataBinding(data, "Table");
				testForm.Show();
				testGrid.ListManager.AddNew();

				testGrid.Focus();
				testGrid.CurrentCell = new DataGridCell(0, 0);

				KeySender.SendKeyPress(testGrid.LastFocusedColumn.EditControl, testGrid.LastFocusedColumn.EditControl.Handle, Keys.D1);
				KeySender.SendKeyPress(testGrid.LastFocusedColumn.EditControl, testGrid.LastFocusedColumn.EditControl.Handle, Keys.D0);
				foreach (var c in Enterprise.ZArchitecture.Core.Culture.Current.NumberFormat.NumberDecimalSeparator)
				{
					KeySender.SendKeyPress(testGrid.LastFocusedColumn.EditControl, testGrid.LastFocusedColumn.EditControl.Handle, c);
				}

				KeySender.SendKeyPress(testGrid.LastFocusedColumn.EditControl, testGrid.LastFocusedColumn.EditControl.Handle, Keys.D4);
				KeySender.SendKeyPress(testGrid.LastFocusedColumn.EditControl, testGrid.LastFocusedColumn.EditControl.Handle, Keys.D4);
				KeySender.SendKeyPress(testGrid.LastFocusedColumn.EditControl, testGrid.LastFocusedColumn.EditControl.Handle, Keys.D4);
				KeySender.SendKeyPress(testGrid.LastFocusedColumn.EditControl, testGrid.LastFocusedColumn.EditControl.Handle, Keys.D4);

				testGrid.CurrentCell = new DataGridCell(1, 0);
				AssertEquals("Rounded value committed to Row", 10.444M, table.Rows[0][0]);

				// begin edit without entering a value
				KeySender.SendKeyPress(testGrid.LastFocusedColumn.EditControl, testGrid.LastFocusedColumn.EditControl.Handle, Keys.Back);
				testGrid.CurrentCell = new DataGridCell(2, 0);
				AssertEquals("0 committed to Row", 0M, table.Rows[1][0]);

				KeySender.PostKeyDown(testGrid.LastFocusedColumn.EditControl, testGrid.LastFocusedColumn.EditControl.Handle, Keys.Subtract);
				Application.DoEvents();
				testGrid.CurrentCell = new DataGridCell(3, 0);
				AssertEquals("0 committed to Row", 0M, table.Rows[2][0]);
			}
		}

		public void TestCommitReadOnly()
		{
			var dummy = Factory.New<DummyWithDummies>();
			var item = Factory.New<ZCalcEditTest.DummyWithDecimals>();
			dummy.Dummies.Add(item);

			using (var form = new ZForm(dummy))
			{
				using (var grid = new ZGrid { Location = new Point(0, 0), Size = new Size(200, 200) })
				{
					form.Controls.Add(grid);
					var info = new ZCalcEditColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Number };
					grid.ColumnStyles.Add(info);
					grid.SetDataBinding(dummy, "Dummies", dummy.Collection.GetType().Name);

					using (var style = new ZCalcEditColumnStyle(info))
					{
						style.PropertyDescriptor = TypeDescriptor
							.GetProperties(typeof(ZCalcEditTest.DummyWithDecimals))
							.OfType<PropertyDescriptor>()
							.Single(descriptor => descriptor.DisplayName == DummyBizoSchema.Constants.Z0_Number);

						style.TextBox.Text = "42";
						((DataGridTextBox)style.TextBox).IsInEditOrNavigateMode = false;
						AssertEquals("Expecting success commit when cell is editable", true, style.Commit_DebugAccess(grid.ListManager, 0));
						AssertEquals("Expecting a value update to editable cell", 42, item.Z0_Number);

						style.isCurrentCellReadOnlyForTest = true;
						style.TextBox.Text = "1985";
						((DataGridTextBox)style.TextBox).IsInEditOrNavigateMode = false;
						AssertEquals("Expecting success commit when cell is read-only", true, style.Commit_DebugAccess(grid.ListManager, 0));
						AssertEquals("Expecting NO value update to read-only cell", 42, item.Z0_Number);
					}
				}
			}
		}

		public void TestShowEmptyStringForEmptyValue()
		{
			var testControl = new ZCalcEditColumnStyleInfo();
			AssertEquals(false, testControl.ShowEmptyStringForEmptyValue);
			testControl.ShowEmptyStringForEmptyValue = false;
			AssertEquals(false, testControl.ShowEmptyStringForEmptyValue);
			testControl.ShowEmptyStringForEmptyValue = true;
			AssertEquals(true, testControl.ShowEmptyStringForEmptyValue);
			testControl.ShowEmptyStringForEmptyValue = false;
			AssertEquals(false, testControl.ShowEmptyStringForEmptyValue);
		}

		public void TestSettingDecimalsUpdatesBoundData()
		{
#pragma warning disable CW1108 // Do Not Use DataSet
			var data = new DataSet("Data"); // for testing only
#pragma warning restore CW1108 // Do Not Use DataSet
			var table = data.Tables.Add("Table");
			table.Columns.Add(new DataColumn("DecimalColumn", typeof(ZDecimal)));
			table.Columns.Add(new DataColumn("StringColumn", typeof(ZString)));

			using (var testForm = new ZForm())
			{
				var testGrid = new ZGrid();
				testGrid.Columns.Add(new ZCalcEditColumnStyleInfo("DecimalColumn", 80, 4));
				testGrid.Columns.Add(new ZTextBoxColumnStyleInfo("StringColumn", 80));

				var calcEditColumn = (ZCalcEditColumnStyle)testGrid.Columns["DecimalColumn"].ColumnStyle;

				testForm.Controls.Add(testGrid);
				testGrid.SetDataBinding(data, "Table");
				testForm.Show();
				testGrid.ListManager.AddNew();

				var decimalCell = new DataGridCell(0, 0);
				var stringCell = new DataGridCell(0, 1);

				testGrid.Focus();
				testGrid.CurrentCell = decimalCell;

				KeySender.SendKeyPress(testGrid.LastFocusedColumn.EditControl, testGrid.LastFocusedColumn.EditControl.Handle, Keys.D0);

				foreach (var c in Enterprise.ZArchitecture.Core.Culture.Current.NumberFormat.NumberDecimalSeparator)
				{
					KeySender.SendKeyPress(testGrid.LastFocusedColumn.EditControl, testGrid.LastFocusedColumn.EditControl.Handle, c);
				}

				KeySender.SendKeyPress(testGrid.LastFocusedColumn.EditControl, testGrid.LastFocusedColumn.EditControl.Handle, Keys.D1);
				KeySender.SendKeyPress(testGrid.LastFocusedColumn.EditControl, testGrid.LastFocusedColumn.EditControl.Handle, Keys.D2);
				KeySender.SendKeyPress(testGrid.LastFocusedColumn.EditControl, testGrid.LastFocusedColumn.EditControl.Handle, Keys.D3);
				KeySender.SendKeyPress(testGrid.LastFocusedColumn.EditControl, testGrid.LastFocusedColumn.EditControl.Handle, Keys.D4);
				KeySender.SendKeyPress(testGrid.LastFocusedColumn.EditControl, testGrid.LastFocusedColumn.EditControl.Handle, Keys.D5);

				testGrid.CurrentCell = stringCell;
				AssertEquals("Decimals is 4, value should be 0.1234", 0.1234M, table.Rows[0][0]);

				calcEditColumn.Decimals = 3;
				AssertEquals("Decimals is 3, value should be 0.123", 0.123M, table.Rows[0][0]);

				calcEditColumn.Decimals = 4;
				AssertEquals("Decimals is 4, value should be 0.123", 0.123M, table.Rows[0][0]);

				calcEditColumn.Decimals = 0;
				AssertEquals("Decimals is 0, value should be 0", 0.0M, table.Rows[0][0]);
			}
		}

		[DeveloperOnlyTest]
		public void TestPastedValueShouldNotExceedMaxValueInfo()
		{
			using (var form = new ZForm(Factory.NewWithValidTestData<DummyBusinessObject>()))
			using (var grid = new ZGrid())
			{
				grid.BindTo = "Collection";
				var info = new ZCalcEditColumnStyleInfo(DummyBusinessObject.Schema.Z0_Decimal, 0, 9);
				info.MaxValue = 10000.12m;
				grid.Columns.Add(info);
				form.Controls.Add(grid);
				form.Show();

				var column = (ZCalcEditColumnStyle)grid.Columns[DummyBusinessObject.Schema.Z0_Decimal].ColumnStyle;

				var m = new Message();

				SafeClipboard.SetDataObject("345879.15");
				System.Threading.Thread.Sleep(50);

				column.ReadOnly = false;
				column.TextBox.ReadOnly = false;
				Assert(((ICustomKeyHandlingGridColumn)column).ProcessCmdKey(ref m, (Keys.Control | Keys.V)));
				AssertEquals("Should not exceed the limit of 10000.12", "10000.120000000", column.TextBox.Text);
			}
		}

		[DeveloperOnlyTest]
		public void TestPastedValueShouldBeCommittedByCtrlV()
		{
			using (var form = new ZForm(Factory.NewWithValidTestData<DummyBusinessObject>()))
			using (var grid = new TestZGrid())
			{
				grid.BindTo = "Collection";
				var info = new ZCalcEditColumnStyleInfo(DummyBusinessObject.Schema.Z0_Decimal, 0, 9);
				grid.Columns.Add(info);
				form.Controls.Add(grid);
				form.Show();

				var column = (ZCalcEditColumnStyle)grid.Columns[DummyBusinessObject.Schema.Z0_Decimal].ColumnStyle;

				var m = new Message();

				SafeClipboard.SetDataObject("123");
				System.Threading.Thread.Sleep(50);

				column.ReadOnly = false;
				column.TextBox.ReadOnly = false;
				Assert(((ICustomKeyHandlingGridColumn)column).ProcessCmdKey(ref m, (Keys.Control | Keys.V)));
				grid.Focus();
				AssertEquals("Should have been pasted successfully by control v", "123.000000000", column.TextBox.Text);
			}
		}

		public void TestShouldProcessCmdKey()
		{
			using (var form = new ZForm(Factory.NewWithValidTestData<DummyBusinessObject>()))
			using (var grid = new ZGrid())
			{
				grid.BindTo = "Collection";
				grid.Columns.Add(new ZCalcEditColumnStyleInfo(DummyBusinessObject.Schema.Z0_Decimal, 0, 9));
				form.Controls.Add(grid);
				form.Show();

				var column = (ZCalcEditColumnStyle)grid.Columns[DummyBusinessObject.Schema.Z0_Decimal].ColumnStyle;

				var m = new Message();

				column.ReadOnly = false;
				column.TextBox.ReadOnly = false;
				column.Core.IsCalculatorEnabled = true;
				Assert(((ICustomKeyHandlingGridColumn)column).ProcessCmdKey(ref m, Keys.F4));
				Assert(!((ICustomKeyHandlingGridColumn)column).ProcessCmdKey(ref m, Keys.F3));

				column.ReadOnly = true;
				Assert(!((ICustomKeyHandlingGridColumn)column).ProcessCmdKey(ref m, Keys.F4));

				column.ReadOnly = false;
				Assert(((ICustomKeyHandlingGridColumn)column).ProcessCmdKey(ref m, Keys.F4));

				column.TextBox.ReadOnly = true;
				Assert(!((ICustomKeyHandlingGridColumn)column).ProcessCmdKey(ref m, Keys.F4));

				column.TextBox.ReadOnly = false;
				Assert(((ICustomKeyHandlingGridColumn)column).ProcessCmdKey(ref m, Keys.F4));

				column.Core.IsCalculatorEnabled = false;
				Assert(!((ICustomKeyHandlingGridColumn)column).ProcessCmdKey(ref m, Keys.F4));

				column.Core.IsCalculatorEnabled = true;
				Assert(((ICustomKeyHandlingGridColumn)column).ProcessCmdKey(ref m, Keys.F4));
			}
		}

		#region Test DecimalPlaces loading from metadata

		public void TestDecimalPlacesLoadingFromMetadata()
		{
			var mainDummy = Factory.New<DummyWithDummies>();
			mainDummy.Dummies.Add(Factory.New<ZCalcEditTest.DummyWithDecimals>());
			mainDummy.Dummies.Add(Factory.New<ZCalcEditTest.DummyWithMetaData>());
			mainDummy.Dummies.Add(Factory.New<ZCalcEditTest.DummyWithMetaDataProperty>());
			mainDummy.Dummies.Add(Factory.New<ZCalcEditTest.DummyWithSpecialMetaDataProperty>());

			using (var form = new ZDecimalTestForm(mainDummy, "Z0_AnotherDecimal"))
			{
				form.Show();
				Application.DoEvents();

				AssertDummyWithDecimals(form, 0, DummyBizoSchema.Z0_AnotherDecimal.Scale, 5, 6, 6);
				AssertDummyWithDecimals(form, 1, 4, 5, 6, 6);
				AssertDummyWithDecimals(form, 2, 4, 5, 6, 6);
				AssertDummyWithDecimals(form, 3, 4, 5, 6, 6);
				AssertDummyWithDecimals(form, 0, DummyBizoSchema.Z0_AnotherDecimal.Scale, 5, 6, 6);
			}
		}

		void AssertDummyWithDecimals(ZDecimalTestForm form, int rowNo, int raw, int gui, int bound, int guiAndBound)
		{
			form.Grid1.CurrentCell = new DataGridCell(rowNo, 0);
			form.GridWithGui.CurrentCell = new DataGridCell(rowNo, 0);
			form.GridWithBinding.CurrentCell = new DataGridCell(rowNo, 0);
			form.GridWithGuiAndBinding.CurrentCell = new DataGridCell(rowNo, 0);
			Application.DoEvents();

			// Do not know why it is needed to call BeginEdit in test - functionally it works without it
			form.Grid1.BeginEdit(form.Grid1.Columns[0].ColumnStyle, rowNo);
			form.GridWithGui.BeginEdit(form.GridWithGui.Columns[0].ColumnStyle, rowNo);
			form.GridWithBinding.BeginEdit(form.GridWithBinding.Columns[0].ColumnStyle, rowNo);
			form.GridWithGuiAndBinding.BeginEdit(form.GridWithGuiAndBinding.Columns[0].ColumnStyle, rowNo);
			Application.DoEvents();

			AssertEquals(raw, ((ZCalcEditColumnStyle)form.Grid1.Columns[0].ColumnStyle).Decimals);
			AssertEquals(gui, ((ZCalcEditColumnStyle)form.GridWithGui.Columns[0].ColumnStyle).Decimals);
			AssertEquals(bound, ((ZCalcEditColumnStyle)form.GridWithBinding.Columns[0].ColumnStyle).Decimals);
			AssertEquals(guiAndBound, ((ZCalcEditColumnStyle)form.GridWithGuiAndBinding.Columns[0].ColumnStyle).Decimals);

			AssertEquals(GetExpectedZeroString(raw), ((ZCalcEditColumnStyle)form.Grid1.Columns[0].ColumnStyle).TextBox.Text);
			AssertEquals(GetExpectedZeroString(gui), ((ZCalcEditColumnStyle)form.GridWithGui.Columns[0].ColumnStyle).TextBox.Text);
			AssertEquals(GetExpectedZeroString(bound), ((ZCalcEditColumnStyle)form.GridWithBinding.Columns[0].ColumnStyle).TextBox.Text);
			AssertEquals(GetExpectedZeroString(guiAndBound), ((ZCalcEditColumnStyle)form.GridWithGuiAndBinding.Columns[0].ColumnStyle).TextBox.Text);
		}

		string GetExpectedZeroString(int decimalPoints)
		{
			return decimalPoints == 0 ? "0" : "0.".PadRight(decimalPoints + 2, '0');
		}

		class DummyWithDummies : DummyBusinessObject
		{
			public DummyWithDummies(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public DummyWithDecimalsCollection Dummies
			{
				get
				{
					if (dummies == null)
					{
						dummies = new DummyWithDecimalsCollection(Factory);
						RegisterEditableChildObject(dummies);
					}
					return dummies;
				}
			}
			DummyWithDecimalsCollection dummies;
		}

		class DummyWithDecimalsCollection : BusinessObjectCollection<ZCalcEditTest.DummyWithDecimals>
		{
			public DummyWithDecimalsCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		class ZDecimalTestForm : ZChildForm
		{
			public ZDecimalTestForm(DummyWithDummies bizo, string property)
				: base(bizo)
			{
				InitializeComponent(property);
			}

			public ZGrid Grid1;
			public ZGrid GridWithGui;
			public ZGrid GridWithBinding;
			public ZGrid GridWithGuiAndBinding;

			void InitializeComponent(string property)
			{
				SuspendLayout();

				Grid1 = new ZGrid { Location = new Point(0, 0), Size = new Size(200, 200) };
				Grid1.BeginInit();
				Controls.Add(Grid1);
				BindingSource.SetBindingMember(Grid1, "Dummies");
				Grid1.ColumnStyles.Add(
					new ZCalcEditColumnStyleInfo
					{
						IsMandatory = true,
						ColumnName = property
					});
				Grid1.EndInit();

				GridWithGui = new ZGrid { Location = new Point(200, 0), Size = new Size(200, 200) };
				GridWithGui.BeginInit();
				Controls.Add(GridWithGui);
				BindingSource.SetBindingMember(GridWithGui, "Dummies");
				GridWithGui.ColumnStyles.Add(
					new ZCalcEditColumnStyleInfo
					{
						IsMandatory = true,
						ColumnName = property,
						Decimals = 5
					});
				GridWithGui.EndInit();

				GridWithBinding = new ZGrid { Location = new Point(0, 200), Size = new Size(200, 200) };
				GridWithBinding.BeginInit();
				Controls.Add(GridWithBinding);
				BindingSource.SetBindingMember(GridWithBinding, "Dummies");
				GridWithBinding.ColumnStyles.Add(
					new ZCalcEditColumnStyleInfo
					{
						IsMandatory = true,
						ColumnName = property,
						BindToDecimalPlaces = "BindableDecimalPlaces"
					});
				GridWithBinding.EndInit();

				GridWithGuiAndBinding = new ZGrid { Location = new Point(200, 200), Size = new Size(200, 200) };
				GridWithGuiAndBinding.BeginInit();
				Controls.Add(GridWithGuiAndBinding);
				BindingSource.SetBindingMember(GridWithGuiAndBinding, "Dummies");
				GridWithGuiAndBinding.ColumnStyles.Add(
					new ZCalcEditColumnStyleInfo
					{
						IsMandatory = true,
						ColumnName = property,
						Decimals = 5,
						BindToDecimalPlaces = "BindableDecimalPlaces"
					});
				GridWithGuiAndBinding.EndInit();

				ClientSize = new Size(400, 400);
				ResumeLayout(true);
			}
		}

		#endregion

		#region Test DecimalPlaces loading from metadata of sub-property

		class SubDummy : DummyBusinessObject
		{
			public SubDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[DecimalPlaces("Decimals")]
			public ZDecimal Rate { get; set; }

			public ZPropertyInfo RateInfo
			{
				get { return GetZPropertyInfo(nameof(Rate)); }
			}

			protected int Decimals { get { return 7; } }
		}

		class SuperDummy : DummyBusinessObject
		{
			public SuperDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public SubDummy SubDummy
			{
				get
				{
					if (subDummy == null)
					{
						subDummy = Factory.New<SubDummy>();
						RegisterEditableChildObject(subDummy);
					}
					return subDummy;
				}
			}
			SubDummy subDummy;
		}

		class SuperDummyCollection : BusinessObjectCollection<SuperDummy>
		{
			public SuperDummyCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		class SuperPuperDummy : DummyBusinessObject
		{
			public SuperPuperDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public SuperDummyCollection SuperCollection
			{
				get
				{
					if (superCollection == null)
					{
						superCollection = new SuperDummyCollection(Factory);
						superCollection.Load();
					}
					return superCollection;
				}
			}
			SuperDummyCollection superCollection;
		}

		class ZDecimalTestForm2 : ZChildForm
		{
			public ZDecimalTestForm2(SuperPuperDummy bizo)
				: base(bizo)
			{
				InitializeComponent();
			}

			public ZGrid Grid;

			new void InitializeComponent()
			{
				SuspendLayout();

				Grid = new ZGrid { Location = new Point(0, 0), Size = new Size(200, 200) };
				Grid.BeginInit();
				Controls.Add(Grid);
				BindingSource.SetBindingMember(Grid, "SuperCollection");
				Grid.ColumnStyles.Add(
					new ZCalcEditColumnStyleInfo
					{
						IsMandatory = true,
						ColumnName = "SubDummy+Rate"
					});
				Grid.EndInit();

				ClientSize = new Size(400, 400);
				ResumeLayout(true);
			}
		}

		public void TestDecimalPlacesLoadingFromMetadataOfSubProperty()
		{
			var mainDummy = Factory.New<SuperPuperDummy>();
			var dummy = Factory.New<SuperDummy>();
			AssertNotNull(dummy.SubDummy);
			mainDummy.SuperCollection.Add(dummy);

			using (var form = new ZDecimalTestForm2(mainDummy))
			{
				form.Show();
				Application.DoEvents();

				form.Grid.CurrentCell = new DataGridCell(0, 0);
				Application.DoEvents();

				// Do not know why it is needed to call BeginEdit in test - functionally it works without it
				form.Grid.BeginEdit(form.Grid.Columns[0].ColumnStyle, 0);

				AssertEquals(7, ((ZCalcEditColumnStyle)form.Grid.Columns[0].ColumnStyle).Decimals);
				AssertEquals(GetExpectedZeroString(7), ((ZCalcEditColumnStyle)form.Grid.Columns[0].ColumnStyle).TextBox.Text);
			}
		}

		#endregion

		#region Test DecimalPlaces for integer property

		public void TestDecimalPlacesForIntegerProperty()
		{
			var mainDummy = Factory.New<DummyWithDummies>();
			mainDummy.Dummies.Add(Factory.New<ZCalcEditTest.DummyWithDecimals>());
			mainDummy.Dummies.Add(Factory.New<ZCalcEditTest.DummyWithDecimals>());

			using (var form = new ZDecimalTestForm(mainDummy, "Z0_Number"))
			{
				form.Show();
				Application.DoEvents();
				AssertDummyWithDecimals(form, 0, 0, 0, 0, 0);
				AssertDummyWithDecimals(form, 1, 0, 0, 0, 0);
				AssertDummyWithDecimals(form, 0, 0, 0, 0, 0);
			}
		}

		#endregion

		#region Test CommitDecimal is rounded correctly

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "Auto generated baseline suppressions - WI00545660")]
		public void TestDecimalShouldBeRoundWhenCommit()
		{
			var dataSet = new DataSet(); // for testing only
			var table = dataSet.Tables.Add("Table");
			table.Columns.Add(new DataColumn("DecimalColumn", typeof(ZDecimal)));
			using (var testForm = new ZForm())
			{
				var testGrid = new ZGrid();
				testGrid.Columns.Add(new ZCalcEditColumnStyleInfo("DecimalColumn", 80, 3));
				testForm.Controls.Add(testGrid);
				testGrid.SetDataBinding(dataSet, "Table");
				var calcEditColumnStyle = testGrid.Columns["DecimalColumn"].ColumnStyle as ZCalcEditColumnStyle;
				testGrid.ListManager.AddNew();
				testForm.Show();
				var decimalCell = new DataGridCell(0, 0);
				testGrid.CurrentCell = decimalCell;
				calcEditColumnStyle.TextBox.Text = "1.1111";
				((DataGridTextBox)calcEditColumnStyle.TextBox).IsInEditOrNavigateMode = false;
				calcEditColumnStyle.Commit_DebugAccess(testGrid.ListManager, 0);
				CombineAssertions(() =>
				{
					AssertEquals("Expecting the value is ", "1.111", calcEditColumnStyle.TextBox.Text);
					AssertEquals("Expecting the value is ", 1.111m, table.Rows[0].ItemArray[0]);
				});
			}
		}

		#endregion
	}
	public class TestNotificationsWithEditControl : NotificationInGridTestCase
	{
		public override void TestLocationAndSizeWithAndWithoutNotifications()
		{
			CheckSizeAndLocation(50, new Point(37, 21), new Size(47, 13));

			Dummy.Collection[0].Z0_NumberInfo.AddError("baad");
			CheckSizeAndLocation(50, new Point(49, 21), new Size(35, 13));
		}

		protected override string GetColumnName()
		{
			return AutoDummyBizo.Schema.Z0_Number;
		}

		protected override ZGridColumnInfo GetInfo()
		{
			return new ZCalcEditColumnStyleInfo();
		}
	}
}

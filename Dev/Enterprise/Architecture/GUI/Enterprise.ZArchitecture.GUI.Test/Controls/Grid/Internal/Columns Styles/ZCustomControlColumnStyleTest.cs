using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class ZCustomControlColumnStyleTest : TransactionedTestCase
	{
		class ZCustomControlColumnStyleForTestEnterEvent : ZCustomControlColumnStyle
		{
			public ZCustomControlColumnStyleForTestEnterEvent(Func<Control> initializeEditControl, ZTextBoxColumnStyleInfo columnInfo)
				: base(initializeEditControl, columnInfo)
			{ }

			protected override object EditValue => null;

			public int ColumnTextBoxChangedCalledCount;
			protected override void ColumnTextBoxChanged(object sender, EventArgs e)
			{
				ColumnTextBoxChangedCalledCount++;
				base.ColumnTextBoxChanged(sender, e);
			}

			public void CallEnterEditControlTwiceForTesting()
			{
				EnterEditControl(this, EventArgs.Empty);
				EnterEditControl(this, EventArgs.Empty);
			}
		}

		public void TestEnterIsCorrectlyHooked()
		{
			var dummy = new BusinessObjectFactory().NewWithValidTestData<DummyBusinessObject>();
			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid { Location = new Point(0, 0), Size = new Size(200, 200) })
			{
				grid.Dock = DockStyle.Fill;
				form.Controls.Add(grid);
				var info = new ZCodeFindBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code };
				grid.ColumnStyles.Add(info);
				dummy.Collection.AddNew();
				dummy.Collection.AddNew();
				grid.SetDataBinding(dummy, "Collection", dummy.Collection.GetType().Name);

				using (var col = new ZCustomControlColumnStyleForTestEnterEvent(() => new ZGridFindBox(), info))
				{
					col.SetParentGrid(grid);
					form.Show();
					grid.Focus();
					grid.GetNextControl(grid, false).Focus();

					var control = col.EditControl;
					col.CallEnterEditControlTwiceForTesting();
					AssertEquals(0, col.ColumnTextBoxChangedCalledCount);
					((IGridControl)control).Text = "HI";
					AssertEquals(1, col.ColumnTextBoxChangedCalledCount);
					((IGridControl)control).Text = "HELLO";
					AssertEquals(2, col.ColumnTextBoxChangedCalledCount);
				}
			}
		}

		#region TestBGColourEmpty
#if !WINZOR
		public void TestBGColourEmpty()
		{
			var dummy = new BusinessObjectFactory().NewWithValidTestData<DummyBusinessObject>();
			using (var form = new ZForm(dummy))
			{
				using (var grid = new ZGrid { Location = new Point(0, 0), Size = new Size(200, 200) })
				{
					form.Controls.Add(grid);
					var info2 = new ZCheckBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code };
					grid.ColumnStyles.Add(info2);
					dummy.Collection.AddNew();
					dummy.Collection.AddNew();
					grid.SetDataBinding(dummy, "Collection", dummy.Collection.GetType().Name);

					var control = new ZGridFindBox();

					using (var col = new ZCheckBoxColumnStyleForTest(() => control, info2))
					{
						col.SetParentGrid(grid);
						var bush = BrushProvider.FromColor(Color.Fuchsia);
						AssertEquals(bush, col.GetBackgroundColourBrush(bush, grid.ListManager, 0));

						grid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(Grid_ColourDeciding);
						col.ReadOnly = true;
						AssertEquals(Color.Azure, ((SolidBrush)col.GetBackgroundColourBrush(bush, grid.ListManager, 0)).Color);

						col.ReadOnly = false;
						AssertEquals(Color.LightGreen, ((SolidBrush)col.GetBackgroundColourBrush(bush, grid.ListManager, 0)).Color);
					}
				}
			}
		}
#endif

		void Grid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			e.Colour = Color.LightGreen;
			e.ReadOnlyColour = Color.Azure;
		}

		class ZCheckBoxColumnStyleForTest : ZCustomControlColumnStyle
		{
			public ZCheckBoxColumnStyleForTest(Func<Control> control, ZTextBoxColumnStyleInfo info)
				: base(control, info)
			{
			}

			protected override object EditValue
			{
				get { return null; }
			}

#if !WINZOR
			public new Brush GetBackgroundColourBrush(Brush originalBackBrush, CurrencyManager source, int paintingRowNum)
			{
				return base.GetBackgroundColourBrush(originalBackBrush, source, paintingRowNum);
			}
#endif
		}

		#endregion

		public void TestColumnTextAtRow()
		{
			using (var testColumn = new TestOGridCustomControlColumnWithNullValue(() => new TestGridControl(), new ZTextBoxColumnStyleInfo("TestColumn", 80)))
			{
				AssertEquals("", testColumn.ColumnTextAtRowForTest());
			}
		}

		public void TestControlTabStop()
		{
			var control = new Control();
			control.TabStop = false;
			using (var testColumn = new TestOGridCustomControlColumnWithNullValue(() => new TestGridControl(), new ZTextBoxColumnStyleInfo("TestColumn", 80)))
			{
				AssertEquals("Editing controls hosted in the columns shouldn't be TabStop", false, control.TabStop);
			}
		}

		public void TestMaxLengthWithUncommittedRow()
		{
			var dummy = new BusinessObjectFactory().NewWithValidTestData<DummyBusinessObject>();
			var bizObj = (DummyChildBusinessObject)((IBindingList)dummy.Collection).AddNew();
			bizObj.Z0_Code = "BBB";
			using (var grid = new ZGrid())
			{
				using (var testColumn = new TestOGridCustomControlColumnWithNullValue(() => new TestGridControl(), new ZTextBoxColumnStyleInfo("TestColumn", 80)))
				{
					var maxlength = testColumn.GetPublicMaxLength(bizObj);
					AssertEquals(-1, maxlength);
				}
			}
		}

		public void TestIsEditingShouldBeSetToFalseOnCommit()
		{
			using (var grid = new ZGrid())
			{
				using (var testColumn = new TestOGridCustomControlColumnWithNullValue(() => new TestGridControl(), new ZTextBoxColumnStyleInfo("TestColumn", 80)))
				{
					testColumn.SetParentGrid(grid);
					testColumn.ColumnValueAtRowForTest = new object();
					testColumn.EditValueForTest = new object();
					testColumn.IsEditingForTest = true;
					testColumn.CommitForTest();
					AssertEquals(false, testColumn.IsEditingValueWhenLastSettingColumnValue);
				}
			}
		}

		public void TestIndexOutOfRangeGetBackgroundColourBrush()
		{
			var dummy = new BusinessObjectFactory().NewWithValidTestData<DummyBusinessObject>();
			using (var form = new ZForm(dummy))
			{
				using (var grid = new ZGrid())
				{
					form.Controls.Add(grid);

					var info2 = new ZCheckBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code };
					grid.ColumnStyles.Add(info2);

					using (var col = new ZCheckBoxColumnStyleForTest(() => null, info2))
					{
						col.SetParentGrid(grid);
#if !WINZOR
						var bush = BrushProvider.FromColor(Color.Fuchsia);
						AssertNoExceptionThrown("IndexOutOfRangeException should not be generated", () => { bush = col.GetBackgroundColourBrush(bush, grid.ListManager, 0); });
#endif
					}

					form.Show();
				}
			}
		}

		#region TestShouldMoveCursorWoInitializedGridControl

		public void TestShouldMoveCursorWoInitializedGridControl()
		{
			using (var columnStyle = new ZCustomControlColumnStyleForTest(new ZTextBoxColumnStyleInfo("xyz", 0)))
			{
				columnStyle.GridControl.Text = "abc";
				columnStyle.TextBox.Text = "abc";

				Assert("Can navigate left", !columnStyle.ShouldMoveCursorExposed(Keys.Left));
				Assert("Cannot navigate right", columnStyle.ShouldMoveCursorExposed(Keys.Right));

				columnStyle.TextBox.SelectionStart = 3;
				columnStyle.TextBox.SelectionLength = 0;

				Assert("Cannot navigate left", columnStyle.ShouldMoveCursorExposed(Keys.Left));
				Assert("Can navigate right", !columnStyle.ShouldMoveCursorExposed(Keys.Right));

				columnStyle.TextBox.SelectionStart = 0;
				columnStyle.TextBox.SelectionLength = 3;

				Assert("Can navigate left", !columnStyle.ShouldMoveCursorExposed(Keys.Left));
				Assert("Can navigate right", !columnStyle.ShouldMoveCursorExposed(Keys.Right));

				columnStyle.GridControl.SelectionStart = 3;
				columnStyle.GridControl.SelectionLength = 0;

				Assert("Cannot navigate left (because of GridControl)", columnStyle.ShouldMoveCursorExposed(Keys.Left));
				Assert("Can navigate right (because of GridControl)", !columnStyle.ShouldMoveCursorExposed(Keys.Right));
			}
		}

		class ZCustomControlColumnStyleForTest : ZCustomControlColumnStyle
		{
			public ZCustomControlColumnStyleForTest(ZTextBoxColumnStyleInfo columnInfo) : base(() => new ZTextBox(), columnInfo) { }

			readonly ZTextBox internalTextBox = new ZTextBox();

			public override TextBox TextBox
			{
				get { return internalTextBox; }
			}

			public bool ShouldMoveCursorExposed(Keys keyData)
			{
				return ShouldMoveCursor(keyData);
			}

			protected override object EditValue
			{
				get { throw new NotImplementedException(); }
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				if (disposing)
				{
					internalTextBox.Dispose();
				}
			}
		}

		#endregion

		#region Implementation

		class TestOGridCustomControlColumnWithNullValue : ZCustomControlColumnStyle
		{
			public TestOGridCustomControlColumnWithNullValue(Func<Control> editControl, ZTextBoxColumnStyleInfo columnInfo)
				: base(editControl, columnInfo)
			{
			}

			public string ColumnTextAtRowForTest()
			{
				return base.ColumnTextAtRow(null, 0);
			}

			protected override object EditValue
			{
				get { return EditValueForTest; }
			}

			protected override object GetColumnValueAtRow(CurrencyManager source, int rowNum)
			{
				return ColumnValueAtRowForTest;
			}

			public int GetPublicMaxLength(BusinessObject bizObj)
			{
				return GetMaxLength(bizObj);
			}
			protected override void SetColumnValueAtRow(CurrencyManager source, int rowNum, object value)
			{
				IsEditingValueWhenLastSettingColumnValue = IsEditing;
				if (ColumnValueAtRowForTest == null)
				{
					base.SetColumnValueAtRow(source, rowNum, value);
				}
			}

			public void CommitForTest()
			{
				var context = new BindingContext();
				var testCurrencyManager = (CurrencyManager)context[new List<string>()];
				Commit(testCurrencyManager, -1);
			}

			public object EditValueForTest
			{
				get;
				set;
			}

			public object ColumnValueAtRowForTest
			{
				get;
				set;
			}

			public bool IsEditingValueWhenLastSettingColumnValue
			{
				get;
				set;
			}

			public bool IsEditingForTest
			{
				get { return IsEditing; }
				set { IsEditing = value; }
			}
		}

		#endregion
	}
}

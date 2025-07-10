using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDropEditColumnStyleTest : NotificationInGridTestCase
	{
		public override void TestLocationAndSizeWithAndWithoutNotifications()
		{
			var themeAdjust = 0;
			if (!Application.RenderWithVisualStyles)
			{
				themeAdjust = -1;
			}

			var expectedLocation = new Point(35, 19);
#if WINZOR
			// In Winzor, the editing control is rendered in front-end, and the default location is (0, 0).
			expectedLocation = new Point();
#endif
			CheckSizeAndLocation(50, expectedLocation, new Size(66 + themeAdjust, 16));

			Dummy.Collection[0].Z0_DescriptionInfo.AddError("baad");
			expectedLocation.X += 12;
			CheckSizeAndLocation(50, expectedLocation, new Size(54 + themeAdjust, 16));
		}

		protected override void AssertSizeAndLocation(Control control, Point expectedLocation, Size expectedSize)
		{
			AssertEquals("Location", expectedLocation, control.Location);
			AssertEquals("Width expected " + expectedSize.Width + " but was " + control.Size.Width, true, expectedSize.Width + 1 >= control.Size.Width && expectedSize.Width - 1 <= control.Size.Width);
			AssertEquals("Height expected " + expectedSize.Height + " but was " + control.Size.Height, true, expectedSize.Height + 1 >= control.Size.Height && expectedSize.Height - 1 <= control.Size.Height);
		}

		public void TestColumnTextBoxChangedReadOnly()
		{
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			collection.AddNew();

			using (var form = new ZForm(collection))
			{
				using (var grid = new ZGrid())
				{
					grid.SetBindingMember(".");
					var info = new TestZDropEditColumnStyleInfo(DummyBizoSchema.Constants.Z0_VarCharMax, 100);
					grid.ColumnStyles.Add(info);
					grid.Dock = DockStyle.Fill;
					form.Controls.Add(grid);
					form.Show();
					Application.DoEvents();
					var style = (TestZDropEditColumnStyle)grid.Columns[0].ColumnStyle;

					style.SetTestValue("This is SPARTA!");
					AssertEquals("Expecting a value update to not read-only cell", true, style.CurrentText.Equals("This is SPARTA!", StringComparison.OrdinalIgnoreCase));

					style.ReadOnly = true;
					style.SetTestValue("You are not SPARTAN!");
					AssertEquals("Expecting NO value update to read-only cell", true, style.CurrentText.Equals("This is SPARTA!", StringComparison.OrdinalIgnoreCase));
				}
			}
		}

		public void TestSetEmailCore()
		{
			var info = new TestZDropEditColumnStyleInfo(DummyBizoSchema.Constants.Z0_VarCharMax, 100);
			using (var column = new TestZDropEditColumnStyle(info))
			{
				AssertNullOrEmpty(column.DropEdit.Text);

				var emailAddress = "test@test.com";
				column.SetEmailCore(emailAddress);

				AssertEquals(column.DropEdit.Text, emailAddress);
			}
		}

		protected override string GetColumnName()
		{
			return AutoDummyBizo.Schema.Z0_Description;
		}

		protected override ZGridColumnInfo GetInfo()
		{
			var info = new ZDropEditColumnStyleInfo();
			info.BindToList = "Collection";
			return info;
		}
#if !WINZOR
		public void TestHorizontalScrollBar()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("CD1", "This is a very long description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   .");
			var collection = new DummyBusinessObjectCollection(new BusinessObjectFactory());
			collection.AddNew();

			using (var form = new ZForm(collection))
			{
				using (var grid = new ZGrid())
				{
					grid.SetBindingMember(".");
					var info1 = new TestZDropEditColumnStyleInfo(DummyBizoSchema.Constants.Z0_VarCharMax, 100);
					grid.ColumnStyles.Add(info1);
					var info2 = new TestZDropEditColumnStyleInfo(DummyBizoSchema.Constants.Z0_NVarCharMax, 100);
					info2.ShowHorizontalScrollBar = true;
					grid.ColumnStyles.Add(info2);
					grid.Dock = DockStyle.Fill;
					form.Controls.Add(grid);
					form.Show();
					Application.DoEvents();
					var style1 = (TestZDropEditColumnStyle)grid.Columns[0].ColumnStyle;
					var dropEdit1 = style1.DropEdit;
					dropEdit1.List = list;
					using (var dropForm1 = new MockZDropForm(dropEdit1))
					{
						AssertNull("HorizontalScrollBar", dropForm1.HorizontalScrollBar_Exposed());
					}

					var style2 = (TestZDropEditColumnStyle)grid.Columns[1].ColumnStyle;
					var dropEdit2 = style2.DropEdit;
					dropEdit2.List = list;
					using (var dropForm2 = new MockZDropForm(dropEdit2))
					{
						AssertNotNull("HorizontalScrollBar", dropForm2.HorizontalScrollBar_Exposed());
					}
				}
			}
		}
#endif
		class TestZDropEditColumnStyleInfo : ZDropEditColumnStyleInfo
		{
			public TestZDropEditColumnStyleInfo(string columnName, int width)
				: base(columnName, width)
			{
			}

			public override Type ColumnStyleType
			{
				get { return typeof(TestZDropEditColumnStyle); }
			}
		}

		class TestZDropEditColumnStyle : ZDropEditColumnStyle
		{
			public TestZDropEditColumnStyle(ZDropEditColumnStyleInfo columnInfo)
				: base(columnInfo)
			{
			}

			public TestZDropEditColumnStyle(ZDropEditColumnStyleInfo columnInfo, Func<Control> editControl)
				: base(columnInfo, editControl)
			{
			}

			public void SetTestValue(string value)
			{
				GridControl.Text = value;
				IsEditing = false;
			}
		}
	}
}

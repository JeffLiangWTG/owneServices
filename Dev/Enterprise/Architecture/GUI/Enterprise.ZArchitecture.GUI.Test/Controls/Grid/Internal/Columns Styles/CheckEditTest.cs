using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture
{
	sealed class CheckEditTest : NotificationInGridTestCase
	{
		#region TestBGColourEmpty

#if !WINZOR

		public void TestBGColourEmpty()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
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

					using (var col = new ZCheckBoxColumnStyleForTest(info2))
					{
						col.SetDataGridExposed(grid);
						col.PaintExposed(grid.CreateGraphics(), new Rectangle(0, 0, 2000, 2000), grid.ListManager, 2, grid.ReadOnlyBrushFromRowNum(0), grid.ReadOnlyBrushFromRowNum(0), false);
						AssertEquals(CheckBoxImageType.UnChecked, col.imageType);
					}
				}
			}
		}

		class ZCheckBoxColumnStyleForTest : ZCheckBoxColumnStyle
		{
			public ZCheckBoxColumnStyleForTest(ZCheckBoxColumnStyleInfo info)
				: base(info)
			{
			}

			protected override void DrawCheckBox(Graphics g, Rectangle bounds, CheckBoxImageType imageType, Color customRowBackgroundColour)
			{
				this.imageType = imageType;
				base.DrawCheckBox(g, bounds, imageType, customRowBackgroundColour);
			}

			public CheckBoxImageType imageType;
		}

#endif

		#endregion

		public void TestShouldNotEnableTextBox()
		{
			var info = new ZCheckBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Bool };
			using (var style = new ZCheckBoxColumnStyle(info))
			using (_ = style.EditControl)
			{
				AssertEquals("TextBox Enabled should be false", false, style.TextBox.Enabled);
			}
		}

		public void TestShouldColumnHandleRightLeftKeyIsFalse()
		{
			form.Show();
			var nav = (INavigatingGridColumn)form.Grid.TableStyles[0].GridColumnStyles[0];
			Assert("Should navigate instead", !nav.ShouldColumnHandleKey(Keys.Right));
			Assert("Should navigate instead", !nav.ShouldColumnHandleKey(Keys.Left));
		}

#if !WINZOR

		public void TestPadRightIfRightAligned()
		{
			form.Show();
			var style = (ZCheckBoxColumnStyle)form.Grid.TableStyles[0].GridColumnStyles[0];

			using (style.UsePadRightForTesting())
			{
				style.Alignment = HorizontalAlignment.Right;
				Assert(style.HeaderText.EndsWith("" + (char)32 + (char)31));

				style.Alignment = HorizontalAlignment.Center;
				Assert(!style.HeaderText.EndsWith("" + (char)32 + (char)31));
			}
		}

		[ExpectNoExceptions]
		public void TestDisposedXPCheckBox()
		{
			form.Show();
			var column = (ZCheckBoxColumnStyle)form.Grid.TableStyles[0].GridColumnStyles[0];
			column.EditControl.Dispose();
			column.EditControl.Invalidate();
			column.checkBoxForDrawing.Dispose();
			column.checkBoxForDrawing.Invalidate();
			Application.DoEvents();
			form.Invalidate();
			Application.DoEvents();
		}

		[ExpectNoExceptions]
		public void TestXPCheckBoxWithInvalidThemeHandle()
		{
			form.Show();
			var column = (ZCheckBoxColumnStyle)form.Grid.TableStyles[0].GridColumnStyles[0];
			column.checkBoxForDrawing.hTheme = new IntPtr(0x123546);
		}

#endif

		[ExpectNoExceptions]
		public void TestGetPreferredSize_NullRef()
		{
			form.Show();
			var column = (ZCheckBoxColumnStyle)form.Grid.TableStyles[0].GridColumnStyles[0];
			var size = column.GetPreferredSizeExposed(form.Grid.CreateGraphics(), null);
			AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(40), size.Width);
		}

		public void TestMinimumWidth()
		{
			form.Show();
			var column = (ZCheckBoxColumnStyle)form.Grid.TableStyles[0].GridColumnStyles[0];
			column.Width = 100;
			AssertEquals(40, column.GetPreferredSizeExposed(form.Grid.CreateGraphics(), ZBool.True).Width);
			AssertEquals(100, column.Width);
			column.Width = 40;
			AssertEquals(40, column.Width);
			column.Width = 39;
			AssertEquals(40, column.Width);
			column.Width = 0;
			AssertEquals(40, column.Width);
			AssertEquals(40, column.GetPreferredSizeExposed(form.Grid.CreateGraphics(), ZBool.True).Width);
		}

		public override void TestLocationAndSizeWithAndWithoutNotifications()
		{
			CheckSizeAndLocation(0, new Point(47, 19), new Size(27, 16));
			CheckSizeAndLocation(10, new Point(47, 19), new Size(27, 16));
			CheckSizeAndLocation(100, new Point(47, 19), new Size(87, 16));

			Dummy.Collection[0].Z0_BoolInfo.AddError("baad");

			CheckSizeAndLocation(0, new Point(47, 19), new Size(27, 16));
			CheckSizeAndLocation(10, new Point(47, 19), new Size(27, 16));
			CheckSizeAndLocation(100, new Point(47, 19), new Size(87, 16));
		}

		protected override string GetColumnName()
		{
			return AutoDummyBizo.Schema.Z0_Bool;
		}

		protected override ZGridColumnInfo GetInfo()
		{
			return new ZCheckBoxColumnStyleInfo();
		}
	}
}

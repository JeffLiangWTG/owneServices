using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	sealed class LayoutGroupBoxTest : TestCase
	{
		public void TestConstructor()
		{
			using (AutoLayoutGroupBox testGroupBox = new AutoLayoutGroupBox())
			{
				AssertEquals(1, testGroupBox.NextHeights.Count);
				AssertEquals(AutoLayoutGroupBox.TopMargin, testGroupBox.NextHeights[0]);
			}
		}

		public void TestAddControl()
		{
			using (AutoLayoutGroupBox testGroupBox = new AutoLayoutGroupBox())
			{
				AssertEquals(AutoLayoutGroupBox.RightMargin, testGroupBox.DesiredWidth);

				TextBox txtBx = new TextBox();
				testGroupBox.AddControl(txtBx);

				AssertEquals(testGroupBox, txtBx.Parent);
				AssertEquals(AutoLayoutGroupBox.TopMargin, txtBx.Top);
				AssertEquals(AutoLayoutGroupBox.LeftMargin, txtBx.Left);
				AssertEquals(AutoLayoutGroupBox.LeftMargin + txtBx.Width + AutoLayoutGroupBox.RightMargin, testGroupBox.DesiredWidth);

				txtBx.Left = 120;

				AssertEquals(120 + txtBx.Width + AutoLayoutGroupBox.RightMargin, testGroupBox.DesiredWidth);
			}
		}

		public void TestAddControl_HeightIsLatest()
		{
			using (var testGroupBox = new AutoLayoutGroupBox())
			{
				var optionGroupUserControl = new OptionGroupUserControl();
				optionGroupUserControl.Layout += (obj, e) =>
				{
					ControlDpiScalingHelper.SetHeight(optionGroupUserControl, 100, false);
				};

				testGroupBox.AddControl(optionGroupUserControl);

				AssertEquals(AutoLayoutGroupBox.TopMargin + 100, testGroupBox.NextHeights[0]);
			}
		}

		public void TestSetHeight()
		{
			using (AutoLayoutGroupBox testGroupBox = new AutoLayoutGroupBox())
			{
				testGroupBox.SetHeight();
				AssertEquals(AutoLayoutGroupBox.TopMargin + AutoLayoutGroupBox.BottomMargin, testGroupBox.Height);

				TextBox txtBx = new TextBox();
				testGroupBox.AddControl(txtBx);

				testGroupBox.SetHeight();

				AssertEquals(AutoLayoutGroupBox.TopMargin + txtBx.Height + AutoLayoutGroupBox.BottomMargin, testGroupBox.Height);
			}
		}

		public void TestForceHeight()
		{
			using (AutoLayoutGroupBox testGroupBox = new AutoLayoutGroupBox())
			{
				testGroupBox.ForceHeight(500);
				AssertEquals(500, testGroupBox.Height);
			}
		}

		public void TestRearrangeInNColumns()
		{
			using (AutoLayoutGroupBox testGroupBox = new AutoLayoutGroupBox())
			{
				for (int i = 0; i < 10; i++)
				{
					testGroupBox.AddControl(new TextBox());
				}

				testGroupBox.SetHeight();
				AssertEquals(AutoLayoutGroupBox.TopMargin + new TextBox().Height * 10 + AutoLayoutGroupBox.BottomMargin, testGroupBox.Height);
				AssertEquals(AutoLayoutGroupBox.LeftMargin + new TextBox().Width + AutoLayoutGroupBox.RightMargin, testGroupBox.DesiredWidth);

				testGroupBox.RearrangeInNColumns(2);
				AssertEquals(AutoLayoutGroupBox.TopMargin + new TextBox().Height * 5 + AutoLayoutGroupBox.BottomMargin, testGroupBox.Height);
				AssertEquals(AutoLayoutGroupBox.LeftMargin + new TextBox().Width + testGroupBox.ColumnWidth + AutoLayoutGroupBox.RightMargin, testGroupBox.DesiredWidth);
			}
		}

		public void TestRearrangeWithDifferentControls()
		{
			using (var testGroupBox = new AutoLayoutGroupBox())
			{
				var child = new DateRangeFieldUserControl();
				testGroupBox.AddControl(child);
				testGroupBox.SetHeight();
				testGroupBox.RearrangeInNColumns(2);
				AssertLessThan("Unexpected Height of 2 columns", child.Left, testGroupBox.DesiredWidth / 2);
			}

			using (var testGroupBox = new AutoLayoutGroupBox())
			{
				for (int i = 0; i < 2; i++)
				{
					testGroupBox.AddControl(new DateRangeFieldUserControl());
				}
				testGroupBox.SetHeight();
				testGroupBox.RearrangeInNColumns(2);
				AssertLessThan("Unexpected Height of 2 columns", testGroupBox.Controls[0].Left, testGroupBox.DesiredWidth / 2);
				AssertGreaterThanOrEqualTo("Unexpected Height of 2 columns", testGroupBox.Controls[1].Left, testGroupBox.DesiredWidth / 2);
			}

			using (var testGroupBox = new AutoLayoutGroupBox())
			{
				for (int i = 0; i < 3; i++)
				{
					testGroupBox.AddControl(new DateRangeFieldUserControl());
				}
				testGroupBox.SetHeight();
				testGroupBox.RearrangeInNColumns(2);
				AssertLessThan("Unexpected Height of 2 columns", testGroupBox.Controls[0].Left, testGroupBox.DesiredWidth / 2);
				AssertLessThan("Unexpected Height of 2 columns", testGroupBox.Controls[1].Left, testGroupBox.DesiredWidth / 2);
				AssertGreaterThanOrEqualTo("Unexpected Height of 2 columns", testGroupBox.Controls[2].Left, testGroupBox.DesiredWidth / 2);
			}

			using (var testGroupBox = new AutoLayoutGroupBox())
			{
				var child0 = new OptionGroupUserControl() { Height = 100 };
				var child1 = new OptionGroupUserControl() { Height = 200 };
				testGroupBox.AddControl(child0);
				testGroupBox.AddControl(child1);
				testGroupBox.SetHeight();
				testGroupBox.RearrangeInNColumns(2);
				AssertLessThan("Unexpected Height of 2 columns", testGroupBox.Controls[0].Left, testGroupBox.DesiredWidth / 2);
				AssertGreaterThanOrEqualTo("Unexpected Height of 2 columns", testGroupBox.Controls[1].Left, testGroupBox.DesiredWidth / 2);
			}

			using (var testGroupBox = new AutoLayoutGroupBox())
			{
				var child0 = new OptionGroupUserControl() { Height = 100 };
				var child1 = new OptionGroupUserControl() { Height = 300 };
				var child2 = new OptionGroupUserControl() { Height = 200 };
				testGroupBox.AddControl(child0);
				testGroupBox.AddControl(child1);
				testGroupBox.AddControl(child2);
				testGroupBox.SetHeight();
				testGroupBox.RearrangeInNColumns(2);
				AssertLessThan("Unexpected Height of 2 columns", testGroupBox.Controls[0].Left, testGroupBox.DesiredWidth / 2);
				AssertLessThan("Unexpected Height of 2 columns", testGroupBox.Controls[1].Left, testGroupBox.DesiredWidth / 2);
				AssertGreaterThanOrEqualTo("Unexpected Height of 2 columns", testGroupBox.Controls[2].Left, testGroupBox.DesiredWidth / 2);
			}

			using (var testGroupBox = new AutoLayoutGroupBox())
			{
				var child0 = new OptionGroupUserControl() { Height = 300 };
				var child1 = new OptionGroupUserControl() { Height = 200 };
				var child2 = new OptionGroupUserControl() { Height = 100 };
				testGroupBox.AddControl(child0);
				testGroupBox.AddControl(child1);
				testGroupBox.AddControl(child2);
				testGroupBox.SetHeight();
				testGroupBox.RearrangeInNColumns(2);
				AssertLessThan("Unexpected Height of 2 columns", testGroupBox.Controls[0].Left, testGroupBox.DesiredWidth / 2);
				AssertGreaterThanOrEqualTo("Unexpected Height of 2 columns", testGroupBox.Controls[1].Left, testGroupBox.DesiredWidth / 2);
				AssertGreaterThanOrEqualTo("Unexpected Height of 2 columns", testGroupBox.Controls[2].Left, testGroupBox.DesiredWidth / 2);
			}
		}

		public void TestRearrangeInNColumnsWithLargerControls()
		{
			using (var testGroupBox = new AutoLayoutGroupBox())
			{
				for (int i = 0; i < 10; i++)
				{
					testGroupBox.AddControl(new DateRangeFieldUserControl());
				}

				var refControl = testGroupBox.Controls[0];

				testGroupBox.SetHeight();
				AssertEquals(AutoLayoutGroupBox.TopMargin + refControl.Height * 10 + AutoLayoutGroupBox.BottomMargin, testGroupBox.Height);
				AssertEquals(AutoLayoutGroupBox.LeftMargin + refControl.Width + AutoLayoutGroupBox.RightMargin, testGroupBox.DesiredWidth);

				testGroupBox.RearrangeInNColumns(2);
				AssertEquals("Unexpected Height of 2 columns", AutoLayoutGroupBox.TopMargin + refControl.Height * 5 + AutoLayoutGroupBox.BottomMargin, testGroupBox.Height);
				AssertEquals("Unexpected Width of 2 columns", AutoLayoutGroupBox.LeftMargin + refControl.Width * 2 + AutoLayoutGroupBox.RightMargin, testGroupBox.DesiredWidth);
			}
		}

		public void TestRearrange2DateRangeFieldUserControlWithoutOverlap()
		{
			using (var testGroupBox = new AutoLayoutGroupBox())
			{
				var c1 = new DateRangeFieldUserControl();
				testGroupBox.AddControl(c1);
				var c2 = new DateRangeFieldUserControl();
				testGroupBox.AddControl(c2);

				testGroupBox.SetHeight();
				testGroupBox.RearrangeInNColumns(2);
				Assert("Controls not align", c1.Top == c2.Top);
				Assert("Controls overlap", c1.Right <= c2.Left);
				AssertEquals("Unexpected Width", AutoLayoutGroupBox.LeftMargin + c1.Width + c2.Width + AutoLayoutGroupBox.RightMargin, testGroupBox.DesiredWidth);
				AssertEquals("Unexpected ColumnWidth", c1.Width, testGroupBox.ColumnWidth);
			}
		}

		public void TestRearrange2MixControlsWithoutOverlap()
		{
			using (var testGroupBox = new AutoLayoutGroupBox())
			{
				var c1 = new TextBox();
				c1.Multiline = true;
				c1.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(500);// Need to increase the height of TextBox to avoid 2 control squeze into one column					
				testGroupBox.AddControl(c1);
				var c2 = new DateRangeFieldUserControl();
				testGroupBox.AddControl(c2);

				testGroupBox.ForceHeight(60);
				testGroupBox.RearrangeInNColumns(2);
				Assert("Controls not align", c1.Top == c2.Top);
				Assert("Controls overlap", c1.Right <= c2.Left);
				AssertEquals("Unexpected Width", AutoLayoutGroupBox.LeftMargin + c2.Width * 2 + AutoLayoutGroupBox.RightMargin, testGroupBox.DesiredWidth);
				AssertEquals("Unexpected ColumnWidth", Math.Max(c1.Width, c2.Width), testGroupBox.ColumnWidth);
			}
		}

		public void TestDesiredWidth()
		{
			using (AutoLayoutGroupBox testGroupBox = new AutoLayoutGroupBox())
			{
				for (int i = 0; i < 10; i++)
				{
					testGroupBox.AddControl(new TextBox());
				}

				AssertEquals(AutoLayoutGroupBox.LeftMargin + new TextBox().Width + AutoLayoutGroupBox.RightMargin, testGroupBox.DesiredWidth);

				testGroupBox.RearrangeInNColumns(2);
				AssertEquals(AutoLayoutGroupBox.LeftMargin + new TextBox().Width + testGroupBox.ColumnWidth + AutoLayoutGroupBox.RightMargin, testGroupBox.DesiredWidth);
			}
		}

		public void TestAlignControls()
		{
			var testText = "I have no idea what I'm doing";
			using (var box = new AutoLayoutGroupBox())
			{
				AddControl<DateRangeFieldUserControl>(box, caption: testText);
				AddControl<DateRangeFieldUserControl>(box);
				AddControl<DateRangeFieldUserControl>(box);
				AddControl<DateRangeFieldUserControl>(box, caption: testText);
				AddControl<LookupFieldUserControl>(box, caption: "Shorter caption");

				box.AlignControls();

				int expectedStartOfControl = TextRenderer.MeasureText(testText, box.Controls[0].Controls.OfType<ZLabel>().Single().GetExtension<ILabelCaptionRenderer>().Font).Width + AutoLayoutGroupBox.RightMargin;
				foreach (var control in box.Controls.Cast<Control>())
				{
					var label = control.Controls.OfType<ZLabel>().SingleOrDefault();
					if (label != null)
					{
						AssertEquals("control does not follow expectation", expectedStartOfControl, label.Right);
					}

					AssertEquals("Control should start at " + expectedStartOfControl, expectedStartOfControl, control.Controls.Cast<Control>().Except(new[] { label }).Single().Left);
				}
			}
		}

		void AddControl<T>(AutoLayoutGroupBox box, string caption = null) where T : RuntimeOptionUserControl, new()
		{
			var ctrl = new T();
			if (!string.IsNullOrEmpty(caption))
			{
				var labelControl = (ctrl.Controls.OfType<ZLabel>().SingleOrDefault() ?? ctrl.Controls[0]);
				labelControl.GetExtension<ILabelCaptionRenderer>().Caption = caption;
			}

			box.AddControl(ctrl);
		}
	}
}

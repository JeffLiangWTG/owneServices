using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Moq;
using NUnit.Framework;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public sealed class DynamicLayoutPanelTest : TestCaseWithFactory
	{
		public void TestDataSourceTypeIsSet()
		{
			using (var panel = new DynamicLayoutPanel())
			{
				var bo = Factory.New<DummyBusinessObject>();
				panel.SetDataBinding(bo, null);
				AssertEquals("DataSourceType", typeof(DummyBusinessObject), panel.DataSourceType);
				panel.SetDataBinding(bo, "Collection");
				AssertEquals("DataSourceType", typeof(DummyChildBusinessObject), panel.DataSourceType);
			}
		}

		public void TestAutoScroll_CalculateAutoScrollMinSize()
		{
			using (var panel = new DynamicLayoutPanel())
			{
				panel.AutoScroll = true;
				AssertEquals("Pre-Condition: AutoScrollMinSize should be zero.", new Size(0, 0), panel.AutoScrollMinSize);
				panel.UpdateLayout(CreateAllIncludedLayout());
				AssertEquals("AutoScrollMinSize should be calculated if AutoScroll = true.", new Size(80, 72), panel.AutoScrollMinSize);
			}
		}

		public void TestAutoScroll_TrimControls()
		{
			using (var panel = new DynamicLayoutPanel())
			{
				panel.AutoScroll = false;
				panel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

				var common = ControlBagForTest2.Instance;
				var layout = new PanelLayout();
				var captionRuler = layout.CreateRuler(50);
				var widthRuler = layout.CreateRightRuler(150);

				layout.RegisterControlBag(common);
				layout.Include(0, captionRuler, common.TextBox1, widthRuler);
				layout.Include(0, captionRuler, common.TextBox2, widthRuler);

				panel.UpdateLayout(layout);
				var controls = panel.Controls.Cast<Control>();
				AssertEquals("Controls should be trimmed: DynamicLayoutPanel.Width(80) - CaptionWidth(50) = ControlWidth(30)", ControlDpiScalingHelper.ScaleToCurrentDpiX(30), controls.First().Width);
			}
		}

		public void TestAutoScroll_TrimControls_ErrorReport()
		{
			using (var userControl = new ZUserControl())
			using (var panel = new DynamicLayoutPanel())
			{
				userControl.Name = "BOBUserControl";
				userControl.Controls.Add(panel);
				panel.Name = "PanelName";
				panel.AutoScroll = false;
				var scaledDpiX30 = ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
				panel.Width = scaledDpiX30;

				var common = ControlBagForTest2.Instance;
				var layout = new PanelLayout();
				var captionRuler = layout.CreateRuler(50);
				var scaledDpiX50 = ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
				var widthRuler = layout.CreateRightRuler(150);

				layout.RegisterControlBag(common);
				layout.Include(0, captionRuler, common.TextBox1, widthRuler);
				layout.Include(0, captionRuler, common.TextBox2, widthRuler);

				panel.UpdateLayout(layout);

				AssertEquals($@"The new width({scaledDpiX30 - scaledDpiX50}) of TextBox2 can't be negative.
Original width: 100 Parent:PanelName width:{scaledDpiX30}
(columnX:0 rulerPosition:150 control.Left:{scaledDpiX50} AutoScroll:False DisplayRectangle.Width:{scaledDpiX30})
This can be caused when form is not big enough to show the control fully. Either set the PanelName.AutoScroll in control BOBUserControl to true or ensure that the minimum form size can show the layout.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestIncludeExclude()
		{
			using (var panel = new DynamicLayoutPanel())
			{
				panel.UpdateLayout(CreateAllIncludedLayout());
				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");

				panel.UpdateLayout(CreateAllExcludedLayout());
				AssertControlsOrder(panel, Array.Empty<string>());

				panel.UpdateLayout(CreateOneExcludedLayout());
				AssertControlsOrder(panel, "Button1", "Button3", "Button4");

				panel.UpdateLayout(CreateReverseOrderLayout());
				AssertControlsOrder(panel, "Button4", "Button3", "Button2", "Button1");

				panel.UpdateLayout(CreateAllIncludedLayout());
				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");
			}
		}

		public void TestMultipleControlsPerRow()
		{
			var common = ControlBagForTest.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(common);

			var ruler = layout.CreateRuler(100);
			layout.Include(common.Button1, ruler, common.Button2);
			layout.Include(common.Button3, common.Button4);

			using (var panel = new DynamicLayoutPanel())
			{
				panel.UpdateLayout(layout);
				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");

				var button1 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button1), 1);
				var button2 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button2), 1);
				var button3 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button3), 1);
				var button4 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button4), 1);

				AssertEquals("Button1 and Button2 are on the same row", button1.Top, button2.Top);
				AssertEquals("Button3 and Button4 are on the same row", button3.Top, button4.Top);
				AssertGreaterThan("Button3 should be on the row that follows Button1", button3.Top, button1.Bottom);

				AssertGreaterThan("Button2 should be after Button1", button2.Left, button1.Right);
				AssertGreaterThan("Button4 should be after Button3", button4.Left, button3.Right);
				AssertEquals("Button2 should be aligned with ruler", ControlDpiScalingHelper.ScaleToCurrentDpiX(100), button2.Left);
			}
		}

		public void TestCombinedControlLayout()
		{
			var bag1 = ControlBagForTest.Instance;
			var bag2 = ControlBagForTest2.Instance;
			var layout = new PanelLayout();

			layout.RegisterControlBag(bag1);
			layout.RegisterControlBag(bag2);

			layout.Include(bag1.Button1);
			layout.Include(bag1.Button2);
			layout.Include(bag1.Button3);
			layout.Include(bag1.Button4);
			layout.Include(bag2.TextBox1);

			var bo = Factory.New<DummyBusinessObject>();
			bo.Z0_NVarChar = "ABC";

			using (var form = new ZForm(bo))
			using (var panel = new DynamicLayoutPanel())
			{
				form.Controls.Add(panel);

				panel.UpdateLayout(layout);
				form.Show();

				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4", "TextBox1");

				var textBox1 = panel.FindSingle<ZTextBox>(c => c.Name == nameof(ControlBagForTest2.TextBox1), 1);
				AssertEquals("ABC", textBox1.Text);
			}
		}

		public void TestVisibility()
		{
			using (var panel = new DynamicLayoutPanel())
			{
				var bo = Factory.New<DummyBusinessObject>();
				panel.SetDataBinding(bo, null);

				panel.UpdateLayout(CreateLayoutWithVisibility());
				AssertControlsOrder(panel, "Button2");

				bo.Z0_NVarChar = "A";
				AssertControlsOrder(panel, "Button1", "Button2");

				bo.Z0_NVarChar = "B";
				AssertControlsOrder(panel, "Button2");

				// checking that previous layout does not affect new layout

				panel.UpdateLayout(CreateAllIncludedLayout());
				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");

				bo.Z0_NVarChar = "A";
				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");

				bo.Z0_NVarChar = "B";
				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");

				panel.UpdateLayout(CreateLayoutWithVisibility());
				AssertControlsOrder(panel, "Button2");

				// checking that change of the bound item updates layout correctly

				var bo2 = Factory.New<DummyBusinessObject>();
				bo2.Z0_NVarChar = "A";
				panel.SetDataBinding(bo2, null);
				AssertControlsOrder(panel, "Button1", "Button2");

				bo2.Z0_NVarChar = "B";
				AssertControlsOrder(panel, "Button2");

				// checking that changes to the previously bound object do not affect layout

				bo.Z0_NVarChar = "A";
				AssertControlsOrder(panel, "Button2");

				bo.Z0_NVarChar = "B";
				AssertControlsOrder(panel, "Button2");
			}
		}

		public void TestVisibility_EventHandlersAfterDispose()
		{
			var bo = Factory.New<DummyBusinessObject>();

			using (var panel = new DynamicLayoutPanel())
			{
				panel.SetDataBinding(bo, null);

				panel.UpdateLayout(CreateLayoutWithVisibility());
				AssertControlsOrder(panel, "Button2");

				bo.Z0_NVarChar = "A";
				AssertControlsOrder(panel, "Button1", "Button2");

				bo.Z0_NVarChar = "B";
				AssertControlsOrder(panel, "Button2");
			}

			// changing business object should not cause exceptions
			bo.Z0_NVarChar = "A";
		}

		public void TestCaption()
		{
			using (var panel = new DynamicLayoutPanel())
			{
				var bo = Factory.New<DummyBusinessObject>();
				panel.SetDataBinding(bo, null);

				panel.UpdateLayout(CreateLayoutWithCaption(true));
				var textBox1 = panel.FindSingle<ZTextBox>(c => c.Name == nameof(ControlBagForTest2.TextBox1), 1);
				var textBox2 = panel.FindSingle<ZTextBox>(c => c.Name == nameof(ControlBagForTest2.TextBox2), 1);

				AssertEquals("", textBox1.CaptionResourceString.Caption);
				AssertEquals("Text Box 2", textBox2.CaptionResourceString.Caption);

				bo.Z0_NVarChar = "Test Caption (A)";
				AssertEquals("Test Caption (A)", textBox1.CaptionResourceString.Caption);
				AssertEquals("Text Box 2", textBox2.CaptionResourceString.Caption);

				bo.Z0_NVarChar = "Test Caption (B)";
				AssertEquals("Test Caption (B)", textBox1.CaptionResourceString.Caption);
				AssertEquals("Text Box 2", textBox2.CaptionResourceString.Caption);

				// checking that previous layout does not affect new layout

				panel.UpdateLayout(CreateLayoutWithCaption(false));
				AssertEquals("Text Box 1", textBox1.CaptionResourceString.Caption);
				AssertEquals("Text Box 2", textBox2.CaptionResourceString.Caption);

				bo.Z0_NVarChar = "Test Caption (C)";
				AssertEquals("Text Box 1", textBox1.CaptionResourceString.Caption);
				AssertEquals("Text Box 2", textBox2.CaptionResourceString.Caption);

				panel.UpdateLayout(CreateLayoutWithCaption(true));
				AssertEquals("Test Caption (C)", textBox1.CaptionResourceString.Caption);
				AssertEquals("Text Box 2", textBox2.CaptionResourceString.Caption);

				// checking that change of the bound item updates layout correctly

				var bo2 = Factory.New<DummyBusinessObject>();
				bo2.Z0_NVarChar = "Test Caption (D)";
				panel.SetDataBinding(bo2, null);
				AssertEquals("Test Caption (D)", textBox1.CaptionResourceString.Caption);
				AssertEquals("Text Box 2", textBox2.CaptionResourceString.Caption);

				// checking that changes to the previously bound object do not affect layout
				bo.Z0_NVarChar = "Test Caption (E)";
				panel.SetDataBinding(bo2, null);
				AssertEquals("Test Caption (D)", textBox1.CaptionResourceString.Caption);
				AssertEquals("Text Box 2", textBox2.CaptionResourceString.Caption);
			}
		}

		public void TestUpdateCaption()
		{
			using (var panel = new DynamicLayoutPanel())
			{
				var bo = Factory.New<DummyBusinessObject>();
				panel.SetDataBinding(bo, null);

				panel.UpdateLayout(CreateLayoutWithCaption(true));
				var textboxcaption = panel.FindSingle<ZTextBox>(c => c.Name == nameof(ControlBagForTest2.TextBox1), 1).GetExtension<IHintExtension>();

				bo.Z0_NVarChar = "Test Caption(0)";
				AssertEquals("Caption of LloydsNumberTextBox's Balloon for the SEA and DEMITH", "Test Caption(0)", textboxcaption.Caption);

				bo.Z0_NVarChar = "Test Caption(1)";
				AssertEquals("Caption of LloydsNumberTextBox's Balloon for AIR and HAVITH", "Test Caption(1)", textboxcaption.Caption);
			}
		}

		public void TestComplexControlCaptions()
		{
			using (var panel = new DynamicLayoutPanel())
			{
				var bo = Factory.New<DummyBusinessObject>();
				panel.SetDataBinding(bo, null);

				panel.UpdateLayout(CreateLayoutWithComplexControlCaptions(true));
				var complexControl = panel.FindSingle<ComplexUserControlForTesting>(c => c.Name == nameof(ControlBagForTestComplexControls.ComplexControl), 1);
				var textBox1 = complexControl.FindSingle<ZTextBox>(c => c.Name == ComplexUserControlForTesting.ControlNames.TextBox1, 1);
				var textBox2 = complexControl.FindSingle<ZTextBox>(c => c.Name == ComplexUserControlForTesting.ControlNames.TextBox2, 1);
				var textBox3 = complexControl.FindSingle<ZTextBox>(c => c.Name == ComplexUserControlForTesting.ControlNames.TextBox3, 1);

				var complexControl2 = panel.FindSingle<ComplexUserControlForTesting>(c => c.Name == nameof(ControlBagForTestComplexControls.ComplexControl2), 1);
				var complexControl2TextBox1 = complexControl2.FindSingle<ZTextBox>(c => c.Name == ComplexUserControlForTesting.ControlNames.TextBox1, 1);
				var complexControl2TextBox2 = complexControl2.FindSingle<ZTextBox>(c => c.Name == ComplexUserControlForTesting.ControlNames.TextBox2, 1);
				var complexControl2TextBox3 = complexControl2.FindSingle<ZTextBox>(c => c.Name == ComplexUserControlForTesting.ControlNames.TextBox3, 1);

				CombineAssertions("Default", () =>
				{
					AssertResourceStringData(complexControl2, string.Empty, string.Empty, "Default", string.Empty);
					AssertResourceStringData(complexControl2TextBox1, string.Empty, string.Empty, "Control Text Box 1", string.Empty);
					AssertResourceStringData(complexControl2TextBox2, string.Empty, string.Empty, "Control Text Box 2", string.Empty);
					AssertResourceStringData(complexControl2TextBox3, "Text Box 3", "Cntrl Text Box 3", "Control Text Box 3", "Full Control Text Box 3");
					AssertResourceStringData(complexControl, string.Empty, string.Empty, "Default", string.Empty);
					AssertResourceStringData(textBox1, string.Empty, string.Empty, string.Empty, string.Empty);
					AssertResourceStringData(textBox2, string.Empty, string.Empty, "Control Text Box 2", string.Empty);
					AssertResourceStringData(textBox3, string.Empty, string.Empty, string.Empty, string.Empty);
				});

				CombineAssertions("First Z0_VarCharMax change", () =>
				{
					bo.Z0_VarCharMax = "Test Short (A)";
					AssertResourceStringData(textBox1, "Test Short (A)", string.Empty, string.Empty, string.Empty);
				});

				CombineAssertions("Second Z0_VarCharMax change", () =>
				{
					bo.Z0_VarCharMax = "Test Short (B)";
					AssertResourceStringData(textBox1, "Test Short (B)", string.Empty, string.Empty, string.Empty);
				});

				CombineAssertions("First Z0_NVarCharMax change", () =>
				{
					bo.Z0_NVarCharMax = "Test Full (C)";
					AssertResourceStringData(textBox1, "Test Short (B)", string.Empty, string.Empty, "Test Full (C)");
				});

				CombineAssertions("First Z0_SparseNVarChar change", () =>
				{
					bo.Z0_SparseNVarChar = "Test 3 Caption (A)";
					AssertResourceStringData(textBox1, "Test Short (B)", string.Empty, string.Empty, "Test Full (C)");
					AssertResourceStringData(textBox3, string.Empty, string.Empty, "Test 3 Caption (A)", string.Empty);
				});

				CombineAssertions("First Z0_Description change", () =>
				{
					bo.Z0_Description = "Test Main Caption (A)";
					AssertResourceStringData(complexControl2, string.Empty, string.Empty, "Test Main Caption (A)", string.Empty);
					AssertResourceStringData(complexControl, string.Empty, string.Empty, "Test Main Caption (A)", string.Empty);
					AssertResourceStringData(textBox1, "Test Short (B)", string.Empty, string.Empty, "Test Full (C)");
					AssertResourceStringData(textBox3, string.Empty, string.Empty, "Test 3 Caption (A)", string.Empty);
				});

				CombineAssertions("checking that previous layout does not affect new layout", () =>
				{
					panel.UpdateLayout(CreateLayoutWithComplexControlCaptions(false));
					AssertResourceStringData(complexControl2, string.Empty, string.Empty, "HELLO WORLD", string.Empty);
					AssertResourceStringData(complexControl2TextBox1, string.Empty, string.Empty, "Control Text Box 1", string.Empty);
					AssertResourceStringData(complexControl2TextBox2, string.Empty, string.Empty, "Control Text Box 2", string.Empty);
					AssertResourceStringData(complexControl2TextBox3, "Text Box 3", "Cntrl Text Box 3", "Control Text Box 3", "Full Control Text Box 3");
					AssertResourceStringData(complexControl, string.Empty, string.Empty, "Control Text Main", string.Empty);
					AssertResourceStringData(textBox1, string.Empty, string.Empty, "Control Text Box 1", string.Empty);
					AssertResourceStringData(textBox2, string.Empty, string.Empty, "Control Text Box 2", string.Empty);
					AssertResourceStringData(textBox3, "Text Box 3", "Cntrl Text Box 3", "Control Text Box 3", "Full Control Text Box 3");
				});

				CombineAssertions("Checking that previous layout does not affect new layout - change Z0_VarCharMax", () =>
				{
					bo.Z0_VarCharMax = "Test Short (D)";
					AssertResourceStringData(textBox1, string.Empty, string.Empty, "Control Text Box 1", string.Empty);
				});

				CombineAssertions("New layout with caption", () =>
				{
					panel.UpdateLayout(CreateLayoutWithComplexControlCaptions(true));
					AssertResourceStringData(complexControl2, string.Empty, string.Empty, "Test Main Caption (A)", string.Empty);
					AssertResourceStringData(complexControl2TextBox1, string.Empty, string.Empty, "Control Text Box 1", string.Empty);
					AssertResourceStringData(complexControl2TextBox2, string.Empty, string.Empty, "Control Text Box 2", string.Empty);
					AssertResourceStringData(complexControl2TextBox3, "Text Box 3", "Cntrl Text Box 3", "Control Text Box 3", "Full Control Text Box 3");
					AssertResourceStringData(complexControl, string.Empty, string.Empty, "Test Main Caption (A)", string.Empty);
					AssertResourceStringData(textBox1, "Test Short (D)", string.Empty, string.Empty, "Test Full (C)");
					AssertResourceStringData(textBox2, string.Empty, string.Empty, "Control Text Box 2", string.Empty);
					AssertResourceStringData(textBox3, string.Empty, string.Empty, "Test 3 Caption (A)", string.Empty);
				});

				CombineAssertions("Checking that change of the bound item updates layout correctly", () =>
				{
					var bo2 = Factory.New<DummyBusinessObject>();
					bo2.Z0_VarCharMax = "Test Caption (E)";
					bo2.Z0_SparseNVarChar = "Test 3 Caption (B)";
					bo2.Z0_Description = "Test Main Caption (B)";
					panel.SetDataBinding(bo2, null);
					AssertResourceStringData(complexControl2, string.Empty, string.Empty, "Test Main Caption (B)", string.Empty);
					AssertResourceStringData(complexControl, string.Empty, string.Empty, "Test Main Caption (B)", string.Empty);
					AssertResourceStringData(textBox1, "Test Caption (E)", string.Empty, string.Empty, string.Empty);
					AssertResourceStringData(textBox3, string.Empty, string.Empty, "Test 3 Caption (B)", string.Empty);
				});

				CombineAssertions("Checking that changes to the previously bound object do not affect layout", () =>
				{
					bo.Z0_VarCharMax = "Test Caption (F)";
					AssertResourceStringData(complexControl2, string.Empty, string.Empty, "Test Main Caption (B)", string.Empty);
					AssertResourceStringData(complexControl, string.Empty, string.Empty, "Test Main Caption (B)", string.Empty);
					AssertResourceStringData(textBox1, "Test Caption (E)", string.Empty, string.Empty, string.Empty);
					AssertResourceStringData(textBox3, string.Empty, string.Empty, "Test 3 Caption (B)", string.Empty);
				});
			}
		}

		public void TestUpdateComplexControlCaptions()
		{
			using (var panel = new DynamicLayoutPanel())
			{
				var bo = Factory.New<DummyBusinessObject>();
				panel.SetDataBinding(bo, null);

				panel.UpdateLayout(CreateLayoutWithComplexControlCaptions(true));
				var complexControl = panel.FindSingle<ComplexUserControlForTesting>(c => c.Name == nameof(ControlBagForTestComplexControls.ComplexControl), 1);
				var textbox = complexControl.FindSingle<ZTextBox>(c => c.Name == ComplexUserControlForTesting.ControlNames.TextBox1, 1);

				bo.Z0_VarCharMax = "Test Short(0)";
				bo.Z0_NVarCharMax = "Test Description";
				AssertResourceStringData(textbox, "Test Short(0)", string.Empty, string.Empty, "Test Description");

				bo.Z0_VarCharMax = "Test Short(1)";
				AssertResourceStringData(textbox, "Test Short(1)", string.Empty, string.Empty, "Test Description");

				bo.Z0_NVarCharMax = "Test Description2";
				AssertResourceStringData(textbox, "Test Short(1)", string.Empty, string.Empty, "Test Description2");

				bo.Z0_NVarChar = "Test Caption";
				AssertResourceStringData(textbox, "Test Short(1)", string.Empty, "Test Caption", "Test Description2");
			}
		}

		public void TestBindingWithRemovedControls()
		{
			var bag = ControlBagForTest2.Instance;

			var layout1 = new PanelLayout();
			var layout2 = new PanelLayout();

			layout1.RegisterControlBag(bag);
			layout2.RegisterControlBag(bag);

			layout1.Include(bag.TextBox1);
			layout1.Include(bag.TextBox2);

			layout2.Include(bag.TextBox2);

			var bo = Factory.New<DummyBusinessObject>();
			bo.Z0_NVarChar = "111";

			using (var form = new ZForm(bo))
			using (var panel = new DynamicLayoutPanel())
			{
				form.Controls.Add(panel);

				panel.UpdateLayout(layout1);
				form.Show();

				AssertControlsOrder(panel, "TextBox1", "TextBox2");
				var textBox1 = panel.FindSingle<TextBox>(c => c.Name == nameof(bag.TextBox1), 1);

				AssertEquals("111", textBox1.Text);

				bo.Z0_NVarChar = "222";
				AssertControlsOrder(panel, "TextBox1", "TextBox2");
				AssertEquals("222", textBox1.Text);
				AssertNotNull(textBox1.BindingContext);

				panel.UpdateLayout(layout2);
				AssertControlsOrder(panel, "TextBox2");

				bo.Z0_NVarChar = "333";
				AssertControlsOrder(panel, "TextBox2");
				AssertEquals("222", textBox1.Text);

				panel.UpdateLayout(layout1);
				AssertControlsOrder(panel, "TextBox1", "TextBox2");
				AssertEquals("333", textBox1.Text);

				bo.Z0_NVarChar = "444";
				AssertControlsOrder(panel, "TextBox1", "TextBox2");
				AssertEquals("444", textBox1.Text);
			}
		}

		public void TestCaptions_EventHandlersAfterDispose()
		{
			var bo = Factory.New<DummyBusinessObject>();

			using (var panel = new DynamicLayoutPanel())
			{
				panel.SetDataBinding(bo, null);

				panel.UpdateLayout(CreateLayoutWithCaption(true));
				var textBox1 = panel.FindSingle<ZTextBox>(c => c.Name == nameof(ControlBagForTest2.TextBox1), 1);

				bo.Z0_NVarChar = "Test Caption (A)";
				AssertEquals("Test Caption (A)", textBox1.CaptionResourceString.Caption);

				bo.Z0_NVarChar = "Test Caption (B)";
				AssertEquals("Test Caption (B)", textBox1.CaptionResourceString.Caption);
			}

			// changing business object should not cause exceptions
			bo.Z0_NVarChar = "Test Caption (C)";
		}

		public void TestVerticalAlignment()
		{
			var common = ControlBagForForMultiHeightTest.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(common);

			layout.Include(common.Button1, common.Button2);
			layout.Include(common.Button3, common.Button4);

			using (var panel = new DynamicLayoutPanel())
			{
				panel.UpdateLayout(layout);

				var button1 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForForMultiHeightTest.Button1), 1);
				var button2 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForForMultiHeightTest.Button2), 1);
				var button3 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForForMultiHeightTest.Button3), 1);
				var button4 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForForMultiHeightTest.Button4), 1);

				int row1Top = Math.Min(button1.Top, button2.Top);
				int row2Top = Math.Min(button3.Top, button4.Top);
				int row1Bottom = Math.Max(button1.Bottom, button2.Bottom);
				int row2Bottom = Math.Max(button3.Bottom, button4.Bottom);

				int row1Height = row1Bottom - row1Top;
				int row2Height = row2Bottom - row2Top;

				AssertLessThan("there must be some space between rows", row1Bottom, row2Top);
				AssertLessThan("Button1 is smaller than Button2", button1.Height, button2.Height);
				AssertLessThan("Button4 is smaller than Button3", button4.Height, button3.Height);

				AssertEquals(row1Height, button2.Height);
				AssertEquals(row2Height, button3.Height);

				int button1TopMargin = button1.Top - row1Top;
				int button1BottomMargin = row1Bottom - button1.Bottom;

				int button4TopMargin = button4.Top - row2Top;
				int button4BottomMargin = row2Bottom - button4.Bottom;

				AssertLessThanOrEqualTo("Button1 is centered vertically", Math.Abs(button1TopMargin - button1BottomMargin), 1);
				AssertLessThanOrEqualTo("Button4 is centered vertically", Math.Abs(button4TopMargin - button4BottomMargin), 1);
			}
		}

		public void TestControlHeightGreaterThanOrEqualToSingleRowHeight()
		{
			var common = ControlBagForForMultiHeightTest.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(common);

			layout.Include(common.GroupBox1);
			layout.Include(common.GroupBox2);
			layout.Include(common.GroupBox3);

			using (var panel = new DynamicLayoutPanel())
			{
				panel.UpdateLayout(layout);

				var groupBox1 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForForMultiHeightTest.GroupBox1), 1);
				var groupBox2 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForForMultiHeightTest.GroupBox2), 1);
				var groupBox3 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForForMultiHeightTest.GroupBox3), 1);

				int row1Bottom = groupBox1.Bottom;
				int row2Top = groupBox2.Top;
				int row2Bottom = groupBox2.Bottom;
				int row3Top = groupBox3.Top;
				var yMargin = ControlDpiScalingHelper.ScaleToCurrentDpiY(4);

				AssertEquals("Space between rows", yMargin, row2Top - row1Bottom);
				AssertEquals("Space between rows is always same", yMargin, row3Top - row2Bottom);
			}
		}

		public void TestColumnLayout()
		{
			var common = ControlBagForTest.Instance;

			var columns2Layout = new PanelLayout();
			{
				columns2Layout.RegisterControlBag(common);

				var c2 = columns2Layout.AddColumn();
				columns2Layout.Include(common.Button1, common.Button2);
				columns2Layout.Include(c2, common.Button3);
				columns2Layout.Include(c2, common.Button4);
			}

			var columns3Layout = new PanelLayout();
			{
				columns3Layout.RegisterControlBag(common);

				var c2 = columns3Layout.AddColumn();
				var c3 = columns3Layout.AddColumn();
				columns3Layout.Include(common.Button1);
				columns3Layout.Include(c2, common.Button2);
				columns3Layout.Include(c2, common.Button3);
				columns3Layout.Include(c3, common.Button4);
			}

			var columns2by2Layout = new PanelLayout();
			{
				columns2by2Layout.RegisterControlBag(common);

				var c1 = columns2by2Layout.AddColumn();
				var c2 = columns2by2Layout.AddColumn();
				columns2by2Layout.Include(c1, common.Button1);
				columns2by2Layout.Include(c1, common.Button2);
				columns2by2Layout.Include(c2, common.Button3);
				columns2by2Layout.Include(c2, common.Button4);
			}

			using (var panel = new DynamicLayoutPanel())
			{
				panel.UpdateLayout(columns2Layout);

				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");
				AssertTabOrder(panel, "Button1", "Button2", "Button3", "Button4");
				CheckColumnsSeparators(panel, new[] { "Button1", "Button2" }, new[] { "Button3", "Button4" });

				panel.UpdateLayout(columns3Layout);

				AssertControlsOrder(panel, "Button1", "Button2", "Button4", "Button3");
				AssertTabOrder(panel, "Button1", "Button2", "Button3", "Button4");
				CheckColumnsSeparators(panel, new[] { "Button1" }, new[] { "Button2", "Button3" }, new[] { "Button4" });

				panel.UpdateLayout(columns2Layout);

				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");
				AssertTabOrder(panel, "Button1", "Button2", "Button3", "Button4");
				CheckColumnsSeparators(panel, new[] { "Button1", "Button2" }, new[] { "Button3", "Button4" });

				panel.UpdateLayout(CreateAllIncludedLayout());

				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");
				AssertTabOrder(panel, "Button1", "Button2", "Button3", "Button4");
				CheckColumnsSeparators(panel, new[] { "Button1", "Button2", "Button3", "Button4" });

				panel.UpdateLayout(columns2by2Layout);

				AssertControlsOrder(panel, "Button1", "Button3", "Button2", "Button4");
				AssertTabOrder(panel, "Button1", "Button2", "Button3", "Button4");
				CheckColumnsSeparators(panel, Array.Empty<string>(), new[] { "Button1", "Button2" }, new[] { "Button3", "Button4" });
			}
		}

		public void TestColumnLayoutTabSequenceRowWise()
		{
			var common = ControlBagForTest.Instance;

			var columns2Layout = new PanelLayout();
			{
				columns2Layout.RegisterControlBag(common);
				columns2Layout.TabSequence = PanelLayoutTabSequence.RowWise;

				var c2 = columns2Layout.AddColumn();
				columns2Layout.Include(common.Button1, common.Button2);
				columns2Layout.Include(c2, common.Button3);
				columns2Layout.Include(c2, common.Button4);
			}

			var columns3Layout = new PanelLayout();
			{
				columns3Layout.RegisterControlBag(common);
				columns3Layout.TabSequence = PanelLayoutTabSequence.RowWise;

				var c2 = columns3Layout.AddColumn();
				var c3 = columns3Layout.AddColumn();
				columns3Layout.Include(common.Button1);
				columns3Layout.Include(c2, common.Button2);
				columns3Layout.Include(c2, common.Button3);
				columns3Layout.Include(c3, common.Button4);
			}

			var columns2by2Layout = new PanelLayout();
			{
				columns2by2Layout.RegisterControlBag(common);
				columns2by2Layout.TabSequence = PanelLayoutTabSequence.RowWise;

				var c1 = columns2by2Layout.AddColumn();
				var c2 = columns2by2Layout.AddColumn();
				columns2by2Layout.Include(c1, common.Button1);
				columns2by2Layout.Include(c1, common.Button2);
				columns2by2Layout.Include(c2, common.Button3);
				columns2by2Layout.Include(c2, common.Button4);
			}

			using (var panel = new DynamicLayoutPanel())
			{
				panel.UpdateLayout(columns2Layout);

				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");
				AssertTabOrder(panel, "Button1", "Button2", "Button3", "Button4");
				CheckColumnsSeparators(panel, new[] { "Button1", "Button2" }, new[] { "Button3", "Button4" });

				panel.UpdateLayout(columns3Layout);

				AssertControlsOrder(panel, "Button1", "Button2", "Button4", "Button3");
				AssertTabOrder(panel, "Button1", "Button2", "Button4", "Button3");
				CheckColumnsSeparators(panel, new[] { "Button1" }, new[] { "Button2", "Button3" }, new[] { "Button4" });

				panel.UpdateLayout(columns2Layout);

				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");
				AssertTabOrder(panel, "Button1", "Button2", "Button3", "Button4");
				CheckColumnsSeparators(panel, new[] { "Button1", "Button2" }, new[] { "Button3", "Button4" });

				var layout = CreateAllIncludedLayout();
				layout.TabSequence = PanelLayoutTabSequence.RowWise;
				panel.UpdateLayout(layout);

				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");
				AssertTabOrder(panel, "Button1", "Button2", "Button3", "Button4");
				CheckColumnsSeparators(panel, new[] { "Button1", "Button2", "Button3", "Button4" });

				panel.UpdateLayout(columns2by2Layout);

				AssertControlsOrder(panel, "Button1", "Button3", "Button2", "Button4");
				AssertTabOrder(panel, "Button1", "Button3", "Button2", "Button4");
				CheckColumnsSeparators(panel, Array.Empty<string>(), new[] { "Button1", "Button2" }, new[] { "Button3", "Button4" });
			}
		}

		public void TestAlignToControl()
		{
			var common = ControlBagForTest.Instance;

			var columns2Layout = new PanelLayout();
			{
				columns2Layout.RegisterControlBag(common);
				columns2Layout.Include(common.Button1);
				columns2Layout.Include(common.Button2);

				var c2 = columns2Layout.AddColumn();
				columns2Layout.Include(c2, common.Button3).AlignToControl = common.Button2;
				columns2Layout.Include(c2, common.Button4);
			}

			using (var panel = new DynamicLayoutPanel())
			{
				panel.UpdateLayout(columns2Layout);

				var button1 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button1), 1);
				var button2 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button2), 1);
				var button3 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button3), 1);
				var button4 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button4), 1);

				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");

				int singleRowHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(24);
				AssertEquals(button1.Top + singleRowHeight, button2.Top);
				AssertEquals(button2.Top, button3.Top);
				AssertEquals(button3.Top + singleRowHeight, button4.Top);
			}
		}

		public void TestAlignToControlVerticalAlignment()
		{
			var common = ControlBagForForMultiHeightTest.Instance;

			var columns2Layout = new PanelLayout();
			columns2Layout.RegisterControlBag(common);
			columns2Layout.Include(common.GroupBox2);

			var c2 = columns2Layout.AddColumn();
			var row = columns2Layout.Include(c2, common.Button1);
			row.AlignToControl = common.GroupBox2;
			row.VerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Bottom;

			using (var panel = new DynamicLayoutPanel())
			{
				panel.UpdateLayout(columns2Layout);

				var button1 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForForMultiHeightTest.Button1), 1);
				var groupBox2 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForForMultiHeightTest.GroupBox2), 1);

				AssertEquals("Bottom of button should line up with bottom of group box", groupBox2.Bottom, button1.Bottom);
			}

			row.VerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Top;

			using (var panel = new DynamicLayoutPanel())
			{
				panel.UpdateLayout(columns2Layout);

				var button1 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForForMultiHeightTest.Button1), 1);
				var groupBox2 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForForMultiHeightTest.GroupBox2), 1);

				AssertEquals("Top of button should line up with top of group box", groupBox2.Top, button1.Top);
			}

			row.VerticalAlignment = System.Windows.Forms.VisualStyles.VerticalAlignment.Center;

			using (var panel = new DynamicLayoutPanel())
			{
				panel.UpdateLayout(columns2Layout);

				var button1 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForForMultiHeightTest.Button1), 1);
				var groupBox2 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForForMultiHeightTest.GroupBox2), 1);

				var button1Center = (button1.Top + button1.Bottom) / 2;
				var groupBox2Center = (groupBox2.Top + groupBox2.Bottom) / 2;

				AssertEquals("Center of button should line up with center of group box", groupBox2Center, button1Center);
			}
		}

		public void TestWidthSettings()
		{
			// ....r1..r2.....r3
			// |   |   |      |
			// [   |   1      ]
			// [  2|   ][  3  ]
			//     [ 4 ]

			var common = ControlBagForTest.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(common);

			var r1 = layout.CreateRuler(100);
			var r2 = layout.CreateRightRuler(200);
			var r3 = layout.CreateRightRuler(300);

			layout.Include(common.Button1, r3);
			layout.Include(common.Button2, r2, common.Button3, r3);
			layout.Include(r1, common.Button4, r2);

			using (var panel = new DynamicLayoutPanel())
			{
				panel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
				panel.UpdateLayout(layout);

				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");

				var button1 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button1), 1);
				var button2 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button2), 1);
				var button3 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button3), 1);
				var button4 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button4), 1);

				AssertEquals(button1.Right, button3.Right);
				AssertEquals(button2.Right, button4.Right);
				AssertEquals(button1.Left, button2.Left);

				AssertLessThanOrEqualTo(button2.Right, button3.Left);
				AssertLessThan(button2.Left, button4.Left);
			}
		}

		public void TestZCodeFindBoxWidthSettings()
		{
			var layout = new PanelLayout();
			var bag = new BagWithZCodeFindBox();
			layout.RegisterControlBag(bag);

			var r = layout.CreateRightRuler(200);
			layout.Include(bag.Button, r);
			layout.Include(bag.CodeFindBox, r);

			using (var panel = new DynamicLayoutPanel())
			{
				panel.UpdateLayout(layout);

				AssertControlsOrder(panel, "Button", "CodeFindBox");

				var button = panel.FindSingle<Control>(c => c.Name == nameof(BagWithZCodeFindBox.Button), 1);
				var codeFindBox = panel.FindSingle<Control>(c => c.Name == nameof(BagWithZCodeFindBox.CodeFindBox), 1);

				AssertEquals(button.Left, codeFindBox.Left);
				AssertEquals(button.Right, codeFindBox.Right);
			}
		}

		public void TestCollapsibleRows()
		{
			using (var panel = new DynamicLayoutPanel())
			{
				var bo = Factory.New<DummyBusinessObject>();
				bo.Z0_NVarChar = "Show";
				panel.SetDataBinding(bo, null);

				panel.UpdateLayout(CreateCollapsibleLayout(collapseEmptyRows: true));
				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");

				var button1 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button1), 1);
				var button2 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button2), 1);
				var button3 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button3), 1);
				var button4 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button4), 1);

				int button1Top = button1.Top;
				int button2Top = button2.Top;
				int button3Top = button3.Top;
				int button4Top = button4.Top;

				bo.Z0_NVarChar = "Hide";
				AssertControlsOrder(panel, "Button1", "Button3", "Button4");
				AssertEquals(button1Top, button1.Top);
				AssertEquals(button2Top, button3.Top);
				AssertEquals(button3Top, button4.Top);

				bo.Z0_NVarChar = "Show";
				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");
				AssertEquals(button1Top, button1.Top);
				AssertEquals(button2Top, button2.Top);
				AssertEquals(button3Top, button3.Top);
				AssertEquals(button4Top, button4.Top);
			}
		}

		public void TestNonCollapsibleRows()
		{
			using (var panel = new DynamicLayoutPanel())
			{
				var bo = Factory.New<DummyBusinessObject>();
				bo.Z0_NVarChar = "Show";
				panel.SetDataBinding(bo, null);

				panel.UpdateLayout(CreateCollapsibleLayout(collapseEmptyRows: false));
				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");

				var button1 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button1), 1);
				var button2 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button2), 1);
				var button3 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button3), 1);
				var button4 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button4), 1);

				int button1Top = button1.Top;
				int button2Top = button2.Top;
				int button3Top = button3.Top;
				int button4Top = button4.Top;

				bo.Z0_NVarChar = "Hide";
				AssertControlsOrder(panel, "Button1", "Button3", "Button4");
				AssertEquals(button1Top, button1.Top);
				AssertEquals(button3Top, button3.Top);
				AssertEquals(button4Top, button4.Top);

				bo.Z0_NVarChar = "Show";
				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");
				AssertEquals(button1Top, button1.Top);
				AssertEquals(button2Top, button2.Top);
				AssertEquals(button3Top, button3.Top);
				AssertEquals(button4Top, button4.Top);
			}
		}

		public void TestNullLayout()
		{
			using (var panel = new DynamicLayoutPanel())
			{
				AssertControlsOrder(panel);

				var bo = Factory.New<DummyBusinessObject>();
				panel.SetDataBinding(bo, null);

				AssertControlsOrder(panel);

				panel.UpdateLayout(CreateAllIncludedLayout());
				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");

				panel.UpdateLayout((PanelLayout)null);
				AssertControlsOrder(panel);
			}
		}

		public void TestUpdateLayout()
		{
			using (var panel = new DynamicLayoutPanel())
			{
				panel.UpdateLayout((IPanelLayoutProvider)null);
				AssertControlsOrder(panel);

				panel.UpdateLayout(new LayoutsForTesting(CreateAllIncludedLayout()));
				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");
			}
		}

		public void TestDispose()
		{
			Control button1;
			Control button2;

			using (var panel = new DynamicLayoutPanel())
			{
				panel.UpdateLayout(CreateAllIncludedLayout());
				AssertControlsOrder(panel, "Button1", "Button2", "Button3", "Button4");

				button1 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button1), 1);
				button2 = panel.FindSingle<Control>(c => c.Name == nameof(ControlBagForTest.Button2), 1);

				panel.UpdateLayout(CreateOneExcludedLayout());
				AssertControlsOrder(panel, "Button1", "Button3", "Button4");
			}

			Assert("Included control was disposed", button1.IsDisposed);
			Assert("Excluded control was disposed", button2.IsDisposed);
		}

		public void TestUpdateCaption_OnDependentPropertyChange()
		{
			var bo = Factory.New<DummyBusinessObjectSupportWithResourceStringDataWithIsApplicable>();
			using (var panel = new DynamicLayoutPanel())
			{
				var layout = CreateLayoutToVerifyResourceStringDataCaptionUpdateTest();
				panel.SetDataBinding(bo, null);
				panel.UpdateLayout(layout);

				var textBoxControl = panel.FindSingle<ZTextBox>("TextBox1");
				var captionRenderer = textBoxControl.Extensions.Get<ILabelCaptionRenderer>();
				AssertEquals("Caption", "Description_3", captionRenderer?.Caption);

				bo.SetDummyValue(1);
				IControlHost controlHost = panel;
				controlHost.UpdateLayout();
				AssertEquals("Caption after Update", "Description_2", captionRenderer?.Caption);
			}
		}

		public static void AssertControlsOrder(Control panel, params string[] expectedControlOrder)
		{
			AssertEquals(
				string.Join("\r\n", expectedControlOrder),
				string.Join("\r\n", panel.Controls.Cast<Control>().Where(c => c.Visible && !c.Name.StartsWith("ColumnSeparator")).OrderBy(c => c.Top).ThenBy(c => c.Left).Select(c => c.Name))
			);
		}

		void AssertTabOrder(Control panel, params string[] expectedControlOrder)
		{
			var sortedControls = panel.Controls.Cast<Control>().Where(c => c.Visible && c.TabStop).OrderBy(c => c.TabIndex).ToList();
			AssertEquals(
				string.Join("\r\n", expectedControlOrder),
				string.Join("\r\n", sortedControls.Select(c => c.Name))
			);
			for (int i = 1; i < sortedControls.Count; i++)
			{
				AssertLessThan("controls should not have same TabIndex", sortedControls[i - 1].TabIndex, sortedControls[i].TabIndex);
			}
		}

		void CheckColumnsSeparators(Control panel, params string[][] columns)
		{
			var columnSeparators = panel.Find(c => c.Name.StartsWith("ColumnSeparator")).OrderBy(c => c.Left).ToList();
			AssertEquals(columns.Length - 1, columnSeparators.Count);

			Control[][] columnControls = columns.Select(col => col.Select(name => panel.FindSingle<Control>(c => c.Name == name, 1)).ToArray()).ToArray();
			for (int i = 1; i < columnControls.Length; i++)
			{
				var columnSeparator = columnSeparators[i - 1];
				foreach (var control in columnControls[i - 1])
				{
					AssertLessThanOrEqualTo(control.Right, columnSeparator.Left);
				}

				foreach (var control in columnControls[i])
				{
					AssertLessThanOrEqualTo(columnSeparator.Right, control.Left);
				}
			}

			foreach (var control in columnControls.SelectMany(col => col))
			{
				foreach (var separator in columnSeparators)
				{
					CombineAssertions("separator must cover full height of control to prevent captions from leaking to other columns", () =>
					{
						AssertLessThanOrEqualTo(separator.Top, control.Top);
						AssertLessThanOrEqualTo(control.Bottom, separator.Bottom);
					});
				}
			}
		}

		void AssertResourceStringData(IResCaptionedControl control, string shortCaption, string mediumCaption, string caption, string fullDescription)
		{
			var resourceStringData = control.CaptionResourceString;
			var name = ((Control)control).Name;
			AssertEquals(name + " - ShortCaption", shortCaption, resourceStringData.ShortCaption);
			AssertEquals(name + " - MediumCaption", mediumCaption, resourceStringData.MediumCaption);
			AssertEquals(name + " - Caption", caption, resourceStringData.Caption);
			AssertEquals(name + " - FullDescription", fullDescription, resourceStringData.FullDescription);
		}

		sealed class ControlBagForTest : ControlBag
		{
			public static ControlBagForTest Instance { get; } = new ControlBagForTest();

			ControlBagForTest()
			{
				Button1 = RegisterControl(nameof(Button1));
				Button2 = RegisterControl(nameof(Button2));
				Button3 = RegisterControl(nameof(Button3));
				Button4 = RegisterControl(nameof(Button4));
			}

			public ControlReference Button1 { get; }
			public ControlReference Button2 { get; }
			public ControlReference Button3 { get; }
			public ControlReference Button4 { get; }

			protected override Control CreateTemplate()
			{
				var template = new ZPanel();

				template.Controls.Add(new ZButton { Name = nameof(Button1) });
				template.Controls.Add(new ZButton { Name = nameof(Button2) });
				template.Controls.Add(new ZButton { Name = nameof(Button3) });
				template.Controls.Add(new ZButton { Name = nameof(Button4) });

				return template;
			}
		}

		sealed class ControlBagForTest2 : ControlBag
		{
			public static ControlBagForTest2 Instance { get; } = new ControlBagForTest2();

			ControlBagForTest2()
			{
				TextBox1 = RegisterControl(nameof(TextBox1));
				TextBox2 = RegisterControl(nameof(TextBox2));
			}

			public ControlReference TextBox1 { get; }
			public ControlReference TextBox2 { get; }

			protected override Control CreateTemplate()
			{
				var template = new ZUserControl();

				var textBox1 = new ZTextBox { Name = "TextBox1" };
				var textBox2 = new ZTextBox { Name = "TextBox2" };

				template.BindingSource.SetBindingMember(textBox1, nameof(DummyBusinessObject.Z0_NVarChar));

				textBox1.CaptionResourceString = Res.GetData("B27730B0-2397-43CD-9E67-3A67E21D1322", "Text Box 1");
				textBox2.CaptionResourceString = Res.GetData("ED68F529-914F-4D4A-A7C5-339228372935", "Text Box 2");

				template.Controls.Add(textBox1);
				template.Controls.Add(textBox2);

				return template;
			}
		}

		sealed class ControlBagForTestComplexControls : ControlBag
		{
			public static ControlBagForTestComplexControls Instance { get; } = new ControlBagForTestComplexControls();

			ControlBagForTestComplexControls()
			{
				ComplexControl = RegisterControl(nameof(ComplexControl));
				ComplexControl2 = RegisterControl(nameof(ComplexControl2));
			}

			public ControlReference ComplexControl { get; }
			public ControlReference ComplexControl2 { get; }

			protected override Control CreateTemplate()
			{
				var template = new ZUserControl();
				var control1 = new ComplexUserControlForTesting() { Name = nameof(ComplexControl) };
				template.Controls.Add(control1);
				var control2 = new ComplexUserControlForTesting() { Name = nameof(ComplexControl2) };
				control2.CaptionResourceString = new ResourceStringData("{1C171168-8DCD-4940-8B69-6F8B11C48AEC}", "HELLO WORLD");
				template.Controls.Add(control2);
				return template;
			}
		}

		sealed class ComplexUserControlForTesting : ZUserControl
		{
			public ComplexUserControlForTesting()
			{
				TextBox1 = new ZTextBox() { Name = nameof(TextBox1) };
				TextBox1.CaptionResourceString = new ResourceStringData("{E88B869A-71A2-4747-8D2D-C1CB660F828A}", "Control Text Box 1");
				BindingSource.SetBindingMember(TextBox1, nameof(DummyBusinessObject.Z0_VarCharMax));
				TextBox2 = new ZTextBox() { Name = nameof(TextBox2) };
				TextBox2.CaptionResourceString = new ResourceStringData("{40D5EF9A-C5B2-4EBA-9C7E-1DD6D4FDBEA9}", "Control Text Box 2");
				TextBox3 = new ZTextBox() { Name = nameof(TextBox3) };
				TextBox3.CaptionResourceString = new ResourceStringData("{85D188AB-D03A-4577-B492-8E6CA2AEBE67}", "Text Box 3", "Cntrl Text Box 3", "Control Text Box 3", "Full Control Text Box 3");
				BindingSource.SetBindingMember(TextBox3, nameof(DummyBusinessObject.Z0_SparseNVarChar));
				Controls.Add(TextBox1);
				Controls.Add(TextBox2);
				Controls.Add(TextBox3);
				CaptionResourceString = new ResourceStringData("{750D0D97-6EB0-43C8-8931-B19115C34E71}", "Control Text Main");
			}

			public static class ControlNames
			{
				public const string TextBox1 = nameof(ComplexUserControlForTesting.TextBox1);
				public const string TextBox2 = nameof(ComplexUserControlForTesting.TextBox2);
				public const string TextBox3 = nameof(ComplexUserControlForTesting.TextBox3);
			}

			readonly ZTextBox TextBox1;
			readonly ZTextBox TextBox2;
			readonly ZTextBox TextBox3;
		}

		sealed class ControlBagForForMultiHeightTest : ControlBag
		{
			public static ControlBagForForMultiHeightTest Instance { get; } = new ControlBagForForMultiHeightTest();

			ControlBagForForMultiHeightTest()
			{
				Button1 = RegisterControl(nameof(Button1));
				Button2 = RegisterControl(nameof(Button2));
				Button3 = RegisterControl(nameof(Button3));
				Button4 = RegisterControl(nameof(Button4));
				GroupBox1 = RegisterControl(nameof(GroupBox1));
				GroupBox2 = RegisterControl(nameof(GroupBox2));
				GroupBox3 = RegisterControl(nameof(GroupBox3));
			}

			public ControlReference Button1 { get; }
			public ControlReference Button2 { get; }
			public ControlReference Button3 { get; }
			public ControlReference Button4 { get; }
			public ControlReference GroupBox1 { get; }
			public ControlReference GroupBox2 { get; }
			public ControlReference GroupBox3 { get; }

			protected override Control CreateTemplate()
			{
				var template = new ZPanel();

				template.Controls.Add(new ZButton { Name = nameof(Button1), Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(20) });
				template.Controls.Add(new ZButton { Name = nameof(Button2), Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(30) });
				template.Controls.Add(new ZButton { Name = nameof(Button3), Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(40) });
				template.Controls.Add(new ZButton { Name = nameof(Button4), Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(20) });
				template.Controls.Add(new ZButton { Name = nameof(GroupBox1), Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(24) });
				template.Controls.Add(new ZButton { Name = nameof(GroupBox2), Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(78) });
				template.Controls.Add(new ZButton { Name = nameof(GroupBox3), Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(60) });

				return template;
			}
		}

		sealed class BagWithZCodeFindBox : ControlBag
		{
			public BagWithZCodeFindBox()
			{
				Button = RegisterControl(nameof(Button));
				CodeFindBox = RegisterControl(nameof(CodeFindBox));
			}

			public ControlReference Button { get; }
			public ControlReference CodeFindBox { get; }

			protected override Control CreateTemplate()
			{
				var template = new ZPanel();
				template.Controls.Add(new ZButton { Name = nameof(Button), Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(50) });
				template.Controls.Add(new ZCodeFindBox { Name = nameof(CodeFindBox), Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80), ShowDescriptionBox = false });
				return template;
			}
		}

		PanelLayout CreateAllIncludedLayout()
		{
			var common = ControlBagForTest.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(common);

			layout.Include(common.Button1);
			layout.Include(common.Button2);
			layout.Include(common.Button3);
			layout.Include(common.Button4);
			return layout;
		}

		PanelLayout CreateAllExcludedLayout()
		{
			var common = ControlBagForTest.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(common);

			return layout;
		}

		PanelLayout CreateOneExcludedLayout()
		{
			var common = ControlBagForTest.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(common);

			layout.Include(common.Button1);
			layout.Include(common.Button3);
			layout.Include(common.Button4);
			return layout;
		}

		PanelLayout CreateReverseOrderLayout()
		{
			var common = ControlBagForTest.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(common);

			layout.Include(common.Button4);
			layout.Include(common.Button3);
			layout.Include(common.Button2);
			layout.Include(common.Button1);
			return layout;
		}

		PanelLayout CreateLayoutWithVisibility()
		{
			var common = ControlBagForTest.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(common);

			layout.Include(common.Button1);
			layout.Include(common.Button2);

			layout.SetVisibility<DummyBusinessObject>(common.Button1, d => d.Z0_NVarChar == "A", d => d.Z0_NVarCharInfo);

			return layout;
		}

		PanelLayout CreateCollapsibleLayout(bool collapseEmptyRows)
		{
			var common = ControlBagForTest.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(common);

			layout.CollapseEmptyRows = collapseEmptyRows;

			layout.Include(common.Button1);
			layout.Include(common.Button2);
			layout.Include(common.Button3);
			layout.Include(common.Button4);

			layout.SetVisibility<DummyBusinessObject>(common.Button2, d => d.Z0_NVarChar == "Show", d => d.Z0_NVarCharInfo);

			return layout;
		}

		PanelLayout CreateLayoutWithCaption(bool shouldSetCaption)
		{
			var common = ControlBagForTest2.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(common);

			layout.Include(common.TextBox1);
			layout.Include(common.TextBox2);

			if (shouldSetCaption)
			{
				layout.SetCaption<DummyBusinessObject>(common.TextBox1, d => Res.GetData("DB168336-B6C6-44D1-BA35-8711C9528EC1", d.Z0_NVarChar), d => d.Z0_NVarCharInfo);
			}

			return layout;
		}

		PanelLayout CreateLayoutWithComplexControlCaptions(bool shouldSetCaption)
		{
			var common = ControlBagForTestComplexControls.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(common);

			layout.Include(common.ComplexControl);
			layout.Include(common.ComplexControl2);

			if (shouldSetCaption)
			{
				layout.SetCaptions<DummyBusinessObject>(common.ComplexControl, d => new Dictionary<string, ResourceStringData>()
				{
					{ nameof(ControlBagForTestComplexControls.ComplexControl), Res.GetData("ABC", d.Z0_Description) },
					{ ComplexUserControlForTesting.ControlNames.TextBox1, Res.GetData("ABC2", d.Z0_VarCharMax, d.Z0_NVarChar, d.Z0_NVarCharMax) },
					{ ComplexUserControlForTesting.ControlNames.TextBox3, Res.GetData("ABC3", d.Z0_SparseNVarChar) }
				}, d => d.Z0_DescriptionInfo, d => d.Z0_VarCharMaxInfo, d => d.Z0_NVarCharInfo, d => d.Z0_NVarCharMaxInfo, d => d.Z0_SparseNVarCharInfo);
				layout.SetCaptions<DummyBusinessObject>(common.ComplexControl2, d => new Dictionary<string, ResourceStringData>() { { nameof(ControlBagForTestComplexControls.ComplexControl2), Res.GetData("ABC4", d.Z0_Description) } }, d => d.Z0_DescriptionInfo);
			}

			return layout;
		}

		PanelLayout CreateLayoutToVerifyResourceStringDataCaptionUpdateTest()
		{
			var bag = ControlBagForResourceStringDataCaptionUpdateTest.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(bag);
			layout.Include(bag.TextBox1);
			return layout;
		}

		sealed class ControlBagForResourceStringDataCaptionUpdateTest : ControlBag
		{
			public static ControlBagForResourceStringDataCaptionUpdateTest Instance { get; } = new ControlBagForResourceStringDataCaptionUpdateTest();

			ControlBagForResourceStringDataCaptionUpdateTest()
			{
				TextBox1 = RegisterControl(nameof(TextBox1));
			}

			public ControlReference TextBox1 { get; }

			protected override Control CreateTemplate() => new ControlForResourceStringDataCaptionUpdateTest();
		}

		sealed class ControlForResourceStringDataCaptionUpdateTest : ZUserControl
		{
			public ControlForResourceStringDataCaptionUpdateTest()
			{
				TextBox1 = new ZTextBox { Name = nameof(TextBox1) };
				TextBox1.Visible = true;
				BindingSource.SetBindingMember(TextBox1, nameof(DummyBusinessObjectSupportWithResourceStringDataWithIsApplicable.Z0_Description));
				Controls.Add(TextBox1);
			}

			readonly ZTextBox TextBox1;
		}

		#region Binding to Child BO Collection

		public void TestSetDataBindingToChildCollection()
		{
			var bo = Factory.New<DummyBusinessObject>();
			using (var form = new ParentFormForTest(bo))
			{
				form.Show();

				var childBO = bo.Collection.AddNew();
				var panel = form.PanelForChildBusinessObject as IDataBoundControl;

				form.PanelForChildBusinessObject.SetDataBinding(bo, "Collection");
				AssertEquals("DataSource", bo, panel.DataSource);
				AssertEquals("Binding to Child", "Collection", panel.DataMember);
				Assert("PanelForChildBusinessObject.Enabled", form.PanelForChildBusinessObject.Enabled);

				form.PanelForChildBusinessObject.SetDataBinding(childBO, "");
				AssertEquals("DataSource", childBO, panel.DataSource);
				AssertEquals("DataMember", "", panel.DataMember);
				Assert("PanelForChildBusinessObject.Enabled", form.PanelForChildBusinessObject.Enabled);
			}
		}

		public void TestVisibilityForChildProperties()
		{
			var bo = Factory.New<DummyBusinessObject>();
			var childBO = Factory.New<DummyChildWithParentBusinessObject>();
			bo.Collection.Add(childBO);

			using (var form = new ParentFormForTest(bo))
			{
				var panel = form.PanelForChildBusinessObject;
				panel.SetDataBinding(childBO, "");
				panel.UpdateLayout(CreateChildLayoutWithVisibility());
				form.Show();

				form.TabControl.SelectedIndex = 1;
				AssertControlsOrder(panel, "TextBox2");

				bo.Z0_NVarChar = "A";
				AssertControlsOrder(panel, "TextBox2");
			}

			childBO.Z0_Guid = bo.PK;
			using (var form = new ParentFormForTest(bo))
			{
				var panel = form.PanelForChildBusinessObject;
				panel.SetDataBinding(childBO, "");
				panel.UpdateLayout(CreateChildLayoutWithVisibility());

				form.Show();
				form.TabControl.SelectedIndex = 1;
				AssertControlsOrder(panel, "TextBox1", "TextBox2");

				bo.Z0_NVarChar = "B";
				AssertControlsOrder(panel, "TextBox2");

				form.TabControl.SelectedIndex = 0;
				bo.Z0_NVarChar = "A";
				form.TabControl.SelectedIndex = 1;
				AssertControlsOrder(panel, "TextBox1", "TextBox2");

				form.TabControl.SelectedIndex = 0;
				bo.Z0_NVarChar = "B";
				form.TabControl.SelectedIndex = 1;
				AssertControlsOrder(panel, "TextBox2");
			}
		}

		sealed class DummyChildWithParentBusinessObject : DummyChildBusinessObject
		{
			public DummyChildWithParentBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DummyBusinessObject Parent => Factory.Load<DummyBusinessObject>(Z0_Guid);
		}

		sealed class ChildControlBagForTest : ControlBag
		{
			public static ChildControlBagForTest Instance { get; } = new ChildControlBagForTest();

			ChildControlBagForTest()
			{
				TextBox1 = RegisterControl(nameof(TextBox1));
				TextBox2 = RegisterControl(nameof(TextBox2));
			}

			public ControlReference TextBox1 { get; }
			public ControlReference TextBox2 { get; }

			protected override Control CreateTemplate()
			{
				var template = new ZUserControl();

				var textBox1 = new ZTextBox { Name = "TextBox1" };
				var textBox2 = new ZTextBox { Name = "TextBox2" };

				template.BindingSource.SetBindingMember(textBox1, nameof(DummyChildWithParentBusinessObject.Z0_NVarChar));
				template.BindingSource.SetBindingMember(textBox2, nameof(DummyChildWithParentBusinessObject.Z0_Description));

				textBox1.CaptionResourceString = Res.GetData("24CC3248-6B0E-47B6-9812-946EAFBE2B93", "Text Box 1");
				textBox2.CaptionResourceString = Res.GetData("E47C8D7D-4CD3-403D-A078-D6F830B1E309", "Text Box 2");

				template.Controls.Add(textBox1);
				template.Controls.Add(textBox2);

				return template;
			}
		}

		sealed class ParentFormForTest : ZForm
		{
			public ParentFormForTest(DummyBusinessObject bo) : base(bo)
			{
				var column = new ZTextBoxColumnStyleInfo("Z0_NVarChar", 100);
				ChildrenGrid.ColumnStyles.Add(column);
				TabControl.TabPages.Add(new TabPage());
				TabControl.TabPages.Add(TabPage);
				TabPage.Controls.Add(ChildrenGrid);
				TabPage.Controls.Add(PanelForChildBusinessObject);
				Controls.Add(TabControl);
				BindingSource.SetBindingMember(ChildrenGrid, "Collection");
				BindingSource.SetBindingMember(PanelForChildBusinessObject, "Collection");
				SetDataBinding(bo, "");
			}

			public ZTabControl TabControl = new ZTabControl();
			public ZTabPage TabPage = new ZTabPage();
			public ZGrid ChildrenGrid = new ZGrid();
			public DynamicLayoutPanel PanelForChildBusinessObject = new DynamicLayoutPanel();
		}

		PanelLayout CreateChildLayoutWithVisibility()
		{
			var common = ChildControlBagForTest.Instance;
			var layout = new PanelLayout();
			layout.RegisterControlBag(common);

			layout.Include(common.TextBox1);
			layout.Include(common.TextBox2);

			layout.SetVisibility<DummyChildWithParentBusinessObject>(common.TextBox1, c => (c.Parent?.Z0_NVarChar ?? ZString.Empty) == "A", c => c.Parent?.Z0_NVarCharInfo);

			return layout;
		}

		#endregion

		#region LayoutProviderWithExtensions

		[ExpectNoExceptions]
		public void TestUpdateLayout_ExtensionRegisterAndCleanupCalledOnLoad()
		{
			var mockLayoutExtension = new Mock<ILayoutExtension>();
			using (var panel = new DynamicLayoutPanel())
			{
				var layoutWithExtensions = new LayoutProviderWithExtensions(mockLayoutExtension.Object);
				panel.UpdateLayout(layoutWithExtensions);
				mockLayoutExtension.Verify(m => m.Initialize(It.Is<IControlHost>(f => f.Equals(panel))));
			}

			mockLayoutExtension.Verify(m => m.Cleanup(), Times.AtLeastOnce);
		}

		sealed class LayoutProviderWithExtensions : IPanelLayoutProviderWithExtensions
		{
			public LayoutProviderWithExtensions(ILayoutExtension testLayoutExtension)
			{
				this.testLayoutExtension = testLayoutExtension;
			}

			readonly ILayoutExtension testLayoutExtension;

			PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateLayout());
			PanelLayout layout;

			IReadOnlyCollection<ILayoutExtension> IPanelLayoutProviderWithExtensions.Extensions => new[] { testLayoutExtension };

			PanelLayout CreateLayout()
			{
				var common = ControlBagForTest2.Instance;
				var panelLayout = new PanelLayout();
				panelLayout.RegisterControlBag(common);
				panelLayout.Include(common.TextBox1);
				return panelLayout;
			}
		}

		#endregion

		#region IControlHost

		public void TestGetControlByName()
		{
			using (var panel = new DynamicLayoutPanel())
			{
				var common = ControlBagForTest2.Instance;
				var layout = new PanelLayout();

				layout.RegisterControlBag(common);
				layout.Include(common.TextBox1);

				panel.UpdateLayout(layout);
				IControlHost controlHost = panel;

				var control = controlHost.GetControlByName("FakeControlName");
				AssertNull("Control do not exist", control);

				control = controlHost.GetControlByName(common.TextBox1.Name);
				AssertNotNull("Control", control);
				AssertType<ZTextBox>(control);
			}
		}

		public void TestGetControlByName_EmptyControlName()
		{
			using (var panel = new DynamicLayoutPanel())
			{
				IControlHost controlHost = panel;
				AssertExceptionThrown<ArgumentException>("When ControlName is null", () => controlHost.GetControlByName(null));
				AssertExceptionThrown<ArgumentException>("When ControlName is Empty", () => controlHost.GetControlByName(""));
			}
		}

		public void TestGetCurrentDataItem()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			using (var panel = new DynamicLayoutPanel())
			{
				panel.SetDataBinding(dummyBO, null);
				panel.UpdateLayout(CreateAllIncludedLayout());

				IControlHost controlHost = panel;
				var dataItem = controlHost.GetCurrentDataItem();
				AssertNotNull(dataItem);
				AssertType<DummyBusinessObject>(dataItem);
				AssertEquals(dummyBO.PK, ((DummyBusinessObject)dataItem).PK);
			}
		}

		public void TestGetControl()
		{
			using (var panel = new DynamicLayoutPanel())
			{
				var common = ControlBagForTest2.Instance;
				var layout = new PanelLayout();

				layout.RegisterControlBag(common);
				layout.Include(common.TextBox1);

				panel.UpdateLayout(layout);
				IControlHost controlHost = panel;

				var control = controlHost.GetControl(common.TextBox1);
				AssertNotNull("Control", control);
				AssertType<ZTextBox>(control);
			}
		}

		#endregion

		#region IDynamicLayoutPanel and TabPage Notifications Exposure

		public void TestApplyBinding_SetsDataSourceFromParent()
		{
			var bizObj = Factory.New<DummyBusinessObject>();

			using (var form = new ZForm(bizObj))
			using (var dynamicLayoutPanel = new DynamicLayoutPanel())
			{
				form.Controls.Add(dynamicLayoutPanel);

				var dataBoundControl = (IDataBoundControl)dynamicLayoutPanel;
				AssertNull("PRE-CONDITION: DataSource", dataBoundControl.DataSource);

				var iDynamicLayoutPanel = (IDynamicLayoutPanel)dynamicLayoutPanel;
				iDynamicLayoutPanel.ApplyBinding();
				AssertSame("POST-CONDITION: DataSource", bizObj, dataBoundControl.DataSource);
			}
		}

		public void TestApplyBinding_ThroughTabPageNotificationsExposer()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			bizObj.Z0_Description = "MessageError";

			using (var form = new ZForm(bizObj))
			using (var tabControl = new ZTabControl())
			using (var tabPage = new ZTabPage())
			using (var dynamicLayoutPanel = new DynamicLayoutPanel())
			{
				tabPage.Controls.Add(dynamicLayoutPanel);
				tabControl.Controls.Add(tabPage);
				form.Controls.Add(tabControl);

				var dataBoundControl = (IDataBoundControl)dynamicLayoutPanel;
				AssertNull("PRE-CONDITION: DataSource", dataBoundControl.DataSource);

				TabPageNotificationsExposer.ExposeTabPageNotifications(form, bizObj);
				AssertSame("POST-CONDITION: DataSource", bizObj, dataBoundControl.DataSource);
			}
		}

		#endregion
	}
}

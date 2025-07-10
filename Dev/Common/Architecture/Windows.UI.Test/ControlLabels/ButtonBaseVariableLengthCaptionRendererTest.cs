using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Application = System.Windows.Forms.Application;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ButtonBaseVariableLengthCaptionRendererTest : TestCase
	{
		#region ToolTip

		[SuppressMessage("CargoWiseOne", "CW1046:DoNotSpecifyTooltipsManuallyRule", Justification = "Testing")]
		public void TestToolTipWorksIfTextChangedAfterLoad()
		{
			var button = new KButton();
			var renderer = new ButtonBaseVariableLengthCaptionRenderer(button);
			renderer.Captions = new string[] { "Test Tooltip" };
			Form.Controls.Add(button);
			ControlDpiScalingHelper.SetWidth(Form, 140, true);
			Form.Show();
			renderer.MouseOverButton();
			AssertEquals("Test Tooltip", ToolTipService.GetToolTip(button));
			ToolTipService.ClearTooltip(button);
			renderer.Captions = null;
			button.ToolTipCaption = (NoResString)"";
			button.Text = "New Test ToolTip";
			((IVariableLengthCaptionRenderer)button).Captions = new string[] { "New Test ToolTip" };
			renderer.MouseOverButton();
			AssertEquals("New Test ToolTip", ToolTipService.GetToolTip(button));
		}

		public void TestToolTipsCaptionGetUpdatedAfterTextChanged()
		{
			var button = new KButton();
			var renderer = new ButtonBaseVariableLengthCaptionRenderer(button);
			renderer.Captions = new[] { "Test Tooltip" };
			Form.Controls.Add(button);
			button.Text = "Test Tooltip update";
			ControlDpiScalingHelper.SetWidth(Form, 140, true);
			Form.Show();
			renderer.MouseOverButton();
			AssertEquals("Test Tooltip update", ToolTipService.GetToolTip(button));
		}

		public void TestToolTipCaption()
		{
			var button = new KButton();
			var renderer = new ButtonBaseVariableLengthCaptionRenderer(button);
			renderer.Captions = new string[] { "Captions" };
			Form.Controls.Add(button);
			Form.Show();
			renderer.MouseOverButton();
			AssertEquals("Captions", ToolTipService.GetToolTip(button));

			renderer.ToolTipCaption = (NoResString)"ToolTipCaption";
			Assert(renderer.HasAutoSetToolTip);

			renderer.MouseOverButton();
			AssertEquals("ToolTipCaption", ToolTipService.GetToolTip(button));
		}

		[SuppressMessage("Naming", "CW1046:CW1046 Do Not Specify Tooltips Manually Rule", Justification = "Control is not a ZButton")]
		public void TestToolTipDontOverrideCustomToolTip()
		{
			ToolTipService.SetToolTip(CheckBox, "This is custom tool tip");
			var renderer = new ButtonBaseVariableLengthCaptionRenderer(CheckBox);
			renderer.Captions = new string[] { "Test Tooltip", "Test &Another && ToolTip" };
			Form.Controls.Add(CheckBox);
			CheckBox.Text = renderer.Captions[0];
			ControlDpiScalingHelper.SetWidth(Form, 140, true);
			Form.Show();
			renderer.MouseOverButton();
			AssertEquals("This is custom tool tip", ToolTipService.GetToolTip(CheckBox));
			ToolTipService.ClearTooltip(CheckBox);
		}

		public void TestToolTipDontDuplicateAutoEllipsis()
		{
			var renderer = new ButtonBaseVariableLengthCaptionRenderer(CheckBox);
			CheckBox.AutoEllipsis = true;

			renderer.Captions = new string[] { "Test Tooltip", "Test &Another && ToolTip" };
			Form.Controls.Add(CheckBox);
			CheckBox.Text = renderer.Captions[0];
			ControlDpiScalingHelper.SetWidth(Form, 140, true);
			Form.Show();
			renderer.MouseOverButton();
			AssertEquals(string.Empty, ToolTipService.GetToolTip(CheckBox));
		}

		public void TestToolTipService()
		{
			var renderer = new ButtonBaseVariableLengthCaptionRenderer(CheckBox);
			renderer.Captions = new string[] { "Test Tooltip", "Test &Another && ToolTip" };
			Form.Controls.Add(CheckBox);
			CheckBox.Text = renderer.Captions[0];
			ControlDpiScalingHelper.SetWidth(Form, 140, true);
			Form.Show();
			renderer.MouseOverButton();
			AssertEquals("Test Tooltip", ToolTipService.GetToolTip(CheckBox));
		}

		#endregion

		#region Other Tests
		[GuiTest]
		public void TestCaption_NotCutOffByForm()
		{
			var renderer = new ButtonBaseVariableLengthCaptionRenderer(CheckBox);
			renderer.Captions = new string[] { "Caption 1", "Caption Length 2", "Caption Longer Length 3" };

			Form.Controls.Add(CheckBox);
			Form.Show();
			ControlDpiScalingHelper.SetWidth(Form, 140, true);
			AssertCollectionContains("Expected one of the longer captions", CheckBox.Text, new[] { "Caption Length 2", "Caption Longer Length 3" });
			Assert(ButtonBaseVariableLengthCaptionRenderer.CheckBoxWidth + CheckBox.Left + CheckBox.Padding.Size.Width + TextRenderer.MeasureText(CheckBox.Text, CheckBox.Font).Width < Form.Width);
		}

		public void TestCaption_NotCutOffBySiblingControl()
		{
			var renderer = new ButtonBaseVariableLengthCaptionRenderer(CheckBox);
			renderer.Captions = new[] { "Caption 1", "Caption Length 2", "Caption Longer Length 3" };

			CheckBox.Location = ControlDpiScalingHelper.NewScaledPoint(10, 10);
			SiblingControl.Bounds = ControlDpiScalingHelper.NewScaledRectangle(165, 10, 10, 10);
			Form.Controls.Add(CheckBox);
			Form.Controls.Add(SiblingControl);

			Form.Show();

			AssertEquals("The longest caption should have been used. IsOnStandardDpi: " + IsOnStandardDpi(), "Caption Longer Length 3", CheckBox.Text);
		}

		public void TestNoEllipsisIfIntersectsWithLeftControl()
		{
			SiblingControl.Bounds = ControlDpiScalingHelper.NewScaledRectangle(0, 0, 150, 10);

			var renderer = new ButtonBaseVariableLengthCaptionRenderer(CheckBox);
			renderer.Captions = new[] { "Einschließlich Staffelung" };
			CheckBox.Location = ControlDpiScalingHelper.NewScaledPoint(140, 0);

			Form.Controls.Add(CheckBox);
			Form.Controls.Add(SiblingControl);
			form.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(400);
			Form.Show();

			AssertNotContains("...", CheckBox.Text);
		}

		public void TestLeftNotModifiedWhenChoosingCaptionSize()
		{
			var renderer = new ButtonBaseVariableLengthCaptionRenderer(CheckBox);
			renderer.Captions = new string[] { "Caption 1", "Caption Length 2", "Caption Longer Length 3" };

			CheckBox.Location = ControlDpiScalingHelper.NewScaledPoint(50, 10);
			ControlDpiScalingHelper.SetWidth(Form, CheckBox.Left + CheckBox.Width / 2, false);
			SiblingControl.Bounds = ControlDpiScalingHelper.NewScaledRectangle(150, 10, 10, 10);
			Form.Controls.Add(CheckBox);

			Form.Show();
			AssertEquals("CheckBox not re-positioned when caption is calculated", ControlDpiScalingHelper.ScaleToCurrentDpiX(50), CheckBox.Left);
		}

		public void TestNotEnabledWhenTextSetManuallyFirst()
		{
			CheckBox.Text = "ManuallySet";
			var renderer = new ButtonBaseVariableLengthCaptionRenderer(CheckBox);
			renderer.Captions = new string[] { "Caption" };
			AssertEquals("CheckBox.Text not set if already set manually.", "ManuallySet", CheckBox.Text);
		}

		public void TestNotEnabledWhenTextSetManually()
		{
			var renderer = new ButtonBaseVariableLengthCaptionRenderer(CheckBox);
			renderer.Captions = new string[] { "Caption" };
			CheckBox.Text = "ManuallySet";
			renderer.Captions = new string[] { "Caption2" };
			AssertEquals("CheckBox.Text not set if already set manually.", "ManuallySet", CheckBox.Text);
		}

		public void TestWhenCheckIsAutoSizeAndRightAligned_ParentLayoutSuspended()
		{
			Form.Controls.Add(CheckBox);
			Form.SuspendLayout();
			Form.Show();

			var renderer = new ButtonBaseVariableLengthCaptionRenderer(CheckBox);
			CheckBox.CheckAlign = ContentAlignment.MiddleRight;
			CheckBox.AutoSize = true;
			CheckBox.Left = 100;
			renderer.Captions = new string[] { "Caption" };

			Form.ResumeLayout();
			Application.DoEvents();

			int right = CheckBox.Right;
			AssertEquals("Caption", CheckBox.Text);

			Form.SuspendLayout();

			renderer.Captions = new string[] { "Longer Caption" };

			Form.ResumeLayout();
			Application.DoEvents();

			AssertEquals("Longer Caption", CheckBox.Text);
			AssertEquals("The position of the right of the check box should stay the same", right, CheckBox.Right);
		}

		public void TestWhenCheckIsAutoSizeAndRightAligned()
		{
			Form.Controls.Add(CheckBox);
			Form.Show();

			var renderer = new ButtonBaseVariableLengthCaptionRenderer(CheckBox);
			CheckBox.CheckAlign = ContentAlignment.MiddleRight;
			CheckBox.AutoSize = true;
			CheckBox.Left = 100;
			renderer.Captions = new string[] { "Caption" };

			int right = CheckBox.Right;
			AssertEquals("Caption", CheckBox.Text);
			renderer.Captions = new string[] { "Longer Caption" };
			AssertEquals("Longer Caption", CheckBox.Text);
			AssertEquals("The position of the right of the check box should stay the same", right, CheckBox.Right);
		}

		public void TestButtonTextAutoEllipsis()
		{
			var button = new KButton();
			button.AutoSize = true;

			using (var form = new KForm())
			{
				form.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(136);
				form.Controls.Add(button);
				form.Show();

				var renderer = new ButtonBaseVariableLengthCaptionRenderer(button);
				renderer.Captions = new string[] { "Caption" };

				AssertEquals("Caption", button.Text);
				renderer.Captions = new string[] { "Longer Captionnnnnnnnn" };
				AssertContains("...", button.Text);

				button.AutoEllipsis = true;
				renderer.Captions = new string[] { "Longer Captionnnnnnnnn1" };
				AssertEquals("Longer Captionnnnnnnnn1", button.Text);

				button.AutoSize = false;
				button.AutoEllipsis = false;
				button.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
				renderer.Captions = new string[] { "Longer Caption" };
				AssertContains("...", button.Text);
			}
		}

		public void TestButtonTextShowCompletelyWithSufficientSpace()
		{
			var button = new KButton();
			button.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			button.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(60);
			using (var form = new KForm())
			{
				form.Controls.Add(button);
				form.Show();

				var renderer = new ButtonBaseVariableLengthCaptionRenderer(button);
				renderer.Captions = new string[] { "Longer Caption" };
				AssertEquals("Longer Caption", button.Text);
			}
		}

		public void TestButtonTextWithHeightLessThanFontHeight()
		{
			var button = new KButton();
			button.Font = new Font("Microsoft Sans Serif", 36);
			button.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			button.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(54);
			using (var form = new KForm())
			{
				form.Controls.Add(button);
				form.Show();

				var renderer = new ButtonBaseVariableLengthCaptionRenderer(button);
				renderer.Captions = new string[] { "AB" };
				AssertNotEquals("A...", button.Text);
				AssertEquals("AB", button.Text);
			}
		}

		public void TestCheckBoxNoObstruction()
		{
			Form.Controls.Add(CheckBox);
			Form.Show();

			var textBox = new TextBox();
			textBox.Location = ControlDpiScalingHelper.NewScaledPoint(233, 20, true);
			textBox.Size = ControlDpiScalingHelper.NewScaledSize(194, 20, true);
			textBox.Text = "Local Ref";

			checkBox.Location = ControlDpiScalingHelper.NewScaledPoint(62, 20, true);
			CheckBox.Size = ControlDpiScalingHelper.NewScaledSize(97, 17, true);
			var renderer = new ButtonBaseVariableLengthCaptionRenderer(CheckBox);
			renderer.Captions = new string[] { "Override Desc." };

			AssertEquals("The checkbox should be free from any obstructions", "Override Desc.", CheckBox.Text);
		}

		#endregion

		#region Implementation

		static bool IsOnStandardDpi()
		{
			// If we are on standard dpi, no scaling should occur, if the values are scaled then we must be non-standard
			return ControlDpiScalingHelper.ScaleToCurrentDpiX(100) == 100 && ControlDpiScalingHelper.ScaleToCurrentDpiY(100) == 100;
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (checkBox != null)
			{
				checkBox.Dispose();
			}
			if (form != null)
			{
				form.Dispose();
			}
		}

		KForm Form
		{
			get
			{
				if (form == null)
				{
					form = new KForm();
					form.Size = ControlDpiScalingHelper.NewScaledSize(200, 200, true);
					form.Location = ControlDpiScalingHelper.NewScaledPoint(200, 200, true);
				}
				return form;
			}
		}
		KForm form;

		CheckBox CheckBox
		{
			get
			{
				if (checkBox == null)
				{
					checkBox = new CheckBox();
					checkBox.AutoSize = true;
				}
				return checkBox;
			}
		}
		CheckBox checkBox;

		TextBox SiblingControl
		{
			get { return siblingControl ?? (siblingControl = new TextBox()); }
		}
		TextBox siblingControl;

		#endregion
	}
}

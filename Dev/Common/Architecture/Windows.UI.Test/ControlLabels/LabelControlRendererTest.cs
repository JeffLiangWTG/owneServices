using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using NUnit.Framework;
using Application = System.Windows.Forms.Application;

namespace CargoWise.Windows.UI.Testing
{
	sealed class LabelControlRendererTest : BaseExtensionTest<LabelCaptionRenderer>
	{
		public void TestCaption()
		{
			using (LabelCaptionRenderer labelCaptionRenderer = new LabelCaptionRenderer())
			{
				labelCaptionRenderer.Captions = new[] { "aaa", "aaa bbb", "aaa bbb ccc" };
				AssertEquals("aaa bbb ccc", labelCaptionRenderer.Caption);

				labelCaptionRenderer.Captions = new[] { "111", "111 222 333", "111 222" };
				AssertEquals("111 222 333", labelCaptionRenderer.Caption);

				labelCaptionRenderer.Captions = new[] { "xxx xxx xxx", "xxx xxx", "xxx" };
				AssertEquals("xxx xxx xxx", labelCaptionRenderer.Caption);

				labelCaptionRenderer.Captions = new[] { "aaa", "bbb", "ccc" };
				AssertEquals("aaa", labelCaptionRenderer.Caption);
			}
		}

		public void TestCaptionAbbreviation()
		{
			var textBox = new TextBox();
			textBox.Left = ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			Form.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			Form.Controls.Add(textBox);

			var captionRenderer = new TestLabelControlRenderer(textBox);
			captionRenderer.Caption = "Caption With Large Long";
			Form.Show();
			Application.DoEvents();
			Assert("Caption should be truncated and end with ...", captionRenderer.LastPaintedText.Contains("..."));
			captionRenderer.CallMouseMove();
			var rendererSurface = LabelCaptionRenderer.GetRenderSurface(textBox, captionRenderer.Alignment);
			Assert(ToolTipService.HasToolTip(rendererSurface));
			AssertEquals("Caption With Large Long", ToolTipService.GetToolTip(rendererSurface));
		}

		public void TestManualToolTip()
		{
			var textBox = new TextBox();
			textBox.Left = ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			Form.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			Form.Controls.Add(textBox);

			var captionRenderer = new TestLabelControlRenderer(textBox);
			captionRenderer.Caption = "Caption";
			Form.Show();
			Application.DoEvents();

			var rendererSurface = LabelCaptionRenderer.GetRenderSurface(textBox, captionRenderer.Alignment);
			captionRenderer.CallMouseMove();
			Assert(!ToolTipService.HasToolTip(rendererSurface));

			captionRenderer.ManuallySetCaptionToolTip("Test ToolTip");
			captionRenderer.CallMouseMove();

			Assert(ToolTipService.HasToolTip(rendererSurface));
			AssertEquals("Test ToolTip", ToolTipService.GetToolTip(rendererSurface));
		}

		public void TestManualToolTip_WhenCaptionIsTruncated()
		{
			var textBox = new TextBox();
			textBox.Left = ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			Form.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			Form.Controls.Add(textBox);

			var captionRenderer = new TestLabelControlRenderer(textBox);
			captionRenderer.Caption = "Long Caption";
			Form.Show();
			Application.DoEvents();

			Assert(captionRenderer.IsCaptionTruncated);

			var rendererSurface = LabelCaptionRenderer.GetRenderSurface(textBox, captionRenderer.Alignment);
			captionRenderer.ManuallySetCaptionToolTip("Test ToolTip");
			captionRenderer.CallMouseMove();

			Assert(ToolTipService.HasToolTip(rendererSurface));
			AssertEquals("Test ToolTip", ToolTipService.GetToolTip(rendererSurface));
		}

		[ExpectNoExceptions]
		public void TestControlNotVisibleWhenMouseMove()
		{
			TextBox textBox = new TextBox();
			textBox.Left = 30;
			textBox.Visible = false;
			Form.Width = 500;
			Form.Controls.Add(textBox);

			var captionRenderer = new TestLabelControlRenderer(textBox);
			captionRenderer.Caption = "Caption With Large Long1";
			Form.Show();
			Application.DoEvents();
			captionRenderer.IsPaintingActive = true;
			captionRenderer.CallMouseMove();
		}

		public void TestCaptionRegion_LeftAligned()
		{
			TestCaptionRegion(
				new string[] { "Caption 1", "Caption Width 2", "Caption Width Longer 3" },
				ControlDpiScalingHelper.NewScaledPoint(150, 20),
				LabelCaptionAlignment.Left,
				"Caption Width 2",
				ControlDpiScalingHelper.NewScaledRectangle(65, 25, 84, 13));
		}

		public void TestCaptionRegion_LeftAligned_WithWrap()
		{
			LabelRenderer.Options = StringRenderingOptions.Wrap;
			TestCaptionRegion(
				new string[] { "Caption 1", "Caption Width 2", "Caption Width Longer 3" },
				ControlDpiScalingHelper.NewScaledPoint(115, 20),
				LabelCaptionAlignment.Left,
				"Caption 1",
				ControlDpiScalingHelper.NewScaledRectangle(50, 18, 44, 26));
		}

		public void TestCaptionRegion_LeftAligned_WithLabelTopSpecified()
		{
			LabelRenderer.LabelTop = ControlDpiScalingHelper.ScaleToCurrentDpiY(10);
			TestCaptionRegion(
				new string[] { "Caption 1", "Caption Width 2", "Caption Width Longer 3" },
				ControlDpiScalingHelper.NewScaledPoint(150, 20),
				LabelCaptionAlignment.Left,
				"Caption Width 2",
				ControlDpiScalingHelper.NewScaledRectangle(65, 30, 84, 13));
		}

		public void TestCaptionRegion_TopAligned()
		{
			TestCaptionRegion(
				new string[] { "Caption 1", "Caption Width 2", "Caption Width Longer 3" },
				ControlDpiScalingHelper.NewScaledPoint(80, 20),
				LabelCaptionAlignment.Top,
				"Caption Width 2",
				ControlDpiScalingHelper.NewScaledRectangle(80, 5, 83, 13));
		}

		void TestCaptionRegion(string[] captions, Point controlLocation, LabelCaptionAlignment alignment, string expectedPaintedCaption, Rectangle expectedCaptionRegion)
		{
			Control.Size = ControlDpiScalingHelper.NewScaledSize(40, 20);
			Control.Location = controlLocation;
			SiblingControl.Location = ControlDpiScalingHelper.NewScaledPoint(10, 20);
			SiblingControl.Size = ControlDpiScalingHelper.NewScaledSize(40, 20);
			Form.Controls.Add(Control);
			Form.Controls.Add(SiblingControl);

			Form.Show();
			Application.DoEvents();
			LabelCaptionTestHelper.FireApplicationIdle();
			AssertEquals("Label not painted yet", null, LabelRenderer.LastPaintedText);

			LabelRenderer.Captions = captions;
			LabelRenderer.Alignment = alignment;
			LabelCaptionTestHelper.FireApplicationIdle();
			ForcePaintEvenWhenComputerLocked(Form);
			// while (true) Application.DoEvents(); //Uncomment this line to see it functionally
			if (ControlDpiScalingHelper.ScaleToCurrentDpiX(100) == 100) //Because text scales slightly slower than DPI, we need different assertions for normal and high DPI.
			{
				AssertEquals("Painted label text", expectedPaintedCaption, LabelRenderer.LastPaintedText);
				AssertRectangleEqualsWithTolerance("Painted text region", expectedCaptionRegion, LabelRenderer.LastPaintedRectangle);
			}

			bool foundCaption = false;
			foreach (string caption in captions.Reverse()) //longest to shortest
			{
				LabelRenderer.Caption = caption;
				LabelCaptionMeasurement measurement = LabelRenderer.MeasureCaption_Exposed();
				if (LabelRenderer.IsCaptionTruncated || measurement.CaptionBounds.Width > LabelRenderer.LastPaintedRectangle.Width || measurement.CaptionBounds.Height > LabelRenderer.LastPaintedRectangle.Height || measurement.CaptionBounds.Width == 0 || measurement.CaptionBounds.Height == 0)
				{
					AssertNotEquals("This caption should not have been drawn", LabelRenderer.LastPaintedText, caption);
				}
				else
				{
					AssertEquals("This caption should have been drawn", LabelRenderer.LastPaintedText, caption);
					AssertRectangleEqualsWithTolerance("Painted text region", expectedCaptionRegion, LabelRenderer.LastPaintedRectangle);
					Assert("Not too wide", Form.Width >= measurement.CaptionBounds.Width);
					Assert("Not too high", Form.Height >= measurement.CaptionBounds.Height);
					foundCaption = true;
					break;
				}
			}
			if (!foundCaption)
			{
				AssertEquals(null, LabelRenderer.LastPaintedText); //happens to TestCaptionRegion_LeftAligned_WithWrap() at 125% DPI and 250% DPI and to no other combination
			}
		}

		void AssertRectangleEqualsWithTolerance(string message, Rectangle expected, Rectangle actual, int tolerance = 15)
		{
			bool isWithinTolerance =
				Math.Abs(expected.X - actual.X) <= tolerance &&
				Math.Abs(expected.Y - actual.Y) <= tolerance &&
				Math.Abs(expected.Width - actual.Width) <= tolerance &&
				Math.Abs(expected.Height - actual.Height) <= tolerance;

			AssertEquals($"{message}: Expected {expected}, but got {actual} (Tolerance: {tolerance})", isWithinTolerance, true);
		}

		public void TestControlWithACaption()
		{
			TestExternalCaptionRenderer externalCaptionRenderer = new TestExternalCaptionRenderer();
			using (LabelCaptionRenderer renderer = new LabelCaptionRenderer(externalCaptionRenderer))
			{
				renderer.Captions = new string[] { "caption1", "caption2" };
				AssertEquals("ICaptionRenderer.Captions set", 2, externalCaptionRenderer.Captions.Length);
			}
		}

		public void TestCaptionVisibilityAfterHandleRecreated()
		{
			TextBox textBox = new TextBox();
			textBox.Left = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			Form.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			Form.Controls.Add(textBox);

			var captionRenderer = new TestLabelControlRenderer(textBox);
			captionRenderer.Caption = "Caption";
			Form.Show();
			Application.DoEvents();
			AssertEquals("Caption", captionRenderer.LastPaintedText);

			captionRenderer.LastPaintedText = null;
			typeof(Control).InvokeMember("RecreateHandle", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, textBox, null);
			Form.Refresh();
			AssertEquals("Caption should still be rendering after HandleCreated event raised again", "Caption", captionRenderer.LastPaintedText);
		}

		public void TestDontPaintOverExistingLabels()
		{
			Control.Size = ControlDpiScalingHelper.NewScaledSize(80, 20);
			Control.Location = ControlDpiScalingHelper.NewScaledPoint(60, 20);
			Form.Controls.Add(Control);
			LabelRenderer.Captions = new string[] { "Caption" };
			LabelRenderer.Alignment = LabelCaptionAlignment.Left;

			Label obstructingLabel = new Label();
			obstructingLabel.Text = "In the way";
			obstructingLabel.Location = ControlDpiScalingHelper.NewScaledPoint(5, 20);
			Form.Controls.Add(obstructingLabel);
			Form.Show();
			Application.DoEvents();
			LabelCaptionTestHelper.FireApplicationIdle();
			AssertNull("Label not painted over an existing label", LabelRenderer.LastPaintedText);

			obstructingLabel.Dispose();
			ForcePaintEvenWhenComputerLocked(Form);
			AssertNotNull("Label painted when label obstruction removed", LabelRenderer.LastPaintedText);
		}

		public void TestCapptionTruncateWhenNotHavingEnoughSpace()
		{
			var textBox = new TextBox();
			textBox.Left = ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			Form.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			Form.Controls.Add(textBox);

			var captionRenderer = new TestLabelControlRenderer(textBox);
			captionRenderer.Caption = "Long Caption";

			Form.Show();
			Application.DoEvents();

			Assert(captionRenderer.IsCaptionTruncated);
		}

		public void TestTruncateIfRenderedSurfaceIsScrolled()
		{
			Panel panel = new Panel { Size = ControlDpiScalingHelper.NewScaledSize(200, 200), Location = ControlDpiScalingHelper.NewScaledPoint(0, 0), AutoScroll = true };
			Form.Controls.Add(panel);

			Control.Size = ControlDpiScalingHelper.NewScaledSize(80, 20);
			Control.Location = ControlDpiScalingHelper.NewScaledPoint(51, 20);
			panel.Controls.Add(Control);

			Button button = new Button { Size = ControlDpiScalingHelper.NewScaledSize(17, 20), Location = ControlDpiScalingHelper.NewScaledPoint(200, 20) };
			panel.Controls.Add(button);

			LabelRenderer.Captions = new[] { "Caption" };
			LabelRenderer.Alignment = LabelCaptionAlignment.Left;
			LabelRenderer.Options = StringRenderingOptions.Truncate;

			Form.Show();
			Application.DoEvents();
			LabelCaptionTestHelper.FireApplicationIdle();
			ForcePaintEvenWhenComputerLocked(Form);

			AssertNotNull("Label painted when label obstruction removed", LabelRenderer.LastPaintedText);
			AssertEquals("Caption", LabelRenderer.LastPaintedText);

			panel.ScrollControlIntoView(button);
			Application.DoEvents();
			LabelCaptionTestHelper.FireApplicationIdle();
			ForcePaintEvenWhenComputerLocked(Form);

			AssertNotNull("Label painted when label obstruction removed", LabelRenderer.LastPaintedText);
			AssertEquals("Ca...", LabelRenderer.LastPaintedText);
		}

		public void TestCaptionChangedEventExist()
		{
			AssertNotNull("CaptionChanged event must exist on the class to prevent binding's PropertyDescriptor.AddValueChanged causing a memory leak", typeof(LabelCaptionRenderer).GetEvent("CaptionChanged"));
		}

		public void TestForeColor()
		{
			Control.Size = ControlDpiScalingHelper.NewScaledSize(80, 20);
			Control.Location = ControlDpiScalingHelper.NewScaledPoint(60, 20);
			Form.Controls.Add(Control);
			LabelRenderer.Captions = new string[] { "Caption" };
			LabelRenderer.Alignment = LabelCaptionAlignment.Left;

			AssertEquals("Empty initial fore color on invisible form", Color.Empty, LabelRenderer.ForeColor);

			Form.Show();
			Application.DoEvents();
			LabelCaptionTestHelper.FireApplicationIdle();

			Form.ForeColor = Color.Blue;
			ForcePaintEvenWhenComputerLocked(Form);

			LabelRenderer.ForeColor = Color.Brown;
			AssertEquals("Explicity set caption color", Color.Brown, LabelRenderer.ForeColor);
			ForcePaintEvenWhenComputerLocked(Form);
		}

		public void TestForTabPages()
		{
			using (KForm form = new KForm())
			using (KTabControl tabControl = new KTabControl())
			using (KTabPage tabPage = new KTabPage())
			using (KTabPage tabPage2 = new KTabPage())
			using (KTabControl tabControl2 = new KTabControl())
			using (KTabPage tabPage3 = new KTabPage())
			using (KTabPage tabPage4 = new KTabPage())
			{
				tabControl.TabPages.Add(tabPage);
				tabControl.TabPages.Add(tabPage2);
				tabPage2.Controls.Add(tabControl2);
				tabControl2.TabPages.Add(tabPage3);
				tabControl2.TabPages.Add(tabPage4);
				form.Controls.Add(tabControl);

				LabelCaptionRendererWithDefaultCaption renderer = new LabelCaptionRendererWithDefaultCaption(tabPage4);
				AssertEquals("Captions might not be available until TabControl.Created && Visible", "", tabPage4.Text);
				form.Show();
				Application.DoEvents();
				AssertEquals("Captions might not be available until TabControl.Created && Visible", "", tabPage4.Text);
				tabControl.SelectedTab = tabPage2;
				AssertEquals("Caption should be re-queried when tab control created and visible", "TabCaption", tabPage4.Text);
			}
		}

		public void TestRecalculateCaptionOnFormResizeAndControlsMove()
		{
			TextBox tb = new TextBox();
			tb.Size = ControlDpiScalingHelper.NewScaledSize(20, 20);
			tb.Location = ControlDpiScalingHelper.NewScaledPoint(60, 20);
			tb.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			Form.Controls.Add(tb);

			Control.Size = ControlDpiScalingHelper.NewScaledSize(20, 20);
			Control.Location = ControlDpiScalingHelper.NewScaledPoint(100, 20);
			Control.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			Form.Controls.Add(Control);

			LabelRenderer.Captions = new[] { "Quite long caption", "Caption", "C" };
			LabelRenderer.Alignment = LabelCaptionAlignment.Left;

			Form.Show();
			Application.DoEvents();
			LabelCaptionTestHelper.FireApplicationIdle();

			AssertEquals("C", LabelRenderer.captionMeasurement.Caption);

			Form.Width += ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			Application.DoEvents();
			LabelCaptionTestHelper.FireApplicationIdle();

			AssertEquals("C", LabelRenderer.captionMeasurement.Caption);
		}

		#region Test Classes

		class TestLabelControlRenderer : LabelCaptionRenderer
		{
			public TestLabelControlRenderer(Control control)
				: base(control)
			{
			}

			public string LastPaintedText { get; set; }
			public Rectangle LastPaintedRectangle { get; set; }

			protected override void Paint(PaintEventArgs e, LabelCaptionMeasurement captionMeasurement)
			{
				LastPaintedText = captionMeasurement.Caption;
				LastPaintedRectangle = captionMeasurement.CaptionBounds;
				base.Paint(e, captionMeasurement);
			}

			public void CallMouseMove()
			{
				painter.renderSurface_MouseMove(Control, new MouseEventArgs(MouseButtons.None, 0, 10, 10, 0));
			}

			public LabelCaptionMeasurement MeasureCaption_Exposed()
			{
				captionMeasurement = null;
				return MeasureCaption();
			}
		}

		class TestExternalCaptionRenderer : IVariableLengthCaptionRenderer
		{
			public string[] Captions { get; set; }
			public bool IsCaptionOverridden { get { return false; } set { } }
		}

		class LabelCaptionRendererWithDefaultCaption : LabelCaptionRenderer
		{
			public LabelCaptionRendererWithDefaultCaption(Control control)
				: base(control)
			{
			}

			public override string[] Captions
			{
				get
				{
					return
						(Control.Parent != null && Control.Parent.Created && Control.Parent.Visible) ?
						new string[] { "TabCaption" } : System.Array.Empty<string>();
				}
			}
		}

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			if (labelRenderer != null)
			{
				labelRenderer.Dispose();
			}
		}

		static void ForcePaintEvenWhenComputerLocked(Control control)
		{
#if !WINZOR
			control.DrawToBitmap(new Bitmap(control.Width, control.Height), control.Bounds);
#endif
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

		TextBox SiblingControl
		{
			get { return siblingControl ?? (siblingControl = CreateTextBoxWithStandardFont("Sibling")); }
		}
		TextBox siblingControl;

		TextBox Control
		{
			get { return control ?? (control = CreateTextBoxWithStandardFont("Target")); }
		}
		TextBox control;

		TextBox CreateTextBoxWithStandardFont(string text)
		{
			TextBox result = new TextBox();
			result.Text = text;
			result.Font = new Font(FontFamily.GenericSerif, 10);
			return result;
		}

		TestLabelControlRenderer LabelRenderer
		{
			get
			{
				if (labelRenderer == null)
				{
					labelRenderer = new TestLabelControlRenderer(Control);
				}
				return labelRenderer;
			}
		}
		TestLabelControlRenderer labelRenderer;

		#endregion
	}
}

using System;
using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Windows.UI.Testing
{
	sealed class VariableLengthCaptionRendererTest : TestCase
	{
		public void TestMeasureCaption()
		{
			var captions = new[] { "Caption 1", "Caption Even Longer 3", "Caption Longer 2" };
			SizeF size;
			bool truncated;

			AssertEquals("Caption Longer 2:", StringRenderingHelper.MeasureBestFit(captions, ControlDpiScalingHelper.ScaleToCurrentDpiX(120), ":", Font, Font.Height, StringRenderingOptions.Default, out size, out truncated));
			AssertScaledSizeEqual(string.Empty, 109, 16, size);

			AssertEquals("Caption Even Longer 3:", StringRenderingHelper.MeasureBestFit(captions, ControlDpiScalingHelper.ScaleToCurrentDpiX(150), ":", Font, Font.Height, StringRenderingOptions.Default, out size, out truncated));
			AssertScaledSizeEqual(string.Empty, 141, 16, size);

			AssertEquals("Caption 1:", StringRenderingHelper.MeasureBestFit(captions, ControlDpiScalingHelper.ScaleToCurrentDpiX(70), ":", Font, Font.Height, StringRenderingOptions.Default, out size, out truncated));
			AssertScaledSizeEqual("Don't wrap", 65, 16, size);
		}

		public void TestCaptionIsTruncatedWhenCaptionIsEmpty()
		{
			var captions = new[] { "" };
			SizeF size;
			bool truncated;

			AssertEquals("", StringRenderingHelper.MeasureBestFit(captions, ControlDpiScalingHelper.ScaleToCurrentDpiX(2), "", Font, Font.Height, StringRenderingOptions.Default, out size, out truncated));
			Assert(!truncated);
		}

		public void TestMeasureCaption_Wrap()
		{
			var captions = new[] { "Caption 1", "Caption Even Longer 3", "Caption Longer 2" };
			SizeF size;
			bool truncated;

			AssertEquals("Caption Even Longer 3:", StringRenderingHelper.MeasureBestFit(captions, ControlDpiScalingHelper.ScaleToCurrentDpiX(150), ":", Font, Font.Height, StringRenderingOptions.Wrap, out size, out truncated));
			AssertScaledSizeEqual(string.Empty, 141, 16, size);

			AssertEquals("Caption 1:", StringRenderingHelper.MeasureBestFit(captions, ControlDpiScalingHelper.ScaleToCurrentDpiX(20), ":", Font, Font.Height, StringRenderingOptions.Wrap, out size, out truncated));
			AssertScaledSizeEqual("Wraps to 2 lines - ie height 32", 51, 32, size);
		}

		public void TestMeasureCaption_Truncate()
		{
			var captions = new[] { "Caption 1", "Caption Even Longer 3", "Caption Longer 2" };
			SizeF size;
			bool truncated;

			AssertEquals("Caption Even Longer 3:", StringRenderingHelper.MeasureBestFit(captions, ControlDpiScalingHelper.ScaleToCurrentDpiX(150), ":", Font, Font.Height, StringRenderingOptions.Wrap, out size, out truncated));
			AssertScaledSizeEqual(string.Empty, 141, 16, size);

			var result = StringRenderingHelper.MeasureBestFit(captions, ControlDpiScalingHelper.ScaleToCurrentDpiX(45), ":", Font, Font.Height, StringRenderingOptions.Truncate, out size, out truncated);
			Assert("The best fit result should be 'Ca...' or 'Cap...'", string.Equals("Ca...:", result) || string.Equals("Cap...:", result));
			AssertScaledSizeEqual("Truncate", 40, 16, size);
		}

		public void TestHasObstruction()
		{
			using (var control = new Control())
			{
				control.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
				control.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(30);
				var measuredTextSize = ControlDpiScalingHelper.NewScaledSize(60, 20);
				Assert(StringRenderingHelper.HasObstruction(control, measuredTextSize));

				measuredTextSize = ControlDpiScalingHelper.NewScaledSize(40, 20);
				Assert(!StringRenderingHelper.HasObstruction(control, measuredTextSize));

				control.Height = control.Font.Height - 1;
				measuredTextSize = ControlDpiScalingHelper.NewScaledSize(40, control.Font.Height);
				Assert(!StringRenderingHelper.HasObstruction(control, measuredTextSize));
			}
		}

		public void TestHasObstructionWithoutControl()
		{
			using (var control = new Control())
			{
				control.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
				control.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(30);
				var measuredTextSize = ControlDpiScalingHelper.NewScaledSize(60, 20);
				Assert(StringRenderingHelper.HasObstruction(control.Size - control.Padding.Size, measuredTextSize, control.Font.Height));

				measuredTextSize = ControlDpiScalingHelper.NewScaledSize(40, 20);
				Assert(!StringRenderingHelper.HasObstruction(control.Size - control.Padding.Size, measuredTextSize, control.Font.Height));

				control.Padding = new Padding(8);
				measuredTextSize = ControlDpiScalingHelper.NewScaledSize(40, 20);
				Assert(StringRenderingHelper.HasObstruction(control.Size - control.Padding.Size, measuredTextSize, control.Font.Height));
				control.Padding = new Padding(0);

				control.Height = control.Font.Height - 1;
				measuredTextSize = ControlDpiScalingHelper.NewScaledSize(40, control.Font.Height);
				Assert(!StringRenderingHelper.HasObstruction(control.Size - control.Padding.Size, measuredTextSize, control.Font.Height));
			}
		}

		#region Test Classes

		[CodeAlive("WI00575476 Baseline")]
		class TestVariableLengthCaptionRenderer : IVariableLengthCaptionRenderer
		{
			public string[] Captions { get; set; }
			public bool IsCaptionOverridden { get { return false; } set { } }
		}

		#endregion

		#region Implementation

		Font Font
		{
			get { return font ?? (font = new Font(FontFamily.GenericSerif, 10)); }
		}
		Font font;

		void AssertScaledSizeEqual(string message, int width, int height, SizeF size)
		{
			var scaledWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(width);
			var scaledHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(height);

			var messageToDisplay = message + Environment.NewLine +
														 string.Format("Expected: Width={0}, Height={1}", scaledWidth, scaledHeight) + Environment.NewLine +
														 string.Format("Received: Width={0}, Height={1}", size.Width, size.Height);

			var heightIsWithinTolerance = IsEqualWithinTolerance(scaledHeight, size.Height);
			var widthIsWithinTolerance = IsEqualWithinTolerance(scaledWidth, size.Width);

			Assert(messageToDisplay.Trim(), heightIsWithinTolerance && widthIsWithinTolerance);
		}

		bool IsEqualWithinTolerance(float a, float b)
		{
			const int minimumPxTolerance = 3;
			const float minimumTolerance = 0.1F; // +- 10% tolerance

			return Math.Abs(a - b) <= minimumPxTolerance ||
						 Math.Abs(a / b - 1) <= minimumTolerance;
		}

		#endregion
	}
}

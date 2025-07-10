using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class TextRendererHelperTest : TestCaseWithFactory
	{
		public void TestDrawTextGDI()
		{
			AssertDrawText(TextRendererType.GDI);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestUnsupportedRenderer_DrawRectangle()
		{
			using (var font = new Font("Tahoma", 10))
			using (var graphics = Graphics.FromImage(new Bitmap(250, 250)))
			{
				TextRendererHelper.DrawText(graphics, "Test Drawing with Rect", font, new Rectangle(2, 2, 250, 250), Brushes.Red, null, (TextRendererType)54);
			}
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestUnsupportedRenderer_DrawPoint()
		{
			using (var font = new Font("Tahoma", 10))
			using (var graphics = Graphics.FromImage(new Bitmap(250, 250)))
			{
				TextRendererHelper.DrawText(graphics, "Test drawing with point", font, new PointF(2, 2), Brushes.Red, null, (TextRendererType)55);
			}
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestUnsupportedRenderer_Measure()
		{
			using (var font = new Font("Tahoma", 10))
			using (var graphics = Graphics.FromImage(new Bitmap(250, 250)))
			{
				TextRendererHelper.MeasureText(graphics, "Test measuring", font, Size.Empty, null, (TextRendererType)56);
			}
		}

		public void TestDrawTextGDIPlus()
		{
			AssertDrawText(TextRendererType.GDIPlus);
		}

		void AssertDrawText(TextRendererType textRendererType)
		{
			TextRendererHelper.UseTextRendererFromReg();
			EnvProxy.Instance.Registry.GraphicRenderingEngineRegItem = textRendererType.ToString();
			AssertEquals(textRendererType, TextRendererHelper.RenderingEngine);
			var text = "After creating the bitmap, the program creates a FileStream object associated with the file that should hold the bitmap.";
			using (var font = new Font("Tahoma", 10))
			{
				var brush = Brushes.Red;
				GetGraphics(out var image1, out var pureBlack, out var graphics);
				TextRendererHelper.DrawText(graphics, text, font, new Rectangle(2, 2, 250, 250), brush);
				AssertBitmapsAreNotSimilar(pureBlack, image1);
				GetGraphics(out var image2, out pureBlack, out graphics);
				TextRendererHelper.DrawText(graphics, text, font, new RectangleF(2, 2, 250, 250), brush);
				AssertBitmapsAreNotSimilar(pureBlack, image2);
				AssertBitmapsAreSimilar(image1, image2);
				GetGraphics(out var image3, out pureBlack, out graphics);
				TextRendererHelper.DrawText(graphics, text, font, new Point(2, 2), brush);
				AssertBitmapsAreNotSimilar(pureBlack, image3);
				GetGraphics(out var image4, out pureBlack, out graphics);
				TextRendererHelper.DrawText(graphics, text, font, 2, 2, brush);
				AssertBitmapsAreNotSimilar(pureBlack, image4);
				AssertBitmapsAreSimilar(image3, image4);
			}
		}

		public void TestBackBrush_GDI()
		{
			TextRendererHelper.UseTextRendererFromReg();
			var textRendererType = nameof(TextRendererType.GDI);
			EnvProxy.Instance.Registry.GraphicRenderingEngineRegItem = textRendererType;
			AssertEquals(textRendererType, textRendererType);
			var text = "After creating the bitmap, the program creates a FileStream object associated with the file that should hold the bitmap.";
			using (var font = new Font("Tahoma", 10))
			{
				var forBrush = Brushes.Red;
				var backBrush = Brushes.Blue;
				GetGraphics(out var image1, out var pureBlack, out var graphics);
				TextRendererHelper.DrawText(graphics, text, font, new Rectangle(2, 2, 250, 250), forBrush);
				AssertBitmapsAreNotSimilar(pureBlack, image1);
				GetGraphics(out var image2, out pureBlack, out graphics);
				TextRendererHelper.DrawText(graphics, text, font, new Rectangle(2, 2, 250, 250), forBrush, backBrush);
				AssertBitmapsAreNotSimilar(pureBlack, image2);
				AssertBitmapsAreNotSimilar(image1, image2);
				GetGraphics(out var image3, out pureBlack, out graphics);
				TextRendererHelper.DrawText(graphics, text, font, new RectangleF(2, 2, 250, 250), forBrush);
				AssertBitmapsAreNotSimilar(pureBlack, image3);
				GetGraphics(out var image4, out pureBlack, out graphics);
				TextRendererHelper.DrawText(graphics, text, font, new RectangleF(2, 2, 250, 250), forBrush, backBrush);
				AssertBitmapsAreNotSimilar(pureBlack, image4);
				AssertBitmapsAreNotSimilar(image3, image4);
				GetGraphics(out var image5, out pureBlack, out graphics);
				TextRendererHelper.DrawText(graphics, text, font, new Point(2, 2), forBrush);
				AssertBitmapsAreNotSimilar(pureBlack, image5);
				GetGraphics(out var image6, out pureBlack, out graphics);
				TextRendererHelper.DrawText(graphics, text, font, new Point(2, 2), forBrush, backBrush);
				AssertBitmapsAreNotSimilar(pureBlack, image6);
				AssertBitmapsAreNotSimilar(image5, image6);
			}
		}

		public void TestMeasureText()
		{
			Bitmap image;
			Graphics graphics;
			var text = "Test text to be measured.";
			var expectedSizes = new Dictionary<int, Tuple<SizeF, SizeF>>()
			{ { 96, new Tuple<SizeF, SizeF>(new SizeF(167, 17), new SizeF(161.3389f, 17.76041f)) }, { 120, new Tuple<SizeF, SizeF>(new SizeF(203, 21), new SizeF(201.6737f, 22.20052f)) }, { 144, new Tuple<SizeF, SizeF>(new SizeF(242, 24), new SizeF(242.0085f, 26.64063f)) }, { 168, new Tuple<SizeF, SizeF>(new SizeF(291, 29), new SizeF(282.3432f, 31.08073f)) }, { 192, new Tuple<SizeF, SizeF>(new SizeF(321, 33), new SizeF(322.6779f, 35.52083f)) }, { 216, new Tuple<SizeF, SizeF>(new SizeF(358, 36), new SizeF(363.0127f, 39.96094f)) }, { 240, new Tuple<SizeF, SizeF>(new SizeF(407, 41), new SizeF(403.3474f, 44.40104f)) }, { 288, new Tuple<SizeF, SizeF>(new SizeF(477, 48), new SizeF(484.0169f, 53.28125f)) }, { 336, new Tuple<SizeF, SizeF>(new SizeF(565, 57), new SizeF(564.6863f, 62.16145f)) } };
			using (var font = new Font("Tahoma", 10))
			{
				GetGraphics(out image, out graphics);
				var measuredTextSizeGDI = TextRendererHelper.MeasureText(graphics, text, font, TextRendererType.GDI);
				var measuredTextSizeGDIPlus = TextRendererHelper.MeasureText(graphics, text, font, TextRendererType.GDIPlus);
				var currentDpi = (int)ControlDpiScalingHelper.DpiX;
				var expectedResultSizeGDI = expectedSizes[currentDpi].Item1;
				var expectedResultSizeGDIPlus = expectedSizes[currentDpi].Item2;
				CombineAssertions(() =>
				{
					AssertSizeFApproximatelyEqual(expectedResultSizeGDI, measuredTextSizeGDI, currentDpi);
					AssertSizeFApproximatelyEqual(expectedResultSizeGDIPlus, measuredTextSizeGDIPlus, currentDpi);
				});
			}
		}

		public void TestMeasureText_DefaultNoBreakWork()
		{
			Bitmap image;
			Graphics graphics;
			var text = "Workflow & process";
			var expectedSizes = new Dictionary<int, Tuple<SizeF, SizeF>>()
			{ { 96, new Tuple<SizeF, SizeF>(new SizeF(130, 17), new SizeF(124.8741f, 17.76041f)) }, { 120, new Tuple<SizeF, SizeF>(new SizeF(157, 21), new SizeF(156.0926f, 22.20052f)) }, { 144, new Tuple<SizeF, SizeF>(new SizeF(185, 24), new SizeF(187.3112f, 26.64063f)) }, { 168, new Tuple<SizeF, SizeF>(new SizeF(226, 29), new SizeF(218.5297f, 31.08073f)) }, { 192, new Tuple<SizeF, SizeF>(new SizeF(251, 33), new SizeF(249.7482f, 35.52083f)) }, { 216, new Tuple<SizeF, SizeF>(new SizeF(277, 36), new SizeF(280.9668f, 39.96094f)) }, { 240, new Tuple<SizeF, SizeF>(new SizeF(316, 41), new SizeF(312.1853f, 44.40104f)) }, { 288, new Tuple<SizeF, SizeF>(new SizeF(372, 48), new SizeF(374.6224f, 53.28125f)) }, { 336, new Tuple<SizeF, SizeF>(new SizeF(440, 57), new SizeF(437.0594f, 62.16145f)) } };
			using (var font = new Font("Tahoma", 10))
			{
				GetGraphics(out image, out graphics);
				var measuredTextSizeGDI = TextRendererHelper.MeasureText(graphics, text, font, TextRendererType.GDI);
				var measuredTextSizeGDIPlus = TextRendererHelper.MeasureText(graphics, text, font, TextRendererType.GDIPlus);
				var currentDpi = (int)ControlDpiScalingHelper.DpiX;
				var expectedResultSizeGDI = expectedSizes[currentDpi].Item1;
				var expectedResultSizeGDIPlus = expectedSizes[currentDpi].Item2;
				CombineAssertions(() =>
				{
					AssertSizeFApproximatelyEqual(expectedResultSizeGDI, measuredTextSizeGDI, currentDpi);
					AssertSizeFApproximatelyEqual(expectedResultSizeGDIPlus, measuredTextSizeGDIPlus, currentDpi);
				});
			}
		}

		public void TestMeasureTextWithProposedSizeStringFormat()
		{
			Bitmap image;
			Graphics graphics;
			var text = "Test text to be measured.";
			var expectedSizes = new Dictionary<int, Tuple<SizeF, SizeF>>()
			{ { 96, new Tuple<SizeF, SizeF>(new SizeF(167f, 17f), new SizeF(161.3389f, 17.76041f)) }, { 120, new Tuple<SizeF, SizeF>(new SizeF(125f, 42f), new SizeF(177.946f, 20f)) }, { 144, new Tuple<SizeF, SizeF>(new SizeF(148f, 48f), new SizeF(174.6289f, 20f)) }, { 168, new Tuple<SizeF, SizeF>(new SizeF(179f, 58f), new SizeF(163.3903f, 20f)) }, { 192, new Tuple<SizeF, SizeF>(new SizeF(177f, 66f), new SizeF(172.2786f, 20f)) }, { 216, new Tuple<SizeF, SizeF>(new SizeF(176f, 108f), new SizeF(167.0801f, 20f)) }, { 240, new Tuple<SizeF, SizeF>(new SizeF(176f, 123f), new SizeF(167.0085f, 20f)) }, { 288, new Tuple<SizeF, SizeF>(new SizeF(175f, 192f), new SizeF(173.75f, 20f)) }, { 336, new Tuple<SizeF, SizeF>(new SizeF(178f, 285f), new SizeF(162.832f, 20f)) } };
			using (var font = new Font("Tahoma", 10))
			using (var stringFormat = new StringFormat())
			{
				var proposedSize = new Size(180, 20);
				GetGraphics(out image, out graphics);
				var measuredTextSizeGDI = TextRendererHelper.MeasureText(graphics, text, font, proposedSize, renderingEngine: TextRendererType.GDI);
				var measuredTextSizeGDIWithStringFormat = TextRendererHelper.MeasureText(graphics, text, font, proposedSize, stringFormat, TextRendererType.GDI);
				var measuredTextSizeGDIPlus = TextRendererHelper.MeasureText(graphics, text, font, proposedSize, renderingEngine: TextRendererType.GDIPlus);
				var measuredTextSizeGDIPlusWithStringFormat = TextRendererHelper.MeasureText(graphics, text, font, proposedSize, stringFormat, TextRendererType.GDIPlus);
				var currentDpi = (int)ControlDpiScalingHelper.DpiX;
				var expectedResultSizeGDI = expectedSizes[currentDpi].Item1;
				var expectedResultSizeGDIPlus = expectedSizes[currentDpi].Item2;
				CombineAssertions(() =>
				{
					AssertSizeFApproximatelyEqual(measuredTextSizeGDI, measuredTextSizeGDIWithStringFormat, currentDpi);
					AssertSizeFApproximatelyEqual(measuredTextSizeGDIPlus, measuredTextSizeGDIPlusWithStringFormat, currentDpi);
					AssertSizeFApproximatelyEqual(expectedResultSizeGDI, measuredTextSizeGDIWithStringFormat, currentDpi);
					AssertSizeFApproximatelyEqual(expectedResultSizeGDIPlus, measuredTextSizeGDIPlusWithStringFormat, currentDpi);
				});
			}
		}

		public void TestMeasureTextWithProposedSizeStringFormat_DefaultNoBreakWork()
		{
			Bitmap image;
			Graphics graphics;
			var text = "Workflow & process";
			var expectedSizes = new Dictionary<int, Tuple<SizeF, SizeF>>()
			{ { 96, new Tuple<SizeF, SizeF>(new SizeF(130f, 17f), new SizeF(124.8741f, 17.76041f)) }, { 120, new Tuple<SizeF, SizeF>(new SizeF(157f, 21f), new SizeF(156.0926f, 20f)) }, { 144, new Tuple<SizeF, SizeF>(new SizeF(185f, 24f), new SizeF(178.1152f, 20f)) }, { 168, new Tuple<SizeF, SizeF>(new SizeF(226f, 29f), new SizeF(173.348f, 20f)) }, { 192, new Tuple<SizeF, SizeF>(new SizeF(251f, 33f), new SizeF(173.3073f, 20f)) }, { 216, new Tuple<SizeF, SizeF>(new SizeF(277f, 36f), new SizeF(168.2373f, 20f)) }, { 240, new Tuple<SizeF, SizeF>(new SizeF(316f, 41f), new SizeF(153.0762f, 20f)) }, { 288, new Tuple<SizeF, SizeF>(new SizeF(372f, 48f), new SizeF(153.1055f, 20f)) }, { 336, new Tuple<SizeF, SizeF>(new SizeF(440f, 57f), new SizeF(178.623f, 20f)) } };
			using (var font = new Font("Tahoma", 10))
			using (var stringFormat = new StringFormat())
			{
				var proposedSize = new Size(180, 20);
				GetGraphics(out image, out graphics);
				var measuredTextSizeGDI = TextRendererHelper.MeasureText(graphics, text, font, proposedSize, renderingEngine: TextRendererType.GDI);
				var measuredTextSizeGDIWithStringFormat = TextRendererHelper.MeasureText(graphics, text, font, proposedSize, stringFormat, TextRendererType.GDI);
				var measuredTextSizeGDIPlus = TextRendererHelper.MeasureText(graphics, text, font, proposedSize, renderingEngine: TextRendererType.GDIPlus);
				var measuredTextSizeGDIPlusWithStringFormat = TextRendererHelper.MeasureText(graphics, text, font, proposedSize, stringFormat, TextRendererType.GDIPlus);
				var currentDpi = (int)ControlDpiScalingHelper.DpiX;
				var expectedResultSizeGDI = expectedSizes[currentDpi].Item1;
				var expectedResultSizeGDIPlus = expectedSizes[currentDpi].Item2;
				CombineAssertions(() =>
				{
					AssertSizeFApproximatelyEqual(measuredTextSizeGDI, measuredTextSizeGDIWithStringFormat, currentDpi);
					AssertSizeFApproximatelyEqual(measuredTextSizeGDIPlus, measuredTextSizeGDIPlusWithStringFormat, currentDpi);
					AssertSizeFApproximatelyEqual(expectedResultSizeGDI, measuredTextSizeGDIWithStringFormat, currentDpi);
					AssertSizeFApproximatelyEqual(expectedResultSizeGDIPlus, measuredTextSizeGDIPlusWithStringFormat, currentDpi);
				});
			}
		}

		void AssertSizeFApproximatelyEqual(SizeF expected, SizeF result, int currentDpi, float threshold = 0.001f)
		{
			var dw = Math.Abs(result.Width - expected.Width);
			var dh = Math.Abs(result.Height - expected.Height);
			var message = Invariant($"Only allow difference for {threshold}, expected:{expected}, result:{result}, current DPI:{currentDpi}, zoom:{currentDpi / 96.0f}");
			AssertLessThan(message, dw / expected.Width, threshold);
			AssertLessThan(message, dh / expected.Height, threshold);
		}

		static void GetGraphics(out Bitmap bitmap, out Graphics graphics)
		{
			var bitmapWidth = 250;
			var bitmapHeight = 250;
			bitmap = new Bitmap(bitmapWidth, bitmapWidth);
			graphics = Graphics.FromImage(bitmap);
			// Initialise to black.
			graphics.FillRectangle(Brushes.Black, 0, 0, bitmapWidth, bitmapHeight);
		}

		static void GetGraphics(out Bitmap image, out Bitmap pureBlack, out Graphics graphics)
		{
			var bitmapWidth = 250;
			var bitmapHeight = 250;
			image = new Bitmap(bitmapWidth, bitmapWidth);
			pureBlack = new Bitmap(bitmapWidth, bitmapWidth);
			graphics = Graphics.FromImage(image);
			// Initialise to black.
			var brush = Brushes.Black;
			graphics.FillRectangle(brush, 0, 0, bitmapWidth, bitmapHeight);
			Graphics.FromImage(pureBlack).FillRectangle(brush, 0, 0, bitmapWidth, bitmapHeight);
		}

		#region ImageComparison
		public static void AssertBitmapsAreNotSimilar(Bitmap imgExpected, Bitmap imgActual, float percentMatch = 0.05f, float threshold = 0.1f)
		{
			AssertGreaterThanOrEqualTo($"Bytes must match less than {percentMatch * 100}%", CompareImage(imgExpected, imgActual, threshold), percentMatch);
		}

		public static void AssertBitmapsAreSimilar(Bitmap imgExpected, Bitmap imgActual, float percentMatch = 0.95f, float threshold = 0.1f)
		{
			if (!imgExpected.Size.Equals(imgActual.Size))
			{
				Assert("Size must be matched to start with, Current DPI: " + (int)ControlDpiScalingHelper.DpiX, true);
				return;
			}

			AssertGreaterThanOrEqualTo($"Bytes must match at least {percentMatch * 100}%", CompareImage(imgExpected, imgActual, threshold), percentMatch);
		}

		// Helper method to visually debug two images. It opens a new form and displays both images side to side
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Utility to be triggered manually in testing.")]
		static void DebugGraphicalComparison(Bitmap img1, Bitmap img2, float percentMatch = 0f)
		{
#if DEBUG
			using (var debugForm = new Form())
			{
				debugForm.Text = percentMatch.ToString();
				debugForm.Visible = true;
				debugForm.AutoSize = false;
				debugForm.Size = new Size(Math.Max(img1.Width, img2.Width) + 34, img1.Height + img2.Height + 67) + SystemInformation.BorderSize + SystemInformation.Border3DSize;
				var g = debugForm.CreateGraphics();
				debugForm.Paint += (sender, e) =>
				{
					g.DrawImage(img1, new Point(10, 10));
					g.DrawRectangle(Pens.Blue, 10, 10, img1.Width, img1.Height);
					g.DrawImage(img2, new Point(10, img1.Height + 20));
					g.DrawRectangle(Pens.Green, 10, img1.Height + 20, img2.Width, img2.Height);
				};
				Application.DoEvents();
			}
#endif
		}

		// Euclidean distance in HSB space. Ideally we do this in LAB space and use something like CIELAB 76, but maybe next time
		static float ColourDistance_HSB(Color c1, Color c2)
		{
			// hue [0, 360]
			// saturation [0, 1]
			// brightness [0, 1]
			return (float)Math.Sqrt(Math.Pow((c1.GetHue() - c2.GetHue()) / 360f, 2) + Math.Pow(c1.GetSaturation() - c2.GetSaturation(), 2) + Math.Pow(c1.GetBrightness() - c2.GetBrightness(), 2));
		}

		// Returns a float [0, 1] where 0 means every pixel is different and 1 means every pixel is identical.
		static float CompareImage(Bitmap img1, Bitmap img2, float threshold = 0.1f)
		{
			int matchCount = 0;
			for (int y = 0; y < img1.Height; ++y)
			{
				for (int x = 0; x < img1.Width; ++x)
				{
					var px1 = img1.GetPixel(x, y);
					var px2 = img2.GetPixel(x, y);
					// short-circuit on exactly-equal colours
					if (px1 == px2 || ColourDistance_HSB(px1, px2) < threshold)
					{
						++matchCount;
					}
				}
			}

			return matchCount / (float)(img1.Width * img1.Height);
		}

		public void TestColourDistance_HSB()
		{
			AssertEquals(1f, ColourDistance_HSB(Color.White, Color.Black));
			AssertEquals(0f, ColourDistance_HSB(Color.Wheat, Color.Wheat));
			AssertEquals(0.4563626f, ColourDistance_HSB(Color.LightCoral, Color.DarkKhaki), 0.001f);
		}

		public void TestCompareImage()
		{
			var img1 = new Bitmap(2, 2);
			var img2 = new Bitmap(2, 2);
			var g1 = Graphics.FromImage(img1);
			var g2 = Graphics.FromImage(img2);
			var brush = Brushes.Black;
			g1.FillRectangle(brush, 0, 0, img1.Width, img1.Height);
			g2.FillRectangle(brush, 0, 0, img2.Width, img2.Height);
			AssertEquals(1f, CompareImage(img1, img2));
			img1.SetPixel(0, 0, Color.White);
			AssertEquals(0.75f, CompareImage(img1, img2));
			img1.SetPixel(0, 1, Color.White);
			AssertEquals(0.50f, CompareImage(img1, img2));
			img1.SetPixel(1, 0, Color.White);
			AssertEquals(0.25f, CompareImage(img1, img2));
			img1.SetPixel(1, 1, Color.White);
			AssertEquals(0f, CompareImage(img1, img2));
		}

		#endregion
		public void TestDefaultWhenFormatNull()
		{
			TextFormatFlags defaultTextFormatFlags = TextFormatFlags.TextBoxControl | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.NoPrefix;
			var gdiFlagResult = TextRendererHelper.ConvertToTextFormatFlags(null);
			AssertEquals(defaultTextFormatFlags, gdiFlagResult);
		}

		public void TestUnsupportedFormatFlags()
		{
			var expectedErrorKey = "UnsupportedConversionFormatUsed";
			using (var testsupportedFlags = new StringFormat())
			{
				testsupportedFlags.Trimming = StringTrimming.EllipsisCharacter;
				TextRendererHelper.ConvertToTextFormatFlags(testsupportedFlags);
				AssertNotEquals("Error must not be reported", expectedErrorKey, ErrorReporter.LastKeyReported);
			}

			ErrorReporter.Clear();
			using (var testUnnsupportedFlags = new StringFormat())
			{
				testUnnsupportedFlags.FormatFlags = StringFormatFlags.NoClip;
				testUnnsupportedFlags.FormatFlags = StringFormatFlags.DirectionVertical;
				testUnnsupportedFlags.Trimming = StringTrimming.EllipsisCharacter;
				var expectedMessage = string.Format(@"StringFormat passed to ConvertToTextFormatFlags() is not supported by GDI TextRenderingEngine. FormatFlag: " + testUnnsupportedFlags.FormatFlags);
				TextRendererHelper.ConvertToTextFormatFlags(testUnnsupportedFlags);
				AssertEquals("Error must be reported", expectedErrorKey, ErrorReporter.LastKeyReported);
				AssertEquals(expectedMessage, ErrorReporter.LastMessageReported);
			}

			ErrorReporter.Clear();
		}

		public void TestConvertToStringFormatFlags()
		{
			//Test expected to match
			var inputGdiPlusFormat = new StringFormat();
			inputGdiPlusFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;
			var expectedGdiPlusFlagToMatch = StringFormatFlags.DirectionRightToLeft;
			var correspondingGdiFlags = TextFormatFlags.RightToLeft;
			var gdiResult = TextRendererHelper.ConvertToTextFormatFlags(inputGdiPlusFormat.FormatFlags, expectedGdiPlusFlagToMatch, correspondingGdiFlags);
			AssertEquals(correspondingGdiFlags, gdiResult);
			//Test not expected to match
			inputGdiPlusFormat = new StringFormat();
			expectedGdiPlusFlagToMatch = StringFormatFlags.NoWrap;
			correspondingGdiFlags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl;
			gdiResult = TextRendererHelper.ConvertToTextFormatFlags(inputGdiPlusFormat.FormatFlags, expectedGdiPlusFlagToMatch, correspondingGdiFlags, false);
			AssertEquals(correspondingGdiFlags, gdiResult);
			inputGdiPlusFormat.Dispose();
		}

		public void TestMatchFlags()
		{
			const TextFormatFlags defaultTextFormatFlags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.NoPrefix;
			AssertGdiPlusFormatFlags(StringFormatFlags.DirectionRightToLeft | StringFormatFlags.NoClip | StringFormatFlags.NoWrap, TextFormatFlags.RightToLeft | TextFormatFlags.NoClipping | TextFormatFlags.NoPrefix);
			AssertGdiPlusFormatFlags(StringFormatFlags.DirectionRightToLeft, TextFormatFlags.RightToLeft | defaultTextFormatFlags);
			AssertGdiPlusFormatFlags(StringFormatFlags.NoClip, TextFormatFlags.NoClipping | TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPrefix);
			AssertGdiPlusFormatFlags(StringFormatFlags.FitBlackBox, TextFormatFlags.NoClipping | TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPrefix);
			AssertGdiPlusFormatFlags(StringFormatFlags.NoWrap, TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.NoPrefix);
			AssertGdiPlusTrimmingFlags(StringTrimming.EllipsisCharacter, TextFormatFlags.EndEllipsis | defaultTextFormatFlags);
			AssertGdiPlusTrimmingFlags(StringTrimming.EllipsisPath, TextFormatFlags.PathEllipsis | defaultTextFormatFlags);
			AssertGdiPlusTrimmingFlags(StringTrimming.EllipsisWord, TextFormatFlags.WordEllipsis | defaultTextFormatFlags);
			AssertGdiPlusAlignmentFlags(StringAlignment.Center, TextFormatFlags.HorizontalCenter | defaultTextFormatFlags);
			AssertGdiPlusAlignmentFlags(StringAlignment.Near, TextFormatFlags.Left | defaultTextFormatFlags);
			AssertGdiPlusAlignmentFlags(StringAlignment.Far, TextFormatFlags.Right | defaultTextFormatFlags);
			AssertGdiPlusLineAlignmentFlags(StringAlignment.Center, TextFormatFlags.VerticalCenter | defaultTextFormatFlags);
			AssertGdiPlusLineAlignmentFlags(StringAlignment.Near, TextFormatFlags.Top | defaultTextFormatFlags);
			AssertGdiPlusLineAlignmentFlags(StringAlignment.Far, TextFormatFlags.Bottom | defaultTextFormatFlags);
		}

		public void AssertGdiPlusFormatFlags(StringFormatFlags stringFormatFlags, TextFormatFlags expectedGdiFlags = TextFormatFlags.Default)
		{
			var gdiPlusFormatFlags = new StringFormat();
			gdiPlusFormatFlags.FormatFlags = stringFormatFlags;
			var gdiFlagResult = TextRendererHelper.ConvertToTextFormatFlags(gdiPlusFormatFlags);
			AssertEquals(expectedGdiFlags, gdiFlagResult);
		}

		public void AssertGdiPlusTrimmingFlags(StringTrimming stringTrimmingFlags, TextFormatFlags expectedGdiFlags)
		{
			var gdiPlusFormatFlags = new StringFormat();
			gdiPlusFormatFlags.Trimming = stringTrimmingFlags;
			var gdiFlagResult = TextRendererHelper.ConvertToTextFormatFlags(gdiPlusFormatFlags);
			AssertEquals(expectedGdiFlags, gdiFlagResult);
		}

		public void AssertGdiPlusAlignmentFlags(StringAlignment stringAlignment, TextFormatFlags expectedGdiFlags)
		{
			var gdiPlusFormatFlags = new StringFormat();
			gdiPlusFormatFlags.Alignment = stringAlignment;
			var gdiFlagResult = TextRendererHelper.ConvertToTextFormatFlags(gdiPlusFormatFlags);
			AssertEquals(expectedGdiFlags, gdiFlagResult);
		}

		public void AssertGdiPlusLineAlignmentFlags(StringAlignment stringAlignment, TextFormatFlags expectedGdiFlags)
		{
			var gdiPlusFormatFlags = new StringFormat();
			gdiPlusFormatFlags.LineAlignment = stringAlignment;
			var gdiFlagResult = TextRendererHelper.ConvertToTextFormatFlags(gdiPlusFormatFlags);
			AssertEquals(expectedGdiFlags, gdiFlagResult);
		}
	}
}

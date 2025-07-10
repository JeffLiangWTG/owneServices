using System.Drawing;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration.Testing
{
	public sealed class ShrinkToFitTest : TestCase
	{
		public void TestShrinkToFit()
		{
			var testAreaAt144Dpi = new SizeF(297.1689f, 24.76062f);
			var testDpi = new SizeF(144f, 144f);

			var dpi = Util.GetDpi();
			var testArea = new SizeF((testAreaAt144Dpi.Width * dpi.X) / testDpi.Width,
				(testAreaAt144Dpi.Height * dpi.Y) / testDpi.Height);

			AssertBestFontSize(null, 8f, 0, testArea, 8f);
			AssertBestFontSize("", 8f, 0, testArea, 8f);
			AssertBestFontSize("2ND FLOOR", 8f, 0, testArea, 8f);

			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH", 8f, 0, testArea, 8f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH ", 8f, 0, testArea, 8f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH D", 8f, 0, testArea, 7.5f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DR", 8f, 0, testArea, 7f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DRI", 8f, 0, testArea, 7f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DRIV", 8f, 0, testArea, 7f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DRIVE", 8f, 0, testArea, 6.5f);

			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DRIVE", 6f, 0, testArea, 6f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DRIVE", 60f, 0, testArea, 6.5f);
		}

		public void TestShrinkToFitForVerticalText()
		{
			var testAreaAt144Dpi = new SizeF(24.76062f, 297.1689f);
			var testDpi = new SizeF(144f, 144f);

			var dpi = Util.GetDpi();
			var testArea = new SizeF((testAreaAt144Dpi.Width * dpi.X) / testDpi.Width,
				(testAreaAt144Dpi.Height * dpi.Y) / testDpi.Height);

			AssertBestFontSize(null, 8f, 90, testArea, 8f);
			AssertBestFontSize("", 8f, 90, testArea, 8f);
			AssertBestFontSize("2ND FLOOR", 8f, 90, testArea, 8f);

			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH", 8f, 90, testArea, 8f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH ", 8f, 90, testArea, 8f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH D", 8f, 90, testArea, 7.5f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DR", 8f, 90, testArea, 7f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DRI", 8f, 90, testArea, 7f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DRIV", 8f, 90, testArea, 7f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DRIVE", 8f, 90, testArea, 6.5f);

			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DRIVE", 6f, 90, testArea, 6f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DRIVE", 60f, 90, testArea, 6.5f);

			AssertBestFontSize(null, 8f, 180, testArea, 8f);
			AssertBestFontSize("", 8f, 180, testArea, 8f);
			AssertBestFontSize("2ND FLOOR", 8f, 180, testArea, 8f);

			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH", 8f, 180, testArea, 8f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH ", 8f, 180, testArea, 8f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH D", 8f, 180, testArea, 7.5f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DR", 8f, 180, testArea, 7f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DRI", 8f, 180, testArea, 7f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DRIV", 8f, 180, testArea, 7f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DRIVE", 8f, 180, testArea, 6.5f);

			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DRIVE", 6f, 180, testArea, 6f);
			AssertBestFontSize("2ND FLOOR, 482 KINGSFORD SMITH DRIVE", 60f, 180, testArea, 6.5f);
		}

		public void TestHandleCharactersThrowingException()
		{
			var testArea = new SizeF(300f, 30f);

			var textWithUnsupportedCharacters = new string(new[]
			{
				(char)160,
				(char)773
			});

			AssertBestFontSize(textWithUnsupportedCharacters, 8f, 0, testArea, 8f);

			AssertEquals("ErrorReporter has been sent", "ShrinkToFitCalculator-ShrinkToFit", ErrorReporter.LastKeyReported);
			AssertEquals("ErrorReporter contains text", textWithUnsupportedCharacters, ErrorReporter.LastExceptionReported.Data["text"]);
			AssertEquals("ErrorReporter contains availableSize", testArea, ErrorReporter.LastExceptionReported.Data["availableSize"]);
			AssertEquals("ErrorReporter contains font", "Arial 8", ErrorReporter.LastExceptionReported.Data["font"]);
			AssertEquals("ErrorReporter contains FormatFlags", StringFormatFlags.MeasureTrailingSpaces, ErrorReporter.LastExceptionReported.Data["FormatFlags"]);

			var veryLongText = new string('z', 32001);
			AssertBestFontSize(veryLongText, 8f, 0, testArea, 8f);
			AssertEquals("ErrorReporter has not been sent again", textWithUnsupportedCharacters, ErrorReporter.LastExceptionReported.Data["text"]);

			ErrorReporter.Clear();
		}

		void AssertBestFontSize(string text, float fontSize, byte rotation, SizeF availableSize, float expectedFontSize)
		{
			var font = new Core.Font(
				"Arial",
				fontSize,
				FontStyle.Regular,
				Color.Black,
				rotation);

			var stringFormat = new StringFormat
			{
				Trimming = StringTrimming.None,
				Alignment = StringAlignment.Near,
				LineAlignment = StringAlignment.Near
			};

			stringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

			using (var shrinkToFitCalculator = new ShrinkToFitCalculator())
			{
				var actualFontSize = shrinkToFitCalculator.ShrinkToFit(
					text,
					availableSize,
					font,
					stringFormat);

				AssertEquals($"{text}, expected font size", expectedFontSize, actualFontSize);
			}
		}
	}
}

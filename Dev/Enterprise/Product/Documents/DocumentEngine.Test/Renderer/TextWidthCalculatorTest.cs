using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class TextWidthCalculatorTest : TestCase
	{
		public void TestGetLength()
		{
			var widthCalculator = new TextSizeCalculator(new Font("Arial", 12));
			AssertEquals(17, widthCalculator.GetLengthInMillimeter("Mohsen"));
		}

		public void TestGetHeight()
		{
			var text = "";
			var font = new Font("Arial", 12);
			var calculator = new TextSizeCalculator(font);

			AssertEquals(0, calculator.GetTextSize(text, font, GraphicsUnit.Millimeter).Height);

			text = "Hello";
			AssertEquals(6, calculator.GetTextSize(text, font, GraphicsUnit.Millimeter).Height);

			text = "Hello\nWorld";
			AssertEquals(11, calculator.GetTextSize(text, font, GraphicsUnit.Millimeter).Height);
		}

		public void TestGetTextSizeDoesNotThrowExceptionWhenNumberOfCharactersIsMoreThan32K()
		{
			var font = new Font("Arial", 12);
			var calculator = new TextSizeCalculator(font);

			var text = new string('A', GraphicsManager.MaxMeasurableStringLengthWithoutLineBreak);
			AssertNoExceptionThrown(() => calculator.GetTextSize(text, font, GraphicsUnit.Display, 2000));

			text += "A";
			AssertNoExceptionThrown(() => calculator.GetTextSize(text, font, GraphicsUnit.Display, 2000));
		}

		public void TestGetTextSizeDoesNotThrowExceptionWhenTextMoreThan32KAndContainsEscapeCharacter()
		{
			var escapeCharacters = new string[] { "\r", "\t", "\n", "\f", "\v" };  // parts of escape characters

			var font = new Font("Arial", 12);
			var calculator = new TextSizeCalculator(font);

			foreach (var ch in escapeCharacters)
			{
				string inputText = new string('A', GraphicsManager.MaxMeasurableStringLengthWithoutLineBreak);
				AssertNoExceptionThrown(() => calculator.GetTextSize(inputText, font, GraphicsUnit.Display));

				inputText += $"A{ch}";
				AssertEquals(32002, inputText.Length);
				Assert(inputText.EndsWith("A" + ch));
				AssertNoExceptionThrown(() => calculator.GetTextSize(inputText, font, GraphicsUnit.Display));
			}
		}

		public void TestStress()
		{
			long totalWidth = 0;
			using (var testFont = new Font("Arial", 12))
			{
				long width = new TextSizeCalculator(testFont).GetLengthInMillimeter("Mohsen");
				const long Repetitions = 100000;

				for (long i = 0; i < Repetitions; i++)
				{
					totalWidth += new TextSizeCalculator(testFont).GetLengthInMillimeter("Mohsen");
				}

				AssertEquals("This test is for testing Out of Memory", Repetitions * width, totalWidth);
			}
		}

		public void TestShouldUnscale()
		{
			var graphicsUnits = Enum.GetValues(typeof(GraphicsUnit)).Cast<GraphicsUnit>();

			foreach (var unit in graphicsUnits)
			{
				if (unit == GraphicsUnit.Display || unit == GraphicsUnit.Pixel)
				{
					AssertEquals(true, TextSizeCalculator.ShouldUnscale(unit));
				}
				else
				{
					AssertEquals(false, TextSizeCalculator.ShouldUnscale(unit));
				}
			}
		}
	}
}

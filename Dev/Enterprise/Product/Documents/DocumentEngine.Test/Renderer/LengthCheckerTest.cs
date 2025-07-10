using System;
using System.Drawing;
using System.Runtime.InteropServices;
using CargoWise.Common;
using Enterprise.DocumentEngine.Renderer;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class LengthCheckerTest : TestCase
	{
		public void TestNumberOfCharsThatFitInProposedWidth()
		{
			string inputText = "This is some input text to check the length of";
			var lengthChecker = new LengthChecker(new GraphicsManager().Graphics, new Font(new FontFamily("Arial"), 10), 1360000);

			AssertEquals("Characters that fit in the proposed width", 46, lengthChecker.NumberOfCharsThatFitInProposedWidth(inputText));
		}

		public void TestNumberOfCharsThatFitInProposedWidth_WithTextOver32kCharsAndNullChar()
		{
			var lengthChecker = new LengthChecker(new GraphicsManager().Graphics, new Font(new FontFamily("Arial"), 10), 1360000);

			string inputText = new string('A', GraphicsManager.MaxMeasurableStringLengthWithoutLineBreak - 1) + Convert.ToChar(0x0);
			AssertNoExceptionThrown(() => lengthChecker.NumberOfCharsThatFitInProposedWidth(inputText));

			inputText += "A";
			AssertNoExceptionThrown(() => lengthChecker.NumberOfCharsThatFitInProposedWidth(inputText));
		}

		public void TestNumberOfCharsThatFitInProposedWidth_WithTextOver32kCharsAndEscapeCharacter()
		{
			var lengthChecker = new LengthChecker(new GraphicsManager().Graphics, new Font(new FontFamily("Arial"), 10), 1360000);

			var escapeCharacters = new string[] { "\r", "\t", "\n", "\f", "\v" };  // parts of escape characters

			foreach (var ch in escapeCharacters)
			{
				string inputText = new string('A', GraphicsManager.MaxMeasurableStringLengthWithoutLineBreak);
				AssertNoExceptionThrown(() => lengthChecker.NumberOfCharsThatFitInProposedWidth(inputText));

				inputText += $"A{ch}";
				AssertEquals(32002, inputText.Length);
				Assert(inputText.EndsWith("A" + ch));
				AssertNoExceptionThrown(() => lengthChecker.NumberOfCharsThatFitInProposedWidth(inputText));
			}
		}

		public void TestGetWidthOfCharacters()
		{
			string characters = " (continued...)";
			int expectedCharactersLength = 88;
			var lengthChecker = new LengthChecker(new GraphicsManager().Graphics, new Font(new FontFamily("Arial"), 10), 0);

			var widthOfCharacters = lengthChecker.GetWidthOfCharacterSequence(characters);
			// Fonts can vary slightly between OS Versions, so a little allowance is made for the sake of tolerance(pass 1M as the delta parameter).
			AssertEquals(expectedCharactersLength, ControlDpiScalingHelper.UnscaleFromCurrentDpiX(widthOfCharacters / MacroValueProviders.ShrinkToFit.XlsWidthConversionFactor), 1M);
		}

		[DeveloperOnlyTest]
		public void TestGraphicsMeasureStringThrow_AGenericErrorOccurredInGDIPlusWillReporteErrorInformation()
		{
			ErrorReporter.Clear();

			var characters = new string(new char[] { (char)160, (char)770 });
			var lengthChecker = new LengthChecker(new GraphicsManager().Graphics, new Font(new FontFamily("Arial"), 10), 0);

			AssertExceptionThrown<ExternalException>(() => lengthChecker.GetWidthOfCharacterSequence(characters));

			AssertContains("Shoud Catch ExternalException", @"A generic error occurred in GDI+.

This is usually due to specific combination of characters in the string that GDI+ cannot handle.
The current solution is to remove illegal strings from the Excel content.
Please use Enterprise.DocumentEngine.StringExtensions.GetShortestInvalidCharacters() to process the string and retrieve the shortest invalid character combination with hex type.
Then add it to Enterprise.DocumentEngine.StringExtensions.InvalidHexStrings.

The string content in Excel is as follows:

 ̂
", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}

using System;
using System.Linq;
using Enterprise.DocumentVisualizer.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class JustifiedTextDrawHelperTest : TestCase
	{
		public void TestDrawJustifiedText_SingleLineText()
		{
			var width = 32F;
			var text = "this is my test for justified text draw helper";

			Func<string, float> measureText = str =>
			{
				Assert("should not contain space in a word", !string.IsNullOrWhiteSpace(str));
				return str.Length;
			};

			DrawText drawText = (words, widths, spaceBetweenWords) =>
			{
				var totalWordWidth = 0F;

				foreach (var word in words)
				{
					totalWordWidth += widths[word];
				}

				AssertEquals(spaceBetweenWords, (3F / 5F + 1F));
				AssertEquals("should only print word that fit in one line", 6, words.Length);
				AssertEquals("total length with space should be the same as line width.", width, totalWordWidth + spaceBetweenWords * (words.Length - 1));
				return false;
			};

			var helper = new JustifiedTextDrawHelper(measureText, drawText);
			helper.Draw(text, width, 1);
		}

		public void TestDrawJustifiedText_Paragraph_PrintMultipleLines()
		{
			var width = 32F;
			var text = "my name is Steven. I am a robot and live in a loop everyday of my life. aha, aha, aha, aha, bla bla bla....malfunctioning, sorry!";

			Func<string, float> measureText = str => str.Length;
			var lineCounter = 0;

			DrawText drawText = (words, widths, spaceBetweenWords) =>
			{
				var totalWordWidth = 0F;

				foreach (var word in words)
				{
					totalWordWidth += widths[word];
				}

				if (words.Last() == "sorry!")
				{
					AssertEquals("last line should have a normal space width.", 1F, spaceBetweenWords);
				}

				lineCounter++;
				return true;
			};

			var helper = new JustifiedTextDrawHelper(measureText, drawText);
			helper.Draw(text, width, 1);

			AssertEquals("number of lines printed is not correct", 5, lineCounter);
		}

		public void TestDrawJustifiedText_MultipleParagraphs_EmptyLineBetweenParagraphs()
		{
			var width = 32F;
			var text = "my name is Steven. I am a robot and live in a loop everyday of my life. aha, aha, aha, aha, bla bla bla....malfunctioning, sorry!"
				+ System.Environment.NewLine
				+ System.Environment.NewLine
				+ "my name is Steven NO.2. I am a robot and live in a loop everyday of my life. aha, aha, aha, aha, bla bla bla....malfunctioning, sorry!";

			Func<string, float> measureText = str => str.Length;

			var lineCounter = 0;
			var emptyLine = 0;

			DrawText drawText = (words, widths, spaceBetweenWords) =>
			{
				var totalWordWidth = 0F;

				foreach (var word in words)
				{
					totalWordWidth += widths[word];
				}

				if (words.Length == 1 && string.IsNullOrEmpty(words[0]))
				{
					emptyLine++;
				}

				lineCounter++;
				return true;
			};

			var helper = new JustifiedTextDrawHelper(measureText, drawText);
			helper.Draw(text, width, 1);

			AssertEquals("empty line in between 2 paragraph should be printed properly", 1, emptyLine);
			AssertEquals("correct number of line should be printed", 11, lineCounter);
		}
	}
}

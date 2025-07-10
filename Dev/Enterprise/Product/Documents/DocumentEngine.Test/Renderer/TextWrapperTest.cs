using System;
using System.Drawing;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class TextWrapperTest : TestCaseWithFactory
	{
		public void TestLastBreakSeparator()
		{
			string longWords = "Antidisestablishmentarianism\r\nHonorificabilitudinitatibus";
			var wrapper = new TextWrapper(longWords, 4000, Arial, 2);
			AssertEquals("wrapper.LastBreakSeparator for break on Line Feed", "\r\n", wrapper.LastBreakSeparator);
			AssertEquals("wrapper.LastBreakWasAtTheEndOfAWord for break on Line Feed", true, wrapper.LastBreakWasAtTheEndOfAWord);

			wrapper = new TextWrapper(longWords, 4000, Arial, 1);
			AssertEquals("wrapper.LastBreakSeparator for break in the middle of a word", "", wrapper.LastBreakSeparator);
			AssertEquals("wrapper.LastBreakWasAtTheEndOfAWord for break in the middle of a word", false, wrapper.LastBreakWasAtTheEndOfAWord);

			wrapper = new TextWrapper("Jimmy is a cold fish, so is Thomas the Blue.", 4000, Arial, 1);
			AssertEquals("wrapper.LastBreakSeparator for break between 2 words", " ", wrapper.LastBreakSeparator);
			AssertEquals("wrapper.LastBreakWasAtTheEndOfAWord for break between 2 words", true, wrapper.LastBreakWasAtTheEndOfAWord);

			wrapper = new TextWrapper("Jimmy is", 4000, Arial, 1);
			AssertEquals("wrapper.LastBreakSeparator for no break", "", wrapper.LastBreakSeparator);
			AssertEquals("wrapper.LastBreakWasAtTheEndOfAWord for no break", true, wrapper.LastBreakWasAtTheEndOfAWord);
		}

		public void TestNonWrappingTextWrapperDoesntGetNullInWrappedText()
		{
			string longWords = "Antidisestablishmentarianism\r\nHonorificabilitudinitatibus";
			var wrapper = new TextWrapper(longWords, 4000, Arial);

			AssertContains("Wrapper with 2 long words, one on each line where both words have to break", "Honor", string.Join("\r\n", wrapper.WrappedTextLines.ToArray()));
		}

		public void TestLongWordsInAShortCellDontExplode()
		{
			string longWord = "Antidisestablishmentarianism";
			var wrapper = new TextWrapper(longWord, 30, Arial);

			string longWordSplitAsOneCharacterPerLine = string.Join("\r\n", Array.ConvertAll(longWord.ToCharArray(), character => character.ToString()));
			AssertMultilineASCIIEquals("WrappedText with impossibly small column", longWordSplitAsOneCharacterPerLine, string.Join("\r\n", wrapper.WrappedTextLines.ToArray()));
		}

		public void TestWrapText()
		{
			var someText = "Something really really long that goees over onto the next page would be great at this point cause otherwise it's going to look really silly.";

			var textWrapper = new TextWrapper(someText, 10000, Arial, 1);

			AssertEquals("Number of lines wrapped", 1, textWrapper.WrappedTextLines.Count);
			AssertEquals("Text on wrapped line 1 of 1", "Something really really long that goees over", textWrapper.WrappedTextLines[0]);

			AssertEquals("Remaining text from original text", "onto the next page would be great at this point cause otherwise it's going to look really silly.", textWrapper.RemainderAfterMaxLines);
		}

		StringBuilder result;
		readonly Font Arial = new Font("Arial", 10);

		public void TestNumberOfIterations()
		{
			result = new StringBuilder();

			AssertLineResult("RealText1", RealText1);
			AssertLineResult("RealText2", RealText2);
			AssertLineResult("RealText3", RealText3);
			AssertLineResult("RealText4", RealText4);
			AssertLineResult("RealText5", RealText5);
			AssertLineResult("SomeOtherText1", SomeOtherText1);
			AssertLineResult("SomeOtherText2", SomeOtherText2);
			AssertMultilineASCIIEquals(result.ToString(), @"
RealText1   Length: 10000   got 4 lines.
RealText1   Length: 4000   got 11 lines.

RealText2   Length: 10000   got 3 lines.
RealText2   Length: 4000   got 8 lines.

RealText3   Length: 10000   got 4 lines.
RealText3   Length: 4000   got 10 lines.

RealText4   Length: 10000   got 3 lines.
RealText4   Length: 4000   got 9 lines.

RealText5   Length: 10000   got 3 lines.
RealText5   Length: 4000   got 6 lines.

SomeOtherText1   Length: 10000   got 2 lines.
SomeOtherText1   Length: 4000   got 4 lines.

SomeOtherText2   Length: 10000   got 3 lines.
SomeOtherText2   Length: 4000   got 6 lines.
", result.ToString());
		}

		void AssertLineResult(string textName, string text)
		{
			result.AppendLine();
			AssertLineResult(textName, text, 10000);
			AssertLineResult(textName, text, 4000);
		}

		void AssertLineResult(string textName, string text, int length)
		{
			TextWrapper wrapper = new TextWrapper(text, length, Arial);
			result.AppendLine(textName + "   Length: " + length.ToString() + "   got " + wrapper.WrappedTextLines.Count.ToString() + " lines.");
		}

		const string RealText1 = @"EDS will retain responsibility of end-user computing and centralised computing services that the outsourcing firm has controlled for the past decade.";
		const string RealText2 = @"The original contract was due to expire in June 2010 but new the deadline will be June 2012 under the latest extension.";
		const string RealText3 = @"The ATO end-user computing environment consists of 28,000 desktops and 4700 laptops, 1776 laser printers and 370 servers. ";
		const string RealText4 = @"The tender was released earlier this year and was also contested by CSC, Kaz Group, Lockheed Martin and Unisys.";
		const string RealText5 = @"EDS had to fend off Lockheed Martin, CSC and IBM to bag the centralised computing deal.";
		const string SomeOtherText1 = @"WWWWWWWWWWWWWWWIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIII";
		const string SomeOtherText2 = @"IIIIIIIIIIIIIIIIIIIIIIIIIIIWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW";

		public void TestGetLines()
		{
			var wrapper = new TextWrapper("         \ntest\n\ntest              \n", 1360, new Font("Arial", 10));

			AssertEquals(5, wrapper.WrappedTextLines.Count);
			AssertEquals("", wrapper.WrappedTextLines[0]);
			AssertEquals("test", wrapper.WrappedTextLines[1]);
			AssertEquals("", wrapper.WrappedTextLines[2]);
			AssertEquals("test", wrapper.WrappedTextLines[3]);
			AssertEquals("", wrapper.WrappedTextLines[4]);
			AssertEquals("THAILAND", new TextWrapper(@"                        MADE IN THAILAND
", 2720, new Font("Arial", 10)).WrappedTextLines[1]);
		}

		public void TestGetLinesCrashesOnCertainStrings()
		{
			AssertEquals(4, new TextWrapper("FOOTWEAR                           SUVA, FIJI                         C/NO 1-22                          MADE IN THAILAND", 7072, new Font("Arial", 10)).WrappedTextLines.Count);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWrappingInExcel()
		{
			string xLSFileName = UnitTestingConstants.TestFilesDir + "WrapperTester.xls";
			using (ExcelInterface @interface = new ExcelInterface())
			{
				@interface.LoadExcelFile(xLSFileName);
				string longText = "quick brown fox jumps over the lazy dog quick brown fox jumps over the lazy dog quick brown fox jumps over the lazy dog quick brown fox jumps over the lazy dog quick brown fox jumps over the lazy dog quick brown fox jumps over the lazy dog quick brown fox jumps over the lazy dog";

				TextWrapper testWrapper = new TextWrapper(longText, @interface.WorkSheets[0].GetCellWidth(0, 2), @interface.WorkSheets[0].GetCellFont(0, 2));
				for (int i = 0; i < 4; i++)
				{
					@interface.WorkSheets[0][i, 2] = testWrapper.WrappedTextLines[i];
				}

				testWrapper = new TextWrapper(longText, @interface.WorkSheets[0].GetCellWidth(6, 2), @interface.WorkSheets[0].GetCellFont(6, 2));
				for (int i = 6; i < 12; i++)
				{
					@interface.WorkSheets[0][i, 2] = testWrapper.WrappedTextLines[i - 6];
				}

				testWrapper = new TextWrapper(longText, @interface.WorkSheets[0].GetCellWidth(14, 2), @interface.WorkSheets[0].GetCellFont(14, 2));
				for (int i = 14; i < 26; i++)
				{
					@interface.WorkSheets[0][i, 2] = testWrapper.WrappedTextLines[i - 14];
				}

				AssertEquals("quick brown fox jumps over the lazy dog quick brown fox jumps over the", @interface.WorkSheets[0][0, 2]);
				AssertEquals("lazy dog quick brown fox jumps over the lazy dog quick brown fox jumps", @interface.WorkSheets[0][1, 2]);
				AssertEquals("over the lazy dog quick brown fox jumps over the lazy dog quick brown fox", @interface.WorkSheets[0][2, 2]);
				AssertEquals("jumps over the lazy dog quick brown fox jumps over the lazy dog", @interface.WorkSheets[0][3, 2]);

				AssertEquals("quick brown fox jumps over the lazy dog quick", @interface.WorkSheets[0][6, 2]);
				AssertEquals("brown fox jumps over the lazy dog quick brown", @interface.WorkSheets[0][7, 2]);
				AssertEquals("fox jumps over the lazy dog quick brown fox", @interface.WorkSheets[0][8, 2]);
				AssertEquals("jumps over the lazy dog quick brown fox jumps", @interface.WorkSheets[0][9, 2]);
				AssertEquals("over the lazy dog quick brown fox jumps over", @interface.WorkSheets[0][10, 2]);
				AssertEquals("the lazy dog quick brown fox jumps over the", @interface.WorkSheets[0][11, 2]);

				AssertEquals("quick brown fox", @interface.WorkSheets[0][14, 2]);
				AssertEquals("jumps over the", @interface.WorkSheets[0][15, 2]);
				AssertEquals("lazy dog quick", @interface.WorkSheets[0][16, 2]);
				AssertEquals("brown fox", @interface.WorkSheets[0][17, 2]);
				AssertEquals("jumps over the", @interface.WorkSheets[0][18, 2]);
				AssertEquals("lazy dog quick", @interface.WorkSheets[0][19, 2]);
				AssertEquals("brown fox", @interface.WorkSheets[0][20, 2]);
				AssertEquals("jumps over the", @interface.WorkSheets[0][21, 2]);
				AssertEquals("lazy dog quick", @interface.WorkSheets[0][22, 2]);
				AssertEquals("brown fox", @interface.WorkSheets[0][23, 2]);
				AssertEquals("jumps over the", @interface.WorkSheets[0][24, 2]);
				AssertEquals("lazy dog quick", @interface.WorkSheets[0][25, 2]);
			}
		}
	}
}

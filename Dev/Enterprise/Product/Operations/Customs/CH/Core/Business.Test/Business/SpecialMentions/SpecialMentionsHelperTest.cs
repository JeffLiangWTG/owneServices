using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

public class SpecialMentionsHelperTest : TestCase
{
	public void TestFormatText()
	{
		CombineAssertions(() =>
		{
			string escape(string str)
			{
				return str.Replace("\r", @"\r").Replace("\n", @"\n");
			}

			const string str60 = "123456789 123456789 123456789 123456789 123456789 123456789 ";

			AssertEquals("empty", escape(""), escape(SpecialMentionsHelper.FormatText("")));
			AssertEquals(escape($"{str60}12345 789"), escape(SpecialMentionsHelper.FormatText($"{str60}12345 789")));
			AssertEquals(escape($"{str60}12345 7890"), escape(SpecialMentionsHelper.FormatText($"{str60}12345 7890")));
			AssertEquals(escape($"{str60}12345\r\n78901"), escape(SpecialMentionsHelper.FormatText($"{str60}12345 78901")));
			AssertEquals(escape($"{str60}12345\r\n789012"), escape(SpecialMentionsHelper.FormatText($"{str60}12345 789012")));
			AssertEquals(escape($"abc\r\n{str60}12345 789"), escape(SpecialMentionsHelper.FormatText($"abc\r\n{str60}12345 789")));
			AssertEquals(escape($"abc\r\n{str60}12345 7890"), escape(SpecialMentionsHelper.FormatText($"abc\r\n{str60}12345 7890")));
			AssertEquals(escape($"abc\r\n{str60}12345\r\n78901"), escape(SpecialMentionsHelper.FormatText($"abc\r\n{str60}12345 78901")));
			AssertEquals(escape($"abc\r\n{str60}12345\r\n789012"), escape(SpecialMentionsHelper.FormatText($"abc\r\n{str60}12345 789012")));
		});
	}

	public void TestCountLines()
	{
		CombineAssertions(() =>
		{
			AssertEquals("empty text", 0, SpecialMentionsHelper.CountLines(""));
			AssertEquals("2 lines", 2, SpecialMentionsHelper.CountLines("aaa\r\nbbb\r\n"));
			AssertEquals("no CRLF at end", 2, SpecialMentionsHelper.CountLines("aaa\r\nbbb"));
			AssertEquals("empty line", 3, SpecialMentionsHelper.CountLines("aaa\r\n\r\nbbb\r\n"));
			AssertEquals("empty line CR only", 3, SpecialMentionsHelper.CountLines("aaa\r\rbbb\r"));
			AssertEquals("empty line LF only", 3, SpecialMentionsHelper.CountLines("aaa\n\nbbb\n"));
			AssertEquals("CR only", 2, SpecialMentionsHelper.CountLines("aaa\rbbb\r"));
			AssertEquals("LF only", 2, SpecialMentionsHelper.CountLines("aaa\nbbb\n"));
		});
	}

	public void TestNodeType()
	{
		AssertEquals("Customs Special Mentions", SpecialMentionsHelper.NoteType.Description);
	}

	public void TestSplitIntoLines()
	{
		CombineAssertions(() =>
		{
			AssertArrayEqualsByElements("With leading CRLF", new[] { new ZString("Line 1"), new ZString("Line 2") }, SpecialMentionsHelper.SplitIntoLines("\r\nLine 1\r\nLine 2"));
			AssertArrayEqualsByElements("With trailing CRLF", new[] { new ZString("Line 1"), new ZString("Line 2") }, SpecialMentionsHelper.SplitIntoLines("Line 1\r\nLine 2\r\n"));
			AssertArrayEqualsByElements("With no trailing CRLF", new[] { new ZString("Line 1"), new ZString("Line 2") }, SpecialMentionsHelper.SplitIntoLines("Line 1\r\nLine 2"));
			AssertArrayEqualsByElements("Empty text", Array.Empty<ZString>(), SpecialMentionsHelper.SplitIntoLines(""));
			AssertArrayEqualsByElements("With empty line", new[] { new ZString("Line 1"), ZString.Empty, new ZString("Line 2") }, SpecialMentionsHelper.SplitIntoLines("Line 1\r\n\r\nLine 2\r\n"));
		});
	}
}

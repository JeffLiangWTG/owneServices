using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Messaging.Testing;

sealed class ExtensionTest : TestCase
{
	public void TestSplitByNewLine()
	{
		CombineAssertions(() =>
		{
			AssertSplitWithDifferentOptions(
				content: "ABCDEFGHI",
				expectedAllLines: new ZString[] { "ABCDEFGHI" }
			);

			AssertSplitWithDifferentOptions(
				content: "ABCDEFGHI\r\n123456",
				expectedAllLines: new ZString[] { "ABCDEFGHI", "123456" }
			);

			AssertSplitWithDifferentOptions(
				content: "ABCDEFGHI\n123456\r\n!@#$",
				expectedAllLines: new ZString[] { "ABCDEFGHI", "123456", "!@#$" }
			);

			AssertSplitWithDifferentOptions(
				content: "",
				expectedAllLines: new ZString[] { "" },
				expectedRemovingEmptyLinesIfDifferent: Array.Empty<ZString>()
			);

			AssertSplitWithDifferentOptions(
				content: "\n",
				expectedAllLines: new ZString[] { "", "" },
				expectedRemovingEmptyLinesIfDifferent: Array.Empty<ZString>()
			);

			AssertSplitWithDifferentOptions(
				content: "\nABCDEFGHI\n\r\n123456\n!@#$\r\n",
				expectedAllLines: new ZString[] { "", "ABCDEFGHI", "", "123456", "!@#$", "" },
				expectedRemovingEmptyLinesIfDifferent: new ZString[] { "ABCDEFGHI", "123456", "!@#$" }
			);
		});
	}

	void AssertSplitWithDifferentOptions(ZString content, ZString[] expectedAllLines, ZString[] expectedRemovingEmptyLinesIfDifferent = null)
	{
		var assertMessagePrefix = $"Source String [{content.Replace("\r", "\\r").Replace("\n", "\\n")}]";
		AssertSplit(assertMessagePrefix + " (keeping all lines)", expectedAllLines, content.SplitByNewLine());
		AssertSplit(assertMessagePrefix + " (removing empty lines)", expectedRemovingEmptyLinesIfDifferent ?? expectedAllLines, content.SplitByNewLine(StringSplitOptions.RemoveEmptyEntries));
	}

	void AssertSplit(string assertMessage, ZString[] expectedLines, IEnumerable<ZString> actualLines)
	{
		AssertEquals($"Count - {assertMessage}", expectedLines.Length, actualLines.Count());

		if (expectedLines.Length == actualLines.Count())
		{
			for (int i = 0; i < expectedLines.Length; i++)
			{
				AssertEquals($"Line({i}) [{assertMessage}]", expectedLines[i], actualLines.ElementAt(i));
			}
		}
	}

	public void TestParseToDateTimeWithFormat()
	{
		AssertEquals(new ZDateTime(2019, 01, 01), Extension.ParseToDateTimeWithFormat("010119", "ddMMyy"));
		AssertEquals(new ZDateTime(2019, 11, 01), Extension.ParseToDateTimeWithFormat("011119", "ddMMyy"));
		AssertEquals(new ZDateTime(2019, 9, 24, 8, 23, 43), Extension.ParseToDateTimeWithFormat("Tue Sep 24 08:23:43 2019", "ddd MMM dd HH:mm:ss yyyy"));
		AssertEquals(new ZDateTime(2019, 01, 01, 10, 15, 00), Extension.ParseToDateTimeWithFormat("01/01/2019 10:15", "dd/MM/yyyy HH:mm"));
		AssertEquals(new ZDateTime(2019, 01, 01, 10, 15, 00), Extension.ParseToDateTimeWithFormat("01/01/19  10:15", "dd/MM/yy  HH:mm"));
		AssertEquals(new ZDateTime(2019, 01, 01, 10, 15, 00), Extension.ParseToDateTimeWithFormat("01/01/19 10:15", "dd/MM/yy HH:mm"));
		AssertExceptionThrown<FormatException>(() =>
		{
			Extension.ParseToDateTimeWithFormat("01/01/19 10:XX", "dd/MM/yy HH:mm");
			Extension.ParseToDateTimeWithFormat("", "dd/MM/yy HH:mm");
		});
	}

	public void TestSubstringAndTrim()
	{
		AssertEquals("THIS", Extension.SubStringAndTrim("THIS IS A STRING", 0, 4));
		AssertEquals("S", Extension.SubStringAndTrim("THIS IS A STRING", 3, 2));
		AssertEquals("", Extension.SubStringAndTrim("THIS IS A STRING", 4, 1));
		AssertExceptionThrown<ArgumentException>(() => { Extension.SubStringAndTrim("THIS IS A STRING", 100, 1); });
	}

	public void TestGetElementAtAndTrim()
	{
		var element = new ZString[] { " ", "one ", "two" };
		AssertEquals("", Extension.GetElementAtAndTrim(element, 0));
		AssertEquals("one", Extension.GetElementAtAndTrim(element, 1));
		AssertEquals("two", Extension.GetElementAtAndTrim(element, 2));
		AssertExceptionThrown<ArgumentOutOfRangeException>(() => { Extension.GetElementAtAndTrim(element, 3); });
	}

	public void TestGetElementAtAndTrimsafe()
	{
		var element = new ZString[] { " ", "one ", "two" };
		AssertEquals("", Extension.GetElementAtAndTrimSafe(element, 0));
		AssertEquals("one", Extension.GetElementAtAndTrimSafe(element, 1));
		AssertEquals("two", Extension.GetElementAtAndTrimSafe(element, 2));
		AssertEquals("", Extension.GetElementAtAndTrimSafe(element, 3));
	}

	public void TestGetZIntOrNullIfEmpty()
	{
		AssertNull("Empty string is not an integer value", Extension.GetZIntOrNullIfEmpty(""));
		AssertEquals("It is an integer value", 1, Extension.GetZIntOrNullIfEmpty("1"));
		AssertEquals("It is an integer value", 1000, Extension.GetZIntOrNullIfEmpty("1000"));
		AssertExceptionThrown<ArgumentOutOfRangeException>("When parameter is not a numeric string, an exception should be raised", () =>
		{
			Extension.GetZIntOrNullIfEmpty("X");
		});
	}

	public void TestGetZDecimalOrNullIfEmpty()
	{
		AssertNull("Empty string is not a decimal value", Extension.GetZDecimalOrNullIfEmpty(""));
		AssertEquals("It is a decimal value", 1m, Extension.GetZDecimalOrNullIfEmpty("1"));
		AssertEquals("It is a decimal value", 1000.1m, Extension.GetZDecimalOrNullIfEmpty("1000.1"));
		AssertExceptionThrown<FormatException>("When parameter is not a numeric string, an exception should be raised", () =>
		{
			Extension.GetZDecimalOrNullIfEmpty("X");
		});
	}

	public void TestParseToDateDDMMYYYYSafe()
	{
		AssertEquals(new ZDate(2019, 01, 01), Extension.ParseToDateDDMMYYYYSafe("01012019"));
		AssertEquals(new ZDate(2019, 12, 31), Extension.ParseToDateDDMMYYYYSafe("31122019"));
		AssertEquals(ZDate.Empty, Extension.ParseToDateDDMMYYYYSafe(ZString.Empty));
		AssertExceptionThrown<FormatException>(() =>
		{
			Extension.ParseToDateDDMMYYYYSafe("XXXX");
			Extension.ParseToDateDDMMYYYYSafe("20193112");
		});
	}

	public void TestGetCountOrNullIfEmpty()
	{
		List<ZString> stringList = new List<ZString>();
		List<ZInt> intList = new List<ZInt>();
		AssertNull("Empty list must return null", Extension.GetCountOrNullIfEmpty(stringList));
		AssertNull("Empty list must return null", Extension.GetCountOrNullIfEmpty(intList));

		stringList.Add("String");
		intList.Add(1);
		intList.Add(2);

		AssertEquals("Not empty list should return the number of items", 1, Extension.GetCountOrNullIfEmpty(stringList));
		AssertEquals("Not empty list should return the number of items", 2, Extension.GetCountOrNullIfEmpty(intList));

		AssertExceptionThrown<ArgumentNullException>("When parameter is null, an exception should be raised", () =>
		{
			Extension.GetCountOrNullIfEmpty((null as IEnumerable<ZString>));
		});
	}
}

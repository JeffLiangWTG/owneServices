using System;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class StringExtensionsTest : NUnit.Framework.TestCase
	{
		public void TestEscapeUnEscapeJScript()
		{
			const string input = "\b\t\n\v\r\"\'\\ .barry";
			const string output = @"\b\t\n\v\r\""'\\ .barry";

			AssertEquals(output, input.EscapeForJScript());
			AssertEquals(input, output.UnEscapeForJScript());
		}

		public void TestTryGetEnum()
		{
			TestEnum value;

			AssertEquals("\"Real1\".TryGetEnum<TestEnum>(out value)", true, "Real1".TryGetEnum<TestEnum>(out value));
			AssertEquals("'TryGetEnum' on \"Real1\"", value, TestEnum.Real1);

			AssertEquals("\"Real3\".TryGetEnum<TestEnum>(out value)", true, "Real3".TryGetEnum<TestEnum>(out value));
			AssertEquals("'TryGetEnum' on \"Real3\"", value, TestEnum.Real3);

			AssertEquals("\"Fake\".TryGetEnum<TestEnum>(out value)", false, "Fake".TryGetEnum<TestEnum>(out value));
			AssertEquals("'TryGetEnum' on \"Fake\"", value, TestEnum.Real1);

			AssertEquals("\"Real2\".TryGetEnum<TestEnum>(out value)", true, "Real2".TryGetEnum<TestEnum>(out value));
			AssertEquals("'TryGetEnum' on \"Real2\"", value, TestEnum.Real2);

			string nullString = null;
			AssertEquals("nullString.TryGetEnum<TestEnum>(out value)", false, nullString.TryGetEnum<TestEnum>(out value));
			AssertEquals("'TryGetEnum' on nullString", value, TestEnum.Real1);
		}

		enum TestEnum { Real1, Real2, Real3 }

		public void TestRemoveBetween()
		{
			string testValue = "Beans Meanz Heinz";
			AssertEquals("Beans Heinz", testValue.RemoveBetween(" M", "z "));
		}

		public void TestShrinkToMaxLength()
		{
			AssertEquals("\"Freddy\".ShrinkToMaxLength(10)", "Freddy", "Freddy".ShrinkToMaxLength(10));
			AssertEquals("\"Freddy Jr.\".ShrinkToMaxLength(10)", "Freddy Jr.", "Freddy Jr.".ShrinkToMaxLength(10));
			AssertEquals("\"Freddy Kruger\".ShrinkToMaxLength(10)", "Fre ... er", "Freddy Kruger".ShrinkToMaxLength(10));
			AssertEquals("\"Freddy Kruger Eats Trees.\".ShrinkToMaxLength(25)", "Freddy Kruger Eats Trees.", "Freddy Kruger Eats Trees.".ShrinkToMaxLength(25));
			AssertEquals("\"Freddy Kruger Eats Big Trees.\".ShrinkToMaxLength(25)", "Freddy Kruger E ... rees.", "Freddy Kruger Eats Big Trees.".ShrinkToMaxLength(25));
			AssertExceptionThrown(typeof(ArgumentException), delegate
			{ "Freddy Kruger Eats Trees.".ShrinkToMaxLength(6); });
		}

		public void TestContainsNumber()
		{
			AssertEquals("\"Fred\".ContainsNumber()", false, "Fred".ContainsNumber());
			AssertEquals("\"Fred1\".ContainsNumber()", true, "Fred1".ContainsNumber());
			AssertEquals("\"Fr2ed\".ContainsNumber()", true, "Fr2ed".ContainsNumber());
			AssertEquals("\"3Fred\".ContainsNumber()", true, "3Fred".ContainsNumber());
			AssertEquals("\"Bob\".ContainsNumber()", false, "Bob".ContainsNumber());
			AssertEquals("\"\".ContainsNumber()", false, "".ContainsNumber());
			AssertEquals("\"0\".ContainsNumber()", true, "0".ContainsNumber());
			AssertEquals("\"9\".ContainsNumber()", true, "9".ContainsNumber());
		}

		public void ContainsInnerWhitespace()
		{
			AssertEquals(false, "".ContainsInnerWhitespace());
			AssertEquals(false, " ".ContainsInnerWhitespace());
			AssertEquals(false, "bla".ContainsInnerWhitespace());
			AssertEquals(false, " bla".ContainsInnerWhitespace());
			AssertEquals(false, "bla ".ContainsInnerWhitespace());
			AssertEquals(false, " bla ".ContainsInnerWhitespace());
			AssertEquals(true, "bla bla".ContainsInnerWhitespace());
		}

		public void TestEscapeQuotes()
		{
			AssertEquals("\\\"", "\"".EscapeQuotes());
			AssertEquals("\\\"", "\\\"".EscapeQuotes());
			AssertEquals("\\\\\"", "\\\\\"".EscapeQuotes());
		}

		public void TestEscapeAngleBrackets()
		{
			AssertEquals("\\", "\\".EscapeAngleBrackets());
			AssertEquals("\\\\", "\\\\".EscapeAngleBrackets());
			AssertEquals("\\<\\>", "<>".EscapeAngleBrackets());
			AssertEquals("\\\\<\\\\>", "\\<\\>".EscapeAngleBrackets());
			AssertEquals("\\\\\\<\\\\\\>", "\\\\<\\\\>".EscapeAngleBrackets());
		}

		public void TestEscapeBackslashes()
		{
			AssertEquals(@"\\\\", @"\\".EscapeBackslashes());
			AssertEquals(@"\\blah\\", @"\blah\".EscapeBackslashes());
			AssertEquals(@"bl\\\\\\\\\\ah\\\\", @"bl\\\\\ah\\".EscapeBackslashes());
			AssertEquals(@"\<\\\""\>", @"\<\\""\>".EscapeBackslashes());
		}

		public void TestUnEscapeBackshashes()
		{
			AssertEquals(@"\", @"\".UnEscapeBackslashes());
			AssertEquals(@"\blah\", @"\blah\\".UnEscapeBackslashes());
			AssertEquals(@"bl\\\\ah\\", @"bl\\\\\\\\ah\\\\".UnEscapeBackslashes());
		}

		public void TestEscapeDollarSign()
		{
			AssertEquals(@"$$", @"$".EscapeDollarSignForRegexReplace());
			AssertEquals(@"$$150", @"$150".EscapeDollarSignForRegexReplace());
			AssertEquals(@"$$$$150", @"$$150".EscapeDollarSignForRegexReplace());
			AssertEquals(@"Testing $$150", @"Testing $150".EscapeDollarSignForRegexReplace());
		}

		public void TestParseDataSource()
		{
			string cellValue = null;
			AssertNull("Should be null when string is null", cellValue.ParseDataSource());
			cellValue = "";
			AssertNull("Should be null when string is empty", cellValue.ParseDataSource());
			cellValue = "BOB";
			AssertNull("Should be null when string is invalide", cellValue.ParseDataSource());
			cellValue = "BOB=Builder";
			AssertNull("Should be null when string is invalide", cellValue.ParseDataSource());
			cellValue = "Data:BOB=Builder";
			var dataSource = cellValue.ParseDataSource();
			AssertEquals("dataSource.TableName", "BOB", dataSource.TableName);
			AssertEquals("dataSource.SelectStatement", "Builder", dataSource.SelectStatement);
		}

		public void TestStripOutMostAngleBrackets()
		{
			var value = "<asdf>";
			AssertEquals("asdf", value.StripOutMostAngleBrackets());
			value = "<>";
			AssertExceptionThrown<ArgumentException>(() => value.StripOutMostAngleBrackets());
			value = "WhatEver";
			AssertExceptionThrown<ArgumentException>(() => value.StripOutMostAngleBrackets());
			value = "<adf";
			AssertExceptionThrown<ArgumentException>(() => value.StripOutMostAngleBrackets());
			value = "asdf>";
			AssertExceptionThrown<ArgumentException>(() => value.StripOutMostAngleBrackets());
		}
	}
}

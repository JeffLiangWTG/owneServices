using System;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class OCsvLineTest : TestCase
	{
		public void TestCsvLine()
		{
			string[] test1 = new OCsvLine("x,y,z").FieldValues;
			AssertEquals("x", test1[0]);
			AssertEquals("y", test1[1]);
			AssertEquals("z", test1[2]);
			string[] test2 = new OCsvLine("\"x\", y ,\"z\"").FieldValues;
			AssertEquals("x", test2[0]);
			AssertEquals("y", test2[1]);
			AssertEquals("z", test2[2]);
			string[] test3 = new OCsvLine("\"x\", y , \"z").FieldValues;
			AssertEquals("x", test3[0]);
			AssertEquals("y", test3[1]);
			AssertEquals("z", test3[2]);
			string[] test4 = new OCsvLine("splat splat").FieldValues;
			AssertEquals("splat splat", test4[0]);
			string[] test5 = new OCsvLine("splat splat,").FieldValues;
			AssertEquals("splat splat", test5[0]);
			AssertEquals("", test5[1]);
			string[] test6 = new OCsvLine("splat splat,\"").FieldValues;
			AssertEquals("splat splat", test6[0]);
			AssertEquals("", test6[1]);
			string[] test7 = new OCsvLine(",splat").FieldValues;
			AssertEquals("", test7[0]);
			AssertEquals("splat", test7[1]);
			string[] test8 = new OCsvLine(",\"s,plat\"").FieldValues;
			AssertEquals("", test8[0]);
			AssertEquals("s,plat", test8[1]);
			string[] test9 = new OCsvLine(",,,").FieldValues;
			AssertEquals("", test9[0]);
			AssertEquals("", test9[1]);
			AssertEquals("", test9[2]);
			string[] test10 = new OCsvLine("splat splat,,\r").FieldValues;
			AssertEquals("splat splat", test10[0]);
			AssertEquals("", test10[2]);
		}

		public void TestCsvLineWithNoQuotes()
		{
			OCsvLine test1 = new OCsvLine("x,y,z");
			AssertEquals(false, test1.IncludeQuotes[0]);
			AssertEquals(false, test1.IncludeQuotes[1]);
			AssertEquals(false, test1.IncludeQuotes[2]);
			AssertEquals("x,y,z", test1.ToString());
			OCsvLine test2 = new OCsvLine("\"x\",y,\"z\"");
			AssertEquals(true, test2.IncludeQuotes[0]);
			AssertEquals(false, test2.IncludeQuotes[1]);
			AssertEquals(true, test2.IncludeQuotes[2]);
			AssertEquals("\"x\",y,\"z\"", test2.ToString());
			OCsvLine test3 = new OCsvLine(System.Environment.NewLine + "col1,col2" + System.Environment.NewLine + ",col3" + System.Environment.NewLine);
			AssertEquals(false, test3.IncludeQuotes[0]);
			AssertEquals(false, test3.IncludeQuotes[1]);
			AssertEquals(false, test3.IncludeQuotes[2]);
			AssertEquals(3, test3.FieldValues.Length);
			AssertEquals("col1", test3.FieldValues[0]);
			AssertEquals("col2", test3.FieldValues[1]);
			AssertEquals("col3", test3.FieldValues[2]);
		}

		public void TestCsvLineWithFieldValuesAndOtherDelimiters()
		{
			var test1 = new OCsvLine(new string[] { "x", "y", "z" }, new bool[] { false, false, false }, ';');
			AssertEquals("x;y;z", test1.ToString());
		}

		public void TestAutoDetectDelimiter()
		{
			AssertEquals("The Delimiter should be comma", ',', OCsvLine.AutoDetectDelimiter("a|b"));
			AssertEquals("The Delimiter should be comma", ',', OCsvLine.AutoDetectDelimiter("a, b, c| ~a, ~b| c, d, e"));
			AssertEquals("The Delimiter should be pipe", '|', OCsvLine.AutoDetectDelimiter("a, b, |c| ~a, ~b| c, d, e|"));
			AssertEquals("The Delimiter should be tilda", '~', OCsvLine.AutoDetectDelimiter("~~a, b, c~~a,~b|c,~d, e|"));
		}

		public void TestToString()
		{
			OCsvLine line = new OCsvLine(new string[] { "1", "xyz", "" });
			AssertEquals("\"1\",\"xyz\",\"\"", line.ToString());
			OCsvLine line2 = new OCsvLine("\"1\",\"xyz\",\r\n\r\n");
			AssertEquals("\"1\",\"xyz\",\"\"", line2.ToString());
			OCsvLine line3 = new OCsvLine(new string[] { "1\"2" }, new bool[] { true });
			AssertEquals("\"1\"\"2\"", line3.ToString());
		}

		public void TestToStringWithNewLine()
		{
			OCsvLine line = new OCsvLine(new string[] { "1", "xyz", "" });
			AssertEquals("\"1\",\"xyz\",\"\"" + '\n', line.ToStringWithNewLine());
		}

		public void TestDoubleQuotes()
		{
			OCsvLine test1 = new OCsvLine("\"ab\"\"\",splat");
			AssertEquals(true, test1.IncludeQuotes[0]);
			AssertEquals(false, test1.IncludeQuotes[1]);
			AssertEquals("ab\"", test1.FieldValues[0]);
			OCsvLine test2 = new OCsvLine("\"\"\"ab\",splat");
			AssertEquals(true, test2.IncludeQuotes[0]);
			AssertEquals(false, test2.IncludeQuotes[1]);
			AssertEquals("\"ab", test2.FieldValues[0]);
			OCsvLine test3 = new OCsvLine("\"ab\"\"\"\"ab\",splat");
			AssertEquals(true, test3.IncludeQuotes[0]);
			AssertEquals(false, test3.IncludeQuotes[1]);
			AssertEquals("ab\"\"ab", test3.FieldValues[0]);
			OCsvLine test4 = new OCsvLine("ab\"\"ab,splat");
			AssertEquals(false, test4.IncludeQuotes[0]);
			AssertEquals(false, test4.IncludeQuotes[1]);
			AssertEquals("ab\"\"ab", test4.FieldValues[0]);
			OCsvLine test5 = new OCsvLine("\"col1.row1" + System.Environment.NewLine + "col1.row2\",\"splat\"");
			AssertEquals(true, test5.IncludeQuotes[0]);
			AssertEquals(true, test5.IncludeQuotes[1]);
			AssertEquals("col1.row1" + System.Environment.NewLine + "col1.row2", test5.FieldValues[0]);
			OCsvLine test6 = new OCsvLine(System.Environment.NewLine + ",\"col1\"," + System.Environment.NewLine + ",\"col2\"," + System.Environment.NewLine);
			AssertEquals(true, test6.IncludeQuotes[1]);
			AssertEquals(true, test6.IncludeQuotes[3]);
			AssertEquals(5, test6.FieldValues.Length);
			AssertEquals("", test6.FieldValues[0]);
			AssertEquals("", test6.FieldValues[2]);
			AssertEquals("", test6.FieldValues[4]);
			OCsvLine test7 = new OCsvLine(System.Environment.NewLine + "\"col1\"" + System.Environment.NewLine + "," + System.Environment.NewLine + "\"col2\"" + System.Environment.NewLine);
			AssertEquals(true, test7.IncludeQuotes[0]);
			AssertEquals(true, test7.IncludeQuotes[1]);
			AssertEquals(2, test7.FieldValues.Length);
			AssertEquals("col1", test7.FieldValues[0]);
			AssertEquals("col2", test7.FieldValues[1]);
			OCsvLine test8 = new OCsvLine("\"col1\",\"col2.1,col2.2\",col3");
			AssertEquals(true, test8.IncludeQuotes[0]);
			AssertEquals(true, test8.IncludeQuotes[1]);
			AssertEquals(false, test8.IncludeQuotes[2]);
			AssertEquals(3, test8.FieldValues.Length);
			AssertEquals("col1", test8.FieldValues[0]);
			AssertEquals("col2.1,col2.2", test8.FieldValues[1]);
			AssertEquals("col3", test8.FieldValues[2]);
			OCsvLine test9 = new OCsvLine("\" col1 \",\"col2.1 , col2.2\",col3");
			AssertEquals(true, test9.IncludeQuotes[0]);
			AssertEquals(true, test9.IncludeQuotes[1]);
			AssertEquals(false, test9.IncludeQuotes[2]);
			AssertEquals(3, test9.FieldValues.Length);
			AssertEquals(" col1 ", test9.FieldValues[0]);
			AssertEquals("col2.1 , col2.2", test9.FieldValues[1]);
			AssertEquals("col3", test9.FieldValues[2]);
		}

		public void TestCsvLineWithOtherDelimiters()
		{
			string[] test1 = new OCsvLine("x~y~z", '~').FieldValues;
			AssertEquals("x", test1[0]);
			AssertEquals("y", test1[1]);
			AssertEquals("z", test1[2]);
			string[] test2 = new OCsvLine("\"x\"| y |\"z\"", '|').FieldValues;
			AssertEquals("x", test2[0]);
			AssertEquals("y", test2[1]);
			AssertEquals("z", test2[2]);
			string[] test3 = new OCsvLine("\"x\"* y * \"z", '*').FieldValues;
			AssertEquals("x", test3[0]);
			AssertEquals("y", test3[1]);
			AssertEquals("z", test3[2]);
			string[] test4 = new OCsvLine("splat splat").FieldValues;
			AssertEquals("splat splat", test4[0]);
			string[] test5 = new OCsvLine("splat splat,").FieldValues;
			AssertEquals("splat splat", test5[0]);
			AssertEquals("", test5[1]);
			string[] test6 = new OCsvLine("splat, splat, splat", '@').FieldValues;
			AssertEquals("splat, splat, splat", test6[0]);
			string[] test7 = new OCsvLine("|||", '|').FieldValues;
			AssertEquals("", test7[0]);
			AssertEquals("", test7[1]);
			AssertEquals("", test7[2]);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestCsvLineWithInvalidDelimiter()
		{
			new OCsvLine("asdf,asdf,asdf,", '"');
		}

		public void TestQuoteWithoutPair()
		{
			string[] test1 = new OCsvLine("x,y,\",z").FieldValues;
			AssertEquals(3, test1.Length);
			AssertEquals("x", test1[0]);
			AssertEquals("y", test1[1]);
			AssertEquals(",z", test1[2]);
			string[] test2 = new OCsvLine("x,y,z,\"").FieldValues;
			AssertEquals("x", test2[0]);
			AssertEquals("y", test2[1]);
			AssertEquals("z", test2[2]);
			AssertEquals("", test2[3]);
		}

		[ExpectNoExceptions]
		public void TestConstructorWithNullString_ShouldNotThrow()
		{
			new OCsvLine((string)null);
		}
	}
}

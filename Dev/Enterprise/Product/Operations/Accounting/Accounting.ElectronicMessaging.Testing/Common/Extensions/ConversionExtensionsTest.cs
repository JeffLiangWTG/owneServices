using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class ConversionExtensionsTest : TestCase
	{
		public static void TestConversion()
		{
			var expectedText = "Sample text to convert to base64string";
			var base64Text = "U2FtcGxlIHRleHQgdG8gY29udmVydCB0byBiYXNlNjRzdHJpbmc=";
			AssertEquals(expectedText, base64Text.ToUTF8FromBase64());
		}

		public static void TestFormatDecimals()
		{
			var expectedText = "125.25";
			ZDecimal decimalValue = 125.2546587;
			AssertEquals(expectedText, decimalValue.FormatDecimals("F2"));
		}

		[TestDate(2020, 9, 15)]
		public static void TestFormatNullable()
		{
			var expectedText = "20200915";
			ZDateTime? nullableDate = ZDateTime.Today;
			AssertEquals(expectedText, nullableDate.FormatNullableDate("yyyyMMdd"));
		}

		public static void TestStringFallbackOnNullOrEmpty()
		{
			var fallbackValue = "test fallback value";
			string testStringEmpty = ZString.Empty;
			string testStringNull = null;
			AssertEquals(fallbackValue, testStringEmpty.FallbackOnNullOrEmpty(fallbackValue));
			AssertEquals(fallbackValue, testStringNull.FallbackOnNullOrEmpty(fallbackValue));
		}

		public static void TestStringFallbackWithFunc()
		{
			var fallbackValue = "test fallback value";
			string testStringEmpty = ZString.Empty;
			string testStringNull = null;
			AssertEquals(fallbackValue, testStringEmpty.FallbackOnNullOrEmpty(() => fallbackValue));
			AssertEquals(fallbackValue, testStringNull.FallbackOnNullOrEmpty(() => fallbackValue));
			AssertEquals("Non-empty string", "Non-empty string".FallbackOnNullOrEmpty(() => throw new System.Exception("boom!")));
		}

		public static void TestStringTrueValueIsBoolTrue()
		{
			var lowerCaseTrue = "true";
			var upperCaseTrue = "TRUE";
			var capitalisedTrue = "True";
			var exampleString = "this is a test example";
			var lowerCaseFalse = "false";
			var upperCaseFalse = "FALSE";
			var capitalisedFalse = "False";
			AssertEquals(true, lowerCaseTrue.IsTrue());
			AssertEquals(true, upperCaseTrue.IsTrue());
			AssertEquals(true, capitalisedTrue.IsTrue());
			AssertEquals(false, exampleString.IsTrue());
			AssertEquals(false, lowerCaseFalse.IsTrue());
			AssertEquals(false, upperCaseFalse.IsTrue());
			AssertEquals(false, capitalisedFalse.IsTrue());
		}
	}
}

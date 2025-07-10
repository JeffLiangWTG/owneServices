using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class NumberToString_DE_DETest : TestCase
	{
		public void TestGetNumberAsString()
		{
			AssertEquals("null", new NumberToString_DE_DE().GetNumberAsString(0));
			AssertEquals("dreißig", new NumberToString_DE_DE().GetNumberAsString(30));
			AssertEquals("zwölf", new NumberToString_DE_DE().GetNumberAsString(12));
			AssertEquals("fünf", new NumberToString_DE_DE().GetNumberAsString(5));
			AssertEquals("null", new NumberToString_DE_DE().GetNumberAsString(0));
			AssertEquals("neun", new NumberToString_DE_DE().GetNumberAsString(9));
			AssertEquals("zehn", new NumberToString_DE_DE().GetNumberAsString(10));
			AssertEquals("neunzehn", new NumberToString_DE_DE().GetNumberAsString(19));
			AssertEquals("zwanzig", new NumberToString_DE_DE().GetNumberAsString(20));
			AssertEquals("neunundneunzig", new NumberToString_DE_DE().GetNumberAsString(99));
			AssertEquals("einhundert", new NumberToString_DE_DE().GetNumberAsString(100));
			AssertEquals("neunhundertneunundneunzig", new NumberToString_DE_DE().GetNumberAsString(999));
			AssertEquals("eintausend", new NumberToString_DE_DE().GetNumberAsString(1000));
			AssertEquals("neunhundertneunundneunzigtausendneunhundertneunundneunzig", new NumberToString_DE_DE().GetNumberAsString(999999));
			AssertEquals("eine Million", new NumberToString_DE_DE().GetNumberAsString(1000000));
			AssertEquals("neunhundertneunundneunzig Millionen neunhundertneunundneunzigtausendneunhundertneunundneunzig", new NumberToString_DE_DE().GetNumberAsString(999999999));
			AssertEquals("eine Millarde", new NumberToString_DE_DE().GetNumberAsString(1000000000));
			AssertEquals("neunhundertneunundneunzig Millarden neunhundertneunundneunzig Millionen neunhundertneunundneunzigtausendneunhundertneunundneunzig", new NumberToString_DE_DE().GetNumberAsString(999999999999));
		}
	}
}

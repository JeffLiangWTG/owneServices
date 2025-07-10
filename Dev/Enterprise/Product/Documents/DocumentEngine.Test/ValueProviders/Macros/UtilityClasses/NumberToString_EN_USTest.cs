using System;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ReportErrorManagement;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class NumberToString_EN_USTest : TestCase
	{
		public void TestReplacement()
		{
			AssertEquals("twelve", new NumberToString_EN().GetNumberAsString(12));
			AssertEquals("zero", new NumberToString_EN().GetNumberAsString(0));
			AssertEquals("thirty two", new NumberToString_EN().GetNumberAsString(32));
			AssertEquals("twenty one", new NumberToString_EN().GetNumberAsString(21));
			AssertEquals("thirteen", new NumberToString_EN().GetNumberAsString(13));
			AssertEquals("ninety nine", new NumberToString_EN().GetNumberAsString(99));
			AssertEquals("eight", new NumberToString_EN().GetNumberAsString(8));
			AssertEquals("six", new NumberToString_EN().GetNumberAsString(6));
			AssertEquals("one hundred and twenty three", new NumberToString_EN().GetNumberAsString(123));
			AssertEquals("four hundred and ninety nine", new NumberToString_EN().GetNumberAsString(499));
			AssertEquals("one hundred and twenty three thousand, five hundred and thirty four", new NumberToString_EN().GetNumberAsString(123534));
			AssertEquals("six hundred thousand", new NumberToString_EN().GetNumberAsString(600000));
			AssertEquals("one million", new NumberToString_EN().GetNumberAsString(1000000));
			AssertEquals("nine million, nine hundred and ninety nine thousand, nine hundred and ninety nine", new NumberToString_EN().GetNumberAsString(9999999));
		}

		public void TestNegative()
		{
			var numberToString = new NumberToString_EN();
			for (long i = -10; i < 0; ++i)
			{
				AssertExceptionThrown(typeof(FormulaProviderException), () => numberToString.GetNumberAsString(i));
				try
				{
					numberToString.GetNumberAsString(i);
				}
				catch (Exception ex)
				{
					AssertNoExceptionThrown(() => new ReportErrorManager(new Report(null, null)).ReportFatalException(ex));
				}
			}
		}
	}
}

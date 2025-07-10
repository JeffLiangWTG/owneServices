using System;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ReportErrorManagement;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class NumberToString_ENG_SAF_Test : TestCase
	{
		public void TestReplacement()
		{
			AssertEquals("twelve", new NumberToString_ENG_SAF().GetNumberAsString(12));
			AssertEquals("zero", new NumberToString_ENG_SAF().GetNumberAsString(0));
			AssertEquals("thirty two", new NumberToString_ENG_SAF().GetNumberAsString(32));
			AssertEquals("thirteen", new NumberToString_ENG_SAF().GetNumberAsString(13));
			AssertEquals("ninety nine", new NumberToString_ENG_SAF().GetNumberAsString(99));
			AssertEquals("one hundred and twenty three", new NumberToString_ENG_SAF().GetNumberAsString(123));
			AssertEquals("four hundred and ninety nine", new NumberToString_ENG_SAF().GetNumberAsString(499));
			AssertEquals("ninety nine thousand, nine hundred and ninety nine", new NumberToString_ENG_SAF().GetNumberAsString(99999));
			AssertEquals("one lakh, twenty three thousand, five hundred and thirty four", new NumberToString_ENG_SAF().GetNumberAsString(123534));
			AssertEquals("ten lakh", new NumberToString_ENG_SAF().GetNumberAsString(1000000));
			AssertEquals("nine crore, ninety nine lakh, ninety nine thousand, nine hundred and ninety nine", new NumberToString_ENG_SAF().GetNumberAsString(99999999));
			AssertEquals("ninety nine thousand, nine hundred and ninety nine crore, ninety nine lakh, ninety nine thousand, nine hundred and ninety nine", new NumberToString_ENG_SAF().GetNumberAsString(999999999999));
		}

		public void TestNegative()
		{
			var numberToString = new NumberToString_ENG_SAF();
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

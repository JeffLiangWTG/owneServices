using System;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ReportErrorManagement;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	sealed class NumberToString_VI_VNTest : TestCase
	{
		public void TestReplacement()
		{
			var vtn = new NumberToString_VI_VN();
			CombineAssertions(delegate
			{
				AssertEquals("mười hai", vtn.GetNumberAsString(12));
				AssertEquals("không", vtn.GetNumberAsString(0));
				AssertEquals("ba mươi hai", vtn.GetNumberAsString(32));
				AssertEquals("hai mươi mốt", vtn.GetNumberAsString(21));
				AssertEquals("mười ba", vtn.GetNumberAsString(13));
				AssertEquals("chín mươi chín", vtn.GetNumberAsString(99));
				AssertEquals("tám", vtn.GetNumberAsString(8));
				AssertEquals("sáu", vtn.GetNumberAsString(6));
				AssertEquals("một trăm hai mươi ba", vtn.GetNumberAsString(123));
				AssertEquals("bốn trăm chín mươi chín", vtn.GetNumberAsString(499));
				AssertEquals("một trăm lẻ một", vtn.GetNumberAsString(101));
				AssertEquals("một trăm lẻ năm", vtn.GetNumberAsString(105));
				AssertEquals("một trăm mười lăm", vtn.GetNumberAsString(115));
				AssertEquals("chín trăm chín mươi lăm", vtn.GetNumberAsString(995));
				AssertEquals("chín trăm hai mươi mốt", vtn.GetNumberAsString(921));
				AssertEquals("một nghìn không trăm lẻ sáu", vtn.GetNumberAsString(1006));
				AssertEquals("mười nghìn không trăm năm mươi lăm", vtn.GetNumberAsString(10055));
				AssertEquals("một tỷ không trăm lẻ một triệu không trăm ba mươi lăm nghìn sáu trăm lẻ năm", vtn.GetNumberAsString(1001035605));
			});
		}

		public void TestNegative()
		{
			var numberToString = new NumberToString_VI_VN();
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

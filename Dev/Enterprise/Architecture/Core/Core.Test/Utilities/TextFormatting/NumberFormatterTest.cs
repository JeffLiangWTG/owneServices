using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class NumberFormatterTest : TestCase
	{
		public void TestToOrdinalString()
		{
			AssertEquals("ToOrdinalString(1)", "1st", NumberFormatter.ToOrdinalString(1));
			AssertEquals("ToOrdinalString(2)", "2nd", NumberFormatter.ToOrdinalString(2));
			AssertEquals("ToOrdinalString(3)", "3rd", NumberFormatter.ToOrdinalString(3));
			AssertEquals("ToOrdinalString(4)", "4th", NumberFormatter.ToOrdinalString(4));
			AssertEquals("ToOrdinalString(5)", "5th", NumberFormatter.ToOrdinalString(5));
			AssertEquals("ToOrdinalString(6)", "6th", NumberFormatter.ToOrdinalString(6));
			AssertEquals("ToOrdinalString(7)", "7th", NumberFormatter.ToOrdinalString(7));
			AssertEquals("ToOrdinalString(8)", "8th", NumberFormatter.ToOrdinalString(8));
			AssertEquals("ToOrdinalString(9)", "9th", NumberFormatter.ToOrdinalString(9));
			AssertEquals("ToOrdinalString(10)", "10th", NumberFormatter.ToOrdinalString(10));

			AssertEquals("ToOrdinalString(11)", "11th", NumberFormatter.ToOrdinalString(11));
			AssertEquals("ToOrdinalString(12)", "12th", NumberFormatter.ToOrdinalString(12));
			AssertEquals("ToOrdinalString(13)", "13th", NumberFormatter.ToOrdinalString(13));
			AssertEquals("ToOrdinalString(14)", "14th", NumberFormatter.ToOrdinalString(14));
			AssertEquals("ToOrdinalString(15)", "15th", NumberFormatter.ToOrdinalString(15));
			AssertEquals("ToOrdinalString(16)", "16th", NumberFormatter.ToOrdinalString(16));
			AssertEquals("ToOrdinalString(17)", "17th", NumberFormatter.ToOrdinalString(17));
			AssertEquals("ToOrdinalString(18)", "18th", NumberFormatter.ToOrdinalString(18));
			AssertEquals("ToOrdinalString(19)", "19th", NumberFormatter.ToOrdinalString(19));
			AssertEquals("ToOrdinalString(20)", "20th", NumberFormatter.ToOrdinalString(20));

			AssertEquals("ToOrdinalString(21)", "21st", NumberFormatter.ToOrdinalString(21));
			AssertEquals("ToOrdinalString(22)", "22nd", NumberFormatter.ToOrdinalString(22));
			AssertEquals("ToOrdinalString(23)", "23rd", NumberFormatter.ToOrdinalString(23));
			AssertEquals("ToOrdinalString(24)", "24th", NumberFormatter.ToOrdinalString(24));
		}
	}
}

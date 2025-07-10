using System;
using System.Globalization;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class DefaultCultureTest : TestCase
	{
		public void TestDefaultCulture()
		{
			CultureInfo cultureInfo = DefaultCulture.Instance;
			AssertEquals("Decimal point", ".", cultureInfo.NumberFormat.NumberDecimalSeparator);
			AssertEquals("Decimal point", ".", cultureInfo.NumberFormat.CurrencyDecimalSeparator);
			AssertEquals("Decimal point", ".", cultureInfo.NumberFormat.PercentDecimalSeparator);
			AssertEquals("Group separator", ",", cultureInfo.NumberFormat.NumberGroupSeparator);
			AssertEquals("Group separator", ",", cultureInfo.NumberFormat.CurrencyGroupSeparator);
			AssertEquals("Group separator", ",", cultureInfo.NumberFormat.PercentGroupSeparator);
			AssertEquals("Currency symbol", "$", cultureInfo.NumberFormat.CurrencySymbol);
			AssertEquals("Positive sign", "+", cultureInfo.NumberFormat.PositiveSign);
			AssertEquals("Negative sign", "-", cultureInfo.NumberFormat.NegativeSign);
			AssertEquals("Date months", "JAN", cultureInfo.DateTimeFormat.AbbreviatedMonthNames[0].ToUpper());
			AssertEquals("Date months", "FEB", cultureInfo.DateTimeFormat.AbbreviatedMonthNames[1].ToUpper());
			AssertEquals("Date months", "MAR", cultureInfo.DateTimeFormat.AbbreviatedMonthNames[2].ToUpper());
			AssertEquals("Date months", "APR", cultureInfo.DateTimeFormat.AbbreviatedMonthNames[3].ToUpper());
			AssertEquals("Date months", "MAY", cultureInfo.DateTimeFormat.AbbreviatedMonthNames[4].ToUpper());
			AssertEquals("Date months", "JUN", cultureInfo.DateTimeFormat.AbbreviatedMonthNames[5].ToUpper());
			AssertEquals("Date months", "JUL", cultureInfo.DateTimeFormat.AbbreviatedMonthNames[6].ToUpper());
			AssertEquals("Date months", "AUG", cultureInfo.DateTimeFormat.AbbreviatedMonthNames[7].ToUpper());
			AssertEquals("Date months", "SEP", cultureInfo.DateTimeFormat.AbbreviatedMonthNames[8].ToUpper());
			AssertEquals("Date months", "OCT", cultureInfo.DateTimeFormat.AbbreviatedMonthNames[9].ToUpper());
			AssertEquals("Date months", "NOV", cultureInfo.DateTimeFormat.AbbreviatedMonthNames[10].ToUpper());
			AssertEquals("Date months", "DEC", cultureInfo.DateTimeFormat.AbbreviatedMonthNames[11].ToUpper());
			AssertEquals("AM Designator", "AM", cultureInfo.DateTimeFormat.AMDesignator);
			AssertEquals("PM Designator", "PM", cultureInfo.DateTimeFormat.PMDesignator);
			AssertEquals("Short Date Format Pattern", "d/MM/yyyy", cultureInfo.DateTimeFormat.ShortDatePattern);
		}

#if NETFRAMEWORK
		[TargetFrameworks(TargetFramework.NetFramework | TargetFramework.NetCore)]
#endif
		public void TestDateStringWithMonthAbbreviation()
		{
			AssertEquals("4-Jul-2023", new DateTime(2023, 7, 4).ToString("d-MMM-yyyy"));
		}

#if NETFRAMEWORK
		[TargetFrameworks(TargetFramework.NetFramework | TargetFramework.NetCore)]
#endif
		public void TestShortDateString()
		{
			AssertEquals("4/06/2023", new DateTime(2023, 06, 04).ToString("d"));
		}
	}
}

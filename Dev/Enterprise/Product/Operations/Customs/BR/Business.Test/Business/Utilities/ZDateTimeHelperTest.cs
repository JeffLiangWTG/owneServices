using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ZDateTimeHelperTest : TestCase
	{
		public void TestToYearDateString()
		{
			var date = ZDateTime.Now;
			AssertEquals("When invalid input parameter, output", ZString.Empty, ZDateTimeHelper.ToYearDateString(ZDateTime.Invalid));
			AssertEquals("When empty input parameter, output", ZString.Empty, ZDateTimeHelper.ToYearDateString(ZDateTime.Empty));
			AssertEquals("When valid input parameter, output", date.Year.ToString(), ZDateTimeHelper.ToYearDateString(date));
		}

		public void TestParseSmallDateTimeFromYear()
		{
			var date = new ZDateTime(ZDateTime.Now.Year, 1, 1);
			AssertEquals("When invalid input parameter, output", ZDateTime.Empty, ZDateTimeHelper.ParseSmallDateTimeFromYear("ASD"));
			AssertEquals("When empty input parameter, output", ZDateTime.Empty, ZDateTimeHelper.ParseSmallDateTimeFromYear(ZString.Empty));
			AssertEquals("When valid input parameter, output", new ZDateTime(1900, 1, 1), ZDateTimeHelper.ParseSmallDateTimeFromYear("1899"));
			AssertEquals("When valid input parameter, output", new ZDateTime(2079, 1, 1), ZDateTimeHelper.ParseSmallDateTimeFromYear("2080"));
			AssertEquals("When valid input parameter, output", date, ZDateTimeHelper.ParseSmallDateTimeFromYear(date.Year.ToString()));
		}

		public void TestToNullableDateTime()
		{
			AssertEquals(new DateTime(2023, 3, 27, 10, 58, 10), new ZDateTime(2023, 3, 27, 10, 58, 10).ToNullableDateTime());
			AssertNull(ZDateTime.Empty.ToNullableDateTime());
			AssertNull(ZDateTime.Invalid.ToNullableDateTime());
		}

		[TestDate(2025, 01, 01)]
		public void TestToDateTimeSafe()
		{
			AssertEquals(new DateTime(2023, 3, 27, 10, 58, 10), new ZDateTime(2023, 3, 27, 10, 58, 10).ToDateTimeSafe());
			AssertEquals(new DateTime(2025, 01, 01), ZDateTime.Empty.ToDateTimeSafe());
			AssertEquals(new DateTime(2025, 01, 01), ZDateTime.Invalid.ToDateTimeSafe());
		}
	}
}

using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ZDateTimeHelperTest : TestCase
{
	public void TestToYearDateString()
	{
		AssertEquals("When invalid input parameter, output", ZString.Empty, ZDateTimeHelper.ToYearDateString(ZDateTime.Invalid));
		AssertEquals("When empty input parameter, output", ZString.Empty, ZDateTimeHelper.ToYearDateString(ZDateTime.Empty));
		AssertEquals("When valid input parameter, output", "2020", ZDateTimeHelper.ToYearDateString(new ZDateTime(2020, 10, 10)));
	}

	public void TestParseDateTimeFromYear()
	{
		AssertEquals("When invalid input parameter, output", ZDateTime.Empty, ZDateTimeHelper.ParseDateTimeFromYear("ASD"));
		AssertEquals("When empty input parameter, output", ZDateTime.Empty, ZDateTimeHelper.ParseDateTimeFromYear(ZString.Empty));
		AssertEquals("When valid input parameter, output", new ZDateTime(2020, 01, 01), ZDateTimeHelper.ParseDateTimeFromYear("2020"));
	}

	public void TestBackwardDateToFirstDayOfYear()
	{
		AssertEquals("When invalid input parameter, output", ZDateTime.Invalid, ZDateTimeHelper.BackwardDateToFirstDayOfYear(ZDateTime.Invalid));
		AssertEquals("When empty input parameter, output", ZDateTime.Empty, ZDateTimeHelper.BackwardDateToFirstDayOfYear(ZDateTime.Empty));
		AssertEquals("When valid input parameter, output", new ZDateTime(2020, 1, 1), ZDateTimeHelper.BackwardDateToFirstDayOfYear(new ZDateTime(2020, 10, 10)));
	}

	public void TestToItalianShortDateString()
	{
		CombineAssertions("Assert ToItalianShortDateString()", () =>
		{
			AssertEquals("When invalid input parameter, output", ZString.Empty, ZDate.Invalid.ToItalianShortDateString());
			AssertEquals("When valid input parameter, output", "01/06/2021", new ZDate(2021, 06, 01).ToItalianShortDateString());
			AssertEquals("When valid input parameter, output", "01/06/2021", new ZDateTime(2021, 06, 01, 01, 21, 30).ToItalianShortDateString());
		});
	}

	public void TestToCustomsDateString()
	{
		CombineAssertions("Assert ToCustomsDateString()", () =>
		{
			AssertEquals("When invalid input parameter, output", ZString.Empty, ZDate.Invalid.ToCustomsDateString());
			AssertEquals("When valid input parameter, output", "01062021", new ZDate(2021, 06, 01).ToCustomsDateString());
		});
	}

	public void TestIsInRangeExcludingBounds()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>("When referenceDate is invalid", () => ZDateTimeHelper.IsInRangeExcludingBounds(ZDate.Invalid, ZDate.Today, ZDate.Today));
			AssertExceptionThrown<ArgumentException>("When rangeLowerBound is invalid", () => ZDateTimeHelper.IsInRangeExcludingBounds(ZDate.Today, ZDate.Invalid, ZDate.Today));
			AssertExceptionThrown<ArgumentException>("When rangeUpperBound is invalid", () => ZDateTimeHelper.IsInRangeExcludingBounds(ZDate.Today, ZDate.Today, ZDate.Invalid));
			AssertExceptionThrown<ArgumentException>("When a date is empty (it is also considered invalid)", () => ZDateTimeHelper.IsInRangeExcludingBounds(ZDate.Empty, ZDate.Today, ZDate.Today));

			var referenceDate = new ZDate(2021, 03, 25);
			AssertEquals("When referenceDate is equal to rangeLowerBound, IsInRangeExcludingBounds()", false, referenceDate.IsInRangeExcludingBounds(referenceDate, referenceDate.AddDays(1)));
			AssertEquals("When referenceDate is included between rangeLowerBound and rangeUpperBound, IsInRangeExcludingBounds()", true, referenceDate.IsInRangeExcludingBounds(referenceDate.AddDays(-1), referenceDate.AddDays(1)));
			AssertEquals("When referenceDate is equal to rangeUpperBound, IsInRangeExcludingBounds()", false, referenceDate.IsInRangeExcludingBounds(referenceDate.AddDays(-1), referenceDate));
			AssertEquals("[EDGE-CASE] When all dates are equal, IsInRangeExcludingBounds()", false, referenceDate.IsInRangeExcludingBounds(referenceDate, referenceDate));
		});
	}

	public void TestTryParseDateWithShortFormat()
	{
		CombineAssertions(() =>
		{
			var isParsed = ZDateTimeHelper.TryParseDateWithShortFormat("22/02/2023", out var parsedDate);
			AssertEquals("IsParsed", true, isParsed);
			AssertEquals("Parsed Date", new ZDateTime(2023, 02, 22), parsedDate);

			isParsed = ZDateTimeHelper.TryParseDateWithShortFormat("01/12/2021", out parsedDate);
			AssertEquals("IsParsed", true, isParsed);
			AssertEquals("Parsed Date", new ZDateTime(2021, 12, 1), parsedDate);

			isParsed = ZDateTimeHelper.TryParseDateWithShortFormat("01/25/2021", out parsedDate);
			AssertEquals("IsParsed", false, isParsed);

			isParsed = ZDateTimeHelper.TryParseDateWithShortFormat("2022/12/01", out parsedDate);
			AssertEquals("IsParsed", false, isParsed);

			isParsed = ZDateTimeHelper.TryParseDateWithShortFormat("2022-12-01", out parsedDate);
			AssertEquals("IsParsed", false, isParsed);

			isParsed = ZDateTimeHelper.TryParseDateWithShortFormat("", out parsedDate);
			AssertEquals("IsParsed", true, isParsed);
			AssertEquals("Parsed Date", ZDateTime.Empty, parsedDate);

			isParsed = ZDateTimeHelper.TryParseDateWithShortFormat(null, out parsedDate);
			AssertEquals("IsParsed", true, isParsed);
			AssertEquals("Parsed Date", ZDateTime.Empty, parsedDate);

			isParsed = ZDateTimeHelper.TryParseDateWithShortFormat("             ", out parsedDate);
			AssertEquals("IsParsed", false, isParsed);
			AssertEquals("Parsed Date", ZDateTime.Invalid, parsedDate);
		});
	}

	public void TestParseToDateDDMMYYYYSafe()
	{
		CombineAssertions("When Input is", () =>
		{
			AssertEquals("Invalid Text", ZDateTime.Empty, ZDateTimeHelper.ParseToDateDDMMYYYYSafe("XYZ"));
			AssertEquals("Empty", ZDateTime.Empty, ZDateTimeHelper.ParseToDateDDMMYYYYSafe(""));
			AssertEquals("Spaces", ZDateTime.Empty, ZDateTimeHelper.ParseToDateDDMMYYYYSafe("  "));
			AssertEquals("Invalid Date", ZDateTime.Empty, ZDateTimeHelper.ParseToDateDDMMYYYYSafe("22332023"));
			AssertEquals("Valid Date", new ZDateTime(2023, 8, 22), ZDateTimeHelper.ParseToDateDDMMYYYYSafe("22082023"));
		});
	}
}

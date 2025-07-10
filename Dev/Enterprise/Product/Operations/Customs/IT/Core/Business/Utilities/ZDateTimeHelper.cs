using System;
using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public static class ZDateTimeHelper
{
	public static ZString ToYearDateString(this ZDateTime dateTime) => dateTime.IsValid ? dateTime.ToString(YearOfIssueFormat, CultureInfo.InvariantCulture) : string.Empty;

	public static ZDateTime ParseDateTimeFromYear(ZString year) => ZDateTime.TryParseExact(year, out var parsedDate, YearOfIssueFormat) ? parsedDate : ZDateTime.Empty;

	public static ZDateTime BackwardDateToFirstDayOfYear(this ZDateTime dateToBackward) => dateToBackward.IsValid ? new ZDateTime(dateToBackward.Year, 1, 1) : dateToBackward;

	public static ZString ToItalianShortDateString(this ZDate date) => date.IsValid ? date.ToString(ShortDateFormat, CultureInfo.InvariantCulture) : string.Empty;
	public static ZString ToItalianShortDateString(this ZDateTime dateTime) => dateTime.Date.ToItalianShortDateString();

	public static ZString ToCustomsDateString(this ZDate date) => date.IsValid ? date.ToString(CustomsDateFormat, CultureInfo.InvariantCulture) : string.Empty;

	public static ZBool IsInRangeExcludingBounds(this ZDate referenceDate, ZDate rangeLowerBound, ZDate rangeUpperBound)
	{
		CheckIsValidDate(referenceDate, nameof(referenceDate));
		CheckIsValidDate(rangeLowerBound, nameof(rangeLowerBound));
		CheckIsValidDate(rangeUpperBound, nameof(rangeUpperBound));

		return referenceDate > rangeLowerBound && referenceDate < rangeUpperBound;

		void CheckIsValidDate(ZDate dateToCheck, ZString argumentName)
		{
			if (!dateToCheck.IsValid)
			{
				throw new ArgumentException($"{argumentName} is not a valid date");
			}
		}
	}

	public static bool TryParseDateWithShortFormat(string stringDate, out ZDateTime dateTime)
	{
		return ZDateTime.TryParseExact(stringDate, out dateTime, ShortDateFormat);
	}

	public static ZDateTime ParseToDateDDMMYYYYSafe(ZString value) => ZDateTime.TryParseExact(value, out var dateTime, CustomsDateFormat) ? dateTime : ZDateTime.Empty;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant year format")]
	const string YearOfIssueFormat = "yyyy";
	const string ShortDateFormat = "dd/MM/yyyy";
	const string CustomsDateFormat = "ddMMyyyy";
}

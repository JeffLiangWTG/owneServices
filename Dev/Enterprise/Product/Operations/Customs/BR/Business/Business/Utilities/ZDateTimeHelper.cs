using System;
using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public static class ZDateTimeHelper
	{
		public static DateTime? ToNullableDateTime(this ZDateTime dateTime) => dateTime.IsValid ? dateTime.ToDateTime() : null;

		public static DateTime ToDateTimeSafe(this ZDateTime dateTime) => dateTime.IsValid ? dateTime.ToDateTime() : ZDateTime.Today.ToDateTime();

		public static ZString ToYearDateString(this ZDateTime dateTime) => dateTime.IsValid ? dateTime.ToString(YearOfIssueFormat, CultureInfo.InvariantCulture) : string.Empty;

		public static ZDateTime ParseSmallDateTimeFromYear(ZString year)
		{
			var result = ZDateTime.Empty;
			if (int.TryParse(year, out var parsedYear) && parsedYear > 0)
			{
				parsedYear = Math.Min(Math.Max(parsedYear, ZDateTime.MinSmallDateTimeValue.Year), ZDateTime.MaxSmallDateTimeValue.Year);
				result = new ZDateTime(parsedYear, 1, 1);
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Year format")]
		const string YearOfIssueFormat = "yyyy";
	}
}

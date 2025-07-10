using System;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public static class DateTimeProviderHelper
	{
		public static DateTime ConvertToUnspecifiedDateTimeKindIfPossible(ZDateTime date, bool removeMillisecond = false)
		{
			var dateTime = date.IsValid ? date.ToDateTime() : DateTime.MinValue;
			return removeMillisecond
				? new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, DateTimeKind.Unspecified)
				: new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond, DateTimeKind.Unspecified);
		}

		public static ZDateTime ConvertAisUcc5DateStringToZDateTime(string dateString) => ZDateTime.TryParseExact(dateString, out var result, Ucc5DateStringFormat) ? result : ZDateTime.Empty;

		public static int? ConvertStringToNullableInt(ZString stringValue) => !stringValue.IsEmpty && int.TryParse(stringValue, out var value) ? value : null;

		const string Ucc5DateStringFormat = "yyyyMMdd";
	}
}

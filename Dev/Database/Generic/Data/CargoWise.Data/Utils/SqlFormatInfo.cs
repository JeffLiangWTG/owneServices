using System;
using System.Globalization;
using CargoWise.Common;

namespace CargoWise.Data
{
	public static class SqlFormatInfo
	{
		static SqlFormatInfo()
		{
			sqlServerDateTimeFormatInfo.FullDateTimePattern = "yyyy-MM-dd HH:mm:ss.fff"; // datetime format string
			sqlServerDateTimeFormatInfo.LongDatePattern = "yyyy-MM-dd";
			sqlServerDateTimeFormatInfo.ShortDatePattern = sqlServerDateTimeFormatInfo.LongDatePattern;
			sqlServerDateTimeFormatInfo.LongTimePattern = "HH:mm:ss.fff";
			sqlServerDateTimeFormatInfo.ShortTimePattern = sqlServerDateTimeFormatInfo.LongTimePattern;
			sqlServerDateTimeFormatInfo.TimeSeparator = ":";
			sqlServerDateTimeFormatInfo.DateSeparator = "-";

			sqlServerDateTimeOffsetFormatInfo.FullDateTimePattern = "yyyy-MM-dd HH:mm:ss.fffffff zzz"; // datetime format string
			sqlServerDateTimeOffsetFormatInfo.LongDatePattern = "yyyy-MM-dd"; // datetime format string
			sqlServerDateTimeOffsetFormatInfo.ShortDatePattern = sqlServerDateTimeFormatInfo.LongDatePattern;
			sqlServerDateTimeOffsetFormatInfo.LongTimePattern = "HH:mm:ss.fffffff zzz"; // datetime format string
			sqlServerDateTimeOffsetFormatInfo.ShortTimePattern = sqlServerDateTimeFormatInfo.LongTimePattern;
			sqlServerDateTimeOffsetFormatInfo.TimeSeparator = ":";
			sqlServerDateTimeOffsetFormatInfo.DateSeparator = "-";
		}

		static readonly DateTimeFormatInfo sqlServerDateTimeFormatInfo = new ();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "If sqlServerDateTimeFormatInfo is fine, so is this")]
		static readonly DateTimeFormatInfo sqlServerDateTimeOffsetFormatInfo = new ();

		#region Sql Server ANSI Formats

		public static string ToSqlDateString(DateTime date)
		{
			return date.ToString("D", sqlServerDateTimeFormatInfo);
		}

		public static DateTime FromSqlDate(string dateTime)
		{
			Argument.NotNullOrEmpty(dateTime, nameof(dateTime));

			return DateTime.ParseExact(dateTime, "D", sqlServerDateTimeFormatInfo);
		}

		public static string ToSqlDateTimeString(DateTime dateTime)
		{
			return dateTime.ToString("F", sqlServerDateTimeFormatInfo);
		}

		public static string ToSqlTimeString(TimeSpan time)
		{
			return time.ToString("c", sqlServerDateTimeFormatInfo);
		}

		public static DateTime FromSqlDateTime(string dateTime)
		{
			Argument.NotNullOrEmpty(dateTime, nameof(dateTime));

			return DateTime.ParseExact(dateTime, "F", sqlServerDateTimeFormatInfo);
		}

		public static TimeSpan FromSqlTime(string time)
		{
			Argument.NotNullOrEmpty(time, nameof(time));

			return TimeSpan.ParseExact(time, "c", sqlServerDateTimeFormatInfo);
		}

		public static bool TryParseFromSqlDateTime(string dateTimeStr, out DateTime result)
		{
			Argument.NotNullOrEmpty(dateTimeStr, nameof(dateTimeStr));

			return DateTime.TryParseExact(dateTimeStr, "F", sqlServerDateTimeFormatInfo, DateTimeStyles.None, out result);
		}

		public static string ToSqlDateTimeOffsetString(DateTimeOffset dateTimeOffset)
		{
			return dateTimeOffset.ToString("F", sqlServerDateTimeOffsetFormatInfo);
		}

		public static DateTimeOffset FromSqlDateTimeOffset(string dateTimeOffset)
		{
			Argument.NotNullOrEmpty(dateTimeOffset, nameof(dateTimeOffset));

			return DateTimeOffset.ParseExact(dateTimeOffset, "F", sqlServerDateTimeOffsetFormatInfo);
		}

		public static bool TryParseFromSqlDateTimeOffset(string dateTimeOffsetStr, out DateTimeOffset result)
		{
			Argument.NotNullOrEmpty(dateTimeOffsetStr, nameof(dateTimeOffsetStr));

			return DateTimeOffset.TryParseExact(dateTimeOffsetStr, "F", sqlServerDateTimeOffsetFormatInfo, DateTimeStyles.None, out result);
		}

		#endregion
	}
}

using System;
using CargoWise.Data;
using CargoWise.Types;

namespace Enterprise.Environment
{
	public class DateTimeProvider : IDateTimeProvider
	{
		public virtual DateTime CurrentLocalDate
		{
			get { return Env.Time.CurrentLocalDate; }
		}

		public virtual DateTime CurrentLocalDateTime
		{
			get { return Env.Time.CurrentLocalDateTime; }
		}

		public virtual DateTime CurrentUtcDateTime
		{
			get { return Env.Time.CurrentUtcDateTime; }
		}

		public virtual (DateTime time, TimeSpan offset) CurrentUNLOCODateTime(string unloco)
		{
			var time = Env.Time.GetUnlocoDateTime(unloco);
			return (time, Env.Time.GetUtcOffsetBasedOnLocal(unloco, time));
		}

		public string FormatSqlDateTime(DateTime dateTime)
		{
			return SqlFormatInfo.ToSqlDateTimeString(dateTime);
		}

		public string FormatSqlTime(TimeSpan dateTime)
		{
			return SqlFormatInfo.ToSqlTimeString(dateTime);
		}

		public DateTime FromSqlDateTime(string sqlDateTime)
		{
			return SqlFormatInfo.FromSqlDateTime(sqlDateTime);
		}

		public TimeSpan FromSqlTime(string sqlTime)
		{
			return SqlFormatInfo.FromSqlTime(sqlTime);
		}

		public DateTimeOffset FromSqlDateTimeOffset(string sqlDateTimeOffset)
		{
			return SqlFormatInfo.FromSqlDateTimeOffset(sqlDateTimeOffset);
		}

		public string FormatSqlDateTimeOffset(DateTimeOffset dateTimeOffset)
		{
			return SqlFormatInfo.ToSqlDateTimeOffsetString(dateTimeOffset);
		}

		public virtual DateTime CurrentUtcDate
		{
			get { return Env.Time.CurrentUtcDate; }
		}

		public string FormatDate(DateTime dateToFormat)
		{
			return Env.Time.FormatDate(dateToFormat);
		}

		public string FormatDateTime(DateTime dateTimeToFormat)
		{
			return Env.Time.FormatDateTime(dateTimeToFormat);
		}

		public string FormatDateTimeWithSeconds(DateTime dateTimeToFormat)
		{
			return Env.Time.FormatDateTimeWithSeconds(dateTimeToFormat);
		}

		public string FormatTime(DateTime timeToFormat)
		{
			return Env.Time.FormatTime(timeToFormat);
		}

		public string FormatTimeWithSeconds(DateTime timeToFormat)
		{
			return Env.Time.FormatTimeWithSeconds(timeToFormat);
		}

		public string FormatDateTimeOffset(DateTimeOffset dateTimeOffsetToFormat)
		{
			return Env.Time.FormatDateTimeOffset(dateTimeOffsetToFormat);
		}

		public string FormatDateTimeOffsetWithSeconds(DateTimeOffset dateTimeOffsetToFormat)
		{
			return Env.Time.FormatDateTimeOffsetWithSeconds(dateTimeOffsetToFormat);
		}

		public string Format(DateTime dateTimeToFormat, string formatString)
		{
			return Env.Time.Format(dateTimeToFormat, formatString);
		}

		public string Format(DateTimeOffset dateTimeOffsetToFormat, string formatString)
		{
			return Env.Time.Format(dateTimeOffsetToFormat, formatString);
		}

		public DateTime GetLocalTimeFromUtc(DateTime utcDateTime)
		{
			return Env.Time.GetLocalTimeFromUtc(utcDateTime);
		}

		public DateTime GetUtcFromLocalTime(DateTime localDateTime)
		{
			return Env.Time.GetUtcFromLocalTime(localDateTime);
		}

		public TimeSpan GetUtcOffsetBasedOnUtc(DateTime utcDateTime)
		{
			return Env.Time.GetUtcOffsetBasedOnUtc(utcDateTime);
		}

		public TimeSpan GetUtcOffsetBasedOnLocal(DateTime localDateTime)
		{
			return Env.Time.GetUtcOffsetBasedOnLocal(localDateTime);
		}
	}
}

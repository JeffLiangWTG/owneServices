using System;

namespace CargoWise.Types
{
	/// <summary>
	/// Interface used by ZDateTime to provide 'current' date and time
	/// </summary>
	[WTG.StaticAnalysis.Annotation.ThreadSafe]
	public interface IDateTimeProvider
	{
		DateTime CurrentLocalDate { get; }
		DateTime CurrentLocalDateTime { get; }
		DateTime CurrentUtcDate { get; }
		DateTime CurrentUtcDateTime { get; }
		(DateTime time, TimeSpan offset) CurrentUNLOCODateTime(string unloco);

		string FormatDate(DateTime dateToFormat);
		string FormatDateTime(DateTime dateTimeToFormat);
		string FormatDateTimeWithSeconds(DateTime dateTimeToFormat);
		string FormatTime(DateTime timeToFormat);
		string FormatTimeWithSeconds(DateTime timeToFormat);
		string FormatSqlDateTime(DateTime dateTime);
		string FormatSqlTime(TimeSpan dateTime);
		string FormatDateTimeOffset(DateTimeOffset dateTimeOffsetToFormat);
		string FormatDateTimeOffsetWithSeconds(DateTimeOffset dateTimeOffsetToFormat);
		string Format(DateTime dateTimeToFormat, string formatString);
		string Format(DateTimeOffset dateTimeOffsetToFormat, string formatString);
		DateTime FromSqlDateTime(string sqlDateTime);
		TimeSpan FromSqlTime(string sqlTime);
		DateTimeOffset FromSqlDateTimeOffset(string sqlDateTimeOffset);
		string FormatSqlDateTimeOffset(DateTimeOffset dateTimeOffset);

		DateTime GetLocalTimeFromUtc(DateTime utcDateTime);
		DateTime GetUtcFromLocalTime(DateTime localDateTime);

		TimeSpan GetUtcOffsetBasedOnUtc(DateTime utcDateTime);

		TimeSpan GetUtcOffsetBasedOnLocal(DateTime localDateTime);
	}
}

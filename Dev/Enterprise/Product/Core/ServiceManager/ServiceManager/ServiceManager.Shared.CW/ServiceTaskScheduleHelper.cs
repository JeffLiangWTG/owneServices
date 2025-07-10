using System;
using Enterprise.Environment;
using Enterprise.Scheduler.Business;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(ScheduleRecurrenceType))]

namespace ServiceManager.Shared.CW;
public static class ServiceTaskScheduleHelper
{
	public static TimeSpan GetPeriodDuration(string s, TimeSpan defaultValue, bool isRandomPeriod)
	{
		int value;
		string recurrence;
		if (ParseFrequency(s, out value, out recurrence))
		{
			if (isRandomPeriod)
			{
				value = new Random().Next(value);
			}

			switch (recurrence)
			{
				case ScheduleRecurrenceType.Second:
					return TimeSpan.FromSeconds(value);

				case ScheduleRecurrenceType.Minute:
					return TimeSpan.FromMinutes(value);

				case ScheduleRecurrenceType.Hourly:
					return TimeSpan.FromHours(value);

				case ScheduleRecurrenceType.Daily:
					return TimeSpan.FromDays(value);

				case ScheduleRecurrenceType.Weekly:
					return TimeSpan.FromDays(value * 7);

				case ScheduleRecurrenceType.Monthly:
					return TimeSpan.FromDays(value * 28);

				case ScheduleRecurrenceType.Yearly:
					return TimeSpan.FromDays(value * 365);
			}
		}

		return defaultValue;
	}

	public static bool ParseFrequency(string s, out int value, out string recurrence)
	{
		if (string.IsNullOrEmpty(s))
		{
			value = 0;
			recurrence = null;
			return false;
		}

		s = s.Trim();
		var numLength = 0;
		for (; numLength < s.Length; numLength++)
		{
			if (!char.IsDigit(s, numLength))
			{
				break;
			}
		}

		var hasValue = int.TryParse(s.Substring(0, numLength), out value);
		switch (s.Substring(numLength).TrimStart().ToLowerInvariant())
		{
			case "s":
			case "second":
			case "seconds":
				recurrence = ScheduleRecurrenceType.Second;
				return hasValue;

			case "m":
			case "minute":
			case "minutes":
				recurrence = ScheduleRecurrenceType.Minute;
				return hasValue;

			case "h":
			case "hour":
			case "hours":
				recurrence = ScheduleRecurrenceType.Hourly;
				return hasValue;

			case "d":
			case "day":
			case "days":
				recurrence = ScheduleRecurrenceType.Daily;
				return hasValue;

			case "w":
			case "week":
			case "weeks":
				recurrence = ScheduleRecurrenceType.Weekly;
				return hasValue;

			case "n":
			case "month":
			case "months":
				recurrence = ScheduleRecurrenceType.Monthly;
				return hasValue;

			case "y":
			case "year":
			case "years":
				recurrence = ScheduleRecurrenceType.Yearly;
				return hasValue;

			default:
				recurrence = null;
				return false;
		}
	}

	public static TimeSpan GetUtcOffsetBasedOnUtc(string unloco, DateTime time)
	{
		return Env.Time.GetUtcOffsetBasedOnUtc(unloco, time);
	}

	public static TimeSpan GetUtcOffsetBasedOnUtc(DateTime time)
	{
		return Env.Time.GetUtcOffsetBasedOnUtc(time);
	}
}

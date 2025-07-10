using System;
using Enterprise.Scheduler.Business;
using Enterprise.ServiceManager.Business;

namespace ServiceManager.Integration.ServiceTasks.CW.Test
{
	public static class ServiceTaskFrequencyTestHelper
	{
		public static TimeSpan GetPeriodDuration(string s, TimeSpan defaultValue) => ServiceTaskScheduleValidation.GetPeriodDuration(s, defaultValue, isRandomPeriod: false);

		public static bool ParseFrequency(string s, out int value, out string recurrence) => ServiceTaskScheduleValidation.ParseFrequency(s, out value, out recurrence);

		public static string CalculateDefaultScheduleRunEvery(string taskPeriod, int taskPeriodCount)
		{
			switch (taskPeriod)
			{
				case ScheduleRecurrenceType.Second:
					return $"{taskPeriodCount}seconds";

				case ScheduleRecurrenceType.Minute:
					return $"{taskPeriodCount}minutes";

				case ScheduleRecurrenceType.Hourly:
					return $"{taskPeriodCount}hours";

				case ScheduleRecurrenceType.Daily:
					return $"{taskPeriodCount}days";

				case ScheduleRecurrenceType.Weekly:
					return $"{taskPeriodCount}weeks";

				case ScheduleRecurrenceType.Monthly:
					return $"{taskPeriodCount}months";

				case ScheduleRecurrenceType.Yearly:
					return $"{taskPeriodCount}years";

				default:
					return "15minutes";
			}
		}
	}
}

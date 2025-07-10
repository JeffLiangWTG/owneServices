using System;
using System.Globalization;
using System.Xml.Serialization;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Shared
{
	public record NextRunTimeCalculatorYearsByDayOfMonth : INextRunTimeCalculator
	{
		[XmlAttribute]
		public int WeekOfTheMonth { get; set; }

		[XmlAttribute]
		public DayOfWeek DayOfTheWeek { get; set; }

		[XmlAttribute]
		public int MonthOfTheYear { get; set; }

		[XmlIgnore]
		public TimeSpan ScheduledRunTime { get; set; }

		[XmlAttribute(AttributeName = nameof(ScheduledRunTime))]
		public string ScheduledRunTimeString
		{
			get => ScheduledRunTime.ToString(ScheduleConfig.TimeFormat, CultureInfo.InvariantCulture);
			set => ScheduledRunTime = TimeSpan.ParseExact(value, ScheduleConfig.TimeFormat, CultureInfo.InvariantCulture);
		}

		public DateTimeOffset CalculateNextRunTime(DateTimeOffset calculateFrom)
		{
			var nextRunTime = calculateFrom
				.ToUniversalTime();

			nextRunTime = nextRunTime
				.AddMonths(MonthOfTheYear - nextRunTime.Month)
				.AddDays(((WeekOfTheMonth - 1) * 7) + 1 - nextRunTime.Day)
				.Subtract(nextRunTime.TimeOfDay)
				.Add(ScheduledRunTime);

			while (nextRunTime.DayOfWeek != DayOfTheWeek)
			{
				nextRunTime = nextRunTime.AddDays(1);
			}

			if (nextRunTime >= calculateFrom)
			{
				return nextRunTime;
			}

			nextRunTime = nextRunTime
				.AddYears(1)
				.AddMonths(MonthOfTheYear - nextRunTime.Month)
				.AddDays(((WeekOfTheMonth - 1) * 7) + 1 - nextRunTime.Day);

			while (nextRunTime.DayOfWeek != DayOfTheWeek)
			{
				nextRunTime = nextRunTime.AddDays(1);
			}

			return nextRunTime;
		}
	}
}

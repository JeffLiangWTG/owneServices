using System;
using System.Globalization;
using System.Xml.Serialization;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Shared
{
	public record NextRunTimeCalculatorMonthsByDayOfWeek : INextRunTimeCalculator
	{
		[XmlAttribute]
		public int WeekOfTheMonth { get; set; }

		[XmlAttribute]
		public DayOfWeek DayOfTheWeek { get; set; }

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

			nextRunTime = CalculateRunTime(nextRunTime);
			if (nextRunTime < calculateFrom)
			{
				nextRunTime = CalculateRunTime(nextRunTime.AddMonths(1));
			}

			return nextRunTime;
		}

		DateTimeOffset CalculateRunTime(DateTimeOffset calculateFrom)
		{
			var nextRunTime = calculateFrom
				.AddDays(((WeekOfTheMonth - 1) * 7) + 1 - calculateFrom.Day)
				.Subtract(calculateFrom.TimeOfDay)
				.Add(ScheduledRunTime);

			while (nextRunTime.DayOfWeek != DayOfTheWeek)
			{
				nextRunTime = nextRunTime.AddDays(1);
			}

			return nextRunTime;
		}
	}
}

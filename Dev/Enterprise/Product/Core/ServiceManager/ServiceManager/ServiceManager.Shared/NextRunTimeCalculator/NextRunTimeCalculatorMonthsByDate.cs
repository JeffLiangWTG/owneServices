using System;
using System.Globalization;
using System.Xml.Serialization;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Shared
{
	public record NextRunTimeCalculatorMonthsByDate : INextRunTimeCalculator
	{
		[XmlAttribute]
		public int Period { get; set; }

		[XmlAttribute]
		public int DayOfOccurrence { get; set; }

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
				.AddDays(1 - nextRunTime.Day)
				.Subtract(nextRunTime.TimeOfDay)
				.Add(ScheduledRunTime)
				.AddDays(DayOfOccurrence - 1);

			if (nextRunTime >= calculateFrom)
			{
				return nextRunTime;
			}

			nextRunTime = nextRunTime
				.AddDays(1 - nextRunTime.Day)
				.AddMonths(Period);

			while (DayOfOccurrence > DateTime.DaysInMonth(nextRunTime.Year, nextRunTime.Month))
			{
				nextRunTime = nextRunTime.AddMonths(Period);
			}

			return nextRunTime.AddDays(DayOfOccurrence - 1);
		}
	}
}

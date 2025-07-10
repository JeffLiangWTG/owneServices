using System;
using System.Globalization;
using System.Xml.Serialization;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Shared
{
	public record NextRunTimeCalculatorWorkingDays : INextRunTimeCalculator
	{
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
				.Subtract(nextRunTime.TimeOfDay)
				.Add(ScheduledRunTime);

			if (nextRunTime < calculateFrom)
			{
				nextRunTime = nextRunTime.AddDays(1);
			}

			while (nextRunTime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
			{
				nextRunTime = nextRunTime.AddDays(1);
			}

			return nextRunTime;
		}
	}
}

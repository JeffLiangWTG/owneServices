using System;
using System.Globalization;
using System.Xml.Serialization;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Shared
{
	public record NextRunTimeCalculatorYearsByDate : INextRunTimeCalculator
	{
		[XmlAttribute]
		public int Month { get; set; }

		[XmlAttribute]
		public int Day { get; set; }

		[XmlIgnore]
		public TimeSpan ScheduledRunTime { get; set; }

		[XmlAttribute(AttributeName = nameof(ScheduledRunTime))]
		public string ScheduledRunTimeString
		{
			get => ScheduledRunTime.ToString(ScheduleConfig.TimeFormat);
			set => ScheduledRunTime = TimeSpan.ParseExact(value, ScheduleConfig.TimeFormat, CultureInfo.InvariantCulture);
		}

		public DateTimeOffset CalculateNextRunTime(DateTimeOffset calculateFrom)
		{
			var nextRunTime = calculateFrom
				.ToUniversalTime();

			nextRunTime = nextRunTime
				.AddMonths(1 - nextRunTime.Month)
				.AddDays(1 - nextRunTime.Day)
				.Subtract(nextRunTime.TimeOfDay)
				.Add(ScheduledRunTime);

			if (Month == 2 && Day == 29 && !DateTime.IsLeapYear(nextRunTime.Year))
			{
				nextRunTime = nextRunTime
					.AddMonths(1)
					.AddDays(27);
			}
			else
			{
				nextRunTime = nextRunTime
					.AddMonths(Month - 1)
					.AddDays(Day - 1);
			}

			if (nextRunTime < calculateFrom)
			{
				nextRunTime = nextRunTime.AddYears(1);

				if (Month == 2 && Day == 29)
				{
					while (!DateTime.IsLeapYear(nextRunTime.Year))
					{
						nextRunTime = nextRunTime.AddYears(1);
					}

					nextRunTime = nextRunTime.AddDays(1);
				}
			}

			return nextRunTime;
		}
	}
}

using System;
using System.Globalization;
using System.Xml.Serialization;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Shared
{
	public record NextRunTimeCalculatorSeconds : INextRunTimeCalculator
	{
		[XmlAttribute]
		public int Period { get; set; }

		[XmlIgnore]
		public TimeSpan? StartTime { get; set; }

		[XmlAttribute(AttributeName = nameof(StartTime))]
		public string? StartTimeString
		{
			get => StartTime?.ToString(ScheduleConfig.TimeFormat, CultureInfo.InvariantCulture);
			set => StartTime = value != null ? TimeSpan.ParseExact(value, ScheduleConfig.TimeFormat, CultureInfo.InvariantCulture) : null;
		}

		[XmlIgnore]
		public TimeSpan? EndTime { get; set; }

		[XmlAttribute(AttributeName = nameof(EndTime))]
		public string? EndTimeString
		{
			get => EndTime?.ToString(ScheduleConfig.TimeFormat, CultureInfo.InvariantCulture);
			set => EndTime = value != null ? TimeSpan.ParseExact(value, ScheduleConfig.TimeFormat, CultureInfo.InvariantCulture) : null;
		}

		TimeSpan GetStartTimeSafe() => StartTime ?? TimeSpan.Zero;

		TimeSpan GetEndTimeSafe() => EndTime ?? TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)).Add(TimeSpan.FromSeconds(59));

		public DateTimeOffset CalculateNextRunTime(DateTimeOffset calculateFrom)
		{
			var safeStartTime = GetStartTimeSafe();
			var safeEndTime = GetEndTimeSafe();

			var nextRunTime = calculateFrom
				.ToUniversalTime()
				.AddSeconds(Period);

			if (safeStartTime <= safeEndTime)
			{
				if (nextRunTime.TimeOfDay < safeStartTime)
				{
					return nextRunTime
						.Subtract(nextRunTime.TimeOfDay)
						.Add(safeStartTime);
				}

				if (nextRunTime.TimeOfDay <= safeEndTime)
				{
					return nextRunTime;
				}

				return nextRunTime
					.AddDays(1)
					.Subtract(nextRunTime.TimeOfDay)
					.Add(safeStartTime);
			}

			if ((nextRunTime.TimeOfDay >= safeStartTime && nextRunTime.TimeOfDay <= TimeSpan.FromDays(1))
				|| (nextRunTime.TimeOfDay <= safeEndTime && nextRunTime.TimeOfDay >= TimeSpan.Zero))
			{
				return nextRunTime;
			}

			return nextRunTime
				.Subtract(nextRunTime.TimeOfDay)
				.Add(safeStartTime);
		}
	}
}

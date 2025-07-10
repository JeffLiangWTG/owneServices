using System;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Shared
{
	public record NextRunTimeCalculatorWeeks : INextRunTimeCalculator
	{
		[XmlAttribute]
		public int Period { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "This is a record type used for serialization needs to be a non complex type")]
		[XmlArray]
		public DayOfWeek[] DaysOfOccurrence { get; set; } = [];

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

			if (DaysOfOccurrence.Length == 0)
			{
				return nextRunTime;
			}

			nextRunTime = nextRunTime
				.Subtract(nextRunTime.TimeOfDay)
				.Add(ScheduledRunTime);

			if (DaysOfOccurrence.Contains(nextRunTime.DayOfWeek) && nextRunTime >= calculateFrom)
			{
				return nextRunTime;
			}

			if (nextRunTime.DayOfWeek == DaysOfOccurrence.OrderBy(day => ((int)day + 6) % 7).Last()) // assign Sunday as the last day of the week (as per Service Task UI)
			{
				nextRunTime = nextRunTime.AddDays((Period - 1) * 7);
			}

			nextRunTime = nextRunTime.AddDays(1);

			while (!DaysOfOccurrence.Contains(nextRunTime.DayOfWeek))
			{
				nextRunTime = nextRunTime.AddDays(1);
			}

			return nextRunTime;
		}

		public virtual bool Equals(NextRunTimeCalculatorWeeks? obj) =>
			obj != null
			&& obj is NextRunTimeCalculatorWeeks
			&& obj.Period == Period
			&& obj.DaysOfOccurrence.SequenceEqual(DaysOfOccurrence)
			&& obj.ScheduledRunTime == ScheduledRunTime;

		public override int GetHashCode() => base.GetHashCode();
	}
}

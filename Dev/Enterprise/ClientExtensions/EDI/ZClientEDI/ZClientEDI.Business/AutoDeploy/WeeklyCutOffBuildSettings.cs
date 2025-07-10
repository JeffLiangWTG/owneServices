using System;
using System.Globalization;
using CargoWise.Common;

namespace Enterprise.Client.EDI.AutoDeploy
{
	public class WeeklyCutOffBuildSettings
	{
		DayOfWeek dayOfWeek;
		DateTime buildCutOffTimeValue;

		public bool IsRequired
		{
			get
			{
				if (!EDIDataRegistry.Instance.EnableWeeklyBuildCutOff.Value)
				{
					return false;
				}

				if (!Enum.TryParse(EDIDataRegistry.Instance.WeeklyBuildCutOffDay.Value, out dayOfWeek))
				{
					ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Wrong EDataRegistry configuration: {0} is enabled but a wrong value is set for {1}", EDIDataRegistry.Instance.EnableWeeklyBuildCutOff.Name, EDIDataRegistry.Instance.WeeklyBuildCutOffDay.Name));
					return false;
				}

				if (!DateTime.TryParse(EDIDataRegistry.Instance.WeeklyBuildCutOffTime.Value, out buildCutOffTimeValue))
				{
					ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Wrong EDataRegistry configuration: {0} is enabled but a wrong value is set for {1}", EDIDataRegistry.Instance.EnableWeeklyBuildCutOff.Name, EDIDataRegistry.Instance.WeeklyBuildCutOffTime.Name));
					return false;
				}

				return true;
			}
		}

		public DayOfWeek? DayOfWeek
		{
			get
			{
				if (!IsRequired)
				{
					return null;
				}

				return dayOfWeek;
			}
		}

		public TimeSpan? TimeOfDay
		{
			get
			{
				if (!IsRequired)
				{
					return null;
				}

				return buildCutOffTimeValue.TimeOfDay;
			}
		}

		public DateTime GetLatestCutOffDateTime(DateTime currentBuildExeDate)
		{
			if (!IsRequired)
			{
				return DateTime.MinValue;
			}

			var delta = currentBuildExeDate.DayOfWeek - DayOfWeek.Value;

			if (delta < 0)
			{
				delta += 7;
			}

			var date = currentBuildExeDate.AddDays(-delta).Date;

			if (currentBuildExeDate.TimeOfDay >= TimeOfDay)
			{
				return date + TimeOfDay.Value;
			}
			else
			{
				//if the day of week coincides with the current build exe date but the time exceeds it then substruct 1 week
				return (date == currentBuildExeDate.Date
										? date.AddDays(-7).Date
										: date) + TimeOfDay.Value;
			}
		}
	}
}

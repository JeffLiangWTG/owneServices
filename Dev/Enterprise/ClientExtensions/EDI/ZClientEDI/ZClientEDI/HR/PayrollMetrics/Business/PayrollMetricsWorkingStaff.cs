using System;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.HR.PayrollMetrics
{
	internal class PayrollMetricsWorkingStaff : WorkingStaff
	{
		public PayrollMetricsWorkingStaff(EDIGlbStaff staff)
			: base(staff)
		{
		}

		const string PayrollLeaveLongService = "Long Service Leave";

		internal decimal CalculateDays(Leave leave)
		{
			return (leave.LeavePayElement != PayrollLeaveLongService)
				? ConvertHoursToDays(leave)
				: CountWorkingDays(leave);
		}

		int CountWorkingDays(Leave leave)
		{
			int days = 0;
			DateTime startDay = leave.DateFrom.Date;
			DateTime endDay = leave.DateTo.Date;
			while (startDay <= endDay)
			{
				if (IsWorkDay(startDay))
				{
					++days;
				}
				startDay = startDay.AddDays(1);
			}
			if (days == 0)
			{
				days = 1;
			}

			return days;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		internal decimal ConvertHoursToDays(Leave leave)
		{
			decimal days = 0;

			if (!leave.IsPartDayLeave)
			{
				days = CountWorkingDays(leave);
			}
			else
			{
				DateTime startDay = leave.DateFrom.Date;
				DateTime endDay = leave.DateTo.Date;
				decimal hours = leave.HoursOrWeeks;
				while (startDay <= endDay && hours > 0)
				{
					var workingTimeForDay = TimeDifference(startDay, startDay.AddMinutes(23 * 60 + 59));
					var workingHoursForDay = (decimal)workingTimeForDay.TotalHours;
					if (workingHoursForDay >= hours)
					{
						days += Utilities.Round(hours / workingHoursForDay, 2);
						hours = 0;
					}
					else if (workingHoursForDay > 0)
					{
						++days;
						hours -= workingHoursForDay;
					}
					startDay = startDay.AddDays(1);
				}
				if (days == 0)
				{
					days = hours / 8;
				}
			}

			return days;
		}
	}
}

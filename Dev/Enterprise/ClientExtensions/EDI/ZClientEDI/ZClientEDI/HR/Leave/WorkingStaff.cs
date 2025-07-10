using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.CalendarArithmetic;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.HR
{
	public class WorkingStaff
	{
		public WorkingStaff(EDIGlbStaff staff)
		{
			Staff = staff;
			DataSource = new CalendarArithmeticDataSource(staff.Factory, staff.GS_GE_HomeDepartment, staff.GS_GB_HomeBranch, staff.PK, false);
			DataSource = new CalendarSourceWithoutStaffHoliday(DataSource);
			DataSource = new CachedCalendarDataSource(DataSource);
			WorkTime = new WorkTimeArithmetic(DataSource);
		}

		public readonly EDIGlbStaff Staff;

		readonly WorkTimeArithmetic WorkTime;
		readonly ICalendarDataSource DataSource;

		internal TimeSpan TimeDifference(DateTime start, DateTime end)
		{
			return WorkTime.TimeDifference(start, end);
		}

		internal bool IsWorkDay(DateTime startDay)
		{
			return WorkTime.IsWorkDay(startDay);
		}

		/// <summary>
		/// Normal working hours for the date.
		/// Zero if not a work day.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public decimal TotalWorkingHours(DateTime date)
		{
			var workWeek = DataSource.StaffWorkTimeWeek;
			if (workWeek == null)
			{
				return 0m;
			}

			var workDay = workWeek.GetDay(date.DayOfWeek);
			var hours = workDay.Hours;
			decimal total = 0;

			for (int i = 0; i < hours.Count; ++i)
			{
				if (hours[i])
				{
					total += 0.5m;
				}
			}

			return total;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public DateTime AddWorkingHours(DateTime from, decimal workingHoursToAdd)
		{
			var result = from;
			if (workingHoursToAdd != 0m)
			{
				int halfHours;
				double inc;
				if (workingHoursToAdd > 0)
				{
					halfHours = (int)(workingHoursToAdd * 2);
					inc = 0.5;
				}
				else
				{
					halfHours = (int)(-workingHoursToAdd * 2);
					inc = -0.5;
				}

				while (halfHours-- > 0)
				{
					result = result.AddHours(inc);
					while (!WorkTime.IsWorkDateTime(result))
					{
						result = result.AddHours(inc);
					}
				}
			}

			return result;
		}

		internal DateTime StartWorkTime(DateTime date)
		{
			var workWeek = DataSource.StaffWorkTimeWeek;
			if (workWeek == null)
			{
				return date;
			}

			var workDay = workWeek.GetDay(date.DayOfWeek);
			var result = date.Date;
			var hours = workDay.Hours;
			for (int i = 0; i < hours.Count; ++i)
			{
				if (hours[i])
				{
					result = result.AddMinutes(i * 30);
					break;
				}
			}

			return result;
		}

		internal DateTime EndWorkTime(DateTime date)
		{
			var workWeek = DataSource.StaffWorkTimeWeek;
			var day = date.Date;
			if (workWeek != null)
			{
				var workDay = workWeek.GetDay(date.DayOfWeek);
				var hours = workDay.Hours;
				for (int i = hours.Count; --i >= 0;)
				{
					if (hours[i])
					{
						return day.AddMinutes((i + 1) * 30);
					}
				}
			}

			return day.AddMinutes(23 * 60 + 59);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		internal decimal ConvertHoursToDays(DateTime startDay, DateTime endDay, decimal totalHours)
		{
			decimal days = 0;

			decimal hours = totalHours;
			while (startDay <= endDay && hours > 0)
			{
				var workingHoursForDay = TotalWorkingHours(startDay);
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

			return days;
		}

		class CalendarSourceWithoutStaffHoliday : ICalendarDataSource
		{
			public CalendarSourceWithoutStaffHoliday(ICalendarDataSource data)
			{
				source = data;
			}
			readonly ICalendarDataSource source;

			public WorkTimeWeek StaffWorkTimeWeek => source.StaffWorkTimeWeek;
			public WorkTimeWeek DepartmentWorkTimeWeek => source.DepartmentWorkTimeWeek;
			public ICollection<DateTime> GetBranchHolidayDatesByYear(int year) => source.GetBranchHolidayDatesByYear(year);
			public DateTimeRange GetStaffHolidayForDateTime(DateTime dateToCheck) => null;
			public DateTimeRange GetStaffHolidayForDay(DateTime dayToCheck) => null;
			public IEnumerable<DateTimeRange> GetStaffHolidaysFromRange(DateTimeRange range) => Enumerable.Empty<DateTimeRange>();
		}
	}
}

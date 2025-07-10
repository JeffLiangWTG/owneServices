using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Macro
{
	public static class AddDurationToDateHelper
	{
		public const int MaxHolidaysForwardCount = 366;

		public static ZDateTime AddDurationToDate(ZDateTime odate, string span, string value, string workDate, string branch)
		{
			if (odate.IsEmpty)
			{
				throw new MacroRuntimeException(string.Format(CultureInfo.InvariantCulture, "Date is empty."));
			}
			if (string.IsNullOrEmpty(span))
			{
				throw new MacroRuntimeException(string.Format(CultureInfo.InvariantCulture, "Span is empty."));
			}
			if (string.IsNullOrEmpty(value))
			{
				throw new MacroRuntimeException(string.Format(CultureInfo.InvariantCulture, "Value is empty."));
			}

			var date = odate.ToDateTime();
			var onlyWorkDay = CheckOnlyWorkDay(workDate);
			var spanUpper = span.ToUpper();

			if (onlyWorkDay && (spanUpper == TimeUnits.Hours || spanUpper == TimeUnits.Minutes || spanUpper == TimeUnits.Seconds || spanUpper == TimeUnits.Milliseconds))
			{
				throw new MacroRuntimeException(string.Format(CultureInfo.InvariantCulture, "The 'WORKDAY' option is only valid when using 'DAYS' as the time span unit."));
			}

			if (int.TryParse(value, out var duration))
			{
				if (duration == 0)
				{
					return date;
				}
				var newDate = spanUpper switch
				{
					TimeUnits.Days => onlyWorkDay ? AddOnlyWorkDay(date, duration, branch) : date.AddDays(duration),
					TimeUnits.Hours => date.AddHours(duration),
					TimeUnits.Minutes => date.AddMinutes(duration),
					TimeUnits.Seconds => date.AddSeconds(duration),
					TimeUnits.Milliseconds => date.AddMilliseconds(duration),
					_ => throw new MacroRuntimeException(string.Format(CultureInfo.InvariantCulture, "'{0}' is not a valid time span unit.", span))
				};

				return newDate;
			}
			else
			{
				throw new MacroRuntimeException(string.Format(CultureInfo.InvariantCulture, "'{0}' is not a valid number.", value));
			}
		}

		static bool CheckOnlyWorkDay(string workDay)
		{
			workDay = workDay?.ToUpper();

			if (workDay == "Y")
			{
				return true;
			}

			if (string.IsNullOrEmpty(workDay) || workDay == "N")
			{
				return false;
			}

			throw new MacroRuntimeException(string.Format(CultureInfo.InvariantCulture, "The WorkDay must be 'Y' or 'N'."));
		}

		static ZDateTime AddOnlyWorkDay(DateTime date, int duration, string branch)
		{
			var remaining = Math.Abs(duration);
			var holidays = GetHoliday(branch);
			var forward = duration > 0;

			date = ForwardHolidays(date, holidays, !forward);
			while (remaining > 0)
			{
				date = forward ? date.AddDays(1) : date.AddDays(-1);
				remaining -= 1;
				date = ForwardHolidays(date, holidays, forward);
			}
			return date;
		}

		static DateTime ForwardHolidays(DateTime date, GlbHolidayDependentCollection holidays, bool forward)
		{
			var increment = 0;
			while (IsNonWorkDay(date, holidays))
			{
				if (increment > MaxHolidaysForwardCount)
				{
					throw new DeveloperNotificationException(string.Format(CultureInfo.InvariantCulture, "Holiday setting error: exceeded maximum allowed iterations while checking holidays."));
				}
				date = forward ? date.AddDays(1) : date.AddDays(-1);
				increment++;
			}
			return date;
		}

		static bool IsNonWorkDay(DateTime dateTime, GlbHolidayDependentCollection holidays)
		{
			var isWeekend = dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday;
			var isHoliday = holidays.Contains(new ZDate(dateTime));
			return isWeekend || isHoliday;
		}

		static GlbHolidayDependentCollection GetHoliday(string branch)
		{
			var glbBranch = string.IsNullOrEmpty(branch) ? GlbBranch.CurrentBranch : GetGlbBranch(branch);
			return glbBranch == null
				? throw new MacroRuntimeException(string.Format(CultureInfo.InvariantCulture, "'{0}' is not a valid branch code.", branch))
				: glbBranch.GlbHolidays;
		}

		static GlbBranch GetGlbBranch(string branch)
		{
			return new BusinessObjectFactory().LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, branch);
		}

		static class TimeUnits
		{
			public const string Days = "DAYS";
			public const string Hours = "HOURS";
			public const string Minutes = "MINUTES";
			public const string Seconds = "SECONDS";
			public const string Milliseconds = "MILLISECONDS";
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public static class ZDateTimeExtensions
	{
		/// <summary>
		/// Friday.AddWeekDays(1) = Monday
		/// Saturday.AddWeekDays(0) = Monday
		/// Saturday.AddWeekDays(1) = Tuesday
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="workingDaysToAdd"></param>
		/// <returns></returns>
		public static ZDateTime AddWorkingDaysFromNearestWorkingDay(this ZDateTime dateTime, int workingDaysToAdd,
			IEnumerable<ZDateTime> holidaysList)
		{
			var result = dateTime;
			if (dateTime.IsValid)
			{
				var startDate = workingDaysToAdd >= 0 ? dateTime.GetNearestWorkingdayAfter(holidaysList) : dateTime.GetNearestWorkingdayBefore(holidaysList);
				result = AddWorkingDays(startDate, workingDaysToAdd, holidaysList);
			}
			return result;
		}

		static ZDateTime AddWorkingDays(ZDateTime startDate, int workingDaysToAdd, IEnumerable<ZDateTime> holidaysList)
		{
			var result = startDate;
			if (result.IsValid)
			{
				result = startDate.AddDays(workingDaysToAdd / 5 * 7);
				var days = workingDaysToAdd % 5;
				var sign = workingDaysToAdd > 0 ? 1 : -1;
				var count = 0;
				while (count < Math.Abs(days))
				{
					result = result.AddDays(sign);
					if (result.DayOfWeek != DayOfWeek.Sunday && result.DayOfWeek != DayOfWeek.Saturday)
					{
						count++;
					}
				}

				var holidaysInRange = CountHolidysInRange(startDate, result, holidaysList);
				if (holidaysInRange != 0)
				{
					result = AddWorkingDays(result, holidaysInRange * sign, holidaysList);
				}
			}
			return result;
		}

		/// <summary>
		/// Friday.GetCountOfWorkingDaysTo(Saturday/Sunday) = 0;
		/// Friday.GetCountOfWorkingDaysTo(Monday) = 1;
		/// Saturday/Sunday.GetCountOfWorkingDaysTo(Monday) = 0;
		/// </summary>
		/// <param name="fromDateTime"></param>
		/// <param name="toDateTime"></param>
		/// <returns></returns>
		public static ZInt GetCountOfWorkingDaysTo(this ZDateTime fromDateTime, ZDateTime toDateTime, IEnumerable<ZDateTime> holidaysList)
		{
			var result = ZInt.Zero;
			var sign = fromDateTime.Date <= toDateTime.Date ? 1 : -1;
			if (fromDateTime.IsValid && toDateTime.IsValid)
			{
				var fromDate = (sign > 0 ? fromDateTime : toDateTime).GetNearestWorkingdayAfter(holidaysList).Date;
				var toDate = (sign > 0 ? toDateTime : fromDateTime).GetNearestWorkingdayBefore(holidaysList).Date;

				if (fromDate < toDate)
				{
					var days = (toDate - fromDate).Days;

					result += days / 7 * 5;
					days = days % 7;

					for (int idx = 1; idx <= Math.Abs(days); idx++)
					{
						var dayOfWeek = fromDate.AddDays(idx).DayOfWeek;
						if (dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday)
						{
							result++;
						}
					}
				}
				result -= CountHolidysInRange(fromDate, toDate, holidaysList);
			}
			return result * sign;
		}

		public static ZDateTime GetNearestWorkingdayBefore(this ZDateTime dateTime, IEnumerable<ZDateTime> holidaysList)
		{
			var result = dateTime;
			if (result.IsValid)
			{
				while (!result.IsWorkingDay(holidaysList))
				{
					result = result.AddDays(-1);
				}
			}
			return result;
		}

		public static ZDateTime GetNearestWorkingdayAfter(this ZDateTime dateTime, IEnumerable<ZDateTime> holidaysList)
		{
			var result = dateTime;
			if (result.IsValid)
			{
				while (!result.IsWorkingDay(holidaysList))
				{
					result = result.AddDays(1);
				}
			}
			return result;
		}

		internal static bool IsWorkingDay(this ZDateTime dateTime, IEnumerable<ZDateTime> holidaysList)
		{
			bool result = false;
			if (dateTime.IsValid)
			{
				var dayOfWeek = dateTime.DayOfWeek;
				result = !(dayOfWeek == DayOfWeek.Sunday || dayOfWeek == DayOfWeek.Saturday || dateTime.IsFederalHoliday(holidaysList));
			}
			return result;
		}

		static bool IsFederalHoliday(this ZDateTime dateTime, IEnumerable<ZDateTime> holidaysList)
		{
			var result = false;
			if (dateTime.IsValid)
			{
				result = holidaysList.Any(holiday => holiday.Date == dateTime.Date);
			}
			return result;
		}

		static int CountHolidysInRange(ZDateTime date1, ZDateTime date2, IEnumerable<ZDateTime> holidaysList)
		{
			var fromDate = date2 > date1 ? date1 : date2;
			var toDate = date2 > date1 ? date2 : date1;
			return holidaysList.Count(holiday => holiday > fromDate && holiday <= toDate);
		}
	}
}

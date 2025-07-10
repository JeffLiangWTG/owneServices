using System;
using System.Data;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	static class GenericDateTypeHelper
	{
		internal static bool IsRangeExceedingMaxTimeSpan(IZDate valueLow, IZDate valueHigh, int dateRangeMaxYears, int dateRangeMaxMonths)
		{
			return (valueLow.Year + dateRangeMaxYears + (dateRangeMaxMonths / 12) < DateTime.MaxValue.Year) && valueLow.AddYears(dateRangeMaxYears).AddMonths(dateRangeMaxMonths).CompareTo(valueHigh) < 0;
		}

		internal static TBaseType AddMinutesForDate<TBaseType>(TBaseType date, double minutes)
		{
			if (date is DateTime valueAsDateTime)
			{
				return (TBaseType)(object)valueAsDateTime.AddMinutes(minutes);
			}
			else if (date is DateTimeOffset valueAsDateTimeOffset)
			{
				return (TBaseType)(object)valueAsDateTimeOffset.AddMinutes(minutes);
			}
			else
			{
				throw new ArgumentException();
			}
		}

		internal static TBaseType AddDaysForDate<TBaseType>(TBaseType date, double days)
		{
			if (date is DateTime valueAsDateTime)
			{
				return (TBaseType)(object)valueAsDateTime.AddDays(days);
			}
			else if (date is DateTimeOffset valueAsDateTimeOffset)
			{
				return (TBaseType)(object)valueAsDateTimeOffset.AddDays(days);
			}
			else
			{
				throw new ArgumentException();
			}
		}

		internal static TZType GetDateFromSchedule<TZType>(DateSchedule schedule)
		{
			if (typeof(TZType) == typeof(ZDateTime))
			{
				return (TZType)(object)schedule.GetScheduleDate();
			}
			else if (typeof(TZType) == typeof(ZDateTimeOffset))
			{
				return (TZType)(object)schedule.GetScheduleDate().ToOffset();
			}
			else
			{
				throw new ArgumentException();
			}
		}

		internal static SqlDbType GetSqlDbType<TBaseType>()
		{
			if (typeof(TBaseType) == typeof(DateTime))
			{
				return SqlDbType.DateTime;
			}
			else if (typeof(TBaseType) == typeof(DateTimeOffset))
			{
				return SqlDbType.DateTimeOffset;
			}
			else
			{
				throw new ArgumentException();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:Do Not Use DateTime Parse Method", Justification = "<Pending>")]
		internal static TBaseType ParseBaseDate<TBaseType>(string value, IFormatProvider provider)
		{
			if (typeof(TBaseType) == typeof(DateTime))
			{
				return (TBaseType)(object)DateTime.Parse(value, provider);
			}
			else if (typeof(TBaseType) == typeof(DateTimeOffset))
			{
				return (TBaseType)(object)DateTimeOffset.Parse(value, provider);
			}
			else
			{
				throw new ArgumentException();
			}
		}

		internal static IZDate ToZDateTimeOrZDateTimeOffset(object dateTimeOrDateTimeOffset)
		{
			if (dateTimeOrDateTimeOffset == null)
			{
				return default;
			}
			return ZDataType.ObjectToZType(dateTimeOrDateTimeOffset) as IZDate;
		}

		internal static DateTime ToDateTime<TBaseType>(TBaseType dateTimeOrDateTimeOffset)
		{
			if (dateTimeOrDateTimeOffset is DateTime valueAsDateTime)
			{
				return valueAsDateTime;
			}
			else if (dateTimeOrDateTimeOffset is DateTimeOffset valueAsDateTimeOffset)
			{
				if (((ZDateTimeOffset)valueAsDateTimeOffset).IsEmpty || !((ZDateTimeOffset)valueAsDateTimeOffset).IsValid)
				{
					return DateTime.MinValue;
				}
				return ((ZDateTimeOffset)valueAsDateTimeOffset).ToDateTime();
			}
			else
			{
				throw new ArgumentException();
			}
		}

		internal static string ToStringWithDateFormatFromDate<TBaseType>(TBaseType dateTimeOrZDateTimeOffset, string dateFormat, IFormatProvider provider = null)
		{
			if (dateTimeOrZDateTimeOffset is DateTime valueAsDateTime)
			{
				return valueAsDateTime.ToString(dateFormat, provider);
			}
			else if (dateTimeOrZDateTimeOffset is DateTimeOffset valueAsDateTimeOffset)
			{
				return valueAsDateTimeOffset.ToString(dateFormat, provider);
			}
			else
			{
				throw new ArgumentException();
			}
		}
	}
}

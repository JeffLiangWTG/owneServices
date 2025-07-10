/// <summary>
/// A ZDateTimeOffsetValueNegatableAttribute is used for offset values which have a Duration Epoch of (2000, 1, 1).
/// The first half of the year represents the positive offset value, like (2000, 1, 2) shows the offset of (2000, 1, 2) - (2000, 1, 1) = 24 hours.
/// The second half of the year represents the negative value, calculated from the reverse duration backward count from (2000, 1, 1), like (1999, 12, 31) shows the offset of (1999, 12, 31) - (2000, 1, 1) = -24 hours.
/// </summary>
using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public sealed class ZDateTimeOffsetValueNegatableAttribute : Attribute, IDurationBasedDateConverter
	{
		public ZDateTime ConvertToDurationBasedDate(ZDateTime date)
		{
			if (date.IsEmpty || !date.IsValid)
			{
				return date;
			}

			return ZDateTime.DefaultNegatableDurationEpoch + date.TimeSpan6MonthsFromStartOfYear;
		}
	}
}

/// <summary>
/// Attribute to convert a ZDateTime property to a duration-based date.
/// A duration value has a default duration epoch of (1900, 01, 01).
/// </summary>
using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
#pragma warning disable CA1813 // Avoid unsealed attributes
	public class ZDateTimeDurationValueAttribute : Attribute, IDurationBasedDateConverter
#pragma warning restore CA1813 // Avoid unsealed attributes
	{
		public virtual ZDateTime ConvertToDurationBasedDate(ZDateTime date)
		{
			if (date.IsEmpty || !date.IsValid)
			{
				return date;
			}

			var startOfYear = new ZDateTime(date.Year, 1, 1);
			var durationFromYearStart = date - startOfYear;
			return ZDateTime.DefaultDurationEpoch + durationFromYearStart;
		}
	}
}

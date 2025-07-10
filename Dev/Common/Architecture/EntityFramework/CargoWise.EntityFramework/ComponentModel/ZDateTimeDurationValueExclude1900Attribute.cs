using System;
/// <summary>
/// This attribute is used for duration properties that have a database column value constraint to not equal 1900-01-01.
/// If the date is equal to the default duration epoch (1900-01-01), it returns ZDateTime.Empty.
/// </summary>
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public sealed class ZDateTimeDurationValueExclude1900Attribute : ZDateTimeDurationValueAttribute
	{
		public override ZDateTime ConvertToDurationBasedDate(ZDateTime date)
		{
			var value = base.ConvertToDurationBasedDate(date);

			if (value == ZDateTime.DefaultDurationEpoch)
			{
				return ZDateTime.Empty;
			}

			return value;
		}
	}
}

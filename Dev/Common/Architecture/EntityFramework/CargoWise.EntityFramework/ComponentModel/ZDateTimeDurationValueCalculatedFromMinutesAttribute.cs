/// <summary>
/// This attribute is used for duration properties that have their value calculated from an integer minutes value.
/// The duration value is never invalid or empty.
/// </summary>
using System;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public sealed class ZDateTimeDurationValueCalculatedFromMinutesAttribute : ZDateTimeDurationValueAttribute
	{
	}
}

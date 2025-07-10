/// <summary>
/// This attribute is used for duration properties whose value is calculated from other properties when ZDateTime.Empty is assigned to the property.
/// </summary>
using System;

namespace CargoWise.EntityFramework
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public sealed class ZDateTimeDurationCalculatedOnEmptyAttribute : ZDateTimeDurationValueAttribute
	{
	}
}

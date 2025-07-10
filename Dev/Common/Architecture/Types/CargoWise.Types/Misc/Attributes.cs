using System;

namespace CargoWise.Types
{
	[AttributeUsage(AttributeTargets.Struct)]
	public sealed class ValueRangeAttribute : Attribute
	{
		public ValueRangeAttribute(long maxValue, long minValue)
		{
			this.maxValue = maxValue;
			this.minValue = minValue;
		}

		public long MaxValue
		{
			get { return maxValue; }
		}

		public long MinValue
		{
			get { return minValue; }
		}

		readonly long maxValue;
		readonly long minValue;
	}

	[AttributeUsage(AttributeTargets.Struct)]
	public sealed class HasDecimalsAttribute : Attribute
	{
		public HasDecimalsAttribute(bool hasDecimals)
		{
			this.hasDecimals = hasDecimals;
		}

		public bool HasDecimals
		{
			get { return hasDecimals; }
		}

		readonly bool hasDecimals;
	}
}

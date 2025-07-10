using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public sealed class GreaterThanOrEqualToDatePartComparisonOperator : ExactComparisonOperator
	{
		[System.Diagnostics.DebuggerHidden]
		public GreaterThanOrEqualToDatePartComparisonOperator() : base(">=", false)
		{
		}

		protected override object ValueAdjustedForComparisonOperatorCore(object value)
		{
			object result;
			if (value is DateTime)
			{
				result = ((DateTime)value).Date;
			}
			else if (value is ZDateTime)
			{
				ZDateTime temp = (ZDateTime)value;
				if (temp.IsValid)
				{
					temp = temp.Date;
				}
				result = temp;
			}
			else if (value is ZDate)
			{
				result = (ZDate)value;
			}
			else if (value is DateTimeOffset)
			{
				result = ZDateTimeOffset.DateAndOffsetHelper((DateTimeOffset)value);
			}
			else if (value is ZDateTimeOffset)
			{
				result = ((ZDateTimeOffset)value).DateAndOffset;
			}
			else
			{
				result = base.ValueAdjustedForComparisonOperatorCore(value);
			}
			return result;
		}
	}
}

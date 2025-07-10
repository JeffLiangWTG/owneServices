using System;
using CargoWise.Common.Testing;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public sealed class LessThanOrEqualToDatePartComparisonOperator : ExactComparisonOperator
	{
		[System.Diagnostics.DebuggerHidden]
		public LessThanOrEqualToDatePartComparisonOperator() : base("<", false)
		{
		}

		protected override object ValueAdjustedForComparisonOperatorCore(object value)
		{
			object result;
			if (value is DateTime)
			{
				result = GetDateOfNextDayOrMaxValue((DateTime)value);
			}
			else if (value is ZDateTime)
			{
				ZDateTime temp = (ZDateTime)value;
				if (temp.IsValid)
				{
					temp = new ZDateTime(GetDateOfNextDayOrMaxValue(temp.ToDateTime()));
				}
				result = temp;
			}
			else if (value is ZDate)
			{
				ZDate temp1 = (ZDate)value;
				if (temp1.IsValid)
				{
					temp1 = new ZDate(GetDateOfNextDayOrMaxValue(temp1.ToDateTime()));
				}
				result = temp1;
			}
			else if (value is DateTimeOffset)
			{
				result = GetDateAndOffsetOfNextDayOrMaxValue(((DateTimeOffset)value));
			}
			else if (value is ZDateTimeOffset)
			{
				ZDateTimeOffset temp = (ZDateTimeOffset)value;
				if (temp.IsValid)
				{
					result = new ZDateTimeOffset(GetDateAndOffsetOfNextDayOrMaxValue(temp.ToDateTimeOffset()));
				}
				else
				{
					result = temp;
				}
			}
			else
			{
				result = base.ValueAdjustedForComparisonOperatorCore(value);
			}

			return result;
		}

		[SuppressThreadStaticFieldMessage]
		static readonly DateTime maxSqlDateTimeValue = new DateTime(9999, 12, 31, 23, 59, 59, 990);

		DateTime GetDateOfNextDayOrMaxValue(DateTime source)
		{
			DateTime result = maxSqlDateTimeValue;
			if (source.Date.Ticks + new TimeSpan(1, 0, 0, 0, 0).Ticks <= maxSqlDateTimeValue.Ticks)
			{
				result = source.Date.AddDays(1);
			}
			return result;
		}

		DateTimeOffset GetDateAndOffsetOfNextDayOrMaxValue(DateTimeOffset source)
		{
			DateTimeOffset result = new DateTimeOffset(maxSqlDateTimeValue, TimeSpan.Zero);
			if (source.Date.Ticks + new TimeSpan(1, 0, 0, 0, 0).Ticks <= maxSqlDateTimeValue.Ticks
			&& source.Date.Ticks + new TimeSpan(1, 0, 0, 0, 0).Ticks + source.Offset.Ticks <= maxSqlDateTimeValue.Ticks)
			{
				DateTime temp = source.Date.AddDays(1);
				result = new DateTimeOffset(temp.Year, temp.Month, temp.Day, 0, 0, 0, source.Offset);
			}
			return result;
		}
	}
}

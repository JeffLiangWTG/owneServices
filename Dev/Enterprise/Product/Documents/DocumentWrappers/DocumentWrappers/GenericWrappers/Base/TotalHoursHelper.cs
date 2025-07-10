using System;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class TotalHoursHelper
	{
		public ZString GetTextFromTime(ZDateTime time)
		{
			return time.IsValid ? GetTextFromTimeSpan(time.TimeSpan6MonthsFromStartOfYear) : ZString.Empty;
		}

		public ZString GetTextFromTimeSpan(TimeSpan timeSpan)
		{
			var result = Math.Abs((int)timeSpan.TotalHours).ToString() + ":" + Math.Abs(timeSpan.Minutes).ToString().PadLeft(MaximumMinutesLength, '0');

			if (timeSpan < TimeSpan.Zero)
			{
				result = "-" + result;
			}

			if (EmptyEquivalent(timeSpan) == result)
			{
				result = ZString.Empty;
			}

			return result;
		}

		ZString EmptyEquivalent(TimeSpan timeSpan)
		{
			return ZString.Empty.PadLeft(timeSpan.TotalHours.ToString().Length, '0') + ":00";
		}

		const int MaximumMinutesLength = 2;
	}
}

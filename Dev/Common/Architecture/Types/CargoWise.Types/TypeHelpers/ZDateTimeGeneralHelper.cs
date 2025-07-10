using System;
using System.Globalization;

namespace CargoWise.Types
{
	public static class ZDateTimeGeneralHelper
	{
		public static string GetTextFromTimeOffset(ZDateTime time, int maximumHours, bool allowNegative)
		{
			string result;
			if (time.IsValid)
			{
				var timeSpan = time.TimeSpan6MonthsFromStartOfYear;
				result = Math.Abs((int)timeSpan.TotalHours).ToString(CultureInfo.InvariantCulture).PadLeft(GetMaximumHoursLength(maximumHours), '0')
					+ ":" + Math.Abs(timeSpan.Minutes).ToString(CultureInfo.InvariantCulture).PadLeft(MaximumMinutesLength, '0');

				if (timeSpan < TimeSpan.Zero)
				{
					result = allowNegative ? "-" + result : string.Empty;
				}
			}
			else
			{
				result = time.ToString();
			}
			return result;
		}

		static int GetMaximumHoursLength(int maximumHours) => maximumHours.ToString().Length;

		const int MaximumMinutesLength = 2;
	}
}

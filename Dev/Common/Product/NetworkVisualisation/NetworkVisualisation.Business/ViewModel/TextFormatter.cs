using System;
using System.Globalization;

namespace CargoWise.NetworkVisualisation.Business
{
	public static class TextFormatter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public static string FormatHoursToTimeString(this decimal hoursValue)
		{
			var hours = Math.Truncate(hoursValue);
			var minutes = (hoursValue % (hours > 0 ? hours : 1)) * 60;

			return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", hours.ToString("0", CultureInfo.InvariantCulture), minutes.ToString("00", CultureInfo.InvariantCulture));
		}

		public static string FormatAsBestReadableDateTime(this DateTime dateTime)
		{
			return dateTime.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture);
		}
	}
}

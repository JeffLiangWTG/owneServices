using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.CustomerService.Business
{
	public static class ZDateTimeConversionHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Standard datetime format as \"yyyy-MM-ddTHH:mm:ss.fffffff\"")]
		public static ZDateTime ParseRoundTripFormatString(string value)
		{
			ZDateTime result = ZDateTime.Empty;
			if (!string.IsNullOrEmpty(value))
			{
				DateTime d;
				// Not using ZDateTime.TryParseExact as it doesn't understand "o" format and replaces the date with today's date.
				DateTime.TryParseExact(value, "o", Culture.Invariant, System.Globalization.DateTimeStyles.RoundtripKind, out d);
				result = d;
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "RoundTrip datetime format \"yyyy-MM-ddTHH:mm:ss.fffffff\", Standard datetime format as \"yyyy-MM-ddTHH:mm:ss.fffffff\"")]
		public static ZDateTime ForceParseRoundTripFormatStringAsUtc(string value)
		{
			ZDateTime result = ZDateTime.Empty;
			if (!string.IsNullOrEmpty(value))
			{
				if (!value.EndsWith("Z", true, Culture.Invariant))
				{
					value = value.Substring(0, "yyyy-MM-ddTHH:mm:ss.fffffff".Length) + "Z";
				}
				DateTime d;
				DateTime.TryParseExact(value, "o", Culture.Invariant, System.Globalization.DateTimeStyles.RoundtripKind, out d);
				result = d;
			}
			return result;
		}
	}
}

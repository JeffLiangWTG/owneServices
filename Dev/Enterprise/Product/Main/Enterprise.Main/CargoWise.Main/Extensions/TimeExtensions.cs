using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Main.Navigation;

[CodeAlive("It will be used by Recent Messages views in RDP and WEB (under construction)")]
static class TimeExtensions
{
	public static string ToFriendlyTimeAgoString(this DateTime utcDateTimeMessageReceived)
	{
		var zUtcDateTime = new ZDateTime(utcDateTimeMessageReceived.Year, utcDateTimeMessageReceived.Month, utcDateTimeMessageReceived.Day, utcDateTimeMessageReceived.Hour, utcDateTimeMessageReceived.Minute, utcDateTimeMessageReceived.Second, DateTimeKind.Utc);
		return zUtcDateTime.ToFriendlyTimeAgoString();
	}

	public static string ToFriendlyTimeAgoString(this ZDateTime utcDateTimeInThePast)
	{
		if (utcDateTimeInThePast.IsValid)
		{
			var now = ZDateTime.UtcNow;
			var difference = now - utcDateTimeInThePast;

			return difference.ToFriendlyTimeAgoString();
		}
		else
		{
			return ZString.Empty;
		}
	}

	public static string ToFriendlyTimeAgoString(this TimeSpan span)
	{
		var friendlyTimeString = span.ToFriendlyTimeString();

		if (span.TotalHours >= 1)
		{
			return Res.GetString("E4DFE3D9-59C6-4DEE-944A-23C05A640017", "{0} ago", friendlyTimeString);
		}
		else if (span.Minutes >= 1)
		{
			return Res.GetString("BB293649-26A7-4ADC-BB4C-151F3C2A2CD4", "{0} ago", friendlyTimeString);
		}
		else if (span.Minutes > -1)
		{
			return Res.GetString("D7BFBD0D-723D-4977-8B02-98FE6F79F73D", "just now");
		}
		else
		{
			return string.Empty;
		}
	}
}


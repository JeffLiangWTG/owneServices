using System;
using System.Globalization;
using CargoWise.Types;
using ServiceManager.Common.Abstractions;

namespace Enterprise.ServiceManager.Shared
{
	public static class ZDateTimeExtensions
	{
		public static string ToLoggerString(this ZDateTime time)
		{
			return time.ToString(LogStringFormats.LoggerTimeMask, CultureInfo.InvariantCulture);
		}

		public static DateTime? ToNullableDateTime(this ZDateTime time)
		{
			var timeCopy = time; //Take a snapshot to make it thread safe.
			return timeCopy.IsEmpty ? null : new DateTime?(timeCopy.ToDateTime());
		}

		public static DateTime? ToNullableDateTime(this ZDateTimeOffset time)
		{
			var timeCopy = time; //Take a snapshot to make it thread safe.
			return timeCopy.IsEmpty ? null : new DateTime?(timeCopy.ToDateTime());
		}

		public static DateTimeOffset? ToNullableDateTimeOffset(this ZDateTime time)
		{
			var timeCopy = time;
			return timeCopy.IsEmpty ? null : timeCopy.ToOffset().ToDateTimeOffsetSafe();
		}
	}
}

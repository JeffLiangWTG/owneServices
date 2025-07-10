using System;
using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public static class ZDateTimeExtensions
{
	public static DateTime? GetDateTime(this ZDateTime time)
	{
		return time.IsValid ? time.ToDateTime() : null;
	}

	public static ZString ToNACCSDate(this DateTime time)
	{
		return time == default ? ZString.Empty : time.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
	}

	public static ZString ToNACCSDateTime(this DateTime time)
	{
		return time == default ? ZString.Empty : time.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture);
	}

	public static ZString ToNACCSTime(this DateTime time)
	{
		return time == default ? ZString.Empty : time.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
	}
}


using System;

namespace Enterprise.Customs.DE.ExitControl.Business
{
	static class UniversalDateAndTimeExtensions
	{
		internal static DateTime ToUnspecified(this DateTime dateTime)
		{
			if (dateTime == default)
			{
				return default;
			}
			return DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
		}
	}
}

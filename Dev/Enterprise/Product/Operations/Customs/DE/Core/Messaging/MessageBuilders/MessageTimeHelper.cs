using System;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public static class MessageTimeHelper
	{
		public static DateTime SafeDateTime(this ZDateTime dateTime) => dateTime.IsValid ? dateTime.ToDateTime() : default;

		public static DateTime SafeDate(this ZDate date) => date.IsValid ? date.ToDateTime().Date : default;

		public static DateTime ZeroFromSecond(this DateTime dateTime) => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);

		public static ZDateTime ZeroFromSecond(this ZDateTime dateTime) => dateTime.IsValid ? new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0) : dateTime;

		public static DateTime? ToNullableDateTime(this ZDateTime dateTime) => dateTime.IsValid ? dateTime.ToDateTime() : null;

		public static DateTime? ToNullableDateTime(this ZDate date) => date.IsValid ? date.ToDateTime() : null;

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

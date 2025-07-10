using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.Shared
{
	public static class SuppressUtil
	{
		public static readonly ZString SuppressedText = "*SUPPRESSED*"; // Hard-coded constant
		public static readonly ZDateTime SuppressedDateTime = DateTime.MinValue.AddDays(1);
		public static readonly ZDateTimeOffset SuppressedDateTimeOffset = new ZDateTimeOffset(SuppressedDateTime);

		public static IZType GetSuppressedValue(IZType value)
		{
			if (value is ZDateTime)
			{
				return SuppressedDateTime;
			}
			if (value is ZDateTimeOffset)
			{
				return SuppressedDateTimeOffset;
			}
			if (value is ZString)
			{
				return SuppressedText;
			}

			return value;
		}

		public static ZString GetSuppressedValue(ZString value, bool isSuppressed)
		{
			return (isSuppressed && !value.IsEmpty) ? SuppressedText : value;
		}

		public static ZDateTime GetSuppressedValue(ZDateTime value, bool isSuppressed)
		{
			return (isSuppressed && !value.IsEmpty) ? SuppressedDateTime : value;
		}

		public static ZDateTimeOffset GetSuppressedValue(ZDateTimeOffset value, bool isSuppressed)
		{
			return (isSuppressed && !value.IsEmpty) ? SuppressedDateTimeOffset : value;
		}

		public static string GetFormattedDate(ZDateTimeOffset value, ZDateTimePickerFormat dateTimeFormat) => GetFormattedDate(value.ToZDateTime(), dateTimeFormat);

		public static string GetFormattedDate(ZDate value) => GetFormattedDate(value.ToZDateTime(), ZDateTimePickerFormat.Short);

		public static string GetFormattedDate(ZDateTime value, ZDateTimePickerFormat dateTimeFormat)
		{
			if (!value.IsEmpty)
			{
				if (value == SuppressedDateTime)
				{
					return SuppressedText;
				}

				return WebDateTimeFormatter.GetFormattedDate(value, dateTimeFormat);
			}

			return string.Empty;
		}
	}
}

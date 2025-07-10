using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Translatable date format")]
	public static class DateTimeOffsetFormatStrings
	{
		public static MultilingualString ShortDateFormat
		{
			get { return ResString.GetMultilingualString("DateTimeOffsetFormat|ShortDateFormat", "dd-MMM-yy"); } // Translatable date format
		}

		public static MultilingualString LongDateFormatIncludingWeek
		{
			get { return ResString.GetMultilingualString("DateTimeOffsetFormat|LongDateFormatIncludingWeek", "dddd, MMMM d, yyyy"); } // Translatable date format
		}

		public static MultilingualString ShortTimeFormat
		{
			get { return ResString.GetMultilingualString("DateTimeOffsetFormat|ShortTimeFormat", "HH:mm"); } // Translatable date format
		}

		public static MultilingualString ShortTimeIncludingSecondsFormat
		{
			get { return ResString.GetMultilingualString("DateTimeOffsetFormat|ShortTimeIncludingSecondsFormat", "HH:mm:ss"); } // Translatable date format
		}

		public static MultilingualString LongTimeFormat
		{
			get { return ResString.GetMultilingualString("DateTimeOffsetFormat|LongTimeFormat", "dd-MMM-yy HH:mm zzz"); } // Translatable date format
		}

		public static MultilingualString LongTimeFormatIncludingGMT
		{
			get { return ResString.GetMultilingualString("DateTimeOffsetFormat|LongTimeFormatIncludingGMT", "dd-MMM-yy HH:mm \"GMT\"zzz"); } // Translatable date format
		}

		public static MultilingualString LongTimeIncludingSecondsFormat
		{
			get { return ResString.GetMultilingualString("DateTimeOffsetFormat|LongTimeIncludingSecondsFormat", "dd-MMM-yy HH:mm:ss zzz"); } // Translatable date format
		}

		public static IEnumerable<MultilingualString> AllDateTimeFormats
		{
			get
			{
				yield return ResString.GetMultilingualString("DateTimeOffsetFormat|yyyy-MM-dd", "yyyy-MM-dd");
				yield return ShortDateFormat;
				yield return LongDateFormatIncludingWeek;
				yield return ShortTimeFormat;
				yield return ShortTimeIncludingSecondsFormat;
				yield return LongTimeFormat;
				yield return LongTimeFormatIncludingGMT;
				yield return LongTimeIncludingSecondsFormat;
				yield return ResString.GetMultilingualString("DateTimeOffsetFormat|dd-MM-yy", "dd-MM-yy");
				yield return ResString.GetMultilingualString("DateTimeOffsetFormat|dd-MMM-yy hh:mm tt zzz", "dd-MMM-yy hh:mm tt zzz");
				yield return ResString.GetMultilingualString("DateTimeOffsetFormat|dd-MMM HH:mm zzz", "dd-MMM HH:mm zzz");
				yield return ResString.GetMultilingualString("DateTimeOffsetFormat|dd-MMM-yyyy", "dd-MMM-yyyy"); // Translatable date format
				yield return ResString.GetMultilingualString("DateTimeOffsetFormat|dd-MMM", "dd-MMM"); // Translatable date format
				yield return ResString.GetMultilingualString("DateTimeOffsetFormat|dd/MM/yyyy)", "dd/MM/yyyy");
			}
		}

		public static string GetLocalizedFormatString(string formatString)
		{
			if (string.IsNullOrEmpty(formatString))
			{
				return LongTimeIncludingSecondsFormat;
			}

			foreach (var knownFormat in DateTimeFormatStrings.AllDateTimeFormats)
			{
				if (formatString.Equals(knownFormat.GetUnresolvedString()))
				{
					return knownFormat;
				}
			}

#if DEBUG
			if (throwExceptionOnUnknownDateFormatString)
			{
				throw new InvalidOperationException("Unknown date format string: " + formatString + ".\r\nPlease use one of the following date format strings:\r\n" + string.Join("\r\n", Array.ConvertAll(DateTimeFormatStrings.AllDateTimeFormats.ToArray(), item => item.GetUnresolvedString())));
			}
#endif
			return formatString;
		}

#if DEBUG
		public static IDisposable ThrowExceptionOnUnknownDateFormatString()
		{
			if (throwExceptionOnUnknownDateFormatString)
			{
				throw new InvalidOperationException("throwExceptionOnUnknownDateFormatString flag is already set");
			}
			throwExceptionOnUnknownDateFormatString = true;
			return new DisposableAction(delegate
				{
					throwExceptionOnUnknownDateFormatString = false;
				});
		}

#pragma warning disable CW1021
		static bool throwExceptionOnUnknownDateFormatString;
#pragma warning restore CW1021
#endif
	}
}

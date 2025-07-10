using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Translatable date format")]
	public static class DateTimeFormatStrings
	{
		public static MultilingualString ShortDateFormat
		{
			get { return ResString.GetMultilingualString("DateTimeFormat|ShortDateFormat", "dd-MMM-yy"); } // Translatable date format
		}

		public static MultilingualString LongDateFormatIncludingWeek
		{
			get { return ResString.GetMultilingualString("DateTimeFormat|LongDateFormatIncludingWeek", "dddd, MMMM d, yyyy"); } // Translatable date format
		}

		public static MultilingualString ShortTimeFormat
		{
			get { return ResString.GetMultilingualString("DateTimeFormat|ShortTimeFormat", "HH:mm"); } // Translatable date format
		}

		public static MultilingualString ShortTimeIncludingSecondsFormat
		{
			get { return ResString.GetMultilingualString("DateTimeFormat|ShortTimeIncludingSecondsFormat", "HH:mm:ss"); } // Translatable date format
		}

		public static MultilingualString LongTimeFormat
		{
			get { return ResString.GetMultilingualString("DateTimeFormat|LongTimeFormat", "dd-MMM-yy HH:mm"); } // Translatable date format
		}

		public static MultilingualString LongTimeFormatIncludingGMT
		{
			get { return ResString.GetMultilingualString("DateTimeFormat|LongTimeFormatIncludingGMT", "dd-MMM-yy HH:mm \"GMT\"zzz"); } // Translatable date format
		}

		public static MultilingualString LongTimeIncludingSecondsFormat
		{
			get { return ResString.GetMultilingualString("DateTimeFormat|LongTimeIncludingSecondsFormat", "dd-MMM-yy HH:mm:ss"); } // Translatable date format
		}

		public static IEnumerable<MultilingualString> AllDateTimeFormats
		{
			get
			{
				yield return ResString.GetMultilingualString("DateTimeFormat|yyyy-MM-dd", "yyyy-MM-dd");
				yield return ShortDateFormat;
				yield return LongDateFormatIncludingWeek;
				yield return ShortTimeFormat;
				yield return ShortTimeIncludingSecondsFormat;
				yield return LongTimeFormat;
				yield return LongTimeFormatIncludingGMT;
				yield return LongTimeIncludingSecondsFormat;
				yield return ResString.GetMultilingualString("DateTimeFormat|dd-MM-yy", "dd-MM-yy");
				yield return ResString.GetMultilingualString("DateTimeFormat|dd-MMM-yy hh:mm tt", "dd-MMM-yy hh:mm tt");
				yield return ResString.GetMultilingualString("DateTimeFormat|dd-MMM HH:mm", "dd-MMM HH:mm");
				yield return ResString.GetMultilingualString("DateTimeFormat|dd-MMM-yyyy", "dd-MMM-yyyy"); // Translatable date format
				yield return ResString.GetMultilingualString("DateTimeFormat|dd-MMM", "dd-MMM"); // Translatable date format
				yield return ResString.GetMultilingualString("DateTimeFormat|dd/MM/yyyy)", "dd/MM/yyyy");
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

		static bool throwExceptionOnUnknownDateFormatString;
#endif
	}
}

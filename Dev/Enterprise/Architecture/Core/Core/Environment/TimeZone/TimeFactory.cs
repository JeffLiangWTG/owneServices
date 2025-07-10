using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using CargoWise.Types;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Environment
{
	public class TimeFactory
	{
		public static TimeFactory Instance
		{
			get
			{
				if (instance == null)
				{
					Interlocked.CompareExchange(ref instance, new TimeFactory(), null);
				}
				return instance;
			}
		}

		[ThreadSafe]
		static TimeFactory instance;

		#region Current Date/Time

		public void RefreshCacheNow() => zoneCollection.RefreshCacheNow();
		public void SetCacheToNeverExpire() => zoneCollection.CacheNeverExpires = true;

		#region Local Time

		public DateTime CurrentLocalDateTime
		{
			get
			{
				string localUnloco = GetCurrentBranchUnloco();
				DateTime result = GetUnlocoDateTime(localUnloco);
				return result;
			}
		}

		public DateTime CurrentLocalDate
		{
			get { return CurrentLocalDateTime.Date; }
		}

		#endregion

		#region UTC

		public DateTime CurrentUtcDateTime
		{
			get
			{
				DateTime result = GetUtcDateTime_Core();
				return result;
			}
		}

		public DateTime CurrentUtcDate
		{
			get { return CurrentUtcDateTime.Date; }
		}

		DateTime GetUtcDateTime_Core()
		{
			DateTime result = zoneCollection.CurrentUtc();
			AdjustDateTimeBasedOnTestDateAttributesIfTesting(ref result);
			return result;
		}

		void AdjustDateTimeBasedOnTestDateAttributesIfTesting(ref DateTime dateTimeToAdjust)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				if (TestDateAttribute.IsActive)
				{
					dateTimeToAdjust = TestDateAttribute.Date;
				}

				if (TestDateIncrementalAttribute.IsActive)
				{
					dateTimeToAdjust += TestDateIncrementalAttribute.GetNextTimeSpan();
				}
			}
#endif
		}

		#endregion

		public DateTime GetUnlocoDateTime(string unloco)
		{
			DateTime utcDateTime = GetUtcDateTime_Core();
			DateTime result = GetUnlocoTimeFromUtc(unloco, utcDateTime);
			return result;
		}

		string GetCurrentBranchUnloco()
		{
#if DEBUG
			if (TestTimeZoneUNLOCOAttribute.IsActive)
			{
				return TestTimeZoneUNLOCOAttribute.UNLOCO;
			}
#endif

			return EnvProxy.Instance.CurrentNKUNLOCO;
		}

		readonly TimeZoneCollection zoneCollection = new TimeZoneCollection();

		#endregion

		#region Time Zone Offset

		readonly TimeZoneOffsetCache timeZoneOffsetCache = new TimeZoneOffsetCache();

		public IReadOnlyCollection<TimeSpan> TimeZoneOffsets
		{
			get { return timeZoneOffsetCache.TimeZoneOffsets; }
		}

		#endregion

		#region Time Zone Conversion

		string GetCurrentBranchUnlocoTimeWithGmt()
		{
			var utcDateTime = GetUtcDateTime_Core();
			var currentUnlocoTime = GetLocalTimeFromUtc(utcDateTime);
			var currentUnlocoGmt = GetUtcOffsetBasedOnUtc(utcDateTime);
			return currentUnlocoTime.ToString(DateTimeFormatStrings.LongTimeFormat, CultureInfo.CurrentCulture) + " GMT" + (currentUnlocoGmt < TimeSpan.Zero ? "-" : "+") + currentUnlocoGmt.ToString("hh\\:mm", CultureInfo.CurrentCulture);
		}

		public DateTime GetLocalTimeFromUtc(DateTime utcDateTime)
		{
			string localUnloco = GetCurrentBranchUnloco();
			DateTime result = GetUnlocoTimeFromUtc(localUnloco, utcDateTime);
			return result;
		}

		public DateTime GetUnlocoTimeFromUtc(string unloco, DateTime utcDateTime)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				if (TestUtcOffsetAttribute.IsActive)
				{
					return DateTime.SpecifyKind(utcDateTime.Add(TestUtcOffsetAttribute.GetTimeSpan(utcDateTime)), DateTimeKind.Unspecified);
				}
				else if (TestDateAttribute.IsActive && !TestDateAttribute.UseUNLOCO)
				{
					return DateTime.SpecifyKind(utcDateTime, DateTimeKind.Unspecified);
				}
			}
#endif

			DateTime result = zoneCollection.ToLocationTimeFromUtc(unloco, utcDateTime);
			return result;
		}

		public TimeSpan GetUtcOffsetBasedOnUtc(DateTime utcDateTime)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				if (TestUtcOffsetAttribute.IsActive)
				{
					return TestUtcOffsetAttribute.GetTimeSpan(utcDateTime);
				}
				else if (TestDateAttribute.IsActive && !TestDateAttribute.UseUNLOCO)
				{
					return new TimeSpan();
				}
			}
#endif
			string localUnloco = GetCurrentBranchUnloco();
			TimeSpan result = GetUtcOffsetBasedOnUtc(localUnloco, utcDateTime);
			return result;
		}

		public TimeSpan GetUtcOffsetBasedOnUtc(string unloco, DateTime utcDateTime)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				if (TestUtcOffsetAttribute.IsActive)
				{
					return TestUtcOffsetAttribute.GetTimeSpan(utcDateTime);
				}
				else if (TestDateAttribute.IsActive && !TestDateAttribute.UseUNLOCO)
				{
					return new TimeSpan();
				}
			}
#endif
			TimeSpan result = zoneCollection.GetUtcOffsetBasedOnUtc(unloco, utcDateTime);
			return result;
		}

		///<summary>Because there can be two different valid UTC times for a given local time (due to DST re-running one hour per year), do NOT call this method if getting the real UTC time is important. Pass the UTC time around instead.
		///If it is ambiguous, the second hour of the two (the non-DST hour) will be returned.</summary>
		public TimeSpan GetUtcOffsetBasedOnLocal(DateTime localDateTime)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				if (TestUtcOffsetAttribute.IsActive)
				{
					return TestUtcOffsetAttribute.GetTimeSpanForLocal(localDateTime);
				}
				else if (TestDateAttribute.IsActive && !TestDateAttribute.UseUNLOCO)
				{
					return new TimeSpan();
				}
			}
#endif
			string localUnloco = GetCurrentBranchUnloco();
			TimeSpan result = GetUtcOffsetBasedOnLocal(localUnloco, localDateTime);
			return result;
		}

		///<summary>Because there can be two different valid UTC times for a given local time (due to DST re-running one hour per year), do NOT call this method if getting the real UTC time is important. Pass the UTC time around instead.
		///If it is ambiguous, the second hour of the two (the non-DST hour) will be returned.</summary>
		public TimeSpan GetUtcOffsetBasedOnLocal(string unloco, DateTime localDateTime)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				if (TestUtcOffsetAttribute.IsActive)
				{
					return TestUtcOffsetAttribute.GetTimeSpanForLocal(localDateTime);
				}
				else if (TestDateAttribute.IsActive && !TestDateAttribute.UseUNLOCO)
				{
					return new TimeSpan();
				}
			}
#endif
			TimeSpan result = zoneCollection.GetUtcOffsetBasedOnLocal(unloco, localDateTime);
			return result;
		}

		///<summary>Because there can be two different valid UTC times for a given local time (due to DST re-running one hour per year), do NOT call this method if getting the real UTC time is important. Pass the UTC time around instead.
		///If it is ambiguous, the second hour of the two (the non-DST hour) will be returned.</summary>
		public DateTime GetUtcFromLocalTime(DateTime localDateTime)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				if (TestUtcOffsetAttribute.IsActive)
				{
					return DateTime.SpecifyKind(localDateTime.Subtract(TestUtcOffsetAttribute.GetTimeSpanForLocal(localDateTime)), DateTimeKind.Utc);
				}
				else if (TestDateAttribute.IsActive && !TestDateAttribute.UseUNLOCO)
				{
					return DateTime.SpecifyKind(localDateTime, DateTimeKind.Utc);
				}
			}
#endif

			string localUnloco = GetCurrentBranchUnloco();
			DateTime result = GetUtcFromUnlocoTime(localUnloco, localDateTime);
			return result;
		}

		///<summary>Because there can be two different valid UTC times for a given local time (due to DST re-running one hour per year), do NOT call this method if getting the real UTC time is important. Pass the UTC time around instead.
		///If it is ambiguous, the second hour of the two (the non-DST hour) will be returned.</summary>
		public DateTime GetUtcFromUnlocoTime(string unloco, DateTime localDateTime)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				if (TestUtcOffsetAttribute.IsActive)
				{
					return DateTime.SpecifyKind(localDateTime.Subtract(TestUtcOffsetAttribute.GetTimeSpanForLocal(localDateTime)), DateTimeKind.Utc);
				}
				else if (TestDateAttribute.IsActive && !TestDateAttribute.UseUNLOCO)
				{
					return DateTime.SpecifyKind(localDateTime, DateTimeKind.Utc);
				}
			}
#endif

			DateTime result = zoneCollection.ToUtcFromLocationTime(unloco, localDateTime);
			return result;
		}

		public DateTime GetTimeInOneZoneFromTimeInAnotherZone(string unlocoFrom, DateTime dateTimeFrom, string unlocoTo)
		{
			return zoneCollection.ToLocationTimeFromAnotherLocationTime(unlocoFrom, dateTimeFrom, unlocoTo);
		}

		public bool IsValidUNLOCO(string unloco)
		{
			return zoneCollection.IsValidUNLOCO(unloco);
		}

		public DateTimeOffset GetLocalTimeFromDateTimeOffset(DateTimeOffset dateTimeOffsetFrom)
		{
			var localUnloco = GetCurrentBranchUnloco();
			var result = GetUnlocoTimeFromDateTimeOffset(localUnloco, dateTimeOffsetFrom);
			return result;
		}

		DateTimeOffset GetUnlocoTimeFromDateTimeOffset(string unloco, DateTimeOffset dateTimeOffsetFrom)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				if (TestUtcOffsetAttribute.IsActive)
				{
					return dateTimeOffsetFrom.ToOffset(TestUtcOffsetAttribute.GetTimeSpan(dateTimeOffsetFrom.UtcDateTime));
				}
				else if (TestDateAttribute.IsActive && !TestDateAttribute.UseUNLOCO)
				{
					return dateTimeOffsetFrom;
				}
			}
#endif

			var result = zoneCollection.GetUtcOffsetBasedOnUtc(unloco, dateTimeOffsetFrom.UtcDateTime);
			return dateTimeOffsetFrom.ToOffset(result);
		}

		#endregion

		#region Formatting

		public string FormatDate(DateTime dateToFormat)
		{
			string result = dateToFormat.Day.ToString("00") + "-" +
				TimeZoneConstants.AbbreviatedMonths[dateToFormat.Month] + "-" +
				(dateToFormat.Year % 100).ToString("00");
			return result;
		}

		public string CurrentLocalDateTimeIncludingGmt => GetCurrentBranchUnlocoTimeWithGmt();

		public string FormatTime(DateTime timeToFormat)
		{
			string result = timeToFormat.Hour.ToString("00") + ":" + timeToFormat.Minute.ToString("00");
			return result;
		}

		public string FormatTimeWithSeconds(DateTime timeToFormat)
		{
			string result = FormatTime(timeToFormat) + ":" + timeToFormat.Second.ToString("00");
			return result;
		}

		public string FormatDateTime(DateTime dateTimeToFormat)
		{
			string result = FormatDate(dateTimeToFormat) + " " + FormatTime(dateTimeToFormat);
			return result;
		}

		public string FormatDateTimeWithSeconds(DateTime dateTimeToFormat)
		{
			string result = FormatDate(dateTimeToFormat) + " " + FormatTimeWithSeconds(dateTimeToFormat);
			return result;
		}

		public string FormatDateTimeOffset(DateTimeOffset dateTimeOffsetToFormat)
		{
			var dateTime = dateTimeOffsetToFormat.DateTime;
			string result = FormatDate(dateTime) + " " + FormatTime(dateTime) + " " + FormatOffset(dateTimeOffsetToFormat.Offset);
			return result;
		}

		public string FormatDateTimeOffsetWithSeconds(DateTimeOffset dateTimeOffsetToFormat)
		{
			var dateTime = dateTimeOffsetToFormat.DateTime;
			string result = FormatDate(dateTime) + " " + FormatTimeWithSeconds(dateTime) + " " + FormatOffset(dateTimeOffsetToFormat.Offset);
			return result;
		}

		public string FormatOffset(TimeSpan offset)
		{
			return ZDateTimeOffset.OffsetStringHelper(offset);
		}

		public string Format(DateTime dateTimeToFormat, string formatString)
		{
			return dateTimeToFormat.ToString(temporaryDateFormat != null && string.IsNullOrEmpty(formatString) ? temporaryDateFormat : DateTimeFormatStrings.GetLocalizedFormatString(formatString));
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Same as ZDateTime version")]
		public string Format(DateTimeOffset dateTimeToFormat, string formatString)
		{
			return dateTimeToFormat.ToString(DateTimeOffsetFormatStrings.GetLocalizedFormatString(formatString));
		}

		public IDisposable SetTemporaryDateFormat(string formatString)
		{
			temporaryDateFormat = formatString;
			return new DisposableAction(() =>
			{
				temporaryDateFormat = null;
			});
		}

		string temporaryDateFormat;

		#endregion

		/// <summary>
		/// Reset the instance, forcing it be recreated on the next Instance access.
		/// Will cause any cached time information to be discarded.
		/// The next call to get the current time will hit the database for the SQL server time.
		/// </summary>
		[System.Diagnostics.Conditional("DEBUG")]
		static public void ResetInstance_ForTest()
		{
			if (Globals.IsTest)
			{
				instance = null;
			}
		}
	}
}

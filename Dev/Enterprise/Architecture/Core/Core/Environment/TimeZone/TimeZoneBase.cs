using System;
using System.Collections.Concurrent;

namespace Enterprise.ZArchitecture.Environment
{
	public interface ITimeZone
	{
		TimeSpan GetUtcOffsetBasedOnUtc(DateTime utcDateTime);
		TimeSpan GetUtcOffsetBasedOnLocal(DateTime localDateTime);
		DateTime ToLocalTime(DateTime utcDateTime);
		bool IsDaylightSavingBasedOnUtc(DateTime utcDateTime);
		DateTime ToUniversalTime(DateTime localDateTime);
		string StandardName { get; }
	}

	public abstract class TimeZoneBase : ITimeZone
	{
		public TimeZoneBase(string zoneSetName, decimal utcOffsetStandard, decimal utcOffsetDst)
		{
			this.zoneSetName = zoneSetName;
			utcSpanStandard = new TimeSpan(0, 0, (int)(utcOffsetStandard * 3600m));
			utcSpanDst = new TimeSpan(0, 0, (int)(utcOffsetDst * 3600m));
		}

		/// <summary>
		/// Returns the UTC offset for the specified time also in UTC (universal time).
		/// </summary>
		public TimeSpan GetUtcOffsetBasedOnUtc(DateTime utcDateTime)
		{
			TimeSpan result = (IsDaylightSavingBasedOnUtc(utcDateTime)) ? utcSpanDst : utcSpanStandard;
			return result;
		}

		/// <summary>
		/// Returns the local time that corresponds to a specified coordinated universal time (UTC).
		/// </summary>
		/// <param name="time">A UTC time.</param>
		public DateTime ToLocalTime(DateTime utcDateTime)
		{
			TimeSpan utcOffset = GetUtcOffsetBasedOnUtc(utcDateTime);
			DateTime result = utcDateTime.AddTicks(utcOffset.Ticks);
			result = DateTime.SpecifyKind(result, DateTimeKind.Local);
			return result;
		}

		/// <summary>
		/// Returns the local DateTimeOffset that corresponds to a specified coordinated universal time (UTC).
		/// </summary>
		/// <param name="time">A UTC time.</param>
		public DateTimeOffset ToLocalTimeOffset(DateTime utcDateTime)
		{
			var utcOffset = GetUtcOffsetBasedOnUtc(utcDateTime);
			var dateTime = utcDateTime.AddTicks(utcOffset.Ticks);
			var result = new DateTimeOffset(dateTime, utcOffset);
			return result;
		}

		/// <summary>
		/// Returns a value indicating whether the specified date and time (UTC) is within a daylight saving time period.
		/// </summary>
		/// <param name="utcDateTime">A UTC time.</param>
		public bool IsDaylightSavingBasedOnUtc(DateTime utcDateTime)
		{
			bool result = false;

			if (HasDaylightSaving)
			{
				DaylightAnnualInfo dstInfo = GetDaylightChanges(utcDateTime.Year);
				result = dstInfo.IsDaylightSavingBasedOnUtc(utcDateTime);
			}

			return result;
		}

		///<summary>Because there can be two different valid UTC times for a given local time (due to DST re-running one hour per year), do NOT call this method if getting the real UTC time is important. Pass the UTC time around instead.
		///If it is ambiguous, the second hour of the two (the non-DST hour) will be returned.</summary>
		public TimeSpan GetUtcOffsetBasedOnLocal(DateTime localDateTime)
		{
			TimeSpan tentativeUtcOffset = utcSpanStandard;
			DateTime resultUtc = localDateTime.Subtract(tentativeUtcOffset);
			TimeSpan checkUtcOffset = GetUtcOffsetBasedOnUtc(resultUtc);
			return checkUtcOffset;
		}

		///<summary>Because there can be two different valid UTC times for a given local time (due to DST re-running one hour per year), do NOT call this method if getting the real UTC time is important. Pass the UTC time around instead.
		///If it is ambiguous, the second hour of the two (the non-DST hour) will be returned.</summary>
		public DateTime ToUniversalTime(DateTime localDateTime)
		{
			DateTime resultUtc = localDateTime.Subtract(GetUtcOffsetBasedOnLocal(localDateTime));
			resultUtc = DateTime.SpecifyKind(resultUtc, DateTimeKind.Utc);
			return resultUtc;
		}

		public virtual string StandardName
		{
			get { return zoneSetName; }
		}

		DaylightAnnualInfo GetDaylightChanges(int year)
		{
			if (year < 1 || year > 9999)
			{
				throw new ArgumentOutOfRangeException(nameof(year), "Year should NOT be less than 1 or greater than 9999");
			}
			else
			{
				return dstInfoDictionary.GetOrAdd(year, (key) => GetAnnualDstInfo(year));
			}
		}

		/// <summary>
		/// If the Daylight has no START or END info for any given year, 
		/// it should be interpreted as:
		///   - !START and !END => No DST in the whole year
		///   -  START and !END => DST from START rule till the end of the year
		///   - !START and  END => DST from beginning of the year till END rule
		///   -  START and  END => DST as per normal
		/// </summary>
		DaylightAnnualInfo GetAnnualDstInfo(int year)
		{
			var startAndEndDstRuleInfo = GetStartAndEndDstRuleInfo(year);
			var startTransitionUtc = new DateTime(year, 1, 1);
			var endTransitionUtc = new DateTime(year, 1, 1);

			if (startAndEndDstRuleInfo.StartRule != null)
			{
				startTransitionUtc = startAndEndDstRuleInfo.StartRule.RuleParser.GetTransitionDateTimeInUtc(utcSpanStandard, utcSpanDst);
			}

			if (startAndEndDstRuleInfo.EndRule != null)
			{
				endTransitionUtc = startAndEndDstRuleInfo.EndRule.RuleParser.GetTransitionDateTimeInUtc(utcSpanStandard, utcSpanDst);
			}

			return new DaylightAnnualInfo(startTransitionUtc, endTransitionUtc);
		}

		protected abstract RefTimeZoneRuleInfoStartAndEndPair GetStartAndEndDstRuleInfo(int year);

		protected abstract bool HasDaylightSaving { get; }

		readonly string zoneSetName;
		readonly TimeSpan utcSpanStandard;
		readonly TimeSpan utcSpanDst;

		readonly ConcurrentDictionary<int, DaylightAnnualInfo> dstInfoDictionary = new ConcurrentDictionary<int, DaylightAnnualInfo>();
	}
}

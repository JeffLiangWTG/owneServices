using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using Enterprise.Integration.Billing;
using Enterprise.ZArchitecture.Environment;
using TimeZoneInfo = System.TimeZoneInfo;

namespace Enterprise.Billing.StlCollector.Retriever
{
	abstract class BaseDateTimeRange : IDateTimeRange
	{
		public BaseDateTimeRange(DateTime startDateTimeInclusive, DateTime endDateTimeExclusive)
		{
			collectionStartDateTimeInclusive = startDateTimeInclusive;
			collectionEndDateTimeExclusive = endDateTimeExclusive;
		}

		public bool IsValid
		{
			get
			{
				return (
					// Start date must be earlier than end date
					collectionStartDateTimeInclusive < collectionEndDateTimeExclusive
					// Minimum DateTime value not allowed as start date
					&& collectionStartDateTimeInclusive != DateTime.MinValue
					// Run concrete range validation
					&& ValidateRangeSpecific()
				);
			}
		}

		protected abstract bool ValidateRangeSpecific();

		public IEnumerable<IStlScriptWithConfig> SelectApplicableScripts(params IStlScriptWithConfig[] allScripts)
		{
			return SelectApplicableScripts(allScripts.Select(s => s));
		}

		public IEnumerable<IStlScriptWithConfig> SelectApplicableScripts(IEnumerable<IStlScriptWithConfig> allScripts)
		{
			return (IsValid) ? allScripts.Where(swc => ScriptResultsShouldBeSubmitted(swc)) : Array.Empty<IStlScriptWithConfig>();
		}

		public bool ScriptResultsShouldBeSubmitted(IStlScriptWithConfig swc) => ScriptResultsShouldBeSubmittedCore(swc);

		protected virtual bool ScriptResultsShouldBeSubmittedCore(IStlScriptWithConfig swc)
		{
			switch (swc.Script.StlGrain)
			{
				case StlDataGrain.Transactional:
				case StlDataGrain.Snapshot:
					return true;
				case StlDataGrain.Daily:
					return DailyRangeStartInclusive.HasValue;
				case StlDataGrain.MonthlyAllowHistoricalData:
					return MonthlyRangeStartInclusive.HasValue;
				case StlDataGrain.MonthlyCurrentDataOnly:
					return MonthlyRangeStartInclusive.HasValue && IsCurrentMonthlyCollection();
				default:
					throw new ArgumentOutOfRangeException(swc.Script.StlGrain.ToString());
			}
		}

		public bool ScriptShouldBeRun(IStlScriptWithConfig swc)
		{
			if (ScriptResultsShouldBeSubmitted(swc))
			{
				return true;
			}

			return (swc.Script.StlGrain == StlDataGrain.MonthlyAllowHistoricalData || swc.Script.StlGrain == StlDataGrain.MonthlyCurrentDataOnly) &&
					swc.Script.IsMandatoryForMilestones && DailyRangeStartInclusive.HasValue && swc.Script.CollectorType != StlCollectorType.Dynamic;
		}

		public DateTime StartDateTimeInclusive
		{
			get { return collectionStartDateTimeInclusive; }
		}
		protected readonly DateTime collectionStartDateTimeInclusive;

		public DateTime EndDateTimeExclusive
		{
			get { return collectionEndDateTimeExclusive; }
		}
		protected readonly DateTime collectionEndDateTimeExclusive;

		public abstract DateTime? MonthlyRangeStartInclusive { get; }
		public abstract DateTime? MonthlyRangeEndExclusive { get; }
		public abstract DateTime? StlMilestoneTimestamp { get; }
		public abstract DateTime? DailyRangeStartInclusive { get; }
		public abstract DateTime? DailyRangeEndInclusive { get; }

		/// <summary>
		/// Regarded as "current enough" monthly collection if collecting data within range or no more than 3 weeks after it.
		/// </summary>
		public bool IsCurrentMonthlyCollection()
		{
			DateTime utcNow = EnvProxy.Instance.Time.CurrentUtcDateTime;
			return (
				MonthlyRangeStartInclusive.HasValue
				&& MonthlyRangeEndExclusive.HasValue
				&& utcNow > MonthlyRangeStartInclusive.Value.AddDays(-1)
				&& utcNow < MonthlyRangeEndExclusive.Value.AddDays(21)
			);
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture,
				"[{0}, {1})",
				SqlFormatInfo.ToSqlDateTimeString(StartDateTimeInclusive),
				SqlFormatInfo.ToSqlDateTimeString(EndDateTimeExclusive)
			);
		}

		internal static DateTime GetMinDateTimeValue(DateTime dt1, DateTime dt2)
		{
			return (dt1 <= dt2) ? dt1 : dt2;
		}

		internal static DateTime GetBillingDateTimeFromUtc(DateTime dt) => TimeZoneInfo.ConvertTimeFromUtc(dt, BillingTimeZoneInfo);

		internal static DateTime GetUtcFromBillingDateTime(DateTime dt) => TimeZoneInfo.ConvertTimeToUtc(dt, BillingTimeZoneInfo);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "literal String is safe to use in this Context")]
		public static TimeZoneInfo BillingTimeZoneInfo { get; } = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
	}
}

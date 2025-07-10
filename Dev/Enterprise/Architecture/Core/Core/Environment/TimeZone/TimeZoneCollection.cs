using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Environment
{
	class TimeZoneCollection
	{
		public DateTime CurrentUtc()
		{
			TryRefreshCache(onlyIfExpired: true);
			return utcAndServerZoneWrapper.GetCurrentUtc();
		}

		internal void RefreshCacheNow()
			=> TryRefreshCache(onlyIfExpired: false);

		/// <summary>
		/// Set this true so that the cache can only be updated by calling <see cref="RefreshCacheNow"/>.
		/// </summary>
		internal bool CacheNeverExpires { get; set; }

		void TryRefreshCache(bool onlyIfExpired)
		{
			if (utcAndServerZoneWrapper == null)
			{
				ReplaceCache(new UtcDateTimeCache());
			}
			else if (!onlyIfExpired || (!CacheNeverExpires && utcAndServerZoneWrapper.IsExpired))
			{
				var newCache = new UtcDateTimeCache();
				if (!newCache.IsExpired) // Was unable to retrieve DateTime values
				{
					ReplaceCache(newCache);
				}
				else
				{
					newCache?.Dispose();
				}
			}
		}

		void ReplaceCache(UtcDateTimeCache newCache)
		{
			var expiredCache = Interlocked.Exchange(ref utcAndServerZoneWrapper, newCache);
			expiredCache?.Dispose();
		}

		public DateTime ToLocationTimeFromAnotherLocationTime(string unlocoFrom, DateTime dateTimeFrom, string unlocoTo)
		{
			var utcDateTime = ToUtcFromLocationTime(unlocoFrom, dateTimeFrom);
			return ToLocationTimeFromUtc(unlocoTo, utcDateTime);
		}

		public DateTime ToLocationTimeFromUtc(string unloco, DateTime utcDateTime)
		{
			var timeZone = GetLocationTimeZoneCreatingIfNotExist(unloco);
			return timeZone.ToLocalTime(utcDateTime);
		}

		///<summary>Because there can be two different valid UTC times for a given local time (due to DST re-running one hour per year), do NOT call this method if getting the real UTC time is important. Pass the UTC time around instead.
		///If it is ambiguous, the second hour of the two (the non-DST hour) will be returned.</summary>
		public DateTime ToUtcFromLocationTime(string unloco, DateTime localDateTime)
		{
			var timeZone = GetLocationTimeZoneCreatingIfNotExist(unloco);
			return timeZone.ToUniversalTime(localDateTime);
		}

		///<summary>Because there can be two different valid UTC times for a given local time (due to DST re-running one hour per year), do NOT call this method if getting the real UTC time is important. Pass the UTC time around instead.
		///If it is ambiguous, the second hour of the two (the non-DST hour) will be returned.</summary>
		public TimeSpan GetUtcOffsetBasedOnLocal(string unloco, DateTime localDateTime)
		{
			var timeZone = GetLocationTimeZoneCreatingIfNotExist(unloco);
			return timeZone.GetUtcOffsetBasedOnLocal(localDateTime);
		}

		public TimeSpan GetUtcOffsetBasedOnUtc(string unloco, DateTime utcDateTime)
		{
			var timeZone = GetLocationTimeZoneCreatingIfNotExist(unloco);
			return timeZone.GetUtcOffsetBasedOnUtc(utcDateTime);
		}

		public bool IsValidUNLOCO(string unloco)
		{
			if (string.IsNullOrEmpty(unloco))
			{
				return false;
			}
			return GetLocationTimeZoneCreatingIfNotExist(unloco, defaulToUtc: false) != null;
		}

		ITimeZone GetLocationTimeZoneCreatingIfNotExist(string unloco, bool defaulToUtc = true)
		{
			if (unloco == null)
			{
				return MockDbServerTimeZone;
			}
			if (timeZones.TryGetValue(unloco, out var zone))
			{
				return zone;
			}
			if (nonExistingTimeZones.Contains(unloco))
			{
				if (defaulToUtc)
				{
					return UTCTimeZoneInfo;
				}
				return null;
			}

			var timeZone = LoadUnlocoTimeZoneInfo(unloco);
#pragma warning disable CW1024
			if (timeZone == null)
			{
				nonExistingTimeZones.TryAdd(unloco);
				if (defaulToUtc)
				{
					return UTCTimeZoneInfo;
				}
				return null;
			}
			timeZones.TryAdd(unloco, timeZone);
#pragma warning restore CW1024
			return timeZone;
		}

		[ThreadSafe]
		static TimeZoneInfo utcTimeZoneInfo;
		static TimeZoneInfo UTCTimeZoneInfo
		{
			get
			{
				if (utcTimeZoneInfo == null)
				{
					utcTimeZoneInfo = new TimeZoneInfo("UTC", 0, 0, Guid.Empty);
				}
				return utcTimeZoneInfo;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		ITimeZone LoadUnlocoTimeZoneInfo(string unloco)
		{
			ITimeZone unlocoTimeZone;

			var zoneSetName = string.Empty;
			var zoneSetPk = Guid.Empty;
			var dstZonePk = Guid.Empty;
			decimal utcOffsetStandard = 0m;
			decimal utcOffsetDst = 0m;

			var sqlText = zoneOffsetSqlText;

			using (DbCommand cmd = Db.Connection.Command(sqlText))
			{
				cmd.AddParameterBasedOnDbColumn("@Unloco", unloco, RefUNLOCOSchema.RL_Code);

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						utcOffsetStandard = (decimal)reader["UtcOffsetStandard"];
						zoneSetName = reader["ZoneSetName"].ToString();
						var utcOffsetDstObj = reader["UtcOffsetDst"];
						var dstZonePkObj = reader["DstZonePk"];
						var zoneSetPkObj = reader["ZoneSetPk"];

						zoneSetPk = (zoneSetPkObj == DBNull.Value) ? Guid.Empty : (Guid)zoneSetPkObj;
						dstZonePk = (dstZonePkObj == DBNull.Value) ? Guid.Empty : (Guid)dstZonePkObj;
						utcOffsetDst = (utcOffsetDstObj == DBNull.Value) ? utcOffsetStandard : (decimal)utcOffsetDstObj;
					}
				}
			}

			if (zoneSetPk == Guid.Empty)
			{
				return null;
			}
			else
			{
				unlocoTimeZone = new TimeZoneInfo(zoneSetName, utcOffsetStandard, utcOffsetDst, dstZonePk);
			}

			return unlocoTimeZone;
		}

		ITimeZone MockDbServerTimeZone
		{
			get
			{
				if (mockDbServerTimeZone == null)
				{
					const string DbServerOffsetSql = "SELECT datediff(minute, SYSUTCDATETIME(), sysdatetime()) / 60.0;";
					var dbServerOffset = Convert.ToDecimal(Db.Connection.ExecuteScalar(DbServerOffsetSql));
					mockDbServerTimeZone = new TimeZoneInfo("DbServerZone", dbServerOffset, dbServerOffset, Guid.Empty);
				}

				return mockDbServerTimeZone;
			}
		}
		ITimeZone mockDbServerTimeZone;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "ConcurrentDictionary is thread safe and time zone information is not thread specific.")]
		protected static readonly ConcurrentDictionary<string, ITimeZone> timeZones = new ConcurrentDictionary<string, ITimeZone>();
		protected static readonly ConcurrentHashSet<string> nonExistingTimeZones = new ConcurrentHashSet<string>();
		protected UtcDateTimeCache utcAndServerZoneWrapper;

		#region SQL Scripts

		const string zoneOffsetSqlText = @"
			SELECT
				RefTimeZoneSet.R3_PK ZoneSetPk
				, RefTimeZoneSet.R3_TimeZoneSetName ZoneSetName
				, RefTimeZoneSet.R3_R2_DaylightSavingZone DstZonePk
				, CAST(CAST(StandardZone.R2_OffsetMinutesFromUTC AS decimal(7,2)) / 60.0 AS decimal(5,2)) AS UtcOffsetStandard
				, CAST(CAST(DstZone.R2_OffsetMinutesFromUTC AS decimal(7,2)) / 60.0 AS decimal(5,2))  AS UtcOffsetDst
			FROM
				dbo.RefUNLOCO 
				INNER JOIN dbo.RefTimeZoneSet  ON RefTimeZoneSet.R3_PK = RefUNLOCO.RL_R3
				INNER JOIN dbo.RefTimeZone StandardZone  ON StandardZone.R2_PK = RefTimeZoneSet.R3_R2_StandardZone
				LEFT JOIN dbo.RefTimeZone DstZone  ON DstZone.R2_PK = RefTimeZoneSet.R3_R2_DaylightSavingZone
			WHERE
				RefUNLOCO.RL_Code = @Unloco";

		#endregion
	}
}

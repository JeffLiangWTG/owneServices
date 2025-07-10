using System;
using System.Data;
using System.ServiceModel;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Environment
{
	public class TimeZoneInfo : TimeZoneBase
	{
		public TimeZoneInfo(string zoneName, decimal utcOffsetStandard, decimal utcOffsetDst, Guid dstZonePk)
			: base(zoneName, utcOffsetStandard, utcOffsetDst)
		{
			if (dstZonePk != Guid.Empty)
			{
				CheckDstZoneExists(zoneName, dstZonePk);
			}

			this.dstZonePk = dstZonePk;
		}

		protected override bool HasDaylightSaving
		{
			get
			{
				if (hasDaylightSaving == null)
				{
					hasDaylightSaving = (dstZonePk != Guid.Empty);
				}

				return hasDaylightSaving.Value;
			}
		}

		bool? hasDaylightSaving;

		protected override RefTimeZoneRuleInfoStartAndEndPair GetStartAndEndDstRuleInfo(int year)
		{
			try
			{
				return GetStartAndEndDstRuleInfoFromDb(year, Db.Connection);
			}
			catch (CommunicationException)
			{
				// Can happen during recovery of a concurrency error whilst multiple DataReaders are open
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					return GetStartAndEndDstRuleInfoFromDb(year, connection);
				}
			}
		}

		RefTimeZoneRuleInfoStartAndEndPair GetStartAndEndDstRuleInfoFromDb(int year, DbConnection connection)
		{
			RefTimeZoneRuleInfo startDstInfo = GetDstRuleInfo(year, TimeZoneConstants.DstTransitionTypeStart, connection);
			RefTimeZoneRuleInfo endDstInfo = GetDstRuleInfo(year, TimeZoneConstants.DstTransitionTypeEnd, connection);
			RefTimeZoneRuleInfoStartAndEndPair result = new RefTimeZoneRuleInfoStartAndEndPair(startDstInfo, endDstInfo);

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		RefTimeZoneRuleInfo GetDstRuleInfo(int year, string transitionType, DbConnection connection)
		{
			RefTimeZoneRuleInfo result = null;

			using (DbCommand cmd = connection.Command(dstRuleSqlText))
			{
				cmd.AddParameter("@DstZonePk", SqlDbType.UniqueIdentifier, dstZonePk);
				cmd.AddParameter("@CurrentYear", SqlDbType.SmallInt, year);
				cmd.AddParameterBasedOnDbColumn("@StartOrEndRule", transitionType, RefTimeZoneRuleSchema.R4_StartOrEndRule);

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						string weekdayOrMonthday = reader["R4_DaylightSavingDayWeekDate"].ToString();
						string transitionTimeBaseCode = reader["R4_TypeOfTime"].ToString();
						DateTime dstDate = Convert.ToDateTime(reader["R4_DaylightSavingDate"]);
						int nthWeekdayOfMonth = Convert.ToInt32(reader["R4_DaylightSavingDayCount"]);
						string weekdayCode = reader["R4_DaylightSavingDayName"].ToString();
						string monthCode = reader["R4_DaylightSavingMonth"].ToString();

						result = new RefTimeZoneRuleInfo(
							year, transitionType, weekdayOrMonthday, transitionTimeBaseCode,
							dstDate, nthWeekdayOfMonth, weekdayCode, monthCode);
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception message")]
		void CheckDstZoneExists(string zoneName, Guid dstZonePk)
		{
			using (DbCommand cmd = Db.Connection.Command(dstZoneSqlText))
			{
				cmd.AddParameter("@DstZonePk", SqlDbType.UniqueIdentifier, dstZonePk);
				int rowCount = (int)cmd.ExecuteScalar();

				if (rowCount != 1)
				{
					string mask = "Unable to obtain date time information: Inexisting Daylight Saving zone for [{0}] time zone.";
					string message = string.Format(mask, zoneName);
					throw new TimeZoneException(message, null);
				}
			}
		}

		readonly Guid dstZonePk;

		#region SQL Scripts

		const string dstRuleSqlText = @"
			SELECT
				R4_DaylightSavingDayWeekDate
				, R4_DaylightSavingDate
				, R4_DaylightSavingDayCount
				, R4_DaylightSavingDayName
				, R4_DaylightSavingMonth
				, R4_TypeOfTime
			FROM
				dbo.RefTimeZoneRule 
			WHERE
				R4_R2 = @DstZonePk
				AND R4_FromYear <= @CurrentYear
				AND R4_StartOrEndRule = @StartOrEndRule
				AND (R4_ToYear >= @CurrentYear OR R4_ToYear = 0)
			";

		const string dstZoneSqlText = @"
			SELECT count(*)
			FROM dbo.RefTimeZone 
			WHERE R2_PK = @DstZonePk
			";

		#endregion
	}
}

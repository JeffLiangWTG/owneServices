using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.HRMS
{
	class CopyCurrentWorkPatternTimes : DataTransformation
	{
		public override string UserDescription => "Copy current Work Pattern's times to Staff record's work times";

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(@"
				IF EXISTS(SELECT TOP 1 NULL FROM dbo.GlbWorkPattern)
				BEGIN
					DECLARE @now AS DATETIMEOFFSET = SYSDATETIMEOFFSET();

					SELECT GWP_GS_Staff, GW_DayOfWeek, GW_StartTime, GW_EndTime
					INTO #CurrentWorkTimes
					FROM dbo.GlbWorkTime JOIN 
					(
						SELECT GWP_PK, GWP_GS_Staff
						FROM dbo.GlbWorkPattern
						WHERE GWP_IsApproved = 1
							AND GWP_EffectiveDate <= @now
							AND (GWP_AutoEffectiveEndDate IS NULL OR GWP_AutoEffectiveEndDate > @now)
					) AS CurrentPatterns
					ON GW_ParentID = GWP_PK

					DELETE dbo.GlbWorkTime
					FROM dbo.GlbWorkTime
					JOIN #CurrentWorkTimes ON GW_ParentID = GWP_GS_Staff

					INSERT dbo.GlbWorkTime (GW_PK, GW_ParentID, GW_ParentTableCode, GW_DayOfWeek, GW_StartTime, GW_EndTime, GW_SystemCreateTimeUtc, GW_SystemCreateUser, GW_SystemLastEditTimeUtc, GW_SystemLastEditUser)
					SELECT NEWID(), GWP_GS_Staff, 'GS', GW_DayOfWeek, GW_StartTime, GW_EndTime, GetUtcDate(), '~BP', GetUtcDate(), '~BP'
					FROM #CurrentWorkTimes

					DROP TABLE #CurrentWorkTimes
				END");
		}
	}
}

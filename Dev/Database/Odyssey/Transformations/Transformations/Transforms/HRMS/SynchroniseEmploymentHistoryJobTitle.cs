using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.HRMS
{
	public class SynchroniseEmploymentHistoryJobTitle : DataTransformation
	{
		public override string UserDescription => "Update Staff GS_Title to most recent GEH_JobTitle in EmploymentHistory";

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(@"
				IF EXISTS(SELECT TOP 1 NULL FROM dbo.GlbEmploymentHistory)
				BEGIN
					UPDATE dbo.GlbStaff
					SET 
						GS_Title = eh.GEH_JobTitle,
						GS_SystemLastEditUser = '~BP',
						GS_SystemLastEditTimeUtc = GETUTCDATE()
					FROM
						dbo.GlbStaff AS gs
							OUTER APPLY (
								SELECT TOP 1
									GEH_JobTitle
								FROM dbo.GlbEmploymentHistory
								WHERE
									GEH_GS_Staff = gs.GS_PK
									AND GEH_IsApproved = 1
									AND GEH_EffectiveDate <= SYSDATETIMEOFFSET()
								ORDER BY GEH_EffectiveDate DESC
							) eh
					WHERE eh.GEH_JobTitle IS NOT NULL AND gs.GS_Title <> eh.GEH_JobTitle
				END");
		}
	}
}

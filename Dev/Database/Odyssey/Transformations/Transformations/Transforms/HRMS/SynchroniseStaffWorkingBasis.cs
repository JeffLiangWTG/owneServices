using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.HRMS
{
	public class SynchroniseStaffWorkingBasis : DataTransformation
	{
		public override string UserDescription => "Update Staff WorkingBasis to most recent GSW_WorkingBasis in StaffWorkingBasis";

		protected override void OfflinePostUpgradeTransform()
		{
			Db.Connection.ExecuteNonQuery(@"
				IF EXISTS(SELECT TOP 1 NULL FROM hrm.GlbStaffWorkingBasis)
				BEGIN
					UPDATE dbo.GlbStaff
					SET 
						GS_EmploymentBasis = wb.GSW_WorkingBasis,
						GS_SystemLastEditUser = '~BP',
						GS_SystemLastEditTimeUtc = GETUTCDATE()
					FROM
					dbo.GlbStaff AS gs
						OUTER APPLY (
							SELECT TOP 1
								GSW_WorkingBasis
							FROM hrm.GlbStaffWorkingBasis
							WHERE GSW_GS_Staff = gs.GS_PK AND GSW_EffectiveDate <= SYSDATETIMEOFFSET()
							ORDER BY GSW_EffectiveDate DESC
						) wb
					WHERE wb.GSW_WorkingBasis IS NOT NULL AND wb.GSW_WorkingBasis <> gs.GS_EmploymentBasis
				END");
		}
	}
}

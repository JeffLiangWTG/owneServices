using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class PopulateSqlStaffLoginPasswordHash : DataTransformation
	{
		public override string UserDescription => "Populate Sql Staff Login Password Hash";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = FormattableString.Invariant(
				$@"
UPDATE staff
	SET
		GS_SqlLoginPasswordHash = lgn.password_hash,
		GS_SystemLastEditTimeUtc = GetUtcDate(),
		GS_SystemLastEditUser = '~BP'
FROM
	dbo.GlbStaff          AS staff
	JOIN dbo.GlbGroupLinK AS groupLink ON groupLink.GK_GS = staff.GS_PK
	JOIN dbo.GlbGroup     AS grp       ON grp.GG_PK = groupLink.GK_GG
	JOIN dbo.GlbGroupRole AS groupRole ON groupRole.GGR_GG_Group = grp.GG_PK
	JOIN sys.sql_logins   AS lgn       ON lgn.name = @prefix + staff.GS_LoginName COLLATE database_default
WHERE 1=1
	AND staff.GS_IsActive = 1
	AND staff.GS_IsResource = 0
	AND groupRole.GGR_RoleName IN (N'cwRestrictedReaderRole', N'db_datawriter', N'db_backupoperator', N'cwHRMStaffRole')
");
			Db.Connection.ExecuteNonQuery(sql, cmd => cmd.AddParameter("@prefix", System.Data.SqlDbType.NVarChar, 128, DbUserRepository.GetStaffDbLoginFullPrefix(Db.DatabaseName)));
		}
	}
}

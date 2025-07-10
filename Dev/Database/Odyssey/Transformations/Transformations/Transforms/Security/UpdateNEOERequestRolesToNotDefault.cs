using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Security
{
	public class UpdateNEOERequestRolesToNotDefault : DataTransformation
	{
		public override string UserDescription => "Remove eRequestViewAll and eRequestViewOwn from Neo Default Role Set";

		protected override void OfflinePostUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, "EdiBilledUsage") || !DbObjectCreator.TableExists(Db.Connection, GlbGroupSchema.Constants.TableName)
				|| !DbObjectCreator.TableExists(Db.Connection, GlbGroupRoleSchema.Constants.TableName))
			{
				return;
			}

			var sql = $@"
DELETE FROM dbo.GlbGroupRole
WHERE
	GGR_RoleName IN ( 'eRequestViewAll', 'eRequestViewOwn' )
	AND GGR_GG_Group IN ( SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Code = 'NEOROLES' AND GG_Type = 'ORG' AND GG_IsSystemDefined = 1)
";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}

using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.HRMS
{
	class GlbStaffEntitlementDeleteDuplicateEntitlementCodeWithinSameRem : DataTransformation
	{
		public override string UserDescription => "Delete duplicate entitlement within the same remuneration package";

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, GlbStaffEntitlementSchema.Constants.TableName, "hrm"))
			{
				Db.Connection.ExecuteNonQuery(sqlUpdate);
			}
		}

		const string sqlUpdate = @"
WITH CTE AS (
    SELECT
        *,
        ROW_NUMBER() OVER (PARTITION BY GSI_GSR_Remuneration, GSI_EntitlementCode ORDER BY GSI_SystemCreateTimeUtc) AS rn
    FROM
        hrm.GlbStaffEntitlement
)
DELETE FROM CTE
WHERE rn > 1;
";
	}
}

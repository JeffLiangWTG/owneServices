using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations
{
	public class UpdateTenantForExistingEdiIdentityApplication : DataTransformation
	{
		public override string UserDescription => "Update the tenant ID for applications that are not customer application";

		protected override void OfflinePostUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "EdiIdentityApplication"))
			{
				var sql = @"
UPDATE dbo.EdiIdentityApplication
SET IDA_IDT = (
    SELECT IDT_PK
    FROM EdiIdentityTenant
    WHERE IDT_TenantId = '1b20b87e-cebd-43cc-97bd-bdd41a2f5cf1'
)
WHERE IDA_IDA_ParentApplication IS NULL
  AND IDA_OH_ParentOrg IS NULL
  AND IDA_IDT IS NULL;";

				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}

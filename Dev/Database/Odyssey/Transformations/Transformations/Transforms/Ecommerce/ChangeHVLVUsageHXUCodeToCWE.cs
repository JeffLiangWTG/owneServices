using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Ecommerce
{
	class ChangeHVLVUsageHXUCodeToCWE : DataTransformation
	{
		public override string UserDescription => "Update HXU_Code from empty to CWE.";

		public override bool IsRequired => base.IsRequired
			&& DbObjectCreator.TableExists(Db.Connection, "HVLVUsage")
			&& DbObjectCreator.ColumnExists(Db.Connection, "HVLVUsage", "HXU_Code");

		protected override void OfflinePreUpgradeTransform()
		{
			var connection = Db.Connection;
			var sql = @"
ALTER TABLE dbo.HVLVUsage DROP CONSTRAINT IF EXISTS Constraint_HXU_Code 

UPDATE dbo.HVLVUsage
SET
	HXU_Code = 'CWE'
WHERE HXU_Code = '' AND HXU_Category = 'CW1'
";

			connection.ExecuteNonQuery(sql);
		}
	}
}

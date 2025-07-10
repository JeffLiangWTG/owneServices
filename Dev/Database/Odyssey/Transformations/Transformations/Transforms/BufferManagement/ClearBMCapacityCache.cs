using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.BufferManagement
{
	public class ClearBMCapacityCache : DataTransformation
	{
		public override string UserDescription => "Clear Capacity Cache";
		public override bool IsRequired => base.IsRequired &&
			DbObjectCreator.TableExists(Db.Connection, "BMCapacityCache") &&
			DbObjectCreator.ColumnExists(Db.Connection, "BMCapacityCache", "BMC_Type");

		protected override void OfflinePreUpgradeTransform() =>	Db.Connection.ExecuteNonQuery($"TRUNCATE TABLE dbo.BMCapacityCache");
	}
}

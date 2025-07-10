using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	public class UpdateNullE6_ParentIDToEmptyForJobConsolCost : DataTransformation
	{
		public override string UserDescription => "Evaluate null E6_ParentID with empty GUID for JobConsolCost";

		protected override void OfflinePreUpgradeTransform()
		{
			const string targetColumnName = JobConsolCostSchema.Constants.E6_ParentID;
			const string tableName = JobConsolCostSchema.Constants.TableName;
			const string sqlSchemaName = JobConsolCostSchema.Constants.SqlSchemaName;

			if (!DbObjectCreator.ColumnsExist(
					Db.Connection,
					Db.Connection.CurrentDatabase,
					sqlSchemaName,
					tableName,
					new string[] { targetColumnName }))
			{
				return;
			}

			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, tableName, "E6_SystemLastEditTimeUtc", nameof(SqlDbType.SmallDateTime));
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, tableName, "E6_SystemLastEditUser", "VARCHAR(3)");

			var sql = @"
UPDATE dbo.JobConsolCost
	SET	E6_ParentID = '00000000-0000-0000-0000-000000000000',
		E6_SystemLastEditTimeUtc = GETUTCDATE(),
		E6_SystemLastEditUser = '~BP'
	WHERE E6_ParentID IS NULL
					";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}

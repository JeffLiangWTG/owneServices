using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	public class UpdateNullAO_ParentIDToEmptyForAccChargeTaxOverride : DataTransformation
	{
		public override string UserDescription => "Evaluate null AO_ParentID with empty GUID for AccChargeTaxOverride";

		protected override void OfflinePreUpgradeTransform()
		{
			const string targetColumnName = AccChargeTaxOverrideSchema.Constants.AO_ParentID;
			const string tableName = AccChargeTaxOverrideSchema.Constants.TableName;
			const string sqlSchemaName = AccChargeTaxOverrideSchema.Constants.SqlSchemaName;

			if (!DbObjectCreator.TableExists(Db.Connection, tableName))
			{
				return;
			}

			if (!DbObjectCreator.ColumnsExist(
					Db.Connection,
					Db.Connection.CurrentDatabase,
					sqlSchemaName,
					tableName,
					new string[] { targetColumnName }))
			{
				return;
			}

			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, tableName, "AO_SystemLastEditTimeUtc", nameof(SqlDbType.SmallDateTime));
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, tableName, "AO_SystemLastEditUser", "VARCHAR(3)");

			var sql = @"
UPDATE dbo.AccChargeTaxOverride
	SET	AO_ParentID = '00000000-0000-0000-0000-000000000000',
		AO_SystemLastEditTimeUtc = GETUTCDATE(),
		AO_SystemLastEditUser = '~BP'
	WHERE AO_ParentID IS NULL
					";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}

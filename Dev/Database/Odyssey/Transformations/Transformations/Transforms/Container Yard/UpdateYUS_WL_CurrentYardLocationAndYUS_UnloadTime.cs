using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.DbUpgrader.Shared.DbObjectCreator;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
#pragma warning disable CS0618
	public class UpdateYUS_WL_CurrentYardLocationAndYUS_UnloadTime : DataTransformation
#pragma warning restore CS0618
	{
		public override string UserDescription => "Update values for YUS_WL_CurrentYardLocation and YUS_UnloadTime";

		protected override void OfflinePreUpgradeTransform()
		{
			if (TableExists(Db.Connection, CYDYardUnitStateSchema.Constants.TableName)
				&& ColumnExists(Db.Connection, CYDYardUnitStateSchema.Constants.TableName, CYDYardUnitStateSchema.Constants.YUS_WL_CurrentYardLocation)
				&& ColumnExists(Db.Connection, CYDYardUnitStateSchema.Constants.TableName, CYDYardUnitStateSchema.Constants.YUS_LoadTime)
				&& ColumnExists(Db.Connection, CYDYardUnitStateSchema.Constants.TableName, CYDYardUnitStateSchema.Constants.YUS_UnloadTime)
				)
			{
				var sql = @"
UPDATE dbo.CYDYardUnitState
SET
	YUS_UnloadTime = NULL,
	YUS_GS_NKUnloadUser = '',
	YUS_SystemLastEditTimeUtc = GetUtcDate(),
	YUS_SystemLastEditUser = '~BP'
WHERE
	YUS_WL_CurrentYardLocation IS NULL 
	AND YUS_LoadTime IS NULL 
	AND YUS_UnloadTime IS NOT NULL;

UPDATE dbo.CYDYardUnitState
SET
	YUS_WL_CurrentYardLocation = NULL,
	YUS_SystemLastEditTimeUtc = GetUtcDate(),
	YUS_SystemLastEditUser = '~BP'
WHERE
	(YUS_WL_CurrentYardLocation IS NOT NULL
	AND	YUS_LOADTIME IS NOT NULL
	AND YUS_UNLOADTIME IS NOT NULL);
";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}

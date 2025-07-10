using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
	class PopulateLoadUnloadUserAndTimeForYardUnitState : DataTransformation
	{
		public override string UserDescription => "Update Load/Unload Time and Load/Unload User for Yard Unit State";

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, CYDYardUnitStateSchema.Constants.TableName)
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDYardUnitStateSchema.Constants.TableName, CYDYardUnitStateSchema.Constants.YUS_GS_NKLoadUser, "VARCHAR(3)", "''")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDYardUnitStateSchema.Constants.TableName, CYDYardUnitStateSchema.Constants.YUS_GS_NKUnloadUser, "VARCHAR(3)", "''")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDYardUnitStateSchema.Constants.TableName, CYDYardUnitStateSchema.Constants.YUS_LoadTime, "DATETIMEOFFSET(0)")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDYardUnitStateSchema.Constants.TableName, CYDYardUnitStateSchema.Constants.YUS_UnloadTime, "DATETIMEOFFSET(0)"))
			{
				var sql = @"
DECLARE @CurrentUtcDate DATETIME;
SET @CurrentUtcDate = GetUtcDate();

UPDATE dbo.CYDYardUnitState
SET
    YUS_LoadTime = NULL,
    YUS_GS_NKLoadUser = '',
	YUS_UnloadTime = NULL,
    YUS_GS_NKUnloadUser = '',
    YUS_SystemLastEditTimeUtc = @CurrentUtcDate,
    YUS_SystemLastEditUser = '~BP'
FROM
    dbo.CYDYardUnitState
WHERE
    (((YUS_LoadTime IS NULL AND YUS_GS_NKLoadUser <> '') OR (YUS_LoadTime IS NOT NULL AND YUS_GS_NKLoadUser = ''))
	AND ((YUS_UnloadTime IS NULL AND YUS_GS_NKUnloadUser <> '') OR (YUS_UnloadTime IS NOT NULL AND YUS_GS_NKUnloadUser = '')));

UPDATE dbo.CYDYardUnitState
SET
    YUS_LoadTime = NULL,
    YUS_GS_NKLoadUser = '',
    YUS_SystemLastEditTimeUtc = @CurrentUtcDate,
    YUS_SystemLastEditUser = '~BP'
FROM
    dbo.CYDYardUnitState
WHERE
    ((YUS_LoadTime IS NULL AND YUS_GS_NKLoadUser <> '') OR (YUS_LoadTime IS NOT NULL AND YUS_GS_NKLoadUser = ''));

UPDATE dbo.CYDYardUnitState
SET
    YUS_UnloadTime = NULL,
    YUS_GS_NKUnloadUser = '',
    YUS_SystemLastEditTimeUtc = @CurrentUtcDate,
    YUS_SystemLastEditUser = '~BP'
FROM
    dbo.CYDYardUnitState
WHERE
    ((YUS_UnloadTime IS NULL AND YUS_GS_NKUnloadUser <> '') OR (YUS_UnloadTime IS NOT NULL AND YUS_GS_NKUnloadUser = ''));";

				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}

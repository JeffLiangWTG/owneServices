using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
	class PopulateUnitLineItemForCYDYardUnitState : DataTransformation
	{
		public override string UserDescription => "Add and populate CYDUnitLineItem for CYDYardUnitState";

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();

			DbObjectCreator.CreateTableIfNotExists(Db.Connection, CYDUnitLineItemSchema.Constants.TableName, @"
CREATE TABLE dbo.[CYDUnitLineItem] (
   [YLI_PK] UNIQUEIDENTIFIER NOT NULL,
   [YLI_Type] CHAR(3) NOT NULL DEFAULT '',
   [YLI_ManufactureDate] DATE NULL,
   [YLI_RC_ContainerType] UNIQUEIDENTIFIER NOT NULL,
   [YLI_SealNumber] VARCHAR(20) NOT NULL DEFAULT '',
   [YLI_Quantity] SMALLINT NOT NULL DEFAULT 0,
   [YLI_IsEmpty] BIT NOT NULL DEFAULT 0,
   [YLI_SystemCreateTimeUtc] SMALLDATETIME NULL,
   [YLI_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '',
   [YLI_SystemLastEditTimeUtc] SMALLDATETIME NULL,
   [YLI_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT '',
);");

			if (DbObjectCreator.TableExists(Db.Connection, CYDYardUnitStateSchema.Constants.TableName)
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDYardUnitStateSchema.Constants.TableName, "YUS_YLI_UnitLineItem", "UNIQUEIDENTIFIER")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDUnitLineItemSchema.Constants.TableName, "YLI_IsDamaged", "BIT", defaultValue: "0")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDUnitLineItemSchema.Constants.TableName, "YLI_TareWeight", "DECIMAL(9,3)", defaultValue: "0")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDUnitLineItemSchema.Constants.TableName, "YLI_GrossWeight", "DECIMAL(9, 3)", defaultValue: "0")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDUnitLineItemSchema.Constants.TableName, "YLI_UnitOfWeight", "VARCHAR(2)", defaultValue: "''")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDUnitLineItemSchema.Constants.TableName, "YLI_IsPreAdvice", "BIT", defaultValue: "0")
				)
			{
				var checkMatchingRowsSql = @"
SELECT COUNT(1)
FROM dbo.[CYDYardUnitState]
WHERE 
    [YUS_YDL_Delivery] IS NOT NULL
    AND [YUS_YLI_UnitLineItem] IS NULL
    AND [YUS_UnloadTime] IS NOT NULL;
";

				var matchingRows = Db.Connection.ExecuteScalar<int>(checkMatchingRowsSql);
				if(matchingRows == 0)
				{
					return; // Skip Transform that has already run
				}

				var addLineItemDefaultValueSql = @"
UPDATE dbo.[CYDYardUnitState]
SET 
    [YUS_YLI_UnitLineItem] = NEWID(),
    [YUS_SystemLastEditTimeUtc] = GETUTCDATE(),
    [YUS_SystemLastEditUser] = '~BP'
WHERE 
    [YUS_YDL_Delivery] IS NOT NULL
	AND [YUS_YLI_UnitLineItem] IS NULL
    AND [YUS_UnloadTime] IS NOT NULL;
";
				Db.Connection.ExecuteNonQuery(addLineItemDefaultValueSql);

				var addDataSql = @"
DECLARE @ContainerPK UNIQUEIDENTIFIER;
SELECT TOP 1 @ContainerPK = RC_PK FROM dbo.RefContainer;

INSERT INTO dbo.[CYDUnitLineItem] (
    [YLI_PK],
    [YLI_Type],
    [YLI_RC_ContainerType],
    [YLI_Quantity],
    [YLI_ManufactureDate],
    [YLI_IsEmpty],
    [YLI_IsDamaged],
    [YLI_TareWeight],
    [YLI_GrossWeight],
    [YLI_UnitOfWeight],
    [YLI_IsPreAdvice],
    [YLI_SystemCreateTimeUtc],
    [YLI_SystemCreateUser],
    [YLI_SystemLastEditTimeUtc],
    [YLI_SystemLastEditUser]
)
SELECT 
    Y.YUS_YLI_UnitLineItem,
    U.YLI_Type,
    U.YLI_RC_ContainerType,
    U.YLI_Quantity,
    U.YLI_ManufactureDate,
    U.YLI_IsEmpty,
    U.YLI_IsDamaged,
    U.YLI_TareWeight,
    U.YLI_GrossWeight,
    U.YLI_UnitOfWeight,
    U.YLI_IsPreAdvice,
    U.YLI_SystemCreateTimeUtc,
    U.YLI_SystemCreateUser,
    GETUTCDATE(),
    '~BP'
FROM 
    dbo.CYDYardUnitState Y
    LEFT JOIN dbo.CYDDelivery D ON D.YDL_PK = Y.YUS_YDL_Delivery
    LEFT JOIN dbo.CYDUnitLineItem U ON D.YDL_YLI_UnitLineItem = U.YLI_PK
WHERE 
    Y.YUS_YDL_Delivery IS NOT NULL 
    AND Y.YUS_UnloadTime IS NOT NULL;
";
				Db.Connection.ExecuteNonQuery(addDataSql);
			}
		}
	}
}

using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
	class PopulateUnitLineItemForPickup : DataTransformation
	{
		public override string UserDescription => "Add and populate CYDUnitLineItem for Pickups";

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
			if (DbObjectCreator.TableExists(Db.Connection, CYDPickupSchema.Constants.TableName)
				&& !DbObjectCreator.ColumnExists(Db.Connection, CYDPickupSchema.Constants.TableName, CYDPickupSchema.Constants.YPL_YLI_UnitLineItem)
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDPickupSchema.Constants.TableName, "YPL_Quantity", "SMALLINT", defaultValue: "0")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDPickupSchema.Constants.TableName, "YPL_Type", "CHAR(3)", defaultValue: "''")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDPickupSchema.Constants.TableName, "YPL_RC_ContainerType", "UNIQUEIDENTIFIER")
				)
			{
				var addLineItemColumnSql = @"
ALTER TABLE dbo.[CYDPickup]
ADD [YPL_YLI_UnitLineItem] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID();
";
				Db.Connection.ExecuteNonQuery(addLineItemColumnSql);

				var addDataSql = @"
DECLARE @ContainerPK UNIQUEIDENTIFIER;
SELECT TOP 1 @ContainerPK = RC_PK FROM dbo.RefContainer;

INSERT INTO dbo.[CYDUnitLineItem] (
    [YLI_PK],
    [YLI_Type],
    [YLI_RC_ContainerType],
    [YLI_Quantity],
    [YLI_SystemCreateTimeUtc],
    [YLI_SystemCreateUser],
    [YLI_SystemLastEditTimeUtc],
    [YLI_SystemLastEditUser]
)
SELECT 
    [YPL_YLI_UnitLineItem],
    [YPL_Type],
    CASE 
        WHEN [YPL_RC_ContainerType] IS NULL THEN @ContainerPK
        ELSE [YPL_RC_ContainerType]
    END,
    [YPL_Quantity],
    [YPL_SystemCreateTimeUtc],
    [YPL_SystemCreateUser],
    CASE 
        WHEN [YPL_RC_ContainerType] IS NULL THEN SYSUTCDATETIME()
        ELSE [YPL_SystemLastEditTimeUtc]
    END,
    CASE 
        WHEN [YPL_RC_ContainerType] IS NULL THEN '~BP'
        ELSE [YPL_SystemLastEditUser]
    END
FROM 
    dbo.[CYDPickup]
";
				Db.Connection.ExecuteNonQuery(addDataSql);
			}
		}
	}
}

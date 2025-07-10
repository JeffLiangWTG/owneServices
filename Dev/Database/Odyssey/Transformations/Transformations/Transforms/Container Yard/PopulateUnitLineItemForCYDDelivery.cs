using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
	class PopulateUnitLineItemForCYDDelivery : DataTransformation
	{
		public override string UserDescription => "Add and populate CYDUnitLineItem for CYDDeliveries";

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
			if (DbObjectCreator.TableExists(Db.Connection, CYDDeliverySchema.Constants.TableName)
				&& !DbObjectCreator.ColumnExists(Db.Connection, CYDDeliverySchema.Constants.TableName, CYDDeliverySchema.Constants.YDL_YLI_UnitLineItem)
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDDeliverySchema.Constants.TableName, "YDL_Quantity", "SMALLINT", defaultValue: "0")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDDeliverySchema.Constants.TableName, "YDL_Type", "CHAR(3)", defaultValue: "''")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDDeliverySchema.Constants.TableName, "YDL_ManufactureDate", "DATE")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDDeliverySchema.Constants.TableName, "YDL_RC_ContainerType", "UNIQUEIDENTIFIER")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDDeliverySchema.Constants.TableName, "YDL_IsEmpty", "BIT", defaultValue: "1")
				)
			{
				var addLineItemColumnSql = @"
ALTER TABLE dbo.[CYDDelivery]
ADD [YDL_YLI_UnitLineItem] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID();
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
    [YLI_ManufactureDate],
    [YLI_IsEmpty],
    [YLI_SystemCreateTimeUtc],
    [YLI_SystemCreateUser],
    [YLI_SystemLastEditTimeUtc],
    [YLI_SystemLastEditUser]
)
SELECT 
    [YDL_YLI_UnitLineItem],
    [YDL_Type],
    CASE 
        WHEN [YDL_RC_ContainerType] IS NULL THEN @ContainerPK
        ELSE [YDL_RC_ContainerType]
    END,
    [YDL_Quantity],
    [YDL_ManufactureDate],
    [YDL_IsEmpty],
    [YDL_SystemCreateTimeUtc],
    [YDL_SystemCreateUser],
    CASE 
        WHEN [YDL_RC_ContainerType] IS NULL THEN SYSUTCDATETIME()
        ELSE [YDL_SystemLastEditTimeUtc]
    END,
    CASE 
        WHEN [YDL_RC_ContainerType] IS NULL THEN '~BP'
        ELSE [YDL_SystemLastEditUser]
    END
FROM 
    dbo.[CYDDelivery]
";
				Db.Connection.ExecuteNonQuery(addDataSql);
			}
		}
	}
}

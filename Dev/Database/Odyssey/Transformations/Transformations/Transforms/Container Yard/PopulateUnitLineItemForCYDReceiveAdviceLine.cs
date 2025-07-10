using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
	class PopulateUnitLineItemForCYDReceiveAdviceLine : DataTransformation
	{
		public override string UserDescription => "Add and populate CYDUnitLineItem for ReceiveAdviceLines";

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
			if (DbObjectCreator.TableExists(Db.Connection, CYDReceiveAdviceLineSchema.Constants.TableName)
				&& !DbObjectCreator.ColumnExists(Db.Connection, CYDReceiveAdviceLineSchema.Constants.TableName, CYDReceiveAdviceLineSchema.Constants.YRL_YLI_UnitLineItem)
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDReceiveAdviceLineSchema.Constants.TableName, "YRL_Quantity", "SMALLINT", defaultValue: "0")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDReceiveAdviceLineSchema.Constants.TableName, "YRL_Type", "CHAR(3)", defaultValue: "''")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDReceiveAdviceLineSchema.Constants.TableName, "YRL_ManufactureDate", "DATE")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDReceiveAdviceLineSchema.Constants.TableName, "YRL_RC_ContainerType", "UNIQUEIDENTIFIER")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDReceiveAdviceLineSchema.Constants.TableName, "YRL_IsEmpty", "BIT", defaultValue: "1")
			)
			{
				var addLineItemColumnSql = @"
ALTER TABLE dbo.[CYDReceiveAdviceLine]
ADD [YRL_YLI_UnitLineItem] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID();
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
    [YRL_YLI_UnitLineItem],
    [YRL_Type],
    CASE 
        WHEN [YRL_RC_ContainerType] IS NULL THEN @ContainerPK
        ELSE [YRL_RC_ContainerType]
    END,
    [YRL_Quantity],
    [YRL_ManufactureDate],
    [YRL_IsEmpty],
    [YRL_SystemCreateTimeUtc],
    [YRL_SystemCreateUser],
    CASE 
        WHEN [YRL_RC_ContainerType] IS NULL THEN SYSUTCDATETIME()
        ELSE [YRL_SystemLastEditTimeUtc]
    END,
    CASE 
        WHEN [YRL_RC_ContainerType] IS NULL THEN '~BP'
        ELSE [YRL_SystemLastEditUser]
    END
FROM 
    dbo.[CYDReceiveAdviceLine]
";
				Db.Connection.ExecuteNonQuery(addDataSql);
			}
		}
	}
}

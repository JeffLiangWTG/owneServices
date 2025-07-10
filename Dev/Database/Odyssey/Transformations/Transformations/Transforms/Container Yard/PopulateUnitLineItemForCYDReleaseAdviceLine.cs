using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
	class PopulateUnitLineItemForCYDReleaseAdviceLine : DataTransformation
	{
		public override string UserDescription => "Add and populate CYDUnitLineItem for ReleaseAdviceLines";

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
			if (DbObjectCreator.TableExists(Db.Connection, CYDReleaseAdviceLineSchema.Constants.TableName)
				&& !DbObjectCreator.ColumnExists(Db.Connection, CYDReleaseAdviceLineSchema.Constants.TableName, CYDReleaseAdviceLineSchema.Constants.YEL_YLI_UnitLineItem)
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDReleaseAdviceLineSchema.Constants.TableName, "YEL_Quantity", "SMALLINT", defaultValue: "0")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDReleaseAdviceLineSchema.Constants.TableName, "YEL_Type", "CHAR(3)", defaultValue: "''")
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDReleaseAdviceLineSchema.Constants.TableName, "YEL_RC_ContainerType", "UNIQUEIDENTIFIER")
				)
			{
				var addLineItemColumnSql = @"
ALTER TABLE dbo.[CYDReleaseAdviceLine]
ADD [YEL_YLI_UnitLineItem] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID();
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
    [YEL_YLI_UnitLineItem],
    [YEL_Type],
    CASE 
        WHEN [YEL_RC_ContainerType] IS NULL THEN @ContainerPK
        ELSE [YEL_RC_ContainerType]
    END,
    [YEL_Quantity],
    [YEL_SystemCreateTimeUtc],
    [YEL_SystemCreateUser],
    CASE 
        WHEN [YEL_RC_ContainerType] IS NULL THEN SYSUTCDATETIME()
        ELSE [YEL_SystemLastEditTimeUtc]
    END,
    CASE 
        WHEN [YEL_RC_ContainerType] IS NULL THEN '~BP'
        ELSE [YEL_SystemLastEditUser]
    END
FROM 
    dbo.[CYDReleaseAdviceLine]
";
				Db.Connection.ExecuteNonQuery(addDataSql);
			}
		}
	}
}

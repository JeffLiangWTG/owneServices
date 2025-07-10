using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
	public class PopulateDeliveryID : DataTransformation
	{
		public override string UserDescription => "Add and populate YDL_DeliveryID in CYDDelivery";

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();

			if (DbObjectCreator.TableExists(Db.Connection, CYDDeliverySchema.Constants.TableName)
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDDeliverySchema.Constants.TableName, CYDDeliverySchema.Constants.YDL_DeliveryID, "VARCHAR(20)", defaultValue: "''"))
			{
				var sql = @"
IF NOT EXISTS (SELECT 1 FROM [dbo].[StmNums] WHERE SN_Name = 'NF-YDL_DeliveryID')
BEGIN
WITH CTE AS (
    SELECT 
		COUNT(*) AS MaxValue,
        [YDL_DeliveryID]
    FROM [CYDDelivery]
    WHERE [YDL_DeliveryID] = ''
	GROUP BY [YDL_DeliveryID]
)
INSERT INTO [dbo].[StmNums]
           ([SN_Name]
           ,[SN_Value]
           ,[SN_SystemCreateTimeUtc])
     SELECT
           'NF-YDL_DeliveryID'
           ,ISNULL(MAX(CTE.MaxValue) + 1, 1) AS LastRowNumber
           ,GETUTCDATE()
	FROM CTE
END;

WITH CTE AS (
    SELECT 
        ROW_NUMBER() OVER (ORDER BY (SELECT YDL_SystemCreateTimeUtc)) AS AutoValue,
        [YDL_DeliveryID],
        [YDL_SystemLastEditTimeUtc],
        [YDL_SystemLastEditUser]
    FROM [CYDDelivery]
    WHERE [YDL_DeliveryID] = ''
)
UPDATE CTE
SET 
    [YDL_DeliveryID] = 'YDL' + RIGHT('000000000000' + CAST(AutoValue AS VARCHAR), 12),
    [YDL_SystemLastEditTimeUtc] = GETUTCDATE(),
    [YDL_SystemLastEditUser] = '~BP'
";

				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}

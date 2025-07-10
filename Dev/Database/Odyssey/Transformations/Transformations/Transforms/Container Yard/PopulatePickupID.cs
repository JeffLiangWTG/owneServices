using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
	public class PopulatePickupID : DataTransformation
	{
		public override string UserDescription => "Add and populate YPL_PickupID in CYDPickup";

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();

			if (DbObjectCreator.TableExists(Db.Connection, CYDPickupSchema.Constants.TableName)
				&& DbObjectCreator.CreateColumnIfNotExists(Db.Connection, CYDPickupSchema.Constants.TableName, CYDPickupSchema.Constants.YPL_PickupID, "VARCHAR(20)", defaultValue: "''"))
			{
				var sql = @"
IF NOT EXISTS (SELECT 1 FROM [dbo].[StmNums] WHERE SN_Name = 'NF-YPL_PickupID')
BEGIN
WITH CTE AS (
    SELECT 
		COUNT(*) AS MaxValue,
        [YPL_PickupID]
    FROM [CYDPickup]
    WHERE [YPL_PickupID] = ''
	GROUP BY [YPL_PickupID]
)
INSERT INTO [dbo].[StmNums]
           ([SN_Name]
           ,[SN_Value]
           ,[SN_SystemCreateTimeUtc])
     SELECT
           'NF-YPL_PickupID'
           ,ISNULL(MAX(CTE.MaxValue) + 1, 1) AS LastRowNumber
           ,GETUTCDATE()
	FROM CTE
END;

WITH CTE AS (
    SELECT 
        ROW_NUMBER() OVER (ORDER BY (SELECT YPL_SystemCreateTimeUtc)) AS AutoValue,
        [YPL_PickupID],
        [YPL_SystemLastEditTimeUtc],
        [YPL_SystemLastEditUser]
    FROM [CYDPickup]
    WHERE [YPL_PickupID] = ''
)
UPDATE CTE
SET 
    [YPL_PickupID] = 'YPL' + RIGHT('000000000000' + CAST(AutoValue AS VARCHAR), 12),
    [YPL_SystemLastEditTimeUtc] = GETUTCDATE(),
    [YPL_SystemLastEditUser] = '~BP'
";

				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}

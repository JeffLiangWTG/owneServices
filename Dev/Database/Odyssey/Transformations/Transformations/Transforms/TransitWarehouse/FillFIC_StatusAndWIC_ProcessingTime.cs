using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class FillFIC_StatusAndWIC_ProcessingTime : DataTransformation
	{
		public override string UserDescription => "Populate Status and ProcessingTime for Transit Cycle Count Location.";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"UPDATE dbo.WhsItemCycleCountLocation
SET 
    WIC_Status = 
        CASE 
            WHEN WIC_EndTime IS NOT NULL THEN 'CMP'
            WHEN WIC_StartTime IS NOT NULL AND WIC_EndTime IS NULL THEN 'INP'
            WHEN WIC_StartTime IS NULL THEN 'NST'
        END,
    WIC_ProcessingTime = COALESCE(WIC_EndTime, WIC_ProcessingTime),
    WIC_SystemLastEditTimeUtc = GETDATE(),
    WIC_SystemLastEditUser = '~BP'
FROM dbo.WhsItemCycleCountLocation
WHERE 
    (WIC_EndTime IS NOT NULL AND WIC_Status != 'CMP')
    OR (WIC_StartTime IS NOT NULL AND WIC_EndTime IS NULL AND WIC_Status != 'INP')
    OR (WIC_StartTime IS NULL AND WIC_Status != 'NST');";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}

using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class ChangeStatusToErrorWhenProcessingTimeIsNotNullAndEndTimeIsNull : DataTransformation
	{
		public override string UserDescription => "Change Transit Cycle Count Status to Error when the processing is time out.";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
UPDATE
	dbo.WhsItemCycleCountLocation
SET
	WIC_Status = 'ERR',
	WIC_SystemLastEditTimeUtc = GETDATE(),
	WIC_SystemLastEditUser = '~BP'
WHERE
	WIC_ProcessingTime IS NOT NULL
	AND WIC_EndTime IS NULL;

DISABLE TRIGGER TG_WhsItemCycleCountLocationVariance_PreventDelete ON dbo.WhsItemCycleCountLocationVariance;

DELETE
	dbo.WhsItemCycleCountLocationVariance
FROM
	dbo.WhsItemCycleCountLocationVariance 
LEFT JOIN
	dbo.WhsItemCycleCountLocation ON WIV_WIC_CycleCountLocation = WIC_PK
WHERE
	WIC_Status = 'ERR';

ENABLE TRIGGER TG_WhsItemCycleCountLocationVariance_PreventDelete ON dbo.WhsItemCycleCountLocationVariance;
";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}

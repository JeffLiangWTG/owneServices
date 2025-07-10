using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Freight.ContainerYard
{
	internal class PopulateAcceptanceAndReleaseNumber : DataTransformation
	{
		public override string UserDescription => "Update values for YRA_AcceptanceNumber and YRE_ReleaseNumber";

		protected override void OfflinePostUpgradeTransform()
		{
			base.OfflinePostUpgradeTransform();

				var sql = @"
DECLARE @CurrentUtcDate DATETIME;
SET @CurrentUtcDate = GetUtcDate();

UPDATE [dbo].[CYDReceiveAdvice]
SET
   [YRA_AcceptanceNumber] = YRA_JobNumber,
   [YRA_SystemLastEditTimeUtc] = @CurrentUtcDate,
   [YRA_SystemLastEditUser] = '~BP'
WHERE [YRA_AcceptanceNumber] = ''

UPDATE [dbo].[CYDReleaseAdvice]
SET
  [YRE_ReleaseNumber] = YRE_JobNumber,
  [YRE_SystemLastEditTimeUtc] = @CurrentUtcDate,
  [YRE_SystemLastEditUser] = '~BP'
WHERE  [YRE_ReleaseNumber] = ''";

				Db.Connection.ExecuteNonQuery(sql);
		}
	}
}

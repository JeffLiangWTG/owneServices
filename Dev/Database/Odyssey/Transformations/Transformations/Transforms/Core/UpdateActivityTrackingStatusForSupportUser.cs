using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	public class UpdateActivityTrackingStatusForSupportUser : DataTransformation
	{
		public override string UserDescription => "Update GS_ActivityTrackingStatus To Yes For Support User";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
UPDATE
	dbo.GlbStaff
SET
	GS_ActivityTrackingStatus = 'YES',
	GS_SystemLastEditTimeUtc = GetUtcDate(),
	GS_SystemLastEditUser = '~BP'
WHERE
	GS_LoginName = 'CW1Support'
 ";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}

using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding.Contracts
{
	class SetAllowRelatedPortsToFalseWhenScheduleIsLinked : DataTransformation
	{
		public override string UserDescription => "Set allow related ports to false when a sailing schedule is attached to an allocation line";

		protected override void OfflinePostUpgradeTransform()
		{
			var sqlQuery = @"
UPDATE dbo.RatingContractAllocationLine
SET RCA_AllowRelatedPorts = 0, RCA_SystemLastEditTimeUtc = GetUtcDate(), RCA_SystemLastEditUser = '~BP'
WHERE RCA_JX_SailingSchedule IS NOT NULL";

			Db.Connection.ExecuteNonQuery(sqlQuery);
		}
	}
}

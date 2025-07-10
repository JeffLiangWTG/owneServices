using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding
{
	public class SetContainerWeightLimitTypeToAdjustedCodes : DataTransformation
	{
		public override string UserDescription => "Adjust RCA_ContainerWeightLimitType to new constraint values";

		protected override void OfflinePostUpgradeTransform()
		{
			base.OfflinePostUpgradeTransform();

			var sql = @"
UPDATE
	dbo.RatingContractAllocationLine
SET
	RCA_ContainerWeightLimitType = CASE
		WHEN RCA_ContainerWeightLimitType = 'ABS' THEN 'ABT'
		WHEN RCA_ContainerWeightLimitType = 'AVG' THEN 'AVT'
	END,
	RCA_SystemLastEditUser = '~BP',
	RCA_SystemLastEditTimeUtc = GETUTCDATE()
WHERE
	RCA_ContainerWeightLimitType IN ('ABS', 'AVG');";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}

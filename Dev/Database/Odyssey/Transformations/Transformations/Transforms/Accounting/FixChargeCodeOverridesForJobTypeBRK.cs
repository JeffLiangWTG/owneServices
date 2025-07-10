using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	public sealed class FixChargeCodeOverridesForJobTypeBRK : DataTransformation
	{
		public override string UserDescription => "Fill Direction and Transport Mode columns with 'ALL' for BRK Job Type on Charge Creditor and Branch Overrides";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
BEGIN TRY
	UPDATE dbo.AccChargeCreditorOverride
	SET ACC_Direction = 'ALL',
		ACC_TransportMode = 'ALL',
		ACC_SystemLastEditTimeUtc = GETUTCDATE(),
		ACC_SystemLastEditUser = 'E'
	WHERE ACC_JobType = 'BRK'
		AND ACC_Direction = ''
		AND ACC_TransportMode = ''

	UPDATE dbo.AccChargeBranchOverride
	SET YA_Direction = 'ALL',
		YA_TransportMode = 'ALL',
		YA_SystemLastEditTimeUtc = GETUTCDATE(),
		YA_SystemLastEditUser = 'E'
	WHERE YA_JobType = 'BRK'
		AND YA_Direction = ''
		AND YA_TransportMode = ''
END TRY
BEGIN CATCH
	THROW
END CATCH";

			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}

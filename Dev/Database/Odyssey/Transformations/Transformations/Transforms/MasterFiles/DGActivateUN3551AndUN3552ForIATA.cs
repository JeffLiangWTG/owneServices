using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterFiles
{
	public class DGActivateUN3551AndUN3552ForIATA : DataTransformation
	{
		public override string UserDescription => "IATA DGR 66th edition - Activate UN3551+UN3552";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
	UPDATE dbo.ZZUNDGSubstance
	SET
		DG_IsActive = 1,
		DG_SystemLastEditTimeUtc = GETUTCDATE(),
		DG_SystemLastEditUser = '~BP'
	WHERE DG_Standard = 'IAT' AND DG_UNNO IN ('3551', '3552')";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}
	}
}

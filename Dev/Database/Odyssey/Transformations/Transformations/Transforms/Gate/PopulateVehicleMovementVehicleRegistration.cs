using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Gate
{
	public class PopulateVehicleMovementVehicleRegistration : DataTransformation
	{
		public override string UserDescription => "Populate GteVehicleMovement.GVM_VehicleRegistration for updated constraint Constraint_GVM_VehicleRegistration";

		protected override void OfflinePostUpgradeTransform()
		{
			const string sql = @"
UPDATE
	dbo.GteVehicleMovement
SET
	GVM_VehicleRegistration = 'N/A',
	GVM_SystemLastEditTimeUtc = GETUTCDATE(),
	GVM_SystemLastEditUser = '~BP'
WHERE
	GVM_VehicleRegistration = '';
";
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}

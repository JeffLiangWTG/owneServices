using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Core.Constants;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Gate;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Gate
{
	[TestedType(typeof(PopulateVehicleMovementVehicleRegistration))]
	public class PopulateVehicleMovementVehicleRegistrationTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateVehicleMovementVehicleRegistration();

		protected override void PrepareTestData()
		{
			using (DataTransformationHelper.SuspendConstraintCheckingIfExists(TestConnection, GteVehicleMovementSchema.Constants.SqlSchemaName, GteVehicleMovementSchema.Constants.TableName, "Constraint_GVM_VehicleRegistration"))
			{
				var sql = new StringBuilder();
				var org = new OrgHeader("ORG").AppendInsertAndReturnObject(sql);
				var facility = new WhsWarehouse("CYD", WarehouseTypes.ContainerYard).AppendInsertAndReturnObject(sql);
				var refContainer = new RefContainer("CONTAINER1", "TCK").AppendInsertAndReturnObject(sql);
				var booking = new GteBooking(org, facility, "GBK001").AppendInsertAndReturnObject(sql);
				new GteVehicleMovement(booking, refContainer, "").AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToString());
			}
		}

		protected override void AssertTransformationResults()
		{
			var vehicleMovement = GteVehicleMovement.ShallowLoadFromDB(TestConnection)[0];
			AssertEquals("Expected GteVehicleMovement GVM_VehicleRegistration not be empty", "N/A", vehicleMovement.GVM_VehicleRegistration);
			AssertEquals("Expected GteVehicleMovement GVM_SystemLastEditUser to be updated", "~BP", vehicleMovement.GVM_SystemLastEditUser);
		}
	}
}

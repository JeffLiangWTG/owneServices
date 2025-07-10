using System.Text;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Core.Constants;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Gate;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Gate
{
	[TestedType(typeof(PopulateMovementBookingNumber))]
	public class PopulateMovementBookingNumberTest : DataTransformationTestCase
	{
		GteGateMovementBooking movementBooking01;
		GteGateMovementBooking movementBooking02;
		GteGateMovementBooking movementBooking03;

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateMovementBookingNumber();

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(GteGateMovementBookingSchema.Constants.TableName, "Constraint_GBM_MovementBookingNumber");
			DBTransformationTestHelper.DropConstraintIfExists(GteGateMovementBookingSchema.Constants.TableName, "DF_GteGateMovementBooking_GBM_MovementBookingNumber");
			DBTransformationTestHelper.DropIndexIfExists(GteGateMovementBookingSchema.Constants.TableName, GteGateMovementBookingSchema.Constants.Indexes.NR_UX__GBM_MovementBookingNumber);
			DBTransformationTestHelper.DropColumnIfExists(GteGateMovementBookingSchema.Constants.TableName, GteGateMovementBookingSchema.Constants.GBM_MovementBookingNumber);
			RemoveExistingNumberFountainIfExists();

			var sql = new StringBuilder();
			var orgHeader = new OrgHeader("ORG").AppendInsertAndReturnObject(sql);
			var containerYardFacility = new WhsWarehouse("CY1", WarehouseTypes.ContainerYard).AppendInsertAndReturnObject(sql);
			var booking = new GteBooking(orgHeader, containerYardFacility, "RFN001").AppendInsertAndReturnObject(sql);
			movementBooking01 = new GteGateMovementBooking(booking).AppendInsertAndReturnObject(sql);
			movementBooking02 = new GteGateMovementBooking(booking).AppendInsertAndReturnObject(sql);
			movementBooking03 = new GteGateMovementBooking(booking).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals("Expected MovementBookingNumber to be GBM00000001", expected: "GBM00000001", GteGateMovementBooking.ShallowLoadFromDB(TestConnection, movementBooking01.PK).GBM_MovementBookingNumber);
			AssertEquals("Expected MovementBookingNumber to be GBM00000002", expected: "GBM00000002", GteGateMovementBooking.ShallowLoadFromDB(TestConnection, movementBooking02.PK).GBM_MovementBookingNumber);
			AssertEquals("Expected MovementBookingNumber to be GBM00000003", expected: "GBM00000003", GteGateMovementBooking.ShallowLoadFromDB(TestConnection, movementBooking03.PK).GBM_MovementBookingNumber);
			Assert("Expected NumberFountain value to exist", TestConnection.Exists("FROM dbo.StmNums WHERE SN_NAME = 'GteMovementBookingNumber'"));
			Assert("Expected NumberFountain value to be 4", TestConnection.Exists("FROM dbo.StmNums WHERE SN_NAME = 'GteMovementBookingNumber' AND SN_Value = 4"));
		}

		public void TestGivenGteGateMovementBooking_WhenGBMHasMovementBookingColumnPopulated_ThenDoNotChange()
		{
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, GteGateMovementBookingSchema.Constants.TableName, GteGateMovementBookingSchema.Constants.GBM_MovementBookingNumber, "VARCHAR(11)", defaultValue: "''");

			var sql = new StringBuilder();
			var orgHeader = new OrgHeader("ORG").AppendInsertAndReturnObject(sql);
			var containerYardFacility = new WhsWarehouse("CY1", WarehouseTypes.ContainerYard).AppendInsertAndReturnObject(sql);
			var booking = new GteBooking(orgHeader, containerYardFacility, "RFN001").AppendInsertAndReturnObject(sql);
			movementBooking01 = new GteGateMovementBooking(booking);
			movementBooking01.GBM_MovementBookingNumber = "GBM00000001";
			movementBooking01.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var transformation = new PopulateMovementBookingNumber();
			transformation.Run();

			AssertEquals("Expected MovementBookingNumber to not change", expected: "GBM00000001", GteGateMovementBooking.ShallowLoadFromDB(TestConnection, movementBooking01.PK).GBM_MovementBookingNumber);
		}

		[ExpectNoExceptions]
		public void TestGivenMovementBookingNumberFountainExists_WhenTransformationRun_ThenDoNotThrowError()
		{
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, GteGateMovementBookingSchema.Constants.TableName, GteGateMovementBookingSchema.Constants.GBM_MovementBookingNumber, "VARCHAR(11)", defaultValue: "''");
			RemoveExistingNumberFountainIfExists();

			var sql = new StringBuilder();
			var orgHeader = new OrgHeader("ORG").AppendInsertAndReturnObject(sql);
			var containerYardFacility = new WhsWarehouse("CY1", WarehouseTypes.ContainerYard).AppendInsertAndReturnObject(sql);
			var booking = new GteBooking(orgHeader, containerYardFacility, "RFN001").AppendInsertAndReturnObject(sql);
			movementBooking01 = new GteGateMovementBooking(booking);
			movementBooking01.GBM_MovementBookingNumber = "GBM00000001";
			movementBooking01.AppendInsertAndReturnObject(sql);

			var createNumberFountainSQL = @"INSERT INTO dbo.StmNums (SN_NAME, SN_Value) VALUES ('GteMovementBookingNumber', 10)";

			TestConnection.ExecuteNonQuery(sql.ToString());
			TestConnection.ExecuteNonQuery(createNumberFountainSQL);

			var transformation = new PopulateMovementBookingNumber();
			transformation.Run();
		}

		void RemoveExistingNumberFountainIfExists()
		{
			TestConnection.ExecuteNonQuery(@"DELETE FROM [StmNums] WHERE SN_Name = 'GteMovementBookingNumber'");
		}
	}
}

using System;
using System.Text;
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
	[TestedType(typeof(PopulateGateBookingFacility))]
	public class PopulateGateBookingFacilityTest : DataTransformationTestCase
	{
		WhsWarehouse containerYardFacility;
		GteBookingOld_V01 booking1;
		GteBookingOld_V01 booking2;
		GteBookingOld_V01 booking3;

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateGateBookingFacility();

		protected override void PrepareTestData()
		{
			var sql = new StringBuilder();

			DBTransformationTestHelper.DropConstraintIfExists(GteGateSchema.Constants.TableName, "Constraint_GTE_IsActive");
			DBTransformationTestHelper.DropConstraintIfExists(GteBookingSchema.Constants.TableName, "GteBooking_GBK_WW_Facility_FK2_WhsWarehouse_RRR_120N");
			DBTransformationTestHelper.DropIndexIfExists(GteBookingSchema.Constants.TableName, "FK_RX__GBK_WW_Facility");
			DBTransformationTestHelper.DropColumnIfExists(GteBookingSchema.Constants.TableName, GteBookingSchema.Constants.GBK_WW_Facility);
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, GteGateSchema.Constants.TableName, "GTE_OA_Address", "UNIQUEIDENTIFIER");

			var transportCompany = new OrgHeader("TRN").AppendInsertAndReturnObject(sql);
			var facilityCompany = new OrgHeader("FAC").AppendInsertAndReturnObject(sql);
			var facilityAddress1 = new OrgAddress(facilityCompany, "OA1", "One Avenue").AppendInsertAndReturnObject(sql);
			var facilityAddress2 = new OrgAddress(facilityCompany, "OA2", "Two Avenue").AppendInsertAndReturnObject(sql);
			var facilityAddress3 = new OrgAddress(facilityCompany, "OA3", "Three Avenue").AppendInsertAndReturnObject(sql);
			var transitFacility = new WhsWarehouse("TW2", WarehouseTypes.Transit, address: facilityAddress2.PK).AppendInsertAndReturnObject(sql);
			var productFacility = new WhsWarehouse("PW3", WarehouseTypes.Product, address: facilityAddress3.PK).WithDockDoor(sql);
			containerYardFacility = new WhsWarehouse("CY1", WarehouseTypes.ContainerYard, address: facilityAddress1.PK).AppendInsertAndReturnObject(sql);

			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
			var gate = new GteGateOld_V01(facilityAddress1, "G01", "This is a gate description").AppendInsertAndReturnObject(sql);
			var lane = new GteLane(gate, "L01", GteGateDirections.Both).AppendInsertAndReturnObject(sql);

			booking1 = new GteBookingOld_V01(transportCompany, "GBK001").AppendInsertAndReturnObject(sql);
			var gateMovementBookingA = new GteGateMovementBooking(booking1) { GBM_MovementBookingNumber = "GBM01" }.AppendInsertAndReturnObject(sql);
			var vehicleDriverBookingA = new GteVehicleDriverBooking(booking1, "David", "LIC123").AppendInsertAndReturnObject(sql);
			var vehicleMovementBookingA = new GteVehicleMovementBooking(booking1) { GBV_VehicleRegistration = "TRUCK001" }.AppendInsertAndReturnObject(sql);

			booking2 = new GteBookingOld_V01(transportCompany, "GBK002").AppendInsertAndReturnObject(sql);
			var gateMovementBookingB = new GteGateMovementBooking(booking2) { GBM_MovementBookingNumber = "GBM02" }.AppendInsertAndReturnObject(sql);
			var vehicleDriverBookingB = new GteVehicleDriverBooking(booking2, "Isaac", "LIC456").AppendInsertAndReturnObject(sql);
			var vehicleMovementBookingB = new GteVehicleMovementBooking(booking2) { GBV_VehicleRegistration = "TRUCK002" }.AppendInsertAndReturnObject(sql);

			booking3 = new GteBookingOld_V01(transportCompany, "GBK003").AppendInsertAndReturnObject(sql);
			var gateMovementBookingC = new GteGateMovementBooking(booking3) { GBM_MovementBookingNumber = "GBM03" }.AppendInsertAndReturnObject(sql);
			var vehicleDriverBookingC = new GteVehicleDriverBooking(booking3, "Zechariah", "LIC789").AppendInsertAndReturnObject(sql);
			var vehicleMovementBookingC = new GteVehicleMovementBooking(booking3) { GBV_VehicleRegistration = "TRUCK003" }.AppendInsertAndReturnObject(sql);

			var vehicleMovement1 = new GteVehicleMovement(booking1, refContainer, "TRUCK001").AppendInsertAndReturnObject(sql);
			var vehicleEntryA = new GteVehicleEntry(vehicleMovement1, lane, "GVE001", DateTimeOffset.UtcNow.AddSeconds(0), true, "Nathan", "12345").AppendInsertAndReturnObject(sql);
			var gateMovementA = new GteGateMovement(gateMovementBookingA, vehicleMovement1).AppendInsertAndReturnObject(sql);

			var vehicleMovement2 = new GteVehicleMovement(booking2, refContainer, "TRUCK002").AppendInsertAndReturnObject(sql);
			var vehicleEntryB = new GteVehicleEntry(vehicleMovement2, lane, "GVE002", DateTimeOffset.UtcNow.AddSeconds(0), true, "Nathan", "12345").AppendInsertAndReturnObject(sql);
			var gateMovementB = new GteGateMovement(gateMovementBookingB, vehicleMovement2).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertTransformationResults()
		{
			var warehouses = WhsWarehouse.ShallowLoadFromDB(TestConnection);
			var bookings = GteBooking.ShallowLoadFromDB(TestConnection);
			var gateMovementBookings = GteGateMovementBooking.ShallowLoadFromDB(TestConnection);
			var vehicleDriverBookings = GteVehicleDriverBooking.ShallowLoadFromDB(TestConnection);
			var vehicleMovementBookings = GteVehicleMovementBooking.ShallowLoadFromDB(TestConnection);
			var vehicleMovements = GteVehicleMovement.ShallowLoadFromDB(TestConnection);
			var vehicleEntries = GteVehicleEntry.ShallowLoadFromDB(TestConnection);
			var gateMovements = GteGateMovement.ShallowLoadFromDB(TestConnection);
			var gates = GteGateOld_V01.ShallowLoadFromDB(TestConnection);
			var lanes = GteLane.ShallowLoadFromDB(TestConnection);

			AssertEquals("Expected three WhsWarehouse rows", 3, warehouses.Length);
			AssertEquals("Expected three GteBooking rows", 3, bookings.Length);
			AssertEquals("Expected two GteGateMovementBooking rows", 3, gateMovementBookings.Length);
			AssertEquals("Expected two GteVehicleDriverBooking rows", 3, vehicleDriverBookings.Length);
			AssertEquals("Expected two GteVehicleMovementBooking rows", 3, vehicleMovementBookings.Length);
			AssertEquals("Expected two GteVehicleMovement rows", 2, vehicleMovements.Length);
			AssertEquals("Expected two GteVehicleEntry rows", 2, vehicleEntries.Length);
			AssertEquals("Expected two GteGateMovement rows", 2, gateMovements.Length);
			AssertEquals("Expected one GteGate row", 1, gates.Length);
			AssertEquals("Expected one GteLane row", 1, lanes.Length);

			var updatedBooking1 = GteBooking.ShallowLoadFromDB(TestConnection, booking1.PK);
			var updatedBooking2 = GteBooking.ShallowLoadFromDB(TestConnection, booking2.PK);
			var updatedBooking3 = GteBooking.ShallowLoadFromDB(TestConnection, booking3.PK);

			AssertEquals("Expected booking1.GBK_WW_Facility to be linked to containerYardFacility.PK", containerYardFacility.PK, updatedBooking1.GBK_WW_Facility.FK);
			AssertEquals("Expected booking2.GBK_WW_Facility to be linked to containerYardFacility.PK", containerYardFacility.PK, updatedBooking2.GBK_WW_Facility.FK);
			AssertEquals("Expected booking3.GBK_WW_Facility to be linked to containerYardFacility.PK", containerYardFacility.PK, updatedBooking3.GBK_WW_Facility.FK);
		}

		public void TestWhenWhsWarehouseTableIsEmpty_ThenDeleteAllRowsInGateManagementSchema_ExceptGTEAndGLNTable()
		{
			var sql = new StringBuilder();

			DBTransformationTestHelper.DropConstraintIfExists(GteGateSchema.Constants.TableName, "Constraint_GTE_IsActive");
			DBTransformationTestHelper.DropConstraintIfExists(GteBookingSchema.Constants.TableName, "GteBooking_GBK_WW_Facility_FK2_WhsWarehouse_RRR_120N");
			DBTransformationTestHelper.DropIndexIfExists(GteBookingSchema.Constants.TableName, "FK_RX__GBK_WW_Facility");
			DBTransformationTestHelper.DropColumnIfExists(GteBookingSchema.Constants.TableName, GteBookingSchema.Constants.GBK_WW_Facility);
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, GteGateSchema.Constants.TableName, "GTE_OA_Address", "UNIQUEIDENTIFIER");

			var transportCompany = new OrgHeader("TRN").AppendInsertAndReturnObject(sql);
			var facilityCompany = new OrgHeader("FAC").AppendInsertAndReturnObject(sql);
			var facilityAddress1 = new OrgAddress(facilityCompany, "OA1", "One Avenue").AppendInsertAndReturnObject(sql);

			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
			var gate = new GteGateOld_V01(facilityAddress1, "G01", "This is a gate description").AppendInsertAndReturnObject(sql);
			var lane = new GteLane(gate, "L01", GteGateDirections.Both).AppendInsertAndReturnObject(sql);

			var booking1 = new GteBookingOld_V01(transportCompany, "GBK001").AppendInsertAndReturnObject(sql);
			var gateMovementBookingA = new GteGateMovementBooking(booking1) { GBM_MovementBookingNumber = "GBM01" }.AppendInsertAndReturnObject(sql);
			var vehicleDriverBookingA = new GteVehicleDriverBooking(booking1, "Jedidiah", "LIC123").AppendInsertAndReturnObject(sql);
			var vehicleMovementBookingA = new GteVehicleMovementBooking(booking1) { GBV_VehicleRegistration = "TRUCK001" }.AppendInsertAndReturnObject(sql);

			var booking2 = new GteBookingOld_V01(transportCompany, "GBK002").AppendInsertAndReturnObject(sql);
			var gateMovementBookingB = new GteGateMovementBooking(booking2) { GBM_MovementBookingNumber = "GBM02" }.AppendInsertAndReturnObject(sql);
			var vehicleDriverBookingB = new GteVehicleDriverBooking(booking2, "Aloysius", "LIC456").AppendInsertAndReturnObject(sql);
			var vehicleMovementBookingB = new GteVehicleMovementBooking(booking2) { GBV_VehicleRegistration = "TRUCK002" }.AppendInsertAndReturnObject(sql);

			var vehicleMovement1 = new GteVehicleMovement(booking1, refContainer, "TRUCK001").AppendInsertAndReturnObject(sql);
			var vehicleEntryA = new GteVehicleEntry(vehicleMovement1, lane, "GVE001", DateTimeOffset.UtcNow.AddSeconds(0), true, "Nathan", "12345").AppendInsertAndReturnObject(sql);
			var gateMovementA = new GteGateMovement(gateMovementBookingA, vehicleMovement1).AppendInsertAndReturnObject(sql);

			var vehicleMovement2 = new GteVehicleMovement(booking2, refContainer, "TRUCK002").AppendInsertAndReturnObject(sql);
			var vehicleEntryB = new GteVehicleEntry(vehicleMovement2, lane, "GVE002", DateTimeOffset.UtcNow.AddSeconds(0), true, "Nathan", "12345").AppendInsertAndReturnObject(sql);
			var gateMovementB = new GteGateMovement(gateMovementBookingB, vehicleMovement2).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var warehouses = WhsWarehouse.ShallowLoadFromDB(TestConnection);
			var bookings = GteBookingOld_V01.ShallowLoadFromDB(TestConnection);
			var gateMovementBookings = GteGateMovementBooking.ShallowLoadFromDB(TestConnection);
			var vehicleDriverBookings = GteVehicleDriverBooking.ShallowLoadFromDB(TestConnection);
			var vehicleMovementBookings = GteVehicleMovementBooking.ShallowLoadFromDB(TestConnection);
			var vehicleMovements = GteVehicleMovement.ShallowLoadFromDB(TestConnection);
			var vehicleEntries = GteVehicleEntry.ShallowLoadFromDB(TestConnection);
			var gateMovements = GteGateMovement.ShallowLoadFromDB(TestConnection);
			var gates = GteGateOld_V01.ShallowLoadFromDB(TestConnection);
			var lanes = GteLane.ShallowLoadFromDB(TestConnection);

			AssertEquals("Pre-condition: Expected WhsWarehouse to be empty", 0, warehouses.Length);
			AssertEquals("Pre-condition: Expected two GteBooking rows", 2, bookings.Length);
			AssertEquals("Pre-condition: Expected two GteGateMovementBooking rows", 2, gateMovementBookings.Length);
			AssertEquals("Pre-condition: Expected two GteVehicleDriverBooking rows", 2, vehicleDriverBookings.Length);
			AssertEquals("Pre-condition: Expected two GteVehicleMovementBooking rows", 2, vehicleMovementBookings.Length);
			AssertEquals("Pre-condition: Expected two GteVehicleMovement rows", 2, vehicleMovements.Length);
			AssertEquals("Pre-condition: Expected two GteVehicleEntry rows", 2, vehicleEntries.Length);
			AssertEquals("Pre-condition: Expected two GteGateMovement rows", 2, gateMovements.Length);
			AssertEquals("Pre-condition: Expected one GteGate row", 1, gates.Length);
			AssertEquals("Pre-condition: Expected one GteLane row", 1, lanes.Length);

			var transformation = new PopulateGateBookingFacility();
			transformation.Run();

			warehouses = WhsWarehouse.ShallowLoadFromDB(TestConnection);
			bookings = GteBookingOld_V01.ShallowLoadFromDB(TestConnection);
			gateMovementBookings = GteGateMovementBooking.ShallowLoadFromDB(TestConnection);
			vehicleDriverBookings = GteVehicleDriverBooking.ShallowLoadFromDB(TestConnection);
			vehicleMovementBookings = GteVehicleMovementBooking.ShallowLoadFromDB(TestConnection);
			vehicleMovements = GteVehicleMovement.ShallowLoadFromDB(TestConnection);
			vehicleEntries = GteVehicleEntry.ShallowLoadFromDB(TestConnection);
			gateMovements = GteGateMovement.ShallowLoadFromDB(TestConnection);
			gates = GteGateOld_V01.ShallowLoadFromDB(TestConnection);
			lanes = GteLane.ShallowLoadFromDB(TestConnection);

			AssertEquals("Expected WhsWarehouse to be empty", 0, warehouses.Length);
			AssertEquals("Expected GteBooking to be empty", 0, bookings.Length);
			AssertEquals("Expected GteGateMovementBooking to be empty", 0, gateMovementBookings.Length);
			AssertEquals("Expected GteVehicleDriverBooking to be empty", 0, vehicleDriverBookings.Length);
			AssertEquals("Expected GteVehicleMovementBooking to be empty", 0, vehicleMovementBookings.Length);
			AssertEquals("Expected GteVehicleMovement to be empty", 0, vehicleMovements.Length);
			AssertEquals("Expected GteVehicleEntry to be empty", 0, vehicleEntries.Length);
			AssertEquals("Expected GteGateMovement to be empty", 0, gateMovements.Length);
			AssertEquals("Expected one GteGate row", 1, gates.Length);
			AssertEquals("Expected one GteLane row", 1, lanes.Length);
		}

		public void TestWhenWhsWarehouseTableHasNoActiveWW_ThenDeleteAllRowsInGateManagementSchema_ExceptGTEAndGLNTable()
		{
			var sql = new StringBuilder();

			DBTransformationTestHelper.DropConstraintIfExists(GteGateSchema.Constants.TableName, "Constraint_GTE_IsActive");
			DBTransformationTestHelper.DropConstraintIfExists(GteBookingSchema.Constants.TableName, "GteBooking_GBK_WW_Facility_FK2_WhsWarehouse_RRR_120N");
			DBTransformationTestHelper.DropIndexIfExists(GteBookingSchema.Constants.TableName, "FK_RX__GBK_WW_Facility");
			DBTransformationTestHelper.DropColumnIfExists(GteBookingSchema.Constants.TableName, GteBookingSchema.Constants.GBK_WW_Facility);
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, GteGateSchema.Constants.TableName, "GTE_OA_Address", "UNIQUEIDENTIFIER");

			var transportCompany = new OrgHeader("TRN").AppendInsertAndReturnObject(sql);
			var facilityCompany = new OrgHeader("FAC").AppendInsertAndReturnObject(sql);
			var facilityAddress1 = new OrgAddress(facilityCompany, "OA1", "One Avenue").AppendInsertAndReturnObject(sql);
			var facilityAddress2 = new OrgAddress(facilityCompany, "OA2", "Two Avenue").AppendInsertAndReturnObject(sql);

			var inactiveTransitFacility = new WhsWarehouse("TF1", WarehouseTypes.Transit, address: facilityAddress1.PK);
			inactiveTransitFacility.WW_IsActive = false;
			inactiveTransitFacility.AppendInsertAndReturnObject(sql);

			var inactiveContainerYardFacility = new WhsWarehouse("CY2", WarehouseTypes.ContainerYard, address: facilityAddress2.PK);
			inactiveContainerYardFacility.WW_IsActive = false;
			inactiveContainerYardFacility.AppendInsertAndReturnObject(sql);

			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
			var gate = new GteGateOld_V01(facilityAddress1, "G01", "This is a gate description").AppendInsertAndReturnObject(sql);
			var lane = new GteLane(gate, "L01", GteGateDirections.Both).AppendInsertAndReturnObject(sql);

			var booking1 = new GteBookingOld_V01(transportCompany, "GBK001").AppendInsertAndReturnObject(sql);
			var gateMovementBookingA = new GteGateMovementBooking(booking1) { GBM_MovementBookingNumber = "GBM01" }.AppendInsertAndReturnObject(sql);
			var vehicleDriverBookingA = new GteVehicleDriverBooking(booking1, "Samuel", "LIC123").AppendInsertAndReturnObject(sql);
			var vehicleMovementBookingA = new GteVehicleMovementBooking(booking1) { GBV_VehicleRegistration = "TRUCK001" }.AppendInsertAndReturnObject(sql);

			var booking2 = new GteBookingOld_V01(transportCompany, "GBK002").AppendInsertAndReturnObject(sql);
			var gateMovementBookingB = new GteGateMovementBooking(booking2) { GBM_MovementBookingNumber = "GBM02" }.AppendInsertAndReturnObject(sql);
			var vehicleDriverBookingB = new GteVehicleDriverBooking(booking2, "Saul", "LIC456").AppendInsertAndReturnObject(sql);
			var vehicleMovementBookingB = new GteVehicleMovementBooking(booking2) { GBV_VehicleRegistration = "TRUCK002" }.AppendInsertAndReturnObject(sql);

			var vehicleMovement1 = new GteVehicleMovement(booking1, refContainer, "TRUCK001").AppendInsertAndReturnObject(sql);
			var vehicleEntryA = new GteVehicleEntry(vehicleMovement1, lane, "GVE001", DateTimeOffset.UtcNow.AddSeconds(0), true, "Nathan", "12345").AppendInsertAndReturnObject(sql);
			var gateMovementA = new GteGateMovement(gateMovementBookingA, vehicleMovement1).AppendInsertAndReturnObject(sql);

			var vehicleMovement2 = new GteVehicleMovement(booking2, refContainer, "TRUCK002").AppendInsertAndReturnObject(sql);
			var vehicleEntryB = new GteVehicleEntry(vehicleMovement2, lane, "GVE002", DateTimeOffset.UtcNow.AddSeconds(0), true, "Nathan", "12345").AppendInsertAndReturnObject(sql);
			var gateMovementB = new GteGateMovement(gateMovementBookingB, vehicleMovement2).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var warehouses = WhsWarehouse.ShallowLoadFromDB(TestConnection);
			var bookings = GteBookingOld_V01.ShallowLoadFromDB(TestConnection);
			var gateMovementBookings = GteGateMovementBooking.ShallowLoadFromDB(TestConnection);
			var vehicleDriverBookings = GteVehicleDriverBooking.ShallowLoadFromDB(TestConnection);
			var vehicleMovementBookings = GteVehicleMovementBooking.ShallowLoadFromDB(TestConnection);
			var vehicleMovements = GteVehicleMovement.ShallowLoadFromDB(TestConnection);
			var vehicleEntries = GteVehicleEntry.ShallowLoadFromDB(TestConnection);
			var gateMovements = GteGateMovement.ShallowLoadFromDB(TestConnection);
			var gates = GteGateOld_V01.ShallowLoadFromDB(TestConnection);
			var lanes = GteLane.ShallowLoadFromDB(TestConnection);

			AssertEquals("Pre-condition: Expected two WhsWarehouse rows", 2, warehouses.Length);
			AssertEquals("Pre-condition: Expected two GteBooking rows", 2, bookings.Length);
			AssertEquals("Pre-condition: Expected two GteGateMovementBooking rows", 2, gateMovementBookings.Length);
			AssertEquals("Pre-condition: Expected two GteVehicleDriverBooking rows", 2, vehicleDriverBookings.Length);
			AssertEquals("Pre-condition: Expected two GteVehicleMovementBooking rows", 2, vehicleMovementBookings.Length);
			AssertEquals("Pre-condition: Expected two GteVehicleMovement rows", 2, vehicleMovements.Length);
			AssertEquals("Pre-condition: Expected two GteVehicleEntry rows", 2, vehicleEntries.Length);
			AssertEquals("Pre-condition: Expected two GteGateMovement rows", 2, gateMovements.Length);
			AssertEquals("Pre-condition: Expected one GteGate row", 1, gates.Length);
			AssertEquals("Pre-condition: Expected one GteLane row", 1, lanes.Length);

			var transformation = new PopulateGateBookingFacility();
			transformation.Run();

			warehouses = WhsWarehouse.ShallowLoadFromDB(TestConnection);
			bookings = GteBookingOld_V01.ShallowLoadFromDB(TestConnection);
			gateMovementBookings = GteGateMovementBooking.ShallowLoadFromDB(TestConnection);
			vehicleDriverBookings = GteVehicleDriverBooking.ShallowLoadFromDB(TestConnection);
			vehicleMovementBookings = GteVehicleMovementBooking.ShallowLoadFromDB(TestConnection);
			vehicleMovements = GteVehicleMovement.ShallowLoadFromDB(TestConnection);
			vehicleEntries = GteVehicleEntry.ShallowLoadFromDB(TestConnection);
			gateMovements = GteGateMovement.ShallowLoadFromDB(TestConnection);
			gates = GteGateOld_V01.ShallowLoadFromDB(TestConnection);
			lanes = GteLane.ShallowLoadFromDB(TestConnection);

			AssertEquals("Expected two WhsWarehouse rows", 2, warehouses.Length);
			AssertEquals("Expected GteBooking to be empty", 0, bookings.Length);
			AssertEquals("Expected GteGateMovementBooking to be empty", 0, gateMovementBookings.Length);
			AssertEquals("Expected GteVehicleDriverBooking to be empty", 0, vehicleDriverBookings.Length);
			AssertEquals("Expected GteVehicleMovementBooking to be empty", 0, vehicleMovementBookings.Length);
			AssertEquals("Expected GteVehicleMovement to be empty", 0, vehicleMovements.Length);
			AssertEquals("Expected GteVehicleEntry to be empty", 0, vehicleEntries.Length);
			AssertEquals("Expected GteGateMovement to be empty", 0, gateMovements.Length);
			AssertEquals("Expected one GteGate row", 1, gates.Length);
			AssertEquals("Expected one GteLane row", 1, lanes.Length);
		}

		public void TestWhenGteBookingDoesNotExist_ThenTransformDoesNotRun()
		{
			DBTransformationTestHelper.DropConstraintIfExists(GteBookingSchema.Constants.TableName, "GteBooking_GBK_WW_Facility_FK2_WhsWarehouse_RRR_120N");
			DBTransformationTestHelper.DropConstraintIfExists(GteBookingSchema.Constants.TableName, "GteBooking_GBK_OH_TransportCompany_FK2_OrgHeader_RRR_120N");
			DBTransformationTestHelper.DropConstraintIfExists(GteGateMovementBookingSchema.Constants.TableName, "GteGateMovementBooking_GBM_GBK_Booking_FK2_GteBooking_RRR_120N");
			DBTransformationTestHelper.DropConstraintIfExists(GteVehicleDriverBookingSchema.Constants.TableName, "GteVehicleDriverBooking_GBD_GBK_Booking_FK2_GteBooking_RRR_120N");
			DBTransformationTestHelper.DropConstraintIfExists(GteVehicleMovementBookingSchema.Constants.TableName, "GteVehicleMovementBooking_GBV_GBK_Booking_FK2_GteBooking_RRR_120N");
			DBTransformationTestHelper.DropConstraintIfExists(GteVehicleMovementSchema.Constants.TableName, "GteVehicleMovement_GVM_GBK_MainBooking_FK2_GteBooking_RRR_120N");
			DBTransformationTestHelper.DropIndexIfExists(GteBookingSchema.Constants.TableName, "NR_UX__GBK_ReferenceNumber");
			DBTransformationTestHelper.DropIndexIfExists(GteBookingSchema.Constants.TableName, "NR_UX__GBK_SourceReferenceNumber");
			DBTransformationTestHelper.DropIndexIfExists(GteBookingSchema.Constants.TableName, "NR_RX__GBK_SystemCreateTimeUtc");
			DBTransformationTestHelper.DropIndexIfExists(GteBookingSchema.Constants.TableName, "NR_RX__GBK_SystemLastEditTimeUtc");
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, GteGateSchema.Constants.TableName, "GTE_OA_Address", "UNIQUEIDENTIFIER");

			DBTransformationTestHelper.DropTableIfExists(GteBookingSchema.Constants.TableName);

			var transform = GetNewTestTransformationInstance();

			AssertEquals("Pre-condition", false, DbObjectCreator.TableExists(TestConnection, GteBookingSchema.Constants.TableName));
			AssertEquals("Transform.IsRequired should be false if GteBooking does not exist", false, transform.IsRequired);
		}

		public void TestWhenFacilityColumnDoesExist_ThenTransformDoesNotRun()
		{
			var transform = GetNewTestTransformationInstance();

			AssertEquals("Pre-condition", true, DbObjectCreator.TableExists(TestConnection, GteBookingSchema.Constants.TableName));
			AssertEquals("Pre-condition", true, DbObjectCreator.ColumnExists(TestConnection, GteBookingSchema.Constants.TableName, GteBookingSchema.Constants.GBK_WW_Facility));
			AssertEquals("Transform.IsRequired should be false if the GBK_WW_Facility exists, because it is already populated", false, transform.IsRequired);
		}

		public void TestWhenGteBookingDoesExist_AndFacilityColumnDoesNotExist_ThenTransformDoesRun()
		{
			DBTransformationTestHelper.DropConstraintIfExists(GteBookingSchema.Constants.TableName, "GteBooking_GBK_WW_Facility_FK2_WhsWarehouse_RRR_120N");
			DBTransformationTestHelper.DropIndexIfExists(GteBookingSchema.Constants.TableName, "FK_RX__GBK_WW_Facility");
			DBTransformationTestHelper.DropColumnIfExists(GteBookingSchema.Constants.TableName, GteBookingSchema.Constants.GBK_WW_Facility);

			var transform = GetNewTestTransformationInstance();

			AssertEquals("Pre-condition", true, DbObjectCreator.TableExists(TestConnection, GteBookingSchema.Constants.TableName));
			AssertEquals("Pre-condition", false, DbObjectCreator.ColumnExists(TestConnection, GteBookingSchema.Constants.TableName, GteBookingSchema.Constants.GBK_WW_Facility));
			AssertEquals("Transform.IsRequired should be true if GBK_WW_Facility does not exist", true, transform.IsRequired);
		}

		public void TestWhenThereAreMultipleWarehouse_ThenPopulateBookingWithCYDWarehouseOnly()
		{
			var sql = new StringBuilder();

			DBTransformationTestHelper.DropConstraintIfExists(GteBookingSchema.Constants.TableName, "GteBooking_GBK_WW_Facility_FK2_WhsWarehouse_RRR_120N");
			DBTransformationTestHelper.DropIndexIfExists(GteBookingSchema.Constants.TableName, "FK_RX__GBK_WW_Facility");
			DBTransformationTestHelper.DropColumnIfExists(GteBookingSchema.Constants.TableName, GteBookingSchema.Constants.GBK_WW_Facility);
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, GteGateSchema.Constants.TableName, "GTE_OA_Address", "UNIQUEIDENTIFIER");

			var transportCompany = new OrgHeader("TRN").AppendInsertAndReturnObject(sql);
			var facilityCompany = new OrgHeader("FAC").AppendInsertAndReturnObject(sql);
			var facilityAddress1 = new OrgAddress(facilityCompany, "OA1", "One Avenue").AppendInsertAndReturnObject(sql);
			var facilityAddress2 = new OrgAddress(facilityCompany, "OA2", "Two Avenue").AppendInsertAndReturnObject(sql);

			var transitFacility = new WhsWarehouse("TRW", WarehouseTypes.Transit, address: facilityAddress1.PK).WithDockDoor(sql);
			var productFacility = new WhsWarehouse("PRW", WarehouseTypes.Product, address: facilityAddress1.PK).WithDockDoor(sql);
			var containerYardFacility = new WhsWarehouse("CYD", WarehouseTypes.ContainerYard, address: facilityAddress1.PK).AppendInsertAndReturnObject(sql);
			var freeTradeZoneFacility = new WhsWarehouse("FTZ", WarehouseTypes.FreeTradeZone, address: facilityAddress1.PK).WithDockDoor(sql);

			var booking = new GteBookingOld_V01(transportCompany, "GBK001").AppendInsertAndReturnObject(sql);
			var gateMovementBooking = new GteGateMovementBooking(booking) { GBM_MovementBookingNumber = "GBM01" }.AppendInsertAndReturnObject(sql);
			var vehicleDriverBooking = new GteVehicleDriverBooking(booking, "Samuel", "LIC123").AppendInsertAndReturnObject(sql);
			var vehicleMovementBooking = new GteVehicleMovementBooking(booking) { GBV_VehicleRegistration = "TRUCK001" }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var transformation = new PopulateGateBookingFacility();
			transformation.Run();

			var updatedBooking = GteBooking.ShallowLoadFromDB(TestConnection, booking.PK);

			AssertEquals("Expected booking.GBK_WW_Facility to be linked to containerYardFacility.PK", containerYardFacility.PK, updatedBooking.GBK_WW_Facility.FK);
		}

		public void TestWhenCYDWarehouseIsInactive_ThenDeleteAllRowsInGateManagementSchema_ExceptGTEAndGLNTable()
		{
			var sql = new StringBuilder();

			DBTransformationTestHelper.DropConstraintIfExists(GteGateSchema.Constants.TableName, "Constraint_GTE_IsActive");
			DBTransformationTestHelper.DropConstraintIfExists(GteBookingSchema.Constants.TableName, "GteBooking_GBK_WW_Facility_FK2_WhsWarehouse_RRR_120N");
			DBTransformationTestHelper.DropIndexIfExists(GteBookingSchema.Constants.TableName, "FK_RX__GBK_WW_Facility");
			DBTransformationTestHelper.DropColumnIfExists(GteBookingSchema.Constants.TableName, GteBookingSchema.Constants.GBK_WW_Facility);
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, GteGateSchema.Constants.TableName, "GTE_OA_Address", "UNIQUEIDENTIFIER");

			var transportCompany = new OrgHeader("TRN").AppendInsertAndReturnObject(sql);
			var facilityCompany = new OrgHeader("FAC").AppendInsertAndReturnObject(sql);
			var facilityAddress1 = new OrgAddress(facilityCompany, "OA1", "One Avenue").AppendInsertAndReturnObject(sql);
			var facilityAddress2 = new OrgAddress(facilityCompany, "OA2", "Two Avenue").AppendInsertAndReturnObject(sql);

			var transitFacility = new WhsWarehouse("TRW", WarehouseTypes.Transit, address: facilityAddress1.PK).WithDockDoor(sql);
			var productFacility = new WhsWarehouse("PRW", WarehouseTypes.Product, address: facilityAddress1.PK).WithDockDoor(sql);
			var freeTradeZoneFacility = new WhsWarehouse("FTZ", WarehouseTypes.FreeTradeZone, address: facilityAddress1.PK).WithDockDoor(sql);
			var containerYardFacility = new WhsWarehouse("CYD", WarehouseTypes.ContainerYard, address: facilityAddress1.PK);
			containerYardFacility.WW_IsActive = false;
			containerYardFacility.AppendInsertAndReturnObject(sql);

			var refContainer = new RefContainer("BOX", "AIR").AppendInsertAndReturnObject(sql);
			var gate = new GteGateOld_V01(facilityAddress1, "G01", "This is a gate description").AppendInsertAndReturnObject(sql);
			var lane = new GteLane(gate, "L01", GteGateDirections.Both).AppendInsertAndReturnObject(sql);

			var booking = new GteBookingOld_V01(transportCompany, "GBK001").AppendInsertAndReturnObject(sql);
			var gateMovementBooking = new GteGateMovementBooking(booking) { GBM_MovementBookingNumber = "GBM01" }.AppendInsertAndReturnObject(sql);
			var vehicleDriverBooking = new GteVehicleDriverBooking(booking, "Samuel", "LIC123").AppendInsertAndReturnObject(sql);
			var vehicleMovementBooking = new GteVehicleMovementBooking(booking) { GBV_VehicleRegistration = "TRUCK001" }.AppendInsertAndReturnObject(sql);

			var vehicleMovement = new GteVehicleMovement(booking, refContainer, "TRUCK001").AppendInsertAndReturnObject(sql);
			var vehicleEntry = new GteVehicleEntry(vehicleMovement, lane, "GVE001", DateTimeOffset.UtcNow.AddSeconds(0), true, "Nathan", "12345").AppendInsertAndReturnObject(sql);
			var gateMovement = new GteGateMovement(gateMovementBooking, vehicleMovement).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var warehouses = WhsWarehouse.ShallowLoadFromDB(TestConnection);
			var bookings = GteBookingOld_V01.ShallowLoadFromDB(TestConnection);
			var gateMovementBookings = GteGateMovementBooking.ShallowLoadFromDB(TestConnection);
			var vehicleDriverBookings = GteVehicleDriverBooking.ShallowLoadFromDB(TestConnection);
			var vehicleMovementBookings = GteVehicleMovementBooking.ShallowLoadFromDB(TestConnection);
			var vehicleMovements = GteVehicleMovement.ShallowLoadFromDB(TestConnection);
			var vehicleEntries = GteVehicleEntry.ShallowLoadFromDB(TestConnection);
			var gateMovements = GteGateMovement.ShallowLoadFromDB(TestConnection);
			var gates = GteGateOld_V01.ShallowLoadFromDB(TestConnection);
			var lanes = GteLane.ShallowLoadFromDB(TestConnection);

			AssertEquals("Pre-condition: Expected four WhsWarehouse rows", 4, warehouses.Length);
			AssertEquals("Pre-condition: Expected one GteBooking rows", 1, bookings.Length);
			AssertEquals("Pre-condition: Expected one GteGateMovementBooking rows", 1, gateMovementBookings.Length);
			AssertEquals("Pre-condition: Expected one GteVehicleDriverBooking rows", 1, vehicleDriverBookings.Length);
			AssertEquals("Pre-condition: Expected one GteVehicleMovementBooking rows", 1, vehicleMovementBookings.Length);
			AssertEquals("Pre-condition: Expected one GteVehicleMovement rows", 1, vehicleMovements.Length);
			AssertEquals("Pre-condition: Expected one GteVehicleEntry rows", 1, vehicleEntries.Length);
			AssertEquals("Pre-condition: Expected one GteGateMovement rows", 1, gateMovements.Length);
			AssertEquals("Pre-condition: Expected one GteGate row", 1, gates.Length);
			AssertEquals("Pre-condition: Expected one GteLane row", 1, lanes.Length);

			var transformation = new PopulateGateBookingFacility();
			transformation.Run();

			warehouses = WhsWarehouse.ShallowLoadFromDB(TestConnection);
			bookings = GteBookingOld_V01.ShallowLoadFromDB(TestConnection);
			gateMovementBookings = GteGateMovementBooking.ShallowLoadFromDB(TestConnection);
			vehicleDriverBookings = GteVehicleDriverBooking.ShallowLoadFromDB(TestConnection);
			vehicleMovementBookings = GteVehicleMovementBooking.ShallowLoadFromDB(TestConnection);
			vehicleMovements = GteVehicleMovement.ShallowLoadFromDB(TestConnection);
			vehicleEntries = GteVehicleEntry.ShallowLoadFromDB(TestConnection);
			gateMovements = GteGateMovement.ShallowLoadFromDB(TestConnection);
			gates = GteGateOld_V01.ShallowLoadFromDB(TestConnection);
			lanes = GteLane.ShallowLoadFromDB(TestConnection);

			AssertEquals("Expected four WhsWarehouse rows", 4, warehouses.Length);
			AssertEquals("Expected GteBooking to be empty", 0, bookings.Length);
			AssertEquals("Expected GteGateMovementBooking to be empty", 0, gateMovementBookings.Length);
			AssertEquals("Expected GteVehicleDriverBooking to be empty", 0, vehicleDriverBookings.Length);
			AssertEquals("Expected GteVehicleMovementBooking to be empty", 0, vehicleMovementBookings.Length);
			AssertEquals("Expected GteVehicleMovement to be empty", 0, vehicleMovements.Length);
			AssertEquals("Expected GteVehicleEntry to be empty", 0, vehicleEntries.Length);
			AssertEquals("Expected GteGateMovement to be empty", 0, gateMovements.Length);
			AssertEquals("Expected one GteGate row", 1, gates.Length);
			AssertEquals("Expected one GteLane row", 1, lanes.Length);
		}
	}
}

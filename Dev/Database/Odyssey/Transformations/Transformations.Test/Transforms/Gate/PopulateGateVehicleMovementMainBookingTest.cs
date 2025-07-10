using System;
using System.Collections.Generic;
using System.Linq;
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
	[TestedType(typeof(PopulateGateVehicleMovementMainBooking))]
	public class PopulateGateVehicleMovementMainBookingTest : DataTransformationTestCase
	{
		RefContainer vehicleType;
		OrgHeader organization;
		WhsWarehouse warehouse;
		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateGateVehicleMovementMainBooking();

		protected override void PrepareTestData()
		{
			base.PrepareTestData();
			DBTransformationTestHelper.DropConstraintIfExists(GteVehicleMovementSchema.Constants.TableName, "GteVehicleMovement_GVM_GBK_MainBooking_FK2_GteBooking_RRR_120N");
			DBTransformationTestHelper.DropIndexIfExists(GteVehicleMovementSchema.Constants.TableName, "FK_RX__GVM_GBK_MainBooking");
			DBTransformationTestHelper.DropColumnIfExists(GteVehicleMovementSchema.Constants.TableName, GteVehicleMovementSchema.Constants.GVM_GBK_MainBooking);
			var sql = new StringBuilder();
			PrepareReferenceDataSql(sql);

			var vehicleMovement1 = new GteVehicleMovementOld_V01(vehicleType, "REG001").AppendInsertAndReturnObject(sql);
			CreateMovementWithBooking(sql, vehicleMovement1, "REG", false, "GBK001", DateTime.Now);

			var vehicleMovement2 = new GteVehicleMovementOld_V01(vehicleType, "REG002").AppendInsertAndReturnObject(sql);
			CreateMovementWithBooking(sql, vehicleMovement2, "REG", false, "GBK002", null);

			var vehicleMovement3 = new GteVehicleMovementOld_V01(vehicleType, "REG003").AppendInsertAndReturnObject(sql);
			CreateMovementWithBooking(sql, vehicleMovement3, "ADH", false, "GBK003", DateTime.Now);

			var vehicleMovement4 = new GteVehicleMovementOld_V01(vehicleType, "REG004").AppendInsertAndReturnObject(sql);
			CreateMovementWithBooking(sql, vehicleMovement4, "ADH", false, "GBK004", null);

			var vehicleMovement5 = new GteVehicleMovementOld_V01(vehicleType, "REG005").AppendInsertAndReturnObject(sql);
			CreateMovementWithBooking(sql, vehicleMovement5, "REG", true, "GBK005", DateTime.Now);

			var vehicleMovement6 = new GteVehicleMovementOld_V01(vehicleType, "REG006").AppendInsertAndReturnObject(sql);
			CreateMovementWithBooking(sql, vehicleMovement6, "REG", true, "GBK006", null);

			var vehicleMovement7 = new GteVehicleMovementOld_V01(vehicleType, "REG007").AppendInsertAndReturnObject(sql);
			CreateMovementWithBooking(sql, vehicleMovement7, "ADH", true, "GBK007", DateTime.Now);

			var vehicleMovement8 = new GteVehicleMovementOld_V01(vehicleType, "REG008").AppendInsertAndReturnObject(sql);
			CreateMovementWithBooking(sql, vehicleMovement8, "ADH", true, "GBK008", null);

			var vehicleMovement9 = new GteVehicleMovementOld_V01(vehicleType, "REG009").AppendInsertAndReturnObject(sql);
			CreateMovementWithBooking(sql, vehicleMovement9, "REG", false, "GBK009", new DateTime(2024, 12, 1));
			CreateMovementWithBooking(sql, vehicleMovement9, "REG", false, "GBK010", new DateTime(2024, 12, 2));

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertTransformationResults()
		{
			base.AssertTransformationResults();
			var vehicleMovements = GteVehicleMovement.ShallowLoadFromDB(TestConnection);
			var gateBookings = GteBooking.ShallowLoadFromDB(TestConnection);
			foreach (var vehicleMovement in vehicleMovements)
			{
				var targetBookingNumber = "GBK" + vehicleMovement.GVM_VehicleRegistration.Substring(3);
				var targetBooking = gateBookings.First(x => x.GBK_ReferenceNumber == targetBookingNumber);
				AssertEquals(vehicleMovement.GVM_GBK_MainBooking.ToString(), targetBooking.PK.ToString());
			}
		}

		public void TestPopulateGateVehicleMovementMainBooking_GivenRankedMovementCasesExcludeTop0_SetsMainBookingCorrectly() => RankedGateMovementTestCase(0);
		public void TestPopulateGateVehicleMovementMainBooking_GivenRankedMovementCasesExcludeTop1_SetsMainBookingCorrectly() => RankedGateMovementTestCase(1);
		public void TestPopulateGateVehicleMovementMainBooking_GivenRankedMovementCasesExcludeTop2_SetsMainBookingCorrectly() => RankedGateMovementTestCase(2);
		public void TestPopulateGateVehicleMovementMainBooking_GivenRankedMovementCasesExcludeTop3_SetsMainBookingCorrectly() => RankedGateMovementTestCase(3);
		public void TestPopulateGateVehicleMovementMainBooking_GivenRankedMovementCasesExcludeTop4_SetsMainBookingCorrectly() => RankedGateMovementTestCase(4);
		public void TestPopulateGateVehicleMovementMainBooking_GivenRankedMovementCasesExcludeTop5_SetsMainBookingCorrectly() => RankedGateMovementTestCase(5);
		public void TestPopulateGateVehicleMovementMainBooking_GivenRankedMovementCasesExcludeTop6_SetsMainBookingCorrectly() => RankedGateMovementTestCase(6);
		public void TestPopulateGateVehicleMovementMainBooking_GivenRankedMovementCasesExcludeTop7_SetsMainBookingCorrectly() => RankedGateMovementTestCase(7);
		public void TestPopulateGateVehicleMovementMainBooking_GivenRankedMovementCasesExcludeTop8_SetsMainBookingCorrectly() => RankedGateMovementTestCase(8);
		public void TestPopulateGateVehicleMovementMainBooking_GivenRankedMovementCasesExcludeTop9_SetsMainBookingCorrectly() => RankedGateMovementTestCase(9);
		public void TestPopulateGateVehicleMovementMainBooking_GivenRankedMovementCasesExcludeTop10_SetsMainBookingCorrectly() => RankedGateMovementTestCase(10);
		public void TestPopulateGateVehicleMovementMainBooking_GivenRankedMovementCasesExcludeTop11_SetsMainBookingCorrectly() => RankedGateMovementTestCase(11);

		void RankedGateMovementTestCase(int excludeTopN)
		{
			var earlyDate = new DateTime(2024, 12, 15);
			var lateDate = new DateTime(2024, 12, 25);
			var testCaseMovements = new List<(bool isCancelled, bool isAdhoc, DateTime? slotStartTime, int rank)>
			{
				(false, false, earlyDate, 1),
				(false, false, lateDate, 2),
				(false, false, null, 3),
				(false, true, earlyDate, 4),
				(false, true, lateDate, 5),
				(false, true, null, 6),
				(true, false, earlyDate, 7),
				(true, false, lateDate, 8),
				(true, false, null, 9),
				(true, true, earlyDate, 10),
				(true, true, lateDate, 11),
				(true, true, null, 12),
			};

			DBTransformationTestHelper.DropConstraintIfExists(GteVehicleMovementSchema.Constants.TableName, "GteVehicleMovement_GVM_GBK_MainBooking_FK2_GteBooking_RRR_120N");
			DBTransformationTestHelper.DropIndexIfExists(GteVehicleMovementSchema.Constants.TableName, "FK_RX__GVM_GBK_MainBooking");
			DBTransformationTestHelper.DropColumnIfExists(GteVehicleMovementSchema.Constants.TableName, GteVehicleMovementSchema.Constants.GVM_GBK_MainBooking);
			var sql = new StringBuilder();
			PrepareReferenceDataSql(sql);

			var vehicleMovement = new GteVehicleMovementOld_V01(vehicleType, "REG001").AppendInsertAndReturnObject(sql);
			GteBooking targetBooking = null;
			var allBookings = new List<GteBooking>();
			foreach (var movementCase in testCaseMovements.Where(x => x.rank > excludeTopN))
			{
				var booking = CreateMovementWithBooking(
					sql,
					vehicleMovement,
					movementCase.isAdhoc ? "ADH" : "REG",
					movementCase.isCancelled,
					"GBK" + movementCase.rank.ToString(),
					movementCase.slotStartTime);

				allBookings.Add(booking);

				if (movementCase.rank == excludeTopN + 1)
				{
					targetBooking = booking;
				}
			}

			TestConnection.ExecuteNonQuery(sql.ToString());

			var transformation = new PopulateGateVehicleMovementMainBooking();
			transformation.Run();

			var vehicleMovementDB = GteVehicleMovement.ShallowLoadFromDB(TestConnection, vehicleMovement.PK);

			AssertEquals("Expect GVM_GBK_MainBooking to be equal to the booking of best fit", vehicleMovementDB.GVM_GBK_MainBooking.ToString(), targetBooking.PK.ToString());
		}

		public void TestPopulateMainBooking_WhenColumnAlreadyPopulated_DoesNotChangeMainBooking()
		{
			var sql = new StringBuilder();
			PrepareReferenceDataSql(sql);
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, GteVehicleMovementSchema.Constants.TableName, GteVehicleMovementSchema.Constants.GVM_GBK_MainBooking, "UNIQUEIDENTIFIER", null);
			AssertEquals("Pre-condition: GVM_GBK_MainBooking Exists", true, DbObjectCreator.ColumnExists(TestConnection, GteVehicleMovementSchema.Constants.TableName, GteVehicleMovementSchema.Constants.GVM_GBK_MainBooking));

			var gteBooking = new GteBooking(organization, warehouse, "GBK001").AppendInsertAndReturnObject(sql);
			var vehicleMovement = new GteVehicleMovement(gteBooking, vehicleType, "REG001").AppendInsertAndReturnObject(sql);
			CreateMovementWithBooking(sql, vehicleMovement, "REG", false, "GBK002", new DateTime(2000, 01, 01));

			TestConnection.ExecuteNonQuery(sql.ToString());
			var transformation = GetNewTestTransformationInstance();
			transformation.Run();

			AssertEquals("GVM_GBM_MainBooking should remain unchanged", gteBooking.PK.ToString(), vehicleMovement.GVM_GBK_MainBooking.ToString());
		}

		public void TestPopulateMainBooking_WhenCannotMatchToBooking_DeletesVehicleMovementAndEntry()
		{
			DBTransformationTestHelper.DropConstraintIfExists(GteGateSchema.Constants.TableName, "Constraint_GTE_IsActive");
			DBTransformationTestHelper.DropConstraintIfExists(GteVehicleMovementSchema.Constants.TableName, "GteVehicleMovement_GVM_GBK_MainBooking_FK2_GteBooking_RRR_120N");
			DBTransformationTestHelper.DropIndexIfExists(GteVehicleMovementSchema.Constants.TableName, "FK_RX__GVM_GBK_MainBooking");
			DBTransformationTestHelper.DropColumnIfExists(GteVehicleMovementSchema.Constants.TableName, GteVehicleMovementSchema.Constants.GVM_GBK_MainBooking);
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, GteGateSchema.Constants.TableName, "GTE_OA_Address", "UNIQUEIDENTIFIER");
			var sql = new StringBuilder();
			PrepareReferenceDataSql(sql);
			var address = new OrgAddress(organization, "ADR", "Address 1").AppendInsertAndReturnObject(sql);
			var gate = new GteGateOld_V01(address, "GTE", "Gate 1").AppendInsertAndReturnObject(sql);
			var lane = new GteLane(gate, "LNE", "BOTH").AppendInsertAndReturnObject(sql);

			var vehicleMovementToDelete = new GteVehicleMovementOld_V01(vehicleType, "REG001").AppendInsertAndReturnObject(sql);
			new GteVehicleEntry(vehicleMovementToDelete, lane, "GVE001", DateTime.Now, true, "John", "LIC123").AppendInsertAndReturnObject(sql);

			var vehicleMovementToKeep = new GteVehicleMovementOld_V01(vehicleType, "REG002").AppendInsertAndReturnObject(sql);
			var vehicleEntryToKeep = new GteVehicleEntry(vehicleMovementToKeep, lane, "GVE002", DateTime.Now, true, "Jane", "321CIL").AppendInsertAndReturnObject(sql);
			CreateMovementWithBooking(sql, vehicleMovementToKeep, "REG", false, "GBK001", null);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var transform = GetNewTestTransformationInstance();
			transform.Run();

			var vehicleMovementsDB = GteVehicleMovement.ShallowLoadFromDB(TestConnection);
			var vehicleEntriesDB = GteVehicleEntry.ShallowLoadFromDB(TestConnection);
			AssertEquals("Expect only one GteVehicleMovement remaining", 1, vehicleMovementsDB.Length);
			AssertEquals("Expect correct GteVehicleMovement to remain", vehicleMovementToKeep.PK.ToString(), vehicleMovementsDB[0].PK.ToString());
			AssertEquals("Expect only one GteVehicleEntry remaining", 1, vehicleEntriesDB.Length);
			AssertEquals("Expect correct GteVehicleEntry to remain", vehicleEntryToKeep.PK.ToString(), vehicleEntriesDB[0].PK.ToString());
		}

		void PrepareReferenceDataSql(StringBuilder sql)
		{
			organization = new OrgHeader("ORG").AppendInsertAndReturnObject(sql);
			warehouse = new WhsWarehouse("CYD", WarehouseTypes.ContainerYard).AppendInsertAndReturnObject(sql);
			vehicleType = RefContainer.ShallowLoadFromDB(TestConnection, x => x.RC_Code == "RTRK").FirstOrDefault() ?? new RefContainer("RTRK", "ROA").AppendInsertAndReturnObject(sql);
		}

		GteBooking CreateMovementWithBooking(StringBuilder sql, IGteVehicleMovementSQL vehicleMovement, string bookingType, bool isCancelled, string bookingReferenceNumber, DateTime? slotStartTime)
		{
			var booking = new GteBooking(organization, warehouse, bookingReferenceNumber);
			booking.GBK_BookingType = bookingType;
			booking.AppendInsertAndReturnObject(sql);
			var gateMovementBooking = new GteGateMovementBooking(booking);
			gateMovementBooking.GBM_SlotStartTime = slotStartTime;
			gateMovementBooking.GBM_MovementBookingNumber = bookingReferenceNumber;
			gateMovementBooking.AppendInsertAndReturnObject(sql);
			var gateMovement = new GteGateMovement(gateMovementBooking, vehicleMovement);
			if (isCancelled)
			{
				gateMovement.GGM_CancelledReason = "Cancelled for test";
				gateMovement.GGM_GS_NKCancelledBy = "~BP";
				gateMovement.GGM_CancelledTime = DateTime.Now;
			}
			gateMovement.AppendInsertAndReturnObject(sql);
			return booking;
		}
	}
}

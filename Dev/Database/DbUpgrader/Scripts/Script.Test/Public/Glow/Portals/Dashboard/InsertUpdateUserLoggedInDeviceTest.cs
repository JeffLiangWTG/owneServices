using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow.Portals.Dashboard;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Portals.Dashboard.Testing
{
	[TestedType(typeof(InsertUpdateUserLoggedInDevice))]
	sealed class InsertUpdateUserLoggedInDeviceTest : DbCreateScriptTest
	{
		public void TestInsertUpdateUserLoggedInDeviceSuccess()
		{
			var hardwareId = "9100";
			var userPK = Guid.NewGuid();
			var userTableCode = "GS";
			var userStaffCode = "TST";
			var devicePK = TestDataCreator.CreateGlbDeviceWithHardwareId(hardwareId);
			var refEquipmentPK = TestDataCreator.CreateRefEquipment();
			TestDataCreator.CreateAssignmentDivot(devicePK, refEquipmentPK);

			using (var command = Db.Connection.Command("InsertUpdateUserLoggedInDevice"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@hardwareId", SqlDbType.VarChar, hardwareId);
				command.AddParameter("@userPK", SqlDbType.UniqueIdentifier, userPK);
				command.AddParameter("@userTableCode", SqlDbType.VarChar, userTableCode);
				command.AddParameter("@userStaffCode", SqlDbType.VarChar, userStaffCode);
				command.ExecuteNonQuery();
			}

			Assert(GetCurrentOperatorOnEquipment(refEquipmentPK, userPK, userTableCode) == 1);
		}

		public void TestInsertUpdateUserLoggedInDevice_MultipleUsersLoggedIn()
		{
			var hardwareId = "91000000";
			var userPK1 = Guid.NewGuid();
			var userPK2 = Guid.NewGuid();
			var userTableCode = "GS";
			var userStaffCode = "TST";
			var devicePK = TestDataCreator.CreateGlbDeviceWithHardwareId(hardwareId);
			var refEquipmentPK = TestDataCreator.CreateRefEquipment();
			TestDataCreator.CreateAssignmentDivot(devicePK, refEquipmentPK);

			using (var command = Db.Connection.Command("InsertUpdateUserLoggedInDevice"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@hardwareId", SqlDbType.VarChar, hardwareId);
				command.AddParameter("@userPK", SqlDbType.UniqueIdentifier, userPK1);
				command.AddParameter("@userTableCode", SqlDbType.VarChar, userTableCode);
				command.AddParameter("@userStaffCode", SqlDbType.VarChar, userStaffCode);
				command.ExecuteNonQuery();
			}

			Assert(GetCurrentOperatorOnEquipment(refEquipmentPK, userPK1, userTableCode) == 1);

			using (var command = Db.Connection.Command("InsertUpdateUserLoggedInDevice"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@hardwareId", SqlDbType.VarChar, hardwareId);
				command.AddParameter("@userPK", SqlDbType.UniqueIdentifier, userPK2);
				command.AddParameter("@userTableCode", SqlDbType.VarChar, userTableCode);
				command.AddParameter("@userStaffCode", SqlDbType.VarChar, userStaffCode);
				command.ExecuteNonQuery();
			}

			Assert(GetCurrentOperatorOnEquipment(refEquipmentPK, userPK1, userTableCode) == 0);
			Assert(GetCurrentOperatorOnEquipment(refEquipmentPK, userPK2, userTableCode) == 1);
		}

		public void TestInsertUpdateUserLoggedInDevice_MultipleEquipmentsLoggedInBySameUser()
		{
			var hardwareId1 = "91000001";
			var hardwareId2 = "91000002";
			var userPK = Guid.NewGuid();
			var userTableCode = "GS";
			var userStaffCode = "TST";
			var devicePK1 = TestDataCreator.CreateGlbDeviceWithHardwareId(hardwareId1);
			var devicePK2 = TestDataCreator.CreateGlbDeviceWithHardwareId(hardwareId2);
			var refEquipmentPK1 = TestDataCreator.CreateRefEquipment();
			TestDataCreator.CreateAssignmentDivot(devicePK1, refEquipmentPK1);
			var refEquipmentPK2 = CreateAnotherRefEquipment();
			TestDataCreator.CreateAssignmentDivot(devicePK2, refEquipmentPK2);

			using (var command = Db.Connection.Command("InsertUpdateUserLoggedInDevice"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@hardwareId", SqlDbType.VarChar, hardwareId1);
				command.AddParameter("@userPK", SqlDbType.UniqueIdentifier, userPK);
				command.AddParameter("@userTableCode", SqlDbType.VarChar, userTableCode);
				command.AddParameter("@userStaffCode", SqlDbType.VarChar, userStaffCode);
				command.ExecuteNonQuery();
			}

			Assert(GetCurrentOperatorOnEquipment(refEquipmentPK1, userPK, userTableCode) == 1);

			using (var command = Db.Connection.Command("InsertUpdateUserLoggedInDevice"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@hardwareId", SqlDbType.VarChar, hardwareId2);
				command.AddParameter("@userPK", SqlDbType.UniqueIdentifier, userPK);
				command.AddParameter("@userTableCode", SqlDbType.VarChar, userTableCode);
				command.AddParameter("@userStaffCode", SqlDbType.VarChar, userStaffCode);
				command.ExecuteNonQuery();
			}

			Assert(GetCurrentOperatorOnEquipment(refEquipmentPK1, userPK, userTableCode) == 0);
			Assert(GetCurrentOperatorOnEquipment(refEquipmentPK2, userPK, userTableCode) == 1);
		}

		public void TestInsertUpdateUserLoggedInDeviceIfNoHardwareIdToDeviceRelation()
		{
			var hardwareId = "91000000";
			var userPK = Guid.NewGuid();
			var userTableCode = "GS";
			var userStaffCode = "TST";
			var devicePK = TestDataCreator.CreateGlbDevice();
			var refEquipmentPK = TestDataCreator.CreateRefEquipment();
			TestDataCreator.CreateAssignmentDivot(devicePK, refEquipmentPK);
			using (var command = Db.Connection.Command("InsertUpdateUserLoggedInDevice"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@hardwareId", SqlDbType.VarChar, hardwareId);
				command.AddParameter("@userPK", SqlDbType.UniqueIdentifier, userPK);
				command.AddParameter("@userTableCode", SqlDbType.VarChar, userTableCode);
				command.AddParameter("@userStaffCode", SqlDbType.VarChar, userStaffCode);

				AssertExceptionThrown("Hardware Id is not linked with an active device.", typeof(SqlException), () => command.ExecuteNonQuery());
			}
		}

		public void TestInsertUpdateUserLoggedInDeviceIfNoRefEquipmentToDeviceRelation()
		{
			var hardwareId = "91000000";
			var userPK = Guid.NewGuid();
			var userTableCode = "GS";
			var userStaffCode = "TST";
			var devicePK = TestDataCreator.CreateGlbDeviceWithHardwareId(hardwareId);
			var refEquipmentPK = TestDataCreator.CreateRefEquipment();
			TestDataCreator.CreateAssignmentDivot(devicePK, refEquipmentPK, DateTime.UtcNow);
			using (var command = Db.Connection.Command("InsertUpdateUserLoggedInDevice"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@hardwareId", SqlDbType.VarChar, hardwareId);
				command.AddParameter("@userPK", SqlDbType.UniqueIdentifier, userPK);
				command.AddParameter("@userTableCode", SqlDbType.VarChar, userTableCode);
				command.AddParameter("@userStaffCode", SqlDbType.VarChar, userStaffCode);

				AssertExceptionThrown("RefEquipment not currently assigned to an active device", typeof(SqlException), () => command.ExecuteNonQuery());
			}
		}

		static int GetCurrentOperatorOnEquipment(Guid refEquipmentPK, Guid userPK, string userTableCode)
		{
			var query = string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM dbo.TelEdge where TE_EntityTableCodeTo = '{0}' AND TE_EntityIdTo = '{1}' AND TE_EntityTableCodeFrom = '{2}' AND TE_EntityIdFrom = '{3}' AND TE_RelationshipType = 'OPT' AND TE_EndTime IS NULL", userTableCode, userPK, "RQ", refEquipmentPK);
			using (var command = Db.Connection.Command(query))
			{
				return (int)command.ExecuteScalar();
			}
		}

		static Guid CreateAnotherRefEquipment()
		{
			var refEquipmentPK = Guid.NewGuid();
			var sql = @"
	INSERT INTO [dbo].[RefEquipment]
           ([RQ_PK],[RQ_Description], [RQ_ShortCode], [RQ_Registration])
     VALUES
           (@refEquipmentPK ,'Test Equipmment 2', 'TD-02', 'TDX02')";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@refEquipmentPK", SqlDbType.UniqueIdentifier, refEquipmentPK);
				command.ExecuteNonQuery();
			}
			return refEquipmentPK;
		}
	}
}


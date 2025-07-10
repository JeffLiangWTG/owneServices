using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.CFS;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.CFS.Testing
{
	[TestedType(typeof(TG_GateTransport_ValidateVehicleRegistration_Update))]
	class TG_GateTransport_ValidateVehicleRegistration_UpdateTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestNormalUpdate()
		{
			try
			{
				var company1PK = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
				var branch1PK = TestDataCreator.CreateBranch(company1PK, "BR1", "AUMEL");

				var company2PK = TestDataCreator.CreateCompany("TC2", "AU", "AUD");
				var branch2PK = TestDataCreator.CreateBranch(company1PK, "BR2", "AUALX");

				var wareHousePk1 = CreateNewWarehouse("000", branch1PK).PK;
				var wareHousePk2 = CreateNewWarehouse("001", branch2PK).PK;

				var transport1 = CreateNewGateTransport(branch1PK, "TESTA");
				CreateNewGateTransport(branch2PK, "TESTB");

				UpdateGateTransport(transport1.PK, branch2PK, "TESTA");
			}
			catch (Exception ex)
			{
				Fail("Unexpected SqlException " + ex.Message);
			}
		}

		public void TestUpdateGTT_WW_Facility()
		{
			try
			{
				var company1PK = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
				var branch1PK = TestDataCreator.CreateBranch(company1PK, "BR1", "AUMEL");

				var company2PK = TestDataCreator.CreateCompany("TC2", "AU", "AUD");
				var branch2PK = TestDataCreator.CreateBranch(company1PK, "BR2", "AUALX");

				var wareHousePk1 = CreateNewWarehouse("002", branch1PK).PK;
				var wareHousePk2 = CreateNewWarehouse("003", branch2PK).PK;

				var transport1 = CreateNewGateTransport(branch1PK, "TESTA");
				CreateNewGateTransport(branch2PK, "TESTA");

				UpdateGateTransport(transport1.PK, branch2PK, "TESTA");

				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals(50000, ex.Number);
				AssertEquals(@"Attempt to add two trucks with the same rego inside the same branch.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		public void TestUpdateGTT_VehicleRegistration()
		{
			try
			{
				var company1PK = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
				var branch1PK = TestDataCreator.CreateBranch(company1PK, "BR1", "AUMEL");
				var wareHousePk1 = CreateNewWarehouse("004", branch1PK).PK;

				var transport1 = CreateNewGateTransport(branch1PK, "TESTA");
				CreateNewGateTransport(branch1PK, "TESTB");

				UpdateGateTransport(transport1.PK, branch1PK, "TESTB");

				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals(50000, ex.Number);
				AssertEquals(@"Attempt to add two trucks with the same rego inside the same branch.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		void UpdateGateTransport(Guid pk, Guid branchPk, string vehicleRegistration)
		{
			var sql = string.Format(@"
UPDATE
	dbo.GateTransport
SET 
	GTT_GB_Branch = '{0}', GTT_VehicleRegistration = '{1}'
WHERE
	GTT_PK = '{2}'",
				branchPk, vehicleRegistration, pk);

			TestConnection.ExecuteNonQuery(sql);
		}

		ActiveRowWrapper CreateNewWarehouse(string warehouseCode, Guid branchPK)
		{
			var warehouse = new ActiveRowWrapper(WhsWarehouseSchema.Instance)
			{
				[WhsWarehouseSchema.WW_OA_WarehouseAddress] = new Guid("6507E0BB-A9FF-43CF-8C46-D6EB9016A767"),
				[WhsWarehouseSchema.WW_GB_RelatedCompanyBranch] = branchPK,
				[WhsWarehouseSchema.WW_IsVirtualWarehouse] = false,
				[WhsWarehouseSchema.WW_WarehouseCode] = warehouseCode,
				[WhsWarehouseSchema.WW_WarehouseType] = "TRW",
				[WhsWarehouseSchema.WW_WLT_DefaultLocationType] = new Guid("16C9FD62-730A-42ED-A20E-699606FFF360")
			};

			warehouse.Save();
			return warehouse;
		}

		ActiveRowWrapper CreateNewGateTransport(Guid branchPk, string vehicleRegistration)
		{
			var gateTransport = new ActiveRowWrapper(GateTransportSchema.Instance)
			{
				[GateTransportSchema.GTT_FacilityType] = "AAA",
				[GateTransportSchema.GTT_JobNumber] = Guid.NewGuid().ToString().Substring(0, 20),
				[GateTransportSchema.GTT_VehicleRegistration] = vehicleRegistration,
				[GateTransportSchema.GTT_GB_Branch] = branchPk,
				[GateTransportSchema.GTT_DriverName] = "BBB",
				[GateTransportSchema.GTT_DriverLicence] = "CCC",
				[GateTransportSchema.GTT_SystemCreateTimeUtc] = DateTime.UtcNow,
				[GateTransportSchema.GTT_SystemLastEditTimeUtc] = DateTime.UtcNow,
				[GateTransportSchema.GTT_SystemCreateUser] = "TST",
				[GateTransportSchema.GTT_SystemLastEditUser] = "TST",
			};

			gateTransport.Save();
			return gateTransport;
		}
	}
}


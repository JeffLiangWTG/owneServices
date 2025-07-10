using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.CFS;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.CFS.Testing
{
	[TestedType(typeof(TG_GateTransport_ValidateVehicleRegistration_Insert))]
	class TG_GateTransport_ValidateVehicleRegistration_InsertTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestNormalInsert()
		{
			try
			{
				var company = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
				var branch = TestDataCreator.CreateBranch(company, "ZZ", "AUSYD");

				CreateNewGateTransport(branch, "TESTA");
			}
			catch (Exception ex)
			{
				Fail("Unexpected SqlException " + ex.Message);
			}
		}

		public void TestInsertWithDuplicateData()
		{
			try
			{
				var company = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
				var branch = TestDataCreator.CreateBranch(company, "ZZ", "AUSYD");

				CreateNewGateTransport(branch, "TESTA");
				CreateNewGateTransport(branch, "TESTA");

				Fail("Should have SqlException");
			}
			catch (SqlException ex)
			{
				AssertEquals(50000, ex.Number);
				AssertEquals(@"Attempt to add two trucks with the same rego inside the same branch.
The transaction ended in the trigger. The batch has been aborted.", ex.Message);
			}
		}

		void CreateNewGateTransport(Guid branchPk, string vehicleRegistration)
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
		}
	}
}


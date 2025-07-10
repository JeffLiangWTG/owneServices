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
	[TestedType(typeof(PopulateGteGateBranch))]
	public class PopulateGteGateBranchTest : DataTransformationTestCase
	{
		Guid branchPK;

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateGteGateBranch();

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(GteGateSchema.Constants.TableName, "Constraint_GTE_IsActive");
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, GteGateSchema.Constants.TableName, "GTE_OA_Address", "UNIQUEIDENTIFIER");

			var sql = new StringBuilder();
			branchPK = new GlbBranch("BR1").AppendInsertAndReturnObject(sql).PK;
			var facilityCompany = new OrgHeader("FAC").AppendInsertAndReturnObject(sql);
			var facilityAddress = new OrgAddress(facilityCompany, "OA1", "One Avenue").AppendInsertAndReturnObject(sql);
			new GteGate(facilityAddress, null, "GC1", "Gate One").AppendInsertAndReturnObject(sql);

			var containerYard = new WhsWarehouse("CY1", WarehouseTypes.ContainerYard, branchPK: branchPK, address: facilityAddress.PK).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		protected override void AssertPreConditions()
		{
			var gates = GteGate.ShallowLoadFromDB(TestConnection);
			AssertEquals("Pre-Condition: Expected one GteGate row", 1, gates.Length);
		}

		protected override void AssertTransformationResults()
		{
			var gates = GteGate.ShallowLoadFromDB(TestConnection);

			AssertEquals("Expected GTE_GB_Branch to be linked to correct branch", branchPK, gates[0].GTE_GB_Branch.FK);
			AssertEquals("Expected GTE_IsActive to be true", true, gates[0].GTE_IsActive);
		}

		public void TestGivenGateAddressMatchesMultipleBranches_GTE_GB_BranchIsNull_GTE_IsActiveIsFalse()
		{
			DBTransformationTestHelper.DropConstraintIfExists(GteGateSchema.Constants.TableName, "Constraint_GTE_IsActive");
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, GteGateSchema.Constants.TableName, "GTE_OA_Address", "UNIQUEIDENTIFIER");

			var sql = new StringBuilder();
			var branch1 = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var branch2 = new GlbBranch("BR2").AppendInsertAndReturnObject(sql);
			var facilityCompany = new OrgHeader("FAC").AppendInsertAndReturnObject(sql);
			var facilityAddress = new OrgAddress(facilityCompany, "OA1", "One Avenue").AppendInsertAndReturnObject(sql);
			var gate = new GteGate(facilityAddress, null, "GC1", "Gate One").AppendInsertAndReturnObject(sql);

			var containerYard = new WhsWarehouse("CY1", WarehouseTypes.ContainerYard, branchPK: branch1.PK, address: facilityAddress.PK).AppendInsertAndReturnObject(sql);
			var transitWarehouse = new WhsWarehouse("TW2", WarehouseTypes.Transit, branchPK: branch2.PK, address: facilityAddress.PK).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var branches = GlbBranch.ShallowLoadFromDB(TestConnection);
			var warehouses = WhsWarehouse.ShallowLoadFromDB(TestConnection);
			var gates = GteGate.ShallowLoadFromDB(TestConnection);

			AssertEquals("Pre-condition: Expected one GteGate row", 1, gates.Length);

			var transformation = new PopulateGteGateBranch();
			transformation.Run();

			var updatedGate = GteGate.ShallowLoadFromDB(TestConnection, gate.PK);

			AssertEquals("Expected GTE_GB_Branch to be null", null, updatedGate.GTE_GB_Branch);
			AssertEquals("Expected GTE_IsActive to be false", false, updatedGate.GTE_IsActive);
		}

		public void TestGivenGateAddressMatchesNoBranches_GTE_GB_BranchIsNull_GTE_IsActiveIsFalse()
		{
			DBTransformationTestHelper.DropConstraintIfExists(GteGateSchema.Constants.TableName, "Constraint_GTE_IsActive");
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, GteGateSchema.Constants.TableName, "GTE_OA_Address", "UNIQUEIDENTIFIER");

			var sql = new StringBuilder();
			var facilityCompany = new OrgHeader("FAC").AppendInsertAndReturnObject(sql);
			var facilityAddress = new OrgAddress(facilityCompany, "OA1", "One Avenue").AppendInsertAndReturnObject(sql);
			var gate = new GteGate(facilityAddress, null, "GC1", "Gate One").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var branches = GlbBranch.ShallowLoadFromDB(TestConnection);
			var warehouses = WhsWarehouse.ShallowLoadFromDB(TestConnection);
			var gates = GteGate.ShallowLoadFromDB(TestConnection);

			AssertEquals("Pre-condition: Expected one GteGate row", 1, gates.Length);

			var transformation = new PopulateGteGateBranch();
			transformation.Run();

			var updatedGate = GteGate.ShallowLoadFromDB(TestConnection, gate.PK);

			AssertEquals("Expected GTE_GB_Branch to be null", null, updatedGate.GTE_GB_Branch);
			AssertEquals("Expected GTE_IsActive to be false", false, updatedGate.GTE_IsActive);
		}

		public void TestGivenNonUniqueBranchCode_GTEIsActiveIsFalse()
		{
			DBTransformationTestHelper.DropConstraintIfExists(GteGateSchema.Constants.TableName, "Constraint_GTE_IsActive");
			DBTransformationTestHelper.DropIndexIfExists(GteGateSchema.Constants.TableName, "NR_UX__GTE_GB_Branch_GTE_Code");
			DbObjectCreator.CreateColumnIfNotExists(TestConnection, GteGateSchema.Constants.TableName, "GTE_OA_Address", "UNIQUEIDENTIFIER");

			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var facilityCompany = new OrgHeader("FAC").AppendInsertAndReturnObject(sql);
			var facilityAddress1 = new OrgAddress(facilityCompany, "OA1", "One Avenue").AppendInsertAndReturnObject(sql);
			var facilityAddress2 = new OrgAddress(facilityCompany, "OA2", "Two Avenue").AppendInsertAndReturnObject(sql);

			var containerYard = new WhsWarehouse("CY1", WarehouseTypes.ContainerYard, branchPK: branch.PK, address: facilityAddress1.PK).AppendInsertAndReturnObject(sql);
			var transitWarehouse = new WhsWarehouse("TW2", WarehouseTypes.Transit, branchPK: branch.PK, address: facilityAddress2.PK).AppendInsertAndReturnObject(sql);

			var gate1 = new GteGate(facilityAddress1, null, "GC1", "Container Yard Gate One").AppendInsertAndReturnObject(sql);
			var gate2 = new GteGate(facilityAddress2, null, "GC1", "Transit Warehouse Gate One").AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			var gates = GteGate.ShallowLoadFromDB(TestConnection);

			AssertEquals("Pre-condition: Expected two GteGate rows", 2, gates.Length);

			var transformation = new PopulateGteGateBranch();
			transformation.Run();

			var updatedGate1 = GteGate.ShallowLoadFromDB(TestConnection, gate1.PK);
			var updatedGate2 = GteGate.ShallowLoadFromDB(TestConnection, gate2.PK);

			CombineAssertions("Expected both gate's GTE_GB_Branch to be linked to the same branch", () =>
			{
				AssertEquals("updatedGate1", branch.PK, updatedGate1.GTE_GB_Branch.FK);
				AssertEquals("updatedGate2", branch.PK, updatedGate2.GTE_GB_Branch.FK);
			});

			CombineAssertions("Expected both gate's to be inactive", () =>
			{
				AssertEquals("updatedGate1", false, updatedGate1.GTE_IsActive);
				AssertEquals("updatedGate2", false, updatedGate2.GTE_IsActive);
			});
		}
	}
}

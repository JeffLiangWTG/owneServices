using System;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.ProductWarehouse
{
	[TestedType(typeof(ConstraintWB_ParentTableCode))]
	public class ConstraintWB_ParentTableCodeTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new ConstraintWB_ParentTableCode();

		protected override void PrepareTestData()
		{
			var sql = new SqlQueryBuilder();

			var client = new OrgHeader("Client1").AppendInsertAndReturnObject(sql);
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(sql);
			var product = new OrgSupplierPart("PR1").AppendInsertAndReturnObject(sql);

			var order = new WhsDocket(client.PK, whs.PK, "INW", "CUS", "ENT", "D1").AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order, product.PK, 1) { WE_StockOnHand = 1, WE_BondedEntryKey = "PR1-01", WE_OriginalInventoryStatus = "PND", WE_CurrentInventoryStatus = "PND" }.AppendInsertAndReturnObject(sql);
			var orderLine2 = new WhsDocketLine(order, product.PK, 2) { WE_StockOnHand = 2, WE_BondedEntryKey = "PR1-02", WE_OriginalInventoryStatus = "PND", WE_CurrentInventoryStatus = "PND" }.AppendInsertAndReturnObject(sql);
			var orderLine3 = new WhsDocketLine(order, product.PK, 3) { WE_StockOnHand = 3, WE_BondedEntryKey = "PR1-02", WE_OriginalInventoryStatus = "PND", WE_CurrentInventoryStatus = "PND" }.AppendInsertAndReturnObject(sql);
			new WhsBondedWarehouseAttribute(orderLine1.PK, "WE") { WB_CustomsQty = 1 }.AppendInsertAndReturnObject(sql);
			new WhsBondedWarehouseAttribute(orderLine2.PK, "VV") { WB_CustomsQty = 2 }.AppendInsertAndReturnObject(sql);
			new WhsBondedWarehouseAttribute(orderLine3.PK, "") { WB_CustomsQty = 3 }.AppendInsertAndReturnObject(sql);
			new WhsBondedWarehouseAttribute(Guid.NewGuid(), "WE") { WB_CustomsQty = 4 }.AppendInsertAndReturnObject(sql);

			new WhsBondedWarehouseAttribute(whs.PK, "WW") { WB_CustomsQty = 5 }.AppendInsertAndReturnObject(sql);
			new WhsBondedWarehouseAttribute(Guid.NewGuid(), "ZZZ") { WB_CustomsQty = 6 }.AppendInsertAndReturnObject(sql);
			new WhsBondedWarehouseAttribute(Guid.NewGuid(), "") { WB_CustomsQty = 7 }.AppendInsertAndReturnObject(sql);
			new WhsBondedWarehouseAttribute(null, "WE") { WB_CustomsQty = 8 }.AppendInsertAndReturnObject(sql);
			new WhsBondedWarehouseAttribute(null, "ZZZ") { WB_CustomsQty = 9 }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertPreConditions()
		{
			AssertEquals(9, WhsBondedWarehouseAttribute.CountInDB(TestConnection));
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(4, WhsBondedWarehouseAttribute.CountInDB(TestConnection));
			AssertEquals(1, WhsBondedWarehouseAttribute.CountInDB(TestConnection, bondedAttrib => bondedAttrib.WB_ParentTableCode == "WE" && bondedAttrib.WB_CustomsQty == 1));
			AssertEquals(1, WhsBondedWarehouseAttribute.CountInDB(TestConnection, bondedAttrib => bondedAttrib.WB_ParentTableCode == "WE" && bondedAttrib.WB_CustomsQty == 2));
			AssertEquals(1, WhsBondedWarehouseAttribute.CountInDB(TestConnection, bondedAttrib => bondedAttrib.WB_ParentTableCode == "WE" && bondedAttrib.WB_CustomsQty == 3));
			AssertEquals(1, WhsBondedWarehouseAttribute.CountInDB(TestConnection, bondedAttrib => bondedAttrib.WB_ParentTableCode == "WE" && bondedAttrib.WB_CustomsQty == 4));
		}

		public void TestGuidChunkingOperation()
		{
			PrepareTestData();
			AssertEquals("Precondition", 9, WhsBondedWarehouseAttribute.CountInDB(TestConnection));

			var batchSize = 1;
			var transformation = new ConstraintWB_ParentTableCode(batchSize);
			transformation.Run();

			AssertTransformationResults();
		}

		public void TestIsOnlinePostUpgrade()
		{
			PrepareTestData();
			AssertEquals("Precondition", 9, WhsBondedWarehouseAttribute.CountInDB(TestConnection));

			var transformation = GetNewTestTransformationInstance();

			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("No change", 9, WhsBondedWarehouseAttribute.CountInDB(TestConnection));

			transformation.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertEquals("No change", 9, WhsBondedWarehouseAttribute.CountInDB(TestConnection));

			transformation.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertEquals("No change", 9, WhsBondedWarehouseAttribute.CountInDB(TestConnection));

			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			AssertEquals(4, WhsBondedWarehouseAttribute.CountInDB(TestConnection));
			AssertEquals(1, WhsBondedWarehouseAttribute.CountInDB(TestConnection, bondedAttrib => bondedAttrib.WB_ParentTableCode == "WE" && bondedAttrib.WB_CustomsQty == 1));
			AssertEquals(1, WhsBondedWarehouseAttribute.CountInDB(TestConnection, bondedAttrib => bondedAttrib.WB_ParentTableCode == "WE" && bondedAttrib.WB_CustomsQty == 2));
			AssertEquals(1, WhsBondedWarehouseAttribute.CountInDB(TestConnection, bondedAttrib => bondedAttrib.WB_ParentTableCode == "WE" && bondedAttrib.WB_CustomsQty == 3));
			AssertEquals(1, WhsBondedWarehouseAttribute.CountInDB(TestConnection, bondedAttrib => bondedAttrib.WB_ParentTableCode == "WE" && bondedAttrib.WB_CustomsQty == 4));
		}

		protected override void SetUp()
		{
			base.SetUp();
			Db.Connection.ExecuteNonQuery("ALTER TABLE WhsBondedWarehouseAttribute DROP CONSTRAINT IF EXISTS Constraint_WB_ParentTableCode_NoCheck");
		}
	}
}

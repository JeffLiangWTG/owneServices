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
	[TestedType(typeof(ConstraintKI_ParentTableCode))]
	sealed class ConstraintKI_ParentTableCodeTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new ConstraintKI_ParentTableCode();
		protected override void PrepareTestData()
		{
			var now = DateTime.Now;
			var sql = new SqlQueryBuilder();

			var client = new OrgHeader("Client1").AppendInsertAndReturnObject(sql);
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW", branch.PK).WithDockDoor(sql);
			var row = new WhsRow(whs, "Row3") { WR_Columns = 2 }.AppendInsertAndReturnObject(sql);
			var area = new WhsArea(whs.PK, "Area2").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);
			var product = new OrgSupplierPart("PR1").AppendInsertAndReturnObject(sql);

			// Repair or Keep
			var clusterKey = 1234;
			var jobComInvoiceHeader = new JobComInvoiceHeader(clusterKey, "AI") { JZ_GB = branch.PK }.AppendInsertAndReturnObject(sql);
			var jobComInvoiceLine1 = new JobComInvoiceLine(jobComInvoiceHeader, clusterKey).AppendInsertAndReturnObject(sql);
			var jobComInvoiceLine2 = new JobComInvoiceLine(jobComInvoiceHeader, clusterKey).AppendInsertAndReturnObject(sql);
			var cusPackingList = new CusPackingList(jobComInvoiceHeader.PK, clusterKey).AppendInsertAndReturnObject(sql);
			var cusPackableItem1 = new CusPackableItem(cusPackingList, jobComInvoiceLine1, clusterKey).AppendInsertAndReturnObject(sql);
			var cusPackableItem2 = new CusPackableItem(cusPackingList, jobComInvoiceLine2, clusterKey).AppendInsertAndReturnObject(sql);

			var packageJob1 = new PkgPackageJob(cusPackingList.PK, "PKG1", "CUL").AppendInsertAndReturnObject(sql);
			var package1 = new PkgPackage(packageJob1, "PLT", 0).AppendInsertAndReturnObject(sql);
			var package2 = new PkgPackage(packageJob1, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package3 = new PkgPackage(packageJob1, "PLT", 2).AppendInsertAndReturnObject(sql);

			new PkgPackageItemDivot(package1.PK, cusPackableItem1.PK, "CUI", 1).AppendInsertAndReturnObject(sql);
			new PkgPackageItemDivot(package2.PK, cusPackableItem2.PK, "ZZZ", 2).AppendInsertAndReturnObject(sql);
			new PkgPackageItemDivot(package3.PK, Guid.NewGuid(), "CUI", 3).AppendInsertAndReturnObject(sql);

			// Repair or Keep
			var receive = new WhsDocket(client.PK, whs.PK, "INW", "REC", "FIN", "REC0001") { WD_FinalisedDate = DateTime.Now, WD_GS_NKFinalizedBy = "A" }.AppendInsertAndReturnObject(sql);
			var receiveLine1 = new WhsDocketLine(receive, product.PK, 100, location.PK) { WE_StockOnHand = 100, WE_OriginalInventoryStatus = "AVL" }.AppendInsertAndReturnObject(sql);
			var receiveLine2 = new WhsDocketLine(receive, product.PK, 100, location.PK) { WE_StockOnHand = 100, WE_OriginalInventoryStatus = "AVL" }.AppendInsertAndReturnObject(sql);
			var pick = new WhsPick(whs, "P1", "NEW").AppendInsertAndReturnObject(sql);
			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "PIC", "ORD0001") { WD_WP = pick.PK }.AppendInsertAndReturnObject(sql);
			var orderLine1 = new WhsDocketLine(order1, product.PK, 4).AppendInsertAndReturnObject(sql);
			var orderLine2 = new WhsDocketLine(order1, product.PK, 5).AppendInsertAndReturnObject(sql);
			var pickLine1 = new WhsPickLine(receiveLine1, orderLine1, 4).AppendInsertAndReturnObject(sql);
			var pickLine2 = new WhsPickLine(receiveLine2, orderLine2, 5).AppendInsertAndReturnObject(sql);

			var packageJob2 = new PkgPackageJob(order1.PK, "PKG2", "WD").AppendInsertAndReturnObject(sql);
			var package4 = new PkgPackage(packageJob2, "PLT", 0).AppendInsertAndReturnObject(sql);
			var package5 = new PkgPackage(packageJob2, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package6 = new PkgPackage(packageJob2, "PLT", 2).AppendInsertAndReturnObject(sql);

			new PkgPackageItemDivot(package4.PK, pickLine1.PK, "WZ", 4).AppendInsertAndReturnObject(sql);
			new PkgPackageItemDivot(package5.PK, pickLine2.PK, "ZZZ", 5).AppendInsertAndReturnObject(sql);
			new PkgPackageItemDivot(package6.PK, Guid.NewGuid(), "WZ", 6).AppendInsertAndReturnObject(sql);

			// Delete
			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "NEW", "ORD0002").AppendInsertAndReturnObject(sql);
			var packageJob3 = new PkgPackageJob(order2.PK, "PKG3", "WD").AppendInsertAndReturnObject(sql);
			var package7 = new PkgPackage(packageJob3, "PLT", 0).AppendInsertAndReturnObject(sql);
			var package8 = new PkgPackage(packageJob3, "PLT", 1).AppendInsertAndReturnObject(sql);

			new PkgPackageItemDivot(package7.PK, whs.PK, "WW", 7).AppendInsertAndReturnObject(sql);
			new PkgPackageItemDivot(package8.PK, Guid.NewGuid(), "ZZZ", 8).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		protected override void AssertPreConditions()
		{
			AssertEquals(8, PkgPackageItemDivot.CountInDB(TestConnection));
		}

		protected override void AssertTransformationResults()
		{
			AssertEquals(6, PkgPackageItemDivot.CountInDB(TestConnection));
			AssertEquals(1, PkgPackageItemDivot.CountInDB(TestConnection, divot => divot.KI_ParentTableCode == "CUI" && divot.KI_PackedQty == 1));
			AssertEquals(1, PkgPackageItemDivot.CountInDB(TestConnection, divot => divot.KI_ParentTableCode == "CUI" && divot.KI_PackedQty == 2));
			AssertEquals(1, PkgPackageItemDivot.CountInDB(TestConnection, divot => divot.KI_ParentTableCode == "CUI" && divot.KI_PackedQty == 3));
			AssertEquals(1, PkgPackageItemDivot.CountInDB(TestConnection, divot => divot.KI_ParentTableCode == "WZ" && divot.KI_PackedQty == 4));
			AssertEquals(1, PkgPackageItemDivot.CountInDB(TestConnection, divot => divot.KI_ParentTableCode == "WZ" && divot.KI_PackedQty == 5));
			AssertEquals(1, PkgPackageItemDivot.CountInDB(TestConnection, divot => divot.KI_ParentTableCode == "WZ" && divot.KI_PackedQty == 6));
		}

		public void TestGuidChunkingOperation()
		{
			PrepareTestData();
			AssertEquals("Precondition", 4, PkgPackageItemDivot.CountInDB(TestConnection, divot => divot.KI_ParentTableCode != "CUI" && divot.KI_ParentTableCode != "WZ"));

			var batchSize = 1;
			var transformation = new ConstraintKI_ParentTableCode(batchSize);
			transformation.Run();

			AssertTransformationResults();
		}

		public void TestIsOnlinePostUpgrade()
		{
			PrepareTestData();
			AssertEquals("Precondition", 4, PkgPackageItemDivot.CountInDB(TestConnection, divot => divot.KI_ParentTableCode != "CUI" && divot.KI_ParentTableCode != "WZ"));

			var transformation = GetNewTestTransformationInstance();

			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("No change", 4, PkgPackageItemDivot.CountInDB(TestConnection, divot => divot.KI_ParentTableCode != "CUI" && divot.KI_ParentTableCode != "WZ"));

			transformation.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertEquals("No change", 4, PkgPackageItemDivot.CountInDB(TestConnection, divot => divot.KI_ParentTableCode != "CUI" && divot.KI_ParentTableCode != "WZ"));

			transformation.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertEquals("No change", 4, PkgPackageItemDivot.CountInDB(TestConnection, divot => divot.KI_ParentTableCode != "CUI" && divot.KI_ParentTableCode != "WZ"));

			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			AssertEquals(0, PkgPackageItemDivot.CountInDB(TestConnection, divot => divot.KI_ParentTableCode != "CUI" && divot.KI_ParentTableCode != "WZ"));
			AssertTransformationResults();
		}

		protected override void SetUp()
		{
			base.SetUp();
			Db.Connection.ExecuteNonQuery("ALTER TABLE PkgPackageItemDivot DROP CONSTRAINT IF EXISTS Constraint_KI_ParentTableCode_NoCheck");
		}
	}
}

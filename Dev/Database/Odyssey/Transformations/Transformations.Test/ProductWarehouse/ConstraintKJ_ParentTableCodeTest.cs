using System;
using System.Collections.Generic;
using System.Linq;
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
	[TestedType(typeof(ConstraintKJ_ParentTableCode))]
	sealed class ConstraintKJ_ParentTableCodeTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new ConstraintKJ_ParentTableCode();

		protected override void PrepareTestData()
		{
			var sql = new SqlQueryBuilder();

			var client = new OrgHeader("Client1").AppendInsertAndReturnObject(sql);
			(var productWarehouseBranch, var productWarehouse) = SetupProductWarehouse(sql);
			(var transitWarehouseBranch, var transitWarehouse, var transitLocation) = SetupTransitWarehouse(sql);
			var refContainer = new RefContainer("KNT", "SEA").AppendInsertAndReturnObject(sql);

			// Keep / Repair
			var jobComInvoiceHeader1 = new JobComInvoiceHeader(1234, "AI") { JZ_GB = productWarehouseBranch.PK }.AppendInsertAndReturnObject(sql);
			var jobComInvoiceHeader2 = new JobComInvoiceHeader(5678, "AI") { JZ_GB = productWarehouseBranch.PK }.AppendInsertAndReturnObject(sql);
			var cusPackingList1 = new CusPackingList(jobComInvoiceHeader1.PK, 1234).AppendInsertAndReturnObject(sql);
			var cusPackingList2 = new CusPackingList(jobComInvoiceHeader2.PK, 5678).AppendInsertAndReturnObject(sql);
			InsertPkgPackageJobWithChildren(cusPackingList1.PK, "CUL1", "CUL", refContainer, sql);
			InsertPkgPackageJobWithChildren(cusPackingList2.PK, "CUL2", "ZZZ", refContainer, sql);

			var jobShipment1 = new JobShipment("SHIP1").AppendInsertAndReturnObject(sql);
			var jobShipment2 = new JobShipment("SHIP2").AppendInsertAndReturnObject(sql);
			InsertPkgPackageJobWithChildren(jobShipment1.PK, "JS1", "JS", refContainer, sql);
			InsertPkgPackageJobWithChildren(jobShipment2.PK, "JS2", "ZZZ", refContainer, sql);

			var dtbBookingConsolidation1 = new DtbBookingConsolidation() { KB_JobID = "KB1" }.AppendInsertAndReturnObject(sql);
			var dtbBookingConsolidation2 = new DtbBookingConsolidation() { KB_JobID = "KB2" }.AppendInsertAndReturnObject(sql);
			InsertPkgPackageJobWithChildren(dtbBookingConsolidation1.PK, "KB1", "KB", refContainer, sql);
			InsertPkgPackageJobWithChildren(dtbBookingConsolidation2.PK, "KB2", "ZZZ", refContainer, sql);

			var dtbBooking1 = new DtbBooking(dtbBookingConsolidation1.PK) { KM_JobID = "KM1" }.AppendInsertAndReturnObject(sql);
			var dtbBooking2 = new DtbBooking(dtbBookingConsolidation2.PK) { KM_JobID = "KM2" }.AppendInsertAndReturnObject(sql);
			InsertPkgPackageJobWithChildren(dtbBooking1.PK, "KM1", "KM", refContainer, sql);
			InsertPkgPackageJobWithChildren(dtbBooking2.PK, "KM2", "ZZZ", refContainer, sql);

			var pkgHandlingUnit1 = new PkgHandlingUnit(productWarehouseBranch.PK).AppendInsertAndReturnObject(sql);
			var pkgHandlingUnit2 = new PkgHandlingUnit(productWarehouseBranch.PK).AppendInsertAndReturnObject(sql);
			InsertPkgPackageJobWithChildren(pkgHandlingUnit1.PK, "KPU1", "KPU", refContainer, sql);
			InsertPkgPackageJobWithChildren(pkgHandlingUnit2.PK, "KPU2", "ZZZ", refContainer, sql);

			var dtbConsignment1 = new DtbConsignment("LTC1", "LTL").AppendInsertAndReturnObject(sql);
			var dtbConsignment2 = new DtbConsignment("LTC2", "LTL").AppendInsertAndReturnObject(sql);
			InsertPkgPackageJobWithChildren(dtbConsignment1.PK, "LTC1", "LTC", refContainer, sql);
			InsertPkgPackageJobWithChildren(dtbConsignment2.PK, "LTC2", "ZZZ", refContainer, sql);

			var docket1 = new WhsDocket(client.PK, productWarehouse.PK, "INW", "REC", "ARV", "WD1").AppendInsertAndReturnObject(sql);
			var docket2 = new WhsDocket(client.PK, productWarehouse.PK, "INW", "REC", "ARV", "WD2").AppendInsertAndReturnObject(sql);
			InsertPkgPackageJobWithChildren(docket1.PK, "WD1", "WD", refContainer, sql);
			InsertPkgPackageJobWithChildren(docket2.PK, "WD2", "ZZZ", refContainer, sql);

			var dcn1 = new WhsItemDispatchConsignment(transitWarehouse, "DCN001", "DCN001", "STD").AppendInsertAndReturnObject(sql);
			var dcn2 = new WhsItemDispatchConsignment(transitWarehouse, "DCN002", "DCN002", "STD").AppendInsertAndReturnObject(sql);
			InsertPkgPackageJobWithChildren(dcn1.PK, "WDC1", "WDC", refContainer, sql);
			InsertPkgPackageJobWithChildren(dcn2.PK, "WDC2", "ZZZ", refContainer, sql);

			var dtu1 = new WhsItemDispatchTransportationUnit(transitWarehouse, "WTU001").AppendInsertAndReturnObject(sql);
			var dtu2 = new WhsItemDispatchTransportationUnit(transitWarehouse, "WTU002").AppendInsertAndReturnObject(sql);
			InsertPkgPackageJobWithChildren(dtu1.PK, "WDH1", "WDH", refContainer, sql);
			InsertPkgPackageJobWithChildren(dtu2.PK, "WDH2", "ZZZ", refContainer, sql);

			var rcn1 = new WhsItemReceiveConsignment(transitWarehouse, "RCN001", "RCN1", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var rcn2 = new WhsItemReceiveConsignment(transitWarehouse, "RCN002", "RCN2", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			InsertPkgPackageJobWithChildren(rcn1.PK, "WRC1", "WRC", refContainer, sql);
			InsertPkgPackageJobWithChildren(rcn2.PK, "WRC2", "ZZZ", refContainer, sql);

			var rtu1 = new WhsItemReceiveTransportationUnit(transitWarehouse, "RTU001", transitLocation, "RTU001").AppendInsertAndReturnObject(sql);
			var rtu2 = new WhsItemReceiveTransportationUnit(transitWarehouse, "RTU002", transitLocation, "RTU002").AppendInsertAndReturnObject(sql);
			InsertPkgPackageJobWithChildren(rtu1.PK, "WRH1", "WRH", refContainer, sql);
			InsertPkgPackageJobWithChildren(rtu2.PK, "WRH2", "ZZZ", refContainer, sql);

			// Delete
			InsertPkgPackageJobWithChildren(Guid.NewGuid(), "RANDOM1", "ZZZ", refContainer, sql);
			InsertPkgPackageJobWithChildren(productWarehouseBranch.PK, "RANDOM2", "WW", refContainer, sql);
			InsertPkgPackageJobWithChildren(Guid.NewGuid(), "RANDOM3", "FIT", refContainer, sql);
			InsertPkgPackageJobWithChildren(Guid.NewGuid(), "RANDOM4", "FRO", refContainer, sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals("Precondition", 26, PkgPackageJob.CountInDB(TestConnection));
			AssertEquals("Precondition", 26 * 3, PkgPackage.CountInDB(TestConnection));
			AssertEquals("Precondition", 26, PkgPackageContainer.CountInDB(TestConnection));
			AssertEquals("Precondition", 26, PkgPackageBookedDetail.CountInDB(TestConnection));
		}

		static void InsertPkgPackageJobWithChildren(Guid parentID, string jobID, string parentTableCode, RefContainer refContainer, SqlQueryBuilder sql)
		{
			var pkgPackageJob = new PkgPackageJob(parentID, jobID, parentTableCode).AppendInsertAndReturnObject(sql);

			var package1 = new PkgPackage(pkgPackageJob, "PLT", 0).AppendInsertAndReturnObject(sql);
			var package2 = new PkgPackage(pkgPackageJob, "PLT", 0) { KP_KP_ParentPackage = package1.PK }.AppendInsertAndReturnObject(sql);
			new PkgPackage(pkgPackageJob, "PLT", 0) { KP_KP_ParentPackage = package2.PK }.AppendInsertAndReturnObject(sql);

			new PkgPackageContainer(package1, refContainer).AppendInsertAndReturnObject(sql);
			new PkgPackageBookedDetail(package1).AppendInsertAndReturnObject(sql);
		}

		protected override void AssertTransformationResults()
		{
			var expectedParentTableCodes = new string[]
			{
				"CUL",
				"CUL",
				"JS",
				"JS",
				"KB",
				"KB",
				"KM",
				"KM",
				"KPU",
				"KPU",
				"LTC",
				"LTC",
				"WD",
				"WD",
				"WDC",
				"WDC",
				"WDH",
				"WDH",
				"WRC",
				"WRC",
				"WRH",
				"WRH",
			};

			var packageJobs = PkgPackageJob.ShallowLoadFromDB(TestConnection);

			AssertContainsExactElementsInAnyOrder(expectedParentTableCodes, packageJobs.Select(p => p.KJ_ParentTableCode));
			AssertEquals(22 * 3, PkgPackage.CountInDB(TestConnection));
			AssertEquals(22, PkgPackageContainer.CountInDB(TestConnection));
			AssertEquals(22, PkgPackageBookedDetail.CountInDB(TestConnection));
		}

		public void TestOrigonalBookedDetailFKsAreCleared()
		{
			var sql = new SqlQueryBuilder();

			// Delete
			var pkgPackageJob1 = new PkgPackageJob(Guid.NewGuid(), "Random", "ZZZ").AppendInsertAndReturnObject(sql);
			var package1 = new PkgPackage(pkgPackageJob1, "PLT", 0).AppendInsertAndReturnObject(sql);
			var packageBookedDetail1 = new PkgPackageBookedDetail(package1).AppendInsertAndReturnObject(sql);

			// Keep
			var transitWarehouseBranch = new GlbBranch("TBR").AppendInsertAndReturnObject(sql);
			var transitWarehouse = new WhsWarehouse("TWS", "TRW", transitWarehouseBranch.PK).WithDockDoor(sql);
			var rcn = new WhsItemReceiveConsignment(transitWarehouse, "RCN0001", "RCN1", "STD", "AUSYD").AppendInsertAndReturnObject(sql);

			var pkgPackageJob2 = new PkgPackageJob(rcn.PK, "Keep", "WRC").AppendInsertAndReturnObject(sql);
			var package2 = new PkgPackage(pkgPackageJob2, "PLT", 0).AppendInsertAndReturnObject(sql);
			var package3 = new PkgPackage(pkgPackageJob2, "PLT", 1).AppendInsertAndReturnObject(sql);
			var packageBookedDetail2 = new PkgPackageBookedDetail(package2, packageBookedDetail1).AppendInsertAndReturnObject(sql);
			var packageBookedDetail3 = new PkgPackageBookedDetail(package3, packageBookedDetail2).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals("Precondition", 3, PkgPackageBookedDetail.CountInDB(TestConnection));
			AssertEquals("Precondition", 1, PkgPackageBookedDetail.CountInDB(TestConnection, row => row.PK == packageBookedDetail2.PK && row.KPB_KPB_OriginalBookedDetail == packageBookedDetail1.PK));
			AssertEquals("Precondition", 1, PkgPackageBookedDetail.CountInDB(TestConnection, row => row.PK == packageBookedDetail3.PK && row.KPB_KPB_OriginalBookedDetail == packageBookedDetail2.PK));

			var transformation = GetNewTestTransformationInstance();
			transformation.Run();

			AssertEquals(2, PkgPackageBookedDetail.CountInDB(TestConnection));
			AssertEquals(1, PkgPackageBookedDetail.CountInDB(TestConnection, row => row.PK == packageBookedDetail2.PK && row.KPB_KPB_OriginalBookedDetail == null));
			AssertEquals(1, PkgPackageBookedDetail.CountInDB(TestConnection, row => row.PK == packageBookedDetail3.PK && row.KPB_KPB_OriginalBookedDetail == packageBookedDetail2.PK));
		}

		public void TestBulk()
		{
			var sql = new SqlQueryBuilder();

			var client = new OrgHeader("CLIENT1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "PRW").WithDockDoor(sql);
			var refContainer = new RefContainer("KNT", "SEA").AppendInsertAndReturnObject(sql);

			var recordsToTest = 2500;

			var dockets = new List<WhsDocket>();
			var pkgPackageJobs = new List<PkgPackageJob>();
			var pkgPackages = new List<PkgPackage>();
			var pkgPackageContainers = new List<PkgPackageContainer>();
			var pkgPackageBookedDetails = new List<PkgPackageBookedDetail>();

			for (var i = 0; i < recordsToTest; i++)
			{
				var jobID = $"J{i}";

				var docket = new WhsDocket(client.PK, whs.PK, "WOR", "ASS", "NEW", jobID);
				var pkgPackageJob = new PkgPackageJob(docket.PK, jobID, "YYY");
				var pkgPackage = new PkgPackage(pkgPackageJob, "PLT", 0);
				var pkgPackageContainer = new PkgPackageContainer(pkgPackage, refContainer);
				var pkgPackageBookedDetail = new PkgPackageBookedDetail(pkgPackage);

				dockets.Add(docket);
				pkgPackageJobs.Add(pkgPackageJob);
				pkgPackages.Add(pkgPackage);
				pkgPackageContainers.Add(pkgPackageContainer);
				pkgPackageBookedDetails.Add(pkgPackageBookedDetail);
			}

			sql.AppendLine(WhsDocket.GetBulkInsertStatement(dockets));
			sql.AppendLine(PkgPackageJob.GetBulkInsertStatement(pkgPackageJobs));
			sql.AppendLine(PkgPackage.GetBulkInsertStatement(pkgPackages));
			sql.AppendLine(PkgPackageContainer.GetBulkInsertStatement(pkgPackageContainers));
			sql.AppendLine(PkgPackageBookedDetail.GetBulkInsertStatement(pkgPackageBookedDetails));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			AssertEquals("Precondition", recordsToTest, PkgPackageJob.CountInDB(TestConnection, pkgPackageJob => pkgPackageJob.KJ_ParentTableCode == "YYY"));

			var transformation = GetNewTestTransformationInstance();
			transformation.Run();

			AssertEquals(recordsToTest, PkgPackageJob.CountInDB(TestConnection, pkgPackageJob => pkgPackageJob.KJ_ParentTableCode == "WD"));
		}

		public void TestIsOnlinePostUpgrade()
		{
			PrepareTestData();
			AssertEquals("Precondition", 12, PkgPackageJob.CountInDB(TestConnection, pick => pick.KJ_ParentTableCode == "ZZZ"));

			var transformation = GetNewTestTransformationInstance();

			transformation.Run(TransformationSection.OnlinePreUpgrade, CancellationToken.None);
			AssertEquals("No change", 12, PkgPackageJob.CountInDB(TestConnection, pick => pick.KJ_ParentTableCode == "ZZZ"));

			transformation.Run(TransformationSection.OfflinePreUpgrade, CancellationToken.None);
			AssertEquals("No change", 12, PkgPackageJob.CountInDB(TestConnection, pick => pick.KJ_ParentTableCode == "ZZZ"));

			transformation.Run(TransformationSection.OfflinePostUpgrade, CancellationToken.None);
			AssertEquals("No change", 12, PkgPackageJob.CountInDB(TestConnection, pick => pick.KJ_ParentTableCode == "ZZZ"));

			transformation.Run(TransformationSection.OnlinePostUpgrade, CancellationToken.None);
			AssertEquals(0, PkgPackageJob.CountInDB(TestConnection, pick => pick.KJ_ParentTableCode == "ZZZ"));
			AssertTransformationResults();
		}

		protected override void SetUp()
		{
			Db.Connection.ExecuteNonQuery("ALTER TABLE PkgPackageJob DROP CONSTRAINT IF EXISTS Constraint_KJ_ParentTableCode_NoCheck");
		}

		#region Implementation

		static (GlbBranch productWarehouseBranch, WhsWarehouse productWarehouse) SetupProductWarehouse(SqlQueryBuilder sql)
		{
			var productWarehouseBranch = new GlbBranch("PBR").AppendInsertAndReturnObject(sql);
			var productWarehouse = new WhsWarehouse("PWS", "PRW", productWarehouseBranch.PK).WithDockDoor(sql);
			return (productWarehouseBranch, productWarehouse);
		}

		static (GlbBranch transitWarehouseBranch, WhsWarehouse transitWarehouse, WhsLocation transitLocation) SetupTransitWarehouse(SqlQueryBuilder sql)
		{
			var transitWarehouseBranch = new GlbBranch("TBR").AppendInsertAndReturnObject(sql);
			var transitWarehouse = new WhsWarehouse("TWS", "TRW", transitWarehouseBranch.PK).WithDockDoor(sql);
			var row = new WhsRow(transitWarehouse, "R1").AppendInsertAndReturnObject(sql);
			var area = new WhsArea(transitWarehouse.PK, "A1").AppendInsertAndReturnObject(sql);
			var location = new WhsLocation(row.PK, area.PK, area.PK) { WL_Column = 1 }.AppendInsertAndReturnObject(sql);

			return (transitWarehouseBranch, transitWarehouse, location);
		}

		#endregion
	}
}

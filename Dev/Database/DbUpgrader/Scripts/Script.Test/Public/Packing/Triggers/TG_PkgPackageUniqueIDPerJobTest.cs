using System;
using System.Text;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Packing.Triggers;
using NUnit.Framework;
namespace Enterprise.Build.Database.Script.Public.Packing.Triggers.Testing
{
	[TestedType(typeof(TG_PkgPackageUniqueIDPerJob))]
	class TG_PkgPackageUniqueIDPerJobTest : DbCreateScriptTest
	{
		#region TestInsert_PkgPackage_ConflictWithPackage

		[UseSnapshotProtection]
		public void TestInsert_PkgPackage_ConflictWithPackage()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(sql);
			var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var staff = new GlbStaff("S1", "S1.Login");
			var job1 = new PkgPackageJob(order.PK, "PJ001", "WD").AppendInsertAndReturnObject(sql);

			var pkgID123a = new PkgPackageHeader("123", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);
			var pkgID123b = new PkgPackageHeader("123", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);

			var pkg123OnJob1 = new PkgPackage(job1, pkgID123a, "PLT", 1).AppendInsertAndReturnObject(sql);
			new PkgPackage(job1, pkgID123b, "PLT", 1).AppendInsertAndReturnObject(sql);
			AssertExceptionThrown(
				typeof(SqlException),
				expectedExceptionMessage,
				() => TestConnection.ExecuteNonQuery(sql.ToString()));
		}

		#endregion

		#region TestInsert_PkgPackage_ConflictWithPivot

		[UseSnapshotProtection]
		public void TestInsert_PkgPackage_ConflictWithPivot()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(sql);
			var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var staff = new GlbStaff("S1", "S1.Login");
			var job1 = new PkgPackageJob(order.PK, "PJ001", "WD").AppendInsertAndReturnObject(sql);

			var pkgID123a = new PkgPackageHeader("123", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);
			var pkgID123b = new PkgPackageHeader("123", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);

			var pivotID1Other = new PkgPackageJobPackageHeaderPivot(job1, pkgID123a).AppendInsertAndReturnObject(sql);

			new PkgPackage(job1, pkgID123b, "PLT", 1).AppendInsertAndReturnObject(sql);
			AssertExceptionThrown(
				typeof(SqlException),
				expectedExceptionMessage,
				() => TestConnection.ExecuteNonQuery(sql.ToString()));
		}

		#endregion

		#region TestInsert_PkgPackage_NoConflictWithPackage_UntilUpdate

		[UseSnapshotProtection]
		public void TestInsert_PkgPackage_NoConflictWithPackage_UntilUpdate()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(sql);
			var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var staff = new GlbStaff("S1", "S1.Login");
			var job1 = new PkgPackageJob(order.PK, "PJ001", "WD").AppendInsertAndReturnObject(sql);

			var pkgID123 = new PkgPackageHeader("123", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);
			var pkgID123b = new PkgPackageHeader("123", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);
			var pkgID456 = new PkgPackageHeader("456", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);

			var pkg123OnJob1 = new PkgPackage(job1, pkgID123, "PLT", 1).AppendInsertAndReturnObject(sql);
			var pkgPackage456Job1 = new PkgPackage(job1, pkgID456, "PLT", 1).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(sql.ToString()));
			AssertExceptionThrown(
				typeof(SqlException),
				expectedExceptionMessage,
				() => PkgPackage.UpdateWhere(pkgPackage456Job1.PK).Set(c => c.KP_KPH_PackageHeader, pkgID123b).Post(TestConnection),
				true);
		}

		#endregion

		#region TestInsert_PkgPackage_NoConflictWithPivot_UntilUpdate
		[UseSnapshotProtection]
		public void TestInsert_PkgPackage_NoConflictWithPivot_UntilUpdate()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(sql);
			var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var staff = new GlbStaff("S1", "S1.Login");
			var job1 = new PkgPackageJob(order.PK, "PJ001", "WD").AppendInsertAndReturnObject(sql);

			var pkgID123 = new PkgPackageHeader("123", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);
			var pkgID123b = new PkgPackageHeader("123", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);
			var pkgID456 = new PkgPackageHeader("456", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);
			var pivotID1Other = new PkgPackageJobPackageHeaderPivot(job1, pkgID123).AppendInsertAndReturnObject(sql);

			var pkgPackage456Job1 = new PkgPackage(job1, pkgID456, "PLT", 1).AppendInsertAndReturnObject(sql);
			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(sql.ToString()));
			AssertExceptionThrown(
				typeof(SqlException),
				expectedExceptionMessage,
				() => PkgPackage.UpdateWhere(pkgPackage456Job1.PK).Set(c => c.KP_KPH_PackageHeader, pkgID123b).Post(TestConnection),
				true);
		}

		#endregion

		#region TestInsert_PkgPackage_NoConflictWithOtherJobPackage

		[UseSnapshotProtection]
		public void TestInsert_PkgPackage_NoConflictWithOtherJobPackage()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(sql);
			var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O2").AppendInsertAndReturnObject(sql);
			var staff = new GlbStaff("S1", "S1.Login");
			var job1 = new PkgPackageJob(order.PK, "PJ001", "WD").AppendInsertAndReturnObject(sql);
			var job2 = new PkgPackageJob(order2.PK, "PJ002", "WD").AppendInsertAndReturnObject(sql);

			var pkgID123 = new PkgPackageHeader("123", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);
			var pkgID123_OtherJob = new PkgPackageHeader("123", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);

			var pkg123OnJob1 = new PkgPackage(job1, pkgID123, "PLT", 1).AppendInsertAndReturnObject(sql);
			var pkg123OtherOnJob2 = new PkgPackage(job2, pkgID123_OtherJob, "PLT", 1).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(sql.ToString()));
		}

		#endregion

		#region TestInsert_PkgPackage_NoConflictWithOtherJobPivot

		[UseSnapshotProtection]
		public void TestInsert_PkgPackage_NoConflictWithOtherJobPivot()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(sql);
			var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O2").AppendInsertAndReturnObject(sql);
			var staff = new GlbStaff("S1", "S1.Login");
			var job1 = new PkgPackageJob(order.PK, "PJ001", "WD").AppendInsertAndReturnObject(sql);
			var job2 = new PkgPackageJob(order2.PK, "PJ002", "WD").AppendInsertAndReturnObject(sql);

			var pkgID123 = new PkgPackageHeader("123", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);
			var pkgID123_OtherJob = new PkgPackageHeader("123", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);
			var pivotID1Other = new PkgPackageJobPackageHeaderPivot(job1, pkgID123).AppendInsertAndReturnObject(sql);

			new PkgPackage(job2, pkgID123_OtherJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(sql.ToString()));
		}

		#endregion

		#region Implementation

		const string expectedExceptionMessage = @"Package ID needs to be unique per Job.
The transaction ended in the trigger. The batch has been aborted.";

		#endregion
	}
}


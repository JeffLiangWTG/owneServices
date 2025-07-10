using System;
using System.Text;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Packing.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Packing.Triggers.Testing
{
	[TestedType(typeof(TG_PkgPackageHeaderUniqueIDPerJob))]
	class TG_PkgPackageHeaderUniqueIDPerJobTest : DbCreateScriptTest
	{
		#region TestUpdate_PkgPackageHeaderID_ConflictWithPackage

		[UseSnapshotProtection]
		public void TestUpdate_PkgPackageHeaderID_ConflictWithPackage()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(sql);
			var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var staff = new GlbStaff("S1", "S1.Login");

			var job1 = new PkgPackageJob(order.PK, "PJ001", "WD").AppendInsertAndReturnObject(sql);
			var pkgID123 = new PkgPackageHeader("123", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);
			var pkgID456 = new PkgPackageHeader("456", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);

			var pkg123OnJob1 = new PkgPackage(job1, pkgID123, "PLT", 1).AppendInsertAndReturnObject(sql);
			var pkg456OnJob1 = new PkgPackage(job1, pkgID456, "PLT", 1).AppendInsertAndReturnObject(sql);

			sql.Append(PkgPackageHeader.UpdateWhere(pkgID456.PK).Set(c => c.KPH_PackageID, pkgID123.KPH_PackageID).AsSQL());
			AssertExceptionThrown(
				typeof(SqlException),
				expectedExceptionMessage,
				() => TestConnection.ExecuteNonQuery(sql.ToString()));
		}

		#endregion

		#region TestUpdate_PkgPackageHeaderID_ConflictWithPivot

		[UseSnapshotProtection]
		public void TestUpdate_PkgPackageHeaderID_ConflictWithPivot()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(sql);
			var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var staff = new GlbStaff("S1", "S1.Login");

			var job1 = new PkgPackageJob(order.PK, "PJ001", "WD").AppendInsertAndReturnObject(sql);
			var pkgID123 = new PkgPackageHeader("123", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);
			var pkgID456 = new PkgPackageHeader("456", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);

			new PkgPackageJobPackageHeaderPivot(job1, pkgID123).AppendInsertAndReturnObject(sql);
			new PkgPackageJobPackageHeaderPivot(job1, pkgID456).AppendInsertAndReturnObject(sql);

			sql.Append(PkgPackageHeader.UpdateWhere(pkgID456.PK).Set(c => c.KPH_PackageID, pkgID123.KPH_PackageID).AsSQL());
			AssertExceptionThrown(
				typeof(SqlException),
				expectedExceptionMessage,
				() => TestConnection.ExecuteNonQuery(sql.ToString()));
		}

		#endregion

		#region TestUpdate_PkgPackageHeaderID_NoConflictWithPackage_OtherJob

		[UseSnapshotProtection]
		public void TestUpdate_PkgPackageHeaderID_NoConflictWithPackage_OtherJob()
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
			var pkgID456 = new PkgPackageHeader("456", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);

			var pkg123OnJob1 = new PkgPackage(job1, pkgID123, "PLT", 1).AppendInsertAndReturnObject(sql);
			var pkg456OnJob2 = new PkgPackage(job2, pkgID456, "PLT", 1).AppendInsertAndReturnObject(sql);

			sql.Append(PkgPackageHeader.UpdateWhere(pkgID456.PK).Set(c => c.KPH_PackageID, pkgID123.KPH_PackageID).AsSQL());
			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(sql.ToString()));
		}

		#endregion

		#region TestUpdate_PkgPackageHeaderID_NoConflictWithPivot_OtherJob

		[UseSnapshotProtection]
		public void TestUpdate_PkgPackageHeaderID_NoConflictWithPivot_OtherJob()
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
			var pkgID456 = new PkgPackageHeader("456", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);

			new PkgPackageJobPackageHeaderPivot(job1, pkgID123).AppendInsertAndReturnObject(sql);
			new PkgPackageJobPackageHeaderPivot(job2, pkgID456).AppendInsertAndReturnObject(sql);

			sql.Append(PkgPackageHeader.UpdateWhere(pkgID456.PK).Set(c => c.KPH_PackageID, pkgID123.KPH_PackageID).AsSQL());
			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(sql.ToString()));
		}

		#endregion

		#region Implementation

		const string expectedExceptionMessage = @"Package ID needs to be unique per Job.
The transaction ended in the trigger. The batch has been aborted.";

		#endregion
	}
}


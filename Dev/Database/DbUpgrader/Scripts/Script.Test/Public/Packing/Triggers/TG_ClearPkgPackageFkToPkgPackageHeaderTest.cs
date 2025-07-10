using System;
using System.Text;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Packing.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Packing.Triggers.Testing
{
	[TestedType(typeof(TG_ClearPkgPackageFkToPkgPackageHeader))]
	class TG_ClearPkgPackageFkToPkgPackageHeaderTest : DbCreateScriptTest
	{
		[UseSnapshotProtection]
		public void TestClearPkgPackageFkToPkgPackageHeader()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(sql);
			var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
			var staff = new GlbStaff("S1", "S1.Login");

			// 2 IDs
			var pkgID1 = new PkgPackageHeader("A1", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);
			var pkgID2 = new PkgPackageHeader("A2", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);

			// Job w/ 2 packages and direct IDs
			var order1 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);

			var job = new PkgPackageJob(order1.PK, "PJ001", "WD").AppendInsertAndReturnObject(sql);
			var pkg1OnJob = new PkgPackage(job, pkgID1, "PLT", 1).AppendInsertAndReturnObject(sql);
			var pkg2OnJob = new PkgPackage(job, pkgID2, "PLT", 1).AppendInsertAndReturnObject(sql);

			// Job (Other) w/ 2 packages and direct ids
			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O2").AppendInsertAndReturnObject(sql);
			var jobOther = new PkgPackageJob(order2.PK, "PJ002", "WD").AppendInsertAndReturnObject(sql);

			var pkg1OnJobOther = new PkgPackage(jobOther, pkgID1, "PLT", 1).AppendInsertAndReturnObject(sql);
			var pkg2OnJobOther = new PkgPackage(jobOther, pkgID2, "PLT", 1).AppendInsertAndReturnObject(sql);

			// create pivot - should clear direct id on pkg1
			var pkgHeaderPivot = new PkgPackageJobPackageHeaderPivot(job, pkgID1);
			pkgHeaderPivot.AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertEquals("Should have cleared pkg1 FK.", 1, GetPkgPackageCount(pkg1OnJob.PK));
			AssertEquals("Should not have cleared pkg2 FK.", 0, GetPkgPackageCount(pkg2OnJob.PK));
			AssertEquals("Should not have cleared pkg1 FK on job other.", 0, GetPkgPackageCount(pkg1OnJobOther.PK));
			AssertEquals("Should not have cleared pkg2 FK on job other.", 0, GetPkgPackageCount(pkg2OnJobOther.PK));
		}

		int GetPkgPackageCount(Guid pk) => PkgPackage.CountInDB(TestConnection, p => p.PK == pk && p.KP_KPH_PackageHeader == null);
	}
}


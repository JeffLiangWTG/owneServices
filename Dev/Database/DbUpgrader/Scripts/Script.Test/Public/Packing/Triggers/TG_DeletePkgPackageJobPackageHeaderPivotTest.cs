using System;
using System.Text;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Packing.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Packing.Triggers.Testing
{
	[TestedType(typeof(TG_DeletePkgPackageJobPackageHeaderPivot))]
	class TG_DeletePkgPackageJobPackageHeaderPivotTest : DbCreateScriptTest
	{
		[UseSnapshotProtection]
		public void TestGivingPackageAnIDDeletesThePivot()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(sql);
			var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var staff = new GlbStaff("S1", "S1.Login");

			// 2 IDs
			var pkgID1 = new PkgPackageHeader("A1", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);
			var pkgID2 = new PkgPackageHeader("A2", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);

			// Job w/ 2 packages and 2 loose ids
			var job = new PkgPackageJob(order.PK, "PJ001", "WD").AppendInsertAndReturnObject(sql);

			var pkg1OnJob = new PkgPackage(job,"PLT", 1).AppendInsertAndReturnObject(sql);
			var pkg2OnJob = new PkgPackage(job,"PLT", 1).AppendInsertAndReturnObject(sql);

			var pivotID1 = new PkgPackageJobPackageHeaderPivot(job, pkgID1).AppendInsertAndReturnObject(sql);
			var pivotID2 = new PkgPackageJobPackageHeaderPivot(job, pkgID2).AppendInsertAndReturnObject(sql);

			// Job (Other) w/ 2 packages and 2 loose ids
			var order2 = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O2").AppendInsertAndReturnObject(sql);
			var jobOther = new PkgPackageJob(order2.PK, "PJ002", "WD").AppendInsertAndReturnObject(sql);

			var pkg1OnJobOther = new PkgPackage(jobOther, "PLT", 1).AppendInsertAndReturnObject(sql);
			var pkg2OnJobOther = new PkgPackage(jobOther, "PLT", 1).AppendInsertAndReturnObject(sql);

			var pivotID1Other = new PkgPackageJobPackageHeaderPivot(jobOther, pkgID1).AppendInsertAndReturnObject(sql);
			var pivotID2Other = new PkgPackageJobPackageHeaderPivot(jobOther, pkgID2).AppendInsertAndReturnObject(sql);

			// link package to ID - should delete Pivot
			sql.Append(PkgPackage.UpdateWhere(pkg1OnJob.PK).Set(c => c.KP_KPH_PackageHeader, pkgID1).AsSQL());
			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertEquals("Should     have deleted Job1 & ID1 Pivot.", 0, GetPkgPackageJobPackageHeaderPivotPivotCount(pivotID1.PK));
			AssertEquals("Should NOT have deleted Job1 & ID2 Pivot.", 1, GetPkgPackageJobPackageHeaderPivotPivotCount(pivotID2.PK));
			AssertEquals("Should NOT have deleted Job2 & ID1 Pivot.", 1, GetPkgPackageJobPackageHeaderPivotPivotCount(pivotID1Other.PK));
			AssertEquals("Should NOT have deleted Job2 & ID2 Pivot.", 1, GetPkgPackageJobPackageHeaderPivotPivotCount(pivotID2Other.PK));
		}

		int GetPkgPackageJobPackageHeaderPivotPivotCount(Guid pk) => PkgPackageJobPackageHeaderPivot.CountInDB(TestConnection, p => p.PK == pk);
	}
}


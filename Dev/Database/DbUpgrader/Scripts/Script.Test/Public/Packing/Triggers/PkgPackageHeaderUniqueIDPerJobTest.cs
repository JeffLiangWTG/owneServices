
using System;
using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Packing.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Packing.Triggers.Testing
{
	[TestedType(typeof(PkgPackageHeaderUniqueIDPerJob))]
	class PkgPackageHeaderUniqueIDPerJobTest : DbCreateScriptTest
	{
		#region TestUpdate_PkgPackageHeaderID_ConflictWithPackage

		public void TestUpdate_PkgPackageHeaderID_ConflictWithPackage()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(sql);
			var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var staff = new GlbStaff("S1", "S1.Login");

			var job1 = new PkgPackageJob(order.PK, "PJ001", "WD").AppendInsertAndReturnObject(sql);
			var pkgID123 = new PkgPackageHeader("123", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);
			var pkgID465 = new PkgPackageHeader("456", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);

			var pkg123OnJob1 = new PkgPackage(job1, pkgID123, "PLT", 1).AppendInsertAndReturnObject(sql);
			var pkg456OnJob1 = new PkgPackage(job1, pkgID465, "PLT", 1).AppendInsertAndReturnObject(sql);

			sql.Append($@"
EXEC dbo.SuspendTrigger 'TG_PkgPackageHeaderUniqueIDPerJob'");
			sql.Append(PkgPackageHeader.UpdateWhere(pkgID465.PK).Set(c => c.KPH_PackageID, pkgID123.KPH_PackageID).AsSQL());
			sql.Append($@"
declare @pks dbo.TVP_uniqueidentifier
insert into @pks values('{pkgID123.PK}')
execute dbo.PkgPackageHeaderUniqueIDPerJob @pks
");
			AssertExceptionThrown(
				typeof(SqlException),
				"Package ID needs to be unique per Job.",
				() => TestConnection.ExecuteNonQuery(sql.ToString()));
		}

		#endregion

		#region TestUpdate_PkgPackageHeaderID_TriggerSuspensionWorks

		public void TestUpdate_PkgPackageHeaderID_TriggerSuspensionWorks()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(sql);
			var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var staff = new GlbStaff("S1", "S1.Login");

			var job1 = new PkgPackageJob(order.PK, "PJ001", "WD").AppendInsertAndReturnObject(sql);

			var pkgIDABC = new PkgPackageHeader("ABC", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);
			var pkgIDDEF = new PkgPackageHeader("DEF", DateTime.UtcNow, staff.GS_Code).AppendInsertAndReturnObject(sql);

			var pkgIDABCOnJob1 = new PkgPackage(job1, pkgIDABC, "PLT", 1).AppendInsertAndReturnObject(sql);
			var pkgIDDEFOnJob1 = new PkgPackage(job1, pkgIDDEF, "PLT", 1).AppendInsertAndReturnObject(sql);

			sql.Append($@"
EXEC dbo.SuspendTrigger 'TG_PkgPackageHeaderUniqueIDPerJob'");
			sql.Append(PkgPackageHeader.UpdateWhere(pkgIDABC.PK).Set(c => c.KPH_PackageID, "DEF").Set(c => c.KPH_SystemLastEditTimeUtc, DateTime.UtcNow).Set(c => c.KPH_SystemLastEditUser, "~BP").AsSQL());

			// Implemented a standard SQL query instead of UpdateWhere due to limitations in the current UpdateWhere implementation, which does not allow multiple UpdateWhere statements within the same query.
			sql.Append($"UPDATE dbo.PkgPackageHeader SET KPH_PackageID = 'GHI', KPH_SystemLastEditTimeUtc = GETUTCDATE(), KPH_SystemLastEditUser = '~BP' WHERE KPH_PK = '{pkgIDDEF.PK}'");

			sql.Append($@"
EXEC dbo.ResumeTrigger 'TG_PkgPackageHeaderUniqueIDPerJob'
declare @pks dbo.TVP_uniqueidentifier
insert into @pks values('{pkgIDABC.PK}')
insert into @pks values('{pkgIDDEF.PK}')
execute dbo.PkgPackageHeaderUniqueIDPerJob @pks
");
			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(sql.ToString()));
		}

		#endregion
	}
}

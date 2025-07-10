using System.Text;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Packing.Triggers.Testing
{
	[TestedType(typeof(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit))]
	class TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnitTest : DbCreateScriptTest
	{
		#region TestUpdate_PkgPackageTopHandlingUnitPackage

		public void TestUpdate_PkgPackageTopHandlingUnitPackage_ThrowsException()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(sql);
			var client = new OrgHeader("O1").AppendInsertAndReturnObject(sql);
			var order = new WhsDocket(client.PK, whs.PK, "ORD", "ORD", "ENT", "O1").AppendInsertAndReturnObject(sql);
			var staff = new GlbStaff("S1", "S1.Login");

			var job = new PkgPackageJob(order.PK, "PJ001", "WD").AppendInsertAndReturnObject(sql);
			var handlingUnitPackage = new PkgPackage(job, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package = new PkgPackage(job, "PLT", 1).AppendInsertAndReturnObject(sql);

			sql.Append($@"
EXEC dbo.SuspendTrigger 'TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit'");
			sql.Append(PkgPackage.UpdateWhere(package.PK).Set(c => c.KP_KP_TopHandlingUnitPackage, handlingUnitPackage).AsSQL());
			sql.Append($@"
DECLARE @pks dbo.TVP_uniqueidentifier
INSERT INTO @pks VALUES('{package.PK}')
EXECUTE dbo.PkgCheckHUAndChildPackageHaveSameTopLevelHandlingUnit @pks
");
			AssertExceptionThrown(
				typeof(SqlException),
				"Attempted to insert/update a package has top level handling unit but not on a handling unit.",
				() => TestConnection.ExecuteNonQuery(sql.ToString()));
		}

		#endregion

		#region TestUpdate_PkgPackageHeaderID_TriggerSuspensionWorks

		public void TestUpdate_PkgPackageTopHandlingUnitPackage_TriggerSuspensionWorks()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
			var job = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			// Single level
			var singleLevelHandlingUnit = new PkgPackage(job, "PLT", 1).AppendInsertAndReturnObject(sql);
			var singleLevelHandlingUnitInner = new PkgPackage(job, singleLevelHandlingUnit, "PKG", 1).AppendInsertAndReturnObject(sql);
			new PkgPackageHandlingUnitDivot(singleLevelHandlingUnit.PK, singleLevelHandlingUnitInner.PK).AppendInsertAndReturnObject(sql);

			// Multi level with sub handling unit
			var topLevelHandlingUnit = new PkgPackage(job, "PLT", 1).AppendInsertAndReturnObject(sql);
			var subLevelHandlingUnit = new PkgPackage(job, topLevelHandlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);
			var topLevelHandlingUnitInner = new PkgPackage(job, topLevelHandlingUnit, "PKG", 1).AppendInsertAndReturnObject(sql);
			var subLevelHandlingUnitInner = new PkgPackage(job, topLevelHandlingUnit, "PKG", 1).AppendInsertAndReturnObject(sql);
			new PkgPackageHandlingUnitDivot(topLevelHandlingUnit.PK, subLevelHandlingUnit.PK).AppendInsertAndReturnObject(sql);
			new PkgPackageHandlingUnitDivot(topLevelHandlingUnit.PK, topLevelHandlingUnitInner.PK).AppendInsertAndReturnObject(sql);
			new PkgPackageHandlingUnitDivot(subLevelHandlingUnit.PK, subLevelHandlingUnitInner.PK).AppendInsertAndReturnObject(sql);

			var query = $@"EXEC dbo.SuspendTrigger 'TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit'
{sql.ToStringWithNewLineBetweenAppends()}
EXEC dbo.ResumeTrigger 'TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit'
DECLARE @pks dbo.TVP_uniqueidentifier
INSERT INTO @pks VALUES ('{singleLevelHandlingUnitInner.PK}'), ('{subLevelHandlingUnit.PK}'), ('{topLevelHandlingUnitInner.PK}'), ('{subLevelHandlingUnitInner.PK}')
EXECUTE dbo.PkgCheckHUAndChildPackageHaveSameTopLevelHandlingUnit @pks";

			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(query));
		}

		#endregion
	}
}

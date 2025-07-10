using System;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(PkgHandlingUnitDivotCheckHaveSameTopLevelHandlingUnit))]
	class PkgHandlingUnitDivotCheckHaveSameTopLevelHandlingUnitTest : DbCreateScriptTest
	{
		const string EnsureChildPackagesHaveSameTopLevelHandlingUnit = "dbo.TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit";
		const string EnsureTopLevelHandlingUnitPackageWhenInsertDivot = "dbo.TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit";
		static readonly string PkgPackageTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(PkgPackageSchema.Instance);
		static readonly string PkgPackageHandlingUnitDivotTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(PkgPackageHandlingUnitDivotSchema.Instance);

		#region TestValidCases

		public void TestValidCases()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit,
				PkgPackageTableNameForSQL, TestConnection))
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);
				var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
				var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

				var topLevelHU = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
				var middleLevelHU = new PkgPackage(packageJob, topLevelHU, "PLT", 1).AppendInsertAndReturnObject(sql);
				var childPackage1 = new PkgPackage(packageJob, topLevelHU, "PLT", 1).AppendInsertAndReturnObject(sql);

				var divot1 = new PkgPackageHandlingUnitDivot(topLevelHU.PK, middleLevelHU.PK).AppendInsertAndReturnObject(sql);
				var divot2 = new PkgPackageHandlingUnitDivot(middleLevelHU.PK, childPackage1.PK).AppendInsertAndReturnObject(sql);

				var handlingUnit = new PkgPackage(packageJob, "BOX", 1).AppendInsertAndReturnObject(sql);
				var childPackage2 = new PkgPackage(packageJob, handlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);

				var divot3 = new PkgPackageHandlingUnitDivot(handlingUnit.PK, childPackage2.PK).AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				TestPkgPackageDivotCheck(new[] { divot1.PK, divot2.PK }, false);
				TestPkgPackageDivotCheck(new[] { divot3.PK }, false);
			}
		}

		#endregion

		#region TestDivotsForHUAndChildPackageHaveDifferentTopHU

		public void TestDivotsForHUAndChildPackageHaveDifferentTopHU()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit,
				PkgPackageTableNameForSQL, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureTopLevelHandlingUnitPackageWhenInsertDivot, PkgPackageHandlingUnitDivotTableNameForSQL, TestConnection))
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);

				var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
				var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
				var package1 = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package2 = new PkgPackage(packageJob, package1, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package3 = new PkgPackage(packageJob, package2, "PLT", 1).AppendInsertAndReturnObject(sql);

				var divot1 = new PkgPackageHandlingUnitDivot(package1.PK, package2.PK).AppendInsertAndReturnObject(sql);
				var divot2 = new PkgPackageHandlingUnitDivot(package2.PK, package3.PK).AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				TestPkgPackageDivotCheck(new[] { divot1.PK, divot2.PK }, true);
			}
		}

		#endregion

		#region TestDivotsForChildPackageHasNoTopLevelHU

		public void TestDivotsForChildPackageHasNoTopLevelHU()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit,
				PkgPackageTableNameForSQL, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureTopLevelHandlingUnitPackageWhenInsertDivot, PkgPackageHandlingUnitDivotTableNameForSQL, TestConnection))
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);

				var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
				var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
				var package1 = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package2 = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);

				var divot = new PkgPackageHandlingUnitDivot(package1.PK, package2.PK).AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				TestPkgPackageDivotCheck(new[] { divot.PK }, true);
			}
		}

		#endregion

		#region TestUnpackDivot

		public void TestUnpackDivot()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit,
				PkgPackageTableNameForSQL, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureTopLevelHandlingUnitPackageWhenInsertDivot, PkgPackageHandlingUnitDivotTableNameForSQL, TestConnection))
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);

				var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
				var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
				var package1 = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package2 = new PkgPackage(packageJob, package1, "PLT", 1).AppendInsertAndReturnObject(sql);

				var divot = new PkgPackageHandlingUnitDivot(package1.PK, package2.PK, DateTime.Now.AddMinutes(10)).AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				TestPkgPackageDivotCheck(new[] { divot.PK }, true, true);
			}
		}

		#endregion

		void TestPkgPackageDivotCheck(Guid[] divotPKs, bool expectAnException, bool unpackCase = false)
		{
			const string sql =
@"
EXEC dbo.PkgHandlingUnitDivotCheckHaveSameTopLevelHandlingUnit @DivotPKs;
";

			var errorMessage = unpackCase ? "Unpacked child package should not have a top level handling unit."
				: "Packed package should have the same top level handling unit as its handling unit.";

			using (var sqlCommand = Db.Connection.Command(sql))
			{
				sqlCommand.AddTableValuedParameter("@DivotPKs", "dbo.TVP_uniqueidentifier", divotPKs);
				if (expectAnException)
				{
					TestWhsDataSetupHelper.IgnoreExceptionWhenTrackingSqlErrors(ex => ex.Message == errorMessage);
					AssertExceptionThrown(typeof(SqlException), () => sqlCommand.ExecuteNonQuery());
				}
				else
				{
					AssertNoExceptionThrown(() => sqlCommand.ExecuteNonQuery());
				}
			}
		}
	}
}

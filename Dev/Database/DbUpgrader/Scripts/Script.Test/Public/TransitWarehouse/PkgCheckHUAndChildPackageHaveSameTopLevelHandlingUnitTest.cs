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
	[TestedType(typeof(PkgCheckHUAndChildPackageHaveSameTopLevelHandlingUnit))]
	class PkgCheckHUAndChildPackageHaveSameTopLevelHandlingUnitTest : DbCreateScriptTest
	{
		const string EnsureChildPackagesHaveSameTopLevelHandlingUnit = "dbo.TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit";
		const string EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit = "dbo.TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit";
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

				new PkgPackageHandlingUnitDivot(topLevelHU.PK, middleLevelHU.PK).AppendInsertAndReturnObject(sql);
				new PkgPackageHandlingUnitDivot(middleLevelHU.PK, childPackage1.PK).AppendInsertAndReturnObject(sql);

				var standAlonePackage = new PkgPackage(packageJob, "BAG", 1).AppendInsertAndReturnObject(sql);

				var handlingUnit = new PkgPackage(packageJob, "BOX", 1).AppendInsertAndReturnObject(sql);
				var childPackage2 = new PkgPackage(packageJob, handlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);

				new PkgPackageHandlingUnitDivot(handlingUnit.PK, childPackage2.PK).AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				TestTopLevelHandlingUnitCheck(new[] { topLevelHU.PK, middleLevelHU.PK, childPackage1.PK }, false);
				TestTopLevelHandlingUnitCheck(new[] { standAlonePackage.PK }, false);
				TestTopLevelHandlingUnitCheck(new[] { handlingUnit.PK, childPackage2.PK }, false);
			}
		}

		#endregion

		#region TestDirectChildPackageInvalidSituation

		public void TestDirectChildPackageInvalidSituation()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit,
					PkgPackageTableNameForSQL, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit, PkgPackageHandlingUnitDivotTableNameForSQL, TestConnection))
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);

				var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
				var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

				var handlingUnit = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package_NoTopLevelHandlingUnit = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package_IncorrectTopLevelHandlingUnit = new PkgPackage(packageJob, package_NoTopLevelHandlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);

				new PkgPackageHandlingUnitDivot(handlingUnit.PK, package_NoTopLevelHandlingUnit.PK).AppendInsertAndReturnObject(sql);
				new PkgPackageHandlingUnitDivot(handlingUnit.PK, package_IncorrectTopLevelHandlingUnit.PK).AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				TestTopLevelHandlingUnitCheck(new[] { handlingUnit.PK }, true);
				TestTopLevelHandlingUnitCheck(new[] { package_NoTopLevelHandlingUnit.PK }, true);
				TestTopLevelHandlingUnitCheck(new[] { package_IncorrectTopLevelHandlingUnit.PK }, true);
			}
	}

		#endregion

		#region TestHUAndChildPackageHaveDifferentTopHU

		public void TestHUAndChildPackageHaveDifferentTopHU()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit,
					PkgPackageTableNameForSQL, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit, PkgPackageHandlingUnitDivotTableNameForSQL, TestConnection))
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);

				var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
				var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
				var topHandlingUnit = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
				var childpackage_firstLevel = new PkgPackage(packageJob, topHandlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);
				var childpackage_secondLevel = new PkgPackage(packageJob, childpackage_firstLevel, "PLT", 1).AppendInsertAndReturnObject(sql);

				new PkgPackageHandlingUnitDivot(topHandlingUnit.PK, childpackage_firstLevel.PK).AppendInsertAndReturnObject(sql);
				new PkgPackageHandlingUnitDivot(childpackage_firstLevel.PK, childpackage_secondLevel.PK).AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				TestTopLevelHandlingUnitCheck(new[] { topHandlingUnit.PK }, false);
				TestTopLevelHandlingUnitCheck(new[] { childpackage_firstLevel.PK }, true);
				TestTopLevelHandlingUnitCheck(new[] { childpackage_secondLevel.PK }, true);
			}
		}

		#endregion

		#region TestDirectChildPackage

		public void TestDirectChildPackage()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit,
					PkgPackageTableNameForSQL, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit, PkgPackageHandlingUnitDivotTableNameForSQL, TestConnection))
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);

				var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
				var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
				var package1 = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
				var package2 = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);

				new PkgPackageHandlingUnitDivot(package1.PK, package2.PK).AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				TestTopLevelHandlingUnitCheck(new[] { package1.PK, package2.PK }, true);
			}
		}

		#endregion

		#region TestStandAlonePackageHasTopHandlingUnit

		public void TestStandAlonePackageHasTopHandlingUnit()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit,
					PkgPackageTableNameForSQL, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit, PkgPackageHandlingUnitDivotTableNameForSQL, TestConnection))
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);

				var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
				var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
				var package = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
				var standAlonePackageHasTopHandlingUnit = new PkgPackage(packageJob, package, "PLT", 1).AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				TestTopLevelHandlingUnitCheck(new[] { package.PK }, false);
				TestTopLevelHandlingUnitCheck(new[] { standAlonePackageHasTopHandlingUnit.PK }, true, false);
			}
		}

		#endregion

		void TestTopLevelHandlingUnitCheck(Guid[] packagePKs, bool expectAnException, bool isPackagePackedToHU = true)
		{
			const string sql =
@"
EXEC dbo.PkgCheckHUAndChildPackageHaveSameTopLevelHandlingUnit @PackagePKs;
";
			using (var sqlCommand = Db.Connection.Command(sql))
			{
				sqlCommand.AddTableValuedParameter("@PackagePKs", "dbo.TVP_uniqueidentifier", packagePKs);
				var errorMessage = isPackagePackedToHU ? "Attempted to insert/update a package which is packed in a different handling unit."
														: "Attempted to insert/update a package has top level handling unit but not on a handling unit.";
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

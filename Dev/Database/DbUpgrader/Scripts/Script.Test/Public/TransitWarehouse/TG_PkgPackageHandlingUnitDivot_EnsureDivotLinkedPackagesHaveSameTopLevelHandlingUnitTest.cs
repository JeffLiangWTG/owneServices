using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit))]
	class TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnitTest : DBCreateTriggerScriptTest
	{
		const string TriggerErrorMessage = "Packed package should have the same top level handling unit as its handling unit.";
		const string TriggerErrorMessage2 = "Unpacked child package should not have a top level handling unit.";
		const string EnsureChildPackagesHaveSameTopLevelHandlingUnit = "dbo.TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit";
		static readonly string PkgPackageTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(PkgPackageSchema.Instance);

		#region TestTrigger_InsertUnpackDivot

		public void TestTrigger_InsertUnpackDivot()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
			var package1 = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package2 = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);

			new PkgPackageHandlingUnitDivot(package1.PK, package2.PK, DateTime.Now.AddMinutes(10)).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should not throw exception", () => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()));
		}

		#endregion

		#region Testtrigger_InsertInvalidDivot

		public void Testtrigger_InsertInvalidDivot()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
			var package1 = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package2 = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);

			new PkgPackageHandlingUnitDivot(package1.PK, package2.PK).AppendInsertAndReturnObject(sql);
			AssertExceptionThrown("Should throw an exception", typeof(SqlException), TriggerErrorMessage, () => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()), true);
		}

		#endregion

		#region TestTrigger_UnpackDivot

		public void TestTrigger_UnpackDivot()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
			var package1 = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package2 = new PkgPackage(packageJob, package1, "PLT", 1).AppendInsertAndReturnObject(sql);

			var divot = new PkgPackageHandlingUnitDivot(package1.PK, package2.PK).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit,
				PkgPackageTableNameForSQL, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var updateSql = PkgPackageHandlingUnitDivot.UpdateWhere(divot.PK).Set(d => d.KPD_UnpackedTime, DateTime.Now).Set(d => d.KPD_GS_NKUnpackedUser, "BP~").AsSQL();

			AssertExceptionThrown("Should throw an exception", typeof(SqlException), TriggerErrorMessage2, () => TestConnection.ExecuteNonQuery(updateSql), true);
		}

		#endregion
	}

	class TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnitNonTransactionTestCase : TestCase
	{
		const string PackageProcedureName = "PkgCheckHUAndChildPackageHaveSameTopLevelHandlingUnit";
		const string PackageTriggerName = "TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit";
		const string ProcedureName = "PkgHandlingUnitDivotCheckHaveSameTopLevelHandlingUnit";
		const string TriggerName = "TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit";
		static readonly string PkgPackageTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(PkgPackageSchema.Instance);
		static readonly string PkgPackageHandlingUnitDivotTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(PkgPackageHandlingUnitDivotSchema.Instance);

		#region TestTrigger_Insert

		[UseSnapshotProtection]
		public void TestTrigger_Insert()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(mainConnection);
				var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
				var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

				var handlingUnit = new PkgPackage(packageJob, "BOX", 1).AppendInsertAndReturnObject(sql);
				var childPackage = new PkgPackage(packageJob, handlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);

				var divot = new PkgPackageHandlingUnitDivot(handlingUnit.PK, childPackage.PK).AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, PackageTriggerName, PkgPackageTableNameForSQL, PkgPackageSchema.Constants.PK, PackageProcedureName))
				{
					AssertCheckProcedureRanForDivots("Insert divot should trigger check procedure.", mainConnection, sql.ToStringWithNewLineBetweenAppends(), divot);
				}
			}
		}

		#endregion

		#region TestTrigger_Update

		[UseSnapshotProtection]
		public void TestTrigger_Update()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(mainConnection);
				var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
				var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

				var handlingUnit = new PkgPackage(packageJob, "BOX", 1).AppendInsertAndReturnObject(sql);
				var childPackage = new PkgPackage(packageJob, handlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);

				var divot = new PkgPackageHandlingUnitDivot(handlingUnit.PK, childPackage.PK).AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, PackageTriggerName, PkgPackageTableNameForSQL, PkgPackageSchema.Constants.PK, PackageProcedureName))
				{
					ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());
				}

				var updateSql =
	$@"
UPDATE {PkgPackageTableNameForSQL}
SET {PkgPackageSchema.Constants.KP_KP_TopHandlingUnitPackage} = NULL,
{PkgPackageSchema.Constants.KP_SystemLastEditTimeUtc} = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', {PkgPackageSchema.Constants.KP_SystemLastEditUser} ='BP~'
WHERE KP_PK = '{childPackage.PK}'
UPDATE {PkgPackageHandlingUnitDivotTableNameForSQL}
SET {PkgPackageHandlingUnitDivotSchema.Constants.KPD_UnpackedTime} = '{DateTime.Now.AddDays(1):yyyy-MM-dd HH:mm:ss}', {PkgPackageHandlingUnitDivotSchema.Constants.KPD_GS_NKUnpackedUser} ='BP~',
{PkgPackageHandlingUnitDivotSchema.Constants.KPD_SystemLastEditTimeUtc} = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', {PkgPackageHandlingUnitDivotSchema.Constants.KPD_SystemLastEditUser} ='BP~'
WHERE KPD_PK = '{divot.PK}'
";
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, PackageTriggerName, PkgPackageTableNameForSQL, PkgPackageSchema.Constants.PK, PackageProcedureName))
				{
					AssertCheckProcedureRanForDivots(
						"Update of record should trigger check procedure.",
						mainConnection,
						updateSql,
						divot);
				}
			}
		}

		#endregion

		#region TestTrigger_IsDeferred

		[UseSnapshotProtection]
		public void TestTrigger_IsDeferred()
		{
			TestTrigger_DeferralCore(isDeferred: true);
		}

		[UseSnapshotProtection]
		public void TestTrigger_IsNotDeferred()
		{
			TestTrigger_DeferralCore(isDeferred: false);
		}

		void TestTrigger_DeferralCore(bool isDeferred)
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(mainConnection);
				var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
				var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

				var handlingUnit = new PkgPackage(packageJob, "BOX", 1).AppendInsertAndReturnObject(sql);
				var childPackage = new PkgPackage(packageJob, handlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);

				var divot = new PkgPackageHandlingUnitDivot(handlingUnit.PK, childPackage.PK).AppendInsertAndReturnObject(sql);

				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, PackageTriggerName, PkgPackageTableNameForSQL, PkgPackageSchema.Constants.PK, PackageProcedureName))
				{
					ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());
				}

				var updateSql =
	$@"
UPDATE {PkgPackageTableNameForSQL} 
SET {PkgPackageSchema.Constants.KP_KP_TopHandlingUnitPackage} = NULL,
{PkgPackageSchema.Constants.KP_SystemLastEditTimeUtc} = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', {PkgPackageSchema.Constants.KP_SystemLastEditUser} ='BP~'
WHERE KP_PK = '{childPackage.PK}'
UPDATE {PkgPackageHandlingUnitDivotTableNameForSQL}
SET {PkgPackageHandlingUnitDivotSchema.Constants.KPD_UnpackedTime} = '{DateTime.Now.AddDays(1):yyyy-MM-dd HH:mm:ss}', {PkgPackageHandlingUnitDivotSchema.Constants.KPD_GS_NKUnpackedUser} ='BP~',
{PkgPackageHandlingUnitDivotSchema.Constants.KPD_SystemLastEditTimeUtc} = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', {PkgPackageHandlingUnitDivotSchema.Constants.KPD_SystemLastEditUser} ='BP~'
WHERE KPD_PK = '{divot.PK}'
";

				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, PackageTriggerName, PkgPackageTableNameForSQL, PkgPackageSchema.Constants.PK, PackageProcedureName))
				{
					if (isDeferred)
					{
						// mocked procedure will always throw exception if run
						AssertNoExceptionThrown("If trigger is suspended then check procedure should not be run.",
							() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, ProcedureName, $"SuspendTrigger '{TriggerName}'\r\n" + updateSql));
					}
					else
					{
						AssertCheckProcedureRanForDivots("If trigger is not suspended then check procedure should be run.", mainConnection, updateSql, divot);
					}
				}
			}
		}

		#endregion

		void AssertCheckProcedureRanForDivots(string errorMessage, DbConnection connection, string sqlToRun, params PkgPackageHandlingUnitDivot[] expectedDivotsInTheProcedure)
		{
			var actualPKsInTheProcedure = TestWhsDataSetupHelper.AssertCheckProcedureRanAndReturnGuidsPassedIntoProcedure(connection, ProcedureName, sqlToRun, expectedDivotsInTheProcedure);
			if (actualPKsInTheProcedure == null)
			{
				Fail(errorMessage);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(errorMessage, expectedDivotsInTheProcedure.Select(l => l.PK), actualPKsInTheProcedure);
			}
		}

		void ExecuteSqlInTransaction(DbConnection connection, string sql)
		{
			using (connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sql);
				connection.CommitTransaction();
			}
		}
	}
}

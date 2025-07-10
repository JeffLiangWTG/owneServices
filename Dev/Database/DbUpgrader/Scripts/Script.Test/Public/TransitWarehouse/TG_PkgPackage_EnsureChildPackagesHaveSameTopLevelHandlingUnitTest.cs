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
	[TestedType(typeof(TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit))]
	class TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnitTest : DBCreateTriggerScriptTest
	{
		const string TriggerErrorMessage = @"Attempted to insert/update a package which is packed in a different handling unit.
The transaction ended in the trigger. The batch has been aborted.";
		const string TriggerErrorMessage2 = @"Attempted to insert/update a package has top level handling unit but not on a handling unit.
The transaction ended in the trigger. The batch has been aborted.";
		const string EnsureChildPackagesHaveSameTopLevelHandlingUnit = "dbo.TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit";
		const string EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit = "dbo.TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit";
		static readonly string PkgPackageTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(PkgPackageSchema.Instance);
		static readonly string PkgPackageHandlingUnitDivotTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(PkgPackageHandlingUnitDivotSchema.Instance);

		#region TestTrigger_InsertStandAlonePackage

		public void TestTrigger_EnsureChildPackagesHaveSameTopLevelHandlingUnit()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
			var package = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);

			AssertNoExceptionThrown("Should not throw exception", () => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()));
		}

		#endregion

		#region TestTrigger_InsertStandAlonePackageHasTopHandlingUnit

		public void TestTrigger_InsertStandAlonePackageHasTopHandlingUnit()
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			var package1 = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			var packageWithoutDivot = new PkgPackage(packageJob, package1, "PLT", 1).AppendInsertAndReturnObject(sql);

			AssertExceptionThrown("Should throw an exception", typeof(SqlException), TriggerErrorMessage2, () => TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends()), true);
		}

		#endregion

		#region TestTrigger_UpdateOnePackage

		public void TestUpdateTopLevelHU()
		{
			TestTrigger_UpdateOnePackageCore(0, false);
		}

		public void TestUpdateMiddleLevelHU_ToNull()
		{
			TestTrigger_UpdateOnePackageCore(1, true);
		}

		public void TestUpdateMiddleLevelHU_NotToNull()
		{
			TestTrigger_UpdateOnePackageCore(1, false);
		}

		public void TestUpdateChildPackage_ToNull()
		{
			TestTrigger_UpdateOnePackageCore(2, true);
		}

		public void TestUpdateChildPackage_NotToNull()
		{
			TestTrigger_UpdateOnePackageCore(2, false);
		}

		void TestTrigger_UpdateOnePackageCore(int level, bool convertToNull)
		{
			var sql = new SqlQueryBuilder();
			var whs = new WhsWarehouse("WH1").WithDockDoor(TestConnection);

			var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
			var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);
			var package1 = new PkgPackage(packageJob, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package2 = new PkgPackage(packageJob, package1, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package3 = new PkgPackage(packageJob, package1, "PLT", 1).AppendInsertAndReturnObject(sql);

			new PkgPackageHandlingUnitDivot(package1.PK, package2.PK).AppendInsertAndReturnObject(sql);
			new PkgPackageHandlingUnitDivot(package2.PK, package3.PK).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit,
				PkgPackageTableNameForSQL, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			}

			var packageID = package1.PK;
			var resultPackageID = convertToNull ? "NULL" : "'" + package2.PK.ToString() + "'";

			switch (level)
			{
				case 1:
					packageID = package2.PK;
					break;
				case 2:
					packageID = package3.PK;
					break;
				default:
					break;
			}

			AssertExceptionThrown("Should throw an exception.", typeof(SqlException), TriggerErrorMessage,
				() => TestConnection.ExecuteNonQuery(
$@"UPDATE {PkgPackageTableNameForSQL} 
SET {PkgPackageSchema.Constants.KP_KP_TopHandlingUnitPackage} = {resultPackageID},
{PkgPackageSchema.Constants.KP_SystemLastEditTimeUtc} = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', {PkgPackageSchema.Constants.KP_SystemLastEditUser} ='BP~'
WHERE KP_PK = '{packageID}'
"), true);
		}

		#endregion

		#region TestTrigger_UnpackPackages

		public void TestTrigger_UnpackPackages()
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

			var updateSql =
$@"
UPDATE {PkgPackageHandlingUnitDivotTableNameForSQL}
SET {PkgPackageHandlingUnitDivotSchema.Constants.KPD_UnpackedTime} = '{DateTime.Now.AddDays(1):yyyy-MM-dd HH:mm:ss}', {PkgPackageHandlingUnitDivotSchema.Constants.KPD_GS_NKUnpackedUser} ='BP~',
{PkgPackageHandlingUnitDivotSchema.Constants.KPD_SystemLastEditTimeUtc} = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', {PkgPackageHandlingUnitDivotSchema.Constants.KPD_SystemLastEditUser} ='BP~'
WHERE KPD_PK = '{divot.PK}'
UPDATE {PkgPackageTableNameForSQL}
SET {PkgPackageSchema.Constants.KP_KP_TopHandlingUnitPackage} = NULL,
{PkgPackageSchema.Constants.KP_SystemLastEditTimeUtc} = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', {PkgPackageSchema.Constants.KP_SystemLastEditUser} ='BP~'
WHERE KP_PK = '{package2.PK}'
";
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit, PkgPackageHandlingUnitDivotTableNameForSQL, TestConnection))
			{
				AssertNoExceptionThrown("Should not throw exception", () => TestConnection.ExecuteNonQuery(updateSql));
			}
		}

		#endregion
	}

	class TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnitNonTransactionTestCase : TestCase
	{
		const string ProcedureName = "PkgCheckHUAndChildPackageHaveSameTopLevelHandlingUnit";
		const string TriggerName = "TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit";
		const string DivotProcedureName = "PkgHandlingUnitDivotCheckHaveSameTopLevelHandlingUnit";
		const string DivotTriggerName = "TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit";
		static readonly string PkgPackageTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(PkgPackageSchema.Instance);
		static readonly string PkgPackageHandlingUnitDivotTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(PkgPackageHandlingUnitDivotSchema.Instance);

		#region TestTrigger_Insert

		[UseSnapshotProtection]
		public void TestTrigger_Insert_WithoutTopHandlingUnit()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(mainConnection);
				var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
				var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

				var standAlonePackage = new PkgPackage(packageJob, "BAG", 1).AppendInsertAndReturnObject(sql);

				AssertNoExceptionThrown("Insert packages with out Top Handling Unit should not trigger the trigger.",
					() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, ProcedureName, sql.ToStringWithNewLineBetweenAppends(), commitChanges: true));
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Insert_WithTopHandlingUnit()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				var sql = new SqlQueryBuilder();
				var whs = new WhsWarehouse("WH1").WithDockDoor(mainConnection);
				var rcn = new WhsItemReceiveConsignment(whs, "RC000001", "RC000001", "STD", "AUSYD");
				var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

				var handlingUnit = new PkgPackage(packageJob, "BOX", 1).AppendInsertAndReturnObject(sql);
				var childPackage = new PkgPackage(packageJob, handlingUnit, "PLT", 1).AppendInsertAndReturnObject(sql);

				AssertCheckProcedureRanForPkgPackages("Insert packages with Top Handling Unit should trigger check procedure.", mainConnection, sql.ToStringWithNewLineBetweenAppends(), new[] { childPackage });
			}
		}

		#endregion

		#region TestTrigger_Update

		[UseSnapshotProtection]
		public void TestTrigger_Update_OtherField()
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

				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TriggerName, PkgPackageTableNameForSQL, PkgPackageSchema.Constants.PK, ProcedureName))
				{
					ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());
				}

				// update Pack Type - should not effect trigger
				var updateSQL = PkgPackage.UpdateWhere(childPackage.PK).Set(p => p.KP_F3_NKPackType, "BOX").AsSQL();
				// mocked procedure will always throw exception if run
				AssertNoExceptionThrown("Update of not related field should NOT trigger the trigger.",
					() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, ProcedureName, updateSQL, commitChanges: true));
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_TopHandlingUnit()
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

				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TriggerName, PkgPackageTableNameForSQL, PkgPackageSchema.Constants.PK, ProcedureName))
				{
					ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());
				}

				var updateSql =
	$@"
UPDATE {PkgPackageHandlingUnitDivotTableNameForSQL}
SET {PkgPackageHandlingUnitDivotSchema.Constants.KPD_UnpackedTime} = '{DateTime.Now.AddDays(1):yyyy-MM-dd HH:mm:ss}', {PkgPackageHandlingUnitDivotSchema.Constants.KPD_GS_NKUnpackedUser} ='BP~',
{PkgPackageHandlingUnitDivotSchema.Constants.KPD_SystemLastEditTimeUtc} = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', {PkgPackageHandlingUnitDivotSchema.Constants.KPD_SystemLastEditUser} ='BP~'
WHERE KPD_PK = '{divot.PK}'
UPDATE {PkgPackageTableNameForSQL}
SET {PkgPackageSchema.Constants.KP_KP_TopHandlingUnitPackage} = NULL,
{PkgPackageSchema.Constants.KP_SystemLastEditTimeUtc} = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', 
{PkgPackageSchema.Constants.KP_SystemLastEditUser} ='BP~'
WHERE KP_PK = '{childPackage.PK}'
";
				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, DivotTriggerName, PkgPackageHandlingUnitDivotTableNameForSQL, PkgPackageHandlingUnitDivotSchema.Constants.PK, DivotProcedureName))
				{
					AssertCheckProcedureRanForPkgPackages(
						"Update of KP_KP_TopHandlingUnitPackage should trigger check procedure.",
						mainConnection,
						updateSql,
						childPackage);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestTrigger_Update_TopHandlingUnit_WithSameValue()
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

				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TriggerName, PkgPackageTableNameForSQL, PkgPackageSchema.Constants.PK, ProcedureName))
				{
					ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());
				}

				var updateSql =
	$@"
UPDATE {PkgPackageTableNameForSQL}
SET {PkgPackageSchema.Constants.KP_KP_TopHandlingUnitPackage} = '{handlingUnit.PK}',
{PkgPackageSchema.Constants.KP_SystemLastEditTimeUtc} = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', 
{PkgPackageSchema.Constants.KP_SystemLastEditUser} ='BP~'
WHERE KP_PK = '{childPackage.PK}'
";
				AssertNoExceptionThrown("Update TopHandlingUnit with same value should NOT trigger the trigger.", () => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, ProcedureName, updateSql, commitChanges: true));
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

				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, TriggerName, PkgPackageTableNameForSQL, PkgPackageSchema.Constants.PK, ProcedureName))
				{
					ExecuteSqlInTransaction(mainConnection, sql.ToStringWithNewLineBetweenAppends());
				}

				var updateSql =
	$@"
UPDATE {PkgPackageHandlingUnitDivotTableNameForSQL}
SET {PkgPackageHandlingUnitDivotSchema.Constants.KPD_UnpackedTime} = '{DateTime.Now.AddDays(1):yyyy-MM-dd HH:mm:ss}', {PkgPackageHandlingUnitDivotSchema.Constants.KPD_GS_NKUnpackedUser} ='BP~',
{PkgPackageHandlingUnitDivotSchema.Constants.KPD_SystemLastEditTimeUtc} = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', {PkgPackageHandlingUnitDivotSchema.Constants.KPD_SystemLastEditUser} ='BP~'
WHERE KPD_PK = '{divot.PK}'
UPDATE {PkgPackageTableNameForSQL}
SET {PkgPackageSchema.Constants.KP_KP_TopHandlingUnitPackage} = NULL,
{PkgPackageSchema.Constants.KP_SystemLastEditTimeUtc} = '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', {PkgPackageSchema.Constants.KP_SystemLastEditUser} ='BP~'
WHERE KP_PK = '{childPackage.PK}'
";

				using (TestWhsDataSetupHelper.SuspendTriggerAndRunAtEnd(mainConnection, DivotTriggerName, PkgPackageHandlingUnitDivotTableNameForSQL, PkgPackageHandlingUnitDivotSchema.Constants.PK, DivotProcedureName))
				{
					if (isDeferred)
					{
						// mocked procedure will always throw exception if run
						AssertNoExceptionThrown("If trigger is suspended then check procedure should not be run.",
							() => TestWhsDataSetupHelper.AssertCheckProcedureDidNotRunAndThrowExceptionIfDidRun(mainConnection, ProcedureName, $"SuspendTrigger '{TriggerName}'\r\n" + updateSql));
					}
					else
					{
						AssertCheckProcedureRanForPkgPackages("If trigger is not suspended then check procedure should be run.", mainConnection, updateSql, childPackage);
					}
				}
			}
		}

		#endregion

		void AssertCheckProcedureRanForPkgPackages(string errorMessage, DbConnection connection, string sqlToRun, params PkgPackage[] expectedPackagesInTheProcedure)
		{
			var actualPKsInTheProcedure = TestWhsDataSetupHelper.AssertCheckProcedureRanAndReturnGuidsPassedIntoProcedure(connection, ProcedureName, sqlToRun, expectedPackagesInTheProcedure);
			if (actualPKsInTheProcedure == null)
			{
				Fail(errorMessage);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(errorMessage, expectedPackagesInTheProcedure.Select(l => l.PK), actualPKsInTheProcedure);
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

using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsLoadPkgPackagePivot_PreventInsertingIfLoadHasNoStartTime))]
	class TG_WhsLoadPkgPackagePivot_PreventInsertingIfLoadHasNoStartTimeTest : DBCreateTriggerScriptTest
	{
		public void Test_WhsLoadPkgPackagePivot_PreventInsertingPackageIfLoadHasNoStartTime()
		{
			var (load, package) = PreparePackageAndLoadWithoutStartTime();
			var pivot = new WhsLoadPkgPackagePivot(load, package);

			AssertNull("Precondition: The prepared load has no StartTime", load.WLO_StartTime);
			AssertExceptionThrown<SqlException>(
				 message: "Insert should trigger exception if WLO_StartTime is null",
				 expectedExceptionMessage: @"Inserting packages into WhsLoadPkgPackagePivot is not allowed if WhsLoad has no StartTime.
The transaction ended in the trigger. The batch has been aborted.",
				 () => { pivot.InsertAndReturnObject(TestConnection); });
		}

		public void Test_WhsLoadPkgPackagePivot_AllowInsertingPackageIfLoadHasStartTime()
		{
			var (load, package) = PreparePackageAndLoadWithoutStartTime();
			var pivot = new WhsLoadPkgPackagePivot(load, package);

			WhsLoad.UpdateWhere(load.PK).Set(x => x.WLO_StartTime, DateTimeOffset.Now).Post(TestConnection);
			AssertNoExceptionThrown(() => { pivot.InsertAndReturnObject(TestConnection); });

			Assert("Insert succeeded.", WhsLoadPkgPackagePivot.ExistsInDB(TestConnection, pivot.PK));
		}

		public void Test_WhsLoadPkgPackagePivot_AllowInsertingHandlingUnitIfLoadHasStartTime()
		{
			var (load, _) = PreparePackageAndLoadWithoutStartTime();

			var handlingUnitPackageJob = new PkgPackageJob(
					Guid.NewGuid(),
					jobID: "KJ_HU001",
					parentTableCode: PkgHandlingUnitSchema.Constants.Prefix).InsertAndReturnObject(TestConnection);

			var handlingUnitPackage = new PkgPackage(handlingUnitPackageJob, "PKG", 1).InsertAndReturnObject(TestConnection);

			var pivot = new WhsLoadPkgPackagePivot(load, handlingUnitPackage);

			AssertNoExceptionThrown(() => { pivot.InsertAndReturnObject(TestConnection); });
			Assert("Insert succeeded.", WhsLoadPkgPackagePivot.ExistsInDB(TestConnection, pivot.PK));
		}

		#region Implementation

		(WhsLoad Load, PkgPackage Package) PreparePackageAndLoadWithoutStartTime()
		{
			// Prepare load
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("WH1", branch.PK).WithDockDoor(TestConnection);
			var areaDDL = new WhsArea(whs.PK, "DDLArea").AppendInsertAndReturnObject(sql);
			var rowDDL = new WhsRow(whs, "DDL").AppendInsertAndReturnObject(sql);
			var ddlLocationType = WhsLocationType.ShallowLoadFromDB(TestConnection, lt => lt.WLT_LocationClass == "DDL")[0];
			var dockDoorLocation = new WhsLocation(rowDDL.PK, areaDDL.PK, areaDDL.PK, ddlLocationType.PK).AppendInsertAndReturnObject(sql);

			var transportCompany = new OrgHeader("TRANSPORT").AppendInsertAndReturnObject(sql);
			var equipment1 = new RefEquipment("ABC", "ABC")
			{
				RQ_CubicCapacity = 1m,
				RQ_CubicUnit = "M3",
				RQ_WeightCapacity = 1m,
				RQ_WeightUnit = "KG"
			}.AppendInsertAndReturnObject(sql);

			var load = new WhsLoad("WL01", "STD", transportCompany.PK, dockDoorLocation.PK)
			{
				WLO_TransportationUnitNumber = "ABC",
				WLO_RQ_TransportationUnit = equipment1.PK
			}.AppendInsertAndReturnObject(sql);

			// Prepare PkgPackage
			var job = new PkgPackageJob(Guid.NewGuid(), "KJ_WD001", WhsDocketSchema.Constants.Prefix).AppendInsertAndReturnObject(sql);
			var package = new PkgPackage(job, "PLT", 1).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			return (load, package);
		}

		#endregion
	}
}

using System;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsLoadPkgPackagePivot_PreventUpdatingFK))]
	class TG_WhsLoadPkgPackagePivot_PreventUpdatingFKTest : DBCreateTriggerScriptTest
	{
		public void Test_TG_WhsLoadPkgPackagePivot_PreventUpdatingWLP_WLO_Load()
		{
			var (load1, package1, load2, _) = PreparePackageAndLoads();
			var pivot = new WhsLoadPkgPackagePivot(load1, package1).InsertAndReturnObject(TestConnection);

			AssertExceptionThrown<SqlException>(
				 message: "Updating WLP_WLO_Load should throw exception",
				 expectedExceptionMessage: @"Updating FK from WhsLoadPkgPackagePivot to WhsLoad or PkgPackage is not allowed.
The transaction ended in the trigger. The batch has been aborted.",
				 () => {
					 TestConnection.ExecuteNonQuery(
						 WhsLoadPkgPackagePivot.UpdateWhere(pivot.PK)
							 .Set(x => x.WLP_WLO_Load, load2)
							 .AsSQL());
				 });
		}

		public void Test_TG_WhsLoadPkgPackagePivot_PreventUpdatingWLP_KP_Package()
		{
			var (load1, package1, _, package2) = PreparePackageAndLoads();
			var pivot = new WhsLoadPkgPackagePivot(load1, package1).InsertAndReturnObject(TestConnection);

			AssertExceptionThrown<SqlException>(
				 message: "Updating WLP_KP_Package should throw exception",
				 expectedExceptionMessage: @"Updating FK from WhsLoadPkgPackagePivot to WhsLoad or PkgPackage is not allowed.
The transaction ended in the trigger. The batch has been aborted.",
				 () =>
				 {
					 TestConnection.ExecuteNonQuery(
					 WhsLoadPkgPackagePivot.UpdateWhere(pivot.PK)
						 .Set(x => x.WLP_KP_Package, package2)
						 .AsSQL());
				 });
		}

		public void Test_TG_WhsLoadPkgPackagePivot_AllowUpdateOtherFieldsOrFKWithoutChange()
		{
			var (load1, package1, _, _) = PreparePackageAndLoads();
			var pivot = new WhsLoadPkgPackagePivot(load1, package1).InsertAndReturnObject(TestConnection);

			AssertNoExceptionThrown(
				message: "Updating fields other than the FKs should be allowed. Updating to FK without value change is allowed.",
				() => {
					WhsLoadPkgPackagePivot.UpdateWhere(pivot.PK)
						.Set(x => x.WLP_WLO_Load, pivot.WLP_WLO_Load)
						.Set(x => x.WLP_LoadedTime, DateTimeOffset.Now)
						.Set(x => x.WLP_GS_NKLoadingUser, "123")
						.Post(TestConnection);
				});

			WhsLoadPkgPackagePivot.AssertFromDB(TestConnection, pivot.PK)
				.ExpectEquals("Update succeeded.", x => x.WLP_GS_NKLoadingUser, "123")
				.VerifyAll();
		}

		#region Implementation

		(WhsLoad load1, PkgPackage package1, WhsLoad load2, PkgPackage package2) PreparePackageAndLoads()
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

			var load1 = new WhsLoad("WL01", "STD", transportCompany.PK, dockDoorLocation.PK)
			{
				WLO_StartTime = DateTime.Now,
				WLO_TransportationUnitNumber = "ABC",
				WLO_RQ_TransportationUnit = equipment1.PK
			}.AppendInsertAndReturnObject(sql);

			var load2 = new WhsLoad("WL02", "STD", transportCompany.PK, dockDoorLocation.PK)
			{
				WLO_StartTime = DateTime.Now,
				WLO_TransportationUnitNumber = "ABC",
				WLO_RQ_TransportationUnit = equipment1.PK
			}.AppendInsertAndReturnObject(sql);

			// Prepare PkgPackage
			var job = new PkgPackageJob(Guid.NewGuid(), parentTableCode: "WD").AppendInsertAndReturnObject(sql);
			var package1 = new PkgPackage(job, "PLT", 1).AppendInsertAndReturnObject(sql);
			var package2 = new PkgPackage(job, "PLT", 2).AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			return (load1, package1, load2, package2);
		}

		#endregion

	}
}

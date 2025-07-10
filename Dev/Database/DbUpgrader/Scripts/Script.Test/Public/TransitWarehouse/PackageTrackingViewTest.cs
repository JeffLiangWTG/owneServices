using System;
using System.Linq;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse.PackageTrackingView))]
	class PackageTrackingViewTest : DbCreateScriptTest
	{
		const string EnsureChildPackagesHaveSameTopLevelHandlingUnit = "dbo.TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit";
		static readonly string PkgPackageTableNameForSQL = TransitWarehouseTestHelper.TableDescriptor(PkgPackageSchema.Instance);

		DateTime GetDateTimeAddDaysWithoutMillisecond(int days)
		{
			var afterAddDays = DateTime.Now.AddDays(days);
			return new DateTime(afterAddDays.Year, afterAddDays.Month, afterAddDays.Day, 0, 0, 0);
		}

		public void TestView_PackageWithID()
		{
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageTableNameForSQL, TestConnection))
			{
				var time1 = GetDateTimeAddDaysWithoutMillisecond(0);
				var sql = new SqlQueryBuilder();
				var branch = new GlbBranch("BR1").InsertAndReturnObject(TestConnection);
				var orgHeader = new OrgHeader("TST").InsertAndReturnObject(TestConnection);
				var orgAddress = new OrgAddress(orgHeader, "TST", "1 TST ST").InsertAndReturnObject(TestConnection);
				var whs = new WhsWarehouse("TR1", "TRW", branch.PK, orgAddress.PK).WithDockDoor(TestConnection);
				var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
				var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
				var locationA = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

				var rcn = new WhsItemReceiveConsignmentOld_V02(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
				var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA, "rtu").AppendInsertAndReturnObject(sql);
				var dcn = new WhsItemDispatchConsignment(whs, "DC000001", "DC000001", "STD").AppendInsertAndReturnObject(sql);
				var dll = new WhsItemDispatchLoadList("dll1", whs).AppendInsertAndReturnObject(sql);
				var airDLL = new WhsItemDispatchLoadList("dll2", whs) { WDL_TransportMode = "AIR" }.AppendInsertAndReturnObject(sql);
				var dtu = new WhsItemDispatchTransportationUnit(whs, "dtu1").AppendInsertAndReturnObject(sql);
				var packageJob = new PkgPackageJob(rcn.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

				var unloadedTime = new DateTimeOffset(GetDateTimeAddDaysWithoutMillisecond(0), new TimeSpan(10, 0, 0));
				var loadedTime = new DateTimeOffset(GetDateTimeAddDaysWithoutMillisecond(1), new TimeSpan(10, 0, 0));

				var expiredUnloadedTime = new DateTimeOffset(GetDateTimeAddDaysWithoutMillisecond(-93), new TimeSpan(10, 0, 0));
				var expiredLoadedTime = new DateTimeOffset(GetDateTimeAddDaysWithoutMillisecond(-92), new TimeSpan(10, 0, 0));

				var expiredAirUnloadedTime = new DateTimeOffset(GetDateTimeAddDaysWithoutMillisecond(-5), new TimeSpan(10, 0, 0));
				var expiredAirLoadedTime = new DateTimeOffset(GetDateTimeAddDaysWithoutMillisecond(-4), new TimeSpan(10, 0, 0));

				var packageHeader = new PkgPackageHeader("P1", DateTime.UtcNow, "A").AppendInsertAndReturnObject(sql);
				var package = new PkgPackage(packageJob, packageHeader, "PLT", 1)
				{
					KP_SystemCreateTimeUtc = time1,
					KP_Weight = 1,
					KP_WeightUQ = "KG",
					KP_Length = 2,
					KP_Width = 3,
					KP_Height = 4,
					KP_DimensionUQ = "M"
				}.AppendInsertAndReturnObject(sql);
				var packageState = new WhsItemPackageState(package.PK, whs, rcn.PK, "FIN")
				{
					WPS_WRH_TransitReceiveHeader = rtu.PK,
					WPS_WL_LastLocation = locationA.PK,
					WPS_UnloadedTime = unloadedTime,
					WPS_UnloadedNotYetProcessedTime = unloadedTime,
					WPS_ReceivedAs = "SCN",
					WPS_WDC_TransitDispatchConsignment = dcn.PK,
					WPS_WDL_LoadList = dll.PK,
					WPS_WDH_TransitDispatchHeader = dtu.PK,
					WPS_LoadedTime = loadedTime,
					WPS_SystemLastEditTimeUtc = time1
				}.AppendInsertAndReturnObject(sql);

				var expiredPackageHeader = new PkgPackageHeader("Expired", DateTime.UtcNow, "A").AppendInsertAndReturnObject(sql);
				var expiredPackage = new PkgPackage(packageJob, expiredPackageHeader, "PLT", 1).AppendInsertAndReturnObject(sql);
				var expiredPackageState = new WhsItemPackageState(expiredPackage.PK, whs, rcn.PK, "FIN")
				{
					WPS_WRH_TransitReceiveHeader = rtu.PK,
					WPS_WL_LastLocation = locationA.PK,
					WPS_UnloadedTime = expiredUnloadedTime,
					WPS_UnloadedNotYetProcessedTime = expiredUnloadedTime,
					WPS_ReceivedAs = "SCN",
					WPS_WDC_TransitDispatchConsignment = dcn.PK,
					WPS_WDL_LoadList = dll.PK,
					WPS_WDH_TransitDispatchHeader = dtu.PK,
					WPS_LoadedTime = expiredLoadedTime,
					WPS_SystemLastEditTimeUtc = time1
				}.AppendInsertAndReturnObject(sql);

				var expiredAirPackageHeader = new PkgPackageHeader("P2", DateTime.UtcNow, "A").AppendInsertAndReturnObject(sql);
				var expiredAirPackage = new PkgPackage(packageJob, expiredAirPackageHeader, "PLT", 1).AppendInsertAndReturnObject(sql);
				var expiredAirPackageState = new WhsItemPackageState(expiredAirPackage.PK, whs, rcn.PK, "FIN")
				{
					WPS_WRH_TransitReceiveHeader = rtu.PK,
					WPS_WL_LastLocation = locationA.PK,
					WPS_UnloadedTime = expiredAirUnloadedTime,
					WPS_UnloadedNotYetProcessedTime = expiredAirUnloadedTime,
					WPS_ReceivedAs = "SCN",
					WPS_WDC_TransitDispatchConsignment = dcn.PK,
					WPS_WDL_LoadList = airDLL.PK,
					WPS_WDH_TransitDispatchHeader = dtu.PK,
					WPS_LoadedTime = expiredAirLoadedTime,
					WPS_SystemLastEditTimeUtc = time1
				}.AppendInsertAndReturnObject(sql);

				var packageHeader_uld = new PkgPackageHeader("ULD1", DateTime.UtcNow, "A").AppendInsertAndReturnObject(sql);
				var package_uld = new PkgPackage(packageJob, packageHeader_uld, "PLT", 1)
				{
					KP_SystemCreateTimeUtc = time1,
					KP_Weight = 1,
					KP_WeightUQ = "KG",
					KP_Length = 2,
					KP_Width = 3,
					KP_Height = 4,
					KP_DimensionUQ = "M"
				}.AppendInsertAndReturnObject(sql);
				var packageState_uld = new WhsItemPackageState(package_uld.PK, whs, null, "DEP")
				{
					WPS_UnitType = "ULD",
					WPS_WRH_TransitReceiveHeader = rtu.PK,
					WPS_WL_LastLocation = locationA.PK,
					WPS_UnloadedTime = unloadedTime,
					WPS_UnloadedNotYetProcessedTime = unloadedTime,
					WPS_ReceivedAs = "SCN",
					WPS_IsHandlingUnit = true,
					WPS_WDL_LoadList = dll.PK,
					WPS_WDH_TransitDispatchHeader = dtu.PK,
					WPS_LoadedTime = loadedTime,
					WPS_SystemLastEditTimeUtc = time1
				}.AppendInsertAndReturnObject(sql);
				var dtu_uld = new WhsItemDispatchTransportationUnit(whs, "ULD1", "ULD").AppendInsertAndReturnObject(sql);
				var packageExtension = new PkgPackageExtension(dtu_uld.PK, "WDH", package_uld.PK).AppendInsertAndReturnObject(sql);

				var dll_uld = new WhsItemDispatchLoadList("dll_uld", whs).AppendInsertAndReturnObject(sql);
				TestDataCreator.CreateCusEntryNum(dll_uld.PK, "WhsItemDispatchLoadList", "MB001", "MAB", "OTH", "");
				var packageHeader_Inner = new PkgPackageHeader("ULD_P1", DateTime.UtcNow, "A").AppendInsertAndReturnObject(sql);
				var package_Inner = new PkgPackage(packageJob, packageHeader_Inner, "PLT", 1)
				{
					KP_KP_TopHandlingUnitPackage = package_uld,
					KP_SystemCreateTimeUtc = time1,
					KP_Weight = 1,
					KP_WeightUQ = "KG",
					KP_Length = 2,
					KP_Width = 3,
					KP_Height = 4,
					KP_DimensionUQ = "M"
				}.AppendInsertAndReturnObject(sql);
				var packageState_Inner = new WhsItemPackageState(package_Inner.PK, whs, rcn.PK, "DEP")
				{
					WPS_WRH_TransitReceiveHeader = rtu.PK,
					WPS_WL_LastLocation = locationA.PK,
					WPS_UnloadedTime = unloadedTime,
					WPS_UnloadedNotYetProcessedTime = unloadedTime,
					WPS_ReceivedAs = "SCN",
					WPS_WDC_TransitDispatchConsignment = dcn.PK,
					WPS_WDL_LoadList = dll_uld.PK,
					WPS_WDH_TransitDispatchHeader = dtu_uld.PK,
					WPS_LoadedTime = loadedTime,
					WPS_SystemLastEditTimeUtc = time1
				}.AppendInsertAndReturnObject(sql);

				TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				var views = PackageTrackingView.ShallowLoadFromDB(TestConnection);
				AssertEquals(3, views.Length);

				var view1 = PackageTrackingView.ShallowLoadFromDB(TestConnection, v => v.PK == package.PK).First();
				AssertView(view1, packageHeader.PK, package.PK, orgAddress.PK, "P1", loadedTime, 1, "KG", 2, 3, 4, "M", string.Empty);

				var view2 = PackageTrackingView.ShallowLoadFromDB(TestConnection, v => v.PK == package_uld.PK).First();
				AssertView(view2, packageHeader_uld.PK, package_uld.PK, orgAddress.PK, "ULD1", loadedTime, 1, "KG", 2, 3, 4, "M", "MB001");

				var view3 = PackageTrackingView.ShallowLoadFromDB(TestConnection, v => v.PK == package_Inner.PK).First();
				AssertView(view3, packageHeader_Inner.PK, package_Inner.PK, orgAddress.PK, "ULD_P1", loadedTime, 1, "KG", 2, 3, 4, "M", string.Empty);
			}
		}

		public void TestView_MultiplePackagesWithTimeConvert()
		{
			var time1 = GetDateTimeAddDaysWithoutMillisecond(0);
			var time1InDateTimeOffset = new DateTimeOffset(GetDateTimeAddDaysWithoutMillisecond(0), TimeSpan.FromMinutes(480));
			var sql = new SqlQueryBuilder();
			var branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "CNNJG" }.InsertAndReturnObject(TestConnection);
			var orgHeader = new OrgHeader("TST").InsertAndReturnObject(TestConnection);
			var orgAddress = new OrgAddress(orgHeader, "TST", "1 TST ST").InsertAndReturnObject(TestConnection);
			var whs = new WhsWarehouse("TR1", "TRW", branch.PK, orgAddress.PK).WithDockDoor(TestConnection);
			var area = new WhsArea(whs.PK, "AREA1").AppendInsertAndReturnObject(sql);
			var row = new WhsRow(whs, "A").AppendInsertAndReturnObject(sql);
			var locationA = new WhsLocation(row.PK, area.PK, area.PK).AppendInsertAndReturnObject(sql);

			var rcn1 = new WhsItemReceiveConsignmentOld_V02(whs, "RC000001", "RC000001", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var packageJob = new PkgPackageJob(rcn1.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000001" }.AppendInsertAndReturnObject(sql);

			var rcn2 = new WhsItemReceiveConsignmentOld_V02(whs, "RC000002", "RC000002", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var packageJob2 = new PkgPackageJob(rcn2.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000002" }.AppendInsertAndReturnObject(sql);

			var rcn3 = new WhsItemReceiveConsignmentOld_V02(whs, "RC000003", "RC000003", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var packageJob3 = new PkgPackageJob(rcn3.PK) { KJ_ParentTableCode = "WRC", KJ_JobID = "P000003" }.AppendInsertAndReturnObject(sql);

			var rtu = new WhsItemReceiveTransportationUnit(whs, "rtu", locationA, "rtu").AppendInsertAndReturnObject(sql);
			var dcn = new WhsItemDispatchConsignment(whs, "DC000001", "DC000001", "STD").AppendInsertAndReturnObject(sql);
			var dll = new WhsItemDispatchLoadList("dll1", whs).AppendInsertAndReturnObject(sql);
			var dtu = new WhsItemDispatchTransportationUnit(whs, "dtu1").AppendInsertAndReturnObject(sql);
			var unloadedTime = new DateTimeOffset(GetDateTimeAddDaysWithoutMillisecond(0), new TimeSpan(10, 0, 0));
			var loadedTime = new DateTimeOffset(GetDateTimeAddDaysWithoutMillisecond(1), new TimeSpan(10, 0, 0));

			var unloadedTime2 = new DateTimeOffset(GetDateTimeAddDaysWithoutMillisecond(1), new TimeSpan(10, 0, 0));
			var loadedTime2 = new DateTimeOffset(GetDateTimeAddDaysWithoutMillisecond(2), new TimeSpan(10, 0, 0));

			var packageHeader = new PkgPackageHeader("P1", DateTime.UtcNow, "A").AppendInsertAndReturnObject(sql);

			var package_Finalized = new PkgPackage(packageJob, packageHeader, "PLT", 1)
			{
				KP_SystemCreateTimeUtc = time1,
			}.AppendInsertAndReturnObject(sql);
			var packageState_Finalized = new WhsItemPackageState(package_Finalized.PK, whs, rcn1.PK, "FIN")
			{
				WPS_WRH_TransitReceiveHeader = rtu.PK,
				WPS_WL_LastLocation = locationA.PK,
				WPS_UnloadedTime = unloadedTime,
				WPS_UnloadedNotYetProcessedTime = unloadedTime,
				WPS_ReceivedAs = "SCN",
				WPS_WDC_TransitDispatchConsignment = dcn.PK,
				WPS_WDL_LoadList = dll.PK,
				WPS_WDH_TransitDispatchHeader = dtu.PK,
				WPS_LoadedTime = loadedTime,
				WPS_SystemLastEditTimeUtc = time1
			}.AppendInsertAndReturnObject(sql);

			var package_Departed = new PkgPackage(packageJob2, packageHeader, "PLT", 1)
			{
				KP_SystemCreateTimeUtc = time1,
			}.AppendInsertAndReturnObject(sql);
			var packageState_Departed = new WhsItemPackageState(package_Departed.PK, whs, rcn2.PK, "DEP")
			{
				WPS_WRH_TransitReceiveHeader = rtu.PK,
				WPS_WL_LastLocation = locationA.PK,
				WPS_UnloadedTime = unloadedTime2,
				WPS_UnloadedNotYetProcessedTime = unloadedTime,
				WPS_ReceivedAs = "SCN",
				WPS_WDC_TransitDispatchConsignment = dcn.PK,
				WPS_WDL_LoadList = dll.PK,
				WPS_WDH_TransitDispatchHeader = dtu.PK,
				WPS_LoadedTime = loadedTime2,
				WPS_SystemLastEditTimeUtc = time1
			}.AppendInsertAndReturnObject(sql);

			var pivot = new PkgPackageJobPackageHeaderPivot(packageJob3, packageHeader)
			{
				KPJ_SystemCreateTimeUtc = time1,
				KPJ_SystemLastEditTimeUtc = DateTime.UtcNow,
				KPJ_SystemLastEditUser = "A",
				KPJ_SystemCreateUser = "A"
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var views = PackageTrackingView.ShallowLoadFromDB(TestConnection);
			AssertEquals(2, views.Length);

			var view1 = PackageTrackingView.ShallowLoadFromDB(TestConnection, v => v.PK == package_Finalized.PK).First();
			AssertView(view1, packageHeader.PK, package_Finalized.PK, orgAddress.PK, "P1", loadedTime, 0, "", 0, 0, 0, "", "");

			var view2 = PackageTrackingView.ShallowLoadFromDB(TestConnection, v => v.PK == package_Departed.PK).First();
			AssertView(view2, packageHeader.PK, package_Departed.PK, orgAddress.PK, "P1", loadedTime2, 0, "", 0, 0, 0, "", "");
		}

		void AssertView(PackageTrackingView view, Guid packageHeaderPK, Guid? packagePK, Guid? addressPK, string packageID, DateTimeOffset? trackedTo, decimal weight, string weightUQ, decimal length, decimal width, decimal height, string dimensionUQ, string masterbill)
		{
			AssertNotNull(view);
			AssertEquals(packageHeaderPK, view.PTV_KPH_PackageHeaderFK.FK);
			if (view.PTV_KP_Package != null)
			{
				AssertEquals(packagePK, view.PTV_KP_Package.FK);
			}
			else
			{
				AssertNull(view.PTV_KP_Package);
			}

			if (view.PTV_OA_Address != null)
			{
				AssertEquals(addressPK, view.PTV_OA_Address.FK);
			}
			else
			{
				AssertNull(view.PTV_OA_Address);
			}

			AssertEquals(packageID, view.PTV_KPH_PackageID);
			AssertEquals(trackedTo, view.PTV_TrackedTo);
			AssertEquals(weight, view.PTV_Weight);
			AssertEquals(weightUQ, view.PTV_WeightUQ);
			AssertEquals(length, view.PTV_Length);
			AssertEquals(width, view.PTV_Width);
			AssertEquals(height, view.PTV_Height);
			AssertEquals(dimensionUQ, view.PTV_DimensionUQ);
			AssertEquals(masterbill, view.PTV_MasterBill);
		}

		protected override bool RequiresSchemaBinding => false;
	}
}

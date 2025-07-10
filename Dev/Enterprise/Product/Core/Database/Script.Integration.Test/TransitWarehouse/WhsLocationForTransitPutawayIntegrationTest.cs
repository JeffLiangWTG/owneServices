using System;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	class WhsLocationForTransitPutawayIntegrationTest : TransactionedTestCase
	{
		#region TestView

		public void TestView()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertCountOfLocations(1);
			AssertLocationRecord(location, rnoType, putawayArea, pickingArea, whs.PK, "R1");
		}

		#endregion

		#region TestView_AreaTypes

		public void TestView_AreaTypes()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A") { WA_AreaType = "BON" }.AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P") { WA_AreaType = "FRE" }.AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertCountOfLocations(1);
			AssertLocationRecord(location, rnoType, putawayArea, pickingArea, whs.PK, "R1");
		}

		#endregion

		#region TestView_PalletSpaces

		public void TestView_PalletSpaces()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "DAM",
				WL_PalletFloorSpaces = 3,
				WL_PalletStackHeight = 4,
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertCountOfLocations(1);
			AssertLocationRecord(location, rnoType, putawayArea, pickingArea, whs.PK, "R1");
		}

		#endregion

		#region TestView_PalletSpaces_Large

		public void TestView_PalletSpaces_Large()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "DAM",
				WL_PalletFloorSpaces = 254,
				WL_PalletStackHeight = 254,
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertCountOfLocations(1);
			AssertLocationRecord(location, rnoType, putawayArea, pickingArea, whs.PK, "R1");
		}

		#endregion

		#region TestView_WithMaxWeight

		public void TestView_WithMaxWeight()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "DAM",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "DT",
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertCountOfLocations(1);
			AssertLocationRecord(location, rnoType, putawayArea, pickingArea, whs.PK, "R1", availableWeight: 4m);
		}

		#endregion

		#region TestView_WithMaxCubic

		public void TestView_WithMaxCubic()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "DAM",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "CF",
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertCountOfLocations(1);
			AssertLocationRecord(location, rnoType, putawayArea, pickingArea, whs.PK, "R1", availableVolume: 5m);
		}

		#endregion

		#region TestView_WithMaxDimensions

		public void TestView_WithMaxDimensions()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "DAM",
				WL_MaxWidth = 7,
				WL_MaxHeight = 8,
				WL_MaxDepth = 9,
				WL_MaxDimensionUnit = "M",
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertCountOfLocations(1);
			AssertLocationRecord(location, rnoType, putawayArea, pickingArea, whs.PK, "R1");
		}

		#endregion

		#region TestView_TransitDischargeFields

		public void TestView_TransitDischargeFields()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "DAM",
				WL_TransitDischargeLRC = "CN",
				WL_RS_NKTransitServiceLevel = "D2D",
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertCountOfLocations(1);
			AssertLocationRecord(location, rnoType, putawayArea, pickingArea, whs.PK, "R1");
		}

		#endregion

		#region TestView_TemperatureControlled

		public void TestView_TemperatureControlled()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var tclType = new WhsLocationType("TCL")
			{
				WLT_LocationClass = "TCL",
				WLT_MaximumTemperature = 5.0m,
				WLT_MinimumTemperature = 2.0m,
				WLT_TemperatureUnit = "F",
				WLT_DefaultCycleCountGranularity = ""
			}.AppendInsertAndReturnObject(sql);

			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, tclType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertCountOfLocations(1);
			AssertLocationRecord(location, tclType, putawayArea, pickingArea, whs.PK, "R1");
		}

		#endregion

		#region TestView_WithPackage

		public void TestView_WithPackage() => TestView_WithPackage(isPallet: false);
		public void TestView_WithPackage_Pallet() => TestView_WithPackage(isPallet: true);

		void TestView_WithPackage(bool isPallet)
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RCN123", "RCN123", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var packageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", location, "VEH123").AppendInsertAndReturnObject(sql);

			var package = new PkgPackage(packageJob, isPallet ? "PLT" : "BOX", 1)
			{
				KP_Weight = 1m,
				KP_WeightUQ = "KG",
				KP_Volume = 2m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var packageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package.PK, "PUT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location);

			Db.Connection.ExecuteNonQuery(sql.ToString());

			AssertCountOfLocations(1);
			AssertLocationRecord(
				location,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: isPallet ? 1 : 0,
				isEmpty: false,
				availableWeight: 4m - 1m,
				availableVolume: 5m - 2m);
		}

		#endregion

		#region TestView_WithPackage_HandlingUnit

		public void TestView_WithPackage_HandlingUnit()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "TRW", branchPK: branch.PK).WithDockDoor(sql);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RCN123", "RCN123", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var packageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", location, "VEH123").AppendInsertAndReturnObject(sql);

			var handlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
			var handlingUnitPackageJob = new PkgPackageJob(handlingUnit.PK, parentTableCode: "KPU") { KJ_JobID = "HU" }.AppendInsertAndReturnObject(sql);

			var handlingUnitPackage = new PkgPackage(handlingUnitPackageJob, "BOX", 1)
			{
				KP_Weight = 3m,
				KP_WeightUQ = "KG",
				KP_Volume = 3m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var handlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, handlingUnitPackage.PK, "PUT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location, isHandlingUnit: true);

			var package = new PkgPackage(packageJob, handlingUnitPackage, "BOX", 1)
			{
				KP_Weight = 1m,
				KP_WeightUQ = "KG",
				KP_Volume = 2m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var packageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package.PK, "PUT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location);

			var divot1 = new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, package.PK).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageSchema.Constants.TableName, TestConnection))
			{
				Db.Connection.ExecuteNonQuery(sql.ToString());
			}

			AssertCountOfLocations(1);
			AssertLocationRecord(
				location,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: 0,
				isEmpty: false,
				availableWeight: 4m - 3m,
				availableVolume: 5m - 3m);
		}

		#endregion

		#region TestView_WithPackage_HandlingUnit_PalletQuantity

		public void TestView_WithPackage_HandlingUnit_PalletQuantity_PalletHU()
		{
			TestView_WithPackage_HandlingUnit_PalletQuantity(huPackType: "PLT", package1PackType: "BOX", package2PackType: "BOX", expectPalletQuantity: true);
		}

		public void TestView_WithPackage_HandlingUnit_PalletQuantity_BoxHU()
		{
			// It's not really valid to have a pallet in a box/any pack type for putaway, but make sure we handle it consistently
			TestView_WithPackage_HandlingUnit_PalletQuantity(huPackType: "BOX", package1PackType: "PLT", package2PackType: "PLT", expectPalletQuantity: false);
		}

		public void TestView_WithPackage_HandlingUnit_PalletQuantity_AllPallets()
		{
			// It's not really valid to have a pallet in a pallet/any pack type for putaway, but make sure we handle it consistently
			TestView_WithPackage_HandlingUnit_PalletQuantity(huPackType: "PLT", package1PackType: "PLT", package2PackType: "PLT", expectPalletQuantity: true);
		}

		void TestView_WithPackage_HandlingUnit_PalletQuantity(string huPackType, string package1PackType, string package2PackType, bool expectPalletQuantity)
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "TRW", branchPK: branch.PK).WithDockDoor(sql);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
			}.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RCN123", "RCN123", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var packageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", location, "VEH123").AppendInsertAndReturnObject(sql);

			var handlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
			var handlingUnitPackageJob = new PkgPackageJob(handlingUnit.PK, parentTableCode: "KPU") { KJ_JobID = "HU" }.AppendInsertAndReturnObject(sql);

			var handlingUnitPackage = new PkgPackage(handlingUnitPackageJob, huPackType, 1).AppendInsertAndReturnObject(sql);

			var handlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, handlingUnitPackage.PK, "PUT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location, isHandlingUnit: true);

			var package1 = new PkgPackage(packageJob, handlingUnitPackage, package1PackType, 1).AppendInsertAndReturnObject(sql);

			var packageState1 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, "PUT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location);

			var divot1 = new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, package1.PK).AppendInsertAndReturnObject(sql);

			var package2 = new PkgPackage(packageJob, handlingUnitPackage, package2PackType, 1).AppendInsertAndReturnObject(sql);

			var packageState2 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package2.PK, "PUT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location);

			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageSchema.Constants.TableName, TestConnection))
			{
				Db.Connection.ExecuteNonQuery(sql.ToString());
			}

			AssertCountOfLocations(1);
			AssertLocationRecord(
				location,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: expectPalletQuantity ? 1 : 0,
				isEmpty: false);
		}

		#endregion

		#region TestView_WithPackage_HandlingUnit_MultiLevel

		public void TestView_WithPackage_HandlingUnit_MultiLevel() => TestView_WithPackage_HandlingUnit_MultiLevel(outerIsAPallet: false);
		public void TestView_WithPackage_HandlingUnit_MultiLevel_OuterPallet() => TestView_WithPackage_HandlingUnit_MultiLevel(outerIsAPallet: true);

		void TestView_WithPackage_HandlingUnit_MultiLevel(bool outerIsAPallet)
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "TRW", branchPK: branch.PK).WithDockDoor(sql);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RCN123", "RCN123", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var packageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", location, "VEH123").AppendInsertAndReturnObject(sql);

			var topHandlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
			var topHandlingUnitPackageJob = new PkgPackageJob(topHandlingUnit.PK, parentTableCode: "KPU") { KJ_JobID = "TOP" }.AppendInsertAndReturnObject(sql);

			var topHandlingUnitPackage = new PkgPackage(topHandlingUnitPackageJob, outerIsAPallet ? "PLT" : "BOX", 1)
			{
				KP_Weight = 3m,
				KP_WeightUQ = "KG",
				KP_Volume = 3m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var topHandlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, topHandlingUnitPackage.PK, "PUT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location, isHandlingUnit: true);

			var secondLevelHandlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
			var secondLevelHandlingUnitPackageJob = new PkgPackageJob(secondLevelHandlingUnit.PK, parentTableCode: "KPU") { KJ_JobID = "2ND" }.AppendInsertAndReturnObject(sql);

			var secondLevelHandlingUnitPackage = new PkgPackage(secondLevelHandlingUnitPackageJob, topHandlingUnitPackage, "PLT", 1)
			{
				KP_Weight = .1m,
				KP_WeightUQ = "KG",
				KP_Volume = .2m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var secondLevelHandlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, secondLevelHandlingUnitPackage.PK, "PUT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location, isHandlingUnit: true);

			var package = new PkgPackage(packageJob, topHandlingUnitPackage, "PLT", 1)
			{
				KP_Weight = 1m,
				KP_WeightUQ = "KG",
				KP_Volume = 2m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var packageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package.PK, "PUT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location);

			var divot1 = new PkgPackageHandlingUnitDivot(secondLevelHandlingUnitPackage.PK, package.PK).AppendInsertAndReturnObject(sql);
			var divot2 = new PkgPackageHandlingUnitDivot(topHandlingUnitPackage.PK, secondLevelHandlingUnitPackage.PK).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageSchema.Constants.TableName, TestConnection))
			{
				Db.Connection.ExecuteNonQuery(sql.ToString());
			}

			AssertCountOfLocations(1);
			AssertLocationRecord(
				location,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: outerIsAPallet ? 1 : 0,
				isEmpty: false,
				availableWeight: 4m - 3m,
				availableVolume: 5m - 3m);
		}

		#endregion

		#region TestView_WithPackage_HandlingUnit_Unpacked

		public void TestView_WithPackage_HandlingUnit_Unpacked()
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "TRW", branchPK: branch.PK).WithDockDoor(sql);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RCN123", "RCN123", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var packageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", location, "VEH123").AppendInsertAndReturnObject(sql);

			var handlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
			var handlingUnitPackageJob = new PkgPackageJob(handlingUnit.PK, parentTableCode: "KPU") { KJ_JobID = "HU" }.AppendInsertAndReturnObject(sql);

			var handlingUnitPackage = new PkgPackage(handlingUnitPackageJob, "PLT", 1)
			{
				KP_Weight = 1m,
				KP_WeightUQ = "KG",
				KP_Volume = 2m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var handlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, handlingUnitPackage.PK, "PUT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location, isHandlingUnit: true);

			var package = new PkgPackage(packageJob, handlingUnitPackage, "BOX", 1)
			{
				KP_Weight = 1m,
				KP_WeightUQ = "KG",
				KP_Volume = 2m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			handlingUnitPackage.KP_Weight = handlingUnitPackage.KP_Weight - package.KP_Weight;
			handlingUnitPackage.KP_Volume = handlingUnitPackage.KP_Volume - package.KP_Volume;
			package.KP_KP_TopHandlingUnitPackage = null;

			var packageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package.PK, "PUT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location);

			var unpackedDivot = new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, package.PK)
			{
				KPD_UnpackedTime = DateTime.UtcNow,
				KPD_GS_NKUnpackedUser = "~BP",
			}.AppendInsertAndReturnObject(sql);

			var divot = new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, package.PK).AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageSchema.Constants.TableName, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureTopLevelHandlingUnitPackageWhenInsertDivot, PkgPackageHandlingUnitDivotSchema.Constants.TableName, TestConnection))
			{
				Db.Connection.ExecuteNonQuery(sql.ToString());
			}
			AssertCountOfLocations(1);
			AssertLocationRecord(
				location,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: 1,
				isEmpty: false,
				availableWeight: 4m - 1m,
				availableVolume: 5m - 2m);
		}

		#endregion

		#region TestView_WithPackage_NoCapacities

		public void TestView_WithPackage_NoCapacities()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
			}.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RCN123", "RCN123", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var packageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", location, "VEH123").AppendInsertAndReturnObject(sql);

			var package = new PkgPackage(packageJob, "BOX", 1)
			{
				KP_Weight = 1m,
				KP_WeightUQ = "KG",
				KP_Volume = 2m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var packageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package.PK, "PUT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location);

			Db.Connection.ExecuteNonQuery(sql.ToString());

			AssertCountOfLocations(1);
			AssertLocationRecord(
				location,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: 0,
				isEmpty: false,
				availableWeight: 0m,
				availableVolume: 0m);
		}

		#endregion

		#region TestView_WithPackage_RequiresUnitConversions

		public void TestView_WithPackage_RequiresUnitConversions()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RCN123", "RCN123", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var packageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", location, "VEH123").AppendInsertAndReturnObject(sql);

			var package = new PkgPackage(packageJob, "BOX", 1)
			{
				KP_Weight = 1000m,
				KP_WeightUQ = "G",
				KP_Volume = 35.3147m,
				KP_VolumeUQ = "CF",
			}.AppendInsertAndReturnObject(sql);

			var packageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package.PK, "PUT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location);

			Db.Connection.ExecuteNonQuery(sql.ToString());

			AssertCountOfLocations(1);
			AssertLocationRecord(
				location,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: 0,
				isEmpty: false,
				availableWeight: 4m - 1m,
				availableVolume: 5m - 1m);
		}

		#endregion

		#region TestView_WithPackages

		public void TestView_WithPackages()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RCN123", "RCN123", "STD", "AUSYD").AppendInsertAndReturnObject(sql);
			var packageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", location, "VEH123").AppendInsertAndReturnObject(sql);

			var package1 = new PkgPackage(packageJob, "BOX", 1)
			{
				KP_Weight = 1m,
				KP_WeightUQ = "KG",
				KP_Volume = 2m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var package2 = new PkgPackage(packageJob, "PLT", 1)
			{
				KP_Weight = .5m,
				KP_WeightUQ = "KG",
				KP_Volume = .5m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var packageState1 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, "PUT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location);

			var packageState2 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package2.PK, "PUT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: location, receiveLocation: location);

			Db.Connection.ExecuteNonQuery(sql.ToString());

			AssertCountOfLocations(1);
			AssertLocationRecord(
				location,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: 1,
				isEmpty: false,
				availableWeight: 4m - 1.5m,
				availableVolume: 5m - 2.5m);
		}

		#endregion

		#region TestView_FinalisedPackage

		public void TestView_FinalisedPackage()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC11", "RC11", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", location, "VEH123").AppendInsertAndReturnObject(sql);
			var pkgPackageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);
			var dispatchTransportationUnit = new WhsItemDispatchTransportationUnit(whs, "DH123").AppendInsertAndReturnObject(sql);
			var dispatchConsignment = new WhsItemDispatchConsignment(whs, "RDC123", "DC123", "EXP").AppendInsertAndReturnObject(sql);
			var dispatchLoadList = new WhsItemDispatchLoadList("LL123", whs).AppendInsertAndReturnObject(sql);

			var package1 = new PkgPackage(pkgPackageJob, "PLT", 1)
			{
				KP_Weight = 1m,
				KP_WeightUQ = "KG",
				KP_Volume = 2m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var packageState1 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, "FIN", whs, receiveConsignment, receiveTransportationUnit, dispatchLoadList, dcn: dispatchConsignment, dtu: dispatchTransportationUnit, lastLocation: location, receiveLocation: location);

			Db.Connection.ExecuteNonQuery(sql.ToString());

			AssertCountOfLocations(1);
			AssertLocationRecord(
				location,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: 0,
				isEmpty: true,
				availableWeight: 4m,
				availableVolume: 5m);
		}

		#endregion

		#region TestView_WithCommittedPackages

		public void TestView_WithCommittedPackages()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var toLocation = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 1,
				WL_Tray = 1,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var fromLocation = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC11", "RC11", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", toLocation, "VEH123").AppendInsertAndReturnObject(sql);
			var pkgPackageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);

			var package1 = new PkgPackage(pkgPackageJob, "PLT", 1)
			{
				KP_Weight = 1m,
				KP_WeightUQ = "KG",
				KP_Volume = 2m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var packageState1 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, "CTT", whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation);

			var transferHeader = new WhsItemTransferHeader("TransHA", whs) { WTH_TransferType = "TRF" }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsItemTransferLine(packageState1, fromLocation, toLocation, transferHeader)
			{
				WTF_GS_NKPickUser = "~BP",
				WTF_PickTime = null,
				WTF_PutTime = null,
			}.AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToString());

			AssertCountOfLocations(2);

			AssertLocationRecord(
				toLocation,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: 1,
				isEmpty: false,
				availableWeight: 4m - 1m,
				availableVolume: 5m - 2m);

			AssertLocationRecord(
				fromLocation,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: 1,
				isEmpty: false,
				availableWeight: 4m - 1m,
				availableVolume: 5m - 2m);
		}

		#endregion

		#region TestView_WithPickedPackages

		public void TestView_WithPickedPackages()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var toLocation = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 1,
				WL_Tray = 1,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var fromLocation = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC11", "RC11", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", toLocation, "VEH123").AppendInsertAndReturnObject(sql);
			var pkgPackageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);

			var package1 = new PkgPackage(pkgPackageJob, "PLT", 1)
			{
				KP_Weight = 1m,
				KP_WeightUQ = "KG",
				KP_Volume = 2m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var packageState1 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, "PIC", whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation);

			var transferHeader = new WhsItemTransferHeader("TransHA", whs) { WTH_TransferType = "TRF" }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsItemTransferLine(packageState1, fromLocation, toLocation, transferHeader)
			{
				WTF_GS_NKPickUser = "~BP",
				WTF_PickTime = DateTime.Today.AddDays(-1),
				WTF_PutTime = null,
			}.AppendInsertAndReturnObject(sql);

			Db.Connection.ExecuteNonQuery(sql.ToString());

			AssertCountOfLocations(2);

			AssertLocationRecord(
				toLocation,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: 1,
				isEmpty: false,
				availableWeight: 4m - 1m,
				availableVolume: 5m - 2m);

			AssertLocationRecord(
				fromLocation,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: 0,
				isEmpty: true,
				availableWeight: 4m,
				availableVolume: 5m);
		}

		#endregion

		#region TestView_WithPickedOrCommittedPackages_HandlingUnits

		public void TestView_WithCommittedPackages_HandlingUnits() => TestView_WithPickedOrCommittedPackages_HandlingUnits(picked: false);
		public void TestView_WithPickedPackages_HandlingUnits() => TestView_WithPickedOrCommittedPackages_HandlingUnits(picked: true);

		void TestView_WithPickedOrCommittedPackages_HandlingUnits(bool picked)
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "TRW", branchPK: branch.PK).WithDockDoor(sql);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var toLocation = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 1,
				WL_Tray = 1,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var fromLocation = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 8,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 10,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC11", "RC11", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", toLocation, "VEH123").AppendInsertAndReturnObject(sql);
			var pkgPackageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);

			var handlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
			var handlingUnitPackageJob = new PkgPackageJob(handlingUnit.PK, parentTableCode: "KPU") { KJ_JobID = "HU" }.AppendInsertAndReturnObject(sql);

			var handlingUnitPackage = new PkgPackage(handlingUnitPackageJob, "PLT", 1)
			{
				KP_Weight = 3m,
				KP_WeightUQ = "KG",
				KP_Volume = 2m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var status = picked ? "PIC" : "CTT";
			var handlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, handlingUnitPackage.PK, status, whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation, isHandlingUnit: true);

			var package1 = new PkgPackage(pkgPackageJob, handlingUnitPackage, "PLT", 1)
			{
				KP_Weight = 2m,
				KP_WeightUQ = "KG",
				KP_Volume = 1m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var package1PackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, status, whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation);

			var divot1 = new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, package1.PK).AppendInsertAndReturnObject(sql);

			var package2 = new PkgPackage(pkgPackageJob, handlingUnitPackage, "BOX", 1)
			{
				KP_Weight = .3m,
				KP_WeightUQ = "KG",
				KP_Volume = .4m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var package2PackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package2.PK, status, whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation);

			var divot2 = new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, package2.PK).AppendInsertAndReturnObject(sql);

			var transferHeader = new WhsItemTransferHeader("TransHA", whs) { WTH_TransferType = "TRF" }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsItemTransferLine(handlingUnitPackageState, fromLocation, toLocation, transferHeader)
			{
				WTF_GS_NKPickUser = "~BP",
				WTF_PickTime = picked ? DateTime.Today.AddDays(-1) : (DateTime?)null,
				WTF_PutTime = null,
			}.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageSchema.Constants.TableName, TestConnection))
			{
				Db.Connection.ExecuteNonQuery(sql.ToString());
			}

			AssertCountOfLocations(2);

			AssertLocationRecord(
				toLocation,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: 1,
				isEmpty: false,
				availableWeight: 4m - 3m,
				availableVolume: 5m - 2m);

			AssertLocationRecord(
				fromLocation,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: picked ? 0 : 1,
				isEmpty: picked,
				availableWeight: picked ? 8m : 8m - 3m,
				availableVolume: picked ? 10m : 10m - 2m);
		}

		#endregion

		#region TestView_WithPickedOrCommittedPackages_HandlingUnits_PalletQuantity

		public void TestView_WithCommittedPackages_HandlingUnits_PalletQuantity_PalletHU()
		{
			TestView_WithPickedOrCommittedPackages_HandlingUnits_PalletQuantity(picked: false, huPackType: "PLT", package1PackType: "BOX", package2PackType: "BOX", expectPalletQuantity: true);
		}

		public void TestView_WithCommittedPackages_HandlingUnits_PalletQuantity_BoxHU()
		{
			// It's not really valid to have a pallet in a box/any pack type for putaway, but make sure we handle it consistently
			TestView_WithPickedOrCommittedPackages_HandlingUnits_PalletQuantity(picked: false, huPackType: "BOX", package1PackType: "PLT", package2PackType: "PLT", expectPalletQuantity: false);
		}

		public void TestView_WithCommittedPackages_HandlingUnits_PalletQuantity_AllPallets()
		{
			// It's not really valid to have a pallet in a box/any pack type for putaway, but make sure we handle it consistently
			TestView_WithPickedOrCommittedPackages_HandlingUnits_PalletQuantity(picked: false, huPackType: "PLT", package1PackType: "PLT", package2PackType: "PLT", expectPalletQuantity: true);
		}

		public void TestView_WithPickedPackages_HandlingUnits_PalletQuantity_PalletHU()
		{
			TestView_WithPickedOrCommittedPackages_HandlingUnits_PalletQuantity(picked: true, huPackType: "PLT", package1PackType: "BOX", package2PackType: "BOX", expectPalletQuantity: true);
		}

		public void TestView_WithPickedPackages_HandlingUnits_PalletQuantity_BoxHU()
		{
			// It's not really valid to have a pallet in a box/any pack type for putaway, but make sure we handle it consistently
			TestView_WithPickedOrCommittedPackages_HandlingUnits_PalletQuantity(picked: true, huPackType: "BOX", package1PackType: "PLT", package2PackType: "PLT", expectPalletQuantity: false);
		}

		public void TestView_WithPickedPackages_HandlingUnits_PalletQuantity_AllPallets()
		{
			// It's not really valid to have a pallet in a box/any pack type for putaway, but make sure we handle it consistently
			TestView_WithPickedOrCommittedPackages_HandlingUnits_PalletQuantity(picked: true, huPackType: "PLT", package1PackType: "PLT", package2PackType: "PLT", expectPalletQuantity: true);
		}

		void TestView_WithPickedOrCommittedPackages_HandlingUnits_PalletQuantity(bool picked, string huPackType, string package1PackType, string package2PackType, bool expectPalletQuantity)
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "TRW", branchPK: branch.PK).WithDockDoor(sql);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var toLocation = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 1,
				WL_Tray = 1,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var fromLocation = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 8,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 10,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC11", "RC11", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", toLocation, "VEH123").AppendInsertAndReturnObject(sql);
			var pkgPackageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);

			var handlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
			var handlingUnitPackageJob = new PkgPackageJob(handlingUnit.PK, parentTableCode: "KPU") { KJ_JobID = "HU" }.AppendInsertAndReturnObject(sql);

			var handlingUnitPackage = new PkgPackage(handlingUnitPackageJob, huPackType, 1)
			{
				KP_Weight = 3m,
				KP_WeightUQ = "KG",
				KP_Volume = 2m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var status = picked ? "PIC" : "CTT";
			var handlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, handlingUnitPackage.PK, status, whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation, isHandlingUnit: true);

			var package1 = new PkgPackage(pkgPackageJob, handlingUnitPackage, package1PackType, 1)
			{
				KP_Weight = 2m,
				KP_WeightUQ = "KG",
				KP_Volume = 1m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var package1PackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, status, whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation);

			var divot1 = new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, package1.PK).AppendInsertAndReturnObject(sql);

			var package2 = new PkgPackage(pkgPackageJob, handlingUnitPackage, package2PackType, 1)
			{
				KP_Weight = .3m,
				KP_WeightUQ = "KG",
				KP_Volume = .4m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var package2PackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package2.PK, status, whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation);

			var divot2 = new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, package2.PK).AppendInsertAndReturnObject(sql);

			var transferHeader = new WhsItemTransferHeader("TransHA", whs) { WTH_TransferType = "TRF" }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsItemTransferLine(handlingUnitPackageState, fromLocation, toLocation, transferHeader)
			{
				WTF_GS_NKPickUser = "~BP",
				WTF_PickTime = picked ? DateTime.Today.AddDays(-1) : (DateTime?)null,
				WTF_PutTime = null,
			}.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageSchema.Constants.TableName, TestConnection))
			{
				Db.Connection.ExecuteNonQuery(sql.ToString());
			}

			AssertCountOfLocations(2);

			AssertLocationRecord(
				toLocation,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: expectPalletQuantity ? 1 : 0,
				isEmpty: false,
				availableWeight: 4m - 3m,
				availableVolume: 5m - 2m);

			AssertLocationRecord(
				fromLocation,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: picked || !expectPalletQuantity ? 0 : 1,
				isEmpty: picked,
				availableWeight: picked ? 8m : 8m - 3m,
				availableVolume: picked ? 10m : 10m - 2m);
		}

		#endregion

		#region TestView_WithPickedOrCommittedPackages_HandlingUnits_MultiLevel

		public void TestView_WithCommittedPackages_HandlingUnits_MultiLevel() => TestView_WithPickedOrCommittedPackages_HandlingUnits_MultiLevel(picked: false);
		public void TestView_WithPickedPackages_HandlingUnits_MultiLevel() => TestView_WithPickedOrCommittedPackages_HandlingUnits_MultiLevel(picked: true);

		void TestView_WithPickedOrCommittedPackages_HandlingUnits_MultiLevel(bool picked)
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "TRW", branchPK: branch.PK).WithDockDoor(sql);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var toLocation = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 1,
				WL_Tray = 1,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var fromLocation = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 8,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 10,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC11", "RC11", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", toLocation, "VEH123").AppendInsertAndReturnObject(sql);
			var pkgPackageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);

			var topHandlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
			var topHandlingUnitPackageJob = new PkgPackageJob(topHandlingUnit.PK, parentTableCode: "KPU") { KJ_JobID = "TOP" }.AppendInsertAndReturnObject(sql);

			var topHandlingUnitPackage = new PkgPackage(topHandlingUnitPackageJob, "PLT", 1)
			{
				KP_Weight = 3m,
				KP_WeightUQ = "KG",
				KP_Volume = 2m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var status = picked ? "PIC" : "CTT";
			var topHandlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, topHandlingUnitPackage.PK, status, whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation, isHandlingUnit: true);

			var secondLevelHandlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
			var secondLevelHandlingUnitPackageJob = new PkgPackageJob(secondLevelHandlingUnit.PK, parentTableCode: "KPU") { KJ_JobID = "2ND" }.AppendInsertAndReturnObject(sql);

			var secondLevelHandlingUnitPackage = new PkgPackage(secondLevelHandlingUnitPackageJob, topHandlingUnitPackage, "PLT", 1)
			{
				KP_Weight = .02m,
				KP_WeightUQ = "KG",
				KP_Volume = .03m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var secondLevelHandlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, secondLevelHandlingUnitPackage.PK, status, whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation, isHandlingUnit: true);

			var package1 = new PkgPackage(pkgPackageJob, topHandlingUnitPackage, "PLT", 1)
			{
				KP_Weight = 2m,
				KP_WeightUQ = "KG",
				KP_Volume = 1m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var package1PackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, status, whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation);

			new PkgPackageHandlingUnitDivot(secondLevelHandlingUnitPackage.PK, package1.PK).AppendInsertAndReturnObject(sql);
			new PkgPackageHandlingUnitDivot(topHandlingUnitPackage.PK, secondLevelHandlingUnitPackage.PK).AppendInsertAndReturnObject(sql);

			var package2 = new PkgPackage(pkgPackageJob, topHandlingUnitPackage, "BOX", 1)
			{
				KP_Weight = .3m,
				KP_WeightUQ = "KG",
				KP_Volume = .4m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var package2PackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package2.PK, status, whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation);

			new PkgPackageHandlingUnitDivot(secondLevelHandlingUnitPackage.PK, package2.PK).AppendInsertAndReturnObject(sql);

			var transferHeader = new WhsItemTransferHeader("TransHA", whs) { WTH_TransferType = "TRF" }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsItemTransferLine(topHandlingUnitPackageState, fromLocation, toLocation, transferHeader)
			{
				WTF_GS_NKPickUser = "~BP",
				WTF_PickTime = picked ? DateTime.Today.AddDays(-1) : (DateTime?)null,
				WTF_PutTime = null,
			}.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageSchema.Constants.TableName, TestConnection))
			{
				Db.Connection.ExecuteNonQuery(sql.ToString());
			}

			AssertCountOfLocations(2);

			AssertLocationRecord(
				toLocation,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: 1,
				isEmpty: false,
				availableWeight: 4m - 3m,
				availableVolume: 5m - 2m);

			AssertLocationRecord(
				fromLocation,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: picked ? 0 : 1,
				isEmpty: picked,
				availableWeight: picked ? 8m : 8m - 3m,
				availableVolume: picked ? 10m : 10m - 2m);
		}

		#endregion

		#region TestView_WithPickedOrCommittedPackages_HandlingUnits_MultiLevel_PalletQuantity

		public void TestView_WithCommittedPackages_HandlingUnits_MultiLevel_PalletQuantity()
		{
			TestView_WithPickedOrCommittedPackages_HandlingUnits_MultiLevel_PalletQuantity(picked: false, outerIsAPallet: false);
		}

		public void TestView_WithCommittedPackages_HandlingUnits_MultiLevel_PalletQuantity_OuterIsAPallet()
		{
			TestView_WithPickedOrCommittedPackages_HandlingUnits_MultiLevel_PalletQuantity(picked: false, outerIsAPallet: true);
		}

		public void TestView_WithPickedPackages_HandlingUnits_MultiLevel_PalletQuantity()
		{
			TestView_WithPickedOrCommittedPackages_HandlingUnits_MultiLevel_PalletQuantity(picked: true, outerIsAPallet: false);
		}

		public void TestView_WithPickedPackages_HandlingUnits_MultiLevel_PalletQuantity_OuterIsAPallet()
		{
			TestView_WithPickedOrCommittedPackages_HandlingUnits_MultiLevel_PalletQuantity(picked: true, outerIsAPallet: true);
		}

		void TestView_WithPickedOrCommittedPackages_HandlingUnits_MultiLevel_PalletQuantity(bool picked, bool outerIsAPallet)
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "TRW", branchPK: branch.PK).WithDockDoor(sql);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var toLocation = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 1,
				WL_Tray = 1,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var fromLocation = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 8,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 10,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC11", "RC11", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", toLocation, "VEH123").AppendInsertAndReturnObject(sql);
			var pkgPackageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);

			var topHandlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
			var topHandlingUnitPackageJob = new PkgPackageJob(topHandlingUnit.PK, parentTableCode: "KPU") { KJ_JobID = "TOP" }.AppendInsertAndReturnObject(sql);

			var topHandlingUnitPackage = new PkgPackage(topHandlingUnitPackageJob, outerIsAPallet ? "PLT" : "BOX", 1)
			{
				KP_Weight = 3m,
				KP_WeightUQ = "KG",
				KP_Volume = 2m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var status = picked ? "PIC" : "CTT";
			var topHandlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, topHandlingUnitPackage.PK, status, whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation, isHandlingUnit: true);

			var secondLevelHandlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
			var secondLevelHandlingUnitPackageJob = new PkgPackageJob(secondLevelHandlingUnit.PK, parentTableCode: "KPU") { KJ_JobID = "2ND" }.AppendInsertAndReturnObject(sql);

			var secondLevelHandlingUnitPackage = new PkgPackage(secondLevelHandlingUnitPackageJob, topHandlingUnitPackage, "PLT", 1)
			{
				KP_Weight = .02m,
				KP_WeightUQ = "KG",
				KP_Volume = .03m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var secondLevelHandlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, secondLevelHandlingUnitPackage.PK, status, whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation, isHandlingUnit: true);

			var package1 = new PkgPackage(pkgPackageJob, topHandlingUnitPackage, "PLT", 1)
			{
				KP_Weight = 2m,
				KP_WeightUQ = "KG",
				KP_Volume = 1m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var package1PackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, status, whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation);

			new PkgPackageHandlingUnitDivot(secondLevelHandlingUnitPackage.PK, package1.PK).AppendInsertAndReturnObject(sql);
			new PkgPackageHandlingUnitDivot(topHandlingUnitPackage.PK, secondLevelHandlingUnitPackage.PK).AppendInsertAndReturnObject(sql);

			var package2 = new PkgPackage(pkgPackageJob, topHandlingUnitPackage, "PLT", 1)
			{
				KP_Weight = .3m,
				KP_WeightUQ = "KG",
				KP_Volume = .4m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var package2PackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package2.PK, status, whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation);

			new PkgPackageHandlingUnitDivot(secondLevelHandlingUnitPackage.PK, package2.PK).AppendInsertAndReturnObject(sql);

			var transferHeader = new WhsItemTransferHeader("TransHA", whs) { WTH_TransferType = "TRF" }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsItemTransferLine(topHandlingUnitPackageState, fromLocation, toLocation, transferHeader)
			{
				WTF_GS_NKPickUser = "~BP",
				WTF_PickTime = picked ? DateTime.Today.AddDays(-1) : (DateTime?)null,
				WTF_PutTime = null,
			}.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageSchema.Constants.TableName, TestConnection))
			{
				Db.Connection.ExecuteNonQuery(sql.ToString());
			}

			AssertCountOfLocations(2);

			AssertLocationRecord(
				toLocation,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: outerIsAPallet ? 1 : 0,
				isEmpty: false,
				availableWeight: 4m - 3m,
				availableVolume: 5m - 2m);

			AssertLocationRecord(
				fromLocation,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: picked || !outerIsAPallet ? 0 : 1,
				isEmpty: picked,
				availableWeight: picked ? 8m : 8m - 3m,
				availableVolume: picked ? 10m : 10m - 2m);
		}

		#endregion

		#region TestView_WithPickedOrCommittedPackages_HandlingUnits_Unpacked

		public void TestView_WithCommittedPackages_HandlingUnits_Unpacked() => TestView_WithPickedOrCommittedPackages_HandlingUnits_Unpacked(picked: false);
		public void TestView_WithPickedPackages_HandlingUnits_Unpacked() => TestView_WithPickedOrCommittedPackages_HandlingUnits_Unpacked(picked: true);

		void TestView_WithPickedOrCommittedPackages_HandlingUnits_Unpacked(bool picked)
		{
			var sql = new StringBuilder();
			var branch = new GlbBranch("BR1").AppendInsertAndReturnObject(sql);
			var whs = new WhsWarehouse("WH1", "TRW", branchPK: branch.PK).WithDockDoor(sql);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var toLocation = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 1,
				WL_Tray = 1,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 4,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 5,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var fromLocation = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
				WL_MaxWeight = 8,
				WL_MaxWeightUnit = "KG",
				WL_MaxCubic = 10,
				WL_MaxCubicUnit = "M3",
			}.AppendInsertAndReturnObject(sql);

			var receiveConsignment = new WhsItemReceiveConsignment(whs, "RC11", "RC11", string.Empty, string.Empty).AppendInsertAndReturnObject(sql);
			var receiveTransportationUnit = new WhsItemReceiveTransportationUnit(whs, "RTU123", toLocation, "VEH123").AppendInsertAndReturnObject(sql);
			var pkgPackageJob = new PkgPackageJob(receiveConsignment.PK, parentTableCode: "WRC").AppendInsertAndReturnObject(sql);

			var handlingUnit = new PkgHandlingUnit(branch.PK).AppendInsertAndReturnObject(sql);
			var handlingUnitPackageJob = new PkgPackageJob(handlingUnit.PK, parentTableCode: "KPU") { KJ_JobID = "HU" }.AppendInsertAndReturnObject(sql);

			var handlingUnitPackage = new PkgPackage(handlingUnitPackageJob, "PLT", 1)
			{
				KP_Weight = 3m,
				KP_WeightUQ = "KG",
				KP_Volume = 2m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var status = picked ? "PIC" : "CTT";
			var handlingUnitPackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, handlingUnitPackage.PK, status, whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation, isHandlingUnit: true);

			var package1 = new PkgPackage(pkgPackageJob, handlingUnitPackage, "PLT", 1)
			{
				KP_Weight = 2m,
				KP_WeightUQ = "KG",
				KP_Volume = 1m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var package1PackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, status, whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation);

			var unpackedDivot1 = new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, package1.PK)
			{
				KPD_UnpackedTime = DateTime.UtcNow,
				KPD_GS_NKUnpackedUser = "~BP",
			}.AppendInsertAndReturnObject(sql);

			var divot1 = new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, package1.PK).AppendInsertAndReturnObject(sql);

			var package2 = new PkgPackage(pkgPackageJob, handlingUnitPackage, "BOX", 1)
			{
				KP_Weight = .3m,
				KP_WeightUQ = "KG",
				KP_Volume = .4m,
				KP_VolumeUQ = "M3",
			}.AppendInsertAndReturnObject(sql);

			var package2PackageState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package2.PK, status, whs, receiveConsignment, receiveTransportationUnit, lastLocation: fromLocation, receiveLocation: fromLocation);

			var unpackedDivot2 = new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, package2.PK)
			{
				KPD_UnpackedTime = DateTime.UtcNow,
				KPD_GS_NKUnpackedUser = "~BP",
			}.AppendInsertAndReturnObject(sql);

			var divot2 = new PkgPackageHandlingUnitDivot(handlingUnitPackage.PK, package2.PK).AppendInsertAndReturnObject(sql);

			var transferHeader = new WhsItemTransferHeader("TransHA", whs) { WTH_TransferType = "TRF" }.AppendInsertAndReturnObject(sql);
			var transferLine1 = new WhsItemTransferLine(handlingUnitPackageState, fromLocation, toLocation, transferHeader)
			{
				WTF_GS_NKPickUser = "~BP",
				WTF_PickTime = picked ? DateTime.Today.AddDays(-1) : (DateTime?)null,
				WTF_PutTime = null,
			}.AppendInsertAndReturnObject(sql);

			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageSchema.Constants.TableName, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(EnsureTopLevelHandlingUnitPackageWhenInsertDivot, PkgPackageHandlingUnitDivotSchema.Constants.TableName, TestConnection))
			{
				Db.Connection.ExecuteNonQuery(sql.ToString());
			}
			AssertCountOfLocations(2);

			AssertLocationRecord(
				toLocation,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: 1,
				isEmpty: false,
				availableWeight: 4m - 3m,
				availableVolume: 5m - 2m);

			AssertLocationRecord(
				fromLocation,
				rnoType,
				putawayArea,
				pickingArea,
				whs.PK,
				"R1",
				palletQuantity: picked ? 0 : 1,
				isEmpty: picked,
				availableWeight: picked ? 8m : 8m - 3m,
				availableVolume: picked ? 10m : 10m - 2m);
		}

		#endregion

		#region TestView_FiltersDockDoorLocations

		public void TestView_FiltersDockDoorLocations()
		{
			new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			AssertCountOfLocations(0);
		}

		#endregion

		#region TestView_FiltersVoidLocations

		public void TestView_ReturnsVoidLocations()
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", "TRW").WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "VOI",
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
			AssertCountOfLocations(1);
			AssertLocationRecord(location, rnoType, putawayArea, pickingArea, whs.PK, "R1");
		}

		#endregion

		#region TestView_FiltersProductWarehouse

		public void TestView_FiltersProductWarehouse() => TestView_FiltersOtherWarehouseTypes("PRW");

		void TestView_FiltersOtherWarehouseTypes(string otherWarehouseType)
		{
			var sql = new StringBuilder();
			var whs = new WhsWarehouse("WH1", otherWarehouseType).WithDockDoor(TestConnection);
			var pickingArea = new WhsArea(whs.PK, "A").AppendInsertAndReturnObject(sql);
			var putawayArea = new WhsArea(whs.PK, "P").AppendInsertAndReturnObject(sql);
			var rowPK = new WhsRow(whs, "R1").AppendInsertAndReturnObject(sql).PK;
			var rnoType = WhsLocationType.ShallowLoadFromDB(TestConnection, wlt => wlt.WLT_Code == "RNO").Single();
			var location = new WhsLocation(rowPK, pickingArea.PK, putawayArea.PK, rnoType.PK)
			{
				WL_Column = 1,
				WL_Level = 2,
				WL_Tray = 3,
				WL_LocationStatus = "NOR",
			}.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());

			AssertCountOfLocations(0);
		}

		#endregion

		#region TestView_FiltersFTZWarehouse

		public void TestView_FiltersFTZWarehouse() => TestView_FiltersOtherWarehouseTypes("FTZ");

		#endregion

		#region TestView_FiltersCYDWarehouse

		public void TestView_FiltersCYDWarehouse() => TestView_FiltersOtherWarehouseTypes("CYD");

		#endregion

		#region Implementation

		void AssertCountOfLocations(int expectedCount)
		{
			AssertEquals(expectedCount, Db.Connection.ExecuteScalar($"SELECT COUNT(*) FROM dbo.WhsLocationForTransitPutaway"));
		}

		void AssertLocationRecord(
			WhsLocation location,
			WhsLocationType locationType,
			WhsArea putawayArea,
			WhsArea pickingArea,
			Guid warehousePK,
			string rowName,
			int palletQuantity = 0,
			bool isEmpty = true,
			decimal availableWeight = 0,
			decimal availableVolume = 0)
		{
			var sql = $"SELECT * from dbo.WhsLocationForTransitPutaway WHERE WLV_PK = '{location.PK}'";
			var locationRecords = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			locationRecords.Load(sql);

			AssertEquals("Should have 1 record.", 1, locationRecords.Count);

			var locationRecord = locationRecords[0];
			CombineAssertions(() =>
			{
				AssertEquals("WarehousePK", warehousePK, ((ZGuid)locationRecord["WLV_WarehousePK"]));
				AssertEquals("AreaName", putawayArea.WA_Name, ((ZString)locationRecord["WLV_AreaName"]));
				AssertEquals("PalletSpaces", location.WL_PalletFloorSpaces * location.WL_PalletStackHeight, ((ZInt)locationRecord["WLV_PalletSpaces"]));
				AssertEquals("PalletQuantity", palletQuantity, ((ZInt)locationRecord["WLV_PalletQuantity"]));
				AssertEquals("IsEmpty", isEmpty, ((ZBool)locationRecord["WLV_IsEmpty"]));
				AssertEquals("AvailableWeight", availableWeight, ((ZDecimal)locationRecord["WLV_AvailableWeight"]), 0.0001m);
				AssertEquals("HasWeightCapacity", location.WL_MaxWeight != 0, ((ZBool)locationRecord["WLV_HasWeightCapacity"]));
				AssertEquals("AvailableVolume", availableVolume, ((ZDecimal)locationRecord["WLV_AvailableVolume"]), 0.0001m);
				AssertEquals("HasVolumeCapacity", location.WL_MaxCubic != 0, ((ZBool)locationRecord["WLV_HasVolumeCapacity"]));
				AssertEquals("LastInventoryChangeDate", location.WL_LastInventoryChangeDate ?? ZDateTimeOffset.Empty, ((ZDateTimeOffset)locationRecord["WLV_LastInventoryChangeDate"]));
				AssertEquals("LocationTypeCode", locationType.WLT_Code, ((ZString)locationRecord["WLV_LocationTypeCode"]));
				AssertEquals("MinimumTemperature", locationType.WLT_MinimumTemperature, ((ZDecimal)locationRecord["WLV_MinimumTemperature"]));
				AssertEquals("MaximumTemperature", locationType.WLT_MaximumTemperature, ((ZDecimal)locationRecord["WLV_MaximumTemperature"]));
				AssertEquals("TemperatureUnit", locationType.WLT_TemperatureUnit, ((ZString)locationRecord["WLV_TemperatureUnit"]));
			});
		}

		const string EnsureChildPackagesHaveSameTopLevelHandlingUnit = "TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit";
		const string EnsureTopLevelHandlingUnitPackageWhenInsertDivot = "TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit";

		#endregion
	}
}

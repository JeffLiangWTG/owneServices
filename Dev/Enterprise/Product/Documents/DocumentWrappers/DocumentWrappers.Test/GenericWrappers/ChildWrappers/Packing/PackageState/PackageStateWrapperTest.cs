using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackageStateWrapper))]
	sealed class PackageStateWrapperTest : GenericWrapperTest
	{
		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			var wrapper = new PackageStateWrapper((WhsItemPackageState)null, Factory);
			AssertEquals(ZString.Empty, wrapper.Status.Code);
			AssertEquals(ZDateTime.Empty, wrapper.Arrived);
			AssertEquals(ZDateTime.Empty, wrapper.Departed);
			AssertEquals(null, wrapper.ReceiveConsignment);
			AssertEquals(ZDateTime.Empty, wrapper.CrossDockTransferPickTime);
			AssertEquals(ZDateTime.Empty, wrapper.CrossDockTransferPutTime);
			AssertEquals(ZDateTime.Empty, wrapper.PutawayTransferPickTime);
			AssertEquals(ZDateTime.Empty, wrapper.PutawayTransferPutTime);
			AssertEquals(ZDateTime.Empty, wrapper.PickTransferPickTime);
			AssertEquals(ZDateTime.Empty, wrapper.PickTransferPutTime);
			AssertEquals(ZDateTime.Empty, wrapper.LoadedTime);
			AssertEquals(ZDateTime.Empty, wrapper.UnloadedNotYetProcessedTime);

			wrapper = new PackageStateWrapper((PackageWrapperFromPkgPackage)null, Factory);
			AssertEquals(ZString.Empty, wrapper.Status.Code);
			AssertEquals(ZDateTime.Empty, wrapper.Arrived);
			AssertEquals(ZDateTime.Empty, wrapper.Departed);
			AssertEquals(null, wrapper.ReceiveConsignment);
			AssertEquals(ZDateTime.Empty, wrapper.CrossDockTransferPickTime);
			AssertEquals(ZDateTime.Empty, wrapper.CrossDockTransferPutTime);
			AssertEquals(ZDateTime.Empty, wrapper.PutawayTransferPickTime);
			AssertEquals(ZDateTime.Empty, wrapper.PutawayTransferPutTime);
			AssertEquals(ZDateTime.Empty, wrapper.PickTransferPickTime);
			AssertEquals(ZDateTime.Empty, wrapper.PickTransferPutTime);
			AssertEquals(ZDateTime.Empty, wrapper.LoadedTime);
			AssertEquals(ZDateTime.Empty, wrapper.UnloadedNotYetProcessedTime);
		}

		#endregion

		public void TestHandlingUnit()
		{
			var package = Factory.New<PkgPackage>();
			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_IsHandlingUnit = true;
			packageState.WPS_KP_Package = package.PK;

			var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
			var packageStateWrapper = new PackageStateWrapper(packageWrapper, Factory);

			AssertEquals(true, packageStateWrapper.IsHandlingUnit);
		}

		public void TestIsHighRisk()
		{
			var package = Factory.New<PkgPackage>();
			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_IsHighRisk = true;
			packageState.WPS_IsHighRiskAuthorized = true;
			packageState.WPS_KP_Package = package.PK;

			var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
			var packageStateWrapper = new PackageStateWrapper(packageWrapper, Factory);

			AssertEquals(true, packageStateWrapper.IsHighRisk);
			AssertEquals(true, packageStateWrapper.IsHighRiskAuthorized);
		}

		#region TestWrapperMappings

		public void TestWrapperMappings()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var branch = helper.CreateGlbBranch("TST");
			branch.GB_RL_NKHomePort = "CNNJG";
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var sydTime = new ZDateTimeOffset(2023, 2, 3, 10, 0, 0, TimeSpan.FromHours(11));

				var location = Factory.New<WhsLocation>();

				var receiveConsignment = Factory.New<WhsItemReceiveConsignment>();
				receiveConsignment.WRC_ConsignmentID = "WRC12345";

				var dispatchConsignment = Factory.New<WhsItemDispatchConsignment>();
				dispatchConsignment.WDC_ConsignmentID = "WDC12345";
				dispatchConsignment.WDC_JobID = "TD00000001";

				var receiveASN = Factory.New<WhsItemReceiveASN>();
				var receiveHeader = Factory.New<WhsItemReceiveTransportationUnit>();
				var warehouse = Factory.New<WhsWarehouse>();
				receiveHeader.WRH_WW_Warehouse = warehouse.PK;
				var dispatchHeader = Factory.New<WhsItemDispatchTransportationUnit>();
				dispatchHeader.WDH_GateOutTime = sydTime.AddDays(-1);

				var package = Factory.New<PkgPackage>();
				var packageState = Factory.New<WhsItemPackageState>();
				packageState.WPS_WRC_TransitReceiveConsignment = receiveConsignment.PK;
				packageState.WPS_WW_Warehouse = warehouse.PK;
				packageState.WPS_WRP_ReceiveExpectedPacking = receiveASN.PK;
				packageState.WPS_WRH_TransitReceiveHeader = receiveHeader.PK;
				packageState.WPS_WDC_TransitDispatchConsignment = dispatchConsignment.PK;
				packageState.WPS_WDH_TransitDispatchHeader = dispatchHeader.PK;
				packageState.WPS_KP_Package = package.PK;
				packageState.WPS_Status = "BKD";
				packageState.WPS_UnloadedTime = sydTime.AddDays(-2);
				packageState.WPS_WL_LastLocation = location.PK;

				var expectedCNTime = new ZDateTimeOffset(2023, 2, 3, 7, 0, 0, TimeSpan.FromHours(8));
				var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
				var wrapper = new PackageStateWrapper(packageWrapper, Factory);
				var whsStatus = new TransitWarehouseStatuses();
				AssertEquals("WRC12345", wrapper.ConsignmentID);
				AssertEquals("BKD", wrapper.Status.Code);
				AssertEquals(whsStatus.GetDescriptionFromCode("BKD"), wrapper.Status.Description);
				AssertEquals(expectedCNTime.AddDays(-2).ToZDateTime(), wrapper.Arrived);
				AssertEquals(expectedCNTime.AddDays(-1).ToZDateTime(), wrapper.Departed);
				AssertEquals(receiveConsignment, wrapper.ReceiveConsignment.WrappedObject);
				AssertEquals(receiveASN, wrapper.ReceiveASN.WrappedObject);
				AssertEquals(warehouse, wrapper.Warehouse.WrappedObject);
				AssertEquals(dispatchConsignment, wrapper.DispatchConsignment.WrappedObject);
				AssertEquals(dispatchHeader, wrapper.DispatchTransportationUnit.WrappedObject);
				AssertEquals(receiveHeader, wrapper.ReceiveTransportationUnit.WrappedObject);
				AssertEquals("TD00000001", wrapper.DispatchConsignment.JobNumber);
				AssertEquals("WDC12345", wrapper.DispatchConsignment.SecondaryNumber);
				AssertEquals(true, wrapper.HasArrived);
				AssertEquals(true, wrapper.HasDeparted);
			}
		}

		#endregion

		#region Test Planned ReceiveTransportationUnit

		public void TestPlannedReceiveTransportationUnitMappings_BookedPackage()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var branch = helper.CreateGlbBranch("TST");
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var receiveConsignment = Factory.New<WhsItemReceiveConsignment>();
				receiveConsignment.WRC_ConsignmentID = "WRC12345";

				var receiveASN = Factory.New<WhsItemReceiveASN>();
				var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
				var receiveASNRTUPivot = Factory.New<WhsItemReceiveASNRTUPivot>();
				var warehouse = Factory.New<WhsWarehouse>();
				rtu.WRH_WW_Warehouse = warehouse.PK;
				receiveASN.WRP_WW_IntendedWarehouse = warehouse.PK;
				receiveASNRTUPivot.WAR_WRP_TransitReceiveASN = receiveASN.PK;
				receiveASNRTUPivot.WAR_WRH_TransitReceiveTransportationUnit = rtu.PK;

				var package = Factory.New<PkgPackage>();
				var packageState = Factory.New<WhsItemPackageState>();
				packageState.WPS_WRC_TransitReceiveConsignment = receiveConsignment.PK;
				packageState.WPS_WW_Warehouse = warehouse.PK;
				packageState.WPS_WRP_ReceiveExpectedPacking = receiveASN.PK;
				packageState.WPS_KP_Package = package.PK;
				packageState.WPS_Status = "BKD";

				var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
				var wrapper = new PackageStateWrapper(packageWrapper, Factory);

				AssertEquals("WRC12345", wrapper.ConsignmentID);
				AssertEquals("BKD", wrapper.Status.Code);
				AssertEquals(receiveConsignment, wrapper.ReceiveConsignment.WrappedObject);
				AssertEquals(receiveASN, wrapper.ReceiveASN.WrappedObject);
				AssertEquals(rtu, wrapper.PlannedReceiveTransportationUnits[0].WrappedObject);
			}
		}

		public void TestPlannedReceiveTransportationUnitMappings_ArrivedPackage_DifferentRtu()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var branch = helper.CreateGlbBranch("TST");
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var receiveConsignment = Factory.New<WhsItemReceiveConsignment>();
				receiveConsignment.WRC_ConsignmentID = "WRC12345";

				var receiveASN = Factory.New<WhsItemReceiveASN>();
				var receiveHeader = Factory.New<WhsItemReceiveTransportationUnit>();
				var plannedRtu = Factory.New<WhsItemReceiveTransportationUnit>();
				var receiveASNRTUPivot = Factory.New<WhsItemReceiveASNRTUPivot>();
				var warehouse = Factory.New<WhsWarehouse>();
				plannedRtu.WRH_WW_Warehouse = warehouse.PK;
				receiveASN.WRP_WW_IntendedWarehouse = warehouse.PK;
				receiveASNRTUPivot.WAR_WRP_TransitReceiveASN = receiveASN.PK;
				receiveASNRTUPivot.WAR_WRH_TransitReceiveTransportationUnit = plannedRtu.PK;

				var package = Factory.New<PkgPackage>();
				var packageState = Factory.New<WhsItemPackageState>();
				packageState.WPS_WRC_TransitReceiveConsignment = receiveConsignment.PK;
				packageState.WPS_WW_Warehouse = warehouse.PK;
				packageState.WPS_WRP_ReceiveExpectedPacking = receiveASN.PK;
				packageState.WPS_WRH_TransitReceiveHeader = receiveHeader.PK;
				packageState.WPS_KP_Package = package.PK;
				packageState.WPS_Status = "ARV";
				packageState.WPS_UnloadedTime = new ZDateTimeOffset(2023, 2, 3, 10, 0, 0, TimeSpan.FromHours(11));

				var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
				var wrapper = new PackageStateWrapper(packageWrapper, Factory);

				AssertEquals("WRC12345", wrapper.ConsignmentID);
				AssertEquals("ARV", wrapper.Status.Code);
				AssertEquals(receiveConsignment, wrapper.ReceiveConsignment.WrappedObject);
				AssertEquals(receiveASN, wrapper.ReceiveASN.WrappedObject);
				AssertEquals(receiveHeader, wrapper.ReceiveTransportationUnit.WrappedObject);
				AssertEquals(plannedRtu, wrapper.PlannedReceiveTransportationUnits[0].WrappedObject);
			}
		}

		#endregion

		public void TestWrapperMappingsTransitDispatchConsignment()
		{
			var package = Factory.New<PkgPackage>();
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			consignment.WDC_ConsignmentID = "WDC12345";
			consignment.WDC_JobID = "TD00000001";
			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_WDC_TransitDispatchConsignment = consignment.PK;
			packageState.WPS_KP_Package = package.PK;

			var helper = new WhsTestHelperFunctions(Factory);
			var whs = helper.CreateWarehouse("1");
			var row = helper.CreateRowAndGenerateLocations(whs, "A", 1, 1, 2);
			packageState.WPS_WL_LastLocation = row.Locations.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1).PK;

			var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
			var wrapper = new PackageStateWrapper(packageWrapper, Factory);

			AssertEquals("TD00000001", wrapper.DispatchConsignment.JobNumber);
			AssertEquals("WDC12345", wrapper.DispatchConsignment.SecondaryNumber);
			AssertEquals("A-1-1-1", wrapper.Location);
		}

		#region TestConsignmentID

		public void TestConsignmentID()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var warehouse = helper.CreateTRWWarehouse();
			var location = helper.CreateLocation(warehouse);
			var rcn = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dcn = helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var rtu = helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageStateOnlyRCN = helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked);
			var wrapper = new PackageStateWrapper(packageStateOnlyRCN, Factory);
			AssertEquals("RCN1", wrapper.ConsignmentID);

			var packageStateOnlyDCN = helper.CreateOverpackPackage("OVP1", dcn, rtu);
			packageStateOnlyDCN.WPS_WDC_TransitDispatchConsignment = dcn.PK;
			wrapper = new PackageStateWrapper(packageStateOnlyDCN, Factory);
			AssertEquals("DCN1", wrapper.ConsignmentID);

			var packageStateBothRCNAndDCN = helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			wrapper = new PackageStateWrapper(packageStateBothRCNAndDCN, Factory);
			AssertEquals("RCN1", wrapper.ConsignmentID);
		}

		#endregion

		#region TestWarehouseBOWrapper_FactoryCached

		public void TestWarehouseBOWrapper_FactoryCached()
		{
			var warehouse = Factory.New<WhsWarehouse>();

			var receiveHeader1 = Factory.New<WhsItemReceiveTransportationUnit>();
			receiveHeader1.WRH_WW_Warehouse = warehouse.PK;

			var package1 = Factory.New<PkgPackage>();
			var packageWrapper1 = new PackageWrapperFromPkgPackage(package1, Factory);

			var packageState1 = Factory.New<WhsItemPackageState>();
			packageState1.WPS_WRH_TransitReceiveHeader = receiveHeader1.PK;
			packageState1.WPS_KP_Package = package1.PK;
			packageState1.WPS_WW_Warehouse = warehouse.PK;
			var packageStateWrapper1 = new PackageStateWrapper(packageWrapper1, Factory);

			var receiveHeader2 = Factory.New<WhsItemReceiveTransportationUnit>();
			receiveHeader2.WRH_WW_Warehouse = warehouse.PK;

			var package2 = Factory.New<PkgPackage>();
			var packageWrapper2 = new PackageWrapperFromPkgPackage(package2, Factory);

			var packageState2 = Factory.New<WhsItemPackageState>();
			packageState2.WPS_WRH_TransitReceiveHeader = receiveHeader2.PK;
			packageState2.WPS_KP_Package = package2.PK;
			packageState2.WPS_WW_Warehouse = warehouse.PK;
			var packageStateWrapper2 = new PackageStateWrapper(packageWrapper2, Factory);

			AssertNotNull("Package state wrapper has warehouse BO wrapper.", packageStateWrapper1.Warehouse);
			AssertNotNull("Package state wrapper has warehouse BO wrapper.", packageStateWrapper2.Warehouse);
			AssertEquals("Both wrappers share the same instance of WarehouseBO wrapper.", packageStateWrapper1.Warehouse, packageStateWrapper2.Warehouse);
			AssertEquals(warehouse, packageStateWrapper1.Warehouse.WrappedObject);
		}

		public void TestWarehouseBOWrapper_FactoryCached_DifferentWarehouse()
		{
			var warehouse1 = Factory.New<WhsWarehouse>();
			var receiveHeader1 = Factory.New<WhsItemReceiveTransportationUnit>();
			receiveHeader1.WRH_WW_Warehouse = warehouse1.PK;

			var package1 = Factory.New<PkgPackage>();
			var packageWrapper1 = new PackageWrapperFromPkgPackage(package1, Factory);

			var packageState1 = Factory.New<WhsItemPackageState>();
			packageState1.WPS_WRH_TransitReceiveHeader = receiveHeader1.PK;
			packageState1.WPS_KP_Package = package1.PK;
			packageState1.WPS_WW_Warehouse = warehouse1.PK;
			var packageStateWrapper1 = new PackageStateWrapper(packageWrapper1, Factory);

			var warehouse2 = Factory.New<WhsWarehouse>();
			var receiveHeader2 = Factory.New<WhsItemReceiveTransportationUnit>();
			receiveHeader2.WRH_WW_Warehouse = warehouse2.PK;

			var package2 = Factory.New<PkgPackage>();
			var packageWrapper2 = new PackageWrapperFromPkgPackage(package2, Factory);

			var packageState2 = Factory.New<WhsItemPackageState>();
			packageState2.WPS_WRH_TransitReceiveHeader = receiveHeader2.PK;
			packageState2.WPS_KP_Package = package2.PK;
			packageState2.WPS_WW_Warehouse = warehouse2.PK;
			var packageStateWrapper2 = new PackageStateWrapper(packageWrapper2, Factory);

			AssertNotEquals("Wrappers don't share the same instance of WarehouseBO wrapper.", packageStateWrapper1.Warehouse, packageStateWrapper2.Warehouse);
			AssertEquals(warehouse1, packageStateWrapper1.Warehouse.WrappedObject);
			AssertEquals(warehouse2, packageStateWrapper2.Warehouse.WrappedObject);
		}

		public void TestWarehouseBOWrapper_FactoryCached_DifferentFactory()
		{
			var warehouse1 = Factory.NewWithValidTestData<WhsWarehouse>();
			Factory.Save();

			var receiveHeader1 = Factory.New<WhsItemReceiveTransportationUnit>();
			receiveHeader1.WRH_WW_Warehouse = warehouse1.PK;

			var package1 = Factory.New<PkgPackage>();
			var packageWrapper1 = new PackageWrapperFromPkgPackage(package1, Factory);

			var packageState1 = Factory.New<WhsItemPackageState>();
			packageState1.WPS_WRH_TransitReceiveHeader = receiveHeader1.PK;
			packageState1.WPS_KP_Package = package1.PK;
			packageState1.WPS_WW_Warehouse = warehouse1.PK;
			var packageStateWrapper1 = new PackageStateWrapper(packageWrapper1, Factory);

			var newFactory = new BusinessObjectFactory();
			var receiveHeader2 = newFactory.New<WhsItemReceiveTransportationUnit>();
			receiveHeader2.WRH_WW_Warehouse = warehouse1.PK;

			var package2 = newFactory.New<PkgPackage>();
			var packageWrapper2 = new PackageWrapperFromPkgPackage(package2, newFactory);

			var packageState2 = newFactory.New<WhsItemPackageState>();
			packageState2.WPS_WRH_TransitReceiveHeader = receiveHeader2.PK;
			packageState2.WPS_KP_Package = package2.PK;
			packageState2.WPS_WW_Warehouse = warehouse1.PK;
			var packageStateWrapper2 = new PackageStateWrapper(packageWrapper2, newFactory);

			AssertNotEquals("Wrappers don't share the same instance of WarehouseBO wrapper.", packageStateWrapper1.Warehouse, packageStateWrapper2.Warehouse);
			AssertEquals(warehouse1, packageStateWrapper1.Warehouse.WrappedObject);

			var warehouseInNewFactory = newFactory.Load<WhsWarehouse>(warehouse1.PK);
			AssertEquals(warehouseInNewFactory, packageStateWrapper2.Warehouse.WrappedObject);
		}

		#endregion

		#region ExpectedDefaultFormatting

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
DispatchConsignment :  is null
DispatchTransportationUnit :  is null
ReceiveASN :  is null
ReceiveConsignment :  is null
ReceiveTransportationUnit :  is null
Registry : (No Default Field Value Available on Registry)
Status : 
Warehouse :  is null
		".Trim();
			}
		}

		#endregion

		#region GetSetupWrapperForDefaultFormatting

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new PackageStateWrapper((WhsItemPackageState)null, Factory);
		}

		#endregion

		#region MasterBill

		public void TestMasterBill()
		{
			var helper = new WhsTransitTestHelper(Factory);

			var warehouse = helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var row = helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			CombineAssertions(() =>
			{
				AssertWrapperMasterBill(1, false, false, false, "");
				AssertWrapperMasterBill(2, false, true, false, "");
				AssertWrapperMasterBill(3, false, true, true, "DLL3's MAB");
				AssertWrapperMasterBill(4, true, false, false, "RCN4's MAB");
				AssertWrapperMasterBill(5, true, true, false, "RCN5's MAB");
				AssertWrapperMasterBill(6, true, true, true, "DLL6's MAB");
			});

			void AssertWrapperMasterBill(int i, bool isRCNhasMasterBill, bool hasDLL, bool isDLLhasMasterbill, ZString expectedMasterBill)
			{
				var rcn = helper.CreateReceiveConsignment($"RCN{i}", warehouse.PK);
				var rtu = helper.CreateReceiveTransportationUnit($"RTU{i}", warehouse.PK, location.PK);
				var package = helper.CreatePackage(rcn.PackageJob, $"PKG{i}", 1, "PKG");
				package.KP_Sequence = 1;
				WhsItemDispatchLoadList dll = null;

				if (isRCNhasMasterBill)
				{
					helper.CreateAdditionalReference(rcn, $"RCN{i}'s MAB", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
				}

				if (hasDLL)
				{
					dll = helper.CreateDispatchLoadList($"DLL{i}", warehouse.PK);

					if (isDLLhasMasterbill)
					{
						helper.CreateAdditionalReference(dll, $"DLL{i}'s MAB", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
					}
				}
				Factory.Save();

				var packageState = helper.CreatePackageState(rcn, 1, "PKG", $"PKGDEP{i}", TransitWarehouseStatuses.Codes.Arrived, dispatchLoadList: dll, receiveUnit: rtu);
				packageState.WPS_KP_Package = package.PK;
				packageState.WPS_WL_LastLocation = location.PK;
				packageState.WPS_WL_ReceiveLocation = location.PK;
				var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
				var packageStateWrapper = new PackageStateWrapper(packageWrapper, Factory);
				AssertEquals($"Package {i} in condition: has RCN MasterBill: {isRCNhasMasterBill}, has package with DLL: {hasDLL}, DLL has MasterBill: {isDLLhasMasterbill}, should have MasterBill = \"{expectedMasterBill}\"", expectedMasterBill, packageStateWrapper.MasterBill);
			}
		}

		#endregion

		#region Location

		public void TestLocation()
		{
			var helper = new WhsTransitTestHelper(Factory);

			var warehouse = helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var row = helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var location2 = row.Locations.First(l => l.ToLocationString() == "Dock-1-2");

			var rcn = helper.CreateReceiveConsignment($"RCN", warehouse.PK);
			var package = helper.CreatePackage(rcn.PackageJob, $"PKG", 1, "PKG");

			var packageState = helper.CreatePackageState(rcn, 1, "PKG", $"PKGDEP", TransitWarehouseStatuses.Codes.Arrived);
			packageState.WPS_KP_Package = package.PK;
			packageState.WPS_WL_LastLocation = location.PK;
			packageState.WPS_WL_ReceiveLocation = location2.PK;
			var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
			var packageStateWrapper = new PackageStateWrapper(packageWrapper, Factory);
			AssertEquals($"PackageState should have location", location.WLV_LocationString, packageStateWrapper.Location);
		}

		#endregion

		#region Status

		public void TestStatus()
		{
			var helper = new WhsTransitTestHelper(Factory);

			var warehouse = helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			var rcn = helper.CreateReceiveConsignment($"RCN", warehouse.PK);
			var package = helper.CreatePackage(rcn.PackageJob, $"PKG", 1, "PKG");
			var packageState = helper.CreatePackageState(rcn, 1, "PKG", $"PKGDEP", TransitWarehouseStatuses.Codes.Arrived);
			packageState.WPS_KP_Package = package.PK;

			var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
			var packageStateWrapper = new PackageStateWrapper(packageWrapper, Factory);
			AssertEquals($"PackageState should have status code", (ZString)TransitWarehouseStatuses.Codes.Arrived, packageStateWrapper.Status.Code);
			AssertEquals($"PackageState should have status description", (ZString)TransitWarehouseStatuses.Descriptions.Arrived, packageStateWrapper.Status.Description);
		}

		#endregion

		#region HasArrived

		public void TestHasArrived()
		{
			var helper = new WhsTransitTestHelper(Factory);

			var warehouse = helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var row = helper.CreateRowAndGenerateLocations(warehouse, "A", 1, 1, 2);
			var location = row.Locations.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1);

			var rtu = helper.CreateReceiveTransportationUnit("RTU001", warehouse.PK, location.PK);
			var rcn = helper.CreateReceiveConsignment($"RCN", warehouse.PK);
			var receivedPackageState = helper.CreatePackageState(rcn, 1, "PKG", $"PKGDEP1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var bookedPackageState = helper.CreatePackageState(rcn, 1, "PKG", $"PKGDEP2", TransitWarehouseStatuses.Codes.Booked);

			var receivedPackageWrapper = new PackageWrapperFromPkgPackage(receivedPackageState.Package, Factory);
			var receivedPackageStateWrapper = new PackageStateWrapper(receivedPackageWrapper, Factory);
			AssertNotEquals(null, receivedPackageState.LastLocation);
			AssertEquals(true, receivedPackageStateWrapper.HasArrived);

			var bookedPackageWrapper = new PackageWrapperFromPkgPackage(bookedPackageState.Package, Factory);
			var bookedPackageStateWrapper = new PackageStateWrapper(bookedPackageWrapper, Factory);
			AssertEquals(null, bookedPackageState.LastLocation);
			AssertEquals(false, bookedPackageStateWrapper.HasArrived);
		}

		public void TestHasArrived_OVP_Booked()
		{
			var helper = new WhsTransitTestHelper(Factory);

			var warehouse = helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var rcn = helper.CreateReceiveConsignment($"RCN", warehouse.PK);

			var packageState1 = helper.CreatePackageState(rcn, 1, "PKG", $"PKGDEP1", TransitWarehouseStatuses.Codes.Booked);
			var packageState2 = helper.CreatePackageState(rcn, 1, "PKG", $"PKGDEP2", TransitWarehouseStatuses.Codes.Booked);

			var ovp = helper.CreateOverpackPackage("OVP001", rcn, receiveUnit: null, rcn: rcn);
			TransitHelper.PackPackageIntoHandlingUnit(ovp, packageState1, ZDateTimeOffset.Now, "ABC", ovp);
			TransitHelper.PackPackageIntoHandlingUnit(ovp, packageState2, ZDateTimeOffset.Now, "ABC", ovp);

			var packageWrapper1 = new PackageWrapperFromPkgPackage(packageState1.Package, Factory);
			var packageStateWrapper1 = new PackageStateWrapper(packageWrapper1, Factory);
			AssertEquals(null, packageState1.LastLocation);
			AssertEquals(false, packageStateWrapper1.HasArrived);

			var packageWrapper2 = new PackageWrapperFromPkgPackage(packageState2.Package, Factory);
			var packageStateWrapper2 = new PackageStateWrapper(packageWrapper2, Factory);
			AssertEquals(null, packageState2.LastLocation);
			AssertEquals(false, packageStateWrapper2.HasArrived);

			var packageWrapperOVP = new PackageWrapperFromPkgPackage(ovp.Package, Factory);
			var packageStateWrapperOVP = new PackageStateWrapper(packageWrapperOVP, Factory);
			AssertEquals(null, ovp.LastLocation);
			AssertEquals(false, packageStateWrapperOVP.HasArrived);
		}

		public void TestHasArrived_OVP_Arrived()
		{
			var helper = new WhsTransitTestHelper(Factory);

			var warehouse = helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var row = helper.CreateRowAndGenerateLocations(warehouse, "A", 1, 1, 2);
			var location = row.Locations.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1);

			var rtu = helper.CreateReceiveTransportationUnit("RTU001", warehouse.PK, location.PK);
			var rcn = helper.CreateReceiveConsignment($"RCN", warehouse.PK);

			var packageState1 = helper.CreatePackageState(rcn, 1, "PKG", $"PKGDEP1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var packageState2 = helper.CreatePackageState(rcn, 1, "PKG", $"PKGDEP2", TransitWarehouseStatuses.Codes.Arrived, rtu);

			var ovp = helper.CreateOverpackPackage("OVP001", rcn, rtu, rcn: rcn);
			ovp.WPS_UnloadedTime = ZDateTimeOffset.Empty;
			TransitHelper.PackPackageIntoHandlingUnit(ovp, packageState1, ZDateTimeOffset.Now, "ABC", ovp);
			TransitHelper.PackPackageIntoHandlingUnit(ovp, packageState2, ZDateTimeOffset.Now, "ABC", ovp);

			var packageWrapper1 = new PackageWrapperFromPkgPackage(packageState1.Package, Factory);
			var packageStateWrapper1 = new PackageStateWrapper(packageWrapper1, Factory);
			AssertNotEquals(null, packageState1.LastLocation);
			AssertEquals(true, packageStateWrapper1.HasArrived);

			var packageWrapper2 = new PackageWrapperFromPkgPackage(packageState2.Package, Factory);
			var packageStateWrapper2 = new PackageStateWrapper(packageWrapper2, Factory);
			AssertNotEquals(null, packageState2.LastLocation);
			AssertEquals(true, packageStateWrapper2.HasArrived);

			var packageWrapperOVP = new PackageWrapperFromPkgPackage(ovp.Package, Factory);
			var packageStateWrapperOVP = new PackageStateWrapper(packageWrapperOVP, Factory);
			AssertNotEquals(null, ovp.LastLocation);
			AssertEquals(true, packageStateWrapperOVP.HasArrived);
		}

		#endregion

		#region HasDeparted

		public void TestHasDeparted()
		{
			var helper = new WhsTransitTestHelper(Factory);

			var warehouse = helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			var rcn = helper.CreateReceiveConsignment($"RCN", warehouse.PK);
			var package = helper.CreatePackage(rcn.PackageJob, $"PKG", 1, "PKG");
			var receivedPackageState = helper.CreatePackageState(rcn, 1, "PKG", $"PKGDEP1", TransitWarehouseStatuses.Codes.Arrived);
			receivedPackageState.WPS_KP_Package = package.PK;
			receivedPackageState.WPS_UnloadedTime = ZDateTimeOffset.Now;

			var dtu = helper.CreateDispatchTransportationUnit("DTU", warehouse.PK);
			dtu.WDH_GateOutTime = ZDateTimeOffset.Now;
			var package2 = helper.CreatePackage(rcn.PackageJob, $"PKG", 1, "PKG");
			var departedPackageState = helper.CreatePackageState(rcn, 1, "PKG", $"PKGDEP2", TransitWarehouseStatuses.Codes.Departed, dispatchUnit: dtu);
			departedPackageState.WPS_KP_Package = package2.PK;

			var receivedPackageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
			var receivedPackageStateWrapper = new PackageStateWrapper(receivedPackageWrapper, Factory);
			AssertEquals(false, receivedPackageStateWrapper.HasDeparted);

			var departedPackageWrapper = new PackageWrapperFromPkgPackage(package2, Factory);
			var departedPackageStateWrapper = new PackageStateWrapper(departedPackageWrapper, Factory);
			AssertEquals(true, departedPackageStateWrapper.HasDeparted);
		}

		#endregion

		#region TestJobType

		public void TestJobType()
		{
			var package = Factory.New<PkgPackage>();
			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_KP_Package = package.PK;
			packageState.WPS_UnitType = TransportUnitTypes.ULD;

			var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
			var packageStateWrapper = new PackageStateWrapper(packageWrapper, Factory);

			AssertEquals(TransportUnitTypes.ULD, packageStateWrapper.JobType);
		}

		#endregion

		#region TestDefaultValuesWhenNullObject

		public void TestDefaultValuesWhenNullObject_PackageWrapperFromPkgPackage()
		{
			var packageStateWrapper = new PackageStateWrapper((PackageWrapperFromPkgPackage)null, Factory);

			AssertNull(packageStateWrapper.ReceiveConsignment);
			AssertNull(packageStateWrapper.ReceiveASN);
			AssertNull(packageStateWrapper.Warehouse);
			AssertNull(null, packageStateWrapper.DispatchConsignment);
			AssertNull(null, packageStateWrapper.DispatchTransportationUnit);
			AssertNull(null, packageStateWrapper.ReceiveTransportationUnit);
			AssertNull(null, packageStateWrapper.PlannedReceiveTransportationUnits);
			AssertNullOrEmpty(ZString.Empty, packageStateWrapper.ConsignmentID);
			AssertNullOrEmpty(ZString.Empty, packageStateWrapper.MasterBill);
			AssertNullOrEmpty(ZString.Empty, packageStateWrapper.Location);
			AssertNullOrEmpty(ZString.Empty, packageStateWrapper.JobType);
			AssertEquals(ZDateTimeOffset.Empty.ToLocalZDateTime(), packageStateWrapper.Arrived);
			AssertEquals(false, packageStateWrapper.HasArrived);
			AssertEquals(ZDateTimeOffset.Empty.ToLocalZDateTime(), packageStateWrapper.Departed);
			AssertEquals(false, packageStateWrapper.HasDeparted);
			AssertEquals(CodeAndDescriptionWrapper.Empty.Code, packageStateWrapper.Status.Code);
			AssertEquals(false, packageStateWrapper.IsPicked);
			AssertEquals(false, packageStateWrapper.IsLoaded);
			AssertEquals(false, packageStateWrapper.IsDamaged);
			AssertEquals(false, packageStateWrapper.IsHandlingUnit);
			AssertEquals(false, packageStateWrapper.IsHighRisk);
			AssertEquals(false, packageStateWrapper.IsHighRiskAuthorized);
		}

		public void TestDefaultValuesWhenNullObject_WhsItemPackageState()
		{
			var packageStateWrapper = new PackageStateWrapper((WhsItemPackageState)null, Factory);

			AssertNull(packageStateWrapper.ReceiveConsignment);
			AssertNull(packageStateWrapper.ReceiveASN);
			AssertNull(packageStateWrapper.Warehouse);
			AssertNull(null, packageStateWrapper.DispatchConsignment);
			AssertNull(null, packageStateWrapper.DispatchTransportationUnit);
			AssertNull(null, packageStateWrapper.ReceiveTransportationUnit);
			AssertNull(null, packageStateWrapper.PlannedReceiveTransportationUnits);
			AssertNullOrEmpty(ZString.Empty, packageStateWrapper.ConsignmentID);
			AssertNullOrEmpty(ZString.Empty, packageStateWrapper.MasterBill);
			AssertNullOrEmpty(ZString.Empty, packageStateWrapper.Location);
			AssertNullOrEmpty(ZString.Empty, packageStateWrapper.JobType);
			AssertEquals(ZDateTimeOffset.Empty.ToLocalZDateTime(), packageStateWrapper.Arrived);
			AssertEquals(false, packageStateWrapper.HasArrived);
			AssertEquals(ZDateTimeOffset.Empty.ToLocalZDateTime(), packageStateWrapper.Departed);
			AssertEquals(false, packageStateWrapper.HasDeparted);
			AssertEquals(CodeAndDescriptionWrapper.Empty.Code, packageStateWrapper.Status.Code);
			AssertEquals(false, packageStateWrapper.IsPicked);
			AssertEquals(false, packageStateWrapper.IsLoaded);
			AssertEquals(false, packageStateWrapper.IsDamaged);
			AssertEquals(false, packageStateWrapper.IsHandlingUnit);
			AssertEquals(false, packageStateWrapper.IsHighRisk);
			AssertEquals(false, packageStateWrapper.IsHighRiskAuthorized);
		}

		#endregion

		#region ExpectedFieldMap

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
PackageState
======================================================================
Name                                    Type
----------------------------------------------------------------------
Status                                  CodeAndDescription
DispatchConsignment                     Freight
DispatchTransportationUnit              Freight
ReceiveASN                              Freight
ReceiveConsignment                      Freight
ReceiveTransportationUnit               Freight
Warehouse                               WarehouseBO
Arrived                                 DateTime
ConsignmentID                           String
CrossDockTransferPickTime               DateTime
CrossDockTransferPutTime                DateTime
Departed                                DateTime
DispatchConsignmentID                   String
HasArrived                              Bool
HasDeparted                             Bool
IsDamaged                               Bool
IsHandlingUnit                          Bool
IsHighRisk                              Bool
IsHighRiskAuthorized                    Bool
IsLoaded                                Bool
IsPicked                                Bool
JobType                                 String
LoadedTime                              DateTime
Location                                String
MasterBill                              String
PickTransferPickTime                    DateTime
PickTransferPutTime                     DateTime
PutawayTransferPickTime                 DateTime
PutawayTransferPutTime                  DateTime
UnloadedNotYetProcessedTime             DateTime

PlannedReceiveTransportationUnits       Freight Collection".Trim();
			}
		}

		#endregion

		#region GetNewDocumentWrapper

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PackageStateWrapper(Factory.GetNull<WhsItemPackageState>(), Factory);
		}

		#endregion

		#region TestLoadedTime

		public void TestLoadedTime_ReturnsCorrectLoadedTime()
		{
			var expectedTime = new ZDateTimeOffset(2025, 3, 31, 18, 0, 0);

			var package = Factory.New<PkgPackage>();
			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_KP_Package = package.PK;
			packageState.WPS_LoadedTime = expectedTime;

			var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
			var wrapper = new PackageStateWrapper(packageWrapper, Factory);

			var result = wrapper.LoadedTime;

			AssertEquals(expectedTime.ToLocalZDateTime(), result);
		}

		#endregion

		#region TestUnloadedNotYetProcessedTime

		public void TestUnloadedNotYetProcessedTime_ReturnsCorrectTime()
		{
			var expectedTime = new ZDateTimeOffset(2025, 3, 31, 18, 0, 0);

			var package = Factory.New<PkgPackage>();
			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_KP_Package = package.PK;
			packageState.WPS_UnloadedNotYetProcessedTime = expectedTime;

			var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
			var wrapper = new PackageStateWrapper(packageWrapper, Factory);

			var result = wrapper.UnloadedNotYetProcessedTime;

			AssertEquals(expectedTime.ToLocalZDateTime(), result);
		}

		#endregion

		#region TransferRelatedTimesTestSetup

		(ZDateTime expectedTime, PackageStateWrapper stateWrapper) SetupTransferPickTime(string transferType)
		{
			var helper = new WhsTransitTestHelper(Factory);

			var warehouse = helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var location = warehouse.Rows.First().Locations.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1);

			var rtu = helper.CreateReceiveTransportationUnit("RTU001", warehouse.PK, location.PK);
			var rcn = helper.CreateReceiveConsignment("RCN", warehouse.PK);

			var receivedPackageState = helper.CreatePackageState(
				rcn, 1, "PKG", "PKGDEP1", TransitWarehouseStatuses.Codes.Arrived, rtu
			);
			var package = receivedPackageState.Package;

			var transferHeader = Factory.New<WhsItemTransferHeader>();
			transferHeader.WTH_TransferType = transferType;
			transferHeader.WTH_ReferenceNumber = "123";
			transferHeader.WTH_WW_Warehouse = warehouse.PK;

			var expectedTime = new ZDateTimeOffset(2025, 3, 31, 10, 0, 0);

			var transferLine = Factory.New<WhsItemTransferLine>();
			transferLine.WTF_PickTime = expectedTime;
			transferLine.WTF_WL_From = location.PK;
			transferLine.WTF_WL_To = location.PK;
			transferLine.WTF_WPS_PackageState = receivedPackageState.PK;
			transferLine.WTF_GS_NKPickUser = "TST";
			transferHeader.Lines.Add(transferLine);

			Factory.Save();

			var receivedPackageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
			var receivedPackageStateWrapper = new PackageStateWrapper(receivedPackageWrapper, Factory);

			return (expectedTime.ToLocalZDateTime(), receivedPackageStateWrapper);
		}

		(ZDateTime expectedTime, PackageStateWrapper stateWrapper) SetupTransferPutTime(string transferType)
		{
			var helper = new WhsTransitTestHelper(Factory);

			var warehouse = helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var location1 = warehouse.Rows[0].Locations.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1);
			var location2 = warehouse.Rows[1].Locations.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1);

			var rtu = helper.CreateReceiveTransportationUnit("RTU001", warehouse.PK, location1.PK);
			var rcn = helper.CreateReceiveConsignment("RCN", warehouse.PK);

			var receivedPackageState = helper.CreatePackageState(
				rcn, 1, "PKG", "PKGDEP1", TransitWarehouseStatuses.Codes.Arrived, rtu
			);
			var package = receivedPackageState.Package;

			var transferHeader = Factory.New<WhsItemTransferHeader>();
			transferHeader.WTH_TransferType = transferType;
			transferHeader.WTH_ReferenceNumber = "123";
			transferHeader.WTH_WW_Warehouse = warehouse.PK;

			var expectedTime = new ZDateTimeOffset(2025, 3, 31, 10, 0, 0);

			var transferLine = Factory.New<WhsItemTransferLine>();
			transferLine.WTF_PickTime = expectedTime.AddHours(-5);
			transferLine.WTF_PutTime = expectedTime;
			transferLine.WTF_WL_From = location1.PK;
			transferLine.WTF_WL_To = location2.PK;
			transferLine.WTF_WPS_PackageState = receivedPackageState.PK;
			transferLine.WTF_GS_NKPutUser = "TST";
			transferLine.WTF_GS_NKPickUser = "TST";
			transferHeader.Lines.Add(transferLine);

			Factory.Save();

			var receivedPackageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
			var receivedPackageStateWrapper = new PackageStateWrapper(receivedPackageWrapper, Factory);

			return (expectedTime.ToLocalZDateTime(), receivedPackageStateWrapper);
		}

		#endregion

		#region TestPutawayTransferTimes

		public void TestPutawayTransferPickedTime_ReturnsCorrectTime()
		{
			var (expectedTime, stateWrapper) = SetupTransferPickTime(TransferTypes.Codes.PUT);
			AssertEquals(expectedTime, stateWrapper.PutawayTransferPickTime);
		}

		public void TestPutawayTransferPutTime_ReturnsCorrectTime()
		{
			var (expectedTime, stateWrapper) = SetupTransferPutTime(TransferTypes.Codes.PUT);
			AssertEquals(expectedTime, stateWrapper.PutawayTransferPutTime);
		}

		#endregion

		#region TestCrossdockTransferTimes

		public void TestCrossdockTransferPickedTime_ReturnsCorrectTime()
		{
			var (expectedTime, stateWrapper) = SetupTransferPickTime(TransferTypes.Codes.XDK);
			AssertEquals(expectedTime, stateWrapper.CrossDockTransferPickTime);
		}

		public void TestCrossdockTransferPutTime_ReturnsCorrectTime()
		{
			var (expectedTime, stateWrapper) = SetupTransferPutTime(TransferTypes.Codes.XDK);
			AssertEquals(expectedTime, stateWrapper.CrossDockTransferPutTime);
		}

		#endregion

		#region TestPickTransferTimes

		public void TestPickTransferPickedTime_ReturnsCorrectTime()
		{
			var (expectedTime, stateWrapper) = SetupTransferPickTime(TransferTypes.Codes.PIC);
			AssertEquals(expectedTime, stateWrapper.PickTransferPickTime);
		}

		public void TestPickTransferPutTime_ReturnsCorrectTime()
		{
			var (expectedTime, stateWrapper) = SetupTransferPutTime(TransferTypes.Codes.PIC);
			AssertEquals(expectedTime, stateWrapper.PickTransferPutTime);
		}

		#endregion

		#region TestDispatchConsignmentID

		public void TestDispatchConsignmentID()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var warehouse = helper.CreateTRWWarehouse();
			var location = helper.CreateLocation(warehouse);
			var rcn = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dcn = helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var rtu = helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageStateOnlyRCN = helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked);
			var wrapper = new PackageStateWrapper(packageStateOnlyRCN, Factory);
			AssertNullOrEmpty(wrapper.DispatchConsignmentID);

			var packageStateOnlyDCN = helper.CreateOverpackPackage("OVP1", dcn, rtu);
			packageStateOnlyDCN.WPS_WDC_TransitDispatchConsignment = dcn.PK;
			wrapper = new PackageStateWrapper(packageStateOnlyDCN, Factory);
			AssertEquals("DCN1", wrapper.DispatchConsignmentID);

			var packageStateBothRCNAndDCN = helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			wrapper = new PackageStateWrapper(packageStateBothRCNAndDCN, Factory);
			AssertEquals("DCN1", wrapper.DispatchConsignmentID);
		}

		#endregion

		WhsTransitTestHelper TransitHelper => transitHelper ?? (transitHelper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper transitHelper;
	}
}

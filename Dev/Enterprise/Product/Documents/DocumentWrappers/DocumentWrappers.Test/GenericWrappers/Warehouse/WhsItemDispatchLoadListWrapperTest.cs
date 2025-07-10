using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WhsItemDispatchLoadListWrapper))]
	sealed class WhsItemDispatchLoadListWrapperTest : WarehouseJobGenericWrapperTest
	{
		#region TestJobNumberHeadingCore

		protected override void TestJobNumberHeadingCore()
		{
			AssertEquals("Dispatch Load List ID", DispatchLoadListWrapper.JobNumberHeading);
		}

		#endregion

		#region TestJobNumberCore

		protected override void TestJobNumberCore()
		{
			DispatchLoadList.WDL_JobID = "WDL010101";

			AssertEquals("WDL010101", DispatchLoadListWrapper.JobNumber);
		}

		#endregion

		#region TestSecondaryHeadingCore

		protected override void TestSecondaryHeadingCore()
		{
			AssertEquals("Dispatch Load List Reference Number", DispatchLoadListWrapper.SecondaryHeading);
		}

		#endregion

		#region TestSecondaryNumberCore

		protected override void TestSecondaryNumberCore()
		{
			DispatchLoadList.WDL_ReferenceNumber = "WDL010101";

			AssertEquals("WDL010101", DispatchLoadListWrapper.SecondaryNumber);
		}

		#endregion

		#region TestStagingLocationStringCore

		protected override void TestStagingLocationStringCore()
		{
			var location = Factory.NewWithValidTestData<WhsLocation>();
			AssertEquals("Staging Location should be empty When Packing Group is null.", string.Empty, DispatchLoadListWrapper.StagingLocationString.ToString());

			DispatchLoadList.WDL_WL_StagingLocation = location.PK;
			AssertEquals("Staging Location should have a value.", location.WLV_LocationString, DispatchLoadListWrapper.StagingLocationString.ToString());
		}

		#endregion

		#region TestMasterBillCore

		protected override void TestMasterBillCore()
		{
			var warehouse = TransitHelper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dispatchLoadList = TransitHelper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var wrapper = new WhsItemDispatchLoadListWrapper(dispatchLoadList, Factory);
			AssertEquals("Load List has no Master Bill", "", wrapper.MasterBill);

			var dispatchLoadList2 = TransitHelper.CreateDispatchLoadList("DLL2", warehouse.PK);
			TransitHelper.CreateAdditionalReference(dispatchLoadList2, "123123", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			Factory.Save();

			var wrapper2 = new WhsItemDispatchLoadListWrapper(dispatchLoadList2, Factory);
			AssertEquals("Load List has Master Bill", "123123", wrapper2.MasterBill);
		}

		#endregion

		#region TestDestination

		protected override void TestDestinationCore()
		{
			var portCode = new ZString("NZTST");
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dispatchLoadList = TransitHelper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dispatchLoadList.WDL_RL_NKLastDischargePort = portCode;
			var wrapper = new WhsItemDispatchLoadListWrapper(dispatchLoadList, Factory);

			Factory.Save();

			AssertEquals("Load List has Last Discharge Port", portCode, wrapper.Destination.Location.PortName);
		}

		#endregion

		#region TestMasterBillHeadingCore

		protected override void TestMasterBillHeadingCore()
		{
			var warehouse = TransitHelper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dispatchLoadList = TransitHelper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var wrapper = new WhsItemDispatchLoadListWrapper(dispatchLoadList, Factory);
			AssertEquals("Default Bill Heading is Master Bill", "Master Bill", wrapper.MasterBillHeading);

			var dispatchLoadList2 = TransitHelper.CreateDispatchLoadList("DLL2", warehouse.PK);
			dispatchLoadList2.WDL_TransportMode = TransportModeList.Codes.Airfreight;
			Factory.Save();

			var wrapper2 = new WhsItemDispatchLoadListWrapper(dispatchLoadList2, Factory);
			AssertEquals("Bill Heading for Air is MAWB", "MAWB", wrapper2.MasterBillHeading);
		}

		#endregion

		#region TestAssignedLoaderCore

		protected override void TestAssignedLoaderCore()
		{
			var warehouse = TransitHelper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dispatchLoadList = TransitHelper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var wrapper = new WhsItemDispatchLoadListWrapper(dispatchLoadList, Factory);
			AssertEquals("Load List has no Assigned Loader", "", wrapper.AssignedLoader);

			var dispatchLoadList2 = TransitHelper.CreateDispatchLoadList("DLL2", warehouse.PK);
			TransitHelper.CreateAdditionalReference(dispatchLoadList2, "ABC", WarehouseAdditionalReferenceTypes.Codes.AssignedPicker);
			Factory.Save();

			var wrapper2 = new WhsItemDispatchLoadListWrapper(dispatchLoadList2, Factory);
			AssertEquals("Load List has Assigned Loader", "ABC", wrapper2.AssignedLoader);
		}

		#endregion

		#region TestDispatchTransportationUnitsCore

		protected override void TestDispatchTransportationUnitsCore()
		{
			var warehouse = TransitHelper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dll = TransitHelper.CreateDispatchLoadList("DLL", warehouse.PK);
			var dtu = TransitHelper.CreateDispatchTransportationUnit("DTU", warehouse.PK);
			var pivot = Factory.New<WhsItemDispatchLoadListDTUPivot>();
			pivot.WLD_WDL_TransitDispatchLoadList = dll.PK;
			pivot.WLD_WDH_TransitDispatchTransportationUnit = dtu.PK;
			Factory.Save();

			var wrapper = new WhsItemDispatchLoadListWrapper(dll, Factory);
			AssertEquals("Load List has DTU List", 1, wrapper.DispatchTransportationUnits.Count);
			AssertEquals("Load List has DTU List", "DTU", ((WhsItemDispatchTransportationUnitWrapper)wrapper.DispatchTransportationUnits.FirstOrDefault()).JobNumber);
		}

		#endregion

		#region TestJobType

		protected override void TestJobTypeCore()
		{
			AssertEquals(TransportUnitTypes.None, DispatchLoadListWrapper.JobType);
		}

		public void TestJobTypeIsVEH()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var dtu1 = TransitHelper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dtu2 = TransitHelper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			var dll = TransitHelper.CreateDispatchLoadList("DLL", warehouse.PK);
			TransitHelper.CreateDispatchDLLDTUPivot(dll.PK, dtu1.PK);
			TransitHelper.CreateDispatchDLLDTUPivot(dll.PK, dtu2.PK);

			Factory.Save();

			var wrapper = new WhsItemDispatchLoadListWrapper(dll, Factory);
			AssertEquals(TransportUnitTypes.Vehicle, wrapper.JobType);
		}

		public void TestJobTypeIsCNT()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var dtu1 = TransitHelper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK, "CNT1", "20GP");
			var dtu2 = TransitHelper.CreateDispatchTransportationUnitWithContainerType("DTU2", warehouse.PK, "CNT2", "20GP");
			var dll = TransitHelper.CreateDispatchLoadList("DLL", warehouse.PK);
			TransitHelper.CreateDispatchDLLDTUPivot(dll.PK, dtu1.PK);
			TransitHelper.CreateDispatchDLLDTUPivot(dll.PK, dtu2.PK);

			Factory.Save();

			var wrapper = new WhsItemDispatchLoadListWrapper(dll, Factory);
			AssertEquals(TransportUnitTypes.Container, wrapper.JobType);
		}

		public void TestJobTypeIsULD()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var dtu1 = TransitHelper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK, "ULD1", "AAA");
			var dtu2 = TransitHelper.CreateDispatchTransportationUnitWithContainerType("DTU2", warehouse.PK, "ULD2", "AAA");
			var dll = TransitHelper.CreateDispatchLoadList("DLL", warehouse.PK);
			TransitHelper.CreateDispatchDLLDTUPivot(dll.PK, dtu1.PK);
			TransitHelper.CreateDispatchDLLDTUPivot(dll.PK, dtu2.PK);

			Factory.Save();

			var wrapper = new WhsItemDispatchLoadListWrapper(dll, Factory);
			AssertEquals(TransportUnitTypes.ULD, wrapper.JobType);
		}

		public void TestJobTypeIsMIX()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var dtu1 = TransitHelper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK, "ULD1", "AAA");
			var dtu2 = TransitHelper.CreateDispatchTransportationUnitWithContainerType("DTU2", warehouse.PK, "CNT2", "20GP");
			var dll = TransitHelper.CreateDispatchLoadList("DLL", warehouse.PK);
			TransitHelper.CreateDispatchDLLDTUPivot(dll.PK, dtu1.PK);
			TransitHelper.CreateDispatchDLLDTUPivot(dll.PK, dtu2.PK);

			Factory.Save();

			var wrapper = new WhsItemDispatchLoadListWrapper(dll, Factory);
			AssertEquals(TransportUnitTypes.Mix, wrapper.JobType);
		}

		#endregion

		#region TestTransportMode

		protected override void TestTransportModeCore()
		{
			DispatchLoadList.WDL_TransportMode = TransportModeList.Codes.Airfreight;

			AssertEquals(TransportModeList.Codes.Airfreight, DispatchLoadListWrapper.TransportMode);
		}

		#endregion

		#region TestCutoffTime

		protected override void TestCutoffTimeCore()
		{
			var dateTime = ZDateTimeOffset.Now;
			DispatchLoadList.WDL_CTOCutOffTime = dateTime;

			AssertEquals(dateTime.ToLocalZDateTime(), DispatchLoadListWrapper.CutoffTime);
		}

		#endregion

		#region TestExpectedDispatchTime

		protected override void TestExpectedDispatchTimeCore()
		{
			var dateTime = ZDateTimeOffset.Now;
			DispatchLoadList.WDL_ExpectedDispatchTime = dateTime;

			AssertEquals(dateTime.ToLocalZDateTime(), DispatchLoadListWrapper.ExpectedDispatchTime);
		}

		#endregion

		#region TestCompleteTime

		protected override void TestCompleteTimeCore()
		{
			var dateTime = ZDateTimeOffset.Now;
			DispatchLoadList.WDL_CompleteTime = dateTime;

			AssertEquals(dateTime.ToLocalZDateTime(), DispatchLoadListWrapper.CompleteTime);
		}

		#endregion

		#region TestAllDCNsAreAuthorized

		protected override void TestAllDCNsAreAuthorizedCore()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var dll = TransitHelper.CreateDispatchLoadList("DLL", warehouse.PK);
			var dcn = TransitHelper.CreateDispatchConsignment("DC", warehouse.PK, "STD");
			dcn.WDC_IsAuthorizedForDispatch = ZBool.True;
			var rcn = transitHelper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			var inBoundLocation = transitHelper.CreateLocation(warehouse, "INB");
			var rtu = transitHelper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, inBoundLocation.PK);
			var packagestate = transitHelper.CreatePackageState(rcn, 1, "PKG", "HUT1_P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll);

			Factory.Save();

			var wrapper = new WhsItemDispatchLoadListWrapper(dll, Factory);
			AssertEquals(ZBool.True, wrapper.AllDCNsAreAuthorized);
		}

		#endregion

		#region TestIsAwaitingForwardingChanges

		protected override void TestIsAwaitingForwardingChangesCore()
		{
			DispatchLoadList.WDL_IsAwaitingForwardingChanges = ZBool.True;

			AssertEquals(ZBool.True, DispatchLoadListWrapper.IsAwaitingForwardingChanges);
		}
		#endregion

		#region TestIsReadyToStage

		protected override void TestIsReadyToStageCore()
		{
			DispatchLoadList.WDL_IsReadyToStage = ZBool.True;

			AssertEquals(ZBool.True, DispatchLoadListWrapper.IsReadyToStage);
		}

		#endregion

		#region TestTotalInnerPackLines

		protected override void TestTotalInnerPackLinesCore()
		{
			var warehouse = TransitHelper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			var dcn = TransitHelper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var rcn = TransitHelper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dll = TransitHelper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var rtu = TransitHelper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var hu1Job = TransitHelper.CreatePackageHandlingUnit();
			var hu1 = TransitHelper.CreateHandlingUnitPackage("HU1", hu1Job, rtu, rcn: rcn, dcn: dcn);
			var innerPackline1 = TransitHelper.CreatePackageState(rcn, 10, "PKL", ZString.Empty, TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.PackLine, dispatchLoadList: dll);
			var innerPackline2 = TransitHelper.CreatePackageState(rcn, 10, "PKL", ZString.Empty, TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.PackLine, dispatchLoadList: dll);
			TransitHelper.PackPackageIntoHandlingUnit(hu1, innerPackline1, ZDateTimeOffset.Now, "ABC", hu1);
			TransitHelper.PackPackageIntoHandlingUnit(hu1, innerPackline2, ZDateTimeOffset.Now, "ABC", hu1);

			var hu2Job = TransitHelper.CreatePackageHandlingUnit();
			var hu2 = TransitHelper.CreateHandlingUnitPackage("HU2", hu2Job, rtu, rcn: rcn, dcn: dcn);
			var innerScannedPackage = TransitHelper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.Package, dispatchLoadList: dll);
			var innerNonScannedPackage = TransitHelper.CreatePackageState(rcn, 1, "PKG", ZString.Empty, TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.Package, dispatchLoadList: dll);
			TransitHelper.PackPackageIntoHandlingUnit(hu2, innerScannedPackage, ZDateTimeOffset.Now, "ABC", hu2);
			TransitHelper.PackPackageIntoHandlingUnit(hu2, innerNonScannedPackage, ZDateTimeOffset.Now, "ABC", hu2);

			TransitHelper.CreatePackageState(rcn, 1, "PKL", "PKG2", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.PackLine, dispatchLoadList: dll);

			var wrapper = new WhsItemDispatchLoadListWrapper(dll, Factory);
			AssertEquals((short)21, wrapper.TotalInnerPackLines);
		}

		#endregion

		#region TestTotalInners

		protected override void TestTotalInnersCore()
		{
			var warehouse = TransitHelper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			var dcn = TransitHelper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var rcn = TransitHelper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dll = TransitHelper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var rtu = TransitHelper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var hu1Job = TransitHelper.CreatePackageHandlingUnit();
			var hu1 = TransitHelper.CreateHandlingUnitPackage("HU1", hu1Job, rtu, rcn: rcn, dcn: dcn);
			var innerPackline1 = TransitHelper.CreatePackageState(rcn, 10, "PKL", ZString.Empty, TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.PackLine, dispatchLoadList: dll);
			var innerPackline2 = TransitHelper.CreatePackageState(rcn, 10, "PKL", ZString.Empty, TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.PackLine, dispatchLoadList: dll);
			TransitHelper.PackPackageIntoHandlingUnit(hu1, innerPackline1, ZDateTimeOffset.Now, "ABC", hu1);
			TransitHelper.PackPackageIntoHandlingUnit(hu1, innerPackline2, ZDateTimeOffset.Now, "ABC", hu1);

			var hu2Job = TransitHelper.CreatePackageHandlingUnit();
			var hu2 = TransitHelper.CreateHandlingUnitPackage("HU2", hu2Job, rtu, rcn: rcn, dcn: dcn);
			var innerScannedPackage = TransitHelper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.Package, dispatchLoadList: dll);
			var innerNonScannedPackage = TransitHelper.CreatePackageState(rcn, 1, "PKG", ZString.Empty, TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.Package, dispatchLoadList: dll);
			TransitHelper.PackPackageIntoHandlingUnit(hu2, innerScannedPackage, ZDateTimeOffset.Now, "ABC", hu2);
			TransitHelper.PackPackageIntoHandlingUnit(hu2, innerNonScannedPackage, ZDateTimeOffset.Now, "ABC", hu2);

			TransitHelper.CreatePackageState(rcn, 1, "PKL", "PKG2", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.PackLine, dispatchLoadList: dll);

			var wrapper = new WhsItemDispatchLoadListWrapper(dll, Factory);
			AssertEquals((short)22, wrapper.TotalInners);
		}

		#endregion

		#region TestTotalOverpack

		protected override void TestTotalOverpacksCore()
		{
			var warehouse = TransitHelper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			var dcn = TransitHelper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var rcn = TransitHelper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dll = TransitHelper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var rtu = TransitHelper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var ovp1_top = TransitHelper.CreateOverpackPackage("OVP1-Top", rcn, rtu, TransitWarehouseStatuses.Codes.Arrived, rcn: rcn, dcn: dcn, unitType: PackageStateUnitType.Codes.Overpack, dll: dll);
			var ovp1_innerOvp = TransitHelper.CreateOverpackPackage("OVP1-InnerOvp", rcn, rtu, TransitWarehouseStatuses.Codes.Arrived, rcn: rcn, dcn: dcn, unitType: PackageStateUnitType.Codes.Overpack, dll: dll);
			var ovp1Package1 = TransitHelper.CreatePackageState(rcn, 1, "PKG", "OVP1-1", TransitWarehouseStatuses.Codes.Arrived, rtu, dispatchLoadList: dll);
			transitHelper.PackPackageIntoHandlingUnit(ovp1_top, ovp1Package1, ZDateTimeOffset.Now, "ABC", ovp1_top);
			transitHelper.PackPackageIntoHandlingUnit(ovp1_top, ovp1_innerOvp, ZDateTimeOffset.Now, "ABC", ovp1_top);

			var ovp2 = TransitHelper.CreateOverpackPackage("OVP2-Top", rcn, rtu, TransitWarehouseStatuses.Codes.Arrived, rcn: rcn, dcn: dcn, unitType: PackageStateUnitType.Codes.Overpack, dll: dll);
			var ovp2Package = TransitHelper.CreatePackageState(rcn, 1, "PKG", "OVP2-1", TransitWarehouseStatuses.Codes.Arrived, rtu, dispatchLoadList: dll);
			TransitHelper.PackPackageIntoHandlingUnit(ovp2, ovp2Package, ZDateTimeOffset.Now, "ABC", ovp2);

			TransitHelper.CreatePackageState(rcn, 1, "PKL", "PKG2", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.Package, dispatchLoadList: dll);

			var hu1Job = TransitHelper.CreatePackageHandlingUnit();
			var hu1 = TransitHelper.CreateHandlingUnitPackage("HU1", hu1Job, rtu, rcn: rcn);
			var ovp3 = TransitHelper.CreateOverpackPackage("OVP1-Top", rcn, rtu, TransitWarehouseStatuses.Codes.Arrived, rcn: rcn, dcn: dcn, unitType: PackageStateUnitType.Codes.Overpack, dll: dll);
			transitHelper.PackPackageIntoHandlingUnit(hu1, ovp3, ZDateTimeOffset.Now, "ABC", hu1);

			var wrapper = new WhsItemDispatchLoadListWrapper(dll, Factory);
			AssertEquals((short)3, wrapper.TotalOverpacks);
		}

		#endregion

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
CarrierAccount :  is null
CarrierServiceLevel :  is null
Client :  is null
ClientRequestedBillToParty :  is null
CODAmount :  is null
CODType :  is null
ConfirmationInstructions : 
Consignee :  is null
ConsigneeAddress :  is null
Consignor :  is null
CubicSent :  is null
CustomerReference : 
CustomsStatus :  is null
Destination :  is null
DistributionCentreAddress :  is null
DropMode :  is null
DropOffAddress :  is null
FinalisedDate : 
Forwarder :  is null
FulfillRule :  is null
GoodsBillToAddress :  is null
HandlingInstructions : 
IncoTerm :  is null
Insurance :  is null
JobClient :  is null
PackagesSent :  is null
PickingInstructions : 
PickMethod : 
PickNo : 
PickNumberReference : 
PickOption :  is null
PickUpAddress :  is null
PrimaryBarcode : 
Registry : (No Default Field Value Available on Registry)
RequiredDate : 
SalesChannel :  is null
SecondaryReference : 
ServiceLevel :  is null
SOPCarrierServiceLevel : 
SOPOrderNumber : 
SOPRequiredDate : 
SOPSpecialInstructions : 
SOPStagingAreaName : 
SOPTransportCompany : 
SplitNumber : 
StagingAreaName : 
StagingLocationString : 
Status : 
Supplier :  is null
SupplierBuyerLink :  is null
SupplierDocAddress :  is null
TotalExtendedLinePrice :  is null
TotalLoadedPackages : 
TotalLoadedUnits : 
TotalLoadedVolume : 
TotalLoadedWeight : 
TotalOuterPackagesVolume : 
TotalOuterPackagesWeight : 
TransportationUnit :  is null
TransportBillToAddress :  is null
TransportCoAddress :  is null
TransportCompany :  is null
TransportReference : 
VehicleReference : 
Warehouse :  is null
WarehouseName : 
WeightSent :  is null
WhoCreated : 
WhoFinalised :
";
			}
		}

		public override void TestWrapperMappingsEmpty()
		{
		}

		#region TestWarehouseBOWrapper_FactoryCached

		protected override void SetWhsBusinessObjectWarehouse(BusinessObject bizO, ZGuid warehousePK)
		{
			((WhsItemDispatchLoadList)bizO).WDL_WW_Warehouse = warehousePK;
		}

		protected override BusinessObject GetNewWhsBusinessObjectInSpecifiedFactory(BusinessObjectFactory factory)
		{
			return factory.New<WhsItemDispatchLoadList>();
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapperInSpecifiedFactory(BusinessObject bizO, BusinessObjectFactory factory)
		{
			return new WhsItemDispatchLoadListWrapper((WhsItemDispatchLoadList)bizO, factory);
		}

		#endregion

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WhsItemDispatchLoadListWrapper((WhsItemDispatchLoadList)bizO, Factory);
		}

		protected override BusinessObject GetNewWhsBusinessObject()
		{
			return Factory.New<WhsItemDispatchLoadList>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new WhsItemDispatchLoadListWrapper(DispatchLoadList, Factory);
		}

		#endregion

		#region TestIsFinalised

		protected override void TestIsFinalisedCore()
		{
			DispatchLoadList.IsFinalised = ZBool.True;
			AssertEquals(ZBool.True, DispatchLoadListWrapper.IsFinalised);
		}

		#endregion

		WhsTransitTestHelper TransitHelper => transitHelper ?? (transitHelper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper transitHelper;

		WhsItemDispatchLoadList DispatchLoadList => (WhsItemDispatchLoadList)WhsBusinessObject;

		WhsItemDispatchLoadListWrapper DispatchLoadListWrapper => (WhsItemDispatchLoadListWrapper)Wrapper;
	}
}

using System.IO;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Barcode.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WhsItemReceiveTransportationUnitWrapper))]
	sealed class WhsItemReceiveTransportationUnitWrapperTest : WarehouseJobGenericWrapperTest
	{
		#region TestWarehouseNameCore

		protected override void TestWarehouseNameCore()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "Test Name";
			ReceiveTransportationUnit.WRH_WW_Warehouse = warehouse.PK;
			AssertEquals("Warehouse", TransitWarehouseReceiveHeaderWrapper.WarehouseName.Label);
			AssertEquals("Test Name", TransitWarehouseReceiveHeaderWrapper.WarehouseName.Value);
		}

		#endregion

		#region TestWarehouseName_Translatable

		public void TestWarehouseName_Translatable()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "Test Warehouse";
			ReceiveTransportationUnit.WRH_WW_Warehouse = warehouse.PK;
			AssertEquals("Warehouse", TransitWarehouseReceiveHeaderWrapper.WarehouseName.Label);
			AssertEquals("Test Warehouse", TransitWarehouseReceiveHeaderWrapper.WarehouseName.Value);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "Test Warehouse").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试仓库"));
				AssertEquals("Warehouse", TransitWarehouseReceiveHeaderWrapper.WarehouseName.Label);
				AssertEquals("测试仓库", TransitWarehouseReceiveHeaderWrapper.WarehouseName.Value);
			}
		}

		#endregion

		#region TestPackagesCore

		protected override void TestPackagesCore()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();

			var package1 = Factory.New<PkgPackage>();
			package1.KP_PackageID = "P1";
			package1.KP_KJ_ParentPackageJob = packageJob.PK;

			var package2 = Factory.New<PkgPackage>();
			package2.KP_PackageID = "P2";
			package2.KP_KJ_ParentPackageJob = packageJob.PK;

			var packageState = ReceiveTransportationUnit.PackageStates.AddNew();
			packageState.WPS_KP_Package = package1.PK;

			var wrapper = new WhsItemReceiveTransportationUnitWrapper(ReceiveTransportationUnit, Factory);
			AssertEquals(1, wrapper.Packages.Count);
		}

		#endregion

		#region TestPackages_OnlyCountOuters

		public void TestPackages_OnlyCountOuters()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var rcn = TransitHelper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var rtu = TransitHelper.CreateReceiveTransportationUnit("RTU0001", warehouse.PK, warehouse.DefaultLocation.PK);

			var packageState1 = TransitHelper.CreatePackageState(rcn, 1, "PKG", "P0001", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var packageState2 = TransitHelper.CreatePackageState(rcn, 1, "PKG", "P0002", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var packageState3 = TransitHelper.CreatePackageState(rcn, 1, "PKG", "P0003", TransitWarehouseStatuses.Codes.Arrived, rtu);

			var pkgHU = TransitHelper.CreatePackageHandlingUnit();
			var huPackageState = TransitHelper.CreateHandlingUnitPackage("HU0001", pkgHU, null);
			TransitHelper.PackPackageIntoHandlingUnit(huPackageState, packageState1, ZDateTimeOffset.Now, "ABC", huPackageState);
			TransitHelper.PackPackageIntoHandlingUnit(huPackageState, packageState2, ZDateTimeOffset.Now, "ABC", huPackageState);
			TransitHelper.PackPackageIntoHandlingUnit(huPackageState, packageState3, ZDateTimeOffset.Now, "ABC", huPackageState);

			var wrapper = new WhsItemReceiveTransportationUnitWrapper(rtu, Factory);
			var groups = wrapper.Packages.GroupBy(p => (p as PackageWrapper).PackageState.ConsignmentID);
			AssertEquals(1, groups.Count());
			AssertEquals(3, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder([1m, 1m, 1m], wrapper.Packages.Select(p => (p as PackageWrapper).Packages.Value));
			AssertEquals(0, (wrapper.Packages.First() as PackageWrapper).PackageState.ReceiveConsignment.Shorts);
		}

		public void TestPackages_OnlyCountOuters_SkipScanMode()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var rcn = TransitHelper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var rtu = TransitHelper.CreateReceiveTransportationUnit("RTU0001", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var packageState1 = TransitHelper.CreatePackageState(rcn, 10, "PKG", string.Empty, TransitWarehouseStatuses.Codes.Arrived, rtu);
			var packageState2 = TransitHelper.CreatePackageState(rcn, 5, "PKG", string.Empty, TransitWarehouseStatuses.Codes.Arrived, rtu);
			TransitHelper.CreatePackageState(rcn, 234, "PKG", string.Empty, TransitWarehouseStatuses.Codes.Booked);

			var pkgHU = TransitHelper.CreatePackageHandlingUnit();
			var huPackageState = TransitHelper.CreateHandlingUnitPackage("HU0001", pkgHU, null);
			TransitHelper.PackPackageIntoHandlingUnit(huPackageState, packageState1, ZDateTimeOffset.Now, "ABC", huPackageState);
			TransitHelper.PackPackageIntoHandlingUnit(huPackageState, packageState2, ZDateTimeOffset.Now, "ABC", huPackageState);

			var wrapper = new WhsItemReceiveTransportationUnitWrapper(rtu, Factory);
			var groups = wrapper.Packages.GroupBy(p => (p as PackageWrapper).PackageState.ConsignmentID);
			AssertEquals(1, groups.Count());
			AssertEquals(2, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder([10m, 5m], wrapper.Packages.Select(p => (p as PackageWrapper).Packages.Value));
			AssertEquals(234, (wrapper.Packages.First() as PackageWrapper).PackageState.ReceiveConsignment.Shorts);
		}

		public void TestPackages_StandalonePackageAndHU()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var rcn = TransitHelper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var rtu = TransitHelper.CreateReceiveTransportationUnit("RTU0001", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var packageState1 = TransitHelper.CreatePackageState(rcn, 10, "PKG", string.Empty, TransitWarehouseStatuses.Codes.Arrived, rtu);
			var pkgHU1 = TransitHelper.CreatePackageHandlingUnit();
			var huPackageState1 = TransitHelper.CreateHandlingUnitPackage("HU0001", pkgHU1, null);
			TransitHelper.PackPackageIntoHandlingUnit(huPackageState1, packageState1, ZDateTimeOffset.Now, "ABC", huPackageState1);

			var packageState2 = TransitHelper.CreatePackageState(rcn, 1, "PKG", "P0001", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var packageState3 = TransitHelper.CreatePackageState(rcn, 1, "PKG", "P0002", TransitWarehouseStatuses.Codes.Arrived, rtu);
			var packageState4 = TransitHelper.CreatePackageState(rcn, 1, "PKG", "P0003", TransitWarehouseStatuses.Codes.Arrived, rtu);

			var pkgHU2 = TransitHelper.CreatePackageHandlingUnit();
			var huPackageState2 = TransitHelper.CreateHandlingUnitPackage("HU0002", pkgHU2, null);
			TransitHelper.PackPackageIntoHandlingUnit(huPackageState2, packageState2, ZDateTimeOffset.Now, "ABC", huPackageState2);
			TransitHelper.PackPackageIntoHandlingUnit(huPackageState2, packageState3, ZDateTimeOffset.Now, "ABC", huPackageState2);
			TransitHelper.PackPackageIntoHandlingUnit(huPackageState2, packageState4, ZDateTimeOffset.Now, "ABC", huPackageState2);

			var packageState5 = TransitHelper.CreatePackageState(rcn, 1, "PKG", "P0004", TransitWarehouseStatuses.Codes.Arrived, rtu);

			var wrapper = new WhsItemReceiveTransportationUnitWrapper(rtu, Factory);
			var groups = wrapper.Packages.GroupBy(p => (p as PackageWrapper).PackageState.ConsignmentID);
			AssertEquals(1, groups.Count());
			AssertEquals(5, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder([10m, 1m, 1m, 1m, 1m], wrapper.Packages.Select(p => (p as PackageWrapper).Packages.Value));
			AssertEquals(0, (wrapper.Packages.First() as PackageWrapper).PackageState.ReceiveConsignment.Shorts);
		}

		#endregion

		#region TestJobNumberHeadingCore

		protected override void TestJobNumberHeadingCore()
		{
			AssertEquals("RTU ID", TransitWarehouseReceiveHeaderWrapper.JobNumberHeading);
		}

		#endregion

		#region TestJobNumberCore

		protected override void TestJobNumberCore()
		{
			ReceiveTransportationUnit.WRH_ReferenceNumber = "WDH010101";

			var wrapper = new WhsItemReceiveTransportationUnitWrapper(ReceiveTransportationUnit, Factory);
			AssertEquals("WDH010101", wrapper.JobNumber);
		}

		#endregion

		#region TestCustomerReferenceBarcodeCore

		protected override void TestCustomerReferenceBarcodeCore()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu1 = TransitHelper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rtu2 = TransitHelper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, stageLocation.PK);
			rtu1.WRH_VehicleReference = "V00001";
			rtu2.WRH_VehicleReference = string.Empty;
			rtu1.WRH_UnitType = TransportUnitTypes.Vehicle;
			rtu2.WRH_UnitType = TransportUnitTypes.Vehicle;
			var expectedBarcode1 = new TextBarcode(rtu1.WRH_VehicleReference).TextAs128sFontString;

			var wrapper1 = new WhsItemReceiveTransportationUnitWrapper(rtu1, Factory);
			var wrapper2 = new WhsItemReceiveTransportationUnitWrapper(rtu2, Factory);
			AssertEquals("Customer Reference Barcode", expectedBarcode1, wrapper1.CustomerReferenceBarcode);
			AssertEquals("Customer Reference Barcode", string.Empty, wrapper2.CustomerReferenceBarcode);
		}

		#endregion

		#region TestVehicleReferenceCore

		protected override void TestVehicleReferenceCore()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu = TransitHelper.CreateReceiveTransportationUnit("RTU", warehouse.PK, stageLocation.PK);
			rtu.WRH_VehicleReference = "V00001";
			rtu.WRH_UnitType = TransportUnitTypes.Vehicle;

			var wrapper = new WhsItemReceiveTransportationUnitWrapper(rtu, Factory);
			AssertEquals("Vehicle Reference", wrapper.VehicleReference.Label);
			AssertEquals("V00001", wrapper.VehicleReference.Value);

			var uld = TransitHelper.CreateReceiveTransportationUnitWithContainerType("ULD", warehouse.PK, stageLocation.PK, "ULD", "AAA");
			uld.WRH_VehicleReference = "ULD00001";
			uld.WRH_UnitType = TransportUnitTypes.ULD;

			wrapper = new WhsItemReceiveTransportationUnitWrapper(uld, Factory);
			AssertEquals("Container Reference", wrapper.VehicleReference.Label);
			AssertEquals("ULD00001", wrapper.VehicleReference.Value);

			var cnt = TransitHelper.CreateReceiveTransportationUnitWithContainerType("CNT", warehouse.PK, stageLocation.PK, "CNT", "AAA");
			cnt.WRH_VehicleReference = "CNT00001";
			cnt.WRH_UnitType = TransportUnitTypes.Container;

			wrapper = new WhsItemReceiveTransportationUnitWrapper(cnt, Factory);
			AssertEquals("Container Reference", wrapper.VehicleReference.Label);
			AssertEquals("CNT00001", wrapper.VehicleReference.Value);
		}

		#endregion

		#region TestStagingLocationStringCore

		protected override void TestStagingLocationStringCore()
		{
			ReceiveTransportationUnit.WRH_ReferenceNumber = "WDH00001";
			var location = Factory.NewWithValidTestData<WhsLocation>();
			var wrapper = new WhsItemReceiveTransportationUnitWrapper(ReceiveTransportationUnit, Factory);
			AssertEquals("Staging Location should be empty When Packing Group is null.", string.Empty, wrapper.StagingLocationString.ToString());

			ReceiveTransportationUnit.WRH_WL_StagingLocation = location.PK;
			AssertEquals("Staging Location should have a value.", location.WLV_LocationString, wrapper.StagingLocationString.ToString());
		}

		#endregion

		#region TestTransportCompany

		public override void TestTransportCompany()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "TRANSCOMM LINK";

			Factory.Save();

			ReceiveTransportationUnit.TransportCompany.OrganisationPK = org.PK;
			var wrapper = new WhsItemReceiveTransportationUnitWrapper(ReceiveTransportationUnit, Factory);
			AssertNotNull(wrapper.TransportCompany);

			AssertEquals("TRANSCOMM LINK", wrapper.TransportCompany.CompanyName);
		}

		#endregion

		#region TestUnloadCompleteTime

		protected override void TestUnloadCompleteTimeCore()
		{
			var dateTime = ZDateTimeOffset.Now;
			ReceiveTransportationUnit.WRH_UnloadCompleteTime = dateTime;

			AssertEquals(dateTime.ToLocalZDateTime(), TransitWarehouseReceiveHeaderWrapper.UnloadCompleteTime);
		}

		#endregion

		#region TestPrimaryBarcodeTextCore

		protected override void TestPrimaryBarcodeTextCore()
		{
			var barcode = new TextBarcode("TR00000001");
			ReceiveTransportationUnit.WRH_ReferenceNumber = barcode.TextToEncode;

			var wrapper = new WhsItemReceiveTransportationUnitWrapper(ReceiveTransportationUnit, Factory);
			AssertEquals(barcode.TextAs128sFontString, wrapper.PrimaryBarcodeText);
		}

		#endregion

		#region TestReceiveDriverNameCore

		protected override void TestReceiveDriverNameCore()
		{
			var driverName = new ZString("Tom");
			ReceiveTransportationUnit.WRH_SignedBy = driverName;

			var wrapper = new WhsItemReceiveTransportationUnitWrapper(ReceiveTransportationUnit, Factory);
			AssertEquals(driverName, wrapper.ReceiveDriverName);
		}

		#endregion

		#region TestReceiveDriverSignatureCore

		protected override void TestReceiveDriverSignatureCore()
		{
			var memoryStream = new MemoryStream();
			var binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(200);
			binaryWriter.Write(100);
			binaryWriter.Write(2);
			binaryWriter.Write(6);
			binaryWriter.Write(20);
			binaryWriter.Write(20);
			binaryWriter.Write(20);
			binaryWriter.Write(80);
			binaryWriter.Write(30);
			binaryWriter.Write(80);
			binaryWriter.Write(40);
			binaryWriter.Write(60);
			binaryWriter.Write(30);
			binaryWriter.Write(20);
			binaryWriter.Write(20);
			binaryWriter.Write(20);
			binaryWriter.Write(9);
			binaryWriter.Write(50);
			binaryWriter.Write(20);
			binaryWriter.Write(50);
			binaryWriter.Write(80);
			binaryWriter.Write(60);
			binaryWriter.Write(80);
			binaryWriter.Write(70);
			binaryWriter.Write(70);
			binaryWriter.Write(60);
			binaryWriter.Write(60);
			binaryWriter.Write(50);
			binaryWriter.Write(50);
			binaryWriter.Write(60);
			binaryWriter.Write(40);
			binaryWriter.Write(70);
			binaryWriter.Write(30);
			binaryWriter.Write(50);
			binaryWriter.Write(20);
			var blob = new ZBlob(memoryStream.ToArray());
			ReceiveTransportationUnit.WRH_SignedBySignature = blob;

			var wrapper = new WhsItemReceiveTransportationUnitWrapper(ReceiveTransportationUnit, Factory);
			AssertNotNull("RTU Wrapper", wrapper);
			AssertNotNull("RTU Driver Signature", wrapper.ReceiveDriverSignature);
			AssertEquals(true, wrapper.HasReceiveDriverSignature);
		}

		#endregion

		#region TestJobType

		protected override void TestJobTypeCore()
		{
			ReceiveTransportationUnit.WRH_UnitType = TransportUnitTypes.ULD;

			AssertEquals(TransportUnitTypes.ULD, TransitWarehouseReceiveHeaderWrapper.JobType);
		}

		#endregion

		#region TestContainerType

		protected override void TestContainerTypeCore()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu = TransitHelper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, stageLocation.PK, "ULD1", "AAA");

			Factory.Save();

			var wrapper = new WhsItemReceiveTransportationUnitWrapper(rtu, Factory);
			AssertEquals("AAA", wrapper.ContainerType);
		}

		#endregion

		#region TestSealCore

		protected override void TestSealCore()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var rtu = TransitHelper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			TransitHelper.CreateAdditionalReference(rtu, "S123", WarehouseAdditionalReferenceTypes.Codes.SealNumber);
			Factory.Save();
			var wrapper = new WhsItemReceiveTransportationUnitWrapper(rtu, Factory);

			AssertEquals("RTU has Seal from its Addtional Reference", "S123", wrapper.Seal);

			var rtu2 = TransitHelper.CreateReceiveTransportationUnitWithContainerType("RTU2", warehouse.PK, warehouse.DefaultLocation.PK);
			TransitHelper.CreateAdditionalReference(rtu2, "S456", WarehouseAdditionalReferenceTypes.Codes.SealNumber);
			var container = rtu2.Container;
			container.K0_Seal1 = "S789";
			Factory.Save();
			var wrapper2 = new WhsItemReceiveTransportationUnitWrapper(rtu2, Factory);

			AssertEquals("RTU has Seal from its Related Container", "S789", wrapper2.Seal);
		}

		#endregion

		#region TestIsSecure

		protected override void TestIsSecureCore()
		{
			ReceiveTransportationUnit.WRH_IsVehicleSecure = ZBool.True;

			AssertEquals(ZBool.True, TransitWarehouseReceiveHeaderWrapper.IsSecure);
		}

		#endregion

		#region TestGateInTime

		protected override void TestGateInTimeCore()
		{
			var dateTime = ZDateTimeOffset.Now;
			ReceiveTransportationUnit.WRH_GateInTime = dateTime;

			AssertEquals(dateTime.ToLocalZDateTime(), TransitWarehouseReceiveHeaderWrapper.GateInTime);
		}

		#endregion

		#region TestGateOutTime

		protected override void TestGateOutTimeCore()
		{
			var dateTime = ZDateTimeOffset.Now;
			ReceiveTransportationUnit.WRH_GateOutTime = dateTime;

			AssertEquals(dateTime.ToLocalZDateTime(), TransitWarehouseReceiveHeaderWrapper.GateOutTime);
		}

		#endregion

		#region TestGetWarehouseExpectedArrivalTime

		protected override void TestGetWarehouseExpectedArrivalTime()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var rcn = TransitHelper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var now = ZDateTime.Now;
			rcn.WRC_ExpectedArrivalTime = now;
			var packageState = ReceiveTransportationUnit.PackageStates.AddNew();
			packageState.WPS_WRC_TransitReceiveConsignment = rcn.PK;

			AssertEquals(now, TransitWarehouseReceiveHeaderWrapper.WarehouseExpectedArrivalTime);
		}

		#endregion

		#region TestExpectedDispatchTime

		protected override void TestExpectedDispatchTimeCore()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var rcn = TransitHelper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var now = ZDateTime.Now;
			rcn.WRC_ExpectedDispatchTime = now;
			var packageState = ReceiveTransportationUnit.PackageStates.AddNew();
			packageState.WPS_WRC_TransitReceiveConsignment = rcn.PK;

			AssertEquals(now, TransitWarehouseReceiveHeaderWrapper.ExpectedDispatchTime);
		}

		#endregion

		#region TestStartTime

		protected override void TestStartTimeCore()
		{
			var dateTime = ZDateTimeOffset.Now;
			ReceiveTransportationUnit.WRH_UnloadStartTime = dateTime;

			AssertEquals(dateTime.ToLocalZDateTime(), TransitWarehouseReceiveHeaderWrapper.StartTime);
		}

		#endregion

		#region TestClientRequestedBillToParty

		protected override void TestClientRequestedBillToPartyCore()
		{
			ReceiveTransportationUnit.ClientRequestedBillToPartyDocAddress.Address1 = "CLIENT REQUESTED BILL TO PARTY DOC ADDRESS1";

			CombineAssertions(() =>
			{
				AssertNotNull(TransitWarehouseReceiveHeaderWrapper.ClientRequestedBillToParty);
				AssertEquals("CLIENT REQUESTED BILL TO PARTY DOC ADDRESS1", TransitWarehouseReceiveHeaderWrapper.ClientRequestedBillToParty.MainAddress.AddressLine1);
			});
		}

		#endregion

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
CarrierAccount :  is null
CarrierServiceLevel :  is null
Client :  is null
ClientRequestedBillToParty : 
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
TransportCompany : 
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

		protected override bool ImplementsWarehouseCore => false;

		#endregion

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new WhsItemReceiveTransportationUnitWrapper(ReceiveTransportationUnit, Factory);
		}

		protected override BusinessObject GetNewWhsBusinessObject()
		{
			return Factory.New<WhsItemReceiveTransportationUnit>();
		}

		WhsItemReceiveTransportationUnit ReceiveTransportationUnit
		{
			get { return (WhsItemReceiveTransportationUnit)WhsBusinessObject; }
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WhsItemReceiveTransportationUnitWrapper((WhsItemReceiveTransportationUnit)bizO, Factory);
		}

		WhsItemReceiveTransportationUnitWrapper TransitWarehouseReceiveHeaderWrapper
		{
			get { return (WhsItemReceiveTransportationUnitWrapper)Wrapper; }
		}

		WhsTransitTestHelper TransitHelper => transitHelper ?? (transitHelper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper transitHelper;
	}
}

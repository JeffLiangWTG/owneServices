using System.IO;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Barcode.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WhsItemDispatchTransportationUnitWrapper))]
	sealed class WhsItemDispatchTransportationUnitWrapperTest : WarehouseJobGenericWrapperTest
	{
		#region TestWarehouseNameCore

		protected override void TestWarehouseNameCore()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "Test Name";
			DispatchTransportationUnit.WDH_WW_Warehouse = warehouse.PK;
			AssertEquals("Warehouse", TransitWarehouseDispatchHeaderWrapper.WarehouseName.Label);
			AssertEquals("Test Name", TransitWarehouseDispatchHeaderWrapper.WarehouseName.Value);
		}

		#endregion

		#region TestWarehouseName_Translatable

		public void TestWarehouseName_Translatable()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "Test Warehouse";
			DispatchTransportationUnit.WDH_WW_Warehouse = warehouse.PK;
			AssertEquals("Warehouse", TransitWarehouseDispatchHeaderWrapper.WarehouseName.Label);
			AssertEquals("Test Warehouse", TransitWarehouseDispatchHeaderWrapper.WarehouseName.Value);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "Test Warehouse").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试仓库"));
				AssertEquals("Warehouse", TransitWarehouseDispatchHeaderWrapper.WarehouseName.Label);
				AssertEquals("测试仓库", TransitWarehouseDispatchHeaderWrapper.WarehouseName.Value);
			}
		}

		#endregion

		#region TestClientRequestedBillToParty

		protected override void TestClientRequestedBillToPartyCore()
		{
			var dtu = TransitHelper.CreateDispatchTransportationUnit("DTU", TransitHelper.CreateTRWWarehouse().PK);
			var wrapper = new WhsItemDispatchTransportationUnitWrapper(dtu, Factory);
			AssertEquals("Client Requested Bill To Party", string.Empty, wrapper.ClientRequestedBillToParty.CompanyNameAndAddress);

			var billingParty = dtu.ClientRequestedBillToPartyDocAddress;
			billingParty.CompanyName = "Honda Motorcycles";
			billingParty.Address1 = "Melbourne";

			wrapper = new WhsItemDispatchTransportationUnitWrapper(dtu, Factory);
			AssertEquals("Client Requested Bill To Party", "HONDA MOTORCYCLES\nMELBOURNE", wrapper.ClientRequestedBillToParty.CompanyNameAndAddress);
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

			var packageState = DispatchTransportationUnit.PackageStates.AddNew();
			packageState.WPS_KP_Package = package1.PK;

			var wrapper = new WhsItemDispatchTransportationUnitWrapper(DispatchTransportationUnit, Factory);
			AssertEquals(1, wrapper.Packages.Count);
		}

		#endregion

		#region TestPackages_OnlyCountOuters

		public void TestPackages_OnlyCountOuters()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();

			var package1 = Factory.New<PkgPackage>();
			package1.KP_PackageID = "P1";
			package1.KP_KJ_ParentPackageJob = packageJob.PK;

			var package2 = Factory.New<PkgPackage>();
			package2.KP_PackageID = "P2";
			package2.KP_KJ_ParentPackageJob = packageJob.PK;

			var packageState = DispatchTransportationUnit.PackageStates.AddNew();
			packageState.WPS_KP_Package = package1.PK;
			package1.Packages.Add(package2);

			var wrapper = new WhsItemDispatchTransportationUnitWrapper(DispatchTransportationUnit, Factory);
			AssertEquals(1, wrapper.Packages.Count);
		}

		#endregion

		protected override void TestJobNumberHeadingCore()
		{
			AssertEquals("DTU ID", TransitWarehouseDispatchHeaderWrapper.JobNumberHeading);
		}

		protected override void TestJobNumberCore()
		{
			DispatchTransportationUnit.WDH_ReferenceNumber = "WDH010101";

			var wrapper = new WhsItemDispatchTransportationUnitWrapper(DispatchTransportationUnit, Factory);
			AssertEquals("WDH010101", wrapper.JobNumber);
		}

		#region TestCustomerReferenceBarcodeCore

		protected override void TestCustomerReferenceBarcodeCore()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var dtu1 = TransitHelper.CreateDispatchTransportationUnit("DTU1", warehouse.PK, "VEH", "AAA");
			var dtu2 = TransitHelper.CreateDispatchTransportationUnit("DTU2", warehouse.PK, "VEH", "AAA");
			dtu1.WDH_VehicleReference = "V00001";
			dtu2.WDH_VehicleReference = string.Empty;
			dtu1.WDH_UnitType = TransportUnitTypes.Vehicle;
			dtu2.WDH_UnitType = TransportUnitTypes.Vehicle;
			var expectedBarcode1 = new TextBarcode(dtu1.WDH_VehicleReference).TextAs128sFontString;

			var wrapper1 = new WhsItemDispatchTransportationUnitWrapper(dtu1, Factory);
			var wrapper2 = new WhsItemDispatchTransportationUnitWrapper(dtu2, Factory);
			AssertEquals("Customer Reference Barcode", expectedBarcode1, wrapper1.CustomerReferenceBarcode);
			AssertEquals("Customer Reference Barcode", string.Empty, wrapper2.CustomerReferenceBarcode);
		}

		#endregion

		#region TestVehicleReferenceCore

		protected override void TestVehicleReferenceCore()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var dtu = TransitHelper.CreateDispatchTransportationUnit("DTU", warehouse.PK, "ULD", "AAA");
			dtu.WDH_VehicleReference = "V00001";
			dtu.WDH_UnitType = TransportUnitTypes.Vehicle;

			var wrapper = new WhsItemDispatchTransportationUnitWrapper(dtu, Factory);
			AssertEquals("Vehicle Reference", wrapper.VehicleReference.Label);
			AssertEquals("V00001", wrapper.VehicleReference.Value);

			var uld = TransitHelper.CreateDispatchTransportationUnitWithContainerType("ULD", warehouse.PK, "ULD", "AAA");
			uld.WDH_VehicleReference = "ULD00001";
			uld.WDH_UnitType = TransportUnitTypes.ULD;

			wrapper = new WhsItemDispatchTransportationUnitWrapper(uld, Factory);
			AssertEquals("Container Reference", wrapper.VehicleReference.Label);
			AssertEquals("ULD00001", wrapper.VehicleReference.Value);

			var cnt = TransitHelper.CreateDispatchTransportationUnitWithContainerType("CNT", warehouse.PK, "CNT", "AAA");
			cnt.WDH_VehicleReference = "CNT00001";
			cnt.WDH_UnitType = TransportUnitTypes.Container;

			wrapper = new WhsItemDispatchTransportationUnitWrapper(cnt, Factory);
			AssertEquals("Container Reference", wrapper.VehicleReference.Label);
			AssertEquals("CNT00001", wrapper.VehicleReference.Value);
		}

		#endregion

		#region TestPrimaryBarcodeTextCore

		protected override void TestPrimaryBarcodeTextCore()
		{
			var barcode = new TextBarcode("TD00000001");
			DispatchTransportationUnit.WDH_ReferenceNumber = barcode.TextToEncode;

			var wrapper = new WhsItemDispatchTransportationUnitWrapper(DispatchTransportationUnit, Factory);
			AssertEquals(barcode.TextAs128sFontString, wrapper.PrimaryBarcodeText);
		}

		#endregion

		#region TestDispatchLoadListsCore

		protected override void TestDispatchLoadListsCore()
		{
			var warehouse = TransitHelper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dtu = TransitHelper.CreateDispatchTransportationUnit("DTU", warehouse.PK);
			var wrapper = new WhsItemDispatchTransportationUnitWrapper(dtu, Factory);
			AssertEquals("DTU has no related Load Lists", 0, wrapper.DispatchLoadLists.Count);

			var dtu2 = TransitHelper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			var dispatchLoadList1 = TransitHelper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dispatchLoadList2 = TransitHelper.CreateDispatchLoadList("DLL2", warehouse.PK);
			TransitHelper.CreateDispatchDLLDTUPivot(dispatchLoadList1.PK, dtu2.PK);
			TransitHelper.CreateDispatchDLLDTUPivot(dispatchLoadList2.PK, dtu2.PK);
			Factory.Save();

			var wrapper2 = new WhsItemDispatchTransportationUnitWrapper(dtu2, Factory);
			AssertEquals("DTU has 2 related Load Lists", 2, wrapper2.DispatchLoadLists.Count);
		}

		#endregion

		#region TestContainersCore

		protected override void TestContainersCore()
		{
			var warehouse = TransitHelper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dtu = TransitHelper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK, "CNT1");
			Factory.Save();

			var wrapper = new WhsItemDispatchTransportationUnitWrapper(dtu, Factory);
			AssertEquals("DTU has Related Container", "CNT1", wrapper.Containers[0].ContainerNo);
		}

		#endregion

		#region TestSealCore

		protected override void TestSealCore()
		{
			var warehouse = TransitHelper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dtu = TransitHelper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			TransitHelper.CreateAdditionalReference(dtu, "S123", WarehouseAdditionalReferenceTypes.Codes.SealNumber);
			Factory.Save();
			var wrapper = new WhsItemDispatchTransportationUnitWrapper(dtu, Factory);

			AssertEquals("DTU has Seal from its Addtional Reference", "S123", wrapper.Seal);

			var dtu2 = TransitHelper.CreateDispatchTransportationUnitWithContainerType("DTU2", warehouse.PK);
			TransitHelper.CreateAdditionalReference(dtu2, "S456", WarehouseAdditionalReferenceTypes.Codes.SealNumber);
			var container = dtu2.Container;
			container.K0_Seal1 = "S789";
			Factory.Save();
			var wrapper2 = new WhsItemDispatchTransportationUnitWrapper(dtu2, Factory);

			AssertEquals("DTU has Seal from its Related Container", "S789", wrapper2.Seal);
		}

		#endregion

		#region TestDispatchDriverNameCore

		protected override void TestDispatchDriverNameCore()
		{
			var driverName = new ZString("Tom");
			DispatchTransportationUnit.WDH_SignedBy = driverName;

			var wrapper = new WhsItemDispatchTransportationUnitWrapper(DispatchTransportationUnit, Factory);
			AssertEquals(driverName, wrapper.DispatchDriverName);
		}

		#endregion

		#region TestDispatchDriverSignatureCore

		protected override void TestDispatchDriverSignatureCore()
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
			DispatchTransportationUnit.WDH_SignedBySignature = blob;

			var wrapper = new WhsItemDispatchTransportationUnitWrapper(DispatchTransportationUnit, Factory);
			AssertNotNull("DTU Wrapper", wrapper);
			AssertNotNull("DTU Driver Signature", wrapper.DispatchDriverSignature);
			AssertEquals(true, wrapper.HasDispatchDriverSignature);
		}

		#endregion

		#region TestJobType

		protected override void TestJobTypeCore()
		{
			DispatchTransportationUnit.WDH_UnitType = TransportUnitTypes.ULD;

			AssertEquals(TransportUnitTypes.ULD, TransitWarehouseDispatchHeaderWrapper.JobType);
		}

		#endregion

		#region TestNextDischargePort

		protected override void TestNextDischargePortCore()
		{
			DispatchTransportationUnit.WDH_RL_NKNextDischarge = "TEST";

			AssertEquals("TEST", TransitWarehouseDispatchHeaderWrapper.NextDischargePort);
		}

		#endregion

		#region TestStartTime

		protected override void TestStartTimeCore()
		{
			var dateTime = ZDateTimeOffset.Now;
			DispatchTransportationUnit.WDH_LoadStartTime = dateTime;

			AssertEquals(dateTime.ToLocalZDateTime(), TransitWarehouseDispatchHeaderWrapper.StartTime);
		}

		#endregion

		#region TestCompleteTime

		protected override void TestCompleteTimeCore()
		{
			var dateTime = ZDateTimeOffset.Now;
			DispatchTransportationUnit.WDH_LoadCompleteTime = dateTime;

			AssertEquals(dateTime.ToLocalZDateTime(), TransitWarehouseDispatchHeaderWrapper.CompleteTime);
		}

		#endregion

		#region TestFinalizedTime

		protected override void TestFinalizedTimeCore()
		{
			var dateTime = ZDateTimeOffset.Now;
			DispatchTransportationUnit.WDH_FinalisedTime = dateTime;

			AssertEquals(dateTime.ToLocalZDateTime(), TransitWarehouseDispatchHeaderWrapper.FinalizedTime);
		}

		#endregion

		#region TestGateInTime

		protected override void TestGateInTimeCore()
		{
			var dateTime = ZDateTimeOffset.Now;
			DispatchTransportationUnit.WDH_GateInTime = dateTime;

			AssertEquals(dateTime.ToLocalZDateTime(), TransitWarehouseDispatchHeaderWrapper.GateInTime);
		}

		#endregion

		#region TestGateOutTime

		protected override void TestGateOutTimeCore()
		{
			var dateTime = ZDateTimeOffset.Now;
			DispatchTransportationUnit.WDH_GateOutTime = dateTime;

			AssertEquals(dateTime.ToLocalZDateTime(), TransitWarehouseDispatchHeaderWrapper.GateOutTime);
		}

		#endregion

		#region TestContainerType

		protected override void TestContainerTypeCore()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var dtu = TransitHelper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK, "ULD1", "AAA");

			Factory.Save();

			var wrapper = new WhsItemDispatchTransportationUnitWrapper(dtu, Factory);
			AssertEquals("AAA", wrapper.ContainerType);
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

		protected override bool ImplementsWarehouseCore => false;

		#endregion

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new WhsItemDispatchTransportationUnitWrapper(DispatchTransportationUnit, Factory);
		}

		protected override BusinessObject GetNewWhsBusinessObject()
		{
			return Factory.New<WhsItemDispatchTransportationUnit>();
		}

		WhsItemDispatchTransportationUnit DispatchTransportationUnit
		{
			get { return (WhsItemDispatchTransportationUnit)WhsBusinessObject; }
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WhsItemDispatchTransportationUnitWrapper((WhsItemDispatchTransportationUnit)bizO, Factory);
		}

		WhsItemDispatchTransportationUnitWrapper TransitWarehouseDispatchHeaderWrapper
		{
			get { return (WhsItemDispatchTransportationUnitWrapper)Wrapper; }
		}

		WhsTransitTestHelper TransitHelper => transitHelper ?? (transitHelper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper transitHelper;
	}
}

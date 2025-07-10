using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(WhsItemReceiveASNWrapper))]
	sealed class WhsItemReceiveASNWrapperTest : WarehouseJobGenericWrapperTest
	{
		#region TestJobNumberHeadingCore

		protected override void TestJobNumberHeadingCore()
		{
			AssertEquals("", TransitWarehouseReceiveExpectedPackingWrapper.JobNumberHeading);
		}

		#endregion

		#region TestJobNumberCore

		protected override void TestJobNumberCore()
		{
			AssertEquals("", TransitWarehouseReceiveExpectedPackingWrapper.JobNumber);
		}

		#endregion

		#region TestWarehouseNameCore

		protected override void TestWarehouseNameCore()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "Test Name";
			ReceiveASN.WRP_WW_IntendedWarehouse = warehouse.PK;
			AssertEquals("Warehouse", TransitWarehouseReceiveExpectedPackingWrapper.WarehouseName.Label);
			AssertEquals("Test Name", TransitWarehouseReceiveExpectedPackingWrapper.WarehouseName.Value);
		}

		#endregion

		#region TestWarehouseName_TranslatableCore

		public void TestWarehouseName_Translatable()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "Test Warehouse";
			ReceiveASN.WRP_WW_IntendedWarehouse = warehouse.PK;
			AssertEquals("Warehouse", TransitWarehouseReceiveExpectedPackingWrapper.WarehouseName.Label);
			AssertEquals("Test Warehouse", TransitWarehouseReceiveExpectedPackingWrapper.WarehouseName.Value);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "Test Warehouse").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试仓库"));
				AssertEquals("Warehouse", TransitWarehouseReceiveExpectedPackingWrapper.WarehouseName.Label);
				AssertEquals("测试仓库", TransitWarehouseReceiveExpectedPackingWrapper.WarehouseName.Value);
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

			var packageState = ReceiveASN.PackageStates.AddNew();
			packageState.WPS_KP_Package = package1.PK;

			var wrapper = new WhsItemReceiveASNWrapper(ReceiveASN, Factory);
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

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var packageState = ReceiveASN.PackageStates.AddNew();
			packageState.WPS_KP_Package = package1.PK;
			package1.Packages.Add(package2);

			var wrapper = new WhsItemReceiveASNWrapper(ReceiveASN, Factory);
			AssertEquals(1, wrapper.Packages.Count);
		}

		#endregion

		#region TestTransportReferenceCore

		protected override void TestTransportReferenceCore()
		{
			ReceiveASN.WRP_VehicleReference = "X123";
			AssertEquals("Vehicle Reference", TransitWarehouseReceiveExpectedPackingWrapper.TransportReference.Label);
			AssertEquals("X123", TransitWarehouseReceiveExpectedPackingWrapper.TransportReference.Value);
		}

		#endregion

		#region TestTransportCompanyCore

		protected override void TestTransportCompanyCore()
		{
			var address = ((IDocAddresses)ReceiveASN).DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress);
			address.E2_AddressOverride = true;
			address.E2_CompanyName = "TEST COMPANY NAME";
			AssertEquals("TEST COMPANY NAME", TransitWarehouseReceiveExpectedPackingWrapper.TransportCompany.CompanyName);
		}

		#endregion

		#region TestJobType

		protected override void TestJobTypeCore()
		{
			AssertEquals(TransportUnitTypes.None, TransitWarehouseReceiveExpectedPackingWrapper.JobType);
		}

		public void TestJobTypeIsVEH()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu1 = TransitHelper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rtu2 = TransitHelper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, stageLocation.PK);
			var asn = TransitHelper.CreateReceiveASN("ASN", warehouse.PK);
			TransitHelper.CreateReceiveASNRTUPivot(rtu1.PK, asn.PK);
			TransitHelper.CreateReceiveASNRTUPivot(rtu2.PK, asn.PK);

			Factory.Save();

			var wrapper = new WhsItemReceiveASNWrapper(asn, Factory);
			AssertEquals(TransportUnitTypes.Vehicle, wrapper.JobType);
		}

		public void TestJobTypeIsCNT()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu1 = TransitHelper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, stageLocation.PK, "CNT1", "20GP");
			var rtu2 = TransitHelper.CreateReceiveTransportationUnitWithContainerType("RTU2", warehouse.PK, stageLocation.PK, "CNT2", "20GP");
			var asn = TransitHelper.CreateReceiveASN("ASN", warehouse.PK);
			TransitHelper.CreateReceiveASNRTUPivot(rtu1.PK, asn.PK);
			TransitHelper.CreateReceiveASNRTUPivot(rtu2.PK, asn.PK);

			Factory.Save();

			var wrapper = new WhsItemReceiveASNWrapper(asn, Factory);
			AssertEquals(TransportUnitTypes.Container, wrapper.JobType);
		}

		public void TestJobTypeIsULD()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu1 = TransitHelper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, stageLocation.PK, "ULD1", "AAA");
			var rtu2 = TransitHelper.CreateReceiveTransportationUnitWithContainerType("RTU2", warehouse.PK, stageLocation.PK, "ULD2", "AAA");
			var asn = TransitHelper.CreateReceiveASN("ASN", warehouse.PK);
			TransitHelper.CreateReceiveASNRTUPivot(rtu1.PK, asn.PK);
			TransitHelper.CreateReceiveASNRTUPivot(rtu2.PK, asn.PK);

			Factory.Save();

			var wrapper = new WhsItemReceiveASNWrapper(asn, Factory);
			AssertEquals(TransportUnitTypes.ULD, wrapper.JobType);
		}

		public void TestJobTypeIsMIX()
		{
			var warehouse = TransitHelper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu1 = TransitHelper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, stageLocation.PK, "ULD1", "AAA");
			var rtu2 = TransitHelper.CreateReceiveTransportationUnitWithContainerType("RTU2", warehouse.PK, stageLocation.PK, "CNT2", "20GP");
			var asn = TransitHelper.CreateReceiveASN("ASN", warehouse.PK);
			TransitHelper.CreateReceiveASNRTUPivot(rtu1.PK, asn.PK);
			TransitHelper.CreateReceiveASNRTUPivot(rtu2.PK, asn.PK);

			Factory.Save();

			var wrapper = new WhsItemReceiveASNWrapper(asn, Factory);
			AssertEquals(TransportUnitTypes.Mix, wrapper.JobType);
		}

		#endregion

		#region TestTransportMode

		protected override void TestTransportModeCore()
		{
			ReceiveASN.WRP_TransportMode = TransportModeList.Codes.Airfreight;

			AssertEquals(TransportModeList.Codes.Airfreight, TransitWarehouseReceiveExpectedPackingWrapper.TransportMode);
		}

		#endregion

		#region TestCompleteTime

		protected override void TestCompleteTimeCore()
		{
			var dateTime = ZDateTimeOffset.Now;
			ReceiveASN.WRP_CompleteTime = dateTime;

			AssertEquals(dateTime.ToLocalZDateTime(), TransitWarehouseReceiveExpectedPackingWrapper.CompleteTime);
		}

		#endregion

		#region TestWarehouseExpectedArrivalTime

		protected override void TestGetWarehouseExpectedArrivalTime()
		{
			var dateTime = ZDateTime.Now;
			ReceiveASN.WRP_ETA = dateTime;

			AssertEquals(dateTime, TransitWarehouseReceiveExpectedPackingWrapper.WarehouseExpectedArrivalTime);
		}

		#endregion

		#region ExpectedDefaultFormatting

		protected override ZString ExpectedDefaultFormatting
		{
			get { return @"
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
TransportCompany : 
TransportReference : 
VehicleReference : 
Warehouse :  is null
WarehouseName : 
WeightSent :  is null
WhoCreated : 
WhoFinalised :
"; }
		}

		#endregion

		public override void TestWrapperMappingsEmpty()
		{
		}

		#region TestWarehouseBOWrapper_FactoryCached

		protected override bool ImplementsWarehouseCore => false;

		#endregion

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new WhsItemReceiveASNWrapper(ReceiveASN, Factory);
		}

		protected override BusinessObject GetNewWhsBusinessObject()
		{
			return Factory.New<WhsItemReceiveASN>();
		}

		WhsItemReceiveASN ReceiveASN
		{
			get { return (WhsItemReceiveASN)WhsBusinessObject; }
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new WhsItemReceiveASNWrapper((WhsItemReceiveASN)bizO, Factory);
		}

		WhsItemReceiveASNWrapper TransitWarehouseReceiveExpectedPackingWrapper
		{
			get { return (WhsItemReceiveASNWrapper)Wrapper; }
		}

		WhsTransitTestHelper TransitHelper => transitHelper ?? (transitHelper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper transitHelper;
	}
}

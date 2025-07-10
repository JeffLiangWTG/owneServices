using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(TransitWarehouseReceiveConsignmentWrapper))]
	sealed class TransitWarehouseReceiveConsignmentWrapperTest : WarehouseJobGenericWrapperTest
	{
		#region TestWarehouseNameCore

		protected override void TestWarehouseNameCore()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "Test Name";
			ReceiveConsignment.WRC_WW_IntendedWarehouse = warehouse.PK;
			AssertEquals("Warehouse", TransitWarehouseReceiveConsignmentWrapper.WarehouseName.Label);
			AssertEquals("Test Name", TransitWarehouseReceiveConsignmentWrapper.WarehouseName.Value);
		}

		#endregion

		#region TestWarehouseName_Translatable

		public void TestWarehouseName_Translatable()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "Test Warehouse";
			ReceiveConsignment.WRC_WW_IntendedWarehouse = warehouse.PK;
			AssertEquals("Warehouse", TransitWarehouseReceiveConsignmentWrapper.WarehouseName.Label);
			AssertEquals("Test Warehouse", TransitWarehouseReceiveConsignmentWrapper.WarehouseName.Value);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "Test Warehouse").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试仓库"));
				AssertEquals("Warehouse", TransitWarehouseReceiveConsignmentWrapper.WarehouseName.Label);
				AssertEquals("测试仓库", TransitWarehouseReceiveConsignmentWrapper.WarehouseName.Value);
			}
		}

		#endregion

		protected override void TestJobNumberHeadingCore() => AssertEquals("Receive Consignment", TransitWarehouseReceiveConsignmentWrapper.JobNumberHeading);

		#region TestCustomsStatus

		protected override void TestCustomsStatusCore()
		{
			ReceiveConsignment.WRC_CustomsStatus = "NON";
			AssertEquals("Customs Status", "NON", TransitWarehouseReceiveConsignmentWrapper.CustomsStatus.Code);
		}

		#endregion

		#region TestJobNumber

		protected override void TestJobNumberCore()
		{
			ReceiveConsignment.WRC_JobID = "RCN1";
			ReceiveConsignment.WRC_ConsignmentID = "WRH010101";

			var wrapper = new TransitWarehouseReceiveConsignmentWrapper(ReceiveConsignment, Factory);
			AssertEquals("RCN1", wrapper.JobNumber);
		}

		#endregion

		#region TestSecondaryHeading

		protected override void TestSecondaryHeadingCore() => AssertEquals("RCN Reference", TransitWarehouseReceiveConsignmentWrapper.SecondaryHeading);

		#endregion

		#region TestSecondaryNumber

		protected override void TestSecondaryNumberCore()
		{
			ReceiveConsignment.WRC_JobID = "RCN1";
			ReceiveConsignment.WRC_ConsignmentID = "WRH010101";

			var wrapper = new TransitWarehouseReceiveConsignmentWrapper(ReceiveConsignment, Factory);
			AssertEquals("WRH010101", wrapper.SecondaryNumber);
		}

		#endregion

		#region TestSecondaryReferenceCore

		protected override void TestSecondaryReferenceCore()
		{
			ReceiveConsignment.WRC_JobID = "RCN1";
			ReceiveConsignment.WRC_ConsignmentID = "WRH010101";

			var wrapper = new TransitWarehouseReceiveConsignmentWrapper(ReceiveConsignment, Factory);
			AssertEquals("Reference Number", wrapper.SecondaryReference.Label);
			AssertEquals("WRH010101", wrapper.SecondaryReference.Value);
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

			var packageState = ReceiveConsignment.PackageStates.AddNew();
			packageState.WPS_KP_Package = package1.PK;

			var wrapper = new TransitWarehouseReceiveConsignmentWrapper(ReceiveConsignment, Factory);
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

			var packageState = ReceiveConsignment.PackageStates.AddNew();
			packageState.WPS_KP_Package = package1.PK;
			package1.Packages.Add(package2);

			var wrapper = new TransitWarehouseReceiveConsignmentWrapper(ReceiveConsignment, Factory);
			AssertEquals(1, wrapper.Packages.Count);
		}

		#endregion

		#region TestReceiveDriverNameCore

		protected override void TestReceiveDriverNameCore()
		{
			var driverName = new ZString("Tom");
			var rcn = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			var rtu = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			var packagestate = Factory.NewWithValidTestData<WhsItemPackageState>();
			packagestate.WPS_WRC_TransitReceiveConsignment = rcn.PK;
			packagestate.WPS_WRH_TransitReceiveHeader = rtu.PK;
			rtu.WRH_SignedBy = driverName;
			var wrapper = new TransitWarehouseReceiveConsignmentWrapper(rcn, Factory);
			AssertEquals(driverName, wrapper.ReceiveDriverName);
		}

		#endregion

		#region TestReceiveDriverName_MultipleRelatedRTUs

		public void TestReceiveDriverName_MultipleRelatedRTUs()
		{
			var driverName1 = new ZString("Tom");
			var driverName2 = new ZString("Jerry");
			var rcn = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			var rtu1 = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			var rtu2 = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			var packagestate1 = Factory.NewWithValidTestData<WhsItemPackageState>();
			var packagestate2 = Factory.NewWithValidTestData<WhsItemPackageState>();
			packagestate1.WPS_WRC_TransitReceiveConsignment = rcn.PK;
			packagestate1.WPS_WRH_TransitReceiveHeader = rtu1.PK;
			packagestate2.WPS_WRC_TransitReceiveConsignment = rcn.PK;
			packagestate2.WPS_WRH_TransitReceiveHeader = rtu2.PK;
			rtu1.WRH_SignedBy = driverName1;
			rtu2.WRH_SignedBy = driverName2;
			var wrapper = new TransitWarehouseReceiveConsignmentWrapper(rcn, Factory);
			AssertEquals(ZString.Empty, wrapper.ReceiveDriverName);
		}

		#endregion

		#region TestReceiveDriverSignatureCore

		protected override void TestReceiveDriverSignatureCore()
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
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
			var rcn = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			var rtu = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			var packagestate = Factory.NewWithValidTestData<WhsItemPackageState>();
			packagestate.WPS_WRC_TransitReceiveConsignment = rcn.PK;
			packagestate.WPS_WRH_TransitReceiveHeader = rtu.PK;
			rtu.WRH_SignedBySignature = blob;
			var wrapper = new TransitWarehouseReceiveConsignmentWrapper(rcn, Factory);
			AssertNotNull("RCN Wrapper", wrapper);
			AssertNotNull("RCN Driver Signature", wrapper.ReceiveDriverSignature);
			AssertEquals(true, wrapper.HasReceiveDriverSignature);
		}

		#endregion

		#region TestReceiveDriverSignature_MultipleRelatedRTUs

		public void TestReceiveDriverSignature_MultipleRelatedRTUs()
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
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
			var rcn = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			var rtu1 = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			var rtu2 = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			var packagestate1 = Factory.NewWithValidTestData<WhsItemPackageState>();
			var packagestate2 = Factory.NewWithValidTestData<WhsItemPackageState>();
			packagestate1.WPS_WRC_TransitReceiveConsignment = rcn.PK;
			packagestate1.WPS_WRH_TransitReceiveHeader = rtu1.PK;
			packagestate2.WPS_WRC_TransitReceiveConsignment = rcn.PK;
			packagestate2.WPS_WRH_TransitReceiveHeader = rtu2.PK;
			rtu1.WRH_SignedBySignature = blob;
			rtu2.WRH_SignedBySignature = blob;
			var wrapper = new TransitWarehouseReceiveConsignmentWrapper(rcn, Factory);
			AssertNotNull(wrapper);
			AssertEquals(null, wrapper.ReceiveDriverSignature);
			AssertEquals(false, wrapper.HasReceiveDriverSignature);
		}

		#endregion

		#region TestReceiveDriverNameCore

		protected override void TestDestinationCore()
		{
			var zAJNB = new RefUNLOCO.Loader(Factory).Load("ZAJNB");
			var rcn = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			rcn.WRC_RL_NKDestination = zAJNB.RL_Code;
			var wrapper = new TransitWarehouseReceiveConsignmentWrapper(rcn, Factory);
			AssertEquals("wrapper.Destination.Location.Country.Code", zAJNB.RL_Code.Left(2), wrapper.Destination.Location.Country.Code);
			AssertEquals("wrapper.Destination.Location.IATACode", zAJNB.RL_IATA, wrapper.Destination.Location.IATACode);
			AssertEquals("wrapper.Destination.Location.PortName", zAJNB.RL_PortName, wrapper.Destination.Location.PortName);
			AssertEquals("wrapper.Destination.Location.UNLOCO", zAJNB.RL_Code, wrapper.Destination.Location.UNLOCO);
			AssertEquals("wrapper.Destination.Location.UNLOCOAndPortName", zAJNB.RL_Code + " - " + zAJNB.RL_PortName, wrapper.Destination.Location.UNLOCOAndPortName);
			AssertEquals("wrapper.Destination.Location.State", ZString.Empty, wrapper.Destination.Location.State);
		}

		#endregion

		#region TestReceiveTransportationUnitsCore

		protected override void TestReceiveTransportationUnitsCore()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var rcn = TransitHelper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var rtu1 = TransitHelper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK, "VEH1");
			var rtu2 = TransitHelper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, warehouse.DefaultLocation.PK, "VEH2");
			var asn = TransitHelper.CreateReceiveASN("ASN1", warehouse.PK);
			var pivot1 = TransitHelper.CreateReceiveASNRTUPivot(rtu1.PK, asn.PK);
			var pivot2 = TransitHelper.CreateReceiveASNRTUPivot(rtu2.PK, asn.PK);

			TransitHelper.CreatePackageState(rcn, 1, "PLT", "P1", status: TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1, receiveASN: asn);
			TransitHelper.CreatePackageState(rcn, 1, "PLT", "P2", status: TransitWarehouseStatuses.Codes.Booked, receiveASN: asn);

			Factory.Save();

			var wrapper = new TransitWarehouseReceiveConsignmentWrapper(rcn, Factory);
			AssertEquals("RCN has RTU List", 2, wrapper.ReceiveTransportationUnits.Count);
			AssertEquals("RCN has RTU with reference RTU1", "VEH1", wrapper.ReceiveTransportationUnits[0].VehicleReference.Value);
			AssertEquals("RCN has RTU with reference RTU2", "VEH2", wrapper.ReceiveTransportationUnits[1].VehicleReference.Value);
		}

		#endregion

		#region TestGetWarehouseFinalisedDateCore

		protected override void TestFinalisedDateCore()
		{
			var expectedFinalisedDate = ZDateTimeOffset.Now;
			var expectedLabel = "Finalized Date";

			var warehouse = TransitHelper.CreateTRWWarehouse();
			var rcn = TransitHelper.CreateReceiveConsignment("RCNTST001", "STD", warehouse.PK);
			rcn.WRC_CompleteTime = expectedFinalisedDate;
			Factory.Save();

			var wrapper = new TransitWarehouseReceiveConsignmentWrapper(rcn, Factory);
			AssertEquals(expectedLabel, wrapper.FinalisedDate.Label);
			AssertEquals(expectedFinalisedDate.ToZDateTime(), wrapper.FinalisedDate.ValueAsDate);
		}

		#endregion

		#region TestGetWarehouseExpectedArrivalTime

		protected override void TestGetWarehouseExpectedArrivalTime()
		{
			var expectedArrivalTime = ZDateTime.Now;

			var warehouse = TransitHelper.CreateTRWWarehouse();
			var rcn = TransitHelper.CreateReceiveConsignment("RCNTST001", "STD", warehouse.PK);
			rcn.WRC_ExpectedArrivalTime = expectedArrivalTime;

			var wrapper = new TransitWarehouseReceiveConsignmentWrapper(rcn, Factory);

			AssertEquals(expectedArrivalTime, wrapper.WarehouseExpectedArrivalTime);
		}

		#endregion

		#region TestGetExpectedDispatchTime

		protected override void TestExpectedDispatchTimeCore()
		{
			var expectedDispatchTime = ZDateTime.Now;

			var warehouse = TransitHelper.CreateTRWWarehouse();
			var rcn = TransitHelper.CreateReceiveConsignment("RCNTST001", "STD", warehouse.PK);
			rcn.WRC_ExpectedDispatchTime = expectedDispatchTime;

			var wrapper = new TransitWarehouseReceiveConsignmentWrapper(rcn, Factory);

			AssertEquals(expectedDispatchTime, wrapper.ExpectedDispatchTime);
		}

		#endregion

		#region TestClientRequestedBillToParty

		protected override void TestClientRequestedBillToPartyCore()
		{
			ReceiveConsignment.ClientRequestedBillToPartyDocAddress.Address1 = "CLIENT REQUESTED BILL TO PARTY DOC ADDRESS1";

			CombineAssertions(() =>
			{
				AssertNotNull(TransitWarehouseReceiveConsignmentWrapper.ClientRequestedBillToParty);
				AssertEquals("CLIENT REQUESTED BILL TO PARTY DOC ADDRESS1", TransitWarehouseReceiveConsignmentWrapper.ClientRequestedBillToParty.MainAddress.AddressLine1);
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
CustomsStatus : CUS - Not cleared by Customs
Destination : 
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
			((WhsItemReceiveConsignment)bizO).WRC_WW_IntendedWarehouse = warehousePK;
		}

		protected override BusinessObject GetNewWhsBusinessObjectInSpecifiedFactory(BusinessObjectFactory factory)
		{
			return factory.New<WhsItemReceiveConsignment>();
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapperInSpecifiedFactory(BusinessObject bizO, BusinessObjectFactory factory)
		{
			return new TransitWarehouseReceiveConsignmentWrapper((WhsItemReceiveConsignment)bizO, factory);
		}

		#endregion

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new TransitWarehouseReceiveConsignmentWrapper(ReceiveConsignment, Factory);
		}

		protected override BusinessObject GetNewWhsBusinessObject()
		{
			return Factory.New<WhsItemReceiveConsignment>();
		}

		WhsItemReceiveConsignment ReceiveConsignment => (WhsItemReceiveConsignment)WhsBusinessObject;

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new TransitWarehouseReceiveConsignmentWrapper((WhsItemReceiveConsignment)bizO, Factory);
		}

		WhsTransitTestHelper TransitHelper => transitHelper ?? (transitHelper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper transitHelper;

		TransitWarehouseReceiveConsignmentWrapper TransitWarehouseReceiveConsignmentWrapper => (TransitWarehouseReceiveConsignmentWrapper)Wrapper;
	}
}

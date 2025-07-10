using System.IO;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(TransitWarehouseDispatchConsignmentWrapper))]
	sealed class TransitWarehouseDispatchConsignmentWrapperTest : WarehouseJobGenericWrapperTest
	{
		protected override void TestJobNumberHeadingCore() => AssertEquals("Dispatch Consignment", TransitWarehouseDispatchConsignmentWrapper.JobNumberHeading);

		#region TestJobNumber

		protected override void TestJobNumberCore()
		{
			DispatchConsignment.WDC_JobID = "DCN1";
			DispatchConsignment.WDC_ConsignmentID = "WRH010101";

			var wrapper = new TransitWarehouseDispatchConsignmentWrapper(DispatchConsignment, Factory);
			AssertEquals("DCN1", wrapper.JobNumber);
		}

		#endregion

		#region TestSecondaryHeading

		protected override void TestSecondaryHeadingCore() => AssertEquals("DCN Reference", TransitWarehouseDispatchConsignmentWrapper.SecondaryHeading);

		#endregion

		#region TestSecondaryNumber

		protected override void TestSecondaryNumberCore()
		{
			DispatchConsignment.WDC_JobID = "DCN1";
			DispatchConsignment.WDC_ConsignmentID = "WRH010101";

			var wrapper = new TransitWarehouseDispatchConsignmentWrapper(DispatchConsignment, Factory);
			AssertEquals("WRH010101", wrapper.SecondaryNumber);
		}

		#endregion

		#region TestSecondaryReferenceCore

		protected override void TestSecondaryReferenceCore()
		{
			DispatchConsignment.WDC_JobID = "DCN1";
			DispatchConsignment.WDC_ConsignmentID = "WRH010101";

			var wrapper = new TransitWarehouseDispatchConsignmentWrapper(DispatchConsignment, Factory);
			AssertEquals("Reference Number", wrapper.SecondaryReference.Label);
			AssertEquals("WRH010101", wrapper.SecondaryReference.Value);
		}

		#endregion

		#region TestDispatchDriverNameCore

		protected override void TestDispatchDriverNameCore()
		{
			var driverName = new ZString("Tom");
			var dcn = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			var dtu = Factory.NewWithValidTestData<WhsItemDispatchTransportationUnit>();
			var packagestate = Factory.NewWithValidTestData<WhsItemPackageState>();
			packagestate.WPS_WDC_TransitDispatchConsignment = dcn.PK;
			packagestate.WPS_WDH_TransitDispatchHeader = dtu.PK;
			dtu.WDH_SignedBy = driverName;
			var wrapper = new TransitWarehouseDispatchConsignmentWrapper(dcn, Factory);
			AssertEquals(driverName, wrapper.DispatchDriverName);
		}

		#endregion

		#region TestDispatchDriverName_MultipleRelatedDTUs

		public void TestDispatchDriverName_MultipleRelatedDTUs()
		{
			var driverName1 = new ZString("Tom");
			var driverName2 = new ZString("Jerry");
			var dcn = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			var dtu1 = Factory.NewWithValidTestData<WhsItemDispatchTransportationUnit>();
			var dtu2 = Factory.NewWithValidTestData<WhsItemDispatchTransportationUnit>();
			var packagestate1 = Factory.NewWithValidTestData<WhsItemPackageState>();
			var packagestate2 = Factory.NewWithValidTestData<WhsItemPackageState>();
			packagestate1.WPS_WDC_TransitDispatchConsignment = dcn.PK;
			packagestate1.WPS_WDH_TransitDispatchHeader = dtu1.PK;
			packagestate2.WPS_WDC_TransitDispatchConsignment = dcn.PK;
			packagestate2.WPS_WDH_TransitDispatchHeader = dtu2.PK;
			dtu1.WDH_SignedBy = driverName1;
			dtu2.WDH_SignedBy = driverName2;
			var wrapper = new TransitWarehouseDispatchConsignmentWrapper(dcn, Factory);
			AssertEquals(ZString.Empty, wrapper.DispatchDriverName);
		}

		#endregion

		#region TestDispatchDriverSignatureCore

		protected override void TestDispatchDriverSignatureCore()
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
			var dcn = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			var dtu = Factory.NewWithValidTestData<WhsItemDispatchTransportationUnit>();
			var packagestate = Factory.NewWithValidTestData<WhsItemPackageState>();
			packagestate.WPS_WDC_TransitDispatchConsignment = dcn.PK;
			packagestate.WPS_WDH_TransitDispatchHeader = dtu.PK;
			dtu.WDH_SignedBySignature = blob;
			var wrapper = new TransitWarehouseDispatchConsignmentWrapper(dcn, Factory);
			AssertNotNull(wrapper);
			AssertNotNull("DCN Driver Signature", wrapper.DispatchDriverSignature);
			AssertEquals(true, wrapper.HasDispatchDriverSignature);
		}

		#endregion

		#region TestDispatchDriverSignature_MultipleRelatedDTUs

		public void TestDispatchDriverSignature_MultipleRelatedDTUs()
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
			var dcn = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			var dtu1 = Factory.NewWithValidTestData<WhsItemDispatchTransportationUnit>();
			var dtu2 = Factory.NewWithValidTestData<WhsItemDispatchTransportationUnit>();
			var packagestate1 = Factory.NewWithValidTestData<WhsItemPackageState>();
			var packagestate2 = Factory.NewWithValidTestData<WhsItemPackageState>();
			packagestate1.WPS_WDC_TransitDispatchConsignment = dcn.PK;
			packagestate1.WPS_WDH_TransitDispatchHeader = dtu1.PK;
			packagestate2.WPS_WDC_TransitDispatchConsignment = dcn.PK;
			packagestate2.WPS_WDH_TransitDispatchHeader = dtu2.PK;
			dtu1.WDH_SignedBySignature = blob;
			dtu2.WDH_SignedBySignature = blob;
			var wrapper = new TransitWarehouseDispatchConsignmentWrapper(dcn, Factory);
			AssertNotNull(wrapper);
			AssertEquals(null, wrapper.DispatchDriverSignature);
			AssertEquals(false, wrapper.HasDispatchDriverSignature);
		}

		#endregion

		#region TestTotalInnerPackLines

		protected override void TestTotalInnerPackLinesCore()
		{
			var warehouse = TransitHelper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			var dcn = TransitHelper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var rcn = TransitHelper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = TransitHelper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var hu1Job = TransitHelper.CreatePackageHandlingUnit();
			var hu1 = TransitHelper.CreateHandlingUnitPackage("HU1", hu1Job, rtu, rcn: rcn);
			var innerPackline1 = TransitHelper.CreatePackageState(rcn, 10, "PKL", ZString.Empty, TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.PackLine);
			var innerPackline2 = TransitHelper.CreatePackageState(rcn, 10, "PKL", ZString.Empty, TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.PackLine);
			TransitHelper.PackPackageIntoHandlingUnit(hu1, innerPackline1, ZDateTimeOffset.Now, "ABC", hu1);
			TransitHelper.PackPackageIntoHandlingUnit(hu1, innerPackline2, ZDateTimeOffset.Now, "ABC", hu1);

			var hu2Job = TransitHelper.CreatePackageHandlingUnit();
			var hu2 = TransitHelper.CreateHandlingUnitPackage("HU2", hu2Job, rtu, rcn: rcn);
			var innerScannedPackage = TransitHelper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.Package);
			var innerNonScannedPackage = TransitHelper.CreatePackageState(rcn, 1, "PKG", ZString.Empty, TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.Package);
			TransitHelper.PackPackageIntoHandlingUnit(hu2, innerScannedPackage, ZDateTimeOffset.Now, "ABC", hu2);
			TransitHelper.PackPackageIntoHandlingUnit(hu2, innerNonScannedPackage, ZDateTimeOffset.Now, "ABC", hu2);

			TransitHelper.CreatePackageState(rcn, 1, "PKL", "PKG2", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.PackLine);

			var wrapper = new TransitWarehouseDispatchConsignmentWrapper(dcn, Factory);
			AssertEquals((short)21, wrapper.TotalInnerPackLines);
		}

		#endregion

		#region TestTotalInners

		protected override void TestTotalInnersCore()
		{
			var warehouse = TransitHelper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			var dcn = TransitHelper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var rcn = TransitHelper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = TransitHelper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var hu1Job = TransitHelper.CreatePackageHandlingUnit();
			var hu1 = TransitHelper.CreateHandlingUnitPackage("HU1", hu1Job, rtu, rcn: rcn);
			var innerPackline1 = TransitHelper.CreatePackageState(rcn, 10, "PKL", ZString.Empty, TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.PackLine);
			var innerPackline2 = TransitHelper.CreatePackageState(rcn, 10, "PKL", ZString.Empty, TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.PackLine);
			TransitHelper.PackPackageIntoHandlingUnit(hu1, innerPackline1, ZDateTimeOffset.Now, "ABC", hu1);
			TransitHelper.PackPackageIntoHandlingUnit(hu1, innerPackline2, ZDateTimeOffset.Now, "ABC", hu1);

			var hu2Job = TransitHelper.CreatePackageHandlingUnit();
			var hu2 = TransitHelper.CreateHandlingUnitPackage("HU2", hu2Job, rtu, rcn: rcn);
			var innerScannedPackage = TransitHelper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.Package);
			var innerNonScannedPackage = TransitHelper.CreatePackageState(rcn, 1, "PKG", ZString.Empty, TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.Package);
			TransitHelper.PackPackageIntoHandlingUnit(hu2, innerScannedPackage, ZDateTimeOffset.Now, "ABC", hu2);
			TransitHelper.PackPackageIntoHandlingUnit(hu2, innerNonScannedPackage, ZDateTimeOffset.Now, "ABC", hu2);

			TransitHelper.CreatePackageState(rcn, 1, "PKL", "PKG2", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.PackLine);

			var wrapper = new TransitWarehouseDispatchConsignmentWrapper(dcn, Factory);
			AssertEquals((short)22, wrapper.TotalInners);
		}

		#endregion

		#region TestTotalOverpack

		protected override void TestTotalOverpacksCore()
		{
			var warehouse = TransitHelper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			var dcn = TransitHelper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var rcn = TransitHelper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = TransitHelper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			var ovp1_top = TransitHelper.CreateOverpackPackage("OVP1-Top", rcn, rtu, TransitWarehouseStatuses.Codes.Arrived, rcn: rcn, dcn: dcn, unitType: PackageStateUnitType.Codes.Overpack);
			var ovp1_innerOvp = TransitHelper.CreateOverpackPackage("OVP1-InnerOvp", rcn, rtu, TransitWarehouseStatuses.Codes.Arrived, rcn: rcn, dcn: dcn, unitType: PackageStateUnitType.Codes.Overpack);
			var ovp1Package1 = TransitHelper.CreatePackageState(rcn, 1, "PKG", "OVP1-1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			transitHelper.PackPackageIntoHandlingUnit(ovp1_top, ovp1Package1, ZDateTimeOffset.Now, "ABC", ovp1_top);
			transitHelper.PackPackageIntoHandlingUnit(ovp1_top, ovp1_innerOvp, ZDateTimeOffset.Now, "ABC", ovp1_top);

			var ovp2 = TransitHelper.CreateOverpackPackage("OVP2-Top", rcn, rtu, TransitWarehouseStatuses.Codes.Arrived, rcn: rcn, dcn: dcn, unitType: PackageStateUnitType.Codes.Overpack);
			var ovp2Package = TransitHelper.CreatePackageState(rcn, 1, "PKG", "OVP2-1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			TransitHelper.PackPackageIntoHandlingUnit(ovp2, ovp2Package, ZDateTimeOffset.Now, "ABC", ovp2);

			TransitHelper.CreatePackageState(rcn, 1, "PKL", "PKG2", TransitWarehouseStatuses.Codes.Arrived, rtu, dcn, unitType: PackageStateUnitType.Codes.Package);

			var hu1Job = TransitHelper.CreatePackageHandlingUnit();
			var hu1 = TransitHelper.CreateHandlingUnitPackage("HU1", hu1Job, rtu, rcn: rcn);
			var ovp3 = TransitHelper.CreateOverpackPackage("OVP1-Top", rcn, rtu, TransitWarehouseStatuses.Codes.Arrived, rcn: rcn, dcn: dcn, unitType: PackageStateUnitType.Codes.Overpack);
			transitHelper.PackPackageIntoHandlingUnit(hu1, ovp3, ZDateTimeOffset.Now, "ABC", hu1);

			var wrapper = new TransitWarehouseDispatchConsignmentWrapper(dcn, Factory);
			AssertEquals((short)3, wrapper.TotalOverpacks);
		}

		#endregion

		#region TestDispatchLoadListsCore

		protected override void TestDispatchLoadListsCore()
		{
			var portCode = new ZString("NZTST");
			var portCode2 = new ZString("AUTST");
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var rcn = TransitHelper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var rtu = TransitHelper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var dcn = TransitHelper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dll = TransitHelper.CreateDispatchLoadList("DLL1", warehouse.PK, warehouse.Rows[0].Locations[0]);
			var dll2 = TransitHelper.CreateDispatchLoadList("DLL2", warehouse.PK, warehouse.Rows[0].Locations[0]);
			dll.WDL_RL_NKLastDischargePort = portCode;
			dll2.WDL_RL_NKLastDischargePort = portCode2;

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;
			var package1 = TransitHelper.CreatePackage(packageJob, "P1", 1, "PLT");
			var package2 = TransitHelper.CreatePackage(packageJob, "P2", 1, "PLT");
			var package3 = TransitHelper.CreatePackage(packageJob, "P1", 1, "PLT");
			var package4 = TransitHelper.CreatePackage(packageJob, "P2", 1, "PLT");

			TransitHelper.CreatePackageState(package: package1, status: TransitWarehouseStatuses.Codes.Arrived,
				receiveConsignment: rcn, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll);
			TransitHelper.CreatePackageState(package: package2, status: TransitWarehouseStatuses.Codes.Arrived,
				receiveConsignment: rcn, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll);
			TransitHelper.CreatePackageState(package: package3, status: TransitWarehouseStatuses.Codes.Arrived,
				receiveConsignment: rcn, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2);
			TransitHelper.CreatePackageState(package: package4, status: TransitWarehouseStatuses.Codes.Arrived,
				receiveConsignment: rcn, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2);
			Factory.Save();

			var wrapper = new TransitWarehouseDispatchConsignmentWrapper(dcn, Factory);

			var results = wrapper.DispatchLoadLists.ToArray<WhsItemDispatchLoadListWrapper>();

			AssertEquals("Expect unique collection of related DLLs to DCN",2, wrapper.DispatchLoadLists.Count);
			AssertEquals("Expect unique return of first load list", 1, results.Count(w => w.Destination.Location.PortName == portCode));
			AssertEquals("Expect unique return of second load list", 1, results.Count(w => w.Destination.Location.PortName == portCode2));
		}

		#endregion

		#region TestReceiveDriverNameCore

		protected override void TestDestinationCore()
		{
			var zAJNB = new RefUNLOCO.Loader(Factory).Load("ZAJNB");
			var dcn = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			dcn.WDC_RL_NKDestination = zAJNB.RL_Code;
			var wrapper = new TransitWarehouseDispatchConsignmentWrapper(dcn, Factory);
			AssertEquals("wrapper.Destination.Location.Country.Code", zAJNB.RL_Code.Left(2), wrapper.Destination.Location.Country.Code);
			AssertEquals("wrapper.Destination.Location.IATACode", zAJNB.RL_IATA, wrapper.Destination.Location.IATACode);
			AssertEquals("wrapper.Destination.Location.PortName", zAJNB.RL_PortName, wrapper.Destination.Location.PortName);
			AssertEquals("wrapper.Destination.Location.UNLOCO", zAJNB.RL_Code, wrapper.Destination.Location.UNLOCO);
			AssertEquals("wrapper.Destination.Location.UNLOCOAndPortName", zAJNB.RL_Code + " - " + zAJNB.RL_PortName, wrapper.Destination.Location.UNLOCOAndPortName);
			AssertEquals("wrapper.Destination.Location.State", ZString.Empty, wrapper.Destination.Location.State);
		}

		#endregion

		#region TestIsAuthorizedForDispatch

		protected override void TestIsAuthorizedForDispatchCore()
		{
			DispatchConsignment.WDC_IsAuthorizedForDispatch = ZBool.False;
			var wrapper = new TransitWarehouseDispatchConsignmentWrapper(DispatchConsignment, Factory);
			AssertEquals(ZBool.False, wrapper.IsAuthorizedForDispatch);

			DispatchConsignment.WDC_IsAuthorizedForDispatch = ZBool.True;
			AssertEquals(ZBool.True, wrapper.IsAuthorizedForDispatch);
		}

		#endregion

		#region TestIsSplit

		protected override void TestIsSplitCore()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dcn1 = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			dcn1.WDC_ParentID = new ZGuid("16071ba6-ca70-4437-a3db-a2c827ffd00c");
			dcn1.WDC_WW_Warehouse = warehouse.PK;

			var dcn2 = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
			dcn2.WDC_ParentID = dcn1.WDC_ParentID;
			dcn2.WDC_WW_Warehouse = dcn1.WDC_WW_Warehouse;

			var wrapper = new TransitWarehouseDispatchConsignmentWrapper(DispatchConsignment, Factory);
			var wrapper2 = new TransitWarehouseDispatchConsignmentWrapper(dcn2, Factory);

			AssertEquals(ZBool.False, wrapper.IsSplit);
			AssertEquals(ZBool.True, wrapper2.IsSplit);
		}

		#endregion

		#region TestIAllowPartialLoading

		protected override void TestAllowPartialLoadingCore()
		{
			DispatchConsignment.WDC_AllowPartialLoading = ZBool.False;
			var wrapper = new TransitWarehouseDispatchConsignmentWrapper(DispatchConsignment, Factory);
			AssertEquals(ZBool.False, wrapper.AllowPartialLoading);

			DispatchConsignment.WDC_AllowPartialLoading = ZBool.True;
			AssertEquals(ZBool.True, wrapper.AllowPartialLoading);
		}

		#endregion

		#region TestExpectedDispatchTime

		protected override void TestExpectedDispatchTimeCore()
		{
			DispatchConsignment.WDC_ExpectedDispatchTime = ZDateTimeOffset.Now;
			var wrapper = new TransitWarehouseDispatchConsignmentWrapper(DispatchConsignment, Factory);
			AssertEquals(DispatchConsignment.WDC_ExpectedDispatchTime.ToZDateTime(), wrapper.ExpectedDispatchTime);
		}

		#endregion

		#region TestCompleteTime

		protected override void TestCompleteTimeCore()
		{
			var dateTime = ZDateTimeOffset.Now;
			DispatchConsignment.WDC_CompleteTime = dateTime;

			AssertEquals(dateTime.ToLocalZDateTime(), TransitWarehouseDispatchConsignmentWrapper.CompleteTime);
		}

		#endregion

		#region TestClientRequestedBillToParty

		protected override void TestClientRequestedBillToPartyCore()
		{
			DispatchConsignment.ClientRequestedBillToPartyDocAddress.Address1 = "CLIENT REQUESTED BILL TO PARTY DOC ADDRESS1";

			CombineAssertions(() =>
			{
				AssertNotNull(TransitWarehouseDispatchConsignmentWrapper.ClientRequestedBillToParty);
				AssertEquals("CLIENT REQUESTED BILL TO PARTY DOC ADDRESS1", TransitWarehouseDispatchConsignmentWrapper.ClientRequestedBillToParty.MainAddress.AddressLine1);
			});
		}

		#endregion

		#region TestMasterBillCore

		protected override void TestMasterBillCore()
		{
			var warehouse = TransitHelper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var rcn = TransitHelper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var rtu = TransitHelper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var dcn = TransitHelper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dcn2 = TransitHelper.CreateDispatchConsignment("DCN2", warehouse.PK);
			var dll2 = TransitHelper.CreateDispatchLoadList("DLL2", warehouse.PK, warehouse.Rows[0].Locations[0]);
			TransitHelper.CreateAdditionalReference(dll2, "123123", WarehouseAdditionalReferenceTypes.Codes.MasterBill);

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;
			var package1 = TransitHelper.CreatePackage(packageJob, "P1", 1, "PLT");
			var package2 = TransitHelper.CreatePackage(packageJob, "P2", 1, "PLT");

			TransitHelper.CreatePackageState(package: package1, status: TransitWarehouseStatuses.Codes.Arrived,
				receiveConsignment: rcn, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: null);
			TransitHelper.CreatePackageState(package: package2, status: TransitWarehouseStatuses.Codes.Arrived,
				receiveConsignment: rcn, receiveUnit: rtu, dispatchConsignment: dcn2, dispatchLoadList: dll2);
			Factory.Save();

			var wrapper = new TransitWarehouseDispatchConsignmentWrapper(dcn, Factory);
			var results = wrapper.DispatchLoadLists.ToArray<WhsItemDispatchLoadListWrapper>();
			AssertEquals("Expect no collection of related DLLs to DCN", 0, results.Length);

			var wrapper2 = new TransitWarehouseDispatchConsignmentWrapper(dcn2, Factory);
			var results2 = wrapper2.DispatchLoadLists.ToArray<WhsItemDispatchLoadListWrapper>();
			AssertEquals("Expect unique collection of related DLLs to DCN", 1, results2.Length);
			AssertEquals("Load List has Master Bill", "123123", results2[0].MasterBill);
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
			((WhsItemDispatchConsignment)bizO).WDC_WW_Warehouse = warehousePK;
		}

		protected override BusinessObject GetNewWhsBusinessObjectInSpecifiedFactory(BusinessObjectFactory factory)
		{
			return factory.New<WhsItemDispatchConsignment>();
		}

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapperInSpecifiedFactory(BusinessObject bizO, BusinessObjectFactory factory)
		{
			return new TransitWarehouseDispatchConsignmentWrapper((WhsItemDispatchConsignment)bizO, factory);
		}

		#endregion

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new TransitWarehouseDispatchConsignmentWrapper(DispatchConsignment, Factory);
		}

		protected override BusinessObject GetNewWhsBusinessObject()
		{
			return Factory.New<WhsItemDispatchConsignment>();
		}

		WhsItemDispatchConsignment DispatchConsignment => (WhsItemDispatchConsignment)WhsBusinessObject;

		protected override WarehouseJobGenericWrapper GetNewWarehouseJobGenericWrapper(BusinessObject bizO)
		{
			return new TransitWarehouseDispatchConsignmentWrapper((WhsItemDispatchConsignment)bizO, Factory);
		}

		WhsTransitTestHelper TransitHelper => transitHelper ?? (transitHelper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper transitHelper;

		TransitWarehouseDispatchConsignmentWrapper TransitWarehouseDispatchConsignmentWrapper => (TransitWarehouseDispatchConsignmentWrapper)Wrapper;
	}
}

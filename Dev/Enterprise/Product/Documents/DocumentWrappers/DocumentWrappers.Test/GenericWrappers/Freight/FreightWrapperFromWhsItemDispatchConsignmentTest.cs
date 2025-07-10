using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromWhsItemDispatchConsignment))]
	sealed class FreightWrapperFromWhsItemDispatchConsignmentTest : FreightWrapperTest
	{
		#region TestWrapperMappingsFull

		public void TestWrapperMappingsFull()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package1 = PackingHelper.CreatePackage(packageJob, "P1", 1, "PLT");
			package1.KP_Weight = 10;
			package1.KP_Volume = 11;
			var package2 = PackingHelper.CreatePackage(packageJob, "P2", 1, "PLT");
			package2.KP_Weight = 12;
			package2.KP_Volume = 13;

			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;

			SetupDocAddress(rcn, "RCN Booking Party", DocAddressType.BookingPartyDocumentaryAddress);
			SetupDocAddress(rcn, "Consignor Co", DocAddressType.LocalCartageExporter);

			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK, "STD");
			dcn.WDC_HouseBillNumber = "TEST House Bill";
			Helper.CreateAdditionalReference(dcn, "TEST Master Bill", AdditionalReferenceTypes.Codes.MasterBill);

			var packageStateRow1 = Helper.CreatePackageState(package1, "ARV", rcn, rtu, dcn);
			var packageStateRow2 = Helper.CreatePackageState(package2, "ARV", rcn, rtu, dcn);

			SetupDocAddress(dcn, "Consignor Co", DocAddressType.LocalCartageExporter);
			SetupDocAddress(dcn, "DCN Booking Party", DocAddressType.BookingPartyDocumentaryAddress);
			SetupDocAddress(dcn, "DCN Delivery Party", DocAddressType.ConsigneePickupDeliveryAddress);

			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals(typeof(TransitWarehouseDispatchConsignmentWrapper), wrapper.WarehouseJob.GetType());
			AssertEquals("WHS", wrapper.WarehouseJob.Warehouse.Code);
			AssertEquals("DCN", wrapper.JobNumber);
			AssertEquals("Dispatch Consignment", wrapper.JobNumberHeading);
			AssertEquals("DCN BOOKING PARTY", wrapper.BookingParty.CompanyName);
			AssertEquals("DCN BOOKING PARTY", wrapper.Client.CompanyName);
			AssertEquals("CONSIGNOR CO", wrapper.Consignor.CompanyName);
			AssertEquals("CONSIGNOR CO", wrapper.PickupAddress.CompanyName);
			AssertEquals("DCN DELIVERY PARTY", wrapper.DeliveryAddress.CompanyName);
			AssertEquals(2m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Packages.Value));
			AssertEquals(22m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Weight.Value));
			AssertEquals(24m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Volume.Value));
			AssertEquals(2m, wrapper.ShipmentOuterPacksQty.Value);
			AssertEquals("STD", wrapper.ServiceLevel.Code);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "TEST Master Bill" }, wrapper.CustomsEntries.Cast<CustomsEntryWrapper>().Select(c => c.EntryNumber));
			AssertEquals("TEST House Bill", wrapper.HouseBill);
			AssertEquals("TEST Master Bill", wrapper.MasterBill);
		}

		void SetupDocAddress(IDocAddresses dispatchConsignment, ZString company, DocAddressType addressType)
		{
			var bookingParty = dispatchConsignment.DocAddresses.AddNew(addressType);
			bookingParty.E2_AddressOverride = true;
			bookingParty.E2_CompanyName = company;
		}

		#endregion

		#region Formatting

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumber", "DCN" },
					{ "JobNumberHeading", "Dispatch Consignment" },
					{ "SecondaryNumber", "DCN" },
					{ "SecondaryHeading", "DCN Reference" },
					{ "MasterBillHeading", "Master Bill" },
					{ "HouseBillHeading", "House Bill" },
					{ "JobNumberBarcodeText", "^TDC=DCN;;|" },
					{ "JobNumberBarcodeTextForFont", "È^TDC=DCN;;|>Ê" },
					{ "JobNumberBarcodeTextWithoutDocManagerCodes", "ÈDCNGÊ" }
				};
			}
		}

		protected override string ExpectedBarcodeText()
		{
			return "È^TDC=DCN;CAD;|zÊ";
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
DepartureCFSTransport : 
WarehouseJob : (No Default Field Value Available on WarehouseJob)";
			}
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK);
			return dcn;
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromWhsItemDispatchConsignment((WhsItemDispatchConsignment)GetNewBusinessObjectToWrap(), Factory);
		}

		#endregion

		#region TestConsignee

		public void TestConsignee()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK);
			SetupDocAddress(dcn, "Test Company", DocAddressTypes.Codes.ConsigneeDocumentaryAddress);
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("Dispatch Consignment has Consignee name.", "TEST COMPANY", wrapper.Consignee.CompanyName);
		}

		#endregion

		#region TestExportReceivingCTOAddress

		public void TestExportReceivingCTOAddress()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK);
			dcn.CTODocAddress.Address1 = "CTO ADDRESS 1";

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("CTO ADDRESS 1", wrapper.ExportReceivingCTOAddress.AddressLine1);
		}

		#endregion

		#region TestPickupAddress

		public void TestPickupAddressFromReceiveConsignment()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("Dispatch Consignment has no Pickup Address name.", "", wrapper.PickupAddress.CompanyName);

			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK);
			SetupDocAddress(dcn2, "Consignor Co", DocAddressTypes.Codes.LocalCartageExporter);
			Factory.Save();

			wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn2, Factory);
			AssertEquals("Dispatch Consignment has Pickup Address name.", "CONSIGNOR CO", wrapper.PickupAddress.CompanyName);
		}

		public void TestPickupAddressFromFirstAndOnlyReceiveConsignment()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);
			SetupDocAddress(rcn, "RCN Consignor Co", DocAddressTypes.Codes.ConsignorPickupDeliveryAddress);

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;

			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK);
			SetupDocAddress(dcn, "DCN Consignor Co", DocAddressTypes.Codes.LocalCartageExporter);

			var package1 = PackingHelper.CreatePackage(packageJob, "", 1, "PLT");
			Helper.CreatePackageState(package1, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn, dispatchConsignment: dcn);

			var package2 = PackingHelper.CreatePackage(packageJob, "", 1, "BOX");
			Helper.CreatePackageState(package2, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn, dispatchConsignment: dcn);
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("Pickup Address name is from the only Receive Consignment.", "RCN CONSIGNOR CO", wrapper.PickupAddress.CompanyName);
		}

		public void TestPickupAddressFromFirstAndOnlyReceiveConsignment_NoPickupAddress()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;

			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK);
			SetupDocAddress(dcn, "DCN Consignor Co", DocAddressTypes.Codes.LocalCartageExporter);

			var package1 = PackingHelper.CreatePackage(packageJob, "", 1, "PLT");
			Helper.CreatePackageState(package1, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn, dispatchConsignment: dcn);

			var package2 = PackingHelper.CreatePackage(packageJob, "", 1, "BOX");
			Helper.CreatePackageState(package2, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn, dispatchConsignment: dcn);
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("Should fallback to Dispatch Consignment's Pickup Address.", "DCN CONSIGNOR CO", wrapper.PickupAddress.CompanyName);
		}

		public void TestPickupAddressFromFirstAndOnlyReceiveConsignment_MultipleRCNs()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK);
			SetupDocAddress(dcn, "DCN Consignor Co", DocAddressTypes.Codes.LocalCartageExporter);

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			SetupDocAddress(rcn1, "RCN1 Consignor Co", DocAddressTypes.Codes.LocalCartageExporter);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			SetupDocAddress(rcn2, "RCN2 Consignor Co", DocAddressTypes.Codes.LocalCartageExporter);

			var packageJob1 = Factory.NewWithValidTestData<PkgPackageJob>();
			packageJob1.KJ_ParentID = rcn1.PK;
			packageJob1.KJ_ParentTableCode = rcn1.TablePrefix;
			var packageJob2 = Factory.NewWithValidTestData<PkgPackageJob>();
			packageJob2.KJ_ParentID = rcn2.PK;
			packageJob2.KJ_ParentTableCode = rcn2.TablePrefix;

			var package1 = PackingHelper.CreatePackage(packageJob1, "", 1, "PLT");
			Helper.CreatePackageState(package1, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn1, dispatchConsignment: dcn);
			var package2 = PackingHelper.CreatePackage(packageJob2, "", 1, "BOX");
			Helper.CreatePackageState(package2, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn2, dispatchConsignment: dcn);
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("Should fallback to Dispatch Consignment's Pickup Address.", "DCN CONSIGNOR CO", wrapper.PickupAddress.CompanyName);
		}

		void SetupDocAddress(WhsItemReceiveConsignment receiveConsignment, ZString company, ZString addressType)
		{
			var bookingParty = Factory.New<JobDocAddress>();
			bookingParty.E2_AddressOverride = true;
			bookingParty.E2_CompanyName = company;
			bookingParty.E2_AddressType = addressType;
			bookingParty.E2_ParentID = receiveConsignment.PK;
			bookingParty.E2_ParentTableCode = receiveConsignment.TablePrefix;
		}

		void SetupDocAddress(WhsItemDispatchConsignment dispatchConsignment, ZString company, ZString addressType)
		{
			var bookingParty = Factory.New<JobDocAddress>();
			bookingParty.E2_AddressOverride = true;
			bookingParty.E2_CompanyName = company;
			bookingParty.E2_AddressType = addressType;
			bookingParty.E2_ParentID = dispatchConsignment.PK;
			bookingParty.E2_ParentTableCode = dispatchConsignment.TablePrefix;
		}

		#endregion

		#region TestDeliveryAddress

		public void TestDeliveryAddressFromDispatchConsignment()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("Dispatch Consignment has no Delivery Address name.", "", wrapper.DeliveryAddress.CompanyName);

			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK);
			SetupDocAddress(dcn2, "Test Company", DocAddressTypes.Codes.ConsigneePickupDeliveryAddress);
			Factory.Save();

			wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn2, Factory);
			AssertEquals("Dispatch Consignment has Delivery Address name.", "TEST COMPANY", wrapper.DeliveryAddress.CompanyName);
		}

		#endregion

		#region TestPackages

		public void TestPackages()
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();

			var package1 = Factory.New<PkgPackage>();
			package1.KP_PackageID = "P1";
			package1.KP_KJ_ParentPackageJob = packageJob.PK;

			var package2 = Factory.New<PkgPackage>();
			package2.KP_PackageID = "P2";
			package2.KP_KJ_ParentPackageJob = packageJob.PK;

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var dispatchConsignment = Helper.CreateDispatchConsignment("DC", warehouse.PK, "STD");
			var packageState = dispatchConsignment.PackageStates.AddNew();
			packageState.WPS_KP_Package = package1.PK;

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dispatchConsignment, Factory);
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
			var dispatchConsignment = Helper.CreateDispatchConsignment("DC", warehouse.PK, "STD");
			var packageState = dispatchConsignment.PackageStates.AddNew();
			packageState.WPS_KP_Package = package1.PK;
			package1.Packages.Add(package2);

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dispatchConsignment, Factory);
			AssertEquals(1, wrapper.Packages.Count);
		}

		#endregion

		#region TestJob

		public void TestJob()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK);
			var job = new JobHeader.Loader(dcn).TryLoadOrCreate();

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);

			AssertNotNull(wrapper.Job);
			AssertEquals("JobHeader", job.PK, wrapper.Job.PK);
		}

		#endregion

		#region TestJobHeaderLocalClient

		public override void TestJobHeaderLocalClient()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK);
			dcn.ClientRequestedBillToPartyDocAddress.CompanyName = "LOCAL CLIENT COMPANY NAME 2";

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertNotNull(wrapper);
			AssertEquals("wrapper.JobHeaderLocalClient.CompanyName", "LOCAL CLIENT COMPANY NAME 2", wrapper.JobHeaderLocalClient.CompanyName);

			wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			var job = new JobHeader.Loader(dcn).TryLoadOrCreate();
			var orgHeader = helper.CreateClient("OH1", "LOCAL CLIENT COMPANY NAME");
			job.LocalChargesPK = orgHeader.PK;
			AssertEquals("wrapper.JobHeaderLocalClient.CompanyName", "LOCAL CLIENT COMPANY NAME", wrapper.JobHeaderLocalClient.CompanyName);
		}

		#endregion

		#region TestMasterBill

		public void TestMasterBill()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("Load List has no Master Bill", "", wrapper.MasterBill);

			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK);
			Helper.CreateAdditionalReference(dcn2, "123123", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemDispatchConsignment(dcn2, Factory);
			AssertEquals("Dispatch Consignment has Master Bill", "123123", wrapper2.MasterBill);
		}

		#endregion

		#region Destination

		public void TestDestination_OneDLL_OnePackage()
		{
			var portCode = "NZTST";
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL", warehouse.PK, warehouse.Rows[0].Locations[0]);

			dll.WDL_RL_NKLastDischargePort = portCode;
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;
			var package1 = PackingHelper.CreatePackage(packageJob, "P1", 1, "PLT");

			Helper.CreatePackageState(package: package1, status: TransitWarehouseStatuses.Codes.Arrived, receiveConsignment: rcn, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll);
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("Expect DCN with one related DLL to return LastDischargePort", portCode, wrapper.Destination.Location.PortName);
		}

		public void TestDestination_OneDLL_MultiplePackages()
		{
			var portCode = "NZTST";
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL", warehouse.PK, warehouse.Rows[0].Locations[0]);
			dll.WDL_RL_NKLastDischargePort = portCode;
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;
			var package1 = PackingHelper.CreatePackage(packageJob, "P1", 1, "PLT");
			var package2 = PackingHelper.CreatePackage(packageJob, "P2", 1, "PLT");

			Helper.CreatePackageState(package: package1, status: TransitWarehouseStatuses.Codes.Arrived, receiveConsignment: rcn, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll);
			Helper.CreatePackageState(package: package2, status: TransitWarehouseStatuses.Codes.Arrived, receiveConsignment: rcn, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll);
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("Expect DCN with one related DLL to return LastDischargePort", portCode, wrapper.Destination.Location.PortName);
		}

		public void TestDestination_NoDLLs()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;
			var package1 = PackingHelper.CreatePackage(packageJob, "P1", 1, "PLT");
			var package2 = PackingHelper.CreatePackage(packageJob, "P2", 1, "PLT");

			Helper.CreatePackageState(package: package1, status: TransitWarehouseStatuses.Codes.Arrived, receiveConsignment: rcn, receiveUnit: rtu, dispatchConsignment: dcn);
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("Expect DCN with no DLL to return nothing for location", ZString.Empty, wrapper.Destination.Location.PortName);
		}

		public void TestDestination_MultipleDLLs()
		{
			var portCode = "NZTST";
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, warehouse.Rows[0].Locations[0]);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK, warehouse.Rows[0].Locations[0]);
			dll.WDL_RL_NKLastDischargePort = portCode;
			dll2.WDL_RL_NKLastDischargePort = portCode;
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;
			var package1 = PackingHelper.CreatePackage(packageJob, "P1", 1, "PLT");
			var package2 = PackingHelper.CreatePackage(packageJob, "P2", 1, "PLT");

			Helper.CreatePackageState(package: package1, status: TransitWarehouseStatuses.Codes.Arrived,
				receiveConsignment: rcn, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll);
			Helper.CreatePackageState(package: package2, status: TransitWarehouseStatuses.Codes.Arrived,
				receiveConsignment: rcn, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2);
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("Expect DCN with more than one DLL to return nothing for location", ZString.Empty, wrapper.Destination.Location.PortName);
		}

		#endregion

		#region TestMasterBillHeading

		public void TestMasterBillHeading()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, "STD");
			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dispatchConsignment, Factory);
			AssertEquals("Default Bill Heading is Master Bill", "Master Bill", wrapper.MasterBillHeading);

			var dispatchConsignment2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK, "STD");

			var packageJob = Factory.New<PkgPackageJob>();
			var package1 = PackingHelper.CreatePackage(packageJob, "P1", 1, "PLT");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;

			var dll = Helper.CreateDispatchLoadList("DLL", warehouse.PK, warehouse.Rows[0].Locations[0]);
			dispatchConsignment2.WDC_TransportMode = TransportModeList.Codes.Airfreight;

			Helper.CreatePackageState(package1, "ARV", rcn, rtu, dispatchConsignment2, dispatchLoadList: dll);

			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemDispatchConsignment(dispatchConsignment2, Factory);
			AssertEquals("Bill Heading for Air is MAWB", "MAWB", wrapper2.MasterBillHeading);
		}

		#endregion

		#region TestHouseBill

		public void TestHouseBill()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("Load List has no House Bill", "", wrapper.HouseBill);

			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK);
			dcn2.WDC_HouseBillNumber = "123123";
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemDispatchConsignment(dcn2, Factory);
			AssertEquals("Dispatch Consignment has House Bill", "123123", wrapper2.HouseBill);
		}

		#endregion

		#region TestHouseBillHeading

		public void TestHouseBillHeading()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, "STD");
			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dispatchConsignment, Factory);
			AssertEquals("Default House Bill Heading is House Bill", "House Bill", wrapper.HouseBillHeading);

			var dispatchConsignment2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK, "STD");

			var packageJob = Factory.New<PkgPackageJob>();
			var package1 = PackingHelper.CreatePackage(packageJob, "P1", 1, "PLT");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;

			var dll = Helper.CreateDispatchLoadList("DLL", warehouse.PK, warehouse.Rows[0].Locations[0]);
			dispatchConsignment2.WDC_TransportMode = TransportModeList.Codes.Airfreight;

			Helper.CreatePackageState(package1, "ARV", rcn, rtu, dispatchConsignment2, dispatchLoadList: dll);

			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemDispatchConsignment(dispatchConsignment2, Factory);
			AssertEquals("House Bill Heading for Air is HAWB", "HAWB", wrapper2.HouseBillHeading);
		}

		#endregion

		#region TestDestination

		public void TestDestination()
		{
			var zAJNB = new RefUNLOCO.Loader(Factory).Load("ZAJNB");
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dcn.WDC_RL_NKDestination = zAJNB.RL_Code;
			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);

			AssertEquals("wrapper.Destination.Location.Country.Code", zAJNB.RL_Code.Left(2), wrapper.Destination.Location.Country.Code);
			AssertEquals("wrapper.Destination.Location.IATACode", zAJNB.RL_IATA, wrapper.Destination.Location.IATACode);
			AssertEquals("wrapper.Destination.Location.PortName", zAJNB.RL_PortName, wrapper.Destination.Location.PortName);
			AssertEquals("wrapper.Destination.Location.UNLOCO", zAJNB.RL_Code, wrapper.Destination.Location.UNLOCO);
			AssertEquals("wrapper.Destination.Location.UNLOCOAndPortName", zAJNB.RL_Code + " - " + zAJNB.RL_PortName, wrapper.Destination.Location.UNLOCOAndPortName);
			AssertEquals("wrapper.Destination.Location.State", ZString.Empty, wrapper.Destination.Location.State);
		}

		#endregion

		#region TestJobNumber

		public void TestJobNumber_IsJobID()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, jobID: "DC00000001");
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("Job Number is Job ID", "DC00000001", wrapper.JobNumber);
		}

		#endregion

		#region TestTransitJobTransportMode

		public void TestTransitJobTransportMode()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, jobID: "DC00000001", transportMode: TransportModeList.Codes.Airfreight);

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("TransitJobTransportMode code", "AIR", wrapper.TransitJobTransportMode.Code);
			AssertEquals("TransitJobTransportMode description", "Air Freight", wrapper.TransitJobTransportMode.Description);
		}

		#endregion

		#region TestDepartureCFSTransport

		public override void TestDepartureCFSTransport()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, jobID: "DC00000001", transportMode: "AIR");
			SetupDocAddress(dcn, "TRANSCOMM", DocAddressTypes.Codes.TransportCompanyDocumentaryAddress);
			var transportCompany = Helper.CreateClient("TRANSCOMM");
			Helper.CreateJobDocAddressFromAddress(dcn, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertNotNull(wrapper.DepartureCFSTransport);
			AssertEquals("TRANSCOMM", wrapper.DepartureCFSTransport.CompanyName);
		}

		#endregion

		#region TestDocumentNumber

		public void TestDocumentNumber()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK);
			var package1 = Helper.CreatePackage(dcn.PackageJob, "P001", 1, "PKG");
			var package2 = Helper.CreatePackage(dcn.PackageJob, "P001", 1, "PKG");
			Factory.Save();

			var iwrapper = (IPackageOverrider)(new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory));
			iwrapper.SetPackageOverride(package2);
			AssertEquals(2, iwrapper.DocumentNumber);
		}

		#endregion

		#region TestSetPackageCollectionOverride

		public void TestSetPackageCollectionOverride()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK);
			var pkgJob = dcn.PackageJob;

			var package1 = Helper.CreatePackage(pkgJob, "P001", 1, "PKG");

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			(wrapper as IPackageOverrider).SetPackageCollectionOverride(new[] { package1 });
			AssertEquals("wrapper.Packages", 1, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages", new[] { package1 }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.WrappedObject));

			var package2 = Helper.CreatePackage(pkgJob, "P002", 1, "PKG");

			wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			(wrapper as IPackageOverrider).SetPackageCollectionOverride(new[] { package1, package2 });
			AssertEquals("wrapper.Packages", 2, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages", new[] { package1, package2 }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.WrappedObject));

			var loosePackageHeader1 = pkgJob.LoosePackageIDs.AddNew();
			loosePackageHeader1.KPH_PackageID = "XXX";
			loosePackageHeader1.CurrentPackageJob = pkgJob;

			wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			(wrapper as IPackageOverrider).SetPackageCollectionOverride(packageHeaders: new[] { loosePackageHeader1 });
			AssertEquals("wrapper.Packages", 1, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages", new[] { loosePackageHeader1 }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.WrappedObject));

			var loosePackageHeader2 = pkgJob.LoosePackageIDs.AddNew();
			loosePackageHeader2.KPH_PackageID = "YYY";
			loosePackageHeader2.CurrentPackageJob = pkgJob;

			wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			(wrapper as IPackageOverrider).SetPackageCollectionOverride(packageHeaders: new[] { loosePackageHeader1, loosePackageHeader2 });
			AssertEquals("wrapper.Packages", 2, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages", new[] { loosePackageHeader1, loosePackageHeader2 }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.WrappedObject));

			wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			(wrapper as IPackageOverrider).SetPackageCollectionOverride(new[] { package1, package2 }, new[] { loosePackageHeader1, loosePackageHeader2 });
			AssertEquals("wrapper.Packages", 4, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages", new[] { "P001", "P002", "XXX", "YYY" }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.RefNumber));
		}

		#endregion

		#region TestSecondaryHeading

		public void TestSecondaryHeading_IsReferenceNumber()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, jobID: "DC00000001");
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("Secondary Heading is Reference Number", "DCN Reference", wrapper.SecondaryHeading);
		}

		#endregion

		#region TestSecondaryNumber

		public void TestSecondaryNumber_IsReferenceNumber()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, jobID: "DC00000001");
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("Secondary Number is Reference Number", "DCN1", wrapper.SecondaryNumber);
		}

		#endregion

		#region TestService

		public void TestService()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "A");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var service = helper.CreateJobService(dcn.PK, "FUM");
			dcn.Services.Add(service);

			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals(1, wrapper.Services.Count);
			AssertEquals("FUM", wrapper.Services[0].Type.Code);
		}

		#endregion

		#region TestLinkedPackage

		public void TestLinkedPackage_LooseIDsMoreThanLoosePackages()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var rcn = Helper.CreateReceiveConsignment("WRCID", "STD", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("WDCID", warehouse.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID1", "BKD", dispatchConsignment: dcn);
			var loosePackageState1 = Helper.CreatePackageState(rcn, 2, "PLT", null, "BKD", dispatchConsignment: dcn);

			var looseIDHeader1 = dcn.PackageJob.LoosePackageIDs.AddNew();
			looseIDHeader1.KPH_PackageID = "LP1";
			looseIDHeader1.CurrentPackageJob = dcn.PackageJob;

			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			((IPackageOverrider)wrapper).SetPackageCollectionOverride(new[] { packageState1.Package }, new[] { looseIDHeader1 });

			AssertLinkedPackage(wrapper, "AlreadyHaveAnID1", rcn, packageState1, null, null);
			AssertLinkedPackage(wrapper, "LP1", rcn, loosePackageState1, null, null);
		}

		public void TestLinkedPackage_LooseIDMoreThanLoosePackage()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var rcn = Helper.CreateReceiveConsignment("WRCID", "STD", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("WDCID", warehouse.PK);

			var packageState = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID1", "BKD", dispatchConsignment: dcn);
			var loosePackageState = Helper.CreatePackageState(rcn, 2, "PLT", null, "BKD", dispatchConsignment: dcn);

			var looseIDHeader1 = dcn.PackageJob.LoosePackageIDs.AddNew();
			looseIDHeader1.KPH_PackageID = "LP1";
			looseIDHeader1.CurrentPackageJob = dcn.PackageJob;

			var looseIDHeader2 = dcn.PackageJob.LoosePackageIDs.AddNew();
			looseIDHeader2.KPH_PackageID = "LP2";
			looseIDHeader2.CurrentPackageJob = dcn.PackageJob;

			var looseIDHeader3 = dcn.PackageJob.LoosePackageIDs.AddNew();
			looseIDHeader3.KPH_PackageID = "LP3";
			looseIDHeader3.CurrentPackageJob = dcn.PackageJob;

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			((IPackageOverrider)wrapper).SetPackageCollectionOverride(new[] { packageState.Package }, new[] { looseIDHeader1, looseIDHeader2, looseIDHeader3 });

			AssertLinkedPackage(wrapper, "AlreadyHaveAnID1", rcn, packageState, null, null);
			AssertLinkedPackage(wrapper, "LP1", rcn, loosePackageState, null, null);
			AssertLinkedPackage(wrapper, "LP2", rcn, loosePackageState, null, null);
			AssertLinkedPackage(wrapper, "LP3", dcn, null, null, null);
		}

		void AssertLinkedPackage(FreightWrapperFromWhsItemDispatchConsignment wrapper, string refNumber, IJobNumber parent, WhsItemPackageState packageState, WhsItemPackageState handlingUnit, WhsItemPackageState topHandlingUnit)
		{
			var packageWrapper = wrapper.Packages.First(p => p["RefNumber"].ToString() == refNumber);

			if (parent != null)
			{
				AssertEquals(parent.JobNumber, (packageWrapper["Parent"] as BusinessObject)["JobNumber"]);
			}

			if (packageState != null)
			{
				var consignmentID = packageState.ReceiveConsignmentID;
				if (string.IsNullOrWhiteSpace(consignmentID))
				{
					consignmentID = packageState.DispatchConsignmentID;
				}
				AssertEquals(consignmentID, (packageWrapper["PackageState"] as BusinessObject)["ConsignmentID"]);
			}

			if (handlingUnit != null)
			{
				AssertEquals(handlingUnit.Package.KP_PackageID, (packageWrapper["HandlingUnit"] as BusinessObject)["RefNumber"]);
			}

			if (topHandlingUnit != null)
			{
				AssertEquals(topHandlingUnit.Package.KP_PackageID, (packageWrapper["TopLevelHandlingUnit"] as BusinessObject)["RefNumber"]);
			}
		}

		#endregion

		#region TestCount

		public void TestCount()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var rcn = Helper.CreateReceiveConsignment("WRCID", "STD", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("WDCID", warehouse.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID1", "BKD", dispatchConsignment: dcn);

			var looseID1 = dcn.PackageJob.LoosePackageIDs.AddNew();
			looseID1.KPH_PackageID = "LP1";
			looseID1.CurrentPackageJob = dcn.PackageJob;

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			((IPackageOverrider)wrapper).SetPackageCollectionOverride(new[] { packageState1.Package }, new[] { looseID1 });

			Assert(wrapper.Packages.All(p => (ZShort)p["OutterPackagesCount"] == 2));
		}

		public void TestCount_HandlintUnit()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var rcn = Helper.CreateReceiveConsignment("WRCID", "STD", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("WDCID", warehouse.PK);
			var location = Helper.CreateLocation(warehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID1", "BKD", dispatchConsignment: dcn);
			var innerPackageState2 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID2", "BKD", dispatchConsignment: dcn);
			var innerPackageState3 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID3", "BKD", dispatchConsignment: dcn);
			var loosePackageState1 = Helper.CreatePackageState(rcn, 2, "PLT", null, "BKD", dispatchConsignment: dcn);
			var loosePackageState2 = Helper.CreatePackageState(rcn, 2, "PLT", null, "BKD", dispatchConsignment: dcn);
			var loosePackageState3 = Helper.CreatePackageState(rcn, 2, "PLT", null, "BKD", dispatchConsignment: dcn);

			var hu1 = Helper.CreateHandlingUnitPackage("HU1", Helper.CreatePackageHandlingUnit(), rtu);
			Helper.PackPackageIntoHandlingUnit(hu1, loosePackageState1, ZDateTimeOffset.Now, "XXX", hu1);
			Helper.PackPackageIntoHandlingUnit(hu1, innerPackageState2, ZDateTimeOffset.Now, "XXX", hu1);

			var hu2 = Helper.CreateHandlingUnitPackage("HU2", Helper.CreatePackageHandlingUnit(), rtu);
			var topHU = Helper.CreateHandlingUnitPackage("TopHU", Helper.CreatePackageHandlingUnit(), rtu);
			Helper.PackPackageIntoHandlingUnit(hu2, innerPackageState3, ZDateTimeOffset.Now, "XXX", topHU);
			Helper.PackPackageIntoHandlingUnit(hu2, loosePackageState2, ZDateTimeOffset.Now, "XXX", topHU);
			Helper.PackPackageIntoHandlingUnit(topHU, hu2, ZDateTimeOffset.Now, "XXX", topHU);
			Helper.PackPackageIntoHandlingUnit(topHU, loosePackageState3, ZDateTimeOffset.Now, "XXX", topHU);

			var looseID1 = dcn.PackageJob.LoosePackageIDs.AddNew();
			looseID1.KPH_PackageID = "LP1";
			looseID1.CurrentPackageJob = dcn.PackageJob;

			var looseID2 = dcn.PackageJob.LoosePackageIDs.AddNew();
			looseID2.KPH_PackageID = "LP2";
			looseID2.CurrentPackageJob = dcn.PackageJob;

			var looseID3 = dcn.PackageJob.LoosePackageIDs.AddNew();
			looseID3.KPH_PackageID = "LP3";
			looseID3.CurrentPackageJob = dcn.PackageJob;

			var looseID4 = dcn.PackageJob.LoosePackageIDs.AddNew();
			looseID4.KPH_PackageID = "LP4";
			looseID4.CurrentPackageJob = dcn.PackageJob;

			var looseID5 = dcn.PackageJob.LoosePackageIDs.AddNew();
			looseID5.KPH_PackageID = "LP5";
			looseID5.CurrentPackageJob = dcn.PackageJob;

			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			((IPackageOverrider)wrapper).SetPackageCollectionOverride(
				new[] { packageState1.Package, innerPackageState2.Package, innerPackageState3.Package },
				new[] { looseID1, looseID2, looseID3, looseID4, looseID5 });

			Assert(wrapper.Packages.All(p => (ZShort)p["OutterPackagesCount"] == 8));
		}

		public void TestCount_OVP()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var rcn = Helper.CreateReceiveConsignment("WRCID", "STD", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("WDCID", warehouse.PK);
			var location = Helper.CreateLocation(warehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID1", "BKD", dispatchConsignment: dcn);
			var innerPackageState2 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID2", "BKD", dispatchConsignment: dcn);
			var innerPackageState3 = Helper.CreatePackageState(rcn, 1, "PLT", "AlreadyHaveAnID3", "BKD", dispatchConsignment: dcn);
			var loosePackageState1 = Helper.CreatePackageState(rcn, 2, "PLT", null, "BKD", dispatchConsignment: dcn);
			var loosePackageState2 = Helper.CreatePackageState(rcn, 2, "PLT", null, "BKD", dispatchConsignment: dcn);
			var loosePackageState3 = Helper.CreatePackageState(rcn, 2, "PLT", null, "BKD", dispatchConsignment: dcn);

			var ovp1 = Helper.CreateOverpackPackage("OVP1", rcn, rtu, rcn: rcn, dcn: dcn);
			Helper.PackPackageIntoHandlingUnit(ovp1, loosePackageState1, ZDateTimeOffset.Now, "XXX", ovp1);
			Helper.PackPackageIntoHandlingUnit(ovp1, innerPackageState2, ZDateTimeOffset.Now, "XXX", ovp1);

			var ovp2 = Helper.CreateOverpackPackage("OVP2", rcn, rtu, rcn: rcn, dcn: dcn);
			var topHU = Helper.CreateHandlingUnitPackage("TopHU", Helper.CreatePackageHandlingUnit(), rtu);
			Helper.PackPackageIntoHandlingUnit(ovp2, innerPackageState3, ZDateTimeOffset.Now, "XXX", topHU);
			Helper.PackPackageIntoHandlingUnit(ovp2, loosePackageState2, ZDateTimeOffset.Now, "XXX", topHU);
			Helper.PackPackageIntoHandlingUnit(topHU, ovp2, ZDateTimeOffset.Now, "XXX", topHU);
			Helper.PackPackageIntoHandlingUnit(topHU, loosePackageState3, ZDateTimeOffset.Now, "XXX", topHU);

			var looseID1 = dcn.PackageJob.LoosePackageIDs.AddNew();
			looseID1.KPH_PackageID = "LP1";
			looseID1.CurrentPackageJob = dcn.PackageJob;

			var looseID2 = dcn.PackageJob.LoosePackageIDs.AddNew();
			looseID2.KPH_PackageID = "LP2";
			looseID2.CurrentPackageJob = dcn.PackageJob;

			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			((IPackageOverrider)wrapper).SetPackageCollectionOverride(new[] { packageState1.Package }, new[] { looseID1, looseID2 });

			Assert(wrapper.Packages.All(p => (ZShort)p["OutterPackagesCount"] == 5));
		}

		#endregion

		#region TestShipmentType

		public void TestShipmentType()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK);

			var wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("wrapper.ShipmentType", "", wrapper.ShipmentType.Code);

			dcn.WDC_Direction = "IMP";

			wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("wrapper.ShipmentType", "IMP", wrapper.ShipmentType.Code);

			dcn.WDC_Direction = "EXP";

			wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("wrapper.ShipmentType", "EXP", wrapper.ShipmentType.Code);

			dcn.WDC_Direction = "DOM";

			wrapper = new FreightWrapperFromWhsItemDispatchConsignment(dcn, Factory);
			AssertEquals("wrapper.ShipmentType", "DOM", wrapper.ShipmentType.Code);
		}

		#endregion

		#region Implementation

		public override void TestOrgWrappersReturnTypesOnEmptyWrapper()
		{
			Assert(true);
		}

		protected override bool IsCarrierUsed
		{
			get { return false; }
		}

		#endregion

		#region Helpers

		WhsTransitTestHelper Helper
		{
			get { return helper ?? (helper = new WhsTransitTestHelper(Factory)); }
		}
		WhsTransitTestHelper helper;

		PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		#endregion
	}
}

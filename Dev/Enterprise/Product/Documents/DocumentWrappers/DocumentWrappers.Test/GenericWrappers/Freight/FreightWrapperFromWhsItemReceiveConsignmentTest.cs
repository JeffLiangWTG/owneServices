using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromWhsItemReceiveConsignment))]
	sealed class FreightWrapperFromWhsItemReceiveConsignmentTest : FreightWrapperTest
	{
		#region TestWrapperMappingsFull

		public void TestWrapperMappingsFull()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "Bobs Warehouse";
			
			var area = Factory.New<WhsArea>();
			area.WA_Name = "area";
			area.WA_WW_Whs = warehouse.PK;

			var row = Factory.New<WhsRow>();
			row.WR_WW_Whs = warehouse.PK;
			row.WR_Name = "A";
			row.WR_Levels = 3;
			row.WR_Columns = 3;
			row.WR_Trays = 3;

			Factory.Save();

			var receiveUnit = Helper.CreateReceiveTransportationUnit("Ref", warehouse.PK, row.Locations[0].PK);
			var receiveConsignment = Helper.CreateReceiveConsignment("CC1", "STD", warehouse.PK);

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = receiveConsignment.PK;
			packageJob.KJ_ParentTableCode = receiveConsignment.TablePrefix;

			var package1 = PackingHelper.CreatePackage(packageJob, "", 4, "PLT");
			package1.KP_Weight = 10;
			package1.KP_Volume = 11;
			var package2 = PackingHelper.CreatePackage(packageJob, "", 5, "PLT");
			package2.KP_Weight = 12;
			package2.KP_Volume = 13;

			var packageState1 = Helper.CreatePackageState(package1, "ARV", receiveConsignment, receiveUnit); // ARV - needs header but no location
			var packageState2 = Helper.CreatePackageState(package2, "ARV", receiveConsignment, receiveUnit);

			SetupDocAddress(receiveConsignment, "Booking Co", DocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			SetupDocAddress(receiveConsignment, "Consignor Co LCE", DocAddressTypes.Codes.LocalCartageExporter);
			SetupDocAddress(receiveConsignment, "Consignee Co", DocAddressTypes.Codes.ConsigneeDocumentaryAddress);
			SetupDocAddress(receiveConsignment, "Consignor Pickup Delivery Co", DocAddressTypes.Codes.ConsignorPickupDeliveryAddress);

			receiveConsignment.WRC_HouseBillNumber = "TEST House Bill";
			SetupAdditionalReference(receiveConsignment, "TEST Master Bill", AdditionalReferenceTypes.Codes.MasterBill);

			var transport = receiveConsignment.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_VoyageFlight = "ROUTING1";

			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(receiveConsignment, Factory);
			AssertEquals("WHS", wrapper.WarehouseJob.Warehouse.Code);
			AssertEquals("CC1", wrapper.JobNumber);
			AssertEquals("TEST House Bill", wrapper.HouseBill);
			AssertEquals("Receive Consignment", wrapper.JobNumberHeading);
			AssertEquals("BOOKING CO", wrapper.BookingParty.CompanyName);
			AssertEquals("BOOKING CO", wrapper.Client.CompanyName);
			AssertEquals("CONSIGNOR CO LCE", wrapper.Consignor.CompanyName);
			AssertEquals("CONSIGNOR PICKUP DELIVERY CO", wrapper.PickupAddress.CompanyName);
			AssertEquals("CONSIGNEE CO", wrapper.Consignee.CompanyName);
			AssertEquals("CONSIGNEE CO", wrapper.DeliveryAddress.CompanyName);
			AssertEquals(9m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Packages.Value));
			AssertEquals(22m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Weight.Value));
			AssertEquals(24m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Volume.Value));
			AssertEquals(9m, wrapper.ShipmentOuterPacksQty.Value);
			AssertEquals(22m, wrapper.Weight.Value);
			AssertEquals(24m, wrapper.Volume.Value);
			AssertEquals("STD", wrapper.ServiceLevel.Code);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "TEST Master Bill" }, wrapper.CustomsEntries.Cast<CustomsEntryWrapper>().Select(c => c.EntryNumber));
			AssertEquals(typeof(TransitWarehouseReceiveConsignmentWrapper), wrapper.WarehouseJob.GetType());

			AssertEquals("TEST House Bill", wrapper.HouseBill);
			AssertEquals("TEST Master Bill", wrapper.MasterBill);

			AssertEquals(1, wrapper.ConsolRoutes.Count);
			AssertEquals(1, wrapper.ShipmentRoutes.Count);
			AssertEquals("AUSYD", wrapper.ConsolRoutes["First"].Origin.UNLOCO);
			AssertEquals("NZAKL", wrapper.ConsolRoutes["First"].Destination.UNLOCO);
			AssertEquals("AUSYD", wrapper.ShipmentRoutes["First"].Origin.UNLOCO);
			AssertEquals("NZAKL", wrapper.ShipmentRoutes["First"].Destination.UNLOCO);
		}

		#endregion

		#region TestImportArrivalCTOAddress

		public void TestImportArrivalCTOAddress()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);

			rcn.CTODocAddress.Address1 = "CTO ADDRESS 1";

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);

			AssertEquals("CTO ADDRESS 1", wrapper.ImportArrivalCTOAddress.AddressLine1);
		}

		#endregion

		#region TestISetGeneratedPackageIDs

		public void TestISetGeneratedPackageIDs_SetGeneratedPackageIDs()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var receiveConsignment = Helper.CreateReceiveConsignment("CC1", "STD", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(receiveConsignment, Factory);
			var iWrapper = (IPackageOverrider)wrapper;

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(receiveConsignment);
			pkgJob.Packages.AddNew();
			pkgJob.Packages.AddNew();
			pkgJob.LoosePackageIDs.AddNew();
			var pkgeHeaderABC = pkgJob.LoosePackageIDs.AddNew();
			pkgeHeaderABC.KPH_PackageID = "ABC";
			pkgeHeaderABC.CurrentPackageJob = pkgJob;
			iWrapper.SetPackageCollectionOverride(packageHeaders: new[] { pkgeHeaderABC });
			AssertEquals(1, wrapper.Packages.Count);
			AssertEquals("Should contain Package IDs passed into interface", "ABC", wrapper.Packages[0].RefNumber);
		}

		#endregion

		#region TestJob

		public void TestJob()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);
			var job = new JobHeader.Loader(rcn).TryLoadOrCreate();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);

			AssertNotNull(wrapper.Job);
			AssertEquals("JobHeader", job.PK, wrapper.Job.PK);
		}

		#endregion

		#region TestJobHeaderLocalClient

		public override void TestJobHeaderLocalClient()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);
			rcn.ClientRequestedBillToPartyDocAddress.CompanyName = "LOCAL CLIENT COMPANY NAME 2";

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			AssertNotNull(wrapper);
			AssertEquals("wrapper.JobHeaderLocalClient.CompanyName", "LOCAL CLIENT COMPANY NAME 2", wrapper.JobHeaderLocalClient.CompanyName);

			wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			var job = new JobHeader.Loader(rcn).TryLoadOrCreate();
			var orgHeader = helper.CreateClient("OH1", "LOCAL CLIENT COMPANY NAME");
			job.LocalChargesPK = orgHeader.PK;
			AssertEquals("wrapper.JobHeaderLocalClient.CompanyName", "LOCAL CLIENT COMPANY NAME", wrapper.JobHeaderLocalClient.CompanyName);
		}

		#endregion

		#region TestMasterBill

		public void TestMasterBill_WhenHasPackageHeader()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);
			var packageHeader = rcn.PackageJob.LoosePackageIDs.AddNew();
			packageHeader.KPH_PackageID = "P0001";
			packageHeader.CurrentPackageJob = rcn.PackageJob;
			Helper.CreateAdditionalReference(rcn, "MAB", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			((IPackageOverrider)wrapper).SetPackageCollectionOverride(packageHeaders: new[] { packageHeader });
			AssertEquals($"Receive Consignment has MasterBill", "MAB", wrapper.MasterBill);
		}

		public void TestMasterBill_WhenHasPackage()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);
			var package = Helper.CreatePackage(rcn.PackageJob, "P001", 1, "PKG");
			var packageState = Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.Booked, rcn);
			Helper.CreateAdditionalReference(rcn, "MAB", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			((IPackageOverrider)wrapper).SetPackageOverride(package);
			AssertEquals($"Receive Consignment has MasterBill", "MAB", wrapper.MasterBill);
		}

		public void TestMasterBill_WhenHasNoPackage()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			AssertEquals($"Receive Consignment has no MasterBill", "", wrapper.MasterBill);
		}

		#endregion

		#region TestMasterBillHeading

		public void TestMasterBillHeading()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			Factory.Save();

			var receiveConsignment = Helper.CreateReceiveConsignment("CC1", "STD", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(receiveConsignment, Factory);
			AssertEquals("Default Bill Heading is Master Bill", "Master Bill", wrapper.MasterBillHeading);

			var receiveConsignment2 = Helper.CreateReceiveConsignment("CC2", "STD", warehouse.PK);
			receiveConsignment2.WRC_TransportMode = TransportModeList.Codes.Airfreight;
			Factory.Save();
			var wrapper2 = new FreightWrapperFromWhsItemReceiveConsignment(receiveConsignment2, Factory);
			AssertEquals("Bill Heading for Air is MAWB", "MAWB", wrapper2.MasterBillHeading);
		}

		#endregion

		#region TestHouseBill

		public void TestHouseBillFromReceiveConsignment()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			AssertEquals("Receive Consignment has no House Bill", "", wrapper.HouseBill);

			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			rcn2.WRC_HouseBillNumber = "123123";
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemReceiveConsignment(rcn2, Factory);
			AssertEquals("Receive Consignment has House Bill", "123123", wrapper2.HouseBill);
		}

		public void TestHouseBillFromFirstAndOnlyDispatchConsignment()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			rcn.WRC_HouseBillNumber = "RCN HSB";

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;

			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dcn.WDC_HouseBillNumber = "DCN HSB";

			var package1 = PackingHelper.CreatePackage(packageJob, "", 1, "PLT");
			Helper.CreatePackageState(package1, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn, dispatchConsignment: dcn);

			var package2 = PackingHelper.CreatePackage(packageJob, "", 1, "BOX");
			Helper.CreatePackageState(package2, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn, dispatchConsignment: dcn);
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			AssertEquals("House Bill should be from Package's first and only Dispatch Consignment.", "DCN HSB", wrapper.HouseBill);
		}

		public void TestHouseBillFromFirstAndOnlyDispatchConsignment_NoHouseBill()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			rcn.WRC_HouseBillNumber = "RCN HSB";

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;

			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var package1 = PackingHelper.CreatePackage(packageJob, "", 1, "PLT");
			Helper.CreatePackageState(package1, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn, dispatchConsignment: dcn);

			var package2 = PackingHelper.CreatePackage(packageJob, "", 1, "BOX");
			Helper.CreatePackageState(package2, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn, dispatchConsignment: dcn);
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			AssertEquals("Should fallback to Receive Consignment's House Bill.", "RCN HSB", wrapper.HouseBill);
		}

		public void TestHouseBillFromFirstAndOnlyDispatchConsignment_MultipleDCNs()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			rcn.WRC_HouseBillNumber = "RCN HSB";

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;

			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dcn1.WDC_HouseBillNumber = "DCN1 HSB";

			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK);
			dcn2.WDC_HouseBillNumber = "DCN2 HSB";

			var package1 = PackingHelper.CreatePackage(packageJob, "", 1, "PLT");
			Helper.CreatePackageState(package1, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn, dispatchConsignment: dcn1);

			var package2 = PackingHelper.CreatePackage(packageJob, "", 1, "BOX");
			Helper.CreatePackageState(package2, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn, dispatchConsignment: dcn2);
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			AssertEquals("Should fallback to Receive Consignment's House Bill.", "RCN HSB", wrapper.HouseBill);
		}

		#endregion

		#region TestHouseBillHeading

		public void TestHouseBillHeading()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			Factory.Save();

			var receiveConsignment = Helper.CreateReceiveConsignment("CC1", "STD", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(receiveConsignment, Factory);
			AssertEquals("Default House Bill Heading is House Bill", "House Bill", wrapper.HouseBillHeading);

			var receiveConsignment2 = Helper.CreateReceiveConsignment("CC2", "STD", warehouse.PK);
			receiveConsignment2.WRC_TransportMode = TransportModeList.Codes.Airfreight;
			SetupAdditionalReference(receiveConsignment2, "AIR", WarehouseAdditionalReferenceTypes.Codes.TransportMode);
			Factory.Save();
			var wrapper2 = new FreightWrapperFromWhsItemReceiveConsignment(receiveConsignment2, Factory);
			AssertEquals("House Bill Heading for Air is HAWB", "HAWB", wrapper2.HouseBillHeading);
		}

		#endregion

		#region TestConsignmentShort

		public void TestConsignmentShort()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";

			var area = Factory.New<WhsArea>();
			area.WA_Name = "area";
			area.WA_WW_Whs = warehouse.PK;

			var row = Factory.New<WhsRow>();
			row.WR_WW_Whs = warehouse.PK;
			row.WR_Name = "Row A";
			row.WR_Levels = 3;
			row.WR_Columns = 3;
			row.WR_Trays = 3;

			Factory.Save();

			var receiveConsignment = Helper.CreateReceiveConsignment("CC1", "STD", warehouse.PK);
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = receiveConsignment.PK;
			packageJob.KJ_ParentTableCode = receiveConsignment.TablePrefix;

			var rtu = Helper.CreateReceiveTransportationUnit("Ref", warehouse.PK, row.Locations[0].PK);

			var arrivedPackage = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "", TransitWarehouseStatuses.Codes.Arrived, rtu);
			arrivedPackage.Package.BookedDimensions.KPB_PackageQty = 0;

			var package_BookedDetailQuantityGreaterThanZero = Helper.CreatePackageState(receiveConsignment, 5, "PKG", "", TransitWarehouseStatuses.Codes.Booked);
			package_BookedDetailQuantityGreaterThanZero.Package.BookedDimensions.KPB_PackageQty = 5;

			var package_BookedDetailQuantityZero = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "", TransitWarehouseStatuses.Codes.Booked);
			package_BookedDetailQuantityZero.Package.BookedDimensions.KPB_PackageQty = 0;

			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(receiveConsignment, Factory);

			AssertEquals("Shorts:", 5, wrapper.Shorts);

			var looseID1 = receiveConsignment.PackageJob.LoosePackageIDs.AddNew();
			looseID1.KPH_PackageID = "LP1";
			looseID1.CurrentPackageJob = packageJob;
			wrapper = new FreightWrapperFromWhsItemReceiveConsignment(receiveConsignment, Factory);
			((IPackageOverrider)wrapper).SetPackageCollectionOverride(new[] { package_BookedDetailQuantityGreaterThanZero.Package }, new[] { looseID1 });
			AssertEquals("Shorts: ", 5, wrapper.Shorts);
		}

		public void TestConsignmentShort_LooseIDOnly()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var receiveConsignment = Helper.CreateReceiveConsignment("CC1", "STD", warehouse.PK);
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = receiveConsignment.PK;
			packageJob.KJ_ParentTableCode = receiveConsignment.TablePrefix;

			var package_BookedDetailQuantityGreaterThanZero = Helper.CreatePackageState(receiveConsignment, 5, "PKG", "", TransitWarehouseStatuses.Codes.Booked);
			package_BookedDetailQuantityGreaterThanZero.Package.BookedDimensions.KPB_PackageQty = 5;

			var looseID1 = receiveConsignment.PackageJob.LoosePackageIDs.AddNew();
			looseID1.KPH_PackageID = "LP1";
			looseID1.CurrentPackageJob = packageJob;

			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(receiveConsignment, Factory);
			((IPackageOverrider)wrapper).SetPackageCollectionOverride(null, new[] { looseID1 });

			AssertEquals("Shorts: ", 0, wrapper.Shorts);
		}

		#endregion

		#region TestConsignmentOver

		public void TestConsignmentOver()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";

			var area = Factory.New<WhsArea>();
			area.WA_Name = "area";
			area.WA_WW_Whs = warehouse.PK;

			var row = Factory.New<WhsRow>();
			row.WR_WW_Whs = warehouse.PK;
			row.WR_Name = "Row A";
			row.WR_Levels = 3;
			row.WR_Columns = 3;
			row.WR_Trays = 3;

			Factory.Save();

			var receiveConsignment = Helper.CreateReceiveConsignment("CC1", "STD", warehouse.PK);
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = receiveConsignment.PK;
			packageJob.KJ_ParentTableCode = receiveConsignment.TablePrefix;

			var rtu = Helper.CreateReceiveTransportationUnit("Ref", warehouse.PK, row.Locations[0].PK);

			Helper.CreatePackageState(receiveConsignment, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.Booked);

			var arrivedPackage_BookedDetailsQuantityEqualToZero1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "P2", TransitWarehouseStatuses.Codes.Arrived, rtu);
			arrivedPackage_BookedDetailsQuantityEqualToZero1.Package.BookedDimensions.KPB_PackageQty = 0;

			var arrivedPackage_BookedDetailsQuantityEqualToZero2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "P3", TransitWarehouseStatuses.Codes.Arrived, rtu);
			arrivedPackage_BookedDetailsQuantityEqualToZero2.Package.BookedDimensions.KPB_PackageQty = 0;

			var arrivedPackage_BookedDetailsQuantityGreaterThanZero1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "P4", TransitWarehouseStatuses.Codes.Arrived, rtu);
			arrivedPackage_BookedDetailsQuantityGreaterThanZero1.Package.BookedDimensions.KPB_PackageQty = 1;

			var arrivedPackage_BookedDetailsQuantityGreaterThanZero2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "P5", TransitWarehouseStatuses.Codes.Arrived, rtu);
			arrivedPackage_BookedDetailsQuantityGreaterThanZero2.Package.BookedDimensions.KPB_PackageQty = 1;

			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(receiveConsignment, Factory);

			AssertEquals("Overs: ", 2, wrapper.Overs);

			var looseID1 = receiveConsignment.PackageJob.LoosePackageIDs.AddNew();
			looseID1.KPH_PackageID = "LP1";
			looseID1.CurrentPackageJob = packageJob;
			wrapper = new FreightWrapperFromWhsItemReceiveConsignment(receiveConsignment, Factory);
			((IPackageOverrider)wrapper).SetPackageCollectionOverride(new[] { arrivedPackage_BookedDetailsQuantityEqualToZero1.Package }, new[] { looseID1 });
			AssertEquals("Overs: ", 1, wrapper.Overs);
		}

		public void TestConsignmentOver_LooseIDOnly()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var location = Helper.CreateLocation(warehouse);
			var receiveConsignment = Helper.CreateReceiveConsignment("CC1", "STD", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("Ref", warehouse.PK, location.PK);
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = receiveConsignment.PK;
			packageJob.KJ_ParentTableCode = receiveConsignment.TablePrefix;

			var arrivedPackage_BookedDetailsQuantityEqualToZero1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "P2", TransitWarehouseStatuses.Codes.Arrived, rtu);
			arrivedPackage_BookedDetailsQuantityEqualToZero1.Package.BookedDimensions.KPB_PackageQty = 0;

			var looseID1 = receiveConsignment.PackageJob.LoosePackageIDs.AddNew();
			looseID1.KPH_PackageID = "LP1";
			looseID1.CurrentPackageJob = packageJob;

			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(receiveConsignment, Factory);
			((IPackageOverrider)wrapper).SetPackageCollectionOverride(null, new[] { looseID1 });
			AssertEquals("Overs: ", 0, wrapper.Overs);
		}

		#endregion

		#region TestDestination

		public void TestDestination()
		{
			var zAJNB = new RefUNLOCO.Loader(Factory).Load("ZAJNB");
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);
			rcn.WRC_RL_NKDestination = zAJNB.RL_Code;
			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);

			AssertEquals("wrapper.Destination.Location.Country.Code", zAJNB.RL_Code.Left(2), wrapper.Destination.Location.Country.Code);
			AssertEquals("wrapper.Destination.Location.IATACode", zAJNB.RL_IATA, wrapper.Destination.Location.IATACode);
			AssertEquals("wrapper.Destination.Location.PortName", zAJNB.RL_PortName, wrapper.Destination.Location.PortName);
			AssertEquals("wrapper.Destination.Location.UNLOCO", zAJNB.RL_Code, wrapper.Destination.Location.UNLOCO);
			AssertEquals("wrapper.Destination.Location.UNLOCOAndPortName", zAJNB.RL_Code + " - " + zAJNB.RL_PortName, wrapper.Destination.Location.UNLOCOAndPortName);
			AssertEquals("wrapper.Destination.Location.State", ZString.Empty, wrapper.Destination.Location.State);
		}

		#endregion

		#region Formatting

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumberHeading", "Receive Consignment" },
					{ "SecondaryHeading", "RCN Reference" },
					{ "MasterBillHeading", "Master Bill" },
					{ "HouseBillHeading", "House Bill" },
					{ "JobNumber", "RCN" },
					{ "SecondaryNumber", "RCN" },
					{ "JobNumberBarcodeTextForFont", "È^TRO=RCN;;|ÅÊ" },
					{ "JobNumberBarcodeText", "^TRO=RCN;;|" },
					{ "JobNumberBarcodeTextWithoutDocManagerCodes", "ÈRCNUÊ" }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
ParentJob : RCN
WarehouseJob : (No Default Field Value Available on WarehouseJob)";
			}
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);
			return rcn;
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromWhsItemReceiveConsignment((WhsItemReceiveConsignment)GetNewBusinessObjectToWrap(), Factory);
		}

		#endregion

		#region TestPackages

		public void TestPackages()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package1 = PackingHelper.CreatePackage(packageJob, "", 4, "PLT");
			package1.KP_Weight = 10;
			package1.KP_Volume = 11;
			var package2 = PackingHelper.CreatePackage(packageJob, "", 5, "PLT");
			package2.KP_Weight = 12;
			package2.KP_Volume = 13;

			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "A");
			var receiveUnit = Helper.CreateReceiveTransportationUnit("Ref", warehouse.PK, warehouse.Rows[0].Locations[0].PK);
			var receiveConsignment = Helper.CreateReceiveConsignment("CC1", "STD", warehouse.PK);
			packageJob.KJ_ParentID = receiveConsignment.PK;
			packageJob.KJ_ParentTableCode = receiveConsignment.TablePrefix;

			Helper.CreatePackageState(package1, "ARV", receiveConsignment, receiveUnit); // ARV - needs header but no location
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(receiveConsignment, Factory);
			AssertEquals(1, wrapper.Packages.Count);
		}

		#endregion

		#region TestParentJob

		public void TestParentJob()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);

			AssertEquals("ParentJob should be a FreightWrapperFromWhsItemReceiveConsignment", typeof(FreightWrapperFromWhsItemReceiveConsignment), wrapper.ParentJob.GetType());
		}

		#endregion

		#region TestDocumentNumber

		public void TestDocumentNumber()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);
			var package1 = Helper.CreatePackage(rcn.PackageJob, "P001", 1, "PKG");
			var package2 = Helper.CreatePackage(rcn.PackageJob, "P001", 1, "PKG");
			Factory.Save();

			var iwrapper = (IPackageOverrider)(new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory));
			iwrapper.SetPackageOverride(package2);
			AssertEquals(2, iwrapper.DocumentNumber);
		}

		#endregion

		#region TestDeliveryAddress

		public void TestDeliveryAddressFromReceiveConsignment()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			AssertEquals("Receive Consignment has no Delivery Address name.", "", wrapper.DeliveryAddress.CompanyName);

			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			SetupDocAddress(rcn2, "Consignee Co", DocAddressTypes.Codes.ConsigneeDocumentaryAddress);
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemReceiveConsignment(rcn2, Factory);
			AssertEquals("Receive Consignment has Delivery Address name.", "CONSIGNEE CO", wrapper2.DeliveryAddress.CompanyName);
		}

		public void TestDeliveryAddressFromFirstAndOnlyDispatchConsignment()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			
			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);
			SetupDocAddress(rcn, "RCN Consignee Co", DocAddressTypes.Codes.ConsigneeDocumentaryAddress);

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;

			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK);
			SetupDocAddress(dcn, "DCN CONSIGNEE CO", DocAddressTypes.Codes.ConsigneePickupDeliveryAddress);

			var package1 = PackingHelper.CreatePackage(packageJob, "", 1, "PLT");
			Helper.CreatePackageState(package1, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn, dispatchConsignment: dcn);

			var package2 = PackingHelper.CreatePackage(packageJob, "", 1, "BOX");
			Helper.CreatePackageState(package2, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn, dispatchConsignment: dcn);
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			AssertEquals("Delivery Address name is from the only Dispatch Consignment.", "DCN CONSIGNEE CO", wrapper.DeliveryAddress.CompanyName);
		}

		public void TestDeliveryAddressFromFirstAndOnlyDispatchConsignment_NoDeliveryAddress()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);
			SetupDocAddress(rcn, "RCN Consignee Co", DocAddressTypes.Codes.ConsigneeDocumentaryAddress);

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;

			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK);

			var package1 = PackingHelper.CreatePackage(packageJob, "", 1, "PLT");
			Helper.CreatePackageState(package1, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn, dispatchConsignment: dcn);

			var package2 = PackingHelper.CreatePackage(packageJob, "", 1, "BOX");
			Helper.CreatePackageState(package2, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn, dispatchConsignment: dcn);
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			AssertEquals("Should fallback to Receive Consignment's Delivery Address.", "RCN CONSIGNEE CO", wrapper.DeliveryAddress.CompanyName);
		}

		public void TestDeliveryAddressFromFirstAndOnlyDispatchConsignment_MultipleDCNs()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);
			SetupDocAddress(rcn, "RCN Consignee Co", DocAddressTypes.Codes.ConsigneeDocumentaryAddress);

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;

			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			SetupDocAddress(dcn1, "DCN1 Consignee Co", DocAddressTypes.Codes.ConsigneeDocumentaryAddress);

			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK);
			SetupDocAddress(dcn2, "DCN2 Consignee Co", DocAddressTypes.Codes.ConsigneeDocumentaryAddress);

			var package1 = PackingHelper.CreatePackage(packageJob, "", 1, "PLT");
			Helper.CreatePackageState(package1, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn, dispatchConsignment: dcn1);

			var package2 = PackingHelper.CreatePackage(packageJob, "", 1, "BOX");
			Helper.CreatePackageState(package2, TransitWarehouseStatuses.Codes.Booked, receiveConsignment: rcn, dispatchConsignment: dcn2);
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			AssertEquals("Should fallback to Receive Consignment's Delivery Address.", "RCN CONSIGNEE CO", wrapper.DeliveryAddress.CompanyName);
		}

		#endregion

		#region TestJobNumber

		public void TestJobNumber_IsJobID()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dcn = Helper.CreateReceiveConsignment("RCN1", string.Empty, warehouse.PK, jobID: "RC00000001");
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(dcn, Factory);
			AssertEquals("Job Number is Job ID", "RC00000001", wrapper.JobNumber);
		}

		#endregion

		#region TestSecondaryHeading

		public void TestSecondaryHeading_IsReferenceNumber()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dcn = Helper.CreateReceiveConsignment("RCN1", string.Empty, warehouse.PK, jobID: "RC00000001");
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(dcn, Factory);
			AssertEquals("Secondary Heading is Reference Number", "RCN Reference", wrapper.SecondaryHeading);
		}

		#endregion

		#region TestSecondaryNumber

		public void TestSecondaryNumber_IsReferenceNumber()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dcn = Helper.CreateReceiveConsignment("RCN1", string.Empty, warehouse.PK, jobID: "RC00000001");
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(dcn, Factory);
			AssertEquals("Job Number is Reference Number", "RCN1", wrapper.SecondaryNumber);
		}

		#endregion

		#region TestService

		public void TestService()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "A");
			var receiveConsignment = Helper.CreateReceiveConsignment("CC1", "STD", warehouse.PK);
			var service = helper.CreateJobService(receiveConsignment.PK, "FUM");
			receiveConsignment.Services.Add(service);

			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(receiveConsignment, Factory);
			AssertEquals(1, wrapper.Services.Count);
			AssertEquals("FUM", wrapper.Services[0].Type.Code);
		}

		#endregion

		#region TestSetPackageCollectionOverride

		public void TestSetPackageCollectionOverride()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);
			var pkgJob = rcn.PackageJob;

			var package1 = Helper.CreatePackage(pkgJob, "P001", 1, "PKG");

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			(wrapper as IPackageOverrider).SetPackageCollectionOverride(new[] { package1 });
			AssertEquals("wrapper.Packages", 1, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages", new[] { package1 }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.WrappedObject));

			var package2 = Helper.CreatePackage(pkgJob, "P002", 1, "PKG");

			wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			(wrapper as IPackageOverrider).SetPackageCollectionOverride(new[] { package1, package2 });
			AssertEquals("wrapper.Packages", 2, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages", new[] { package1, package2 }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.WrappedObject));

			var loosePackageHeader1 = pkgJob.LoosePackageIDs.AddNew();
			loosePackageHeader1.KPH_PackageID = "XXX";
			loosePackageHeader1.CurrentPackageJob = pkgJob;

			wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			(wrapper as IPackageOverrider).SetPackageCollectionOverride(packageHeaders: new[] { loosePackageHeader1 });
			AssertEquals("wrapper.Packages", 1, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages", new[] { loosePackageHeader1 }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.WrappedObject));

			var loosePackageHeader2 = pkgJob.LoosePackageIDs.AddNew();
			loosePackageHeader2.KPH_PackageID = "YYY";
			loosePackageHeader2.CurrentPackageJob = pkgJob;

			wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			(wrapper as IPackageOverrider).SetPackageCollectionOverride(packageHeaders: new[] { loosePackageHeader1, loosePackageHeader2 });
			AssertEquals("wrapper.Packages", 2, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages", new[] { loosePackageHeader1, loosePackageHeader2 }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.WrappedObject));

			wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			(wrapper as IPackageOverrider).SetPackageCollectionOverride(new[] { package1, package2 }, new[] { loosePackageHeader1, loosePackageHeader2 });
			AssertEquals("wrapper.Packages", 4, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages", new[] { "P001", "P002", "XXX", "YYY" }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.RefNumber));
		}

		#endregion

		#region TestShipmentType

		public void TestShipmentType()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var rcn = Helper.CreateReceiveConsignment("RCN", warehouse.PK);

			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			AssertEquals("wrapper.ShipmentType", "", wrapper.ShipmentType.Code);

			rcn.WRC_Direction = "IMP";

			wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			AssertEquals("wrapper.ShipmentType", "IMP", wrapper.ShipmentType.Code);

			rcn.WRC_Direction = "EXP";

			wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			AssertEquals("wrapper.ShipmentType", "EXP", wrapper.ShipmentType.Code);

			rcn.WRC_Direction = "DOM";

			wrapper = new FreightWrapperFromWhsItemReceiveConsignment(rcn, Factory);
			AssertEquals("wrapper.ShipmentType", "DOM", wrapper.ShipmentType.Code);
		}

		#endregion

		#region TestTransitJobTransportMode

		public void TestTransitJobTransportMode()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "A");
			var receiveConsignment = Helper.CreateReceiveConsignment("CC1", "STD", warehouse.PK);
			Factory.Save();
			receiveConsignment.WRC_TransportMode = TransportModeList.Codes.Seafreight;
			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(receiveConsignment, Factory);
			AssertEquals("TransitJobTransportMode code", "SEA", wrapper.TransitJobTransportMode.Code);
			AssertEquals("TransitJobTransportMode description", "Sea Freight", wrapper.TransitJobTransportMode.Description);
		}

		#endregion

		#region TestWarehouseNextDischargePort

		public void TestWarehouseNextDischargePort()
		{
			var nextDischargePort = "PORT1";
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "A");
			var receiveConsignment = Helper.CreateReceiveConsignment("CC1", "STD", warehouse.PK);
			Factory.Save();

			receiveConsignment.WRC_RL_NKNextDischargePort = nextDischargePort;
			var wrapper = new FreightWrapperFromWhsItemReceiveConsignment(receiveConsignment, Factory);
			AssertEquals(nextDischargePort, wrapper.WarehouseNextDischargePort);
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

		protected override string ExpectedBarcodeText()
		{
			return "È^TRO=RCN;CAD;|ZÊ";
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

		void SetupAdditionalReference(WhsItemReceiveConsignment receiveConsignment, ZString number, ZString entryType)
		{
			var hsb = Factory.New<CusEntryNumber>();
			hsb.CE_ParentID = receiveConsignment.PK;
			hsb.CE_ParentTable = receiveConsignment.TableName;
			hsb.CE_EntryNum = number;
			hsb.CE_EntryType = entryType;
			receiveConsignment.AdditionalReferenceNumbers.Add(hsb);
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

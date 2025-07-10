using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromWhsItemReceiveTransportationUnit))]
	sealed class FreightWrapperFromWhsItemReceiveHeaderTest : FreightWrapperTest
	{
		#region TestWrapperMappingsFull

		public void TestWrapperMappingsFull()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			Factory.Save();

			var rtu = Helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			rcn.WRC_HouseBillNumber = "TEST House Bill";

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;

			var package1 = PackingHelper.CreatePackage(packageJob, "P1", 1, "PLT");
			package1.KP_Weight = 10;
			package1.KP_Volume = 11;
			var package2 = PackingHelper.CreatePackage(packageJob, "P2", 1, "PLT");
			package2.KP_Weight = 12;
			package2.KP_Volume = 13;

			Helper.CreateAdditionalReference(rtu, "TEST Master Bill", AdditionalReferenceTypes.Codes.MasterBill);

			var packageStateRow1 = Helper.CreatePackageState(package1, "ARV", rcn, rtu);
			var packageStateRow2 = Helper.CreatePackageState(package2, "ARV", rcn, rtu);

			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(rtu, Factory);
			AssertEquals(typeof(WhsItemReceiveTransportationUnitWrapper), wrapper.WarehouseJob.GetType());
			AssertEquals("WHS", wrapper.WarehouseJob.Warehouse.Code);
			AssertEquals("RTU", wrapper.JobNumber);
			AssertEquals("Transit Receive Transportation Unit", wrapper.JobNumberHeading);
			AssertEquals(2m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Packages.Value));
			AssertEquals(22m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Weight.Value));
			AssertEquals(24m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Volume.Value));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "TEST Master Bill" }, wrapper.CustomsEntries.Cast<CustomsEntryWrapper>().Select(c => c.EntryNumber));
			AssertEquals("TEST House Bill", wrapper.HouseBill);
			AssertEquals("TEST Master Bill", wrapper.MasterBill);
		}

		#endregion

		#region TestWarehouseJob

		public void TestWarehouseJob()
		{
			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(ReceiveTransportationUnit, Factory);
			AssertEquals(typeof(WhsItemReceiveTransportationUnitWrapper), wrapper.WarehouseJob.GetType());
		}

		#endregion

		#region ReceiveTransportationUnit

		WhsItemReceiveTransportationUnit ReceiveTransportationUnit
		{
			get { return receiveTransportationUnit ?? (receiveTransportationUnit = Factory.New<WhsItemReceiveTransportationUnit>()); }
		}
		WhsItemReceiveTransportationUnit receiveTransportationUnit;

		#endregion

		#region TestISetGeneratedPackageIDs

		public void TestISetGeneratedPackageIDs_SetGeneratedPackageIDs()
		{
			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(ReceiveTransportationUnit, Factory);
			var iWrapper = (IPackageOverrider)wrapper;
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(ReceiveTransportationUnit);
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

		#region TestPackages

		public void TestPackages()
		{
			var warehouse = Helper.CreateTRWWarehouse("TRW");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var uld = Helper.CreateReceiveTransportationUnitWithContainerType("ULD", warehouse.PK, warehouse.DefaultLocation.PK, "CNT1");
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();

			var packagePK = uld.PackageExtension.Package.PK;
			var packageStateForRTU = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, packagePK)).Single();
			packageStateForRTU.WPS_Status = TransitWarehouseStatuses.Codes.Arrived;
			packageStateForRTU.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			packageStateForRTU.WPS_WRH_TransitReceiveHeader = rtu.PK;

			var package1 = Factory.New<PkgPackage>();
			package1.KP_PackageID = "P1";
			package1.KP_KJ_ParentPackageJob = packageJob.PK;

			var package2 = Factory.New<PkgPackage>();
			package2.KP_PackageID = "P2";
			package2.KP_KJ_ParentPackageJob = packageJob.PK;

			var packageState = Helper.CreatePackageState(package1, TransitWarehouseStatuses.Codes.ArrivedPacked, receiveUnit: uld);
			packageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(packageStateForRTU, packageState, ZDateTimeOffset.Now, "ABC", packageStateForRTU);

			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(uld, Factory);
			AssertEquals(1, wrapper.Packages.Count);
		}

		#endregion

		#region TestMasterBill

		public void TestMasterBill()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var location = warehouse.DefaultInboundDockDoorLocation;
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(rtu, Factory);
			AssertEquals("Receive Transportation Unit has no Master Bill", "", wrapper.MasterBill);

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			Helper.CreateAdditionalReference(rtu2, "123123", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemReceiveTransportationUnit(rtu2, Factory);
			AssertEquals("Receive Transportation Unit has Master Bill", "123123", wrapper2.MasterBill);
		}

		#endregion

		#region TestMasterBillHeading

		public void TestMasterBillHeading()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(receiveTransportationUnit, Factory);
			AssertEquals("Default Bill Heading is Master Bill", "Master Bill", wrapper.MasterBillHeading);

			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, warehouse.DefaultLocation.PK);
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			receiveConsignment.WRC_TransportMode = TransportModeList.Codes.Airfreight;
			Helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", "ARV", receiveUnit: receiveTransportationUnit2);
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemReceiveTransportationUnit(receiveTransportationUnit2, Factory);
			AssertEquals("Bill Heading for Air is MAWB", "MAWB", wrapper2.MasterBillHeading);
		}

		#endregion

		#region TestHouseBill

		public void TestHouseBill()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var location = warehouse.DefaultInboundDockDoorLocation;
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(rtu, Factory);
			AssertEquals("Receive Transportation Unit has no House Bill", "", wrapper.HouseBill);

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			var asn = Helper.CreateReceiveASN("ASN2", warehouse.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;
			var package1 = PackingHelper.CreatePackage(packageJob, "P1", 1, "PLT");
			Helper.CreatePackageState(package1, "ARV", rcn, receiveASN: asn, receiveUnit: rtu2);
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemReceiveTransportationUnit(rtu2, Factory);
			AssertEquals("Receive Transportation Unit has no House Bill", "", wrapper.HouseBill);

			rcn.WRC_HouseBillNumber = "123123";
			Factory.Save();

			wrapper2 = new FreightWrapperFromWhsItemReceiveTransportationUnit(rtu2, Factory);
			AssertEquals("Receive Transportation Unit has House Bill", "123123", wrapper2.HouseBill);
		}

		#endregion

		#region TestHouseBillHeading

		public void TestHouseBillHeading()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("DLL1", warehouse.PK, warehouse.DefaultLocation.PK);
			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(receiveTransportationUnit, Factory);
			AssertEquals("Default House Bill Heading is House Bill", "House Bill", wrapper.HouseBillHeading);

			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("DLL2", warehouse.PK, warehouse.DefaultLocation.PK);
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			receiveConsignment.WRC_TransportMode = TransportModeList.Codes.Airfreight;
			Helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", "ARV", receiveUnit: receiveTransportationUnit2);
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemReceiveTransportationUnit(receiveTransportationUnit2, Factory);
			AssertEquals("House Bill Heading for Air is HAWB", "HAWB", wrapper2.HouseBillHeading);
		}

		#endregion

		#region TestJobNumberHeading

		public void TestJobNumberHeading()
		{
			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(ReceiveTransportationUnit, Factory);
			AssertEquals("Job Number Heading is Transit Receive Transportation Unit", "Transit Receive Transportation Unit", wrapper.JobNumberHeading);
		}

		#endregion

		#region TestJobNumber

		public void TestJobNumber()
		{
			ReceiveTransportationUnit.WRH_ReferenceNumber = "WDH00001";
			ReceiveTransportationUnit.WRH_VehicleReference = "DC00000001";
			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(ReceiveTransportationUnit, Factory);
			AssertEquals("Job Number is Reference Number", "WDH00001", wrapper.JobNumber);
		}

		#endregion

		#region TestSecondaryHeading

		public void TestSecondaryHeading_ContainerReference()
		{
			ReceiveTransportationUnit.WRH_UnitType = TransportUnitTypes.ULD;

			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(ReceiveTransportationUnit, Factory);
			AssertEquals("Secondary Heading is Vehicle Reference", "Container Reference", wrapper.SecondaryHeading);
		}

		public void TestSecondaryHeading_VehicleReference()
		{
			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(ReceiveTransportationUnit, Factory);
			AssertEquals("Secondary Heading is Vehicle Reference", "Vehicle Reference", wrapper.SecondaryHeading);
		}

		#endregion

		#region TestSecondaryNumber

		public void TestSecondaryNumber()
		{
			ReceiveTransportationUnit.WRH_ReferenceNumber = "WDH00001";
			ReceiveTransportationUnit.WRH_VehicleReference = "DC00000001";
			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(ReceiveTransportationUnit, Factory);
			AssertEquals("Secondary Number is Vehicle Number", "DC00000001", wrapper.SecondaryNumber);
		}

		#endregion

		#region TestSetPackageCollectionOverride

		public void TestSetPackageCollectionOverride()
		{
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(ReceiveTransportationUnit);

			var package1 = Helper.CreatePackage(pkgJob, "P001", 1, "PKG");

			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(ReceiveTransportationUnit, Factory);
			(wrapper as IPackageOverrider).SetPackageCollectionOverride(new[] { package1 });
			AssertEquals("wrapper.Packages", 1, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages", new[] { package1 }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.WrappedObject));

			var package2 = Helper.CreatePackage(pkgJob, "P002", 1, "PKG");

			wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(ReceiveTransportationUnit, Factory);
			(wrapper as IPackageOverrider).SetPackageCollectionOverride(new[] { package1, package2 });
			AssertEquals("wrapper.Packages", 2, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages", new[] { package1, package2 }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.WrappedObject));

			var loosePackageHeader1 = pkgJob.LoosePackageIDs.AddNew();
			loosePackageHeader1.KPH_PackageID = "XXX";
			loosePackageHeader1.CurrentPackageJob = pkgJob;

			wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(ReceiveTransportationUnit, Factory);
			(wrapper as IPackageOverrider).SetPackageCollectionOverride(packageHeaders: new[] { loosePackageHeader1 });
			AssertEquals("wrapper.Packages", 1, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages", new[] { loosePackageHeader1 }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.WrappedObject));

			var loosePackageHeader2 = pkgJob.LoosePackageIDs.AddNew();
			loosePackageHeader2.KPH_PackageID = "YYY";
			loosePackageHeader2.CurrentPackageJob = pkgJob;

			wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(ReceiveTransportationUnit, Factory);
			(wrapper as IPackageOverrider).SetPackageCollectionOverride(packageHeaders: new[] { loosePackageHeader1, loosePackageHeader2 });
			AssertEquals("wrapper.Packages", 2, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages", new[] { loosePackageHeader1, loosePackageHeader2 }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.WrappedObject));

			wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(ReceiveTransportationUnit, Factory);
			(wrapper as IPackageOverrider).SetPackageCollectionOverride(new[] { package1, package2 }, new[] { loosePackageHeader1, loosePackageHeader2 });
			AssertEquals("wrapper.Packages", 4, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages", new[] { "P001", "P002", "XXX", "YYY" }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.RefNumber));
		}

		#endregion

		#region TestGateInTime

		public void TestGateInTime()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var rtu = Helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var dateTime = ZDateTimeOffset.Now;
			rtu.WRH_GateInTime = dateTime;
			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(rtu, Factory);

			AssertEquals(dateTime, wrapper.GateInTime);
		}

		#endregion

		#region TestGateOutTime

		public void TestGateOutTime()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var rtu = Helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var dateTime = ZDateTimeOffset.Now;
			rtu.WRH_GateOutTime = dateTime;
			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(rtu, Factory);

			AssertEquals(dateTime, wrapper.GateOutTime);
		}

		#endregion

		#region TestTransportReference

		public void TestTransportReference()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var rtu = Helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			Helper.CreateAdditionalReference(rtu, "TEST Transport Reference", WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber);

			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(rtu, Factory);

			AssertEquals("TEST Transport Reference", wrapper.TransportReference);
		}

		#endregion

		#region TestBookingReference

		public void TestBookingReference()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var rtu = Helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var asn = Helper.CreateReceiveASN("ASN", warehouse.PK);
			Helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);
			Helper.CreateAdditionalReference(asn, "TEST Booking Reference", WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference);

			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(rtu, Factory);

			AssertEquals("TEST Booking Reference", wrapper.BookingReference);
		}

		#endregion

		#region TestArrivalCFSTransport

		public override void TestArrivalCFSTransport()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var transportCompany = Helper.CreateClient("TRANSCOMM");
			Factory.Save();

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(rtu, Factory);
			AssertNotNull(wrapper.ArrivalCFSTransport);
			AssertEquals("TRANSCOMM", wrapper.ArrivalCFSTransport.CompanyName);
		}

		#endregion

		#region TestJob

		public void TestJob()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var job = new JobHeader.Loader(rtu).TryLoadOrCreate();

			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(rtu, Factory);

			AssertNotNull(wrapper.Job);
			AssertEquals("JobHeader", job.PK, wrapper.Job.PK);
		}

		#endregion

		#region TestJobHeaderLocalClient

		public override void TestJobHeaderLocalClient()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			rtu.ClientRequestedBillToPartyDocAddress.CompanyName = "LOCAL CLIENT COMPANY NAME 2";

			var wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(rtu, Factory);
			AssertNotNull(wrapper);
			AssertEquals("wrapper.JobHeaderLocalClient.CompanyName", "LOCAL CLIENT COMPANY NAME 2", wrapper.JobHeaderLocalClient.CompanyName);

			wrapper = new FreightWrapperFromWhsItemReceiveTransportationUnit(rtu, Factory);
			var job = new JobHeader.Loader(rtu).TryLoadOrCreate();
			var orgHeader = helper.CreateClient("OH1", "LOCAL CLIENT COMPANY NAME");
			job.LocalChargesPK = orgHeader.PK;
			AssertEquals("wrapper.JobHeaderLocalClient.CompanyName", "LOCAL CLIENT COMPANY NAME", wrapper.JobHeaderLocalClient.CompanyName);
		}

		#endregion

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumberHeading", "Transit Receive Transportation Unit" },
					{ "SecondaryHeading", ReceiveTransportationUnit.IsContainerUnitType ? "Container Reference" : "Vehicle Reference" },
					{ "MasterBillHeading", "Master Bill" },
					{ "HouseBillHeading", "House Bill" }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
ArrivalCFSTransport : 
WarehouseJob : (No Default Field Value Available on WarehouseJob)";
			}
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);

			return rtu;
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromWhsItemReceiveTransportationUnit(null, Factory);
		}

		protected override string ExpectedBarcodeText()
		{
			return "È^TRU=RTU1;CAD;|JÊ";
		}

		#endregion

		#region Helpers

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
		PackingTestHelper packingHelper;

		#endregion
	}
}

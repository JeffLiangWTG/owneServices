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
	[TestedType(typeof(FreightWrapperFromWhsItemDispatchTransportationUnit))]
	sealed class FreightWrapperFromWhsItemDispatchHeaderTest : FreightWrapperTest
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

			var dll = Helper.CreateDispatchLoadList("DLL", warehouse.PK, warehouse.Rows[0].Locations[0]);
			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK, "STD");
			var dtu = Helper.CreateDispatchTransportationUnit("DTU", warehouse.PK);
			dcn.WDC_HouseBillNumber = "TEST House Bill";
			Helper.CreateAdditionalReference(dtu, "TEST Master Bill", AdditionalReferenceTypes.Codes.MasterBill);

			var packageStateRow1 = Helper.CreatePackageState(package1, "DEP", rcn, rtu, dcn, dtu, dll);
			packageStateRow1.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			var packageStateRow2 = Helper.CreatePackageState(package2, "DEP", rcn, rtu, dcn, dtu, dll);
			packageStateRow2.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(dtu, Factory);
			AssertEquals(typeof(WhsItemDispatchTransportationUnitWrapper), wrapper.WarehouseJob.GetType());
			AssertEquals("WHS", wrapper.WarehouseJob.Warehouse.Code);
			AssertEquals("DTU", wrapper.JobNumber);
			AssertEquals("Transit Dispatch Transportation Unit", wrapper.JobNumberHeading);
			AssertEquals(2m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Packages.Value));
			AssertEquals(22m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Weight.Value));
			AssertEquals(24m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Volume.Value));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "TEST Master Bill" }, wrapper.CustomsEntries.Cast<CustomsEntryWrapper>().Select(c => c.EntryNumber));
			AssertEquals("TEST House Bill", wrapper.HouseBill);
			AssertEquals("TEST Master Bill", wrapper.MasterBill);
		}

		#endregion

		public void TestWarehouseJob()
		{
			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(DispatchTransportationUnit, Factory);
			AssertEquals(typeof(WhsItemDispatchTransportationUnitWrapper), wrapper.WarehouseJob.GetType());
		}

		#region DispatchTransportationUnit

		WhsItemDispatchTransportationUnit DispatchTransportationUnit
		{
			get { return dispatchTransportationUnit ?? (dispatchTransportationUnit = Factory.New<WhsItemDispatchTransportationUnit>()); }
		}
		WhsItemDispatchTransportationUnit dispatchTransportationUnit;

		#endregion

		#region TestPackages

		public void TestPackages()
		{
			var warehouse = Helper.CreateTRWWarehouse("TRW");
			var dtu = Helper.CreateDispatchTransportationUnit("DTU", warehouse.PK);
			var uld = Helper.CreateDispatchTransportationUnitWithContainerType("ULD", warehouse.PK, "CNT1");
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();

			var packagePK = uld.PackageExtension.Package.PK;
			var packageStateForDTU = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, packagePK)).Single();
			packageStateForDTU.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
			packageStateForDTU.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			packageStateForDTU.WPS_WDH_TransitDispatchHeader = dtu.PK;

			var package1 = Factory.New<PkgPackage>();
			package1.KP_PackageID = "P1";
			package1.KP_KJ_ParentPackageJob = packageJob.PK;

			var package2 = Factory.New<PkgPackage>();
			package2.KP_PackageID = "P2";
			package2.KP_KJ_ParentPackageJob = packageJob.PK;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL", warehouse.PK, warehouse.DefaultLocation);
			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK, "STD");
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;

			var packageState = Helper.CreatePackageState(package1, TransitWarehouseStatuses.Codes.FreightLoaded, rcn, rtu, dcn, uld, dll);
			packageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(packageStateForDTU, packageState, ZDateTimeOffset.Now, "ABC", packageStateForDTU);

			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(uld, Factory);
			AssertEquals(1, wrapper.Packages.Count);
		}

		#endregion

		#region TestMasterBill

		public void TestMasterBill()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(dtu, Factory);
			AssertEquals("Dispatch Transportation Unit has no Master Bill", "", wrapper.MasterBill);

			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			Helper.CreateAdditionalReference(dtu2, "123123", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemDispatchTransportationUnit(dtu2, Factory);
			AssertEquals("Dispatch Transportation Unit has Master Bill", "123123", wrapper2.MasterBill);
		}

		#endregion

		#region TestBookingReference

		public void TestBookingReference()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dtu = Helper.CreateDispatchTransportationUnit("DTU", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL", warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			Helper.CreateAdditionalReference(dll, "TEST Booking Reference", WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference);

			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(dtu, Factory);

			AssertEquals("TEST Booking Reference", wrapper.BookingReference);
		}

		#endregion

		#region TestMasterBillHeading

		public void TestMasterBillHeading()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(dispatchTransportationUnit, Factory);
			AssertEquals("Default Bill Heading is Master Bill", "Master Bill", wrapper.MasterBillHeading);

			var dispatchTransportationUnit2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dispatchConsignment.WDC_TransportMode = TransportModeList.Codes.Airfreight;
			Helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", "FLO", receiveTransportationUnit, dispatchConsignment, dispatchTransportationUnit2, loadList, location: warehouse.DefaultOutboundDockDoorLocation);
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemDispatchTransportationUnit(dispatchTransportationUnit2, Factory);
			AssertEquals("Bill Heading for Air is MAWB", "MAWB", wrapper2.MasterBillHeading);
		}

		#endregion

		#region TestHouseBill

		public void TestHouseBill()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(dtu, Factory);
			AssertEquals("Dispatch Transportation Unit has no House Bill", "", wrapper.HouseBill);

			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dispatchConsignment.WDC_TransportMode = TransportModeList.Codes.Airfreight;
			Helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", "FLO", receiveTransportationUnit, dispatchConsignment, dtu2, loadList, location: warehouse.DefaultOutboundDockDoorLocation);
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemDispatchTransportationUnit(dtu2, Factory);
			AssertEquals("Dispatch Transportation Unit has no House Bill", "", wrapper.HouseBill);

			dispatchConsignment.WDC_HouseBillNumber = "123123";
			Factory.Save();
			var wrapper3 = new FreightWrapperFromWhsItemDispatchTransportationUnit(dtu2, Factory);
			AssertEquals("Dispatch Transportation Unit has House Bill", "123123", wrapper3.HouseBill);
		}

		#endregion

		#region TestHouseBillHeading

		public void TestHouseBillHeading()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(dispatchTransportationUnit, Factory);
			AssertEquals("Default House Bill Heading is House Bill", "House Bill", wrapper.HouseBillHeading);

			var dispatchTransportationUnit2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dispatchConsignment.WDC_TransportMode = TransportModeList.Codes.Airfreight;
			Helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", "FLO", receiveTransportationUnit, dispatchConsignment, dispatchTransportationUnit2, loadList, location: warehouse.DefaultOutboundDockDoorLocation);
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemDispatchTransportationUnit(dispatchTransportationUnit2, Factory);
			AssertEquals("House Bill Heading for Air is HAWB", "HAWB", wrapper2.HouseBillHeading);
		}

		#endregion

		#region TestJob

		public void TestJob()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dtu = Helper.CreateDispatchTransportationUnit("DTU", warehouse.PK);
			var job = new JobHeader.Loader(dtu).TryLoadOrCreate();

			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(dtu, Factory);

			AssertNotNull(wrapper.Job);
			AssertEquals("JobHeader", job.PK, wrapper.Job.PK);
		}

		#endregion

		#region TestJobHeaderLocalClient

		public override void TestJobHeaderLocalClient()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var dtu = Helper.CreateDispatchTransportationUnit("DTU", warehouse.PK);
			dtu.ClientRequestedBillToPartyDocAddress.CompanyName = "LOCAL CLIENT COMPANY NAME";

			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(dtu, Factory);
			AssertNotNull(wrapper);
			AssertEquals("wrapper.JobHeaderLocalClient.CompanyName", "LOCAL CLIENT COMPANY NAME", wrapper.JobHeaderLocalClient.CompanyName);

			wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(dtu, Factory);
			var job = new JobHeader.Loader(dtu).TryLoadOrCreate();
			var orgHeader = helper.CreateClient("OH1", "LOCAL CLIENT COMPANY NAME 1");
			job.LocalChargesPK = orgHeader.PK;
			AssertEquals("wrapper.JobHeaderLocalClient.CompanyName", "LOCAL CLIENT COMPANY NAME 1", wrapper.JobHeaderLocalClient.CompanyName);
		}

		#endregion

		#region TestJobNumberHeading

		public void TestJobNumberHeading()
		{
			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(DispatchTransportationUnit, Factory);
			AssertEquals("Job Number Heading is Transit Dispatch Transportation Unit", "Transit Dispatch Transportation Unit", wrapper.JobNumberHeading);
		}

		#endregion

		#region TestJobNumber

		public void TestJobNumber()
		{
			DispatchTransportationUnit.WDH_ReferenceNumber = "WDH00001";
			DispatchTransportationUnit.WDH_VehicleReference = "DC00000001";
			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(DispatchTransportationUnit, Factory);
			AssertEquals("Job Number is Reference Number", "WDH00001", wrapper.JobNumber);
		}

		#endregion

		#region TestSecondaryHeading

		public void TestSecondaryHeading_ContainerReference()
		{
			DispatchTransportationUnit.WDH_UnitType = TransportUnitTypes.ULD;

			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(DispatchTransportationUnit, Factory);
			AssertEquals("Secondary Heading is Vehicle Reference", "Container Reference", wrapper.SecondaryHeading);
		}

		public void TestSecondaryHeading_VehicleReference()
		{
			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(DispatchTransportationUnit, Factory);
			AssertEquals("Secondary Heading is Vehicle Reference", "Vehicle Reference", wrapper.SecondaryHeading);
		}

		#endregion

		#region TestSecondaryNumber

		public void TestSecondaryNumber()
		{
			DispatchTransportationUnit.WDH_ReferenceNumber = "WDH00001";
			DispatchTransportationUnit.WDH_VehicleReference = "DC00000001";
			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(DispatchTransportationUnit, Factory);
			AssertEquals("Secondary Number is Vehicle Number", "DC00000001", wrapper.SecondaryNumber);
		}

		#endregion

		#region TestGateInTime

		public void TestGateInTime()
		{
			var dateTime = ZDateTimeOffset.Now;
			DispatchTransportationUnit.WDH_GateInTime = dateTime;
			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(DispatchTransportationUnit, Factory);
			AssertEquals(dateTime, wrapper.GateInTime);
		}

		#endregion

		#region TestGateOutTime

		public void TestGateOutTime()
		{
			var dateTime = ZDateTimeOffset.Now;
			DispatchTransportationUnit.WDH_GateOutTime = dateTime;

			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(DispatchTransportationUnit, Factory);
			AssertEquals(dateTime, wrapper.GateOutTime);
		}

		#endregion

		#region TestDepartureCFSTransport

		public override void TestDepartureCFSTransport()
		{
			var transportCompany = Helper.CreateClient("TRANSCOMM");
			Helper.CreateJobDocAddressFromAddress(DispatchTransportationUnit, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);

			var wrapper = new FreightWrapperFromWhsItemDispatchTransportationUnit(DispatchTransportationUnit, Factory);
			AssertNotNull(wrapper.DepartureCFSTransport);
			AssertEquals("TRANSCOMM", wrapper.DepartureCFSTransport.CompanyName);
		}

		#endregion

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumberHeading", "Transit Dispatch Transportation Unit" },
					{ "SecondaryHeading", DispatchTransportationUnit.IsContainerUnitType ? "Container Reference" : "Vehicle Reference" },
					{ "MasterBillHeading", "Master Bill" },
					{ "HouseBillHeading", "House Bill" }
				};
			}
		}

		protected override string ExpectedBarcodeText()
		{
			return "È^TDU=DTU;CAD;|<Ê";
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
			var dtu = Helper.CreateDispatchTransportationUnit("DTU", warehouse.PK);
			return dtu;
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromWhsItemDispatchTransportationUnit(null, Factory);
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

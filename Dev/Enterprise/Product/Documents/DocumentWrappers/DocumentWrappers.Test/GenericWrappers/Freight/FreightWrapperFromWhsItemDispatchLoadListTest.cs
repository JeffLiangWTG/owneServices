using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromWhsItemDispatchLoadList))]
	sealed class FreightWrapperFromWhsItemDispatchLoadListTest : FreightWrapperTest
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
			var innerPackage = package2.Packages.AddNew("PLT", "P201");
			innerPackage.KP_Weight = 14;
			innerPackage.KP_Volume = 15;

			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;

			var dll = Helper.CreateDispatchLoadList("DLL", warehouse.PK, warehouse.Rows[0].Locations[0]);
			dll.WDL_ReferenceNumber = "ReferenceNumber";
			dll.WDL_Priority = (ZByte)1;
			Helper.CreateAdditionalReference(dll, "TEST Master Bill", AdditionalReferenceTypes.Codes.MasterBill);
			Helper.CreateAdditionalReference(dll, "TEST Booking Reference", "CBR");

			var transport = dll.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_VoyageFlight = "ROUTING1";

			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK, "STD");
			dcn.WDC_HouseBillNumber = "TEST House Bill";

			var packageStateRow1 = Helper.CreatePackageState(package1, "ARV", rcn, rtu, dcn, dispatchLoadList: dll);
			var packageStateRow2 = Helper.CreatePackageState(package2, "ARV", rcn, rtu, dcn, dispatchLoadList: dll);

			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemDispatchLoadList(dll, Factory);
			AssertEquals(typeof(WhsItemDispatchLoadListWrapper), wrapper.WarehouseJob.GetType());
			AssertEquals((ZByte)1, wrapper.Priority);
			AssertEquals("WHS", wrapper.WarehouseJob.Warehouse.Code);
			AssertEquals("DLL", wrapper.JobNumber);
			AssertEquals("Dispatch Load List", wrapper.JobNumberHeading);
			AssertEquals("ReferenceNumber", wrapper.SecondaryNumber);
			AssertEquals("Dispatch Load List Reference Number", wrapper.SecondaryHeading);
			AssertEquals(2, wrapper.Packages.Cast<PackageWrapper>().Count());
			AssertEquals(2m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Packages.Value));
			AssertEquals(36m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Weight.Value));
			AssertEquals(24m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Volume.Value));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "TEST Master Bill", "TEST Booking Reference" }, wrapper.CustomsEntries.Cast<CustomsEntryWrapper>().Select(c => c.EntryNumber));
			AssertEquals("TEST House Bill", wrapper.HouseBill);
			AssertEquals("TEST Master Bill", wrapper.MasterBill);
			AssertEquals("TEST Booking Reference", wrapper.BookingReference);
			AssertEquals(1, wrapper.ConsolRoutes.Count);
			AssertEquals(1, wrapper.ShipmentRoutes.Count);
			AssertEquals("AUSYD", wrapper.ConsolRoutes["First"].Origin.UNLOCO);
			AssertEquals("NZAKL", wrapper.ConsolRoutes["First"].Destination.UNLOCO);
			AssertEquals("AUSYD", wrapper.ShipmentRoutes["First"].Origin.UNLOCO);
			AssertEquals("NZAKL", wrapper.ShipmentRoutes["First"].Destination.UNLOCO);
		}

		#endregion

		#region TestMasterBill

		public void TestMasterBill()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemDispatchLoadList(dispatchLoadList, Factory);
			AssertEquals("Load List has no Master Bill", "", wrapper.MasterBill);

			var dispatchLoadList2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			Helper.CreateAdditionalReference(dispatchLoadList2, "123123", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemDispatchLoadList(dispatchLoadList2, Factory);
			AssertEquals("Load List has Master Bill", "123123", wrapper2.MasterBill);
		}

		#endregion

		#region TestDestination

		public void TestDestination()
		{
			var portCode = new ZString("NZTST");
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dispatchLoadList.WDL_RL_NKLastDischargePort = portCode;
			var wrapper = new FreightWrapperFromWhsItemDispatchLoadList(dispatchLoadList, Factory);

			Factory.Save();

			AssertEquals("Load List has Last Discharge Port", portCode, wrapper.Destination.Location.PortName);
		}

		#endregion

		#region TestMasterBillHeading

		public void TestMasterBillHeading()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemDispatchLoadList(dispatchLoadList, Factory);
			AssertEquals("Default Bill Heading is Master Bill", "Master Bill", wrapper.MasterBillHeading);

			var dispatchLoadList2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			dispatchLoadList2.WDL_TransportMode = TransportModeList.Codes.Airfreight;
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemDispatchLoadList(dispatchLoadList2, Factory);
			AssertEquals("Bill Heading for Air is MAWB", "MAWB", wrapper2.MasterBillHeading);
		}

		#endregion

		#region TestHouseBill

		public void TestHouseBill()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemDispatchLoadList(dispatchLoadList, Factory);
			AssertEquals("Load List has no House Bill", "", wrapper.HouseBill);

			var dispatchLoadList2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);

			var packageJob = Factory.New<PkgPackageJob>();
			var package1 = PackingHelper.CreatePackage(packageJob, "P1", 1, "PLT");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			packageJob.KJ_ParentID = rcn.PK;
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;
			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK, "STD");
			var packageStateRow1 = Helper.CreatePackageState(package1, "ARV", rcn, rtu, dcn, dispatchLoadList: dispatchLoadList2);
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemDispatchLoadList(dispatchLoadList2, Factory);
			AssertEquals("Load List has no House Bill", "", wrapper2.HouseBill);

			dcn.WDC_HouseBillNumber = "123123";
			Factory.Save();

			var wrapper3 = new FreightWrapperFromWhsItemDispatchLoadList(dispatchLoadList2, Factory);
			AssertEquals("Load List has House Bill", "123123", wrapper3.HouseBill);
		}

		#endregion

		#region TestHouseBillHeading

		public void TestHouseBillHeading()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemDispatchLoadList(dispatchLoadList, Factory);
			AssertEquals("Default House Bill Heading is House Bill", "House Bill", wrapper.HouseBillHeading);

			var dispatchLoadList2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			dispatchLoadList2.WDL_TransportMode = TransportModeList.Codes.Airfreight;
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemDispatchLoadList(dispatchLoadList2, Factory);
			AssertEquals("House Bill Heading for Air is HAWB", "HAWB", wrapper2.HouseBillHeading);
		}

		#endregion

		#region TestTransitJobTransportMode

		public void TestTransitJobTransportMode()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL", warehouse.PK);
			dispatchLoadList.WDL_TransportMode = TransportModeList.Codes.Seafreight;

			var wrapper = new FreightWrapperFromWhsItemDispatchLoadList(dispatchLoadList, Factory);
			AssertEquals("TransitJobTransportMode code", "SEA", wrapper.TransitJobTransportMode.Code);
			AssertEquals("TransitJobTransportMode description", "Sea Freight", wrapper.TransitJobTransportMode.Description);
		}
		#endregion

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumberHeading", "Dispatch Load List" },
					{ "SecondaryHeading", "Dispatch Load List Reference Number" },
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
WarehouseJob : (No Default Field Value Available on WarehouseJob)";
			}
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<WhsItemDispatchLoadList>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromWhsItemDispatchLoadList((WhsItemDispatchLoadList)GetNewBusinessObjectToWrap(), Factory);
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

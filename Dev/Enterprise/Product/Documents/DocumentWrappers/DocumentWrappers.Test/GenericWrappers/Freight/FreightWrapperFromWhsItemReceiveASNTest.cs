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
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromWhsItemReceiveASN))]
	sealed class FreightWrapperFromWhsItemReceiveASNTest : FreightWrapperTest
	{
		#region TestWrapperMappingsFull

		public void TestWrapperMappingsFull()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");

			var asn = Helper.CreateReceiveASN("ASN", warehouse.PK);
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

			var packageState1 = Helper.CreatePackageState(package1, "BKD", rcn, receiveASN: asn);
			var packageState2 = Helper.CreatePackageState(package2, "BKD", rcn, receiveASN: asn);

			Helper.CreateAdditionalReference(asn, "TEST Master Bill", AdditionalReferenceTypes.Codes.MasterBill);

			var transport = asn.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_VoyageFlight = "ROUTING1";

			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveASN(asn, Factory);
			AssertEquals(typeof(WhsItemReceiveASNWrapper), wrapper.WarehouseJob.GetType());
			AssertEquals("WHS", wrapper.WarehouseJob.Warehouse.Code);
			AssertEquals("ASN", wrapper.JobNumber);
			AssertEquals("Warehouse Reference", wrapper.JobNumberHeading);
			AssertEquals(2m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Packages.Value));
			AssertEquals(22m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Weight.Value));
			AssertEquals(24m, wrapper.Packages.Cast<PackageWrapper>().Sum(p => p.Volume.Value));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "TEST Master Bill" }, wrapper.CustomsEntries.Cast<CustomsEntryWrapper>().Select(c => c.EntryNumber));
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

		#region TestMasterBill

		public void TestMasterBill()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemReceiveASN(asn, Factory);
			AssertEquals("Receive ASN has no Master Bill", "", wrapper.MasterBill);

			var asn2 = Helper.CreateReceiveASN("ASN2", warehouse.PK);
			Helper.CreateAdditionalReference(asn2, "123123", WarehouseAdditionalReferenceTypes.Codes.MasterBill);
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemReceiveASN(asn2, Factory);
			AssertEquals("Receive ASN has Master Bill", "123123", wrapper2.MasterBill);
		}

		#endregion

		#region TestMasterBillHeading

		public void TestMasterBillHeading()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var receiveASN = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemReceiveASN(receiveASN, Factory);
			AssertEquals("Default Bill Heading is Master Bill", "Master Bill", wrapper.MasterBillHeading);

			var receiveASN2 = Helper.CreateReceiveASN("ASN2", warehouse.PK);
			receiveASN2.WRP_TransportMode = TransportModeList.Codes.Airfreight;
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemReceiveASN(receiveASN2, Factory);
			AssertEquals("Bill Heading for Air is MAWB", "MAWB", wrapper2.MasterBillHeading);
		}

		#endregion

		#region TestHouseBill

		public void TestHouseBill()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemReceiveASN(asn, Factory);
			AssertEquals("Receive ASN has no House Bill", "", wrapper.HouseBill);

			var asn2 = Helper.CreateReceiveASN("ASN2", warehouse.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN", "STD", warehouse.PK);
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentTableCode = rcn.TablePrefix;
			var package1 = PackingHelper.CreatePackage(packageJob, "P1", 1, "PLT");
			Helper.CreatePackageState(package1, "BKD", rcn, receiveASN: asn2);
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemReceiveASN(asn2, Factory);
			AssertEquals("Receive ASN has no House Bill", "", wrapper2.HouseBill);

			rcn.WRC_HouseBillNumber = "123123";
			Factory.Save();
			wrapper2 = new FreightWrapperFromWhsItemReceiveASN(asn2, Factory);
			AssertEquals("Receive ASN has House Bill", "123123", wrapper2.HouseBill);
		}

		#endregion

		#region TestHouseBillHeading

		public void TestHouseBillHeading()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var receiveASN = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemReceiveASN(receiveASN, Factory);
			AssertEquals("Default House Bill Heading is House Bill", "House Bill", wrapper.HouseBillHeading);

			var receiveASN2 = Helper.CreateReceiveASN("ASN2", warehouse.PK);
			receiveASN2.WRP_TransportMode = TransportModeList.Codes.Airfreight;
			Helper.CreateAdditionalReference(receiveASN2, "AIR", WarehouseAdditionalReferenceTypes.Codes.TransportMode);
			Factory.Save();

			var wrapper2 = new FreightWrapperFromWhsItemReceiveASN(receiveASN2, Factory);
			AssertEquals("House Bill Heading for Air is HAWB", "HAWB", wrapper2.HouseBillHeading);
		}

		#endregion

		#region TestTransportReference

		public void TestTransportReference()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var asn = Helper.CreateReceiveASN("ASN", warehouse.PK);
			asn.WRP_VehicleReference = "Vehicle Reference";
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveASN(asn, Factory);
			AssertEquals("Vehicle Reference", wrapper.TransportReference);
		}

		#endregion

		#region TestArrivalCFSTransport

		public override void TestArrivalCFSTransport()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var transportCompany = Helper.CreateClient("TRANSCOMM");
			Factory.Save();

			var asn = Helper.CreateReceiveASN("ASN", warehouse.PK);
			Helper.CreateJobDocAddressFromAddress(asn, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			var wrapper = new FreightWrapperFromWhsItemReceiveASN(asn, Factory);
			AssertNotNull(wrapper.ArrivalCFSTransport);
			AssertEquals("TRANSCOMM", wrapper.ArrivalCFSTransport.CompanyName);
		}

		#endregion

		#region TestTransitJobTransportMode

		public void TestTransitJobTransportMode()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			var asn = Helper.CreateReceiveASN("ASN", warehouse.PK);
			asn.WRP_TransportMode = TransportModeList.Codes.Airfreight;
			Factory.Save();

			var wrapper = new FreightWrapperFromWhsItemReceiveASN(asn, Factory);
			AssertEquals("TransitJobTransportMode code", "AIR", wrapper.TransitJobTransportMode.Code);
			AssertEquals("TransitJobTransportMode description", "Air Freight", wrapper.TransitJobTransportMode.Description);
		}

		#endregion

		#region TestBookingReference

		public void TestBookingReference()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var asn = Helper.CreateReceiveASN("ASN", warehouse.PK);
			var additionalReference = helper.CreateAdditionalReference(asn, "TestBooking", WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference);
			var wrapper = new FreightWrapperFromWhsItemReceiveASN(asn, Factory);
			AssertEquals("TestBooking", wrapper.BookingReference);
		}

		#endregion

		#region BookingParty

		public void TestGetBookingParty()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var asn = Helper.CreateReceiveASN("ASN", warehouse.PK);
			var wrapper = new FreightWrapperFromWhsItemReceiveASN(asn, Factory);
			AssertEquals("EAGLE DATAMATION INTERNATIONAL", wrapper.BookingParty.BrandName.ToString());
		}

		#endregion

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumberHeading", "Warehouse Reference" },
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
			return Factory.New<WhsItemReceiveASN>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromWhsItemReceiveASN(null, Factory);
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

using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Sailing
{
	[TestedType(typeof(DocSailing))]
	sealed class DocSailingTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocSailing.New(Sailing, Factory) };
		}

		#region TestHazardousCutOffDate

		public void TestHazardousCutOffDate()
		{
			Origin.JA_DGCutOff = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, SailingWrapper.HazardousCutOff);
		}

		#endregion

		#region TestHazardousReceivingDates

		public void TestHazardousReceivalCommences()
		{
			Origin.JA_DGReceivalCommences = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, SailingWrapper.HazardousReceivalCommences);
		}

		#endregion

		public void TestPacklines()
		{
			Sailing.Origin.JA_RL_NKPortOfLoading = "AUSYD";
			Sailing.Destination.JB_RL_NKPortOfDischarge = "NZAKL";

			JobVoyage secondVoyage = (JobVoyage)Voyage.TemplateCopy();
			JobSailing secondSailing = secondVoyage.Sailings.GetSailingFromLoadAndDischarge(Sailing.Origin.JA_RL_NKPortOfLoading, Sailing.Destination.JB_RL_NKPortOfDischarge);

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_JX = Sailing.PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment.JS_RL_NKOrigin = Sailing.Origin.JA_RL_NKPortOfLoading;
			shipment.JS_RL_NKDestination = Sailing.Destination.JB_RL_NKPortOfDischarge;
			shipment.JS_UniqueConsignRef = "snth";
			shipment.OuterPackLines.AddNew();

			var secondShipment = Factory.New<CommonShipment>();
			secondShipment.JS_IsBooking = true;
			secondShipment.JS_IsForwardRegistered = false;
			secondShipment.JS_JX = secondSailing.PK;
			secondShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			secondShipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			secondShipment.JS_RL_NKOrigin = secondSailing.Origin.JA_RL_NKPortOfLoading;
			secondShipment.JS_RL_NKDestination = secondSailing.Destination.JB_RL_NKPortOfDischarge;
			secondShipment.JS_UniqueConsignRef = "aoeu";
			secondShipment.OuterPackLines.AddNew();

			Factory.Save();

			DocSailing wrapper = DocSailing.New(Sailing, Sailing.Factory);
			DocLoadListPackLineCollection collection = wrapper.PackLines;

			AssertEquals("there is only one shipment attached to the sailing", 1, collection.Count);
			AssertEquals("should be shipment", shipment.JS_UniqueConsignRef, collection[0].Shipment.ShipmentNumber);
		}

		public void TestVoyage()
		{
			Voyage.JV_VoyageFlight = "TEST VOY";
			AssertEquals("TEST VOY", SailingWrapper.Voyage.VoyageFlight);
		}

		public void TestVoyageFlight()
		{
			AssertEquals("", SailingWrapper.VoyageFlight);

			Voyage.JV_VoyageFlight = "VOY 333";
			AssertEquals("VOY 333", SailingWrapper.VoyageFlight);
		}

		public void TestETD()
		{
			AssertEquals(ZDateTime.Empty, SailingWrapper.ETD);

			Origin.JA_E_DEP = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, SailingWrapper.ETD);
		}

		public void TestPortOfLoading()
		{
			Origin.JA_RL_NKPortOfLoading = "AUCNS";
			AssertEquals("AUCNS", SailingWrapper.PortOfLoading.Code);

			Origin.JA_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("AUSYD", SailingWrapper.PortOfLoading.Code);
			AssertEquals("AUSYD", SailingWrapper.PortOfLoading.ToString());
		}

		public void TestETA()
		{
			AssertEquals(ZDateTime.Empty, SailingWrapper.ETA);

			Destination.JB_E_ARV = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, SailingWrapper.ETA);
		}

		public void TestPortOfDischarge()
		{
			Destination.JB_RL_NKPortOfDischarge = "NLAMS";
			AssertEquals("NLAMS", SailingWrapper.PortOfDischarge.Code);

			Destination.JB_RL_NKPortOfDischarge = "USCHI";
			AssertEquals("USCHI", SailingWrapper.PortOfDischarge.Code);
			AssertEquals("USCHI", SailingWrapper.PortOfDischarge.ToString());
		}

		public void TestVessel()
		{
			AssertNull(SailingWrapper.Vessel);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "RV TEST";

			Voyage.JV_RV_NKVessel = vessel.RV_FK;
			AssertEquals("RV TEST", SailingWrapper.Vessel.Code);
		}

		public void TestLCLReceivalCommences()
		{
			AssertEquals(ZDateTime.Empty, SailingWrapper.LCLReceivalCommences);
			Sailing.JX_DepotReceivalCommences = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, SailingWrapper.LCLReceivalCommences);
		}

		public void TestLCLCutOff()
		{
			AssertEquals(ZDateTime.Empty, SailingWrapper.LCLCutOff);
			Sailing.JX_DepotCutOff = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, SailingWrapper.LCLCutOff);
		}

		public void TestFCLReceivalCommences()
		{
			AssertEquals(ZDateTime.Empty, SailingWrapper.FCLReceivalCommences);
			Sailing.Origin.JA_ReceivalCommences = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, SailingWrapper.FCLReceivalCommences);
		}

		public void TestFCLCutOff()
		{
			AssertEquals(ZDateTime.Empty, SailingWrapper.FCLCutOff);
			Sailing.Origin.JA_CutOff = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, SailingWrapper.FCLCutOff);
		}

		public void TestLCLAvailabilityDate()
		{
			AssertEquals(ZDateTime.Empty, SailingWrapper.LCLAvailabilityDate);
			Sailing.JX_DepotAvailabilityDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, SailingWrapper.LCLAvailabilityDate);
		}

		public void TestLCLStorageDate()
		{
			AssertEquals(ZDateTime.Empty, SailingWrapper.LCLStorageDate);
			Sailing.JX_DepotStorageDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, SailingWrapper.LCLStorageDate);
		}

		public void TestDocsCutOff()
		{
			AssertEquals(ZDateTime.Empty, SailingWrapper.DocsCutOff);
			Sailing.Origin.JA_DocumentaryCutoff = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, SailingWrapper.DocsCutOff);
		}

		public void TestAvailabilityDate()
		{
			AssertEquals(ZDateTime.Empty, SailingWrapper.AvailabilityDate);
			Sailing.Destination.JB_AvailabilityDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, SailingWrapper.AvailabilityDate);
		}

		public void TestStorageDate()
		{
			AssertEquals(ZDateTime.Empty, SailingWrapper.StorageDate);
			Sailing.Destination.JB_StorageDate = ZDateTime.Today;
			AssertEquals(ZDateTime.Today, SailingWrapper.StorageDate);
		}

		public void TestVoyageOrigin()
		{
			Origin.JA_RL_NKPortOfLoading = "AUBNE";
			AssertEquals("AUBNE", SailingWrapper.VoyageOrigin.NKPortOfLoading.Code);
		}

		public void TestVoyageDestination()
		{
			Destination.JB_RL_NKPortOfDischarge = "USLAX";
			AssertEquals("USLAX", SailingWrapper.VoyageDestination.NKPortOfDischarge.Code);
		}

		public void TestReservedMasterBill()
		{
			AssertEquals("", SailingWrapper.ReservedMasterBill);
			Sailing.JX_ReservedMasterBill = "RSVD MASTERBILL TEST";
			AssertEquals("RSVD MASTERBILL TEST", SailingWrapper.ReservedMasterBill);
		}

		public void TestArrivalReference()
		{
			AssertEquals("", SailingWrapper.ArrivalReference);

			Destination.JB_ArrivalReference = "ARV REF";
			AssertEquals("ARV REF", SailingWrapper.ArrivalReference);
		}

		public void TestDepartureReference()
		{
			AssertEquals("", SailingWrapper.DepartureReference);

			Origin.JA_DepartReference = "DEP REF";
			AssertEquals("DEP REF", SailingWrapper.DepartureReference);
		}

		public void TestPackLines()
		{
			var shipment1 = Sailing.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00009999";
			var pack1 = shipment1.OuterPackLines.AddNew();
			var pack2 = shipment1.OuterPackLines.AddNew();

			var shipment2 = Sailing.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S0000ZZZZ";
			var pack3 = shipment2.OuterPackLines.AddNew();

			var container1 = Sailing.Containers.AddNew();
			container1.JC_ContainerNum = "CONTNUMBER1";
			var container2 = Sailing.Containers.AddNew();
			container2.JC_ContainerNum = "CONTNUMBER2";

			container1.PackLines.Add(pack1);
			container1.PackLines.Add(pack3);
			container2.PackLines.Add(pack2);
			Factory.Save();
			AssertEquals(3, SailingWrapper.PackLines.Count);
		}

		public void TestCombinedPackLines()
		{
			var shipment1 = Sailing.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00009999";
			var pack1 = shipment1.OuterPackLines.AddNew();
			var pack2 = shipment1.OuterPackLines.AddNew();

			var shipment2 = Sailing.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S0000ZZZZ";
			var pack3 = shipment2.OuterPackLines.AddNew();

			pack1.JL_PackageCount = 5;
			pack2.JL_PackageCount = 2;
			pack3.JL_PackageCount = 4;

			var container1 = Sailing.Containers.AddNew();
			container1.JC_ContainerNum = "CONTNUMBER1";
			var container2 = Sailing.Containers.AddNew();
			container2.JC_ContainerNum = "CONTNUMBER2";

			container1.PackLines.Add(pack1);
			container1.PackLines.Add(pack2);
			container2.PackLines.Add(pack3);

			DocLoadListPackLineCollection result = SailingWrapper.PackLines;
			AssertEquals(2, result.Count);
			AssertEquals(7, result[0].PackageCount);
			AssertEquals(4, result[1].PackageCount);
		}

		public void TestUnAllocatedPackLines()
		{
			var shipment1 = Sailing.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00009999";
			var pack1 = shipment1.OuterPackLines.AddNew();
			var pack2 = shipment1.OuterPackLines.AddNew();

			var shipment2 = Sailing.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S0000ZZZZ";
			var pack3 = shipment2.OuterPackLines.AddNew();

			var shipment3 = Sailing.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "S00028472";
			var pack4 = shipment3.OuterPackLines.AddNew();

			var container1 = Sailing.Containers.AddNew();
			container1.JC_ContainerNum = "CONTNUMBER1";
			var container2 = Sailing.Containers.AddNew();
			container2.JC_ContainerNum = "CONTNUMBER2";

			container1.PackLines.Add(pack1);
			container1.PackLines.Add(pack3);
			container2.PackLines.Add(pack2);
			Factory.Save();
			AssertEquals(4, SailingWrapper.PackLines.Count);
		}

		public void TestContext()
		{
			AssertEquals("SAILING", SailingWrapper.Context);
		}

		public void TestShippingLine()
		{
			var line = Factory.New<OrgHeader>();
			line.OH_FullName = "VERY BIG SHIPPING LINE";
			Voyage.JV_OH_Line = line.PK;
			AssertEquals("VERY BIG SHIPPING LINE", SailingWrapper.ShippingLine.Name);
		}
		public void TestDepartureCTO()
		{
			AssertNull(SailingWrapper.DepartureCTO);

			var testOrg = Factory.New<OrgHeader>();
			var address1 = testOrg.Addresses.AddNew();
			address1.OA_Address1 = "TEST ADDRESS";
			Origin.JA_OA_DepartureCTOAddress = address1.PK;
			AssertEquals("TEST ADDRESS", SailingWrapper.DepartureCTO.Address1);
		}
		public void TestArrivalCTO()
		{
			AssertNull(SailingWrapper.ArrivalCTO);

			var testOrg = Factory.New<OrgHeader>();
			var address1 = testOrg.Addresses.AddNew();
			address1.OA_Address1 = "TEST ARRIVAL CTO ADDRESS";
			Destination.JB_OA_ArrivalCTOAddress = address1.PK;
			AssertEquals("TEST ARRIVAL CTO ADDRESS", SailingWrapper.ArrivalCTO.Address1);
		}

		protected override void SetUp()
		{
			Voyage = Factory.New<JobVoyage>();

			Origin = Voyage.Origins.AddNew();
			Origin.JA_RL_NKPortOfLoading = "AUBNE";

			Destination = Voyage.Destinations.AddNew();
			Destination.JB_RL_NKPortOfDischarge = "SGSIN";

			Voyage.GenerateSailings();
			Sailing = Voyage.Sailings[0];
			Sailing.JX_IsPublished = true;

			SailingWrapper = DocSailing.New(Sailing, Factory);
			AssertNotNull("Sailing wrapper should not be null", SailingWrapper);
			base.SetUp();
		}

		DocSailing SailingWrapper;
		JobSailing Sailing;
		JobVoyage Voyage;
		VoyageOrigin Origin;
		VoyageDestination Destination;
	}
}

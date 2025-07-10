using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(RouteEntryCollectionSynchroniser))]
	sealed class RouteEntryCollectionSynchroniserTest : TestCaseWithFactory
	{
		public void TestSynchronise()
		{
			manifestHeader.Synchroniser.Synchronise(true);

			CombineAssertions(() =>
			{
				AssertEquals("Should create 4 itinerary data.", 4, manifestHeader.Itinerary.Count);

				var itinerary1 = manifestHeader.Itinerary.Single<RouteEntry>(x => x.CY_Order == 1);
				Assert("ReadOnly", itinerary1.ReadOnly);
				AssertEquals("Load Port added from JK_RL_NKLoadPort of first transport with discharge in EU where JW_LegOrder == 1", "USPLA", itinerary1.CY_Code);

				var itinerary2 = manifestHeader.Itinerary.Single<RouteEntry>(x => x.CY_Order == 2);
				Assert("ReadOnly", itinerary2.ReadOnly);
				AssertEquals("Port", "DEHAM", itinerary2.CY_Code);

				var itinerary3 = manifestHeader.Itinerary.Single<RouteEntry>(x => x.CY_Order == 3);
				Assert("ReadOnly", itinerary3.ReadOnly);
				AssertEquals("Port", "DEFRA", itinerary3.CY_Code);

				var itinerary4 = manifestHeader.Itinerary.Single<RouteEntry>(x => x.CY_Order == 4);
				Assert("ReadOnly", itinerary4.ReadOnly);
				AssertEquals("Port", "USMSY", itinerary4.CY_Code);
			});
		}

		public void TestSynchronise_WhenLoadPortChanges()
		{
			manifestHeader.Synchroniser.Synchronise(true);

			route1.JW_RL_NKLoadPort = "USLAX";
			var itinerary1 = manifestHeader.Itinerary.Single<RouteEntry>(x => x.CY_Order == 1);
			AssertEquals("USLAX", itinerary1.CY_Code);
		}

		public void TestSynchronise_NoLoadPortWhenNoEuDischargePort()
		{
			manifestHeader.Synchroniser.Synchronise(true);

			route2.JW_RL_NKDiscPort = "USPLA";

			CombineAssertions(() =>
			{
				AssertEquals("Should create 3 itinerary data.", 3, manifestHeader.Itinerary.Count);

				var itinerary2 = manifestHeader.Itinerary.Single<RouteEntry>(x => x.CY_Order == 1);
				Assert("ReadOnly", itinerary2.ReadOnly);
				AssertEquals("Port", "DEHAM", itinerary2.CY_Code);

				var itinerary3 = manifestHeader.Itinerary.Single<RouteEntry>(x => x.CY_Order == 2);
				Assert("ReadOnly", itinerary3.ReadOnly);
				AssertEquals("Port", "USPLA", itinerary3.CY_Code);

				var itinerary4 = manifestHeader.Itinerary.Single<RouteEntry>(x => x.CY_Order == 3);
				Assert("ReadOnly", itinerary4.ReadOnly);
				AssertEquals("Port", "USMSY", itinerary4.CY_Code);
			});
		}

		public void TestSynchronise_NoLoadPortWhenNotSea()
		{
			manifestHeader.Synchroniser.Synchronise(true);
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			CombineAssertions(() =>
			{
				AssertEquals("Should create 3 itinerary data.", 3, manifestHeader.Itinerary.Count);

				AssertNull(manifestHeader.Itinerary.FirstOrDefault<RouteEntry>(x => x.CY_Code == "USLAX"));
			});
		}

		public void TestSynchronise_NoDuplicateLoadPortForEuDischargeCreatedWhenLoadPortAlreadyIncluded()
		{
			manifestHeader.Synchroniser.Synchronise(true);
			route1.JW_RL_NKDiscPort = "USLAX";

			CombineAssertions(() =>
			{
				AssertEquals("Should create 3 itinerary data.", 3, manifestHeader.Itinerary.Count);

				AssertEquals(1, manifestHeader.Itinerary.Count<RouteEntry>(x => x.CY_Code == "USLAX"));
			});
		}

		public void TestSynchronise_NoLoadPortAddedWhenFirstEuEntryHasLegOrderEqual2()
		{
			route1.JW_TransportMode = Core.Constants.TransportModes.Road;
			manifestHeader.Synchroniser.Synchronise(true);

			CombineAssertions(() =>
			{
				AssertEquals("Should create 3 itinerary data.", 3, manifestHeader.Itinerary.Count);

				var itinerary1 = manifestHeader.Itinerary.Single<RouteEntry>(x => x.CY_Order == 1);
				Assert("ReadOnly", itinerary1.ReadOnly);
				AssertEquals("Port", "DEHAM", itinerary1.CY_Code);

				var itinerary2 = manifestHeader.Itinerary.Single<RouteEntry>(x => x.CY_Order == 2);
				Assert("ReadOnly", itinerary2.ReadOnly);
				AssertEquals("Port", "DEFRA", itinerary2.CY_Code);

				var itinerary3 = manifestHeader.Itinerary.Single<RouteEntry>(x => x.CY_Order == 3);
				Assert("ReadOnly", itinerary3.ReadOnly);
				AssertEquals("Port", "USMSY", itinerary3.CY_Code);
			});
		}

		public void TestSynchronise_ShouldAddBillsOrigins_WhenTheyDifferFromOtherItineraries()
		{
			var helper = new MasterFilesTestHelper(Factory);
			var austria = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Austria);
			helper.CreateUnlocoIfNotExists("ATVIE", austria);
			helper.CreateUnlocoIfNotExists("ATINB", austria);

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "ATVIE";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_RL_NKOrigin = "USPLA"; // this Origin is the duplicate to added from transport and should not be added

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_RL_NKOrigin = "ATINB";

			var shipment4 = consol.Shipments.AddNew();
			shipment4.JS_RL_NKDestination = "ATABC"; // invalid code should not be added

			manifestHeader.Synchroniser.Synchronise(true);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition", 4, manifestHeader.Bills.Count);

				AssertEquals("Should create 4 itineraries +2 from bills.", 6, manifestHeader.Itinerary.Count);

				var itinerary3 = manifestHeader.Itinerary.Single<RouteEntry>(x => x.CY_Order == 3);
				AssertEquals("Precondition: first Itinerary not from bills", "USPLA", itinerary3.CY_Code);

				var itinerary1 = manifestHeader.Itinerary.Single<RouteEntry>(x => x.CY_Order == 1);
				Assert("ReadOnly", itinerary1.ReadOnly);
				AssertEquals("Load Port added from Origin of bill that is different from other Itineraries", "ATVIE", itinerary1.CY_Code);

				var itinerary2 = manifestHeader.Itinerary.Single<RouteEntry>(x => x.CY_Order == 2);
				Assert("ReadOnly", itinerary2.ReadOnly);
				AssertEquals("Load Port added from Origin of bill that is different from other Itineraries", "ATINB", itinerary2.CY_Code);
			});
		}

		public void TestSynchronise_ShouldAddBillsFinalDestinations_WhenTheyDifferFromOtherItineraries()
		{
			var helper = new MasterFilesTestHelper(Factory);
			var austria = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Austria);
			helper.CreateUnlocoIfNotExists("ATSLZ", austria);
			helper.CreateUnlocoIfNotExists("ATGRZ", austria);

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKDestination = "ATGRZ";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_RL_NKDestination = "USMSY"; // this Destination is the same as last Itinerary and should not be added
			
			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_RL_NKDestination = "ATSLZ";

			var shipment4 = consol.Shipments.AddNew();
			shipment4.JS_RL_NKDestination = "ATABC"; // invalid code should not be added

			manifestHeader.Synchroniser.Synchronise(true);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition", 4, manifestHeader.Bills.Count);

				AssertEquals("Should create 4 itineraries +2 from bills.", 6, manifestHeader.Itinerary.Count);

				var itinerary4 = manifestHeader.Itinerary.Single<RouteEntry>(x => x.CY_Order == 4);
				AssertEquals("Precondition: last Itinerary not from bills", "USMSY", itinerary4.CY_Code);

				var itinerary5 = manifestHeader.Itinerary.Single<RouteEntry>(x => x.CY_Order == 5);
				Assert("ReadOnly", itinerary5.ReadOnly);
				AssertEquals("Load Port added from Final Destination of bill that is different from other Itineraries", "ATGRZ", itinerary5.CY_Code);

				var itinerary6 = manifestHeader.Itinerary.Single<RouteEntry>(x => x.CY_Order == 6);
				Assert("ReadOnly", itinerary6.ReadOnly);
				AssertEquals("Load Port added from Final Destination of bill that is different from other Itineraries", "ATSLZ", itinerary6.CY_Code);
			});
		}

		public void TestSynchronise_ShouldAddFromBillsTriggered_WhenNumberOfBillsChange()
		{
			var helper = new MasterFilesTestHelper(Factory);
			var austria = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Austria);
			helper.CreateUnlocoIfNotExists("ATVIE", austria);
			helper.CreateUnlocoIfNotExists("ATINB", austria);
			helper.CreateUnlocoIfNotExists("ATSLZ", austria);
			helper.CreateUnlocoIfNotExists("ATGRZ", austria);

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_RL_NKOrigin = "ATVIE";
			shipment1.JS_RL_NKDestination = "ATGRZ";

			manifestHeader.Synchroniser.Synchronise(true);
			AssertEquals("Precondition: 4 itineraries +2 from bills.", 6, manifestHeader.Itinerary.Count);

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_RL_NKOrigin = "ATINB";
			shipment2.JS_RL_NKDestination = "ATSLZ";
			AssertEquals("Precondition: 4 itineraries +4 from bills.", 8, manifestHeader.Itinerary.Count);
		}

		public void TestSynchronise_ShouldNotCauseChanges_WhenSynchronizedSecondTime()
		{
			manifestHeader.Synchroniser.Synchronise(true);
			Factory.Save();
			var newFactory = NewFactory();
			var newManifestHeader = newFactory.Load<AsycudaManifestHeader>(manifestHeader.PK);

			newManifestHeader.Synchroniser.Synchronise(true);

			AssertEquals(false, newManifestHeader.Itinerary.HasChanges);
		}

		protected override void SetUp()
		{
			base.SetUp();

			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var helper = new MasterFilesTestHelper(Factory);
			var usa = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedStates);
			helper.CreateUnlocoIfNotExists("USPLA", usa);
			helper.CreateUnlocoIfNotExists("USNYC", usa);
			helper.CreateUnlocoIfNotExists("USLAX", usa);
			helper.CreateUnlocoIfNotExists("USMSY", usa);
			var germany = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Germany);
			helper.CreateUnlocoIfNotExists("DEHAM", germany);
			helper.CreateUnlocoIfNotExists("DEFRA", germany);
			consol.Transports.RemoveAndDeleteAll();
			route1 = consol.Transports[0];

			route1.JW_RL_NKLoadPort = "USPLA";
			route1.JW_RL_NKDiscPort = "DEHAM";

			route2 = consol.Transports.AddNew();

			route2.JW_RL_NKLoadPort = "DEHAM";
			route2.JW_RL_NKDiscPort = "DEFRA";

			route3 = consol.Transports.AddNew();

			route3.JW_RL_NKLoadPort = "DEFRA";
			route3.JW_RL_NKDiscPort = "USMSY";

			manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
		}

		protected override void TearDown()
		{
			base.TearDown();

			foreach (ForwardingShipment consolShipment in consol.Shipments)
			{
				consolShipment.ShipmentJobHeader?.Dispose();
			}
		}

		ForwardingConsol consol;
		AsycudaManifestHeader manifestHeader;
		Transport route1;
		Transport route2;
		Transport route3;
	}
}

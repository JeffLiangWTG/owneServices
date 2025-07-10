using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AsycudaManifestHeaderSynchroniser))]
	sealed class AsycudaManifestHeaderSynchroniserTest : TestCaseWithFactory
	{
		public void TestPortOfLoadingSynchronised_Sea()
		{
			manifest.Synchroniser.Synchronise(true);
			AssertEquals("USPLA", manifest.AMA_RL_NKPortOfLoading);
		}

		public void TestPortOfLoadingSynchronised_NotSea()
		{
			consol.JK_RL_NKLoadPort = "USNYC";
			consol.JK_TransportMode = Constants.TransportModes.Road;
			manifest.Synchroniser.Synchronise(true);
			AssertEquals("USNYC", manifest.AMA_RL_NKPortOfLoading);
		}

		public void TestPortOfLoadingSynchronised_WhenConsolTransportModeChangedNotSea()
		{
			consol.JK_RL_NKLoadPort = "USNYC";
			manifest.Synchroniser.Synchronise(true);

			consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals("USNYC", manifest.AMA_RL_NKPortOfLoading);
		} 

		public void TestPortOfLoadingSynchronised_WhenTransportModeChanged()
		{
			route2.JW_RL_NKLoadPort = "USNYC";
			manifest.Synchroniser.Synchronise(true);

			route1.JW_TransportMode = Constants.TransportModes.Road;
			AssertEquals("USNYC", manifest.AMA_RL_NKPortOfLoading);
		}

		public void TestPortOfLoadingSynchronised_WhenLoadPortUpdated()
		{
			manifest.Synchroniser.Synchronise(true);
			route1.JW_RL_NKLoadPort = "USNYC";
			AssertEquals("USNYC", manifest.AMA_RL_NKPortOfLoading);
		}

		public void TestPortOfLoadingSynchronised_WhenFirstTransportChanged()
		{
			manifest.Synchroniser.Synchronise(true);
			route1.JW_RL_NKDiscPort = "USLAX";
			route1.JW_Vessel = "3333";
			route2.JW_RL_NKLoadPort = "USNYC";

			AssertEquals("USNYC", manifest.AMA_RL_NKPortOfLoading);
		}

		public void TestPortOfLoadingSynchronised_WhenFirstTransportChangedNorway()
		{
			manifest.Synchroniser.Synchronise(true);
			route1.JW_RL_NKDiscPort = "NOAAF";
			route2.JW_RL_NKLoadPort = "NOAAF";

			AssertEquals("USPLA", manifest.AMA_RL_NKPortOfLoading);
		}

		public void TestPortOfLoadingSynchronised_WhenFirstTransportChangedSwitzerland()
		{
			manifest.Synchroniser.Synchronise(true);
			route1.JW_RL_NKDiscPort = "USNYC";
			route2.JW_RL_NKLoadPort = "USNYC";
			route2.JW_RL_NKDiscPort = "CHABC";

			AssertEquals("USPLA", manifest.AMA_RL_NKPortOfLoading);
		}

		public void TestPortOfLoadingSynchronised_WhenTransportAdded()
		{
			route1.JW_LegOrder = 2;
			route1.JW_LegOrder = 3;

			manifest.Synchroniser.Synchronise(true);

			var route3 = consol.Transports.AddNew();
			route3.JW_RL_NKLoadPort = "AUVIC";
			route3.JW_RL_NKDiscPort = "DEHAM";
			route3.JW_Vessel = "2222";
			route3.JW_LegOrder = 1;

			AssertEquals("AUVIC", manifest.AMA_RL_NKPortOfLoading);
		}

		public void TestFirstPortOfArrivalSynchronised_Sea()
		{
			manifest.Synchroniser.Synchronise(true);
			AssertEquals("DEHAM", manifest.AMA_RL_NKPortOfFirstArrival);
		}

		public void TestFirstPortOfArrivalSynchronised_Norway()
		{
			manifest.Synchroniser.Synchronise(true);
			route1.JW_RL_NKDiscPort = "NOAAF";
			AssertEquals("NOAAF", manifest.AMA_RL_NKPortOfFirstArrival);
		}

		public void TestFirstPortOfArrivalSynchronised_WhenDiscPortChanged()
		{
			manifest.Synchroniser.Synchronise(true);
			route1.JW_RL_NKDiscPort = "DEWIB";
			AssertEquals("DEWIB", manifest.AMA_RL_NKPortOfFirstArrival);
		}

		public void TestFirstPortOfArrivalSynchronised_WhenFirstTransportChanged()
		{
			manifest.Synchroniser.Synchronise(true);
			route1.JW_RL_NKDiscPort = "USLAX";
			AssertEquals("DEBER", manifest.AMA_RL_NKPortOfFirstArrival);
		}

		public void TestFirstPortOfArrivalSynchronised_WhenTransportAdded()
		{
			route1.JW_LegOrder = 2;
			route1.JW_LegOrder = 3;

			manifest.Synchroniser.Synchronise(true);

			var route3 = consol.Transports.AddNew();
			route3.JW_RL_NKLoadPort = "USLAX";
			route3.JW_RL_NKDiscPort = "DEHAM";
			route3.JW_LegOrder = 1;

			AssertEquals("DEHAM", manifest.AMA_RL_NKPortOfFirstArrival);
		}

		public void TestFirstPortOfArrivalSynchronised_WhenConsolTransportModeChangedNotSea()
		{
			consol.JK_RL_NKPortOfFirstArrival = "DEWIB";
			manifest.Synchroniser.Synchronise(true);

			consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals("DEWIB", manifest.AMA_RL_NKPortOfFirstArrival);
		}

		public void TestFirstPortOfArrivalSynchronised_WhenTransportModeChangedNotSea()
		{
			manifest.Synchroniser.Synchronise(true);

			route1.JW_TransportMode = Constants.TransportModes.Road;
			AssertEquals("DEBER", manifest.AMA_RL_NKPortOfFirstArrival);
		}

		public void TestDischargePortSynchronised_Sea()
		{
			manifest.Synchroniser.Synchronise(true);

			AssertEquals("DEBER", manifest.AMA_RL_NKPortOfDischarge);
		}

		public void TestDischargePortSynchronised_WhenDiscPortChanged()
		{
			manifest.Synchroniser.Synchronise(true);

			route2.JW_RL_NKDiscPort = "DEWIB";
			AssertEquals("DEWIB", manifest.AMA_RL_NKPortOfDischarge);
		}

		public void TestDischargePortSynchronised_WhenLastTransportChanged()
		{
			manifest.Synchroniser.Synchronise(true);

			var route3 = consol.Transports.AddNew();
			route3.JW_RL_NKLoadPort = "DEBER";
			route3.JW_RL_NKDiscPort = "DEWIB";

			AssertEquals("DEWIB", manifest.AMA_RL_NKPortOfDischarge);
		}

		public void TestDischargePortSynchronised_WhenConsolTransportModeChangedNotSea()
		{
			manifest.Synchroniser.Synchronise(true);
			route2.JW_TransportMode = Constants.TransportModes.Road;
			consol.JK_TransportMode = Constants.TransportModes.Road;
			AssertEquals("DEBER", manifest.AMA_RL_NKPortOfDischarge);
		}

		public void TestDischargePortSynchronised_WhenTransportModeChangedNotSea()
		{
			manifest.Synchroniser.Synchronise(true);
			route2.JW_TransportMode = Constants.TransportModes.Road;
			AssertEquals("DEHAM", manifest.AMA_RL_NKPortOfDischarge);
		}

		public void TestSyncABL_PrepaidCollect()
		{
			var forwardingConsol = Factory.New<ForwardingConsol>();
			CreateShipment(OrgConstants.CreditAgreedPaymentMethods.Code.BusinessCheck);
			CreateShipment(OrgConstants.CreditAgreedPaymentMethods.Code.CreditCard);
			CreateShipment(OrgConstants.CreditAgreedPaymentMethods.Code.BankTransfer);
			CreateShipment(OrgConstants.CreditAgreedPaymentMethods.Code.CashAndBankCheck);
			CreateShipment(OrgConstants.CreditAgreedPaymentMethods.Code.DebitCard);
			CreateShipment(OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest);
			CreateShipment(OrgConstants.CreditAgreedPaymentMethods.Code.EPayment);
			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(forwardingConsol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					EUICS2PaymentMethodList.Codes.C, EUICS2PaymentMethodList.Codes.B, EUICS2PaymentMethodList.Codes.H, EUICS2PaymentMethodList.Codes.A,
					EUICS2PaymentMethodList.Codes.D, EUICS2PaymentMethodList.Codes.D, EUICS2PaymentMethodList.Codes.H,
				}, manifestHeader.Bills.Select(b => b.ABL_PrepaidCollect));

			void CreateShipment(ZString creditAgreedPaymentMethod)
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.CompanyData.OB_ARCreditAgreedPaymentMethod = creditAgreedPaymentMethod;
				var shipment = forwardingConsol.Shipments.AddNew();
				shipment.CreateShipmentJobHeaderWithMutex();
				shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr_ZAddress.OrgPK = orgHeader.PK;
			}
		}

		public void TestSyncVesselName()
		{
			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			sourceConsol.Transports.RemoveAndDeleteAll();

			var leg1 = sourceConsol.Transports.AddNew("VUVLI", "LKCMB");
			leg1.JW_Vessel = "VESSEL1";
			var leg2 = sourceConsol.Transports.AddNew("USLGB", "DEBRE");
			leg2.JW_Vessel = "VESSEL2";

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(sourceConsol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("When Transport Mode:Sea, Should sync the Vessel with the first EU Discharge Port line.", leg2.JW_Vessel, manifestHeader.AMA_VesselName);

			sourceConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			leg1.JW_TransportMode = Core.Constants.TransportModes.Air;
			leg2.JW_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals("When Transport Mode not Sea, Should sync the Vessel with the first Transport Line. Transport Mode:Air", leg1.JW_Vessel, manifestHeader.AMA_VesselName);
		}

		public void TestSyncVoyage()
		{
			var sourceConsol = Factory.New<ForwardingConsol>();
			sourceConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			sourceConsol.Transports.RemoveAndDeleteAll();

			var leg1 = sourceConsol.Transports.AddNew("VUVLI", "LKCMB");
			leg1.JW_VoyageFlight = "VOYAGE1";
			var leg2 = sourceConsol.Transports.AddNew("USLGB", "DEBRE");
			leg2.JW_VoyageFlight = "VOYAGE2";

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(sourceConsol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("When Transport Mode:Sea, Should sync the Voyage with the first EU Discharge Port line.", leg2.JW_VoyageFlight, manifestHeader.AMA_Voyage);

			sourceConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			leg1.JW_TransportMode = Core.Constants.TransportModes.Air;
			leg2.JW_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals("When Transport Mode not Sea, Should sync the Voyage with the first Transport Line. Transport Mode:Air", leg1.JW_VoyageFlight, manifestHeader.AMA_VesselName);
		}

		protected override void SetUp()
		{
			base.SetUp();

			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			var helper = new MasterFilesTestHelper(Factory);
			var usa = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedStates);
			helper.CreateUnlocoIfNotExists("USPLA", usa);
			helper.CreateUnlocoIfNotExists("USNYC", usa);
			var germany = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Germany);
			helper.CreateUnlocoIfNotExists("DEHAM", germany);
			helper.CreateUnlocoIfNotExists("DEBER", germany);
			helper.CreateUnlocoIfNotExists("DEWIB", germany);
			var norway = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Norway);
			helper.CreateUnlocoIfNotExists("NOAAF", norway);
			var switzerland = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Switzerland);
			helper.CreateUnlocoIfNotExists("CHABC", switzerland);
			consol.Transports.RemoveAndDeleteAll();
			route1 = consol.Transports.AddNew();

			route1.JW_RL_NKLoadPort = "USPLA";
			route1.JW_RL_NKDiscPort = "DEHAM";
			route1.JW_Vessel = "2222";

			route2 = consol.Transports.AddNew();

			route2.JW_RL_NKLoadPort = "DEHAM";
			route2.JW_RL_NKDiscPort = "DEBER";
			route2.JW_Vessel = "2222";

			manifest = Factory.New<AsycudaManifestHeader>();
			manifest.SetParent(consol);
		}

		ForwardingConsol consol;
		AsycudaManifestHeader manifest;
		Transport route1;
		Transport route2;
	}
}

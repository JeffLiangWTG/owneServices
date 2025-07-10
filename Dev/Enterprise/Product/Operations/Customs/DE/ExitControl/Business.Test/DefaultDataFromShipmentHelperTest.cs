using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	sealed class DefaultDataFromShipmentHelperTest : TestCaseWithFactory
	{
		public void TestGetConsol()
		{
			CombineAssertions(() =>
			{
				CreateUNLOCOsForTest();

				var shipment = Factory.New<ForwardingShipment>();
				var consol = CreateConsolWithShipment(shipment, "DE123");

				consol.Transports.AddNew("DE123", "DE456");
				AssertSame("Consol has Transport from EUN to EUN", consol, DefaultDataFromShipmentHelper.GetConsol(shipment));

				consol.Transports.RemoveAndDeleteAll();
				consol.Transports.AddNew("US123", "US456");
				AssertNull("Consol has not Transports from EUN", DefaultDataFromShipmentHelper.GetConsol(shipment));

				consol.Transports.RemoveAndDeleteAll();
				consol.Transports.AddNew("DE123", "US123");
				AssertSame("Consol Transport goes from EUN to outside EUN", consol, DefaultDataFromShipmentHelper.GetConsol(shipment));
			});
		}

		public void TestGetConsol_Multiple()
		{
			CombineAssertions(() =>
			{
				CreateUNLOCOsForTest();

				var shipment = Factory.New<ForwardingShipment>();
				var consol1 = CreateConsolWithShipment(shipment, "DE123");
				var consol2 = CreateConsolWithShipment(shipment, "US456");

				consol2.Transports.AddNew("US123", "US456");

				consol1.Transports.AddNew("DE123", "DE456");
				AssertSame("Consol Transport does not go from EUN to outside EUN", consol1, DefaultDataFromShipmentHelper.GetConsol(shipment));

				consol1.Transports.RemoveAndDeleteAll();
				consol1.Transports.AddNew("US123", "US456");
				AssertNull("Consol has not Transports from EUN", DefaultDataFromShipmentHelper.GetConsol(shipment));

				consol1.Transports.RemoveAndDeleteAll();
				consol1.Transports.AddNew("DE123", "US123");
				AssertSame("Consol Transport goes from EUN to outside EUN", consol1, DefaultDataFromShipmentHelper.GetConsol(shipment));

				consol1.Delete();
				AssertNull("Only remaining consol is from Not EUN -> Not EUN, not eligible", DefaultDataFromShipmentHelper.GetConsol(shipment));
			});
		}

		public void TestGetTransport()
		{
			CreateUNLOCOsForTest();

			var shipment = Factory.New<ForwardingShipment>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_MasterBillNum = "02032646283";
			consol.Shipments.Add(shipment);
			consol.Transports.RemoveAndDeleteAll();

			var transport = shipment.TransportsIncludingRelated.AddNew();
			var (shipmentTransport, airlineCountryCode) = DefaultDataFromShipmentHelper.GetShipmentTransport(shipment);

			CombineAssertions(() =>
			{
				AssertNull("Transport ports not set", shipmentTransport);

				transport.JW_RL_NKLoadPort = "DE123";
				transport.JW_RL_NKDiscPort = "DE456";
				(shipmentTransport, _) = DefaultDataFromShipmentHelper.GetShipmentTransport(shipment);
				AssertNull("No transport departing the EU present", shipmentTransport);

				var exitEuTransport = shipment.TransportsIncludingRelated.AddNew();
				exitEuTransport.JW_RL_NKLoadPort = "DE456";
				exitEuTransport.JW_RL_NKDiscPort = "US123";
				exitEuTransport.JW_LegOrder = 1;
				exitEuTransport.JW_VoyageFlight = "XX123";

				(shipmentTransport, _) = DefaultDataFromShipmentHelper.GetShipmentTransport(shipment);
				AssertSame("Transport is from within EUN to outside EUN so should be selected", exitEuTransport, shipmentTransport);
				AssertEquals("Airline2LetterCode is invalid", ZString.Empty, airlineCountryCode);

				exitEuTransport.JW_LegOrder = 2;

				var exitEuTransport2 = shipment.TransportsIncludingRelated.AddNew();
				exitEuTransport2.JW_RL_NKLoadPort = "DE123";
				exitEuTransport2.JW_RL_NKDiscPort = "US123";
				exitEuTransport2.JW_LegOrder = 1;
				exitEuTransport2.JW_VoyageFlight = "LH428";

				(shipmentTransport, airlineCountryCode) = DefaultDataFromShipmentHelper.GetShipmentTransport(shipment);
				AssertSame("Transport with lowest value for JW_LegOrder that is from within EUN to outside EUN so should be selected", exitEuTransport2, shipmentTransport);
				AssertEquals("Airline2LetterCode is valid", Core.Constants.CountryCodes.Germany, airlineCountryCode);
			});
		}

		public void TestGetTransport_AllInEU()
		{
			CreateUNLOCOsForTest();

			var shipment = Factory.New<ForwardingShipment>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_MasterBillNum = "02032646283";
			consol.Shipments.Add(shipment);
			consol.Transports.RemoveAndDeleteAll();

			var transport = shipment.TransportsIncludingRelated[0];
			var (shipmentTransport, _) = DefaultDataFromShipmentHelper.GetShipmentTransport(shipment);

			CombineAssertions(() =>
			{
				AssertNull("Transport ports not set", shipmentTransport);

				transport.JW_RL_NKLoadPort = "DE123";
				transport.JW_RL_NKDiscPort = "DE456";
				(shipmentTransport, _) = DefaultDataFromShipmentHelper.GetShipmentTransport(shipment);
				AssertSame("No transport departing the EU present", transport, shipmentTransport);

				var withinEuTransport = shipment.TransportsIncludingRelated.AddNew();
				withinEuTransport.JW_RL_NKLoadPort = "DE456";
				withinEuTransport.JW_RL_NKDiscPort = "FR111";
				withinEuTransport.JW_LegOrder = 1;

				(shipmentTransport, _) = DefaultDataFromShipmentHelper.GetShipmentTransport(shipment);
				AssertEquals("All Transports are from within EUN to within EUN, transport with Highest JW_LegOrder should be selected", withinEuTransport, shipmentTransport);

				withinEuTransport.JW_LegOrder = 2;

				var withinEuTransport2 = shipment.TransportsIncludingRelated.AddNew();
				withinEuTransport2.JW_RL_NKLoadPort = "DE123";
				withinEuTransport2.JW_RL_NKDiscPort = "FR222";
				withinEuTransport2.JW_LegOrder = 1;

				(shipmentTransport, _) = DefaultDataFromShipmentHelper.GetShipmentTransport(shipment);
				AssertSame("Transport with highest value for JW_LegOrder should be selected as there is no transport exiting the EU", withinEuTransport, shipmentTransport);

				var exitsEuTransport = shipment.TransportsIncludingRelated.AddNew();
				exitsEuTransport.JW_RL_NKLoadPort = "FR222";
				exitsEuTransport.JW_RL_NKDiscPort = "US123";
				exitsEuTransport.JW_LegOrder = 3;

				(shipmentTransport, _) = DefaultDataFromShipmentHelper.GetShipmentTransport(shipment);
				AssertSame("First transport exiting EUN is selected", exitsEuTransport, shipmentTransport);
			});
		}

		public void TestGetAirlineCountryFallback()
		{
			CreateUNLOCOsForTest(createForAirlines: true);

			var delta = RefAirline.LoadFromAirline2LetterCode(Factory, "DL");
			var american = RefAirline.LoadFromAirline2LetterCode(Factory, "AA");

			delta.RM_AirlineCity = "Fort Worth";
			american.RM_AirlineCity = "Atlanta";

			var shipment = Factory.New<ForwardingShipment>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_MasterBillNum = "02032646283";
			consol.Shipments.Add(shipment);
			consol.Transports.RemoveAndDeleteAll();

			var transport = shipment.TransportsIncludingRelated.AddNew();
			var (shipmentTransport, airlineCountryCode) = DefaultDataFromShipmentHelper.GetShipmentTransport(shipment);

			CombineAssertions(() =>
			{
				transport.JW_RL_NKLoadPort = "DE123";
				transport.JW_RL_NKDiscPort = "DE456";

				var exitEuTransport = shipment.TransportsIncludingRelated.AddNew();
				exitEuTransport.JW_RL_NKLoadPort = "DE456";
				exitEuTransport.JW_RL_NKDiscPort = "US123";
				exitEuTransport.JW_LegOrder = 1;
				exitEuTransport.JW_VoyageFlight = "DL123";

				(_, airlineCountryCode) = DefaultDataFromShipmentHelper.GetShipmentTransport(shipment);
				AssertEquals("Airline2LetterCode is valid fallback", Core.Constants.CountryCodes.UnitedStates, airlineCountryCode);

				exitEuTransport.JW_VoyageFlight = "AA123";
				(_, airlineCountryCode) = DefaultDataFromShipmentHelper.GetShipmentTransport(shipment);
				AssertEquals("Airline2LetterCode fallback not present", ZString.Empty, airlineCountryCode);
			});
		}

		void CreateUNLOCOsForTest(bool createForAirlines = false)
		{
			var helper = new MasterFilesTestHelper(Factory);
			var germany = RefCountry.LoadFromCountryCode(Factory, "DE");
			var unitedStates = RefCountry.LoadFromCountryCode(Factory, "US");
			var uk = RefCountry.LoadFromCountryCode(Factory, "GB");
			var france = RefCountry.LoadFromCountryCode(Factory, "FR");
			helper.CreateUnlocoIfNotExists("DE123", germany);
			helper.CreateUnlocoIfNotExists("DE456", germany);
			helper.CreateUnlocoIfNotExists("FR111", france);
			helper.CreateUnlocoIfNotExists("FR222", france);
			var us1 = helper.CreateUnlocoIfNotExists("US123", unitedStates);
			helper.CreateUnlocoIfNotExists("US456", unitedStates);
			if (createForAirlines)
			{
				us1.RL_PortName = "Fort Worth";
				var us2 = helper.CreateUnlocoIfNotExists("US222", unitedStates);
				var us3 = helper.CreateUnlocoIfNotExists("US333", unitedStates);
				var uk1 = helper.CreateUnlocoIfNotExists("GB111", uk);
				us2.RL_PortName = "Fort Worth";
				us3.RL_PortName = "Atlanta";
				uk1.RL_PortName = "Atlanta";
			}
			Factory.Save();
		}

		ForwardingConsol CreateConsolWithShipment(ForwardingShipment shipment, string dischargePort)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Transports.RemoveAndDeleteAll();
			consol.JK_RL_NKDischargePort = dischargePort;
			consol.Shipments.Add(shipment);
			return consol;
		}
	}
}

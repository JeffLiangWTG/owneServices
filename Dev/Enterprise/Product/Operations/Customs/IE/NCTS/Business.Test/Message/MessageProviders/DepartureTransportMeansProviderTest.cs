using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class DepartureTransportMeansProviderTest : TestCaseWithFactory
	{
		public void TestGetTransportMean_EnRouteIncident()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var incident = nctsHeader.EnRouteIncidents.AddNew();
			incident.BN_TransportAtDepartureType = "10";
			incident.BN_TransportAtDepartureID = "1234";
			incident.BN_RN_NKTransportAtDepartureIDNationality = "IE";
			var transportMean = DepartureTransportMeansProvider.GetTransportMean(incident);
			CombineAssertions(() =>
			{
				AssertEquals("TypeOfIdentification", "10", transportMean.TypeOfIdentification);
				AssertEquals("IdentificationNumber", "1234", transportMean.IdentificationNumber);
				AssertEquals("Nationality", "IE", transportMean.Nationality);
			});
		}

		public void TestInlandTransportMode_1()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			var bill = nctsHeader.Bills.AddNew();
			movementHeader.InlandTransportModeAtDeparture = ModeOfTransportList.Codes._3_RoadTransport;
			movementHeader.TransportAtDeparture = "header777";
			movementHeader.TransportCountryAtDeparture = "IE";
			movementHeader.Trailer1IDAtDeparture = "Header1111";
			movementHeader.Trailer1NationalityAtDeparture = "AU";
			movementHeader.Trailer2IDAtDeparture = "Header2222";
			movementHeader.Trailer2NationalityAtDeparture = "CN";
			bill.TransportAtDeparture = "Bill777";
			bill.TransportCountryAtDeparture = "GB";
			bill.Trailer1IDAtDeparture = "BillS1111";
			bill.Trailer1NationalityAtDeparture = "US";
			bill.Trailer2IDAtDeparture = "BillS2222";
			bill.Trailer2NationalityAtDeparture = "BE";
			CombineAssertions(() =>
			{
				var transportMeans = DepartureTransportMeansProvider.GetTransportMeans(movementHeader).ToArray();
				AssertEquals(3, transportMeans.Length);
				AssertEquals("Header Transport Mean Seq.1 IdentificationNumber", "HEADER777", transportMeans[0].IdentificationNumber);
				AssertEquals("Header Transport Mean Seq.2 IdentificationNumber", "HEADER1111", transportMeans[1].IdentificationNumber);
				AssertEquals("Header Transport Mean Seq.3 IdentificationNumber", "HEADER2222", transportMeans[2].IdentificationNumber);

				AssertEquals("Header Transport Mean Seq.1 Nationality", "IE", transportMeans[0].Nationality);
				AssertEquals("Header Transport Mean Seq.2 Nationality", "AU", transportMeans[1].Nationality);
				AssertEquals("Header Transport Mean Seq.3 Nationality", "CN", transportMeans[2].Nationality);

				transportMeans = DepartureTransportMeansProvider.GetTransportMeans(bill).ToArray();
				AssertEquals(3, transportMeans.Length);
				AssertEquals("Header Transport Mean Seq.1 IdentificationNumber", "Bill777", transportMeans[0].IdentificationNumber);
				AssertEquals("Header Transport Mean Seq.2 IdentificationNumber", "BillS1111", transportMeans[1].IdentificationNumber);
				AssertEquals("Header Transport Mean Seq.3 IdentificationNumber", "BillS2222", transportMeans[2].IdentificationNumber);

				AssertEquals("Header Transport Mean Seq.1 Nationality", "GB", transportMeans[0].Nationality);
				AssertEquals("Header Transport Mean Seq.2 Nationality", "US", transportMeans[1].Nationality);
				AssertEquals("Header Transport Mean Seq.3 Nationality", "BE", transportMeans[2].Nationality);
			});
		}

		public void TestTypeOfIdentification_1()
		{
			AssertTypeOfIdentification(inlandTransportMode: ModeOfTransportList.Codes._1_SeaTransport, expectedTypeOfIdentification: NctsTransportTypeOfIdList.Codes._10, transportTypeAtDeparture: NctsTransportTypeOfIdList.Codes._10);
		}

		public void TestTypeOfIdentification_2()
		{
			CombineAssertions(() =>
			{
				AssertTypeOfIdentification(
					inlandTransportMode: ModeOfTransportList.Codes._2_RailTransport,
					expectedTypeOfIdentification: NctsTransportTypeOfIdList.Codes._20,
					trailer1IDAtDeparture: "456");
				AssertTypeOfIdentification(
					inlandTransportMode: ModeOfTransportList.Codes._2_RailTransport,
					expectedTypeOfIdentification: NctsTransportTypeOfIdList.Codes._21,
					transportAtDeparture: "123");
			});
		}

		public void TestTypeOfIdentification_3()
		{
			CombineAssertions(() =>
			{
				AssertTypeOfIdentification(
					inlandTransportMode: ModeOfTransportList.Codes._3_RoadTransport,
					expectedTypeOfIdentification: NctsTransportTypeOfIdList.Codes._30,
					transportAtDeparture: "123");
				AssertTypeOfIdentification(
					inlandTransportMode: ModeOfTransportList.Codes._3_RoadTransport,
					expectedTypeOfIdentification: NctsTransportTypeOfIdList.Codes._31,
					trailer1IDAtDeparture: "456");
				AssertTypeOfIdentification(
					inlandTransportMode: ModeOfTransportList.Codes._3_RoadTransport,
					expectedTypeOfIdentification: NctsTransportTypeOfIdList.Codes._31,
					trailer2IDAtDeparture: "AS222");
			});
		}

		public void TestTypeOfIdentification_4()
		{
			CombineAssertions(() =>
			{
				AssertTypeOfIdentification(
					inlandTransportMode: ModeOfTransportList.Codes._4_AirTransport,
					expectedTypeOfIdentification: NctsTransportTypeOfIdList.Codes._41,
					aircraftIDAtDeparture: "AS222");
				AssertTypeOfIdentification(
					inlandTransportMode: ModeOfTransportList.Codes._4_AirTransport,
					expectedTypeOfIdentification: NctsTransportTypeOfIdList.Codes._41,
					transportTypeAtDeparture: NctsTransportTypeOfIdList.Codes._41,
					transportAtDeparture: "AS222");
				AssertTypeOfIdentification(
					inlandTransportMode: ModeOfTransportList.Codes._4_AirTransport,
					expectedTypeOfIdentification: NctsTransportTypeOfIdList.Codes._40,
					transportTypeAtDeparture: NctsTransportTypeOfIdList.Codes._40,
					transportAtDeparture: "AS222");
			});
		}

		public void TestTypeOfIdentification_8()
		{
			CombineAssertions(() =>
			{
				AssertTypeOfIdentification(
					inlandTransportMode: ModeOfTransportList.Codes._8_InlandWaterwayTransport,
					expectedTypeOfIdentification: NctsTransportTypeOfIdList.Codes._81,
					transportTypeAtDeparture: NctsTransportTypeOfIdList.Codes._81,
					transportAtDeparture: "AS222");
				AssertTypeOfIdentification(
					inlandTransportMode: ModeOfTransportList.Codes._8_InlandWaterwayTransport,
					expectedTypeOfIdentification: NctsTransportTypeOfIdList.Codes._80,
					transportTypeAtDeparture: NctsTransportTypeOfIdList.Codes._80,
					transportAtDeparture: "AS222");
			});
		}

		public void TestTypeOfIdentification_9()
		{
			AssertTypeOfIdentification(inlandTransportMode: ModeOfTransportList.Codes._9_OwnPropulsion, expectedTypeOfIdentification: NctsTransportTypeOfIdList.Codes._20, transportTypeAtDeparture: NctsTransportTypeOfIdList.Codes._20);
		}

		public void TestIdentificationNumber_1()
		{
			AssertIdentificationNumber(inlandTransportMode: ModeOfTransportList.Codes._1_SeaTransport, expectedIdentificationNumber: "AS222", vesselNameAtDeparture: "AS222");
		}

		public void TestIdentificationNumber_2()
		{
			CombineAssertions(() =>
			{
				AssertIdentificationNumber(
						inlandTransportMode: ModeOfTransportList.Codes._2_RailTransport,
						expectedIdentificationNumber: "AS222",
						trailer1IDAtDeparture: "AS222");
				AssertIdentificationNumber(
						inlandTransportMode: ModeOfTransportList.Codes._2_RailTransport,
						expectedIdentificationNumber: "AS222",
						transportAtDeparture: "AS222");
			});
		}

		public void TestIdentificationNumber_3()
		{
			CombineAssertions(() =>
			{
				AssertIdentificationNumber(
						inlandTransportMode: ModeOfTransportList.Codes._3_RoadTransport,
						expectedIdentificationNumber: "AS222",
						transportAtDeparture: "AS222");
				AssertIdentificationNumber(
						inlandTransportMode: ModeOfTransportList.Codes._3_RoadTransport,
						expectedIdentificationNumber: "AS222",
						trailer1IDAtDeparture: "AS222");
				AssertIdentificationNumber(
						inlandTransportMode: ModeOfTransportList.Codes._3_RoadTransport,
						expectedIdentificationNumber: "AS222",
						trailer2IDAtDeparture: "AS222");
			});
		}

		public void TestIdentificationNumber_4()
		{
			CombineAssertions(() =>
			{
				AssertIdentificationNumber(
						inlandTransportMode: ModeOfTransportList.Codes._4_AirTransport,
						expectedIdentificationNumber: "AS222",
						aircraftIDAtDeparture: "AS222");
				AssertIdentificationNumber(
						inlandTransportMode: ModeOfTransportList.Codes._4_AirTransport,
						expectedIdentificationNumber: "AS222",
						transportAtDeparture: "AS222");
			});
		}

		public void TestIdentificationNumber_8()
		{
			AssertIdentificationNumber(inlandTransportMode: ModeOfTransportList.Codes._8_InlandWaterwayTransport, expectedIdentificationNumber: "AS222", vesselNameAtDeparture: "AS222");
		}

		public void TestIdentificationNumber_9()
		{
			AssertIdentificationNumber(inlandTransportMode: ModeOfTransportList.Codes._9_OwnPropulsion, expectedIdentificationNumber: "AS222", transportAtDeparture: "AS222");
		}

		public void TestNationality_1()
		{
			AssertNationality(inlandTransportMode: ModeOfTransportList.Codes._1_SeaTransport, expectedNationality: "AU", vesselNameAtDeparture: "AU123", vesselCountryAtDeparture: "AU");
		}

		public void TestNationality_2()
		{
			CombineAssertions(() =>
			{
				AssertNationality(
						inlandTransportMode: ModeOfTransportList.Codes._2_RailTransport,
						expectedNationality: "AU",
						trailer1IDAtDeparture: "AU123",
						trailer1NationalityAtDeparture: "AU",
						transportCountryAtDeparture: "IE");
				AssertNationality(
						inlandTransportMode: ModeOfTransportList.Codes._2_RailTransport,
						expectedNationality: "IE",
						transportAtDeparture: "IE123",
						trailer1NationalityAtDeparture: "AU",
						transportCountryAtDeparture: "IE");
			});
		}

		public void TestNationality_3()
		{
			CombineAssertions(() =>
			{
				AssertNationality(
						inlandTransportMode: ModeOfTransportList.Codes._3_RoadTransport,
						expectedNationality: "AU",
						trailer1IDAtDeparture: "AU123",
						trailer2IDAtDeparture: "",
						transportAtDeparture: "",
						trailer1NationalityAtDeparture: "AU",
						trailer2NationalityAtDeparture: "CN",
						transportCountryAtDeparture: "IE");
				AssertNationality(
						inlandTransportMode: ModeOfTransportList.Codes._3_RoadTransport,
						expectedNationality: "CN",
						trailer1IDAtDeparture: "",
						trailer2IDAtDeparture: "CN123",
						transportAtDeparture: "",
						trailer1NationalityAtDeparture: "AU",
						trailer2NationalityAtDeparture: "CN",
						transportCountryAtDeparture: "IE");
				AssertNationality(
						inlandTransportMode: ModeOfTransportList.Codes._3_RoadTransport,
						expectedNationality: "IE",
						trailer1IDAtDeparture: "",
						trailer2IDAtDeparture: "",
						transportAtDeparture: "IE123",
						trailer1NationalityAtDeparture: "AU",
						trailer2NationalityAtDeparture: "CN",
						transportCountryAtDeparture: "IE");
			});
		}

		public void TestNationality_4()
		{
			CombineAssertions(() =>
			{
				AssertNationality(
						inlandTransportMode: ModeOfTransportList.Codes._4_AirTransport,
						expectedNationality: "AU",
						transportAtDeparture: "Tra_AU123",
						transportCountryAtDeparture: "AU");
				AssertNationality(
						inlandTransportMode: ModeOfTransportList.Codes._4_AirTransport,
						expectedNationality: "AU",
						aircraftIDAtDeparture: "Air_AU123",
						transportCountryAtDeparture: "AU");
			});
		}

		public void TestNationality_8()
		{
			AssertNationality(
					inlandTransportMode: ModeOfTransportList.Codes._8_InlandWaterwayTransport,
					expectedNationality: "AU",
					vesselNameAtDeparture: "AU123",
					vesselCountryAtDeparture: "AU");
		}

		public void TestNationality_9()
		{
			AssertNationality(inlandTransportMode: ModeOfTransportList.Codes._9_OwnPropulsion, expectedNationality: "AU", transportAtDeparture: "AU123", transportCountryAtDeparture: "AU");
		}

		void AssertTypeOfIdentification(
			string inlandTransportMode,
			string expectedTypeOfIdentification,
			string transportAtDeparture = null,
			string transportTypeAtDeparture = null,
			string trailer1IDAtDeparture = null,
			string trailer2IDAtDeparture = null,
			string aircraftIDAtDeparture = null)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			var bill = nctsHeader.Bills.AddNew();

			movementHeader.InlandTransportModeAtDeparture = inlandTransportMode;

			if (transportAtDeparture != null)
			{
				movementHeader.TransportAtDeparture = transportAtDeparture;
				bill.TransportAtDeparture = transportAtDeparture;
			}
			if (transportTypeAtDeparture != null)
			{
				movementHeader.TransportTypeAtDeparture = transportTypeAtDeparture;
				bill.TransportTypeAtDeparture = transportTypeAtDeparture;
			}
			if (trailer1IDAtDeparture != null)
			{
				movementHeader.Trailer1IDAtDeparture = trailer1IDAtDeparture;
				bill.Trailer1IDAtDeparture = trailer1IDAtDeparture;
			}
			if (trailer2IDAtDeparture != null)
			{
				movementHeader.Trailer2IDAtDeparture = trailer2IDAtDeparture;
				bill.Trailer2IDAtDeparture = trailer2IDAtDeparture;
			}
			if (aircraftIDAtDeparture != null)
			{
				movementHeader.AircraftIDAtDeparture = aircraftIDAtDeparture;
				bill.AircraftIDAtDeparture = aircraftIDAtDeparture;
			}

			AssertEquals("TypeOfIdentification of MovementHeader", expectedTypeOfIdentification, DepartureTransportMeansProvider.GetTransportMeans(movementHeader).First().TypeOfIdentification);
			AssertEquals("TypeOfIdentification of Bill", expectedTypeOfIdentification, DepartureTransportMeansProvider.GetTransportMeans(bill).First().TypeOfIdentification);
		}

		void AssertIdentificationNumber(
			string inlandTransportMode,
			string expectedIdentificationNumber,
			string transportAtDeparture = null,
			string trailer1IDAtDeparture = null,
			string trailer2IDAtDeparture = null,
			string aircraftIDAtDeparture = null,
			string vesselNameAtDeparture = null)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			var bill = nctsHeader.Bills.AddNew();

			movementHeader.InlandTransportModeAtDeparture = inlandTransportMode;
			if (transportAtDeparture != null)
			{
				movementHeader.TransportAtDeparture = transportAtDeparture;
				bill.TransportAtDeparture = transportAtDeparture;
			}
			if (trailer1IDAtDeparture != null)
			{
				movementHeader.Trailer1IDAtDeparture = trailer1IDAtDeparture;
				bill.Trailer1IDAtDeparture = trailer1IDAtDeparture;
			}
			if (trailer2IDAtDeparture != null)
			{
				movementHeader.Trailer2IDAtDeparture = trailer2IDAtDeparture;
				bill.Trailer2IDAtDeparture = trailer2IDAtDeparture;
			}
			if (aircraftIDAtDeparture != null)
			{
				movementHeader.AircraftIDAtDeparture = aircraftIDAtDeparture;
				bill.AircraftIDAtDeparture = aircraftIDAtDeparture;
			}
			if (vesselNameAtDeparture != null)
			{
				movementHeader.VesselNameAtDeparture = vesselNameAtDeparture;
				bill.VesselNameAtDeparture = vesselNameAtDeparture;
			}

			AssertEquals("IdentificationNumber of MovementHeader", expectedIdentificationNumber, DepartureTransportMeansProvider.GetTransportMeans(movementHeader).First().IdentificationNumber);
			AssertEquals("IdentificationNumber of Bill", expectedIdentificationNumber, DepartureTransportMeansProvider.GetTransportMeans(bill).First().IdentificationNumber);
		}

		void AssertNationality(
			string inlandTransportMode,
			string expectedNationality,
			string transportAtDeparture = null,
			string trailer1IDAtDeparture = null,
			string trailer2IDAtDeparture = null,
			string aircraftIDAtDeparture = null,
			string vesselNameAtDeparture = null,
			string transportCountryAtDeparture = null,
			string trailer1NationalityAtDeparture = null,
			string trailer2NationalityAtDeparture = null,
			string vesselCountryAtDeparture = null)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			var bill = nctsHeader.Bills.AddNew();

			movementHeader.InlandTransportModeAtDeparture = inlandTransportMode;

			if (transportAtDeparture != null)
			{
				movementHeader.TransportAtDeparture = transportAtDeparture;
				bill.TransportAtDeparture = transportAtDeparture;
			}
			if (trailer1IDAtDeparture != null)
			{
				movementHeader.Trailer1IDAtDeparture = trailer1IDAtDeparture;
				bill.Trailer1IDAtDeparture = trailer1IDAtDeparture;
			}
			if (trailer2IDAtDeparture != null)
			{
				movementHeader.Trailer2IDAtDeparture = trailer2IDAtDeparture;
				bill.Trailer2IDAtDeparture = trailer2IDAtDeparture;
			}
			if (aircraftIDAtDeparture != null)
			{
				movementHeader.AircraftIDAtDeparture = aircraftIDAtDeparture;
				bill.AircraftIDAtDeparture = aircraftIDAtDeparture;
			}
			if (vesselNameAtDeparture != null)
			{
				movementHeader.VesselNameAtDeparture = vesselNameAtDeparture;
				bill.VesselNameAtDeparture = vesselNameAtDeparture;
			}
			if (transportCountryAtDeparture != null)
			{
				movementHeader.TransportCountryAtDeparture = transportCountryAtDeparture;
				bill.TransportCountryAtDeparture = transportCountryAtDeparture;
			}
			if (trailer1NationalityAtDeparture != null)
			{
				movementHeader.Trailer1NationalityAtDeparture = trailer1NationalityAtDeparture;
				bill.Trailer1NationalityAtDeparture = trailer1NationalityAtDeparture;
			}
			if (trailer2NationalityAtDeparture != null)
			{
				movementHeader.Trailer2NationalityAtDeparture = trailer2NationalityAtDeparture;
				bill.Trailer2NationalityAtDeparture = trailer2NationalityAtDeparture;
			}
			if (vesselCountryAtDeparture != null)
			{
				movementHeader.VesselCountryAtDeparture = vesselCountryAtDeparture;
				bill.VesselCountryAtDeparture = vesselCountryAtDeparture;
			}

			AssertEquals("Nationality of MovementHeader", expectedNationality, DepartureTransportMeansProvider.GetTransportMeans(movementHeader).First().Nationality);
			AssertEquals("Nationality of Bill", expectedNationality, DepartureTransportMeansProvider.GetTransportMeans(bill).First().Nationality);
		}
	}
}

using System;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class DepartureTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<DepartureTransportMeansProvider>
	{
		public void TestCreateCollection_InlandTransportModeEmpty()
		{
			CombineAssertions(() =>
			{
				var declaration = CreateDeclaration(string.Empty);
				var collection = DepartureTransportMeansProvider.CreateCollection(declaration).ToArray();
				AssertEquals("collection.Length", 0, collection.Length);
			});
		}

		public void TestCreateCollection_Sea()
		{
			CombineAssertions(() =>
			{
				var declaration = CreateDeclaration(Core.Constants.TransportModes.Sea);
				var collection = DepartureTransportMeansProvider.CreateCollection(declaration).ToArray();
				AssertEquals("collection.Length", 1, collection.Length);
				AssertDepartureTransportMeans(collection[0], "AB", "ID1234567", "NZ");
			});
		}

		public void TestCreateCollection_InlandWaterwayTransport()
		{
			CombineAssertions(() =>
			{
				var declaration = CreateDeclaration(Core.Constants.TransportModes.InlandWaterwayTransport);
				var collection = DepartureTransportMeansProvider.CreateCollection(declaration).ToArray();
				AssertEquals("collection.Length", 1, collection.Length);
				AssertDepartureTransportMeans(collection[0], "AB", "ID1234567", "NZ");
			});
		}

		public void TestCreateCollection_OwnPropulsion()
		{
			CombineAssertions(() =>
			{
				var declaration = CreateDeclaration(Core.Constants.TransportModes.OwnPropulsion);
				var collection = DepartureTransportMeansProvider.CreateCollection(declaration).ToArray();
				AssertEquals("collection.Length", 1, collection.Length);
				AssertDepartureTransportMeans(collection[0], "AB", "ID1234567", "NZ");
			});
		}

		public void TestCreateCollection_Road()
		{
			var declaration = CreateDeclaration(Core.Constants.TransportModes.Road);
			var collection = DepartureTransportMeansProvider.CreateCollection(declaration).ToArray();
			CombineAssertions("Full", () =>
			{
				AssertEquals("collection.Length", 3, collection.Length);
				AssertDepartureTransportMeans(collection[0], "AB", "ID1234567", "NZ");
				AssertDepartureTransportMeans(collection[1], "31", "TRG123", "NA");
				AssertDepartureTransportMeans(collection[2], "31", "TRG456", "NB");
			});
			CombineAssertions("JE_Trailer2RegNo=Empty,JE_RN_NKTrailer2Nationality=Empty", () =>
			{
				declaration.JE_Trailer2RegNo = ZString.Empty;
				declaration.JE_RN_NKTrailer2Nationality = ZString.Empty;
				collection = DepartureTransportMeansProvider.CreateCollection(declaration).ToArray();
				AssertEquals("collection.Length", 2, collection.Length);
				AssertDepartureTransportMeans(collection[0], "AB", "ID1234567", "NZ");
				AssertDepartureTransportMeans(collection[1], "31", "TRG123", "NA");
			});
			CombineAssertions("JE_Trailer1RegNo=Empty,JE_RN_NKTrailer1Nationality=Empty", () =>
			{
				declaration.JE_Trailer2RegNo = "TRG456";
				declaration.JE_RN_NKTrailer2Nationality = "NB";
				declaration.JE_Trailer1RegNo = ZString.Empty;
				declaration.JE_RN_NKTrailer1Nationality = ZString.Empty;
				collection = DepartureTransportMeansProvider.CreateCollection(declaration).ToArray();
				AssertEquals("collection.Length", 2, collection.Length);
				AssertDepartureTransportMeans(collection[0], "AB", "ID1234567", "NZ");
				AssertDepartureTransportMeans(collection[1], "31", "TRG456", "NB");
			});
			CombineAssertions("JE_Trailer1RegNo=Empty,JE_RN_NKTrailer2Nationality=Empty", () =>
			{
				declaration.JE_Trailer1RegNo = ZString.Empty;
				declaration.JE_RN_NKTrailer1Nationality = "NA";
				declaration.JE_Trailer2RegNo = String.Empty;
				declaration.JE_RN_NKTrailer2Nationality = "NB";
				collection = DepartureTransportMeansProvider.CreateCollection(declaration).ToArray();
				AssertEquals("collection.Length", 1, collection.Length);
				AssertDepartureTransportMeans(collection[0], "AB", "ID1234567", "NZ");
			});
		}

		public void TestCreateCollection_Road_IgnoreSegmentForIdType30WhenEmpty()
		{
			var declaration = CreateDeclaration(Core.Constants.TransportModes.Road);
			var code30 = Customs.Business.TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle;
			declaration.JE_TransportMeans = code30;
			declaration.JE_TransportIDInland = string.Empty;
			declaration.JE_RN_NKTransportNationalityInland = string.Empty;
			CombineAssertions("For JE_TransportModeInland ROA, it is possbile that only Trailer IDs is known. Should ignore TypeOfID30 segment if ID and Nationality is empty.", () =>
			{
				AssertEquals("ROA-30, empty ID and Nationality, should ignore TypeOfID30 segment", false, DepartureTransportMeansProvider.CreateCollection(declaration).Any(item => item.TypeOfIdentification == code30));

				declaration.JE_TransportIDInland = "ID1234567";
				AssertEquals("ROA-30, valid ID, should output TypeOfID30 segment", true, DepartureTransportMeansProvider.CreateCollection(declaration).Any(item => item.TypeOfIdentification == code30));
			});
		}

		public void TestTypeOfIdentification()
		{
			typeOfIdentification = "TOI";
			AssertEquals("TypeOfIdentification", "TOI", Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			identificationNumber = "ID123";
			AssertEquals("IdentificationNumber", "ID123", Provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			nationality = "AU";
			AssertEquals("Nationality", "AU", Provider.Nationality);
		}

		JobDeclaration CreateDeclaration(ZString transportModeInland)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportModeInland = transportModeInland;
			declaration.JE_TransportMeans = "AB";
			declaration.JE_TransportIDInland = "ID1234567";
			declaration.JE_RN_NKTransportNationalityInland = "NZ";
			declaration.JE_Trailer1RegNo = "TRG123";
			declaration.JE_RN_NKTrailer1Nationality = "NA";
			declaration.JE_Trailer2RegNo = "TRG456";
			declaration.JE_RN_NKTrailer2Nationality = "NB";
			declaration.JE_AircraftRegistrationInland = "AIR123";
			return declaration;
		}

		public static void AssertDepartureTransportMeans(ITransportMeans departureTransportMeans, string typeOfIdentification, string identificationNumber, string nationality)
		{
			AssertEquals("TypeOfIdentification", typeOfIdentification, departureTransportMeans.TypeOfIdentification);
			AssertEquals("IdentificationNumber", identificationNumber, departureTransportMeans.IdentificationNumber);
			AssertEquals("Nationality", nationality, departureTransportMeans.Nationality);
		}

		protected override DepartureTransportMeansProvider GetProvider() => new DepartureTransportMeansProvider() { TypeOfIdentification = typeOfIdentification, IdentificationNumber = identificationNumber, Nationality = nationality };
		string typeOfIdentification;
		string identificationNumber;
		string nationality;
	}
}

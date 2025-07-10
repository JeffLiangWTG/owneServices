using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class DepartureTransportMeansProviderTest : DataProviderTestCase<DepartureTransportMeansProvider>
	{
		public void TestTypeOfIdentification()
		{
			const string transportMeans = "01";

			var declaration = CreateDeclaration();

			foreach (var transportMode in GetAllExceptMailAndFixTransportModes())
			{
				declaration.JE_TransportModeInland = transportMode;
				declaration.JE_TransportMeans = transportMeans;

				var departureTransportMeansCollection = DepartureTransportMeansProvider.CreateCollection(declaration).ToArray();

				AssertEquals(1, departureTransportMeansCollection.Length);
				AssertEquals(transportMeans, departureTransportMeansCollection[0].TypeOfIdentification);
			}
		}

		public void TestIdentificationNumber()
		{
			const string identificationNumber = "ABC123";

			var declaration = CreateDeclaration();

			foreach (var transportMode in GetAllExceptMailAndFixTransportModes())
			{
				declaration.JE_TransportModeInland = transportMode;
				declaration.JE_TransportIDInland = identificationNumber;

				var departureTransportMeansCollection = DepartureTransportMeansProvider.CreateCollection(declaration).ToArray();

				AssertEquals(1, departureTransportMeansCollection.Length);
				AssertEquals(identificationNumber, departureTransportMeansCollection[0].IdentificationNumber);
			}
		}

		public void TestNationality()
		{
			const string nationality = "DE";

			var declaration = CreateDeclaration();

			foreach (var transportMode in GetAllExceptMailAndFixTransportModes())
			{
				declaration.JE_TransportModeInland = transportMode;
				declaration.JE_RN_NKTransportNationalityInland = nationality;

				var departureTransportMeansCollection = DepartureTransportMeansProvider.CreateCollection(declaration).ToArray();

				AssertEquals(1, departureTransportMeansCollection.Length);
				AssertEquals(nationality, departureTransportMeansCollection[0].Nationality);
			}
		}

		public void TestCreateCollectionForMailOrFix_ShouldReturnEmptyCollection_TransportMeansEmpty()
		{
			const string transportMeans = "01";
			const string nationality = "DE";
			const string identificationNumber = "ABC123";

			var declaration = CreateDeclaration();

			foreach (var transportMode in GetMailAndFixTransportModes())
			{
				declaration.JE_TransportModeInland = transportMode;
				declaration.JE_TransportIDInland = identificationNumber;
				declaration.JE_RN_NKTransportNationalityInland = nationality;

				declaration.JE_TransportMeans = transportMeans;
				var departureTransportMeansCollection = DepartureTransportMeansProvider.CreateCollection(declaration).ToArray();
				AssertEquals(1, departureTransportMeansCollection.Length);
				AssertEquals(nationality, departureTransportMeansCollection[0].Nationality);

				declaration.JE_TransportMeans = string.Empty;
				AssertEquals(false, DepartureTransportMeansProvider.CreateCollection(declaration).Any());
			}
		}

		public void TestCreateCollection_ShouldReturnEmptyCollection_WhenTransportModeInlandIsEmpty()
		{
			var declaration = CreateDeclaration(transportModeInland: string.Empty);

			var departureTransportMeansCollection = DepartureTransportMeansProvider.CreateCollection(declaration);

			AssertEquals(0, departureTransportMeansCollection.Count);
		}

		public void TestCreateCollection_ShouldReturnEmptyCollection_WhenEntryInstructionHas9AtPosition4()
		{
			var declaration = CreateDeclaration();
			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.CEI_Style = "000900";

			foreach (var transportMode in GetAllExceptMailAndFixTransportModes())
			{
				declaration.JE_TransportModeInland = transportMode;

				var departureTransportMeansCollection = DepartureTransportMeansProvider.CreateCollection(declaration);

				AssertEquals(0, departureTransportMeansCollection.Count);
			}
		}

		public void TestCreateCollection_ShouldMapAdditionalWagons_WhenTransportModeInlandIsRAI()
		{
			const string additionalWagonIdentificationNumber = "ADW";
			const string additionalWagonNationality = "AT";

			var declaration = CreateDeclaration(Core.Constants.TransportModes.Rail);

			var additionalWagon = declaration.InlandTransports.AddNew();
			additionalWagon.CY_Data = additionalWagonIdentificationNumber;
			additionalWagon.Nationality = additionalWagonNationality;

			var departureTransportMeansCollection = DepartureTransportMeansProvider.CreateCollection(declaration).ToArray();

			AssertEquals(2, departureTransportMeansCollection.Length);

			var departureTransportMeans = departureTransportMeansCollection[1];
			AssertEquals(TransportMeansList.Codes.WagonNumber, departureTransportMeans.TypeOfIdentification);
			AssertEquals(additionalWagonIdentificationNumber, departureTransportMeans.IdentificationNumber);
			AssertEquals(additionalWagonNationality, departureTransportMeans.Nationality);
		}

		public void TestCreateCollection_ShouldMapTrailers_WhenTransportModeInlandIsROA()
		{
			const string trailer1IdentificationNumber = "TR1";
			const string trailer1Nationality = "DE";

			const string trailer2IdentificationNumber = "TR2";
			const string trailer2Nationality = "AT";

			var declaration = CreateDeclaration(Core.Constants.TransportModes.Road);

			declaration.JE_Trailer1RegNo = trailer1IdentificationNumber;
			declaration.JE_RN_NKTrailer1Nationality = trailer1Nationality;

			declaration.JE_Trailer2RegNo = trailer2IdentificationNumber;
			declaration.JE_RN_NKTrailer2Nationality = trailer2Nationality;

			var departureTransportMeansCollection = DepartureTransportMeansProvider.CreateCollection(declaration).ToArray();

			AssertEquals(3, departureTransportMeansCollection.Length);

			var departureTransportMeans1 = departureTransportMeansCollection[1];
			AssertEquals(TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, departureTransportMeans1.TypeOfIdentification);
			AssertEquals(trailer1IdentificationNumber, departureTransportMeans1.IdentificationNumber);
			AssertEquals(trailer1Nationality, departureTransportMeans1.Nationality);

			var departureTransportMeans2 = departureTransportMeansCollection[2];
			AssertEquals(TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, departureTransportMeans2.TypeOfIdentification);
			AssertEquals(trailer2IdentificationNumber, departureTransportMeans2.IdentificationNumber);
			AssertEquals(trailer2Nationality, departureTransportMeans2.Nationality);
		}

		protected override DepartureTransportMeansProvider GetProvider() => new DepartureTransportMeansProvider();

		JobDeclaration CreateDeclaration(string transportModeInland = null, string transportMeans = null, string transportIdInland = null, string transportNationalityInland = null)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportModeInland = transportModeInland;
			declaration.JE_TransportMeans = transportMeans;
			declaration.JE_TransportIDInland = transportIdInland;
			declaration.JE_RN_NKTransportNationalityInland = transportNationalityInland;

			return declaration;
		}

		static string[] GetAllExceptMailAndFixTransportModes() =>
			new[]
			{
				Core.Constants.TransportModes.Air,
				Core.Constants.TransportModes.Sea,
				Core.Constants.TransportModes.InlandWaterwayTransport,
				Core.Constants.TransportModes.OwnPropulsion,
				Core.Constants.TransportModes.Rail,
				Core.Constants.TransportModes.Road
			};

		static string[] GetMailAndFixTransportModes() =>
			new[]
			{
				Core.Constants.TransportModes.Mail,
				Core.Constants.TransportModes.FixedTransportInstallations
			};
	}
}

using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.Declaration.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	sealed class ArrivalTransportMeansWrapperTest : Customs.Business.Testing.DataProviderTestCase<ArrivalTransportMeansWrapper>
	{
		protected override ArrivalTransportMeansWrapper GetProvider()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.ZG_Box18TransportID = "ID";
			return ArrivalTransportMeansWrapper.New(declaration);
		}

		ArrivalTransportMeansWrapper GetCustomProvider(string transportMode, string transportMeans)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = transportMode;
			declaration.JE_TransportMeans = transportMeans;
			declaration.JE_VoyageFlightNo = "CX100";
			declaration.JE_VesselName = "MY VESSEL";

			return ArrivalTransportMeansWrapper.New(declaration);
		}

		public void TestIdentificationNumber()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "LLO YDS";
			vessel.RV_Name = "MY VESSEL";

			var provider = GetCustomProvider(Core.Constants.TransportModes.Air, TransportMeansList.Codes.IataFlightNumber);
			AssertEquals("IdentificationNumber should be equal to JE_VoyageFlightNo when JE_TransportMode is AIR and JE_TransportMeans is 40.", "CX100", provider.IdentificationNumber);

			provider = GetCustomProvider(Core.Constants.TransportModes.Air, TransportMeansList.Codes.RegistrationNumberOfTheAircraft);
			AssertEquals("IdentificationNumber should be empty when JE_TransportMode is AIR and JE_TransportMeans is 41.", string.Empty, provider.IdentificationNumber);

			provider = GetCustomProvider(Core.Constants.TransportModes.InlandWaterwayTransport, TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel);
			AssertEquals("IdentificationNumber should be equal to JE_VesselName when JE_TransportMode is IWT and JE_TransportMeans is 81.", "MYVESSEL", provider.IdentificationNumber);

			provider = GetCustomProvider(Core.Constants.TransportModes.InlandWaterwayTransport, TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode);
			AssertEquals("IdentificationNumber should be empty when JE_TransportMode is IWT and JE_TransportMeans is 80.", string.Empty, provider.IdentificationNumber);

			provider = GetCustomProvider(Core.Constants.TransportModes.Rail, TransportMeansList.Codes.WagonNumber);
			AssertEquals("IdentificationNumber should be equal to JE_VesselName when JE_TransportMode is RAI and JE_TransportMeans is 20.", "MYVESSEL", provider.IdentificationNumber);

			provider = GetCustomProvider(Core.Constants.TransportModes.Rail, TransportMeansList.Codes.TrainNumber);
			AssertEquals("IdentificationNumber should be empty when JE_TransportMode is RAI and JE_TransportMeans is 21.", string.Empty, provider.IdentificationNumber);

			provider = GetCustomProvider(Core.Constants.TransportModes.Road, TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle);
			AssertEquals("IdentificationNumber should be equal to JE_VesselName when JE_TransportMode is ROA and JE_TransportMeans is 30.", "MYVESSEL", provider.IdentificationNumber);

			provider = GetCustomProvider(Core.Constants.TransportModes.Road, TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer);
			AssertEquals("IdentificationNumber should be empty when JE_TransportMode is ROA and JE_TransportMeans is 31.", string.Empty, provider.IdentificationNumber);

			provider = GetCustomProvider(Core.Constants.TransportModes.Sea, TransportMeansList.Codes.NameOfTheSeaGoingVessel);
			AssertEquals("IdentificationNumber should be equal to JE_VesselName when JE_TransportMode is SEA and JE_TransportMeans is 11.", "MYVESSEL", provider.IdentificationNumber);

			provider = GetCustomProvider(Core.Constants.TransportModes.Sea, TransportMeansList.Codes.ImoShipIdentificationNumber);
			AssertEquals("IdentificationNumber should be equal to RV_LloydsNumber when JE_TransportMode is SEA and JE_TransportMeans is 10.", "LLOYDS", provider.IdentificationNumber);
		}

		public void TestTypeOfIdentification()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclarationForTest>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				AssertEquals("TypeOfIdentification should be equal to JE_TransportMeans.", "20", Provider.TypeOfIdentification);
			}
		}
	}
}

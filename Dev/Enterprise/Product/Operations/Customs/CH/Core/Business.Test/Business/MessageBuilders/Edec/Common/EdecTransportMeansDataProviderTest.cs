using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business.Testing;

public class EdecTransportMeansDataProviderTest : TestCaseWithFactory
{
	public void TestConstructorNullArgument()
	{
		var declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			AssertNull("Null", EdecTransportMeansDataProvider.New(null));
			AssertNotNull("Not Null", EdecTransportMeansDataProvider.New(declaration));
		});
	}

	public void TestProvider()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
		declaration.JE_VehicleType = "1";
		declaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.Switzerland;
		declaration.JE_VesselName = "Cargo123";

		var messageBuilder = EdecTransportMeansDataProvider.New(declaration);

		CombineAssertions(() =>
		{
			AssertEquals(nameof(messageBuilder.TransportMode), "2", messageBuilder.TransportMode);
			AssertEquals(nameof(messageBuilder.TransportationType), "1", messageBuilder.TransportationType);
			AssertEquals(nameof(messageBuilder.TransportationCountry), Core.Constants.CountryCodes.Switzerland, messageBuilder.TransportationCountry);
			AssertEquals(nameof(messageBuilder.TransportationNumber), "Cargo123", messageBuilder.TransportationNumber);
		});
	}

	public void TestTransportationNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
		declaration.JE_VesselName = "Vessel123";
		var messageBuilder = EdecTransportMeansDataProvider.New(declaration);
		AssertEquals("When TransportationType != Air, TransportationNumber should return Declaration.JE_VesselName", "Vessel123", messageBuilder.TransportationNumber);

		declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		declaration.JE_VoyageFlightNo = "VoFlNo123";
		messageBuilder = EdecTransportMeansDataProvider.New(declaration);
		AssertEquals("When TransportationType == Air, TransportationNumber should return Declaration.JE_VoyageFlightNo", "VoFlNo123", messageBuilder.TransportationNumber);
	}

	public void TestTransportModeMapping()
	{
		var declaration = Factory.New<JobDeclaration>();
		var transportMeansMessageProvider = new TransportMeansMessageProviderForTest(declaration);
		AssertEquals("Default value should be: ", "0", transportMeansMessageProvider.MapTransportModeExposed("Default"));
		AssertEquals($"{Core.Constants.TransportModes.Rail} should be: ", "2", transportMeansMessageProvider.MapTransportModeExposed(Core.Constants.TransportModes.Rail));
		AssertEquals($"{Core.Constants.TransportModes.Road} should be: ", "3", transportMeansMessageProvider.MapTransportModeExposed(Core.Constants.TransportModes.Road));
		AssertEquals($"{Core.Constants.TransportModes.Air} should be: ", "4", transportMeansMessageProvider.MapTransportModeExposed(Core.Constants.TransportModes.Air));
		AssertEquals($"{Core.Constants.TransportModes.Mail} should be: ", "5", transportMeansMessageProvider.MapTransportModeExposed(Core.Constants.TransportModes.Mail));
		AssertEquals($"{Core.Constants.TransportModes.FixedTransportInstallations} should be: ", "7", transportMeansMessageProvider.MapTransportModeExposed(Core.Constants.TransportModes.FixedTransportInstallations));
		AssertEquals($"{Core.Constants.TransportModes.InlandWaterwayTransport} should be: ", "8", transportMeansMessageProvider.MapTransportModeExposed(Core.Constants.TransportModes.InlandWaterwayTransport));
		AssertEquals($"{Core.Constants.TransportModes.OwnPropulsion} should be: ", "9", transportMeansMessageProvider.MapTransportModeExposed(Core.Constants.TransportModes.OwnPropulsion));
	}

	public class TransportMeansMessageProviderForTest : EdecTransportMeansDataProvider
	{
		public TransportMeansMessageProviderForTest(JobDeclaration declaration) : base(declaration) { }

		public string MapTransportModeExposed(ZString transportMode) => base.MapTransportMode(transportMode);
	}
}

using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(TransportMeansDataProvider))]
sealed class TransportMeansDataProviderTest : BasePassarDataProviderTest<TransportMeansDataProvider>
{
	public void TestNew()
	{
		AssertNull(TransportMeansDataProvider.New(null));
	}

	public void TestProperties()
	{
		Declaration.JE_RN_NKTransportNationality = "GR";

		AssertEquals("Nationality", "GR", DataProvider.Nationality);
	}

	public void TestIdentificationNumber() => CombineAssertions(() =>
	{
		const string flight = "flight";
		const string vessel = "vessel";

		AssertIdentificationNumber(flight, TransportModes.Air);
		AssertIdentificationNumber(vessel, TransportModes.FixedTransportInstallations);
		AssertIdentificationNumber(vessel, TransportModes.InlandWaterwayTransport);
		AssertIdentificationNumber(vessel, TransportModes.OwnPropulsion);
		AssertIdentificationNumber(vessel, TransportModes.Rail);
		AssertIdentificationNumber(vessel, TransportModes.Road);
		AssertIdentificationNumber(null, ZString.Empty);

		void AssertIdentificationNumber(string expectedIdentificationNumber, ZString transportMode)
		{
			Declaration.JE_TransportMode = transportMode;
			Declaration.JE_VoyageFlightNo = flight;
			Declaration.JE_VesselName = vessel;
			AssertEquals($"TransportMode={transportMode}", expectedIdentificationNumber, DataProvider.IdentificationNumber);
		}
	});

	public void TestTypeOfIdentification() => CombineAssertions(() =>
	{
		Declaration.JE_TransportMeans = "07";

		AssertTypeOfIdentification("40", TransportModes.Air);
		AssertTypeOfIdentification("99", TransportModes.FixedTransportInstallations);
		AssertTypeOfIdentification("81", TransportModes.InlandWaterwayTransport);
		AssertTypeOfIdentification("07", TransportModes.OwnPropulsion);
		AssertTypeOfIdentification("21", TransportModes.Rail);
		AssertTypeOfIdentification("30", TransportModes.Road);
		AssertTypeOfIdentification(null, ZString.Empty);

		void AssertTypeOfIdentification(string expectedIdentificationNumber, ZString transportMode)
		{
			Declaration.JE_TransportMode = transportMode;
			AssertEquals($"TransportMode={transportMode}", expectedIdentificationNumber, DataProvider.TypeOfIdentification);
		}
	});

	protected override TransportMeansDataProvider CreateDataProvider() => TransportMeansDataProvider.New(Declaration);
}

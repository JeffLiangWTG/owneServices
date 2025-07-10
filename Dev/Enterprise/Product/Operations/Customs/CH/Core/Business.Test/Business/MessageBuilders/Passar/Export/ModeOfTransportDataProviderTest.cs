using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.CustomsLists;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ModeOfTransportDataProvider))]
sealed class ModeOfTransportDataProviderTest : BasePassarDataProviderTest<ModeOfTransportDataProvider>
{
	public void TestNew()
	{
		AssertNull(ModeOfTransportDataProvider.New(null));
	}

	public void TestInlandModeOfTransport() => CombineAssertions(() =>
	{
		AssertInlandModeOfTransport("4", TransportTypeGenericList.Codes.Air);
		AssertInlandModeOfTransport("7", TransportTypeGenericList.Codes.FixedTransportInstallations);
		AssertInlandModeOfTransport("8", TransportTypeGenericList.Codes.InlandWaterwayTransport);
		AssertInlandModeOfTransport("9", TransportTypeGenericList.Codes.OwnPropulsion);
		AssertInlandModeOfTransport("2", TransportTypeGenericList.Codes.Rail);
		AssertInlandModeOfTransport("3", TransportTypeGenericList.Codes.Road);
		AssertInlandModeOfTransport(null, ZString.Empty);

		void AssertInlandModeOfTransport(string expectedInlandModeOfTransport, ZString transportMode)
		{
			Declaration.JE_TransportMode = transportMode;
			AssertEquals($"TransportMode={transportMode}", expectedInlandModeOfTransport, DataProvider.InlandModeOfTransport);
		}
	});

	public void TestTransportMeans() => CombineAssertions(() =>
	{
		Declaration.JE_TransportMode = TransportTypeList.Codes.OwnPropulsion;
		Declaration.JE_VoyageFlightNo = ZString.Empty;
		Declaration.JE_VesselName = ZString.Empty;
		Declaration.JE_RN_NKTransportNationality = ZString.Empty;
		AssertNotNull(DataProvider.TransportMeans);
		AssertSame("cached", DataProvider.TransportMeans, DataProvider.TransportMeans);

		ResetDataProvider();

		Declaration.JE_TransportMode = TransportTypeList.Codes.Air;
		AssertNull("TransportDataProvider null", DataProvider.TransportMeans);

		ResetDataProvider();

		Declaration.JE_VoyageFlightNo = "AB";
		AssertNotNull("TransportDataProvider not null", DataProvider.TransportMeans);

		ResetDataProvider();

		Declaration.JE_TransportMode = ZString.Empty;
		AssertNull("TransportDataProvider null", DataProvider.TransportMeans);
	});

	protected override ModeOfTransportDataProvider CreateDataProvider() => ModeOfTransportDataProvider.New(Declaration);
}

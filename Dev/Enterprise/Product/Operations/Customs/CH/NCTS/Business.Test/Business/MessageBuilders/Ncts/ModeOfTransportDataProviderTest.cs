using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class ModeOfTransportDataProviderTest : BaseDepartureDataProviderTest<ModeOfTransportDataProvider, NctsHeaderDepartureMessageSendingObject>
{
	public void TestNew() => AssertNull("NctsHeader==null", ModeOfTransportDataProvider.New(null));

	public void TestProvider()
	{
		const string inlandModeOfTransport = ModeOfTransportList.Codes._2_RailTransport;

		NctsHeader.MovementHeader.BM_InlandTransportMode = inlandModeOfTransport;

		AssertEquals("InlandModeOfTransport", inlandModeOfTransport, DataProvider.InlandModeOfTransport);
	}

	public void TestTransportMeans()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(DataProvider.TransportMeans);
			AssertType<TransportMeansAtDepartureDataProvider>("type", DataProvider.TransportMeans);
			AssertSame("cached", DataProvider.TransportMeans, DataProvider.TransportMeans);
		});
	}

	protected override ModeOfTransportDataProvider CreateDataProvider() => ModeOfTransportDataProvider.New(NctsHeader.MovementHeader);
}

using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSArrivalTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<PNTSArrivalTransportMeansProvider>
{
	public void TestTypeOfIdentification() => CombineAssertions(() =>
	{
		AssertEquals(0, provider.TypeOfIdentification);
		transportMeans.TPM_TypeOfIdentification = "11";
		AssertEquals(11, provider.TypeOfIdentification);
		transportMeans.TPM_TypeOfIdentification = "X";
		AssertEquals(0, provider.TypeOfIdentification);
	});

	public void TestIdentificationNumber()
	{
		transportMeans.TPM_IdentificationNumber = "RN";
		AssertEquals("RN", provider.IdentificationNumber);
	}

	protected override PNTSArrivalTransportMeansProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		transportMeans = Factory.New<ArrivalTransportMeans>();
		provider = new PNTSArrivalTransportMeansProvider(transportMeans);
	}

	ArrivalTransportMeans transportMeans;
	PNTSArrivalTransportMeansProvider provider;
}

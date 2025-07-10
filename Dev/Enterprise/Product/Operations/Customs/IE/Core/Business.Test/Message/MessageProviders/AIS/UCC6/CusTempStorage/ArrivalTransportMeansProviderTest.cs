using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class ArrivalTransportMeansProviderTest : DataProviderTestCase<ArrivalTransportMeansProvider>
	{
		public void TestType()
		{
			arrivalTransportMeans.TPM_TypeOfIdentification = "TI";
			AssertEquals("Type", "TI", Provider.Type);
		}

		public void TestId()
		{
			arrivalTransportMeans.TPM_IdentificationNumber = "Identification";
			AssertEquals("Id", "Identification", Provider.Id);
		}

		protected override void SetUp()
		{
			base.SetUp();

			arrivalTransportMeans = Factory.New<ArrivalTransportMeans>();
		}
		ArrivalTransportMeans arrivalTransportMeans;

		ArrivalTransportMeansProvider GenerateProvider(ArrivalTransportMeans arrivalTransportMeans) => ArrivalTransportMeansProvider.New(arrivalTransportMeans);

		protected sealed override ArrivalTransportMeansProvider GetProvider()
		{
			return GenerateProvider(arrivalTransportMeans);
		}
	}
}

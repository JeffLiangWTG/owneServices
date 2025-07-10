using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class ArrivalTransportMeansProviderTest : DataProviderTestCase<ArrivalTransportMeansProvider>
	{
		public void TestIIdType()
		{
			Assert("Should implement IIdType", Provider is IIdType);
		}

		public void TestType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMeans = "AA";
			var provider = ArrivalTransportMeansProvider.New(declaration);
			AssertEquals("AA", provider.Type);

			declaration.JE_TransportMeans = "TE";
			AssertEquals("TE", provider.Type);
		}

		public void TestId()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportIDInland = "12WX21";
			var provider = ArrivalTransportMeansProvider.New(declaration);
			AssertEquals("12WX21", provider.Id);

			declaration.JE_TransportIDInland = "241D142";
			AssertEquals("241D142", provider.Id);
		}

		protected override ArrivalTransportMeansProvider GetProvider() => ArrivalTransportMeansProvider.New(Factory.New<JobDeclaration>());
	}
}

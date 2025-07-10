using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class MeansIdentityAtBorderMandatoryProviderTest : DataProviderTestCase<MeansIdentityAtBorderMandatoryProvider>
	{
		public void TestType()
		{
			AssertEquals("Type", "10", Provider.Type);
		}

		public void TestNumber()
		{
			AssertEquals("Number", "Identification", Provider.Number);
		}

		protected override MeansIdentityAtBorderMandatoryProvider GetProvider() => MeansIdentityAtBorderMandatoryProvider.New(header);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			header.TransportType = "10";
			header.ArrivalTransportMeans.TPM_IdentificationNumber = "Identification";
		}
		TemporaryStorageHeader header;
	}
}

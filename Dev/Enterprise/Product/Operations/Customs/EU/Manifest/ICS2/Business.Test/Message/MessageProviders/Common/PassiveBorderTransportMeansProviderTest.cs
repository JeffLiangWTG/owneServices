using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class PassiveBorderTransportMeansProviderTest : DataProviderTestCase<PassiveBorderTransportMeansProvider>
	{
		public void TestConstructor()
		{
			var transportMeans = Factory.NewWithValidTestData<AsycudaTransportMeans>();
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>(() => new PassiveBorderTransportMeansProvider(null));
				AssertNoExceptionThrown(() => new PassiveBorderTransportMeansProvider(transportMeans));
			});
		}

		public void TestIdentificationNumber()
		{
			transportMeans.TPM_IdentificationNumber = "123";
			AssertEquals("IdentificationNumber", "123", Provider.IdentificationNumber);
		}

		public void TestTypeOfIdentification()
		{
			transportMeans.TPM_TypeOfIdentification = "AB";
			AssertEquals("TypeOfIdentification", "AB", Provider.TypeOfIdentification);
		}

		public void TestTypeOfMeansOfTransport()
		{
			transportMeans.TPM_TypeOfTransportMeans = "1234";
			AssertEquals("TypeOfTransportMeans", "1234", Provider.TypeOfMeansOfTransport);
		}

		public void TestNationality()
		{
			transportMeans.TPM_RN_NKTransportNationality = "DE";
			AssertEquals("Nationality", "DE", Provider.Nationality);
		}

		protected override void SetUp()
		{
			base.SetUp();
			transportMeans = Factory.NewWithValidTestData<AsycudaTransportMeans>();
		}
		AsycudaTransportMeans transportMeans;

		protected override PassiveBorderTransportMeansProvider GetProvider()
		{
			return new PassiveBorderTransportMeansProvider(transportMeans);
		}
	}
}

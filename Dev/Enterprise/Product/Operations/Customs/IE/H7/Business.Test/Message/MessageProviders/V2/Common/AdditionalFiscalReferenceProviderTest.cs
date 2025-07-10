using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class AdditionalFiscalReferenceProviderTest : DataProviderTestCase<AdditionalFiscalReferenceProvider>
	{
		public void TestRole()
		{
			AssertEquals("FR5", additionalFiscalReferenceProvider.Role);
		}

		public void TestVatIdentificationNumber()
		{
			AssertEquals("testId", additionalFiscalReferenceProvider.VatIdentificationNumber);
		}

		public void TestSequenceNumber()
		{
			AssertEquals("1", additionalFiscalReferenceProvider.SequenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			additionalFiscalReferenceProvider = new AdditionalFiscalReferenceProvider("testId");
		}
		AdditionalFiscalReferenceProvider additionalFiscalReferenceProvider;

		protected override AdditionalFiscalReferenceProvider GetProvider()
		{
			return additionalFiscalReferenceProvider;
		}
	}
}

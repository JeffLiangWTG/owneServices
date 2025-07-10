using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class SealsProviderTest : DataProviderTestCase<SealsProvider>
	{
		public void TestSequenceNumber()
		{
			AssertEquals(1, Provider.SequenceNumber);
		}

		public void TestIdentifier()
		{
			AssertEquals("sealnr", Provider.Identifier);
		}

		protected override SealsProvider GetProvider() => new SealsProvider("sealnr", 1);
	}
}

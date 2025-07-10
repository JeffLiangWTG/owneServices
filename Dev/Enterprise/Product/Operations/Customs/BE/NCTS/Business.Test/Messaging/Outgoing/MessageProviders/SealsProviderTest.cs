using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(SealsProvider))]
	class SealsProviderTest : Customs.Business.Testing.DataProviderTestCase<SealsProvider>
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

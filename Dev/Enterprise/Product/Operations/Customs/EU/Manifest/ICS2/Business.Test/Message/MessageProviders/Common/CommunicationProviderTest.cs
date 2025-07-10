using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	public class CommunicationProviderTest : DataProviderTestCase<CommunicationProvider>
	{
		public void TestIdentifier()
		{
			AssertEquals("Communication Info", "111-222-3333", Provider.Identifier);
		}

		public void TestType()
		{
			AssertEquals("Communication Type", "TE", Provider.Type);
		}

		CommunicationProvider GenerateProvider(string communicationInfo, string type) => new CommunicationProvider(communicationInfo, type);

		protected override CommunicationProvider GetProvider()
		{
			return GenerateProvider("111-222-3333", "TE");
		}
	}
}

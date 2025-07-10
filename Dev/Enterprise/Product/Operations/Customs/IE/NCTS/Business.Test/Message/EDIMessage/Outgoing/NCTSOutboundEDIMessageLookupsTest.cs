using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class NCTSOutboundEDIMessageLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			var message = Factory.New<NCTSOutboundEDIMessage>();
			var lookups = message.Lookups.MessageTypeList;
			Assert("Codes", !lookups.ContainsCode("034"));
			AssertSame("MessageTypeList", message.Lookups.MessageTypeList, lookups);
		}
	}
}

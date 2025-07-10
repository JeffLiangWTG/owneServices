using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class NCTSInboundEDIMessageLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			AssertSame("MessageTypeList", Factory.GetCachedValue<NCTSIncomingMessageTypeList>(), Factory.New<NCTSInboundEDIMessage>().Lookups.MessageTypeList);
		}
	}
}

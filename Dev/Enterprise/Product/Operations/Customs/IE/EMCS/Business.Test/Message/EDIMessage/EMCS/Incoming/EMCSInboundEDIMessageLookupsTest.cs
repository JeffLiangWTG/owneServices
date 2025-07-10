using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	sealed class EMCSInboundEDIMessageLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			AssertSame("MessageTypeList", Factory.GetCachedValue<EMCSIncomingMessageTypeList>(), Factory.New<EMCSInboundEDIMessage>().Lookups.MessageTypeList);
		}
	}
}

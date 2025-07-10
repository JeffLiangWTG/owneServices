using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	public class EMCSOutboundEDIMessageLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			AssertSame("MessageTypeList", Factory.GetCachedValue<EMCSOutgoingMessageTypeList>(), Factory.New<EMCSOutboundEDIMessage>().Lookups.MessageTypeList);
		}
	}
}

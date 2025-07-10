using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class AESOutBoundEDIMessageLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			AssertSame("MessageTypeList", Factory.GetCachedValue<AESOutgoingMessageTypeList>(), Factory.New<AESOutboundEDIMessage>().Lookups.MessageTypeList);
		}
	}
}

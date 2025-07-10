using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	class AISUCC5OutboundEDIMessageLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			AssertSame("MessageTypeList", Factory.GetCachedValue<AISInterchangeTypeList>(), Factory.New<AISUCC5OutboundEDIMessage>().Lookups.MessageTypeList);
		}
	}
}

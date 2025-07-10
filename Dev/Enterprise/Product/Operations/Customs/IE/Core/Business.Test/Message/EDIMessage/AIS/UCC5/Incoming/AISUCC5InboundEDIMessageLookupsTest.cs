using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	class AISUCC5InboundEDIMessageLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			AssertSame("MessageTypeList", Factory.GetCachedValue<AISInterchangeTypeList>(), Factory.New<AISUCC5InboundEDIMessage>().Lookups.MessageTypeList);
		}
	}
}

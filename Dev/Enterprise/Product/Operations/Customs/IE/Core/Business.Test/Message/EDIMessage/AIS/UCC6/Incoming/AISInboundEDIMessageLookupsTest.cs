using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	class AISInboundEDIMessageLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			AssertSame("MessageTypeList", Factory.GetCachedValue<AISInterchangeTypeList>(), Factory.New<AISInboundEDIMessage>().Lookups.MessageTypeList);
		}
	}
}

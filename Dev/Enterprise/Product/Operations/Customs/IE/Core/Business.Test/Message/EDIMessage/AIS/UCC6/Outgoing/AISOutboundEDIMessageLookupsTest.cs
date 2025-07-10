using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	class AISOutboundEDIMessageLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			AssertSame("MessageTypeList", Factory.GetCachedValue<AISInterchangeTypeList>(), Factory.New<AISOutboundEDIMessage>().Lookups.MessageTypeList);
		}
	}
}

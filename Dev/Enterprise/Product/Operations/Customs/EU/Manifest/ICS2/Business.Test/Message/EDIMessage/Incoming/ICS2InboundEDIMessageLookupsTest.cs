using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	class ICS2InboundEDIMessageLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			AssertSame("MessageTypeList", Factory.GetCachedValue<MessageTypes>(), Factory.New<ICS2InboundEDIMessage>().Lookups.MessageTypeList);
		}
	}
}

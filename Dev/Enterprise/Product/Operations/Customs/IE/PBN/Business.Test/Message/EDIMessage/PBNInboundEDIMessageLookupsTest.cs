using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.PBN.Business.Testing;

sealed class PBNInboundEDIMessageLookupsTest : TestCaseWithFactory
{
	public void TestMessageTypeList()
	{
		AssertSame("MessageTypeList", Factory.GetCachedValue<PBNMessageTypes>(), Factory.New<PBNInboundEDIMessage>().Lookups.MessageTypeList);
	}
}

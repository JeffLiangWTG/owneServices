using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	sealed class EMCSInboundEDIMessageLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			AssertSame("MessageTypeList", Factory.GetCachedValue<EMCSGBIncomingMessageTypeList>(), Factory.New<EMCSInboundEDIMessage>().Lookups.MessageTypeList);
		}
	}
}

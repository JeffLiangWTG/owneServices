using Enterprise.Customs.BE.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(NCTSMessage))]
	sealed class NCTSMessageTest : EDIMessageTest
	{
		public void TestSetDefaultValues()
		{
			AssertEquals("EM_MessageType", SendMessageTypes.Codes.NCT, Factory.New<NCTSMessage>().EM_MessageType);
		}
	}
}

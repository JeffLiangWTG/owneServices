using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(PBNOutboundEDIMessage))]
	sealed class PBNOutboundEDIMessageTest : EDIMessageTest
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<PBNOutboundEDIMessage>();
			AssertEquals(EDIMessage.ApplicationCodes.IECustomsPBN, message.EM_ApplicationCode);
		}
	}
}

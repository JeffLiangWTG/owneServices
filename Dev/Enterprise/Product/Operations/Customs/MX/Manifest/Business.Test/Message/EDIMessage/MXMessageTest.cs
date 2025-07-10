using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	[TestedType(typeof(MXMessage))]
	class MXMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<MXMessage>();
			AssertEquals(EDIInterchange.ApplicationCodes.MXCustoms, message.EM_ApplicationCode);
		}
	}
}

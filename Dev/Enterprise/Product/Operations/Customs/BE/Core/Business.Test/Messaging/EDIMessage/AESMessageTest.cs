using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(AESMessage))]
sealed class AESMessageTest : EDIMessageTest
{
	public void TestSetDefaultValues()
	{
		AssertEquals("EM_MessageType", MessageVersionRegistry.AESDomainCode, Factory.New<AESMessage>().EM_MessageType);
	}

	public void TestMessageNumberPlaceHolderOverride()
	{
		var message = Factory.New<AESMessage>();
		var messageNumberPlaceHolderOverride = message.GetType().GetProperty("MessageNumberPlaceHolderOverride", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(message);
		AssertEquals("&lt;&lt;MSGNO PLACEHOLDER&gt;&gt;", messageNumberPlaceHolderOverride);
	}
}

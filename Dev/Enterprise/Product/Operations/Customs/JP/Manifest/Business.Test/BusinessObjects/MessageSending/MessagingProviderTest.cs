using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing;

[TestedType(typeof(MessagingProvider))]
sealed class MessagingProviderTest : TestCaseWithFactory
{
	public void TestMessageStatusProvider()
	{
		var messagingProvider = new MessagingProvider();
		AssertType<MessageStatusProvider>(messagingProvider.MessageStatusProvider);
	}
}

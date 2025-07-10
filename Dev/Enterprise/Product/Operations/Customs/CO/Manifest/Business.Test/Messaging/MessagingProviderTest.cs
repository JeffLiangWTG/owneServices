using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	class MessagingProviderTest : TestCaseWithFactory
	{
		public void TestMessageStatusProvider()
		{
			var messagingProvider = new MessagingProvider();
			AssertType<MessageStatusProvider>(messagingProvider.MessageStatusProvider);
		}
	}
}

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	using CargoWise.EntityFramework.Testing;

	class MessagingProviderTest : TestCaseWithFactory
	{
		public void TestMessageStatusProvider()
		{
			var messagingProvider = new MessagingProvider();
			AssertType<MessageStatusProvider>(messagingProvider.MessageStatusProvider);
		}
	}
}

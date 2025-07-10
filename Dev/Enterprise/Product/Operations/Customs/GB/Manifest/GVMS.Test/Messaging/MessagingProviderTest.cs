using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	public class MessagingProviderTest : TestCaseWithFactory
	{
		public void TestMessageStatusProvider()
		{
			var messagingProvider = new MessagingProvider();
			AssertType<MessageStatusProvider>(messagingProvider.MessageStatusProvider);
		}
	}
}

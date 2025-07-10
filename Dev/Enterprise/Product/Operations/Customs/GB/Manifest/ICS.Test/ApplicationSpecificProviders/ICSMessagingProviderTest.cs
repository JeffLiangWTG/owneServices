using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.Testing
{
	sealed class ICSMessagingProviderTest : TransactionedTestCase
	{
		public void TestMessageStatusProvider()
		{
			var provider = new ICSMessagingProvider();
			AssertType<ICSMessageStatusProvider>(provider.MessageStatusProvider);
		}
	}
}

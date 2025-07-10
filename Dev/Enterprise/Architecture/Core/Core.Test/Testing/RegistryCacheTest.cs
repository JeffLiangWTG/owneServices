using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class RegistryCacheTest : TransactionedTestCase
	{
		public void TestSetRegistry()
		{
			AssertEquals("MailboxUserName", "", EnvProxy.Instance.Registry.MailboxUserName);
			EnvProxy.Instance.Registry.MailboxUserName = "TestName";
			AssertEquals("MailboxUserName", "TestName", EnvProxy.Instance.Registry.MailboxUserName);
		}

		public void TestGetRegistry()
		{
			AssertEquals("MailboxUserName still not changed", "", EnvProxy.Instance.Registry.MailboxUserName);
		}
	}
}

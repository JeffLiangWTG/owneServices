using NUnit.Framework;
#if NETFRAMEWORK
using NUnit.Framework.TestHelper;
#endif

namespace Enterprise.Messaging.Integration.Testing
{
	sealed class DuplicateMessageExceptionTest : TestCase
	{
#if NETFRAMEWORK
		public void TestDuplicateMessageExceptionSerialisation()
		{
			var applicationReference = "TestapplicationReference";
			var ex = new DuplicateMessageException(applicationReference);
			var exceptionDesirialized = SerializationTestWithAppDomainHelper.PassBetweenAppDomains(ex) as DuplicateMessageException;

			AssertEquals("Message", applicationReference, exceptionDesirialized.ApplicationReference);
		}
#endif
	}
}

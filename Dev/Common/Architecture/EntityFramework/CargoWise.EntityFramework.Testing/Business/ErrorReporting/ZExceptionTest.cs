using NUnit.Framework;

#if NETFRAMEWORK
using NUnit.Framework.TestHelper;
#endif

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZExceptionTest : TestCase
	{
#if NETFRAMEWORK
		public void TestSerializable()
		{
			var ex = new ZException("Message");
			var deserialized = (ZException)SerializationTestWithAppDomainHelper.PassBetweenAppDomains(ex);
			AssertEquals("Deserialized message", "Message", deserialized.Message);
		}
#endif

		public void TestMessage()
		{
			AssertEquals("Message", new ZException("Message").Message);
		}
	}
}

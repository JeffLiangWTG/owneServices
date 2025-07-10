using NUnit.Framework;

#if NETFRAMEWORK
using NUnit.Framework.TestHelper;
#endif

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZConcurrencyCheckFailureExceptionTest : TestCase
	{
		public void TestConstructor()
		{
			var exception = new ZConcurrencyCheckFailureException("Message2", "Heading2", true);
			AssertEquals("Message should be set correctly", "Message2", exception.Message);
			AssertEquals("Heading should be set correctly", "Heading2", exception.Heading);
			AssertEquals("ShouldReprocess should be set correctly", true, exception.ShouldReprocess);

			exception = new ZConcurrencyCheckFailureException("Message3", "Heading3", false);
			AssertEquals("Message should be set correctly", "Message3", exception.Message);
			AssertEquals("Heading should be set correctly", "Heading3", exception.Heading);
			AssertEquals("ShouldReprocess should be set correctly", false, exception.ShouldReprocess);
		}

#if NETFRAMEWORK
		public void TestSerializable()
		{
			var original = new ZConcurrencyCheckFailureException("Some failure message", "Failure Heading", true);

			var roundTripped = SerializationTestWithAppDomainHelper.PassBetweenAppDomains(original) as ZConcurrencyCheckFailureException;

			AssertEquals("Message should be the same", original.Message, roundTripped.Message);
			AssertEquals("Heading should be the same", original.Heading, roundTripped.Heading);
			AssertEquals("ShouldReprocess should be the same", original.ShouldReprocess, roundTripped.ShouldReprocess);
		}
#endif
	}
}

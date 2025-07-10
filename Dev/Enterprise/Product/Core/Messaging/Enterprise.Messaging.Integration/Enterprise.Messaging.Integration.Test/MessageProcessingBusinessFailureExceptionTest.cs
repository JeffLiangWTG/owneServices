using NUnit.Framework;
#if NETFRAMEWORK
using NUnit.Framework.TestHelper;
#endif

namespace Enterprise.Messaging.Integration.Testing
{
	sealed class MessageProcessingBusinessFailureExceptionTest : TestCase
	{
#if NETFRAMEWORK
		public void TestMessageProcessingBusinessFailureExceptionSerialisation()
		{
			var message = "TestMessage";
			var caption = "TestCaption";
			var shouldRetry = true;
			var logNote = "TestLogNote";
			var ex = new MessageProcessingBusinessFailureException(message, caption, shouldRetry, logNote);

			var exceptionDesirialized = SerializationTestWithAppDomainHelper.PassBetweenAppDomains(ex) as MessageProcessingBusinessFailureException;

			AssertEquals("Message", message, exceptionDesirialized.Message);
			AssertEquals("Caption", caption, exceptionDesirialized.Caption);
			AssertEquals("ShouldRetry", shouldRetry, exceptionDesirialized.ShouldRetry);
			AssertEquals("LogNote", logNote, exceptionDesirialized.LogNote);
		}
#endif
	}
}

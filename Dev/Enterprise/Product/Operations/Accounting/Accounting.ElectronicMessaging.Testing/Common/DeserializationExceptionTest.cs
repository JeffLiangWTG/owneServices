using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public class DeserializationExceptionTest : TestCaseWithFactory
	{
		public void TestMessage_EmptyPayload()
		{
			var exception = new DeserializationException("Empty Payload Error Message", "PayloadHeading", "");
			AssertEquals(@"Empty Payload Error Message
PayloadHeading:
<Empty>", exception.Message);
		}

		public void TestMessage_NonEmptyPayload()
		{
			var exception = new DeserializationException("Some error message for non-empty Payload", "PayloadHeading", "Non-Empty Payload");
			AssertEquals(@"Some error message for non-empty Payload
PayloadHeading:
Non-Empty Payload", exception.Message);
		}
	}
}

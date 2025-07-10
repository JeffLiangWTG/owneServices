using System;
using NUnit.Framework;

#if NETFRAMEWORK
using NUnit.Framework.TestHelper;
#endif

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZCannotSaveExceptionTest : TestCase
	{
		public void TestConstructor()
		{
			var exception = new ZCannotSaveException("Message1", "Heading1");
			AssertEquals("Message should be set correctly", "Message1", exception.Message);
			AssertEquals("Heading should be set correctly", "Heading1", exception.Heading);
			AssertEquals("ShouldReprocess should default to false", false, exception.ShouldReprocess);
			AssertEquals("ExceptionType should be set correctly", ExceptionType.Unhandled, exception.Type);

			exception = new ZCannotSaveException("Message1B", "Heading1B", ExceptionType.BusinessFailure);
			AssertEquals("Message should be set correctly", "Message1B", exception.Message);
			AssertEquals("Heading should be set correctly", "Heading1B", exception.Heading);
			AssertEquals("ShouldReprocess should default to false", false, exception.ShouldReprocess);
			AssertEquals("ExceptionType should be set correctly", ExceptionType.BusinessFailure, exception.Type);

			exception = new ZCannotSaveException("Message2", "Heading2", true);
			AssertEquals("Message should be set correctly", "Message2", exception.Message);
			AssertEquals("Heading should be set correctly", "Heading2", exception.Heading);
			AssertEquals("ShouldReprocess should be set correctly", true, exception.ShouldReprocess);
			AssertEquals("ExceptionType should be set correctly", ExceptionType.Unhandled, exception.Type);

			exception = new ZCannotSaveException("Message2B", "Heading2B", true, ExceptionType.BusinessFailure);
			AssertEquals("Message should be set correctly", "Message2B", exception.Message);
			AssertEquals("Heading should be set correctly", "Heading2B", exception.Heading);
			AssertEquals("ShouldReprocess should be set correctly", true, exception.ShouldReprocess);
			AssertEquals("ExceptionType should be set correctly", ExceptionType.BusinessFailure, exception.Type);

			exception = new ZCannotSaveException("Message3", "Heading3", false);
			AssertEquals("Message should be set correctly", "Message3", exception.Message);
			AssertEquals("Heading should be set correctly", "Heading3", exception.Heading);
			AssertEquals("ShouldReprocess should be set correctly", false, exception.ShouldReprocess);
			AssertEquals("ExceptionType should be set correctly", ExceptionType.Unhandled, exception.Type);

			exception = new ZCannotSaveException("Message3B", "Heading3B", false, ExceptionType.BusinessFailure);
			AssertEquals("Message should be set correctly", "Message3B", exception.Message);
			AssertEquals("Heading should be set correctly", "Heading3B", exception.Heading);
			AssertEquals("ShouldReprocess should be set correctly", false, exception.ShouldReprocess);
			AssertEquals("ExceptionType should be set correctly", ExceptionType.BusinessFailure, exception.Type);

			var expectedInnerException = new Exception();
			exception = new ZCannotSaveException("Message4", "Heading4", expectedInnerException);
			AssertEquals("Message should be set correctly", "Message4", exception.Message);
			AssertEquals("Heading should be set correctly", "Heading4", exception.Heading);
			AssertEquals("ShouldReprocess should default to false", false, exception.ShouldReprocess);
			AssertEquals("InnerException", expectedInnerException, exception.InnerException);
			AssertEquals("ExceptionType should be set correctly", ExceptionType.Unhandled, exception.Type);

			exception = new ZCannotSaveException("Message4B", "Heading4B", expectedInnerException, ExceptionType.BusinessFailure);
			AssertEquals("Message should be set correctly", "Message4B", exception.Message);
			AssertEquals("Heading should be set correctly", "Heading4B", exception.Heading);
			AssertEquals("ShouldReprocess should default to false", false, exception.ShouldReprocess);
			AssertEquals("InnerException", expectedInnerException, exception.InnerException);
			AssertEquals("ExceptionType should be set correctly", ExceptionType.BusinessFailure, exception.Type);

			exception = new ZCannotSaveException("Message5", "Heading5", true, expectedInnerException);
			AssertEquals("Message should be set correctly", "Message5", exception.Message);
			AssertEquals("Heading should be set correctly", "Heading5", exception.Heading);
			AssertEquals("ShouldReprocess should be set correctly", true, exception.ShouldReprocess);
			AssertEquals("InnerException", expectedInnerException, exception.InnerException);
			AssertEquals("ExceptionType should be set correctly", ExceptionType.Unhandled, exception.Type);

			exception = new ZCannotSaveException("Message5B", "Heading5B", true, expectedInnerException, ExceptionType.BusinessFailure);
			AssertEquals("Message should be set correctly", "Message5B", exception.Message);
			AssertEquals("Heading should be set correctly", "Heading5B", exception.Heading);
			AssertEquals("ShouldReprocess should be set correctly", true, exception.ShouldReprocess);
			AssertEquals("InnerException", expectedInnerException, exception.InnerException);
			AssertEquals("ExceptionType should be set correctly", ExceptionType.BusinessFailure, exception.Type);

			exception = new ZCannotSaveException("Message6", "Heading6", false, expectedInnerException);
			AssertEquals("Message should be set correctly", "Message6", exception.Message);
			AssertEquals("Heading should be set correctly", "Heading6", exception.Heading);
			AssertEquals("ShouldReprocess should be set correctly", false, exception.ShouldReprocess);
			AssertEquals("InnerException", expectedInnerException, exception.InnerException);
			AssertEquals("ExceptionType should be set correctly", ExceptionType.Unhandled, exception.Type);

			exception = new ZCannotSaveException("Message6B", "Heading6B", false, expectedInnerException, ExceptionType.BusinessFailure);
			AssertEquals("Message should be set correctly", "Message6B", exception.Message);
			AssertEquals("Heading should be set correctly", "Heading6B", exception.Heading);
			AssertEquals("ShouldReprocess should be set correctly", false, exception.ShouldReprocess);
			AssertEquals("InnerException", expectedInnerException, exception.InnerException);
			AssertEquals("ExceptionType should be set correctly", ExceptionType.BusinessFailure, exception.Type);
		}

#if NETFRAMEWORK
		public void TestSerializable()
		{
			var original = new ZCannotSaveException("Some failure message", "Failure Heading", true, ExceptionType.BusinessFailure);

			var roundTripped = SerializationTestWithAppDomainHelper.PassBetweenAppDomains(original) as ZCannotSaveException;

			AssertEquals("Message should be the same", original.Message, roundTripped.Message);
			AssertEquals("Heading should be the same", original.Heading, roundTripped.Heading);
			AssertEquals("ShouldReprocess should be the same", original.ShouldReprocess, roundTripped.ShouldReprocess);
			AssertEquals("ExceptionType should be the same", original.Type, roundTripped.Type);
		}
#endif
	}
}

using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class UnableToInterpretInterchangeExceptionTest : TestCaseWithFactory
{
	public void TestUnableToInterpretInterchangeException()
	{
		CombineAssertions("Single parameter - Exception - constructor", () =>
		{
			var ex = new UnableToInterpretInterchangeException(new ArgumentOutOfRangeException());
			AssertEquals("Default Message", "Unable to parse the message content.", ex.Message);
			AssertNotNull("Inner Exception", ex.InnerException);
			AssertType<ArgumentOutOfRangeException>(ex.InnerException);
		});

		CombineAssertions("Single parameter - Message - constructor", () =>
		{
			var ex = new UnableToInterpretInterchangeException("The message interpreter was unable to read received message.");
			AssertEquals("Message", "The message interpreter was unable to read received message.", ex.Message);
		});

		CombineAssertions("2 parameters - Message And Exception - constructor", () =>
		{
			var ex = new UnableToInterpretInterchangeException("The message interpreter was unable to read received message.", new ArgumentOutOfRangeException());
			AssertEquals("Message", "The message interpreter was unable to read received message.", ex.Message);
			AssertNotNull("Inner Exception", ex.InnerException);
			AssertType<ArgumentOutOfRangeException>(ex.InnerException);
		});
	}
}

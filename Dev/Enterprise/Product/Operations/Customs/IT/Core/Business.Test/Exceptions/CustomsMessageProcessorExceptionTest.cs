using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITCustomsMessageProcessorExceptionTest : TestCaseWithFactory
{
	public void TestITCustomsMessageProcessorException()
	{
		var ex = new CustomsMessageProcessorException("This is a generic error message.");
		AssertEquals("Message", "This is a generic error message.", ex.Message);

		ex = new CustomsMessageProcessorException("The argument was null", new ArgumentOutOfRangeException());
		AssertEquals("Message", "The argument was null", ex.Message);
		AssertNotNull("Inner Exception", ex.InnerException);
		AssertType<ArgumentOutOfRangeException>(ex.InnerException);
	}
}

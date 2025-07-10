using System;
using CargoWise.Data.SqlServer;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Diagnostics.Testing
{
	internal class AlwaysOnDelayProviderExceptionTest : TestCase
	{
		public void TestIsRunnerInternalException()
		{
			var exception = new AlwaysOnDelayProviderException();
			AssertExceptionThrown<Exception>(() => throw exception);
		}

		public void TestInnerException()
		{
			var exception = new Exception();
			var result = new AlwaysOnDelayProviderException("message", exception).InnerException;
			AssertEquals(exception, result);
		}
	}
}

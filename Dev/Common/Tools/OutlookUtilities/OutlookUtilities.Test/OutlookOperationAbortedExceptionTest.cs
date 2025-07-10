using System;
using NUnit.Framework;

namespace Enterprise.Interop.OutlookIntegration.Testing
{
	sealed class OutlookOperationAbortedExceptionTest : TestCase
	{
		public void TestConstructor()
		{
			var ex = new OutlookOperationAbortedException("Message", new InvalidOperationException());
			AssertEquals("Message", ex.Message);
			AssertEquals(true, ex.InnerException is InvalidOperationException);
		}
	}
}

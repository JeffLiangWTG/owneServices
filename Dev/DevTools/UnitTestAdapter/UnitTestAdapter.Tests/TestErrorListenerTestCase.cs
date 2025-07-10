using System;
using NUnit.Framework;
using TestCaseBaseClass = NUnit.Framework.TestCase;

namespace CWNUnit.TestAdapter.Tests
{
	public class TestErrorListenerTestCase : TestCaseBaseClass
	{
		public void TestAddError()
		{
			var listener = new TestErrorListener();
			AssertEquals(0, listener.Errors.Count);

			var e = new Exception("Test");
			((ITestListener)listener).AddError(e, null);

			AssertEquals(1, listener.Errors.Count);
			AssertSame(e, listener.Errors[0]);
		}

		public void TestBeforeEachTest()
		{
			var listener = new TestErrorListener();
			((ITestListener)listener).AddError(new Exception("Test"), null);
			AssertEquals(1, listener.Errors.Count);

			((ITestListener)listener).BeforeEachTest(DateTime.UtcNow);

			AssertEquals(0, listener.Errors.Count);
		}
	}
}

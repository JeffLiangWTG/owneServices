using System;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Utilities.Test.Environment
{
	internal class TestExceptionMessage : TestCase
	{
		public void TestExceptionMessageConstructor()
		{
			Exception testException = new Exception("Test Exception");
			string testKey = "This is a test key";
			string testMessage = "This is a test exception";

			ExceptionMessage testExceptionMessage = new ExceptionMessage(testException, testKey, testMessage);

			AssertEquals(testException, testExceptionMessage.Exception);
			AssertEquals("Key", testKey, testExceptionMessage.Key);
			AssertEquals(testMessage, testExceptionMessage.Message);
		}
	}
}

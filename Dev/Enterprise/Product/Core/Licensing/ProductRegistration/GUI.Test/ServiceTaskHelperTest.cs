using System;
using System.ComponentModel;
using NUnit.Framework;

namespace Enterprise.ProductRegistration.GUI.Test
{
	class ServiceTaskHelperTest : TestCase
	{
		public void TestHandleTryGetServiceStatusException()
		{
			TestException(new Exception("some Exception", new Win32Exception(5)), true);
			TestException(new Exception("some Exception", new Win32Exception(1060)), true);
			TestException(new Exception("some Exception", new Win32Exception(1722)), true);
			TestException(new Exception("some Exception", new Win32Exception(1727)), true);

			TestException(new Exception("some Exception", new Win32Exception(1)), false);

			void TestException(Exception exception, bool expectedResult)
			{
				// Arrange
				// Act
				var result = ServiceTaskHelper.IgnoreTryGetServiceStatusException(exception);

				// Assert
				AssertEquals(expectedResult, result);
			}
		}
	}
}

using System;
using NUnit.Framework;

namespace CargoWise.Common.Collections.Testing
{
	class ExceptionVisibilityAttributeTest : TestCase
	{
		public void TestEvaluate()
		{
			AssertEquals(ExceptionVisibility.User, ExceptionVisibilityAttribute.Evaluate(new UserVisibleException()));
			AssertEquals(ExceptionVisibility.Developer, ExceptionVisibilityAttribute.Evaluate(new Exception()));
		}

		public void TestGetFirstOccurenceOfUserException_UserVisibleException_TopLevel()
		{
			var testMessage = "test message";
			Exception ex = new UserVisibleException(testMessage, new Exception("Exception 1", new Exception("Exception 2", new Exception("Exception 3"))));
			var firstOccurenceOfUserException = ExceptionVisibilityAttribute.GetFirstOccurenceOfUserException(ex);
			AssertNotNull(firstOccurenceOfUserException);
			AssertEquals(testMessage, firstOccurenceOfUserException.Message);
		}

		public void TestGetFirstOccurenceOfUserException_UserVisibleException_InnerException()
		{
			var testMessage = "test message";
			Exception ex = new Exception("Exception 1", new Exception("Exception 2", new UserVisibleException(testMessage, new Exception("Exception 3"))));
			var firstOccurenceOfUserException = ExceptionVisibilityAttribute.GetFirstOccurenceOfUserException(ex);
			AssertNotNull(firstOccurenceOfUserException);
			AssertEquals(testMessage, firstOccurenceOfUserException.Message);
		}

		public void TestGetFirstOccurenceOfUserException_NoUserVisibleException()
		{
			Exception ex = new Exception("Exception 1", new Exception("Exception 2", new Exception("Exception 3")));
			var firstOccurenceOfUserException = ExceptionVisibilityAttribute.GetFirstOccurenceOfUserException(ex);
			AssertNull(firstOccurenceOfUserException);
		}

		#region Test Classes
		[ExceptionVisibility(ExceptionVisibility.User), Serializable]
		class UserVisibleException : Exception
		{
			public UserVisibleException() : base()
			{
			}

#if NETFRAMEWORK
			protected UserVisibleException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
			{
			}
#endif

			public UserVisibleException(string message, Exception innerException) : base(message, innerException)
			{
			}
		}
		#endregion
	}
}
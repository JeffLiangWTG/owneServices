using System;
using NUnit.Framework;
using ServiceManager.Common.CW;

namespace Enterprise.Semaphores.Common.Testing
{
	class SqlMutexLockReleaseExceptionTest : TestCase
	{
		public void TestDefaultMessage()
		{
			// Arrange
			var exception = new SqlMutexLockReleaseException();

			// Act
			var result = exception.Message;

			// Assert
			AssertEquals("Error during lock release.", result);
			AssertNull(exception.ErrorNumber);
		}

		public void TestDefaultMessageForInternalException()
		{
			CombineAssertions(() =>
			{
				Test<InvalidOperationException>();
				Test<OverflowException>();
				Test<Exception>();
			});

			void Test<T>() where T : Exception, new()
			{
				// Arrange
				var exception = new SqlMutexLockReleaseException(new T());

				// Act
				var result = exception.Message;

				// Assert
				AssertEquals("Error during lock release.", result);
				AssertNull(exception.ErrorNumber);
			}
		}

		public void TestInternalException()
		{
			CombineAssertions(() =>
			{
				Test<InvalidOperationException>();
				Test<OverflowException>();
				Test<Exception>();
			});

			void Test<T>() where T : Exception, new()
			{
				// Arrange
				var exception = new SqlMutexLockReleaseException(new T());

				// Act
				var result = exception.InnerException;

				// Assert
				AssertType<T>(result);
				AssertNull(exception.ErrorNumber);
			}
		}

		public void TestMessageForErrorNumber()
		{
			CombineAssertions(() =>
			{
				Test(1);
				Test(10);
				Test(100);
			});

			void Test(int errorNumber)
			{
				// Arrange
				var exception = new SqlMutexLockReleaseException(errorNumber);

				// Act
				var result = exception.Message;

				// Assert
				AssertContains("Error during lock release.", result);
				AssertContains($"Error number: [{errorNumber}].", result);
				AssertEquals(errorNumber, exception.ErrorNumber);
			}
		}
	}
}

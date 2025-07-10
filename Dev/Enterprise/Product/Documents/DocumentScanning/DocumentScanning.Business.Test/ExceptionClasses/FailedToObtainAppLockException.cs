using System.Collections.Generic;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.ExceptionClasses.Testing
{
	sealed class FailedToObtainAppLockExceptionTest : TestCase
	{
		public void TestDefaultMessage()
		{
			// Arrange
			const string expectedMessage = "Failed to get AppLock. Please try again after other process completes its task.";
			var exception = new FailedToObtainAppLockException();

			// Act
			// Assert
			AssertEquals(expectedMessage, exception.Message);
		}

		public void TestErrorMessage()
		{
			var testList = new List<(string database, LockedProcessResult failedReason)>()
			{
				("NotImportantDb", LockedProcessResult.Error),
				("MyBirthDayIsYesterday", LockedProcessResult.Completed),
				(string.Empty, LockedProcessResult.AlreadyBeingProcessed),
			};

			CombineAssertions(() =>
			{
				testList.ForEach(x => TestErrorMessage(x.database, x.failedReason));
			});

			void TestErrorMessage(string database, LockedProcessResult failedReason)
			{
				// Arrange
				var expectedMessage = $"Failed to get AppLock for {database} with Lock result {failedReason}. Please try again after other process finishes its task.";
				var exception = new FailedToObtainAppLockException(database, failedReason);

				// Act
				// Assert
				AssertEquals(expectedMessage, exception.Message);
			}
		}
	}
}

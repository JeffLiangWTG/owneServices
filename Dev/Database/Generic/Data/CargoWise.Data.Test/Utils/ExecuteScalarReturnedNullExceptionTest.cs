using System;
using NUnit.Framework;

namespace CargoWise.Data.Utils.Testing
{
	public class ExecuteScalarReturnedNullExceptionTest : TestCase
	{
		public void TestExceptionMessage()
		{
			// Arrange
			var exception = new ExecuteScalarReturnedNullException();

			// Act
			// Assert
			AssertEquals("Statement returned a null value", exception.Message);
		}

		public void TestIsCriticalException()
		{
			// Arrange
			var exception = new ExecuteScalarReturnedNullException();

			// Act
			// Assert
			AssertEquals(true, exception.IsCriticalException);
		}

		public void TestIsArgumentException()
		{
			// Arrange
			var exception = new ExecuteScalarReturnedNullException();

			// Act
			// Assert
			AssertEquals(true, exception is ArgumentException);
		}
	}
}

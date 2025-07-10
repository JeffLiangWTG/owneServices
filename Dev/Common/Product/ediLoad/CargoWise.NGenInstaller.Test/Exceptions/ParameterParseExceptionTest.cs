using System;
using CargoWise.NGenInstallerProgram.Exceptions;
using NUnit.Framework;

namespace CargoWise.NGenInstaller.Testing.Exceptions
{
	public class ParameterParseExceptionTest : TestCase
	{
		public void TestMessage()
		{
			// Arrange
			const string TestExMessage = "This is an error message";
			var innerEx = new ApplicationException(TestExMessage);

			// Action
			var ex = new ParameterParseException(innerEx);

			// Assert
			AssertEquals("Failed to parse command line parameters. Should be: CargoWise.NGenInstaller.exe RootFilePath (Install | Uninstall) [ExecutionTimeoutSeconds]", ex.Message);
			AssertNotNull(ex.InnerException);
			AssertType<ApplicationException>(ex.InnerException);
			AssertEquals(TestExMessage, ex.InnerException.Message);
		}
	}
}

using CargoWise.NGenInstallerProgram.Exceptions;
using NUnit.Framework;

namespace CargoWise.NGenInstaller.Testing.Exceptions
{
	public class NGenCallExceptionTest : TestCase
	{
		public void TestMessage_ForOutputError()
		{
			// Arrange
			const string TestArguments = "A 1 B 2";
			const int TestExitCode = -1;
			const string TestOutput = "TestOutput";

			// Action
			var ex = new NGenCallException(TestExitCode, TestArguments, TestOutput);

			// Assert
			AssertEquals($"Process finished with error: Process exit code {TestExitCode} Arguments: \"{TestArguments}\" Output: \"{TestOutput}\"", ex.Message);
		}
	}
}

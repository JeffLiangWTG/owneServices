using Enterprise.Client.EDI.Escrow.Interfaces;
using NUnit.Framework;

namespace ZClientEDI.Test.Escrow
{
	class GitAdapterExceptionTest : TestCase
	{
		public void TestMessage()
		{
			CombineAssertions(() =>
			{
				Test(-1, "A 1 B 2", "TestOutput", "Process finished with error: Process exit code [-1] Arguments: \"A 1 B 2\" Output: \"TestOutput\"");
				Test(1, "A B C", "Test Output", "Process finished with error: Process exit code [1] Arguments: \"A B C\" Output: \"Test Output\"");
			});

			void Test(int exitCode, string arguments, string output, object expectedMessage)
			{
				// Arrange
				var exception = new GitAdapterException(exitCode, arguments, output);

				// Action
				var result = exception.Message;

				// Assert
				AssertEquals(expectedMessage, result);
			}
		}
	}
}

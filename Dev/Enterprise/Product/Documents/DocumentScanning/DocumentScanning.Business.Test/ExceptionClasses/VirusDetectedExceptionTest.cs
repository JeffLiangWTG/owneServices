using Enterprise.DocumentScanning.Integration;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.ExceptionClasses.Testing
{
	sealed class VirusDetectedExceptionTest : TestCase
	{
		public void TestErrorMessage()
		{
			var filename = "test";
			var expectedMessage = "The file has been detected with virus and therefore cannot be saved or opened.";
			var expectedFriendlyMessage = $"The file \"{filename}\" has been detected with virus and therefore cannot be saved or opened.";
			var exception = new VirusDetectedException(filename);

			AssertEquals(expectedMessage, exception.Message);
			AssertEquals(expectedFriendlyMessage, exception.VirusDetectedFriendlyMessage);
		}
	}
}

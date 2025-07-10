using System.IO;
using Enterprise.Environment;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class FileAccessExceptionTest : TestCaseWithDocumentFactory
	{
		public void TestConstructor()
		{
			string filename = Path.Combine(Env.TempPath, TestDocsHelper.TestDocsPath, @"small.gif");
			FileAccessException exception = new FileAccessException("test", filename, null);
			AssertEquals("Filename", filename, exception.Filename);
		}
	}
}

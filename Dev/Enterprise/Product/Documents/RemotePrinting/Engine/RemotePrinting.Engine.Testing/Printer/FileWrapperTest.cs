using System;
using System.IO;
using CargoWise.IO;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	sealed class FileWrapperTest : PrintEngineTestCase
	{
		public void TestConstructor()
		{
			AssertEquals("TestFile", new FileWrapper("TestFile").FullPathAndFilename);
		}

		public void TestConstructor_InvalidArgs()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new FileWrapper(null));
			AssertExceptionThrown<ArgumentException>(() => new FileWrapper(""));
		}

		public void TestDeleteFileToPrint()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var documentXlsPath = resourceRetriever.SaveResourceToFile("Enterprise.RemotePrinting.Engine.Testing.TestFiles.Document.xls", "Document.xls");
				var fileToPrint = CopyFileForTesting(documentXlsPath);
				var fileName = fileToPrint.FullPathAndFilename;
				AssertEquals("Precondition: file should exist", true, File.Exists(fileName));

				fileToPrint.Dispose();
				AssertEquals("File should have been deleted", false, File.Exists(fileName));

				AssertNoExceptionThrown("No problems disposing twice", () => fileToPrint.Dispose());
			}
		}

		public void TestDeleteFileToPrint_DoesNotFailIfExceptionIsThrownOnDelete()
		{
			using (var directory = new TempDirectory())
			{
				var tempFile = Temp.GetTempFileName(directory.DirectoryName, "XLS");

				using (var fileWrapper = new FileWrapper(tempFile))
				{
					Assert("Precondition: file should exist", File.Exists(tempFile));

					using (var stream = File.OpenRead(tempFile))
					{
						AssertNoExceptionThrown(() => fileWrapper.Dispose());
						Assert("File should still exist. Since the Delete would have failed.", File.Exists(tempFile));
					}
				}
			}
		}
	}
}

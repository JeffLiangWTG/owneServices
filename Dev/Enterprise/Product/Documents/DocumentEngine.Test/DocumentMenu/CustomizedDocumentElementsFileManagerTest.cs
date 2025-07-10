using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class CustomizedDocumentElementsFileManagerTest : TestCase
	{
		public void TestGetFilePath()
		{
			var fileManager = new CustomizedDocumentElementsFileManager();
			var filePath = fileManager.GetFilePath();

			var filePathBeforePend = fileManager.GetFilePath();
			AssertEquals("filePath.Equals(filePathBeforePend)", false, filePath.Equals(filePathBeforePend));

			fileManager.PendingChangesForTesting = new string[] { filePath };

			var filePathAfterPend = fileManager.GetFilePath();
			AssertEquals("filePath.Equals(filePathAfterPend)", true, filePath.Equals(filePathAfterPend));
		}
	}
}

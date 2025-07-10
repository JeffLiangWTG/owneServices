using System.IO;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZFileUploadDialogInternalTest : TestCaseWithFactory
	{
		public void TestFileNamesWithInvalidChars()
		{
			var invalidChars = Path.GetInvalidFileNameChars();
			Assert(invalidChars.Length > 0);
			var fileUploadDialog = new ZFileUploadDialogForTest();

			foreach (var invalidChar in Path.GetInvalidFileNameChars())
			{
				fileUploadDialog.UploadedFileNameForTest = $"Some{invalidChar}File.pdf";
				fileUploadDialog.HandleOkButtonClickInternal();
				AssertEquals("Some File.pdf", fileUploadDialog.ProcessedFileNameForTest);
			}
		}
	}
}

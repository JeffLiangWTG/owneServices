using System.Windows.Forms;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentScanning.Module.Testing
{
	internal sealed class DocumentAllocationModuleTest : TestCaseWithDocumentFactory
	{
		public void TestShow()
		{
			using (var module = new DocumentAllocationModule())
			{
				var parent = MasterFactory.New(typeof(StorageMain)) as StorageMain;
				parent.SM_DB = 200; // a ridiculous number that will never be reached...
				MasterFactory.Save();

				var dbHelper = new DocManagerDBHelper();
				var messageText = @"Allocate eDocs has encountered an error due to missing Document Databases.
Please inform your System Administrator that the following document databases are missing:

	- " + dbHelper.GetDatabaseName(0) + @"_SD200

Most likely the main database " + dbHelper.GetDatabaseName(0) + @" has been renamed or copied from elsewhere and has existing document data in it.
To fix the error, rename or copy all other existing databases with the prefix " + dbHelper.GetDatabaseName(0) + @"_SDXXX.

Note: This error will continue to happen on jobs mapped to missing eDocs databases.
However, allocate will still work on new jobs, jobs mapped to existing eDocs and jobs with no previously allocated documents.

To proceed and open the module anyway, click 'OK'.
";

				Assert(!module.IsShownForTestingOnly);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				module.Show();
				Assert("Module not supposed to be shown", !module.IsShownForTestingOnly);
				AssertEquals(messageText, ((UnitTestUserNotification)Globals.Message).LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				module.Show();
				Assert("You said OK, so show the module", module.IsShownForTestingOnly);

				module.CloseFormForTestingOnly();

				parent.Delete();
				MasterFactory.Save();

				module.Show();
				Assert("Module supposed to be shown", module.IsShownForTestingOnly);

				module.CloseFormForTestingOnly();
			}
		}
	}
}

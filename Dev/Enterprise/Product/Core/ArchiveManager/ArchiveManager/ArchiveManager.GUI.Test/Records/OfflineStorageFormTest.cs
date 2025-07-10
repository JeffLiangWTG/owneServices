using CargoWise.EntityFramework.Testing;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.ArchiveManager.GUI.Records;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ArchiveManager.GUI.Test
{
	sealed class OfflineStorageFormTest : TestCaseWithFactory
	{
		public void TestConfirmationPrompt()
		{
			var offlineStorage = new OfflineStorage();
			using (var form = new OfflineStorageForm(offlineStorage))
			{
				form.Show();
				form.Archive();
				AssertEquals("Confirm", UnitTestUserNotification.Instance.LastConfirmationStringShown);
				form.Close();
			}

			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void TestSizeIsNotBound()
		{
			using var form = new OfflineStorageForm();
			AssertEquals("Maximum is not set - should default to size 0", new System.Drawing.Size(0, 0), form.MaximumSize);
			AssertEquals("Minimum is not set - should default to size 0", new System.Drawing.Size(0, 0), form.MinimumSize);
		}
	}
}

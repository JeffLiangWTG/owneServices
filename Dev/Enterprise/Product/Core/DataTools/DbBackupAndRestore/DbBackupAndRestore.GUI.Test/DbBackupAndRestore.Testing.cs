using System.Reflection;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.DataTools.DbBackupAndRestore.Business;
using Enterprise.DataTools.DbBackupAndRestore.GUI;
using Enterprise.DataTools.DbBackupAndRestore.Testing.Restore;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Testing
{
	class DbBackupAndRestoreGUITesting : TestCase
	{
		public void TestEnableRestoreExtraDbsCheckboxes()
		{
			var control = new DbRestoreControl();

			using (var tempDir = new TempDirectory())
			{
				var testLocaMainDBBackUpFilePath = TestDataHelpers.CopyAndNameTestFileResource(tempDir.DirectoryName, "OdysseyNoDescription.bak", "testFBK.bak");

				var dbRestoreFilePathTextBox = control.GetType().GetField("DbRestoreFilePathTextBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(control);
				var dbRestoreFilePathTextBoxText = dbRestoreFilePathTextBox.GetType().GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);
				dbRestoreFilePathTextBoxText.SetValue(dbRestoreFilePathTextBox, testLocaMainDBBackUpFilePath);

				var result = control.GetType().GetMethod("ShouldEnableRestoreExtraDbsCheckboxes", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(control, null);

				AssertEquals("checkboxes should be enabled for FBK Backup files", true, result);
			}
		}

		[ExpectNoExceptions]
		public void TestDbBackupAndRestore_DoesNotCheckSchemaVersion()
		{
			// Arrange
			try
			{
				StartupDirector.Main(new[] { "-h" }); // Setup Environment
				var backupManager = new DbBackupManager();
				var errorLog = string.Empty;
				backupManager.OnTaskFailed += (string message) => errorLog += message;

				// Act
				var dbList = backupManager.GetServerDbList(Db.ServerName);

				// Assert
				AssertNullOrEmpty(errorLog);
				AssertGreaterThan($"There should be databases on {Db.ServerName}. If not, something went wrong.", dbList.Length, 0);
				AssertEquals(true, Db.IsGlobalSchemaVersionCheckDisabled);
			}
			finally
			{
				//StartupDirector disables schema checks so we need to re-enable them
				Db.EnableSchemaVersionCheckPermanently_ForTest();
			}

			AssertEquals(false, Db.IsGlobalSchemaVersionCheckDisabled);
		}

		public void TestGetServerDbListShowWarningWhenServerIsUnreachable()
		{
			// Arrange
			const string serverName = "NotReachableServer";
			const string expectedInfo = "Failed to connect to Sql Server. It might be triggered by incorrect Server value. If problem persists, please contact your administrator.";

			var outputErrorMessage = string.Empty;
			var backupManager = new DbBackupManager();
			backupManager.OnTaskFailed += (errorMessage) => outputErrorMessage = errorMessage;

			// Act
			backupManager.GetServerDbList(serverName);

			// Assert
			AssertContains(expectedInfo, outputErrorMessage);
		}
	}
}

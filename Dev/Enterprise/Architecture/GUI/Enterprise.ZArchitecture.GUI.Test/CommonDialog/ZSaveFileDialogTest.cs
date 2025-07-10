using System.IO;
using CargoWise.Application;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZSaveFileDialogTest : TransactionedTestCase
	{
		public void TestSaveFileUnauthorizedAccess()
		{
			using (var fileToSave = TempFile.New())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				File.SetAttributes(fileToSave.Filename, FileAttributes.ReadOnly);

				using (ZSaveFileDialog.OpenFile(fileToSave.Filename)) { }

				AssertEquals("Should display file access issue message", "Cannot write the file to disk. Please check with your system administrator.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		[DeveloperOnlyTest]
		public void TestSaveFileNotSupportedPath()
		{
			var endPath = @"\\SYDCO-SADS-2\SYDCO-PC454E-MainArea\file.txt"; //SYD main printer
			using (ZSaveFileDialog.OpenFile(endPath)) { }
			AssertEquals("Display that the path cannot be saved to a printer on the network.", "Cannot write the file. The specified path is not supported.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestSaveFileByBatchProcessor()
		{
			ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
			DataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnectorMode = RemoteConnectingModes.ConnectorOnly;
			using (var fileToSave = TempFile.New())
			{
				using (ZSaveFileDialog.OpenFile(fileToSave.Filename)) { }
				AssertEquals("Should display remote connector not installed error message", ZTerminalService.ClientPluginApplicationNotInstalledError, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Globals.IsUserInteractive = false;
				using (ZSaveFileDialog.OpenFile(fileToSave.Filename)) { }
				Assert("Should not display remote connector not installed error message", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
			}
		}
	}
}

using System;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Script.Test;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class UpgFormTest : TransactionedTestCase
	{
		[ExpectNoExceptions()]
		public void TestUpgFormDoesNotCrashOnEventHandlers()
		{
			using (var form = new UpgForm(new UpgradeManagerForTest()))
			{
				form.CloseButton.TextChanged += new EventHandler(CloseButton_TextChanged);
				form.ShowDialog();
			}
		}

		void CloseButton_TextChanged(object sender, EventArgs e)
		{
			Button closeButton = (Button)sender;
			if (closeButton.Text == "Close")
			{
				closeButton.FindForm().Close();
			}
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestUpgFormUsesDisposableActionForDbConnection()
		{
			using (var conn = Db.NewAdminConnection())
			{
				var testScriptUpgrader = new ScriptUpgraderForTesting(conn);
				var testScriptUpgradeManager = new AlwaysRequiredScriptUpgradeManagerForTesting(testScriptUpgrader);

				using (var form = new UpgForm(testScriptUpgradeManager))
				{
					form.CloseButton.TextChanged += new EventHandler(CloseButton_TextChanged);
					conn.ThreadSentry.RelinquishThreadOwnership();
					form.ShowDialog();
				}

				Thread.Sleep(1000);
				conn.ThreadSentry.TakeThreadOwnership();
			}
		}

		[GuiTest]
		[DeveloperOnlyTest]
		public void TestCopyLogs()
		{
			using (var form = new UpgForm(new UpgradeManagerForTest()))
			{
				SafeClipboard.SetText("PreviousClipboardText");
				form.CallCopyLogsButtonClick();
				AssertEquals("[PRE-CONDITION] Log Contents", "PreviousClipboardText", GetClipboardWithRetry());

				form.SetCompletedTasksRichTextBoxContents("SomeLog");
				form.CallCopyLogsButtonClick();
				AssertEquals("Log Contents", "SomeLog", GetClipboardWithRetry());

				form.SetCompletedTasksRichTextBoxContents("LogLine1\r\nLogLine2");
				form.CallCopyLogsButtonClick();
				AssertEquals("Log Contents", "LogLine1\r\nLogLine2", GetClipboardWithRetry());
			}
		}

		string GetClipboardWithRetry()
		{
			string result = null;

			for (int i = 0; i < 10; i++)
			{
				result = SafeClipboard.GetText();

				if (!string.IsNullOrEmpty(result))
				{
					break;
				}
			}

			return result;
		}

		class UpgradeManagerForTest : BaseUpgradeManager
		{
			public override bool IsHosted => false;

			public override ValidationResponse Run()
			{
				var result = new ValidationResponse();

				((IUpgradeManager)this).ActivateTaskProgress(20);
				for (int i = 0; i < 20; i++)
				{
					((IUpgradeManager)this).StartTask("Task " + i);
					((IUpgradeManager)this).ActivateSubtaskProgress(20);
					for (int j = 0; j < 20; j++)
					{
						((IUpgradeManager)this).StartSubtask("SubTask " + j);
						((IUpgradeManager)this).ShowInfoMessage("Info " + i + "," + j);
					}
				}
				result.Successful = true;
				return result;
			}

			public override VersionLabel SchemaVersionBeforeUpgrade
			{
				get { throw new NotImplementedException(); }
			}

			public override VersionLabel TransformationVersionBeforeUpgrade
			{
				get { throw new NotImplementedException(); }
			}

			protected override bool GetConfirmationIfUserAttended(string title, string message, string[] detailLines)
			{
				return false;
			}
		}
	}
}

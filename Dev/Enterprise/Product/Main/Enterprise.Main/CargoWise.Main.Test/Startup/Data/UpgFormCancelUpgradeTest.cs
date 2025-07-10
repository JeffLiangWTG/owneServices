using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class UpgFormCancelUpgradeTest : TestCase
	{
		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestCancelUpgrade_ExceptionsAreHandled()
		{
			// Arrange
			var upgrader = new UpgradeManagerForTest();

			var upgradeFormMock = new Mock<UpgForm>(upgrader) { CallBase = true };

			var textBoxMessages = new ConcurrentBag<string>();
			upgradeFormMock.Setup(x => x.AppendMessageToTextBox(It.IsAny<string>()))
				.Callback<string>(x => textBoxMessages.Add(x));
			upgradeFormMock.Setup(x => x.KillConnections())
				.Throws(new TimeoutException(DbConnectionKiller.TimeoutExceptionMessagePrefix));
			upgradeFormMock
				.Setup(x => x.GetUserConfirmationToCancelUpgradeProcess())
				.Returns(DialogResult.Yes);

			var upgradeProgressMessages = new ConcurrentBag<string>();
			upgrader.UpgradeEvent += (type, message) => upgradeProgressMessages.Add(message);

			using (var form = upgradeFormMock.Object)
			{
				form.CloseButton.TextChanged += new EventHandler(CloseButton_TextChanged);

				upgrader.UpgradeEvent += (_, message) =>
				{
					if (message == "Task 1")
					{
						form.CancelUpgrade_Exposed();
					}
				};

				form.ShowDialog();
			}

			// Assert
			CombineAssertions(() =>
			{
				var allTextBoxMessages = string.Join(System.Environment.NewLine, textBoxMessages);
				AssertNotContains("Timeout is not added to textbox", DbConnectionKiller.TimeoutExceptionMessagePrefix, allTextBoxMessages);
				var progressMessages = string.Join(System.Environment.NewLine, upgradeProgressMessages);
				AssertNotContains("Timeout is not logged", DbConnectionKiller.TimeoutExceptionMessagePrefix, progressMessages);
			});
		}

		public void TestCancelUpgrade()
		{
			var (hostName, hostProcessId) = DbConnectionKiller.GetCurrentProcessInfo_Exposed();
			var upgradeConnectionsBeforeCancel = 0;
			var upgradeConnectionsAfterCancel = 0;

			var upgrader = new UpgradeManagerForTest();
			using (var form = new UpgForm(upgrader))
			{
				form.CloseButton.TextChanged += new EventHandler(CloseButton_TextChanged);

				upgrader.UpgradeEvent += (_, message) =>
				{
					if (message == "Task 1")
					{
						Thread.Sleep(100);
						upgradeConnectionsBeforeCancel = DbConnectionKiller.GetConnectionsCount_ForTest(hostName, hostProcessId);

						form.CancelUpgrade_Exposed();

						upgradeConnectionsAfterCancel = DbConnectionKiller.GetConnectionsCount_ForTest(hostName, hostProcessId);
					}
				};

				form.ShowDialog();
			}

			AssertGreaterThanOrEqualTo("Precondition", upgradeConnectionsBeforeCancel, 1);
			AssertEquals("All process connections have been killed", 0, upgradeConnectionsAfterCancel);
		}

		#region Implementation

		void CloseButton_TextChanged(object sender, EventArgs e)
		{
			var closeButton = (Button)sender;
			if (closeButton.Text == "Close")
			{
				closeButton.FindForm().Close();
			}
		}

		#endregion // Implementation

		#region Helper classes

		class UpgradeManagerForTest : BaseUpgradeManager
		{
			public override bool IsHosted => false;

			public override ValidationResponse Run()
			{
				var result = new ValidationResponse();

				try
				{
					((IUpgradeManager)this).ActivateTaskProgress(1);
					using (var connection = Db.NewAdminConnection())
					{
						((IUpgradeManager)this).StartTask("Task 1");
						connection.ExecuteNonQuery("WAITFOR DELAY '00:00:10';");
					}

					result.Successful = true;
				}
				catch
				{
					result.Successful = false;
				}

				return result;
			}

			public override VersionLabel SchemaVersionBeforeUpgrade => throw new NotImplementedException();
			public override VersionLabel TransformationVersionBeforeUpgrade => throw new NotImplementedException();
			protected override bool GetConfirmationIfUserAttended(string title, string message, string[] detailLines) => false;
		}

		#endregion // Helper classes
	}
}

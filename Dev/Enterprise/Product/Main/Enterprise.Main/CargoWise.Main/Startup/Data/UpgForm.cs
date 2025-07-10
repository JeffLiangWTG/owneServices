using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	public partial class UpgForm : Form
	{
		#region WinForm Stuff

#if DEBUG
		public
#endif
			UpgForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.Icon = BrandingFactory.Instance.ProductIcon;
		}

		public UpgForm(BaseUpgradeManager upgrader)
			: this()
		{
			this.TheUpgrader = upgrader;

			upgrader.SyncInvoke = this;
			upgrader.OnTasksEstimated += new UpgraderNumOfStepsEstimatedEvent(ResetTaskBar);
			upgrader.OnSubtasksEstimated += new UpgraderNumOfStepsEstimatedEvent(ResetSubtaskBar);
			upgrader.OnIncrementNumberOfTasks += new UpgraderNumOfStepsEstimatedEvent(IncrementTaskBarMaximumTasks);
			upgrader.OnUpdateCurrentProgress += new UpgraderNumOfStepsEstimatedEvent(IncreaseTaskBarValue);
			upgrader.UpgradeEvent += Upgrader_UpgradeEvent;
			upgrader.OnErrorWithRetry += new UpgraderWithRetryEvent(OnErrorWithRetry);

			this.Text += String.Format((NoResString)" - Server: {0} - Database: {1}", Db.Connection.ServerNameReportedByDatabase, Db.DatabaseName);
		}

		public bool UpgradeSucceeded;

		#endregion

		#region Implementation

		delegate void DeferredStartDelegate();
		readonly BaseUpgradeManager TheUpgrader;
		Thread UpgraderThread;
		bool UpgradeCompleted;
		bool UpgradeCancelled;

		#region Properties

		int TaskMaximum
		{
			get { return TaskProgressBar.Maximum; }
			set
			{
				if (value > 0)
				{
					TaskLabel.Visible = true;
					TaskProgressBar.Visible = true;
				}
				else
				{
					TaskLabel.Visible = false;
					TaskProgressBar.Visible = false;
				}
				TaskProgressBar.Maximum = value;
			}
		}

		int SubtaskMaximum
		{
			set
			{
				SubtaskProgressBar.Maximum = value;
				if (value > 0)
				{
					SubtaskLabel.Visible = true;
					SubtaskProgressBar.Visible = true;
				}
				else
				{
					SubtaskLabel.Visible = false;
					SubtaskProgressBar.Visible = false;
				}
			}
		}

		string TaskMessage
		{
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					AppendMessageToTextBox(" " + value);
				}
				TaskLabel.Text = value;
			}
		}

		string SubtaskMessage
		{
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					AppendMessageToTextBox("\t" + value);
				}
				SubtaskLabel.Text = value;
			}
		}

		#endregion

		/// <summary>
		/// Starts the Upgrade process
		/// Note: This method call is queued as to run after the form is completely loaded.
		/// </summary>
		void DeferredStartUpgrade()
		{
			UpgraderThread = new Thread(new ThreadStart(OnUpgraderThreadStart));
			UpgraderThread.Start();
		}

		void OnUpgraderThreadStart()
		{
			using (Db.DisposableActionForDbConnection())
			{
				UpgradeSucceeded = TheUpgrader.Run().Successful;
			}

			// Don't do it on the thread!
			Invoke(new ThreadStart(FinaliseUpgrade));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Suppressed due to known issue with leading/trailing whitespace in resource string.")]
		void FinaliseUpgrade()
		{
			UpgradeCompleted = true;
			this.CloseButton.Text = Res.GetString("341526dd-ebef-41db-b120-15cf00ff0e1b", "Close");

			if (UpgradeSucceeded)
			{
				string successMessage = Res.GetString("5be8310f-7864-44e8-bb9a-2b1345c7abf8", "DATABASE UPDATED SUCCESSFULLY");
				this.WarningLabel.Text = successMessage;
				this.WarningLabel.ForeColor = Color.Green;
				this.TaskProgressBar.Value = this.TaskProgressBar.Maximum;

				AppendMessageToTextBox("\r\n\r\n\t" + successMessage);
			}
			else if (UpgradeCancelled)
			{
				this.WarningLabel.Text = Res.GetString("5f49d4aa-da2c-4fc4-9b42-e49eaabe6466", "DATABASE UPGRADE CANCELED");
				this.WarningLabel.ForeColor = this.CompletedTasksRichTextBox.ForeColor = Color.DarkGray;
				AppendMessageToTextBox("\r\n\r\n\tDATABASE UPGRADE WAS CANCELED");
			}
			else
			{
				string failureMessage = Res.GetString("6594f1e8-e839-4109-b34e-8fc9f44158d5", "DATABASE UPGRADE FAILED");
				this.WarningLabel.Text = failureMessage;
				this.CompletedTasksRichTextBox.ForeColor = Color.Red;

				AppendMessageToTextBox("\r\n\r\n\tDATABASE UPGRADE FAILED - THE SOFTWARE UPGRADE WILL BE ABORTED.\r\n\t Please raise an eRequest from within your " + Core.Constants.ProductName + " system, click on Help > eRequest/Incident. \r\n\t Please copy and paste the upgrade log text into the incident for analysis.");
			}

			OnFinaliseUpgradeCompleted_ForTest();
		}

		partial void OnFinaliseUpgradeCompleted_ForTest();

#if DEBUG
		internal virtual
#endif
		void AppendMessageToTextBox(string message)
		{
			// Cannot hit the DB here. Multi-threading causes SqlCommand is busy fetching error (SARS).
			string timeStamp = DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]");

			CompletedTasksRichTextBox.AppendText(
				timeStamp +
				((message.Trim() == ".") ? "" : message) +
				System.Environment.NewLine);

			CompletedTasksRichTextBox.ScrollToCaret();
		}

		#endregion

		#region Event Handlers

		#region Form Events

		/// <summary>
		/// Queue the call to DeferredStartUpgrade to run after the form is completely loaded.
		/// </summary>
		void UpgForm_Load(object sender, EventArgs e)
		{
			this.BeginInvoke(new DeferredStartDelegate(DeferredStartUpgrade));
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (!UpgradeCompleted && UpgraderThread != null)
			{
				this.CloseButton.Enabled = false;
				if (GetUserConfirmationToCancelUpgradeProcess() == DialogResult.Yes)
				{
					CancelUpgrade();
				}

				e.Cancel = true;
				this.CloseButton.Enabled = true;
			}

			base.OnClosing(e);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Globals.Message might hit the database, avoid this because we may still be in the db upgrade transaction")]
#if DEBUG
		internal virtual
#endif
		DialogResult GetUserConfirmationToCancelUpgradeProcess()
		{
			return MessageBox.Show(
				this,
				(NoResString)"Do you want to cancel the upgrade process?\r\nIt may take a while to rollback changes already made to the database, during which time the system may not be usable.",
				(NoResString)"Cancel?",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button2);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Suppressed due to known issue with leading/trailing whitespace in resource string.")]
		void CancelUpgrade()
		{
			if (UpgradeCompleted || UpgraderThread == null)
			{
				return;
			}

			try
			{
				this.OnShowInfoMessage("\r\n\r\nCanceling upgrade process...\r\n\r\n");
				KillConnections();
				this.OnShowInfoMessage(Res.GetString("5afe1a15-ec09-4cf0-bcc0-2f97db3112a1", "SPID killed"));
				UpgradeCancelled = true;
			}
			catch (TimeoutException ex) when (ex.Message.StartsWith(DbConnectionKiller.TimeoutExceptionMessagePrefix))
			{
				// ignored
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				this.OnShowInfoMessage(ex.Message);
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void CopyLogsButton_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(CompletedTasksRichTextBox.Text))
			{
				SafeClipboard.SetText(CompletedTasksRichTextBox.Text);
			}
		}

		internal protected virtual void KillConnections()
		{
			var writeLog = true;
			var timer = Stopwatch.StartNew();
			DbConnectionKiller.KillAllConnectionsOfTheCurrentProcess(feedbackMethod: (message) =>
			{
				if (writeLog)
				{
					this.OnShowInfoMessage(message);
					timer.Restart();
				}

				writeLog = timer.Elapsed >= TimeSpan.FromMinutes(1);
			});
		}

		#endregion

		#region Upgrader Events

		void ResetTaskBar(int max)
		{
			ResetSubtaskBar(0);
			TaskMessage = "";
			TaskProgressBar.Value = 0;
			TaskMaximum = max;
			TaskProgressBar.Step = 1;
		}

		void ResetSubtaskBar(int max)
		{
			SubtaskMessage = "";
			SubtaskProgressBar.Value = 0;
			SubtaskMaximum = max;
			SubtaskProgressBar.Step = 1;
		}

		void IncreaseTaskBarValue(int newValue)
		{
			if (newValue > TaskProgressBar.Value && newValue <= TaskProgressBar.Maximum)
			{
				TaskProgressBar.Value = newValue;
			}
		}

		void IncrementTaskBarMaximumTasks(int tasksToAdd)
		{
			TaskMaximum = TaskMaximum + tasksToAdd;
		}

		void Upgrader_UpgradeEvent(UpgradeEventType eventType, string message)
		{
			switch (eventType)
			{
				case UpgradeEventType.TaskStarted:
					OnTaskStarted(message);
					break;
				case UpgradeEventType.SubtaskStarted:
					OnSubtaskStarted(message);
					break;
				case UpgradeEventType.InfoMessage:
					OnShowInfoMessage(message);
					break;
				case UpgradeEventType.TaskFailed:
					OnTaskFailed(message);
					break;
			}
		}

		void OnTaskStarted(string task)
		{
			ResetSubtaskBar(0);
			TaskMessage = task;
			TaskProgressBar.PerformStep();
			this.Update();
		}

		void OnSubtaskStarted(string subtask)
		{
			SubtaskMessage = subtask;
			SubtaskProgressBar.PerformStep();
			this.Update();
		}

		void OnShowInfoMessage(string message)
		{
			AppendMessageToTextBox("\t" + message);
		}

		void OnTaskFailed(string message)
		{
			AppendMessageToTextBox(System.Environment.NewLine + System.Environment.NewLine + message + System.Environment.NewLine);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Globals.Message might hit the database, avoid this because we may still be in the db upgrade transaction")]
		bool OnErrorWithRetry(string message)
		{
			AppendMessageToTextBox(System.Environment.NewLine + System.Environment.NewLine + message + System.Environment.NewLine);
			return MessageBox.Show(this, message, this.Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry;
		}

		#endregion

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.Startup
{
	#region Partial class

	public partial class UpgForm
	{
		internal void CancelUpgrade_Exposed()
		{
			CancelUpgrade();
		}

		internal void CallCopyLogsButtonClick() => CopyLogsButton_Click(null, null);
		internal void SetCompletedTasksRichTextBoxContents(string contents) => CompletedTasksRichTextBox.Text = contents;

		partial void OnFinaliseUpgradeCompleted_ForTest()
		{
			OnFinaliseUpgradeCompleted?.Invoke(WarningLabel.Text);
		}

		internal Action<string> OnFinaliseUpgradeCompleted;
	}

	#endregion // Partial class

}

#endif
#endregion

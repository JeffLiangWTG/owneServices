using System;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.DataTools.DbBackupAndRestore.Business;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	public interface IDialogService
	{
		DialogResult ShowMessageBox(string text, string caption, MessageBoxButtons buttons);
	}

	public class MessageBoxHelper : IDialogService
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Cannot call globals as it is not in CargoWiseOne, this is dbbackupandrestore tool which is external.")]
		public DialogResult ShowMessageBox(string text, string caption, MessageBoxButtons buttons)
		{
			return MessageBox.Show(text, caption, buttons);
		}
	}

	public partial class DBMaintenanceControl : TaskPageControl
	{
		readonly IDialogService dialogService;

		public DBMaintenanceControl(IDialogService dialogService)
		{
			InitializeComponent();
			InitialiseMaintenanceManager();
			this.dialogService = dialogService;
		}

		void InitialiseMaintenanceManager()
		{
			MaintenanceManager = new DbMaintenanceManager();
			MaintenanceManager.SyncInvoke = this;
			MaintenanceManager.OnTaskStarted += DbTools_OnTaskStarted;
			MaintenanceManager.OnSubtaskStarted += DbTools_OnSubtaskStarted;
			MaintenanceManager.OnTaskCompleted += DbTools_OnTaskCompleted;
			MaintenanceManager.OnTaskFailed += DbTools_OnTaskFailed;
			MaintenanceManager.OnPromtForReleaseKey = MaintenanceManager_OnPromtForReleaseKey;
		}

		protected override void OnLoad(EventArgs e)
		{
			InitializeDefaultValues();
		}

		void InitializeDefaultValues()
		{
			DbMaintenanceServerTextBox.Text = Defaults.Instance.ServerName;
			if (!string.IsNullOrEmpty(Defaults.Instance.ServerName))
			{
				try
				{
					PopulateDatabaseListBox();
					MaintenanceDatabaseComboBox.SelectedItem = Defaults.Instance.DatabaseName;
				}
				catch (Exception e) when (!e.IsCriticalException())
				{ }
			}
		}

		#region Drop Database

		void SetDropButtonAccessability()
		{
			DbDropButton.Enabled =
				   !string.IsNullOrEmpty(DbMaintenanceServerTextBox.Text)
				&& MaintenanceDatabaseComboBox.SelectedIndex >= 0;
		}

		void DropDatabases()
		{
			DropServerNameThreadingBuffer = DbMaintenanceServerTextBox.Text;
			DropDatabaseNameThreadingBuffer = MaintenanceDatabaseComboBox.Text;
			IncludeOperationalDatabasesThreadingBuffer = IncludeOperationalDatabasesCheckBox.Checked;
			IncludeRefFilesDatabasesThreadingBuffer = IncludeReferenceFilesDatabasesCheckBox.Checked;
			IncludeBiDatabasesThreadingBuffer = IncludeBiDatabasesCheckBox.Checked;

			StartTaskOnASeparateThread(OnDropDatabasesTaskThreadStart);
		}

		void OnDropDatabasesTaskThreadStart()
		{
			try
			{
				MaintenanceManager.DropDatabases(
					DropServerNameThreadingBuffer,
					DropDatabaseNameThreadingBuffer,
					IncludeOperationalDatabasesThreadingBuffer,
					IncludeRefFilesDatabasesThreadingBuffer,
					IncludeBiDatabasesThreadingBuffer);

				BeginInvoke(() => CompletedDropDB());
			}
			finally
			{
				Invoke(new ThreadStart(OnTaskThreadFinished));
			}
		}

		void MaintenanceDatabaseComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetDropButtonAccessability();
		}

		void PopulateDatabaseListBox()
		{
			MaintenanceDatabaseComboBox.Items.Clear();

			var dbList = MaintenanceManager.GetServerDbList(DbMaintenanceServerTextBox.Text, true);

			if (dbList.Length > 0)
			{
				MaintenanceDatabaseComboBox.Items.AddRange(dbList);
				MaintenanceDatabaseComboBox.Enabled = true;
			}
		}

		void CompletedDropDB()
		{
			PopulateDatabaseListBox();
			SetDropButtonAccessability();
		}

		void DisableDatabaseComboBox()
		{
			MaintenanceDatabaseComboBox.Enabled = false;
			MaintenanceDatabaseComboBox.Items.Clear();
		}

		void EnableOrDisableServerTextBoxDependentButtons()
		{
			var enableStatus = !string.IsNullOrEmpty(DbMaintenanceServerTextBox.Text);

			RefreshDatabasesButton.Enabled = enableStatus;
		}

		DbMaintenanceManager MaintenanceManager;
		string DropServerNameThreadingBuffer;
		string DropDatabaseNameThreadingBuffer;
		bool IncludeOperationalDatabasesThreadingBuffer;
		bool IncludeRefFilesDatabasesThreadingBuffer;
		bool IncludeBiDatabasesThreadingBuffer;

		#endregion

		#region Event Handlers

		void DbMaintenanceServerTextBox_TextChanged(object sender, EventArgs e)
		{
			SetDropButtonAccessability();

			DisableDatabaseComboBox();
			EnableOrDisableServerTextBoxDependentButtons();
		}

		void RefreshDatabasesButton_Click(object sender, EventArgs e)
		{
			Parent.Parent.Enabled = false;

			try
			{
				PopulateDatabaseListBox();
			}
			finally
			{
				Parent.Parent.Enabled = true;
			}
		}

		void IncludeOperationalDatabasesCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			IncludeReferenceFilesDatabasesCheckBox.Checked &= IncludeOperationalDatabasesCheckBox.Checked;
		}

		void IncludeReferenceFilesDatabasesCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			IncludeOperationalDatabasesCheckBox.Checked |= IncludeReferenceFilesDatabasesCheckBox.Checked;
		}

		void MaintenanceManager_OnPromtForReleaseKey(SessionInfo sessionInfo)
		{
			var removeForm = new DbOverwriteReleaseForm(sessionInfo);
			removeForm.ShowDialog();
		}

		void DbDropButton_Click(object sender, EventArgs e)
		{
			var confirmResult = dialogService.ShowMessageBox("Are you sure you want to delete this database?",
						 "Confirm Delete",
						 MessageBoxButtons.YesNo);
			if (confirmResult == DialogResult.Yes)
			{
				DropDatabases();
			}
		}

		void DbMaintenanceServerTextBox_Leave(object sender, EventArgs e)
		{
			DbMaintenanceServerTextBox.Text = Utilities.GetLocalMachineNameForLocalhost(DbMaintenanceServerTextBox.Text);
		}

#if DEBUG
		public void DbDropButton_ClickForTest()
		{
			DbDropButton_Click(null, null);
		}

#endif

		#endregion
	}
}

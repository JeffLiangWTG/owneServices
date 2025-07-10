using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Enterprise.DataTools.DbBackupAndRestore.Business;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	public partial class DbSecurityControl : TaskPageControl
	{
		public DbSecurityControl()
		{
			InitializeComponent();
			InitialiseListControls();
			InitialiseLoginCreator();
			InitializeDefaultValues();
		}

		void InitialiseListControls()
		{
			this.DbSecurityActionComboBox.SelectedIndex = (int)DbSecurityActionEnum.Undefined;
		}

		void InitializeDefaultValues()
		{
			DbSecurityServerTextBox.Text = Defaults.Instance.ServerName;
		}

		#region Security Procedures

		DbSecurityManager securityManager;
		string dbServerThreadingBuffer;
		string dbNameThreadingBuffer;
		string sysadminNewPassword;

		void InitialiseLoginCreator()
		{
			if (securityManager == null)
			{
				DbSecurityManager tempSecurityManager = new DbSecurityManager();
				tempSecurityManager.SyncInvoke = this;
				tempSecurityManager.OnTaskStarted += new InformationEvent(DbTools_OnTaskStarted);
				tempSecurityManager.OnSubtaskStarted += new ProgressEvent(DbTools_OnSubtaskStarted);
				tempSecurityManager.OnTaskCompleted += new InformationEvent(DbTools_OnTaskCompleted);
				tempSecurityManager.OnTaskFailed += new InformationEvent(DbTools_OnTaskFailed);

				securityManager = tempSecurityManager;
			}
		}

		DbSecurityManager.DatabaseEntryCollection dbCollection = new DbSecurityManager.DatabaseEntryCollection();
		void PopulateDatabaseListBox()
		{
			OutputTextBox.Text = "";
			DbSecurityDatabaseComboBox.Items.Clear();

			dbCollection = securityManager.GetServerMainDbList(DbSecurityServerTextBox.Text, SaValueToUse);

			if (dbCollection.Count > 0)
			{
				DbSecurityDatabaseComboBox.Items.AddRange(dbCollection.GetDatabaseList());
				DbSecurityDatabaseComboBox.Enabled = true;
			}
		}

		void ResetCW1Sysadmin()
		{
			dbServerThreadingBuffer = DbSecurityServerTextBox.Text;
			dbNameThreadingBuffer = DbSecurityDatabaseComboBox.Text;
			sysadminNewPassword = SysadminNewPasswordTextBox.Text;
			StartTaskOnASeparateThread(OnResetCW1SysadminTaskThreadStart);
		}

		void ResetAllStaffLocalPassword()
		{
			dbServerThreadingBuffer = DbSecurityServerTextBox.Text;
			dbNameThreadingBuffer = DbSecurityDatabaseComboBox.Text;
			StartTaskOnASeparateThread(OnResetAllStaffLocalPasswordTaskThreadStart);
		}

		void OnResetCW1SysadminTaskThreadStart()
		{
			try
			{
				securityManager.ResetCW1Sysadmin(dbServerThreadingBuffer, dbNameThreadingBuffer, sysadminNewPassword);
			}
			finally
			{
				Invoke(new ThreadStart(OnTaskThreadFinished));
			}
		}

		void OnResetAllStaffLocalPasswordTaskThreadStart()
		{
			try
			{
				securityManager.ResetAllStaffLocalPassword(dbServerThreadingBuffer, dbNameThreadingBuffer);
			}
			finally
			{
				Invoke(new ThreadStart(OnTaskThreadFinished));
			}
		}

		string SaValueToUse
		{
			get { return (UseClientsOwnSaPwdCheckBox.Checked) ? SaPasswordTextBox.Text : null; }
		}

		#endregion

		#region Event Handlers

		void UseClientsOwnSaPwdCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (UseClientsOwnSaPwdCheckBox.Checked)
			{
				SaPasswordTextBox.Text = "(please type 'sa' password here)";
				SaPasswordTextBox.UseSystemPasswordChar = false;
				SaPasswordTextBox.Enabled = true;
				SaPasswordTextBox.Focus();
				SaPasswordTextBox.SelectAll();
			}
			else
			{
				SaPasswordTextBox.Enabled = false;
				SaPasswordTextBox.Text = "";
			}
		}

		void DBScecurityConfigServerTextBox_TextChanged(object sender, EventArgs e)
		{
			SetExecuteActionButtonAccessability();
		}

		void DbSecurityDatabaseComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetExecuteActionButtonAccessability();
		}

		void DbSecurityActionComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			SetExecuteActionButtonAccessability();
			ResetSysadminGroupBox.Visible = (DbSecurityActionEnum)DbSecurityActionComboBox.SelectedIndex == DbSecurityActionEnum.ResetSysadmin;
		}

		void SetExecuteActionButtonAccessability()
		{
			bool serverNameTextBoxEmpty = string.IsNullOrWhiteSpace(DbSecurityServerTextBox.Text);
			bool dbNameTextBoxEmpty = string.IsNullOrWhiteSpace(DbSecurityDatabaseComboBox.Text);

			ExecuteActionButton.Enabled = !serverNameTextBoxEmpty && !dbNameTextBoxEmpty && DbSecurityActionComboBox.SelectedIndex > 0;
			RefreshDatabasesButton.Enabled = !serverNameTextBoxEmpty;
		}

		void ExecuteActionButton_Click(object sender, EventArgs e)
		{
			if (ConfirmExecution())
			{
				switch ((DbSecurityActionEnum)DbSecurityActionComboBox.SelectedIndex)
				{
					case DbSecurityActionEnum.ResetSysadmin:
						ResetCW1Sysadmin();
						break;
					case DbSecurityActionEnum.ResetAllStaffLocalPassword:
						ResetAllStaffLocalPassword();
						break;
					default:
						PrintOutput("Please select an Action then click Execute Action !");
						break;
				}
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
		[SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Cannot call Globals as it is not in CargoWiseOne, this is dbbackupandrestore tool which is external.")]
		bool ConfirmExecution()
		{
			var question = string.Format(CultureInfo.InvariantCulture, "You are about to perform the following action:\r\n{0}\r\n\r\nAre you sure to continue?", DbSecurityActionComboBox.Text);
			var caption = "Confirm Execution";
			return MessageBox.Show(question, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK;
		}

		void RefreshDatabasesButton_Click(object sender, EventArgs e)
		{
			PopulateDatabaseListBox();
		}

		void SaPasswordTextBox_TextChanged(object sender, EventArgs e)
		{
			if (!SaPasswordTextBox.UseSystemPasswordChar)
			{
				SaPasswordTextBox.UseSystemPasswordChar = true;
			}
		}

		void DbSecurityServerTextBox_Leave(object sender, EventArgs e)
		{
			DbSecurityServerTextBox.Text = Utilities.GetLocalMachineNameForLocalhost(DbSecurityServerTextBox.Text);
		}

		#endregion
	}
}

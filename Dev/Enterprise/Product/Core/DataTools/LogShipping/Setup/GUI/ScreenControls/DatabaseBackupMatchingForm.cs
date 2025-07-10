namespace Enterprise.LogShipping.Setup.GUI
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Windows.Forms;
	using CargoWise.Common;

	public partial class DatabaseBackupMatchingForm : Form
	{
		public DatabaseBackupMatchingForm(LogShippingInfo setupInfo, Dictionary<string, List<string>> listOfBackups)
		{
			Argument.NotNull(setupInfo, nameof(setupInfo));

			InitializeComponent();
			InitializeDataDirectoryOverrides(setupInfo);
			InitializeDataGridView(setupInfo);
			this.listOfBackups = listOfBackups;
		}

#if DEBUG
		public DatabaseBackupMatchingForm()
		{
			InitializeComponent();
		}
#endif // DEBUG

		readonly Dictionary<string, List<string>> listOfBackups;

		void InitializeDataGridView(LogShippingInfo setupInfo)
		{
			Argument.NotNull(setupInfo, nameof(setupInfo));

			dbMatchingDataGridView.AutoGenerateColumns = false;
			dbMatchingDataGridView.DataSource = setupInfo.GetDatabaseInfoListToProcess();
			ShouldInitialise.Visible = setupInfo.SetupAction == SetupAction.Change;
		}

		void InitializeDataDirectoryOverrides(LogShippingInfo setupInfo)
		{
			Argument.NotNull(setupInfo, nameof(setupInfo));

			dataDirectoryOverrideTextBox.Text = setupInfo.RestoreDataDirectoryOverride;
			logDirectoryOverrideTextBox.Text = setupInfo.RestoreLogDirectoryOverride;
		}

		public string RestoreDataDirectoryOverride { get { return dataDirectoryOverrideTextBox.Text; } }
		public string RestoreLogDirectoryOverride { get { return logDirectoryOverrideTextBox.Text; } }

		#region Event Handlers

		void dbMatchingDataGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
		{
			if (dbMatchingDataGridView.Rows.Count > e.RowIndex)
			{
				var row = dbMatchingDataGridView.Rows[e.RowIndex];

				var bkpFileCell = row.Cells[BackupFilename.Name] as DataGridViewComboBoxCell;

				var dbCell = row.Cells[DatabaseName.Name] as DataGridViewTextBoxCell;

				bkpFileCell.Items.Clear();

				List<string> backups;
				string databaseName = dbCell.Value as string;

				if (databaseName != null && listOfBackups != null && listOfBackups.TryGetValue(databaseName, out backups))
				{
					if (backups != null)
					{
						bkpFileCell.Items.AddRange(backups.ToArray());
					}
				}
			}
		}

		void dbMatchingDataGridView_CellValidated(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == 1 && dbMatchingDataGridView.Rows.Count > e.RowIndex)
			{
				var row = dbMatchingDataGridView.Rows[e.RowIndex];

				ValidateBackupFilename(row);
			}
		}

		void dataDirectoryOverrideBrowseButton_Click(object sender, EventArgs e)
		{
			OpenFolderBrowserAndSetValue(dataDirectoryOverrideTextBox);
		}

		void logDirectoryOverrideBrowseButton_Click(object sender, EventArgs e)
		{
			OpenFolderBrowserAndSetValue(logDirectoryOverrideTextBox);
		}

		void dataDirectoryOverrideTextBox_Validating(object sender, CancelEventArgs e)
		{
			Argument.NotNull(sender, nameof(sender));

			ValidateLocalDirectory(((TextBox)sender));
		}

		void logDirectoryOverrideTextBox_Validating(object sender, CancelEventArgs e)
		{
			Argument.NotNull(sender, nameof(sender));

			ValidateLocalDirectory(((TextBox)sender));
		}

		void dbMatchingDataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
		}

		#endregion

		#region Overrides

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Cannot use ZArch because it is an external tool")]
		protected override void OnClosing(CancelEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				if (!ValidateAll() && e != null)
				{
					MessageBox.Show(this, "Please correct the errors", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					e.Cancel = true;
				}
			}

			base.OnClosing(e);
		}

		#endregion

		#region Validation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		bool ValidateBackupFilename(DataGridViewRow row)
		{
			Argument.NotNull(row, nameof(row));

			bool result = true;

			if (BackupFilename.Name != null && row.Cells[BackupFilename.Name] != null)
			{
				if (row.Cells[ShouldInitialise.Name] != null &&
					row.Cells[ShouldInitialise.Name].Value != null &&
					(bool)row.Cells[ShouldInitialise.Name].Value &&
					string.IsNullOrEmpty(row.Cells[BackupFilename.Name].Value as string))
				{
					row.Cells[BackupFilename.Name].ErrorText = "Please select a file.";
					result = false;
				}
				else
				{
					row.Cells[BackupFilename.Name].ErrorText = string.Empty;
				}
			}

			return result;
		}

		bool ValidateAllBackupFileNames()
		{
			bool result = true;

			foreach (DataGridViewRow row in dbMatchingDataGridView.Rows)
			{
				result &= ValidateBackupFilename(row);
			}

			return result;
		}

		bool ValidateLocalDirectory(TextBox textBoxToValidate)
		{
			Argument.NotNull(textBoxToValidate, nameof(textBoxToValidate));

			var error = "";
			var result =
					String.IsNullOrEmpty(textBoxToValidate.Text) ||
					ScreenNavigator.CheckLocalDirectory(textBoxToValidate.Text, out error);

			errorProvider.SetError(textBoxToValidate, error);
			return result;
		}

		bool ValidateAll()
		{
			var result = ValidateAllBackupFileNames();
			result &= ValidateLocalDirectory(dataDirectoryOverrideTextBox);
			result &= ValidateLocalDirectory(logDirectoryOverrideTextBox);

			return result;
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1090:Don't use System.Windows.Forms dialogs", Justification = "Standalone tool, no reference to ZArchitecture")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Standalone tool, no reference to ZArchitecture, Cannot use ZArch because it is an external tool")]
		void OpenFolderBrowserAndSetValue(TextBox textBoxToSet)
		{
			Argument.NotNull(textBoxToSet, nameof(textBoxToSet));

			using (var folderBrowserDialog = new FolderBrowserDialog())
			{
				folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;

				if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
				{
					try
					{
						textBoxToSet.Text = folderBrowserDialog.SelectedPath;
						ValidateLocalDirectory(textBoxToSet);
					}
					catch (NotSupportedException ex)
					{
						MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}
	}
}

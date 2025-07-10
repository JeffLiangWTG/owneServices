using CargoWise.Windows.UI;
using Enterprise.DataTools.DbBackupAndRestore.Business;
using Enterprise.DataTools.DbBackupAndRestore.Business.Testing.Restore;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI.Testing
{
	sealed class DbRestoreControlForTest : DbRestoreControl
	{
		public DbRestoreControlForTest()
		{
		}

		public void InitialiseRestoreManagerWithMock()
		{
			restoreManager = new DbRestoreManagerForTest();
		}

		#region Method Exposure

		public void OnBackupFileSelectedExposed(string path) => OnBackupFileSelected(path);

		public void PopulateAvailabilityGroupsComboBoxExposed() => PopulateAvailabilityGroupsComboBox();

		public bool ValidateAvailabilityGroupSelectionExposed() => ValidateAvailabilityGroupSelection();

		public void CheckBiServerExposed() => CheckBiServer();

		public void ResetDbFilesExposed() => ResetDbFiles();

		public void RefreshListViewExposed() => RefreshListView();

		public void UpdateDbFilesFolderPathViewExposed() => UpdateDbFilesFolderPathView();

		#endregion

		#region Member Exposure

		public DbRestoreManagerForTest RestoreManagerExposed => restoreManager as DbRestoreManagerForTest;

		public KCheckBox DifferentialBackupsCheckBoxExposed => DifferentialBackupsCheckBox;
		public KComboBox DbRestoreOptionComboBoxExposed => DbRestoreOptionComboBox;
		public KCheckBox RestoreBiDatabasesCheckBoxExposed => RestoreBiDatabasesCheckBox;
		public KTextBox DbRestoreFilePathTextBoxExposed => DbRestoreFilePathTextBox;
		public KTextBox DbRestoreDatabaseNameTextBoxExposed => DbRestoreDatabaseNameTextBox;
		public KTextBox DbRestoreServerTextBoxExposed => DbRestoreServerTextBox;
		public KTextBox DbRestoreAuditServerTextBoxExposed => DbRestoreAuditServerTextBox;
		public KTextBox DbRestoreDwServerTextBoxExposed => DbRestoreDwServerTextBox;
		public KDataGridView DbRestoreListViewExposed { get => DbRestoreListView; set => DbRestoreListView = value; }
		public DbFileInfoCollection RestoreDbFilesExposed { get => restoreDbFiles; set => restoreDbFiles = value; }
		public KCheckBox AddDbToAvailabilityGroupCheckBoxExposed => AddDbToAvailabilityGroupCheckBox;
		public KComboBox AvailabilityGroupComboBoxExposed => AvailabilityGroupComboBox;

		#endregion
	}
}

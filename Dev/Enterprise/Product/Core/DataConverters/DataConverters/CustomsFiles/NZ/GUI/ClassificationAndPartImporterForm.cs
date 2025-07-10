using System;
using System.Threading;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DataConverters.CustomsFiles.NZ.GUI
{
#if DEBUG
	[SuppressFormsLocalizedTest]	// NZ Specific Form
#endif
	public partial class ClassificationAndPartImporterForm : ZChildForm
	{
		public ClassificationAndPartImporterForm()
		{
			InitializeComponent();
		}

		protected override void Dispose(bool disposing )
		{
			if (disposing )
			{
				if (OpenFileDialog != null)
				{
					OpenFileDialog.Dispose();
				}
			}
			base.Dispose(disposing );
		}

		internal ProgressLogger Logger;

		#region ImportButtonClick Handler
		void StartImportButton_Click(object sender, EventArgs e)
		{
			SetEnabledForUserModifiableControls(false);
			LogListBox.Items.Clear();
			Logger = new ProgressLogger();
			try
			{
				Logger.UpdateCounters += new EventHandler(Logger_OnUpdateCounters);
				Logger.UpdateLogText += new EventHandler(Logger_OnUpdateLogText);

				ImportData(Logger);
			}
			finally
			{
				Logger.UpdateCounters -= new EventHandler(Logger_OnUpdateCounters);
				Logger.UpdateLogText -= new EventHandler(Logger_OnUpdateLogText);
				SetEnabledForUserModifiableControls(true);
			}
		}
		#endregion

		#region GetDataImporter()
		void ImportData(ProgressLogger logger)
		{
			string fileName = DataSourcePathTextBox.Text;
			using (ZOpenFileDialog.ForceLocalFile(ref fileName))
			{
				if (ClassificationsRadioButton.Checked)
				{
					DataImporter importer = new DataImporters.Excel.ClassificationDataImporter(logger, fileName, !UpdateExistingRecordsCheckBox.Checked);
					importer.Import();
				}
				else if (ProductsRadioButton.Checked)
				{
					DataImporter importer = new DataImporters.Excel.ProductDataImporter(logger, fileName, !UpdateExistingRecordsCheckBox.Checked);
					importer.Import();
				}
			}
		}
		#endregion

		#region Message Display Methods
		protected virtual void ShowErrorMessage(string message)
		{
			Globals.Message.ShowError(message, Text);
		}

		protected virtual void ShowInformationMessage(string message)
		{
			Globals.Message.ShowInformation(message, Text);
		}
		#endregion

		#region SetEnabledForUserModifiableControls
		internal void SetEnabledForUserModifiableControls(bool value)
		{
			DataSourceTypeGroupBox.Enabled = value;
			DataToImportGroupBox.Enabled = value;
			DataSourcePathTextBox.Enabled = value;
			DataSourcePathBrowseButton.Enabled = value;
			UpdateExistingRecordsCheckBox.Enabled = value;
			StartImportButton.Enabled = value;
			CopyLogButton.Enabled = value;
			CloseButton.Enabled = value;
		}
		#endregion

		#region Importer Progress Event Handlers
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void Logger_OnUpdateCounters(object sender, EventArgs e)
		{
			ProgressLogger logger = (ProgressLogger)sender;

			RowsCreatedLabel.Text = logger.RecordsCreated.ToString();
			RowsCreatedLabel.Refresh();
			RowsUpdatedLabel.Text = logger.RecordsUpdated.ToString();
			RowsUpdatedLabel.Refresh();
			RowsExcludedLabel.Text = logger.RecordsExcluded.ToString();
			RowsExcludedLabel.Refresh();
			RowsInvalidLabel.Text = logger.RecordsInvalid.ToString();
			RowsInvalidLabel.Refresh();
			RowsTotalLabel.Text = logger.RecordsToBeProcessed.ToString();
			RowsTotalLabel.Refresh();
			RowsProcessedLabel.Text = logger.RecordsProcessed.ToString();
			RowsProcessedLabel.Refresh();

			if (logger.RecordsToBeProcessed > 0)
			{
				ProgressBar.Value = logger.RecordsProcessed * 100 / logger.RecordsToBeProcessed;
				ProgressBar.Refresh();
			}

			Application.DoEvents();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void Logger_OnUpdateLogText(object sender, EventArgs e)
		{
			this.LogListBox.Items.Add(sender);
			Application.DoEvents();
		}
		#endregion

		#region Close Button
		void ClassificationAndPartImporter_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = !CloseButton.Enabled;
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
		#endregion

		#region Browse Button
		void DataSourcePathBrowseButton_Click(object sender, EventArgs e)
		{
			BrowseForFile();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Setting initial/default directory in file dialog form")]
		void BrowseForFile()
		{
			OpenFileDialog.InitialDirectory = "c:\\" ;  // Setting initial/default directory in file dialog form
			OpenFileDialog.Filter = "TXT files (*.txt)|*.txt|CSV files (*.csv)|*.csv|All files (*.*)|*.*" ;
			OpenFileDialog.FilterIndex = 2 ;
			OpenFileDialog.RestoreDirectory = true ;

			if (OpenFileDialog.ShowDialog() == DialogResult.OK)
			{
				DataSourcePathTextBox.Text = OpenFileDialog.UnmappedFileName;
				DataSourcePathTextBox.Focus();
				DataSourcePathTextBox.SelectionStart = DataSourcePathTextBox.Text.Length;
				DataSourcePathTextBox.SelectionLength = 0;
			}
		}
		#endregion

		#region Copy Log to Clipboard
		internal void CopyLogButton_Click(object sender, EventArgs e)
		{
			if (Logger == null)
			{
				ShowErrorMessage(CopyToClipboardNotPossibleYet);
			}
			else
			{
				Thread.Sleep(0);
				if (!SafeClipboard.SetDataObject(Logger.ToString(), true))
				{
					ShowErrorMessage(CopyToClipboardFailed);
					return;
				}
				Thread.Sleep(100); // Is a workaround for an exception that happens upon pasting on fast Windows 2003 machines.
				ShowInformationMessage(CopyToClipboardSuccessful);
			}
		}
		public const string CopyToClipboardSuccessful = "Text Log Copied to Clipboard. You can paste the log into Word or another text editor for review.";
		public const string CopyToClipboardFailed = "Could not Copy Log to Clipboard. Clipboard not available on this Session.";
		public const string CopyToClipboardNotPossibleYet = "Could not Copy Log to Clipboard. There is no log to copy yet.";
		#endregion

		#region InternalForTesting
		internal ZGroupBox DataToImportGroupBoxInternal => DataToImportGroupBox;
		internal ZGroupBox DataSourceTypeGroupBoxInternal => DataSourceTypeGroupBox;
		internal ZCheckBox UpdateExistingRecordsCheckBoxInternal => UpdateExistingRecordsCheckBox;
		internal ZButton CopyLogButtonInternal => CopyLogButton;
		internal ZButton CloseButtonInternal => CloseButton;
		internal ZButton StartImportButtonInternal => StartImportButton;
		internal ZArchitecture.ZTextBox DataSourcePathTextBoxInternal => DataSourcePathTextBox;
		internal ZButton DataSourcePathBrowseButtonInternal => DataSourcePathBrowseButton;
		#endregion
	}
}

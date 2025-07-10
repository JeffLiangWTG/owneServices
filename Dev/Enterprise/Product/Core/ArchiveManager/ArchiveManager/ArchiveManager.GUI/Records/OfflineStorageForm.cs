using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ArchiveManager.GUI.Records
{
	public partial class OfflineStorageForm : ZChildForm, IArchiveLogger
	{
		public OfflineStorageForm()
		{
			InitializeComponent();

#if DEBUG
			TypeDescriptor.AddAttributes(archiveProgressBar, new[] { new SuppressDpiAwareBasherAttribute() });
#endif
		}

		public OfflineStorageForm(OfflineStorage offlineStorage)
			: base(offlineStorage)
		{
			InitializeComponent();
		}

		public ZGlobalMutex FormMutex { get; set; }

		public override string FormHeading
			=> Res.GetString("3ba5f062-36c9-4346-b317-852bdb5253a9", "Archive Offline");

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (FormMutex != null && FormMutex.IsLocked)
			{
				FormMutex.Unlock();
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		protected override ZTabControl TopLevelTabControl
			=> mainTabControl;

		protected override void OnClosing(CancelEventArgs e)
		{
			if (OfflineStorageEntity.IsArchiving)
			{
				Globals.Message.ShowError(Res.GetString("d756afba-c2c3-4ed3-8095-42b6da8a2e13", "You cannot close the form while an archive process is still running. You must abort the archive process before closing the form."));
				e.Cancel = true;
			}

			base.OnClosing(e);
		}

		OfflineStorage OfflineStorageEntity
			=> BusinessEntity as OfflineStorage;

		void folderButton_Click(object sender, EventArgs e)
		{
			folderBrowserDialog.RequireMappablePath = true;
			var result = folderBrowserDialog.ShowDialog();

			if (result == DialogResult.OK)
			{
				try
				{
					OfflineStorageEntity.OfflineLocation = folderBrowserDialog.MappedSelectedPath;
				}
				catch (NotSupportedException)
				{
					OfflineStorageEntity.OfflineLocation = Res.GetString("161cf085-7b17-47ca-b811-1050903894ae", "Invalid Folder Selected.");
				}
			}
		}

		void archiveButton_Click(object sender, EventArgs e)
		{
			OfflineStorageEntity.CalculateRequiredStorageCapacity();
			ValidateAll(CargoWise.EntityFramework.ValidationType.Full);

			if (OfflineStorageEntity.HasErrors)
			{
				LogError("OFL", Res.GetString("7dd0a56b-2b66-433d-a71c-980defb41f1b", "Please fix errors on Form first."));
			}
			else
			{
				Archive();
			}
		}

		public void Archive()
		{
			var caption = Res.GetString("0DBB7214-4587-472F-A476-06AC7068669B", "Confirmation");
			var confirmationString = Res.GetString("88B444D2-5EF0-432C-A7DE-25E74BAEC468", "Confirm");
			var confirmationPrompt = Res.GetString("83B952AA-9061-4AD2-9B97-D450DFD13C47", "Please confirm that you want to archive the data to an offline location.");
			if (Globals.Message.ShowConfirmation(Res.GetString("f56fc81a-3b79-42e3-bcf3-bf3f5404aaae", "Do you want to start archiving offline?"), caption, confirmationPrompt, confirmationString, MessageBoxIcon.Warning) == DialogResult.OK)
			{
				try
				{
					UpdateFormControls(false);
					progressTextBox.Clear();
					OfflineStorageEntity.Logger = this;

					archiveProgressBar.Maximum = OfflineStorageEntity.StorageMainRecordsForArchiving;
					archiveProgressBar.Value = 0;

					OfflineStorageEntity.ArchivedOneUnit += new EventHandler(OfflineStorageEntity_ArchivedOneUnit);
					OfflineStorageEntity.Archive();
				}
				finally
				{
					UpdateFormControls(true);
					OfflineStorageEntity.Logger = null;
				}
			}
		}

		void UpdateFormControls(bool enabled)
		{
			archiveButton.Visible = enabled;
			abortButton.Visible = !enabled;
			archiveDateToDateEdit.ReadOnly = !enabled;
			locationTextBox.ReadOnly = !enabled;
			folderButton.Enabled = enabled;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void OfflineStorageEntity_ArchivedOneUnit(object sender, EventArgs e)
		{
			if (archiveProgressBar.Value < archiveProgressBar.Maximum)
			{
				archiveProgressBar.Value++;
			}

			Application.DoEvents();
		}

		void abortButton_Click(object sender, EventArgs e)
		{
			if (OfflineStorageEntity.IsArchiving)
			{
				if (Globals.Message.ShowConfirmation(Res.GetString("070b5cda-ef7d-45a4-bdc8-a8d8197cbf08", "Do you want to abort the archive process? All volume files created so far will be considered as closed and completed volumes for future restores."), Res.GetString("7b5f8bb6-749d-4efb-99cc-189524838e0f", "Abort"), Res.GetString("a4f72829-b8a0-48c3-8f68-6cc45706afd2", "yes"), MessageBoxIcon.Warning) == DialogResult.OK)
				{
					OfflineStorageEntity.AbortArchiving();
				}
			}
		}

		#region IArchiveLogger Members

		public void LogAndReportError(string key, string systemCode, string message, Exception exception)
		{
			if (ErrorReporter.HasBeenReported(key))
			{
				LogError(message + "\n\n" + exception.ToString());
			}

			ErrorReporter.ReportOnce(key, message, exception);
		}

		public void LogError(string code, string message)
			=> LogError(message);

		public void LogError(string message)
			=> progressTextBox.AppendText(message + System.Environment.NewLine);

		public void LogInfo(string code, string message)
			=> LogInfo(message);

		public void LogInfo(string message)
			=> progressTextBox.AppendText(message + System.Environment.NewLine);

		public void LogWarning(string code, string message)
			=> LogWarning(code);

		public void LogWarning(string message)
			=> progressTextBox.AppendText(message + System.Environment.NewLine);

		#endregion
	}
}

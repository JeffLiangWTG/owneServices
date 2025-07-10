using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.DataImport;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Client.UPE.GUI.DataImport
{
	public partial class Level1DataImportForm : ZChildForm, INotifications
	{
		public Level1DataImportForm(Level1DataImport businessEntity) : base(businessEntity)
		{
		}

		public Level1DataImport Level1DataImport
		{
			get { return (Level1DataImport)base.BusinessEntity; }
		}

		protected override void OnLoad(EventArgs e)
		{
			if (Level1DataImport != null && Level1DataImport.IsImportToManifest)
			{
				detailForManifestControl.Visible = true;
				detailControl.Visible = false;
				reasonsForManifestControl.Visible = true;
				reasonsControl.Visible = false;
			}
			else
			{
				detailForManifestControl.Visible = false;
				detailControl.Visible = true;
				reasonsForManifestControl.Visible = false;
				reasonsControl.Visible = true;
			}
			base.OnLoad(e);
		}

		internal void DoLoad()
		{
			SummaryTextBox.Clear();
			Level1DataImport.PecentageOfDuplicateHAWBs = 0;
			Level1DataImport.ValidateAll();

			if (Level1DataImport.HasErrors)
			{
				Globals.Message.ShowInformation("All errors must be corrected before loading is possible.");
			}
			else
			{
				if (OpenFileDialogResult == DialogResult.OK)
				{
					try
					{
						SetControlState(false);
						Level1DataImport.FileName = Dialog.ForceLocalFile();
						if (Level1DataImport.IsImportToManifest)
						{
							DataImporter = new Level1DataFileImporterForSGAccess(Level1DataImport, this);
						}
						else
						{
							DataImporter = new Level1DataFileImporterForAU(Level1DataImport, this);
						}

						MainTabControl.SelectedIndex = 1;
						DataImporter.LoadFile();
						Level1DataImport.PecentageOfDuplicateHAWBs = DataImporter.PercentageOfDuplicateHAWBs;
						ShowSummaryInformation();
					}
					finally
					{
						SetControlState(true);
					}
				}
			}
		}
		protected Level1DataFileImporterCore DataImporter;

		protected virtual DialogResult OpenFileDialogResult
		{
			get { return Dialog.ShowDialog(this); }
		}

		#region Summary Information

		internal  void ShowSummaryInformation()
		{
			var summaryInformation = DataImporter.GetSummaryInformation();
			Level1DataImport.LoadSummaryInformation = summaryInformation;
			SummaryTextBox.Text = summaryInformation;
		}

		#endregion

		internal void SetControlState(bool active)
		{
			if (!active)
			{
				ResetProgressBar();
			}
			LoadButton.Enabled = active;
			SaveButton.Enabled = active;
			CloseButton.Enabled = active;
		}

		internal void DoSave()
		{
			Level1DataImport.ValidateAll();
			var errorMessage = DataImporter?.GetErrorMessage() ?? ZString.Empty;

			if (Level1DataImport.HasErrors)
			{
				Globals.Message.ShowInformation("All errors must be corrected before saving is possible.");
			}
			else if (DataImporter == null || Level1DataImport.FileName.IsEmpty)
			{
				Globals.Message.ShowInformation("Please load the file before you save.");
			}
			else if (!errorMessage.IsEmpty)
			{
				Globals.Message.ShowInformation(errorMessage);
			}
			else
			{
				SetControlState(false);
				MainTabControl.SelectedIndex = 1;

				if (Globals.Message.Show("Are you sure you want to save?", "Confirm Save?", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
				{
					try
					{
						DataImporter.Save();
					}
					finally
					{
						CloseButton.Enabled = true;
					}
				}
				else
				{
					SetControlState(true);
				}
			}
		}

		void ResetProgressBar()
		{
			progressBar.Value = 0;
		}

		protected ZOpenFileDialog Dialog
		{
			get
			{
				if (dialog == null)
				{
					dialog = new ZOpenFileDialog();
					Dialog.Filter = "All files (*.*)|*.*";
					Dialog.CheckFileExists = true;
				}
				return dialog;
			}
		}
		ZOpenFileDialog dialog;

		#region INotifications Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void INotifications.Add(INotification notification)
		{
			if (notification is ProgressNotification)
			{
				progressBar.Value = ((ProgressNotification)notification).PercentageComplete;
			}
			else
			{
				SummaryTextBox.AppendText(string.Concat(notification.Message, System.Environment.NewLine));
			}
			Application.DoEvents();
		}

		#endregion

		void LoadButton_Click(object sender, EventArgs e)
		{
			DoLoad();
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			DoSave();
		}

		#region Dispose

		System.ComponentModel.IContainer components;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (dialog != null)
				{
					dialog.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

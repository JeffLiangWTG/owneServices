using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = MailManager.Module.Res;

namespace Enterprise.MailManager.GUI
{
	public partial class MailItemForm : ZForm, IPreviousNextControlProvider
	{
		public MailItemForm(MailItem businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
			DisplayModeChanged += MailItemForm_DisplayModeChanged;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			if (IsNew)
			{
				SaveEntireObjectButton.Enabled = false;
				SaveOrAddAttachmentButton.Enabled = true;
				SaveAllAttachmentsOrRemoveAttachmentButton.Text = Res.GetString("17df4c0b-1c5f-4dae-8718-4844d818cba9", "Remove attachment(s)");
				SaveOrAddAttachmentButton.Text = Res.GetString("619e16c2-0b0f-40ef-93ea-043a80a8b004", "Add attachment(s)");
				this.ValidatingForSave += InitializeApplication;
			}
			else
			{
				if (Item.MailAttachments.Count == 0)
				{
					SaveAllAttachmentsOrRemoveAttachmentButton.Enabled = false;
					SaveOrAddAttachmentButton.Enabled = false;
				}
			}
		}

		void InitializeApplication(object sender, ValidatingForSaveEventArgs e)
		{
			bool result = true;
			if (!BusinessEntity.IsInDatabase)
			{
				var filterLocator = new MailFilters.MailFilterLocator(true);
				var applicableFilters = filterLocator.GetFilters().Where(x => x.CanProcess((IMailItem)BusinessEntity)).ToList();

				if (applicableFilters.Count > 0)
				{
					((IMailItem)BusinessEntity).MI_Application = applicableFilters.First().Code;
					if (applicableFilters.Count > 1)
					{
						result = Globals.Message.Show(Res.GetString("7003d9c3-7ad1-4e93-82bc-231324f7fa8c",
   "Note that more than one filter matched this piece of mail; verify that this is what you wanted, then press Yes to save or No to continue modifying the email. (Matching Application Codes: {0})",
   applicableFilters.Select(x => x.Code).Aggregate((x, y) => x + ", " + y)), Res.GetString("f097d57a-9e75-44f0-9c8a-9273b673f97c", "Multiple Filters Match"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes;
					}
				}
				else
				{
					result = Globals.Message.Show(Res.GetString("ab30efca-0bb6-4d8e-a9f0-97fe095789b3",
							"Note that no filters matched this piece of mail, so it is unlikely to be processed; verify that this is what you wanted, then press Yes to save or No to continue modifying the email."),
							Res.GetString("08a7bc08-3462-4f74-98ca-8b7afcb9b8f0", "No Filters Match"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes;
				}
			}

			e.ContinueWithSave = result ? ContinueWithSave.Yes : ContinueWithSave.No;
		}

		void MailItemForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			if (IsNew)
			{
				PostingButtonsUserControl.SaveButton.Visible = false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant from base...........................")]
		bool IsNew
		{
			get { return FormVerb == "New"; }
		}

#if DEBUG
		internal
#else
		protected
#endif
		MailItem Item
		{
			get { return (MailItem)BusinessEntity; }
		}

#if DEBUG
		virtual
#endif
		protected ZString GetAttachmentsSavePathFromUser()
		{
			ZString result = "";

			var destinationDialog = new ZFolderBrowserDialog();
			destinationDialog.CreateDirectory = true;
			destinationDialog.Description = Res.GetString("MailItemForm|Description", "Please select the destination folder to save attachments to..");
			DialogResult dialogResult = destinationDialog.ShowDialog();

			if (dialogResult == DialogResult.OK)
			{
				result = destinationDialog.UnmappedSelectedPath;
			}

			return result;
		}

		internal void AddAttachments()
		{
			ZOpenFileDialog.FileInfo[] fileNamesChosen = GetAttachmentFilesToAddFromUser();

			foreach (var file in fileNamesChosen)
			{
				var att = Item.MailAttachments.AddNew();
				att.SetMA_DataSource(file.GetStreamSource());
				att.MA_FileName = Path.GetFileName(file.UnmappedFileName);
			}
		}

		internal virtual ZOpenFileDialog.FileInfo[] GetAttachmentFilesToAddFromUser()
		{
			ZOpenFileDialog.FileInfo[] result = Array.Empty<ZOpenFileDialog.FileInfo>();
			var openFileDialog = new ZOpenFileDialog();
			openFileDialog.Multiselect = true;
			var dialogResult = openFileDialog.ShowDialog();

			if (dialogResult == DialogResult.OK)
			{
				result = openFileDialog.SelectedFiles;
			}
			return result;
		}

		void SaveAddAttachmentButton_Click(object sender, EventArgs e)
		{
			if (IsNew)
			{
				AddAttachments();
			}
			else
			{
				SaveOneAttachment();
			}
		}

#if DEBUG
		internal
#endif
		void SaveOneAttachment()
		{
			if (AttachmentsGrid.SelectedElements.Length > 0)
			{
				ZString savePath = GetAttachmentsSavePathFromUser();
				if (!savePath.IsEmpty)
				{
					MailAttachment[] attachmentsToSave = new MailAttachment[AttachmentsGrid.SelectedElements.Length];
					AttachmentsGrid.SelectedElements.CopyTo(attachmentsToSave, 0);

					try
					{
						Item.SaveAttachmentsTo(attachmentsToSave, savePath);
						ShowInformation(Res.GetString("5dc56d55-ec22-4e6e-ac3f-cf6d4bb0ab22", "Attachments were successfully saved to the {0} directory.", savePath));
					}
					catch (IOException ex)
					{
						ShowCannotSaveError(ex);
					}
					catch (UnauthorizedAccessException ex)
					{
						ShowCannotSaveError(ex);
					}
				}
			}
			else
			{
				ShowError(Res.GetString("898956f7-7a79-4f78-af6e-400b97a8d318", "Please select Attachments to save."));
			}
		}

		void SaveAllAttachmentsOrRemoveAttachmentButton_Click(object sender, EventArgs e)
		{
			if (IsNew)
			{
				RemoveSelectedAttachments();
			}
			else
			{
				SaveAllAttachments();
			}
		}

		internal void RemoveSelectedAttachments()
		{
			if (AttachmentsGrid.SelectedElements.Length > 0)
			{
				var attachmentsToRemove = AttachmentsGrid.SelectedElements.ToList();
				attachmentsToRemove.ForEach(att => Item.MailAttachments.RemoveAndDelete(att));
			}
			else
			{
				ShowInformation(Res.GetString("7cfc6d47-e39a-45e6-8e21-7409cd3ac38c", "Please select attachments to remove."));
			}
		}

#if DEBUG
		internal
#else
		protected
#endif
		void SaveAllAttachments()
		{
			if (AttachmentsGrid.List.Count > 0)
			{
				ZString savePath = GetAttachmentsSavePathFromUser();
				if (!savePath.IsEmpty)
				{
					try
					{
						Item.SaveAllAttachmentsTo(savePath);
						ShowInformation(Res.GetString("5dc56d55-ec22-4e6e-ac3f-cf6d4bb0ab22", "Attachments were successfully saved to the {0} directory.", savePath));
					}
					catch (IOException ex)
					{
						ShowCannotSaveError(ex);
					}
					catch (UnauthorizedAccessException ex)
					{
						ShowCannotSaveError(ex);
					}
				}
			}
			else
			{
				ShowError(Res.GetString("608b3242-f879-46c3-b7b6-1d63dce077cd", "No Attachments available"));
			}
		}

		void ShowCannotSaveError(Exception ex)
		{
			ShowError(Res.GetString("5667fcd3-f3df-461d-8fe1-5221e2655756", "Cannot save attachments.\r\n{0}", ex.Message));
		}

		void ShowInformation(string message)
		{
			Globals.Message.ShowInformation(message);
		}

		void ShowError(string message)
		{
			Globals.Message.ShowError(message);
		}

		void SaveEntireObjectButton_Click(object sender, EventArgs e)
		{
			SaveEntireEmail();
		}

#if DEBUG
		internal
#else
		protected
#endif
		void SaveEntireEmail()
		{
			string displayFileName;
			Stream saveFileStream = GetEmailSaveStreamFromUser(out displayFileName);
			if (saveFileStream != null)
			{
				try
				{
					Item.SaveEntireEmailAsEml(saveFileStream);
					ShowInformation(Res.GetString("6e1bbc71-e8cb-4439-846e-5799b6ed40f4", "Email was successfully saved as '{0}'.", displayFileName));
				}
				catch (IOException ex)
				{
					ShowCannotSaveError(ex);
				}
				catch (UnauthorizedAccessException ex)
				{
					ShowCannotSaveError(ex);
				}
				finally
				{
					saveFileStream.Dispose();
				}
			}
		}

		internal virtual Stream GetEmailSaveStreamFromUser(out string displayFileName)
		{
			Stream result = null;
			displayFileName = null;

			var destinationDialog = new ZSaveFileDialog();
			destinationDialog.Title = Res.GetString("5f866f78-8594-478f-81e9-a09c8983a664", "Please select a directory and filename for saving");
			destinationDialog.Filter = (NoResString)"Email files (*.eml)|*.eml|Text files (*.txt)|*.txt|All files (*.*)|*.*";
			destinationDialog.FileName = Item.MI_Subject.ExcludeChars(@"\/?%*:|""<>.") + ".eml";
			destinationDialog.RestoreDirectory = true;
			DialogResult dialogResult = destinationDialog.ShowDialog();

			if (dialogResult == DialogResult.OK)
			{
				displayFileName = destinationDialog.UnmappedFileName;
				result = destinationDialog.OpenFile();
			}
			return result;
		}

		#region IPreviousNextControlProvider Members

		bool IPreviousNextControlProvider.AutoAddPreviousNextButtons
		{
			get { return true; }
		}

#if DEBUG
		ZPreviousNextControl IPreviousNextControlProvider.PreviousNextControlForTesting
		{
			get;
			set;
		}

#endif
		#endregion

	}
}

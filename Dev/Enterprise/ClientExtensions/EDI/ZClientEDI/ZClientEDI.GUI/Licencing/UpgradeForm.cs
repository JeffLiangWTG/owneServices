using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.Mail.GUI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	public partial class UpgradeForm : ZChildForm
	{
		public UpgradeForm(UpgradeRequestCollectionContainer upgrades)
			: base(upgrades)
		{
			SetupForm();
		}

		void SetupForm()
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton);
		}

		public new void Show()
		{
			ArrayList unconfirmedUpgrades = new ArrayList();
			ZGuid prevOrgPK = ZGuid.Empty;
			bool removeUnconfirmed = false;

			foreach (UpgradeRequest upgrade in BusinessEntity.Upgrades)
			{
				if (prevOrgPK != upgrade.OrganisationPk)
				{
					removeUnconfirmed = false;

					var org = upgrade.Factory.Load<EDIOrgHeader>(upgrade.OrganisationPk);
					if (org != null && !CheckAndConfirmDeliveryInstruction(org))
					{
						removeUnconfirmed = true;
						unconfirmedUpgrades.Add(upgrade);
					}
				}
				else if (removeUnconfirmed)
				{
					unconfirmedUpgrades.Add(upgrade);
				}
			}

			foreach (UpgradeRequest upgrade in unconfirmedUpgrades)
			{
				BusinessEntity.Upgrades.Remove(upgrade);
			}

			if (BusinessEntity.Upgrades.Count > 0)
			{
				base.Show();
			}
			else
			{
				Globals.Message.ShowInformation("Selected organisations do not support upgrades.\r\nPlease, change your selection.");
			}
		}

		bool CheckAndConfirmDeliveryInstruction(EDIOrgHeader header)
		{
			bool result = true;
			StmNote[] notesFound = header.Notes.FindByDescription(PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description);
			if (notesFound.Length > 0)
			{
				DialogResult confirmationResult = Globals.Message.ShowConfirmation(notesFound[0].ST_NoteText, "Upgrade Delivery Instruction", "To continue, type: ", "Upgrade", MessageBoxIcon.Question);

				if (confirmationResult != DialogResult.OK)
				{
					result = false;
				}
			}
			return result;
		}

		public override string FormCaption
		{
			get { return "Upgrade"; }
		}

		public override string FormVerb
		{
			get { return "Send"; }
		}

		public new UpgradeRequestCollectionContainer BusinessEntity
		{
			get { return (UpgradeRequestCollectionContainer)base.BusinessEntity; }
		}

		#region Send Upgrade

		void SendButton_Click(object sender, EventArgs e)
		{
			SendUpgrade();
		}

		void SendUpgrade()
		{
			bool shouldClose = false;

			if (BusinessEntity.IsSaveToDisk)
			{
				shouldClose = SaveToDisk();
			}
			else
			{
				shouldClose = SendUpgradeToClients();
			}

			if (shouldClose)
			{
				Close();
			}
		}

		bool SaveToDisk()
		{
			string saveToDiskDirectory = "";
			bool saveToDisk = false;

			IPreUpgradeStatus status = BusinessEntity.GetPreUpgradeStatus();

			if (status.IsError)
			{
				this.ShowPreUpgradeStatusAndConfirm(status);
				saveToDisk = false;
			}
			else
			{
				int retries = 2;
				while (retries >= 0)
				{
					try
					{
						saveToDisk = AskForDestinationFolder(out saveToDiskDirectory);
						break;
					}
					catch (ArgumentException)
					{
						Globals.Message.ShowError("Selected directory is inaccessible or otherwise invalid. Please select another location or try again", "Invalid directory");
						if (retries == 0)
						{
							throw;
						}

						saveToDisk = false;
					}
					retries--;
				}

				if (saveToDisk)
				{
#if WINZOR
					SaveAndSend(saveToDiskDirectory);
#else
					ShowPostUpgradeMessages(BusinessEntity.SaveToDisk(saveToDiskDirectory));
#endif
				}
			}

			return saveToDisk;
		}

#if WINZOR
		void SaveAndSend(string saveToDiskDirectory)
		{
			var tempDirectory = EnvProxy.Instance.TempPath;
			var postUpgradeStatus = BusinessEntity.SaveToDisk(tempDirectory);
			if (postUpgradeStatus.IsError || string.IsNullOrEmpty(postUpgradeStatus.PackagePath))
			{
				ShowPostUpgradeMessages(postUpgradeStatus);
				return;
			}

			try
			{
				var fileName = Path.GetFileName(postUpgradeStatus.PackagePath);
				var data = File.ReadAllBytes(postUpgradeStatus.PackagePath);
				var filePath = Path.Combine(saveToDiskDirectory, fileName);
				RemoteDesktopServices.Server.RemoteFileDialog.SaveFile(filePath, data);
				var message = UpgradeRequestCollectionContainer.GetSaveToDiskSuccessMessage(saveToDiskDirectory) + "\r\n\r\n";
				Globals.Message.ShowInformation(message, "Upgrade Sent/Saved Successfully");
			}
			catch (Exception e)
			{
				var message = UpgradeRequestCollectionContainer.GetSaveToDiskExceptionMessage(e) + "\r\n\r\n";
				Globals.Message.ShowError(message, "Error Occurred");
			}
			finally
			{
				File.Delete(postUpgradeStatus.PackagePath);
			}
		}
#endif

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		protected virtual bool AskForDestinationFolder(out string destinationDirectory)
		{
			using (var destinationDialog = new ZFolderBrowserDialog())
			{
				destinationDialog.Description = "Please select folder to save the Build package";
				destinationDialog.SelectedPath = Path.Combine(
					System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),
					BusinessEntity.Upgrades.Count > 0 ? BusinessEntity.Upgrades[0].EnterpriseCode : ZString.Empty);
				destinationDialog.RequireMappablePath = true;

				if (destinationDialog.ShowDialog() != DialogResult.OK)
				{
					destinationDirectory = null;
					return false;
				}

				destinationDirectory = destinationDialog.MappedSelectedPath;
				return true;
			}
		}

		bool SendUpgradeToClients()
		{
			bool result = false;
			if (BusinessEntity.HasErrors)
			{
				this.ShowErrorsDialog();
			}
			else if (ShowPreUpgradeStatusAndConfirm(BusinessEntity.GetPreUpgradeStatus()))
			{
				ShowPostUpgradeMessages(BusinessEntity.PlaceUpgradesToClients());
				result = true;
			}
			return result;
		}

		bool ShowPreUpgradeStatusAndConfirm(IPreUpgradeStatus status)
		{
			bool result = false;

			if (!status.IsError)
			{
				StringBuilder message = new StringBuilder();
				if (!string.IsNullOrEmpty(status.Message))
				{
					message.Append(status.Message + "\r\n");
				}

				if (status.ActiveUpgradesTotalCount > 0)
				{
					message.Append(String.Format(CultureInfo.CurrentCulture, "\r\nTotal number of upgrade requests to add: {0}.\r\n", status.ActiveUpgradesTotalCount));
					for (UpgradeStatusType statusType = UpgradeStatusType.ViaHttp; statusType <= UpgradeStatusType.WithoutNotificationAddress; statusType++)
					{
						int count = status.GetRequestCountByStatusType(statusType);
						if (count > 0)
						{
							message.Append(String.Format(CultureInfo.CurrentCulture, "\r\nNumber of upgrade requests {0}: {1}.", status.GetStatusTypeDescription(statusType), count));
						}
					}

					message.Append("\r\n\r\nDo you want to proceed with the upgrade?");
					DialogResult userResponse = Globals.Message.Show(message.ToString(), "Upgrade Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					result = (userResponse == DialogResult.Yes);
				}
				else
				{
					string noActiveRequestsMessage = "There are no upgrades supported for the selected organisations.\r\nPlease, change your selection and try again.";
					Globals.Message.ShowError(noActiveRequestsMessage, "No Organisation supporting upgrade");
				}
			}
			else
			{
				Globals.Message.ShowError(status.Message, "Error Occurred");
			}
			return result;
		}

		void ShowPostUpgradeMessages(IPostUpgradeStatus status)
		{
			if (!status.IsError)
			{
				StringBuilder message = new StringBuilder();
				if (!string.IsNullOrEmpty(status.Message))
				{
					message.Append(status.Message + "\r\n\r\n");
				}

				if (status.CreatedUpgradesToClientsCount > 0)
				{
					message.Append(String.Format(CultureInfo.CurrentCulture, "Total number of scheduled upgrades: {0}\r\n", status.CreatedUpgradesToClientsCount));
				}
				if (status.NotificationEmailsCount > 0)
				{
					message.Append(String.Format(CultureInfo.CurrentCulture, "Total number of notification emails sent: {0}\r\n", status.NotificationEmailsCount));
				}

				if (status.NotificationEmailsWithoutRecipientCount > 0)
				{
					message.Append("There are some notification emails not sent because the re were no recipients specified.\r\n" +
						"They will be shown to you, so you could specify email address to send upgrade notification.");
				}

				Globals.Message.ShowInformation(message.ToString(), "Upgrade Sent/Saved Successfully");

				if (!status.NotificationEmailsSent && status.NotificationEmailsCount > 0)
				{
					foreach (CustomerServiceEmail unsentEmail in status.GetNotificationEmails())
					{
						ShowEmailForm(new CustomerServiceEmailForm(unsentEmail));
					}
				}

				foreach (EmailToContactBusinessObject unsentEmail in status.GetNotificationEmailsWithoutRecipient())
				{
					ShowEmailForm(new EmailContactForm(unsentEmail));
				}
			}
			else
			{
				Globals.Message.ShowError(status.Message, "Error Occurred");
			}
		}

#endregion

		void SaveToDiskRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			UpdateSendButtonText();
		}

		void UpdateSendButtonText()
		{
			SendButton.Text = (SaveToDiskRadioButton.Checked) ? "Save" : "Send";
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			DisplayMode = ODisplayMode.Browse;
			base.OnClosing(e);
		}

		protected virtual void ShowEmailForm(EmailContactForm form)
		{
			form.Show();
		}
	}
}

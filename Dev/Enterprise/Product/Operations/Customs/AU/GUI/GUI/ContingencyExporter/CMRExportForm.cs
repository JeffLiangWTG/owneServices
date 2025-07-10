using System.IO;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class CMRExportForm : ZChildForm
	{
		public CMRExportForm(ZForm parent, CMRDataExporterCSV exporter)
			: base(exporter)
		{
			this.parent = parent;
		}
		readonly ZForm parent;

		#region Export and Email

		public new CMRDataExporterCSV BusinessEntity
		{
			get { return (CMRDataExporterCSV)base.BusinessEntity; }
		}

		bool IsImportDeclarationContingency
		{
			get
			{
				var declaration = BusinessEntity.BizObj as JobDeclaration;
				return declaration != null && declaration.IsImport;
			}
		}

		internal const string CompulsoryTextRequiredInDeclarationContingency = @"Please note that the body of the email sent to customs containing a FID Contingency Data file must contain the following declaration:

If clearance is granted to take the goods identified in the attached contingency import declaration file into home consumption or for warehousing, I undertake to give Customs a declaration, in any case not later than 24 hours after the CEO declares that the Integrated Cargo System is operative, providing all particulars in accordance with section 71L in respect of the goods, and pay any duty, pay or defer GST/WET/LCT or any other charge owing at the rate applicable at the time the clearance is granted and to comply [sic] with any condition to which this clearance is subject. Failure to comply with any of the conditions may result in penalty action being undertaken.";

		public string Export()
		{
			string result = "";
			string tempFileName = "";
			DialogResult dialogRes = DialogResult.Cancel;

			if (parent != null)
			{
				var bizObjToBeExported = BusinessEntity.BizObj;
				if (bizObjToBeExported.HasChanges)
				{
					if (Globals.Message.Show("The data has not yet been saved. Do you want to save and proceed?", "Save", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
					{
						parent.FireSaveButton();
						if (bizObjToBeExported.HasChanges || bizObjToBeExported.HasErrors)
						{
							return result;
						}
					}
					else
					{
						return result;
					}
				}
			}

			AdditionalContingencyData additionalData = BusinessEntity.AdditionalData;
			using (AdditionalContingencyDataForm additionalDataForm = new AdditionalContingencyDataForm(additionalData))
			{
				additionalDataForm.ControlVisibility();
				ZFormModaliser.ShowDialogWithoutDispose(additionalDataForm);
#if DEBUG
				if (FormResult != DialogResult.None)
				{
					dialogRes = FormResult;
				}
				else
#endif
				{
					dialogRes = additionalDataForm.DialogResult;
				}
			}

			if (dialogRes == DialogResult.Cancel)
			{
				return result;
			}

			DataExporterResult generatedData = null;
			try
			{
				generatedData = BusinessEntity.Generate();
				tempFileName = Path.Combine(Env.TempPath, generatedData.FileName);
				using (var stream = File.Create(tempFileName))
				{
					BusinessEntity.SaveToFile(stream);
				}
			}
			catch (ExportException ex)
			{
				Globals.Message.ShowError(ex.Message);
				return result;
			}

			if (dialogRes == DialogResult.Yes)
			{
				var emailRecipients = GetEmailRecipients();
				if (emailRecipients.Length > 0)
				{
					if (File.Exists(tempFileName))
					{
						if (!BusinessEntity.BodyText.IsEmpty && Globals.Message.ShowConfirmation(BusinessEntity.BodyText, "EMail Body Text?",
		@"To acknowledge the above undertaking and add this text to the body of
the E-Mail type the following: ",
		"ADD ABOVE TEXT TO E-MAIL", MessageBoxIcon.Hand) == DialogResult.OK)
						{
							SendEmailWithFile(tempFileName, emailRecipients, BusinessEntity.BodyText);
						}
						else
						{
							SendEmailWithFile(tempFileName, emailRecipients);
						}
						Globals.Message.ShowInformation("Contingency Data Email was sent to " + RecipientEmailsAsString(emailRecipients) + ".", "Contingency Data");
						BusinessEntity.AttachToeDocs();
					}
				}
				else
				{
					Globals.Message.ShowInformation("You did not select a recipient for the Contingency Data.", "Contingency Data");
				}
			}
			else
			{
				if (IsImportDeclarationContingency)
				{
					Globals.Message.ShowInformation(CompulsoryTextRequiredInDeclarationContingency);
				}
				result = QueryForFolderNameAndSave(generatedData.FileName);
			}
			File.Delete(tempFileName);

			return result;
		}

		ZString RecipientEmailsAsString(string[] recipientEmails)
		{
			var result = new ZStringBuilder();
			foreach (var email in recipientEmails)
			{
				result.AppendIfNotEmpty(email);
			}
			return result.ToStringWithDelimiterBetweenAppends(",");
		}

#if DEBUG
		public DialogResult FormResult
		{
			get
			{
				return formResult;
			}
			set
			{
				formResult = value;
			}
		}
		DialogResult formResult = DialogResult.None;
#endif

		protected virtual string[] GetEmailRecipients()
		{
			var result = System.Array.Empty<string>();
			EmailRecipientSelection emailSelection = new EmailRecipientSelection(BusinessEntity.Factory);
			if (ZFormModaliser.ShowDialogAndDispose(new EmailRecipientQueryForm(emailSelection)) == DialogResult.OK)
			{
				result = emailSelection.RecipientEmails;
			}
			return result;
		}

		protected void SendEmailWithFile(string savedFileName, string[] emailRecipients)
		{
			SendEmailWithFile(savedFileName, emailRecipients, "");
		}
		void SendEmailWithFile(string savedFileName, string[] emailRecipients, ZString bodyText)
		{
			EmailDef email = new EmailDef(Env.CurrentUser.PK);
			email.Subject = BusinessEntity.MailSubject;
			email.Attachments.Add(new AttachmentDef(savedFileName));
			email.AddRecipientForSystemCommunication(emailRecipients);
			if (!bodyText.IsEmpty)
			{
				email.Body = bodyText;
			}
			Env.OutgoingCustomsMailManager.CreateAndSave(email);
		}

		string QueryForFolderNameAndSave(ZString fileName)
		{
			string fullFileName = "";

			var dialog = new ZFolderBrowserDialog();

			DialogResult response = ShowFolderBrowseDialogToUser(dialog);
			if (response == DialogResult.OK)
			{
				fullFileName = Path.Combine(dialog.UnmappedSelectedPath, fileName);
				using (var stream = ZSaveFileDialog.OpenFile(fullFileName))
				{
					BusinessEntity.SaveToFile(stream);
				}
				Globals.Message.ShowInformation("Contingency Data saved to '" + fullFileName + "'");
				BusinessEntity.AttachToeDocs();
			}

			return fullFileName;
		}

		protected virtual DialogResult ShowFolderBrowseDialogToUser(ZFolderBrowserDialog dialog)
		{
			return dialog.ShowDialog(this);
		}

		#endregion
	}
}

using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.IO;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class FileSaveToOpenForm : ZChildForm
	{
		protected FileSaveToOpenForm()
		{
			InitializeComponent();

			EmailBody = Res.GetString("8242a2a5-9d24-417b-9d7c-f5df21817e82", "Your document is attached.");
			emailButton.Enabled = !string.IsNullOrEmpty(EnvProxy.Instance.CurrentUser.EmailAddress);
		}

#if DEBUG
		static public FileSaveToOpenForm InstanceForTesting()
		{
			return new FileSaveToOpenForm();
		}
#endif

		public static void ShowDialog(FileGeneratorCallback fileGenerator, string defaultFileName)
		{
			ShowDialog(fileGenerator, defaultFileName, null, null, null);
		}

		public static void ShowDialog(FileGeneratorCallback fileGenerator, string defaultFileName, string dialogMessage, string emailSubject, string emailBody)
		{
			var form = new FileSaveToOpenForm();
			form.FileGenerator = fileGenerator;
			form.DefaultFileName = defaultFileName;
			if (!string.IsNullOrEmpty(dialogMessage))
			{
				form.messageLabel.Text = dialogMessage;
			}
			if (!string.IsNullOrEmpty(emailSubject))
			{
				form.EmailSubject = emailSubject;
			}
			if (!string.IsNullOrEmpty(emailBody))
			{
				form.EmailBody = emailBody;
			}
			ZFormModaliser.ShowDialogAndDispose(form);
		}

		public delegate void FileGeneratorCallback(Stream fileStream);

		public FileGeneratorCallback FileGenerator
		{
			get;
			set;
		}

		public string DefaultFileName
		{
			get { return defaultFileName; }
			set
			{
				defaultFileName = (PathValidation.GetSafeFilename(value));
				if (string.IsNullOrEmpty(EmailSubject))
				{
					EmailSubject = Path.GetFileName(defaultFileName);
				}
			}
		}
		string defaultFileName;

		public string EmailSubject
		{
			get;
			set;
		}

		public string EmailBody
		{
			get;
			set;
		}

		void emailButton_Click(object sender, EventArgs e)
		{
			var email = new EmailDef();
			var env = EnvProxy.Instance;
			email.Subject = EmailSubject;
			email.AddRecipientForUserCommunication(env.CurrentUser.EmailAddress);
			email.Body = EmailSubject;

			using (var tempFile = TempFile.New())
			{
				using (var tempFileStream = File.Create(tempFile.Filename))
				{
					FileGenerator(tempFileStream);
				}
				email.Attachments.Add(new AttachmentDef(Path.GetFileName(DefaultFileName), tempFile.Filename));
				env.OutgoingMailManager.CreateAndSave(email);
			}

			Globals.Message.Show(Res.GetString("dce2abf1-00b1-4a1b-ae74-5699738e10b3", "The document has been emailed as an attachment to {0}.", EnvProxy.Instance.CurrentUser.EmailAddress));
			Close();
		}

		void saveButton_Click(object sender, EventArgs e)
		{
			using (var dialog = new ZSaveFileDialog())
			{
				var extension = Path.GetExtension(DefaultFileName);
				if (extension.StartsWith("."))
				{
					extension = extension.Substring(1);
					dialog.AddExtension = true;
					dialog.Filter = "*." + extension + "|*." + extension;
					dialog.DefaultExt = extension;
				}
				dialog.FileName = Path.GetFileNameWithoutExtension(DefaultFileName);
				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					using (var fileStream = dialog.OpenFile())
					{
						FileGenerator(fileStream);
					}
					Globals.Message.Show(Res.GetString("60352311-402e-4fd1-b862-01f71c513b01", "The document was saved as {0}", dialog.UnmappedFileName));
				}
			}
			Close();
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}

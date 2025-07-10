using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DataTransfer.Business
{
	public class EmailExport : FileExport
	{
		public EmailExport(ExportInstructions instructions, INotifications notifications) : base(instructions, notifications)
		{
		}

		public override ExportType ExportType
		{
			get { return ExportType.Email; }
		}

		public override void Deliver(ZString savedExportFile)
		{
			base.Deliver(savedExportFile);
			string filename = ZString.Empty;
			if (IsReadyToSend)
			{
				filename = Instructions.OutputFile;
				EmailDef email = new EmailDef(GlbStaff.CurrentUser.PK.ToGuid());
				AttachmentDef fileToDeliver = new AttachmentDef(Path.GetFileName(filename), filename);
				email.Attachments.Add(fileToDeliver);
				email.AddRecipientForUserCommunication(Instructions.EmailProperties.Recipients);
				email.Subject = Instructions.EmailProperties.Subject;
				email.Body = Instructions.EmailProperties.Body;

				try
				{
					SendEmail(email);
				}
				catch (EmailSendFailedException e)
				{
					string error = Res.GetString("9addff38-05ec-4b33-83fd-3d5e7efd352b", "There was an error sending the email {0}: {1}", email.Subject, e.Message);
					Notifications.Notify(new ErrorNotification(ErrorType.ErrorSendingEmail, error));
				}
			}

			if (File.Exists(filename))
			{
				File.Delete(filename);
			}
			if (File.Exists(savedExportFile))
			{
				File.Delete(savedExportFile);
			}
		}

		protected
#if DEBUG
			virtual
#endif
			void SendEmail(EmailDef email)
		{
			Env.OutgoingMailManager.CreateAndSave(email);
		}

		public override bool CanDeliver
		{
			get { return IsRecipientListValid; }
		}

		bool IsReadyToSend
		{
			get { return IsAttachmentValid && IsRecipientListValid; }
		}

		bool IsAttachmentValid
		{
			get { return File.Exists(Instructions.OutputFile); }
		}

		bool IsRecipientListValid
		{
			get { return Instructions.EmailProperties.Recipients.Count > 0; }
		}

		protected override bool IsRemoteFile
		{
			get { return false; }
		}
	}
}

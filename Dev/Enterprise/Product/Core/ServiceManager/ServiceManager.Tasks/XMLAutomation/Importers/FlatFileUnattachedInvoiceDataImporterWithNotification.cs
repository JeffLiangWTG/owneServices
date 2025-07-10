using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class FlatFileUnattachedInvoiceDataImporterWithNotification : FlatFileUnattachedInvoiceDataImporter
	{
		public FlatFileUnattachedInvoiceDataImporterWithNotification(INotifications notifications, Guid notificationGroup, ZString fileName)
			: base(fileName)
		{
			this.LogEvent += Importer_LogEvent;
			this.NotificationGroup = notificationGroup;
			this.Notifications = new NotificationBuffer(notifications);
		}

		protected override void AfterImport(bool isImportSuccessful)
		{
			base.AfterImport(isImportSuccessful);
			SendImportNotificationEmail(isImportSuccessful);
		}

		void Importer_LogEvent(object sender, EventArgs e)
		{
			ZString message = !string.IsNullOrEmpty(LogEntry) ? LogEntry : Res.GetString("5c93d181-a816-414b-896f-d2f13d965560", "{0} records processed", ProcessedRecordCount);
			Notifications.Notify(new InfoNotification(message));
		}

		bool IsEmailToBeSentOnSuccess
		{
			get { return !SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.Value; }
		}

		protected virtual void SendImportNotificationEmail(bool isImportSuccessful)
		{
			try
			{
				if ((!isImportSuccessful) || (isImportSuccessful && IsEmailToBeSentOnSuccess))
				{
					EmailDef eMail = BuildEmailDef(isImportSuccessful);
					Env.OutgoingMailManager.CreateAndSave(eMail, NotificationGroup, GroupSourceLocator.GetFromGroup(factory.Load<MasterFiles.Business.GlbGroup>(NotificationGroup)));
				}
			}
			catch (EmailSendFailedException) { }
		}

		EmailDef BuildEmailDef(bool isImportSuccessful)
		{
			EmailDef eMail = new EmailDef();

			string importStatusAsString = isImportSuccessful ? Res.GetString("8c8a26a3-5c0e-4081-9fdc-22b94553d900", "Succeeded") : Res.GetString("852bc060-ecdc-4c5a-a2ac-1a6d2e019085", "Failed");
			eMail.Subject = Res.GetString("5f43f97a-d427-4142-ad2f-6cd3cd647b50", "Commercial Invoice Import {0}", importStatusAsString);
			eMail.Body = Notifications.AsString;

			if (File.Exists(FileName))
			{
				eMail.Subject += " - " + Path.GetFileName(FileName);
				if (!isImportSuccessful)
				{
					eMail.Attachments.Add(new AttachmentDef(FileName));
				}
			}

			return eMail;
		}

		readonly Guid NotificationGroup;
		readonly NotificationBuffer Notifications;
	}
}

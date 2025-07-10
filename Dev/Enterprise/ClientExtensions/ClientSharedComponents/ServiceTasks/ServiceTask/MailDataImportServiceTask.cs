using System;
using System.Globalization;
using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.ClientSharedComponents.ServiceTasks
{
	public abstract class MailDataImportServiceTask : ServiceProviderImpl
	{
		public const string MinimumPeriod = "30seconds";

		#region abstract members

		protected abstract ZGuid NotificationGroupPK
		{
			get;
		}

		protected abstract GuidRegistryItem NotificationGroupPKRegistryItem
		{
			get;
		}

		protected abstract ZString AttachmentFileExtension
		{
			get;
		}

		protected abstract DataImporter GetImporter();
		protected abstract IMailFilter GetMailFilter();

		#endregion

		#region overrides
		public override void RunTask(CancellationToken token)
		{
			var validMailItems = MailFilter.Load(Factory, -1);
			if (validMailItems.Length > 0)
			{
				Notify.Notify(new InfoNotification(string.Format(CultureInfo.InvariantCulture, "{0} email(s) found for import. Processing email(s)...", validMailItems.Length)));
				var emailCounter = 1;

				foreach (MailItem mailItem in validMailItems)
				{
					token.ThrowIfCancellationRequested();
					try
					{
						Notify.Notify(new InfoNotification(string.Format(CultureInfo.InvariantCulture, "{0}. Email received on {1} contains {2} attachment(s). Processing attachment(s)...", emailCounter, mailItem.MI_SystemCreateTimeUtc.ToLongTimeString(), mailItem.MailAttachments.Count)));

						if (mailItem.MailAttachments.Count == 0)
						{
							Notify.Notify(new ErrorNotification(ErrorType.Error, string.Format(CultureInfo.InvariantCulture, "Email from: {0} does not have an attachment.", mailItem.MI_From)));
						}

						foreach (MailAttachment attachment in mailItem.MailAttachments)
						{
							if (attachment.MA_FileName.ToUpper().EndsWith(AttachmentFileExtension.ToUpper(), StringComparison.OrdinalIgnoreCase))
							{
								using (var attachmentStream = new MemoryStream(attachment.MA_Data))
								using (var reader = new StreamReader(attachmentStream))
								{
									_ = Importer.ImportData(reader, attachment.MA_FileName, Notify, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.ClientSpecifiedImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, attachment.MA_FileName));
								}
							}
						}

						mailItem.MI_Status = MailStatus.Processed;
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						mailItem.MI_Status = MailStatus.Failed;
						Notify.Notify(new ErrorNotification(ErrorType.Error, string.Format(CultureInfo.InvariantCulture, "An error has occurred whilst processing email from: {0}" + System.Environment.NewLine + "Message: {1}", mailItem.MI_From, ex.Message)));
					}
					finally
					{
						PerformSave(mailItem);
						SendEmail(mailItem);
					}
				}
			}
		}

		#endregion

		#region Implementation

		void PerformSave(MailItem mail)
		{
			try
			{
				Factory.Save();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				BusinessObjectFactory newFactory = new();
				var newMail = newFactory.Load<MailItem>(mail.PK);
				newMail.MI_Status = MailStatus.Failed;
				newFactory.Save();
				throw;
			}
		}

		void SendEmail(MailItem mailItem)
		{
			if (!NotificationGroupPK.IsEmpty)
			{
				var notificationGroup = Factory.Load<GlbGroup>(NotificationGroupPK);

				if (notificationGroup != null)
				{
					var emailDef = new EmailDef();
					emailDef.Subject = "Automated Manifest Import Report";

					if (Notify.HasErrors)
					{
						emailDef.Subject = "ERROR: " + emailDef.Subject;
						var errorMsg = string.Format("Error(s) have occurred while processing email from {0}. ", mailItem.MI_From);
						emailDef.Body = errorMsg + "Please see below for further details:" + System.Environment.NewLine + System.Environment.NewLine;

						foreach (var currentEvent in Notify.Events)
						{
							if (currentEvent is ErrorNotification)
							{
								emailDef.Body += currentEvent.Message + System.Environment.NewLine;
							}
						}

						foreach (MailAttachment attachment in mailItem.MailAttachments)
						{
							_ = emailDef.Attachments.Add(new AttachmentDef(attachment.MA_FileName, attachment.MA_Data));
						}
					}
					else
					{
						emailDef.Subject = "SUCCESS: " + emailDef.Subject;
						var successMsg = string.Format("The following attachment(s) from {0} have been imported successfully.", mailItem.MI_From);
						emailDef.Body = successMsg + System.Environment.NewLine + System.Environment.NewLine;

						foreach (MailAttachment attachment1 in mailItem.MailAttachments)
						{
							emailDef.Body += attachment1.MA_FileName + System.Environment.NewLine;
						}
					}

					Env.OutgoingMailManager.CreateAndSave(emailDef, notificationGroup.PK.ToGuid(), GroupSourceLocator.GetFromRegistryItem(NotificationGroupPKRegistryItem));
				}
			}
		}

		#region MailFilter

		IMailFilter fMailFilter;
#if DEBUG
		public
#endif
		IMailFilter MailFilter => LazyInitializer.EnsureInitialized(ref fMailFilter, GetMailFilter);

		#endregion

		#region Importer
		DataImporter fImporter;

#if DEBUG
		public
#endif
		DataImporter Importer
		{
			get
			{
				return fImporter ??= GetImporter();
			}
		}
		#endregion

		#region Notify
#if DEBUG
		public
#endif
		NotificationBuffer Notify
		{
			get
			{
				return notify ??= new NotificationBuffer(ServiceLogger.GetTaskNotificationSubscriber());
			}
		}
		NotificationBuffer notify;
		#endregion

		#region Factory
		protected BusinessObjectFactory Factory
		{
			get
			{
				return factory ??= new BusinessObjectFactory();
			}
		}
		BusinessObjectFactory factory;
		#endregion

		#endregion
	}
}

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.YAS.Business.ProofOfDeliveryInterface
{
	public class PODImportProcessor
	{
		public void ExecuteForTest(INotifications notifications)
		{
			Execute(notifications, CancellationToken.None);
		}

		public void Execute(INotifications iNotifications, CancellationToken token)
		{
			notifications = iNotifications;
			try
			{
				EnsureEnvironmentOk();

				if (!YASDataRegistry.Instance.PODImportFolder.IsEmpty && Directory.Exists(YASDataRegistry.Instance.PODImportFolder))
				{
					DirectoryInfo directory = new DirectoryInfo(YASDataRegistry.Instance.PODImportFolder);
					FileInfo[] files = directory.GetFiles();
					foreach (FileInfo file in files)
					{
						token.ThrowIfCancellationRequested();
						ExecuteCore(file);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				iNotifications.Add(NotificationType.Error, string.Format(CultureInfo.InvariantCulture, "Finished import with errors: {0}\r\n\r\nStack Trace:{1}", ex.Message, ex.StackTrace));
			}
		}

		void ExecuteCore(FileInfo file)
		{
			notifications.Add(new InfoNotification("Starting import processing of file (" + file.FullName + ")."));
			try
			{
				if (!Importer.ImportData(file.FullName, notifications, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.ClientSpecifiedImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, file.FullName)))
				{
					if (Importer.buffer.Events.Where(notification => notification.Type is PODErrorType).Count() > 0)
					{
						AttachFileToEmailAndSend(file.FullName, Importer.buffer);
					}
				}
			}
			catch (IOException ex)
			{
				notifications.Add(NotificationType.Error, ex.Message + "\r\n" + ex.StackTrace);
				AttachFileToEmailAndSend(file.FullName, ex);
			}
			finally
			{
				notifications.Add(new InfoNotification("Finished import."));
				file.Delete();
			}
		}

		void EnsureEnvironmentOk()
		{
			ZStringBuilder exceptionMessage = new ZStringBuilder();

			if (YASDataRegistry.Instance.PODImportFolder.IsEmpty)
			{
				exceptionMessage.Append("Proof Of Delivery Import Folder is not set in the registry.  See System -> Registry -> " + YASDataRegistry.PODInterfaceCategory.Replace("/", " -> ") + " -> Proof Of Delivery Import Folder");
			}

			if (!Directory.Exists(YASDataRegistry.Instance.PODImportFolder))
			{
				exceptionMessage.Append("Proof Of Delivery Import Folder doesn't exist.  See System -> Registry -> " + YASDataRegistry.PODInterfaceCategory.Replace("/", " -> ") + " -> Proof Of Delivery Import Folder");
			}

			if (YASDataRegistry.Instance.PODEmailNotificationGroup.IsEmpty)
			{
				exceptionMessage.Append("No Proof Of Delivery email notification group has been set in the registry.  See System -> Registry -> " + YASDataRegistry.PODInterfaceCategory.Replace("/", " -> ") + " -> Email Notification Group");
			}

			if (!exceptionMessage.IsEmpty)
			{
				throw new InvalidOperationException(exceptionMessage.ToStringWithNewLineBetweenAppends());
			}
		}

		void AttachFileToEmailAndSend(ZString file1, INotifications notifications)
		{
			AttachFileToEmailAndSend(file1, null, notifications);
		}

		void AttachFileToEmailAndSend(ZString file1, Exception e)
		{
			AttachFileToEmailAndSend(file1, e, null);
		}

		void AttachFileToEmailAndSend(ZString file1, Exception e, INotifications notifications)
		{
			if (!YASDataRegistry.Instance.PODEmailNotificationGroup.IsEmpty)
			{
				EmailDef email = new EmailDef();
				try
				{
					email.Attachments.Add(new AttachmentDef(file1));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
				email.Subject = "There was a problem importing files ";

				ZStringBuilder emailBodyBuilder = new ZStringBuilder();
				emailBodyBuilder.AppendLine(ZString.Format("There was a problem importing {0}.", Path.GetFileName(file1)));

				if (notifications != null)
				{
					foreach (var notification in ((NotificationBuffer)notifications).Events.Where(notification => notification.Type is PODErrorType))
					{
						emailBodyBuilder.AppendLine(notification.Message);
					}
				}

				if (e != null)
				{
					emailBodyBuilder.Append(ZString.Format("{0}{1}{2}", e.Message, System.Environment.NewLine, e.StackTrace));
				}

				email.Body = emailBodyBuilder.ToString();
				Env.OutgoingMailManager.CreateAndSave(email, YASDataRegistry.Instance.PODEmailNotificationGroup.ToGuid(), GroupSourceLocator.GetFromRegistryItem(YASDataRegistry.Instance.PODEmailNotificationGroupItem));
			}
		}

		PODDataImporter Importer
		{
			get
			{
				if (importer == null)
				{
					importer = new PODDataImporter();
				}
				return importer;
			}
		}
		PODDataImporter importer;

		INotifications notifications;
	}
}

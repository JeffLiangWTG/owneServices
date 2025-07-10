using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Client.NIP.Business.ConsolAndShipmentImport;
using Enterprise.Client.NIP.ServiceTasks;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	NIPConsolAndShipmentServiceTask.Code,
	"Import Consol And Shipment Data",
	"CSP",
	typeof(NIPConsolAndShipmentServiceTask),
	MinimumPeriod = "30seconds",
	DefaultScheduleRunEvery = "30seconds"
	)]
namespace Enterprise.Client.NIP.ServiceTasks
{
	public class NIPConsolAndShipmentServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			INotifications notifications = ServiceLogger.GetTaskNotificationSubscriber();
			if (!EnvironmentSetUp)
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error, "Please set up registry settings: Admin -> System -> Registry -> NIP Client-Extensions -> Import of Consol + Shipment Data"));
			}
			else if (!Directory.Exists(NIPDataRegistry.Instance.ConsolAndShipmentImportDirectory.Value))
			{
				notifications.Notify(new ErrorNotification(ErrorType.Error, FormattableString.Invariant($"The directory '{NIPDataRegistry.Instance.ConsolAndShipmentImportDirectory.Value}' from the registry does not exist or is inaccessible. See: Admin -> System -> Registry -> NIP Client-Extensions -> Import of Consol + Shipment Data")));
			}
			else
			{
				notifications.Add(new InfoNotification("Start data import"));

				foreach (FileInfo fileInfo in GetNewFiles())
				{
					var shouldDeleteFile = true;
					youMustReactToThisToken.ThrowIfCancellationRequested();
					notifications.Add(new InfoNotification("Import processing of file (" + fileInfo.Name + ") started."));
					NotificationBuffer buffer = new NotificationBuffer(notifications);
					bool hasErrors = false;
					try
					{
						Importer.ImportData(fileInfo.FullName, notifications, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.ClientSpecifiedImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, fileInfo.FullName));
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						buffer.AddError(e.Message + System.Environment.NewLine + e.StackTrace);
						shouldDeleteFile = !(e is IOException);
						hasErrors = true;
					}
					finally
					{
						if (shouldDeleteFile)
						{
							DeleteFile(fileInfo, notifications);
						}
						if (hasErrors)
						{
							SendErrorNotificationEmail(fileInfo.Name, buffer);
							notifications.Add(new InfoNotification(string.Format(CultureInfo.InvariantCulture, "Import processing file failed for {0} and {1}", fileInfo.Name, shouldDeleteFile ? "will not retry later" : "will retry later")));
						}
						else
						{
							SendNotificationEmail(fileInfo.Name);
							notifications.Add(new InfoNotification("Import processing file complete"));
						}
					}
				}
				notifications.Add(new InfoNotification("Import finished"));
			}
		}

		#region Email processing

		void SendNotificationEmail(string fileName)
		{
			if (!((ZGuid)NIPDataRegistry.Instance.ConsolAndShipmentImportNotificationGroup.Value).IsEmpty)
			{
				EmailDef email = new EmailDef();

				email.Subject = "Consol and Shipment data import successful";
				email.Body = "File " + Path.GetFileName(fileName) + System.Environment.NewLine + " imported successfully";
				Env.OutgoingMailManager.CreateAndSave(email, NIPDataRegistry.Instance.ConsolAndShipmentImportNotificationGroup.Value,
					GroupSourceLocator.GetFromRegistryItem(NIPDataRegistry.Instance.ConsolAndShipmentImportNotificationGroup));
			}
		}

		void SendErrorNotificationEmail(string fileName, NotificationBuffer buffer)
		{
			if (!((ZGuid)NIPDataRegistry.Instance.ConsolAndShipmentImportNotificationGroup.Value).IsEmpty)
			{
				EmailDef email = new EmailDef();
				try
				{
					email.Attachments.Add(new AttachmentDef(fileName));
				}
				catch (Exception ex) when (!ex.IsCriticalException()) { }
				email.Subject = "Consol and Shipment data import failed";
				email.Body = GetErrorNotifications(buffer);
				Env.OutgoingMailManager.CreateAndSave(email, NIPDataRegistry.Instance.ConsolAndShipmentImportNotificationGroup.Value,
					GroupSourceLocator.GetFromRegistryItem(NIPDataRegistry.Instance.ConsolAndShipmentImportNotificationGroup));
			}
		}

		ZString GetErrorNotifications(NotificationBuffer notifications)
		{
			ZStringBuilder builder = new ZStringBuilder();
			foreach (INotification current in notifications.GetEventsByType(ErrorType.Error))
			{
				builder.Append(current.Message);
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}

		#endregion

		#region File processing

		void DeleteFile(FileInfo fileInfo, INotifications notifications)
		{
			try
			{
				fileInfo.Delete();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				notifications.Notify(new WarningNotification(WarningType.Warning, "Unable to delete file - " + fileInfo.Name + ". Error description - " + e.Message));
			}
		}

		protected virtual FileInfo[] GetNewFiles()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(NIPDataRegistry.Instance.ConsolAndShipmentImportDirectory.Value);

			return directoryInfo.GetFiles();
		}

		#endregion

		#region Environment Set Up

		ZBool EnvironmentSetUp
		{
			get
			{
				return !((ZString)NIPDataRegistry.Instance.ConsolAndShipmentImportDirectory.Value).IsEmpty
					   && !((ZGuid)NIPDataRegistry.Instance.ConsolAndShipmentImportNotificationGroup.Value).IsEmpty;
			}
		}

		#endregion

		#region Importer

		DataImporter Importer
		{
			get
			{
				importer = importer ?? new NIPConsolAndShipmentDataImporter();
				return importer;
			}
		}
		DataImporter? importer;

		#endregion

		public const string Code = "ZN1";
	}
}

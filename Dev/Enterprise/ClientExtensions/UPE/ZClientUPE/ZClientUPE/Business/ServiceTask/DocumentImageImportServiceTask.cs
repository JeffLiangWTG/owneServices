using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.DocumentImaging;
using Enterprise.Client.UPE.ServiceTask;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("ZU5", "Document Image Importer", "CSP",
	typeof(DocumentImageImportServiceTask),
	MinimumPeriod = "1minute",
	MaximumPeriod = "240minutes",
	DefaultScheduleRunEvery = "30minutes"
	)]
namespace Enterprise.Client.UPE.ServiceTask
{
	public class DocumentImageImportServiceTask : UPEServiceTask
	{
		public DocumentImageImportServiceTask()
		{
		}

		public DocumentImageImportServiceTask(ILogger logger)
			: base(logger)
		{
		}

		#region Execute

		protected bool IsEnvironmentDataValid()
		{
			bool result = true;
			if (new BusinessObjectFactory().Load<GlbGroup>(UPEDataRegistry.Instance.DocumentImagingNotificationGroup.Value) == null)
			{
				ServiceLogger.Log(LogType.Error, documentImageNotificationGroupMessage);
				result = false;
			}
			return result;
		}

		const string documentImageNotificationGroupMessage = "You must set the 'Document Imaging Notification Group' in the registry to a valid staff group";

		public override void RunTask(CancellationToken token)
		{
			var branch = UPETools.Instance.UPECustomisationBranches(false).FirstOrDefault();
			if (branch != null)
			{
				using (branch.SetAsTemporaryContext())
				{
					if (!IsEnvironmentDataValid())
					{
						Notifications.Notify(new ErrorNotification(ErrorType.Error, documentImageNotificationGroupMessage));
						return;
					}

					Notifications.Notify(new InfoNotification("Purging old Documents..."));
					int documentsPurged = DocumentPurger.PurgeDocuments(token);
					Notifications.Notify(new InfoNotification(documentsPurged + " document(s) were purged."));

					Notifications.Notify(new InfoNotification("Starting Importing Documents"));
					DocumentImporter.ExecuteBatch(token);
					Notifications.Notify(new InfoNotification("Finished Importing Documents"));
				}
			}
		}

		DocumentImagePurger DocumentPurger
		{
			get { return documentPurger ?? (documentPurger = new DocumentImagePurger()); }
		}
		DocumentImagePurger documentPurger;

		DocumentImageImporter DocumentImporter
		{
			get { return documentImporter ?? (documentImporter = new DocumentImageImporter(Notifications)); }
		}
		DocumentImageImporter documentImporter;

		#endregion
	}
}

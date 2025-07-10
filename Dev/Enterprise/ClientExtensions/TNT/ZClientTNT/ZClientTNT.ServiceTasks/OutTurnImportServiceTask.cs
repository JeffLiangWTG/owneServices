using System.IO;
using System.Threading;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Client.TNT;
using Enterprise.Client.TNT.OutTurnDataImport;
using Enterprise.Client.TNT.ServiceTasks;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	TNTConstants.OutTurnFileImportSrvTaskCode,
	"Outturn Files Import",
	"CSP",
	typeof(OutTurnImportServiceTask),
	MinimumPeriod = "30seconds",
	DefaultScheduleRunEvery = "30seconds"
	)]
namespace Enterprise.Client.TNT.ServiceTasks
{
	class OutTurnImportServiceTask : TNTServiceTask
	{
		protected override void Execute(NotificationBuffer notify, CancellationToken token)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(TNTDataRegistry.Instance.OutTurnFileSourceDirectory);
			FileInfo[] files = directoryInfo.GetFiles("*." + TNTConstants.OuturnFileExtension);

			foreach (FileInfo file in files)
			{
				token.ThrowIfCancellationRequested();
				notify.Notify(new InfoNotification("Processing " + file.FullName));

				OutTurnDataImporter importer = new OutTurnDataImporter();
				importer.ImportData(file.FullName, notify, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.ClientSpecifiedImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, file.FullName));

				ProcessedFileMover mover = new ProcessedFileMover(TNTDataRegistry.Instance.OutTurnFileProcessedDirectory);
				mover.Move(file);
			}

			if (files.Length > 0)
			{
				notify.SendEmail(TNTConstants.NotificationEmailSubjectForOutTurnFiles, TNTConstants.DataImportNotificationGroupCode);
			}
		}

		protected override bool IsEnvironmentValid
		{
			get
			{
				return !TNTDataRegistry.Instance.OutTurnFileProcessedDirectory.IsEmpty &&
					Directory.Exists(TNTDataRegistry.Instance.OutTurnFileProcessedDirectory) &&
					!TNTDataRegistry.Instance.OutTurnFileSourceDirectory.IsEmpty &&
					Directory.Exists(TNTDataRegistry.Instance.OutTurnFileSourceDirectory);
			}
		}

		protected override ZString RegistriesNotSetErrMesg
		{
			get { return ErrorMessage; }
		}
		internal const string ErrorMessage = "Outturn Files Import Registry Items are not set or invalid. Please verify the values in Admin-> Registry-> TNT Client Extensions-> Outturn Files";
	}
}

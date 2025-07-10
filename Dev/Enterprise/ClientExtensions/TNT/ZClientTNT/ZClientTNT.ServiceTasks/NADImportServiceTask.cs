using System.Globalization;
using System.IO;
using System.Threading;
using CargoWise.Types;
using Enterprise.Client.TNT;
using Enterprise.Client.TNT.NADDataImport;
using Enterprise.Client.TNT.ServiceTasks;
using Enterprise.ZArchitecture;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	TNTConstants.NADFileImportSrvTaskCode,
	"NAD Files Import",
	"CSP",
	typeof(NADImportServiceTask),
	MinimumPeriod = "30seconds",
	DefaultScheduleRunEvery = "30seconds"
	)]
namespace Enterprise.Client.TNT.ServiceTasks
{
	class NADImportServiceTask : TNTServiceTask
	{
		protected override void Execute(NotificationBuffer notify, CancellationToken token)
		{
			notify.Notify(new InfoNotification("Searching " + TNTDataRegistry.Instance.NADFileSourceDirectory + " for NAD files to process..."));
			DirectoryInfo info = new DirectoryInfo(TNTDataRegistry.Instance.NADFileSourceDirectory);
			FileInfo[] allFiles = info.GetFiles();
			FileInfo[] nADFiles = info.GetFiles("*." + TNTDataRegistry.Instance.NADFileExtension);

			if (nADFiles.Length > 0)
			{
				if (allFiles.Length > nADFiles.Length)
				{
					notify.Notify(new InfoNotification(allFiles.Length.ToString(CultureInfo.InvariantCulture) + " file(s) found in directory but " + (allFiles.Length - nADFiles.Length).ToString(CultureInfo.InvariantCulture) + " file(s) determined as NOT NAD files"));
				}
				else
				{
					notify.Notify(new InfoNotification(nADFiles.Length.ToString(CultureInfo.InvariantCulture) + " NAD file(s) found in directory"));
				}

				NADDataImporter dataImporter = new NADDataImporter(notify);
				dataImporter.ProcessFiles(nADFiles, token);
			}
		}

		protected override bool IsEnvironmentValid
		{
			get
			{
				return !TNTDataRegistry.Instance.NADFileExtension.IsEmpty &&
					!TNTDataRegistry.Instance.NADFileProcessedDirectory.IsEmpty && Directory.Exists(TNTDataRegistry.Instance.NADFileProcessedDirectory) &&
					!TNTDataRegistry.Instance.NADFileSourceDirectory.IsEmpty && Directory.Exists(TNTDataRegistry.Instance.NADFileSourceDirectory);
			}
		}

		protected override ZString RegistriesNotSetErrMesg
		{
			get { return ErrorMessage; }
		}

		internal const string ErrorMessage = "NAD Files Import Registry Items are not set or invalid. Please verify the values in Admin-> Registry-> TNT Client Extensions-> NAD Files";
	}
}

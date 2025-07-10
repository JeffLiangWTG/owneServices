using System.Linq;
using System.Threading;
using CargoWise.Data;
using Enterprise.DocumentScanning.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	EDocsAllContentToExternalStorageProcessingTask.ServiceTaskCode,
	EDocsAllContentToExternalStorageProcessingTask.ServiceTaskDescription,
	"DOC",
	typeof(EDocsAllContentToExternalStorageProcessingTask),
	AllowsMultipleInstances = true,
	MinimumPeriod = "1hour",
	MaximumPeriod = "3months",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true)]

namespace Enterprise.DocumentScanning.ServiceTasks
{
	public class EDocsAllContentToExternalStorageProcessingTask : BaseEDocsContentExternalStorageTask
	{
		public const string ServiceTaskCode = "DES";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const string for the task description.")]
		public const string ServiceTaskDescription = "All External Storage Processing Task";

		[HostedServiceRequirement]
		public static string CheckEDocsStorageProvider() => HostedServiceRequirementAttribute.CheckValueIsEqualTo(SystemDataRegistry.Instance.EDocsStorageProvider, Core.Constants.EDocsStorageProviders.Code.S3);

		[HostedServiceRequirement]
		public static string CheckEDocsDatabase() => (Db.Connection.GetDatabases(DatabaseType.SD).Count() == 1) ? EDocsAllContentToExternalStorageProcessor.ServiceNotRequiredMessage : string.Empty;

		protected override void RunTaskCore(CancellationToken token)
		{
			if (!CanAccessExternalStorage)
			{
				ServiceLogger.Error(ServiceStoppedNoAccessToExternalStorageMessage);
				return;
			}

			Processor ??= new EDocsAllContentToExternalStorageProcessor();

			var branch = GlbBranch.GetOneActiveBranchPerCompany().FirstOrDefault();

			if (branch != null)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					Processor.Run(ServiceLogger, token);
				}
			}
		}

		internal EDocsAllContentToExternalStorageProcessor Processor { get; set; }
	}
}

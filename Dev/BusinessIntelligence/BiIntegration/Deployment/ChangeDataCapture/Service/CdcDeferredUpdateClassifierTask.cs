using System.Threading;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.ChangeDataCapture.Service.CdcDeferredUpdateClassifierTask.ServiceTaskCode, Enterprise.ChangeDataCapture.Service.CdcDeferredUpdateClassifierTask.ServiceTaskName, "BI",
	typeof(Enterprise.ChangeDataCapture.Service.CdcDeferredUpdateClassifierTask),
	CanRunInAnyBranch = true,
	IsMandatory = true,
	MinimumPeriod = "1seconds",
	MaximumPeriod = "1minute",
	AllowsMultipleInstances = false,
	DefaultScheduleRunEvery = "30seconds",
	ActiveByDefault = true
	)
]

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(CargoWise.Bi.Product.ServiceTask.AuditEtlExecutionTask))]

namespace Enterprise.ChangeDataCapture.Service
{
	public class CdcDeferredUpdateClassifierTask : ServiceProviderImpl, IBiNotificationSource
	{
		public const string ServiceTaskCode = "DUC";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Name")]
		public const string ServiceTaskName = "Change Data Capture - Deferred Update Classifier service";
		public bool isHostedWithCargoWise = EnvProxy.IsHostedWithCargowise;

		#region SuppressResourceStringsCheckRegion
		#region IBiNotificationSource Members

		string IBiNotificationSource.Code => ServiceTaskCode;
		string IBiNotificationSource.Description => ServiceTaskName;

		#endregion

		[HostedServiceRequirement]
		public static string CheckCdcIsEnabled() => CdcDatabase.CheckIsEnabled();

		[HostedServiceRequirement]
		public static string HasBeenFullyImplemented() => "Remove t̲h̲i̲s̲ when finished.";

		public CdcDeferredUpdateClassifierTask(DbConnection biConnection, ILogger logger)
		{
			this.biConnection = biConnection;
			this.logger = logger;
		}
		readonly DbConnection biConnection;
		readonly ILogger logger;

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			Run(true, youMustReactToThisToken);
		}

		#region CDC
		void Run(bool retryIfFail, CancellationToken token)
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(Db.AuditDatabaseName))
			{
				logger.Log(LogType.Debug, "Logic hasn't been implemented yet");
			}
		}

		#endregion

		#endregion
	}
}

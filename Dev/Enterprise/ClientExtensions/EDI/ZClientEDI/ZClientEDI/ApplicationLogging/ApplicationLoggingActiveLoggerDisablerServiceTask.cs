using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.ApplicationLogging;
using Enterprise.Client.EDI.ApplicationLogging.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ApplicationLoggingActiveLoggerDisablerServiceTask.Code,
	ApplicationLoggingActiveLoggerDisablerServiceTask.Description,
	"CSP",
	typeof(ApplicationLoggingActiveLoggerDisablerServiceTask),
	MinimumPeriod = "15minutes",
	MaximumPeriod = "4hours",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1hour",
	IsMandatory = true,
	ActiveByDefault = true
	)]

namespace Enterprise.Client.EDI.ApplicationLogging
{
	class ApplicationLoggingActiveLoggerDisablerServiceTask : ServiceProviderImpl
	{
		public const string Code = "ALD";
		public const string Description = "Application Logging Active Logger Disabler";

		public override void RunTask(CancellationToken cancellationToken)
		{
			var activeLoggers = Factory.Load<ApplicationActiveLogger>(ApplicationActiveLoggerQuery);

			foreach (var activeLogger in activeLoggers)
			{
				cancellationToken.ThrowIfCancellationRequested();
				ServiceLogger.Log(LogType.Information, $"The {activeLogger.ApplicationLogger.ALG_Name} Active Logger expired on {activeLogger.AAL_ActiveUntil} and has been deactivated.");
				activeLogger.AAL_ActiveUntil = ZDateTimeOffset.Empty;
			}

			Factory.Save();
		}

		ZQuery ApplicationActiveLoggerQuery
		{
			get
			{
				return new ZQuery(ApplicationActiveLoggerSchema.AAL_ActiveUntil, SQLComparisonOperator.NotEqual, null)
					.AddToFilter(ApplicationActiveLoggerSchema.AAL_ActiveUntil, SQLComparisonOperator.LessThanOrEqualTo, ZDateTimeOffset.Now);
			}
		}

		BusinessObjectFactory Factory => factory ??= new BusinessObjectFactory();
		BusinessObjectFactory factory;
	}
}

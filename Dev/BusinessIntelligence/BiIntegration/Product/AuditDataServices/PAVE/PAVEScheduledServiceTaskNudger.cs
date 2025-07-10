using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.PAVE;
using Enterprise.Integration;
using Enterprise.TimeEngineScheduler.Integration;
using ServiceManager.Integration.Abstractions;

[assembly: SchedulerAction(
		PAVEScheduledServiceTaskNudger.Code,
		PAVEScheduledServiceTaskNudger.Description,
		typeof(PAVEScheduledServiceTaskNudger))]
namespace Enterprise.AuditDataServices.PAVE
{
	public class PAVEScheduledServiceTaskNudger : ISchedulerAction
	{
		public const string Code = "STN";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "used for service task logging")]
		public const string Description = "PAVE Scheduled Service Task Nudger";

		public string Execute(CancellationToken token, BusinessObjectFactory factory, ILogger logger, ZGuid targetPk, string targetCode, string parameter, ZDateTime systemCreateTimeUtc)
		{
			var nudger = ObjectFactory.Get<IServiceTaskNudger>();
			nudger.NudgeServiceTask(parameter);
			logger.Information($"Nudged the {parameter} service task.");
			return Constants.SchedulerActionSuccessResult;
		}
	}
}

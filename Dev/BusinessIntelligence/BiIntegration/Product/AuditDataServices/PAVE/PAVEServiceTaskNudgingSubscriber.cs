using CargoWise.Application;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.TimeEngineScheduler.Integration;

namespace Enterprise.AuditDataServices.PAVE
{
	public abstract class PAVEServiceTaskNudgingSubscriber : ActualDataChangesAuditSubscriber
	{
		protected void MaybeNudgeServiceTask(ILogger logger, bool shouldNudge, string serviceTaskCode, int delay)
		{
			if (shouldNudge)
			{
				var actionScheduleProvider = ObjectFactory.Get<IActionScheduleProvider>();
				var executionTime = ZDateTime.UtcNow.AddMinutes(delay);
				var schedule = actionScheduleProvider.ScheduleActionIfNotScheduled(PAVEScheduledServiceTaskNudger.Code,
					executionDateTimeUtc: executionTime,
					targetPk: ZGuid.Empty,
					targetTableCode: "n/a",
					jsonParameter: serviceTaskCode);

				if (schedule.NonPersistentSchedulingState == TimeActionSchedulingState.Scheduled)
				{
					if (delay == 0)
					{
						var message = $"Scheduled nudging the {serviceTaskCode} service task immediately (at {schedule.ExecutionDateTimeUtc}).";
						logger.Log(LogType.Information, $"Scheduled nudging the {serviceTaskCode} service task immediately (at {schedule.ExecutionDateTimeUtc}).");
					}
					else
					{
						logger.Log(LogType.Information, $"Scheduled nudging the {serviceTaskCode} service task with a {delay} minute delay (at {schedule.ExecutionDateTimeUtc}).");
					}
				}
				else if (schedule.NonPersistentSchedulingState == TimeActionSchedulingState.Loaded)
				{
					logger.Log(LogType.Information, $"No need to schedule nudging the {serviceTaskCode} service task as it is already scheduled at {schedule.ExecutionDateTimeUtc}.");
				}
			}
			else
			{
				logger.Log(LogType.Debug, $"Skipped nudging the {serviceTaskCode} service task as disabled in the registry.");
			}
		}

		protected IBMSRegistry BMSRegistry => bmsRegistry ?? (bmsRegistry = ObjectFactory.Get<IBMSRegistry>());
		IBMSRegistry bmsRegistry;
	}
}

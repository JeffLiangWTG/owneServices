using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.TimeEngineScheduler.Business;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.PAVE.Subscribers
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Message")]
	public abstract class BMComponentRelatedResponsiveWorkflowUpdateSubscriberBase : ActualDataChangesAuditSubscriber
	{
		protected bool MaybeScheduleWorkflowsResponsiveUpdate(ILogger logger, string logMessage, Guid componentPK, string operationType, BusinessObjectFactory scheduleFactory, ReadOnlyBusinessObjectFactory readonlyFactory)
		{
			var component = LoadComponentOrLogComponentDeleted(readonlyFactory, componentPK, logger, logMessage);

			if (component == null)
			{
				return false;
			}

			var system = readonlyFactory.Load<IBMSystem>(component.FC_FS_System);

			if (system == null)
			{
				ErrorReporter.ReportOnce("Something's wrong: a buffer management system cannot be deleted without deleting its components, and still a buffer management system does not exists whereas its component does exist.");
				return false;
			}

			var operationDesc = operationType == ProcessHeaderResponsiveActionConstants.UpdateDedicatedBufferEffectiveBranchAndDepartment
				? "both dedicated buffers and effective branches and departments"
				: operationType == ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer
					? "dedicated buffers"
					: operationType == ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment
						? "effective branches and departments"
						: throw new InvalidOperationException("Unexpected responsive update operation");
			operationDesc += $" ({operationType} responsive action)";

			var schedule = ActionScheduleProvider.ScheduleOrRescheduleAction(ProcessHeaderResponsiveActionConstants.ProcessHeaderResponsiveUpdateSchedulerActionCode,
				ZDateTime.UtcNow,
				targetPk: componentPK,
				targetTableCode: BMComponentSchema.Constants.Prefix,
				jsonParameter: operationType,
				token: system.PK.ToString(),
				scheduleSuspended: !system.FS_IsLive,
				factory: scheduleFactory);
			logger.Log(LogType.Information, $"{logMessage}: {(schedule.NonPersistentSchedulingState == TimeActionSchedulingState.Rescheduled ? "rescheduled" : "scheduled")} responsive update of {operationDesc} on workflows situated in component {GetComponentFullyQualifiedDisplayName(component, system)} with status {schedule.ExecutionStatus}.");
			return true;
		}

		protected static IBMComponent LoadComponentOrLogComponentDeleted(ReadOnlyBusinessObjectFactory readonlyFactory, Guid componentPK, ILogger logger, string logMessage)
		{
			var component = readonlyFactory.Load<IBMComponent>(componentPK);

			if (component == null)
			{
				// the user cannot remove a component when there are workflows in it
				// if the component is removed, it means there are no workflows to update
				logger.Log(LogType.Debug, $"{logMessage}: no need to schedule responsive update of workflows as component with PK = {componentPK} does not exist.");
				return null;
			}

			return component;
		}

		protected string GetComponentDisplayName(IBMComponent component)
		{
			var name = component != null ? (string)component.FC_Name : "<deleted>";
			return $"{name} (PK = {component.PK})";
		}

		protected string GetComponentFullyQualifiedDisplayName(IBMComponent component, IBMSystem system)
		{
			return $"{GetComponentDisplayName(component)} of system {system.FS_Name} (IsLive = {system.FS_IsLive})";
		}

		ActionScheduleProvider ActionScheduleProvider => actionScheduleProvider ?? (actionScheduleProvider = new ActionScheduleProvider());
		ActionScheduleProvider actionScheduleProvider;
	}
}

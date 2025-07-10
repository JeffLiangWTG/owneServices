using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TimeEngineScheduler.Business
{
	public class ActionScheduleProvider : IActionScheduleProvider
	{
		public ZQuery GetRunnableSchedulesQuery()
		{
			var filter = new ZQuery(TimeActionScheduleSchema.TAS_ExecutionDateTimeUtc, SQLComparisonOperator.LessThan, ZDateTime.UtcNow);
			filter.AddToFilter(TimeActionScheduleSchema.TAS_ExecutionStatus, new[] { Constants.TimeActionScheduleStatus.Scheduled });
			filter.OrderBy = TimeActionScheduleSchema.Constants.TAS_ExecutionDateTimeUtc;
			return filter;
		}

		public IReadOnlyCollection<IActionSchedule> GetSchedules(BusinessObjectFactory factory, string actionCode, ZGuid targetPk, string targetTableCode, string jsonParameter = null, ZDateTime? scheduledLaterThanDateTimeUtc = null, int? batchSize = null)
		{
			var filter = new ZQuery(TimeActionScheduleSchema.TAS_ActionCode, actionCode);
			filter.AddToFilter(TimeActionScheduleSchema.TAS_TargetPK, targetPk);
			filter.AddToFilter(TimeActionScheduleSchema.TAS_TargetTableCode, targetTableCode);

			if (jsonParameter != null)
			{
				filter.AddToFilter(TimeActionScheduleSchema.TAS_JsonParameter, jsonParameter);
			}

			if (scheduledLaterThanDateTimeUtc.HasValue)
			{
				filter.AddToFilter(TimeActionScheduleSchema.TAS_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThan, scheduledLaterThanDateTimeUtc.Value);
			}

			if (batchSize.HasValue)
			{
				filter.MaximumRows = batchSize.Value;
			}
			return factory.Load<TimeActionSchedule>(filter);
		}

		public IActionSchedule ScheduleAction(string actionCode,
			ZDateTime executionDateTimeUtc,
			ZGuid targetPk,
			string targetTableCode,
			string jsonParameter = null,
			ZGuid? executionBranchPk = null,
			ZGuid? executionDepartmentPk = null,
			string token = null,
			bool scheduleSuspended = false,
			BusinessObjectFactory factory = null)
		{
			var shouldSave = factory == null;
			factory = factory ?? new BusinessObjectFactory() { RefreshEnabled = false };
			var schedule = factory.New<TimeActionSchedule>();
			schedule.TAS_ExecutionStatus = scheduleSuspended ? Constants.TimeActionScheduleStatus.Suspended : Constants.TimeActionScheduleStatus.Scheduled;
			schedule.TAS_ActionCode = actionCode;
			schedule.TAS_ExecutionDateTimeUtc = executionDateTimeUtc;
			schedule.TAS_TargetPK = targetPk;
			schedule.TAS_TargetTableCode = targetTableCode;
			schedule.TAS_JsonParameter = jsonParameter ?? string.Empty;
			schedule.TAS_GB_Branch = executionBranchPk != null ? executionBranchPk.Value : Env.CurrentBranchPK;
			schedule.TAS_GE_Department = executionDepartmentPk != null ? executionDepartmentPk.Value : Env.CurrentDepartmentPK;
			schedule.TAS_Token = token;
			schedule.NonPersistentSchedulingState = TimeActionSchedulingState.Scheduled;

			if (shouldSave)
			{
				factory.Save();
			}

			return schedule;
		}

		public IActionSchedule ScheduleOrRescheduleAction(string actionCode,
			ZDateTime executionDateTimeUtc,
			ZGuid targetPk,
			string targetTableCode,
			string jsonParameter = null,
			ZGuid? executionBranchPk = null,
			ZGuid? executionDepartmentPk = null,
			string token = null,
			bool scheduleSuspended = false,
			BusinessObjectFactory factory = null)
		{
			return ScheduleOrRescheduleActionIfNeeded(actionCode, executionDateTimeUtc, targetPk, targetTableCode, jsonParameter, executionBranchPk, executionDepartmentPk, token, scheduleSuspended, rescheduleIfScheduled: true, factory);
		}

		public IActionSchedule ScheduleActionIfNotScheduled(string actionCode,
			ZDateTime executionDateTimeUtc,
			ZGuid targetPk,
			string targetTableCode,
			string jsonParameter = null,
			ZGuid? executionBranchPk = null,
			ZGuid? executionDepartmentPk = null,
			string token = null,
			bool scheduleSuspended = false,
			BusinessObjectFactory factory = null)
		{
			return ScheduleOrRescheduleActionIfNeeded(actionCode, executionDateTimeUtc, targetPk, targetTableCode, jsonParameter, executionBranchPk, executionDepartmentPk, token, scheduleSuspended, rescheduleIfScheduled: false, factory);
		}

		IActionSchedule ScheduleOrRescheduleActionIfNeeded(string actionCode,
			ZDateTime executionDateTimeUtc,
			ZGuid targetPk,
			string targetTableCode,
			string jsonParameter,
			ZGuid? executionBranchPk,
			ZGuid? executionDepartmentPk,
			string token,
			bool scheduleSuspended,
			bool rescheduleIfScheduled,
			BusinessObjectFactory factory)
		{
			IActionSchedule schedule = null;

			ZExceptionReporting.ProcessWithConcurrencyHandling(() => // if when we reschedule, the TAS service task executes the action being rescheduled, we try again 
			{
				var shouldSave = factory == null;
				factory = factory ?? new BusinessObjectFactory() { RefreshEnabled = false };

				var scheduledActions = GetSchedules(factory, actionCode, targetPk, targetTableCode, jsonParameter)
					.Where(s => s.ExecutionStatus != Constants.TimeActionScheduleStatus.Closed && s.ExecutionStatus != Constants.TimeActionScheduleStatus.Failed)
					.ToArray();

				schedule = null;

				if (scheduledActions.Length == 1)
				{
					schedule = scheduledActions[0];
				}
				else if (scheduledActions.Length > 1)
				{
					var builder = new ZStringBuilder((NoResString)"Found more than one scheduled action while trying to reschedule:");

					foreach (var scheduledAction in scheduledActions)
					{
						builder.Append(scheduledAction.ToString());
					}
					ErrorReporter.ReportOnce("ActionScheduleProvider.ScheduleOrRescheduleAction", builder.ToStringWithNewLineBetweenAppends());
				}

				if (schedule != null)
				{
					if (rescheduleIfScheduled)
					{
						schedule.ExecutionDateTimeUtc = executionDateTimeUtc;

						if (executionBranchPk.HasValue)
						{
							schedule.ExecutionBranch = executionBranchPk.Value;
						}

						if (executionDepartmentPk.HasValue)
						{
							schedule.ExecutionDepartment = executionDepartmentPk.Value;
						}

						schedule.Token = token;
						schedule.ExecutionStatus = scheduleSuspended ? Constants.TimeActionScheduleStatus.Suspended : Constants.TimeActionScheduleStatus.Scheduled;
						((TimeActionSchedule)schedule).NonPersistentSchedulingState = TimeActionSchedulingState.Rescheduled;
					}
					else
					{
						shouldSave = false;
					}
				}
				else
				{
					schedule = ScheduleAction(actionCode, executionDateTimeUtc, targetPk, targetTableCode, jsonParameter, executionBranchPk, executionDepartmentPk, token, scheduleSuspended, factory);
				}

				if (shouldSave)
				{
					factory.Save();
				}
			}, onRetry: () => { });
			return schedule;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "It's faster to do one single database hit.")]
		public void ChangeStatusByToken(string token, string newStatus)
		{
			string sql = @"
UPDATE dbo.TimeActionSchedule
SET TAS_ExecutionStatus = @NewStatus
WHERE TAS_Token = @Token
	AND TAS_ExecutionStatus IN ('SCH', 'SUS')
";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@Token", token, TimeActionScheduleSchema.TAS_Token);
				cmd.AddParameterBasedOnDbColumn("@NewStatus", newStatus, TimeActionScheduleSchema.TAS_ExecutionStatus);

				cmd.ExecuteNonQuery();
			}
		}

		public void ChangeState(IActionSchedule schedule)
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var actualSchedule = factory.Load<TimeActionSchedule>(schedule.PK);
			actualSchedule.TAS_ExecutionStatus = schedule.ExecutionStatus;
			actualSchedule.TAS_ExecutionResult = schedule.ExecutionResult;
			actualSchedule.TAS_RetryAttempts = schedule.RetryAttempts;
			actualSchedule.TAS_ExecutionDateTimeUtc = schedule.ExecutionDateTimeUtc;
			actualSchedule.TAS_GB_Branch = schedule.ExecutionBranch;
			actualSchedule.TAS_GE_Department = schedule.ExecutionDepartment;
			actualSchedule.TAS_Token = schedule.Token;

			factory.Save();
		}
	}
}

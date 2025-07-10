using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.TimeEngineScheduler.Integration
{
	public interface IActionScheduleProvider
	{
		IActionSchedule ScheduleAction(string actionCode,
			ZDateTime executionDateTimeUtc,
			ZGuid targetPk,
			string targetTableCode,
			string jsonParameter = null,
			ZGuid? executionBranchPk = null,
			ZGuid? executionDepartmentPk = null,
			string token = null,
			bool scheduleSuspended = false,
			BusinessObjectFactory factory = null);

		/// <summary>
		/// Schedules an action or reschedules it if the same action is already scheduled.
		/// Actions are considered the same when they have the same parameters:
		/// - TAS_TargetPK
		/// - TAS_TargetTableCode
		/// - TAS_ActionCode
		/// - TAS_JsonParameters
		/// </summary>
		public IActionSchedule ScheduleOrRescheduleAction(string actionCode,
			ZDateTime executionDateTimeUtc,
			ZGuid targetPk,
			string targetTableCode,
			string jsonParameter = null,
			ZGuid? executionBranchPk = null,
			ZGuid? executionDepartmentPk = null,
			string token = null,
			bool scheduleSuspended = false,
			BusinessObjectFactory factory = null);

		/// <summary>
		/// Schedules an action if the same action was not already scheduled.
		/// Actions are considered the same when they have the same parameters:
		/// - TAS_TargetPK
		/// - TAS_TargetTableCode
		/// - TAS_ActionCode
		/// - TAS_JsonParameters
		/// </summary>
		public IActionSchedule ScheduleActionIfNotScheduled(string actionCode,
			ZDateTime executionDateTimeUtc,
			ZGuid targetPk,
			string targetTableCode,
			string jsonParameter = null,
			ZGuid? executionBranchPk = null,
			ZGuid? executionDepartmentPk = null,
			string token = null,
			bool scheduleSuspended = false,
			BusinessObjectFactory factory = null);

		void ChangeStatusByToken(string token, string newStatus);

		void ChangeState(IActionSchedule schedule);

		ZQuery GetRunnableSchedulesQuery();

		IReadOnlyCollection<IActionSchedule> GetSchedules(BusinessObjectFactory factory, string actionCode, ZGuid targetPk, string targetTableCode, string jsonParameter = null, ZDateTime? scheduledLaterThanDateTimeUtc = null, int? batchSize = null);
	}

	public static class IActionScheduleProvider_Extensions
	{
		public static IReadOnlyCollection<IActionSchedule> GetRunnableSchedules(this IActionScheduleProvider provider, BusinessObjectFactory factory, int? batchSize = null)
		{
			var query = provider.GetRunnableSchedulesQuery();
			if (batchSize.HasValue)
			{
				query.MaximumRows = batchSize.Value;
			}
			return factory.Load<IActionSchedule>(query);
		}
	}
}

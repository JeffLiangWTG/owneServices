using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.TimeEngineScheduler.Integration;

namespace Enterprise.TimeEngineScheduler.Business.Test
{
	public class TimeEngineSchedulerTestHelper : ITimeEngineSchedulerTestHelper
	{
		public IActionSchedule ScheduleAction(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<TimeActionSchedule>();
		}

		public IActionSchedule ScheduleAction(BusinessObjectFactory factory, string actionCode, Guid targetPK, string targetTableCode,
			DateTime executionDateTimeUtc, string jsonParameter, string executionStatus = Constants.TimeActionScheduleStatus.Scheduled,
			string executionResult = "", byte retryAttempts = 0)
		{
			var schedule = factory.New<TimeActionSchedule>();
			schedule.TAS_ActionCode = actionCode;
			schedule.TAS_TargetPK = targetPK;
			schedule.TAS_TargetTableCode = targetTableCode;
			schedule.TAS_ExecutionDateTimeUtc = executionDateTimeUtc;
			schedule.TAS_JsonParameter = jsonParameter;
			schedule.TAS_ExecutionStatus = executionStatus;
			schedule.TAS_ExecutionResult = executionResult;
			schedule.TAS_RetryAttempts = retryAttempts;
			schedule.TAS_GB_Branch = GlbBranch.CurrentBranch.PK;
			schedule.TAS_GE_Department = GlbDepartment.CurrentDepartment.PK;
			return schedule;
		}
	}
}

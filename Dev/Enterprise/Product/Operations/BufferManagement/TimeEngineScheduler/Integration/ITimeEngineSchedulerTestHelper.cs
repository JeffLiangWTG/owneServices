#if DEBUG

using System;
using CargoWise.EntityFramework;

namespace Enterprise.TimeEngineScheduler.Integration
{
	public interface ITimeEngineSchedulerTestHelper
	{
		IActionSchedule ScheduleAction(BusinessObjectFactory factory);

		IActionSchedule ScheduleAction(BusinessObjectFactory factory, string actionCode, Guid targetPK, string targetTableCode,
			DateTime executionDateTimeUtc, string jsonParameter, string executionStatus = Constants.TimeActionScheduleStatus.Scheduled,
			string executionResult = "", byte retryAttempts = 0);
	}
}

#endif

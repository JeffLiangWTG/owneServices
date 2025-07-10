using System;
using System.Collections.Generic;

namespace Enterprise.Integration.Billing
{
	public interface IDateTimeRange
	{
		DateTime StartDateTimeInclusive { get; }
		DateTime EndDateTimeExclusive { get; }
		DateTime? MonthlyRangeStartInclusive { get; }
		DateTime? MonthlyRangeEndExclusive { get; }
		DateTime? StlMilestoneTimestamp { get; }
		DateTime? DailyRangeStartInclusive { get; }
		DateTime? DailyRangeEndInclusive { get; }

		bool IsCurrentMonthlyCollection();
		bool IsValid { get; }
		IEnumerable<IStlScriptWithConfig> SelectApplicableScripts(IEnumerable<IStlScriptWithConfig> allScripts);
		bool ScriptShouldBeRun(IStlScriptWithConfig script);
		bool ScriptResultsShouldBeSubmitted(IStlScriptWithConfig script);
	}
}

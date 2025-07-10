using System;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class DstRuleParserForTesting : DstRuleParser
	{
		public DstRuleParserForTesting(int currentYear, string startOrEndRule, string weekdayOrMonthday, string transitionTimeBaseCode, DateTime dstDate, int weekOfMonth, string weekdayCode, string monthCode)
			: base(currentYear, startOrEndRule, weekdayOrMonthday, transitionTimeBaseCode, dstDate, weekOfMonth, weekdayCode, monthCode)
		{
		}

		public DstRuleParserForTesting()
			: this(2000, TimeZoneConstants.DstTransitionTypeStart, TimeZoneConstants.DstRuleDayOfMonth, TimeZoneConstants.DstTimeBaseLocal, new DateTime(2000, 1, 1), 0, "", "")
		{
		}

		public DayOfWeek GetDayOfWeekFromCode_Exposed(string weekdayCode)
		{
			return GetDayOfWeekFromCode(weekdayCode);
		}
	}
}

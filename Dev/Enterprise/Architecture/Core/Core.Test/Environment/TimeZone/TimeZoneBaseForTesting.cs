using System;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class TimeZoneBaseForTesting : TimeZoneBase
	{
		public TimeZoneBaseForTesting()
			: base("", 0m, 0m)
		{
		}

		protected override bool HasDaylightSaving
		{
			get { return true; }
		}

		protected override RefTimeZoneRuleInfoStartAndEndPair GetStartAndEndDstRuleInfo(int year)
		{
			RefTimeZoneRuleInfo startRule = new RefTimeZoneRuleInfo(
						year, TimeZoneConstants.DstTransitionTypeStart, TimeZoneConstants.DstRuleDayOfMonth,
						TimeZoneConstants.DstTimeBaseUtc, new DateTime(1900, 08, 30, 0, 0, 0), 0, "", "");
			RefTimeZoneRuleInfo endRule = new RefTimeZoneRuleInfo(
						year, TimeZoneConstants.DstTransitionTypeEnd, TimeZoneConstants.DstRuleDayOfMonth,
						TimeZoneConstants.DstTimeBaseUtc, new DateTime(1900, 05, 01, 0, 0, 0), 0, "", "");

			if (year < 1998 || year > 2000)
			{
				startRule = null;
				endRule = null;
			}
			else if (year == 1998)
			{
				endRule = null;
			}
			else if (year == 2000)
			{
				startRule = null;
			}

			RefTimeZoneRuleInfoStartAndEndPair result = new RefTimeZoneRuleInfoStartAndEndPair(startRule, endRule);
			return result;
		}
	}
}

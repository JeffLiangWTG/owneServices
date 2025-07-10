using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class RefTimeZoneRuleInfo
	{
		public RefTimeZoneRuleInfo(
			int year, string ruleCode, string weekdayOrMonthday, string transitionTimeBaseCode,
			DateTime dstDate, int nthWeekdayOfMonth, string weekdayCode, string monthCode)
		{
			this.year = year;
			this.ruleCode = ruleCode;
			this.weekdayOrMonthday = weekdayOrMonthday;
			this.transitionTimeBaseCode = transitionTimeBaseCode;
			this.dstDate = dstDate;
			this.nthWeekdayOfMonth = nthWeekdayOfMonth;
			this.weekdayCode = weekdayCode;
			this.monthCode = monthCode;
		}

		public DstRuleParser RuleParser
		{
			get
			{
				if (ruleParser == null)
				{
					ruleParser = new DstRuleParser(
						year, ruleCode, weekdayOrMonthday, transitionTimeBaseCode,
						dstDate, nthWeekdayOfMonth, weekdayCode, monthCode);
				}

				return ruleParser;
			}
		}

		DstRuleParser ruleParser;

		public int Year
		{
			get { return year; }
		}

		readonly int year;

		public string RuleCode
		{
			get { return ruleCode; }
		}

		readonly string ruleCode;

		public string WeekdayOrMonthday
		{
			get { return weekdayOrMonthday; }
		}

		readonly string weekdayOrMonthday;

		public string TransitionTimeBaseCode
		{
			get { return transitionTimeBaseCode; }
		}

		readonly string transitionTimeBaseCode;

		public DateTime DstDate
		{
			get { return dstDate; }
		}

		readonly DateTime dstDate;

		public int NthWeekdayOfMonth
		{
			get { return nthWeekdayOfMonth; }
		}

		readonly int nthWeekdayOfMonth;

		public string WeekdayCode
		{
			get { return weekdayCode; }
		}

		readonly string weekdayCode;

		public string MonthCode
		{
			get { return monthCode; }
		}

		readonly string monthCode;
	}
}

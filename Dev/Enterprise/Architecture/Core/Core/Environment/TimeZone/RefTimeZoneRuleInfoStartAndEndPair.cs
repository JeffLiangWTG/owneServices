namespace Enterprise.ZArchitecture.Environment
{
	public class RefTimeZoneRuleInfoStartAndEndPair
	{
		public RefTimeZoneRuleInfoStartAndEndPair(RefTimeZoneRuleInfo startRule, RefTimeZoneRuleInfo endRule)
		{
			CheckRuleIsExpectedType(startRule, TimeZoneConstants.DstTransitionTypeStart);
			CheckRuleIsExpectedType(endRule, TimeZoneConstants.DstTransitionTypeEnd);
			CheckRuleYearIsTheSame(startRule, endRule);

			this.startRule = startRule;
			this.endRule = endRule;
		}

		public RefTimeZoneRuleInfo StartRule
		{
			get { return startRule; }
		}

		readonly RefTimeZoneRuleInfo startRule;

		public RefTimeZoneRuleInfo EndRule
		{
			get { return endRule; }
		}

		readonly RefTimeZoneRuleInfo endRule;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception messages")]
		void CheckRuleIsExpectedType(RefTimeZoneRuleInfo rule, string expectedTransitionType)
		{
			if (rule != null && rule.RuleCode != expectedTransitionType)
			{
				string message = string.Format(
					"Invalid transition type [{0}] - Expected [{1}].",
					rule.RuleCode, expectedTransitionType);
				throw new TimeZoneException(message, null);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception messages")]
		void CheckRuleYearIsTheSame(RefTimeZoneRuleInfo start, RefTimeZoneRuleInfo end)
		{
			if (start != null && end != null && start.Year != end.Year)
			{
				string message = string.Format(
					"Start year [{0}] and End year [{1}] do not match.",
					start.Year.ToString(), end.Year.ToString());
				throw new TimeZoneException(message, null);
			}
		}
	}
}

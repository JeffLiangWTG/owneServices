using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class RefTimeZoneRuleInfoStartAndEndPairTest : TestCase
	{
		public void TestThrowsExceptionWhenTransitionTypesDoNotMatch()
		{
			RefTimeZoneRuleInfo ruleStart = new RefTimeZoneRuleInfo(
				2000, TimeZoneConstants.DstTransitionTypeStart, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseUtc, new DateTime(1900, 01, 01, 0, 0, 0), 0, "", "");

			RefTimeZoneRuleInfo ruleEnd = new RefTimeZoneRuleInfo(
				2000, TimeZoneConstants.DstTransitionTypeEnd, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseUtc, new DateTime(1900, 01, 01, 0, 0, 0), 0, "", "");

			// Should NOT throw exceptions
			TimeZoneException nullException = null;
			AssertInstantiatingRulePair(null, null, nullException);
			AssertInstantiatingRulePair(ruleStart, null, nullException);
			AssertInstantiatingRulePair(null, ruleEnd, nullException);
			AssertInstantiatingRulePair(ruleStart, ruleEnd, nullException);

			// Should throw exceptions
			TimeZoneException expectedException = new TimeZoneException("Invalid transition type [STA] - Expected [END].", null);
			AssertInstantiatingRulePair(null, ruleStart, expectedException);
			AssertInstantiatingRulePair(ruleStart, ruleStart, expectedException);
			expectedException = new TimeZoneException("Invalid transition type [END] - Expected [STA].", null);
			AssertInstantiatingRulePair(ruleEnd, null, expectedException);
			AssertInstantiatingRulePair(ruleEnd, ruleEnd, expectedException);
			AssertInstantiatingRulePair(ruleEnd, ruleStart, expectedException);
		}

		public void TestThrowsExceptionWhenYearsDoNotMatch()
		{
			RefTimeZoneRuleInfo ruleStart2000 = new RefTimeZoneRuleInfo(
				2000, TimeZoneConstants.DstTransitionTypeStart, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseUtc, new DateTime(1900, 01, 01, 0, 0, 0), 0, "", "");

			RefTimeZoneRuleInfo ruleEnd2000 = new RefTimeZoneRuleInfo(
				2000, TimeZoneConstants.DstTransitionTypeEnd, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseUtc, new DateTime(1900, 01, 01, 0, 0, 0), 0, "", "");

			RefTimeZoneRuleInfo ruleEnd2001 = new RefTimeZoneRuleInfo(
				2001, TimeZoneConstants.DstTransitionTypeEnd, TimeZoneConstants.DstRuleDayOfMonth,
				TimeZoneConstants.DstTimeBaseUtc, new DateTime(1900, 01, 01, 0, 0, 0), 0, "", "");

			// Should NOT throw exceptions
			TimeZoneException nullException = null;
			AssertInstantiatingRulePair(null, null, nullException);
			AssertInstantiatingRulePair(ruleStart2000, null, nullException);
			AssertInstantiatingRulePair(null, ruleEnd2000, nullException);
			AssertInstantiatingRulePair(ruleStart2000, ruleEnd2000, nullException);

			// Should throw exceptions
			TimeZoneException expectedException = new TimeZoneException("Start year [2000] and End year [2001] do not match.", null);
			AssertInstantiatingRulePair(ruleStart2000, ruleEnd2001, expectedException);
		}

		void AssertInstantiatingRulePair(RefTimeZoneRuleInfo startRule, RefTimeZoneRuleInfo endRule, TimeZoneException expectedException)
		{
			try
			{
				new RefTimeZoneRuleInfoStartAndEndPair(startRule, endRule);
				Assert("Expected Exception not null => An exception should have been thrown.", expectedException == null);
			}
			catch (TimeZoneException ex)
			{
				AssertEquals("Caught Exception: ", expectedException.Message, ex.Message);
			}
		}
	}
}

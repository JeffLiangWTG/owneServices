using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	class DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRateComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, comparer.GetHashCode(originalAssessmentSpecificRate));
		}

		public void TestEquals()
		{
			AssertEquals(true, comparer.Equals(originalAssessmentSpecificRate, CreateAssessmentSpecificRate("AAA", 1.12M)));
		}

		public void TestEquals_NullString()
		{
			var assessmentSpecificRate = CreateAssessmentSpecificRate(null, 1.12M);
			AssertNoExceptionThrown(() => comparer.Equals(assessmentSpecificRate, originalAssessmentSpecificRate));
		}

		public void TestEquals_Type()
		{
			var assessmentSpecificRate = CreateAssessmentSpecificRate("BBB", 1.12M);
			AssertEquals(false, comparer.Equals(originalAssessmentSpecificRate, assessmentSpecificRate));
		}

		public void TestEquals_Value()
		{
			var assessmentSpecificRate = CreateAssessmentSpecificRate("AAA", 1.34M);
			AssertEquals(false, comparer.Equals(originalAssessmentSpecificRate, assessmentSpecificRate));
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalAssessmentSpecificRate = CreateAssessmentSpecificRate("AAA", 1.12M);
			comparer = new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRateComparer();
		}
		DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate originalAssessmentSpecificRate;
		DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRateComparer comparer;

		DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate CreateAssessmentSpecificRate(string type, decimal value)
		{
			return new DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate
			{
				Type = type,
				Value = value
			};
		}
	}
}

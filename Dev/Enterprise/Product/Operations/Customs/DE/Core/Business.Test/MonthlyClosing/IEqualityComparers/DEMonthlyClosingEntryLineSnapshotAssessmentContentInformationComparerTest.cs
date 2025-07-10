using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	class DEMonthlyClosingEntryLineSnapshotAssessmentContentInformationComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, comparer.GetHashCode(originalAssessmentContentInformation));
		}

		public void TestEquals()
		{
			AssertEquals(true, comparer.Equals(originalAssessmentContentInformation, CreateAssessmentContentInformation("AAA", 1.12M)));
		}

		public void TestEquals_NullString()
		{
			var assessmentContentInformation = CreateAssessmentContentInformation(null, 1.12M);
			AssertNoExceptionThrown(() => comparer.Equals(assessmentContentInformation, originalAssessmentContentInformation));
		}

		public void TestEquals_Type()
		{
			var assessmentContentInformation = CreateAssessmentContentInformation("BBB", 1.12M);
			AssertEquals(false, comparer.Equals(originalAssessmentContentInformation, assessmentContentInformation));
		}

		public void TestEquals_DegreePercentage()
		{
			var assessmentContentInformation = CreateAssessmentContentInformation("AAA", 1.34M);
			AssertEquals(false, comparer.Equals(originalAssessmentContentInformation, assessmentContentInformation));
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalAssessmentContentInformation = CreateAssessmentContentInformation("AAA", 1.12M);
			comparer = new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformationComparer();
		}
		DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation originalAssessmentContentInformation;
		DEMonthlyClosingEntryLineSnapshotAssessmentContentInformationComparer comparer;

		DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation CreateAssessmentContentInformation(string type, decimal degreePercentage)
		{
			return new DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation
			{
				Type = type,
				DegreePercentage = degreePercentage
			};
		}
	}
}

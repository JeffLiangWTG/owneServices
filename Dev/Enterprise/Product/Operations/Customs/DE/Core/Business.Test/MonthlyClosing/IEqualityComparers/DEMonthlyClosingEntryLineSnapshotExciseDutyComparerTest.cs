using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	class DEMonthlyClosingEntryLineSnapshotExciseDutyComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, comparer.GetHashCode(originalExciseDuty));
		}

		public void TestEquals()
		{
			AssertEquals(true, comparer.Equals(originalExciseDuty, CreateExciseDuty(1.12M, "AAA", 2.12M, new Amount() { Qualifier = "QQQ" })));
		}

		public void TestEquals_NullString()
		{
			var exciseDuty = CreateExciseDuty(1.12M, null, 2.12M, new Amount() { Qualifier = "QQQ" });
			AssertNoExceptionThrown(() => comparer.Equals(exciseDuty, originalExciseDuty));
		}

		public void TestEquals_Value()
		{
			var exciseDuty = CreateExciseDuty(1.34M, "AAA", 2.12M, new Amount() { Qualifier = "QQQ" });
			AssertEquals(false, comparer.Equals(originalExciseDuty, exciseDuty));
		}

		public void TestEquals_Code()
		{
			var exciseDuty = CreateExciseDuty(1.12M, "BBB", 2.12M, new Amount() { Qualifier = "QQQ" });
			AssertEquals(false, comparer.Equals(originalExciseDuty, exciseDuty));
		}

		public void TestEquals_DegreePercentage()
		{
			var exciseDuty = CreateExciseDuty(1.12M, "AAA", 2.34M, new Amount() { Qualifier = "QQQ" });
			AssertEquals(false, comparer.Equals(originalExciseDuty, exciseDuty));
		}

		public void TestEquals_Amount()
		{
			var exciseDuty = CreateExciseDuty(1.12M, "AAA", 2.12M, new Amount() { Qualifier = "WWW" });
			AssertEquals(false, comparer.Equals(originalExciseDuty, exciseDuty));
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalExciseDuty = CreateExciseDuty(1.12M, "AAA", 2.12M, new Amount() { Qualifier = "QQQ" });
			comparer = new DEMonthlyClosingEntryLineSnapshotExciseDutyComparer();
		}
		DEMonthlyClosingEntryLineSnapshotExciseDuty originalExciseDuty;
		DEMonthlyClosingEntryLineSnapshotExciseDutyComparer comparer;

		DEMonthlyClosingEntryLineSnapshotExciseDuty CreateExciseDuty(decimal value, string code, decimal degreePercentage, Amount amount)
		{
			return new DEMonthlyClosingEntryLineSnapshotExciseDuty
			{
				Value = value,
				Code = code,
				DegreePercentage = degreePercentage,
				Amount = amount
			};
		}
	}
}

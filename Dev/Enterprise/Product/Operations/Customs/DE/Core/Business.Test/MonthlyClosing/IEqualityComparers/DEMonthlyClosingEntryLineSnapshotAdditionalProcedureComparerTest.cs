using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	class DEMonthlyClosingEntryLineSnapshotAdditionalProcedureComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, comparer.GetHashCode(originalAdditionalProcedure));
		}

		public void TestEquals()
		{
			AssertEquals(true, comparer.Equals(originalAdditionalProcedure, CreateAdditionalProcedure("AAA")));
		}

		public void TestEquals_NullString()
		{
			var additionalProcedure = CreateAdditionalProcedure(null);
			AssertNoExceptionThrown(() => comparer.Equals(additionalProcedure, originalAdditionalProcedure));
		}

		public void TestEquals_Code()
		{
			var additionalProcedure = CreateAdditionalProcedure("BBB");
			AssertEquals(false, comparer.Equals(originalAdditionalProcedure, additionalProcedure));
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalAdditionalProcedure = CreateAdditionalProcedure("AAA");
			comparer = new DEMonthlyClosingEntryLineSnapshotAdditionalProcedureComparer();
		}
		DEMonthlyClosingEntryLineSnapshotAdditionalProcedure originalAdditionalProcedure;
		DEMonthlyClosingEntryLineSnapshotAdditionalProcedureComparer comparer;

		DEMonthlyClosingEntryLineSnapshotAdditionalProcedure CreateAdditionalProcedure(string code)
		{
			return new DEMonthlyClosingEntryLineSnapshotAdditionalProcedure
			{
				Code = code
			};
		}
	}
}

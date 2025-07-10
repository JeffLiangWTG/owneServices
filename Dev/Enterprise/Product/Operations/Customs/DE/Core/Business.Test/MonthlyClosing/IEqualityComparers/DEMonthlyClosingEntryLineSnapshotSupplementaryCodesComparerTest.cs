using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	class DEMonthlyClosingEntryLineSnapshotSupplementaryCodesComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, comparer.GetHashCode(originalSupplementaryCodes));
		}

		public void TestEquals()
		{
			AssertEquals(true, comparer.Equals(originalSupplementaryCodes, CreateSupplementaryCodes("AAA")));
		}

		public void TestEquals_NullString()
		{
			var supplementaryCodes = CreateSupplementaryCodes(null);
			AssertNoExceptionThrown(() => comparer.Equals(supplementaryCodes, originalSupplementaryCodes));
		}

		public void TestEquals_Code()
		{
			var supplementaryCodes = CreateSupplementaryCodes("BBB");
			AssertEquals(false, comparer.Equals(originalSupplementaryCodes, supplementaryCodes));
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalSupplementaryCodes = CreateSupplementaryCodes("AAA");
			comparer = new DEMonthlyClosingEntryLineSnapshotSupplementaryCodesComparer();
		}
		DEMonthlyClosingEntryLineSnapshotSupplementaryCodes originalSupplementaryCodes;
		DEMonthlyClosingEntryLineSnapshotSupplementaryCodesComparer comparer;

		DEMonthlyClosingEntryLineSnapshotSupplementaryCodes CreateSupplementaryCodes(string code)
		{
			return new DEMonthlyClosingEntryLineSnapshotSupplementaryCodes
			{
				Code = code
			};
		}
	}
}

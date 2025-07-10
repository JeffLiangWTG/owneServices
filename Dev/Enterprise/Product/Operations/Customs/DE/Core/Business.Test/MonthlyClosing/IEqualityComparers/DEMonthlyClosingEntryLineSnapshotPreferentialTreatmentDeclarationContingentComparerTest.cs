using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	class DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingentComparerTest : TestCase
	{
		public void TestGetHashCode()
		{
			AssertEquals(0, comparer.GetHashCode(originalContingent));
		}

		public void TestEquals()
		{
			AssertEquals(true, comparer.Equals(originalContingent, CreatePreferentialTreatmentDeclarationContingent("AAA")));
		}

		public void TestEquals_NullString()
		{
			var contingent = CreatePreferentialTreatmentDeclarationContingent(null);
			AssertNoExceptionThrown(() => comparer.Equals(contingent, originalContingent));
		}

		public void TestEquals_ContingentNumber()
		{
			var contingent = CreatePreferentialTreatmentDeclarationContingent("BBB");
			AssertEquals(false, comparer.Equals(originalContingent, contingent));
		}

		protected override void SetUp()
		{
			base.SetUp();
			originalContingent = CreatePreferentialTreatmentDeclarationContingent("AAA");
			comparer = new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingentComparer();
		}
		DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent originalContingent;
		DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingentComparer comparer;

		DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent CreatePreferentialTreatmentDeclarationContingent(string contingentNumber)
		{
			return new DEMonthlyClosingEntryLineSnapshotPreferentialTreatmentDeclarationContingent
			{
				ContingentNumber = contingentNumber
			};
		}
	}
}

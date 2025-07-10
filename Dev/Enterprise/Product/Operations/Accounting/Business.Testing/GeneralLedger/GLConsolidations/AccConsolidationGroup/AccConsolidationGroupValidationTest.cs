using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.Testing
{
	internal class AccConsolidationGroupValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCodeIsMandatoryAndUnique()
		{
			var group1 = Factory.New<AccConsolidationGroup>();
			group1.RunPreSaveValidation();
			AssertHasErrors(group1.YR_CodeInfo);

			group1.YR_Code = "ABC";
			AssertNoErrors(group1.YR_CodeInfo);

			var group2 = Factory.New<AccConsolidationGroup>();
			group2.YR_Code = "ABC";
			AssertHasErrors(group2.YR_CodeInfo);
		}

		public void TestDescriptionIsMandatory()
		{
			var group1 = Factory.New<AccConsolidationGroup>();
			group1.RunPreSaveValidation();
			AssertHasErrors(group1.YR_DescriptionInfo);

			group1.YR_Description = "Some Description";
			AssertNoErrors(group1.YR_DescriptionInfo);
		}

		public void TestParentGroupDetectsCauseCyclicDependencies()
		{
			var group1 = Factory.New<AccConsolidationGroup>();
			group1.YR_Code = "Group1";

			var group2 = Factory.New<AccConsolidationGroup>();
			group2.YR_Code = "Group2";

			AssertNoErrors(group1.YR_YR_ConsolidationGroupInfo);
			AssertNoErrors(group2.YR_YR_ConsolidationGroupInfo);

			group1.YR_YR_ConsolidationGroup = group2.PK;
			AssertNoErrors(group1.YR_YR_ConsolidationGroupInfo);
			AssertNoErrors(group2.YR_YR_ConsolidationGroupInfo);

			group2.YR_YR_ConsolidationGroup = group1.PK;
			AssertHasErrors(group2.YR_YR_ConsolidationGroupInfo);

			var group3 = Factory.New<AccConsolidationGroup>();
			group3.YR_Code = "Group3";
			group2.YR_YR_ConsolidationGroup = group3.PK;
			AssertNoErrors(group2.YR_YR_ConsolidationGroupInfo);

			group3.YR_YR_ConsolidationGroup = group1.PK;
			AssertHasErrors(group3.YR_YR_ConsolidationGroupInfo);

			var group4 = Factory.New<AccConsolidationGroup>();
			group4.YR_Code = "Group4";
			group3.YR_YR_ConsolidationGroup = group4.PK;
			AssertNoErrors(group3.YR_YR_ConsolidationGroupInfo);
		}
	}
}
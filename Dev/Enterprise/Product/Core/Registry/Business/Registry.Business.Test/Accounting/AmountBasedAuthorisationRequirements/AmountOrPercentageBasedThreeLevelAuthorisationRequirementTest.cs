using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AmountOrPercentageBasedThreeLevelAuthorisationRequirement))]
class AmountOrPercentageBasedThreeLevelAuthorisationRequirementTest : AmountBasedThreeLevelAuthorisationRequirementTest
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new AmountOrPercentageBasedThreeLevelAuthorisationRequirement();

			result.Range = result.RangeList[0].Code;
			result.Amount = 500;
			result.AuthorisationRequirement = result.AuthorisationRequirementList[0].Code;

			return result;
		}

		protected override AmountBasedAuthorisationRequirementCollection GetAuthorisationRequirementCollection()
		{
			return new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
		}

		public void TestValidateOnlyAllowsPercentageOrAmountInASetting()
		{
			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			var setting1 = collection.AddNew();
			setting1.RunPreSaveValidation();
			AssertNoError(setting1.AmountInfo, "Please either use percentages in all settings or amounts in all settings. You cannot enter both.");
			AssertNoError(setting1.PercentageInfo, "Please either use percentages in all settings or amounts in all settings. You cannot enter both.");
			setting1.Amount = 100;
			setting1.Percentage = 100;

			setting1.RunPreSaveValidation();
			AssertHasError(setting1.AmountInfo, "Please either use percentages in all settings or amounts in all settings. You cannot enter both.");
			AssertHasError(setting1.PercentageInfo, "Please either use percentages in all settings or amounts in all settings. You cannot enter both.");
		}

		public void TestValidateOnlyAllowsPercentageOrAmountAcrossSettings()
		{
			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			var setting1 = collection.AddNew();
			var setting2 = collection.AddNew();
			setting1.RunPreSaveValidation();
			setting2.RunPreSaveValidation();
			AssertNoError(setting1.AmountInfo, "Please either use percentages in all settings or amounts in all settings. You cannot enter both.");
			AssertNoError(setting1.PercentageInfo, "Please either use percentages in all settings or amounts in all settings. You cannot enter both.");
			AssertNoError(setting2.AmountInfo, "Please either use percentages in all settings or amounts in all settings. You cannot enter both.");
			AssertNoError(setting2.PercentageInfo, "Please either use percentages in all settings or amounts in all settings. You cannot enter both.");
			setting1.Amount = 100;
			setting2.Percentage = 100;
			setting1.RunPreSaveValidation();
			setting2.RunPreSaveValidation();
			AssertHasError("Error because amount is set, and mix of % and amount used", setting1.AmountInfo, "Please either use percentages in all settings or amounts in all settings. You cannot enter both.");
			AssertNoError("No error because percentage is not set", setting1.PercentageInfo, "Please either use percentages in all settings or amounts in all settings. You cannot enter both.");
			AssertNoError("No error because amount is not set", setting2.AmountInfo, "Please either use percentages in all settings or amounts in all settings. You cannot enter both.");
			AssertHasError("Error because % is set, and mix of % and amount used", setting2.PercentageInfo, "Please either use percentages in all settings or amounts in all settings. You cannot enter both.");
			setting2.Amount = 200;
			setting2.Percentage = 0;
			setting1.RunPreSaveValidation();
			setting2.RunPreSaveValidation();
			AssertNoError(setting1.AmountInfo, "Please either use percentages in all settings or amounts in all settings. You cannot enter both.");
			AssertNoError(setting1.PercentageInfo, "Please either use percentages in all settings or amounts in all settings. You cannot enter both.");
			AssertNoError(setting2.AmountInfo, "Please either use percentages in all settings or amounts in all settings. You cannot enter both.");
			AssertNoError(setting2.PercentageInfo, "Please either use percentages in all settings or amounts in all settings. You cannot enter both.");
		}

		public void TestPercentageGreaterThanZero()
		{
			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			var setting1 = collection.AddNew();
			setting1.RunPreSaveValidation();
			AssertNoError(setting1.PercentageInfo, "Please enter a 'Percentage' greater than or equal to 0.");
			setting1.Percentage = -0.01;
			setting1.RunPreSaveValidation();
			AssertHasError(setting1.PercentageInfo, "Please enter a 'Percentage' greater than or equal to 0.");
			setting1.Percentage = 0;
			setting1.RunPreSaveValidation();
			AssertNoError(setting1.PercentageInfo, "Please enter a 'Percentage' greater than or equal to 0.");
			setting1.Percentage = 0.01;
			setting1.RunPreSaveValidation();
			AssertNoErrors(setting1.PercentageInfo);
		}

		public void TestAuthorizationRequirement()
		{
			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			var setting1 = collection.AddNew();
			setting1.RunPreSaveValidation();
			AssertNoError(setting1.AuthorisationRequirementInfo, "An 'Up to' range with 'Amount = 0' must have an authorization requirement of 'None'.");

			setting1.Percentage = 0;
			setting1.Amount = 0;
			setting1.Range = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			setting1.AuthorisationRequirement = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			setting1.RunPreSaveValidation();
			AssertHasError(setting1.AuthorisationRequirementInfo, "An 'Up to' range with 'Amount = 0' must have an authorization requirement of 'None'.");

			setting1.Range = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			setting1.RunPreSaveValidation();
			AssertNoError(setting1.AuthorisationRequirementInfo, "An 'Up to' range with 'Amount = 0' must have an authorization requirement of 'None'.");

			setting1.Range = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			setting1.RunPreSaveValidation();
			AssertHasError(setting1.AuthorisationRequirementInfo, "An 'Up to' range with 'Amount = 0' must have an authorization requirement of 'None'.");

			setting1.AuthorisationRequirement = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			setting1.RunPreSaveValidation();
			AssertNoError(setting1.AuthorisationRequirementInfo, "An 'Up to' range with 'Amount = 0' must have an authorization requirement of 'None'.");
		}

		public void TestUpToLinesWithTheSameAmountValidation()
		{
			// Amounts
			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			var setting1 = collection.AddNew();
			var setting2 = collection.AddNew();
			setting1.Range = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.RangeCodes.UpTo;
			setting1.Percentage = 0;
			setting1.Amount = 20;
			setting2.Range = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.RangeCodes.UpTo;
			setting2.Percentage = 0;
			setting2.Amount = 20;
			setting2.RunPreSaveValidation();
			AssertHasError(setting2.AmountInfo, "No two 'Up to' Lines can have the same amount.");
			AssertNoErrors(setting2.PercentageInfo);
			setting2.Amount = 10;
			setting2.RunPreSaveValidation();
			AssertNoErrors(setting2.AmountInfo);
			AssertNoErrors(setting2.PercentageInfo);

			// Percentages
			collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			setting1 = collection.AddNew();
			setting2 = collection.AddNew();
			setting1.Range = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.RangeCodes.UpTo;
			setting1.Percentage = 20;
			setting1.Amount = 0;
			setting2.Range = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.RangeCodes.UpTo;
			setting2.Percentage = 20;
			setting2.Amount = 0;
			setting2.RunPreSaveValidation();
			AssertHasError(setting2.PercentageInfo, "No two 'Up to' Lines can have the same amount.");
			AssertNoErrors(setting2.AmountInfo);
			setting2.Percentage = 10;
			setting2.RunPreSaveValidation();
			AssertNoErrors(setting2.PercentageInfo);
			AssertNoErrors(setting2.AmountInfo);
		}

		public void TestAboveLineMustBeAsLastUpToLineValidation()
		{
			// Amounts
			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			var setting1 = collection.AddNew();
			var setting2 = collection.AddNew();
			setting1.Range = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.RangeCodes.UpTo;
			setting1.Percentage = 0;
			setting1.Amount = 20;
			setting2.Range = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.RangeCodes.Above;
			setting2.Percentage = 0;
			setting2.Amount = 30;
			setting2.RunPreSaveValidation();
			AssertHasError(setting2.AmountInfo, "The Above Line's amount must be 20.");
			AssertNoErrors(setting2.PercentageInfo);
			setting2.Amount = 20;
			setting2.RunPreSaveValidation();
			AssertNoErrors(setting2.AmountInfo);
			AssertNoErrors(setting2.PercentageInfo);

			// Percentages
			collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			setting1 = collection.AddNew();
			setting2 = collection.AddNew();
			setting1.Range = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.RangeCodes.UpTo;
			setting1.Percentage = 20;
			setting1.Amount = 0;
			setting2.Range = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.RangeCodes.Above;
			setting2.Percentage = 30;
			setting2.Amount = 0;
			setting2.RunPreSaveValidation();
			AssertHasError(setting2.PercentageInfo, "The Above Line's amount must be 20.");
			AssertNoErrors(setting2.AmountInfo);
			setting2.Percentage = 20;
			setting2.RunPreSaveValidation();
			AssertNoErrors(setting2.PercentageInfo);
			AssertNoErrors(setting2.AmountInfo);
		}

		public void TestHigherAmountsHigherAuthorisationLevelAndMandatoryAboveLevelValidation()
		{
			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			var setting1 = collection.AddNew();
			var setting2 = collection.AddNew();
			var setting3 = collection.AddNew();
			setting1.Range = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.RangeCodes.UpTo;
			setting1.Percentage = 100;
			setting1.AuthorisationRequirement = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			setting2.Range = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.RangeCodes.UpTo;
			setting2.Percentage = 50;
			setting2.AuthorisationRequirement = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			setting2.RunPreSaveValidation();
			setting3.Range = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.RangeCodes.Above;
			setting3.Percentage = 200;
			setting3.AuthorisationRequirement = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;
			AssertHasError(setting2.PercentageInfo, "Higher amounts must have Authorization level higher than lower amounts.");
			setting2.Percentage = 150;
			setting2.RunPreSaveValidation();
			AssertNoErrors(setting2.PercentageInfo);
		}

		public override void TestValidateAmount()
		{
			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			var setting1 = collection.AddNew();
			setting1.Amount = 1;
			setting1.RunPreSaveValidation();
			AssertNoErrors(setting1.AmountInfo);
			setting1.Amount = -1;
			setting1.RunPreSaveValidation();
			AssertHasError(setting1.AmountInfo, "Please enter an 'Amount' greater than or equal to 0.");

			collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			setting1 = collection.AddNew();
			var setting2 = collection.AddNew();
			var setting3 = collection.AddNew();
			var setting4 = collection.AddNew();

			AssertNoErrors("Precondition: setting2.RangeInfo should not have errors.", setting1.RangeInfo);
			AssertNoErrors("Precondition: setting2.RangeInfo should not have errors.", setting2.RangeInfo);
			AssertNoErrors("Precondition: setting3.RangeInfo should not have errors.", setting3.RangeInfo);
			AssertNoErrors("Precondition: setting4.RangeInfo should not have errors.", setting4.RangeInfo);

			setting1.Range = "Up to";
			setting1.AuthorisationRequirement = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;
			setting1.Amount = 700;
			setting2.Range = "Up to";
			setting2.AuthorisationRequirement = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			setting2.Amount = 200;
			setting3.Range = "Up to";
			setting3.AuthorisationRequirement = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			setting3.Amount = 300;
			setting4.Range = "Above";
			setting4.AuthorisationRequirement = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			setting4.Amount = 700;
			AssertHasErrors("At least one item must be 'Above'.", setting1.AmountInfo);
			AssertHasErrors("At least one item must be 'Above'.", setting2.AmountInfo);
			AssertHasErrors("At least one item must be 'Above'.", setting3.AmountInfo);
			AssertHasErrors("Higher Amounts must have Authorisation level higher than lower amounts.", setting4.AmountInfo);

			setting3.AuthorisationRequirement = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			setting4.AuthorisationRequirement = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;
			setting1.Amount = setting1.Amount;
			setting2.Amount = setting2.Amount;
			setting3.Amount = setting3.Amount;
			setting4.Amount = setting4.Amount;
			AssertNoErrors(setting1.AmountInfo);
			AssertNoErrors(setting2.AmountInfo);
			AssertNoErrors(setting3.AmountInfo);
			AssertNoErrors(setting4.AmountInfo);
		}

		public void TestValidatePercentage()
		{
			var collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			var setting1 = collection.AddNew();
			setting1.Percentage = 1;
			setting1.RunPreSaveValidation();
			AssertNoErrors(setting1.PercentageInfo);
			setting1.Percentage = -1;
			setting1.RunPreSaveValidation();
			AssertHasError(setting1.PercentageInfo, "Please enter a 'Percentage' greater than or equal to 0.");

			collection = new AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection();
			setting1 = collection.AddNew();
			var setting2 = collection.AddNew();
			var setting3 = collection.AddNew();
			var setting4 = collection.AddNew();

			AssertNoErrors("Precondition: setting2.RangeInfo should not have errors.", setting1.RangeInfo);
			AssertNoErrors("Precondition: setting2.RangeInfo should not have errors.", setting2.RangeInfo);
			AssertNoErrors("Precondition: setting3.RangeInfo should not have errors.", setting3.RangeInfo);
			AssertNoErrors("Precondition: setting4.RangeInfo should not have errors.", setting4.RangeInfo);

			setting1.Range = "Up to";
			setting1.AuthorisationRequirement = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;
			setting1.Percentage = 700;
			setting2.Range = "Up to";
			setting2.AuthorisationRequirement = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			setting2.Percentage = 200;
			setting3.Range = "Up to";
			setting3.AuthorisationRequirement = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			setting3.Percentage = 300;
			setting4.Range = "Above";
			setting4.AuthorisationRequirement = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			setting4.Percentage = 700;
			AssertHasErrors("At least one item must be 'Above'.", setting1.PercentageInfo);
			AssertHasErrors("At least one item must be 'Above'.", setting2.PercentageInfo);
			AssertHasErrors("At least one item must be 'Above'.", setting3.PercentageInfo);
			AssertHasErrors("Higher Amounts must have Authorisation level higher than lower amounts.", setting4.PercentageInfo);

			setting3.AuthorisationRequirement = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			setting4.AuthorisationRequirement = AmountOrPercentageBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;
			setting1.Percentage = setting1.Percentage;
			setting2.Percentage = setting2.Percentage;
			setting3.Percentage = setting3.Percentage;
			setting4.Percentage = setting4.Percentage;
			AssertNoErrors(setting1.PercentageInfo);
			AssertNoErrors(setting2.PercentageInfo);
			AssertNoErrors(setting3.PercentageInfo);
			AssertNoErrors(setting4.PercentageInfo);
		}

		#endregion
	}
}

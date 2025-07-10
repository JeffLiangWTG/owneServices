using System;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using AuthorisationRequirement = Enterprise.Registry.Business.AmountBasedTwoLevelAuthorisationRequirement;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AmountBasedTwoLevelAuthorisationRequirement))]
	public abstract class AmountBasedTwoLevelAuthorisationRequirementTest : AmountBasedMultiLevelAuthorisationRequirementTest
	{
		public override void TestAuthorisationRequirementList()
		{
			AssertEquals("AuthorisationRequirementList.Count", 3, TestBizObj.AuthorisationRequirementList.Count);
			AssertEquals("AuthorisationRequirementList should contain 'None'", true, TestBizObj.AuthorisationRequirementList.ContainsCode("None"));
			AssertEquals("AuthorisationRequirementList should contain '1st Level Only'", true, TestBizObj.AuthorisationRequirementList.ContainsCode("1st Level Only"));
			AssertEquals("AuthorisationRequirementList should contain '2nd Level Only'", true, TestBizObj.AuthorisationRequirementList.ContainsCode("2nd Level Only"));
			AssertEquals("AuthorisationRequirementList should contain '1st and 2nd Level'", false, TestBizObj.AuthorisationRequirementList.ContainsCode("1st and 2nd Level"));
			AssertEquals("AuthorisationRequirementList should not contain '1st, 2nd and 3rd Level'", false, TestBizObj.AuthorisationRequirementList.ContainsCode("1st, 2nd and 3rd Level"));
		}

		public override void TestValidateAmount()
		{
			base.TestValidateAmount();

			AmountBasedAuthorisationRequirementCollection collection = GetAuthorisationRequirementCollection();
			AuthorisationRequirement setting1 = (AuthorisationRequirement)collection.AddNew();
			AuthorisationRequirement setting2 = (AuthorisationRequirement)collection.AddNew();
			AuthorisationRequirement setting3 = (AuthorisationRequirement)collection.AddNew();

			AssertNoErrors("Precondition: setting2.RangeInfo should not have errors.", setting1.RangeInfo);
			AssertNoErrors("Precondition: setting2.RangeInfo should not have errors.", setting2.RangeInfo);
			AssertNoErrors("Precondition: setting3.RangeInfo should not have errors.", setting3.RangeInfo);

			setting1.Range = "Up to";
			setting1.AuthorisationRequirement = AuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			setting1.Amount = 700;
			setting2.Range = "Up to";
			setting2.AuthorisationRequirement = AuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			setting2.Amount = 200;
			setting3.Range = "Up to";
			setting3.AuthorisationRequirement = AuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			setting3.Amount = 300;
			AssertHasErrors("At least one item must be 'Above'.", setting1.AmountInfo);
			AssertHasErrors("At least one item must be 'Above'.", setting2.AmountInfo);
			AssertHasErrors("Higher amounts must have Authorisation level higher than lower amounts.", setting3.AmountInfo);

			setting1.Amount = setting1.Amount;
			AssertHasErrors("Higher Amounts must have Authorisation level higher than lower amounts.", setting1.AmountInfo);
			AssertHasErrors("At least one item must be 'Above'.", setting2.AmountInfo);
			AssertHasErrors("Higher amounts must have Authorisation level higher than lower amounts.", setting3.AmountInfo);

			setting1.AuthorisationRequirement = AuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			setting3.AuthorisationRequirement = AuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			setting1.Amount = setting1.Amount;
			setting3.Amount = setting3.Amount;
			AssertHasErrors("At least one item must be 'Above'.", setting1.AmountInfo);
			AssertHasErrors("At least one item must be 'Above'.", setting2.AmountInfo);
			AssertHasErrors("At least one item must be 'Above'.", setting3.AmountInfo);

			setting3.AuthorisationRequirement = AuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			setting3.Range = "Above";
			setting3.Amount = 700;
			setting1.Amount = setting1.Amount;
			setting2.Amount = setting2.Amount;
			AssertNoErrors(setting1.AmountInfo);
			AssertNoErrors(setting2.AmountInfo);
			AssertHasErrors("Higher amounts must have Authorisation level higher than lower amounts.", setting3.AmountInfo);

			setting1.AuthorisationRequirement = AuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			setting3.AuthorisationRequirement = AuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			setting3.Amount = setting3.Amount;
			AssertNoErrors(setting1.AmountInfo);
			AssertNoErrors(setting2.AmountInfo);
			AssertNoErrors(setting3.AmountInfo);
		}

		public override void TestValidateAuthorisationRequirement()
		{
			AssertNoErrors("Precondition: AuthorisationRequirement should not have errors.", TestBizObj.AuthorisationRequirementInfo);

			AssertNotNullOrEmpty("Precondition: AuthorisationRequirementList[0].Code should not be empty.", TestBizObj.AuthorisationRequirementList[0].Code);

			TestBizObj.AuthorisationRequirement = "!@#";
			AssertHasError(TestBizObj.AuthorisationRequirementInfo, "Enter a valid selection.");

			TestBizObj.AuthorisationRequirement = TestBizObj.AuthorisationRequirementList[0].Code;
			AssertNoErrors(TestBizObj.AuthorisationRequirementInfo);

			TestBizObj.AuthorisationRequirement = "";
			AssertHasError(TestBizObj.AuthorisationRequirementInfo, "Please enter a value.");

			AmountBasedAuthorisationRequirementCollection collection = GetAuthorisationRequirementCollection();
			AuthorisationRequirement setting1 = (AuthorisationRequirement)collection.AddNew();
			AuthorisationRequirement setting2 = (AuthorisationRequirement)collection.AddNew();
			AuthorisationRequirement setting3 = (AuthorisationRequirement)collection.AddNew();

			AssertNoErrors("Precondition: setting1.RangeInfo should not have errors.", setting1.AuthorisationRequirementInfo);
			AssertNoErrors("Precondition: setting2.RangeInfo should not have errors.", setting2.AuthorisationRequirementInfo);
			AssertNoErrors("Precondition: setting3.RangeInfo should not have errors.", setting3.AuthorisationRequirementInfo);

			setting1.AuthorisationRequirement = AuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			setting2.AuthorisationRequirement = AuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			setting3.AuthorisationRequirement = AuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;

			AssertNoErrors(setting1.AuthorisationRequirementInfo);
			AssertNoErrors(setting2.AuthorisationRequirementInfo);
			AssertHasError(setting3.AuthorisationRequirementInfo, "There must be only one '1st Level Only' line.");

			setting3.AuthorisationRequirement = AuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			AssertNoErrors(setting1.AuthorisationRequirementInfo);
			AssertNoErrors(setting2.AuthorisationRequirementInfo);
			AssertNoErrors(setting3.AuthorisationRequirementInfo);

			setting3.AuthorisationRequirement = AuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;

			AssertNoErrors(setting1.AuthorisationRequirementInfo);
			AssertNoErrors(setting2.AuthorisationRequirementInfo);
			AssertHasError(setting3.AuthorisationRequirementInfo, "There must be only one 'None' line.");

			setting3.AuthorisationRequirement = AuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			AssertNoErrors(setting1.AuthorisationRequirementInfo);
			AssertNoErrors(setting2.AuthorisationRequirementInfo);
			AssertNoErrors(setting3.AuthorisationRequirementInfo);

			setting2.AuthorisationRequirement = AuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;

			AssertNoErrors(setting1.AuthorisationRequirementInfo);
			AssertHasError(setting2.AuthorisationRequirementInfo, "There must be only one '2nd Level Only' line.");
			AssertNoErrors(setting3.AuthorisationRequirementInfo);

			setting2.AuthorisationRequirement = AuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			AssertNoErrors(setting1.AuthorisationRequirementInfo);
			AssertNoErrors(setting2.AuthorisationRequirementInfo);
			AssertNoErrors(setting3.AuthorisationRequirementInfo);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			AmountBasedTwoLevelAuthorisationRequirement result = (AmountBasedTwoLevelAuthorisationRequirement)GetNewBusinessObject();

			result.Range = result.RangeList[0].Code;
			result.Amount = 500;
			result.AuthorisationRequirement = result.AuthorisationRequirementList[0].Code;

			return result;
		}

		AmountBasedTwoLevelAuthorisationRequirement TestBizObj
		{
			get { return (AmountBasedTwoLevelAuthorisationRequirement)BizObj; }
		}

		FallbackLevel fCurrentFallbackLevel;
		protected FallbackLevel CurrentFallbackLevel
		{
			get { return fCurrentFallbackLevel ?? (fCurrentFallbackLevel = new FallbackLevel(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)); }
		}

		#endregion
	}
}

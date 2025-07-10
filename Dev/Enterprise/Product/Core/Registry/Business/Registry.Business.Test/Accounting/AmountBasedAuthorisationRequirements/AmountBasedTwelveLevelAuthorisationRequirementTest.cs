using NUnit.Framework;
using AuthorisationRequirement = Enterprise.Registry.Business.AmountBasedTwelveLevelAuthorisationRequirement;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AuthorisationRequirement))]
	public abstract class AmountBasedTwelveLevelAuthorisationRequirementTest : AmountBasedTwoLevelAuthorisationRequirementTest
	{
		public override void TestAuthorisationRequirementList()
		{
			AssertEquals("AuthorisationRequirementList.Count", 13, BizObj.AuthorisationRequirementList.Count);
			AssertEquals("AuthorisationRequirementList should contain 'None'", true, BizObj.AuthorisationRequirementList.ContainsCode(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired));
			AssertEquals("AuthorisationRequirementList should contain '1st Level'", true, BizObj.AuthorisationRequirementList.ContainsCode(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly));
			AssertEquals("AuthorisationRequirementList should contain '2nd Level'", true, BizObj.AuthorisationRequirementList.ContainsCode(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly));
			AssertEquals("AuthorisationRequirementList should contain '3rd Level'", true, BizObj.AuthorisationRequirementList.ContainsCode(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly));
			AssertEquals("AuthorisationRequirementList should contain '4th Level'", true, BizObj.AuthorisationRequirementList.ContainsCode(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FourthApprovalRequiredOnly));
			AssertEquals("AuthorisationRequirementList should contain '5th Level'", true, BizObj.AuthorisationRequirementList.ContainsCode(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FifthApprovalRequiredOnly));
			AssertEquals("AuthorisationRequirementList should contain '6th Level'", true, BizObj.AuthorisationRequirementList.ContainsCode(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SixthApprovalRequiredOnly));
			AssertEquals("AuthorisationRequirementList should contain '7th Level'", true, BizObj.AuthorisationRequirementList.ContainsCode(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SeventhApprovalRequiredOnly));
			AssertEquals("AuthorisationRequirementList should contain '8th Level'", true, BizObj.AuthorisationRequirementList.ContainsCode(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.EighthApprovalRequiredOnly));
			AssertEquals("AuthorisationRequirementList should contain '9th Level'", true, BizObj.AuthorisationRequirementList.ContainsCode(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NinthApprovalRequiredOnly));
			AssertEquals("AuthorisationRequirementList should contain '10th Level'", true, BizObj.AuthorisationRequirementList.ContainsCode(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.TenthApprovalRequiredOnly));
			AssertEquals("AuthorisationRequirementList should contain '11th Level'", true, BizObj.AuthorisationRequirementList.ContainsCode(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.EleventhApprovalRequiredOnly));
			AssertEquals("AuthorisationRequirementList should contain '12th Level'", true, BizObj.AuthorisationRequirementList.ContainsCode(AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.TwelfthApprovalRequiredOnly));
		}

		public override void TestValidateAmount()
		{
			base.TestValidateAmount();

			AmountBasedAuthorisationRequirementCollection collection = GetAuthorisationRequirementCollection();
			var setting1 = (AuthorisationRequirement)collection.AddNew();
			var setting2 = (AuthorisationRequirement)collection.AddNew();
			var setting3 = (AuthorisationRequirement)collection.AddNew();
			var setting4 = (AuthorisationRequirement)collection.AddNew();
			var setting5 = (AuthorisationRequirement)collection.AddNew();
			var setting6 = (AuthorisationRequirement)collection.AddNew();
			var setting7 = (AuthorisationRequirement)collection.AddNew();

			AssertNoErrors("Precondition: setting2.RangeInfo should not have errors.", setting1.RangeInfo);
			AssertNoErrors("Precondition: setting2.RangeInfo should not have errors.", setting2.RangeInfo);
			AssertNoErrors("Precondition: setting3.RangeInfo should not have errors.", setting3.RangeInfo);
			AssertNoErrors("Precondition: setting4.RangeInfo should not have errors.", setting4.RangeInfo);
			AssertNoErrors("Precondition: setting5.RangeInfo should not have errors.", setting5.RangeInfo);
			AssertNoErrors("Precondition: setting6.RangeInfo should not have errors.", setting6.RangeInfo);
			AssertNoErrors("Precondition: setting7.RangeInfo should not have errors.", setting7.RangeInfo);

			setting1.Range = "Up to";
			setting1.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;
			setting1.Amount = 700;
			setting2.Range = "Up to";
			setting2.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			setting2.Amount = 200;
			setting3.Range = "Up to";
			setting3.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			setting3.Amount = 300;
			setting4.Range = "Above";
			setting4.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			setting4.Amount = 700;
			AssertHasErrors("At least one item must be 'Above'.", setting1.AmountInfo);
			AssertHasErrors("At least one item must be 'Above'.", setting2.AmountInfo);
			AssertHasErrors("At least one item must be 'Above'.", setting3.AmountInfo);
			AssertHasErrors("Higher Amounts must have Authorisation level higher than lower amounts.", setting4.AmountInfo);

			setting3.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			setting4.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;
			setting1.Amount = setting1.Amount;
			setting2.Amount = setting2.Amount;
			setting3.Amount = setting3.Amount;
			setting4.Amount = setting4.Amount;
			AssertNoErrors(setting1.AmountInfo);
			AssertNoErrors(setting2.AmountInfo);
			AssertNoErrors(setting3.AmountInfo);
			AssertNoErrors(setting4.AmountInfo);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (AuthorisationRequirement)GetNewBusinessObject();

			result.Range = result.RangeList[0].Code;
			result.Amount = 500;
			result.AuthorisationRequirement = result.AuthorisationRequirementList[0].Code;

			return result;
		}

		protected new AuthorisationRequirement BizObj
		{
			get { return (AuthorisationRequirement)base.BizObj; }
		}

		#endregion
	}
}

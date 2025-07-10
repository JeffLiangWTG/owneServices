using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(PeriodReopenLevels))]
	public class PeriodReopenLevelsTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDaysIsReadOnlyWhenRangeIsUnlimited()
		{
			TestBizObj.Days = 20;
			Assert(!TestBizObj.DaysInfo.ReadOnly);
			TestBizObj.Range = PeriodReopenLevels.RangeCodes.Unlimited;
			Assert(TestBizObj.DaysInfo.ReadOnly);
			AssertEquals(0, TestBizObj.Days);
		}

		public void TestValidateRange()
		{
			AssertNoErrors("Precondition: Range should not have errors.", TestBizObj.RangeInfo);
			Assert("Precondition: RangeList[0].Code should not be empty.", !string.IsNullOrEmpty(TestBizObj.RangeList[0].Code));

			TestBizObj.Range = "!@#";
			AssertHasError(TestBizObj.RangeInfo, "Enter a valid selection.");

			TestBizObj.Range = TestBizObj.RangeList[0].Code;
			AssertNoErrors(TestBizObj.RangeInfo);

			TestBizObj.Range = "";
			AssertHasError(TestBizObj.RangeInfo, "Please enter a value.");

			PeriodReopenLevelsCollection collection = new PeriodReopenLevelsCollection();
			PeriodReopenLevels setting1 = collection.AddNew();
			PeriodReopenLevels setting2 = collection.AddNew();
			PeriodReopenLevels setting3 = collection.AddNew();

			AssertNoErrors("Precondition: setting1.RangeInfo should not have errors.", setting1.RangeInfo);
			AssertNoErrors("Precondition: setting2.RangeInfo should not have errors.", setting2.RangeInfo);
			AssertNoErrors("Precondition: setting3.RangeInfo should not have errors.", setting3.RangeInfo);

			setting1.Range = PeriodReopenLevels.RangeCodes.DaysAfterEndOfPeriod;
			setting2.Range = PeriodReopenLevels.RangeCodes.Unlimited;
			setting3.Range = PeriodReopenLevels.RangeCodes.Unlimited;

			AssertNoErrors(setting1.RangeInfo);
			AssertNoErrors(setting2.RangeInfo);
			AssertHasError(setting3.RangeInfo, "There must be only one 'Unlimited' line.");

			setting3.Range = PeriodReopenLevels.RangeCodes.DaysAfterEndOfPeriod;
			AssertNoErrors(setting2.RangeInfo);
			AssertNoErrors(setting3.RangeInfo);
		}

		public virtual void TestValidateDays()
		{
			AssertNoErrors("Precondition: Days should not have errors.", TestBizObj.DaysInfo);

			TestBizObj.Days = 100;
			AssertNoErrors(TestBizObj.DaysInfo);

			TestBizObj.Days = -1;
			AssertHasErrorContaining(TestBizObj.DaysInfo, string.Format("Please enter a '{0}' greater than 0.", TestBizObj.DaysInfo.HumanReadableName));

			TestBizObj.Days = 0;
			AssertHasErrors("Single not ranged line can't be equal 0.", TestBizObj.DaysInfo);

			TestBizObj.Range = PeriodReopenLevels.RangeCodes.DaysAfterEndOfPeriod;
			TestBizObj.Days = -1;
			AssertHasErrorContaining(TestBizObj.DaysInfo, string.Format("'{0}' greater than 0.", TestBizObj.DaysInfo.HumanReadableName));

			TestBizObj.Days = 0;
			AssertHasErrorContaining(TestBizObj.DaysInfo, string.Format("'{0}' greater than 0.", TestBizObj.DaysInfo.HumanReadableName));

			PeriodReopenLevelsCollection collection = new PeriodReopenLevelsCollection();
			PeriodReopenLevels setting1 = collection.AddNew();

			AssertNoErrors("Precondition: setting1.RangeInfo should not have errors.", setting1.DaysInfo);

			setting1.Days = 0;
			AssertHasErrors("Zero is not valid for not ranged line in one line collection.", setting1.DaysInfo);

			setting1.Range = PeriodReopenLevels.RangeCodes.DaysAfterEndOfPeriod;
			setting1.Days = 0;
			AssertHasErrorContaining(setting1.DaysInfo, string.Format("'{0}' greater than 0.", setting1.DaysInfo.HumanReadableName));

			setting1.Range = PeriodReopenLevels.RangeCodes.DaysAfterEndOfPeriod;
			setting1.Days = -1;
			AssertHasErrorContaining(setting1.DaysInfo, string.Format("'{0}' greater than 0.", setting1.DaysInfo.HumanReadableName));

			PeriodReopenLevels setting2 = collection.AddNew();
			PeriodReopenLevels setting3 = collection.AddNew();

			AssertNoErrors("Precondition: setting2.RangeInfo should not have errors.", setting2.DaysInfo);
			AssertNoErrors("Precondition: setting3.RangeInfo should not have errors.", setting3.DaysInfo);

			setting1.Range = PeriodReopenLevels.RangeCodes.DaysAfterEndOfPeriod;
			setting1.AuthorisationRequirement = PeriodReopenLevels.AuthorisationRequirementCodes.ThirdApprovalRequired;
			setting1.Days = 700;
			setting2.Range = PeriodReopenLevels.RangeCodes.DaysAfterEndOfPeriod;
			setting2.AuthorisationRequirement = PeriodReopenLevels.AuthorisationRequirementCodes.FirstApprovalRequired;
			setting2.Days = 200;
			setting3.Range = PeriodReopenLevels.RangeCodes.DaysAfterEndOfPeriod;
			setting3.AuthorisationRequirement = PeriodReopenLevels.AuthorisationRequirementCodes.SecondApprovalRequired;
			setting3.Days = 200;

			AssertNoErrors(setting1.DaysInfo);
			AssertNoErrors(setting2.DaysInfo);
			AssertHasError(setting3.DaysInfo, "Higher days amounts must have Authorization level higher than lower days amounts.");

			setting3.Days = 300;

			AssertNoErrors(setting1.DaysInfo);
			AssertNoErrors(setting2.DaysInfo);
			AssertNoErrors(setting3.DaysInfo);
		}

		public virtual void TestValidateAuthorisationRequirement()
		{
			AssertNoErrors("Precondition: AuthorisationRequirement should not have errors.", TestBizObj.AuthorisationRequirementInfo);

			Assert("Precondition: AuthorisationRequirementList[0].Code should not be empty.", !string.IsNullOrEmpty(TestBizObj.AuthorisationRequirementList[0].Code));

			TestBizObj.AuthorisationRequirement = "!@#";
			AssertHasError(TestBizObj.AuthorisationRequirementInfo, "Enter a valid selection.");

			TestBizObj.AuthorisationRequirement = TestBizObj.AuthorisationRequirementList[0].Code;
			AssertNoErrors(TestBizObj.AuthorisationRequirementInfo);

			TestBizObj.AuthorisationRequirement = "";
			AssertHasError(TestBizObj.AuthorisationRequirementInfo, "Please enter a value.");

			PeriodReopenLevelsCollection collection = new PeriodReopenLevelsCollection();
			PeriodReopenLevels setting1 = collection.AddNew();
			PeriodReopenLevels setting2 = collection.AddNew();

			setting1.AuthorisationRequirement = PeriodReopenLevels.AuthorisationRequirementCodes.FirstApprovalRequired;
			setting2.AuthorisationRequirement = PeriodReopenLevels.AuthorisationRequirementCodes.FirstApprovalRequired;
			AssertHasError(setting2.AuthorisationRequirementInfo, "There must be only one '1ST' line.");

			setting2.AuthorisationRequirement = PeriodReopenLevels.AuthorisationRequirementCodes.SecondApprovalRequired;
			AssertNoErrors(setting2.AuthorisationRequirementInfo);
		}

		public void TestRunPreSaveValidation()
		{
			TestBizObj.Range = "!@#";
			TestBizObj.AuthorisationRequirement = "!@#";

			TestBizObj.ClearAllNotifications();
			AssertNoErrors("Precondition: There should be no errors.", TestBizObj);

			TestBizObj.RunPreSaveValidation();
			AssertHasErrors(TestBizObj.RangeInfo);
			AssertHasErrors(TestBizObj.DaysInfo);
			AssertHasErrors(TestBizObj.AuthorisationRequirementInfo);
		}

		public void TestRangeList()
		{
			AssertEquals("RangeList.Count", 2, TestBizObj.RangeList.Count);
			AssertEquals("The RangeList should contain 'DAE'", true, TestBizObj.RangeList.ContainsCode("DAE"));
			AssertEquals("The RangeList should contain 'UNL'", true, TestBizObj.RangeList.ContainsCode("UNL"));
		}

		public virtual void TestAuthorisationRequirementList()
		{
			AssertEquals("AuthorisationRequirementList.Count", 3, TestBizObj.AuthorisationRequirementList.Count);
			AssertEquals("AuthorisationRequirementList should contain '1ST'", true, TestBizObj.AuthorisationRequirementList.ContainsCode("1ST"));
			AssertEquals("AuthorisationRequirementList should contain '2ND'", true, TestBizObj.AuthorisationRequirementList.ContainsCode("2ND"));
			AssertEquals("AuthorisationRequirementList should contain '3RD'", true, TestBizObj.AuthorisationRequirementList.ContainsCode("3RD"));
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			PeriodReopenLevels result = (PeriodReopenLevels)GetNewBusinessObject();

			result.Range = result.RangeList[0].Code;
			result.Days = 5;
			result.AuthorisationRequirement = result.AuthorisationRequirementList[0].Code;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		PeriodReopenLevels TestBizObj
		{
			get { return (PeriodReopenLevels)BizObj; }
		}

		#endregion
	}
}

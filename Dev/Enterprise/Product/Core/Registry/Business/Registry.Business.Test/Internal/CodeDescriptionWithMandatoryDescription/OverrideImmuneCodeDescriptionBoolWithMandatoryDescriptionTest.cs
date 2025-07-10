using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OverrideImmuneCodeDescriptionBoolWithMandatoryDescription))]
	class OverrideImmuneCodeDescriptionBoolWithMandatoryDescriptionTest : OverrideImmuneCodeDescriptionBoolTest
	{
		public void TestRunPreSaveValidation()
		{
			TestBizObj.Code = "";
			TestBizObj.EnglishDescription = "";
			TestBizObj.RunPreSaveValidation();
			AssertHasErrors(TestBizObj.CodeInfo);
			AssertHasErrors(TestBizObj.DescriptionInfo);

			TestBizObj.Code = "TST";
			TestBizObj.EnglishDescription = "";
			TestBizObj.RunPreSaveValidation();
			AssertNoErrors(TestBizObj.CodeInfo);
			AssertHasErrors(TestBizObj.DescriptionInfo);

			TestBizObj.Code = "";
			TestBizObj.EnglishDescription = "Test Description";
			TestBizObj.RunPreSaveValidation();
			AssertHasErrors(TestBizObj.CodeInfo);
			AssertNoErrors(TestBizObj.DescriptionInfo);

			TestBizObj.Code = "TST";
			TestBizObj.EnglishDescription = "Test Description";
			TestBizObj.RunPreSaveValidation();
			AssertNoErrors(TestBizObj.CodeInfo);
			AssertNoErrors(TestBizObj.DescriptionInfo);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new OverrideImmuneCodeDescriptionBoolWithMandatoryDescription();
			result.Code = "AAA";
			result.EnglishDescription = "AAA Description";
			result.Bool = true;

			return result;
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			AssertEquals("AAA", clone.Code);
			AssertEquals("AAA Description", clone.Description);
			AssertEquals(true, ((OverrideImmuneCodeDescriptionBoolWithMandatoryDescription)clone).Bool);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OverrideImmuneCodeDescriptionBoolWithMandatoryDescription();
		}

		OverrideImmuneCodeDescriptionBoolWithMandatoryDescription TestBizObj
		{
			get { return (OverrideImmuneCodeDescriptionBoolWithMandatoryDescription)BizObj; }
		}

		#endregion
	}
}

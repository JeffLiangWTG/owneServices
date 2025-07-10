using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolWithMandatoryDescription))]
	sealed class CodeDescriptionBoolWithMandatoryDescriptionTest : CodeDescriptionBoolTest
	{
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new CodeDescriptionBoolWithMandatoryDescription();
			result.Code = "AAA";
			result.EnglishDescription = "AAA Description";
			result.Bool = true;

			return result;
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			AssertEquals("AAA", clone.Code);
			AssertEquals("AAA Description", clone.Description);
			AssertEquals(true, ((CodeDescriptionBoolWithMandatoryDescription)clone).Bool);
		}

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

		#region Implementation

		protected override CargoWise.EntityFramework.BusinessObject GetNewBusinessObject()
		{
			return new CodeDescriptionBoolWithMandatoryDescription();
		}

		CodeDescriptionBoolWithMandatoryDescription TestBizObj
		{
			get { return (CodeDescriptionBoolWithMandatoryDescription)BizObj; }
		}

		#endregion
	}
}

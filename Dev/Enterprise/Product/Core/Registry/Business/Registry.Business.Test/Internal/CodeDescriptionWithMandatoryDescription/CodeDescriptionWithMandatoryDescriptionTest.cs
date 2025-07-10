using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionWithMandatoryDescription))]
	sealed class CodeDescriptionWithMandatoryDescriptionTest : RegistryBusinessObjectTestCaseBase
	{
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new CodeDescriptionWithMandatoryDescription();
			result.Code = "AAA";
			result.EnglishDescription = "AAA Description";

			return result;
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

		protected override BusinessObject GetNewBusinessObject()
		{
			return NewPopulatedBusinessObject();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		CodeDescriptionWithMandatoryDescription TestBizObj
		{
			get { return (CodeDescriptionWithMandatoryDescription)BizObj; }
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override bool IsDescriptionMandatory => true;

		RegistryBusinessObjectTemplate NewPopulatedBusinessObject()
		{
			var result = new CodeDescriptionWithMandatoryDescription();
			result.Code = "TTT";
			result.Description = (NoResString)"Test Template";
			return result;
		}

		#endregion
	}
}

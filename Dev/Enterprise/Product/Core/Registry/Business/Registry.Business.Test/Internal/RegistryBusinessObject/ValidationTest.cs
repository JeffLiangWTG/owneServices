using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DummyRegistryBusinessObject))]
	public class ValidationTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestValidateCodeClearsNotification()
		{
			BizObj.Code = "ABC";
			BizObj.CodeInfo.AddError("Error!");
			BizObj.ValidateCode();
			AssertEquals("Code should not have any errors", false, BizObj.CodeInfo.HasErrors());
		}

		public void TestValidateDescriptionClearsNotification()
		{
			BizObj.DescriptionInfo.AddError("Error!");
			BizObj.ValidateDescription();
			AssertEquals("Description should not have any errors", false, BizObj.DescriptionInfo.HasErrors());
		}

		public void TestValidateCodeCore()
		{
			AssertEquals("Precondition: ValidateCode() should not have been called yet", false, BizObj.ValidateCodeCalled);
			BizObj.Code = "";
			AssertEquals("ValidateCode() should have been called", true, BizObj.ValidateCodeCalled);
			AssertHasErrors(BizObj.CodeInfo);

			BizObj.IsCodeMandatory_Exposed = false;
			BizObj.ValidateCode();
			AssertNoErrors(BizObj.CodeInfo);
		}

		public void TestValidateDescriptionCore()
		{
			AssertEquals("Precondition: ValidateDescription() should not have been called yet", false, BizObj.ValidateDescriptionCalled);
			BizObj.Description = (NoResString)"";
			AssertEquals("ValidateDescription() should have been called", true, BizObj.ValidateDescriptionCalled);
		}

		public void TestRunPreSaveValidation()
		{
			AssertEquals("Precondition: ValidateCode() should not have been called yet", false, BizObj.ValidateCodeCalled);
			AssertEquals("Precondition: ValidateDescription() should not have been called yet", false, BizObj.ValidateDescriptionCalled);

			BizObj.RunPreSaveValidation();
			AssertEquals("ValidateCode() should have been called", true, BizObj.ValidateCodeCalled);
			AssertEquals("ValidateDescription() should have been called", true, BizObj.ValidateDescriptionCalled);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			BizObj.Code = "ABC";
			BizObj.Description = (NoResString)"Desc";

			return BizObj;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		new DummyRegistryBusinessObject BizObj
		{
			get { return (DummyRegistryBusinessObject)base.BizObj; }
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}

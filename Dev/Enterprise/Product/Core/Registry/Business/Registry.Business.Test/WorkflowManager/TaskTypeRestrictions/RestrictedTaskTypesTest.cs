using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RestrictedTaskTypes))]
	sealed class RestrictedTaskTypesTest : RegistryBusinessObjectTest
	{
		public void TestValidationShouldNotChangeObjectState()
		{
			var taskType = new RestrictedTaskTypes
			{
				WorkflowType = "WKI",
				Code = "SHV"
			};
			AssertEquals("Description should be set from the code/pair list", taskType.TaskTypeList.GetDescriptionFromCode("SHV"), taskType.Description);

			//this is non persistent BO which cannot be created by Factory
			//therefore we cannot save this BO in the Factory to clear HasChanges property
			//so we do this manually for testing purpose only
			taskType.HasChanges = false;

			taskType.ValidateCode();
			Assert(@"Validation should not set HasChanges property to true because 
validation is not supposed to change a business object.
We should not set BO properties in validation methods.", !taskType.HasChanges);
		}

		public override void TestMaxDescriptionLength()
		{
			AssertEquals("Custom MaxDescriptionLength", 256, BizObj.DescriptionInfo.MaxLength);
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone1)
		{
			var clone = (RestrictedTaskTypes)clone1;
			AssertEquals("WKI", clone.WorkflowType);
			AssertEquals("COD", clone.Code);
			AssertEquals("Coding", clone.Description);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (RestrictedTaskTypes)base.GetBusinessObjectToClone();
			result.WorkflowType = "WKI";
			result.Code = "COD";

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		new RestrictedTaskTypes BizObj
		{
			get { return (RestrictedTaskTypes)base.BizObj; }
		}

		protected override string CodeDisplayName
		{
			get { return "Task Type"; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			WorkflowDataRegistryTestHelper.SetupTaskTypesRegistry();
			Factory.Save();
		}
	}
}

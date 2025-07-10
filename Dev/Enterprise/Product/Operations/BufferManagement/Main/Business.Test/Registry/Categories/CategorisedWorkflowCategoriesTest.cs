using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Testing
{
	[TestedType(typeof(CategorisedWorkflowCategories))]
	class CategorisedWorkflowCategoriesTest : RegistryBusinessObjectTest
	{
		public void TestCategories()
		{
			var newCategories = new WorkflowCategoryCollection();
			var oldCategories = BizObj.Categories;
			Assert(BizObj.IsRegisteredEditableChildObject(oldCategories));

			BizObj.SetCategories(newCategories);
			Assert(!BizObj.IsRegisteredEditableChildObject(oldCategories));
			Assert(BizObj.IsRegisteredEditableChildObject(newCategories));
			AssertEquals(newCategories, BizObj.Categories);
		}

		public override void TestCloneValues()
		{
			var boToClone = (CategorisedWorkflowCategories)GetBusinessObjectToClone();
			var newObject = (CategorisedWorkflowCategories)boToClone.Clone(null, null);

			Assert(!boToClone.Categories.Equals(newObject.Categories));
			AssertCloneValues(newObject);
		}

		public override void TestMaxDescriptionLength()
		{
			AssertEquals("Custom MaxDescriptionLength", 256, BizObj.DescriptionInfo.MaxLength);
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			var clonedTypes = (CategorisedWorkflowCategories)clone;
			AssertEquals(5, clonedTypes.CodeMaxLength);
			AssertEquals("WRK", clonedTypes.Code);
			AssertEquals("Work Item", clonedTypes.Description);

			AssertEquals(2, clonedTypes.Categories.Count);
			AssertEquals("BLA", clonedTypes.Categories[0].Code);
			AssertEquals("Give me a reason!", clonedTypes.Categories[0].Description);
			AssertEquals("HEY", clonedTypes.Categories[1].Code);
			AssertEquals("I still need a reason", clonedTypes.Categories[1].Description);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (CategorisedWorkflowCategories)GetNewBusinessObject();

			result.CodeMaxLength = 5;
			result.Code = "WRK";
			result.Description = (NoResString)"Work Item";

			var category1 = result.Categories.AddNew();
			category1.Code = "BLA";
			category1.Description = (NoResString)"Give me a reason!";

			var category2 = result.Categories.AddNew();
			category2.Code = "HEY";
			category2.Description = (NoResString)"I still need a reason";

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected new CategorisedWorkflowCategories BizObj => (CategorisedWorkflowCategories)base.BizObj;
	}
}


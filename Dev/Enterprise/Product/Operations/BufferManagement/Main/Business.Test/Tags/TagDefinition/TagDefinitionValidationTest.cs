using CargoWise.EntityFramework.Testing;

namespace Enterprise.BufferManagement.Business.Test
{
	class TagDefinitionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestScope()
		{
			var definition = Factory.New<TagDefinition>();
			definition.Validation.ValidateAll();
			AssertNoErrors(definition.TGD_ScopeInfo);

			definition.TGD_UsageScope = TagUsageScopeList.Codes.Rule;
			definition.TGD_Scope = TagScopeList.Codes.Task;

			AssertHasError(definition.TGD_ScopeInfo, "Tag Rules can only apply tags to workflows");
		}

		public void TestCode()
		{
			var definition = Factory.New<TagDefinition>();
			definition.Validation.ValidateAll();
			AssertHasErrors(definition.TGD_CodeInfo);

			definition.TGD_Code = "WOW";
			AssertNoErrors(definition.TGD_CodeInfo);

			definition.TGD_Code = "";
			AssertHasErrors(definition.TGD_CodeInfo);
		}

		public void TestCode_WhenConflictingWithSystemDefinedTags()
		{
			var definition = Factory.New<TagDefinition>();
			definition.TGD_Code = "WOW";
			AssertNoErrors(definition.TGD_CodeInfo);

			definition.TGD_Code = "CRR";
			AssertHasError(definition.TGD_CodeInfo, "This code conflicts with a system-defined Tag Group.");

			definition.TGD_Code = "NRR";
			AssertHasError(definition.TGD_CodeInfo, "This code conflicts with a system-defined Tag Group.");

			definition.TGD_Code = "QUE";
			AssertHasError(definition.TGD_CodeInfo, "This code conflicts with a system-defined Tag Group.");

			definition.TGD_Code = "LOL";
			AssertNoErrors(definition.TGD_CodeInfo);
		}

		public void TestCode_ForSystemDefinedTag()
		{
			var definition = TagProvider.GetCCPMReleaseTagGroup(Factory);
			definition.Validation.ValidateAll();
			AssertNoErrors(definition.TGD_CodeInfo);
		}

		public void TestCode_Duplicates()
		{
			var definition = Factory.New<TagDefinition>();
			definition.Validation.ValidateAll();
			AssertHasErrors(definition.TGD_CodeInfo);

			definition.TGD_Code = "WOW";
			AssertNoErrors(definition.TGD_CodeInfo);

			definition.TGD_Code = "";
			AssertHasErrors(definition.TGD_CodeInfo);

			definition.TGD_Code = "WOW";
			AssertNoErrors(definition.TGD_CodeInfo);

			var moarDefinition = Factory.New<TagDefinition>();
			moarDefinition.TGD_Code = "WOW";

			moarDefinition.Validation.ValidateAll();
			AssertHasErrors(moarDefinition.TGD_CodeInfo);
		}

		public void TestDescription()
		{
			var definition = Factory.New<TagDefinition>();

			definition.Validation.ValidateAll();
			AssertHasErrors(definition.TGD_DescriptionInfo);

			definition.TGD_Description = "I'm feeling uncomfortably brilliant today";
			AssertNoErrors(definition.TGD_DescriptionInfo);

			definition.TGD_Description = "";
			AssertHasErrors(definition.TGD_DescriptionInfo);
		}
	}
}

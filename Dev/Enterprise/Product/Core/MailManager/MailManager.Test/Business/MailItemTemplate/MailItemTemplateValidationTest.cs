using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MailManager.Business.Testing
{
	sealed class MailItemTemplateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAll()
		{
			var template = Factory.New<MailItemTemplate>();
			template.MIT_Name = "";
			template.MIT_Category = "";
			template.MIT_Language = "";
			template.Validation.ValidateAll();
			Assert("Name, Category and Language are empty, expecting error", template.HasErrors());
			Assert("Name is empty, expecting error", template.MIT_NameInfo.HasErrors());
			Assert("Category is empty, expecting error", template.MIT_CategoryInfo.HasErrors());
			Assert("Language is empty, expecting error", template.MIT_LanguageInfo.HasErrors());

			template.MIT_Name = "template";
			template.MIT_Category = MailDBItemTemplateLookups.New().MailTemplateCategories[0].Code;
			template.MIT_Language = MailDBItemTemplateLookups.New().Languages[0].Code;
			template.Validation.ValidateAll();
			Assert("Name, Category and Language are not empty, expecting no errors", !template.HasErrors());
			Assert("Name is not empty, expecting no errors", !template.MIT_NameInfo.HasErrors());
			Assert("Category is not empty, expecting no errors", !template.MIT_CategoryInfo.HasErrors());
			Assert("Language is not empty, expecting no errors", !template.MIT_LanguageInfo.HasErrors());
		}

		public void TestValidateTemplateID()
		{
			var template = Factory.New<MailItemTemplate>();
			template.MIT_Name = "";
			template.MIT_Category = "";
			template.Validation.ValidateTemplateID();
			AssertEquals("Precondition", "", template.TemplateID);
			Assert("TemplateID is empty, not expecting error", !template.TemplateIDInfo.HasErrors());

			template.MIT_Name = "template";
			template.MIT_Category = "ABC";
			template.Validation.ValidateTemplateID();
			AssertEquals("Precondition", "ABC_template", template.TemplateID);
			Assert("TemplateID is unique, not expecting error", !template.TemplateIDInfo.HasErrors());
			Factory.Save();

			var template2 = Factory.New<MailItemTemplate>();
			template2.MIT_Name = "template";
			template2.MIT_Category = "ABC";
			template2.Validation.ValidateTemplateID();
			AssertEquals("Precondition", "ABC_template", template2.TemplateID);
			Assert("TemplateID is not unique, expecting error", template2.TemplateIDInfo.HasErrors());

			template2.MIT_Name = "Default";
			template2.Validation.ValidateTemplateID();
			AssertEquals("Precondition", "ABC_Default", template2.TemplateID);
			Assert("TemplateID is unique, not expecting error", !template.TemplateIDInfo.HasErrors());
		}

		public void TestValidateBranch()
		{
			var template = Factory.New<MailItemTemplate>();
			template.MIT_GC_Company = GlbCompany.CurrentCompany.PK;
			template.MIT_GB_Branch = GlbBranch.CurrentBranch.PK;
			AssertNoErrors(template.MIT_GB_BranchInfo);

			template.MIT_GB_Branch = Factory.New<GlbBranch>().PK;
			AssertHasError(template.MIT_GB_BranchInfo, "The selected branch doesn't belong to the specified company.");

			template.MIT_GB_Branch = GlbBranch.CurrentBranch.PK;
			AssertNoErrors(template.MIT_GB_BranchInfo);

			template.MIT_GC_Company = Factory.New<GlbCompany>().PK;
			AssertHasError(template.MIT_GB_BranchInfo, "The selected branch doesn't belong to the specified company.");
		}
	}
}

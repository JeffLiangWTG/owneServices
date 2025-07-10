using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MailManager.Module.Testing
{
	[TestedType(typeof(MailItemTemplateFilterBusinessObject))]
	class MailItemTemplateFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new MailItemTemplateFilterBusinessObject();
		}

		public void TestCategoryFilter()
		{
			var filterBizO = new MailItemTemplateFilterBusinessObject();
			var categoryFilter = ((ModuleTextFilter)filterBizO["Category"]);
			AssertContainsExactElementsInAnyOrder(MailDBItemTemplateLookups.New().MailTemplateCategories, categoryFilter.List);
			AssertEquals("Category filter does not have IsBlank operator", false, categoryFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			AssertEquals("Category filter does not have IsNotBlank operator", false, categoryFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));
		}

		public void TestLanguageFilter()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Japanese))
			{
				var filterBizO = new MailItemTemplateFilterBusinessObject();
				var languageFilter = ((ModuleTextFilter)filterBizO["Language"]);
				AssertContainsExactElementsInAnyOrder(MailDBItemTemplateLookups.New().Languages, languageFilter.List);
				AssertEquals("Language filter does not have IsBlank operator", false, languageFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
				AssertEquals("Language filter does not have IsNotBlank operator", false, languageFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));
				AssertEquals(Core.SharedConstants.Languages.Japanese, languageFilter.DefaultProperty);
				AssertEquals(FilterVisibility.AlwaysVisible, languageFilter.Visibility);
			}
		}

		public void TestCompanyFilter()
		{
			var allCompanies = Factory.Load<GlbCompany>(new ZQuery());
			Env.Security.ViewAllEmailTemplates.IsAllowed = true;
			var filterBizO = new MailItemTemplateFilterBusinessObject();
			var companyFilter = (ModuleGuidFilter)filterBizO["Company"];
			AssertEquals(FilterVisibility.Visible, companyFilter.Visibility);
			AssertContainsExactElementsInAnyOrder(allCompanies.Select(x => x.PK), companyFilter.List.Cast<GlbCompany>().Select((x => x.PK)));
			Env.Security.ViewAllEmailTemplates.IsAllowed = false;
			filterBizO = new MailItemTemplateFilterBusinessObject();
			companyFilter = (ModuleGuidFilter)filterBizO["Company"];
			AssertEquals(FilterVisibility.AlwaysVisible, companyFilter.Visibility);
			AssertEquals(GlbCompany.CurrentCompany.PK, companyFilter.Property);
			companyFilter.Property = ZGuid.NewZGuid();
			Assert(companyFilter.PropertyInfo.HasError(@"Your security rights only allow you to view email templates belonging to your current login company, branch and department.
If you think this is incorrect, please contact your system administrator."));
		}

		public void TestBranchFilter()
		{
			var allBranches = Factory.Load<GlbBranch>(new ZQuery());
			Env.Security.ViewAllEmailTemplates.IsAllowed = true;
			var filterBizO = new MailItemTemplateFilterBusinessObject();
			var branchFilter = (ModuleGuidFilter)filterBizO["Branch"];
			AssertEquals(FilterVisibility.Visible, branchFilter.Visibility);
			((GlbBranchCollection)branchFilter.List).Load();
			AssertContainsExactElementsInAnyOrder(allBranches.Select(x => x.PK), branchFilter.List.Cast<GlbBranch>().Select((x => x.PK)));
			Env.Security.ViewAllEmailTemplates.IsAllowed = false;
			filterBizO = new MailItemTemplateFilterBusinessObject();
			branchFilter = (ModuleGuidFilter)filterBizO["Branch"];
			AssertEquals(FilterVisibility.AlwaysVisible, branchFilter.Visibility);
			AssertEquals(GlbBranch.CurrentBranch.PK, branchFilter.Property);
			branchFilter.Property = ZGuid.NewZGuid();
			Assert(branchFilter.PropertyInfo.HasError(@"Your security rights only allow you to view email templates belonging to your current login company, branch and department.
If you think this is incorrect, please contact your system administrator."));
		}

		public void TestDepartmentFilter()
		{
			var allDepartments = Factory.Load<GlbDepartment>(new ZQuery());
			Env.Security.ViewAllEmailTemplates.IsAllowed = true;
			var filterBizO = new MailItemTemplateFilterBusinessObject();
			var departmentFilter = (ModuleGuidFilter)filterBizO["Department"];
			AssertEquals(FilterVisibility.Visible, departmentFilter.Visibility);
			AssertContainsExactElementsInAnyOrder(allDepartments.Select(x => x.PK), departmentFilter.List.Cast<GlbDepartment>().Select((x => x.PK)));
			Env.Security.ViewAllEmailTemplates.IsAllowed = false;
			filterBizO = new MailItemTemplateFilterBusinessObject();
			departmentFilter = (ModuleGuidFilter)filterBizO["Department"];
			AssertEquals(FilterVisibility.AlwaysVisible, departmentFilter.Visibility);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, departmentFilter.Property);
			departmentFilter.Property = ZGuid.NewZGuid();
			Assert(departmentFilter.PropertyInfo.HasError(@"Your security rights only allow you to view email templates belonging to your current login company, branch and department.
If you think this is incorrect, please contact your system administrator."));
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Res = MailManager.Module.Res;
using ResString = MailManager.Module.ResString;

namespace Enterprise.MailManager.Module
{
	public class MailItemTemplateFilterBusinessObject : FilterStripBusinessObject
	{
		public MailItemTemplateFilterBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddGuidFilters(filters);

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var nameFilter = filters.AddTextFilter("Name", MailDBItemTemplateSchema.MIT_Name);
			nameFilter.MultilingualDescription = ResString.GetMultilingualString("MaiItemTemplate|Filter|Name", "Name");
			var categoryFilter = filters.AddTextFilter("Category", MailDBItemTemplateSchema.MIT_Category, Lookups.MailTemplateCategories);
			categoryFilter.MultilingualDescription = ResString.GetMultilingualString("MaiItemTemplate|Filter|Category", "Category");
			categoryFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			categoryFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			var languageFilter = filters.AddTextFilter("Language", MailDBItemTemplateSchema.MIT_Language, Lookups.Languages);
			languageFilter.MultilingualDescription = ResString.GetMultilingualString("MaiItemTemplate|Filter|Language", "Language");
			languageFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			languageFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			languageFilter.DefaultProperty = Res.CurrentLanguage;
			languageFilter.Visibility = FilterVisibility.AlwaysVisible;
		}

		void AddGuidFilters(ModuleFilterCollection filters)
		{
			var companyFilter = filters.AddGuidFilter("Company", ModuleIDs.GlbCompany, GetCompanyQuery, Companies);
			companyFilter.Property = GlbCompany.CurrentCompany.PK;
			companyFilter.MultilingualDescription = ResString.GetMultilingualString("MaiItemTemplate|Filter|Company", "Company");

			var branchFilter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, GetBranchQuery, Branches);
			branchFilter.Property = GlbBranch.CurrentBranch.PK;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("MaiItemTemplate|Filter|Branch", "Branch");

			var departmentFilter = filters.AddGuidFilter("Department", ModuleIDs.GlbDepartment, GetDepartmentQuery, Departments);
			departmentFilter.Property = GlbDepartment.CurrentDepartment.PK;
			departmentFilter.MultilingualDescription = ResString.GetMultilingualString("MaiItemTemplate|Filter|Department", "Department");

			if (!Env.Security.ViewAllEmailTemplates.IsAllowed)
			{
				companyFilter.Visibility = FilterVisibility.AlwaysVisible;
				companyFilter.DefaultProperty = GlbCompany.CurrentCompany.PK;
				companyFilter.PropertyValidation = CompanyFilterValidation;

				branchFilter.Visibility = FilterVisibility.AlwaysVisible;
				branchFilter.DefaultProperty = GlbBranch.CurrentBranch.PK;
				branchFilter.PropertyValidation = BranchFilterValidation;

				departmentFilter.Visibility = FilterVisibility.AlwaysVisible;
				departmentFilter.DefaultProperty = GlbDepartment.CurrentDepartment.PK;
				departmentFilter.PropertyValidation = DepartmentFilterValidation;
			}
		}

		void CompanyFilterValidation(ZPropertyInfo info)
		{
			if ((ZGuid)info.Value != GlbCompany.CurrentCompany.PK)
			{
				info.AddError(CompanyBranchDepartmentFilterErrorMessage);
			}
		}

		void BranchFilterValidation(ZPropertyInfo info)
		{
			if ((ZGuid)info.Value != GlbBranch.CurrentBranch.PK)
			{
				info.AddError(CompanyBranchDepartmentFilterErrorMessage);
			}
		}

		void DepartmentFilterValidation(ZPropertyInfo info)
		{
			if ((ZGuid)info.Value != GlbDepartment.CurrentDepartment.PK)
			{
				info.AddError(CompanyBranchDepartmentFilterErrorMessage);
			}
		}

		string CompanyBranchDepartmentFilterErrorMessage
		{
			get { return Res.GetString("5FE76850-2912-42B8-AE97-AB51D73817E2", @"Your security rights only allow you to view email templates belonging to your current login company, branch and department.
If you think this is incorrect, please contact your system administrator."); }
		}

		ZQuery GetCompanyQuery(ZGuid companyPK)
		{
			return GetQuery(companyPK, MailDBItemTemplateSchema.MIT_GC_Company);
		}

		ZQuery GetBranchQuery(ZGuid branchPK)
		{
			return GetQuery(branchPK, MailDBItemTemplateSchema.MIT_GB_Branch);
		}

		ZQuery GetDepartmentQuery(ZGuid departmentPK)
		{
			return GetQuery(departmentPK, MailDBItemTemplateSchema.MIT_GE_Department);
		}

		ZQuery GetQuery(ZGuid value, SchemaGuidColumn column)
		{
			var result = new ZQuery();
			if (value.IsValid)
			{
				result.AddToFilter(column, value);
				result.AddToFilter(JoinCondition.Or, column, null);
			}
			return result;
		}

		#region Lookups

		MailDBItemTemplateLookups Lookups
		{
			get { return lookups ?? (lookups = MailDBItemTemplateLookups.New()); }
		}
		MailDBItemTemplateLookups lookups;

		#region Companies

		GlbCompanyCollection Companies
		{
			get { return companies ?? (companies = new GlbCompanyCollection(Factory)); }
		}
		GlbCompanyCollection companies;

		#endregion

		#region Branches

		GlbBranchCollection Branches
		{
			get { return branches ?? (branches = new GlbBranchCollection(Factory)); }
		}
		GlbBranchCollection branches;

		#endregion

		#region Companies

		GlbDepartmentCollection Departments
		{
			get { return departments ?? (departments = new GlbDepartmentCollection(Factory)); }
		}
		GlbDepartmentCollection departments;

		#endregion

		#endregion
	}
}

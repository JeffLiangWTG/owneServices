using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GeneralLedger.GLBudget;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class GLBudgetFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			ModuleNumberFilter yearFilter = filters.AddNumberFilter("Year", GetYearQuery);
			yearFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLBudgetFilter|Year", "Year");
			yearFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			yearFilter.UseMultiSearch = false;

			yearFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.Contains);
			yearFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.StartsWith);
			yearFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.NotStartsWith);
			yearFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.NotContain);
			yearFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.NotEqual);
			yearFilter.PropertyValidation = YearValidation;

			var filter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, AccGLBudgetSchema.AU_GB, AU_GB_List);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLBudgetFilter|Branch", "Branch");

			filter = filters.AddGuidFilter("Department", ModuleIDs.GlbDepartment, AccGLBudgetSchema.AU_GE, AU_GE_List);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLBudgetFilter|Department", "Department");

			filter = filters.AddGuidFilter("GL Account", ModuleIDs.AccGLHeader, AccGLBudgetSchema.AU_AG, AU_AG_List);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|GLBudgetFilter|GLAccount", "GL Account");

			AddBranchManagementCodeFilter(filters);

			return filters;
		}

		#region Filter Delegates

		#region GetBranchManagementCodeQuery

		protected override ZQuery GetBranchManagementCodeQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(GLBudget));

			if (!value.IsEmpty)
			{
				var branchManagementCodeQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
				branchManagementCodeQuery.AddToFilter(GlbBranchSchema.GB_AccountingGroupCode, value);
				result.AddSubQuery(AccGLBudgetSchema.AU_GB, branchManagementCodeQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion

		ZQuery GetYearQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();

			if (ZInt.CanParse(value))
			{
				ZInt year = ZInt.Parse(value);
				query.AddToFilter(AccGLBudgetSchema.AU_Year, SQLComparisonOperator.Equal, year);
			}

			return query;
		}

		#endregion

		#region Validation

		void YearValidation(ZPropertyInfo info)
		{
			ZString valueAsString = (ZString)info.Value;

			info.Value = valueAsString.TrimStart(' ', '0').TrimEnd(' ');

			if (!valueAsString.IsEmpty)
			{
				if (!ZInt.CanParse(valueAsString) || valueAsString.Length > 4)
				{
					info.AddError(Res.GetString("b4d929e4-c155-46dc-9ee2-f228dea5798f", "Year format is not valid"));
				}
			}
		}

		#endregion

		#region AU_AG_List

		AccGLHeaderCollection fAU_AG_List;
		public AccGLHeaderCollection AU_AG_List
		{
			get
			{
				if (fAU_AG_List == null)
				{
					fAU_AG_List = new AccGLHeaderCollection(Factory);
				}

				return fAU_AG_List;
			}
		}

		#endregion

		#region AU_GB_List

		GlbBranchCollection fAU_GB_List;
		public GlbBranchCollection AU_GB_List
		{
			get
			{
				if (fAU_GB_List == null)
				{
					fAU_GB_List = new GlbBranchCollection(Factory);
				}

				return fAU_GB_List;
			}
		}

		#endregion

		#region AU_GE_List

		GlbDepartmentCollection fAU_GE_List;
		public GlbDepartmentCollection AU_GE_List
		{
			get
			{
				if (fAU_GE_List == null)
				{
					fAU_GE_List = new GlbDepartmentCollection(Factory);
				}

				return fAU_GE_List;
			}
		}

		#endregion
	}
}


using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public interface IGenericChargeCollectionRequired
	{ //when adding new fields that alter collection query update KeyForCollectionCachingInFactory method to include the fields
		bool IsJobRelated { get; }
		GlbDepartment Department { get; }
		BusinessObjectFactory Factory { get; }
	}

	internal static class IGenericChargeCollectionRequiredExtentions
	{
		internal static string KeyForCollectionCachingInFactory(this IGenericChargeCollectionRequired keyInput)
		{
			return (keyInput.Department != null ? keyInput.Department.PK.ToStringKey() : "") + keyInput.IsJobRelated;
		}
	}

	public abstract partial class GenericChargeCollectionBuilder
	{
		protected GenericChargeCollectionBuilder(IGenericChargeCollectionRequired parent)
		{
			Factory = parent.Factory;
			Department = parent.Department;
			IsJobRelated = parent.IsJobRelated;
		}

		protected BusinessObjectFactory Factory;
		protected GlbDepartment Department;
		protected bool IsJobRelated;

		public void SetJobInfo(IGenericChargeCollectionRequired parent)
		{
			Factory = parent.Factory;
			Department = parent.Department;
			IsJobRelated = parent.IsJobRelated;
		}

		public GenericChargeCollection GetBuiltButNotLoadedCollection(Action<AccGLHeaderCollection, List<AccGLHeader>> showGLAccountsForImportAction = null)
		{
			return new GenericChargeCollection(Factory, GenerateFilter(), showGLAccountsForImportAction);
		}

		public ZQuery GetQueryForValidation()
		{
			return GenerateValidationFilter();
		}

		protected virtual ZQuery GenerateValidationFilter()
		{
			ZQuery result = new ZQuery(ChargeTypeFilter());
			ZQuery departmentFilterListQuery = new ZQuery();
			if (Department != null)
			{
				departmentFilterListQuery.AddToFilter(ViewGenericChargeSchema.VC_DepartmentFilterList, SQLComparisonOperator.Contains, "ALL");
				departmentFilterListQuery.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_DepartmentFilterList, SQLComparisonOperator.Contains, Department.GE_Code);
			}
			else
			{
				departmentFilterListQuery.AddToFilter(ViewGenericChargeSchema.VC_DepartmentFilterList, SQLComparisonOperator.Equal, ZString.Empty);
			}
			result.AddToFilter(JoinCondition.And, ViewGenericChargeSchema.VC_DisallowDirectPosting, SQLComparisonOperator.Equal, false);
			result.AddToFilter(departmentFilterListQuery);

			return result;
		}

		protected virtual ZQuery GenerateFilter()
		{
			var result = new ZDBOnlyQuery(typeof(AutoViewGenericCharge));

			var commonFilter = new ZQuery(GLAccountFilterForDisplay());
			commonFilter.AddToFilter(ChargeTypeFilterForDisplay(), JoinCondition.Or);
			commonFilter.AddToFilter(JoinCondition.And, ViewGenericChargeSchema.VC_DisallowDirectPosting, SQLComparisonOperator.Equal, false);

			var chargeCodeQuery = new ZDBOnlySubQuery(typeof(AutoViewGenericCharge), ViewGenericChargeSchema.PK);
			chargeCodeQuery.AddToFilter(JoinCondition.And, ViewGenericChargeSchema.VC_TableName, "Charge Code");
			chargeCodeQuery.AddToFilter(commonFilter);

			var globalGLHeaderSubQuery = new ZDBOnlySubQuery(typeof(AccGLHeader), AccGLHeaderSchema.PK);
			globalGLHeaderSubQuery.AddToFilter(AccGLHeaderSchema.AG_IsGlobal, true);
			globalGLHeaderSubQuery.AddToFilter(commonFilter);

			var nonGlobalGLHeaderSubQuery = new ZDBOnlySubQuery(typeof(AutoViewGenericCharge), ViewGenericChargeSchema.PK);
			var isGlobalFilterSubQuery = new ZDBOnlySubQuery(typeof(AccGLHeader), AccGLHeaderSchema.PK);
			isGlobalFilterSubQuery.AddToFilter(AccGLHeaderSchema.AG_IsGlobal, false);
			var companyFilterSubQuery = new ZDBOnlySubQuery(typeof(AccGLHeaderCompanyFilter), AccGLHeaderCompanyFilterSchema.ACF_AG_Header);
			companyFilterSubQuery.AddToFilter(AccGLHeaderCompanyFilterSchema.ACF_GC_Company, GlbCompany.CurrentCompany.PK);
			nonGlobalGLHeaderSubQuery.AddSubQuery(isGlobalFilterSubQuery, JoinCondition.And);
			nonGlobalGLHeaderSubQuery.AddSubQuery(companyFilterSubQuery, JoinCondition.And);
			nonGlobalGLHeaderSubQuery.AddToFilter(JoinCondition.And, ViewGenericChargeSchema.VC_TableName, "GL Account");
			nonGlobalGLHeaderSubQuery.AddToFilter(commonFilter);

			chargeCodeQuery.AddAsUnionQuery(globalGLHeaderSubQuery, true);
			chargeCodeQuery.AddAsUnionQuery(nonGlobalGLHeaderSubQuery, true);

			result.AddSubQuery(chargeCodeQuery, JoinCondition.And);

			return result;
		}

		protected virtual ZQuery ChargeTypeFilter()
		{
			return GLAccountFilter();
		}

		protected virtual ZQuery GLAccountFilter()
		{
			ZQuery gLAccountFilter = new ZQuery(ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.AccountType.ProfitAndLossAccount);
			ZQuery balanceSheetFilter = new ZQuery(ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.AccountType.BalanceSheetAccount);
			balanceSheetFilter.AddToFilter(JoinCondition.And, ViewGenericChargeSchema.VC_IsControlAccount, SQLComparisonOperator.Equal, false);
			gLAccountFilter.AddToFilter(balanceSheetFilter, JoinCondition.Or);

			return gLAccountFilter;
		}

		protected ZQuery GLAccountFilterForDisplay()
		{
			ZQuery gLAccountFilter = new ZQuery(ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.AccountType.ProfitAndLossAccount);
			ZQuery balanceSheetFilter = new ZQuery(ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.AccountType.BalanceSheetAccount);
			balanceSheetFilter.AddToFilter(JoinCondition.And, ViewGenericChargeSchema.VC_IsControlAccount, SQLComparisonOperator.Equal, false);
			gLAccountFilter.AddToFilter(balanceSheetFilter, JoinCondition.Or);

			return gLAccountFilter;
		}

		protected virtual ZQuery ChargeTypeFilterForDisplay()
		{
			ZQuery result = new ZQuery();

			result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Comment);
			result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Margin);
			result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Disbursement);
			result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.NonAccrual);
			result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Overhead);
			result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.ManualJobAccrual);

			return result;
		}
	}

	public class APGenericChargeCollectionBuilder : GenericChargeCollectionBuilder
	{
		public APGenericChargeCollectionBuilder(IGenericChargeCollectionRequired requestor)
			: base(requestor)
		{
		}

		protected override ZQuery GenerateFilter()
		{
			ZQuery result = base.GenerateFilter();
			return result;
		}

		protected override ZQuery ChargeTypeFilter()
		{
			ZQuery result = base.ChargeTypeFilter();
			if (IsJobRelated)
			{
				result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Comment);
				result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Margin);
				result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Disbursement);
				result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.ManualJobAccrual);
			}
			else
			{
				result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Comment);
				result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.NonAccrual);
				result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Overhead);
			}
			return result;
		}

		protected override ZQuery GLAccountFilter()
		{
			if (!IsJobRelated)
			{
				return base.GLAccountFilter();
			}
			else
			{
				return new ZQuery();
			}
		}
	}

	public class ARGenericChargeCollectionBuilder : GenericChargeCollectionBuilder
	{
		public ARGenericChargeCollectionBuilder(IGenericChargeCollectionRequired requestor)
			: base(requestor)
		{
		}

		protected override ZQuery ChargeTypeFilter()
		{
			ZQuery result = base.ChargeTypeFilter();
			result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Comment);
			result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Revenue);
			result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.NonAccrual);
			return result;
		}

		protected override ZQuery ChargeTypeFilterForDisplay()
		{
			return ChargeTypeFilter();
		}
	}

	public class ARAmendingChargeCollectionBuilder : ARGenericChargeCollectionBuilder
	{
		public ARAmendingChargeCollectionBuilder(IGenericChargeCollectionRequired requestor)
			: base(requestor)
		{
		}

		protected override ZQuery ChargeTypeFilter()
		{
			ZQuery result = base.ChargeTypeFilter();
			result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, new string[] {
				Core.Constants.ChargeType.Margin,
				Core.Constants.ChargeType.Disbursement,
				Core.Constants.ChargeType.ManualJobAccrual
			});
			return result;
		}
	}

	public class APOverheadsGenericChargeCollectionBuilder : GenericChargeCollectionBuilder
	{
		public APOverheadsGenericChargeCollectionBuilder(IGenericChargeCollectionRequired requestor)
			: base(requestor)
		{
		}

		protected override ZQuery GenerateFilter()
		{
			ZQuery result = base.GenerateFilter();
			return result;
		}

		protected override ZQuery ChargeTypeFilter()
		{
			ZQuery result = base.ChargeTypeFilter();
			result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.NonAccrual);
			result.AddToFilter(JoinCondition.Or, ViewGenericChargeSchema.VC_Type, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Overhead);
			return result;
		}

		protected override ZQuery ChargeTypeFilterForDisplay()
		{
			return ChargeTypeFilter();
		}
	}
}

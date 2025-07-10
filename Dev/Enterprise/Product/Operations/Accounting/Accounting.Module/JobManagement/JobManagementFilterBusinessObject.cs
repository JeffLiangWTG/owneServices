using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class JobManagementFilterBusinessObject : JobManagementFilterBusinessObjectBase
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = base.GetModuleFiltersCore();
			AccountingFilterStrip.AddAmountFilters(filters);
			AddJobHeaderFilters(filters);
			AddMissingInvalidJobParentFilter(filters);
			return filters;
		}

		#region Other Operations Filters

		protected override void AddOtherOperationsFilters(ModuleFilterCollection filters)
		{
			base.AddOtherOperationsFilters(filters);
			AddDSBJobCloseBatchFilters(filters);
		}

		void AddDSBJobCloseBatchFilters(ModuleFilterCollection filters)
		{
			if (AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.Value)
			{
				ModuleTextFilter profitLossReasonFilter = filters.AddTextFilter("Disbursement Job Close Batch #", GetDSBJobCloseBatchQuery);
				profitLossReasonFilter.Category = OperationsFiltersCategory;
				profitLossReasonFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|DSBJobCloseBatch", "Disbursement Job Close Batch #");
			}
		}

		ZQuery GetDSBJobCloseBatchQuery(SQLComparisonOperator @operator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobHeader));

			var batchSubQuery = new ZDBOnlySubQuery(typeof(DsbJobCloseBatch), DsbJobCloseBatchSchema.PK);
			batchSubQuery.AddToFilter_PossiblyCommaSeparated(DsbJobCloseBatchSchema.JBB_BatchNumber, @operator, value);
			batchSubQuery.AddToFilter(DsbJobCloseBatchSchema.JBB_GC, GlbCompany.CurrentCompany.PK);

			var lineSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_JH);
			lineSubQuery.AddSubQuery(AccTransactionLinesSchema.AL_JBB, batchSubQuery, JoinCondition.And);

			result.AddSubQuery(JobHeaderSchema.PK, lineSubQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region Job Header Filter
		void AddJobHeaderFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter profitLossReasonFilter = filters.AddTextFilter("Profit/Loss Reason", JobHeaderSchema.JH_ProfitLossReasonCode, ProfitLossReasonList);
			profitLossReasonFilter.Category = JobHeaderCategory;
			profitLossReasonFilter.PropertyValidation = ValidateProfitLossReason;
			profitLossReasonFilter.Validation.ValidateProperty();
			profitLossReasonFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|ProfitLossReason", "Profit/Loss Reason");
		}

		void AddMissingInvalidJobParentFilter(ModuleFilterCollection filters)
		{
			var filterName = ResString.GetMultilingualString("bf1bc6bb-c5b8-4e91-8896-6d02bfb9b0f9", "Missing/Invalid Job Parent");
			var missingInvalidJobParentFilter = filters.AddFlagsFilter(filterName.EnglishText,
				new string[] { Res.GetString("2eb4b8c0-21b6-4751-8514-703f81fa30b1", "Jobs without a valid parent") }, new GetFlagsQuery[] { GetMissingInvalidJobParentQuery });
			missingInvalidJobParentFilter.Category = FilterCategories.StatusAndFlags;
			missingInvalidJobParentFilter.MultilingualDescription = filterName;
		}

		ZQuery GetMissingInvalidJobParentQuery(ZBool value)
		{
			return value ?
				new ZDBOnlyQuery(typeof(JobHeader)).AddFilterAndZSQLParameterCollection(MissingInvalidJobParentQuery, new ZSqlParameterCollection()) :
				new ZQuery();
		}

		#endregion

		#region Job Header Validation

		protected void ValidateProfitLossReason(ZPropertyInfo info)
		{
			if (ProfitLossReasonList.Count == 0)
			{
				info.AddWarning(Res.GetString("Accounting|JobManagementFilter|ProfitLossReasonValidationWarning", "{0} registry item ({1}) is empty. Please set correct values.", AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.Caption, AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.Category));
			}
		}

		#endregion

		#region Lists

		public CodeDescriptionPairList ProfitLossReasonList
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.Value.GetCodeDescriptionPairList();
			}
		}
		#endregion

		#region BulkJobClosure Filter
		public ZQuery GetAdditionalFilterForBulkJobClosure(ZQuery query)
		{
			var excludeJobPkFilter = @"
			JH_PK NOT IN 
			(	
				SELECT	distinct AL_JH 
				FROM	dbo.AccTransactionLines AL 
				WHERE	AL.AL_LineType IN ('REV', 'CST', 'WIP', 'ACR') 
						AND
						AL.AL_ReverseDate IS NULL
						AND AL_JH IS NOT NULL
				UNION
		
				SELECT	distinct JR_JH 
				FROM	dbo.JobCharge JR 
				WHERE	(JR_AL_ARLine IS NULL AND JR_LocalSellAmt > 0)
						OR
						(JR_AL_APLine IS NULL AND JR_LocalCostAmt > 0)";

			var checker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
			if (checker.IsReceivablesCashAdvanceFunctionalityEnabled || checker.IsPayablesCashAdvanceFunctionalityEnabled)
			{
				excludeJobPkFilter = excludeJobPkFilter + @"
				UNION

				SELECT DISTINCT CAH_JH_JOB
				FROM dbo.AccCashAdvanceRequestHeader
				WHERE CAH_Status NOT IN ('CAN', 'INV')
			)	";
			}
			else
			{
				excludeJobPkFilter = excludeJobPkFilter + @"
			)	";
			}

			GetAdditionalQueryThatAlwaysNeedsToAppended(query);
			query.AddFilterAndZSQLParameterCollection(excludeJobPkFilter, null);

			return query;
		}

		#endregion
	}
}

using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class BulkDSBJobCloseBatchApprovalFilterBusinessObject : FilterStripBusinessObject
	{
		public BulkDSBJobCloseBatchApprovalFilterBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddFilters(filters);
			return filters;
		}

		protected void AddFilters(ModuleFilterCollection filters)
		{
			var batchNumberFilter = filters.AddNumberFilter(AccountingUtils.DsbJobCloseBatchFilterTypes.BatchNumber, GetBatchNumberQuery).WithMaxLengthOf(DsbJobCloseBatchSchema.JBB_BatchNumber);
			batchNumberFilter.Category = FilterCategories.NumbersAndReferences;
			batchNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|BulkDSBJobCloseBatchApprovalFilter|BatchNumber", "Batch Number");

			var approvingUserFilter = filters.AddNkFilter(AccountingUtils.DsbJobCloseBatchFilterTypes.ApprovingUser, DsbJobCloseBatchSchema.JBB_GS_NKApprovingUser, ModuleIDs.GlbStaff, Staffs);
			approvingUserFilter.Category = FilterCategories.Organisations;
			approvingUserFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|BulkDSBJobCloseBatchApprovalFilter|ApprovingUser", "Approving User");

			var approvalStatusFilter = filters.AddTextFilter(AccountingUtils.DsbJobCloseBatchFilterTypes.ApprovalStatus, GetApprovalStatusQuery, ApprovalStatusList);
			approvalStatusFilter.Category = FilterCategories.StatusAndFlags;
			approvalStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|BulkDSBJobCloseBatchApprovalFilter|ApprovalStatus", "Approval Status");

			var approvalDateFilter = filters.AddDateFilter(AccountingUtils.DsbJobCloseBatchFilterTypes.ApprovalDate, DsbJobCloseBatchSchema.JBB_ApprovalTimeUtc, true);
			approvalDateFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|BulkDSBJobCloseBatchApprovalFilter|ApprovedDate", "Approved Date");
		}

		#region Query

		ZQuery GetBatchNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!value.IsEmpty)
			{
				ZQuery query = new ZQuery();
				query.AddToFilter_PossiblyCommaSeparated(DsbJobCloseBatchSchema.JBB_BatchNumber, comparisonOperator, value);
				return query;
			}
			else
			{
				return null;
			}
		}

		ZQuery GetApprovalStatusQuery(ZString value)
		{
			if (!value.IsEmpty)
			{
				return new ZQuery(DsbJobCloseBatchSchema.JBB_BatchStatus, SQLComparisonOperator.Equal, value);
			}
			else
			{
				return null;
			}
		}

		#endregion

		#region List

		ICodeDescriptionPairList ApprovalStatusList
		{
			get { return fApprovalStatusList ?? (fApprovalStatusList = DsbJobCloseBatchLookups.DSBJobCloseBatchStatusList); }
		}
		ICodeDescriptionPairList fApprovalStatusList;

		GlbStaffCollection Staffs
		{
			get { return FindboxLookupCollections.GetStaffCollection(Factory); }
		}

		#endregion
	}
}

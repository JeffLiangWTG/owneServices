using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ResString = Enterprise.CommissionManagement.Module.ResString;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionApprovalRequestFilterBusinessObject : FilterStripBusinessObject
	{
		#region Module Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddNumberFilter(FilterDescription.BatchNumber, AccCommissionApprovalRequestSchema.CRQ_BatchNumber)
				.MultilingualDescription = ResString.GetMultilingualString("9549f077-cd08-4a59-97ee-24b717762f4e", "Batch Number");

			var staffFilter = filters.AddNkFilter(FilterDescription.ApprovingStaff, GetApprovingStaffQuery, ModuleIDs.GlbStaff, Staff);
			staffFilter.MultilingualDescription = ResString.GetMultilingualString("46dd1c29-27f0-4ad7-bda0-860b08ab729e", "Authorization Staff");
			staffFilter.Category = FilterCategories.RelationshipOrgAndStaff;

			var statusFilter = filters.AddTextFilter(FilterDescription.Status, GetStatusQuery, Statuses);
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("190afbcc-e79b-44a4-a0fa-acc24e82b415", "Status");
			RemoveAllComparisonOperatorsExceptExactAndNotEquals(statusFilter);
			statusFilter.Category = FilterCategories.StatusAndFlags;

			filters.AddNkFilter(FilterDescription.ContainsCommissionForStaff, GetCommissionForStaffQuery, ModuleIDs.GlbStaff, Staff)
				.MultilingualDescription = ResString.GetMultilingualString("dd8f6832-96bd-4dc6-a358-46e84e6c4169", "Contains Commission for Staff");

			filters.AddGuidFilter(FilterDescription.ContainsCommissionForParty, ModuleIDs.Organisation, GetCommissionForPartyQuery, Parties)
				.MultilingualDescription = ResString.GetMultilingualString("1b6f16d5-a7d0-4d3e-9538-3845d994c927", "Contains Commission for Organization");

			return filters;
		}

		void RemoveAllComparisonOperatorsExceptExactAndNotEquals(ModuleTextFilter textFilter)
		{
			foreach (ICodeDescription comparisonOperator in textFilter.ComparisonOperator_List.ToArray())
			{
				if (comparisonOperator.Code != ModuleTextFilter.ComparisonConstants.Exact && comparisonOperator.Code != ModuleTextFilter.ComparisonConstants.NotEqual)
				{
					textFilter.ComparisonOperator_List.Remove(comparisonOperator);
				}
			}
		}

		ZQuery GetApprovingStaffQuery(ZString staffNk)
		{
			var query = new ZQuery(AccCommissionApprovalRequestSchema.CRQ_GS_NKApprovingStaff1, staffNk);
			query.AddToFilter(JoinCondition.Or, AccCommissionApprovalRequestSchema.CRQ_GS_NKApprovingStaff2, staffNk);

			return query;
		}

		ZQuery GetStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(AccCommissionApprovalRequest));

			var isNegative = comparisonOperator.IsNegativeSQLOperator();
			if (!isNegative)
			{
				if (value == CommissionApprovalRequestStatusList.Codes.Canceled)
				{
					var hasItemSubQuery = new ZDBOnlySubQuery(typeof(AccCommissionApprovalRequestItem), AccCommissionApprovalRequestItemSchema.CRI_CRQ, true);
					query.AddSubQuery(hasItemSubQuery, JoinCondition.And);
				}
				else if (value == CommissionApprovalRequestStatusList.Codes.Approved)
				{
					var hasItemSubQuery = new ZDBOnlySubQuery(typeof(AccCommissionApprovalRequestItem), AccCommissionApprovalRequestItemSchema.CRI_CRQ, false);
					query.AddSubQuery(hasItemSubQuery, JoinCondition.And);

					var staff1NotPendingQuery = new ZQuery(AccCommissionApprovalRequestSchema.CRQ_GS_NKApprovingStaff1, SQLComparisonOperator.Equal, ZString.Empty);
					staff1NotPendingQuery.AddToFilter(JoinCondition.Or, AccCommissionApprovalRequestSchema.CRQ_Staff1HasApproved, true);

					var staff2NotPendingQuery = new ZQuery(AccCommissionApprovalRequestSchema.CRQ_GS_NKApprovingStaff2, SQLComparisonOperator.Equal, ZString.Empty);
					staff2NotPendingQuery.AddToFilter(JoinCondition.Or, AccCommissionApprovalRequestSchema.CRQ_Staff2HasApproved, true);

					query.AddToFilter(new ZQuery(staff1NotPendingQuery, JoinCondition.And, staff2NotPendingQuery), JoinCondition.And);
				}
				else if (value == CommissionApprovalRequestStatusList.Codes.Pending)
				{
					var hasItemSubQuery = new ZDBOnlySubQuery(typeof(AccCommissionApprovalRequestItem), AccCommissionApprovalRequestItemSchema.CRI_CRQ);
					query.AddSubQuery(hasItemSubQuery, JoinCondition.And);

					var staff1PendingQuery = new ZQuery(AccCommissionApprovalRequestSchema.CRQ_GS_NKApprovingStaff1, SQLComparisonOperator.NotEqual, ZString.Empty);
					staff1PendingQuery.AddToFilter(JoinCondition.And, AccCommissionApprovalRequestSchema.CRQ_Staff1HasApproved, false);

					var staff2PendingQuery = new ZQuery(AccCommissionApprovalRequestSchema.CRQ_GS_NKApprovingStaff2, SQLComparisonOperator.NotEqual, ZString.Empty);
					staff2PendingQuery.AddToFilter(JoinCondition.And, AccCommissionApprovalRequestSchema.CRQ_Staff2HasApproved, false);

					query.AddToFilter(new ZQuery(staff1PendingQuery, JoinCondition.Or, staff2PendingQuery), JoinCondition.And);
				}
			}
			else
			{
				if (value == CommissionApprovalRequestStatusList.Codes.Canceled)
				{
					var hasItemSubQuery = new ZDBOnlySubQuery(typeof(AccCommissionApprovalRequestItem), AccCommissionApprovalRequestItemSchema.CRI_CRQ, false);
					query.AddSubQuery(hasItemSubQuery, JoinCondition.Or);

					return query;
				}
				else if (value == CommissionApprovalRequestStatusList.Codes.Approved)
				{
					var hasItemSubQuery = new ZDBOnlySubQuery(typeof(AccCommissionApprovalRequestItem), AccCommissionApprovalRequestItemSchema.CRI_CRQ, true);
					query.AddSubQuery(hasItemSubQuery, JoinCondition.Or);

					var staff1PendingQuery = new ZQuery(AccCommissionApprovalRequestSchema.CRQ_GS_NKApprovingStaff1, SQLComparisonOperator.NotEqual, ZString.Empty);
					staff1PendingQuery.AddToFilter(JoinCondition.And, AccCommissionApprovalRequestSchema.CRQ_Staff1HasApproved, false);

					var staff2PendingQuery = new ZQuery(AccCommissionApprovalRequestSchema.CRQ_GS_NKApprovingStaff2, SQLComparisonOperator.NotEqual, ZString.Empty);
					staff2PendingQuery.AddToFilter(JoinCondition.And, AccCommissionApprovalRequestSchema.CRQ_Staff2HasApproved, false);

					query.AddToFilter(new ZQuery(staff1PendingQuery, JoinCondition.Or, staff2PendingQuery), JoinCondition.Or);
				}
				else if (value == CommissionApprovalRequestStatusList.Codes.Pending)
				{
					var hasItemSubQuery = new ZDBOnlySubQuery(typeof(AccCommissionApprovalRequestItem), AccCommissionApprovalRequestItemSchema.CRI_CRQ, true);
					query.AddSubQuery(hasItemSubQuery, JoinCondition.Or);

					var staff1NotPendingQuery = new ZQuery(AccCommissionApprovalRequestSchema.CRQ_GS_NKApprovingStaff1, ZString.Empty);
					staff1NotPendingQuery.AddToFilter(JoinCondition.Or, AccCommissionApprovalRequestSchema.CRQ_Staff1HasApproved, true);

					var staff2NotPendingQuery = new ZQuery(AccCommissionApprovalRequestSchema.CRQ_GS_NKApprovingStaff2, ZString.Empty);
					staff2NotPendingQuery.AddToFilter(JoinCondition.Or, AccCommissionApprovalRequestSchema.CRQ_Staff2HasApproved, true);

					query.AddToFilter(new ZQuery(staff1NotPendingQuery, JoinCondition.And, staff2NotPendingQuery), JoinCondition.Or);
				}
			}

			return query;
		}

		ZQuery GetCommissionForStaffQuery(ZString nK)
		{
			var linesSubquery = new ZDBOnlySubQuery(typeof(AccCommissionLine), AccCommissionApprovalRequestItemSchema.CRI_CL0);
			linesSubquery.AddToFilter(AccCommissionLineSchema.CL0_GS_NKStaff, nK);

			var requestItemSubquery = new ZDBOnlySubQuery(typeof(AccCommissionApprovalRequestItem), AccCommissionApprovalRequestItemSchema.CRI_CRQ);
			requestItemSubquery.AddSubQuery(linesSubquery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(AccCommissionApprovalRequest));
			result.AddSubQuery(requestItemSubquery, JoinCondition.And);

			return result;
		}

		ZQuery GetCommissionForPartyQuery(ZGuid value)
		{
			var linesSubquery = new ZDBOnlySubQuery(typeof(AccCommissionLine), AccCommissionApprovalRequestItemSchema.CRI_CL0);
			linesSubquery.AddToFilter(AccCommissionLineSchema.CL0_OH_Party, value);

			var requestItemSubquery = new ZDBOnlySubQuery(typeof(AccCommissionApprovalRequestItem), AccCommissionApprovalRequestItemSchema.CRI_CRQ);
			requestItemSubquery.AddSubQuery(linesSubquery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(AccCommissionApprovalRequest));
			result.AddSubQuery(requestItemSubquery, JoinCondition.And);

			return result;
		}

		#region Module Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string BatchNumber = "BatchNumber";
			public const string ApprovingStaff = "ApprovingStaff";
			public const string Status = "Status";
			public const string ContainsCommissionForStaff = "ContainsCommissionForStaff";
			public const string ContainsCommissionForParty = "ContainsCommissionForParty";

			#endregion
		}

		#endregion

		#endregion

		#region Lookups

		#region Staff

		GlbStaffCollection Staff
		{
			get { return staff ?? (staff = new GlbStaffCollection(Factory)); }
		}
		GlbStaffCollection staff;

		#endregion

		#region Parties

		OrgHeaderCollection Parties
		{
			get { return parties ?? (parties = new OrgHeaderCollection(Factory)); }
		}
		OrgHeaderCollection parties;

		#endregion

		#region Statuses

		public ReadOnlyCodeDescriptionPairList Statuses
		{
			get { return new CommissionApprovalRequestStatusList(); }
		}

		#endregion

		#endregion
	}
}

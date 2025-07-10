using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public abstract class AccQueryClaimFilterBusinessObject : AccountingFilterStripBusinessObject, IQueryClaim
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddStatusFilters(filters);
			AddRelatedItemFilters(filters);
			AddBranchManagementCodeFilter(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Query Number", AccQueryClaimSchema.AY_QueryClaimReference).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccQueryClaim|QueryNumber", "Query Number");
		}

		#region GetBranchManagementCodeQuery

		protected override ZQuery GetBranchManagementCodeQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(AccQueryClaim));

			if (!value.IsEmpty)
			{
				var branchManagementCodeQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
				branchManagementCodeQuery.AddToFilter(GlbBranchSchema.GB_AccountingGroupCode, value);
				result.AddSubQuery(AccQueryClaimSchema.AY_GB, branchManagementCodeQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion

		#endregion

		#region Status

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter statusFilter = filters.AddTextFilter("Status", GetStatusFilter, ClaimStatus_List);
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccQueryClaim|Status", "Status");

			ModuleTextFilter reasonFilter = filters.AddTextFilter("Reason", GetReasonFilter, ClaimReason_List);
			reasonFilter.Category = FilterCategories.StatusAndFlags;
			reasonFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccQueryClaim|Reason", "Reason");

			ModuleTextFilter typeFilter = filters.AddTextFilter("Type", GetTypeFilter, ClaimType_List);
			typeFilter.Category = FilterCategories.StatusAndFlags;
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccQueryClaim|Type", "Type");
		}

		ZQuery GetStatusFilter(ZString value)
		{
			ZQuery query = new ZQuery();

			if (!value.IsEmpty)
			{
				query.AddToFilter(AccQueryClaimSchema.AY_QueryClaimStatus, value);
			}

			return query;
		}

		ZQuery GetReasonFilter(ZString value)
		{
			ZQuery query = new ZQuery();

			if (!value.IsEmpty)
			{
				query.AddToFilter(AccQueryClaimSchema.AY_QueryClaimReasonCode, value);
			}

			return query;
		}

		ZQuery GetTypeFilter(ZString value)
		{
			ZQuery query = new ZQuery();

			if (!value.IsEmpty)
			{
				query.AddToFilter(AccQueryClaimSchema.AY_QueryClaimType, value);
			}

			return query;
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			ModuleGuidFilter debtorFilter = filters.AddGuidFilter(CreditorDebtorName, ModuleIDs.Organisation, AccQueryClaimSchema.AY_OH_Debtor, Organisations);
			debtorFilter.MultilingualDescription = CreditorDebtorCaption;
			debtorFilter.PropertyInfo.ValueChanged += delegate
				{
					fAY_OH_Debtor = ((ModuleGuidFilter)this[CreditorDebtorName]).Property;
				};

			filters.AddTextFilter("Invoice Number", GetInvoiceFilter)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_TransactionNum)
				.MultilingualDescription = InvoiceFilterCaption;

			ModuleGuidFilter branchFilter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, AccQueryClaimSchema.AY_GB, Branches);
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccQueryClaim|Branch", "Branch");
			branchFilter.PropertyInfo.ValueChanged += delegate
				{
					fAY_GB = ((ModuleGuidFilter)this["Branch"]).Property;
				};

			var staffFilter = filters.AddNkFilter("Staff member", AccQueryClaimSchema.AY_GS_NKStaffAssignedTo, ModuleIDs.GlbStaff, Staff);
			staffFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccQueryClaim|StaffMember", "Staff member");
		}

		public ZQuery GetInvoiceFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();

			if (!value.IsEmpty)
			{
				ZDBOnlySubQuery invoiceQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccQueryClaimSchema.AY_AH);
				invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, Ledger);
				invoiceQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, comparisonOperator, value);

				ZDBOnlyQuery claimQuery = new ZDBOnlyQuery(typeof(AccQueryClaim));
				claimQuery.AddSubQuery(invoiceQuery, JoinCondition.And);

				query.AddToFilter(claimQuery);
			}

			return query;
		}

		public abstract ZString CreditorDebtorName { get; }
		public abstract MultilingualString CreditorDebtorCaption { get; }
		public abstract MultilingualString InvoiceFilterCaption { get; }

		ZGuid fAY_GB;
		ZGuid fAY_OH_Debtor;

		#endregion

		#endregion

		#region Lookups

		#region ClaimType_List

		public ICodeDescriptionPairListWithDefaultCode ClaimType_List
		{
			get { return AccountingConfigurationRegistry.Instance.QueryClaimType.Value; }
		}

		#endregion

		#region ClaimReason_List

		public ICodeDescriptionPairListWithDefaultCode ClaimReason_List
		{
			get { return AccountingConfigurationRegistry.Instance.ClaimReason.Value; }
		}

		#endregion

		#region ClaimStatus_List

		public ICodeDescriptionPairListWithDefaultCode ClaimStatus_List
		{
			get { return AccountingConfigurationRegistry.Instance.ClaimStatus.Value; }
		}

		#endregion

		#region Branches

		protected GlbBranchCollection fBranches;
		public GlbBranchCollection Branches
		{
			get
			{
				if (fBranches == null)
				{
					fBranches = new GlbBranchCollection(Factory, new ZQuery());
				}
				return fBranches;
			}
		}

		#endregion

		#region Staff

		protected GlbStaffCollection fStaff;
		public GlbStaffCollection Staff
		{
			get
			{
				if (fStaff == null)
				{
					fStaff = new GlbStaffCollection(Factory);
				}
				return fStaff;
			}
		}

		#endregion

		#region Organisations

		public OrgHeaderCollection Organisations
		{
			get
			{
				if (fOrganisations == null)
				{
					fOrganisations = new OrgHeaderCollection(Factory);
				}
				return fOrganisations;
			}
		}
		OrgHeaderCollection fOrganisations;

		#endregion

		#endregion

		#region IQueryClaim Members

		public ZGuid AY_GB
		{
			get
			{
				return fAY_GB;
			}
		}

		public ZPropertyInfo AY_GBInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(AccQueryClaimSchema.Constants.AY_GB); }
		}

		public ZGuid AY_OH_Debtor
		{
			get
			{
				return fAY_OH_Debtor;
			}
		}

		public ZPropertyInfo AY_OH_DebtorInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(AccQueryClaimSchema.Constants.AY_OH_Debtor); }
		}

		protected abstract ZString GetLedger();
		public ZString Ledger
		{
			get
			{
				return GetLedger();
			}
		}

		#endregion
	}
}

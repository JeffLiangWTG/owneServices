using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class AccountingFilterStripBusinessObject : FilterStripBusinessObject
	{
		public AccountingFilterStripBusinessObject()
			: base()
		{
		}

		#region Constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected const string HasRelatedAPClaim = "Has AP Claim";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected const string HasNoRelatedAPClaim = "No AP Claim";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected const string HasRelatedARClaim = "Has AR Claim";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected const string HasNoRelatedARClaim = "No AR Claim";

		#endregion

		#region Properties

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			return new ModuleFilterCollection();
		}

		protected override bool IsActiveStatusFilterAlwaysApplied()
		{
			return false;
		}

		#region AddBranchManagementCodeFilter

		protected void AddBranchManagementCodeFilter(ModuleFilterCollection filters)
		{
			var branchManagementCodeFilter = filters.AddTextFilter("Branch Management Code", GetBranchManagementCodeQuery, BranchManagementCodeList);
			branchManagementCodeFilter.Category = FilterCategories.Organisations;
			branchManagementCodeFilter.MultilingualDescription = ResString.GetMultilingualString("360a1eb5-9f81-49b5-af8c-4363ef0ec5c8", "Branch Management Code");
		}

		#endregion

		#region Get[AP/AR]OrganizationAndAddressFilter

		public ZQuery GetAPOrganizationAndAddressFilter(ZGuid orgPK, ZGuid addressPK)
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			if (orgPK != ZGuid.Empty)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_OH, orgPK);
			}

			if (addressPK != ZGuid.Empty)
			{
				var org = Factory.Load<OrgHeader>(orgPK);
				if (org != null && org.AddressForSendingAPDocuments != null && addressPK == org.AddressForSendingAPDocuments.PK)
				{
					var zParams = new ZSqlParameterCollection();
					zParams.Add("@addressPK", addressPK, AccTransactionHeaderSchema.AH_OA_InvoiceAddressOverride);
					result.AddFilterAndZSQLParameterCollection(@" AH_OA_InvoiceAddressOverride = @addressPK OR AH_OA_InvoiceAddressOverride IS NULL ", zParams);
				}
				else
				{
					result.AddToFilter(AccTransactionHeaderSchema.AH_OA_InvoiceAddressOverride, addressPK);
				}
			}
			result.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			return result;
		}

		#endregion

		#region Match Status And Match Status Reason Filter

		protected void AddMatchStatusAndReasonFilter(ModuleFilterCollection filters)
		{
			ModuleTextFilter matchStatusFilter = filters.AddTextFilter("Match Status", AccTransactionHeaderSchema.AH_MatchStatus, AccountingConfigurationRegistry.Instance.MatchStatus.Value.GetCodeDescriptionPairList());
			matchStatusFilter.Category = FilterCategories.StatusAndFlags;
			matchStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|MatchStatus", "Match Status");

			ModuleTextFilter matchStatusReasonFilter = filters.AddTextFilter("Match Status Reason", AccTransactionHeaderSchema.AH_MatchStatusReasonCode, AccountingConfigurationRegistry.Instance.MatchStatusReason.Value.GetCodeDescriptionPairList());
			matchStatusReasonFilter.Category = FilterCategories.StatusAndFlags;
			matchStatusReasonFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|MatchStatusReason", "Match Status Reason");
		}

		#endregion

		#region TaxTransactionFilters

		protected void AddServiceCodeFilter(ModuleFilterCollection filters)
		{
			if (GlbCompany.CurrentCompany.IsAPWithholdTaxEnabled())
			{
				var serviceCodeFilter = filters.AddTextFilter("Service Code", GetServiceCodeQuery);
				serviceCodeFilter.SubGroup = SubGroup;
				serviceCodeFilter.Category = TaxTransactionCategory;
				serviceCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|ServiceCode", "Service Code");
			}
		}

		protected void AddNotionalWHTFilter(ModuleFilterCollection filters)
		{
			if (GlbCompany.CurrentCompany.IsAPWithholdTaxEnabled())
			{
				var notionalWHTModuleFilter = filters.AddFlagsFilter(
				"Notional WHT",
				new string[] { Res.GetString("Accounting|TransactionFilter|NotionalWHT", "Notional WHT") },
				new GetFlagsQuery[] { GetNotionalWHTFilterQuery });
				notionalWHTModuleFilter.SubGroup = SubGroup;
				notionalWHTModuleFilter.Category = TaxTransactionCategory;
				notionalWHTModuleFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|NotionalWHT", "Notional WHT");
			}
		}

		ZQuery GetServiceCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZQuery();

			result.AddToFilter(AccTaxTransactionSchema.ATT_TaxAuthorityServiceCode, comparisonOperator, value);

			return result;
		}

		ZQuery GetNotionalWHTFilterQuery(ZBool value)
		{
			var result = new ZQuery();

			if (value)
			{
				result.AddToFilter(AccTaxTransactionSchema.ATT_TaxSuperType, SQLComparisonOperator.Equal, AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.StandardPaymentRetention.Code);
				result.AddToFilter(AccTaxTransactionSchema.ATT_RealisationDate, SQLComparisonOperator.Equal, null);
				result.AddToFilter(AccTaxTransactionSchema.ATT_IsCancelled, SQLComparisonOperator.Equal, false);
			}

			return result;
		}

		public class TaxTransactionFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				var subQueryForTaxTransaction = new ZDBOnlySubQuery(typeof(AccTaxTransaction), AccTaxTransactionSchema.ATT_AH);
				subQueryForTaxTransaction.AddToFilter(filter);

				query.AddSubQuery(subQueryForTaxTransaction, JoinCondition.And);
				return query;
			}
		}

		FilterCategory fTaxTransactions;
		protected FilterCategory TaxTransactionCategory
		{
			get
			{
				if (fTaxTransactions == null)
				{
					fTaxTransactions = new FilterCategory(ResString.GetMultilingualString("FilterCategory.TaxTransactions", "Tax Transactions"));
				}
				return fTaxTransactions;
			}
		}

		TaxTransactionFilterSubGroup subGroup;
		TaxTransactionFilterSubGroup SubGroup => subGroup ?? (subGroup = new TaxTransactionFilterSubGroup());

		#endregion

		#region GetHasRelatedClaimQuery

		protected ZQuery GetHasRelatedClaimQuery(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery subQuery = null;

			if (value == HasRelatedAPClaim || value == HasRelatedARClaim)
			{
				subQuery = new ZDBOnlySubQuery(typeof(AccQueryClaim), AccQueryClaimSchema.AY_AH, false);
			}
			else if (value == HasNoRelatedAPClaim || value == HasNoRelatedARClaim)
			{
				subQuery = new ZDBOnlySubQuery(typeof(AccQueryClaim), AccQueryClaimSchema.AY_AH, true);
			}

			if (subQuery != null)
			{
				query.AddSubQuery(subQuery, JoinCondition.And);
			}

			return query;
		}

		#endregion

		#region GetBranchManagementCodeQuery

		protected virtual ZQuery GetBranchManagementCodeQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			if (!value.IsEmpty)
			{
				var branchManagementCodeQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
				branchManagementCodeQuery.AddToFilter(GlbBranchSchema.GB_AccountingGroupCode, value);
				result.AddSubQuery(AccTransactionHeaderSchema.AH_GB, branchManagementCodeQuery, JoinCondition.And);
			}

			return result;
		}

		#endregion

		#region GetInvoiceRemittanceReferenceQuery

		protected ZQuery GetInvoiceRemittanceReferenceQuery(SQLComparisonOperator @operator, ZString number)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			var headerReference = new ZDBOnlySubQuery(typeof(AccTransactionHeaderReference), AccTransactionHeaderReferenceSchema.AH1_AH);
			headerReference.AddToFilter_PossiblyCommaSeparated(AccTransactionHeaderReferenceSchema.AH1_Reference, @operator, number);
			headerReference.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRR);
			query.AddSubQuery(headerReference, JoinCondition.And);

			if (@operator == SQLComparisonOperator.IsBlank)
			{
				var headerBlankReference = new ZDBOnlySubQuery(typeof(AccTransactionHeaderReference), AccTransactionHeaderReferenceSchema.AH1_AH, true);
				headerBlankReference.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRR);
				query.AddSubQuery(headerBlankReference, JoinCondition.Or);
			}

			return query;
		}

		#endregion

		#region Lookups

		#region HasRelatedAPClaimFilterOptionsList

		protected CodeDescriptionPairList fHasRelatedClaimFilterOptionsList;

		public virtual CodeDescriptionPairList HasRelatedClaimFilterOptionsList
		{
			get
			{
				if (fHasRelatedClaimFilterOptionsList == null)
				{
					fHasRelatedClaimFilterOptionsList = new CodeDescriptionPairList();

					fHasRelatedClaimFilterOptionsList.AddPair(HasRelatedAPClaim, Res.GetString("0fbf1341-87a7-4056-9d37-8f6512ef0a00", "Only Transaction only with Related AP Claim"));
					fHasRelatedClaimFilterOptionsList.AddPair(HasNoRelatedAPClaim, Res.GetString("d01baee8-2718-438b-8e7d-99e2db17a77f", "Only Transactions with NO Related AP Claim"));
				}

				return fHasRelatedClaimFilterOptionsList;
			}
		}

		#endregion

		#region Branch Management Code

		protected CodeDescriptionPairList BranchManagementCodeList
		{
			get { return AccountingMasterFilesRegistry.Instance.BranchManagementCodes.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList(); }
		}

		#endregion

		#region TransactionTypeList

		CodeDescriptionPairList fAHTransactionTypeList;
		protected virtual CodeDescriptionPairList AHTransactionTypeList
		{
			get
			{
				if (fAHTransactionTypeList == null)
				{
					fAHTransactionTypeList = new CodeDescriptionPairList();
					fAHTransactionTypeList.AddPair("ALL", Res.GetString("Accounting|TransactionFilter|AllTransactions", "All Transactions"));
					fAHTransactionTypeList.AddPair("ADJ", Res.GetString("Accounting|TransactionFilter|AdjustmentNote", "Adjustment Note"));
					fAHTransactionTypeList.AddPair("CTR", Res.GetString("Accounting|TransactionFilter|Contra", "Contra"));
					fAHTransactionTypeList.AddPair("CRD", Res.GetString("Accounting|TransactionFilter|CreditNote", "Credit Note"));
					fAHTransactionTypeList.AddPair("DSC", Res.GetString("Accounting|TransactionFilter|Discount", "Discount"));
					fAHTransactionTypeList.AddPair("EXX", Res.GetString("Accounting|TransactionFilter|ExchangeDifference", "Exchange Difference"));
					fAHTransactionTypeList.AddPair("INV", Res.GetString("Accounting|TransactionFilter|Invoice", "Invoice"));
					fAHTransactionTypeList.AddPair("JNL", Res.GetString("Accounting|TransactionFilter|Journal", "Journal"));
					fAHTransactionTypeList.AddPair("OVP", Res.GetString("Accounting|TransactionFilter|Overpayment", "Overpayment"));
					fAHTransactionTypeList.AddPair("PAY", Res.GetString("Accounting|TransactionFilter|Payment", "Payment"));
					fAHTransactionTypeList.AddPair("REC", Res.GetString("Accounting|TransactionFilter|Receipt", "Receipt"));
					fAHTransactionTypeList.AddPair("TRF", Res.GetString("Accounting|TransactionFilter|Transfer", "Transfer"));
				}
				return fAHTransactionTypeList;
			}
		}

		#endregion

		#endregion
	}
}

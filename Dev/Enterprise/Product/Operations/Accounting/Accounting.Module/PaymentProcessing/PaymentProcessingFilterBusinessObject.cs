using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public abstract partial class PaymentProcessingFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		protected abstract ZString LedgerCode { get; }

		protected abstract string OrganisationFilterName { get; }
		protected abstract MultilingualString OrganisationFilterCaption { get; }

		protected abstract string OrganisationAndAddressFilterName { get; }
		protected abstract MultilingualString OrganisationAndAddressFilterCaption { get; }
		protected abstract bool IsDebtor { get; }

		FilterCategory fFinancialDetailsCategory;
		protected FilterCategory FinancialDetailsCategory
		{
			get
			{
				if (fFinancialDetailsCategory == null)
				{
					fFinancialDetailsCategory = new FilterCategory(ResString.GetMultilingualString("Accounting|PaymentProcessingFilter|FinancialDetails", "Financial Details"));
				}
				return fFinancialDetailsCategory;
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			ModuleFilter filter = filters.AddNumberFilter("Check or Reference", AccPaymentApprovalSchema.AV_ChequeOrReference);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentProcessingFilter|ChequeOrReference", "Check or Reference");

			filter = filters.AddGuidFilter("Bank", ModuleIDs.AccBankAccount, AccPaymentApprovalSchema.AV_AB, AV_ABList);
			filter.Category = FinancialDetailsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentProcessingFilter|Bank", "Bank");

			filter = filters.AddGuidFilter("Check", ModuleIDs.AccChequeBook, AccPaymentApprovalSchema.AV_AK, AV_AKList);
			filter.Category = FinancialDetailsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentProcessingFilter|Cheque", "Check");

			filter = filters.AddNkFilter("Currency", AccPaymentApprovalSchema.AV_RX_NKPaymentCurrency, ModuleIDs.RefCurrency, AV_RXList);
			filter.Category = FinancialDetailsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentProcessingFilter|Currency", "Currency");

			filter = filters.AddGuidFilter(OrganisationFilterName, ModuleIDs.Organisation, AccPaymentApprovalSchema.AV_OH, AV_OHList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = OrganisationFilterCaption;

			AddOrgAndAddressFilter(filters);

			filter = filters.AddTextFilter("Status", GetStatusQuery, AV_StatusList);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentProcessingFilter|Status", "Status");

			filter = filters.AddNumberRangeFilter("Amount", AccPaymentApprovalSchema.AV_Amount);
			filter.Category = FinancialDetailsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentProcessingFilter|Amount", "Amount");

			filter = filters.AddNkFilter("First Approval", AccPaymentApprovalSchema.AV_GS_NKApproval1st, ModuleIDs.GlbStaff, AV_GS_FirstApprovalList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentProcessingFilter|FirstApproval", "First Approval");

			filter = filters.AddNkFilter("Second Approval", AccPaymentApprovalSchema.AV_GS_NKApproval2nd, ModuleIDs.GlbStaff, AV_GS_SecondApprovalList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentProcessingFilter|SecondApproval", "Second Approval");

			filter = filters.AddNkFilter("Third Approval", AccPaymentApprovalSchema.AV_GS_NKApproval3rd, ModuleIDs.GlbStaff, AV_GS_ThirdApprovalList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentProcessingFilter|ThirdApproval", "Third Approval");

			filter = filters.AddGuidFilter("Check Book Branch", ModuleIDs.GlbBranch, GetCheckBookBranchQuery, AV_AK_GBList);
			filter.Category = FinancialDetailsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentProcessingFilter|CheckBookBranch", "Check Book Branch");

			filter = filters.AddDateFilter("Payment Date", AccPaymentApprovalSchema.AV_PaymentDate);
			filter.Category = FinancialDetailsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentProcessingFilter|PaymentDate", "Payment Date");

			ModuleFilter reasonCodefilter = filters.AddTextFilter("Rejection Reason", GetRejectionReasonCodeQuery, RejectionReasonCodeList);
			reasonCodefilter.Category = FilterCategories.StatusAndFlags;
			reasonCodefilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentProcessingFilter|RejectionReason", "Rejection Reason");

			var approvalReferenceFilter = filters.AddNumberFilter("Payment Approval Reference", GetPaymentApprovalReference);
			approvalReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("6a6096f3-4f23-42a7-a870-948a002ce5e6", "Payment Approval Reference");

			if (AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.Value.IsEPaymentEnabledForAnyProvider)
			{
				filter = filters.AddTextFilter("E-Payment Status", GetEPaymentStatusQuery, EPaymentStatusCodeList);
				filter.Category = FilterCategories.StatusAndFlags;
				filter.MaxLength = AccEPaymentDealSchema.AED_Status.MaxLength;
				filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PaymentProcessingFilter|E-Payment Status", "E-Payment Status");

				filter = filters.AddDateFilter("E-Payment Submitted Date", GetEPaymentSubmittedDate, false);
				filter.MultilingualDescription = ResString.GetMultilingualString("f4efba22-4b21-4246-82e9-4b411d833690", "E-Payment Submitted Date");

				filter = filters.AddTextFilter("E-Payment Provider Reference", GetEPaymentProviderReferenceQuery);
				filter.MaxLength = AccEPaymentDealSchema.AED_ProviderReference.MaxLength;
				filter.MultilingualDescription = ResString.GetMultilingualString("4e4bd92d-1dd2-428f-8933-9e196914685d", "E-Payment Provider Reference");

				filter = filters.AddDateFilter("E-Payment Last Response Received Date", GetEPaymentLastResponseReceivedDateQuery, false);
				filter.MultilingualDescription = ResString.GetMultilingualString("597df72d-4b8d-4cf5-820a-6668bf5ce5c4", "E-Payment Last Response Received Date");
			}
			return filters;
		}

		ZQuery GetPaymentApprovalReference(SQLComparisonOperator comparisonOperator, ZString value) => new ZQuery(AccPaymentApprovalSchema.AV_PaymentApprovalReference, comparisonOperator, value);

		ZQuery GetStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();

			query.AddToFilter(AccPaymentApprovalSchema.AV_Status, SQLComparisonOperator.Equal, value);

			return query;
		}

		ZQuery GetCheckBookBranchQuery(ZGuid value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccPaymentApproval));
			ZDBOnlySubQuery checkBookBranchQuery = new ZDBOnlySubQuery(typeof(AccChequeBook), AccPaymentApprovalSchema.AV_AK);
			checkBookBranchQuery.AddToFilter(AccChequeBookSchema.AK_GB, value);
			query.AddSubQuery(checkBookBranchQuery, JoinCondition.And);

			return query;
		}

		#region EPayment

		ReadOnlyCodeDescriptionPairList EPaymentStatusCodeList => PaymentApprovalLookups.EPaymentStatusList;

		ZDBOnlyQuery GetActiveDealQuery(ZQuery dealQuery)
		{
			var query = new ZDBOnlyQuery(typeof(AccPaymentApproval));
			var para = dealQuery.LiteralTextSqlFormatted;
			if (!string.IsNullOrEmpty(para))
			{
				para = $" AND ({para})";
			}
			var sql = $@"{AccPaymentApprovalSchema.Constants.PK} IN (SELECT {AccEPaymentQuoteSchema.Constants.QU_AV} FROM
(SELECT {AccEPaymentQuoteSchema.Constants.PK}, {AccEPaymentQuoteSchema.Constants.QU_AV}, {AccEPaymentDealSchema.Constants.AED_SystemCreateTimeUtc}, {AccEPaymentDealSchema.Constants.AED_QU_Quote},
{AccEPaymentDealSchema.Constants.AED_Status}, {AccEPaymentDealSchema.Constants.AED_LastResponseReceivedUtc}, {AccEPaymentDealSchema.Constants.AED_ProviderReference},
ROW_NUMBER()
OVER(PARTITION BY {AccEPaymentQuoteSchema.Constants.QU_AV} ORDER BY {AccEPaymentDealSchema.Constants.AED_SystemCreateTimeUtc} DESC) AS RowRank
FROM {AccEPaymentQuoteSchema.Constants.SqlSchemaName}.{AccEPaymentQuoteSchema.Constants.TableName} 
JOIN {AccEPaymentDealSchema.Constants.SqlSchemaName}.{AccEPaymentDealSchema.Constants.TableName} 
ON {AccEPaymentQuoteSchema.Constants.PK} = {AccEPaymentDealSchema.Constants.AED_QU_Quote}) AS QuoteDeal
WHERE QuoteDeal.RowRank = 1 {para})";

			query.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection());
			return query;
		}

		ZQuery GetEPaymentStatusQuery(ZString value)
		{
			var dealQuery = new ZQuery();
			dealQuery.AddToFilter(AccEPaymentDealSchema.AED_Status, value);
			var query = GetActiveDealQuery(dealQuery);
			return query;
		}

		ZQuery GetEPaymentProviderReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var dealQuery = new ZQuery();
			dealQuery.AddToFilter(AccEPaymentDealSchema.AED_ProviderReference, comparisonOperator, value);
			var query = GetActiveDealQuery(dealQuery);
			return query;
		}

		ZQuery GetEPaymentSubmittedDate(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var dealQuery = new ZQuery();
			AddDateTimeRange(dealQuery, comparisonOperator, JoinCondition.And, AccEPaymentDealSchema.AED_SystemCreateTimeUtc, value1, value2, true);
			var query = GetActiveDealQuery(dealQuery);
			return query;
		}

		ZQuery GetEPaymentLastResponseReceivedDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var dealQuery = new ZQuery();
			AddDateTimeRange(dealQuery, comparisonOperator, JoinCondition.And, AccEPaymentDealSchema.AED_LastResponseReceivedUtc, value1, value2, true);
			var query = GetActiveDealQuery(dealQuery);
			return query;
		}

		#endregion

		#region Rejection Reason

		ReadOnlyCodeDescriptionPairList RejectionReasonCodeList => AccountingMasterFilesRegistry.Instance.PaymentRejectionReasonCodesList.Value;

		ZQuery GetRejectionReasonCodeQuery(ZString value) => !value.IsEmpty
				? new ZQuery(AccPaymentApprovalSchema.AV_RejectionReasonCode, SQLComparisonOperator.Equal, value)
				: null;

		#endregion

		#region AV_StatusList

		public CodeDescriptionPairList AV_StatusList => fAV_StatusList ?? (fAV_StatusList = AccPaymentApprovalLookups.GetStatusList());
		protected CodeDescriptionPairList fAV_StatusList;

		#endregion

		#region AV_GS_FirstApprovalList

		public GlbStaffCollection AV_GS_FirstApprovalList
		{
			get
			{
				if (fAV_GS_FirstApprovalList == null)
				{
					fAV_GS_FirstApprovalList = new GlbStaffCollection(Factory);
				}

				return fAV_GS_FirstApprovalList;
			}
		}

		protected GlbStaffCollection fAV_GS_FirstApprovalList;

		#endregion

		#region AV_GS_SecondApprovalList

		public GlbStaffCollection AV_GS_SecondApprovalList
		{
			get
			{
				if (fAV_GS_SecondApprovalList == null)
				{
					fAV_GS_SecondApprovalList = new GlbStaffCollection(Factory);
				}

				return fAV_GS_SecondApprovalList;
			}
		}

		protected GlbStaffCollection fAV_GS_SecondApprovalList;

		#endregion

		#region AV_GS_ThirdApprovalList

		public GlbStaffCollection AV_GS_ThirdApprovalList
		{
			get
			{
				if (fAV_GS_ThirdApprovalList == null)
				{
					fAV_GS_ThirdApprovalList = new GlbStaffCollection(Factory);
				}

				return fAV_GS_ThirdApprovalList;
			}
		}

		protected GlbStaffCollection fAV_GS_ThirdApprovalList;

		#endregion

		#region AV_ABList

		public AccBankAccountCollection AV_ABList
		{
			get
			{
				if (fAV_ABList == null)
				{
					fAV_ABList = new AccBankAccountCollection(Factory);
				}

				return fAV_ABList;
			}
		}

		protected AccBankAccountCollection fAV_ABList;

		#endregion

		#region AV_AKList

		public AccChequeBookCollection AV_AKList
		{
			get
			{
				if (fAV_AKList == null)
				{
					fAV_AKList = new AccChequeBookCollection(Factory);
				}

				return fAV_AKList;
			}
		}

		protected AccChequeBookCollection fAV_AKList;

		#endregion

		#region AV_AK_GBList

		GlbBranchCollection fAV_AK_GBList;
		public GlbBranchCollection AV_AK_GBList
		{
			get { return fAV_AK_GBList ?? (fAV_AK_GBList = new GlbBranchCollection(Factory)); }
		}

		#endregion

		#region AV_OHList

		public OrgHeaderCollection AV_OHList
		{
			get
			{
				if (fAV_OHList == null)
				{
					fAV_OHList = new OrgHeaderCollection(Factory);
				}

				return fAV_OHList;
			}
		}

		protected OrgHeaderCollection fAV_OHList;

		#endregion

		#region AV_RXList

		public RefCurrencyCollection AV_RXList
		{
			get
			{
				if (fAV_RXList == null)
				{
					fAV_RXList = new RefCurrencyCollection(Factory);
				}

				return fAV_RXList;
			}
		}

		protected RefCurrencyCollection fAV_RXList;

		#endregion

		public override ZQuery Filter
		{
			get
			{
				List<ZGuid> branches = new List<ZGuid>();

				foreach (GlbBranch branch in GlbCompany.CurrentCompany.Branches)
				{
					branches.Add(branch.PK);
				}

				ZQuery result = base.Filter;

				result.AddToFilter(AccPaymentApprovalSchema.AV_GB, branches);
				result.AddToFilter(AccPaymentApprovalSchema.AV_Ledger, LedgerCode);
				return result;
			}
		}

		void AddOrgAndAddressFilter(ModuleFilterCollection filters)
		{
			OrgWithAddressFilter orgWithAddressFilter = new OrgWithAddressFilter(OrganisationAndAddressFilterName, GetOrgAndAddressFilter, IsDebtor);
			orgWithAddressFilter.MultilingualDescription = OrganisationAndAddressFilterCaption;
			orgWithAddressFilter.Category = FilterCategories.Organisations;
			filters.AddCustomFilter(orgWithAddressFilter);
		}

		ZQuery GetOrgAndAddressFilter(ZGuid orgPK, ZGuid addressPK)
		{
			var result = new ZDBOnlyQuery(typeof(AccPaymentApproval));

			if (orgPK != ZGuid.Empty)
			{
				result.AddToFilter(AccPaymentApprovalSchema.AV_OH, orgPK);
			}

			if (addressPK != ZGuid.Empty)
			{
				var org = Factory.Load<OrgHeader>(orgPK);

				ZGuid defaultAddress = ZGuid.Empty;

				if (org != null)
				{
					if (IsDebtor && org.AddressForSendingARDocuments != null)
					{
						defaultAddress = org.AddressForSendingARDocuments.PK;
					}
					else if (!IsDebtor && org.AddressForSendingAPDocuments != null)
					{
						defaultAddress = org.AddressForSendingAPDocuments.PK;
					}
				}

				if (defaultAddress == addressPK)
				{
					var zParams = new ZSqlParameterCollection();
					zParams.Add("@addressPK", addressPK, AccPaymentApprovalSchema.AV_OA_AddressOverride);
					result.AddFilterAndZSQLParameterCollection(@" AV_OA_AddressOverride = @addressPK OR AV_OA_AddressOverride IS NULL ", zParams);
				}
				else
				{
					result.AddToFilter(AccPaymentApprovalSchema.AV_OA_AddressOverride, addressPK);
				}
			}

			return result;
		}
	}
}

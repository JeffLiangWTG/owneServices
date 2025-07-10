using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Module
{
	public partial class ARTransactionFilterStripBusinessObject : TransactionFilterStripBusinessObject
	{
		public override ZString CreditorDebtorText => AccountingUtils.OrganisationFilterTypes.Debtor;

		public override MultilingualString CreditorDebtorCaption
		{
			get { return ResString.GetMultilingualString("Accounting|ARTransactionFilter|Debtor", "Debtor"); }
		}

		protected override MultilingualString CreditorDebtorGroupCaption
		{
			get { return ResString.GetMultilingualString("Accounting|APTransactionFilter|DebtorGroup", "Debtor Group"); }
		}

		public override ZBool IsPayableModule
		{
			get { return ZBool.False; }
		}

		protected override ModuleIdentifier CreditorDebtorGroupModuleID
		{
			get { return ModuleIDs.OrgDebtorGroup; }
		}

		protected override ZString[] LedgersToUse
		{
			get { return new ZString[] { ZArchitecture.Core.LedgerTypes.AccountsReceivable }; }
		}

		public override bool ShouldEnableDisbursementRelatingToFilter
		{
			get { return !IsPayableModule && TransactionFilterStripControlPresentationProvider.IsRelatedDisbursementTransactionFilterAvailable(); }
		}

		protected override void AddNumberFilters(ModuleFilterCollection filters)
		{
			base.AddNumberFilters(filters);
			AddConsolidationNumberFilter(filters);
			AddCommonNumberReferenceFilter(filters);
			AddCollectionReferenceNumberFilter(filters);
			AddRelatedDisbursementTransactionsFilter(filters);

			if (GlbCompany.CurrentCompany.Country.Code == Enterprise.Core.Constants.CountryCodes.VietNam)
			{
				AddSupportingDocumentNumberFilter(filters);
			}
		}

		protected override void AddEInvoicingGovernmentAllocatedNumberFilter(ModuleFilterCollection filters)
		{
			if (ShouldShowAmendStatusCode)
			{
				var filter = filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.EInvoicingGovernmentAllocatedNumber, AddReceiptIDNumberQuery);
				filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
				filter.WithMaxLengthOf(AccEInvoicingBatchSchema.AIB_GovernmentAllocatedNumber)
					.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|EReportingGovt", "E-Reporting Govt #");
			}
			else
			{
				base.AddEInvoicingGovernmentAllocatedNumberFilter(filters);
			}
		}

		protected override void AddStatusesFilters(ModuleFilterCollection filters)
		{
			base.AddStatusesFilters(filters);

			if (ShouldShowAmendStatusCode)
			{
				AddAmendStatusCodeFilter(filters);
			}

			if (AccountingMasterFilesUtils.IsEnableChinaEInvoicing)
			{
				AddComplianceDocumentStatusFilter(filters);
			}
		}

		protected override void AddInvoiceAddressFilter(ModuleFilterCollection filters)
		{
			OrgWithAddressFilter orgWithAddressFilter = new OrgWithAddressFilter("Debtor and Address", GetAROrganizationAndAddressFilter, true);
			orgWithAddressFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ARTransactionFilter|DebtorAndAddress", "Debtor and Address");
			filters.AddCustomFilter(orgWithAddressFilter);
		}

		#region AddDateFilters

		protected override void AddDateFilters(ModuleFilterCollection filters)
		{
			base.AddDateFilters(filters);

			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.China)
			{
				filters.AddDateFilter(AccountingUtils.DateFilterTypes.ComplianceDocDate, GetAH_ComplianceDocumentDate).MultilingualDescription = ResString.GetMultilingualString("Accounting|ARTransactionFilter|ComplianceDocDate", "Compliance Doc Date");
			}
		}

		ZQuery GetAH_ComplianceDocumentDate(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetZQueryFor2DateTime(comparisonOperator, AccTransactionHeaderSchema.AH_ComplianceDocumentDate, date1, date2);
		}

		#endregion

		protected override void AddOtherFilters(ModuleFilterCollection filters)
		{
			base.AddOtherFilters(filters);
			AddInvoicePaymentReferenceCodeFilter(filters);
		}

		ZQuery GetAROrganizationAndAddressFilter(ZGuid orgPK, ZGuid addressPK)
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			if (orgPK != ZGuid.Empty)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_OH, orgPK);
			}

			// This address relates to DisplayInvoiceAddressOverride property on the Transaction Header. We are searching for the address
			// as "displayed" and as "sent to" by the document delivery. This is derived address using fallbacks. At the time of writing:
			// First we look at the address override. If not set we look at the job header to see if the invoice debtor matches the local client
			// or overseas agent, and if so use the address on the job. If not then we use the organisations default address for AR.

			if (addressPK != ZGuid.Empty)
			{
				var org = Factory.Load<OrgHeader>(orgPK);
				string sql;

				if (org != null && org.AddressForSendingARDocuments != null && addressPK == org.AddressForSendingARDocuments.PK)
				{
					// Scenario  1: Finding transactions that will default to this address, which is the default for sending AR docs.
					//
					// Three ways to get final address which we need to cater for:
					// Overriden address, AddressForSendingARDocuments and Job (i.e. local client/overseas agent).
					// The overriden address check is the "AH_OA_InvoiceAddressOverride = @addressPK"
					// The AddressForSendingARDocuments check is the "AH_OA_InvoiceAddressOverride IS NULL"
					// However if the address is null we must exclude transactions where the debtor matches the local client or overseas agent (subquery)
					// Unless the address selected for the agent/client for the job happens to be the one we are looking for (hence the <> checks in the subquery)
					// So now we have done the Job check too.

					sql = @" AH_OA_InvoiceAddressOverride = @addressPK OR
							AH_OA_InvoiceAddressOverride IS NULL AND AH_PK NOT IN 
							(
								select AH_PK
								from dbo.AccTransactionHeader 
								inner join dbo.JobHeader on AH_JH = JH_PK
								left join dbo.OrgAddress As LocalChargeOrgAddress on JH_OA_LocalChargesAddr = LocalChargeOrgAddress.OA_PK
								where JH_OA_LocalChargesAddr is not null AND JH_OA_LocalChargesAddr <> @addressPK AND AH_OH = LocalChargeOrgAddress.OA_OH
								union all 
								select AH_PK
								from dbo.AccTransactionHeader 
								inner join dbo.JobHeader on AH_JH = JH_PK
								left join dbo.OrgAddress As AgentCollectOrgAddress on JH_OA_AgentCollectAddr = AgentCollectOrgAddress.OA_PK
								where JH_OA_AgentCollectAddr is not null AND JH_OA_AgentCollectAddr <> @addressPK AND AH_OH = AgentCollectOrgAddress.OA_OH				
							)";
				}
				else
				{
					// Scenario  2: Finding transactions that will default to this address, which is NOT the default for sending AR docs.
					//
					// Three ways to get final address which we need to cater for:
					// Overriden address, AddressForSendingARDocuments and Job (i.e. local client/overseas agent).
					// The overriden address check is the "AH_OA_InvoiceAddressOverride = @addressPK"
					// The Job check is the combination of "AH_OA_InvoiceAddressOverride IS NULL" and the subquery
					// Where there is no matching client/agent on the job, it would default to AddressForSendingARDocuments so these
					// are ignored.

					sql = @" AH_OA_InvoiceAddressOverride = @addressPK OR
							AH_OA_InvoiceAddressOverride IS NULL AND AH_PK IN 
							(
								select AH_PK
								from dbo.AccTransactionHeader 
								inner join dbo.JobHeader on AH_JH = JH_PK
								left join dbo.OrgAddress As LocalChargeOrgAddress on JH_OA_LocalChargesAddr = LocalChargeOrgAddress.OA_PK
								where JH_OA_LocalChargesAddr = @addressPK AND AH_OH = LocalChargeOrgAddress.OA_OH
								union all
								select AH_PK
								from dbo.AccTransactionHeader 
								inner join dbo.JobHeader on AH_JH = JH_PK
								left join dbo.OrgAddress As AgentCollectOrgAddress on JH_OA_AgentCollectAddr = AgentCollectOrgAddress.OA_PK
								where JH_OA_AgentCollectAddr = @addressPK AND AH_OH = AgentCollectOrgAddress.OA_OH
							)";
				}
				var zParams = new ZSqlParameterCollection();
				zParams.Add("@addressPK", addressPK, AccTransactionHeaderSchema.AH_OA_InvoiceAddressOverride);
				result.AddFilterAndZSQLParameterCollection(sql, zParams);
			}
			return result;
		}

		protected override SecurityCheckpoint ViewingNonLoginBranchTransactions
		{
			get { return Env.Security.ReceivablesViewingNonLoginBranchTransactions; }
		}

		#region Module Filters related

		void AddConsolidationNumberFilter(ModuleFilterCollection filters)
		{
			ModuleNumberFilter consolidationNumberFilter = filters.AddNumberFilter(Business.AccountingUtils.NumberFilterTypes.ConsolidationNumber, GetJobInvoiceNumberQuery);
			consolidationNumberFilter.MaxLength = AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef.MaxLength;
			consolidationNumberFilter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.StartsWith;
			consolidationNumberFilter.RemoveComparisonOperatorsLeavingOne(ModuleNumberFilter.ComparisonConstants.StartsWith);
			consolidationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ARTransactionFilter|ConsolidationNumber", "Job Invoice #");
		}

		void AddCommonNumberReferenceFilter(ModuleFilterCollection filters)
		{
			var commonNumbersFilter = filters.AddNumberFilter("Common Numbers and References", GetCommonNumberReferenceQuery);
			commonNumbersFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ARTransactionFilter|CommonNumbersAndReferences", "Common Numbers and References");
			commonNumbersFilter.MaxLength = AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef.MaxLength;
		}

		void AddSupportingDocumentNumberFilter(ModuleFilterCollection filters)
		{
			var supportingDocumentNumberFilter = filters.AddTextFilter(AccountingUtils.NumberFilterTypes.SupportingDocumentNumber, GetSupportingDocumentNumberQuery);
			supportingDocumentNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ARTransactionFilter|SupportingDocumentNumber", "Supporting Document Number");
			supportingDocumentNumberFilter.MaxLength = AccTransactionHeaderReferenceSchema.AH1_Reference.MaxLength;
			supportingDocumentNumberFilter.Category = FilterCategories.NumbersAndReferences;
		}

		void AddCollectionReferenceNumberFilter(ModuleFilterCollection filters)
		{
			var referenceNumberFilter = filters.AddTextFilter(AccountingUtils.NumberFilterTypes.InvoiceTransactionReference, GetInvoiceTransactionReferenceQuery);
			referenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ARTransactionFilter|InvoiceTransactionReference", "Invoice Transaction Reference");
			referenceNumberFilter.MaxLength = AccTransactionHeaderReferenceSchema.AH1_Reference.MaxLength;
			referenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
		}

		void AddRelatedDisbursementTransactionsFilter(ModuleFilterCollection filters)
		{
			if (ShouldEnableDisbursementRelatingToFilter)
			{
				var relatedDisbursementTransactionsFilter = filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.RelatedDisbursementTransactions, GetRelatedDisbursementTransactionsQuery);
				relatedDisbursementTransactionsFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|RelatedDisbursementTransactions", "Disbursement Relating to");
				relatedDisbursementTransactionsFilter.Category = FilterCategories.NumbersAndReferences;
				relatedDisbursementTransactionsFilter.MaxLength = AccTransactionHeaderReferenceSchema.AH1_Reference.MaxLength;
			}
		}

		void AddInvoicePaymentReferenceCodeFilter(ModuleFilterCollection filters)
		{
			var referenceCodeFilter = filters.AddTextFilter(AccountingUtils.NumberFilterTypes.InvoiceRemittanceType, AccTransactionHeaderSchema.AH_InvoicePaymentReferenceCode);
			referenceCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ARTransactionFilter|InvoiceRemittanceType", "Invoice Remittance Type");
			referenceCodeFilter.MaxLength = AccTransactionHeaderSchema.AH_InvoicePaymentReferenceCode.MaxLength;
			referenceCodeFilter.Category = FilterCategories.Other;
		}

		void AddAmendStatusCodeFilter(ModuleFilterCollection filters)
		{
			var amendStatusCodeFilter = filters.AddTextFilter("Amend Status Code", GetAmendStatusCode, AmendStatusCodeProvider.AmendStatusCodeList);
			amendStatusCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ARTransactionFilter|AmendStatusCode", "Amend Status Code");
			amendStatusCodeFilter.Category = FilterCategories.StatusAndFlags;
		}

		void AddComplianceDocumentStatusFilter(ModuleFilterCollection filters)
		{
			var complianceDocumentStatusProvider = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceDocumentStatusProvider(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)?.GetComplianceDocumentStatusTypes() as CodeDescriptionPairList;
			if (complianceDocumentStatusProvider != null)
			{
				var complianceDocumentStatusFilter = filters.AddTextFilter("Compliance Document Status", GeComplianceDocumentStatus, complianceDocumentStatusProvider);
				complianceDocumentStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ARTransactionFilter|ComplianceDocumentStatus", "Compliance Document Status");
				complianceDocumentStatusFilter.Category = FilterCategories.StatusAndFlags;
			}
		}

		ZQuery AddReceiptIDNumberQuery(SQLComparisonOperator comparisonOperator, ZString receiptID)
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			var subQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeaderReference), AccTransactionHeaderReferenceSchema.AH1_AH);
			subQuery.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AccTransactionHeaderReferenceTypes.RED);
			subQuery.AddToFilter_PossiblyCommaSeparated(AccTransactionHeaderReferenceSchema.AH1_Reference, comparisonOperator, receiptID);

			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetSupportingDocumentNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			var headerReference = new ZDBOnlySubQuery(typeof(AccTransactionHeaderReference), AccTransactionHeaderReferenceSchema.AH1_AH);
			headerReference.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Reference, comparisonOperator, value);
			headerReference.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRD);

			query.AddSubQuery(headerReference, JoinCondition.And);
			return query;
		}

		ZQuery GetInvoiceTransactionReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery headerReference = new ZDBOnlySubQuery(typeof(AccTransactionHeaderReference), AccTransactionHeaderReferenceSchema.AH1_AH);
			headerReference.AddToFilter_PossiblyCommaSeparated(AccTransactionHeaderReferenceSchema.AH1_Reference, comparisonOperator, value);
			headerReference.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ITR);

			query.AddSubQuery(headerReference, JoinCondition.And);
			return query;
		}

		ZQuery GetAmendStatusCode(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			var headerReference = new ZDBOnlySubQuery(typeof(AccTransactionHeaderReference), AccTransactionHeaderReferenceSchema.AH1_AH);
			headerReference.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Reference, value);
			headerReference.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AmendStatusCodeProvider?.AmendStatusCodeReferenceType ?? ZString.Empty);

			query.AddSubQuery(headerReference, JoinCondition.And);
			return query;
		}

		ZQuery GeComplianceDocumentStatus(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			var headerReference = new ZDBOnlySubQuery(typeof(AccTransactionHeaderReference), AccTransactionHeaderReferenceSchema.AH1_AH);
			headerReference.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Reference, value);
			headerReference.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.CDS);

			query.AddSubQuery(headerReference, JoinCondition.And);
			return query;
		}

		#region GetRelatedDisbursementTransactionsQuery

		ZQuery GetRelatedDisbursementTransactionsQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery headerReference = new ZDBOnlySubQuery(typeof(AccTransactionHeaderReference), AccTransactionHeaderReferenceSchema.AH1_AH);
			headerReference.AddToFilter_PossiblyCommaSeparated(AccTransactionHeaderReferenceSchema.AH1_Reference, comparisonOperator, value);
			headerReference.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AccTransactionHeaderReferenceTypes.ReceivableDisbursementInvoice);

			query.AddSubQuery(headerReference, JoinCondition.And);
			return query;
		}

		#endregion

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			ZInt fountainPaddingLength = 0;
			var currentSequence = AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation;
			if (currentSequence.Inner.GetCurrentValueToUse(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty) != ValueToUse.DefaultValue)
			{
				fountainPaddingLength = currentSequence.Value.Select(a => ((TransactionNumberSequenceCustomisation)a))
					.Where(a => a.ElementName == TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber && a.Include).Select(a => a.Length).FirstOrDefault();
			}
			if (fountainPaddingLength == 0)
			{
				fountainPaddingLength = AccountingConfigurationRegistry.Instance.ARInvoiceNumberLengthConfiguration.Value;
			}

			ModuleFountainFilter transactionNumberFilter = new ModuleFountainFilter(Business.AccountingUtils.NumberFilterTypes.TransactionNumber, AccTransactionHeaderSchema.AH_TransactionNum, ZString.Empty, fountainPaddingLength);
			transactionNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ARTransactionFilter|TransactionNumber", "Transaction #");
			return transactionNumberFilter;
		}

		IAmendStatusCodeProvider AmendStatusCodeProvider
		{
			get
			{
				if (amendStatusCodeProvider == null)
				{
					var amendStatusCodeInstanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.Country.Code) as IInstanceProvider<IAmendStatusCodeProvider>;
					amendStatusCodeProvider = amendStatusCodeInstanceProvider?.Get();
				}
				return amendStatusCodeProvider;
			}
		}
		IAmendStatusCodeProvider amendStatusCodeProvider;

		bool ShouldShowAmendStatusCode => AmendStatusCodeProvider?.ShouldShowAmendStatusCode() ?? false;

		#endregion
	}
}

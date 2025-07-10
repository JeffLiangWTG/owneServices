using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Filters;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Matching
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filters")]
	public abstract partial class MatchingFilterBusinessObject : FilterStripBusinessObject
	{
		public const string AllNumbers = "All Numbers";
		public const string TransactionNumber = "Transaction #";
		public const string InvoiceTransactionReference = "Invoice Transaction Reference";
		public const string InvoiceRemittanceReference = "Invoice Remittance Reference";
		public const string JobInvoiceNumber = "Job Invoice #";
		public const string HouseBillNumber = "House Bill #";
		public const string ChequeReferenceNumber = "Cheque/Reference #";
		public const string MasterBillNumber = "Master Bill #";
		public const string InvoiceBatchNumber = "Invoice Batch #";
		public const string OutstandingAmount = "Outstanding Amount";
		public const string DueDate = "Due Date";
		public const string TransactionDate = "Transaction Date";
		public const string Branch = "Branch";
		public const string Department = "Department";
		public const string Currency = "Currency";
		public const string FlightVoyageNumberAndVessel = "Flight/Voyage # and Vessel";
		public const string Description = "Description";
		public const string LedgerTransactionType = "Ledger/Transaction Type";
		public const string DebtorAndAddress = "Debtor and Address";
		public const string CreditorAndAddress = "Creditor and Address";
		public const string DisbursementInvoice = "Disbursement Invoice";
		public const string DisbursementRelatingTo = "Disbursement Relating To";
		public const string ComplianceNumber = "Compliance #";

		protected MatchingFilterBusinessObject() : base()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = LayoutContextName;
		}

		protected MatchingFilterBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = LayoutContextName;
		}

		protected abstract string LayoutContextName { get; }

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddNumberFilters(filters);
			AddDateFilters(filters);
			AddGuidFilters(filters);
			AddNkFilters(filters);
			AddTextNkFilters(filters);
			AddTextFilters(filters);
			AddMiscFilters(filters);
			AddFlagFilters(filters);

			return filters;
		}

		#region Filters

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(AllNumbers, GetAllNumbersQuery)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|AllNumbers", AllNumbers);

			filters.AddNumberFilter(TransactionNumber, GetTransactionNumberQuery)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_TransactionNum)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|TransactionNumber", TransactionNumber);

			filters.AddNumberFilter(JobInvoiceNumber, GetJobInvoiceNumberQuery)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|JobInvoiceNumber", JobInvoiceNumber);

			filters.AddNumberFilter(HouseBillNumber, GetHouseBillNumberQuery)
				.WithMaxLengthOf(JobShipmentSchema.JS_HouseBill.MaxLength > JobDeclarationSchema.JE_HouseBill.MaxLength ? JobShipmentSchema.JS_HouseBill : JobDeclarationSchema.JE_HouseBill)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|HouseBillNumber", HouseBillNumber);

			filters.AddNumberFilter(ChequeReferenceNumber, GetChequeReferenceNumberQuery)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_ChequeOrReference)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|ChequeReferenceNumber", ChequeReferenceNumber);

			filters.AddNumberFilter(InvoiceRemittanceReference, GetInvoiceRemittanceReferenceQuery)
				.WithMaxLengthOf(AccTransactionHeaderReferenceSchema.AH1_Reference)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|InvoiceRemittanceReference", InvoiceRemittanceReference);

			filters.AddNumberFilter(MasterBillNumber, GetMasterBillNumberQuery)
				.WithMaxLengthOf(JobConsolSchema.JK_MasterBillNum)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|MasterBillNumber", MasterBillNumber);

			filters.AddNumberFilter(InvoiceBatchNumber, GetInvoiceBatchNumberQuery)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_ReceiptBatchNo)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|InvoiceBatchNumber", InvoiceBatchNumber);

			filters.AddTextFilterForExactComparison(OutstandingAmount, GetOutstandingAmountQuery)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_OutstandingAmount)
				.WithCategory(FilterCategories.NumbersAndReferences)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|OutstandingAmount", OutstandingAmount);

			if (GlbCompany.CurrentCompany.Country.SupportComplianceSubType)
			{
				var complianceNumberDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|GovtComplianceNumber", ComplianceNumber);
				if (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value)
				{
					filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.GovtComplianceNumber, AccountingUtils.GetComplianceNumberQuery).MultilingualDescription = complianceNumberDescription;
				}
				else
				{
					filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.GovtComplianceNumber, AccTransactionHeaderSchema.AH_TransactionReference).MultilingualDescription = complianceNumberDescription;
				}
			}

			filters.AddTextFilter(InvoiceTransactionReference, GetInvoiceTransactionReferenceQuery)
				.WithMaxLengthOf(AccTransactionHeaderReferenceSchema.AH1_Reference)
				.WithCategory(FilterCategories.NumbersAndReferences)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|InvoiceTransactionReference", InvoiceTransactionReference);

			if (EnableRelatedDisbursementTransactions)
			{
				filters.AddNumberFilter(DisbursementRelatingTo, GetRelatedDisbursementTransactionsQuery)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_TransactionNum)
				.WithCategory(FilterCategories.NumbersAndReferences)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|DisbursementRelatingTo", DisbursementRelatingTo);
			}
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(DueDate, AccTransactionHeaderSchema.AH_DueDate)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_DueDate)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|DueDate", DueDate);

			filters.AddDateFilter(TransactionDate, AccTransactionHeaderSchema.AH_InvoiceDate)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_InvoiceDate)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|TransactionDate", TransactionDate);
		}

		void AddGuidFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter(Branch, ModuleIDs.GlbBranch, AccTransactionHeaderSchema.AH_GB, Branches)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_GB)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|Branch", Branch);

			filters.AddGuidFilter(Department, ModuleIDs.GlbDepartment, AccTransactionHeaderSchema.AH_GE, Departments)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_GE)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|Department", Department);
		}

		void AddNkFilters(ModuleFilterCollection filters)
		{
			filters.AddNkFilter(Currency, AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, ModuleIDs.RefCurrency, Currencies)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|Currency", Currency);
		}

		void AddTextNkFilters(ModuleFilterCollection filters)
		{
			filters.AddTextAndNkFilter(FlightVoyageNumberAndVessel, GetFlightVoyageNumberAndVesselQuery, ModuleIDs.RefVessel, BindingLists.RefVessel_List)
				.WithMaxLengthOf(RefVesselSchema.RV_Code, JobVoyageSchema.JV_RV_NKVessel)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|FlightVoyageVesselNumber", FlightVoyageNumberAndVessel);
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(Description, AccTransactionHeaderSchema.AH_Desc)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_Desc)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|Description", Description);
		}

		void AddMiscFilters(ModuleFilterCollection filters)
		{
			var ledgerTransactionTypeFilter = new DependentListFilter(LedgerTransactionType, GetLedgerTransactionTypeQuery, LedgerTypesList, GetTransactionTypesList);
			ledgerTransactionTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|LedgerTransactionType", LedgerTransactionType);
			filters.AddCustomFilter(ledgerTransactionTypeFilter);

			var debtorAndAddressFilter = new OrgWithAddressFilter(DebtorAndAddress, GetDebtorAndAddressQuery, true);
			debtorAndAddressFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|DebtorAndAddress", DebtorAndAddress);
			filters.AddCustomFilter(debtorAndAddressFilter);

			var creditorAndAddressFilter = new OrgWithAddressFilter(CreditorAndAddress, GetCreditorAndAddressQuery, false);
			creditorAndAddressFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|CreditorAndAddress", CreditorAndAddress);
			filters.AddCustomFilter(creditorAndAddressFilter);
		}

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var disbursementFlagNames = new string[] { ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|OnlyDisplayDisbursementInvoices", "Only Display Disbursement Invoices") };
			var disbursementFlagQueries = new GetFlagsQuery[] { GetDisbursementQuery };

			filters.AddFlagsFilter(DisbursementInvoice, disbursementFlagNames, disbursementFlagQueries)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|MatchingTransactionFilter|DisbursementInvoice", DisbursementInvoice);
		}

		#region Manual Filter Setters

		public void SetLedgerTransactionTypeFilter(ZString ledgerType, ZString transactionType)
		{
			var filter = (DependentListFilter)this[LedgerTransactionType];
			filter.IsActive = true;
			filter.Property1 = ledgerType;
			filter.Property2 = transactionType;
		}

		public void SetAllNumbersFilter(ZString number)
		{
			var filter = (ModuleNumberFilter)this[AllNumbers];
			filter.IsActive = true;
			filter.Property = number;
		}

		#endregion

		#region Clear Filters

		public void ClearFilters()
		{
			this.ForEach(filter => filter.Clear());
		}

		#endregion

		#endregion

		#region Queries

		ZQuery GetChequeReferenceNumberQuery(SQLComparisonOperator comparisonOperator, ZString chequeOrReference)
		{
			return GenerateAccTransactionHeaderQuery(AccTransactionHeaderSchema.AH_ChequeOrReference, comparisonOperator, chequeOrReference);
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

		ZQuery GetTransactionNumberQuery(SQLComparisonOperator comparisonOperator, ZString transactionNumber)
		{
			return GenerateAccTransactionHeaderQuery(AccTransactionHeaderSchema.AH_TransactionNum, comparisonOperator, transactionNumber);
		}

		ZQuery GetJobInvoiceNumberQuery(SQLComparisonOperator comparisonOperator, ZString jobInvoiceNumber)
		{
			return GenerateAccTransactionHeaderQuery(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, comparisonOperator, jobInvoiceNumber);
		}

		ZQuery GetInvoiceRemittanceReferenceQuery(SQLComparisonOperator comparisonOperator, ZString invoiceRemittanceReference)
		{
			var subquery = new ZDBOnlySubQuery(typeof(AccTransactionHeaderReference), AccTransactionHeaderReferenceSchema.AH1_AH);
			subquery.AddToFilter_PossiblyCommaSeparated(AccTransactionHeaderReferenceSchema.AH1_Reference, comparisonOperator, invoiceRemittanceReference);
			subquery.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.IRR);
			return GenerateAccTransactionHeaderQuery(subquery);
		}

		ZQuery GetHouseBillNumberQuery(SQLComparisonOperator comparisonOperator, ZString houseBillNumber)
		{
			var subquery = new ZDBOnlySubQuery(typeof(JobHeader), AccTransactionHeaderSchema.AH_JH);
			subquery.AddToFilter(AccountingUtils.GetHouseBillQueryForJobHeader(comparisonOperator, houseBillNumber));
			return GenerateAccTransactionHeaderQuery(subquery);
		}

		ZQuery GetInvoiceBatchNumberQuery(SQLComparisonOperator comparisonOperator, ZString invoiceBatchNumber)
		{
			var invoicingFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
			invoicingFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, TransactionTypes.CreditNote);
			invoicingFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, TransactionTypes.AdjustmentNote);

			var query = GenerateAccTransactionHeaderQuery(AccTransactionHeaderSchema.AH_ReceiptBatchNo, comparisonOperator, invoiceBatchNumber);
			query.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_Ledger, SQLComparisonOperator.Equal, LedgerTypes.AccountsReceivable);
			query.AddToFilter(invoicingFilter, JoinCondition.And);

			return query;
		}

		ZQuery GetOutstandingAmountQuery(SQLComparisonOperator comparisonOperator, ZString outstandingAmountString)
		{
			if (ZDecimal.TryParse(outstandingAmountString, out var outstandingAmount))
			{
				if (outstandingAmount >= -922337203685477m && outstandingAmount <= 922337203685477m) // SQL Money Type range
				{
					return GenerateAccTransactionHeaderQuery(AccTransactionHeaderSchema.AH_OutstandingAmount, SQLComparisonOperator.Equal, outstandingAmount);
				}
			}

			var query = new ZQuery();
			query.IsNoResultQuery = true;
			return query;
		}

		ZQuery GetMasterBillNumberQuery(SQLComparisonOperator comparisonOperator, ZString masterBillNumber)
		{
			var subquery = new ZDBOnlySubQuery(typeof(JobHeader), AccTransactionHeaderSchema.AH_JH);
			subquery.AddSubQuery(AccountingUtils.GetMasterBillSubQueryForJobHeader(comparisonOperator, masterBillNumber), JoinCondition.And);
			return GenerateAccTransactionHeaderQuery(subquery);
		}

		ZQuery GetFlightVoyageNumberAndVesselQuery(SQLComparisonOperator comparisonOperator, ZString flightOrVoyageNumber, ZString vesselNK)
		{
			var subquery = new ZDBOnlySubQuery(typeof(JobHeader), AccTransactionHeaderSchema.AH_JH);
			subquery.AddToFilter(AccountingUtils.GetFlightVoyageNumberAndVesselQueryForJobHeader(comparisonOperator, flightOrVoyageNumber, vesselNK));
			return GenerateAccTransactionHeaderQuery(subquery);
		}

		ZQuery GetAllNumbersQuery(SQLComparisonOperator comparisonOperator, ZString number)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			var subqueries = new[] {
				GetChequeReferenceNumberQuery(comparisonOperator, number),
				GetTransactionNumberQuery(comparisonOperator, number),
				GetJobInvoiceNumberQuery(comparisonOperator, number),
				GetHouseBillNumberQuery(comparisonOperator, number),
				GetMasterBillNumberQuery(comparisonOperator, number),
				GetInvoiceBatchNumberQuery(comparisonOperator, number),
				GetOutstandingAmountQuery(comparisonOperator, number),
				GetInvoiceTransactionReferenceQuery(comparisonOperator, number),
				GetInvoiceRemittanceReferenceQuery(comparisonOperator, number),
				GetFlightVoyageNumberAndVesselQuery(comparisonOperator, number, ZString.Empty),
			};

			subqueries.ForEach(subquery => query.AddToFilter(subquery, JoinCondition.Or));

			return query;
		}

		ZQuery GetLedgerTransactionTypeQuery(ZString ledgerType, ZString transactionType)
		{
			var ledgerComparisonOperator = ledgerType.IsEmpty ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal;
			var transactionTypeComparisonOperator = transactionType.IsEmpty ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal;

			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ledgerComparisonOperator, ledgerType);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transactionTypeComparisonOperator, transactionType);
			return query;
		}

		ZQuery GetDisbursementQuery(ZBool isDisbursement)
		{
			return isDisbursement ?
				GenerateAccTransactionHeaderQuery(AccTransactionHeaderSchema.AH_TransactionCategory, SQLComparisonOperator.Equal, InvoiceTypeCalculationProvider.DisbursementInvoiceTypes) :
				new ZQuery();
		}

		ZQuery GetRelatedDisbursementTransactionsQuery(SQLComparisonOperator comparisonOperator, ZString number)
		{
			var subquery = new ZDBOnlySubQuery(typeof(AccTransactionHeaderReference), AccTransactionHeaderReferenceSchema.AH1_AH);
			subquery.AddToFilter_PossiblyCommaSeparated(AccTransactionHeaderReferenceSchema.AH1_Reference, comparisonOperator, number);
			subquery.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.ReceivableDisbursementInvoice);
			return GenerateAccTransactionHeaderQuery(subquery);
		}

		ZQuery GetCreditorAndAddressQuery(ZGuid orgPK, ZGuid addressPK)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_OH, orgPK);

			if (addressPK != ZGuid.Empty)
			{
				var org = Factory.Load<OrgHeader>(orgPK);
				if (org != null && org.AddressForSendingAPDocuments != null && addressPK == org.AddressForSendingAPDocuments.PK)
				{
					var subquery = new ZQuery(AccTransactionHeaderSchema.AH_OA_InvoiceAddressOverride, addressPK);
					subquery.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_OA_InvoiceAddressOverride, DBNull.Value);
					query.AddToFilter(subquery);
				}
				else
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_OA_InvoiceAddressOverride, addressPK);
				}
			}
			return query;
		}

		ZQuery GetDebtorAndAddressQuery(ZGuid orgPK, ZGuid addressPK)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_OH, orgPK);

			if (addressPK != ZGuid.Empty)
			{
				var sql = "";
				var org = Factory.Load<OrgHeader>(orgPK);
				if (org != null && org.AddressForSendingARDocuments != null && addressPK == org.AddressForSendingARDocuments.PK)
				{
					sql = @" 
					SELECT	AH_PK 
					FROM	dbo.AccTransactionHeader 
					WHERE	AH_OA_InvoiceAddressOverride = @addr AND AH_TransactionType <> 'INB' AND 
							AH_OH = @org AND 
							AH_FullyPaidDate IS NULL
					
					UNION ALL

					SELECT	AH_PK 
					FROM	dbo.AccTransactionHeader 
							INNER JOIN dbo.JobHeader ON AH_JH = JH_PK
							LEFT JOIN  dbo.OrgAddress AgentAddress on JH_OA_AgentCollectAddr = AgentAddress.OA_PK AND AgentAddress.OA_OH = @org
					WHERE	AH_GC = @comp AND AH_TransactionType <> 'INB' AND
							AH_OH = @org AND AH_OA_InvoiceAddressOverride IS NULL AND
							AH_FullyPaidDate IS NULL AND 
							(
								JH_OA_LocalChargesAddr = @addr OR 
								( JH_OA_LocalChargesAddr is null AND AgentAddress.OA_PK IS NULL )
							) 

					UNION ALL

					SELECT	AH_PK 
					FROM	dbo.AccTransactionHeader 
							INNER JOIN dbo.JobHeader ON AH_JH = JH_PK
							LEFT JOIN dbo.OrgAddress LocalClientAddress on JH_OA_LocalChargesAddr = LocalClientAddress.OA_PK AND LocalClientAddress.OA_OH = @org
					WHERE	AH_GC = @comp AND AH_TransactionType <> 'INB' AND
							AH_OH = @org AND AH_OA_InvoiceAddressOverride IS NULL AND
							AH_FullyPaidDate IS NULL AND
							(
								JH_OA_AgentCollectAddr = @addr OR 
								( JH_OA_AgentCollectAddr is null AND LocalClientAddress.OA_PK IS NULL )
							) ";
				}
				else
				{
					sql = @" SELECT AH_PK FROM dbo.AccTransactionHeader WHERE AH_OA_InvoiceAddressOverride = @addr AND AH_OH = @org AND AH_FullyPaidDate IS NULL AND AH_TransactionType <> 'INB'
					UNION ALL
					SELECT AH_PK FROM dbo.AccTransactionHeader INNER JOIN dbo.JobHeader ON AH_JH = JH_PK
					WHERE JH_OA_LocalChargesAddr = @addr AND AH_OH = @org AND AH_OA_InvoiceAddressOverride IS NULL AND AH_FullyPaidDate IS NULL AND AH_TransactionType <> 'INB'
					AND JH_GC = @comp
					UNION ALL
					SELECT AH_PK FROM dbo.AccTransactionHeader INNER JOIN dbo.JobHeader ON AH_JH = JH_PK
					WHERE JH_OA_AgentCollectAddr = @addr AND AH_OH = @org AND AH_OA_InvoiceAddressOverride IS NULL AND AH_FullyPaidDate IS NULL AND AH_TransactionType <> 'INB'
					AND JH_GC = @comp";
				}

				sql = " AH_PK IN (" + sql + ")";

				var zParams = new ZSqlParameterCollection();
				zParams.Add("@addr", addressPK, AccTransactionHeaderSchema.AH_OA_InvoiceAddressOverride);
				zParams.Add("@org", orgPK, AccTransactionHeaderSchema.AH_OH);
				zParams.Add("@comp", Environment.Env.CurrentCompany.PK, JobHeaderSchema.JH_GC);
				query.AddFilterAndZSQLParameterCollection(sql, zParams);
			}

			return query;
		}

		#endregion

		#region Query Helper Methods

		ZDBOnlyQuery GenerateAccTransactionHeaderQuery(SchemaColumn filterSchemaColumn, SQLComparisonOperator filterComparisonOperator, object filterValue)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			query.AddToFilter_PossiblyCommaSeparated(filterSchemaColumn, filterComparisonOperator, filterValue);
			return query;
		}

		ZDBOnlyQuery GenerateAccTransactionHeaderQuery(ZDBOnlySubQuery subquery)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			query.AddSubQuery(subquery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Filter For DB Reload

		public virtual ZQuery FilterForDBReload
		{
			get
			{
				var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				query.MaximumRows = AccountingConfigurationRegistry.Instance.MaximumResultsInMatchingSearch.Value;
				query.OrderBy = AccTransactionHeaderSchema.Constants.AH_DueDate;
				query.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, SQLComparisonOperator.Equal, null);
				query.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.NotEqual, TransactionTypes.InvoiceBatch);

				var (receivableOrgs, payableOrgs, excludedOrgs) = GroupSettlementOrgsByLedger();

				foreach (OrgLedgerFilter orgLedger in SettlementOrgInfos)
				{
					if (orgLedger.Organization.IsValid)
					{
						if (orgLedger.ARLedger)
						{
							receivableOrgs.Add(orgLedger.Organization.ToGuid());
						}

						if (orgLedger.APLedger)
						{
							payableOrgs.Add(orgLedger.Organization.ToGuid());
						}

						if (!orgLedger.APLedger && !orgLedger.ARLedger)
						{
							excludedOrgs.Add(orgLedger.Organization.ToGuid());
						}
					}
				}

				if (receivableOrgs.Count > 0 || payableOrgs.Count > 0)
				{
					var ledgerAndOrgQuery = new ZQuery();

					if (receivableOrgs.Count > 0)
					{
						var receivablesOrgQuery = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
						receivablesOrgQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, receivableOrgs.ToArray());
						ledgerAndOrgQuery.AddToFilter(receivablesOrgQuery, JoinCondition.Or);
					}

					if (payableOrgs.Count > 0)
					{
						var payablesOrgQuery = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
						payablesOrgQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, payableOrgs.ToArray());
						ledgerAndOrgQuery.AddToFilter(payablesOrgQuery, JoinCondition.Or);
					}

					query.AddToFilter(ledgerAndOrgQuery);

					if (excludedOrgs.Count > 0)
					{
						query.AddToFilter(AccTransactionHeaderSchema.AH_OH, SQLComparisonOperator.NotEqual, excludedOrgs.ToArray());
					}
				}
				else
				{
					query.IsNoResultQuery = true;
				}

				query.AddToFilter(Filter);
				query.AddOptionRecompileConditionally = true;

				return query;
			}
		}

		public ZBool IsDBReloadRequired
		{
			get
			{
				return PrimaryOrganization.IsValid;
			}
		}

		#endregion

		#region Primary Organization

		public ZGuid PrimaryOrganization
		{
			get { return primaryOrganization; }
			set
			{
				SetNonPersistentPropertyValue(PrimaryOrganizationInfo, ref primaryOrganization, value);
				var orgBizO = Factory.Load<OrgHeader>(PrimaryOrganization);
				SettlementOrgInfos.LoadOrgs(SettlementOrgQuery, orgBizO);
			}
		}
		ZGuid primaryOrganization;

		public ZPropertyInfo PrimaryOrganizationInfo
		{
			get { return GetZPropertyInfo(nameof(PrimaryOrganization)); }
		}

		public abstract OrgHeaderCollection PrimaryOrgHeaders { get; }

		#endregion

		#region Include All AR/AP

		public ZBool IncludeAllAR
		{
			get { return fIncludeAllAR; }
			set
			{
				fIncludeAllAR = value;
				IncludeAllARInfo.RefreshBinding();
				SettlementOrgInfos.SetARLedgerOfAllElements(value);
			}
		}
		ZBool fIncludeAllAR;

		public ZPropertyInfo IncludeAllARInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeAllAR)); }
		}

		public ZBool IncludeAllAP
		{
			get { return fIncludeAllAP; }
			set
			{
				fIncludeAllAP = value;
				IncludeAllAPInfo.RefreshBinding();
				SettlementOrgInfos.SetAPLedgerOfAllElements(value);
			}
		}
		ZBool fIncludeAllAP;

		public ZPropertyInfo IncludeAllAPInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeAllAP)); }
		}

		#endregion

		#region Settlement Orgs

		public virtual OrgLedgerFilterCollection SettlementOrgInfos
		{
			get
			{
				if (settlementOrgInfos == null)
				{
					settlementOrgInfos = new OrgLedgerFilterCollection(Factory);
				}
				return settlementOrgInfos;
			}
		}
		OrgLedgerFilterCollection settlementOrgInfos;

		public (List<Guid> ReceivableOrgs, List<Guid> PayableOrgs, List<Guid> ExcludedOrgs) GroupSettlementOrgsByLedger()
		{
			var receivableOrgs = new List<Guid>();
			var payableOrgs = new List<Guid>();
			var excludedOrgs = new List<Guid>();

			foreach (OrgLedgerFilter orgLedger in SettlementOrgInfos)
			{
				if (orgLedger.Organization.IsValid)
				{
					if (orgLedger.ARLedger)
					{
						receivableOrgs.Add(orgLedger.Organization.ToGuid());
					}

					if (orgLedger.APLedger)
					{
						payableOrgs.Add(orgLedger.Organization.ToGuid());
					}

					if (!orgLedger.APLedger && !orgLedger.ARLedger)
					{
						excludedOrgs.Add(orgLedger.Organization.ToGuid());
					}
				}
			}

			return (receivableOrgs, payableOrgs, excludedOrgs);
		}

		ZQuery SettlementOrgQuery
		{
			get
			{
				if (PrimaryOrganization.IsEmpty)
				{
					return null;
				}

				var relatedPartySubQuery = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent);
				relatedPartySubQuery.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, PrimaryOrganization);
				relatedPartySubQuery.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyType);
				relatedPartySubQuery.AddToFilter(OrgRelatedPartySchema.PR_FreightDirection, RelatedPartyFreightDirection);
				relatedPartySubQuery.AddToFilter(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);

				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddSubQuery(relatedPartySubQuery, JoinCondition.And);
				return query;
			}
		}

		protected abstract ZString RelatedPartyType { get; }

		protected abstract ZString RelatedPartyFreightDirection { get; }

		#endregion

		#region Lists

		#region LedgerTypesList

		public CodeDescriptionPairList LedgerTypesList
		{
			get
			{
				if (ledgerTypesList == null)
				{
					ledgerTypesList = new CodeDescriptionPairList();
					ledgerTypesList.AddPair(ZString.Empty, "Both");
					ledgerTypesList.AddPair(LedgerTypes.AccountsReceivable, "Accounts Receivable");
					ledgerTypesList.AddPair(LedgerTypes.AccountsPayable, "Accounts Payable");
				}
				return ledgerTypesList;
			}
		}
		CodeDescriptionPairList ledgerTypesList;

		#endregion

		#region TransactionTypesList

		[SuppressWeaklyTypedCollectionMessage]
		public IList GetTransactionTypesList(ZString ledger)
		{
			var list = new CodeDescriptionPairList();

			if (ledger == LedgerTypes.AccountsReceivable || ledger == LedgerTypes.AccountsPayable || ledger.IsEmpty)
			{
				list.AddPair(ZString.Empty, Res.GetString("Accounting|MatchingTransactionFilter|AllTransactionTypes", "All Transaction Types"));
				list.AddPair(TransactionTypes.AdjustmentNote, Res.GetString("Accounting|MatchingTransactionFilter|AdjustmentNote", "Adjustment Note"));
				list.AddPair(TransactionTypes.Contra, Res.GetString("Accounting|MatchingTransactionFilter|Contra", "Contra"));
				list.AddPair(TransactionTypes.CreditNote, Res.GetString("Accounting|MatchingTransactionFilter|CreditNote", "Credit Note"));
				list.AddPair(TransactionTypes.Discount, Res.GetString("Accounting|MatchingTransactionFilter|Discount", "Discount"));
				list.AddPair(TransactionTypes.ExchangeDifference, Res.GetString("Accounting|MatchingTransactionFilter|ExchangeDifference", "Exchange Difference"));
				list.AddPair(TransactionTypes.Invoice, Res.GetString("Accounting|MatchingTransactionFilter|Invoice", "Invoice"));
				list.AddPair(TransactionTypes.Journal, Res.GetString("Accounting|MatchingTransactionFilter|Journal", "Journal"));
				list.AddPair(TransactionTypes.Overpayment, Res.GetString("Accounting|MatchingTransactionFilter|Overpayment", "Overpayment"));
				list.AddPair(TransactionTypes.Payment, Res.GetString("Accounting|MatchingTransactionFilter|Payment", "Payment"));
				list.AddPair(TransactionTypes.Receipt, Res.GetString("Accounting|MatchingTransactionFilter|Receipt", "Receipt"));
				list.AddPair(TransactionTypes.Transfer, Res.GetString("Accounting|MatchingTransactionFilter|Transfer", "Transfer"));
			}

			return list;
		}

		#endregion

		#region BindingLists

		BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion

		#region Branches

		GlbBranchCollection Branches
		{
			get
			{
				if (branches == null)
				{
					var filter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
					branches = new GlbBranchCollection(Factory, filter);
				}
				return branches;
			}
		}
		GlbBranchCollection branches;

		#endregion

		#region Departments

		GlbDepartmentCollection Departments
		{
			get
			{
				if (departments == null)
				{
					departments = new GlbDepartmentCollection(Factory);
				}
				return departments;
			}
		}
		GlbDepartmentCollection departments;

		#endregion

		#region Currencies

		RefCurrencyCollection Currencies
		{
			get
			{
				if (currencies == null)
				{
					currencies = new RefCurrencyCollection(Factory);
				}
				return currencies;
			}
		}
		RefCurrencyCollection currencies;

		#endregion

		#endregion

		#region EnableRelatedDisbursementTransactions

		protected virtual bool EnableRelatedDisbursementTransactions => false;

		#endregion
	}
}

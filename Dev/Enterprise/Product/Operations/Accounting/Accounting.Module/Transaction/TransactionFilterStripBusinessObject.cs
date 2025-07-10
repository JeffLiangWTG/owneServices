using System;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public abstract partial class TransactionFilterStripBusinessObject : AccountingFilterStripBusinessObject
	{
		#region Abstract Members

		public abstract ZString CreditorDebtorText { get; }
		public abstract MultilingualString CreditorDebtorCaption { get; }

		public abstract ZBool IsPayableModule { get; }
		protected abstract ModuleIdentifier CreditorDebtorGroupModuleID { get; }
		protected abstract ZString[] LedgersToUse { get; }
		protected abstract SecurityCheckpoint ViewingNonLoginBranchTransactions { get; }

		protected virtual ZString CreditorDebtorGroupText
		{
			get { return string.Format((NoResString)"{0} Group", CreditorDebtorText); }
		}

		protected virtual MultilingualString BranchDescription
		{
			get { return ResString.GetMultilingualString("Accounting|TransactionFilter|Branch", "Branch"); }
		}

		protected abstract MultilingualString CreditorDebtorGroupCaption { get; }

		protected virtual bool IsSingleLedger
		{
			get { return true; }
		}

		public virtual ZBool ShouldShowRelatedClaim
		{
			get { return false; }
		}

		ZBool UseCreditor
		{
			get { return IsPayableModule; }
		}

		ZBool UseDebtor
		{
			get { return !IsPayableModule; }
		}

		protected virtual bool ShouldAddRelatedTransactionsNotPaidFilter
		{
			get { return true; }
		}

		protected virtual bool ShouldAddComplianceDocumentRecordFilter => AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value;

		protected virtual bool ShouldAddEInvoicingFilter
		{
			get
			{
				ZString ledgerType = IsPayableModule ? LedgerTypes.AccountsPayable : LedgerTypes.AccountsReceivable;
				return TransactionFilterStripControlPresentationProvider.IsEInvoicingColumnsAvailable(ledgerType);
			}
		}

		public virtual bool ShouldEnableDisbursementRelatingToFilter
		{
			get { return false; }
		}

		#endregion

		#region Constants

		protected const string RelatedARTransactionsFullyPaid = "ARPAID";
		protected const string RelatedARTransactionsNotFullyPaid = "AROPEN";
		protected const string RelatedARDSBTransactionsFullyPaid = "DSBPAID";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected const string RelatedARDSBTransactionsNotFullyPaid = "DSB AR NOT";
		protected const string RelatedAPTransactionsFullyPaid = "APPAID";
		protected const string RelatedAPTransactionsNotFullyPaid = "APOPEN";

		#endregion

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection moduleFilters = new ModuleFilterCollection();

			AddNumberFilters(moduleFilters);
			AddDateFilters(moduleFilters);
			AddOrganisationFilters(moduleFilters);
			AddModesAndTypesFilters(moduleFilters);
			AddStatusesFilters(moduleFilters);
			AddFinancialFilters(moduleFilters);
			AddOperationalFilters(moduleFilters);
			AddOtherFilters(moduleFilters);
			AddBranchManagementCodeFilter(moduleFilters);

			return moduleFilters;
		}

		#region	AIPStatusQuery

		ZQuery AIPStatusQuery(ZString aipStatus)
		{
			return ElectronicInvoicingHelper.GetAIPStatusQuery(aipStatus);
		}

		#endregion

		#region	AIPLastResponseReceivedQuery

		ZQuery AIPLastResponseReceivedQuery(DateComparisonOperator comparisonOperator, ZDateTime from, ZDateTime to)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(AccEInvoicingTransactionPivot), AccEInvoicingTransactionPivotSchema.AIP_ParentID);
			AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, AccEInvoicingTransactionPivotSchema.AIP_LastResponseReceivedUtc, from, to);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		#endregion

		public override ZQuery Filter
		{
			get
			{
				ZQuery resultFilter = base.Filter;
				AddLedgerFilter(resultFilter);

				return resultFilter;
			}
		}

		protected virtual void AddLedgerFilter(ZQuery filter)
		{
			if (IsSingleLedger)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgersToUse[0]);
			}
			else
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgersToUse);
			}
		}

		#region Number Filters

		protected virtual void AddNumberFilters(ModuleFilterCollection filters)
		{
			AddJobNumberFilter(filters);
			AddAllNumbersFilter(filters);
			AddMiscellaneousNumberFilters(filters);
			AddEInvoicingNumberFilters(filters);
			AddInvoiceRemittanceReferenceNumberFilter(filters);
		}

		void AddInvoiceRemittanceReferenceNumberFilter(ModuleFilterCollection filters)
		{
			var referenceNumberFilter = filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.InvoiceRemittanceReference, GetInvoiceRemittanceReferenceQuery);
			referenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|ARTransactionFilter|InvoiceRemittanceReference", "Invoice Remittance Reference");
			referenceNumberFilter.MaxLength = AccTransactionHeaderReferenceSchema.AH1_Reference.MaxLength;
			referenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
		}

		protected virtual void AddEInvoicingNumberFilters(ModuleFilterCollection filters)
		{
			if (ShouldAddEInvoicingFilter)
			{
				var batchNumFilter = filters.AddNumberRangeFilter(AccountingUtils.NumberFilterTypes.EInvoicingBatchNumber, GetEReportingBatch)
					.WithMaxLengthOf(AccEInvoicingBatchSchema.AIB_BatchNumber);
				batchNumFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|EReportingBatch", "E-Reporting Batch");
				batchNumFilter.MinValue = 0m;
				batchNumFilter.Decimals = 0;
				AddEInvoicingGovernmentAllocatedNumberFilter(filters);
				filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.EInvoicingeHubAllocatedNumber, GetEReportingeHubAllocatedNumber)
					.WithMaxLengthOf(AccEInvoicingBatchSchema.AIB_EHubAllocatedNumber)
					.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|EReportingeHub", "E-Reporting eHub #");
				filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.EInvoicingAuthorisationNumber, GetEReportingAuthorisationNumber)
					.WithMaxLengthOf(AccTransactionHeaderAuthorisationRecordSchema.AHF_Number)
					.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|EReportingAuth", "E-Reporting Auth #");
			}
		}

		protected virtual void AddEInvoicingGovernmentAllocatedNumberFilter(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.EInvoicingGovernmentAllocatedNumber, GetEReportingGovernmentAllocatedNumber)
				.WithMaxLengthOf(AccEInvoicingBatchSchema.AIB_GovernmentAllocatedNumber)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|EReportingGovt", "E-Reporting Govt #");
		}

		protected virtual void AddMiscellaneousNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.ChequeReferenceNumber, GetChequeNumberQuery)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_ChequeOrReference)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|ChequeReferenceNumber", "Check/Reference #");
			filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.DepositBatchNumber, GetDepositBatchNumberQuery)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_ReceiptBatchNo)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|DepositBatchNumber", "Deposit Batch #");
			filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.DDRBatchNumber, GetDDRBatchNumberQuery)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_ReceiptBatchNo)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|DDRBatchNumber", "DDR Batch #");
			if (GlbCompany.CurrentCompany.Country.SupportComplianceSubType)
			{
				var complianceNumberDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|GovtComplianceNumber", "Compliance #");
				if (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.Value)
				{
					filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.GovtComplianceNumber, AccountingUtils.GetComplianceNumberQuery).MultilingualDescription = complianceNumberDescription;
				}
				else
				{
					filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.GovtComplianceNumber, AccTransactionHeaderSchema.AH_TransactionReference).MultilingualDescription = complianceNumberDescription;
				}
			}
		}

		protected void AddAllNumbersFilter(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter("All Numbers", GetAllNumbersQuery)
				.WithMaxLengthOf(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|AllNumbers", "All Numbers");
		}

		protected virtual void AddJobNumberFilter(ModuleFilterCollection filters)
		{
			ModuleNumberFilter jobNumberFilter = filters.AddNumberFilter(AccountingUtils.NumberFilterTypes.JobNumber, GetJobNumberQuery);
			jobNumberFilter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Exact;
			CodeDescriptionPairList itemsToRemoveFromList = new CodeDescriptionPairList();
			foreach (CodeDescriptionPair pair in jobNumberFilter.ComparisonOperator_List)
			{
				if (pair.Code != ModuleNumberFilter.ComparisonConstants.Exact)
				{
					itemsToRemoveFromList.Add(pair);
				}
			}
			foreach (CodeDescriptionPair pair in itemsToRemoveFromList)
			{
				jobNumberFilter.ComparisonOperator_List.Remove(pair);
			}
			jobNumberFilter.RemoveComparisonOperatorsLeavingOne(ModuleNumberFilter.ComparisonConstants.Exact);
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|JobNumber", "Job #");
		}

		#region GetEReportingQuery

		ZQuery GetEReportingBatch(INumericZType from, INumericZType to)
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			var subQuery = new ZDBOnlySubQuery(typeof(AccEInvoicingTransactionPivot), AccEInvoicingTransactionPivotSchema.AIP_ParentID);
			var subQuery2 = new ZDBOnlySubQuery(typeof(AccEInvoicingBatch), AccEInvoicingBatchSchema.PK);
			subQuery2.AddToFilter(ModuleNumberRangeFilter.AddToFilters(new ZQuery(), AccEInvoicingBatchSchema.AIB_BatchNumber, from, to));
			subQuery.AddSubQuery(AccEInvoicingTransactionPivotSchema.AIP_AIB, subQuery2, JoinCondition.And);
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetEReportingGovernmentAllocatedNumber(SQLComparisonOperator comparisonOperator, ZString governmentAllocatedNumber)
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			var subQuery = new ZDBOnlySubQuery(typeof(AccEInvoicingTransactionPivot), AccEInvoicingTransactionPivotSchema.AIP_ParentID);
			var subQuery2 = new ZDBOnlySubQuery(typeof(AccEInvoicingBatch), AccEInvoicingBatchSchema.PK);
			subQuery2.AddToFilter_PossiblyCommaSeparated(AccEInvoicingBatchSchema.AIB_GovernmentAllocatedNumber, comparisonOperator, governmentAllocatedNumber);
			subQuery.AddSubQuery(AccEInvoicingTransactionPivotSchema.AIP_AIB, subQuery2, JoinCondition.And);

			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetEReportingeHubAllocatedNumber(SQLComparisonOperator comparisonOperator, ZString eHubAllocatedNumber)
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			var subQuery = new ZDBOnlySubQuery(typeof(AccEInvoicingTransactionPivot), AccEInvoicingTransactionPivotSchema.AIP_ParentID);
			var subQuery2 = new ZDBOnlySubQuery(typeof(AccEInvoicingBatch), AccEInvoicingBatchSchema.PK);
			subQuery2.AddToFilter_PossiblyCommaSeparated(AccEInvoicingBatchSchema.AIB_EHubAllocatedNumber, comparisonOperator, eHubAllocatedNumber);
			subQuery.AddSubQuery(AccEInvoicingTransactionPivotSchema.AIP_AIB, subQuery2, JoinCondition.And);

			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetEReportingAuthorisationNumber(SQLComparisonOperator comparisonOperator, ZString authorisationNumber)
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			var subQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeaderAuthorisationRecord), AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId);
			subQuery.AddToFilter_PossiblyCommaSeparated(AccTransactionHeaderAuthorisationRecordSchema.AHF_Number, comparisonOperator, authorisationNumber);

			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region GetJobNumberQuery

		ZQuery GetJobNumberQuery(SQLComparisonOperator @operator, ZString number)
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			result.AddSubQuery(JobNumberQuery(@operator, number), JoinCondition.And);
			return result;
		}

		ZDBOnlySubQuery JobNumberQuery(SQLComparisonOperator @operator, ZString number)
		{
			var transactionLinesSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);

			if (AccountingUtils.AnyNumberNotExceedingMaxLength(ref number, JobHeaderSchema.JH_JobNum.MaxLength))
			{
				var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
				jobHeaderSubQuery.AddToFilter_PossiblyCommaSeparated(JobHeaderSchema.JH_JobNum, @operator, number);
				AddJobCompanyFilter(jobHeaderSubQuery);
				transactionLinesSubQuery.AddSubQuery(AccTransactionLinesSchema.AL_JH, jobHeaderSubQuery, JoinCondition.And);
			}
			else
			{
				transactionLinesSubQuery.IsNoResultQuery = true;
			}
			return transactionLinesSubQuery;
		}

		protected virtual void AddJobCompanyFilter(ZDBOnlySubQuery jobHeaderSubQuery)
		{
			jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
		}

		#endregion

		#region	GetChequeNumberQuery

		ZQuery GetChequeNumberQuery(SQLComparisonOperator @operator, ZString number)
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			result.AddSubQuery(ChequeNumberQuery(@operator, number), JoinCondition.And);
			return result;
		}

		ZDBOnlySubQuery ChequeNumberQuery(SQLComparisonOperator @operator, ZString number)
		{
			var result = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);

			if (AccountingUtils.AnyNumberNotExceedingMaxLength(ref number, AccTransactionHeaderSchema.AH_ChequeOrReference.MaxLength))
			{
				result.AddToFilter_PossiblyCommaSeparated(AccTransactionHeaderSchema.AH_ChequeOrReference, @operator, number);
				result.AddToFilter(GetInvoiceCommonFilter());
			}
			else
			{
				result.IsNoResultQuery = true;
			}
			return result;
		}

		#endregion

		#region GetDepositBatchNumberQuery

		ZQuery GetDepositBatchNumberQuery(SQLComparisonOperator @operator, ZString number)
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			result.AddSubQuery(DepositBatchNumberQuery(@operator, number), JoinCondition.And);
			return result;
		}

		ZDBOnlySubQuery DepositBatchNumberQuery(SQLComparisonOperator @operator, ZString number)
		{
			var result = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);

			if (AccountingUtils.AnyNumberNotExceedingMaxLength(ref number, AccTransactionHeaderSchema.AH_ReceiptBatchNo.MaxLength))
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionTypes.Receipt);
				result.AddToFilter_PossiblyCommaSeparated(AccTransactionHeaderSchema.AH_ReceiptBatchNo, @operator, number);
				result.AddToFilter(GetInvoiceCommonFilter());
			}
			else
			{
				result.IsNoResultQuery = true;
			}
			return result;
		}

		#endregion

		#region GetDDRBatchNumberQuery

		ZQuery GetDDRBatchNumberQuery(SQLComparisonOperator @operator, ZString number)
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			result.AddSubQuery(DDRBatchNumberQuery(@operator, number), JoinCondition.And);
			return result;
		}

		ZDBOnlySubQuery DDRBatchNumberQuery(SQLComparisonOperator @operator, ZString number)
		{
			var result = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);

			if (AccountingUtils.AnyNumberNotExceedingMaxLength(ref number, AccTransactionHeaderSchema.AH_ReceiptBatchNo.MaxLength))
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionTypes.Payment);
				result.AddToFilter_PossiblyCommaSeparated(AccTransactionHeaderSchema.AH_ReceiptBatchNo, @operator, number);
				result.AddToFilter(GetInvoiceCommonFilter());
			}
			else
			{
				result.IsNoResultQuery = true;
			}
			return result;
		}

		#endregion

		#region GetAllNumbersQuery

		protected virtual ZQuery GetAllNumbersQuery(SQLComparisonOperator @operator, ZString number)
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			var jobNumberQuery = JobNumberQuery(@operator, number);
			var transactionNumberQuery = TransactionNumberQuery(@operator, number);
			var chequeOrReferenceQuery = ChequeNumberQuery(@operator, number);
			var depositBatchNumberQuery = DepositBatchNumberQuery(@operator, number);
			var ddrBatchNumberQuery = DDRBatchNumberQuery(@operator, number);
			var jobInvoiceNumberQuery = JobInvoiceNumberQuery(@operator, number);

			var queriesToJoinWithUnion = new[] {
				jobNumberQuery,
				transactionNumberQuery,
				chequeOrReferenceQuery,
				depositBatchNumberQuery,
				ddrBatchNumberQuery,
				jobInvoiceNumberQuery
			}.Where(q => !q.IsNoResultQuery);

			if (queriesToJoinWithUnion.Any())
			{
				var firstSubQuery = queriesToJoinWithUnion.First();
				queriesToJoinWithUnion.Skip(1).ForEach(q => firstSubQuery.AddAsUnionQuery(q, true));
				result.AddSubQuery(firstSubQuery, JoinCondition.And);
			}
			else
			{
				result.IsNoResultQuery = true;
			}
			return result;
		}

		protected virtual ZQuery GetCommonNumberReferenceQuery(SQLComparisonOperator @operator, ZString number)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			ZDBOnlySubQuery jobInvoiceNumberQuery = JobInvoiceNumberQuery(@operator, number);
			ZDBOnlySubQuery transactionNumberQuery = TransactionNumberQuery(@operator, number);

			jobInvoiceNumberQuery.AddAsUnionQuery(transactionNumberQuery, true);
			result.AddSubQuery(jobInvoiceNumberQuery, JoinCondition.And);

			return result;
		}

		protected ZQuery GetInvoiceCommonFilter()
		{
			var result = new ZQuery();
			result.AddToFilter(GetInvoiceCompanyFilter());
			result.AddToFilter(GetInvoiceLedgerAndTransactionTypeFilter());

			return result;
		}

		protected virtual ZQuery GetInvoiceCompanyFilter()
		{
			var result = new ZQuery();
			result.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);

			return result;
		}

		protected virtual ZQuery GetInvoiceLedgerAndTransactionTypeFilter()
		{
			var result = new ZQuery();
			result.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, SQLComparisonOperator.Equal, LedgersToUse);
			result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.NotEqual, TransactionTypes.InvoiceBatch);

			return result;
		}

		protected virtual ZDBOnlySubQuery TransactionNumberQuery(SQLComparisonOperator @operator, ZString number)
		{
			var result = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);

			if (AccountingUtils.AnyNumberNotExceedingMaxLength(ref number, AccTransactionHeaderSchema.AH_TransactionNum.MaxLength))
			{
				result.AddToFilter_PossiblyCommaSeparated(AccTransactionHeaderSchema.AH_TransactionNum, @operator, number);
				result.AddToFilter(GetInvoiceCommonFilter());
			}
			else
			{
				result.IsNoResultQuery = true;
			}
			return result;
		}

		#endregion

		#region GetJobInvoiceNumberQuery

		protected ZQuery GetJobInvoiceNumberQuery(SQLComparisonOperator @operator, ZString number)
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			result.AddSubQuery(JobInvoiceNumberQuery(@operator, number), JoinCondition.And);
			return result;
		}

		protected ZDBOnlySubQuery JobInvoiceNumberQuery(SQLComparisonOperator @operator, ZString number)
		{
			var result = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);

			if (AccountingUtils.AnyNumberNotExceedingMaxLength(ref number, AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef.MaxLength))
			{
				result.AddToFilter_PossiblyCommaSeparated(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, @operator, number);
				result.AddToFilter(GetInvoiceCommonFilter());
			}
			else
			{
				result.IsNoResultQuery = true;
			}
			return result;
		}

		#endregion

		#endregion

		#region Operations Filters

		protected virtual void AddOperationalFilters(ModuleFilterCollection filters)
		{
			FilterCategory operationsFiltersCategory = new FilterCategory(ResString.GetMultilingualString("Accounting|TransactionFilter|OperationsFilters", "Operations Filters"));

			var filter2 = filters.AddNumberFilter("House Bill #", GetHouseBillQuery);
			filter2.Category = operationsFiltersCategory;
			filter2.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|HouseBill", "House Bill #");
			filter2.MaxLength = Math.Max(JobShipmentSchema.JS_HouseBill.MaxLength, JobDeclarationSchema.JE_HouseBill.MaxLength);

			ModuleFilter filter = filters.AddNumberFilter("Master Bill #/Ocean Bill #", GetMasterBillQuery);
			filter.Category = operationsFiltersCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|MasterBillOceanBill", "Master Bill #/Ocean Bill #");

			filter = filters.AddTextAndNkFilter("Flight/Voyage # and Vessel", GetFlightVoyageNumberAndVesselQuery, ModuleIDs.RefVessel, BindingLists.RefVessel_List)
				.WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			filter.Category = operationsFiltersCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|FlightVoyageAndVessel", "Flight/Voyage # and Vessel");

			ModuleNumberFilter customsEntryNoFilter = filters.AddNumberFilter("Customs Entry #", GetCustomsEntryNoQuery);
			customsEntryNoFilter.Category = operationsFiltersCategory;
			customsEntryNoFilter.IsPublishedOnWeb = false;
			customsEntryNoFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|CustomsEntry", "Customs Entry #");
			customsEntryNoFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;

			ModuleNumberFilter orderNoFilter = filters.AddNumberFilter("Order #", GetOrderNoQuery);
			orderNoFilter.Category = operationsFiltersCategory;
			orderNoFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|Order", "Order #");
			orderNoFilter.MaxLength = JobOrderHeaderSchema.JD_OrderNumber.MaxLength;

			filter = filters.AddGuidFilter("Carrier", ModuleIDs.Organisation, GetCarrierQuery, BindingLists.ShippingProvider_List);
			filter.Category = operationsFiltersCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|Carrier", "Carrier");

			filters.AddNumberFilter("Job Local Reference", GetLocalJobReferenceQuery)
				.WithMaxLengthOf(JobHeaderSchema.JH_JobLocalReference)
				.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|LocalJobReference", "Job Local Reference");
		}

		#region GetCarrierQuery

		ZQuery GetCarrierQuery(ZGuid carrierPK)
		{
			ZQuery result = new ZQuery();
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery transactionLine1 = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
			ZDBOnlySubQuery jobSubQuery1 = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobShipmentSchema.PK);
			ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			ZDBOnlySubQuery consolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);
			ZDBOnlySubQuery addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobConsolSchema.JK_OA_ShippingLineAddress);
			addressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, carrierPK);
			consolSubQuery.AddSubQuery(addressSubQuery, JoinCondition.And);
			pivotSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);
			ZDBOnlySubQuery shipmentConsolPivotSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobShipmentSchema.PK);
			shipmentConsolPivotSubQuery.AddToFilter(JobShipmentSchema.JS_IsShipping, ZBool.False);
			shipmentConsolPivotSubQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);

			ZQuery shipmentDeliveryAgentQuery = new ZQuery(JobShipmentSchema.JS_OH_DeliveryAgent, carrierPK);
			shipmentDeliveryAgentQuery.AddToFilter(JobShipmentSchema.JS_IsShipping, ZBool.True);
			shipmentSubQuery.AddSubQuery(shipmentConsolPivotSubQuery, JoinCondition.And);
			shipmentSubQuery.AddToFilter(shipmentDeliveryAgentQuery, JoinCondition.Or);
			jobSubQuery1.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery, JoinCondition.And);

			ZDBOnlySubQuery consolSubQuery2 = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConsolSchema.PK);
			ZDBOnlySubQuery addressSubQuery2 = new ZDBOnlySubQuery(typeof(OrgAddress), JobConsolSchema.JK_OA_ShippingLineAddress);
			addressSubQuery2.AddToFilter(OrgAddressSchema.OA_OH, carrierPK);
			consolSubQuery2.AddSubQuery(addressSubQuery2, JoinCondition.And);
			jobSubQuery1.AddSubQuery(JobHeaderSchema.JH_ParentID, consolSubQuery2, JoinCondition.Or);
			transactionLine1.AddSubQuery(AccTransactionLinesSchema.AL_JH, jobSubQuery1, JoinCondition.And);
			dbOnlyQuery.AddSubQuery(transactionLine1, JoinCondition.And);
			result.AddToFilter(dbOnlyQuery);
			return result;
		}

		#endregion

		#region GetOrderNoQuery

		ZQuery GetOrderNoQuery(SQLComparisonOperator comparisonOperator, ZString orderNo)
		{
			ZQuery result = new ZQuery();
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery transactionLinesSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
			ZDBOnlySubQuery jobSubQuery = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobShipmentSchema.PK);

			ZDBOnlySubQuery orderSubQuery1 = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.JD_JS);
			orderSubQuery1.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumber, comparisonOperator, orderNo);
			shipmentSubQuery.AddSubQuery(orderSubQuery1, JoinCondition.And);

			ZDBOnlySubQuery cartageSubQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
			ZDBOnlySubQuery orderItemSubQuery = new ZDBOnlySubQuery(typeof(OrderItem), JobOrderItemSchema.JT_JP);
			orderItemSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobOrderItemSchema.JT_OrderReference, comparisonOperator, orderNo);
			cartageSubQuery.AddSubQuery(orderItemSubQuery, JoinCondition.And);
			shipmentSubQuery.AddSubQuery(cartageSubQuery, JoinCondition.Or);

			jobSubQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery, JoinCondition.And);

			ZDBOnlySubQuery orderSubQuery2 = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.JD_JE);
			ZDBOnlySubQuery declarationSubQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.PK);
			orderSubQuery2.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumber, comparisonOperator, orderNo);
			declarationSubQuery.AddSubQuery(orderSubQuery2, JoinCondition.And);

			jobSubQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, declarationSubQuery, JoinCondition.Or);
			transactionLinesSubQuery.AddSubQuery(AccTransactionLinesSchema.AL_JH, jobSubQuery, JoinCondition.And);
			dbOnlyQuery.AddSubQuery(transactionLinesSubQuery, JoinCondition.And);
			result.AddToFilter(dbOnlyQuery);

			return result;
		}

		#endregion

		#region GetCustomsEntryNoQuery

		ZQuery GetCustomsEntryNoQuery(SQLComparisonOperator comparisonOperator, ZString customsEntryNo)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery transactionLine = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
			ZDBOnlySubQuery jobSubQuery = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);

			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			shipmentSubQuery.AddToFilter(ForwardingShipmentFilterProvider.GetJobShipmentFromEntryNumber(comparisonOperator, customsEntryNo));
			jobSubQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery, JoinCondition.And);

			ZDBOnlySubQuery declarationSubQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.PK);
			declarationSubQuery.AddToFilter(ObjectFactory.Get<ICustomsFilterProvider>().GetJobDeclarationFromEntryNumber(comparisonOperator, customsEntryNo));
			jobSubQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, declarationSubQuery, JoinCondition.Or);

			transactionLine.AddSubQuery(AccTransactionLinesSchema.AL_JH, jobSubQuery, JoinCondition.And);
			result.AddSubQuery(transactionLine, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetFlightVoyageNumberAndVesselQuery

		protected virtual ZQuery GetFlightVoyageNumberAndVesselQuery(SQLComparisonOperator @operator, ZString flightOrVoyageNo, ZString vesselNK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery transactionLine = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
			ZDBOnlySubQuery jobSubQuery = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			ZDBOnlySubQuery conShipSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			ZDBOnlySubQuery consolTransportSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			ZDBOnlySubQuery sailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			ZDBOnlySubQuery voyDestinationSubQuery = new ZDBOnlySubQuery(typeof(AutoJobVoyDestination), JobVoyDestinationSchema.PK);
			ZDBOnlySubQuery voySubQuery = new ZDBOnlySubQuery(typeof(AutoJobVoyage), JobVoyageSchema.PK);

			if (vesselNK != "")
			{
				voySubQuery.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, vesselNK);
			}
			voySubQuery.AddToFilter(JobVoyageSchema.JV_VoyageFlight, @operator, flightOrVoyageNo);
			voyDestinationSubQuery.AddSubQuery(JobVoyDestinationSchema.JB_JV, voySubQuery, JoinCondition.And);
			sailingSubQuery.AddSubQuery(JobSailingSchema.JX_JB, voyDestinationSubQuery, JoinCondition.And);
			consolTransportSubQuery.AddSubQuery(JobConsolTransportSchema.JW_JX, sailingSubQuery, JoinCondition.And);
			conShipSubQuery.AddSubQuery(JobConShipLinkSchema.JN_JK, consolTransportSubQuery, JoinCondition.And);
			shipmentSubQuery.AddToFilter(JobShipmentSchema.JS_IsShipping, ZBool.False);
			shipmentSubQuery.AddSubQuery(conShipSubQuery, JoinCondition.And);

			jobSubQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery, JoinCondition.And);

			ZDBOnlySubQuery shipmentSubQuery2 = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			ZDBOnlySubQuery conShipSubQuery2 = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			ZDBOnlySubQuery consolTransportSubQuery2 = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			if (vesselNK != "")
			{
				consolTransportSubQuery2.AddToFilter(JobConsolTransportSchema.JW_Vessel, vesselNK);
			}
			consolTransportSubQuery2.AddToFilter(JobConsolTransportSchema.JW_VoyageFlight, @operator, flightOrVoyageNo);
			conShipSubQuery2.AddSubQuery(JobConShipLinkSchema.JN_JK, consolTransportSubQuery2, JoinCondition.And);
			shipmentSubQuery2.AddToFilter(JobShipmentSchema.JS_IsShipping, ZBool.False);
			shipmentSubQuery2.AddSubQuery(conShipSubQuery2, JoinCondition.And);
			jobSubQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery2, JoinCondition.Or);
			transactionLine.AddSubQuery(AccTransactionLinesSchema.AL_JH, jobSubQuery, JoinCondition.And);

			ZDBOnlySubQuery jobSubQuery4 = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);
			ZDBOnlySubQuery shipmentSubQuery4 = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			ZDBOnlySubQuery sailingSubQuery4 = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			ZDBOnlySubQuery voyDestinationSubQuery4 = new ZDBOnlySubQuery(typeof(AutoJobVoyDestination), JobVoyDestinationSchema.PK);
			ZDBOnlySubQuery voySubQuery4 = new ZDBOnlySubQuery(typeof(AutoJobVoyage), JobVoyageSchema.PK);
			if (vesselNK != "")
			{
				voySubQuery4.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, vesselNK);
			}
			shipmentSubQuery4.AddToFilter(JobShipmentSchema.JS_IsShipping, ZBool.True);
			voySubQuery4.AddToFilter(JobVoyageSchema.JV_VoyageFlight, @operator, flightOrVoyageNo);
			voyDestinationSubQuery4.AddSubQuery(JobVoyDestinationSchema.JB_JV, voySubQuery4, JoinCondition.And);
			sailingSubQuery4.AddSubQuery(JobSailingSchema.JX_JB, voyDestinationSubQuery4, JoinCondition.And);
			shipmentSubQuery4.AddSubQuery(JobShipmentSchema.JS_JX, sailingSubQuery4, JoinCondition.And);
			jobSubQuery4.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery4, JoinCondition.And);
			transactionLine.AddSubQuery(AccTransactionLinesSchema.AL_JH, jobSubQuery4, JoinCondition.Or);
			result.AddSubQuery(transactionLine, JoinCondition.And);
			return result;
		}

		#endregion

		#region GetLocalJobReferenceQuery

		ZQuery GetLocalJobReferenceQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery transactionLinesSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
			ZDBOnlySubQuery jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
			jobHeaderSubQuery.AddToFilter_PossiblyCommaSeparated(JobHeaderSchema.JH_JobLocalReference, @operator, value);
			AddJobCompanyFilter(jobHeaderSubQuery);
			transactionLinesSubQuery.AddSubQuery(AccTransactionLinesSchema.AL_JH, jobHeaderSubQuery, JoinCondition.And);
			result.AddSubQuery(transactionLinesSubQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region GetMasterBillQuery

		ZQuery GetMasterBillQuery(SQLComparisonOperator @operator, ZString masterBill)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery transactionLine = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);

			transactionLine.AddSubQuery(AccTransactionLinesSchema.AL_JH,
				AccountingUtils.GetMasterBillSubQueryForJobHeader(@operator, masterBill), JoinCondition.And);
			result.AddSubQuery(transactionLine, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetHouseBillQuery

		ZQuery GetHouseBillQuery(SQLComparisonOperator @operator, ZString houseBill)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery transactionLine = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
			ZDBOnlySubQuery jobSubQuery = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);
			ZDBOnlySubQuery shipmentSubQuery = null;
			ZDBOnlySubQuery jobDeclarationSubQuery = null;

			var oldValue = houseBill;

			if (AccountingUtils.AnyNumberNotExceedingMaxLength(ref houseBill, JobShipmentSchema.JS_HouseBill.MaxLength))
			{
				shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
				shipmentSubQuery.AddToFilter_PossiblyCommaSeparated(JobShipmentSchema.JS_HouseBill, @operator, houseBill);
				jobSubQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery, JoinCondition.And);
			}

			houseBill = oldValue;

			if (AccountingUtils.AnyNumberNotExceedingMaxLength(ref houseBill, JobDeclarationSchema.JE_HouseBill.MaxLength))
			{
				jobDeclarationSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
				jobDeclarationSubQuery.AddToFilter_PossiblyCommaSeparated(JobDeclarationSchema.JE_HouseBill, @operator, houseBill);
				jobSubQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, jobDeclarationSubQuery, JoinCondition.Or);
			}

			if (shipmentSubQuery != null || jobDeclarationSubQuery != null)
			{
				transactionLine.AddSubQuery(AccTransactionLinesSchema.AL_JH, jobSubQuery, JoinCondition.And);
				result.AddSubQuery(transactionLine, JoinCondition.And);
			}
			else
			{
				result.IsNoResultQuery = true;
			}

			return result;
		}

		#endregion

		#endregion

		#region Date Filters

		protected virtual void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(Business.AccountingUtils.DateFilterTypes.PostDate, GetAH_PostDateQuery).MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|PostDate", "Post Date");
			filters.AddDateFilter(Business.AccountingUtils.DateFilterTypes.TransactionDate, GetAH_InvoiceDateQuery).MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|TransactionDate", "Transaction Date");
			filters.AddDateFilter(Business.AccountingUtils.DateFilterTypes.DueDate, GetAH_DueDateQuery).MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|DueDate", "Due Date");
			filters.AddDateFilter("All", GetAllDatesQuery).MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|All", "All");

			if (ShouldAddEInvoicingFilter)
			{
				var eInvoicingLastResponseReceivedFilter = filters.AddDateFilter("EInvoicing Last Response Received UTC", AIPLastResponseReceivedQuery);
				eInvoicingLastResponseReceivedFilter.Category = FilterCategories.Dates;
				eInvoicingLastResponseReceivedFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|EInvoicingLastResponseReceivedUTC", "E-Reporting Last Response Received UTC");
			}
		}

		#region GetZQueryFor2DateTime

		protected ZQuery GetZQueryFor2DateTime(DateComparisonOperator comparisonOperator, SchemaDateTimeColumn column, ZDateTime date1, ZDateTime date2)
		{
			ZQuery filter = new ZQuery();
			AddDateTimeRange(filter, comparisonOperator, JoinCondition.And, column, date1, date2);
			return filter;
		}

		#endregion

		#region GetAH_PostDateQuery

		ZQuery GetAH_PostDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetZQueryFor2DateTime(comparisonOperator, AccTransactionHeaderSchema.AH_PostDate, date1, date2);
		}

		#endregion

		#region GetAH_InvoiceDateQuery

		ZQuery GetAH_InvoiceDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetZQueryFor2DateTime(comparisonOperator, AccTransactionHeaderSchema.AH_InvoiceDate, date1, date2);
		}

		#endregion

		#region GetAH_DueDateQuery

		ZQuery GetAH_DueDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetZQueryFor2DateTime(comparisonOperator, AccTransactionHeaderSchema.AH_DueDate, date1, date2);
		}

		#endregion

		#region GetAllDatesQuery

		ZQuery GetAllDatesQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			ZQuery result = new ZQuery();
			result.DefaultJoinCondition = JoinCondition.Or;
			result.AddToFilter(GetAH_PostDateQuery(comparisonOperator, date1, date2));
			result.AddToFilter(GetAH_InvoiceDateQuery(comparisonOperator, date1, date2));
			result.AddToFilter(GetAH_DueDateQuery(comparisonOperator, date1, date2));
			return result;
		}

		#endregion

		#endregion

		#region Organisation Filters

		protected virtual void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			ModuleGuidFilter creditorDebtorFilter = filters.AddGuidFilter(CreditorDebtorText, ModuleIDs.Organisation, GetCreditorDebtorQuery, AH_OHList);
			creditorDebtorFilter.Category = FilterCategories.Organisations;
			creditorDebtorFilter.MultilingualDescription = CreditorDebtorCaption;

			ModuleGuidFilter creditorDebtorGroupFilter = filters.AddGuidFilter(CreditorDebtorGroupText, CreditorDebtorGroupModuleID, GetDebtorCreditorGroupQuery, CreditorDebtorGroupCollection);
			creditorDebtorGroupFilter.Category = FilterCategories.Organisations;
			creditorDebtorGroupFilter.MultilingualDescription = CreditorDebtorGroupCaption;

			ModuleGuidFilter settlementGroupFilter = filters.AddGuidFilter("Settlement Group", ModuleIDs.Organisation, GetSettlementGroupQuery, OrgSettlementGroup_List);
			settlementGroupFilter.Category = FilterCategories.Organisations;
			settlementGroupFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|SettlementGroup", "Settlement Group");

			ModuleGuidFilter branchFilter = filters.AddGuidFilter("Branch", ModuleIDs.GlbBranch, AccTransactionHeaderSchema.AH_GB, BranchList);
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.MultilingualDescription = BranchDescription;
			if (!ViewingNonLoginBranchTransactions.IsAllowed)
			{
				branchFilter.DefaultProperty = GlbBranch.CurrentBranch.PK;
				branchFilter.Visibility = FilterVisibility.AlwaysVisible;
				branchFilter.ReadOnly = true;
			}

			if (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value)
			{
				ModuleGuidFilter taxBranchFilter = filters.AddGuidFilter("Tax Branch", ModuleIDs.GlbBranch, AccTransactionHeaderSchema.AH_GB_TaxBranch, BranchList);
				taxBranchFilter.Category = FilterCategories.Organisations;
				taxBranchFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|TaxBranch", "Tax Branch");
			}

			ModuleGuidFilter departmentFilter = filters.AddGuidFilter("Department", ModuleIDs.GlbDepartment, AccTransactionHeaderSchema.AH_GE, AH_GEList);
			departmentFilter.Category = FilterCategories.Organisations;
			departmentFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|Department", "Department");

			ModuleGuidFilter relatedTransactionsOrganisationFilter = filters.AddGuidFilter("Related Invoice Organization", ModuleIDs.Organisation, GetRelatedTransactionsOrganisationQuery, AH_OHListForRelatedTransactions);
			relatedTransactionsOrganisationFilter.Category = FilterCategories.Organisations;
			relatedTransactionsOrganisationFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|RelatedTransactionsOrganisation", "Related Invoice Organization");

			AddInvoiceAddressFilter(filters);
		}

		protected virtual void AddInvoiceAddressFilter(ModuleFilterCollection filters)
		{ }

		#region GetCreditorDebtorQuery

		protected virtual ZQuery GetCreditorDebtorQuery(ZGuid oH_PK)
		{
			return new ZQuery(AccTransactionHeaderSchema.AH_OH, oH_PK);
		}

		#endregion

		#region GetDebtorCreditorGroupQuery

		protected virtual ZQuery GetDebtorCreditorGroupQuery(ZGuid debtorCreditorPK)
		{
			ZQuery query = new ZQuery();
			ZDBOnlyQuery transactionHeaderQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			if (UseCreditor)
			{
				orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_OG_APCreditorGroup, debtorCreditorPK);
			}
			else
			{
				orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_OJ_ARDebtorGroup, debtorCreditorPK);
			}
			orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			transactionHeaderQuery.AddSubQuery(AccTransactionHeaderSchema.AH_OH, orgCompanyDataQuery, JoinCondition.And);
			query.AddToFilter(transactionHeaderQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region GetSettlementGroupQuery

		protected virtual ZQuery GetSettlementGroupQuery(ZGuid settlementGroup)
		{
			ZDBOnlySubQuery orgRelatedPartyQuery = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent);
			orgRelatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_PartyType, UseCreditor ? RelatedPartyTypeList.Codes.APSettlementGroup : RelatedPartyTypeList.Codes.ARSettlementGroup);
			orgRelatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, settlementGroup);
			orgRelatedPartyQuery.AddToFilter(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);

			ZDBOnlyQuery transactionHeaderQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			transactionHeaderQuery.AddSubQuery(AccTransactionHeaderSchema.AH_OH, orgRelatedPartyQuery, JoinCondition.And);

			OrgHeader settlementHeader = Factory.Load<OrgHeader>(settlementGroup);
			bool fallbackToMainOrg = settlementHeader != null && !(UseCreditor ? settlementHeader.APSettlementGroupPK : settlementHeader.ARSettlementGroupPK).IsValid;
			if (fallbackToMainOrg)
			{
				transactionHeaderQuery.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_OH, settlementGroup);
			}

			return transactionHeaderQuery;
		}

		#endregion

		#endregion.

		#region ModesAndTypes Filters

		void AddModesAndTypesFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter transactionTypeFilter = filters.AddTextFilter(AccountingUtils.ModesAndTypesFilterTypes.TransactionType, GetTransactionTypeQuery, TransactionTypeList);
			transactionTypeFilter.Category = FilterCategories.ModesAndTypes;
			transactionTypeFilter.MultilingualDescription = AccountingUtils.ModesAndTypesFilterTypes.TransactionTypeDescription;
		}

		#region GetTransactionTypeQuery

		protected virtual ZQuery GetTransactionTypeQuery(ZString value)
		{
			ZQuery result = new ZQuery();
			if (value != "ALL")
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, value);
			}
			return result;
		}

		#endregion

		#endregion

		#region Statuses Filters

		protected virtual void AddStatusesFilters(ModuleFilterCollection filters)
		{
			AddPaymentStatusFilter(filters);
			AddPrintedStatusFilter(filters);
			if (!IsPayableModule)
			{
				AddUsedByActiveCollectionBatchStatusFilter(filters);
			}
			if (GlbCompany.CurrentCompany.Country.SupportComplianceSubType)
			{
				AddComplianceSubTypeFilter(filters);
			}

			if (!IsPayableModule)
			{
				ModuleFlagsFilter isDisbursementFilter = filters.AddFlagsFilter("Disbursement Invoice", new string[1] { Res.GetString("Accounting|TransactionFilter|DisbursementInvoice", "Disbursement Invoice") }, new GetFlagsQuery[1] { GetDisbursementQuery });
				isDisbursementFilter.Category = FilterCategories.StatusAndFlags;
				isDisbursementFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|DisbursementInvoice", "Disbursement Invoice");
			}

			if (ShouldAddRelatedTransactionsNotPaidFilter)
			{
				ModuleTextFilter relatedTransactionsNotPaidFilter = filters.AddTextFilter("Related Transactions Not Paid", GetRelatedTransactionsNotPaidQuery, RelatedTransactionsNotPaidFilterOptionsList);
				relatedTransactionsNotPaidFilter.Category = FilterCategories.StatusAndFlags;
				relatedTransactionsNotPaidFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|RelatedTransactionsNotPaid", "Related Transactions Not Paid");
			}

			if (ShouldAddEInvoicingFilter)
			{
				var eInvoicingStatusFilter = filters.AddTextFilter("EInvoicing Status", AIPStatusQuery, AccEInvoicingTransactionPivotLookups.PivotStatusList);
				eInvoicingStatusFilter.Category = FilterCategories.StatusAndFlags;
				eInvoicingStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|EInvoicingStatus", "E-Reporting Status");
			}

			AddMatchStatusAndReasonFilter(filters);
		}

		void AddComplianceSubTypeFilter(ModuleFilterCollection filters)
		{
			ModuleTextFilter complianceSubTypeFilter = filters.AddTextFilter("Compliance SubType", ComplianceSubTypeQuery, ComplianceSubTypeList);
			complianceSubTypeFilter.Category = FilterCategories.StatusAndFlags;
			complianceSubTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoicePrintingFilter|ComplianceSubType", "Compliance Sub Type");
		}

		protected virtual void AddUsedByActiveCollectionBatchStatusFilter(ModuleFilterCollection filters)
		{
			ModuleTextFilter usedByBatchFilter = filters.AddTextFilter("Collection Batch", CollectionBatchStatusQuery, CollectionBatchStatusList);
			usedByBatchFilter.Category = FilterCategories.StatusAndFlags;
			usedByBatchFilter.DefaultProperty = "ALL";
			usedByBatchFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|CollectionBatch", "Collection Batch");
		}

		protected virtual void AddPrintedStatusFilter(ModuleFilterCollection filters)
		{
			ModuleTextFilter printedFilter = filters.AddTextFilter("Printed", PrintedQuery, PrintedList);
			printedFilter.Category = FilterCategories.StatusAndFlags;
			printedFilter.DefaultProperty = "ALL";
			printedFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|Printed", "Printed");
		}

		protected virtual void AddPaymentStatusFilter(ModuleFilterCollection filters)
		{
			ModuleTextFilter paymentStatusFilter = filters.AddTextFilter("Payment Status", GetPaymentStatusQuery, PaymentStatusList);
			paymentStatusFilter.Category = FilterCategories.StatusAndFlags;
			paymentStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|PaymentStatus", "Payment Status");
		}

		#region CollectionBatchStatusQuery

		ZQuery CollectionBatchStatusQuery(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZDBOnlySubQuery batchQuery = null;
			if (value == "IAB")
			{
				batchQuery = new ZDBOnlySubQuery(typeof(AccCollectionOrderLine), AccCollectionOrderLineSchema.AOL_AH, false);
			}
			else if (value == "NAB")
			{
				batchQuery = new ZDBOnlySubQuery(typeof(AccCollectionOrderLine), AccCollectionOrderLineSchema.AOL_AH, true);
			}
			if (batchQuery != null)
			{
				batchQuery.AddToFilter(AccCollectionOrderLineSchema.AOL_IsCancelled, false);
				query.AddSubQuery(batchQuery, JoinCondition.And);
			}
			return query;
		}

		#endregion

		#region PrintedQuery

		ZQuery PrintedQuery(ZString value)
		{
			ZQuery query = new ZQuery();

			if (!value.IsEmpty)
			{
				if (value == "PRN")
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_InvoicePrinted, SQLComparisonOperator.Equal, true);
				}
				else if (value == "NPR")
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_InvoicePrinted, SQLComparisonOperator.Equal, false);
				}
			}

			return query;
		}

		#endregion

		#region GetPaymentStatusQuery

		ZQuery GetPaymentStatusQuery(ZString value)
		{
			return AccountingUtils.GetPaymentStatusFilter(value);
		}

		#endregion

		#region ComplianceSubTypeQuery

		ZQuery ComplianceSubTypeQuery(ZString subType)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			if (!subType.IsEmpty)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
				var subQuery1 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentPivot), AccComplianceDocumentPivotSchema.ADP_AL);
				var subQuery2 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentLine), AccComplianceDocumentLineSchema.PK);
				var subQuery3 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentHeader), AccComplianceDocumentHeaderSchema.PK);
				subQuery3.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_ComplianceSubType, SQLComparisonOperator.Equal, subType);

				subQuery2.AddSubQuery(AccComplianceDocumentLineSchema.ADL_ADH, subQuery3, JoinCondition.And);
				subQuery1.AddSubQuery(AccComplianceDocumentPivotSchema.ADP_ADL, subQuery2, JoinCondition.And);
				subQuery.AddSubQuery(AccTransactionLinesSchema.PK, subQuery1, JoinCondition.And);
				query.AddSubQuery(AccTransactionHeaderSchema.PK, subQuery, JoinCondition.And);

				query.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_ComplianceSubType, SQLComparisonOperator.Equal, subType);
			}

			return query;
		}

		#endregion

		#region GetDisbursementQuery

		ZQuery GetDisbursementQuery(ZBool value)
		{
			ZQuery query = new ZQuery();
			if (value)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, InvoiceTypeCalculationProvider.DisbursementInvoiceTypes);
			}
			return query;
		}

		#endregion

		#region	GetRelatedTransactionsNotPaidQuery

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of SQL condition, Part of SQL command")]
		ZQuery GetRelatedTransactionsNotPaidQuery(ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			if (!value.IsEmpty)
			{
				ZSqlParameterCollection parameters = new ZSqlParameterCollection();
				parameters.Add("@AR", LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger);
				parameters.Add("@AP", LedgerTypes.AccountsPayable, AccTransactionHeaderSchema.AH_Ledger);
				parameters.Add("@INV", TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionType);
				parameters.Add("@CRD", TransactionTypes.CreditNote, AccTransactionHeaderSchema.AH_TransactionType);

				bool isFullyPaid = value == RelatedARTransactionsFullyPaid || value == RelatedAPTransactionsFullyPaid || value == RelatedARDSBTransactionsFullyPaid;
				bool isDSB = value == RelatedARDSBTransactionsFullyPaid || value == RelatedARDSBTransactionsNotFullyPaid;

				StringBuilder dSBCondition = new StringBuilder();
				if (isDSB)
				{
					dSBCondition.Append((NoResString)@" AND (");
					int length = InvoiceTypeCalculationProvider.DisbursementInvoiceTypes.Length;

					for (int i = 0; i < length; i++)
					{
						string disbursementType = InvoiceTypeCalculationProvider.DisbursementInvoiceTypes[i];
						string paramName = "@DSB" + i.ToString();

						dSBCondition.Append(string.Format("SecondaryHeader.{0} = {1}", AccTransactionHeaderSchema.Constants.AH_TransactionCategory, paramName));
						parameters.Add(paramName, disbursementType, AccTransactionHeaderSchema.AH_TransactionCategory);
						if (i < (length - 1))
						{
							dSBCondition.Append(" OR ");
						}
					}

					dSBCondition.Append(")");
				}

				result.AddFilterAndZSQLParameterCollection(GetRelatedTransactionCommandTextCommon(string.Empty, string.Empty, string.Format(@"
AND SecondaryHeader." + AccTransactionHeaderSchema.Constants.AH_FullyPaidDate + @" IS {0}NULL{1})", isFullyPaid ? "NOT " : string.Empty, dSBCondition.ToString()), false), parameters);
			}

			return result;
		}

		ZQuery GetRelatedTransactionsOrganisationQuery(ZGuid value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			if (value.IsValid)
			{
				ZSqlParameterCollection parameters = new ZSqlParameterCollection();
				parameters.Add("@AR", LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger);
				parameters.Add("@AP", LedgerTypes.AccountsPayable, AccTransactionHeaderSchema.AH_Ledger);
				parameters.Add("@INV", TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionType);
				parameters.Add("@CRD", TransactionTypes.CreditNote, AccTransactionHeaderSchema.AH_TransactionType);
				parameters.Add("@Organisation", value, AccTransactionHeaderSchema.AH_OH);
				result.AddFilterAndZSQLParameterCollection(GetRelatedTransactionCommandTextCommon(string.Empty, string.Empty, (NoResString)@"
AND SecondaryHeader." + AccTransactionHeaderSchema.Constants.AH_OH + (NoResString)@" = @Organisation)", false), parameters);
			}

			return result;
		}

		protected string GetRelatedTransactionCommandTextCommon(string prefix, string additionalFields, string suffix, bool notIn)
		{
			return string.Format(AccTransactionHeaderSchema.Constants.PK + " " + (notIn ? "NOT " : string.Empty) + @"IN (
{0}
SELECT 
	PrimaryHeader." + AccTransactionHeaderSchema.Constants.PK + @"{1}
FROM 
	" + AccTransactionHeaderSchema.Constants.SqlSchemaName + "." + AccTransactionHeaderSchema.Constants.TableName + @" AS PrimaryHeader
	INNER JOIN " + AccTransactionLinesSchema.Constants.SqlSchemaName + "." + AccTransactionLinesSchema.Constants.TableName + @" AS PrimaryLines ON PrimaryLines." + AccTransactionLinesSchema.Constants.AL_AH + @" = PrimaryHeader." + AccTransactionHeaderSchema.Constants.PK + @" 
	INNER JOIN " + AccTransactionLinesSchema.Constants.SqlSchemaName + "." + AccTransactionLinesSchema.Constants.TableName + @" AS SecondaryLines ON 
		SecondaryLines." + AccTransactionLinesSchema.Constants.AL_AC + @" = PrimaryLines." + AccTransactionLinesSchema.Constants.AL_AC + @" 
		AND SecondaryLines." + AccTransactionLinesSchema.Constants.AL_JH + @" = PrimaryLines." + AccTransactionLinesSchema.Constants.AL_JH + @" 
		AND SecondaryLines." + AccTransactionLinesSchema.Constants.AL_GB + @" = PrimaryLines." + AccTransactionLinesSchema.Constants.AL_GB + @" 
		AND SecondaryLines." + AccTransactionLinesSchema.Constants.AL_GE + @" = PrimaryLines." + AccTransactionLinesSchema.Constants.AL_GE + @"
	INNER JOIN " + AccTransactionHeaderSchema.Constants.SqlSchemaName + "." + AccTransactionHeaderSchema.Constants.TableName + @" AS SecondaryHeader ON 
		SecondaryHeader." + AccTransactionHeaderSchema.Constants.PK + @" = SecondaryLines." + AccTransactionLinesSchema.Constants.AL_AH + @"
		AND SecondaryHeader." + AccTransactionHeaderSchema.Constants.AH_Ledger + @" != PrimaryHeader." + AccTransactionHeaderSchema.Constants.AH_Ledger + @"
		AND (SecondaryHeader." + AccTransactionHeaderSchema.Constants.AH_Ledger + @" = @AR OR SecondaryHeader." + AccTransactionHeaderSchema.Constants.AH_Ledger + @" = @AP) 
		AND (SecondaryHeader." + AccTransactionHeaderSchema.Constants.AH_TransactionType + @" = @INV OR SecondaryHeader." + AccTransactionHeaderSchema.Constants.AH_TransactionType + @" = @CRD) 
{2}", prefix, additionalFields, suffix);
		}

		#endregion

		#endregion

		#region Financial Filters

		protected virtual void AddFinancialFilters(ModuleFilterCollection filters)
		{
			FilterCategory financialFiltersCategory = new FilterCategory(ResString.GetMultilingualString("Accounting|TransactionFilter|FinancialDetails", "Financial Details"));

			ModuleNkFilter currencyFilter = filters.AddNkFilter("Currency", AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, ModuleIDs.RefCurrency, AH_RXList);
			currencyFilter.Category = financialFiltersCategory;
			currencyFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|Currency", "Currency");

			ModuleNumberRangeFilter transactionAmountFilter = filters.AddNumberRangeFilter("Transaction Amount", AccTransactionHeaderSchema.AH_OSTotal);
			transactionAmountFilter.Category = financialFiltersCategory;
			transactionAmountFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|TransactionAmount", "Transaction Amount");
		}

		#endregion

		#region Other Filters

		protected virtual void AddOtherFilters(ModuleFilterCollection filters)
		{
			ModuleGuidFilter bankAccountFilter = filters.AddGuidFilter("Bank Account", ModuleIDs.AccBankAccount, AccTransactionHeaderSchema.AH_AB, BankAccounts);
			bankAccountFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|BankAccount", "Bank Account");

			if (IsPayableModule)
			{
				ModuleTextFilter agreedPaymentMethodOrganisationFilter = filters.AddTextFilter("Agreed Payment Method", AccTransactionHeaderSchema.AH_AgreedPaymentMethodOverride, Env.Registry.PayablesCreditAgreedPaymentMethodsList);
				agreedPaymentMethodOrganisationFilter.Category = FilterCategories.Other;
				agreedPaymentMethodOrganisationFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|PayablesAgreedPaymentMethod", "Agreed Payment Method");
			}
			else
			{
				ModuleTextFilter agreedPaymentMethodOrganisationFilter = filters.AddTextFilter("Agreed Payment Method", AccTransactionHeaderSchema.AH_AgreedPaymentMethodOverride, OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value.GetCodeDescriptionPairList());
				agreedPaymentMethodOrganisationFilter.Category = FilterCategories.Other;
				agreedPaymentMethodOrganisationFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|ReceivablesAgreedPaymentMethod", "Agreed Payment Method");
			}

			if (ShouldAddComplianceDocumentRecordFilter)
			{
				ModuleTextFilter complianceDocumentFilter = filters.AddTextFilter("Compliance Document Record", GetComplianceDocumentRecordQuery, ComplianceDocumentRecordTypeList);
				complianceDocumentFilter.Category = FilterCategories.Other;
				complianceDocumentFilter.MaxLength = 3;
				complianceDocumentFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|ComplianceDocumentRecord", "Compliance Document Record");
			}
		}

		protected virtual ZQuery GetComplianceDocumentRecordQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			var subQueryForNotCommentLine = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK);
			subQueryForNotCommentLine.AddToFilter(AccTransactionLinesSchema.AL_AC, SQLComparisonOperator.Equal, null);

			var subQueryForCommentCharge = new ZDBOnlySubQuery(typeof(AccChargeCode), AccChargeCodeSchema.PK, true);
			subQueryForCommentCharge.AddToFilter(AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, Constants.ChargeType.Comment);
			subQueryForNotCommentLine.AddSubQuery(AccTransactionLinesSchema.AL_AC, subQueryForCommentCharge, JoinCondition.Or);
			if (value == "CRE")
			{
				var subQuery1 = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH, true);
				var subQuery2 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentPivot), AccComplianceDocumentPivotSchema.ADP_AL, true);
				subQuery1.AddSubQuery(AccTransactionLinesSchema.PK, subQuery2, JoinCondition.And);
				subQuery1.AddToFilter(AccTransactionLinesSchema.AL_AH, SQLComparisonOperator.NotEqual, null);
				subQuery1.AddSubQuery(AccTransactionLinesSchema.PK, subQueryForNotCommentLine, JoinCondition.And);
				result.AddSubQuery(subQuery1, JoinCondition.And);

				var subQuery3 = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
				subQuery3.AddSubQuery(AccTransactionLinesSchema.PK, subQueryForNotCommentLine, JoinCondition.And);
				result.AddSubQuery(subQuery3, JoinCondition.And);
			}
			else if (value == "NCR")
			{
				var subQuery1 = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH, true);
				var subQuery2 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentPivot), AccComplianceDocumentPivotSchema.ADP_AL);
				subQuery1.AddSubQuery(AccTransactionLinesSchema.PK, subQuery2, JoinCondition.And);
				subQuery1.AddToFilter(AccTransactionLinesSchema.AL_AH, SQLComparisonOperator.NotEqual, null);
				subQuery1.AddSubQuery(AccTransactionLinesSchema.PK, subQueryForNotCommentLine, JoinCondition.And);
				result.AddSubQuery(subQuery1, JoinCondition.And);

				var subQuery3 = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
				subQuery3.AddSubQuery(AccTransactionLinesSchema.PK, subQueryForNotCommentLine, JoinCondition.And);
				result.AddSubQuery(subQuery3, JoinCondition.And);
			}
			else if (value == "PCR")
			{
				var subQuery1 = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
				var subQuery2 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentPivot), AccComplianceDocumentPivotSchema.ADP_AL);
				var queryForNoComplianceDocumentData = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
				var subQueryForNoComplianceDocumentData = new ZDBOnlySubQuery(typeof(AccComplianceDocumentPivot), AccComplianceDocumentPivotSchema.ADP_AL, true);
				queryForNoComplianceDocumentData.AddSubQuery(AccTransactionLinesSchema.PK, subQueryForNotCommentLine, JoinCondition.And);
				queryForNoComplianceDocumentData.AddSubQuery(AccTransactionLinesSchema.PK, subQueryForNoComplianceDocumentData, JoinCondition.And);
				subQuery1.AddSubQuery(AccTransactionLinesSchema.PK, subQuery2, JoinCondition.And);
				subQuery1.AddSubQuery(AccTransactionLinesSchema.AL_AH, queryForNoComplianceDocumentData, JoinCondition.And);
				subQuery1.AddSubQuery(AccTransactionLinesSchema.PK, subQueryForNotCommentLine, JoinCondition.And);
				result.AddSubQuery(subQuery1, JoinCondition.And);

				var subQuery3 = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
				subQuery3.AddSubQuery(AccTransactionLinesSchema.PK, subQueryForNotCommentLine, JoinCondition.And);
				result.AddSubQuery(subQuery3, JoinCondition.And);
			}
			else
			{
				result.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
				result.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote);
			}
			return result;
		}

		AccBankAccountCollection bankAccounts;
		AccBankAccountCollection BankAccounts
		{
			get
			{
				if (bankAccounts == null)
				{
					bankAccounts = new AccBankAccountCollection(Factory, GlbCompany.CurrentCompany);
				}
				return bankAccounts;
			}
		}

		#endregion

		#region ModuleFilter Lists

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion

		#endregion

		#region Lookups

		#region AH_OHList

		OrgHeaderCollection fAH_OHList;
		OrgHeaderCollection AH_OHList
		{
			get { return fAH_OHList ?? (fAH_OHList = new OrgHeaderCollection(new BusinessObjectFactory(), GetAH_OHListFilter(UseDebtor))); }
		}

		OrgHeaderCollection fAH_OHListForRelatedTransactions;
		OrgHeaderCollection AH_OHListForRelatedTransactions
		{
			get { return fAH_OHListForRelatedTransactions ?? (fAH_OHListForRelatedTransactions = new OrgHeaderCollection(new BusinessObjectFactory(), GetAH_OHListFilter(UseCreditor))); }
		}

		protected virtual ZQuery GetAH_OHListFilter(bool isDebtor)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			subQuery.AddToFilter(isDebtor ? OrgCompanyDataSchema.OB_IsDebtor : OrgCompanyDataSchema.OB_IsCreditor, true);
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region CreditorDebtorGroupCollection

		public BusinessObjectCollection CreditorDebtorGroupCollection
		{
			get
			{
				if (fCreditorDebtorGroupCollection == null)
				{
					if (UseCreditor)
					{
						fCreditorDebtorGroupCollection = new OrgCreditorGroupCollection(Factory);
					}
					else
					{
						fCreditorDebtorGroupCollection = new OrgDebtorGroupCollection(Factory);
					}
				}
				return fCreditorDebtorGroupCollection;
			}
		}
		protected BusinessObjectCollection fCreditorDebtorGroupCollection;

		#endregion

		#region OrgSettlementGroup_List

		public OrganisationsFindBoxCollection OrgSettlementGroup_List
		{
			get { return orgSettlementGroup_List ?? (orgSettlementGroup_List = new OrganisationsFindBoxCollection(Factory)); }
		}
		OrganisationsFindBoxCollection orgSettlementGroup_List;

		#endregion

		#region TransactionTypeList

		protected virtual CodeDescriptionPairList TransactionTypeList => AHTransactionTypeList;

		#endregion

		#region ComplianceDocumentRecordTypeList

		CodeDescriptionPairList fComplianceDocumentRecordTypeList;

		protected virtual CodeDescriptionPairList ComplianceDocumentRecordTypeList
		{
			get
			{
				if (fComplianceDocumentRecordTypeList == null)
				{
					fComplianceDocumentRecordTypeList = new CodeDescriptionPairList();
					fComplianceDocumentRecordTypeList.AddPair("ALL", Res.GetString("Accounting|TransactionFilter|AllTransactions", "All Transactions"));
					fComplianceDocumentRecordTypeList.AddPair("CRE", Res.GetString("Accounting|TransactionFilter|TransactionsCreated", "Compliance document record(s) has/have been created for all transaction lines"));
					fComplianceDocumentRecordTypeList.AddPair("NCR", Res.GetString("Accounting|TransactionFilter|TransactionsNotCreated", "Compliance document record(s) has/have not been created for all transaction lines"));
					fComplianceDocumentRecordTypeList.AddPair("PCR", Res.GetString("Accounting|TransactionFilter|TransactionsPartCreated", "Compliance document record(s) has/have been created for some of the transaction lines"));
				}
				return fComplianceDocumentRecordTypeList;
			}
		}

		#endregion

		#region AH_RXList

		protected RefCurrencyCollection fAH_RXList;
		public RefCurrencyCollection AH_RXList
		{
			get
			{
				if (fAH_RXList == null)
				{
					fAH_RXList = new RefCurrencyCollection(Factory);
				}
				return fAH_RXList;
			}
		}

		#endregion

		#region ComplianceSubType List

		protected CodeDescriptionPairList fComplianceSubTypeList;
		public CodeDescriptionPairList ComplianceSubTypeList
		{
			get
			{
				if (fComplianceSubTypeList == null)
				{
					fComplianceSubTypeList = AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				}
				return fComplianceSubTypeList;
			}
		}

		#endregion

		#region PaymentStatusList

		protected CodeDescriptionPairList fPaymentStatusList;
		public CodeDescriptionPairList PaymentStatusList
		{
			get
			{
				if (fPaymentStatusList == null)
				{
					fPaymentStatusList = new CodeDescriptionPairList();
					fPaymentStatusList.AddPair(Business.AccountingUtils.PaymentStatusTypes.All, DisplayAllTransactionsDescription);
					fPaymentStatusList.AddPair(Business.AccountingUtils.PaymentStatusTypes.Unpaid, Res.GetString("Accounting|TransactionFilter|DisplayUnpaidTransactions", "Display unpaid transactions"));
					fPaymentStatusList.AddPair(Business.AccountingUtils.PaymentStatusTypes.Paid, Res.GetString("Accounting|TransactionFilter|DisplayPaidTransactions", "Display fully paid transactions"));
					fPaymentStatusList.AddPair(Business.AccountingUtils.PaymentStatusTypes.PartPaid, Res.GetString("Accounting|TransactionFilter|DisplayPartPaidTransactions", "Display partially paid transactions"));
				}
				return fPaymentStatusList;
			}
		}

		string DisplayAllTransactionsDescription
		{
			get { return Res.GetString("Accounting|TransactionFilter|DisplayAllTransactions", "Display all transactions"); }
		}

		#endregion

		#region BranchList

		protected GlbBranchCollection fBranchList;
		public virtual GlbBranchCollection BranchList
		{
			get
			{
				if (fBranchList == null)
				{
					ZQuery filter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
					fBranchList = new GlbBranchCollection(Factory, filter);
				}
				return fBranchList;
			}
		}

		#endregion

		#region AH_GEList

		protected GlbDepartmentCollection fAH_GEList;
		public GlbDepartmentCollection AH_GEList
		{
			get
			{
				if (fAH_GEList == null)
				{
					fAH_GEList = new GlbDepartmentCollection(Factory);
				}
				return fAH_GEList;
			}
		}

		#endregion

		#region Printed List

		CodeDescriptionPairList PrintedList
		{
			get
			{
				if (fPrintedList == null)
				{
					fPrintedList = new CodeDescriptionPairList();

					fPrintedList.AddPair("ALL", DisplayAllTransactionsDescription);
					fPrintedList.AddPair("PRN", Res.GetString("Accounting|TransactionFilter|DisplayPrintedTransactions", "Display printed transactions"));
					fPrintedList.AddPair("NPR", Res.GetString("Accounting|TransactionFilter|DisplayNotPrintedTransactions", "Display not printed transactions"));
				}

				return fPrintedList;
			}
		}
		CodeDescriptionPairList fPrintedList;

		#endregion

		#region Collection Batch Status List

		CodeDescriptionPairList CollectionBatchStatusList
		{
			get
			{
				if (fCollectionBatchStatusList == null)
				{
					fCollectionBatchStatusList = new CodeDescriptionPairList();

					fCollectionBatchStatusList.AddPair(Business.AccountingUtils.CollectionBatchStatusTypes.All, DisplayAllTransactionsDescription);
					fCollectionBatchStatusList.AddPair(Business.AccountingUtils.CollectionBatchStatusTypes.IncludeInActiveBatch, Res.GetString("Accounting|TransactionFilter|InCollectionBatch", "Display transactions included in active collection batch"));
					fCollectionBatchStatusList.AddPair(Business.AccountingUtils.CollectionBatchStatusTypes.NotIncludeInActiveBatch, Res.GetString("Accounting|TransactionFilter|NotInCollectionBatch", "Display transactions not included in active collection batch"));
				}

				return fCollectionBatchStatusList;
			}
		}
		CodeDescriptionPairList fCollectionBatchStatusList;

		#endregion

		#region RelatedTransactionsNotPaidFilterOptionsList

		CodeDescriptionPairList fRelatedTransactionsNotPaidFilterOptionsList;
		public CodeDescriptionPairList RelatedTransactionsNotPaidFilterOptionsList
		{
			get
			{
				if (fRelatedTransactionsNotPaidFilterOptionsList == null)
				{
					fRelatedTransactionsNotPaidFilterOptionsList = new CodeDescriptionPairList();
					if (IsPayableModule)
					{
						fRelatedTransactionsNotPaidFilterOptionsList.AddPair(RelatedARTransactionsFullyPaid, Res.GetString("Accounting|TransactionFilter|RelatedARTransactionsFullyPaid", "Related AR fully paid"));
						fRelatedTransactionsNotPaidFilterOptionsList.AddPair(RelatedARTransactionsNotFullyPaid, Res.GetString("Accounting|TransactionFilter|RelatedARTransactionsNotFullyPaid", "Related AR outstanding"));
						fRelatedTransactionsNotPaidFilterOptionsList.AddPair(RelatedARDSBTransactionsFullyPaid, Res.GetString("Accounting|TransactionFilter|RelatedARDSBTransactionsFullyPaid", "Related Disbursement AR paid"));
						fRelatedTransactionsNotPaidFilterOptionsList.AddPair(RelatedARDSBTransactionsNotFullyPaid, Res.GetString("Accounting|TransactionFilter|RelatedARDSBTransactionsNotFullyPaid", "Related Disbursement AR outstanding"));
					}
					else
					{
						fRelatedTransactionsNotPaidFilterOptionsList.AddPair(RelatedAPTransactionsFullyPaid, Res.GetString("Accounting|TransactionFilter|RelatedAPTransactionsFullyPaid", "Related AP fully paid"));
						fRelatedTransactionsNotPaidFilterOptionsList.AddPair(RelatedAPTransactionsNotFullyPaid, Res.GetString("Accounting|TransactionFilter|RelatedAPTransactionsNotFullyPaid", "Related AP is outstanding"));
					}
				}
				return fRelatedTransactionsNotPaidFilterOptionsList;
			}
		}

		#endregion

		#endregion

		protected ITransactionFilterStripControlPresentationProvider TransactionFilterStripControlPresentationProvider => transactionFilterStripControlPresentationProvider ?? (transactionFilterStripControlPresentationProvider = ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetTransactionFilterStripControlPresentationProvider());
		ITransactionFilterStripControlPresentationProvider transactionFilterStripControlPresentationProvider;
	}
}

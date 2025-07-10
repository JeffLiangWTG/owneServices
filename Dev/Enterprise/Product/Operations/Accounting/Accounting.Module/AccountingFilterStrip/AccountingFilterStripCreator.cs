using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public partial class AccountingFilterStripCreator : FilterStripBusinessObject, IAccountingFilterStrip
	{
		public AccountingFilterStripCreator(IAccountingFilterStripHolder filterStrip) : base(filterStrip.Factory)
		{
			this.FilterStrip = filterStrip;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new AccountingFilterStripCreator(FilterStrip);

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			return new ModuleFilterCollection();
		}

		readonly IAccountingFilterStripHolder FilterStrip;

		public void Initialize(bool addRevenueFilters = true, bool addWIPAccrualHasFilters = true, bool addSupplierCostReferenceFilters = true,
			bool addOrganisationFilters = true, bool addDateFilters = true, bool addAmountFilters = true, bool addNumbersAndReferencesFilters = true, bool addProfitLossReasonFilters = false)
		{
			needToAddRevenueFilters = addRevenueFilters;
			needToAddWIPAccrualHasFilters = addWIPAccrualHasFilters;
			needToAddSupplierCostReferenceFilters = addSupplierCostReferenceFilters;
			needToAddOrganisationFilters = addOrganisationFilters;
			needToAddDateFilters = addDateFilters;
			needToAddAmountFilters = addAmountFilters;
			needToAddNumbersAndReferencesFilters = addNumbersAndReferencesFilters;
			needToAddProfitLossReasonFilters = addProfitLossReasonFilters;
		}

		bool needToAddWIPAccrualHasFilters = true;
		bool needToAddSupplierCostReferenceFilters = true;
		bool needToAddOrganisationFilters = true;
		bool needToAddRevenueFilters = true;
		bool needToAddDateFilters = true;
		bool needToAddAmountFilters = true;
		bool needToAddNumbersAndReferencesFilters = true;
		bool needToAddProfitLossReasonFilters;

		public void AddBillingFilters(object moduleFilterCollection)
		{
			var filters = (ModuleFilterCollection)moduleFilterCollection;
	
			var apInvoiceNumberFilter = filters.AddNumberFilter(APInvoiceNumber, GetAPInvoiceNumberQuery);
			apInvoiceNumberFilter.Category = BillingCategory;
			apInvoiceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccountingFilter|APInvoiceNumber", "AP Invoice #");
			apInvoiceNumberFilter.MaxLength = AccTransactionHeaderSchema.AH_TransactionNum.MaxLength;
			HideFiltersOnWeb(apInvoiceNumberFilter);

			AddChargesWithDebtorFilter(filters);

			AddChargesWithCreditorFilter(filters);

			if (needToAddRevenueFilters)
			{
				var arTransactionNumberFilter = filters.AddFountainFilter(ARTransactionNumber, GetARTransactionNumber, "");
				arTransactionNumberFilter.Category = BillingCategory;
				arTransactionNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccountingFilter|ARTransactionNumber", "AR Transaction #");
				arTransactionNumberFilter.MaxLength = AccTransactionHeaderSchema.AH_TransactionNum.MaxLength;
				arTransactionNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
				arTransactionNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
				HideFiltersOnWeb(arTransactionNumberFilter);
			}

			if (needToAddSupplierCostReferenceFilters)
			{
				var supplierCostReferenceFilter = filters.AddNumberFilter(SupplierCostReference, GetSupplierCostReferenceQuery);
				supplierCostReferenceFilter.Category = BillingCategory;
				supplierCostReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccountingFilter|SupplierCostReference", "Supplier Cost Reference");
				supplierCostReferenceFilter.MaxLength = JobChargeSchema.JR_CostReference.MaxLength;
				HideFiltersOnWeb(supplierCostReferenceFilter);
			}
		}

		public CodeDescriptionPairList ProfitLossReasonList => AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.Value.GetCodeDescriptionPairList();

		protected void ValidateProfitLossReason(ZPropertyInfo info)
		{
			if (ProfitLossReasonList.Count == 0)
			{
				info.AddWarning(Res.GetString("Accounting|JobManagementFilter|ProfitLossReasonValidationWarning", "{0} registry item ({1}) is empty. Please set correct values.", AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.Caption, AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.Category));
			}
		}

		ZQuery GetProfitLossReasonQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var jobHeaderQuery = GetBaseJobSubQuery();
			jobHeaderQuery.AddToFilter_PossiblyCommaSeparated(JobHeaderSchema.JH_ProfitLossReasonCode, comparisonOperator, value);

			return TopLevelBusinessObjectQuery(jobHeaderQuery);
		}

		void AddChargesWithDebtorFilter(object moduleFilterCollection)
		{
			var filters = (ModuleFilterCollection)moduleFilterCollection;
			var filter = filters.AddGuidFilter(ChargesWithDebtor, ModuleIDs.Organisation, JobChargeSchema.JR_OH_SellAccount, FindboxLookupCollections.GetDebtorCollection(Factory));

			filter.Category = BillingCategory;
			filter.SubGroup = ChargeSubGroup;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccountingFilter|ChargesWithDebtor", "Charges with Debtor");
			HideFiltersOnWeb(filter);
		}

		void AddChargesWithCreditorFilter(object moduleFilterCollection)
		{
			var filters = (ModuleFilterCollection)moduleFilterCollection;
			var filter = filters.AddGuidFilter(ChargesWithCreditor, ModuleIDs.Organisation, JobChargeSchema.JR_OH_CostAccount, FindboxLookupCollections.GetCreditorCollection(Factory));

			filter.Category = BillingCategory;
			filter.SubGroup = ChargeSubGroup;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccountingFilter|ChargesWithCreditor", "Charges with Creditor");
			HideFiltersOnWeb(filter);
		}

		ModuleFilterSubGroup ChargeSubGroup => chargeSubGroup ?? (chargeSubGroup = new ChargeFilterSubGroup(this));
		ModuleFilterSubGroup chargeSubGroup;

		class ChargeFilterSubGroup : ModuleFilterSubGroup
		{
			AccountingFilterStripCreator Creator { get; }

			public ChargeFilterSubGroup(AccountingFilterStripCreator creator)
			{
				this.Creator = creator;
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var jobHeaderQuery = Creator.GetBaseJobSubQuery();
				var chargesSubQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_JH);
				chargesSubQuery.AddToFilter(filter);
				jobHeaderQuery.AddSubQuery(chargesSubQuery, JoinCondition.And);
				
				return Creator.TopLevelBusinessObjectQuery(jobHeaderQuery);
			}
		}

		public void AddJobManagementFilters(object moduleFilterCollection, SecurityCheckpoint jobManagementSecurity)
		{
			if (jobManagementSecurity != null && jobManagementSecurity.IsAllowed)
			{
				var filters = (ModuleFilterCollection)moduleFilterCollection;
				if (needToAddOrganisationFilters)
				{
					AddOrganisationFilters(filters);
				}

				if (needToAddDateFilters)
				{
					AddDateFilters(filters);
				}

				if (needToAddAmountFilters)
				{
					AddAmountFilters(filters);
				}

				if (needToAddNumbersAndReferencesFilters)
				{
					AddNumbersAndReferencesFilters(filters);
				}

				if (needToAddProfitLossReasonFilters)
				{
					AddProfitLossReasonFilter(filters);
				}

				AddInvoicingJobStatusFilter(filters);
				AddInvoicedChargesFilter(filters);
			}
		}

		void AddInvoicingJobStatusFilter(ModuleFilterCollection filters)
		{
			var invoicingJobStatusFilterName = InvoicingJobStatusFilterName;
			var jobStatusList = new JobHeaderStatusList();
			jobStatusList.AddPair(jobHeaderStatusOpenCode, Res.GetString("CDB6198F-FFDA-4E1A-A8B0-7F47A4E81343", "Open"));
			var invoicingJobStatusFilter = filters.AddTextFilter(((ResourceString)invoicingJobStatusFilterName).EnglishText, GetInvoicingJobStatusQuery, jobStatusList);
			invoicingJobStatusFilter.MaxLength = JobHeaderSchema.JH_Status.MaxLength;
			invoicingJobStatusFilter.Category = FilterCategories.StatusAndFlags;
			invoicingJobStatusFilter.MultilingualDescription = invoicingJobStatusFilterName;
			invoicingJobStatusFilter.ComparisonOperator_List.Clear();
			invoicingJobStatusFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			invoicingJobStatusFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			HideFiltersOnWeb(invoicingJobStatusFilter);
		}

		void AddProfitLossReasonFilter(ModuleFilterCollection filters)
		{
			ModuleTextFilter profitLossReasonFilter = filters.AddTextFilter("Profit/Loss Reason", GetProfitLossReasonQuery, ProfitLossReasonList);
			profitLossReasonFilter.Category = FilterCategories.StatusAndFlags;
			profitLossReasonFilter.PropertyValidation = ValidateProfitLossReason;
			profitLossReasonFilter.Validation.ValidateProperty();
			profitLossReasonFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|ProfitLossReason", "Profit/Loss Reason");
			HideFiltersOnWeb(profitLossReasonFilter);
		}

		void AddInvoicedChargesFilter(ModuleFilterCollection filters)
		{
			if (HasBusinessObjectTypeConfiguration)
			{
				var flagNames = new List<string>();
				var flagQueries = new List<GetFlagsQuery>();
				var options = InvoicedChargesFilterOptionsSelected;
				var index = 0;

				if (options.HasFlag(InvoicedChargesFilterOptions.NotInvoicedOnly) || options.HasFlag(InvoicedChargesFilterOptions.Default))
				{
					flagNames.Insert(index, Res.GetString("Accounting|AccountingFilter|NotInvoicedOnly", "Not Invoiced"));
					flagQueries.Insert(index, GetNotInvoicedQuery);
					index++;
				}

				if (options.HasFlag(InvoicedChargesFilterOptions.NotInvoicedImmediate) || options.HasFlag(InvoicedChargesFilterOptions.Default))
				{
					flagNames.Insert(index, Res.GetString("Accounting|AccountingFilter|NotInvoicedImmediate", "Not Invoiced Immediate"));
					flagQueries.Insert(index, GetNotInvoicedImmediateQuery);
					index++;
				}

				if (options.HasFlag(InvoicedChargesFilterOptions.NotInvoicedDeferred) || options.HasFlag(InvoicedChargesFilterOptions.Default))
				{
					flagNames.Insert(index, Res.GetString("Accounting|AccountingFilter|NotInvoicedDeferred", "Not Invoiced Deferred"));
					flagQueries.Insert(index, GetNotInvoicedDeferredQuery);
					index++;
				}

				if (options.HasFlag(InvoicedChargesFilterOptions.CostsNotPostedOnly) || options.HasFlag(InvoicedChargesFilterOptions.Default))
				{
					flagNames.Insert(index, Res.GetString("Accounting|AccountingFilter|CostsNotPostedOnly", "Costs Not Posted"));
					flagQueries.Insert(index, GetCostsNotPostedQuery);
					index++;
				}

				if (options.HasFlag(InvoicedChargesFilterOptions.NoChargesOnly) || options.HasFlag(InvoicedChargesFilterOptions.Default))
				{
					flagNames.Insert(index, Res.GetString("Accounting|AccountingFilter|NoChargesOnly", "No Charges"));
					flagQueries.Insert(index, GetNoChargesQuery);
					index++;
				}

				if (options.HasFlag(InvoicedChargesFilterOptions.NoCostsOnly) || options.HasFlag(InvoicedChargesFilterOptions.Default))
				{
					flagNames.Insert(index, Res.GetString("Accounting|AccountingFilter|NoCostsOnly", "No Costs"));
					flagQueries.Insert(index, GetNoCostsQuery);
					index++;
				}

				if (options.HasFlag(InvoicedChargesFilterOptions.LocalBillingNotPaid))
				{
					flagNames.Insert(index, Res.GetString("Accounting|AccountingFilter|LocalBillingNotPaid", "Local Billing Not Paid"));
					flagQueries.Insert(index, GetLocalBillingNotPaidQuery);
					index++;
				}

				var invoicedChargesFilterName = InvoicedChargesFilterName;
				var invoicedChargesFilter = filters.AddFlagsFilter(((ResourceString)invoicedChargesFilterName).EnglishText, flagNames.ToArray(), flagQueries.ToArray());
				invoicedChargesFilter.Category = FilterCategories.StatusAndFlags;
				invoicedChargesFilter.MultilingualDescription = invoicedChargesFilterName;
				HideFiltersOnWeb(invoicedChargesFilter);
			}
		}

		public void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddNumberFilter("Job Local Reference", GetLocalJobReferenceQuery);
			filter.MultilingualDescription = AppendFilterNameSuffixInOtherCategories(ResString.GetMultilingualString("b196e531-edce-4e1e-a33e-c034d8165d13", "Job Local Reference"));
			filter.MaxLength = JobHeaderSchema.JH_JobLocalReference.MaxLength;
		}

		public void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var gbFilter = filters.AddGuidFilter("Job Branch", ModuleIDs.GlbBranch, GetBranchQuery(), Branches, JobHeaderSchema.JH_GB);
			gbFilter.Category = FilterCategories.Organisations;
			gbFilter.MultilingualDescription = AppendFilterNameSuffixInOtherCategories(ResString.GetMultilingualString("76ea2ef5-d96d-4954-ba27-149375bcec7b", "Job Branch"));
			HideFiltersOnWeb(gbFilter);

			var geFilter = filters.AddGuidFilter("Job Department", ModuleIDs.GlbDepartment, GetDepartmentQuery(), Departments, JobHeaderSchema.JH_GE);
			geFilter.Category = FilterCategories.Organisations;
			geFilter.MultilingualDescription = AppendFilterNameSuffixInOtherCategories(ResString.GetMultilingualString("6490922c-bdc7-41d2-b7aa-06b9e3d86ec3", "Job Department"));
			HideFiltersOnWeb(geFilter);

			var opStaffFilter = filters.AddNkFilter("Job Operation Staff", GetOperationStaffQuery, ModuleIDs.GlbStaff, Staffs);
			opStaffFilter.Category = FilterCategories.Organisations;
			opStaffFilter.MultilingualDescription = AppendFilterNameSuffixInOtherCategories(ResString.GetMultilingualString("9d6d8ed3-f13e-4782-91eb-c2ce83d25e04", "Job Operation Staff"));
			HideFiltersOnWeb(opStaffFilter);

			var saleStaffFilter = filters.AddNkFilter("Job Sales Staff", GetSalesStaffQuery, ModuleIDs.GlbStaff, Staffs);
			saleStaffFilter.Category = FilterCategories.Organisations;
			saleStaffFilter.MultilingualDescription = AppendFilterNameSuffixInOtherCategories(ResString.GetMultilingualString("3cbe9d68-8b9c-4fd6-86f8-ed66ed516f38", "Job Sales Staff"));
			HideFiltersOnWeb(saleStaffFilter);

			var gbManagementCodeFilter = filters.AddTextFilter("Job Branch Management Code", GetBranchManagementCodeQuery, BranchManagementCodeList);
			gbManagementCodeFilter.Category = FilterCategories.Organisations;
			gbManagementCodeFilter.MultilingualDescription = AppendFilterNameSuffixInOtherCategories(ResString.GetMultilingualString("3a8b1aec-85b0-457b-831b-98f126613ff1", "Job Branch Management Code"));

			if (AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				var taxBranchFilter = filters.AddGuidFilter("Job Tax Branch", ModuleIDs.GlbBranch, GetTaxBranchQuery, Branches, JobHeaderSchema.JH_GB_TaxBranch);
				taxBranchFilter.Category = FilterCategories.Organisations;
				taxBranchFilter.MultilingualDescription = AppendFilterNameSuffixInOtherCategories(ResString.GetMultilingualString("c005368f-85c6-4782-ab24-453f026a120b", "Job Tax Branch"));
				HideFiltersOnWeb(taxBranchFilter);
			}
		}

		public void AddDateFilters(ModuleFilterCollection filters)
		{
			HideFiltersOnWeb(filters.AddDateFilter("Job Open or Close", GetAllDatesQuery)).MultilingualDescription = AppendFilterNameSuffixInOtherCategories(ResString.GetMultilingualString("cc4673b5-cd15-40cb-a566-4fe16be4e9e9", "Job Open or Close"));
			HideFiltersOnWeb(filters.AddDateFilter("Job Open", GetJobOpenQuery)).MultilingualDescription = AppendFilterNameSuffixInOtherCategories(ResString.GetMultilingualString("56e8bd63-3eac-4919-beaf-a2bb36749f23", "Job Open"));
			HideFiltersOnWeb(filters.AddDateFilter("Job Close", GetJobCloseQuery)).MultilingualDescription = AppendFilterNameSuffixInOtherCategories(ResString.GetMultilingualString("08b758e4-138b-4746-8a14-b3706331cef2", "Job Close"));
			AddRevenueRecognitionDateFilter(filters);
		}

		void AddRevenueRecognitionDateFilter(ModuleFilterCollection filters)
		{
			var revenueRecognitionDateFilter = new JobManagementModuleDateFilter("Job Revenue Recognition Date", GetRevenueRecognitionDateQuery);
			revenueRecognitionDateFilter.MultilingualDescription = AppendFilterNameSuffixInOtherCategories(ResString.GetMultilingualString("cf1ddcb5-bb08-45e4-96f9-f7a82db4cc86", "Job Revenue Recognition Date"));
			HideFiltersOnWeb(revenueRecognitionDateFilter);
			filters.AddFilter(revenueRecognitionDateFilter);
		}

		public void AddAmountFilters(ModuleFilterCollection filters)
		{
			var costFilter = new JobManagementAmountFilter("Job Cost Amount", GetJobCostQuery);
			costFilter.Category = FinancialDetailsCategory;
			costFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|JobCostAmount", "Job Cost Amount");
			HideFiltersOnWeb(costFilter);
			filters.AddCustomFilter(costFilter);

			var accrualFilter = new JobManagementAmountFilter("Job Accrual Amount", GetAccrualQuery);
			accrualFilter.Category = FinancialDetailsCategory;
			accrualFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|JobAccrualAmount", "Job Accrual Amount");
			HideFiltersOnWeb(accrualFilter);
			filters.AddCustomFilter(accrualFilter);

			if (needToAddRevenueFilters)
			{
				var profitFilter = new JobManagementAmountFilter("Job Profit Amount", GetJobProfitQuery);
				profitFilter.Category = FinancialDetailsCategory;
				profitFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|JobProfitAmount", "Job Profit Amount");
				HideFiltersOnWeb(profitFilter);
				filters.AddCustomFilter(profitFilter);

				var revenueFilter = new JobManagementAmountFilter("Job Revenue Amount", GetJobRevenueQuery);
				revenueFilter.Category = FinancialDetailsCategory;
				revenueFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|JobRevenueAmount", "Job Revenue Amount");
				HideFiltersOnWeb(revenueFilter);
				filters.AddCustomFilter(revenueFilter);

				var wipFilter = new JobManagementAmountFilter("Job WIP Amount", GetWIPQuery);
				wipFilter.Category = FinancialDetailsCategory;
				wipFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|JobWIPAmount", "Job WIP Amount");
				HideFiltersOnWeb(wipFilter);
				filters.AddCustomFilter(wipFilter);

				var wipFilterExcludingDeferredCharges = new JobManagementAmountFilter("Job WIP Amount (Excluding Deferred Charges)", GetWIPQueryExcludingDeferredCharges);
				wipFilterExcludingDeferredCharges.Category = FinancialDetailsCategory;
				wipFilterExcludingDeferredCharges.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|JobWIPAmountExcludingDeferredCharges", "Job WIP Amount (Excluding Deferred Charges)");
				HideFiltersOnWeb(wipFilterExcludingDeferredCharges);
				filters.AddCustomFilter(wipFilterExcludingDeferredCharges);

				var wipFilterDeferredChargesOnly = new JobManagementAmountFilter("Job WIP Amount (Deferred Charges Only)", GetWIPQueryDeferredChargesOnly);
				wipFilterDeferredChargesOnly.Category = FinancialDetailsCategory;
				wipFilterDeferredChargesOnly.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|JobWIPAmountDeferredChargesOnly", "Job WIP Amount (Deferred Charges Only)");
				HideFiltersOnWeb(wipFilterDeferredChargesOnly);
				filters.AddCustomFilter(wipFilterDeferredChargesOnly);

				var marginFilter = new JobManagementAmountFilter("Job Margin %", GetMarginQuery);
				marginFilter.Category = FinancialDetailsCategory;
				marginFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|MarginPercent", "Job Margin %");
				HideFiltersOnWeb(marginFilter);
				filters.AddCustomFilter(marginFilter);
			}

			if (needToAddWIPAccrualHasFilters)
			{
				var hasAccrualFilter = filters.AddFlagsFilter("Jobs with outstanding Accruals", new string[] { Res.GetString("96f5819f-f59f-47ca-a690-f159846e5429", "Only Find jobs with at least one outstanding accrual") }, new GetFlagsQuery[] { GetHasAccrualQuery });
				hasAccrualFilter.Category = FinancialDetailsCategory;
				hasAccrualFilter.MultilingualDescription = ResString.GetMultilingualString("65439cec-0f00-4a02-a6aa-b6ea55ed7025", "Jobs with outstanding Accruals");

				var noAccrualFilter = filters.AddFlagsFilter("Jobs without any outstanding Accrual", new string[] { Res.GetString("3409f72a-510f-4d6c-b161-cd85dc233fd7", "Only Find jobs with no outstanding accruals") }, new GetFlagsQuery[] { GetHasNoAccrualQuery });
				noAccrualFilter.Category = FinancialDetailsCategory;
				noAccrualFilter.MultilingualDescription = ResString.GetMultilingualString("b5876e08-dd9c-4e18-8e01-2b8d197671df", "Jobs without any outstanding Accrual");

				var hasWipFilter = filters.AddFlagsFilter("Jobs with outstanding WIPs", new string[] { Res.GetString("4d58389a-b3ef-4585-bf8b-c11d4fe4be08", "Only Find jobs with at least one outstanding WIP") }, new GetFlagsQuery[] { GetHasWIPQuery });
				hasWipFilter.Category = FinancialDetailsCategory;
				hasWipFilter.MultilingualDescription = ResString.GetMultilingualString("3f60155a-7e1e-4771-bf30-49bb4a71d815", "Jobs with outstanding WIPs");

				var noWIPFilter = filters.AddFlagsFilter("Jobs without any outstanding WIP", new string[] { Res.GetString("f503579b-2111-47bd-b258-1d07c9783eeb", "Only Find jobs with no outstanding WIPs") }, new GetFlagsQuery[] { GetHasNoWIPQuery });
				noWIPFilter.Category = FinancialDetailsCategory;
				noWIPFilter.MultilingualDescription = ResString.GetMultilingualString("4183e9ed-08bb-42fb-a6e9-eeaaf1bb46ae", "Jobs without any outstanding WIP");
			}
		}

		ModuleFilter HideFiltersOnWeb(ModuleFilter filter)
		{
			filter.IsPublishedOnWeb = false;
			return filter;
		}

		bool IsFilterStripForParentTable
		{
			get { return FilterStrip.IsFilterStripForParentTable; }
		}

		ZDBOnlyQuery GetBaseJobSubQuery()
		{
			ZDBOnlyQuery jobHeaderQuery;
			if (IsFilterStripForParentTable)
			{
				jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
				jobHeaderQuery.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_IsActive, true);
			}
			else
			{
				jobHeaderQuery = new ZDBOnlyQuery(typeof(JobHeader));
			}
			jobHeaderQuery.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			return jobHeaderQuery;
		}

		ZQuery TopLevelBusinessObjectQuery(ZDBOnlyQuery jobHeaderQuery)
		{
			if (IsFilterStripForParentTable)
			{
				return FilterStrip.TopLevelBusinessObjectQuery((ZDBOnlySubQuery)jobHeaderQuery);
			}
			else
			{
				return jobHeaderQuery;
			}
		}

		#region Billing Filters Delegates

		ZQuery GetAPInvoiceNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetTransactionNumberQuery(comparisonOperator, value, ZArchitecture.Core.LedgerTypes.AccountsPayable);
		}

		ZQuery GetARTransactionNumber(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetTransactionNumberQuery(comparisonOperator, value, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
		}

		ZQuery GetTransactionNumberQuery(SQLComparisonOperator comparisonOperator, ZString value, string ledgerType)
		{
			var transactionHeaderQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			var transactionLinesQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_JH);
			var jobHeaderQuery = GetBaseJobSubQuery();

			var transactionTypeQuery = new ZQuery();

			transactionTypeQuery.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, "INV");
			transactionTypeQuery.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, "CRD");

			transactionHeaderQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, AccTransactionHeaderSchema.AH_TransactionNum, comparisonOperator, value);
			transactionHeaderQuery.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_Ledger, ledgerType);
			transactionHeaderQuery.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			transactionHeaderQuery.AddToFilter(transactionTypeQuery, JoinCondition.And);
			transactionLinesQuery.AddSubQuery(AccTransactionLinesSchema.AL_AH, transactionHeaderQuery, JoinCondition.And);
			jobHeaderQuery.AddSubQuery(JobHeaderSchema.PK, transactionLinesQuery, JoinCondition.And);

			return TopLevelBusinessObjectQuery(jobHeaderQuery);
		}

		#endregion

		#region Numbers And References Filters Delegates

		ZQuery GetLocalJobReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var jobHeaderQuery = GetBaseJobSubQuery();
			jobHeaderQuery.AddToFilter_PossiblyCommaSeparated(JobHeaderSchema.JH_JobLocalReference, comparisonOperator, value);
			return TopLevelBusinessObjectQuery(jobHeaderQuery);
		}

		ZQuery GetSupplierCostReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var jobHeaderQuery = GetBaseJobSubQuery();
			var jobChargeQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_JH);
			jobChargeQuery.AddToFilter_PossiblyCommaSeparated(JobChargeSchema.JR_CostReference, comparisonOperator, value);
			jobHeaderQuery.AddSubQuery(jobChargeQuery, JoinCondition.And);
			return TopLevelBusinessObjectQuery(jobHeaderQuery);
		}

		ZQuery GetInvoicingJobStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (value.Length > JobHeaderSchema.JH_Status.MaxLength)
			{
				return new ZQuery { IsNoResultQuery = true };
			}

			if (value == jobHeaderStatusOpenCode)
			{
				comparisonOperator = comparisonOperator == SQLComparisonOperator.NotEqual ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
				value = JobHeaderStatus.Closed.Code;
			}

			if (DoesJobHeaderHaveSubParent)
			{
				var query = new ZDBOnlyQuery(JobHeaderBusinessObjectType);
				query.AddSubQuery((ZDBOnlySubQuery)GetBaseJobSubQuery().AddToFilter(JobHeaderSchema.JH_Status, comparisonOperator, value), JoinCondition.And);
				query.AddSubQuery(JobHeaderAdditionalKeyColumn, (ZDBOnlySubQuery)GetBaseJobSubQuery().AddToFilter(JobHeaderSchema.JH_Status, comparisonOperator, value), JoinCondition.Or);
				return query;
			}
			else if (HasCustomFilter)
			{
				var baseQuery = GetBaseJobSubQuery();
				baseQuery.AddToFilter(JobHeaderSchema.JH_Status, comparisonOperator, value);
				return InvoicingJobStatusFilterMakeCustomFilter(baseQuery);
			}
			else
			{
				var query = GetBaseJobSubQuery();
				query.AddToFilter(JobHeaderSchema.JH_Status, comparisonOperator, value);
				return TopLevelBusinessObjectQuery(query);
			}
		}

		ZQuery GetInvoicedChargesQuery(ZString invoicingSubQuery)
		{
			ZDBOnlyQuery jobHeaderQuery;
			jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);

			var invoicingSubQueryParams = new ZSqlParameterCollection();
			invoicingSubQueryParams.Add("@CurrentCompany", GlbCompany.CurrentCompany.PK, GlbBranchSchema.GB_GC);
			jobHeaderQuery.AddFilterAndZSQLParameterCollection(invoicingSubQuery, invoicingSubQueryParams);

			return TopLevelBusinessObjectQuery(jobHeaderQuery);
		}

		ZQuery GetNotInvoicedQuery(ZBool value) => value ? GetInvoicedChargesQuery(NotInvoicedQuery) : new ZQuery();
		ZQuery GetNotInvoicedImmediateQuery(ZBool value) => value ? GetInvoicedChargesQuery(NotInvoicedImmediateQuery) : new ZQuery();
		ZQuery GetNotInvoicedDeferredQuery(ZBool value) => value ? GetInvoicedChargesQuery(NotInvoicedDeferredQuery) : new ZQuery();
		ZQuery GetCostsNotPostedQuery(ZBool value) => value ? GetInvoicedChargesQuery(CostsNotPostedQuery) : new ZQuery();
		ZQuery GetNoChargesQuery(ZBool value) => value ? GetInvoicedChargesQuery(NoChargesQuery) : new ZQuery();
		ZQuery GetNoCostsQuery(ZBool value) => value ? GetInvoicedChargesQuery(NoCostsQuery) : new ZQuery();

		ZQuery GetLocalBillingNotPaidQuery(ZBool value)
		{
			var result = new ZQuery();
			if (value)
			{
				var query = GetBaseJobSubQuery();

				var transactionHeaderSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.AH_JH);
				var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), AccTransactionHeaderSchema.AH_OH);
				orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				var accTransactionHeaderFilter = new ZQuery();
				accTransactionHeaderFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice);
				accTransactionHeaderFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote);
				accTransactionHeaderFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.AdjustmentNote);

				transactionHeaderSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				transactionHeaderSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_OutstandingAmount, SQLComparisonOperator.NotEqual, 0);
				transactionHeaderSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				transactionHeaderSubQuery.AddToFilter(accTransactionHeaderFilter);

				transactionHeaderSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);
				query.AddSubQuery(transactionHeaderSubQuery, JoinCondition.And);

				result = TopLevelBusinessObjectQuery(query);
			}
			return result;
		}

		ZString NotInvoicedDeferredQuery => PKColumn.Name + @" IN (
	SELECT JH_ParentID
	FROM dbo.JobHeader
	LEFT JOIN dbo.AccTransactionLines ON AL_JH = JH_PK
		AND AL_LineType IN ('WIP','REV')
	LEFT JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK
	LEFT JOIN dbo.JobCharge ON JR_JH = JH_PK
	WHERE JH_GC = @CurrentCompany
		AND JR_InvoiceType IN ('DED','DCD','DBD','FID','CUD','FRD','ITD')
	GROUP BY JH_ParentID
	HAVING sum(
		CASE
			WHEN AL_LineType = 'WIP' AND AL_ReverseDate IS NULL THEN 1
			ELSE 0
		END) > 0
	OR sum(
		CASE
			WHEN AL_LineType = 'REV' AND (AH_IsCancelled = 0 OR AH_IsCancelled IS NULL) THEN 1
			ELSE 0
		END) = 0
)";

		ZString NotInvoicedImmediateQuery => PKColumn.Name + @" IN (
	SELECT JH_ParentID
	FROM dbo.JobHeader
	LEFT JOIN dbo.AccTransactionLines ON AL_JH = JH_PK
		AND AL_LineType IN ('WIP','REV')
	LEFT JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK
	LEFT JOIN dbo.JobCharge ON JR_JH = JH_PK
	WHERE JH_GC = @CurrentCompany
		AND (JR_InvoiceType IN ('DES','DCU','DBT','FIN','CUR','FRT','ITC','') OR JR_InvoiceType IS NULL)
	GROUP BY JH_ParentID
	HAVING sum(
		CASE
			WHEN AL_LineType = 'WIP' AND AL_ReverseDate IS NULL THEN 1
			ELSE 0
		END) > 0
	OR sum(
		CASE
			WHEN AL_LineType = 'REV' AND (AH_IsCancelled = 0 OR AH_IsCancelled IS NULL) THEN 1
			ELSE 0
		END) = 0
)";

		ZString NotInvoicedQuery => PKColumn.Name + @" IN (
	SELECT Z.JH_ParentID
	FROM 
		(SELECT JH_ParentID, NULL AS AL_LineType, NULL AS AL_JH, NULL AS AL_ReverseDate, NULL AS AH_IsCancelled
		FROM dbo.JobHeader
		WHERE JH_GC = @CurrentCompany
		UNION ALL
		SELECT JH_ParentID, AL_LineType, AL_JH, AL_ReverseDate, AH_IsCancelled FROM dbo.JobHeader
		LEFT JOIN dbo.AccTransactionLines ON AL_JH = JH_PK AND AL_JH IS NOT NULL AND AL_LineType IN ('WIP','REV')
		LEFT JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK
		WHERE JH_GC = @CurrentCompany
		) AS Z
	GROUP BY Z.JH_ParentID
	HAVING sum(
			CASE
				WHEN AL_LineType = 'WIP' AND AL_ReverseDate IS NULL THEN 1
				ELSE 0
			END) > 0
	  OR sum(
			CASE
				WHEN AL_LineType = 'REV' AND (AH_IsCancelled = 0 OR AH_IsCancelled IS NULL) THEN 1
				ELSE 0
			END) = 0
)";

		ZString CostsNotPostedQuery => PKColumn.Name + @" IN (
	SELECT JH_ParentID
	FROM dbo.JobHeader
	LEFT JOIN dbo.AccTransactionLines ON AL_JH = JH_PK 
		AND AL_LineType IN ('ACR','CST')
	LEFT JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK
	WHERE JH_GC = @CurrentCompany
	GROUP BY JH_ParentID
	HAVING sum(
		CASE
			 WHEN AL_LineType = 'ACR' AND AL_ReverseDate IS NULL THEN 1
			 ELSE 0
		END) > 0
	OR sum(
		CASE
			 WHEN AL_LineType = 'CST' AND (AH_IsCancelled = 0 OR AH_IsCancelled IS NULL) THEN 1
			 ELSE 0
		END) = 0
)";

		ZString NoChargesQuery => PKColumn.Name + @" IN (
	SELECT DISTINCT(JH_ParentID) 
	FROM dbo.JobHeader
	WHERE JH_GC = @CurrentCompany 
		AND NOT EXISTS (
			SELECT TOP 1 JR_PK 
			FROM dbo.JobCharge 
			WHERE JR_JH = JH_PK 
				AND JR_LocalSellAmt != 0
		)
)";

		ZString NoCostsQuery => PKColumn.Name + @" IN (
	SELECT DISTINCT(JH_ParentID) 
	FROM dbo.JobHeader
	WHERE JH_GC = @CurrentCompany 
		AND NOT EXISTS (
			SELECT TOP 1 JR_PK 
			FROM dbo.JobCharge 
			WHERE JR_JH = JH_PK 
				AND JR_LocalCostAmt != 0
		)
)";

		bool HasBusinessObjectTypeConfiguration => FilterStrip.AccountingFilterStripConfiguration != null && FilterStrip.AccountingFilterStripConfiguration.ContainsKey(AccountingFilterStripConfigurationKeys.BusinessObjectType);

		bool DoesJobHeaderHaveSubParent => FilterStrip.AccountingFilterStripConfiguration != null
			&& FilterStrip.AccountingFilterStripConfiguration.ContainsKey(AccountingFilterStripConfigurationKeys.JobHeaderBusinessObjectType)
			&& FilterStrip.AccountingFilterStripConfiguration.ContainsKey(AccountingFilterStripConfigurationKeys.JobHeaderAdditionalKeyColumn);

		bool HasCustomFilter => FilterStrip.AccountingFilterStripConfiguration
			?.ContainsKey(AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterMakeCustomFilter) ?? false;

		MultilingualString InvoicingJobStatusFilterName => FilterStrip.AccountingFilterStripConfiguration != null
			&& FilterStrip.AccountingFilterStripConfiguration.ContainsKey(AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride) ?
				(MultilingualString)FilterStrip.AccountingFilterStripConfiguration[AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride] :
				ResString.GetMultilingualString("Accounting|AccountingFilter|InvoicingJobStatus", "Invoicing Job Status");

		MultilingualString InvoicedChargesFilterName => FilterStrip.AccountingFilterStripConfiguration != null
			&& FilterStrip.AccountingFilterStripConfiguration.ContainsKey(AccountingFilterStripConfigurationKeys.InvoicedChargesFilterNameOverride) ?
				(MultilingualString)FilterStrip.AccountingFilterStripConfiguration[AccountingFilterStripConfigurationKeys.InvoicedChargesFilterNameOverride] :
				ResString.GetMultilingualString("Accounting|AccountingFilter|InvoicedCharges", "Invoiced / Charges");

		Type BusinessObjectType => (Type)FilterStrip.AccountingFilterStripConfiguration[AccountingFilterStripConfigurationKeys.BusinessObjectType];

		Type JobHeaderBusinessObjectType => (Type)FilterStrip.AccountingFilterStripConfiguration[AccountingFilterStripConfigurationKeys.JobHeaderBusinessObjectType];

		SchemaPKColumn PKColumn => ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(BusinessObjectFactory.GetTableNameFromType(BusinessObjectType));

		SchemaGuidColumn JobHeaderAdditionalKeyColumn => (SchemaGuidColumn)FilterStrip.AccountingFilterStripConfiguration[AccountingFilterStripConfigurationKeys.JobHeaderAdditionalKeyColumn];

		Func<ZDBOnlyQuery, ZDBOnlyQuery> InvoicingJobStatusFilterMakeCustomFilter =>
			(Func<ZDBOnlyQuery, ZDBOnlyQuery>)FilterStrip.AccountingFilterStripConfiguration[AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterMakeCustomFilter];

		readonly string jobHeaderStatusOpenCode = "OPN";

		InvoicedChargesFilterOptions InvoicedChargesFilterOptionsSelected
		{
			get
			{
				object result;
				if (!FilterStrip.AccountingFilterStripConfiguration.TryGetValue(AccountingFilterStripConfigurationKeys.InvoicedChargesFilterOptionsSelected, out result))
				{
					result = InvoicedChargesFilterOptions.Default;
				}
				return (InvoicedChargesFilterOptions)result;
			}
		}

		#endregion

		#region Organisation Filter Delegates

		#region GetBranchManagementCodeQuery

		ZQuery GetBranchManagementCodeQuery(ZString value)
		{
			var jobHeaderQuery = GetBaseJobSubQuery();

			if (!value.IsEmpty)
			{
				var branchManagementCodeQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
				branchManagementCodeQuery.AddToFilter(GlbBranchSchema.GB_AccountingGroupCode, value);
				jobHeaderQuery.AddSubQuery(JobHeaderSchema.JH_GB, branchManagementCodeQuery, JoinCondition.And);
			}

			return TopLevelBusinessObjectQuery(jobHeaderQuery);
		}

		#endregion

		GetGuidQueryWithOperatorSupportsFiltersMatch GetBranchQuery()
		{
			ZQuery result(ZDBOnlySubQuery filtersMatchQuery, SQLComparisonOperator comparisonOperator, object value)
			{
				var jobHeaderQuery = GetBaseJobSubQuery();
				if (filtersMatchQuery != null)
				{
					jobHeaderQuery.AddSubQuery(filtersMatchQuery, JoinCondition.And);
				}
				else if (comparisonOperator != SQLComparisonOperator.IsNotBlank)
				{
					jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GB, comparisonOperator, value);
				}
				return TopLevelBusinessObjectQuery(jobHeaderQuery);
			}
			return result;
		}

		ZQuery GetTaxBranchQuery(ZDBOnlySubQuery filtersMatchQuery, SQLComparisonOperator comparisonOperator, object value)
		{
			var jobHeaderQuery = GetBaseJobSubQuery();
			if (filtersMatchQuery != null)
			{
				jobHeaderQuery.AddSubQuery(filtersMatchQuery, JoinCondition.And);
			}
			else
			{
				jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GB_TaxBranch, comparisonOperator, value);
			}
			return TopLevelBusinessObjectQuery(jobHeaderQuery);
		}

		GetGuidQueryWithOperatorSupportsFiltersMatch GetDepartmentQuery()
		{
			ZQuery result(ZDBOnlySubQuery filtersMatchQuery, SQLComparisonOperator comparisonOperator, object value)
			{
				var jobHeaderQuery = GetBaseJobSubQuery();
				if (filtersMatchQuery != null)
				{
					jobHeaderQuery.AddSubQuery(filtersMatchQuery, JoinCondition.And);
				}
				else if (comparisonOperator != SQLComparisonOperator.IsNotBlank)
				{
					jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GE, comparisonOperator, value);
				}
				return TopLevelBusinessObjectQuery(jobHeaderQuery);
			}
			return result;
		}

		ZQuery GetOperationStaffQuery(ZDBOnlySubQuery filtersMatchQuery, SQLComparisonOperator comparisonOperator, ZString nK)
		{
			var jobHeaderQuery = GetBaseJobSubQuery();

			if (filtersMatchQuery != null)
			{
				jobHeaderQuery.AddSubQuery(JobHeaderSchema.JH_GS_NKRepOps, GlbStaffSchema.GS_Code, filtersMatchQuery, JoinCondition.And);
			}
			else
			{
				jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GS_NKRepOps, comparisonOperator, nK);
			}

			return TopLevelBusinessObjectQuery(jobHeaderQuery);
		}

		ZQuery GetSalesStaffQuery(ZDBOnlySubQuery filtersMatchQuery, SQLComparisonOperator comparisonOperator, ZString nK)
		{
			var jobHeaderQuery = GetBaseJobSubQuery();

			if (filtersMatchQuery != null)
			{
				jobHeaderQuery.AddSubQuery(JobHeaderSchema.JH_GS_NKRepSales, GlbStaffSchema.GS_Code, filtersMatchQuery, JoinCondition.And);
			}
			else
			{
				jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GS_NKRepSales, comparisonOperator, nK);
			}

			return TopLevelBusinessObjectQuery(jobHeaderQuery);
		}

		#endregion

		#region Date Filter Delegates

		ZQuery GetJobOpenQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var jobHeaderQuery = GetBaseJobSubQuery();
			AddDateTimeRange(jobHeaderQuery, comparisonOperator, JoinCondition.And, JobHeaderSchema.JH_A_JOP, fromDate, toDate);
			return TopLevelBusinessObjectQuery(jobHeaderQuery);
		}

		ZQuery GetJobCloseQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var jobHeaderQuery = GetBaseJobSubQuery();
			AddDateTimeRange(jobHeaderQuery, comparisonOperator, JoinCondition.And, JobHeaderSchema.JH_A_JCL, fromDate, toDate);
			return TopLevelBusinessObjectQuery(jobHeaderQuery);
		}

		ZQuery GetRevenueRecognitionDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var jobHeaderQuery = GetBaseJobSubQuery();
			var subQuery = new ZDBOnlySubQuery(typeof(JobChargeRevRecognition), JobChargeRevRecognitionSchema.D3_JH);
			AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, JobChargeRevRecognitionSchema.D3_RecognitionDate, fromDate, toDate);
			jobHeaderQuery.AddSubQuery(subQuery, JoinCondition.And);

			return TopLevelBusinessObjectQuery(jobHeaderQuery);
		}

		ZQuery GetAllDatesQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var jobHeaderQuery = GetBaseJobSubQuery();
			var jobOpenOrCloseQuery = new ZDBOnlyQuery(typeof(JobHeader));
			var jobOpenQuery = new ZDBOnlyQuery(typeof(JobHeader));
			var jobCloseQuery = new ZDBOnlyQuery(typeof(JobHeader));

			AddDateTimeRange(jobOpenQuery, comparisonOperator, JoinCondition.And, JobHeaderSchema.JH_A_JOP, fromDate, toDate);
			AddDateTimeRange(jobCloseQuery, comparisonOperator, JoinCondition.And, JobHeaderSchema.JH_A_JCL, fromDate, toDate);

			jobOpenOrCloseQuery.AddToFilter(jobOpenQuery);
			jobOpenOrCloseQuery.AddToFilter(jobCloseQuery, JoinCondition.Or);

			jobHeaderQuery.AddToFilter(jobOpenOrCloseQuery);

			return TopLevelBusinessObjectQuery(jobHeaderQuery);
		}

		#endregion

		#region Amount Filter Delegates

		ZQuery GetJobProfitQuery(SQLComparisonOperator comparisonOperator, ZDecimal amount)
		{
			var query = GetBaseJobSubQuery();

			var queryParams = new ZSqlParameterCollection();
			queryParams.Add("@WIP", TransactionLineTypes.WIP, AccTransactionLinesSchema.AL_LineType);
			queryParams.Add("@ACR", TransactionLineTypes.Accrual, AccTransactionLinesSchema.AL_LineType);
			queryParams.Add("@CST", TransactionLineTypes.Cost, AccTransactionLinesSchema.AL_LineType);
			queryParams.Add("@REV", TransactionLineTypes.Revenue, AccTransactionLinesSchema.AL_LineType);
			queryParams.Add("@ProfitAmount", amount, AccTransactionLinesSchema.AL_LineAmount);

			string queryText = @"(
		SELECT SUM(X.Amount)
		FROM 
		(
			SELECT CAST(AL_LineAmount AS DECIMAL(24,9)) AS Amount
			FROM dbo.AccTransactionLines  
			WHERE AL_JH = JH_PK AND AL_LineType IN ('CST', 'REV')

			UNION ALL

			SELECT CAST(-AL_LineAmount AS DECIMAL(24,9)) AS Amount
			FROM dbo.AccTransactionLines  
			WHERE AL_JH = JH_PK AND AL_ReverseDate IS NULL AND AL_LineType IN ('WIP', 'ACR')
		) AS X
		 )";

			if (amount == 0 && comparisonOperator == SQLComparisonOperator.Equal)
			{
				queryText = string.Format((NoResString)" IsNull({0}, 0) {1} {2}", queryText, comparisonOperator.ComparisonText(amount), " @ProfitAmount");
			}
			else
			{
				queryText = string.Format(" {0} {1} {2}", queryText, comparisonOperator.ComparisonText(amount), " @ProfitAmount");
			}

			query.AddFilterAndZSQLParameterCollection(queryText, queryParams);
			query.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			return TopLevelBusinessObjectQuery(query);
		}

		ZQuery GetJobRevenueQuery(SQLComparisonOperator comparisonOperator, ZDecimal amount)
		{
			var query = GetBaseJobSubQuery();
			AddAmountSubQueryForREVAndCST(query, TransactionLineTypes.Revenue, amount, comparisonOperator, "");
			return TopLevelBusinessObjectQuery(query);
		}

		ZQuery GetJobCostQuery(SQLComparisonOperator comparisonOperator, ZDecimal amount)
		{
			var query = GetBaseJobSubQuery();
			AddAmountSubQueryForREVAndCST(query, TransactionLineTypes.Cost, amount, comparisonOperator, "-");
			return TopLevelBusinessObjectQuery(query);
		}

		ZQuery GetWIPQuery(SQLComparisonOperator comparisonOperator, ZDecimal amount)
		{
			var query = GetBaseJobSubQuery();
			AddWIPSubQuery(query, amount, comparisonOperator);
			return TopLevelBusinessObjectQuery(query);
		}

		ZQuery GetWIPQueryExcludingDeferredCharges(SQLComparisonOperator comparisonOperator, ZDecimal amount)
		{
			var query = GetBaseJobSubQuery();
			AddWIPSubQueryConsideringDeferredCharges(query, amount, comparisonOperator, true);
			return TopLevelBusinessObjectQuery(query);
		}

		ZQuery GetWIPQueryDeferredChargesOnly(SQLComparisonOperator comparisonOperator, ZDecimal amount)
		{
			var query = GetBaseJobSubQuery();
			AddWIPSubQueryConsideringDeferredCharges(query, amount, comparisonOperator, false);
			return TopLevelBusinessObjectQuery(query);
		}

		ZQuery GetAccrualQuery(SQLComparisonOperator comparisonOperator, ZDecimal amount)
		{
			var query = GetBaseJobSubQuery();
			AddACRSubQuery(query, amount, comparisonOperator);
			return TopLevelBusinessObjectQuery(query);
		}

		ZQuery GetMarginQuery(SQLComparisonOperator comparisonOperator, ZDecimal amount)
		{
			var query = GetBaseJobSubQuery();

			var queryParams = new ZSqlParameterCollection();
			queryParams.Add("@WIP", TransactionLineTypes.WIP, AccTransactionLinesSchema.AL_LineType);
			queryParams.Add("@ACR", TransactionLineTypes.Accrual, AccTransactionLinesSchema.AL_LineType);
			queryParams.Add("@CST", TransactionLineTypes.Cost, AccTransactionLinesSchema.AL_LineType);
			queryParams.Add("@REV", TransactionLineTypes.Revenue, AccTransactionLinesSchema.AL_LineType);

			string queryText = @"
		(SELECT 
			(CASE 
				WHEN EXISTS(SELECT null FROM " + AccTransactionLinesSchema.Constants.SqlSchemaName + "." + AccTransactionLinesSchema.Constants.TableName + "  WHERE " + AccTransactionLines.Schema.AL_JH + " = " + JobHeader.Schema.PK + @") OR
					EXISTS(SELECT null FROM " + JobChargeSchema.Constants.SqlSchemaName + "." + JobChargeSchema.Constants.TableName + "  WHERE " + JobCharge.Schema.JR_JH + " = " + JobHeader.Schema.PK + @")
				THEN (SELECT CASE 
					WHEN (TotalRevenue = 0 AND TotalWIP = 0 AND TotalCost = 0 AND TotalAccrual = 0) THEN 0 
					WHEN (TotalRevenue + TotalWIP) != 0 THEN ROUND((TotalLineAmount  * 100) / (TotalRevenue + TotalWIP), 2) 
					ELSE CASE WHEN TotalLineAmount > 0 THEN 100 ELSE -100 END 
					END 
					FROM (SELECT
							SUM(RevRecognized) + SUM(RevNotRecognized) AS TotalRevenue, 
							SUM(WipRecognized) + SUM(WipNotRecognized) AS TotalWIP,
							SUM(CstRecognized) + SUM(CstNotRecognized) AS TotalCost,  
							SUM(AcrRecognized) + SUM(AcrNotRecognized) AS TotalAccrual, 
							SUM(RevRecognized) + SUM(WipRecognized) + SUM(CstRecognized) + SUM(AcrRecognized) + SUM(RevNotRecognized) + SUM(WipNotRecognized) + SUM(CstNotRecognized) + SUM(AcrNotRecognized) AS TotalLineAmount
						FROM (
							SELECT
								CASE WHEN " + AccTransactionLines.Schema.AL_LineType + " = @REV AND " + AccTransactionLines.Schema.AL_ReverseDate + " IS NOT NULL THEN CAST(" + AccTransactionLines.Schema.AL_LineAmount + @" AS DECIMAL(24,9)) ELSE 0 END AS RevRecognized,   
								CASE WHEN " + AccTransactionLines.Schema.AL_LineType + " = @REV AND " + AccTransactionLines.Schema.AL_ReverseDate + " IS NULL THEN CAST(" + AccTransactionLines.Schema.AL_LineAmount + @" AS DECIMAL(24,9)) ELSE 0 END AS RevNotRecognized, 
								CASE WHEN " + AccTransactionLines.Schema.AL_LineType + " = @CST AND " + AccTransactionLines.Schema.AL_ReverseDate + " IS NOT NULL THEN CAST(" + AccTransactionLines.Schema.AL_LineAmount + @" AS DECIMAL(24,9)) ELSE 0 END AS CstRecognized,
								CASE WHEN " + AccTransactionLines.Schema.AL_LineType + " = @CST AND " + AccTransactionLines.Schema.AL_ReverseDate + " IS NULL THEN CAST(" + AccTransactionLines.Schema.AL_LineAmount + @" AS DECIMAL(24,9)) ELSE 0 END AS CstNotRecognized, 
								CASE WHEN " + AccTransactionLines.Schema.AL_LineType + " = @WIP AND " + AccTransactionLines.Schema.AL_ReverseDate + " IS NULL THEN CAST(-" + AccTransactionLines.Schema.AL_LineAmount + @" AS DECIMAL(24,9)) ELSE 0 END AS WipRecognized, 
								0 AS WipNotRecognized, 
								CASE WHEN " + AccTransactionLines.Schema.AL_LineType + " = @ACR AND " + AccTransactionLines.Schema.AL_ReverseDate + " IS NULL THEN CAST(-" + AccTransactionLines.Schema.AL_LineAmount + @" AS DECIMAL(24,9)) ELSE 0 END AS AcrRecognized,
								0 AS AcrNotRecognized
		 					FROM " + AccTransactionLinesSchema.Constants.SqlSchemaName + "." + AccTransactionLinesSchema.Constants.TableName + @" 
							WHERE " + AccTransactionLines.Schema.AL_JH + " = " + JobHeader.Schema.PK + @"
							UNION ALL
							SELECT
								0 AS RevRecognized, 
								0 AS RevNotRecognized, 
								0 AS CstRecognized,
								0 AS CstNotRecognized, 
								0 AS WipRecognized, 
								CASE WHEN " + JobCharge.Schema.JR_AL_ARLine + " IS NULL THEN " + JobCharge.Schema.JR_LocalSellAmt + @" ELSE 0 END AS WipNotRecognized, 
								0 AS AcrRecognized,
								CASE WHEN " + JobCharge.Schema.JR_AL_APLine + " IS NULL THEN -" + JobCharge.Schema.JR_LocalCostAmt + @" ELSE 0 END  AS AcrNotRecognized
							FROM " + JobChargeSchema.Constants.SqlSchemaName + "." + JobChargeSchema.Constants.TableName + @" 
							WHERE " + JobCharge.Schema.JR_JH + " = " + JobHeader.Schema.PK + @"
						) AS Details
					) AS Totals
				)
				ELSE 0 END) AS TotalMargin) " + comparisonOperator.ComparisonText(amount) + " " + amount.ToString(2);

			query.AddFilterAndZSQLParameterCollection(queryText, queryParams);

			return TopLevelBusinessObjectQuery(query);
		}

		void AddAmountSubQueryForREVAndCST(ZDBOnlyQuery query, string lineType, ZDecimal amount, SQLComparisonOperator comparisonOperator, ZString signChange)
		{
			var @params = new ZSqlParameterCollection();

			string lineTypeParamName = "@LineType" + lineType;
			string amountParamName = "@Amount" + lineType;

			@params.Add(lineTypeParamName, lineType, AccTransactionLinesSchema.AL_LineType);
			@params.Add(amountParamName, amount, AccTransactionLinesSchema.AL_LineAmount);

			var queryCore = string.Format(@"
								(
									SELECT	ISNULL(SUM(Amount), 0) 
									FROM	(
												SELECT	Amount = CAST({1}AL_LineAmount AS DECIMAL(24,9))
												FROM	dbo.AccTransactionLines  
												WHERE	AL_JH = JH_PK
														AND AL_LineType = {0}
											) a 
								)", lineTypeParamName, signChange);

			query.AddFilterAndZSQLParameterCollection(queryCore + comparisonOperator.ComparisonText(amount) + " " + amountParamName, @params);
		}

		void AddACRSubQuery(ZDBOnlyQuery query, ZDecimal amount, SQLComparisonOperator comparisonOperator)
		{
			var @params = new ZSqlParameterCollection();

			string lineTypeParamName = "@LineType" + TransactionLineTypes.Accrual;
			string amountParamName = "@Amount" + TransactionLineTypes.Accrual;

			@params.Add(lineTypeParamName, TransactionLineTypes.Accrual, AccTransactionLinesSchema.AL_LineType);
			@params.Add(amountParamName, amount, AccTransactionLinesSchema.AL_LineAmount);

			var queryCore = @"( SELECT ISNULL(SUM(Amount), 0) [SumAmount]
								FROM
								(
									----Acr Recognized
									SELECT CAST(ISNULL(AL_LineAmount, 0) AS DECIMAL(24,9)) [Amount] FROM dbo.AccTransactionLines WHERE AL_JH = JH_PK AND AL_LineType = 'ACR' AND AL_ReverseDate IS NULL
								
									UNION ALL 

									--AcrNotRecognized								
									SELECT CAST(ISNULL(JR_LocalCostAmt, 0) AS DECIMAL(24,9)) [Amount] FROM dbo.JobCharge WHERE JR_JH = JH_PK AND JR_AL_APLine IS NULL
								) a
							 ) ";

			query.AddFilterAndZSQLParameterCollection(queryCore + comparisonOperator.ComparisonText(amount) + " " + amountParamName, @params);
		}

		void AddWIPSubQuery(ZDBOnlyQuery query, ZDecimal amount, SQLComparisonOperator comparisonOperator)
		{
			var @params = new ZSqlParameterCollection();

			var lineTypeParamName = "@LineType" + TransactionLineTypes.WIP;
			var amountParamName = "@Amount" + TransactionLineTypes.WIP;

			@params.Add(lineTypeParamName, TransactionLineTypes.WIP, AccTransactionLinesSchema.AL_LineType);
			@params.Add(amountParamName, amount, AccTransactionLinesSchema.AL_LineAmount);

			var queryCore = @"( SELECT ISNULL(SUM(Amount), 0) [SumAmount]
									FROM
									(
										--WipOrAcrRecognized
										SELECT CAST(ISNULL(-AL_LineAmount, 0) AS DECIMAL(24,9)) [Amount] FROM dbo.AccTransactionLines WHERE AL_JH = JH_PK AND AL_LineType = 'WIP' AND AL_ReverseDate IS NULL
			
										UNION ALL 
								
										--WipNotRecognized
										SELECT CAST(ISNULL(JR_LocalSellAmt, 0) AS DECIMAL(24,9)) [Amount] FROM dbo.JobCharge WHERE JR_JH = JH_PK AND JR_AL_ARLine IS NULL
									) a
								  ) ";

			query.AddFilterAndZSQLParameterCollection(queryCore + comparisonOperator.ComparisonText(amount) + " " + amountParamName, @params);
		}

		void AddWIPSubQueryConsideringDeferredCharges(ZDBOnlyQuery query, ZDecimal amount, SQLComparisonOperator comparisonOperator, bool excludeDeferredCharges)
		{
			var @params = new ZSqlParameterCollection();

			var lineTypeParamName = "@LineType" + TransactionLineTypes.WIP;
			var amountParamName = "@Amount" + TransactionLineTypes.WIP;

			@params.Add(lineTypeParamName, TransactionLineTypes.WIP, AccTransactionLinesSchema.AL_LineType);
			@params.Add(amountParamName, amount, AccTransactionLinesSchema.AL_LineAmount);

			var queryCore = string.Format(@"
								(SELECT ISNULL(SUM(Amount), 0) [SumAmount]
								 FROM
									(
										--WipOrAcrRecognized
										SELECT	CAST(ISNULL(-AL_LineAmount, 0) AS DECIMAL(24,9)) [Amount] 
										FROM	dbo.AccTransactionLines 
												JOIN dbo.JobCharge ON AL_PK = JR_AL_ARLine 
										WHERE	AL_JH = JH_PK 
												AND AL_LineType = 'WIP' 
												AND AL_ReverseDate IS NULL 
												AND JR_InvoiceType {0} IN ('DED', 'DCD', 'DBD', 'FID', 'CUD', 'FRD', 'ITD')  
					
										UNION ALL 
							 
										--WipNotRecognized
										SELECT CAST(ISNULL(JR_LocalSellAmt, 0) AS DECIMAL(24,9)) [Amount] 
										FROM	dbo.JobCharge 
										WHERE	JR_JH = JH_PK 
												AND JR_AL_ARLine IS NULL 
												AND JR_InvoiceType {0} IN ('DED', 'DCD', 'DBD', 'FID', 'CUD', 'FRD', 'ITD')  
									) a
								) ", excludeDeferredCharges ? "NOT" : "");

			query.AddFilterAndZSQLParameterCollection(queryCore + comparisonOperator.ComparisonText(amount) + " " + amountParamName, @params);
		}

		ZQuery GetHasAccrualQuery(ZBool thisOptionChecked)
		{
			if (thisOptionChecked)
			{
				var query = GetBaseJobSubQuery();
				var queryCore = @"	
								EXISTS (SELECT 1 FROM dbo.AccTransactionLines WHERE AL_JH = JH_PK AND AL_LineType = 'ACR' AND AL_ReverseDate IS NULL AND AL_LineAmount <> 0)		
								OR
								EXISTS (SELECT 1 FROM dbo.JobCharge WHERE JR_JH = JH_PK AND JR_AL_APLine IS NULL AND JR_LocalCostAmt <> 0)";

				query.AddFilterAndZSQLParameterCollection(queryCore, null);
				return TopLevelBusinessObjectQuery(query);
			}

			return new ZQuery();
		}
		public ZQuery GetHasNoAccrualQuery(ZBool thisOptionChecked)
		{
			if (thisOptionChecked)
			{
				var query = GetBaseJobSubQuery();
				var queryCore = @"	
								NOT EXISTS (SELECT 1 FROM dbo.AccTransactionLines WHERE AL_JH = JH_PK AND AL_LineType = 'ACR' AND AL_ReverseDate IS NULL AND AL_LineAmount <> 0)		
								AND
								NOT EXISTS (SELECT 1 FROM dbo.JobCharge WHERE JR_JH = JH_PK AND JR_AL_APLine IS NULL AND JR_LocalCostAmt <> 0)";

				query.AddFilterAndZSQLParameterCollection(queryCore, null);
				return TopLevelBusinessObjectQuery(query);
			}

			return new ZQuery();
		}
		ZQuery GetHasWIPQuery(ZBool thisOptionChecked)
		{
			if (thisOptionChecked)
			{
				var query = GetBaseJobSubQuery();
				var queryCore = @"	
								EXISTS(SELECT 1 FROM dbo.AccTransactionLines WHERE AL_JH = JH_PK AND AL_LineType = 'WIP' AND AL_ReverseDate IS NULL AND AL_LineAmount<> 0)		
								OR
								EXISTS(SELECT 1 FROM dbo.JobCharge WHERE JR_JH = JH_PK AND JR_AL_ARLine IS NULL AND JR_LocalSellAmt <> 0)";

				query.AddFilterAndZSQLParameterCollection(queryCore, null);
				return TopLevelBusinessObjectQuery(query);
			}

			return new ZQuery();
		}
		public ZQuery GetHasNoWIPQuery(ZBool thisOptionChecked)
		{
			if (thisOptionChecked)
			{
				var query = GetBaseJobSubQuery();
				var queryCore = @"	
								NOT EXISTS(SELECT 1 FROM dbo.AccTransactionLines WHERE AL_JH = JH_PK AND AL_LineType = 'WIP' AND AL_ReverseDate IS NULL AND AL_LineAmount<> 0)		
								AND
								NOT EXISTS(SELECT 1 FROM dbo.JobCharge WHERE JR_JH = JH_PK AND JR_AL_ARLine IS NULL AND JR_LocalSellAmt <> 0)";

				query.AddFilterAndZSQLParameterCollection(queryCore, null);
				return TopLevelBusinessObjectQuery(query);
			}

			return new ZQuery();
		}
		#endregion

		#region Lists

		public GlbBranchDependentCollection Branches
		{
			get { return FindboxLookupCollections.GetCompanyBranchesCollection(Factory); }
		}

		public GlbDepartmentCollection Departments
		{
			get { return FindboxLookupCollections.GetDepartmentCollection(Factory); }
		}

		public GlbStaffCollection Staffs
		{
			get { return FindboxLookupCollections.GetStaffCollection(Factory); }
		}

		#region Branch Management Code

		protected CodeDescriptionPairList BranchManagementCodeList
		{
			get { return AccountingMasterFilesRegistry.Instance.BranchManagementCodes.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList(); }
		}

		#endregion

		#endregion

		#region Filter Names and Categories

		FilterCategory FinancialDetailsCategory
		{
			get
			{
				var categoryNameOverride = FilterStrip.AmountFiltersCategoryNameOveride;
				return FinancialDetailsCategory_innerValue ?? (FinancialDetailsCategory_innerValue = FilterCategories.GetOrCreateFilterCategory(categoryNameOverride ?? ResString.GetMultilingualString("Accounting|JobManagementFilter|AmountFilters", "Amount Filters")));
			}
		}
		FilterCategory FinancialDetailsCategory_innerValue;

		FilterCategory BillingCategory
		{
			get
			{
				var categoryNameOverride = FilterStrip.BillingFiltersCategoryNameOveride;
				return BillingCategory_innerValue ?? (BillingCategory_innerValue = FilterCategories.GetOrCreateFilterCategory(categoryNameOverride ?? ResString.GetMultilingualString("0618a68d-6efb-4224-b680-d3f8b850400d", "Billing")));
			}
		}
		FilterCategory BillingCategory_innerValue;

		MultilingualString AppendFilterNameSuffixInOtherCategories(MultilingualString category)
		{
			return new ModifiedMultilingualString((string[] s) =>
			{
				return s[0] + (s.Length > 1 && !string.IsNullOrEmpty(s[1]) ? " " + s[1] : "");
			}, category, FilterStrip.FilterNameSuffixInOtherCategories);
		}

		public string APInvoiceNumber
		{
			get { return (NoResString)"AP Invoice #"; }
		}

		public string ARTransactionNumber
		{
			get { return (NoResString)"AR Transaction #"; }
		}

		public string SupplierCostReference
		{
			get { return (NoResString)"Supplier Cost Reference"; }
		}

		public ZString ChargesWithDebtor
		{
			get { return (NoResString)"Charges with Debtor"; }
		}

		public ZString ChargesWithCreditor
		{
			get { return (NoResString)"Charges with Creditor"; }
		}

		#endregion

		#region IAccountingFilterStrip Members

		object IAccountingFilterStrip.BillingFilterCategory
		{
			get { return BillingCategory; }
		}

		#endregion
	}
}

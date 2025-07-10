using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class PeriodicInvoiceBaseJobFilterBusinessObject : FilterStripBusinessObject
	{
		public PeriodicInvoiceBaseJobFilterBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "PeriodicInvoiceForm";
			QueryObjectType = typeof(Periodic_Invoicing.PeriodicInvoiceSelectableJob);
			this.SelectedJobTypes = new List<ZString>();
		}

		#region Filters

		protected override bool ShouldAddCustomSqlFilter => false;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddCurrencyFilter(filters);
			AddDebtorFilter(filters);
			AddTaxBranchFilter(filters);
			AddInvoiceTypeFilter(filters);
			AddARSettlementGroupFilter(filters);
			AddOrganizationBranchFilter(filters);

			ModuleFilter filter = filters.AddGuidFilter("Charge Line Branch", ModuleIDs.GlbBranch, JobChargeSchema.JR_GB, BindingLists.GlbBranch_List);
			filter.Category = ChargeCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|ChargeLineBranch", "Charge Line Branch");
			filter.SubGroup = JobChargeSubGroup;

			filter = filters.AddGuidFilter("Charge Line Department", ModuleIDs.GlbDepartment, JobChargeSchema.JR_GE, GlbDepartment_List);
			filter.Category = ChargeCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|ChargeLineDepartment", "Charge Line Department");
			filter.SubGroup = JobChargeSubGroup;

			filter = filters.AddGuidFilter("Charge Code", ModuleIDs.AccChargeCode, JobChargeSchema.JR_AC, ChargeCode_List);
			filter.Category = ChargeCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|ChargeCode", "Charge Code");
			filter.SubGroup = JobChargeSubGroup;

			filter = filters.AddTextFilter("Charge Line Sell Reference", JobChargeSchema.JR_SellReference);
			filter.Category = ChargeCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|ChargeLineSellReference", "Charge Line Sell Reference");
			filter.SubGroup = JobChargeSubGroup;

			if (PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany))
			{
				filter = filters.AddTextFilter("Charge Line Fixed Place of Supply", JobChargeSchema.JR_SellPlaceOfSupply, PlaceOfSupplyListProvider.GetCurrentCompanyPlaceOfSupplyList());
				filter.Category = ChargeCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("27aa20d8-4ca6-4a36-a911-2ddf68baa1c1", "Charge Line Fixed Place of Supply");
				filter.SubGroup = JobChargeSubGroup;
			}

			if (AccountingConfigurationRegistry.Instance.AllowDescriptionChargeLineFilterInPeriodicInvoice.Value)
			{
				filter = filters.AddTextFilter("Charge Line Description", JobChargeSchema.JR_Desc);
				filter.Category = ChargeCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("0a6e069e-cdf6-4497-9cd2-0de6c7436f33", "Charge Line Description");
				filter.SubGroup = JobChargeSubGroup;
			}

			filter = filters.AddTextFilter("Job Status", GetJobStatusQuery, JobStatusList);
			filter.Category = JobHeaderCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|JobStatus", "Job Status");
			filter.SubGroup = JobSubGroup;

			filter = filters.AddDateFilter("Job Open Date", JobHeaderSchema.JH_A_JOP);
			filter.Category = JobHeaderCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|JobOpenDate", "Job Open Date");
			filter.SubGroup = JobSubGroup;

			filter = filters.AddGuidFilter("Job Local Client", ModuleIDs.Organisation, GetLocalClientQuery, BindingLists.OrgHeader_List);
			filter.Category = JobHeaderCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|JobLocalClient", "Job Local Client");
			filter.SubGroup = JobSubGroup;

			filter = filters.AddGuidFilter("Job Header Branch", ModuleIDs.GlbBranch, JobHeaderSchema.JH_GB, BindingLists.GlbBranch_List);
			filter.Category = JobHeaderCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|JobHeaderBranch", "Job Header Branch");
			filter.SubGroup = JobSubGroup;

			filter = filters.AddGuidFilter("Job Header Department", ModuleIDs.GlbDepartment, JobHeaderSchema.JH_GE, GlbDepartment_List);
			filter.Category = JobHeaderCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|JobHeaderDepartment", "Job Header Department");
			filter.SubGroup = JobSubGroup;

			if (AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				filter = filters.AddGuidFilter("Job Header Tax Branch", ModuleIDs.GlbBranch, JobHeaderSchema.JH_GB_TaxBranch, BindingLists.GlbBranch_List);
				filter.Category = JobHeaderCategory;
				filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|JobHeaderTaxBranch", "Job Header Tax Branch");
				filter.SubGroup = JobSubGroup;
			}

			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
			{
				filter = filters.AddDateFilter(ACCOUNTING_DATE, AccountingUtils.GetAccountingDateQuery);
				filter.Category = FilterCategories.Dates;
				filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|AccountingDate", "Accounting Date");
				filter.SubGroup = MiscSubGroup;
			}

			filter = filters.AddDateFilter(ETA, JobFilterProvider.GetETAQuery);
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|ETA", "ETA");
			filter.SubGroup = MiscSubGroup;

			filter = filters.AddDateFilter(ETD, JobFilterProvider.GetETDQuery);
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|ETD", "ETD");
			filter.SubGroup = MiscSubGroup;

			filter = filters.AddDateFilter(ATA, JobFilterProvider.GetATAQuery);
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|ATA", "ATA");
			filter.SubGroup = MiscSubGroup;

			filter = filters.AddDateFilter(ATD, JobFilterProvider.GetATDQuery);
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|ATD", "ATD");
			filter.SubGroup = MiscSubGroup;

			filter = filters.AddDateFilter(DELIVERY_DATE, JobFilterProvider.GetDeliveryDateQuery);
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|DeliveryDate", "Delivery Date");
			filter.SubGroup = MiscSubGroup;

			filter = filters.AddDateFilter(PICKUP_DATE, JobFilterProvider.GetPickupDateQuery);
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|PickupDate", "Pickup Date");
			filter.SubGroup = MiscSubGroup;

			filter = filters.AddDateFilter(CUSTOMS_CLEARANCE_DATE, JobFilterProvider.GetCustomsClearanceDateQuery);
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|CustomsClearanceDate", "Customs Clearance Date");
			filter.SubGroup = MiscSubGroup;

			filter = filters.AddDateFilter(AWB_ISSUE_DATE, JobFilterProvider.GetAWBCutOffDateQuery);
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|AWBIssueDate", "AWB Issue Date");
			filter.SubGroup = MiscSubGroup;

			filter = filters.AddDateFilter(COMPLETION_DATE, JobFilterProvider.GetCompletionDate);
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|CompletionDate", "Completion Date");
			filter.SubGroup = MiscSubGroup;

			filter = filters.AddTextFilter(TRANSPORT_MODE, JobFilterProvider.GetTransportMode, TransportMode_List);
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|TransportMode", "Transport Mode");
			filter.SubGroup = MiscSubGroup;

			filter = filters.AddTextFilter(SERVICE_DIRECTION, JobFilterProvider.GetServiceDirection, ServiceDirection_List);
			filter.Category = FilterCategories.ModesAndTypes;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|ServiceDirection", "Service Direction");
			filter.SubGroup = MiscSubGroup;

			filter = filters.AddGuidFilter(CARRIER, ModuleIDs.Organisation, JobFilterProvider.GetCarrierQuery, BindingLists.OrgHeader_List);
			filter.Category = OperationsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|Carrier", "Carrier");
			filter.SubGroup = OperationsSubGroup;

			filter = filters.AddGuidFilter(PRINCIPAL, ModuleIDs.Organisation, JobFilterProvider.GetPrincipalQuery, BindingLists.OrgHeader_List);
			filter.Category = OperationsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|Principal", "Principal");
			filter.SubGroup = OperationsSubGroup;

			filter = filters.AddTextAndNkFilter(VOYAGEVESSEL, (op, text, nk) => { return JobFilterProvider.GetVoyageAndVesselQuery(op, text, nk); }, ModuleIDs.RefVessel, BindingLists.RefVessel_List);
			filter.Category = OperationsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|Vessel", "Voyage/Vessel");
			filter.SubGroup = OperationsSubGroup;

			var helper = ObjectFactory.Get<IPeriodicInvoicingReferenceNumberFilter>("IPeriodicInvoicingReferenceNumberFilter");
			var referenceNumberFilter = (ModuleFilter)helper.GetReferenceNumberFilter(JobFilterProvider, Additional_Reference);
			referenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|AdditionalReference", "Additional Reference #");
			referenceNumberFilter.SubGroup = OperationsSubGroup;
			referenceNumberFilter.Category = OperationsCategory;
			filters.AddCustomFilter(referenceNumberFilter);

			filter = filters.AddNumberFilter(Order_Reference, JobFilterProvider.GetOrderNoQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobOrderItemSchema.JT_OrderReference);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|OrderReference", "Order Reference");
			filter.Category = OperationsCategory;
			filter.SubGroup = OperationsSubGroup;

			filter = filters.AddNkFilter(SERVICE_LEVEL, JobFilterProvider.GetServiceLevel, ModuleIDs.ServiceLevel, ServiceLevel_List);
			filter.Category = OperationsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|ServiceLevel", "Service Level");
			filter.SubGroup = operationsSubGroup;

			filter = filters.AddGuidFilter(SENDING_AGENT, ModuleIDs.Organisation, JobFilterProvider.GetSendingAgentQuery, Forwarders);
			filter.Category = OperationsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|Sending Agent", "Sending Agent");
			filter.SubGroup = OperationsSubGroup;

			filter = filters.AddGuidFilter(RECEIVING_AGENT, ModuleIDs.Organisation, JobFilterProvider.GetReceivingAgentQuery, Forwarders);
			filter.Category = OperationsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|Receiving Agent", "Receiving Agent");
			filter.SubGroup = OperationsSubGroup;

			AddCurrentCompanyFilter(filters);
			AddTransactionsEligibleForPostingFilter(filters);
			AddDeferredTransactions(filters);

			return filters;
		}

		protected override void AddInitialAuditFilters(ModuleFilterCollection filters)
		{
			var filterCollectionBeforeAddingAuditFilters = filters.ToArray();
			base.AddInitialAuditFilters(filters);
			var filterCollectionAfterAddingAuditFilters = filters.ToArray();

			var auditFilters = filterCollectionAfterAddingAuditFilters.Except(filterCollectionBeforeAddingAuditFilters).ToList();
			auditFilters.ForEach(f => f.SubGroup = JobSubGroup);
		}

		protected virtual ModuleNkFilter AddCurrencyFilter(ModuleFilterCollection filterCollection)
		{
			var filter = new ModuleNkFilter("Currency", GetCurrencyQuery, ModuleIDs.RefCurrency, AH_RXList);
			filter.SubGroup = JobChargeSubGroup;
			filter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

			filterCollection.AddFilter(filter);

			return filter;
		}

		protected virtual ModuleGuidFilter AddDebtorFilter(ModuleFilterCollection filterCollection)
		{
			var filter = new ModuleGuidFilter("Debtor", ModuleIDs.Organisation, JobChargeSchema.JR_OH_SellAccount, BindingLists.OrgHeader_List);
			filter.Category = ChargeOrganizationCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|Debtor", "Debtor");
			filter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			filter.SubGroup = JobChargeSubGroup;

			filterCollection.AddFilter(filter);

			return filter;
		}

		protected ModuleGuidFilter AddTaxBranchFilter(ModuleFilterCollection filterCollection)
		{
			var filter = new ModuleGuidFilter("Tax Branch", ModuleIDs.GlbBranch, GetTaxBranchQuery, BindingLists.GlbBranch_List);
			filter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			filter.SubGroup = JobChargeSubGroup;

			filterCollection.AddFilter(filter);

			return filter;
		}

		protected virtual ModuleTextFilter AddInvoiceTypeFilter(ModuleFilterCollection filterCollection)
		{
			var filter = new ModuleTextFilter("Invoice Type", GetInvoiceTypeQuery, InvoiceTypeList);
			filter.Category = ChargeCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|PeriodicInvoiceJobsFilter|InvoiceType", "Invoice Type");
			filter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			filter.SubGroup = JobChargeSubGroup;

			filterCollection.AddFilter(filter);

			return filter;
		}

		protected virtual void AddARSettlementGroupFilter(ModuleFilterCollection filterCollection)
		{
		}

		protected virtual void AddOrganizationBranchFilter(ModuleFilterCollection filterCollection)
		{
		}

		protected virtual ModuleTextFilter AddCurrentCompanyFilter(ModuleFilterCollection filterCollection)
		{
			var filter = new ModuleTextFilter("Current Company", GetCurrentCompanyFilter);
			filter.SubGroup = JobChargeSubGroup;
			filter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

			filterCollection.AddFilter(filter);

			return filter;
		}

		protected virtual ModuleTextFilter AddTransactionsEligibleForPostingFilter(ModuleFilterCollection filterCollection)
		{
			var filter = new ModuleTextFilter("Transactions Eligible for Posting", GetTransactionEligibleForPosting);
			filter.SubGroup = JobChargeSubGroup;
			filter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

			filterCollection.AddFilter(filter);

			return filter;
		}

		protected virtual void AddDeferredTransactions(ModuleFilterCollection filters)
		{
		}

		public virtual void SetupFilter(PeriodicInvoiceBase parent)
		{
			((ModuleNkFilter)this["Currency"]).Property = parent.CurrencyNK;
			SelectedJobTypes = parent.SelectedJobTypeCodes;
		}

		public List<ZString> SelectedJobTypes
		{
			get
			{
				return selectedJobTypes;
			}
			private set
			{
				selectedJobTypes = value;
				foreach (ModuleFilter filter in this)
				{
					if (filter.MultilingualDescription != null && !filter.MultilingualDescription.IsEmpty)
					{
						string description = filter.MultilingualDescription.ToString();
						int index = description.IndexOf('[');
						filter.MultilingualDescription = ResString.GetMultilingualString("c9833e46-b090-481f-9609-794c4ba4af31", "{0}{1}", description.Substring(0, index > 0 ? index : description.Length).Trim(), GetFilterDescription(filter.Description));
					}
				}
			}
		}
		List<ZString> selectedJobTypes;

		public ZQuery GetChargeQuery()
		{
			return GetChargeQueryCore();
		}

		protected virtual ZQuery GetChargeQueryCore()
		{
			IsChargeQuery = true;
			try
			{
				return Filter;
			}
			finally
			{
				IsChargeQuery = false;
			}
		}

		bool IsChargeQuery { get; set; }

		public ZQuery GetChargeQueryWithoutSpecialFilters()
		{
			IsExcludeSpecialFilter = true;
			try
			{
				return GetChargeQuery();
			}
			finally
			{
				IsExcludeSpecialFilter = false;
			}
		}

		bool IsExcludeSpecialFilter { get; set; }

		JobFilterProviderForPeriodicInvoice JobFilterProvider
		{
			get { return jobFilterProvider ?? (jobFilterProvider = new JobFilterProviderForPeriodicInvoice(this)); }
		}
		JobFilterProviderForPeriodicInvoice jobFilterProvider;

		MiscFilterSubGroup MiscSubGroup
		{
			get
			{
				return miscFilterSubGroup ?? (miscFilterSubGroup = new MiscFilterSubGroup(this));
			}
		}
		MiscFilterSubGroup miscFilterSubGroup;

		class MiscFilterSubGroup : ModuleFilterSubGroup
		{
			public MiscFilterSubGroup(PeriodicInvoiceBaseJobFilterBusinessObject parent)
			{
				this.ParentFilterBO = parent;
			}
			readonly PeriodicInvoiceBaseJobFilterBusinessObject ParentFilterBO;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				if (ParentFilterBO.IsChargeQuery)
				{
					return new ZQuery();
				}
				else
				{
					return filter;
				}
			}
		}

		JobFilterSubGroup JobSubGroup
		{
			get
			{
				return jobFilterSubGroup ?? (jobFilterSubGroup = new JobFilterSubGroup(this));
			}
		}
		JobFilterSubGroup jobFilterSubGroup;

		class JobFilterSubGroup : ModuleFilterSubGroup
		{
			public JobFilterSubGroup(PeriodicInvoiceBaseJobFilterBusinessObject parent)
			{
				this.ParentFilterBO = parent;
			}
			readonly PeriodicInvoiceBaseJobFilterBusinessObject ParentFilterBO;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				if (ParentFilterBO.IsChargeQuery)
				{
					return new ZQuery();
				}
				else
				{
					return filter;
				}
			}
		}

		OperationsFilterSubGroup OperationsSubGroup
		{
			get
			{
				return operationsSubGroup ?? (operationsSubGroup = new OperationsFilterSubGroup(this));
			}
		}
		OperationsFilterSubGroup operationsSubGroup;

		class OperationsFilterSubGroup : ModuleFilterSubGroup
		{
			public OperationsFilterSubGroup(PeriodicInvoiceBaseJobFilterBusinessObject parent)
			{
				this.ParentFilterBO = parent;
			}
			readonly PeriodicInvoiceBaseJobFilterBusinessObject ParentFilterBO;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				if (ParentFilterBO.IsChargeQuery)
				{
					return new ZQuery();
				}
				else
				{
					return filter;
				}
			}
		}

		protected JobChargeFilterSubGroup JobChargeSubGroup
		{
			get
			{
				return jobChargeSubGroup ?? (jobChargeSubGroup = new JobChargeFilterSubGroup(this));
			}
		}
		JobChargeFilterSubGroup jobChargeSubGroup;

		protected class JobChargeFilterSubGroup : ModuleFilterSubGroup
		{
			public JobChargeFilterSubGroup(PeriodicInvoiceBaseJobFilterBusinessObject parent)
			{
				this.ParentFilterBO = parent;
			}
			readonly PeriodicInvoiceBaseJobFilterBusinessObject ParentFilterBO;

			protected ZDBOnlyQuery GetChargeQuery(bool getSubQuery, ZQuery filter)
			{
				ZDBOnlyQuery chargesFilter = getSubQuery ? new ZDBOnlySubQuery(typeof(Charge), JobChargeSchema.JR_JH) : new ZDBOnlyQuery(typeof(Charge));

				if (filter != null)
				{
					chargesFilter.AddToFilter(filter);
				}

				return chargesFilter;
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				if (ParentFilterBO.IsChargeQuery)
				{
					return GetChargeQuery(false, filter);
				}
				else
				{
					ZDBOnlyQuery jobFilter = new ZDBOnlyQuery(typeof(Job));
					var chargeSubQuery = (ZDBOnlySubQuery)GetChargeQuery(true, filter);

					jobFilter.AddSubQuery(chargeSubQuery, JoinCondition.And);

					return jobFilter;
				}
			}
		}

		protected SpecialChargeFilterSubGroup FilterNotRequiredOrAlreadyAppliedForSinglePeriodicInvoiceSubGroup
		{
			get
			{
				return specialChargeSubGroup ?? (specialChargeSubGroup = new SpecialChargeFilterSubGroup(JobChargeSubGroup, this));
			}
		}
		SpecialChargeFilterSubGroup specialChargeSubGroup;

		protected class SpecialChargeFilterSubGroup : ModuleFilterSubGroup
		{
			public SpecialChargeFilterSubGroup(ModuleFilterSubGroup parentSubGroup, PeriodicInvoiceBaseJobFilterBusinessObject filterBusinessObject)
				: base(parentSubGroup)
			{
				this.ParentFilterBO = filterBusinessObject;
			}

			readonly PeriodicInvoiceBaseJobFilterBusinessObject ParentFilterBO;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				if (ParentFilterBO.IsExcludeSpecialFilter)
				{
					return new ZQuery();
				}
				else
				{
					return filter;
				}
			}
		}

		#region Filter

		public override ZQuery Filter
		{
			get
			{
				var filterQuery = base.Filter;
				if (!IsChargeQuery)
				{
					filterQuery.AddToFilter(JobFilterProvider.Filter);
				}
				return filterQuery;
			}
		}

		#endregion

		#region Filter Categories

		FilterCategory fChargeOrganizationCategory;
		protected FilterCategory ChargeOrganizationCategory
		{
			get
			{
				if (fChargeOrganizationCategory == null)
				{
					fChargeOrganizationCategory = new FilterCategory(ResString.GetMultilingualString("90244440-01FB-4d88-9FC2-E0392F5AE891", "Charge Organization"));
				}
				return fChargeOrganizationCategory;
			}
		}

		FilterCategory fChargeCategory;
		protected FilterCategory ChargeCategory
		{
			get
			{
				if (fChargeCategory == null)
				{
					fChargeCategory = new FilterCategory(ResString.GetMultilingualString("F87605D2-776A-4d86-8049-DD97C69D11FC", "Charge"));
				}
				return fChargeCategory;
			}
		}

		FilterCategory fJobHeaderCategory;
		protected FilterCategory JobHeaderCategory
		{
			get
			{
				if (fJobHeaderCategory == null)
				{
					fJobHeaderCategory = new FilterCategory(ResString.GetMultilingualString("4BD16684-F528-4855-9CAA-B3FC55427F71", "Job Header"));
				}
				return fJobHeaderCategory;
			}
		}

		FilterCategory fDatesCategory;
		protected FilterCategory DatesCategory
		{
			get
			{
				if (fDatesCategory == null)
				{
					fDatesCategory = new FilterCategory(ResString.GetMultilingualString("8fb3403b-1a2f-4177-acbe-8376df8e3ae2", "Dates"));
				}
				return fDatesCategory;
			}
		}

		FilterCategory operationsCategory;
		protected FilterCategory OperationsCategory
		{
			get
			{
				if (operationsCategory == null)
				{
					operationsCategory = new FilterCategory(ResString.GetMultilingualString("e04b85c5-ae49-499b-a1f8-df40cc55e40d", "Operations"));
				}
				return operationsCategory;
			}
		}

		#endregion

		#region Filter Description
		string GetFilterDescription(string filterName)
		{
			var result = string.Empty;
			var applicableAllJobTypes = JobFilterProvider.GetJobTypesApplicableToAFilter(filterName);
			if (applicableAllJobTypes != null && applicableAllJobTypes.Length > 0)
			{
				var appJobTypes = applicableAllJobTypes.Where(x => SelectedJobTypes.Contains(x)).ToArray();
				if (appJobTypes.Any())
				{
					result = Res.GetString("3546bb38-aebe-4637-bffc-50813ffa2274", "[{0}]", string.Join(", ", appJobTypes.ToArray()));
				}
			}
			return result;
		}
		#endregion

		#endregion

		#region Queries

		RefCurrencyCollection AH_RXList
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
		RefCurrencyCollection fAH_RXList;

		protected virtual ZQuery GetCurrencyQuery(ZString value)
		{
			var result = new ZQuery();
			var localCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			if (value == localCurrency)
			{
				var notSellInvoiceCurrency = new ZQuery(JobChargeSchema.JR_RX_NKSellInvoiceCurrency, ZString.Empty);
				notSellInvoiceCurrency.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_RX_NKSellInvoiceCurrency, localCurrency);
				result.AddToFilter(notSellInvoiceCurrency);
				result.AddToFilter(JobChargeSchema.JR_InvoiceType, DeferredInvoiceTypesBillingInLocalCurrency);

				var billInLocalCurrency = new ZQuery(JobChargeSchema.JR_RX_NKSellCurrency, value);
				billInLocalCurrency.AddToFilter(JobChargeSchema.JR_InvoiceType, SQLComparisonOperator.NotEqual, DeferredInvoiceTypesBillingInLocalCurrency);
				result.AddToFilter(billInLocalCurrency, JoinCondition.Or);
			}
			else
			{
				result = new ZQuery(JobChargeSchema.JR_RX_NKSellInvoiceCurrency, value);
				result.AddToFilter(JobChargeSchema.JR_InvoiceType, DeferredInvoiceTypesBillingInLocalCurrency);

				var sellCurrencyFilter = new ZQuery(JobChargeSchema.JR_RX_NKSellCurrency, value);
				sellCurrencyFilter.AddToFilter(JobChargeSchema.JR_InvoiceType, SQLComparisonOperator.NotEqual, DeferredInvoiceTypesBillingInLocalCurrency);
				result.AddToFilter(sellCurrencyFilter, JoinCondition.Or);
			}

			return result;
		}

		protected virtual ZQuery GetTaxBranchQuery(ZGuid value)
		{
			var result = new ZQuery();

			if (value.IsValid)
			{
				result.AddToFilter(JobChargeSchema.JR_GB_SellTaxBranch, value);
			}
			return result;
		}

		protected ZQuery GetInvoiceTypeQuery(ZString value)
		{
			ZQuery invoiceTypeQuery = new ZQuery();
			invoiceTypeQuery.AddToFilter(JobChargeSchema.JR_InvoiceType, value);

			return invoiceTypeQuery;
		}

		ZQuery GetJobStatusQuery(ZString value)
		{
			var jobQuery = new ZQuery();

			if (value == "OPN")
			{
				jobQuery.AddToFilter(JobHeaderSchema.JH_Status, SQLComparisonOperator.NotEqual, JobHeaderStatus.Closed.Code);
			}
			else
			{
				jobQuery.AddToFilter(JobHeaderSchema.JH_Status, SQLComparisonOperator.Equal, value);
			}

			return jobQuery;
		}

		ZQuery GetLocalClientQuery(ZGuid localClientPK)
		{
			ZDBOnlyQuery jobQuery = new ZDBOnlyQuery(typeof(Job));

			if (localClientPK.IsValid)
			{
				ZDBOnlySubQuery addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
				addressQuery.AddToFilter(OrgAddressSchema.OA_OH, localClientPK);
				jobQuery.AddSubQuery(addressQuery, JoinCondition.And);
			}

			return jobQuery;
		}

		protected List<ZString> DeferredInvoiceTypesBillingInLocalCurrency
		{
			get
			{
				if (DeferredInvoiceTypesBillingInLocalCurrency_innerValue == null)
				{
					DeferredInvoiceTypesBillingInLocalCurrency_innerValue = new List<ZString>();
					foreach (string deferredInvoiceType in InvoiceTypeCalculationProvider.DeferredInvoiceTypes)
					{
						if (InvoiceTypeCalculationProvider.BillInLocalCurrency(deferredInvoiceType))
						{
							DeferredInvoiceTypesBillingInLocalCurrency_innerValue.Add(deferredInvoiceType);
						}
					}
				}

				return DeferredInvoiceTypesBillingInLocalCurrency_innerValue;
			}
		}

		List<ZString> DeferredInvoiceTypesBillingInLocalCurrency_innerValue;

		ZQuery GetTransactionEligibleForPosting(SQLComparisonOperator sqlOperator, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(Charge));

			ZDBOnlySubQuery transactionLinesSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), JobChargeSchema.JR_AL_ARLine);
			transactionLinesSubQuery.AddToFilter(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.WIP);

			ZDBOnlyQuery transactionLinesQuery = new ZDBOnlyQuery(typeof(Charge));
			transactionLinesQuery.AddToFilter(JobChargeSchema.JR_AL_ARLine, SQLComparisonOperator.Equal, null);
			transactionLinesQuery.AddSubQuery(transactionLinesSubQuery, JoinCondition.Or);

			result.AddToFilter(transactionLinesQuery);

			ZDBOnlySubQuery chargeCodeSubQuery = new ZDBOnlySubQuery(typeof(AccChargeCode), JobChargeSchema.JR_AC);
			chargeCodeSubQuery.AddToFilter(AccChargeCodeSchema.AC_ChargeType, Constants.ChargeType.Comment);

			ZDBOnlyQuery chargeCodeQuery = new ZDBOnlyQuery(typeof(Charge));
			chargeCodeQuery.AddToFilter(JobChargeSchema.JR_OSSellAmt, SQLComparisonOperator.NotEqual, 0);
			chargeCodeQuery.AddSubQuery(chargeCodeSubQuery, JoinCondition.Or);

			result.AddToFilter(chargeCodeQuery);

			return result;
		}

		ZQuery GetCurrentCompanyFilter(SQLComparisonOperator sqlOperator, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(Charge));

			ZDBOnlySubQuery branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobChargeSchema.JR_GB);
			branchQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			result.AddSubQuery(branchQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Lists

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		public CodeDescriptionPairList JobStatusList
		{
			get
			{
				CodeDescriptionPairList list = new JobHeaderStatusList();
				list.AddPair("OPN", Res.GetString("ffec4b40-a674-427e-85fe-0adfd90bfcdc", "All Open Jobs"));
				return list;
			}
		}

		public CodeDescriptionPairList InvoiceTypeList
		{
			get
			{
				if (fInvoiceTypeList == null)
				{
					fInvoiceTypeList = new CodeDescriptionPairList();
					foreach (CodeDescriptionPair invoiceType in new InvoiceTypesList())
					{
						if (InvoiceTypeCalculationProvider.IsDeferredInvoiceType(invoiceType.Code))
						{
							fInvoiceTypeList.Add(invoiceType);
						}
					}
				}
				return fInvoiceTypeList;
			}
		}

		CodeDescriptionPairList fInvoiceTypeList;

		public GlbDepartmentCollection GlbDepartment_List
		{
			get { return FindboxLookupCollections.GetDepartmentCollection(Factory); }
		}

		CodeDescriptionPairList TransportMode_List
		{
			get { return FreightCodePairLists.JS_TransportModeList(); }
		}

		AccChargeCodeCollection ChargeCode_List
		{
			get { return FindboxLookupCollections.GetChargeCodeCollection(Factory); }
		}

		#region Service Level List

		ActiveServiceLevelCollection ServiceLevel_List => fServiceLevel_List ?? (fServiceLevel_List = new ActiveServiceLevelCollection(Factory));

		ActiveServiceLevelCollection fServiceLevel_List;

		#endregion

		public CodeDescriptionPairList ServiceDirection_List
		{
			get
			{
				if (ServiceDirectionList_internal == null)
				{
					ServiceDirectionList_internal = new CodeDescriptionPairList();
					ServiceDirectionList_internal.AddPair(OrgConstants.ServiceDirection.Code.Export, OrgDescriptions.ServiceDirection.Export);
					ServiceDirectionList_internal.AddPair(OrgConstants.ServiceDirection.Code.Import, OrgDescriptions.ServiceDirection.Import);
					ServiceDirectionList_internal.AddPair(OrgConstants.ServiceDirection.Code.Domestic, OrgDescriptions.ServiceDirection.Domestic);
					ServiceDirectionList_internal.AddPair(OrgConstants.ServiceDirection.Code.CrossTrade, OrgDescriptions.ServiceDirection.CrossTrade);
					ServiceDirectionList_internal.Sort();
				}
				return ServiceDirectionList_internal;
			}
		}
		CodeDescriptionPairList ServiceDirectionList_internal;

		public ForwarderCollection Forwarders
		{
			get { return FindboxLookupCollections.GetForwarderCollection(Factory); }
		}

		#endregion

		#region Filter Name
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Name. No Translation required")]
		public const string TRANSPORT_MODE = "Transport Mode";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String Constant")]
		public const string CUSTOMS_CLEARANCE_DATE = "Customs Clearance Date";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String Constant")]
		public const string COMPLETION_DATE = "Completion Date";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String Constant")]
		public const string DELIVERY_DATE = "Delivery Date";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String Constant")]
		public const string PICKUP_DATE = "Pickup Date";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String Constant")]
		public const string AWB_ISSUE_DATE = "AWB Issue Date";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String Constant")]
		public const string ACCOUNTING_DATE = "Accounting Date";
		public const string ATD = "ATD";
		public const string ATA = "ATA";
		public const string ETA = "ETA";
		public const string ETD = "ETD";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String Constant")]
		public const string SERVICE_DIRECTION = "Service Direction";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String Constant")]
		public const string CARRIER = "Carrier";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String Constant")]
		public const string PRINCIPAL = "Principal";
		public const string VOYAGEVESSEL = "Voyage/Vessel";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String Constant")]
		public const string SENDING_AGENT = "Sending Agent";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String Constant")]
		public const string RECEIVING_AGENT = "Receiving Agent";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String Constant")]
		public const string SERVICE_LEVEL = "Service Level";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String Constant")]
		public const string Order_Reference = "Order Reference";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String Constant")]
		public const string Additional_Reference = "Additional Reference #";
		#endregion
	}
}

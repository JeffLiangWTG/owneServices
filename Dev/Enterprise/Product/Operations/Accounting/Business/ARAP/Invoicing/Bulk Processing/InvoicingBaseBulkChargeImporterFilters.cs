using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoicingBaseBulkChargeImporterFilters : InvoiceBulkOperationWithParentChildRelationshipFilters
	{
		public InvoicingBaseBulkChargeImporterFilters(InvoicingBase parentInvoice) : base(parentInvoice)
		{
			QueryObjectType = typeof(Job);
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "APInvoiceBulkChargeImportForm";
			SetDefaultValueInFilters();
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new InvoicingBaseBulkChargeImporterFilters(ParentInvoice);

		protected override bool ShouldAddCustomSqlFilter => false;

		#region ResourceStrings

		static string IncludeAccrualsWithNoCreditor
		{
			get { return Res.GetString("Accounting|APInvoiceBulkChargeImporterFilters|IncludeAccrualsWithNoCreditor", "Include Accruals With No Creditor"); }
		}

		static MultilingualString IncludeAccrualsWithNoCreditorFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|APInvoiceBulkChargeImporterFilters|IncludeAccrualsWithNoCreditor", "Include Accruals With No Creditor"); }
		}

		static MultilingualString JobBranch
		{
			get { return ResString.GetMultilingualString("Accounting|APInvoiceBulkChargeImporterFilters|JobBranch", "Job Branch"); }
		}

		static MultilingualString JobDepartment
		{
			get { return ResString.GetMultilingualString("Accounting|APInvoiceBulkChargeImporterFilters|JobDepartment", "Job Department"); }
		}

		static MultilingualString JobTaxBranch
		{
			get { return ResString.GetMultilingualString("Accounting|APInvoiceBulkChargeImporterFilters|JobTaxBranch", "Job Tax Branch"); }
		}

		static MultilingualString OtherCreditorsFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|APInvoiceBulkChargeImporterFilters|OtherCreditors", "Other Creditors"); }
		}

		static MultilingualString AccrualPostDateFilterDescripton
		{
			get { return ResString.GetMultilingualString("Accounting|APInvoiceBulkChargeImporterFilters|AccrualPostDate", "Accrual Post Date"); }
		}

		static MultilingualString HouseBillFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|APInvoiceBulkChargeImporterFilters|HouseBill", "House Bill"); }
		}

		#endregion

		#region SubGroups

		#region JobChargeSubGroup

		public ModuleFilterSubGroup JobChargeSubGroup
		{
			get
			{
				return jobChargeSubGroup ?? (jobChargeSubGroup = new JobChargeSubGroupImplementation(this));
			}
		}
		ModuleFilterSubGroup jobChargeSubGroup;

		class JobChargeSubGroupImplementation : ModuleFilterSubGroup
		{
			public JobChargeSubGroupImplementation(InvoicingBaseBulkChargeImporterFilters filter)
			{
				Filter = filter;
			}

			InvoicingBaseBulkChargeImporterFilters Filter { get; }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var invoiceDetailsQuery = new ZQuery(JobChargeSchema.JR_APInvoiceNum, Filter.ParentInvoice.AH_TransactionNum);
				invoiceDetailsQuery.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_APInvoiceNum, ZString.Empty);

				var chargeQuery = new ZQuery(Filter.GetChargePKsToExcludeForWhereClause());
				chargeQuery.AddToFilter(JobChargeSchema.JR_OSCostAmt, SQLComparisonOperator.NotEqual, 0);
				chargeQuery.AddToFilter(invoiceDetailsQuery);
				chargeQuery.AddToFilter(JobChargeSchema.JR_GC, GlbCompany.CurrentCompany.PK);
				chargeQuery.AddToFilter(JobChargeSchema.JR_E6, null);
				chargeQuery.AddToFilter(filter);

				ZQuery result;

				if (Filter.IsChildFiltersGenerationRunning)
				{
					result = chargeQuery;
				}
				else
				{
					var chargeSubQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_JH);
					chargeSubQuery.AddToFilter(chargeQuery);

					var jobQuery = new ZDBOnlyQuery(typeof(JobHeader));
					jobQuery.AddSubQuery(chargeSubQuery, JoinCondition.And);

					result = jobQuery;
				}

				return result;
			}
		}

		#endregion

		#region ConsolSubGroup

		public ModuleFilterSubGroup ConsolSubGroup
		{
			get
			{
				return consolSubGroup ?? (consolSubGroup = new ConsolSubGroupImplementation(this));
			}
		}
		ModuleFilterSubGroup consolSubGroup;

		class ConsolSubGroupImplementation : ModuleFilterSubGroup
		{
			public ConsolSubGroupImplementation(InvoicingBaseBulkChargeImporterFilters filter)
			{
				Filter = filter;
			}

			InvoicingBaseBulkChargeImporterFilters Filter { get; }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZQuery();
				if (!Filter.IsChildFiltersGenerationRunning)
				{
					string query = JobHeaderSchema.Constants.PK + jobConsolQuery + filter.GetAsWhereClause(false) + ")";
					ZSqlParameterCollection parameters = new ZSqlParameterCollection(filter.Params);
					Filter.Helper.ReplaceAutogeneratedParameters(ref query, parameters);
					result.AddFilterAndZSQLParameterCollection(query, parameters);
				}
				return result;
			}
		}

		#endregion

		#region AccrualSubGroup

		public ModuleFilterSubGroup AccrualSubGroup
		{
			get
			{
				return accrualSubGroup ?? (accrualSubGroup = new AccrualSubGroupImplementation(JobChargeSubGroup));
			}
		}
		ModuleFilterSubGroup accrualSubGroup;

		class AccrualSubGroupImplementation : ModuleFilterSubGroup
		{
			public AccrualSubGroupImplementation(ModuleFilterSubGroup parent)
				: base(parent)
			{
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(JobCharge));

				var accrualQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK);
				accrualQuery.AddToFilter(AccTransactionLinesSchema.AL_GC, GlbCompany.CurrentCompany.PK);
				accrualQuery.AddToFilter(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.Accrual);

				if (filter.LiteralTextADO == DefaultFilter.LiteralTextADO)
				{
					result.AddToFilter(JobChargeSchema.JR_AL_APLine, null);
				}
				else
				{
					accrualQuery.AddToFilter(filter);
				}

				result.AddSubQuery(JobChargeSchema.JR_AL_APLine, AccTransactionLinesSchema.PK, accrualQuery, JoinCondition.Or);

				return result;
			}
		}

		#endregion

		#region DeclarationSubGroup

		public ModuleFilterSubGroup DeclarationSubGroup
		{
			get
			{
				return declarationSubGroup ?? (declarationSubGroup = new DeclarationSubGroupImplementation(this));
			}
		}
		ModuleFilterSubGroup declarationSubGroup;

		class DeclarationSubGroupImplementation : ModuleFilterSubGroup
		{
			public DeclarationSubGroupImplementation(InvoicingBaseBulkChargeImporterFilters filter)
			{
				Filter = filter;
			}

			InvoicingBaseBulkChargeImporterFilters Filter { get; }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				if (Filter.IsChildFiltersGenerationRunning)
				{
					return new ZDBOnlyQuery(typeof(JobCharge));
				}
				else
				{
					var result = new ZDBOnlyQuery(typeof(JobHeader));
					result.AddToFilter(filter);
					return result;
				}
			}
		}

		#endregion

		#region ShipmentSubGroup

		public ModuleFilterSubGroup ShipmentSubGroup
		{
			get
			{
				return shipmentSubGroup ?? (shipmentSubGroup = new ShipmentSubGroupImplementation(this));
			}
		}
		ModuleFilterSubGroup shipmentSubGroup;

		class ShipmentSubGroupImplementation : ModuleFilterSubGroup
		{
			public ShipmentSubGroupImplementation(InvoicingBaseBulkChargeImporterFilters filter)
			{
				Filter = filter;
			}

			InvoicingBaseBulkChargeImporterFilters Filter { get; }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				if (!Filter.IsChildFiltersGenerationRunning)
				{
					ZDBOnlySubQuery shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
					shipmentQuery.AddToFilter(filter);

					ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobHeader));
					result.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentQuery, JoinCondition.And);
					return result;
				}
				else
				{
					return new ZQuery();
				}
			}
		}

		#endregion

		#region RunSheetNumberSubGroup

		ModuleFilterSubGroup RunSheetNumberSubGroup
		{
			get { return runSheetNumberSubGroup ?? (runSheetNumberSubGroup = new RunSheetNumberSubGroupImplementation()); }
		}
		ModuleFilterSubGroup runSheetNumberSubGroup;

		class RunSheetNumberSubGroupImplementation : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var runSheetNumberQuery = new ZDBOnlyQuery(typeof(JobHeader));

				var dtbBookingSubQuery = new ZDBOnlySubQuery(typeof(IDtbBooking), JobHeaderSchema.JH_ParentID);
				var dtbBookingInstructionSubQuery = new ZDBOnlySubQuery(typeof(IDtbBookingInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement);
				var dtbBookingConfirmationSubQuery = new ZDBOnlySubQuery(typeof(IDtbConsignmentConfirmation), DtbBookingConfirmationSchema.KK_KN_BookingInstruction);
				var dtbConsignmentRunSheetInstructionSubQuery = new ZDBOnlySubQuery(typeof(IDtbConsignmentRunSheetInstruction), DtbBookingConfirmationSchema.KK_K1_RunSheetInstruction);
				var dtbConsignmentRunSheetSubQuery = new ZDBOnlySubQuery(typeof(IDtbConsignmentRunSheet), DtbConsignmentRunSheetInstructionSchema.K1_KG_RunSheet);

				dtbConsignmentRunSheetSubQuery.AddToFilter(filter);

				dtbConsignmentRunSheetInstructionSubQuery.AddSubQuery(dtbConsignmentRunSheetSubQuery, JoinCondition.And);
				dtbBookingConfirmationSubQuery.AddSubQuery(dtbConsignmentRunSheetInstructionSubQuery, JoinCondition.And);
				dtbBookingInstructionSubQuery.AddSubQuery(dtbBookingConfirmationSubQuery, JoinCondition.And);
				dtbBookingSubQuery.AddSubQuery(dtbBookingInstructionSubQuery, JoinCondition.And);

				runSheetNumberQuery.AddSubQuery(dtbBookingSubQuery, JoinCondition.And);

				return runSheetNumberQuery;
			}
		}

		#endregion

		#region FlightSubGroup

		ModuleFilterSubGroup FlightSubGroup
		{
			get { return flightSubGroup ?? (flightSubGroup = new FlightSubGroupImplementation(ConsolSubGroup)); }
		}
		ModuleFilterSubGroup flightSubGroup;

		class FlightSubGroupImplementation : ModuleFilterSubGroup
		{
			public FlightSubGroupImplementation(ModuleFilterSubGroup parent)
				: base(parent) { }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var jW_VoyageFlightFilterText = filter.LiteralTextADO.Replace(JobVoyageSchema.JV_VoyageFlight.Name, JobConsolTransportSchema.JW_VoyageFlight.Name);
				var jW_VoyageFlightFilter = new ZQuery().AddFilterAndZSQLParameterCollection(jW_VoyageFlightFilterText, new ZSqlParameterCollection());

				var flightQuery = new ZDBOnlyQuery(typeof(ForwardingConsol));

				var jobConsolTransportSubQuery = new ZDBOnlySubQuery(typeof(AutoJobConsolTransport), JobConsolTransportSchema.JW_ParentGUID);
				var jobSailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobConsolTransportSchema.JW_JX);
				var jobVoyDestinationSubQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
				var jobVoyageSubQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyDestinationSchema.JB_JV);

				jobVoyageSubQuery.AddToFilter(filter, JoinCondition.And);
				jobVoyDestinationSubQuery.AddSubQuery(jobVoyageSubQuery, JoinCondition.And);
				jobSailingSubQuery.AddSubQuery(jobVoyDestinationSubQuery, JoinCondition.And);
				jobConsolTransportSubQuery.AddSubQuery(jobSailingSubQuery, JoinCondition.And);
				jobConsolTransportSubQuery.AddToFilter(jW_VoyageFlightFilter, JoinCondition.Or);

				flightQuery.AddSubQuery(jobConsolTransportSubQuery, JoinCondition.And);

				return flightQuery;
			}
		}

		#endregion

		#region JobFilterSubGroup

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
			public JobFilterSubGroup(InvoicingBaseBulkChargeImporterFilters filter)
			{
				Filter = filter;
			}
			InvoicingBaseBulkChargeImporterFilters Filter { get; }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return !Filter.IsChildFiltersGenerationRunning ? filter : new ZQuery();
			}
		}

		#endregion

		#endregion

		public override ZQuery Filter
		{
			get
			{
				Helper.ResetUniqueParameterNumber();

				if (IsChildFiltersGenerationRunning)
				{
					var result = new ZDBOnlyQuery(typeof(JobCharge));
					result.AddToFilter(base.Filter);
					return result;
				}
				else
				{
					var result = new ZDBOnlyQuery(typeof(JobHeader));
					result.AddToFilter(base.Filter);
					return result;
				}
			}
		}

		#region Module Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = base.GetModuleFiltersCore();

			AddHiddenMandatoryFilters(filters);
			AddJobHeaderFilters(filters);
			AddCustomSqlFilters(filters);

			return filters;
		}

		void AddHiddenMandatoryFilters(ModuleFilterCollection filters)
		{
			var textFilter = filters.AddTextFilter("Accruals or no linked line", (SQLComparisonOperator sqlOperator, ZString value) => DefaultFilter);
			textFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			textFilter.SubGroup = AccrualSubGroup;
		}

		static ZQuery DefaultFilter => new ZQuery().AddFilterAndZSQLParameterCollection("1=1", null);

		protected override void AddOrganizationFilters(ModuleFilterCollection filters)
		{
			base.AddOrganizationFilters(filters);

			ModuleGuidFilter guidfilter = (ModuleGuidFilter)filters[InvoiceBulkOperationFilterHelper.CreditorFilterName];
			guidfilter.PropertyValidation = ValidateCreditor;
			guidfilter.SubGroup = JobChargeSubGroup;
			guidfilter.Visibility = FilterVisibility.AlwaysVisible;

			guidfilter = (ModuleGuidFilter)filters[InvoiceBulkOperationFilterHelper.SendingAgentFilterName];
			guidfilter.SubGroup = ConsolSubGroup;

			guidfilter = (ModuleGuidFilter)filters[InvoiceBulkOperationFilterHelper.ReceivingAgentFilterName];
			guidfilter.SubGroup = ConsolSubGroup;

			ModuleFlagsFilter flagFilter = filters.AddFlagsFilter(InvoiceBulkOperationFilterHelper.IncludeAccrualsWithNoCreditorFilterName,
				new[] { IncludeAccrualsWithNoCreditor },
				new GetFlagsQuery[] { value => new ZQuery() });
			flagFilter.Category = FilterCategories.Organisations;
			flagFilter.MultilingualDescription = IncludeAccrualsWithNoCreditorFilterDescription;
			flagFilter.Property0 = true;
			flagFilter.Visibility = FilterVisibility.AlwaysVisible;

			ModuleTextFilter textFilter = filters.AddTextFilter(InvoiceBulkOperationFilterHelper.OtherCreditorsFilterName, GetOtherCreditorsFilter, OtherCreditorsFilterList);
			textFilter.Category = FilterCategories.Organisations;
			textFilter.MultilingualDescription = OtherCreditorsFilterDescription;
			bool includeChargesForAllOtherCreditors = AccountingConfigurationRegistry.Instance.IncludeChargesForAllOtherCreditors.Value;
			bool includeChargesForCreditorsWithTheSameAPSettlementGroup = AccountingConfigurationRegistry.Instance.IncludeChargesForCreditorsWithTheSameAPSettlementGroup.Value;
			if (includeChargesForCreditorsWithTheSameAPSettlementGroup)
			{
				textFilter.Property = OtherCreditors.CreditorsSharingSameAPSettlementGroup;
				textFilter.Visibility = FilterVisibility.AlwaysVisible;
			}
			else if (includeChargesForAllOtherCreditors)
			{
				textFilter.Property = OtherCreditors.AllOtherCreditors;
				textFilter.Visibility = FilterVisibility.AlwaysVisible;
			}
		}

		protected override void AddDateFilters(ModuleFilterCollection filters)
		{
			base.AddDateFilters(filters);

			ModuleDateFilter dateFilter = (ModuleDateFilter)filters[InvoiceBulkOperationFilterHelper.PostDateFilterName];
			dateFilter.MultilingualDescription = AccrualPostDateFilterDescripton;
			dateFilter.SubGroup = AccrualSubGroup;

			dateFilter = (ModuleDateFilter)filters[InvoiceBulkOperationFilterHelper.ShipmentETA_ETDFilterName];
			dateFilter.SubGroup = ShipmentSubGroup;

			// AWB Issue Date
			AddDateFilter(filters, InvoiceBulkOperationFilterHelper.AWBIssueDateFilterName, InvoiceBulkOperationFilterHelper.AWBIssueDateFilterDescription, GetAWBIssueDateFilter);
			// ATA/ATD
			AddDateFilter(filters, InvoiceBulkOperationFilterHelper.ConsolATA_ATDFilterName, InvoiceBulkOperationFilterHelper.ConsolATA_ATDFilterDescription, GetATA_ATDFilter);
			//Customs Clearance Date
			AddDateFilter(filters, InvoiceBulkOperationFilterHelper.CustomsClearanceDateFilterName, InvoiceBulkOperationFilterHelper.CustomsClearanceDateFilterDescription, GetCustomsClearanceDateFilter);
			//Delivery Date
			AddDateFilter(filters, InvoiceBulkOperationFilterHelper.ShipmentActualDeliveryDateFilterName, InvoiceBulkOperationFilterHelper.ShipmentActualDeliveryDateFilterDescription, GetShipmentActualDeliveryDateFilter);
			// HAWB Issue Date
			AddDateFilter(filters, InvoiceBulkOperationFilterHelper.HAWBIssueDateFilterName, InvoiceBulkOperationFilterHelper.HAWBIssueDateFilterDescription, GetHAWBIssueDateFilter);
			//Pickup Date
			AddDateFilter(filters, InvoiceBulkOperationFilterHelper.ShipmentActualPickupDateFilterName, InvoiceBulkOperationFilterHelper.ShipmentActualPickupDateFilterDescription, GetShipmentActualPickupDateFilter);
			// Accounting Date
			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
			{
				AddDateFilter(filters, InvoiceBulkOperationFilterHelper.AccountingDateFilterName, InvoiceBulkOperationFilterHelper.AccountingDateFilterDescription, AccountingUtils.GetAccountingDateQuery);
				var accountingDateFilter = (ModuleDateFilter)filters[InvoiceBulkOperationFilterHelper.AccountingDateFilterName];
				accountingDateFilter.SubGroup = DeclarationSubGroup;
			}
		}

		protected override void AddOtherOperationsFilters(ModuleFilterCollection filters)
		{
			base.AddOtherOperationsFilters(filters);

			ModuleTextFilter textFilter = (ModuleTextFilter)filters[InvoiceBulkOperationFilterHelper.TransportModeFilterName];
			textFilter.SubGroup = ConsolSubGroup;

			textFilter = (ModuleTextFilter)filters[InvoiceBulkOperationFilterHelper.ContainerModeFilterName];
			textFilter.SubGroup = ConsolSubGroup;

			textFilter = (ModuleTextFilter)filters[InvoiceBulkOperationFilterHelper.FlightFilterName];
			textFilter.SubGroup = FlightSubGroup;

			textFilter = (ModuleTextFilter)filters[InvoiceBulkOperationFilterHelper.MasterBillFilterName];

			textFilter = filters.AddTextFilter(InvoiceBulkOperationFilterHelper.HouseBillFilterName, GetHouseBillFilter);
			textFilter.Category = Helper.OtherOperationsFilters;
			textFilter.MultilingualDescription = HouseBillFilterDescription;
			textFilter.MaxLength = JobDeclarationSchema.JE_HouseBill.MaxLength;

			textFilter = filters.AddFountainFilter(InvoiceBulkOperationFilterHelper.RunSheetNumberFilterName, GetRunSheetNumberFilter, "CR");
			textFilter.MaxLength = DtbConsignmentRunSheetSchema.KG_RunSheetNumber.MaxLength;
			textFilter.Category = Helper.OtherOperationsFilters;
			textFilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.RunSheetNumberFilterDescription;
			textFilter.Prefix = "CR";
			textFilter.SubGroup = RunSheetNumberSubGroup;
		}

		protected override void AddLineFilters(ModuleFilterCollection filters)
		{
			base.AddLineFilters(filters);

			ModuleGuidFilter guidfilter = (ModuleGuidFilter)filters[InvoiceBulkOperationFilterHelper.ChargeCodeFilterName];
			guidfilter.SubGroup = JobChargeSubGroup;

			ModuleNkFilter nkfilter = (ModuleNkFilter)filters[InvoiceBulkOperationFilterHelper.CurrencyFilterName];
			nkfilter.SubGroup = JobChargeSubGroup;

			guidfilter = filters.AddGuidFilter(InvoiceBulkOperationFilterHelper.BranchFilterName, ModuleIDs.GlbBranch, JobChargeSchema.JR_GB, Helper.Branches);
			guidfilter.Category = Helper.ChargeLineFilters;
			guidfilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.LineBranchFilterDerscriptor;
			guidfilter.SubGroup = JobChargeSubGroup;

			guidfilter = filters.AddGuidFilter(InvoiceBulkOperationFilterHelper.DepartmentFilterName, ModuleIDs.GlbDepartment, JobChargeSchema.JR_GE, Helper.Departments);
			guidfilter.Category = Helper.ChargeLineFilters;
			guidfilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.LineDepartmentFilterDescription;
			guidfilter.SubGroup = JobChargeSubGroup;

			ModuleFlagsFilter flagFilter = filters.AddFlagsFilter(InvoiceBulkOperationFilterHelper.ExcludeReverseSignedChargesFilterName,
				new[] { ParentInvoice is APCreditNote ? InvoiceBulkOperationFilterHelper.ExcludePositiveCharges : InvoiceBulkOperationFilterHelper.ExcludeNegativeCharges },
				new GetFlagsQuery[] { GetExcludeReverseSignedConsolCostsFilterQuery });
			flagFilter.Category = Helper.ChargeLineFilters;
			flagFilter.MultilingualDescription = ParentInvoice is APCreditNote ? InvoiceBulkOperationFilterHelper.ExcludePositiveChargesFilterDescription : InvoiceBulkOperationFilterHelper.ExcludeNegativeChargesFilterDescription;
			flagFilter.SubGroup = JobChargeSubGroup;

			if (PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany))
			{
				ModuleTextFilter placeOfSupplyFilter = filters.AddTextFilter(InvoiceBulkOperationFilterHelper.FixedPlaceOfSupplyFilterName, JobChargeSchema.JR_CostPlaceOfSupply, PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany));
				placeOfSupplyFilter.Category = Helper.ChargeLineFilters;
				placeOfSupplyFilter.SubGroup = JobChargeSubGroup;
				placeOfSupplyFilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.FixedPlaceOfSupplyFilterDescription;
			}

			ModuleNumberFilter supplierCostReferenceFilter = (ModuleNumberFilter)filters[InvoiceBulkOperationFilterHelper.SupplierCostReferenceFilterName];
			supplierCostReferenceFilter.SubGroup = JobChargeSubGroup;

			if (AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				guidfilter = filters.AddGuidFilter(InvoiceBulkOperationFilterHelper.TaxBranchFilterName, ModuleIDs.GlbBranch, JobChargeSchema.JR_GB_CostTaxBranch, Helper.Branches);
				guidfilter.Category = Helper.ChargeLineFilters;
				guidfilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.CostTaxBranchFilterDescription;
				guidfilter.SubGroup = JobChargeSubGroup;
				guidfilter.Visibility = FilterVisibility.AlwaysVisible;
				guidfilter.DefaultProperty = DefaultTaxBranchFilterValue;
			}
		}

		void AddJobHeaderFilters(ModuleFilterCollection filters)
		{
			ModuleGuidFilter guidfilter = filters.AddGuidFilter(InvoiceBulkOperationFilterHelper.JobBranchFilterName, ModuleIDs.GlbBranch, GetJobBranchFilter, Helper.Branches);
			guidfilter.Category = Helper.JobHeaderFilters;
			guidfilter.MultilingualDescription = JobBranch;

			guidfilter = filters.AddGuidFilter(InvoiceBulkOperationFilterHelper.JobDepartmentFilterName, ModuleIDs.GlbDepartment, GetJobDepartmentFilter, Helper.Departments);
			guidfilter.Category = Helper.JobHeaderFilters;
			guidfilter.MultilingualDescription = JobDepartment;

			if (AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				guidfilter = filters.AddGuidFilter(InvoiceBulkOperationFilterHelper.JobTaxBranchFilterName, ModuleIDs.GlbBranch, GetJobTaxBranchFilter, Helper.Branches);
				guidfilter.Category = Helper.JobHeaderFilters;
				guidfilter.MultilingualDescription = JobTaxBranch;
			}
		}

		void AddCustomSqlFilters(ModuleFilterCollection filters)
		{
			var jobCustomSqlFilter = filters.AddSqlFilter(InvoiceBulkOperationFilterHelper.JobHeaderCustomSqlFilterName, QueryObjectType);
			jobCustomSqlFilter.Category = FilterCategories.Other;
			jobCustomSqlFilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.JobHeaderCustomSqlFilterDescription;
			jobCustomSqlFilter.SubGroup = JobSubGroup;
			jobCustomSqlFilter.ReadOnly = !EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed;

			var chargeCustomSqlFilter = filters.AddSqlFilter(InvoiceBulkOperationFilterHelper.JobChargeCustomSqlFilterName, typeof(JobCharge));
			chargeCustomSqlFilter.Category = FilterCategories.Other;
			chargeCustomSqlFilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.JobChargeCustomSqlFilterDescription;
			chargeCustomSqlFilter.SubGroup = JobChargeSubGroup;
			chargeCustomSqlFilter.ReadOnly = !EnvProxy.Instance.Security.UseSqlFilterStrip.IsAllowed;
		}

		protected override IFilterStripsHelper GetWorkflowFilterStripsHelper()
		{
			ZString templateCode = "JOB";
			var subQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
			var helper = ObjectFactory.Get<IInvoiceBulkOperationWorkflowFilterStripsHelper>("IInvoiceBulkOperationWorkflowFilterStripsHelper", templateCode, typeof(JobHeader), Factory, false);
			helper.AddRelatedParentJoiningQuery(subQuery);

			return helper;
		}

		#endregion

		#region Overriden Filter Methods

		protected override void AddInitialAuditFilters(ModuleFilterCollection filters)
		{
			var filterCollectionBeforeAddingAuditFilters = filters.ToArray();
			base.AddInitialAuditFilters(filters);
			var filterCollectionAfterAddingAuditFilters = filters.ToArray();

			var auditFilters = filterCollectionAfterAddingAuditFilters.Except(filterCollectionBeforeAddingAuditFilters).ToList();
			auditFilters.ForEach(f => f.SubGroup = JobSubGroup);
		}

		protected override ZQuery GetCreditorFilter(ZGuid creditorPK)
		{
			bool isForJobHeader = !IsChildFiltersGenerationRunning;
			var result = new ZQuery();

			if (isForJobHeader || creditorPK.IsValid)
			{
				if (IncludeAccrualsWithNoCreditorFilter)
				{
					string paramName = Helper.GetUniqueParameterName(JobChargeSchema.JR_OH_CostAccount.Name);
					ZSqlParameterCollection parameters = new ZSqlParameterCollection { ZSqlParameter.New(paramName, creditorPK, JobChargeSchema.JR_OH_CostAccount) };
					result.AddFilterAndZSQLParameterCollection(string.Format("(JR_OH_CostAccount = {0} OR JR_OH_CostAccount IS NULL)", paramName), parameters);
				}
				if (IncludeChargesForAllOtherCreditorsFilter)
				{
					result.AddFilterAndZSQLParameterCollection("(JR_OH_CostAccount IS NOT NULL)", new ZSqlParameterCollection { }, JoinCondition.Or);
				}
				else if (IncludeChargesForCreditorsWithSameAPSettlementGroupFilter)
				{
					OrgHeader creditor = Factory.Load<OrgHeader>(creditorPK);
					ZGuid aPSettlementGroupPK = creditor.APSettlementGroup != null ? creditor.APSettlementGroup.PK : creditor.PK;

					if (aPSettlementGroupPK.IsValid)
					{
						ZQuery orgRelatedPartyFilterQuery = new ZQuery();

						orgRelatedPartyFilterQuery.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, aPSettlementGroupPK);
						orgRelatedPartyFilterQuery.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.APSettlementGroup);
						orgRelatedPartyFilterQuery.AddToFilter(OrgRelatedPartySchema.PR_GC, GlbCompany.CurrentCompany.PK);

						ZDBOnlySubQuery settlementGroupFilter = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
						ZDBOnlySubQuery orgRelatedPartyFilter = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent);
						orgRelatedPartyFilter.AddToFilter(orgRelatedPartyFilterQuery);

						settlementGroupFilter.AddSubQuery(orgRelatedPartyFilter, JoinCondition.And);

						ZDBOnlyQuery jobChargesAPSettlementGroupFilter = new ZDBOnlyQuery(typeof(JobCharge));
						jobChargesAPSettlementGroupFilter.AddSubQuery(JobChargeSchema.JR_OH_CostAccount, settlementGroupFilter, JoinCondition.And);

						result.AddFilterAndZSQLParameterCollection(jobChargesAPSettlementGroupFilter.LiteralTextSqlFormatted, new ZSqlParameterCollection { }, JoinCondition.Or);
					}
					else
					{
						if (!IncludeAccrualsWithNoCreditorFilter)
						{
							result.AddToFilter(JobChargeSchema.JR_OH_CostAccount, creditorPK);
						}
					}
				}
				if (!IncludeAccrualsWithNoCreditorFilter && !IncludeChargesForAllOtherCreditorsFilter && !IncludeChargesForCreditorsWithSameAPSettlementGroupFilter)
				{
					result.AddToFilter(JobChargeSchema.JR_OH_CostAccount, creditorPK);
				}
			}

			return result;
		}

		protected override ZQuery GetSendingAgentFilter(ZGuid sendingAgentPK)
		{
			return Helper.GetSendingAgentSubQuery(sendingAgentPK);
		}

		protected override ZQuery GetReceivingAgentFilter(ZGuid receivingAgentPK)
		{
			return Helper.GetReceivingAgentSubQuery(receivingAgentPK);
		}

		protected override ZQuery GetPostDateFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			return Helper.GetDateFilter(AccTransactionLinesSchema.AL_PostDate, comparisonOperator, value1, value2);
		}

		protected override ZQuery GetShipmentETA_ETDFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZQuery();
			AddDateTimeRange(result, comparisonOperator, JoinCondition.And, JobShipmentSchema.JS_E_DEP, value1, value2);
			AddDateTimeRange(result, comparisonOperator, JoinCondition.Or, JobShipmentSchema.JS_E_ARV, value1, value2);

			return result;
		}

		protected override ZQuery GetFlightFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobVoyage));
			result.AddToFilter(JobVoyageSchema.JV_VoyageFlight, comparisonOperator, value);
			return result;
		}

		protected override ZQuery GetTransportModeFilter(ZString value)
		{
			return Helper.GetQueryWithParameter(JobConsolSchema.JK_TransportMode, SQLComparisonOperator.Equal, value);
		}

		protected override ZQuery GetContainerModeFilter(ZString value)
		{
			return Helper.GetQueryWithParameter(JobConsolSchema.JK_ConsolMode, SQLComparisonOperator.Equal, value);
		}

		protected override ZQuery GetMasterBillFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!IsChildFiltersGenerationRunning)
			{
				var result = new ZDBOnlyQuery(typeof(JobHeader));
				result.AddSubQuery(AccountingUtils.GetMasterBillSubQueryForJobHeader(comparisonOperator, value), JoinCondition.And);
				return result;
			}
			else
			{
				return new ZQuery();
			}
		}

		protected override ZQuery GetCoLoadMasterBillFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!IsChildFiltersGenerationRunning)
			{
				var result = new ZDBOnlyQuery(typeof(JobHeader));
				result.AddSubQuery(AccountingUtils.GetCoLoadMasterBillSubQueryForJobHeader(comparisonOperator, value), JoinCondition.And);
				return result;
			}
			else
			{
				return new ZQuery();
			}
		}

		protected override ZQuery GetContainerNumberFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!IsChildFiltersGenerationRunning)
			{
				return AccountingUtils.GetContainerNumberQueryForJobHeader(comparisonOperator, value);
			}
			else
			{
				return new ZQuery();
			}
		}

		protected override ZQuery GetChargeCodeFilter(ZGuid chargeCodePK)
		{
			return new ZQuery(JobChargeSchema.JR_AC, chargeCodePK);
		}

		protected override ZQuery GetCurrencyFilter(ZString value)
		{
			return new ZQuery(JobChargeSchema.JR_RX_NKCostCurrency, value);
		}

		protected override ZQuery GetSupplierCostReferenceFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery().AddToFilter_PossiblyCommaSeparated(JobChargeSchema.JR_CostReference, comparisonOperator, value);
		}

		#endregion

		#region New Filters

		#region GetRunSheetNumberFilter

		ZQuery GetRunSheetNumberFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!IsChildFiltersGenerationRunning)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(IDtbConsignmentRunSheet));
				result.AddToFilter_PossiblyCommaSeparated(DtbConsignmentRunSheetSchema.KG_RunSheetNumber, comparisonOperator, value);

				return result;
			}
			else
			{
				return new ZQuery();
			}
		}

		#endregion

		List<ZGuid> fChargePKsToExclude;
		public List<ZGuid> ChargePKsToExclude
		{
			get { return fChargePKsToExclude ?? (fChargePKsToExclude = new List<ZGuid>()); }
		}

		#region IncludeAccrualsWithNoCreditorFilter

		public ZBool IncludeAccrualsWithNoCreditorFilter
		{
			get { return ((ModuleFlagsFilter)this[InvoiceBulkOperationFilterHelper.IncludeAccrualsWithNoCreditorFilterName]).Property0; }
		}

		#endregion

		#region OtherCreditors

		public static class OtherCreditors
		{
			public const string AllOtherCreditors = "OTH";
			public const string CreditorsSharingSameAPSettlementGroup = "ASG";
		}

		public CodeDescriptionPairList OtherCreditorsFilterList
		{
			get
			{
				if (fOtherCreditorsFilterList == null)
				{
					fOtherCreditorsFilterList = new CodeDescriptionPairList();
					fOtherCreditorsFilterList.AddPair(OtherCreditors.AllOtherCreditors, Res.GetString("01e1ba99-7893-4fdf-ab81-b3e8ab75361e", "All Other Creditors"));
					fOtherCreditorsFilterList.AddPair(OtherCreditors.CreditorsSharingSameAPSettlementGroup, Res.GetString("8a681cb4-70b2-4fca-9c5a-2ce4abbcd77d", "Creditors Sharing Same AP Settlement Group"));
				}
				return fOtherCreditorsFilterList;
			}
		}
		CodeDescriptionPairList fOtherCreditorsFilterList;

		ZQuery GetOtherCreditorsFilter(ZString value)
		{
			return new ZQuery();
		}

		#endregion

		#region IncludeChargesForAllOtherCreditorsFilter

		public ZBool IncludeChargesForAllOtherCreditorsFilter
		{
			get
			{
				return ((ModuleTextFilter)this[InvoiceBulkOperationFilterHelper.OtherCreditorsFilterName]).Property == OtherCreditors.AllOtherCreditors;
			}
		}

		#endregion

		#region ExcludeReverseSignedChargesFilter

		ZQuery GetExcludeReverseSignedConsolCostsFilterQuery(ZBool value)
		{
			if (value)
			{
				return new ZQuery(JobChargeSchema.JR_OSCostAmt, ParentInvoice is APInvoice ? SQLComparisonOperator.GreaterThan : SQLComparisonOperator.LessThan, ZDecimal.Zero);
			}
			else
			{
				return new ZQuery(JobChargeSchema.JR_OSCostAmt, SQLComparisonOperator.NotEqual, ZDecimal.Zero);
			}
		}

		#endregion

		#region JobBranchFilter

		ZQuery GetJobBranchFilter(ZGuid branchPK)
		{
			var result = new ZQuery();
			if (!IsChildFiltersGenerationRunning && !branchPK.IsEmpty && branchPK.IsValid)
			{
				result.AddToFilter(JobHeaderSchema.JH_GB, branchPK);
			}
			return result;
		}

		#endregion

		#region JobDepartmentFilter

		ZQuery GetJobDepartmentFilter(ZGuid departmentPK)
		{
			var result = new ZQuery();
			if (!IsChildFiltersGenerationRunning && !departmentPK.IsEmpty && departmentPK.IsValid)
			{
				result.AddToFilter(JobHeaderSchema.JH_GE, departmentPK);
			}
			return result;
		}

		#endregion

		#region JobTaxBranchFilter

		ZQuery GetJobTaxBranchFilter(ZGuid branchPK)
		{
			var result = new ZQuery();
			if (!IsChildFiltersGenerationRunning && !branchPK.IsEmpty && branchPK.IsValid)
			{
				result.AddToFilter(JobHeaderSchema.JH_GB_TaxBranch, branchPK);
			}
			return result;
		}

		#endregion

		#region HouseBillFilter

		ZQuery GetHouseBillFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobHeader));
			if (!IsChildFiltersGenerationRunning && !value.IsEmpty)
			{
				var resultQuery1 = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
				var resultQuery2 = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
				var jobShipmentSchemaColumn = JobShipmentSchema.JS_HouseBill;
				var jobDeclarationSchemaColumn = JobDeclarationSchema.JE_HouseBill;

				ZDBOnlySubQuery shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
				shipmentQuery.AddToFilter(jobShipmentSchemaColumn, comparisonOperator, value.Left(jobShipmentSchemaColumn.MaxLength));
				resultQuery1.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentQuery, JoinCondition.And);

				ZDBOnlySubQuery jobQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
				jobQuery.AddToFilter(jobDeclarationSchemaColumn, comparisonOperator, value.Left(jobDeclarationSchemaColumn.MaxLength));
				resultQuery2.AddSubQuery(JobHeaderSchema.JH_ParentID, jobQuery, JoinCondition.And);

				resultQuery1.AddAsUnionQuery(resultQuery2);

				result.AddSubQuery(resultQuery1, JoinCondition.And);
			}
			return result;
		}

		#endregion

		#region IncludeChargesForCreditorsWithSameAPSettlementGroupFilter

		public ZBool IncludeChargesForCreditorsWithSameAPSettlementGroupFilter
		{
			get
			{
				return ((ModuleTextFilter)this[InvoiceBulkOperationFilterHelper.OtherCreditorsFilterName]).Property == OtherCreditors.CreditorsSharingSameAPSettlementGroup;
			}
		}

		#endregion

		ZQuery GetATA_ATDFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			ZQuery result = new ZQuery();
			if (!IsChildFiltersGenerationRunning)
			{
				ZQuery ataQUery = JobFilterProvider.GetATAQuery(comparisonOperator, value1, value2);
				ZQuery atdQUery = JobFilterProvider.GetATDQuery(comparisonOperator, value1, value2);
				result = new ZQuery(ataQUery, JoinCondition.Or, atdQUery);
			}
			return result;
		}

		ZQuery GetAWBIssueDateFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			ZQuery result = new ZQuery();
			if (!IsChildFiltersGenerationRunning)
			{
				result = JobFilterProvider.GetAWBCutOffDateQuery(comparisonOperator, value1, value2);
			}
			return result;
		}

		ZQuery GetCustomsClearanceDateFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			ZQuery result = new ZQuery();
			if (!IsChildFiltersGenerationRunning)
			{
				result = JobFilterProvider.GetCustomsClearanceDateQuery(comparisonOperator, value1, value2);
			}
			return result;
		}

		ZQuery GetHAWBIssueDateFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			ZDBOnlyQuery jobHeaderQuery = new ZDBOnlyQuery(typeof(Job));
			if (!IsChildFiltersGenerationRunning)
			{
				if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
				{
					AddHAWBQueryWithNoDateEntered(jobHeaderQuery);
				}
				else if (comparisonOperator == DateComparisonOperator.HasDateEntered)
				{
					AddHAWBQueryWithDateEntered(jobHeaderQuery);
				}
				else
				{
					AddHAWBQueryWithDateRange(jobHeaderQuery, value1, value2);
				}
			}
			return jobHeaderQuery;
		}

		ZQuery GetShipmentActualPickupDateFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			ZDBOnlyQuery jobHeaderQuery = new ZDBOnlyQuery(typeof(Job));
			if (!IsChildFiltersGenerationRunning)
			{
				ZDBOnlySubQuery shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderSchema.JH_ParentID);
				ZDBOnlySubQuery jobDocsAndCartageQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
				AddDateTimeRange(jobDocsAndCartageQuery, comparisonOperator, JoinCondition.And, JobDocsAndCartageSchema.JP_PickupCartageCompleted, value1, value2);
				shipmentQuery.AddSubQuery(jobDocsAndCartageQuery, JoinCondition.And);
				jobHeaderQuery.AddSubQuery(shipmentQuery, JoinCondition.And);
			}
			return jobHeaderQuery;
		}

		ZQuery GetShipmentActualDeliveryDateFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			ZDBOnlyQuery jobHeaderQuery = new ZDBOnlyQuery(typeof(Job));

			if (!IsChildFiltersGenerationRunning)
			{
				ZDBOnlySubQuery shipmentQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobHeaderSchema.JH_ParentID);
				ZDBOnlySubQuery jobDocsAndCartageQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
				AddDateTimeRange(jobDocsAndCartageQuery, comparisonOperator, JoinCondition.And, JobDocsAndCartageSchema.JP_DeliveryCartageCompleted, value1, value2);
				shipmentQuery.AddSubQuery(jobDocsAndCartageQuery, JoinCondition.And);
				jobHeaderQuery.AddSubQuery(shipmentQuery, JoinCondition.And);
			}

			return jobHeaderQuery;
		}

		void AddHAWBQueryWithNoDateEntered(ZDBOnlyQuery jobHeaderQuery)
		{
			string qryCore = @"
								JH_ParentID IN 
								(
									SELECT JS_PK  
									FROM dbo.JobShipment 
									WHERE JS_HouseBillIssueDate is null 
										  AND 
										  (
												JS_PK NOT IN 
												(
													SELECT JS_PK 
													FROM dbo.JobShipment 
													WHERE JS_PK IN 
															(
																SELECT EH_ParentID 
																FROM dbo.ExportAWBHeader 
																WHERE EH_AWBIssueDate is not null
															) 
															AND JS_IsCancelled = 0
												)
										 ) 
										 AND 
										 JS_IsCancelled = 0
								 )";

			jobHeaderQuery.AddFilterAndZSQLParameterCollection(qryCore, null, JoinCondition.And);
		}

		void AddHAWBQueryWithDateEntered(ZDBOnlyQuery jobHeaderQuery)
		{
			string qryCore = @"
								JH_ParentID IN 
								(
									SELECT JS_PK
									FROM dbo.JobShipment
									WHERE 
									JS_HouseBillIssueDate is not null 
									OR
									(
										JS_HouseBillIssueDate is null 
										AND
										JS_PK IN 
										(
											SELECT EH_ParentID FROM dbo.ExportAWBHeader WHERE EH_AWBIssueDate is not null
										)
									)
								)";

			jobHeaderQuery.AddFilterAndZSQLParameterCollection(qryCore, null, JoinCondition.And);
		}

		void AddHAWBQueryWithDateRange(ZDBOnlyQuery jobHeaderQuery, ZDateTime date1, ZDateTime date2)
		{
			string qryCore = @"	JH_ParentID IN 
								(
									SELECT JS_PK
									FROM dbo.JobShipment
									WHERE 
									(
										JS_HouseBillIssueDate >= @date1 
										AND
										JS_HouseBillIssueDate < @date2
									)
									OR
									(
										JS_HouseBillIssueDate is null 
										AND
										(
											JS_PK IN 
											(
												SELECT EH_ParentID FROM dbo.ExportAWBHeader WHERE EH_AWBIssueDate >= @hawbdate1
												AND
												EH_AWBIssueDate < @hawbdate2
											)
										)
									)
								)";

			var queryParams = new ZSqlParameterCollection();
			queryParams.Add("@date1", date1, JobShipmentSchema.JS_HouseBillIssueDate);
			queryParams.Add("@date2", date2, JobShipmentSchema.JS_HouseBillIssueDate);
			queryParams.Add("@hawbdate1", date1, ExportAWBHeaderSchema.EH_AWBIssueDate);
			queryParams.Add("@hawbdate2", date2, ExportAWBHeaderSchema.EH_AWBIssueDate);

			jobHeaderQuery.AddFilterAndZSQLParameterCollection(qryCore, queryParams);
		}

		#endregion

		#region Implementation

		JobFilterProvider JobFilterProvider
		{
			get
			{
				if (jobFilterProvider == null)
				{
					jobFilterProvider = new JobFilterProvider();
					jobFilterProvider.IsForwardingModule = true;
				}
				return jobFilterProvider;
			}
		}
		JobFilterProvider jobFilterProvider;

		ZQuery GetChargePKsToExcludeForWhereClause()
		{
			var result = new ZQuery();
			if (ChargePKsToExclude != null && ChargePKsToExclude.Count > 0)
			{
				result.AddToFilter(JobChargeSchema.PK, SQLComparisonOperator.NotEqual, ChargePKsToExclude);
			}

			return result;
		}

		const string jobConsolQuery = @" IN
(SELECT 
	" + JobHeaderSchema.Constants.PK + @"
FROM
	" + JobHeaderSchema.Constants.SqlSchemaName + "." + JobHeaderSchema.Constants.TableName + @"
	INNER JOIN " + JobShipmentSchema.Constants.SqlSchemaName + "." + JobShipmentSchema.Constants.TableName + " ON " + JobShipmentSchema.Constants.PK + " = " + JobHeaderSchema.Constants.JH_ParentID + @"
	INNER JOIN " + JobConShipLinkSchema.Constants.SqlSchemaName + "." + JobConShipLinkSchema.Constants.TableName + " ON " + JobConShipLinkSchema.Constants.JN_JS + " = " + JobShipmentSchema.Constants.PK + @"
	INNER JOIN " + JobConsolSchema.Constants.SqlSchemaName + "." + JobConsolSchema.Constants.TableName + " ON " + JobConsolSchema.Constants.PK + " = " + JobConShipLinkSchema.Constants.JN_JK + " ";

		#endregion

		#region Validation

		void ValidateCreditor(ZPropertyInfo propertyInfo)
		{
			MandatoryValidation.CheckEntered(propertyInfo);
		}

		#endregion
	}
}

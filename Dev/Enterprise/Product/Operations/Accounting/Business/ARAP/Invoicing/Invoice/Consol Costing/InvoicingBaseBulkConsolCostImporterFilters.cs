using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoicingBaseBulkConsolCostImporterFilters : InvoiceBulkOperationWithParentChildRelationshipFilters
	{
		public InvoicingBaseBulkConsolCostImporterFilters(InvoicingBase parentInvoice) : base(parentInvoice)
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "APInvoiceBulkConsolCostImportForm";
			SetDefaultValueInFilters();
		}

		public InvoicingBaseBulkConsolCostImporterFilters()
		{
		}

		public List<ZGuid> ConsolCostPKsToExclude
		{
			get { return consolCostPKsToExclude ?? (consolCostPKsToExclude = new List<ZGuid>()); }
		}
		List<ZGuid> consolCostPKsToExclude;

		#region Resource Strings

		static string IncludeConsolCostsWithNoCreditor
		{
			get { return Res.GetString("Accounting|InvoicingBaseBulkConsolCostImporterFilters|IncludeConsolCostsWithNoCreditor", "Include Consol Costs With No Creditor"); }
		}

		static MultilingualString IncludeConsolCostsWithNoCreditorFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoicingBaseBulkConsolCostImporterFilters|IncludeConsolCostsWithNoCreditor", "Include Consol Costs With No Creditor"); }
		}

		static string ExcludePositiveCosts
		{
			get { return Res.GetString("8b0c83ac-d961-4a27-8bf1-d72afa429f43", "Exclude Positive Costs"); }
		}

		static string ExcludeNegativeCosts
		{
			get { return Res.GetString("c489154f-f659-4838-b58e-01f6bdd62b9a", "Exclude Negative Costs"); }
		}

		static MultilingualString ConsolETA_ETD
		{
			get { return ResString.GetMultilingualString("Accounting|APInvoiceBulkConsolCostImporterFilters|ConsolETA_ETD", "Consol ETA/ETD"); }
		}

		#endregion

		#region SubGroups

		#region ConsolCostSubGroup

		public ModuleFilterSubGroup ConsolCostSubGroup
		{
			get { return consolCostSubGroup ?? (consolCostSubGroup = new ConsolCostSubGroupImplementation(this)); }
		}
		ModuleFilterSubGroup consolCostSubGroup;

		class ConsolCostSubGroupImplementation : ModuleFilterSubGroup
		{
			public ConsolCostSubGroupImplementation(InvoicingBaseBulkConsolCostImporterFilters filter)
				: base()
			{
				this.Filter = filter;
			}

			readonly InvoicingBaseBulkConsolCostImporterFilters Filter;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZQuery subQuery = new ZQuery();

				subQuery.AddToFilter(JobConsolCostSchema.E6_OSCostAmount, SQLComparisonOperator.NotEqual, 0m);
				subQuery.AddToFilter(JobConsolCostSchema.E6_AH_APInvoice, null);
				subQuery.AddToFilter(JobConsolCostSchema.PK, SQLComparisonOperator.NotEqual, Filter.ConsolCostPKsToExclude);
				subQuery.AddToFilter(JobConsolCostSchema.E6_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK.ToGuid());

				ZQuery invoiceDetailesQuery = new ZQuery(JobConsolCostSchema.E6_InvoiceNum, Filter.ParentInvoice.AH_TransactionNum);
				invoiceDetailesQuery.AddToFilter(JoinCondition.Or, JobConsolCostSchema.E6_InvoiceNum, ZString.Empty);
				subQuery.AddToFilter(invoiceDetailesQuery);

				subQuery.AddToFilter(filter);

				ZDBOnlyQuery result;

				if (Filter.IsChildFiltersGenerationRunning)
				{
					result = new ZDBOnlyQuery(typeof(JobConsolCost));
					result.AddToFilter(subQuery);
				}
				else
				{
					result = new ZDBOnlyQuery(typeof(GenericConsol.GenericConsol));
					ZDBOnlySubQuery consolCostSubQuery = new ZDBOnlySubQuery(typeof(JobConsolCost), JobConsolCostSchema.E6_ParentID);
					consolCostSubQuery.AddToFilter(subQuery);
					result.AddSubQuery(consolCostSubQuery, JoinCondition.And);
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
				return accrualSubGroup ?? (accrualSubGroup = new AccrualSubGroupImplementation(ConsolCostSubGroup));
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
				ZDBOnlySubQuery accrualQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK);
				accrualQuery.AddToFilter(filter);

				ZDBOnlySubQuery jobChargeQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_E6);
				jobChargeQuery.AddSubQuery(JobChargeSchema.JR_AL_APLine, AccTransactionLinesSchema.PK, accrualQuery, JoinCondition.And);

				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobConsolCost));
				result.AddSubQuery(jobChargeQuery, JoinCondition.And);
				return result;
			}
		}

		#endregion

		#region ContainerNumberSubGroup

		ModuleFilterSubGroup ContainerNumberSubGroup
		{
			get { return containerNumberSubGroup ?? (containerNumberSubGroup = new ContainerNumberSubGroupImplementation()); }
		}
		ModuleFilterSubGroup containerNumberSubGroup;

		class ContainerNumberSubGroupImplementation : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var containerNumberQuery = new ZDBOnlyQuery(typeof(GenericConsol.GenericConsol));
				var jobContainerSubQuery = new ZDBOnlySubQuery(typeof(ForwardingContainer), JobContainerSchema.JC_JK);

				jobContainerSubQuery.AddToFilter(filter);

				containerNumberQuery.AddSubQuery(jobContainerSubQuery, JoinCondition.And);
				return containerNumberQuery;
			}
		}

		#endregion

		#region FlightSubGroup

		ModuleFilterSubGroup FlightSubGroup
		{
			get { return flightSubGroup ?? (flightSubGroup = new FlightSubGroupImplementation()); }
		}
		ModuleFilterSubGroup flightSubGroup;

		class FlightSubGroupImplementation : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var jW_VoyageFlightFilterText = filter.LiteralTextADO.Replace(JobVoyageSchema.JV_VoyageFlight.Name, JobConsolTransportSchema.JW_VoyageFlight.Name);
				var jW_VoyageFlightFilter = new ZQuery().AddFilterAndZSQLParameterCollection(jW_VoyageFlightFilterText, new ZSqlParameterCollection());

				var flightQuery = new ZDBOnlyQuery(typeof(GenericConsol.GenericConsol));

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

		#endregion

		#region Module Filters

		protected override void AddOrganizationFilters(ModuleFilterCollection filters)
		{
			base.AddOrganizationFilters(filters);

			ModuleGuidFilter guidfilter = (ModuleGuidFilter)filters[InvoiceBulkOperationFilterHelper.CreditorFilterName];
			guidfilter.SubGroup = ConsolCostSubGroup;
			guidfilter.Visibility = FilterVisibility.AlwaysVisible;
			guidfilter.ReadOnly = true;

			ModuleFlagsFilter flagFilter = filters.AddFlagsFilter(InvoiceBulkOperationFilterHelper.IncludeConsolCostsWithNoCreditorFilterName,
				new[] { IncludeConsolCostsWithNoCreditor },
				new GetFlagsQuery[] { value => new ZQuery() });
			flagFilter.Category = Helper.ChargeLineFilters;
			flagFilter.MultilingualDescription = IncludeConsolCostsWithNoCreditorFilterDescription;

			ModuleNkFilter ownerFilter = filters.AddNkFilter(InvoiceBulkOperationFilterHelper.ConsolCostOwnerFilterName, GetConsolCostOwnerFilter, ModuleIDs.GlbStaff, Helper.Staffs);
			ownerFilter.Category = FilterCategories.Organisations;
			ownerFilter.SubGroup = ConsolCostSubGroup;
			ownerFilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.ConsolCostOwnerFilterDescription;
		}

		protected override void AddOtherOperationsFilters(ModuleFilterCollection filters)
		{
			base.AddOtherOperationsFilters(filters);

			var textFilter = (ModuleTextFilter)filters[InvoiceBulkOperationFilterHelper.ContainerNumberFilterName];
			textFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|ContainerNumber", "Container Number");
			textFilter.SubGroup = ContainerNumberSubGroup;

			textFilter = (ModuleTextFilter)filters[InvoiceBulkOperationFilterHelper.FlightFilterName];
			textFilter.SubGroup = FlightSubGroup;
		}

		protected override void AddDateFilters(ModuleFilterCollection filters)
		{
			base.AddDateFilters(filters);

			ModuleDateFilter dateFilter = (ModuleDateFilter)filters[InvoiceBulkOperationFilterHelper.PostDateFilterName];
			dateFilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.AccrualPostDateFilterDescription;
			dateFilter.SubGroup = AccrualSubGroup;

			dateFilter = (ModuleDateFilter)filters[InvoiceBulkOperationFilterHelper.ShipmentETA_ETDFilterName];
			dateFilter.MultilingualDescription = ConsolETA_ETD;

			// ATA/ATD
			AddDateFilter(filters, InvoiceBulkOperationFilterHelper.ConsolATA_ATDFilterName, InvoiceBulkOperationFilterHelper.ConsolATA_ATDFilterDescription, GetATA_ATDFilter);
			// Master Bill Issue Date 
			AddDateFilter(filters, InvoiceBulkOperationFilterHelper.MasterBillIssueDateFilterName, InvoiceBulkOperationFilterHelper.MasterBillIssueDateFilterDescription, GetMasterBillIssueDateFilter);
		}

		protected override void AddLineFilters(ModuleFilterCollection filters)
		{
			base.AddLineFilters(filters);

			ModuleGuidFilter guidfilter = (ModuleGuidFilter)filters[InvoiceBulkOperationFilterHelper.ChargeCodeFilterName];
			guidfilter.SubGroup = ConsolCostSubGroup;

			ModuleNkFilter nkfilter = (ModuleNkFilter)filters[InvoiceBulkOperationFilterHelper.CurrencyFilterName];
			nkfilter.SubGroup = ConsolCostSubGroup;

			ModuleFlagsFilter flagFilter = filters.AddFlagsFilter(InvoiceBulkOperationFilterHelper.ExcludeReverseSignedConsolCostsFilterName,
				new[] { ParentInvoice is APCreditNote ? ExcludePositiveCosts : ExcludeNegativeCosts },
				new GetFlagsQuery[] { GetExcludeReverseSignedConsolCostsFilterQuery });
			flagFilter.Category = Helper.ChargeLineFilters;
			flagFilter.MultilingualDescription = ParentInvoice is APCreditNote ? InvoiceBulkOperationFilterHelper.ExcludePositiveCostsFilterDescription : InvoiceBulkOperationFilterHelper.ExcludeNegativeCostsFilterDescription;
			flagFilter.SubGroup = ConsolCostSubGroup;

			if (PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany))
			{
				ModuleTextFilter placeOfSupplyFilter = filters.AddTextFilter("Fixed Place of Supply", JobConsolCostSchema.E6_PlaceOfSupply, PlaceOfSupplyListProvider.GetPlaceOfSupplyList(GlbCompany.CurrentCompany));
				placeOfSupplyFilter.Category = Helper.ChargeLineFilters;
				placeOfSupplyFilter.SubGroup = ConsolCostSubGroup;
				placeOfSupplyFilter.MultilingualDescription = ResString.GetMultilingualString("2a8e5f85-ede8-46f6-adfc-09ded8832180", "Fixed Place of Supply");
			}

			ModuleNumberFilter supplierCostReferenceFilter = (ModuleNumberFilter)filters[InvoiceBulkOperationFilterHelper.SupplierCostReferenceFilterName];
			supplierCostReferenceFilter.SubGroup = ConsolCostSubGroup;

			if (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value)
			{
				guidfilter = filters.AddGuidFilter(InvoiceBulkOperationFilterHelper.TaxBranchFilterName, ModuleIDs.GlbBranch, JobConsolCostSchema.E6_GB_CostTaxBranch, Helper.Branches);
				guidfilter.Category = Helper.ChargeLineFilters;
				guidfilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.TaxBranchFilterDescription;
				guidfilter.SubGroup = ConsolCostSubGroup;
				guidfilter.Visibility = FilterVisibility.AlwaysVisible;
				guidfilter.DefaultProperty = DefaultTaxBranchFilterValue;
			}
		}

		protected override IFilterStripsHelper GetWorkflowFilterStripsHelper()
		{
			ZString templateCode = "JOB";
			return ObjectFactory.Get<IInvoiceBulkOperationWorkflowFilterStripsHelper>("IInvoiceBulkOperationWorkflowFilterStripsHelper", templateCode, typeof(GenericConsol.GenericConsol), Factory, false);
		}

		#endregion

		#region Overriden Filter methods 

		protected override ZQuery GetCreditorFilter(ZGuid creditorPK)
		{
			ZQuery creditorFilter = new ZQuery(JobConsolCostSchema.E6_OH_Creditor, creditorPK);
			if (IncludeConsolCostsWithNoCreditorFilter)
			{
				creditorFilter.AddToFilter(JoinCondition.Or, JobConsolCostSchema.E6_OH_Creditor, null);
			}
			return creditorFilter;
		}

		public ZBool IncludeConsolCostsWithNoCreditorFilter
		{
			get { return ((ModuleFlagsFilter)this[InvoiceBulkOperationFilterHelper.IncludeConsolCostsWithNoCreditorFilterName]).Property0; }
		}

		protected override ZQuery GetSendingAgentFilter(ZGuid sendingAgentPK)
		{
			if (!IsChildFiltersGenerationRunning)
			{
				ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), ViewGenericConsolSchema.VX_OA_SendingForwarderAddress);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, sendingAgentPK);
				orgAddressSubQuery.AddToFilter(ViewGenericConsolSchema.VX_ParentTableCode, JobConsolSchema.Constants.Prefix);// We can add this filter because VX_OA_SendingForwarderAddress is only defined for JK in the view. See ViewGenericConsol.sql for details

				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(GenericConsol.GenericConsol));
				result.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
				return result;
			}
			else
			{
				return new ZQuery();
			}
		}

		protected override ZQuery GetReceivingAgentFilter(ZGuid receivingAgentPK)
		{
			if (!IsChildFiltersGenerationRunning)
			{
				ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), ViewGenericConsolSchema.VX_OA_ReceivingForwarderAddress);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, receivingAgentPK);
				orgAddressSubQuery.AddToFilter(ViewGenericConsolSchema.VX_ParentTableCode, JobConsolSchema.Constants.Prefix);// We can add this filter because VX_OA_ReceivingForwarderAddress is only defined for JK in the view. See ViewGenericConsol.sql for details
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(GenericConsol.GenericConsol));
				result.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
				return result;
			}
			else
			{
				return new ZQuery();
			}
		}

		protected override ZQuery GetPostDateFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			if (value1.IsValid || value2.IsValid)
			{
				return Helper.GetDateFilter(AccTransactionLinesSchema.AL_PostDate, comparisonOperator, value1, value2);
			}
			else
			{
				return new ZQuery();
			}
		}

		protected override ZQuery GetShipmentETA_ETDFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			if (!IsChildFiltersGenerationRunning && (value1.IsValid || value2.IsValid))
			{
				ZDBOnlySubQuery dateFilter = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);

				SailingFilterBuilder builderETA = new SailingFilterBuilder(Factory);
				builderETA.SetDateRange(SailingFilterBuilder.Dates.ETA, DateComparisonOperator.HasDateInRange, value1, value2);
				dateFilter.AddToFilter(builderETA.ToTransportFilter());

				SailingFilterBuilder builderETD = new SailingFilterBuilder(Factory);
				builderETD.SetDateRange(SailingFilterBuilder.Dates.ETD, DateComparisonOperator.HasDateInRange, value1, value2);
				dateFilter.AddToFilter(builderETD.ToTransportFilter(), JoinCondition.Or);

				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(GenericConsol.GenericConsol));
				result.AddSubQuery(dateFilter, JoinCondition.And);

				return result;
			}
			else
			{
				return new ZQuery();
			}
		}

		protected override ZQuery GetTransportModeFilter(ZString value)
		{
			if (!IsChildFiltersGenerationRunning)
			{
				return new ZQuery(ViewGenericConsolSchema.VX_TransportMode, value);
			}
			else
			{
				return new ZQuery();
			}
		}

		protected override ZQuery GetFlightFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!IsChildFiltersGenerationRunning)
			{
				var result = new ZDBOnlyQuery(typeof(JobVoyage));
				result.AddToFilter(JobVoyageSchema.JV_VoyageFlight, comparisonOperator, value);
				return result;
			}
			return new ZQuery();
		}

		protected override ZQuery GetContainerNumberFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!IsChildFiltersGenerationRunning)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ForwardingContainer));
				result.AddToFilter(JobContainerSchema.JC_ContainerNum, comparisonOperator, value);

				return result;
			}
			else
			{
				return new ZQuery();
			}
		}

		protected override ZQuery GetContainerModeFilter(ZString value)
		{
			if (!IsChildFiltersGenerationRunning)
			{
				var query = new ZQuery(ViewGenericConsolSchema.VX_ConsolMode, value);
				query.AddToFilter(ViewGenericConsolSchema.VX_ParentTableCode, JobConsolSchema.Constants.Prefix);// We can add this filter because VX_ConsolMode is only defined for JK in the view. See ViewGenericConsol.sql for details
				return query;
			}
			else
			{
				return new ZQuery();
			}
		}

		protected override ZQuery GetMasterBillFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			if (!IsChildFiltersGenerationRunning)
			{
				var query = new ZQuery(ViewGenericConsolSchema.VX_SecondaryCode, comparisonOperator, value);
				query.AddToFilter(ViewGenericConsolSchema.VX_ParentTableCode, JobConsolSchema.Constants.Prefix);// We can add this filter because VX_SecondaryCode is only defined for JK in the view. See ViewGenericConsol.sql for details
				return query;
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
				var query = new ZQuery(ViewGenericConsolSchema.VX_CoLoadMasterBill, comparisonOperator, value);
				query.AddToFilter(ViewGenericConsolSchema.VX_ParentTableCode, JobConsolSchema.Constants.Prefix);// We can add this filter because VX_CoLoadMasterBill is only defined for JK in the view. See ViewGenericConsol.sql for details
				return query;
			}
			else
			{
				return new ZQuery();
			}
		}

		protected override ZQuery GetChargeCodeFilter(ZGuid chargeCodePK)
		{
			return new ZQuery(JobConsolCostSchema.E6_AC_ChargeCode, chargeCodePK);
		}

		protected override ZQuery GetCurrencyFilter(ZString value)
		{
			return new ZQuery(JobConsolCostSchema.E6_RX_NKCurrency, value);
		}

		protected override ZQuery GetSupplierCostReferenceFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery().AddToFilter_PossiblyCommaSeparated(JobConsolCostSchema.E6_CostReference, comparisonOperator, value);
		}

		protected override void AddInitialAuditFilters(ModuleFilterCollection filters)
		{
		}

		#endregion

		#region New Filter Methods

		ZQuery GetConsolCostOwnerFilter(ZString value)
		{
			return new ZQuery(JobConsolCostSchema.E6_GS_NKConsolCostOwner, value);
		}

		ZQuery GetExcludeReverseSignedConsolCostsFilterQuery(ZBool value)
		{
			return value ? new ZQuery(JobConsolCostSchema.E6_OSCostAmount, ParentInvoice is APInvoice ? SQLComparisonOperator.GreaterThan : SQLComparisonOperator.LessThan, 0m)
				: new ZQuery(JobConsolCostSchema.E6_OSCostAmount, SQLComparisonOperator.NotEqual, 0m);
		}

		#endregion

		ZQuery GetATA_ATDFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			if (!IsChildFiltersGenerationRunning)
			{
				ZDBOnlySubQuery dateFilter = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);

				SailingFilterBuilder builderATA = new SailingFilterBuilder(Factory);
				builderATA.SetDateRange(SailingFilterBuilder.Dates.ATA, comparisonOperator, value1, value2);
				dateFilter.AddToFilter(builderATA.ToTransportFilter());

				SailingFilterBuilder builderATD = new SailingFilterBuilder(Factory);
				builderATD.SetDateRange(SailingFilterBuilder.Dates.ATD, comparisonOperator, value1, value2);
				dateFilter.AddToFilter(builderATD.ToTransportFilter(), JoinCondition.Or);

				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(GenericConsol.GenericConsol));
				result.AddSubQuery(dateFilter, JoinCondition.And);

				return result;
			}
			else
			{
				return new ZQuery();
			}
		}

		ZQuery GetMasterBillIssueDateFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(GenericConsol.GenericConsol));
			var parentTableCodeFilter = FormattableString.Invariant($" AND VX_ParentTableCode = '{JobConsolSchema.Constants.Prefix}'");

			if (!IsChildFiltersGenerationRunning)
			{
				if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
				{
					result.AddFilterAndZSQLParameterCollection(" VX_PK IN (SELECT JK_PK FROM dbo.JobConsol WHERE JK_MasterBillIssueDate IS NULL)" + parentTableCodeFilter, null);
				}
				else if (comparisonOperator == DateComparisonOperator.HasDateEntered)
				{
					result.AddFilterAndZSQLParameterCollection(" VX_PK IN (SELECT JK_PK FROM dbo.JobConsol WHERE JK_MasterBillIssueDate IS NOT NULL)" + parentTableCodeFilter, null);
				}
				else
				{
					var queryParams = new ZSqlParameterCollection();
					queryParams.Add("@date1", value1, JobConsolSchema.JK_MasterBillIssueDate);
					queryParams.Add("@date2", value2, JobConsolSchema.JK_MasterBillIssueDate);

					result.AddFilterAndZSQLParameterCollection(" VX_PK IN (SELECT JK_PK FROM dbo.JobConsol WHERE JK_MasterBillIssueDate >= @date1 and JK_MasterBillIssueDate < @date2)" + parentTableCodeFilter, queryParams);
				}
			}

			return result;
		}
	}
}

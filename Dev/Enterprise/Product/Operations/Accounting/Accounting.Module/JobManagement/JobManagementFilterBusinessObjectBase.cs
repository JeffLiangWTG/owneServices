using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class JobManagementFilterBusinessObjectBase : AccountingFilterStripBusinessObject, IAccountingFilterStripHolder
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddJobHeaderFilters(filters);
			AccountingFilterStrip.AddJobManagementFilters(filters, Env.Security.JobManagement);
			AddOtherOperationsFilters(filters);
			AddFlagsFilters(filters);
			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);
			return filters;
		}

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			var filter = new ModuleNumberFilter(AccountingUtils.NumberFilterTypes.JobNumber, JobHeaderSchema.JH_JobNum)
			{
				Category = JobHeaderCategory,
				MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|Job", AccountingUtils.NumberFilterTypes.JobNumber)
			};

			return filter;
		}

		protected AccountingFilterStripCreator AccountingFilterStrip
		{
			get
			{
				if (AccountingFilterStrip_innerValue == null)
				{
					AccountingFilterStrip_innerValue = new AccountingFilterStripCreator(this);
					AccountingFilterStrip_innerValue.Initialize(addAmountFilters: false, addNumbersAndReferencesFilters: false);
				}
				return AccountingFilterStrip_innerValue;
			}
		}

		AccountingFilterStripCreator AccountingFilterStrip_innerValue;

		#region Other Operations Filters

		protected FilterCategory OperationsFiltersCategory
		{
			get
			{
				if (fOperationsFiltersCategory == null)
				{
					fOperationsFiltersCategory = new FilterCategory(ResString.GetMultilingualString("Accounting|JobManagementFilter|OtherOperationsFilters", "Other Operations Filters"));
				}
				return fOperationsFiltersCategory;
			}
		}

		FilterCategory fOperationsFiltersCategory;

		protected virtual void AddOtherOperationsFilters(ModuleFilterCollection filters)
		{
			var relatedJobNumberFilter = filters.AddNumberFilter(BaseCharge.DefaultGatewayJobsFilterName, GetGatewayJobsQuery);
			relatedJobNumberFilter.Category = OperationsFiltersCategory;
			relatedJobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|GatewayJobForShipment", BaseCharge.DefaultGatewayJobsFilterName);
			relatedJobNumberFilter.MaxLength = JobShipmentSchema.JS_UniqueConsignRef.MaxLength;

			var consolNumberFilter = filters.AddNumberFilter("Consol #", GetConsolNumberQuery);
			consolNumberFilter.Category = OperationsFiltersCategory;
			consolNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|Consol", "Consol #");
			consolNumberFilter.Prefix = "C";
			consolNumberFilter.MaxLength = JobConsolSchema.JK_UniqueConsignRef.MaxLength;

			var houseBillNumberFilter = filters.AddNumberFilter("House Bill #", GetHouseBillQuery);
			houseBillNumberFilter.Category = OperationsFiltersCategory;
			houseBillNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|HouseBill", "House Bill #");
			houseBillNumberFilter.Prefix = "H";
			houseBillNumberFilter.MaxLength = JobShipmentSchema.JS_HouseBill.MaxLength;

			var masterBillFilter = filters.AddNumberFilter("Master Bill #", GetMasterBillQuery);
			masterBillFilter.Category = OperationsFiltersCategory;
			masterBillFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|MasterBill", "Master Bill #");
			masterBillFilter.Prefix = "M";
			masterBillFilter.MaxLength = JobConsolSchema.JK_MasterBillNum.MaxLength;

			var flightVoyageFilter = filters.AddTextAndNkFilter("Flight/Voyage # and Vessel", GetFlightVoyageNumberAndVesselQuery, ModuleIDs.RefVessel, BindingLists.RefVessel_List)
				.WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			flightVoyageFilter.Category = OperationsFiltersCategory;
			flightVoyageFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|FlightVoyageAndVessel", "Flight/Voyage # and Vessel");
			flightVoyageFilter.Prefix = "V";

			var customsEntryNoFilter = filters.AddNumberFilter("Customs Entry #", GetCustomsEntryNoQuery);
			customsEntryNoFilter.Category = OperationsFiltersCategory;
			customsEntryNoFilter.IsPublishedOnWeb = false;
			customsEntryNoFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|CustomsEntry", "Customs Entry #");
			customsEntryNoFilter.Prefix = "E";
			customsEntryNoFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;

			var orderNumberFilter = filters.AddNumberFilter("Order #", GetOrderNoQuery);
			orderNumberFilter.Category = OperationsFiltersCategory;
			orderNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|Order", "Order #");
			orderNumberFilter.Prefix = "O";
			orderNumberFilter.MaxLength = JobOrderHeaderSchema.JD_OrderNumber.MaxLength;

			var transportBookingReferenceFilter = filters.AddNumberFilter("Transport Booking Reference", GetTransportBookingReferenceQuery);
			transportBookingReferenceFilter.Category = OperationsFiltersCategory;
			transportBookingReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|TransportBookingReference", "Transport Booking Reference");
			transportBookingReferenceFilter.Prefix = "B";
			transportBookingReferenceFilter.MaxLength = DtbBookingSchema.KM_TransportReference.MaxLength;

			var consignmentRunsheetNumberFilter = filters.AddNumberFilter(InvoiceBulkOperationFilterHelper.RunSheetNumberFilterName, GetConsignmentRunSheetNumberQuery);
			consignmentRunsheetNumberFilter.Category = OperationsFiltersCategory;
			consignmentRunsheetNumberFilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.RunSheetNumberFilterDescription;
			consignmentRunsheetNumberFilter.Prefix = "CR";
			consignmentRunsheetNumberFilter.MaxLength = DtbConsignmentRunSheetSchema.KG_RunSheetNumber.MaxLength;

			var containerNumberFilter = filters.AddNumberFilter("Container #", GetContainerNumberQuery);
			containerNumberFilter.Category = OperationsFiltersCategory;
			containerNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|ContainerNumber", "Container #");
			containerNumberFilter.Prefix = "T";
			containerNumberFilter.MaxLength = JobContainerSchema.JC_ContainerNum.MaxLength;

			var filterGuid = filters.AddGuidFilter("Carrier/Principal", ModuleIDs.Organisation, GetCarrierQuery, BindingLists.ShippingProvider_List);
			filterGuid.Category = OperationsFiltersCategory;
			filterGuid.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|CarrierPrincipal", "Carrier/Principal");

			var localJobReferenceFilter = filters.AddNumberFilter("Job Local Reference", JobHeaderSchema.JH_JobLocalReference);
			localJobReferenceFilter.Category = OperationsFiltersCategory;
			localJobReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|LocalJobReference", "Job Local Reference");
			localJobReferenceFilter.Prefix = "L";
		}

		#endregion

		#region Container

		ZQuery GetContainerNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return AccountingUtils.GetContainerNumberQueryForJobHeader(comparisonOperator, value);
		}

		#endregion

		#region Other Operations Filters Delegates

		#region GetConsolNumberQuery

		ZQuery GetConsolNumberQuery(SQLComparisonOperator @operator, ZString consolID)
		{
			return AccountingUtils.GetConsolNumberQueryForJobHeader(@operator, consolID);
		}

		#endregion

		#region  GetRelatedJobQuery

		ZQuery GetGatewayJobsQuery(SQLComparisonOperator sqlOperator, ZString shipmentJobNum)
		{
			return AccountingUtils.GetGatewayJobsQueryForJobHeader(sqlOperator, shipmentJobNum);
		}

		#endregion

		#region GetHouseBillQuery

		ZQuery GetHouseBillQuery(SQLComparisonOperator @operator, ZString houseBill)
		{
			return AccountingUtils.GetHouseBillQueryForJobHeader(@operator, houseBill);
		}

		#endregion

		#region GetMasterBillQuery

		ZQuery GetMasterBillQuery(SQLComparisonOperator @operator, ZString masterBill)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobHeader));
			result.AddSubQuery(AccountingUtils.GetMasterBillSubQueryForJobHeader(@operator, masterBill), JoinCondition.And);
			return result;
		}

		#endregion

		#region GetFlightVoyageNumberAndVesselQuery

		protected virtual ZQuery GetFlightVoyageNumberAndVesselQuery(SQLComparisonOperator @operator, ZString flightOrVoyageNo, ZString vesselNK)
		{
			return AccountingUtils.GetFlightVoyageNumberAndVesselQueryForJobHeader(@operator, flightOrVoyageNo, vesselNK);
		}

		#endregion

		#region GetCustomsEntryNoQuery

		ZQuery GetCustomsEntryNoQuery(SQLComparisonOperator comparisonOperator, ZString customsEntryNo)
		{
			return AccountingUtils.GetCustomsEntryNumberQueryForJobHeader(comparisonOperator, customsEntryNo);
		}

		#endregion

		#region GetOrderNoQuery

		ZQuery GetOrderNoQuery(SQLComparisonOperator comparisonOperator, ZString orderNo)
		{
			return AccountingUtils.GetOrderNumberQueryForJobHeader(comparisonOperator, orderNo);
		}

		#endregion

		#region GetTransportBookingReferenceQuery

		ZQuery GetTransportBookingReferenceQuery(SQLComparisonOperator comparisonOperator, ZString orderNo)
		{
			return AccountingUtils.GetTransportBookingReferenceQueryForJobHeader(comparisonOperator, orderNo);
		}

		#endregion

		#region GetConsignmentRunSheetNumberQuery

		ZQuery GetConsignmentRunSheetNumberQuery(SQLComparisonOperator comparisonOperator, ZString runsheetNumber)
		{
			return AccountingUtils.GetConsignmentRunsheetQueryForJobHeader(comparisonOperator, runsheetNumber);
		}

		#endregion

		#region GetCarrierQuery

		ZQuery GetCarrierQuery(ZGuid carrierPK)
		{
			return AccountingUtils.GetCarrierQueryForJobHeader(carrierPK);
		}

		#endregion

		#endregion

		#region Flags Filter

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			filters.AddFlagsFilter("No Recognized Date", new string[] { Res.GetString("Accounting|JobManagementFilter|NoRecognizedDate", "No Recognized Date") }, new GetFlagsQuery[] { GetNoRecognizedDateQuery }).MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|NoRecognizedDate", "No Recognized Date");
		}

		ZQuery GetNoRecognizedDateQuery(ZBool value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobHeader));
			if (value)
			{
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(JobChargeRevRecognition), JobChargeRevRecognitionSchema.D3_JH, true);
				subQuery.AddToFilter(JobChargeRevRecognitionSchema.D3_RecognitionDate, SQLComparisonOperator.LessThan, AccountingConstants.RevenueRecognitionDateConstants.MinSpecialDate);
				query.AddSubQuery(subQuery, JoinCondition.And);
			}
			return query;
		}

		#endregion

		#region Job Header Filter

		void AddJobHeaderFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddGuidFilter("Job Local Client", ModuleIDs.Organisation, GetLocalClientQuery, BindingLists.OrgHeader_List);
			filter.Category = JobHeaderCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|JobLocalClient", "Job Local Client");

			filter = filters.AddTextRangeFilter("Job Number Range", GetJobNumberRangeQuery);
			filter.Category = JobHeaderCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|JobNumberRange", "Job Number Range");

			filter = filters.AddGuidFilter("Job Overseas Agent", ModuleIDs.Organisation, GetOverseasAgentQuery, BindingLists.OrgHeader_List);
			filter.Category = JobHeaderCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|JobOverseasAgent", "Job Overseas Agent");

			filter = filters.AddTextFilter("Job Status Hold Reason", JobHeaderSchema.JH_HoldReason);
			filter.Category = JobHeaderCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|JobStatusHoldReason", "Job Status Hold Reason");
		}

		FilterCategory fJobHeaderCategory;

		protected FilterCategory JobHeaderCategory
		{
			get
			{
				if (fJobHeaderCategory == null)
				{
					fJobHeaderCategory = new FilterCategory(ResString.GetMultilingualString("Accounting|JobManagementFilter|JobHeader", "Job Header"));
				}
				return fJobHeaderCategory;
			}
		}

		#endregion

		#region Job Header Filter Delegates

		#region Job Local Client Query

		ZQuery GetLocalClientQuery(ZGuid localClientPK)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobHeader));

			if (localClientPK.IsValid)
			{
				ZDBOnlySubQuery addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
				addressQuery.AddToFilter(OrgAddressSchema.OA_OH, localClientPK);
				query.AddSubQuery(addressQuery, JoinCondition.And);
			}

			return query;
		}

		#endregion

		#region Overseas Agent Query

		ZQuery GetOverseasAgentQuery(ZGuid overseasAgentPK)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobHeader));

			if (overseasAgentPK.IsValid)
			{
				ZDBOnlySubQuery addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_AgentCollectAddr);
				addressQuery.AddToFilter(OrgAddressSchema.OA_OH, overseasAgentPK);
				query.AddSubQuery(addressQuery, JoinCondition.And);
			}

			return query;
		}

		#endregion

		ZQuery GetJobNumberRangeQuery(ZString fromNumber, ZString toNumber)
		{
			var query = new ZQuery();

			if (!fromNumber.IsEmpty)
			{
				query.AddToFilter(JobHeaderSchema.JH_JobNum, SQLComparisonOperator.GreaterThanOrEqualTo, fromNumber);
			}

			if (!toNumber.IsEmpty)
			{
				query.AddToFilter(JobHeaderSchema.JH_JobNum, SQLComparisonOperator.LessThanOrEqualTo, toNumber);
			}

			return query;
		}

		#endregion

		#region Filter Overrides

		protected string MissingInvalidJobParentQuery = "NOT EXISTS (SELECT 1 FROM dbo.ViewGenericJob WHERE VJ_PK = JH_ParentID)";

		public override ZQuery Filter
		{
			get
			{
				ZQuery result = base.Filter;
				GetAdditionalQueryThatAlwaysNeedsToAppended(result);
				return result;
			}
		}

		protected void GetAdditionalQueryThatAlwaysNeedsToAppended(ZQuery baseQuery)
		{
			var nonSpotQuoteQuery = new ZQuery(JobHeaderSchema.JH_ParentTableCode, SQLComparisonOperator.NotEqual, RatingHeaderSchema.Constants.Prefix);

			var currentLoginCompanyQuery = new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			if (!baseQuery.FilterString.Contains(MissingInvalidJobParentQuery))
			{
				baseQuery.AddToFilter(GetNonForwardingConsolsOnlyQuery());
			}

			baseQuery.AddToFilter(nonSpotQuoteQuery);
			baseQuery.AddToFilter(NonTransportBookingQuoteQuery());
			baseQuery.AddToFilter(currentLoginCompanyQuery);
		}

		ZQuery GetNonForwardingConsolsOnlyQuery()
		{
			var nonForwardingConsolsOnlyQuery = new ZQuery();
			var consolQuery = new ZDBOnlyQuery(typeof(JobHeader));
			var consolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobHeaderSchema.JH_ParentID);
			consolSubQuery.AddToFilter(JobConsolSchema.JK_IsCFS, true);
			consolQuery.AddSubQuery(consolSubQuery, JoinCondition.Or);
			consolQuery.AddToFilter(new ZQuery(JobHeaderSchema.JH_ParentTableCode, SQLComparisonOperator.NotEqual, JobConsolSchema.Constants.Prefix), JoinCondition.Or);
			nonForwardingConsolsOnlyQuery.AddToFilter(consolQuery);

			var gatewayBillingSubQuery = new ZQuery(JobHeaderSchema.JH_JobNum, SQLComparisonOperator.EndsWith, Core.Constants.GatewaySuffixForJobHeaderDeprecated); /*the GW Suffix is deprecated*/
			gatewayBillingSubQuery.AddToFilter(JobHeaderSchema.JH_ParentTableCode, SQLComparisonOperator.Equal, JobConsolSchema.Constants.Prefix);

			var gatewayBillingGCN = new ZDBOnlyQuery(typeof(JobHeader));
			var gatewayBillingGCNSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobHeaderSchema.JH_ParentID);
			gatewayBillingGCNSubQuery.AddToFilter(JobConsolSchema.JK_SendingForwarderHandlingType, new[] { AgentStatusList.Codes.GatewayAgent, AgentStatusList.Codes.GatewayAgentWithTariff });
			gatewayBillingGCNSubQuery.AddToFilter(JoinCondition.Or, JobConsolSchema.JK_ReceivingForwarderHandlingType, new[] { AgentStatusList.Codes.GatewayAgent, AgentStatusList.Codes.GatewayAgentWithTariff });
			gatewayBillingGCNSubQuery.AddToFilter(JobConsolSchema.JK_IsForwarding, true);
			gatewayBillingGCNSubQuery.AddToFilter(JobConsolSchema.JK_IsCFS, false);

			gatewayBillingGCN.AddSubQuery(gatewayBillingGCNSubQuery, JoinCondition.And);

			nonForwardingConsolsOnlyQuery.AddToFilter(gatewayBillingSubQuery, JoinCondition.Or);
			nonForwardingConsolsOnlyQuery.AddToFilter(gatewayBillingGCN, JoinCondition.Or);

			return nonForwardingConsolsOnlyQuery;
		}

		#region NonTransportBookingQuoteQuery

		ZQuery NonTransportBookingQuoteQuery()
		{
			var query = new ZQuery();

			// DtbBookingConsolidation.KB_JobType <> 'QTE' 
			var jhDBOnlyQuery = new ZDBOnlyQuery(typeof(JobHeader));
			var bookingSubQuery = new ZDBOnlySubQuery(typeof(IDtbBooking), DtbBookingSchema.PK);
			var consolSubQuery = new ZDBOnlySubQuery(typeof(IDtbBookingConsolidation), DtbBookingConsolidationSchema.PK);
			consolSubQuery.AddToFilter(DtbBookingConsolidationSchema.KB_JobType, SQLComparisonOperator.NotEqual, TransportConsolidationJobTypes.Codes.QuotedBooking);
			bookingSubQuery.AddSubQuery(DtbBookingSchema.KM_KB_Booking, consolSubQuery, JoinCondition.And);
			jhDBOnlyQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, bookingSubQuery, JoinCondition.And);

			// JobHeader.JH_ParentTableCode <> 'KM'
			var jhQuery = new ZQuery(JobHeaderSchema.JH_ParentTableCode, SQLComparisonOperator.NotEqual, DtbBookingSchema.Constants.Prefix);

			query.AddToFilter(jhDBOnlyQuery);
			query.AddToFilter(jhQuery, JoinCondition.Or);

			return query;
		}

		#endregion

		protected override bool IsActiveStatusFilterAlwaysApplied() => true;

		#endregion

		#region Filter Defaults

		protected override void SetExternalDefaultsCore(FilterBusinessObjectDefault filterDefault, IEnumerable<ZString> skippedOrCategory = null)
		{
			if ((filterDefault.Value is ZString) && (this[filterDefault.FilterName] is ModuleTextAndNkFilter))
			{
				ModuleTextAndNkFilter moduleFilter = (ModuleTextAndNkFilter)this[filterDefault.FilterName];
				if (moduleFilter != null)
				{
					ZString[] values = ((ZString)filterDefault.Value).Split('/');
					moduleFilter.Visibility = FilterVisibility.AlwaysVisible;
					moduleFilter[filterDefault.PropertyName] = values[0];
					if (values.Length > 1)
					{
						moduleFilter.NkProperty = values[1];
					}
				}
				else
				{
					string message = string.Format((NoResString)"Setting defaults for FilterBizO '{0}'. Filter '{1}' with property '{2}' not found.", GetType().FullName, filterDefault.FilterName, filterDefault.PropertyName);
					Globals.Message.ShowDeveloperException(new BadExternalDefaultException(message, null));
				}
			}
			else
			{
				base.SetExternalDefaultsCore(filterDefault);
			}
		}

		#endregion

		#region Lists

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

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

		#endregion

		#region IAccountingFilterStripHolder Members

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => false;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
		{
			{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("Accounting|JobManagementFilter|JobStatus", "Job Status") }
		};

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			return billingPKSubQuery;
		}

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories
		{
			get { return null; }
		}

		#endregion

		readonly JobManagementCRMSecurityProvider SecurityProvider = new JobManagementCRMSecurityProvider();
	}
}

using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GenericConsol;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public class InvoicingBaseApprovalFilterBusinessObject : TransactionApprovalFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			AddOperationalFilters(filters);
			return filters;
		}

		void AddOperationalFilters(ModuleFilterCollection filters)
		{
			FilterCategory operationsFiltersCategory = new FilterCategory(ResString.GetMultilingualString("Accounting|TransactionApprovalFilter|OperationsFilters", "Operations Filters"));

			ModuleFilter filter = filters.AddNumberFilter("House Bill #", GetHouseBillQuery);
			filter.Category = operationsFiltersCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionApprovalFilter|HouseBill", "House Bill #");
			filter.MaxLength = JobShipmentSchema.JS_HouseBill.MaxLength;

			filter = filters.AddNumberFilter("Master Bill #/Ocean Bill #", GetMasterBillQuery);
			filter.Category = operationsFiltersCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionApprovalFilter|MasterBillOceanBill", "Master Bill #/Ocean Bill #");
			filter.MaxLength = JobConsolSchema.JK_MasterBillNum.MaxLength;

			filter = filters.AddNumberFilter("Job Local Reference", GetLocalJobReferenceQuery);
			filter.Category = operationsFiltersCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|JobManagementFilter|LocalJobReference", "Job Local Reference");
			filter.MaxLength = JobHeaderSchema.JH_JobLocalReference.MaxLength;

			filter = filters.AddTextAndNkFilter("Flight/Voyage # and Vessel", GetFlightVoyageNumberAndVesselQuery, ModuleIDs.RefVessel, BindingLists.RefVessel_List)
				.WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			filter.Category = operationsFiltersCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionApprovalFilter|FlightVoyageAndVessel", "Flight/Voyage # and Vessel");

			ModuleNumberFilter customsEntryNoFilter = filters.AddNumberFilter("Customs Entry #", GetCustomsEntryNoQuery);
			customsEntryNoFilter.Category = operationsFiltersCategory;
			customsEntryNoFilter.IsPublishedOnWeb = false;
			customsEntryNoFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionApprovalFilter|CustomsEntry", "Customs Entry #");
			customsEntryNoFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;

			ModuleNumberFilter orderNoFilter = filters.AddNumberFilter("Order #", GetOrderNoQuery);
			orderNoFilter.Category = operationsFiltersCategory;
			orderNoFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionApprovalFilter|Order", "Order #");
			orderNoFilter.MaxLength = JobOrderHeaderSchema.JD_OrderNumber.MaxLength;

			filter = filters.AddGuidFilter("Carrier", ModuleIDs.Organisation, GetCarrierQuery, BindingLists.ShippingProvider_List);
			filter.Category = operationsFiltersCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionApprovalFilter|Carrier", "Carrier");

			filter = filters.AddTextFilter(Business.AccountingUtils.NumberFilterTypes.JobNumber, JobNumberQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			GetJobFilterMultilingualDescription(filter);
			filter.MaxLength = JobHeaderSchema.JH_JobNum.MaxLength;
		}

		protected virtual void GetJobFilterMultilingualDescription(ModuleFilter filter)
		{
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|TransactionApprovalFilter|JobNumber", "Job #");
		}

		BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#region GetLocalJobReferenceQuery

		ZQuery GetLocalJobReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			if (!value.IsEmpty)
			{
				ZDBOnlyQuery selectAccTransHeaderQuery = new ZDBOnlyQuery(typeof(GenApprovalRequest));
				ZDBOnlySubQuery selectJobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), GenApprovalRequestSchema.XP_ParentID);
				selectJobHeaderQuery.AddToFilter_PossiblyCommaSeparated(JobHeaderSchema.JH_JobLocalReference, comparisonOperator, value);
				selectJobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				selectAccTransHeaderQuery.AddSubQuery(selectJobHeaderQuery, JoinCondition.And);
				query.AddToFilter(selectAccTransHeaderQuery, JoinCondition.And);
			}

			return query;
		}

		#endregion

		ZQuery GetFlightVoyageNumberAndVesselQuery(SQLComparisonOperator @operator, ZString flightOrVoyageNo, ZString vesselNK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(GenApprovalRequest));
			ZDBOnlySubQuery jobSubQuery = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			ZDBOnlySubQuery conShipSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			ZDBOnlySubQuery consolTransportSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			ZDBOnlySubQuery sailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			ZDBOnlySubQuery voyDestinationSubQuery = new ZDBOnlySubQuery(typeof(AutoJobVoyDestination), JobVoyDestinationSchema.PK);
			ZDBOnlySubQuery voySubQuery = new ZDBOnlySubQuery(typeof(AutoJobVoyage), JobVoyageSchema.PK);

			if (vesselNK != "")
			{
				voySubQuery.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, @operator, vesselNK);
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
				consolTransportSubQuery2.AddToFilter(JobConsolTransportSchema.JW_Vessel, @operator, vesselNK);
			}
			consolTransportSubQuery2.AddToFilter(JobConsolTransportSchema.JW_VoyageFlight, @operator, flightOrVoyageNo);
			conShipSubQuery2.AddSubQuery(JobConShipLinkSchema.JN_JK, consolTransportSubQuery2, JoinCondition.And);
			shipmentSubQuery2.AddToFilter(JobShipmentSchema.JS_IsShipping, ZBool.False);
			shipmentSubQuery2.AddSubQuery(conShipSubQuery2, JoinCondition.And);
			jobSubQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery2, JoinCondition.Or);
			result.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, jobSubQuery, JoinCondition.And);

			ZDBOnlySubQuery jobSubQuery4 = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);
			ZDBOnlySubQuery shipmentSubQuery4 = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			ZDBOnlySubQuery sailingSubQuery4 = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			ZDBOnlySubQuery voyDestinationSubQuery4 = new ZDBOnlySubQuery(typeof(AutoJobVoyDestination), JobVoyDestinationSchema.PK);
			ZDBOnlySubQuery voySubQuery4 = new ZDBOnlySubQuery(typeof(AutoJobVoyage), JobVoyageSchema.PK);
			if (vesselNK != "")
			{
				voySubQuery4.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, @operator, vesselNK);
			}
			shipmentSubQuery4.AddToFilter(JobShipmentSchema.JS_IsShipping, ZBool.True);
			voySubQuery4.AddToFilter(JobVoyageSchema.JV_VoyageFlight, @operator, flightOrVoyageNo);
			voyDestinationSubQuery4.AddSubQuery(JobVoyDestinationSchema.JB_JV, voySubQuery4, JoinCondition.And);
			sailingSubQuery4.AddSubQuery(JobSailingSchema.JX_JB, voyDestinationSubQuery4, JoinCondition.And);
			shipmentSubQuery4.AddSubQuery(JobShipmentSchema.JS_JX, sailingSubQuery4, JoinCondition.And);
			jobSubQuery4.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery4, JoinCondition.And);
			result.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, jobSubQuery4, JoinCondition.Or);
			return result;
		}

		ZQuery GetCustomsEntryNoQuery(SQLComparisonOperator comparisonOperator, ZString customsEntryNo)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(GenApprovalRequest));
			ZDBOnlySubQuery jobSubQuery = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			shipmentSubQuery.AddToFilter(ForwardingShipmentFilterProvider.GetJobShipmentFromEntryNumber(comparisonOperator, customsEntryNo));
			jobSubQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery, JoinCondition.And);

			ZDBOnlySubQuery declarationSubQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.PK);
			declarationSubQuery.AddToFilter(ObjectFactory.Get<ICustomsFilterProvider>().GetJobDeclarationFromEntryNumber(comparisonOperator, customsEntryNo));
			jobSubQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, declarationSubQuery, JoinCondition.Or);

			result.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, jobSubQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetOrderNoQuery(SQLComparisonOperator comparisonOperator, ZString orderNo)
		{
			ZQuery result = new ZQuery();
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(GenApprovalRequest));
			ZDBOnlySubQuery jobSubQuery = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobShipmentSchema.PK);
			ZDBOnlySubQuery orderSubQuery1 = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.JD_JS);
			orderSubQuery1.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumber, comparisonOperator, orderNo);
			shipmentSubQuery.AddSubQuery(orderSubQuery1, JoinCondition.And);
			jobSubQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery, JoinCondition.And);

			ZDBOnlySubQuery orderSubQuery2 = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.JD_JE);
			ZDBOnlySubQuery declarationSubQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.PK);
			orderSubQuery2.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumber, comparisonOperator, orderNo);
			declarationSubQuery.AddSubQuery(orderSubQuery2, JoinCondition.And);

			jobSubQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, declarationSubQuery, JoinCondition.Or);
			dbOnlyQuery.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, jobSubQuery, JoinCondition.And);
			result.AddToFilter(dbOnlyQuery);
			return result;
		}

		ZQuery GetHouseBillQuery(SQLComparisonOperator @operator, ZString houseBill)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(GenApprovalRequest));
			ZDBOnlySubQuery jobSubQuery = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			shipmentSubQuery.AddToFilter_PossiblyCommaSeparated(JobShipmentSchema.JS_HouseBill, @operator, houseBill);
			jobSubQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery, JoinCondition.And);
			result.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, jobSubQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetMasterBillQuery(SQLComparisonOperator @operator, ZString masterBill)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(GenApprovalRequest));
			result.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, AccountingUtils.GetMasterBillSubQueryForJobHeader(@operator, masterBill), JoinCondition.And);
			return result;
		}

		ZQuery GetCarrierQuery(ZGuid carrierPK)
		{
			ZQuery result = new ZQuery();
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(GenApprovalRequest));
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
			dbOnlyQuery.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, jobSubQuery1, JoinCondition.And);
			result.AddToFilter(dbOnlyQuery);
			return result;
		}

		ZQuery JobNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(GenApprovalRequest));

			var jobSubQuery = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);
			jobSubQuery.AddToFilter(JobHeaderSchema.JH_JobNum, comparisonOperator, value);

			var transactionHeaderSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);

			//Job # filter max length is 35 (JH_JobNum max length) and AH_TransactionNum max length is 38
			//So value.Length <= AccTransactionHeaderSchema.AH_TransactionNum.MaxLength will always be true
			//This check is needed if there comes a situation where Job # filter max length > AH_TransactionNum max length
			if (value.Length <= AccTransactionHeaderSchema.AH_TransactionNum.MaxLength)
			{
				transactionHeaderSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, comparisonOperator, value);
			}
			var comparator = new SQLComparisonOperator[3]
			{
				SQLComparisonOperator.NotEqual,
				SQLComparisonOperator.NotContains,
				SQLComparisonOperator.DoesNotStartWith
			};
			transactionHeaderSubQuery.AddSubQuery(AccTransactionHeaderSchema.AH_JH, jobSubQuery, Array.IndexOf(comparator, comparisonOperator) >= 0 ? JoinCondition.And : JoinCondition.Or);
			jobSubQuery.AddAsUnionQuery(transactionHeaderSubQuery, true);

			if (value.Length <= ViewGenericConsolSchema.VX_Code.MaxLength)
			{
				var consolSubQuery = new ZDBOnlySubQuery(typeof(GenericConsol), ViewGenericConsolSchema.PK);
				consolSubQuery.AddToFilter(ViewGenericConsolSchema.VX_Code, comparisonOperator, value);
				jobSubQuery.AddAsUnionQuery(consolSubQuery, true);
			}

			result.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, jobSubQuery, JoinCondition.And);

			var childSubQuery = new ZDBOnlySubQuery(typeof(GenApprovalRequest), GenApprovalRequestSchema.PK);
			childSubQuery.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, jobSubQuery, JoinCondition.And);

			result.AddSubQuery(GenApprovalRequestSchema.XP_ParentID, childSubQuery, JoinCondition.Or);

			return result;
		}
	}
}

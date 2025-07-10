using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module.AirCargo
{
	public class AUCustomsAirCargoFilterStripBusinessObject : AUCustomsHouseAirCargoFilterBusinessObject, IAccountingFilterStripHolder
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = base.GetModuleFiltersCore();
			AccountingFilterStrip.AddJobManagementFilters(filters, Env.Security.ACAMasterImportJobInvoicing);

			return filters;
		}

		#region Number Filter Overrides

		protected override void AddJobNumberQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZQuery hAWBQuery = new ZQuery();
			base.AddJobNumberQuery(hAWBQuery, @operator, value);
			AddMAWBFilterFromHAWBFilter(query, hAWBQuery);
		}

		protected override void AddMasterBillQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter_PossiblyCommaSeparated(CusMAWBSchema.CM_MAWB, @operator, value);
		}

		protected override void AddConsolNumberQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(CusMAWBSchema.CM_JK, SQLComparisonOperator.Equal, GetConsolPK((ZString)value));
		}

		protected override void AddFlightNoQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter_PossiblyCommaSeparated(CusMAWBSchema.CM_FlightNo, @operator, value);
			ZDBOnlyQuery joinQuery = new ZDBOnlyQuery(typeof(CusMAWB));
			ZDBOnlySubQuery underbondQuery = UnderbondQueryForFlightNo(@operator, value);
			joinQuery.AddSubQuery(underbondQuery, JoinCondition.Or);

			ZDBOnlySubQuery hAWBQuery = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.CS_CM);
			hAWBQuery.AddSubQuery(underbondQuery, JoinCondition.Or);
			joinQuery.AddSubQuery(hAWBQuery, JoinCondition.Or);
			query.AddToFilter(joinQuery, JoinCondition.Or);
		}

		protected override void AddCoLoadMasterQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZQuery hAWBQuery = new ZQuery();
			base.AddCoLoadMasterQuery(hAWBQuery, @operator, value);
			AddMAWBFilterFromHAWBFilter(query, hAWBQuery);
		}

		protected override void AddHAWBQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZQuery hAWBQuery = new ZQuery();
			base.AddHAWBQuery(hAWBQuery, @operator, value);
			AddMAWBFilterFromHAWBFilter(query, hAWBQuery);
		}

		protected override void AddConRefQuery(ZQuery query, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var mAWBQuery = new ZDBOnlyQuery(typeof(CusMAWB));
			var hAWBQuery = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.CS_CM);
			base.AddConRefQuery(hAWBQuery, comparisonOperator, value);
			mAWBQuery.AddSubQuery(hAWBQuery, JoinCondition.And);
			query.AddToFilter(mAWBQuery);
		}

		#endregion

		#region Organisation Filter Overrides

		protected override void AddConsigneeQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddHousebillSubQueryForOrgHeader(CusHAWBSchema.CS_OA_ConsigneeAddress, query, @operator, value);
		}

		protected override void AddConsignorQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddHousebillSubQueryForOrgHeader(CusHAWBSchema.CS_OA_ConsignorAddress, query, @operator, value);
		}

		#endregion

		#region Location Filter Overrides

		protected override void AddLoadQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(CusMAWBSchema.CM_RL_NKLoadPort, @operator, value);
		}

		protected override void AddDischageQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(CusMAWBSchema.CM_RL_NKDischargePort, @operator, value);
		}

		protected override void AddOriginQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddHousebillSubQuery(CusHAWBSchema.CS_RL_NKOrigin, query, @operator, value);
		}

		protected override void AddDestinationQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddHousebillSubQuery(CusHAWBSchema.CS_RL_NKDestination, query, @operator, value);
		}

		#endregion

		#region Status Filter Overrides

		protected override void AddFirstLandingQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZQuery hAWBQuery = new ZQuery();
			base.AddFirstLandingQuery(hAWBQuery, @operator, value);
			AddMAWBFilterFromHAWBFilter(query, hAWBQuery);
		}

		protected override void AddCMRCustomsStatusQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZQuery hAWBQuery = new ZQuery();
			base.AddCMRCustomsStatusQuery(hAWBQuery, @operator, value);
			AddMAWBFilterFromHAWBFilter(query, hAWBQuery);
		}

		protected override void AddCMRMessageStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZQuery hAWBQuery = new ZQuery();
			base.AddCMRMessageStatusFilter(hAWBQuery, @operator, value);
			AddMAWBFilterFromHAWBFilter(query, hAWBQuery);
		}

		protected override void AddLastLandingQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZQuery hAWBQuery = new ZQuery();
			base.AddLastLandingQuery(hAWBQuery, @operator, value);
			AddMAWBFilterFromHAWBFilter(query, hAWBQuery);
		}

		protected override bool SupportsMasterAndHouse
		{
			get { return true; }
		}

		protected override void AddUnderbondStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlySubQuery entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryStatus, @operator, value);
			entryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumber.EntryType.UnderbondStatus);

			ZDBOnlySubQuery underbondQuery = new ZDBOnlySubQuery(typeof(CusUnderbond), CusUnderbondSchema.C4_ParentID);
			underbondQuery.AddSubQuery(entryNumQuery, JoinCondition.And);
			underbondQuery.AddToFilter(JoinCondition.Or, CusUnderbondSchema.C4_Status, @operator, value);

			ZDBOnlySubQuery mAWBQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusMAWBSchema.PK);
			mAWBQuery.AddSubQuery(underbondQuery, JoinCondition.And);

			ZDBOnlySubQuery hAWBQuery = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.CS_CM);
			hAWBQuery.AddSubQuery(underbondQuery, JoinCondition.And);

			ZDBOnlyQuery mAWBOrHAWBQuery = new ZDBOnlyQuery(typeof(CusMAWB));
			mAWBOrHAWBQuery.AddSubQuery(hAWBQuery, JoinCondition.Or);
			mAWBOrHAWBQuery.AddSubQuery(mAWBQuery, JoinCondition.Or);

			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusMAWB));
			dBQuery.AddToFilter(mAWBOrHAWBQuery, JoinCondition.And);
			query.AddToFilter(dBQuery);
		}

		protected override void AddUnderbondMasterStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlySubQuery entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryStatus, @operator, value);
			entryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumber.EntryType.UnderbondStatus);

			ZDBOnlySubQuery underbondQuery = new ZDBOnlySubQuery(typeof(CusUnderbond), CusUnderbondSchema.C4_ParentID);
			underbondQuery.AddSubQuery(entryNumQuery, JoinCondition.And);
			underbondQuery.AddToFilter(JoinCondition.Or, CusUnderbondSchema.C4_Status, @operator, value);

			ZDBOnlySubQuery mAWBQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusMAWBSchema.PK);
			mAWBQuery.AddSubQuery(underbondQuery, JoinCondition.And);

			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusMAWB));
			dBQuery.AddSubQuery(mAWBQuery, JoinCondition.And);
			query.AddToFilter(dBQuery);
		}

		protected override void AddOutturnStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlySubQuery entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryStatus, @operator, value);
			entryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumber.EntryType.OutturnStatus);

			ZDBOnlySubQuery underbondQuery = new ZDBOnlySubQuery(typeof(CusUnderbond), CusUnderbondSchema.C4_ParentID);
			underbondQuery.AddSubQuery(entryNumQuery, JoinCondition.And);

			ZDBOnlySubQuery mAWBQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusMAWBSchema.PK);
			mAWBQuery.AddSubQuery(underbondQuery, JoinCondition.And);

			ZDBOnlySubQuery hAWBQuery = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.CS_CM);
			hAWBQuery.AddSubQuery(underbondQuery, JoinCondition.And);

			ZDBOnlyQuery mAWBOrHAWBQuery = new ZDBOnlyQuery(typeof(CusMAWB));
			mAWBOrHAWBQuery.AddSubQuery(hAWBQuery, JoinCondition.Or);
			mAWBOrHAWBQuery.AddSubQuery(mAWBQuery, JoinCondition.Or);

			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusMAWB));
			dBQuery.AddToFilter(mAWBOrHAWBQuery, JoinCondition.And);
			query.AddToFilter(dBQuery);
		}

		protected override void AddOutturnMasterStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlySubQuery entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryStatus, @operator, value);
			entryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumber.EntryType.OutturnStatus);

			ZDBOnlySubQuery underbondQuery = new ZDBOnlySubQuery(typeof(CusUnderbond), CusUnderbondSchema.C4_ParentID);
			underbondQuery.AddSubQuery(entryNumQuery, JoinCondition.And);

			ZDBOnlySubQuery mAWBQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusMAWBSchema.PK);
			mAWBQuery.AddSubQuery(underbondQuery, JoinCondition.And);

			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusMAWB));
			dBQuery.AddSubQuery(mAWBQuery, JoinCondition.And);
			query.AddToFilter(dBQuery);
		}

		#endregion

		#region CMROnly Filter Overrides

		protected override void AddCMROnlyQuery(ZString applicationCode, SQLComparisonOperator comparisonOperator, ZQuery query)
		{
			query.AddToFilter(JoinCondition.And, CusMAWBSchema.CM_ApplicationCode, SQLComparisonOperator.Equal, applicationCode);

			ZDBOnlyQuery noApplicationCodeQuery = new ZDBOnlyQuery(typeof(CusMAWB));
			noApplicationCodeQuery.AddToFilter(JoinCondition.And, CusMAWBSchema.CM_ApplicationCode, SQLComparisonOperator.Equal, ZString.Empty);
			noApplicationCodeQuery.AddToFilter(JoinCondition.And, CusMAWBSchema.CM_ArrivalDate, comparisonOperator, Core.Constants.AUCustoms.CMRImportsCutOverDate);
			query.AddToFilter(noApplicationCodeQuery, JoinCondition.Or);
		}

		#endregion

		#region Arrival Date Filters Implementation Overrides

		protected override void AddArrivalDateQuery(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			AddDateRange(query, comparisonOperator, JoinCondition.And, CusMAWBSchema.CM_ArrivalDate, fromDate.Date, toDate.Date);
		}

		#endregion

		#region CTOMAWB Filter Overrides

		protected override ZQuery CTOMAWBQuery
		{
			get
			{
				ZQuery result = new ZQuery(CusMAWBSchema.CM_IsCTOMAWB, ZBool.False);
				result.AddToFilter(CusMAWBSchema.CM_ApplicationCode, CusMAWBBase.Loader.CMRApplicationCodes);
				result.AddToFilter(CusMAWBSchema.CM_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
				return result;
			}
		}

		#endregion

		#region Custom Fields Filter Overrides

		protected override WorkflowFilterStripsHelper GetWorkflowFilterStripHelper() => new WorkflowFilterStripsHelper(typeof(CusMAWB), JobInvoicingConsumerTypes.CusMAWB.Code, Factory);

		#endregion

		#region Implementation

		protected void AddMAWBFilterFromHAWBFilter(ZQuery query, ZQuery hAWBQuery)
		{
			ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(CusMAWB));
			ZDBOnlySubQuery hAWBSubQuery = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.CS_CM);
			hAWBSubQuery.AddToFilter(hAWBQuery);
			dBOnlyQuery.AddSubQuery(hAWBSubQuery, JoinCondition.And);
			query.AddToFilter(dBOnlyQuery);
		}

		protected void AddHousebillSubQuery(SchemaColumn col, ZQuery mainQuery, SQLComparisonOperator @operator, object value)
		{
			if (!((IZType)value).IsEmpty)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CusMAWB));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.CS_CM);
				subQuery.AddToFilter(col, @operator, value);
				query.AddSubQuery(subQuery, JoinCondition.And);
				mainQuery.AddToFilter(query);
			}
		}

		protected void AddHousebillSubQueryForOrgHeader(SchemaColumn col, ZQuery mainQuery, SQLComparisonOperator @operator, object value)
		{
			if (!((IZType)value).IsEmpty)
			{
				var query = new ZDBOnlyQuery(typeof(CusMAWB));
				var addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), col);
				addressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, @operator, value);
				var hawbSubQuery = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.CS_CM);
				hawbSubQuery.AddSubQuery(addressSubQuery, JoinCondition.And);
				query.AddSubQuery(hawbSubQuery, JoinCondition.And);
				mainQuery.AddToFilter(query);
			}
		}

		#endregion

		#region IAccountingFilterStripHolder Members

		IAccountingFilterStrip AccountingFilterStrip
		{
			get
			{
				return accountingFilterStrip_innerValue ?? (accountingFilterStrip_innerValue =
					(IAccountingFilterStrip)Activator.CreateInstance(ObjectFactory.GetType<IAccountingFilterStrip>(), this));
			}
		}
		IAccountingFilterStrip accountingFilterStrip_innerValue;

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			ZDBOnlyQuery mawbQuery = new ZDBOnlyQuery(typeof(CusMAWB));
			mawbQuery.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return mawbQuery;
		}

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => null;

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

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
	}
}

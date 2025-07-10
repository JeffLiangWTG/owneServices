using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public class AirCargoOutturnBillsFilterBusinessObject : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddNumberFilters(filters);
			AddDateFilters(filters);
			AddStatusFilters(filters);

			AccountingFilterStrip.AddJobManagementFilters(filters, Env.Security.ACAOutturnBillsJobInvoicing);

			return filters;
		}

		public override ZQuery Filter
		{
			get
			{
				var result = base.Filter;
				result.AddToFilter(CusUnderbondSchema.C4_ApplicationCode, Enterprise.Customs.Business.CusUnderbondApplicationCodeList.Codes.AUUnderbond);
				return result;
			}
		}

		#region Number Filter

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(FilterConstants.NumberTypes.SendersRef, CusUnderbondSchema.C4_SendersMessageReference);
			var outturnSendersRefFilter = filters.AddNumberFilter(FilterConstants.NumberTypes.OutturnSendersRef, GetOutturnSendersRefNumberQuery);
			outturnSendersRefFilter.MaxLength = CusHAWB.Schema.CS_MessageReferenceMaxLength;
			var houseBillFilter = filters.AddNumberFilter(FilterConstants.NumberTypes.Housebill, GetOutturnHouseBillNumberQuery);
			houseBillFilter.MaxLength = CusHAWB.Schema.CS_HAWBMaxLength;
			var masterBillFilter = filters.AddNumberFilter(FilterConstants.NumberTypes.Masterbill, GetOutturnMasterBillNumberQuery);
			masterBillFilter.MaxLength = CusMAWB.Schema.CM_MAWBMaxLength;
			filters.AddNumberFilter(FilterConstants.NumberTypes.Flight, CusUnderbondSchema.C4_FlightNo);
		}

		#endregion

		#region Number Filter Delegates

		ZQuery GetOutturnSendersRefNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddOutturnSendersRefNumberQuery(query, @operator, value);
			return query;
		}

		ZQuery GetOutturnHouseBillNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddOutturnHouseBillNumberQuery(query, @operator, value);
			return query;
		}

		ZQuery GetOutturnMasterBillNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddOutturnMasterBillNumberQuery(query, @operator, value);
			return query;
		}

		#endregion

		#region Number Filter Implementation

		protected virtual void AddOutturnSendersRefNumberQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			var underbondQuery = new ZDBOnlyQuery(typeof(CusUnderbond));
			var sendersRefQuery = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.PK);
			sendersRefQuery.AddToFilter_PossiblyCommaSeparated(CusHAWBSchema.CS_MessageReference, @operator, value);
			var outturnsForRefQuery = new ZDBOnlySubQuery(typeof(CusOutturn), CusOutturnSchema.C5_C4_Underbond);
			outturnsForRefQuery.AddSubQuery(CusOutturnSchema.C5_ParentID, sendersRefQuery, JoinCondition.And);

			underbondQuery.AddSubQuery(outturnsForRefQuery, JoinCondition.And);
			query.AddToFilter(underbondQuery);
		}

		protected virtual void AddOutturnHouseBillNumberQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery underbondQuery = new ZDBOnlyQuery(typeof(CusUnderbond));
			ZDBOnlySubQuery outturnSubQuery = new ZDBOnlySubQuery(typeof(CusOutturn), CusOutturnSchema.C5_C4_Underbond);
			outturnSubQuery.AddToFilter_PossiblyCommaSeparated(CusOutturnSchema.C5_HouseBill, value);

			ZDBOnlySubQuery hAWBQuery = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.PK);
			hAWBQuery.AddToFilter_PossiblyCommaSeparated(CusHAWBSchema.CS_HAWB, @operator, value);
			ZDBOnlySubQuery outturnsForHAWBQuery = new ZDBOnlySubQuery(typeof(CusOutturn), CusOutturnSchema.C5_C4_Underbond);
			outturnsForHAWBQuery.AddSubQuery(CusOutturnSchema.C5_ParentID, hAWBQuery, JoinCondition.And);

			underbondQuery.AddSubQuery(outturnSubQuery, JoinCondition.And);
			underbondQuery.AddSubQuery(outturnsForHAWBQuery, JoinCondition.Or);

			query.AddToFilter(underbondQuery);
		}

		protected virtual void AddOutturnMasterBillNumberQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery underbondQuery = new ZDBOnlyQuery(typeof(CusUnderbond));
			ZDBOnlySubQuery masterSubQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusUnderbondSchema.C4_ParentID);
			masterSubQuery.AddToFilter_PossiblyCommaSeparated(CusMAWBSchema.CM_MAWB, @operator, value);
			underbondQuery.AddSubQuery(masterSubQuery, JoinCondition.And);

			ZQuery sendersRefQuery = new ZQuery();
			sendersRefQuery.AddToFilter_PossiblyCommaSeparated(CusUnderbondSchema.C4_MAWB, value);
			underbondQuery.AddToFilter(sendersRefQuery, JoinCondition.Or);

			query.AddToFilter(underbondQuery);
		}

		#endregion

		#region Date Filter

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(FilterConstants.DateTypes.ArrivalDate, CusUnderbondSchema.C4_ArrivalDate);
			filters.AddDateFilter(FilterConstants.DateTypes.OutturnDate, CusUnderbondSchema.C4_Outurned);
		}

		#endregion

		#region Status Filter

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(FilterConstants.StatusTypes.Cargo, GetCargoStatusQuery, StatusList).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter(FilterConstants.StatusTypes.Outturn, GetOutturnStatusQuery, StatusList).Category = FilterCategories.StatusAndFlags;
		}

		public CodeDescriptionPairList StatusList
		{
			get { return new CMRAllStatuses(); }
		}

		#endregion

		#region Status Filter Delegates

		ZQuery GetCargoStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddCargoStatusQuery(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetOutturnStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddOutturnStatusQuery(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		#endregion

		#region Status Filter Implementation

		void AddCargoStatusQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery underbondQuery = new ZDBOnlyQuery(typeof(CusUnderbond));

			ZDBOnlySubQuery outturnSubQuery = new ZDBOnlySubQuery(typeof(CusOutturn), CusOutturnSchema.C5_C4_Underbond);
			outturnSubQuery.AddToFilter(CusOutturnSchema.C5_CustomsStatus, value);

			underbondQuery.AddSubQuery(outturnSubQuery, JoinCondition.And);
			query.AddToFilter(underbondQuery);
		}

		void AddOutturnStatusQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery underbondQuery = new ZDBOnlyQuery(typeof(CusUnderbond));

			ZDBOnlySubQuery entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryStatus, @operator, value);
			entryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumber.EntryType.OutturnStatus);

			underbondQuery.AddSubQuery(entryNumQuery, JoinCondition.And);
			query.AddToFilter(underbondQuery);
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
			ZDBOnlyQuery mawbQuery = new ZDBOnlyQuery(typeof(CusUnderbond));
			mawbQuery.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return mawbQuery;
		}

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => null;

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

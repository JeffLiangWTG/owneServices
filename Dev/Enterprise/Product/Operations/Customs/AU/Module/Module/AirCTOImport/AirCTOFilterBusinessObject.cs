using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Module.AirCargo;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public class AirCTOImportFilterBusinessObject : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddNumberFilters(filters);
			AddLocationFilters(filters);
			AddStatusFilters(filters);
			AddDateFilters(filters);

			AccountingFilterStrip.AddJobManagementFilters(filters, Env.Security.AUCustomsAirCTOImportJobInvoicing);

			return filters;
		}

		#region Number Filter

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddNumberFilter(AirCTOFilterConstants.NumberFilterTypes.JobNumber, GetJobNumberQuery);
			filter.MaxLength = CusHAWBSchema.CS_MessageReference.MaxLength;
			filter.UseMultiSearch = false; //TODO: Needs to work with multiple values. (AddJobNumberQuery)
			filter = filters.AddNumberFilter(AirCTOFilterConstants.NumberFilterTypes.MasterBillNumber, GetMasterBillQuery);
			filter.MaxLength = CusHAWBSchema.CS_HAWB.MaxLength;
			filter.UseMultiSearch = false; //TODO: Needs to work with multiple values. (AddMasterBillQuery)
			filter = filters.AddNumberFilter(AirCTOFilterConstants.NumberFilterTypes.FlightNo, GetFlightNoQuery);
			filter.MaxLength = CusMAWBSchema.CM_FlightNo.MaxLength;
			filter.UseMultiSearch = false; //TODO: Needs to work with multiple values. (GetFlightNoQuery)
		}

		#endregion

		#region Number Filter Delegates

		ZQuery GetJobNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddJobNumberQuery(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetMasterBillQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddMasterBillQuery(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetFlightNoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddFlightNoQuery(query, comparisonOperator, value);
			return query;
		}

		#endregion

		#region Number Filter Implementation

		protected void AddJobNumberQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			//TODO: Needs to work with multiple values.
			ZString jobNumber = (ZString)value;
			if (jobNumber.Length < 9 && !jobNumber.Contains('A'))
			{
				jobNumber = jobNumber.PadLeft(8, '0');
				query.AddToFilter(CusHAWBSchema.CS_MessageReference, @operator, 'A' + jobNumber);
			}
			else
			{
				query.AddToFilter(CusHAWBSchema.CS_MessageReference, @operator, jobNumber);
			}
		}

		protected void AddMasterBillQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			//TODO: Needs to work with multiple values.
			ZString masterBillNumber = ((ZString)value).Replace("-", "").Replace(" ", "");
			if (!masterBillNumber.IsEmpty)
			{
				query.AddToFilter(CusHAWBSchema.CS_HAWB, @operator, masterBillNumber);
			}
		}

		protected void AddFlightNoQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			//TODO: Needs to work with multiple values.
			ZString flightNo = ((ZString)value).Replace(" ", "");
			if (!flightNo.IsEmpty)
			{
				CusMAWBQuery mAWBQuery = new CusMAWBQuery();
				mAWBQuery.CusMAWBColumn = CusMAWBSchema.CM_FlightNo;
				mAWBQuery.Operator = @operator;
				mAWBQuery.Value = flightNo;
				AddDBOnlySubQueryForCusMAWB(query, new CusMAWBQuery[] { mAWBQuery }, JoinCondition.And);

				ZDBOnlySubQuery partShipQuery = new ZDBOnlySubQuery(typeof(CusPartShip), CusPartShipSchema.CG_CS);
				partShipQuery.AddToFilter(JoinCondition.And, CusPartShipSchema.CG_FlightNo, @operator, value);
				ZDBOnlyQuery joinQuery = new ZDBOnlyQuery(typeof(CusHAWB));
				joinQuery.AddSubQuery(partShipQuery, JoinCondition.Or);
				query.AddToFilter(joinQuery, JoinCondition.Or);
			}
		}

		#endregion

		#region Location Filter

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			filters.AddNkFilter(AirCTOFilterConstants.PortFilterTypes.PortOf1stArrival, GetFirstArrivalPortQuery, ModuleIDs.Location, Locations).Category = FilterCategories.Locations;
			filters.AddLocationFilter(string.Format("{0} / {1}", AirCTOFilterConstants.PortFilterTypes.Load, AirCTOFilterConstants.PortFilterTypes.PortOfArrival), GetLoadDischargeQuery, Locations, Locations).SetItemDescriptions(Res.GetData("2048b3f5-0b8b-4662-b142-0c4b8daef561", "Load"), Res.GetData("6185709d-834b-4a02-ae81-bf688867ceab", "Discharge"));
			filters.AddLocationFilter(string.Format("{0} / {1}", AirCTOFilterConstants.PortFilterTypes.Origin, AirCTOFilterConstants.PortFilterTypes.Destination), GetOriginDestinationQuery, Locations, Locations).SetItemDescriptions(Res.GetData("b3c5f2b8-7341-49a5-9342-ac2efcb3f29d", "Origin"), Res.GetData("17ed7007-edac-492a-a674-086c1e8b7fe6", "Destination"));
		}

		public LocationCollection Locations
		{
			get
			{
				return new LocationCollection(Factory);
			}
		}

		#endregion

		#region Location Filter Delegates

		ZQuery GetLoadDischargeQuery(ZString loadNk, ZString dischargeNk)
		{
			ZQuery query = new ZQuery();

			if (!loadNk.IsEmpty)
			{
				query.AddToFilter(GetPortQuery(AirCTOFilterConstants.PortFilterTypes.Load, loadNk));
			}

			if (!dischargeNk.IsEmpty)
			{
				query.AddToFilter(GetPortQuery(AirCTOFilterConstants.PortFilterTypes.PortOfArrival, dischargeNk));
			}

			return query;
		}

		ZQuery GetOriginDestinationQuery(ZString originNk, ZString destinationNk)
		{
			ZQuery query = new ZQuery();

			if (!originNk.IsEmpty)
			{
				query.AddToFilter(GetPortQuery(AirCTOFilterConstants.PortFilterTypes.Origin, originNk));
			}

			if (!destinationNk.IsEmpty)
			{
				query.AddToFilter(GetPortQuery(AirCTOFilterConstants.PortFilterTypes.Destination, destinationNk));
			}

			return query;
		}

		ZQuery GetFirstArrivalPortQuery(ZString portNk)
		{
			return GetPortQuery(AirCTOFilterConstants.PortFilterTypes.PortOf1stArrival, portNk);
		}

		ZQuery GetPortQuery(ZString portType, ZString portNk)
		{
			ZQuery query = new ZQuery();

			bool isCountryCode = (portNk.Length == 2);
			SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

			AddPortFilter(query, portType, comparisonOperator, portNk);
			return query;
		}

		#endregion

		#region Location Filter Implementation

		protected void AddPortFilter(ZQuery query, ZString portType, SQLComparisonOperator @operator, object value)
		{
			SchemaColumn column = null;
			switch (portType)
			{
				case AirCTOFilterConstants.PortFilterTypes.Load:
					column = CusHAWBSchema.CS_RL_NKLoadPort;
					break;
				case AirCTOFilterConstants.PortFilterTypes.Origin:
					column = CusHAWBSchema.CS_RL_NKOrigin;
					break;
				case AirCTOFilterConstants.PortFilterTypes.Destination:
					column = CusHAWBSchema.CS_RL_NKDestination;
					break;
				case AirCTOFilterConstants.PortFilterTypes.PortOfArrival:
					column = CusMAWBSchema.CM_RL_NKDischargePort;
					break;
				case AirCTOFilterConstants.PortFilterTypes.PortOf1stArrival:
					column = CusMAWBSchema.CM_RL_NKFirstArrivalPort;
					break;
			}

			if (column != null)
			{
				if (column == CusMAWBSchema.CM_RL_NKDischargePort || column == CusMAWBSchema.CM_RL_NKFirstArrivalPort)
				{
					CusMAWBQuery mAWBQuery = new CusMAWBQuery();
					mAWBQuery.CusMAWBColumn = column;
					mAWBQuery.Operator = @operator;
					mAWBQuery.Value = value;

					AddDBOnlySubQueryForCusMAWB(query, new CusMAWBQuery[] { mAWBQuery }, JoinCondition.And);
				}
				else
				{
					query.AddToFilter(column, @operator, value);
				}
			}
		}

		#endregion

		#region Status Filter

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(AirCargoFilterConstants.StatusFilterType.Outturn, GetOutturnStatusQuery, GetStatusList(AirCargoFilterConstants.StatusFilterType.Outturn)).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter(AirCargoFilterConstants.StatusFilterType.CMRUnderbond, GetUnderbondStatusQuery, GetStatusList(AirCargoFilterConstants.StatusFilterType.CMRUnderbond)).Category = FilterCategories.StatusAndFlags;
		}

		public CodeDescriptionPairList GetStatusList(ZString statusType)
		{
			CodeDescriptionPairList result;
			switch (statusType)
			{
				case AirCargoFilterConstants.StatusFilterType.CMRUnderbond:
					result = new CMRAllStatuses();
					break;

				case AirCargoFilterConstants.StatusFilterType.Outturn:
					result = new CMRAllStatuses();
					break;

				default:
					result = new CodeDescriptionPairList();
					result.AddRange(new AirCargoStatusList());
					result.AddRange(new CMRAllStatuses());
					break;
			}
			return result;
		}

		#endregion

		#region Status Filters Delegates

		ZQuery GetOutturnStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddOutturnStatusFilter(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetUnderbondStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddUnderbondStatusFilter(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		#endregion

		#region Status Filter Implementation

		protected void AddOutturnStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlySubQuery entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryStatus, @operator, value);
			entryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumber.EntryType.OutturnStatus);

			ZDBOnlySubQuery underbondQuery = new ZDBOnlySubQuery(typeof(CusUnderbond), CusUnderbondSchema.C4_ParentID);

			underbondQuery.AddSubQuery(entryNumQuery, JoinCondition.And);

			ZDBOnlySubQuery mAWBQuery = new ZDBOnlySubQuery(typeof(CTOCusMAWB), CusMAWBSchema.PK);
			mAWBQuery.AddSubQuery(underbondQuery, JoinCondition.And);

			ZDBOnlySubQuery hAWBQuery = new ZDBOnlySubQuery(typeof(CTOCusHAWB), CusHAWBSchema.PK);
			hAWBQuery.AddSubQuery(underbondQuery, JoinCondition.And);

			ZDBOnlyQuery mAWBOrHAWBQuery = new ZDBOnlyQuery(typeof(CTOCusHAWB));

			mAWBOrHAWBQuery.AddSubQuery(CusHAWBSchema.CS_CM, mAWBQuery, JoinCondition.Or);
			mAWBOrHAWBQuery.AddSubQuery(hAWBQuery, JoinCondition.Or);

			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CTOCusHAWB));
			dBQuery.AddToFilter(mAWBOrHAWBQuery, JoinCondition.And);
			query.AddToFilter(dBQuery);
		}

		protected void AddUnderbondStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CTOCusHAWB));
			dBQuery.AddToFilter(JoinCondition.And, CusHAWBSchema.CS_CustomsStatus, @operator, value);
			query.AddToFilter(dBQuery);
		}

		#endregion

		#region Date Filter

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(AirCTOFilterConstants.DateFilterType.EstimatedArrival, GetEstimatedDateQuery);
			filters.AddDateFilter(AirCTOFilterConstants.DateFilterType.DateOf1stArrival, GetFirstArrivalDateQuery);
		}

		ZQuery GetEstimatedDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddDateOfArrivalQuery(query, comparisonOperator, CusMAWBSchema.CM_ArrivalDate, fromDate, toDate);
			return query;
		}

		ZQuery GetFirstArrivalDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddDateOfArrivalQuery(query, comparisonOperator, CusMAWBSchema.CM_DateOfFirstArrival, fromDate, toDate);
			return query;
		}

		void AddDateOfArrivalQuery(ZQuery query, DateComparisonOperator comparisonOperator, SchemaDateTimeColumn column, ZDateTime dateFrom, ZDateTime dateTo)
		{
			List<CusMAWBQuery> result = new List<CusMAWBQuery>();

			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				CusMAWBQuery mAWBQuery = new CusMAWBQuery();
				mAWBQuery.CusMAWBColumn = column;
				mAWBQuery.Operator = SQLComparisonOperator.Equal;
				mAWBQuery.Value = null;
				result.Add(mAWBQuery);
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				CusMAWBQuery mAWBQuery = new CusMAWBQuery();
				mAWBQuery.CusMAWBColumn = column;
				mAWBQuery.Operator = SQLComparisonOperator.NotEqual;
				mAWBQuery.Value = null;
				result.Add(mAWBQuery);
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateInRange)
			{
				if (dateFrom.IsValid)
				{
					CusMAWBQuery mAWBQuery = new CusMAWBQuery();
					mAWBQuery.CusMAWBColumn = column;
					mAWBQuery.Operator = SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly;
					mAWBQuery.Value = dateFrom;
					result.Add(mAWBQuery);
				}

				if (dateTo.IsValid)
				{
					CusMAWBQuery mAWBQuery = new CusMAWBQuery();
					mAWBQuery.CusMAWBColumn = column;
					mAWBQuery.Operator = SQLComparisonOperator.LessThanOrEqualToDatePartOnly;
					mAWBQuery.Value = dateTo;
					result.Add(mAWBQuery);
				}
			}

			AddDBOnlySubQueryForCusMAWB(query, result.ToArray(), JoinCondition.And);
		}

		#endregion

		#region CTOMAWB Filter

		public override ZQuery Filter
		{
			get
			{
				ZQuery filter = base.Filter;
				filter.AddToFilter(CTOMAWBQuery);
				return filter;
			}
		}

		protected virtual ZQuery CTOMAWBQuery
		{
			get
			{
				ZQuery query = new ZQuery();

				CusMAWBQuery mAWBQuery = new CusMAWBQuery();
				mAWBQuery.CusMAWBColumn = CusMAWBSchema.CM_IsCTOMAWB;
				mAWBQuery.Operator = SQLComparisonOperator.Equal;
				mAWBQuery.Value = ZBool.True;

				CusMAWBQuery applicationCodeQuery = new CusMAWBQuery();
				applicationCodeQuery.CusMAWBColumn = CusMAWBSchema.CM_ApplicationCode;
				applicationCodeQuery.Operator = SQLComparisonOperator.Equal;
				applicationCodeQuery.Value = CusMAWBBase.Loader.CMRApplicationCodes;

				AddDBOnlySubQueryForCusMAWB(query, new CusMAWBQuery[] { mAWBQuery, applicationCodeQuery }, JoinCondition.And);

				return query;
			}
		}

		#endregion

		#region Implementation

		public struct CusMAWBQuery
		{
			public SchemaColumn CusMAWBColumn { get; set; }
			public SQLComparisonOperator Operator { get; set; }
			public object Value { get; set; }
		}

		void AddDBOnlySubQueryForCusMAWB(ZQuery query, CusMAWBQuery[] cusMAWBQueries, JoinCondition conditionBetweenCusMAWBQueries)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CTOCusMAWB), CusHAWBSchema.CS_CM);

			foreach (CusMAWBQuery mAWBQuery in cusMAWBQueries)
			{
				subQuery.AddToFilter(conditionBetweenCusMAWBQueries, mAWBQuery.CusMAWBColumn, mAWBQuery.Operator, mAWBQuery.Value);
			}

			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CTOCusHAWB));
			dBQuery.AddSubQuery(subQuery, JoinCondition.And);

			query.AddToFilter(dBQuery);
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
			ZDBOnlySubQuery mawbQuery = new ZDBOnlySubQuery(typeof(CTOCusMAWB), CusHAWBSchema.CS_CM);
			mawbQuery.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			ZDBOnlyQuery hawbQuery = new ZDBOnlyQuery(typeof(CTOCusHAWB));
			hawbQuery.AddSubQuery(mawbQuery, JoinCondition.And);

			return hawbQuery;
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

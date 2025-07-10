using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module.AirCargo
{
	public class AUCustomsHouseAirCargoFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddNumberFilters(filters);
			AddOrganisationFilters(filters);
			AddLocationFilters(filters);
			AddStatusFilters(filters);
			AddDateFilters(filters);
			return filters;
		}

		#region Number Filters

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(AirCargoFilterConstants.NumberFilterTypes.JobNumber, GetJobNumberQuery).WithMaxLengthOf<ModuleNumberFilter>(CusHAWBSchema.CS_MessageReference);
			filters.AddNumberFilter(AirCargoFilterConstants.NumberFilterTypes.ConsolNumber, GetConsolNumberQuery).WithMaxLengthOf<ModuleNumberFilter>(JobConsolSchema.JK_UniqueConsignRef);
			filters.AddNumberFilter(AirCargoFilterConstants.NumberFilterTypes.HouseBillNumber, GetHouseBillNumberQuery).WithMaxLengthOf<ModuleNumberFilter>(CusHAWBSchema.CS_HAWB);
			filters.AddNumberFilter(AirCargoFilterConstants.NumberFilterTypes.CoLoadMaster, GetCoLoadMasterQuery).WithMaxLengthOf<ModuleNumberFilter>(CusHAWBSchema.CS_MasterHouseBill);
			filters.AddNumberFilter(AirCargoFilterConstants.NumberFilterTypes.MasterBillNumber, GetMasterBillNumberQuery).WithMaxLengthOf<ModuleNumberFilter>(CusMAWBSchema.CM_MAWB);
			filters.AddNumberFilter(AirCargoFilterConstants.NumberFilterTypes.FlightNo, GetFlightNoQuery).WithMaxLengthOf<ModuleNumberFilter>(CusMAWBSchema.CM_FlightNo);

			if (AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.Value)
			{
				var consignRefNumberFilter = filters.AddNumberFilter(AirCargoFilterConstants.NumberFilterTypes.ConRef, GetConRefNumberQuery);
				consignRefNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
				consignRefNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
				consignRefNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
				consignRefNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
				consignRefNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			}
		}

		#endregion

		#region Number Filters Delegates

		ZQuery GetJobNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddJobNumberQuery(query, @operator, value);
			return query;
		}

		ZQuery GetMasterBillNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddMasterBillQuery(query, @operator, value);
			return query;
		}

		ZQuery GetHouseBillNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddHAWBQuery(query, @operator, value);
			return query;
		}

		ZQuery GetConRefNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			var query = new ZQuery();
			AddConRefQuery(query, @operator, value);
			return query;
		}

		ZQuery GetConsolNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddConsolNumberQuery(query, @operator, value);
			return query;
		}

		ZQuery GetCoLoadMasterQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddCoLoadMasterQuery(query, @operator, value);
			return query;
		}

		ZQuery GetFlightNoQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddFlightNoQuery(query, @operator, value);
			return query;
		}

		#endregion

		#region Number Filters Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI003", Justification = "Don't have access to the alternatives it wants")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		protected virtual void AddJobNumberQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			var strValue = (ZString)value;
			var multiJobNumber = new ZString[] { strValue };

			if (!string.IsNullOrEmpty(RawDataRegistry.Instance.MultiSearchSeparator.Value))
			{
				multiJobNumber = strValue.Split(RawDataRegistry.Instance.MultiSearchSeparator.Value);
			}
			foreach (var number in multiJobNumber)
			{
				var jobNumber = number.Trim();
				var innerQuery = new ZQuery();
				if (jobNumber.Length < 9 && !jobNumber.Contains('A') && !jobNumber.Contains('S'))
				{
					jobNumber = jobNumber.PadLeft(8, '0');
					ZQuery subQuery = new ZQuery(new ZQuery(CusHAWBSchema.CS_MessageReference, @operator, 'A' + jobNumber));
					subQuery.AddToFilter(JoinCondition.Or, CusHAWBSchema.CS_MessageReference, @operator, 'S' + jobNumber);
					innerQuery.AddToFilter(subQuery);
				}
				else
				{
					innerQuery.AddToFilter(CusHAWBSchema.CS_MessageReference, @operator, jobNumber);
				}
				query.AddToFilter(innerQuery, @operator.IsNegativeSQLOperator() ? JoinCondition.And : JoinCondition.Or);
			}
		}

		protected virtual void AddHAWBQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter_PossiblyCommaSeparated(CusHAWBSchema.CS_HAWB, @operator, value);
		}

		protected virtual void AddConRefQuery(ZQuery query, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var billAddOnColumnQuery = new GenAddOnColumnQueryHelper(typeof(CusHAWBBase)).GetQueryOnGenAddOnColumn(CusHAWBBase.Schema.CS_fPartShipConsignmentReference, comparisonOperator, value);
			query.AddToFilter(billAddOnColumnQuery, JoinCondition.And);
		}

		protected virtual void AddCoLoadMasterQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter_PossiblyCommaSeparated(CusHAWBSchema.CS_MasterHouseBill, @operator, value);
		}

		protected virtual void AddMasterBillQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZString mAWB = (ZString)value;
			AddMasterQuery(query, @operator, mAWB.Replace("-", "").Replace(" ", ""), CusMAWBSchema.CM_MAWB);
		}

		protected virtual void AddFlightNoQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddMasterQuery(query, @operator, value, CusMAWBSchema.CM_FlightNo);
			ZDBOnlyQuery joinQuery = new ZDBOnlyQuery(typeof(CusHAWB));
			joinQuery.AddSubQuery(UnderbondQueryForFlightNo(@operator, value), JoinCondition.And);
			query.AddToFilter(joinQuery, JoinCondition.Or);
		}

		protected ZDBOnlySubQuery UnderbondQueryForFlightNo(SQLComparisonOperator @operator, object value)
		{
			ZDBOnlySubQuery underbondQuery = new ZDBOnlySubQuery(typeof(CusUnderbond), CusUnderbondSchema.C4_ParentID);
			underbondQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, CusUnderbondSchema.C4_FlightNo, @operator, value);
			return underbondQuery;
		}

		protected virtual void AddConsolNumberQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddMasterQuery(query, SQLComparisonOperator.Equal, GetConsolPK((ZString)value), CusMAWBSchema.CM_JK);
		}

		protected object GetConsolPK(ZString consolID)
		{
			var multiID = new ZString[] { consolID };

			if (!string.IsNullOrEmpty(RawDataRegistry.Instance.MultiSearchSeparator.Value))
			{
				multiID = consolID.Split(RawDataRegistry.Instance.MultiSearchSeparator.Value);
			}

			var consolPKs = new List<ZGuid>();

			foreach (var id in multiID)
			{
				consolID = id.Trim();
				if (consolID.Length < 9 && !consolID.StartsWith("C"))
				{
					consolID = consolID.PadLeft(8, '0');
					consolID = 'C' + consolID;
				}
				var consol = Factory.LoadFromNaturalKey<ForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, consolID);
				var consolPK = consol != null ? consol.PK : ZGuid.Invalid;
				consolPKs.Add(consolPK);
			}

			if (consolPKs.Count == 0)
			{
				return ZGuid.Invalid;
			}
			else if (consolPKs.Count == 1)
			{
				return consolPKs.First();
			}
			else
			{
				return consolPKs.ToArray();
			}
		}

		#endregion

		#region Organisation Filters

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			ModuleGuidsFilter cnrCneFilter = filters.AddGuidFilter(string.Format("{0} / {1}", AirCargoFilterConstants.PartyFilterTypes.Consignor, AirCargoFilterConstants.PartyFilterTypes.Consignee), ModuleIDs.Organisation, GetConsignorConsigneeQuery, Consignors, Consignees);
			cnrCneFilter.SetItemDescriptions(Res.GetData("a965aac1-87eb-42ab-8947-10a88bef4c05", "Consignor"), Res.GetData("67e0d37e-0c2b-4ea7-bda0-d02dfc8c203f", "Consignee"));
			cnrCneFilter.Category = FilterCategories.Organisations;
		}

		public ConsigneeCollection Consignees
		{
			get
			{
				return new ConsigneeCollection(Factory);
			}
		}

		public ConsignorCollection Consignors
		{
			get
			{
				return new ConsignorCollection(Factory);
			}
		}

		#endregion

		#region Organisation Filters Delegates

		ZQuery GetConsignorConsigneeQuery(ZGuid consignor, ZGuid consignee)
		{
			ZQuery query = new ZQuery();

			if (consignor.IsValid)
			{
				AddConsignorQuery(query, SQLComparisonOperator.Equal, consignor);
			}

			if (consignee.IsValid)
			{
				AddConsigneeQuery(query, SQLComparisonOperator.Equal, consignee);
			}

			return query;
		}

		#endregion

		#region Organisation Filters Implementation

		protected virtual void AddConsigneeQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			var addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), CusHAWBSchema.CS_OA_ConsigneeAddress);
			addressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, @operator, value);
			var hawbQuery = new ZDBOnlyQuery(typeof(CusHAWB));
			hawbQuery.AddSubQuery(addressSubQuery, JoinCondition.And);
			query.AddToFilter(hawbQuery);
		}

		protected virtual void AddConsignorQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			var addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), CusHAWBSchema.CS_OA_ConsignorAddress);
			addressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, @operator, value);
			var hawbQuery = new ZDBOnlyQuery(typeof(CusHAWB));
			hawbQuery.AddSubQuery(addressSubQuery, JoinCondition.And);
			query.AddToFilter(hawbQuery);
		}

		#endregion

		#region Location Filters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			filters.AddLocationFilter(AirCargoFilterConstants.PortFilterTypes.LoadDischarge, GetLoadDischargeQuery, Locations, Locations).SetItemDescriptions(Res.GetData("95315ae0-8810-4022-adcc-82f64de94207", "Load"), Res.GetData("488a5d38-1a0b-40a9-91f9-511244268f72", "Discharge"));
			filters.AddLocationFilter(AirCargoFilterConstants.PortFilterTypes.OriginDestination, GetOriginDestinationQuery, Locations, Locations).SetItemDescriptions(Res.GetData("b5b84305-4c80-4087-a9d8-83320c5cd90e", "Origin"), Res.GetData("cf98abba-87a0-4308-ba53-44cae4669e68", "Destination"));
		}

		public LocationCollection Locations
		{
			get
			{
				return new LocationCollection(Factory);
			}
		}

		#endregion

		#region Location Filters Delegates

		ZQuery GetLoadDischargeQuery(ZString loadNk, ZString dischargeNk)
		{
			ZQuery query = new ZQuery();

			if (!loadNk.IsEmpty)
			{
				bool isCountryCode = (loadNk.Length == 2);
				SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

				AddLoadQuery(query, comparisonOperator, loadNk);
			}

			if (!dischargeNk.IsEmpty)
			{
				bool isCountryCode = (dischargeNk.Length == 2);
				SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

				AddDischageQuery(query, comparisonOperator, dischargeNk);
			}

			return query;
		}

		ZQuery GetOriginDestinationQuery(ZString originNk, ZString destinationNk)
		{
			ZQuery query = new ZQuery();

			if (!originNk.IsEmpty)
			{
				bool isCountryCode = (originNk.Length == 2);
				SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

				AddOriginQuery(query, comparisonOperator, originNk);
			}

			if (!destinationNk.IsEmpty)
			{
				bool isCountryCode = (destinationNk.Length == 2);
				SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

				AddDestinationQuery(query, comparisonOperator, destinationNk);
			}

			return query;
		}

		#endregion

		#region Location Filters Implementation

		protected virtual void AddLoadQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddMasterQuery(query, @operator, value, CusMAWBSchema.CM_RL_NKLoadPort);
		}

		protected virtual void AddDischageQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddMasterQuery(query, @operator, value, CusMAWBSchema.CM_RL_NKDischargePort);
		}

		protected virtual void AddOriginQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(CusHAWBSchema.CS_RL_NKOrigin, @operator, value);
		}

		protected virtual void AddDestinationQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(CusHAWBSchema.CS_RL_NKDestination, @operator, value);
		}

		#endregion

		#region Status Filters

		protected virtual bool SupportsMasterAndHouse
		{
			get { return false; }
		}

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(AirCargoFilterConstants.StatusFilterType.CMRCustoms, GetCMRCustomsStatusQuery, GetStatusList(AirCargoFilterConstants.StatusFilterType.CMRCustoms)).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter(AirCargoFilterConstants.StatusFilterType.CMRMessage, GetCMRMessageStatusQuery, GetStatusList(AirCargoFilterConstants.StatusFilterType.CMRMessage)).Category = FilterCategories.StatusAndFlags;
			if (SupportsMasterAndHouse)
			{
				filters.AddTextFilter(AirCargoFilterConstants.StatusFilterType.CMRUnderbondBoth, GetUnderbondStatusQuery, GetStatusList(AirCargoFilterConstants.StatusFilterType.CMRUnderbond)).Category = FilterCategories.StatusAndFlags;
				filters.AddTextFilter(AirCargoFilterConstants.StatusFilterType.CMRUnderbondMaster, GetUnderbondMasterStatusQuery, GetStatusList(AirCargoFilterConstants.StatusFilterType.CMRUnderbond)).Category = FilterCategories.StatusAndFlags;
				filters.AddTextFilter(AirCargoFilterConstants.StatusFilterType.OutturnBoth, GetOutturnStatusQuery, GetStatusList(AirCargoFilterConstants.StatusFilterType.Outturn)).Category = FilterCategories.StatusAndFlags;
				filters.AddTextFilter(AirCargoFilterConstants.StatusFilterType.OutturnMaster, GetOutturnMasterStatusQuery, GetStatusList(AirCargoFilterConstants.StatusFilterType.Outturn)).Category = FilterCategories.StatusAndFlags;
			}
			else
			{
				filters.AddTextFilter(AirCargoFilterConstants.StatusFilterType.CMRUnderbond, GetUnderbondStatusQuery, GetStatusList(AirCargoFilterConstants.StatusFilterType.CMRUnderbond)).Category = FilterCategories.StatusAndFlags;
				filters.AddTextFilter(AirCargoFilterConstants.StatusFilterType.Outturn, GetOutturnStatusQuery, GetStatusList(AirCargoFilterConstants.StatusFilterType.Outturn)).Category = FilterCategories.StatusAndFlags;
				if (Enterprise.Registry.Business.HVLVDataRegistry.HasHVLVClearance)
				{
					filters.AddFlagsFilter(AirCargoFilterConstants.StatusFilterType.InBondStore, new[] { "In Bond Store?" }, new GetFlagsQuery[] { GetInBondStoreQuery });
				}
			}
		}

		public virtual CodeDescriptionPairList GetStatusList(ZString statusType)
		{
			CodeDescriptionPairList result;
			switch (statusType)
			{
				case AirCargoFilterConstants.StatusFilterType.CMRUnderbond:
					result = new CMRAllStatuses();
					break;

				case AirCargoFilterConstants.StatusFilterType.Outturn:
					result = new CMRBaseStatuses();
					break;

				case AirCargoFilterConstants.StatusFilterType.CMRMessage:
					result = new CMRBaseStatuses();
					break;

				case AirCargoFilterConstants.StatusFilterType.CMRCustoms:
					result = CMRConsolidatedCargoStatuses.AllFilterStatuses;
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

		ZQuery GetInBondStoreQuery(ZBool value)
		{
			var result = new ZDBOnlyQuery(typeof(CusHAWB));
			if (value)
			{
				result.AddToFilter(CusHAWBSchema.CS_IsHeldAtOutturn, true);

				var cadQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
				cadQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CargoReceivedAtDepotCode);

				result.AddSubQuery(cadQuery, JoinCondition.And);
			}
			return result;
		}

		ZQuery GetUnderbondStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddUnderbondStatusFilter(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetUnderbondMasterStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddUnderbondMasterStatusFilter(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetCMRCustomsStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddCMRCustomsStatusQuery(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetCMRMessageStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddCMRMessageStatusFilter(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetOutturnStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddOutturnStatusFilter(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetOutturnMasterStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddOutturnMasterStatusFilter(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		#endregion

		#region Status Filters Implementation

		protected virtual void AddUnderbondStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddDepotStatusFilter(query, @operator, value, CusEntryNumber.EntryType.UnderbondStatus);
		}

		protected virtual void AddUnderbondMasterStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddDepotMasterStatusFilter(query, @operator, value, CusEntryNumber.EntryType.UnderbondStatus);
		}

		protected virtual void AddOutturnStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddDepotStatusFilter(query, @operator, value, CusEntryNumber.EntryType.OutturnStatus);
		}

		protected virtual void AddOutturnMasterStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddDepotMasterStatusFilter(query, @operator, value, CusEntryNumber.EntryType.OutturnStatus);
		}

		protected virtual void AddCMRCustomsStatusQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (value.ToString() == CMRConsolidatedCargoStatuses.Filter.Codes.NotClear)
			{
				foreach (CodeDescriptionPair codeDescription in CMRConsolidatedCargoStatuses.AllClearStatus)
				{
					StatusQuery(query, SQLComparisonOperator.NotEqual, (ZString)codeDescription.Code, CusHAWBSchema.CS_CustomsStatus);
				}
			}
			else if (value.ToString() == CMRConsolidatedCargoStatuses.Filter.Codes.HeldOrConditional)
			{
				query.AddToFilter(JoinCondition.Or, CusHAWBSchema.CS_CustomsStatus, SQLComparisonOperator.Equal, (ZString)CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl);
				query.AddToFilter(JoinCondition.Or, CusHAWBSchema.CS_CustomsStatus, SQLComparisonOperator.Equal, (ZString)CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation);
			}
			else if (value.ToString() == CMRConsolidatedCargoStatuses.Filter.Codes.ClearOrConditional)
			{
				query.AddToFilter(JoinCondition.Or, CusHAWBSchema.CS_CustomsStatus, SQLComparisonOperator.Equal, (ZString)CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased);
				query.AddToFilter(JoinCondition.Or, CusHAWBSchema.CS_CustomsStatus, SQLComparisonOperator.Equal, (ZString)CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation);
			}
			else
			{
				StatusQuery(query, @operator, value, CusHAWBSchema.CS_CustomsStatus);
			}
		}

		protected virtual void AddCMRMessageStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			switch (value.ToString())
			{
				case CMRBaseStatuses.Codes.NotSent:
					query.AddToFilter(JoinCondition.Or, CusHAWBSchema.CS_MsgStatus, @operator, new ZString[] { CMRBaseStatuses.Codes.NotSent, ZString.Empty });
					break;
				default:
					StatusQuery(query, @operator, value, CusHAWBSchema.CS_MsgStatus);
					break;
			}
		}

		protected virtual void AddFirstLandingQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			StatusQuery(query, @operator, value, CusHAWBSchema.CS_CustomsMainStatus);
		}

		protected virtual void AddLastLandingQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			StatusQuery(query, @operator, value, CusHAWBSchema.CS_CustomsStatus);
		}

		protected virtual void AddDepotStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value, object entryNumberType)
		{
			ZDBOnlySubQuery entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryStatus, @operator, value);
			entryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, entryNumberType);

			ZDBOnlySubQuery underbondQuery = new ZDBOnlySubQuery(typeof(CusUnderbond), CusUnderbondSchema.C4_ParentID);
			underbondQuery.AddSubQuery(entryNumQuery, JoinCondition.And);

			ZDBOnlySubQuery mAWBQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusMAWBSchema.PK);
			mAWBQuery.AddSubQuery(underbondQuery, JoinCondition.And);

			ZDBOnlySubQuery hAWBQuery = new ZDBOnlySubQuery(typeof(CusHAWB), CusHAWBSchema.PK);
			hAWBQuery.AddSubQuery(underbondQuery, JoinCondition.And);

			ZDBOnlyQuery mAWBOrHAWBQuery = new ZDBOnlyQuery(typeof(CusHAWB));
			mAWBOrHAWBQuery.AddSubQuery(CusHAWBSchema.CS_CM, mAWBQuery, JoinCondition.Or);
			mAWBOrHAWBQuery.AddSubQuery(hAWBQuery, JoinCondition.Or);

			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusHAWB));
			dBQuery.AddToFilter(mAWBOrHAWBQuery, JoinCondition.And);
			query.AddToFilter(dBQuery);
		}

		protected virtual void AddDepotMasterStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value, object entryNumberType)
		{
			throw new NotSupportedException("Master only underbond/outturn query not supported by this module");
		}

		protected void StatusQuery(ZQuery query, SQLComparisonOperator @operator, object value, SchemaColumn columnName)
		{
			ZString status = new ZString(value);
			if (status == AirCargoMessage.NewStatus.Waiting)
			{
				query.AddToFilter(CusHAWBSchema.CS_IsResponsePending, "Y");
			}
			else
			{
				query.AddToFilter(columnName, @operator, value);
			}
		}

		#endregion

		#region CMROnly Filter Implementation

		protected virtual void AddCMROnlyQuery(ZString applicationCode, SQLComparisonOperator comparisonOperator, ZQuery query)
		{
			ZDBOnlySubQuery applicationCodeQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusMAWBSchema.PK);
			applicationCodeQuery.AddToFilter(JoinCondition.And, CusMAWBSchema.CM_ApplicationCode, SQLComparisonOperator.Equal, applicationCode);

			ZDBOnlySubQuery noApplicationCodeQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusMAWBSchema.PK);
			noApplicationCodeQuery.AddToFilter(JoinCondition.And, CusMAWBSchema.CM_ApplicationCode, SQLComparisonOperator.Equal, ZString.Empty);
			noApplicationCodeQuery.AddToFilter(JoinCondition.And, CusMAWBSchema.CM_ArrivalDate, comparisonOperator, Core.Constants.AUCustoms.CMRImportsCutOverDate);

			ZDBOnlyQuery hAWBs = new ZDBOnlyQuery(typeof(CusHAWB));
			hAWBs.AddSubQuery(CusHAWBSchema.CS_CM, CusMAWBSchema.PK, applicationCodeQuery, JoinCondition.And);
			hAWBs.AddSubQuery(CusHAWBSchema.CS_CM, CusMAWBSchema.PK, noApplicationCodeQuery, JoinCondition.Or);

			query.AddToFilter(hAWBs, JoinCondition.And);
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
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);
				subQuery.AddToFilter(CusMAWBSchema.CM_IsCTOMAWB, SQLComparisonOperator.Equal, ZBool.False);
				subQuery.AddToFilter(CusMAWBSchema.CM_ApplicationCode, CusMAWBBase.Loader.CMRApplicationCodes);
				subQuery.AddToFilter(CusMAWBSchema.CM_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
				ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusHAWB));
				dBQuery.AddSubQuery(subQuery, JoinCondition.And);
				query.AddToFilter(dBQuery);
				return query;
			}
		}

		#endregion

		#region ArrivalDate Filters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("Arrival Date", GetArrivalDateQuery);
		}

		ZQuery GetArrivalDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddArrivalDateQuery(query, comparisonOperator, fromDate, toDate);
			return query;
		}

		protected virtual void AddArrivalDateQuery(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);
			AddDateTimeRange(subQuery, comparisonOperator, JoinCondition.And, CusMAWBSchema.CM_ArrivalDate, fromDate, toDate);

			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusHAWB));
			dBQuery.AddSubQuery(subQuery, JoinCondition.And);

			query.AddToFilter(dBQuery);
		}

		#endregion

		#region Custom Fields Filters

		protected virtual WorkflowFilterStripsHelper GetWorkflowFilterStripHelper() => new WorkflowFilterStripsHelper(typeof(CusHAWB), WorkflowDescriptors.CustomsHouseAirCargoCode, Factory);

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			helpers.Add(GetWorkflowFilterStripHelper());
			return helpers;
		}

		#endregion

		#region Implementation

		protected virtual void AddMasterQuery(ZQuery query, SQLComparisonOperator @operator, object value, SchemaColumn columnName)
		{
			if ((value is IZType zType && !zType.IsEmpty) || (value is ZGuid[] guids && guids.Length > 0))
			{
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);
				subQuery.AddToFilter_PossiblyCommaSeparated(columnName, @operator, value);

				ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusHAWB));
				dBQuery.AddSubQuery(subQuery, JoinCondition.And);

				query.AddToFilter(dBQuery);
			}
		}

		#endregion
	}
}

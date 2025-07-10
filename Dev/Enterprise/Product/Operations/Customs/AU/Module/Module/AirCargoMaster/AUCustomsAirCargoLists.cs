using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module.AirCargo
{
	public class AUCustomsAirCargoLists
	{
		public AUCustomsAirCargoLists(BusinessObjectFactory factory)
		{
			this.Factory = factory;
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

		protected readonly BusinessObjectFactory Factory;

		public SQLComparisonOperator FilterOperator;

		#region Query Provider

		public CodeDescriptionPairList CMROnlyList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(AirCargoFilterConstants.CMROnlyTypes.All);
				result.AddPair(AirCargoFilterConstants.CMROnlyTypes.CMRonly);
				result.AddPair(AirCargoFilterConstants.CMROnlyTypes.LegacyOnly);
				return result;
			}
		}

		#endregion

		#region Delegates

		protected virtual void AddConsigneeFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			var addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), CusHAWBSchema.CS_OA_ConsigneeAddress);
			addressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, @operator, value);
			var hawbQuery = new ZDBOnlyQuery(typeof(CusHAWB));
			hawbQuery.AddSubQuery(addressSubQuery, JoinCondition.And);
			query.AddToFilter(hawbQuery);
		}

		protected virtual void AddConsignorFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			var addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), CusHAWBSchema.CS_OA_ConsignorAddress);
			addressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, @operator, value);
			var hawbQuery = new ZDBOnlyQuery(typeof(CusHAWB));
			hawbQuery.AddSubQuery(addressSubQuery, JoinCondition.And);
			query.AddToFilter(hawbQuery);
		}

		#region Numbers

		protected virtual void AddJobNumberQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (!((ZString)value).IsEmpty)
			{
				ZString jobNumber = (ZString)value;
				if (jobNumber.Length < 9 && !jobNumber.Contains('A') && !jobNumber.Contains('S'))
				{
					jobNumber = jobNumber.PadLeft(8, '0');
					ZQuery subQuery = new ZQuery(new ZQuery(CusHAWBSchema.CS_MessageReference, @operator, 'A' + jobNumber));
					subQuery.AddToFilter(JoinCondition.Or, CusHAWBSchema.CS_MessageReference, @operator, 'S' + jobNumber);
					query.AddToFilter(subQuery);
				}
				else
				{
					query.AddToFilter(CusHAWBSchema.CS_MessageReference, @operator, jobNumber);
				}
			}
		}

		protected virtual void AddHAWBQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(CusHAWBSchema.CS_HAWB, @operator, value);
		}

		protected virtual void AddCoLoadMasterQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(CusHAWBSchema.CS_MasterHouseBill, @operator, value);
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
			underbondQuery.AddToFilter(JoinCondition.And, CusUnderbondSchema.C4_FlightNo, @operator, value);
			return underbondQuery;
		}

		protected virtual void AddConsolNumberQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (!((IZType)value).IsEmpty)
			{
				AddMasterQuery(query, SQLComparisonOperator.Equal, GetConsolPK((ZString)value), CusMAWBSchema.CM_JK);
			}
		}

		protected ZGuid GetConsolPK(ZString consolID)
		{
			if (consolID.Length < 9 && !consolID.StartsWith("C"))
			{
				consolID = consolID.PadLeft(8, '0');
				consolID = 'C' + consolID;
			}
			var consol = Factory.LoadFromNaturalKey<ForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, consolID);
			ZGuid consolPK = consol != null ? consol.PK : ZGuid.Invalid;
			return consolPK;
		}

		public virtual void AddMasterQuery(ZQuery query, SQLComparisonOperator @operator, object value, SchemaColumn columnName)
		{
			if (!((IZType)value).IsEmpty)
			{
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusMAWB), CusHAWBSchema.CS_CM);
				subQuery.AddToFilter(columnName, @operator, value);

				ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusHAWB));
				dBQuery.AddSubQuery(subQuery, JoinCondition.And);

				query.AddToFilter(dBQuery);
			}
		}

		#endregion

		#region Port Filters

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

		protected virtual void AddOutturnStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddDepotStatusFilter(query, @operator, value, CusEntryNumber.EntryType.OutturnStatus);
		}

		protected virtual void AddUnderbondStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddDepotStatusFilter(query, @operator, value, CusEntryNumber.EntryType.UnderbondStatus);
		}

		protected virtual void AddCMRMessageStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			StatusQuery(query, @operator, value, CusHAWBSchema.CS_MsgStatus);
		}

		protected virtual void AddFirstLandingQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			StatusQuery(query, @operator, value, CusHAWBSchema.CS_CustomsMainStatus);
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

		protected virtual void AddLastLandingQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			StatusQuery(query, @operator, value, CusHAWBSchema.CS_CustomsStatus);
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

	}
}

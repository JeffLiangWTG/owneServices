using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module.AirCargo
{
	public class AirCargoDepotStandAloneFilterStripBusinessObject : AUCustomsAirCargoFilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = base.GetModuleFiltersCore();
			AddEstablishmentFilters(filters);
			return filters;
		}

		#region Status Filter Overrides

		public override CodeDescriptionPairList GetStatusList(ZString statusType)
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

				case AirCargoFilterConstants.StatusFilterType.CMRCustoms:
					result = new CMRConsolidatedCargoStatuses();
					break;

				case AirCargoFilterConstants.StatusFilterType.CMRMessage:
					result = new CMRBaseStatuses();
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

		#region Establishment Filter

		void AddEstablishmentFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(AirCargoFilterConstants.EstablishmentTypes.DestinationAddress, GetDestinationAddressFilter).MaxLength = OrgAddressSchema.OA_Address1.MaxLength;
			filters.AddTextFilter(AirCargoFilterConstants.EstablishmentTypes.DestinationCode, GetDestinationCodeFilter).MaxLength = CusUnderbondSchema.C4_DestinationPremiseID.MaxLength;
			filters.AddTextFilter(AirCargoFilterConstants.EstablishmentTypes.OriginAddress, GetOriginAddressFilter).MaxLength = OrgAddressSchema.OA_Address1.MaxLength;
			filters.AddTextFilter(AirCargoFilterConstants.EstablishmentTypes.OriginCode, GetOriginCodeFilter).MaxLength = CusUnderbondSchema.C4_OriginPremiseID.MaxLength;
		}

		#endregion

		#region Establishment Filter Delegates

		ZQuery GetDestinationAddressFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddDestinationAddressFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetDestinationCodeFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddDestinationCodeFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetOriginAddressFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddOriginAddressFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetOriginCodeFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddOriginCodeFilter(query, comparisonOperator, value);
			return query;
		}

		#endregion

		#region Establishment Filter Implementation

		protected void AddAddressFilter(ZQuery query, SQLComparisonOperator @operator, SchemaColumn schemaColumn, object value)
		{
			ZDBOnlySubQuery addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			addressQuery.AddToFilter(OrgAddressSchema.OA_Address1, @operator, value);
			addressQuery.AddToFilter(JoinCondition.Or, OrgAddressSchema.OA_Address2, @operator, value);

			ZDBOnlySubQuery underbondQuery1 = new ZDBOnlySubQuery(typeof(CusUnderbond), CusUnderbondSchema.C4_ParentID);
			underbondQuery1.AddSubQuery(schemaColumn, addressQuery, JoinCondition.And);

			ZDBOnlyQuery mAWBQuery = new ZDBOnlyQuery(typeof(CusMAWB));
			mAWBQuery.AddSubQuery(underbondQuery1, JoinCondition.And);

			query.AddToFilter(mAWBQuery);
		}

		protected void AddOriginAddressFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddAddressFilter(query, @operator, CusUnderbondSchema.C4_OA_OriginAddress, value);
		}

		protected void AddDestinationAddressFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddAddressFilter(query, @operator, CusUnderbondSchema.C4_OA_DestinationAddress, value);
		}

		protected void AddCodeFilter(ZQuery query, SQLComparisonOperator @operator, SchemaColumn schemaColumn, object value)
		{
			ZDBOnlySubQuery underbondQuery = new ZDBOnlySubQuery(typeof(CusUnderbond), CusUnderbondSchema.C4_ParentID);
			underbondQuery.AddToFilter(schemaColumn, @operator, value);

			ZDBOnlyQuery mAWBQuery = new ZDBOnlyQuery(typeof(CusMAWB));
			mAWBQuery.AddSubQuery(underbondQuery, JoinCondition.And);

			//ZDBOnlyQuery DBQuery = new ZDBOnlyQuery(typeof(CusHAWB));
			//DBQuery.AddSubQuery(CusHAWBSchema.CS_CM.Name, MAWBQuery, JoinCondition.And);
			query.AddToFilter(mAWBQuery);
		}

		protected void AddOriginCodeFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddCodeFilter(query, @operator, CusUnderbondSchema.C4_OriginPremiseID, value);
		}

		protected void AddDestinationCodeFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddCodeFilter(query, @operator, CusUnderbondSchema.C4_DestinationPremiseID, value);
		}

		#endregion
	}
}


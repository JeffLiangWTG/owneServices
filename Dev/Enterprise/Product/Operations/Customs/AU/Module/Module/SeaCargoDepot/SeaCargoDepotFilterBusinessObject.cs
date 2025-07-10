using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public class SeaCargoDepotFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddNumberFilters(filters);
			AddDateFilters(filters);
			AddStatusFilters(filters);
			AddVoyageVesselFilters(filters);
			return filters;
		}

		#region Number Filter

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(SeaCargoDepotFilterConstants.NumberFilterTypes.OutturnReference, CusOutturnHeaderSchema.C6_SendersMessageReference);
			filters.AddNumberFilter(SeaCargoDepotFilterConstants.NumberFilterTypes.LloydsNumber, CusOutturnHeaderSchema.C6_LloydsIMO);
			filters.AddNumberFilter(SeaCargoDepotFilterConstants.NumberFilterTypes.MasterBillNumber, GetMasterBillQuery).MaxLength = CusOutturnSchema.C5_MasterBill.MaxLength;
			filters.AddNumberFilter(SeaCargoDepotFilterConstants.NumberFilterTypes.HouseBillNumber, GetHouseBillQuery).MaxLength = CusOutturnSchema.C5_HouseBill.MaxLength;
			filters.AddNumberFilter(SeaCargoDepotFilterConstants.NumberFilterTypes.ContainerNumber, GetContainerNumberQuery).MaxLength = CusOutturnSchema.C5_ContainerNumber.MaxLength;
			filters.AddNumberFilter(SeaCargoDepotFilterConstants.NumberFilterTypes.PremiseID, GetPremiseIDQuery).MaxLength = CusOutturnHeaderSchema.C6_OutturningPremiseID.MaxLength;
		}

		#endregion

		#region Number Filter Delegates

		ZQuery GetMasterBillQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddMasterBillFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetHouseBillQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddHouseBillFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetContainerNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddContainerNumberFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetPremiseIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddIfNotEmpty(query, CusOutturnHeaderSchema.C6_OutturningPremiseID, comparisonOperator, value, true);
			return query;
		}

		#endregion

		#region Number Filter Implementation

		void AddMasterBillFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddCusOutturnSubQuery(query, @operator, CusOutturnSchema.C5_MasterBill, value);
		}

		void AddHouseBillFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddCusOutturnSubQuery(query, @operator, CusOutturnSchema.C5_HouseBill, value);
		}

		void AddContainerNumberFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddCusOutturnSubQuery(query, @operator, CusOutturnSchema.C5_ContainerNumber, value);
		}

		#endregion

		#region Date Filter

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(SeaCargoDepotFilterConstants.DateFilterTypes.UnpackDate, GetUnpackDateQuery);
			filters.AddDateFilter(SeaCargoDepotFilterConstants.DateFilterTypes.CargoReceiptDate, GetReceiptDateQuery);
		}

		#endregion

		#region Date Filter Delegates

		ZQuery GetUnpackDateQuery(DateComparisonOperator comaprisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();

			AddCusOutturnSubQuery(query, (ZDBOnlySubQuery subQuery) => AddDateRange(subQuery, comaprisonOperator, JoinCondition.And, CusOutturnSchema.C5_CargoUnpackDate, fromDate.Date, toDate.Date));

			return query;
		}

		ZQuery GetReceiptDateQuery(DateComparisonOperator comaprisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();

			AddCusOutturnSubQuery(query, (ZDBOnlySubQuery subQuery) => AddDateRange(subQuery, comaprisonOperator, JoinCondition.And, CusOutturnSchema.C5_CargoReceiptDate, fromDate.Date, toDate.Date));

			return query;
		}

		#endregion

		#region Status Filter

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(FilterConstants.StatusTypes.Outturn, GetOutturnStatusQuery, GetStatusList(FilterConstants.StatusTypes.Outturn)).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter(FilterConstants.StatusTypes.Cargo, GetCargoStatusQuery, GetStatusList(FilterConstants.StatusTypes.Cargo)).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter(FilterConstants.StatusTypes.Commercial, GetCommercialStatusQuery, GetStatusList(FilterConstants.StatusTypes.Commercial)).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter(FilterConstants.StatusTypes.Underbond, GetUnderbondStatusQuery, GetStatusList(FilterConstants.StatusTypes.Underbond)).Category = FilterCategories.StatusAndFlags;
		}

		public virtual ReadOnlyCodeDescriptionPairList GetStatusList(ZString statusType)
		{
			ReadOnlyCodeDescriptionPairList result = null;

			switch (statusType)
			{
				case FilterConstants.StatusTypes.Outturn:
					result = new CMRBaseStatuses();
					break;

				case FilterConstants.StatusTypes.Commercial:
					result = Env.Registry.AUCustoms.SeaCargoCommercialStatus;
					break;

				case FilterConstants.StatusTypes.Cargo:
					result = new CMRConsolidatedCargoStatuses();
					break;

				case FilterConstants.StatusTypes.Underbond:
					result = new CMRUnderbondStatuses();
					break;

				default:
					result = new ReadOnlyCodeDescriptionPairList();
					break;
			}

			return result;
		}

		#endregion

		#region Status Filter Delegates

		ZQuery GetOutturnStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddOutturnStatusFilter(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetCargoStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddCusOutturnSubQuery(query, SQLComparisonOperator.Equal, CusOutturnSchema.C5_CustomsStatus, value);
			return query;
		}

		ZQuery GetCommercialStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddCusOutturnSubQuery(query, SQLComparisonOperator.Equal, CusOutturnSchema.C5_CommercialStatus, value);
			return query;
		}

		ZQuery GetUnderbondStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddCusOutturnSubQuery(query, SQLComparisonOperator.Equal, CusOutturnSchema.C5_MessageStatus, value);
			return query;
		}

		#endregion

		#region Status Filter Implementation

		void AddOutturnStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			query.AddToFilter(CusOutturnHeaderSchema.C6_MessageStatus, @operator, value);
			ZString stringValue = (ZString)value;
			if (stringValue.EqualsIgnoringCase(CMRBaseStatuses.Codes.NotSent))
			{
				query.AddToFilter(JoinCondition.Or, CusOutturnHeaderSchema.C6_MessageStatus, SQLComparisonOperator.Equal, ZString.Empty);
			}
		}

		#endregion

		#region Vessel / Voyage Filter

		public RefVesselCollection Vessels
		{
			get
			{
				return new RefVesselCollection(Factory);
			}
		}

		void AddVoyageVesselFilters(ModuleFilterCollection filters)
		{
			var voyageVesselFilter = filters.AddTextAndNkFilter("Vessel / Voyage", GetVoyageVesselQuery, ModuleIDs.RefVessel, Vessels);
			voyageVesselFilter.MaxLength = CusOutturnHeaderSchema.C6_VoyageNum.MaxLength;
			voyageVesselFilter.NkMaxLength = CusOutturnHeaderSchema.C6_VesselName.MaxLength;
			voyageVesselFilter.Category = FilterCategories.NumbersAndReferences;
		}

		#endregion

		#region Vessel Filter Delegates

		ZQuery GetVoyageVesselQuery(SQLComparisonOperator comparisonOperator, ZString voyage, ZString vesselNk)
		{
			ZQuery query = new ZQuery();

			if (!voyage.IsEmpty || comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				AddVoyageFilter(query, comparisonOperator, voyage);
			}

			if (!vesselNk.IsEmpty || comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				AddVesselFilter(query, comparisonOperator, vesselNk);
			}

			return query;
		}

		#endregion

		#region Vessel / Voyage Filter Implementation

		protected virtual void AddVesselFilter(ZQuery query, SQLComparisonOperator @operator, ZString value)
		{
			query.AddToFilter(CusOutturnHeaderSchema.C6_VesselName, @operator, value);
		}

		protected virtual void AddVoyageFilter(ZQuery query, SQLComparisonOperator @operator, ZString value)
		{
			query.AddToFilter(CusOutturnHeaderSchema.C6_VoyageNum, @operator, value);
		}

		#endregion

		#region Implementation

		delegate void AddSubQueryConditionsDelegate(ZDBOnlySubQuery query);

		void AddCusOutturnSubQuery(ZQuery query, AddSubQueryConditionsDelegate addSubQueryConditions)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusOutturn), CusOutturnSchema.C5_C6);
			addSubQueryConditions(subQuery);

			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusOutturnHeader));
			dBQuery.AddSubQuery(subQuery, JoinCondition.And);
			query.AddToFilter(dBQuery);
		}

		void AddCusOutturnSubQuery(ZQuery query, SQLComparisonOperator @operator, SchemaColumn column, object value)
		{
			AddCusOutturnSubQuery(query, (ZDBOnlySubQuery subQuery) => subQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, column, @operator, value));
		}

		#endregion
	}
}

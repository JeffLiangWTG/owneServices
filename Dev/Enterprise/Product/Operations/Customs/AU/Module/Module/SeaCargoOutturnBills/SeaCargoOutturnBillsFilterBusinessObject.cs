using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public class SeaCargoOutturnBillsFilterBusinessObject : FilterStripBusinessObject
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
			filters.AddNumberFilter(SeaCargoDepotFilterConstants.NumberFilterTypes.LloydsNumber, GetLloydsNumberQuery).MaxLength = CusOutturnHeaderSchema.C6_LloydsIMO.MaxLength;
			filters.AddNumberFilter(SeaCargoDepotFilterConstants.NumberFilterTypes.MasterBillNumber, CusOutturnSchema.C5_MasterBill);
			filters.AddNumberFilter(SeaCargoDepotFilterConstants.NumberFilterTypes.HouseBillNumber, CusOutturnSchema.C5_HouseBill);
			filters.AddNumberFilter(SeaCargoDepotFilterConstants.NumberFilterTypes.ContainerNumber, CusOutturnSchema.C5_ContainerNumber);
			filters.AddNumberFilter(SeaCargoDepotFilterConstants.NumberFilterTypes.ShipmentOrContainerJobID, GetShipmentOrContainerJobIDQuery).MaxLength = JobShipmentSchema.JS_UniqueConsignRef.MaxLength;
			filters.AddNumberFilter(SeaCargoDepotFilterConstants.NumberFilterTypes.PremiseID, GetPremiseIDQuery).MaxLength = CusOutturnHeaderSchema.C6_OutturningPremiseID.MaxLength;
		}

		#endregion

		#region Number Filter Delegates

		ZQuery GetLloydsNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddLloydsNumFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetShipmentOrContainerJobIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddShipmentOrContainerJobIDFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetPremiseIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddPremiseIDFilter(query, comparisonOperator, value);
			return query;
		}

		#endregion

		#region Number Filter Implementation

		void AddLloydsNumFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddCusOutturnHeaderSubQuery(query, @operator, CusOutturnHeaderSchema.C6_LloydsIMO, value);
		}

		void AddShipmentOrContainerJobIDFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery dbQuery = new ZDBOnlyQuery(typeof(CusOutturn));
			{
				ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CFSShipment), CusOutturnSchema.C5_ParentID);
				shipmentSubQuery.AddToFilter_PossiblyCommaSeparated(JobShipmentSchema.JS_UniqueConsignRef, @operator, value);

				dbQuery.AddSubQuery(shipmentSubQuery, JoinCondition.Or);
			}

			{
				ZDBOnlySubQuery containerSubQuery = new ZDBOnlySubQuery(typeof(CFSContainer), CusOutturnSchema.C5_ParentID);
				containerSubQuery.AddToFilter_PossiblyCommaSeparated(JobContainerSchema.JC_ContainerJobID, @operator, value);

				dbQuery.AddSubQuery(containerSubQuery, JoinCondition.Or);
			}

			query.AddToFilter(dbQuery);
		}

		void AddPremiseIDFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddCusOutturnHeaderSubQuery(query, @operator, CusOutturnHeaderSchema.C6_OutturningPremiseID, value);
		}

		#endregion

		#region Date Filter

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(SeaCargoDepotFilterConstants.DateFilterTypes.UnpackDate, CusOutturnSchema.C5_CargoUnpackDate);
			filters.AddDateFilter(SeaCargoDepotFilterConstants.DateFilterTypes.CargoReceiptDate, CusOutturnSchema.C5_CargoReceiptDate);
		}

		#endregion

		#region Status Filter

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(FilterConstants.StatusTypes.Cargo, GetCargoStatusQuery, GetStatusList(FilterConstants.StatusTypes.Cargo)).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter(FilterConstants.StatusTypes.Commercial, GetCommercialStatusQuery, GetStatusList(FilterConstants.StatusTypes.Commercial)).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter(FilterConstants.StatusTypes.Underbond, GetUnderbondStatusQuery, GetStatusList(FilterConstants.StatusTypes.Underbond)).Category = FilterCategories.StatusAndFlags;
		}

		public ReadOnlyCodeDescriptionPairList GetStatusList(ZString statusType)
		{
			ReadOnlyCodeDescriptionPairList result = null;

			switch (statusType)
			{
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

		ZQuery GetCargoStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddCargoStatusFilter(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetCommercialStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddCommercialStatusFilter(query, SQLComparisonOperator.Equal, value);
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

		protected void AddCargoStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery outturnQuery = new ZDBOnlyQuery(typeof(Business.CusOutturn));
			outturnQuery.AddToFilter(CusOutturnSchema.C5_CustomsStatus, @operator, value);
			query.AddToFilter(outturnQuery);
		}

		protected void AddCommercialStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery outturnQuery = new ZDBOnlyQuery(typeof(Business.CusOutturn));
			outturnQuery.AddToFilter(CusOutturnSchema.C5_CommercialStatus, @operator, value);
			query.AddToFilter(outturnQuery);
		}

		protected void AddUnderbondStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlyQuery outturnQuery = new ZDBOnlyQuery(typeof(Business.CusOutturn));
			outturnQuery.AddToFilter(CusOutturnSchema.C5_MessageStatus, @operator, value);
			query.AddToFilter(outturnQuery);
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
			var vesselVoyage = filters.AddTextAndNkFilter("Vessel / Voyage", GetVoyageVesselQuery, ModuleIDs.RefVessel, Vessels);
			vesselVoyage.MaxLength = CusOutturnHeaderSchema.C6_VoyageNum.MaxLength;
			vesselVoyage.NkMaxLength = CusOutturnHeaderSchema.C6_VesselName.MaxLength;
			vesselVoyage.Category = FilterCategories.NumbersAndReferences;
			vesselVoyage.SupportsBlankComparisonOperators = false;
		}

		#endregion

		#region Vessel / Voyage Filter Delegates

		ZQuery GetVoyageVesselQuery(SQLComparisonOperator comparisonOperator, ZString voyage, ZString vesselNk)
		{
			ZQuery query = new ZQuery();

			if (!voyage.IsEmpty)
			{
				AddVoyageFilter(query, comparisonOperator, voyage);
			}

			if (!vesselNk.IsEmpty)
			{
				AddVesselFilter(query, comparisonOperator, vesselNk);
			}

			return query;
		}

		#endregion

		#region Vessel / Voyage Filter Implementation

		protected virtual void AddVoyageFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddCusOutturnHeaderSubQuery(query, @operator, CusOutturnHeaderSchema.C6_VoyageNum, value);
		}

		protected virtual void AddVesselFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddCusOutturnHeaderSubQuery(query, @operator, CusOutturnHeaderSchema.C6_VesselName, value);
		}

		#endregion

		#region Implementation

		void AddCusOutturnHeaderSubQuery(ZQuery query, SQLComparisonOperator @operator, SchemaColumn column, object value)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusOutturnHeader), CusOutturnSchema.C5_C6);
			subQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, column, @operator, value);

			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusOutturn));
			dBQuery.AddSubQuery(subQuery, JoinCondition.And);
			query.AddToFilter(dBQuery);
		}

		#endregion
	}
}

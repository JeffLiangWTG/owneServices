using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public class VoyageManifestFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddNumberFilters(filters);
			AddLocationFilters(filters);
			AddOrganisationFilters(filters);
			AddStatusFilters(filters);
			AddDateFilters(filters);
			AddVoyageVesselFilters(filters);
			return filters;
		}

		#region Number Filter

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(VoyageManifestFilterConstants.NumberFilterTypes.OceanBill, GetOceanBillNumberQuery).MaxLength = CusSeaManOBLHeaderSchema.BO_OceanBill.MaxLength;
			filters.AddNumberFilter(VoyageManifestFilterConstants.NumberFilterTypes.Container, GetContainerNumberQuery).MaxLength = CusSeaManOBLDetailSchema.BD_ContainerNumber.MaxLength;
		}

		#endregion

		#region Number Filter Delegates

		ZQuery GetOceanBillNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddOceanBillQuery(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetContainerNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddContainerQuery(query, comparisonOperator, value);
			return query;
		}

		#endregion

		#region Number Filters Implemantation

		void AddOceanBillQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddHeaderQuery(query, @operator, value, CusSeaManOBLHeaderSchema.BO_OceanBill);
		}

		void AddContainerQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddDetailQuery(query, @operator, value, CusSeaManOBLDetailSchema.BD_ContainerNumber);
		}

		#endregion

		#region Location Filter

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			filters.AddLocationFilter(ZString.Format("{0} / {1}", VoyageManifestFilterConstants.PortFilterTypes.Load, VoyageManifestFilterConstants.PortFilterTypes.Discharge), GetLoadDischargeQuery, Locations, Locations).SetItemDescriptions(Res.GetData("fb378ade-5864-46be-8846-017045e8152a", "Load"), Res.GetData("5636b99f-9528-4bf4-b334-7425b7c98aa4", "Discharge"));
			filters.AddLocationFilter(ZString.Format("{0} / {1}", VoyageManifestFilterConstants.PortFilterTypes.Origin, VoyageManifestFilterConstants.PortFilterTypes.Destination), GetOriginDestinationQuery, Locations, Locations).SetItemDescriptions(Res.GetData("4475ffa1-a726-4afe-9427-6bba99ad18c7", "Origin"), Res.GetData("89ad80bc-5f4f-4a0b-ad5d-369df450564b", "Destination"));
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

				AddDischargeQuery(query, comparisonOperator, dischargeNk);
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

		#region Location Filter Implementation

		void AddLoadQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddHeaderQuery(query, @operator, value, CusSeaManOBLHeaderSchema.BO_RL_NKLoadPort);
		}

		void AddOriginQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddHeaderQuery(query, @operator, value, CusSeaManOBLHeaderSchema.BO_RL_NKOriginPort);
		}

		void AddDischargeQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddHeaderQuery(query, @operator, value, CusSeaManOBLHeaderSchema.BO_RL_NKDischargePort);
		}

		void AddDestinationQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddHeaderQuery(query, @operator, value, CusSeaManOBLHeaderSchema.BO_RL_NKDestinationPort);
		}

		#endregion

		#region Organisation Filter

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			ModuleGuidsFilter cnrCneFilter = filters.AddGuidFilter(String.Format("{0} / {1}", VoyageManifestFilterConstants.PartyFilterTypes.Consignor, VoyageManifestFilterConstants.PartyFilterTypes.Consignee), ModuleIDs.Organisation, GetConsignorConsigneeQuery, Consignors, Consignees);
			cnrCneFilter.SetItemDescriptions(Res.GetData("a9d70599-adb6-4a50-9dab-11ae9c9270b1", "Consignor"), Res.GetData("ce5db711-e514-42f9-8caa-dfe690d2f62e", "Consignee"));
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

		#region Organisation Filter Delegates

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

		#region Organisation Filter Implementation

		void AddConsigneeQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddHeaderQuery(query, @operator, value, CusSeaManOBLHeaderSchema.BO_OH_Consignee);
		}

		void AddConsignorQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddHeaderQuery(query, @operator, value, CusSeaManOBLHeaderSchema.BO_OH_Consignor);
		}

		#endregion

		#region Status Filter

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(VoyageManifestFilterConstants.StatusFilterTypes.ImpendingArrival, GetImpendingArrivalQuery, StatusList).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter(VoyageManifestFilterConstants.StatusFilterTypes.ActualArrival, GetActualArrivalQuery, StatusList).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter(VoyageManifestFilterConstants.StatusFilterTypes.CargoList, GetCargoListQuery, StatusList).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter(VoyageManifestFilterConstants.StatusFilterTypes.CargoReport, GetCargoReportQuery, StatusList).Category = FilterCategories.StatusAndFlags;
		}

		public CMRBaseStatuses StatusList
		{
			get { return new CMRBaseStatuses(); }
		}

		#endregion

		#region Status Filter Delegates

		ZQuery GetImpendingArrivalQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddImpendingArrivalQuery(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetActualArrivalQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddActualArrivalQuery(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetCargoListQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddCargoListQuery(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetCargoReportQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddCargoReportQuery(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		#endregion

		#region Status Filter Implementation

		void AddImpendingArrivalQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumber.EntryType.ImpendingArrivalResponseStatus);
			subQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusSeaManTranHead.Schema.TableName);
			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryStatus, @operator, value);

			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusSeaManTranHead));
			dBQuery.AddSubQuery(subQuery, JoinCondition.And);

			query.AddToFilter(dBQuery);
		}

		void AddActualArrivalQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddHeaderStatusQuery(query, @operator, value, CusEntryNumber.EntryType.ActualArrivalResponseStatus, CusSeaManArrivalPort.Schema.TableName, typeof(CusSeaManArrivalPort), CusSeaManArrivalPortSchema.BA_BT);
		}

		void AddCargoListQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddHeaderStatusQuery(query, @operator, value, CusEntryNumber.EntryType.CargoListStatus, CusSeaManArrivalPort.Schema.TableName, typeof(CusSeaManArrivalPort), CusSeaManArrivalPortSchema.BA_BT);
		}

		void AddCargoReportQuery(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddHeaderStatusQuery(query, @operator, value, CusEntryNumber.EntryType.CargoReportStatus, CusSeaManOBLHeader.Schema.TableName, typeof(CusSeaManOBLHeader), CusSeaManOBLHeaderSchema.BO_BT);
		}

		void AddHeaderStatusQuery(ZQuery query, SQLComparisonOperator @operator, object value, string statusType, string tableName, Type businessObjectType, SchemaColumn businessObjectSchemaColumn)
		{
			if (!((IZType)value).IsEmpty)
			{
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, statusType);
				subQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, tableName);
				subQuery.AddToFilter(CusEntryNumSchema.CE_EntryStatus, @operator, value);

				ZDBOnlySubQuery subQueryOBL = new ZDBOnlySubQuery(businessObjectType, businessObjectSchemaColumn);
				subQueryOBL.AddSubQuery(subQuery, JoinCondition.And);

				ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusSeaManTranHead));
				dBQuery.AddSubQuery(subQueryOBL, JoinCondition.And);

				query.AddToFilter(dBQuery);
			}
		}

		#endregion

		#region Date Filter

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(VoyageManifestFilterConstants.DateFilterTypes.ActualArrivalDate, GetActualArrivalDateQuery);
			filters.AddDateFilter(VoyageManifestFilterConstants.DateFilterTypes.EstimatedArrivalDate, GetEstimatedArrivalDateQuery);
			filters.AddDateFilter(VoyageManifestFilterConstants.DateFilterTypes.DepartureDate, GetDepartureDateQuery);
		}

		#endregion

		#region Date Filter Delegates

		ZQuery GetActualArrivalDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddActualArrivalDateFilter(query, comparisonOperator, fromDate, toDate);
			return query;
		}

		ZQuery GetEstimatedArrivalDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddEstimatedArrivalDateFilter(query, comparisonOperator, fromDate, toDate);
			return query;
		}

		ZQuery GetDepartureDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZQuery query = new ZQuery();
			AddDepartureDateFilter(query, comparisonOperator, fromDate, toDate);
			return query;
		}

		#endregion

		#region Date Filter Implementation

		void AddActualArrivalDateFilter(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			AddArrivalQuery(query, comparisonOperator, dateFrom, dateTo, CusSeaManArrivalPortSchema.BA_ArrivalPortATA);
		}

		void AddEstimatedArrivalDateFilter(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			AddArrivalQuery(query, comparisonOperator, dateFrom, dateTo, CusSeaManArrivalPortSchema.BA_ArrivalPortETA);
		}

		void AddDepartureDateFilter(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			AddDateRange(query, comparisonOperator, JoinCondition.And, CusSeaManTranHeadSchema.BT_PortOfLastForeignPortATD, dateFrom.Date, dateTo.Date);
		}

		#endregion

		#region Vessel / Voyage Filter

		void AddVoyageVesselFilters(ModuleFilterCollection filters)
		{
			filters.AddTextAndNkFilter("Vessel / Voyage", GetVoyageVesselQuery, ModuleIDs.RefVessel, Vessels).Category = FilterCategories.NumbersAndReferences;
		}

		public RefVesselCollection Vessels
		{
			get
			{
				return new RefVesselCollection(Factory);
			}
		}

		#endregion

		#region Vessel / Voyage Filter Delegates

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
			query.AddToFilter(CusSeaManTranHeadSchema.BT_VesselName, @operator, value);
		}

		protected virtual void AddVoyageFilter(ZQuery query, SQLComparisonOperator @operator, ZString value)
		{
			query.AddToFilter(CusSeaManTranHeadSchema.BT_VoyageNum, @operator, value.SubstringSafe(0, CusSeaManTranHeadSchema.BT_VoyageNum.MaxLength));
		}

		#endregion

		#region Implementation

		void AddArrivalQuery(ZQuery query, DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo, SchemaDateTimeColumn columnName)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusSeaManArrivalPort), CusSeaManArrivalPortSchema.BA_BT);
			AddDateRange(subQuery, comparisonOperator, JoinCondition.And, columnName, dateFrom.Date, dateTo.Date);

			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusSeaManTranHead));
			dBQuery.AddSubQuery(subQuery, JoinCondition.And);

			query.AddToFilter(dBQuery);
		}

		void AddHeaderQuery(ZQuery query, SQLComparisonOperator @operator, object value, SchemaColumn columnName)
		{
			if (!((IZType)value).IsEmpty)
			{
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusSeaManOBLHeader), CusSeaManOBLHeaderSchema.BO_BT);
				subQuery.AddToFilter_PossiblyCommaSeparated(columnName, @operator, value);

				ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusSeaManTranHead));
				dBQuery.AddSubQuery(subQuery, JoinCondition.And);

				query.AddToFilter(dBQuery);
			}
		}

		void AddDetailQuery(ZQuery query, SQLComparisonOperator @operator, object value, SchemaColumn columnName)
		{
			if (!((IZType)value).IsEmpty)
			{
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusSeaManOBLDetail), CusSeaManOBLDetailSchema.BD_BO);
				subQuery.AddToFilter_PossiblyCommaSeparated(columnName, @operator, value);

				ZDBOnlySubQuery subQueryHeader = new ZDBOnlySubQuery(typeof(CusSeaManOBLHeader), CusSeaManOBLHeaderSchema.BO_BT);
				subQueryHeader.AddSubQuery(subQuery, JoinCondition.And);

				ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusSeaManTranHead));
				dBQuery.AddSubQuery(subQueryHeader, JoinCondition.And);

				query.AddToFilter(dBQuery);
			}
		}

		#endregion
	}
}

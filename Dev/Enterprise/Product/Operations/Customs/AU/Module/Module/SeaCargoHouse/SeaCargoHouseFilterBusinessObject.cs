using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public class SeaCargoHouseFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddNumberFilters(filters);
			AddLocationFilters(filters);
			AddStatusFilters(filters);
			AddDateFilters(filters);
			AddEstablishmentFilters(filters);
			return filters;
		}

		#region Number Filters

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(SeaCargoFilterConstants.NumberFilterTypes.ContainerNumber, GetContainerNumberFilterQuery).WithMaxLengthOf<ModuleNumberFilter>(CusSCAContainerSchema.CN_ContainerNumber).SubGroup = HousePivotSubGroup;
			filters.AddNumberFilter(SeaCargoFilterConstants.NumberFilterTypes.HouseBillNumber, GetHouseBillNumberQuery).WithMaxLengthOf<ModuleNumberFilter>(CusSCAHouseSchema.CA_HouseBill);
			filters.AddNumberFilter(SeaCargoFilterConstants.NumberFilterTypes.OceanBillNumber, GetOceanBillNumberFilterQuery).WithMaxLengthOf<ModuleNumberFilter>(CusSCAOceanBillSchema.CB_OceanBill).SubGroup = HouseOceanBillSubGroup;
			filters.AddNumberFilter(SeaCargoFilterConstants.NumberFilterTypes.ParentBillNumber, GetParentBillNumberQuery).WithMaxLengthOf<ModuleNumberFilter>(CusSCAHouseSchema.CA_MasterHouseBill);
			filters.AddNumberFilter(SeaCargoFilterConstants.NumberFilterTypes.ResponsiblePartyID, GetResponsiblePartyIDQuery).WithMaxLengthOf<ModuleNumberFilter>(CusSCAHouseSchema.CA_ResponsiblePartyID);

			var vesselVoyageNumberFilter = filters.AddTextAndNkFilter(SeaCargoFilterConstants.NumberFilterTypes.VesselVoyage, GetVesselVoyageNumberFilterQuery, ZArchitecture.Modules.ModuleIDs.RefVessel, new RefVesselCollection(Factory));
			SetSubGroupAndCategory(vesselVoyageNumberFilter, HouseOceanBillSubGroup, FilterCategories.NumbersAndReferences);
		}

		ZQuery GetContainerNumberFilterQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var containerQuery = new ZDBOnlySubQuery(typeof(CusSCAContainer), CusSCAPivotSchema.CV_CN);
			containerQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, CusSCAContainerSchema.CN_ContainerNumber, comparisonOperator, value);

			var pivotQuery = new ZDBOnlyQuery(typeof(CusSCAPivot));
			pivotQuery.AddSubQuery(containerQuery, JoinCondition.And);
			return pivotQuery;
		}

		ZQuery GetHouseBillNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery().AddToFilter_PossiblyCommaSeparated(CusSCAHouseSchema.CA_HouseBill, comparisonOperator, value);
		}

		ZQuery GetOceanBillNumberFilterQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery().AddToFilter_PossiblyCommaSeparated(JoinCondition.And, CusSCAOceanBillSchema.CB_OceanBill, comparisonOperator, value);
		}

		ZQuery GetParentBillNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery().AddToFilter_PossiblyCommaSeparated(CusSCAHouseSchema.CA_MasterHouseBill, comparisonOperator, value);
		}

		ZQuery GetResponsiblePartyIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery().AddToFilter_PossiblyCommaSeparated(CusSCAHouseSchema.CA_ResponsiblePartyID, comparisonOperator, value);
		}

		ZQuery GetVesselVoyageNumberFilterQuery(SQLComparisonOperator comparisonOperator, ZString voyage, ZString vesselNk)
		{
			var query = new ZQuery();

			if (!voyage.IsEmpty || comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter(CusSCAOceanBillSchema.CB_Voyage, comparisonOperator, voyage.Left(CusSCAOceanBill.Schema.CB_VoyageMaxLength));
			}
			if (!vesselNk.IsEmpty || comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter(CusSCAOceanBillSchema.CB_VesselName, comparisonOperator, vesselNk.Left(CusSCAOceanBill.Schema.CB_VesselNameMaxLength));
			}

			return query;
		}

		#endregion

		#region Location Filters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var loadingDischargeFilter = filters.AddLocationFilter(SeaCargoFilterConstants.PortFilterTypes.LoadingDischarge, GetLoadDischargeQuery, Locations, Locations);
			loadingDischargeFilter.SetItemDescriptions(Res.GetData("DB8CB3E8-D21E-480B-9C39-CF657A4C7C6D", "Load"), Res.GetData("03C6FC99-099A-4C95-A80D-74FB92BFD840", "Discharge"));
			loadingDischargeFilter.SubGroup = HouseOceanBillSubGroup;

			var originDestinationFilter = filters.AddLocationFilter(SeaCargoFilterConstants.PortFilterTypes.OriginDestination, GetOriginDestinationQuery, Locations, Locations);
			originDestinationFilter.SetItemDescriptions(Res.GetData("01d1b24a-90a5-47ec-a2e9-7df69f26c87e", "Port of Origin"), Res.GetData("f4ea1fa9-8e3e-425a-88e3-63b19d8f10dc", "Destination"));
		}

		ZQuery GetLoadDischargeQuery(ZString loadNk, ZString dischargeNk)
		{
			ZQuery query = new ZQuery();

			if (!loadNk.IsEmpty)
			{
				query.AddToFilter(CreateLocationQuery(CusSCAOceanBillSchema.CB_RL_NKPortOfLoading, loadNk, typeof(CusSCAOceanBill)));
			}

			if (!dischargeNk.IsEmpty)
			{
				query.AddToFilter(CreateLocationQuery(CusSCAOceanBillSchema.CB_RL_NKPortOfDischarge, dischargeNk, typeof(CusSCAOceanBill)));
			}

			return query;
		}

		ZQuery GetOriginDestinationQuery(ZString originNk, ZString destinationNk)
		{
			var query = new ZQuery();

			if (!originNk.IsEmpty)
			{
				query.AddToFilter(CreateLocationQuery(CusSCAHouseSchema.CA_RL_NK_PortOfOrigin, originNk, typeof(CusSCAHouse)));
			}

			if (!destinationNk.IsEmpty)
			{
				query.AddToFilter(CreateLocationQuery(CusSCAHouseSchema.CA_RL_NK_PortOfDestination, destinationNk, typeof(CusSCAHouse)));
			}

			return query;
		}

		ZQuery CreateLocationQuery(SchemaColumn column, ZString port, Type bizoType)
		{
			var isCountryCode = (port.Length == 2);
			var comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

			var provider = new LocationQueryProvider(Factory, column, bizoType);
			return provider.GetQuery(comparisonOperator, port);
		}

		LocationCollection Locations => locations ?? (locations = new LocationCollection(Factory));
		LocationCollection locations;

		#endregion

		#region Status Filters

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(SeaCargoFilterConstants.StatusFilterTypes.CustomsStatus, GetCustomsCargoStatusFilterQuery, CMRConsolidatedCargoStatuses.SeaFilterStatuses).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter(SeaCargoFilterConstants.StatusFilterTypes.MessageStatus, GetCustomsMessageStatusQuery, new CMRBaseStatuses()).Category = FilterCategories.StatusAndFlags;

			var underbondStatusFilter = filters.AddTextFilter(SeaCargoFilterConstants.StatusFilterTypes.UnderbondStatus, GetUnderbondStatusFilterQuery, new CMRUnderbondStatuses());
			SetSubGroupAndCategory(underbondStatusFilter, HouseUnderbondSubGroup, FilterCategories.StatusAndFlags);
		}

		ZQuery GetCustomsCargoStatusFilterQuery(ZString value)
		{
			if (value == CMRConsolidatedCargoStatuses.Filter.Codes.NotClear)
			{
				return new ZQuery(CusSCAHouseSchema.CA_ShipmentStatus, SQLComparisonOperator.NotEqual, CMRConsolidatedCargoStatuses.AllClearStatus.GetAllCodes());
			}

			return new ZQuery(CusSCAHouseSchema.CA_ShipmentStatus, value);
		}

		ZQuery GetCustomsMessageStatusQuery(ZString value)
		{
			object queryValue = value == CMRBaseStatuses.Codes.NotSent ? new ZString[] { CMRBaseStatuses.Codes.NotSent, ZString.Empty } : value;
			return new ZQuery(CusSCAHouseSchema.CA_MessageStatus, queryValue);
		}

		ZQuery GetUnderbondStatusFilterQuery(ZString value)
		{
			var entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumber.EntryType.UnderbondStatus);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryStatus, value);

			var underbondQuery = new ZDBOnlyQuery(typeof(CusUnderbond));
			underbondQuery.AddSubQuery(entryNumQuery, JoinCondition.And);
			return underbondQuery;
		}

		#endregion

		#region Date Filters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			var arrivalDateFilter = filters.AddDateFilter(SeaCargoFilterConstants.DateFilterTypes.ArrivalDate, ArrivalDateQuery);
			SetSubGroupAndCategory(arrivalDateFilter, HouseOceanBillSubGroup, FilterCategories.Dates);
			var departureDateFilter = filters.AddDateFilter(SeaCargoFilterConstants.DateFilterTypes.DepartureDate, DepartureDateQuery);
			SetSubGroupAndCategory(departureDateFilter, HouseOceanBillSubGroup, FilterCategories.Dates);
			var firstArrivalDateFilter = filters.AddDateFilter(SeaCargoFilterConstants.DateFilterTypes.FirstArrivalDate, FirstArrivalDateQuery);
			SetSubGroupAndCategory(firstArrivalDateFilter, HouseOceanBillSubGroup, FilterCategories.Dates);
		}

		ZQuery ArrivalDateQuery(DateComparisonOperator comparisonOperator, ZDateTimeOffset value1, ZDateTimeOffset value2)
		{
			return DateQuery(CusSCAOceanBillSchema.CB_DateOfArrival, comparisonOperator, value1, value2);
		}

		ZQuery DepartureDateQuery(DateComparisonOperator comparisonOperator, ZDateTimeOffset value1, ZDateTimeOffset value2)
		{
			return DateQuery(CusSCAOceanBillSchema.CB_DateOfDeparture, comparisonOperator, value1, value2);
		}

		ZQuery FirstArrivalDateQuery(DateComparisonOperator comparisonOperator, ZDateTimeOffset value1, ZDateTimeOffset value2)
		{
			return DateQuery(CusSCAOceanBillSchema.CB_DateOfFirstArrival, comparisonOperator, value1, value2);
		}

		ZQuery DateQuery(SchemaDateTimeColumn column, DateComparisonOperator comparisonOperator, ZDateTimeOffset value1, ZDateTimeOffset value2)
		{
			var query = new ZQuery();
			AddDateRange(query, comparisonOperator, JoinCondition.And, column, value1.Date, value2.Date);
			return query;
		}

		#endregion

		#region Establishment Filter

		void AddEstablishmentFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(SeaCargoFilterConstants.EstablishmentTypes.DestinationAddress, GetDestinationAddressFilterQuery).WithMaxLengthOf<ModuleTextFilter>(OrgAddressSchema.OA_Address1).SubGroup = HouseUnderbondSubGroup;
			filters.AddTextFilter(SeaCargoFilterConstants.EstablishmentTypes.OriginAddress, GetOriginAddressFilterQuery).WithMaxLengthOf<ModuleTextFilter>(OrgAddressSchema.OA_Address1).SubGroup = HouseUnderbondSubGroup;
		}

		ZQuery GetDestinationAddressFilterQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressFilterQuery(CusUnderbondSchema.C4_OA_DestinationAddress, comparisonOperator, value);
		}

		ZQuery GetOriginAddressFilterQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressFilterQuery(CusUnderbondSchema.C4_OA_OriginAddress, comparisonOperator, value);
		}

		ZQuery GetAddressFilterQuery(SchemaColumn column, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var joinCondition = comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator.IsNegativeSQLOperator() ? JoinCondition.And : JoinCondition.Or;

			var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			addressQuery.AddToFilter(OrgAddressSchema.OA_Address1, comparisonOperator, value);
			addressQuery.AddToFilter(joinCondition, OrgAddressSchema.OA_Address2, comparisonOperator, value);

			var underbondQuery = new ZDBOnlyQuery(typeof(CusUnderbond));
			underbondQuery.AddSubQuery(column, addressQuery, JoinCondition.And);

			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				underbondQuery.AddToFilter(JoinCondition.Or, column, null);
			}

			return underbondQuery;
		}

		#endregion

		#region Custom Fields Filters

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var workflowHelper = new WorkflowFilterStripsHelper(typeof(CusSCAHouse), WorkflowDescriptors.CusSCAHouseWorkflowDescriptorCode, Factory);
			workflowHelper.SetShouldAddWorkflowCustomFieldsFilters(true);

			var helpers = base.GetCustomFilterStripsHelpersCore();
			helpers.Add(workflowHelper);
			return helpers;
		}

		#endregion

		#region SubGroups

		HouseOceanBillModuleFilterSubGroup HouseOceanBillSubGroup => houseOceanBillSubGroup ?? (houseOceanBillSubGroup = new HouseOceanBillModuleFilterSubGroup());
		HouseOceanBillModuleFilterSubGroup houseOceanBillSubGroup;

		HousePivotModuleFilterSubGroup HousePivotSubGroup => housePivotSubGroup ?? (housePivotSubGroup = new HousePivotModuleFilterSubGroup());
		HousePivotModuleFilterSubGroup housePivotSubGroup;

		HouseUnderbondModuleFilterSubGroup HouseUnderbondSubGroup => houseUnderbondSubGroup ?? (houseUnderbondSubGroup = new HouseUnderbondModuleFilterSubGroup());
		HouseUnderbondModuleFilterSubGroup houseUnderbondSubGroup;

		class HouseOceanBillModuleFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filterQuery)
			{
				var oceanBillQuery = new ZDBOnlySubQuery(typeof(CusSCAOceanBill), CusSCAHouseSchema.CA_CB);
				oceanBillQuery.AddToFilter(filterQuery);

				var houseQuery = new ZDBOnlyQuery(typeof(CusSCAHouse));
				houseQuery.AddSubQuery(oceanBillQuery, JoinCondition.And);
				return houseQuery;
			}
		}

		class HousePivotModuleFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filterQuery)
			{
				var pivotQuery = new ZDBOnlySubQuery(typeof(CusSCAPivot), CusSCAPivotSchema.CV_CA);
				pivotQuery.AddToFilter(filterQuery);

				var houseQuery = new ZDBOnlyQuery(typeof(CusSCAHouse));
				houseQuery.AddSubQuery(pivotQuery, JoinCondition.And);
				return houseQuery;
			}
		}

		class HouseUnderbondModuleFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filterQuery)
			{
				var underbondQuery = new ZDBOnlySubQuery(typeof(CusUnderbond), CusUnderbondSchema.C4_ParentID);
				underbondQuery.AddToFilter(filterQuery);

				var pivotQuery = new ZDBOnlySubQuery(typeof(CusSCAPivot), CusSCAPivotSchema.CV_CA);
				pivotQuery.AddSubQuery(underbondQuery, JoinCondition.And);

				var houseQuery = new ZDBOnlyQuery(typeof(CusSCAHouse));
				houseQuery.AddSubQuery(pivotQuery, JoinCondition.And);
				return houseQuery;
			}
		}

		void SetSubGroupAndCategory(ModuleFilter filter, ModuleFilterSubGroup subGroup, FilterCategory category)
		{
			filter.SubGroup = subGroup;
			filter.Category = category;
		}

		#endregion
	}
}

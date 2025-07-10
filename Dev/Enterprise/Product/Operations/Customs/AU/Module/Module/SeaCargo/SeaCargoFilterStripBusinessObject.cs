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
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Module
{
	public class SeaCargoFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddNumberFilters(filters);
			AddDateFilters(filters);
			AddLocationFilters(filters);
			AddStatusFilters(filters);
			AddEstablishmentFilters(filters);
			AddVoyageVesselFilters(filters);
			return filters;
		}

		#region workflow Custom Fields filter
		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var workflowHelper = new WorkflowFilterStripsHelper(typeof(CusSCAOceanBill), WorkflowDescriptors.CusSCAOceanBillWorkflowDescriptorCode, Factory);
			workflowHelper.SetShouldAddWorkflowCustomFieldsFilters(true);
			helpers.Add(workflowHelper);
			return helpers;
		}
		#endregion

		#region Filter

		public override ZQuery Filter
		{
			get
			{
				var result = base.Filter;
				result.AddToFilter(CusSCAOceanBillSchema.CB_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
				result.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, CusSCAOceanBill.ApplicationCodes);
				return result;
			}
		}

		#endregion // Filter

		#region Number Filters

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(SeaCargoFilterConstants.NumberFilterTypes.OceanBillNumber, CusSCAOceanBillSchema.CB_OceanBill);
			filters.AddNumberFilter(SeaCargoFilterConstants.NumberFilterTypes.HouseBillNumber, GetHouseBillControlFilter).MaxLength = CusSCAHouseSchema.CA_HouseBill.MaxLength;
			filters.AddNumberFilter(SeaCargoFilterConstants.NumberFilterTypes.ParentBillNumber, CusSCAOceanBillSchema.CB_MasterHouseBill);
			filters.AddNumberFilter(SeaCargoFilterConstants.NumberFilterTypes.ContainerNumber, GetContainerNumberFilter).MaxLength = CusSCAContainerSchema.CN_ContainerNumber.MaxLength;
			filters.AddNumberFilter(SeaCargoFilterConstants.NumberFilterTypes.ResponsiblePartyID, CusSCAOceanBillSchema.CB_ResponsiblePartyID);
		}

		#endregion

		#region Date Filters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(SeaCargoFilterConstants.DateFilterTypes.DepartureDate, CusSCAOceanBillSchema.CB_DateOfDeparture).Category = FilterCategories.Dates;
			filters.AddDateFilter(SeaCargoFilterConstants.DateFilterTypes.ArrivalDate, CusSCAOceanBillSchema.CB_DateOfArrival).Category = FilterCategories.Dates;
			filters.AddDateFilter(SeaCargoFilterConstants.DateFilterTypes.FirstArrivalDate, CusSCAOceanBillSchema.CB_DateOfFirstArrival).Category = FilterCategories.Dates;
		}

		#endregion

		#region Number Filter Delegates

		ZQuery GetHouseBillControlFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddHouseBillControlFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetContainerNumberFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddContainerNumberFilter(query, comparisonOperator, value);
			return query;
		}

		#endregion

		#region Number Filter Implementation

		protected virtual void AddHouseBillControlFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusSCAHouse), CusSCAHouseSchema.CA_CB);
			subQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, CusSCAHouseSchema.CA_HouseBill, @operator, value);

			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusSCAOceanBill));
			dBQuery.AddSubQuery(subQuery, JoinCondition.And);
			query.AddToFilter(dBQuery);
		}

		protected virtual void AddContainerNumberFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlySubQuery containers = new ZDBOnlySubQuery(typeof(CusSCAContainer), CusSCAContainerSchema.CN_CB);
			containers.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, CusSCAContainerSchema.CN_ContainerNumber, @operator, value);

			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusSCAOceanBill));
			dBQuery.AddSubQuery(containers, JoinCondition.And);
			query.AddToFilter(dBQuery);
		}

		#endregion

		#region Location Filter

		public LocationCollection Locations
		{
			get
			{
				return new LocationCollection(Factory);
			}
		}

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			filters.AddLocationFilter(SeaCargoFilterConstants.PortFilterTypes.LoadingDischarge, GetLoadDischargeQuery, Locations, Locations).SetItemDescriptions(Res.GetData("c691bdbd-ba35-4b4c-9bc8-892d2a94a952", "Load"), Res.GetData("74396f54-fef0-44ba-bd75-d51833f86a42", "Discharge"));
			filters.AddLocationFilter(SeaCargoFilterConstants.PortFilterTypes.OriginDestination, GetOriginDestinationQuery, Locations, Locations).SetItemDescriptions(Res.GetData("34ccdf62-7f25-4693-946e-7c8451e6618c", "Origin"), Res.GetData("21d0478f-e45e-4c7f-9d2b-3ee76c8b7724", "Destination"));
		}

		#endregion

		#region Location Filter Delegates

		ZQuery GetLoadDischargeQuery(ZString loadNk, ZString dischargeNk)
		{
			ZQuery query = new ZQuery();

			if (!loadNk.IsEmpty)
			{
				bool isCountryCode = (loadNk.Length == 2);
				SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

				AddPortOfLoadFilter(query, comparisonOperator, loadNk);
			}

			if (!dischargeNk.IsEmpty)
			{
				bool isCountryCode = (dischargeNk.Length == 2);
				SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

				AddPortOfDischargeFilter(query, comparisonOperator, dischargeNk);
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

				AddPortOfOriginFilter(query, comparisonOperator, originNk);
			}

			if (!destinationNk.IsEmpty)
			{
				bool isCountryCode = (destinationNk.Length == 2);
				SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

				AddPortOfDestinationFilter(query, comparisonOperator, destinationNk);
			}

			return query;
		}

		#endregion

		#region Location Filter Implementation

		protected virtual void AddPortOfLoadFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			LocationQueryProvider provider = new LocationQueryProvider(Factory, CusSCAOceanBillSchema.CB_RL_NKPortOfLoading, typeof(CusSCAOceanBill));
			query.AddToFilter(provider.GetQuery(@operator, value));
		}

		protected virtual void AddPortOfDischargeFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			LocationQueryProvider provider = new LocationQueryProvider(Factory, CusSCAOceanBillSchema.CB_RL_NKPortOfDischarge, typeof(CusSCAOceanBill));
			query.AddToFilter(provider.GetQuery(@operator, value));
		}

		protected virtual void AddPortOfOriginFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddPortFilter(query, @operator, CusSCAHouseSchema.CA_RL_NK_PortOfOrigin, value);
		}

		protected virtual void AddPortOfDestinationFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			AddPortFilter(query, @operator, CusSCAHouseSchema.CA_RL_NK_PortOfDestination, value);
		}

		protected virtual void AddPortFilter(ZQuery query, SQLComparisonOperator @operator, SchemaColumn schemaColumn, object value)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusSCAHouse), CusSCAHouseSchema.CA_CB);
			subQuery.AddToFilter(JoinCondition.And, schemaColumn, @operator, value);

			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusSCAOceanBill));
			dBQuery.AddSubQuery(subQuery, JoinCondition.And);
			query.AddToFilter(dBQuery);
		}

		protected virtual void AddPortOfFirstArrivalFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZQuery filter = new ZQuery(CusSCAOceanBillSchema.CB_RL_NKPortOfFirstArrival, @operator, value);
			query.AddToFilter(filter);
		}

		#endregion

		#region Status Filter

		void AddStatusFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(SeaCargoFilterConstants.StatusFilterTypes.CustomsStatus, GetCustomsStatusQuery, GetStatusList(SeaCargoFilterConstants.StatusFilterTypes.CustomsStatus)).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter(SeaCargoFilterConstants.StatusFilterTypes.MessageStatus, GetMessageStatusQuery, GetStatusList(SeaCargoFilterConstants.StatusFilterTypes.MessageStatus)).Category = FilterCategories.StatusAndFlags;
			filters.AddTextFilter(SeaCargoFilterConstants.StatusFilterTypes.UnderbondStatus, GetUnderbondStatusQuery, GetStatusList(SeaCargoFilterConstants.StatusFilterTypes.UnderbondStatus)).Category = FilterCategories.StatusAndFlags;
		}

		public virtual CodeDescriptionPairList GetStatusList(ZString statusType)
		{
			CodeDescriptionPairList result;
			switch (statusType)
			{
				case SeaCargoFilterConstants.StatusFilterTypes.UnderbondStatus:
					result = new CMRUnderbondStatuses();
					break;

				case SeaCargoFilterConstants.StatusFilterTypes.OutturnStatus:
					result = new CMRAllStatuses();
					break;

				case SeaCargoFilterConstants.StatusFilterTypes.CustomsStatus:
					result = CMRConsolidatedCargoStatuses.SeaFilterStatuses;
					break;

				case SeaCargoFilterConstants.StatusFilterTypes.MessageStatus:
					result = new CMRBaseStatuses();
					break;

				default:
					result = new CodeDescriptionPairList();
					result.AddRange(new CMRAllStatuses());
					break;
			}
			return result;
		}

		#endregion

		#region Status Filter Delegates

		ZQuery GetCustomsStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddCustomsStatusFilter(query, SQLComparisonOperator.Equal, value);
			return query;
		}

		ZQuery GetMessageStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			AddMessageStatusFilter(query, SQLComparisonOperator.Equal, value);
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

		protected virtual void AddCustomsStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			if (value.ToString() == CMRConsolidatedCargoStatuses.Filter.Codes.NotClear)
			{
				foreach (CodeDescriptionPair codeDescription in CMRConsolidatedCargoStatuses.AllClearStatus)
				{
					AddPivotCargoStatusQueryFilter(query, SQLComparisonOperator.NotEqual, (ZString)codeDescription.Code);
				}
			}
			else
			{
				AddPivotCargoStatusQueryFilter(query, @operator, value);
			}
		}

		void AddPivotCargoStatusQueryFilter(ZQuery query, SQLComparisonOperator oper, object value)
		{
			var houseQuery = new ZDBOnlySubQuery(typeof(CusSCAHouse), CusSCAHouseSchema.CA_CB);
			var pivotQuery = new ZDBOnlySubQuery(typeof(CusSCAPivot), CusSCAPivotSchema.CV_CA);
			pivotQuery.AddToFilter(JoinCondition.And, CusSCAPivotSchema.CV_CargoStatus, oper, value);
			houseQuery.AddSubQuery(pivotQuery, JoinCondition.And);

			var oceanBillQuery = new ZDBOnlyQuery(typeof(CusSCAOceanBill));
			oceanBillQuery.AddSubQuery(houseQuery, JoinCondition.And);
			query.AddToFilter(oceanBillQuery);
		}

		protected virtual void AddMessageStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			object valueToCompare;
			switch (value.ToString())
			{
				case CMRBaseStatuses.Codes.NotSent:
					valueToCompare = new ZString[] { CMRBaseStatuses.Codes.NotSent, ZString.Empty };
					break;
				default:
					valueToCompare = value;
					break;
			}
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusSCAHouse), CusSCAHouseSchema.CA_CB);
			subQuery.AddToFilter(JoinCondition.And, CusSCAHouseSchema.CA_MessageStatus, @operator, valueToCompare);

			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusSCAOceanBill));
			dBQuery.AddSubQuery(subQuery, JoinCondition.And);
			query.AddToFilter(dBQuery);
		}

		protected virtual void AddUnderbondStatusFilter(ZQuery query, SQLComparisonOperator @operator, object value)
		{
			ZDBOnlySubQuery entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryStatus, @operator, value);
			entryNumQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumber.EntryType.UnderbondStatus);

			ZDBOnlySubQuery underbondQuery = new ZDBOnlySubQuery(typeof(CusUnderbond), CusUnderbondSchema.C4_ParentID);
			underbondQuery.AddSubQuery(entryNumQuery, JoinCondition.And);

			ZDBOnlySubQuery pivotQuery = new ZDBOnlySubQuery(typeof(CusSCAPivot), CusSCAPivotSchema.CV_CN);
			pivotQuery.AddSubQuery(underbondQuery, JoinCondition.And);

			ZDBOnlySubQuery containerQuery = new ZDBOnlySubQuery(typeof(CusSCAContainer), CusSCAContainerSchema.CN_CB);
			containerQuery.AddSubQuery(underbondQuery, JoinCondition.Or);
			containerQuery.AddSubQuery(pivotQuery, JoinCondition.Or);
			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusSCAOceanBill));
			dBQuery.AddSubQuery(containerQuery, JoinCondition.And);

			query.AddToFilter(dBQuery);
		}

		#endregion

		#region Establishment Filter

		void AddEstablishmentFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(SeaCargoFilterConstants.EstablishmentTypes.OriginAddress, GetOriginAddressQuery).MaxLength = OrgAddressSchema.OA_Address1.MaxLength;
			filters.AddTextFilter(SeaCargoFilterConstants.EstablishmentTypes.OriginCode, GetOriginCodeQuery).MaxLength = CusUnderbondSchema.C4_OriginPremiseID.MaxLength;
			filters.AddTextFilter(SeaCargoFilterConstants.EstablishmentTypes.DestinationAddress, GetDestinationAddressQuery).MaxLength = OrgAddressSchema.OA_Address1.MaxLength;
			filters.AddTextFilter(SeaCargoFilterConstants.EstablishmentTypes.DestinationCode, GetDestinationCodeQuery).MaxLength = CusUnderbondSchema.C4_DestinationPremiseID.MaxLength;
		}

		#endregion

		#region Establishment Filter Delegates

		ZQuery GetDestinationAddressQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddDestinationAddressFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetDestinationCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddDestinationCodeFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetOriginAddressQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			AddOriginAddressFilter(query, comparisonOperator, value);
			return query;
		}

		ZQuery GetOriginCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
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

			ZDBOnlySubQuery containerQuery = new ZDBOnlySubQuery(typeof(CusSCAContainer), CusSCAContainerSchema.CN_CB);
			containerQuery.AddSubQuery(underbondQuery1, JoinCondition.And);

			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusSCAOceanBill));
			dBQuery.AddSubQuery(containerQuery, JoinCondition.And);

			query.AddToFilter(dBQuery);
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
			ZDBOnlySubQuery containerQuery = new ZDBOnlySubQuery(typeof(CusSCAContainer), CusSCAContainerSchema.CN_CB);
			containerQuery.AddSubQuery(underbondQuery, JoinCondition.And);
			ZDBOnlyQuery dBQuery = new ZDBOnlyQuery(typeof(CusSCAOceanBill));
			dBQuery.AddSubQuery(containerQuery, JoinCondition.And);
			query.AddToFilter(dBQuery);
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
			filters.AddTextAndNkFilter(SeaCargoFilterConstants.NumberFilterTypes.VesselVoyage, GetVoyageVesselQuery, ModuleIDs.RefVessel, Vessels).Category = FilterCategories.NumbersAndReferences;
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
			query.AddToFilter(CusSCAOceanBillSchema.CB_VesselName, @operator, value);
		}

		protected virtual void AddVoyageFilter(ZQuery query, SQLComparisonOperator @operator, ZString value)
		{
			query.AddToFilter(CusSCAOceanBillSchema.CB_Voyage, @operator, value.SubstringSafe(0, CusSCAOceanBillSchema.CB_Voyage.MaxLength));
		}

		#endregion
	}
}

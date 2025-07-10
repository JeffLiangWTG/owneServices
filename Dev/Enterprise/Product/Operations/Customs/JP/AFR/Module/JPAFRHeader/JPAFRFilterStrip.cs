using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Module
{
	public class JPAFRFilterStrip : FilterStripBusinessObject
	{
		public static class FilterConstants
		{
			public const string JobReference = "Job Reference";
			public const string CarrierCode = "Carrier Code";
			public const string VesselName = "Vessel Name";
			public const string VesselCallSign = "Vessel Call Sign";
			public const string VesselCountry = "Vessel Nationality";
			public const string VoyageNumber = "Voyage Number";
			public const string LoadDischarge = "Load / Discharge";
			public const string EstimatedTimeDeparture = "Estimated Time Departure";
			public const string EstimatedTimeArrival = "Estimated Time Arrival";
			public const string HousebillMessageStatus = "Bill Message Status";
			public const string HousebillRegistrationStatus = "Bill Release Status";
			public const string MasterbillMessageStatus = "Message Status";
			public const string MasterbillRegistrationStatus = "Completion Status";
			public const string Branch = "Branch";
			public const string MasterBillOfLading = "Master Bill Of Lading";
			public const string IsShippingLineEntry = "Is Shipping Line Entry";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var jobReferenceFilter = result.AddTextFilter(FilterConstants.JobReference, JPAFRHeaderSchema.JPH_JobReference);
			jobReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			jobReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("ea211504-1fe4-4c7d-8312-a99a24557549", "Job Reference");

			var carrierCodeFilter = result.AddTextFilter(FilterConstants.CarrierCode, JPAFRHeaderSchema.JPH_CarrierCode);
			carrierCodeFilter.Category = FilterCategories.NumbersAndReferences;
			carrierCodeFilter.MultilingualDescription = ResString.GetMultilingualString("ee92e37a-f781-46d0-8535-c7409cd744be", "Carrier Code");

			var vesselNameFilter = result.AddTextFilter(FilterConstants.VesselName, JPAFRHeaderSchema.JPH_VesselName);
			vesselNameFilter.Category = FilterCategories.NumbersAndReferences;
			vesselNameFilter.MultilingualDescription = ResString.GetMultilingualString("39ccc9ec-5d8a-496f-9238-10b2568aec46", "Vessel Name");

			var voyageNumberFilter = result.AddTextFilter(FilterConstants.VoyageNumber, JPAFRHeaderSchema.JPH_Voyage);
			voyageNumberFilter.Category = FilterCategories.NumbersAndReferences;
			voyageNumberFilter.MultilingualDescription = ResString.GetMultilingualString("31c27fca-95a2-494e-84ad-1811c5b45d97", "Voyage Number");

			var vesselCallSignFilter = result.AddTextFilter(FilterConstants.VesselCallSign, JPAFRHeaderSchema.JPH_RadioCallSign);
			vesselCallSignFilter.Category = FilterCategories.NumbersAndReferences;
			vesselCallSignFilter.MultilingualDescription = ResString.GetMultilingualString("c1484d21-fabc-4bab-8c34-371e0c313495", "Vessel Call Sign");

			var estimatedTimeDepartureFilter = result.AddDateFilter(FilterConstants.EstimatedTimeDeparture, JPAFRHeaderSchema.JPH_ETD);
			estimatedTimeDepartureFilter.Category = FilterCategories.Dates;
			estimatedTimeDepartureFilter.MultilingualDescription = ResString.GetMultilingualString("8e66f819-50e4-4a7e-8193-725c35d4fd0d", "Estimated Time Departure");

			var estimatedTimeArrivalFilter = result.AddDateFilter(FilterConstants.EstimatedTimeArrival, JPAFRHeaderSchema.JPH_ETA);
			estimatedTimeArrivalFilter.Category = FilterCategories.Dates;
			estimatedTimeArrivalFilter.MultilingualDescription = ResString.GetMultilingualString("0acd61a6-544d-443c-8578-ebe10b56b657", "Estimated Time Arrival");

			var branchFilter = result.AddGuidFilter(FilterConstants.Branch, ModuleIDs.GlbBranch, JPAFRHeaderSchema.JPH_GB_Branch, HeaderLookups.Branches);
			branchFilter.Category = FilterCategories.NumbersAndReferences;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("38be1c49-0d06-4dd9-ba44-e3ee79a49ae6", "Branch");

			var masterBillNumberFilter = result.AddTextFilter(FilterConstants.MasterBillOfLading, JPAFRHeaderSchema.JPH_MasterBillNumber);
			masterBillNumberFilter.Category = FilterCategories.NumbersAndReferences;
			masterBillNumberFilter.MultilingualDescription = ResString.GetMultilingualString("728b3ac6-91cc-4761-8afa-a43109baf2cc", "Master Bill Of Lading");

			var messageStatusFilter = result.AddTextFilter(FilterConstants.MasterbillMessageStatus, JPAFRHeaderSchema.JPH_MessageStatus, MessageStatusList.GetHeaderLevelStatusList(Factory));
			messageStatusFilter.Category = FilterCategories.StatusAndFlags;
			messageStatusFilter.MultilingualDescription = ResString.GetMultilingualString("e6bd7be5-15e6-4fe3-87f2-50f64559a3b2", "Message Status");

			var masterBillRegistrationStatusFilter = result.AddTextFilter(FilterConstants.MasterbillRegistrationStatus, GetMasterbillRegistrationStatusQuery, Factory.GetCachedValue<YesNoList>());
			masterBillRegistrationStatusFilter.Category = FilterCategories.StatusAndFlags;
			masterBillRegistrationStatusFilter.MultilingualDescription = ResString.GetMultilingualString("00bc5062-c0f0-49d5-80f9-28779a6ccc64", "Completion Status");

			AddHouseBillMessageStatusFilter(result);
			AddHouseBillRegistrationStatusFilter(result);

			var isShippingLineEntryFilter = result.AddTextFilter(FilterConstants.IsShippingLineEntry, GetIsShippingLineEntryQuery, Factory.GetCachedValue<YesNoList>());
			isShippingLineEntryFilter.Category = FilterCategories.StatusAndFlags;
			isShippingLineEntryFilter.MultilingualDescription = ResString.GetMultilingualString("c1f352bb-31ab-4130-95e0-d74212167d91", "Is Shipping Line Entry");
			AddLocationFilter(result);
			return result;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var workflowHelper = new WorkflowFilterStripsHelperJPAFR(typeof(JPAFRHeader), WorkflowDescriptors.JPAFRWorkflowDescriptorCode, Factory);
			workflowHelper.SetShouldAddWorkflowCustomFieldsFilters(true);
			helpers.Add(workflowHelper);
			return helpers;
		}

		void AddHouseBillMessageStatusFilter(ModuleFilterCollection filters)
		{
			var housebillMessageStatusFilter = filters.AddTextFilter(FilterConstants.HousebillMessageStatus, GetHouseBillMessageStatusQuery, MessageStatusList.GetBillLevelStatusList(Factory));
			housebillMessageStatusFilter.Category = FilterCategories.StatusAndFlags;
			housebillMessageStatusFilter.MultilingualDescription = ResString.GetMultilingualString("c04c4a9a-99aa-4eb7-adba-1e87d4a22067", "Bill Message Status");
			housebillMessageStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Exact);
			housebillMessageStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			housebillMessageStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			housebillMessageStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			housebillMessageStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			housebillMessageStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			housebillMessageStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
		}

		void AddHouseBillRegistrationStatusFilter(ModuleFilterCollection filters)
		{
			var houseBillRegistrationStatusFilter = filters.AddTextFilter(FilterConstants.HousebillRegistrationStatus, GetHouseBillRegistrationStatusQuery, Factory.GetCachedValue<AFRBillCustomsStatusList>());
			houseBillRegistrationStatusFilter.Category = FilterCategories.StatusAndFlags;
			houseBillRegistrationStatusFilter.MultilingualDescription = ResString.GetMultilingualString("95ef9133-8b57-4527-9154-c66f5204ebe7", "Bill Release Status");
			houseBillRegistrationStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			houseBillRegistrationStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			houseBillRegistrationStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			houseBillRegistrationStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			houseBillRegistrationStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			houseBillRegistrationStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			houseBillRegistrationStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
		}

		void AddLocationFilter(ModuleFilterCollection filters)
		{
			var locationFilter = filters.AddLocationFilter(FilterConstants.LoadDischarge, GetLoadDischargePortQuery, Location_List, Location_List);
			locationFilter.SetItemDescriptions(Res.GetData("JPAFRFilterStrip|Load", "Load"), Res.GetData("JPAFRFilterStrip", "Discharge"));
			locationFilter.Category = FilterCategories.Locations;
			locationFilter.MultilingualDescription = ResString.GetMultilingualString("417a29d1-d66d-4799-a82c-b3837730ca5d", "Load / Discharge");

			var vesselCountryFilter = filters.AddNkFilter(FilterConstants.VesselCountry, JPAFRHeaderSchema.JPH_RN_NKCountryOfReg, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			vesselCountryFilter.Category = FilterCategories.Locations;
			vesselCountryFilter.MultilingualDescription = ResString.GetMultilingualString("84216be5-7f09-4b52-bc86-b20cb39698b9", "Vessel Nationality");
		}

		ZQuery GetIsShippingLineEntryQuery(ZString value)
		{
			return new ZQuery(JPAFRHeaderSchema.JPH_IsShippingLineEntry, value == YesNoList.Codes.Yes ? 1 : 0);
		}
		ZQuery GetLoadDischargePortQuery(ZString loadPort, ZString discPort)
		{
			ZQuery query = new ZQuery();

			if (!loadPort.IsEmpty || !discPort.IsEmpty)
			{
				if (!loadPort.IsEmpty)
				{
					bool isCountryCode = (loadPort.Length == 2);
					SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;
					query.AddToFilter(JPAFRHeaderSchema.JPH_RL_NKLoading, comparisonOperator, loadPort);
				}

				if (!discPort.IsEmpty)
				{
					bool isCountryCode = (discPort.Length == 2);
					SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;
					query.AddToFilter(JPAFRHeaderSchema.JPH_RL_NKDischarge, comparisonOperator, discPort);
				}
			}
			return query;
		}

		ZQuery GetMasterbillRegistrationStatusQuery(ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JPAFRHeader));
			var completeQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, value != YesNoList.Codes.Yes);
			completeQuery.AddToFilter(JPAFRHeader.BillRegistrationCompletedQuery);
			result.AddSubQuery(completeQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetHouseBillMessageStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var sqlFilter = string.Format(@"
{0} in (
SELECT {1} FROM {2} 
WHERE {3} = @messageStatus
)"
				, JPAFRHeaderSchema.Constants.PK
				, JPAFRBillsSchema.Constants.JPB_JPH_Header
				, JPAFRBillsSchema.Constants.TableName
				, JPAFRBillsSchema.Constants.JPB_MessageStatus
);
			var sqlFilterParameters = new ZSqlParameterCollection(
				ZSqlParameter.New("@messageStatus", value, JPAFRBillsSchema.JPB_MessageStatus)
				);
			var result = new ZDBOnlyQuery(typeof(JPAFRHeader));
			result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);
			return result;
		}

		ZQuery GetHouseBillRegistrationStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var baseQuery = @"
{0} IN (
SELECT {1} FROM {2} 
WHERE {3} IN ( @releaseStatus, @alternativeReleaseStatus)
) ";
			if (comparisonOperator == SQLComparisonOperator.Equal)
			{
				baseQuery += @"AND {0} NOT IN (
SELECT {1} FROM {2} 
WHERE {3} NOT IN ( @releaseStatus, @alternativeReleaseStatus)
)";
			}
			var sqlFilter = string.Format(baseQuery
				, JPAFRHeaderSchema.Constants.PK
				, JPAFRBillsSchema.Constants.JPB_JPH_Header
				, JPAFRBillsSchema.Constants.TableName
				, JPAFRBillsSchema.Constants.JPB_ReleaseStatus
				);
			var sqlFilterParameters = new ZSqlParameterCollection();
			sqlFilterParameters.Add(ZSqlParameter.New("@releaseStatus", value, JPAFRBillsSchema.JPB_ReleaseStatus));
			sqlFilterParameters.Add(ZSqlParameter.New("@alternativeReleaseStatus", value == AFRBillCustomsStatusList.Codes.NotRegistered ? ZString.Empty : value, JPAFRBillsSchema.JPB_ReleaseStatus));
			var result = new ZDBOnlyQuery(typeof(JPAFRHeader));
			result.AddFilterAndZSQLParameterCollection(sqlFilter, sqlFilterParameters);
			return result;
		}

		LocationCollection Location_List
		{
			get { return fLocation_List ?? (fLocation_List = new LocationCollection(Factory)); }
		}
		LocationCollection fLocation_List;

		JPAFRHeaderLookups HeaderLookups
		{
			get
			{
				if (headerLookups == null)
				{
					headerLookups = Factory.GetNull<JPAFRHeader>().Lookups;
				}
				return headerLookups;
			}
		}
		JPAFRHeaderLookups headerLookups;
	}
}

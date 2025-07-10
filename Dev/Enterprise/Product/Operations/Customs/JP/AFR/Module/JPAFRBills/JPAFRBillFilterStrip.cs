using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Module
{
	public class JPAFRBillFilterStrip : FilterStripBusinessObject
	{
		public static class FilterConstants
		{
			public const string JobReference = "Job Reference";
			public const string CarrierCode = "Carrier Code";
			public const string VesselName = "Vessel Name";
			public const string VesselCallSign = "Vessel Call Sign";
			public const string VoyageNumber = "Voyage Number";
			public const string LoadDischarge = "Load / Discharge";
			public const string EstimatedTimeDeparture = "Estimated Time Departure";
			public const string EstimatedTimeArrival = "Estimated Time Arrival";
			public const string Branch = "Branch";
			public const string MasterBillOfLading = "Master Bill Of Lading";
			public const string JobCreatedBy = "Job Created By";
			public const string JobCreatedTime = "Job Created Time";
			public const string HousebillRegistrationCompleted = "Housebill Registration Completed";

			public const string BillOfLading = "Bill Of Lading";
			public const string ReleaseStatus = "Release Status";
			public const string MessageStatus = "Message Status";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddHeaderFilters(result);
			var billOfLadingFilter = result.AddTextFilter(FilterConstants.BillOfLading, JPAFRBillsSchema.JPB_BillNumber);
			billOfLadingFilter.Category = FilterCategories.NumbersAndReferences;
			billOfLadingFilter.MultilingualDescription = ResString.GetMultilingualString("b270b980-07c2-4cfa-bdb0-a7d0dec77d5a", "Bill Of Lading");

			var releasesStatusFilter = result.AddTextFilter(FilterConstants.ReleaseStatus, releaseStatusQuery, Factory.GetCachedValue<AFRBillCustomsStatusList>());
			releasesStatusFilter.Category = FilterCategories.StatusAndFlags;
			releasesStatusFilter.MultilingualDescription = ResString.GetMultilingualString("f64c7527-255c-49fe-bd2e-07b7cd44b33b", "Release Status");

			var messageStatusFilter = result.AddTextFilter(FilterConstants.MessageStatus, JPAFRBillsSchema.JPB_MessageStatus, MessageStatusList.GetBillLevelStatusList(Factory));
			messageStatusFilter.Category = FilterCategories.StatusAndFlags;
			messageStatusFilter.MultilingualDescription = ResString.GetMultilingualString("e6bd7be5-15e6-4fe3-87f2-50f64559a3b2", "Message Status");

			return result;
		}

		static FilterCategory JobCategory
		{
			get { return FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("JPAFRBillFilterStrip.HeaderCategory", "Job")); }
		}

		ZQuery releaseStatusQuery(ZString value)
		{
			var releaseStatusQuery = new ZQuery(JPAFRBillsSchema.JPB_ReleaseStatus, value);
			if (value == AFRBillCustomsStatusList.Codes.NotRegistered)
			{
				releaseStatusQuery.AddToFilter(JoinCondition.Or, JPAFRBillsSchema.JPB_ReleaseStatus, ZString.Empty);
			}
			return releaseStatusQuery;
		}
		void AddHeaderFilters(ModuleFilterCollection result)
		{
			var textFilter = result.AddTextFilter(FilterConstants.JobReference, new GetTextQueryWithOperator((comparisonOperator, value) => GetHeaderQuery(JPAFRHeaderSchema.JPH_JobReference, comparisonOperator, value)));
			textFilter.Category = JobCategory;
			textFilter.MaxLength = JPAFRHeader.Schema.JPH_JobReferenceMaxLength;
			textFilter.SubGroup = HeaderSubGroup;
			textFilter.MultilingualDescription = ResString.GetMultilingualString("ea211504-1fe4-4c7d-8312-a99a24557549", "Job Reference");

			textFilter = result.AddTextFilter(FilterConstants.CarrierCode, new GetTextQueryWithOperator((comparisonOperator, value) => GetHeaderQuery(JPAFRHeaderSchema.JPH_CarrierCode, comparisonOperator, value)));
			textFilter.Category = FilterCategories.NumbersAndReferences;
			textFilter.MaxLength = JPAFRHeader.Schema.JPH_CarrierCodeMaxLength;
			textFilter.SubGroup = HeaderSubGroup;
			textFilter.MultilingualDescription = ResString.GetMultilingualString("ee92e37a-f781-46d0-8535-c7409cd744be", "Carrier Code");

			textFilter = result.AddTextFilter(FilterConstants.VesselName, new GetTextQueryWithOperator((comparisonOperator, value) => GetHeaderQuery(JPAFRHeaderSchema.JPH_VesselName, comparisonOperator, value)));
			textFilter.Category = FilterCategories.NumbersAndReferences;
			textFilter.MaxLength = JPAFRHeader.Schema.JPH_VesselNameMaxLength;
			textFilter.SubGroup = HeaderSubGroup;
			textFilter.MultilingualDescription = ResString.GetMultilingualString("39ccc9ec-5d8a-496f-9238-10b2568aec46", "Vessel Name");

			textFilter = result.AddTextFilter(FilterConstants.VoyageNumber, new GetTextQueryWithOperator((comparisonOperator, value) => GetHeaderQuery(JPAFRHeaderSchema.JPH_Voyage, comparisonOperator, value)));
			textFilter.Category = FilterCategories.NumbersAndReferences;
			textFilter.MaxLength = JPAFRHeader.Schema.JPH_VoyageMaxLength;
			textFilter.SubGroup = HeaderSubGroup;
			textFilter.MultilingualDescription = ResString.GetMultilingualString("31c27fca-95a2-494e-84ad-1811c5b45d97", "Voyage Number");

			textFilter = result.AddTextFilter(FilterConstants.VesselCallSign, RefVesselSchema.RV_RadioCallSign);
			textFilter.Category = FilterCategories.NumbersAndReferences;
			textFilter.SubGroup = new VesselCallSignSubGroup(HeaderSubGroup);
			textFilter.MultilingualDescription = ResString.GetMultilingualString("c1484d21-fabc-4bab-8c34-371e0c313495", "Vessel Call Sign");

			var estimatedTimeDepartureFilter = result.AddDateFilter(FilterConstants.EstimatedTimeDeparture, new GetDateQuery((comparisonOperator, value1, value2) => GetHeaderQuery(JPAFRHeaderSchema.JPH_ETD, comparisonOperator, value1, value2)));
			estimatedTimeDepartureFilter.Category = FilterCategories.Dates;
			estimatedTimeDepartureFilter.MultilingualDescription = ResString.GetMultilingualString("8e66f819-50e4-4a7e-8193-725c35d4fd0d", "Estimated Time Departure");

			var estimatedTimeArrivalFilter = result.AddDateFilter(FilterConstants.EstimatedTimeArrival, new GetDateQuery((comparisonOperator, value1, value2) => GetHeaderQuery(JPAFRHeaderSchema.JPH_ETA, comparisonOperator, value1, value2)));
			estimatedTimeArrivalFilter.Category = FilterCategories.Dates;
			estimatedTimeArrivalFilter.MultilingualDescription = ResString.GetMultilingualString("0acd61a6-544d-443c-8578-ebe10b56b657", "Estimated Time Arrival");

			var guidFilter = result.AddGuidFilter(FilterConstants.Branch, ModuleIDs.GlbBranch, new GetGuidQueryWithOperator((comparisonOperator, value) => GetHeaderQuery(JPAFRHeaderSchema.JPH_GB_Branch, comparisonOperator, value)), HeaderLookups.Branches);
			guidFilter.Category = FilterCategories.NumbersAndReferences;
			guidFilter.SubGroup = HeaderSubGroup;
			guidFilter.MultilingualDescription = ResString.GetMultilingualString("38be1c49-0d06-4dd9-ba44-e3ee79a49ae6", "Branch");

			textFilter = result.AddTextFilter(FilterConstants.MasterBillOfLading, new GetTextQueryWithOperator((comparisonOperator, value) => GetHeaderQuery(JPAFRHeaderSchema.JPH_MasterBillNumber, comparisonOperator, value)));
			textFilter.Category = FilterCategories.NumbersAndReferences;
			textFilter.MaxLength = JPAFRHeader.Schema.JPH_MasterBillNumberMaxLength;
			textFilter.SubGroup = HeaderSubGroup;
			textFilter.MultilingualDescription = ResString.GetMultilingualString("728b3ac6-91cc-4761-8afa-a43109baf2cc", "Master Bill Of Lading");

			var nKFilter = result.AddNkFilter(FilterConstants.JobCreatedBy, new GetNkQueryWithOperator((comparisonOperator, value) => GetHeaderQuery(JPAFRHeaderSchema.JPH_SystemCreateUser, comparisonOperator, value)), ModuleIDs.GlbStaff, new GlbStaffCollection(Factory));
			nKFilter.Category = JobCategory;
			nKFilter.SubGroup = HeaderSubGroup;
			nKFilter.MultilingualDescription = ResString.GetMultilingualString("ae5a8459-1694-4232-a33a-eb194fcce7ee", "Job Created By");

			var jobCreatedDateFilter = result.AddDateFilter(FilterConstants.JobCreatedTime, new GetDateQuery((comparisonOperator, value1, value2) => GetHeaderQuery(JPAFRHeaderSchema.JPH_SystemCreateTimeUtc, comparisonOperator, value1, value2)));
			jobCreatedDateFilter.Category = JobCategory;
			jobCreatedDateFilter.MultilingualDescription = ResString.GetMultilingualString("859e4422-ae7b-4d34-9877-3452e570e4a3", "Job Created Time");

			var locationFilter = result.AddLocationFilter(FilterConstants.LoadDischarge, GetLoadDischargePortFilter, Location_List, Location_List);
			locationFilter.SetItemDescriptions(Res.GetData("JPAFRBillFilterStrip|Load", "Load"), Res.GetData("JPAFRBillFilterStrip", "Discharge"));
			locationFilter.Category = FilterCategories.Locations;
			locationFilter.SubGroup = new DischargePortSubGroup(HeaderSubGroup);
			locationFilter.MultilingualDescription = ResString.GetMultilingualString("417a29d1-d66d-4799-a82c-b3837730ca5d", "Load / Discharge");

			var houseBillRegistrationCompletedFilter = result.AddTextFilter(FilterConstants.HousebillRegistrationCompleted, GetHousebillRegistrationCompletedQuery, Factory.GetCachedValue<YesNoList>());
			houseBillRegistrationCompletedFilter.Category = FilterCategories.StatusAndFlags;
			houseBillRegistrationCompletedFilter.MultilingualDescription = ResString.GetMultilingualString("a162dcbf-c850-4af8-a1a8-877824576ba2", "House bill Registration Completed");
		}

		ZQuery GetHousebillRegistrationCompletedQuery(ZString value)
		{
			var headerQuery = new ZDBOnlySubQuery(typeof(JPAFRHeader), JPAFRHeaderSchema.PK);
			headerQuery.AddToFilter(JPAFRHeaderSchema.JPH_IsShippingLineEntry, ZBool.False);
			var completeQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, value != YesNoList.Codes.Yes);
			completeQuery.AddToFilter(JPAFRHeader.BillRegistrationCompletedQuery);
			headerQuery.AddSubQuery(completeQuery, JoinCondition.And);
			var result = new ZDBOnlyQuery(typeof(JPAFRBills));
			result.AddSubQuery(JPAFRBillsSchema.JPB_JPH_Header, headerQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetLoadDischargePortFilter(ZString loadPort, ZString discPort)
		{
			var headerQuery = new ZDBOnlyQuery(typeof(JPAFRHeader));
			if (!loadPort.IsEmpty || !discPort.IsEmpty)
			{
				if (!loadPort.IsEmpty)
				{
					bool isCountryCode = (loadPort.Length == 2);
					SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;
					headerQuery.AddToFilter(JPAFRHeaderSchema.JPH_RL_NKLoading, comparisonOperator, loadPort);
				}

				if (!discPort.IsEmpty)
				{
					bool isCountryCode = (discPort.Length == 2);
					SQLComparisonOperator comparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;
					headerQuery.AddToFilter(JPAFRHeaderSchema.JPH_RL_NKDischarge, comparisonOperator, discPort);
				}
			}
			return headerQuery;
		}

		ZQuery GetHeaderQuery(SchemaGuidColumn column, SQLComparisonOperator comparisonOperator, object value)
		{
			var result = new ZDBOnlyQuery(typeof(JPAFRBills));
			result.AddToFilter(JPAFRHeaderSchema.JPH_IsShippingLineEntry, ZBool.False);
			result.AddToFilter(column, comparisonOperator, value);
			return result;
		}

		ZQuery GetHeaderQuery(SchemaDateTimeColumn column, DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZDBOnlyQuery(typeof(JPAFRBills));
			var headerQuery = new ZDBOnlySubQuery(typeof(JPAFRHeader), JPAFRHeaderSchema.PK);
			headerQuery.AddToFilter(JPAFRHeaderSchema.JPH_IsShippingLineEntry, ZBool.False);
			switch (comparisonOperator)
			{
				case DateComparisonOperator.HasDateEntered:
					headerQuery.AddToFilter(column, value1);
					break;
				case DateComparisonOperator.HasNoDateEntered:
					headerQuery.AddToFilter(column, ZDateTime.Empty);
					break;
				default:
					AddDateTimeRange(headerQuery, comparisonOperator, JoinCondition.And, column, value1, value2);
					break;
			}
			result.AddSubQuery(JPAFRBillsSchema.JPB_JPH_Header, headerQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetHeaderQuery(SchemaStringColumn column, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JPAFRBills));
			result.AddToFilter(JPAFRHeaderSchema.JPH_IsShippingLineEntry, ZBool.False);
			result.AddToFilter(column, comparisonOperator, value);
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

		protected ModuleFilterSubGroup HeaderSubGroup
		{
			get { return headerSubGroup ?? (headerSubGroup = new HeaderFilterSubGroup()); }
		}
		ModuleFilterSubGroup headerSubGroup;
	}

	class DischargePortSubGroup : ModuleFilterSubGroup
	{
		public DischargePortSubGroup(ModuleFilterSubGroup parent)
			: base(parent) { }

		public override ZQuery GetSubQuery(ZQuery filter)
		{
			var result = new ZDBOnlyQuery(typeof(JPAFRBills));
			result.AddToFilter(filter);
			result.AddToFilter(JPAFRHeaderSchema.JPH_IsShippingLineEntry, ZBool.False);
			return result;
		}
	}

	class HeaderFilterSubGroup : ModuleFilterSubGroup
	{
		public override ZQuery GetSubQuery(ZQuery filter)
		{
			var result = new ZDBOnlyQuery(typeof(JPAFRBills));
			var headerQuery = new ZDBOnlySubQuery(typeof(JPAFRHeader), JPAFRHeaderSchema.PK);
			headerQuery.AddToFilter(filter);
			result.AddSubQuery(JPAFRBillsSchema.JPB_JPH_Header, headerQuery, JoinCondition.And);
			return result;
		}
	}

	class VesselCallSignSubGroup : ModuleFilterSubGroup
	{
		public VesselCallSignSubGroup(ModuleFilterSubGroup parent)
			: base(parent) { }

		public override ZQuery GetSubQuery(ZQuery filter)
		{
			var result = new ZDBOnlyQuery(typeof(JPAFRBills));
			var vesselQuery = new ZDBOnlySubQuery(typeof(RefVessel), RefVesselSchema.RV_Code);
			vesselQuery.AddToFilter(filter);
			result.AddToFilter(JPAFRHeaderSchema.JPH_IsShippingLineEntry, ZBool.False);
			result.AddSubQuery(JPAFRHeaderSchema.JPH_VesselName, vesselQuery, JoinCondition.And);
			return result;
		}
	}
}

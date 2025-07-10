using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Module
{
	public class MiscRequestMessagesFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string JobNumber = "Job Number";
			public const string MessageType = "Message Type";
			public const string Status = "Status";
			public const string RequestDate = "Request Date";
			public const string CustomsOffice = "Customs Office";
			public const string CustomsBroker = "Customs Broker";
			public const string Branch = "Branch";
			public const string EntryNumber = "Entry Number";
			public const string EntryType = "Entry Type";
			public const string RequestDetails = "Request Details";
			public const string ApplicationNumber = "Application Number";
			public const string ApplicationStartPeriod = "Application Start Period";
			public const string ReviewDate = "Review Date";
		}

		public MiscRequestMessagesFilterLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new MiscRequestMessagesFilterLookups(this);
				}
				return lookups;
			}
		}
		MiscRequestMessagesFilterLookups lookups;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var jobNumberFilter = result.AddNumberFilter(Schema.JobNumber, CusMiscRequestHeaderSchema.CMR_JobNumber);
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("MiscRequestMessagesFilterStripBusinessObject|JobtNumber", Schema.JobNumber);

			var messageTypeFilter = result.AddTextFilter(Schema.MessageType, CusMiscRequestHeaderSchema.CMR_MessageType, Lookups.RequestMessageTypeList);
			messageTypeFilter.MultilingualDescription = ResString.GetMultilingualString("MiscRequestMessagesFilterStripBusinessObject|MessageType", Schema.MessageType);
			messageTypeFilter.Category = FilterCategories.ModesAndTypes;
			messageTypeFilter.ComparisonOperator_List.Clear();
			messageTypeFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			messageTypeFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			messageTypeFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Exact;

			var statusFilter = result.AddTextFilter(Schema.Status, CusMiscRequestHeaderSchema.CMR_Status, Lookups.StatusList);
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("MiscRequestMessagesFilterStripBusinessObject|Status", Schema.Status);
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.ComparisonOperator_List.Clear();
			statusFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			statusFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			statusFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank));
			statusFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank));
			statusFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Exact;

			var requestDateFilter = result.AddDateFilter(Schema.RequestDate, CusMiscRequestHeaderSchema.CMR_RequestDate);
			requestDateFilter.MultilingualDescription = ResString.GetMultilingualString("MiscRequestMessagesFilterStripBusinessObject|RequestDate", Schema.RequestDate);

			var applicationStartPeriodFilter = result.AddDateFilter(Schema.ApplicationStartPeriod, GetApplicationStartPeriodQuery);
			applicationStartPeriodFilter.MultilingualDescription = ResString.GetMultilingualString("MiscRequestMessagesFilterStripBusinessObject|ApplicationStartPeriod", Schema.ApplicationStartPeriod);

			var reviewDateFilter = result.AddDateFilter(Schema.ReviewDate, GetReviewDateQuery);
			reviewDateFilter.MultilingualDescription = ResString.GetMultilingualString("MiscRequestMessagesFilterStripBusinessObject|ReviewDate", Schema.ReviewDate);

			var customsOfficeFilter = result.AddNkFilter(Schema.CustomsOffice, GetCustomsOfficeQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.CustomsOffices);
			customsOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("MiscRequestMessagesFilterStripBusinessObject|CustomsOffice", Schema.CustomsOffice);
			customsOfficeFilter.Category = FilterCategories.Locations;
			customsOfficeFilter.ComparisonOperator_List.Clear();
			customsOfficeFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			customsOfficeFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			customsOfficeFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Exact;

			var brokerFilter = result.AddNkFilter(Schema.CustomsBroker, CusMiscRequestHeaderSchema.CMR_GS_NKBroker, ModuleIDs.GlbStaff, Lookups.StaffList);
			brokerFilter.MultilingualDescription = ResString.GetMultilingualString("MiscRequestMessagesFilterStripBusinessObject|CustomsBroker", Schema.CustomsBroker);
			brokerFilter.Category = FilterCategories.Organisations;

			var currentBranchFilter = result.AddGuidFilter(Schema.Branch, ModuleIDs.GlbBranch, GetBranchQuery, Lookups.BranchList);
			currentBranchFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

			var entryNumberFilter = result.AddNumberFilter(Schema.EntryNumber, CusMiscRequestLineSchema.CML_EntryNumber);
			entryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("MiscRequestMessagesFilterStripBusinessObject|EntryNumber", Schema.EntryNumber);
			entryNumberFilter.SubGroup = RequestLineSubGroup;

			var entryTypeFilter = result.AddTextFilter(Schema.EntryType, CusMiscRequestLineSchema.CML_EntryType, Lookups.RequestEntryTypeList);
			entryTypeFilter.MultilingualDescription = ResString.GetMultilingualString("MiscRequestMessagesFilterStripBusinessObject|EntryType", Schema.EntryType);
			entryTypeFilter.Category = FilterCategories.ModesAndTypes;
			entryTypeFilter.SubGroup = RequestLineSubGroup;

			var applicationNumberFilter = result.AddNumberFilter(Schema.ApplicationNumber, GetApplicationNumberQuery);
			applicationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("MiscRequestMessagesFilterStripBusinessObject|ApplicationNumber", Schema.ApplicationNumber);
			applicationNumberFilter.PropertyValidation = ApplicationNumberFilterValidation;
			applicationNumberFilter.ComparisonOperator_List.Clear();
			applicationNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			applicationNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith));
			applicationNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains));
			applicationNumberFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Contains;
			return result;
		}
		void ApplicationNumberFilterValidation(ZPropertyInfo info)
		{
			var errorWarning = Res.GetString("0F972A8B-F4DE-4438-A054-D04A8B5C647C", "The search will be done without formatting characters like '-'.");
			var value = (ZString)info.Value;
			if (!value.IsLettersAndNumbersOnlyOrEmpty)
			{
				info.AddWarning(errorWarning);
			}
		}

		ZQuery GetApplicationNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var cusMiscRequestHeaderQuery = new ZDBOnlyQuery(typeof(CusMiscRequestHeader));

			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.PK);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, comparisonOperator, value.KeepAlphanumericCharacters());
			cusMiscRequestHeaderQuery.AddSubQuery(CusMiscRequestHeaderSchema.PK, CusEntryNumSchema.CE_ParentID, cusEntryNumQuery, JoinCondition.And);

			return cusMiscRequestHeaderQuery;
		}
		ZQuery GetApplicationStartPeriodQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var cusMiscRequestHeaderQuery = new ZDBOnlyQuery(typeof(CusMiscRequestHeader));
			cusMiscRequestHeaderQuery.AddToFilter(CusMiscRequestHeaderSchema.CMR_MessageType, new string[] { ElectronicDocumentTypeList.Codes._5AC, ElectronicDocumentTypeList.Codes._5GW });

			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.PK);

			AddDateTimeRange(cusEntryNumQuery, comparisonOperator, JoinCondition.And, CusEntryNumSchema.CE_IssueDate, value1, value2);
			cusMiscRequestHeaderQuery.AddSubQuery(CusMiscRequestHeaderSchema.PK, CusEntryNumSchema.CE_ParentID, cusEntryNumQuery, JoinCondition.And);

			return cusMiscRequestHeaderQuery;
		}

		ZQuery GetReviewDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var cusMiscRequestHeaderQuery = new ZDBOnlyQuery(typeof(CusMiscRequestHeader));
			cusMiscRequestHeaderQuery.AddToFilter(CusMiscRequestHeaderSchema.CMR_MessageType, ElectronicDocumentTypeList.Codes._5SG);

			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.PK);

			AddDateTimeRange(cusEntryNumQuery, comparisonOperator, JoinCondition.And, CusEntryNumSchema.CE_IssueDate, value1, value2);
			cusMiscRequestHeaderQuery.AddSubQuery(CusMiscRequestHeaderSchema.PK, CusEntryNumSchema.CE_ParentID, cusEntryNumQuery, JoinCondition.And);

			return cusMiscRequestHeaderQuery;
		}

		ZQuery GetBranchQuery(ZGuid value)
		{
			return new ZQuery(CusMiscRequestHeaderSchema.CMR_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
		}

		ZQuery GetCustomsOfficeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(CusMiscRequestHeaderSchema.CMR_CustomsOffice, comparisonOperator == SQLComparisonOperator.Equal ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.DoesNotStartWith, value);
		}

		#region Line Filters

		ModuleFilterSubGroup RequestLineSubGroup => requestLineSubGroup ?? (requestLineSubGroup = new RequestLineFilterSubGroup());
		ModuleFilterSubGroup requestLineSubGroup;

		class RequestLineFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(CusMiscRequestLine), CusMiscRequestLineSchema.CML_CMR);
				subQuery.AddToFilter(filter);

				var dbOnlyResult = new ZDBOnlyQuery(typeof(CusMiscRequestHeader));
				dbOnlyResult.AddSubQuery(subQuery, JoinCondition.And);
				return dbOnlyResult;
			}
		}

		#endregion
	}
}

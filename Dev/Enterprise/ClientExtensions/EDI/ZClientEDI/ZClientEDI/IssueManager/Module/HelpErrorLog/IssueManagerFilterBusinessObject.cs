using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IssueManager.Module
{
	public class IssueManagerFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddDateRangeFilter(filters);
			AddFlagsFilter(filters);
			AddOccurrencesFilters(filters);
			AddNumberRangeFilters(filters);
			AddRelatedItemFilters(filters);
			return filters;
		}

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleFountainFilter("Issue Number", HelpErrorLogSchema.HE_IssueNumber, ZString.Empty);
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Exception Type", HelpErrorLogSchema.HE_ExceptionType);
			filters.AddTextFilter("Exception Message", HelpErrorLogSchema.HE_ExceptionMessage);
			filters.AddTextFilter("Exception Source", HelpErrorLogSchema.HE_ExceptionSource);
			filters.AddTextFilter("First Version Number", HelpErrorLogSchema.HE_FirstVersionNumber);
			filters.AddTextFilter("Last Version Number", HelpErrorLogSchema.HE_LastVersionNumber);
			filters.AddTextFilter("Exception Key", GetExceptionKeyFilter);
		}

		ZQuery GetExceptionKeyFilter(SQLComparisonOperator comparison, ZString key)
		{
			ZQuery query = new ZQuery();
			ZDBOnlyQuery keyQuery = new ZDBOnlyQuery(typeof(EdiHelpErrorLog));

			ZDBOnlySubQuery keySubQuery = new ZDBOnlySubQuery(typeof(HelpErrorLogKey), HelpErrorLogKeySchema.HK_HE);
			keySubQuery.AddToFilter(JoinCondition.And, HelpErrorLogKeySchema.HK_Key, comparison, key);

			keyQuery.AddSubQuery(keySubQuery, JoinCondition.And);
			query.AddToFilter(keyQuery);
			return query;
		}

		public static ZQuery GetDatabaseServerCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(EdiHelpErrorLog));
			ZDBOnlySubQuery hoQuery = new ZDBOnlySubQuery(typeof(HelpErrorLogOccurrence), HelpErrorLogOccurrenceSchema.HO_HE);
			ZDBOnlySubQuery dbQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
			dbQuery.AddToFilter(LicenceDatabaseSchema.LD_ServerCode, comparisonOperator, value);
			result.AddSubQuery(HelpErrorLogOccurrenceSchema.HO_LD, dbQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region Date Time

		void AddDateRangeFilter(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("First Processed", HelpErrorLogSchema.HE_FirstProcessed, true);
			filters.AddDateFilter("First Reported", HelpErrorLogSchema.HE_FirstReported, true);
			filters.AddDateFilter("Last Reported", HelpErrorLogSchema.HE_LastReported, true);
			filters.AddDateFilter("Fixed Date", HelpErrorLogSchema.HE_FixedDate, true);
			filters.AddDateFilter("Latest EXE", HelpErrorLogSchema.HE_LastEXEVersionDate, true);
			filters.AddDateFilter("First EXE", HelpErrorLogSchema.HE_FirstEXEVersionDate, true);
		}

		#endregion

		#region NumberRange

		void AddNumberRangeFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberRangeFilter("Fixed Count", HelpErrorLogSchema.HE_FixedCount);
			filters.AddNumberRangeFilter("Fail Count", HelpErrorLogSchema.HE_FailCount);
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter("Related To Work Item", ModuleIDs.WorkItem, GetWorkItemQuery, new NewWorkItemCollection(Factory));
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
		}

		ZQuery GetWorkItemQuery(SQLComparisonOperator comparisonOperator, object pkValue)
		{
			var pk = ZGuid.Empty;
			if (ZGuid.TryParse(pkValue, out pk))
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(EdiHelpErrorLog));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation2ID);
				subQuery.AddToFilter(GenPivotSchema.XX_RelationType, Enterprise.Core.Constants.GenPivotTypes.ProcessManagement);
				subQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, WorkItemSchema.Constants.Prefix);
				subQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, HelpErrorLogSchema.Constants.Prefix);
				subQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, pk);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return query;
			}

			return ZQuery.NoResultQuery;
		}

		#endregion

		#region Flags

		void AddFlagsFilter(ModuleFilterCollection filters)
		{
			ModuleFlagsFilter visibleFilter = filters.AddFlagsFilter("Client Visible", new string[] { "Client Visible" }, new GetFlagsQuery[] { GetVisibleFilter });

			filters.AddCustomFilter(new ErrorLogStatusFilter("Status", GetStatusFilter, StatusList));
			errorLogStatusFilter = (ErrorLogStatusFilter)filters["Status"];
			errorLogStatusFilter.Category = FilterCategories.StatusAndFlags;
			errorLogStatusFilter.ErrorOnCodeNotPresent = true;

			filters.AddTextFilter("Work Items", GetWithWorkItemsFilter, HasWorkItemsList).Category = FilterCategories.StatusAndFlags;
		}

		internal ErrorLogStatusFilter errorLogStatusFilter;

		ZQuery GetNeedsWorkitemFilter()
		{
			var query = new ZDBOnlyQuery(typeof(EdiHelpErrorLog));
			var sql = @"
HE_FixedDate is NULL
AND
(
	HE_LastVersionNumber >= '14.%'
	OR
	(
		HE_IsClientVisible = 1
		AND HE_LastEXEVersionDate > '2014-04-01 00:00:00'
	)
)
AND HE_PK NOT IN
(
	SELECT XX_Relation2ID
	FROM
		dbo.GenPivot
		JOIN dbo.ProcessTasks ON P9_ParentID = XX_Relation1ID
	WHERE
		XX_RelationType = 'WRK'
		AND XX_Relation2TableCode = 'HE'
		AND P9_Status NOT in ('CLS', 'CAN')
		AND P9_Type <> 'MIL'
		AND P9_Type <> 'TRG'
		AND P9_Type <> 'EXC'
)
";
			query.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection());
			return query;
		}

		ZQuery GetVisibleFilter(ZBool value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(HelpErrorLogSchema.HE_IsClientVisible, value);
			return query;
		}

		public CodeDescriptionPairList VisibleList
		{
			get
			{
				if (visibleList == null)
				{
					visibleList = new CodeDescriptionPairList();
					visibleList.AddPair(VisibleCodes.Visible);
					visibleList.AddPair(VisibleCodes.Invisible);
				}
				return visibleList;
			}
		}

		CodeDescriptionPairList visibleList;

		class VisibleCodes
		{
			public const string Visible = "Vislble";
			public const string Invisible = "Invisible";
		}

		ZQuery GetWithWorkItemsFilter(ZString value)
		{
			if (value == WithWorkItemsCodes.WithoutWorkItemsCode ||
				value == WithWorkItemsCodes.WithWorkItemsCode)
			{
				bool notIn = value == WithWorkItemsCodes.WithoutWorkItemsCode;
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(EdiHelpErrorLog));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation2ID, notIn);
				subQuery.AddToFilter(GenPivotSchema.XX_RelationType, Enterprise.Core.Constants.GenPivotTypes.ProcessManagement);
				subQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, WorkItemSchema.Constants.Prefix);
				subQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, HelpErrorLogSchema.Constants.Prefix);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return query;
			}
			else if (value == WithWorkItemsCodes.NeedsWorkItem)
			{
				return GetNeedsWorkitemFilter();
			}
			else
			{
				return new ZQuery();
			}
		}

		CodeDescriptionPairList HasWorkItemsList
		{
			get
			{
				if (hasWorkItemsList == null)
				{
					hasWorkItemsList = new CodeDescriptionPairList();
					hasWorkItemsList.AddPair(WithWorkItemsCodes.All, WithWorkItemsCodes.All);
					hasWorkItemsList.AddPair(WithWorkItemsCodes.WithWorkItemsCode, WithWorkItemsCodes.WithWorkItemsCode);
					hasWorkItemsList.AddPair(WithWorkItemsCodes.WithoutWorkItemsCode, WithWorkItemsCodes.WithoutWorkItemsCode);
					hasWorkItemsList.AddPair(WithWorkItemsCodes.NeedsWorkItem, "Needs workitem (or new task)");
				}
				return hasWorkItemsList;
			}
		}

		CodeDescriptionPairList hasWorkItemsList;

		internal class WithWorkItemsCodes
		{
			public const string All = "All";
			public const string WithWorkItemsCode = "With work items";
			public const string WithoutWorkItemsCode = "Without work items";
			public const string NeedsWorkItem = "Needs work item";
		}

		ZQuery GetStatusFilter(ZString status)
		{
			ZQuery query = new ZQuery();
			if (!status.IsEmpty)
			{
				switch (status)
				{
					case ErrorLogStatusFilter.CodeConstants.NotFixed:
						AddNotFixedFilter(query);
						break;

					case ErrorLogStatusFilter.CodeConstants.Fixed:
						AddFixedFilter(query);
						break;

					case ErrorLogStatusFilter.CodeConstants.Ignored:
						AddIgnoredFilter(query);
						break;

					default:
						break;
				}
			}

			return query;
		}

		void AddIgnoredFilter(ZQuery query)
		{
			query.AddToFilter(HelpErrorLogSchema.HE_FixedDate, SQLComparisonOperator.NotEqual, null);
		}

		void AddNotFixedFilter(ZQuery query)
		{
			query.AddToFilter(HelpErrorLogSchema.HE_FixedDate, null);
			if (!errorLogStatusFilter.RelatedTimeFrame.IsEmpty)
			{
				int hours = (int)ZArchitecture.Core.Utilities.Round(errorLogStatusFilter.RelatedTimeFrame * -24, 0);
				query.AddToFilter(HelpErrorLogSchema.HE_FixedDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow.AddHours(hours));
			}
		}

		void AddFixedFilter(ZQuery query)
		{
			query.AddToFilter(HelpErrorLogSchema.HE_FixedDate, SQLComparisonOperator.NotEqual, null);
			if (!errorLogStatusFilter.RelatedTimeFrame.IsEmpty)
			{
				int hours = (int)ZArchitecture.Core.Utilities.Round(errorLogStatusFilter.RelatedTimeFrame * -24, 0);
				query.AddToFilter(HelpErrorLogSchema.HE_FixedDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.UtcNow.AddHours(hours));
			}
		}

		#endregion

		#region Occurrences Filters

		void AddOccurrencesFilters(ModuleFilterCollection filters)
		{
			OccurrenceFilterSubGroup subGroup = new OccurrenceFilterSubGroup();
			filters.AddTextFilter("Client Name", HelpErrorLogOccurrenceSchema.HO_Company).SubGroup = subGroup;
			filters.AddTextFilter("Exception ID", HelpErrorLogOccurrenceSchema.HO_ExceptionID).SubGroup = subGroup;
			filters.AddDateFilter("Occurrences Date", HelpErrorLogOccurrenceSchema.HO_ExceptionDateTime, true).SubGroup = subGroup;
			filters.AddTextFilter("Server Name", HelpErrorLogOccurrenceSchema.HO_ServerName).SubGroup = subGroup;
			var sessionIdFilter = new GuidTextFilter("Session ID", HelpErrorLogOccurrenceSchema.HO_SessionID) { SubGroup = subGroup };
			filters.AddFilter(sessionIdFilter);
			filters.AddNumberRangeFilter("Sequence", HelpErrorLogOccurrenceSchema.HO_Sequence).SubGroup = subGroup;
			AddLicenceFilters(filters, subGroup);
		}

		void AddLicenceFilters(ModuleFilterCollection filters, OccurrenceFilterSubGroup subGroup)
		{
			var enterpriseFilter = filters.AddGuidFilter("Enterprise Code", ClientModuleRegistration.LicenceEnterprise, GetLicenceEnterpriseQuery, new LicenceEnterpriseCollectionForEntCodeFilter(Factory));
			enterpriseFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			enterpriseFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			enterpriseFilter.SubGroup = subGroup;

			var enterpriseIDFilter = filters.AddGuidFilter("Enterprise ID", ClientModuleRegistration.LicenceEnterprise, GetLicenceEnterpriseQuery, new LicenceEnterpriseCollection(Factory));
			enterpriseIDFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			enterpriseIDFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			enterpriseIDFilter.SubGroup = subGroup;

			var serverFilter = filters.AddTextFilter("Database Server Code", GetDatabaseServerCodeQuery);
			serverFilter.MaxLength = LicenceDatabaseSchema.LD_ServerCode.MaxLength;
			serverFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			serverFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			serverFilter.SubGroup = subGroup;
		}

		ZQuery GetLicenceEnterpriseQuery(SQLComparisonOperator comparisonOperator, object pkValue)
		{
			var enterprisePK = ZGuid.Empty;
			ZGuid.TryParse(pkValue, out enterprisePK);

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(EdiHelpErrorLog));
			ZDBOnlySubQuery ldQuery = new ZDBOnlySubQuery(typeof(LicenceDatabase), LicenceDatabaseSchema.PK);
			ZDBOnlySubQuery leQuery = new ZDBOnlySubQuery(typeof(LicenceEnterprise), LicenceEnterpriseSchema.PK);
			leQuery.AddToFilter(LicenceEnterpriseSchema.PK, comparisonOperator, enterprisePK);
			ldQuery.AddSubQuery(LicenceDatabaseSchema.LD_LE, leQuery, JoinCondition.And);
			result.AddSubQuery(HelpErrorLogOccurrenceSchema.HO_LD, ldQuery, JoinCondition.And);

			return result;
		}

		class OccurrenceFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(EdiHelpErrorLog));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(HelpErrorLogOccurrence), HelpErrorLogOccurrenceSchema.HO_HE);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		#endregion

		#endregion

		#region Lookups

		CodeDescriptionPairList StatusList
		{
			get
			{
				if (statusList == null)
				{
					statusList = new CodeDescriptionPairList();
					statusList.AddPair(ErrorLogStatusFilter.CodeConstants.NotFixed, ErrorLogStatusFilter.DescriptionConstants.NotFixed);
					statusList.AddPair(ErrorLogStatusFilter.CodeConstants.Fixed, ErrorLogStatusFilter.DescriptionConstants.Fixed);
					statusList.AddPair(ErrorLogStatusFilter.CodeConstants.Ignored, ErrorLogStatusFilter.DescriptionConstants.Ignored);
				}
				return statusList;
			}
		}

		CodeDescriptionPairList statusList;

		#endregion
	}
}

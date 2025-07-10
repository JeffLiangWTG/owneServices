using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using Constants = Enterprise.Core.Constants;
using ResString = Enterprise.DocumentEngine.Module.ResString;

namespace Enterprise.DocumentEngine.Scheduler.Module
{
	public class ReportManagementFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddRelatedItemFilters(filters);
			AddNumberFilters(filters);
			AddHiddenFilters(filters);
			return filters;
		}

		#endregion

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Description", StmScheduleTaskSchema.S5_ScheduleDescription).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportManagementFilter|Description", "Description");
			filters.AddTextFilter("Running Server", GetRunningServerQuery).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportManagementFilter|Running Server", "Running Server");
		}

		ZDBOnlyQuery GetRunningServerQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(StmScheduleTask));

			var subQuery = new ZDBOnlySubQuery(typeof(StmReportRun), StmReportRunSchema.RRI_S5_Schedule);
			subQuery.AddToFilter(StmReportRunSchema.RRI_Status, Constants.StmReportRunState.Running);
			subQuery.AddToFilter(StmReportRunSchema.RRI_RunningServer, comparisonOperator, value);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Report Name", ModuleIDs.StmMenuItem, StmScheduleTaskSchema.S5_ParentID, MenuItemsList).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportManagementFilter|Report Name", "Report Name");
			filters.AddGuidFilter("Print User", ModuleIDs.GlbStaff, GetPrintUserQuery, new GlbStaffCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportManagementFilter|Print User", "Print User");
		}

		ZQuery GetPrintUserQuery(ZGuid staffGuid)
		{
			var staff = Factory.Load<GlbStaff>(staffGuid);
			var userCode = staff != null ? staff.GS_Code : ZString.Empty;
			return new ZQuery(StmScheduleTaskSchema.S5_GS_NKPrintUser, userCode);
		}

		#endregion

		#region Number Filters

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberRangeFilter("Last Processed Time (Seconds)", GetLastProcessedTimeQuery).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportManagementFilter|Last Processed Time Seconds", "Last Processed Time (Seconds)");
			filters.AddNumberRangeFilter("Time Being Processed (Seconds)", GetTimeBeingProcessedQuery).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportManagementFilter|Time Being Processed Seconds", "Time Being Processed (Seconds)");
		}

		ZQuery GetLastProcessedTimeQuery(INumericZType fromValue, INumericZType toValue)
		{
			var query = new ZDBOnlyQuery(typeof(StmScheduleTask));
			var subQuery = new ZDBOnlySubQuery(typeof(StmReportRun), StmReportRunSchema.RRI_S5_Schedule);
			subQuery.AddToFilter(StmReportRunSchema.RRI_Status, Constants.StmReportRunState.Finished);

			subQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "DATEDIFF(second, RRI_StartTimeUtc, RRI_EndTimeUtc) >= {0}", fromValue), null);
			subQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "DATEDIFF(second, RRI_StartTimeUtc, RRI_EndTimeUtc) <= {0}", toValue), null);

			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetTimeBeingProcessedQuery(INumericZType fromValue, INumericZType toValue)
		{
			var query = new ZDBOnlyQuery(typeof(StmScheduleTask));

			var subQuery = new ZDBOnlySubQuery(typeof(StmReportRun), StmReportRunSchema.RRI_S5_Schedule);
			subQuery.AddToFilter(StmReportRunSchema.RRI_Status, Constants.StmReportRunState.Running);

			subQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "DATEDIFF(second, RRI_StartTimeUtc, GetUTCDate()) >= {0}", fromValue), null);
			subQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "DATEDIFF(second, RRI_StartTimeUtc, GetUTCDate()) <= {0}", toValue), null);

			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Hidden Filters

		void AddHiddenFilters(ModuleFilterCollection filters)
		{
			var scheduleReportFilter = filters.AddTextFilter("Schedule Report", GetAlwaysAppliedAndHiddenQuery);
			scheduleReportFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
		}

		ZQuery GetAlwaysAppliedAndHiddenQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(StmScheduleTaskSchema.S5_NextScheduledPrintRunTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);

			return query;
		}

		#endregion

		#region Lookups

		#region MenuItems

		public StmMenuItemCollection MenuItemsList => fMenuItems ?? (fMenuItems = new StmMenuItemCollection(Factory));

		StmMenuItemCollection fMenuItems;

		#endregion

		#endregion

		#region Index Search

		protected override SearchField ResolveSearchField(SearchField searchField)
		{
			var fieldNameUpper = searchField.FieldName.ToUpperInvariant();
			switch (fieldNameUpper)
			{
				case SearchFieldConstants.ReportName:
					var reportNameFilter = new IndexSearchModuleGuidFilter(searchField, ModuleIDs.StmMenuItem, MenuItemsList);
					return new SearchFieldOverride(searchField, reportNameFilter);
				case SearchFieldConstants.NextScheduledPrintRunTimeUtc:
					var nextScheduledPrintRunTimeUtcFilter = new IndexSearchModuleDateFilter(searchField);
					nextScheduledPrintRunTimeUtcFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
					nextScheduledPrintRunTimeUtcFilter.PropertySearch = ModuleDateFilter.Past;
					return new SearchFieldOverride(searchField, nextScheduledPrintRunTimeUtcFilter);
				case SearchFieldConstants.StartDate:
				case SearchFieldConstants.EndDate:
				case SearchFieldConstants.DeliveryAddress:
				case SearchFieldConstants.AddressOverride:
				case SearchFieldConstants.EmailBCC:
				case SearchFieldConstants.EmailCC:
				case SearchFieldConstants.Branch:
					return null;
				default:
					return base.ResolveSearchField(searchField);
			}
		}

		static class SearchFieldConstants
		{
			public const string ReportName = "REPORTNAMEBYPARENTID";
			public const string NextScheduledPrintRunTimeUtc = "NEXTSCHEDULEDPRINTRUNTIMEUTC";
			public const string StartDate = "STARTDATE";
			public const string EndDate = "ENDDATE";
			public const string DeliveryAddress = "DELIVERYADDRESS";
			public const string AddressOverride = "ADDRESSOVERRIDE";
			public const string EmailBCC = "EMAILBCC";
			public const string EmailCC = "EMAILCC";
			public const string Branch = "BRANCHPK";
		}

		#endregion
	}
}

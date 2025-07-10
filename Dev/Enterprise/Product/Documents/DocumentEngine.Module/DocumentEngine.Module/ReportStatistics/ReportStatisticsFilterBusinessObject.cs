using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;
using Res = Enterprise.DocumentEngine.Module.Res;
using ResString = Enterprise.DocumentEngine.Module.ResString;

namespace Enterprise.DocumentEngine.Scheduler.Module
{
	public class ReportStatisticsFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddDateFilters(filters);
			AddNumberFilters(filters);
			AddRelatedFilters(filters);
			AddReportSourceFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Report Name", StmReportRunSchema.RRI_ReportName).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportStatisticsFilter|Report Name", "Report Name");
			filters.AddTextFilter("Report Description", StmReportRunSchema.RRI_ReportDescription).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportStatisticsFilter|Report Description", "Report Description");
			filters.AddTextFilter("Status", StmReportRunSchema.RRI_Status, StmReportRun.StmReportRunStatus).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportStatisticsFilter|Status", "Status");
			filters.AddTextFilter("Running Server", StmReportRunSchema.RRI_RunningServer).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportStatisticsFilter|Running Server", "Running Server");
			filters.AddTextFilter("SQL Server", StmReportRunSchema.RRI_SQLServer).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportStatisticsFilter|SQL Server", "SQL Server");
		}

		#endregion

		#region Date

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter("Queue Start Date", StmReportRunSchema.RRI_StartTimeInQueueUtc, true).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportStatisticsFilter|Queue Start Date", "Queue Start Date");
			filters.AddDateFilter("Start Date", StmReportRunSchema.RRI_StartTimeUtc, true).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportStatisticsFilter|Start Date", "Start Date");
			filters.AddDateFilter("End Date", StmReportRunSchema.RRI_EndTimeUtc, true).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportStatisticsFilter|End Date", "End Date");
		}

		#endregion

		#region Related Items

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberRangeFilter("Report Size (Bytes)", StmReportRunSchema.RRI_ReportSizeBytes).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportStatisticsFilter|Report Size Bytes", "Report Size (Bytes)");
			filters.AddNumberRangeFilter("Rows Returned", StmReportRunSchema.RRI_RowsReturned).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportStatisticsFilter|Rows Returned", "Rows Returned");
			var filter = filters.AddNumberRangeFilter("Duration (Seconds)", GetDurationQuery);
			filter.MinValue = 0;
			filter.MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportStatisticsFilter|Duration Seconds", "Duration (Seconds)");
			filter = filters.AddNumberRangeFilter("Queue Duration (Seconds)", GetQueueDurationQuery);
			filter.MinValue = 0;
			filter.MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportStatisticsFilter|Queue Duration Seconds", "Queue Duration (Seconds)");
			filters.AddNumberRangeFilter("SQL Run Duration (Milliseconds)", StmReportRunSchema.RRI_SQLRunDurationMilliseconds).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportStatisticsFilter|RRI_SQLRunDurationMilliseconds", "SQL Run Duration (Milliseconds)");
			filters.AddNumberRangeFilter("SQL CPU Duration (Milliseconds)", StmReportRunSchema.RRI_SQLCPUDurationMilliseconds).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportStatisticsFilter|RRI_SQLCPUDurationMilliseconds", "SQL CPU Duration (Milliseconds)");
			filters.AddNumberRangeFilter("Client Run Duration (Milliseconds)", StmReportRunSchema.RRI_ClientRunDurationMilliseconds).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportStatisticsFilter|RRI_ClientRunDurationMilliseconds", "Client Run Duration (Milliseconds)");
			filters.AddNumberRangeFilter("Client CPU Duration (Milliseconds)", StmReportRunSchema.RRI_ClientCPUDurationMilliseconds).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportStatisticsFilter|RRI_ClientCPUDurationMilliseconds", "Client CPU Duration (Milliseconds)");
		}

		void AddRelatedFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Print User", ModuleIDs.GlbStaff, GetPrintUserQuery, new GlbStaffCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|ReportStatisticsFilter|Print User", "Print User");
		}

		ZQuery GetPrintUserQuery(ZGuid staffGuid)
		{
			var staff = Factory.Load<GlbStaff>(staffGuid);
			var userCode = staff != null ? staff.GS_Code : ZString.Empty;
			return new ZQuery(StmReportRunSchema.RRI_GS_NKPrintUser, userCode);
		}

		ZQuery GetQueueDurationQuery(INumericZType value1, INumericZType value2)
		{
			var result = new ZDBOnlyQuery(typeof(StmReportRun));
			if (!value1.IsEmpty)
			{
				result.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "DATEDIFF(second, RRI_StartTimeInQueueUtc, RRI_StartTimeUtc) >= {0}", value1), null);
			}
			if (!value2.IsEmpty)
			{
				result.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "DATEDIFF(second, RRI_StartTimeInQueueUtc, RRI_StartTimeUtc) <= {0}", value2), null);
			}
			return result;
		}

		ZQuery GetDurationQuery(INumericZType value1, INumericZType value2)
		{
			var result = new ZDBOnlyQuery(typeof(StmReportRun));
			if (!value1.IsEmpty)
			{
				result.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "DATEDIFF(second, RRI_StartTimeUtc, RRI_EndTimeUtc) >= {0}", value1), null);
			}
			if (!value2.IsEmpty)
			{
				result.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "DATEDIFF(second, RRI_StartTimeUtc, RRI_EndTimeUtc) <= {0}", value2), null);
			}
			return result;
		}

		#endregion

		#region Is System Defined

		public override bool ShouldAddSystemDefinedStatusFilter => false;

		#endregion

		#region Report Source

		void AddReportSourceFilters(ModuleFilterCollection filters)
		{
			var reportSourceFilter = filters.AddTextFilter(SystemDefinedStatus, GetReportSourceQuery, GetReportSourceList());
			reportSourceFilter.DefaultProperty = DefinedStatusAllCode;
			reportSourceFilter.Category = FilterCategories.Other;
			reportSourceFilter.MultilingualDescription = ResString.GetMultilingualString("62C4B790-F0EF-4F90-9017-1CD9E2C39227", SystemDefinedStatus);
		}

		ZQuery GetReportSourceQuery(ZString value)
		{
			var query = new ZQuery();

			if (value == DefinedStatusSystemCode)
			{
				query.AddToFilter(StmReportRunSchema.RRI_IsSystemDefined, ZBool.True);
			}
			else if (value == CustomizedCode)
			{
				query.AddToFilter(StmReportRunSchema.RRI_IsSystemDefined, ZBool.False);
			}

			return query;
		}

		CodeDescriptionPairList GetReportSourceList()
		{
			var reportSourceList = new CodeDescriptionPairList();
			reportSourceList.AddPair(DefinedStatusAllCode, DefinedStatusAllDescription);
			reportSourceList.AddPair(DefinedStatusSystemCode, DefinedStatusSystemDescription);
			reportSourceList.AddPair(CustomizedCode, CustomizedDescription);
			return reportSourceList;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter code")]
		const string SystemDefinedStatus = "Is System Defined";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter code")]
		const string CustomizedCode = "Customized";
		string CustomizedDescription => Res.GetString("9D4AC788-9114-407B-89BC-9E32455B3A49", "Customized");

		#endregion

		#endregion

		#region IndexSearch

		protected override SearchField ResolveSearchField(SearchField searchField)
		{
			if (searchField.FieldName == "ISSYSTEMDEFINED")
			{
				var filter = new IndexSearchModuleTextFilter(SystemDefinedStatus, searchField, GetSystemDefinedStatusGlowQuery, GetReportSourceList(), FilterCategories.Other);
				filter.DefaultProperty = DefinedStatusAllCode;
				return new SearchFieldOverride(searchField, filter);
			}
			return base.ResolveSearchField(searchField);
		}

		IGlowQuery GetSystemDefinedStatusGlowQuery(SearchField searchField, ZString value)
		{
			if (value == DefinedStatusAllCode)
			{
				return new EmptyQuery();
			}

			var isSystemDefined = value == DefinedStatusSystemCode;
			return IndexSearchModuleFlagsFilter.GetGlowIndexQueryCore(searchField, isSystemDefined);
		}

		#endregion
	}
}

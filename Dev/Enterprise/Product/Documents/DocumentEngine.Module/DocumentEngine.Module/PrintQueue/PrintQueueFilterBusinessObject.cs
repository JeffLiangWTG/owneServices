using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Module
{
	public class PrintQueueFilterBusinessObject : FilterStripBusinessObject
	{
		public PrintQueueFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddFlagFilters(filters);
			return filters;
		}

		protected override void AddInitialAuditFilters(ModuleFilterCollection filters)
		{
			base.AddInitialAuditFilters(filters);
			AddLastUsedDateFilter(filters);
		}

		void AddLastUsedDateFilter(ModuleFilterCollection filters)
		{
			var filterUtc = filters.AddDateFilter("Last Used Date (UTC)", StmPrintQueueSchema.SQ_LastUsedDateTimeUtc, false);
			filterUtc.MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|PrintQueueFilter|LastUsedDateUTC", "Last Used Date (UTC)");
			filterUtc.Category = FilterCategories.AuditInformation;
			filterUtc.UserEntersUtcValue = true;

			var filterLocal = filters.AddDateFilter("Last Used Date (Local)", StmPrintQueueSchema.SQ_LastUsedDateTimeUtc, true);
			filterLocal.MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|PrintQueueFilter|LastUsedDateLocal", "Last Used Date (Local)");
			filterLocal.Category = FilterCategories.AuditInformation;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var requestSubGroup = new ServerFilterSubGroup();

			var serverFilter = filters.AddTextFilter("Server Name", StmPrintServerSchema.SPS_ServerName);
			serverFilter.MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|PrintQueueFilter|ServerName", "Server Name");
			serverFilter.SubGroup = requestSubGroup;

			filters.AddTextFilter("Display Name", StmPrintQueueSchema.SQ_DisplayName).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|PrintQueueFilter|DisplayName", "Display Name");
			filters.AddTextFilter("Queue Name", StmPrintQueueSchema.SQ_QueueName).MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|PrintQueueFilter|QueueName", "Queue Name");
		}

		class ServerFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(StmPrintQueue));
				var serverSubQuery = new ZDBOnlySubQuery(typeof(StmPrintServer), StmPrintQueueSchema.SQ_SPS_Server);
				serverSubQuery.AddToFilter(filter);
				result.AddSubQuery(serverSubQuery, JoinCondition.And);
				return result;
			}
		}

		#endregion

		#region Flags

		void AddFlagFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddFlagsFilter("Allow Printing Only", new string[] { Res.GetString("DocumentEngine|PrintQueueFilter|AllowPrintingOnly", "Allow Printing Only") }, new CargoWise.Schema.SchemaBoolColumn[] { StmPrintQueueSchema.SQ_AllowPrinting });
			filter.MultilingualDescription = ResString.GetMultilingualString("DocumentEngine|PrintQueueFilter|AllowPrintingOnly", "Allow Printing Only");
			filter.Visibility = FilterVisibility.AlwaysVisible;
		}

		#endregion

		#endregion
	}
}

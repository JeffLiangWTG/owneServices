using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Resource.Shared;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.GlowInterop;
using Newtonsoft.Json;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.AuditDataServices.Glow.Subscribers
{
	public class GlowIndexUpdateSubscriber : ChangedTableListOnlyAuditSubscriber
	{
		public override string Code => "GIU";

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description")]
		public override string Description => "GLOW Index Updater";

		public override bool IsRequired() => GlowServiceClient.IsServiceUriSpecified;

		IGlowServiceClient Client => client ??= GlowServiceClient.GetClient();

		[ThreadSafe(ThreadSafeAttribute.Mechanism.Interlocked)]
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static IGlowServiceClient client;

		public override IEnumerable<ITableSchema> SubscribedTables => subscribedTables ?? (subscribedTables = GetSubscribedTables());

		[ThreadSafe(ThreadSafeAttribute.Mechanism.Interlocked)]
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static IEnumerable<ITableSchema> subscribedTables;

		public override void ProcessChanges(ILogger logger, IEnumerable<ITableSchema> changedTables)
		{
			var tableNames = changedTables.Select(t => t.TableName);
			if (tableNames.Any() && Client != null)
			{
				var model = new ReportTablesChangedRequestModel(tableNames);
				Client.PostJsonAsync("odata/Index/ReportTablesChanged", model, logger).ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}

		static IEnumerable<ITableSchema> GetSubscribedTables() =>
			CdcRequiredTableHelper
				.Load(Assembly.Load("Enterprise.DbUpgrader.Resource"))
				.Select(t => t.TableName)
				.Select(ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema).Where(t => t != null);

		public static void ResetStaticMemebers()
		{
			if (client != null)
			{
				client.Dispose();
				client = null;
			}
		}
	}

	class ReportTablesChangedRequestModel(IEnumerable<string> tableNames)
	{
		[JsonProperty("tableNames")]
		public IEnumerable<string> TableNames { get; } = tableNames;
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.AuditDataServices.Glow.Subscribers
{
	public class ProcessTaskTemplateGlowSubscriber : ActualDataChangesAuditSubscriber
	{
		public override ITableSchema Table => ProcessTaskTemplateSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns =>
		[
			ProcessTaskTemplateSchema.P0_Name,
			ProcessTaskTemplateSchema.P0_ProcessType,
			ProcessTaskTemplateSchema.P0_IsActive,
			ProcessTaskTemplateSchema.P0_IsPartialTemplate,
			ProcessTaskTemplateSchema.P0_IsSystem,
			ProcessTaskTemplateSchema.P0_IsUniversal,
			ProcessTaskTemplateSchema.P0_GC,
			ProcessTaskTemplateSchema.P0_OH_Client,
			ProcessTaskTemplateSchema.P0_SubType1,
			ProcessTaskTemplateSchema.P0_LoadPortCountry,
			ProcessTaskTemplateSchema.P0_DischargePortCountry,
			ProcessTaskTemplateSchema.P0_EffectiveStartDateUtc,
			ProcessTaskTemplateSchema.P0_EffectiveEndDateUtc,
		];

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => true;

		public override Action<DataRow> CustomFilter => null;

		public override string Code => "PTG";

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "ProcessTaskTemplate Change Subscriber";

		public override bool IsRequired() => GlowServiceClient.IsServiceUriSpecified;

		IGlowServiceClient Client => client ??= GlowServiceClient.GetClient();

		[ThreadSafe(ThreadSafeAttribute.Mechanism.Interlocked)]
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static IGlowServiceClient client;

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			var data = new HashSet<string>();

			foreach (DataRow row in changeTable.Rows)
			{
				string processType = null;

				if (row.RowState != DataRowState.Deleted)
				{
					processType = row[ProcessTaskTemplateSchema.Constants.P0_ProcessType].ToString();
					data.Add(processType);
				}

				if (row.RowState != DataRowState.Added)
				{
					var originalProcessType = row[ProcessTaskTemplateSchema.Constants.P0_ProcessType, DataRowVersion.Original].ToString();

					if (row.RowState == DataRowState.Deleted || processType != originalProcessType)
					{
						data.Add(originalProcessType);
					}
				}
			}

			if (data.Count > 0 && Client != null)
			{
				Client.PostJsonAsync("api/workflowTemplate/reportChanged", data, logger).ConfigureAwait(false).GetAwaiter().GetResult();
			}
		}

		public static void ResetStaticMemebers()
		{
			if (client != null)
			{
				client.Dispose();
				client = null;
			}
		}
	}
}

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
	public class ProcessTemplateValidationActionGlowSubscriber : ActualDataChangesAuditSubscriber
	{
		public override ITableSchema Table => ProcessTemplateValidationActionSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns =>
		[
			ProcessTemplateValidationActionSchema.P0A_ActionSource,
			ProcessTemplateValidationActionSchema.P0A_P0_WorkflowTemplate,
			ProcessTemplateValidationActionSchema.P0A_P0V_ValidationRule,
		];

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => true;

		public override Action<DataRow> CustomFilter => null;

		public override string Code => "PTA";

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "ProcessTemplateValidationAction Change Subscriber";

		public override bool IsRequired() => GlowServiceClient.IsServiceUriSpecified;

		IGlowServiceClient Client => client ??= GlowServiceClient.GetClient();

		[ThreadSafe(ThreadSafeAttribute.Mechanism.Interlocked)]
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static IGlowServiceClient client;

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			var pks = new HashSet<Guid>();
			foreach (DataRow row in changeTable.Rows)
			{
				if (row.RowState != DataRowState.Deleted)
				{
					var templatePK = (Guid)row[ProcessTemplateValidationActionSchema.Constants.P0A_P0_WorkflowTemplate];
					pks.Add(templatePK);
				}

				if (row.RowState != DataRowState.Added)
				{
					var originalTemplatePK = (Guid)row[ProcessTemplateValidationActionSchema.Constants.P0A_P0_WorkflowTemplate, DataRowVersion.Original];
					pks.Add(originalTemplatePK);
				}
			}

			if (pks.Count > 0 && Client != null)
			{
				Client.PostJsonAsync("api/workflowTemplate/validation/reportChanged", pks, logger).ConfigureAwait(false).GetAwaiter().GetResult();
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

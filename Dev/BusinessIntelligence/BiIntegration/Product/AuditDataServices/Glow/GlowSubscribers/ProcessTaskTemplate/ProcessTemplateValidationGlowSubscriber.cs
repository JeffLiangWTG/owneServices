using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.AuditDataServices.Glow.Subscribers
{
	public class ProcessTemplateValidationGlowSubscriber : ActualDataChangesAuditSubscriber
	{
		public override ITableSchema Table => ProcessTemplateValidationSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns =>
		[
			ProcessTemplateValidationSchema.P0V_Severity,
			ProcessTemplateValidationSchema.P0V_ValidationRule,
			ProcessTemplateValidationSchema.P0V_Message,
			ProcessTemplateValidationSchema.P0V_Condition1,
			ProcessTemplateValidationSchema.P0V_Condition2,
			ProcessTemplateValidationSchema.P0V_Condition2Value,
			ProcessTemplateValidationSchema.P0V_Description,
			ProcessTemplateValidationSchema.P0V_FieldToDisplayValidation,
			ProcessTemplateValidationSchema.P0V_P0_WorkflowTemplate,
		];

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => true;

		public override Action<DataRow> CustomFilter => null;

		public override string Code => "PTV";

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "subscriber description")]
		public override string Description => "ProcessTemplateValidation Change Subscriber";

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
					var templatePK = (Guid)row[ProcessTemplateValidationSchema.Constants.P0V_P0_WorkflowTemplate];
					pks.Add(templatePK);
				}

				if (row.RowState != DataRowState.Added)
				{
					var originalTemplatePK = (Guid)row[ProcessTemplateValidationSchema.Constants.P0V_P0_WorkflowTemplate, DataRowVersion.Original];
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

		protected BusinessObjectFactory DataFactory => dataFactory ??= new BusinessObjectFactory { RefreshEnabled = false };
		BusinessObjectFactory dataFactory;
	}
}

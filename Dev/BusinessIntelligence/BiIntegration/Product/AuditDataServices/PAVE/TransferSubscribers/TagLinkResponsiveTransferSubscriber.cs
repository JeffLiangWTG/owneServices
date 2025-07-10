using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.BufferManagement.Service.Client;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.PAVE.Subscribers
{
	public class TagLinkResponsiveTransferSubscriber : PAVEServiceTaskNudgingSubscriber
	{
		public override string Code => "PTT";

		public override ITableSchema Table => TagLinkSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => new SchemaColumn[4] {
			TagLinkSchema.TGL_ParentId,
			TagLinkSchema.TGL_ParentTableCode,
			TagLinkSchema.TGL_Magnitude,
			TagLinkSchema.TGL_TGM_Magnitude
		};

		public override bool IsRequired() => true;

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => true;

		public override string Description => nameof(TagLinkResponsiveTransferSubscriber);

		public override Action<DataRow> CustomFilter => null;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Message")]
		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			var isEnabled = BMSRegistry.BufferManagementEnabled && BMSRegistry.EnableResponsivePAVEDataProcessing && BMSRegistry.TransferWorkflowComponentOnChanges;

			if (!isEnabled)
			{
				logger.Log(LogType.Debug, $"{changeTable.Rows.Count} changes were skipped as the {Code} subscriber is disabled.");
				return;
			}

			if (changeTable.Rows.Count > BMSRegistry.TransferWorkflowComponentMaximumNumberOfCDCChanges)
			{
				logger.Log(LogType.Warning, $"Number of changes: {changeTable.Rows.Count} greater than {nameof(BMSRegistry.TransferWorkflowComponentMaximumNumberOfCDCChanges)}: {BMSRegistry.TransferWorkflowComponentMaximumNumberOfCDCChanges}; Workflow Transfers will not be processed responsively and will be deferred to the Buffer Management Schematic Transfer Runner service task."); // Log Message

				BMSRegistry.ProcessAllTransferRulesLinksOnNextBMSRun = true;
				MaybeNudgeServiceTask(logger,
					shouldNudge: BMSRegistry.EnableNudgingBMSServiceTaskByThePVEServiceTask,
					serviceTaskCode: "BMS",
					delay: BMSRegistry.DelayForNudgingTheBMSServiceTaskByThePVEServiceTask);
				return;
			}

			var rowTransEndTime = changeTable.Rows
				.Cast<DataRow>()
				.Select(change =>
				{
					var dataVersion = change.RowState != DataRowState.Deleted ? DataRowVersion.Current : DataRowVersion.Original;

					return new
					{
						ParentId = new Guid(change[TagLinkSchema.Constants.TGL_ParentId, dataVersion].ToString()),
						ParentTableCode = change[TagLinkSchema.Constants.TGL_ParentTableCode, dataVersion].ToString(),
						TransactionEndTimeUtc = DateTime.SpecifyKind((DateTime)change["TranEndTimeUtc", dataVersion], DateTimeKind.Utc),
					};
				})
				.Where(row => row.ParentTableCode == ProcessHeaderSchema.Constants.Prefix)
				.ToArray();

			var parentIDs = rowTransEndTime
				.Select(row => row.ParentId)
				.ToHashSet();

			var pks = GetWorkflowPKs(parentIDs).ToArray();
			var schematicServiceClient = ObjectFactory.Get<IPAVEServiceClientFactory>().GetSchematicServiceClient();
			const int processTransferRulesBatchSize = 1024; //TODO: Move to registry?

			try
			{
				foreach (var batch in pks.Batch(processTransferRulesBatchSize))
				{
					schematicServiceClient.ProcessTransferRules(batch, logger);
				}
			}
			catch (Exception e)
			{
				logger.Warning(string.Format(CultureInfo.InvariantCulture, "CDC updates failed for {0}, Error: {1})", Table.TableName, e.ToString()));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static IEnumerable<Guid> GetWorkflowPKs(HashSet<Guid> parentIDs)
		{
			if (parentIDs.Count == 0)
			{
				return Enumerable.Empty<Guid>();
			}

			var list = new List<Guid>();
			var selectFromPKs = "(SELECT * FROM @PKs)";
			var query = string.Format(CultureInfo.InvariantCulture,
		$@"SELECT {ProcessHeaderSchema.Constants.PK}, {ProcessHeaderSchema.Constants.FH_FH_ParentHeader}
FROM dbo.ProcessHeader
WHERE {ProcessHeaderSchema.Constants.PK} IN {selectFromPKs} AND {ProcessHeaderSchema.Constants.FH_FH_ParentHeader} IS NOT NULL
UNION
SELECT {ProcessHeaderSchema.Constants.PK}, {ProcessHeaderSchema.Constants.FH_FH_ParentHeader}
FROM dbo.ProcessHeader
WHERE {ProcessHeaderSchema.Constants.FH_FH_ParentHeader} IN {selectFromPKs}");

			var command = Db.Connection.Command(query);

			command.AddTableValuedParameter("@PKs", "dbo.TVP_uniqueidentifier", parentIDs);

			using (var result = command.ExecuteReader(CommandBehavior.SequentialAccess))
			{
				while (result.Read())
				{
					var pk = result.GetGuid(0);
					var parentPk = result.GetGuid(1);
					if (parentIDs.Contains(pk) || parentIDs.Contains(parentPk))
					{
						list.Add(pk);
					}
				}
			}

			return list;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.PAVE.Subscribers
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Message")]
	public class ProcessHeaderEffectiveNudgeUpdateSubscriber : ActualDataChangesAuditSubscriber
	{
		#region Properties

		public override string Code => "PHN";

		public override string Description => nameof(ProcessHeaderEffectiveNudgeUpdateSubscriber);

		public override ITableSchema Table => ProcessHeaderSchema.Instance;

		public override IEnumerable<SchemaColumn> SpecificColumns => [ProcessHeaderSchema.FH_VoteUpDownAmount, ProcessHeaderSchema.FH_IsActive, ProcessHeaderSchema.FH_Status];

		public override Action<DataRow> CustomFilter => (DataRow row) =>
		{
			if (row[ProcessHeaderSchema.Constants.FH_P0_Template] != DBNull.Value)
			{
				row.Delete();
			}
		};

		public override bool IsRequired() => true;

		public override bool NotifyInsert => true;

		public override bool NotifyUpdate => true;

		public override bool NotifyDelete => false;

		#endregion
		#region ProcessChange

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			var bmsRegistry = ObjectFactory.Get<IBMSRegistry>();
			var isEnabled = bmsRegistry.BufferManagementEnabled && bmsRegistry.EnableResponsivePAVEDataProcessing && bmsRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges;

			if (!isEnabled)
			{
				logger.Log(LogType.Information, $"{changeTable.Rows.Count} changes were skipped as the {Code} subscriber is disabled.");
				return;
			}

			var shouldLogWorkflowInfo = ObjectFactory.Get<IBMSRegistry>().LogWorkflowInfoOnResponsiveWorkflowUpdatesOnRelatedObjectChanges;

			var rows = changeTable.Rows.Cast<DataRow>();
			var workflowPKs = rows.Select(r => (Guid)r[ProcessHeaderSchema.Constants.PK, DataRowVersion.Current]).ToHashSet();

			var batchCount = 0;
			var workflowCountTotal = 0;

			foreach (var batch in workflowPKs.Batch(GetBatchSize()))
			{
				try
				{
					batchCount++;

					var processedWorkflowsInBatch = 0;

					ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
					{
						var factory = new BusinessObjectFactory() { NameForDebugging = $"{nameof(ProcessHeaderEffectiveNudgeUpdateSubscriber)}{batchCount}" };
						processedWorkflowsInBatch = 0;

						var workflows = factory.Load<IProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, batch))
							.Where(w => w.FH_FH_ParentHeader.IsEmpty || !workflowPKs.Contains(w.FH_FH_ParentHeader.ToGuid())) // otherwise we update this workflows through its JLW
							.ToArray();

						if (workflows.Length > 0)
						{
							AddFetchHintsForBatchProcessing(workflows, factory);

							foreach (var workflow in workflows)
							{
								workflow.UpdateEffectiveNudge();
								processedWorkflowsInBatch++;

								if (shouldLogWorkflowInfo)
								{
									logger.Debug($"Updated effective nudge on {workflow.FH_CompletionStatement} (PK = {workflow.PK}, Job = {workflow.ParentJobDescription})");
								}
							}
							factory.Save();
						}
					}, onRetry: () => { });
					workflowCountTotal += processedWorkflowsInBatch;
					logger.Information($"Updated effective nudge on {pluralizationHelper.Value.GetCountText(processedWorkflowsInBatch)} in a batch ({pluralizationHelper.Value.GetCountText(workflowCountTotal)} in total in {pluralizationHelper.Value.GetCountText(batchCount, "batch", "batches")})");
				}
				catch (ZConcurrencyCheckFailureException ex) when (IsBookingConvertedToShipment(ex))
				{
					logger.Information("Skipping nudge updates for workflows - booking has been converted to shipment.");
					continue;
				}
			}
		}

		public bool IsBookingConvertedToShipment(ZConcurrencyCheckFailureException ex)
		{
			return ex.Message.Contains("Another user has converted the booking into a shipment.");
		}

		void AddFetchHintsForBatchProcessing(IReadOnlyCollection<IProcessHeader> workflows, BusinessObjectFactory factory)
		{
			var jobHeaderPKs = workflows.Where(w => w.FH_FH_ParentHeader.IsEmpty).Select(w => w.PK).ToArray();
			factory.AddFetchHint(ProcessHeaderSchema.Instance, new ZQuery(ProcessHeaderSchema.FH_FH_ParentHeader, jobHeaderPKs)); // child workflows

			foreach (var workflow in workflows)
			{
				workflow.AddDeepFetchHintForParentType(specificFactory: null, parentType: null);

				if (workflow.FH_FH_ParentHeader.IsEmpty)
				{
					var jobHeader = workflow;
					var childWorkflows = factory.Load<IProcessHeader>(new ZQuery(ProcessHeaderSchema.FH_FH_ParentHeader, jobHeader.PK));

					foreach (var childWorkflow in childWorkflows)
					{
						childWorkflow.AddTagLinksFetchHint();
					}
					jobHeader.AddTagLinksFetchHint();
				}
				else
				{
					workflow.AddTagLinksFetchHint();
					workflow.JobHeader.AddTagLinksFetchHint();
				}
			}
		}

		int GetBatchSize() => ObjectFactory.Get<IBMSRegistry>().ResponsiveWorkflowUpdatesBatchSize;

		static string[] GetWorkflowOpenStatuses() => [OpenWorkflowStatus, BlockedWorkflowStatus];

		const string OpenWorkflowStatus = "OPN";
		const string BlockedWorkflowStatus = "BLK";

		readonly Lazy<IBMPluralizationHelper> pluralizationHelper = new Lazy<IBMPluralizationHelper>(() => ObjectFactory.Get<IBMPluralizationHelper>());

		#endregion
	}
}

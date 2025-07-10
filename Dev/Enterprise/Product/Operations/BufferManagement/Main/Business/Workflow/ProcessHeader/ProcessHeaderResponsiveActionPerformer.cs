using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessHeaderResponsiveActionPerformer
	{
		public ProcessHeaderResponsiveActionPerformer(ILogger logger, ZQuery workflowQuery, string actionCode)
		{
			this.logger = logger;
			this.workflowQuery = workflowQuery;
			this.actionCode = actionCode;
		}

		readonly ILogger logger;
		readonly ZQuery workflowQuery;
		readonly string actionCode;

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task logging")]
		public void PerformActionOnMultipleWorkflows(CancellationToken token = default)
		{
			using (var loader = new ProcessHeaderResponsiveActionBatchLoader(logger, workflowQuery, GetWorkflowQueryNameCore()))
			{
				ProcessHeader[] workflowBatch;
				int batchCount = 0;
				int workflowCountTotal = 0;

				var shouldLogWorkflowInfo = BMSRegistry.Instance.LogWorkflowInfoOnResponsiveWorkflowUpdatesOnRelatedObjectChanges.Value;
				var reloadedComponentPKs = new HashSet<ZGuid>();

				while ((workflowBatch = loader.LoadNextBatch().Cast<ProcessHeader>().ToArray()).Any())
				{
					token.ThrowIfCancellationRequested();

					batchCount++;
					int workflowCountInBatch = 0;

					loader.AddFetchHintsForBatchProcessing(workflowBatch);
					ReloadBMComponentsFromDbIgnoringUberCacheIfNeeded(workflowBatch, reloadedComponentPKs);
					ProcessHeader.SuppressUpdatingWorkflowStatusesOnSave(workflowBatch[0].Factory);

					foreach (var workflow in workflowBatch)
					{
						workflowCountInBatch++;
						workflowCountTotal++;
						workflow.PerformResponsiveAction(actionCode);

						if (shouldLogWorkflowInfo)
						{
							logger.Log(LogType.Debug, $"Performed {actionCode} responsive action on {workflow.FH_CompletionStatement} (PK = {workflow.PK}, Job = {workflow.ParentJobDescription})");
						}
					}
					loader.SaveBatchChanges();
					var pluralizationHelper = new BMPluralizationHelper();
					logger.Log(LogType.Debug, $"Performed {actionCode} responsive action on {pluralizationHelper.GetCountText(workflowCountInBatch)} in a batch ({pluralizationHelper.GetCountText(workflowCountTotal)} in total in {pluralizationHelper.GetCountText(batchCount, "batch", "batches")})");
				}

				if (batchCount == 0)
				{
					logger.Log(LogType.Debug, $"No workflows to perform {actionCode} responsive action on");
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Command")]
		protected string GetWorkflowQueryNameCore()
		{
			var queryType = nameof(ProcessHeaderResponsiveActionPerformer);
			var queryName = $@"
-- Database:    {Db.DatabaseName}
-- Type:        {queryType}
-- Action code: {actionCode}";
			return queryName;
		}

		void ReloadBMComponentsFromDbIgnoringUberCacheIfNeeded(ProcessHeader[] workflows, HashSet<ZGuid> reloadedComponentPKs)
		{
			if (actionCode != ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment &&
				actionCode != ProcessHeaderResponsiveActionConstants.UpdateDedicatedBufferEffectiveBranchAndDepartment &&
				actionCode != ProcessHeaderResponsiveActionConstants.UpdateAllExceptDedicatedBuffer)
			{
				return;
			}

			var componentPKs = workflows
				.SelectMany(w => new ZGuid[] { w.FH_FC_CurrentComponent, w.FH_FC_DedicatedBuffer })
				.Where(pk => pk.IsValid)
				.Distinct()
				.Except(reloadedComponentPKs);
			var query = new ZQuery(BMComponentSchema.PK, componentPKs);
			query.ReLoadExistingRows = true;
			var factory = workflows.First().Factory;
			factory.Load<BMComponent>(query);
			reloadedComponentPKs.AddRange(componentPKs);

			ProcessHeader.SuppressReloadingComponentsFromDbOnSave(factory);
		}

		class ProcessHeaderResponsiveActionBatchLoader : WorkflowBatchLoader<DataAccessor>
		{
			public ProcessHeaderResponsiveActionBatchLoader(ILogger logger, ZQuery query, string queryName)
				: base(new DataAccessor(logger), query, queryName)
			{
				this.logger = logger;
			}

			readonly ILogger logger;

			protected override int GetWorkflowBatchSizeCore() => BMSRegistry.Instance.ResponsiveWorkflowUpdatesBatchSize.Value;

			protected override BatchLogger GetBatchLoggerCore()
			{
				return new BatchLogger(logger, nameof(ProcessHeaderResponsiveActionBatchLoader));
			}

			protected override string GetEmailIntroForNotificationGroupAboutSQLException()
			{
				var emailIntro = Res.GetString("96F246CB-A3ED-43A3-9E8C-33EA83C8ED4B", @"A SQL exception has occurred when trying to update workflows");
				return emailIntro;
			}

			protected override string GetLogHeaderForNotificationGroupAboutSQLException()
			{
				var logHeader = (NoResString)@"A SQL exception has occurred when trying to update workflows"; // Logs should not be translated
				return logHeader;
			}

			public void SaveBatchChanges() => FactoryProvider.Save(createNew: false);

			public void AddFetchHintsForBatchProcessing(IReadOnlyCollection<ProcessHeader> workflows)
			{
				foreach (var workflow in workflows)
				{
					workflow.AddDeepFetchHintForParentType();
					ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects(FactoryProvider.Current, workflows.Select(x => x.PK));
					ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects(FactoryProvider.Current, workflows.Select(x => x.FH_FH_ParentHeader));
				}
			}
		}
	}
}

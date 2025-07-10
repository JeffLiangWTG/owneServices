using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class TransferRuleRunner : IPAVEProcessor
	{
		public TransferRuleRunner(
			IPAVESystem system,
			ITransferRuleRunnerDataAccessor dataAccessor,
			ITransferRuleRunnerLogger logger,
			ITransferRuleRunnerParams transferRuleRunnerParams
		)
		{
			System = system;
			DataAccessor = dataAccessor;
			Logger = logger;
			this.transferRuleRunnerParams = transferRuleRunnerParams;
		}

		protected IPAVESystem System { get; }
		IReadOnlyCollection<IComponentLink> linksToProcess;
		protected ITransferRuleRunnerDataAccessor DataAccessor { get; }
		protected ITransferRuleRunnerLogger Logger { get; }
		protected readonly ITransferRuleRunnerParams transferRuleRunnerParams;

#if DEBUG
		public bool HadConcurrencyException_ForTest { get; private set; }
#endif

		List<IComponentLink> skippedLinks;
		WorkflowStartingComponentCache startingComponentCache;
		Guid lastProcessedComponentFrom;
		HashSet<Guid> allWorkflowsWithDedicatedBufferSet;
		HashSet<Guid> workflowsWithDedicatedBufferBeingSetToLink;
		List<Guid> workflowPKsAwaitingTransferOrSettingDedicatedBuffer;

		public void Process(CancellationToken token) => ProcessCore(token);

		protected virtual void ProcessCore(CancellationToken token)
		{
			if (!System.IsLive)
			{
				return;
			}

			linksToProcess = DataAccessor.GetComponentLinks(System).ToArray();

			skippedLinks = new List<IComponentLink>();
			var processor = new LinksProcessorWithDeactivation(linksToProcess,
				DataAccessor,
				shouldUseSecondaryServerIfAllowed: true,
				Logger,
				ProcessFunc,
				OnBeforeProcess,
				link => ShouldProcessLink(link, skippedLinks),
				TransferWorkflowsOrSetDedicatedBuffer,
				ShouldSaveOnProcessedFunc);

			using (ProcessTask.Loader.SuppressTemplateApplication())
			{
				var result = processor.Process(token);
#if DEBUG
				HadConcurrencyException_ForTest = result.HadConcurrencyException;
#endif
			}
		}

		bool ShouldProcessLink(IComponentLink link, List<IComponentLink> skippedLinks)
		{
			var result = ShouldProcessLinkCore(link, skippedLinks);

			if (result)
			{
				if (lastProcessedComponentFrom != link.ComponentFrom)
				{
					lastProcessedComponentFrom = link.ComponentFrom;
					allWorkflowsWithDedicatedBufferSet = new HashSet<Guid>();
				}
			}

			return result;
		}

		bool ShouldProcessLinkCore(IComponentLink link, List<IComponentLink> skippedLinks)
		{
			if (transferRuleRunnerParams.IsResponsive)
			{
				return true; //responsive should process all links
			}

			if (!BMSRegistry.Instance.DynamicallyFilterTransferRules.Value || !link.SkipTransfer)
			{
				return true; // if is not enabled on registry or should not skip transfer link should be processed
			}

			if (!BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.Value || !BMSRegistry.Instance.TransferWorkflowComponentOnChanges.Value)
			{
				Logger.Information($"Component link [{link.DisplayText}]: not skipped because responsive transfer is disabled just as safe guard");
				return true;
			}

			if (BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.Value)
			{
				Logger.Information($"Component link [{link.DisplayText}]: not skipped because ProcessAllTransferRulesLinksOnNextBMSRun is true");
				return true;
			}

			if (!transferRuleRunnerParams.IsCdcEnabled)
			{
				Logger.Information($"Component link [{link.DisplayText}]: not skipped because CDC is not available or disabled");
				return true; //Disable skip if BI CDC is not available or disabled
			}

			skippedLinks?.Add(link);
			return false;
		}

		void OnBeforeProcess()
		{
			startingComponentCache = new WorkflowStartingComponentCache(DataAccessor);
			workflowPKsAwaitingTransferOrSettingDedicatedBuffer = new List<Guid>();
		}

		protected virtual void ProcessFunc(ITransferrableProcessHeader workflow, IComponentLink link)
		{
			if (!link.IsReleaseGateRuleApplied)
			{
				startingComponentCache.UpdateWorkflow(workflow);
			}
			workflowPKsAwaitingTransferOrSettingDedicatedBuffer.Add(workflow.PK);
		}

		#region Transfer Or Setting Dedicated Buffer

		void TransferWorkflowsOrSetDedicatedBuffer(IComponentLink link, IReadOnlyCollection<ITransferrableProcessHeader> workflowBatch)
		{
			if (workflowPKsAwaitingTransferOrSettingDedicatedBuffer.Count == 0)
			{
				return;
			}

			DataAccessor.CreateNewWithoutSave();
			var workflows = DataAccessor.LoadWorkflowsToTransfer(workflowPKsAwaitingTransferOrSettingDedicatedBuffer, link)
				.Where(w => !allWorkflowsWithDedicatedBufferSet.Contains(w.PK))
				.ToArray();

			if (link.IsReleaseGateRuleApplied)
			{
				workflowsWithDedicatedBufferBeingSetToLink = new HashSet<Guid>();
				SetDedicatedBufferOnWorkflows(link, workflows);
			}
			else
			{
				TransferWorkflows(link, workflows);
			}

			try
			{
				SavePendingWorkflows();
			}
			catch (SaveConcurrencyException)
			{
				startingComponentCache.NotifySaveConcurrencyError(workflows);
				throw;
			}
			finally
			{
				workflowPKsAwaitingTransferOrSettingDedicatedBuffer.Clear();
			}

			if (link.IsReleaseGateRuleApplied)
			{
				allWorkflowsWithDedicatedBufferSet.AddRange(workflowsWithDedicatedBufferBeingSetToLink);
			}
			else
			{
				startingComponentCache.UpdateWorkflows(workflows);
			}
			OnTransferWorkflows();
		}

		void TransferWorkflows(IComponentLink link, ITransferrableProcessHeader[] workflows)
		{
			foreach (var workflow in workflows)
			{
				TransferWorkflow(workflow, link);
			}

			Logger.Log($"Component link [{link.DisplayText}]: {pluralizationHelper.Value.GetCountText(workflows.Length)} transferred");
		}

		void TransferWorkflow(ITransferrableProcessHeader workflow, IComponentLink link)
		{
			DataAccessor.TransferWorkflow(workflow, link, transferRuleRunnerParams.IsResponsive);
			workflow.DedicatedBuffer = Guid.Empty;

			if (BMSRegistry.Instance.LogWorkflowInfoOnTransfer.Value)
			{
				Logger.Log($"Moved workflow {GetWorkflowDescription(workflow)} from component {link.GetComponentFromName()} to {link.GetComponentToName()}");
			}
		}

		void SetDedicatedBufferOnWorkflows(IComponentLink link, ITransferrableProcessHeader[] workflows)
		{
			var updatedWorkflowsCount = 0;

			foreach (var workflow in workflows)
			{
				if (SetDedicatedBufferOnWorkflow(workflow, link))
				{
					updatedWorkflowsCount++;
				}
			}
			workflowsWithDedicatedBufferBeingSetToLink.AddRange(workflows.Select(w => w.PK));

			Logger.Log($"Component link [{link.DisplayText}]: calculated dedicated buffer for {pluralizationHelper.Value.GetCountText(workflows.Length)}, updated on {pluralizationHelper.Value.GetCountText(updatedWorkflowsCount)}");
		}

		bool SetDedicatedBufferOnWorkflow(ITransferrableProcessHeader workflow, IComponentLink link)
		{
			var dedicatedBuffer = link.ComponentTo; // it's enough to take ComponentTo from cache without reloading ignoring Uber Factory cache as all the components were already reloaded when getting component links to process

			if (workflow.DedicatedBuffer == dedicatedBuffer)
			{
				return false;
			}

			workflow.DedicatedBuffer = dedicatedBuffer;

			if (BMSRegistry.Instance.LogWorkflowInfoOnTransfer.Value)
			{
				Logger.Log($"Set dedicated buffer {link.GetComponentToName()} on workflow {GetWorkflowDescription(workflow)}");
			}
			return true;
		}

		protected virtual bool ShouldSaveOnProcessedFunc(CancellationToken token)
		{
			if (skippedLinks.Count > 0)
			{
				var skippedString = string.Join(", ", skippedLinks.Select(link => $"[{link.DisplayText}]"));
				Logger.Information($"{skippedLinks.Count} component link(s) were skipped: {skippedString}; they should be processed through Responsive Transfer");
			}

			var wereWorkflowsDeactivated = false;

			foreach (var (workflow, path) in startingComponentCache.GetWorkflowsToDeactivate())
			{
				token.ThrowIfCancellationRequested();

				DataAccessor.DeactivateWorkflow(workflow);
				wereWorkflowsDeactivated = true;
				Logger.Log(LogType.Warning, $"Workflow {GetWorkflowDescription(workflow)} has been deactivated since it has completed a loop of the system {System.Name} via [{path}]. Please address the issue and reactivate the workflow.");
			}

			return wereWorkflowsDeactivated;
		}

		protected virtual void SavePendingWorkflows() => DataAccessor.Save(createNew: true, swallowSaveConcurrencyException: false);

		protected virtual void OnTransferWorkflows() { }

		protected static string GetWorkflowDescription(ITransferrableProcessHeader workflow) => $"{workflow.CompletionStatement} (PK = {workflow.PK}, Job = {GetJobDescription(workflow)})";

		protected static string GetJobDescription(ITransferrableProcessHeader workflow)
		{
			var description = workflow.ParentJobDescription;

			if (string.IsNullOrEmpty(description))
			{
				description = (NoResString)"Unknown";
			}

			return description;
		}

		#endregion

		readonly Lazy<BMPluralizationHelper> pluralizationHelper = new Lazy<BMPluralizationHelper>(() => new BMPluralizationHelper());
	}
}

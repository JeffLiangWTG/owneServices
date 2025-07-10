using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Service
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task logging")]
	public class ResponsiveTransferRuleRunner : TransferRuleRunner
	{
		internal ResponsiveTransferRuleRunner(
			IPAVESystem system,
			ITransferRuleRunnerDataAccessor dataAccessor,
			ITransferRuleRunnerLogger logger,
			IReadOnlyCollection<Guid> transferablesPKs)
			: base(system, dataAccessor, logger, new TransferRuleRunnerParams(responsive: true))
		{
			unupdatedWorkflowPKs = transferablesPKs.ToHashSet();
			Logger.Log(LogType.Debug, $"{pluralizationHelper.Value.GetCountText(transferablesPKs.Count)} to process");
		}

		new ResponsiveTransferRuleRunnerDataAccessor DataAccessor => (ResponsiveTransferRuleRunnerDataAccessor)base.DataAccessor;
		readonly HashSet<Guid> unupdatedWorkflowPKs;

		protected override void ProcessFunc(ITransferrableProcessHeader workflow, IComponentLink link)
		{
			base.ProcessFunc(workflow, link);

			unupdatedWorkflowPKs.Remove(workflow.PK);
		}

		protected override bool ShouldSaveOnProcessedFunc(CancellationToken token)
		{
			var result = base.ShouldSaveOnProcessedFunc(token);

			if (unupdatedWorkflowPKs.Any())
			{
				if (!BMSRegistry.Instance.ResetDedicatedBufferOnWorkflowsNotMatchingComponentLinks.Value)
				{
					Logger.Log(LogType.Debug, "Skipped resetting dedicated buffers as disabled in the registry");
					return result;
				}

				var workflowBatchLoader = DataAccessor.GetUnupdatedWorkflowBatchLoader(unupdatedWorkflowPKs, System.PK);
				IReadOnlyCollection<ITransferrableProcessHeader> workflowBatch;
				var resetWorkflowCount = 0;
				var addedHeaderToLog = false;

				while ((workflowBatch = workflowBatchLoader.LoadNextBatch().ToArray()).Any())
				{
					token.ThrowIfCancellationRequested();

					if (!addedHeaderToLog)
					{
						Logger.Log(@"Resetting Dedicated Buffer for active workflows no longer eligible for release:");
					}
					addedHeaderToLog = true;

					foreach (var workflow in workflowBatch)
					{
						workflow.DedicatedBuffer = Guid.Empty;
						resetWorkflowCount++;

						if (BMSRegistry.Instance.LogWorkflowInfoOnTransfer.Value)
						{
							Logger.Log($"Reset dedicated buffer for workflow {GetWorkflowDescription(workflow)}");
						}
					}
#if DEBUG
					OnNullSettingBatchSaving_ForTest?.Invoke();
#endif
					DataAccessor.Save(createNew: true, swallowSaveConcurrencyException: false);
				}

				if (resetWorkflowCount != 0)
				{
					Logger.Log($"Reset dedicated buffer for {pluralizationHelper.Value.GetCountText(resetWorkflowCount)}");
					return false; // no need to save afterwards - we've already saved
				}
			}

			return result;
		}

		readonly Lazy<IBMPluralizationHelper> pluralizationHelper = new Lazy<IBMPluralizationHelper>(() => ObjectFactory.Get<IBMPluralizationHelper>());

		#region For Tests
#if DEBUG
		public Action OnNullSettingBatchSaving_ForTest { get; set; }
#endif
		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class TransferRuleRunnerDataAccessor : DataAccessor, ITransferRuleRunnerDataAccessor
	{
		public TransferRuleRunnerDataAccessor(ITransferRuleRunnerLogger logger)
			: base(logger, TransferRuleRunnerServiceTask.Code, mustCatchZSaveConcurrencyException: false)
		{
		}

		#region ITransferRuleRunnerDataAccessor Members

		void ITransferRuleRunnerDataAccessor.LogSaveConcurrencyError(string exceptionMessage) => LogZSaveConcurrencyError(exceptionMessage);

		#endregion

		#region Load Component Links to Process

		public IEnumerable<IComponentLink> GetComponentLinks(IPAVESystem system) => GetComponentLinksCore(system);

		protected virtual IEnumerable<IComponentLink> GetComponentLinksCore(IPAVESystem system)
		{
			var linkQuery = new ZDBOnlyQuery(typeof(BMComponentLink));
			linkQuery.AddToFilter(BMComponentLinkSchema.FL_TransferRulesEnabled, true);

			var componentFromSubQuery = new ZDBOnlySubQuery(typeof(BMComponent), BMComponentSchema.PK);
			componentFromSubQuery.AddToFilter(BMComponentSchema.FC_FS_System, system.PK);
			componentFromSubQuery.AddToFilter(BMComponentSchema.FC_IsActive, true);
			linkQuery.AddSubQuery(BMComponentLinkSchema.FL_FC_ComponentFrom, componentFromSubQuery, JoinCondition.And);

			var componentToSubQuery = new ZDBOnlySubQuery(typeof(BMComponent), BMComponentSchema.PK);
			componentToSubQuery.AddToFilter(BMComponentSchema.FC_IsActive, true);
			linkQuery.AddSubQuery(BMComponentLinkSchema.FL_FC_ComponentTo, componentToSubQuery, JoinCondition.And);

			linkQuery.ReLoadExistingRows = true; // ignores factory cache and Uber Factory cache

			var factory = ((BMSystem)system).Factory;
			var links = factory.Load<BMComponentLink>(linkQuery);

			var componentPKs = links.SelectMany<BMComponentLink, ZGuid>(l => [l.FL_FC_ComponentFrom, l.FL_FC_ComponentTo]).Distinct();
			var componentQuery = new ZQuery(BMComponentSchema.PK, componentPKs);
			componentQuery.ReLoadExistingRows = true;
			componentQuery.AllowTableValuedParameters = true;
			factory.Load<BMComponent>(componentQuery);

			var orderedLinks = links
				.OrderBy(l => l.ComponentFrom.FC_DisplaySequence)
				.ThenBy(l => l.FL_Sequence)
				.ThenBy(l => l.ComponentTo.FC_DisplaySequence);

			return orderedLinks;
		}

		#endregion

		#region Load Workflows to Process

		public IWorkflowBatchLoader GetLinkAssociatedWorkflowBatchLoader(IComponentLink componentLink, bool shouldUseSecondaryServerIfAllowed) => GetLinkAssociatedWorkflowBatchLoaderCore(componentLink, shouldUseSecondaryServerIfAllowed);

		protected virtual IWorkflowBatchLoader GetLinkAssociatedWorkflowBatchLoaderCore(IComponentLink componentLink, bool shouldUseSecondaryServerIfAllowed)
		{
			return new LinkAssociatedWorkflowBatchLoader(this, componentLink, Logger, () => GetLinkAssociatedWorkflowQuery(componentLink), shouldUseSecondaryServerIfAllowed);
		}

		#region Workflow Query

		protected (ZQuery query, string queryName) GetLinkAssociatedWorkflowQuery(IComponentLink link)
		{
			var componentLink = (BMComponentLink)link;
			var query = GetLinkAssociatedWorkflowQueryCore(componentLink);
			query.AddToFilter(ProcessHeaderSchema.FH_IsActive, true);

			var queryName = GetWorkflowQueryNameCore(componentLink);
			return (query, queryName);
		}

		protected virtual ZQuery GetLinkAssociatedWorkflowQueryCore(BMComponentLink link)
		{
			var config = new WorkflowLoadConfig();
			var source = new WorkflowComponentsData(Array.Empty<ZQuery>(), WorkflowLoader.GetStmModuleFilterQuery(link.FilterRule, true), new[] { link.FL_FC_ComponentFrom });
			return WorkflowLoader.GetQuery(config, source);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Command")]
		protected virtual string GetWorkflowQueryNameCore(BMComponentLink link)
		{
			var systemFrom = link.ComponentFrom.System.FS_Name.ToString();
			var systemTo = link.ComponentTo.System.FS_Name.ToString();
			var systemDescription = systemFrom == systemTo ? systemFrom : FormattableString.Invariant($"{systemFrom}/{systemTo}");
			var queryType = "Transfer Rules";
			var queryName = FormattableString.Invariant($@"
-- Database:  {Db.DatabaseName}
-- System(s): {systemDescription}
-- Link:      {link.DisplayText}
-- Type:	  {queryType}");

			return queryName;
		}

		#endregion

		#region Batch Size

		public int GetWorkflowBatchSize() => GetWorkflowBatchSizeCore();

		protected virtual int GetWorkflowBatchSizeCore() => BMSRegistry.Instance.TransferRuleBatchSize.Value;

		#endregion

		#region Workflow Fetch Hints

		public void AddFetchHintsForLinkAssociatedWorkflowProcessing(IReadOnlyCollection<ITransferrableProcessHeader> workflowBatch, IComponentLink link) => AddFetchHintsForLinkAssociatedWorkflowProcessingCore(workflowBatch, link);

		protected virtual void AddFetchHintsForLinkAssociatedWorkflowProcessingCore(IReadOnlyCollection<ITransferrableProcessHeader> workflowBatch, IComponentLink link)
		{
			var componentLink = (BMComponentLink)link;
			Current.ImportFromAnotherFactory(componentLink.ComponentFrom); // when updating a workflow on WorkflowStartingComponentCache CurrentComponent (ComponentFrom) is loaded
		}

		#endregion

		#endregion

		#region Transfer Workflows

		public IEnumerable<ITransferrableProcessHeader> LoadWorkflowsToTransfer(IEnumerable<Guid> workflowPKs, IComponentLink link)
		{
			var additionalFilter = new ZQuery(ProcessHeaderSchema.FH_FC_CurrentComponent, link.ComponentFrom);
			var pks = workflowPKs.Select(pk => (ZGuid)pk).ToArray();
			var workflows = LoadWorkflowsToTransferCore(pks, additionalFilter).WhereNotNull().ToArray();
			ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects(Current, pks, shouldForceSeek: true);
			AddFetchHintsForSavingWorkflows(workflows, link);

			if (workflows.Length > 0)
			{
				var factory = workflows[0].Factory;
				// when current component or dedicated buffer is changed on workflows, we reload component data needed to update effective branch and department from db,
				// but there is no need to do that in transfer rule runner as it reloads actual data from db before processing (see GetComponentLinksCore)
				ProcessHeader.SuppressReloadingComponentsFromDbOnSave(factory);
				ProcessHeader.SuppressUpdatingWorkflowStatusesOnSave(factory); // transfer rule runner is not supposed to change workflow statuses
			}
			return workflows.Where(w => !w.IsDeleted).ToArray();
		}

		protected virtual ProcessHeader[] LoadWorkflowsToTransferCore(ZGuid[] pks, ZQuery additionalFilter)
		{
			return WorkflowLoader.LoadWorkflowsByPKUsingTableValuedParameter(Current, pks, additionalFilter);
		}

		protected void AddFetchHintsForSavingWorkflows(ProcessHeader[] workflows, IComponentLink link = null)
		{
#if DEBUG
			AddingFetchHintsForWorkflowsToTransfer_ForTest?.Invoke(this, workflows);
#endif

			foreach (var workflow in workflows)
			{
				if (workflow.IsDeleted)
				{
					ReportDeletedWorkflow(workflow);
					continue;
				}

				workflow.AddDeepFetchHintForParentType();
				workflow.AddTagLinksFetchHint();
				workflow.JobHeader.AddTagLinksFetchHint();
				Current.AddFetchHint(typeof(BMComponent), workflow.FH_FC_CurrentComponent);
			}
			ImportComponentsNeededForSettingsDedicatedBuffer(link);
		}

		void ImportComponentsNeededForSettingsDedicatedBuffer(IComponentLink link)
		{
			if (link != null)
			{
				var componentLink = (BMComponentLink)link;
				ImportComponentFromLinkFactoryIfNeeded(componentLink.FL_FC_ComponentFrom, componentLink.ComponentFrom); // we load CurrentComponent (ComponentFrom) when call UpdateEffectiveBranchAndDepartment on setting dedicated buffers
				ImportComponentFromLinkFactoryIfNeeded(componentLink.FL_FC_ComponentTo, componentLink.ComponentTo); // we load DedicatedBuffer (ComponentTo) when call UpdateEffectiveBranchAndDepartment on setting dedicated buffers
			}
		}

		void ImportComponentFromLinkFactoryIfNeeded(ZGuid componentPK, BMComponent component)
		{
			var componentQuery = new ZQuery(BMComponentSchema.PK, componentPK);
			componentQuery.FetchOnlyFromLocalCache = true;
			var loadedComponent = Current.Load<BMComponent>(componentQuery).SingleOrDefault();
			if (loadedComponent == null)
			{
				Current.ImportFromAnotherFactory(component);
			}
		}

#if DEBUG
		public event EventHandler<ProcessHeader[]> AddingFetchHintsForWorkflowsToTransfer_ForTest;
#endif

		static void ReportDeletedWorkflow(ProcessHeader workflow)
		{
			var builder = new ZStringBuilder((NoResString)"Trying to add fetch hints to a deleted workflows."); // Diagnostic information
			var callstack_message = workflow.DeletionCallstack ?? (NoResString)"Call stack not yet available, either because ReportWorkflowDeletionCallstackWhenLoadingWorkflowsToTransfer isn't enabled in the registry, or because Delete() wasn't actually called on the ProcessHeader. This registry item has now been turned on for this customer system, so you should see the actual call stack here in the next occurrence."; // Diagnostic infomation

			builder.Append($"PK: {workflow.PK}");   // Diagnostic information
			builder.Append((NoResString)"Deletion callstack:");  // Diagnostic information
			builder.Append(callstack_message);

			ErrorReporter.ReportOnce($"{nameof(TransferRuleRunnerDataAccessor)}:AddingFetchHintsToDeletedWorkflows", builder.ToStringWithNewLineBetweenAppends());

			EnableCallStackReportingInRegistrySoErrorsAreMoreHelpfulNextTimeThisHappens();
		}

		static void EnableCallStackReportingInRegistrySoErrorsAreMoreHelpfulNextTimeThisHappens()
		{
			if (!BMSRegistry.Instance.ReportWorkflowDeletionCallstackWhenLoadingWorkflowsToTransfer.Value)
			{
				BMSRegistry.Instance.ReportWorkflowDeletionCallstackWhenLoadingWorkflowsToTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}
		}

		public void TransferWorkflow(ITransferrableProcessHeader workflow, IComponentLink link, bool isResponsive)
		{
			var header = (ProcessHeader)workflow;
			header.TransferTo((BMComponentLink)link, isResponsive);
		}

		#endregion

		#region Deactivate Workflows

		public IEnumerable<ITransferrableProcessHeader> LoadWorkflowsToDeactivate(IEnumerable<Guid> transferredWorkflowPKs)
		{
			var pks = transferredWorkflowPKs.Select(pk => (ZGuid)pk).ToArray();
			var workflows = WorkflowLoader.LoadWorkflowsByPKUsingTableValuedParameter(Current, pks)
				.WhereNotNull()
				.Where(w => !w.IsDeleted)
				.ToArray();

			ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects(Current, pks, shouldForceSeek: true);
			AddFetchHintsForSavingWorkflows(workflows);

			if (workflows.Length > 0)
			{
				ProcessHeader.SuppressUpdatingWorkflowStatusesOnSave(workflows[0].Factory); // transfer rule runner is not supposed to change workflow statuses
			}

			return workflows;
		}

		public void DeactivateWorkflow(ITransferrableProcessHeader workflow)
		{
			((ProcessHeader)workflow).FH_IsActive = false;
		}

		#endregion

		#region Deactivate Component Links

		public IEnumerable<IComponentLink> LoadComponentLinksToDeactivate(IEnumerable<Guid> componentLinkPKs)
		{
			return Current.Load<BMComponentLink>(new ZQuery(BMComponentLinkSchema.PK, componentLinkPKs)).OrderBy(l => l.FL_Sequence);
		}

		public void DeactivateComponentLink(IComponentLink link)
		{
			((BMComponentLink)link).FL_TransferRulesEnabled = false;
		}

		#endregion

		#region Service Task Schedule Branch

		public IDisposable GetTemporaryEnvironmentForServiceTaskBranch()
		{
			return GetTemporaryEnvironmentForServiceTaskBranchCore();
		}

		protected virtual IDisposable GetTemporaryEnvironmentForServiceTaskBranchCore()
		{
			return Helper.GetTemporaryEnvironmentForServiceTaskBranch();
		}

		BMSServiceTaskHelper Helper => helper ?? (helper = new BMSServiceTaskHelper());
		BMSServiceTaskHelper helper;

		#endregion

		#region For Testing
#if DEBUG

		public BusinessObjectFactory CurrentFactory_ExposedForTest => Current;

		public (ZQuery query, string queryName) GetWorkflowQuery_ExposedForTest(IComponentLink link) => GetLinkAssociatedWorkflowQuery(link);

		internal void NotifyBatchLoaded_ForTest(ref ITransferrableProcessHeader[] workflowBatch, ref ITransferrableProcessHeader lastProcessHeaderRead)
		{
			OnBatchLoaded_ForTest(ref workflowBatch, ref lastProcessHeaderRead);
		}

		protected virtual void OnBatchLoaded_ForTest(ref ITransferrableProcessHeader[] workflowBatch, ref ITransferrableProcessHeader lastProcessHeaderRead)
		{
		}

#endif
		#endregion

	}
}

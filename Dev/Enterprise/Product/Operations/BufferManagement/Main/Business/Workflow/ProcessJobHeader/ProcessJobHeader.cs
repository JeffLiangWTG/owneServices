using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Service.Client;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessJobHeader : ProcessHeader,
		IProcessJobHeader,
		IUniversalCopySelectivelySupportable
	{
		public ProcessJobHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Fetch hints

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ProcessJobHeaderFetchStrategy(this);
		}

		class ProcessJobHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			public ProcessJobHeaderFetchStrategy(ProcessJobHeader processJobHeader)
				: base(processJobHeader)
			{
				this.processJobHeader = processJobHeader;
			}

			readonly ProcessJobHeader processJobHeader;

			protected override void FetchForBindCore()
			{
				base.FetchForBindCore();

				if (!processJobHeader.FH_ParentTableCode.IsEmpty)
				{
					processJobHeader.AddDeepFetchHintForParentType(Factory);
					Factory.AddFetchHint(ProcessTasksSchema.Instance, processJobHeader.GetBaseTasksQuery());
				}
			}
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			FH_Category = BMConstants.JobLevelWorkflowCategoryCode;
		}

		#region Load/Create

		public static ProcessJobHeader GetForParentWithoutCreation(IWorkflowProvider parent, BusinessObjectFactory factory, bool shouldRecheckDatabase = false)
		{
			Argument.NotNull(parent, nameof(parent));

			var bizo = parent as BusinessObject
				?? throw new ArgumentException("Only IWorkflowProviders that are BusinessObjects can be linked to a ProcessJobHeader.", nameof(parent));

			var query = GetJobHeaderQuery(parent);
			var result = factory.LoadTop1<ProcessJobHeader>(query);

			if (shouldRecheckDatabase && result == null && bizo.IsInDatabase)
			{
				query.FetchOnlyFromLocalCache = false;
				query.ReLoadExistingRows = true;
				factory.ClearQueryCache(ProcessHeaderSchema.Constants.TableName);

				result = factory.LoadTop1<ProcessJobHeader>(query);
			}

			return result;
		}

		static IEnumerable<ProcessJobHeader> GetAllForParentWithoutCreation(IWorkflowProvider parent, BusinessObjectFactory factory)
		{
			Argument.NotNull(parent, nameof(parent));

			var bizo = parent as BusinessObject
				?? throw new ArgumentException("Only IWorkflowProviders that are BusinessObjects can be linked to a ProcessJobHeader.", nameof(parent));

			var query = GetJobHeaderQuery(parent, forceLoadFromDatabase: true);

			return factory.Load<ProcessJobHeader>(query);
		}

		public static ZQuery GetJobHeaderQuery(IWorkflowProvider parent, bool forceLoadFromDatabase = false)
		{
			var bizo = (BusinessObject)parent;
			var isInDatabase = bizo.IsInDatabase;

			var query = GetProcessHeadersInJobQuery(parent.PK, bizo.TablePrefix);
			query.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, null);
			query.FetchOnlyFromLocalCache = !isInDatabase;
			query.ReLoadExistingRows = isInDatabase && (forceLoadFromDatabase || WorkflowDataRegistry.Instance.AlwaysCheckForExistingJobLevelWorkflowsInTheDatabaseWhenCreatingJobLevelWorkflows.Value);

			return query;
		}

		public ZQuery GetAllProcessHeadersInJobQuery(bool allowFetchOnlyFromLocalCacheIfNotInDatabase = true)
		{
			var query = FH_ParentId.IsValid
				? GetProcessHeadersInJobQuery(FH_ParentId, FH_ParentTableCode)
				: new ZQuery(); // For workflow templates which don't have a value in FH_ParentId

			if (allowFetchOnlyFromLocalCacheIfNotInDatabase && !IsInDatabase)
			{
				query.FetchOnlyFromLocalCache = true;
			}

			return query;
		}

		static ZQuery GetProcessHeadersInJobQuery(ZGuid parentId, string parentTableCode)
		{
			var query = new ZQuery(ProcessHeaderSchema.FH_ParentId, parentId);
			query.AddToFilter(ProcessHeaderSchema.FH_ParentTableCode, parentTableCode);

			return query;
		}

		public static ProcessJobHeader GetForParent(IWorkflowProvider parent, BusinessObjectFactory factory, bool addDefaultProcessHeaderIfNone = true, bool checkTemplates = true, bool shouldRecheckDatabase = false)
		{
			if (parent is ProcessHeader)
			{
				ErrorReporter.ReportOnce("Parents should not be workflows", "A parent which is a workflow means we have flowception");
			}

			var result = GetForParentWithoutCreation(parent, factory, shouldRecheckDatabase);

			var businessObject = (BusinessObject)parent;

			if (result == null && checkTemplates && businessObject.HasChanges)
			{
				var templates = ProcessTask.Loader.LoadTemplateMatches(parent, factory);
				if (templates.Any())
				{
					var bestTemplate = templates.First();
					if (bestTemplate.GetJobHeader() != null)
					{
						result = GetForTemplate(bestTemplate, parent);
					}
				}
			}

			if (result == null)
			{
				if (parent is BusinessObject bizo && bizo.IsDeleted)
				{
					ErrorReporter.ReportOnce("ProcessJobHeader.Initialise:Deleted", $@"Tried to add/initialise a job header on a job that has been deleted. That's madness.

Job type: {parent.GetType().Name}
Job name/code: {bizo.HumanReadableName}
PK: {bizo.PK}
Is in database: {bizo.IsInDatabase}");
					return null;
				}
				else
				{
					result = factory.New<ProcessJobHeader>();
					Initialise(result, parent);
				}
			}
			else
			{
				result.Parent = parent;
			}

			if (addDefaultProcessHeaderIfNone && BMSRegistryProvider.IsBufferManagementEnabled)
			{
				result.AddDefaultProcessHeaderIfNone();
			}

			businessObject.RegisterEditableChildObject(result);

			return result;
		}

		internal static ProcessJobHeader GetForTemplate(ProcessTaskTemplate template, IWorkflowProvider workflowProvider)
		{
			var result = GetForParentWithoutCreation(workflowProvider, template.Factory, shouldRecheckDatabase: true);
			var templateJobHeader = template.GetJobHeader();

			if (result == null)
			{
				var parentBizo = (BusinessObject)workflowProvider;

				if (templateJobHeader != null)
				{
					result = (ProcessJobHeader)templateJobHeader.Clone(CreateTemplateJobHeaderCloneArgs());

					Initialise(result, workflowProvider);
					parentBizo.RegisterEditableChildObject(result);
				}
				else
				{
					ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "ProcessTaskTemplate.GetJobHeader returned null.\r\nTemplate: {0}, Workflow Provider: {1}", template.HumanReadableName, parentBizo.HumanReadableName));

					result = GetForParent(workflowProvider, template.Factory, addDefaultProcessHeaderIfNone: false, checkTemplates: false);
				}
			}
			else if (templateJobHeader != null && result.ProcessHeaders.Count == 0)
			{
				result.CopyPersistentValuesFrom(templateJobHeader, CreateTemplateJobHeaderCloneArgs());
			}

			if (templateJobHeader != null)
			{
				if (result.FH_ParentTemplateId.IsEmpty)
				{
					result.FH_ParentTemplateId = templateJobHeader.PK;
				}

				if (!templateJobHeader.FH_GG_ReleaseGroup.IsEmpty && result.FH_GG_ReleaseGroup.IsEmpty)
				{
					result.FH_GG_ReleaseGroup = templateJobHeader.FH_GG_ReleaseGroup;
				}
			}

			return result;
		}

		static BusinessObjectCloneArgs CreateTemplateJobHeaderCloneArgs()
		{
			return new BusinessObjectCloneArgs(new[]
			{
				ProcessHeaderSchema.FH_GG_ReleaseGroup.Name,
				ProcessHeaderSchema.FH_IsReleaseGroupSetByTemplate.Name,
				ProcessHeaderSchema.FH_P0_Template.Name,
				ProcessHeaderSchema.FH_ParentId.Name,
				ProcessHeaderSchema.FH_ParentTableCode.Name,
				ProcessHeaderSchema.FH_ParentTemplateId.Name,
				ProcessHeaderSchema.FH_WorkflowType.Name,
			});
		}

		static void Initialise(ProcessJobHeader jobHeader, IWorkflowProvider workflowProvider)
		{
			var workflowProviderBizo = (BusinessObject)workflowProvider;

			jobHeader.Parent = workflowProvider;

			using (jobHeader.SuspendSettingHasChanges())
			{
				jobHeader.FH_ParentId = workflowProviderBizo.PK;
				jobHeader.FH_ParentTableCode = workflowProviderBizo.TablePrefix;
				jobHeader.FH_WorkflowType = workflowProvider.WorkflowType;

				if (jobHeader.FH_WorkflowType.IsEmpty)
				{
					jobHeader.FH_WorkflowType = BMConstants.WorkflowTypeNotFoundCode;
				}

				if (jobHeader.FH_CompletionStatement.IsEmpty)
				{
					jobHeader.FH_CompletionStatement = Res.GetString("7a28127f-1fd4-4a9f-ba9f-f837d7282f57", "Job {0} is complete.", workflowProviderBizo.HumanReadableName);
				}
			}
		}

		internal ProcessHeader AddDefaultProcessHeader()
		{
			using (SuspendSettingHasChanges())
			{
				var defaultWorkflow = Factory.New<ProcessHeader>();

				using (defaultWorkflow.SuspendSettingHasChanges())
				{
					defaultWorkflow.FH_CompletionStatement = BMGlobalConstants.DefaultWorkflowCompletionStatement;
					defaultWorkflow.FH_ParentTemplateId = BMGlobalConstants.DefaultWorkflowTemplateID;
					ProcessHeaders.SetDefaultsForNewWorkflow(defaultWorkflow);

					foreach (ProcessTask task in Parent.WorkflowItems.Tasks)
					{
						if (task.P9_FH_ProcessHeader.IsEmpty)
						{
							using (task.SuspendSettingHasChanges())
							{
								task.P9_FH_ProcessHeader = defaultWorkflow.PK;
							}
						}
					}
				}
				return defaultWorkflow;
			}
		}

		ProcessHeader AddDefaultProcessHeaderIfNone(bool forceCheckDB = false)
		{
			if (forceCheckDB)
			{
				var query = new ZQuery(ProcessHeaderSchema.FH_FH_ParentHeader, PK) { ReLoadExistingRows = true };

				if (Factory.Exists(typeof(ProcessJobHeader), query))
				{
					return null;
				}
			}
			else if (GetWorkflowsEvenIfListChangedEventsDelayed().Any())
			{
				return null;
			}

			return AddDefaultProcessHeader();
		}

		internal ICollection<ProcessHeader> GetWorkflowsEvenIfListChangedEventsDelayed()
		{
			if (Factory.AreCollectionListChangedEventsDelayed)
			{
				return Factory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.FH_FH_ParentHeader, PK));
			}
			else
			{
				return ProcessHeaders;
			}
		}

		#endregion

		#region ApplyTemplate

		public void ApplyTemplate(IProcessTaskTemplate template, IWorkflowTemplateApplicationParameters parameters = null)
		{
			ApplyTemplate(template, w => w.IsWorkflow, parameters);
		}

		public IEnumerable<IProcessHeader> ApplyTemplate(IProcessTaskTemplate template, Predicate<IProcessHeader> templateProcessHeaderPredicate, IWorkflowTemplateApplicationParameters parameters = null)
		{
			var concreteTemplate = (ProcessTaskTemplate)template;
			var workflowsAdded = AddWorkflowsFromTemplate(concreteTemplate, this, templateProcessHeaderPredicate, parameters);

			AddWorkflowLinksFromTemplate(concreteTemplate, this, templateProcessHeaderPredicate);

			return workflowsAdded;
		}

		public IProcessHeader ApplyTemplateWorkflow(IProcessTaskTemplate template, IProcessHeader templateWorkflow, IWorkflowTemplateApplicationParameters parameters = null)
		{
			var workflowsAdded = ApplyTemplate(template, w => w.PK == templateWorkflow.PK, parameters);

			return workflowsAdded.SingleOrDefault();
		}

		public event EventHandler OnBeforeAddWorkflowsFromTemplate;

		#region AddWorkflowsFromTemplate

		IEnumerable<IProcessHeader> AddWorkflowsFromTemplate(ProcessTaskTemplate template, ProcessJobHeader jobHeader, Predicate<ProcessHeader> templateProcessHeaderPredicate, IWorkflowTemplateApplicationParameters parameters)
		{
			var result = new List<ProcessHeader>();
			var templateWorkflows = template.GetWorkflows().ToArray();
			var templateJobHeader = template.GetJobHeader();
			var initialProcessHeaderCount = jobHeader.ProcessHeaders.Count;

			OnBeforeAddWorkflowsFromTemplate?.Invoke(this, EventArgs.Empty);

			if (parameters?.ReapplyProcessHeaders == true && jobHeader.ProcessHeaders.Any(p => !p.IsInDatabase))
			{
				ErrorReporter.ReportOnce("It shouldn't be possible to reapply process headers more than once without saving as order is required for and dependancies and process tasks to the correct header");
			}

			if (initialProcessHeaderCount == 0 && templateWorkflows.Length == 0)
			{
				if (jobHeader.IsInDatabase)
				{
					jobHeader.ProcessHeaders.UpdateFromDb();
				}

				if (jobHeader.ProcessHeaders.Count == 0)
				{
					jobHeader.AddDefaultProcessHeaderIfNone(forceCheckDB: true);
				}
			}

			var workflowsToAdd =
				(from ProcessHeader workflow in templateWorkflows
				 where ShouldAddTemplateWorkflow(workflow, template, jobHeader, templateProcessHeaderPredicate, parameters)
				 select workflow).ToArray();

			if (workflowsToAdd.Any() && jobHeader.IsInDatabase)
			{
				jobHeader.ProcessHeaders.UpdateFromDb();
				workflowsToAdd =
						(from ProcessHeader workflow in templateWorkflows
						 where ShouldAddTemplateWorkflow(workflow, template, jobHeader, templateProcessHeaderPredicate, parameters)
						 select workflow).ToArray();
			}

			var processHeadersForTagFetchHints = templateJobHeader != null ? workflowsToAdd.Append(templateJobHeader).ToArray() : workflowsToAdd;

			AddTagFetchHints(template.Factory, processHeadersForTagFetchHints);

			using (jobHeader.SetWorkflowTemplateApplicationMode())
			{
				foreach (var templateWorkflow in workflowsToAdd)
				{
					var completionStatement = templateWorkflow.FH_CompletionStatement;
					if (parameters != null && parameters.ReapplyProcessHeaders)
					{
						completionStatement = ProcessHeaderCompletionStatementSequenceHandler.GetUniqueCompletionStatementForReappliedWorkflowTemplate(jobHeader.ProcessHeaders, templateWorkflow.FH_CompletionStatement);
					}

					var workflow = jobHeader.ProcessHeaders.AddNew();
					workflow.FH_CompletionStatement = completionStatement;
					workflow.FH_AllowTaskAutoAssignment = templateWorkflow.FH_AllowTaskAutoAssignment;
					workflow.FH_IsCriticalHandover = templateWorkflow.FH_IsCriticalHandover;
					workflow.FH_IsStandby = templateWorkflow.FH_IsStandby;
					workflow.FH_DateAcceptability = templateWorkflow.FH_DateAcceptability;
					workflow.FH_ParentTemplateId = templateWorkflow.PK;
					workflow.FH_AgreedDeliveryDateDefaultsFrom = templateWorkflow.FH_AgreedDeliveryDateDefaultsFrom;
					workflow.FH_AgreedDeliveryDateDefaultHoursOffset = templateWorkflow.FH_AgreedDeliveryDateDefaultHoursOffset;
					workflow.FH_EarliestStartDateDefaultsFrom = templateWorkflow.FH_EarliestStartDateDefaultsFrom;
					workflow.FH_EarliestStartDefaultHoursOffset = templateWorkflow.FH_EarliestStartDefaultHoursOffset;
					workflow.FH_Category = templateWorkflow.FH_Category;
					workflow.FH_IsApproved = templateWorkflow.FH_IsApproved;
					workflow.FH_DeadlineType = templateWorkflow.FH_DeadlineType;
					workflow.FH_GB_Branch = templateWorkflow.FH_GB_Branch;
					workflow.FH_GE_Department = templateWorkflow.FH_GE_Department;
					workflow.FH_BMT_BufferTimespan = templateWorkflow.FH_BMT_BufferTimespan;
					workflow.FH_MilestoneCompletionPivotKey = templateWorkflow.FH_MilestoneCompletionPivotKey;

					if (!templateWorkflow.FH_GG_ReleaseGroup.IsEmpty)
					{
						workflow.FH_GG_ReleaseGroup = templateWorkflow.FH_GG_ReleaseGroup;
					}
					if (workflow.FH_GG_ReleaseGroup.IsEmpty)
					{
						if (!templateJobHeader.FH_GG_ReleaseGroup.IsEmpty)
						{
							workflow.FH_GG_ReleaseGroup = templateJobHeader.FH_GG_ReleaseGroup;
						}
					}

					CloneTagLinks(templateWorkflow, workflow);

					result.Add(workflow);
				}
			}

			if (templateJobHeader != null)
			{
				CloneTagLinks(templateJobHeader, jobHeader);
			}

			return result;
		}

		public IDisposable SetWorkflowTemplateApplicationMode()
		{
			templateIsBeingAppliedCount++;
			return new DisposableAction(() => templateIsBeingAppliedCount--);
		}

		int templateIsBeingAppliedCount;

		internal bool IsTemplateBeingApplied => templateIsBeingAppliedCount > 0;

		static bool ShouldAddTemplateWorkflow(ProcessHeader templateWorkflow, ProcessTaskTemplate template, ProcessJobHeader jobHeader, Predicate<ProcessHeader> templateProcessHeaderPredicate, IWorkflowTemplateApplicationParameters parameters)
		{
			return templateProcessHeaderPredicate(templateWorkflow)
				&& !HasMatchingProcessHeaderThatBlocksTemplateApplicaiton(templateWorkflow, template, jobHeader, parameters)
				&& HasTasksOrDependencyLinks(templateWorkflow, template, jobHeader.Parent);
		}

		static bool HasTasksOrDependencyLinks(ProcessHeader templateWorkflow, ProcessTaskTemplate template, IWorkflowProvider workflowProvider)
		{
			return HasTasks(templateWorkflow, template, workflowProvider) || HasLinks(templateWorkflow, template);
		}

		static bool HasTasks(ProcessHeader templateWorkflow, ProcessTaskTemplate template, IWorkflowProvider workflowProvider)
		{
			return workflowProvider.WorkflowItems.Tasks.GetItemsToCreateFromTemplate(template).Any(t => t.P9_FH_ProcessHeader == templateWorkflow.PK);
		}

		static bool HasLinks(ProcessHeader templateWorkflow, ProcessTaskTemplate template)
		{
			return template.ProcessHeaderLinks.Cast<IProcessHeaderLink>().Any(l => l.FP_FH_HeaderFrom == templateWorkflow.PK || l.FP_FH_HeaderTo == templateWorkflow.PK);
		}

		static bool HasMatchingProcessHeaderThatBlocksTemplateApplicaiton(ProcessHeader templateProcessHeader, ProcessTaskTemplate template, ProcessJobHeader jobHeader, IWorkflowTemplateApplicationParameters parameters)
		{
			if (parameters != null && parameters.ReapplyProcessHeaders)
			{
				return false;
			}

			return jobHeader.ProcessHeaders.Any(potentialMatchingProcessHeader =>
			{
				if (potentialMatchingProcessHeader.FH_CompletionStatement == templateProcessHeader.FH_CompletionStatement)
				{
					return true;
				}

				var sourceTemplateProcessHeader = potentialMatchingProcessHeader.SourceTemplateProcessHeader;
				if (sourceTemplateProcessHeader != null)
				{
					return sourceTemplateProcessHeader.FH_CompletionStatement == templateProcessHeader.FH_CompletionStatement;
				}

				return false;
			});
		}

		static void CloneTagLinks(ProcessHeader templateWorkflow, ProcessHeader workflow)
		{
			foreach (var link in templateWorkflow.TagLinks)
			{
				var magnitude = link.TagMagnitude;

				if (magnitude != null)
				{
					magnitude.Factory.AddFetchHint(TagDefinitionSchema.PK, magnitude.TGM_TGD_Tag);
				}
			}

			var argsToCloneTagLinksWith = new TagLinkCloneArgs(workflow.PK, true, true, new[] { TagLinkSchema.TGL_ParentId.Name });
			templateWorkflow.TagLinks
				.Where(l => ShouldApplyTag(workflow, l))
				.ForEach(oldTagLink => ((TagLink)oldTagLink).Clone(argsToCloneTagLinksWith));
		}

		static bool ShouldApplyTag(ProcessHeader workflow, ITagLink templateTagLink)
		{
			var magnitude = templateTagLink.TagMagnitude;

			return magnitude != null
				&& !workflow.IsTagApplied(magnitude)
				&& (!magnitude.TagDefinition.TGD_IsExclusive || !workflow.TagLinks.Any(l => l.TagMagnitude.TGM_TGD_Tag == templateTagLink.TagMagnitude.TGM_TGD_Tag));
		}

		static void AddTagFetchHints(BusinessObjectFactory factory, ProcessHeader[] workflows)
		{
			foreach (var templateWorkflow in workflows)
			{
				factory.AddFetchHint(TagLinkSchema.TGL_ParentId, templateWorkflow.PK);
			}

			// Pre-loading all tag links so that magnitudes are loaded in one hit via TagLinkRowFetchStrategy

			var query = new ZQuery(TagLinkSchema.TGL_ParentId, workflows.Select(w => w.PK));
			factory.Load<TagLink>(query);
		}

		#endregion

		#region AddWorkflowLinksFromTemplate

		static void AddWorkflowLinksFromTemplate(ProcessTaskTemplate template, ProcessJobHeader jobHeader, Predicate<ProcessHeader> templateProcessHeaderPredicate)
		{
			var utcNow = ZDateTime.UtcNow;
			var templateWorkflows = template.ProcessHeaders.Cast<ProcessHeader>().Where(w => templateProcessHeaderPredicate(w)).ToArray();
			var templateLinksApplied = new HashSet<ProcessHeaderLink>();
			var orderedWorkflows = jobHeader.ProcessHeaders.OrderByDescending(w => w.IsInDatabase ? w.FH_SystemCreateTimeUtc : utcNow);

			ProcessHeaderLink.LoadLinksIntoFactoryOptimisingForUnsavedProcessHeaders(jobHeader.Factory, templateWorkflows, shouldForceSeek: true);

			foreach (var templateHeader in templateWorkflows)
			{
				var links = templateHeader.Links_ForBinding.ToArray();

				if (links.Length > 0)
				{
					bool HasMatchingCompletionStatementWithTemplateHeader(ProcessHeader templateHeader, ProcessHeader currentHeader)
					{
						return currentHeader.FH_CompletionStatement == templateHeader.FH_CompletionStatement || (currentHeader.SourceTemplateProcessHeader != null && currentHeader.SourceTemplateProcessHeader.FH_CompletionStatement == templateHeader.FH_CompletionStatement);
					}

					var header = orderedWorkflows.FirstOrDefault(x => HasMatchingCompletionStatementWithTemplateHeader(templateHeader, x));

					if (header != null)
					{
						foreach (var templateLink in links)
						{
							if (templateLink.HeaderFrom != null
							&& templateLink.HeaderTo != null
							&& templateLink.HeaderFrom.FH_CompletionStatement != templateLink.HeaderTo.FH_CompletionStatement
							&& !templateLinksApplied.Contains(templateLink))
							{
								templateLinksApplied.Add(templateLink);

								var fromHeader = orderedWorkflows.FirstOrDefault(x => HasMatchingCompletionStatementWithTemplateHeader(templateLink.HeaderFrom, x));
								var toHeader = orderedWorkflows.FirstOrDefault(x => HasMatchingCompletionStatementWithTemplateHeader(templateLink.HeaderTo, x));

								if (toHeader != null && fromHeader != null
									&& (!toHeader.IsInDatabase || !fromHeader.IsInDatabase)
									&& !fromHeader.LinksFromMeToOthers.Any(x => x.FP_FH_HeaderTo == toHeader.PK && x.FP_LinkType == templateLink.FP_LinkType))
								{
									ProcessHeaderLink link;

									using (ActiveBusinessObjectCollection.DelayListChangedEvents(header.Factory))
									{
										link = header.Factory.New<ProcessHeaderLink>();
										link.FP_FH_HeaderFrom = fromHeader.PK;
										link.FP_FH_HeaderTo = toHeader.PK;
									}

									link.FP_LinkType = templateLink.FP_LinkType;
									link.FP_TimeDelayFactor = templateLink.FP_TimeDelayFactor;
									link.FP_TimeDelayMinutes = templateLink.FP_TimeDelayMinutes;
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#endregion

		#region On Save

		protected override void ScheduleActionForTransfer()
		{
			var pks = ProcessHeaders.Where(w => !w.FH_DoNotStartBeforeDate.IsValid).Select(w => w.PK).ToArray();

			if (pks.Length == 0)
			{
				return;
			}

			var pksJson = pks.JsonSerialize();
			ActionScheduleProvider.ScheduleAction(TransferRuleSchedulerAction.Code, FH_DoNotStartBeforeDate, PK, TablePrefix, pksJson);
		}

		public static void ReloadJobRelatedTasksFromDbOnSaveIfNeeded(ProcessHeader processHeader)
		{
			var jobsWithReloadedTasksPKs = GetJobsWithReloadedTasksPKs(processHeader);

			if (processHeader.IsInDatabase && WorkflowDataRegistry.Instance.EnhancedWorkflowStatusUpdateMode.Value && !jobsWithReloadedTasksPKs.Contains(processHeader.FH_ParentId))
			{
				var taskQueryFetchOnlyFromLocalCache = new ZQuery() { FetchOnlyFromLocalCache = true };
				var tasks = processHeader.Factory.Load<ProcessTask>(taskQueryFetchOnlyFromLocalCache);
				var changedTasksPKs = tasks.Where(t => t.HasChanges).Select(t => t.PK);

				var taskQuery = GetTasksQuery(processHeader);
				var reloadFromDBQuery = new ZQuery(taskQuery) { ReLoadExistingRows = true, AllowTableValuedParameters = true };
				reloadFromDBQuery.AddToFilter(ProcessTasksSchema.PK, SQLComparisonOperator.NotEqual, changedTasksPKs);
				processHeader.Factory.Load<ProcessTask>(reloadFromDBQuery);

				jobsWithReloadedTasksPKs.Add(processHeader.FH_ParentId);
			}
		}

		internal static HashSet<ZGuid> GetJobsWithReloadedTasksPKs(ProcessHeader processHeader) => processHeader.Factory.GetCachedValue(nameof(GetJobsWithReloadedTasksPKs), () => new HashSet<ZGuid>(), CacheStalenessPolicy.StaleOnFactorySave);

		public static void ReloadJobRelatedLinksFromDbOnSaveIfNeeded(ProcessHeader processHeader)
		{
			var jobsWithReloadedLinksPKs = GetJobsWithReloadedLinksPKs(processHeader);

			if (processHeader.IsInDatabase && WorkflowDataRegistry.Instance.EnhancedWorkflowStatusUpdateMode.Value && !jobsWithReloadedLinksPKs.Contains(processHeader.FH_ParentId))
			{
				var linkQueryFetchOnlyFromLocalCache = new ZQuery() { FetchOnlyFromLocalCache = true };
				var links = processHeader.Factory.Load<ProcessHeaderLink>(linkQueryFetchOnlyFromLocalCache);
				var changedLinksPKs = links.Where(l => l.HasChanges).Select(t => t.PK);

				var workflowSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
				workflowSubQuery.AddToFilter(GetProcessHeadersInJobQuery(processHeader.FH_ParentId, processHeader.FH_ParentTableCode));

				var linkQuery = new ZDBOnlyQuery(typeof(ProcessHeaderLink));
				linkQuery.AddSubQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, workflowSubQuery, JoinCondition.And);
				linkQuery.AddSubQuery(ProcessHeaderLinkSchema.FP_FH_HeaderTo, workflowSubQuery, JoinCondition.Or);

				var reloadFromDBQuery = new ZQuery(linkQuery) { ReLoadExistingRows = true, AllowTableValuedParameters = true };
				reloadFromDBQuery.AddToFilter(ProcessHeaderLinkSchema.PK, SQLComparisonOperator.NotEqual, changedLinksPKs);
				processHeader.Factory.Load<ProcessHeaderLink>(reloadFromDBQuery);

				jobsWithReloadedLinksPKs.Add(processHeader.FH_ParentId);
			}
		}

		internal static HashSet<ZGuid> GetJobsWithReloadedLinksPKs(ProcessHeader processHeader) => processHeader.Factory.GetCachedValue(nameof(GetJobsWithReloadedLinksPKs), () => new HashSet<ZGuid>(), CacheStalenessPolicy.StaleOnFactorySave);

		#endregion

		#region Workflow Status

		public void RefreshWorkflowsStatus()
		{
			WorkflowStatusUpdater.UpdateWorkflowStatuses(ProcessHeaders);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal void SetWorkflowsLastEditTimeForStatusChecking()
		{
			var workflows = Factory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.FH_ParentId, FH_ParentId) { FetchOnlyFromLocalCache = true }).ToDictionary(x => x.PK);

			var sql = FormattableString.Invariant($@"
SELECT {ProcessHeaderSchema.Constants.PK} PK,
       {ProcessHeaderSchema.Constants.FH_SystemLastEditTimeUtc} LastEditTimeForWorkflowStatus
FROM   {ProcessHeaderSchema.Constants.SqlSchemaName}.{ProcessHeaderSchema.Constants.TableName}
WHERE  {ProcessHeaderSchema.Constants.PK} IN (SELECT Value FROM @workflowPks)");  // SQL command

			var command = Db.Connection.Command(sql); // We don't want to use a factory because that would load the business objects entirely.
			command.AddTableValuedParameter("@workflowPks", ProcessHeaderSchema.PK, workflows.Keys);

			using (var reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
			{
				while (reader.Read())
				{
					if (workflows.TryGetValue(reader.GetGuid(0), out var workflow))
					{
						workflow.LastEditTimeFromDatabaseForStatusCheck = new ZDateTime(reader.GetDateTime(1));
					}
				}
			}
		}

		#endregion

		#region Related Business Objects

		#region Tasks

		public override IEnumerable<ProcessTask> Tasks
		{
			get { return base.Tasks.Union(ProcessHeaders.SelectMany(ph => ph.Tasks)); }
		}

		internal override ZQuery GetTasksQuery() => GetTasksQuery(this);

		// returns the query to load all tasks in the job without the need to load ProcessJobHeader
		internal static ZQuery GetTasksQuery(ProcessHeader processHeader)
		{
			var query = GetBaseTasksQuery(processHeader);
			query.AddToFilter(ProcessTask.GetNonTasksExclusionQuery());

			return query;
		}

		internal ZQuery GetBaseTasksQuery() => GetBaseTasksQuery(this);

		internal static ZQuery GetBaseTasksQuery(ProcessHeader processHeader)
		{
			var query = new ZQuery(ProcessTasksSchema.P9_ParentID, processHeader.FH_ParentId) { FetchOnlyFromLocalCache = !processHeader.IsInDatabase };
			query.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, processHeader.FH_ParentTableCode);
			return query;
		}

		#endregion

		#region ProcessHeaders

		public override ProcessJobHeader JobHeader
		{
			get { return this; }
		}

		[ChildEditable]
		[ChildEditableTestExclude]
		public ProcessHeaderCollection ProcessHeaders
		{
			get
			{
				if (processHeaders == null)
				{
					processHeaders = ProcessHeaderCollection.GetCollectionForJobNotIncludingJobLevelWorkflow(this);
					(Parent as BusinessObject)?.RegisterEditableChildObject(processHeaders);
				}

				return processHeaders;
			}
		}

		ProcessHeaderCollection processHeaders;

		ProcessHeaderCollection NonChildEditableProcessHeaders
		{
			get
			{
				if (nonChildEditableProcessHeaders == null)
				{
					nonChildEditableProcessHeaders = ProcessHeaderCollection.GetCollectionForJobNotIncludingJobLevelWorkflow(this);
				}

				return nonChildEditableProcessHeaders;
			}
		}

		ProcessHeaderCollection nonChildEditableProcessHeaders;

		public JobDependencyGraph DependencyGraph
		{
			get { return dependencyGraph ?? (dependencyGraph = JobDependencyGraph.Create(this)); }
		}

		JobDependencyGraph dependencyGraph;

		#endregion

		#endregion

		#region Existing Properties

		#region FH_TimeDelayMinutes

		[ReadOnly(false)]
		public override ZInt FH_TimeDelayMinutes
		{
			get { return base.FH_TimeDelayMinutes; }
			set { base.FH_TimeDelayMinutes = value; }
		}

		[ZDateTimeDurationValueCalculatedFromMinutes]
		[ResourceStringData("ProcessJobHeader|StaggeredReleaseTimeDelay", ShortCaption = "Delay Time", Caption = "Release Delay Time", FullDescription = "The number of hours/minutes from release of previous workflows that need to be elapsed when determining a workflow’s Staggered Release Delay Expiry.")]
		public ZDateTime StaggeredReleaseTimeDelay
		{
			get { return FH_TimeDelayMinutes.GetDateTimeFromMinutes(); }
			set
			{
				FH_TimeDelayMinutes = (ZInt)value.GetMinutesFromDateTimeSpan();
				StaggeredReleaseTimeDelayInfo.RefreshBinding();
				FH_TimeDelayMinutesInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo StaggeredReleaseTimeDelayInfo
		{
			get { return GetZPropertyInfo(nameof(StaggeredReleaseTimeDelay)); }
		}

		#endregion

		#region FH_TimeDelayFactor

		[ReadOnly(false)]
		public override ZDecimal FH_TimeDelayFactor
		{
			get { return base.FH_TimeDelayFactor; }
			set { base.FH_TimeDelayFactor = value; }
		}

		#endregion

		#region FH_FC_CurrentComponent

		protected override bool FH_FC_CurrentComponent_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region IsReleased

		[ResourceStringData("ProcessJobHeader.IsReleased", Caption = "Released", FullDescription = "Indicates whether any workflow in this job is currently in a buffer component.")]
		public override ZBool IsReleased
		{
			get { return ProcessHeaders.Any(w => w.IsReleased); }
		}

		#endregion

		#region FH_CompletionStatement

		public override ZString FH_CompletionStatement
		{
			get { return base.FH_CompletionStatement; }
			set
			{
				base.FH_CompletionStatement = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateFH_CompletionStatement();
				}
			}
		}

		#endregion

		#region FH_PlannedDurationInMinutes

		[BusinessObjectTestExclude] // This field is maintained when saving the bizo - edits in test won't have an effect
		public override ZInt FH_PlannedDurationInMinutes
		{
			get { return base.FH_PlannedDurationInMinutes; }
			set { base.FH_PlannedDurationInMinutes = value; }
		}

		protected internal override int GetPlannedDuration()
		{
			return ProcessHeaders.Where(w => !w.IsChildWorkflowOfAnotherWorkflowWithinJob()).Sum(w => w.FH_PlannedDurationInMinutes);
		}

		#endregion

		#region FH_AgreedDeliveryDate

		public override ZDateTime FH_AgreedDeliveryDate
		{
			get { return base.FH_AgreedDeliveryDate; }
			set
			{
				base.FH_AgreedDeliveryDate = value;

				if (!IsValidationSuspended)
				{
					foreach (ProcessHeader workflow in ProcessHeaders)
					{
						workflow.Validation.ValidateFH_DateAcceptability();
					}
				}

				UpdateEffectiveAgreedDeliveryDate();
			}
		}

		public override void UpdateEffectiveAgreedDeliveryDate()
		{
			foreach (ProcessHeader workflow in ProcessHeaders)
			{
				workflow.UpdateEffectiveAgreedDeliveryDate();
			}
		}

		public override void UpdateReleaseSequenceSortDate()
		{
			foreach (ProcessHeader workflow in ProcessHeaders)
			{
				workflow.UpdateReleaseSequenceSortDate();
			}
		}

		#endregion

		#region FH_P0_Template

		public override ZGuid FH_P0_Template
		{
			get => base.FH_P0_Template;
			set
			{
				base.FH_P0_Template = value;

				if (!IsInDatabase)
				{
					templateWorkflowConstructionStack = new StackTrace();
				}
			}
		}

		StackTrace templateWorkflowConstructionStack;

		#endregion

		#region FH_ParentId

		public override ZGuid FH_ParentId
		{
			get => base.FH_ParentId;
			set
			{
				base.FH_ParentId = value;

				if (!IsInDatabase)
				{
					parentIdSettingStack = new StackTrace();
				}
			}
		}

		StackTrace parentIdSettingStack;

		#endregion

		#region FH_IsApproved

		HashSet<ProcessJobHeader> GetIsApprovedCascadingJobHeaders(bool value)
		{
			var headers = new HashSet<ProcessJobHeader>();
			void PopulateCascadingJobHeaders(ProcessJobHeader header)
			{
				if (FH_IsApproved != value && headers.Add(header))
				{
					foreach (var child in header.ChildLinks.Select(link => link.HeaderFrom).OfType<ProcessJobHeader>())
					{
						PopulateCascadingJobHeaders(child);
					}
				}
			}

			PopulateCascadingJobHeaders(this);

			return headers;
		}

		void SetApprovedFlagAndCascadeToContainedWorkflows(bool value)
		{
			base.FH_IsApproved = value;

			foreach (var workflow in ProcessHeaders)
			{
				workflow.FH_IsApproved = value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:Do Not Show Message Box From Business Layer", Justification = "Baseline")]
		void SetApprovedFlag(bool value)
		{
			var cascadingJobHeaders = GetIsApprovedCascadingJobHeaders(value);

			// Let the user know if it will cascade to too many children. But only if they have visibility in the first place.
			if (cascadingJobHeaders.Count >= 10 && BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.Value)
			{
				var result = Globals.Message.Show(
					Res.GetString("92602178-E80D-44B9-A2F4-83E0CD72D3E9", "This will set (or reset) the Approved flag for {0} job-level workflows.", cascadingJobHeaders.Count),
					Res.GetString("59420822-0050-468E-A853-77560A10B5AB", "Confirm Cascading Approved Flag"),
					ZMessageBoxButtons.OKCancel,
					ZMessageBoxIcon.Warning,
					ZDialogResult.OK);

				if (result != ZDialogResult.OK)
				{
					return;
				}
			}

			foreach (var jobHeader in cascadingJobHeaders)
			{
				jobHeader.SetApprovedFlagAndCascadeToContainedWorkflows(value);
			}
		}

		public override ZBool FH_IsApproved
		{
			get => base.FH_IsApproved;
			set => SetApprovedFlag(value);
		}

		protected override bool FH_IsApproved_ReadOnly => false;

		#endregion

		#region FH_Category

		protected override bool FH_Category_ReadOnly => true;

		#endregion

		#region FH_GB_Branch And FH_GE_Department

		public override void UpdateEffectiveBranch()
		{
			foreach (var workflow in ProcessHeaders)
			{
				workflow.UpdateEffectiveBranch();
			}
		}

		public override void UpdateEffectiveDepartment()
		{
			foreach (var workflow in ProcessHeaders)
			{
				workflow.UpdateEffectiveDepartment();
			}
		}

		#endregion

		#region FH_ISActive

		[BusinessObjectTestExclude]
		public override ZBool FH_IsActive
		{
			get { return true; }
		}

		protected override bool FH_IsActive_ReadOnly => true;

		#endregion

		#region CanCancel

		public override string CanCancel() => Res.GetString("4f874180-875d-4ff1-b261-8124b8bb061f", "Job '{0}' cannot be deactivated.", FH_JobDescription);

		#endregion

		#endregion

		#region New Properties

		public override ZDecimal TotalRelevantEstimatedHours
		{
			get { return ProcessHeaders.Sum(h => h.TotalRelevantEstimatedHours); }
		}

		[ResourceStringData("ProcessJobHeader.TotalEstimatedHoursSummary", ShortCaption = "Estimate Summary", Caption = "Low to High Estimate Summary (including children)", FullDescription = "The total estimate range of the tasks on this workflow and child workflows.")]
		public override ZString TotalEstimatedHoursSummary
		{
			get
			{
				if (Parent != null)
				{
					var totalLowEstimate = (ZDecimal)Parent.WorkflowItems.Tasks.Cast<ProcessTask>().Sum(x => x.LowEstimatedDurationHours);
					var totalHighEstimate = (ZDecimal)Parent.WorkflowItems.Tasks.Cast<ProcessTask>().Sum(x => x.HighEstimatedDurationHours);

					return Res.GetString("4fcdf80c-7b90-4e7f-9117-e7e984e309a6", "{0} hrs to {1} hrs.", totalLowEstimate.ToString(1), totalHighEstimate.ToString(1));
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo TotalEstimatedHoursSummaryInfo
		{
			get { return GetZPropertyInfo(nameof(TotalEstimatedHoursSummary)); }
		}

		[ResourceStringData("ProcessJobHeader.OverallEstimateFactor", ShortCaption = "Estimate Factor", Caption = "Overall Estimate Variation Factor", FullDescription = "The overall factor of low to high estimates for this workflow.")]
		public override ZDecimal OverallEstimateFactor
		{
			get
			{
				if (Parent != null)
				{
					var totalLowEstimate = Parent.WorkflowItems.Tasks.Cast<ProcessTask>().Sum(x => x.LowEstimatedDurationHours);
					var totalHighEstimate = Parent.WorkflowItems.Tasks.Cast<ProcessTask>().Sum(x => x.HighEstimatedDurationHours);

					if (totalHighEstimate > 0 && totalLowEstimate > 0)
					{
						return totalHighEstimate / totalLowEstimate;
					}
				}

				return ZDecimal.Zero;
			}
		}

		public ZPropertyInfo OverallEstimateFactorInfo
		{
			get { return GetZPropertyInfo(nameof(OverallEstimateFactor)); }
		}

		public override ZString ProcessHeaderType => FH_P0_Template.IsValid ? ProcessHeaderTypeList.Descriptions.TemplateJob : ProcessHeaderTypeList.Descriptions.Job;

		[ResourceStringData("ProcessJobHeader.IsOpen", Caption = "Open", FullDescription = "Indicates whether this job has any open workflows or children.")]
		public override ZBool IsOpen
		{
			get { return base.IsOpen; }
		}

		internal override bool GetIsOpen(HashSet<ZGuid> existing = null)
		{
			HashSet<ZGuid> currentExisting = existing == null ? new HashSet<ZGuid>() : new HashSet<ZGuid>(existing);

			if (!currentExisting.Add(PK))
			{
				return false;
			}
			else
			{
				return ProcessHeaders.Concat(ChildLinks.Select(l => l.HeaderFrom).WhereNotNull()).Any(h => h.GetIsOpen(currentExisting));
			}
		}

		public override ZString ApplicableDateAcceptability
		{
			get { return FH_DateAcceptability; }
		}

		protected override IEnumerable<ProcessTask> GetRelevantTasksForEstimates()
		{
			return Factory.Load<ProcessTask>(GetTasksQuery());
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			ProcessHeaders.DeleteAll();

			if (!IsDeleted)
			{
				var nonLinkedProcessHeadersQuery = new ZQuery(ProcessHeaderSchema.FH_FH_ParentHeader, PK);
				nonLinkedProcessHeadersQuery.AddToFilter(ProcessHeaderSchema.FH_ParentId, null);

				foreach (var result in Factory.Load<ProcessHeader>(nonLinkedProcessHeadersQuery))
				{
					result.Delete();
				}
			}

			base.Delete();
		}

		#endregion

		#region Defer

		protected override void DeferCore(BMComponent startingComponent, ZDateTime earliestStartDate, string reason = "")
		{
			foreach (ProcessHeader processHeader in ProcessHeaders)
			{
				processHeader.Defer(startingComponent, earliestStartDate, reason);
			}
		}

		#endregion

		#region Promote / Demote

		public override void Promote(ProcessJobHeader jobHeader, bool cloneLinks = true, bool deleteOldTasks = true)
		{
			throw new InvalidOperationException("Cannot promote a ProcessJobHeader.");
		}

		public ProcessHeader Demote(IWorkflowProvider provider, BusinessObjectFactory factory)
		{
			return Demote(ProcessJobHeader.GetForParent(provider, factory));
		}

		public ProcessHeader Demote(ProcessJobHeader newJobHeader)
		{
			var newWorkflow = newJobHeader.ProcessHeaders.AddNew();
			newWorkflow.FH_CompletionStatement = GetDemotedWorkflowCompletionStatement();

			var workflows = ProcessHeaders.ToArray();
			var workflowsIncludingJob = workflows.Union(new[] { newJobHeader }).ToArray();

			foreach (var task in TaskCollection.Cast<ProcessTask>().ToArray())
			{
				task.P9_FH_ProcessHeader = newWorkflow.PK;
				task.P9_ParentID = newJobHeader.FH_ParentId;
				task.P9_ParentTableCode = newJobHeader.FH_ParentTableCode;
			}

			var externalPrereqLinkDestinations = Links.Where(l => !workflowsIncludingJob.Any(w => w.PK == l.FP_FH_HeaderFrom))
				.Select(l => Tuple.Create(l.FP_FH_HeaderFrom, l.FP_LinkType)).Distinct().ToArray();
			var externalPostreqLinkDestinations = Links.Where(l => !workflowsIncludingJob.Any(w => w.PK == l.FP_FH_HeaderTo))
				.Select(l => Tuple.Create(l.FP_FH_HeaderTo, l.FP_LinkType)).Distinct().ToArray();

			foreach (var destination in externalPrereqLinkDestinations)
			{
				var link = newWorkflow.LinksFromOthersToMe_ForBinding.AddNew();
				link.FP_FH_HeaderFrom = destination.Item1;
				link.FP_LinkType = destination.Item2;
				link.Validation.ValidateAll();

				if (link.HasErrors)
				{
					link.Delete();
				}
			}

			foreach (var destination in externalPostreqLinkDestinations)
			{
				var link = newWorkflow.LinksFromMeToOthers_ForBinding.AddNew();
				link.FP_FH_HeaderTo = destination.Item1;
				link.FP_LinkType = destination.Item2;
				link.Validation.ValidateAll();

				if (link.HasErrors)
				{
					link.Delete();
				}
			}

			Links.DeleteAll();
			ProcessHeaders.DeleteAll();

			if (Parent != null)
			{
				Parent.WorkflowItems.RefreshBinding();
			}

			TaskCollection.RefreshBinding();

			return newWorkflow;
		}

		string GetDemotedWorkflowCompletionStatement()
		{
			var completionStatement = new StringBuilder(ProviderJobNumber);
			if (completionStatement.Length > 0)
			{
				completionStatement.Append(": ");
			}
			var description = ProviderJobDescription;
			if (!string.IsNullOrWhiteSpace(description))
			{
				completionStatement.Append(description);
			}
			else
			{
				completionStatement.Append(FH_CompletionStatement);
			}

			return completionStatement.ToString();
		}

		#endregion

		#region Validation

		protected override ProcessHeaderValidation GetNewValidation()
		{
			return new ProcessJobHeaderValidation(this);
		}

		#endregion

		#region FloatCalculator

		internal ProcessHeaderFloatCalculator FloatCalculator
		{
			get
			{
				if (floatCalculator == null || Factory.CacheVersion > cacheVersion)
				{
					floatCalculator = ProcessHeaderFloatCalculator.CreateCalculator(this);
					cacheVersion = Factory.CacheVersion;
				}

				return floatCalculator;
			}
		}

		int cacheVersion;
		ProcessHeaderFloatCalculator floatCalculator;

		#endregion

		#region Nudge

		protected override ZDecimal DirectNudge => (ZDecimal)FH_VoteUpDownAmount;

		public override void UpdateEffectiveNudge()
		{
			foreach (var header in ProcessHeaders)
			{
				header.UpdateEffectiveNudge();
			}
		}

		#endregion

		#region BusinessObject Overrides

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (dependencyGraph != null)
			{
				dependencyGraph = null;
			}

			if (FH_ParentId.IsValid && Parent is BusinessObject bo && bo.HasChanges && BMSRegistry.Instance.EnableSynchronousPAVEDataProcessing.Value)
			{
				var responsivePKs = ProcessHeaders.Select(w => w.PK.ToGuid()).ToArray();
				if (responsivePKs.Any())
				{
					var logger = new BufferManagementLogger();
					SchematicService.ProcessTransferRules(responsivePKs, logger);
				}
			}
		}

		ISchematicService schematicService;
		ISchematicService SchematicService => schematicService ?? (schematicService = ObjectFactory.Get<IPAVEServiceClientFactory>().GetSchematicServiceClient());

		public override bool IsSavedByFactory
		{
			get
			{
				var isSaved = base.IsSavedByFactory;

				if (isSaved && !IsDeleted && FH_P0_Template.IsValid)
				{
					var template = Template;

					if (template != null)
					{
						return !template.IsNonPersistant; // Terrible hack caused by WI00024918
					}
				}
				return isSaved;
			}
		}

		#endregion

		#region UniqueIndexFailureHandler

		public class OnDeletingToHandleDuplicateWorkflowEventArgs
		{
			public OnDeletingToHandleDuplicateWorkflowEventArgs(ProcessJobHeader processJobHeaderDeleted, ProcessJobHeader processJobHeaderInDatabase)
			{
				ProcessJobHeaderDeleted = processJobHeaderDeleted;
				ProcessJobHeaderInDatabase = processJobHeaderInDatabase;
			}

			public ProcessJobHeader ProcessJobHeaderDeleted { get; private set; }

			public ProcessJobHeader ProcessJobHeaderInDatabase { get; private set; }
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new ProcessJobHeaderUniqueIndexFailureHandler(this); }
		}

		public event EventHandler<OnDeletingToHandleDuplicateWorkflowEventArgs> OnDeletingToHandleDuplicateWorkflow;

		class ProcessJobHeaderUniqueIndexFailureHandler : ProcessHeaderUniqueIndexFailureHandler
		{
			internal ProcessJobHeaderUniqueIndexFailureHandler(ProcessJobHeader jobHeader)
			{
				this.jobHeader = jobHeader;
			}

			readonly ProcessJobHeader jobHeader;

			protected override IEnumerable<string> HandledUniqueIndexNamesCore => base.HandledUniqueIndexNamesCore.Append(ProcessHeaderSchema.Constants.Indexes.NR_UX__FH_P0_Template);

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Diagnostic information")]
			protected override void NotifyUserAndAttemptToResolveCore(INotificationHandler notifier, string indexName)
			{
				ProcessJobHeader[] jobHeaders = null;
				string stringToReport = null;

				if (indexName == ProcessHeaderSchema.Constants.Indexes.NR_UX__FH_P0_Template)
				{
					stringToReport = "Duplicate job-level workflow created:" + System.Environment.NewLine + jobHeader.templateWorkflowConstructionStack.ToString();
				}
				else if (indexName == ProcessHeaderSchema.Constants.Indexes.NR_UX__FH_ParentId_FH_ParentTableCode)
				{
					var builder = new ZStringBuilder((NoResString)"Duplicate job-level workflow created.");
					jobHeaders = GetAllForParentWithoutCreation(jobHeader.Parent, jobHeader.Factory).ToArray();

					foreach (var header in jobHeaders)
					{
						builder.Append($@"
Description: {header.FH_CompletionStatement}
Parent Table Code: {header.FH_ParentTableCode}
Is in database: {header.IsInDatabase}
{header.parentIdSettingStack}");
					}

					stringToReport = builder.ToStringWithNewLineBetweenAppends();
				}

				if (!AttemptToResolveDuplicateWorkflow(jobHeaders))
				{
					if (stringToReport != null)
					{
						ErrorReporter.ReportOnce(stringToReport);
					}

					base.NotifyUserAndAttemptToResolveCore(notifier, indexName);
				}
			}

			bool AttemptToResolveDuplicateWorkflow(ProcessJobHeader[] jobHeaders)
			{
				if (jobHeader.Parent == null)
				{
					return false;
				}

				if (jobHeaders == null)
				{
					jobHeaders = GetAllForParentWithoutCreation(jobHeader.Parent, jobHeader.Factory).ToArray();
				}

				if (jobHeaders.Length == 0)
				{
					return false;
				}

				var jobHeadersNotInDatabase = jobHeaders.Where(jh => !jh.IsInDatabase);
				var jobHeadersInDatabase = jobHeaders.Where(jh => jh.IsInDatabase);

				// if any of the conditions below are true then something else went wrong when checking for duplicate JLWs
				if (jobHeadersNotInDatabase.Count() != 1 || jobHeadersInDatabase.Count() != 1)
				{
					return false;
				}

				var notInDatabase = jobHeadersNotInDatabase.Single();
				var inDatabase = jobHeadersInDatabase.Single();

				if (notInDatabase.ProcessHeaders.Count > 0)
				{
					var duplicateWorkflowPrompt = Res.GetString("eb3de466-4e02-443d-b11f-cb333e707b9c", "A user attempted to create and save new workflows that would conflict with existing workflows. The workflows that failed to save were:");
					duplicateWorkflowPrompt += (System.Environment.NewLine) + (System.Environment.NewLine);

					foreach (ProcessHeader p in notInDatabase.ProcessHeaders)
					{
						duplicateWorkflowPrompt += (p.FH_CompletionStatement + System.Environment.NewLine);
					}
					inDatabase.WorkflowNote.ST_NoteData = ORtfTextUtil.ConcatRtfString(inDatabase.WorkflowNote.ST_NoteData, duplicateWorkflowPrompt);
				}

				notInDatabase.OnDeletingToHandleDuplicateWorkflow?.Invoke(this, new OnDeletingToHandleDuplicateWorkflowEventArgs(notInDatabase, inDatabase));
				notInDatabase.Delete();

				return true;
			}
		}

		#endregion

		#region IProcessJobHeader Members

		IProcessHeaderCollection IProcessJobHeader.ProcessHeaders
		{
			get { return ProcessHeaders; }
		}

		void IProcessJobHeader.ApplyReleaseGroupRules()
		{
			var workflowsWereAlreadyLoaded = HaveWorkflowsAlreadyBeenLoaded();
			var recordsUpdated = new List<ProcessHeader>();
			var determinator = new ReleaseGroupDeterminator(Parent);

			AddReleaseGroupRulesFetchHints();

			var existingReleaseGroup = FH_GG_ReleaseGroup;
			var newJobLevelReleaseGroup = Lazy.Create(() => determinator.DetermineReleaseGroup(this));
			if (MaybeChangeReleaseGroup(newJobLevelReleaseGroup, workflowsWereAlreadyLoaded))
			{
				recordsUpdated.Add(this);
			}

			if (FH_GG_ReleaseGroup.IsEmpty || FH_GG_ReleaseGroup != existingReleaseGroup)
			{
				foreach (ProcessHeader workflow in ProcessHeaders)
				{
					var newWorkflowReleaseGroup = Lazy.Create(() => determinator.DetermineReleaseGroup(workflow));
					if (workflow.MaybeChangeReleaseGroup(newWorkflowReleaseGroup, workflowsWereAlreadyLoaded))
					{
						recordsUpdated.Add(workflow);
					}
				}
			}

			if (!workflowsWereAlreadyLoaded)
			{
				ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects(Factory, recordsUpdated.Select(w => w.PK), relationshipDirection: RelationshipDirection.From); // Load all links in advance of updating parent workflow last edit times.
			}
		}

		bool HaveWorkflowsAlreadyBeenLoaded()
		{
			var query = ProcessHeaders.CompleteFilter;
			query.FetchOnlyFromLocalCache = true;

			return Factory.Load<ProcessHeader>(query).Any();
		}

		void AddReleaseGroupRulesFetchHints()
		{
			var templateVersionPKs = FH_ParentTemplateId.WrapWithEnumerable().Concat(ProcessHeaders.Select(ph => ph.FH_ParentTemplateId)).Where(g => g.IsValid).Distinct().ToArray();

			if (templateVersionPKs.Any())
			{
				Factory.AddFetchHint(ProcessHeaderSchema.Instance, new ZQuery(ProcessHeaderSchema.PK, templateVersionPKs));
			}
		}

		IProcessHeader IProcessJobHeader.CreateQualityIterationWorkflow(IProcessHeader triggerWorkflow, string iterationType)
		{
			var processHeader = (ProcessHeader)triggerWorkflow;
			var cloneArgs = new BusinessObjectCloneArgs(new[] { nameof(processHeader.FH_P0_Template), nameof(processHeader.FH_ParentTemplateId) });
			var qualityIterationWorkflow = (ProcessHeader)processHeader.Clone(cloneArgs);

			qualityIterationWorkflow.FH_CompletionStatement = GetQualityIterationCompletionStatement(processHeader, iterationType);
			qualityIterationWorkflow.GetOrCreateLinkToParent(processHeader);
			qualityIterationWorkflow.FH_FC_CurrentComponent = processHeader.FH_FC_CurrentComponent;
			qualityIterationWorkflow.SynchroniseBufferPenetration = ZBool.True;
			qualityIterationWorkflow.FH_Category = processHeader.FH_Category;

			return qualityIterationWorkflow;
		}

		string GetQualityIterationCompletionStatement(IProcessHeader triggerWorkflow, string iterationType)
		{
			iterationType = GetIterationType(iterationType);
			var triggerCompletionStatementWithoutSuffix = Regex.Replace(triggerWorkflow.FH_CompletionStatement, string.Format(CultureInfo.InvariantCulture, @" \({0}\s?\d*\).*", Regex.Escape(iterationType)), string.Empty);
			var existingCompletionStatements = ProcessHeaders.Select(x => x.FH_CompletionStatement.ToString()).ToArray();
			var iterationCount = 1;
			var completionStatement = string.Empty;

			while (iterationCount == 1 || existingCompletionStatements.Contains(completionStatement))
			{
				completionStatement = string.Format(CultureInfo.InvariantCulture, "{0} ({1} {2})", triggerCompletionStatementWithoutSuffix, iterationType, iterationCount++);
			}

			return completionStatement;
		}

		static string GetIterationType(string iterationType)
		{
			return string.IsNullOrEmpty(iterationType) ? DefaultIterationType : iterationType;
		}

		static string DefaultIterationType => Res.GetString("12044f5b-672a-4ac7-8bcb-7aa0f2aa29af", "Quality Iteration");

		#endregion

		#region IProposedNetworkEntity

		protected override bool IsStartable()
		{
			return ProcessHeaders.Any(x => ((IProposedNetworkEntity)x).IsStartable);
		}

		protected override ProcessTask CreateNewCompletionStatementTask()
		{
			var workflow = ProcessHeaders.OrderBy(w => w.Sequence).LastOrDefault()
			 ?? AddDefaultProcessHeader();
			return workflow.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
		}

		#endregion

		#region ILinkEntity

		public override IEnumerable<ProcessHeader> ChildHeaders
		{
			get
			{
				foreach (ProcessHeader workflow in NonChildEditableProcessHeaders)
				{
					if (!workflow.ParentLinks.Any(IsLinkToWorkflowWithinJob))
					{
						yield return workflow;
					}
				}

				foreach (ProcessHeader workflowFromChildLinks in base.ChildHeaders)
				{
					yield return workflowFromChildLinks;
				}
			}
		}

		bool IsLinkToWorkflowWithinJob(ProcessHeaderLink link)
		{
			return link.FP_FH_HeaderTo != PK && link.HeaderTo != null && link.HeaderTo.FH_ParentId == FH_ParentId;
		}

		public override IEnumerable<ProcessHeader> ParentHeaders
		{
			get { return ParentLinks.Select(s => s.HeaderTo).WhereNotNull(); }
		}

		#endregion

		#region IUniversalCopySelectivelySupportable Members

		bool IUniversalCopySelectivelySupportable.SupportsUniversalCopy => false;

		string IUniversalCopySelectivelySupportable.ReasonForNotSupportingUniversalCopy => Res.GetString("0ecfb06f-7a6e-463a-9e33-87fc9e29e4d3", "There can only be one job-level workflow per job.");

		#endregion
	}
}

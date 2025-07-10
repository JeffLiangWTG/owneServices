using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[CodeProperty("Code"), DescriptionProperty("Description")]
	[UniversalCopyWithExtendedEntities(FinishCopyMethod = "FinishUniversalCopy")]
	[UniversalCopyIgnoreElement(nameof(FH_FH_ParentHeader), nameof(FH_WorkflowType), nameof(FH_ParentTableCode), nameof(FH_ParentId), nameof(FH_P0_Template))]
	public class ProcessHeader : AutoProcessHeader,
		IProcessHeader,
		IWorkflow,
		IWorkflowTriggerEventSource,
		IWorkflowOrderable,
		ITransferrableProcessHeader,
		IProposedNetworkEntity,
		ITagable,
		ILinkEntity,
		IBufferedItem,
		IChildBufferedItem,
		IDateAcceptability,
		IDefaultedFromDateProvider,
		IDataVersionLoggingSupported
	{
		public ProcessHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(FH_BufferPenetrationPercentWhenCompleted), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(FH_ReleaseDateTime), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(FH_FC_CurrentComponent), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(FH_PlannedDurationInMinutes), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(FH_StaggeredReleaseDelayExpiry), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(FH_LastTransferType), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(FH_FC_DedicatedBuffer), ConcurrencyPolicy.Observe);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(FH_EffectiveAgreedDeliveryDateUtc), ConcurrencyPolicy.Observe);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(FH_LatestAcceptableReleaseDateUtc), ConcurrencyPolicy.Observe);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(FH_ReleaseSequenceSortDateUtc), ConcurrencyPolicy.Observe);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(FH_GB_EffectiveBranch), ConcurrencyPolicy.Observe);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(FH_GE_EffectiveDepartment), ConcurrencyPolicy.Observe);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(FH_EffectiveNudge), ConcurrencyPolicy.Ignore); // we'll calculate FH_EffectiveNudge solely through CDC subscribers, so, whoever calculates the last get the most up-to-date value; this value will not be updated in UI
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(FH_TaskLowestOpenSequenceNumber), ConcurrencyPolicy.Ignore);
			ProcessHeaderOnSavingService.HookupFactory(Factory);
		}

		#region TypeDecider

		class ProcessHeaderTypeDecider : TypeDecider
		{
			public override Type GetTypeForBinding()
			{
				return typeof(ProcessHeader);
			}

			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				return row[ProcessHeaderSchema.FH_FH_ParentHeader.Name] != DBNull.Value ? typeof(ProcessHeader) : typeof(ProcessJobHeader);
			}

			public override Type GetTypeForNew()
			{
				return typeof(ProcessHeader);
			}
		}

		public static readonly TypeDecider TypeDecider = new ProcessHeaderTypeDecider();

		#endregion

		#region Fetch hints

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ProcessHeaderFetchStrategy(this);
		}

		public class ProcessHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			public ProcessHeaderFetchStrategy(ProcessHeader processHeader)
				: base(processHeader)
			{
				this.processHeader = processHeader;
			}

			readonly ProcessHeader processHeader;

			protected override void FetchForLoadCore()
			{
				if (!Factory.ThreadSentry.IsOwner)
				{
					Factory.ThreadSentry.Post((processHeaderFetchStrategy) => (processHeaderFetchStrategy as ProcessHeaderFetchStrategy).FetchForLoadCore(),
						this,
						$"{nameof(ProcessHeader)}.{nameof(ProcessHeaderFetchStrategy)}.{nameof(FetchForLoadCore)}"); // callingMethodName
					return;
				}

				base.FetchForLoadCore();

				if (processHeader.IsWorkflow)
				{
					Factory.AddFetchHint(typeof(ProcessHeader), processHeader.FH_FH_ParentHeader);
					Factory.AddFetchHint(typeof(ProcessTask), processHeader.GetTasksQuery());
				}

				if (BMSRegistry.Instance.ReleaseSequencesModuleEnabled.Value)
				{
					Factory.AddFetchHint(BMReleaseSequenceItemSchema.BMI_FH_ProcessHeader, processHeader.PK);
					Factory.AddFetchHint(BMReleaseSequenceItemSchema.BMI_FH_ProcessHeader, processHeader.FH_FH_ParentHeader);
				}
			}

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				base.FetchForViewCore(columns);
				foreach (var column in columns)
				{
					switch (column.ColumnName)
					{
						case nameof(EffectiveNudge):
							Factory.AddFetchHint(TagLinkSchema.TGL_ParentId, BusinessObject.PK);
							Factory.AddFetchHint(ProcessHeaderLinkSchema.FP_FH_HeaderTo, BusinessObject.PK);
							break;
						case nameof(CurrentComponentSystemPK):
							if (processHeader.CurrentComponent != null)
							{
								Factory.AddFetchHint(BMSystemSchema.PK, processHeader.CurrentComponent.FC_FS_System);
							}
							break;
					}
				}

				Factory.AddFetchHint(typeof(ProcessTask), processHeader.GetTasksQuery());
				Factory.AddFetchHint(ProcessHeaderLinkSchema.Instance, processHeader.LinksFromOthersToMe_ForBinding.CompleteFilter);
			}

			protected override void FetchForFactorySaveCore()
			{
				base.FetchForFactorySaveCore();
				WorkflowStartabilityStrategy.AddFetchHints(processHeader, Factory);
			}
		}

		#endregion

		#region On Save

		protected override void OnFactorySaving()
		{
			UpdateDatesFromJob();
			base.OnFactorySaving();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded)
			{
				if (workflowNote != null && workflowNote.IsDeleted)
				{
					workflowNote = null;
				}
			}
		}

		internal void OnSavingForService()
		{
			if (!IsTemplate)
			{
				ProcessHeaderLogger.AddStartabilityEventIfNeeded(this);
				UpdatePlannedDuration();
				AssignDefaultCapabilitiesToTasks();
				MaybeLogManualRelease();
				MaybeScheduleTransfer();
				MaybeResetDedicatedBuffer();
				MaybeUpdateEffectiveBranchAndDepartment();

				if (FH_StatusInfo.HasChanges || FH_IsActiveInfo.HasChanges || FH_FC_DedicatedBufferInfo.HasChanges)
				{
					UpdateEffectiveAgreedDeliveryDate();
				}

				if (FH_StatusInfo.HasChanges && FH_Status == WorkflowStatusList.Codes.Closed)
				{
					DeleteRelatedBizos<StmNote>(GetSuccessfulReleaseNoteQuery());
				}

				if (FH_IsApprovedInfo.HasChanges)
				{
					ProcessHeaderLogger.AddApprovedEvent(this, FH_IsApproved);
				}

				CollectDataForDeferralWithESDOrADDRemovedReport();
				ProcessEstimateLogHelper.CreateProcessHeaderEstimateLog(this);
			}

			if (IsTemplate && FH_WorkflowType.IsEmpty)
			{
				FH_WorkflowType = ProcessTaskTemplateSchema.Constants.Prefix;
			}
		}

		void CollectDataForDeferralWithESDOrADDRemovedReport()
		{
			if (IsInDatabase && !IsCollectDataForDeferralWithESDOrADDRemovedReportSuspended)
			{
				var originalEarliestStartDate = (ZDateTime)FH_DoNotStartBeforeDateInfo.OriginalValue;

				if (!FH_EarliestStartDateDefaultsFrom.IsEmpty &&
					FH_DoNotStartBeforeDate != originalEarliestStartDate)
				{
					var collector = ObjectFactory.New<IPAVEUsageCollector>();
					var eventData = new List<KeyValuePair<string, string>>();

					eventData.Add(new KeyValuePair<string, string>("PK", PK.ToString()));
					eventData.Add(new KeyValuePair<string, string>((NoResString)"Name", FH_CompletionStatement));
					eventData.Add(new KeyValuePair<string, string>("FromVisualBoard", "NO"));
					eventData.Add(new KeyValuePair<string, string>("OriginalESDDefaultsFrom", FH_EarliestStartDateDefaultsFrom));

					collector.ReportDeferralWithESDOrADDRemoved(eventData.ToDictionary(kv => kv.Key, kv => kv.Value));
				}
			}
		}

		internal IDisposable SuspendCollectDataForDeferralWithESDOrADDRemovedReport()
		{
			IsCollectDataForDeferralWithESDOrADDRemovedReportSuspended = true;
			return new DisposableAction(() => IsCollectDataForDeferralWithESDOrADDRemovedReportSuspended = false);
		}

		internal bool IsCollectDataForDeferralWithESDOrADDRemovedReportSuspended { get; private set; }

		void MaybeScheduleTransfer()
		{
			if (IsWorkflow && JobHeader != null
				&& JobHeader.FH_DoNotStartBeforeDate.IsValid
				&& JobHeader.FH_DoNotStartBeforeDate > ZDateTime.UtcNow
				&& FH_DoNotStartBeforeDate.IsEmpty && !IsInDatabase)
			{
				ActionScheduleProvider.ScheduleAction(TransferRuleSchedulerAction.Code, JobHeader.FH_DoNotStartBeforeDate, PK, TablePrefix);
			}
			else if (FH_DoNotStartBeforeDate.IsValid && FH_DoNotStartBeforeDate > ZDateTime.UtcNow
				&& (FH_DoNotStartBeforeDateInfo.HasChanges || !IsInDatabase))
			{
				ScheduleActionForTransfer();
			}
		}

		protected virtual void ScheduleActionForTransfer()
		{
			ActionScheduleProvider.ScheduleAction(TransferRuleSchedulerAction.Code, FH_DoNotStartBeforeDate, PK, TablePrefix);
		}

		protected IActionScheduleProvider ActionScheduleProvider => actionScheduleProvider.Value;

		readonly Lazy<IActionScheduleProvider> actionScheduleProvider = new Lazy<IActionScheduleProvider>(() => ObjectFactory.Get<IActionScheduleProvider>());

		void MaybeResetDedicatedBuffer()
		{
			if (!FH_IsActive)
			{
				FH_FC_DedicatedBuffer = ZGuid.Empty;
			}
		}

		void AssignDefaultCapabilitiesToTasks()
		{
			if (!IsWorkflow)
			{
				return;
			}

			var tasks = GetTasksWithoutAccessingWorkflowParent();
			if (!tasks.Any())
			{
				return;
			}

			var newTasks = tasks.Where(t => !t.IsInDatabase);
			if (!newTasks.Any())
			{
				return;
			}

			var firstTask = tasks.MinBy(t => t.P9_Sequence);
			if (!firstTask.IsInDatabase || firstTask.P9_GS_NKAssignedStaffMember != ZString.Empty || !firstTask.P9_G4_RequiredCapability.IsValid)
			{
				return;
			}

			var registryTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.Value.GetTaskTypesFromWorkflowCode(FH_WorkflowType);
			newTasks.ForEach(task =>
			{
				var match = registryTaskTypes.FirstOrDefault(r => (r as WorkflowTaskType).Code == task.P9_Type) as WorkflowTaskType;
				if (match != null && match.DefaultCapability.IsValid)
				{
					task.P9_G4_RequiredCapability = match.DefaultCapability;
					task.DefaultCapabilityAssigned = true;
					task.Validation.ValidateP9_G4_RequiredCapability();
				}
			});
		}

		internal ZDateTime? LastEditTimeFromDatabaseForStatusCheck { get; set; }

		public static void SuppressUpdatingWorkflowStatusesOnSave(BusinessObjectFactory factory)
		{
			GetProcessHeaderSaveSettings(factory).UpdatingWorkflowStatusesSuppressed = true;
		}

		public static void SuppressReloadingComponentsFromDbOnSave(BusinessObjectFactory factory)
		{
			GetProcessHeaderSaveSettings(factory).ReloadingComponentsIgnoringUberFactoryCacheSuppressed = true;
		}

		public static ProcessHeaderSaveSettings GetProcessHeaderSaveSettings(BusinessObjectFactory factory) => factory.GetCachedValue(nameof(ProcessHeaderSaveSettings), () => new ProcessHeaderSaveSettings(), CacheStalenessPolicy.StaleOnFactorySave);

		public class ProcessHeaderSaveSettings
		{
			public bool UpdatingWorkflowStatusesSuppressed { get; set; }
			public bool ReloadingComponentsIgnoringUberFactoryCacheSuppressed { get; set; }
		}

		#endregion

		public bool IsInBuffer
		{
			get
			{
				return CurrentComponent != null && CurrentComponent.IsBuffer;
			}
		}

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			FH_LastTransferType = BMConstants.LastTransferTypeDefaultCode;
		}

		protected override ZString HumanReadableNameCore
		{
			get { return string.Format(CultureInfo.InvariantCulture, "{0}: {1}", ProcessHeaderType, FH_CompletionStatement); }
		}

		protected override void OnConcurrencyExceptionCore(IEnumerable<IPropertyRecord> propertyRecords)
		{
			base.OnConcurrencyExceptionCore(propertyRecords);

			if (propertyRecords.Any(record => record.ColumnName == ProcessHeaderSchema.FH_Status.Name))
			{
				ClearLinkCaches();

				Factory.ReloadAllSafe<ProcessHeaderLink>();
			}
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();

			ClearLinkCaches();
		}

		void ClearLinkCaches()
		{
			Factory.ClearCachedValue<ProcessHeaderLink[]>(LinksFromMeToOthers_CacheKey);
			Factory.ClearCachedValue<ProcessHeaderLink[]>(LinksFromOthersToMe_CacheKey);
		}

		#endregion

		#region Related Business Objects

		#region Tasks

		public virtual IEnumerable<ProcessTask> Tasks
		{
			get
			{
				if (Parent != null)
				{
					foreach (ProcessTask task in Parent.WorkflowItems.Tasks)
					{
						if (task.P9_FH_ProcessHeader == PK)
						{
							yield return task;
						}
					}
				}
				else
				{
					foreach (var task in GetTasksWithoutAccessingWorkflowParent())
					{
						yield return task;
					}
				}
			}
		}

		[ChildEditable]
		public ProcessTaskCollectionView TaskCollection
		{
			get
			{
				if (taskCollection == null)
				{
					taskCollection = GetTaskCollectionForLoadedParent(this, Parent, () => new ProcessHeaderProcessTaskCollectionView(this));
				}

				return taskCollection;
			}
		}

		ProcessTaskCollectionView taskCollection;

		IBindingListView IProcessHeader.TaskCollectionIncludingChildWorkflowTasksBindingListView => TaskCollectionIncludingChildWorkflowTasks;

		[ChildEditable]
		public ProcessTaskCollectionView TaskCollectionIncludingChildWorkflowTasks
		{
			get
			{
				if (taskCollectionIncludingChildWorkflowTasks == null)
				{
					taskCollectionIncludingChildWorkflowTasks = GetTaskCollectionForLoadedParent(this, Parent, () => new ProcessHeaderAndChildrenProcessTaskCollectionView(this));
				}

				return taskCollectionIncludingChildWorkflowTasks;
			}
		}

		ProcessTaskCollectionView taskCollectionIncludingChildWorkflowTasks;

		[ChildEditable]
		public ProcessTaskCollectionView AllTasksAndCompletionStatements
		{
			get
			{
				if (allTasksAndCompletionStatements == null)
				{
					allTasksAndCompletionStatements = GetTaskCollectionForLoadedParent(this, Parent, () => new ProcessHeaderProcessTaskCollectionView(this, allowCompletionStatements: true));
				}

				return allTasksAndCompletionStatements;
			}
		}

		ProcessTaskCollectionView allTasksAndCompletionStatements;

		[ChildEditable]
		public ProcessTaskCollectionView CompletionStatementTasksIncludingChildWorkflowTasks
		{
			get
			{
				if (completionStatementTasksIncludingChildWorkflowTasks == null)
				{
					completionStatementTasksIncludingChildWorkflowTasks = GetTaskCollectionForLoadedParent(this, Parent, () => new CompletionStatementProcessTaskCollectionView(this));
				}

				return completionStatementTasksIncludingChildWorkflowTasks;
			}
		}

		ProcessTaskCollectionView completionStatementTasksIncludingChildWorkflowTasks;

		static ProcessTaskCollectionView GetTaskCollectionForLoadedParent(ProcessHeader processHeader, IWorkflowProvider parent, Func<ProcessTaskCollectionView> collectionCreator)
		{
			ProcessTaskCollectionView result;

			if (parent == null)
			{
				result = new ProcessTaskCollectionView(new ProcessTaskCollection(processHeader.Factory, ZQuery.NoResultQuery));
			}
			else
			{
				if (!parent.WorkflowItems.IsLoaded)
				{
					parent.WorkflowItems.Load();
				}
				result = collectionCreator();
			}

			processHeader.RegisterEditableChildObject(result);

			return result;
		}

		public ICollection<ProcessTask> GetTasksWithoutAccessingWorkflowParent()
		{
			return Factory.Load<ProcessTask>(GetTasksQuery());
		}

		internal virtual ZQuery GetTasksQuery()
		{
			return new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, PK) { FetchOnlyFromLocalCache = !IsInDatabase };
		}

		#endregion

		#region Links

		#region Collections For Binding Only

		[ChildEditable]
		public ProcessHeaderLinkCollection LinksFromMeToOthers_ForBinding
		{
			get { return Links_ForBinding.FromHeaderLinks; }
		}

		// This header's dependencies(prerequisites) and parent/child links
		[ChildEditable]
		public ProcessHeaderLinkCollection LinksFromOthersToMe_ForBinding
		{
			get { return Links_ForBinding.ToHeaderLinks; }
		}

		// This header's prerequisites
		[ChildEditable]
		public ProcessHeaderLinkCollection PrerequisiteLinks_ForBinding
		{
			get
			{
				if (prerequisiteLinks == null)
				{
					prerequisiteLinks = new ProcessHeaderLinkCollection(this, ProcessHeaderLinkSchema.FP_FH_HeaderTo, ProcessHeaderLinkTypeList.Codes.Dependency);
					RegisterEditableChildObject(prerequisiteLinks);
				}

				return prerequisiteLinks;
			}
		}
		ProcessHeaderLinkCollection prerequisiteLinks;

		// This header's postrequisites
		[ChildEditable]
		public ProcessHeaderLinkCollection PostrequisiteLinks_ForBinding
		{
			get
			{
				if (postrequisiteLinks == null)
				{
					postrequisiteLinks = new ProcessHeaderLinkCollection(this, ProcessHeaderLinkSchema.FP_FH_HeaderFrom, ProcessHeaderLinkTypeList.Codes.Dependency);
					RegisterEditableChildObject(postrequisiteLinks);
				}

				return postrequisiteLinks;
			}
		}
		ProcessHeaderLinkCollection postrequisiteLinks;

		// This header's children
		[ChildEditable]
		public ProcessHeaderLinkCollection ChildLinks_ForBinding
		{
			get
			{
				if (childLinks == null)
				{
					childLinks = new ProcessHeaderLinkCollection(this, ProcessHeaderLinkSchema.FP_FH_HeaderTo, ProcessHeaderLinkTypeList.Codes.ParentChild);
					RegisterEditableChildObject(childLinks);
				}

				return childLinks;
			}
		}
		ProcessHeaderLinkCollection childLinks;

		// This header's parents
		[ChildEditable]
		public ProcessHeaderLinkCollection ParentLinks_ForBinding
		{
			get
			{
				if (parentLinks == null)
				{
					parentLinks = new ProcessHeaderLinkCollection(this, ProcessHeaderLinkSchema.FP_FH_HeaderFrom, ProcessHeaderLinkTypeList.Codes.ParentChild);
					RegisterEditableChildObject(parentLinks);
				}

				return parentLinks;
			}
		}
		ProcessHeaderLinkCollection parentLinks;

		public ProcessHeaderLinkWrapperCollection Links_ForBinding
		{
			get
			{
				if (links_ForBinding == null)
				{
					links_ForBinding = new ProcessHeaderLinkWrapperCollection(this);
				}

				return links_ForBinding;
			}
		}

		ProcessHeaderLinkWrapperCollection links_ForBinding;

		#endregion

		#region Link Sets

		public IEnumerable<ProcessHeaderLink> Links => LinksFromOthersToMe.Concat(LinksFromMeToOthers);

		public IEnumerable<ProcessHeaderLink> LinksFromMeToOthers
		{
			get
			{
				return Factory.GetCachedValue(LinksFromMeToOthers_CacheKey, () => Factory.Load<ProcessHeaderLink>(LinksFromMeToOthers_Query), LinkSetStalenessPolicy).Where(w => !w.IsDeleted);
			}
		}

		public IEnumerable<ProcessHeaderLink> LinksFromOthersToMe
		{
			get
			{
				return Factory.GetCachedValue(LinksFromOthersToMe_CacheKey, () => Factory.Load<ProcessHeaderLink>(LinksFromOthersToMe_Query), LinkSetStalenessPolicy).Where(w => !w.IsDeleted);
			}
		}

		ZQuery LinksFromMeToOthers_Query => new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, PK) { FetchOnlyFromLocalCache = ShouldFetchLinksOnlyFromLocalCache };
		ZQuery LinksFromOthersToMe_Query => new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderTo, PK) { FetchOnlyFromLocalCache = ShouldFetchLinksOnlyFromLocalCache };

		// reloading links on save may not be needed if the links are already reloaded from db in ProcessHeaderOnSavingService.UpdateHeaders
		bool ShouldFetchLinksOnlyFromLocalCache => !IsInDatabase || ProcessJobHeader.GetJobsWithReloadedLinksPKs(this).Contains(FH_ParentId);

		string LinksFromMeToOthers_CacheKey => string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", nameof(ProcessHeader), nameof(LinksFromMeToOthers), PK);
		string LinksFromOthersToMe_CacheKey => string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", nameof(ProcessHeader), nameof(LinksFromOthersToMe), PK);

		CacheStalenessPolicy LinkSetStalenessPolicy => CacheStalenessPolicy.StaleWhenDataTableChanges(ProcessHeaderLinkSchema.Constants.TableName, Factory);

		public IEnumerable<ProcessHeaderLink> PrerequisiteLinks => LinksFromOthersToMe.Where(l => l.FP_LinkType == ProcessHeaderLinkTypeList.Codes.Dependency);

		public IEnumerable<ProcessHeaderLink> PostrequisiteLinks => LinksFromMeToOthers.Where(l => l.FP_LinkType == ProcessHeaderLinkTypeList.Codes.Dependency);

		public IEnumerable<ProcessHeaderLink> ChildLinks => LinksFromOthersToMe.Where(l => l.FP_LinkType == ProcessHeaderLinkTypeList.Codes.ParentChild);

		public IEnumerable<ProcessHeaderLink> ParentLinks => LinksFromMeToOthers.Where(l => l.FP_LinkType == ProcessHeaderLinkTypeList.Codes.ParentChild);

		public bool IsTerminal => LinksFromOthersToMe.Count() + LinksFromMeToOthers.Count() < 2;

		/// <summary>
		/// Gets the workflows which this workflow is a pre-requisite to. If this workflow is a pre-requisite to a job, this returns all the
		/// workflows in that job which only have open dependencies of this workflow (ie, can be started once the current workflow is complete).
		/// </summary>
		public IEnumerable<ProcessHeader> PostrequisiteWorkflows
		{
			get
			{
				var postReqs = new List<ProcessHeader>();

				foreach (var headerTo in PostrequisiteLinks.Select(pr => pr.HeaderTo).WhereNotNull())
				{
					if (headerTo.IsWorkflow)
					{
						postReqs.Add(headerTo);
					}
					else
					{
						foreach (var workflow in ((ProcessJobHeader)headerTo).ProcessHeaders.Where(w => w.IsOpen))
						{
							if (workflow.PrerequisiteLinks.Select(l => l.HeaderFrom).WhereNotNull().All(h => !h.IsOpen || h == this))
							{
								postReqs.Add(workflow);
							}
						}
					}
				}

				return postReqs;
			}
		}

		#endregion

		#region Link Helpers

		public bool MakePrerequisiteOf(ProcessHeader processHeader)
		{
			var result = WorkflowRelationshipCreator.CreateDependencyRelationship(Factory, this, processHeader, RelationshipOptions.CreateOnlyIfLinkDoesNotExist | RelationshipOptions.ReverseExistingRelationship);
			return !result.IsExistingLink;
		}

		public ProcessHeaderLink GetOrCreateDependencyLink(ProcessHeader processHeader)
		{
			return WorkflowRelationshipCreator.CreateDependencyRelationship(Factory, this, processHeader, RelationshipOptions.CreateOnlyIfLinkDoesNotExist).Link;
		}

		#endregion

#if DEBUG
		public bool MakePrerequisiteOfAllowingReverseRelationship_ForTest(ProcessHeader processHeader)
		{
			var result = WorkflowRelationshipCreator.CreateDependencyRelationship(Factory, this, processHeader, RelationshipOptions.CreateOnlyIfLinkDoesNotExist);
			return !result.IsExistingLink;
		}

		public ProcessHeaderLink GetOrCreateDependencyLinkAllowingReverseRelationship_ForTest(ProcessHeader processHeader)
		{
			return WorkflowRelationshipCreator.CreateDependencyRelationship(Factory, this, processHeader, RelationshipOptions.CreateOnlyIfLinkDoesNotExist).Link;
		}
#endif

		#endregion

		#region System

		public BMSystem BMSystem
		{
			get
			{
				var currentComponentSystem = CurrentComponent?.System;
				if (currentComponentSystem != null)
				{
					return currentComponentSystem;
				}

				var template = GetTemplate();
				if (template != null)
				{
					return BMSystem.GetForTemplate(template);
				}

				if (Parent != null)
				{
					return BMSystem.GetSystemForWorkflowProvider(Parent, Factory);
				}

				return null;
			}
		}

		[List("Lookups.Systems")]
		public ZGuid CurrentComponentSystemPK
		{
			get
			{
				if (currentComponentSystemPK.IsEmpty)
				{
					var system = BMSystem;
					if (system != null && (FH_FC_CurrentComponent.IsValid || !IsWorkflow))
					{
						currentComponentSystemPK = system.PK;
					}
				}
				return currentComponentSystemPK;
			}
			set
			{
				if (HasSecurityRightsToChangeCurrentComponent)
				{
					currentComponentSystemPK = value;
					ChangeSystemAndComponent(currentComponentSystemPK);
					if (!IsValidationSuspended)
					{
						Validation.ValidateCurrentComponentSystemPK();
					}
				}
				CurrentComponentSystemPKInfo.RefreshBinding();
				FH_FC_CurrentComponentInfo.RefreshBinding();
			}
		}
		ZGuid currentComponentSystemPK;

		public ZPropertyInfo CurrentComponentSystemPKInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentComponentSystemPK)); }
		}

		void ChangeSystemAndComponent(ZGuid systemPK)
		{
			var system = Factory.Load<BMSystem>(systemPK);
			if (system != null)
			{
				var firstComponent = GetFirstBMComponent(system);
				if (firstComponent != null)
				{
					FH_FC_CurrentComponent = firstComponent.PK;
				}
			}
			else
			{
				FH_FC_CurrentComponent = ZGuid.Invalid;
			}
		}

		internal ProcessTaskTemplate GetTemplate()
		{
			if (JobHeader != null && !(JobHeader.FH_ParentTemplateId.IsEmpty))
			{
				var templateWorkflow = Factory.Load<ProcessHeader>(JobHeader.FH_ParentTemplateId);
				if (templateWorkflow != null)
				{
					return Factory.Load<ProcessTaskTemplate>(templateWorkflow.FH_P0_Template);
				}
			}
			else
			{
				return Template;
			}

			return null;
		}

		#endregion

		#region Component

		public BMComponent CurrentComponent
		{
			get => FH_FC_CurrentComponent.IsEmpty ? null : Factory.Load<BMComponent>(FH_FC_CurrentComponent);
		}

		#endregion

		#region Dedicated Buffer

		public BMComponent DedicatedBuffer
		{
			get => FH_FC_DedicatedBuffer.IsEmpty ? null : Factory.Load<BMComponent>(FH_FC_DedicatedBuffer);
		}

		#endregion

		public bool HasSecurityRightsToChangeCurrentComponent
		{
			get
			{
				return IsCurrentComponentSecurityValidationSuspended || Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed;
			}
		}

		public bool CannotEditJLWReleaseGroup => !FH_FH_ParentHeader.IsValid && !Env.Security.JLWReleaseGroupEdit.IsAllowed;

		#region Process Header

		public virtual ProcessJobHeader JobHeader
		{
			get
			{
				if ((jobHeader == null || jobHeader.IsDeleted || jobHeader.PK != FH_FH_ParentHeader) && FH_FH_ParentHeader.IsValid)
				{
					jobHeader = Factory.LoadTop1<ProcessJobHeader>(new ZQuery(ProcessHeaderSchema.PK, FH_FH_ParentHeader) { FetchOnlyFromLocalCache = !IsInDatabase });
				}
				return jobHeader;
			}
		}

		ProcessJobHeader jobHeader;

		public ProcessHeader WorkflowParent
		{
			get
			{
				var link = WorkflowParentLink;
				return link != null ? link.HeaderTo : null;
			}
		}

		public ProcessHeaderLink WorkflowParentLink
		{
			get
			{
				if (IsWorkflow)
				{
					return ParentLinks.FirstOrDefault(l => l.HeaderTo != null && IsChildWorkflowWithinJobOf(l.HeaderTo));
				}
				else
				{
					return null;
				}
			}
		}

		public IEnumerable<ProcessHeader> GetWorkflowParents(HashSet<ZGuid> existingParent = null)
		{
			HashSet<ZGuid> currentExistingParent = existingParent == null
				? new HashSet<ZGuid>(new[] { PK }) // stop current-processheader to be its own parent
				: new HashSet<ZGuid>(existingParent);

			var workflowParent = WorkflowParent;
			if (workflowParent != null)
			{
				if (!currentExistingParent.Add(workflowParent.PK))
				{
					yield break;
				}
				else
				{
					yield return workflowParent;

					foreach (var parent in workflowParent.GetWorkflowParents(currentExistingParent))
					{
						yield return parent;
					}
				}
			}
		}

		#endregion

		#region Parent

		public IWorkflowProvider Parent
		{
			get { return parent ?? (parent = GetParent(Factory)); }
			protected set { parent = value; }
		}

		IWorkflowProvider parent;

		public IWorkflowProvider GetParent(BusinessObjectFactory factory)
		{
			if (FH_ParentId.IsValid)
			{
				var parentType = ParentType;
				if (parentType != null)
				{
					return factory.Load(parentType, FH_ParentId) as IWorkflowProvider;
				}
			}
			return null;
		}

		public Type ParentType => BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(FH_ParentTableCode);

		// search up 'FetchHints' on the company intranet for more info on deep v shallow fetchhints! :)
		public void AddDeepFetchHintForParentType(BusinessObjectFactory specificFactory = null, Type parentType = null)
		{
			if (IsDeleted)
			{
				return;
			}

			var parentTypeToHint = parentType ?? ParentType;

			if (parentTypeToHint != null)
			{
				if (typeof(NonPersistentBusinessObject).IsAssignableFrom(parentTypeToHint))
				{
					if (FH_ParentTableCode == ViewQuotedBookingSchema.Constants.Prefix)
					{
						parentTypeToHint = ObjectFactory.GetType<IViewQuotedBooking>();
					}
					else
					{
						var workflowProvider = Parent;

						if (workflowProvider == null)
						{
							return;
						}

						var workflowType = workflowProvider.WorkflowType;
						var descriptor = new WorkflowDescriptor.Loader().GetWorkflowDescriptor(workflowType);

						if (descriptor == null)
						{
							ReportWorkflowDescriptorError(parentTypeToHint, workflowType);
							return;
						}

						parentTypeToHint = descriptor.WorkflowProviderTypeForNonPersistentBusinessObjects;
					}
				}

				var factoryForFetchHints = specificFactory ?? Factory;
				factoryForFetchHints.AddFetchHint(parentTypeToHint, FH_ParentId);
			}
		}

		void ReportWorkflowDescriptorError(Type parentTypeToHint, ZString workflowType)
		{
			var message = FormattableString.Invariant($@"Attempted to add fetch hints to a workflow for a NonPersistentBusinessObject but could not get a matching workflow provider. Details:
Parent type: {parentTypeToHint.Name}
Workflow type: {workflowType}
FH_ParentTableCode: {FH_ParentTableCode}
Workflow description: {FH_CompletionStatement}");

			ErrorReporter.ReportOnce("NoWorkflowDescriptorForWorkflowProvider", message);
		}

		public bool IsInSameJob(ProcessHeader processHeader)
		{
			return FH_ParentId == processHeader.FH_ParentId
				&& FH_ParentTableCode == processHeader.FH_ParentTableCode;
		}

		public bool IsInSameJob(ProcessTask task)
		{
			return FH_ParentId == task.P9_ParentID
				&& FH_ParentTableCode == task.P9_ParentTableCode;
		}

		#endregion

		#region Shapes

		public IBMNCNShape DirectlyApprovedShape
		{
			get
			{
				var query = new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, PK);
				query.AddToFilter(BMNCNShapeSchema.BNS_GS_NKApprovedBy, SQLComparisonOperator.NotEqual, ZString.Empty);
				query.IncludeBlob(BMNCNShapeSchema.BNS_LayoutData);

				return Factory.LoadTop1<IBMNCNShape>(query);
			}
		}

		public IBMNCNShape ApprovedShape
		{
			get
			{
				if (!IsInPlanningManagementMode)
				{
					return null;
				}

				return GetApprovedShape();
			}
		}

		public IBMNCNShape GetApprovedShape(HashSet<ZGuid> existingParent = null)
		{
			if (approvedShape_DoNotAccessDirectly == null)
			{
				approvedShape_DoNotAccessDirectly = FindApprovedShape(existingParent);
			}
			else if (!approvedShape_DoNotAccessDirectly.IsApproved)
			{
				approvedShape_DoNotAccessDirectly = null;
			}

			return approvedShape_DoNotAccessDirectly;
		}

		IBMNCNShape approvedShape_DoNotAccessDirectly;

		IBMNCNShape FindApprovedShape(HashSet<ZGuid> existingParent = null)
		{
			if (!IsInPlanningManagementMode)
			{
				return null;
			}

			HashSet<ZGuid> currentExistingParent = existingParent == null ? new HashSet<ZGuid>() : new HashSet<ZGuid>(existingParent);

			var shape = DirectlyApprovedShape;
			if (shape == null)
			{
				if (!currentExistingParent.Add(PK))
				{
					return shape;
				}
				else
				{
					if (IsWorkflow)
					{
						var workflowParent = WorkflowParent;

						if (workflowParent != null)
						{
							shape = workflowParent.GetApprovedShape(currentExistingParent);
						}

						if (shape == null)
						{
							shape = JobHeader?.GetApprovedShape(currentExistingParent);
						}
					}
					else
					{
						shape = (
							from link in ParentLinks
							let parentWorkflow = link.HeaderTo
							where parentWorkflow != null
							let s = parentWorkflow.GetApprovedShape(currentExistingParent)
							where s != null
							select s
							).FirstOrDefault();
					}
				}
			}

			return shape;
		}

		#region ApprovedShapeProperties

		// Can't bind these directly to ApprovedShape since ZArch thinks the interface type is a collection rather than a business object

		[ReadOnly(true)]
		[ResourceStringData("ProcessHeader.ApprovedShapeExplicitDurationHours", Caption = "Planned Duration Hours", FullDescription = "The number of hours planned to complete this item.")]
		public ZDecimal ApprovedShapeExplicitDurationHours
		{
			get { return ApprovedShape != null ? ApprovedShape.ExplicitDurationHours : ZDecimal.Zero; }
		}

		[VisualBoardSearchable]
		[ReadOnly(true)]
		[ResourceStringData("ProcessHeader.ApprovedShapeIsCriticalPath", Caption = "Critical Chain", FullDescription = "Indicates whether this item is on the critical chain of the approved diagram.")]
		public ZBool ApprovedShapeIsCriticalPath
		{
			get { return ApprovedShape != null ? ApprovedShape.IsCriticalPath : ZBool.False; }
		}

		[ReadOnly(true)]
		[ResourceStringData("ProcessHeader.ApprovedShapeScheduledStartTimeLocal", Caption = "Scheduled Start", FullDescription = "The time this item has been scheduled to start on an approved Network Diagram.")]
		public ZDateTime ApprovedShapeScheduledStartTimeLocal
		{
			get { return ApprovedShape != null ? ApprovedShape.ScheduledStartTimeUtc.ToLocalBranchTime(Factory) : ZDateTime.Empty; }
		}

		[ReadOnly(true)]
		[ResourceStringData("ProcessHeader.ApprovedShapeScheduledFinishTimeLocal", Caption = "Scheduled Finish", FullDescription = "The time this item has been scheduled to finish on an approved Network Diagram.")]
		public ZDateTime ApprovedShapeScheduledFinishTimeLocal
		{
			get { return ApprovedShape != null ? ApprovedShape.ScheduledFinishTimeUtc.ToLocalBranchTime(Factory) : ZDateTime.Empty; }
		}

		[ReadOnly(true)]
		[ResourceStringData("ProcessHeader.ApprovedShapeRootDiagramName", Caption = "Diagram Name", FullDescription = "The name of the approved Network Diagram this shape is part of.")]
		public ZString ApprovedShapeRootDiagramName
		{
			get
			{
				var approvedShape = ApprovedShape;
				var diagram = approvedShape != null ? approvedShape.RootDiagram : null;

				return diagram != null ? diagram.BNS_Name : ZString.Empty;
			}
		}

		#endregion

		#endregion

		#region Tag Links

		[ChildEditable]
		public TagLinkCollection TagLinks_ForBinding
		{
			get
			{
				if (tagLinks == null)
				{
					tagLinks = new TagLinkCollection(this);
					RegisterEditableChildObject(tagLinks);
				}
				return tagLinks;
			}
		}
		TagLinkCollection tagLinks;

		#region Tags For UC
		/// <summary>
		/// This property has been created to intercept the signature [CollectionRelationProperty("Tags", "TGL_ParentId", "FHTags")] into Odyssey.Interfaces.cs.
		/// As we never want Tags with the UsageScope RUL to be copied, we are always using this property to copy.
		/// </summary>
		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public IEnumerable<TagLink> Tags
		{
			get
			{
				if (tagLinks_ForUC == null)
				{
					tagLinks_ForUC = new TagLinkCollection(this);
				}
				return tagLinks_ForUC.Where(o => o.Definition.TGD_UsageScope != TagUsageScopeList.Codes.Rule);
			}
		}
		TagLinkCollection tagLinks_ForUC;

		#endregion

		#endregion

		#region Iteration Links

		[ChildEditable]
		public IProcessTaskIterationLinkCollection IterationLinks
		{
			get
			{
				if (iterationLinks == null)
				{
					iterationLinks = ObjectFactory.Get<IProcessTaskIterationLinkCollection>("IProcessTaskIterationLinkCollection", this);
					RegisterEditableChildObject(iterationLinks);
				}

				return iterationLinks;
			}
		}

		IProcessTaskIterationLinkCollection iterationLinks;

		#endregion

		#region Iteration Reason

		[MaxLength(3)]
		[BusinessObjectTestExclude]
		[List("Lookups.IterationReasons")]
		[ReadOnlyMember(nameof(IterationReason_ReadOnly))]
		[ResourceStringData("ProcessHeader.IterationReason", Caption = "Iteration Reason", FullDescription = "The reason why this Quality Iteration was triggered.")]
		public ZString IterationReason
		{
			get
			{
				return !HasValidIterationLink() ? ZString.Empty : IterationLinks[IterationLinks.Count - 1].P9I_IterationReason;
			}
			set
			{
				if (!HasValidIterationLink())
				{
					throw new NotSupportedException("Cannot set Iteration Reason when there are no Iteration Links");
				}

				IterationLinks[IterationLinks.Count - 1].P9I_IterationReason = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateIterationReason();
				}
			}
		}

		public ZPropertyInfo IterationReasonInfo
		{
			get { return GetZPropertyInfo(nameof(IterationReason)); }
		}

		internal bool HasValidIterationLink()
		{
			return IterationLinks.Count > 0;
		}

		protected bool IterationReason_ReadOnly
		{
			get { return !HasValidIterationLink(); }
		}

		#endregion

		#region WorkflowNote

		public WorkflowStmNote WorkflowNote
		{
			get
			{
				if (workflowNote == null)
				{
					workflowNote = WorkflowStmNote.GetOrCreateForParent(this);
					RegisterEditableChildObject(workflowNote);
				}

				return workflowNote;
			}
		}

		WorkflowStmNote workflowNote;

		#endregion

		#endregion

		#region Properties

		#region FH_FH_ParentHeader

		[RelatedBusinessObject("ParentHeader")]
		[List("Lookups.ParentHeaders")]
		public override ZGuid FH_FH_ParentHeader
		{
			get { return base.FH_FH_ParentHeader; }
			set
			{
				base.FH_FH_ParentHeader = value;

				Parent = JobHeader?.Parent;
			}
		}

		public ProcessHeader ParentHeader
		{
			get { return Factory.Load<ProcessHeader>(FH_FH_ParentHeader); }
		}

		internal void SetBufferManagementComponentIfBlank()
		{
			if (FH_FC_CurrentComponent.IsEmpty && FH_FH_ParentHeader.IsValid && JobHeader?.Parent != null)
			{
				var firstComponent = GetFirstBMComponent();
				if (firstComponent != null)
				{
					using (SuspendSettingHasChanges())
					{
						FH_FC_CurrentComponent = firstComponent.PK;
					}
				}
			}
		}

		internal BMComponent GetFirstBMComponent(BMSystem otherSystem = null)
		{
			var system = otherSystem ?? BMSystem ?? JobHeader?.BMSystem;
			return system?.GetFirstBMComponent();
		}

		#endregion

		#region FH_ParentId

		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public override ZGuid FH_ParentId
		{
			get { return base.FH_ParentId; }
			set
			{
				var originalValue = FH_ParentId;

				base.FH_ParentId = value;

				if (!originalValue.IsEmpty && value.IsEmpty)
				{
					FH_FC_CurrentComponent = ZGuid.Empty;
				}
				else if (!value.IsEmpty)
				{
					SetBufferManagementComponentIfBlank();
				}
			}
		}

		#endregion

		#region FH_ParentTableCode

		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public override ZString FH_ParentTableCode
		{
			get => base.FH_ParentTableCode;
			set => base.FH_ParentTableCode = value;
		}

		#endregion

		#region FH_MilestoneCompletionPivotKey

		[List("Lookups.CompletionMilestones")]
		public override ZString FH_MilestoneCompletionPivotKey
		{
			get => base.FH_MilestoneCompletionPivotKey;
			set => base.FH_MilestoneCompletionPivotKey = value;
		}

		#endregion

		#region FH_DoNotStartBeforeDate

		public override ZDateTime FH_DoNotStartBeforeDate
		{
			get { return new ZDateTime(base.FH_DoNotStartBeforeDate, DateTimeKind.Utc); }
			set
			{
				base.FH_DoNotStartBeforeDate = value;
				DoNotStartBeforeDateLocalInfo.RefreshBinding();

				UpdateReleaseSequenceSortDate();
			}
		}

		[ResourceStringData("ProcessHeader.DoNotStartBeforeDateLocal", ShortCaption = "Earliest Start", Caption = "Earliest Start Date", FullDescription = "The date prior to which work should not commence on this workflow.")]
		public ZDateTime DoNotStartBeforeDateLocal
		{
			get { return FH_DoNotStartBeforeDate.ToLocalBranchTime(Factory); }
			set { FH_DoNotStartBeforeDate = value.ToUniversalBranchTime(Factory); }
		}

		public ZPropertyInfo DoNotStartBeforeDateLocalInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DoNotStartBeforeDateLocal), o => FH_DoNotStartBeforeDateInfo); }
		}

		public ZDateTime ApplicableDoNotStartBeforeDateLocal
		{
			get
			{
				if (FH_DoNotStartBeforeDate.IsEmpty)
				{
					var jobHeader = JobHeader;
					if (jobHeader != null)
					{
						return jobHeader.DoNotStartBeforeDateLocal;
					}
				}

				return DoNotStartBeforeDateLocal;
			}
		}

		public ZDateTime ApplicableDoNotStartBeforeDateUtc
		{
			get
			{
				if (FH_DoNotStartBeforeDate.IsEmpty)
				{
					var jobHeader = JobHeader;
					if (jobHeader != null)
					{
						return jobHeader.FH_DoNotStartBeforeDate;
					}
				}

				return FH_DoNotStartBeforeDate;
			}
		}

		#endregion

		#region FH_AgreedDeliveryDate

		public override ZDateTime FH_AgreedDeliveryDate
		{
			get { return new ZDateTime(base.FH_AgreedDeliveryDate, DateTimeKind.Utc); }
			set
			{
				base.FH_AgreedDeliveryDate = value;
				AgreedDeliveryDateLocalInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateFH_DateAcceptability();
				}

				UpdateEffectiveAgreedDeliveryDate();
			}
		}

		public virtual void UpdateEffectiveAgreedDeliveryDate()
		{
			if (!CanCalculateEffectiveAgreedDeliveryDate)
			{
				FH_EffectiveAgreedDeliveryDateUtc = ZDateTime.Empty;
				return;
			}

			if (IsInBuffer && FH_EffectiveAgreedDeliveryDateUtc.IsValid)
			{
				return; 
			}

			FH_EffectiveAgreedDeliveryDateUtc = FH_AgreedDeliveryDate != ZDateTime.Empty
				? FH_AgreedDeliveryDate
				: JobHeader?.FH_AgreedDeliveryDate ?? ZDateTime.Empty;
		}

		[ResourceStringData("ProcessHeader.AgreedDeliveryDateLocal", ShortCaption = "Agreed Delivery", Caption = "Agreed Delivery Date", FullDescription = "The agreed delivery date of this workflow.")]
		public ZDateTime AgreedDeliveryDateLocal
		{
			get { return FH_AgreedDeliveryDate.ToLocalBranchTime(Factory); }
			set { FH_AgreedDeliveryDate = value.ToUniversalBranchTime(Factory); }
		}

		public ZPropertyInfo AgreedDeliveryDateLocalInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(AgreedDeliveryDateLocal), o => FH_AgreedDeliveryDateInfo); }
		}

		public ZDateTime ApplicableAgreedDeliveryDateLocal
		{
			get
			{
				if (FH_AgreedDeliveryDate.IsEmpty)
				{
					var jobHeader = JobHeader;
					if (jobHeader != null)
					{
						return jobHeader.AgreedDeliveryDateLocal;
					}
				}

				return AgreedDeliveryDateLocal;
			}
		}

		public ZDateTime ApplicableAgreedDeliveryDateUtc
		{
			get
			{
				if (FH_AgreedDeliveryDate.IsEmpty)
				{
					var jobHeader = JobHeader;
					if (jobHeader != null)
					{
						return jobHeader.FH_AgreedDeliveryDate;
					}
				}

				return FH_AgreedDeliveryDate;
			}
		}

		#endregion

		#region Release Gate Related Dates

		[ReadOnly(true)]
		[CustomisedControlExclude]
		public override ZDateTime FH_EffectiveAgreedDeliveryDateUtc { get => base.FH_EffectiveAgreedDeliveryDateUtc; set => base.FH_EffectiveAgreedDeliveryDateUtc = value; }

		[ReadOnly(true)]
		[CustomisedControlExclude]
		public override ZDateTime FH_LatestAcceptableReleaseDateUtc
		{
			get => base.FH_LatestAcceptableReleaseDateUtc;
			set
			{
				base.FH_LatestAcceptableReleaseDateUtc = value;
				LatestAcceptableReleaseDateStringInfo.RefreshBinding();
			}
		}

		[CustomisedControlExclude]
		public ZString LatestAcceptableReleaseDateUtcString
		{
			get
			{
				return FH_LatestAcceptableReleaseDateUtc.ToShortDateString();
			}
		}

		public ZPropertyInfo LatestAcceptableReleaseDateStringInfo
		{
			get { return GetZPropertyInfo(nameof(LatestAcceptableReleaseDateUtcString)); }
		}

		[ReadOnly(true)]
		[CustomisedControlExclude]
		public override ZDateTime FH_ReleaseSequenceSortDateUtc { get => base.FH_ReleaseSequenceSortDateUtc; set => base.FH_ReleaseSequenceSortDateUtc = value; }

		#endregion

		#region FH_ReleaseDateTime

		[ReadOnly(true)]
		public override ZDateTime FH_ReleaseDateTime
		{
			get { return new ZDateTime(base.FH_ReleaseDateTime, DateTimeKind.Utc); }
			set
			{
				base.FH_ReleaseDateTime = value;
				LastTransferDateLocalInfo.RefreshBinding();

				UpdateReleaseSequenceSortDate();
			}
		}

		[ReadOnly(true)]
		public ZDateTime LastTransferDateLocal
		{
			get { return FH_ReleaseDateTime.ToLocalBranchTime(Factory); }
			set { FH_ReleaseDateTime = value.ToUniversalBranchTime(Factory); }
		}

		public ZPropertyInfo LastTransferDateLocalInfo
		{
			get { return GetZPropertyInfo(nameof(LastTransferDateLocal)); }
		}

		#endregion

		#region FH_GG_ReleaseGroup

		[List("Lookups.AllReleaseGroups")]
		[ReadOnlyMember(nameof(CannotEditJLWReleaseGroup))]
		public override ZGuid FH_GG_ReleaseGroup
		{
			get { return base.FH_GG_ReleaseGroup; }
			set
			{
				base.FH_GG_ReleaseGroup = value;
				FH_IsReleaseGroupSetByTemplate = false;
			}
		}

		internal bool MaybeChangeReleaseGroup(Lazy<ZGuid> newReleaseGroup, bool workflowsWereAlreadyLoaded)
		{
			if (FH_P0_Template.IsEmpty)
			{
				var templateVersion = FH_ParentTemplateId.IsValid ? Factory.Load<ProcessHeader>(FH_ParentTemplateId) : null;

				if (templateVersion == null || templateVersion.FH_GG_ReleaseGroup.IsEmpty)
				{
					var shouldChange = newReleaseGroup.Value.IsValid && newReleaseGroup.Value != FH_GG_ReleaseGroup;

					if (shouldChange)
					{
						FH_GG_ReleaseGroup = newReleaseGroup.Value;
						FH_IsReleaseGroupSetByTemplate = true;

						return true;
					}
				}
			}

			return false;
		}

		#endregion

		#region FH_DateAcceptability

		[List("Lookups.DateAcceptabilities")]
		public override ZString FH_DateAcceptability
		{
			get { return base.FH_DateAcceptability; }
			set
			{
				base.FH_DateAcceptability = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateFH_AgreedDeliveryDate();
				}
			}
		}

		public virtual ZString ApplicableDateAcceptability
		{
			get { return !FH_DateAcceptability.IsEmpty ? FH_DateAcceptability : JobHeader != null ? JobHeader.ApplicableDateAcceptability : ZString.Empty; }
		}

		#endregion

		#region FH_DeadlineType

		[List("Lookups.DeadlineTypes")]
		public override ZString FH_DeadlineType
		{
			get { return base.FH_DeadlineType; }
			set
			{
				base.FH_DeadlineType = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateFH_AgreedDeliveryDate();
				}
			}
		}

		#endregion

		#region FH_BMT_BufferTimespan

		[RelatedBusinessObject("BufferTimespan")]
		[List("Lookups.BufferTimespans")]
		[CustomisedControlExclude]
		public override ZGuid FH_BMT_BufferTimespan { get => base.FH_BMT_BufferTimespan; set => base.FH_BMT_BufferTimespan = value; }

		public BMBufferTimespan BufferTimespan => Factory.Load<BMBufferTimespan>(FH_BMT_BufferTimespan);

		#endregion

		#region FH_FC_CurrentComponent

		internal IDisposable SuspendCheckingCurrentComponentSecurity()
		{
			IsCurrentComponentSecurityValidationSuspended = true;
			return new DisposableAction(() => IsCurrentComponentSecurityValidationSuspended = false);
		}

		internal bool IsCurrentComponentSecurityValidationSuspended { get; private set; }

		bool HasSecurityToChangeComponent
		{
			get { return IsCurrentComponentSecurityValidationSuspended || Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed; }
		}

		[List("BMSystem.Components")]
		[RelatedBusinessObject("CurrentComponent")]
		public override ZGuid FH_FC_CurrentComponent
		{
			get => base.FH_FC_CurrentComponent;
			set
			{
				ChangeComponent(ComponentChangeMode.ManualTransfer, value);
			}
		}

		void ChangeComponent(ComponentChangeMode mode, ZGuid newValue, BMComponentLink link = null, string reason = "")
		{
			BufferStatus bufferStatus = null;

			var currentComponent = CurrentComponent;
			if (currentComponent != null && currentComponent.IsBuffer)
			{
				var constraintStatus = ConstrainedModeHelper.GetConstraintStatus(this);
				var bufferPenetration = CalculateBufferPenetration();
				var zone = ZoneCalculator.CalculateZone(bufferPenetration.Penetration);

				bufferStatus = new BufferStatus(constraintStatus, bufferPenetration.Penetration, zone);
			}

			var oldValue = FH_FC_CurrentComponent;
			base.FH_FC_CurrentComponent = newValue;

			if (newValue != oldValue)
			{
				UpdateEffectiveBranchAndDepartment();
			}

			Factory.ClearCachedValue<string>(CurrentStatusCacheKey);

			if (HasSecurityToChangeComponent && oldValue != FH_FC_CurrentComponent)
			{
				this.OnComponentChanged(oldValue, mode, bufferStatus, link, reason);
			}

			if (!IsValidationSuspended)
			{
				Validation.ValidateFH_FC_CurrentComponent();
			}

			CurrentStatusInfo.RefreshBinding();
		}

		protected virtual bool FH_FC_CurrentComponent_ReadOnly
		{
			get { return false; }
		}

		#region Logging

		internal static void AddChangedComponentLog<TWorkflow>(TWorkflow workflow, ZGuid oldValue, ComponentChangeMode componentChangeMode, BufferStatus bufferStatus, IBMComponentLink link, string reason = "")
			where TWorkflow : EnterpriseBusinessObject, IWorkflow
		{
			var parameters = new Dictionary<string, string>
			{
				{ BMConstants.ComponentChangedEventParameters.Mode, componentChangeMode.ToCode() },
				{ BMConstants.ComponentChangedEventParameters.FromComponent, oldValue.ToString() },
				{ BMConstants.ComponentChangedEventParameters.ToComponent, workflow.CurrentComponentPK.ToString() },
				{ BMConstants.ComponentChangedEventParameters.WorkflowStatus, workflow.Status }
			};

			if (bufferStatus != null)
			{
				if (bufferStatus.ConstraintStatus != Business.ConstraintStatus.Unknown)
				{
					parameters.Add(BMConstants.ComponentChangedEventParameters.CcrStatus, bufferStatus.ConstraintStatus.ToCode());
				}

				foreach (var param in GetBufferLogInfo(bufferStatus))
				{
					parameters[param.Key] = param.Value;
				}
			}

			if (link != null)
			{
				parameters.Add(BMConstants.ComponentChangedEventParameters.ComponentLinkPk, link.PK.ToString());
			}

			if (!reason.IsNullOrEmpty())
			{
				parameters.Add(BMConstants.ComponentChangedEventParameters.Reason, reason);
			}

			var eventValue = new EventValue(Events.WorkflowTransferredBetweenSystemComponents,
				parameters: parameters,
				deferFiringWorkflow: true); // Triggers & milestones on XFR events will fire with Log Walker rather than in the main process. This is to reduce performance impact on BMS/BMG service tasks.

			workflow.Logs.AddNew(eventValue);
		}

		internal static IEnumerable<KeyValuePair<string, string>> GetBufferLogInfo(BufferStatus bufferStatus)
		{
			if (bufferStatus != null)
			{
				var maxPenetration = Math.Min(bufferStatus.BufferPenetration, 99.99m); // Maximum buffer penetration is 9999% to avoid overflow on SL_Reference.

				yield return new KeyValuePair<string, string>(BMConstants.BufferParameters.BufferPenetration, Utilities.Round(maxPenetration, 2).ToString("0.00", CultureInfo.InvariantCulture));
				yield return new KeyValuePair<string, string>(BMConstants.BufferParameters.BufferZone, bufferStatus.BufferZone.ToString(CultureInfo.InvariantCulture));
			}
		}

		void MaybeLogManualRelease()
		{
			if (FH_FC_CurrentComponentInfo.HasChanges && FH_FC_CurrentComponent.IsValid && BMSRegistry.Instance.LogTransferIntoBufferDetails.Value)
			{
				if (ShouldLogBufferReleaseDetailsWhenSaving())
				{
					var component = CurrentComponent;
					if (component.IsBuffer)
					{
						var message = Factory.IsForServiceTask(TransferRuleRunnerServiceTask.Code) ? CapacityReservationLogCreator.GetNonReleaseGateTransferMessage() : CapacityReservationLogCreator.GetManuallyReleasedMessage();
						var logger = new ReleaseGateLogger();
						logger.LogSuccessfulRelease(PK, component, message);
						logger.CommitSuccessReleaseLogs(Factory, saveFactory: false);
					}
				}
			}
		}

		bool ShouldLogBufferReleaseDetailsWhenSaving()
		{
			return Factory.IsForServiceTask(TransferRuleRunnerServiceTask.Code) || Factory.IsNotForAnyServiceTask();
		}

		#endregion

		#endregion

		#region FH_FC_DedicatedBuffer

		[ResourceStringData("ProcessHeader.DedicatedBufferName", Caption = "Dedicated Buffer", ShortCaption = "Buffer", FullDescription = "The buffer this workflow is going to be released to.")]
		[CustomisedControlExclude]
		public override ZGuid FH_FC_DedicatedBuffer
		{
			get => base.FH_FC_DedicatedBuffer;
			set
			{
				if (value != FH_FC_DedicatedBuffer)
				{
					var oldValue = FH_FC_DedicatedBuffer;
					base.FH_FC_DedicatedBuffer = value;

					UpdateEffectiveBranchAndDepartment();
					UpdateEffectiveAgreedDeliveryDate();
					UpdateReleaseSequenceSortDate();

					if (oldValue.IsEmpty && value.IsValid || oldValue.IsValid && value.IsEmpty)
					{
						UpdateEffectiveNudge();
					}
				}
			}
		}

		[ResourceStringData("ProcessHeader.DedicatedBufferName", Caption = "Dedicated Buffer", ShortCaption = "Buffer", FullDescription = "The buffer this workflow is going to be released to.")]
		[CustomisedControlExclude]
		public ZString DedicatedBufferName => DedicatedBuffer != null ? DedicatedBuffer.FC_Name : ZString.Empty;

		#endregion

		#region FH_GB_Branch And FH_GE_Department

		[CustomisedControlExclude]
		public override ZGuid FH_GB_Branch
		{
			get => base.FH_GB_Branch;
			set
			{
				if (value != FH_GB_Branch)
				{
					base.FH_GB_Branch = value;

					UpdateEffectiveBranch();
					UpdateLatestAcceptableReleaseDate();
				}
			}
		}

		[CustomisedControlExclude]
		public override ZGuid FH_GE_Department
		{
			get => base.FH_GE_Department;
			set
			{
				if (value != FH_GE_Department)
				{
					base.FH_GE_Department = value;
					UpdateEffectiveDepartment();
					UpdateLatestAcceptableReleaseDate();
				}
			}
		}

		[CustomisedControlExclude]
		public override ZGuid FH_GB_EffectiveBranch { get => base.FH_GB_EffectiveBranch; set => base.FH_GB_EffectiveBranch = value; }

		[CustomisedControlExclude]
		public override ZGuid FH_GE_EffectiveDepartment { get => base.FH_GE_EffectiveDepartment; set => base.FH_GE_EffectiveDepartment = value; }

		[ResourceStringData("ProcessHeader.EffectiveBranchName", Caption = "Effective Branch", ShortCaption = "E.Branch", FullDescription = "The effective branch which is used for calculating such properties as the latest acceptable release date and buffer penetration (it's either the branch set on the workflow or its job level workflow or the aging branch on the dedicated buffer).")]
		[CustomisedControlExclude]
		public ZString EffectiveBranchCode => EffectiveBranch != null ? EffectiveBranch.GB_Code : ZString.Empty;

		[ResourceStringData("ProcessHeader.EffectiveDepartmentName", Caption = "Effective Department", ShortCaption = "E.Department", FullDescription = "The effective department which is used for calculating such properties as the latest acceptable release date and buffer penetration (it's either the department set on the workflow or its job level workflow or the aging department on the dedicated buffer).")]
		[CustomisedControlExclude]
		public ZString EffectiveDepartmentCode => EffectiveDepartment != null ? EffectiveDepartment.GE_Code : ZString.Empty;

		void MaybeUpdateEffectiveBranchAndDepartment()
		{
			// yes, we update effective branch and department in setters of FH_FC_CurrentComponent and FH_FC_DedicatedBuffer to make UI responsive,
			// but we may reload component data from db on save as they are cached in the Uber Factory and in this case we need to update one more time
			if (FH_FC_CurrentComponentInfo.HasChanges || FH_FC_DedicatedBufferInfo.HasChanges || FH_StatusInfo.HasChanges || FH_IsActiveInfo.HasChanges)
			{
				UpdateEffectiveBranchAndDepartment();
			}
		}

		public void UpdateEffectiveBranchAndDepartment()
		{
			UpdateEffectiveBranch();
			UpdateEffectiveDepartment();
		}

		public virtual void UpdateEffectiveBranch()
		{
			if (!FH_IsActive || IsClosed)
			{
				FH_GB_EffectiveBranch = ZGuid.Empty;
				return;
			}

			FH_GB_EffectiveBranch = FH_GB_Branch.IsValid ? FH_GB_Branch
				: JobHeader != null && JobHeader.FH_GB_Branch.IsValid ? JobHeader.FH_GB_Branch
				: CurrentComponent != null && CurrentComponent.IsBuffer ? CurrentComponent.FC_GB_AgingBranch
				: DedicatedBuffer != null && DedicatedBuffer.IsBuffer ? DedicatedBuffer.FC_GB_AgingBranch
				: ZGuid.Empty;
		}

		public virtual void UpdateEffectiveDepartment()
		{
			if (!FH_IsActive || IsClosed)
			{
				FH_GE_EffectiveDepartment = ZGuid.Empty;
				return;
			}

			FH_GE_EffectiveDepartment = FH_GE_Department.IsValid ? FH_GE_Department
				: JobHeader != null && JobHeader.FH_GE_Department.IsValid ? JobHeader.FH_GE_Department
				: CurrentComponent != null && CurrentComponent.IsBuffer ? CurrentComponent.FC_GE_AgingDepartment
				: DedicatedBuffer != null && DedicatedBuffer.IsBuffer ? DedicatedBuffer.FC_GE_AgingDepartment
				: ZGuid.Empty;
		}

		#endregion

		#region FH_PlannedDurationInMinutes

		[ReadOnly(true)]
		public override ZInt FH_PlannedDurationInMinutes
		{
			get { return base.FH_PlannedDurationInMinutes; }
			set
			{
				var valueHasChanged = FH_PlannedDurationInMinutes != value;

				base.FH_PlannedDurationInMinutes = value;

				if (valueHasChanged && IsWorkflow && JobHeader != null)
				{
					JobHeader.UpdatePlannedDuration();
				}
				PlannedDurationInfo.RefreshBinding();
			}
		}

		[ResourceStringData("ProcessHeader.PlannedDuration", Caption = "Planned Duration", FullDescription = "The original planned duration of this workflow. This will remain constant once the workflow has entered a buffer component.")]
		public ZDateTime PlannedDuration
		{
			get { return FH_PlannedDurationInMinutes.GetDateTimeFromMinutes(); }
		}

		public ZPropertyInfo PlannedDurationInfo
		{
			get { return GetZPropertyInfo(nameof(PlannedDuration)); }
		}

		void UpdatePlannedDuration()
		{
			var currentComponent = CurrentComponent;
			if (!IsWorkflow || (currentComponent != null && !currentComponent.IsBuffer))
			{
				UpdatePlannedDurationCore();
			}
			else if (!IsInDatabase || FH_FC_CurrentComponentInfo.HasChanges)
			{
				var originalComponent = FH_FC_CurrentComponentInfo.HasChanges ? Factory.Load<BMComponent>((ZGuid)FH_FC_CurrentComponentInfo.OriginalValue) : null;
				if (originalComponent == null || !originalComponent.IsBuffer)
				{
					var component = CurrentComponent;
					if (component == null || component.IsBuffer)
					{
						UpdatePlannedDurationCore();
					}
				}
			}
		}

		void UpdatePlannedDurationCore()
		{
			FH_PlannedDurationInMinutes = GetPlannedDuration();
			UpdateLatestAcceptableReleaseDate();
		}

		public void UpdateLatestAcceptableReleaseDate()
		{
			var currentComponent = CurrentComponent;

			if (IsInBuffer && FH_LatestAcceptableReleaseDateUtc.IsValid)
			{
				return;
			}

			if (FH_EffectiveAgreedDeliveryDateUtc.IsEmpty)
			{
				FH_LatestAcceptableReleaseDateUtc = ZDateTime.Empty;
				return;
			}

			var context = WorkingTimeContext;

			if (context == null && DedicatedBuffer != null && DedicatedBuffer.IsBuffer)
			{
				context = DedicatedBuffer.GetRelevantContext(
					branchOverride: EffectiveBranch,
					departmentOverride: EffectiveDepartment);
			}

			if (context == null)
			{
				FH_LatestAcceptableReleaseDateUtc = ZDateTime.Empty;
				return;
			}

			var workflowDurationInMinutes = FH_PlannedDurationInMinutes;
			var bufferTimeSpanInMinutes = CalculateBufferMinutes();

			if (bufferTimeSpanInMinutes <= 0 || workflowDurationInMinutes <= 0)
			{
				FH_LatestAcceptableReleaseDateUtc = ZDateTime.Empty;
				return;
			}

			var workTimeArithmetic = context.GetWorkTimeArithmetic(Factory);
			var totalBufferTimeInHours = (bufferTimeSpanInMinutes + workflowDurationInMinutes) / 60.0;

			var latestReleaseDate = workTimeArithmetic.GetDateTimeInWorkingHoursFutureOrPast(
				FH_EffectiveAgreedDeliveryDateUtc.ToDateTime(),
				-totalBufferTimeInHours);

			FH_LatestAcceptableReleaseDateUtc = new ZDateTime(latestReleaseDate);
		}

		protected internal virtual int GetPlannedDuration()
		{
			return (int)Utilities.Round(TotalRelevantEstimatedHoursIncludingChildren * 60, 0);
		}

		#endregion

		#region FH_StaggeredReleaseDelayExpiry

		[ReadOnly(true)]
		[ResourceStringData("ProcessHeader.FH_StaggeredReleaseDelayExpiry", Caption = "Staggered Release Delay Expiry (UTC)", ShortCaption = "Release Delay Expiry (UTC)", FullDescription = "The time at which the staggered release delay expires and the workflow can be released.")]
		public override ZDateTime FH_StaggeredReleaseDelayExpiry
		{
			get { return base.FH_StaggeredReleaseDelayExpiry; }
			set
			{
				base.FH_StaggeredReleaseDelayExpiry = value;
				PrerequisiteStatusShortDescriptionInfo.RefreshBinding();
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("ProcessHeader.StaggeredReleaseDelayExpiryLocal", Caption = "Staggered Release Delay Expiry", ShortCaption = "Release Delay Expiry", FullDescription = "The time at which the staggered release delay expires and the workflow can be released.")]
		public ZDateTime StaggeredReleaseDelayExpiryLocal
		{
			get { return FH_StaggeredReleaseDelayExpiry.ToLocalBranchTime(); }
		}

		#endregion

		#region FH_IsActive

		protected virtual bool FH_IsActive_ReadOnly
		{
			get
			{
				if (FH_IsActive == true)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		[ReadOnlyMember(nameof(FH_IsActive_ReadOnly))]
		public override ZBool FH_IsActive
		{
			get => base.FH_IsActive;
			set
			{
				base.FH_IsActive = value;

				UpdateEffectiveAgreedDeliveryDate();
				UpdateReleaseSequenceSortDate();
				UpdateEffectiveBranchAndDepartment();
			}
		}

		#endregion

		#region FH_Status

		[ReadOnly(true)]
		[List("Lookups.WorkflowStatusList")]
		public override ZString FH_Status
		{
			get { return base.FH_Status; }
			set
			{
				if (base.FH_Status != value && value == WorkflowStatusList.Codes.Closed && !IsTemplate && IsWorkflow && Factory.IsNotForAnyServiceTask() && Tasks.Any(t => t.IsOpen))
				{
					return;
				}

				base.FH_Status = value;
				FH_StatusDescriptionInfo.RefreshBinding();
				PrerequisiteStatusShortDescriptionInfo.RefreshBinding();

				UpdateEffectiveAgreedDeliveryDate();
				UpdateReleaseSequenceSortDate();
				UpdateEffectiveBranchAndDepartment();
			}
		}

		[ResourceStringData("ProcessHeader.FH_StatusDescription", Caption = "Workflow Status", FullDescription = "The completeness and start-ability status of this workflow.")]
		public ZString FH_StatusDescription
		{
			get { return Lookups.WorkflowStatusList.GetDescriptionFromCode(FH_Status); }
		}

		public ZPropertyInfo FH_StatusDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(FH_StatusDescription)); }
		}

		#endregion

		#region FH_SystemLastEditTimeUtc

		public override ZDateTime FH_SystemLastEditTimeUtc
		{
			get { return base.FH_SystemLastEditTimeUtc; }
			set
			{
				base.FH_SystemLastEditTimeUtc = value;

				var workflowParent = WorkflowParent;
				if (workflowParent != null && !IsAncestorOf(workflowParent))
				{
					workflowParent.FH_SystemLastEditTimeUtc = value;
				}
			}
		}

		public void UpdateLastEditTimeIfNotDeleting()
		{
			if (!IsDeletingNow)
			{
				var now = ZDateTime.UtcNow;
				FH_SystemLastEditTimeUtc = now;

				var jobHeader = this as ProcessJobHeader;
				if (jobHeader != null)
				{
					foreach (var workflow in jobHeader.ProcessHeaders)
					{
						workflow.FH_SystemLastEditTimeUtc = now;
					}
				}
			}
		}

		#endregion

		#region FH_TimeDelayFactor

		[ReadOnly(true)]
		public override ZDecimal FH_TimeDelayFactor
		{
			get { return base.FH_TimeDelayFactor; }
			set { base.FH_TimeDelayFactor = value; }
		}

		#endregion

		#region FH_TimeDelayMinutes

		[ReadOnly(true)]
		public override ZInt FH_TimeDelayMinutes
		{
			get { return base.FH_TimeDelayMinutes; }
			set { base.FH_TimeDelayMinutes = value; }
		}

		#endregion

		#region FH_WorkflowType

		[ReadOnly(true)]
		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public override ZString FH_WorkflowType
		{
			get { return base.FH_WorkflowType; }
			set { base.FH_WorkflowType = value; }
		}

		#endregion

		#region FH_LastTransferType

		[ReadOnly(true)]
		[ResourceStringData("ProcessHeader.FH_LastTransferType", Caption = "Last Transfer Type")]
		public override ZString FH_LastTransferType
		{
			get { return base.FH_LastTransferType; }
			set { base.FH_LastTransferType = value; }
		}

		[ResourceStringData("ProcessHeader.LastTransferTypeDescription", Caption = "Last Transfer Type Description")]
		public ZString LastTransferTypeDescription => Lookups.TransferTypeList.GetDescriptionFromCode(FH_LastTransferType);

		#endregion

		#region FH_AgreedDeliveryDateDefaultsFrom

		[List("Lookups.DatesDefaultsFromList")]
		public override ZString FH_AgreedDeliveryDateDefaultsFrom
		{
			get { return base.FH_AgreedDeliveryDateDefaultsFrom; }
			set
			{
				base.FH_AgreedDeliveryDateDefaultsFrom = value;
				if (FH_AgreedDeliveryDateDefaultsFrom.IsEmpty && FH_AgreedDeliveryDateDefaultHoursOffset.IsValid)
				{
					FH_AgreedDeliveryDateDefaultHoursOffset = ZDateTime.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateFH_AgreedDeliveryDateDefaultHoursOffset();
					Validation.ValidateFH_AgreedDeliveryDate();
				}
			}
		}

		#endregion

		#region FH_AgreedDeliveryDateDefaultHoursOffset

		[ZDateTimeOffsetValueNegatable]
		public override ZDateTime FH_AgreedDeliveryDateDefaultHoursOffset
		{
			get => base.FH_AgreedDeliveryDateDefaultHoursOffset;
			set => base.FH_AgreedDeliveryDateDefaultHoursOffset = value.ConvertToDurationBasedDate(FH_AgreedDeliveryDateDefaultHoursOffsetInfo);
		}

		#endregion

		#region FH_EarliestStartDateDefaultsFrom

		[List("Lookups.DatesDefaultsFromList")]
		public override ZString FH_EarliestStartDateDefaultsFrom
		{
			get { return base.FH_EarliestStartDateDefaultsFrom; }
			set
			{
				base.FH_EarliestStartDateDefaultsFrom = value;
				if (FH_EarliestStartDateDefaultsFrom.IsEmpty && FH_EarliestStartDefaultHoursOffset.IsValid)
				{
					FH_EarliestStartDefaultHoursOffset = ZDateTime.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateFH_EarliestStartDefaultHoursOffset();
					Validation.ValidateFH_DoNotStartBeforeDate();
				}
			}
		}

		#endregion

		#region FH_EarliestStartDefaultHoursOffset

		[ZDateTimeOffsetValueNegatable]
		public override ZDateTime FH_EarliestStartDefaultHoursOffset
		{
			get => base.FH_EarliestStartDefaultHoursOffset;
			set => base.FH_EarliestStartDefaultHoursOffset = value.ConvertToDurationBasedDate(FH_EarliestStartDefaultHoursOffsetInfo);
		}

		#endregion

		#region FH_Category

		[List("Lookups.WorkflowCategories")]
		[ReadOnlyMember(nameof(FH_Category_ReadOnly))]
		[ResourceStringData("ProcessHeader.FH_Category", Caption = "Category", FullDescription = "The Categories can be configured in the Registry under Workflow Manager > Buffer Management > Workflow Categories")]
		public override ZString FH_Category
		{
			get => base.FH_Category;
			set => base.FH_Category = value;
		}

		[ResourceStringData("ProcessHeader.CategoryDescription", Caption = "Category Description", FullDescription = "The Categories can be configured in the Registry under Workflow Manager > Buffer Management > Workflow Categories")]
		public ZString CategoryDescription => ((ICodeDescriptionPairList)Lookups.WorkflowCategories).GetDescriptionFromCode(FH_Category);

		protected virtual bool FH_Category_ReadOnly => false;

		#endregion

		#region FH_TaskPenetrationResetDateTimeUtc

		[ReadOnly(true)]
		[ResourceStringData("ProcessHeader.FH_TaskPenetrationResetDateTimeUtc", Caption = "Task Penetration Reset Time (UTC)")]
		public override ZDateTime FH_TaskPenetrationResetDateTimeUtc
		{
			get => base.FH_TaskPenetrationResetDateTimeUtc;
			set => base.FH_TaskPenetrationResetDateTimeUtc = value;
		}

		#endregion

		#region FH_IsApproved

		protected virtual bool FH_IsApproved_ReadOnly => true;

		#endregion

		#region Workflow Descriptor

		public WorkflowDescriptor WorkflowDescriptor
		{
			get
			{
				var templateWorkflowType = IsTemplate ? Template.P0_ProcessType : FH_WorkflowType;
				if (workflowDescriptor == null
					|| (workflowDescriptor.Code != templateWorkflowType))
				{
					workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(templateWorkflowType);
				}
				return workflowDescriptor;
			}
		}
		WorkflowDescriptor workflowDescriptor;

		#endregion

		#region IsReleased

		[CustomisedControlExclude]
		[ResourceStringData("ProcessHeader.IsReleased", Caption = "Released", FullDescription = "Indicates whether this workflow is currently in a buffer component.")]
		public virtual ZBool IsReleased
		{
			get
			{
				return FH_ReleaseDateTime.IsValid
					&& CurrentComponent != null
					&& CurrentComponent.IsBuffer;
			}
		}

		#endregion

		#region Source Template

		[List("Lookups.Templates")]
		[ResourceStringData("ProcessHeader.SourceTemplatePK", Caption = "Source Template", FullDescription = "The Workflow Template which this item was copied from.")]
		public ZGuid SourceTemplatePK
		{
			get
			{
				if (FH_ParentTemplateId.IsValid)
				{
					return SourceTemplateProcessHeader != null ? SourceTemplateProcessHeader.FH_P0_Template : ZGuid.Invalid;
				}
				else
				{
					return FH_ParentTemplateId.IsEmpty ? ZGuid.Empty : ZGuid.Invalid;
				}
			}
		}

		public ProcessHeader SourceTemplateProcessHeader
		{
			get
			{
				if (FH_ParentTemplateId.IsValid && sourceTemplateProcessHeader == null)
				{
					sourceTemplateProcessHeader = Factory.Load<ProcessHeader>(FH_ParentTemplateId);
				}
				return sourceTemplateProcessHeader;
			}
		}
		ProcessHeader sourceTemplateProcessHeader;

		#endregion

		#region Job Properties

		public void UpdateJobProperties()
		{
			if (FH_FH_ParentHeader == ZGuid.Empty && Parent != null)
			{
				var jobHeader = this as IProcessJobHeader;

				var newCodeValue = ZString.Empty;
				var newDescValue = ZString.Empty;
				var codeChanged = false;
				var descChanged = false;

				if (Parent.GetType().IsDefined(typeof(CodePropertyAttribute)))
				{
					newCodeValue = CodePropertyAttribute.CodeFromBusinessObject(Parent as BusinessObject).Truncate(Schema.FH_JobCodeMaxLength);
					if (FH_JobCode != newCodeValue)
					{
						codeChanged = true;
						FH_JobCode = newCodeValue;
					}
				}
				if (Parent.GetType().IsDefined(typeof(DescriptionPropertyAttribute)))
				{
					newDescValue = DescriptionPropertyAttribute.DescriptionFromBusinessObject(Parent as BusinessObject).Truncate(Schema.FH_JobDescriptionMaxLength);
					if (FH_JobDescription != newDescValue)
					{
						descChanged = true;
						FH_JobDescription = newDescValue;
					}
				}

				if (jobHeader != null)
				{
					foreach (ProcessHeader header in jobHeader.ProcessHeaders)
					{
						if (codeChanged)
						{
							header.FH_JobCode = newCodeValue;
						}
						if (descChanged)
						{
							header.FH_JobDescription = newDescValue;
						}
					}
				}
			}
		}

		#endregion

		#endregion

		#region New Properties

		#region Release sequence

		[ReadOnly(true)]
		[CustomisedControlExclude]
		[ResourceStringData("ProcessHeader.ReleaseSequence", ShortCaption = "Release", Caption = "Release Sequence", FullDescription = "The order in which workflows are released into a Buffer.")]
		[MaxLength(BMSystem.Schema.FS_NameMaxLength + 6)]
		public ZString ReleaseSequence
		{
			get { return releaseSequence; }
			set { SetNonPersistentPropertyValue(ReleaseSequenceInfo, ref releaseSequence, value, false); }
		}

		ZString releaseSequence;

		public ZPropertyInfo ReleaseSequenceInfo
		{
			get { return GetZPropertyInfo(nameof(ReleaseSequence)); }
		}

		#endregion

		#region Release Notes

		public string GetReleaseFailureReasonForBuffer(BMComponent buffer)
		{
			if (FH_FC_CurrentComponent == buffer.PK)
			{
				return string.Empty;
			}

			var failureLogService = ReleaseGateFailureLogService.GetOrAddReleaseGateFailureLogService(Factory);
			var logs = failureLogService.GetReleaseFailureLogs(buffer.PK, PK);

			if (logs == null || logs.Length == 0)
			{
				return GetReleaseFailureReasonNotAvailableMessage();
			}

			return string.Join(System.Environment.NewLine, logs);
		}

		string GetReleaseFailureReasonNotAvailableMessage() => $"{ReleaseFailureReasonNotAvailableMessage}{(FH_IsActive ? string.Empty : System.Environment.NewLine + WorkflowIsDeactivatedMessage)}";

		public static string ReleaseFailureReasonNotAvailableMessage => Res.GetString("dc5213e7-66bf-4d08-bc1b-e21f8e9a1d65", "Details are not available until the next time the service task runs.");

		static string WorkflowIsDeactivatedMessage => Res.GetString("71229A45-87EF-4CE2-9944-F70471FF5756", "The workflow is currently deactivated and ignored by Release Gate.");

		public string GetSuccessfulReleaseNotes()
		{
			var note = GetSuccessfulReleaseNote(Factory);

			return note == null ? string.Empty : note.ST_NoteText.ToString();
		}

		#region Implementation

#if DEBUG
		public
#endif
		StmNote GetSuccessfulReleaseNote(BusinessObjectFactory factory)
		{
			return factory.LoadTop1<StmNote>(GetSuccessfulReleaseNoteQuery());
		}

		ZQuery GetSuccessfulReleaseNoteQuery()
		{
			var query = new ZQuery(StmNoteSchema.ST_ParentID, PK);
			query.AddToFilter(StmNoteSchema.ST_NoteType, BMConstants.LastSuccessfulReleaseNoteType);
			query.AddToFilter(StmNoteSchema.ST_Table, Schema.TableName);

			return query;
		}

		#endregion

		#endregion

		#region Relationships

		[ResourceStringData("cdcdf948-3032-4e89-8271-cc77e15b3f71", Caption = "Has Open Prerequisites")]
		[VisualBoardSearchable]
		public ZBool HasOpenPrerequisites
		{
			get { return HasChanges ? GetHasOpenPrerequisites() : FH_Status == WorkflowStatusList.Codes.Blocked; }
		}

		internal bool GetHasOpenPrerequisites()
		{
			return GetPrerequisitesUpTheTree(true).Any(w => w.GetIsOpen());
		}

		public IEnumerable<ProcessHeader> GetPrerequisitesUpTheTree(bool getApplicableDependenciesOnly = false)
		{
			return GetPrerequisitesAndTheirLinksUpTheTree(getApplicableDependenciesOnly).Select(x => x.ProcessHeader);
		}

		public IEnumerable<LinkedProcessHeader> GetPrerequisitesAndTheirLinksUpTheTree(bool getApplicableDependenciesOnly = false)
		{
			return GetPrerequisitesUpTheTreeCore(this, new HashSet<ProcessHeader>(), getApplicableDependenciesOnly);
		}

		static IEnumerable<LinkedProcessHeader> GetPrerequisitesUpTheTreeCore(ProcessHeader workflow, HashSet<ProcessHeader> foundPrerequisites, bool getApplicableDependenciesOnly)
		{
			var strategy = new ProcessHeaderDescendantsStrategy();
			var applicabilityFunc = new Func<ILink, bool>(l => ((ProcessHeaderLink)l).FP_LinkType == ProcessHeaderLinkTypeList.Codes.Dependency);

			var ancestors = ((ILinkEntity)workflow).Ancestors(strategy);

			var prereqLinksIncludingAncestorsLinks = workflow.PrerequisiteLinks.Union(
				ancestors.SelectMany(a => ((ProcessHeader)a).PrerequisiteLinks.Where(l => l.HeaderFrom != null)));

			var applicableLinks = prereqLinksIncludingAncestorsLinks.Where(l => !getApplicableDependenciesOnly || l.IsApplicableDependencyToEntity(workflow, strategy, applicabilityFunc: applicabilityFunc));

			foreach (var link in applicableLinks)
			{
				var prereq = link.HeaderFrom;
				if (prereq != null && !foundPrerequisites.Contains(prereq))
				{
					foundPrerequisites.Add(prereq);
					yield return new LinkedProcessHeader(prereq, link);

					foreach (var nextLevelPrereq in GetPrerequisitesUpTheTreeCore(prereq, foundPrerequisites, getApplicableDependenciesOnly))
					{
						yield return nextLevelPrereq;
					}
				}
			}
		}

		public IEnumerable<LinkedProcessHeader> GetPostRequisitesAndTheirLinksDownTheTree()
		{
			return GetPostRequisitesDownTheTreeCore(this, new HashSet<ProcessHeader>());
		}

		static IEnumerable<LinkedProcessHeader> GetPostRequisitesDownTheTreeCore(ProcessHeader workflow, HashSet<ProcessHeader> foundPostRequisites)
		{
			foreach (var link in workflow.PostrequisiteLinks.Union(workflow.JobHeader.PostrequisiteLinks))
			{
				var postreq = link.HeaderTo;
				if (postreq != null && !foundPostRequisites.Contains(postreq))
				{
					foundPostRequisites.Add(postreq);
					yield return new LinkedProcessHeader(postreq, link);

					foreach (var nextLevelPrereq in GetPostRequisitesDownTheTreeCore(postreq, foundPostRequisites))
					{
						yield return nextLevelPrereq;
					}
				}
			}
		}

		public IEnumerable<ProcessHeader> GetChildWorkflowsDownTheHierarchy()
		{
			return GetChildrenDownTheHierarchy().Select(l => l.ProcessHeader);
		}

		public IEnumerable<LinkedProcessHeader> GetChildrenDownTheHierarchy()
		{
			return GetChildWorkflowsDownTheHierarchy(this, new HashSet<ProcessHeader>());
		}

		static IEnumerable<LinkedProcessHeader> GetChildWorkflowsDownTheHierarchy(ProcessHeader workflow, HashSet<ProcessHeader> foundChildren)
		{
			foreach (var link in workflow.ChildLinks)
			{
				var child = link.HeaderFrom;

				if (!foundChildren.Contains(child))
				{
					foundChildren.Add(child);

					yield return new LinkedProcessHeader(child, link);

					foreach (var nextLevelChild in GetChildWorkflowsDownTheHierarchy(child, foundChildren))
					{
						yield return nextLevelChild;
					}
				}
			}
		}

		public IEnumerable<LinkedProcessHeader> GetImmediateChildren()
		{
			foreach (var link in ChildLinks)
			{
				var child = link.HeaderFrom;

				yield return new LinkedProcessHeader(child, link);
			}
		}

		public IEnumerable<LinkedProcessHeader> GetParentsUpTheHierarchy(HashSet<ZGuid> existingParent = null)
		{
			HashSet<ZGuid> currentExistingParent = existingParent == null
				? new HashSet<ZGuid>(new[] { PK }) // current workflow should not be its own parent
				: new HashSet<ZGuid>(existingParent);

			foreach (var parent in GetParentsUpTheHierarchyCore(ParentLinks, currentExistingParent))
			{
				yield return parent;
			}

			if (IsWorkflow)
			{
				foreach (var parent in GetParentsUpTheHierarchyCore(JobHeader.ParentLinks, currentExistingParent))
				{
					yield return parent;
				}
			}
		}

		IEnumerable<LinkedProcessHeader> GetParentsUpTheHierarchyCore(IEnumerable<ProcessHeaderLink> processHeaderLinks, HashSet<ZGuid> currentExistingParent)
		{
			foreach (var link in processHeaderLinks.WhereNotNull())
			{
				var workflow = link.HeaderTo;

				if (workflow != null)
				{
					if (!currentExistingParent.Add(workflow.PK))
					{
						yield break;
					}
					else
					{
						yield return new LinkedProcessHeader(workflow, link);
					}

					foreach (var grandparent in workflow.GetParentsUpTheHierarchy(currentExistingParent))
					{
						yield return grandparent;
					}
				}
			}
		}

		public IEnumerable<LinkedProcessHeader> GetImmediateParents()
		{
			foreach (var link in ParentLinks)
			{
				var parent = link.HeaderFrom;

				yield return new LinkedProcessHeader(parent, link);
			}
		}

		IEnumerable<ProcessHeader> GetRelatedReleaseSequenceProcessHeaders(ProcessHeader current, int depth)
		{
			yield return current;

			if (current.JobHeader != null && current != current.JobHeader)
			{
				yield return current.JobHeader;

				if (depth < BMConstants.MaximumReleaseSequenceParentWorkflowsDepth)
				{
					foreach (var parent in current.JobHeader.ParentLinks)
					{
						foreach (var header in GetRelatedReleaseSequenceProcessHeaders(parent.HeaderTo, depth + 1))
						{
							yield return header;
						}
					}
				}
			}

			if (depth < BMConstants.MaximumReleaseSequenceParentWorkflowsDepth)
			{
				foreach (var parent in current.ParentLinks)
				{
					foreach (var header in GetRelatedReleaseSequenceProcessHeaders(parent.HeaderTo, depth + 1))
					{
						yield return header;
					}
				}
			}
		}

		#endregion

		#region Descriptions

		public ZString Code => FH_CompletionStatement;

		public ZString CodeWithParentCode
		{
			get
			{
				var parentObject = Parent as BusinessObject;
				if (parentObject != null)
				{
					var code = CodePropertyAttribute.CodeFromBusinessObject(parentObject);
					if (!string.IsNullOrEmpty(code))
					{
						return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", FH_CompletionStatement, code);
					}
				}

				return Description;
			}
		}

		[ResourceStringData("ProcessHeader|Description", Caption = "Workflow Description", ShortCaption = "Desc.", FullDescription = "The job details and completion statement of this workflow.")]
		public ZString Description
		{
			get { return (!ParentJobDescription.IsEmpty ? (ParentJobDescription + " - ") : string.Empty) + FH_CompletionStatement; }
		}

		public ZString DescriptionWithReleaseGroup
		{
			get
			{
				var releaseGroup = ReleaseGroup;
				if (releaseGroup != null)
				{
					return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", Description, releaseGroup.GG_Desc);
				}
				else
				{
					return Description;
				}
			}
		}

		[ResourceStringData("ProcessHeader|ParentJobDescription", Caption = "Job", FullDescription = "The job that this workflow is attached to.")]
		public ZString ParentJobDescription
		{
			get
			{
				var parentObject = Parent as BusinessObject;
				return parentObject != null ? parentObject.HumanReadableName : ZString.Empty;
			}
		}

		[ResourceStringData("ProcessHeader|ProviderJobNumber", Caption = "Job Number")]
		public ZString ProviderJobNumber
		{
			get
			{
				if (Parent != null)
				{
					try
					{
						return CodePropertyAttribute.CodeFromBusinessObject((BusinessObject)(Parent));
					}
					catch (NoCodePropertyException)
					{
						return ZString.Empty;
					}
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo ProviderJobNumberInfo
		{
			get { return GetZPropertyInfo(nameof(ProviderJobNumber)); }
		}

		[ResourceStringData("ProcessHeader|ProviderJobDescription", Caption = "Job Description")]
		public ZString ProviderJobDescription
		{
			get
			{
				if (Parent != null)
				{
					try
					{
						return DescriptionPropertyAttribute.DescriptionFromBusinessObject((BusinessObject)(Parent));
					}
					catch (NoCodePropertyException)
					{
						return ZString.Empty;
					}
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo ProviderJobDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ProviderJobDescription)); }
		}

		[ResourceStringData("ProcessHeader.CompletionCriteria", Caption = "Completion Statements")]
		public ZString CompletionCriteria
		{
			get
			{
				if (Parent == null)
				{
					return ZString.Empty; // Can happen silently when navigating on Workflow Relationships form, and re-binding property after removing current workflow from binding collection.
				}

				var descriptions = CompletionStatementTasksIncludingChildWorkflowTasks
					.Cast<ProcessTask>()
					.OrderBy(t => t.P9_Sequence)
					.Select(t => t.P9_NotesAsString);

				return string.Join(System.Environment.NewLine, descriptions);
			}
		}

		public ZPropertyInfo CompletionCriteriaInfo
		{
			get { return GetZPropertyInfo(nameof(CompletionCriteria)); }
		}

		#endregion

		#region Estimates

		#region Total Relevant Estimated Hours

		[ResourceStringData("ProcessHeader.TotalRelevantEstimatedHoursSummary", Caption = "Relevant Estimate Hours Summary", ShortCaption = "Est. Hours", FullDescription = "The total estimates of the tasks on this workflow, either the Standard Estimates or the Estimated Time to Complete if specified.")]
		public ZString TotalRelevantEstimatedHoursSummary => ConvertHoursToSummary(TotalRelevantEstimatedHours);

		[ResourceStringData("ProcessHeader.TotalRelevantEstimatedHoursLabel", Caption = "Relevant Estimate Hours", ShortCaption = "Est. Hours", FullDescription = "The total estimates of the tasks on this workflow, either the Standard Estimates or the Estimated Time to Complete if specified.")]
		public ZString TotalRelevantEstimatedHoursLabel => ConvertHoursToLabel(TotalRelevantEstimatedHours);

		[CustomisedControlExclude]
		[ResourceStringData("ProcessHeader.TotalRelevantEstimatedHours", Caption = "Relevant Estimate Hours", ShortCaption = "Est. Hours", FullDescription = "The total estimates of the tasks on this workflow, either the Standard Estimates or the Estimated Time to Complete if specified.")]
		public virtual ZDecimal TotalRelevantEstimatedHours
		{
			get { return GetRelevantTasksForEstimates().Sum(t => t.RelevantEstimateHours); }
		}

		[ResourceStringData("ProcessHeader.TotalRelevantEstimatedHoursIncludingChildrenSummary", Caption = "Relevant Estimate Hours (including children)", ShortCaption = "Est. Hours (incl. children)", FullDescription = "The total estimates of the tasks on this workflow and child workflows, either the Standard Estimates or the Estimated Time to Complete if specified.")]
		public ZString TotalRelevantEstimatedHoursIncludingChildrenSummary => ConvertHoursToSummary(TotalRelevantEstimatedHoursIncludingChildren);

		[ResourceStringData("ProcessHeader.TotalRelevantEstimatedHoursIncludingChildrenLabel", Caption = "Relevant Estimate Hours (including children)", ShortCaption = "Est. Hours (incl. children)", FullDescription = "The total estimates of the tasks on this workflow and child workflows, either the Standard Estimates or the Estimated Time to Complete if specified.")]
		public ZString TotalRelevantEstimatedHoursIncludingChildrenLabel => ConvertHoursToLabel(TotalRelevantEstimatedHoursIncludingChildren);

		[CustomisedControlExclude]
		[ResourceStringData("ProcessHeader.TotalRelevantEstimatedHoursIncludingChildren", Caption = "Relevant Estimate Hours (including children)", ShortCaption = "Est. Hours (incl. children)", FullDescription = "The total estimates of the tasks on this workflow and child workflows, either the Standard Estimates or the Estimated Time to Complete if specified.")]
		public ZDecimal TotalRelevantEstimatedHoursIncludingChildren
		{
			get { return TotalRelevantEstimatedHours + GetChildWorkflowsDownTheHierarchy().Sum(w => w.TotalRelevantEstimatedHours); }
		}

		#endregion

		public ZDecimal TotalNonCancelledEstimatedHoursIncludingChildren
		{
			get
			{
				return GetCurrentAndChildrenWorkflows(onlyOpenWorkflows: false)
					.SelectMany(workflow => workflow.GetRelevantTasksForEstimates())
					.Where(task => task.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled)
					.Sum(task => task.RelevantEstimateHours);
			}
		}

		#region Total Estimated Hours

		[ResourceStringData("ProcessHeader.TotalEstimatedHoursSummary", ShortCaption = "Estimate Summary", Caption = "Low to High Estimate Summary (including children)", FullDescription = "The total estimate range of the tasks on this workflow and child workflows.")]
		public virtual ZString TotalEstimatedHoursSummary
		{
			get
			{
				var tasksIncludingChildren = GetCurrentAndChildrenWorkflows(onlyOpenWorkflows: false)
					.SelectMany(w => w.Tasks)
					.ToArray();

				var totalLowEstimate = (ZDecimal)tasksIncludingChildren.Sum(x => x.LowEstimatedDurationHours);
				var totalHighEstimate = (ZDecimal)tasksIncludingChildren.Sum(x => x.HighEstimatedDurationHours);
				var totalStandardEstimate = (ZDecimal)tasksIncludingChildren.Sum(x => x.StandardEstimateHours);

				return Res.GetString("6948d6d9-17e4-4de4-9742-ca2a3547da01", "{0} hrs to {1} hrs ({2} standard estimate).", totalLowEstimate.ToString(1), totalHighEstimate.ToString(1), totalStandardEstimate.ToString(1));
			}
		}

		public ZDecimal TotalLowEstimate => GetTotalEstimate((x) => x.LowEstimatedDurationHours);
		public ZDecimal TotalHighEstimate => GetTotalEstimate((x) => x.HighEstimatedDurationHours);
		public ZDecimal TotalStandardEstimate => GetTotalEstimate((x) => x.StandardEstimateHours);

		ZDecimal GetTotalEstimate(Func<ProcessTask, ZDecimal> estimateHours)
		{
			return (ZDecimal)GetCurrentAndChildrenWorkflows(onlyOpenWorkflows: false).SelectMany(w => w.Tasks).Sum(x => estimateHours(x));
		}

		public ZDecimal PreviousTotalLowEstimate
		{
			get
			{
				if (previousTotalLowEstimate == null)
				{
					previousTotalLowEstimate = TotalLowEstimate;
				}
				return previousTotalLowEstimate;
			}
			set
			{
				previousTotalLowEstimate = value;
				PreviousTotalLowEstimateInfo.RefreshBinding();
			}
		}
		ZDecimal previousTotalLowEstimate;

		public ZDecimal PreviousTotalHighEstimate
		{
			get
			{
				if (previousTotalHighEstimate == null)
				{
					previousTotalHighEstimate = TotalHighEstimate;
				}
				return previousTotalHighEstimate;
			}
			set
			{
				previousTotalHighEstimate = value;
				PreviousTotalHighEstimateInfo.RefreshBinding();
			}
		}
		ZDecimal previousTotalHighEstimate;

		public ZPropertyInfo PreviousTotalLowEstimateInfo => GetZPropertyInfo(nameof(PreviousTotalLowEstimate));

		public ZPropertyInfo PreviousTotalHighEstimateInfo => GetZPropertyInfo(nameof(PreviousTotalHighEstimate));

		#endregion

		[CustomisedControlExclude]
		[ResourceStringData("ProcessHeader.ConstraintStatus", Caption = "Constraint Status", FullDescription = "Indicates workflow status based on CCR (Capacity Constraint Resource).")]
		public virtual ZString ConstraintStatus
		{
			get
			{
				var constraintStatus = ConstrainedModeHelper.GetConstraintStatus(this);
				return constraintStatus.ToCode();
			}
		}

		[ResourceStringData("ProcessHeader.OverallEstimateFactor", ShortCaption = "Estimate Factor", Caption = "Overall Estimate Variation Factor", FullDescription = "The overall factor of low to high estimates for this workflow.")]
		public virtual ZDecimal OverallEstimateFactor
		{
			get
			{
				var totalLowEstimate = GetRelevantTasksForEstimates().Sum(x => x.LowEstimatedDurationHours);
				var totalHighEstimate = GetRelevantTasksForEstimates().Sum(x => x.HighEstimatedDurationHours);

				if (totalHighEstimate > 0 && totalLowEstimate > 0)
				{
					return totalHighEstimate / totalLowEstimate;
				}
				return ZDecimal.Zero;
			}
		}

		#region Total Actual Hours

		[ResourceStringData("ProcessHeader.TotalActualHoursSummary", ShortCaption = "Actual Hours", Caption = "Actual Hours", FullDescription = "The total actual time recorded against all tasks on this workflow.")]
		public virtual ZString TotalActualHoursSummary => ConvertHoursToSummary(TotalActualHours);

		[ResourceStringData("ProcessHeader.TotalActualHoursLabel", ShortCaption = "Actual Hrs.", Caption = "Actual Hours", FullDescription = "The total actual time recorded against all tasks on this workflow.")]
		public ZString TotalActualHoursLabel => ConvertHoursToLabel(TotalActualHours);

		[CustomisedControlExclude]
		[ResourceStringData("ProcessHeader.TotalActualHours", ShortCaption = "Actual Hrs.", Caption = "Actual Hours", FullDescription = "The total actual time recorded against all tasks on this workflow.")]
		public virtual ZDecimal TotalActualHours
		{
			get { return GetRelevantTasksForEstimates().Where(x => !x.P9_ActualDuration.IsEmpty).Sum(x => x.ActualDurationHours); }
		}

		[ResourceStringData("ProcessHeader.TotalActualHoursIncludingChildrenSummary", ShortCaption = "Actual Hrs. Incl. Children", Caption = "Actual Hours Including Child Workflows", FullDescription = "The total actual time recorded against all tasks on this workflow and all child workflows.")]
		public ZString TotalActualHoursIncludingChildrenSummary => ConvertHoursToSummary(TotalActualHoursIncludingChildren);

		[ResourceStringData("ProcessHeader.TotalActualHoursIncludingChildrenLabel", ShortCaption = "Actual Hrs. Incl. Children", Caption = "Actual Hours Including Child Workflows", FullDescription = "The total actual time recorded against all tasks on this workflow and all child workflows.")]
		public ZString TotalActualHoursIncludingChildrenLabel => ConvertHoursToLabel(TotalActualHoursIncludingChildren);

		[CustomisedControlExclude]
		[ResourceStringData("ProcessHeader.TotalActualHoursIncludingChildrenDateTime", ShortCaption = "Actual Hrs. Incl. Children", Caption = "Actual Hours Including Child Workflows", FullDescription = "The total actual time recorded against all tasks on this workflow and all child workflows.")]
		public ZDateTime TotalActualHoursIncludingChildrenDateTime => ConvertHoursToDatetime(TotalActualHoursIncludingChildren);

		[CustomisedControlExclude]
		[ResourceStringData("ProcessHeader.TotalActualHoursIncludingChildren", ShortCaption = "Actual Hrs. Incl. Children", Caption = "Actual Hours Including Child Workflows", FullDescription = "The total actual time recorded against all tasks on this workflow and all child workflows.")]
		public ZDecimal TotalActualHoursIncludingChildren => Factory.GetCachedValue((PK, nameof(TotalActualHoursIncludingChildren)), () => GetCurrentAndChildrenWorkflows(onlyOpenWorkflows: false).Sum(workflow => workflow.TotalActualHours), CacheStalenessPolicy.StaleOnFactorySave);

		#endregion

		#region Remaining Estimate Hours

		[ResourceStringData("ProcessHeader.RemainingEstimateHoursSummary", Caption = "Remaining Estimate Hours", ShortCaption = "Remaining Hours", FullDescription = "The total of the estimate hours of all open tasks in this workflow.")]
		public ZString RemainingEstimateHoursSummary => ConvertHoursToSummary(RemainingEstimateHours);

		[ResourceStringData("ProcessHeader.RemainingEstimateHoursLabel", Caption = "Remaining Estimate Hours", ShortCaption = "Remaining Hours", FullDescription = "The total of the estimate hours of all open tasks in this workflow.")]
		public ZString RemainingEstimateHoursLabel => ConvertHoursToLabel(RemainingEstimateHours);

		[CustomisedControlExclude]
		[ResourceStringData("ProcessHeader.RemainingEstimateHours", Caption = "Remaining Estimate Hours", ShortCaption = "Remaining Hours", FullDescription = "The total of the estimate hours of all open tasks in this workflow.")]
		public ZDecimal RemainingEstimateHours
		{
			get
			{
				return GetRelevantTasksForEstimates()
					 .Where(task => task.IsOpen)
					 .Sum(task => task.RelevantEstimateHours);
			}
		}

		[ResourceStringData("ProcessHeader.RemainingEstimateHoursIncludingChildrenSummary", ShortCaption = "Remaining Hrs. Incl. Children", Caption = "Remaining Estimate Hours Including Child Workflows", FullDescription = "The total of the estimate hours of all open tasks in this workflow and all child workflows.")]
		public ZString RemainingEstimateHoursIncludingChildrenSummary => ConvertHoursToSummary(RemainingEstimateHoursIncludingChildren);

		[ResourceStringData("ProcessHeader.RemainingEstimateHoursIncludingChildrenLabel", ShortCaption = "Remaining Hrs. Incl. Children", Caption = "Remaining Estimate Hours Including Child Workflows", FullDescription = "The total of the estimate hours of all open tasks in this workflow and all child workflows.")]
		public ZString RemainingEstimateHoursIncludingChildrenLabel => ConvertHoursToLabel(RemainingEstimateHoursIncludingChildren);

		[CustomisedControlExclude]
		public ZDecimal RemainingEstimateHoursIncludingChildren => GetCurrentAndChildrenWorkflows(onlyOpenWorkflows: true).Sum(workflow => workflow.RemainingEstimateHours);

		#endregion

		IEnumerable<ProcessHeader> GetCurrentAndChildrenWorkflows(bool onlyOpenWorkflows)
		{
			var currentAndChildrenWorkflows = new List<ProcessHeader>() { this };

			var currentIndex = 0;
			while (currentIndex < currentAndChildrenWorkflows.Count)
			{
				var current = currentAndChildrenWorkflows[currentIndex];

				Factory.AddFetchHint(ProcessHeaderSchema.Instance, new ZQuery(ProcessHeaderSchema.PK, current.ChildLinks.Select(l => l.FP_FH_HeaderFrom)));

				var children = current.ChildLinks
					.Select(link => link.HeaderFrom)
					.WhereNotNull()
					.Where(workflow => !onlyOpenWorkflows || workflow.IsOpen);

				currentAndChildrenWorkflows.AddRange(children.Distinct().Except(currentAndChildrenWorkflows).ToArray());

				currentIndex++;
			}

			return currentAndChildrenWorkflows;
		}

		[ResourceStringData("ProcessHeader.ImplicitDurationHours", Caption = "Implicit Duration", FullDescription = "The shortest estimated time required to completion.")]
		public ZDecimal ImplicitDurationHours
		{
			get { return GetRelevantTasksForEstimates().Sum(t => t.StandardEstimateHours); }
		}

		protected virtual IEnumerable<ProcessTask> GetRelevantTasksForEstimates()
		{
			return GetTasksWithoutAccessingWorkflowParent();
		}

		string ConvertHoursToSummary(ZDecimal hours) => Res.GetString("f1ca8de5-c89c-4948-8a61-0234036e17ff", "{0} hrs.", hours.ToString(1));

		string ConvertHoursToLabel(ZDecimal hours) => TimeSpan.FromHours((double)hours).ToHoursAndMinutesString();

		ZDateTime ConvertHoursToDatetime(ZDecimal hours) => new ZInt(Convert.ToInt32(hours * 60)).GetDateTimeFromMinutes();

		#endregion

		#region Effective Buffer Duration
		public TimeSpan EffectiveBufferDuration
		{
			get
			{
				if (this is ProcessJobHeader)
				{
					return TimeSpan.Zero;
				}

				var bufferMinutes = CalculateBufferMinutes();

				if (bufferMinutes <= 0)
				{
					return TimeSpan.Zero;
				}

				return bufferMinutes.GetDateTimeFromMinutes().ToTimeSpan();
			}
		}

		public ZString EffectiveBufferDurationString
		{
			get
			{
				if (this is ProcessJobHeader || EffectiveBufferDuration == TimeSpan.Zero)
				{
					return ZString.Empty;
				}

				return (ZString)EffectiveBufferDuration.ToHoursAndMinutesString();
			}
		}

		public ZInt EffectiveBufferDurationMinutes
		{
			get
			{
				if (this is ProcessJobHeader)
				{
					return ZInt.Zero;
				}

				return CalculateBufferMinutes();
			}
		}

		ZInt CalculateBufferMinutes()
		{
			ZInt bufferMinutes = 0;

			if (FH_BMT_BufferTimespan.IsValid && BufferTimespan != null)
			{
				bufferMinutes = BufferTimespan.BMT_BufferTimespanInMinutes;
			}
			else if (JobHeader != null && JobHeader.FH_BMT_BufferTimespan.IsValid && JobHeader.BufferTimespan != null)
			{
				bufferMinutes = JobHeader.BufferTimespan.BMT_BufferTimespanInMinutes;
			}
			else if (DedicatedBuffer != null && DedicatedBuffer.FC_BufferTimespanInMinutes > 0)
			{
				bufferMinutes = DedicatedBuffer.FC_BufferTimespanInMinutes;
			}
			return bufferMinutes;
		}
		#endregion

		#region Workflow status

		[CustomisedControlExclude]
		[ResourceStringData("ProcessHeader.CurrentStatus", ShortCaption = "Status", Caption = "Current Status", FullDescription = "The current component and buffer zone this workflow is located within. Also shows any sub-components and buffer zones this workflow has penetrated.")]
		public ZString CurrentStatus
		{
			get { return Factory.GetCachedValue(CurrentStatusCacheKey, () => string.Join(System.Environment.NewLine, GetStatusLines()), CacheStalenessPolicy.StaleOnFactorySave); }
		}

		string CurrentStatusCacheKey
		{
			get { return "ProcessHeader.CurrentStatus" + PK; }
		}

		public ZPropertyInfo CurrentStatusInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentStatus)); }
		}

		[ResourceStringData("e0477a41-e98d-4a7e-a144-396d1d8e308b", ShortCaption = "Status", Caption = "Current Status", FullDescription = "The current component and buffer zone this workflow is located within. Also shows any sub-components and buffer zones this workflow has penetrated.")]
		public ZString CurrentStatusOnSingleLine
		{
			get
			{
				var lines = GetStatusLines().ToArray();
				switch (lines.Length)
				{
					case 0:
						return string.Empty;
					case 1:
						return Res.GetString("2fe56115-89b0-43dc-a26b-8f7285c0c677", "Current Component: {0}", lines[0]);
					default:
						return Res.GetString("33265b65-d009-4947-aca8-2973c9f439bf", "Current Component: {0}, Penetrated Components: {1}", lines[0], string.Join(", ", lines.Skip(1)).Replace("\t", ""));
				}
			}
		}

		public ZPropertyInfo CurrentStatusOnSingleLineInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentStatusOnSingleLine)); }
		}

		IEnumerable<ZString> GetStatusLines()
		{
			var currentComponent = CurrentComponent;
			if (currentComponent != null)
			{
				yield return GetBufferStatusLine(currentComponent);

				if (currentComponent.IsBuffer)
				{
					foreach (var childComponent in PenetratedComponents.ToArray())
					{
						yield return GetBufferStatusLine(childComponent, indent: true);
					}
				}
			}
		}

		string GetBufferStatusLine(BMComponent component, bool indent = false)
		{
			var result = new StringBuilder();
			if (indent)
			{
				result.Append("\t");
			}

			result.Append(component.FC_Name);

			if (component.IsBuffer)
			{
				var approvedRootDiagram = ApprovedShape?.RootDiagram as IBranchDepartmentProvider;
				var context = approvedRootDiagram == null ? component.GetRelevantContext() : WorkingTimeContext.Create(approvedRootDiagram);
				var penetration = BufferPenetrationCalculator.CalculatePenetrationPercentage(this, context, Factory);

				if (penetration.IsValid)
				{
					var actualPenetration = penetration.Penetration;

					if (component.IsChildBuffer)
					{
						actualPenetration = CalculateParentComponentPenetration(component, actualPenetration);
					}

					result.Append(" - ");

					var zone = ZoneCalculator.CalculateZone(actualPenetration);
					result.Append(Res.GetString("220e6dbc-c2ba-4460-8940-018ed8dd9169", "Zone {0}", zone));

					if (!component.IsChildBuffer && penetration.PenetratingBuffer.Type != BufferType.Operational && !string.IsNullOrEmpty(penetration.PenetratingBuffer.Name))
					{
						result.AppendFormat(CultureInfo.InvariantCulture, " ({0})", penetration.PenetratingBuffer.Name);
					}
				}
			}

			return result.ToString();
		}

		static decimal CalculateParentComponentPenetration(BMComponent component, decimal actualPenetration)
		{
			var parentComponent = component.ParentComponent;
			if (parentComponent != null)
			{
				var penetrationMinutes = parentComponent.FC_BufferTimespanInMinutes * actualPenetration;
				var penetrationMinutesIntoSubComponent = penetrationMinutes - component.FC_OffsetInMinutes;

				actualPenetration = penetrationMinutesIntoSubComponent / (decimal)component.FC_BufferTimespanInMinutes;
			}
			return actualPenetration;
		}

		[CustomisedControlExclude]
		[ResourceStringData("ProcessHeader.ProcessHeaderType", Caption = "Type")]
		public virtual ZString ProcessHeaderType
		{
			get { return FH_P0_Template.IsValid ? ProcessHeaderTypeList.Descriptions.TemplateWorkflow : ProcessHeaderTypeList.Descriptions.Workflow; }
		}

		#endregion

		#region Prerequisite Status

		[List("Lookups.WorkflowPrerequisiteStatusList")]
		public ZString PrerequisiteStatus
		{
			get
			{
				var isBlocked = FH_Status == WorkflowStatusList.Codes.Blocked;
				var isClosed = IsClosed;

				if (!isBlocked && !isClosed)
				{
					return WorkflowPrerequisiteStatusList.Codes.ClearedToStart;
				}

				if (HasStaggeredDelayExpired)
				{
					return WorkflowPrerequisiteStatusList.Codes.ClearedToRelease;
				}

				switch (FH_Status)
				{
					case WorkflowStatusList.Codes.ClosedWithOpenPrerequisites:
						return WorkflowPrerequisiteStatusList.Codes.ClosedWithOpenPrerequisites;
					case WorkflowStatusList.Codes.Closed:
						return WorkflowPrerequisiteStatusList.Codes.ClearedToStart;
					default:
						return WorkflowPrerequisiteStatusList.Codes.Blocked;
				}
			}
		}

		[ResourceStringData("ProcessHeader|PrerequisiteStatusDescription", Caption = "Prerequisite Status", FullDescription = "Indicates if this workflow is blocked by prerequisites.")]
		public ZString PrerequisiteStatusDescription
		{
			get { return Lookups.WorkflowPrerequisiteStatusList.GetDescriptionFromCode(PrerequisiteStatus); }
		}

		public ZPropertyInfo PrerequisiteStatusDescriptionInfo => GetZPropertyInfo(nameof(PrerequisiteStatusDescription));

		[ResourceStringData("ProcessHeader|PrerequisiteStatusShortDescription", Caption = "Prerequisite Status", FullDescription = "Indicates if this workflow is blocked by prerequisites.")]
		public ZString PrerequisiteStatusShortDescription
		{
			get
			{
				var isClosed = IsClosed;

				switch (PrerequisiteStatus)
				{
					case WorkflowPrerequisiteStatusList.Codes.Blocked:
						return isClosed
							? Res.GetString("1cddc009-afbc-40a9-8135-e482d4b5f7c4", "Workflow is complete, but has open prerequisites")
							: Res.GetString("128be101-dd66-44fd-b580-a4b62f373c86", "Blocked");

					case WorkflowPrerequisiteStatusList.Codes.ClearedToRelease:
						return isClosed
							? Res.GetString("82a721ef-bf11-4736-8744-74b6e823e412", "Workflow is complete, but is cleared for a staggered release")
							: Res.GetString("1ba583a6-db78-40b4-b0ae-732507f5fb78", "Cleared for a staggered release");

					case WorkflowPrerequisiteStatusList.Codes.ClearedToStart:
						return isClosed
							? Res.GetString("14446160-cceb-4ec6-8a8f-924c8a1d938b", "Workflow is complete")
							: Res.GetString("47a87654-e5ed-492a-bc61-472ce10f471d", "No open prerequisites");

					case WorkflowPrerequisiteStatusList.Codes.ClosedWithOpenPrerequisites:
						return Res.GetString("4DBED745-D4E4-4BAA-A230-824EFEB97200", "Workflow is closed with open prerequisites");

					default:
						return Res.GetString("3779f54f-9594-4be7-96dc-fbf4e937a9b9", "Unknown");
				}
			}
		}

		public ZPropertyInfo PrerequisiteStatusShortDescriptionInfo => GetZPropertyInfo(nameof(PrerequisiteStatusShortDescription));

		#endregion

		#region NumberOfOpenPrerequisitesUpTheTree

		[ResourceStringData("ProcessHeader|NumberOfOpenPrerequisitesUpTheTree", Caption = "Number of Open Prerequisites", ShortCaption = "Open Prerequisites", FullDescription = "The number of open prerequisites, including those inherited from the job-level workflow and any other indirect prerequisites.")]
		public ZInt NumberOfOpenPrerequisitesUpTheTree
		{
			get
			{
				return Factory.GetCachedValue(NumberOfOpenPrerequisitesUpTheTree_CacheKey, () => GetPKsOfPrerequisitesUpTheTree(includeOpenPrereqsOnly: true).Length, CacheStalenessPolicy.StaleOnFactorySave);
			}
		}

		string NumberOfOpenPrerequisitesUpTheTree_CacheKey => "ProcessHeader.NumberOfOpenPrerequisitesUpTheTree." + PK;

		public ZPropertyInfo NumberOfOpenPrerequisitesUpTheTreeInfo => GetZPropertyInfo(nameof(NumberOfOpenPrerequisitesUpTheTree));

		public void RefreshOpenPrerequisiteStatus()
		{
			var workflows = JobHeader.ProcessHeaders.Append(JobHeader);
			WorkflowStatusUpdater.UpdateWorkflowStatuses(workflows);

			foreach (var workflow in workflows)
			{
				workflow.OnRefreshOpenPrerequisiteStatus();
			}
		}

		void OnRefreshOpenPrerequisiteStatus()
		{
			Factory.ClearCachedValue<int>(NumberOfOpenPrerequisitesUpTheTree_CacheKey);

			NumberOfOpenPrerequisitesUpTheTreeInfo.RefreshBinding();
			PrerequisiteStatusShortDescriptionInfo.RefreshBinding();
		}

		public ZGuid[] GetPKsOfPrerequisitesUpTheTree(bool includeOpenPrereqsOnly = false)
		{
			return GetPrerequisitesAndTheirLinksUpTheTree(getApplicableDependenciesOnly: true).Where(x => !includeOpenPrereqsOnly || x.ProcessHeader.IsOpen).Select(x => x.LinkToProcessHeader.PK).ToArray();
		}

		#endregion

		#region BufferZone

		internal BufferPenetrationResult CalculateBufferPenetration()
		{
			var component = CurrentComponent;

			if (component != null && component.IsBuffer)
			{
				var context = WorkingTimeContext;
				return BufferPenetrationCalculator.CalculatePenetrationPercentage(this, context, Factory);
			}
			else
			{
				return BufferPenetrationResult.Zero;
			}
		}

		[BusinessObjectTestExclude] // We want to be able to return a null value if the header is not in a buffer zone
		public int? BufferZone
		{
			get
			{
				var penetration = CalculateBufferPenetration();

				if (penetration.IsValid)
				{
					return ZoneCalculator.CalculateZone(penetration.Penetration);
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		#region PenetratedComponent

		public IEnumerable<BMComponent> PenetratedComponents
		{
			get { return GetPenetratedComponents(); }
		}

		public IEnumerable<BMComponent> GetPenetratedComponents(ProcessTask task = null)
		{
			var components = Enumerable.Empty<BMComponent>();
			var currentComponent = CurrentComponent;

			if (currentComponent != null && currentComponent.IsBuffer)
			{
				var childComponents =
					from c in currentComponent.ChildComponents
					where c.FC_IsActive
					orderby c.FC_OffsetInMinutes, c.FC_DisplaySequence
					select c;

				components = GetPenetratedChildComponents(childComponents, task);
			}

			return components;
		}

		IEnumerable<BMComponent> GetPenetratedChildComponents(IEnumerable<BMComponent> childComponents, ProcessTask task = null)
		{
			var processHeaderCCRStatus = (task == null)
				? ConstrainedModeHelper.GetConstraintStatus(this)
				: ConstrainedModeHelper.GetConstraintStatus(task);

			switch (processHeaderCCRStatus)
			{
				case Enterprise.BufferManagement.Business.ConstraintStatus.Unknown:
				case Enterprise.BufferManagement.Business.ConstraintStatus.NonConstrained:
					return Enumerable.Empty<BMComponent>();

				default:
					return childComponents.Where(c => c.CCRStatus == processHeaderCCRStatus);
			}
		}

		#endregion

		#region CurrentTaskResource

		[ResourceStringData("ProcessHeader|CurrentTaskResourceCode", Caption = "Startable Task Resource Code", FullDescription = "The resource assigned to the current task of this Workflow.")]
		public ZString CurrentTaskResourceCode
		{
			get
			{
				var currentTaskResource = CurrentTaskResource;
				return currentTaskResource != null ? currentTaskResource.GS_Code : ZString.Empty;
			}
		}

		[ResourceStringData("ProcessHeader|CurrentTaskResourceName", Caption = "Startable Task Resource Name", FullDescription = "The resource assigned to the current task of this Workflow.")]
		public ZString CurrentTaskResourceName
		{
			get
			{
				var currentTaskResource = CurrentTaskResource;
				return currentTaskResource != null ? currentTaskResource.GS_FullName : ZString.Empty;
			}
		}

		GlbStaff CurrentTaskResource
		{
			get
			{
				var currentTask = GetTasksWithoutAccessingWorkflowParent().OrderBy(t => t.P9_Sequence).FirstOrDefault(t => t.IsCurrent);
				return currentTask != null ? currentTask.AssignedStaffMember : null;
			}
		}

		#endregion

		#region CurrentTasks

		public IEnumerable<ProcessTask> CurrentTasks
		{
			get { return Tasks.Where(t => !t.IsClosed || t.IsSuspended).CollectMinBy(t => t.P9_Sequence).ToArray() ?? Enumerable.Empty<ProcessTask>(); }
		}

		[ResourceStringData("ProcessHeader.CurrentTasksStatus", Caption = "Startable Task Status", ShortCaption = "Task Status", FullDescription = "The status of any current tasks within this workflow")]
		public ZString CurrentTasksStatus
		{
			get { return string.Join(", ", CurrentTasks.Select(t => t.CardStatusDescription)); }
		}

		#endregion

		#region SequenceNumber

		[CustomisedControlExclude]
		[ResourceStringData("ProcessHeader.Sequence", ShortCaption = "Seq.", Caption = "Sequence", FullDescription = "The sequence of workflows within this job according to order of dependency.")]
		public ZString Sequence
		{
			get { return JobHeader != null ? JobHeader.DependencyGraph.GetSequence(this) : string.Empty; }
		}

		#endregion

		#region IsDefaultWorkflow

		public bool IsDefaultWorkflow => !IsTemplate && FH_ParentTemplateId == BMGlobalConstants.DefaultWorkflowTemplateID;

		#endregion

		#region IsWorkflow

		public bool IsWorkflow
		{
			get { return !FH_FH_ParentHeader.IsEmpty; }
		}

		#endregion

		#region IsTemplate

		public bool IsTemplate
		{
			get { return !FH_P0_Template.IsEmpty; }
		}

		#endregion

		#region IsOpen

		[CustomisedControlExclude]
		[ResourceStringData("ProcessHeader.IsOpen", Caption = "Open", FullDescription = "Indicates whether this workflow has any incomplete tasks.")]
		[VisualBoardSearchable]
		public virtual ZBool IsOpen
		{
			get { return GetIsOpenIfHasChanges(); }
		}

		ZBool GetIsOpenIfHasChanges(HashSet<ZGuid> existing = null)
		{
			return HasChanges ? GetIsOpen(existing) : HasOpenStatus;
		}

		internal virtual bool GetIsOpen(HashSet<ZGuid> existing = null)
		{
			var currentExisting = existing == null ? new HashSet<ZGuid>() : new HashSet<ZGuid>(existing);

			if (!currentExisting.Add(PK))
			{
				return false;
			}

			return GetTasksWithoutAccessingWorkflowParent().Any(t => t.IsOpen) || ChildLinks.Any(l => l.HeaderFrom != null && l.HeaderFrom.GetIsOpenIfHasChanges(currentExisting));
		}

		public bool IsClosed => HasClosedStatus;
		public bool HasClosedStatus => FH_Status == WorkflowStatusList.Codes.Closed || FH_Status == WorkflowStatusList.Codes.ClosedWithOpenPrerequisites;
		public bool HasOpenStatus => FH_Status == WorkflowStatusList.Codes.Blocked || FH_Status == WorkflowStatusList.Codes.Open;
		public bool HasBlockedStatus => FH_Status == WorkflowStatusList.Codes.Blocked || FH_Status == WorkflowStatusList.Codes.ClosedWithOpenPrerequisites;
		public bool HasBlockingStatus => FH_Status == WorkflowStatusList.Codes.Blocked || FH_Status == WorkflowStatusList.Codes.ClosedWithOpenPrerequisites || FH_Status == WorkflowStatusList.Codes.Open;
		public bool HasUnblockedStatus => FH_Status == WorkflowStatusList.Codes.Open || FH_Status == WorkflowStatusList.Codes.Closed;

		public static IEnumerable<string> GetOpenStatuses()
		{
			yield return WorkflowStatusList.Codes.Blocked;
			yield return WorkflowStatusList.Codes.Open;
		}

		#endregion

		#region IsCriticalHandover

		[VisualBoardSearchable]
		[ResourceStringData("ProcessHeader.ApplicableIsCriticalHandover", Caption = "Critical Handover", FullDescription = "Indicates whether this workflow has a critical handover.")]
		public ZBool ApplicableIsCriticalHandover
		{
			get { return FH_IsCriticalHandover || (JobHeader != null && JobHeader.FH_IsCriticalHandover); }
		}

		#endregion

		#region IsStandby

		public ZBool ApplicableIsStandby
		{
			get { return FH_IsStandby || (JobHeader != null && JobHeader.FH_IsStandby); }
		}

		#endregion

		#region Parent/Child Status

		public bool IsParentOf(ProcessHeader other)
		{
			return ChildLinks.Any(l => l.FP_FH_HeaderFrom == other.PK);
		}

		public bool IsChildOf(ProcessHeader other)
		{
			return ParentLinks.Any(l => l.FP_FH_HeaderTo == other.PK);
		}

		public bool IsAncestorOf(ProcessHeader other, HashSet<ZGuid> existing = null)
		{
			HashSet<ZGuid> currentExisting = existing == null ? new HashSet<ZGuid>() : new HashSet<ZGuid>(existing);

			if (!currentExisting.Add(PK))
			{
				return false;
			}
			else
			{
				return
					other != null
					&& ChildLinks
						.Any(l =>
							l.HeaderFrom != null
							&& (l.FP_FH_HeaderFrom == other.PK || l.HeaderFrom.IsAncestorOf(other, currentExisting)));
			}
		}

		public bool IsPrerequisiteRecursiveOf(ProcessHeader other)
		{
			return IsPrerequisiteRecursiveOfCore(other, new HashSet<ProcessHeader>());
		}

		bool IsPrerequisiteRecursiveOfCore(ProcessHeader other, HashSet<ProcessHeader> visited)
		{
			if (other == null || visited.Contains(this))
			{
				return false;
			}

			visited.Add(this);
			return PostrequisiteLinks.Any(l => l.HeaderTo != null && (l.FP_FH_HeaderTo == other.PK || l.HeaderTo.IsPrerequisiteRecursiveOfCore(other, visited)));
		}

		public bool IsChildWorkflowWithinJobOf(ProcessHeader parentHeader)
		{
			if (IsInSameJob(parentHeader) && parentHeader.PK != PK)
			{
				return ParentLinks.Any(l => l.FP_FH_HeaderTo == parentHeader.PK && l.HeaderTo != null && l.HeaderTo.IsWorkflow);
			}
			else
			{
				return false;
			}
		}

		public bool IsChildWorkflowOfAnotherWorkflowWithinJob()
		{
			return IsWorkflow && JobHeader.ProcessHeaders.Any(w => IsChildWorkflowWithinJobOf(w));
		}

		public ProcessHeaderLink GetOrCreateLinkToParent(ProcessHeader parentHeader)
		{
			var link = ParentLinks_ForBinding.FirstOrDefault(l => l.FP_FH_HeaderTo == parentHeader.PK);
			if (link == null)
			{
				link = ParentLinks_ForBinding.AddNew();
				link.FP_FH_HeaderTo = parentHeader.PK;
				link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;

				link.ProcessNewLink();
			}

			return link;
		}

		#endregion

		#region SynchroniseBufferPenetration

		[BusinessObjectTestExclude] // Because the test wants to set the value to true, which we only allow for child workflows
		[ReadOnlyMember(nameof(SynchroniseBufferPenetration_ReadOnly))]
		[ResourceStringData("ProcessHeader.SynchroniseBufferPenetration", Caption = "Synchronize Buffer Penetration", ShortCaption = "Sync Buffer Penetration", FullDescription = "Synchronize the penetration of this workflow with its parent.")]
		public ZBool SynchroniseBufferPenetration
		{
			get
			{
				if (!BMSRegistry.Instance.SynchroniseBufferPenetration.Value)
				{
					return false;
				}

				var linkToParent = WorkflowParentLink;
				return linkToParent != null ? linkToParent.FP_SynchroniseBufferPenetration : ZBool.False;
			}
			set
			{
				var linkToParent = WorkflowParentLink;
				if (linkToParent != null)
				{
					linkToParent.FP_SynchroniseBufferPenetration = value;
				}

				SynchroniseBufferPenetrationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SynchroniseBufferPenetrationInfo
		{
			get { return GetZPropertyInfo(nameof(SynchroniseBufferPenetration)); }
		}

		protected bool SynchroniseBufferPenetration_ReadOnly
		{
			get { return WorkflowParentLink == null; }
		}

		#endregion

		#region SupportsFormFlowTypeTasks

		public bool SupportsFormFlowTypeTasks
			=> FormFlowTypeSupportingParents.Contains(FH_ParentTableCode);

		readonly HashSet<string> FormFlowTypeSupportingParents
			= [WhsDocketSchema.Constants.Prefix,
				WhsPickSchema.Constants.Prefix,
				WhsCycleCountWaveSchema.Constants.Prefix,
				WhsLoadSchema.Constants.Prefix];

		#endregion

		#endregion

		#region SQL

		public static string GetBaseSqlQuery(string whereClause)
		{
			return string.Format(CultureInfo.InvariantCulture, "SELECT * FROM dbo.{0} WHERE ({1}) ", ProcessHeaderSchema.Constants.TableName, string.IsNullOrEmpty(whereClause) ? "1 = 1" : whereClause);
		}

		#endregion

		#region Delete

		internal bool IsDeletingNow { get; private set; }

		public event EventHandler Deleting;

		public override void Delete()
		{
			IsDeletingNow = true;

			try
			{
				if (IsDeleted)
				{
					return;
				}

				var jobHeader = JobHeader;
				if (jobHeader != null && jobHeader.PK != PK)
				{
					jobHeader.FH_RemainingMinutesToComplete = Math.Max(0, jobHeader.FH_RemainingMinutesToComplete - FH_RemainingMinutesToComplete); // To prevent a merge conflict during de-duplication logic.
					if (!jobHeader.HasChanges)
					{
						jobHeader.FH_SystemLastEditTimeUtc = ZDateTime.UtcNow; // Ensure that we recalculate status.
					}
				}

				Deleting?.Invoke(this, EventArgs.Empty);

				var taskQuery = new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, PK) { FetchOnlyFromLocalCache = !IsInDatabase };
				DeleteRelatedBizos<ProcessTask>(taskQuery);
				DeleteRelatedBizos<TagLink>(new ZQuery(TagLinkSchema.TGL_ParentId, PK));

				UpdateLinkedShapesAndArrows(DeleteWorkflowOption.UnLinkShapes);
				DeleteRelatedLinks();

				foreach (var link in IterationLinks.ToArray())
				{
					link.Delete();
				}

				base.Delete();
				SaveDeletionCallstack();
			}
			finally
			{
				IsDeletingNow = false;
			}
		}

		protected override IAdditionalNoteProvider GetAdditionalNoteProvider() => new WorkflowAdditionalNoteProvider(base.GetAdditionalNoteProvider(), this);

		void DeleteRelatedBizos<T>(ZQuery query)
			where T : class, IBusiness
		{
			foreach (var bizo in Factory.Load<T>(query))
			{
				bizo.Delete();
			}
		}

		void DeleteRelatedLinks()
		{
			var linkQuery = new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, PK) { FetchOnlyFromLocalCache = !IsInDatabase };
			linkQuery.AddToFilter(new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderTo, PK), JoinCondition.Or);

			var links = Factory.Load<ProcessHeaderLink>(linkQuery).ToHashSet();

			foreach (var link in links)
			{
				Factory.AddFetchHint(ProcessHeaderSchema.PK, link.FP_FH_HeaderTo);
			}

			foreach (var link in links)
			{
				var headerTo = link.HeaderTo;
				if (headerTo != null)
				{
					var query = new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, PK) { FetchOnlyFromLocalCache = !IsInDatabase };
					Factory.AddFetchHint(ProcessHeaderLinkSchema.Instance, query);
				}
			}

			foreach (var link in links)
			{
				link.Delete();
			}
		}

		public ICollection<IBMNCNShape> GetLinkedShapes()
		{
			var shapeQuery = new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, PK) { FetchOnlyFromLocalCache = !IsInDatabase };

			return Factory.Load<IBMNCNShape>(shapeQuery);
		}

		public void UpdateLinkedShapesAndArrows(DeleteWorkflowOption option)
		{
			foreach (var shape in GetLinkedShapes())
			{
				if (option == DeleteWorkflowOption.DeleteShapes || shape.IsDefaultDiagram() || shape.IsDefaultDiagramChild())
				{
					shape.Delete();
				}
				else
				{
					shape.DisconnectRelatedEntity();
				}
			}
		}

		void SaveDeletionCallstack()
		{
			if (BMSRegistry.Instance.ReportWorkflowDeletionCallstackWhenLoadingWorkflowsToTransfer.Value)
			{
				DeletionCallstack = System.Environment.StackTrace;
			}
		}

		internal string DeletionCallstack { get; private set; }

#if DEBUG
		public string DeletionCallstack_ExposedForTest => DeletionCallstack;
#endif

		protected override bool ShouldIncludePropertyValueInRowDeletedError(string propertyName)
		{
			return propertyName == ProcessHeaderSchema.Constants.FH_ParentTableCode;
		}

		#endregion

		#region Promote

		public virtual void Promote(IWorkflowProvider provider)
		{
			var descriptionProperty = DescriptionPropertyAttribute.GetProperty(provider.GetType(), typeof(DescriptionPropertyAttribute));
			SetProviderPropertySafe(provider, descriptionProperty, FH_CompletionStatement);

			var bizO = (BusinessObject)provider;
			var jobHeader = ProcessJobHeader.GetForParent(provider, bizO.Factory);

			Promote(jobHeader);
		}

		public virtual void Promote(ProcessJobHeader jobHeader, bool cloneLinks = true, bool deleteOldTasks = true)
		{
			foreach (var link in Links.ToArray())
			{
				if (link.FP_FH_HeaderFrom == PK)
				{
					if (cloneLinks)
					{
						var newLink = (ProcessHeaderLink)jobHeader.Factory.Load<ProcessHeaderLink>(link.PK).Clone();
						newLink.FP_FH_HeaderFrom = jobHeader.PK;
					}
					else
					{
						link.FP_FH_HeaderFrom = jobHeader.PK;
					}
				}
				if (link.FP_FH_HeaderTo == PK)
				{
					if (cloneLinks)
					{
						var newLink = (ProcessHeaderLink)jobHeader.Factory.Load<ProcessHeaderLink>(link.PK).Clone();
						newLink.FP_FH_HeaderTo = jobHeader.PK;
					}
					else
					{
						link.FP_FH_HeaderTo = jobHeader.PK;
					}
				}
			}

			var tasks = Tasks.ToArray();
			if (tasks.Length > 0)
			{
				var workflow = jobHeader.ProcessHeaders.FirstOrDefault()
					?? jobHeader.AddDefaultProcessHeader();

				foreach (var task in tasks)
				{
					var newTask = workflow.TaskCollection.AddNew();

					var args = new BusinessObjectCloneArgs(columnNamesToExcludeFromCopy: new[] { AutoProcessTasks.Schema.P9_ParentID }, typeof(ProcessTask));
					newTask.P9_ParentID = jobHeader.FH_ParentId;
					newTask.CopyPersistentValuesFrom(task, args);
					newTask.P9_FH_ProcessHeader = workflow.PK;
					newTask.P9_ParentTableCode = jobHeader.FH_ParentTableCode;

					if (deleteOldTasks)
					{
						task.Delete();
					}
				}
			}
		}

		#endregion

		#region ModuleFilterConstants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Module filter 'name'")]
		public static class ModuleFilterConstants
		{
			public const string ActiveStatus = "Active Status";
			public const string AgreedDeliveryDate = "Agreed Delivery Date";
			public const string Approved = "Approved";
			public const string ApprovedScheduledStartTime = "Approved Scheduled Start Time";
			public const string ApprovedScheduledFinishTime = "Approved Scheduled Finish Time";
			public const string ApprovedScheduleType = "Approved Schedule Type";
			public const string AutoAssignTasks = "Auto Assign Tasks";
			public const string Branch = "Branch";
			public const string BufferManagementComponent = "Buffer Management Component";
			public const string BufferReleaseDate = "Buffer Release Date";
			public const string BufferTimespan = "Buffer Timespan";
			public const string BufferZone = "Buffer Zone";
			public const string CompletionStatement = "Completion Statement";
			public const string ConstraintStatus = "Constraint Status";
			public const string CurrentComponent = "Current Component";
			public const string CriticalHandover = "Critical Handover";
			public const string DateAcceptability = "Date Acceptability";
			public const string DeadlineType = "Deadline Type";
			public const string DedicatedBuffer = "Dedicated Buffer";
			public const string Department = "Department";
			public const string DependentWorkflows = "Dependent Workflows";
			public const string DurationRangeFilter = "Duration Range Filter";
			public const string EarliestStartDate = "Earliest Start Date";
			public const string EffectiveAgreedDeliveryDate = "Effective Agreed Delivery Date";
			public const string EffectiveBranch = "Effective Branch";
			public const string EffectiveBufferDuration = "Effective Buffer Duration";
			public const string EffectiveDepartment = "Effective Department";
			public const string JobCode = "Job Code";
			public const string JobDescription = "Job Description";
			public const string JobLevelWorkflow = "Job-level Workflow";
			public const string JobOrWorkflow = "Job or Workflow";
			public const string JobProperty = "Job Property";
			public const string LastTransferType = "Last Transfer Type";
			public const string LatestAcceptableReleaseDate = "Latest Acceptable Release Date";
			public const string LeadTime = "Lead Time";
			public const string ParentJob = "Parent Job";
			public const string ParentWorkflows = "Parent Workflows";
			public const string PlannedDuration = "Planned Duration";
			public const string PrerequisiteStatus = "Prerequisite Status";
			public const string PrerequisiteWorkflows = "Prerequisite Workflows";
			public const string QualityIteration = "Quality Iteration";
			public const string QueueStatus = "Queue Status";
			public const string ReleaseGroup = "Release Group";
			public const string ReleaseSequenceSortDate = "Release Sequence Sort Date";
			public const string ResourceAssignedToAnyTask = "Resource Assigned To Any Task";
			public const string ResourceAssignedToStartableTask = "Resource Assigned To Current Task";
			public const string SequencedWorkflows = "Sequenced Workflows";
			public const string ShownOnNetworkDiagram = "Shown on Network Diagram";
			public const string StaggeredReleaseDelayExpiry = "Staggered Release Delay Expiry";
			public const string StandbyTask = "Standby Task";
			public const string TagDefinitionCode = "Tag Definition Code";
			public const string TagMagnitude = "Tag Magnitude";
			public const string TagTaskDefinitionCode = "Tag Task Definition Code";
			public const string TagTaskMagnitude = "Tag Task Magnitude";
			public const string TaskAssigned = "Aggregated Task Assigned";
			public const string TaskOpenEstimateRange = "Open Task Estimate Range";
			public const string Template = "Template";
			public const string WorkflowCategory = "Workflow Category";
			public const string WorkflowsForJob = "Workflows for Job";
			public const string WorkflowStatus = "Workflow Status";
			public const string WorkflowType = "Workflow Type";
		}

		#endregion

		#region Nudge

		public void NudgeUp()
		{
			NudgeUp(true);
		}

		public void NudgeDown()
		{
			NudgeDown(true);
		}

		public void NudgeUp(bool log)
		{
			Nudge(1, log);
		}

		public void NudgeDown(bool log)
		{
			Nudge(-1, log);
		}

		void Nudge(int amount, bool log)
		{
			var newValue = FH_VoteUpDownAmount + amount;
			if (newValue <= short.MaxValue && newValue >= short.MinValue)
			{
				FH_VoteUpDownAmount = (ZShort)newValue;

				if (log)
				{
					var direction = amount > 0 ? "UP" : "DOWN";
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, "NUDGE " + direction + " TO " + FH_VoteUpDownAmount);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
			else if (!log)
			{
				throw new NudgeOutOfRangeException();
			}
		}

		#region Nudge out of Range

		[Serializable]
		public class NudgeOutOfRangeException : Exception
		{
			public NudgeOutOfRangeException()
				: base(Res.GetString("87cdd3c4-9a14-4652-969f-c4c5d2500957", "Cannot nudge out of bounds."))
			{
			}

			public NudgeOutOfRangeException(string message)
				: base(message)
			{
			}

			public NudgeOutOfRangeException(string message, Exception innerException)
				: base(message, innerException)
			{
			}

#if NETFRAMEWORK
			protected NudgeOutOfRangeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		#endregion

		#endregion

		#region Release Sequences

		BMReleaseSequenceItem SequenceItem => Factory.LoadTop1<BMReleaseSequenceItem>(new ZQuery(BMReleaseSequenceItemSchema.BMI_FH_ProcessHeader, PK) { FetchOnlyFromLocalCache = !IsInDatabase });

		internal ZDecimal SequenceNudge => SequenceItem != null ? (decimal)(SequenceItem.Sequence.BMR_SequenceNudge - (SequenceItem.BMI_Position - 1) * 0.0001) : 0.0m;

		public ProcessHeader HighestSequencedWorkflow
		{
			get
			{
				if (BMSRegistry.Instance.ReleaseSequencesModuleEnabled.Value && highestSequencedWorkflow == null)
				{
					highestSequencedWorkflow = GetRelatedReleaseSequenceProcessHeaders(this, 0).Where(w => w.SequenceItem != null).MaxBySafe(p => p.SequenceNudge);
				}

				return highestSequencedWorkflow;
			}
		}
		ProcessHeader highestSequencedWorkflow;

		[ResourceStringData("ProcessHeader.ReleaseSequenceWorkflow", Caption = "Release Sequence Workflow", ShortCaption = "Workflow", FullDescription = "Release sequence workflow identifier.")]
		public ZString ReleaseSequenceWorkflow => HighestSequencedWorkflow != null ? HighestSequencedWorkflow.Code : ZString.Empty;

		[ResourceStringData("ProcessHeader.ReleaseSequenceParentJob", Caption = "Release Sequence Parent Job", ShortCaption = "Parent Job", FullDescription = "Release sequence parent job identifier.")]
		public ZString ReleaseSequenceParentJob => HighestSequencedWorkflow != null && HighestSequencedWorkflow.JobHeader != null ? HighestSequencedWorkflow.JobHeader.Description : ZString.Empty;

		[ResourceStringData("ProcessHeader.ReleaseSequenceName", Caption = "Release Sequence Name", ShortCaption = "Sequence", FullDescription = "The name of the release sequence the workflow is linked to.")]
		public ZString ReleaseSequenceName => HighestSequencedWorkflow != null ? HighestSequencedWorkflow.SequenceItem.Sequence.BMR_Name : ZString.Empty;

		[ResourceStringData("ProcessHeader.ReleaseSequencePosition", Caption = "Release Sequence Position", ShortCaption = "Position", FullDescription = "The position of the release sequence the workflow is linked to.")]
		public ZInt ReleaseSequencePosition => HighestSequencedWorkflow != null ? HighestSequencedWorkflow.SequenceItem.BMI_Position : (ZInt)0;

		[ResourceStringData("ProcessHeader.ReleaseSequenceInvestment", Caption = "Release Sequence Investment", ShortCaption = "Investment", FullDescription = "The value of the release sequence the workflow is linked to.")]
		public ZInt ReleaseSequenceInvestment => HighestSequencedWorkflow != null ? HighestSequencedWorkflow.SequenceItem.BMI_Investment : (ZInt)0;

		[ResourceStringData("ProcessHeader.ReleaseSequenceValue", Caption = "Release Sequence Value", ShortCaption = "Value", FullDescription = "The value of the release sequence the workflow is linked to.")]
		public ZInt ReleaseSequenceValue => HighestSequencedWorkflow != null ? HighestSequencedWorkflow.SequenceItem.BMI_Value : (ZInt)0;

		public ZGuid ReleaseSequencePK => HighestSequencedWorkflow != null ? HighestSequencedWorkflow.SequenceItem.Sequence.PK : ZGuid.Empty;

		public IBMReleaseSequence HighestReleaseSequence => HighestSequencedWorkflow?.SequenceItem.Sequence;

		[ResourceStringData("ProcessHeader.EffectiveNudge", Caption = "Effective Nudge", FullDescription = "The nudge value used for sorting items inside the Release Gate. It considers the nudge applied to this workflow as well as nudges of applied tags, and the number of closed direct prerequisites.")]
		public ZDecimal EffectiveNudge => GetEffectiveNudge();

		public ZPropertyInfo EffectiveNudgeInfo
		{
			get { return GetZPropertyInfo(nameof(EffectiveNudge)); }
		}

		protected virtual ZDecimal DirectNudge => GetNudgeFromJobLevelWorkflowIfSet() + (ZDecimal)FH_VoteUpDownAmount;

		ZDecimal GetNudgeFromJobLevelWorkflowIfSet()
		{
			var jobLevelWorkflow = JobHeader;

			return jobLevelWorkflow != null ? jobLevelWorkflow.FH_VoteUpDownAmount : 0m;
		}

		[ReadOnly(true)]
		[CustomisedControlExclude]
		public override ZDecimal FH_EffectiveNudge
		{
			get
			{
				return base.FH_EffectiveNudge;
			}
			set
			{
				base.FH_EffectiveNudge = value;
				FH_EffectiveNudgeInfo.RefreshBinding();
			}
		}

		public bool CanCalculateEffectiveAgreedDeliveryDate =>
			FH_IsActive && !IsClosed;

		public bool CanCalculateSparsePropertiesForNewReleaseGate =>
			FH_IsActive && !IsClosed && FH_FC_DedicatedBuffer.IsValid;

		public virtual void UpdateEffectiveNudge()
		{
			if (!CanCalculateSparsePropertiesForNewReleaseGate)
			{
				FH_EffectiveNudge = 0m; // will be null in the database which saves db space for sparse columns like FH_EffectiveNudge
			}
			else
			{
				FH_EffectiveNudge = GetEffectiveNudge();
			}
		}

		ZDecimal GetEffectiveNudge() => NudgeCalculator.GetEffectiveNudge(this);

		public ZDateTime ReleaseSequenceSortDate
		{
			get { return ApplicableDoNotStartBeforeDateUtc.IsValid ? ApplicableDoNotStartBeforeDateUtc : FH_ReleaseDateTime.IsValid ? FH_ReleaseDateTime : ZDateTime.MaxSmallDateTime; }
		}

		public virtual void UpdateReleaseSequenceSortDate()
		{
			FH_ReleaseSequenceSortDateUtc =
				!CanCalculateSparsePropertiesForNewReleaseGate
				? ZDateTime.Empty
				: ReleaseSequenceSortDate;
		}

		#endregion

		#region Logging

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => this.AreDataVersionsLogged();

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion

		#region IProcessHeader Members

		bool IProcessHeader.IsCurrent(IProcessTask task)
		{
			return task.IsStartable();
		}

		IProcessJobHeader IProcessHeader.JobHeader
		{
			get { return JobHeader; }
		}

		IProcessHeaderValidation IProcessHeader.Validation
		{
			get { return base.Validation; }
		}

		IEnumerable<IProcessTask> IProcessHeader.Tasks
		{
			get { return Tasks; }
		}

		IBMComponent IProcessHeader.CurrentComponent
		{
			get { return CurrentComponent; }
		}

		IBMSystem IProcessHeader.BMSystem
		{
			get { return BMSystem; }
		}

		IWorkflowProviderCore IProcessHeader.Parent
		{
			get { return Parent; }
		}

		IGlbGroup IProcessHeader.ReleaseGroup
		{
			get { return ReleaseGroup; }
		}

		IEnumerable<IProcessHeaderLink> IProcessHeader.Links
		{
			get { return Links; }
		}

		IProcessHeaderLinkCollection IProcessHeader.LinksFromMeToOthers
		{
			get { return LinksFromMeToOthers_ForBinding; }
		}

		IProcessHeaderLinkCollection IProcessHeader.LinksFromOthersToMe
		{
			get { return LinksFromOthersToMe_ForBinding; }
		}

		IProcessHeaderLink IProcessHeader.GetOrCreateLinkToParent(IProcessHeader parentHeader)
		{
			return GetOrCreateLinkToParent((ProcessHeader)parentHeader);
		}

		IProcessHeaderLink IProcessHeader.GetOrCreateDependencyLink(IProcessHeader postrequisite)
		{
			return GetOrCreateDependencyLink((ProcessHeader)postrequisite);
		}

		bool IProcessHeader.IsParentOf(IProcessHeader other)
		{
			return IsParentOf((ProcessHeader)other);
		}

		IProcessHeader IProcessHeader.Clone()
		{
			return (IProcessHeader)Clone();
		}

		IEnumerable<IProcessHeaderLink> IProcessHeader.PrerequisiteLinks
		{
			get { return PrerequisiteLinks; }
		}

		IEnumerable<IProcessHeaderLink> IProcessHeader.ParentLinks
		{
			get { return ParentLinks; }
		}

		IEnumerable<IProcessHeader> IProcessHeader.GetPrerequisitesUpTheTree()
		{
			return GetPrerequisitesUpTheTree();
		}

		IEnumerable<IProcessHeader> IProcessHeader.GetParentWorkflowsUpTheHierarchy()
		{
			return GetParentsUpTheHierarchy().Select(link => link.ProcessHeader);
		}

		IEnumerable<IProcessHeader> IProcessHeader.GetChildWorkflowsDownTheHierarchy()
		{
			return GetChildWorkflowsDownTheHierarchy();
		}

		IEnumerable<IProcessHeader> IProcessHeader.GetAncestors()
		{
			return this.Ancestors();
		}

		IProcessHeaderLookups IProcessHeader.Lookups
		{
			get { return Lookups; }
		}

		ITagLinkCollection IProcessHeader.TagLinks_ForBinding => TagLinks_ForBinding;

		IProcessHeader IProcessHeader.ParentHeader
		{
			get { return ParentHeader; }
		}

		bool IProcessHeader.IsReleased
		{
			get { return IsReleased; }
		}

		public WorkingTimeContext WorkingTimeContext => CurrentComponent is BMComponent component && component.IsBuffer
			? component.GetRelevantContext(branchOverride: EffectiveBranch, departmentOverride: EffectiveDepartment)
			: null;

		#region Startable

		TimeSpan IProcessHeader.GetWorkingTimeSinceTaskBecameStartable(ZDateTime timeTaskBecameStartableUtc)
		{
			if (timeTaskBecameStartableUtc == ZDateTime.Empty)
			{
				return TimeSpan.Zero;
			}

			WorkingTimeContext context = null;

			if (OverrideContextProvider != null)
			{
				context = WorkingTimeContext.Create(OverrideContextProvider);
			}

			if (context == null || context.Branch == null || context.Department == null)
			{
				context = CurrentComponent.GetRelevantContext();
			}

			if (context == null || context.Branch == null || context.Department == null)
			{
				return TimeSpan.Zero;
			}

			var locationFrom = timeTaskBecameStartableUtc.ToLocationTime(context.Branch.HomePort).ToDateTime();
			var locationTo = ZDateTime.UtcNow.ToLocationTime(context.Branch.HomePort).ToDateTime();

			var helper = WorkingDays.GetInstance(Factory, context.Department.PK, context.Branch.PK, ZGuid.Empty);
			return helper.TimeDifference(locationFrom, locationTo);
		}

		public IBranchDepartmentProvider OverrideContextProvider
		{
			get; set;
		}

		#endregion

		#endregion

		#region ITransferrableProcessHeader Members

		Guid ITransferrableProcessHeader.PK => PK.ToGuid();

		DateTime ITransferrableProcessHeader.SystemLastEditTimeUtc => FH_SystemLastEditTimeUtc.ToDateTime();

		string ITransferrableProcessHeader.CompletionStatement => FH_CompletionStatement;

		string ITransferrableProcessHeader.ParentJobDescription => ParentJobDescription;

		Guid ITransferrableProcessHeader.CurrentComponent => FH_FC_CurrentComponent.ToGuid();

		short ITransferrableProcessHeader.VoteUpDownAmount => FH_VoteUpDownAmount;

		Guid ITransferrableProcessHeader.DedicatedBuffer
		{
			get => FH_FC_DedicatedBuffer.IsValid ? FH_FC_DedicatedBuffer.ToGuid() : Guid.Empty;
			set
			{
				FH_FC_DedicatedBuffer = value;
			}
		}

		#endregion

		#region Reset Penetration

		ProcessHeader ParentWorkflowWithinJob
		{
			get
			{
				var parents = GetParentsUpTheHierarchy()
					.Select(w => w.ProcessHeader)
					.Where(w => w.IsWorkflow && w.FH_FH_ParentHeader == FH_FH_ParentHeader);

				return parents.Any() ? parents.Last() : this;
			}
		}

		ZDateTime PenetrationResetDateUtc => ParentWorkflowWithinJob.FH_TaskPenetrationResetDateTimeUtc;

		public void SetPenetrationResetDate()
		{
			var workflow = ParentWorkflowWithinJob;

			if (workflow.FH_TaskPenetrationResetDateTimeUtc.IsEmpty)
			{
				workflow.FH_TaskPenetrationResetDateTimeUtc = ZDateTime.UtcNow;
			}
		}

		public decimal GetPenetrationPercentage()
		{
			return CalculateBufferPenetration().Penetration;
		}

		public decimal GetTaskPenetrationPercentage(ProcessTask task)
		{
			var component = CurrentComponent;

			if (component != null && component.IsBuffer)
			{
				return CalculatePenetrationPercentage(task, WorkingTimeContext);
			}
			else
			{
				return BufferPenetrationResult.Zero.Penetration;
			}
		}

		public decimal CalculatePenetrationPercentage(WorkingTimeContext context, BusinessObjectFactory factory, bool includeNetworkBuffers = true)
		{
			if (!IsInPlanningManagementMode)
			{
				includeNetworkBuffers = false;
			}

			return BufferPenetrationCalculator.CalculatePenetrationPercentage(this, context, factory, includeNetworkBuffers).Penetration;
		}

		public decimal CalculatePenetrationPercentage(WorkingTimeContext context, bool includeNetworkBuffers = true)
		{
			return CalculatePenetrationPercentage(context, Factory, includeNetworkBuffers);
		}

		public decimal CalculatePenetrationPercentage(ProcessTask task, WorkingTimeContext context, Func<decimal> workflowPenetrationGetter, bool includeNetworkBuffers = true)
		{
			var specialPenetration = GetTaskSpecialPenetration(CurrentComponent, task, context);
			return workflowPenetrationGetter != null ?
					(specialPenetration != -1.0M ? Math.Min(specialPenetration, workflowPenetrationGetter()) : workflowPenetrationGetter()) :
				CalculatePenetrationPercentage(context, includeNetworkBuffers);
		}

		public decimal CalculatePenetrationPercentage(ProcessTask task, WorkingTimeContext context, bool includeNetworkBuffers = true)
		{
			var specialPenetration = GetTaskSpecialPenetration(CurrentComponent, task, context);
			return specialPenetration != -1.0M ?
				Math.Min(specialPenetration, CalculatePenetrationPercentage(context, includeNetworkBuffers)) :
					CalculatePenetrationPercentage(context, includeNetworkBuffers);
		}

		decimal GetTaskSpecialPenetration(BMComponent component, ProcessTask task, WorkingTimeContext context)
		{
			if (component == null || !component.IsBuffer || !task.P9_IsResetBeingAppliedToThisTask || !WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.Value || PenetrationResetDateUtc.IsEmpty)
			{
				return -1.0M;
			}

			var releaseGroupLink = component.ReleaseGroupLinks.Cast<IBMComponentReleaseGroupLink>().FirstOrDefault(w => w.ReleaseGroup.PK == FH_GG_ReleaseGroup);
			if (releaseGroupLink == null)
			{
				return -1.0M;
			}

			var contextWithFactory = new WorkingTimeContextWithFactory(context, Factory);
			var bufferAge = GetAgeInMinutes(contextWithFactory, ZDateTime.UtcNow);
			var resetAge = GetAgeInMinutes(contextWithFactory, PenetrationResetDateUtc);
			var age = (bufferAge - resetAge) * releaseGroupLink.FO_ResetTaskPenetrationThrottleFactor;
			return age > bufferAge ? -1.0M : Convert.ToDecimal(age) / component.FC_BufferTimespanInMinutes;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int GetAgeInMinutes(WorkingTimeContextWithFactory context, ZDateTime zDateTime)
		{
			return BufferPenetrationCalculator.GetAgeInMinutes(this, context, zDateTime.ToDateTime());
		}

		#endregion

		#region Status Events

#if DEBUG
		public int HeaderStatusChangeCount_ForTesting { get; private set; }
#endif

		internal void UpdateBufferPenetrationPercentage()
		{
			if (FH_Status == WorkflowStatusList.Codes.Closed)
			{
				var currentComponent = CurrentComponent;

				if (currentComponent != null)
				{
					if (currentComponent.IsBuffer)
					{
						FH_BufferPenetrationPercentWhenCompleted = BufferPenetrationCalculator.CalculatePenetrationPercentage(this, WorkingTimeContext, Factory, IsInPlanningManagementMode).Penetration;
					}
					else if (currentComponent.IsBucket)
					{
						FH_BufferPenetrationPercentWhenCompleted = -1m;
					}
					else
					{
						FH_BufferPenetrationPercentWhenCompleted = 0m;
					}
				}
				else
				{
					FH_BufferPenetrationPercentWhenCompleted = 0m;
				}
			}
			else if (FH_Status == WorkflowStatusList.Codes.Open)
			{
				FH_BufferPenetrationPercentWhenCompleted = 0m;
			}
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		public ProcessHeader CloneWorkflow()
		{
			var clone = (ProcessHeader)Clone();

			SetClonedWorkflowComponent(clone);
			ProcessHeaderCompletionStatementSequenceHandler.SetClonedWorkflowSequence(clone);
			CloneTasks(clone);
			CloneTagLinks(clone);

			return clone;
		}

		void SetClonedWorkflowComponent(ProcessHeader clone)
		{
			if (!clone.FH_FC_CurrentComponent.IsEmpty)
			{
				var firstComponent = GetFirstBMComponent();
				if (firstComponent != null && firstComponent.PK != clone.FH_FC_CurrentComponent)
				{
					clone.FH_FC_CurrentComponent = firstComponent.PK;
				}
			}
		}

		void CloneTasks(ProcessHeader clone)
		{
			foreach (var task in AllTasksAndCompletionStatements.Cast<ProcessTask>().ToArray())
			{
				using (task.SuspendUpdatingIterationPivots())
				{
					var cloneTask = (ProcessTask)task.Clone();
					cloneTask.P9_FH_ProcessHeader = clone.PK;
					cloneTask.P9_Sequence += 100;
					clone.TaskCollection.Add(cloneTask);
				}
			}
		}

		void CloneTagLinks(ProcessHeader clone)
		{
			foreach (var tagLink in TagLinks)
			{
				var magnitude = tagLink.TagMagnitude;
				if (!(magnitude is WorkQueue)
					&& (!(magnitude.TagDefinition.TGD_UsageScope.Equals(TagUsageScopeList.Codes.Rule))))
				{
					clone.AddTag(magnitude);
				}
			}
		}

		#endregion

		#region Cancel

		public void CancelAllTasksAndCompletionStatements(ZString cancellationReason)
		{
			foreach (ProcessTask task in AllTasksAndCompletionStatements)
			{
				if (task.IsOpen)
				{
					task.CancelAndSuspendValidationOnTaskCancellation();

					if (!task.IsCompletionStatement)
					{
						string newNoteText;
						if (!task.P9_Notes.IsEmpty)
						{
							string rtfText = ORtfTextUtil.TextToRtf(string.Concat("\r\n\r\n", cancellationReason));
							newNoteText = ORtfTextUtil.AppendRtfStrings(task.P9_Notes.ToUTF8(), rtfText);
						}
						else
						{
							newNoteText = ORtfTextUtil.TextToRtf(cancellationReason);
						}
						task.P9_Notes = ZBlob.FromUTF8(newNoteText);
					}
				}
			}
		}

		#endregion

		#region Component Change Event

		internal void Defer(BMComponent startingComponent, ZDateTime earliestStartDate, string reason = "")
		{
			DeferCore(startingComponent, earliestStartDate, reason);
		}

		protected virtual void DeferCore(BMComponent startingComponent, ZDateTime earliestStartDate, string reason = "")
		{
			ChangeComponent(ComponentChangeMode.Defer, startingComponent.PK, null, reason);

			DoNotStartBeforeDateLocal = earliestStartDate;
		}

		public void TransferTo(BMComponentLink link, bool isResponsiveTransfer)
		{
			ChangeComponent(isResponsiveTransfer ? ComponentChangeMode.ResponsiveTransfer : ComponentChangeMode.SchematicTransfer, link.FL_FC_ComponentTo, link);
		}

		#endregion

		#region Date Defaulting

		void UpdateDatesFromJob()
		{
			if (!IsDeleted && FH_P0_Template.IsEmpty && Parent is BusinessObject parentBizo && parentBizo.HasChanges && !parentBizo.IsDeleted)
			{
				UpdateDatesFromJob(FH_DoNotStartBeforeDateInfo, FH_EarliestStartDateDefaultsFrom, FH_EarliestStartDefaultHoursOffset);
				UpdateDatesFromJob(FH_AgreedDeliveryDateInfo, FH_AgreedDeliveryDateDefaultsFrom, FH_AgreedDeliveryDateDefaultHoursOffset);
			}
		}

		void UpdateDatesFromJob(ZPropertyInfo dateProperty, ZString dateTimeSourceType, ZDateTime dateTimeSourceOffsetToAdd)
		{
			if (!dateTimeSourceType.IsEmpty)
			{
				var offsetValue = WorkflowDescriptor?.GetDateTimeFromParent(this, dateTimeSourceType, dateTimeSourceOffsetToAdd);
				var datetimeValue = offsetValue == null ? ZDateTime.Empty : offsetValue.Value.ToUtcZDateTime();

				if ((ZDateTime)dateProperty.Value != datetimeValue)
				{
					dateProperty.Value = datetimeValue;
				}
			}
		}

		#endregion

		#region To String

		public override string ToString()
		{
			if (!string.IsNullOrEmpty(FH_CompletionStatement))
			{
				return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", base.ToString(), FH_CompletionStatement);
			}
			else
			{
				return base.ToString();
			}
		}

		#endregion

		#region IWorkflowTriggerEventSource Members

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get { return Parent != null ? new IWorkflowProviderCore[] { Parent } : Array.Empty<IWorkflowProviderCore>(); }
		}

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get { return GlbCompany.GetCurrentCompany(Factory); }
		}

		#endregion

		#region Universal copy helpers

		protected void FinishUniversalCopy()
		{
			EnsureWorkflowLinkedToJobLevelWorkflow();
			EnsureUniqueCompletionStatement();

			if (Parent != null)
			{
				Parent.WorkflowItems.Load();

				if (FH_WorkflowType.IsEmpty)
				{
					FH_WorkflowType = Parent.WorkflowType;
				}
			}

			if (IsWorkflow && FH_FC_CurrentComponent.IsEmpty)
			{
				var firstComponent = GetFirstBMComponent();
				if (firstComponent != null)
				{
					FH_FC_CurrentComponent = firstComponent.PK;
				}
			}
		}

		void EnsureWorkflowLinkedToJobLevelWorkflow()
		{
			if (FH_Category != BMConstants.JobLevelWorkflowCategoryCode)
			{
				var jobHeaderQuery = new ZQuery(ProcessHeaderSchema.FH_ParentId, Parent.PK);
				jobHeaderQuery.AddToFilter(ProcessHeaderSchema.FH_Category, BMConstants.JobLevelWorkflowCategoryCode);
				jobHeaderQuery.FetchOnlyFromLocalCache = true;

				var copiedJobHeader = Factory.Load<ProcessJobHeader>(jobHeaderQuery).FirstOrDefault();
				if (copiedJobHeader != null)
				{
					FH_FH_ParentHeader = copiedJobHeader.PK;
				}
			}
		}

		void EnsureUniqueCompletionStatement()
		{
			var jobHeader = JobHeader;
			if (jobHeader != null)
			{
				var completionStatement = FH_CompletionStatement;
				while (jobHeader.ProcessHeaders.Any(h => !object.ReferenceEquals(h, this) && string.Equals(h.FH_CompletionStatement, completionStatement, StringComparison.OrdinalIgnoreCase)))
				{
					completionStatement = completionStatement.ToString().AppendNextBracketedNumber();
				}
				FH_CompletionStatement = completionStatement;
			}
		}

		#endregion

		#region IProposedNetworkEntity Members

		string IProposedNetworkEntity.CompletionCriteria
		{
			get
			{
				return CompletionCriteria;
			}
			set
			{
				var completionStatements = value.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				var collection = CompletionStatementTasksIncludingChildWorkflowTasks;

				for (int i = 0; i < completionStatements.Length; i++)
				{
					var statementTask = i < collection.Count ? collection[i] : CreateNewCompletionStatementTask();
					statementTask.P9_NotesAsString = completionStatements[i];
				}

				foreach (var unneededTask in collection.Skip(completionStatements.Length).ToArray())
				{
					unneededTask.Delete();
				}
			}
		}

		protected virtual ProcessTask CreateNewCompletionStatementTask()
		{
			return CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
		}

		string IProposedNetworkEntity.EstimateSummary
		{
			get { return TotalEstimatedHoursSummary; }
		}

		bool IProposedNetworkEntity.IsOnCriticalPath
		{
			get
			{
				var schedule = JobHeader?.FloatCalculator?.GetSchedule(this);
				return schedule != null && schedule.IsCriticalPath;
			}
		}

		bool IProposedNetworkEntity.CanUnlinkEntity
		{
			get { return false; }
		}

		IProposedNetworkEntity IProposedNetworkEntity.Parent
		{
			get { return null; }
		}

		IEnumerable<IEntityRelationship> IProposedNetworkEntity.Links
		{
			get { return Links.Cast<IEntityRelationship>(); }
		}

		string IProposedNetworkEntity.Name
		{
			get { return IsWorkflow ? Name : ((IProposedNetworkEntity)this).JobName; }
			set
			{
				if (IsWorkflow)
				{
					Name = value;
				}
				else
				{
					((IProposedNetworkEntity)this).JobName = value;
				}
			}
		}

		public string Name
		{
			get { return FH_CompletionStatement; }
			set
			{
				if (FH_CompletionStatement != value)
				{
					FH_CompletionStatement = value;

					OnPropertyChanged();
				}
			}
		}

		string IProposedNetworkEntity.JobNumber
		{
			get { return ProviderJobNumber; }
		}

		string IProposedNetworkEntity.JobName
		{
			get { return ProviderJobDescription; }
			set
			{
				if (Parent != null)
				{
					SetProviderPropertySafe(Parent, GetParentDescriptionProperty(), value);
				}
			}
		}

		static void SetProviderPropertySafe(IWorkflowProvider provider, PropertyInfo property, ZString value)
		{
			if (property != null && property.GetSetMethod() != null)
			{
				var propertyInfo = ((BusinessObject)provider).ZPropertyInfoHash[property.Name];
				if (value.Length > propertyInfo.MaxLength)
				{
					value = value.Substring(0, propertyInfo.MaxLength);
				}

				object valueToSet;

				if (typeof(ZString).IsAssignableFrom(property.PropertyType))
				{
					valueToSet = value;
				}
				else
				{
					valueToSet = value;
				}

				property.SetValue(provider, valueToSet, null);
			}
		}

		bool IProposedNetworkEntity.JobName_ReadOnly
		{
			get { return GetParentDescriptionProperty() == null; }
		}

		PropertyInfo GetParentDescriptionProperty()
		{
			return Parent == null ? null : DescriptionPropertyAttribute.GetProperty(Parent.GetType(), typeof(DescriptionPropertyAttribute));
		}

		string IProposedNetworkEntity.Description
		{
			get { return CurrentStatus; }
		}

		IEnumerable<IEntityRelationship> IProposedNetworkEntity.PostRequisiteLinks
		{
			get { return PostrequisiteLinks; }
		}

		IEnumerable<IEntityRelationship> IProposedNetworkEntity.PreRequisiteLinks
		{
			get { return PrerequisiteLinks; }
		}

		WorkStatus IProposedNetworkEntity.Status
		{
			get
			{
				if (HasOpenPrerequisites)
				{
					return WorkStatus.Blocked;
				}
				else if (GetTasksWithoutAccessingWorkflowParent().Count == 0)
				{
					return FH_ParentId.IsValid ? WorkStatus.Complete : WorkStatus.None;
				}
				else if (!IsOpen)
				{
					if (IsWorkflowCancelled)
					{
						return WorkStatus.Cancelled;
					}
					return WorkStatus.Complete;
				}
				else if (IsWorking)
				{
					return WorkStatus.Working;
				}
				else if (IsSuspended)
				{
					return WorkStatus.Suspended;
				}
				else
				{
					return WorkStatus.Startable;
				}
			}
		}

		public string StatusDescription
		{
			get { return this.GetNetworkStatusDescription(); }
		}

		string IProposedNetworkEntity.StatusName
		{
			get { return this.GetStatusName(); }
		}

		bool IProposedNetworkEntity.IsStartable
		{
			get { return IsStartable(); }
		}

		protected virtual bool IsStartable()
		{
			return !HasOpenPrerequisites && IsOpen && !IsWorkflowCancelled;
		}

		bool IsWorkflowCancelled
		{
			get
			{
				var tasks = GetTasksWithoutAccessingWorkflowParent();

				return tasks.Count > 0 && tasks.All(t => t.P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled);
			}
		}

		internal bool IsWorking
		{
			get { return GetTasksWithoutAccessingWorkflowParent().Any(t => t.P9_Status == ProcessTaskStatusCodeList.Codes.Working); }
		}

		internal bool IsSuspended
		{
			get { return GetTasksWithoutAccessingWorkflowParent().Any(t => t.P9_Status == ProcessTaskStatusCodeList.Codes.Suspended); }
		}

		bool IProposedNetworkEntity.IsSameEntity(IProposedNetworkEntity other)
		{
			var processHeader = other as ProcessHeader;
			if (processHeader != null)
			{
				return processHeader == this;
			}

			var shape = other as IBMNCNShape;
			if (shape != null)
			{
				return shape.BNS_RelatedEntityID == PK;
			}

			return false;
		}

		public bool CanDeleteUnderlyingEntity
		{
			get { return FH_ParentId.IsValid; }
		}

		void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region ITagable Members

		public ICollection<ITagLink> TagLinks => Factory.Load<TagLink>(this.CreateTagLinksQuery());

		public void AddTagLinksFetchHint()
		{
			Factory.AddFetchHint(TagLinkSchema.Instance, this.CreateTagLinksQuery());
		}

		ITagable ITagable.Parent => JobHeader;

		string ITagable.Description => Description;

		#endregion

		#region ILinkEntity Members

		Guid ILinkEntity.PK
		{
			get { return PK.ToGuid(); }
		}

		bool ILinkEntity.IsLeaf
		{
			get { return !ChildHeaders.Any(); }
		}

		string ILinkEntity.DisplayName
		{
			get { return Code; }
		}

		DateTime ILinkEntity.AgreedDeliveryDateInUtc
		{
			get { return ApplicableAgreedDeliveryDateUtc.IsValid && !ApplicableAgreedDeliveryDateUtc.IsEmpty ? ApplicableAgreedDeliveryDateUtc.ToDateTime() : default(DateTime); }
		}

		IReadOnlyCollection<ILink> ILinkEntity.Links
		{
			get { return Links.Where(l => l.FP_LinkType == ProcessHeaderLinkTypeList.Codes.Dependency).ToList(); }
		}

		internal IEnumerable<ProcessHeader> LinkEntityParents
		{
			get { return ParentHeaders; }
		}

		public virtual IEnumerable<ProcessHeader> ParentHeaders
		{
			get
			{
				var workflowParentLink = ParentLinks.FirstOrDefault(l => l.HeaderTo != null && IsChildWorkflowWithinJobOf(l.HeaderTo));
				if (workflowParentLink != null)
				{
					yield return workflowParentLink.HeaderTo;
				}
				else if (JobHeader != null)
				{
					yield return JobHeader;
				}
			}
		}

		public virtual IEnumerable<ProcessHeader> ChildHeaders
		{
			get
			{
				foreach (var childLink in ChildLinks.Where(l => l.HeaderFrom != null))
				{
					yield return childLink.HeaderFrom;
				}
			}
		}

		public IEnumerable<ProcessHeader> DirectPrerequisites => PrerequisiteLinks.Select(p => p.HeaderFrom).WhereNotNull();

		public IEnumerable<ProcessHeader> DirectPostrequisites => PostrequisiteLinks.Select(p => p.HeaderTo).WhereNotNull();

		public IEnumerable<ProcessHeader> DirectParents
		{
			get
			{
				var directParents = ParentLinks
					.Select(l => l.HeaderTo)
					.WhereNotNull()
					.ToArray();

				if (FH_FH_ParentHeader.IsValid && directParents.All(p => p.FH_FH_ParentHeader != FH_FH_ParentHeader) && JobHeader != null) // if all links are external
				{
					return directParents.Prepend(JobHeader);
				}
				return directParents;
			}
		} 
		ILinkDescendantsStrategy ILinkEntity.GetDefaultDescendantsStrategy()
		{
			return new ProcessHeaderDescendantsStrategy();
		}

		#endregion

		#region Staggered Starts

		bool HasStaggeredDelayExpired => FH_StaggeredReleaseDelayExpiry.IsValid && FH_StaggeredReleaseDelayExpiry < ZDateTime.UtcNow;

		public void CalculateReleaseDelayExpiry()
		{
			FH_StaggeredReleaseDelayExpiry = GetValidStaggeredStartReleaseTime();
		}

		public ZDateTime GetValidStaggeredStartReleaseTime()
		{
			var prerequisiteHeaders = GetPrerequisitesAndTheirLinksUpTheTree(getApplicableDependenciesOnly: true).ToArray();
			var openPrerequisites = prerequisiteHeaders.Where(x => x.ProcessHeader.IsOpen).ToArray();

			return GetValidStaggeredStartReleaseTime(this, openPrerequisites);
		}

		internal static ZDateTime GetValidStaggeredStartReleaseTime(ProcessHeader workflow, LinkedProcessHeader[] openPrerequisiteAndTheirLinks)
		{
			var jobHeader = workflow.JobHeader;

			if (!openPrerequisiteAndTheirLinks.All(t => t.ProcessHeader.IsReleased))
			{
				return ZDateTime.Empty; // Can only start considering staggered release time once all prerequisites have been released
			}
			else
			{
				return openPrerequisiteAndTheirLinks.Length > 0 ? openPrerequisiteAndTheirLinks.Max(x => GetValidReleaseTime(x, jobHeader)) : ZDateTime.Empty;
			}
		}

		static ZDateTime GetValidReleaseTime(LinkedProcessHeader pair, ProcessJobHeader jobHeader)
		{
			var result = ZDateTime.Empty;

			var link = pair.LinkToProcessHeader;
			var timeDelayFactor = link.GetApplicableTimeDelayFactor(jobHeader);
			var timeDelayMinutes = link.GetApplicableAbsoluteGapMinutes(jobHeader);

			var isValid = !timeDelayFactor.IsEmpty || !timeDelayMinutes.IsEmpty;
			if (isValid)
			{
				var headerFrom = link.HeaderFrom;
				if (headerFrom.IsWorkflow)
				{
					if (headerFrom.FH_ReleaseDateTime.IsValid)
					{
						result = GetDelayExpiry(headerFrom, timeDelayFactor, timeDelayMinutes);
					}
				}
				else
				{
					var prereqJobHeader = (ProcessJobHeader)headerFrom;
					if (prereqJobHeader.ProcessHeaders.Count == 0)
					{
						result = ZDateTime.UtcNow;
					}
					else
					{
						var latestWorkflowInJob = prereqJobHeader == null
							? null
							: prereqJobHeader.ProcessHeaders
								.Where(w => w.FH_ReleaseDateTime.IsValid)
								.OrderByDescending(w => w.FH_ReleaseDateTime)
								.FirstOrDefault();

						if (latestWorkflowInJob != null)
						{
							result = GetDelayExpiry(latestWorkflowInJob, timeDelayFactor, timeDelayMinutes);
						}
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		static ZDateTime GetDelayExpiry(ProcessHeader prereq, decimal timeDelayFactor, int timeDelayMinutes)
		{
			var factorExpiry = GetDelayFactorExpiry(prereq, timeDelayFactor);
			var gapExpiry = GetTimeGapExpiry(prereq, timeDelayMinutes);

			if (!factorExpiry.IsValid)
			{
				return gapExpiry;
			}
			else if (!gapExpiry.IsValid)
			{
				return factorExpiry;
			}
			else
			{
				return factorExpiry > gapExpiry ? factorExpiry : gapExpiry;
			}
		}

		static ZDateTime GetDelayFactorExpiry(ProcessHeader prereq, decimal timeDelayFactor)
		{
			if (timeDelayFactor > 0)
			{
				var requiredGapHours = prereq.Tasks.Sum(t => t.StandardEstimateHours) * timeDelayFactor;
				return prereq.FH_ReleaseDateTime + TimeSpan.FromHours((double)requiredGapHours);
			}

			return ZDateTime.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		static ZDateTime GetTimeGapExpiry(ProcessHeader prereq, int timeDelayMinutes)
		{
			if (timeDelayMinutes > 0)
			{
				return prereq.FH_ReleaseDateTime + TimeSpan.FromMinutes(timeDelayMinutes);
			}

			return ZDateTime.Empty;
		}

		#endregion

		#region IBufferedItem Members

		IReadOnlyCollection<IBuffer> IBufferedItem.GetRelatedBuffers(bool includeNetworkBuffers)
		{
			if (includeNetworkBuffers)
			{
				var approvedShape = GetApprovedCCPMShape();
				var relatedNetworkBuffers = approvedShape != null ? approvedShape.GetRelatedBuffers(includeNetworkBuffers) : null;

				if (relatedNetworkBuffers != null && relatedNetworkBuffers.Count > 0)
				{
					return relatedNetworkBuffers;
				}
			}

			var timespanMins = EffectiveBufferDurationMinutes;
			if (timespanMins > 0)
			{
				return new List<IBuffer> { new BufferDTO { SizeInMinutes = timespanMins } };
			}

			var operationalBuffer = CurrentComponent;
			if (operationalBuffer != null && operationalBuffer.IsBuffer)
			{
				return new List<IBuffer> { operationalBuffer };
			}

			return new List<IBuffer>();
		}

		IReadOnlyCollection<IBuffer> IBufferedItem.RelatedSubBuffers
		{
			get
			{
				var operationalBuffer = CurrentComponent;
				return operationalBuffer.ChildComponents.Where(c => c.IsBuffer).ToList();
			}
		}

		DateTime IBufferedItem.StartableTime
		{
			get
			{
				if (!BMSRegistry.Instance.SynchroniseBufferPenetration.Value)
				{
					return FH_ReleaseDateTime.IsValid ? FH_ReleaseDateTime.ToDateTime() : default(DateTime);
				}

				var service = GetApprovedShapeBufferPenetrationService();
				var shapeDetails = service?.GetApprovedShapeDetails(this);

				if (shapeDetails != null && shapeDetails.HasRelatedBuffers)
				{
					return shapeDetails.StartableTimeUtc.ToDateTime();
				}

				var shape = service == null ? GetApprovedCCPMShape() : null;

				if (shape != null)
				{
					return shape.StartableTime;
				}

				return FH_ReleaseDateTime.IsValid ? FH_ReleaseDateTime.ToDateTime() : default(DateTime);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int IBufferedItem.RemainingEstimateInMinutes
		{
			get
			{
				if (!BMSRegistry.Instance.SynchroniseBufferPenetration.Value)
				{
					return (int)Utilities.Round(RemainingEstimateHours * 60m, 0);
				}

				var service = GetApprovedShapeBufferPenetrationService();
				var shape = service == null // This is a shortcut for avoiding checking for an approved shape when we've cached CCPM buffer penetration separately. No need to hit BMNCNShape table in this case.
					? GetApprovedCCPMShape() : null;

				if (shape != null)
				{
					return shape.RemainingEstimateInMinutes;
				}

				return (int)Utilities.Round(RemainingEstimateHoursIncludingChildren * 60m, 0);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int IBufferedItem.PlannedDurationInMinutes
		{
			get
			{
				if (!BMSRegistry.Instance.SynchroniseBufferPenetration.Value)
				{
					return FH_PlannedDurationInMinutes;
				}

				var service = GetApprovedShapeBufferPenetrationService();
				var shapeDetails = service?.GetApprovedShapeDetails(this);

				if (shapeDetails != null && shapeDetails.HasRelatedBuffers)
				{
					return shapeDetails.PlannedDurationInMinutes;
				}

				var shape = service == null ? GetApprovedCCPMShape() : null;

				if (shape != null)
				{
					return shape.PlannedDurationInMinutes;
				}

				return FH_PlannedDurationInMinutes;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int IBufferedItem.PenetrationMinutesWithoutAging
		{
			get { return this.CalculatePenetrationMinutesWithoutAging(); }
		}

		WorkStatus IBufferedItem.WorkStatus
		{
			get { return ((IProposedNetworkEntity)this).Status; }
		}

		public bool HasBufferPenetrationWhenClosed => true;

		bool IsInPlanningManagementMode => BMSRegistry.Instance.WorkflowManagementMode.Value == WorkflowManagementModes.Codes.PlanningManagement;

		IBMNCNShape GetApprovedCCPMShape()
		{
			if (!IsInPlanningManagementMode)
			{
				return null;
			}

			var approvedShape = ApprovedShape;

			return approvedShape?.IsBuffered ?? false ? approvedShape : null;
		}

		ApprovedShapeBufferPenetrationService GetApprovedShapeBufferPenetrationService()
		{
			if (!IsInPlanningManagementMode)
			{
				return null;
			}

			return Factory.ServiceContainer.GetService<ApprovedShapeBufferPenetrationService>();
		}

		#endregion

		#region IChildBufferedItem Members

		IBufferedItem IChildBufferedItem.Parent
		{
			get { return WorkflowParent; }
		}

		bool IChildBufferedItem.UseParentBufferPenetration
		{
			get { return SynchroniseBufferPenetration; }
		}

		#endregion

		#region IWorkflow Members

		Guid IWorkflow.PK
		{
			get { return PK.ToGuid(); }
		}

		Guid IWorkflow.ReleaseGroupPK
		{
			get { return FH_GG_ReleaseGroup.IsValid ? FH_GG_ReleaseGroup.ToGuid() : Guid.Empty; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		int IWorkflow.PlannedDurationMinutes
		{
			get { return FH_PlannedDurationInMinutes; }
			set { FH_PlannedDurationInMinutes = value; }
		}

		string IWorkflow.Description
		{
			get { return FH_CompletionStatement; }
		}

		DateTime IWorkflow.EarliestStartDateUtc
		{
			get { return FH_DoNotStartBeforeDate.IsValid ? FH_DoNotStartBeforeDate.ToDateTime() : default(DateTime); }
		}

		DateTime IWorkflow.JobEarliestStartDateUtc
		{
			get
			{
				var jobHeader = JobHeader;

				if (jobHeader != null && jobHeader.FH_DoNotStartBeforeDate.IsValid)
				{
					return jobHeader.FH_DoNotStartBeforeDate.ToDateTime();
				}

				return default(DateTime);
			}
		}

		string IWorkflow.JobDescription
		{
			get { return ParentJobDescription; }
		}

		string IWorkflow.WorkflowType
		{
			get { return Parent != null ? Parent.WorkflowType : ZString.Empty; }
		}

		string IWorkflow.Status
		{
			get { return FH_Status; }
		}

		IEnumerable<IWorkflowTask> IWorkflow.Tasks
		{
			get { return GetTasksWithoutAccessingWorkflowParent(); }
		}

		Guid IWorkflow.CurrentComponentPK
		{
			get { return FH_FC_CurrentComponent.IsValid ? FH_FC_CurrentComponent.ToGuid() : default(Guid); }
			set { FH_FC_CurrentComponent = value; }
		}

		Guid IWorkflow.JobLevelWorkflowPK
		{
			get { return FH_FH_ParentHeader.IsValid ? FH_FH_ParentHeader.ToGuid() : default(Guid); }
		}

		Guid IWorkflow.ParentId
		{
			get { return FH_ParentId.IsValid ? FH_ParentId.ToGuid() : default(Guid); }
		}

		string IWorkflow.ParentTableCode
		{
			get { return FH_ParentTableCode; }
		}

		DateTime IWorkflow.LastTransferDateUtc
		{
			get { return FH_ReleaseDateTime.IsValid ? FH_ReleaseDateTime.ToDateTime() : default(DateTime); }
			set { FH_ReleaseDateTime = value; }
		}

		string IWorkflow.LastTransferType
		{
			get => FH_LastTransferType;
			set => FH_LastTransferType = value;
		}

		public bool IsCcpmScheduleReleasable
		{
			get
			{
				var tagLink = (
					from l in this.GetApplicableTagLinks()
					let magnitude = l.Magnitude
					where magnitude != null
					where magnitude.TGM_Code == BMConstants.ReadyToReleaseTagCode
					let definition = magnitude.Definition
					where definition.TGD_Code == BMConstants.CCPMReleaseRulesTagGroupCode
					select l
					).FirstOrDefault();

				return tagLink != null;
			}
		}

		#endregion

		#region IWorkflowSortable Members

		ZDecimal IWorkflowOrderable.EffectiveNudge
		{
			get { return EffectiveNudge; }
		}

		ZDateTime IWorkflowOrderable.AgreedDeliveryDate
		{
			get { return ApplicableAgreedDeliveryDateUtc; }
		}

		ZDateTime IWorkflowOrderable.ReleaseSequenceSortDate
		{
			get { return ReleaseSequenceSortDate; }
		}

		ZDateTime IWorkflowOrderable.ReleaseDateTime
		{
			get { return FH_ReleaseDateTime; }
		}

		ZDateTime IWorkflowOrderable.CreateTime
		{
			get { return FH_SystemCreateTimeUtc; }
		}

		ZString IWorkflowOrderable.ReleaseSequence
		{
			get { return ReleaseSequence; }
		}

		#endregion

		#region IDateAcceptability Members

		ZString IDateAcceptability.FH_DateAcceptability => FH_DateAcceptability;

		ZPropertyInfo IDateAcceptability.FH_DateAcceptabilityInfo => FH_DateAcceptabilityInfo;

		#endregion

		#region IDefaultedFromDateProvider Members

		ZDateTimeOffset IDefaultedFromDateProvider.PredecessorDefaultDate => ZDateTimeOffset.Empty; // Workflows don't implement the concept of offsetting dates from their 'predecessor' like milestones do.

		IWorkflowProvider IDefaultedFromDateProvider.Parent => Parent;

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new ProcessHeaderUniqueIndexFailureHandler(); }
		}

		internal class ProcessHeaderUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				NotifyUserAndAttemptToResolveCore(notifier, indexName);
			}

			protected virtual void NotifyUserAndAttemptToResolveCore(INotificationHandler notifier, string indexName)
			{
				notifier.ReportError(
					Res.GetString("df078e77-ff5c-4d03-a1c7-85f73a29d18c", "There are duplicate job-level workflows. This form will need to be closed and re-opened to correct this problem."),
					Res.GetString("6b31e577-8918-48b7-97f4-6069c09d5107", "Cannot Save"));
			}

			public IEnumerable<string> HandledUniqueIndexNames => HandledUniqueIndexNamesCore;

			protected virtual IEnumerable<string> HandledUniqueIndexNamesCore
			{
				get { yield return ProcessHeaderSchema.Constants.Indexes.NR_UX__FH_ParentId_FH_ParentTableCode; }
			}
		}

		#endregion

		#region Responsive Actions

		public void PerformResponsiveAction(string actionCode)
		{
			switch (actionCode)
			{
				case ProcessHeaderResponsiveActionConstants.UpdateAllExceptDedicatedBuffer:
					UpdateEffectiveBranchAndDepartment();
					UpdateEffectiveNudge();
					UpdateEffectiveAgreedDeliveryDate();
					UpdateReleaseSequenceSortDate();
					UpdateLatestAcceptableReleaseDate();
					break;

				case ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer:
					MarkForDedicatedBufferUpdate();
					break;

				case ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment:
					UpdateEffectiveBranchAndDepartment();
					break;

				case ProcessHeaderResponsiveActionConstants.UpdateDedicatedBufferEffectiveBranchAndDepartment:
					MarkForDedicatedBufferUpdate();
					UpdateEffectiveBranchAndDepartment();
					break;

				default:
					ErrorReporter.ReportOnce($"Unsupported responsive action {actionCode}");
					break;
			}
		}

		void MarkForDedicatedBufferUpdate()
		{
			if (FH_IsActive)
			{
				FH_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			}
		}

		#endregion

		#region For Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			if (FH_WorkflowType.IsEmpty)
			{
				FH_WorkflowType = BMConstants.WorkflowTypeNotFoundCode;
			}

			FH_ParentTableCode = "OH"; // Because this column is in a unique index now, NewWithValidTestData wants to put nonsense in it, which fails when loading the Parent property.
		}

		protected override void BeforeSuccessfulDelete()
		{
			base.BeforeSuccessfulDelete();
			SystemLastEditTimeAfterDelete_ForTest = FH_SystemLastEditTimeUtc;
		}

		public ZDateTime SystemLastEditTimeAfterDelete_ForTest { get; private set; }

#endif
		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowCapabilityAssigner : IWorkflowCapabilityAssigner
	{
		public void AutoAssignWorkflowsImmediatelyOrDelayed(BusinessObjectFactory factory, IEnumerable<Guid> workflowPKs, ILogger logger)
		{
			Argument.NotNull(workflowPKs, nameof(workflowPKs));
			Argument.NotNull(logger, nameof(logger));
			var dataAccessor = new WorkflowCapabilityAssignerDataAccessor(logger); // dataAccessor should not ideally be created here and should be passed as a parameter; but let it be here for now (will be added to the interface later if needed)
			var workflows = dataAccessor.LoadWorkflowsByPKs(workflowPKs);
			var capacityAdjustmentCache = new Dictionary<string, decimal>();

			if (workflows.Any())
			{
				using (var relatedDataLoader = dataAccessor.GetRelatedDataLoader())
				{
					var branchPK = Guid.Empty;
					var departmentPK = Guid.Empty;
					IDisposable context = null;

					try
					{
						foreach (var sameBufferGroup in workflows.GroupBy(w => w.CurrentComponent))
						{
							var bufferComponent = sameBufferGroup.Key;
							var newBranchPK = bufferComponent.FC_GB_AgingBranch.ToGuid();
							var newDepartmentPK = bufferComponent.FC_GE_AgingDepartment.ToGuid();

							if (branchPK != newBranchPK || departmentPK != newDepartmentPK)
							{
								branchPK = newBranchPK;
								departmentPK = newDepartmentPK;
								context = SwitchBranchAndDepartment(context, branchPK, departmentPK);
							}

							var workingTimeContext = WorkingTimeContext.Create(bufferComponent);

							var workflowBatch = sameBufferGroup.ToArray();

#if DEBUG
							PerformPreAssignmentAction_ForTest(workflowBatch);
#endif
							try
							{
								AutoAssignWorkflows(relatedDataLoader, () => workflows.First().Factory.Save(), workflowBatch, capacityAdjustmentCache, bufferComponent, workingTimeContext, logger, allowScheduling: true);
							}
							catch (ZSaveConcurrencyException ex)
							{
								logger.Log(LogType.Warning, "Concurrency error: " + ex.Message); // Service Task Logging
							}
						}
					}
					finally
					{
						context?.Dispose();
					}
				}
			}
		}

		protected virtual IDisposable SwitchBranchAndDepartment(IDisposable context, Guid branchPK, Guid departmentPK)
		{
			context?.Dispose();
			return Env.SetTemporaryUserContext(Env.CurrentUserPK, branchPK, departmentPK);
		}

		internal void AutoAssignWorkflowsInBatchesImmediately(BMComponent bufferComponent, ILogger logger, IWorkflowCapabilityAssignerDataAccessor dataAccessor, string workflowBatchNameForLogging)
		{
			if (bufferComponent.FC_Type != BMComponentTypeList.Codes.Buffer)
			{
				return;
			}

			var workingTimeContext = WorkingTimeContext.Create(bufferComponent);
			var capacityAdjustmentCache = new Dictionary<string, decimal>();

			ProcessHeader[] workflowBatch;
			using (var workflowBatchProcessor = dataAccessor.GetWorkflowBatchProcessor(bufferComponent, workflowBatchNameForLogging))
			using (var relatedDataLoader = dataAccessor.GetRelatedDataLoader())
			{
				while ((workflowBatch = workflowBatchProcessor.LoadNextBatch().ToArray()).Any())
				{
#if DEBUG
					PerformPreAssignmentAction_ForTest(workflowBatch);
#endif
					try
					{
						AutoAssignWorkflows(relatedDataLoader, () => workflowBatchProcessor.SaveBatchChanges(), workflowBatch, capacityAdjustmentCache, bufferComponent, workingTimeContext, logger);
						using (ProcessTask.Loader.SuppressTemplateApplication())
						{
							workflowBatchProcessor.SaveBatchChangesAndReclaimMemory();
						}
					}
					catch (ZSaveConcurrencyException ex)
					{
						logger.Log(LogType.Warning, "Concurrency error: " + ex.Message); // Service Task Logging
					}
				}
			}
		}

		void AutoAssignWorkflows(IWorkflowCapabilityAssignerRelatedDataLoader relatedDataLoader, Action saveWorkflowChangesAction, ProcessHeader[] workflows, Dictionary<string, decimal> capacityAdjustmentCache, BMComponent bufferComponent, WorkingTimeContext workingTimeContext, ILogger logger, bool allowScheduling = false)
		{
			if (!workflows.Any())
			{
				return;
			}

			var componentReleaseGroupLinksGroupedByReleaseGroupKey = relatedDataLoader.GetComponentReleaseGroupLinks(bufferComponent?.PK).ToDictionary(l => l.FO_GG_ReleaseGroup);
			var workflowTaskDictionary = new Dictionary<ProcessHeader, ProcessTask[]>();

			foreach (var workflow in workflows)
			{
				var tasks = relatedDataLoader.GetWorkflowTasksForAutoAssignment(workflow).ToArray();
				workflowTaskDictionary.Add(workflow, tasks);
			}

			foreach (var (workflow, tasks) in workflowTaskDictionary.Select(k => (k.Key, k.Value)))
			{
				componentReleaseGroupLinksGroupedByReleaseGroupKey.TryGetValue(workflow.FH_GG_ReleaseGroup, out BMComponentReleaseGroupLink releaseGroupLink);

				var delayedLogger = new DelayedLogger(logger);

				AutoAssignWorkflowCore(relatedDataLoader, workflow, tasks, capacityAdjustmentCache, releaseGroupLink, workingTimeContext, delayedLogger, bufferComponent, allowScheduling);

				try
				{
					if (workflow.HasChanges || tasks.Any(t => t.HasChanges))
					{
						using (ProcessTask.Loader.SuppressTemplateApplication())
						{
							saveWorkflowChangesAction();
						}
					}

					delayedLogger.FlushLogs(true);
				}
				catch (ZSaveException)
				{
					delayedLogger.FlushLogs(false);
					throw;
				}
			}
		}

		static bool CanAutoAssignTasksByReleaseGroup(GlbCapability capability)
		{
			return capability.ReleaseGroupPivots.Any() && capability.G4_CapacityScope == GlbCapabilityScopeList.Codes.GroupScope;
		}

		void AutoAssignWorkflowCore(IWorkflowCapabilityAssignerRelatedDataLoader relatedDataLoader, ProcessHeader workflow, ProcessTask[] tasks, IDictionary<string, decimal> capacityAdjustmentCache, BMComponentReleaseGroupLink releaseGroupLink, WorkingTimeContext workingTimeContext, DelayedLogger logger, BMComponent bufferForCapacity, bool allowScheduling)
		{
			var componentAutoAssignmentAge = bufferForCapacity.FC_AutoAssignTasksAge;
			var releaseGroupLinkAutoAssignmentAge = releaseGroupLink?.FO_AutoAssignTasksAge ?? ZDateTime.Empty;
			var taskAllocationBlocks = TaskAssignmentHelper.GetTaskAllocationBlocks(tasks, WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.Value);

			CapabilityTaskAssignmentScheduleForWorkflow assignmentSchedule = null;

			foreach (var taskGroup in taskAllocationBlocks)
			{
				var openTasks = taskGroup.Value.Where(t => t.IsOpen).ToArray();

				if (!openTasks.Any())
				{
					continue;
				}

				var capability = taskGroup.Key;

				if (!capability.G4_AllowTaskAutoAssignment && !CanAutoAssignTasksByReleaseGroup(capability))
				{
					continue;
				}

				var duplicateTasks = tasks.GroupBy(x => x.P9_TaskID).Where(g => g.Count() > 1).Select(y => y.Key);

				if (duplicateTasks.Any())
				{
					ErrorReporter.ReportOnce(
						"DuplicateTaskInTaskAllocationBlock",
						string.Format(
							CultureInfo.InvariantCulture,
							DuplicateTaskErrorMessage,
							capability.ToString(),
							string.Join(", ", tasks.Select(t => GetProcessTaskAs(t))),
							string.Join(", ", duplicateTasks.ToString()))); // logging ranks in the OWASP top 10 reasons for security defects and as someone implementing logging I am making a security override of this code smell
				}

				var timeAfterWhichAutoAssignmentCanHappen = GetTimeAfterWhichAutoAssignmentCanHappen(capability, logger, componentAutoAssignmentAge, releaseGroupLinkAutoAssignmentAge, workflow.FH_GG_ReleaseGroup);
				AutoAssignTasksOrProposeSchedule(relatedDataLoader, timeAfterWhichAutoAssignmentCanHappen, bufferForCapacity, workflow, openTasks, capacityAdjustmentCache, capability, workingTimeContext, logger, allowScheduling, ref assignmentSchedule);
			}

			if (assignmentSchedule != null && assignmentSchedule.IsSchedulingRequired)
			{
				ScheduleAutoAssignment(workflow, assignmentSchedule, logger);
			}
		}

		static ZDateTime GetTimeAfterWhichAutoAssignmentCanHappen(GlbCapability capability, DelayedLogger logger, ZDateTime componentAutoAssignmentAge, ZDateTime releaseGroupLinkAutoAssignmentAge, ZGuid releaseGroupPK)
		{
			var canAutoAssignByReleaseGroup = CanAutoAssignTasksByReleaseGroup(capability);
			if (canAutoAssignByReleaseGroup)
			{
				var releaseGroupCapabilityAutoAssignmentAge = capability.ReleaseGroupPivots.SingleOrDefault(p => p.GGC_GG_Group == releaseGroupPK && p.GGC_AllowTaskAutoAssignment)?
					.GGC_AutoAssignTasksAge ?? ZDateTime.Empty;

				if (releaseGroupCapabilityAutoAssignmentAge.IsValid)
				{
					logger.LogAlways(LogType.Information, string.Format(CultureInfo.InvariantCulture, (NoResString)"Auto-assigning tasks using release group capability auto-assigning task age.")); // Service task logging

					return releaseGroupCapabilityAutoAssignmentAge;
				}

				canAutoAssignByReleaseGroup = false;
			}

			var capabilityAutoAssignmentAge = capability.G4_AutoAssignTasksAge;

			if (capabilityAutoAssignmentAge.IsValid && capability.G4_AllowTaskAutoAssignment && !canAutoAssignByReleaseGroup)
			{
				logger.LogAlways(LogType.Information, string.Format(CultureInfo.InvariantCulture, (NoResString)"Auto-assigning tasks using capability auto-assigning task age.")); // Service task logging

				return capabilityAutoAssignmentAge;
			}
			else if (releaseGroupLinkAutoAssignmentAge.IsValid)
			{
				logger.LogAlways(LogType.Information, string.Format(CultureInfo.InvariantCulture, (NoResString)"Auto-assigning tasks using component release group link auto-assigning task age."));// Service task logging

				return releaseGroupLinkAutoAssignmentAge;
			}
			else
			{
				logger.LogAlways(LogType.Information, string.Format(CultureInfo.InvariantCulture, (NoResString)"Auto-assigning tasks using CurrentComponent auto-assigning task age."));// Service task logging

				return componentAutoAssignmentAge;
			}
		}

		static void AutoAssignTasksOrProposeSchedule(IWorkflowCapabilityAssignerRelatedDataLoader relatedDataLoader,
			ZDateTime timeAfterWhichAutoAssignmentCanHappen,
			BMComponent buffer,
			ProcessHeader workflow,
			ICollection<ProcessTask> sameCapabilityOpenTasks,
			IDictionary<string, decimal> capacityAdjustmentCache,
			GlbCapability capability,
			WorkingTimeContext workingTimeContext,
			DelayedLogger logger,
			bool allowScheduling,
			ref CapabilityTaskAssignmentScheduleForWorkflow assignmentSchedule)
		{
			if (!sameCapabilityOpenTasks.Any())
			{
				return;
			}

			var targetAgeForAutoAssignment = TimeSpan.FromMinutes(timeAfterWhichAutoAssignmentCanHappen.GetMinutesFromDateTimeSpan());
			var localReleaseTime = relatedDataLoader.GetWorkflowReleaseLocalTime(workingTimeContext, workflow);
			var localNow = relatedDataLoader.GetCurrentLocalTime(workingTimeContext);

			var workTimeArithmetic = relatedDataLoader.GetWorkTimeArithmetic(workingTimeContext);
			var age = workTimeArithmetic.TimeDifference(localReleaseTime, localNow);

			if (age < targetAgeForAutoAssignment)
			{
				if (allowScheduling)
				{
					var targetTimeUtc = ZDateTime.UtcNow + targetAgeForAutoAssignment;
					assignmentSchedule = ProposeAutoAssignmentSchedule(assignmentSchedule, targetTimeUtc, sameCapabilityOpenTasks, capability);
					LogTasksRequireAssignmentLater(logger, sameCapabilityOpenTasks, capability, workflow);
				}
				else
				{
					ReportTooEarlyForAutoAssignment(workflow, localReleaseTime, age, targetAgeForAutoAssignment, logger);
				}

				return;
			}

			if (capability.G4_CapacityScope == GlbCapabilityScopeList.Codes.GroupScope)
			{
				var openTasksWithoutTaskGroup = sameCapabilityOpenTasks.Where(t => t.P9_GG_AssignedGroup.IsEmpty).ToList();
				var openTasksWithTaskGroup = sameCapabilityOpenTasks.Where(t => !t.P9_GG_AssignedGroup.IsEmpty).GroupBy(g => g.P9_GG_AssignedGroup);

				foreach (var tasks in openTasksWithTaskGroup)
				{
					var groupPK = tasks.Key;
					var tasksToAssign = WhereCapabilitySpecificReleaseGroupConfigurationIsEmptyOrEnabled(tasks, capability, groupPK).ToList();
					MaybeAutoAssignTasks(relatedDataLoader, buffer, workflow, capacityAdjustmentCache, capability, workingTimeContext, logger, tasksToAssign, groupPK, shouldReport: false);
				}

				var unassignedTaskGroupTasks = sameCapabilityOpenTasks.Where(t => !t.P9_GG_AssignedGroup.IsEmpty && t.P9_GS_NKAssignedStaffMember.IsEmpty);
				if (unassignedTaskGroupTasks.Any())
				{
					openTasksWithoutTaskGroup.AddRange(unassignedTaskGroupTasks);
				}

				openTasksWithoutTaskGroup = WhereCapabilitySpecificReleaseGroupConfigurationIsEmptyOrEnabled(openTasksWithoutTaskGroup, capability, workflow.FH_GG_ReleaseGroup).ToList();

				MaybeAutoAssignTasks(relatedDataLoader, buffer, workflow, capacityAdjustmentCache, capability, workingTimeContext, logger, openTasksWithoutTaskGroup, workflow.FH_GG_ReleaseGroup);
			}
			else
			{
				MaybeAutoAssignTasks(relatedDataLoader, buffer, workflow, capacityAdjustmentCache, capability, workingTimeContext, logger, sameCapabilityOpenTasks.ToList(), workflow.FH_GG_ReleaseGroup);
			}
		}

		public static IEnumerable<ProcessTask> WhereCapabilitySpecificReleaseGroupConfigurationIsEmptyOrEnabled(IEnumerable<ProcessTask> tasks, GlbCapability capability, ZGuid releaseGroupPK)
		{
			return tasks.Where(task =>
			{
				var releaseGroupPivot = capability.ReleaseGroupPivots.FirstOrDefault(p => p.GGC_GG_Group == releaseGroupPK);
				return releaseGroupPivot == null || releaseGroupPivot.GGC_AllowTaskAutoAssignment;
			});
		}

		static void MaybeAutoAssignTasks(IWorkflowCapabilityAssignerRelatedDataLoader relatedDataLoader, BMComponent buffer, ProcessHeader workflow, IDictionary<string, decimal> capacityAdjustmentCache, GlbCapability capability, WorkingTimeContext workingTimeContext, DelayedLogger logger, List<ProcessTask> openTasks, ZGuid groupPK, bool shouldReport = true)
		{
			if (openTasks.Count == 0)
			{
				return;
			}

			var resourcesWithCapability = relatedDataLoader.GetWorkingResourcesWithCapability(capability.PK, workingTimeContext, groupPK).ToArray();

			if (resourcesWithCapability.Length <= 0)
			{
				if (shouldReport)
				{
					ReportNoActiveResourcesHaveCapability(openTasks, workflow, capability, logger);
					CreateFailureWorkItemIfNecessary(capability, workflow, openTasks);
				}

				return;
			}

			var capacityBreakdowns = CapacityCalculator.GetUtilisedCapacityBreakdown(resourcesWithCapability, buffer);

			AutoAssignTasks(relatedDataLoader, openTasks, capacityBreakdowns, capacityAdjustmentCache, buffer, workflow, capability, logger);
		}

		static CapabilityTaskAssignmentScheduleForWorkflow ProposeAutoAssignmentSchedule(CapabilityTaskAssignmentScheduleForWorkflow oldSchedule, ZDateTime targetTimeUtc, IEnumerable<ProcessTask> tasks, GlbCapability capability)
		{
			if (oldSchedule == null || !oldSchedule.IsSchedulingRequired || targetTimeUtc < oldSchedule.TargetTimeUtc)
			{
				return new CapabilityTaskAssignmentScheduleForWorkflow(targetTimeUtc, tasks, capability);
			}

			if (targetTimeUtc > oldSchedule.TargetTimeUtc)
			{
				return oldSchedule;
			}

			oldSchedule.EarliestTasks.Add(new TasksAndRequiredCapability(tasks, capability));
			return oldSchedule;
		}

		void ScheduleAutoAssignment(ProcessHeader workflow, CapabilityTaskAssignmentScheduleForWorkflow schedule, DelayedLogger logger)
		{
			ActionScheduleProvider.ScheduleAction(CapabilityAutoAssignmentSchedulerActionCode, schedule.TargetTimeUtc, workflow.PK, TargetTableForScheduling);
			LogSuccesfullyScheduledTasks(logger, schedule, workflow);
		}

		IActionScheduleProvider ActionScheduleProvider => actionScheduleProvider.Value;

		readonly Lazy<IActionScheduleProvider> actionScheduleProvider = new Lazy<IActionScheduleProvider>(() => ObjectFactory.Get<IActionScheduleProvider>());

		const string CapabilityAutoAssignmentSchedulerActionCode = "ACT";
		const string TargetTableForScheduling = "FH";

		static void AutoAssignTasks(
			IWorkflowCapabilityAssignerRelatedDataLoader relatedDataLoader,
			ICollection<ProcessTask> sameCapabilityOpenTasks,
			IDictionary<GlbStaff, IResourceCapacity> capacityBreakdowns,
			IDictionary<string, decimal> capacityAdjustmentCache,
			BMComponent buffer,
			ProcessHeader workflow,
			GlbCapability capability,
			DelayedLogger logger)
		{
			if (relatedDataLoader.DifRestrictionsExistInRegistry())
			{
				AutoAssignTasksWithDifRestrictions(sameCapabilityOpenTasks, capacityBreakdowns, capacityAdjustmentCache, buffer, workflow, capability, logger);
			}
			else
			{
				AutoAssignTasksWithoutDifRestrictions(sameCapabilityOpenTasks, capacityBreakdowns, capacityAdjustmentCache, buffer, workflow, capability, logger);
			}
		}

		static void AutoAssignTasksWithDifRestrictions(ICollection<ProcessTask> sameCapabilityOpenTasks, IDictionary<GlbStaff, IResourceCapacity> capacityBreakdowns, IDictionary<string, decimal> capacityAdjustmentCache, BMComponent buffer, ProcessHeader workflow, GlbCapability capability, DelayedLogger logger)
		{
			foreach (var capacity in capacityBreakdowns.OrderByDescending(r => r.Value.AvailableCapacity))
			{
				var tasksToAssign = sameCapabilityOpenTasks.Where(t => t.P9_GS_NKAssignedStaffMember.IsEmpty);
				if (!tasksToAssign.Any())
				{
					break;
				}

				var resourceTasks = new List<ProcessTask>();
				foreach (var task in tasksToAssign)
				{
					if (TaskAssignmentHelper.DoRestrictionsAllowResourceToBeAssignedToTasks(capacity.Key.GS_Code, resourceTasks.Concat(task.WrapWithEnumerable())))
					{
						resourceTasks.Add(task);
					}
				}

				if (resourceTasks.Any() && CapacityIsEnoughForAssignment(capacity.Value, resourceTasks, capacityAdjustmentCache))
				{
					AssignTasksAndReport(resourceTasks, buffer, capacity.Key, workflow, capability, logger, capacityBreakdowns, capacityAdjustmentCache);
				}
			}

			var unassignedTasks = sameCapabilityOpenTasks.Where(t => t.P9_GS_NKAssignedStaffMember.IsEmpty);
			if (unassignedTasks.Any())
			{
				ReportUnassignedTasks(unassignedTasks, workflow, capability, logger);
			}
		}

		static void AutoAssignTasksWithoutDifRestrictions(ICollection<ProcessTask> sameCapabilityOpenTasks, IDictionary<GlbStaff, IResourceCapacity> capacityBreakdowns, IDictionary<string, decimal> capacityAdjustmentCache, BMComponent buffer, ProcessHeader workflow, GlbCapability capability, DelayedLogger logger)
		{
			KeyValuePair<GlbStaff, IResourceCapacity>? highestCapacityResource = GetHighestCapacityResource(sameCapabilityOpenTasks, capacityBreakdowns);

			if (highestCapacityResource == null)
			{
				ReportNoActiveResourcesHaveCapability(sameCapabilityOpenTasks, workflow, capability, logger);
				return;
			}

			if (!CapacityIsEnoughForAssignment(highestCapacityResource?.Value, sameCapabilityOpenTasks, capacityAdjustmentCache))
			{
				ReportNoResourcesHaveEnoughCapacity(sameCapabilityOpenTasks, workflow, capability, logger);
				return;
			}

			AssignTasksAndReport(sameCapabilityOpenTasks, buffer, highestCapacityResource?.Key, workflow, capability, logger, capacityBreakdowns, capacityAdjustmentCache);
		}

		static KeyValuePair<GlbStaff, IResourceCapacity>? GetHighestCapacityResource(ICollection<ProcessTask> sameCapabilityOpenTasks, IDictionary<GlbStaff, IResourceCapacity> capacityBreakdowns)
		{
			if (WorkflowDataRegistry.Instance.RequireResourceToHaveCapability.Value)
			{
				var resourcesWithCapability = capacityBreakdowns
					.Where(kvp => kvp.Key.Capabilities.Any(cap => cap.PK.Equals(sameCapabilityOpenTasks.First().P9_G4_RequiredCapability)));

				if (resourcesWithCapability.Any())
				{
					return resourcesWithCapability.MaxBy(kvp => kvp.Value.AvailableCapacity);
				}
				else
				{
					return null;
				}
			}
			else
			{
				return capacityBreakdowns.MaxBy(kvp => kvp.Value.AvailableCapacity);
			}
		}

		static bool CapacityIsEnoughForAssignment(IResourceCapacity capacity, IEnumerable<ProcessTask> tasks, IDictionary<string, decimal> capacityAdjustmentCache)
		{
			var requiredCapacity = tasks.Sum(t => t.RelevantEstimateHours);
			capacityAdjustmentCache.TryGetValue(capacity.StaffCode, out var adjustValue);
			return BMSRegistry.Instance.AutoAssignTasksRegardlessCapacity.Value || (capacity.AvailableCapacity - adjustValue) * 2 >= requiredCapacity;
		}

		static void AssignTasksAndReport(ICollection<ProcessTask> tasks, BMComponent buffer, GlbStaff resource, ProcessHeader workflow, GlbCapability capability, DelayedLogger logger, IDictionary<GlbStaff, IResourceCapacity> capacityBreakdowns, IDictionary<string, decimal> capacityAdjustmentCache)
		{
			var resourceCode = resource.GS_Code;
			var resourceName = resource.GS_FullName;
			var assignedTaskPKs = new HashSet<ZGuid>();

			foreach (var task in tasks)
			{
				if (!assignedTaskPKs.Contains(task.PK))
				{
					task.P9_GS_NKAssignedStaffMember = resourceCode;

					if (!capacityAdjustmentCache.TryGetValue(resourceCode, out var adjustCapacity))
					{
						capacityAdjustmentCache[resourceCode] = task.StandardEstimateHours;
					}
					else
					{
						capacityAdjustmentCache[resourceCode] += task.StandardEstimateHours;
					}
				}

				assignedTaskPKs.UnionWith(task.RelatedTaskPKsFromLastTaskAutoAssignment);
			}

			var availableResourceStringList = new List<string>();
			var estimate = tasks.Sum(t => t.StandardEstimateHours);

			if (BMSRegistry.Instance.CacheCalculatedCapacity.Value)
			{
				var cache = BufferCapacityCache.Get(buffer.PK);
				cache.DeductInMemoryCapacity(resourceCode, estimate);
				var capacityInCache = cache.GetCapacity(resourceCode, workflow.Factory);

				foreach (var capacity in capacityBreakdowns.OrderByDescending(r => r.Value.AvailableCapacity).ThenBy(r => r.Key.GS_FullName))
				{
					var capacityLog = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", capacity.Key.GS_FullName, capacity.Value.AvailableCapacity); // Service task logging
					if (resourceCode == capacity.Key.GS_Code && capacityInCache != null)
					{
						capacityLog += string.Format(CultureInfo.InvariantCulture, (NoResString)" ({0} after assignment)", capacityInCache.AvailableCapacity); // Service task logging
					}
					availableResourceStringList.Add(capacityLog);
				}
			}

			var appendString = "";

			if (!availableResourceStringList.IsNullOrEmpty())
			{
				appendString += (NoResString)"\nResource Capacity at the time of assignment (in hours):\n" + string.Join((NoResString)"\n", availableResourceStringList); // Service task logging
			}

			if (assignedTaskPKs.Any())
			{
				appendString += (NoResString)"\nTask assignment resulted in the assigning of related tasks.\n"; // Service task logging
			}

			LogSuccesfullyAssignedTasks(appendString, logger, tasks, capability, workflow, resourceName, estimate);
		}

		static void CreateFailureWorkItemIfNecessary(GlbCapability capability, ProcessHeader workflow,
			ICollection<ProcessTask> tasks)
		{
			if (!BMSRegistry.Instance.AutoAssignmentCapabilityTasksFailure.Value.Enabled)
			{
				return;
			}

			ZString summary = (capability.G4_CapacityScope == GlbCapabilityScopeList.Codes.GlobalScope)
				? Res.GetString("2C45EE12-A5C0-4C3C-AFAB-1E0C6A6A7036",
					"Capability task auto assignment failure for capability {0}", capability.G4_Code)
				: Res.GetString("32883D71-5B97-49AC-9401-B1CCD8A2025F",
					"Capability task auto assignment failure for capability {0} and group {1}",
					capability.G4_Code, workflow.ReleaseGroup.GG_Code);

			ZString reasonForFailure = capability.G4_CapacityScope == GlbCapabilityScopeList.Codes.GlobalScope
				? Res.GetString("0191A226-312A-4713-87C9-17E2AE497D02", "Global Capability has no active members")
				: Res.GetString("16756E2D-848D-418C-8EEB-3B5278C898ED",
					"Group Capability - Release Group intersection has no active members");

			var details = ZString.Empty;
			if (WorkflowDescriptors.Instance.TryGetValue(workflow.FH_WorkflowType, out var descriptor))
			{
				details = string.Format(CultureInfo.InvariantCulture, @"{{\rtf1
Time (UTC): {0}\line
Job: {{\colortbl;\red0\green0\blue255;}}{{\field{{\*\fldinst{{HYPERLINK ""{1}""}}}}{{\fldrslt{{\cf1\ul {2}}}}}}}\line
Workflow: {3}\line
Release Group: {4}\line
Task: {5}\line
Capability: {6} - {7}\line
Reason: {8}
}}", ZDateTime.UtcNow,
					ObjectFactory.Get<IShowEditFormUrlCreator>().Create(descriptor.ControllerID, workflow.Parent.PK.ToGuid()),
					(workflow.Parent as BusinessObject).HumanReadableName,
					workflow.FH_CompletionStatement,
					capability.G4_CapacityScope == GlbCapabilityScopeList.Codes.GlobalScope ? ZString.Empty : workflow.ReleaseGroup.GG_Code,
					tasks.First().P9_Description,
					capability.G4_Code, capability.G4_Description,
					reasonForFailure); // Rtf formatting
			}

			ObjectFactory.Get<IWorkItemHelper>().CreateWorkItem(workflow.Factory,
				checkExistingWorkItem: true,
				BMSRegistry.Instance.AutoAssignmentCapabilityTasksFailure.Value.Criterion1,
				BMSRegistry.Instance.AutoAssignmentCapabilityTasksFailure.Value.Criterion2,
				BMSRegistry.Instance.AutoAssignmentCapabilityTasksFailure.Value.Criterion3,
				BMSRegistry.Instance.AutoAssignmentCapabilityTasksFailure.Value.Criterion4,
				BMSRegistry.Instance.AutoAssignmentCapabilityTasksFailure.Value.Criterion5,
				summary,
				details);
		}

		#region Logging Action

		static void LogTasksRequireAssignmentLater(DelayedLogger logger, ICollection<ProcessTask> tasks, GlbCapability capability, ProcessHeader workflow)
		{
			logger.LogIfSaveSuccessful(LogType.Information, string.Format(CultureInfo.InvariantCulture, (NoResString)"Tasks [{0}] requiring capability [{1}] in workflow [{2}] require delayed assignment.", // Service task logging
				GetTaskIDs(tasks), capability.G4_Code, workflow.DescriptionWithReleaseGroup));
		}

		static void LogSuccesfullyScheduledTasks(DelayedLogger logger, CapabilityTaskAssignmentScheduleForWorkflow schedule, ProcessHeader workflow)
		{
			var builder = new ZStringBuilder();

			foreach (var tasksAndCapability in schedule.EarliestTasks)
			{
				builder.Append(string.Format(CultureInfo.InvariantCulture, (NoResString)"- [{0}] with required capability [{1}]", // Service task logging
					GetTaskIDs(tasksAndCapability.Tasks), tasksAndCapability.RequiredCapability.G4_Code));
			}

			logger.LogIfSaveSuccessful(LogType.Information, string.Format(CultureInfo.InvariantCulture, (NoResString)@"Scheduled assigning capability tasks in workflow [{0}] at [{1}]. The earliest tasks requiring assignment are as follows:{2}{3}", // Service task logging
				workflow.DescriptionWithReleaseGroup, schedule.TargetTimeUtc, System.Environment.NewLine, builder.ToStringWithNewLineBetweenAppends()));
		}

		static void LogSuccesfullyAssignedTasks(string appendString, DelayedLogger logger, ICollection<ProcessTask> tasks, GlbCapability capability, ProcessHeader workflow, string resourceName, decimal estimate)
		{
			logger.LogIfSaveSuccessful(LogType.Information, string.Format(CultureInfo.InvariantCulture, (NoResString)"Assigned tasks [{0}] requiring capability [{1}] in workflow [{2}] to resource [{3}], consuming [{4}] hours of capacity." + appendString, // Service task logging
				GetTaskIDs(tasks), capability.G4_Code, workflow.DescriptionWithReleaseGroup, resourceName, estimate));
		}

		static void ReportUnassignedTasks(IEnumerable<ProcessTask> tasks, ProcessHeader workflow, GlbCapability capability, DelayedLogger logger)
		{
			logger.LogAlways(LogType.Information, string.Format(CultureInfo.InvariantCulture, (NoResString)"Could not auto assign tasks [{0}] requiring capability [{1}] in workflow [{2}] because no resources have enough capacity or they are not allowed to be assigned by task autoassignment restrictions.", // Service task logging
				GetTaskIDs(tasks), capability.G4_Code, workflow.DescriptionWithReleaseGroup));
		}

		static void ReportNoResourcesHaveEnoughCapacity(IEnumerable<ProcessTask> tasks, ProcessHeader workflow, GlbCapability capability, DelayedLogger logger)
		{
			logger.LogAlways(LogType.Information, string.Format(CultureInfo.InvariantCulture, (NoResString)"Could not auto assign tasks [{0}] requiring capability [{1}] in workflow [{2}] because no resources have enough capacity.", // Service task logging
				GetTaskIDs(tasks), capability.G4_Code, workflow.DescriptionWithReleaseGroup));
		}

		static void ReportNoActiveResourcesHaveCapability(IEnumerable<ProcessTask> tasks, ProcessHeader workflow, GlbCapability capability, DelayedLogger logger)
		{
			logger.LogAlways(LogType.Information, string.Format(CultureInfo.InvariantCulture, (NoResString)"Could not auto-assign tasks [{0}] in workflow [{1}] because capability don't have assign tasks group enabled or no active staff [{2}] capability.", // Service task logging
				GetTaskIDs(tasks), workflow.DescriptionWithReleaseGroup, capability.G4_Code));
		}

		static void ReportTooEarlyForAutoAssignment(ProcessHeader workflow, ZDateTime localReleaseTime, TimeSpan workflowAge, TimeSpan targetAgeForAutoAssignment, DelayedLogger logger)
		{
			logger.LogAlways(LogType.Information, string.Format(CultureInfo.InvariantCulture, (NoResString)"Could not auto-assign tasks in workflow [{0}] since it was released at {1}, which is {2} working hours ago. (Minimum hours before auto assignment is {3})", // Service task logging
				workflow.DescriptionWithReleaseGroup, localReleaseTime.ToStandardDateTimeString(), workflowAge.ToHoursAndMinutesString(), targetAgeForAutoAssignment.ToHoursAndMinutesString()));
		}

		static string GetTaskIDs(IEnumerable<ProcessTask> tasks) => string.Join(", ", tasks.Select(t => t.P9_TaskID).OrderBy(t => t));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "you gonna stop me? Ok fine, ErrorReporter string only, buddy.")]
		const string DuplicateTaskErrorMessage = @"Duplicate tasks were returned in a single Task Allocation Block.
The grouping capability is:	{0}.
The task IDs involved are:	{1}
The duplicate tasks are:	{2}";

		/// <summary>
		/// Because ProcessTask is so bloated, literally adding "ToString" method where none existed isn't possible without breaking things
		/// </summary>
		static string GetProcessTaskAs(ProcessTask task)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}: {1}-{2}-{3}-{4}", nameof(ProcessTask), task.P9_Type, task.P9_TaskID, task.P9_Description, task.P9_SE_NKMilestoneEvent);
		}

		#endregion

		#region For Test
#if DEBUG

		protected virtual void PerformPreAssignmentAction_ForTest(ProcessHeader[] workflowBatch)
		{
		}

#endif
		#endregion
	}
}

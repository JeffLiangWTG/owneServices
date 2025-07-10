using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	/// <summary>
	/// This class handles the business logic and cached state associated with the capacity reservation system.
	/// </summary>
	/*
	   This system is the thing that decides whether one or more resources have enough capacity to release a workflow to a buffer. This is more complex than just "do the resources have enough capacity for all the tasks" for two reasons:
	   
	   1. There's a queueing system. The sequence in which work is released is very important - a high-priority workflow like a defect fix should be released before a non-urgent feature, for example. So if one resource blocks the release of that defect fix,
	   then we reserve all the capacity required across the other resources too, so that those resources don't then have a whole bunch of other capacity free that less important work can utilise. If we didn't reserve capacity like this, then large chunks would
	   often be delayed by many smaller chunks getting released when there is insufficient capacity for the large chunk.
	   
	   2. Calculating the capacity requirements for capability tasks is not super simple. In essence, the algorithm could be described as "is there enough capacity across all the resources who possess the capability for the capability task estimates?"
	   But there are other considerations. Like, we only consider resources within the workflow's release group, if the capability is so configured. And we don't allow individual resource's negative capacity to impact the overall capability capacity beyond a certain floor limit.
	   
	   There are some other important things to consider:
	   1. If the workflow involves a Capacity Constrained Resource (CCR) then different rules for calculating Available Capacity are used.
	   2. The task Standard Estimate may be overridden by the Estimated Time to Complete field if the user has entered one for a particular task.
	   3. The impact on Utilised Capacity may not be exactly the same as the relevant estimated duration if there is a Zone Multiplier configured for the buffer/release group. We consider this multiplier when considering if work can be released to a buffer.
	   4. Some task types can be configured to bypass capacity entirely. For example at WTG, checkin tasks don't matter for the chunking requirement or when considering their capacity impact. 
	   5. When resources are on leave, they may not be allowed to have any work released to them for a period up until their return. This is configurable, and overrides their available capacity (if any).
	   6. Work on an approved CCPM diagram will be released according to its approved schedule regardless of capacity. However we still reserve the capacity impact of this work, so that other work further back in the queue can be held back within the same release gate run if the CCPM work uses all their available capacity.
	   7. Workflows won't be released to a buffer unless there is at least one incomplete task requiring capacity of a resource (or resources who possess a capability), OR all incomplete tasks are set to 'ignored' in the Task Types registry item.
	 	*/
	public sealed class CapacityReservationTracker
	{
		public CapacityReservationTracker(BMComponent buffer, IDictionary<GlbStaff, IResourceCapacity> capacities, bool trackResourceQueues)
		{
			// We trust that the release gate will pass all capacities that will later be required.
			Argument.NotNull(capacities, nameof(capacities));
			Argument.NotNull(buffer, nameof(buffer));

			buffer.RequireBuffer();

			Buffer = buffer;
			AllStaff = capacities.Keys.ToDictionary(s => s.GS_Code, s => s);
			ResourcesWithCapabilityCache = new WorkingResourcesWithCapabilityCache(buffer);
			TrackedAvailableCapacityByResource = capacities.ToDictionary(k => k.Key.GS_Code, k => k.Value.Clone());

			if (trackResourceQueues)
			{
				PlaceInQueueByResource = new Dictionary<string, int>();
			}
		}

		public CapacityReservationReport BuildCapacityReservationReportAndUpdateTrackedCapacityReservations(IWorkflow workflow, BusinessObjectFactory factory)
		{
			var shouldUseConstrainedModeCapacityCalculation = ShouldUseConstrainedModeCapacityCalculationForNonCCRs(workflow);
			var releaseGroupPK = workflow.ReleaseGroupPK == default ? ZGuid.Empty : workflow.ReleaseGroupPK;
			var zoneMultiplier = GetZoneMultiplier(releaseGroupPK, Buffer.Factory);
			var report = new CapacityReservationReport(Buffer, workflow, shouldUseConstrainedModeCapacityCalculation, zoneMultiplier);

			FillReport(report, workflow, factory);

			return report;
		}

		#region Report Setup

		void FillReport(CapacityReservationReport report, IWorkflow workflow, BusinessObjectFactory factory)
		{
			var relevantTasks = TaskInclusionHelper.GetTasksForCapacityCheck(workflow).ToArray();
			var directlyAssignedTasks = relevantTasks.Where(t => !t.P9_GS_NKAssignedStaffMember.IsEmpty).GroupBy(t => GetStaff(t, Buffer.Factory)).ToArray();
			var assignedStaffCodesWithNoStaffRecord = directlyAssignedTasks.Where(t => t.Key == null).SelectMany(grp => grp.Select(t => t.P9_GS_NKAssignedStaffMember.ToString())).ToArray();

			if (assignedStaffCodesWithNoStaffRecord.Any())
			{
				report.BufferReleaseOutcome = BufferReleaseOutcome.BlockedByMissingResource;
				report.MissingResourceCodes.AddRange(assignedStaffCodesWithNoStaffRecord);

				return;
			}

			if (!BMSRegistry.Instance.DisableCapacityCalculations.Value)
			{
				var unclaimedCapabilityTasks = relevantTasks.Where(t => t.RequiresResourceWithCapability()).GroupBy(t => t.GetRequiredCapability(factory)).Where(x => x.Key != null).ToArray();
				var resourcesByCapability = GetResourcesByCapability(unclaimedCapabilityTasks, workflow);
				var allResourcesOfWhomCapacityIsRequired = resourcesByCapability.SelectMany(kvp => kvp.Value.Select(c => c.Staff)).Concat(directlyAssignedTasks.Select(x => x.Key)).WhereNotNull().Distinct().ToArray();

				EnsureRequiredCapacityIsCached(allResourcesOfWhomCapacityIsRequired);

				var lines = CreateRequiredLines(report, allResourcesOfWhomCapacityIsRequired);

				FillWithCapacityRequiredForDirectlyAssignedTasks(lines, directlyAssignedTasks);
				ReserveCapacityForDirectlyAssignedTasks(report);

				FillWithCapacityRequiredForUnclaimedCapabilityTasks(report, lines, workflow, unclaimedCapabilityTasks, resourcesByCapability);
			}

			CheckCCPMSchedule(report);
			CheckResourceLeave(report);
			CheckValidTasks(report);

			if (report.BufferReleaseOutcome == BufferReleaseOutcome.Unknown)
			{
				report.BufferReleaseOutcome = BufferReleaseOutcome.Releasable;
			}
		}

		Dictionary<GlbStaff, CapacityReservationReportLine> CreateRequiredLines(CapacityReservationReport report, IEnumerable<GlbStaff> allResourcesOfWhomCapacityIsRequired)
		{
			var lines = new Dictionary<GlbStaff, CapacityReservationReportLine>();

			foreach (var resource in allResourcesOfWhomCapacityIsRequired)
			{
				var capacity = TrackedAvailableCapacityByResource[resource.GS_Code];
				var placeInQueue = GetNextPlaceInQueue(resource);
				var line = new CapacityReservationReportLine(report, resource, capacity, placeInQueue);

				lines.Add(resource, line);
				report.ResourceCapacityReportLines.Add(line);

				SetResourceLeaveDetails(line);
			}

			return lines;
		}

		#endregion

		#region Determining capacity requirements

		void FillWithCapacityRequiredForDirectlyAssignedTasks(Dictionary<GlbStaff, CapacityReservationReportLine> lines, IEnumerable<IGrouping<GlbStaff, IWorkflowTask>> directlyAssignedTasks)
		{
			foreach (var group in directlyAssignedTasks)
			{
				var line = lines[group.Key];

				foreach (var task in group)
				{
					var standardEstimateHours = GetEstimateHoursConsumedIfReleased(task, line.Report);

					line.CapacityHoursRequiredForDirectlyAssignedTasks += standardEstimateHours;
					line.DirectlyAssignedTasks.Add(new TaskLogDetails(task.P9_TaskID, standardEstimateHours, requiredCapabilityName: null));
				}
			}
		}

		void FillWithCapacityRequiredForUnclaimedCapabilityTasks(
			CapacityReservationReport report,
			Dictionary<GlbStaff, CapacityReservationReportLine> lines,
			IWorkflow workflow,
			IEnumerable<IGrouping<GlbCapability, IWorkflowTask>> unclaimedCapabilityTasks,
			DictionaryOfLists<GlbCapability, StaffWithReleaseGroup> resourcesByCapabilityAndReleaseGroup)
		{
			foreach (var tasksByCapability in unclaimedCapabilityTasks)
			{
				var capability = tasksByCapability.Key;
				var tasks = tasksByCapability.Select(task => task).ToArray();

				if (capability.G4_CapacityScope == GlbCapabilityScopeList.Codes.GlobalScope)
				{
					var resources = resourcesByCapabilityAndReleaseGroup[capability]
						.Select(capabilityPerResource => capabilityPerResource.Staff)
						.Distinct()
						.ToArray();

					FillWithCapacityRequiredForUnclaimedCapabilityTasks(report, lines, capability, tasks, resources);
				}
				else if (capability.G4_CapacityScope == GlbCapabilityScopeList.Codes.GroupScope)
				{
					var resourcesWithCapability = resourcesByCapabilityAndReleaseGroup[capability];
					var tasksByReleaseGroup = resourcesWithCapability
						.GroupBy(resource => resource.ReleaseGroupPK, resource => tasksByCapability
							.Where(task => (task.P9_GG_AssignedGroup.IsEmpty && workflow.ReleaseGroupPK == resource.ReleaseGroupPK) || task.P9_GG_AssignedGroup == resource.ReleaseGroupPK))
							.ToDictionary(t => t.Key, t => t.SelectMany(task => task).Distinct());

					foreach (var taskByReleaseGroup in tasksByReleaseGroup)
					{
						var releaseGroup = taskByReleaseGroup.Key;
						var taskInReleaseGroup = taskByReleaseGroup.Value.ToArray();
						var resourcesWithCapabilityAndReleaseGroup = resourcesWithCapability
							.Where(r => r.ReleaseGroupPK == releaseGroup)
							.Select(r => r.Staff)
							.Distinct()
							.ToArray();

						FillWithCapacityRequiredForUnclaimedCapabilityTasks(report, lines, capability, taskInReleaseGroup, resourcesWithCapabilityAndReleaseGroup);
					}
				}
			}
		}

		void FillWithCapacityRequiredForUnclaimedCapabilityTasks(
			CapacityReservationReport report,
			Dictionary<GlbStaff, CapacityReservationReportLine> lines,
			GlbCapability capability,
			IWorkflowTask[] tasks,
			GlbStaff[] resourcesWithCapability)
		{
			var taskRelevantEstimates = tasks.ToDictionary(task => task, task => GetEstimateHoursConsumedIfReleased(task, report));
			var capacityHoursRequiredOfResourcesWithCapability = taskRelevantEstimates.Sum(kvp => kvp.Value);
			var availableCapacityAcrossStaffWithCapability = GetAvailableCapacityForResourcesWithCapability(resourcesWithCapability, report);
			var sufficientCapacityExistsForCapabilityTasks = availableCapacityAcrossStaffWithCapability.EffectiveAvailableCapacityConsideringAllowedOverload >= capacityHoursRequiredOfResourcesWithCapability;

			if (!sufficientCapacityExistsForCapabilityTasks)
			{
				report.BufferReleaseOutcome = BufferReleaseOutcome.BlockedByResourceCapacity;
			}

			if (resourcesWithCapability.Any())
			{
				var capacityToReserveAgainstEachStaff = capacityHoursRequiredOfResourcesWithCapability / resourcesWithCapability.Length;

				foreach (var resource in resourcesWithCapability)
				{
					var previousAvailableCapacity = report.GetRelevantAvailableCapacity(resource, TrackedAvailableCapacityByResource[resource.GS_Code]);

					ReserveCapacity(capacityToReserveAgainstEachStaff, resource);

					var line = lines[resource];

					line.CapacityHoursRequiredForCapabilityTasks += capacityToReserveAgainstEachStaff;

					if (!sufficientCapacityExistsForCapabilityTasks && previousAvailableCapacity.EffectiveAvailableCapacityConsideringAllowedOverload < capacityToReserveAgainstEachStaff)
					{
						line.ThisResourceBlocksWorkflowRelease = true;
					}

					AddCapabilityTaskLogDetails(line.CapabilityTasks);
				}
			}
			else
			{
				var line = new MissingCapabilityCapacityReservationReportLine(capability, capacityHoursRequiredOfResourcesWithCapability);

				AddCapabilityTaskLogDetails(line.CapabilityTasks);
				report.CapabilityTasksWithNoResourcesReportLines.Add(line);
			}

			void AddCapabilityTaskLogDetails(ICollection<TaskLogDetails> taskLogDetailsList)
			{
				var estimateDivisor = resourcesWithCapability.Any() ? resourcesWithCapability.Length : 1;

				foreach (var task in tasks)
				{
					taskLogDetailsList.Add(new TaskLogDetails(task.P9_TaskID, taskRelevantEstimates[task] / estimateDivisor, capability.G4_Description));
				}
			}
		}

		AvailableCapacity GetAvailableCapacityForResourcesWithCapability(IEnumerable<GlbStaff> resourcesWithCapability, CapacityReservationReport report)
		{
			var totalAvailableCapacityAcrossCapability = 0m;
			var effectiveAvailableCapacityConsideringAllowedOverload = 0m;

			foreach (var resource in resourcesWithCapability)
			{
				var capacity = TrackedAvailableCapacityByResource[resource.GS_Code];
				var availableCapacity = report.GetRelevantAvailableCapacity(resource, capacity);

				if (availableCapacity.RealAvailableCapacity < 0)
				{
					// We don't want someone with negative one billion hours capacity blocking release of work when there are other resources with capacity.
					// This puts a limit to how negative we allow each resource to go with their impact on available capacity across a capability.

					var negativeCapacityFloor = -1 * BMSRegistry.Instance.MaximumCapabilityTaskOverloadLimit.Value * capacity.FullCapacity;
					var adjustedRealCapacity = Math.Max(negativeCapacityFloor, availableCapacity.RealAvailableCapacity);
					var adjustedCapacityConsideringAllowedOverload = Math.Max(negativeCapacityFloor, availableCapacity.EffectiveAvailableCapacityConsideringAllowedOverload);

					availableCapacity = new AvailableCapacity(adjustedRealCapacity, adjustedCapacityConsideringAllowedOverload);
				}

				totalAvailableCapacityAcrossCapability += availableCapacity.RealAvailableCapacity;
				effectiveAvailableCapacityConsideringAllowedOverload += availableCapacity.EffectiveAvailableCapacityConsideringAllowedOverload;
			}

			return new AvailableCapacity(totalAvailableCapacityAcrossCapability, effectiveAvailableCapacityConsideringAllowedOverload);
		}

		#endregion

		#region Other Release Outcomes

		void CheckCCPMSchedule(CapacityReservationReport report)
		{
			if (ApprovedCcpmReleaseCache.IsCCPMReadyToRelease(report.Buffer.Factory, report.WorkflowBeingConsideredForRelease.PK))
			{
				report.BufferReleaseOutcome = BufferReleaseOutcome.ReleasableByCCPMSchedule;

				if (report.WorkflowBeingConsideredForRelease is ICCPMSchedulable schedulable)
				{
					schedulable.IsCcpmScheduleReleasable = true;
				}
			}
		}

		void CheckResourceLeave(CapacityReservationReport report)
		{
			if (report.BufferReleaseOutcome == BufferReleaseOutcome.Unknown && report.ResourceCapacityReportLines.Any(l => l.ThisResourcesLeaveBlocksWorkflowRelease))
			{
				report.BufferReleaseOutcome = BufferReleaseOutcome.BlockedByResourceLeave;
			}
		}

		void CheckValidTasks(CapacityReservationReport report)
		{
			if (report.ResourceCapacityReportLines.Count == 0 && report.CapabilityTasksWithNoResourcesReportLines.Count == 0)
			{
				var incompleteIgnoredTasksThatWillLikeyBeShownInAChannel = TaskInclusionHelper.GetIncompleteIgnoredTasks(report.WorkflowBeingConsideredForRelease)
					.Where(t => !t.P9_GS_NKAssignedStaffMember.IsEmpty || !t.P9_G4_RequiredCapability.IsEmpty);

				if (!incompleteIgnoredTasksThatWillLikeyBeShownInAChannel.Any()) // We allow release of work that will likely be shown in channels somewhere, but doesn't require any capacity.
				{
					report.BufferReleaseOutcome = BufferReleaseOutcome.BlockedByWorkflowEmptiness;
				}
			}
		}

		#endregion

		#region Capacity reservation

		void ReserveCapacityForDirectlyAssignedTasks(CapacityReservationReport report)
		{
			foreach (var line in report.ResourceCapacityReportLines)
			{
				if (line.CapacityHoursRequiredForDirectlyAssignedTasks > 0)
				{
					ReserveCapacity(line.CapacityHoursRequiredForDirectlyAssignedTasks, line.Resource);

					line.ThisResourceBlocksWorkflowRelease = line.CapacityHoursRequiredForDirectlyAssignedTasks > line.AvailableCapacityAtThisPointInQueueIncludingAllowedOverload_ForEvaluatingCapacity;

					if (line.ThisResourceBlocksWorkflowRelease)
					{
						report.BufferReleaseOutcome = BufferReleaseOutcome.BlockedByResourceCapacity;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		IResourceCapacity ReserveCapacity(decimal capacityHoursToReserve, GlbStaff resource)
		{
			var key = resource.GS_Code;
			var capacity = TrackedAvailableCapacityByResource[key];
			var newCapacity = capacity.Deduct(capacityHoursToReserve);

			return TrackedAvailableCapacityByResource[key] = newCapacity;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		decimal GetEstimateHoursConsumedIfReleased(IWorkflowTask task, CapacityReservationReport report)
		{
			return (decimal)task.GetRelevantEstimateHours() * report.ZoneMultiplier;
		}

		#endregion

		#region Helpers

		GlbStaff GetStaff(IWorkflowTask task, BusinessObjectFactory factory = null)
		{
			if (task.P9_GS_NKAssignedStaffMember.IsEmpty)
			{
				return null;
			}

			if (AllStaff.TryGetValue(task.P9_GS_NKAssignedStaffMember, out var staff))
			{
				return staff;
			}

			staff = factory == null ? task.GetAssignedStaffMember() as GlbStaff : task.GetAssignedStaffMember(factory);

			if (staff == null)
			{
				return null;
			}

			AllStaff.GetOrAdd(staff.GS_Code, () => staff);

			return staff;
		}

		bool ShouldUseConstrainedModeCapacityCalculationForNonCCRs(IWorkflow workflow)
		{
			var tasks = workflow.Tasks;
			AddResourcesWithCapabilitiesFetchHints(Buffer.Factory, tasks);

			return tasks.Any(t => ConstrainedModeHelper.IsCCRTask(t, workflow, Buffer, cCRsMustBeWithinConstrainedReleaseGroup: true)); // All tasks, not just those configured to be relevant for capacity consideration, are relevant when deciding whether constrained mode capacity calculation is required.
		}

		void AddResourcesWithCapabilitiesFetchHints(BusinessObjectFactory factory, IEnumerable<IWorkflowTask> tasks)
		{
			var requiredCapabilities = tasks.Where(t => GetStaff(t) == null).Select(t => t.GetRequiredCapability(factory)).Where(c => c != null);
			ZQuery resourceWithCapabilityQuery = new ZQuery(GlbResourceCapabilityPivotSchema.G5_G4_Capability, requiredCapabilities.Select(c => c.PK));
			factory.AddFetchHint(GlbResourceCapabilityPivotSchema.Instance, resourceWithCapabilityQuery);
		}

		void EnsureRequiredCapacityIsCached(IEnumerable<GlbStaff> allResourcesOfWhomCapacityIsRequired)
		{
			var nonCachedStaff = allResourcesOfWhomCapacityIsRequired.Where(s => !TrackedAvailableCapacityByResource.ContainsKey(s.GS_Code));

			if (nonCachedStaff.Any())
			{
				var breakdowns = CapacityCalculator.GetUtilisedCapacityBreakdown(nonCachedStaff, Buffer);

				foreach (var kvp in breakdowns)
				{
					TrackedAvailableCapacityByResource.Add(kvp.Key.GS_Code, kvp.Value);
				}
			}
		}

		int? GetNextPlaceInQueue(GlbStaff resource)
		{
			if (PlaceInQueueByResource == null)
			{
				return null;
			}

			var key = resource.GS_Code;

			if (!PlaceInQueueByResource.ContainsKey(key))
			{
				PlaceInQueueByResource.Add(key, 0);
			}

			return ++PlaceInQueueByResource[key];
		}

		DictionaryOfLists<GlbCapability, StaffWithReleaseGroup> GetResourcesByCapability(IEnumerable<IGrouping<GlbCapability, IWorkflowTask>> unclaimedCapabilityTasks, IWorkflow workflow)
		{
			var result = new DictionaryOfLists<GlbCapability, StaffWithReleaseGroup>();

			foreach (var group in unclaimedCapabilityTasks)
			{
				var capability = group.Key;
				var tasksGroupedByReleaseGroup = group.GroupBy(x => x.P9_GG_AssignedGroup).ToArray();
				foreach (var tasks in tasksGroupedByReleaseGroup)
				{
					var releaseGroup = GetReleaseGroup(tasks, workflow);
					var resources = ResourcesWithCapabilityCache.GetWorkingResourcesWithCapability(capability.PK, releaseGroup).ToArray();

					result.AddValues(capability, resources.Select(x => new StaffWithReleaseGroup(x, releaseGroup)).ToArray());
				}
			}

			return result;
		}

		ZGuid GetReleaseGroup(IGrouping<ZGuid, IWorkflowTask> tasksByReleaseGroup, IWorkflow workflow)
		{
			if (tasksByReleaseGroup.Key.IsEmpty)
			{
				return workflow.ReleaseGroupPK == default
					? ZGuid.Empty
					: new ZGuid(workflow.ReleaseGroupPK);
			}

			return tasksByReleaseGroup.Key;
		}

		decimal GetZoneMultiplier(ZGuid releaseGroup, BusinessObjectFactory factory)
		{
			var disableZoneMultipliers = ExperimentalSettingsProvider.ZoneMultipliersDisabled(Buffer);

			if (disableZoneMultipliers)
			{
				return BMConstants.Zone3DefaultMultiplier;
			}

			return ZoneMultiplierCache.GetOrAdd(releaseGroup, () => BMZoneCapacityMultiplier.GetZoneMultiplier(factory, Buffer.PK, releaseGroup, zoneId: 3));
		}

		/// <summary>
		/// LOGIC:
		/// ======
		/// given time T1 (converted to resource's time)
		///	if T1 falls on resource's holiday and window is not 100%, then
		///
		/// STEP1:
		/// ------
		/// Find the end time for this holiday and all subsequent holidays, i.e. without a break in between: T2 (in resource's timezone)
		///  
		/// STEP2:
		/// ------
		/// Convert T2 to buffer timezone: T3 (in buffer timezone)
		///  
		/// STEP3:
		/// ------
		/// IF window% != 0 
		/// 	Find out going backwards from T3 the last normal working time for the buffer: T4 (in buffer timezone)
		/// 	Subtract window% from T4: T5 is the earliest release time (in buffer timezone)
		/// 
		/// OTHERWISE 
		/// 	T3 is the earliest release time
		/// </summary>
		void SetResourceLeaveDetails(CapacityReservationReportLine line)
		{
			var resource = line.Resource;
			line.EarliestTimeWorkCanBeReleasedToResource = ZDateTime.Empty;
			line.ResourceReturningFromLeaveTime = ZDateTime.Empty;

			if (!resource.IsOnLeave || BMSRegistry.Instance.ResourceLeaveWindowforReleasingWork.Value >= 100)
			{
				line.ThisResourcesLeaveBlocksWorkflowRelease = false;
				return;
			}

			var timeNowUTC = ZDateTime.UtcNow;
			var localResourceTime = resource.HomeBranch != null ?
					timeNowUTC.ToLocationTime(resource.HomeBranch.HomePort).ToZDateTime() :
					timeNowUTC.ToLocationTime(Buffer.AgingBranch.HomePort).ToZDateTime();

			var firstAvailableDateTimeResourceTimezone = resource.GetFirstDateTimeNotOnHoliday(localResourceTime);
			if (firstAvailableDateTimeResourceTimezone.IsEmpty)
			{
				line.ThisResourcesLeaveBlocksWorkflowRelease = true;
				return;
			}

			var firstAvailableDateTimeUTC = resource.HomeBranch != null ?
					firstAvailableDateTimeResourceTimezone.ToUniversalBranchTime(resource.HomeBranch).ToZDateTime() :
					firstAvailableDateTimeResourceTimezone.ToUniversalBranchTime(Buffer.AgingBranch).ToZDateTime();
			var firstAvailableDateTimeBufferTimezone = firstAvailableDateTimeUTC.ToLocationTime(Buffer.AgingBranch.HomePort).ToZDateTime();

			line.ResourceReturningFromLeaveTime = line.EarliestTimeWorkCanBeReleasedToResource = firstAvailableDateTimeUTC;

			var leaveWindowHours = BMSRegistry.Instance.ResourceLeaveWindowforReleasingWork.Value / 100.0 * Buffer.BufferTimeSpanHours; // A percentage of the buffer time period.
			if (leaveWindowHours != 0)
			{
				var context = WorkingTimeContext.Create(Buffer);
				var workingTimeCalculator = context.GetWorkTimeArithmetic(Buffer.Factory, checkHomeBranchAndDepartment: true);
				var earliestReleaseTime = workingTimeCalculator.GetDateTimeInWorkingHoursFutureOrPast(firstAvailableDateTimeBufferTimezone.ToDateTime(), -leaveWindowHours);
				line.EarliestTimeWorkCanBeReleasedToResource = new ZDateTime(earliestReleaseTime).ToUniversalBranchTime(Buffer.AgingBranch);
			}

			line.ThisResourcesLeaveBlocksWorkflowRelease = line.EarliestTimeWorkCanBeReleasedToResource > timeNowUTC;
		}

		#endregion

		#region State

		internal Dictionary<ZString, IResourceCapacity> TrackedAvailableCapacityByResource { get; }
		Dictionary<string, int> PlaceInQueueByResource { get; }

		Dictionary<ZGuid, decimal> ZoneMultiplierCache { get; } = new Dictionary<ZGuid, decimal>();

		BMComponent Buffer { get; }

		TaskInclusionHelper TaskInclusionHelper { get; } = new TaskInclusionHelper();
		WorkingResourcesWithCapabilityCache ResourcesWithCapabilityCache { get; }

		Dictionary<ZString, GlbStaff> AllStaff { get; } = new Dictionary<ZString, GlbStaff>();
		#endregion

		struct StaffWithReleaseGroup
		{
			public StaffWithReleaseGroup(GlbStaff staff, ZGuid releaseGroupPK)
			{
				Staff = staff;
				ReleaseGroupPK = releaseGroupPK;
			}

			public GlbStaff Staff { get; }
			public ZGuid ReleaseGroupPK { get; }
		}
	}
}

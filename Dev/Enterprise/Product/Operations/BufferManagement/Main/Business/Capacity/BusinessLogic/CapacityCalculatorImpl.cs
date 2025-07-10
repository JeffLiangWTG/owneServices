using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	static class CapacityCalculatorImpl
	{
		#region Resource Capacity

		#region Full Capacity

		internal static IResourceCapacity GetFullCapacity(GlbStaff resource, BMComponent buffer, WorkingTimeContext workingTimeContext, bool considerZoneMultipliers)
		{
			if (resource == null)
			{
				return ResourceCapacity.Zero(considerZoneMultipliers);
			}

			if (!resource.GS_IsActive)
			{
				return ResourceCapacity.CreateForResource(resource.GS_Code, considerZoneMultipliers);
			}

			var loadFactor = ConstrainedModeHelper.GetFractionOfBufferSizeForResourceFullCapacity(resource.GS_Code, buffer);
			var ccrOverloadMultiplier = ConstrainedModeHelper.GetFullCapacityMultiplierConsideringCapacityConstrainedResources(resource.GS_Code, buffer);

			var totalHours = GetTotalResourceHoursInBuffer(resource, buffer, workingTimeContext);
			var fullCapacity = new ZDecimal(totalHours * loadFactor).Round(CapacityCalculator.DecimalPlaces);

			return ResourceCapacity.CreateForResourceWithFullCapacity(resource.GS_Code, fullCapacity, considerZoneMultipliers).WithNonCCROverloadMultiplier(ccrOverloadMultiplier);
		}

		#endregion

		#region Utilised Capacity

		internal static Dictionary<string, IResourceCapacity> GetUtilisedCapacityBreakdowns(GlbStaff[] staffs, BMComponent component, bool considerZoneMultipliers = true, ILogger logger = null)
		{
			WorkflowDTO[] contents = null;

			contents = GetComponentContents(component, staffs, logger).ToArray();
			AddFetchHints(contents, staffs, component);

			if (component.IsBuffer)
			{
				AddBufferFetchHints(component);

				var bufferWorkTimeContext = WorkingTimeContext.Create(component);

				return staffs.ToDictionary(resource => resource.GS_Code.ToString(), resource =>
				{
					var resourceWorkingHoursContext = WorkingTimeContext.Create(component, resource); // WorkingTimeContext associated with the resource and the buffer's (!) aging branch and department
					var originalCapacity = GetFullCapacity(resource, component, resourceWorkingHoursContext, considerZoneMultipliers);
					var relevantTasks = GetRelevantTasks(contents, resource, resource.Capabilities, component.Factory);

					return GetUtilisedCapacityBreakdownForBuffer(originalCapacity, relevantTasks, component, bufferWorkTimeContext, considerZoneMultipliers);
				});
			}
			else if (component.IsBucket)
			{
				return staffs.ToDictionary(resource => resource.GS_Code.ToString(), resource =>
				{
					var originalCapacity = ResourceCapacity.CreateForResource(resource.GS_Code, considerZoneMultipliers);
					var relevantTasks = GetRelevantTasks(contents, resource, resource.Capabilities, component.Factory);

					return GetUtilisedCapacityBreakdownForBucket(originalCapacity, relevantTasks);
				});
			}

			return new Dictionary<string, IResourceCapacity>();
		}

		static IResourceCapacity GetUtilisedCapacityBreakdownForBuffer(IResourceCapacity originalCapacity, IEnumerable<TaskDTO> relevantTasks, BMComponent component, WorkingTimeContext bufferWorkTimeContext, bool considerZoneMultipliers = true)
		{
			if (considerZoneMultipliers)
			{
				var capacityPair = relevantTasks.Aggregate(
					new ReservedAllocatedCapacityPair(new Dictionary<int, decimal>(), new Dictionary<int, decimal>()),
					(accumulated, task) =>
					{
						return bufferWorkTimeContext.ZoneMultiplierProvider.AccumulateCapacities(accumulated, task.Workflow, component, bufferWorkTimeContext, task);
					});

				return originalCapacity.WithZoneCapacities(capacityPair.AllocatedCapacity, capacityPair.ReservedCapcity).WithRoundedBreakdown(CapacityCalculator.DecimalPlaces);
			}
			else
			{
				var utilisedCapacity = relevantTasks.Aggregate(0m, (accumulated, task) => accumulated + task.EstimateHours);
				return originalCapacity.Deduct(utilisedCapacity);
			}
		}

		static IResourceCapacity GetUtilisedCapacityBreakdownForBucket(IResourceCapacity originalCapacity, IEnumerable<TaskDTO> relevantTasks)
		{
			var utilisedCapacity = relevantTasks.Aggregate(0m, (accumulated, task) => accumulated + task.EstimateHours);
			return originalCapacity.Deduct(utilisedCapacity);
		}

		static TaskDTO[] GetRelevantTasks(WorkflowDTO[] workflows, GlbStaff staff, BusinessObjectCollection resourceCapabilities, BusinessObjectFactory factory)
		{
			var capabilityTasks = GetCapabilityTasks(workflows, resourceCapabilities);

			var assignedTasks = workflows
				.SelectMany(w => w.Tasks)
				.Where(task => task.StaffCode == staff.GS_Code);
			var globalCapabilityTasks = capabilityTasks
				.Where(capabilityWorkflowTask => capabilityWorkflowTask.Capability.G4_CapacityScope == GlbCapabilityScopeList.Codes.GlobalScope)
				.Select(capabilityWorkflowTask => capabilityWorkflowTask.Task);
			var groupCapabilityTasks = capabilityTasks
				.Where(capabilityWorkflowTask => capabilityWorkflowTask.Capability.G4_CapacityScope == GlbCapabilityScopeList.Codes.GroupScope)
				.ToArray();

			if (groupCapabilityTasks.Any())
			{
				var relevantGroupCapabilityTasks = GetRelevantGroupCapabilityTasks(groupCapabilityTasks, staff, factory);
				return assignedTasks.Union(globalCapabilityTasks).Union(relevantGroupCapabilityTasks).ToArray();
			}

			return assignedTasks.Union(globalCapabilityTasks).ToArray();
		}

		static List<TaskWithReleaseGroupAndCapability> GetCapabilityTasks(IEnumerable<WorkflowDTO> workflows, BusinessObjectCollection resourceCapabilities)
		{
			return workflows
				.SelectMany(workflow => workflow.Tasks
					.Where(task => string.IsNullOrEmpty(task.StaffCode) && task.RequiredCapability != Guid.Empty && resourceCapabilities.Contains(task.RequiredCapability))
					.Select(task => new TaskWithReleaseGroupAndCapability(workflow.ReleaseGroup, task, resourceCapabilities.FindByPK(task.RequiredCapability) as GlbCapability)))
				.ToList();
		}

		struct TaskWithReleaseGroupAndCapability
		{
			public TaskWithReleaseGroupAndCapability(Guid workflowReleaseGroupPK, TaskDTO task, GlbCapability capability)
			{
				Task = task;
				Capability = capability;
				TaskGroupPK = task.Group;
				WorkflowReleaseGroupPK = workflowReleaseGroupPK;
			}

			public TaskDTO Task { get; }
			public GlbCapability Capability { get; }
			public Guid TaskGroupPK { get; }
			public Guid WorkflowReleaseGroupPK { get; }
			public bool NoGroup => TaskGroupPK == Guid.Empty && WorkflowReleaseGroupPK == Guid.Empty;
		}

		static IEnumerable<TaskDTO> GetRelevantGroupCapabilityTasks(TaskWithReleaseGroupAndCapability[] groupCapabilityTasks, GlbStaff staff, BusinessObjectFactory factory)
		{
			var releaseGroupsPKs = groupCapabilityTasks
				.Select(groupCapabilityTask => groupCapabilityTask.WorkflowReleaseGroupPK)
				.Union(groupCapabilityTasks.Select(x => x.Task.Group))
				.Distinct();

			var releaseGroupLinks = GetReleaseGroupLinks(releaseGroupsPKs, staff.PK, factory);
			var relevantGroupCapabilityTasks = groupCapabilityTasks
				.Where(groupCapabilityTask => groupCapabilityTask.NoGroup || releaseGroupLinks.Any(releaseGroup => releaseGroup.GK_GS == staff.PK
						&& (
								(releaseGroup.GK_GG == groupCapabilityTask.WorkflowReleaseGroupPK && groupCapabilityTask.TaskGroupPK == Guid.Empty)
								||
								(releaseGroup.GK_GG == groupCapabilityTask.TaskGroupPK)
							)
						)
					)
				.Select(groupCapabilityTask => groupCapabilityTask.Task);

			return relevantGroupCapabilityTasks;
		}

		static IEnumerable<GlbGroupLink> GetReleaseGroupLinks(IEnumerable<Guid> releaseGroupsPKs, ZGuid staffPK, BusinessObjectFactory factory)
		{
			var releaseGroupLinksQuery = new ZQuery(GlbGroupLinkSchema.GK_GG, releaseGroupsPKs.Where(pk => pk != Guid.Empty));

			releaseGroupLinksQuery.AddToFilter(GlbGroupLinkSchema.GK_GS, staffPK);

			return factory.Load<GlbGroupLink>(releaseGroupLinksQuery);
		}

		static void AddFetchHints(IEnumerable<WorkflowDTO> workflowDTOs, IEnumerable<GlbStaff> staffs, BMComponent component)
		{
			var factory = component.Factory;
			var tasks = workflowDTOs.SelectMany(w => w.Tasks);

			foreach (var shapePK in tasks.Select(t => t.Workflow.ShapeForZoneCalculation).Where(pk => pk.IsValid).Distinct())
			{
				var query = new ZQuery(BMNCNShapeSchema.PK, shapePK);
				query.IncludeBlob(BMNCNShapeSchema.BNS_LayoutData);
				factory.AddFetchHint(BMNCNShapeSchema.Instance, query);
			}

			foreach (var workflowPK in tasks.Select(t => t.Workflow.ParentWorkflowForZoneCalculation).Where(pk => pk.IsValid).Distinct())
			{
				factory.AddFetchHint(ProcessHeaderSchema.PK, workflowPK);
			}

			var releaseGroupsOfWorkflowsWithCapabilityTasks = workflowDTOs
				.Where(workflow => workflow.ReleaseGroup != Guid.Empty && workflow.Tasks.Any(task => string.IsNullOrEmpty(task.StaffCode) && task.RequiredCapability != Guid.Empty))
				.Select(workflow => workflow.ReleaseGroup)
				.Distinct()
				.ToArray();

			foreach (var staff in staffs)
			{
				factory.AddFetchHint(GlbStaffHolidaySchema.GA_GS, staff.PK);
				factory.AddFetchHint(GlbResourceCapabilityPivotSchema.G5_GS_Resource, staff.PK);
				var workTimeQuery = new ZQuery(GlbWorkTimeSchema.GW_ParentID, staff.PK);
				workTimeQuery.AddToFilter(GlbWorkTimeSchema.GW_ParentTableCode, GlbStaffSchema.Constants.Prefix);
				factory.AddFetchHint(GlbWorkTimeSchema.Instance, workTimeQuery);

				var glbGroupLinkQuery = new ZQuery(GlbGroupLinkSchema.GK_GG, releaseGroupsOfWorkflowsWithCapabilityTasks);
				glbGroupLinkQuery.AddToFilter(GlbGroupLinkSchema.GK_GS, staff.PK);
				factory.AddFetchHint(GlbGroupLinkSchema.Instance, glbGroupLinkQuery);

				var componentResourceLinkQuery = new ZQuery(BMComponentResourceLinkSchema.FD_FC_Component, component.PK);
				componentResourceLinkQuery.AddToFilter(BMComponentResourceLinkSchema.FD_GS_NKResource, staff.GS_Code);
				factory.AddFetchHint(BMComponentResourceLinkSchema.Instance, componentResourceLinkQuery);
				factory.AddFetchHint(BMComponentResourceLinkSchema.FD_GS_NKResource, staff.GS_Code);
			}
		}

		static void AddBufferFetchHints(BMComponent buffer)
		{
			buffer.Factory.AddFetchHint(BMZoneCapacityMultiplierSchema.Instance, new ZQuery(BMZoneCapacityMultiplierSchema.BZC_FC_Component, buffer.PK));
		}

		internal static decimal GetUtilisedCapacity(GlbStaff resource, BMComponent component, WorkingTimeContext workingTimeContext)
		{
			var contents = GetComponentContents(component, new[] { resource });
			var result = contents.SelectMany(x => x.Tasks).Sum(task => workingTimeContext.ZoneMultiplierProvider.GetZoneMultipliedEstimate(task.Workflow, component, workingTimeContext, task));

			return new ZDecimal(result).Round(CapacityCalculator.DecimalPlaces);
		}

		#endregion

		#region Resource Membership Factor

		internal static decimal GetComponentResourceMembershipFactor(GlbStaff resource, BMComponent component)
		{
			var componentMembership = resource.ComponentMembership.Cast<BMComponentResourceLink>().FirstOrDefault(l => l.FD_FC_Component == component.PK);
			return componentMembership != null ? componentMembership.FD_CapacityLimitPercent / 100.0m : 1;
		}

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		static decimal GetTotalResourceHoursInBuffer(GlbStaff resource, BMComponent buffer, WorkingTimeContext workingTimeContext)
		{
			if (!buffer.IsBuffer)
			{
				throw new ArgumentException("Can only get capacity for a buffer");
			}

			var factory = resource.Factory;
			var bufferWorkTimeArithmetic = WorkingDays.GetInstance(factory, workingTimeContext.Department.PK, workingTimeContext.Branch.PK);
			var resourceWorkTimeArithmetic = workingTimeContext.GetWorkTimeArithmetic(factory, checkHomeBranchAndDepartment: true);

			var bufferBranchLocalTime = workingTimeContext.GetCurrentLocalTime(factory).ToDateTime();
			var bufferEndTime = bufferWorkTimeArithmetic.GetDateTimeInWorkingHoursFutureOrPast(bufferBranchLocalTime, buffer.BufferTimeSpanHours);
			var bufferWindowSize = bufferEndTime - bufferBranchLocalTime;

			var homeBranchLocalTime = workingTimeContext.GetCurrentLocalTime(factory, checkHomeBranchAndDepartment: true).ToDateTime();
			var homeBranchBufferEndTime = homeBranchLocalTime + bufferWindowSize;
			var totalWorkingTime = resourceWorkTimeArithmetic.TimeDifference(homeBranchLocalTime, homeBranchBufferEndTime);

			if (totalWorkingTime.TotalMinutes > buffer.FC_BufferTimespanInMinutes)
			{
				totalWorkingTime = TimeSpan.FromMinutes(buffer.FC_BufferTimespanInMinutes);
			}

			var membershipFactor = GetComponentResourceMembershipFactor(resource, buffer);

			return ((decimal)Math.Truncate(totalWorkingTime.TotalMinutes)) / 60 * membershipFactor;
		}

		#endregion

		#endregion

		#region Component Contents

		internal static IEnumerable<WorkflowDTO> GetComponentContents(BMComponent component, IEnumerable<GlbStaff> resources, ILogger logger = null)
		{
			if (BMSRegistry.Instance.DisableCapacityCalculations.Value)
			{
				ErrorReporter.ReportOnce("Capacity was calculated even though the DisableCapacityCalculations registry item was enabled.");
			}

			IEnumerable<WorkflowDTO> result;
			var startingTime = ZDateTime.UtcNow;

			if (ExperimentalSettingsProvider.SimpleCapacityQueryEnabled(component))
			{
				logger?.Log(LogType.Information, (NoResString)"Using Capacity Query...");
				result = CapacitySimpleQueryContentProvider.Load(component, resources, logger);
			}
			else
			{
				logger?.Log(LogType.Information, (NoResString)"Using Old Capacity Query...");
				result = CapacityContentProvider.Load(component, resources, logger);
			}

			var finishedTime = ZDateTime.UtcNow;
			logger?.Log(LogType.Information, $"Finished loading workflows in {finishedTime - startingTime}");

			return result;
		}

		#endregion
	}
}

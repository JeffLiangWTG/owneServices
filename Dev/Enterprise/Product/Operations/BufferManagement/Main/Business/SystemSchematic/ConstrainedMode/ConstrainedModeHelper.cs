using System;
using System.Collections.Generic;
using System.Globalization;
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
	public static class ConstrainedModeHelper
	{
		#region IsInConstrainedMode

		public static bool IsInConstrainedMode(BMComponent component, ZGuid releaseGroupPK)
		{
			if (component.ChildComponents.Any(c => c.IsConstraint))
			{
				var factory = component.Factory;
				var systemPk = component.FC_FS_System;

				if (!BMSystem.DoesSystemHaveReleaseGroups(factory, systemPk))
				{
					return true;
				}

				var query = new ZQuery(BMComponentReleaseGroupLinkSchema.FO_GG_ReleaseGroup, releaseGroupPK);
				query.AddToFilter(BMComponentReleaseGroupLinkSchema.FO_FC_Component, component.PK);

				var link = factory.LoadTop1<BMComponentReleaseGroupLink>(query);
				return link != null && link.FO_IsConstrainedMode;
			}

			return false;
		}

		public static BMComponentReleaseGroupLink SwitchToConstrainedMode(GlbGroup group, ZGuid componentPK)
		{
			var link = GetLink(group.Factory, componentPK, group.PK);

			if (link == null)
			{
				link = group.Factory.New<BMComponentReleaseGroupLink>();
				link.FO_GG_ReleaseGroup = group.PK;
				link.FO_FC_Component = componentPK;
			}

			link.FO_IsConstrainedMode = ZBool.True;

			return link;
		}

		public static BMComponentReleaseGroupLink GetLink(BusinessObjectFactory factory, ZGuid componentPK, ZGuid releaseGroupPK)
		{
			var query = new ZQuery(BMComponentReleaseGroupLinkSchema.FO_GG_ReleaseGroup, releaseGroupPK);
			query.AddToFilter(BMComponentReleaseGroupLinkSchema.FO_FC_Component, componentPK);

			return factory.LoadTop1<BMComponentReleaseGroupLink>(query);
		}

		#endregion

		#region Is Capacity Constrained Resource

		public static bool IsDesignatedCapacityConstrainedResource(BusinessObjectFactory factory, string resourceCode, ZGuid componentPK)
		{
			var component = factory.Load<BMComponent>(componentPK);

			return IsDesignatedCapacityConstrainedResource(factory, resourceCode, component);
		}

		public static bool IsDesignatedCapacityConstrainedResource(GlbStaff resource, BMComponent component)
		{
			var factory = component.Factory;

			return IsDesignatedCapacityConstrainedResource(factory, resource.GS_Code, component);
		}

		public static bool IsDesignatedCapacityConstrainedResource(BusinessObjectFactory factory, ZString resourceCode, BMComponent component)
		{
			var service = factory.ServiceContainer.GetService<CapacityConstrainedResourcesCacheService>();

			if (service != null)
			{
				return service.GetCapacityConstrainedResourceStatus(resourceCode, component.PK).IsDesignatedCCR;
			}

			var componentLink = component.GetResourceLink(resourceCode);

			return componentLink != null && componentLink.FD_IsCapacityConstrained;
		}

		public static bool IsDesignatedCapacityConstrainedResourceInConstrainedReleaseGroup(BusinessObjectFactory factory, ZString resourceCode, BMComponent component)
		{
			return IsDesignatedCapacityConstrainedResource(factory, resourceCode, component)
				&& (!BMSystem.DoesSystemHaveReleaseGroups(component.Factory, component.FC_FS_System) || component.ReleaseGroupLinks.Where(l => l.FO_IsConstrainedMode).Any(g => g.ReleaseGroup.Staff.Cast<GlbStaff>().Any(s => s.GS_Code == resourceCode)));
		}

		public static bool IsCCRTask(IWorkflowTask task, IWorkflow workflow, BMComponent buffer, bool cCRsMustBeWithinConstrainedReleaseGroup = false, BusinessObjectFactory ccrFactory = null)
		{
			var factory = ccrFactory ?? buffer.Factory;
			var isDesignatedCCRFunc = cCRsMustBeWithinConstrainedReleaseGroup ? (Func<BusinessObjectFactory, ZString, BMComponent, bool>)IsDesignatedCapacityConstrainedResourceInConstrainedReleaseGroup : IsDesignatedCapacityConstrainedResource;

			if (!task.P9_GS_NKAssignedStaffMember.IsEmpty)
			{
				return isDesignatedCCRFunc(factory, task.P9_GS_NKAssignedStaffMember, buffer);
			}

			if (task.RequiresResourceWithCapability())
			{
				var staffIntersection = task.GetIntersectionOfCapabilityAndGroup(workflow, factory).Where(s => s.GS_IsActive).ToArray();
				return staffIntersection.Any() && staffIntersection.All(s => isDesignatedCCRFunc(factory, s.GS_Code, buffer));
			}

			return false;
		}

		public static bool IsCCRChannel(this IVisualBoardChannel channel, BMBoardSection section, bool? isInConstrainedMode = null)
		{
			var code = channel.ChannelEntityCode;

			return (isInConstrainedMode ?? section.IsInConstrainedMode)
				&& channel.EntityType == ChannelTypeList.Codes.Resource
				&& !code.IsEmpty
				&& IsDesignatedCapacityConstrainedResource(section.Component.Factory, code, section.Component);
		}

		#endregion

		#region Numbers

		public static decimal GetFractionOfBufferSizeForResourceFullCapacity(string resourceCode, BMComponent buffer)
		{
			if (IsDesignatedCapacityConstrainedResourceInConstrainedReleaseGroup(buffer.Factory, resourceCode, buffer))
			{
				var query = new ZQuery(BMComponentSchema.FC_FC_ParentComponent, buffer.PK);
				query.AddToFilter(BMComponentSchema.FC_Type, BMComponentTypeList.Codes.Constraint);
				var constraintSubComponent = buffer.Factory.LoadTop1<BMComponent>(query);

				if (constraintSubComponent != null)
				{
					return constraintSubComponent.FC_OffsetInMinutes / (decimal)buffer.FC_BufferTimespanInMinutes;
				}
			}

			return buffer.FC_BufferLoadLimitPercent / 100.0m;
		}

		public static decimal GetFullCapacityMultiplierConsideringCapacityConstrainedResources(string resourceCode, BMComponent buffer)
		{
			return IsDesignatedCapacityConstrainedResourceInConstrainedReleaseGroup(buffer.Factory, resourceCode, buffer)
				? 1m
				: (decimal)buffer.FC_NonCCRTemporaryOverloadLimitMultiplier;
		}

		#endregion

		#region Task Pre / Post Constraint

		public static void FetchForConstraintStatus(BusinessObjectFactory factory, IEnumerable<ICardContent> cards)
		{
			var tasks = factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, cards.Select(c => c.TaskIdentifier)));
			var workflows = factory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, cards.Select(c => c.WorkflowIdentifier)));

			workflows.Select(w => w.FH_FC_CurrentComponent)
				.Distinct()
				.ForEach(pk => factory.AddFetchHint(BMComponentSchema.PK, pk));

			tasks.Select(t => new { StaffCode = t.P9_GS_NKAssignedStaffMember, ComponentPK = t.ProcessHeader.FH_FC_CurrentComponent })
				.Where(c => !string.IsNullOrEmpty(c.StaffCode))
				.Distinct()
				.ForEach(pair => factory.AddFetchHint(BMComponentLinkSchema.Instance, BMComponent.GetComponentLinkQuery(pair.StaffCode, pair.ComponentPK)));
		}

		public static ConstraintStatus GetConstraintStatus(ProcessTask processTask, BusinessObjectFactory factory = null)
		{
			Argument.NotNull(processTask, "processTask");

			var workflow = processTask.GetProcessHeader();

			if (workflow != null)
			{
				var currentComponent = workflow.CurrentComponent;

				if (currentComponent != null && currentComponent.IsBuffer)
				{
					workflow.Factory.AddFetchHint(BMComponentResourceLinkSchema.FD_FC_Component, currentComponent.PK);

					if (IsCCRTask(processTask, workflow, currentComponent, cCRsMustBeWithinConstrainedReleaseGroup: false, factory))
					{
						return ConstraintStatus.ReadyForConstraint;
					}

					var allTasks = workflow.GetTasksWithoutAccessingWorkflowParent();
					var lastConstraintTask = allTasks.OrderByDescending(t => t.P9_Sequence).FirstOrDefault(task => IsCCRTask(task, workflow, currentComponent, cCRsMustBeWithinConstrainedReleaseGroup: false, factory));

					if (lastConstraintTask != null)
					{
						if (processTask.P9_Sequence <= lastConstraintTask.P9_Sequence)
						{
							return ConstraintStatus.PreConstraint;
						}
						else if (processTask.P9_Sequence > lastConstraintTask.P9_Sequence)
						{
							return ConstraintStatus.PostConstraint;
						}
					}
					return ConstraintStatus.NonConstrained;
				}
			}

			return ConstraintStatus.Unknown;
		}

		public static ConstraintStatus GetConstraintStatus(ProcessHeader workflow)
		{
			if (workflow is ProcessJobHeader)
			{
				return ConstraintStatus.Unknown;
			}

			var tasks = workflow.GetTasksWithoutAccessingWorkflowParent();

			if (tasks.Count == 0)
			{
				return ConstraintStatus.Unknown;
			}

			var firstOpenTask = (workflow.CurrentComponent != null)
				? tasks.OrderBy(t => t.P9_Sequence).ThenBy(task => !IsCCRTask(task, workflow, workflow.CurrentComponent)).FirstOrDefault(t => t.IsOpen)
				: tasks.OrderBy(t => t.P9_Sequence).FirstOrDefault(t => t.IsOpen);
			return firstOpenTask != null ? GetConstraintStatus(firstOpenTask) : ConstraintStatus.Unknown;
		}

		public static ConstraintStatus GetConstraintStatusFromString(string constraintStatusAsString)
		{
			switch (constraintStatusAsString)
			{
				case ConstraintStatusList.Codes.NonConstrained:
					return ConstraintStatus.NonConstrained;
				case ConstraintStatusList.Codes.PreConstraint:
					return ConstraintStatus.PreConstraint;
				case ConstraintStatusList.Codes.ReadyforConstraint:
					return ConstraintStatus.ReadyForConstraint;
				case ConstraintStatusList.Codes.PostConstraint:
					return ConstraintStatus.PostConstraint;
			}
			return ConstraintStatus.Unknown;
		}

		public static ZQuery GetConstraintStatusQuery(ConstraintStatus constraintStatus)
		{
			var sql = string.Empty;

			switch (constraintStatus)
			{
				case ConstraintStatus.NonConstrained:
					sql = FilterStripSqlQueryNonConstraint;
					break;
				case ConstraintStatus.ReadyForConstraint:
					sql = FilterStripSqlQueryReadyForConstraint;
					break;
				case ConstraintStatus.PreConstraint:
					sql = string.Format(CultureInfo.InvariantCulture, FilterStripSqlQueryPreConstraint, SqlQueryForLowerOpenTaskAndLastCCRTask);
					break;
				case ConstraintStatus.PostConstraint:
					sql = string.Format(CultureInfo.InvariantCulture, FilterStripSqlQueryPostConstraint, SqlQueryForLowerOpenTaskAndLastCCRTask);
					break;
				default:
					return ZQuery.NoResultQuery;
			}

			return new ZDBOnlyQuery(typeof(ProcessHeader)).AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection());
		}

		#endregion

		#region Buffer Pre / Post Constraint

		public static ConstraintStatus GetConstraintStatus(BMComponent component)
		{
			if (component.IsConstraint)
			{
				return ConstraintStatus.ReadyForConstraint;
			}
			else
			{
				var parentComponent = component.ParentComponent;

				if (parentComponent != null)
				{
					var constraint = parentComponent.ChildComponents.FirstOrDefault(c => c.IsConstraint);

					if (constraint != null)
					{
						if (component.FC_OffsetInMinutes < constraint.FC_OffsetInMinutes)
						{
							return ConstraintStatus.PreConstraint;
						}
						if (component.FC_OffsetInMinutes >= constraint.FC_OffsetInMinutes)
						{
							return ConstraintStatus.PostConstraint;
						}
					}
				}
			}

			return ConstraintStatus.Unknown;
		}

		#endregion

		#region Implementation

		const string FilterStripSqlQueryNonConstraint = @"FH_PK IN (
SELECT FH_PK FROM (
	SELECT FH_PK, ccrLinkCount, totalOpenTasks FROM (
		SELECT processheader.FH_PK, COUNT(link.FD_PK) ccrLinkCount, COUNT(openprocesstask.P9_PK) totalOpenTasks
		FROM dbo.ProcessHeader AS processheader
		INNER JOIN dbo.BMComponent AS component ON component.FC_PK = processheader.FH_FC_CurrentComponent
		LEFT JOIN dbo.ProcessTasks AS processtask on P9_FH_ProcessHeader = processheader.FH_PK
		LEFT JOIN dbo.BMComponentResourceLink link on link.FD_FC_Component = processheader.FH_FC_CurrentComponent AND link.FD_IsCapacityConstrained = 1
			AND link.FD_GS_NKResource = P9_GS_NKAssignedStaffMember
		LEFT JOIN dbo.ProcessTasks AS openprocesstask on openprocesstask.P9_FH_ProcessHeader = processheader.FH_PK
			AND openprocesstask.P9_Status NOT IN ('CLS', 'CAN')
		WHERE processHeader.FH_FH_ParentHeader IS NOT NULL
			AND component.FC_Type = 'BUF'
		group by FH_PK
	) headersWithStuff
		WHERE ccrLinkCount < 1
			AND (totalOpenTasks > 0)
) NonConstraintProcessHeader
)";

		const string FilterStripSqlQueryReadyForConstraint = @"FH_PK IN (
SELECT processheader.FH_PK
FROM dbo.ProcessHeader AS processheader
INNER JOIN (
	select * from (
		SELECT P9_FH_ProcessHeader, P9_GS_NKAssignedStaffMember, rank() over (partition by P9_FH_ProcessHeader order by processtask.P9_Sequence) rn
		FROM dbo.ProcessTasks AS processtask
		where processtask.P9_Status NOT IN ('CLS', 'CAN')
	) OpenTasks where rn = 1
) LowestOpenTasks ON LowestOpenTasks.P9_FH_ProcessHeader = processheader.FH_PK 
INNER JOIN dbo.BMComponent AS component ON component.FC_PK = processheader.FH_FC_CurrentComponent
INNER JOIN dbo.BMComponentResourceLink as link ON 
	link.FD_FC_Component = processheader.FH_FC_CurrentComponent 
	AND link.FD_GS_NKResource = LowestOpenTasks.P9_GS_NKAssignedStaffMember
WHERE link.FD_IsCapacityConstrained = 1
	AND component.FC_Type = 'BUF'
)";

		const string FilterStripSqlQueryPreConstraint = @"FH_PK IN (
{0}
	WHERE (LowestOpenTaskLink.FD_IsCapacityConstrained IS NULL OR LowestOpenTaskLink.FD_IsCapacityConstrained = 0)
		AND LowestOpenTask.P9_Sequence < LastCCRTask.P9_Sequence
		AND component.FC_Type = 'BUF'
)";

		const string FilterStripSqlQueryPostConstraint = @"FH_PK IN (
{0}
	WHERE LowestOpenTask.P9_GS_NKAssignedStaffMember IS NOT NULL 
		AND (LowestOpenTaskLink.FD_IsCapacityConstrained IS NULL OR LowestOpenTaskLink.FD_IsCapacityConstrained = 0)
		AND LowestOpenTask.P9_Sequence > LastCCRTask.P9_Sequence
		AND component.FC_Type = 'BUF'
)";

		const string SqlQueryForLowerOpenTaskAndLastCCRTask = @"
SELECT processheader.FH_PK
	FROM dbo.ProcessHeader AS processheader
	INNER JOIN dbo.BMComponent AS component ON component.FC_PK = processheader.FH_FC_CurrentComponent
	JOIN (
		SELECT * FROM (
			select P9_Sequence, P9_FH_ProcessHeader, P9_GS_NKAssignedStaffMember, row_number() over (partition by P9_FH_ProcessHeader order by processtask.P9_Sequence, processtask.P9_PK) rn
			FROM dbo.ProcessTasks AS processtask
			where processtask.P9_Status NOT IN ('CLS', 'CAN')
		) OpenTasks
		where rn = 1
	) LowestOpenTask on LowestOpenTask.P9_FH_ProcessHeader = processheader.FH_PK
	LEFT JOIN dbo.BMComponentResourceLink as LowestOpenTaskLink ON 
		LowestOpenTaskLink.FD_FC_Component = processheader.FH_FC_CurrentComponent 
		AND LowestOpenTaskLink.FD_GS_NKResource = LowestOpenTask.P9_GS_NKAssignedStaffMember
	JOIN (
		select * from (
			SELECT FD_FC_Component, P9_Sequence, P9_FH_ProcessHeader, P9_GS_NKAssignedStaffMember, row_number() over (partition by P9_FH_ProcessHeader, FD_FC_Component order by processtask.P9_Sequence desc, processtask.P9_PK) RN
			FROM dbo.ProcessTasks AS processtask
			INNER JOIN dbo.BMComponentResourceLink as link ON 
				link.FD_GS_NKResource = processtask.P9_GS_NKAssignedStaffMember
			WHERE processtask.P9_GS_NKAssignedStaffMember IS NOT NULL 
				AND link.FD_IsCapacityConstrained = 1
		) CCRTasks where rn = 1	
	) LastCCRTask
	on LastCCRTask.P9_GS_NKAssignedStaffMember != LowestOpenTask.P9_GS_NKAssignedStaffMember
		AND LastCCRTask.P9_FH_ProcessHeader = processheader.FH_PK
		AND LastCCRTask.FD_FC_Component = processheader.FH_FC_CurrentComponent
";

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Pipes;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.VisualBoards.Business.Test
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
	public abstract class VisualBoardsTestHelper
	{
		#region Create BusinessObjects

		public static void AddTaskTypesToRegistry(string workflowType, params string[] taskTypes)
		{
			MasterFilesTestHelper.AddTaskTypesToRegistry(workflowType, taskTypes);
		}

		public static void ClearTaskAssignmentRestrictions()
		{
			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new TaskTypeRestrictionsCollection());
		}

		public static void AddTaskAssignmentRestriction(string workflowType, string mainTaskType, string[] otherTaskTypes, string restrictionType, string notificationType = NotificationTypeList.Codes.Warning, string scope = ScopeList.Codes.Job, bool active = true)
		{
			var collection = WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.Value;

			var restriction = collection.AddNew();
			restriction.Active = active;
			restriction.WorkflowType = workflowType;
			restriction.TaskType = mainTaskType;
			restriction.NotificationType = notificationType;
			restriction.Scope = scope;
			restriction.RestrictionType = restrictionType;

			foreach (var type in otherTaskTypes)
			{
				var otherTaskType = restriction.TaskTypesCollection.AddNew();
				otherTaskType.Code = type;
			}

			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		public static Tuple<BMSystem, BMComponent> CreateSystemAndBuffer(BusinessObjectFactory factory, params string[] workflowTypes)
		{
			var system = CreateSystem(factory, workflowTypes);
			var buffer = CreateBuffer(system);
			return Tuple.Create(system, buffer);
		}

		public static BMSystem GetOrCreateSystem(BusinessObjectFactory factory, params string[] workflowTypes)
		{
			var system = factory.Load<BMSystem>(new ZQuery())
				.FirstOrDefault(s => s.RelatedWorkflowTypes
					.Select(c => (string)c.FSW_WorkflowType)
					.ContainsSameElementsInAnyOrder(workflowTypes));

			if (system == null)
			{
				system = factory.NewWithValidTestData<BMSystem>();
				system.FS_Description = "Herp Derble.";

				foreach (var type in workflowTypes)
				{
					AddWorkflowType(system, type);
				}
			}

			return system;
		}

		public static BMSystem CreateSystem(BusinessObjectFactory factory, params string[] workflowTypes)
		{
			var system = factory.NewWithValidTestData<BMSystem>();
			system.FS_Description = "Herp Derble.";
			system.FS_IsLive = true;

			foreach (var type in workflowTypes)
			{
				AddWorkflowType(system, type);
			}

			return system;
		}

		public static BMSystem CreateSystemAndRelatedWorkflowType(BusinessObjectFactory factory, string workflowType, bool isActive)
		{
			var system = factory.NewWithValidTestData<BMSystem>();
			system.FS_Description = "Herp Derble.";
			system.FS_IsLive = true;
			AddWorkflowType(system, workflowType, isActive);
			return system;
		}

		public static void AddWorkflowType(BMSystem system, string workflowType, bool isActive = true)
		{
			if (system.Factory.Exists(typeof(BMSystemWorkflowDeterminer), new ZQuery(BMSystemWorkflowDeterminerSchema.FSW_WorkflowType, workflowType)))
			{
				throw new InvalidOperationException(string.Format($"The related workflow type {workflowType} has already been added to another system."));
			}

			var relatedWorkflowTypes = system.RelatedWorkflowTypes.AddNew();
			relatedWorkflowTypes.FSW_WorkflowType = workflowType;
			relatedWorkflowTypes.FSW_IsActive = isActive;
		}

		public static BMComponent CreateBucket(BMSystem system, string name = "bucket", int offsetMinutes = 0, int sequence = 0)
		{
			var bucket = system.Components.AddNew();
			bucket.FC_Name = name;
			bucket.FC_DisplaySequence = sequence;
			bucket.FC_OffsetInMinutes = offsetMinutes;

			return bucket;
		}

		public static BMComponent CreateBuffer(BMSystem system, string name = "buffer", int timespanMinutes = 96 * 60, byte loadLimitPercent = 50, int sequence = 0)
		{
			var buffer = system.Components.AddNew();
			buffer.FC_Name = name;
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			buffer.FC_BufferTimespanInMinutes = timespanMinutes;
			buffer.FC_BufferLoadLimitPercent = loadLimitPercent;
			buffer.FC_DisplaySequence = sequence;

			return buffer;
		}

		public static BMComponent CreateConstraint(BMComponent buffer, string name = "constraint", int offsetMinutes = 0, int sequence = 0)
		{
			var constraint = buffer.ChildComponents.AddNew();
			constraint.FC_Name = buffer.FC_Name + name;
			constraint.FC_Type = BMComponentTypeList.Codes.Constraint;
			constraint.FC_OffsetInMinutes = offsetMinutes;
			constraint.FC_DisplaySequence = sequence;

			return constraint;
		}

		public static BMComponent CreateDecouple(BMComponent buffer, string name = "decouple", int offsetMinutes = 0, int sequence = 0)
		{
			var constraint = buffer.ChildComponents.AddNew();
			constraint.FC_Name = name;
			constraint.FC_Type = BMComponentTypeList.Codes.Decouple;
			constraint.FC_OffsetInMinutes = offsetMinutes;
			constraint.FC_DisplaySequence = sequence;

			return constraint;
		}

		public static BMComponent CreateSubBuffer(BMComponent buffer, string name = "sub-buffer", int timespanMinutes = 96 * 60, int offsetMinutes = 0, int sequence = 0)
		{
			var subBuffer = buffer.ChildComponents.AddNew();
			subBuffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			subBuffer.FC_Name = name;
			subBuffer.FC_BufferTimespanInMinutes = timespanMinutes;
			subBuffer.FC_OffsetInMinutes = offsetMinutes;
			subBuffer.FC_DisplaySequence = sequence;

			return subBuffer;
		}

		public static BMZoneCapacityMultiplier CreateZoneMultiplier(BMComponent buffer, decimal zone3Multiplier = 1, decimal zone2Multiplier = 1, decimal zone1Multiplier = 2, decimal zone0Multiplier = 3, ZGuid releaseGroupPK = default(ZGuid))
		{
			var zoneMultiplier = buffer.ZoneCapacityMultipliers.AddNew();
			zoneMultiplier.BZC_Zone3Multiplier = zone3Multiplier;
			zoneMultiplier.BZC_Zone2Multiplier = zone2Multiplier;
			zoneMultiplier.BZC_Zone1Multiplier = zone1Multiplier;
			zoneMultiplier.BZC_Zone0Multiplier = zone0Multiplier;

			zoneMultiplier.BZC_GG_ReleaseGroup = releaseGroupPK;

			return zoneMultiplier;
		}

		public static BMSystemReleaseGroup CreateReleaseGroup(BMSystem system, GlbGroup group, BMComponent constrainedModeComponent = null)
		{
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			if (constrainedModeComponent != null)
			{
				var link = constrainedModeComponent.ReleaseGroupLinks.AddNew();
				link.FO_GG_ReleaseGroup = group.PK;
				link.FO_IsConstrainedMode = true;
			}

			return releaseGroup;
		}

		public static BMSystemReleaseGroup GetReleaseGroup(BusinessObjectFactory factory, BMSystem system, GlbGroup group)
		{
			var releaseGroup = factory.Load<BMSystemReleaseGroup>(new ZQuery())
			.FirstOrDefault(s => s.BMSystem.PK.Equals(system.PK) && s.Group.PK.Equals(group.PK));

			return releaseGroup;
		}

		public static BMBoardSectionChannel CreatePrimaryChannelForSection(BMBoardSection section, ZString channelTypeCode, ZGuid? channelPK, bool overrideChannels = true, int? displaySequence = null)
		{
			section.SectionConfiguration.OverrideChannels = overrideChannels;

			if (section.SectionConfiguration.ChannelBy == ChannelTypeList.Codes.NotChanneled)
			{
				section.SectionConfiguration.ChannelBy = channelTypeCode;
			}

			var channel = section.SectionConfiguration.PrimaryAxisChannels.AddNew();
			SetChannelConfig(channel, channelTypeCode, channelPK, displaySequence);

			return channel;
		}

		public static BMBoardSectionChannel CreatePrimaryChannelForSection(BMBoardSection section, bool overrideChannels = true)
		{
			return CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, ZGuid.Empty, overrideChannels);
		}

		public static BMBoardSectionChannel CreateSecondaryChannelForSection(BMBoardSection section, ZString channelTypeCode, ZGuid channelPK, bool overrideChannels = true, int? displaySequence = null)
		{
			section.SectionConfiguration.OverrideSecondaryChannels = overrideChannels;

			if (section.SectionConfiguration.ChannelSecondaryBy == ChannelTypeList.Codes.NotChanneled)
			{
				section.SectionConfiguration.ChannelSecondaryBy = channelTypeCode;
			}

			var channel = section.SectionConfiguration.SecondaryAxisChannels.AddNew();
			SetChannelConfig(channel, channelTypeCode, channelPK, displaySequence);

			return channel;
		}

		public static BMBoardSectionChannel CreateSecondaryChannelForSection(BMBoardSection section, bool overrideChannels = true)
		{
			return CreateSecondaryChannelForSection(section, ChannelTypeList.Codes.Resource, ZGuid.Empty, overrideChannels);
		}

		static void SetChannelConfig(BMBoardSectionChannel channel, ZString channelTypeCode, ZGuid? channelPK, int? displaySequence)
		{
			channel.MSC_ChannelType = channelTypeCode;

			if (channelPK != null)
			{
				channel.MSC_ParentID = channelPK.Value;
			}

			if (displaySequence != null)
			{
				channel.MSC_Sequence = displaySequence.Value;
			}
		}

		public static BMComponentLink LinkComponents(BMComponent from, BMComponent to, byte sequence = 0, bool? isReleaseGate = null, bool? isActive = null)
		{
			var link = from.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = to.PK;
			link.FL_Sequence = sequence;

			if (isReleaseGate != null)
			{
				link.FL_IsReleaseGateRuleApplied = isReleaseGate.Value;
			}

			if (isActive != null)
			{
				link.FL_TransferRulesEnabled = isActive.Value;
			}

			return link;
		}

		public static ProcessJobHeader CreateJobHeader<T>(BusinessObjectFactory factory, bool addDefaultProcessHeaderIfNone = true, string description = null, bool checkTemplates = false)
			where T : BusinessObject, IWorkflowProviderCore
		{
			var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)factory.NewWithValidTestData<T>(), factory, addDefaultProcessHeaderIfNone, checkTemplates);
			if (description != null)
			{
				jobHeader.FH_CompletionStatement = description;
			}

			return jobHeader;
		}

		static void SetWorkflowAttributes(ProcessHeader workflow, string completionStatement, BMComponent currentComponent = null, ZDateTime? releaseDateTime = null, ZGuid? releaseGroupPK = null, bool autoAssignTasks = false, short? nudge = null)
		{
			workflow.FH_CompletionStatement = completionStatement;
			workflow.FH_AllowTaskAutoAssignment = autoAssignTasks;

			if (currentComponent != null)
			{
				workflow.FH_FC_CurrentComponent = currentComponent.PK;
			}
			workflow.FH_ReleaseDateTime = releaseDateTime ?? ZDateTime.UtcNow;

			if (releaseGroupPK.HasValue)
			{
				workflow.FH_GG_ReleaseGroup = releaseGroupPK.Value;
			}

			if (nudge != null)
			{
				workflow.FH_VoteUpDownAmount = nudge.Value;
			}
		}

		public static ProcessHeader CreateWorkflow(BusinessObjectFactory factory, string completionStatement, BMComponent currentComponent = null, ZDateTime? releaseDateTime = null, ZGuid? releaseGroupPK = null, bool autoAssignTasks = false)
		{
			var jobHeader = CreateJobHeader<OrgHeader>(factory);
			var hasDefaultWorkflow = jobHeader.ProcessHeaders.Count == 1;
			var workflow = hasDefaultWorkflow ? jobHeader.ProcessHeaders[0] : factory.New<ProcessHeader>();

			SetWorkflowAttributes(workflow, completionStatement, currentComponent, releaseDateTime, releaseGroupPK, autoAssignTasks);

			if (!hasDefaultWorkflow)
			{
				jobHeader.ProcessHeaders.AddAndSetDefaults(workflow);
			}

			return workflow;
		}

		public static ProcessHeader CreateWorkflow(ProcessJobHeader jobHeader, string completionStatement, BMComponent currentComponent = null, ZDateTime? releaseDateTime = null, ZGuid? releaseGroupPK = null, bool autoAssignTasks = false, short? nudge = null)
		{
			return CreateWorkflow<ProcessHeader>(jobHeader, completionStatement, currentComponent, releaseDateTime, releaseGroupPK, autoAssignTasks, nudge);
		}

		public static TWorkflowType CreateWorkflow<TWorkflowType>(ProcessJobHeader jobHeader, string completionStatement, BMComponent currentComponent = null, ZDateTime? releaseDateTime = null, ZGuid? releaseGroupPK = null, bool autoAssignTasks = false, short? nudge = null)
			where TWorkflowType : ProcessHeader
		{
			var workflow = jobHeader.Factory.New<TWorkflowType>();
			jobHeader.ProcessHeaders.AddAndSetDefaults(workflow);

			SetWorkflowAttributes(workflow, completionStatement, currentComponent, releaseDateTime, releaseGroupPK, autoAssignTasks, nudge);

			return workflow;
		}

		public static ProcessHeader CreateWorkflowAndParents<T>(BusinessObjectFactory factory, string completionStatement, BMComponent currentComponent = null, ZDateTime? releaseDateTime = null, ZGuid? releaseGroupPK = null, bool autoAssignTasks = false, short? nudge = null)
			where T : BusinessObject, IWorkflowProviderCore
		{
			return CreateWorkflowAndParents<T, ProcessHeader>(factory, completionStatement, currentComponent, releaseDateTime, releaseGroupPK, autoAssignTasks, nudge);
		}

		public static TWorkflowType CreateWorkflowAndParents<TJobType, TWorkflowType>(BusinessObjectFactory factory, string completionStatement, BMComponent currentComponent = null, ZDateTime? releaseDateTime = null, ZGuid? releaseGroupPK = null, bool autoAssignTasks = false, short? nudge = null)
			where TJobType : BusinessObject, IWorkflowProviderCore
			where TWorkflowType : ProcessHeader
		{
			var jobHeader = CreateJobHeader<TJobType>(factory, addDefaultProcessHeaderIfNone: false);
			return CreateWorkflow<TWorkflowType>(jobHeader, completionStatement, currentComponent, releaseDateTime, releaseGroupPK, autoAssignTasks, nudge);
		}

		static ProcessHeader CreateWorkflowAndTaskInternal(ProcessHeader workflow, string completionStatement, BMComponent currentComponent = null, ZDateTime? releaseDateTime = null, string staffCode = "", int lowEstMinutes = 0, string taskType = "UDF", int estVariationFactor = 1, ZInt? sequence = null, string description = "", string taskStatus = "ASN", GlbCapability capability = null)
		{
			var system = workflow.BMSystem;
			if (system != null && system.ReleaseGroups.Count == 1)
			{
				workflow.FH_GG_ReleaseGroup = system.ReleaseGroups[0].FSG_GG_Group;
			}

			CreateTask(workflow, staffCode, lowEstMinutes, taskType: taskType, estVariationFactor: estVariationFactor, sequence: sequence, description: description, taskStatus: taskStatus, capability: capability);

			return workflow;
		}

		public static ProcessHeader CreateWorkflowAndTask(BusinessObjectFactory factory, string completionStatement, BMComponent currentComponent = null, ZDateTime? releaseDateTime = null, string staffCode = "", int lowEstMinutes = 0, string taskType = "UDF", int estVariationFactor = 1, ZInt? sequence = null, string description = "", string taskStatus = "ASN", GlbCapability capability = null)
		{
			var workflow = CreateWorkflow(factory, completionStatement, currentComponent, releaseDateTime);
			return CreateWorkflowAndTaskInternal(workflow, completionStatement, currentComponent, releaseDateTime, staffCode, lowEstMinutes, taskType, estVariationFactor, sequence, description, taskStatus: taskStatus, capability: capability);
		}

		public static ProcessHeader CreateWorkflowAndTask(ProcessJobHeader jobHeader, string completionStatement, BMComponent currentComponent = null, ZDateTime? releaseDateTime = null, string staffCode = "", int lowEstMinutes = 0, string taskType = "UDF", int estVariationFactor = 1, ZInt? sequence = null, string description = "", GlbCapability capability = null, string taskStatus = ProcessTaskStatusCodeList.Codes.Assigned)
		{
			var workflow = CreateWorkflow(jobHeader, completionStatement, currentComponent, releaseDateTime);
			return CreateWorkflowAndTaskInternal(workflow, completionStatement, currentComponent, releaseDateTime, staffCode, lowEstMinutes, taskType, estVariationFactor, sequence, description, taskStatus, capability: capability);
		}

		public static ProcessHeader[] CreateWorkflows(BMComponent component, int numberOfWorkflows, int numberOfTasksPerWorkflow = 0, GlbGroup releaseGroup = null, GlbStaff staff = null, GlbCapability capability = null)
		{
			return CreateWorkflows<OrgHeader>(component, numberOfWorkflows, numberOfTasksPerWorkflow, releaseGroup, staff, capability);
		}

		public static ProcessHeader[] CreateWorkflows<T>(BMComponent component, int numberOfWorkflows, int numberOfTasksPerWorkflow = 0, GlbGroup releaseGroup = null, GlbStaff staff = null, GlbCapability capability = null)
			where T : BusinessObject, IWorkflowProvider
		{
			var workflows = new ProcessHeader[numberOfWorkflows];

			for (int i = 0; i < numberOfWorkflows; i++)
			{
				var workflow = workflows[i] = CreateJobHeader<T>(component.Factory).ProcessHeaders[0];
				workflow.FH_FC_CurrentComponent = component.PK;
				workflow.GetApplicableTagLinks();

				if (releaseGroup != null)
				{
					workflow.FH_GG_ReleaseGroup = releaseGroup.PK;
					workflow.JobHeader.FH_GG_ReleaseGroup = releaseGroup.PK;
				}

				workflow.FH_CompletionStatement = "Workflow " + i;

				for (int j = 0; j < numberOfTasksPerWorkflow; j++)
				{
					CreateTask(workflow, (staff ?? GlbStaff.CurrentUser).GS_Code, 60, capability: capability, description: "Task " + j);
				}
			}

			return workflows;
		}

		public static bool MakeChildOf(ProcessHeader child, ProcessHeader processHeader)
		{
			if (!child.IsChildOf(processHeader))
			{
				MakeChildOfAndGetLink(child, processHeader);

				return true;
			}

			return false;
		}

		public static ProcessHeaderLink MakeChildOfAndGetLink(ProcessHeader child, ProcessHeader parent)
		{
			var link = child.LinksFromMeToOthers_ForBinding.AddNew();
			link.FP_FH_HeaderTo = parent.PK;
			link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;

			return link;
		}

		public static IEnumerable<ProcessTask> CreateTasks(IWorkflowProvider parent, ProcessHeader workflow, params Tuple<string, int, string, ZGuid>[] taskDefinitions)
		{
			var tasks = new List<ProcessTask>();

			for (int i = 0; i < taskDefinitions.Length; i++)
			{
				var def = taskDefinitions[i];
				var task = parent.WorkflowItems.AddNew();
				task.P9_FH_ProcessHeader = workflow.PK;
				task.P9_Status = def.Item1;
				task.P9_Sequence = def.Item2;
				task.P9_GS_NKAssignedStaffMember = def.Item3;
				task.P9_G4_RequiredCapability = def.Item4;
				task.P9_NotesAsString = string.Format("{0} Task {1}", workflow.FH_CompletionStatement, def.Item2);

				tasks.Add(task);
			}

			return tasks;
		}

		public static ProcessTask CreateTask(ProcessHeader workflow, string staffCode = "", int lowEstMinutes = 0, string taskType = "UDF", string taskStatus = "ASN", GlbCapability capability = null, ZInt? sequence = null, int estVariationFactor = 2, string description = "", ZDateTime? estimatedHandoverTimeUtc = null, ZGuid? taskGroupPK = null)
		{
			var task = CreateTask(workflow.Parent, staffCode, lowEstMinutes, taskType, taskStatus, capability, sequence, estVariationFactor, description, estimatedHandoverTimeUtc, workflow);
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_GG_AssignedGroup = taskGroupPK ?? ZGuid.Empty;

			if (!workflow.IsDeleted)
			{
				task.P9_FC_CurrentComponent = workflow.FH_FC_CurrentComponent;
			}

			return task;
		}

		public static ProcessTask CreateTaskForTemplate(ProcessTaskTemplate template, ProcessHeader workflow, string staffCode = "", int lowEstMinutes = 0, string taskType = "UDF", string taskStatus = "ASN", GlbCapability capability = null, ZInt? sequence = null, int estVariationFactor = 2, string description = "", ZDateTime? estimatedHandoverTimeUtc = null, ZGuid? taskGroupPK = null)
		{
			return CreateTask(template, staffCode, lowEstMinutes, taskType, taskStatus, capability, sequence, estVariationFactor, description, estimatedHandoverTimeUtc, workflow);
		}

		public static ProcessTask CreateTaskWithCompany(ProcessHeader workflow, string staffCode = "", int lowEstMinutes = 0, string taskType = "UDF", string taskStatus = "ASN", GlbCapability capability = null, ZInt? sequence = null, int estVariationFactor = 2, string description = "", IGlbCompany company = null)
		{
			var task = CreateTask(workflow.Parent, staffCode, lowEstMinutes, taskType, taskStatus, capability, sequence, estVariationFactor, description);
			task.P9_GC = (company != null) ? company.PK : GlbCompany.CurrentCompany.PK;

			return task;
		}

		public static ProcessTask CreateTask(IWorkflowProvider parent, string staffCode = "", int lowEstMinutes = 0, string taskType = "UDF", string taskStatus = "ASN", GlbCapability capability = null, ZInt? sequence = null, int estVariationFactor = 2, string description = "", ZDateTime? estimatedHandoverTimeUtc = null, IProcessHeader workflow = null)
		{
			var task = parent.WorkflowItems.AddNew();
			if (workflow != null)
			{
				task.P9_FH_ProcessHeader = workflow.PK;
			}
			task.P9_GS_NKAssignedStaffMember = staffCode;
			task.P9_EstDuration = new ZInt(lowEstMinutes).GetDateTimeFromMinutes();
			task.P9_Type = taskType;
			task.P9_Status = taskStatus;
			task.P9_EstimateVariationFactor = estVariationFactor;

			if (!string.IsNullOrEmpty(description))
			{
				task.P9_Description = description;
			}

			if (capability != null)
			{
				task.P9_G4_RequiredCapability = capability.PK;
			}
			if (sequence != null)
			{
				task.P9_Sequence = sequence.Value;
			}
			if (estimatedHandoverTimeUtc != null)
			{
				task.P9_EstimatedHandoverTime = new ZDateTimeOffset(estimatedHandoverTimeUtc.Value, DateTimeKind.Utc);
			}

			return task;
		}

		public static ProcessTask CreateStandaloneTask(BusinessObjectFactory factory, string staffCode = null, string taskStatus = null)
		{
			var task = factory.New<ProcessTask>();

			if (staffCode != null)
			{
				task.P9_GS_NKAssignedStaffMember = staffCode;
			}

			if (taskStatus != null)
			{
				task.P9_Status = taskStatus;
			}

			return task;
		}

		public static ProcessHeader CreateProcessHeaderAndTask(ProcessJobHeader jobHeader, BMComponent component, double daysSinceRelease, string assignedResource = "", ZGuid requiredCapability = default(ZGuid), string completionStatement = null, ZDateTime taskLowEst = default(ZDateTime), ZGuid releaseGroupPK = default(ZGuid))
		{
			var workflow = jobHeader.Factory.New<ProcessHeader>();
			workflow.FH_FC_CurrentComponent = component.PK;
			workflow.FH_ReleaseDateTime = ZDateTime.Now.ToDateTime().AddDays(-daysSinceRelease);
			workflow.FH_CompletionStatement = completionStatement ?? daysSinceRelease.ToString();
			jobHeader.ProcessHeaders.AddAndSetDefaults(workflow);

			if (releaseGroupPK.IsValid)
			{
				workflow.FH_GG_ReleaseGroup = releaseGroupPK;
			}

			var task = jobHeader.Parent.WorkflowItems.AddNew();
			task.P9_Description = "Task " + workflow.FH_CompletionStatement;
			task.P9_GS_NKAssignedStaffMember = assignedResource;
			task.P9_G4_RequiredCapability = requiredCapability;
			task.P9_FH_ProcessHeader = workflow.PK;
			if (!taskLowEst.IsEmpty)
			{
				task.P9_EstDuration = taskLowEst;
			}

			return workflow;
		}

		public static ProcessHeader CreateProcessHeader(ProcessJobHeader header, BMComponent component, string completionStatement, ZDateTime releaseDate)
		{
			var workflow = header.ProcessHeaders.AddNew();
			workflow.FH_FC_CurrentComponent = component.PK;
			workflow.FH_CompletionStatement = completionStatement;
			workflow.FH_ReleaseDateTime = releaseDate;

			return workflow;
		}

		public static void MakeCompletionStatementTaskType(string workflowType, string completionStatementTaskType)
		{
			var categorisedTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.Value;
			var taskTypeCategory = categorisedTaskTypes
				.OfType<CategorisedWorkflowTaskTypes>()
				.FirstOrDefault(x => x.Code == workflowType);

			if (taskTypeCategory == null)
			{
				taskTypeCategory = categorisedTaskTypes.AddNew();
				taskTypeCategory.Code = workflowType;
			}

			var taskType = taskTypeCategory.TaskTypes
				.OfType<WorkflowTaskType>()
				.FirstOrDefault(x => x.Code == completionStatementTaskType);

			if (taskType == null)
			{
				taskType = taskTypeCategory.TaskTypes.AddNew();
				taskType.Code = completionStatementTaskType;
			}
			taskType.IsCompletionStatementTaskType = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
		}

		public static BMBoardSlideshow CreateSlideshow(BusinessObjectFactory factory, params BMBoard[] boards)
		{
			return CreateSlideshow(factory, boards.Select(b => Tuple.Create(b, 60)).ToArray());
		}

		public static BMBoardSlideshow CreateSlideshow(BusinessObjectFactory factory, params Tuple<BMBoard, int>[] boardsAndTheirDisplayDurations)
		{
			var slideshow = factory.NewWithValidTestData<BMBoardSlideshow>();

			foreach (var tuple in boardsAndTheirDisplayDurations)
			{
				var pivot = slideshow.BoardPivots.AddNew();

				pivot.MC_MB_Board = tuple.Item1.PK;
				pivot.MC_DurationInSeconds = (short)tuple.Item2;
			}

			return slideshow;
		}

		public static BMBoard CreateBoard(BMSystem system, string name = "An Board", string description = "This Board")
		{
			var board = system.Boards.AddNew();
			board.MB_Name = name;
			board.MB_Description = description;

			return board;
		}

		public static BMBoardSection CreateBoardSection(BMComponent component, BMBoard board = null, int row = 0, int col = 0, int rowHeightPercent = 100, int colWidthPercent = 100, string backColor = null, string panelLayoutStyle = null, int? cellsPerSubsection = null, bool setReleaseGroupIfRequired = true, ZGuid? customReleaseGroupPK = null)
		{
			if (board == null)
			{
				board = CreateBoard(component.System);
			}

			var section = board.Sections.AddNew();
			section.MS_FC_Component = component.PK;

			section.Row = row;
			section.Column = col;
			section.RowHeightPercent = rowHeightPercent;
			section.ColWidthPercent = colWidthPercent;

			if (!string.IsNullOrEmpty(backColor))
			{
				section.BackgroundColor = backColor;
			}

			if (!string.IsNullOrEmpty(panelLayoutStyle))
			{
				section.SectionConfiguration.PanelLayoutStyle = panelLayoutStyle;
			}

			if (cellsPerSubsection != null)
			{
				section.SectionConfiguration.CellsPerSubsection = cellsPerSubsection.Value;
			}

			if (customReleaseGroupPK != null)
			{
				section.SectionConfiguration.ReleaseGroupPK = customReleaseGroupPK.Value;
			}
			else if (setReleaseGroupIfRequired && component.IsBucket && component.System != null && component.System.ReleaseGroups.Count == 1)
			{
				section.SectionConfiguration.ReleaseGroupPK = component.System.ReleaseGroups[0].FSG_GG_Group;
			}

			return section;
		}

		public static void SetOverriddenSectionName(BMBoardSection section, string name)
		{
			section.SectionConfiguration.SectionNameIsOverridden = true;
			section.SectionConfiguration.SectionNameOverride = name;
		}

		public static BMBoardSection CreateBoardSection(ModuleIdentifier moduleIdentifier, BMBoard board)
		{
			var section = board.Sections.AddNew();
			section.MS_SectionType = BMConstants.ModuleGridSectionType;
			var panelConfig = new ModuleGridSectionPanelConfiguration(section);
			panelConfig.ModuleName = moduleIdentifier.Name;
			((ModuleGridSectionConfiguration)section.Configuration).PanelConfigurations.Add(panelConfig);

			return section;
		}

		public static BoardSectionDataSource GetDataSource(BMBoardSection section)
		{
			return new BoardSectionDataSource(section, CreateViewModel(section));
		}

		public static BMBoardSection CreateReleaseSchedulerBoardSection(BMComponent component, GlbGroup releaseGroup, BMBoard board = null, bool ensureReleaseGroupIsInConstrainedMode = true)
		{
			if (releaseGroup != null && ensureReleaseGroupIsInConstrainedMode && !ConstrainedModeHelper.IsInConstrainedMode(component, releaseGroup.PK))
			{
				ConstrainedModeHelper.SwitchToConstrainedMode(releaseGroup, component.PK);
			}

			var section = CreateBoardSection(component, board);
			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.CellsPerSubsection = 1;
			sectionConfiguration.ShowZones = false;
			sectionConfiguration.FadeBackgroundAtPercentage = 0;
			sectionConfiguration.IsReleaseScheduler = true;

			if (releaseGroup != null)
			{
				sectionConfiguration.ReleaseGroupPK = releaseGroup.PK;
			}

			sectionConfiguration.CardType = CardTypeList.Codes.Workflow;
			sectionConfiguration.ChannelBy = sectionConfiguration.ChannelSecondaryBy = ChannelTypeList.Codes.ReleaseSchedulerChannels;
			section.BackgroundColor = "Beige";
			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			sectionConfiguration.ShowZones = false;
			sectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Stacked;

			return section;
		}

		public static BMBoardSectionAdditionalComponent CreateAdditionalComponent(BMBoardSection section, BMComponent component)
		{
			var componentLink = section.SectionConfiguration.AdditionalComponents.AddNew();
			componentLink.BSA_FC_Component = component.PK;
			return componentLink;
		}

		public static BMComponentResourceLink CreateComponentResourceLink(BMComponent component, GlbStaff resource)
		{
			var link = component.ResourceLinks.AddNew();

			link.FD_GS_NKResource = resource.GS_Code;

			return link;
		}

		public static GlbStaff CreateStaffInCurrentBranchDept(BusinessObjectFactory factory)
		{
			return CreateStaffInCurrentBranchDept(factory, null, null);
		}

		public static GlbStaff CreateStaffInCurrentBranchDept(BusinessObjectFactory factory, string code, string fullName, params GlbCapability[] capabilities)
		{
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;

			if (code != null)
			{
				staff.GS_Code = code;
			}
			if (fullName != null)
			{
				staff.GS_FullName = fullName;
			}

			if (capabilities.Length > 0)
			{
				staff.Capabilities.AddRange(capabilities);
			}

			return staff;
		}

		public static GlbGroupLink GetOrCreateGroupLink(BusinessObjectFactory factory, ZGuid groupPK, ZGuid staffPK)
		{
			var groupQuery = new ZQuery(GlbGroupLinkSchema.GK_GG, groupPK) { ReLoadExistingRows = true };
			var query = new ZQuery(GlbGroupLinkSchema.GK_GS, staffPK);
			query.AddToFilter(groupQuery);
			var groupLink = factory.LoadTop1<GlbGroupLink>(query);

			if (groupLink == null)
			{
				groupLink = factory.NewWithValidTestData<GlbGroupLink>();
				groupLink.GK_GG = groupPK;
				groupLink.GK_GS = staffPK;
			}

			return groupLink;
		}

		public static GlbGroup CreateGroup(BusinessObjectFactory factory, string code = null, string description = null)
		{
			var group = factory.NewWithValidTestData<GlbGroup>();

			if (code != null)
			{
				group.GG_Code = code;
			}

			if (description != null)
			{
				group.GG_Desc = description;
			}

			return group;
		}

		public static GlbResourceCapabilityPivot GetOrCreateCapabilityPivot(BusinessObjectFactory factory, ZGuid capabilityPK, ZGuid staffPK)
		{
			var capabilityQuery = new ZQuery(GlbResourceCapabilityPivotSchema.G5_G4_Capability, capabilityPK) { ReLoadExistingRows = true };
			var query = new ZQuery(GlbResourceCapabilityPivotSchema.G5_GS_Resource, staffPK);
			query.AddToFilter(capabilityQuery);
			var capabilityPivot = factory.LoadTop1<GlbResourceCapabilityPivot>(query);

			if (capabilityPivot == null)
			{
				capabilityPivot = factory.NewWithValidTestData<GlbResourceCapabilityPivot>();
				capabilityPivot.G5_G4_Capability = capabilityPK;
				capabilityPivot.G5_GS_Resource = staffPK;
			}

			return capabilityPivot;
		}

		public static GlbCapability CreateCapability(BusinessObjectFactory factory, string code = null, string description = null, bool autoAssignTasks = false, bool isGroupScope = false)
		{
			var capability = factory.NewWithValidTestData<GlbCapability>();

			if (code != null)
			{
				capability.G4_Code = code;
			}

			if (description != null)
			{
				capability.G4_Description = description;
			}

			capability.G4_AllowTaskAutoAssignment = autoAssignTasks;

			if (isGroupScope)
			{
				capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			}

			return capability;
		}

		public static BMComponentAcceptabilityBand CreateAcceptabilityBand_WorkflowsInComponent(BMComponent component, int cautionMin, int goodMin, int excellentMin, int excellentMax, int goodMax, int cautionMax, string name = "Number of Workflows", bool filterByReleaseGroup = true)
		{
			return CreateAcceptabilityBand(component, cautionMin, goodMin, excellentMin, excellentMax, goodMax, cautionMax, name, filterByReleaseGroup ? NumberOfWorkflowsInComponentWithReleaseGroupSql : NumberOfWorkflowsInComponentWithoutReleaseGroupSql);
		}

		public static BMComponentAcceptabilityBand CreateAcceptabilityBand_AverageNumberOfTasksPerWorkflow(BMComponent component, int cautionMin, int goodMin, int excellentMin, int excellentMax, int goodMax, int cautionMax)
		{
			return CreateAcceptabilityBand(component, cautionMin, goodMin, excellentMin, excellentMax, goodMax, cautionMax, "Number of Tasks per Workflow", AverageNumberOfTasksPerWorkflowSql);
		}

		public static BMComponentAcceptabilityBand CreateAcceptabilityBand_NullResult(BMComponent component, int cautionMin, int goodMin, int excellentMin, int excellentMax, int goodMax, int cautionMax, string name = "Null Result")
		{
			return CreateAcceptabilityBand(component, cautionMin, goodMin, excellentMin, excellentMax, goodMax, cautionMax, name, NullResultSql);
		}

		public static BMComponentAcceptabilityBand CreateAcceptabilityBand(BusinessObjectFactory factory, int cautionMin, int goodMin, int excellentMin, int excellentMax, int goodMax, int cautionMax, string name, string sql = "", string type = AcceptabilityBandTypes.Codes.SQL)
		{
			var band = factory.New<BMComponentAcceptabilityBand>();
			SetBandParams(band, cautionMin, goodMin, excellentMin, excellentMax, goodMax, cautionMax, name, sql, type);

			return band;
		}

		public static BMComponentAcceptabilityBand CreateAcceptabilityBand(BMComponent component, int cautionMin, int goodMin, int excellentMin, int excellentMax, int goodMax, int cautionMax, string name, string sql = "", string type = AcceptabilityBandTypes.Codes.SQL)
		{
			var band = component.AcceptabilityBands.AddNew();
			SetBandParams(band, cautionMin, goodMin, excellentMin, excellentMax, goodMax, cautionMax, name, sql, type);

			return band;
		}

		static void SetBandParams(BMComponentAcceptabilityBand band, int cautionMin, int goodMin, int excellentMin, int excellentMax, int goodMax, int cautionMax, string name, string sql = "", string type = AcceptabilityBandTypes.Codes.SQL)
		{
			band.BAB_CautionLowerBound = cautionMin;
			band.BAB_GoodLowerBound = goodMin;
			band.BAB_ExcellentLowerBound = excellentMin;
			band.BAB_ExcellentUpperBound = excellentMax;
			band.BAB_GoodUpperBound = goodMax;
			band.BAB_CautionUpperBound = cautionMax;

			band.BAB_Name = name;

			if (string.IsNullOrEmpty(sql) && type == AcceptabilityBandTypes.Codes.SQL)
			{
				sql = "SELECT 0 Value, NEWID() Component, NEWID() ReleaseGroup";
			}
			band.BAB_SqlText = sql;

			band.BAB_Type = type;
		}

		public static TagDefinition CreateTagDefinition(BusinessObjectFactory factory, string code, string description = "", bool isExclusive = false, string usageScope = null, string scope = null, bool isSystem = false, bool isActive = true)
		{
			var tagDef = factory.New<TagDefinition>();
			tagDef.TGD_Code = code;
			tagDef.TGD_Description = description;
			tagDef.TGD_IsExclusive = isExclusive;
			tagDef.TGD_UsageScope = usageScope ?? TagUsageScopeList.Codes.All;
			tagDef.TGD_Scope = scope ?? TagScopeList.Codes.All;
			tagDef.TGD_IsSystem = isSystem;
			tagDef.TGD_IsActive = isActive;

			return tagDef;
		}

		public static TagMagnitude CreateTagMagnitude(TagDefinition definition, string code, string description = "", int ruleRunSequence = 0, Color? color = null, int nudge = 0, int visualPriority = 0, VisualBoardButtonBorderStyle? borderStyle = null, bool isActive = true)
		{
			var magnitude = definition.Magnitudes.AddNew();
			magnitude.TGM_Code = code;
			magnitude.TGM_Description = description;
			magnitude.TGM_RuleRunSequence = ruleRunSequence;
			magnitude.TGM_NudgeAmount = nudge;
			magnitude.VisualStylePriority = visualPriority;
			magnitude.TGM_IsActive = isActive;

			if (color != null)
			{
				magnitude.Color = ColorList.NameFromColor(color.Value);
			}

			if (borderStyle.HasValue)
			{
				magnitude.BorderStyle = borderStyle.ToString();
			}

			return magnitude;
		}

		public static TagRule CreateTagRule(TagMagnitude tag, string name, string actionType, decimal templateMagnitude = 0m, string templateDescription = "")
		{
			var rule = tag.Factory.New<TagRule>();
			rule.TGR_IsActive = true;
			rule.TGR_Name = name;

			using (rule.GetValidationSuspender()) // We suspend validation here to prevent premature validation of ActionType in TagLinkTemplate.cs
			{
				rule.TGR_ActionType = actionType;
				var template = rule.TagTemplate;
				template.TGL_TGM_Magnitude = tag.PK;
				template.TGL_Magnitude = templateMagnitude;
				template.TGL_Description = templateDescription;

				var filter = rule.Filter; // Ensures the filter is lazily populated.
			}

			rule.Schedule.Recurrence.TaskPeriod = ScheduleRecurrenceType.Second;

			return rule;
		}

		public static TagRule CreateTagRuleWithDefAndMag(BusinessObjectFactory factory, string name = "The Tag Rule", string actionType = TagRuleActionTypeList.Codes.AddTag)
		{
			return CreateTagRuleWithAccessibleDefAndMag(factory, name: name, actionType: actionType).Rule;
		}

		public static TagRule CreateTagRuleWithDefAndMagAndTemplate(BusinessObjectFactory factory, string ruleName = "JamesRule", string actionType = TagRuleActionTypeList.Codes.AddTag)
		{
			var tagPackage = CreateTagRuleWithAccessibleDefAndMag(factory, name: ruleName, actionType: actionType);
			var template = tagPackage.Rule.TagTemplate;
			template.TGL_TGM_Magnitude = tagPackage.Mag.PK;
			return tagPackage.Rule;
		}

		static (TagDefinition Def, TagMagnitude Mag, TagRule Rule) CreateTagRuleWithAccessibleDefAndMag(BusinessObjectFactory factory, string name = "The Tag Rule", string actionType = TagRuleActionTypeList.Codes.AddTag)
		{
			var definition = CreateTagDefinition(factory, "AAA", "The AAA Tags");
			var magnitude = CreateTagMagnitude(definition, "BBB", "BBB Tag");
			return (definition, magnitude, CreateTagRule(magnitude, name, actionType));
		}

		public static RuleRunnerTagLink CreateRuleRunnerTagLink(TagMagnitude tag, ProcessHeader workflow)
		{
			var factory = new BusinessObjectFactory();
			factory.ServiceContainer.AddService(new ServiceTaskCodeService(TagServiceTask.Code));

			var tagLink = factory.New<RuleRunnerTagLink>();
			tagLink.TGL_ParentId = workflow.PK;
			tagLink.TGL_ParentTableCode = workflow.TablePrefix;
			tagLink.TGL_TGM_Magnitude = tag.PK;
			factory.Save();

			return tagLink;
		}

		public static BMControlCustomisation CreateControlCustomisation(BusinessObjectFactory factory, string type = CustomisedControlTypeList.Codes.DetailedCard, int width = 100, int height = 100, string backgroundColor = "Hot Pink", string name = null)
		{
			var customisation = factory.New<BMControlCustomisation>();
			customisation.FM_ControlType = type;
			customisation.Width = width;
			customisation.Height = height;
			customisation.BackgroundColor = backgroundColor;

			if (!string.IsNullOrEmpty(name))
			{
				customisation.FM_Name = name;
			}

			return customisation;
		}

		public static StaticControlCustomisation CreateStaticControlCustomisation(BMControlCustomisation customisation, string type, string label, int left, int top, int width, int height, string backColor, string foreColor, int fontSize, bool isBold, bool readOnly, string alignment = ControlAlignmentList.Codes.Left)
		{
			var customisedControl = customisation.CustomisedControls.AddNew();
			customisedControl.ControlType = type;
			customisedControl.Label = label;
			customisedControl.Left = left;
			customisedControl.Top = top;
			if (width > 0)
			{
				customisedControl.Width = width;
			}
			if (height > 0)
			{
				customisedControl.Height = height;
			}
			if (!string.IsNullOrEmpty(backColor))
			{
				customisedControl.BackgroundColor = backColor;
			}
			if (!string.IsNullOrEmpty(foreColor))
			{
				customisedControl.ForegroundColor = foreColor;
			}
			customisedControl.FontSize = fontSize;
			customisedControl.IsBold = isBold;
			customisedControl.IsReadOnly = readOnly;
			customisedControl.Alignment = alignment;

			return customisedControl;
		}

		public static BMControlCustomisationLine CreateLine(BMControlCustomisation customisation, string source, string property, string type)
		{
			var line = customisation.CustomisationLines.AddNew();
			line.PropertySource = source;
			line.PropertyName = property;
			line.ControlType = type;
			return line;
		}

		public static BMControlCustomisationLine CreateLine(BMControlCustomisation customisation, string source, string property, string type, string label, int left, int top, int width, int height, string backColor, string foreColor, int fontSize, bool isBold, bool readOnly, bool autoSize, string alignment = ControlAlignmentList.Codes.Left)
		{
			var line = CreateLine(customisation, source, property, type);
			line.Label = label;
			line.Left = left;
			line.Top = top;
			if (width > 0)
			{
				line.Width = width;
			}
			if (height > 0)
			{
				line.Height = height;
			}
			if (!string.IsNullOrEmpty(backColor))
			{
				line.BackgroundColor = backColor;
			}
			if (!string.IsNullOrEmpty(foreColor))
			{
				line.ForegroundColor = foreColor;
			}
			line.FontSize = fontSize;
			line.IsBold = isBold;
			line.IsReadOnly = readOnly;
			line.AutoSize = autoSize;
			line.Alignment = alignment;

			return line;
		}

		static void CreateSectionAndViewModelValidationCheck(string componentType, int subsections, string channelBy, params BusinessObject[] channels)
		{
			if (componentType == BMComponentTypeList.Codes.Buffer && subsections > 1 && channels.Any())
			{
				throw new InvalidOperationException("Buffers can not be wrapped sections and have channels");
			}
		}

		public static Tuple<BMBoardSection, BMBoardSectionViewModel> CreateSectionAndViewModel(BMComponent component, int subsections, int cellsPerSubsection, string flowDirection, string lastCell, string channelBy, bool? showZones, ZGuid releaseGroup, params BusinessObject[] channels)
		{
			return CreateSectionAndViewModelCore(component, subsections, cellsPerSubsection, flowDirection, lastCell, channelBy, showZones, releaseGroup, channels);
		}

		public static Tuple<BMBoardSection, BMBoardSectionViewModel> CreateSectionAndViewModel(BMComponent component, int subsections = 1, int cellsPerSubsection = 1, string flowDirection = FlowDirectionList.Codes.Up, string lastCell = LastCellList.Codes.Top, string channelBy = ChannelTypeList.Codes.NotChanneled, params BusinessObject[] channels)
		{
			return CreateSectionAndViewModelCore(component, subsections, cellsPerSubsection, flowDirection, lastCell, channelBy, null, default(ZGuid), channels);
		}

		public static Tuple<BMBoardSection, BMBoardSectionViewModel> CreateSectionAndViewModel(BMSystem system, string componentType, int subsections, int cellsPerSubsection, string flowDirection, string lastCell, string channelBy, params BusinessObject[] channels)
		{
			CreateSectionAndViewModelValidationCheck(componentType, subsections, channelBy, channels);
			var component = system.Components.AddNew();
			component.FC_Name = component.FC_Type = componentType;

			if (component.FC_Type == BMComponentTypeList.Codes.Buffer)
			{
				component.FC_BufferTimespanInMinutes = 96 * 60;
			}

			return CreateSectionAndViewModelCore(component, subsections, cellsPerSubsection, flowDirection, lastCell, channelBy, null, default(ZGuid), channels);
		}

		public static Tuple<BMBoardSection, BMBoardSectionViewModel> CreateSectionAndViewModel(BMSystem system)
		{
			var component = system.Components.AddNew();
			component.FC_Name = component.FC_Type = BMComponentTypeList.Codes.Buffer;
			component.FC_BufferTimespanInMinutes = 96 * 60;

			return CreateSectionAndViewModelCore(component, 1, 13, FlowDirectionList.Codes.Left, LastCellList.Codes.Bottom, ChannelTypeList.Codes.Resource, false, default(ZGuid), GlbStaff.CurrentUser, system.Factory.NewWithValidTestData<GlbStaff>());
		}

		public static Tuple<BMBoardSection, BMBoardSectionViewModel> CreateSectionAndViewModel(BMSystem system, string componentType, int subsections, int cellsPerSubsection, string flowDirection, string lastCell, string channelBy, bool? showZones = null, params BusinessObject[] channels)
		{
			var component = system.Components.AddNew();
			component.FC_Name = component.FC_Type = componentType;
			if (component.FC_Type == BMComponentTypeList.Codes.Buffer)
			{
				component.FC_BufferTimespanInMinutes = 96 * 60;
			}

			return CreateSectionAndViewModelCore(component, subsections, cellsPerSubsection, flowDirection, lastCell, channelBy, showZones, default(ZGuid), channels);
		}

		public static Tuple<BMBoardSection, BMBoardSectionViewModel> CreateSectionAndViewModel(BMComponent component, int subsections, int cellsPerSubsection, string flowDirection, string lastCell, string channelBy, bool? showZones = null, params BusinessObject[] channels)
		{
			return CreateSectionAndViewModelCore(component, subsections, cellsPerSubsection, flowDirection, lastCell, channelBy, showZones, default(ZGuid), channels);
		}

		static Tuple<BMBoardSection, BMBoardSectionViewModel> CreateSectionAndViewModelCore(BMComponent component, int subsections, int cellsPerSubsection, string flowDirection, string lastCell, string channelBy, bool? showZones, ZGuid releaseGroup, params BusinessObject[] channels)
		{
			CreateSectionAndViewModelValidationCheck(component.FC_Type, subsections, channelBy, channels);

			var board = component.System.Boards.AddNew();
			board.MB_Name = component.PK.ToString() + component.System.Boards.Count;
			var section = board.Sections.AddNew();
			var sectionConfiguration = section.SectionConfiguration;
			section.MS_FC_Component = component.PK;
			if (releaseGroup != ZGuid.Empty)
			{
				section.MS_GG_ReleaseGroup = releaseGroup;
				sectionConfiguration.ReleaseGroupPK = releaseGroup;
			}
			sectionConfiguration.Subsections = subsections;
			sectionConfiguration.CellsPerSubsection = cellsPerSubsection;
			sectionConfiguration.FlowDirection = flowDirection;
			sectionConfiguration.LastCell = lastCell;
			sectionConfiguration.ChannelBy = channelBy;

			sectionConfiguration.ShowZones = showZones ?? component.FC_Type == BMComponentTypeList.Codes.Buffer;

			component.Factory.Save();

			var boardViewModel = CreateBoardViewModel(board);

			foreach (var bizo in channels)
			{
				sectionConfiguration.OverrideChannels = true;
				var channelType = ZString.Empty;

				if (bizo is GlbStaff)
				{
					channelType = ChannelTypeList.Codes.Resource;
				}
				else if (bizo is GlbCapability)
				{
					channelType = ChannelTypeList.Codes.Capability;
				}
				else if (bizo is GlbGroup)
				{
					channelType = ChannelTypeList.Codes.Group;
				}
				CreatePrimaryChannelForSection(section, channelType, bizo.PK);
			}

			return Tuple.Create(section, CreateViewModel(section, boardViewModel));
		}

		public static BMBoardSectionViewModel CreateViewModel(BMBoardSection section, bool isPreview = false, BoardFactoryProvider factoryProvider = null, bool populatePropertyCache = false, bool useStatusCache = true)
		{
			return CreateViewModel(section, CreateBoardViewModel(section.Board, factoryProvider, useStatusCache, isPreview), populatePropertyCache);
		}

		public static BMBoardSectionViewModel CreateViewModel(BMBoardSection section, BoardViewModel boardViewModel, bool populatePropertyCache = false)
		{
			var sectionViewModel = CreateViewModelCore(section, boardViewModel);

			if (populatePropertyCache)
			{
				CreateAndPopulateBoardSectionPropertyCacheWithEmptyTasks(sectionViewModel, section);
			}

			return sectionViewModel;
		}

		public static BMBoardSectionViewModel CreateViewModel(BMBoardSection section, params ProcessHeader[] workflows)
		{
			var viewModel = CreateViewModelCore(section, CreateBoardViewModel(section.Board, factoryProvider: null, useStatusCache: true));

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				BMBoardSectionViewModel.CreateAndPopulatePropertyCache(
					TaskChannelMap.ForTest(section, viewModel, workflows),
						viewModel);

				strategy.AwaitAll(taskToIgnore: null);
			}

			return viewModel;
		}

		static BMBoardSectionViewModel CreateViewModelCore(BMBoardSection section, BoardViewModel boardViewModel)
		{
			var sectionViewModel = new BMBoardSectionViewModel(section, boardViewModel);
			((ICollection<BoardSectionViewModel>)boardViewModel.GetSections()).Add(sectionViewModel);

			return sectionViewModel;
		}

		public static BMBoardSectionViewModel CreateDummyViewModel(BusinessObjectFactory factory)
		{
			var system = VisualBoardsTestHelper.CreateSystem(factory);
			var section = CreateBoardSection(CreateBucket(system));

			return CreateViewModel(section);
		}

		public static OrgHeader CreateValidOrgHeader(BusinessObjectFactory factory)
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			FillWithValidTestDataSoFormSaveWorks(org);

			return org;
		}

		public static void FillWithValidTestDataSoFormSaveWorks(OrgHeader org)
		{
			MasterFilesTestHelper.FillWithValidTestDataSoFormSaveWorks(org);
		}

		public static GlbStaffHoliday CreateStaffHoliday(BusinessObjectFactory factory, ZDateTime start, ZDateTime end, ZGuid staffPK, string recordType = "LEV", string workHolidayType = "ANN", decimal leaveDaysTaken = 1.0m, byte availabilityFactor = 0, string comment = "")
		{
			var holiday = factory.NewWithValidTestData<GlbStaffHoliday>();
			holiday.GA_StartTime = start;
			holiday.GA_EndTime = end;
			holiday.GA_GS = staffPK;
			holiday.GA_AvailabilityPercentage = availabilityFactor;
			holiday.GA_DaysLeaveTaken = leaveDaysTaken;
			holiday.GA_ApprovalStatus = GlbStaffHolidayLookupsReal.Approved;
			holiday.GA_RecordType = recordType;
			holiday.GA_WorkHolidayType = workHolidayType;
			holiday.GA_LeaveComment = comment;

			return holiday;
		}

		public static void AssertAcceptabilityBandSubheadingDetails(BMBoardSectionViewModel viewModel, BMBoardSection section, string message, string expectedSubHeading, string expectedSubHeadingMouseoverText, Color expectedSubheadingBackColor)
		{
			CreateAndPopulateBoardSectionPropertyCacheWithEmptyTasks(viewModel, section);
			var results = viewModel.GetAcceptabilityBandResults(section.Factory);
			viewModel.RefreshAcceptabilityBandSubHeading(results);
			AssertAcceptabilityBandSubheadingDetails(viewModel.SubHeadingAppearance, message, expectedSubHeading, expectedSubHeadingMouseoverText, expectedSubheadingBackColor);
		}

		public static void CreateAndPopulateBoardSectionPropertyCacheWithEmptyTasks(BMBoardSectionViewModel viewModel, BMBoardSection section)
		{
			var dataSource = new BoardSectionDataSource(section, viewModel);
			var map = dataSource.GetIncompleteTasksForCurrentChannels();

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				BMBoardSectionViewModel.CreateAndPopulatePropertyCache(map, viewModel);

				strategy.AwaitAll(taskToIgnore: null);
			}
		}

		public static void AssertAcceptabilityBandSubheadingDetails(ISectionSubHeadingAppearance viewModel, string message, string expectedSubHeading, string expectedSubHeadingMouseoverText, Color expectedSubheadingBackColor)
		{
			Assertion.CombineAssertions(message, () =>
			{
				Assertion.AssertEquals("SectionSubHeading", expectedSubHeading, viewModel.SectionSubHeading);
				Assertion.AssertMultilineASCIIEquals("SectionSubHeadingMouseOverText", expectedSubHeadingMouseoverText, viewModel.SectionSubHeadingDetailText);
				Assertion.AssertColorEquals("SectionHeadingBackgroundColor", expectedSubheadingBackColor, viewModel.SectionHeadingBackgroundColor);
			});
		}

		const string NumberOfWorkflowsInComponentWithReleaseGroupSql = @"
			select count(FH_PK) ""Value"", FC_PK ""Component"", FH_GG_ReleaseGroup ""ReleaseGroup""
			from dbo.BMComponent
			left join dbo.ProcessHeader on FH_FC_CurrentComponent = FC_PK
			group by FC_PK, FH_GG_ReleaseGroup";

		const string NumberOfWorkflowsInComponentWithoutReleaseGroupSql = @"
			select count(FH_PK) ""Value"", FC_PK ""Component"", null ""ReleaseGroup""
			from dbo.BMComponent
			left join dbo.ProcessHeader on FH_FC_CurrentComponent = FC_PK
			group by FC_PK";

		const string AverageNumberOfTasksPerWorkflowSql = @"
			select avg(Count) ""Value"", FC_PK ""Component"", FH_GG_ReleaseGroup ""ReleaseGroup""
			from (
				select count(P9_PK) Count, FH_PK Workflow
				from dbo.ProcessHeader
				left join dbo.ProcessTasks on P9_FH_ProcessHeader = FH_PK
				group by FH_PK
			) x
			join dbo.ProcessHeader on Workflow = FH_PK
			join dbo.BMComponent on FH_FC_CurrentComponent = FC_PK
			group by FC_PK, FH_GG_ReleaseGroup";

		const string NullResultSql = @"select null Value, null Component, null ReleaseGroup";

		#endregion

		#region Other Helpers

		public static string[] GetActiveFactoryNames()
		{
			var stats = new PerformanceStatistic();
			stats.Load();

			return stats.FactoryStatistics.Cast<BusinessObjectFactoryStatistic>().Select(s => string.IsNullOrEmpty(s.Name) ? "<Unknown>" : s.Name.ToString()).OrderBy(s => s).ToArray();
		}

		public static int GetActiveFactoryCount()
		{
			return GetActiveFactoryNames().Length;
		}

		public static BusinessObjectFactory GetActiveFactory(string nameForDebugging)
		{
			var stats = new PerformanceStatistic();
			stats.Load();

			return stats.FactoryStatistics
				.Cast<BusinessObjectFactoryStatistic>()
				.Select(s => s.FactoryInternals)
				.Cast<BusinessObjectFactory>()
				.Where(f => f != null)
				.FirstOrDefault(f => f.NameForDebugging.Contains(nameForDebugging));
		}

		public static BoardSlideshowViewModel CreateSlideshowViewModel(IVisualBoardProvider slideshowOrBoard, BoardFactoryProvider factoryProvider = null, bool useStatusCache = true, bool isPreview = false)
		{
			return CreateSlideshowViewModel(slideshowOrBoard, null, factoryProvider, useStatusCache, isPreview);
		}

		public static BoardSlideshowViewModel CreateSlideshowViewModel(IVisualBoardProvider slideshowOrBoard, IDispatcher dispatcher, BoardFactoryProvider factoryProvider = null, bool useStatusCache = true, bool isPreview = false)
		{
			var viewModel = new BoardSlideshowViewModel(slideshowOrBoard, null, factoryProvider, isPreview) { Dispatcher = dispatcher };

			if (dispatcher == null && AsyncStrategy.Default is MockAsyncStrategy)
			{
				// If we have disabled background threads, it's OK to use the main form's thread to execute actions normally dispatched from a background thread to the visual board's thread.
				viewModel.Dispatcher = new MainFormDispatcher();
			}

			if (!useStatusCache)
			{
				viewModel.Services_ExposedForTest.RemoveWhere(service => service is RoadRunnerStatusCacheService);
			}

			return viewModel;
		}

		public static BoardViewModel CreateBoardViewModel(IBMBoard board, BoardFactoryProvider factoryProvider = null, bool useStatusCache = true, bool isPreview = false)
		{
			return new BoardViewModel(board, CreateSlideshowViewModel(board, factoryProvider, useStatusCache, isPreview));
		}

		public static BoardViewModel CreateBoardViewModel(IBMBoard board, BoardSlideshowViewModel slideShowViewModel)
		{
			return new BoardViewModel(board, slideShowViewModel);
		}

		#endregion

		public static string GetGridAsHTML(BMBoardSectionViewModel viewModel)
		{
			var grid = viewModel.ComponentGrid;

			var debugFactory = new BusinessObjectFactory();
			var section = debugFactory.Load<BMBoardSection>(viewModel.SectionPK);

			var gridContent = grid.Cells;

			var builder = new StringBuilder();

			builder.Append("<table border='1'>");

			var sorted = gridContent.OrderBy(c => c.Row).ThenBy(c => c.Column);

			string row1 = new string(Enumerable.Range(sorted.Min(c => c.Column), sorted.Max(c => c.Column) + 1).SelectMany(x => string.Format("<td>{0}</td>", x.ToString())).ToArray());
			builder.Append(string.Format("<tr><td>row\\column</td>{0}</tr>", row1));
			builder.AppendLine();

			foreach (var groupByRow in sorted.GroupBy(c => c.Row))
			{
				builder.Append("<tr>");
				builder.Append(string.Format("<td>{0}</td>", groupByRow.Key));

				var allocationMap = grid.CardAllocationMap;

				foreach (var cell in groupByRow)
				{
					builder.Append(string.Format("<td>{0}</td>", GetSymbol(section, cell, allocationMap)));
				}
				builder.Append("</tr>");
				builder.AppendLine();
			}

			builder.Append("</table>");

			return builder.ToString();
		}

		static string GetSymbol(BMBoardSection section, CellContent content, CardAllocationMap map)
		{
			map.CardsByCell_ForTest.TryGetValue(content, out var cards);

			var contentType = content.ContentType;
			switch (contentType)
			{
				case CellContentType.AgeHeading:
					return "AgeHead";
				case CellContentType.CCRHeading:
					var ccrHeadingZone = content.CCRHeaderZone != null ? content.CCRHeaderZone.ToString() : string.Empty;
					return "CCRHead-Zone" + ccrHeadingZone;
				case CellContentType.ChannelHeading:
					return "ChnlHead-" + content.Channel.GetChannelName(DisplayNameType.FullName);
				case CellContentType.Label:
					return "Label";
				case CellContentType.SubComponentZoneHeading:
					return "Cons-Zone" + (content.SubComponentZones.Any() ? content.SubComponentZones.First(z => z.Key.In(content.SubComponentHeadingPK)).Value.ToString() : string.Empty);
				case CellContentType.ZoneHeading:
					var zoneHeading = content.Zone != null ? content.Zone.ToString() : string.Empty;
					return "ZoneHead" + zoneHeading;
				case CellContentType.Cards:
					var tasksAsString = new StringBuilder();
					if (cards != null)
					{
						cards
							.Select(c => GetDebugInfo(section, c))
							.ForEach(t => tasksAsString.Append(t + "<br />"));
					}
					return "Card<br/>" + tasksAsString.ToString();
				default:
					return "?";
			}
		}

		static string GetDebugInfo(BMBoardSection section, ICardContent card)
		{
			var task = card.GetTask(section.Factory);
			var workflow = card.GetWorkflow(section.Factory);
			var relevantEstimateHours = task.RelevantEstimateHours;

			var releaseDateMinusToday = workflow != null && workflow.FH_ReleaseDateTime.IsValid
				? ZDateTime.Today.ToDateTime().Subtract(workflow.FH_ReleaseDateTime.ToDateTime())
				: TimeSpan.Zero;

			var penetratedComponentPKs = task.GetProcessHeader().GetPenetratedComponents(task).Select(c => c.PK).ToArray();
			var penetratedComponentNames = new StringBuilder();
			var penetratedComponents = section.Factory.Load<BMComponent>(new ZQuery(BMComponentSchema.PK, penetratedComponentPKs));

			if (penetratedComponents != null)
			{
				foreach (var component in penetratedComponents)
				{
					penetratedComponentNames.Append(component.FC_Name);
				}
			}

			return string.Format(CultureInfo.CurrentCulture, "flow={0}, task={1}, workflow={2}, Est={3}hrs, ReleaseDate-Today={4}days, penetrated={5}", // for debug
				section.SectionConfiguration.FlowDirection,
				task.P9_Description,
				workflow != null ? workflow.FH_CompletionStatement : ZString.Empty,
				relevantEstimateHours.ToString("#.##", CultureInfo.CurrentCulture),
				releaseDateMinusToday.Days,
				penetratedComponentNames.ToString());
		}

		public static IDisposable UseBizoCardContents()
		{
			CardAllocationMap.UseBizoCardContents.Value = true;
			return new DisposableAction(CardAllocationMap.UseBizoCardContents.ResetValue);
		}
	}

	[DebuggerDisplay("PrimaryAxis = {PrimaryAxis}, SecondaryAxis = {SecondaryAxis}, Value = {Value}")]
	public class VisualBoardPositionForTest<T>
	{
		public int PrimaryAxis { get; private set; }
		public int SecondaryAxis { get; private set; }
		public T Value { get; set; }
		string flowDirection = FlowDirectionList.Codes.Down;

		public VisualBoardPositionForTest(int primaryAxis, int secondaryAxis)
		{
			PrimaryAxis = primaryAxis;
			SecondaryAxis = secondaryAxis;
		}

		public VisualBoardPositionForTest(int primaryAxis, int secondaryAxis, T value)
		{
			PrimaryAxis = primaryAxis;
			SecondaryAxis = secondaryAxis;
			Value = value;
		}

		public static string Log(BMBoardSection section, IEnumerable<VisualBoardPositionForTest<T>> visualBoardPositions)
		{
			var logs = new List<string>();

			foreach (var visualBoardPosition in visualBoardPositions.OrderBy(t => t.PrimaryAxis).GroupBy(t => t.PrimaryAxis, t => t))
			{
				var logLine = Enumerable.Range(0, section.SectionConfiguration.CellsPerSubsection + 1)
					.Select(i => visualBoardPosition.FirstOrDefault(l => l.PrimaryAxis == visualBoardPosition.Key && l.SecondaryAxis == i))
					.Select(i => i != null ? i.Value.ToString() : "X")
					.ToArray();

				logs.Add(string.Join(", ", logLine));
			}
			return string.Join("\r\n", logs.ToArray());
		}

		public void ConvertToFlowDirectioned(BMBoardSection section, string defaultFlowdirection = FlowDirectionList.Codes.Down)
		{
			var newflowDirection = section.SectionConfiguration.FlowDirection.ToString();

			if (flowDirection != newflowDirection)
			{
				SecondaryAxis = defaultFlowdirection == FlowDirectionList.Codes.Down && newflowDirection.In(FlowDirectionList.Codes.Up, FlowDirectionList.Codes.Left)
						? section.SectionConfiguration.CellsPerSubsection - SecondaryAxis + 1
						: SecondaryAxis;

				flowDirection = newflowDirection;
			}
		}

		public static void AssertCollection(string message, IEnumerable<VisualBoardPositionForTest<T>> expecteds, IEnumerable<VisualBoardPositionForTest<T>> actuals, int totalTabs = 0)
		{
			var tabs = new string(' ', totalTabs * 4);

			var sortedExpecteds = expecteds.OrderBy(c => c.PrimaryAxis).ThenBy(c => c.SecondaryAxis);
			var sortedActuals = actuals.OrderBy(c => c.PrimaryAxis).ThenBy(c => c.SecondaryAxis);

			Assertion.AssertEquals("Expected length and Actual length should be the same", expecteds.Count(), actuals.Count());

			var expectedStrings = new ZStringBuilder();
			foreach (var grouped in sortedExpecteds.GroupBy(c => c.PrimaryAxis))
			{
				var valueAsString = string.Join(", ", grouped.Select(c => string.Format("{0}[{1}]", c.SecondaryAxis, c.Value.ToString())));
				expectedStrings.Append(string.Format("\r\n{0}PrimaryAxis {1}: {2}", tabs, grouped.Key, valueAsString));
			}

			var actualStrings = new ZStringBuilder();
			foreach (var grouped in sortedActuals.GroupBy(c => c.PrimaryAxis))
			{
				var valueAsString = string.Join(", ", grouped.Select(c => string.Format("{0}[{1}]", c.SecondaryAxis, c.Value.ToString())));
				actualStrings.Append(string.Format("\r\n{0}PrimaryAxis {1}: {2}", tabs, grouped.Key, valueAsString));
			}

			var assertMessage = string.Format("{0}{1}.\r\n{0} * Expected:{2}\r\n{0}* But found: {3}",
				tabs,
				message,
				expectedStrings.ToString(),
				actualStrings.ToString());

			var expectedsAsArray = sortedExpecteds.ToArray();
			var actualsAsArray = sortedActuals.ToArray();

			var comparer = new VisualBoardPositionTestComparer<T>();

			for (var index = 0; index < expectedsAsArray.Length; index++)
			{
				if (!comparer.Equals(expectedsAsArray[index], actualsAsArray[index]))
				{
					Assertion.Assert(assertMessage, false);
					break;
				}
			}
		}
	}

	public class VisualBoardPositionTestComparer<T> : IEqualityComparer<VisualBoardPositionForTest<T>>
	{
		public int GetHashCode(VisualBoardPositionForTest<T> type)
		{
			return type.GetHashCode();
		}

		public bool Equals(VisualBoardPositionForTest<T> first, VisualBoardPositionForTest<T> second)
		{
			return first.PrimaryAxis == second.PrimaryAxis && first.SecondaryAxis == second.SecondaryAxis
				&& (
					(first.Value == null && second.Value == null)
					|| (first.Value != null && second.Value != null && first.Value.Equals(second.Value))
				);
		}
	}

	public class TestFactoryProvider : SharedBoardFactoryProvider
	{
		public TestFactoryProvider(BusinessObjectFactory mainThreadFactory, BoardSlideshowViewModel viewModel)
			: base(viewModel)
		{
			this.mainThreadFactory = mainThreadFactory;
		}

		public override BusinessObjectFactory GetBoardGUIThreadFactory()
		{
			return mainThreadFactory;
		}

		readonly BusinessObjectFactory mainThreadFactory;
	}
}

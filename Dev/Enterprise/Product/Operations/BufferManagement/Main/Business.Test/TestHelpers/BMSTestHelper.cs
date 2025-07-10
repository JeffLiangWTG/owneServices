using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Application.Exceptions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
namespace Enterprise.BufferManagement.Business.Test
{
	public class BMSTestHelper : VisualBoardsTestHelper, IBMTestHelper
	{
		#region IBMTestHelper Members

		IBMSystem IBMTestHelper.CreateSystem(BusinessObjectFactory factory, params string[] workflowTypes)
		{
			return VisualBoardsTestHelper.CreateSystem(factory, workflowTypes);
		}

		IBMSystem IBMTestHelper.CreateSystemAndRelatedWorkflowType(BusinessObjectFactory factory, string workflowType, bool isActive)
		{
			return VisualBoardsTestHelper.CreateSystemAndRelatedWorkflowType(factory, workflowType, isActive);
		}

		void IBMTestHelper.MarkAsReleaseGroupWithinSystem(IBMSystem system, IGlbGroup group) => CreateReleaseGroup((BMSystem)system, (GlbGroup)group);

		IProcessJobHeader IBMTestHelper.CreateJobHeader<T>(BusinessObjectFactory factory, bool addDefaultProcessHeaderIfNone)
		{
			return ProcessJobHeader.GetForParent(CreateJob<T>(factory), factory, addDefaultProcessHeaderIfNone);
		}

		IProcessJobHeader IBMTestHelper.GetJobHeaderForParent(IWorkflowProviderCore parent, BusinessObjectFactory factory, bool addDefaultProcessHeaderIfNone)
		{
			return GetJobHeaderForParent((IWorkflowProvider)parent, factory, addDefaultProcessHeaderIfNone);
		}

		IProcessHeader IBMTestHelper.CreateWorkflow(IProcessJobHeader jobHeader, string name, Guid? releaseGroupPK, DateTime? penetrationResetDateTime)
		{
			var workflow = CreateWorkflow((ProcessJobHeader)jobHeader, name, releaseGroupPK: releaseGroupPK);
			if (penetrationResetDateTime != null)
			{
				workflow.FH_TaskPenetrationResetDateTimeUtc = penetrationResetDateTime.Value;
			}
			return workflow;
		}

		IProcessHeader IBMTestHelper.CreateWorkflow(IProcessTaskTemplate template, string description)
		{
			return CreateWorkflow((ProcessTaskTemplate)template, description);
		}

		IProcessHeader IBMTestHelper.CreateWorkflow(BusinessObjectFactory factory, string completionStatement, IBMComponent currentComponent, DateTime? releaseDateTime, Guid? releaseGroupPK, bool autoAssignTasks)
		{
			return BMSTestHelper.CreateWorkflow(factory, completionStatement, currentComponent as BMComponent, releaseDateTime, releaseGroupPK, autoAssignTasks);
		}

		IProcessHeaderLink IBMTestHelper.CreateDependencyLink(IProcessTaskTemplate template, IProcessHeader headerFrom, IProcessHeader headerTo)
		{
			return CreateDependencyLink((ProcessTaskTemplate)template, (ProcessHeader)headerFrom, (ProcessHeader)headerTo);
		}

		IProcessHeaderLink IBMTestHelper.CreateParentChildLink(IProcessTaskTemplate template, IProcessHeader headerFrom, IProcessHeader headerTo)
		{
			return CreateParentChildLink((ProcessTaskTemplate)template, (ProcessHeader)headerFrom, (ProcessHeader)headerTo);
		}

		IProcessHeaderLink IBMTestHelper.CreateLink(IProcessHeader headerFrom, IProcessHeader headerTo, string linkType)
		{
			var link = headerFrom.Factory.New<ProcessHeaderLink>();
			link.FP_FH_HeaderFrom = headerFrom.PK;
			link.FP_FH_HeaderTo = headerTo.PK;
			link.FP_LinkType = linkType;

			return link;
		}

		#region ProcessTask

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		IProcessTask IBMTestHelper.CreateTask(IProcessHeader workflow, string staffCode, int lowEstMinutes, string taskType, string taskStatus, int? sequence, string description, bool createStaffIfNotExist, IGlbCapability capability, IGlbGroup group)
		{
			if (createStaffIfNotExist && !string.IsNullOrEmpty(staffCode))
			{
				GetOrCreateStaff(workflow.Factory, staffCode);
			}

			return CreateTask((ProcessHeader)workflow, staffCode, lowEstMinutes, taskType, taskStatus, sequence: sequence, description: description, capability: (GlbCapability)capability, taskGroupPK: group?.PK);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		IProcessTask IBMTestHelper.CreateTaskWithCompany(IProcessHeader workflow, string staffCode, int lowEstMinutes, string taskType, string taskStatus, int? sequence, string description, IGlbCompany company)
		{
			return CreateTaskWithCompany((ProcessHeader)workflow, staffCode, lowEstMinutes, taskType, taskStatus, sequence: sequence, description: description, company: company);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		IProcessTask IBMTestHelper.CreateTask(BusinessObject job, string staffCode, int lowEstMinutes, string taskType, string taskStatus, int? sequence, string description)
		{
			return CreateTask((IWorkflowProvider)job, staffCode, lowEstMinutes, taskType, taskStatus, sequence: sequence, description: description);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		IProcessTask IBMTestHelper.CreateTask(IProcessTaskTemplate template, IProcessHeader workflow, string resourceCode, int lowEstMinutes, string description, decimal estVariationFactor)
		{
			return CreateTask((ProcessTaskTemplate)template, (ProcessHeader)workflow, resourceCode, lowEstMinutes, description, estVariationFactor);
		}

		#endregion

		public static IWorkflowProvider CreateJob<T>(BusinessObjectFactory factory)
		{
			Type realType;
			try
			{
				realType = ObjectFactory.GetType<T>();
			}
			catch (NoSuchObjectDefinitionException)
			{
				realType = typeof(T);
			}

			var job = factory.New(realType);
			job.FillWithValidTestData();

			return (IWorkflowProvider)job;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		IProcessHeader IBMTestHelper.CreateWorkflowAndTask(BusinessObjectFactory factory, string completionStatement, IBMComponent currentComponent, DateTime? releaseDateTime, string staffCode, int lowEstMinutes, string taskType, int estVariationFactor, int? sequence, string description, string taskStatus, Guid? capabilityPK, bool createStaffIfNotExist)
		{
			var capability = capabilityPK != null
				? factory.Load<GlbCapability>(capabilityPK.Value)
				: null;

			if (createStaffIfNotExist && !string.IsNullOrEmpty(staffCode))
			{
				GetOrCreateStaff(factory, staffCode);
			}

			return BMSTestHelper.CreateWorkflowAndTask(factory, completionStatement, currentComponent as BMComponent, releaseDateTime, staffCode, lowEstMinutes, taskType, estVariationFactor, sequence, description, taskStatus, capability);
		}

		IProcessHeader IBMTestHelper.CreateQualityIteration(IProcessTask iterateFromTask, IProcessTask containmentBarrierTask, string iterationWorkflowDescription, string resourceUnderReviewStaffCode, string iterationReasonCode, bool shouldCreateWorkflowForIteration)
		{
			return CreateQualityIteration(iterateFromTask, containmentBarrierTask, iterationWorkflowDescription, resourceUnderReviewStaffCode, iterationReasonCode, shouldCreateWorkflowForIteration);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		IBMComponent IBMTestHelper.CreateBucket(IBMSystem system, string name, int sequence, int offsetMinutes)
		{
			return CreateBucket((BMSystem)system, name, offsetMinutes: offsetMinutes, sequence: sequence);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		IBMComponent IBMTestHelper.CreateBuffer(IBMSystem system, string name, int timespanMinutes, byte loadLimitPercent, int sequence)
		{
			return CreateBuffer((BMSystem)system, name, timespanMinutes, loadLimitPercent, sequence);
		}

		IBMComponentLink IBMTestHelper.LinkComponents(IBMComponent from, IBMComponent to, byte sequence, bool? isReleaseGate)
		{
			return LinkComponents((BMComponent)from, (BMComponent)to, sequence, isReleaseGate);
		}

		ITagDefinition IBMTestHelper.CreateTagDefinition(BusinessObjectFactory factory, string code, string description, bool isExclusive, string usageScope, string scope, bool isActive)
		{
			return CreateTagDefinition(factory, code, description, isExclusive, usageScope, scope, isActive: isActive);
		}

		ITagMagnitude IBMTestHelper.CreateTagMagnitude(ITagDefinition definition, string code, string description, int nudge, bool isActive)
		{
			return CreateTagMagnitude((TagDefinition)definition, code, description, nudge: nudge, isActive: isActive);
		}

		IBMControlCustomisation IBMTestHelper.CreateControlCustomisation(BusinessObjectFactory factory, string type, int width, int height, string backgroundColor)
		{
			return CreateControlCustomisation(factory, type, width, height, backgroundColor);
		}

		IBMControlCustomisationLink IBMTestHelper.CreateControlCustomisationLink(BusinessObjectFactory factory, ICustomisedLayoutSupportable parent, IBMControlCustomisation controlLayout)
		{
			return CreateControlCustomisationLink(factory, parent, (BMControlCustomisation)controlLayout);
		}

		IBMControlCustomisationLink IBMTestHelper.CreateControlCustomisationLink(BusinessObjectFactory factory, IGlbGroup group, IBMControlCustomisation controlLayout)
		{
			return CreateControlCustomisationLink(factory, group, (BMControlCustomisation)controlLayout);
		}

		IProcessTaskTemplate IBMTestHelper.CreateWorkflowTemplate(BusinessObjectFactory factory, string processType, string subType1, string subType2, string subType3, string subType4, string subType5, string name, string description, bool isPartial, bool isUniversal)
		{
			return CreateWorkflowTemplate(factory, processType, subType1, subType2, subType3, subType4, subType5, name, description, isPartial, isUniversal);
		}

		IBMReleaseSequence IBMTestHelper.CreateReleaseSequence(BusinessObjectFactory factory, Guid releaseGroup, string name, bool isActive, Guid? capability, int? nudge)
		{
			return CreateReleaseSequence(factory, releaseGroup, name, isActive, capability, nudge);
		}

		IBMReleaseSequenceItem IBMTestHelper.CreateReleaseSequenceItem(IBMReleaseSequence sequence, IProcessHeader workflow, int position, int value, int investment, string note)
		{
			return CreateReleaseSequenceItem((BMReleaseSequence)sequence, (ProcessHeader)workflow, position, value, investment, note);
		}

		void IBMTestHelper.EnableBMSInRegistry()
		{
			EnableBMSInRegistry();
		}

		void IBMTestHelper.DisableBMSInRegistry()
		{
			DisableBMSInRegistry();
		}

		void IBMTestHelper.SetWorkflowManagementModeInRegistry(string mode)
		{
			SetWorkflowManagementModeInRegistry(mode);
		}

		void IBMTestHelper.SetAlwaysCreateWorkflowLinksBetweenJobsGeneratedAsTheResultOfApplyingPartialWorkflowTemplates(bool value)
		{
			WorkflowDataRegistry.Instance.AlwaysCreateWorkflowLinksBetweenJobsGeneratedAsTheResultOfApplyingPartialWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		void IBMTestHelper.AssertIsPrerequisite(IProcessHeader fromHeader, IProcessHeader toHeader)
		{
			BMSTestCaseWithFactory.AssertIsPrerequisite((ProcessHeader)fromHeader, (ProcessHeader)toHeader);
		}

		void IBMTestHelper.AssertIsNotPrerequisite(IProcessHeader fromHeader, IProcessHeader toHeader)
		{
			BMSTestCaseWithFactory.AssertIsNotPrerequisite((ProcessHeader)fromHeader, (ProcessHeader)toHeader);
		}

		void IBMTestHelper.AssertIsParent(IProcessHeader childHeader, IProcessHeader parentHeader)
		{
			BMSTestCaseWithFactory.AssertIsParent((ProcessHeader)childHeader, (ProcessHeader)parentHeader);
		}

		#region Performance

		IDisposable IBMTestHelper.TemporarilyDisableTableCachingInUberFactory(IReadOnlyCollection<string> tablesToDisableCaching) => TemporarilyDisableTableCachingInUberFactory(tablesToDisableCaching);

		#endregion

		#endregion

		#region Helper Methods

		public static void AddMultipleBucketsBuffersAndLinks(BMSystem system, int numberOfComponentsToAdd)
		{
			var previousComponent = system.Components.LastOrDefault();
			var componentsCount = system.Components.Count;

			for (int i = componentsCount; i < (componentsCount + numberOfComponentsToAdd); i++)
			{
				var currentComponent = (i % 6 == 0)
																? VisualBoardsTestHelper.CreateBuffer(system, $"{i} TestBuffer", 100, sequence: i)
																: VisualBoardsTestHelper.CreateBucket(system, $"{i} TestBucket", sequence: i);

				if (previousComponent != null)
				{
					VisualBoardsTestHelper.LinkComponents(previousComponent, currentComponent);
				}

				previousComponent = currentComponent;
			}
		}

		public static ProcessJobHeader CreateJobHeader(IWorkflowProvider job, bool addDefaultProcessHeaderIfNone = true, string description = null)
		{
			var jobHeader = ProcessJobHeader.GetForParent(job, ((BusinessObject)job).Factory, addDefaultProcessHeaderIfNone);

			if (jobHeader != null && !string.IsNullOrEmpty(description))
			{
				jobHeader.FH_CompletionStatement = description;
			}

			return jobHeader;
		}

		public static IProcessJobHeader GetJobHeaderForParent(IWorkflowProvider parent, BusinessObjectFactory factory, bool addDefaultProcessHeaderIfNone = true)
		{
			return ProcessJobHeader.GetForParent(parent, factory, addDefaultProcessHeaderIfNone);
		}

		public static TagLink CreateTagLink(ProcessHeader workflow, TagMagnitude tag = null)
		{
			var link = workflow.Factory.New<TagLink>();
			link.TGL_ParentId = workflow.PK;
			link.TGL_ParentTableCode = workflow.TablePrefix;

			if (tag != null)
			{
				link.TGL_TGM_Magnitude = tag.PK;
			}

			return link;
		}

		public static TagLink CreateTagLink(ProcessTask task, TagMagnitude tag = null)
		{
			var link = task.Factory.New<TagLink>();
			link.TGL_ParentId = task.PK;
			link.TGL_ParentTableCode = task.TablePrefix;

			if (tag != null)
			{
				link.TGL_TGM_Magnitude = tag.PK;
			}

			return link;
		}

		public static BoardSectionAcceptabilityBand AddAcceptabilityBandToSection(BMBoardSection section, BMComponentAcceptabilityBand band, AcceptabilityBandShowOnOption showOn = AcceptabilityBandShowOnOption.Tile, string displayName = null)
		{
			var sectionBand = section.SectionConfiguration.AcceptabilityBands.AddNew();
			sectionBand.AcceptabilityBandPK = band.PK;
			sectionBand.SelectedShowOnOption = showOn;

			if (displayName != null)
			{
				sectionBand.DisplayName = displayName;
			}

			return sectionBand;
		}

		public static WorkQueue CreateWorkQueue(BusinessObjectFactory factory, string code, string description)
		{
			var queue = factory.New<WorkQueue>();
			queue.TGM_Code = code;
			queue.TGM_Description = description;

			return queue;
		}

		public static ProcessTemplateReleaseGroupRule CreateTemplateReleaseGroupRule(ProcessTaskTemplate template, string valueSelectionMacro = null)
		{
			var rule = (ProcessTemplateReleaseGroupRule)template.ReleaseGroupRules.AddNew();

			if (valueSelectionMacro != null)
			{
				rule.PTR_ValueSelectionMacro = valueSelectionMacro;
			}

			return rule;
		}

		public static ProcessTemplateReleaseGroupRuleCategory CreateTemplateReleaseGroupRuleCategory(ProcessTemplateReleaseGroupRule rule, string categoryCode = null)
		{
			var ruleCategory = rule.Categories.AddNew();

			if (categoryCode != null)
			{
				ruleCategory.PTC_Category = categoryCode;
			}

			return ruleCategory;
		}

		public static ProcessTemplateReleaseGroupRuleMapping CreateTemplateReleaseGroupRuleMapping(ProcessTemplateReleaseGroupRule rule, string value = null, GlbGroup mappedGroup = null)
		{
			var mapping = rule.GroupMappings.AddNew();

			if (value != null)
			{
				mapping.PTM_Value = value;
			}

			if (mappedGroup != null)
			{
				mapping.PTM_GG_Group = mappedGroup.PK;
			}

			return mapping;
		}

		public static ProcessTaskTemplate CreateWorkflowTemplate(BusinessObjectFactory factory, string processType, string subType1 = null, string subType2 = null, string subType3 = null, string subType4 = null, string subType5 = null, string name = null, string description = null, bool isPartial = false, bool isUniversal = false)
		{
			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = processType;
			template.P0_IsPartialTemplate = isPartial;
			template.P0_IsUniversal = isUniversal;

			if (!string.IsNullOrEmpty(name))
			{
				template.P0_Name = name;
			}

			template.P0_Description = description;

			template.P0_SubType1 = subType1;
			template.P0_SubType2 = subType2;
			template.P0_SubType3 = subType3;
			template.P0_SubType4 = subType4;
			template.P0_SubType5 = subType5;

			if (isUniversal)
			{
				template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.NeverFallback;
			}

			return template;
		}

		public static ITemplateTrigger CreateUniversalTrigger(ProcessTaskTemplate universalTemplate, Event @event)
		{
			var trigger = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			((BusinessObject)trigger).FillWithValidTestData();

			trigger.TriggerEventCode = @event.Code;
			trigger.Description = @event.Description;

			return trigger;
		}

		public static ProcessTaskNotification CreateEmailTriggerAction(ITemplateTrigger trigger, string body, string emailAddress = "this@daveeast.com")
		{
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			triggerAction.PQ_EmailAddr = emailAddress;
			triggerAction.PQ_EmailText = body;

			return triggerAction;
		}

		public static ProcessHeader CreateWorkflow(ProcessTaskTemplate template, string description = "Zoot! Review.", ZGuid? releaseGroupPK = null)
		{
			if (!template.ProcessHeaders.AllowNew)
			{
				throw new InvalidOperationException("You're probably trying to create a workflow without having a valid BMSystem for this template type, or the BufferManagementEnabled registry item is set to false. You'll need a BMSystem so that the workflow collection works just like it does functionally.");
			}

			var workflow = (ProcessHeader)template.ProcessHeaders.AddNew();
			workflow.FH_CompletionStatement = description;

			if (releaseGroupPK != null)
			{
				workflow.FH_GG_ReleaseGroup = releaseGroupPK.Value;
			}

			return workflow;
		}

		public static ProcessHeaderLink CreateParentChildLink(ProcessTaskTemplate template, ProcessHeader headerFrom = null, ProcessHeader headerTo = null)
		{
			return CreateLink(template, headerFrom, headerTo, ProcessHeaderLinkTypeList.Codes.ParentChild);
		}

		public static ProcessHeaderLink CreateDependencyLink(ProcessTaskTemplate template, ProcessHeader headerFrom = null, ProcessHeader headerTo = null)
		{
			return CreateLink(template, headerFrom, headerTo, ProcessHeaderLinkTypeList.Codes.Dependency);
		}

		static ProcessHeaderLink CreateLink(ProcessTaskTemplate template, ProcessHeader headerFrom, ProcessHeader headerTo, string linkType)
		{
			var link = (ProcessHeaderLink)template.ProcessHeaderLinks.AddNew();
			SetLinkProperties(link, headerFrom, headerTo, linkType);

			return link;
		}

		public static ProcessHeaderLink CreateParentChildLink(ProcessHeader parentWorkflow, ProcessHeader childWorkflow, bool? syncBufferPenetration = null)
		{
			var link = parentWorkflow.Factory.New<ProcessHeaderLink>();
			SetLinkProperties(link, childWorkflow, parentWorkflow, ProcessHeaderLinkTypeList.Codes.ParentChild, syncBufferPenetration);

			return link;
		}

		public static ProcessHeaderLink CreateDependencyLink(ProcessHeader headerFrom, ProcessHeader headerTo, bool? syncBufferPenetration = null)
		{
			var link = headerFrom.Factory.New<ProcessHeaderLink>();
			SetLinkProperties(link, headerFrom, headerTo, ProcessHeaderLinkTypeList.Codes.Dependency, syncBufferPenetration);

			return link;
		}

		static void SetLinkProperties(ProcessHeaderLink link, ProcessHeader headerFrom, ProcessHeader headerTo, string linkType, bool? syncBufferPenetration = null)
		{
			link.FP_LinkType = linkType;

			if (headerFrom != null)
			{
				link.FP_FH_HeaderFrom = headerFrom.PK;
			}

			if (headerTo != null)
			{
				link.FP_FH_HeaderTo = headerTo.PK;
			}

			if (syncBufferPenetration != null)
			{
				link.FP_SynchroniseBufferPenetration = syncBufferPenetration.Value;
			}
		}

		#region ProcessTask

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public static ProcessTask CreateTask(ProcessTaskTemplate template, ProcessHeader workflow, string resourceCode = "", int lowEstMinutes = 60, string description = "Eat a Beet", decimal estVariationFactor = 2m)
		{
			var task = template.WorkflowItems.Tasks.AddNew();
			task.P9_GS_NKAssignedStaffMember = resourceCode;

			if (!string.IsNullOrEmpty(description))
			{
				task.P9_Description = description;
			}

			task.P9_EstDuration = new ZInt(lowEstMinutes).GetDateTimeFromMinutes();
			task.P9_EstimateVariationFactor = estVariationFactor;
			task.P9_FH_ProcessHeader = workflow.PK;

			var row = ((INeedRow)task).Row;
			var workflowFK = row[ProcessTasksSchema.Constants.P9_FH_ProcessHeader];

			if (workflowFK is DBNull || ((Guid)workflowFK) != workflow.PK)
			{
				row[ProcessTasksSchema.Constants.P9_FH_ProcessHeader] = workflow.PK.ToGuid(); // If there is only one workflow in the template, tasks 'detect' the workflow FK rather than it actually being set on the row. Very Unfair!
			}

			return task;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public static ProcessTask CreateTaskAndWorkflow(BMComponent component, GlbStaff staff = null, GlbCapability capability = null, GlbGroup group = null, GlbGroup workflowReleaseGroup = null, int? estimateTimeToCompleteInMinutes = null, int estimateDurationInMinutes = 0, int estimateVariationFactor = 1)
		{
			var task = CreateWorkflowAndTask(
				component.Factory,
				"workflow",
				component,
				lowEstMinutes: estimateDurationInMinutes,
				estVariationFactor: estimateVariationFactor,
				capability: capability).Tasks.First();

			if (staff != null)
			{
				task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			}

			if (group != null)
			{
				task.P9_GG_AssignedGroup = group.PK;
			}

			if (workflowReleaseGroup != null)
			{
				task.GetProcessHeader().FH_GG_ReleaseGroup = workflowReleaseGroup.PK;
			}

			if (estimateTimeToCompleteInMinutes.HasValue)
			{
				task.P9_EstimatedTimeToComplete = new ZInt(estimateTimeToCompleteInMinutes.Value).GetDateTimeFromMinutes();
			}

			return task;
		}

		#endregion

		public static IWorkflowProvider CreateDummyWorkflowProvider(BusinessObjectFactory factory, string code = "IAmA", string description = "Or am I?? Let's read on.")
		{
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_Code = code;
			dummy.Z0_Description = description;

			return dummy;
		}

		public static BMControlCustomisation CreateTinyTicketLayout(BusinessObjectFactory factory, ICustomisedLayoutSupportable layoutSupportable, string controlType)
		{
			var layout = CreateControlCustomisation(factory, controlType, 5, 5);

			CreateControlCustomisationLink(factory, layoutSupportable, layout);

			return layout;
		}

		public static BMControlCustomisationLink CreateControlCustomisationLink(BusinessObjectFactory factory, ICustomisedLayoutSupportable parent, BMControlCustomisation controlLayout, string jobType = null)
		{
			return CreateControlCustomisationLink(factory, parent.Identifier, ((BusinessObject)parent).TablePrefix, controlLayout, jobType);
		}

		public static BMControlCustomisationLink CreateControlCustomisationLink(BusinessObjectFactory factory, IGlbGroup group, BMControlCustomisation controlLayout, string jobType = null)
		{
			return CreateControlCustomisationLink(factory, @group.PK, ((BusinessObject)@group).TablePrefix, controlLayout, jobType);
		}

		public static BMControlCustomisationLink CreateControlCustomisationLink(BusinessObjectFactory factory, BMSystemReleaseGroup parent, BMControlCustomisation controlLayout, string jobType = null)
		{
			return CreateControlCustomisationLink(factory, parent.FSG_GG_Group, parent.Group.TablePrefix, controlLayout, jobType);
		}

		static BMControlCustomisationLink CreateControlCustomisationLink(BusinessObjectFactory factory, ZGuid parentId, ZString parentTableCode, BMControlCustomisation controlLayout, string jobType = null)
		{
			var link = factory.New<BMControlCustomisationLink>();
			link.FML_ParentId = parentId;
			link.FML_ParentTableCode = parentTableCode;
			link.FML_FM_ControlCustomisation = controlLayout.PK;

			if (!string.IsNullOrEmpty(jobType))
			{
				link.FML_JobType = jobType;
			}

			return link;
		}

		public static void SetComponentMembership(GlbStaff resource, BMComponent buffer, byte capacityLimitPercent)
		{
			buffer.GetOrCreateResourceLink(resource.GS_Code).FD_CapacityLimitPercent = capacityLimitPercent;
		}

		public static ConstraintStatusTestCase[] CreateConstraintStatusTestCases(BusinessObjectFactory factory, ConstrainedSchematicTestConfig config, BMComponent component = null)
		{
			var currentComponent = component ?? config.Buffer;
			var readyCCRWorkflow = BMSTestHelper.CreateWorkflow(factory, "ready-CCR workflow", currentComponent);
			BMSTestHelper.CreateTask(readyCCRWorkflow, config.CCR.GS_Code, description: "ready-CCR workflow: CCR task");

			var nonCCRWorkflow = BMSTestHelper.CreateWorkflow(factory, "non-CCR workflow", currentComponent);
			BMSTestHelper.CreateTask(nonCCRWorkflow, config.NonCCR1.GS_Code);

			var preCCRWorkflow = BMSTestHelper.CreateWorkflow(factory, "pre-CCR workflow", currentComponent);
			BMSTestHelper.CreateTask(preCCRWorkflow, config.NonCCR1.GS_Code, sequence: 1, description: "pre-CCR workflow: NonCCR1 task");
			BMSTestHelper.CreateTask(preCCRWorkflow, config.CCR.GS_Code, sequence: 2, description: "pre-CCR workflow: CCR task");

			var postCCRWorkflow = BMSTestHelper.CreateWorkflow(factory, "post-CCR workflow", currentComponent);
			BMSTestHelper.CreateTask(postCCRWorkflow, config.NonCCR1.GS_Code, sequence: 2, description: "post-CCR workflow: NonCCR1 task");
			BMSTestHelper.CreateTask(postCCRWorkflow, config.CCR.GS_Code, sequence: 1, taskStatus: "CLS", description: "post-CCR workflow: non-CCR task");

			factory.Save();

			return new[]
			{
				new ConstraintStatusTestCase(ConstraintStatusList.Codes.NonConstrained, nonCCRWorkflow),
				new ConstraintStatusTestCase(ConstraintStatusList.Codes.ReadyforConstraint, readyCCRWorkflow),
				new ConstraintStatusTestCase(ConstraintStatusList.Codes.PreConstraint, preCCRWorkflow),
				new ConstraintStatusTestCase(ConstraintStatusList.Codes.PostConstraint, postCCRWorkflow)
			};
		}

		public static BMComponent CreateComponent(BusinessObjectFactory factory, string type, string name = "component", bool isActive = true, BMSystem system = null)
		{
			if (type == BMComponentTypeList.Codes.ComponentRelationship)
			{
				return CreateComponentRelationship(factory, name, isActive);
			}

			BMComponent component = factory.New<BMComponent>();

			if (system != null)
			{
				component.FC_FS_System = system.PK;
			}
			component.FC_Type = type;
			component.FC_Name = name;
			component.FC_IsActive = isActive;

			return component;
		}

		public static ComponentRelationship CreateComponentRelationship(BusinessObjectFactory factory, string name = "componentRelationship", bool isActive = true)
		{
			ComponentRelationship component = factory.New<ComponentRelationship>();

			component.FC_Name = name;
			component.FC_IsActive = isActive;

			return component;
		}

		public static ComponentRelationshipLink CreateComponentRelationshipLink(BusinessObjectFactory factory, BMComponent componentFrom = null, BMComponent componentTo = null)
		{
			var link = factory.New<ComponentRelationshipLink>();

			if (componentFrom != null)
			{
				link.FL_FC_ComponentFrom = componentFrom.PK;
			}

			if (componentTo != null)
			{
				link.FL_FC_ComponentTo = componentTo.PK;
			}

			return link;
		}

		public static void EnableBMSInRegistry()
		{
			SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
		}

		public static void DisableBMSInRegistry()
		{
			SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);
		}

		public static void SetWorkflowManagementModeInRegistry(string mode)
		{
			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mode);
		}

		public static void AddWorkflowCategoryToRegistry(string workflowType, string code, string description)
		{
			Argument.NotNull(workflowType, nameof(workflowType));

			var categoriesCollection = BMSRegistry.Instance.WorkflowCategories.Value;

			var category = categoriesCollection.GetCategory(workflowType, code);
			if (category != null)
			{
				return;
			}

			var categories = categoriesCollection.GetCategoriesFromWorkflowCode(workflowType);
			if (categories.Count == 1 && categories[0].Code == "UDF")
			{
				categories.RemoveAll();
			}

			category = categories.AddNew();
			category.Code = code;
			category.Description = (NoResString)description;

			BMSRegistry.Instance.WorkflowCategories.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categoriesCollection);
		}

		public class ConstraintStatusTestCase
		{
			public readonly string ConstraintStatus = string.Empty;
			public readonly ProcessHeader ExpectedProcessHeader;

			public ConstraintStatusTestCase(string constraintStatus, ProcessHeader expectedProcessHeader)
			{
				ConstraintStatus = constraintStatus;
				ExpectedProcessHeader = expectedProcessHeader;
			}
		}

		public static void CreateCircularDependency(ProcessHeader first, ProcessHeader second, DbConnection testConnection)
		{
			var createCircularDependencySql = $@"
INSERT INTO dbo.ProcessHeaderLink
(FP_PK, FP_LinkType, FP_FH_HeaderFrom, FP_FH_HeaderTo, FP_TimeDelayFactor, FP_TimeDelayMinutes, FP_SynchroniseBufferPenetration, FP_IsActive, FP_SystemCreateTimeUtc, FP_SystemCreateUser, FP_SystemLastEditTimeUtc, FP_SystemLastEditUser)
VALUES
(NEWID(), 'PCH', '{first.PK}', '{second.PK}', 0, 0, 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), 'PCH', '{second
					.PK}', '{first.PK}', 0, 0, 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E')";

			testConnection.ExecuteNonQuery(createCircularDependencySql);
		}

		public static ProcessHeader[] CreateCircularDependency(BusinessObjectFactory factory, DbConnection testConnection)
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			factory.Save();

			var createCircularDependencySql = $@"
INSERT INTO dbo.ProcessHeaderLink
(FP_PK, FP_LinkType, FP_FH_HeaderFrom, FP_FH_HeaderTo, FP_TimeDelayFactor, FP_TimeDelayMinutes, FP_SynchroniseBufferPenetration, FP_IsActive, FP_SystemCreateTimeUtc, FP_SystemCreateUser, FP_SystemLastEditTimeUtc, FP_SystemLastEditUser)
VALUES
(NEWID(), 'PCH', '{workflow1.PK}', '{workflow2.PK}', 0, 0, 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), 'PCH', '{workflow2.PK}', '{workflow1.PK}', 0, 0, 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E')";

			testConnection.ExecuteNonQuery(createCircularDependencySql);

			var newFactory = factory.CreateNewFactory();

			return new[] {
				newFactory.Load<ProcessHeader>(workflow1.PK),
				newFactory.Load<ProcessHeader>(workflow2.PK)
			};
		}

		public static ProcessJobHeader[] CreateCircularDependency_JobHeader(BusinessObjectFactory factory, DbConnection testConnection)
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(factory, addDefaultProcessHeaderIfNone: false);
			jobHeader1.FH_CompletionStatement = "JobHeader1";
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(factory, addDefaultProcessHeaderIfNone: false);
			jobHeader2.FH_CompletionStatement = "JobHeader2";

			factory.Save();

			var createCircularDependencySql = $@"
INSERT INTO dbo.ProcessHeaderLink
(FP_PK, FP_LinkType, FP_FH_HeaderFrom, FP_FH_HeaderTo, FP_TimeDelayFactor, FP_TimeDelayMinutes, FP_SynchroniseBufferPenetration, FP_IsActive, FP_SystemCreateTimeUtc, FP_SystemCreateUser, FP_SystemLastEditTimeUtc, FP_SystemLastEditUser)
VALUES
(NEWID(), 'PCH', '{jobHeader1.PK}', '{jobHeader2.PK}', 0, 0, 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), 'PCH', '{jobHeader2.PK}', '{jobHeader1.PK}', 0, 0, 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E')";

			testConnection.ExecuteNonQuery(createCircularDependencySql);

			var newFactory = factory.CreateNewFactory();

			return new[] {
				newFactory.Load<ProcessJobHeader>(jobHeader1.PK),
				newFactory.Load<ProcessJobHeader>(jobHeader2.PK)
			};
		}

		public static void CacheTasksStartability(BMBoardSectionViewModel viewModel, params ProcessTask[] tasks)
		{
			foreach (var task in tasks)
			{
				task.IsStartable(viewModel.Cache);
			}

			var workflows = WorkflowLoader.LoadWorkflows(viewModel.Section, viewModel.BMBoardChannels, null, null);
			viewModel.PopulateCacheHasStartableTaskOnChannel(TaskChannelMap.ForTest(viewModel.Section, viewModel, workflows), viewModel.Cache);
		}

		#region GlbStaff

		public static IGlbStaff GetOrCreateStaff(BusinessObjectFactory factory, string staffCode, string fullName = "")
		{
			var staff = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffCode);

			if (staff == null)
			{
				staff = factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = staffCode;
				staff.ResetBranchAndDepartment();
			}

			staff.GS_FullName = fullName;

			return staff;
		}

		public static GlbStaff CreateStaff(BusinessObjectFactory factory, string staffCode, string fullName = null, GlbBranch homeBranch = null, GlbDepartment homeDepartment = null)
		{
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = staffCode;

			if (fullName != null)
			{
				staff.GS_FullName = fullName;
			}

			if (homeBranch != null)
			{
				staff.GS_GB_HomeBranch = homeBranch.PK;
			}

			if (homeDepartment != null)
			{
				staff.GS_GE_HomeDepartment = homeDepartment.PK;
			}

			return staff;
		}

		public static GlbStaff CreateStaff(BusinessObjectFactory factory, GlbCapability capability = null, GlbGroup group = null)
		{
			var staff = factory.NewWithValidTestData<GlbStaff>();

			if (capability != null)
			{
				staff.Capabilities.Add(capability);
			}

			if (group != null)
			{
				staff.Groups.Add(group);
			}

			return staff;
		}

		#endregion

		public static string CreateVeryLongString()
		{
			var longString = string.Empty;

			for (int j = 0; j < 10000; j++)
			{
				longString += "You are looking at the longest string in the history of the world. ";
			}

			return longString;
		}

		public static void ClearCachedCapacity(ZGuid bufferPK)
		{
			BufferCapacityCache.Clear();
		}

		public static IEnumerable<string> GetEnumMemberAttributeValues<T>() where T : Enum
		{
			var enumType = typeof(T);
			return Enum.GetNames(enumType)
				.Select(enumName => enumType.GetMember(enumName)
					.First()
					.GetCustomAttributes(false)
					.OfType<EnumMemberAttribute>()
					.FirstOrDefault()?
					.Value)
				.WhereNotNull()
				.OrderBy(enumMemberValue => enumMemberValue)
				.ToArray();
		}

		public static void SetupEmailAndGroup(BusinessObjectFactory factory)
		{
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "frodo@bagend.com";

			var bmsNotificationGroup = factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(bmsNotificationGroup);

			factory.Save();

			BMSRegistry.Instance.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bmsNotificationGroup.PK.ToGuid());
		}

		public static IGlbBranch GetBranch(IBranchDepartmentProvider provider, BusinessObjectFactory factory)
		{
			return provider.GetBranch(factory);
		}

		public static IGlbDepartment GetDepartment(IBranchDepartmentProvider provider, BusinessObjectFactory factory)
		{
			return provider.GetDepartment(factory);
		}

		#region Serialisation

		public static void SerialiseAndDeSerialise(IXmlSerializable source, IXmlSerializable destination)
		{
			var builder = new StringBuilder();

			using (var writer = XmlWriter.Create(builder, new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment }))
			{
				source.WriteXml(writer);
			}

			var reader = XmlReader.Create(new StringReader(builder.ToString()), new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment });

			reader.MoveToContent();
			destination.ReadXml(reader);
		}

		#endregion

		#region Performance

		/// <summary>
		/// Reset the statically-cached factory, named 'Client side Cache', which holds rows that can be used by all other factories. This can make db hit tests deterministic if some tables fluctuate, especially after the first time the test is run.
		/// </summary>
		public static void ClearUberFactory()
		{
			RowFactory.ResetCacheAfterDbUpgrade();
		}

		public static IDisposable TemporarilyDisableTableCachingInUberFactory(IReadOnlyCollection<string> tablesToDisableCaching)
		{
			var settings = TestEntityFrameworkSettings.Get();
			var cachedTables = settings.CachedTables.Split(',').ToList();

			foreach (var tableName in tablesToDisableCaching)
			{
				cachedTables.Remove(tableName);
			}
			var cachedTablesString = string.Join(",", cachedTables);
			settings.CachedTables = cachedTablesString;

			var tableCachingRemoval = RowFactory.RemoveCachedTablesTemporarily(tablesToDisableCaching.ToArray());

			return new DisposableAction(() =>
			{
				tableCachingRemoval.Dispose();
				settings.Dispose();
			});
		}

		public static StmUsage CreateSectionPerformanceStatistics(BMBoardSection section, ZDateTime startTimeUtc, TimeSpan duration, bool emptyEntry = false)
		{
			var viewModel = CreateViewModel(section);
			var statisticsNames = viewModel.CreatePerformanceStatisticsNamePair();
			return StatisticsTestHelper.CreateUsage(section.Factory, startTimeUtc.ToDateTime(), (startTimeUtc + duration).ToDateTime(), "1.1.1.1", "C_1", "B_1", "E", statisticsNames.Item1, emptyEntry ? string.Empty : statisticsNames.Item2, 1, Convert.ToDecimal(duration.TotalSeconds));
		}

		#endregion

		#region ProcessHeader Release Failure

		public static void SetLastReleaseFailureReason(ProcessHeader workflow, string failureReason, BMComponent buffer, ReleaseGateFailureLogService failureService)
		{
			var logs = new Dictionary<ZGuid, string>();

			logs.Add(buffer.PK, failureReason);

			failureService.QueueFailureLog(workflow.PK, logs);
			failureService.SubmitQueueToRemoteCache();
		}

		#endregion

		#region Workflow

		public static void Defer(ProcessHeader workflow, string reason = "")
		{
			new DeferWorkflowViewModel(workflow, null).Defer(reason);
		}

		public static ProcessHeader CreateQualityIteration(IProcessTask iterateFromTask, IProcessTask containmentBarrierTask, string iterationWorkflowDescription = "", string resourceUnderReviewStaffCode = null, string iterationReasonCode = null, bool shouldCreateWorkflowForIteration = true)
		{
			return BMSTestCaseWithFactory.CreateQualityIteration(iterateFromTask, containmentBarrierTask, iterationWorkflowDescription, resourceUnderReviewStaffCode, iterationReasonCode, shouldCreateWorkflowForIteration);
		}

		#endregion

		#region Factory

		public static bool IsPAVEFactory(BusinessObjectFactory factory)
		{
			return factory.NameForDebugging.Contains("SetupTasks")
				   || factory.NameForDebugging.Contains("Pipe")
				   || factory.NameForDebugging.Contains("Channel")
				   || factory.NameForDebugging.Contains("TaskCard")
				   || factory.NameForDebugging.Contains("Board")
				   || factory.NameForDebugging.Contains("Headings")
				   || factory.NameForDebugging.Contains("CustomisedCard")
				   || factory.NameForDebugging.Contains("ReleaseGate")
				   || factory.NameForDebugging.Contains(nameof(ServiceTaskFactoryProviderWrapper))
				   || factory.NameForDebugging.Contains(nameof(TransferRuleRunnerDataAccessor))
				   || factory.NameForDebugging.Contains(nameof(WorkflowCapabilityAssignerDataAccessor))
				   || factory.NameForDebugging.Contains(nameof(WorkflowCapabilityAssignerRelatedDataLoader))
				   || BMSTestCaseWithFactory.IsAcceptabilityBandCalculationFactory(factory);
		}

		#endregion

		#region Resource

		/// <summary>
		/// This represents the calculations done in the <see cref="CapacityConstrainedResourceStatusServiceTask"/> when merging capacity utilised in feeding buckets.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public static decimal GetStandardEstimateHoursInFeedingComponents(GlbStaff staff, BMComponent buffer)
		{
			var feedingComponents = buffer.GetFeedingComponents().ToArray();

			if (feedingComponents.Length > 0)
			{
				var context = WorkingTimeContext.Create(buffer);
				return feedingComponents.Sum(c => CapacityCalculator.GetUtilisedCapacity_ForTest(staff, c, context)) * 60;
			}

			return decimal.Zero;
		}

		#endregion

		public static string EncloseInQuotes(string s)
		{
			return '\'' + s + '\'';
		}

		public static T GetChannelEntity<T>(IVisualBoardChannel channel, BusinessObjectFactory factory)
			where T : BusinessObject
		{
			return factory.Load<T>(channel.EntityPK);
		}

		public static Color[] GetTagColors(ICardContent card, BMBoardSectionViewModel viewModel)
		{
			return TagProvider.GetOrderedColors(card.Definitions, card.ApplicableTagMagnitudes);
		}

		public static ITagOperationResult AddTagAndClearCache(ITagable tagable, TagMagnitude tag, BMBoardSectionViewModel viewModelToClearCache, BMBoardSection section, params ProcessHeader[] workflows)
		{
			viewModelToClearCache?.Cache.Clear();

			var result = tagable.AddTag(tag);

			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				BMBoardSectionViewModel.CreateAndPopulatePropertyCache(
					TaskChannelMap.ForTest(section, viewModelToClearCache, workflows),
					viewModelToClearCache);

				strategy.AwaitAll(taskToIgnore: null);
			}

			return result;
		}

		public static void SetValuesExclusive(ZBoolDescriptionPairList list, params string[] codes)
		{
			// As this is O(n^2) it is for testing only.
			foreach (var pair in list)
			{
				pair.Value = codes.Any(code => pair.Description.StartsWith(code));
			}
		}

		public static bool HandleTaskAssignment(IVisualBoardChannel destinationChannel, BusinessObjectFactory factory, BMBoardSectionViewModel sectionViewModel, ICardContent cardContent, IVisualBoardChannel currentChannel = null, IMultiActionButtonDialogWrapper<CrossChannelTaskAssignments> dialogProvider = null, BMBoardSectionViewModel sourceSectionViewModel = null)
		{
			var sourceCellContent = new CellContent(0, 0, CellContentType.Cards) { Channel = currentChannel };
			var destinationCellContent = new CellContent(0, 0, CellContentType.Cards) { Channel = destinationChannel };

			return new TicketDragDropHandler(factory, sectionViewModel, sourceCellContent, destinationCellContent, cardContent, dialogProvider, (sourceSectionViewModel ?? sectionViewModel)).TryMoveToDestinationCell();
		}

		public static decimal GetTaskEstimatesInCells(ComponentGrid grid, IEnumerable<int> primaryAxes = null, IEnumerable<int> secondaryAxes = null, IEnumerable<ZString> staffCodes = null, CCRFilter ccrFilter = CCRFilter.None, bool includePenetratedComponents = false, params ZGuid[] components)
		{
			return ComponentGrid.GetTaskEstimatesInCells(grid.CardAllocationMap, primaryAxes, secondaryAxes, staffCodes, ccrFilter, includePenetratedComponents, components);
		}

		public static IEnumerable<StmALog> GetLogsForAllWorkflows(IWorkflowProvider job, Event eventType)
		{
			var workflows = new BusinessObjectFactory().Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.FH_ParentId, job.PK));

			foreach (var workflow in workflows)
			{
				var logs = workflow.Logs.Find(x => x.SL_SE_NKEvent == eventType.Code).ToArray();

				foreach (var log in logs)
				{
					yield return log;
				}
			}
		}

		#region Release Sequencing

		public static BMReleaseSequence CreateReleaseSequence(BusinessObjectFactory factory, ZGuid releaseGroup, string name = "Skywalker Saga", bool isActive = true, ZGuid? capability = null, int? nudge = null)
		{
			var sequence = factory.New<BMReleaseSequence>();

			sequence.BMR_Name = name;
			sequence.BMR_IsActive = isActive;
			sequence.BMR_G4_Capability = capability ?? ZGuid.Empty;
			sequence.BMR_GG_ReleaseGroup = releaseGroup;
			if (nudge != null)
			{
				sequence.BMR_SequenceNudge = nudge.Value;
			}

			return sequence;
		}

		public static BMReleaseSequenceItem CreateReleaseSequenceItem(BMReleaseSequence sequence, ProcessHeader workflow, int position = 1, int value = 0, int investment = 0, string note = "If this is a consular ship, where is the ambassador?")
		{
			var item = sequence.Items.AddNew();

			item.BMI_FH_ProcessHeader = workflow.PK;
			item.BMI_Position = position;
			item.BMI_Value = value;
			item.BMI_Investment = investment;
			item.BMI_Note = note;

			return item;
		}

		#endregion

		#region AcceptabilityBandResultCache

		public static void DisableAcceptabilityBandResultCache()
		{
			BMSRegistry.Instance.AcceptabilityBandClientCacheTimePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			BMSRegistry.Instance.AcceptabilityBandServerCacheTimePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
		}

		public static void EnableAcceptabilityBandResultClientCache(TimeSpan? timeout = null)
		{
			if (timeout == null)
			{
				timeout = TimeSpan.FromSeconds(BMSRegistry.Instance.AcceptabilityBandClientCacheTimePeriod.DefaultValue);
			}

			BMSRegistry.Instance.AcceptabilityBandClientCacheTimePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Convert.ToInt32(timeout?.TotalSeconds));
		}

		public static ICollection<ZGuid> GetAcceptabilityBandCachedWorkflowPKs(ZGuid sectionPK)
		{
			var items = MemoryCache.Default.Get("BoardAcceptabilityBandLocalCache.SectionWorkflowPKs." + sectionPK) as BoardAcceptabilityBandCalculator.ABWorkflowPKsCacheItem;
			return items?.PKs;
		}

		public static void ClearAcceptabilityBandCache() => BoardAcceptabilityBandCalculator.ClearBoardAcceptabilityBandLocalCache();

		#endregion

		#endregion

		#region Service Tasks

		public static void CreateServiceTask_WithoutAssemblyReference(BusinessObjectFactory factory, string code, string name, int taskPeriodCount, char taskPeriod, bool active = true, ZDateTime? nextRunTime = null, Guid? branch = null)
		{
			CreateServiceTask(factory, code, name, taskPeriodCount, taskPeriod, active, nextRunTime, branch);
		}

		public static StmScheduleTask CreateServiceTask(BusinessObjectFactory factory, string code, string name, int taskPeriodCount = 1, char taskPeriod = 'H', bool active = true, ZDateTime? nextRunTime = null, Guid? branch = null)
		{
			var task = factory.NewWithValidTestData<StmScheduleTask>();
			task.S5_TypeOfDocument = BMSServiceTaskBase.Category;
			task.S5_ParentTableCode = StmServiceHostSchema.Constants.Prefix;
			task.S5_ScheduleType = code;
			task.S5_ScheduleDescription = name;
			task.S5_TaskPeriodCount = taskPeriodCount;
			task.S5_TaskPeriod = taskPeriod.ToString();
			task.S5_IsActive = active;

			if (nextRunTime != null)
			{
				task.S5_NextScheduledPrintRunTimeUtc = nextRunTime.Value;
			}

			if (branch != null)
			{
				task.S5_GB = branch.Value;
			}

			return task;
		}

		public static string RunLogWalker()
		{
			return MasterFilesTestHelper.RunLogWalker();
		}

		#endregion

		#region Assert Methods

		/// <summary>
		/// </summary>
		/// <param name="message"></param>
		/// <param name="job"></param>
		/// <param name="eventType"></param>
		/// <param name="expectedWorkflowsWithLog">If you're expecting a workflow to have more than one of the specified log type, include that workflow multiple times.</param>
		public static void AssertWorkflowsHaveLog(string message, IWorkflowProvider job, Event eventType, params IProcessHeader[] expectedWorkflowsWithLog)
		{
			var logs = GetLogsForAllWorkflows(job, eventType).ToArray();
			var workflowPKs = logs.Select(x => x.SL_Parent);
			var workflows = job.LogsFactory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, workflowPKs));
			var actualWorkflows = new List<string>();

			foreach (var log in logs)
			{
				var workflow = workflows.Single(x => x.PK == log.SL_Parent);
				actualWorkflows.Add(workflow.FH_CompletionStatement);
			}

			var expectedWorkflows = expectedWorkflowsWithLog.Select(x => x.FH_CompletionStatement);

			Assertion.AssertContainsExactElementsInAnyOrder(message, expectedWorkflows, actualWorkflows);
		}

		public static void AssertReleaseGroup(GlbGroup expectedReleaseGroup, ProcessHeader workflow)
		{
			AssertReleaseGroup(string.Empty, expectedReleaseGroup, workflow);
		}

		public static void AssertReleaseGroup(string assertionMessage, GlbGroup expectedReleaseGroup, ProcessHeader workflow)
		{
			if (expectedReleaseGroup == null)
			{
				Assertion.AssertNull(assertionMessage, workflow.ReleaseGroup);
			}
			else
			{
				Assertion.AssertNotNull(assertionMessage, workflow.ReleaseGroup);
				Assertion.AssertEquals(assertionMessage, expectedReleaseGroup.GG_Code, workflow.ReleaseGroup.GG_Code);
			}
		}

		/// <summary>
		/// Asserting colors array with enhanded message:
		///		1) color name as string instead of its Hex value
		///		2) One assertion message per test
		///		3) Split multiple primary-axes to multiple lines
		/// Examples:
		///		* Expected:
		///		PrimaryAxis 6: 1.DefaultZone0, 2.DefaultZone1, 3.DefaultZone2, 4.DefaultZone2, 5.DefaultZone3, 6.DefaultZone1, 7.DefaultZone1, 8.DefaultZone1, 9.DefaultZone2, 10.DefaultZone2, 11.DefaultZone3, 12.DefaultZone3, 13.DefaultZone3
		///		* But found: 
		///		PrimaryAxis 6: 1.DefaultZone0, 2.DefaultZone1, 3.DefaultZone2, 4.DefaultZone2, 5.DefaultZone3, 6.DefaultZone1, 7.DefaultZone1, 8.DefaultZone1, 9.DefaultZone2, 10.DefaultZone2, 11.DefaultZone3, 12.DefaultZone3, 13.DefaultZone3
		/// </summary>
		public static void AssertColors(string message, BMBoardSection section, IEnumerable<VisualBoardPositionForTest<Color?>> expectedColors, IEnumerable<VisualBoardPositionForTest<Color?>> actualColors, int totalTabs = 0)
		{
			var tabs = new string(' ', totalTabs * 4);

			var sortedExpectedColors = expectedColors.OrderBy(c => c.PrimaryAxis).ThenBy(c => c.SecondaryAxis);
			var sortedActualColors = actualColors.OrderBy(c => c.PrimaryAxis).ThenBy(c => c.SecondaryAxis);

			Assertion.AssertEquals("Expected length and Actual length should be the same", expectedColors.Count(), actualColors.Count());

			var expectedStringColors = new ZStringBuilder();
			foreach (var groupedColors in sortedExpectedColors.GroupBy(c => c.PrimaryAxis))
			{
				var stringColors = string.Join(", ", groupedColors.Select(c => c.SecondaryAxis + "." + GetColorInfoAsString(section, c.Value)));
				expectedStringColors.Append(string.Format("\r\n{0}PrimaryAxis {1}: {2}", tabs, groupedColors.Key, stringColors));
			}

			var actualStringColors = new ZStringBuilder();
			foreach (var groupedColors in sortedActualColors.GroupBy(c => c.PrimaryAxis))
			{
				var stringColors = string.Join(", ", groupedColors.Select(c => c.SecondaryAxis + "." + GetColorInfoAsString(section, c.Value)));
				actualStringColors.Append(string.Format("\r\n{0}PrimaryAxis {1}: {2}", tabs, groupedColors.Key, stringColors));
			}

			var assertMessage = string.Format("{0}{1}.\r\n{0} * Expected:{2}\r\n{0}* But found: {3}",
				tabs,
				message,
				expectedStringColors.ToString(),
				actualStringColors.ToString());

			var expectedColorsAsArray = sortedExpectedColors.ToArray();
			var actualColorsAsArray = sortedActualColors.ToArray();

			var comparer = new VisualBoardPositionTestComparer<Color?>();

			for (var index = 0; index < expectedColorsAsArray.Length; index++)
			{
				if (!comparer.Equals(expectedColorsAsArray[index], actualColorsAsArray[index]))
				{
					Assertion.Assert(assertMessage, false);
					break;
				}
			}
		}

		public static string GetColorInfoAsString(BMBoardSection section, Color? nullableColor)
		{
			if (nullableColor == null)
			{
				return "Null";
			}

			var color = nullableColor.Value;

			if (section != null && section.SectionConfiguration != null)
			{
				if (color == section.SectionConfiguration.BufferZone0ColorValue)
				{
					return "SectionZone0";
				}
				else if (color == section.SectionConfiguration.BufferZone1ColorValue)
				{
					return "SectionZone1";
				}
				else if (color == section.SectionConfiguration.BufferZone2ColorValue)
				{
					return "SectionZone2";
				}
				else if (color == section.SectionConfiguration.BufferZone3ColorValue)
				{
					return "SectionZone3";
				}

				if (section.SectionConfiguration.BufferZone0ColorValue != null)
				{
					if (color == section.SectionConfiguration.BufferZone0ColorValue.Value.FadeTowardsWhite())
					{
						return "SectionZone0Fade";
					}
					else if (color == section.SectionConfiguration.BufferZone1ColorValue.Value.FadeTowardsWhite())
					{
						return "SectionZone1Fade";
					}
					else if (color == section.SectionConfiguration.BufferZone2ColorValue.Value.FadeTowardsWhite())
					{
						return "SectionZone2Fade";
					}
					else if (color == section.SectionConfiguration.BufferZone3ColorValue.Value.FadeTowardsWhite())
					{
						return "SectionZone3Fade";
					}
				}
			}

			if (color == BMConstants.Zone0DefaultColor)
			{
				return "DefaultZone0";
			}
			else if (color == BMConstants.Zone1DefaultColor)
			{
				return "DefaultZone1";
			}
			else if (color == BMConstants.Zone2DefaultColor)
			{
				return "DefaultZone2";
			}
			else if (color == BMConstants.Zone3DefaultColor)
			{
				return "DefaultZone3";
			}
			else if (color == section.BackgroundColorValue)
			{
				return "SectionBGColor";
			}
			else if (color == SystemColors.Control)
			{
				return "SystemColor";
			}
			else if (color == BMConstants.Zone0DefaultColor.FadeTowardsWhite())
			{
				return "DefaultZone0Fade";
			}
			else if (color == BMConstants.Zone1DefaultColor.FadeTowardsWhite())
			{
				return "DefaultZone1Fade";
			}
			else if (color == BMConstants.Zone2DefaultColor.FadeTowardsWhite())
			{
				return "DefaultZone2Fade";
			}
			else if (color == BMConstants.Zone3DefaultColor.FadeTowardsWhite())
			{
				return "DefaultZone3Fade";
			}
			else if (color == section.BackgroundColorValue.FadeTowardsWhite())
			{
				return "SectionBGColorFade";
			}
			else if (color == SystemColors.Control.FadeTowardsWhite())
			{
				return "SystemColorFade";
			}
			else
			{
				return color.Name;
			}
		}

		public static ProcessHeader CreateWorkflowForTest(BusinessObjectFactory factory, ComplexConstrainedSchematicTestConfig config, ConstraintStatus constraintStatus, ZString ccr = default(ZString), ZString ncr = default(ZString))
		{
			Assertion.Assert("Method only designed for pre/post constraint, please extend it if needed", constraintStatus.In(ConstraintStatus.PreConstraint, ConstraintStatus.PostConstraint));

			var ccrSequence = constraintStatus == ConstraintStatus.PreConstraint ? 2 : 1;
			var nonCCRSequence = constraintStatus == ConstraintStatus.PreConstraint ? 1 : 2;

			var constraintStatusCode = constraintStatus.ToCode();
			var workflowDesription = string.Format("workflow-{0}", constraintStatusCode);

			var workflow = BMSTestHelper.CreateWorkflowAndTask(
				factory,
				completionStatement: workflowDesription,
				description: string.Format("{0}: CCR-task", workflowDesription),
				currentComponent: config.Buffer,
				releaseDateTime: ZDateTime.UtcToday,
				staffCode: ccr.IsEmpty ? config.CCR.GS_Code : ccr,
				lowEstMinutes: 100,
				taskStatus: ProcessTaskStatusCodeList.Codes.Closed,
				sequence: ccrSequence);

			var nonCCRTask = BMSTestHelper.CreateTask(
				workflow,
				ncr.IsEmpty ? config.NonCCR1.GS_Code : ncr,
				description: string.Format("{0}: {1}-task", workflowDesription, constraintStatusCode),
				lowEstMinutes: 100,
				sequence: nonCCRSequence);

			config.Workflows.Add(workflow);

			return workflow;
		}

		#endregion

		#region Assert Helper Methods

		public static void AssertComponent(BMComponent expectedComponent, BMComponent actualComponent)
		{
			AssertComponent(string.Empty, expectedComponent, actualComponent);
		}

		public static void AssertComponent(string message, BMComponent expectedComponent, BMComponent actualComponent)
		{
			StringBuilder messages = new StringBuilder(message);
			messages.AppendLine(string.Format("Expected component '{0}'({1}) but found '{2}'({3}) ", expectedComponent.FC_Name, expectedComponent.PK, actualComponent.FC_Name, actualComponent.PK));
			Assertion.AssertEquals(messages.ToString(), expectedComponent.PK, actualComponent.PK);
		}

		public static void AssertModuleFilterDeepClone(StmModuleFilter original, StmModuleFilter clone, BusinessObject cloneParent)
		{
			RelatedModuleFilterTestHelper.AssertModuleFilterDeepClone(original, clone, cloneParent);
		}

		public static void AssertConnectionCannotBeUsedForWrites(DbConnection connection, string message)
		{
			var factoryOnReaderConnection = new BusinessObjectFactory(connection);
			var bizo = factoryOnReaderConnection.NewWithValidTestData<BMSystem>();

			var exception = Assertion.AssertExceptionThrown<ZSaveException>(message, factoryOnReaderConnection.Save);

			Assertion.AssertContains("The INSERT permission was denied on the object 'BMSystem'", exception.Message);
		}

		public static void AssertConnectionCanBeUsedForWrites(DbConnection connection, string message)
		{
			var factoryOnReaderConnection = new BusinessObjectFactory(connection);
			var bizo = factoryOnReaderConnection.NewWithValidTestData<BMSystem>();

			Assertion.AssertNoExceptionThrown(message, factoryOnReaderConnection.Save);
		}

		#endregion
	}
}

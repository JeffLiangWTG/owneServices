using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.TimeEngineScheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMSystem))]
	public class BMSystemTest : EnterpriseBusinessObjectTestCase
	{
		#region Delete

		[ExpectNoExceptions]
		public void TestDelete_WhenChildObjectsNotAlreadyLoaded()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedSystem = newFactory.Load<BMSystem>(system.PK);

			loadedSystem.Delete();
			newFactory.Save();
		}

		public void TestDelete_ShouldDeleteLayoutLinks()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var layout = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			var link = BMSTestHelper.CreateControlCustomisationLink(Factory, system, layout);

			Factory.Save();

			var loadedSystem = Factory.CreateNewFactory().Load<BMSystem>(system.PK);
			loadedSystem.Delete();
			loadedSystem.Factory.Save();

			AssertEquals(true, link.IsDeleted);
		}

		#endregion

		#region Clone

		public void TestClone_ComplexSystem()
		{
			var system = Factory.New<BMSystem>();
			system.FS_Name = "DeathStar street elmo facelift";

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();

			#region ComponentLinks

			// component1 -> component2
			// component1 -> component3
			// component1 -> component4
			// component2 -> component3
			// component2 -> component4
			// component3 -> component4
			// component3 -> component1

			var component1 = system.Components.AddNew();
			component1.FC_Name = "One";
			var component2 = system.Components.AddNew();
			component2.FC_Name = "Two";
			var component3 = system.Components.AddNew();
			component3.FC_Name = "Three";
			var component4 = system.Components.AddNew();
			component4.FC_Name = "Four";

			// component1 links
			var link1_2 = component1.FromMeToOthersLinks.AddNew();
			link1_2.FL_FC_ComponentTo = component2.PK;
			var link1_3 = component1.FromMeToOthersLinks.AddNew();
			link1_3.FL_FC_ComponentTo = component3.PK;
			link1_3.FilterRule.S9_FilterData = new ZBlob(new byte[] { 1, 2, 3, 4, 5 });

			var link1_4 = component1.FromMeToOthersLinks.AddNew();
			link1_4.FL_FC_ComponentTo = component4.PK;

			// component2 links
			var link2_3 = component2.FromMeToOthersLinks.AddNew();
			link2_3.FL_FC_ComponentTo = component3.PK;

			var link2_4 = component2.FromMeToOthersLinks.AddNew();
			link2_4.FL_FC_ComponentTo = component4.PK;

			// component3 links
			var link3_4 = component3.FromMeToOthersLinks.AddNew();
			link3_4.FL_FC_ComponentTo = component4.PK;

			var link3_4FilterData = new ZBlob(new byte[] { 9, 8, 7, 6, 5 });
			var link3_4Filter = link3_4.FilterRule;
			link3_4Filter.S9_FilterData = link3_4FilterData;

			var link3_1 = component3.FromMeToOthersLinks.AddNew();
			link3_1.FL_FC_ComponentTo = component1.PK;

			var zoneMultiplier1_1 = component1.ZoneCapacityMultipliers.AddNew();
			zoneMultiplier1_1.BZC_GG_ReleaseGroup = group1.PK;

			var zoneMultiplier3_1 = component3.ZoneCapacityMultipliers.AddNew();
			zoneMultiplier3_1.BZC_GG_ReleaseGroup = group2.PK;

			#endregion

			var workflowType = system.RelatedWorkflowTypes.AddNew();
			workflowType.FSW_WorkflowType = "SIM";

			Factory.Save();

			var clone = (BMSystem)system.Clone();

			AssertEquals(1, clone.RelatedWorkflowTypes.Count);
			AssertEquals("SIM", clone.RelatedWorkflowTypes[0].FSW_WorkflowType);

			AssertEquals(system.Components.Count, clone.Components.Count);

			var cloneComponent1 = clone.Components.Single(c => c.FC_Name == component1.FC_Name);
			var cloneComponent2 = clone.Components.Single(c => c.FC_Name == component2.FC_Name);
			var cloneComponent3 = clone.Components.Single(c => c.FC_Name == component3.FC_Name);
			var cloneComponent4 = clone.Components.Single(c => c.FC_Name == component4.FC_Name);

			var componentPKMapping = new Dictionary<ZGuid, ZGuid>();
			componentPKMapping[component1.PK] = cloneComponent1.PK;
			componentPKMapping[component2.PK] = cloneComponent2.PK;
			componentPKMapping[component3.PK] = cloneComponent3.PK;
			componentPKMapping[component4.PK] = cloneComponent4.PK;
			var reversedPKMapping = componentPKMapping.ToDictionary(x => x.Value, x => x.Key);

			AssertComponentReplicatesItSelf(componentPKMapping, reversedPKMapping, component1, cloneComponent1);
			AssertComponentReplicatesItSelf(componentPKMapping, reversedPKMapping, component2, cloneComponent2);
			AssertComponentReplicatesItSelf(componentPKMapping, reversedPKMapping, component3, cloneComponent3);
			AssertComponentReplicatesItSelf(componentPKMapping, reversedPKMapping, component4, cloneComponent4);

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestTableIsCached()
		{
			AssertContains(BMSystemSchema.Constants.TableName, SystemDataRegistry.Instance.CachedTables.Value);

			var expectedHitCounts = new Dictionary<string, int>
			{
				{ BMSystemSchema.Constants.TableName, 0 },
			};

			using (AssertDbHitsForAllFactories(expectedHitCounts, ignoreUnspecified: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				var system = Factory.New<BMSystem>();
				system.FS_Name = "Teehee";
				Factory.Save();

				var reloaded = Factory.Load<BMSystem>(system.PK);
				var reloadedAgain = Factory.Load<BMSystem>(system.PK);
				var reloadedOnceMore = Factory.Load<BMSystem>(system.PK);
			}
		}

		public void TestClone_ChildComponentsOnSystem()
		{
			var system = Factory.New<BMSystem>();
			system.FS_Name = "Teehee";

			var component1 = system.Components.AddNew();
			component1.FC_Name = "a";

			var childComponent = component1.ChildComponents.AddNew();

			var clone = (BMSystem)system.Clone();

			AssertEquals(true, clone.Components.SelectMany(c => c.ChildComponents).All(c => c.FC_FS_System == clone.PK));
		}

		public void TestClone()
		{
			var group1 = Factory.New<GlbGroup>();
			var group2 = Factory.New<GlbGroup>();

			var system = BMSTestHelper.CreateSystem(Factory, "SIM");
			system.FS_Name = "Teehee";
			system.FS_ResourceCountdownHours = new ZDateTime(ZDateTime.BrettsBirthday);

			var component1 = system.Components.AddNew();
			component1.FC_Name = "a";
			var component2 = system.Components.AddNew();
			var component3 = system.Components.AddNew();
			component3.FC_Name = "b";

			var link1_2 = component1.FromMeToOthersLinks.AddNew();
			link1_2.FL_FC_ComponentTo = component2.PK;
			var link3_1 = component3.FromMeToOthersLinks.AddNew();
			link3_1.FL_FC_ComponentTo = component1.PK;

			var zoneMultiplier1_1 = component1.ZoneCapacityMultipliers.AddNew();
			zoneMultiplier1_1.BZC_GG_ReleaseGroup = group1.PK;

			var zoneMultiplier3_1 = component3.ZoneCapacityMultipliers.AddNew();
			zoneMultiplier3_1.BZC_GG_ReleaseGroup = group2.PK;

			var clone = (BMSystem)system.Clone();

			AssertEquals(system.FS_Name + " [1]", clone.FS_Name);
			AssertEquals(system.FS_ResourceCountdownHours, clone.FS_ResourceCountdownHours);
			AssertEquals(3, clone.Components.Count);

			var cloneComponent1 = clone.Components.Single(c => c.FC_Name == component1.FC_Name);
			var cloneComponent3 = clone.Components.Single(c => c.FC_Name == component3.FC_Name);

			AssertNotEquals(component1.ZoneCapacityMultipliers.Single(l => l.BZC_GG_ReleaseGroup == group1.PK).PK, cloneComponent1.ZoneCapacityMultipliers.Single(l => l.BZC_GG_ReleaseGroup == group1.PK).PK);
			AssertNotEquals(component3.ZoneCapacityMultipliers.Single(l => l.BZC_GG_ReleaseGroup == group2.PK).PK, cloneComponent3.ZoneCapacityMultipliers.Single(l => l.BZC_GG_ReleaseGroup == group2.PK).PK);

			AssertEquals(1, cloneComponent1.FromMeToOthersLinks.Count);
			AssertEquals(1, cloneComponent1.FromOthersToMeLinks.Count);

			AssertEquals(1, clone.RelatedWorkflowTypes.Count);
			AssertEquals("SIM", clone.RelatedWorkflowTypes.Single().FSW_WorkflowType);
		}

		public void TestClone_WhenLinkIsNonReleaseGateLink_ShouldPreseveFlagOnClone()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var link = BMSTestHelper.LinkComponents(bucket, buffer, isReleaseGate: false);

			AssertEquals(false, link.FL_IsReleaseGateRuleApplied);

			var cloneSystem = (BMSystem)system.Clone();
			var cloneLink = cloneSystem.Components.Single(c => c.FC_Name == "bucket").FromMeToOthersLinks[0];

			AssertEquals(false, cloneLink.FL_IsReleaseGateRuleApplied);
		}

		#endregion

		public void TestComponents_ShouldSortBySequence()
		{
			var system = Factory.New<BMSystem>();
			var component1 = system.Components.AddNew();
			component1.FC_Name = "component1";
			component1.FC_DisplaySequence = 2;
			var component2 = system.Components.AddNew();
			component2.FC_Name = "component2";
			component2.FC_DisplaySequence = 1;

			AssertEquals(component2, system.Components[0]);
			AssertEquals(component1, system.Components[1]);
		}

		public void TestConfiguration()
		{
			var system = Factory.New<BMSystem>();
			system.FS_Name = "name";
			system.FS_Description = "desc";

			var workflowType1 = system.RelatedWorkflowTypes.AddNew();
			workflowType1.FSW_WorkflowType = "ABC";

			var workflowType2 = system.RelatedWorkflowTypes.AddNew();
			workflowType2.FSW_WorkflowType = "XYZ";

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup1 = system.ReleaseGroups.AddNew();
			releaseGroup1.FSG_GG_Group = group1.PK;

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup2 = system.ReleaseGroups.AddNew();
			releaseGroup2.FSG_GG_Group = group2.PK;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var reloadedSystem = factory2.Load<BMSystem>(system.PK);
			AssertEquals(2, reloadedSystem.RelatedWorkflowTypes.Count);
			Assert(reloadedSystem.RelatedWorkflowTypes.Any(x => x.FSW_WorkflowType == "ABC"));
			Assert(reloadedSystem.RelatedWorkflowTypes.Any(x => x.FSW_WorkflowType == "XYZ"));
			AssertEquals(false, reloadedSystem.HasChanges);

			AssertEquals(2, reloadedSystem.ReleaseGroups.Count);
			Assert(reloadedSystem.ReleaseGroups.Cast<BMSystemReleaseGroup>().Any(g => g.FSG_GG_Group == group1.PK));
			Assert(reloadedSystem.ReleaseGroups.Cast<BMSystemReleaseGroup>().Any(g => g.FSG_GG_Group == group2.PK));
		}

		public void TestTwoSystems_CorrectEntryComponentSelected()
		{
			var system1 = Factory.New<BMSystem>();
			system1.FS_Name = "Fast";

			var component11 = BMSTestHelper.CreateBucket(system1, "Start", sequence: 1);
			var component12 = BMSTestHelper.CreateBuffer(system1, "Keep going", sequence: 2);
			var component13 = BMSTestHelper.CreateBucket(system1, "Finish", sequence: 3);

			BMSTestHelper.LinkComponents(component11, component12);
			BMSTestHelper.LinkComponents(component12, component13);

			var workflowType = system1.RelatedWorkflowTypes.AddNew();
			workflowType.FSW_WorkflowType = "ORG";

			var system2 = Factory.New<BMSystem>();
			system2.FS_Name = "Slow";

			var component21 = BMSTestHelper.CreateBucket(system2, "Staaaaaart", sequence: 1);
			var component22 = BMSTestHelper.CreateBuffer(system2, "Keeeeep gooooiiiing", sequence: 2);
			var component23 = BMSTestHelper.CreateBucket(system2, "Fiiiiiniiiish", sequence: 3);

			BMSTestHelper.LinkComponents(component21, component22);
			BMSTestHelper.LinkComponents(component22, component23);

			BMSTestHelper.LinkComponents(component23, component11);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			var templateTask = template.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.ApplyWorkflowTemplates();
			AssertEquals("Workflow was applied to the job", 1, job.WorkflowItems.Count);

			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders.Single();

			AssertEquals("Job header created from template should be linked to system 1", system1.PK, jobHeader.BMSystem.PK);
			AssertEquals("Workflow created from template should be linked to system 1", system1.PK, workflow.BMSystem.PK);

			AssertEquals("Entry component of system 1 should be selected despite another component from another system pointing to it",
				component11.PK, workflow.FH_FC_CurrentComponent);
		}

		public void TestTemplateWithWorkflowsNotApplied_WhenRelatedJobTypeIsDisabled()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "Fast";

			var workflowType = system.RelatedWorkflowTypes.Single();
			AssertEquals("The related workflow type should be an Organization", workflowType.FSW_WorkflowType, "ORG");

			workflowType.FSW_IsActive = false;

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			var templateTrigger = template.WorkflowItems.Triggers.AddNew();

			Factory.Save();

			var jobBefore = Factory.NewWithValidTestData<OrgHeader>();
			jobBefore.ApplyWorkflowTemplates();

			var jobHeaderBefore = ProcessJobHeader.GetForParent(jobBefore, Factory, addDefaultProcessHeaderIfNone: false);
			CombineAssertions(() =>
			{
				AssertEquals("The workflow should not be applied to the before job", 0, jobHeaderBefore.ProcessHeaders.Count);
				AssertEquals("The task should be applied to the before job", 1, jobBefore.WorkflowItems.Tasks.Count);
				AssertEquals("The milestone should be applied to the before job", 1, jobBefore.WorkflowItems.Milestones.Count);
				AssertEquals("The trigger should be applied to the before job", 1, jobBefore.WorkflowItems.Triggers.Count);
			});

			workflowType.FSW_IsActive = true;
			Factory.Save();

			var jobAfter = Factory.NewWithValidTestData<OrgHeader>();
			jobAfter.ApplyWorkflowTemplates();

			var jobHeaderAfter = ProcessJobHeader.GetForParent(jobAfter, Factory, addDefaultProcessHeaderIfNone: false);

			CombineAssertions(() =>
			{
				AssertEquals("The workflow should now be applied to the after job", 1, jobHeaderAfter.ProcessHeaders.Count);
				AssertEquals("The task should still be applied to the after job", 1, jobAfter.WorkflowItems.Tasks.Count);
				AssertEquals("The milestone should still be applied to the after job", 1, jobAfter.WorkflowItems.Milestones.Count);
				AssertEquals("The trigger should still be applied to the after job", 1, jobAfter.WorkflowItems.Triggers.Count);
			});
		}

		public void TestTemplateApplied_EvenWhenRelatedJobTypeAndBufferManagementIsDisabled()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "Fast";

			var workflowType = system.RelatedWorkflowTypes.Single();
			AssertEquals("The related workflow type should be an Organization", workflowType.FSW_WorkflowType, "ORG");

			workflowType.FSW_IsActive = false;

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			var templateTrigger = template.WorkflowItems.Triggers.AddNew();

			Factory.Save();

			BMSTestHelper.DisableBMSInRegistry();

			var jobBefore = Factory.NewWithValidTestData<OrgHeader>();
			jobBefore.ApplyWorkflowTemplates();

			var jobHeaderBefore = ProcessJobHeader.GetForParent(jobBefore, Factory, addDefaultProcessHeaderIfNone: false);
			CombineAssertions(() =>
			{
				AssertEquals("The workflow should NOT be applied to the before job", 0, jobHeaderBefore.ProcessHeaders.Count);
				AssertEquals("The task should not be applied to the before job", 1, jobBefore.WorkflowItems.Tasks.Count);
				AssertEquals("The milestone should not be applied to the before job", 1, jobBefore.WorkflowItems.Milestones.Count);
				AssertEquals("The trigger should not be applied to the before job", 1, jobBefore.WorkflowItems.Triggers.Count);
			});

			workflowType.FSW_IsActive = true;
			Factory.Save();

			var jobAfter = Factory.NewWithValidTestData<OrgHeader>();
			jobAfter.ApplyWorkflowTemplates();

			var jobHeaderAfter = ProcessJobHeader.GetForParent(jobAfter, Factory, addDefaultProcessHeaderIfNone: false);

			CombineAssertions(() =>
			{
				AssertEquals("The workflow should still NOT be applied to the after job", 0, jobHeaderAfter.ProcessHeaders.Count);
				AssertEquals("The task should now be applied to the after job", 1, jobAfter.WorkflowItems.Tasks.Count);
				AssertEquals("The milestone should now be applied to the after job", 1, jobAfter.WorkflowItems.Milestones.Count);
				AssertEquals("The trigger should now be applied to the after job", 1, jobAfter.WorkflowItems.Triggers.Count);
			});
		}

		#region GetSystemForWorkflowProvider

		public void TestGetSystemForWorkflowProvider()
		{
			var system = Factory.New<BMSystem>();
			system.FS_Name = "name";
			system.FS_Description = "desc";

			var workflowType1 = system.RelatedWorkflowTypes.AddNew();
			workflowType1.FSW_WorkflowType = "WKI"; // WorkItem

			var workflowType2 = system.RelatedWorkflowTypes.AddNew();
			workflowType2.FSW_WorkflowType = "WKP"; // Project

			var system2 = Factory.New<BMSystem>();
			system2.FS_Name = "name2";
			system2.FS_Description = "desc2";

			var workflowType3 = system2.RelatedWorkflowTypes.AddNew();
			workflowType3.FSW_WorkflowType = "ORD"; // Order

			var workflowType4 = system2.RelatedWorkflowTypes.AddNew();
			workflowType4.FSW_WorkflowType = "CON"; // Consol

			var workItem = (IWorkflowProviderCore)Factory.NewWithValidTestData(ObjectFactory.GetType<IWorkItem>());
			var project = (IWorkflowProviderCore)Factory.NewWithValidTestData(ObjectFactory.GetType<IProject>());
			var order = (IWorkflowProviderCore)Factory.NewWithValidTestData(ObjectFactory.GetType<IOrder>());
			var consol = (IWorkflowProviderCore)Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingConsol>());
			var container = (IWorkflowProviderCore)Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingContainer>());

			Factory.Save();

			AssertEquals(system, BMSystem.GetSystemForWorkflowProvider(workItem, Factory));
			AssertEquals(system, BMSystem.GetSystemForWorkflowProvider(project, Factory));
			AssertEquals(system2, BMSystem.GetSystemForWorkflowProvider(order, Factory));
			AssertEquals(system2, BMSystem.GetSystemForWorkflowProvider(consol, Factory));
			AssertEquals(null, BMSystem.GetSystemForWorkflowProvider(container, Factory));
		}

		public void TestGetSystemForWorkflowProvider_ShouldThrow_WhenProviderIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => BMSystem.GetSystemForWorkflowProvider(null, Factory));
		}

		#endregion

		#region GetSystemForWorkflowType

		public void TestGetSystemForWorkflowType()
		{
			var system = Factory.New<BMSystem>();
			system.FS_Name = "name";
			system.FS_Description = "desc";

			var workflowType1 = system.RelatedWorkflowTypes.AddNew();
			workflowType1.FSW_WorkflowType = "ABC";

			var workflowType2 = system.RelatedWorkflowTypes.AddNew();
			workflowType2.FSW_WorkflowType = "XYZ";

			var system2 = Factory.New<BMSystem>();
			system2.FS_Name = "name2";
			system2.FS_Description = "desc2";

			var workflowType3 = system2.RelatedWorkflowTypes.AddNew();
			workflowType3.FSW_WorkflowType = "ZAY";

			var workflowType4 = system2.RelatedWorkflowTypes.AddNew();
			workflowType4.FSW_WorkflowType = "RYL";

			Factory.Save();

			AssertEquals(system, BMSystem.GetSystemForWorkflowType("ABC", Factory));
			AssertEquals(system2, BMSystem.GetSystemForWorkflowType("ZAY", Factory));
			AssertEquals(system, BMSystem.GetSystemForWorkflowType("XYZ", Factory));
			AssertEquals(system2, BMSystem.GetSystemForWorkflowType("RYL", Factory));
			AssertEquals(null, BMSystem.GetSystemForWorkflowType("DUM", Factory));
		}

		#endregion

		public void TestBoards()
		{
			var system = Factory.New<BMSystem>();
			var board1 = Factory.New<BMBoard>();
			var board2 = Factory.New<BMBoard>();
			board1.MB_FS_System = system.PK;

			AssertEquals(1, system.Boards.Count);
			AssertCollectionContains(board1, system.Boards);
		}

		public void TestCustomisedCards_TaskCardSection()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory);
			var component1 = BMSTestHelper.CreateBucket(system1);
			var section1 = BMSTestHelper.CreateBoardSection(component1);

			var system2 = BMSTestHelper.CreateSystem(Factory);
			var component2 = BMSTestHelper.CreateBucket(system2);
			var section2 = BMSTestHelper.CreateBoardSection(component2);

			var taskDetailedCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			taskDetailedCard.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;
			var taskSummaryCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			taskSummaryCard.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;

			var workflowDetailedCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			workflowDetailedCard.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowDetailedCard;
			var workflowSummaryCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			workflowSummaryCard.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowSummaryCard;

			BMSTestHelper.CreateControlCustomisationLink(Factory, system1, taskDetailedCard);
			BMSTestHelper.CreateControlCustomisationLink(Factory, system1, taskSummaryCard);
			BMSTestHelper.CreateControlCustomisationLink(Factory, system1, workflowDetailedCard);
			BMSTestHelper.CreateControlCustomisationLink(Factory, system1, workflowSummaryCard);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section2.PK);

			var loadedTaskDetailedCard = BMControlCustomisation.GetCustomisedDetailedCard(loadedSection.SectionConfiguration, string.Empty);
			var loadedTaskSummaryCard = BMControlCustomisation.GetCustomisedSummaryCard(loadedSection.SectionConfiguration, string.Empty);

			AssertNull(loadedTaskDetailedCard);
			AssertNull(loadedTaskSummaryCard);

			loadedSection = newFactory.Load<BMBoardSection>(section1.PK);

			loadedTaskDetailedCard = BMControlCustomisation.GetCustomisedDetailedCard(loadedSection.SectionConfiguration, string.Empty);
			loadedTaskSummaryCard = BMControlCustomisation.GetCustomisedSummaryCard(loadedSection.SectionConfiguration, string.Empty);

			AssertNotNull(loadedTaskDetailedCard);
			AssertNotNull(loadedTaskSummaryCard);

			AssertEquals(taskDetailedCard.PK, loadedTaskDetailedCard.PK);
			AssertEquals(taskSummaryCard.PK, loadedTaskSummaryCard.PK);
		}

		public void TestCustomisedCards_WorkflowCardSection()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory);
			var component1 = BMSTestHelper.CreateBucket(system1);
			var section1 = BMSTestHelper.CreateBoardSection(component1);
			section1.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var system2 = BMSTestHelper.CreateSystem(Factory);
			var component2 = BMSTestHelper.CreateBucket(system2);
			var section2 = BMSTestHelper.CreateBoardSection(component2);
			section2.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var workflowDetailedCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			workflowDetailedCard.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowDetailedCard;
			var workflowSummaryCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			workflowSummaryCard.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowSummaryCard;

			BMSTestHelper.CreateControlCustomisationLink(Factory, system1, workflowDetailedCard);
			BMSTestHelper.CreateControlCustomisationLink(Factory, system1, workflowSummaryCard);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section2.PK);

			var loadedWorkflowDetailedCard = BMControlCustomisation.GetCustomisedDetailedCard(loadedSection.SectionConfiguration, string.Empty);
			var loadedWorkflowSummaryCard = BMControlCustomisation.GetCustomisedSummaryCard(loadedSection.SectionConfiguration, string.Empty);

			AssertNull(loadedWorkflowDetailedCard);
			AssertNull(loadedWorkflowSummaryCard);

			loadedSection = newFactory.Load<BMBoardSection>(section1.PK);

			loadedWorkflowDetailedCard = BMControlCustomisation.GetCustomisedDetailedCard(loadedSection.SectionConfiguration, string.Empty);
			loadedWorkflowSummaryCard = BMControlCustomisation.GetCustomisedSummaryCard(loadedSection.SectionConfiguration, string.Empty);

			AssertNotNull(loadedWorkflowDetailedCard);
			AssertNotNull(loadedWorkflowSummaryCard);

			AssertEquals(workflowDetailedCard.PK, loadedWorkflowDetailedCard.PK);
			AssertEquals(workflowSummaryCard.PK, loadedWorkflowSummaryCard.PK);
		}

		public void TestCustomisedCards_JobCardSection()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory);
			var component1 = BMSTestHelper.CreateBucket(system1);
			var section1 = BMSTestHelper.CreateBoardSection(component1);
			section1.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			var system2 = BMSTestHelper.CreateSystem(Factory);
			var component2 = BMSTestHelper.CreateBucket(system2);
			var section2 = BMSTestHelper.CreateBoardSection(component2);
			section2.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			var workflowDetailedCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			workflowDetailedCard.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowDetailedCard;
			var workflowSummaryCard = Factory.NewWithValidTestData<BMControlCustomisation>();
			workflowSummaryCard.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowSummaryCard;

			BMSTestHelper.CreateControlCustomisationLink(Factory, system1, workflowDetailedCard);
			BMSTestHelper.CreateControlCustomisationLink(Factory, system1, workflowSummaryCard);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(section2.PK);

			var loadedWorkflowDetailedCard = BMControlCustomisation.GetCustomisedDetailedCard(loadedSection.SectionConfiguration, string.Empty);
			var loadedWorkflowSummaryCard = BMControlCustomisation.GetCustomisedSummaryCard(loadedSection.SectionConfiguration, string.Empty);

			AssertNull(loadedWorkflowDetailedCard);
			AssertNull(loadedWorkflowSummaryCard);

			loadedSection = newFactory.Load<BMBoardSection>(section1.PK);

			loadedWorkflowDetailedCard = BMControlCustomisation.GetCustomisedDetailedCard(loadedSection.SectionConfiguration, string.Empty);
			loadedWorkflowSummaryCard = BMControlCustomisation.GetCustomisedSummaryCard(loadedSection.SectionConfiguration, string.Empty);

			AssertNotNull(loadedWorkflowDetailedCard);
			AssertNotNull(loadedWorkflowSummaryCard);

			AssertEquals(workflowDetailedCard.PK, loadedWorkflowDetailedCard.PK);
			AssertEquals(workflowSummaryCard.PK, loadedWorkflowSummaryCard.PK);
		}

		public void TestIRootTypeProvider_ProcessTask()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var task = orgHeader.WorkflowItems.AddNew();
			var taskAsRootTypeProvider = task as IRootTypeProvider;

			AssertArrayEqualsByElements("GIVEN No Valid BM System for process type, WHEN GetRootTypes, THEN should not have typeof(IProcessJobHeader)",
				new[] { typeof(OrgHeader), typeof(OrgHeaderProcessTask) },
				taskAsRootTypeProvider.RootTypes);

			AssertArrayEqualsByElements("GIVEN No Valid BM System for process type, WHEN GetRootsCore, THEN should not have ProcessJobHeaderProvider.GetForParent",
				new[] { (BusinessObject)orgHeader, task },
				taskAsRootTypeProvider.Roots);

			var orgSystem = BMSTestHelper.CreateSystem(Factory, "ORG");
			Factory.Save();

			AssertArrayEqualsByElements("GIVEN Valid BM System for process type, WHEN GetRootTypes, THEN should have typeof(IProcessJobHeader)",
				new[] { typeof(OrgHeader), typeof(OrgHeaderProcessTask), typeof(IProcessJobHeader) },
				taskAsRootTypeProvider.RootTypes);

			var processJobHeader = ProcessJobHeader.GetForParent(orgHeader, Factory);
			AssertArrayEqualsByElements("GIVEN Valid BM System for process type, WHEN GetRootsCore, THEN should have ProcessJobHeaderProvider.GetForParent",
				new[] { (BusinessObject)orgHeader, processJobHeader, task },
				taskAsRootTypeProvider.Roots);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			var templateTaskAsRootTypeProvider = templateTask as IRootTypeProvider;

			AssertArrayEqualsByElements("GIVEN Valid BM System for process type but a template, WHEN GetRootTypes, THEN should have typeof(IProcessJobHeader)",
				new[] { typeof(OrgHeader), typeof(IProcessJobHeader), typeof(TemplateProcessTask) },
				templateTaskAsRootTypeProvider.RootTypes);

			AssertEquals("GIVEN Valid BM System for process type but a template, WHEN GetRootsCore, THEN should not have ProcessJobHeaderProvider.GetForParent",
				0,
				templateTaskAsRootTypeProvider.Roots.Length);
		}

		public void TestIRootTypeProvider_ProcessTaskNotification()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var trigger = orgHeader.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			var actionAsRootTypeProvider = action as IRootTypeProvider;

			AssertArrayEqualsByElements("GIVEN No Valid BM System for process type, WHEN GetRootTypes, THEN should not have typeof(IProcessJobHeader)",
				new[] { typeof(OrgHeader), typeof(OrgHeaderProcessTask), typeof(ProcessTaskNotification), typeof(StmALog) },
				actionAsRootTypeProvider.RootTypes);

			AssertArrayEqualsByElements("GIVEN No Valid BM System for process type, WHEN GetRootsCore, THEN should not have ProcessJobHeaderProvider.GetForParent",
				new[] { (BusinessObject)orgHeader, trigger, action },
				actionAsRootTypeProvider.Roots);

			var orgSystem = BMSTestHelper.CreateSystem(Factory, "ORG");
			Factory.Save();

			AssertArrayEqualsByElements("GIVEN Valid BM System for process type, WHEN GetRootTypes, THEN should have typeof(IProcessJobHeader)",
				new[] { typeof(OrgHeader), typeof(OrgHeaderProcessTask), typeof(IProcessJobHeader), typeof(ProcessTaskNotification), typeof(StmALog) },
				actionAsRootTypeProvider.RootTypes);

			AssertArrayEqualsByElements("GIVEN Valid BM System for process type, WHEN GetRootsCore, THEN should have ProcessJobHeaderProvider.GetForParent",
				new[] { (BusinessObject)orgHeader, ProcessJobHeader.GetForParent(orgHeader, Factory), trigger, action },
				actionAsRootTypeProvider.Roots);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			var templateAction = templateTrigger.ProcessTaskNotifications.AddNew();
			var templateActionAsRootTypeProvider = templateAction as IRootTypeProvider;

			AssertArrayEqualsByElements("GIVEN Valid BM System for process type but a template, WHEN GetRootTypes, THEN should have typeof(IProcessJobHeader)",
				new[] { typeof(OrgHeader), typeof(IProcessJobHeader), typeof(TemplateProcessTask), typeof(ProcessTaskNotification), typeof(StmALog) },
				templateActionAsRootTypeProvider.RootTypes);

			AssertEquals("GIVEN Valid BM System for process type but a template, WHEN GetRootsCore, THEN should not have ProcessJobHeaderProvider.GetForParent",
				0,
				templateActionAsRootTypeProvider.Roots.Length);
		}

		#region Properties

		public void TestDefaultSystem_ShouldBeNotLive_Factory_New()
		{
			var system = Factory.New<BMSystem>();
			AssertEquals("We made a not-live system", false, system.FS_IsLive);
		}

		public void TestDefaultSystem_ShouldBeNotLive_FillWithValidTestData()
		{
			var system = Factory.New<BMSystem>();
			system.FillWithValidTestData();
			AssertEquals("We made a not-live system", false, system.FS_IsLive);
		}

		public void TestDefaultSystem_ShouldBeNotLive_BMSTestHelper_CreateSystem()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "HII");
			AssertEquals("We made a live system", true, system.FS_IsLive);
		}

		public void TestHumanReadableName()
		{
			var bmSystem = BMSTestHelper.CreateSystem(Factory);
			bmSystem.FS_Name = string.Empty;
			AssertEquals("Human readable name should be 'Buffer Management System - '", bmSystem.HumanReadableName, "Buffer Management System - ");

			bmSystem.FS_Name = "Name";
			AssertEquals("Human readable name should be 'Buffer Management System - Name'", bmSystem.HumanReadableName, "Buffer Management System - Name");
		}

		#endregion

		#region Update Related Workflows

		[TestDate(2025, 01, 24)]
		public void TestUpdateRelatedWorkflows()
		{
			const int nudgeDelay = 3; // 3 minute delay for nudging the BMS service task
			BMSRegistry.Instance.DelayForResponsiveWorkflowUpdatesOnRelatedObjectChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, nudgeDelay);
			BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false); // we need to ensure it turns to true

			var system1 = BMSTestHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);
			system1.FS_Name = "System 1";
			var component1_1 = BMSTestHelper.CreateBucket(system1, "bucket 1_1");
			var component1_2 = BMSTestHelper.CreateBucket(system1, "bucket 1_2");
			var component1_3 = BMSTestHelper.CreateBucket(system1, "bucket 1_3 (inactive)");
			component1_3.FC_IsActive = false;
			var component1_4 = BMSTestHelper.CreateBucket(system1, "bucket 1_4 (has just an inactive outgoing link)");
			var component1_5 = BMSTestHelper.CreateBucket(system1, "bucket 1_5 (sink)");
			BMSTestHelper.LinkComponents(component1_1, component1_2);
			BMSTestHelper.LinkComponents(component1_2, component1_3);
			BMSTestHelper.LinkComponents(component1_3, component1_4);
			BMSTestHelper.LinkComponents(component1_4, component1_5).FL_TransferRulesEnabled = false;

			var system2 = BMSTestHelper.CreateSystem(Factory, "ORG");
			system2.FS_Name = "System 2";
			var component2_1 = BMSTestHelper.CreateBucket(system2, "bucket 2_1");
			var component2_2 = BMSTestHelper.CreateBucket(system2, "bucket 2_2");
			BMSTestHelper.LinkComponents(component2_1, component2_2);

			new TimeActionScheduleCollection(Factory).DeleteAll();

			Factory.Save();

			var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
			using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
			{
				system1.UpdateRelatedWorkflows();

				var collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
				var scheduledComponentPKs = collection.Select(a => a.TAS_TargetPK);
				AssertContainsExactElementsInAnyOrder("Should schedule update actions for the components of the BMS", [component1_1.PK, component1_2.PK, component1_3.PK, component1_4.PK], scheduledComponentPKs);
				AssertCollectionContains("Should schedule for inactive components as well", component1_3.PK, scheduledComponentPKs);
				AssertCollectionNotContains("Should not schedule for components with no outgoing links", component1_5.PK, scheduledComponentPKs);
				AssertCollectionContains("Should still schedule for components that only have inactive outgoing links", component1_4.PK, scheduledComponentPKs);
				Assert("Should schedule actions targeting BMComponentLinks", collection.All(s => s.TAS_TargetTableCode == BMComponentSchema.Constants.Prefix));
				Assert("Should schedule actions with proper code", collection.All(s => s.TAS_ActionCode == "PHU"));
				Assert("Should schedule actions with proper parameter to update all properties", collection.All(s => s.TAS_JsonParameter == ProcessHeaderResponsiveActionConstants.UpdateAllExceptDedicatedBuffer));
				Assert("Should schedule actions as active", collection.All(s => s.TAS_ExecutionStatus == "SCH"));
				Assert("Should schedule without a token", collection.All(s => string.IsNullOrEmpty(s.TAS_Token)));
				Assert("Should schedule actions ready for execution", collection.All(s => s.TAS_ExecutionDateTimeUtc == ZDateTime.UtcNow));

				serviceTaskNudgerMock.Verify(nudger => nudger.NudgeServiceTask(TransferRuleRunnerServiceTask.Code, TimeSpan.FromMinutes(nudgeDelay)), Times.Once);
				Assert("Initiating the update should enable the registry item so that the nudged BMS processes all the links", BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.Value);

				new TimeActionScheduleCollection(new BusinessObjectFactory()).DeleteAll();

				system1.UpdateRelatedWorkflows();
				system1.UpdateRelatedWorkflows(); // let's do it twice to ensure we don't schedule updates multiple times when the user clicks the corresponding button multiple times

				collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
				AssertContainsExactElementsInAnyOrder("Should schedule update actions for the components of the BMS", [component1_1.PK, component1_2.PK, component1_3.PK, component1_4.PK], collection.Select(a => a.TAS_TargetPK));
				Assert("Should schedule actions targeting BMComponentLinks", collection.All(s => s.TAS_TargetTableCode == BMComponentSchema.Constants.Prefix));
				Assert("Should schedule actions with proper code", collection.All(s => s.TAS_ActionCode == "PHU"));
				Assert("Should schedule actions with proper parameter to update all properties", collection.All(s => s.TAS_JsonParameter == ProcessHeaderResponsiveActionConstants.UpdateAllExceptDedicatedBuffer));
				Assert("Should schedule actions as active", collection.All(s => s.TAS_ExecutionStatus == "SCH"));
				Assert("Should schedule without a token", collection.All(s => string.IsNullOrEmpty(s.TAS_Token)));
				Assert("Should schedule actions ready for execution", collection.All(s => s.TAS_ExecutionDateTimeUtc == ZDateTime.UtcNow));
			}
		}

		#endregion

		#region Logs

		public void TestNoStmALogs()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, system.PK);
			AssertEquals("Should not create Add event", 0, Factory.Load<StmALog>(query).Length);

			system.FS_Name = "New name";
			Factory.Save();
			AssertEquals("Should not create Edit event", 0, Factory.Load<StmALog>(query).Length);

			system.Delete();
			Factory.Save();
			AssertEquals("Should not create Delete event", 0, Factory.Load<StmALog>(query).Length);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}

		static void AssertComponentReplicatesItSelf(Dictionary<ZGuid, ZGuid> componentPKMapping, Dictionary<ZGuid, ZGuid> reversedPKMapping, BMComponent expectedComponent, BMComponent clonedComponent)
		{
			AssertEquals(expectedComponent.ZoneCapacityMultipliers.Count, clonedComponent.ZoneCapacityMultipliers.Count);

			AssertEquals(expectedComponent.FromOthersToMeLinks.Count, clonedComponent.FromOthersToMeLinks.Count);
			AssertEquals(expectedComponent.FromMeToOthersLinks.Count, clonedComponent.FromMeToOthersLinks.Count);

			AssertContainsExactElementsInAnyOrder(expectedComponent.FromMeToOthersLinks.Select(l => l.FL_FC_ComponentTo), clonedComponent.FromMeToOthersLinks.Select(l => reversedPKMapping[l.FL_FC_ComponentTo]));
			AssertContainsExactElementsInAnyOrder(expectedComponent.FromOthersToMeLinks.Select(l => l.FL_FC_ComponentFrom), clonedComponent.FromOthersToMeLinks.Select(l => reversedPKMapping[l.FL_FC_ComponentFrom]));

			AssertContainsExactElementsInAnyOrder(expectedComponent.FromMeToOthersLinks.Select(l => l.FilterRule != null ? l.FilterRule.S9_FilterData : null).WhereNotNull(), clonedComponent.FromMeToOthersLinks.Select(l => l.FilterRule != null ? l.FilterRule.S9_FilterData : null).WhereNotNull());
		}

		#endregion
	}
}

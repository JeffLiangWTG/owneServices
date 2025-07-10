using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class TagilatorMenuItemProviderTest : BMSTestCaseWithFactory
	{
		#region Adding tags

		public void TestAddTags_RemovesExclusiveTags()
		{
			var system = CreateSystem("ORG");

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "BAA", "Group 1");
			definition.TGD_IsExclusive = true;
			var mag1 = BMSTestHelper.CreateTagMagnitude(definition, "TAG", "Tag1");
			var mag2 = BMSTestHelper.CreateTagMagnitude(definition, "TAT", "Tag2");

			var definition2 = BMSTestHelper.CreateTagDefinition(Factory, "BAB", "Group 2");

			Factory.Save();

			AssertEquals(false, workflow1.TagLinks.Any());

			module.IDOverride = ModuleIDs.ProcessHeader;
			module.SelectedBusinessObjectsOverride = new[] { workflow1 };

			module.GridCollection.Add(workflow1);

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(2, menuItems.Length);

			var addTag = (TagMenuItemMenuTree)menuItems[0];
			addTag.OnPopup();

			module.SelectedBusinessObjectsOverride = new[] { workflow1, workflow2 };

			addTag.MenuItems[0].MenuItems[0].PerformClick();

			AssertEquals(1, workflow1.TagLinks.Count);
			AssertEquals(1, workflow2.TagLinks.Count);
			AssertEquals(mag1.PK, workflow1.TagLinks.First().TGL_TGM_Magnitude);

			addTag.MenuItems[0].MenuItems[1].PerformClick();

			AssertEquals(1, workflow1.TagLinks.Count);
			AssertEquals(1, workflow2.TagLinks.Count);
			AssertEquals(mag2.PK, workflow1.TagLinks.First().TGL_TGM_Magnitude);
		}

		public void TestAddTag_ShouldExcludeInactiveTags()
		{
			var system = CreateSystem("ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "BAA", "Group 1");
			var mag1 = BMSTestHelper.CreateTagMagnitude(definition, "TAG", "Tag1");
			var mag2 = BMSTestHelper.CreateTagMagnitude(definition, "TAT", "Tag2", isActive: false);

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessHeader;
			module.SelectedBusinessObjectsOverride = new[] { jobHeader };

			module.GridCollection.Add(jobHeader);

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(2, menuItems.Length);

			var addTag = (TagMenuItemMenuTree)menuItems[0];
			addTag.OnPopup();

			AssertEquals("BAA - Group 1", addTag.MenuItems[0].Text);
			AssertEquals("Should exclude inactive tags", 1, addTag.MenuItems[0].MenuItems.Count);
			AssertEquals("TAG - Tag1", addTag.MenuItems[0].MenuItems[0].Text);
		}

		public void TestAddTag_ToOperationalBusinessObject_ShouldAddToJobHeader()
		{
			var factory = module.GridCollection.Factory;
			var system = BMSTestHelper.CreateSystem(factory, "ORG");
			var org1 = factory.NewWithValidTestData<OrgHeader>();
			var org2 = factory.NewWithValidTestData<OrgHeader>();
			var org3 = factory.NewWithValidTestData<OrgHeader>();

			var definition = BMSTestHelper.CreateTagDefinition(factory, "AAA", "aaa");
			var tag1 = BMSTestHelper.CreateTagMagnitude(definition, "111");
			var tag2 = BMSTestHelper.CreateTagMagnitude(definition, "222");
			var tag3 = BMSTestHelper.CreateTagMagnitude(definition, "333");

			factory.Save();

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(2, menuItems.Length);

			var addTag = (TagMenuItemMenuTree)menuItems[0];
			addTag.OnPopup();

			module.SelectedBusinessObjectsOverride = new[] { org1, org2 };
			addTag.MenuItems[0].MenuItems[0].PerformClick();

			var jobHeader1 = ProcessJobHeader.GetForParentWithoutCreation(org1, factory);
			var jobHeader2 = ProcessJobHeader.GetForParentWithoutCreation(org2, factory);
			var jobHeader3 = ProcessJobHeader.GetForParentWithoutCreation(org3, factory);

			AssertNotNull(jobHeader1);
			AssertNotNull(jobHeader2);
			AssertNull(jobHeader3);

			AssertTagApplied(jobHeader1, tag1);
			AssertTagApplied(jobHeader2, tag1);

			module.SelectedBusinessObjectsOverride = new[] { org2, org3 };
			addTag.MenuItems[0].MenuItems[2].PerformClick();

			var newFactory = factory.CreateNewFactory();
			jobHeader1 = newFactory.Load<ProcessJobHeader>(jobHeader1.PK);
			jobHeader2 = newFactory.Load<ProcessJobHeader>(jobHeader2.PK);
			jobHeader3 = ProcessJobHeader.GetForParentWithoutCreation(org3, newFactory);
			AssertNotNull("jobHeader3 should have been created when tagging job", jobHeader3);

			AssertTagApplied(jobHeader1, tag1);
			AssertTagApplied(jobHeader2, tag1);
			AssertTagNotApplied(jobHeader3, tag1);

			AssertTagNotApplied(jobHeader1, tag3);
			AssertTagApplied(jobHeader2, tag3);
			AssertTagApplied(jobHeader3, tag3);
		}

		public void TestAddTag_ToOperationalBusinessObject_WorkQueue_ShouldAddToJobHeader()
		{
			var factory = module.GridCollection.Factory;
			var system = BMSTestHelper.CreateSystem(factory, "ORG");
			var org1 = factory.NewWithValidTestData<OrgHeader>();
			var org2 = factory.NewWithValidTestData<OrgHeader>();
			var org3 = factory.NewWithValidTestData<OrgHeader>();

			var queue = BMSTestHelper.CreateWorkQueue(factory, "AAA", "aaa");

			factory.Save();

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(2, menuItems.Length);

			var addTag = (TagMenuItemMenuTree)menuItems[0];
			addTag.OnPopup();

			module.SelectedBusinessObjectsOverride = new[] { org1, org2 };
			addTag.MenuItems[0].MenuItems[0].PerformClick();

			var jobHeader1 = ProcessJobHeader.GetForParentWithoutCreation(org1, factory);
			var jobHeader2 = ProcessJobHeader.GetForParentWithoutCreation(org2, factory);
			var jobHeader3 = ProcessJobHeader.GetForParentWithoutCreation(org3, factory);

			AssertNotNull(jobHeader1);
			AssertNotNull(jobHeader2);
			AssertNull(jobHeader3);

			AssertTagApplied(jobHeader1, queue);
			AssertTagApplied(jobHeader2, queue);

			module.SelectedBusinessObjectsOverride = new[] { org2, org3 };
			addTag.MenuItems[0].MenuItems[0].PerformClick();

			var newFactory = factory.CreateNewFactory();
			jobHeader1 = newFactory.Load<ProcessJobHeader>(jobHeader1.PK);
			jobHeader2 = newFactory.Load<ProcessJobHeader>(jobHeader2.PK);
			jobHeader3 = ProcessJobHeader.GetForParentWithoutCreation(org3, newFactory);
			AssertNotNull("jobHeader3 should have been created when adding job to queue", jobHeader3);

			AssertTagApplied(jobHeader1, queue);
			AssertTagApplied(jobHeader2, queue);
			AssertTagApplied(jobHeader3, queue);
		}

		public void TestAddTag_ToOperationalBusinessObject_WhenNoJobHeaderExists_TemplateApplicationShouldStillWork()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var org = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			AssertNull(ProcessJobHeader.GetForParentWithoutCreation(org, Factory));

			var addTag = (TagMenuItemMenuTree)provider.GetMenuItems(module).First();
			addTag.OnPopup();

			module.SelectedBusinessObjectsOverride = new[] { org };
			addTag.MenuItems.Cast<ZMenuItem>().Single(i => i.Text == "PRI - Priority Tags").MenuItems[0].PerformClick();

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(org, Factory);
			AssertNotNull(jobHeader);
			AssertEquals(0, jobHeader.ProcessHeaders.Count);
			AssertTagApplied(jobHeader, config.GoldTag);
			AssertEquals(0.0m, jobHeader.FH_TimeDelayFactor);
			AssertEquals(0, jobHeader.FH_TimeDelayMinutes);
			AssertEquals(false, jobHeader.FH_AllowTaskAutoAssignment);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			var template = BMSTestHelper.CreateWorkflowTemplate(newFactory, "ORG");
			var templateJobHeader = template.GetJobHeader();
			templateJobHeader.FH_TimeDelayFactor = 2.0;
			templateJobHeader.FH_TimeDelayMinutes = 10;
			templateJobHeader.FH_AllowTaskAutoAssignment = true;
			var templateWorkflow = template.ProcessHeaders.AddNew();
			templateWorkflow.FH_CompletionStatement = "Dis Workflow";
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_FH_ProcessHeader = templateWorkflow.PK;

			newFactory.Save();

			org = newFactory.Load<OrgHeader>(org.PK);
			new ProcessTask.Loader(newFactory).CreateTasksAndMilestonesFromTemplateIfRequired(org, TemplateApplicationParameters.ApplyIgnoreHasChanges());

			newFactory.Save();

			jobHeader = newFactory.Load<ProcessJobHeader>(jobHeader.PK);

			AssertTagApplied("Tag should still be applied", jobHeader, config.GoldTag);
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals("Dis Workflow", jobHeader.ProcessHeaders[0].FH_CompletionStatement);
			AssertEquals(2.0m, jobHeader.FH_TimeDelayFactor);
			AssertEquals(10, jobHeader.FH_TimeDelayMinutes);
			AssertEquals(true, jobHeader.FH_AllowTaskAutoAssignment);
		}

		public void TestAddTag_ToOperationalBusinessObject_NewWorkQueue()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			ProcessJobHeader.GetForParent(org, Factory);

			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			Factory.Save();

			var addTag = (TagMenuItemMenuTree)provider.GetMenuItems(module).First();
			addTag.OnPopup();

			module.SelectedBusinessObjectsOverride = new[] { org };

			var workQueueMenuItem = addTag.MenuItems.FindByText("QUE - Work Queues");
			AssertEquals(3, workQueueMenuItem.MenuItems.Count);
			AssertEquals("AAA - aaa", workQueueMenuItem.MenuItems[0].Text);
			AssertEquals("-", workQueueMenuItem.MenuItems[1].Text);
			AssertEquals("New...", workQueueMenuItem.MenuItems[2].Text);

			AssertNoExceptionThrown("Clicking the separator should be safe", () => workQueueMenuItem.MenuItems[1].PerformClick());
			workQueueMenuItem.MenuItems[2].PerformClick();

			WorkQueue newQueue;

			using (var workQueueForm = Application.OpenForms.OfType<WorkQueueForm>().Single())
			{
				newQueue = (WorkQueue)workQueueForm.BusinessEntity;

				newQueue.TGM_Code = "BBB";
				newQueue.TGM_Description = "bbb";

				AssertSaved(workQueueForm.FireSaveButton());
			}

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(org, Factory);

			AssertNotNull(jobHeader);
			Assert(newQueue.ContainsMember(jobHeader));

			addTag.OnPopup();
			workQueueMenuItem = addTag.MenuItems.FindByText("QUE - Work Queues");
			AssertEquals("New queue should be present in the menu", 4, workQueueMenuItem.MenuItems.Count);

			AssertEquals("AAA - aaa", workQueueMenuItem.MenuItems[0].Text);
			AssertEquals("BBB - bbb", workQueueMenuItem.MenuItems[1].Text);
			AssertEquals("-", workQueueMenuItem.MenuItems[2].Text);
			AssertEquals("New...", workQueueMenuItem.MenuItems[3].Text);
		}

		public void TestAddTag_BatchTagAdderDbHits_Headers()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader4, "workflow4");
			Factory.Save();

			AssertBatchAddTags_DbHits(new[] { workflow1, workflow2, workflow3, workflow4 }, new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 1 },
				{ BMNCNShapeSchema.Constants.TableName, 0 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 5 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 16 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ TagDefinitionSchema.Constants.TableName, 1 },
				{ TagLinkSchema.Constants.TableName, 9 },
				{ TagMagnitudeSchema.Constants.TableName, 1 },
			});
		}

		public void TestAddTag_ToOperationalBusinessObject_NewWorkQueue_CancelCreatingQueue()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			ProcessJobHeader.GetForParent(org, Factory);

			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			Factory.Save();

			var addTag = (TagMenuItemMenuTree)provider.GetMenuItems(module).First();
			addTag.OnPopup();

			module.SelectedBusinessObjectsOverride = new[] { org };

			var workQueueMenuItem = addTag.MenuItems.FindByText("QUE - Work Queues");
			AssertEquals(3, workQueueMenuItem.MenuItems.Count);
			AssertEquals("AAA - aaa", workQueueMenuItem.MenuItems[0].Text);
			AssertEquals("-", workQueueMenuItem.MenuItems[1].Text);
			AssertEquals("New...", workQueueMenuItem.MenuItems[2].Text);

			AssertNoExceptionThrown("Clicking the separator should be safe", () => workQueueMenuItem.MenuItems[1].PerformClick());
			workQueueMenuItem.MenuItems[2].PerformClick();

			WorkQueue newQueue;

			using (var workQueueForm = Application.OpenForms.OfType<WorkQueueForm>().Single())
			{
				newQueue = (WorkQueue)workQueueForm.BusinessEntity;

				newQueue.TGM_Code = "BBB";
				newQueue.TGM_Description = "bbb";
			}

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(org, Factory);

			AssertNotNull(jobHeader);
			Assert(newQueue.ContainsMember(jobHeader));

			addTag.OnPopup();
			workQueueMenuItem = addTag.MenuItems.FindByText("QUE - Work Queues");
			AssertEquals("New queue should NOT be present in the menu", 3, workQueueMenuItem.MenuItems.Count);

			AssertEquals("AAA - aaa", workQueueMenuItem.MenuItems[0].Text);
			AssertEquals("-", workQueueMenuItem.MenuItems[1].Text);
			AssertEquals("New...", workQueueMenuItem.MenuItems[2].Text);
		}

		public void TestAddTag_ToOperationalBusinessObject_WorkQueue_ShouldUseQueueMembershipValidation()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MAIORGSYD";

			var jobHeader = ProcessJobHeader.GetForParent(org, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow");

			var result = workflow.AddTag(queue);
			AssertEquals(true, result.WasSuccessful);

			Factory.Save();

			var addTag = (TagMenuItemMenuTree)provider.GetMenuItems(module).First();
			addTag.OnPopup();
			module.SelectedBusinessObjectsOverride = new[] { org };

			var workQueueMenuItem = addTag.MenuItems.FindByText("QUE - Work Queues");
			AssertEquals("AAA - aaa", workQueueMenuItem.MenuItems[0].Text);

			workQueueMenuItem.MenuItems[0].PerformClick();

			var loadedJobHeader = Factory.CreateNewFactory().Load<ProcessJobHeader>(jobHeader.PK);
			AssertTagNotApplied(loadedJobHeader, queue);
			AssertMultilineASCIIEquals("",
@"The selected item could not be added to the queue.

Cannot add the job of a workflow already in the queue. The following workflows are already present in this queue:
	workflow (Organization (MAIORGSYD) - Job Organization (MAIORGSYD) is complete.)", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestAddTagMenuItem_ShouldExcludeGroupsWithNoActiveTags()
		{
			var definitionWithMagnitudes = BMSTestHelper.CreateTagDefinition(Factory, "DE1", "Definition 1");
			BMSTestHelper.CreateTagMagnitude(definitionWithMagnitudes, "MG1", "Magnitude 1");

			var definitionWithNoActiveMagnitudes = BMSTestHelper.CreateTagDefinition(Factory, "DE2", "Definition 2");
			BMSTestHelper.CreateTagMagnitude(definitionWithNoActiveMagnitudes, "MG2", "Magnitude 2", isActive: false);

			var definitionWithNoMagnitudes = BMSTestHelper.CreateTagDefinition(Factory, "DE3", "Definition 3");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessHeader;
			module.SelectedBusinessObjectsOverride = new[] { workflow };
			module.GridCollection.Add(workflow);

			var menuItems = provider.GetMenuItems(module).ToArray();
			var addTag = (TagMenuItemMenuTree)menuItems[0];
			AssertEquals("Add Tag", addTag.Text);
			addTag.OnPopup();

			AssertEquals(true, definitionWithMagnitudes.Magnitudes.Any(m => m.TGM_IsActive));
			AssertNotNull("Groups with active tags should be in the menu 'Add Tag'.", addTag.MenuItems.FindByText(definitionWithMagnitudes.DisplayText));

			AssertEquals(false, definitionWithNoActiveMagnitudes.Magnitudes.Any(m => m.TGM_IsActive));
			AssertNull("Groups with no active tags should be in the menu 'Add Tag'.", addTag.MenuItems.FindByText(definitionWithNoActiveMagnitudes.DisplayText));

			AssertEquals(false, definitionWithNoMagnitudes.Magnitudes.Any());
			AssertNull("Groups with no tags should not be in the menu 'Add Tag'.", addTag.MenuItems.FindByText(definitionWithNoMagnitudes.DisplayText));
		}

		void AssertBatchAddTags_DbHits(ITagable[] processHeaders, Dictionary<string, int> allowedDbHits)
		{
			var newFactory = module.GridCollection.Factory;
			newFactory.ResetDatabaseLoadCount();

			var addTag = (TagMenuItemMenuTree)provider.GetMenuItems(module).First();
			addTag.OnPopup();

			module.SelectedBusinessObjectsOverride = processHeaders.Cast<BusinessObject>().ToArray();

			var workQueueMenuItem = addTag.MenuItems.FindByText("QUE - Work Queues");
			AssertEquals(3, workQueueMenuItem.MenuItems.Count);
			workQueueMenuItem.MenuItems[0].PerformClick();

			AssertDbHits(allowedDbHits, newFactory);
		}

		#endregion

		#region Removing tags

		public void TestRemoveTag_ShouldExcludeInactiveTags()
		{
			var system = CreateSystem("ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "BAA", "Group 1");
			var mag1 = BMSTestHelper.CreateTagMagnitude(definition, "TAG", "Tag1");
			var mag2 = BMSTestHelper.CreateTagMagnitude(definition, "TAT", "Tag2", isActive: false);

			jobHeader.AddTag(mag2);

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessHeader;
			module.SelectedBusinessObjectsOverride = new[] { jobHeader };

			module.GridCollection.Add(jobHeader);

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(2, menuItems.Length);

			var removeTag = (TagMenuItemMenuTree)menuItems[1];
			removeTag.OnPopup();

			AssertEquals("BAA - Group 1", removeTag.MenuItems[0].Text);
			AssertEquals("Should contain inactive tags currently applied", 1, removeTag.MenuItems[0].MenuItems.Count);
			AssertEquals("TAT - Tag2", removeTag.MenuItems[0].MenuItems[0].Text);
		}

		public void TestRemoveTag_FromOperationalBusinessObject_ShouldRemoveFromJobHeader()
		{
			var system = CreateSystem("ORG");
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var jobHeader1 = ProcessJobHeader.GetForParent(org1, Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(org2, Factory);

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "AAA", "aaa");
			var tag1 = BMSTestHelper.CreateTagMagnitude(definition, "111", "one");
			var tag2 = BMSTestHelper.CreateTagMagnitude(definition, "222", "two");
			var tag3 = BMSTestHelper.CreateTagMagnitude(definition, "333", "three");

			jobHeader1.AddTag(tag1);
			jobHeader1.AddTag(tag2);
			jobHeader2.AddTag(tag2);

			Factory.Save();

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(2, menuItems.Length);

			module.SelectedBusinessObjectsOverride = new[] { org1, org2, org3 };

			var removeTag = (TagMenuItemMenuTree)menuItems[1];
			AssertEquals("Remove Tag", removeTag.Text);
			removeTag.OnPopup();

			AssertEquals("111 - one", removeTag.MenuItems[0].MenuItems[0].Text);
			removeTag.MenuItems[0].MenuItems[0].PerformClick();

			var jobHeader3 = ProcessJobHeader.GetForParentWithoutCreation(org3, Factory);

			AssertNull(jobHeader3);

			AssertTagNotApplied(jobHeader1, tag1);
			AssertTagNotApplied(jobHeader2, tag1);

			AssertTagApplied(jobHeader1, tag2);
			AssertTagApplied(jobHeader2, tag2);

			module.SelectedBusinessObjectsOverride = new[] { org2 };
			removeTag.OnPopup();
			AssertEquals("222 - two", removeTag.MenuItems[0].MenuItems[0].Text);
			removeTag.MenuItems[0].MenuItems[0].PerformClick();

			AssertTagApplied(jobHeader1, tag2);
			AssertTagNotApplied(jobHeader2, tag2);
		}

		public void TestRemoveTag_ForItemInWorkQueue_ShouldLeaveSequenceNumbersAsEntered()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4");
			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow5");

			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var link1 = queue.AddMember(workflow5).Link;
			var link2 = queue.AddMember(workflow4).Link;
			var link3 = queue.AddMember(workflow3).Link;
			var link4 = queue.AddMember(workflow2).Link;
			var link5 = queue.AddMember(workflow1).Link;

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessHeader;
			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(2, menuItems.Length);

			module.SelectedBusinessObjectsOverride = new[] { workflow3 };

			var removeTag = (TagMenuItemMenuTree)menuItems[1];
			AssertEquals("Remove Tag", removeTag.Text);
			removeTag.OnPopup();

			AssertEquals("AAA - aaa", removeTag.MenuItems[0].MenuItems[0].Text);
			removeTag.MenuItems[0].MenuItems[0].PerformClick();

			var loadedQueue = Factory.CreateNewFactory().Load<WorkQueue>(queue.PK);
			BMSTestCaseWithFactory.AssertSamePK(workflow5, loadedQueue.MembersInSequence.ElementAt(0));
			BMSTestCaseWithFactory.AssertSamePK(workflow4, loadedQueue.MembersInSequence.ElementAt(1));
			BMSTestCaseWithFactory.AssertSamePK(workflow2, loadedQueue.MembersInSequence.ElementAt(2));
			BMSTestCaseWithFactory.AssertSamePK(workflow1, loadedQueue.MembersInSequence.ElementAt(3));

			AssertEquals(4, loadedQueue.Members.Count);

			AssertEquals((short)1, loadedQueue.Members.Single(l => l.TGL_ParentId == workflow5.PK).TGL_Sequence);
			AssertEquals((short)2, loadedQueue.Members.Single(l => l.TGL_ParentId == workflow4.PK).TGL_Sequence);
			AssertEquals((short)4, loadedQueue.Members.Single(l => l.TGL_ParentId == workflow2.PK).TGL_Sequence);
			AssertEquals((short)5, loadedQueue.Members.Single(l => l.TGL_ParentId == workflow1.PK).TGL_Sequence);
		}

		public void TestRemoveTagMenuItem_NoTagsApplied()
		{
			var system = CreateSystem("ORG");
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader1 = ProcessJobHeader.GetForParent(org1, Factory);

			Factory.Save();

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(2, menuItems.Length);

			var removeTag = (TagMenuItemMenuTree)menuItems[1];
			AssertEquals("Remove Tag", removeTag.Text);
			removeTag.OnPopup();

			AssertEquals("There are no tags applied", removeTag.MenuItems[0].Text);
			AssertEquals(false, removeTag.MenuItems[0].Enabled);
		}

		#endregion

		#region DB Hits On Menu Opening

		public void TestAddTagMenuItem_ShouldReloadTagMagnitudesOnlyOnce_OnMenuOpening()
		{
			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow 2");

			BulkCreateTagDefinitionsAndMagnitudes();

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessHeader;
			module.SelectedBusinessObjectsOverride = new[] { workflow1, workflow2 };
			module.GridCollection.Add(workflow1);
			module.GridCollection.Add(workflow2);

			var menuItems = provider.GetMenuItems(module).ToArray();
			var addTag = (TagMenuItemMenuTree)menuItems[0];
			AssertEquals("Add Tag", addTag.Text);

			var removeTag = (TagMenuItemMenuTree)menuItems[1];
			AssertEquals("Remove Tag", removeTag.Text);

			using (AssertDbHitsForAllFactories("Tag magnitudes should be reloaded only once on menu opening to ensure they are up to date (1 DB hit).", new Dictionary<string, int>
			{
				{ TagDefinitionSchema.Constants.TableName, 1 },
				{ TagMagnitudeSchema.Constants.TableName, 2 },
				{ TagLinkSchema.Constants.TableName, 2 }
			}))
			{
				addTag.OnPopup();
				removeTag.OnPopup();
			}
		}

		#endregion

		#region Module applicability

		public void TestShouldNotDisplayWhenBMIsDisabled()
		{
			BMSTestHelper.DisableBMSInRegistry();
			var system = CreateSystem("ORG");

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			definition.TGD_Code = "BAA";
			definition.TGD_Description = "Group 1";
			var mag1 = BMSTestHelper.CreateTagMagnitude(definition, "TAG", "Tag1");
			var mag2 = BMSTestHelper.CreateTagMagnitude(definition, "TAT", "Tag2");

			var definition2 = Factory.NewWithValidTestData<TagDefinition>();
			definition2.TGD_Description = "Group 2";
			definition2.TGD_Description = "BAB";

			Factory.Save();

			module.IDOverride = ModuleIDs.Organisation;

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(0, menuItems.Length);
		}

		public void TestShouldNotDisplayForRandomModules()
		{
			var system = CreateSystem("WKI");

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			definition.TGD_Code = "BAA";
			definition.TGD_Description = "Group 1";
			var mag1 = BMSTestHelper.CreateTagMagnitude(definition, "TAG", "Tag1");
			var mag2 = BMSTestHelper.CreateTagMagnitude(definition, "TAT", "Tag2");

			var definition2 = Factory.NewWithValidTestData<TagDefinition>();
			definition2.TGD_Description = "Group 2";
			definition2.TGD_Description = "BAB";

			Factory.Save();

			module.IDOverride = ModuleIDs.Organisation;

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(0, menuItems.Length);
		}

		public void TestShouldDisplayForModulePartOfBufferManagementSystem()
		{
			var system = CreateSystem("ORG");

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "AAA", "aaa");
			var tag1 = BMSTestHelper.CreateTagMagnitude(definition, "111");
			var tag2 = BMSTestHelper.CreateTagMagnitude(definition, "222");

			Factory.Save();

			module.IDOverride = ModuleIDs.Organisation;
			AssertEquals(2, provider.GetMenuItems(module).ToArray().Length);
		}

		#endregion

		#region Menu structure

		public void TestShouldOpenTagApplicationMenu()
		{
			var system = CreateSystem("ORG");

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			definition.TGD_Code = "GR1";
			definition.TGD_Description = "Group 1";
			var mag1 = BMSTestHelper.CreateTagMagnitude(definition, "TAG", "Tag1");
			var mag2 = BMSTestHelper.CreateTagMagnitude(definition, "TAT", "Tag2");

			var definition2 = Factory.NewWithValidTestData<TagDefinition>();
			definition2.TGD_Code = "GR2";
			definition2.TGD_Description = "Group 2";
			var mag3 = BMSTestHelper.CreateTagMagnitude(definition2, "TAN", "Tag3");

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessHeader;
			module.SelectedBusinessObjectsOverride = System.Array.Empty<ProcessHeader>();

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(2, menuItems.Length);

			var addTag = (TagMenuItemMenuTree)menuItems[0];
			addTag.OnPopup();

			AssertCollectionContains(addTag.MenuItems.Cast<MenuItem>(), t => t.Text == definition.DisplayText);
			AssertCollectionContains(addTag.MenuItems.Cast<MenuItem>(), t => t.Text == definition2.DisplayText);
			AssertEquals("TAG - Tag1", addTag.MenuItems.Cast<MenuItem>().Single(t => t.Text == definition.DisplayText).MenuItems[0].Text);
			AssertEquals("TAT - Tag2", addTag.MenuItems.Cast<MenuItem>().Single(t => t.Text == definition.DisplayText).MenuItems[1].Text);
		}

		public void TestMenu_DontProvideRuleTags()
		{
			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "NOG", "Nogg");
			var mag1 = BMSTestHelper.CreateTagMagnitude(definition1, "THI", "Thin Tag");

			var definition2 = BMSTestHelper.CreateTagDefinition(Factory, "EGG", "Egg");
			var mag2 = BMSTestHelper.CreateTagMagnitude(definition2, "FAT", "Fat Tag");

			var system = CreateSystem("ORG");
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, string.Empty, 30);

			task.AddTag(mag1);
			task.AddTag(mag2);

			Factory.Save();

			definition2.TGD_UsageScope = TagUsageScopeList.Codes.Rule;

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessHeader;

			module.SelectedBusinessObjectsOverride = new[] { task };

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(2, menuItems.Length);

			var addTag = (TagMenuItemMenuTree)menuItems[0];
			addTag.OnPopup();
			AssertCollectionContains(addTag.MenuItems.OfType<MenuItem>(), m => m.Text == "NOG - Nogg");
			AssertCollectionNotContains(addTag.MenuItems.OfType<MenuItem>(), m => m.Text == "EGG - Egg");

			var removeTag = (TagMenuItemMenuTree)menuItems[1];
			removeTag.OnPopup();

			AssertCollectionContains(addTag.MenuItems.OfType<MenuItem>(), m => m.Text == "NOG - Nogg");
			AssertCollectionNotContains(addTag.MenuItems.OfType<MenuItem>(), m => m.Text == "EGG - Egg");
		}

		#region Inclusion And Exclusion Of Work Queues For Different Workflow Management Modes

		#region Related Modules

		public void TestAddTagMenuItem_ShouldIncludeWorkQueues_ForRelatedModules_WhenWorkflowManagementModeIsSetToPLN()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry("PLN");

			var definitionWithMagnitudes = BMSTestHelper.CreateTagDefinition(Factory, "DE1", "Definition 1");
			BMSTestHelper.CreateTagMagnitude(definitionWithMagnitudes, "MG1", "Magnitude 1");
			BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var system = CreateSystem("ORG");
			var org = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			module.IDOverride = ModuleIDs.Organisation;
			module.SelectedBusinessObjectsOverride = new[] { org };
			module.GridCollection.Add(org);

			var menuItems = provider.GetMenuItems(module).ToArray();
			var addTag = (TagMenuItemMenuTree)menuItems.SingleOrDefault(m => m.Text == "Add Tag");
			AssertNotNull(addTag);
			addTag.OnPopup();

			AssertEquals(2, addTag.MenuItems.Count);
			var subMenuItem = addTag.MenuItems.Cast<ZMenuItem>().ToArray();
			AssertNotNull(subMenuItem.SingleOrDefault(m => m.Text.Contains("DE1")));
			AssertNotNull("Should contain work queue tags", subMenuItem.SingleOrDefault(m => m.Text.Contains("QUE")));
		}

		public void TestAddTagMenuItem_ShouldExcludeWorkQueues_ForRelatedModules_WhenWorkflowManagementModeIsSetToBUF()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry("BUF");

			var definitionWithMagnitudes = BMSTestHelper.CreateTagDefinition(Factory, "DE1", "Definition 1");
			BMSTestHelper.CreateTagMagnitude(definitionWithMagnitudes, "MG1", "Magnitude 1");
			BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var system = CreateSystem("ORG");
			var org = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			module.IDOverride = ModuleIDs.Organisation;
			module.SelectedBusinessObjectsOverride = new[] { org };
			module.GridCollection.Add(org);

			var menuItems = provider.GetMenuItems(module).ToArray();
			var addTag = (TagMenuItemMenuTree)menuItems.SingleOrDefault(m => m.Text == "Add Tag");
			AssertNotNull(addTag);
			addTag.OnPopup();

			AssertEquals(1, addTag.MenuItems.Count);
			var subMenuItem = addTag.MenuItems.Cast<ZMenuItem>().ToArray();
			AssertNotNull(subMenuItem.SingleOrDefault(m => m.Text.Contains("DE1")));
			AssertNull("Should not contain work queue tags", subMenuItem.SingleOrDefault(m => m.Text.Contains("QUE")));
		}

		public void TestAddTagMenuItem_ShouldExcludeWorkQueues_ForRelatedModules_WhenWorkflowManagementModeIsSetToEWF()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry("EWF");

			var definitionWithMagnitudes = BMSTestHelper.CreateTagDefinition(Factory, "DE1", "Definition 1");
			BMSTestHelper.CreateTagMagnitude(definitionWithMagnitudes, "MG1", "Magnitude 1");
			BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var system = CreateSystem("ORG");
			var org = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			module.IDOverride = ModuleIDs.Organisation;
			module.SelectedBusinessObjectsOverride = new[] { org };
			module.GridCollection.Add(org);

			var menuItems = provider.GetMenuItems(module).ToArray();
			var addTag = (TagMenuItemMenuTree)menuItems.SingleOrDefault(m => m.Text == "Add Tag");
			AssertNotNull(addTag);
			addTag.OnPopup();

			AssertEquals(1, addTag.MenuItems.Count);
			var subMenuItem = addTag.MenuItems.Cast<ZMenuItem>().ToArray();
			AssertNotNull(subMenuItem.SingleOrDefault(m => m.Text.Contains("DE1")));
			AssertNull("Should not contain work queue tags", subMenuItem.SingleOrDefault(m => m.Text.Contains("QUE")));
		}

		public void TestAddTagMenuItem_ShouldNotShow_ForRelatedModules_WhenWorkflowManagementModeIsSetToBWF()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry("BWF");

			var definitionWithMagnitudes = BMSTestHelper.CreateTagDefinition(Factory, "DE1", "Definition 1");
			BMSTestHelper.CreateTagMagnitude(definitionWithMagnitudes, "MG1", "Magnitude 1");
			BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var system = CreateSystem("ORG");
			var org = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			module.IDOverride = ModuleIDs.Organisation;
			module.SelectedBusinessObjectsOverride = new[] { org };
			module.GridCollection.Add(org);

			var menuItems = provider.GetMenuItems(module).ToArray();
			var addTag = (TagMenuItemMenuTree)menuItems.SingleOrDefault(m => m.Text == "Add Tag");
			AssertNull(addTag);
		}

		#endregion

		#region Process Headers

		public void TestAddTagMenuItem_ShouldIncludeWorkQueues_ForProcessHeaders_WhenWorkflowManagementModeIsSetToPLN()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry("PLN");

			var definitionWithMagnitudes = BMSTestHelper.CreateTagDefinition(Factory, "DE1", "Definition 1");
			BMSTestHelper.CreateTagMagnitude(definitionWithMagnitudes, "MG1", "Magnitude 1");
			BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessHeader;
			module.SelectedBusinessObjectsOverride = new[] { workflow };
			module.GridCollection.Add(workflow);

			var menuItems = provider.GetMenuItems(module).ToArray();
			var addTag = (TagMenuItemMenuTree)menuItems.SingleOrDefault(m => m.Text == "Add Tag");
			AssertNotNull(addTag);
			addTag.OnPopup();

			AssertEquals(2, addTag.MenuItems.Count);
			var subMenuItem = addTag.MenuItems.Cast<ZMenuItem>().ToArray();
			AssertNotNull(subMenuItem.SingleOrDefault(m => m.Text.Contains("DE1")));
			AssertNotNull("Should contain work queue tags", subMenuItem.SingleOrDefault(m => m.Text.Contains("QUE")));
		}

		public void TestAddTagMenuItem_ShouldExclueWorkQueues_ForProcessHeaders_WhenWorkflowManagementModeIsSetToBUF()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry("BUF");

			var definitionWithMagnitudes = BMSTestHelper.CreateTagDefinition(Factory, "DE1", "Definition 1");
			BMSTestHelper.CreateTagMagnitude(definitionWithMagnitudes, "MG1", "Magnitude 1");
			BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessHeader;
			module.SelectedBusinessObjectsOverride = new[] { workflow };
			module.GridCollection.Add(workflow);

			var menuItems = provider.GetMenuItems(module).ToArray();
			var addTag = (TagMenuItemMenuTree)menuItems.SingleOrDefault(m => m.Text == "Add Tag");
			AssertNotNull(addTag);
			addTag.OnPopup();

			AssertEquals(1, addTag.MenuItems.Count);
			var subMenuItem = addTag.MenuItems.Cast<ZMenuItem>().ToArray();
			AssertNotNull(subMenuItem.SingleOrDefault(m => m.Text.Contains("DE1")));
			AssertNull("Should not contain work queue tags", subMenuItem.SingleOrDefault(m => m.Text.Contains("QUE")));
		}

		public void TestAddTagMenuItem_ShouldExcludeWorkQueues_ForProcessHeaders_WhenWorkflowManagementModeIsSetToEWF()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry("EWF");

			var definitionWithMagnitudes = BMSTestHelper.CreateTagDefinition(Factory, "DE1", "Definition 1");
			BMSTestHelper.CreateTagMagnitude(definitionWithMagnitudes, "MG1", "Magnitude 1");
			BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessHeader;
			module.SelectedBusinessObjectsOverride = new[] { workflow };
			module.GridCollection.Add(workflow);

			var menuItems = provider.GetMenuItems(module).ToArray();
			var addTag = (TagMenuItemMenuTree)menuItems.SingleOrDefault(m => m.Text == "Add Tag");
			AssertNotNull(addTag);
			addTag.OnPopup();

			AssertEquals(1, addTag.MenuItems.Count);
			var subMenuItem = addTag.MenuItems.Cast<ZMenuItem>().ToArray();
			AssertNotNull(subMenuItem.SingleOrDefault(m => m.Text.Contains("DE1")));
			AssertNull("Should not contain work queue tags", subMenuItem.SingleOrDefault(m => m.Text.Contains("QUE")));
		}

		#endregion

		#region Process Tasks

		public void TestAddTagMenuItem_ShouldExcludeWorkQueues_ForProcessTasks_WhenWorkflowManagementModeIsSetToPLN()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry("PLN");

			var definitionWithMagnitudes = BMSTestHelper.CreateTagDefinition(Factory, "DE1", "Definition 1");
			BMSTestHelper.CreateTagMagnitude(definitionWithMagnitudes, "MG1", "Magnitude 1");
			BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			var task = CreateTask(workflow, "", 20);

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessTasks;
			module.SelectedBusinessObjectsOverride = new[] { task };
			module.GridCollection.Add(task);

			var menuItems = provider.GetMenuItems(module).ToArray();
			var addTag = (TagMenuItemMenuTree)menuItems.SingleOrDefault(m => m.Text == "Add Tag");
			AssertNotNull(addTag);
			addTag.OnPopup();

			AssertEquals(1, addTag.MenuItems.Count);
			var subMenuItem = addTag.MenuItems.Cast<ZMenuItem>().ToArray();
			AssertNotNull(subMenuItem.SingleOrDefault(m => m.Text.Contains("DE1")));
			AssertNull("Should not contain work queue tags", subMenuItem.SingleOrDefault(m => m.Text.Contains("QUE")));
		}

		#endregion

		#endregion

		#endregion

		#region ProcessHeader

		public void TestProcessHeader_ShowWorkflowApplicableTags()
		{
			var system = CreateSystem("ORG");
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];

			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "COW", "Group 1", scope: TagScopeList.Codes.Task);
			var mag1 = BMSTestHelper.CreateTagMagnitude(definition1, "TAG", "Tag1");

			var definition2 = BMSTestHelper.CreateTagDefinition(Factory, "BIN", "Group 2", scope: TagScopeList.Codes.Workflow);
			var mag2 = BMSTestHelper.CreateTagMagnitude(definition2, "TIG", "Tig1");

			var definition3 = BMSTestHelper.CreateTagDefinition(Factory, "SOW", "Group 3");
			var mag3 = BMSTestHelper.CreateTagMagnitude(definition3, "TOG", "Tog1");

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessHeader;

			module.SelectedBusinessObjectsOverride = new[] { workflow1 };

			var menuItems = provider.GetMenuItems(module).ToArray();
			var addTag = (TagMenuItemMenuTree)menuItems[0];
			addTag.OnPopup();

			AssertCollectionNotContains("Task only tag group not available.", addTag.MenuItems.Cast<MenuItem>(), m => m.Text == definition1.DisplayText);
			AssertCollectionContains("Workflow tag group available", addTag.MenuItems.Cast<MenuItem>(), m => m.Text == definition2.DisplayText);
			AssertCollectionContains("All scope tag group available", addTag.MenuItems.Cast<MenuItem>(), m => m.Text == definition3.DisplayText);
		}

		public void TestProcessHeader_ShouldOpenTagApplicationMenu_Remove_OnlyShowTagsSelected()
		{
			var system = CreateSystem("ORG");
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			definition.TGD_Code = "COW";
			definition.TGD_Description = "Group 1";
			var mag1 = BMSTestHelper.CreateTagMagnitude(definition, "TAG", "Tag1");
			var mag2 = BMSTestHelper.CreateTagMagnitude(definition, "TAT", "Tag2");

			var definition2 = Factory.NewWithValidTestData<TagDefinition>();
			definition2.TGD_Code = "COZ";
			definition2.TGD_Description = "Group 2";

			var tag = BMSTestHelper.CreateTagLink(workflow1, mag1);

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessHeader;

			module.SelectedBusinessObjectsOverride = new[] { workflow1 };

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(2, menuItems.Length);

			var removeTag = (TagMenuItemMenuTree)menuItems[1];
			removeTag.OnPopup();
			AssertEquals(1, removeTag.MenuItems.Count);

			AssertEquals("COW - Group 1", removeTag.MenuItems[0].Text);
			AssertEquals(1, removeTag.MenuItems[0].MenuItems.Count);
			AssertEquals("TAG - Tag1", removeTag.MenuItems[0].MenuItems[0].Text);
		}

		public void TestProcessHeader_AddTagShouldTagToSelected()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var workflow1 = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders[0];
			var workflow2 = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders[0];

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			definition.TGD_Description = "Group 1";
			definition.TGD_Code = "GR1";
			var mag1 = BMSTestHelper.CreateTagMagnitude(definition, "TAG", "Tag1");
			var mag2 = BMSTestHelper.CreateTagMagnitude(definition, "TAT", "Tag2");

			var definition2 = Factory.NewWithValidTestData<TagDefinition>();
			definition2.TGD_Description = "Group 2";
			definition2.TGD_Code = "GR2";

			Factory.Save();

			AssertEquals(false, workflow1.TagLinks.Any());

			module.IDOverride = ModuleIDs.ProcessHeader;
			module.SelectedBusinessObjectsOverride = new[] { workflow1 };

			module.GridCollection.Add(workflow1);

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(2, menuItems.Length);

			var addTag = (TagMenuItemMenuTree)menuItems[0];
			var removeTag = (TagMenuItemMenuTree)menuItems[1];
			addTag.OnPopup();

			module.SelectedBusinessObjectsOverride = new[] { workflow1 };
			Assert(module.GetSelectedBusinessObjects().Contains(workflow1));
			addTag.MenuItems.Cast<MenuItem>().Single(t => t.Text == definition.DisplayText).MenuItems[0].PerformClick();

			AssertEquals(true, workflow1.TagLinks.Any());
			AssertEquals(false, workflow2.TagLinks.Any());
			AssertEquals(mag1, workflow1.TagLinks_ForBinding[0].Magnitude);

			removeTag.OnPopup();
			removeTag.MenuItems.Cast<MenuItem>().Single(t => t.Text == definition.DisplayText).MenuItems[0].PerformClick();

			AssertEquals(false, workflow1.TagLinks.Any());

			removeTag.OnPopup();

			AssertEquals(1, removeTag.MenuItems.Count);
		}

		#endregion

		#region ProcessTask

		public void TestProcessTask_ShowTaskApplicableTags()
		{
			var system = CreateSystem("ORG");
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var task1 = CreateTask(workflow1, "", 20);

			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "COW", "Group 1", scope: TagScopeList.Codes.Task);
			var mag1 = BMSTestHelper.CreateTagMagnitude(definition1, "TAG", "Tag1");

			var definition2 = BMSTestHelper.CreateTagDefinition(Factory, "BIN", "Group 2", scope: TagScopeList.Codes.Workflow);
			var mag2 = BMSTestHelper.CreateTagMagnitude(definition2, "TIG", "Tig1");

			var definition3 = BMSTestHelper.CreateTagDefinition(Factory, "SOW", "Group 3");
			var mag3 = BMSTestHelper.CreateTagMagnitude(definition3, "TOG", "Tog1");

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessTasks;
			module.SelectedBusinessObjectsOverride = new[] { task1 };
			module.GridCollection.Add(task1);

			var menuItems = provider.GetMenuItems(module).ToArray();
			var addTag = (TagMenuItemMenuTree)menuItems[0];
			addTag.OnPopup();

			AssertCollectionContains("Task only tag group available.", addTag.MenuItems.Cast<MenuItem>(), m => m.Text == definition1.DisplayText);
			AssertCollectionNotContains("Workflow tag group not available", addTag.MenuItems.Cast<MenuItem>(), m => m.Text == definition2.DisplayText);
			AssertCollectionContains("All scope tag group available", addTag.MenuItems.Cast<MenuItem>(), m => m.Text == definition3.DisplayText);
		}

		public void TestProcessTask_ShouldOpenTagApplicationMenu_Remove_OnlyShowTagsSelected()
		{
			var system = CreateSystem("ORG");
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, string.Empty, 30);

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			definition.TGD_Code = "COW";
			definition.TGD_Description = "Group 1";
			var mag1 = BMSTestHelper.CreateTagMagnitude(definition, "TAG", "Tag1");
			var mag2 = BMSTestHelper.CreateTagMagnitude(definition, "TAT", "Tag2");

			var definition2 = Factory.NewWithValidTestData<TagDefinition>();
			definition2.TGD_Code = "COZ";
			definition2.TGD_Description = "Group 2";

			task.AddTag(mag1);

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessHeader;

			module.SelectedBusinessObjectsOverride = new[] { task };

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(2, menuItems.Length);

			var removeTag = (TagMenuItemMenuTree)menuItems[1];
			removeTag.OnPopup();
			AssertEquals(1, removeTag.MenuItems.Count);

			AssertEquals("COW - Group 1", removeTag.MenuItems[0].Text);
			AssertEquals(1, removeTag.MenuItems[0].MenuItems.Count);
			AssertEquals("TAG - Tag1", removeTag.MenuItems[0].MenuItems[0].Text);
		}

		public void TestProcessTask_AddTagShouldTagToSelected()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders[0];

			var task1 = BMSTestHelper.CreateTask(workflow, string.Empty, 30);
			var task2 = BMSTestHelper.CreateTask(workflow, string.Empty, 30);

			var definition = Factory.NewWithValidTestData<TagDefinition>();
			definition.TGD_Description = "Group 1";
			definition.TGD_Code = "GR1";
			var mag1 = BMSTestHelper.CreateTagMagnitude(definition, "TAG", "Tag1");
			var mag2 = BMSTestHelper.CreateTagMagnitude(definition, "TAT", "Tag2");

			var definition2 = Factory.NewWithValidTestData<TagDefinition>();
			definition2.TGD_Description = "Group 2";
			definition2.TGD_Code = "GR2";

			Factory.Save();

			AssertEquals(false, ((ITagable)task1).TagLinks.Cast<TagLink>().Any());

			module.IDOverride = ModuleIDs.ProcessTasks;
			module.SelectedBusinessObjectsOverride = new[] { task1 };

			module.GridCollection.Add(task1);

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(2, menuItems.Length);

			var addTag = (TagMenuItemMenuTree)menuItems[0];
			var removeTag = (TagMenuItemMenuTree)menuItems[1];
			addTag.OnPopup();

			module.SelectedBusinessObjectsOverride = new[] { task1 };
			Assert(module.GetSelectedBusinessObjects().Contains(task1));
			addTag.MenuItems.Cast<MenuItem>().Single(t => t.Text == definition.DisplayText).MenuItems[0].PerformClick();

			AssertEquals(true, ((ITagable)task1).TagLinks.Cast<TagLink>().Any());
			AssertEquals(false, ((ITagable)task2).TagLinks.Cast<TagLink>().Any());
			AssertEquals(mag1, ((ITagable)task1).TagLinks.Cast<TagLink>().First().Magnitude);

			removeTag.OnPopup();
			removeTag.MenuItems.Cast<MenuItem>().Single(t => t.Text == definition.DisplayText).MenuItems[0].PerformClick();

			AssertEquals(false, ((ITagable)task1).TagLinks.Cast<TagLink>().Any());

			removeTag.OnPopup();

			AssertEquals(1, removeTag.MenuItems.Count);
		}

		#endregion

		#region TryGetButtonDetail

		public void TestTryGetButtonDetail()
		{
			module.IDOverride = ModuleIDs.ProcessHeader;

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(2, menuItems.Length);
			AssertEquals(TagilatorMenuItemProvider.AddTagMenuItemName, menuItems[0].Name);
			AssertEquals(TagilatorMenuItemProvider.RemoveTagMenuItemName, menuItems[1].Name);

			IconTypes buttonImage = IconTypes.None, buttonImageActive = IconTypes.None;
			string buttonToolTip = string.Empty;

			provider.TryGetButtonDetail(menuItems[0], ref buttonImage, ref buttonImageActive, ref buttonToolTip);

			AssertEquals(IconTypes.Tag, buttonImage);
			AssertEquals(IconTypes.Tag, buttonImageActive);
			AssertEquals("Add a tag to the selected rows.", buttonToolTip);

			provider.TryGetButtonDetail(menuItems[1], ref buttonImage, ref buttonImageActive, ref buttonToolTip);

			AssertEquals(IconTypes.Tag, buttonImage);
			AssertEquals(IconTypes.Tag, buttonImageActive);
			AssertEquals("Remove a tag from the selected rows.", buttonToolTip);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			module = new DummyModuleForVisualBoards();
			provider = new TagilatorMenuItemProvider();
		}

		protected override void TearDown()
		{
			base.TearDown();
			module.Dispose();
		}

		IFilterGridTopLevelMenuItemProvider provider;
		DummyModuleForVisualBoards module;

		#endregion
	}
}

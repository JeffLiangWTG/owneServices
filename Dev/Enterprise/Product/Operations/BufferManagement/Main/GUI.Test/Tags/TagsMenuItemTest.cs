using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI.Test
{
	class TagsMenuItemTest : BMSTestCaseWithFactory
	{
		public void TestExclusiveTag_FailureMessage()
		{
			var def = BMSTestHelper.CreateTagDefinition(Factory, "DE1", "Definition 1", true);
			var mag1 = BMSTestHelper.CreateTagMagnitude(def, "MG1", "Origional Exclusive Tag");
			var mag2 = BMSTestHelper.CreateTagMagnitude(def, "MG2", "New Exclusive Tag");

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow Completion Statement");
			var task = Factory.New<ProcessTask>();
			task.P9_Description = "Task Description";

			var tagables = new ITagable[] { workflow, task };
			var definitions = new TagDefinitionCache(Factory);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var addTag = new AddTagMenuItemViewModel((_, x_) => tagables, typeof(ProcessHeader), Lazy.Create(() => definitions), Factory);
			var tree = new TagToolStripMenuTree(addTag, viewModel, new TaskCardContent(task, viewModel));

			tree.OnDropDownOpening_ForTest();

			((ToolStripMenuItem)((ToolStripMenuItem)tree.DropDownItems[0]).DropDownItems[0]).PerformClick(); // Add mag2 to workflow.

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			((ToolStripMenuItem)((ToolStripMenuItem)tree.DropDownItems[0]).DropDownItems[1]).PerformClick(); // Add mag1 to workflow and click ok to replace mag2.

			AssertEquals(@"The following items already have a tag from the tag group 'DE1 - Definition 1' applied:
'Organization (XVBQP68SIYXQ) - Workflow Completion Statement' already has the tag 'MG1 - Origional Exclusive Tag'
'Task Description' already has the tag 'MG1 - Origional Exclusive Tag'
Replace existing tags?", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestAddToolStripMenuItem_WithExclusiveTags()
		{
			var def = BMSTestHelper.CreateTagDefinition(Factory, "DNK", "IS DANIEL KEOGH THE BEST?", true);

			var mag1 = BMSTestHelper.CreateTagMagnitude(def, "YEP", "YES HE IS!");
			var mag2 = BMSTestHelper.CreateTagMagnitude(def, "NUP", "BABOW!");

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow Completion Statement");
			var task = Factory.New<ProcessTask>();

			var tagables = new ITagable[] { workflow, task };
			var definitions = new TagDefinitionCache(Factory);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var addTag = new AddTagMenuItemViewModel((_, x_) => tagables, typeof(ProcessTask), Lazy.Create(() => definitions), Factory);
			var tree = new TagToolStripMenuTree(addTag, viewModel, new TaskCardContent(task, viewModel));

			tree.OnDropDownOpening_ForTest();

			AssertEquals(def.DisplayText, tree.DropDownItems[0].Text);
			AssertEquals(mag1.DisplayText, ((ToolStripMenuItem)tree.DropDownItems[0]).DropDownItems[1].Text);

			((ToolStripMenuItem)((ToolStripMenuItem)tree.DropDownItems[0]).DropDownItems[0]).PerformClick(); // Add mag2 to workflow.

			AssertCollectionContains(workflow.TagLinks, t => t.TGL_TGM_Magnitude == mag2.PK);
			AssertCollectionContains(((ITagable)task).TagLinks.Cast<ITagLink>(), t => t.TGL_TGM_Magnitude == mag2.PK);

			var newFactory = Factory.CreateNewFactory();
			var workflowInNewFactory = newFactory.Load<ProcessHeader>(workflow.PK);
			var taskInNewFactory = newFactory.Load<ProcessTask>(task.PK);

			AssertCollectionContains(workflowInNewFactory.TagLinks, t => t.TGL_TGM_Magnitude == mag2.PK);
			AssertCollectionContains(((ITagable)taskInNewFactory).TagLinks.Cast<ITagLink>(), t => t.TGL_TGM_Magnitude == mag2.PK);

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			((ToolStripMenuItem)((ToolStripMenuItem)tree.DropDownItems[0]).DropDownItems[1]).PerformClick(); // Add Mag1 to workflow and click ok to replace mag2.

			AssertCollectionContains(workflow.TagLinks, t => t.TGL_TGM_Magnitude == mag1.PK);
			AssertCollectionContains(((ITagable)task).TagLinks.Cast<ITagLink>(), t => t.TGL_TGM_Magnitude == mag1.PK);
			AssertCollectionNotContains(((ITagable)task).TagLinks.Cast<ITagLink>(), t => t.TGL_TGM_Magnitude == mag2.PK);

			var anotherNewFactory = Factory.CreateNewFactory();
			workflowInNewFactory = anotherNewFactory.Load<ProcessHeader>(workflow.PK);
			taskInNewFactory = anotherNewFactory.Load<ProcessTask>(task.PK);

			AssertCollectionContains(workflowInNewFactory.TagLinks, t => t.TGL_TGM_Magnitude == mag1.PK);
			AssertCollectionContains(((ITagable)taskInNewFactory).TagLinks.Cast<ITagLink>(), t => t.TGL_TGM_Magnitude == mag1.PK);
			AssertCollectionNotContains(((ITagable)taskInNewFactory).TagLinks.Cast<ITagLink>(), t => t.TGL_TGM_Magnitude == mag2.PK);
		}

		public void TestAddToolStripMenuItem()
		{
			var def = Factory.New<TagDefinition>();
			def.TGD_Code = "DNK";
			def.TGD_Description = "IS DANIEL KEOGH THE BEST?";

			var mag1 = BMSTestHelper.CreateTagMagnitude(def, "YEP", "YES HE IS!");
			var mag2 = BMSTestHelper.CreateTagMagnitude(def, "NUP", "BABOW!");

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Jimminy Jillikers");
			var task = Factory.New<ProcessTask>();

			var tagables = new ITagable[] { workflow, task };
			var definitions = new TagDefinitionCache(Factory);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var addTag = new AddTagMenuItemViewModel((_, x_) => tagables, typeof(ProcessTask), Lazy.Create(() => definitions), Factory);
			var tree = new TagToolStripMenuTree(addTag, viewModel, new TaskCardContent(task, viewModel));

			tree.OnDropDownOpening_ForTest();

			AssertEquals(def.DisplayText, tree.DropDownItems[0].Text);
			AssertEquals(mag1.DisplayText, ((ToolStripMenuItem)tree.DropDownItems[0]).DropDownItems[1].Text);

			((ToolStripMenuItem)((ToolStripMenuItem)tree.DropDownItems[0]).DropDownItems[1]).PerformClick();

			AssertCollectionContains(workflow.TagLinks, t => t.TGL_TGM_Magnitude == mag1.PK);
			AssertCollectionContains(((ITagable)task).TagLinks.Cast<ITagLink>(), t => t.TGL_TGM_Magnitude == mag1.PK);

			var newFactory = Factory.CreateNewFactory();
			var workflowInNewFactory = newFactory.Load<ProcessHeader>(workflow.PK);
			var taskInNewFactory = newFactory.Load<ProcessTask>(task.PK);

			AssertCollectionContains(workflowInNewFactory.TagLinks, t => t.TGL_TGM_Magnitude == mag1.PK);
			AssertCollectionContains(((ITagable)taskInNewFactory).TagLinks.Cast<ITagLink>(), t => t.TGL_TGM_Magnitude == mag1.PK);
		}

		public void TestRemoveToolStripMenuItem()
		{
			var def = Factory.New<TagDefinition>();
			def.TGD_Code = "DNK";
			def.TGD_Description = "IS DANIEL KEOGH THE BEST?";

			var mag1 = BMSTestHelper.CreateTagMagnitude(def, "YEP", "YES HE IS!");
			var mag2 = BMSTestHelper.CreateTagMagnitude(def, "NUP", "BABOW!");

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow");
			var task = Factory.New<ProcessTask>();

			task.AddTag(mag2);
			workflow.AddTag(mag2);

			var tagables = new ITagable[] { workflow, task };
			var definitions = new TagDefinitionCache(Factory);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var removeTag = new RemoveTagMenuItemViewModel((_, x_) => tagables, Lazy.Create(() => definitions), Factory);
			var tree = new TagToolStripMenuTree(removeTag, viewModel, new TaskCardContent(task, viewModel));

			tree.OnDropDownOpening_ForTest();

			AssertEquals(def.DisplayText, tree.DropDownItems[0].Text);
			AssertEquals(mag2.DisplayText, ((ToolStripMenuItem)tree.DropDownItems[0]).DropDownItems[0].Text);
			AssertEquals(1, ((ToolStripMenuItem)tree.DropDownItems[0]).DropDownItems.Count);

			((ToolStripMenuItem)((ToolStripMenuItem)tree.DropDownItems[0]).DropDownItems[0]).PerformClick();

			tree.OnDropDownOpening_ForTest();
			AssertEquals(1, tree.DropDownItems.Count);

			AssertCollectionNotContains(workflow.TagLinks, t => t.TGL_TGM_Magnitude == mag2.PK);
			AssertCollectionNotContains(((ITagable)task).TagLinks.Cast<ITagLink>(), t => t.TGL_TGM_Magnitude == mag2.PK);

			var newFactory = Factory.CreateNewFactory();
			var workflowInNewFactory = newFactory.Load<ProcessHeader>(workflow.PK);
			var taskInNewFactory = newFactory.Load<ProcessTask>(task.PK);

			AssertCollectionNotContains(workflowInNewFactory.TagLinks, t => t.TGL_TGM_Magnitude == mag2.PK);
			AssertCollectionNotContains(((ITagable)taskInNewFactory).TagLinks.Cast<ITagLink>(), t => t.TGL_TGM_Magnitude == mag2.PK);
		}

		#region DB Hits On Menu Opening

		public void TestAddTagToolStripMenuItem_ShouldReloadTagMagnitudesOnlyOnce_OnMenuOpening()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			var task = Factory.New<ProcessTask>();

			BulkCreateTagDefinitionsAndMagnitudes();

			Factory.Save();

			var tagables = new ITagable[] { workflow, task };
			var definitions = new TagDefinitionCache(Factory);
			var viewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var addTag = new AddTagMenuItemViewModel((_, x_) => tagables, typeof(ProcessTask), Lazy.Create(() => definitions), Factory);
			var tree = new TagToolStripMenuTree(addTag, viewModel, new TaskCardContent(task, viewModel));

			using (AssertDbHitsForAllFactories("Tag magnitudes should be reloaded only once on menu opening to ensure they are up to date (1 DB hit).", new Dictionary<string, int>
			{
				{ TagMagnitudeSchema.Constants.TableName, 1 },
				{ TagLinkSchema.Constants.TableName, 0 }
			}))
			{
				tree.OnDropDownOpening_ForTest();
			}
		}

		public void TestRemoveTagToolStripMenuItem_ShouldReloadTagMagnitudesOnlyOnce_OnMenuOpening()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			var task = Factory.New<ProcessTask>();

			BulkCreateTagDefinitionsAndMagnitudes(workflow, task);

			Factory.Save();

			var tagables = new ITagable[] { workflow, task };
			var definitions = new TagDefinitionCache(Factory);
			var viewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var removeTag = new RemoveTagMenuItemViewModel((_, x_) => tagables, Lazy.Create(() => definitions), Factory);
			var tree = new TagToolStripMenuTree(removeTag, viewModel, new TaskCardContent(task, viewModel));

			using (AssertDbHitsForAllFactories("Tag magnitudes should be reloaded only once on menu opening to ensure they are up to date (1 DB hit), and a tagable should query DB to get tag links (1 DB hit per tagable).", new Dictionary<string, int>
			{
				{ TagMagnitudeSchema.Constants.TableName, 1 },
				{ TagLinkSchema.Constants.TableName, tagables.Length }
			}))
			{
				tree.OnDropDownOpening_ForTest();
			}
		}

		#endregion

		public void TestRemoveToolStripMenuItem_NoTagsApplied()
		{
			var workflow = Factory.NewWithValidTestData<ProcessHeader>();
			var task = Factory.New<ProcessTask>();
			var tagables = new ITagable[] { workflow, task };
			var definitions = new TagDefinitionCache(Factory);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var removeTag = new RemoveTagMenuItemViewModel((_, x_) => tagables, Lazy.Create(() => definitions), Factory);
			var tree = new TagToolStripMenuTree(removeTag, viewModel, new TaskCardContent(task, viewModel));

			tree.OnDropDownOpening_ForTest();

			AssertEquals(1, tree.DropDownItems.Count);
			AssertEquals("There are no tags applied", tree.DropDownItems[0].Text);
			AssertEquals(false, tree.DropDownItems[0].Enabled);
		}

		public void TestAddTags_OnNewThread_ShouldNotCauseThreadSentryError()
		{
			var def = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var mag1 = BMSTestHelper.CreateTagMagnitude(def, "BAN", "It's not a ban");
			var mag2 = BMSTestHelper.CreateTagMagnitude(def, "YOU", "That's your word");
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", buffer);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			var definitions = new TagDefinitionCache(Factory);
			var sectionViewModel = BMSTestHelper.CreateDummyViewModel(Factory);

			Factory.Save();

			var addTagViewModel = new AddTagMenuItemViewModel((factory, isForRemovingTag) =>
			{
				var tagableWorkflow = factory.Load<ProcessHeader>(workflow.PK);
				var tagableTask = factory.Load<ProcessTask>(task.PK);
				return new ITagable[] { tagableWorkflow, tagableTask };
			}, typeof(ProcessHeader), Lazy.Create(() => definitions), Factory);

			AssertMenuClick_OnNewThread_ShouldNotCauseThreadSentryError(sectionViewModel, addTagViewModel, task, def);

			var newFactory = new BusinessObjectFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			var tags = loadedWorkflow.GetApplicableTags();
			AssertContainsExactElementsInAnyOrder("Both tags should have been applied, and yet...", new[] { mag1.TGM_Description, mag2.TGM_Description }, tags.Select(x => x.TGM_Description));
		}

		public void TestRemoveTags_OnNewThread_ShouldNotCauseThreadSentryError()
		{
			var def = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var mag1 = BMSTestHelper.CreateTagMagnitude(def, "BAN", "It's not a ban");
			var mag2 = BMSTestHelper.CreateTagMagnitude(def, "YOU", "That's your word");
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", buffer);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			workflow.AddTag(mag1);
			workflow.AddTag(mag2);

			var definitions = new TagDefinitionCache(Factory);
			var sectionViewModel = BMSTestHelper.CreateDummyViewModel(Factory);

			Factory.Save();

			var removeTagViewModel = new RemoveTagMenuItemViewModel((factory, isForRemovingTag) =>
			{
				var tagableWorkflow = factory.Load<ProcessHeader>(workflow.PK);
				var tagableTask = factory.Load<ProcessTask>(task.PK);
				return new ITagable[] { tagableWorkflow, tagableTask };
			}, Lazy.Create(() => definitions), Factory);

			AssertMenuClick_OnNewThread_ShouldNotCauseThreadSentryError(sectionViewModel, removeTagViewModel, task, def);

			var newFactory = new BusinessObjectFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			var tags = loadedWorkflow.GetApplicableTags();
			AssertContainsExactElementsInAnyOrder("Both tags should have been removed, and yet...", System.Array.Empty<TagMagnitude>(), tags.Select(x => x.TGM_Description));
		}

		static void AssertMenuClick_OnNewThread_ShouldNotCauseThreadSentryError(BMBoardSectionViewModel sectionViewModel, ITagMenuTreeViewModel menuItemViewModel, ProcessTask task, TagDefinition tagDefinition)
		{
			var factory = task.Factory;
			factory.RelinquishThreadOwnership();

			Task.Factory.StartNew(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					factory.TakeThreadOwnership();

					var tree = new TagToolStripMenuTree_ForTest(menuItemViewModel, sectionViewModel, new TaskCardContent(task, sectionViewModel));
					menuItemViewModel.GetValidDefinitionMenuItems(); // loads the list... this would always happen in gui whilst navigating menu (how good are side effects?!)
					var descriptors = menuItemViewModel.GetValidMagnitudeMenuItems(tagDefinition).ToArray();
					tree.OnDropDownOpening_ForTest();

					AssertNoExceptionThrown("The click handler happens on the main thread. If ThreadSentry was handled correctly, a ThreadSentry exception should not have been thrown, and yet...", () =>
					{
						tree.DoClickEvent_Exposed(descriptors[0]);
						tree.DoClickEvent_Exposed(descriptors[1]);
					});

					AssertEquals("Clicking the menu item should not leave the factory unowned, and yet...", true, factory.IsOwnedByCurrentThread);
					factory.RelinquishThreadOwnership();
				}
			}).Wait();

			Application.DoEvents();
		}

		public void TestAddTagMenuItem_ForMultipleJobs_ShouldAddLogToAllJobs()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var jobHeader1 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "Job Header 1");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "Job Header 2");

			var def = BMSTestHelper.CreateTagDefinition(Factory, "ABC");
			var mag = BMSTestHelper.CreateTagMagnitude(def, "XYZ");

			Factory.Save();

			UseTagMenuItemOnAllItems(TagilatorMenuItemProvider.AddTagMenuItemName);

			AssertTagApplied("The tag should have been added to both workflows. SAD!", jobHeader1, mag);
			AssertTagApplied("The tag should have been added to both workflows. SAD!", jobHeader2, mag);

			BMSTestHelper.AssertWorkflowsHaveLog("Both workflows should have the Add Tag event. SAD!", jobHeader1.Parent, Events.TagWasAddedOrRemoved, jobHeader1);
			BMSTestHelper.AssertWorkflowsHaveLog("Both workflows should have the Add Tag event. SAD!", jobHeader2.Parent, Events.TagWasAddedOrRemoved, jobHeader2);
		}

		public void TestRemoveTagMenuItem_ForMultipleJobs_ShouldAddLogToAllJobs()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var jobHeader1 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "Job Header 1");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "Job Header 2");

			var def = BMSTestHelper.CreateTagDefinition(Factory, "ABC");
			var mag = BMSTestHelper.CreateTagMagnitude(def, "XYZ");

			jobHeader1.AddTag(mag);
			jobHeader2.AddTag(mag);

			Factory.Save();

			UseTagMenuItemOnAllItems(TagilatorMenuItemProvider.RemoveTagMenuItemName);

			AssertTagNotApplied("The tag should have been removed from both workflows. SAD!", jobHeader1, mag);
			AssertTagNotApplied("The tag should have been removed from both workflows. SAD!", jobHeader2, mag);

			BMSTestHelper.AssertWorkflowsHaveLog("Both workflows should have two Tag events (one for add and one for remove). SAD!", jobHeader1.Parent, Events.TagWasAddedOrRemoved, jobHeader1, jobHeader1);
			BMSTestHelper.AssertWorkflowsHaveLog("Both workflows should have two Tag events (one for add and one for remove). SAD!", jobHeader2.Parent, Events.TagWasAddedOrRemoved, jobHeader2, jobHeader2);
		}

		public void TestAddTagMenuItem_ShouldExcludeGroupsWithNoActiveTags()
		{
			var definitionWithMagnitudes = BMSTestHelper.CreateTagDefinition(Factory, "DE1", "Definition 1");
			BMSTestHelper.CreateTagMagnitude(definitionWithMagnitudes, "MG1", "Magnitude 1");

			var definitionWithNoActiveMagnitudes = BMSTestHelper.CreateTagDefinition(Factory, "DE2", "Definition 2");
			BMSTestHelper.CreateTagMagnitude(definitionWithNoActiveMagnitudes, "MG2", "Magnitude 2", isActive: false);

			var definitionWithNoMagnitudes = BMSTestHelper.CreateTagDefinition(Factory, "DE3", "Definition 3");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow");
			var task = Factory.New<ProcessTask>();

			Factory.Save();

			var tagables = new ITagable[] { workflow, task };
			var definitions = new TagDefinitionCache(Factory);
			var viewModel = BMSTestHelper.CreateDummyViewModel(Factory);
			var addTag = new AddTagMenuItemViewModel((_, x_) => tagables, typeof(ProcessTask), Lazy.Create(() => definitions), Factory);
			var tree = new TagToolStripMenuTree(addTag, viewModel, new TaskCardContent(task, viewModel));
			tree.OnDropDownOpening_ForTest();
			var dropDownItems = tree.DropDownItems.ToList<ToolStripMenuItem>();

			AssertEquals(true, definitionWithMagnitudes.Magnitudes.Any(m => m.TGM_IsActive));
			AssertNotNull("Groups with active tags should be in the menu 'Add Tag'.", FindByText(dropDownItems, definitionWithMagnitudes.DisplayText));

			AssertEquals(false, definitionWithNoActiveMagnitudes.Magnitudes.Any(m => m.TGM_IsActive));
			AssertNull("Groups with no active tags should be in the menu 'Add Tag'.", FindByText(dropDownItems, definitionWithNoActiveMagnitudes.DisplayText));

			AssertEquals(false, definitionWithNoMagnitudes.Magnitudes.Any());
			AssertNull("Groups with no tags should not be in the menu 'Add Tag'.", FindByText(dropDownItems, definitionWithNoMagnitudes.DisplayText));
		}

		static ToolStripMenuItem FindByText(IEnumerable<ToolStripMenuItem> dropDownItems, string text) => dropDownItems.FirstOrDefault(i => i.Text == text);

		static void UseTagMenuItemOnAllItems(string menuItemName)
		{
			ZFormModaliser.ShowDialogsInTest = true;

			using (var module = new DummyWithWorkflowModule())
			using (module.ShowPopup())
			{
				Application.DoEvents();

				module.PerformSearch_ForTest();
				Application.DoEvents();

				module.SelectedBusinessObjectsOverride = module.GridCollection.ToArray(); // Select all rows

				var rootMenuItem = (TagMenuItemMenuTree)module.ContextMenuExposed.Single(x => x.Name == menuItemName);
				rootMenuItem.OnPopup();
				Application.DoEvents();

				var magnitudeMenuItem = rootMenuItem.MenuItems[0].MenuItems[0];
				magnitudeMenuItem.PerformClick();
				Application.DoEvents();
			}
		}

		#region Performance

		public void TestMenuItemInModule_WhenShortcutKeysPressed_ShouldNotLoad()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, "DUM");
			Factory.Save();

			ZFormModaliser.ShowDialogsInTest = true;

			using (var module = new DummyWithWorkflowModule())
			using (module.ShowPopup())
			{
				Application.DoEvents();

				var rootMenuItem = (TagMenuItemMenuTree)module.ContextMenuExposed.Single(x => x.Name == TagilatorMenuItemProvider.AddTagMenuItemName);
				AssertContainsExactElementsInAnyOrder("The menu should not be loaded just by opening the module. SAD!", new[] { "-" }, GetMenuItemLabels(rootMenuItem.MenuItems));

				KeySender.SendKeyDownToProcessCmdKey(module.DisplayGrid, Keys.Control | Keys.Shift | Keys.S);
				Application.DoEvents();

				AssertContainsExactElementsInAnyOrder("The menu should not be loaded by pressing shortcut keys. SAD!", new[] { "-" }, GetMenuItemLabels(rootMenuItem.MenuItems));
			}
		}

		public void TestMenuItemInModule_Cache()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var jobHeader1 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "Job Header 1");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, description: "Job Header 2");

			var def = BMSTestHelper.CreateTagDefinition(Factory, "ABC");
			var mag = BMSTestHelper.CreateTagMagnitude(def, "XYZ");

			jobHeader1.AddTag(mag);
			jobHeader2.AddTag(mag);

			Factory.Save();

			using (var module = new DummyWithWorkflowModule())
			using (var form = (Form)module.ShowPopup())
			{
				Application.DoEvents();

				AssertNull(module.GridCollection.Factory.GetCachedValue<TagDefinitionCache>(nameof(TagDefinitionCache), () => null));
				module.GridCollection.Factory.ClearCachedValue<TagDefinitionCache>(nameof(TagDefinitionCache));

				var grid = form.FindAll<ZGrid>().Single();
				grid.ContextMenu.OnPopup_ForTest();
				foreach (MenuItem item in grid.ContextMenu.MenuItems)
				{
					item.OnPopup_Exposed();
				}
				AssertNotNull(module.GridCollection.Factory.GetCachedValue<TagDefinitionCache>(nameof(TagDefinitionCache), () => null));
			}
		}

		static IEnumerable<string> GetMenuItemLabels(Menu.MenuItemCollection menuItems)
		{
			foreach (MenuItem item in menuItems)
			{
				yield return item.Text;
			}
		}

		#endregion

		#region Implementation

		protected override bool ShouldDisableAsyncBehaviour => true;

		class TagToolStripMenuTree_ForTest : TagToolStripMenuTree
		{
			public TagToolStripMenuTree_ForTest(ITagMenuTreeViewModel model, BMBoardSectionViewModel viewModel, ICardContent content)
				: base(model, viewModel, content)
			{
			}

			public void DoClickEvent_Exposed(MenuItemDescriptor<TagMagnitude> descriptor)
			{
				DoClickEvent(this, descriptor);
			}
		}

		#endregion
	}
}

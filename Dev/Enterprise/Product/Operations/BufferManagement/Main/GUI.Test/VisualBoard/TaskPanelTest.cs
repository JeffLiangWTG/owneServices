using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class TaskPanelTest : BMSGUITestCase
	{
		#region Board filters

		public void TestSetupTasks_WithFilter_ShouldRepositionUndrawn()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var job = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(job, "Designate Apparent", releaseGroupPK: group.PK);

			var tasksCount = 35;
			var tasks = Enumerable.Range(0, tasksCount).Select(num => CreateTask(workflow, resource.GS_Code, 20, description: "T" + num)).ToArray();

			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 200, height: 200))
			{
				var taskPanel = form.FindSingle<TaskPanel>();

				const int firstTaskUnderConsiderationIndex = 18;

				// Setting up tasks with no filters
				taskPanel.SetupTasksForTest(Factory, Enumerable.Empty<IBoardFilter>());
				var cardCount = taskPanel.TaskCards.Count();
				AssertCollectionNotContains("This task doesn't fit", tasks[firstTaskUnderConsiderationIndex].PK, taskPanel.TaskCards.Select(c => c.CardContent.TaskIdentifier));
				var visibleCardsCount = taskPanel.TaskCards.Count(c => c.Visible);
				AssertEquals("Should be some number of visible task cards plus one invisible", taskPanel.TaskCards.Count(), visibleCardsCount + 1);

				// Setting up tasks with a highlighting filter for tasks in workflow2
				var filters = new[] { new FarTooLateVisibilityFilter(tasks.Skip(firstTaskUnderConsiderationIndex).Take(tasksCount).Select(t => t.PK).ToArray()) };
				taskPanel.SetupTasksForTest(Factory, filters);

				AssertEquals("Even though some other tasks were previously unrendered, they are now visible", visibleCardsCount, taskPanel.TaskCards.Count(t => t.Visible));
				AssertEquals(tasks[firstTaskUnderConsiderationIndex].PK, taskPanel.TaskCards.First(t => t.Visible).CardContent.TaskIdentifier);
				AssertEquals("New task cards (up to the number 18 plus all currently visible ones plus the next invisible one) should be allocated in addition to all previously allocated cards", firstTaskUnderConsiderationIndex + visibleCardsCount + 1, taskPanel.TaskCards.Count());

				filters = new[] { new FarTooLateVisibilityFilter(new ZGuid[] { tasks[firstTaskUnderConsiderationIndex].PK }) };
				taskPanel.SetupTasksForTest(Factory, filters);

				AssertEquals("Should be the only one visible task", 1, taskPanel.TaskCards.Count(t => t.Visible));
				AssertEquals(tasks[firstTaskUnderConsiderationIndex].PK, taskPanel.TaskCards.Single(t => t.Visible).CardContent.TaskIdentifier);
				AssertEquals("All task cards should be allocated when trying to fill the task panel with visible cards", tasksCount, taskPanel.TaskCards.Count());
			}
		}

		class FarTooLateVisibilityFilter : IFilterApplicator
		{
			public FarTooLateVisibilityFilter(ZGuid[] taskPKs)
			{
				matchingTasks = taskPKs;
			}
			readonly ZGuid[] matchingTasks;

			public bool IsApplicable(ICardContent cardContent, CellContent cell, BMBoardSectionViewModel boardSection)
			{
				return matchingTasks.Contains(cardContent.TaskIdentifier);
			}

			public void Apply(IComponent control, bool isApplicable, IEnumerable<AppliedFilter> applicableApplicators)
			{
				((Control)control).Visible = isApplicable;
			}

			public bool AllowMultiple
			{
				get { return false; }
			}

			public string FilterName
			{
				get { return "YouMissedIt"; }
			}

			public bool RequiresRedraw
			{
				get { return true; }
			}

			public bool RequiresRemoval { get; set; }

			public Action RestoreVisualStateAfterFilterRemovedAction { get; set; }
		}

		public void TestSetupTasks_ShouldApplyFilters()
		{
			var system = Factory.New<BMSystem>();
			system.FS_Name = "system";

			var group = BMSTestHelper.CreateGroup(Factory, "AAA");

			var bucket = system.Components.AddNew();
			bucket.FC_Name = "bee";

			var job = Factory.New<OrgHeader>();
			job.OH_Code = "MAIFSTORG";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_FC_CurrentComponent = bucket.PK;
			workflow1.FH_GG_ReleaseGroup = group.PK;

			var channel = Factory.NewWithValidTestData<GlbStaff>();

			var task1 = job.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			task1.P9_GS_NKAssignedStaffMember = channel.GS_Code;

			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 200, height: 200))
			{
				var taskPanel = form.FindAll<TaskPanel>().Single();
				taskPanel.SetupTasksForTest(Factory, null, Enumerable.Empty<IBoardFilter>());

				var startingTaskCard = taskPanel.TaskCards.First();
				AssertEquals("TaskCardControl", startingTaskCard.Name);

				taskPanel.SetupTasksForTest(Factory, null, new[] { new FilterWhichAppliesNameToControl("TaskCrad1") });
				var newTaskCard = taskPanel.TaskCards.First();
				AssertEquals("TaskCrad1", newTaskCard.Name);
				Assert("Should be new task card instances", !Object.ReferenceEquals(startingTaskCard, newTaskCard));
			}
		}

		public void TestSetupTasks_WithFiltersNotRequiringRedraw_ShouldApplyFiltersAndPreserveCards()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_FC_CurrentComponent = bucket.PK;
			workflow1.FH_GG_ReleaseGroup = group.PK;

			var channel = Factory.NewWithValidTestData<GlbStaff>();

			var task1 = workflow1.TaskCollection.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			task1.P9_GS_NKAssignedStaffMember = channel.GS_Code;

			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 200, height: 200))
			{
				var taskPanel = form.FindSingle<TaskPanel>();

				taskPanel.SetupTasksForTest(Factory, null, Enumerable.Empty<IBoardFilter>());

				var startingTaskCard = taskPanel.TaskCards.Single();
				AssertEquals("TaskCardControl", startingTaskCard.Name);

				taskPanel.SetupTasksForTest(Factory, null, new[] { new FilterWhichAppliesNameToControl("TaskCrad1") { RequiresRedraw = false } }, requiresFullRedraw: false);
				var newTaskCard = taskPanel.TaskCards.Single();
				AssertEquals("TaskCrad1", newTaskCard.Name);
				Assert("Should be the same task card instance", Object.ReferenceEquals(startingTaskCard.BackgroundImage, newTaskCard.BackgroundImage));
			}
		}

		class FilterWhichAppliesNameToControl : IBoardFilter, IFilterApplicator
		{
			internal FilterWhichAppliesNameToControl(string name)
			{
				this.name = name;
			}

			readonly string name;

			public bool AllowMultiple
			{
				get { return false; }
			}

			public void Apply(IComponent control, bool isApplicable, IEnumerable<AppliedFilter> filters = null)
			{
				((Control)control).Name = isApplicable ? name : string.Empty;
			}

			public string FilterName
			{
				get { return GetType().Name; }
			}

			public bool IsApplicable(ICardContent cardContent, CellContent cell, BMBoardSectionViewModel boardSection = null)
			{
				return true;
			}

			public bool RequiresRedraw
			{
				get { return requiresRedraw; }
				set { requiresRedraw = value; }
			}
			bool requiresRedraw = true;

			public bool RequiresRemoval { get; set; }

			public Action RestoreVisualStateAfterFilterRemovedAction { get; set; }
		}

		#endregion

		public void TestCardStagger_WhenCustomisedCardLayoutWiderThanDefaultIsUsed()
		{
			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;
			customisation.Width = 150;
			customisation.Height = 52;
			customisation.BackgroundColor = "Hot Pink";

			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var component = CreateBucket(system);
			var section = CreateBoardSection(component);
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisation);

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_GG_ReleaseGroup = group.PK;
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section, width: 800, height: 800))
			{
				form.Show();

				Application.DoEvents();

				var cards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals("Should be two cards", 2, cards.Length);
				AssertEquals(task1.PK, cards[0].CardContent.TaskIdentifier);
				AssertEquals(task2.PK, cards[1].CardContent.TaskIdentifier);

				AssertEquals("First card left offset should just include the border padding", ControlDpiScalingHelper.ScaleToCurrentDpiX(3), cards[0].Left);
				AssertEquals("Second card should stack up against the first card - not behind it.", cards[0].Left + cards[0].Width, cards[1].Left);
			}
		}

		public void TestCustomisedCardWithNonExistentJobProperty_ShouldNotReportError()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var customisation = Factory.NewWithValidTestData<BMControlCustomisation>();
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;
			customisation.Width = 300;
			customisation.Height = 200;
			customisation.BackgroundColor = "Hot Pink";

			BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisation);

			var fieldLine = customisation.CustomisationLines.AddNew();
			fieldLine.PropertySource = PropertySourceList.Codes.Job;
			fieldLine.PropertyName = "SomethingBlah";
			fieldLine.ControlType = PropertyTypeList.Codes.Text;
			fieldLine.IsReadOnly = true;

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			workflow.FH_GG_ReleaseGroup = group.PK;
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = GetAndShowVisualBoardForm(section))
			{
				AssertEquals(1, form.FindAll<TaskCardControl>().Count());
				AssertNull(ErrorReporter.LastExceptionReported);
			}
		}

		public void TestReleaseSchedulerSection_ShouldShowInOrderOfReleaseSequence()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group, constrainedModeComponent: buffer);

			var section = CreateReleaseSchedulerBoardSection(buffer, group);
			LinkComponents(bucket, buffer);

			var priorityTags = BMSTestHelper.CreateTagDefinition(Factory, "PRI");
			var platinum = BMSTestHelper.CreateTagMagnitude(priorityTags, "PLT", "Platinum", nudge: 10);
			var gold = BMSTestHelper.CreateTagMagnitude(priorityTags, "GLD", "Gold", nudge: 5);

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.DesignateAsCCR(buffer);

			group.Staff.Add(resource);

			AssertNoErrors(section);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.AddTag(gold);
			workflow1.FH_CompletionStatement = "workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.AddTag(platinum);
			workflow2.FH_CompletionStatement = "workflow2";

			workflow1.MakePrerequisiteOf(workflow2);

			var task1 = CreateTask(workflow1, resource.GS_Code, 90);
			var task2 = CreateTask(workflow2, resource.GS_Code, 90);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var taskCards = form.FindAll<TaskCardControl>();

				AssertEquals(2, taskCards.Count());
				AssertEquals("Should order platinum workflow first", workflow2.PK, taskCards.ElementAt(0).CardContent.WorkflowIdentifier);
				AssertEquals("Should order gold workflow second", workflow1.PK, taskCards.ElementAt(1).CardContent.WorkflowIdentifier);
			}
		}

		public void TestBackColorChanged_ShouldSetHeaderControlBackColor()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled).Item2;
			var cell = viewModel.ComponentGrid[0, 0];

			using (var panel = BMSGUITestCase.CreateTaskPanel(cell, viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(panel);
				form.Show();

				cell.BackColor = Color.AliceBlue;
				AssertEquals(Color.AliceBlue, panel.BackColor);
			}
		}

		public void TestBackColorChanged_NoExceptionWhenSetCellBackColorToNull()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled).Item2;
			var cell = viewModel.ComponentGrid[0, 0];

			using (var panel = BMSGUITestCase.CreateTaskPanel(cell, viewModel))
			using (var form = new ZForm())
			{
				form.Controls.Add(panel);
				form.Show();

				var originalBackColor = panel.BackColor;
				cell.BackColor = Color.AliceBlue;
				AssertEquals(Color.AliceBlue, panel.BackColor);

				cell.BackColor = null;
				AssertEquals(originalBackColor, panel.BackColor);
			}
		}

		public void TestBackColorChanged_NoException()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var viewModel = BMSTestHelper.CreateSectionAndViewModel(config.Bucket, 1, 2, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled).Item2;
			var cell = viewModel.ComponentGrid[0, 0];

			var i = 0;
			using (var panel = BMSGUITestCase.CreateTaskPanel(cell, viewModel))
			using (var form = new ZForm())
			{
				panel.BackColorChanged += (s, e) => i++;

				form.Controls.Add(panel);
				form.Show();

				var thread = new System.Threading.Thread(() =>
				{
					cell.BackColor = Color.AliceBlue;
				});

				thread.Start();
				thread.Join();
				AssertEquals("The event handler (Cell_BackgroundColorChanged) uses the BeginInvokeSafe to place an action in the queue, so the multiple threads do not use same resources at the same time.", 1, i);
				Application.DoEvents();
				AssertEquals("All events in the queue should be completed", 2, i);
			}
		}

		public void TestCellContentSubscription_ShouldNotLeakControl()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var cell = new CellContent(0, 0, CellContentType.Cards);
			var reference = CreateTaskPanel(cell, config.Bucket);

			GC.Collect(); // It's a unit test
			GC.WaitForFullGCComplete(); // It's a unit test

			AssertNull(reference.Target);
		}

		WeakReference CreateTaskPanel(CellContent cell, BMComponent component)
		{
			var viewModel = BMSTestHelper.CreateSectionAndViewModel(component, 1, 2, FlowDirectionList.Codes.Right, LastCellList.Codes.Bottom, ChannelTypeList.Codes.NotChanneled).Item2;
			var panel = BMSGUITestCase.CreateTaskPanel(cell, viewModel);
			var reference = new WeakReference(panel);

			panel.Dispose();

			return reference;
		}

		[TestDate(2013, 11, 8)]
		public void TestRefreshTasks_Sorting()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			var priorityTags = BMSTestHelper.CreateTagDefinition(Factory, "PRI");
			var priority1 = BMSTestHelper.CreateTagMagnitude(priorityTags, "PR1", nudge: 30);
			var priority2 = BMSTestHelper.CreateTagMagnitude(priorityTags, "PR2", nudge: 20);
			var priority3 = BMSTestHelper.CreateTagMagnitude(priorityTags, "PR3", nudge: 10);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-59);
			workflow1.AddTag(priority3);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";
			workflow2.FH_FC_CurrentComponent = buffer.PK;
			workflow2.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-59);
			workflow2.AddTag(priority2);

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "workflow3";
			workflow3.FH_FC_CurrentComponent = buffer.PK;
			workflow3.FH_ReleaseDateTime = ZDateTime.UtcNow;
			workflow3.AddTag(priority1);

			var channel = Factory.NewWithValidTestData<GlbStaff>();

			var task1 = CreateTask(workflow1, channel.GS_Code, 0, description: "task1", taskStatus: ProcessTaskStatusCodeList.Codes.Working);
			task1.P9_Sequence = 10;
			var task2 = CreateTask(workflow2, channel.GS_Code, 0, description: "task2");
			task2.P9_Sequence = 20;
			var task3 = CreateTask(workflow3, channel.GS_Code, 0, description: "task3");
			task3.P9_Sequence = 30;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 400, height: 400))
			{
				var taskPanel = form.FindAll<TaskPanel>().First();
				var cell = taskPanel.Cell;

				var taskContents = new[] { task1, task2, task3 }.Select(t => new TaskCardContent(t, viewModel)).ToArray();
				taskPanel.SetupTasksForTest(Factory, taskContents, Enumerable.Empty<IBoardFilter>());

				AssertEquals(CardSortType.BufferWorkSequence, cell.CardSortType);

				var taskPanelCards = taskPanel.TaskCards.ToArray();
				AssertEquals(3, taskPanelCards.Length);
				AssertEquals("task1 is set to WRK - should always be highest", task1.PK, taskPanelCards[0].CardContent.TaskIdentifier);
				AssertEquals("task3 has higher priority than task2", task3.PK, taskPanelCards[1].CardContent.TaskIdentifier);
				AssertEquals("task2 is the lowest priority", task2.PK, taskPanelCards[2].CardContent.TaskIdentifier);

				cell.CardSortType = CardSortType.ReleaseSequence;
				taskPanel.SetupTasksForTest(Factory, taskContents, Enumerable.Empty<IBoardFilter>());
				taskPanelCards = taskPanel.TaskCards.ToArray();

				AssertEquals(3, taskPanelCards.Length);
				AssertEquals(task3.PK, taskPanelCards[0].CardContent.TaskIdentifier);
				AssertEquals(task2.PK, taskPanelCards[1].CardContent.TaskIdentifier);
				AssertEquals(task1.PK, taskPanelCards[2].CardContent.TaskIdentifier);
			}

			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-30);
			workflow2.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-60);
			workflow3.FH_ReleaseDateTime = ZDateTime.UtcNow;

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 400, height: 400))
			{
				var taskPanel = form.FindAll<TaskPanel>().First();
				var cell = taskPanel.Cell;

				var taskContents = new[] { task1, task2, task3 }.Select(t => new TaskCardContent(t, viewModel)).ToArray();
				taskPanel.SetupTasksForTest(Factory, taskContents, Enumerable.Empty<IBoardFilter>());

				var taskPanelCards = taskPanel.TaskCards.ToArray();
				AssertEquals(3, taskPanelCards.Length);

				cell.CardSortType = CardSortType.LastTransferTime;
				taskPanel.SetupTasksForTest(Factory, taskContents, Enumerable.Empty<IBoardFilter>());
				taskPanelCards = taskPanel.TaskCards.ToArray();

				AssertEquals(3, taskPanelCards.Length);
				AssertEquals(task2.PK, taskPanelCards[0].CardContent.TaskIdentifier);
				AssertEquals(task1.PK, taskPanelCards[1].CardContent.TaskIdentifier);
				AssertEquals(task3.PK, taskPanelCards[2].CardContent.TaskIdentifier);
			}
		}

		public void TestTaskPanel_ShouldUpdateContextMenusAsPerCurrentFiltersAppliedOnBoardSection()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);
			Factory.Save();
			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(section))
			{
				var controls = form.FindAll<TaskPanel>().ToList();
				var control1 = controls.ElementAt(0);
				var control2 = controls.ElementAt(1);

				var contextMenu = (LazyContextMenuStrip)control1.ContextMenuStrip;
				contextMenu.AddItems_ForTest(control1);

				contextMenu = (LazyContextMenuStrip)control2.ContextMenuStrip;
				contextMenu.AddItems_ForTest(control2);
				var contextMenuItem = contextMenu.Items.OfType<CurrentTaskFilterMenuItem>().Single(m => m.Text.Equals("Show Startable Items"));
				contextMenuItem.PerformClick();
				Assert("Show Startable Items in Context Menu of Task Panel 1 must be shown as Checked", contextMenuItem.Checked);

				var secondContextMenu = control1.ContextMenuStrip;
				contextMenuItem = secondContextMenu.Items.OfType<CurrentTaskFilterMenuItem>().Single(m => m.Text.Equals("Show Startable Items"));
				Assert("Show Startable Items in Context Menu of Task Panel 2 must be shown as Checked", contextMenuItem.Checked);
			}
		}

		public void TestTaskPanel_ShouldUpdateContextMenusAsPerCurrentFiltersAppliedOnBoardSectionForBMComponentControl()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);
			Factory.Save();
			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(section))
			{
				var controls = form.FindAll<TaskPanel>().ToList();
				var bmComponents = form.FindAll<BMComponentControl>().ToList();

				var taskPanel = controls.ElementAt(0);
				var bmComponent = bmComponents.ElementAt(0);

				var bMContextMenu = bmComponent.ContextMenuStrip; //we cast as a task panel for simplicity
				var contextMenuItem = bMContextMenu.Items.OfType<CurrentTaskFilterMenuItem>().Single(m => m.Text.Equals("Show Startable Items"));
				contextMenuItem.PerformClick();
				Assert("Show Startable Items in Context Menu of BMComponent must be show checked", contextMenuItem.Checked);

				var contextMenu = (LazyContextMenuStrip)taskPanel.ContextMenuStrip;
				contextMenu.AddItems_ForTest(taskPanel);
				contextMenuItem = contextMenu.Items.OfType<CurrentTaskFilterMenuItem>().Single(m => m.Text.Equals("Show Startable Items"));
				Assert("Show Startable Items in Context Menu of Task Panel 1 must be shown as Checked", contextMenuItem.Checked);
				contextMenuItem.PerformClick();
				Assert("Show Startable Items in Context Menu of Task Panel 1 must be shown as Not Checked", !contextMenuItem.Checked);

				var secondContextMenu = bmComponent.ContextMenuStrip;
				contextMenuItem = secondContextMenu.Items.OfType<CurrentTaskFilterMenuItem>().Single(m => m.Text.Equals("Show Startable Items"));
				Assert("Show Startable Items in Context Menu of BMComponent must be shown as not Checked", !contextMenuItem.Checked);
			}
		}

		public void TestOnRefreshDeletedBusinessObject()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", buffer, ZDateTime.UtcNow.AddMinutes(-59));

			var channel = Factory.NewWithValidTestData<GlbStaff>();

			var task1 = CreateTask(workflow1, channel.GS_Code, 0, description: "task1", taskStatus: ProcessTaskStatusCodeList.Codes.Working);
			task1.P9_Sequence = 10;
			var task2 = CreateTask(workflow1, channel.GS_Code, 0, description: "task2");
			task2.P9_Sequence = 20;
			var task3 = CreateTask(workflow1, channel.GS_Code, 0, description: "task3");
			task3.P9_Sequence = 30;

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			var cell = new CellContent(0, 0, CellContentType.Cards)
			{
				Channel = viewModel.CreateChannelForTest(channel),
				TimeIndex = 0,
				TimeInCell = TimeSpan.FromHours(1),
				CardSortType = CardSortType.BufferWorkSequence,
			};

			using (var form = GetAndShowVisualBoardForm(section))
			{
				Assert(!task3.IsDeleted);
				task3.Delete();
				Factory.Save();
				Assert(task3.IsDeleted);

				viewModel.RefreshAll(new WorkflowUpdatedOperation(new[] { task3.PK }, Array.Empty<ZGuid>(), Factory));
			}

			workflow1.Delete();
		}

		public void TestContextMenu_EditSchedulesThisSectionMenuItem_ShouldShowMessageIfNoJobsInSection()
		{
			ClickMenuStripItemAndAssertError("Edit Schedules", "This section", "This section has no workflows to edit schedules for.");
		}

		public void TestContextMenu_EditSchedulesThisSectionMenuItem_ShouldOpenFormWithAllWorkflowsFromCell()
		{
			var form = ClickMenuStripItemAndGetForm<MultiJobHeaderEditorForm>("Edit Schedules", "This section");
			var jobWorkflows = form.SchedulesGrid.List;
			AssertNotEquals(0, jobWorkflows.Count);
			var message = "\n" +
				"Given there are 6 tasks (belonging to 6 workflows), \n" +
				"And the section is configured to only show a chanel for staff1 \n" +
				"And 4 of them are in the first cell, 1 in the second, 1 allocted to staff2, \n" +
				"When right clicking on the first cell (with 4 tasks), \n" +
				"And selecting Edit Schedules -> This section, " +
				"Then a new form should open \n" +
				"And its grid should show 5 workflows\n\n";
			AssertContainsExactElementsInAnyOrder(message,
				new[] { "workflow1 - in cell 1", "workflow2 - in cell 1", "workflow3 - in cell 1", "workflow4 - in cell 2", "workflow6 - on buffer 1 - nostaff - capability 2" },
				jobWorkflows.Cast<JobHeaderView>().Select(x => x.ProcessHeader.FH_CompletionStatement));

			form.Close();
		}

		public void TestContextMenu_EditSchedulesThisCellMenuItem_ShouldShowMessageIfNoJobsInSection()
		{
			ClickMenuStripItemAndAssertError("Edit Schedules", "This cell", "This cell has no workflows to edit schedules for.");
		}

		public void TestContextMenu_EditSchedulesThisCellMenuItem_ShouldOpenFormWithAllWorkflowsFromCell()
		{
			var form = ClickMenuStripItemAndGetForm<MultiJobHeaderEditorForm>("Edit Schedules", "This cell");
			var jobWorkflows = form.SchedulesGrid.List;
			AssertNotEquals(0, jobWorkflows.Count);
			var message = "\n" +
				"Given there are 6 tasks (belonging to 6 workflows), \n" +
				"And the section is configured to only show a chanel for staff1 \n" +
				"And 4 of them are in the first cell, 1 in the second, 1 allocted to staff2 \n" +
				"When right clicking on the first cell (with 4 tasks), \n" +
				"And selecting Edit Schedules -> This Cell, \n" +
				"Then a new form should open \n" +
				"And its grid should show 4 workflows\n\n";
			AssertContainsExactElementsInAnyOrder(message,
				new[] { "workflow1 - in cell 1", "workflow2 - in cell 1", "workflow3 - in cell 1", "workflow6 - on buffer 1 - nostaff - capability 2" },
				jobWorkflows.Cast<JobHeaderView>().Select(x => x.ProcessHeader.FH_CompletionStatement));

			form.Close();
		}

		public void TestContextMenu_OpenJobWorkflowsThisSectionMenuItem_ShouldShowMessageIfNoJobsInSection()
		{
			ClickMenuStripItemAndAssertError("Open Job Workflows Module", "This section", "This section has no workflows to open.");
		}

		public void TestContextMenu_OpenJobWorkflowsThisSectionMenuItem_ShouldOpenFormWithAllWorkflowsFromCell()
		{
			var form = ClickMenuStripItemAndGetForm<EmbeddedModulePopup>("Open Job Workflows Module", "This section");
			var jobWorkflows = form.Module_ForTest.GridCollection;
			AssertNotEquals(0, jobWorkflows.Count);
			var message = "\n" +
				"Given there are 6 tasks (belonging to 6 workflows), \n" +
				"And the section is configured to only show a chanel for staff1 \n" +
				"And 4 of them are in the first cell, 1 in the second, 1 allocted to staff2 \n" +
				"When right clicking on the first cell (with 4 tasks), \n" +
				"And selecting Open Job Workflows Module -> This section, \n" +
				"Then a new form should open \n" +
				"And its grid should show 5 workflows\n\n";
			AssertContainsExactElementsInAnyOrder(message,
				new[] { "workflow1 - in cell 1", "workflow2 - in cell 1", "workflow3 - in cell 1", "workflow4 - in cell 2", "workflow6 - on buffer 1 - nostaff - capability 2" },
				jobWorkflows.Cast<ProcessHeader>().Select(x => x.FH_CompletionStatement));

			form.Close();
		}

		public void TestContextMenu_OpenJobWorkflowsThisCellMenuItem_ShouldShowMessageIfNoJobsInSection()
		{
			ClickMenuStripItemAndAssertError("Open Job Workflows Module", "This cell", "This cell has no workflows to open.");
		}

		public void TestContextMenu_OpenJobWorkflowsThisCellMenuItem_ShouldOpenFormWithAllWorkflowsFromCell()
		{
			var form = ClickMenuStripItemAndGetForm<EmbeddedModulePopup>("Open Job Workflows Module", "This cell");
			var jobWorkflows = form.Module_ForTest.GridCollection;
			AssertNotEquals(0, jobWorkflows.Count);
			var message = "\n" +
				"Given there are 6 tasks (belonging to 6 workflows), \n" +
				"And the section is configured to only show a chanel for staff1 \n" +
				"And 4 of them are in the first cell, 1 in the second, 1 allocted to staff2 \n" +
				"When right clicking on the first cell (with 4 tasks), \n" +
				"And selecting Open Job Workflows Module -> This cell, \n" +
				"Then a new form should open \n" +
				"And its grid should show 4 workflows\n\n";
			AssertContainsExactElementsInAnyOrder(message,
				new[] { "workflow1 - in cell 1", "workflow2 - in cell 1", "workflow3 - in cell 1", "workflow6 - on buffer 1 - nostaff - capability 2" },
				jobWorkflows.Cast<ProcessHeader>().Select(x => x.FH_CompletionStatement));

			form.Close();
		}

		void ClickMenuStripItemAndAssertError(string menuStripItem, string menuStripDropdown, string errorMessage)
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);

			Factory.Save();

			var viewModel = VisualBoardFormBasherTest.GetViewModel(config.BufferBoard);

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(config.BufferBoard.PK, viewModel.CurrentBoardViewModel.BoardPK);

				var controls = form.FindAll<TaskPanel>();
				var control = controls.First();

				var contextMenu = (LazyContextMenuStrip)control.ContextMenuStrip;
				contextMenu.AddItems_ForTest(control);
				AssertNotNull(contextMenu);

				var editShedulesMenuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().Single(m => m.Text == menuStripItem);
				var thisCellMenuItem = editShedulesMenuItem.DropDownItems.OfType<ZToolStripMenuItem>().Single(m => m.Text == menuStripDropdown);

				AssertNotNull($" \nGiven a visual board with {menuStripDropdown}, \nWhen right clicking anywhere on {menuStripDropdown}, \nThen {menuStripItem} -> {menuStripDropdown} menu item should be available\n\n", thisCellMenuItem);
				thisCellMenuItem.PerformClick();

				AssertEquals($" \nGiven a visual board with no tasks in {menuStripDropdown}, \nWhen right clicking anywhere on {menuStripDropdown} \nAnd selecting {menuStripItem} -> {menuStripDropdown}), \nThen a message dialog should show that there are no workflows.\n\n",
					errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		T ClickMenuStripItemAndGetForm<T>(string menuStripItem, string menuStripDropdown) where T : ZForm
		{
			var capability1 = BMSTestHelper.CreateCapability(Factory, "VVV", "V for Vendetta");
			var capability2 = BMSTestHelper.CreateCapability(Factory, "MMM", "mmmmmmmmm nutella");

			var staff1 = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, "ABC", "AbbCCC", capability1, capability2);
			var staff2 = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, "XYZ", "Phisycs yo", capability1);
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var buffer1 = config.Buffer;
			var board = config.BufferBoard;

			var jobHeader = CreateJobHeader<OrgHeader>(false, "jh1");

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, staff1.PK, true);

			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow1 - in cell 1", buffer1, releaseDateTime: ZDateTime.Now.AddDays(-1), staffCode: staff1.GS_Code);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow2 - in cell 1", buffer1, releaseDateTime: ZDateTime.Now.AddDays(-1), staffCode: staff1.GS_Code);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow3 - in cell 1", buffer1, releaseDateTime: ZDateTime.Now.AddDays(-1), staffCode: staff1.GS_Code);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow4 - in cell 2", buffer1, releaseDateTime: ZDateTime.Now.AddDays(-5), staffCode: staff1.GS_Code);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow5 - on buffer 1 - staff 2 - capability 1", buffer1, releaseDateTime: ZDateTime.Now.AddDays(-2), staffCode: staff2.GS_Code, capability: capability1);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow6 - on buffer 1 - nostaff - capability 2", buffer1, releaseDateTime: ZDateTime.Now.AddDays(-1), capability: capability2);

			Factory.Save();

			var viewModel = VisualBoardFormBasherTest.GetViewModel(config.BufferBoard);

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(config.BufferBoard.PK, viewModel.CurrentBoardViewModel.BoardPK);

				var controls = form.FindAll<TaskPanel>();
				var cells = controls.Where(w => !w.TaskCards.IsNullOrEmpty());
				AssertNotNull(cells);
				var cell2 = cells.Last();

				var contextMenu = (LazyContextMenuStrip)cell2.ContextMenuStrip;
				contextMenu.AddItems_ForTest(cell2);
				AssertNotNull(contextMenu);

				var editShedulesMenuItem = contextMenu.Items.OfType<ZToolStripMenuItem>().Single(m => m.Text == menuStripItem);
				var thisCellMenuItem = editShedulesMenuItem.DropDownItems.OfType<ZToolStripMenuItem>().Single(m => m.Text == menuStripDropdown);
				thisCellMenuItem.PerformClick();

				var popupForm = BMSFormTestHelper.GetOpenForms<T>().FirstOrDefault();
				AssertNotNull($" \nGiven 2 task panels, \nwhen right clicking on {menuStripDropdown} \nAnd selecting {menuStripItem} -> {menuStripDropdown}, \nThen a new {typeof(T)} should open\n\n", popupForm);
				AssertNull("Form should not be modal", popupForm.Parent);
				return popupForm;
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestControlClickMultipleTotalTasksButtons_ShouldOpenAllPanels()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartmentPK);

			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.CellsPerSubsection = 4;
			section.SectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Age;
			section.SectionConfiguration.TimeField = TimeProgressionFieldList.Codes.TransferTime;
			section.SectionConfiguration.TimePerCell = new ZInt(60 * 8).GetDateTimeFromMinutes();
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = CreateWorkflow(jobHeader, "Workflow1", releaseGroupPK: group.PK);
			var workflow2 = CreateWorkflow(jobHeader, "Workflow2", releaseDateTime: ZDateTime.UtcNow.AddDays(-1), releaseGroupPK: group.PK);
			var workflow3 = CreateWorkflow(jobHeader, "Workflow3", releaseDateTime: ZDateTime.UtcNow.AddDays(-2), releaseGroupPK: group.PK);
			var workflow4 = CreateWorkflow(jobHeader, "Workflow4", releaseDateTime: ZDateTime.UtcNow.AddDays(-3), releaseGroupPK: group.PK);

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 20);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 20);
			var task3 = CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 20);
			var task4 = CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 20);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var allPanels = form.FindAll<TaskPanel>().ToArray();

				AssertEquals(4, allPanels.Length);

				CombineAssertions("Should be one ticket per panel", () =>
				{
					foreach (var panel in allPanels)
					{
						AssertEquals(panel.Cell.Label, 1, panel.FindAll<TaskCardControl>().Count());
					}
				});

				var panel1 = form.FindSingle<TaskPanel>(p => p.Cell.TimeIndex == 0);
				var panel2 = form.FindSingle<TaskPanel>(p => p.Cell.TimeIndex == 1);
				var panel3 = form.FindSingle<TaskPanel>(p => p.Cell.TimeIndex == 2);
				var panel4 = form.FindSingle<TaskPanel>(p => p.Cell.TimeIndex == 3);

				foreach (var panel in allPanels)
				{
					var modifierKeysProviderMock = new Mock<TaskPanel.IModifierKeysProvider>();
					modifierKeysProviderMock.Setup(m => m.ModifierKeys).Returns(Keys.Control);
					panel.ModifierKeysProvider = modifierKeysProviderMock.Object;
				}

				ClickAllTasksButton(panel1);
				AssertPanelShowingAllTasksPanel(panel1, 1, true);
				AssertPanelShowingAllTasksPanel(panel2, 2, false);
				AssertPanelShowingAllTasksPanel(panel3, 3, false);
				AssertPanelShowingAllTasksPanel(panel4, 4, false);

				ClickAllTasksButton(panel2);
				AssertPanelShowingAllTasksPanel(panel1, 1, true);
				AssertPanelShowingAllTasksPanel(panel2, 2, true);
				AssertPanelShowingAllTasksPanel(panel3, 3, false);
				AssertPanelShowingAllTasksPanel(panel4, 4, false);

				ClickAllTasksButton(panel3);
				AssertPanelShowingAllTasksPanel(panel1, 1, true);
				AssertPanelShowingAllTasksPanel(panel2, 2, true);
				AssertPanelShowingAllTasksPanel(panel3, 3, true);
				AssertPanelShowingAllTasksPanel(panel4, 4, false);

				ClickAllTasksButton(panel4);
				AssertPanelShowingAllTasksPanel(panel1, 1, true);
				AssertPanelShowingAllTasksPanel(panel2, 2, true);
				AssertPanelShowingAllTasksPanel(panel3, 3, true);
				AssertPanelShowingAllTasksPanel(panel4, 4, true);

				ClickAllTasksButton(panel4);
				AssertPanelShowingAllTasksPanel(panel1, 1, true);
				AssertPanelShowingAllTasksPanel(panel2, 2, true);
				AssertPanelShowingAllTasksPanel(panel3, 3, true);
				AssertPanelShowingAllTasksPanel(panel4, 4, false);

				ClickAllTasksButton(panel3);
				AssertPanelShowingAllTasksPanel(panel1, 1, true);
				AssertPanelShowingAllTasksPanel(panel2, 2, true);
				AssertPanelShowingAllTasksPanel(panel3, 3, false);
				AssertPanelShowingAllTasksPanel(panel4, 4, false);

				ClickAllTasksButton(panel2);
				AssertPanelShowingAllTasksPanel(panel1, 1, true);
				AssertPanelShowingAllTasksPanel(panel2, 2, false);
				AssertPanelShowingAllTasksPanel(panel3, 3, false);
				AssertPanelShowingAllTasksPanel(panel4, 4, false);

				ClickAllTasksButton(panel1);
				AssertPanelShowingAllTasksPanel(panel1, 1, false);
				AssertPanelShowingAllTasksPanel(panel2, 2, false);
				AssertPanelShowingAllTasksPanel(panel3, 3, false);
				AssertPanelShowingAllTasksPanel(panel4, 4, false);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestTotalTasksButtonClicked_ShouldOnlyShowOnePanelPerBoard()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartmentPK);

			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.CellsPerSubsection = 2;
			section.SectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Age;
			section.SectionConfiguration.TimeField = TimeProgressionFieldList.Codes.TransferTime;
			section.SectionConfiguration.TimePerCell = new ZInt(60 * 8).GetDateTimeFromMinutes();
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = CreateWorkflow(jobHeader, "Workflow1", releaseGroupPK: group.PK);
			var workflow2 = CreateWorkflow(jobHeader, "Workflow2", releaseDateTime: ZDateTime.UtcNow.AddDays(-1), releaseGroupPK: group.PK);

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 20);
			var task2 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 20);
			var task3 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 20);
			var task4 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 20);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var panel1 = form.FindSingle<TaskPanel>(p => p.Cell.TimeIndex == 0);
				var panel2 = form.FindSingle<TaskPanel>(p => p.Cell.TimeIndex == 1);

				ClickAllTasksButton(panel1);
				AssertPanelShowingAllTasksPanel(panel1, 1, true);
				AssertPanelShowingAllTasksPanel(panel2, 2, false);

				ClickAllTasksButton(panel1);
				AssertPanelShowingAllTasksPanel(panel1, 1, false);
				AssertPanelShowingAllTasksPanel(panel2, 2, false);

				ClickAllTasksButton(panel2);
				AssertPanelShowingAllTasksPanel(panel1, 1, false);
				AssertPanelShowingAllTasksPanel(panel2, 2, true);

				ClickAllTasksButton(panel1);
				AssertPanelShowingAllTasksPanel(panel1, 1, true);
				AssertPanelShowingAllTasksPanel(panel2, 2, false);

				ClickAllTasksButton(panel1);
				AssertPanelShowingAllTasksPanel(panel1, 1, false);
				AssertPanelShowingAllTasksPanel(panel2, 2, false);
			}
		}

		public void TestTotalTasksButtonClicked_ShouldShowNotShownCards()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "W1");

			var twentyTasks = Enumerable.Range(0, 20).Select(_ => CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20)).ToList();

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var panel = form.FindAll<TaskPanel>().First();
				ClickAllTasksButton(panel);
				AssertPanelShowingAllTasksPanel(panel, 1, true);
				var control = form.FindSingle<CellTasksControl>();
				AssertEquals(20, control.FindAll<TaskCardControl>().Count());
			}
		}

		[TestDate(2021, 9, 20, 4, 30, 00)]
		public void TestPanel_When_TimeProgressionField_Is_WorkingTimeSinceStartable_ShouldOnlyShowStartableTasks_And_InTheCorrectCells()
		{
			var componentBranch = Factory.NewWithValidTestData<GlbBranch>();
			componentBranch.GB_Code = "BR2";
			componentBranch.GB_RL_NKHomePort = "NLRTM"; // Netherlands/Rotterdam - difference with UTC +2:00

			var componentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, componentDepartment.PK);

			var boardBranch = Factory.NewWithValidTestData<GlbBranch>();
			boardBranch.GB_Code = "BR5";
			boardBranch.GB_RL_NKHomePort = "CNSHA"; // China/Shanghai - difference with UTC +8:00

			var boardDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, boardDepartment.PK);

			var system = CreateSystem("ORG");

			var buffer = CreateBuffer(system);
			buffer.FC_GB_AgingBranch = componentBranch.PK;
			buffer.FC_GE_AgingDepartment = componentDepartment.PK;
			buffer.FC_BufferTimespanInMinutes = 180;

			var section = CreateBoardSection(buffer);
			section.Board.MB_GB_AgingBranch = boardBranch.PK;
			section.Board.MB_GE_AgingDepartment = boardDepartment.PK;

			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Age;
			section.SectionConfiguration.TimeField = TimeProgressionFieldList.Codes.WorkingTimeSinceStartable;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "W1");
			var twentyTasks = Enumerable.Range(0, 20).Select(_ => CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20)).ToList(); // only the first of these will be startable

			Factory.Save();

			TestDateAttribute.AddHours(2);

			var newStaff = Factory.NewWithValidTestData<GlbStaff>(); // another startable task in the same workflow
			var newTask = CreateTask(workflow, newStaff.GS_Code, 20, sequence: 1);
			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				// CHI time:
				//		Task: startable since 04:30 + 8:00 = 12:30
				//		NewTask: startable since 06:30 + 8:00 = 14:30
				//
				//		time now: 12:30 + 2:00 = 14:30, 2 working hours into the buffer, i.e.
				//			cell 6 for Task(77.8 %)
				//			cell 0 for NewTask (0%)
				//
				// NED times (UTC+2) would have produced different times / cells

				for (int i = 0; i < 10; i++)
				{
					var panel = form.FindSingle<TaskPanel>(p => p.Cell.TimeIndex == i);

					if (i == 0 || i == 6)
					{
						ClickAllTasksButton(panel);
						AssertPanelShowingAllTasksPanel(panel, i, true);
					}
					else
					{
						AssertPanelShowingAllTasksPanel(panel, i, false);
					}
				}
			}
		}

		static void ClickAllTasksButton(TaskPanel panel)
		{
			panel.Controls.OfType<Button>().First().PerformClick();
		}

		static void AssertPanelShowingAllTasksPanel(TaskPanel panel, int day, bool shouldBeShowingPanel)
		{
			var form = panel.FindForm() as VisualBoardForm;
			var cellTasksControl = form.FindSingleOrDefault<CellTasksControl>(c => c.Cell == panel.Cell);

			AssertEquals(shouldBeShowingPanel, cellTasksControl != null);
		}

		public void TestTotalTasksButton_WhenRecreatingFailed_ShouldReportException()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "Workflow", bucket, releaseGroupPK: group.PK);

			CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 150, height: 200))
			{
				var panel = form.FindSingle<TaskPanel>();

				panel.SetupTasksForTest(Factory, Enumerable.Empty<IBoardFilter>(), requiresFullRedraw: false);
				Application.DoEvents();

				var totalTasksButton = panel.Controls.OfType<ZButton>().FirstOrDefault();

				AssertNotNull("Should display total number of cards", totalTasksButton);
				AssertEquals("1", totalTasksButton.Text);

				ErrorReporter.Clear();
				var someException = new Exception("On No!");

				totalTasksButton.Disposed += (_, __) => throw someException;
				totalTasksButton.HandleCreated += TotalTasksButton_HandleCreated;
				totalTasksButton.HandleDestroyed += TotalTasksButton_HandleDestroyed;
				panel.SetupTasksForTest(Factory, Enumerable.Empty<IBoardFilter>(), requiresFullRedraw: false);

				Application.DoEvents();

				AssertEquals("Error when creating total tasks button", ErrorReporter.LastMessageReported);
				AssertEquals(someException, ErrorReporter.LastExceptionReported);

				ErrorReporter.Clear();
			}
		}

		void TotalTasksButton_HandleDestroyed(object sender, EventArgs e)
		{
		}

		void TotalTasksButton_HandleCreated(object sender, EventArgs e)
		{
		}

		public void TestAddTasks_ShouldSpaceEvenlyAcrossPanel()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "Workflow", releaseGroupPK: group.PK);

			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);
			var task3 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);
			var task4 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 328, height: 328))
			{
				var panel = form.FindSingle<TaskPanel>();

				panel.SetupTasksForTest(Factory, Enumerable.Empty<IBoardFilter>());
				var taskCards = panel.TaskCards.ToArray();

				AssertEquals("Should create tasks after all tasks are added", 4, taskCards.Length);
				CombineAssertions(() =>
				{
					AssertPointCloseEnough(ControlDpiScalingHelper.NewScaledPoint(3, 3), taskCards[0].Location);
					AssertPointCloseEnough(ControlDpiScalingHelper.NewScaledPoint(60, 13), taskCards[1].Location);
					AssertPointCloseEnough(ControlDpiScalingHelper.NewScaledPoint(117, 23), taskCards[2].Location);
					AssertPointCloseEnough(ControlDpiScalingHelper.NewScaledPoint(174, 33), taskCards[3].Location);
				});
			}
		}

		public void TestAddTasksForWorkflows_ShouldShowFirstTaskInWorkflow()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow1 = CreateWorkflow(jobHeader, "W1");
			var workflow2 = CreateWorkflow(jobHeader, "W2");

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 20);
			var task2 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 20);
			var task3 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 20);
			var task4 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 20);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new ZForm())
			using (var panel = BMSGUITestCase.CreateTaskPanel(new CellContent(0, 0, CellContentType.Cards) { Label = "Day 1", Channel = viewModel.CreateChannelForTest(GlbStaff.CurrentUser) }, viewModel))
			{
				panel.Size = ControlDpiScalingHelper.NewScaledSize(300, 300);

				form.Controls.Add(panel);
				form.Show();

				panel.AddTask(task1);
				panel.AddTask(task2);
				panel.AddTask(task3);
				panel.AddTask(task4);

				AssertEquals(0, panel.Controls.Count);

				BMBoardSectionViewModel.CreateAndPopulatePropertyCache(
					TaskChannelMap.ForTest(section, viewModel, workflow1, workflow2),
					viewModel);

				panel.SetupTasksForTest(Factory, Enumerable.Empty<IBoardFilter>());
				var taskCards = panel.Controls.OfType<TaskCardControl>().ToArray();

				AssertEquals("Should create tasks after all tasks are added", 2, taskCards.Length);
				AssertEquals(workflow1.PK, taskCards[0].CardContent.WorkflowIdentifier);
				AssertEquals(workflow2.PK, taskCards[1].CardContent.WorkflowIdentifier);

				AssertEquals(task1.PK, taskCards[0].CardContent.TaskIdentifier);
				AssertEquals(task3.PK, taskCards[1].CardContent.TaskIdentifier);
			}
		}

		public void TestResizePanel_ShouldAdjustControls()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "Workflow", bucket, releaseGroupPK: group.PK);

			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);
			CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);
			CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);
			CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 150, height: 200))
			{
				var panel = form.FindSingle<TaskPanel>();

				panel.SetupTasksForTest(Factory, Enumerable.Empty<IBoardFilter>());
				Application.DoEvents();
				AssertEquals("Should be just two task cards", 2, panel.TaskCards.Count());
				AssertEquals("Should be just one visible task card", 1, panel.TaskCards.Count(c => c.Visible));

				var taskCountLabel = panel.Controls.OfType<ZButton>().FirstOrDefault();
				AssertNotNull("Should display total number of cards", taskCountLabel);
				AssertEquals("4", taskCountLabel.Text);
			}
		}

		public void TestSetupTasks_ShouldRemoveDeletedOrCompleteTasksOrThoseNoLongerAssignedToChannel()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "The Bed Header", bucket);

			var task1 = CreateTask(workflow, staff.GS_Code, 30);
			var task2 = CreateTask(workflow, staff.GS_Code, 30, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task3 = CreateTask(workflow, staff.GS_Code, 30, taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);
			var task4 = CreateTask(workflow, "E", 30);
			var task5 = CreateTask(workflow, staff.GS_Code, 30);

			var tasks = new[] { task1, task2, task3, task4, task5 };

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 300, height: 300))
			{
				var panel = form.FindSingle<TaskPanel>();

				task5.Delete();
				Factory.Save();

				form.RefreshBoard();
				Application.DoEvents();

				AssertEquals(2, panel.Controls.Count);

				var taskCards = panel.TaskCards.ToArray();
				AssertEquals("Should be just one task card", 1, taskCards.Length);
				AssertEquals(task1.PK, taskCards[0].CardContent.TaskIdentifier);
			}
		}

		public void TestSetupTasks_ShouldShowTasksInPenetratedComponents()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = CreateBuffer(system);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "The Bed Header", buffer);
			var task = CreateTask(workflow, staff.GS_Code, 30);

			var subBuffer = buffer.ChildComponents.AddNew();
			subBuffer.FC_Name = "sub-buffer";
			subBuffer.FC_BufferTimespanInMinutes = 5760;

			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 300, height: 300))
			{
				var panel = form.FindAll<TaskPanel>().First();

				panel.SetupTasksForTest(Factory, Enumerable.Empty<IBoardFilter>());
				var taskCards = panel.TaskCards.ToArray();
				AssertEquals("Should be just one task card", 1, taskCards.Length);
				AssertEquals(task.PK, taskCards[0].CardContent.TaskIdentifier);
			}
		}

		public void TestMoveTaskCardToAnotherComponent_ShouldRemoveTaskCard()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "Bucket 1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "Bucket 2";
			var link1_2 = bucket1.FromMeToOthersLinks.AddNew();
			link1_2.FL_FC_ComponentTo = bucket2.PK;

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			workflow.FH_FC_CurrentComponent = bucket2.PK;
			workflow.FH_GG_ReleaseGroup = group.PK;
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			var section = CreateBoardSection(bucket2);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 300, height: 300))
			{
				var panel = form.FindSingle<TaskPanel>();

				panel.SetupTasksForTest(Factory, Enumerable.Empty<IBoardFilter>());

				var taskCard = panel.TaskCards.Single();
				workflow.MoveToComponent(bucket1);
				Factory.Save();

				AssertEquals(bucket1.PK, workflow.FH_FC_CurrentComponent);

				form.RefreshBoard();
				Application.DoEvents();

				AssertEquals(0, panel.TaskCards.Count());
			}
		}

		public void TestSetupTasks_StackedPanel_TaskCardsShouldStackToTheRightWhenFellOffTheBottomPanel()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var workflow = CreateWorkflow(CreateJobHeader<OrgHeader>(), "workflow", config.Buffer);
			BMSTestHelper.CreateTask(workflow, staffCode: GlbStaff.CurrentUser.GS_Code);
			BMSTestHelper.CreateTask(workflow, staffCode: GlbStaff.CurrentUser.GS_Code);

			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Stacked;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 500, height: 100))
			{
				var panel = form.FindAll<TaskPanel>().First();

				panel.SetupTasksForTest(Factory, Enumerable.Empty<IBoardFilter>());

				AssertEquals("Should be two task cards", 2, panel.TaskCards.Count());

				var taskCard1 = panel.TaskCards.ElementAt(0);
				var taskCard2 = panel.TaskCards.ElementAt(1);

				AssertEquals(taskCard1.Location.Y, taskCard2.Location.Y);
				AssertEquals(taskCard1.Location.X + taskCard1.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(4), taskCard2.Location.X);
				Assert(taskCard1.Visible);
				Assert(taskCard2.Visible);
			}
		}

		public void TestSetupTasks_StackedPanel_TaskCardsShouldNotBeVisibleWhenNotEnoughSpaceToTheRight()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var workflow = CreateWorkflow(CreateJobHeader<OrgHeader>(), "workflow", config.Buffer);
			BMSTestHelper.CreateTask(workflow, staffCode: GlbStaff.CurrentUser.GS_Code);
			BMSTestHelper.CreateTask(workflow, staffCode: GlbStaff.CurrentUser.GS_Code);
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Stacked;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 150, height: 60))
			{
				var panel = form.FindAll<TaskPanel>().First();
				panel.SetupTasksForTest(Factory, Enumerable.Empty<IBoardFilter>());

				AssertEquals("Should be only one task card to fit", 1, panel.TaskCards.Count(c => c.Visible));
			}
		}

		public void TestSetupTasks_StackedPanel_TaskCardsShouldBeVisibleInFirstColumnEvenWhenNotEnoughSpaceToTheRight()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var workflow = CreateWorkflow(CreateJobHeader<OrgHeader>(), "workflow", config.Buffer);
			BMSTestHelper.CreateTask(workflow, staffCode: GlbStaff.CurrentUser.GS_Code);
			BMSTestHelper.CreateTask(workflow, staffCode: GlbStaff.CurrentUser.GS_Code);
			BMSTestHelper.CreateTask(workflow, staffCode: GlbStaff.CurrentUser.GS_Code);
			BMSTestHelper.CreateTask(workflow);
			BMSTestHelper.CreateTask(workflow);
			section.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Stacked;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.CellsPerSubsection = 4;
			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 157, height: 650))
			{
				var panel = form.FindAll<TaskPanel>().First();

				panel.SetupTasksForTest(Factory, Enumerable.Empty<IBoardFilter>());

				AssertEquals("Only 2 task cards should be visible. Cards that don't fit, will not be rendered", 2, panel.TaskCards.Count(c => c.Visible));
				Assert(panel.TaskCards.ElementAt(0).Width > panel.Width);
				Assert(panel.TaskCards.ElementAt(1).Width > panel.Width);
			}
		}

		public void TestTaskLocation_WrapBackToTheTop()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.CellsPerSubsection = 4;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "Workflow");

			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);
			var task3 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);
			var task4 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);
			var task5 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			int cardHeight;

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 300, height: 75))
			{
				var panel = form.FindAll<TaskPanel>().First();

				panel.SetupTasksForTest(Factory, Enumerable.Empty<IBoardFilter>());
				cardHeight = panel.TaskCards.First().Height;
			}

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 300, height: 75))
			{
				var panel = form.FindAll<TaskPanel>().First();

				panel.SetupTasksForTest(Factory, Enumerable.Empty<IBoardFilter>());

				var cards = panel.TaskCards.ToArray();

				AssertEquals("5 cards should have been drawn, and yet... maybe there's a DPI scaling issue.", 5, cards.Length);
				AssertEquals("Last task card should be back at the top since there was no more room at the bottom", ControlDpiScalingHelper.ScaleToCurrentDpiY(TaskPanel.TaskCardPadding), cards[4].Top);
			}
		}

		public void TestTaskLocation_WrapBackToTheLeft()
		{
			var system = CreateSystem("ORG");
			var buffer = CreateBuffer(system);
			var section = CreateBoardSection(buffer);

			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.CellsPerSubsection = 4;

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = CreateWorkflow(jobHeader, "Workflow", buffer);

			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);
			var task3 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);
			var task4 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);
			var task5 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 20);

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardFormWithTaskPanel(section.Board, width: 245, height: 750))
			{
				var panel = form.FindAll<TaskPanel>().First();

				panel.SetupTasksForTest(Factory, Enumerable.Empty<IBoardFilter>());

				var cards = panel.TaskCards.ToArray();

				AssertCloseEnough("Last task card should be back at the left since there was no more room at the top", ControlDpiScalingHelper.ScaleToCurrentDpiX(TaskPanel.TaskCardPadding), cards[4].Left);
			}
		}

		public static VisualBoardForm GetAndShowVisualBoardFormWithTaskPanel(BMBoard board, int width = 200, int height = 200)
		{
			Action<VisualBoardForm, int, int> formAction = ((form, w, h) =>
			{
				var taskPanel = form.FindAll<TaskPanel>().First();
				taskPanel.MaximumSize = ControlDpiScalingHelper.NewScaledSize(w, h);
				taskPanel.Size = ControlDpiScalingHelper.NewScaledSize(w, h);
			});

			return GetAndShowVisualBoardForm(board, width, height, formAction);
		}

		protected override bool ShouldDisableAsyncBehaviour => true;
	}
}

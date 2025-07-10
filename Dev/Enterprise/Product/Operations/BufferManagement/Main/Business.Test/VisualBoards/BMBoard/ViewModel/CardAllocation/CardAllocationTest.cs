using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Pipes.Test;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class CardAllocationTest : BMSTestCaseWithFactory
	{
		#region Deferring Workflow

		[RequiresSTA]
		public void TestDeferWorkflow()
		{
			var buffer = CreateBuffer(System);

			var section = CreateBoardSection(buffer);
			section.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow1", buffer);
			var task1 = BMSTestHelper.CreateTask(workflow1, description: "task1");

			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow2", buffer);
			var task2 = BMSTestHelper.CreateTask(workflow2, description: "task2");

			Factory.Save();

			AssertEquals("workflow1 is in buffer", true, workflow1.CurrentComponent.IsBuffer);

			var sectionViewModel = VisualBoardsTestHelper.CreateViewModel(section);

			AssertNotNull(sectionViewModel.ComponentGrid);

			var newFactory = new BusinessObjectFactory() { NameForDebugging = "AssertDeferWorkflowAndGetComponentGrid" };
			newFactory.Loaded += (sender, eventArgs) =>
			{
				if (eventArgs.NewObjects.OfType<ProcessTask>().Any())
				{
					var workflowToDefer = Factory.Load<ProcessHeader>(workflow1.PK);
					workflowToDefer.FH_FC_CurrentComponent = Bucket2.PK;
					Factory.Save();
				}
			};

			var loadCardContentSetup = new LoadCardContentSetup(sectionViewModel, () => newFactory);
			var engine = BoardSectionRefreshPipeEngine.Create(new BoardRefreshEventArgs(), loadCardContentSetup, null);
			var dispatcher = new MockDispatcher();
			engine.ExecuteAll(dispatcher, dispatcher);
			dispatcher.DispatchAll();

			AssertEquals("workflow1 should not be in a buffer because it has been deferred", false, workflow1.CurrentComponent.IsBuffer);
			AssertEquals("Workflow2 should still in buffer because it has not been deferred", true, workflow2.CurrentComponent.IsBuffer);

			var cardAllocationMap = sectionViewModel.ComponentGrid.CardAllocationMap;

			AssertNotNull("Deferred task1 should be showed because nothing has removed it", cardAllocationMap.GetCells(task1.PK).SingleOrDefault());
			AssertNotNull("Non-deferred task2 should be shown because nothing has removed it", cardAllocationMap.GetCells(task2.PK).SingleOrDefault());
		}

		[TestDate(2017, 1, 2)]
		public void TestCardAllocationMap_WhenProcessHeaderIsUnexpectedlyNull_ShouldNotDieHorribly()
		{
			var buffer = CreateBuffer(System);
			var section = CreateBoardSection(buffer);
			section.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow1", buffer);
			var task1 = BMSTestHelper.CreateTask(workflow1, Staff1.GS_Code, description: "task1");

			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow1", buffer);
			var task2 = BMSTestHelper.CreateTask(workflow2, Staff1.GS_Code, description: "task2");

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(5);

			var sectionViewModel = VisualBoardsTestHelper.CreateViewModel(section);

			AssertNotNull(sectionViewModel.ComponentGrid);

			AssertEquals("Penetration has not been calculated and cached", 0m, sectionViewModel.Cache.GetCachedValue<decimal>(workflow1.PK, TaskJobWorkflowCacheHelper.CacheConstants.Penetration));

			sectionViewModel.BeforeCardAllocation_ForTest += (sender, eventArgs) =>
			{
				task1.P9_FH_ProcessHeader = ZGuid.Empty;
				Factory.Save();
			};

			var taskChannelMap = TaskChannelMap.ForTest(section, sectionViewModel, workflow1);

			AssertNoExceptionThrown("Allocating cards when a processHeader goes AWOL doesn't crash everything we care about", () => CardAllocationMap.NewAllocationMap(section, sectionViewModel, taskChannelMap));
		}

		#endregion

		public void TestCardAllocationMap_BasicAssignment()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Second verse", Bucket1);
			var task1 = BMSTestHelper.CreateTask(workflow, Staff1.GS_Code);
			var task2 = BMSTestHelper.CreateTask(workflow, Staff1.GS_Code);

			var boardViewModel = VisualBoardsTestHelper.CreateBoardViewModel(Board);
			var section1ViewModel = new BMBoardSectionViewModel(Section1, boardViewModel);
			var section2ViewModel = new BMBoardSectionViewModel(Section1, boardViewModel);

			AssertNotNull(section1ViewModel.ComponentGrid);

			var taskChannelMap = TaskChannelMap.ForTest(Section1, section1ViewModel, workflow);
			var cardAllocationMap = CardAllocationMap.NewAllocationMap(Section1, section1ViewModel, taskChannelMap);

			AssertEquals(Staff1.PK, cardAllocationMap.GetCells(task1.PK).Single().Channel.EntityPK);
			AssertEquals(Staff1.PK, cardAllocationMap.GetCells(task2.PK).Single().Channel.EntityPK);

			task1.P9_GS_NKAssignedStaffMember = Staff2.GS_Code;

			taskChannelMap = TaskChannelMap.ForTest(Section1, section1ViewModel, workflow);
			cardAllocationMap = CardAllocationMap.NewAllocationMap(Section1, section1ViewModel, taskChannelMap);

			AssertEquals(Staff2.PK, cardAllocationMap.GetCells(task1.PK).Single().Channel.EntityPK);
			AssertEquals(Staff1.PK, cardAllocationMap.GetCells(task2.PK).Single().Channel.EntityPK);
		}

		[RequiresSTA]
		public void TestCardAllocation_UpdateOperationCreatesSameMapAsInit()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Same as the first", Bucket1);
			var task1 = BMSTestHelper.CreateTask(workflow, Staff1.GS_Code);
			var task2 = BMSTestHelper.CreateTask(workflow, Staff1.GS_Code);

			Factory.Save();

			var boardViewModel = VisualBoardsTestHelper.CreateBoardViewModel(Board);
			var dataSource = boardViewModel.Build(Board);
			var section1ViewModel = (BMBoardSectionViewModel)boardViewModel.GetSections().First(s => s.SectionPK == Section1.PK);
			var section2ViewModel = (BMBoardSectionViewModel)boardViewModel.GetSections().First(s => s.SectionPK == Section2.PK);

			AssertNotNull(section1ViewModel.ComponentGrid);

			var engine = BoardSectionRefreshPipeEngine.Create(new BoardRefreshEventArgs(), new LoadCardContentSetup(section1ViewModel), null);
			var dispatcher = new MockDispatcher();
			engine.ExecuteAll(dispatcher, dispatcher);
			dispatcher.DispatchAll();

			var taskChannelMap = TaskChannelMap.ForTest(Section1, section1ViewModel, workflow);
			var cardAllocationMap = CardAllocationMap.NewAllocationMap(Section1, section1ViewModel, taskChannelMap);

			AssertEquals(Staff1.PK, cardAllocationMap.GetCells(task1.PK).Single().Channel.EntityPK);
			AssertEquals(Staff1.PK, cardAllocationMap.GetCells(task2.PK).Single().Channel.EntityPK);

			task1.P9_GS_NKAssignedStaffMember = Staff2.GS_Code;

			Factory.Save();

			taskChannelMap = TaskChannelMap.ForTest(Section1, section1ViewModel, workflow);
			cardAllocationMap = CardAllocationMap.NewAllocationMap(Section1, section1ViewModel, taskChannelMap);
			section1ViewModel.RefreshAll(new WorkflowUpdatedOperation(Array.Empty<ZGuid>(), new[] { workflow.PK }, Factory));

			AssertAllocationMapEquals(cardAllocationMap, section1ViewModel.ComponentGrid.CardAllocationMap);
		}

		public void TestCardAllocationMap_NullReferenceException()
		{
			var buffer = CreateBuffer(System);
			var section = CreateBoardSection(buffer);
			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.ShowUnchanneled = true;

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var jobHeader = ProcessJobHeader.GetForParent(enquiry, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = CreateWorkflow(jobHeader, "workflow1", buffer);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", buffer);

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var task1 = CreateTask(workflow1, staff1.GS_Code, 60);
			var task2 = CreateTask(workflow2, staff2.GS_Code, 60);

			//standalone task
			var task = Factory.New<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task.P9_Description = "standaloneTask";

			Factory.Save();

			var boardViewModel = VisualBoardsTestHelper.CreateBoardViewModel(section.Board);
			var sectionViewModel = new BMBoardSectionViewModel(section, boardViewModel);

			var taskChannelMap = TaskChannelMap.ForTest(section, sectionViewModel, jobHeader);
			AssertNoExceptionThrown(delegate
			{ CardAllocationMap.NewAllocationMap(section, sectionViewModel, taskChannelMap); });
		}

		public void TestCardAllocationMap_ShouldHandleRaceCondition_WhenTasksDeleted()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow", Bucket1);
			var task1 = BMSTestHelper.CreateTask(workflow, Staff1.GS_Code, description: "task1");
			var task2 = BMSTestHelper.CreateTask(workflow, Staff1.GS_Code, description: "task2");

			var boardViewModel = VisualBoardsTestHelper.CreateBoardViewModel(Board);
			var section1ViewModel = new BMBoardSectionViewModel(Section1, boardViewModel);
			section1ViewModel.BeforeCardAllocation_ForTest += (sender, eventArgs) =>
			{
				task1.Delete();
				task2.Delete();
			};

			Section1.SectionConfiguration.CardType = CardTypeList.Codes.Task;
			Section1.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			Section1.SectionConfiguration.ShowUnchanneled = true;
			section1ViewModel.DummyStrategy_ForTest = new DummyCardStrategy(forceWorkflowReturnNull: false);

			AssertNotNull(section1ViewModel.ComponentGrid);

			var taskChannelMap = TaskChannelMap.ForTest(Section1, section1ViewModel, workflow);

			AssertNoExceptionThrown(() => CardAllocationMap.NewAllocationMap(Section1, section1ViewModel, taskChannelMap));
		}

		public void TestCardAllocationMap_ShouldHandleRaceCondition_WhenWorkflowsDeleted()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Boop", Bucket1);
			var task1 = BMSTestHelper.CreateTask(workflow, Staff1.GS_Code);
			var task2 = BMSTestHelper.CreateTask(workflow, Staff1.GS_Code);

			var boardViewModel = VisualBoardsTestHelper.CreateBoardViewModel(Board);
			var section1ViewModel = new BMBoardSectionViewModel(Section1, boardViewModel);

			Section1.SectionConfiguration.CardType = CardTypeList.Codes.Task;
			Section1.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			Section1.SectionConfiguration.ShowUnchanneled = true;
			section1ViewModel.DummyStrategy_ForTest = new DummyCardStrategy(forceWorkflowReturnNull: true);

			AssertNotNull(section1ViewModel.ComponentGrid);

			var taskChannelMap = TaskChannelMap.ForTest(Section1, section1ViewModel, workflow);

			AssertNoExceptionThrown(() => CardAllocationMap.NewAllocationMap(Section1, section1ViewModel, taskChannelMap));
		}

		public void TestCardAllocationMap_CloneReturnsNewObject()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Second verse", Bucket1);
			var task1 = BMSTestHelper.CreateTask(workflow, Staff1.GS_Code);
			var task2 = BMSTestHelper.CreateTask(workflow, Staff1.GS_Code);

			var boardViewModel = VisualBoardsTestHelper.CreateBoardViewModel(Board);
			var section1ViewModel = new BMBoardSectionViewModel(Section1, boardViewModel);
			AssertNotNull(section1ViewModel.ComponentGrid);

			var taskChannelMap = TaskChannelMap.ForTest(Section1, section1ViewModel, workflow);
			var originalCardAllocationMap = CardAllocationMap.NewAllocationMap(Section1, section1ViewModel, taskChannelMap);
			var cloneCardAllocationMap = originalCardAllocationMap.Clone();

			AssertNotNull("Original", originalCardAllocationMap);
			AssertNotNull("Clone ", cloneCardAllocationMap);
			AssertNotEquals("Clone != original", originalCardAllocationMap, cloneCardAllocationMap);

			AssertNotNull("CellsByCard Original", originalCardAllocationMap.CellsByCard_ForTest);
			AssertNotNull("CellsByCard Clone", cloneCardAllocationMap.CellsByCard_ForTest);
			AssertNotEquals("CellsByCard Clone != original", originalCardAllocationMap.CellsByCard_ForTest, cloneCardAllocationMap.CellsByCard_ForTest);
			foreach (var card in originalCardAllocationMap.CellsByCard_ForTest.Keys)
			{
				AssertNotEquals("CellsByCard Clone != original", originalCardAllocationMap.CellsByCard_ForTest[card], cloneCardAllocationMap.CellsByCard_ForTest[card]);
				AssertSequencesEqual("Contents should be same", originalCardAllocationMap.CellsByCard_ForTest[card], cloneCardAllocationMap.CellsByCard_ForTest[card]);
			}

			AssertNotEquals("CardsByCell Clone != original", originalCardAllocationMap.CardsByCell_ForTest, cloneCardAllocationMap.CardsByCell_ForTest);
			foreach (var cell in originalCardAllocationMap.CardsByCell_ForTest.Keys)
			{
				AssertNotEquals("CardsByCell Clone != original", originalCardAllocationMap.CardsByCell_ForTest[cell], cloneCardAllocationMap.CardsByCell_ForTest[cell]);
				AssertSequencesEqual("Contents should be same", originalCardAllocationMap.CardsByCell_ForTest[cell], cloneCardAllocationMap.CardsByCell_ForTest[cell]);
			}

			AssertContainsExactElementsInAnyOrder("Card PKs Match", originalCardAllocationMap.GetCardPKs(), cloneCardAllocationMap.GetCardPKs());
			AssertContainsExactElementsInAnyOrder("Cells Match", originalCardAllocationMap.GetCells(), cloneCardAllocationMap.GetCells());
		}

		public void TestFactorylessCardContent_ShouldNotFindNullWorkflowsOrTasks()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "INQ");
			var processHeaderForDeletion = BMSTestHelper.CreateWorkflow(Factory, "I finished");

			config.BufferSection.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			var taskForDeletion = BMSTestHelper.CreateTask(processHeaderForDeletion);
			var viewModel = BMSTestHelper.CreateViewModel(config.BufferSection);
			processHeaderForDeletion.FH_FC_CurrentComponent = viewModel.ComponentPK;
			var map = TaskChannelMap.ForTest(config.BufferSection, viewModel);
			var strategy = new PopulateWorkflowCardStrategy();

			var workflow = taskForDeletion.GetProcessHeaderForCardType(viewModel.ShowJobCards);
			var task = BMSTestHelper.CreateTask(workflow);
			var customisationData = new CustomisedControlDataCache();
			var definitions = new TagDefinitionCache(Factory);

			var dootyBoi = new FactorylessCardContentDto(workflow, task, viewModel, new CustomisedControlDataCache(), new TagDefinitionCache(Factory), strategy);

			Factory.Save();

			taskForDeletion = null;

			AssertNoExceptionThrown("We should not have a null ref thrown despite having our fauxMethod forcing a null ref exception, and yet...",
				() => new FactorylessCardContent(dootyBoi));

			taskForDeletion = BMSTestHelper.CreateTask(processHeaderForDeletion);
			processHeaderForDeletion = null;

			AssertNoExceptionThrown("We should cause a null reference exception doing this, and yet...", () => new FactorylessCardContent(workflow, taskForDeletion, viewModel, customisationData, definitions, strategy));
		}

		#region Assertion Helpers

		void AssertAllocationMapEquals(CardAllocationMap cardAllocationMap1, CardAllocationMap cardAllocationMap2)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expect both maps to have the same number of cells", cardAllocationMap1.CardsByCell_ForTest.Count, cardAllocationMap2.CardsByCell_ForTest.Count);
				AssertEquals("Expected both maps to have the same number of cards", cardAllocationMap1.CellsByCard_ForTest.Count, cardAllocationMap2.CellsByCard_ForTest.Count);

				AssertDictionaryEquals(cardAllocationMap1.CardsByCell_ForTest, cardAllocationMap2.CardsByCell_ForTest);
				AssertDictionaryEquals(cardAllocationMap1.CellsByCard_ForTest, cardAllocationMap2.CellsByCard_ForTest, g => Factory.Load<ProcessTask>(g).P9_Description);
			});
		}

		static void AssertDictionaryEquals<TKey, TValue>(IDictionary<TKey, TValue> dictionary1, IDictionary<TKey, TValue> dictionary2, Func<TKey, string> stringSelector = null)
			where TValue : IEnumerable<object>
		{
			var defaultStringSelector = stringSelector ?? new Func<TKey, string>(k => k.ToString());

			foreach (var pair in dictionary1)
			{
				TValue content;
				if (dictionary2.TryGetValue(pair.Key, out content))
				{
					AssertContainsExactElementsInAnyOrder(string.Format("Expected item {0} [{1}], to have the same elements.", pair.Key.GetType().Name, defaultStringSelector(pair.Key)), pair.Value, content);
				}
				else
				{
					Fail(string.Format("Expected {0} [{1}], but it was only in the first map.", pair.Key.GetType().Name, defaultStringSelector(pair.Key)));
				}
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			System = CreateSystem("ORG");

			Bucket1 = CreateBucket(System);
			Bucket2 = CreateBucket(System, "bucket2");

			Board = CreateBoard(System);
			Section1 = CreateBoardSection(Bucket1, Board, row: 0, col: 0);
			Section2 = CreateBoardSection(Bucket2, Board, row: 0, col: 1);

			Staff1 = CreateStaffInCurrentBranchDept("KOJ", "Kranky Old Jooniper");
			Staff2 = CreateStaffInCurrentBranchDept("KLI", "Kilo Intern");

			VisualBoardsTestHelper.CreatePrimaryChannelForSection(Section1, ChannelTypeList.Codes.Resource, Staff1.PK, true);
			VisualBoardsTestHelper.CreatePrimaryChannelForSection(Section1, ChannelTypeList.Codes.Resource, Staff2.PK, true);

			VisualBoardsTestHelper.CreatePrimaryChannelForSection(Section2, ChannelTypeList.Codes.Resource, Staff1.PK, true);
			VisualBoardsTestHelper.CreatePrimaryChannelForSection(Section2, ChannelTypeList.Codes.Resource, Staff2.PK, true);

			Factory.Save();
		}

		protected override bool ShouldDisableAsyncBehaviour => true;

		public BMSystem System { get; private set; }
		public BMComponent Bucket1 { get; private set; }
		public BMComponent Bucket2 { get; private set; }
		public BMBoard Board { get; private set; }
		public BMBoardSection Section1 { get; private set; }
		public BMBoardSection Section2 { get; private set; }
		public GlbStaff Staff1 { get; private set; }
		public GlbStaff Staff2 { get; private set; }

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture.ActivityLogging;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Microsoft.Win32;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class VisualBoardFormNonTransactionedTest : NonTransactionedTestCase
	{
		#region Acceptability Bands

		public void TestAcceptabilityBandTilesShouldNotRender_WhenWorkflowManagementModeIsEWForBWF()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket, customReleaseGroupPK: group.PK);
			var band = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(bucket, 1, 2, 3, 4, 5, 6);
			var sectionBand = section.SectionConfiguration.AcceptabilityBands.AddNew();
			sectionBand.DisplayUnits = "something";
			sectionBand.AcceptabilityBandPK = band.PK;

			BMSTestHelper.CreateWorkflows(bucket, 1, 1);

			Factory.Save();

			void TestCase(string workflowManagementMode, bool shouldRenderBandTitles)
			{
				BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, workflowManagementMode);

				using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
				using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
				{
					form.AwaitAll();
					AssertEquals(shouldRenderBandTitles, form.FindAll<AcceptabilityBandTileControl>().Any());
					form.ReloadBoard();
					Application.DoEvents();
					AssertEquals(shouldRenderBandTitles, form.FindAll<AcceptabilityBandTileControl>().Any());
				}
			}

			TestCase(WorkflowManagementModes.Codes.BasicWorkflow, shouldRenderBandTitles: false);
			TestCase(WorkflowManagementModes.Codes.EnhancedWorkflow, shouldRenderBandTitles: false);
			TestCase(WorkflowManagementModes.Codes.IncludesBufferManagement, shouldRenderBandTitles: true);
			TestCase(WorkflowManagementModes.Codes.PlanningManagement, shouldRenderBandTitles: true);
		}

		public void TestAcceptabilityBandTiles()
		{
			var system = VisualBoardsTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var band1 = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(bucket, 10, 12, 14, 16, 18, 19);
			var band2 = BMSTestHelper.CreateAcceptabilityBand_AverageNumberOfTasksPerWorkflow(bucket, 2, 3, 4, 5, 6, 7);

			var sectionBand1 = section.SectionConfiguration.AcceptabilityBands.AddNew();
			sectionBand1.DisplayUnits = "bottles of beer";
			sectionBand1.AcceptabilityBandPK = band1.PK;

			BMSTestHelper.CreateWorkflows(bucket, 10, 4);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				var tile1 = form.FindAll<AcceptabilityBandTileControl>().Single();
				AssertEquals(band1.PK, tile1.ViewModel.AcceptabilityBandPK);

				AssertEquals("Number of Workflows: 10 bottles of beer", tile1.NameAndResultLabel.Text);
				AssertColorEquals(BMConstants.CautionBoardColor, tile1.BackColor);

				var sectionBand2 = section.SectionConfiguration.AcceptabilityBands.AddNew();
				sectionBand2.DisplayUnits = "beetles of bott";
				sectionBand2.AcceptabilityBandPK = band2.PK;

				Factory.Save();
				form.ReloadBoard();
				form.AwaitAll();

				AssertEquals("Should regenerate board controls since full reload was performed", true, tile1.IsDisposed);

				var tiles = form.FindAll<AcceptabilityBandTileControl>().ToArray();
				AssertEquals(2, tiles.Length);

				tile1 = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == band1.PK);
				var tile2 = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == band2.PK);

				AssertEquals("Number of Workflows: 10 bottles of beer", tile1.NameAndResultLabel.Text);
				AssertEquals("Number of Tasks per Workflow: 4 beetles of bott", tile2.NameAndResultLabel.Text);
				AssertColorEquals(BMConstants.CautionBoardColor, tile1.BackColor);
				AssertColorEquals(BMConstants.ExcellentBoardColor, tile2.BackColor);

				BMSTestHelper.CreateWorkflows(bucket, 10, 2);
				Factory.Save();

				form.RefreshBoard();
				form.AwaitAll();

				tiles = form.FindAll<AcceptabilityBandTileControl>().ToArray();
				tile1 = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == band1.PK);
				tile2 = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == band2.PK);

				AssertEquals("Number of Workflows: 20 bottles of beer", tile1.NameAndResultLabel.Text);
				AssertEquals("Number of Tasks per Workflow: 3 beetles of bott", tile2.NameAndResultLabel.Text);
				AssertColorEquals(BMConstants.HighRiskBoardColor, tile1.BackColor);
				AssertColorEquals(BMConstants.GoodBoardColor, tile2.BackColor);
			}
		}

		public void TestAcceptabilityBandStatusSharedCacheServiceFactoriesShouldNotLeak()
		{
			var board = GetBoardForTestingAcceptabilityBandStatusSharedCacheServiceFactories();
			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
			}
			GC.Collect();
			GC.WaitForPendingFinalizers();
			AssertEquals("The factories should not leak", 0, PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Count(f => BMSTestCaseWithFactory.IsAcceptabilityBandCalculationFactory(f)));
		}

		BMBoard GetBoardForTestingAcceptabilityBandStatusSharedCacheServiceFactories()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System, "Boardelle");
			var bufferSection = BMSTestHelper.CreateBoardSection(config.Buffer, board, row: 0);
			var bucketSection = BMSTestHelper.CreateBoardSection(config.Bucket, board, row: 1);

			var testBand1_1 = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 1, 2, 3, 4, 5, "Bandit1_1", type: AcceptabilityBandTypes.Codes.Count);
			var testBand1_2 = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 1, 2, 3, 4, 5, "Bandit1_2", type: AcceptabilityBandTypes.Codes.Count);
			var testBand1_3 = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 1, 2, 3, 4, 5, "Bandit1_3", type: AcceptabilityBandTypes.Codes.Count);
			var testBand2_1 = BMSTestHelper.CreateAcceptabilityBand(config.Bucket, 0, 1, 2, 3, 4, 5, "Bandit2_1", type: AcceptabilityBandTypes.Codes.Count);
			var testBand2_2 = BMSTestHelper.CreateAcceptabilityBand(config.Bucket, 0, 1, 2, 3, 4, 5, "Bandit2_2", type: AcceptabilityBandTypes.Codes.Count);
			var testBand2_3 = BMSTestHelper.CreateAcceptabilityBand(config.Bucket, 0, 1, 2, 3, 4, 5, "Bandit2_3", type: AcceptabilityBandTypes.Codes.Count);
			var sectionBand1_1 = BMSTestHelper.AddAcceptabilityBandToSection(bufferSection, testBand1_1, AcceptabilityBandShowOnOption.Tile);
			var sectionBand1_2 = BMSTestHelper.AddAcceptabilityBandToSection(bufferSection, testBand1_2, AcceptabilityBandShowOnOption.Tile);
			var sectionBand1_3 = BMSTestHelper.AddAcceptabilityBandToSection(bufferSection, testBand1_3, AcceptabilityBandShowOnOption.Tile);
			var sectionBand2_1 = BMSTestHelper.AddAcceptabilityBandToSection(bucketSection, testBand2_1, AcceptabilityBandShowOnOption.Tile);
			var sectionBand2_2 = BMSTestHelper.AddAcceptabilityBandToSection(bucketSection, testBand2_2, AcceptabilityBandShowOnOption.Tile);
			var sectionBand2_3 = BMSTestHelper.AddAcceptabilityBandToSection(bucketSection, testBand2_3, AcceptabilityBandShowOnOption.Tile);

			return board;
		}

		#endregion

		#region TaskCardReloading

		public void TestHighlightCardsInWorkflowFilter()
		{
			TaskCardControl.AutoGenerateTaskMenuItems.Value = true;

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.OverrideChannels = true;

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frodo Baggins");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BIL", "Bilbo Baggins");

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var workflow1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			var task1_1 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, 60);
			var task1_2 = BMSTestHelper.CreateTask(workflow1, resource2.GS_Code, 60);

			var workflow2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			var task2_1 = BMSTestHelper.CreateTask(workflow2, resource1.GS_Code, 60);
			var task2_2 = BMSTestHelper.CreateTask(workflow2, resource2.GS_Code, 60);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(4, taskCards.Length);

				var workflow1Task1Card = taskCards.First(t => t.CardContent.TaskIdentifier == task1_1.PK);

				workflow1Task1Card.ContextMenuStrip.Items.Cast<ToolStripItem>().First(i => i.Text == "Highlight Tasks in Workflow").PerformClick();
				form.AwaitAll();

				var regeneratedTaskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals("Applying the filter should make task cards disappear", 2, regeneratedTaskCards.Length);

				workflow1Task1Card = regeneratedTaskCards.First(t => t.CardContent.TaskIdentifier == task1_1.PK);

				for (int i = 0; i < regeneratedTaskCards.Length; i++)
				{
					var card2 = regeneratedTaskCards[i];
					AssertEquals(workflow1Task1Card.CardContent.WorkflowIdentifier, card2.CardContent.WorkflowIdentifier);
				}

				workflow1Task1Card.ContextMenuStrip.Items.Cast<ToolStripItem>().First(i => i.Text == "Highlight Tasks in Workflow").PerformClick();
				form.AwaitAll();

				var reRegeneratedTaskCards = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals("Removing the filter should make all the task cards appear again", 4, reRegeneratedTaskCards.Length);

				workflow1Task1Card = reRegeneratedTaskCards.First(t => t.CardContent.TaskIdentifier == task1_1.PK);

				for (int i = 0; i < reRegeneratedTaskCards.Length; i++)
				{
					var card1 = taskCards[i];
					var card2 = reRegeneratedTaskCards[i];

					AssertEquals(card1.CardContent.TaskIdentifier, card2.CardContent.TaskIdentifier);
					AssertNotEquals(card1, card2);
				}
			}
		}

		#endregion

		#region Board Function Reloading

		public void TestSwitchToConstrainedMode_ShouldReloadBoard()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			BMSTestHelper.CreateConstraint(buffer);

			var section = BMSTestHelper.CreateBoardSection(buffer);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(system, group);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			resource.DesignateAsCCR(buffer);
			group.Staff.Add(resource);

			Factory.Save();

			var viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(section.Board);

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new MockAsyncStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				var componentControl = form.FindAll<BMComponentControl>().Single();
				var menuItem = componentControl.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Switch to Constrained Mode");

				menuItem.PerformClick();
				form.AwaitAll();

				var componentControlAfterSwitchingToConstrainedMode = form.FindAll<BMComponentControl>().Single();
				AssertNotEquals("Should re-create all controls when switching to constrained mode", componentControl, componentControlAfterSwitchingToConstrainedMode);

				menuItem = componentControlAfterSwitchingToConstrainedMode.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Switch to non-Constrained Mode");
				menuItem.PerformClick();
				form.AwaitAll();

				var componentControlAfterSwitchingBackToNonConstrainedMode = form.FindAll<BMComponentControl>().Single();
				AssertNotEquals("Should re-create all controls when switching back to non-constrained mode", componentControlAfterSwitchingToConstrainedMode, componentControlAfterSwitchingBackToNonConstrainedMode);
			}
		}

		#endregion
		
		#region Slideshow

		public void TestSlideShowProgression_IClickFasterThanTheBoardCanHandle()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "badTime";
			var bucket = BMSTestHelper.CreateBucket(system, "bucket", offsetMinutes: 0, sequence: 0);

			var board1 = BMSTestHelper.CreateBoard(system, "board1", "board1");
			var board2 = BMSTestHelper.CreateBoard(system, "board2", "board2");
			var board3 = BMSTestHelper.CreateBoard(system, "board3", "board3");
			var board4 = BMSTestHelper.CreateBoard(system, "board4", "board4");

			var section1 = BMSTestHelper.CreateBoardSection(bucket, board1);
			var section2 = BMSTestHelper.CreateBoardSection(bucket, board2);
			var section3 = BMSTestHelper.CreateBoardSection(bucket, board3);
			var section4 = BMSTestHelper.CreateBoardSection(bucket, board4);
			section1.BackgroundColor = "Blue";
			section2.BackgroundColor = "HotPink";
			section3.BackgroundColor = "Tomato";
			section4.BackgroundColor = "Green";

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2, board3, board4);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(slideshow))
			{
				form.AwaitAll();
				form.NextButton_ExpandAndPerformClickOnBackgroundThread();
				form.AwaitAll(); // NextButton-click perform refresh, wait for it so IsInRefresh/IsInReload is false on next refresh and get executed

				var componentControl1 = form.FindAll<BMComponentControl>().Single();
				AssertEquals("Board will only advance one slide", "board2", form.Text);
				AssertEquals(string.Format("b1: {0}, b2: {1}, b3: {2}, b4: {3}", board1.PK, board2.PK, board3.PK, board4.PK), board2.PK, form.BoardViewModel.BoardPK);
				AssertEquals(string.Format("b1: {0}, b2: {1}, b3: {2}, b4: {3}", board1.PK, board2.PK, board3.PK, board4.PK), board2.PK, componentControl1.ViewModel.BoardViewModel.BoardPK);

				form.NextButton_ExpandAndPerformClickOnBackgroundThread();
				form.NextButton_ExpandAndPerformClickOnBackgroundThread();
				form.NextButton_ExpandAndPerformClickOnBackgroundThread();

				form.AwaitAll();

				var componentControl2 = form.FindAll<BMComponentControl>().Single();
				AssertEquals("Clicking 3 times quickly causes the first click to trigger a refresh, the other clicks do nothing because buttons are .Enabled = false", "board3", form.Text);
				AssertEquals(string.Format("b1: {0}, b2: {1}, b3: {2}, b4: {3}", board1.PK, board2.PK, board3.PK, board4.PK), board3.PK, form.BoardViewModel.BoardPK);
				AssertEquals(string.Format("b1: {0}, b2: {1}, b3: {2}, b4: {3}", board1.PK, board2.PK, board3.PK, board4.PK), board3.PK, componentControl2.ViewModel.BoardViewModel.BoardPK);
			}
		}

		public void TestSlideshowProgression_WhenConfigIsChanged_ShouldNotCacheTableLayouts()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "Dat System";
			var bucket = BMSTestHelper.CreateBucket(system, "bucket", offsetMinutes: 0, sequence: 0);

			var board1 = BMSTestHelper.CreateBoard(system, "board1", "board1");
			var board2 = BMSTestHelper.CreateBoard(system, "board2", "board2");
			var board3 = BMSTestHelper.CreateBoard(system, "board3", "board3");

			var section1 = BMSTestHelper.CreateBoardSection(bucket, board1);
			var section2 = BMSTestHelper.CreateBoardSection(bucket, board2);
			var section3 = BMSTestHelper.CreateBoardSection(bucket, board3);
			section1.BackgroundColor = "Chartreuse";
			section2.BackgroundColor = "HotPink";
			section3.BackgroundColor = "Orange";

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2, board3);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new MockAsyncStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(slideshow))
			{
				form.AwaitAll();
				var componentControl1 = form.FindAll<BMComponentControl>().Single();
				AssertEquals(board1.PK, componentControl1.ViewModel.BoardViewModel.BoardPK);

				form.MoveNext();
				form.AwaitAll();

				var componentControl2 = form.FindAll<BMComponentControl>().Single();
				AssertEquals(board2.PK, componentControl2.ViewModel.BoardViewModel.BoardPK);

				form.MoveNext();
				form.AwaitAll();

				var componentControl3 = form.FindAll<BMComponentControl>().Single();
				AssertEquals(board3.PK, componentControl3.ViewModel.BoardViewModel.BoardPK);

				form.MoveNext();
				form.AwaitAll();

				AssertEquals("Should cache component control", componentControl1, form.FindAll<BMComponentControl>().Single());

				section2.BackgroundColor = "Purple";
				Factory.Save();

				form.MoveNext();
				form.AwaitAll();

				AssertNotEquals("Should NOT cache component control since config has been changed", componentControl2, form.FindAll<BMComponentControl>().Single());
			}
		}

		#endregion

		#region AutoRefresh

		public void TestRefreshingResetsAutoRefreshTimer()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board, 1000), Tuple.Create(board, 1000));

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			Factory.Save();

			viewModel.RefreshSeconds = 5;

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				form.controlsPanel.Expand();
				var refreshWasStarted = 0;
				Application.DoEvents();
				form.RefreshStarted += (sender, args) =>
				{
					refreshWasStarted++;
				};

				form.AwaitAll();
				form.RefreshButton.PerformClick();
				form.AwaitAll();
				AssertEquals("Auto Refresh should have its timer reset and thus shouldn't be called for another 3000", 1, refreshWasStarted);
			}
		}

		#endregion

		#region Crossthreading exception

		public void TestShowBoard_ShouldNotReportThreadSentryError()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System);
			var section1 = BMSTestHelper.CreateBoardSection(config.Bucket, board);
			var section2 = BMSTestHelper.CreateBoardSection(config.Buffer, board);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
			}

			AssertNull(ErrorReporter.LastExceptionReported);
			AssertEquals(true, Env.Registry.ReportCrossThreadFactoryAccess);
		}

		public void TestCloseVisualBoard_WhileInitialisation()
		{
			var staff = new[]
			{
				BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frodo Baggins"),
				BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "SAM", "Samwise Gamgee"),
				BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BIL", "Bilbo Baggins"),
			};

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.AddRange(staff);

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			config.BufferSection.SectionConfiguration.ReleaseGroupPK = group.PK;
			config.BufferSection.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			for (var s = 0; s < staff.Length; ++s)
			{
				for (var j = 0; j < 10; ++j)
				{
					var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
					var workflow = BMSTestHelper.CreateWorkflow(jobHeader, string.Format("workflow{0}{1}", s, j), config.Buffer);
					var task1 = BMSTestHelper.CreateTask(workflow, staff[s].GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working);
					var task2 = BMSTestHelper.CreateTask(workflow, staff[s].GS_Code, 60);
				}
			}

			Factory.Save();
			var dispatcher = ApplicationDispatcher.Current;
			try
			{
				using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
				using (var form = VisualBoardFormDisplayer.ShowBoard(config.BufferBoard))
				{
					form.AwaitAll();
					ApplicationDispatcher.Current = null;
					AssertNotNull("Board should be exist", form);
				}
			}
			finally
			{
				AssertNull("LastExceptionReported", ErrorReporter.LastExceptionReported);
				ErrorReporter.Clear();
				ApplicationDispatcher.Current = dispatcher;
			}
		}

		#endregion

		#region Reload

		[TestDate(2015, 7, 14)]
		public void TestReloadBoardAndThenEditBoardConfiguration_ShouldReloadBoardAgain_WithoutReportingThreadSentryErrors()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BucketSection;

			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.BackgroundColor = Color.Blue.Name;

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();

				var startingControl = form.FindSingle<BMComponentControl>();

				BMSFormTestHelper.PressHotkeys(form, Keys.Shift | Keys.F5);
				form.AwaitAll();

				var reloadedControl = form.FindSingle<BMComponentControl>();

				AssertNotEquals("Reloading the board should have re-created the contents of the section", startingControl, reloadedControl);

				form.controlsPanel.Expand();
				AssertColorEquals(Color.Blue, form.FindSingle<TaskPanel>().BackColor);

				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

				form.controlsPanel.ConfigButton.PerformClick();
				form.AwaitAll();

				using (var boardEditForm = (ZForm)ZFormModaliser.LastFormShownForTest)
				{
					var editFormBoard = (BMBoard)boardEditForm.BusinessEntity;
					var editFormSection = editFormBoard.Sections.Single();

					editFormSection.BackgroundColor = Color.Red.Name;

					editFormBoard.RunPreSaveValidation();

					AssertNoErrors(editFormBoard);
					AssertEquals(ContinueWithSave.Yes, boardEditForm.FireSaveButton());
					Application.DoEvents();

					AssertColorEquals("Should not update board until the edit form has been closed", Color.Blue, form.FindSingle<TaskPanel>().BackColor);

					boardEditForm.Close();
				}

				form.AwaitAll();

				AssertColorEquals(Color.Red, form.FindSingle<TaskPanel>().BackColor);
				AssertNull("No thread sentry errors, of any kind.", ErrorReporter.LastExceptionReported);
			}
		}

		public void TestSlideshowTransition_WhenSectionHasNoFilter_ShouldNotThrowException()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board1 = BMSTestHelper.CreateBoard(config.System, "One");
			var board2 = BMSTestHelper.CreateBoard(config.System, "Two");
			var board3 = BMSTestHelper.CreateBoard(config.System, "Threeee!");
			var section1 = BMSTestHelper.CreateBoardSection(config.Bucket, board1);
			var section2 = BMSTestHelper.CreateBoardSection(config.Bucket, board2);
			var section3 = BMSTestHelper.CreateBoardSection(config.Bucket, board3);

			var colors = new[]
			{
				Color.Red,
				Color.Green,
				Color.Cyan,
			};

			section1.BackgroundColor = colors[0].Name;
			section2.BackgroundColor = colors[1].Name;
			section3.BackgroundColor = colors[2].Name;

			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section3, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2, board3);

			var filter1 = section1.WorkflowFilter;
			var filter2 = section2.WorkflowFilter;
			var filter3 = section3.WorkflowFilter;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Shift and hold", releaseGroupPK: config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			CombineAssertions("Section filters should not be saved since they're empty", () =>
			{
				AssertNotNull("filter1", newFactory.Load<StmModuleFilter>(filter1.PK));
				AssertNotNull("filter2", newFactory.Load<StmModuleFilter>(filter2.PK));
				AssertNotNull("filter3", newFactory.Load<StmModuleFilter>(filter3.PK));
			});

			AssertEquals(true, section1.WorkflowFilter.IsInDatabase);
			AssertEquals(true, section2.WorkflowFilter.IsInDatabase);
			AssertEquals(true, section3.WorkflowFilter.IsInDatabase);

			var strategy = new TriggerableAsyncStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			using (var form = VisualBoardFormDisplayer.ShowBoard(slideshow))
			{
				strategy.DoAllActions();
				for (var i = 0; i < colors.Length * 2; i++)
				{
					var color = colors[i % colors.Length];
					var control = form.FindAll<TaskPanel>().First();

					AssertEquals(color, control.BackColor);

					form.MoveNext();
					strategy.DoAllActions();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestReloadSlideshowViewModel_OnVisualBoardFormThread()
		{
			var board1 = Factory.NewWithValidTestData<BMBoard>();
			var board2 = Factory.NewWithValidTestData<BMBoard>();

			var boardSlideShow = Factory.NewWithValidTestData<BMBoardSlideshow>();
			boardSlideShow.MD_Name = "Board 2: Electric Boogaloo";

			var boardPivot1 = boardSlideShow.BoardPivots.AddNew();
			boardPivot1.MC_MB_Board = board1.PK;
			boardPivot1.MC_Sequence = 2;

			var boardPivot2 = boardSlideShow.BoardPivots.AddNew();
			boardPivot2.MC_MB_Board = board2.PK;
			boardPivot2.MC_Sequence = 1;

			Factory.Save();

			var reloadedSlideShow = new BusinessObjectFactory().Load<BMBoardSlideshow>(boardSlideShow.PK);
			AssertNotNull("Precondition", reloadedSlideShow);

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(reloadedSlideShow))
			{
				form.AwaitAll();
				ZFormModaliser.ShowDialogsInTest = true;

				form.controlsPanel.Expand();
				form.controlsPanel.ConfigButton.PerformClick();

				Application.DoEvents();
				var slideshowConfigForm = ZFormModaliser.LastFormShownForTest as BMBoardSlideshowForm;
				AssertNotNull("Precondition", slideshowConfigForm);

				slideshowConfigForm.Close();
			}

			var message = @"
				Given a VisualBoardForm on its own thread
				When we open its config (on the main thread) and close it to triger a board reload
				Then the board reload should happen on the ViaulBoardForm thread and not cause a cross thread exception";
			AssertEquals(message, 0, ErrorReporter.TotalErrorCount);
		}

		public void TestPopulateEstimatedLoadTimeCache_ShouldGetFromDBUsingNolock()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = VisualBoardsTestHelper.CreateBoard(config.System);
			board.MB_Name = "TestBoard_new";

			VisualBoardsTestHelper.CreateBoardSection(config.Bucket, board, row: 0);

			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			{
				BMSTestHelper.CreateBoardViewModel(board);

				var statisticsMeasurementsQueries = TestConnection.ExecutedCommands.Where(q => q.Contains("vw_StatisticsMeasurementsIncludingChildren"));
				Assert(statisticsMeasurementsQueries.All(query => query.Contains("FROM dbo.vw_StatisticsMeasurementsIncludingChildren WITH (NOLOCK)")));
			}
		}

		public void TestBoardEditAndSave_ShouldNotTouchOtherThreads_WhenFindingGetLatestCustomisationEditTimeUTC()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(system, "The will to live");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;

			var firstLayout = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.WorkflowSummaryCard, backgroundColor: "Tomato", name: "Ryan");
			var secondLayout = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.WorkflowDetailedCard, backgroundColor: "Tomato", name: "Letourneau");

			BMSTestHelper.CreateControlCustomisationLink(Factory, section, firstLayout);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, secondLayout);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				ZFormModaliser.ShowDialogsInTest = true;

				form.controlsPanel.Expand();
				form.controlsPanel.ConfigButton.PerformClick();

				Application.DoEvents();

				using (var boardEditForm = (ZForm)ZFormModaliser.LastFormShownForTest)
				{
					board.MB_Name = "butte";
					Factory.Save();

					AssertNoExceptionThrown("We expect no CrossThreadAccessExceptions to be thrown due to the board refresh, and yet...", () =>
					{
						form.RefreshBoardAndWait();
					});

					boardEditForm.Close();
				}
			}
		}

		#endregion

		#region Last Edit Time

		public void TestBoardIsReloadedWhenLastEditTimeChanges()
		{
			Factory.RefreshEnabled = false;
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var section = BMSTestHelper.CreateBoardSection(BMSTestHelper.CreateBucket(system));
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.Subsections = 1;

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();

				AssertEquals(1, form.FindAll<BMComponentControl>().First().ViewModel.ComponentGrid.TotalColumns);

				section.SectionConfiguration.Subsections = 10;

				Factory.Save();

				if (AsyncStrategy.Default is TaskTrackingAsyncBaseStrategy taskTracking)
				{
					taskTracking.AwaitAll(null, () => Application.DoEvents());
				}

				form.RefreshBoard();
				form.AwaitAll();

				Application.DoEvents();

				var componentControl = form.FindAll<BMComponentControl>().First();
				var cells = componentControl.ViewModel.ComponentGrid.Cells.Count(x => x.ContentType == CellContentType.Cards);

				AssertEquals("GIVEN SubSections=10 and CellsPerSubSection=1, THEN should be 10 cards", 10, cells);
			}
		}

		public void TestRefresh_ShouldUpgradeToReload_ByCustomisedTicketLayoutsLastEditTime()
		{
			Factory.RefreshEnabled = false;

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			var section = BMSTestHelper.CreateBoardSection(config.Bucket);
			var summaryCard = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var link1 = BMSTestHelper.CreateControlCustomisationLink(Factory, section, summaryCard);
			var link2 = BMSTestHelper.CreateControlCustomisationLink(Factory, section.Board, summaryCard);

			Factory.Save();

			var viewModel = VisualBoardFormTest.GetViewModel(section.Board);

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				var taskCard = form.FindAll<TaskCardControl>().Single();
				var width = taskCard.Width;

				var anotherFactory = new BusinessObjectFactory();
				var loadedSummaryCard = anotherFactory.Load<BMControlCustomisation>(summaryCard.PK);
				loadedSummaryCard.Width *= 3;
				anotherFactory.Save();

				form.RefreshBoardAndWait();
				taskCard = form.FindAll<TaskCardControl>().Single();
				AssertGreaterThan(taskCard.Width, width);
				width = taskCard.Width;

				link1.Delete();
				Factory.Save();
				form.RefreshBoard();

				loadedSummaryCard = anotherFactory.Load<BMControlCustomisation>(summaryCard.PK);
				loadedSummaryCard.Width *= 2;
				anotherFactory.Save();

				form.RefreshBoardAndWait();
				taskCard = form.FindAll<TaskCardControl>().Single();
				AssertGreaterThan(taskCard.Width, width);
			}
		}

		#endregion

		#region Bitmap Caching

		[ExpectNoExceptions]
		public void TestShowDetailedCardAndTakeTask_ShouldNotUseDisposedCachedBitmap()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var resource1 = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, "PLG", "Darth Plagueis the Wise", capability);
			var resource2 = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, "SID", "Darth Sidious", capability);
			var resource3 = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, "VAD", "Darth Vader", capability);

			config.ReleaseGroup.Staff.AddRange(resource1, resource2, resource3);

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", currentComponent: config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task1 = BMSTestHelper.CreateTask(workflow, string.Empty, 60, description: "task1", capability: capability);
			var task2 = BMSTestHelper.CreateTask(workflow, string.Empty, 60, description: "task2", capability: capability);
			var task3 = BMSTestHelper.CreateTask(workflow, string.Empty, 60, description: "task3", capability: capability);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			Factory.Save();

			using (Env.SetTemporaryUserContext(resource1.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.RefreshButton.PerformClick();

				form.AwaitAll();

				TakeTask(task1, resource2, form);
				TakeTask(task2, resource2, form);
				TakeTask(task3, resource3, form);

				BMSGUITestCase.PlayTask(task1, resource2, form);

				form.AwaitAll();
			}
		}

#if !WINZOR  // Bitmap rendering is not used for Winzor, so these tests do not apply.
		public void TestTaskCardBitmapCaching()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", config.Bucket);
			var task1 = BMSTestHelper.CreateTask(workflow1, config.CCR.GS_Code, 60, description: "task1");
			var task2 = BMSTestHelper.CreateTask(workflow2, config.CCR.GS_Code, 60, description: "task2");

			var board = BMSTestHelper.CreateBoard(config.System);
			var section1 = BMSTestHelper.CreateBoardSection(config.Bucket, board);
			var section2 = BMSTestHelper.CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup, board);
			section1.Column = 0;
			section2.Column = 1;
			section1.ColWidthPercent = 50;
			section2.ColWidthPercent = 50;

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				var section1Control = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == section1.PK);
				var section2Control = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == section2.PK);

				var section1TaskCards = section1Control.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, section1TaskCards.Length);
				var section2TaskCards = section2Control.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, section2TaskCards.Length);

				var task1_1BackgroundImage = GetForTask(task1, section1TaskCards).BackgroundImage;
				var task2_1BackgroundImage = GetForTask(task2, section1TaskCards).BackgroundImage;
				var task1_2BackgroundImage = GetForTask(task1, section2TaskCards).BackgroundImage;
				var task2_2BackgroundImage = GetForTask(task2, section2TaskCards).BackgroundImage;

				AssertNotNull("Should draw task card as a bitmap", task1_1BackgroundImage);
				AssertNotNull("Should draw task card as a bitmap", task2_1BackgroundImage);
				AssertNotNull("Should draw task card as a bitmap", task1_2BackgroundImage);
				AssertNotNull("Should draw task card as a bitmap", task2_2BackgroundImage);

				form.RefreshBoardAndWait();

				// Refreshing the board should regenerate controls, but use cached bitmaps
				var newSection1TaskCards = section1Control.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, newSection1TaskCards.Length);
				var newSection2TaskCards = section2Control.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, newSection2TaskCards.Length);

				AssertEquals("Should regenerate task cards", true, section1TaskCards.Contains(newSection1TaskCards[0]));
				AssertEquals("Should regenerate task cards", true, section1TaskCards.Contains(newSection1TaskCards[1]));
				AssertEquals("Should regenerate task cards", true, section2TaskCards.Contains(newSection2TaskCards[0]));
				AssertEquals("Should regenerate task cards", true, section2TaskCards.Contains(newSection2TaskCards[1]));

				AssertEquals("Should use the same background image as the task and workflow haven't changed", task1_1BackgroundImage, GetForTask(task1, newSection1TaskCards).BackgroundImage);
				AssertEquals("Should use the same background image as the task and workflow haven't changed", task2_1BackgroundImage, GetForTask(task2, newSection1TaskCards).BackgroundImage);
				AssertEquals("Should use the same background image as the task and workflow haven't changed", task1_2BackgroundImage, GetForTask(task1, newSection2TaskCards).BackgroundImage);
				AssertEquals("Should use the same background image as the task and workflow haven't changed", task2_2BackgroundImage, GetForTask(task2, newSection2TaskCards).BackgroundImage);

				// Cell tasks overlay should not use cached bitmaps as they are stretched out
				var taskPanel = section1Control.FindAll<TaskPanel>().First();

				((ZButton)taskPanel.Controls.Find("TotalTasksButton", true)[0]).PerformClick();

				var cellTasksControl = (CellTasksControl)form.CurrentlyShownCellTasksControl.Target;
				var cellTasksCards = cellTasksControl.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, cellTasksCards.Length);

				AssertNotEquals("Should not use cached bitmaps as the overlay control stretches them out", task1_1BackgroundImage, GetForTask(task1, cellTasksCards).BackgroundImage);
				AssertNotEquals("Should not use cached bitmaps as the overlay control stretches them out", task1_2BackgroundImage, GetForTask(task2, cellTasksCards).BackgroundImage);

				// Change the task and bitmap cache should be cleareds
				task1.P9_Description = "Look in your drawer...";
				Factory.Save();
				form.AwaitAll();

				form.RefreshBoardAndWait();

				newSection1TaskCards = section1Control.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, newSection1TaskCards.Length);
				newSection2TaskCards = section2Control.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, newSection2TaskCards.Length);

				var task1_1NewBackgroundImage = GetForTask(task1, newSection1TaskCards).BackgroundImage;
				var task2_1NewBackgroundImage = GetForTask(task2, newSection1TaskCards).BackgroundImage;
				var task1_2NewBackgroundImage = GetForTask(task1, newSection2TaskCards).BackgroundImage;
				var task2_2NewBackgroundImage = GetForTask(task2, newSection2TaskCards).BackgroundImage;

				AssertNotEquals("Should regenerate background image as the task has changed", task1_1BackgroundImage, task1_1NewBackgroundImage);
				AssertEquals("Should use the same background image as the task and workflow haven't changed", task2_1BackgroundImage, task2_1NewBackgroundImage);
				AssertNotEquals("Should regenerate background image as the task has changed", task1_2BackgroundImage, task1_2NewBackgroundImage);
				AssertEquals("Should use the same background image as the task and workflow haven't changed", task2_2BackgroundImage, task2_2NewBackgroundImage);
			}
		}

		public void TestTaskCardBitmapCaching_WhenTaggingJob()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			var section = BMSTestHelper.CreateBoardSection(config.Bucket);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				var taskCard = form.FindAll<TaskCardControl>().Single();
				var taskBackgroundImage = taskCard.BackgroundImage;

				form.RefreshBoardAndWait();

				var newTaskCard1 = form.FindAll<TaskCardControl>().Single();
				var jobTagLink = jobHeader.AddTag(config.RedTag).Link;
				Factory.Save();
				form.AwaitAll();

				form.RefreshBoardAndWait();

				var newTaskCard2 = form.FindAll<TaskCardControl>().Single();
				AssertNotEquals("Should regenerate background image as a tag was added", taskBackgroundImage, newTaskCard2.BackgroundImage);

				var workflowTagLink = workflow.AddTag(config.PlatinumTag).Link;
				Factory.Save();
				form.AwaitAll();

				form.RefreshBoardAndWait();

				var newTaskCard3 = form.FindAll<TaskCardControl>().Single();
				AssertNotEquals("Should regenerate background image as a tag was added", newTaskCard2.BackgroundImage, newTaskCard3.BackgroundImage);

				workflowTagLink.TGL_TGM_Magnitude = config.GoldTag.PK;
				Factory.Save();
				form.AwaitAll();

				form.RefreshBoardAndWait();

				var newTaskCard4 = form.FindAll<TaskCardControl>().Single();
				AssertNotEquals("Should regenerate background image as a tag was changed", newTaskCard3.BackgroundImage, newTaskCard4.BackgroundImage);

				var image = newTaskCard4.BackgroundImage;

				form.RefreshBoardAndWait();

				var newTaskCard5 = form.FindAll<TaskCardControl>().Single();
				AssertEquals("No changes, so cached bitmap should be used", image, newTaskCard5.BackgroundImage);

				workflowTagLink.Delete();
				Factory.Save();
				form.AwaitAll();

				form.RefreshBoardAndWait();

				var newTaskCard6 = form.FindAll<TaskCardControl>().Single();
				AssertNotEquals("Should regenerate background image as a tag was removed", newTaskCard5.BackgroundImage, newTaskCard6.BackgroundImage);

				jobTagLink.Delete();
				Factory.Save();
				form.AwaitAll();

				form.RefreshBoardAndWait();

				var newTaskCard7 = form.FindAll<TaskCardControl>().Single();
				AssertNotEquals("Should regenerate background image as a tag was removed", newTaskCard6.BackgroundImage, newTaskCard7.BackgroundImage);
			}
		}

		static TaskCardControl GetForTask(ProcessTask task, TaskCardControl[] controls)
		{
			return controls.Single(c => c.CardContent.TaskIdentifier == task.PK);
		}
#endif

		#endregion

#if !WINZOR
		#region Resources stress test

		[SnailTest]
		[StressTest]
		[ExpectNoExceptions]
		[TestDate(2013, 2, 22)]
		public void TestFormLoad_ShouldNotRunOutOfHandles()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var board = system.Boards.AddNew();
			var bucket1Section = board.Sections.AddNew();
			bucket1Section.MS_FC_Component = bucket1.PK;
			bucket1Section.SectionConfiguration.CellsPerSubsection = 13;
			bucket1Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			bucket1Section.Row = 0;
			bucket1Section.Column = 0;
			bucket1Section.RowHeightPercent = 50;
			bucket1Section.ColWidthPercent = 20;
			bucket1Section.SectionConfiguration.TimePerCell = new ZInt(60 * 8).GetDateTimeFromMinutes();

			var bucket2Section = board.Sections.AddNew();
			bucket2Section.MS_FC_Component = bucket2.PK;
			bucket2Section.SectionConfiguration.CellsPerSubsection = 13;
			bucket2Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			bucket2Section.Row = 1;
			bucket2Section.Column = 0;
			bucket2Section.RowHeightPercent = 50;
			bucket2Section.ColWidthPercent = 20;
			bucket2Section.SectionConfiguration.TimePerCell = new ZInt(60 * 8).GetDateTimeFromMinutes();

			var bufferSection = board.Sections.AddNew();
			bufferSection.MS_FC_Component = buffer.PK;
			bufferSection.SectionConfiguration.CellsPerSubsection = 13;
			bufferSection.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			bufferSection.SectionConfiguration.OverrideChannels = true;
			bufferSection.SectionConfiguration.ShowUnchanneled = true;
			bufferSection.SectionConfiguration.FadeBackgroundAtPercentage = 80;
			bufferSection.Row = 0;
			bufferSection.Column = 1;
			bufferSection.RowSpan = 2;
			bufferSection.RowHeightPercent = 100;
			bufferSection.ColWidthPercent = 80;

			bufferSection.SectionConfiguration.PrimaryAxisChannels[0].MSC_ChannelType = ChannelTypeList.Codes.Resource;

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "COD";
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frodo Baggins", capability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "SAM", "Samwise The Brave");

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.AddRange(resource1, resource2);

			BMSTestHelper.CreateReleaseGroup(system, group);
			bufferSection.SectionConfiguration.ReleaseGroupPK = group.PK;

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, resource2.PK);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var processJobHeader = ProcessJobHeader.GetForParent(org, Factory);
			for (int i = 0; i <= 17; i++)
			{
				BMSTestHelper.CreateProcessHeaderAndTask(processJobHeader, bucket1, i, taskLowEst: new ZInt(60).GetDateTimeFromMinutes(), completionStatement: $"a{i}"); // Rock tasks
				BMSTestHelper.CreateProcessHeaderAndTask(processJobHeader, bucket1, i, taskLowEst: new ZInt(0).GetDateTimeFromMinutes(), completionStatement: $"b{i}"); // Sand tasks

				BMSTestHelper.CreateProcessHeaderAndTask(processJobHeader, bucket2, i, completionStatement: $"c{i}"); // Platinum tasks
				BMSTestHelper.CreateProcessHeaderAndTask(processJobHeader, bucket2, i, completionStatement: $"d{i}"); // Gold tasks

				BMSTestHelper.CreateProcessHeaderAndTask(processJobHeader, buffer, i, resource1.GS_Code, completionStatement: $"e{i}");
				BMSTestHelper.CreateProcessHeaderAndTask(processJobHeader, buffer, i, resource2.GS_Code, completionStatement: $"f{i}");
				BMSTestHelper.CreateProcessHeaderAndTask(processJobHeader, buffer, i, "", capability.PK, releaseGroupPK: group.PK, completionStatement: $"g{i}");
			}

			var standaloneTask = Factory.New<ProcessTask>();
			standaloneTask.P9_Description = "standaloneTask";
			standaloneTask.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			standaloneTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(board))
			{
				form.AwaitAll();
				var startingGDICount = GetGuiResourcesGDICount();
				var startingUSERObjectCount = GetGuiResourcesUserCount();

				const int differenceTolerance = 200;

				for (int refreshes = 0; refreshes < 100; refreshes++)
				{
					form.RefreshBoardAndWait();
				}

				GC.Collect(); // Test case - go away
				GC.WaitForFullGCComplete();

				Application.DoEvents();

				var finalGDICount = (int)GetGuiResourcesGDICount();
				var finalUSERObjectCount = (int)GetGuiResourcesUserCount();

				CombineAssertions(() =>
				{
					Assert(string.Format("Should be a difference of no more than {0} GDI objects, but there were {1} after the first board refresh, and {2} after 100 refreshes.", differenceTolerance, startingGDICount, finalGDICount),
						finalGDICount - startingGDICount <= differenceTolerance);

					Assert(string.Format("Should be a difference of no more than {0} USER objects, but there were {1} after the first board refresh, and {2} after 100 refreshes.", differenceTolerance, startingUSERObjectCount, finalUSERObjectCount),
						finalUSERObjectCount - startingUSERObjectCount <= differenceTolerance);
				});
			}
		}

		static uint GetGuiResourcesGDICount()
		{
			return NativeMethods.GetGuiResources(Process.GetCurrentProcess().Handle, 0u);
		}

		static uint GetGuiResourcesUserCount()
		{
			return NativeMethods.GetGuiResources(Process.GetCurrentProcess().Handle, 1u);
		}

		#endregion
#endif

		#region Background Thread Cache Population

		[ExpectNoExceptions]
		public void TestPlayTask_ChannelCacheShouldPopulateOnBackgroundThread()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "XW@", "X-Wing @Alicioussness");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TSW", "Tyroil Smoochie-Wallace");
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.AddRange(resource1, resource2);

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			BMSTestHelper.CreateReleaseGroup(config.System, group);
			config.BufferSection.SectionConfiguration.ReleaseGroupPK = group.PK;
			config.BufferSection.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", config.Buffer);
			workflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var task1 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, 60);

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(config.BufferBoard))
			{
				form.AwaitAll();
				BMSGUITestCase.PlayTask(task1, resource1, form);
				form.AwaitAll();
			}
		}

		#endregion

		#region Channel Status

		[TestDate(2015, 7, 14)]
		public void TestStatusValueChanges()
		{
			var factory = new BusinessObjectFactory();
			var system = VisualBoardsTestHelper.CreateSystem(factory, "ORG");
			var buffer = VisualBoardsTestCase.CreateBuffer(system);
			var section = VisualBoardsTestCase.CreateBoardSection(buffer);
			section.SectionConfiguration.CellsPerSubsection = 4;

			var customisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			var buttonCustomisation = BMSTestHelper.CreateStaticControlCustomisation(customisation, StaticControlTypeList.Codes.StatusButtons, string.Empty, 120, 10, 0, 0, string.Empty, string.Empty, 8, false, false);
			buttonCustomisation.SuspendButtonBehavior = StatusButtonBehaviorOptionsList.Codes.Manual;
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisation);

			var resource = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(factory, "Jim", "Jimmy");
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);

			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(factory);
			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, "complete");
			var task = VisualBoardsTestCase.CreateTask(workflow, resource.GS_Code, 60);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			factory.Save();

			ChannelHeaderControl control;
			Image startingImage;

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();

				control = form.FindAll<ChannelHeaderControl>().Single();
				startingImage = control.StatusPictureBox.Image;
				var formFactory = form.BusinessEntityForPersistingForm.Factory;
				var loadedTask = formFactory.Load<ProcessTask>(task.PK);

				BMSGUITestCase.SuspendTask(loadedTask, loadedTask.AssignedStaffMember, form);

				form.AwaitAll();

				var newImage = control.StatusPictureBox.Image;
				AssertNotEquals(startingImage, newImage);
			}

			var newDisposedImage = control.StatusPictureBox.Image;
			AssertNotEquals(startingImage, newDisposedImage);
		}

		#endregion

		#region Performance

		public void TestVisualBoard_LoggingNestedPerformanceStatistics_SimpleMode()
		{
			var board = SetupLoggingNestedPerformanceStatisticsTest(mode: "Simple");
			AssertLoggingNestedPerformanceStatistics(board);

			var newFactory = new BusinessObjectFactory() { NameForDebugging = "SetupVisualBoard_LoggingNestedPerformanceStatistics", RefreshEnabled = true };
			var visualBoardThreadLogs = new List<ZArchitecture.Business.Statistics.Xml.IUsage>();
			visualBoardThreadLogs.AddRange(StatisticsTestHelper.GetUsageActions(newFactory, "RefreshBoardSection"));
			visualBoardThreadLogs.AddRange(StatisticsTestHelper.GetUsageActionsBySubString(newFactory, "GetRelevantDataSource"));
			var pipeLogs = StatisticsTestHelper.GetUsageActionsBySubString(newFactory, "BusinessObjectFactory.Load");

			AssertContainsExactElementsInAnyOrder("GIVEN performance stats is simple-mode, WHEN showing visual board, THEN should log visual board thread stats",
				GetExpectedVisualBoardThreadLogs(board.Sections[0].PK, board.Sections[1].PK),
				visualBoardThreadLogs.Select(s => $"Name: {s.Name}, SubName: {s.SubName}, Count: {s.ActionCount}"));

			AssertCollectionNotContains("GIVEN performance stats is simple-mode, WHEN showing visual board, THEN should not log pipe stats",
				expectedPipeLogs,
				pipeLogs.Select(s => s.Name));
		}

		string[] GetExpectedVisualBoardThreadLogs(ZGuid section1PK, ZGuid section2PK)
		{
			return new[] {
				// AsyncVisualBoardFormWrapper.Create
				$"Name: RefreshBoardSection, SubName: {section1PK}|Board-X|bucket, Count: 1",
				$"Name: RefreshBoardSection, SubName: {section2PK}|Board-X|buffer, Count: 1",

				// load
				$"Name: RefreshBoardSection, SubName: {section1PK}|Board-X|bucket, Count: 1",
				$"Name: RefreshBoardSection, SubName: {section2PK}|Board-X|buffer, Count: 1",

				// reload
				$"Name: RefreshBoardSection, SubName: {section1PK}|Board-X|bucket, Count: 1",
				$"Name: RefreshBoardSection, SubName: {section2PK}|Board-X|buffer, Count: 1"
			};
		}

		readonly string[] expectedPipeLogs =
		{
			"BusinessObjectFactory.Load(ProcessTask, ZQuery)",
			"BusinessObjectFactory.Load(ProcessHeader, ZQuery)",
			"BusinessObjectFactory.Load(ProcessHeaderLink, ZQuery)",
			"BusinessObjectFactory.Load(TagLink, ZQuery)"
		};

		public void AssertLoggingNestedPerformanceStatistics(BMBoard board)
		{
			CleanupUsages();

			var newFactory = new BusinessObjectFactory() { NameForDebugging = "SetupVisualBoard_LoggingNestedPerformanceStatisticsJKTODO1", RefreshEnabled = true };

			var stats = newFactory.Load<StmUsage>(new ZQuery());
			Assert("GIVEN no related performance-stats", stats.Length == 0);

			var loadedBoard = newFactory.Load<BMBoard>(board.PK);

			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (PerformanceStatisticsCollector.StartMonitoring("TestVisualBoard_LoggingNestedPerformanceStatistics"))
			using (var form = VisualBoardFormDisplayer.ShowBoard(loadedBoard))
			{
				form.AwaitAll();
				var taskCards = form.FindAll<TaskCardControl>().ToArray();
				Assert("taskcards should be shown", taskCards.Length > 0);

				form.RefreshBoardAndWait();
				form.ReloadBoard();
				form.AwaitAll();
			}
		}

		static void CleanupUsages()
		{
			using (PerformanceStatisticsCollector.StartMonitoring("initialize_collector_classes"))
			{
				Thread.Sleep(10);
			}
			PerformanceStatisticsCollector.AttemptFlush(true); // Flush everything that may not be flushed previously
			WaitForFlushToComplete();
			Db.Connection.ExecuteNonQuery("DELETE dbo.StmUsageQueue; DELETE dbo.StmUsage;");
			AssertNotNull(ZDateTime.Now); // Just to pre-cache RefTimeZoneCollection
		}
		static void WaitForFlushToComplete()
		{
			for (int i = 0; ((i < 1000) && (TestPerformanceStatisticsCollector.PendingSaves > 0)); i++)
			{
				Thread.Sleep(10);
			}
			AssertEquals(0, TestPerformanceStatisticsCollector.PendingSaves);
		}

		BMBoard SetupLoggingNestedPerformanceStatisticsTest(string mode)
		{
			SystemDataRegistry.Instance.StatisticsCollectionEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mode);
			AssertEquals($"GIVEN Performance-stats mode = {mode}", mode, ObjectFactory.Get<ISystemDataRegistry>().StatisticsCollectionEnabled);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System, name: "Board-X");
			var section1 = BMSTestHelper.CreateBoardSection(config.Bucket, board, row: 0);
			var section2 = BMSTestHelper.CreateBoardSection(config.Buffer, board, row: 1);
			section2.SectionConfiguration.CellsPerSubsection = 4;

			// To ensure Factory.Load monitoring doesn't return duration = 0 (hence not logged), we populate more data 
			for (var i = 1; i <= 10; i++)
			{
				var capability = Factory.NewWithValidTestData<GlbCapability>();
				var resource = VisualBoardsTestHelper.CreateStaffInCurrentBranchDept(Factory, $"R{i}", $"Resource{i}", capability);
				BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, resource.PK, overrideChannels: true);

				var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
				var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1", config.Buffer, releaseDateTime: ZDateTime.Now.AddDays(-i), releaseGroupPK: config.ReleaseGroup.PK);
				var task1 = VisualBoardsTestCase.CreateTask(workflow1, resource.GS_Code, 60, description: "task1");
				task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

				var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
				var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2", config.Buffer, releaseDateTime: ZDateTime.Now.AddDays(-i), releaseGroupPK: config.ReleaseGroup.PK);
				var task2 = VisualBoardsTestCase.CreateTask(workflow2, resource.GS_Code, 60, description: "task2");
				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

				var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
				var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow3", config.Buffer, releaseDateTime: ZDateTime.Now.AddDays(-i), releaseGroupPK: config.ReleaseGroup.PK);
				var task3 = VisualBoardsTestCase.CreateTask(workflow3, resource.GS_Code, 60, description: "task3");
				task3.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

				workflow1.GetOrCreateLinkToParent(workflow2);
				workflow1.GetOrCreateDependencyLink(workflow3);
			}

			Factory.Save();

			return board;
		}

		#endregion

		#region Session Switching

		public void TestSessionLoggedInCheckForRefresh_ShouldResetOnClickAndKeyDown()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(board);

			using (var form = new VisualBoardFormForRefreshTest(viewModel))
			{
				AssertEquals(true, form.IsWindowsSessionActive);

				form.SimulateSessionSwitch(SessionSwitchReason.SessionLock);
				AssertEquals(false, form.IsWindowsSessionActive);

				form.OnKeyDown_ForTest(new KeyEventArgs(Keys.A));
				AssertEquals("A key was pressed, so the session should be considered active just to be safe, even though it didn't receive that session switch message, and yet...", true, form.IsWindowsSessionActive);

				form.SimulateSessionSwitch(SessionSwitchReason.SessionLock);
				AssertEquals(false, form.IsWindowsSessionActive);

				form.OnMouseButtonClick_Exposed();
				AssertEquals("The mouse was clicked, so the session should be considered active just to be safe, even though it didn't receive that session switch message, and yet...", true, form.IsWindowsSessionActive);

				form.SimulateSessionSwitch(SessionSwitchReason.SessionLock);
				AssertEquals(false, form.IsWindowsSessionActive);

				form.OnActivated_Exposed();
				AssertEquals("The form was activated (e.g. brought to the front via Ctrl+Tab), so the session should be considered active just to be safe, even though it didn't receive that session switch message, and yet...", true, form.IsWindowsSessionActive);
			}
		}

		#endregion

		#region Logging

		public void TestVisualBoardActivityLogs_ShouldIncludeBoardName()
		{
			EnvProxy.Instance.Registry.UserEventTrackingEnterprise = true;

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var board = config.BucketBoard; // lazy creation, needed before Factory.Save()

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			staff.GS_ActivityTrackingStatus = ActivityTrackingStatus.Yes;
			staff.GS_IsActivityLogged = true;
			Factory.Save();

			var isActivityLoggerInitiallyEnabled = ZFormActivityLogger.Instance.IsEnabled;

			try
			{
				if (!isActivityLoggerInitiallyEnabled)
				{
					ZFormActivityLogger.Instance.EnableActivityLogger();
				}

				ZFormActivityLogger.Instance.StatLogs.Clear();

				using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				using (VisualBoardsTestCase.ApplyAsyncStrategy(new TaskTrackingAsyncBaseStrategy()))
				using (var form = VisualBoardFormDisplayer.ShowBoard(board))
				{
					form.AwaitAll();

					var log = ZFormActivityLogger.Instance.StatLogs.SingleOrDefault(x => x.ModuleName == ModuleIDs.VisualBoard.Name);
					AssertNotNull("There should be a log for the visual board. SAD!", log);
					AssertEquals("The board name should appear in the activity log. SAD!", "Bucket Board", log.FormCaption);
				}
			}
			finally
			{
				if (!isActivityLoggerInitiallyEnabled)
				{
					ZFormActivityLogger.Instance.DisableActivityLogger();
				}
			}
		}

		#endregion

		#region Implementation

		static void TakeTask(ProcessTask task, GlbStaff resource, VisualBoardForm form)
		{
			var taskCard = form.FindAll<TaskCardControl>().Single(t => t.CardContent.TaskIdentifier == task.PK && t.Cell.Channel.EntityPK == resource.PK);
			taskCard.ShowDetailedCard();

			var detailedCard = form.FindAll<TaskCardDetailControl>().Single();

			detailedCard.FindAll<CapabilityAssignmentButton>().Single().PerformClick();
			detailedCard.FindAll<SaveButton>().Single().PerformClick();

			Application.DoEvents();
		}

		protected override void SetUp()
		{
			BMSTestCaseWithFactory.SetupAndClearTables();

			base.SetUp();

			BMSTestHelper.DisableAcceptabilityBandResultCache();
		}

		#endregion
	}
}

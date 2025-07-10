using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class AcceptabilityBandTileControlShowMatchingItemsTest : BMSTestCaseWithFactory
	{
		public void TestShowMatchingItems_ForNUPRule()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			BMSTestHelper.CreateReleaseGroup(config.System, group2);

			var goldenRule = BMSGUITestHelper.CreateAcceptabilityBandForTag(config.Buffer, 0, 0, 0, 0, 0, 0, "Nope", config.PlatinumTag, type: AcceptabilityBandTypes.Codes.NumberAsPercentage);
			goldenRule.BAB_FiltersByReleaseGroup = true;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", currentComponent: config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", currentComponent: config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3", currentComponent: config.Buffer, releaseGroupPK: group2.PK);

			workflow1.AddTag(config.PlatinumTag);
			workflow3.AddTag(config.PlatinumTag);

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 120);
			var task3 = BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 180);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;

			BMSTestHelper.AddAcceptabilityBandToSection(section, goldenRule);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();

				var control = form.FindAll<BMComponentControl>().Single();
				var filterManager = control.ViewModel.FilterManager;
				var tile = form.FindAll<AcceptabilityBandTileControl>().Single();

				AssertEquals("Nope: 50", tile.NameAndResultLabel.Text);

				AssertEquals(0, filterManager.AppliedFilters.Count());
				BMSGUITestCase.AssertTasksShown("No filter applied so all tasks should be visible", control, task1, task2, task3);

				AssertEquals(AcceptabilityBandTileControl.ShowMatchingItemsLabel, BMSGUITestCase.GetHighlightLabelMenuItem(tile).Text);

				BMSGUITestCase.ToggleShowMatchingItems(tile);

				AssertType<AcceptabilityBandMatchesVisibilityFilter>(filterManager.AppliedFilters.Single());
				BMSGUITestCase.AssertTasksShown("Only platinum items in the section release group should be shown", control, task1);
				BMSGUITestCase.AssertTasksNotShown("Platinum items outside the release group should be hidden", control, task2, task3);
			}
		}

		public void TestShowMatchingItems_ForDUPRule()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			BMSTestHelper.CreateReleaseGroup(config.System, group2);

			var goldenRule = BMSGUITestHelper.CreateAcceptabilityBandForTag(config.Buffer, 0, 0, 0, 0, 0, 0, "Nope", config.PlatinumTag, type: AcceptabilityBandTypes.Codes.PlannedDurationPercentage);
			goldenRule.BAB_FiltersByReleaseGroup = true;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", currentComponent: config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", currentComponent: config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3", currentComponent: config.Buffer, releaseGroupPK: group2.PK);

			workflow1.AddTag(config.PlatinumTag);
			workflow3.AddTag(config.PlatinumTag);

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 120);
			var task3 = BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 180);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;

			BMSTestHelper.AddAcceptabilityBandToSection(section, goldenRule);

			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();

				var control = form.FindAll<BMComponentControl>().Single();
				var filterManager = control.ViewModel.FilterManager;
				var tile = form.FindAll<AcceptabilityBandTileControl>().Single();
				AssertEquals("Nope: 33.33", tile.NameAndResultLabel.Text);

				AssertEquals(0, filterManager.AppliedFilters.Count());
				BMSGUITestCase.AssertTasksShown("No filter applied so all tasks should be visible", control, task1, task2, task3);

				AssertEquals(AcceptabilityBandTileControl.ShowMatchingItemsLabel, BMSGUITestCase.GetHighlightLabelMenuItem(tile).Text);

				BMSGUITestCase.ToggleShowMatchingItems(tile);

				AssertType<AcceptabilityBandMatchesVisibilityFilter>(filterManager.AppliedFilters.Single());
				BMSGUITestCase.AssertTasksShown("Only platinum items in the section release group should be shown", control, task1);
				BMSGUITestCase.AssertTasksNotShown("Platinum items outside the release group should be hidden", control, task2, task3);
			}
		}

		public void TestShowMatchingItems_ShouldIncludeSectionWorkflowsOnSqlRegardlessOfSectionFilteringSettings()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			BMSTestHelper.CreateReleaseGroup(config.System, group2);

			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "Nope", string.Empty, type: AcceptabilityBandTypes.Codes.PlannedDurationPercentage);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			BMSTestHelper.AddAcceptabilityBandToSection(section, band);

			void ShowFormAndClickBandAndAssertFiltering()
			{
				Factory.Save();

				using (DisableAsyncBehaviour())
				using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
				{
					form.Show();
					Application.DoEvents();
					var tile = form.FindAll<AcceptabilityBandTileControl>().Single();

					using (CargoWise.Data.Db.Connection.TrackExecutedCommands())
					{
						BMSGUITestCase.ToggleShowMatchingItems(tile);
						AssertContains("FH_PK IN (SELECT FH_PK FROM SectionWorkflows", string.Join(System.Environment.NewLine, CargoWise.Data.Db.Connection.ExecutedCommands));
					}
				}
			}

			band.BAB_FiltersBySection = true;
			ShowFormAndClickBandAndAssertFiltering();

			band.BAB_FiltersBySection = false;
			ShowFormAndClickBandAndAssertFiltering();
		}

		public void TestShowMatchingItems_WhenActive_OnRefresh_ShouldKeepToggledVisualAppearance()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "INQ");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "A", config.Buffer);
			BMSTestHelper.CreateWorkflowAndTask(jobHeader, "B", config.Buffer);

			var bandA = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "A", type: AcceptabilityBandTypes.Codes.Count);
			var bandB = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "B", type: AcceptabilityBandTypes.Codes.Count);
			bandA.BAB_FiltersByReleaseGroup = false;
			bandB.BAB_FiltersByReleaseGroup = false;

			FilterStripsTestHelper.AddStartsWithFilter(bandA.FilterRule, "Completion Statement", "A");
			FilterStripsTestHelper.AddStartsWithFilter(bandB.FilterRule, "Completion Statement", "B");

			BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, bandA);
			BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, bandB);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(config.BufferSection))
			{
				Application.DoEvents();

				void GetBandControls(out AcceptabilityBandTileControl tileA, out AcceptabilityBandTileControl tileB)
				{
					var bandTiles = form.FindAll<AcceptabilityBandTileControl>().ToArray();
					tileA = bandTiles.Single(x => x.NameAndResultLabel.Text == "A: 1");
					tileB = bandTiles.Single(x => x.NameAndResultLabel.Text == "B: 1");
				}

				GetBandControls(out var bandTileA, out var bandTileB);

				BMSGUITestCase.ToggleShowMatchingItems(bandTileA);
				AssertEquals(true, bandTileA.IsShowingMatchingItems);
				AssertEquals(false, bandTileB.IsShowingMatchingItems);

				form.RefreshBoard();
				Application.DoEvents();

				GetBandControls(out bandTileA, out bandTileB);
				AssertEquals("The highlighted tile should have stayed highlighted after refreshing. SAD!", true, bandTileA.IsShowingMatchingItems);
				AssertEquals(false, bandTileB.IsShowingMatchingItems);

				BMSGUITestCase.ToggleShowMatchingItems(bandTileA);
				AssertEquals(false, bandTileA.IsShowingMatchingItems);
				AssertEquals(false, bandTileB.IsShowingMatchingItems);

				form.RefreshBoard();
				Application.DoEvents();

				GetBandControls(out bandTileA, out bandTileB);
				AssertEquals(false, bandTileA.IsShowingMatchingItems);
				AssertEquals(false, bandTileB.IsShowingMatchingItems);

				BMSGUITestCase.ToggleShowMatchingItems(bandTileB);
				AssertEquals(false, bandTileA.IsShowingMatchingItems);
				AssertEquals(true, bandTileB.IsShowingMatchingItems);

				form.RefreshBoard();
				Application.DoEvents();

				GetBandControls(out bandTileA, out bandTileB);
				AssertEquals(false, bandTileA.IsShowingMatchingItems);
				AssertEquals(true, bandTileB.IsShowingMatchingItems);

				BMSGUITestCase.ToggleShowMatchingItems(bandTileA);
				Application.DoEvents();
				AssertEquals(true, bandTileA.IsShowingMatchingItems);

				form.RefreshBoard();
				Application.DoEvents();

				GetBandControls(out bandTileA, out bandTileB);
				AssertEquals(true, bandTileA.IsShowingMatchingItems);
				AssertEquals(false, bandTileB.IsShowingMatchingItems);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.DisableAcceptabilityBandResultCache();
		}
	}

	class AcceptabilityBandTileControlTest : BMSTestCaseWithFactory
	{
		#region Dispose

		public void TestDispose_ShouldNotHoldOntoContextMenu()
		{
			ContextMenuStrip contextMenu;

			using (var control = GetControl())
			{
				contextMenu = control.ContextMenuStrip;

				AssertEquals(false, contextMenu.IsDisposed);
			}

			AssertEquals(true, contextMenu.IsDisposed);
		}

		#endregion

		#region Show Calculation Details

		[TestDate(2019, 8, 4)]
		public void TestShowCalculationDetails()
		{
			using (var control = GetControl())
			{
				FindAndClickCalculationDetailsMenuItem(control);

				AssertEquals("Number of Workflows", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertStartsWith("should start with", @"Status: Good

Lowest 'Caution' value: 0
Lowest 'Good' value: 1
Lowest 'Excellent' value: 2
Highest 'Excellent' value: 3
Highest 'Good' value: 4
Highest 'Caution' value: 5

Calculated in", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEndsWith("should end with", @"seconds
Accurate as of 04-Aug-19 10:00:00
", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowCalculationDetails_CalculationDurationAndActualTime()
		{
			AssertCalculationDetailsHaveCertainText(TimeSpan.FromSeconds(2.5), new DateTime(2017, 7, 5, 0, 0, 0), "Calculated in 2.5 seconds", "Accurate as of 05-Jul-17 10:00:00");
			AssertCalculationDetailsHaveCertainText(TimeSpan.FromSeconds(1.0), new DateTime(2017, 7, 5, 2, 0, 0), "Calculated in 1 seconds", "Accurate as of 05-Jul-17 12:00:00");
		}

		void AssertCalculationDetailsHaveCertainText(TimeSpan calculationDuration, DateTime actualTimeUtc, string expectedCalculationText, string expectedActualTimeText) // PerformClick on new menu item and assert the whole text of the message shown, instead of this nonsense.
		{
			using (var control = GetControl())
			{
				control.ViewModel.SetCalculationTimeAndDuration_ForTesting(calculationDuration, actualTimeUtc);

				FindAndClickCalculationDetailsMenuItem(control);

				var messageText = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertContains("Calculation info", expectedCalculationText, messageText);
				AssertContains("Actual time", expectedActualTimeText, messageText);
			}
		}

		[TestDate(2017, 11, 08, 14, 00, 00)]
		public void TestShowCalculationDetails_ShouldUpdateCalculatedTimeWithSecondsPrecision()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "INQ");
			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "Mand");
			var sectionViewModel = BMSTestHelper.CreateViewModel(config.BufferSection);
			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band);
			var result = new AcceptabilityBandResult(true, 10, "ButtFore");
			result.SetAccurateAsOfTimeUtc_ForTest(ZDateTime.Now);
			var viewModel = new AcceptabilityBandTileViewModel(sectionBand, sectionViewModel, result);
			string firstResultTime = null;

			using (var control = new AcceptabilityBandTileControl(viewModel))
			{
				FindAndClickCalculationDetailsMenuItem(control);

				firstResultTime = ZDateTime.Now.ToString();
				AssertContains(firstResultTime, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(10);
			// this simulates finding a new result within a couple of seconds.
			result = new AcceptabilityBandResult(true, 10, "ButtFore");
			result.SetAccurateAsOfTimeUtc_ForTest(ZDateTime.Now);
			viewModel = new AcceptabilityBandTileViewModel(sectionBand, sectionViewModel, result);

			using (var control = new AcceptabilityBandTileControl(viewModel))
			{
				FindAndClickCalculationDetailsMenuItem(control);

				var secondResultTime = ZDateTime.Now.ToString();
				AssertContains(secondResultTime, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotEquals(firstResultTime, secondResultTime);
			}
		}

		[TestDate(2024, 01, 31, 15, 10, 15)]
		public void TestShowCalculationDetails_ForBandStillLoading_ShouldNotShowTimes()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "INQ");
			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "Mand");
			var sectionViewModel = BMSTestHelper.CreateViewModel(config.BufferSection);
			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band);
			var result = BoardSectionAcceptabilityBandResult.Empty(sectionBand);
			var viewModel = new AcceptabilityBandTileViewModel(sectionBand, sectionViewModel, result.Result);

			using (var control = new AcceptabilityBandTileControl(viewModel))
			{
				FindAndClickCalculationDetailsMenuItem(control);
				var text = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertContains("Calculating", text);
				AssertNotContains("Calculated in", text);
				AssertNotContains("Accurate as of", text);
			}
		}

		[TestDate(2024, 01, 31, 15, 10, 15)]
		public void TestShowCalculationDetails_WhenTimeout()
		{
			Factory.SuspendValidation();
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "INQ");
			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "Mand");
			var sectionViewModel = BMSTestHelper.CreateViewModel(config.BufferSection);
			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band);
			var result = BoardSectionAcceptabilityBandResult.Empty(sectionBand);
			result.Result.Status = ComponentAcceptabilityStatus.Timeout;
			var viewModel = new AcceptabilityBandTileViewModel(sectionBand, sectionViewModel, result.Result);
			viewModel.SetCalculationTimeAndDuration_ForTesting(TimeSpan.FromSeconds(75), ZDateTime.Now);

			using (var control = new AcceptabilityBandTileControl(viewModel))
			{
				FindAndClickCalculationDetailsMenuItem(control);
				var text = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertContains("Calculation info", "Calculation timeout in 75 seconds", text);
				AssertContains("Actual time", "Accurate as of 31-Jan-24 15:10:15", text);
			}
		}

		static void FindAndClickCalculationDetailsMenuItem(AcceptabilityBandTileControl control)
		{
			var menuItem = control.ContextMenuStrip.Items.Cast<ZToolStripMenuItem>().Single(i => i.Text == AcceptabilityBandTileControl.ShowStatusCalculationDetailsLabel);
			AssertEquals(AcceptabilityBandTileControl.ShowStatusCalculationDetailsLabel, menuItem.Text);

			menuItem.PerformClick();
			Application.DoEvents();
		}

		#endregion

		#region Update Details

		public void TestUpdateDetails()
		{
			using (var control = GetControl())
			{
				AssertEquals("Number of Workflows: 1", control.NameAndResultLabel.Text);
				AssertEquals(BMConstants.GoodBoardColor, control.BackColor);
				AssertEquals(Color.White, control.ForeColor);
			}
		}

		public void TestUpdateDetails_FilterByReleaseGroup()
		{
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			BMSTestHelper.CreateReleaseGroup(config.System, group2);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2", releaseGroupPK: group2.PK);

			using (var control = GetControl())
			{
				AssertEquals("Number of Workflows: 1", control.NameAndResultLabel.Text);
			}

			workflow2.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			Factory.Save();

			using (var control = GetControl())
			{
				AssertEquals("Number of Workflows: 2", control.NameAndResultLabel.Text);
			}
		}

		public void TestUpdateDetails_WhenNoDisplayNameSpecified()
		{
			sectionBand.DisplayName = ZString.Empty;

			using (var control = GetControl())
			{
				AssertEquals("1", control.NameAndResultLabel.Text);
			}
		}

		#endregion

		#region Show All Matches

		public void TestShowAllMatches_WithAlwaysAppliedFilter_ShouldShowCorrectResults()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "DUM");
			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Inactive workflow");
			workflow1.FH_IsActive = false;
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Also Inactive workflow");
			workflow2.FH_IsActive = false;

			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "Inactive", type: AcceptabilityBandTypes.Codes.Count);
			FilterStripsTestHelper.AddFilterStrips(band.FilterRule,
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement, FilterStripValueSetter = filter => ((ModuleTextFilter)filter).Property = "I" },
				new FilterStripsTestHelper.FilterStripDefinition { FilterStripName = "Active Status", FilterStripValueSetter = filter => ((ModuleTextFilter)filter).Property = "Inactive" });
			band.BAB_FiltersBySection = false;

			BMSTestHelper.AddAcceptabilityBandToSection(config.BucketSection, band);

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(config.BucketBoard))
			{
				var tile = form.FindSingle<AcceptabilityBandTileControl>();
				AssertEquals("Inactive: 1", tile.NameAndResultLabel.Text);

				ZString[] listedWorkflows = null;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(dialog =>
				{
					if (dialog is EmbeddedModulePopup popup)
					{
						listedWorkflows = popup.Module_ForTest.GridCollection.Cast<ProcessHeader>().Select(x => x.FH_CompletionStatement).ToArray();
					}
				});

				var menuItem = tile.ContextMenuStrip.Items.Cast<ZToolStripMenuItem>().Single(i => i.Text == AcceptabilityBandTileControl.ShowAllWorkflowsLabel);
				menuItem.PerformClick();
				Application.DoEvents();

				AssertContainsExactElementsInAnyOrder("The workflow that matches both filter strips should have been found, despite the fact that one of the filters is an 'AlwaysVisible' one.",
					new[] { "Inactive workflow" }, listedWorkflows);
			}
		}

		public void TestShowAllMatches_WithCustomFilter_ShouldShowAllWorkflows()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "DUM");
			config.BufferSection.MS_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			config.ReleaseGroup.Staff.Add(staff);
			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "AAAA", currentComponent: config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow1, staff.GS_Code);
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "BBBB", currentComponent: config.Buffer);
			var task2 = BMSTestHelper.CreateTask(workflow2, staff.GS_Code);

			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "workflows", type: AcceptabilityBandTypes.Codes.Count);
			band.BAB_FiltersBySection = true;

			var abSecttion = BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band);
			abSecttion.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.YesOptionCode;

			Factory.Save();

			var customFilter = BMSGUITestHelper.CreateTemporaryBMFilterRuleModuleFilter(Factory, "CustomFilter");
			FilterStripsTestHelper.AddStartsWithFilter(customFilter, ProcessHeader.ModuleFilterConstants.CompletionStatement, "AAAA");

			using (DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(config.BufferSection))
			{
				var tile = form.FindSingle<AcceptabilityBandTileControl>();
				AssertEquals("workflows: 2", tile.NameAndResultLabel.Text);

				var control = form.FindAll<BMComponentControl>().Single();
				var controlViewModel = control.ViewModel;
				control.SetCustomWorkflowFilterForTestingAndRefresh(customFilter);

				ZString[] listedWorkflows = null;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(dialog =>
				{
					if (dialog is EmbeddedModulePopup popup)
					{
						listedWorkflows = popup.Module_ForTest.GridCollection.Cast<ProcessHeader>().Select(x => x.FH_CompletionStatement).ToArray();
					}
				});

				tile = form.FindSingle<AcceptabilityBandTileControl>();
				var showAllMenuItem = tile.ContextMenuStrip.Items.Cast<ZToolStripMenuItem>().Single(i => i.Text == AcceptabilityBandTileControl.ShowAllWorkflowsLabel);

				showAllMenuItem.PerformClick();
				Application.DoEvents();

				AssertContainsExactElementsInAnyOrder("Show all workflow that matches the AB and in the board without the section custom filter",
				new[] { "AAAA", "BBBB" }, listedWorkflows);
			}
		}

		public void TestShowAllWorkflows_ShouldForcePublishUnpublishedFilter()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "DUM");
			config.BufferSection.MS_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			config.ReleaseGroup.Staff.Add(staff);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Test Workflow", currentComponent: config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow, staff.GS_Code);
			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "Unpublished AB", type: AcceptabilityBandTypes.Codes.Count);
			BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band);

			Factory.Save();

			var customFilter = BMSGUITestHelper.CreateTemporaryBMFilterRuleModuleFilter(Factory, "CustomFilter");
			customFilter.S9_IsPublished = false;
			FilterStripsTestHelper.AddStartsWithFilter(customFilter, ProcessHeader.ModuleFilterConstants.CompletionStatement, "Test Workflow");

			using (DisableAsyncBehaviour())
			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(config.BufferSection))
			{
				var tile = form.FindSingle<AcceptabilityBandTileControl>();
				AssertEquals("Unpublished AB: 1", tile.NameAndResultLabel.Text);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(dialog =>
				{
					if (dialog is EmbeddedModulePopup popup)
					{
						AssertEquals("Should be forced published before shown", true, band.FilterRule.S9_IsPublished);
					}
				});

				var menuItem = tile.ContextMenuStrip.Items.Cast<ZToolStripMenuItem>().Single(i => i.Text == AcceptabilityBandTileControl.ShowAllWorkflowsLabel);
				menuItem.PerformClick();
				Application.DoEvents();
			}
		}

		#endregion

		#region Implementation

		SchematicTestConfig config;
		BMComponentAcceptabilityBand band;
		BoardSectionAcceptabilityBand sectionBand;
		BMBoardSectionViewModel sectionViewModel;

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.DisableAcceptabilityBandResultCache();

			config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var section = CreateBoardSection(config.Bucket);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;

			Factory.SuspendValidation();
			band = CreateAcceptabilityBand_WorkflowsInComponent(config.Bucket, 0, 1, 2, 3, 4, 5);
			sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(section, band);
			sectionBand.DisplayName = "Number of Workflows";
			Factory.ResumeValidation();

			sectionViewModel = BMSTestHelper.CreateViewModel(section);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", releaseGroupPK: config.ReleaseGroup.PK);

			Factory.Save();
		}

		AcceptabilityBandTileControl GetControl()
		{
			var results = sectionViewModel.GetAcceptabilityBandResults(Factory);
			return new AcceptabilityBandTileControl(new AcceptabilityBandTileViewModel(sectionBand, sectionViewModel, results.Single(x => x.SectionBand.AcceptabilityBandPK == band.PK).Result));
		}

		#endregion
	}

	class AcceptabilityBandTileControlNonTransactionedTest : NonTransactionedTestCase
	{
		#region Secondary Server

		public void TestSecondaryServerConnectionIsUsed()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var sqlAcceptabilityBand = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "SQL Band", type: AcceptabilityBandTypes.Codes.SQL);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", currentComponent: config.Buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", currentComponent: config.Buffer);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3", currentComponent: config.Buffer);

			workflow1.AddTag(config.PlatinumTag);
			workflow2.AddTag(config.RedTag);
			workflow3.AddTag(config.GoldTag);

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var task3 = BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var sectionBand1 = BMSTestHelper.AddAcceptabilityBandToSection(section, config.PlatinumRule);
			var sectionBand2 = BMSTestHelper.AddAcceptabilityBandToSection(section, config.RedRule);
			var sectionBand3 = BMSTestHelper.AddAcceptabilityBandToSection(section, sqlAcceptabilityBand);

			Factory.Save();

			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				var control = form.FindAll<BMComponentControl>().Single();
				var filterManager = control.ViewModel.FilterManager;
				var tiles = form.FindAll<AcceptabilityBandTileControl>().ToArray();
				AssertEquals(3, tiles.Length);

				var tile1 = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == config.PlatinumRule.PK);
				var tile2 = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == config.RedRule.PK);
				var tile3 = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == sqlAcceptabilityBand.PK);
			}
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}

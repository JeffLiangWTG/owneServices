using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.BufferManagement.GUI.Test
{
	class AcceptabilityBandTileControlShowAllMatchedTest : BMSGUITestCase
	{
		#region Filter by Board Section

		public void TestShowAllWorkflowsMatchingCriteria_NullReferenceExceptionHappensProbablyBecauseConnectionTimedOut_ShouldAlertUserOfErrorAndNotThrowException()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			config.PlatinumRule.BAB_FC_Component = ZGuid.Empty;
			config.PlatinumRule.BAB_FiltersByReleaseGroup = false;
			config.PlatinumRule.BAB_FiltersBySection = false;

			SetUpObjectsForSectionFilteringTests(config);

			var sectionBand = config.BucketSection.SectionConfiguration.AcceptabilityBands.Cast<BoardSectionAcceptabilityBand>().Single(x => x.AcceptabilityBand == config.PlatinumRule);
			sectionBand.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.YesOptionCode;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BucketBoard))
			{
				var tiles = form.FindAll<AcceptabilityBandTileControl>().ToArray();
				var tile = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == config.PlatinumRule.PK);

				BusinessObject[] result = null;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(popup =>
				{
					if (popup is EmbeddedModulePopup modulePopup)
					{
						result = modulePopup.Module_ForTest.GridCollection.ToArray();
					}
				});

				tile.OnShowAllMatches += (s, e) =>
				{
					throw new NullReferenceException();
				};

				TriggerShowAllMatches(tile);
				AssertEquals("Unable to display results since an error has occurred. If the problem persists, talk to your system administrator. Consider simplifying filters defined in Acceptability Band configuration.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowAllWorkflowsMatchingCriteria_FilterBySection_Disabled()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			config.PlatinumRule.BAB_FC_Component = ZGuid.Empty;
			config.PlatinumRule.BAB_FiltersByReleaseGroup = false;
			config.PlatinumRule.BAB_FiltersBySection = false;

			SetUpObjectsForSectionFilteringTests(config);

			AssertShowAllMatchesCorrectBehaviour("Not filtering by board section should include all matching workflows regardless of section. SAD!",
				config, new[] { "Workflow in Bucket 1 - Platinum", "Workflow in Bucket 2 - Platinum" });
		}

		public void TestShowAllWorkflowsMatchingCriteria_FilterBySection()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			config.PlatinumRule.BAB_FC_Component = ZGuid.Empty;
			config.PlatinumRule.BAB_FiltersByReleaseGroup = false;
			config.PlatinumRule.BAB_FiltersBySection = true;

			SetUpObjectsForSectionFilteringTests(config);

			AssertShowAllMatchesCorrectBehaviour("Filtering by board section should exclude workflows that match the filters but are not shown on the section. SAD!",
				config, new[] { "Workflow in Bucket 1 - Platinum" });
		}

		public void TestShowAllWorkflowsMatchingCriteria_FilterBySection_OverriddenToTrue()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			config.PlatinumRule.BAB_FC_Component = ZGuid.Empty;
			config.PlatinumRule.BAB_FiltersByReleaseGroup = false;
			config.PlatinumRule.BAB_FiltersBySection = false;

			SetUpObjectsForSectionFilteringTests(config);

			var sectionBand = config.BucketSection.SectionConfiguration.AcceptabilityBands.Cast<BoardSectionAcceptabilityBand>().Single(x => x.AcceptabilityBand == config.PlatinumRule);
			sectionBand.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.YesOptionCode;

			Factory.Save();

			AssertShowAllMatchesCorrectBehaviour("Filtering by board section is overridden to Yes, so it should exclude workflows that match the filters but are not shown on the section. SAD!",
				config, new[] { "Workflow in Bucket 1 - Platinum" });
		}

		public void TestShowAllWorkflowsMatchingCriteria_FilterBySection_OverriddenToFalse()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			config.PlatinumRule.BAB_FC_Component = ZGuid.Empty;
			config.PlatinumRule.BAB_FiltersByReleaseGroup = false;
			config.PlatinumRule.BAB_FiltersBySection = true;

			SetUpObjectsForSectionFilteringTests(config);

			var sectionBand = config.BucketSection.SectionConfiguration.AcceptabilityBands.Cast<BoardSectionAcceptabilityBand>().Single(x => x.AcceptabilityBand == config.PlatinumRule);
			sectionBand.FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.NoOptionCode;

			Factory.Save();

			AssertShowAllMatchesCorrectBehaviour("Filtering by board section is overridden to No, so it should include workflows that match the filters from any section. SAD!",
				config, new[] { "Workflow in Bucket 1 - Platinum", "Workflow in Bucket 2 - Platinum" });
		}

		void SetUpObjectsForSectionFilteringTests(AcceptabilityBandTestConfig config)
		{
			var sqlAcceptabilityBand = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "SQL Band", type: AcceptabilityBandTypes.Codes.SQL);

			var bucket1 = config.Bucket;
			var bucket2 = BMSTestHelper.CreateBucket(config.System, "Bucket 2");

			var section1 = config.BucketSection;
			var section2 = BMSTestHelper.CreateBoardSection(bucket2, config.BucketBoard, row: 1);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow in Bucket 1 - Platinum", currentComponent: bucket1);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow in Bucket 2 - Platinum", currentComponent: bucket2);
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow in Bucket 1 - Gold", currentComponent: bucket1);
			var workflow4 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow in Bucket 2 - Gold", currentComponent: bucket2);

			workflow1.AddTag(config.PlatinumTag);
			workflow2.AddTag(config.PlatinumTag);
			workflow3.AddTag(config.GoldTag);
			workflow4.AddTag(config.GoldTag);

			BMSTestHelper.AddAcceptabilityBandToSection(section1, config.PlatinumRule);
			BMSTestHelper.AddAcceptabilityBandToSection(section1, config.RedRule);
			BMSTestHelper.AddAcceptabilityBandToSection(section1, sqlAcceptabilityBand);

			Factory.Save();
		}

		#endregion

		#region Filter by Release Group

		public void TestShowAllWorkflowsMatchingCriteria_FilterByReleaseGroup_Disabled()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			config.PlatinumRule.BAB_FC_Component = ZGuid.Empty;
			config.PlatinumRule.BAB_FiltersByReleaseGroup = false;
			config.PlatinumRule.BAB_FiltersBySection = false;

			SetUpObjectsForReleaseGroupFilteringTests(config);

			AssertShowAllMatchesCorrectBehaviour("Filter by Release Group is off so the results should include all workflows that match the filters. SAD!",
				config, new[] { "Matching Workflow in Release Group", "Matching Workflow not in Release Group" });
		}

		public void TestShowAllWorkflowsMatchingCriteria_FilterByReleaseGroup()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			config.PlatinumRule.BAB_FC_Component = ZGuid.Empty;
			config.PlatinumRule.BAB_FiltersByReleaseGroup = true;
			config.PlatinumRule.BAB_FiltersBySection = false;

			SetUpObjectsForReleaseGroupFilteringTests(config);

			AssertShowAllMatchesCorrectBehaviour("Filter by Release Group is on so the results should exclude workflows that match the filters but are not in the release group. SAD!",
				config, new[] { "Matching Workflow in Release Group" });
		}

		public void TestShowAllWorkflowsMatchingCriteria_FilterByReleaseGroup_OverriddenToTrue()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			config.PlatinumRule.BAB_FC_Component = ZGuid.Empty;
			config.PlatinumRule.BAB_FiltersByReleaseGroup = false;
			config.PlatinumRule.BAB_FiltersBySection = false;

			SetUpObjectsForReleaseGroupFilteringTests(config);

			var sectionBand = config.BucketSection.SectionConfiguration.AcceptabilityBands.Cast<BoardSectionAcceptabilityBand>().Single(x => x.AcceptabilityBand == config.PlatinumRule);
			sectionBand.FiltersByReleaseGroupOverride = BoardSectionAcceptabilityBandLookups.YesOptionCode;

			Factory.Save();
			AssertShowAllMatchesCorrectBehaviour("Filtering by Release Group is overridden to Yes, so it should exclude workflows that match the filters but aren't in the release group. SAD!",
				config, new[] { "Matching Workflow in Release Group" });
		}

		public void TestShowAllWorkflowsMatchingCriteria_FilterByReleaseGroup_OverriddenToFalse()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			config.PlatinumRule.BAB_FC_Component = ZGuid.Empty;
			config.PlatinumRule.BAB_FiltersByReleaseGroup = true;
			config.PlatinumRule.BAB_FiltersBySection = false;

			SetUpObjectsForReleaseGroupFilteringTests(config);

			var sectionBand = config.BucketSection.SectionConfiguration.AcceptabilityBands.Cast<BoardSectionAcceptabilityBand>().Single(x => x.AcceptabilityBand == config.PlatinumRule);
			sectionBand.FiltersByReleaseGroupOverride = BoardSectionAcceptabilityBandLookups.NoOptionCode;

			Factory.Save();
			AssertShowAllMatchesCorrectBehaviour("Filtering by Release Group is overridden to No, so all workflows that match the filters should be included regardless of release group. SAD!",
				config, new[] { "Matching Workflow in Release Group", "Matching Workflow not in Release Group" });
		}

		public void TestShowAllWorkflowsMatchingCriteria_AcceptabilityBandUsingAGRBandTypeShouldThrowUnitTestUserNotification()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var aggregateAcceptabilityBand = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 4, 5, 6, 7, "Aggregate Band with SQL", type: AcceptabilityBandTypes.Codes.Aggregate);

			aggregateAcceptabilityBand.BAB_SqlText = @"SELECT Count(*) Value, FH_FC_CurrentComponent Component, FH_GG_ReleaseGroup ReleaseGroup from dbo.ProcessHeader Group By FH_FC_CurrentComponent, FH_GG_ReleaseGroup";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", currentComponent: config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(section, aggregateAcceptabilityBand);
			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();
				var tile = form.FindAll<AcceptabilityBandTileControl>().Single(t => t.ViewModel.BandName == "Aggregate Band with SQL");
				TriggerShowAllMatches(tile);
			}

			AssertEquals("Only Acceptability Bands based on filter strips can be used to display Job Workflows.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		void SetUpObjectsForReleaseGroupFilteringTests(AcceptabilityBandTestConfig config)
		{
			var sqlAcceptabilityBand = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "SQL Band", type: AcceptabilityBandTypes.Codes.SQL);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Matching Workflow in Release Group", currentComponent: config.Bucket);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Matching Workflow not in Release Group", currentComponent: config.Bucket);
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Non-Matching Workflow in Release Group", currentComponent: config.Bucket);
			var workflow4 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Non-Matching Workflow not in Release Group", currentComponent: config.Bucket);

			var releaseGroupPk = config.ReleaseGroup.PK;
			config.BucketSection.MS_GG_ReleaseGroup = releaseGroupPk;
			workflow1.FH_GG_ReleaseGroup = releaseGroupPk;
			workflow2.FH_GG_ReleaseGroup = ZGuid.Empty;
			workflow3.FH_GG_ReleaseGroup = releaseGroupPk;
			workflow4.FH_GG_ReleaseGroup = ZGuid.Empty;

			workflow1.AddTag(config.PlatinumTag);
			workflow2.AddTag(config.PlatinumTag);
			workflow3.AddTag(config.GoldTag);
			workflow4.AddTag(config.GoldTag);

			BMSTestHelper.AddAcceptabilityBandToSection(config.BucketSection, config.PlatinumRule);
			BMSTestHelper.AddAcceptabilityBandToSection(config.BucketSection, config.RedRule);
			BMSTestHelper.AddAcceptabilityBandToSection(config.BucketSection, sqlAcceptabilityBand);

			Factory.Save();
		}

		#endregion

		#region Filter by-related Assertion

		static void AssertShowAllMatchesCorrectBehaviour(string assertMessage, AcceptabilityBandTestConfig config, string[] expectedWorkflowDescriptions)
		{
			using (var form = GetAndShowVisualBoardForm(config.BucketBoard))
			{
				var tiles = form.FindAll<AcceptabilityBandTileControl>().ToArray();
				AssertEquals(3, tiles.Length);

				var tile1 = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == config.PlatinumRule.PK);
				var tile2 = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == config.RedRule.PK);
				var tile3 = tiles.Single(t => t != tile1 && t != tile2);

				AssertEquals("Platinum Golden Rule: " + expectedWorkflowDescriptions.Length, tile1.NameAndResultLabel.Text);
				AssertEquals("Red Golden Rule: 0", tile2.NameAndResultLabel.Text);
				AssertEquals("SQL Band: ", tile3.NameAndResultLabel.Text);

				BusinessObject[] result = null;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(popup =>
				{
					if (popup is EmbeddedModulePopup modulePopup)
					{
						result = modulePopup.Module_ForTest.GridCollection.ToArray();
					}
				});

				TriggerShowAllMatches(tile1);
				AssertContainsExactElementsInAnyOrder(assertMessage, expectedWorkflowDescriptions, result.Cast<ProcessHeader>().Select(x => x.Name));

				TriggerShowAllMatches(tile2);
				AssertEquals("Only Acceptability Bands based on filter strips can be used to display Job Workflows.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();

				TriggerShowAllMatches(tile3);
				AssertEquals("Only Acceptability Bands based on filter strips can be used to display Job Workflows.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Other

		public void TestShowAllWorkflowsMatchingCriteria_FilterBySectionNum()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);

			config.PlatinumRule.BAB_FiltersByReleaseGroup = false;
			config.PlatinumRule.BAB_FiltersBySection = true;

			var numAcceptabilityBand = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 1, 2, 3, 4, 5, "NUM Band", type: AcceptabilityBandTypes.Codes.Count);

			var capability1 = BMSTestHelper.CreateCapability(Factory, "DJI", "Djibouti");
			var capability2 = BMSTestHelper.CreateCapability(Factory, "JAM", "Jamaica");

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "SDJ", "Sheik Djibouti", capability1, capability2);
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "JME", "Jamaican Meangry", capability1);

			var buffer1 = config.Buffer;
			var buffer2 = BMSTestHelper.CreateBuffer(config.System, "buffer2");

			var section1 = config.BufferSection;
			var section2 = BMSTestHelper.CreateBoardSection(buffer2, config.BufferBoard, row: 1);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow1 - buffer 1", currentComponent: buffer1, releaseDateTime: ZDateTime.Now.AddDays(-3), staffCode: staff1.GS_Code);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow2 - buffer 1", currentComponent: buffer1, releaseDateTime: ZDateTime.Now.AddDays(-3), staffCode: staff1.GS_Code);
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow3 - buffer 1", currentComponent: buffer1, releaseDateTime: ZDateTime.Now.AddDays(-3), staffCode: staff1.GS_Code);
			var workflow4 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow4 - buffer 2 - staff 1 - buffer 2", currentComponent: buffer2, releaseDateTime: ZDateTime.Now.AddDays(-3), capability: capability1, staffCode: staff1.GS_Code);
			var workflow5 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow5 - buffer 1 - nostaff - capability 1", currentComponent: buffer1, releaseDateTime: ZDateTime.Now.AddDays(-3), capability: capability1);
			var workflow6 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow6 - buffer 1 - staff 2 - capability 2", currentComponent: buffer1, releaseDateTime: ZDateTime.Now.AddDays(-3), capability: capability2, staffCode: staff2.GS_Code);

			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, staff1.PK, true);

			workflow1.AddTag(config.PlatinumTag);
			workflow2.AddTag(config.PlatinumTag);
			workflow3.AddTag(config.GoldTag);
			workflow4.AddTag(config.PlatinumTag);
			workflow5.AddTag(config.GoldTag);
			workflow6.AddTag(config.PlatinumTag);

			BMSTestHelper.AddAcceptabilityBandToSection(section1, config.PlatinumRule);
			BMSTestHelper.AddAcceptabilityBandToSection(section1, config.RedRule);
			BMSTestHelper.AddAcceptabilityBandToSection(section1, numAcceptabilityBand);

			Factory.Save();

			AssertEquals("ORG", workflow1.FH_WorkflowType);

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(config.BufferBoard)))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindAll<BMComponentControl>().First();
				var filterManager = control.ViewModel.FilterManager;
				var tiles = form.FindAll<AcceptabilityBandTileControl>().ToArray();
				AssertEquals(3, tiles.Length);

				var tile1 = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == config.PlatinumRule.PK);
				var tile2 = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == config.RedRule.PK);
				var tile3 = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == numAcceptabilityBand.PK);

				BusinessObject[] result = null;
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((o) =>
				{
					var popupForm = o as EmbeddedModulePopup;
					if (popupForm != null)
					{
						popupForm.Closing += (s, e) =>
						{
							result = popupForm.Module_ForTest.GridCollection.ToArray();
						};
					}
				});

				CombineAssertions(() =>
				{
					TriggerShowAllMatches(tile1);
					AssertEquals("Workflow count and tag should match", "Platinum Golden Rule: 2", tile1.NameAndResultLabel.Text);
					AssertEquals("Should display results, as two workflows have Platinum tag configuration and obey the Golden Rule", 2, result.Length);
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

					TriggerShowAllMatches(tile2);
					AssertEquals("Red Golden Rule: 0", tile2.NameAndResultLabel.Text);

					TriggerShowAllMatches(tile3);
					AssertEquals("Should display results, as four workflows have NUM Band configuration", 4, result.Length);
					AssertEquals("Only Acceptability Bands based on filter strips can be used to display Job Workflows.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Workflow count and tag should match", "NUM Band: 4", tile3.NameAndResultLabel.Text);
				});
			}
		}

		public void TestShowAllMatchesMenuItem_ShouldShowExportToExcelOption()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var sqlAcceptabilityBand = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "SQL Band", type: AcceptabilityBandTypes.Codes.SQL);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", currentComponent: config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);

			workflow1.AddTag(config.PlatinumTag);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var sectionBand1 = BMSTestHelper.AddAcceptabilityBandToSection(section, config.PlatinumRule);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var tile1 = form.FindAll<AcceptabilityBandTileControl>().Single(t => t.ViewModel.AcceptabilityBandPK == config.PlatinumRule.PK);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(popup =>
				{
					AssertNotNull(((EmbeddedModulePopup)popup).Module_ForTest.FormActionMenu.FindByText("Export All Columns To Excel", true));
				});

				TriggerShowAllMatches(tile1);
			}
		}

		public void TestShowMatchingItems_Refresh()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", currentComponent: config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var sectionBand1 = BMSTestHelper.AddAcceptabilityBandToSection(section, config.PlatinumRule);

			Factory.Save();

			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				var control = form.FindAll<BMComponentControl>().Single();
				var filterManager = control.ViewModel.FilterManager;
				var tiles = form.FindAll<AcceptabilityBandTileControl>().ToArray();
				AssertEquals(1, tiles.Length);

				var tile1 = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == config.PlatinumRule.PK);

				AssertEquals(0, filterManager.AppliedFilters.Count());
				AssertEquals(AcceptabilityBandTileControl.ShowMatchingItemsLabel, BMSGUITestCase.GetHighlightLabelMenuItem(tile1).Text);

				BMSGUITestCase.ToggleShowMatchingItems(tile1);

				AssertType<AcceptabilityBandMatchesVisibilityFilter>(filterManager.AppliedFilters.Single());
				BMSGUITestCase.AssertTasksNotShown("Non-platinum items should be hidden", control, task1);

				workflow1.AddTag(config.PlatinumTag);
				Factory.Save();

				form.RefreshBoard();

				AssertType<AcceptabilityBandMatchesVisibilityFilter>(filterManager.AppliedFilters.Single());
				BMSGUITestCase.AssertTasksShown("Adding platinum-tag to workflow and refresh should show platinum items", control, task1);
			}
		}

		public void TestShowMatchingItems_WhenLoadFromDatabaseThrowsException_ShouldShowErrorToUser_AndNotReportNullReference()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "A", currentComponent: config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "B", currentComponent: config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);

			workflow1.AddTag(config.PlatinumTag);

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 120);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;

			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(section, config.PlatinumRule);

			Factory.Save();

			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				var control = form.FindAll<BMComponentControl>().Single();
				var filterManager = control.ViewModel.FilterManager;
				var tile = form.FindSingle<AcceptabilityBandTileControl>();
				AssertEquals("Platinum Golden Rule: 1", tile.NameAndResultLabel.Text);

				AssertEquals(0, filterManager.AppliedFilters.Count());
				BMSGUITestCase.AssertTasksShown("No filter applied so all tasks should be visible", control, task1, task2);

				FilterStripsTestHelper.AddCustomSQLFilterStrip(config.PlatinumRule.FilterRule, "FH_ThisWillThrowException = 'A'");
				Factory.Save();

				try
				{
					BMSGUITestCase.ToggleShowMatchingItems(tile);
				}
				catch (Exception ex)
				{
					AssertStartsWith("Hmm", "Invalid column name", ex.Message);
				}
				AssertEquals(0, filterManager.AppliedFilters.Count());
				BMSGUITestCase.AssertTasksShown("The filter couldn't be applied, so all tasks should be visible.", control, task1, task2);
			}
		}

		public void TestEditContextMenu()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = Factory.New<GlbGroup>().PK;
			var band = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(bucket, 0, 0, 0, 0, 0, 0);
			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(section, band);

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders.AddNew();

			Factory.Save();

			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				var tile = form.FindAll<AcceptabilityBandTileControl>().Single();
				tile.ContextMenuStrip.Items.Cast<ZToolStripMenuItem>().Single(i => i.Text == AcceptabilityBandTileControl.EditLabel).PerformClick();
				Application.DoEvents();

				using (var acceptabilityBandForm = Application.OpenForms.OfType<AcceptabilityBandForm>().SingleOrDefault())
				{
					AssertNotNull("Should open acceptability band form", acceptabilityBandForm);
				}
			}
		}

		public void TestShowMatchingItems()
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
				var control = form.FindAll<BMComponentControl>().Single();
				var filterManager = control.ViewModel.FilterManager;
				var tiles = form.FindAll<AcceptabilityBandTileControl>().ToArray();
				AssertEquals(3, tiles.Length);

				var tile1 = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == config.PlatinumRule.PK);
				var tile2 = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == config.RedRule.PK);
				var tile3 = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == sqlAcceptabilityBand.PK);

				AssertEquals(0, filterManager.AppliedFilters.Count());
				BMSGUITestCase.AssertTasksShown("No filter applied so all tasks should be visible", control, task1, task2, task3);

				BMSGUITestCase.ToggleShowMatchingItems(tile1);
				AssertType<AcceptabilityBandMatchesVisibilityFilter>(filterManager.AppliedFilters.Single());
				BMSGUITestCase.AssertTasksShown("Only platinum items should be shown", control, task1);
				BMSGUITestCase.AssertTasksNotShown("Non-platinum items should be hidden", control, task2, task3);
				Assert(BMSGUITestCase.GetHighlightLabelMenuItem(tile1).Checked);

				BMSGUITestCase.ToggleShowMatchingItems(tile1);
				AssertEquals(0, filterManager.AppliedFilters.Count());
				BMSGUITestCase.AssertTasksShown("Toggling same filter should deactivate it", control, task1, task2, task3);
				Assert(!BMSGUITestCase.GetHighlightLabelMenuItem(tile1).Checked);

				BMSGUITestCase.ToggleShowMatchingItems(tile2);
				AssertEquals(0, filterManager.AppliedFilters.Count());
				Assert("Red rule -> title2 do not have filters so won't toggle", !BMSGUITestCase.GetHighlightLabelMenuItem(tile2).Checked);
				AssertEquals("Only Acceptability Bands based on filter strips can be used to Show matching cards.", UnitTestUserNotification.Instance.LastMessage.Text);
				BMSGUITestCase.AssertTasksShown("All items should be visible", control, task1, task2, task3);

				BMSGUITestCase.ToggleShowMatchingItems(tile1);
				AssertType<AcceptabilityBandMatchesVisibilityFilter>(filterManager.AppliedFilters.Single());
				BMSGUITestCase.AssertTasksShown("Toggling platinum filter should deactivate red filter", control, task1);
				BMSGUITestCase.AssertTasksNotShown("Non-platinum items should be hidden", control, task2, task3);
				Assert(BMSGUITestCase.GetHighlightLabelMenuItem(tile1).Checked);

				BMSGUITestCase.ToggleShowMatchingItems(tile3);
				Assert(!BMSGUITestCase.GetHighlightLabelMenuItem(tile3).Checked);
				AssertEquals("Only Acceptability Bands based on filter strips can be used to Show matching cards.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowMatchingItems_FilteredByReleaseGroup()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			BMSTestHelper.CreateReleaseGroup(config.System, group2);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", currentComponent: config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", currentComponent: config.Buffer, releaseGroupPK: group2.PK);

			jobHeader.AddTag(config.PlatinumTag);

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;

			config.PlatinumRule.BAB_FiltersByReleaseGroup = true;
			BMSTestHelper.AddAcceptabilityBandToSection(section, config.PlatinumRule);

			Factory.Save();

			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				var control = form.FindAll<BMComponentControl>().Single();
				var filterManager = control.ViewModel.FilterManager;
				var tile = form.FindAll<AcceptabilityBandTileControl>().Single();

				AssertEquals(0, filterManager.AppliedFilters.Count());
				BMSGUITestCase.AssertTasksShown("No filter applied so all tasks should be visible", control, task1, task2);

				AssertHighlightLabelMenuItemExists(tile);

				AssertEquals("We should specifically get the 'ShowMatchingItems' label, but instead, we got...", AcceptabilityBandTileControl.ShowMatchingItemsLabel, BMSGUITestCase.GetHighlightLabelMenuItem(tile).Text);

				BMSGUITestCase.ToggleShowMatchingItems(tile);

				AssertType<AcceptabilityBandMatchesVisibilityFilter>(filterManager.AppliedFilters.Single());
				BMSGUITestCase.AssertTasksShown("Only platinum items in the section release group should be shown", control, task1);
				BMSGUITestCase.AssertTasksNotShown("Platinum items outside the release group should be hidden", control, task2);
			}
		}

		static void AssertHighlightLabelMenuItemExists(AcceptabilityBandTileControl tile)
		{
			var assertionString = "Our tile should have menu items called " + AcceptabilityBandTileControl.ShowMatchingItemsLabel + ", but alas it does not. Instead it has: ";

			AssertNotNull("Our tile's ContextMenuStrip should have been initialized by now, but instead, it has not!", tile.ContextMenuStrip);
			AssertNotNull("Our tile's ContextMenuStrip's Items should have been initialized by now, but instead, it has not!", tile.ContextMenuStrip.Items);

			var tileMenuItems = tile.ContextMenuStrip.Items;

			foreach (ZToolStripMenuItem item in tileMenuItems)
			{
				assertionString = assertionString + "\r\n" + item.Text;
			}

			var specificItems = BMSGUITestCase.GetHighlightLabelMenuItem(tile);

			AssertNotNull(assertionString, specificItems);
		}

		#region Matching Items

#if WINZOR
		public void TestBandTileShouldHas3DStyleBorderWhenSelected()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			var sectionBand1 = BMSTestHelper.AddAcceptabilityBandToSection(section, config.PlatinumRule);

			Factory.Save();

			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				var tiles = form.FindAll<AcceptabilityBandTileControl>().ToArray();
				var tile1 = tiles.Single(t => t.ViewModel.AcceptabilityBandPK == config.PlatinumRule.PK);

				AssertNotContains("border-top: 3px #c00 double;border-left: 3px #c00 double;border-bottom:3px black solid;border-right:3px black solid;outline: 1px black solid;outline-offset: -1px;", tile1.ExtraStyleString);
				AssertNotContains("top: 1px; left: 1px;", tile1.NameAndResultLabel.ExtraStyleString);

				BMSGUITestCase.ToggleShowMatchingItems(tile1);

				AssertContains("border-top: 3px #c00 double;border-left: 3px #c00 double;border-bottom:3px black solid;border-right:3px black solid;outline: 1px black solid;outline-offset: -1px;", tile1.ExtraStyleString);
				AssertContains("top: 1px; left: 1px;", tile1.NameAndResultLabel.ExtraStyleString);
			}
		}
#endif

		#endregion

		#endregion

		#region Implementation

		static ZToolStripMenuItem GetShowAllMatchesMenuItem(AcceptabilityBandTileControl tile)
		{
			return tile.ContextMenuStrip.Items.Cast<ZToolStripMenuItem>().Single(i => i.Text == AcceptabilityBandTileControl.ShowAllWorkflowsLabel);
		}

		static void TriggerShowAllMatches(AcceptabilityBandTileControl tile)
		{
			GetShowAllMatchesMenuItem(tile).PerformClick();
		}

		protected override bool ShouldDisableAsyncBehaviour => true;

		#endregion
	}
}

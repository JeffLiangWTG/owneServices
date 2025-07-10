using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.ActivityLogging;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Win32;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class VisualBoardFormTest : BMSGUITestCase
	{
		class SysInfoWithNotEnoughVirtualMemoryForVisualBoard : ZSystemInformation
		{
			protected override void FillMemoryStatusStruct(ref MemoryStatusStructEx memoryStatus)
			{
				memoryStatus.ullAvailVirtual = (ulong)(98L) * 1024L * 1024L;
				memoryStatus.ullTotalPhys = (ulong)ZSystemInformation.MinimumRequirements.TotalPhysicalMemoryInMB * 2L * 1024L * 1024L;
				memoryStatus.ullAvailPhys = (ulong)ZSystemInformation.MinimumRequirements.TotalPhysicalMemoryInMB * 2L * 1024L * 1024L;
			}
		}

		#region Forced Reload

		public void TestForceReloadViaViewModel_ShouldReloadBoard()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System, "Decks Dark");
			BMSTestHelper.CreateBoardSection(config.Bucket, board);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var startingComponentControl = form.FindSingle<BMComponentControl>();

				form.RefreshBoard();

				AssertEquals(startingComponentControl, form.FindSingle<BMComponentControl>());

				((IBoardRefreshable)form.BoardViewModel).PerformRefreshAction(new ForcedReloadOperation());

				AssertNotEquals("Board should have been reloaded, so the section layout should have been recreated.", startingComponentControl, form.FindSingle<BMComponentControl>());
			}
		}

		public void TestForceReloadViaViewModel_ForSlideshow_ShouldReloadBoard()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board1 = BMSTestHelper.CreateBoard(config.System, "Decks Dark");
			var board2 = BMSTestHelper.CreateBoard(config.System, "Kadish Tolesa");

			BMSTestHelper.CreateBoardSection(config.Bucket, board1);
			BMSTestHelper.CreateBoardSection(config.Buffer, board2);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(slideshow))
			{
				var startingComponentControl = form.FindSingle<BMComponentControl>();

				AssertEquals(board1.PK, form.BoardViewModel.BoardPK);

				form.MoveNext();
				AssertEquals(board2.PK, form.BoardViewModel.BoardPK);

				var secondComponentControl = form.FindSingle<BMComponentControl>();

				AssertNotEquals(startingComponentControl, secondComponentControl);

				form.MoveNext();
				AssertEquals(board1.PK, form.BoardViewModel.BoardPK);
				AssertEquals(startingComponentControl, form.FindSingle<BMComponentControl>());

				form.MoveNext();
				AssertEquals(board2.PK, form.BoardViewModel.BoardPK);
				AssertEquals(secondComponentControl, form.FindSingle<BMComponentControl>());

				((IBoardRefreshable)form.BoardViewModel).PerformRefreshAction(new ForcedReloadOperation());
				Application.DoEvents();

				AssertEquals(board1.PK, form.BoardViewModel.BoardPK);
				AssertNotEquals("Board should have been reloaded, so the section layout should have been recreated.", startingComponentControl, form.FindSingle<BMComponentControl>());

				form.MoveNext();
				AssertEquals(board2.PK, form.BoardViewModel.BoardPK);
				AssertNotEquals("Board should have been reloaded, so cached section layout should not have been used.", secondComponentControl, form.FindSingle<BMComponentControl>());
			}
		}

		#endregion

		#region activityLog
		public void TestVisualBoardFormOpenThenClose_ActivityLogContainsBizo()
		{
			EnvProxy.Instance.Registry.UserEventTrackingEnterprise = true;

			var system = VisualBoardsTestHelper.CreateSystem(Factory);
			var board = system.Boards.AddNew();
			Factory.Save();

			ZFormActivityLogger.Instance.StatLogs.Clear();
			ZFormActivityLogger.Instance.EnableActivityLogger();

			using (var form = new VisualBoardForm(GetViewModel(board)))
			{
				var stats = UserEventTracker.Instance.GetFormStatsForForm(form);
				form.Show();
				form.Close();

				AssertEquals(stats.BusinessObjectPK, board.PK);
				AssertEquals(stats.BusinessObjectTableCode, "MB");

				stats.NotifyFormShownUtc(board.MB_Description, "MBBoard", DateTime.UtcNow);
				ZFormActivityLogger.AllowActivityLogSavesInTests = true;
				ZFormActivityLogger.Instance.SavePendingLogs(true);
				ZFormActivityLogger.AllowActivityLogSavesInTests = false;

				var activityRecord = Factory.LoadTop1<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_ParentID, board.PK));
				AssertNotNull(activityRecord);
				AssertEquals(activityRecord.S7_ParentTableCode, "MB");
			}
		}
		#endregion

		#region Hyperlink

		class HyperlinkTest : BMSTestCaseWithFactory
		{
			public void TestCopyHyperlinkToClipboard_Board()
			{
				var board = Factory.NewWithValidTestData<BMBoard>();
				board.MB_Name = "Dat Board";

				Factory.Save();

				var helper = new ClipboardTestHelper();
				using (helper.MockClipboard())
				using (var form = new VisualBoardForm(GetViewModel(board)))
				{
					KeySender.SendKeyDownToProcessCmdKey(form, (int)(Keys.Control | Keys.H));
					AssertEquals("Dat Board", (string)helper.ClipboardData.GetData(DataFormats.Text));

					var url = (string)helper.ClipboardData.GetData(DataFormats.Html);
					AssertContains(string.Format("<html><body><!--StartFragment--><a href=\"edient:Command=ShowEditForm&ControllerID=VisualBoard&BusinessEntityPK={0}&VersionNumber={1}&Hash=%", board.PK, new EnterpriseInformationRetriever().VersionNumber), url);
					AssertEndsWith("URL end", ">Dat Board</a><!--EndFragment--></body></html>", url);
				}
			}

			public void TestCopyHyperlinkToClipboard_Slideshow()
			{
				var board1 = Factory.NewWithValidTestData<BMBoard>();
				board1.MB_Name = "Dat Board 1";
				var board2 = Factory.NewWithValidTestData<BMBoard>();
				board2.MB_Name = "Dat Board 2";

				var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2);
				slideshow.MD_Name = "Dat Slideshow";

				Factory.Save();

				var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);
				var helper = new ClipboardTestHelper();
				using (helper.MockClipboard())
				using (var form = new VisualBoardForm(viewModel))
				{
					KeySender.SendKeyDownToProcessCmdKey(form, (int)(Keys.Control | Keys.H));
					AssertEquals("Dat Slideshow", (string)helper.ClipboardData.GetData(DataFormats.Text));

					var url = (string)helper.ClipboardData.GetData(DataFormats.Html);
					AssertContains(string.Format("<html><body><!--StartFragment--><a href=\"edient:Command=ShowEditForm&ControllerID=VisualBoard&BusinessEntityPK={0}&VersionNumber={1}&Hash=%", slideshow.PK, new EnterpriseInformationRetriever().VersionNumber), url);
					AssertEndsWith("URL end", ">Dat Slideshow</a><!--EndFragment--></body></html>", url);
				}
			}

			public void TestStartFormFromUrl_Board()
			{
				var board = Factory.NewWithValidTestData<BMBoard>();
				board.MB_Name = "Dat Board";

				Factory.Save();

				var url = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.VisualBoard, board.PK);
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				Application.DoEvents();

				using (var form = Application.OpenForms.OfType<VisualBoardForm>().SingleOrDefault())
				{
					AssertNotNull(form);
					AssertEquals(board.PK, ZFormUtilities.GetBusiness(form).Identifier);

					var recentItems = RecentItemManager.Instance.GetRecentItems(ModuleIDs.VisualBoard.Name);
					AssertCollectionContains(recentItems, l => l.STL_ItemPK == board.PK);
				}
			}

			public void TestStartFormFromUrl_Slideshow()
			{
				var board1 = Factory.NewWithValidTestData<BMBoard>();
				board1.MB_Name = "Dat Board 1";
				var board2 = Factory.NewWithValidTestData<BMBoard>();
				board2.MB_Name = "Dat Board 2";

				var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2);

				Factory.Save();

				var url = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.VisualBoard, slideshow.PK);
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				Application.DoEvents();

				using (var form = Application.OpenForms.OfType<VisualBoardForm>().SingleOrDefault())
				{
					AssertNotNull(form);
					AssertEquals(slideshow.PK, ZFormUtilities.GetBusiness(form).Identifier);

					var recentItems = RecentItemManager.Instance.GetRecentItems(ModuleIDs.VisualBoard.Name);
					AssertCollectionContains(recentItems, l => l.STL_ItemPK == slideshow.PK);
				}
			}

			protected override bool ShouldDisableAsyncBehaviour => true;
			IDisposable instanceDetailsDisposable;

			protected override void SetUp()
			{
				base.SetUp();
				instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest();
			}

			protected override void TearDown()
			{
				instanceDetailsDisposable?.Dispose();
				base.TearDown();
			}
		}

		#endregion

		#region Current User Channel

		public void TestCurrentUserChannel_WhenCurrentLoggedInUserNotAlreadyExplicitlyConfiguredInSetOfChannels_ShouldShowCurrentUserAlongsideConfiguredChannels()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron A. Aaronson");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BBB", "Barry B. Barrymore");

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, description: "Yowser");
			var task2 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, 60, description: "Bowser");
			var task3 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, 60, description: "Kaplowser");

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource1.PK, displaySequence: 1);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.CurrentUser, null, displaySequence: 2);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource2.PK, displaySequence: 3);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var control = form.FindSingle<BMComponentControl>();
				var channelHeadings = control.ViewModel.ComponentGrid.Cells.Where(c => c.ContentType == CellContentType.ChannelHeading).ToArray();

				AssertEquals(3, channelHeadings.Length);
				AssertEquals(resource1.PK, channelHeadings[0].Channel.EntityPK);
				AssertEquals(Env.CurrentUserPK, channelHeadings[1].Channel.EntityPK);
				AssertEquals(resource2.PK, channelHeadings[2].Channel.EntityPK);

				var taskCard1 = form.FindAll<TaskCardControl>().SingleOrDefault(t => t.CardContent.GetTask(Factory).PK == task1.PK);
				var taskCard2 = form.FindAll<TaskCardControl>().SingleOrDefault(t => t.CardContent.GetTask(Factory).PK == task2.PK);
				var taskCard3 = form.FindAll<TaskCardControl>().SingleOrDefault(t => t.CardContent.GetTask(Factory).PK == task3.PK);

				AssertNotNull(taskCard1);
				AssertNotNull(taskCard2);
				AssertNotNull(taskCard3);
			}
		}

		public void TestCurrentUserChannel_WhenCurrentLoggedInUserIsExplicitlyConfiguredInSetOfChannels_ShouldNotShowDuplicateChannels()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron A. Aaronson");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BBB", "Barry B. Barrymore");

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, description: "Yowser");
			var task2 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, 60, description: "Bowser");
			var task3 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, 60, description: "Kaplowser");

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource1.PK, displaySequence: 1);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.CurrentUser, null, displaySequence: 2);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, Env.CurrentUserPK, displaySequence: 3);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource2.PK, displaySequence: 4);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var control = form.FindSingle<BMComponentControl>();
				var channelHeadings = control.ViewModel.ComponentGrid.Cells.Where(c => c.ContentType == CellContentType.ChannelHeading).ToArray();

				AssertEquals(3, channelHeadings.Length);
				AssertEquals(resource1.PK, channelHeadings[0].Channel.EntityPK);
				AssertEquals(Env.CurrentUserPK, channelHeadings[1].Channel.EntityPK);
				AssertEquals(resource2.PK, channelHeadings[2].Channel.EntityPK);

				var taskCard1 = form.FindAll<TaskCardControl>().SingleOrDefault(t => t.CardContent.GetTask(Factory).PK == task1.PK);
				var taskCard2 = form.FindAll<TaskCardControl>().SingleOrDefault(t => t.CardContent.GetTask(Factory).PK == task2.PK);
				var taskCard3 = form.FindAll<TaskCardControl>().SingleOrDefault(t => t.CardContent.GetTask(Factory).PK == task3.PK);

				AssertNotNull(taskCard1);
				AssertNotNull(taskCard2);
				AssertNotNull(taskCard3);
			}
		}

		#endregion

		#region Filters

		public void TestDisableRiskFilter_ShouldNotCollapseChannelHeadings()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.CCR.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR2.PK);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var control = form.FindSingle<BMComponentControl>();
				var cell = control.ViewModel.ComponentGrid.Cells.Single(c => c.ContentType == CellContentType.ChannelHeading && c.Channel.EntityPK == config.CCR.PK);

				AssertEquals(false, control.IsHeaderExpanded_ForTest(cell));

				ToggleHeadingExpansion(control, cell);
				AssertEquals(true, control.IsHeaderExpanded_ForTest(cell));

				FindAndClickRiskFilterMenuItem(control);
				AssertEquals(true, control.ViewModel.FilterManager.IsApplied(typeof(BoardMeetingModeFilter)));
				AssertEquals(true, control.IsHeaderExpanded_ForTest(cell));

				FindAndClickRiskFilterMenuItem(control);
				AssertEquals(false, control.ViewModel.FilterManager.IsApplied(typeof(BoardMeetingModeFilter)));
				AssertEquals(true, control.IsHeaderExpanded_ForTest(cell));
			}
		}

		public void TestCurrentTaskFilter_TextFilter_FindIntersect()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow.FH_GG_ReleaseGroup = group.PK;
			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, description: "Yowser");
			var task2 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, description: "Bowser");

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var componentControl = form.FindAll<BMComponentControl>().Single();
				var currentTaskMenuItem = componentControl.ContextMenuStrip.Items[0];
				AssertEquals("Show Startable Items", currentTaskMenuItem.Text);

				var taskCard1 = form.FindAll<TaskCardControl>().Single(t => t.CardContent.GetTask(Factory).PK == task1.PK);
				var taskCard2 = form.FindAll<TaskCardControl>().Single(t => t.CardContent.GetTask(Factory).PK == task2.PK);

				AssertEquals(true, taskCard1.Visible);
				AssertEquals(true, taskCard2.Visible);

				var searchControl = form.FindAll<ZSearchBox>().Single();
				searchControl.SearchTerm = "owser";
				searchControl.OnSearchPerformed(false);

				currentTaskMenuItem.PerformClick();
				taskCard1 = form.FindAll<TaskCardControl>().SingleOrDefault(t => t.CardContent.GetTask(Factory).PK == task1.PK);
				taskCard2 = form.FindAll<TaskCardControl>().SingleOrDefault(t => t.CardContent.GetTask(Factory).PK == task2.PK);
				AssertNotNull(taskCard1);
				AssertNull(taskCard2);
			}
		}

		public void TestCurrentTaskFilter_ToggleAFewTimes_ShouldKeepWorking()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow.FH_GG_ReleaseGroup = group.PK;
			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var componentControl = form.FindAll<BMComponentControl>().Single();
				var currentTaskMenuItem = (ZToolStripMenuItem)componentControl.ContextMenuStrip.Items[0];
				AssertEquals("Show Startable Items", currentTaskMenuItem.Text);
				AssertEquals(false, currentTaskMenuItem.Checked);

				var taskCard = form.FindAll<TaskCardControl>().SingleOrDefault(t => t.CardContent.GetTask(Factory).PK == task2.PK);
				AssertEquals(true, taskCard.Visible);

				currentTaskMenuItem.PerformClick();
				taskCard = form.FindAll<TaskCardControl>().SingleOrDefault(t => t.CardContent.GetTask(Factory).PK == task2.PK);
				AssertNull(taskCard);
				AssertEquals(true, currentTaskMenuItem.Checked);

				currentTaskMenuItem.PerformClick();
				taskCard = form.FindAll<TaskCardControl>().SingleOrDefault(t => t.CardContent.GetTask(Factory).PK == task2.PK);
				AssertNotNull(taskCard);
				AssertEquals(false, currentTaskMenuItem.Checked);

				currentTaskMenuItem.PerformClick();
				taskCard = form.FindAll<TaskCardControl>().SingleOrDefault(t => t.CardContent.GetTask(Factory).PK == task2.PK);
				AssertNull(taskCard);
				AssertEquals(true, currentTaskMenuItem.Checked);

				currentTaskMenuItem.PerformClick();
				taskCard = form.FindAll<TaskCardControl>().SingleOrDefault(t => t.CardContent.GetTask(Factory).PK == task2.PK);
				AssertNotNull(taskCard);
				AssertEquals(false, currentTaskMenuItem.Checked);
			}
		}

		public void TestCurrentTaskFilter_ApplyAndRemoveTwice()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, "ROG");

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.CardType = "TSK";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "INTJ", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task1 = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, 60, sequence: 10, description: "1"); // current
			var task2 = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, 60, sequence: 20, description: "2"); // not current

			Factory.Save();

			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(section.Board))
			{
				var taskCardsInitial = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(2, taskCardsInitial.Length);

				var control = form.FindAll<BMComponentControl>().Single();
				var currentTaskFilter = (ZToolStripMenuItem)control.ContextMenuStrip.Items[0];

				control.ContextMenuStrip.Items.Cast<ToolStripItem>().First(i => i.Text == "Show Startable Items").PerformClick();
				var taskCards1 = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals("Applying the Current Items filter should make task2 disappear", 1, taskCards1.Length);
				AssertEquals(true, currentTaskFilter.Checked);

				control.ContextMenuStrip.Items.Cast<ToolStripItem>().First(i => i.Text == "Show Startable Items").PerformClick();
				var taskCards2 = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals("Applying the Current Items filter should make task2 disappear", 2, taskCards2.Length);
				AssertEquals(false, currentTaskFilter.Checked);

				control.ContextMenuStrip.Items.Cast<ToolStripItem>().First(i => i.Text == "Show Startable Items").PerformClick();
				var taskCards3 = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals("Applying the Current Items filter should make task2 disappear", 1, taskCards3.Length);
				AssertEquals(true, currentTaskFilter.Checked);

				control.ContextMenuStrip.Items.Cast<ToolStripItem>().First(i => i.Text == "Show Startable Items").PerformClick();
				var taskCards4 = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals("Applying the Current Items filter should make task2 disappear", 2, form.FindAll<TaskCardControl>().ToArray().Length);
				AssertEquals(false, currentTaskFilter.Checked);
			}
		}

		[TestDate(2017, 11, 16)]
		public void TestCurrentTaskFilterAndRiskFilter_ApplyBothFiltersThenRemoveOneFilter_ShouldNotAffectOtherFilter()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory, "ROG");

			ConstrainedModeHelper.SwitchToConstrainedMode(config.ReleaseGroup, config.Buffer.PK);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.CardType = "TSK";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "INTJ", config.Buffer, releaseDateTime: ZDateTime.UtcToday, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "ENTJ", config.Buffer, releaseDateTime: ZDateTime.UtcToday.AddDays(-15), releaseGroupPK: config.ReleaseGroup.PK);
			var task1_1 = BMSTestHelper.CreateTask(workflow1, config.NonCCR1.GS_Code, 60, sequence: 10, description: "1_1"); // not risk and current
			var task1_2 = BMSTestHelper.CreateTask(workflow1, config.NonCCR1.GS_Code, 60, sequence: 20, description: "1_2"); // not risk and not current
			var task2_1 = BMSTestHelper.CreateTask(workflow2, config.NonCCR1.GS_Code, 60, sequence: 10, description: "2_1"); // risk and current
			var task2_2 = BMSTestHelper.CreateTask(workflow2, config.NonCCR1.GS_Code, 60, sequence: 20, description: "2_2"); // risk and not current

			Factory.Save();

			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(section.Board))
			{
				var taskCardsInitial = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals(4, taskCardsInitial.Length);

				var control = form.FindAll<BMComponentControl>().Single();

				FindAndClickRiskFilterMenuItem(control);
				var taskCardsRisk = form.FindAll<TaskCardControl>().ToArray();
				AssertContainsExactElementsInAnyOrder(taskCardsRisk.Select(s => s.CardContent.GetTask(Factory).P9_Description).ToArray(), new ZString[] { task2_1.P9_Description, task2_2.P9_Description });
				AssertEquals("Applying the risk filter should make task1_1 and task1_2 disappear", 2, taskCardsRisk.Length);

				control.ContextMenuStrip.Items.Cast<ToolStripItem>().First(i => i.Text == "Show Startable Items").PerformClick();
				var taskCardsCurrent = form.FindAll<TaskCardControl>().ToArray();
				AssertContainsExactElementsInAnyOrder(taskCardsCurrent.Select(s => s.CardContent.GetTask(Factory).P9_Description).ToArray(), new ZString[] { task2_1.P9_Description });
				AssertEquals("Applying current items filter should make task1_2 and task2_2 disappear", 1, taskCardsCurrent.Length);

				FindAndClickRiskFilterMenuItem(control);
				var taskCardsRemoveRisk = form.FindAll<TaskCardControl>().ToArray();
				AssertContainsExactElementsInAnyOrder(taskCardsRemoveRisk.Select(s => s.CardContent.GetTask(Factory).P9_Description).ToArray(), new ZString[] { task1_1.P9_Description, task2_1.P9_Description });
				AssertEquals("Removing risk filter should make task1_1 reappear", 2, taskCardsRemoveRisk.Length);

				control.ContextMenuStrip.Items.Cast<ToolStripItem>().First(i => i.Text == "Show Startable Items").PerformClick();
				var taskCardsRemoveCurrent = form.FindAll<TaskCardControl>().ToArray();
				AssertContainsExactElementsInAnyOrder(taskCardsRemoveCurrent.Select(s => s.CardContent.GetTask(Factory).P9_Description).ToArray(), new ZString[] { task1_1.P9_Description, task1_2.P9_Description, task2_1.P9_Description, task2_2.P9_Description });
				AssertEquals("Removing current item filter should make all tasks reappear", 4, taskCardsRemoveCurrent.Length);
			}
		}

		public void TestCurrentTaskFilter_FilterButtonHooks()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow.FH_GG_ReleaseGroup = group.PK;
			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();
				var filterButton = (FilterButton)form.Find(control => control is FilterButton).First();
				AssertEquals(false, filterButton.hasFilters);

				var componentControl = form.FindAll<BMComponentControl>().Single();
				var currentTaskMenuItem = (ZToolStripMenuItem)componentControl.ContextMenuStrip.Items[0];
				AssertEquals("Show Startable Items", currentTaskMenuItem.Text);
				AssertEquals(false, currentTaskMenuItem.Checked);

				var taskCard = form.FindAll<TaskCardControl>().Single(t => t.CardContent.GetTask(Factory).PK == task2.PK);
				AssertEquals(true, taskCard.Visible);
				AssertEquals(false, filterButton.hasFilters);

				currentTaskMenuItem.PerformClick();

				AssertEquals(filterButton.filterable, form.SlideShowViewModel);
				Assert(form.SlideShowViewModel.FilterManager.AllChildFilterables.SelectMany(f => f.FilterManager.AppliedFilters).Any());

				Application.DoEvents();
				AssertEquals(false, taskCard.Visible);
				AssertEquals(true, currentTaskMenuItem.Checked);
				AssertEquals(true, filterButton.hasFilters);

				currentTaskMenuItem.PerformClick();
				AssertEquals(false, filterButton.hasFilters);
				AssertEquals(false, currentTaskMenuItem.Checked);
			}
		}

		public void TestCurrentTaskFilter_OpenTasksShouldBlockStartabilityOfNextTask()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow.FH_GG_ReleaseGroup = group.PK;
			var openTask = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 1, taskStatus: "OPN");
			var assignedTask = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 2, taskStatus: "ASN");

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var componentControl = form.FindSingle<BMComponentControl>();
				var currentTaskMenuItem = (ZToolStripMenuItem)componentControl.ContextMenuStrip.Items[0];
				AssertEquals("Show Startable Items", currentTaskMenuItem.Text);
				AssertEquals(false, currentTaskMenuItem.Checked);

				var openTaskCard = form.FindSingle<TaskCardControl>(t => t.CardContent.GetTask(Factory).PK == openTask.PK);
				var assignedTaskCard = form.FindSingle<TaskCardControl>(t => t.CardContent.GetTask(Factory).PK == assignedTask.PK);
				AssertEquals("Open tasks should be visible when 'Show Startable Items' is not checked.", true, openTaskCard.Visible);
				AssertEquals("Assigned tasks should be visible when 'Show Startable Items' is not checked.", true, assignedTaskCard.Visible);

				currentTaskMenuItem.PerformClick();
				openTaskCard = form.FindSingleOrDefault<TaskCardControl>(t => t.CardContent.GetTask(Factory).PK == openTask.PK);
				assignedTaskCard = form.FindSingleOrDefault<TaskCardControl>(t => t.CardContent.GetTask(Factory).PK == assignedTask.PK);
				AssertNull("Open tasks should not be startable because it's OPN, not ASN, WRK, or SUS.", openTaskCard);
				AssertNull("Assigned tasks should not be startable because there's an open task that comes earlier in the sequence.", assignedTaskCard);
				AssertEquals(true, currentTaskMenuItem.Checked);
			}
		}

		public void TestCurrentTaskFilter_CloseTasksShouldNotBlockStartabilityOfNextTask()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow.FH_GG_ReleaseGroup = group.PK;
			var closeTask = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 1, taskStatus: "CLS");
			var assignedTask = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 2, taskStatus: "ASN");

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var componentControl = form.FindSingle<BMComponentControl>();
				var currentTaskMenuItem = (ZToolStripMenuItem)componentControl.ContextMenuStrip.Items[0];
				AssertEquals("Show Startable Items", currentTaskMenuItem.Text);
				AssertEquals(false, currentTaskMenuItem.Checked);

				var closeTaskCard = form.FindSingleOrDefault<TaskCardControl>(t => t.CardContent.GetTask(Factory).PK == closeTask.PK);
				var assignedTaskCard = form.FindSingle<TaskCardControl>(t => t.CardContent.GetTask(Factory).PK == assignedTask.PK);
				AssertNull("Close tasks should not be on visual boards.", closeTaskCard);
				AssertEquals("Assigned tasks should be visible when 'Show Startable Items' is not checked.", true, assignedTaskCard.Visible);

				currentTaskMenuItem.PerformClick();
				closeTaskCard = form.FindSingleOrDefault<TaskCardControl>(t => t.CardContent.GetTask(Factory).PK == closeTask.PK);
				assignedTaskCard = form.FindSingle<TaskCardControl>(t => t.CardContent.GetTask(Factory).PK == assignedTask.PK);
				AssertNull("Close tasks should not be on visual boards.", closeTaskCard);
				AssertEquals("Assigned tasks should be startable because there's no open task that comes earlier in the sequence.", true, assignedTaskCard.Visible);
				AssertEquals(true, currentTaskMenuItem.Checked);
			}
		}

		public void TestCurrentTaskFilter_OpenTasksShouldNotBlockStartabilityOfTasksInSameOrEarlierSequence()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow.FH_GG_ReleaseGroup = group.PK;
			var assignedTask = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 1, taskStatus: "ASN");
			var openTask1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 1, taskStatus: "OPN");
			var openTask2 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 2, taskStatus: "OPN");

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var componentControl = form.FindSingle<BMComponentControl>();
				var currentTaskMenuItem = (ZToolStripMenuItem)componentControl.ContextMenuStrip.Items[0];
				AssertEquals("Show Startable Items", currentTaskMenuItem.Text);
				AssertEquals(false, currentTaskMenuItem.Checked);

				var assignedTaskCard = form.FindSingle<TaskCardControl>(t => t.CardContent.GetTask(Factory).PK == assignedTask.PK);
				var openTaskCard1 = form.FindSingle<TaskCardControl>(t => t.CardContent.GetTask(Factory).PK == openTask1.PK);
				var openTaskCard2 = form.FindSingle<TaskCardControl>(t => t.CardContent.GetTask(Factory).PK == openTask2.PK);
				AssertEquals("Assigned tasks should be visible when 'Show Startable Items' is not checked.", true, assignedTaskCard.Visible);
				AssertEquals("Open tasks should be visible when 'Show Startable Items' is not checked.", true, openTaskCard1.Visible);
				AssertEquals("Open tasks should be visible when 'Show Startable Items' is not checked.", true, openTaskCard2.Visible);

				currentTaskMenuItem.PerformClick();
				assignedTaskCard = form.FindSingle<TaskCardControl>(t => t.CardContent.GetTask(Factory).PK == assignedTask.PK);
				openTaskCard1 = form.FindSingleOrDefault<TaskCardControl>(t => t.CardContent.GetTask(Factory).PK == openTask1.PK);
				openTaskCard2 = form.FindSingleOrDefault<TaskCardControl>(t => t.CardContent.GetTask(Factory).PK == openTask2.PK);
				AssertEquals("Assigned tasks should be startable because there's no open task that comes earlier in the sequence.", true, assignedTaskCard.Visible);
				AssertNull("Open tasks should not be startable because it's OPN, not ASN, WRK, or SUS.", openTaskCard1);
				AssertNull("Open tasks should not be startable because it's OPN, not ASN, WRK, or SUS.", openTaskCard2);
				AssertEquals(true, currentTaskMenuItem.Checked);
			}
		}

		public void TestShouldNotCancelTasksFromBoard_WhenUserDoesNotHaveRequisitePermission() // modify as appropriate
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			section.SectionConfiguration.OverrideChannels = true;

			var layout = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.DetailedCard, 300, 200, "Hot Pink");
			BMSTestHelper.CreateStaticControlCustomisation(layout, StaticControlTypeList.Codes.SaveButton, "SAVE", 120, 10, 50, 20, "White", "Black", 8, false, false);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, layout);

			var staff = BMSTestHelper.CreateStaff(Factory, "AAA");
			staff.GS_FullName = "Aaron A. Aaronson";

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "aaa", buffer);
			var task = BMSTestHelper.CreateTask(workflow);
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Type = "UDF";

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypes = categorisedTaskTypes.AddNew();
			taskTypes.Code = "ORG";
			var taskType = taskTypes.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.CanCancelTask = false;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
			Env.Security.WorkflowTasksCanCancelAllTasks.IsAllowed = false;

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var ticket = form.FindAll<TaskCardControl>().SingleOrDefault();
				AssertNotNull(ticket);

				ticket.ShowDetailedCard();

				var detailedCard = form.FindAll<TaskCardDetailControl>().Single();
				detailedCard.ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
				detailedCard.Save_ForTest(ensureNoValidationErrors: false);
				Application.DoEvents();

				CombineAssertions(() =>
				{
					AssertNotNull("The simple task card should still be on the form due to the validation error produced when attempting to cancel the task", form.FindAll<TaskCardControl>().SingleOrDefault());
					AssertNotNull("The detailed task card should not be closed due to the validation error produced when attempting to cancel the task", form.FindAll<TaskCardControl>().SingleOrDefault());
				});
			}
		}

		public void TestPlay_ReordersOnSave()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			section.SectionConfiguration.OverrideChannels = true;

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "PRI");
			var green = BMSTestHelper.CreateTagMagnitude(tagGroup, "GRN", nudge: 200);
			var platinum = BMSTestHelper.CreateTagMagnitude(tagGroup, "PLT", nudge: 100);
			var gold = BMSTestHelper.CreateTagMagnitude(tagGroup, "GLD", nudge: 50);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Grr", buffer);
			workflow1.AddTag(green);
			BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);

			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Argh", buffer);
			workflow2.AddTag(platinum);
			BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			var workflow3 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, ":/", buffer);
			workflow3.AddTag(gold);
			BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();
				var card = form.FindAll<TaskCardControl>().First();

				var panel = (TaskPanel)card.Parent;

				AssertEquals(CardSortType.BufferWorkSequence, card.Cell.CardSortType);

				var cards = panel.Controls.OfType<TaskCardControl>().ToArray();

				AssertEquals(workflow1.PK, cards[0].CardContent.GetWorkflow(Factory).PK);
				AssertEquals(workflow2.PK, cards[1].CardContent.GetWorkflow(Factory).PK);
				AssertEquals(workflow3.PK, cards[2].CardContent.GetWorkflow(Factory).PK);

				cards[1].ShowDetailedCard();

				var detailedCard = form.FindAll<TaskCardDetailControl>().First();
				detailedCard.ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

				Application.DoEvents();
				detailedCard.Save_ForTest();
				Application.DoEvents();

				cards = panel.Controls.OfType<TaskCardControl>().ToArray();

				AssertEquals(workflow2.PK, cards[0].CardContent.GetWorkflow(Factory).PK);
				AssertEquals(workflow1.PK, cards[1].CardContent.GetWorkflow(Factory).PK);
				AssertEquals(workflow3.PK, cards[2].CardContent.GetWorkflow(Factory).PK);

				cards[0].ShowDetailedCard();

				detailedCard = form.FindAll<TaskCardDetailControl>().First();
				detailedCard.ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

				Application.DoEvents();
				detailedCard.Save_ForTest();
				Application.DoEvents();

				cards = panel.Controls.OfType<TaskCardControl>().ToArray();

				AssertEquals(workflow1.PK, cards[0].CardContent.GetWorkflow(Factory).PK);
				AssertEquals(workflow3.PK, cards[1].CardContent.GetWorkflow(Factory).PK);
				AssertEquals(workflow2.PK, cards[2].CardContent.GetWorkflow(Factory).PK);
			}
		}

		public void TestNudge_ReordersOnSave_IsReleaseScheduler()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var bucket = BMSTestHelper.CreateBucket(system);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			BMSTestHelper.CreateReleaseGroup(system, group);
			var staff = (GlbStaff)BMSTestHelper.GetOrCreateStaff(Factory, "ABC");
			group.Staff.Add(staff);
			var section = BMSTestHelper.CreateReleaseSchedulerBoardSection(buffer, group);

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "PRI");
			var green = BMSTestHelper.CreateTagMagnitude(tagGroup, "GRN", nudge: 3);
			var platinum = BMSTestHelper.CreateTagMagnitude(tagGroup, "PLT", nudge: 2);
			var gold = BMSTestHelper.CreateTagMagnitude(tagGroup, "GLD", nudge: 1);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow1", currentComponent: bucket, releaseGroupPK: group.PK);
			workflow1.AddTag(green);
			BMSTestHelper.CreateTask(workflow1, staff.GS_Code, 60);

			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow2", currentComponent: bucket, releaseGroupPK: group.PK);
			workflow2.AddTag(platinum);
			BMSTestHelper.CreateTask(workflow2, staff.GS_Code, 60);

			var workflow3 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow3", currentComponent: bucket, releaseGroupPK: group.PK);
			workflow3.AddTag(gold);
			BMSTestHelper.CreateTask(workflow3, staff.GS_Code, 60);

			BMSTestHelper.LinkComponents(bucket, buffer, isReleaseGate: true);

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();
				var card = form.FindAll<TaskCardControl>().First();
				var panel = (TaskPanel)card.Parent;

				AssertEquals(CardSortType.ReleaseSequence, card.Cell.CardSortType);

				var cards = panel.Controls.OfType<TaskCardControl>().ToArray();

				AssertEquals(workflow1.PK, cards[0].CardContent.GetWorkflow(Factory).PK);
				AssertEquals(workflow2.PK, cards[1].CardContent.GetWorkflow(Factory).PK);
				AssertEquals(workflow3.PK, cards[2].CardContent.GetWorkflow(Factory).PK);

				cards[1].ShowDetailedCard();
				var detailedCard = form.FindAll<TaskCardDetailControl>().First();
				detailedCard.ProcessTask.ProcessHeader.NudgeUp();
				detailedCard.ProcessTask.ProcessHeader.NudgeUp();

				Application.DoEvents();
				detailedCard.Save_ForTest();
				Application.DoEvents();

				cards = panel.Controls.OfType<TaskCardControl>().ToArray();

				cards[2].ShowDetailedCard();
				detailedCard = form.FindAll<TaskCardDetailControl>().First();
				detailedCard.ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

				Application.DoEvents();
				detailedCard.Save_ForTest();
				Application.DoEvents();

				cards = panel.Controls.OfType<TaskCardControl>().ToArray();

				CombineAssertions(() =>
				{
					AssertSamePK("First card", workflow2, cards[0].CardContent.GetWorkflow(Factory));
					AssertSamePK("Second card", workflow1, cards[1].CardContent.GetWorkflow(Factory));
					AssertSamePK("Third card", workflow3, cards[2].CardContent.GetWorkflow(Factory));
				});

				cards[0].ShowDetailedCard();
				detailedCard = form.FindAll<TaskCardDetailControl>().First();

				detailedCard.ProcessTask.ProcessHeader.NudgeDown();
				detailedCard.ProcessTask.ProcessHeader.NudgeDown();
				detailedCard.ProcessTask.ProcessHeader.NudgeDown();
				detailedCard.ProcessTask.ProcessHeader.NudgeDown();

				detailedCard.Save_ForTest();

				cards = panel.Controls.OfType<TaskCardControl>().ToArray();

				CombineAssertions(() =>
				{
					AssertSamePK("First card", workflow1, cards[0].CardContent.GetWorkflow(Factory));
					AssertSamePK("Second card", workflow3, cards[1].CardContent.GetWorkflow(Factory));
					AssertSamePK("Third card", workflow2, cards[2].CardContent.GetWorkflow(Factory));
				});
			}
		}

		[TestDate(2017, 6, 26)]
		public void TestLoadBoard_ShouldNotHitRegistryForParentJobFilter()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, "RES", GlbStaff.CurrentUser.PK, overrideChannels: true);
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			const string registryKeyWeWantToNotHitWhenLoadingBoard = "DefaultBookingNewButton";
			FreightDataRegistry.Instance.RemoveItemFromCacheIfOlderThan(registryKeyWeWantToNotHitWhenLoadingBoard, TimeSpan.MinValue);

			using (Db.Connection.TrackExecutedCommands())
			using (GetAndShowVisualBoardForm(config.BufferBoard))
			{
				AssertNotContains(registryKeyWeWantToNotHitWhenLoadingBoard, string.Join(System.Environment.NewLine, Db.Connection.ExecutedCommands));
			}

			FreightDataRegistry.Instance.RemoveItemFromCacheIfOlderThan(registryKeyWeWantToNotHitWhenLoadingBoard, TimeSpan.MinValue);

			using (Db.Connection.TrackExecutedCommands())
			{
				FilterStripsTestHelper.AddFilterStrip<ModuleGuidModuleSpecifiedFilter>(config.BufferSection.WorkflowFilter, "Parent Job", filter => filter.SelectedModule = ModuleIDs.ProcessTasks.Name);
				AssertContains("This assertion proves that the freight registry is normally hit when validating the list of modules on the parent job filter. If freight stop doing that registry check when creating that particular controller, we probably need to change this test to check something different to ensure that controllers aren't created by that filter when the board loads.",
					registryKeyWeWantToNotHitWhenLoadingBoard, string.Join(System.Environment.NewLine, Db.Connection.ExecutedCommands));
			}
		}

		#endregion

		#region Search Filter

		public void TestFilter_WhenShowingBoardThenShouldApplyUserDefinedFilter()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessTasks))
			{
				var filterBizO = module.FilterBusinessObject;
				var descriptionFilter = filterBizO.AddFilterStrip<ModuleTextFilter>("Description");
				descriptionFilter.Property = "user defined description";

				FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "User Defined Filter X", isPublished: true, isPublishedGlobal: true, isUserDefinedFilter: true);
			}

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			FilterStripsTestHelper.AddFilterStrip<ModuleUserDefinedFilter>(section.TaskFilter, "[USR]User Defined Filter X");

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "workflow1", config.Buffer, description: "user defined description");
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: "workflow2", config.Buffer, description: "task 2");

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();

				var tickets = form.FindAll<TaskCardControl>().Select(ticket => ticket.CardContent.DisplayTextForDebugging);
				AssertContainsExactElementsInAnyOrder("Should only show workflow that match UserDefinedFilter", new[] { "Task: [user defined description]; Workflow: [workflow1]" }, tickets);
			}
		}

		public void TestSearch()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			var section = BMSTestHelper.CreateBoardSection(bucket, board);

			Factory.Save();

			var viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				var searchControl = form.FindAll<ZSearchBox>().FirstOrDefault();
				AssertNotNull("Should be a ZSearchBox on the form", searchControl);

				searchControl.SearchTerm = "Blah";
				searchControl.OnSearchPerformed(false);

				AssertEquals(true, form.BoardViewModel.FilterManager.IsApplied(typeof(SearchFilter)));

				searchControl.OnSearchPerformed(true);
				AssertEquals(false, form.BoardViewModel.FilterManager.IsApplied(typeof(SearchFilter)));
			}
		}

		public void TestSearchShowsTermSearchedFor()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			var section = BMSTestHelper.CreateBoardSection(bucket, board);

			Factory.Save();

			var viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				var searchControl = form.FindAll<ZSearchBox>().FirstOrDefault();
				AssertNotNull("Should be a ZSearchBox on the form", searchControl);

				searchControl.SearchTerm = "Kogan ";
				searchControl.OnSearchPerformed(false);
				AssertEquals("Search term should have no whitespace at the end.", "Kogan", searchControl.SearchTerm);

				searchControl.SearchTerm = " Kogan ";
				searchControl.OnSearchPerformed(false);
				AssertEquals("Search term should keep space before word.", " Kogan", searchControl.SearchTerm);

				searchControl.SearchTerm = "Kogan   e";
				searchControl.OnSearchPerformed(false);
				AssertEquals("Search term should not have changed.", "Kogan   e", searchControl.SearchTerm);
			}
		}

		public void TestSearch_FiltersTaskCards()
		{
			var factory1 = new BusinessObjectFactory { NameForDebugging = "Factory1", RefreshEnabled = false };
			var group = BMSTestHelper.CreateGroup(factory1, "AAA");
			var system = BMSTestHelper.CreateSystem(factory1, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var resource = factory1.NewWithValidTestData<GlbStaff>();
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory1);
			var workflow = BMSTestHelper.CreateProcessHeaderAndTask(jobHeader, bucket, 1, resource.GS_Code, releaseGroupPK: group.PK);
			var task2 = workflow.Parent.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow.PK;
			task2.P9_Description = "Blah";

			factory1.Save();

			var viewModel = GetViewModel(section.Board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				form.Refresh();
				Application.DoEvents();

				var taskPanel = form.FindAll<TaskPanel>().Single();
				var taskCards = form.FindAll<TaskCardControl>();
				AssertEquals(2, taskCards.Count(t => t.Visible));
				AssertTotalTasksButtonAndCellTaskControlTaskCount(form, taskPanel, 2);

				var searchControl = form.FindAll<ZSearchBox>().FirstOrDefault();
				AssertNotNull("Should be a ZSearchBox on the form", searchControl);

				searchControl.SearchTerm = "Blah";
				searchControl.OnSearchPerformed(false);

				AssertEquals(true, form.BoardViewModel.FilterManager.IsApplied(typeof(SearchFilter)));

				Application.DoEvents();
				taskCards = form.FindAll<TaskCardControl>();
				AssertEquals(1, taskCards.Count(t => t.Visible));
				AssertTotalTasksButtonAndCellTaskControlTaskCount(form, taskPanel, 1);

				searchControl.OnSearchPerformed(true);
				AssertEquals(false, form.BoardViewModel.FilterManager.IsApplied(typeof(SearchFilter)));

				Application.DoEvents();
				taskCards = form.FindAll<TaskCardControl>();
				AssertEquals(2, taskCards.Count(t => t.Visible));
				AssertTotalTasksButtonAndCellTaskControlTaskCount(form, taskPanel, 2);
			}
		}

		public void TestSearch_RepositionTaskCards_Tiled()
		{
			Search_RepositionTaskCards(PanelLayoutTypeList.Codes.Stacked);
		}

		public void TestSearch_RepositionTaskCards_Stacked()
		{
			Search_RepositionTaskCards(PanelLayoutTypeList.Codes.Staggered);
		}

		void Search_RepositionTaskCards(ZString panelLayoutStyle)
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			section.SectionConfiguration.PanelLayoutStyle = panelLayoutStyle;
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "WadadaGanadada", releaseGroupPK: group.PK);

			var task1 = CreateTask(workflow, resource.GS_Code, 60, description: "Goop");
			var task2 = CreateTask(workflow, resource.GS_Code, 60, description: "Loop");

			Factory.Save();

			var viewModel = GetViewModel(section.Board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				form.Refresh();
				Application.DoEvents();

				var taskPanel = form.FindAll<TaskPanel>().Single();
				var taskCards = form.FindAll<TaskCardControl>().Where(t => t.Visible).ToArray();
				AssertEquals(2, taskCards.Length);

				AssertEquals(task1.PK, taskCards[0].CardContent.TaskIdentifier);
				var task1Position = new Point(taskCards[0].Left, taskCards[0].Top);

				var searchControl = form.FindAll<ZSearchBox>().Single();
				searchControl.SearchTerm = "Loop";
				searchControl.OnSearchPerformed(false);

				AssertEquals(true, form.BoardViewModel.FilterManager.IsApplied(typeof(SearchFilter)));

				Application.DoEvents();
				taskCards = form.FindAll<TaskCardControl>().Where(t => t.Visible).ToArray();

				AssertEquals(1, taskCards.Length);
				AssertEquals(task2.PK, taskCards[0].CardContent.TaskIdentifier);

				AssertCloseEnoughForJazz(task1Position.X, taskCards[0].Left, 5);
				AssertCloseEnoughForJazz(task1Position.Y, taskCards[0].Top, 5);
			}
		}

		public void TestFilter_ButtonShownOnlyWhenFilterIsApplied()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			var section = BMSTestHelper.CreateBoardSection(bucket, board);

			Factory.Save();

			var viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				var searchControl = form.FindAll<ZSearchBox>().FirstOrDefault();
				AssertNotNull("Should be a ZSearchBox on the form", searchControl);
				Assert("Filter button is not visible", !form.controlsPanel.FilterButton.Visible);
				searchControl.SearchTerm = "Blah";
				searchControl.OnSearchPerformed(false);

				AssertEquals(true, form.BoardViewModel.FilterManager.IsApplied(typeof(SearchFilter)));
				Assert("Filter button is now visible", form.controlsPanel.FilterButton.Visible);
				searchControl.OnSearchPerformed(true);
				AssertEquals(false, form.BoardViewModel.FilterManager.IsApplied(typeof(SearchFilter)));
				Assert("Filter button is hidden", !form.controlsPanel.FilterButton.Visible);
			}
		}

		public void TestSearch_SlideShow_FiltersTaskCards()
		{
			var factory1 = new BusinessObjectFactory { NameForDebugging = "Factory1", RefreshEnabled = false };
			var group = BMSTestHelper.CreateGroup(factory1, "AAA");
			var system = BMSTestHelper.CreateSystem(factory1, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var resource = factory1.NewWithValidTestData<GlbStaff>();
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory1);
			var workflow = BMSTestHelper.CreateProcessHeaderAndTask(jobHeader, bucket, 1, resource.GS_Code, releaseGroupPK: group.PK);
			var task = workflow.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_Description = "Flamboyant Badger";

			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(bucket, board);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board, 10), Tuple.Create(board, 10));

			factory1.Save();

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				form.Refresh();
				Application.DoEvents();

				MoveNext_ForTest(form);

				MoveNext_ForTest(form);

				var taskPanel = form.FindAll<TaskPanel>().Single();
				var taskCards = form.FindAll<TaskCardControl>();
				AssertEquals(2, taskCards.Count(t => t.Visible));
				AssertTotalTasksButtonAndCellTaskControlTaskCount(form, taskPanel, 2);

				var searchControl = form.FindAll<ZSearchBox>().FirstOrDefault();
				AssertNotNull("Should be a ZSearchBox on the form", searchControl);

				searchControl.SearchTerm = "Badger";
				searchControl.OnSearchPerformed(false);

				AssertEquals(true, form.BoardViewModel.FilterManager.IsApplied(typeof(SearchFilter)));

				Application.DoEvents();
				taskCards = form.FindAll<TaskCardControl>();
				AssertEquals(1, taskCards.Count(t => t.Visible));
				AssertTotalTasksButtonAndCellTaskControlTaskCount(form, taskPanel, 1);

				searchControl.OnSearchPerformed(true);
				Application.DoEvents();
				AssertEquals(false, form.BoardViewModel.FilterManager.IsApplied(typeof(SearchFilter)));

				Application.DoEvents();
				taskCards = form.FindAll<TaskCardControl>();
				AssertEquals(2, taskCards.Count(t => t.Visible));
				AssertTotalTasksButtonAndCellTaskControlTaskCount(form, taskPanel, 2);
			}
		}

		static void AssertTotalTasksButtonAndCellTaskControlTaskCount(VisualBoardForm form, TaskPanel taskPanel, int expectedTaskCount)
		{
			var totalTasksButton = taskPanel.FindAll<ZButton>().Single();

			if (expectedTaskCount > 0)
			{
				AssertEquals(expectedTaskCount.ToString(), totalTasksButton.Text);

				totalTasksButton.PerformClick();
				var cellTasksControl = form.CurrentlyShownCellTasksControl != null ? form.CurrentlyShownCellTasksControl.Target as CellTasksControl : null;
				AssertNotNull(cellTasksControl);

				AssertEquals(expectedTaskCount, cellTasksControl.FindAll<TaskCardControl>().Count());

				totalTasksButton.PerformClick();
				AssertNull(form.CurrentlyShownCellTasksControl);
			}
			else
			{
				AssertEquals(false, totalTasksButton.Visible);
			}
		}

		public void TestFilterFormSearchReturnsRightResults()
		{
			var system = CreateSystem("ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Complete!!", bucket, releaseGroupPK: group.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "dont search me!!", bucket, releaseGroupPK: group.PK);

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "BLA", "blah");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "XYZ", "Tootle");
			magnitude.TGM_Description = "Im a description";
			var tag1 = (TagLink)workflow1.AddTag(magnitude).Link;

			BMSTestHelper.CreateTask(workflow1, "", 60, description: "Workflow 1 Task Tagged 1");
			BMSTestHelper.CreateTask(workflow1, "", 60, description: "Workflow 1 Task Tagged 2");
			BMSTestHelper.CreateTask(workflow2, "", 60, description: "Workflow 2 Task Not Tagged 1");
			BMSTestHelper.CreateTask(workflow2, "", 60, description: "Workflow 2 Task Not Tagged 2");

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();
				var control = form.FindAll<BMComponentControl>().Single();

				Action<object> addFilters = ((f) =>
				{
					var zForm = (ZForm)f;
					var viewModel = (StmModuleFilterViewModel)zForm.DataSource;
					var stmFilter = viewModel.WorkflowFilter;
					stmFilter.S9_ModuleID = ModuleIDs.BMFilterRule.Name;

					FilterStripsTestHelper.AddFilterStrips(stmFilter,

					new FilterStripsTestHelper.FilterStripDefinition
					{
						FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
						FilterStripValueSetter = filter =>
						((ModuleTextFilter)filter).Property = "Complete!!"
					});
				});

				Assert("[Pre-Condition] Task cards with the tag to be searched for should be present.", ContainsTaskCardWithTag(form, tag1.Magnitude));
				Assert("[Pre-Condition] Task cards without the tag to be searched for should be present.", ContainsTaskCardWithoutTag(form, tag1.Magnitude)); // the form must contain tags both with and without the condition
				AssertEquals("The wrong number of visible task card controls are on the board", 4, form.FindAll<TaskCardControl>().Count(c => c.Visible));

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(new ZFormModaliser.PreShowInvoker(addFilters));
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				control.ShowFilterDialog();

				Assert("After applyng workflow filters, the task cards for the searched workflow should be present.", ContainsTaskCardWithTag(form, tag1.Magnitude));
				AssertEquals("After applying workflow filters, the task cards for one workflow should be left on the board.", 2, form.FindAll<TaskCardControl>().Count(c => c.Visible));

				addFilters = ((f) =>
				{
					var zForm = (ZForm)f;
					var viewModel = (StmModuleFilterViewModel)zForm.DataSource;
					var stmFilter = viewModel.TaskFilter;
					stmFilter.S9_ModuleID = ModuleIDs.ProcessTasks.Name;

					FilterStripsTestHelper.AddFilterStrips(stmFilter,

					new FilterStripsTestHelper.FilterStripDefinition
					{
						FilterStripName = "Description",
						FilterStripValueSetter = filter =>
						((ModuleTextFilter)filter).Property = "Workflow 1 Task Tagged 2"
					});
				});

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(new ZFormModaliser.PreShowInvoker(addFilters));
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				control.ShowFilterDialog();

				var taskCards = form.FindAll<TaskCardControl>().Where(c => c.Visible).ToArray();
				AssertEquals("After applying task filters, one task card should be left on the board.", 1, taskCards.Length);
				AssertEquals("After applying filters, the task card searched for should be present.", "Workflow 1 Task Tagged 2", taskCards.Single().CardContent.GetTask(Factory).P9_Description);
			}
		}

		[ExpectNoExceptions]
		public void TestSQLCustomFilterFormSearchRunsValidation()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var section = CreateBoardSection(bucket);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow 1", bucket);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow 2", bucket);

			BMSTestHelper.CreateTask(workflow1, "", 60, description: "Workflow 1 Task 1");
			BMSTestHelper.CreateTask(workflow1, "", 60, description: "Workflow 1 Task 2");
			BMSTestHelper.CreateTask(workflow2, "", 60, description: "Workflow 2 Task 1");
			BMSTestHelper.CreateTask(workflow2, "", 60, description: "Workflow 2 Task 2");
			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();
				var control = form.FindAll<BMComponentControl>().Single();

				Action<object> addFilters = ((f) =>
				{
					var zForm = (ZForm)f;
					var viewModel = (StmModuleFilterViewModel)zForm.DataSource;
					var stmFilter = viewModel.WorkflowFilter;
					stmFilter.S9_ModuleID = ModuleIDs.BMFilterRule.Name;

					FilterStripsTestHelper.AddFilterStrips(stmFilter,

					new FilterStripsTestHelper.FilterStripDefinition
					{
						FilterStripName = "Custom SQL Filter",
						FilterStripValueSetter = newFilter => ((ModuleSQLFilter)newFilter).Property1 = "Test",
					});

					var filterStripForm = zForm as FilterStripsForm;
					filterStripForm.Show();
					filterStripForm.ApplyButton.PerformClick();
				});

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(new ZFormModaliser.PreShowInvoker(addFilters));
				control.ShowFilterDialog();
			}
		}

		public void TestFilterSearchReturnsRightResults()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group);
			var section = CreateBoardSection(bucket);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Complete!!", bucket, releaseGroupPK: group.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Complete!!", bucket, releaseGroupPK: group.PK);

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "BLA", "blah");
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "XYZ", "Tootle");
			magnitude.TGM_Description = "Im a description";
			var tag1 = (TagLink)workflow1.AddTag(magnitude).Link;

			BMSTestHelper.CreateTask(workflow1, "", 60, description: "Workflow 1 Task Tagged");
			BMSTestHelper.CreateTask(workflow2, "", 60, description: "Workflow 2 Task Not Tagged");
			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(section.Board)))
			{
				form.Show();
				form.Refresh();
				Application.DoEvents();

				Assert("[Pre-Condition] No task card with the tag to be searched for is present.", ContainsTaskCardWithTag(form, tag1.Magnitude));
				Assert("[Pre-Condition] No task card without the tag to be searched for is present.", ContainsTaskCardWithoutTag(form, tag1.Magnitude)); // the form must contain tags both with and without the condition
				AssertEquals("The wrong amount of visible task card controls are on the board", 2, form.FindAll<TaskCardControl>().Count(c => c.Visible));

				Action postSearchAssertion = () => Assert("The card on the board does not have the tag that was searched for", ContainsTaskCardWithTag(form, tag1.Magnitude));
				SearchAndAssert(form, "XYZ", postSearchAssertion);
				SearchAndAssert(form, "BLA", postSearchAssertion);
				SearchAndAssert(form, "Im a description", postSearchAssertion);
			}
		}

		bool ContainsTaskCardWithTag(VisualBoardForm form, TagMagnitude magnitude)
		{
			var taskCardList = form.FindAll<TaskCardControl>();
			return taskCardList.Any(card => card.CardContent.GetTask(Factory).GetApplicableTags().Any(m => m.PK == magnitude.PK));
		}

		bool ContainsTaskCardWithoutTag(VisualBoardForm form, TagMagnitude magnitude)
		{
			var taskCardList = form.FindAll<TaskCardControl>();
			return taskCardList.Any(cardCheck1 => cardCheck1.CardContent.GetTask(Factory).GetApplicableTags().Any(m => m.PK != magnitude.PK)
				|| taskCardList.Any(cardCheck2 => !cardCheck2.CardContent.GetTask(Factory).GetApplicableTags().Any()));
		}

		public void TestFilterSearchBooleanProperty()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = CreateReleaseGroup(system, group);
			var section = CreateBoardSection(bucket, customReleaseGroupPK: group.PK);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Complete!!", bucket, releaseGroupPK: group.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Complete!!", bucket, releaseGroupPK: group.PK);

			BMSTestHelper.CreateTask(workflow1, "", 60, description: "Workflow 1 value true");
			BMSTestHelper.CreateTask(workflow2, "", 60, description: "Workflow 2 value false");
			workflow1.FH_IsCriticalHandover = true;
			workflow2.FH_IsCriticalHandover = false;
			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(section.Board)))
			{
				form.Show();
				form.Refresh();
				Application.DoEvents();

				Assert("[Pre-Condition] No task card with the tag to be searched for is present.", CheckForWorkflowsCriticalHandover(form, true));
				Assert("[Pre-Condition] No task card without the tag to be searched for is present.", CheckForWorkflowsCriticalHandover(form, false)); // the form must contain tags both with and without the condition
				AssertEquals("The wrong amount of visible task card controls are on the board", 2, form.FindAll<TaskCardControl>().Count(c => c.Visible));

				Action postSearchAssertion = () => Assert("The card on the board does not have the tag that was searched for", CheckForWorkflowsCriticalHandover(form, true));
				SearchAndAssert(form, ProcessHeader.ModuleFilterConstants.CriticalHandover, postSearchAssertion);
				SearchAndAssert(form, "Critical", postSearchAssertion);
				SearchAndAssert(form, "cal Han", postSearchAssertion);
				SearchAndAssert(form, "handover", postSearchAssertion);
			}
		}

		void SearchAndAssert(VisualBoardForm form, string searchTerm, Action postSearchAssertion)
		{
			var searchControl = form.FindAll<ZSearchBox>().FirstOrDefault();
			AssertNotNull("Should be a ZSearchBox on the form", searchControl);
			searchControl.SearchTerm = searchTerm;
			searchControl.OnSearchPerformed(false);
			AssertEquals("The search filter was not applied", true, form.BoardViewModel.FilterManager.IsApplied(typeof(SearchFilter)));

			Application.DoEvents();
			AssertEquals("The wrong amount of visible task card controls are on the board", 1, form.FindAll<TaskCardControl>().Count(c => c.Visible));
			postSearchAssertion();

			searchControl.SearchTerm = "";
			searchControl.OnSearchPerformed(false);
			AssertEquals("The wrong amount of visible task card controls are on the board", 2, form.FindAll<TaskCardControl>().Count(c => c.Visible));
		}

		bool CheckForWorkflowsCriticalHandover(VisualBoardForm form, bool expectedValue)
		{
			return form.FindAll<TaskCardControl>().Any(tc => tc.CardContent.GetWorkflow(Factory).FH_IsCriticalHandover == expectedValue);
		}

		public void TestFilterSearchButtonIsCorrectlyBinded()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board, 10), Tuple.Create(board, 10));
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			Factory.Save();

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				var filterButton = form.FindAll<FilterButton>().Single();

				AssertEquals(form.SlideShowViewModel, filterButton.filterable);

				MoveNext_ForTest(form);

				AssertEquals(form.SlideShowViewModel, filterButton.filterable);
			}
		}

		public void TestFilterSearchBoxFilterCorrectFilterManager()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board, 10), Tuple.Create(board, 10));
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			Factory.Save();

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				var searchControl = form.FindAll<ZSearchBox>().FirstOrDefault();

				searchControl.SearchTerm = "1,2,3,4 I declare a thumb war";

				searchControl.OnSearchPerformed(false);
				AssertEquals(true, form.BoardViewModel.FilterManager.IsApplied(typeof(SearchFilter)));
				AssertEquals(0, form.BoardViewModel.FilterManager.AppliedFilters.Count());

				AssertEquals(true, form.SlideShowViewModel.FilterManager.IsApplied(typeof(SearchFilter)));
				AssertEquals(1, form.SlideShowViewModel.FilterManager.AppliedFilters.Count());

				MoveNext_ForTest(form);

				AssertEquals(true, form.BoardViewModel.FilterManager.IsApplied(typeof(SearchFilter)));
				AssertEquals(0, form.BoardViewModel.FilterManager.AppliedFilters.Count());

				AssertEquals(true, form.SlideShowViewModel.FilterManager.IsApplied(typeof(SearchFilter)));
				AssertEquals(1, form.SlideShowViewModel.FilterManager.AppliedFilters.Count());
			}
		}

		[TestDate(2017, 10, 5)]
		public void TestSearch_DbHitsForTags()
		{
			var allowedHits = new Dictionary<string, int>
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsForAllFactories(allowedHits, includeFactoryPredicate: f => f.NameForDebugging == "ComponentGrid.RefreshFilters"))
			{
				var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
				var section = BMSTestHelper.CreateBoardSection(config.Buffer);

				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
				var grandparentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "grandparent", config.Buffer);
				var parentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "parent", config.Buffer);
				var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "child", config.Buffer);

				var task1 = BMSTestHelper.CreateTask(grandparentWorkflow);
				var task2 = BMSTestHelper.CreateTask(parentWorkflow);
				var task3 = BMSTestHelper.CreateTask(childWorkflow);

				BMSTestHelper.MakeChildOf(childWorkflow, parentWorkflow);
				BMSTestHelper.MakeChildOf(parentWorkflow, grandparentWorkflow);

				jobHeader.AddTag(config.DerpyHoovesTag);
				grandparentWorkflow.AddTag(config.PrincessCelestiaTag);
				parentWorkflow.AddTag(config.PrincessLunaTag);
				childWorkflow.AddTag(config.RainbowDashTag);

				Factory.Save();

				var viewModel = BMSTestHelper.CreateSlideshowViewModel(section.Board);

				using (var form = new VisualBoardForm(viewModel))
				{
					form.Show();

					var taskTickets = BMSGUITestCase.FindTaskCardControls(form);

					AssertEquals(3, taskTickets.Length);

					var searchControl = form.FindAll<ZSearchBox>().FirstOrDefault();

					searchControl.SearchTerm = "Rainbow Dash";
					searchControl.OnSearchPerformed(false);

					taskTickets = BMSGUITestCase.FindTaskCardControls(form);

					AssertEquals(1, taskTickets.Length);
					AssertEquals(task3.PK, taskTickets[0].CardContent.TaskIdentifier);
				}
			}
		}

		public void TestFilterJobLevelWorkflows_ByReleaseGroupSpecifiedInJobLevelWorkflow()
		{
			var sectionGroup = Factory.NewWithValidTestData<GlbGroup>();
			sectionGroup.GG_Code = "Grape";

			var otherGroup = Factory.NewWithValidTestData<GlbGroup>();
			otherGroup.GG_Code = "Grip";

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			BMSTestHelper.CreateReleaseGroup(config.System, sectionGroup);
			BMSTestHelper.CreateReleaseGroup(config.System, otherGroup);
			config.BucketSection.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			config.BucketSection.SectionConfiguration.ReleaseGroupPK = sectionGroup.PK;

			var jobHeaderSectionRG1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Section 1 Job-Level Workflow");
			jobHeaderSectionRG1.FH_GG_ReleaseGroup = sectionGroup.PK;
			var workflowSectionRG1 = BMSTestHelper.CreateWorkflow(jobHeaderSectionRG1, "Workflow Section 1", config.Bucket, releaseGroupPK: otherGroup.PK);
			BMSTestHelper.CreateTask(workflowSectionRG1, GlbStaff.CurrentUser.GS_Code, 60);

			var jobHeaderSectionRG2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Section 2 Job-Level Workflow");
			jobHeaderSectionRG2.FH_GG_ReleaseGroup = sectionGroup.PK;
			var workflowSectionRG2 = BMSTestHelper.CreateWorkflow(jobHeaderSectionRG2, "Workflow Section 2", config.Bucket, releaseGroupPK: otherGroup.PK);
			BMSTestHelper.CreateTask(workflowSectionRG2, GlbStaff.CurrentUser.GS_Code, 60);

			var jobHeaderOtherRG1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Other 1 Job-Level Workflow");
			jobHeaderOtherRG1.FH_GG_ReleaseGroup = otherGroup.PK;
			var workflowOtherRG1 = BMSTestHelper.CreateWorkflow(jobHeaderOtherRG1, "Workflow Other 1", config.Bucket, releaseGroupPK: sectionGroup.PK);
			BMSTestHelper.CreateTask(workflowOtherRG1, GlbStaff.CurrentUser.GS_Code, 60);

			var jobHeaderOtherRG2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Other 2 Job-Level Workflow");
			jobHeaderOtherRG2.FH_GG_ReleaseGroup = otherGroup.PK;
			var workflowOtherRG2 = BMSTestHelper.CreateWorkflow(jobHeaderOtherRG2, "Workflow Other 2", config.Bucket, releaseGroupPK: sectionGroup.PK);
			BMSTestHelper.CreateTask(workflowOtherRG2, GlbStaff.CurrentUser.GS_Code, 60);

			config.BucketSection.SectionConfiguration.ShowWorkInReleaseGroupOnly = true;
			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(config.BucketSection.Board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCards = form.FindAll<TaskCardControl>().Where(c => c.Visible).ToArray();
				CombineAssertions(() =>
				{
					AssertEquals("Only two task cards representing the job-level workflows assigned to the section release group should be shown", 2, taskCards.Length);
					AssertNotNull(taskCards.Single(c => c.CardContent.GetWorkflow(Factory).FH_CompletionStatement == "Section 1 Job-Level Workflow"));
					AssertNotNull(taskCards.Single(c => c.CardContent.GetWorkflow(Factory).FH_CompletionStatement == "Section 2 Job-Level Workflow"));
				});
			}

			config.BucketSection.SectionConfiguration.ShowWorkInReleaseGroupOnly = false;
			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(config.BucketSection.Board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCards = form.FindAll<TaskCardControl>().Where(c => c.Visible).ToArray();
				CombineAssertions(() =>
				{
					AssertEquals("All four task cards representing all job-level workflows should be shown", 4, taskCards.Length);
					AssertNotNull(taskCards.Single(c => c.CardContent.GetWorkflow(Factory).FH_CompletionStatement == "Section 1 Job-Level Workflow"));
					AssertNotNull(taskCards.Single(c => c.CardContent.GetWorkflow(Factory).FH_CompletionStatement == "Section 2 Job-Level Workflow"));
					AssertNotNull(taskCards.Single(c => c.CardContent.GetWorkflow(Factory).FH_CompletionStatement == "Other 1 Job-Level Workflow"));
					AssertNotNull(taskCards.Single(c => c.CardContent.GetWorkflow(Factory).FH_CompletionStatement == "Other 2 Job-Level Workflow"));
				});
			}
		}

		public void TestParentWorkflowFilterReturnsRightResults_JobLevelWorkflow()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			config.BucketSection.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Parent Job-Level Workflow");
			jobHeader1.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Parent Workflow 1", config.Bucket);
			BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Child Job-Level Workflow");
			jobHeader2.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Child Workflow 2", config.Bucket);
			BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			jobHeader2.GetOrCreateLinkToParent(jobHeader1);

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Unrelated Job-Level Workflow");
			jobHeader3.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "Unrelated Workflow 3 in another component", config.Buffer);
			BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(config.BucketSection.Board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCards = form.FindAll<TaskCardControl>().Where(c => c.Visible).ToArray();
				AssertEquals("Since there are no filters, two task card should be shown", 2, taskCards.Length);
				AssertNotNull("'Parent Job-Level Workflow' should be shown.", taskCards.Single(c => c.CardContent.GetWorkflow(Factory).FH_CompletionStatement == "Parent Job-Level Workflow"));
				AssertNotNull("'Child Job-Level Workflow' should be shown.", taskCards.Single(c => c.CardContent.GetWorkflow(Factory).FH_CompletionStatement == "Child Job-Level Workflow"));
			}

			FilterStripsTestHelper.AddFilterStrips(config.BucketSection.WorkflowFilter,
				new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.ParentWorkflows,
					FilterStripValueSetter = f => ((ModuleGuidPivotFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch
				});

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(config.BucketSection.Board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCards = form.FindAll<TaskCardControl>().Where(c => c.Visible).ToArray();
				AssertEquals("After applying workflow filters, one task card should be left on the board.", 1, taskCards.Length);
				AssertNotNull("'Parent Job-Level Workflow' should be shown.", taskCards.Single(c => c.CardContent.GetWorkflow(Factory).FH_CompletionStatement == "Parent Job-Level Workflow"));
			}
		}

		public void TestParentWorkflowFilterReturnsRightResults()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			config.BucketSection.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Parent Job-Level Workflow");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Parent Workflow 1", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Child Job-Level Workflow");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Child Workflow 2", config.Bucket, releaseGroupPK: config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			workflow2.GetOrCreateLinkToParent(workflow1);

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Unrelated Job-Level Workflow in another component");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "Unrelated Workflow 3", config.Buffer);
			BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(config.BucketSection.Board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCards = form.FindAll<TaskCardControl>().Where(c => c.Visible).ToArray();
				AssertEquals("Since there are no filters, two task card should be shown", 2, taskCards.Length);
				AssertNotNull("'Parent Workflow 1' should be shown.", taskCards.SingleOrDefault(c => c.CardContent.GetWorkflow(Factory).FH_CompletionStatement == "Parent Workflow 1"));
				AssertNotNull("'Child Workflow 2' should be shown.", taskCards.SingleOrDefault(c => c.CardContent.GetWorkflow(Factory).FH_CompletionStatement == "Child Workflow 2"));
			}

			FilterStripsTestHelper.AddFilterStrips(config.BucketSection.WorkflowFilter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.ParentWorkflows,
				FilterStripValueSetter = f => ((ModuleGuidPivotFilter)f).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch
			});

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(config.BucketSection.Board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCards = form.FindAll<TaskCardControl>().Where(c => c.Visible).ToArray();
				AssertEquals("After applying workflow filters, one task card should be left on the board.", 1, taskCards.Length);
				AssertNotNull("'Parent Workflow 1' should be shown.", taskCards.SingleOrDefault(c => c.CardContent.GetWorkflow(Factory).FH_CompletionStatement == "Parent Workflow 1"));
			}
		}

		#endregion

		#region Age Headings

		[TestDate(2018, 5, 4)]
		public void TestNoMultipleBoldAgeHeadingCells_WhenBoardIsRefreshedAndBufferPenetrationChanges()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Environment.Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var section = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty).Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ChannelSecondaryBy = BMConstants.ChannelByTimeCode;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today, staffCode: resource1.GS_Code, lowEstMinutes: 60);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-1), staffCode: resource1.GS_Code, lowEstMinutes: 60);
			var workflow3 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-2), staffCode: resource1.GS_Code, lowEstMinutes: 60);
			var workflow4 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-3), staffCode: resource1.GS_Code, lowEstMinutes: 60);

			var workflow5 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-4), staffCode: resource2.GS_Code, lowEstMinutes: 60);
			var workflow6 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-7), staffCode: resource2.GS_Code, lowEstMinutes: 60);
			var workflow7 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-8), staffCode: resource2.GS_Code, lowEstMinutes: 60);
			var workflow8 = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, section.Component, releaseDateTime: ZDateTime.Today.AddDays(-9), staffCode: resource2.GS_Code, lowEstMinutes: 60);

			Factory.Save();

			var sectionViewModel = BMSTestHelper.CreateViewModel(section);
			var boardViewModel = GetViewModel(section.Board);

			using (var form = new VisualBoardForm(boardViewModel))
			{
				form.Show();
				Application.DoEvents();

				var control = form.GetSectionControls().ToArray()[0] as BMComponentControl;

				var ageHeaderControl58 = control.FindAll<ZLabel>().Single(s => s.Text.StartsWith("58"));
				var ageHeaderControl75 = control.FindAll<ZLabel>().Single(s => s.Text.StartsWith("75"));

				AssertEquals("Only 58.3% header should now be bold", true, ageHeaderControl58.Font.Bold);
				AssertEquals("75% header should not be bold yet", false, ageHeaderControl75.Font.Bold);

				workflow1.FH_ReleaseDateTime = workflow1.FH_ReleaseDateTime.AddDays(-2);
				workflow2.FH_ReleaseDateTime = workflow2.FH_ReleaseDateTime.AddDays(-2);
				workflow3.FH_ReleaseDateTime = workflow3.FH_ReleaseDateTime.AddDays(-2);
				workflow4.FH_ReleaseDateTime = workflow4.FH_ReleaseDateTime.AddDays(-2);
				workflow5.FH_ReleaseDateTime = workflow5.FH_ReleaseDateTime.AddDays(-2);
				workflow6.FH_ReleaseDateTime = workflow6.FH_ReleaseDateTime.AddDays(-2);
				workflow7.FH_ReleaseDateTime = workflow7.FH_ReleaseDateTime.AddDays(-2);
				workflow8.FH_ReleaseDateTime = workflow8.FH_ReleaseDateTime.AddDays(-2);

				Factory.Save();

				form.controlsPanel.Expand();
				Application.DoEvents();

				form.controlsPanel.RefreshButton.PerformClick();
				Application.DoEvents();
				ageHeaderControl58 = control.FindAll<ZLabel>().Single(s => s.Text.StartsWith("58"));
				ageHeaderControl75 = control.FindAll<ZLabel>().Single(s => s.Text.StartsWith("75"));

				AssertEquals("58.3% header should no longer be bold", false, ageHeaderControl58.Font.Bold);
				AssertEquals("Only 75% header should now be bold", true, ageHeaderControl75.Font.Bold);
			}
		}

		#endregion

		#region BoardMeetingMode

		public void TestBoardMeetingMode_WithMultipleSections_WhenChannelsCreatedInDifferentOrderThanDisplaySequence_ShouldIterateSequentially()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var board = BMSTestHelper.CreateBoard(config.System);

			BMBoardSection CreateSection(string sectionName, int row, int col)
			{
				var section = BMSTestHelper.CreateBoardSection(config.Buffer, board, row, col);

				// Create the channels out of order, ensuring we test the channels sequencing properly.
				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK, displaySequence: 1);
				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource3.PK, displaySequence: 3);
				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK, displaySequence: 2);

				BMSTestHelper.SetOverriddenSectionName(section, sectionName);

				return section;
			}

			var section1 = CreateSection("Blue kangs are the best kangs", row: 0, col: 0);
			var section2 = CreateSection("As everybody knows", row: 0, col: 1);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				form.EnterBoardMeetingMode();

				AssertExpandedChannel(form, section1, resource1);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section1, resource2);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section1, resource3);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section2, resource1);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section2, resource2);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section2, resource3);

				// Moving past the final channel should roll back to the start

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section1, resource1);

				// Going backwards should work too.

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section2, resource3);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section2, resource2);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section2, resource1);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section1, resource3);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section1, resource2);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section1, resource1);
			}
		}

		public void TestBoardMeetingMode_WithMultipleSections_WhenSortingAlphabetically_AndStaffAddedToGroupAfterBoardSaved_ShouldIterateSequentially()
		{
			var group = BMSTestHelper.CreateGroup(Factory, "GRP", "The BEST group");
			var resource1 = BMSTestHelper.CreateStaff(Factory, "AD", "Adam");
			var resource2 = BMSTestHelper.CreateStaff(Factory, "BAD", "Badam");
			var resource3 = BMSTestHelper.CreateStaff(Factory, "CAD", "Cadam");
			group.Staff.Add(resource1);
			group.Staff.Add(resource3);

			Factory.Save();

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System);

			Factory.Save();

			BMBoardSection CreateSection(int row, int col)
			{
				var section = BMSTestHelper.CreateBoardSection(config.Buffer, board, row, col);
				var sectionConfig = section.SectionConfiguration;

				sectionConfig.ReleaseGroupPK = group.PK;
				sectionConfig.ChannelBy = ChannelTypeList.Codes.Resource;
				sectionConfig.SortPrimaryChannels = true;
				sectionConfig.OverrideChannels = false;

				return section;
			}

			var section1 = CreateSection(row: 0, col: 0);
			var section2 = CreateSection(row: 0, col: 1);

			Factory.Save();

			group.Staff.Add(resource2);
			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				form.EnterBoardMeetingMode();

				AssertExpandedChannel(form, section1, resource1);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section1, resource2);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section1, resource3);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section2, resource1);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section2, resource2);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section2, resource3);

				// Moving past the final channel should roll back to the start

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section1, resource1);

				// Going backwards should work too.

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section2, resource3);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section2, resource2);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section2, resource1);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section1, resource3);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section1, resource2);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section1, resource1);
			}
		}

		public void TestBoardMeetingMode_WithMultipleSections_ShouldIterateOverAllSections()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var board = BMSTestHelper.CreateBoard(config.System);

			BMBoardSection CreateSection(string sectionName, int row, int col)
			{
				var section = BMSTestHelper.CreateBoardSection(config.Buffer, board, row, col);

				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);
				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource3.PK);

				BMSTestHelper.SetOverriddenSectionName(section, sectionName);

				return section;
			}

			var section1 = CreateSection("Big Boy Top 1", row: 0, col: 0);
			var section2 = CreateSection("Big Boy Top 2", row: 0, col: 1);
			var section3 = CreateSection("Big Boy Bottom 1", row: 1, col: 0);
			var section4 = CreateSection("Big Boy Bottom 2", row: 1, col: 1);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				form.EnterBoardMeetingMode();

				AssertExpandedChannel(form, section1, resource1);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section1, resource2);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section1, resource3);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section2, resource1);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section2, resource2);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section2, resource3);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section3, resource1);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section3, resource2);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section3, resource3);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section4, resource1);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section4, resource2);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section4, resource3);

				// Should rollover back to the start
				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section1, resource1);

				// Going backwards should work too.
				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section4, resource3);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section4, resource2);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section4, resource1);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section3, resource3);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section3, resource2);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section3, resource1);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section2, resource3);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section2, resource2);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section2, resource1);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section1, resource3);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section1, resource2);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section1, resource1);
			}
		}

		public void TestBoardMeetingMode_WithMultipleSections_IncludingReleaseScheduler_ShouldSkipReleaseScheduler()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);

			var board = BMSTestHelper.CreateBoard(config.System);
			var section1 = BMSTestHelper.CreateBoardSection(config.Buffer, board);
			var section2 = BMSTestHelper.CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup, board);

			section2.Row = 1;

			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, config.CCR.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, config.NonCCR1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, config.NonCCR2.PK);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				form.EnterBoardMeetingMode();

				AssertExpandedChannel(form, section1, config.CCR);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section1, config.NonCCR1);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section1, config.NonCCR2);

				// Should rollover back to the first channel in this section since release schedulers aren't considered for board meeting mode.

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section1, config.CCR);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section1, config.NonCCR1);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section1, config.NonCCR2);

				// Going backwards should work too.

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section1, config.NonCCR1);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section1, config.CCR);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section1, config.NonCCR2);
			}
		}

		public void TestBoardMeetingMode_WithMultipleNonBufferSections_ShouldExpandBufferChannelsOnly()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);

			var board = BMSTestHelper.CreateBoard(config.System);
			var section1 = BMSTestHelper.CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup, board);
			var section2 = BMSTestHelper.CreateBoardSection(config.Bucket, board);
			var section3 = BMSTestHelper.CreateBoardSection(config.Buffer, board);

			section2.Row = 1;
			section3.Row = 2;

			void AddChannels(BMBoardSection section)
			{
				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.CCR.PK);
				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR1.PK);
				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, config.NonCCR2.PK);
			}

			AddChannels(section2);
			AddChannels(section3);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				form.EnterBoardMeetingMode();

				AssertExpandedChannel(form, section3, config.CCR);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section3, config.NonCCR1);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section3, config.NonCCR2);

				// Should rollover back to the first channel in this section since release schedulers aren't considered for board meeting mode.

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section3, config.CCR);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section3, config.NonCCR1);

				form.ShowNextChannel_ForTest();
				AssertExpandedChannel(form, section3, config.NonCCR2);

				// Going backwards should work too.

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section3, config.NonCCR1);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section3, config.CCR);

				form.ShowPreviousChannel_ForTest();
				AssertExpandedChannel(form, section3, config.NonCCR2);
			}
		}

		public void TestBoardMeetingMode_TasksFlyOver()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var buffer = CreateBuffer(system);
			var board = system.Boards.AddNew();

			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.Subsections = 1;
			section.SectionConfiguration.CellsPerSubsection = 13;

			for (int workflowsCount = 0; workflowsCount < 3; workflowsCount++)
			{
				var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
				var workflow = BMSTestHelper.CreateProcessHeaderAndTask(jobHeader, buffer, daysSinceRelease: 30, assignedResource: resource1.GS_Code);

				for (int tasksCount = 0; tasksCount < workflowsCount; tasksCount++)
				{
					BMSTestHelper.CreateTask(workflow, resource1.GS_Code, lowEstMinutes: 60);
				}

				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK, overrideChannels: true);
			}

			Factory.Save();

			var viewModel = GetViewModel(board);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var control = form.GetSectionControls().ToArray()[0] as BMComponentControl;
				AssertNotNull(control);

				var startingSection1Width = control.Table.ColumnStyles[2].Width;
				form.EnterBoardMeetingMode();
				Application.DoEvents();

				AssertEquals("First header of section should be expanded", startingSection1Width * 3, control.Table.ColumnStyles[2].Width);

				var boardMeetingForm = Application.OpenForms.OfType<BoardMeetingModeForm>().First();
				AssertNotNull(boardMeetingForm);

				boardMeetingForm.Focus();

				var taskPanel1 = form.FindAll<TaskPanel>().FirstOrDefault(t => t.TaskCards.Count() == 1); // First channel = First Workflow has only 1 task
				var totalTasksButton1 = taskPanel1.FindAll<ZButton>().Single();

				var taskPanel2 = form.FindAll<TaskPanel>().FirstOrDefault(t => t.TaskCards.Count() == 2); // Second channel = Second Workflow has only 2 tasks
				var totalTasksButton2 = taskPanel2.FindAll<ZButton>().Single();

				var mockRepository = new MockRepository(MockBehavior.Default);
				var taskPanel1Mock = mockRepository.Create<TaskPanel.IModifierKeysProvider>();

				taskPanel1Mock.Setup(m => m.ModifierKeys).Returns(Keys.Control);
				taskPanel1.ModifierKeysProvider = taskPanel1Mock.Object;

				var taskPanel2Mock = mockRepository.Create<TaskPanel.IModifierKeysProvider>();
				taskPanel2Mock.Setup(m => m.ModifierKeys).Returns(Keys.Control);
				taskPanel2.ModifierKeysProvider = taskPanel2Mock.Object;
				var cellTasksControls = form.Controls.OfType<CellTasksControl>();
				AssertEquals("Initially no tasks-fly-over should shown.", 0, cellTasksControls.Count());

				totalTasksButton1.PerformClick();

				cellTasksControls = form.Controls.OfType<CellTasksControl>();
				AssertEquals("GIVEN on meeting-mode WHEN CTRL + TotalTasksButton clicked SHOULD show 1 tasks-fly-over", 1, cellTasksControls.Count());

				totalTasksButton2.PerformClick();

				cellTasksControls = form.Controls.OfType<CellTasksControl>();
				AssertEquals("GIVEN on meeting-mode WHEN CTRL + 2x TotalTasksButton clicked SHOULD show 2 tasks-fly-over", 2, cellTasksControls.Count());

				boardMeetingForm.ShowNextChannel_ForTest();
				Application.DoEvents();
				form.Focus();

				cellTasksControls = form.Controls.OfType<CellTasksControl>();
				AssertEquals("GIVEN on meeting-mode and tasks-fly-over shown WHEN execute ShowNextChannel, all tasks-fly-over should be closed.", 0, cellTasksControls.Count());

				totalTasksButton1 = taskPanel1.FindAll<ZButton>().Single();
				totalTasksButton1.PerformClick();

				totalTasksButton2 = taskPanel2.FindAll<ZButton>().Single();
				totalTasksButton2.PerformClick();

				cellTasksControls = form.Controls.OfType<CellTasksControl>();
				AssertEquals("GIVEN on meeting-mode WHEN CTRL + 2x TotalTasksButton clicked SHOULD show 2 tasks-fly-over", 2, cellTasksControls.Count());

				boardMeetingForm.ShowPreviousChannel_ForTest();
				Application.DoEvents();
				form.Focus();

				cellTasksControls = form.Controls.OfType<CellTasksControl>();
				AssertEquals("GIVEN on meeting-mode and tasks-fly-over shown WHEN execute ShowPreviousChannel, all tasks-fly-over should be closed.", 0, cellTasksControls.Count());
			}
		}

		public void TestBoardMeetingMode_OpensBoardMeetingModeForm()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.FillWithValidTestData();
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			var section = BMSTestHelper.CreateBoardSection(bucket, board);
			Factory.Save();

			var viewModel = GetViewModel(board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.EnterBoardMeetingMode();
				AssertEquals(true, viewModel.CurrentBoardViewModel.FilterManager.IsApplied(new BoardMeetingModeFilter()));

				var boardMeetingForm = Application.OpenForms.OfType<BoardMeetingModeForm>().First();
				AssertNotNull(boardMeetingForm);
				AssertEquals(typeof(BoardMeetingModeForm), boardMeetingForm.GetType());
				AssertEquals(false, boardMeetingForm.IsDisposed);

				form.LeaveBoardMeetingMode();
				AssertEquals(false, viewModel.CurrentBoardViewModel.FilterManager.IsApplied(new BoardMeetingModeFilter()));
				AssertEquals(true, boardMeetingForm.IsDisposed);
			}
		}

		public void TestBoardMeetingMode_ClosingBoardMeetingModeForm_SingleBoard()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var board = BMSTestHelper.CreateBoard(system);
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			var section = BMSTestHelper.CreateBoardSection(bucket, board);
			Factory.Save();

			var viewModel = GetViewModel(board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.EnterBoardMeetingMode();
				AssertEquals(true, viewModel.CurrentBoardViewModel.FilterManager.IsApplied(new BoardMeetingModeFilter()));
				AssertEquals(true, form.IsInBoardMeeting);

				var boardMeetingForm = Application.OpenForms.OfType<BoardMeetingModeForm>().First();
				AssertNotNull(boardMeetingForm);
				AssertEquals(typeof(BoardMeetingModeForm), boardMeetingForm.GetType());
				AssertEquals(false, boardMeetingForm.IsDisposed);

				boardMeetingForm.Close();

				AssertEquals(true, boardMeetingForm.IsDisposed);
				AssertEquals(false, viewModel.CurrentBoardViewModel.FilterManager.IsApplied(new BoardMeetingModeFilter()));
				AssertEquals(false, form.IsInBoardMeeting);
			}
		}

		BMBoard CreateMultipleSectionBoardForBoardMeetingMode(bool firstBufferIsReleaseScheduler = false)
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer1 = BMSTestHelper.CreateBuffer(system, "Buffer1", sequence: 0);
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Buffer2", sequence: 1);
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket1", sequence: 2);
			var board = BMSTestHelper.CreateBoard(system);

			var section1 = BMSTestHelper.CreateBoardSection(buffer1, board, row: 0);
			var section2 = BMSTestHelper.CreateBoardSection(buffer2, board, row: 1);
			var section3 = BMSTestHelper.CreateBoardSection(bucket, board, row: 2);

			var sectionConfiguration1 = section1.SectionConfiguration;
			var sectionConfiguration2 = section2.SectionConfiguration;
			var sectionConfiguration3 = section3.SectionConfiguration;

			sectionConfiguration1.OverrideChannels = true;
			sectionConfiguration2.OverrideChannels = true;
			sectionConfiguration3.OverrideChannels = true;

			if (firstBufferIsReleaseScheduler)
			{
				sectionConfiguration1.IsReleaseScheduler = true;
			}

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);
			var channel3 = BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);
			var channel4 = BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);
			var channel5 = BMSTestHelper.CreatePrimaryChannelForSection(section3, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);
			var channel6 = BMSTestHelper.CreatePrimaryChannelForSection(section3, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);

			Factory.Save();
			return board;
		}

		BoardSlideshowViewModel CreateNoPreconstraintBoardForBoardMeetingMode(Orientation orientation)
		{
			var systemAndBuffer = BMSTestHelper.CreateSystemAndBuffer(Factory);
			systemAndBuffer.Item1.FS_Name = "WTGDEV";
			var board = BMSTestHelper.CreateBoard(systemAndBuffer.Item1, "My board", "Some description");

			var section = BMSTestHelper.CreateBoardSection(systemAndBuffer.Item2, board);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ShowZones = true;
			section.SectionConfiguration.Subsections = 1;
			section.SectionConfiguration.CellsPerSubsection = 8;
			if (orientation == Orientation.Horizontal)
			{
				section.SectionConfiguration.LastCell = LastCellList.Codes.Left;
				section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			}
			else
			{
				section.SectionConfiguration.LastCell = LastCellList.Codes.Top;
				section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Left;
			}

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, Factory.NewWithValidTestData<GlbStaff>().PK);

			Factory.Save();
			return GetViewModel(board);
		}

		BoardSlideshowViewModel CreatePreconstrainedBoardForBoardMeetingMode(Orientation orientation)
		{
			var systemAndBuffer = BMSTestHelper.CreateSystemAndBuffer(Factory);
			systemAndBuffer.Item1.FS_Name = "WTGDEV";
			var board = BMSTestHelper.CreateBoard(systemAndBuffer.Item1, "My board", "Some description");

			var section = BMSTestHelper.CreateBoardSection(systemAndBuffer.Item2, board);
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			var resourceCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CCR", "CCR Resource 1");
			var resourceNonCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NCC", "Non CCR Resource 1");

			var buffer = section.Component;
			var subBuffer = BMSTestHelper.CreateSubBuffer(buffer, timespanMinutes: 72 * 60, offsetMinutes: 0);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "constraint", offsetMinutes: 64 * 60);

			var group = BMSecurityTestHelper.CreateGroup(Factory, "GAA");
			var releaseGroup = CreateReleaseGroup(section.Board.System, group, constrainedModeComponent: buffer);
			group.Staff.AddRange(resourceCCR1, resourceNonCCR1);

			var flowDirection = orientation == Orientation.Vertical ? FlowDirectionList.Codes.Up : FlowDirectionList.Codes.Left;

			BMBoardSectionTestHelper.SetValuesForSectionConfiguration(section, releaseGroupPK: group.PK, cellsPerSubsection: 14, flowDirection: flowDirection, timeProgressionMode: TimeProgressionModeList.Codes.Age, timeField: TimeProgressionFieldList.Codes.TransferTime, maxOverdueSlots: 0);
			resourceCCR1.DesignateAsCCR(buffer);
			AssertNoErrors(section);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceCCR1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceNonCCR1.PK);
			Factory.Save();

			return GetViewModel(board);
		}

		public void TestBoardMeetingMode_ExpandChannelShortcutsWhenBoardMeetingFormNotInFocus()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var buffer = CreateBuffer(system);
			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.Subsections = 1;
			section.SectionConfiguration.CellsPerSubsection = 13;
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource3.PK, overrideChannels: true);

			Factory.Save();

			var viewModel = GetViewModel(board);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var control = form.GetSectionControls().ToArray()[0] as BMComponentControl;
				AssertNotNull(control);

				var startingSection1Width = control.Table.ColumnStyles[2].Width;
				form.EnterBoardMeetingMode();
				Application.DoEvents();

				AssertEquals("First header of section should be expanded", startingSection1Width * 3, control.Table.ColumnStyles[2].Width);

				var boardMeetingForm = Application.OpenForms.OfType<BoardMeetingModeForm>().First();
				AssertNotNull(boardMeetingForm);

				boardMeetingForm.Focus();
				boardMeetingForm.ShowNextChannel_ForTest();
				Application.DoEvents();

				AssertEquals("Navigation should continue", startingSection1Width * 3, control.Table.ColumnStyles[3].Width);

				form.Focus();
				BMSFormTestHelper.PressHotkeys(form, Keys.Control | Keys.Right);
				Application.DoEvents();

				AssertEquals("Navigation should continue", startingSection1Width * 3, control.Table.ColumnStyles[4].Width);

				BMSFormTestHelper.PressHotkeys(form, Keys.Control | Keys.Left);
				Application.DoEvents();

				AssertEquals("Navigation should continue", startingSection1Width * 3, control.Table.ColumnStyles[3].Width);
			}
		}

		public void TestBoardMeetingMode_MultipleSectionsHeaderExpanding()
		{
			var board = CreateMultipleSectionBoardForBoardMeetingMode();
			var viewModel = GetViewModel(board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();
				var channelHeaders = form.FindAll<ChannelHeaderControl>();
				AssertNotEquals("There should be channel headers", 0, channelHeaders.Count());

				var controls = form.GetSectionControls().ToArray();
				AssertEquals("Should be 3 controls", 3, controls.Length);

				var control = controls[0] as BMComponentControl;
				var startingWidth = control.Table.ColumnStyles[2].Width;
				var control2 = controls[1] as BMComponentControl;
				var control2StartingWidth = control2.Table.ColumnStyles[1].Width;
				var control3 = controls[2] as BMComponentControl;
				var control3StartingWidth = control3.Table.ColumnStyles[1].Width;

				form.EnterBoardMeetingMode();
				var boardMeetingForm = Application.OpenForms.OfType<BoardMeetingModeForm>().First();

				AssertEquals("First header of first section should be expanded", startingWidth * 3, control.Table.ColumnStyles[2].Width);
				AssertEquals("Second section should not be affected", control2StartingWidth, control2.Table.ColumnStyles[1].Width);
				AssertEquals("Bucket section should not be affected", control3StartingWidth, control3.Table.ColumnStyles[1].Width);

				boardMeetingForm.Close();
			}
		}

		public void TestBoardMeetingMode_ZoneAndSectionHeadersCollapseAfterBoardMeetingEnded_VerticalOrientation()
		{
			var viewModel = CreatePreconstrainedBoardForBoardMeetingMode(Orientation.Vertical);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var control = form.GetSectionControls().ToArray()[0] as BMComponentControl;
				AssertNotNull(control);

				var zone0rowStartingHeight = control.Table.RowStyles[1].Height;
				var zone1rowStartingHeight = control.Table.RowStyles[3].Height;
				var zone2rowStartingHeight = control.Table.RowStyles[7].Height;
				var zone2row2StartingHeight = control.Table.RowStyles[8].Height;
				var zone2row3StartingHeight = control.Table.RowStyles[9].Height;
				var zone2row4StartingHeight = control.Table.RowStyles[10].Height;
				var startingSection1Width = control.Table.ColumnStyles[2].Width;

				form.EnterBoardMeetingMode();
				Application.DoEvents();

				AssertEquals("Zone 0 expanded", zone0rowStartingHeight * 3, control.Table.RowStyles[1].Height);
				AssertEquals("Zone 1 expanded", zone1rowStartingHeight * 3, control.Table.RowStyles[3].Height);
				AssertEquals("Zone 2 first row expanded", zone2rowStartingHeight * 3, control.Table.RowStyles[7].Height);
				AssertEquals("Zone 2 second row expanded", zone2row2StartingHeight * 3, control.Table.RowStyles[8].Height);
				AssertEquals("Zone 2 third row remains the same", zone2row3StartingHeight, control.Table.RowStyles[9].Height);
				AssertEquals("Zone 2 fourth row still same", zone2row4StartingHeight, control.Table.RowStyles[10].Height);
				AssertEquals("First header of section expanded", startingSection1Width * 3, control.Table.ColumnStyles[2].Width);

				form.ShowNextChannel_ForTest();
				Application.DoEvents();

				AssertEquals("First header of section collapsed", startingSection1Width, control.Table.ColumnStyles[2].Width);
				AssertEquals("Second header of section expanded", startingSection1Width * 3, control.Table.ColumnStyles[5].Width);

				form.LeaveBoardMeetingMode();
				Application.DoEvents();

				AssertEquals("Zone 0 collapsed", zone0rowStartingHeight, control.Table.RowStyles[1].Height);
				AssertEquals("Zone 1 collapsed", zone1rowStartingHeight, control.Table.RowStyles[3].Height);
				AssertEquals("Zone 2 first row collapsed", zone2rowStartingHeight, control.Table.RowStyles[7].Height);
				AssertEquals("Zone 2 second row collapsed", zone2row2StartingHeight, control.Table.RowStyles[8].Height);
				AssertEquals("Zone 2 third row collapsed", zone2row3StartingHeight, control.Table.RowStyles[9].Height);
				AssertEquals("Zone 2 fourth row still same", zone2row4StartingHeight, control.Table.RowStyles[10].Height);
				AssertEquals("First header of section collapsed", startingSection1Width, control.Table.ColumnStyles[2].Width);
			}
		}

		public void TestBoardMeetingMode_ZoneAndSectionHeadersCollapseAfterBoardMeetingEnded_HorizontalOrientation()
		{
			var viewModel = CreatePreconstrainedBoardForBoardMeetingMode(Orientation.Horizontal);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var control = form.GetSectionControls().ToArray()[0] as BMComponentControl;
				AssertNotNull(control);

				var zone0ColumnStartingWidth = control.Table.ColumnStyles[1].Width;
				var zone1ColumnStartingWidth = control.Table.ColumnStyles[4].Width;
				var zone2ColumnStartingWidth = control.Table.ColumnStyles[7].Width;
				var zone2Column2StartingWidth = control.Table.ColumnStyles[8].Width;
				var zone2Column3StartingWidth = control.Table.ColumnStyles[9].Width;
				var startingSection1Height = control.Table.RowStyles[2].Height;

				form.EnterBoardMeetingMode();
				Application.DoEvents();

				AssertEquals("Zone 0 expanded", zone0ColumnStartingWidth * 3, control.Table.ColumnStyles[1].Width);
				AssertEquals("Zone 1 expanded", zone1ColumnStartingWidth * 3, control.Table.ColumnStyles[4].Width);
				AssertEquals("Zone 2 first column expanded", zone2ColumnStartingWidth * 3, control.Table.ColumnStyles[7].Width);
				AssertEquals("Zone 2 second column expanded", zone2Column2StartingWidth * 3, control.Table.ColumnStyles[8].Width);
				AssertEquals("Zone 2 third column remained", zone2Column3StartingWidth, control.Table.ColumnStyles[9].Width);
				AssertEquals("First header of section expanded", startingSection1Height * 3, control.Table.RowStyles[2].Height);

				form.ShowNextChannel_ForTest();
				Application.DoEvents();

				AssertEquals("First header of section collapsed", startingSection1Height, control.Table.RowStyles[2].Height);
				AssertEquals("Second header of section expanded", startingSection1Height * 3, control.Table.RowStyles[5].Height);

				form.LeaveBoardMeetingMode();
				Application.DoEvents();

				AssertEquals("Zone 0 collapsed", zone0ColumnStartingWidth, control.Table.ColumnStyles[1].Width);
				AssertEquals("Zone 1 collapsed", zone1ColumnStartingWidth, control.Table.ColumnStyles[4].Width);
				AssertEquals("Zone 2 first column collapsed", zone2ColumnStartingWidth, control.Table.ColumnStyles[7].Width);
				AssertEquals("Zone 2 second column remained", zone2Column2StartingWidth, control.Table.ColumnStyles[8].Width);
				AssertEquals("Zone 2 third column remained", zone2Column3StartingWidth, control.Table.ColumnStyles[9].Width);
				AssertEquals("First header of section collapsed", startingSection1Height, control.Table.RowStyles[2].Height);
			}
		}

		public void TestBoardMeetingMode_ExpandsZonesHorizontalNoPreconstraint()
		{
			var viewModel = CreateNoPreconstraintBoardForBoardMeetingMode(Orientation.Horizontal);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				Application.DoEvents();
				var channelHeaders = form.FindAll<ChannelHeaderControl>();
				AssertNotEquals("There should be channel headers", 0, channelHeaders.Count());

				var controls = form.GetSectionControls().ToArray();
				AssertEquals("Should be 1 control", 1, controls.Length);

				var sectionControl = controls[0] as BMComponentControl;
				AssertNotNull(sectionControl);

				var zone0rowStartingHeight = sectionControl.Table.RowStyles[1].Height;
				var zone1rowStartingHeight = sectionControl.Table.RowStyles[3].Height;
				var zone2rowStartingHeight = sectionControl.Table.RowStyles[5].Height;
				var zone2row2StartingHeight = sectionControl.Table.RowStyles[6].Height;

				form.EnterBoardMeetingMode();
				Application.DoEvents();

				AssertEquals("Zone 0 expanded", zone0rowStartingHeight * 3, sectionControl.Table.RowStyles[1].Height);
				AssertEquals("Zone 1 expanded", zone1rowStartingHeight * 3, sectionControl.Table.RowStyles[3].Height);
				AssertEquals("Zone 2 top row expanded", zone2rowStartingHeight * 3, sectionControl.Table.RowStyles[5].Height);
				AssertEquals("Zone 2 bottom row not expanded", zone2row2StartingHeight, sectionControl.Table.RowStyles[6].Height);
			}
		}

		public void TestBoardMeetingMode_ExpandsZonesVerticalNoPreconstraint()
		{
			var viewModel = CreateNoPreconstraintBoardForBoardMeetingMode(Orientation.Vertical);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				Application.DoEvents();
				var channelHeaders = form.FindAll<ChannelHeaderControl>();
				AssertNotEquals("There should be channel headers", 0, channelHeaders.Count());

				var controls = form.GetSectionControls().ToArray();
				AssertEquals("Should be 1 control", 1, controls.Length);

				var sectionControl = controls[0] as BMComponentControl;
				AssertNotNull(sectionControl);

				var zone0rowStartingWidth = sectionControl.Table.ColumnStyles[1].Width;
				var zone1rowStartingWidth = sectionControl.Table.ColumnStyles[3].Width;
				var zone2rowStartingWidth = sectionControl.Table.ColumnStyles[5].Width;
				var zone2row2StartingWidth = sectionControl.Table.ColumnStyles[6].Width;

				form.EnterBoardMeetingMode();
				Application.DoEvents();

				AssertEquals("Zone 0 expanded", zone0rowStartingWidth * 3, sectionControl.Table.ColumnStyles[1].Width);
				AssertEquals("Zone 1 expanded", zone1rowStartingWidth * 3, sectionControl.Table.ColumnStyles[3].Width);
				AssertEquals("Zone 2 first column expanded", zone2rowStartingWidth * 3, sectionControl.Table.ColumnStyles[5].Width);
				AssertEquals("Zone 2 second column not expanded", zone2row2StartingWidth, sectionControl.Table.ColumnStyles[6].Width);
			}
		}

		public void TestBoardMeetingMode_ExpandsZonesVerticalWithPreconstraint()
		{
			var viewModel = CreatePreconstrainedBoardForBoardMeetingMode(Orientation.Vertical);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				Application.DoEvents();
				var channelHeaders = form.FindAll<ChannelHeaderControl>();
				AssertNotEquals("There should be channel headers", 0, channelHeaders.Count());

				var controls = form.GetSectionControls().ToArray();
				AssertEquals("Should be 1 control", 1, controls.Length);

				var sectionControl = controls[0] as BMComponentControl;
				AssertNotNull(sectionControl);

				var zone0rowStartingHeight = sectionControl.Table.RowStyles[1].Height;
				var zone1rowStartingHeight = sectionControl.Table.RowStyles[3].Height;
				var zone2rowStartingHeight = sectionControl.Table.RowStyles[7].Height;
				var zone2row2StartingHeight = sectionControl.Table.RowStyles[8].Height;
				var zone2row3StartingHeight = sectionControl.Table.RowStyles[9].Height;
				var zone2row4StartingHeight = sectionControl.Table.RowStyles[10].Height;

				form.EnterBoardMeetingMode();

				AssertEquals("Zone 0 expanded", zone0rowStartingHeight * 3, sectionControl.Table.RowStyles[1].Height);
				AssertEquals("Zone 1 expanded", zone1rowStartingHeight * 3, sectionControl.Table.RowStyles[3].Height);
				AssertEquals("Zone 2 first row expanded", zone2rowStartingHeight * 3, sectionControl.Table.RowStyles[7].Height);
				AssertEquals("Zone 2 second row expanded", zone2row2StartingHeight * 3, sectionControl.Table.RowStyles[8].Height);
				AssertEquals("Zone 2 third row stays the same", zone2row3StartingHeight, sectionControl.Table.RowStyles[9].Height);
				AssertEquals("Zone 2 fourth row not expanded", zone2row4StartingHeight, sectionControl.Table.RowStyles[10].Height);
			}
		}

		public void TestBoardMeetingMode_ExpandsZonesHorizontalWithPreconstraint()
		{
			var viewModel = CreatePreconstrainedBoardForBoardMeetingMode(Orientation.Horizontal);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				Application.DoEvents();
				var channelHeaders = form.FindAll<ChannelHeaderControl>();
				AssertNotEquals("There should be channel headers", 0, channelHeaders.Count());

				var controls = form.GetSectionControls().ToArray();
				AssertEquals("Should be 1 control", 1, controls.Length);

				var sectionControl = controls[0] as BMComponentControl;
				AssertNotNull(sectionControl);

				var zone0rowStartingWidth = sectionControl.Table.ColumnStyles[1].Width;
				var zone1rowStartingWidth = sectionControl.Table.ColumnStyles[3].Width;
				var zone2rowStartingWidth = sectionControl.Table.ColumnStyles[7].Width;
				var zone2row2StartingWidth = sectionControl.Table.ColumnStyles[8].Width;
				var zone2row3StartingWidth = sectionControl.Table.ColumnStyles[9].Width;
				var zone2row4StartingWidth = sectionControl.Table.ColumnStyles[10].Width;

				form.EnterBoardMeetingMode();
				Application.DoEvents();

				AssertEquals("Zone 0 expanded", zone0rowStartingWidth * 3, sectionControl.Table.ColumnStyles[1].Width);
				AssertEquals("Zone 1 expanded", zone1rowStartingWidth * 3, sectionControl.Table.ColumnStyles[3].Width);
				AssertEquals("Zone 2 first column expanded", zone2rowStartingWidth * 3, sectionControl.Table.ColumnStyles[7].Width);
				AssertEquals("Zone 2 second column expanded", zone2row2StartingWidth * 3, sectionControl.Table.ColumnStyles[8].Width);
				AssertEquals("Zone 2 third column not expanded", zone2row3StartingWidth, sectionControl.Table.ColumnStyles[9].Width);
				AssertEquals("Zone 2 second column not expanded", zone2row4StartingWidth, sectionControl.Table.ColumnStyles[10].Width);
			}
		}

		public void TestBoardMeetingMode_ClosingBoardMeetingModeForm_SlideShow()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board, 10), Tuple.Create(board, 10));
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			Factory.Save();

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.EnterBoardMeetingMode();
				AssertEquals(true, viewModel.CurrentBoardViewModel.FilterManager.IsApplied(new BoardMeetingModeFilter()));
				AssertEquals(true, form.IsInBoardMeeting);
				AssertEquals(false, form.IsSlideshowRunning);

				var boardMeetingForm = Application.OpenForms.OfType<BoardMeetingModeForm>().First();
				AssertNotNull(boardMeetingForm);
				AssertEquals(typeof(BoardMeetingModeForm), boardMeetingForm.GetType());
				AssertEquals(false, boardMeetingForm.IsDisposed);

				boardMeetingForm.Close();

				AssertEquals(true, boardMeetingForm.IsDisposed);
				AssertEquals(false, viewModel.CurrentBoardViewModel.FilterManager.IsApplied(new BoardMeetingModeFilter()));
				AssertEquals(false, form.IsInBoardMeeting);
				AssertEquals(true, form.IsSlideshowRunning);
			}
		}

		public void TestBoardMeetingMode_InBoardMeetingMode_RefreshButtonClicked()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board, 10), Tuple.Create(board, 10));
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			Factory.Save();

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				form.controlsPanel.Expand();
				form.EnterBoardMeetingMode();
				AssertEquals(true, form.IsInBoardMeeting);

				var refreshWasStarted = false;
				form.RefreshStarted += (sender, args) =>
				{
					refreshWasStarted = true;
				};

				form.RefreshButton.PerformClick();
				Application.DoEvents();

				AssertEquals(true, refreshWasStarted);
				AssertEquals(true, form.IsInBoardMeeting);

				form.LeaveBoardMeetingMode();
				refreshWasStarted = false;
				form.controlsPanel.Expand();
				form.RefreshButton.PerformClick();
				Application.DoEvents();

				AssertEquals(true, refreshWasStarted);
				AssertEquals(false, form.IsInBoardMeeting);
			}
		}

		public void TestBoardMeetingMode_InBoardMeetingMode_RefreshButtonClickedAfterDisposed()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board, 10), Tuple.Create(board, 10));
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			Factory.Save();

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.EnterBoardMeetingMode();
				AssertEquals(true, form.IsInBoardMeeting);

				var refreshWasStarted = false;
				form.RefreshStarted += (sender, args) =>
				{
					refreshWasStarted = true;
				};
				form.controlsPanel.Expand();
				form.RefreshButton.PerformClick();
				Application.DoEvents();

				AssertEquals(true, refreshWasStarted);
				AssertEquals(true, form.IsInBoardMeeting);

				form.Dispose();
				form.controlsPanel.Expand();
				form.RefreshButton.PerformClick();

				AssertEquals(true, form.IsDisposed);
				AssertEquals(true, form.IsInBoardMeeting);
			}
		}

		public void TestBoardMeetingMode_AutoRefreshSuppressed()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board, 10), Tuple.Create(board, 10));

			Factory.Save();

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);
			viewModel.RefreshSeconds = 5;

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.EnterBoardMeetingMode();
				AssertEquals(true, form.IsInBoardMeeting);

				var refreshWasStarted = false;
				form.RefreshStarted += (sender, args) =>
				{
					refreshWasStarted = true;
				};

				AssertEquals(false, refreshWasStarted);
				AssertEquals(true, form.IsInBoardMeeting);
			}
		}

		public void TestBoardMeetingMode_IsInBoardMeeting_SingleBoard()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.FillWithValidTestData();
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			var section = BMSTestHelper.CreateBoardSection(bucket, board);
			Factory.Save();

			var viewModel = GetViewModel(board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.EnterBoardMeetingMode();
				AssertEquals(true, form.IsInBoardMeeting);

				form.LeaveBoardMeetingMode();
				AssertEquals(false, form.IsInBoardMeeting);
			}
		}

		public void TestBoardMeetingMode_IsInBoardMeeting_SlideShow()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board, 10), Tuple.Create(board, 10));
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			Factory.Save();

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.EnterBoardMeetingMode();
				AssertEquals(true, form.IsInBoardMeeting);
				AssertEquals(false, form.IsSlideshowRunning);

				form.LeaveBoardMeetingMode();
				AssertEquals(false, form.IsInBoardMeeting);
				AssertEquals(true, form.IsSlideshowRunning);
			}
		}

		public void TestBoardMeetingMode_KeepsOtherFilters()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.FillWithValidTestData();
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			BMSTestHelper.CreateBoardSection(bucket, board);
			Factory.Save();

			var viewModel = GetViewModel(board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				var currentTaskFilter = new CurrentTaskFilter();

				AssertEquals(0, form.BoardViewModel.FilterManager.AppliedFilters.Count());

				form.BoardViewModel.FilterManager.ApplyFilter(new CurrentTaskFilter());

				foreach (var section in form.BoardViewModel.GetSections())
				{
					section.FilterManager.ApplyFilter(new CurrentTaskFilter());
				}

				Assert("Current task filter is applied to the board.", form.BoardViewModel.FilterManager.AppliedAndInheritedFilters.Any(f => f.Equals(currentTaskFilter)));
				Assert("Current task filter is applied to all of the sections too.", form.BoardViewModel.GetSections().All(s => s.FilterManager.AppliedAndInheritedFilters.Any(f => f.Equals(currentTaskFilter))));

				form.EnterBoardMeetingMode();

				Assert("Current task filter is still applied to board.", form.BoardViewModel.FilterManager.AppliedAndInheritedFilters.Any(f => f.Equals(currentTaskFilter)));
				Assert("Current task filter is still applied to all sections.", form.BoardViewModel.GetSections().All(s => s.FilterManager.AppliedAndInheritedFilters.Any(f => f.Equals(currentTaskFilter))));
			}
		}

		public void TestBoardMeetingMode_ClickResumeStopsBoardMeetingMode()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board, 10), Tuple.Create(board, 10));
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			Factory.Save();

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.EnterBoardMeetingMode();
				AssertEquals(true, form.IsInBoardMeeting);
				AssertEquals(false, form.IsSlideshowRunning);

				form.controlsPanel.Expand();
				var resumeButton = form.controlsPanel.PauseResumeButton;
				resumeButton.PerformClick();

				AssertEquals(false, form.IsInBoardMeeting);
				AssertEquals(true, form.IsSlideshowRunning);
			}
		}

		public void TestBoardMeetingMode_EnableBoardMovementButtons()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board, 10), Tuple.Create(board, 10));
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			Factory.Save();

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.EnterBoardMeetingMode();

				AssertEquals(false, form.IsSlideshowRunning);

				form.VisualBoardForm_RefreshFinished(board, BoardRefreshEventArgs.Empty);

				AssertEquals(false, form.IsSlideshowRunning);
			}
		}

		public void TestBoardMeetingMode_BoardMovementButtons_ExitsBoardMeetingModeAndStartsSlideShow()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board, 10), Tuple.Create(board, 10));
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			Factory.Save();

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.EnterBoardMeetingMode();

				AssertEquals(false, form.IsSlideshowRunning);

				MoveNext_ForTest(form);
				AssertEquals(true, form.IsSlideshowRunning);
				AssertEquals(false, form.IsInBoardMeeting);

				MoveNext_ForTest(form);
				AssertEquals(true, form.IsSlideshowRunning);
				AssertEquals(false, form.IsInBoardMeeting);
			}
		}

		public void TestBoardMeetingMode_AfterMovement_PauseOnOriginalSlide()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board, 10), Tuple.Create(board, 10));
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			Factory.Save();

			var triggerable = new TriggerableAsyncStrategy();
			using (ApplyAsyncStrategy(triggerable))
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.EnterBoardMeetingMode();

				AssertEquals(false, form.IsSlideshowRunning);

				MoveNext_ForTest(form);
				AssertEquals(true, form.IsSlideshowRunning);

				MoveNext_ForTest(form);
				AssertEquals(true, form.IsSlideshowRunning);
				form.controlsPanel.Expand();
				form.controlsPanel.PauseResumeButton.Enabled = true;
				form.controlsPanel.PauseResumeButton.PerformClick();
				DoAsyncSynchronously_ForTest(triggerable);
				form.RefreshNow_ForTest(true);
				AssertEquals(false, form.IsSlideshowRunning);
				AssertEquals(false, form.IsInBoardMeeting);
				triggerable.DoAllActions();
			}
		}

		public void TestBoardMeetingMode_AfterMovement_PauseOnSecondSlide()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board, 10), Tuple.Create(board, 10));
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			Factory.Save();

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.EnterBoardMeetingMode();

				AssertEquals(false, form.IsSlideshowRunning);

				MoveNext_ForTest(form);
				AssertEquals(true, form.IsSlideshowRunning);
				form.controlsPanel.Expand();
				form.controlsPanel.PauseResumeButton.PerformClick();
				AssertEquals(false, form.IsSlideshowRunning);
				AssertEquals(false, form.IsInBoardMeeting);
			}
		}

		public void TestBoardMeetingMode_FilterManager_CorrectReferenceHeld()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board, 10), Tuple.Create(board, 10));
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			Factory.Save();

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				var firstSlideShowFilterManager = form.SlideShowViewModel.FilterManager;
				var firstFilterManager = form.BoardViewModel.FilterManager;

				MoveNext_ForTest(form);

				var secondSlideShowFilterManager = form.SlideShowViewModel.FilterManager;
				var secondFilterManager = form.BoardViewModel.FilterManager;

				AssertEquals(secondSlideShowFilterManager, firstSlideShowFilterManager);
				AssertNotEquals(firstFilterManager, secondFilterManager);
			}
		}

		public void TestBoardMeetingMode_FilterManager()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board, 10), Tuple.Create(board, 10));
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			Factory.Save();

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.EnterBoardMeetingMode();
				AssertEquals(true, form.SlideShowViewModel.FilterManager.IsApplied(new BoardMeetingModeFilter()));
				AssertEquals(1, form.SlideShowViewModel.FilterManager.AppliedFilters.Count());
				AssertEquals(true, form.IsInBoardMeeting);

				MoveNext_ForTest(form);

				AssertEquals(false, form.IsInBoardMeeting);
				AssertEquals(0, form.SlideShowViewModel.FilterManager.AppliedFilters.Count());

				MoveNext_ForTest(form);

				AssertEquals(false, form.IsInBoardMeeting);
				AssertEquals(0, form.SlideShowViewModel.FilterManager.AppliedFilters.Count());
			}
		}

		#endregion

		#region SlideShow

		public void TestSlideshow_GivenOneBoard_WhenClosingConfiguration_ShouldNotNotError()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board1 = BMSTestHelper.CreateBoard(system, "Board 1");
			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1);

			Factory.Save();

			var viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(slideshow);
			AssertEquals("Precondition: GIVEN 1 board in slideshow", 1, viewModel.BoardViewModels.Length);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				form.controlsPanel.Expand();
				form.controlsPanel.ConfigButton.PerformClick();
				Application.DoEvents();

				var configurationForm = (ZForm)ZFormModaliser.LastFormShownForTest;
				AssertNotNull(configurationForm);
				AssertEquals(typeof(BMBoardSlideshowForm).FullName, configurationForm.GetType().ToString());
				AssertNoExceptionThrown("WHEN closing configuration form (BMBoardSlideshowForm) THEN should not error (InvalidCastException)", () =>
				{
					configurationForm.Close();
				});
			}
		}

		public void TestSlideshowProgression_WhenMoreTicketsThanAllowedForSlideCache_WithBoardMeetingMode_ShouldNotRetainControls()
		{
			BMSRegistry.Instance.MaxNumberOfItemsAllowedToCacheBoardInSlideShow.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, systemName: "Destiny");
			BMSTestHelper.CreateTinyTicketLayout(Factory, config.System, CustomisedControlTypeList.Codes.TaskCard);

			var board1 = BMSTestHelper.CreateBoard(config.System, "Air");
			var board2 = BMSTestHelper.CreateBoard(config.System, "Darkness");
			var board3 = BMSTestHelper.CreateBoard(config.System, "Light");
			var board4 = BMSTestHelper.CreateBoard(config.System, "Water");

			var section1 = BMSTestHelper.CreateBoardSection(config.Buffer, board1, backColor: Color.Red.Name, panelLayoutStyle: PanelLayoutTypeList.Codes.Staggered, cellsPerSubsection: 4);
			var section2 = BMSTestHelper.CreateBoardSection(config.Buffer, board2, backColor: Color.Green.Name, panelLayoutStyle: PanelLayoutTypeList.Codes.Staggered, cellsPerSubsection: 4);
			var section3 = BMSTestHelper.CreateBoardSection(config.Buffer, board3, backColor: Color.Blue.Name, panelLayoutStyle: PanelLayoutTypeList.Codes.Staggered, cellsPerSubsection: 4);
			var section4 = BMSTestHelper.CreateBoardSection(config.Buffer, board4, backColor: Color.Yellow.Name, panelLayoutStyle: PanelLayoutTypeList.Codes.Staggered, cellsPerSubsection: 4);

			for (var i = 0; i <= BMSRegistry.Instance.MaxNumberOfItemsAllowedToCacheBoardInSlideShow.Value; i++)
			{
				var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Stargate Universe", config.Buffer);
				BMSTestHelper.CreateTask(workflow, description: "The best show ever");
			}

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2, board3, board4);

			Factory.Save();

			var controlReferences = new List<WeakReference>();

			using (var form = GetAndShowVisualBoardForm(slideshow))
			{
				form.Size = new Size(1280, 1024);
				Application.DoEvents();

				CombineAssertions(() =>
				{
					for (var slideIndex = 0; slideIndex < slideshow.BoardPivots.Count; slideIndex++)
					{
						CollectReferenceAndAssertActiveControls(form, controlReferences, slideshow, slideIndex);
					}
				});
				CombineAssertions(() =>
				{
					for (int slideIndex = 0; slideIndex < slideshow.BoardPivots.Count * 2; slideIndex++)
					{
						var shownControl = form.FindSingleOrDefault<BMComponentControl>();
						AssertNotNull("Control should not be null", shownControl);
						AssertEquals("Control should not be disposed", shownControl.IsDisposed, false);
						MoveNext_ForTest(form);
					}
				});
			}
		}

		public void TestBoardSlides_ItemsInCache_ShouldNotBeMoreThenMaxNumberOfBoardsAllowedToCacheInSlideShow()
		{
			BMSRegistry.Instance.MaxNumberOfBoardsAllowedToCacheInSlideShow.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "Dat System";
			var bucket = BMSTestHelper.CreateBucket(system, "bucket", offsetMinutes: 0, sequence: 0);
			var listOfBoards = new List<Tuple<BMBoard, int>>(5);

			for (var slideIndex = 0; slideIndex < 5; slideIndex++)
			{
				var boardName = "board" + slideIndex.ToString();
				var board = BMSTestHelper.CreateBoard(system, boardName, "something for test");
				BMSTestHelper.CreateBoardSection(bucket, board);
				listOfBoards.Add(Tuple.Create(board, 60));
			}

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, listOfBoards.ToArray());

			Factory.Save();

			var reloadedSlideShow = new BusinessObjectFactory().Load<BMBoardSlideshow>(slideshow.PK);

			AssertNotNull("Precondition", reloadedSlideShow);

			var viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(reloadedSlideShow);
			var controlReferences = new List<WeakReference>();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				CombineAssertions(() =>
				{
					for (int slideIndex = 0; slideIndex < slideshow.BoardPivots.Count; slideIndex++)
					{
						CollectReferenceAssertActiveControlsCountLessOrEqualAndMoveNext(form, controlReferences, slideshow, slideIndex, 4); //Sometimes it keeps one more reference to report usage
					}
				});
				CombineAssertions(() =>
				{
					for (int slideIndex = 0; slideIndex < slideshow.BoardPivots.Count * 2; slideIndex++)
					{
						var shownControl = form.FindSingleOrDefault<BMComponentControl>();
						AssertNotNull("Control should not be null", shownControl);
						AssertEquals("Control should not be disposed", shownControl.IsDisposed, false);
						MoveNext_ForTest(form);
					}
				});
				CombineAssertions(() =>
				{
					for (int slideIndex = 0; slideIndex < slideshow.BoardPivots.Count * 2; slideIndex++)
					{
						var shownControl = form.FindSingleOrDefault<BMComponentControl>();
						AssertNotNull("Control should not be null", shownControl);
						AssertEquals("Control should not be disposed", shownControl.IsDisposed, false);
						MovePrevious_ForTest(form);
					}
				});
			}
		}

		static void CollectReferenceAssertActiveControlsCountLessOrEqualAndMoveNext(VisualBoardForm form, List<WeakReference> controlReferences, BMBoardSlideshow slideshow, int slideIndex, int expected)
		{
			// Doing this in a separate function so the shownControl instance variable doesn't keep it in memory due to debug build.
			var shownControl = form.FindSingle<BMComponentControl>();

			GC.Collect();
			GC.WaitForFullGCComplete();

			AssertEquals("Pre-condition: control References", slideIndex, controlReferences.Count);
			AssertEquals(slideshow.BoardPivots[slideIndex].Board.MB_Name, shownControl.ViewModel.BoardViewModel.BoardName);

			var activeWeakReferences = controlReferences.Count(r =>
			{
				var targetObject = r.Target;
				return ((targetObject != null) && (targetObject is BMComponentControl));
			});
			AssertLessThanOrEqualTo("Controls retained on memory", activeWeakReferences, expected);

			controlReferences.Add(new WeakReference(shownControl));

			MoveNext_ForTest(form);
			Application.DoEvents();
		}

		static void CollectReferenceAndAssertActiveControls(VisualBoardForm form, List<WeakReference> controlReferences, BMBoardSlideshow slideshow, int slideIndex)
		{
			// Doing this in a separate function so the shownControl instance variable doesn't keep it in memory due to debug build.

			var shownControl = form.FindSingle<BMComponentControl>();

			AssertEquals("Pre-condition: the test is written correctly...", slideIndex, controlReferences.Count);
			AssertEquals(slideshow.BoardPivots[slideIndex].Board.MB_Name, shownControl.ViewModel.BoardViewModel.BoardName);

			GC.Collect();
			GC.WaitForFullGCComplete();

			var activeWeakReferences = controlReferences.Select(r => r.Target).ToArray().WhereNotNull().Cast<BMComponentControl>().Select(c => c.ViewModel.BoardViewModel.BoardName).ToArray();

			AssertContainsExactElementsInAnyOrder("Controls retained on iteration " + slideIndex, Array.Empty<string>(), activeWeakReferences);

			controlReferences.Add(new WeakReference(shownControl));

			form.EnterBoardMeetingMode();
			form.LeaveBoardMeetingMode();

			MoveNext_ForTest(form);
			Application.DoEvents();
		}

		[TestDate(2015, 7, 14)]
		public void TestSlideShowProgression_WhenMoreTicketsThanAllowedForSlideCache_ShouldUpdateAcceptabilityBandsAndChannelHeadings()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 1, 2, 3, 4, 5, "Number of Workflows", type: AcceptabilityBandTypes.Codes.Count);
			band.BAB_FiltersBySection = true; //when not filtered by section, a section filter is not created and there will be one hit less; let's assume the pessimistic scenario

			// Ensures all tickets fit on the form without resizing
			var layout = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard, 10, 10);
			BMSTestHelper.CreateControlCustomisationLink(Factory, config.System, layout);

			var board1 = BMSTestHelper.CreateBoard(config.System, "Caretaker");
			var board2 = BMSTestHelper.CreateBoard(config.System, "Parallax");
			var board3 = BMSTestHelper.CreateBoard(config.System, "Time and Again");
			var section1 = BMSTestHelper.CreateBoardSection(config.Buffer, board1, backColor: Color.Red.Name, panelLayoutStyle: PanelLayoutTypeList.Codes.Stacked);
			var section2 = BMSTestHelper.CreateBoardSection(config.Buffer, board2, backColor: Color.Green.Name, panelLayoutStyle: PanelLayoutTypeList.Codes.Stacked);
			var section3 = BMSTestHelper.CreateBoardSection(config.Buffer, board3, backColor: Color.Blue.Name, panelLayoutStyle: PanelLayoutTypeList.Codes.Stacked);

			BMSTestHelper.AddAcceptabilityBandToSection(section1, band);
			BMSTestHelper.AddAcceptabilityBandToSection(section2, band);
			BMSTestHelper.AddAcceptabilityBandToSection(section3, band);

			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, resource.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, resource.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section3, ChannelTypeList.Codes.Resource, resource.PK);

			FilterStripsTestHelper.AddCustomSQLFilterStrip(section1.SectionConfiguration.WorkflowFilter, "69=69");
			FilterStripsTestHelper.AddCustomSQLFilterStrip(section2.SectionConfiguration.WorkflowFilter, "69=69");
			FilterStripsTestHelper.AddCustomSQLFilterStrip(section3.SectionConfiguration.WorkflowFilter, "69=69");

			FilterStripsTestHelper.AddCustomSQLFilterStrip(band.FilterRule, "68=68");

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2, board3);

			BMSRegistry.Instance.MaxNumberOfItemsAllowedToCacheBoardInSlideShow.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);

			var numberOfTasks = BMSRegistry.Instance.MaxNumberOfItemsAllowedToCacheBoardInSlideShow.Value + 10;
			BMSTestHelper.CreateWorkflows(config.Buffer, numberOfWorkflows: 1, numberOfTasksPerWorkflow: numberOfTasks, staff: resource);

			Factory.Save();

			using (KUserControl.TrackInstantiatedControls_ForTest())
			using (Db.Connection.TrackExecutedCommands())
			using (var form = GetAndShowVisualBoardForm(slideshow))
			{
				var sectionControlsCreated = 1;
				var sectionQueriesExecuted = 2; // Extra one due to need for validation of section upon initial load
				var bandQueriesExecuted = 1;
				var ticketControlsCreated = numberOfTasks + 1; // Extra one at each refresh for the 'template' ticket created during rendering process.

				AssertChannelHeadingAndAcceptabilityBandTileDrawnCorrectly("First slide shown should be drawn correctly", form, sectionControlsCreated, sectionQueriesExecuted, bandQueriesExecuted, ticketControlsCreated);

				for (var i = 1; i < slideshow.BoardPivots.Count; i++)
				{
					sectionControlsCreated++;
					sectionQueriesExecuted += 2;
					bandQueriesExecuted++;

					ticketControlsCreated += numberOfTasks + 1;

					MoveNext_ForTest(form, triggeredByButtonClick: false);
					AssertChannelHeadingAndAcceptabilityBandTileDrawnCorrectly("First time around all slides via the countdown timer should update the section correctly", form, sectionControlsCreated, sectionQueriesExecuted, bandQueriesExecuted, ticketControlsCreated);
				}

				for (var i = 0; i < slideshow.BoardPivots.Count; i++)
				{
					sectionControlsCreated++;

					ticketControlsCreated += numberOfTasks + 1;
					sectionQueriesExecuted += 2;
					bandQueriesExecuted++;

					MoveNext_ForTest(form, triggeredByButtonClick: true);
					AssertChannelHeadingAndAcceptabilityBandTileDrawnCorrectly("Second time around all slides via clicking the timer manually should update the section correctly", form, sectionControlsCreated, sectionQueriesExecuted, bandQueriesExecuted, ticketControlsCreated);
				}
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestSlideshowProgression_WhenMoreTicketsThanAllowedForSlideCache_ForOneSlideOnly_ShouldUpdateAcceptabilityBandsAndChannelHeadings()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var section1 = BMSTestHelper.CreateBoardSection(config.Buffer);
			var section2 = BMSTestHelper.CreateBoardSection(config.Bucket);

			section1.Board.MB_Name = "Raxacoricofallapatorius";
			section2.Board.MB_Name = "Clom";
			section1.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Stacked;
			section2.SectionConfiguration.PanelLayoutStyle = PanelLayoutTypeList.Codes.Stacked;

			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 1, 2, 3, 4, 5, "Number of Workflows", type: AcceptabilityBandTypes.Codes.Count);
			band.BAB_FiltersBySection = true;

			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, resource.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, resource.PK);
			BMSTestHelper.AddAcceptabilityBandToSection(section1, band);
			BMSTestHelper.AddAcceptabilityBandToSection(section2, band);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, section1.Board, section2.Board);
			const int ticketLimit = 100;

			BMSRegistry.Instance.MaxNumberOfItemsAllowedToCacheBoardInSlideShow.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ticketLimit);

			BMSTestHelper.CreateWorkflows(config.Buffer, ticketLimit + 1, numberOfTasksPerWorkflow: 1, releaseGroup: config.ReleaseGroup, staff: resource);
			BMSTestHelper.CreateWorkflows(config.Bucket, ticketLimit - 1, numberOfTasksPerWorkflow: 1, releaseGroup: config.ReleaseGroup, staff: resource);

			// Ensures all tickets fit on the form without resizing
			var layout = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard, 10, 10);
			BMSTestHelper.CreateControlCustomisationLink(Factory, config.System, layout);

			Factory.Save();

			using (KUserControl.TrackInstantiatedControls_ForTest())
			using (var form = GetAndShowVisualBoardForm(slideshow))
			{
				var instantiatedSectionControls = 1;
				var instantiatedTicketControls = ticketLimit + 2; // Extra one per refresh for the template layout

				AssertChannelHeadingAndAcceptabilityBandTileDrawnCorrectly("Initial load of the board should re-draw all section layouts", form, instantiatedSectionControls, instantiatedTicketControls, "Overloaded, Idle", 101);

				// Move forward for the first time around via the slideshow timer

				MoveNext_ForTest(form, triggeredByButtonClick: false);
				instantiatedSectionControls++;
				instantiatedTicketControls += ticketLimit;

				AssertChannelHeadingAndAcceptabilityBandTileDrawnCorrectly("First time to the second slide should re-draw all section layouts", form, instantiatedSectionControls, instantiatedTicketControls, "Idle", 99);

				// Move forward through the slides again via the slideshow timer

				MoveNext_ForTest(form, triggeredByButtonClick: false);
				instantiatedSectionControls++;
				instantiatedTicketControls += ticketLimit + 2;

				AssertChannelHeadingAndAcceptabilityBandTileDrawnCorrectly("Second time around the slide with too many tickets to be cached. Should redraw layout since it wasn't cached.", form, instantiatedSectionControls, instantiatedTicketControls, "Overloaded, Idle", 101);

				MoveNext_ForTest(form, triggeredByButtonClick: false);

				AssertChannelHeadingAndAcceptabilityBandTileDrawnCorrectly("Second time around the slide with few enough tickets to be cached. Should not re-create any tickets or section layout.", form, instantiatedSectionControls, instantiatedTicketControls, "Idle", 99);

				// Move forward again via the next slide button

				MoveNext_ForTest(form, triggeredByButtonClick: true);
				instantiatedSectionControls++;
				instantiatedTicketControls += ticketLimit + 2;

				AssertChannelHeadingAndAcceptabilityBandTileDrawnCorrectly("Third time around the slide with too many tickets to be cached. Should redraw layout since it wasn't cached.", form, instantiatedSectionControls, instantiatedTicketControls, "Overloaded, Idle", 101);

				MoveNext_ForTest(form, triggeredByButtonClick: true);

				AssertChannelHeadingAndAcceptabilityBandTileDrawnCorrectly("Third time around the slide with few enough tickets to be cached. Should not re-create any tickets or section layout.", form, instantiatedSectionControls, instantiatedTicketControls, "Idle", 99);

				// Move backward via the previous slide button

				MovePrevious_ForTest(form);
				instantiatedSectionControls++;
				instantiatedTicketControls += ticketLimit + 2;

				AssertChannelHeadingAndAcceptabilityBandTileDrawnCorrectly("Fourth time around the slide with too many tickets to be cached. Should redraw layout since it wasn't cached.", form, instantiatedSectionControls, instantiatedTicketControls, "Overloaded, Idle", 101);

				MovePrevious_ForTest(form);

				AssertChannelHeadingAndAcceptabilityBandTileDrawnCorrectly("Fourth time around the slide with few enough tickets to be cached. Should not re-create any tickets or section layout.", form, instantiatedSectionControls, instantiatedTicketControls, "Idle", 99);
			}
		}

		static void AssertChannelHeadingAndAcceptabilityBandTileDrawnCorrectly(string message, VisualBoardForm form, int expectedInstantiatedSectionControls, int expectedSectionQueryCount, int expectedBandQueryCount, int expectedTicketControlsCreated)
		{
			AssertChannelHeadingAndAcceptabilityBandTileDrawnCorrectly(message, form, expectedInstantiatedSectionControls, expectedTicketControlsCreated);

			AssertExecutedCommandCountMatching("Should have executed the section query if the board has not previously been shown. The next time around it can use the CardAllocationMap stored on the viewmodel still.", "69=69", expectedSectionQueryCount);
			AssertExecutedCommandCountMatching("Should have executed the acceptability band query if the board has not previously been shown. The second time around it can use the AcceptabilityBandTiles stored on the viewmodel still.", "68=68", expectedBandQueryCount);
		}

		static void AssertChannelHeadingAndAcceptabilityBandTileDrawnCorrectly(string message, VisualBoardForm form, int expectedInstantiatedSectionControls, int expectedTicketControlsCreated, string expectedChannelStatus = "Overloaded, Idle", int expectedNumberOfWorkflowsBandValue = 1)
		{
			Application.DoEvents();

			var control = form.FindSingle<BMComponentControl>();
			var channelHeading = control.FindSingle<ChannelHeaderControl>();
			var tile = control.FindSingle<AcceptabilityBandTileControl>();

			var anyTicket = control.FindAll<TaskCardControl>().First();

			CombineAssertions(message, () =>
			{
				AssertEquals("Channel heading status", expectedChannelStatus, channelHeading.ChannelStatus);
				AssertEquals("Acceptability band status", "Number of Workflows: " + expectedNumberOfWorkflowsBandValue, tile.NameAndResultLabel.Text);

				AssertEquals("Should have created a new control since the layout has either not been created yet (first time around the board) or it has too many tickets to be cached", expectedInstantiatedSectionControls, KUserControl.InstantiatedControls_ForTest[typeof(BMComponentControl)]);
				AssertEquals("Should have created a new set of tickets since the layout has either not been created yet (first time around the board) or it has too many tickets to be cached", expectedTicketControlsCreated, KUserControl.InstantiatedControls_ForTest[anyTicket.GetType()]);
			});
		}

		public void TestSlideShow_ManualControl_Forwards()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
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

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);
			viewModel.RefreshSeconds = 1000;

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Assert("Precondition: initial load should be 'true' when we load board for the first time", form.BoardRefreshEventArgs_ForTest.IsInitialLoad);
				AssertEquals("Initially board1 should be shown", board1.PK, viewModel.CurrentBoardViewModel.BoardPK);

				form.controlsPanel.Expand();
				form.controlsPanel.PauseResumeButton.PerformClick();
				Application.DoEvents();

				AssertEquals("WHEN clicking pause button, THEN board1 should still be shown", board1.PK, viewModel.CurrentBoardViewModel.BoardPK);

				AssertEquals("WHEN clicking pause button, THEN IsSlideshowRunning should be false", false, form.IsSlideshowRunning);

				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MoveNext_ForTest(form, triggeredByButtonClick: true), form, board2, system);
				Assert("Initial load flag should be 'true' when we move to the next board which is not loaded yet and not in the cache", form.BoardRefreshEventArgs_ForTest.IsInitialLoad);

				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MoveNext_ForTest(form, triggeredByButtonClick: true), form, board3, system);
				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MoveNext_ForTest(form, triggeredByButtonClick: true), form, board4, system);
				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MoveNext_ForTest(form, triggeredByButtonClick: true), form, board1, system);

				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MoveNext_ForTest(form, triggeredByButtonClick: true), form, board2, system);
				Assert("Initial load flag should be 'false' because we move to the board which is already loaded and in the cache", !form.BoardRefreshEventArgs_ForTest.IsInitialLoad);
				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MoveNext_ForTest(form, triggeredByButtonClick: true), form, board3, system);
				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MoveNext_ForTest(form, triggeredByButtonClick: true), form, board4, system);
				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MoveNext_ForTest(form, triggeredByButtonClick: true), form, board1, system);
			}
		}

		public void TestSlideShow_ManualControl_InBoardMeetingModeShouldBeEnabled()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system, "bucket", offsetMinutes: 0, sequence: 0);

			var board1 = BMSTestHelper.CreateBoard(system, "board1", "board1");
			var board2 = BMSTestHelper.CreateBoard(system, "board2", "board2");

			var section1 = BMSTestHelper.CreateBoardSection(bucket, board1);
			var section2 = BMSTestHelper.CreateBoardSection(bucket, board2);
			section1.BackgroundColor = "Blue";
			section2.BackgroundColor = "HotPink";

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);
			viewModel.RefreshSeconds = 1000;

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				AssertEquals(board1.PK, viewModel.CurrentBoardViewModel.BoardPK);

				form.EnterBoardMeetingMode();

				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MoveNext_ForTest(form, triggeredByButtonClick: true), form, board2, system);
				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MoveNext_ForTest(form, triggeredByButtonClick: true), form, board1, system);

				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MoveNext_ForTest(form, triggeredByButtonClick: true), form, board2, system);
				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MoveNext_ForTest(form, triggeredByButtonClick: true), form, board1, system);
			}
		}

		public void TestSlideShow_ManualControl_ShouldStillLetSlideShowRun()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
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

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);
			viewModel.RefreshSeconds = 1000;

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				AssertEquals(board1.PK, viewModel.CurrentBoardViewModel.BoardPK);

				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MoveNext_ForTest(form, true), form, board2, system);
				AssertEquals(true, form.IsSlideshowRunning);
			}
		}

		public void TestSlideShow_ManualControl_ShouldKeepPausedShowOnPause()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
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

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);
			viewModel.RefreshSeconds = 1000;

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				AssertEquals(board1.PK, viewModel.CurrentBoardViewModel.BoardPK);
				form.controlsPanel.Expand();
				form.controlsPanel.PauseResumeButton.PerformClick();
				Application.DoEvents();

				AssertEquals("WHEN clicking pause button, THEN IsSlideshowRunning should be false", false, form.IsSlideshowRunning);

				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MoveNext_ForTest(form, triggeredByButtonClick: true), form, board2, system);
				AssertEquals(false, form.IsSlideshowRunning);
			}
		}

		public void TestSlideShow_ManualControl_Backwards()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
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

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);
			viewModel.RefreshSeconds = 1000;

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				AssertEquals("Initially board1 should be shown", board1.PK, viewModel.CurrentBoardViewModel.BoardPK);

				form.controlsPanel.Expand();
				form.controlsPanel.PauseResumeButton.PerformClick();
				Application.DoEvents();

				AssertEquals("WHEN clicking pause button, THEN board1 should still be shown", board1.PK, viewModel.CurrentBoardViewModel.BoardPK);

				AssertEquals("WHEN clicking pause button, THEN IsSlideshowRunning should be false", false, form.IsSlideshowRunning);

				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MovePrevious_ForTest(form), form, board4, system);
				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MovePrevious_ForTest(form), form, board3, system);
				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MovePrevious_ForTest(form), form, board2, system);
				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MovePrevious_ForTest(form), form, board1, system);

				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MovePrevious_ForTest(form), form, board4, system);
				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MovePrevious_ForTest(form), form, board3, system);
				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MovePrevious_ForTest(form), form, board2, system);
				RefreshAndAssertCurrentBoardViewModel_ForTest(() => MovePrevious_ForTest(form), form, board1, system);
			}
		}

		public void TestSlideshow()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();

			var board = CreateBoard(system, "board1", "board1");
			var board2 = CreateBoard(system, "board2", "board2");
			var board3 = CreateBoard(system, "board3", "board3");

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board, 3), Tuple.Create(board2, 3), Tuple.Create(board3, 3));

			Factory.Save();

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			var triggerable = new TriggerableAsyncStrategy();
			using (ApplyAsyncStrategy(triggerable))
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				DoAsyncSynchronously_ForTest(triggerable);
				AssertCurrentBoard("Board1 should be shown initially", board, form);

				MoveNext_ForTest(form, triggeredByButtonClick: false, triggerable: triggerable);
				AssertCurrentBoard("GIVEN Board1 is showing, WHEN clicking form.MoveNext should show Board2", board2, form);

				MoveNext_ForTest(form, triggeredByButtonClick: false, triggerable: triggerable);
				AssertCurrentBoard("GIVEN Board2 is showing, WHEN clicking form.MoveNext should show Board3", board3, form);

				MoveNext_ForTest(form, triggeredByButtonClick: true, triggerable: triggerable);
				AssertCurrentBoard("GIVEN Board3 is showing, WHEN execute next-button should show Board1", board, form);

				AssertEquals(true, form.IsSlideshowRunning);
				AssertEquals(false, form.IsInRefresh);

				PulseAndDoEvents(form.AutoRefreshForTest, 3, triggerable);
				AssertCurrentBoard("GIVEN Board1 is showing, WHEN wait for 3s should show Board2", board2, form);

				PulseAndDoEvents(form.AutoRefreshForTest, 3, triggerable);
				AssertCurrentBoard("GIVEN Board2 is showing, WHEN wait for 3s should show Board3", board3, form);

				PulseAndDoEvents(form.AutoRefreshForTest, 3, triggerable);
				AssertCurrentBoard("GIVEN Board3 is showing, WHEN wait for 3s should show Board1", board, form);

				MoveNext_ForTest(form, triggeredByButtonClick: true, triggerable: triggerable);
				AssertCurrentBoard("GIVEN Board1 is showing, WHEN clicking next-button should show Board2", board2, form);

				MovePrevious_ForTest(form);
				AssertCurrentBoard("GIVEN Board2 is showing, WHEN clicking previous-button should show Board1", board, form);

				var slideShowButton = form.controlsPanel.PauseResumeButton;
				slideShowButton.Enabled = true;
				slideShowButton.PerformClick();
				DoAsyncSynchronously_ForTest(triggerable);
				AssertImagePixelsEqual(VisualBoards.GUI.Properties.Resources.play, (Bitmap)slideShowButton.BackgroundImage);

				slideShowButton = form.controlsPanel.PauseResumeButton;
				slideShowButton.PerformClick();
				DoAsyncSynchronously_ForTest(triggerable);
				AssertImagePixelsEqual(VisualBoards.GUI.Properties.Resources.pause, (Bitmap)slideShowButton.BackgroundImage);
				AssertEquals("Timer restarted", true, form.IsSlideshowRunning);
				MoveNext_ForTest(form, triggeredByButtonClick: false, triggerable: triggerable);
				AssertCurrentBoard("GIVEN timer restarted WHEN execute move-next should show board2", board2, form);

				slideShowButton.PerformClick();
				form.RefreshNow_ForTest(true);
				AssertImagePixelsEqual(VisualBoards.GUI.Properties.Resources.play, (Bitmap)slideShowButton.BackgroundImage);
				AssertEquals("Timer stopped", false, form.IsSlideshowRunning);
			}
		}

		void AssertCurrentBoard(string message, BMBoard expectedBoard, VisualBoardForm form)
		{
			var actualBoardPk = form.SlideShowViewModel.CurrentBoardViewModel.BoardPK;
			var actualBoardName = form.SlideShowViewModel.BoardViewModels.FirstOrDefault(b => b.BoardPK == actualBoardPk).BoardName;

			var specificMessage = string.Format("Expected board {0} but found {1}", expectedBoard.MB_Name, actualBoardName);
			AssertEquals(string.Format("{0}\r\n{1}", message, specificMessage), expectedBoard.PK, actualBoardPk);
		}

		public void TestSlideshowProgression_ShouldCacheTableLayouts()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system, "bucket", 0, 0);

			var board1 = CreateBoard(system, "board1", "board1");
			var board2 = CreateBoard(system, "board2", "board2");
			var board3 = CreateBoard(system, "board3", "board3");

			var section1 = CreateBoardSection(bucket, board1);
			var section2 = CreateBoardSection(bucket, board2);
			var section3 = CreateBoardSection(bucket, board3);
			section1.BackgroundColor = "Chartreuse";
			section2.BackgroundColor = "HotPink";
			section3.BackgroundColor = "Orange";

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2, board3);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);
			var triggerable = new TriggerableAsyncStrategy();
			using (ApplyAsyncStrategy(triggerable))
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				DoAsyncSynchronously_ForTest(triggerable);

				var componentControl1 = form.FindAll<BMComponentControl>().Single();
				AssertEquals("Initially should show board1", board1.PK, componentControl1.ViewModel.BoardViewModel.BoardPK);

				MoveNext_ForTest(form, triggeredByButtonClick: false, triggerable: triggerable);
				var componentControl2 = form.FindAll<BMComponentControl>().Single();
				AssertEquals("MoveNext should show board2", board2.PK, componentControl2.ViewModel.BoardViewModel.BoardPK);

				MoveNext_ForTest(form, triggeredByButtonClick: false, triggerable: triggerable);
				var componentControl3 = form.FindAll<BMComponentControl>().Single();
				AssertEquals("MoveNext should show board3", board3.PK, componentControl3.ViewModel.BoardViewModel.BoardPK);

				MoveNext_ForTest(form, triggeredByButtonClick: false, triggerable: triggerable);
				AssertComponentControl("MoveNext should show cached-board1", componentControl1, form);

				MoveNext_ForTest(form, triggeredByButtonClick: false, triggerable: triggerable);
				AssertComponentControl("MoveNext should show cached-board2", componentControl2, form);

				MoveNext_ForTest(form, triggeredByButtonClick: false, triggerable: triggerable);
				AssertComponentControl("MoveNext should show cached-board3", componentControl3, form);
			}
		}

		void AssertComponentControl(string message, BMComponentControl expectedComponentControl, VisualBoardForm form)
		{
			BMComponentControl actualComponentControl = form.FindAll<BMComponentControl>().Single();

			CombineAssertions(message, () =>
			{
				var specificMessage = string.Format("Expecting component-control with board [{0}] but find board [{1}]", expectedComponentControl.ViewModel.BoardViewModel.BoardName, actualComponentControl.ViewModel.BoardViewModel.BoardName);
				AssertEquals(specificMessage, expectedComponentControl, actualComponentControl);
				AssertEquals("Form-slideshow-viewmodel-current-board should be the same as the form-controls-viewmodel", expectedComponentControl.ViewModel.BoardViewModel.BoardName, form.SlideShowViewModel.CurrentBoardViewModel.BoardName);
				Assert("Form-cache should contain the expected-component-control", form.GetPanelCacheByBoard().ContainsKey(expectedComponentControl.ViewModel.BoardViewModel.BoardPK));
			});
		}

		public void TestSlideshowProgression_ShouldCacheTableLayouts_MovingPrevious_KnowsCorrectPrevious()
		{
			var system = CreateSystem("ORG");
			var group = Factory.NewWithValidTestData<GlbGroup>();
			BMSTestHelper.CreateReleaseGroup(system, group);
			var bucket = CreateBucket(system, "bucket", 0, 0);

			var board1 = CreateBoard(system, "board1");
			var board2 = CreateBoard(system, "board2");
			var board3 = CreateBoard(system, "board3");

			var section1 = CreateBoardSection(bucket, board1);
			var section2 = CreateBoardSection(bucket, board2);
			var section3 = CreateBoardSection(bucket, board3);
			section1.BackgroundColor = "Yellow";
			section2.BackgroundColor = "Blue";
			section3.BackgroundColor = "Red";

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2, board3);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);
			var triggerable = new TriggerableAsyncStrategy();
			using (ApplyAsyncStrategy(triggerable))
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				DoAsyncSynchronously_ForTest(triggerable);

				var componentControl1 = form.FindAll<BMComponentControl>().Single();
				AssertEquals(
					string.Format("Initial condition, expected board {0} but found {1}", board1.GetLogInfo(), componentControl1.ViewModel.BoardViewModel.GetLogInfo(Factory)),
					board1.PK,
					componentControl1.ViewModel.BoardViewModel.BoardPK);
				AssertEquals("Yellow", ColorList.NameFromColor(componentControl1.FindAll<TaskPanel>().First().BackColor));

				MoveNext_ForTest(form, false, triggerable);
				var componentControl2 = form.FindAll<BMComponentControl>().Single();
				AssertEquals(
					string.Format("When move next #1, expected board {0} but found {1}", board2.GetLogInfo(), componentControl2.ViewModel.BoardViewModel.GetLogInfo(Factory)),
					board2.PK,
					componentControl2.ViewModel.BoardViewModel.BoardPK);
				AssertEquals("Blue", ColorList.NameFromColor(componentControl2.FindAll<TaskPanel>().First().BackColor));

				MoveNext_ForTest(form, false, triggerable);
				var componentControl3 = form.FindAll<BMComponentControl>().Single();
				AssertEquals(
					string.Format("When move next #2, Expected board {0} but found {1}", board3.GetLogInfo(), componentControl3.ViewModel.BoardViewModel.GetLogInfo(Factory)),
					board3.PK,
					componentControl3.ViewModel.BoardViewModel.BoardPK);
				AssertEquals("Red", ColorList.NameFromColor(componentControl3.FindAll<TaskPanel>().First().BackColor));

				MovePrevious_ForTest(form, triggerable);
				var newComponentControl2 = form.FindAll<BMComponentControl>().Single();
				AssertEquals(string.Format("When move previous #1, should cache component control: expected {0} but found {1}", componentControl2.ViewModel.BoardViewModel.GetLogInfo(Factory), newComponentControl2.ViewModel.BoardViewModel.GetLogInfo(Factory)),
					componentControl2,
					newComponentControl2);
				AssertEquals("Blue", ColorList.NameFromColor(newComponentControl2.FindAll<TaskPanel>().First().BackColor));

				MovePrevious_ForTest(form, triggerable);
				var newComponentControl1 = form.FindAll<BMComponentControl>().Single();
				AssertEquals("Yellow", ColorList.NameFromColor(newComponentControl1.FindAll<TaskPanel>().First().BackColor));
				AssertEquals(string.Format("When move previous #2, should cache component control: expected {0} but found {1}", componentControl1.ViewModel.BoardViewModel.GetLogInfo(Factory), newComponentControl1.ViewModel.BoardViewModel.GetLogInfo(Factory)),
					componentControl1,
					newComponentControl1);

				MovePrevious_ForTest(form, triggerable);
				var newComponentControl3 = form.FindAll<BMComponentControl>().Single();
				AssertEquals("Red", ColorList.NameFromColor(newComponentControl3.FindAll<TaskPanel>().First().BackColor));
				AssertEquals(string.Format("When move previous #3, should cache component control: expected {0} but found {1}", componentControl3.ViewModel.BoardViewModel.GetLogInfo(Factory), newComponentControl3.ViewModel.BoardViewModel.GetLogInfo(Factory)),
					componentControl3,
					newComponentControl3);
			}
		}

		#endregion

		#region SlideShowRefresh

		public void TestNavigatingSlidesViaForwardButton_ShouldNotRefreshAfterFirstLap()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board1 = CreateBoard(config.System, "Board1");
			var board2 = CreateBoard(config.System, "Board2");
			var section1 = CreateBoardSection(config.Bucket, board1);
			var section2 = CreateBoardSection(config.Bucket, board2);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);
			var refreshCount = 0;

			using (var form = new VisualBoardForm(viewModel))
			{
				form.RefreshStarted += delegate
				{ refreshCount++; };
				form.Show();

				AssertEquals(1, refreshCount);

				MoveNext_ForTest(form, true);
				AssertEquals("Should have refreshed when switching to slide not previously shown", 2, refreshCount);

				MoveNext_ForTest(form, true);
				AssertEquals("Should not have refreshed when switching to slide previously shown", 2, refreshCount);

				MoveNext_ForTest(form, true);
				AssertEquals("Should not have refreshed when switching to slide previously shown", 2, refreshCount);
			}
		}

		public void TestNavigatingSlidesViaBackwardButton_ShouldNotRefreshAfterFirstLap()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board1 = CreateBoard(config.System, "Board1");
			var board2 = CreateBoard(config.System, "Board2");
			var section1 = CreateBoardSection(config.Bucket, board1);
			var section2 = CreateBoardSection(config.Bucket, board2);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);
			var refreshCount = 0;

			var triggerable = new TriggerableAsyncStrategy();
			using (ApplyAsyncStrategy(triggerable))
			using (var form = new VisualBoardForm(viewModel))
			{
				form.RefreshStarted += delegate
				{ refreshCount++; };
				form.Show();

				DoAsyncSynchronously_ForTest(triggerable);

				AssertEquals(1, refreshCount);

				MovePrevious_ForTest(form, triggerable);
				AssertEquals("Should have refreshed when switching to slide not previously shown", 2, refreshCount);

				MovePrevious_ForTest(form, triggerable);
				AssertEquals("Should not have refreshed when switching to slide previously shown", 2, refreshCount);

				MovePrevious_ForTest(form, triggerable);
				AssertEquals("Should not have refreshed when switching to slide previously shown", 2, refreshCount);
			}
		}

		public void TestNavigatingSlidesViaSlideProgression_ShouldRefreshOnEveryLap()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board1 = CreateBoard(config.System, "Board1");
			var board2 = CreateBoard(config.System, "Board2");
			var section1 = CreateBoardSection(config.Bucket, board1);
			var section2 = CreateBoardSection(config.Bucket, board2);

			section1.BackgroundColor = ColorList.NameFromColor(Color.HotPink);
			section2.BackgroundColor = ColorList.NameFromColor(Color.PaleGoldenrod);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);
			var refreshCount = 0;

			using (var form = new VisualBoardForm(viewModel))
			{
				form.RefreshStarted += delegate
				{ refreshCount++; };
				form.Show();
				form.controlsPanel.Expand();
				Application.DoEvents();

				AssertEquals(1, refreshCount);

				form.AutoRefreshForTest.TimeUntilRefresh = new TimeSpan(0);
				form.AutoRefreshForTest.Refresh();
				Application.DoEvents();
				AssertEquals(2, refreshCount);

				form.AutoRefreshForTest.TimeUntilRefresh = new TimeSpan(0);
				form.AutoRefreshForTest.Refresh();
				Application.DoEvents();
				AssertEquals(3, refreshCount);

				form.AutoRefreshForTest.TimeUntilRefresh = new TimeSpan(0);
				form.AutoRefreshForTest.Refresh();
				Application.DoEvents();
				AssertEquals(4, refreshCount);
			}
		}

		public void TestNavigatingSlidesViaSlideProgression_ShouldDisplayCorrectRemainingTime()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board1 = CreateBoard(config.System, "Board1");
			var board2 = CreateBoard(config.System, "Board2");
			var section1 = CreateBoardSection(config.Bucket, board1);
			var section2 = CreateBoardSection(config.Bucket, board2);

			section1.BackgroundColor = ColorList.NameFromColor(Color.HotPink);
			section2.BackgroundColor = ColorList.NameFromColor(Color.PaleGoldenrod);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board1, 10), Tuple.Create(board2, 20));

			Factory.Save();

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			var triggerable = new TriggerableAsyncStrategy();
			using (ApplyAsyncStrategy(triggerable))
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				form.controlsPanel.Expand();
				triggerable.DoAllActions();

				AssertEquals("00:10", form.controlsPanel.CountdownLabel.Text);

				form.controlsPanel.NextButton.PerformClick();
				DoAsyncSynchronously_ForTest(triggerable);
				AssertEquals("00:20", form.controlsPanel.CountdownLabel.Text);

				form.controlsPanel.NextButton.PerformClick();
				DoAsyncSynchronously_ForTest(triggerable);

				AssertEquals("00:10", form.controlsPanel.CountdownLabel.Text);

				form.controlsPanel.NextButton.PerformClick();
				Application.DoEvents();
				triggerable.DoAllActions();
				AssertEquals("00:20", form.controlsPanel.CountdownLabel.Text);
			}
		}

		public void TestRefresh_ShouldDisableSlideShowMoveControls()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board, board);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			var slideShowEnabledChanged = 0;
			var previousEnabledChanged = 0;
			var nextEnabledChanged = 0;

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				form.controlsPanel.PauseResumeButton.EnabledChanged += delegate
				{ slideShowEnabledChanged++; };
				form.controlsPanel.PreviousButton.EnabledChanged += delegate
				{ previousEnabledChanged++; };
				form.controlsPanel.NextButton.EnabledChanged += delegate
				{ nextEnabledChanged++; };
				AssertEquals("Should not refresh if control panel collapsed", 0, slideShowEnabledChanged);
				AssertEquals("Should not refresh if control panel collapsed", 0, previousEnabledChanged);
				AssertEquals("Should not refresh if control panel collapsed", 0, nextEnabledChanged);

				form.controlsPanel.Expand();
				Application.DoEvents();

				form.controlsPanel.RefreshButton.PerformClick();
				Application.DoEvents();
				AssertEquals(2, slideShowEnabledChanged);
				AssertEquals(2, previousEnabledChanged);
				AssertEquals(2, nextEnabledChanged);
			}
		}

		public void TestVisualBoardWithoutSlideShowDoesntDisableMoveControls()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var viewModel = GetViewModel(board);

			Factory.Save();

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();
				var componentControls = form.FindAll<BMComponentControl>();
				AssertEquals(1, componentControls.Count());

				AssertEquals(null, form.controlsPanel.PreviousButton);
				AssertEquals(null, form.controlsPanel.NextButton);
			}
		}

		public void TestOnRefreshFinishedAfterRefresh()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board, board);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				using (var timer = new System.Windows.Forms.Timer())
				{
					timer.Interval = 10;
					var ticks = 0;

					timer.Tick += (s, e) =>
					{
						if (ticks > 100)
						{
							Fail("OnRefresh was not called from the section");
						}
						else
						{
							ticks++;
						}
					};

					form.RefreshFinished += (s, e) =>
					{
						AssertEquals(false, form.IsInRefresh);
					};
					form.RefreshStarted += (s, e) =>
					{
						AssertEquals(true, form.IsInRefresh);
					};
					timer.Start();
					form.RefreshNow_ForTest(forceReload: false);
				}
			}
		}

		public void TestIsInRefresh()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board, board);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				AssertEquals(false, form.IsInRefresh);

				form.VisualBoardForm_RefreshStarted(null, BoardRefreshEventArgs.Empty);
				AssertEquals(true, form.IsInRefresh);

				form.VisualBoardForm_RefreshFinished(null, BoardRefreshEventArgs.Empty);
				AssertEquals(false, form.IsInRefresh);
			}
		}

		public void TestShouldCompleteReloadAndAllowNextRefresh_WhenAllAcceptabilityBandsAreDisabled()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);

			var band = BMSTestHelper.CreateAcceptabilityBand(buffer, 0, 1, 2, 3, 4, 5, "Disabled AB", type: AcceptabilityBandTypes.Codes.Count);
			band.BAB_IsActive = false;
			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(section, band);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board, board);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				AssertEquals("Should complete loading and allow next refresh", false, form.IsInReload);
				AssertEquals("Should complete loading and allow next refresh", false, form.IsInRefresh);
			}
		}

		#endregion

		#region Acceptability Bands

		public void TestGetWorkflowAndSectionWorkflowCTE_ShouldUseTableValuedParameterWorkflowPKs()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System, "Caretaker");
			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 10, 12, 14, 16, 18, 21, "Number of Workflows", type: AcceptabilityBandTypes.Codes.Count);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer, board, backColor: Color.Red.Name, panelLayoutStyle: PanelLayoutTypeList.Codes.Stacked);

			var capability = BMSTestHelper.CreateCapability(Factory, "INV", "Investor");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TBP", "T. Boone Pickens", capability);
			var releaseGroup = Factory.New<GlbGroup>();
			releaseGroup.GG_Code = "FST";
			releaseGroup.GG_Desc = "Release me";

			BMSTestHelper.AddAcceptabilityBandToSection(section, band);

			CreateWorkflows(config.Buffer, numberOfWorkflows: 2, numberOfTasksPerWorkflow: 1, releaseGroup: releaseGroup, staff: staff);

			band.BAB_FiltersBySection = true;

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board);

			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			using (var form = GetAndShowVisualBoardForm(slideshow))
			{
				form.Show();

				var commandText = Db.Connection.ExecutedCommands.SingleOrDefault(c => c.Contains("-- Acceptability Band name: ["));
				AssertNotNull(commandText);
				AssertEquals("Parameter should appear within the query proper and in the set of parameters", 2, Regex.Matches(commandText, "@WorkflowPKs").Count);
				AssertEquals(true, commandText.Contains("FH_PK IN (SELECT Value FROM @WorkflowPKs)"));
				AssertEquals(1, Regex.Matches(commandText, "Params(?s).+@WorkflowPKs").Count);
			}
		}

		public void TestGetWorkflowAndSectionWorkflowCTE_ShouldParameteriseCurrentComponentPK()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System, "Caretaker");
			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 10, 12, 14, 16, 18, 21, "Number of Workflows", type: AcceptabilityBandTypes.Codes.Count);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer, board, backColor: Color.Red.Name, panelLayoutStyle: PanelLayoutTypeList.Codes.Stacked);

			var capability = BMSTestHelper.CreateCapability(Factory, "INV", "Investor");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TBP", "T. Boone Pickens", capability);
			var releaseGroup = Factory.New<GlbGroup>();
			releaseGroup.GG_Code = "FST";
			releaseGroup.GG_Desc = "Release me";

			BMSTestHelper.AddAcceptabilityBandToSection(section, band);

			CreateWorkflows(config.Buffer, numberOfWorkflows: 2, numberOfTasksPerWorkflow: 1, releaseGroup: releaseGroup, staff: staff);

			band.BAB_FiltersBySection = true;

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board);

			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			using (var form = GetAndShowVisualBoardForm(slideshow))
			{
				form.Show();

				var commandText = Db.Connection.ExecutedCommands.SingleOrDefault(c => c.Contains("-- Acceptability Band name: ["));
				AssertNotNull(commandText);
				AssertEquals("Parameter should appear within the query proper and in the set of parameters", 2, Regex.Matches(commandText, "@CurrentComponentPK").Count);
				AssertEquals(1, Regex.Matches(commandText, "Params(?s).+@CurrentComponentPK").Count);
			}
		}

		public void TestGetWorkflowAndSectionWorkflowCTE_ShouldParameteriseReleaseGroupPK()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			config.BufferSection.MS_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var otherReleaseGroup = Factory.NewWithValidTestData<GlbGroup>();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			var staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "TOP";
			staff2.GS_Code = "ONE";
			staff3.GS_Code = "PER";
			staff4.GS_Code = "CNT";

			otherReleaseGroup.Staff.Add(staff1);
			config.ReleaseGroup.Staff.Add(staff2);
			config.ReleaseGroup.Staff.Add(staff3);
			config.ReleaseGroup.Staff.Add(staff4);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer, releaseGroupPK: otherReleaseGroup.PK);
			var task1 = BMSTestHelper.CreateTask(workflow1, staff2.GS_Code, sequence: 3);
			var task2 = BMSTestHelper.CreateTask(workflow1, staff3.GS_Code, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow1, staff3.GS_Code, sequence: 1);
			var task4 = BMSTestHelper.CreateTask(workflow1, staff4.GS_Code, sequence: 4);
			var task5 = BMSTestHelper.CreateTask(workflow2, staff1.GS_Code, sequence: 5);

			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "Tasks", @"
SELECT		COUNT(*) Value,
	FH_FC_CurrentComponent Component,
	FH_GG_ReleaseGroup ReleaseGroup,
	P9_GS_NKAssignedStaffMember AdditionalAggregator
FROM		dbo.ProcessHeader
JOIN		dbo.ProcessTasks ON P9_FH_ProcessHeader = FH_PK
GROUP BY	FH_FC_CurrentComponent,
	FH_GG_ReleaseGroup,
	P9_GS_NKAssignedStaffMember
");

			band.BAB_FiltersByReleaseGroup = true;
			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band);
			sectionBand.MaximumItems = 2;

			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			using (DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				form.Show();
				var tiles = form.FindAll<AcceptabilityBandTileControl>();

				var commandText = Db.Connection.ExecutedCommands.SingleOrDefault(c => c.Contains("-- Acceptability Band name: ["));
				AssertNotNull(commandText);
				AssertEquals("Parameter should appear within the query proper and in the set of parameters", 2, Regex.Matches(commandText, "@ReleaseGroupPK").Count);
				AssertEquals(1, Regex.Matches(commandText, "Params(?s).+@ReleaseGroupPK").Count);
			}
		}

		public void TestGetWorkflowAndSectionWorkflowCTE_ShouldParameteriseComponentPK()
		{
			var capability = BMSTestHelper.CreateCapability(Factory, "USA", "American");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "EDW", "Ed Winchester", capability);

			var releaseGroup = Factory.New<GlbGroup>();
			releaseGroup.GG_Code = "FST";
			releaseGroup.GG_Desc = "Please release me, let me go!";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			BMSTestHelper.CreateReleaseGroup(config.System, releaseGroup);
			var section = config.BufferSection;
			section.SectionConfiguration.ReleaseGroupPK = releaseGroup.PK;

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Look out!";
			workflow1.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow1.FH_AgreedDeliveryDate = new ZDateTime(2016, 4, 20);
			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1);
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, true);

			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "Small Boys in the Park", type: AcceptabilityBandTypes.Codes.NumberAsPercentage);

			FilterStripsTestHelper.AddFilterStrips(band.SupersetItemsFilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate,
				FilterStripValueSetter = f => ((ModuleDateFilter)f).PropertySearch = "Today",
			});

			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			using (BMSGUITestCase.DisableAsyncBehaviour())
			using (Db.Connection.TrackExecutedCommands())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(config.BufferBoard)))
			{
				form.Show();
				Application.DoEvents();

				var commandText = Db.Connection.ExecutedCommands.SingleOrDefault(c => c.Contains("-- Acceptability Band name: ["));
				AssertNotNull(commandText);
				AssertEquals("Parameter should appear within the query proper and in the set of parameters", 3, Regex.Matches(commandText, "@ComponentPK").Count);
				AssertEquals(1, Regex.Matches(commandText, "Params(?s).+@ComponentPK").Count);
			}
		}

		public void TestGetWorkflowAndSectionWorkflowCTE_ShouldUseOnlyTwoParameterDeclarationsWithinQueryProper()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var board = BMSTestHelper.CreateBoard(config.System, "Caretaker");
			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 10, 12, 14, 16, 18, 21, "Number of Workflows", type: AcceptabilityBandTypes.Codes.Count);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer, board, backColor: Color.Red.Name, panelLayoutStyle: PanelLayoutTypeList.Codes.Stacked);

			var capability = BMSTestHelper.CreateCapability(Factory, "INV", "Investor");
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TBP", "T. Boone Pickens", capability);
			var releaseGroup = Factory.New<GlbGroup>();
			releaseGroup.GG_Code = "FST";
			releaseGroup.GG_Desc = "Release me";

			BMSTestHelper.AddAcceptabilityBandToSection(section, band);

			CreateWorkflows(config.Buffer, numberOfWorkflows: 2, numberOfTasksPerWorkflow: 1, releaseGroup: releaseGroup, staff: staff);

			band.BAB_FiltersBySection = true;

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board);

			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			using (var form = GetAndShowVisualBoardForm(slideshow))
			{
				form.Show();

				var commandText = Db.Connection.ExecutedCommands.SingleOrDefault(c => c.Contains("-- Acceptability Band name: ["));
				AssertNotNull(commandText);
				AssertEquals("Should contain only two parameter declarations in query proper", 2, Regex.Matches(commandText, "declare ").Count);
				AssertEquals(true, commandText.Contains("declare @Sum decimal(10,2);"));
				AssertEquals(true, commandText.Contains("declare @RowCount int;"));
			}
		}

		public void TestAcceptabilityBandLabel_ShouldUpdateWithRefresh()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var acceptabilityBand = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(bucket, 10, 12, 14, 16, 18, 21);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();

				var label = form.Controls.Find("SubHeadingLabel", true)[0];
				AssertEquals("Status: High Risk", label.Text);

				BMSTestHelper.CreateWorkflows(bucket, 10);
				Factory.Save();

				form.RefreshNow_ForTest(forceReload: false);

				AssertEquals("Status: Caution", label.Text);

				RowFactory.ResetCacheAfterDbUpgrade();

				AssertEquals("Status: Caution", label.Text);
			}
		}

		public void TestAcceptabilityBandLabel_WhenDisabledInRegistry()
		{
			BMSRegistry.Instance.ShowAcceptabilityBandsOnBoards.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var acceptabilityBand = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(config.Bucket, 10, 12, 14, 16, 18, 21);
			var board = config.BucketBoard;

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();

				var label = form.Controls.Find("SubHeadingLabel", true).FirstOrDefault();

				AssertNull("When deactivated in the registry, don't show a sub-heading label for acceptability bands", label);
			}
		}

		public void TestAcceptabilityBandLabel_ThreadsEndAfterFormDisposed()
		{
			var triggerable = new TriggerableAsyncStrategy();
			using (ApplyAsyncStrategy(triggerable))
			{
				var system = BMSTestHelper.CreateSystem(Factory, "ORG");
				var bucket = BMSTestHelper.CreateBucket(system);
				var acceptabilityBand = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(bucket, 10, 12, 14, 16, 18, 21);
				var section = BMSTestHelper.CreateBoardSection(bucket);
				BMSTestHelper.AddAcceptabilityBandToSection(section, acceptabilityBand, AcceptabilityBandShowOnOption.Heading);

				var board1 = BMSTestHelper.CreateBoard(system, "board1", "board1");
				var board2 = BMSTestHelper.CreateBoard(system, "board2", "board2");
				var board3 = BMSTestHelper.CreateBoard(system, "board3", "board3");

				var section1 = BMSTestHelper.CreateBoardSection(bucket, board1);
				var section2 = BMSTestHelper.CreateBoardSection(bucket, board2);
				var section3 = BMSTestHelper.CreateBoardSection(bucket, board3);
				section1.BackgroundColor = "Chartreuse";
				section2.BackgroundColor = "HotPink";
				section3.BackgroundColor = "Orange";

				BMSTestHelper.AddAcceptabilityBandToSection(section1, acceptabilityBand, AcceptabilityBandShowOnOption.Heading);
				BMSTestHelper.AddAcceptabilityBandToSection(section2, acceptabilityBand, AcceptabilityBandShowOnOption.Heading);
				BMSTestHelper.AddAcceptabilityBandToSection(section3, acceptabilityBand, AcceptabilityBandShowOnOption.Heading);

				var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2, board3);

				Factory.Save();

				var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

				using (var form = new VisualBoardForm(viewModel))
				{
					form.Show();
					triggerable.DoFirstAction();

					var label = form.Controls.Find("SubHeadingLabel", true)[0];
					AssertEquals("The label needs content or else it will not take up vertical space.", " ", label.Text);

					for (var i = 0; i < triggerable.Actions.Count - 1; i++)
					{
						triggerable.DoFirstAction(); // do all except the last action, which should be updating the summary label.
					}

					var last = triggerable.Actions.Dequeue();

					label = form.Controls.Find("SubHeadingLabel", true)[0];
					AssertEquals("The label being the same as original proves the action we are holding is our load Acceptability Band Action", " ", label.Text);

					form.controlsPanel.NextButton.PerformClick();
					AssertNoExceptionThrown("The original setup should not blow up.", () =>
					{
						last();
						triggerable.DoAllActions();
						Application.DoEvents();
					});
				}
			}
		}

		public void TestAcceptabilityBandTiles_ShouldNotShowWhenTurnedOffInRegistry()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var board = config.BufferBoard;
			var tile = BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, config.PlatinumRule);

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();

				var tileContainer = form.FindAll<AcceptabilityBandTileContainerControl>().SingleOrDefault();
				AssertNotNull("Default value in the registry is for acceptability bands to be shown.", tileContainer);
				AssertEquals("One band tile should be shown", 1, tileContainer.FindAll<AcceptabilityBandTileControl>().Count());
			}

			BMSRegistry.Instance.ShowAcceptabilityBandsOnBoards.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();

				var tileContainer = form.FindAll<AcceptabilityBandTileContainerControl>().SingleOrDefault();
				AssertNull("When disabled in the registry, acceptability bands are not shown.", tileContainer);
			}
		}

		#region Loading

		public void TestAcceptabilityBandTiles_ShouldNotShowValuesWhileLoading()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var board = config.BufferBoard;

			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 1, 2, 3, 4, 5, "This is NOT a golden rule!", type: AcceptabilityBandTypes.Codes.Count);
			FilterStripsTestHelper.AddFilterStrips(band.FilterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.TagMagnitude,
				FilterStripValueSetter = f => ((ModuleGuidFilter)f).Property = config.PlatinumTag.PK,
			});

			var outsiderStaff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			BMSTestHelper.CreatePrimaryChannelForSection(board.Sections.Single(), ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK, true);

			BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band, AcceptabilityBandShowOnOption.Both);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflowOutOfBuffer = BMSTestHelper.CreateWorkflow(jobHeader, "workflow out of buffer");
			workflowOutOfBuffer.AddTag(config.PlatinumTag);
			var workflowOutOfBufferTask = BMSTestHelper.CreateTask(workflowOutOfBuffer, GlbStaff.CurrentUser.GS_Code, 60, description: "Out of buffer");

			var workflowOutsideChannel = BMSTestHelper.CreateWorkflow(jobHeader, "workflow inside channel", config.Buffer);
			workflowOutsideChannel.AddTag(config.PlatinumTag);
			var workflowInBufferTask = BMSTestHelper.CreateTask(workflowOutsideChannel, outsiderStaff.GS_Code, 60, description: "Out of channel");

			var workflowInsideChannel = BMSTestHelper.CreateWorkflow(jobHeader, "workflow inside channel", config.Buffer);
			workflowInsideChannel.AddTag(config.PlatinumTag);
			var workflowWithPlatinumTagTask = BMSTestHelper.CreateTask(workflowInsideChannel, GlbStaff.CurrentUser.GS_Code, 60, description: "Inside channel");

			Factory.Save();

			var triggerable = new TriggerableAsyncStrategy();
			using (ApplyAsyncStrategy(triggerable))
			using (var form = new VisualBoardForm(GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				while (triggerable.Actions.Count > 0)
				{
					triggerable.DoFirstAction();
					Application.DoEvents();
					var control = form.FindAll<AcceptabilityBandTileControl>().SingleOrDefault();
					if (control != null)
					{
						AssertNotEquals("This is NOT a golden rule!: 2", control.NameAndResultLabel.Text);
					}
				}
			}
		}

		#endregion

		[TestDate(2015, 7, 14)]
		public void TestDisableAcceptabilityBand_ShouldRemoveFromBoard()
		{
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var board = config.BufferBoard;
			var tile1 = BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, config.PlatinumRule, AcceptabilityBandShowOnOption.Both);
			var tile2 = BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, config.RedRule, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var sectionControl = form.FindAll<BMComponentControl>().Single();
				var tileControl = sectionControl.FindAll<AcceptabilityBandTileControl>().SingleOrDefault(t => t.ViewModel.AcceptabilityBandPK == config.RedRule.PK);

				AssertNotNull(tileControl);
				AssertMultilineASCIIEquals("", @"Caution: Platinum Golden Rule: 0 (target is between 0 and 5)
Caution: Red Golden Rule: 0 (target is between 0 and 50)", sectionControl.ViewModel.SubHeadingAppearance.SectionSubHeadingDetailText);

				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

				config.RedRule.BAB_IsActive = false;
				Factory.Save();

				form.RefreshNow_ForTest(forceReload: false);
				Application.DoEvents();

				sectionControl = form.FindAll<BMComponentControl>().Single();
				tileControl = sectionControl.FindAll<AcceptabilityBandTileControl>().SingleOrDefault(t => t.ViewModel.AcceptabilityBandPK == config.RedRule.PK);

				AssertNull(tileControl);
				AssertMultilineASCIIEquals("", @"Caution: Platinum Golden Rule: 0 (target is between 0 and 5)", sectionControl.ViewModel.SubHeadingAppearance.SectionSubHeadingDetailText);
			}
		}

		[TestDate]
		public void TestAcceptabilityBand_Aggregate_FiltersBySection_Workflows()
		{
			AssertAcceptabilityBandFiltersBySection(AcceptabilityBandTypes.Codes.Aggregate, "Dat Band: 4 fish", "Dat Band: 2 fish", string.Format(SqlBandTestFormat, BMComponentAcceptabilityBand.FilteredWorkflowsResultSetName, "{0}"));
		}

		[TestDate]
		public void TestAcceptabilityBand_Aggregate_FiltersBySection_SectionWorkflows()
		{
			AssertAcceptabilityBandFiltersBySection(AcceptabilityBandTypes.Codes.Aggregate, "Dat Band: 11 fish", "Dat Band: 4 fish", string.Format(SqlBandTestFormat, BMComponentAcceptabilityBand.BoardSectionWorkflowsResultSetName, "{0}"));
		}

		[TestDate]
		public void TestAcceptabilityBand_Count_FiltersBySection()
		{
			AssertAcceptabilityBandFiltersBySection(AcceptabilityBandTypes.Codes.Count, "Dat Band: 4 fish", "Dat Band: 2 fish");
		}

		[TestDate]
		public void TestAcceptabilityBand_NumberAsPercentage_FiltersBySection()
		{
			AssertAcceptabilityBandFiltersBySection(AcceptabilityBandTypes.Codes.NumberAsPercentage, "Dat Band: 66.67 fish", "Dat Band: 50 fish");
		}

		[TestDate]
		public void TestAcceptabilityBand_PlannedDurationPercentage_FiltersBySection()
		{
			AssertAcceptabilityBandFiltersBySection(AcceptabilityBandTypes.Codes.PlannedDurationPercentage, "Dat Band: 40 fish", "Dat Band: 10 fish");
		}

		[TestDate]
		public void TestAcceptabilityBand_TotalPlannedDuration_FiltersBySection()
		{
			AssertAcceptabilityBandFiltersBySection(AcceptabilityBandTypes.Codes.TotalPlannedDuration, "Dat Band: 3.5 fish", "Dat Band: 0.75 fish");
		}

		const string SqlBandTestFormat = @"
SELECT	
(
	SELECT	CONVERT(DECIMAL(38, 2), COUNT(*))
	FROM		{0} 
)	as Value,
'{1}'	as Component,
NULL as ReleaseGroup
";

		public void AssertAcceptabilityBandFiltersBySection(ZString bandType, string expectedLabelWithSectionFilteringDisabled, string expectedLabelWithSectionFilteringEnabled, string sql = null)
		{
			var now = new DateTime(2016, 5, 25, 10, 0, 0);
			TestDateAttribute.Date = now;
			var otherUser = Factory.NewWithValidTestData<GlbStaff>();
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			BMSTestHelper.CreatePrimaryChannelForSection(section, "RES", GlbStaff.CurrentUser.PK, overrideChannels: true);
			var boardViewModel = BMSTestHelper.CreateSlideshowViewModel(board);

			var band = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(buffer, 0, 1, 2, 3, 4, 5, "Themes");
			band.BAB_Type = bandType;
			var sectionBand = section.SectionConfiguration.AcceptabilityBands.AddNew();
			sectionBand.AcceptabilityBandPK = band.PK;
			sectionBand.DisplayName = "Dat Band";
			sectionBand.DisplayUnits = "fish";

			var orgHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var inqHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflows = new List<ProcessHeader>(9);
			var startingComponent = buffer;

			var workflow1 = BMSTestHelper.CreateWorkflow(orgHeader, "Insatiable hunger for fish 1", startingComponent, ZDateTime.Now.AddDays(-1));
			workflow1.FH_SystemCreateUser = otherUser.GS_Code;
			workflows.Add(workflow1);
			BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 10);

			var workflow2 = BMSTestHelper.CreateWorkflow(orgHeader, "Insatiable hunger for fish 2", startingComponent, ZDateTime.Now.AddDays(-2));
			workflow2.FH_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			workflows.Add(workflow2);
			BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 20);

			var workflow3 = BMSTestHelper.CreateWorkflow(inqHeader, "Insatiable hunger for fish 3", startingComponent, ZDateTime.Now.AddDays(-3));
			workflow3.FH_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			workflows.Add(workflow3);
			BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 30);

			var workflow4 = BMSTestHelper.CreateWorkflow(inqHeader, "Something else 4", startingComponent, ZDateTime.Now.AddDays(-4));
			workflow4.FH_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			workflows.Add(workflow4);
			BMSTestHelper.CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 40);

			var workflow5 = BMSTestHelper.CreateWorkflow(orgHeader, "Insatiable hunger for fish 5", startingComponent, ZDateTime.Now.AddDays(-5));
			workflow5.FH_SystemCreateUser = otherUser.GS_Code;
			workflows.Add(workflow5);
			BMSTestHelper.CreateTask(workflow5, otherUser.GS_Code, 50);

			var workflow6 = BMSTestHelper.CreateWorkflow(orgHeader, "Insatiable hunger for fish 6", startingComponent, ZDateTime.Now.AddDays(-6));
			workflow6.FH_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			workflows.Add(workflow6);
			BMSTestHelper.CreateTask(workflow6, otherUser.GS_Code, 60);

			var workflow7 = BMSTestHelper.CreateWorkflow(inqHeader, "Insatiable hunger for fish 7", startingComponent, ZDateTime.Now.AddDays(-7));
			workflow7.FH_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			workflows.Add(workflow7);
			BMSTestHelper.CreateTask(workflow7, otherUser.GS_Code, 70);

			var workflow8 = BMSTestHelper.CreateWorkflow(inqHeader, "Something else 8", startingComponent, ZDateTime.Now.AddDays(-8));
			workflow8.FH_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			workflows.Add(workflow8);
			BMSTestHelper.CreateTask(workflow8, otherUser.GS_Code, 80);

			var workflow9 = BMSTestHelper.CreateWorkflow(inqHeader, "Insatiable hunger for fish 9", startingComponent, ZDateTime.Now.AddDays(-1));
			workflow9.FH_SystemCreateUser = otherUser.GS_Code;
			workflows.Add(workflow9);
			BMSTestHelper.CreateTask(workflow9, GlbStaff.CurrentUser.GS_Code, 90);

			// Board filter (plus channel limitation) results in workflow1, workflow2, workflow3, workflow9 appearing on the board.
			FilterStripsTestHelper.AddFilterStrips(section.WorkflowFilter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = (f) => ((ModuleTextFilter)f).Property = "Insatiable"
			});

			if (!string.IsNullOrEmpty(sql))
			{
				band.BAB_SqlText = string.Format(sql, band.BAB_FC_Component);
			}
			else if (band.BAB_Type == AcceptabilityBandTypes.Codes.PlannedDurationPercentage || band.BAB_Type == AcceptabilityBandTypes.Codes.NumberAsPercentage)
			{
				// Superset filter matches workflow1, workflow5, and workflow9 only
				FilterStripsTestHelper.AddFilterStrips(band.SupersetItemsFilterRule, new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = "Creating User",
					FilterStripValueSetter = (f) => ((ModuleNkFilter)f).Property = otherUser.GS_Code
				});
			}

			if (band.BAB_Type != AcceptabilityBandTypes.Codes.SQL)
			{
				// Filter rule matches workflow1, workflow2, workflow5, and workflow6
				FilterStripsTestHelper.AddFilterStrips(band.FilterRule, new FilterStripsTestHelper.FilterStripDefinition
				{
					FilterStripName = ProcessHeader.ModuleFilterConstants.WorkflowType,
					FilterStripValueSetter = (f) => ((ModuleTextFilter)f).Property = OrgHeaderWorkflowDescriptor.WorkflowTypeCode
				});
			}

			band.BAB_FiltersBySection = false;
			Factory.Save();

			ShowFormAndAssertAcceptabilityBandLabel("Section filtering should not happen because that option is disabled on the acceptability band, and yet...", boardViewModel, expectedLabelWithSectionFilteringDisabled, band, expectedTaskCardCount: 4);

			band.BAB_FiltersBySection = true;
			Factory.Save();

			boardViewModel = BMSTestHelper.CreateSlideshowViewModel(board);
			ShowFormAndAssertAcceptabilityBandLabel("The band should now filter by board section (considering channels and workflow filters), and yet...", boardViewModel, expectedLabelWithSectionFilteringEnabled, band);
		}

		static void ShowFormAndAssertAcceptabilityBandLabel(string message, BoardSlideshowViewModel viewModel, string expectedLabel, BMComponentAcceptabilityBand band, int expectedTaskCardCount = -1)
		{
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				if (expectedTaskCardCount >= 0)
				{
					var taskCards = form.FindAll<TaskCardControl>();
					AssertEquals("Board should only show workflows for the current user (the only channel) and filtered by completion statement starting with 'Insatiable', and yet...", 4, taskCards.Count());
				}

				var bandTile = form.FindAll<AcceptabilityBandTileControl>().Single();

				var bandViewModel = form.FindAll<AcceptabilityBandTileControl>().Single().ViewModel;
				var commandText = bandViewModel == null ? string.Empty : band.GetAcceptabilityBandSqlCommand(bandViewModel.CreateParametersForCalculation_ForTest(band), Db.Connection).CommandText;

				AssertEquals(message + commandText, expectedLabel, bandTile.NameAndResultLabel.Text);
			}
		}

		public void TestFiltersByReleaseGroupOverride()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			BMSTestHelper.CreatePrimaryChannelForSection(section, "RES", GlbStaff.CurrentUser.PK, overrideChannels: true);
			var boardViewModel = BMSTestHelper.CreateSlideshowViewModel(board);

			var orgHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var releaseGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			BMSTestHelper.CreateReleaseGroup(system, releaseGroup1);
			var releaseGroup2 = Factory.NewWithValidTestData<GlbGroup>();
			BMSTestHelper.CreateReleaseGroup(system, releaseGroup2);
			section.SectionConfiguration.ReleaseGroupPK = releaseGroup1.PK;

			var workflow1 = BMSTestHelper.CreateWorkflow(orgHeader, "Insatiable hunger for fish", buffer, releaseGroupPK: releaseGroup1.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(orgHeader, "Insatiable hunger for fish", buffer, releaseGroupPK: releaseGroup2.PK);
			BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 10);
			BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 10);

			var bandReleaseDisabledNotOverridden = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, null, false, null);
			var bandReleaseEnabledNotOverridden = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, true, null, false, null);
			var bandReleaseDisabledOverriddenNo = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, "No", false, null);
			var bandReleaseEnableddOverriddenNo = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, true, "No", false, null);
			var bandReleaseDisabledOverriddenYes = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, "Yes", false, null);
			var bandReleaseEnableddOverriddenYes = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, true, "Yes", false, null);

			Factory.Save();

			using (var form = new VisualBoardForm(boardViewModel))
			{
				form.Show();
				Application.DoEvents();
				var tiles = form.FindAll<AcceptabilityBandTileControl>().ToArray();

				AssertAcceptabilityBandLabel("Release group filtering should not occur because it's disabled on the band and not overridden by the board section, and yet...", bandReleaseDisabledNotOverridden, tiles, "Dat Band: 2 fish");
				AssertAcceptabilityBandLabel("Release group filtering should occur because it's enabled on the band and not overridden by the board section, and yet...", bandReleaseEnabledNotOverridden, tiles, "Dat Band: 1 fish");
				AssertAcceptabilityBandLabel("Release group filtering should not occur because it's overridden by the board section, and yet...", bandReleaseDisabledOverriddenNo, tiles, "Dat Band: 2 fish");
				AssertAcceptabilityBandLabel("Release group filtering should not occur even though it's enabled on the band, because it's overridden by the board section, and yet...", bandReleaseEnableddOverriddenNo, tiles, "Dat Band: 2 fish");
				AssertAcceptabilityBandLabel("Release group filtering should occur because it's overridden by the board section, and yet...", bandReleaseDisabledOverriddenYes, tiles, "Dat Band: 1 fish");
				AssertAcceptabilityBandLabel("Release group filtering should occur because it's overridden by the board section, and yet...", bandReleaseEnableddOverriddenYes, tiles, "Dat Band: 1 fish");
			}
		}

		public void TestFiltersBySectionOverride()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			BMSTestHelper.CreatePrimaryChannelForSection(section, "RES", GlbStaff.CurrentUser.PK, overrideChannels: true);
			var boardViewModel = BMSTestHelper.CreateSlideshowViewModel(board);

			var orgHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(orgHeader, "Insatiable hunger for fish", buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(orgHeader, "Hey! That's just my as-per-in!", buffer);
			BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 10);
			BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 10);

			FilterStripsTestHelper.AddFilterStrips(section.WorkflowFilter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = (f) => ((ModuleTextFilter)f).Property = "Insatiable"
			});

			var bandFilterDisabledNotOverridden = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, null, false, null);
			var bandFilterEnabledNotOverridden = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, null, true, null);
			var bandFilterDisabledOverriddenNo = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, null, false, "No");
			var bandFilterEnableddOverriddenNo = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, null, true, "No");
			var bandFilterDisabledOverriddenYes = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, null, false, "Yes");
			var bandFilterEnableddOverriddenYes = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, null, true, "Yes");

			Factory.Save();

			using (var form = new VisualBoardForm(boardViewModel))
			{
				form.Show();
				Application.DoEvents();
				var tiles = form.FindAll<AcceptabilityBandTileControl>().ToArray();

				AssertAcceptabilityBandLabel("Board section filtering should not occur because it's disabled on the band and not overridden by the board section, and yet...", bandFilterDisabledNotOverridden, tiles, "Dat Band: 2 fish");
				AssertAcceptabilityBandLabel("Board section filtering should occur because it's enabled on the band and not overridden by the board section, and yet...", bandFilterEnabledNotOverridden, tiles, "Dat Band: 1 fish");
				AssertAcceptabilityBandLabel("Board section filtering should not occur because it's overridden by the board section, and yet...", bandFilterDisabledOverriddenNo, tiles, "Dat Band: 2 fish");
				AssertAcceptabilityBandLabel("Board section filtering should not occur even though it's enabled on the band, because it's overridden by the board section, and yet...", bandFilterEnableddOverriddenNo, tiles, "Dat Band: 2 fish");
				AssertAcceptabilityBandLabel("Board section filtering should occur because it's overridden by the board section, and yet...", bandFilterDisabledOverriddenYes, tiles, "Dat Band: 1 fish");
				AssertAcceptabilityBandLabel("Board section filtering should occur because it's overridden by the board section, and yet...", bandFilterEnableddOverriddenYes, tiles, "Dat Band: 1 fish");
			}
		}

		public void TestBoundaryValuesOverride()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			BMSTestHelper.CreatePrimaryChannelForSection(section, "RES", GlbStaff.CurrentUser.PK, overrideChannels: true);
			var boardViewModel = BMSTestHelper.CreateSlideshowViewModel(board);

			var orgHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(orgHeader, "Insatiable hunger for fish", buffer);
			BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 10);

			var notOverriddenBand = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, null, false, null);
			var band1 = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, null, false, null, true, 2, 3, 4, 5, 6, 7);
			var band2 = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, null, false, null, true, 1, 2, 3, 4, 5, 6);
			var band3 = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, null, false, null, true, 0, 1, 2, 3, 4, 5);
			var band4 = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, null, false, null, true, -1, 0, 1, 2, 3, 4);
			var band5 = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, null, false, null, true, -2, -1, 0, 1, 2, 3);
			var band6 = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, null, false, null, true, -3, -2, -1, 0, 1, 2);
			var band7 = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, null, false, null, true, -4, -3, -2, -1, 0, 1);
			var band8 = CreateAcceptabilityBandForFilterOverrideTests(buffer, section, false, null, false, null, true, -5, -4, -3, -2, -1, 0);

			Factory.Save();

			using (var form = new VisualBoardForm(boardViewModel))
			{
				form.Show();
				Application.DoEvents();
				var tiles = form.FindAll<AcceptabilityBandTileControl>().ToArray();

				AssertAcceptabilityBandStatus("The acceptability band is not using overridden boundary values, so status should be based on the acceptability band's configured values, and yet...", notOverriddenBand, tiles, ComponentAcceptabilityStatus.HighRisk);
				AssertAcceptabilityBandStatus("The acceptability band is using overridden boundary values, so status should not be based on the acceptability band's configured values, and yet...", band1, tiles, ComponentAcceptabilityStatus.HighRisk);
				AssertAcceptabilityBandStatus("The acceptability band is using overridden boundary values, so status should not be based on the acceptability band's configured values, and yet...", band2, tiles, ComponentAcceptabilityStatus.Caution);
				AssertAcceptabilityBandStatus("The acceptability band is using overridden boundary values, so status should not be based on the acceptability band's configured values, and yet...", band3, tiles, ComponentAcceptabilityStatus.Good);
				AssertAcceptabilityBandStatus("The acceptability band is using overridden boundary values, so status should not be based on the acceptability band's configured values, and yet...", band4, tiles, ComponentAcceptabilityStatus.Excellent);
				AssertAcceptabilityBandStatus("The acceptability band is using overridden boundary values, so status should not be based on the acceptability band's configured values, and yet...", band5, tiles, ComponentAcceptabilityStatus.Excellent);
				AssertAcceptabilityBandStatus("The acceptability band is using overridden boundary values, so status should not be based on the acceptability band's configured values, and yet...", band6, tiles, ComponentAcceptabilityStatus.Good);
				AssertAcceptabilityBandStatus("The acceptability band is using overridden boundary values, so status should not be based on the acceptability band's configured values, and yet...", band7, tiles, ComponentAcceptabilityStatus.Caution);
				AssertAcceptabilityBandStatus("The acceptability band is using overridden boundary values, so status should not be based on the acceptability band's configured values, and yet...", band8, tiles, ComponentAcceptabilityStatus.HighRisk);
			}
		}

		static void AssertAcceptabilityBandLabel(string message, BMComponentAcceptabilityBand band, IEnumerable<AcceptabilityBandTileControl> tiles, string expectedLabel)
		{
			var tile = tiles.Single(x => x.ViewModel.AcceptabilityBandPK == band.PK);
			AssertEquals(message, expectedLabel, tile.NameAndResultLabel.Text);
		}

		static void AssertAcceptabilityBandStatus(string message, BMComponentAcceptabilityBand band, IEnumerable<AcceptabilityBandTileControl> tiles, ComponentAcceptabilityStatus expectedStatus)
		{
			var tile = tiles.Single(x => x.ViewModel.AcceptabilityBandPK == band.PK);
			AssertEquals(message, expectedStatus, tile.ViewModel.Status);
		}

		static BMComponentAcceptabilityBand CreateAcceptabilityBandForFilterOverrideTests(BMComponent buffer, BMBoardSection section, bool filtersByReleaseGroup, string filtersByReleaseGroupOverride, bool filtersBySection, string filtersBySectionOverride,
			bool overrideBoundaryValues = false, int cautionMin = 1, int goodMin = 2, int excellentMin = 3, int excellentMax = 4, int goodMax = 5, int cautionMax = 6)
		{
			var band = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(buffer, 0, 0, 0, 0, 0, 0, "Themes");
			band.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			band.BAB_FiltersByReleaseGroup = filtersByReleaseGroup;
			band.BAB_FiltersBySection = filtersBySection;
			band.BAB_Name = ZGuid.NewZGuid().ToString();

			var sectionBand = section.SectionConfiguration.AcceptabilityBands.AddNew();
			sectionBand.AcceptabilityBandPK = band.PK;
			sectionBand.DisplayName = "Dat Band";
			sectionBand.DisplayUnits = "fish";

			if (!string.IsNullOrEmpty(filtersByReleaseGroupOverride))
			{
				sectionBand.FiltersByReleaseGroupOverride = filtersByReleaseGroupOverride;
			}

			if (!string.IsNullOrEmpty(filtersBySectionOverride))
			{
				sectionBand.FiltersBySectionOverride = filtersBySectionOverride;
			}

			if (overrideBoundaryValues)
			{
				sectionBand.AreBoundaryValuesOverridden = true;
				sectionBand.CautionMinOverride = cautionMin;
				sectionBand.GoodMinOverride = goodMin;
				sectionBand.ExcellentMinOverride = excellentMin;
				sectionBand.ExcellentMaxOverride = excellentMax;
				sectionBand.GoodMaxOverride = goodMax;
				sectionBand.CautionMaxOverride = cautionMax;
			}

			return band;
		}

		public void TestAcceptabilityBandSummary_ShouldObeySectionFiltering()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			BMSTestHelper.CreatePrimaryChannelForSection(section, "RES", GlbStaff.CurrentUser.PK, overrideChannels: true);
			var boardViewModel = BMSTestHelper.CreateSlideshowViewModel(board);

			var band = BMSTestHelper.CreateAcceptabilityBand(buffer, 0, 0, 0, 0, 1, 2, "Themes");
			band.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			BMSTestHelper.AddAcceptabilityBandToSection(section, band, AcceptabilityBandShowOnOption.Both);

			var orgHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(orgHeader, "Insatiable hunger for fish", buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(orgHeader, "Hey! That's just my as-per-in!", buffer);
			BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 10);
			BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 10);

			FilterStripsTestHelper.AddFilterStrips(section.WorkflowFilter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = (f) => ((ModuleTextFilter)f).Property = "Insatiable"
			});

			Factory.Save();

			using (var form = new VisualBoardForm(boardViewModel))
			{
				form.Show();
				Application.DoEvents();
				var tiles = form.FindAll<AcceptabilityBandTileControl>().ToArray();

				AssertAcceptabilityBandStatus("Only one workflow on the board so the status should be good, and yet...", band, tiles, ComponentAcceptabilityStatus.Good);

				var label = form.Controls.Find("SubHeadingLabel", true)[0];
				AssertEquals("Only one workflow on the board so the status should be good, and yet...", "Status: Good", label.Text);
			}
		}

		public void TestAcceptabilityBandsShownOnBoth_WithSameParameters_ShouldOnlyCalculateOncePerSection()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section1 = BMSTestHelper.CreateBoardSection(buffer, board);
			BMSTestHelper.CreatePrimaryChannelForSection(section1, "RES", GlbStaff.CurrentUser.PK, overrideChannels: true);

			var band = BMSTestHelper.CreateAcceptabilityBand(buffer, 0, 0, 0, 0, 1, 2, "Themes");
			band.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(section1, band, AcceptabilityBandShowOnOption.Heading);
			AssertEquals("Precondition", true, sectionBand.IsFilteringBySection);

			CombineAssertions("One section, one section band", () =>
			{
				ShowFormAndAssertCalculationCount("The band appears once on one section and so one calculation should occur, and yet...", 1, board);

				sectionBand.SelectedShowOnOption = AcceptabilityBandShowOnOption.Tile;
				ShowFormAndAssertCalculationCount("The band appears once on one section and so one calculation should occur, and yet...", 1, board);

				sectionBand.SelectedShowOnOption = AcceptabilityBandShowOnOption.Both;
				ShowFormAndAssertCalculationCount("The band appears twice on one section in two different forms, but the result for the band should be calculated just once, and yet...", 1, board);

				sectionBand.SelectedShowOnOption = AcceptabilityBandShowOnOption.Heading;
			});

			var sectionBand2 = BMSTestHelper.AddAcceptabilityBandToSection(section1, band, AcceptabilityBandShowOnOption.Heading);
			sectionBand2.FiltersByReleaseGroupOverride = BoardSectionAcceptabilityBandLookups.YesOptionCode;
			AssertEquals("Precondition", true, sectionBand2.IsFilteringBySection);

			CombineAssertions("One section, two section bands", () =>
			{
				ShowFormAndAssertCalculationCount(@"The band appears twice on one section,
					is defined as two separate section bands which results weren't cached by this moment (which is the main thing),
					so two calculations should occur (even if the parameters are the same), and yet...", 2, board);
			});
		}

		public void TestAcceptabilityBands_WithSameParameters_OnDifferentSections_ShouldOnlyCalculateOnce_IfNotFilteredBySectionFilters_AndSharedCachingIsEnabledLocally()
		{
			BMSTestHelper.EnableAcceptabilityBandResultClientCache();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);

			var band = BMSTestHelper.CreateAcceptabilityBand(buffer, 0, 0, 0, 0, 1, 2, "Themes");
			band.BAB_Type = AcceptabilityBandTypes.Codes.Count;

			var section1 = BMSTestHelper.CreateBoardSection(buffer, board, row: 0);
			var sectionBand1 = BMSTestHelper.AddAcceptabilityBandToSection(section1, band, AcceptabilityBandShowOnOption.Both);

			var section2 = BMSTestHelper.CreateBoardSection(buffer, board, row: 1);
			var sectionBand2 = BMSTestHelper.AddAcceptabilityBandToSection(section2, band, AcceptabilityBandShowOnOption.Both);

			CombineAssertions("Section bands are filtered by section filters", () =>
			{
				AssertEquals("Precondition", true, sectionBand1.IsFilteringBySection);
				AssertEquals("Precondition", true, sectionBand2.IsFilteringBySection);

				ShowFormAndAssertCalculationCount(@"The section bands are on different sections with different section filters involved,
					so two calculations should happen (one per section - even if the band parameters are the same), and yet...", 2, board);
			});

			CombineAssertions("Section bands are NOT filtered by section filters", () =>
			{
				sectionBand1.FiltersBySectionOverride = "No";
				sectionBand2.FiltersBySectionOverride = "No";
				AssertEquals("Precondition", false, sectionBand1.IsFilteringBySection);
				AssertEquals("Precondition", false, sectionBand2.IsFilteringBySection);

				ShowFormAndAssertCalculationCount(@"The section bands are on different sections, the band parameters are the same,
					the section filters are different but not involved,
					so that by the moment when the second section is calculated, the result for the first section is already cached and reused,
					therefore just one calculation should happen, and yet...", 1, board);
			});

			CombineAssertions("The first section band is filtered by section filters and the second is not", () =>
			{
				sectionBand1.FiltersBySectionOverride = "Yes";
				sectionBand2.FiltersBySectionOverride = "No";
				AssertEquals("Precondition", true, sectionBand1.IsFilteringBySection);
				AssertEquals("Precondition", false, sectionBand2.IsFilteringBySection);

				ShowFormAndAssertCalculationCount(@"The section bands are on different sections, the band parameters are the same,
					the section filters are different and involved for the first section band,
					so two calculations should happen (one per section - even if the band parameters are the same), and yet...", 2, board);
			});

			CombineAssertions("The second section band is filtered by section filters and the first is not", () =>
			{
				sectionBand1.FiltersBySectionOverride = "No";
				sectionBand2.FiltersBySectionOverride = "Yes";
				AssertEquals("Precondition", false, sectionBand1.IsFilteringBySection);
				AssertEquals("Precondition", true, sectionBand2.IsFilteringBySection);

				ShowFormAndAssertCalculationCount(@"The section bands are on different sections, the band parameters are the same,
					the section filters are different and involved for the second section band,
					so two calculations should happen (one per section - even if the band parameters are the same), and yet...", 2, board);
			});

			CombineAssertions("Section bands are NOT filtered by section filters but have different overriden boundaries", () =>
			{
				sectionBand1.FiltersBySectionOverride = "No";
				sectionBand2.FiltersBySectionOverride = "No";
				AssertEquals("Precondition", false, sectionBand1.IsFilteringBySection);
				AssertEquals("Precondition", false, sectionBand2.IsFilteringBySection);

				sectionBand2.AreBoundaryValuesOverridden = true;
				sectionBand2.CautionMaxOverride = 1;
				AssertNotEquals("Precondition", sectionBand1.CautionMaxOverride, sectionBand2.CautionMaxOverride);

				ShowFormAndAssertCalculationCount(@"The section bands are on different sections, the band parameters are the same (the boundary overrides do not count),
					the section filters are different but not involved,
					so that by the moment when the second section is calculated, the result for the first section is already cached and reused,
					therefore just one calculation should happen, and yet...", 1, board);
			});

			BMSTestHelper.DisableAcceptabilityBandResultCache();

			CombineAssertions("Section bands are NOT filtered by section filters, caching is off", () =>
			{
				ShowFormAndAssertCalculationCount(@"The section bands are on different sections, the band parameters are the same (the boundary overrides do not count),
					the section filters are different but not involved;
					however, the caching is switched of,
					so two calculations should happen, and yet...", 2, board);
			});
		}

		void ShowFormAndAssertCalculationCount(string message, int expected, BMBoard board)
		{
			Factory.Save();
			BMSTestHelper.ClearAcceptabilityBandCache();
			AcceptabilityBandDataProvider.OnBeforeReaderCommandExecutedCount_ForTest = 0;

			using (var form = GetAndShowVisualBoardForm(board))
			{
				AssertEquals(message, expected, AcceptabilityBandDataProvider.OnBeforeReaderCommandExecutedCount_ForTest);
			}
		}

		public void TestRefresh_ShouldRefreshAcceptabilityBandCaches()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			BMSTestHelper.CreatePrimaryChannelForSection(section, "RES", GlbStaff.CurrentUser.PK, overrideChannels: true);

			var band1 = BMSTestHelper.CreateAcceptabilityBand(buffer, 0, 0, 0, 0, 1, 2, "Themes");
			band1.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			var sectionBand1 = BMSTestHelper.AddAcceptabilityBandToSection(section, band1, AcceptabilityBandShowOnOption.Tile);

			var band2 = BMSTestHelper.CreateAcceptabilityBand(buffer, 0, 0, 0, 0, 1, 2, "For the party");
			band2.BAB_Type = AcceptabilityBandTypes.Codes.Count;
			var sectionBand2 = BMSTestHelper.AddAcceptabilityBandToSection(section, band2, AcceptabilityBandShowOnOption.Heading);

			var orgHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(orgHeader, "Insatiable hunger for fish", buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(orgHeader, "Hey! That's just my as-per-in!", buffer);
			BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 10);
			BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 10);

			FilterStripsTestHelper.AddFilterStrips(section.WorkflowFilter, new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = (f) => ((ModuleTextFilter)f).Property = "Insatiable"
			});

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var viewModel = form.FindAll<BMComponentControl>().Single().ViewModel;
				var tiles = form.FindAll<AcceptabilityBandTileControl>().ToArray();

				AssertAcceptabilityBandStatus("Only one workflow on the board so the status should be good, and yet...", band1, tiles, ComponentAcceptabilityStatus.Good);

				var label = form.Controls.Find("SubHeadingLabel", true)[0];
				AssertEquals("Only one workflow on the board so the status should be good, and yet...", "Status: Good", label.Text);

				var cache = viewModel.CachedWorkflowPks;
				AssertEquals(1, cache.Count);

				var workflow3 = BMSTestHelper.CreateWorkflow(orgHeader, "Insatiable hunger for fish", buffer);
				BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 10);

				Factory.Save();

				form.RefreshNow_ForTest();
				Application.DoEvents();

				cache = viewModel.CachedWorkflowPks;
				AssertEquals(1, cache.Count);

				tiles = form.FindAll<AcceptabilityBandTileControl>().ToArray();
				label = form.Controls.Find("SubHeadingLabel", true)[0];

				AssertAcceptabilityBandStatus("Two workflows should now be on the board, so if the band didn't update it might indicate that the cache is stale.", band1, tiles, ComponentAcceptabilityStatus.Caution);
				AssertEquals("Two workflows should now be on the board, so if the heading didn't update it might indicate that the cache is stale.", "Status: Caution", label.Text);
			}
		}

		public void TestLoadingAcceptabilityBandTilesAndHeaders_ShouldHitFilterTablesOnlyOnceForAllBands()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "INQ");

			for (var i = 0; i < 10; i++)
			{
				var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "Band " + i, type: AcceptabilityBandTypes.Codes.Count);
				FilterStripsTestHelper.AddStartsWithFilter(band.FilterRule, "Completion Statement", "Shalala");
				BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band, AcceptabilityBandShowOnOption.Both);
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedBoard = newFactory.Load<BMBoard>(config.BufferBoard.PK);

			using (AssertDbHitsForAllFactories(
				new Dictionary<string, int>
				{
					{ BMComponentAcceptabilityBandSchema.Constants.TableName, 1 },
					{ StmModuleFilterSchema.Constants.TableName, 1 },
					{ StmModuleFilterUserDataSchema.Constants.TableName, 1 }
				}, includeFactoryPredicate: factory => factory.NameForDebugging.Contains("UpdateAcceptabilityBands")))
			using (var form = GetAndShowVisualBoardForm(loadedBoard))
			{
			}
		}

		public void TestSectionWorkflowFilter_ShouldNotBeCreated_IfThereIsNotAcceptabilityBand_FilteredBySectionFilters()
		{
			BMSTestHelper.EnableAcceptabilityBandResultClientCache(); // we need the cache to be enabled to be able to check whether section filters are created or not by addressing to the cache
			var config = AcceptabilityBandTestConfigsHelper.CreateAcceptabilityBandTestConfig(Factory);
			var sectionBand1 = BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, config.PlatinumRule, AcceptabilityBandShowOnOption.Both);
			var sectionBand2 = BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, config.RedRule, AcceptabilityBandShowOnOption.Both);
			sectionBand1.FiltersBySectionOverride = "No";
			sectionBand2.FiltersBySectionOverride = "No";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedBoard = newFactory.Load<BMBoard>(config.BufferBoard.PK);
			var loadedSection = loadedBoard.Sections.Single();
			AssertNotEquals("Precondition", loadedSection.WorkflowFilter);

			CombineAssertions("Section bands are NOT filtered by section filters", () =>
			{
				AssertEquals("Precondition", false, sectionBand1.IsFilteringBySection);
				AssertEquals("Precondition", false, sectionBand2.IsFilteringBySection);

				// in order to check when a section workflow filter is created, we need to set a custom filter on the section to ensure that WorkflowLoader is hit if the workflow section filter is needed
				// otherwise workflow PKs will be simply taken from the list of workflows loaded on the section without referring to WorkflowLoader and we get 0 db hits

				VisualBoardFormAllFactoriesHitTest.AssertDBHitsForRefreshingBoardWithCustomFilter(new Dictionary<string, int>
					{
						{ BMComponentAcceptabilityBandSchema.Constants.TableName, 2 },
					}, loadedBoard, includeFactoryPredicate: f => BMSTestCaseWithFactory.IsAcceptabilityBandCalculationFactory(f));

				AssertNull("Section workflow filter should NOT be created and cached as it is not used by any section band", BMSTestHelper.GetAcceptabilityBandCachedWorkflowPKs(config.BufferSection.PK));
			});

			BMSTestHelper.ClearAcceptabilityBandCache();
			CombineAssertions("One section band is filtered by section filters", () =>
			{
				sectionBand1.FiltersBySectionOverride = "Yes";
				sectionBand2.FiltersBySectionOverride = "No";
				Factory.Save();

				AssertEquals("Precondition", true, sectionBand1.IsFilteringBySection);
				AssertEquals("Precondition", false, sectionBand2.IsFilteringBySection);

				VisualBoardFormAllFactoriesHitTest.AssertDBHitsForRefreshingBoardWithCustomFilter(new Dictionary<string, int>
					{
						{ BMComponentAcceptabilityBandSchema.Constants.TableName, 2 },
						{ BMBoardSchema.Constants.TableName, 1 },
						{ BMBoardSectionSchema.Constants.TableName, 1 },
						{ BMBoardSectionAdditionalComponentSchema.Constants.TableName, 1 },
						{ BMBoardSectionChannelSchema.Constants.TableName, 2 },
						{ ProcessHeaderSchema.Constants.TableName, 1 },
						{ StmModuleFilterSchema.Constants.TableName, 2 },
					}, loadedBoard, includeFactoryPredicate: f => BMSTestCaseWithFactory.IsAcceptabilityBandCalculationFactory(f));
				AssertNotNull("Section workflow filter should be created and cached as it is used by some section band", BMSTestHelper.GetAcceptabilityBandCachedWorkflowPKs(config.BufferSection.PK));
			});
		}

		public void TestCustomSectionFilterStrips_ShouldNotAffectAcceptabilityBandResults_WhenShowingTasks()
		{
			AssertCustomSectionFilterDoNotAffectAcceptabilityBandResult(CardTypeList.Codes.Task);
		}

		void AssertCustomSectionFilterDoNotAffectAcceptabilityBandResult(ZString cardType)
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "INQ");

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource.PK);
			config.BufferSection.SectionConfiguration.CardType = cardType;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, completionStatement: "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, completionStatement: "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, completionStatement: "workflow3");
			BMSTestHelper.CreateTask(workflow1, resource.GS_Code, 60, taskStatus: "ASN");
			BMSTestHelper.CreateTask(workflow2, resource.GS_Code, 60, taskStatus: "ASN");
			BMSTestHelper.CreateTask(workflow3, resource.GS_Code, 60, taskStatus: "ASN");

			workflow1.MoveToComponent(config.Buffer);
			workflow2.MoveToComponent(config.Buffer);

			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "Band", type: AcceptabilityBandTypes.Codes.Count);
			BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				CombineAssertions("No user defined custom filters", () =>
				{
					var viewModel = form.FindAll<BMComponentControl>().Single().ViewModel;
					Assert("Custom filters should not be applied", !viewModel.AreWorkflowOrTaskFiltersRedefined);

					AssertContainsExactElementsInAnyOrder(nameof(viewModel.SectionWorkflowPKsWithTaskAndSectionFiltersApplied), new[] { workflow1.PK, workflow2.PK }, viewModel.SectionWorkflowPKsWithTaskAndSectionFiltersApplied);
					AssertContainsExactElementsInAnyOrder(nameof(viewModel.CachedWorkflowPks), new[] { workflow1.PK, workflow2.PK }, viewModel.CachedWorkflowPks);

					var formTaskCards = form.FindAll<TaskCardControl>();
					BMSGUITestCase.AssertVisibilityTaskCard(Factory, formTaskCards, workflow1, true);
					BMSGUITestCase.AssertVisibilityTaskCard(Factory, formTaskCards, workflow2, true);

					var tile = form.FindAll<AcceptabilityBandTileControl>().Single();
					AssertEquals("Band: 2", tile.NameAndResultLabel.Text);
				});

				workflow3.MoveToComponent(config.Buffer); //we want to ensure acceptaband calculates its value based on actual data, does not just take the value from some cache
				Factory.Save();

				CombineAssertions("User defined custom filter added", () =>
				{
					var customFilter = BMSGUITestHelper.CreateTemporaryBMFilterRuleModuleFilter(Factory, "CustomFilter");
					FilterStripsTestHelper.AddStartsWithFilter(customFilter, ProcessHeader.ModuleFilterConstants.CompletionStatement, "workflow1");

					var loadedFilter = new BusinessObjectFactory().Load<StmModuleFilter>(customFilter.PK);
					AssertNull("Precondition: the custom filter should be defined on the BMBoardSectionViewModel only and should not be saved into the database", loadedFilter);

					var control = form.FindAll<BMComponentControl>().Single();
					control.SetCustomWorkflowFilterForTestingAndRefresh(customFilter);

					var viewModel = control.ViewModel;
					Assert("The view model should be aware that the custom filter is applied", viewModel.AreWorkflowOrTaskFiltersRedefined);

					AssertContainsExactElementsInAnyOrder("The custom filter should define workflows shown on the section", new[] { workflow1.PK }, viewModel.SectionWorkflowPKsWithTaskAndSectionFiltersApplied);
					AssertContainsExactElementsInAnyOrder(nameof(viewModel.CachedWorkflowPks), new[] { workflow1.PK }, viewModel.CachedWorkflowPks);

					var formTaskCards = form.FindAll<TaskCardControl>();
					BMSGUITestCase.AssertVisibilityTaskCard(Factory, formTaskCards, workflow1, true);
					BMSGUITestCase.AssertVisibilityTaskCard(Factory, formTaskCards, workflow2, false);
					BMSGUITestCase.AssertVisibilityTaskCard(Factory, formTaskCards, workflow3, false);

					var tile = form.FindAll<AcceptabilityBandTileControl>().Single();
					AssertEquals("The custom filter should not affect acceptability bands - they should use the filter persisted in the database (even if the section filter and custom filters are defined via the same BMBoardSectionViewModel properties)", "Band: 3", tile.NameAndResultLabel.Text);
				});
			}
		}

		public void TestCurrentTaskFilter_ShouldNotAffectAcceptabilityBandResults()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "INQ");

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var someOtherResource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource.PK);
			config.BufferSection.SectionConfiguration.CardType = CardTypeList.Codes.Task;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, completionStatement: "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, completionStatement: "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, completionStatement: "workflow3");
			var task1 = BMSTestHelper.CreateTask(workflow1, resource.GS_Code, 60, taskStatus: "ASN", sequence: 1);
			BMSTestHelper.CreateTask(workflow2, someOtherResource.GS_Code, 60, taskStatus: "ASN", sequence: 1);
			var task2 = BMSTestHelper.CreateTask(workflow2, resource.GS_Code, 60, taskStatus: "ASN", sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow3, resource.GS_Code, 60, taskStatus: "ASN", sequence: 1);

			Assert("Precondition: task1 should be startable", task1.IsStartable());
			Assert("Precondition: task2 should not be startable", !task2.IsStartable());
			Assert("Precondition: task3 should be startable", task3.IsStartable());

			workflow1.MoveToComponent(config.Buffer);
			workflow2.MoveToComponent(config.Buffer);

			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "Band", type: AcceptabilityBandTypes.Codes.Count);
			BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			CombineAssertions("No current task filters", () =>
			{
				var boardViewModel = VisualBoardFormTest.GetViewModel(config.BufferBoard);
				using (var form = new VisualBoardForm(boardViewModel))
				{
					form.Show();

					var formTaskCards = form.FindAll<TaskCardControl>();
					AssertVisibilityTaskCard(Factory, formTaskCards, workflow1, true);
					AssertVisibilityTaskCard(Factory, formTaskCards, workflow2, true);

					var viewModel = form.FindAll<BMComponentControl>().Single().ViewModel;
					AssertContainsExactElementsInAnyOrder(nameof(viewModel.SectionWorkflowPKsWithTaskAndSectionFiltersApplied), new[] { workflow1.PK, workflow2.PK }, viewModel.SectionWorkflowPKsWithTaskAndSectionFiltersApplied);
					AssertContainsExactElementsInAnyOrder(nameof(viewModel.CachedWorkflowPks), new[] { workflow1.PK, workflow2.PK }, viewModel.CachedWorkflowPks);
					var tile = form.FindAll<AcceptabilityBandTileControl>().Single();
					AssertEquals("Band: 2", tile.NameAndResultLabel.Text);
				}
			});

			workflow3.MoveToComponent(config.Buffer); //we want to ensure acceptaband calculates its value based on actual data, does not just take the value from some cache
			Factory.Save();

			CombineAssertions("Current task filter added", () =>
			{
				var boardViewModel = VisualBoardFormTest.GetViewModel(config.BufferBoard);
				using (var form = new VisualBoardForm(boardViewModel))
				{
					form.Show();

					boardViewModel.FilterManager.ApplyFilter(new CurrentTaskFilter());
					form.RefreshNow_ForTest(forceReload: false);

					var formTaskCards = form.FindAll<TaskCardControl>();
					AssertVisibilityTaskCard(Factory, formTaskCards, workflow1, true);
					AssertVisibilityTaskCard(Factory, formTaskCards, workflow2, false);
					AssertVisibilityTaskCard(Factory, formTaskCards, workflow3, true);

					var viewModel = form.FindAll<BMComponentControl>().Single().ViewModel;
					AssertContainsExactElementsInAnyOrder(nameof(viewModel.SectionWorkflowPKsWithTaskAndSectionFiltersApplied), new[] { workflow1.PK, workflow2.PK, workflow3.PK }, viewModel.SectionWorkflowPKsWithTaskAndSectionFiltersApplied);
					AssertContainsExactElementsInAnyOrder(nameof(viewModel.CachedWorkflowPks), new[] { workflow1.PK, workflow2.PK, workflow3.PK }, viewModel.CachedWorkflowPks);
					var tile = form.FindAll<AcceptabilityBandTileControl>().Single();
					AssertEquals("The current filter should not affect acceptability bands", "Band: 3", tile.NameAndResultLabel.Text);
				}
			});
		}

		[TestDate(2017, 10, 25)]
		public void TestAcceptabilityBandStatusCalculation_ShouldNotLoadIncompleteWorkflows_IfTheyAreAlreadyLoaded_AndThereAreNoCustomFilters()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "INQ");

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var channel = BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource.PK);
			config.BufferSection.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			var orgHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(orgHeader, "workflow1", config.Buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(orgHeader, "workflow2", config.Buffer);
			BMSTestHelper.CreateTask(workflow1, resource.GS_Code, 10);
			BMSTestHelper.CreateTask(workflow2, resource.GS_Code, 10);

			workflow1.MoveToComponent(config.Buffer);
			workflow2.MoveToComponent(config.Buffer);

			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "Band", type: AcceptabilityBandTypes.Codes.Count);
			BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band, AcceptabilityBandShowOnOption.Both);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedBoard = newFactory.Load<BMBoard>(config.BufferBoard.PK);

			var customFilter = BMSGUITestHelper.CreateTemporaryBMFilterRuleModuleFilter(Factory, "CustomFilter");
			FilterStripsTestHelper.AddStartsWithFilter(customFilter, ProcessHeader.ModuleFilterConstants.CompletionStatement, "workflow1");

			const string sql = "SELECT P9_FH_ProcessHeader FROM dbo.ProcessTasks WHERE P9_FH_ProcessHeader IS NOT NULL";

			using (var testSet = TestEntityFrameworkSettings.Get())
			{
				testSet.ApplyIsNotNullToJoinOnFK = true;
				CombineAssertions("No user defined custom filters", () =>
				{
					using (Db.Connection.TrackExecutedCommands())
					using (AssertDbHitsForAllFactories(
						new Dictionary<string, int>
						{
							{ BMComponentAcceptabilityBandSchema.Constants.TableName, 1 },
							{ StmModuleFilterSchema.Constants.TableName, 2 },
						}, includeFactoryPredicate: f => BMSTestCaseWithFactory.IsAcceptabilityBandCalculationFactory(f)))
					using (var form = GetAndShowVisualBoardForm(loadedBoard))
					{
						var executedParameterizedQueries = Db.Connection.ExecutedCommands.Where(p => p.Contains(sql));
						AssertEquals("Should hit database only once as AcceptabilityBandStatusSharedCacheService should use a section filter created based on preloaded section's incomplete workflows", 1, executedParameterizedQueries.Count());
					}
				});

				CombineAssertions("User defined custom filter added", () =>
				{
					using (var form = GetAndShowVisualBoardForm(loadedBoard)) //checking db hits after applying the custom filter and refreshing
					using (Db.Connection.TrackExecutedCommands())
					using (AssertDbHitsForAllFactories(
						new Dictionary<string, int>
						{
							{ BMComponentAcceptabilityBandSchema.Constants.TableName, 1 },
							{ BMBoardSchema.Constants.TableName, 1 },
							{ BMBoardSectionSchema.Constants.TableName, 1 },
							{ BMBoardSectionAdditionalComponentSchema.Constants.TableName, 1 },
							{ BMBoardSectionChannelSchema.Constants.TableName, 2 },
							{ ProcessHeaderSchema.Constants.TableName, 1 },
							{ StmModuleFilterSchema.Constants.TableName, 2 },
						}, includeFactoryPredicate: f => BMSTestCaseWithFactory.IsAcceptabilityBandCalculationFactory(f)))
					{
						var control = form.FindAll<BMComponentControl>().Single();
						control.SetCustomWorkflowFilterForTestingAndRefresh(customFilter);

						var executedParameterizedQueries = Db.Connection.ExecutedCommands.Where(p => p.Contains(sql));
						AssertEquals("There should be two db hits after applying the custom filter: 1) board refresh 2) acceptaband calculation (we cannot rely on preloaded workflows anymore)", 2, executedParameterizedQueries.Count());
					}
				});
			}
		}

		public void TestAcceptabilityBands_WithAdditionalAggregator_ShouldCreateMultipleTilesAndSubheadingLines()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			config.BufferSection.MS_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var otherReleaseGroup = Factory.NewWithValidTestData<GlbGroup>();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			var staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "TOP";
			staff2.GS_Code = "ONE";
			staff3.GS_Code = "PER";
			staff4.GS_Code = "CNT";

			otherReleaseGroup.Staff.Add(staff1);
			config.ReleaseGroup.Staff.Add(staff2);
			config.ReleaseGroup.Staff.Add(staff3);
			config.ReleaseGroup.Staff.Add(staff4);

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Buffer, releaseGroupPK: otherReleaseGroup.PK);
			var task1 = BMSTestHelper.CreateTask(workflow1, staff2.GS_Code, sequence: 3);
			var task2 = BMSTestHelper.CreateTask(workflow1, staff3.GS_Code, sequence: 2);
			var task3 = BMSTestHelper.CreateTask(workflow1, staff3.GS_Code, sequence: 1);
			var task4 = BMSTestHelper.CreateTask(workflow1, staff4.GS_Code, sequence: 4);
			var task5 = BMSTestHelper.CreateTask(workflow2, staff1.GS_Code, sequence: 5);

			var band = BMSTestHelper.CreateAcceptabilityBand(config.Buffer, 0, 0, 0, 0, 0, 0, "Tasks", @"
SELECT		COUNT(*) Value,
			FH_FC_CurrentComponent Component,
			FH_GG_ReleaseGroup ReleaseGroup,
			P9_GS_NKAssignedStaffMember AdditionalAggregator
FROM		dbo.ProcessHeader
JOIN		dbo.ProcessTasks ON P9_FH_ProcessHeader = FH_PK
GROUP BY	FH_FC_CurrentComponent,
			FH_GG_ReleaseGroup,
			P9_GS_NKAssignedStaffMember
");
			band.BAB_FiltersByReleaseGroup = true;
			var sectionBand = BMSTestHelper.AddAcceptabilityBandToSection(config.BufferSection, band);
			sectionBand.MaximumItems = 2;

			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var tiles = form.FindAll<AcceptabilityBandTileControl>();
				AssertArrayEqualsByElements(new[] { "Tasks (CNT): 1", "Tasks (ONE): 1" }, tiles.Select(x => x.NameAndResultLabel.Text).ToArray());
			}

			sectionBand.DisplayName = "Shalala";
			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var tiles = form.FindAll<AcceptabilityBandTileControl>();
				AssertArrayEqualsByElements(new[] { "Shalala (CNT): 1", "Shalala (ONE): 1" }, tiles.Select(x => x.NameAndResultLabel.Text).ToArray());
			}

			sectionBand.DisplayName = ZString.Empty;
			sectionBand.MaximumItems = 999;
			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var tiles = form.FindAll<AcceptabilityBandTileControl>();
				AssertArrayEqualsByElements(new[] { "CNT: 1", "ONE: 1", "PER: 2" }, tiles.Select(x => x.NameAndResultLabel.Text).ToArray());
			}

			band.BAB_SqlText = @"
SELECT		TOP 10 COUNT(*) Value,
			FH_FC_CurrentComponent Component,
			FH_GG_ReleaseGroup ReleaseGroup,
			P9_GS_NKAssignedStaffMember AdditionalAggregator
FROM		dbo.ProcessHeader
JOIN		dbo.ProcessTasks ON P9_FH_ProcessHeader = FH_PK
GROUP BY	FH_FC_CurrentComponent,
			FH_GG_ReleaseGroup,
			P9_GS_NKAssignedStaffMember
ORDER BY	P9_GS_NKAssignedStaffMember desc
";
			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var tiles = form.FindAll<AcceptabilityBandTileControl>();
				AssertArrayEqualsByElements(new[] { "PER: 2", "ONE: 1", "CNT: 1" }, tiles.Select(x => x.NameAndResultLabel.Text).ToArray());
			}
		}

		#endregion

		#region Refresh

		public void TestRefreshClearsActiveControl()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var board = BMSTestHelper.CreateBoard(system);

			var buffer = BMSTestHelper.CreateBuffer(system);
			var bufferSection = BMSTestHelper.CreateBoardSection(buffer, board);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RE1", "resource1");
			BMSTestHelper.CreateWorkflows(buffer, numberOfWorkflows: 5, numberOfTasksPerWorkflow: 1, staff: resource1);
			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(board)))
			using (form.EnableShouldReportLowMemory_ForTest())
			{
				form.Show();
				Application.DoEvents();
				form.ActiveControl = form.Controls[0];

				Application.DoEvents();
				form.RefreshBoard();

				Application.DoEvents();
				AssertNull(form.ActiveControl);
			}
		}

		public void TestReportError_WhenLowMemory()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var board = BMSTestHelper.CreateBoard(system);

			var buffer = BMSTestHelper.CreateBuffer(system);
			var bufferSection = BMSTestHelper.CreateBoardSection(buffer, board);

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RE1", "resource1");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RE2", "resource2");
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, resource2.PK);
			BMSTestHelper.CreateWorkflows(buffer, numberOfWorkflows: 1, numberOfTasksPerWorkflow: 2, staff: resource1); //2
			BMSTestHelper.CreateWorkflows(buffer, numberOfWorkflows: 3, numberOfTasksPerWorkflow: 4, staff: resource2); //12

			var bucket = BMSTestHelper.CreateBucket(system);
			var bucketSection = BMSTestHelper.CreateBoardSection(bucket, board);
			bucketSection.Row = 1;
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "RE3", "resource3");
			BMSTestHelper.CreatePrimaryChannelForSection(bucketSection, ChannelTypeList.Codes.Resource, resource3.PK);
			BMSTestHelper.CreateWorkflows(bucket, numberOfWorkflows: 5, numberOfTasksPerWorkflow: 6, staff: resource3); //30

			var moduleGridsection = board.Sections.AddNew();
			moduleGridsection.MS_SectionType = BMConstants.ModuleGridSectionType;
			var moduleGridsectionConfig = (ModuleGridSectionConfiguration)moduleGridsection.Configuration;
			moduleGridsectionConfig.SectionNameIsOverridden = true;
			moduleGridsectionConfig.SectionNameOverride = "Job Workflows";
			moduleGridsection.Row = 2;

			bucketSection.SectionConfiguration.Validation.ValidateAll();
			AssertNoErrors(bucketSection.SectionConfiguration);

			Factory.Save();

			using (ZSystemInformation.SetInstanceForTesting(new SysInfoWithNotEnoughVirtualMemoryForVisualBoard()))
			using (var form = new VisualBoardForm(GetViewModel(board)))
			using (form.EnableShouldReportLowMemory_ForTest())
			{
				form.Show();
				Application.DoEvents();
			}

			var lastErrorMessage = ErrorReporter.LastMessageReported;

			AssertNotNull(lastErrorMessage);

			var errorMessage = @"Running low on virtual memory: 98 MB. Board name: An Board, Total tickets: 44
Section: Type: CMP, Name: buffer, Tickets: 14 Channel: Type: RES, Code: RE1, Tasks: 2 Channel: Type: RES, Code: RE2, Tasks: 12
Section: Type: CMP, Name: bucket, Tickets: 30 Channel: Type: RES, Code: RE3, Tasks: 30
Section: Type: MOD, Name: Job Workflows";

			AssertEquals(errorMessage, lastErrorMessage);

			ErrorReporter.Clear();
		}

#if !WINZOR

		public void TestRefresh_TriesToKeepSessionAlive()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);
			Factory.Save();

			var keepAliveTracker = new MockKeepSessionAlive();
			using (ObjectFactory.Substitute("IKeepSessionAlive", keepAliveTracker))
			using (var form = new VisualBoardForm(GetViewModel(config.Section.Board)))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("IKeepAliveTracker is called when board is opened for first time", 1, keepAliveTracker.HitTracker);

				keepAliveTracker.Clear();
				form.RefreshNow_ForTest(true);
				Application.DoEvents();

				AssertEquals("IKeepAliveTracker is called again.", 1, keepAliveTracker.HitTracker);
			}
		}

		class MockKeepSessionAlive : IKeepSessionAlive
		{
			public void KeepAlive() => HitTracker++;

			public int HitTracker { get; private set; }

			public void Clear() => HitTracker = 0;
		}

#endif

		public void TestRefresh_SwitchToNonConstrainedMode()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);
			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(config.Section.Board)))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("GIVEN: CCR exist", true, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(config.CCR, config.Buffer));
				AssertEquals("GIVEN: currently in ConstrainedMode", true, ConstrainedModeHelper.IsInConstrainedMode(config.Section.Component, config.Section.SectionConfiguration.ApplicableReleaseGroupPK));

				var control = form.FindAll<BMComponentControl>().First();
				var ccrHeaderControl = control.ViewModel.ComponentGrid.Cells.Where(cell => cell.ContentType == CellContentType.CCRHeading);
				AssertEquals("GIVEN: CCR-Header exists", 13, ccrHeaderControl.Count());

				var menuItem = control.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Switch to non-Constrained Mode");
				menuItem.PerformClick();
				Application.DoEvents();
				AssertEquals("WHEN: switching to NonConstrainedMode", false, ConstrainedModeHelper.IsInConstrainedMode(config.Section.Component, config.Section.SectionConfiguration.ApplicableReleaseGroupPK));

				control = form.FindAll<BMComponentControl>().First();
				ccrHeaderControl = control.ViewModel.ComponentGrid.Cells.Where(cell => cell.ContentType == CellContentType.CCRHeading);
				AssertEquals("THEN: CCR-Header should disappear", 0, ccrHeaderControl.Count());
			}
		}

		public void TestRefresh_SwitchToConstrainedMode()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);
			AssertEquals("GIVEN: CCR exist", true, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(config.CCR, config.Buffer));

			config.ReleaseGroup.Staff.Add(config.CCR);
			AssertCollectionContains("GIVEN: CCR exists in ReleaseGroup so no error on 'Switch to Constrained Mode'", config.CCR, config.ReleaseGroup.Staff);

			config.ComponentReleaseGroupLink.Delete();
			AssertEquals("GIVEN: currently in NonConstrainedMode", false, ConstrainedModeHelper.IsInConstrainedMode(config.Section.Component, config.Section.SectionConfiguration.ApplicableReleaseGroupPK));

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(config.Section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var control = form.FindAll<BMComponentControl>().First();
				var ccrHeaderControl = control.ViewModel.ComponentGrid.Cells.Where(cell => cell.ContentType == CellContentType.CCRHeading);
				AssertEquals("GIVEN: CCR-Headers do not exist", 0, ccrHeaderControl.Count());

				var menuItem = control.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Switch to Constrained Mode");
				menuItem.PerformClick();
				Application.DoEvents();
				AssertEquals("WHEN: switching to ConstrainedMode - has no error", false, UnitTestUserNotification.Instance.IsShowingError);
				AssertEquals("WHEN: switching to ConstrainedMode", true, ConstrainedModeHelper.IsInConstrainedMode(config.Section.Component, config.Section.SectionConfiguration.ApplicableReleaseGroupPK));

				control = form.FindAll<BMComponentControl>().First();
				ccrHeaderControl = control.ViewModel.ComponentGrid.Cells.Where(cell => cell.ContentType == CellContentType.CCRHeading);
				AssertEquals("THEN: CCR-Header should appear", 13, ccrHeaderControl.Count());
			}
		}

		[TestDate(2000, 1, 1)]
		public void TestRefresh_MarkAsCCR()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);
			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(config.Section.Board)))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("GIVEN: CCR is constrained resource", true, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(config.CCR, config.Buffer));
				AssertEquals("GIVEN: NonCCR2 is not constrained resource", false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(config.NonCCR2, config.Buffer));

				var control = form.FindAll<BMComponentControl>().First();

				var cCRTaskPanelsInfo = control
					.FindAll<TaskPanel>()
					.Where(taskPanel => taskPanel.Cell.Channel.EntityPK == config.CCR.PK)
					.OrderBy(taskPanel => taskPanel.Cell.Row)
					.Select(taskPanel => string.Format("Row: {0}, BackgroundColor: {1}", taskPanel.Cell.Row, taskPanel.Cell.BackColor.ToString()));
				var cCRTaskPanelsInfoAsString = string.Join("\r\n", cCRTaskPanelsInfo);

				var nonCCR2TaskPanelsInfo = control
					.FindAll<TaskPanel>()
					.Where(taskPanel => taskPanel.Cell.Channel.EntityPK == config.NonCCR2.PK)
					.OrderBy(taskPanel => taskPanel.Cell.Row)
					.Select(taskPanel => string.Format("Row: {0}, BackgroundColor: {1}", taskPanel.Cell.Row, taskPanel.Cell.BackColor.ToString()));
				AssertNotEquals("GIVEN: NonCCR2 channel backcolor is not the same as CCR", cCRTaskPanelsInfoAsString, string.Join("\r\n", nonCCR2TaskPanelsInfo));

				var nonCCR2Column = control.ViewModel.ComponentGrid.Cells
					.Where(cell => cell.Channel != null && cell.Channel.EntityPK == config.NonCCR2.PK)
					.Select(cell => cell.Column).First();
				var nonCCR2_CCRHeaderControl = control.ViewModel.ComponentGrid.Cells
					.Where(cell => cell.ContentType == CellContentType.CCRHeading && cell.Column == nonCCR2Column - 1)
					.Where(cell => cell.ContentType == CellContentType.CCRHeading);
				AssertEquals("GIVEN: no CCR-Header before NonCCR2 channel", 0, nonCCR2_CCRHeaderControl.Count());

				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
				var channelHeaderControl = BMSGUITestCase.FindChannelHeaderControl(form, config.NonCCR2);
				((LazyContextMenuStrip)channelHeaderControl.ContextMenuStrip).AddItems_ForTest(channelHeaderControl);
				var ccrMenuItem = channelHeaderControl.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().SingleOrDefault(t => t.Text == "Mark as capacity constrained");
				ccrMenuItem.PerformClick();
				Application.DoEvents();
				BMSFormTestHelper.PressHotkeys(form, Keys.F5 | Keys.Shift);
				Application.DoEvents();
				AssertEquals("WHEN: Modifing NonCCR2 to constrained-resource and Reload Board (Shift+F5)", true, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(config.NonCCR2, config.Buffer));

				control = form.FindAll<BMComponentControl>().First();
				nonCCR2TaskPanelsInfo = control
					.FindAll<TaskPanel>()
					.Where(taskPanel => taskPanel.Cell.Channel.EntityPK == config.NonCCR2.PK)
					.OrderBy(taskPanel => taskPanel.Cell.Row)
					.Select(taskPanel => string.Format("Row: {0}, BackgroundColor: {1}", taskPanel.Cell.Row, taskPanel.Cell.BackColor.ToString()));
				AssertMultilineASCIIEquals("THEN: NonCCR2 channel backcolor should be same as CCR because NonCCR2 is constrained-resource now", cCRTaskPanelsInfoAsString, string.Join("\r\n", nonCCR2TaskPanelsInfo));

				nonCCR2Column = control.ViewModel.ComponentGrid.Cells
					.Where(cell => cell.Channel != null && cell.Channel.EntityPK == config.NonCCR2.PK)
					.Select(cell => cell.Column).First();
				nonCCR2_CCRHeaderControl = control.ViewModel.ComponentGrid.Cells
					.Where(cell => cell.ContentType == CellContentType.CCRHeading && cell.Column == nonCCR2Column - 1)
					.Where(cell => cell.ContentType == CellContentType.CCRHeading);
				AssertEquals("THEN: CCR-Header should be shown before NonCCR2 channel because NonCCR2 is constrained-resource now", 13, nonCCR2_CCRHeaderControl.Count());
			}
		}

		public void TestF1KeyDownWhenContextMenuIsDisplayed()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);
			Factory.Save();

			using (var form = new VisualBoardFormWithExposedGlobalHotkeys(GetViewModel(config.Section.Board)))
			{
				form.GlobalHotkeys_Exposed.RegisterHotKey(Keys.F1, ProcessNewServiceRequestHotKey);
				form.Show();
				Application.DoEvents();
				var control = form.FindAll<BMComponentControl>().First();
				control.ContextMenuStrip.Show(form, 0, 0);

				BMSFormTestHelper.PressHotkeys(control.ContextMenuStrip, Keys.F1);

				AssertEquals("F1 was pressed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		bool ProcessNewServiceRequestHotKey(object sender, Keys keyPressed)
		{
			Globals.Message.Show("F1 was pressed.");
			return true;
		}

		class VisualBoardFormWithExposedGlobalHotkeys : VisualBoardForm
		{
			internal VisualBoardFormWithExposedGlobalHotkeys(BoardSlideshowViewModel viewModel)
				: base(viewModel)
			{
			}

			internal HotkeyRegister GlobalHotkeys_Exposed => base.GlobalHotkeys;
		}

		[TestDate(2000, 1, 1)]
		public void TestRefresh_MarkAsNonCCR()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);
			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(config.Section.Board)))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("GIVEN: CCR is constrained resource", true, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(config.CCR, config.Buffer));
				AssertEquals("GIVEN: NonCCR2 is not constrained resource", false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(config.NonCCR2, config.Buffer));

				var control = form.FindAll<BMComponentControl>().First();

				var cCRTaskPanelsInfo = control
					.FindAll<TaskPanel>()
					.Where(taskPanel => taskPanel.Cell.Channel.EntityPK == config.CCR.PK)
					.OrderBy(taskPanel => taskPanel.Cell.Row)
					.Select(taskPanel => string.Format("Row: {0}, BackgroundColor: {1}", taskPanel.Cell.Row, taskPanel.Cell.BackColor.ToString()));

				var nonCCR2TaskPanelsInfo = control
					.FindAll<TaskPanel>()
					.Where(taskPanel => taskPanel.Cell.Channel.EntityPK == config.NonCCR2.PK)
					.OrderBy(taskPanel => taskPanel.Cell.Row)
					.Select(taskPanel => string.Format("Row: {0}, BackgroundColor: {1}", taskPanel.Cell.Row, taskPanel.Cell.BackColor.ToString()));
				var nonCCR2TaskPanelsInfoAsString = string.Join("\r\n", nonCCR2TaskPanelsInfo);
				AssertNotEquals("GIVEN: NonCCR2 channel backcolor is not the same as CCR", nonCCR2TaskPanelsInfoAsString, string.Join("\r\n", cCRTaskPanelsInfo));

				var cCRColumn = control.ViewModel.ComponentGrid.Cells
					.Where(cell => cell.Channel != null && cell.Channel.EntityPK == config.CCR.PK)
					.Select(cell => cell.Column).First();
				var cCR_CCRHeaderControl = control.ViewModel.ComponentGrid.Cells
					.Where(cell => cell.ContentType == CellContentType.CCRHeading && cell.Column == cCRColumn - 1)
					.Where(cell => cell.ContentType == CellContentType.CCRHeading);
				AssertEquals("GIVEN: CCR-Header exists before CCR channel", 13, cCR_CCRHeaderControl.Count());

				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
				var channelHeaderControl = BMSGUITestCase.FindChannelHeaderControl(form, config.CCR);
				((LazyContextMenuStrip)channelHeaderControl.ContextMenuStrip).AddItems_ForTest(channelHeaderControl);
				var ccrMenuItem = channelHeaderControl.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().SingleOrDefault(t => t.Text == "Mark as non-capacity constrained");
				ccrMenuItem.PerformClick();
				Application.DoEvents();
				BMSFormTestHelper.PressHotkeys(form, Keys.F5 | Keys.Shift);
				Application.DoEvents();
				AssertEquals("WHEN: Modifing CCR to non-constrained-resource and Reload Board (Shift+F5)", false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(config.CCR, config.Buffer));

				control = form.FindAll<BMComponentControl>().First();
				cCRTaskPanelsInfo = control
					.FindAll<TaskPanel>()
					.Where(taskPanel => taskPanel.Cell.Channel.EntityPK == config.CCR.PK)
					.OrderBy(taskPanel => taskPanel.Cell.Row)
					.Select(taskPanel => string.Format("Row: {0}, BackgroundColor: {1}", taskPanel.Cell.Row, taskPanel.Cell.BackColor.ToString()));
				AssertMultilineASCIIEquals("THEN: CCR channel backcolor should be same as NonCCR2 because CCR is non-constrained-resource now", nonCCR2TaskPanelsInfoAsString, string.Join("\r\n", cCRTaskPanelsInfo));

				cCRColumn = control.ViewModel.ComponentGrid.Cells
					.Where(cell => cell.Channel != null && cell.Channel.EntityPK == config.CCR.PK)
					.Select(cell => cell.Column).First();
				cCR_CCRHeaderControl = control.ViewModel.ComponentGrid.Cells
					.Where(cell => cell.ContentType == CellContentType.CCRHeading && cell.Column == cCRColumn - 1)
					.Where(cell => cell.ContentType == CellContentType.CCRHeading);
				AssertEquals("THEN: CCR-Header should not be shown before CCR channel because CCR is non-constrained-resource now", 0, cCR_CCRHeaderControl.Count());
			}
		}

		[TestDate(2015, 1, 29, 11, 0, 0)]
		public void TestRefreshCells_CardSameValuesOnRefresh()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "MOE", "Man Of Eggs");
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "FiddlyBits", buffer);
			var task1 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, 80);

			Factory.Save();

			var viewModel = GetViewModel(section.Board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var cache = form.BoardViewModel.GetSections().Cast<BMBoardSectionViewModel>().Select(s => s.Cache).Single();
				var copy = cache.GetDumpOfCacheForTest();

				form.RefreshNow_ForTest(true);

				Application.DoEvents();

				cache = form.BoardViewModel.GetSections().Cast<BMBoardSectionViewModel>().Select(s => s.Cache).Single();
				var otherCopy = cache.GetDumpOfCacheForTest();

				CombineAssertions(() =>
				{
					foreach (var pair in otherCopy.Where(p => p.Value is IZType))
					{
						AssertEquals(string.Format("Key {0} wasn't equal after refresh", pair.Key), pair.Value, copy[pair.Key]);
					}
				});
			}
		}

		[TestDate(2015, 1, 29, 11, 0, 0)]
		[ExpectNoExceptions]
		public void TestDeletingTaskBeforeEnterBoardMeetingMode()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var section = BMSTestHelper.CreateBoardSection(buffer);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ShowZones = true;
			section.SectionConfiguration.Subsections = 1;
			section.SectionConfiguration.CellsPerSubsection = 4;

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "MOE", "Man Of Eggs");
			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "FiddlyBits", buffer);
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var bsViewModel = BMSTestHelper.CreateViewModel(section);

			var viewModel = GetViewModel(section.Board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				form.RefreshNow_ForTest(true);
				Application.DoEvents();

				Assert("Assigned task should be in channel", bsViewModel.GetOrCreateChannelForTest(channel, section.Factory).IsInChannel(task, false));

				task.Delete();

				Factory.Save();

				form.EnterBoardMeetingMode();
			}
		}

		public void TestRefreshBoard_ShouldRemoveOverlayControls()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.FillWithValidTestData();
			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(board)))
			using (var label = new ZLabel())
			{
				form.AddOverlayControl(label, Point.Empty, Point.Empty, 0);
				form.RefreshNow_ForTest(forceReload: false);
				Assert(label.IsDisposed);
			}
		}

		public void TestUnchannelledChannelsFilterTasksCorrectly()
		{
			var systemAndBuffer = BMSTestHelper.CreateSystemAndBuffer(Factory);
			systemAndBuffer.Item1.FS_Name = "WTGDEV";
			var board = BMSTestHelper.CreateBoard(systemAndBuffer.Item1, "My board", "Some description");

			var section = BMSTestHelper.CreateBoardSection(systemAndBuffer.Item2, board);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ShowZones = true;
			section.SectionConfiguration.Subsections = 1;
			section.SectionConfiguration.CellsPerSubsection = 4;
			section.SectionConfiguration.ShowUnchanneled = true;
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);
			AssertEquals(2, BMBoardSectionTestHelper.GetPrimaryAxisChannelCount(section));
			var unchannelledChannel = section.SectionConfiguration.PrimaryAxisChannels.Single(c => c.IsUnChanneled);
			unchannelledChannel.MSC_ChannelType = ChannelTypeList.Codes.Resource;
			var viewModel = GetViewModel(board);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "My Workflow", systemAndBuffer.Item2);
			var task1 = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow, capability: BMSTestHelper.CreateCapability(Factory, "ABC", "Dummydummy"));

			Factory.Save();

			var bsViewModel = BMSTestHelper.CreateViewModel(section);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();
				Assert("Assigned task should be in first channel", bsViewModel.GetOrCreateChannelForTest(channel1, section.Factory).IsInChannel(task1));
				Assert("Unassigned task should be in unchanneled channel", bsViewModel.GetOrCreateChannelForTest(unchannelledChannel, section.Factory).IsInChannel(task2));

				task1.P9_GS_NKAssignedStaffMember = null;
				Factory.Save();
				form.RefreshNow_ForTest(forceReload: false);

				Assert("Unassigned task1 should not be in first channel", !bsViewModel.GetOrCreateChannelForTest(channel1, section.Factory).IsInChannel(task1));
				Assert("Unassigned task1 should now be in unchanneled channel", bsViewModel.GetOrCreateChannelForTest(unchannelledChannel, section.Factory).IsInChannel(task1));
				Assert("Unassigned task2 should still be in unchanneled channel", bsViewModel.GetOrCreateChannelForTest(unchannelledChannel, section.Factory).IsInChannel(task2));
			}
		}

		[TestDate(2013, 8, 1, 5, 47, 39)] // Thursday
		[TestUtcOffset(10, 0, 0)]
		public void TestUpdatesHeadingsOnRefresh()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_Name = "WTGDEV";
			var bucket = system.Components.AddNew();
			bucket.FC_Name = "Tim";
			var board = system.Boards.AddNew();
			board.MB_Description = "International Logistics";
			board.MB_Name = "B1";
			var section = board.Sections.AddNew();
			section.MS_FC_Component = bucket.PK;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.OverrideChannels = true;
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);
			var viewModel = GetViewModel(board);

			Factory.Save();

			AssertEquals(resource.WorkStatus, "Working");

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				var headerControls = form.Find(c => c is ChannelHeaderControl);
				AssertEquals(1, headerControls.Count());

				var headerControl = (ChannelHeaderControl)headerControls.First();
				AssertEquals(headerControl.ChannelStatus, "Idle");

				WorkingDaysTestHelper.UpdateStaffDay(Factory, resource.PK, DayOfWeek.Thursday, "");
				Factory.Save();

				AssertEquals(resource.WorkStatus, "Non Work Day");
				AssertEquals("Idle", headerControl.ChannelStatus);

				form.RefreshNow_ForTest(forceReload: false);
				AssertEquals("Away until Fri 2-Aug", headerControl.ChannelStatus);
			}
		}

		public void TestFormClosedWithWindowPersister_WhileScreenLocked_WhenReopened_ShouldConsiderSessionNotLoggedIn()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = GetViewModel(config.BucketBoard);

			Factory.Save();
			var url = string.Empty;

			// Form closed and reopened while session is active

			using (var form = new VisualBoardFormForRefreshTest(viewModel))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("The session should be active by default when the form is opened normally, and yet...", true, form.IsWindowsSessionActive);

				url = WindowPersister.GetOpenFormUrls();
				form.Close();
			}

			var boardForms = BMSFormTestHelper.GetOpenForms<VisualBoardForm>().ToArray();
			AssertEquals(0, boardForms.Length);
			VisualBoardForm loadedForm = null;

			try
			{
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				Application.DoEvents();
				boardForms = BMSFormTestHelper.GetOpenForms<VisualBoardForm>().ToArray();
				AssertEquals(1, boardForms.Length);

				loadedForm = boardForms.Single();
				AssertEquals("The board should consider its session active because it was active when the previous form was closed and saved to the registry, and yet...", true, loadedForm.IsWindowsSessionActive);
			}
			finally
			{
				loadedForm?.Dispose();
			}

			// Form closed and reopened while session is inactive

			using (var form = new VisualBoardFormForRefreshTest(viewModel))
			{
				form.Show();
				Application.DoEvents();

				form.SimulateSessionSwitch(SessionSwitchReason.SessionLock);
				AssertEquals(false, form.IsWindowsSessionActive);

				url = WindowPersister.GetOpenFormUrls();
				form.Close();
			}

			boardForms = BMSFormTestHelper.GetOpenForms<VisualBoardForm>().ToArray();
			AssertEquals(0, boardForms.Length);

			try
			{
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				Application.DoEvents();
				boardForms = BMSFormTestHelper.GetOpenForms<VisualBoardForm>().ToArray();
				AssertEquals(1, boardForms.Length);

				loadedForm = boardForms.Single();
				AssertEquals("The board should consider its session inactive because it was inactive when the previous form was closed and saved to the registry, and yet...", false, loadedForm.IsWindowsSessionActive);
				AssertEquals("The form should not refresh because the session is offline, and yet...", 0, loadedForm.RefreshCount_ForTest);

				loadedForm.IsWindowsSessionActive = true;
				Application.DoEvents();
				AssertEquals("The form should refresh when the session comes online, and yet... ", 1, loadedForm.RefreshCount_ForTest);
			}
			finally
			{
				loadedForm?.Dispose();
			}
		}

		public void TestFormClosedWithWindowPersister_WhileScreenLocked_WhenReopenedAndDisposed_DontRefresh()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = GetViewModel(config.BucketBoard);

			Factory.Save();
			var url = string.Empty;
			using (var form = new VisualBoardFormForRefreshTest(viewModel))
			{
				form.Show();
				Application.DoEvents();
				url = WindowPersister.GetOpenFormUrls();
				form.Close();
			}

			EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
			Application.DoEvents();
			var loadedForm = BMSFormTestHelper.GetOpenForms<VisualBoardForm>().Single();
			loadedForm.IsWindowsSessionActive = false;
			loadedForm.Dispose();
			AssertNoExceptionThrown(() => loadedForm.IsWindowsSessionActive = true);
			AssertEquals(0, loadedForm.RefreshCount_ForTest);
		}

		public void TestFormClosedWithWindowsPersister_WhenReopened_ShouldResumeAutoRefreshTimerWhereItLeftOff()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = GetViewModel(config.BucketBoard);
			Factory.Save();

			string url;

			using (var form = new VisualBoardFormForRefreshTest(viewModel))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(TimeSpan.FromMinutes(10), form.AutoRefreshForTest.TimeUntilRefresh);
				form.AutoRefreshForTest.TimeUntilRefresh = TimeSpan.FromMinutes(2);

				var label = form.FindSingle<ZLabel>(x => x.Name == "SystemUpdateCompleteLabel");
				AssertEquals(false, label.Visible);

				url = WindowPersister.GetOpenFormUrls();
				form.Close();
			}

			var boardForms = BMSFormTestHelper.GetOpenForms<VisualBoardForm>().ToArray();
			AssertEquals(0, boardForms.Length);
			VisualBoardForm loadedForm = null;

			try
			{
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				Application.DoEvents();
				boardForms = BMSFormTestHelper.GetOpenForms<VisualBoardForm>().ToArray();
				AssertEquals(1, boardForms.Length);

				loadedForm = boardForms.Single();
				AssertEquals(TimeSpan.FromMinutes(2), loadedForm.AutoRefreshForTest.TimeUntilRefresh);

				var label = loadedForm.FindSingle<ZLabel>(x => x.Name == "SystemUpdateCompleteLabel");
				AssertEquals(true, label.Visible);

				var wasFormRefreshed = false;
				loadedForm.RefreshStarted += (_, x_) =>
				{
					AssertEquals(false, label.Visible);
					wasFormRefreshed = true;
				};
				loadedForm.SystemUpdateCompleteLabelClick_ForTest();

				const int maxWaitCycles = 10;
				var waitCycles = 0;

				while (!wasFormRefreshed && waitCycles < maxWaitCycles)
				{
					Thread.Sleep(10);
					Application.DoEvents();
					waitCycles++;
				}

				AssertEquals(true, wasFormRefreshed);
			}
			finally
			{
				loadedForm?.Dispose();
			}
		}

		public void TestFormClosedWithWindowsPersister_WhenReopened_WithMissingArg_ShouldNotThrowException()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var viewModel = GetViewModel(config.BucketBoard);
			Factory.Save();

			string url;

			using (var form = new VisualBoardFormForRefreshTest(viewModel))
			{
				form.Show();
				Application.DoEvents();

				form.AutoRefreshForTest.TimeUntilRefresh = TimeSpan.FromMinutes(2);
				url = WindowPersister.GetOpenFormUrls();
				form.Close();
			}

			url = url.Replace("AutoRefresh", "Rararararara");
			VisualBoardForm loadedForm = null;

			try
			{
				EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);
				Application.DoEvents();

				var boardForms = BMSFormTestHelper.GetOpenForms<VisualBoardForm>().ToArray();
				AssertEquals(1, boardForms.Length);

				loadedForm = boardForms.Single();
				AssertEquals("The invalid arg should have been ignored, and yet... This might happen the first time the system upgrades to a version that uses the AutoRefresh arg.", TimeSpan.FromMinutes(10), loadedForm.AutoRefreshForTest.TimeUntilRefresh);
			}
			finally
			{
				loadedForm?.Dispose();
			}
		}

		public void TestWindowsPersisterDoesntTouchTheDbWhenAttemptingToReloadVisualBoardDatabaseUpgradeException()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);
			BMSTestHelper.CreateBoardSection(buffer, board);

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedBoard = newFactory.Load<BMBoard>(board.PK);

			var viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(loadedBoard);

			using (DbEnv.Instance.DisableTimerDuringDbUpgrade())
			using (var form = new VisualBoardFormForRefreshTest(viewModel))
			{
				form.Show();
				Application.DoEvents();

				WindowPersister.SaveUrlsToRegistry();
				OpenedFormCache.GetInstance().CloseAllCachedForms();

				RegistryItemDictionary.Instance.PurgeAll();

				Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = true;

				try
				{
					var expectedHits = new Dictionary<string, int> { };
					using (AssertDbHitsForAllFactories(expectedHits, useOnlyNewFactories: true, includeFactoryPredicate: f => Business.Test.BMSTestHelper.IsPAVEFactory(f), ignoreHitsFromTablesCachedInUberFactory: true))
					{
						newFactory.RelinquishThreadOwnership();
						WindowPersister.GetOpenFormUrls();
					}
				}
				finally
				{
					newFactory.TakeThreadOwnership();
					Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;
				}
			}
		}

		public void TestMoveToComponent_OnJobTicket_ShouldUpdateTicketsAutomatically()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var destinationBucket = BMSTestHelper.CreateBucket(system, "The Forbidden Bucket of Mystery");
			var additionalBuffer = BMSTestHelper.CreateBuffer(system, "The Void Beyond");
			var doneBucket = BMSTestHelper.CreateBucket(system, "Ducket");

			var board = BMSTestHelper.CreateBoard(system);
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			section.SectionConfiguration.CellsPerSubsection = 13;
			BMSTestHelper.CreateAdditionalComponent(section, additionalBuffer);
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);
			var viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(board);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "This is the whole job.");
			var workflowToMove1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow to move 1", buffer, releaseDateTime: ZDateTime.Now.AddDays(-2), staffCode: resource.GS_Code);
			var workflowToMove2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow to move 2", buffer, releaseDateTime: ZDateTime.Now.AddDays(-9), staffCode: resource.GS_Code);
			var closedWorkflow = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Closed Workflow", doneBucket, staffCode: resource.GS_Code);
			var workflowInOtherComponent = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow in other component", additionalBuffer, staffCode: resource.GS_Code);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "A different job.");
			var workflowInJob2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader2, "Workflow in job 2", buffer, releaseDateTime: ZDateTime.Now.AddDays(-2), staffCode: resource.GS_Code);

			Factory.Save();

			using (var form = new VisualBoardFormForRefreshTest(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var tickets = form.FindAll<TaskCardControl>().Where(t => t.Visible).ToArray();
				AssertEquals(2, tickets.Length);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var jobToMoveTicket = tickets.Single(x => x.CardContent.WorkflowIdentifier == jobHeader.PK);
				jobToMoveTicket.MoveToComponent_ForTest(destinationBucket);
				Application.DoEvents();

				AssertEquals("Are you sure to move 3 workflows in job [Organization (XVBQP68SIYXQ) - This is the whole job.] to component [The Forbidden Bucket of Mystery]? Note that based on transfer rules, your workflows may move back to their original components.", UnitTestUserNotification.Instance.LastMessage.Text);
				tickets = form.FindAll<TaskCardControl>().Where(t => t.Visible).ToArray();
				AssertEquals("The job should have been removed from the board leaving the other job, and yet...", 1, tickets.Length);
				var remainingJobTicket = tickets.Single();
				AssertEquals(jobHeader2.PK, remainingJobTicket.CardContent.WorkflowIdentifier);
				AssertEquals(buffer.FC_Name, workflowInJob2.CurrentComponent.FC_Name);

				AssertEquals(destinationBucket.FC_Name, workflowToMove1.CurrentComponent.FC_Name);
				AssertEquals(destinationBucket.FC_Name, workflowToMove2.CurrentComponent.FC_Name);
				AssertEquals("The workflow was in a component not shown on ", doneBucket.FC_Name, closedWorkflow.CurrentComponent.FC_Name);
				AssertEquals(destinationBucket.FC_Name, workflowInOtherComponent.CurrentComponent.FC_Name);

				remainingJobTicket.MoveToComponent_ForTest(buffer);
				Application.DoEvents();
				AssertEquals("The destination component cannot be the same as the source component.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

#if !WINZOR
		#region Session Switching Monitor

		public void TestRefresh_ShouldWaitUntilFormVisible()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_Name = "WTGDEV";
			var bucket = system.Components.AddNew();
			bucket.FC_Name = "Trampoline Inspection Schedule";
			var board = system.Boards.AddNew();
			board.MB_Description = "International Logistics";
			board.MB_Name = "B1";
			var section = board.Sections.AddNew();
			section.MS_FC_Component = bucket.PK;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.OverrideChannels = true;
			var viewModel = GetViewModel(board);

			viewModel.RefreshSeconds = 2;
			var boardRefreshCount = 0;

			Factory.Save();

			using (var form = new VisualBoardFormForRefreshTest(viewModel))
			{
				form.RefreshFinished += (s, e) =>
				{
					boardRefreshCount++;
				};

				form.Show();

				var countdownLabel = form.controlsPanel.CountdownLabel;

				DoAsyncSynchronously_ForTest();

				AssertEquals("WHEN board is shown, it should refresh automatically", 1, boardRefreshCount);

				PulseAndDoEvents(form.AutoRefreshForTest, 2);

				AssertEquals("GIVEN form is not minimised and session is logged in, WHEN waiting for 2 seconds, board should refresh", 2, boardRefreshCount);

				var originalWindowState = form.WindowState;
				form.WindowState = FormWindowState.Minimized;

				PulseAndDoEvents(form.AutoRefreshForTest, 2);

				AssertEquals("GIVEN form is minimised, WHEN waiting for 2 seconds, form should not refresh", 2, boardRefreshCount);
				AssertEquals("GIVEN form is minimised, WHEN waiting for 2 seconds, clock should stay at 0", "00:00", countdownLabel.Text);

				form.WindowState = originalWindowState;
				form.RefreshBoard();
				DoAsyncSynchronously_ForTest();

				AssertEquals("GIVEN form is shown, WHEN refresh, it should refresh", 3, boardRefreshCount);
				AssertNotEquals("WHEN board is refreshed, clock value should not be 0", "00:00", countdownLabel.Text);
				AssertEquals("WHEN form is refreshed, clock should resume back to positive value", false, countdownLabel.Text.Contains('-'));

				form.SimulateSessionSwitch(SessionSwitchReason.SessionLock);
				PulseAndDoEvents(form.AutoRefreshForTest, 2);

				AssertEquals("WHEN session is not logged in, form should not refresh", 3, boardRefreshCount);
				AssertEquals("WHEN session is not logged in, clock value should stay at 0", "00:00", countdownLabel.Text);

				form.SimulateSessionSwitch(SessionSwitchReason.SessionUnlock);
				form.RefreshBoard();
				DoAsyncSynchronously_ForTest();

				AssertEquals("WHEN session is active again, form should refresh", 4, boardRefreshCount);
				AssertNotEquals("WHEN form is refreshed, clock should update", "00:00", countdownLabel.Text);

				form.SetRefreshDelayAndRestartTimer(TimeSpan.FromSeconds(60));
				form.WindowState = FormWindowState.Minimized;
				Application.DoEvents();
				form.WindowState = originalWindowState;
				Application.DoEvents();

				PulseAndDoEvents(form.AutoRefreshForTest, 2);

				AssertEquals("GIVEN Refresh-Delay=1-minute, WHEN form is restored from minimised state and wait for < 1 minute, form shouldn't automatically refresh", 4, boardRefreshCount);
				AssertNotEquals("WHEN form is not refreshed, clock should not reset to original RefreshTime value", "01:00", countdownLabel.Text);
				AssertNotEquals("WHEN form is not refreshed, clock should not automatically jump down to 0", "00:00", countdownLabel.Text);
			}
		}

		public void TestSessionSwitchHandling_ShouldRegisterClientAndServiceSessionSwitch_WhenIsUsingRemoteDesktopService()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();

			var viewModel = GetViewModel(board);

			Factory.Save();

			var enterpriseChannelMock = new Mock<EnterpriseChannel>();
			enterpriseChannelMock.Setup(m => m.IsConnected).Returns(true);

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardFormForRefreshTest(viewModel))
			{
				form.SetEnterpriseChannel(enterpriseChannelMock.Object);
				form.Show();
				Application.DoEvents();

				AssertEquals(1, enterpriseChannelMock.Object.ClientSessionSwitch.GetInvocationList().Length);
				AssertEquals(1, enterpriseChannelMock.Object.ServerSessionSwitch.GetInvocationList().Length);
			}

			AssertNull(enterpriseChannelMock.Object.ClientSessionSwitch);
			AssertNull(enterpriseChannelMock.Object.ServerSessionSwitch);
		}

		public void TestIsSessionLoggedIn()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			var viewModel = GetViewModel(board);

			using (var form = new VisualBoardFormForRefreshTest(viewModel))
			{
				AssertEquals("The session should be considered active by default, and yet...", true, form.IsWindowsSessionActive);

				var offlineTypes = new[] { SessionSwitchReason.SessionLock, SessionSwitchReason.ConsoleDisconnect, SessionSwitchReason.RemoteDisconnect, SessionSwitchReason.SessionLogoff };

				foreach (var type in offlineTypes)
				{
					form.SimulateSessionSwitch(SessionSwitchReason.ConsoleConnect);
					AssertEquals(true, form.IsWindowsSessionActive);

					form.SimulateSessionSwitch(type);
					AssertEquals(false, form.IsWindowsSessionActive);
				}

				var otherTypes = Enum.GetValues(typeof(SessionSwitchReason)).Cast<SessionSwitchReason>().Where(x => !offlineTypes.Contains(x)).ToArray();
				AssertEquals(true, otherTypes.Any());

				foreach (var type in otherTypes)
				{
					form.SimulateSessionSwitch(SessionSwitchReason.ConsoleDisconnect);
					AssertEquals(false, form.IsWindowsSessionActive);

					form.SimulateSessionSwitch(type);
					AssertEquals(true, form.IsWindowsSessionActive);
				}
			}
		}

		public void TestMultipleVisualBoardForms_ShouldShareTheSameSessionSwitchMonitor()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var board1 = config.BufferBoard;
			var board2 = config.BucketBoard;

			Factory.Save();

			using (var form1 = GetAndShowVisualBoardForm(board1))
			using (var form2 = GetAndShowVisualBoardForm(board2))
			{
				var monitor1 = form1.SessionChangeMonitor_ExposedForTest;
				var monitor2 = form2.SessionChangeMonitor_ExposedForTest;

				AssertNotNull(monitor1);
				AssertNotNull(monitor2);

				AssertEquals("All boards should now share the same session switch monitor, so that we don't have to create so many damn window handles. SAD!", monitor1, monitor2);
			}
		}

		public void TestMultipleVisualBoardForms_WhenOneFormClosed_ShouldRemoveSessionSwitchCallbackFromMonitor_AndNotCloseMonitor()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var board1 = config.BufferBoard;
			var board2 = config.BucketBoard;

			Factory.Save();

			using (var form1 = GetAndShowVisualBoardForm(board1))
			using (var form2 = GetAndShowVisualBoardForm(board2))
			{
				var monitor = form1.SessionChangeMonitor_ExposedForTest;
				AssertEquals("There are 2 visual board forms open, so there should be 2 callbacks. SAD!", 2, monitor.CallbackCount);

				form2.Close();
				Application.DoEvents();

				monitor = form1.SessionChangeMonitor_ExposedForTest;
				AssertNotNull("There's still a visual board open, so the monitor should not yet be closed. SAD!", monitor);
				AssertEquals("There's still a visual board open, so the monitor should not yet be closed. SAD!", false, monitor.IsClosed);
				AssertEquals("One form was closed, so one of the callbacks should have been removed. SAD!", 1, monitor.CallbackCount);

				monitor = form2.SessionChangeMonitor_ExposedForTest;
				AssertNull("The form is closed, so its reference to the monitor should have been removed. SAD!", monitor);
			}
		}

		public void TestSessionSwitchMonitor_WhenLastFormClosed_ShouldCloseSessionSwitchMonitor()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var board = config.BufferBoard;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var monitor = form.SessionChangeMonitor_ExposedForTest;
				AssertEquals("There is one visual board open, so there should be one callback. SAD!", 1, monitor.CallbackCount);
				AssertEquals("The monitor should not be closed while the form is running. SAD!", false, monitor.IsClosed);

				form.Close();
				Application.DoEvents();

				AssertEquals("The last form is closed, so the last callback should have been removed. SAD!", 0, monitor.CallbackCount);
				AssertEquals("The last form is closed, so the monitor should be closed too. SAD!", true, monitor.IsClosed);

				monitor = form.SessionChangeMonitor_ExposedForTest;
				AssertNull("The form is closed, so its reference to the monitor should have been removed. SAD!", monitor);
			}
		}

		#endregion
#endif

		#region Refresh Strip

		public void TestRefreshStripLocationChanged()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.FillWithValidTestData();
			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(board)))
			{
				form.Size = ControlDpiScalingHelper.NewScaledSize(100, 50);
				form.Show();

				var originalButtonLocation = form.controlsPanel.Location;

				form.controlsPanel.Location = new Point(form.controlsPanel.Location.X - form.Width, form.controlsPanel.Location.Y);
				AssertEquals(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(form.ClientRectangle.Width) - 3 - ControlDpiScalingHelper.UnscaleFromCurrentDpiX(form.controlsPanel.Width), ControlDpiScalingHelper.UnscaleFromCurrentDpiX(form.controlsPanel.Location.X));
				AssertEquals(originalButtonLocation.Y, form.controlsPanel.Location.Y);
				form.controlsPanel.Location = originalButtonLocation;

				form.controlsPanel.Location = new Point(form.controlsPanel.Location.X + form.Width, form.controlsPanel.Location.Y);
				AssertEquals(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(form.ClientRectangle.Width) - 3 - ControlDpiScalingHelper.UnscaleFromCurrentDpiX(form.controlsPanel.Width), ControlDpiScalingHelper.UnscaleFromCurrentDpiX(form.controlsPanel.Location.X));
				AssertEquals(originalButtonLocation.Y, form.controlsPanel.Location.Y);

				form.Size = ControlDpiScalingHelper.NewScaledSize(800, 600);
				AssertEquals(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(form.ClientRectangle.Width) - 3 - ControlDpiScalingHelper.UnscaleFromCurrentDpiX(form.controlsPanel.Width), ControlDpiScalingHelper.UnscaleFromCurrentDpiX(form.controlsPanel.Location.X));
			}
		}

		public void TestRefreshStripLocationChangedIgnoreZeroWidthAndHeight()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.FillWithValidTestData();
			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(board)))
			{
				form.Size = new Size(0, 0);
				form.MinimumSize = new Size(0, 0);
				form.Height = 0;
				form.Width = 0;

				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		#endregion

		#region Reload

		[TestDate(2014, 12, 16, 12, 0, 0)]
		public void TestSectionChangesOnReloadBoard()
		{
			void AssertConfigChangesReloadBoardByShiftF5(VisualBoardForm form)
			{
				var componentControl = form.FindAll<BMComponentControl>().Single();
				BMSFormTestHelper.PressHotkeys(form, Keys.F5);
				form.AwaitAll();

				AssertEquals(componentControl, form.FindAll<BMComponentControl>().Single());

				BMSFormTestHelper.PressHotkeys(form, Keys.F5 | Keys.Shift);
				form.AwaitAll();

				var newComponentControl = form.FindAll<BMComponentControl>().Single();
				AssertNotEquals(componentControl, newComponentControl);

				BMSFormTestHelper.PressHotkeys(form, Keys.F5 | Keys.Shift);
				form.AwaitAll();

				var newerComponentControl = form.FindAll<BMComponentControl>().Single();
				AssertNotEquals(newComponentControl, newerComponentControl);
			}

			Factory.RefreshEnabled = false;
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var section = BMSTestHelper.CreateBoardSection(BMSTestHelper.CreateBucket(system));
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.Subsections = 1;

			Factory.Save();

			using (VisualBoardsTestCase.ApplyAsyncStrategy(new MockAsyncStrategy()))
			using (var form = VisualBoardFormDisplayer.ShowBoard(section.Board))
			{
				form.AwaitAll();
				section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Left;
				Factory.Save();

				AssertConfigChangesReloadBoardByShiftF5(form);

				section.SectionConfiguration.ShowUnchanneled = true;
				Factory.Save();

				AssertConfigChangesReloadBoardByShiftF5(form);
			}
		}

		public void TestRefreshBoardWithInvalidChannel_ShouldNotReloadBoard()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "pig";

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup.GG_Desc = "porcines";
			system.ReleaseGroups.AddNew().FSG_GG_Group = releaseGroup.PK;

			var buffer = BMSTestHelper.CreateBuffer(system, "trough");
			var board = BMSTestHelper.CreateBoard(system, "sty");
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.ReleaseGroupPK = releaseGroup.PK;

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BAB", "Babe");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "WLB", "Wilbur");
			var resourceInvalid = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "GUI", "Guinea");

			releaseGroup.Staff.AddRange(resource1, resource2);

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			var channel2 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);
			var channelInvalid = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resourceInvalid.PK);

			section.SectionConfiguration.OverrideChannels = false;

			Factory.Save();

			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(board))
			{
				AssertEquals("Only resources in the release group should appear on the board", false, section.SectionConfiguration.OverrideChannels);

				var startingComponentControl = form.FindSingle<BMComponentControl>();
				AssertEquals(2, startingComponentControl.ViewModel.PrimaryChannels.Count());

				form.ExpandButtonPanelAndClickRefreshButton();
				Application.DoEvents();

				AssertEquals("Refresh -- Board should NOT have been reloaded, so the section layout should NOT have been recreated.",
					startingComponentControl, form.FindSingle<BMComponentControl>());
			}
		}

		public void TestRefreshBoardWithDefaultChannelsAdded_ShouldNotReloadBoard()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "pig";

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "STS", "Straw Straughan");

			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup.GG_Desc = "diminutive porcines";
			releaseGroup.Staff.Add(resource);
			system.ReleaseGroups.AddNew().FSG_GG_Group = releaseGroup.PK;

			var buffer = BMSTestHelper.CreateBuffer(system, "trough");
			var board = BMSTestHelper.CreateBoard(system, "sty");
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			section.SectionConfiguration.ReleaseGroupPK = releaseGroup.PK;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;

			var channel1 = BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);

			Factory.Save();

			using (var form = BMSGUITestCase.GetAndShowVisualBoardForm(board))
			{
				var startingComponentControl = form.FindSingle<BMComponentControl>();
				AssertEquals(1, startingComponentControl.ViewModel.PrimaryChannels.Count());

				form.ExpandButtonPanelAndClickRefreshButton();
				Application.DoEvents();

				AssertEquals("Refresh -- Board should NOT have been reloaded, so the section layout should NOT have been recreated.",
					startingComponentControl, form.FindSingle<BMComponentControl>());
			}
		}

		public void TestRefresh_DeletedBoardHandled()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "DeleteTest";
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

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				bool formIsClosed = false;
				form.FormClosed += (s, e) => formIsClosed = true;
				Application.DoEvents();

				slideshow.BoardPivots.FirstOrDefault(x => x.MC_MB_Board == board1.PK).Delete();
				board1.Delete();
				Factory.Save();

				Assert(!board1.IsInDatabase);
				form.RefreshNow_ForTest(false);

				Application.DoEvents();
				AssertEquals("Only two slides should remain", 2, viewModel.BoardViewModels.Length);

				slideshow.BoardPivots.FirstOrDefault(x => x.MC_MB_Board == board2.PK).Delete();
				slideshow.BoardPivots.FirstOrDefault(x => x.MC_MB_Board == board3.PK).Delete();
				board2.Delete();
				board3.Delete();
				Factory.Save();

				Assert(!board2.IsInDatabase);
				Assert(!board3.IsInDatabase);

				form.RefreshNow_ForTest(false);
				Application.DoEvents();

				Assert("Form should be closed when the board is deleted", formIsClosed);
				AssertEquals("The board you were looking at no longer exists.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSectionConfigChangesReloadBoard()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			var buffer = BMSTestHelper.CreateBucket(system);
			var board = system.Boards.AddNew();
			var section = BMSTestHelper.CreateBoardSection(buffer, board);
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;

			var channel = BMSTestHelper.CreatePrimaryChannelForSection(section);
			channel.MSC_ChannelType = ChannelTypeList.Codes.Capability;
			channel.MSC_ParentID = capability.PK;
			Factory.Save();

			var viewModel = GetViewModel(board);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Left;
				Factory.Save();

				AssertConfigChangesReloadBoard(form);

				section.ForegroundColor = "Pink";
				Factory.Save();

				AssertConfigChangesReloadBoard(form);

				section.SectionConfiguration.ShowUnchanneled = true;
				section.SectionConfiguration.PrimaryAxisChannels[1].MSC_ChannelType = ChannelTypeList.Codes.Capability;
				Factory.Save();

				AssertConfigChangesReloadBoard(form);
			}
		}

		void AssertConfigChangesReloadBoard(VisualBoardForm form)
		{
			var componentControl = GetComponentControl(form);

			form.RefreshNow_ForTest(forceReload: false);
			var newComponentControl = GetComponentControl(form);
			AssertNotEquals(componentControl, newComponentControl);
		}

		BMComponentControl GetComponentControl(VisualBoardForm form)
		{
			return form.FindAll<BMComponentControl>().Single();
		}

		public void TestBoardConfigurationDialogClose_ReloadRequired()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			section.BackgroundColor = Color.Blue.Name;

			Factory.Save();

			var viewModel = GetViewModel(section.Board);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				form.controlsPanel.Expand();
				AssertColorEquals(Color.Blue, form.FindAll<TaskPanel>().Single().BackColor);

				var configButton = form.controlsPanel.ConfigButton;
				configButton.PerformClick();
				Application.DoEvents();

				var boardForm = (ZForm)ZFormModaliser.LastFormShownForTest;

				section.BackgroundColor = Color.Red.Name;
				Factory.Save();
				Application.DoEvents();

				AssertColorEquals(Color.Blue, form.FindAll<TaskPanel>().Single().BackColor);

				boardForm.Close();
				Application.DoEvents();

				AssertColorEquals(Color.Red, form.FindAll<TaskPanel>().Single().BackColor);
			}
		}

		public void TestBoardConfigurationDialogClose_NoReloadRequired()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);

			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.BackgroundColor = Color.Blue.Name;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			Factory.Save();

			var viewModel = GetViewModel(section.Board);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				form.controlsPanel.Expand();
				AssertColorEquals(Color.Blue, form.FindAll<TaskPanel>().Single().BackColor);

				var configButton = form.controlsPanel.ConfigButton;
				configButton.PerformClick();
				Application.DoEvents();

				var boardForm = (ZForm)ZFormModaliser.LastFormShownForTest;

				Factory.Save();
				Application.DoEvents();

				AssertColorEquals(Color.Blue, form.FindAll<TaskPanel>().Single().BackColor);

				boardForm.Close();
				Application.DoEvents();

				AssertColorEquals(Color.Blue, form.FindAll<TaskPanel>().Single().BackColor);
			}
		}

		public void TestBoardConfigurationDialogClose_InBoardMeetingMode_ReloadRequired()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.BackgroundColor = Color.Blue.Name;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			Factory.Save();

			var viewModel = GetViewModel(section.Board);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				form.controlsPanel.Expand();
				form.EnterBoardMeetingMode();

				var boardMeetingModeForm = Application.OpenForms.OfType<BoardMeetingModeForm>().Single();

				AssertEquals(true, form.IsInBoardMeeting);
				AssertColorEquals(Color.Blue, form.FindAll<TaskPanel>().Single().BackColor);

				var configButton = form.controlsPanel.ConfigButton;
				configButton.PerformClick();
				Application.DoEvents();

				var boardEditForm = (ZForm)ZFormModaliser.LastFormShownForTest;

				section.BackgroundColor = Color.Red.Name;
				Factory.Save();
				Application.DoEvents();

				AssertColorEquals(Color.Blue, form.FindAll<TaskPanel>().Single().BackColor);
				AssertEquals(false, boardMeetingModeForm.IsDisposed);

				boardEditForm.Close();
				Application.DoEvents();

				AssertColorEquals(Color.Red, form.FindAll<TaskPanel>().Single().BackColor);
				AssertEquals(false, form.IsInBoardMeeting);
				AssertEquals(true, boardMeetingModeForm.IsDisposed);
			}
		}

		public void TestBoardConfigurationDialogClose_InBoardMeetingMode_NoReloadRequired()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.BackgroundColor = Color.Blue.Name;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			Factory.Save();

			var viewModel = GetViewModel(section.Board);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				form.controlsPanel.Expand();
				form.EnterBoardMeetingMode();
				AssertEquals(true, form.IsInBoardMeeting);
				AssertColorEquals(Color.Blue, form.FindAll<TaskPanel>().Single().BackColor);

				var configButton = form.controlsPanel.ConfigButton;
				configButton.PerformClick();
				Application.DoEvents();

				var boardForm = (ZForm)ZFormModaliser.LastFormShownForTest;

				Factory.Save();
				Application.DoEvents();

				AssertColorEquals(Color.Blue, form.FindAll<TaskPanel>().Single().BackColor);

				boardForm.Close();
				Application.DoEvents();

				AssertColorEquals(Color.Blue, form.FindAll<TaskPanel>().Single().BackColor);
				AssertEquals(true, form.IsInBoardMeeting);
			}
		}

		public void TestBoardMeetingButton_AppearsOnlyInBoardMeetingMode()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.BackgroundColor = Color.Blue.Name;

			Factory.Save();

			var viewModel = GetViewModel(section.Board);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Assert("Board Meeting mode button is invisible", !form.controlsPanel.BoardMeetingButton.Visible);
				form.EnterBoardMeetingMode();
				AssertEquals(true, form.IsInBoardMeeting);
				Assert("Board Meeting mode button is now visible", form.controlsPanel.BoardMeetingButton.Visible);
			}
		}

		public void TestBoardReload_InBoardMeetingMode()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);

			Factory.Save();

			var viewModel = GetViewModel(section.Board);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.EnterBoardMeetingMode();
				AssertEquals(true, form.IsInBoardMeeting);
				AssertEquals(1, form.SlideShowViewModel.FilterManager.AppliedFilters.Count());
				Assert("Board Meeting mode button is visible", form.controlsPanel.BoardMeetingButton.Visible);

				form.ReloadBoard();

				AssertEquals(false, form.IsInBoardMeeting);
				AssertEquals(0, form.SlideShowViewModel.FilterManager.AppliedFilters.Count());
				Assert("Board Meeting mode button is not visible", !form.controlsPanel.BoardMeetingButton.Visible);
				Assert("Filter button is not visible", !form.controlsPanel.FilterButton.Visible);
			}
		}

		public void TestBoardRefreshOnShortcut_InBoardMeetingMode()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);

			Factory.Save();

			var viewModel = GetViewModel(section.Board);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.EnterBoardMeetingMode();
				AssertEquals(true, form.IsInBoardMeeting);

				var refreshStarted = false;
				form.RefreshStarted += delegate
				{
					refreshStarted = true;
				};

				BMSFormTestHelper.PressHotkeys(form, Keys.F5);
				Application.DoEvents();

				AssertEquals(true, form.IsInBoardMeeting);
				AssertEquals(true, refreshStarted);
			}
		}

		public void TestBoardRefreshOnShortcut_InSlideShowMode()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system, "bucket", offsetMinutes: 0, sequence: 0);

			var board1 = BMSTestHelper.CreateBoard(system, "board1", "board1");
			var board2 = BMSTestHelper.CreateBoard(system, "board2", "board2");

			var section1 = BMSTestHelper.CreateBoardSection(bucket, board1);
			var section2 = BMSTestHelper.CreateBoardSection(bucket, board2);
			section1.BackgroundColor = "Blue";
			section2.BackgroundColor = "HotPink";

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);
			viewModel.RefreshSeconds = 1000;

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				AssertEquals("The first board should be shown when the slideshow loads, and yet...", board1.PK, form.SlideShowViewModel.CurrentBoardViewModel.BoardPK);

				var refreshStarted = false;
				form.RefreshStarted += delegate
				{
					refreshStarted = true;
				};

				BMSFormTestHelper.PressHotkeys(form, Keys.F5);

				Application.DoEvents();

				AssertEquals(true, refreshStarted);
				AssertEquals("The first board should still be shown when the user presses F5 to refresh (rather than advancing to the next slide). And yet...", board1.PK, form.SlideShowViewModel.CurrentBoardViewModel.BoardPK);
			}
		}

		int HeadersWithText(Control form, string text)
		{
			return form.Find(control => control is ChannelHeaderControl).Count(control => control.Find(child => child.Text == text).Any());
		}

		public void TestDeleteSectionOnVisualBoard()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var acceptabilityBand = BMSTestHelper.CreateAcceptabilityBand_WorkflowsInComponent(bucket, 10, 12, 14, 16, 18, 21);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			var section2 = BMSTestHelper.CreateBoardSection(bucket, section.Board);

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();

				section.Delete();
				Factory.Save();

				AssertNoExceptionThrown("Sections should be filtered out during reload", () => form.RefreshNow_ForTest(forceReload: true));
			}
		}

		#endregion

		#region Configuration

		[TestDate(2016, 8, 31)]
		public void TestTaskCardMoves_WhenSingleWorkflowIsUpdated()
		{
			var staff = CreateStaffInCurrentBranchDept("ARM", "Aaaahh!!! Real Monsters");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");

			var system = BMSTestHelper.CreateSystem(Factory, "DRP");
			var bucket = BMSTestHelper.CreateBucket(system, "bouquet");

			var board = BMSTestHelper.CreateBoard(system, "bored");
			var section = BMSTestHelper.CreateBoardSection(bucket, board);
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;
			section.SectionConfiguration.CellsPerSubsection = 10;
			section.SectionConfiguration.TimePerCell = new ZInt(8 * 60).GetDateTimeFromMinutes();
			section.SectionConfiguration.TimeProgressionMode = TimeProgressionModeList.Codes.Due;
			section.SectionConfiguration.TimeField = TimeProgressionFieldList.Codes.DoNotStartBeforeDate;
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var otherFactory = new BusinessObjectFactory();
			var jobHeader = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(otherFactory);
			var workflow1 = CreateWorkflow(jobHeader, "Ickis", bucket, releaseGroupPK: group.PK);
			var workflow2 = CreateWorkflow(jobHeader, "Oblina", bucket, releaseGroupPK: group.PK);
			var workflow3 = CreateWorkflow(jobHeader, "Krumm", bucket, releaseGroupPK: group.PK);
			var workflow4 = CreateWorkflow(jobHeader, "Gromble", bucket, releaseGroupPK: group.PK);
			var workflow5 = CreateWorkflow(jobHeader, "Zimbo", bucket, releaseGroupPK: group.PK);

			CreateTask(workflow1, staff.GS_Code, 35, description: "1");
			CreateTask(workflow2, staff.GS_Code, 35, description: "2");
			CreateTask(workflow3, staff.GS_Code, 35, description: "3");
			CreateTask(workflow4, staff.GS_Code, 35, description: "4");
			CreateTask(workflow5, staff.GS_Code, 35, description: "5");

			var vm = VisualBoardsTestHelper.CreateSlideshowViewModel(board);

			Factory.Save();
			otherFactory.Save();

			BMSRegistry.Instance.UpdateTicketsWithDataRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var form = new VisualBoardForm(vm))
			{
				form.Show();
				Application.DoEvents();

				var workflowTickets = form.FindAll<TaskCardControl>().Where(t => t.Visible && t.CardContent.CardType == CardType.Workflow);
				AssertEquals("Initial card position", 5, workflowTickets.Count(t => t.Cell.Row == 9));

				workflow1.FH_DoNotStartBeforeDate = ZDateTime.Now.AddDays(1);
				otherFactory.Save();

				Application.DoEvents();

				workflowTickets = form.FindAll<TaskCardControl>().Where(t => t.Visible && t.CardContent.CardType == CardType.Workflow);

				AssertEquals(5, workflowTickets.Count());
				AssertEquals("Card should NOT move when data refresh is turned OFF", 5, workflowTickets.Count(t => t.Cell.Row == 9));
				AssertEquals("Card should NOT move when data refresh is turned OFF", 0, workflowTickets.Count(t => t.Cell.Row == 3 && t.CardContent.WorkflowIdentifier == workflow1.PK));
			}

			BMSRegistry.Instance.UpdateTicketsWithDataRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var form = new VisualBoardForm(vm))
			{
				form.Show();
				Application.DoEvents();

				workflow1.FH_DoNotStartBeforeDate = ZDateTime.Now.AddDays(1);
				otherFactory.Save();

				Application.DoEvents();

				var workflowTickets = form.FindAll<TaskCardControl>().Where(t => t.Visible && t.CardContent.CardType == CardType.Workflow);

				AssertEquals("Card should move when data refresh is turned ON", 4, workflowTickets.Count(t => t.Cell.Row == 9));
				AssertEquals("Card should move when data refresh is turned ON", 1, workflowTickets.Count(t => t.Cell.Row == 6 && t.CardContent.WorkflowIdentifier == workflow1.PK));
			}
		}

		public void TestWorkflowCardsAppearInAllChannels()
		{
			var s1 = CreateStaffInCurrentBranchDept("AG!", "Aching Gorillas");
			var s2 = CreateStaffInCurrentBranchDept("AQ!", "Aching Queen");
			var s3 = CreateStaffInCurrentBranchDept("AA!", "Aching Anthill");

			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);
			var board = CreateBoard(system);
			var section = CreateBoardSection(bucket, board);
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, s1.PK, true);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, s2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, s3.PK);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "Shoo", bucket);
			CreateTask(workflow, s1.GS_Code, 35);
			CreateTask(workflow, s2.GS_Code, 35);
			CreateTask(workflow, s3.GS_Code, 35);

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Workflow cards should appear in all channels.", 3, form.FindAll<TaskCardControl>().Count(t => t.Visible && t.CardContent.CardType == CardType.Workflow));
			}
		}

		#region Open Board Config

		public void TestShouldOpenBoardConfigEditForm_WhenTheUserIsOwner()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GS_NKStaffCode = staff.GS_Code;

			Factory.Save();

			AssertClickingEditButtonOpensBoardConfigForm(ODisplayMode.Browse, board, staff, allowStaffToView: false, allowStaffToEdit: false, allowStaffToEditTeamBoards: false);
		}

		public void TestShouldOpenBoardConfigEditForm_WhenTheUserIsNotOwner_ButCanEditAllBoards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GS_NKStaffCode = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			Factory.Save();

			AssertClickingEditButtonOpensBoardConfigForm(ODisplayMode.Browse, board, staff, allowStaffToView: false, allowStaffToEdit: true, allowStaffToEditTeamBoards: false);
		}

		public void TestShouldOpenBoardConfigViewForm_WhenTheUserIsNotOwner_AndCannotEditAllBoards_ButCanViewBoards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GS_NKStaffCode = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			Factory.Save();

			AssertClickingEditButtonOpensBoardConfigForm(ODisplayMode.ReadOnly, board, staff, allowStaffToView: true, allowStaffToEdit: false, allowStaffToEditTeamBoards: false);
		}

		public void TestShouldInformAboutInsufficientBoardViewRights_WhenTheUserIsNotOwner_AndCannotEditAllBoards_AndCannotViewBoards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GS_NKStaffCode = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			Factory.Save();

			AssertClickingEditButtonDisplaysMessageAboutInsufficientBoardViewRights(board, staff, allowStaffToView: false, allowStaffToEdit: false, allowStaffToEditTeamBoards: false);
		}

		public void TestShouldOpenBoardConfigEditForm_WhenTheUserIsWithinBoardReleaseGroup_AndHasRightToEditTeamBoards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GG_ReleaseGroup = group.PK;

			Factory.Save();

			AssertClickingEditButtonOpensBoardConfigForm(ODisplayMode.Browse, board, staff, allowStaffToView: false, allowStaffToEdit: false, allowStaffToEditTeamBoards: true);
		}

		public void TestShouldOpenBoardConfigViewForm_WhenTheUserIsWithinBoardReleaseGroup_ButHasNoRightToEditTeamBoards_ButStillCanViewBoards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GG_ReleaseGroup = group.PK;

			Factory.Save();

			AssertClickingEditButtonOpensBoardConfigForm(ODisplayMode.ReadOnly, board, staff, allowStaffToView: true, allowStaffToEdit: false, allowStaffToEditTeamBoards: false);
		}

		public void TestShouldInformAboutInsufficientBoardViewRights_WhenTheUserIsWithinBoardReleaseGroup_ButHasNoRightToEditTeamBoards_AndCannotViewBoards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GG_ReleaseGroup = group.PK;

			Factory.Save();

			AssertClickingEditButtonDisplaysMessageAboutInsufficientBoardViewRights(board, staff, allowStaffToView: false, allowStaffToEdit: false, allowStaffToEditTeamBoards: false);
		}

		public void TestShouldOpenBoardConfigViewForm_WhenTheUserHasRightToEditTeamBoards_ButIsNotWithinBoardReleaseGroup_ButStillCanViewBoards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GG_ReleaseGroup = Factory.NewWithValidTestData<GlbGroup>().PK;

			Factory.Save();

			AssertClickingEditButtonOpensBoardConfigForm(ODisplayMode.ReadOnly, board, staff, allowStaffToView: true, allowStaffToEdit: false, allowStaffToEditTeamBoards: true);
		}

		public void TestShouldInformAboutInsufficientBoardViewRights_WhenTheUserHasRightToEditTeamBoards_ButIsNotWithinBoardReleaseGroup_AndCannotViewBoards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GG_ReleaseGroup = Factory.NewWithValidTestData<GlbGroup>().PK;

			Factory.Save();

			AssertClickingEditButtonDisplaysMessageAboutInsufficientBoardViewRights(board, staff, allowStaffToView: false, allowStaffToEdit: false, allowStaffToEditTeamBoards: true);
		}

		void AssertClickingEditButtonOpensBoardConfigForm(ODisplayMode expectedDisplayMode, BMBoard board, GlbStaff staff, bool allowStaffToView, bool allowStaffToEdit, bool allowStaffToEditTeamBoards)
		{
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var form = new VisualBoardForm(GetViewModel(board)))
			{
				Env.Security.BMBoardView.IsAllowed = allowStaffToView;
				Env.Security.BMBoardEdit.IsAllowed = allowStaffToEdit;
				Env.Security.BMBoardEditTeamBoards.IsAllowed = allowStaffToEditTeamBoards;

				form.Show();
				Application.DoEvents();
				ZFormModaliser.ShowDialogsInTest = true;

				form.controlsPanel.Expand();
				form.controlsPanel.ConfigButton.PerformClick();

				var lastformShown = ZFormModaliser.LastFormShownForTest as ZForm;
				AssertNotNull(lastformShown);
				AssertEquals($"Should be of type BMBoardForm, yet why is it {lastformShown.GetType().FullName}, I wonder...", typeof(BMBoardForm).FullName, lastformShown.GetType().FullName);
				AssertEquals(expectedDisplayMode, lastformShown.DisplayMode);
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void AssertClickingEditButtonDisplaysMessageAboutInsufficientBoardViewRights(BMBoard board, GlbStaff staff, bool allowStaffToView, bool allowStaffToEdit, bool allowStaffToEditTeamBoards)
		{
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var form = new VisualBoardForm(GetViewModel(board)))
			{
				Env.Security.BMBoardView.IsAllowed = allowStaffToView;
				Env.Security.BMBoardEdit.IsAllowed = allowStaffToEdit;
				Env.Security.BMBoardEditTeamBoards.IsAllowed = allowStaffToEditTeamBoards;

				form.Show();
				Application.DoEvents();
				ZFormModaliser.ShowDialogsInTest = true;

				form.controlsPanel.Expand();
				form.controlsPanel.ConfigButton.PerformClick();

				var lastformShown = ZFormModaliser.LastFormShownForTest;
				AssertNull(lastformShown);
				AssertEquals(Env.Security.BMBoardView.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Open Slideshow Config

		public void TestShouldOpenSlideshowConfigEditForm_WhenTheUserCanEditSlideshows()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board1 = BMSTestHelper.CreateBoard(system, "Alextraza Board");
			var board2 = BMSTestHelper.CreateBoard(system, "Nozdormu Board");
			var board3 = BMSTestHelper.CreateBoard(system, "Ysera Board");
			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2, board3);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var form = new VisualBoardForm(viewModel))
			{
				Env.Security.BMSlideShowsView.IsAllowed = true;
				Env.Security.BMSlideShowsEdit.IsAllowed = true;

				form.Show();
				Application.DoEvents();
				ZFormModaliser.ShowDialogsInTest = true;

				form.controlsPanel.Expand();
				form.controlsPanel.ConfigButton.PerformClick();

				var lastformShown = ZFormModaliser.LastFormShownForTest as ZForm;
				AssertNotNull(lastformShown);
				AssertEquals(ODisplayMode.Browse, lastformShown.DisplayMode);
				AssertEquals($"Should be of type BMBoardSlideshowForm, yet why is it {lastformShown.GetType().FullName}, I wonder...", typeof(BMBoardSlideshowForm).FullName, lastformShown.GetType().FullName);
			}
		}

		public void TestShouldOpenSlideshowConfigViewForm_WhenTheUserCanEditSlideshows()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board1 = BMSTestHelper.CreateBoard(system, "Alextraza Board");
			var board2 = BMSTestHelper.CreateBoard(system, "Nozdormu Board");
			var board3 = BMSTestHelper.CreateBoard(system, "Ysera Board");
			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2, board3);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var form = new VisualBoardForm(viewModel))
			{
				Env.Security.BMSlideShowsView.IsAllowed = true;
				Env.Security.BMSlideShowsEdit.IsAllowed = false;

				form.Show();
				Application.DoEvents();
				ZFormModaliser.ShowDialogsInTest = true;

				form.controlsPanel.Expand();
				form.controlsPanel.ConfigButton.PerformClick();

				var lastformShown = ZFormModaliser.LastFormShownForTest as ZForm;
				AssertNotNull(lastformShown);
				AssertEquals(ODisplayMode.ReadOnly, lastformShown.DisplayMode);
				AssertEquals($"Should be of type BMBoardSlideshowForm, yet why is it {lastformShown.GetType().FullName}, I wonder...", typeof(BMBoardSlideshowForm).FullName, lastformShown.GetType().FullName);
			}
		}

		public void TestShouldInformAboutInsufficientSlideshowViewRights_WhenTheUserCanNeitherEditNorViewSlideshows()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board1 = BMSTestHelper.CreateBoard(system, "Ragnaros Board");
			var board2 = BMSTestHelper.CreateBoard(system, "Al'Akir Board");
			var board3 = BMSTestHelper.CreateBoard(system, "Neptulon Board");
			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2, board3);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var form = new VisualBoardForm(viewModel))
			{
				Env.Security.BMSlideShowsView.IsAllowed = false;
				Env.Security.BMSlideShowsEdit.IsAllowed = false;

				form.Show();
				Application.DoEvents();
				ZFormModaliser.ShowDialogsInTest = true;

				form.controlsPanel.Expand();
				form.controlsPanel.ConfigButton.PerformClick();

				var boardForm = ZFormModaliser.LastFormShownForTest;

				AssertNull(boardForm);
				AssertEquals(Env.Security.BMSlideShowsView.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		public void TestBoardRefreshInterval_ShouldDefaultFromRegistry()
		{
			BMSRegistry.Instance.DefaultBoardRefreshIntervalMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 11);

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var board = config.BufferBoard;

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(board)))
			{
				form.Show();

				AssertEquals("Refresh interval should be 11 minutes", 660, form.SlideShowViewModel.RefreshSeconds);
			}
		}

		#endregion

		#region Board Picker

		public void TestBoardPicker_WhenBoardSelected_ShouldOpenNewBoardForm()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board1 = BMSTestHelper.CreateBoard(system, "Orkish: Lok'tar Ogar");
			var board2 = BMSTestHelper.CreateBoard(system, "Thalassian: An'u belore delen'na.");

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(board1)))
			{
				form.Show();
				Application.DoEvents();

				ZFormModaliser.ShowDialogsInTest = true;

				form.controlsPanel.Expand();
				var button = form.controlsPanel.BoardPickerButton;
				button.PerformClick();
				var contextMenu = button.ContextMenuStrip;
				AssertNotNull(contextMenu);

				var menuItem = button.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Boards not associated with a Release Group");
				menuItem.ShowDropDown();
				AssertEquals(2, menuItem.DropDownItems.Count);

				var boardMenuItem = menuItem.DropDownItems[0];
				AssertEquals(board1.MB_Name, boardMenuItem.Text);
				boardMenuItem = menuItem.DropDownItems[1];
				AssertEquals(board2.MB_Name, boardMenuItem.Text);

				boardMenuItem.PerformClick();
				Application.DoEvents();

				var lastOpenForm = BMSFormTestHelper.GetOpenForms<VisualBoardForm>().SingleOrDefault(x => x.BoardViewModel.BoardPK == board2.PK);
				AssertNotNull($"Should have opened the Thalassian board, yet why has it not been found, I wonder...", lastOpenForm);
				lastOpenForm.Dispose();
			}
		}

		public void TestBoardPicker_WhenFindBoardSelected_ShouldOpenBoardPickerForm()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system, "For the Horde");

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				ZFormModaliser.ShowDialogsInTest = true;

				form.controlsPanel.Expand();
				var button = form.controlsPanel.BoardPickerButton;
				button.PerformClick();
				var contextMenu = button.ContextMenuStrip;
				AssertNotNull(contextMenu);
				var menuItems = contextMenu.Items;

				AssertEquals(VisualBoardMenuItemProvider.SearchItemText, menuItems[0].Text);
				Assert(menuItems[1] is ToolStripSeparator);
				AssertEquals("Boards not associated with a Release Group", menuItems[2].Text);

				menuItems[0].PerformClick();
				Application.DoEvents();

				var boardForm = ZFormModaliser.LastFormShownDialogForTest as VisualBoardPickerForm;
				var isBoardFormNull = boardForm == null;
				boardForm.Dispose();
				AssertEquals("Should be of type VisualBoardPickerForm, yet why is the last form not of that type, I wonder...", false, isBoardFormNull);
				AssertEquals("Search Board", boardForm.Text);
			}
		}

		#endregion

		#region Open in Browser

		public void TestPAVEOnTheWeb_ShouldReturnErrorWhenRegistryIsEmpty()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(board)))
			{
				form.Show();

				ZFormModaliser.ShowDialogsInTest = true;

				form.controlsPanel.Expand();
				form.controlsPanel.OpenInBrowserButton.PerformClick();

				var assertMessage = "Should display error when glow url was not configured in registry";
				AssertEquals(assertMessage, "This Visual Board cannot be opened in a browser as GLOW has not been configured for this client.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPAVEOnTheWeb()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var board = BMSTestHelper.CreateBoard(system);
			board.MB_Name = "The Board";
			board.MB_Description = "My Board";

			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(board)))
			{
				form.Show();

				ZFormModaliser.ShowDialogsInTest = true;

				form.controlsPanel.Expand();
				form.controlsPanel.OpenInBrowserButton.PerformClick();
				AssertEquals("Should not show error if registry item exists", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			AssertBoardOpenedOnTheWeb(board, glowPortalsUri: "address");

			var link = ZArchitecture.Favorites.RecentItemManager.Instance.GetRecentItems(ControllerIDs.VisualBoard.Name).First();
			AssertEquals("The Board - My Board (Web)", link.STL_ItemDescription);
		}

		public static void AssertBoardOpenedOnTheWeb(BMBoard board, string glowPortalsUri)
		{
			var launchedUrl = WebUrlLauncher.LastUrlLaunched;
			var uri = new Uri(launchedUrl, UriKind.Absolute);
			var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
			var accessToken = queryKeyValuePairs["sso_otp"];
			var entityPK = queryKeyValuePairs["entityPK"];

			AssertEquals("https", uri.Scheme);
			AssertEquals(glowPortalsUri, uri.Host);
			AssertEquals("/Goto/Board", uri.AbsolutePath);
			AssertEquals(board.PK.ToString(), entityPK);

			var consumed = ObjectFactory.Get<ITokenizedAccessControl>().TryConsume(accessToken, AccessTokenTypes.LocalIdentity, out var tokenInfo);
			Assert(nameof(consumed), consumed);
			AssertEquals(Env.CurrentUserPK, tokenInfo.ParentId);
			AssertEquals(GlbStaffSchema.Constants.Prefix, tokenInfo.ParentTableCode);

			AssertNotNullOrEmpty(tokenInfo.Scope);
			var scopeObject = JObject.Parse(tokenInfo.Scope);

			// DO NOT MODIFY WITHOUT ALSO EDITING THE CORRESPONDING CODE IN GLOW
			var branchPK = new Guid((string)scopeObject["branch"]);
			AssertEquals(Env.CurrentBranchPK, branchPK);

			// DO NOT MODIFY WITHOUT ALSO EDITING THE CORRESPONDING CODE IN GLOW
			var departmentPK = new Guid((string)scopeObject["department"]);
			AssertEquals(Env.CurrentDepartmentPK, departmentPK);
		}

		#endregion

		#region Remove Cards

		public void TestRemoveDeletedTasksFromTheBoard_EvenIfDeletedInOtherFactory()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Dingus", bucket, releaseGroupPK: group.PK);
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			var viewModel = GetViewModel(section.Board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.RefreshNow_ForTest(forceReload: false);
				Application.DoEvents();

				form.FindAll<TaskCardControl>().SingleOrDefault().ShowDetailedCard();
				Application.DoEvents();

				AssertNotNull(form.FindAll<TaskCardDetailControl>().SingleOrDefault());

				task.Delete();
				task.RefreshBinding();
				Factory.Save();

				Application.DoEvents();

				AssertNull(form.FindAll<TaskCardDetailControl>().SingleOrDefault());
			}
		}

		public void TestClosedTaskCardsShouldDisappearFromBoard_EvenWithFilters()
		{
			var factory1 = new BusinessObjectFactory { NameForDebugging = "Factory1", RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory { NameForDebugging = "Factory2", RefreshEnabled = false };
			var group = BMSTestHelper.CreateGroup(factory1, "AAA");
			var system = BMSTestHelper.CreateSystem(factory1, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var resource = factory1.NewWithValidTestData<GlbStaff>();
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory1);

			BMSTestHelper.CreateProcessHeaderAndTask(jobHeader, bucket, 1, resource.GS_Code, releaseGroupPK: group.PK);

			factory1.Save();

			var viewModel = GetViewModel(section.Board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.RefreshNow_ForTest(forceReload: false);
				Application.DoEvents();

				var cardControl = form.Find(control => control is TaskCardControl).Cast<TaskCardControl>().SingleOrDefault();

				AssertNotNull(cardControl);

				var loadedTask = factory2.Load<ProcessTask>(cardControl.CardContent.GetTask(Factory).PK);
				loadedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				factory2.Save();

				var filter = new CurrentTaskFilter();

				form.BoardViewModel.FilterManager.ToggleFilter(filter);
				form.RefreshNow_ForTest(forceReload: false);
				Application.DoEvents();

				cardControl = form.Find(control => control is TaskCardControl).Cast<TaskCardControl>().SingleOrDefault();

				AssertNull(cardControl);
			}
		}

		public void TestClosedTaskCardsShouldDisappearFromBoard_EvenFromCellTaskControls()
		{
			var factory1 = new BusinessObjectFactory { NameForDebugging = "Factory1", RefreshEnabled = false };
			var group = BMSTestHelper.CreateGroup(factory1, "AAA");
			var system = BMSTestHelper.CreateSystem(factory1, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var resource = factory1.NewWithValidTestData<GlbStaff>();
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory1);

			BMSTestHelper.CreateProcessHeaderAndTask(jobHeader, bucket, 1, resource.GS_Code, releaseGroupPK: group.PK);

			factory1.Save();

			var viewModel = GetViewModel(section.Board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.RefreshNow_ForTest(forceReload: false);
				Application.DoEvents();

				var button = (ZButton)form.Find(control => control.Name == "TotalTasksButton").First();
				button.PerformClick();

				Application.DoEvents();
				var cellTasks = form.FindAll<CellTasksControl>().First();

				var cardControl = cellTasks.FindAll<TaskCardControl>().First();

				var task = cardControl.CardContent.GetTask(Factory);
				cardControl.ShowDetailedCard();

				var detailedCardControl = form.FindAll<TaskCardDetailControl>().First();
				detailedCardControl.ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				detailedCardControl.Save_ForTest();
				detailedCardControl.Close();

				Application.DoEvents();

				AssertEquals(false, cellTasks.FindAll<TaskCardControl>().Any());
			}
		}

		public void TestOpenedTaskCardsPanel_IfVisibilityFilterIsAppliedShouldFilterCorrectly()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var job = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(job, "One job", releaseGroupPK: group.PK);

			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, description: "current task");
			var task2 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, description: "not a current task");

			AssertEquals(true, task1.IsStartable());
			AssertEquals(false, task2.IsStartable());

			Factory.Save();

			var viewModel = GetViewModel(section.Board);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var button = (ZButton)form.Find(control => control.Name == "TotalTasksButton").First();
				button.PerformClick();
				Application.DoEvents();
				var cellTasksControl = form.CurrentlyShownCellTasksControl != null ? form.CurrentlyShownCellTasksControl.Target as CellTasksControl : null;
				AssertNotNull(cellTasksControl);
				AssertEquals(2, cellTasksControl.FindAll<TaskCardControl>().Count());

				var componentControl = form.FindAll<BMComponentControl>().Single();
				var currentTaskMenuItem = componentControl.ContextMenuStrip.Items[0];
				AssertEquals("Show Startable Items", currentTaskMenuItem.Text);

				currentTaskMenuItem.PerformClick();
				Application.DoEvents();

				AssertEquals("Non-current task should be removed from CellTasksControl", 1, cellTasksControl.FindAll<TaskCardControl>().Count());
				currentTaskMenuItem.PerformClick();
				Application.DoEvents();
				AssertEquals("Non-current task should be shown again", 2, cellTasksControl.FindAll<TaskCardControl>().Count());
			}
		}

		public void TestDeletedTasksShouldRemoveTaskCardOnClick()
		{
			var factory1 = new BusinessObjectFactory { NameForDebugging = "Factory1", RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory { NameForDebugging = "Factory2", RefreshEnabled = false };

			var system = BMSTestHelper.CreateSystem(factory1, "ORG");
			var group = BMSTestHelper.CreateGroup(factory1, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			var resource = factory1.NewWithValidTestData<GlbStaff>();
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory1);

			BMSTestHelper.CreateProcessHeaderAndTask(jobHeader, bucket, 1, resource.GS_Code, releaseGroupPK: group.PK);

			factory1.Save();

			var viewModel = GetViewModel(section.Board);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				var taskPanel = form.FindAll<TaskPanel>().SingleOrDefault();
				var task = jobHeader.Tasks.First();
				var cardControl = form.FindAll<TaskCardControl>().SingleOrDefault();

				AssertNotNull(cardControl);

				var loadedTask = factory2.Load<ProcessTask>(task.PK);
				loadedTask.Delete();

				factory2.Save();

				cardControl.OnTaskCardControlClicked();

				cardControl = form.FindAll<TaskCardControl>().SingleOrDefault();
				AssertNull(cardControl);
			}
		}

		#endregion

		#region Overlay

		public void TestAddOverlayControl_ShouldPositionVisiblyOnScreen()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.FillWithValidTestData();
			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(board)))
			using (var panel1 = new ZPanel())
			using (var panel2 = new ZPanel())
			using (var containerPanel = new ZPanel { Dock = DockStyle.Fill })
			{
				panel1.Size = ControlDpiScalingHelper.NewScaledSize(50, 50);
				panel2.Size = ControlDpiScalingHelper.NewScaledSize(50, 50);

				form.Controls.Add(containerPanel);

				form.Show();
				form.WindowState = FormWindowState.Normal;
				form.Size = ControlDpiScalingHelper.NewScaledSize(1024, 768);

				form.AddOverlayControl(panel1, containerPanel.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(974, 678)), Point.Empty, 0);
				AssertPointsCloseTogether("Should position control as proposed - there is room", ControlDpiScalingHelper.NewScaledPoint(974, 678), panel1.Location, 3);

				form.AddOverlayControl(panel2, containerPanel.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(974, 728)), ControlDpiScalingHelper.NewScaledPoint(10, 10), 10);
				AssertPointsCloseTogether("Should move control to the left and nudge up to the bottom of the form", ControlDpiScalingHelper.NewScaledPoint(924, 678), panel2.Location, 5);
			}
		}

		public void TestAddOverlayControl_ShouldNotAddControlOutsideFormBounds()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.FillWithValidTestData();
			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(board)))
			using (var panel = new ZPanel())
			{
				panel.Size = ControlDpiScalingHelper.NewScaledSize(50, 50);

				form.Show();
				form.WindowState = FormWindowState.Normal;
				form.Size = ControlDpiScalingHelper.NewScaledSize(1024, 768);
				form.Location = new Point(0, 0);

				// Attempt to place overlay control off the right edge, but since it can't fit there, require it to jump back the full width of the form to fall off the left edge.
				form.AddOverlayControl(panel, ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(form.Width), 0), Point.Empty, xJumpBackPixels: form.Width);
				AssertEquals("Overlay control should be placed within the bounds of the form", 0, panel.Left);
			}
		}

		void AssertPointsCloseTogether(string message, Point expectedPoint, Point actualPoint, int tolerance)
		{
			var messageBase = "Difference between expected point ({0}) and actual ({1}) was outside tolerance of {2} pixels.\r\n{3}";

			var xDiff = Math.Abs(expectedPoint.X - actualPoint.X);
			Assert(string.Format(messageBase, expectedPoint, actualPoint, tolerance, message), xDiff <= tolerance);
			var yDiff = Math.Abs(expectedPoint.Y - actualPoint.Y);
			Assert(string.Format(messageBase, expectedPoint, actualPoint, tolerance, message), yDiff <= tolerance);
		}

		[ExpectNoExceptions]
		public void TestAddDisposedOverlayControl()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			using (var control = new Control())
			{
				form.Show();
				control.Dispose();
				form.AddOverlayControl(control, new Point(), new Point(), 2);
			}
		}

		#endregion

		#region Layout

		public void TestSetupTable()
		{
			var system = Factory.New<BMSystem>();
			system.FS_Name = "WTGDEV";
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket");
			system.Components.Add(bucket1);
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");
			system.Components.Add(bucket2);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			system.Components.Add(buffer);

			var board = system.Boards.AddNew();
			board.MB_Name = "International Logistics";
			board.MB_Description = "Nope.";

			var bucket1Section = CreateSection(board, bucket1, 0, 0, 2, 1, 0, 10, LastCellList.Codes.Right);
			var bucket2Section = CreateSection(board, bucket2, 0, 1, 1, 1, 10, 90, LastCellList.Codes.Top);
			var bufferSection = CreateSection(board, buffer, 1, 1, 1, 1, 90, 90);
			bufferSection.SectionConfiguration.CellsPerSubsection = 4;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			BMSTestHelper.CreateReleaseGroup(system, group);

			bucket1Section.SectionConfiguration.ReleaseGroupPK = group.PK;
			bucket2Section.SectionConfiguration.ReleaseGroupPK = group.PK;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				AssertStartsWith("Form title should be board name (not description)", "International Logistics", form.Text);
				var boardSectionControls = form.VisualBoardTableLayoutPanel.Controls.OfType<ZUserControl>().ToArray();
				AssertEquals(3, boardSectionControls.Length);

				AssertSectionNameIsInTableCell(form.VisualBoardTableLayoutPanel, bucket1.FC_Name, 0, 0, 2, 1, 100, 10);
				AssertSectionNameIsInTableCell(form.VisualBoardTableLayoutPanel, bucket2.FC_Name, 0, 1, 1, 1, 10, 90);
				AssertSectionNameIsInTableCell(form.VisualBoardTableLayoutPanel, buffer.FC_Name, 1, 1, 1, 1, 90, 90);
			}
		}

		public void TestSetupTable_SensibleWidthDefaults()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket1 = BMSTestHelper.CreateBucket(system, "Brian");
			var board = BMSTestHelper.CreateBoard(system);

			var section1 = CreateSection(board, bucket1, 0, 0, 1, 1);
			var section2 = CreateSection(board, bucket1, 1, 0, 1, 1);
			var section3 = CreateSection(board, bucket1, 0, 1, 1, 1);
			var section4 = CreateSection(board, bucket1, 1, 1, 1, 1);

			var viewModel = GetViewModel(board);

			Factory.Save();

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				AssertStartsWith("Form title should be board name (not description)", "An Board", form.Text);
				var boardSectionControls = form.VisualBoardTableLayoutPanel.Controls.OfType<ZUserControl>().ToArray();
				AssertEquals(4, boardSectionControls.Length);

				var componentControls = form.FindAll<BMComponentControl>().ToArray();

				AssertEquals(4, componentControls.Length);

				AssertCloseEnoughForJazz(componentControls[0].Width, componentControls[1].Width);
				AssertCloseEnoughForJazz(componentControls[1].Width, componentControls[2].Width);
				AssertCloseEnoughForJazz(componentControls[2].Width, componentControls[3].Width);

				AssertCloseEnoughForJazz(componentControls[0].Height, componentControls[1].Height);
				AssertCloseEnoughForJazz(componentControls[1].Height, componentControls[2].Height);
				AssertCloseEnoughForJazz(componentControls[2].Height, componentControls[3].Height);

				AssertCloseEnoughForJazz(form.Width / 2, componentControls[0].Width, 25);
			}
		}

		void AssertCloseEnoughForJazz(int val1, int val2, int fudge = 5)
		{
			AssertCloseEnough(val1, val2, fudge);
		}

		void AssertSectionNameIsInTableCell(TableLayoutPanel table, string sectionName, int expectedCol, int expectedRow, int expectedColSpan, int expectedRowSpan, float expectedWidthPercentage, float expectedHeightPercentage)
		{
			var sectionControl = table.Find(c => c.Text.StartsWith(sectionName)).First().Parent.Parent.Parent;
			AssertNotNull("There should be a section with name " + sectionName, sectionControl);

			var position = table.GetCellPosition(sectionControl);
			AssertEquals(expectedCol, position.Column);
			AssertEquals(expectedRow, position.Row);

			var expectedWith = (int)(table.Width * expectedWidthPercentage / 100);
			var actualWidth = table.GetColumnWidths().Skip(expectedCol).Take(expectedColSpan).Sum();
			Assert(Math.Abs(expectedWith - actualWidth) < 5);

			var expectedHeight = (int)(table.Height * expectedHeightPercentage / 100);
			var actualHeight = table.GetRowHeights().Skip(expectedRow).Take(expectedRowSpan).Sum();
			Assert(Math.Abs(expectedHeight - actualHeight) < 5);
		}

		BMBoardSection CreateSection(BMBoard board, BMComponent component, int col, int row, int colSpan, int rowSpan, int? colWidthPercent = null, int? rowHeightPercent = null, string lastCell = LastCellList.Codes.Top)
		{
			var section = board.Sections.AddNew();
			section.MS_FC_Component = component.PK;
			section.Column = col;
			section.Row = row;
			section.ColSpan = colSpan;
			section.RowSpan = rowSpan;
			section.ColWidthPercent = colWidthPercent ?? section.ColWidthPercent;
			section.RowHeightPercent = rowHeightPercent ?? section.RowHeightPercent;
			section.SectionConfiguration.LastCell = lastCell;

			return section;
		}

		public void TestMainContentTable_ShouldSitAboveStatusBar()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);

			Factory.Save();

			var viewModel = GetViewModel(section.Board);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				var contentTable = form.Controls.OfType<TableLayoutPanel>().Single();
				AssertEquals("content table should be placed at the top of the status bar (so there is no overlap)", form.MainStatusBar.Top, contentTable.Height);
			}
		}

		public void TestBufferLayout_WhenSectionConfigDoesNotIncludeChannelBy()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);

			section.SectionConfiguration.OverrideChannels = true;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);

			Factory.Save();

			AssertEquals("", section.SectionConfiguration.ChannelBy);

			TestConnection.ExecuteNonQuery(string.Format(@"
				UPDATE dbo.BMBoardSection
				SET MS_LayoutData.modify('
					delete //ChannelBy[1]
				'),
				MS_SystemLastEditTimeUTC = getutcdate(),
				MS_SystemLastEditUser = '~BP'
				WHERE MS_PK = '{0}'
				", section.PK));

			var loadedSection = Factory.CreateNewFactory().Load<BMBoardSection>(section.PK);
			AssertEquals("", loadedSection.SectionConfiguration.ChannelBy);

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();

				AssertNotNull("Form should render control correctly", form.FindAll<BMComponentControl>().SingleOrDefault());
			}
		}

		public void TestRefreshHeaderWithTagAsSecondaryChannel()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);

			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.OverrideSecondaryChannels = true;

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "PRI");
			var green = BMSTestHelper.CreateTagMagnitude(tagGroup, "GRN", nudge: 200);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);
			BMSTestHelper.CreateSecondaryChannelForSection(section, ChannelTypeList.Codes.Tag, green.PK);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				var boardSection = section.Board.Sections.Single();
				Assert("GIVEN section with bucket component", boardSection.Component.IsBucket);
				AssertEquals("GIVEN section with tag-rule as secondary channel", ChannelTypeList.Codes.Tag, boardSection.SectionConfiguration.SecondaryAxisChannels.First().MSC_ChannelType);

				ErrorReporter.Clear();
				AssertNullOrEmpty("GIVEN no error", ErrorReporter.LastMessageReported);

				form.Show();

				AssertNullOrEmpty("WHEN showing visual-board THEN expect no error", ErrorReporter.LastMessageReported);
			}
		}

		public void TestBucketLayout_WhenSectionConfigDoesNotIncludeChannelBy()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);

			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.OverrideSecondaryChannels = true;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);
			BMSTestHelper.CreateSecondaryChannelForSection(section, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK);

			Factory.Save();

			AssertEquals("", section.SectionConfiguration.ChannelBy);
			AssertEquals("", section.SectionConfiguration.ChannelSecondaryBy);

			TestConnection.ExecuteNonQuery(string.Format(@"
				UPDATE dbo.BMBoardSection
				SET MS_LayoutData.modify('
					delete //ChannelBy[1]
				'),
				MS_SystemLastEditTimeUTC = getutcdate(),
				MS_SystemLastEditUser = '~BP'
				WHERE MS_PK = '{0}'

				UPDATE dbo.BMBoardSection
				SET MS_LayoutData.modify('
					delete //ChannelSecondaryBy[1]
				'),
				MS_SystemLastEditTimeUTC = getutcdate(),
				MS_SystemLastEditUser = '~BP'
				WHERE MS_PK = '{0}'
				", section.PK));

			var loadedSection = Factory.CreateNewFactory().Load<BMBoardSection>(section.PK);
			AssertEquals("", loadedSection.SectionConfiguration.ChannelBy);
			AssertEquals("", loadedSection.SectionConfiguration.ChannelSecondaryBy);

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();

				AssertNotNull("Form should render control correctly", form.FindAll<BMComponentControl>().SingleOrDefault());
			}
		}

		public void TestInvalidProperty_InCustomLayout_WhenPropertyNameContainsPlus_ShouldNotThrowException()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG", "INQ");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var task1 = BMSTestHelper.CreateTask(workflow1);

			var layout = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
			layout.Width = 300;
			layout.Height = 200;
			layout.BackgroundColor = "Hot Pink";

			BMSTestHelper.CreateControlCustomisationLink(Factory, section, layout);

			BMSTestHelper.CreateLine(layout, PropertySourceList.Codes.Job, OrgHeaderSchema.Constants.OH_Code, PropertyTypeList.Codes.Text, "Code", 100, 0, 100, 20, Color.Transparent.Name, Color.Black.Name, 8, false, true, true);
			BMSTestHelper.CreateLine(layout, PropertySourceList.Codes.Job, "<Something.Something.Something.Dark Side>", PropertyTypeList.Codes.Text, "Label 1", 100, 0, 100, 20, Color.Red.Name, Color.Black.Name, 8, false, true, false);
			BMSTestHelper.CreateLine(layout, PropertySourceList.Codes.Job, "Something, Something, Something, Dark Side", PropertyTypeList.Codes.Text, "Label 2", 100, 0, 100, 20, Color.Red.Name, Color.Black.Name, 8, false, true, false);
			BMSTestHelper.CreateLine(layout, PropertySourceList.Codes.Job, "Something+Something+Something+DarkSide", PropertyTypeList.Codes.Text, "Label 3", 100, 0, 100, 20, Color.Red.Name, Color.Black.Name, 8, false, true, false);

			Factory.Save();

			var viewModel = GetViewModel(section.Board);

			AssertNoExceptionThrown("The visual board should not blow up.", () =>
			{
				using (var form = new VisualBoardForm(viewModel))
				{
					form.Show();
					Application.DoEvents();
				}
			});
		}

		#endregion

		#region Channels

		public void TestBoardWithNonOverriddenChannels_SortedAlphabetically_WhenNewStaffAddedToGroup_ShouldPutNewChannelInAlphabeticalOrder()
		{
			var group = BMSTestHelper.CreateGroup(Factory, "GRP", "The BEST group");
			var staff1 = BMSTestHelper.CreateStaff(Factory, "AD", "Adam");
			var staff2 = BMSTestHelper.CreateStaff(Factory, "BAD", "Badam");
			var staff3 = BMSTestHelper.CreateStaff(Factory, "CAD", "Cadam");
			group.Staff.Add(staff1);
			group.Staff.Add(staff3);

			Factory.Save();

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var sectionConfig = config.BufferSection.SectionConfiguration;
			sectionConfig.ReleaseGroupPK = group.PK;
			sectionConfig.ChannelBy = ChannelTypeList.Codes.Resource;
			sectionConfig.SortPrimaryChannels = true;
			sectionConfig.OverrideChannels = false;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var channels = form.FindAll<ChannelHeaderControl>();
				AssertSequencesEqual(new[] { "Adam", "Cadam" }, channels.Select(x => x.ChannelNameLabel.Text));
			}

			group.Staff.Add(staff2);
			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var channels = form.FindAll<ChannelHeaderControl>();
				AssertSequencesEqual("The channels should be shown in alphabetical order, even though we never opened the board config form so that the sequence numbers could be updated. SAD!", new[] { "Adam", "Badam", "Cadam" }, channels.Select(x => x.ChannelNameLabel.Text));
			}
		}

		#endregion

		#region Exception Handling

		public void TestOpenBoard_WhenPassedQualityIterationsExist_ShouldNotThrowException()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CBF", "ORG");

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var board = config.BufferBoard;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Drumph", config.Buffer);
			var task1 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "CBF");
			var task2 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code);

			CreateIterationLink(task1, null, outcome: IterationLinkOutcomeList.Codes.Passed);
			CreateIterationLink(task1, null, outcome: IterationLinkOutcomeList.Codes.Passed);
			CreateIterationLink(task1, null, outcome: IterationLinkOutcomeList.Codes.Deferred);
			CreateIterationLink(task1, null, outcome: IterationLinkOutcomeList.Codes.Deferred);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				AssertNotNull(form.FindSingle<TaskCardControl>());
			}
		}

		#endregion

		#region Summary Cards

		public void TestTicketLayouts_WhenLayoutContainsJobPropertiesValidForOneJobTypeOnly()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG", "INQ");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2");

			var task1 = BMSTestHelper.CreateTask(workflow1);
			var task2 = BMSTestHelper.CreateTask(workflow2);

			var layout = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, layout);
			layout.Height *= 2;

			BMSTestHelper.CreateLine(layout, PropertySourceList.Codes.Job, OrgHeaderSchema.Constants.OH_Code, PropertyTypeList.Codes.Text, "Code", 100, 0, 100, 20, Color.Transparent.Name, Color.Black.Name, 8, false, true, true);
			BMSTestHelper.CreateLine(layout, PropertySourceList.Codes.Job, OrgHeaderSchema.Constants.OH_IsActive, PropertyTypeList.Codes.Boolean, "Active", 100, 0, 100, 20, Color.White.Name, Color.Black.Name, 8, false, true, true);
			BMSTestHelper.CreateLine(layout, PropertySourceList.Codes.Job, OrgHeaderSchema.Constants.OH_SystemLastEditTimeUtc, PropertyTypeList.Codes.DateTime, "Last Edited", 100, 0, 100, 20, Color.White.Name, Color.Black.Name, 8, false, true, true);
			BMSTestHelper.CreateLine(layout, PropertySourceList.Codes.Job, OrgColdCallRegisterSchema.Constants.O1_LeadCalledDate, PropertyTypeList.Codes.Date, "Lead Called", 100, 0, 100, 20, Color.White.Name, Color.Black.Name, 8, false, true, true);

			Factory.Save();

			var viewModel = GetViewModel(section.Board);

			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				Application.DoEvents();

				var ticket1 = BMSGUITestCase.FindTaskCardControl(form, task1);
				var ticket2 = BMSGUITestCase.FindTaskCardControl(form, task2);

				AssertNotNull(ticket1);
				AssertNotNull(ticket2);
			}
		}

		#endregion

		#region Detailed Cards

		[TestDate(2015, 7, 14)]
		public void TestChangeTaskStatusViaDetailedCard_ShouldNotAddTasksThatShouldNotBeShown()
		{
			BMSRegistry.Instance.UpdateTicketsWithDataRefresh.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var otherBucket = BMSTestHelper.CreateBucket(config.System, "Other Bucket");
			BMSTestHelper.LinkComponents(otherBucket, config.Bucket);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowInBuffer = BMSTestHelper.CreateWorkflow(jobHeader, "When you said the words", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflowInNonFeedingBucket = BMSTestHelper.CreateWorkflow(jobHeader, "And he's using them back", otherBucket, releaseGroupPK: config.ReleaseGroup.PK);

			workflowInBuffer.GetOrCreateDependencyLink(workflowInNonFeedingBucket);

			var task1 = BMSTestHelper.CreateTask(workflowInBuffer, config.CCR.GS_Code);
			var task2 = BMSTestHelper.CreateTask(workflowInNonFeedingBucket, config.CCR.GS_Code);

			var board = BMSTestHelper.CreateBoard(config.System);
			var releaseSchedulerSection = BMSTestHelper.CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup, board);
			var bufferSection = BMSTestHelper.CreateBoardSection(config.Buffer, board, row: 1);

			var job = (OrgHeader)jobHeader.Parent;

			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job);
			job.RunPreSaveValidation();

			AssertNoErrors(job);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var releaseSchedulerControl = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == releaseSchedulerSection.PK);
				var bufferControl = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == bufferSection.PK);

				AssertEquals(1, FindTaskCardControls(releaseSchedulerControl).Length);

				FindTaskCardControl(bufferControl, task1).ShowDetailedCard();
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

				var detailedCard = form.FindAll<TaskCardDetailControl>().Single();
				detailedCard.ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
				detailedCard.Save_ForTest();

				Application.DoEvents();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, FindTaskCardControls(releaseSchedulerControl).Length);
				AssertEquals(1, FindTaskCardControls(bufferControl).Length);
			}
		}

		public void TestNudgeWorkflowAndJobLevelWorkflow_ReturnsCorrespondingNudgeValues()
		{
			var sectionGroup = Factory.NewWithValidTestData<GlbGroup>();

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			BMSTestHelper.CreateReleaseGroup(config.System, sectionGroup);
			var section = config.BucketSection;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;
			section.SectionConfiguration.ReleaseGroupPK = sectionGroup.PK;

			var staff = BMSTestHelper.CreateStaff(Factory, "AAA");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "Job Header", addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_GG_ReleaseGroup = sectionGroup.PK;
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Work Flow", currentComponent: config.Bucket, releaseGroupPK: sectionGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, staffCode: staff.GS_Code, description: "Process Task");

			jobHeader.FH_VoteUpDownAmount = 42;
			workflow.FH_VoteUpDownAmount = 69;

			Factory.Save();

			void ClickLabel(KLabel label)
			{
				typeof(Control).InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, label, new object[] { new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0) });
			}

			using (var form = new VisualBoardForm(GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCard = form.FindSingle<TaskCardControl>();
				AssertNotNull(taskCard);

				taskCard.ShowDetailedCard();
				var detailedCard = form.FindSingle<TaskCardDetailControl>();
				AssertNotNull(detailedCard);

				var nudgeControl = detailedCard.FindSingle<NudgeControls>();
				AssertNotNull(nudgeControl);

				AssertEquals("The nudge value on the detailed card should match the workflow's nudge value", "69", nudgeControl.NudgeAmount.Text);

				ClickLabel(nudgeControl.LabelVoteDown);
				AssertEquals("The nudge value on the detailed card should decrease by one", "68", nudgeControl.NudgeAmount.Text);

				ClickLabel(nudgeControl.LabelVoteUp);
				AssertEquals("The nudge value on the detailed card should increase by one", "69", nudgeControl.NudgeAmount.Text);
			}

			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCard = form.FindSingle<TaskCardControl>();
				AssertNotNull(taskCard);

				taskCard.ShowDetailedCard();
				var detailedCard = form.FindSingle<TaskCardDetailControl>();
				AssertNotNull(detailedCard);

				var nudgeControl = detailedCard.FindSingle<NudgeControls>();
				AssertNotNull(nudgeControl);

				AssertEquals("The nudge value on the detailed card should match the job level workflow's nudge value", "42", nudgeControl.NudgeAmount.Text);

				ClickLabel(nudgeControl.LabelVoteDown);
				AssertEquals("The nudge value on the detailed card should decrease by one", "41", nudgeControl.NudgeAmount.Text);

				ClickLabel(nudgeControl.LabelVoteUp);
				AssertEquals("The nudge value on the detailed card should increase by one", "42", nudgeControl.NudgeAmount.Text);
			}
		}

		#region Current task transitions

		[TestDate(2014, 9, 29)]
		public void TestShowDetailedCardAndCompleteTaskInsideQualityIteration_FollowingTasksInParentWorkflowShouldBeCurrent()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Buffer);
			workflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 10, description: "task1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 20, description: "task2", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, taskType: "QCB");
			var task3 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 30, description: "task3");

			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "ORG");

			var qualityIterationWorkflow = CreateQualityIteration(task1, task2);
			var qiTask1 = qualityIterationWorkflow.Tasks.Single(t => t.P9_Description == "task1");
			var qiTask2 = qualityIterationWorkflow.Tasks.Single(t => t.P9_Description == "task2");

			qiTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var section = CreateBoardSection(config.Buffer);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var task3Control = FindTaskCardControl(form, task3);
				var qiTask2Control = FindTaskCardControl(form, qiTask2);

				var task3Bitmap = task3Control.BackgroundImage;
				var qiTask2Bitmap = qiTask2Control.BackgroundImage;

				TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
				CloseTask(qiTask2, null, form);

				task3Control = FindTaskCardControl(form, task3);
				qiTask2Control = FindTaskCardControl(form, qiTask2);

				AssertNull("Quality iteration task is closed now", qiTask2Control);
				AssertNotNull("Task3 should still be on the board", task3Control);

				AssertNotEquals(task3Bitmap, task3Control.BackgroundImage);
			}
		}

		[TestDate(2014, 9, 29)]
		public void TestShowDetailedCardAndCompletePrerequisiteTask_FollowingTaskShouldBeCurrent()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Buffer);
			workflow.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;
			var task1 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 10, description: "task1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 20, description: "task2");
			var task3 = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 30, description: "task3");

			var section = CreateBoardSection(config.Buffer);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var task2Control = FindTaskCardControl(form, task2);
				var task3Control = FindTaskCardControl(form, task3);

				var task2Bitmap = task2Control.BackgroundImage;
				var task3Bitmap = task3Control.BackgroundImage;

				TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
				CloseTask(task2, null, form);

				task2Control = FindTaskCardControl(form, task2);
				task3Control = FindTaskCardControl(form, task3);

				AssertNull("task2 is closed now", task2Control);
				AssertNotNull("task3 should still be on the board", task3Control);

				AssertNotEquals(task3Bitmap, task3Control.BackgroundImage);
			}
		}

		[TestDate(2014, 9, 29)]
		public void TestShowDetailedCardAndCompleteTaskInPrerequisiteWorkflow_TaskInFollowingWorkflowShouldBeCurrent()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1", config.Buffer);
			var workflow2 = CreateWorkflow(jobHeader, "workflow2", config.Buffer);
			workflow1.FH_GG_ReleaseGroup = workflow2.FH_GG_ReleaseGroup = config.ReleaseGroup.PK;

			workflow1.GetOrCreateDependencyLink(workflow2);

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, sequence: 10, description: "task1", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60, sequence: 20, description: "task2");
			var task3 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60, sequence: 30, description: "task3");

			var section = CreateBoardSection(config.Buffer);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var task2Control = FindTaskCardControl(form, task2);
				var task3Control = FindTaskCardControl(form, task3);

				var task2Bitmap = task2Control.BackgroundImage;
				var task3Bitmap = task3Control.BackgroundImage;

				TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
				CloseTask(task2, null, form);

				task2Control = FindTaskCardControl(form, task2);
				task3Control = FindTaskCardControl(form, task3);

				AssertNull("task2 is closed now", task2Control);
				AssertNotNull("task3 should still be on the board", task3Control);

				AssertNotEquals(task3Bitmap, task3Control.BackgroundImage);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestShowDetailedCardAndClose_ForCCRChannelWithNoRiskState_ShouldNotThrowException()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			config.ReleaseGroup.Staff.AddRange(resource1, resource2);
			resource1.DesignateAsCCR(config.Buffer);
			ConstrainedModeHelper.SwitchToConstrainedMode(config.ReleaseGroup, config.Buffer.PK);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.CellsPerSubsection = 13;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "INTJ", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "ENTJ", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task1_1 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, 10);
			var task1_2 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, config.Buffer.FC_BufferTimespanInMinutes / 4, estVariationFactor: 1); // Avoids the CCR queue too short risk state.
			var task2 = BMSTestHelper.CreateTask(workflow2, resource2.GS_Code, 60);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var channelHeader = FindChannelHeaderControl(form, resource1);

				AssertEquals("Idle", channelHeader.ChannelStatus);

				PlayTask(task1_1, resource1, form);
				AssertEquals("Working", channelHeader.ChannelStatus);

				CloseTask(task1_1, resource1, form);
				AssertEquals("Idle", channelHeader.ChannelStatus);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestShowDetailedCardAndClose_ForCCRChannelWithNoRiskState_AndPreCCRTasksForADifferentCCR_ShouldNotThrowException()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var resource_ccr1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource_ccr2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource_nonCCR = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			config.ReleaseGroup.Staff.AddRange(resource_ccr1, resource_ccr2, resource_nonCCR);
			resource_ccr1.DesignateAsCCR(config.Buffer);
			resource_ccr2.DesignateAsCCR(config.Buffer);
			ConstrainedModeHelper.SwitchToConstrainedMode(config.ReleaseGroup, config.Buffer.PK);

			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;
			section.SectionConfiguration.ReleaseGroupPK = config.ReleaseGroup.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.CellsPerSubsection = 13;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "INTJ", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "ENTJ", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task1_1 = BMSTestHelper.CreateTask(workflow1, resource_ccr1.GS_Code, 10);
			var task1_2 = BMSTestHelper.CreateTask(workflow1, resource_ccr1.GS_Code, config.Buffer.FC_BufferTimespanInMinutes / 4, estVariationFactor: 1); // Avoids the CCR queue too short risk state.
			var task2_1 = BMSTestHelper.CreateTask(workflow2, resource_nonCCR.GS_Code, 60);
			var task2_2 = BMSTestHelper.CreateTask(workflow2, resource_ccr2.GS_Code, 60);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();
				Application.DoEvents();

				var channelHeader = FindChannelHeaderControl(form, resource_ccr1);

				AssertEquals("Idle", channelHeader.ChannelStatus);

				PlayTask(task1_1, resource_ccr1, form);
				AssertEquals("Working", channelHeader.ChannelStatus);

				CloseTask(task1_1, resource_ccr1, form);
				AssertEquals("Idle", channelHeader.ChannelStatus);
			}
		}

		#endregion

		#endregion

		#region GUI Items In Memory

		public void TestCountControls()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			for (int i = 0; i < 40; i++)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
				var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow" + i, buffer);
				var task = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 60, description: "task" + i);
			}

			Factory.Save();

			var viewModel = GetViewModel(section.Board);
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				form.RefreshNow_ForTest(forceReload: false);
				Application.DoEvents();

				var expectation = new Dictionary<Type, int>
				{
					{ typeof(TaskCardControl), 40 },
					{ typeof(KTableLayoutPanel), 3 },
					{ typeof(ZButton), 7 },
					{ typeof(FilterButton), 1 },
					{ typeof(BoardPickerButton), 1 },
					{ typeof(ZLabel), 3 },
					{ typeof(TaskPanel), 10 },
					{ typeof(HeaderControlProvider.HeaderWrapperControl), 14 },
					{ typeof(DirectionalLabel), 14 },
				};

				AssertControlCounts(form, expectation, 2, 10);
			}
		}

		void AssertControlCounts(Control control, Dictionary<Type, int> countsPerType, int maxNumberBeforeFlag, int fudgeFactor)
		{
			var componentDictionary = control.FindAll<Component>().ToKeyListDictionary(t => t.GetType());

			CombineAssertions(() =>
			{
				foreach (var pair in componentDictionary.OrderBy(i => i.Value.Count))
				{
					if (countsPerType.TryGetValue(pair.Key, out var counts))
					{
						AssertEquals(string.Format("Expected control type {0} to have limited instances.", pair.Key.FullName), counts, pair.Value.Count);
						countsPerType.Remove(pair.Key);
					}
					else if (maxNumberBeforeFlag < pair.Value.Count)
					{
						Fail(string.Format("Expected control type {0} to have fewer instances. Max instances {1}, actual {2}", pair.Key.FullName, maxNumberBeforeFlag, pair.Value.Count));
					}
				}

				foreach (var pair in countsPerType)
				{
					Fail(string.Format("Expected control type {0} to have instances but there were none", pair.Key.FullName, pair.Value));
				}
			});
		}

		#endregion

		#region Capability Tasks

		public void TestVariousChannelTypesLoadCapabilityTasks()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			config.BufferSection.SectionConfiguration.OverrideChannels = true;
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BAM", "BammBamm");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "PEB", "Pebbles");
			var capability1 = BMSTestHelper.CreateCapability(Factory, "STR", "Strong");
			var capability2 = BMSTestHelper.CreateCapability(Factory, "SMT", "Smart");

			resource1.Capabilities.Add(capability1);
			resource2.Capabilities.Add(capability2);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Capability, capability1.PK);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.Staff.AddRange(resource1);
			var releaseGroup1 = CreateReleaseGroup(config.System, group1, config.Buffer);

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.Staff.AddRange(resource2);
			var releaseGroup2 = CreateReleaseGroup(config.System, group2, config.Buffer);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Group, group1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Group, group2.PK);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Cave Kids", config.Buffer);

			var resourceAndCapabilityTask1 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, capability: capability1, description: "resourceAndCapabilityTask1");
			var resourceAndCapabilityTask2 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, capability: capability2, description: "resourceAndCapabilityTask2");
			var resourceOnlyTask1 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, description: "resourceOnlyTask1");
			var resourceOnlyTask2 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, description: "resourceOnlyTask2");
			var capabilityOnlyTask1 = BMSTestHelper.CreateTask(workflow, capability: capability1, description: "capabilityOnlyTask1");
			var capabilityOnlyTask2 = BMSTestHelper.CreateTask(workflow, capability: capability2, description: "capabilityOnlyTask2");

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(config.BufferSection.PK);

			using (var form = new VisualBoardForm(GetViewModel(loadedSection.Board)))
			{
				form.Show();
				Application.DoEvents();

				AssertTasksInChannel(form, resource1, new[] { resourceAndCapabilityTask1, resourceOnlyTask1, capabilityOnlyTask1 });
				AssertTasksNotInChannel(form, resource1, new[] { resourceAndCapabilityTask2, resourceOnlyTask2, capabilityOnlyTask2 });

				AssertTasksInChannel(form, resource2, new[] { resourceAndCapabilityTask2, resourceOnlyTask2, capabilityOnlyTask2 });
				AssertTasksNotInChannel(form, resource2, new[] { resourceAndCapabilityTask1, resourceOnlyTask1, capabilityOnlyTask1 });

				AssertTasksInChannel(form, capability1, new[] { resourceAndCapabilityTask1, capabilityOnlyTask1 });
				AssertTasksNotInChannel(form, capability1, new[] { resourceAndCapabilityTask2, resourceOnlyTask1, resourceOnlyTask2, capabilityOnlyTask2 });

				AssertTasksInChannel(form, group1, new[] { resourceAndCapabilityTask1, resourceOnlyTask1 });
				AssertTasksNotInChannel(form, group1, new[] { resourceAndCapabilityTask2, resourceOnlyTask2, capabilityOnlyTask1, capabilityOnlyTask2 });

				AssertTasksInChannel(form, group2, new[] { resourceAndCapabilityTask2, resourceOnlyTask2 });
				AssertTasksNotInChannel(form, group2, new[] { resourceAndCapabilityTask1, resourceOnlyTask1, capabilityOnlyTask1, capabilityOnlyTask2 });
			}
		}

		[TestDate(2015, 8, 3)]
		public void TestReleaseSchedulerWithTooManyItemsOnBoard()
		{
			BMSRegistry.Instance.MaxNumberOfItemsOnBoards.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);

			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var section = CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup);

			var unrelatedComponent = Factory.NewWithValidTestData<BMComponent>();

			Factory.Save();

			BMSTestHelper.CreateWorkflows(config.Buffer, 50, 1, config.ReleaseGroup, config.CCR);
			BMSTestHelper.CreateWorkflows(config.Bucket, 50, 1, config.ReleaseGroup, config.CCR);
			BMSTestHelper.CreateWorkflows(unrelatedComponent, 100, 1, config.ReleaseGroup, config.CCR);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();

				var taskCardsShown = form.FindAll<TaskCardControl>().ToArray();
				Assert("Should load and show up to 100 workflows.", taskCardsShown.Length <= 100);
			}

			BMSTestHelper.CreateWorkflows(config.Buffer, 1, 1, config.ReleaseGroup, config.CCR);
			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();

				var taskCardsShown = form.FindAll<TaskCardControl>().ToArray();
				AssertEquals("Should dispay a label, as there are too may items on the board.", 0, taskCardsShown.Length);

				var labels = form.FindAll<ZLabel>(x => x.Name.StartsWith("LoadFailedLabel", StringComparison.OrdinalIgnoreCase)).OrderBy(l => l.Location.Y).ToArray();
				AssertEquals("This section (Release Gate for buffer) cannot be displayed, as the number of items to be shown exceeds the maximum allowed (100). Please revise filters and configuration of this section.", labels.First().Text);
			}
		}

		public void TestHideCapabilityTasksFromResourceChannel_AndResourceTasksFromCapabilityChannels()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "LIM", "Limestone");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "LAV", "Lava");
			var capability1 = BMSTestHelper.CreateCapability(Factory, "SLT", "Stalactights");
			var capability2 = BMSTestHelper.CreateCapability(Factory, "SLM", "Stalagmights");

			resource1.Capabilities.Add(capability1);
			resource2.Capabilities.Add(capability2);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Fun with caves", config.Buffer);

			var resourceAndCapabilityTask1 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, capability: capability1, description: "resourceAndCapabilityTask1");
			var resourceAndCapabilityTask2 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, capability: capability2, description: "resourceAndCapabilityTask2");
			var resourceOnlyTask1 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, description: "resourceOnlyTask1");
			var resourceOnlyTask2 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, description: "resourceOnlyTask2");
			var capabilityOnlyTask1 = BMSTestHelper.CreateTask(workflow, capability: capability1, description: "capabilityOnlyTask1");
			var capabilityOnlyTask2 = BMSTestHelper.CreateTask(workflow, capability: capability2, description: "capabilityOnlyTask2");

			config.BufferSection.SectionConfiguration.HideCapabilityTasksFromResourceChannels = true;
			config.BufferSection.SectionConfiguration.HideResourceTasksFromCapabilityChannels = true;

			config.BufferSection.SectionConfiguration.OverrideChannels = true;

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource2.PK);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Capability, capability1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Capability, capability2.PK);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedSection = newFactory.Load<BMBoardSection>(config.BufferSection.PK);

			using (var form = new VisualBoardForm(GetViewModel(loadedSection.Board)))
			{
				form.Show();

				BMSGUITestCase.AssertTasksInChannel(form, resource1, new[] { resourceAndCapabilityTask1, resourceOnlyTask1 });
				BMSGUITestCase.AssertTasksNotInChannel(form, resource1, new[] { resourceAndCapabilityTask2, resourceOnlyTask2, capabilityOnlyTask1 });

				BMSGUITestCase.AssertTasksInChannel(form, resource2, new[] { resourceAndCapabilityTask2, resourceOnlyTask2 });
				BMSGUITestCase.AssertTasksNotInChannel(form, resource2, new[] { resourceAndCapabilityTask1, resourceOnlyTask1, capabilityOnlyTask2 });

				BMSGUITestCase.AssertTasksInChannel(form, capability1, new[] { capabilityOnlyTask1 });
				BMSGUITestCase.AssertTasksNotInChannel(form, capability1, new[] { resourceAndCapabilityTask1, resourceAndCapabilityTask2, resourceOnlyTask1, resourceOnlyTask2, capabilityOnlyTask2 });

				BMSGUITestCase.AssertTasksInChannel(form, capability2, new[] { capabilityOnlyTask2 });
				BMSGUITestCase.AssertTasksNotInChannel(form, capability2, new[] { resourceAndCapabilityTask1, resourceAndCapabilityTask2, resourceOnlyTask1, resourceOnlyTask2, capabilityOnlyTask1 });
			}
		}

		public void TestDoNotShowResourceNotInReleaseGroup_WhenResourceRemovedFromReleaseGroup()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var section = BMSTestHelper.CreateBoardSection(buffer);
			var sectionConfiguration = section.SectionConfiguration;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			BMSTestHelper.CreateReleaseGroup(system, group);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron A. Aaronson");
			group.Staff.Add(resource1);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BBB", "Bearon B. Bearonson");
			group.Staff.Add(resource2);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CCC", "Corin C. Corinson");
			group.Staff.Add(resource3);

			sectionConfiguration.ReleaseGroupPK = group.PK;
			sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			sectionConfiguration.OverrideChannels = false;

			AssertEquals(3, sectionConfiguration.PrimaryAxisChannels.Count);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				AssertEquals(3, form.FindAll<ChannelHeaderControl>().Count());
			}

			group.Staff.Remove(resource3);
			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(section))
			{
				var channels = form.FindAll<ChannelHeaderControl>();
				AssertEquals(2, channels.Count());
				AssertCollectionNotContains("Corin C. Corinson", channels.Select(c => c.ChannelNameLabel.Text));
			}
		}

		#endregion

		#region Performance

		public void TestFactoriesUsedDuringBoardLoad_ShouldBeReadonly_UnlessSpecificallyRequiredForSaving()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory);
			var board = config.Section.Board;
			var releaseSchedulerSection = BMSTestHelper.CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup, board);

			releaseSchedulerSection.Column = 1;

			Factory.Save();

			using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
			{
				using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board)))
				{
					form.Show();
				}

				var newFactories = PersistentFactoryCacheManager.Instance.TrackedFactories_ForTest.ToArray();
				var factoryNames = new Dictionary<string, int>();

				AssertNotEquals("Surely some factories have been created whilst loading the board...", 0, newFactories.Length);

				foreach (var factory in newFactories)
				{
					if (!(factory is ReadOnlyBusinessObjectFactory) && !IsFactoryEverSaved(factory))
					{
						var name = string.IsNullOrEmpty(factory.NameForDebugging) ? factory.AllocationPath : factory.NameForDebugging;

						if (!factoryNames.ContainsKey(name))
						{
							factoryNames.Add(name, 1);
						}
						else
						{
							factoryNames[name]++;
						}
					}
				}

				CombineAssertions($"Factories created during board load should be of type {nameof(ReadOnlyBusinessObjectFactory)} so that ActiveBusinessObjectCollection indices do not re-calculate when bizos are loaded. This behaviour is only needed when data can change, however the contents of all collections created during board load will be unaffected by loads of subsequent collections. Brett's 'FastMode' functionality stops all this extra processing, but we need to opt-out using {nameof(ReadOnlyBusinessObjectFactory)}.", () =>
				{
					foreach (var kvp in factoryNames.OrderByDescending(kvp => kvp.Value))
					{
						Fail($"Count: {kvp.Value}; Name or allocation path: {kvp.Key}");
					}
				});
			}
		}

		static bool IsFactoryEverSaved(BusinessObjectFactory factory)
		{
			switch (factory.NameForDebugging)
			{
				case "TaskCardControl.MenuItems": // When configuring workflow prereqs
				case "FilterStripBusinessObject (Constructor)": // When a user saves filter layouts on modules
					return true;

				default:
					return false;
			}
		}

		public void TestResizeForm_ShouldNotHitDatabase()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, resource3.PK);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Your words", config.Buffer);
			BMSTestHelper.CreateTask(workflow, resource1.GS_Code);
			BMSTestHelper.CreateTask(workflow, resource2.GS_Code);
			BMSTestHelper.CreateTask(workflow, resource3.GS_Code);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(config.BufferBoard)))
			{
				form.Show();
				form.WindowState = FormWindowState.Maximized;
				Application.DoEvents();

				var control = form.FindSingle<BMComponentControl>();
				var tickets = BMSGUITestCase.FindTaskCardControls(control);

				AssertEquals(3, tickets.Length);

				var transformationsToMake = new[]
				{
					Tuple.Create("minimising the form", new Action(() => form.WindowState = FormWindowState.Minimized)),
					Tuple.Create("maximising the form", new Action(() => form.WindowState = FormWindowState.Maximized)),
					Tuple.Create("clicking the 'restore down' button", new Action(() => form.WindowState = FormWindowState.Normal)),
					Tuple.Create("resizing the form", new Action(() => form.Size = new Size(form.Width - 10, form.Height - 10))),
				};

				CombineAssertions(() =>
				{
					foreach (var transform in transformationsToMake)
					{
						using (Db.Connection.TrackExecutedCommands())
						{
							transform.Item2();
							Application.DoEvents();
							AssertContainsExactElementsInAnyOrder("Should be no database hits after " + transform.Item1, Array.Empty<string>(), Db.Connection.ExecutedCommands);
						}
					}
				});

				AssertEquals(false, tickets[0].IsDisposed);
				AssertEquals(false, tickets[1].IsDisposed);
				AssertEquals(false, tickets[2].IsDisposed);
			}
		}

		public void TestLoadProcessHeaderLinks_ShouldNotUseOrInAnyQueries_TaskCards()
		{
			AssertLoadProcessHeaderLinks_ShouldNotUseOrInAnyQueries(CardTypeList.Codes.Task);
		}

		public void TestLoadProcessHeaderLinks_ShouldNotUseOrInAnyQueries_WorkflowCards()
		{
			AssertLoadProcessHeaderLinks_ShouldNotUseOrInAnyQueries(CardTypeList.Codes.Workflow);
		}

		public void TestLoadProcessHeaderLinks_ShouldNotUseOrInAnyQueries_JobCards()
		{
			AssertLoadProcessHeaderLinks_ShouldNotUseOrInAnyQueries(CardTypeList.Codes.JobLevelWorkflow);
		}

		void AssertLoadProcessHeaderLinks_ShouldNotUseOrInAnyQueries(string ticketType)
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, "INQ");
			config.BufferSection.SectionConfiguration.CardType = ticketType;

			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow1", config.Buffer);
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow2", config.Bucket);
			workflow1.MakePrerequisiteOf(workflow2);

			Factory.Save();

			using (Db.Connection.TrackExecutedCommands(includeStackTrace: true))
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.TVPRule = new TVPRule("0");
				using (GetAndShowVisualBoardForm(config.BufferBoard))
				{
					var relevantQueries = Db.Connection.ExecutedCommands.Where(x => x.Contains(ProcessHeaderLinkSchema.Constants.FP_FH_HeaderFrom + " in") || x.Contains(ProcessHeaderLinkSchema.Constants.FP_FH_HeaderTo + " in"));

					CombineAssertions("The following queries use an OR operator for HeaderFrom and HeaderTo queries. These should be executed separately. They may be caused by fetch hints. The OR is very bad for query performance. SAD! You should fix this.", () =>
					{
						foreach (var query in relevantQueries)
						{
							AssertNotContains("or (FP_FH_HeaderTo in", query, ignoreCase: true);
							AssertNotContains("or (FP_FH_HeaderFrom in", query, ignoreCase: true);
							AssertContains("Table valued parameters should be used. SAD!", "SELECT Value FROM @CWO", query, ignoreCase: true);
						}
					});
				}
			}
		}

		#endregion

		#region Release Scheduler

		public void TestReleaseSchedulerSection_ShouldConsiderFilterRulesForReleaseGateComponentsOnly()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var otherBucket1 = BMSTestHelper.CreateBucket(config.System, "Stucket");
			var otherBucket2 = BMSTestHelper.CreateBucket(config.System, "Scrucket");
			var otherLink1 = BMSTestHelper.LinkComponents(otherBucket1, config.Buffer, isReleaseGate: true);
			var otherLink2 = BMSTestHelper.LinkComponents(otherBucket2, config.Buffer, isReleaseGate: false); // Not marked as 'release gate' so no workflows in this bucket should be included.

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_1", config.Bucket);
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_2", config.Bucket);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow2_1 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2_1", otherBucket1);
			var workflow2_2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2_2", otherBucket1);

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow3_1 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow3_1", otherBucket2);
			var workflow3_2 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow3_2", otherBucket2);

			var task1_1 = BMSTestHelper.CreateTask(workflow1_1, config.CCR.GS_Code);
			var task1_2 = BMSTestHelper.CreateTask(workflow1_2, config.CCR.GS_Code);
			var task2_1 = BMSTestHelper.CreateTask(workflow2_1, config.CCR.GS_Code);
			var task2_2 = BMSTestHelper.CreateTask(workflow2_2, config.CCR.GS_Code);
			var task3_1 = BMSTestHelper.CreateTask(workflow3_1, config.CCR.GS_Code);
			var task3_2 = BMSTestHelper.CreateTask(workflow3_2, config.CCR.GS_Code);

			FilterStripsTestHelper.AddCustomSQLFilterStrip(config.ComponentLink.FilterRule, $"FH_PK = '{workflow1_1.PK}'");
			FilterStripsTestHelper.AddCustomSQLFilterStrip(otherLink1.FilterRule, $"FH_PK = '{workflow2_1.PK}'");
			FilterStripsTestHelper.AddCustomSQLFilterStrip(otherLink2.FilterRule, $"FH_PK = '{workflow3_1.PK}'");

			var releaseScheduler = BMSTestHelper.CreateReleaseSchedulerBoardSection(config.Buffer, config.ReleaseGroup);

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(releaseScheduler.Board)))
			{
				form.Show();

				var matchingTasks = new[] { task1_1, task2_1 };
				AssertTasksInChannel("The only matching workflows should be the ones that are in components which have links into the buffer that are marked as 'release gate' links, and match that link's filter rules.", form, config.CCR, new[] { task1_1, task2_1 });
			}
		}

		#endregion

		#region Job-level Workflow Tickets

		public void TestJobLevelWorkflowTickets_PlusTaskFilters_ShouldFilterCorrectly()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = config.BufferSection;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			FilterStripsTestHelper.AddFilterStrip<ModuleTextFilter>(section.TaskFilter, "Description", f => f.Property = "One");

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_1", config.Buffer);
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_2", config.Buffer);

			var workflow2_1 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2_1", config.Bucket);
			var workflow2_2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2_2", config.Bucket);

			BMSTestHelper.CreateTask(workflow1_1, description: "One");
			BMSTestHelper.CreateTask(workflow1_2, description: "Two");

			BMSTestHelper.CreateTask(workflow2_1, description: "One");
			BMSTestHelper.CreateTask(workflow2_2, description: "Two");

			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();

				var taskTicket = form.FindSingle<TaskCardControl>();

				AssertEquals("Only the workflow in the buffer with a task that matches the task filter should be shown", workflow1_1.PK, taskTicket.CardContent.WorkflowIdentifier);
			}

			section.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			Factory.Save();

			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(section.Board)))
			{
				form.Show();

				var taskTicket = form.FindSingle<TaskCardControl>();

				AssertEquals("Only the job-level workflow which has a workflow in the buffer with a task that matches the task filter should be shown", jobHeader1.PK, taskTicket.CardContent.WorkflowIdentifier);
			}
		}

		#endregion

		#region System

		public void TestComponentsFromDifferentSystems_OneBoardSection()
		{
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Aaron A. Aaronson");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BBB", "Brian B. Bronson");
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CCC", "Colin C. Colinson");

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.AddRange(resource1, resource2, resource3);

			var mainBoardSystem = Factory.NewWithValidTestData<BMSystem>();
			var secondaryBoardSystem1 = Factory.NewWithValidTestData<BMSystem>();
			var secondaryBoardSystem2 = Factory.NewWithValidTestData<BMSystem>();
			BMSTestHelper.CreateReleaseGroup(mainBoardSystem, group);

			var mainBoardBuffer = BMSTestHelper.CreateBuffer(mainBoardSystem, "Board Buffer");
			var secondaryBoardBuffer1 = BMSTestHelper.CreateBuffer(secondaryBoardSystem1, "Secondary Buffer 1");
			var secondaryBoardBuffer2 = BMSTestHelper.CreateBuffer(secondaryBoardSystem2, "Secondary Buffer 2");

			var jobHeaderBoard = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeaderSecondaryBoard1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeaderSecondaryBoard2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflowBoard = BMSTestHelper.CreateWorkflowAndTask(jobHeaderBoard, "B", currentComponent: mainBoardBuffer, staffCode: resource1.GS_Code, description: "B");
			var workflowSecondaryBoard1 = BMSTestHelper.CreateWorkflowAndTask(jobHeaderSecondaryBoard1, "O", currentComponent: secondaryBoardBuffer1, staffCode: resource2.GS_Code, description: "O");
			var workflowSecondaryBoard2 = BMSTestHelper.CreateWorkflowAndTask(jobHeaderSecondaryBoard2, "Y", currentComponent: secondaryBoardBuffer2, staffCode: resource3.GS_Code, description: "Y");

			var board = BMSTestHelper.CreateBoard(mainBoardSystem, "I'm Board");
			var section = BMSTestHelper.CreateBoardSection(mainBoardBuffer, board);
			var additionalSection1 = BMSTestHelper.CreateAdditionalComponent(section, secondaryBoardBuffer1);
			var additionalSection2 = BMSTestHelper.CreateAdditionalComponent(section, secondaryBoardBuffer2);

			var sectionConfiguration = section.SectionConfiguration;
			sectionConfiguration.ReleaseGroupPK = group.PK;
			sectionConfiguration.OverrideChannels = true;

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource3.PK);

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCards = form.FindAll<TaskCardControl>();
				AssertEquals(3, taskCards.Count());
				AssertContainsExactElementsInAnyOrder(new[] { workflowBoard.Tasks.First().PK,
					workflowSecondaryBoard1.Tasks.First().PK, workflowSecondaryBoard2.Tasks.First().PK }, taskCards.Select(c => c.CardContent.TaskIdentifier));
			}
		}

		#endregion

		#region Board Factory Services

		[TestDate(2015, 7, 14)]
		public void TestBoardFactoryServices_ShouldBeResetAccordingToStalenessPolicy()
		{
			var service1_resetsOnBoardRefresh = new DummyBoardFactoryService { StalenessPolicy = BoardServiceStalenessPolicy.StaleBeforeBoardRefresh };
			var service2_resetsOnSavingDetailedTicket = new DummyBoardFactoryService { StalenessPolicy = BoardServiceStalenessPolicy.StaleBeforeSavingTicket };
			var service3_resetsOnBoardRefreshAndSavingDetailedTicket = new DummyBoardFactoryService { StalenessPolicy = BoardServiceStalenessPolicy.StaleBeforeBoardRefresh | BoardServiceStalenessPolicy.StaleBeforeSavingTicket };
			var service4_neverResets = new DummyBoardFactoryService { StalenessPolicy = BoardServiceStalenessPolicy.NeverStale };

			var descriptor = new DummySchematicComponentSectionDescriptor(service1_resetsOnBoardRefresh, service2_resetsOnSavingDetailedTicket, service3_resetsOnBoardRefreshAndSavingDetailedTicket, service4_neverResets);

			using (ObjectFactory.Substitute("VisualBoardSectionDescriptors", new ArrayList { descriptor }))
			{
				var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
				var board = BMSTestHelper.CreateBoard(config.System);
				var section = BMSTestHelper.CreateBoardSection(config.Bucket, board);

				var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
				var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource1.PK);
				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource2.PK);

				var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Look how orange you look", releaseGroupPK: config.ReleaseGroup.PK);
				var task1 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code);
				var task2 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code);

				workflow.RunPreSaveValidation();
				AssertNoErrors(workflow);

				Factory.Save();

				using (var form = GetAndShowVisualBoardForm(board))
				{
					AssertEquals(0, service1_resetsOnBoardRefresh.CacheRefreshedCount);
					AssertEquals(0, service2_resetsOnSavingDetailedTicket.CacheRefreshedCount);
					AssertEquals(0, service3_resetsOnBoardRefreshAndSavingDetailedTicket.CacheRefreshedCount);
					AssertEquals(0, service4_neverResets.CacheRefreshedCount);

					PlayTask(task1, resource1, form);

					AssertEquals(0, service1_resetsOnBoardRefresh.CacheRefreshedCount);
					AssertEquals(1, service2_resetsOnSavingDetailedTicket.CacheRefreshedCount);
					AssertEquals(1, service3_resetsOnBoardRefreshAndSavingDetailedTicket.CacheRefreshedCount);
					AssertEquals(0, service4_neverResets.CacheRefreshedCount);

					PlayTask(task2, resource2, form);

					AssertEquals(0, service1_resetsOnBoardRefresh.CacheRefreshedCount);
					AssertEquals(2, service2_resetsOnSavingDetailedTicket.CacheRefreshedCount);
					AssertEquals(2, service3_resetsOnBoardRefreshAndSavingDetailedTicket.CacheRefreshedCount);
					AssertEquals(0, service4_neverResets.CacheRefreshedCount);

					form.RefreshNow_ForTest();

					AssertEquals(1, service1_resetsOnBoardRefresh.CacheRefreshedCount);
					AssertEquals(2, service2_resetsOnSavingDetailedTicket.CacheRefreshedCount);
					AssertEquals(3, service3_resetsOnBoardRefreshAndSavingDetailedTicket.CacheRefreshedCount);
					AssertEquals(0, service4_neverResets.CacheRefreshedCount);
				}
			}
		}

		class DummySchematicComponentSectionDescriptor : SchematicComponentSectionDescriptor
		{
			public DummySchematicComponentSectionDescriptor(params IBoardFactoryService[] services)
			{
				this.services = services;
			}

			readonly IBoardFactoryService[] services;

			protected override IEnumerable<IBoardFactoryService> GetFactoryServicesCore(IVisualBoardProvider source)
			{
				return base.GetFactoryServicesCore(source).Concat(services);
			}
		}

		#endregion

		#region Road Runner Status

		[TestDate(2017, 10, 23, 9, 0, 0)]
		public void TestTwoSectionsOnABoard_UseTheSameRoadRunnerStatusCacheService()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var board = BMSTestHelper.CreateBoard(system);

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, string.Empty, buffer);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var workingTask = CreateTask(workflow, staff.GS_Code, 35, taskStatus: ProcessTaskStatusCodeList.Codes.Working, description: "1");
			CreateTask(workflow, staff.GS_Code, 35, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "2");
			CreateTask(workflow, staff.GS_Code, 35, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, description: "3");

			var section1 = BMSTestHelper.CreateBoardSection(buffer, board, row: 0);
			var section2 = BMSTestHelper.CreateBoardSection(buffer, board, row: 1);
			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Resource, staff.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(section2, ChannelTypeList.Codes.Resource, staff.PK, overrideChannels: true);

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board);

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(2);

			var slideShowViewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow, useStatusCache: true);
			var service = slideShowViewModel.Services_ExposedForTest.OfType<RoadRunnerStatusCacheService>().First();

			using (var form = new VisualBoardForm(slideShowViewModel))
			{
				var staffRoadRunnerStatusCache = service.GetStaffCache_ForTesting(staff);
				AssertEquals("Precondition: Nothing in cache service", 0, staffRoadRunnerStatusCache.Count);
				form.Show();

				Application.DoEvents();

				var sections = slideShowViewModel.CurrentBoardViewModel.GetSections().OfType<BMBoardSectionViewModel>();
				var section1ViewModel = sections.ElementAt(0);
				var section2ViewModel = sections.ElementAt(1);

				AssertContainsExactElementsInAnyOrder("Cache is populated", new List<string>() { "GetWorkingTask", "AppendRelevantTimeText", "GetWorkingTaskInAnyBuffer", "GetRelevantActivityTime" }, staffRoadRunnerStatusCache.Keys);

				var channel1 = section1ViewModel.AllChannels.First();
				var channel2 = section2ViewModel.AllChannels.First();

				var cachedValue = staffRoadRunnerStatusCache["AppendRelevantTimeText"];

				var cacheHit = cachedValue;
				staffRoadRunnerStatusCache["AppendRelevantTimeText"] = cacheHit;
				channel1.ClearChannelCache();

				AssertEquals("Channel status is 'Working'", "Zone 3, Working for 2 hours", channel1.Status);

				staffRoadRunnerStatusCache["AppendRelevantTimeText"] = cacheHit;
				channel2.ClearChannelCache();

				AssertEquals("Channel on second section has cached status", "Zone 3, Working for 2 hours", channel2.Status);
			}
		}

		[TestDate(2017, 12, 1, 10, 0, 0)]
		public void TestChannelHeader_Fade_ShouldNotChangeOnRefresh()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, "RES", GlbStaff.CurrentUser.PK, overrideChannels: true);
			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var panel = form.FindSingle<ZFadePanel>();
				AssertEquals(Control.DefaultBackColor, panel.FadeStartColor);
			}
		}

#if !WINZOR
		[TestDate(2017, 12, 1, 10, 0, 0)]
		public void TestChannelHeader_ForResourceWorkingOvertime_ShouldHaveYellowHeader()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, "RES", GlbStaff.CurrentUser.PK, overrideChannels: true);
			var task = BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow", config.Buffer, staffCode: GlbStaff.CurrentUser.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Working, lowEstMinutes: 1);
			Factory.Save();

			TestDateAttribute.AddHours(1);

			using (var form = GetAndShowVisualBoardForm(config.BufferBoard))
			{
				var header = form.FindSingle<ChannelHeaderControl>();
				AssertEquals("The channel should be considered working overtime. SAD!", true, header.Channel.IsOvertime);

				var bitmap = new Bitmap(header.Width, header.Height);
				header.DrawToBitmap(bitmap, header.DisplayRectangle);
				var actualColour = bitmap.GetPixel(0, 0);

				AssertEquals("The channel header should be yellow. SAD!", BMConstants.CautionBoardColor.R, actualColour.R);
				AssertEquals("The channel header should be yellow. SAD!", BMConstants.CautionBoardColor.G, actualColour.G);
				// for some reason the blue is off... but this still proves it's the yellow colour and not red or blue.
			}
		}
#endif

		#endregion

		#region CCPM Things

		[TestDate(2015, 7, 14)]
		public void TestRefreshBoard_WhenMultipleSectionsAndApprovedDiagramsInvolved_BufferPenetrationShouldNotChangeIndependently()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Astockalypse", config.Buffer, ZDateTime.UtcNow.AddDays(-20), config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			var diagram = NetworkTestCase.CreateDiagram(Factory, scheduledStartTimeUTC: ZDateTime.UtcNow);
			var shape = NetworkTestCase.CreateShape(workflow, diagram);

			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();

			network.SwitchToScaled();
			viewModel.SuggestAndAcceptAllBuffers();
			viewModel.ToggleApproval();

			var board = BMSTestHelper.CreateBoard(config.System);
			var section1 = BMSTestHelper.CreateBoardSection(config.Buffer, board);
			var section2 = BMSTestHelper.CreateBoardSection(config.Buffer, board);

			section2.Row = 1;
			section1.SectionConfiguration.CellsPerSubsection = 13;
			section2.SectionConfiguration.CellsPerSubsection = 13;

			FilterStripsTestHelper.AddCustomSQLFilterStrip(section2.SectionConfiguration.WorkflowFilter, "1=2");

			Factory.Save();

			RunCCPMAndNCNTagRules(Factory);
			NetworkTestCase.RunBufferPenetrationUpdater();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var ticket = FindTaskCardControl(form, task);

				AssertNotNull(ticket);
				AssertEquals(0, ticket.Cell.TimeIndex);

				form.RefreshBoard();

				ticket = FindTaskCardControl(form, task);

				AssertNotNull(ticket);
				AssertEquals(0, ticket.Cell.TimeIndex);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestSavingDetailedTicket_WhenMultipleSectionsAndApprovedDiagramsInvolved_BufferPenetrationShouldNotChangeIndependently()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Astockalypse", config.Buffer, ZDateTime.UtcNow.AddDays(-20), config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code);

			var diagram = NetworkTestCase.CreateDiagram(Factory, scheduledStartTimeUTC: ZDateTime.UtcNow.AddDays(-10));
			var shape = NetworkTestCase.CreateShape(workflow, diagram);

			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();

			network.SwitchToScaled();
			NetworkTestCase.SetShapeSize(network.Entities.GetInstance(shape), network.Entities.GetInstance(diagram), 4000, 100); // Make the shape really big so it's buffer penetration supplies are low, even though it's time since startable supplies are high.

			viewModel.SuggestAndAcceptAllBuffers();
			viewModel.ToggleApproval();

			var board = BMSTestHelper.CreateBoard(config.System);
			var section1 = BMSTestHelper.CreateBoardSection(config.Buffer, board);
			var section2 = BMSTestHelper.CreateBoardSection(config.Buffer, board);

			section2.Row = 1;
			section1.SectionConfiguration.CellsPerSubsection = 13;
			section2.SectionConfiguration.CellsPerSubsection = 13;

			FilterStripsTestHelper.AddCustomSQLFilterStrip(section2.SectionConfiguration.WorkflowFilter, "1=2");

			Factory.Save();

			RunCCPMAndNCNTagRules(Factory);
			NetworkTestCase.RunBufferPenetrationUpdater();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var ticket = FindTaskCardControl(form, task);

				AssertNotNull(ticket);
				AssertEquals(0, ticket.Cell.TimeIndex);

				TestDateAttribute.AddMinutes(1);

				var detailedTicket = FindOrShowDetailedTicket(form, task);
				detailedTicket.ProcessTask.P9_CardNote = ":S";
				detailedTicket.Save_ForTest();

				ticket = FindTaskCardControl(form, task);

				AssertNotNull(ticket);
				AssertEquals(0, ticket.Cell.TimeIndex);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestSavingDetailedTicket_WhenCcpmBufferPenetrationVaries_BufferPenetrationShouldNotChangeIndependently()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Ken Leeeee", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, lowEstMinutes: 60, description: "Tulibu dibu douchu");

			const int cellsPerSection = 13;

			var section = config.BufferSection;
			section.SectionConfiguration.CellsPerSubsection = cellsPerSection;

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(workflow, diagram);

			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();

			network.SwitchToScaled();
			viewModel.SuggestAndAcceptAllBuffers();
			viewModel.ToggleApproval();

			Factory.Save();
			RunCCPMAndNCNTagRules(Factory);

			var schedule = shape.ScheduleBizo;

			AssertNotNull(schedule);

			using (var form = GetAndShowVisualBoardForm(section))
			{
				form.WindowState = FormWindowState.Maximized;

				const decimal bufferPenetrationIncrement = 100m / (cellsPerSection - 1); // The board section has 13 slots, but one is for zone 0 so 100% begins at slot 12.

				for (var i = 0; i < cellsPerSection; i++)
				{
					var ticket = FindTaskCardControl(form, task);
					AssertEquals("Ticket should be in the slot corresponding to the penetration of its approved shape", i, ticket.Cell.TimeIndex);

					form.RefreshBoard();
					Application.DoEvents();

					ticket = FindTaskCardControl(form, task);
					AssertEquals("Ticket should not have moved after refreshing the board", i, ticket.Cell.TimeIndex);

					var detailedTicket = FindOrShowDetailedTicket(form, task);
					detailedTicket.ProcessTask.P9_CardNote = "Refresh: " + i;
					detailedTicket.Save_ForTest();
					Application.DoEvents();

					ticket = FindTaskCardControl(form, task);
					AssertEquals("Ticket should not have moved after saving its detailed ticket", i, ticket.Cell.TimeIndex);

					schedule.BNC_BufferPenetrationPercent += bufferPenetrationIncrement + 0.01m; // A little bit extra to move to the next slot, rather than just the top of the current slot...
					Factory.Save();
					form.RefreshBoard();
					Application.DoEvents();
				}
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestSavingDetailedTicket_WhenCcpmBufferPenetrationVaries_ForChildWorkflowOfApprovedDiagramWorkflow_BufferPenetrationShouldNotChangeIndependently()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var parentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Ken Leeeee", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Tulibu dibu douchu", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(childWorkflow, resource.GS_Code, lowEstMinutes: 60, description: "Ken Lee Anymo");

			BMSTestHelper.CreateParentChildLink(parentWorkflow, childWorkflow, syncBufferPenetration: true);

			const int cellsPerSection = 13;

			var section = config.BufferSection;
			section.SectionConfiguration.CellsPerSubsection = cellsPerSection;

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(parentWorkflow, diagram);

			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();

			network.SwitchToScaled();
			viewModel.SuggestAndAcceptAllBuffers();
			viewModel.ToggleApproval();

			Factory.Save();
			RunCCPMAndNCNTagRules(Factory);

			var schedule = shape.ScheduleBizo;

			AssertNotNull(schedule);

			using (var form = GetAndShowVisualBoardForm(section))
			{
				form.WindowState = FormWindowState.Maximized;

				const decimal bufferPenetrationIncrement = 100m / (cellsPerSection - 1); // The board section has 13 slots, but one is for zone 0 so 100% begins at slot 12.

				for (var i = 0; i < cellsPerSection; i++)
				{
					var ticket = FindTaskCardControl(form, task);
					AssertEquals("Ticket should be in the slot corresponding to the penetration of its approved shape", i, ticket.Cell.TimeIndex);

					form.RefreshBoard();
					Application.DoEvents();

					ticket = FindTaskCardControl(form, task);
					AssertEquals("Ticket should not have moved after refreshing the board", i, ticket.Cell.TimeIndex);

					var detailedTicket = FindOrShowDetailedTicket(form, task);
					detailedTicket.ProcessTask.P9_CardNote = "Refresh: " + i;
					detailedTicket.Save_ForTest();
					Application.DoEvents();

					ticket = FindTaskCardControl(form, task);
					AssertEquals("Ticket should not have moved after saving its detailed ticket", i, ticket.Cell.TimeIndex);

					schedule.BNC_BufferPenetrationPercent += bufferPenetrationIncrement + 0.01m; // A little bit extra to move to the next slot, rather than just the top of the current slot...
					Factory.Save();
					form.RefreshBoard();
					Application.DoEvents();
				}
			}
		}

		#endregion

		#region Task Card Reset

		[TestDate(2020, 8, 7)]
		public void TestTaskCardPositioning_WhenSpecialPenetrationIsApplied_WithTwoTasksAndResetAppliedToOneTask()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var group = BMSTestHelper.CreateGroup(Factory, "GGG");
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			config.BufferSection.SectionConfiguration.ReleaseGroupPK = group.PK;

			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "AAA";
			staff1.GS_LoginName = "Aaron A. Aaronson";

			var staff2 = group.Staff.AddNew();
			staff2.GS_Code = "BBB";
			staff2.GS_LoginName = "Barry B. Bearonson";

			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, "RES", staff1.PK, overrideChannels: true);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, "RES", staff2.PK, overrideChannels: true);

			var link = config.Buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationThrottleFactor = 2;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "aaa", releaseGroupPK: group.PK.ToGuid());
			workflow.FH_TaskPenetrationResetDateTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-5);

			var taskWithReset = BMSTestHelper.CreateTask(workflow, staff1.GS_Code, taskType: "UDF");
			taskWithReset.P9_IsResetBeingAppliedToThisTask = true;

			var taskWithoutReset = BMSTestHelper.CreateTask(workflow, staff2.GS_Code, taskType: "UDF");
			taskWithoutReset.P9_IsResetBeingAppliedToThisTask = false;

			Factory.Save();

			using (var form = new VisualBoardForm(GetViewModel(config.BufferSection.Board)))
			{
				form.Show();
				Application.DoEvents();

				var taskCardResetApplied = form.FindAll<TaskCardControl>().Single(t => t.CardContent.GetTask(Factory).PK == taskWithReset.PK);
				var taskCardResetNotApplied = form.FindAll<TaskCardControl>().Single(t => t.CardContent.GetTask(Factory).PK == taskWithoutReset.PK);

				AssertEquals(true, taskCardResetApplied.Visible);
				AssertEquals(true, taskCardResetNotApplied.Visible);

				AssertEquals(11, taskCardResetApplied.Cell.Row);
				AssertEquals(9, taskCardResetNotApplied.Cell.Row);
			}
		}

		#endregion

		#region Aging Branch & Department

		public void TestShowBoardWithBucketSection_WhenBranchDepartmentNotSpecifiedOnBoardOrBucket_ShouldShowSection()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var board = config.BucketBoard;
			config.Bucket.FC_GB_AgingBranch = ZGuid.Empty;
			config.Bucket.FC_GE_AgingDepartment = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, config.BucketBoard.MB_GB_AgingBranch);
			AssertEquals(ZGuid.Empty, config.BucketBoard.MB_GE_AgingDepartment);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				var loadFailedLabel = form.FindSingleOrDefault<ZLabel>("LoadFailedLabel");
				AssertNull("The board section should have loaded correctly even without any aging branch/department specified on the board and component.", loadFailedLabel);
			}
		}

		#endregion

		#region Implementation

		internal static BoardSlideshowViewModel GetViewModel(IBMBoard board, bool useStatusCache = true)
		{
			return VisualBoardFormBasherTest.GetViewModel(board, useStatusCache);
		}

		internal static void MoveNext_ForTest(VisualBoardForm form, bool triggeredByButtonClick = false, TriggerableAsyncStrategy triggerable = null)
		{
			if (triggeredByButtonClick)
			{
				form.controlsPanel.Expand();
				form.controlsPanel.NextButton.PerformClick();
			}
			else
			{
				form.MoveNext();
			}

			DoAsyncSynchronously_ForTest(triggerable);
		}

		internal static void MovePrevious_ForTest(VisualBoardForm form, TriggerableAsyncStrategy triggerable = null)
		{
			form.controlsPanel.Expand();
			form.controlsPanel.PreviousButton.PerformClick();

			DoAsyncSynchronously_ForTest(triggerable);
		}

		internal static void RefreshAndAssertCurrentBoardViewModel_ForTest(Action refreshAction, VisualBoardForm form, BMBoard expectedBoard, BMSystem system)
		{
			refreshAction();

			var shownBoard = system.Boards.FirstOrDefault(b => b.PK == form.SlideShowViewModel.CurrentBoardViewModel.BoardPK);
			var message = ZString.Format("After refreshing the expected {0} was not shown, instead it shows {1}", expectedBoard.HumanReadableName, shownBoard != null ? shownBoard.HumanReadableName : ZString.Empty);
			AssertEquals(message, expectedBoard.PK, form.SlideShowViewModel.CurrentBoardViewModel.BoardPK);
		}

		protected override bool ShouldDisableAsyncBehaviour => true;

		#endregion

		#region Interlopers

		public void TestReloadTaskLocationWhenResized()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = BMSTestHelper.CreateGroup(Factory, "AAA");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow.FH_GG_ReleaseGroup = group.PK;
			var tasksCount = 14;
			Enumerable.Range(0, tasksCount).Select(num => BMSTestHelper.CreateTask(workflow)).ToArray();
			Factory.Save();

			using (var form = new VisualBoardFormForRefreshTest(GetViewModel(section.Board)))
			{
				form.Size = ControlDpiScalingHelper.NewScaledSize(1025, 1000);
				form.WindowState = FormWindowState.Normal;

				form.Show();
				Application.DoEvents();

				var tasks = form.FindAll<TaskCardControl>().Where(c => c.Visible);
				var taskPanelPaddingInPixels = ControlDpiScalingHelper.ScaleToCurrentDpiX(TaskPanel.TaskCardPadding);
				var numInFirstColumn = tasks.Count(t => t.Location.X == taskPanelPaddingInPixels);

				form.Size = form.MinimumSize;

				Application.DoEvents();
				Assert("The number of tasks in the first column should be less than the original number. The tasks have not reloaded.", tasks.Count(t => t.Location.X == taskPanelPaddingInPixels) < numInFirstColumn);

				form.Size = ControlDpiScalingHelper.NewScaledSize(1025, 1000);
				AssertEquals("The tasks should have reloaded and changed location.", numInFirstColumn, tasks.Count(t => t.Location.X == taskPanelPaddingInPixels));
			}
		}

		public void TestOpenVisualBoardInLastLocation()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);

			Factory.Save();
			var formLocation = new Point(100, 100);

			using (var form = new VisualBoardForm(GetViewModel(section.Board)))
			{
				form.Show();
				form.WindowState = FormWindowState.Normal;
				AssertNotEquals("Precondition", form.MinimumSize, form.Size);
				form.Size = form.MinimumSize;
				form.Location = formLocation;
				Application.DoEvents();
				form.Close();

				form.Show();
				Application.DoEvents();

				AssertEquals("The form size should remain as per last opened", form.MinimumSize, form.Size);
				AssertEquals("The form location should remain as per last opened", formLocation, form.Location);
			}
		}

		public void TestNoNullReferenceException_WhenDragTicketAndRefreshInactiveForm()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(config.Bucket, numberOfWorkflows: 1, numberOfTasksPerWorkflow: 10, releaseGroup: config.ReleaseGroup);
			BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(config.Buffer, numberOfWorkflows: 1, numberOfTasksPerWorkflow: 10);

			BMSTestHelper.CreateBoardSection(config.Buffer, config.BucketBoard, row: 1);

			var anotherBoard = BMSTestHelper.CreateBoard(config.System, "AnotherBoard");
			BMSTestHelper.CreateBoardSection(config.Buffer, anotherBoard, row: 1);

			var viewModel = GetViewModel(config.BucketBoard);
			var viewModel2 = GetViewModel(anotherBoard);

			Factory.Save();

			using (var form = new VisualBoardForm(viewModel))
			using (var form2 = new VisualBoardForm(viewModel2))
			{
				form.Show();
				Application.DoEvents();

				var taskCard = form.FindAll<TaskCardControl>().First();

				taskCard.OnDragDropStarting();
				taskCard.Left = taskCard.Left + 10;
				taskCard.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
				Application.DoEvents();
				taskCard.OnDragDropFinished();

				form2.Show();
				Application.DoEvents();

				form.Size = form.MinimumSize;
				Application.DoEvents();

				form.RefreshBoard();
				Application.DoEvents();

				AssertEquals("The NullReferenceException should not be reported.", "", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		void TestDisplayWorkflowItemsMacro(string macro, string property)
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Environment.Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ADI", "Adiaga");

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);

			var customisation = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
			customisation.FM_JobType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			customisation.Height = 300;

			BMSTestHelper.CreateControlCustomisationLink(Factory, section, customisation);

			var line = BMSTestHelper.CreateLine(customisation, PropertySourceList.Codes.Job, macro, PropertyTypeList.Codes.Text);

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(newFactory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Work", currentComponent: config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workingTask = BMSTestHelper.CreateTask(workflow1, resource.GS_Code, description: "Task", taskStatus: ProcessTaskStatusCodeList.Codes.Working);
			BMSTestHelper.CreateTask(workflow1, resource.GS_Code, description: "Cask");
			BMSTestHelper.CreateTask(workflow1, resource.GS_Code, description: "Bask");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Worm", currentComponent: config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var cardTask = BMSTestHelper.CreateTask(workflow2, resource.GS_Code, description: "Rask");
			BMSTestHelper.CreateTask(workflow2, resource.GS_Code, description: "Gask");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Wort", currentComponent: config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(workflow3, resource.GS_Code, description: "Zask");

			newFactory.Save();

			using (var form = GetAndShowVisualBoardForm(section.Board, width: 1600))
			{
				var taskCard = BMSGUITestCase.FindTaskCardControls(form, cardTask).Single();
				Application.DoEvents();

				var status = (ZString)((BusinessObject)taskCard.CardContent.Bindable)[property];
				AssertEquals("WRK", status);

				workingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
				newFactory.Save();
				form.RefreshBoard();
				Application.DoEvents();

				taskCard = BMSGUITestCase.FindTaskCardControls(form, cardTask).Single();
				Application.DoEvents();

				status = (ZString)((BusinessObject)taskCard.CardContent.Bindable)[property];
				AssertEquals("SUS", status);

				workingTask.P9_Description = "Trask";
				newFactory.Save();
				form.RefreshBoard();
				Application.DoEvents();

				taskCard = BMSGUITestCase.FindTaskCardControls(form, cardTask).Single();
				Application.DoEvents();

				status = (ZString)((BusinessObject)taskCard.CardContent.Bindable)[property];
				AssertEquals(ZString.Empty, status);
			}
		}

		public void TestDisplayWorkflowItemsMacro_First()
		{
			TestDisplayWorkflowItemsMacro("<WorkflowItems.First(\"<P9_Description>\" == \"Task\").P9_Status>", "JOB_WorkflowItems_First(\"<P9_Description>\" == \"Task\")_P9_Status");
		}

		public void TestDisplayWorkflowItemsMacro_FirstOrDefault()
		{
			TestDisplayWorkflowItemsMacro("<WorkflowItems.FirstOrDefault(\"<P9_Description>\" == \"Task\").P9_Status>", "JOB_WorkflowItems_FirstOrDefault(\"<P9_Description>\" == \"Task\")_P9_Status");
		}

		public void TestDisplayCustomFields_UsingGetCustomFieldMacro()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Environment.Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "TWY", "Tammy Wynette");

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK);

			var taskCardCustomisation = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.TaskCard);
			taskCardCustomisation.FM_JobType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			taskCardCustomisation.Height = 300;

			var taskCardLine = BMSTestHelper.CreateLine(taskCardCustomisation, PropertySourceList.Codes.Job, "<GetCustomField(Icecream Type)>", PropertyTypeList.Codes.Text);
			taskCardLine.Top = 200;

			var detailedCardCustomisation = BMControlCustomisation.GetNewDefaultCardLayout(Factory, CustomisedControlTypeList.Codes.DetailedCard);
			detailedCardCustomisation.FM_JobType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			detailedCardCustomisation.Height = 300;

			var detailedCardLine = BMSTestHelper.CreateLine(detailedCardCustomisation, PropertySourceList.Codes.Job, "<GetCustomField(Icecream Type)>", PropertyTypeList.Codes.Text);
			detailedCardLine.Top = 200;

			BMSTestHelper.CreateControlCustomisationLink(Factory, section, taskCardCustomisation);
			BMSTestHelper.CreateControlCustomisationLink(Factory, section, detailedCardCustomisation);

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };

			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

			var custom = template.GenCustomColumnDefinitions.AddNew();
			custom.XC_Name = "Icecream Type";
			custom.XC_Type = AddOnColumnDataType.Codes.String;

			newFactory.Save();

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(newFactory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Work", currentComponent: config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, description: "Task");
			var job = (OrgHeader)jobHeader.Parent;

			var customValue = newFactory.New<GenCustomAddOnValue>();
			customValue.XV_ParentID = jobHeader.FH_ParentId;
			customValue.XV_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			customValue.XV_Name = custom.XC_Name;
			customValue.XV_Type = custom.XC_Type;
			customValue.XV_Data = "Mine's a '99";

			newFactory.Save();

			using (var form = GetAndShowVisualBoardForm(section.Board, width: 1600))
			{
				var taskCard = BMSGUITestCase.FindTaskCardControls(form, task).Single();
				Application.DoEvents();

				var status = (ZString)((BusinessObject)taskCard.CardContent.Bindable)["JOB_GetCustomField(Icecream Type)"];
				AssertEquals("Mine's a '99", status);

				taskCard.ShowDetailedCard();

				var detailedCard = form.FindAll<TaskCardDetailControl>().Single();
				Application.DoEvents();

				AssertNotNull(detailedCard);

				var textBoxes = detailedCard.FindAll<ZTextBox>().Where(w => w.Text == "Mine's a '99");
				Application.DoEvents();
				AssertEquals(1, textBoxes.Count());

				var textBox = textBoxes.First();
				textBox.Focus();
				textBox.Text = "Mummu Macadamia";
				Application.DoEvents();

				detailedCard.Save_ForTest();
				Application.DoEvents();

				customValue.Reload();

				AssertEquals("Mummu Macadamia", customValue.XV_Data);
			}
		}

		#endregion

		#region Disposing

		public void TestVisualBoardFormDispose_RefreshDuringDisposing()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var section = BMSTestHelper.CreateBoardSection(config.Buffer);
			Factory.Save();
			using (var form = GetAndShowVisualBoardForm(section.Board, width: 1600))
			{
				form.AwaitAll();
				form.AutoRefreshForTest.Dispose();
				AssertNoExceptionThrown(form.RefreshBoard);
			}
		}

		#endregion

		#region Setup and TearDown

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.DisableAcceptabilityBandResultCache();
		}

		#endregion

		#region Error creating window handle

		class VisualBoardForm_ForTestErrorCreatingWindowHandle : VisualBoardForm
		{
			public VisualBoardForm_ForTestErrorCreatingWindowHandle(BoardSlideshowViewModel viewModel) : base(viewModel) { }

			protected override bool SetupTableLayoutCore(BoardDataSource dataSource, bool ignoreCache, BoardRefreshEventArgs args)
			{
				throw new Win32Exception(0, "Error creating window handle") { Source = "System.Windows.Forms" };
			}
		}

		public void TestShowsErrorMessageWhenErrorCreatingHandle()
		{
			var system = Factory.New<BMSystem>();
			system.FS_Name = "WTGDEV";

			var board = system.Boards.AddNew();
			board.MB_Name = "International Logistics";
			board.MB_Description = "Nope.";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			BMSTestHelper.CreateReleaseGroup(system, group);

			Factory.Save();

			using (var form = new VisualBoardForm_ForTestErrorCreatingWindowHandle(BMSTestHelper.CreateSlideshowViewModel(board)) { Size = ControlDpiScalingHelper.NewScaledSize(1024, 768) })
			{
				form.Show();
				AssertEquals($"The board [{board.MB_Name}] failed to display properly. Please close and reopen this window.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion
	}

	#region TooManyItemsTests

	class VisualBoardFormTooManyItemsTest : BMSGUITestCase
	{
		public void TestShowForm_WhenTooManyItemsForPrimaryChannels_ButSecondaryChannelsWouldFilterFurther_ShouldDisplaySection()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "LLA", "Llamas");
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "MOO", "A Møøse once bit my sister");
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup, "KAR", "She was Karving her initials on the møøse");

			BMSTestHelper.CreatePrimaryChannelForSection(section1, ChannelTypeList.Codes.Tag, tag1.PK);

			var workflows = BMSTestHelper.CreateWorkflows<OrgHeader>(bucket1, numberOfWorkflows: MaxItemsPerSection + 1, numberOfTasksPerWorkflow: 1, releaseGroup: group);
			var doubleTaggedWorkflow = workflows[0];

			foreach (var workflow in workflows)
			{
				workflow.AddTag(tag1);
			}

			doubleTaggedWorkflow.AddTag(tag2);

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				AssertTooManyItemsLabelShown(form, section1, true);
				AssertTooManyItemsLabelShown(form, section2, false);
			}

			BMSTestHelper.CreateSecondaryChannelForSection(section1, ChannelTypeList.Codes.Tag, tag2.PK);
			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				AssertTooManyItemsLabelShown(form, section1, false);
				AssertTooManyItemsLabelShown(form, section2, false);

				var componentControl = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section1.PK);
				var ticket = FindTaskCardControl(componentControl, doubleTaggedWorkflow.Tasks.Single());

				AssertNotNull(ticket);
			}
		}

		public void TestShowForm_WhenTooManyItemsOnOneSection_ShouldShowLabelInsteadOfSection()
		{
			BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(bucket1, numberOfWorkflows: 1, numberOfTasksPerWorkflow: MaxItemsPerSection + 10, releaseGroup: group);
			BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(bucket2, numberOfWorkflows: 1, numberOfTasksPerWorkflow: MaxItemsPerSection - 10, releaseGroup: group);
			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				AssertTooManyItemsLabelShown(form, section1, true);
				AssertTooManyItemsLabelShown(form, section2, false);
			}
		}

		public void TestShowForm_WhenTooManyTasksButNotWorkflows_ShouldShowSection()
		{
			BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(bucket1, numberOfWorkflows: 1, numberOfTasksPerWorkflow: MaxItemsPerSection + 10, releaseGroup: group); // Single workflow, many tasks
			BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(bucket2, numberOfWorkflows: MaxItemsPerSection + 10, numberOfTasksPerWorkflow: 1, releaseGroup: group); // Many workflows, single tasks

			section1.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;
			section2.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				AssertTooManyItemsLabelShown(form, section1, false);
				AssertTooManyItemsLabelShown(form, section2, true);

				var section1Control = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == section1.PK);

				AssertEquals(1, section1Control.FindAll<TaskCardControl>().ToArray().Length);
			}

			section1.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;
			section2.SectionConfiguration.CardType = CardTypeList.Codes.JobLevelWorkflow;

			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				AssertTooManyItemsLabelShown(form, section1, false);
				AssertTooManyItemsLabelShown(form, section2, true);
			}
		}

		public void TestShowForm_WhenTooManyItemsOnMultipleSections_ShouldShowTwoLabels()
		{
			BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(bucket1, numberOfWorkflows: 1, numberOfTasksPerWorkflow: MaxItemsPerSection + 10, releaseGroup: group);
			BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(bucket2, numberOfWorkflows: 1, numberOfTasksPerWorkflow: MaxItemsPerSection + 10, releaseGroup: group);
			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				AssertTooManyItemsLabelShown(form, section1, true);
				AssertTooManyItemsLabelShown(form, section2, true);
			}
		}

		public void TestShowForm_ShouldFilterTasksBeforeLoad()
		{
			var workflows = BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(bucket1, numberOfWorkflows: 1, numberOfTasksPerWorkflow: MaxItemsPerSection * 2, releaseGroup: group);
			workflows[0].Tasks.ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);
			workflows[0].Tasks.First().P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(bucket1, numberOfWorkflows: 1, numberOfTasksPerWorkflow: MaxItemsPerSection + 1, releaseGroup: group);
			Factory.Save();

			using (var form = GetAndShowVisualBoardForm(board))
			{
				AssertTooManyItemsLabelShown(form, section1, true);
				AssertTooManyItemsLabelShown(form, section2, false);
			}
		}

		BMComponent bucket1, bucket2;
		BMBoardSection section1, section2;
		BMBoard board;
		GlbGroup group;

		const int MaxItemsPerSection = 100;

		static void AssertTooManyItemsLabelShown(VisualBoardForm form, BMBoardSection section, bool shouldLabelBeShown)
		{
			// Make sure the board didn't die horribly, for some reason.
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(false, form.IsDisposed);

			var control = form.FindSingle<BMComponentControl>(c => c.ViewModel.SectionPK == section.PK);
			var label = control.FindSingleOrDefault<ZLabel>("LoadFailedLabel");

			if (shouldLabelBeShown)
			{
				AssertNotNull(label);
				AssertEquals($"This section ({section.SectionName}) cannot be displayed, as the number of items to be shown exceeds the maximum allowed (100). Please revise filters and configuration of this section.", label.Text);
			}
			else
			{
				AssertNull(label);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.MaxNumberOfItemsOnBoards.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MaxItemsPerSection);

			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");

			group = BMSTestHelper.CreateGroup(Factory, "AAA");

			board = BMSTestHelper.CreateBoard(system);
			section1 = BMSTestHelper.CreateBoardSection(bucket1, board);
			section2 = BMSTestHelper.CreateBoardSection(bucket2, board);
			section1.Row = 0;
			section2.Row = 1;
			section1.RowHeightPercent = section2.RowHeightPercent = 50;
			section1.SectionConfiguration.ReleaseGroupPK = group.PK;
			section2.SectionConfiguration.ReleaseGroupPK = group.PK;
		}

		protected override bool ShouldDisableAsyncBehaviour => true;
	}

	class VisualBoardFormSlideShowTooManyItemsTest : NonTransactionedTestCase
	{
		public void TestShowSlideShow_WhenTooManyItemsOnOneSection_ShouldShowLabelInsteadOfSection()
		{
			BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(bucket1, numberOfWorkflows: 1, numberOfTasksPerWorkflow: MaxItemsPerSection + 10, releaseGroup: group);
			BMSTestHelper.CreateWorkflows<DummyWithWorkflow>(bucket2, numberOfWorkflows: 1, numberOfTasksPerWorkflow: MaxItemsPerSection - 10, releaseGroup: group);
			Factory.Save();

			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();

				VisualBoardFormTest.DoAsyncSynchronously_ForTest();

				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(false, form.IsDisposed);

				var label = form.FindAll<ZLabel>().Single(l => l.Name == "LoadFailedLabel");

				AssertEquals("This section (bucket1) cannot be displayed, as the number of items to be shown exceeds the maximum allowed (100). Please revise filters and configuration of this section.", label.Text);

				VisualBoardFormTest.MoveNext_ForTest(form, triggeredByButtonClick: true);
				AssertEquals("Should advance a slide instead of having a disabled button", form.SlideShowViewModel.CurrentBoardViewModel.BoardPK, board2.PK);

				label = form.FindAll<ZLabel>().Single(l => l.Name == "LoadFailedLabel");
				AssertEquals("This section (bucket1) cannot be displayed, as the number of items to be shown exceeds the maximum allowed (100). Please revise filters and configuration of this section.", label.Text);
			}
		}

		BMComponent bucket1, bucket2;
		BMBoard board1, board2;
		BoardSlideshowViewModel viewModel;
		GlbGroup group;

		const int MaxItemsPerSection = 100;

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.MaxNumberOfItemsOnBoards.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MaxItemsPerSection);

			group = BMSTestHelper.CreateGroup(Factory, "AAA");

			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");

			board1 = BMSTestHelper.CreateBoard(system);
			board2 = BMSTestHelper.CreateBoard(system, "ab board");
			var section1 = BMSTestHelper.CreateBoardSection(bucket1, board1);
			section1.SectionConfiguration.ReleaseGroupPK = group.PK;
			var section2 = BMSTestHelper.CreateBoardSection(bucket2, board1);
			section2.SectionConfiguration.ReleaseGroupPK = group.PK;
			section1.Row = 0;
			section2.Row = 1;

			var section3 = BMSTestHelper.CreateBoardSection(bucket1, board2);
			section3.SectionConfiguration.ReleaseGroupPK = group.PK;

			section1.RowHeightPercent = section2.RowHeightPercent = 50;

			var slideshow = VisualBoardsTestHelper.CreateSlideshow(Factory, board1, board2);

			viewModel = BMSTestHelper.CreateSlideshowViewModel(slideshow);
		}
	}

	#endregion

	#region BusinessObjectsInMemoryTest

	class VisualBoardFormBusinessObjectsPerformanceTest : BMSGUITestCase
	{
		[TestDate(2014, 5, 28)]
		public void TestVisualBoardFormLoadPerformance_DontLoadRedundantWorkflowsForCapabilityChannels()
		{
			var capabilities = CreateTestData_CapabilityWorkflows();
			CreateCapabilityBucketSection(capabilities);

			ShowFormAndDo(form =>
			{
				var viewModel = form.BoardViewModel.GetSections().First();
				var stats = new PerformanceStatistic();
				var componentControlFactory = stats.FactoryStatistics.Cast<BusinessObjectFactoryStatistic>().Select(s => s.FactoryInternals).SingleOrDefault(f => ((BusinessObjectFactory)f).NameForDebugging.Contains("SetupTasks"));

				AssertNotNull(componentControlFactory);

				CombineAssertions(() =>
				{
					AssertLoadCount<GlbCapability>(2, componentControlFactory);
					AssertLoadCount<ProcessTask>(6, componentControlFactory);
					AssertLoadCount<ProcessHeader>(8, componentControlFactory);
				});
			});
		}

		void AssertLoadCount<T>(int count, IBusinessObjectFactoryInternals factoryInternals)
		{
			AssertEquals(string.Format("Expect {0} objects of type [{1}]", count, typeof(T).ToString()), count, factoryInternals.AllBusinessObjects.OfType<T>().Count());
		}

		void ShowFormAndDo(Action<VisualBoardForm> formAction, bool enableTracking = true)
		{
			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			using (enableTracking ? PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest() : DisposableAction.NoAction)
			{
				form.Show();
				formAction(form);
			}
		}

		[TestDate(2014, 5, 28)]
		public void TestBusinessObjectsHeldInMemory_BufferSection_ShouldOnlyRetainItemsNeededForDisplayingOnBoard()
		{
			CreateTestData_SimpleWorkflows();
			CreateBufferSection();

			ShowFormAndAssertExpectedBizosHeldInMemory();
		}

		[TestDate(2014, 5, 28)]
		public void TestBusinessObjectsHeldInMemory_ReleaseSchedulerSection_ShouldOnlyRetainItemsNeededForDisplayingOnBoard()
		{
			CreateTestData_SimpleWorkflows();
			section = CreateReleaseSchedulerBoardSection(buffer, group);

			ShowFormAndAssertExpectedBizosHeldInMemory();
		}

		[TestDate(2014, 5, 28)]
		public void TestBusinessObjectsHeldInMemory_BucketSection_ShouldOnlyRetainItemsNeededForDisplayingOnBoard()
		{
			CreateTestData_SimpleWorkflows();
			CreateBucketSection();

			ShowFormAndAssertExpectedBizosHeldInMemory();
		}

		void ShowFormAndAssertExpectedBizosHeldInMemory()
		{
			Factory.Save();

			using (BMSTestCaseWithFactory.DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(VisualBoardFormTest.GetViewModel(section.Board)))
			{
				form.Show();

				using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
				{
					for (int i = 0; i < 5; i++)
					{
						form.RefreshNow_ForTest(forceReload: false);
						Application.DoEvents();
					}

					GC.Collect(); // Test case - go away
					GC.WaitForFullGCComplete();

					var control = form.FindAll<BMComponentControl>().Single();
					var componentControlFactory = BMSGUITestCase.GetSetupTasksFactory();

					AssertNotNull(componentControlFactory);

					AssertBusinessObjectsHeldInFactory(componentControlFactory, 3, 3, 3, "Should only load objects which are directly displayed in the section");
				}
			}
		}

		static void AssertBusinessObjectsHeldInFactory(BusinessObjectFactory factory, int expectedJobHeaders, int expectedWorkflows, int expectedTasks, string message)
		{
			AssertBusinessObjectsHeldInFactory(message, factory, true,
				Tuple.Create(typeof(ProcessJobHeader), expectedJobHeaders),
				Tuple.Create(typeof(ProcessHeader), expectedWorkflows)
				);

			AssertBusinessObjectsHeldInFactory(message, factory, false,
				Tuple.Create(typeof(ProcessTask), expectedTasks)
				);
		}

		IEnumerable<GlbCapability> CreateTestData_CapabilityWorkflows()
		{
			var system = CreateSystem("ORG");
			bucket = CreateBucket(system);

			var unrelatedBucket = CreateBucket(system, "unrelatedBucket"); // So that release scheduler section has only tasks in Released sections, preventing loading tasks in other workflows. This allows us to assert that unneeded tasks aren't loaded.
			buffer = CreateBuffer(system);

			LinkComponents(unrelatedBucket, buffer);
			LinkComponents(bucket, unrelatedBucket);

			var capability1 = CreateCapability("WPM", "The pogo bogo");
			var capability2 = CreateCapability("STL", "Fairy Dust");

			var resource1 = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins", capability1);
			var resource2 = CreateStaffInCurrentBranchDept("SAM", "Samwise Gamgee", capability2);
			var resource3 = CreateStaffInCurrentBranchDept("BIL", "Bilbo Baggins", capability1, capability2);

			group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.AddRange(resource1, resource2, resource3);
			var releaseGroup = CreateReleaseGroup(system, group, buffer);

			resource1.DesignateAsCCR(buffer);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();
			var jobHeader4 = CreateJobHeader<OrgHeader>();

			var workflow1_1 = CreateWorkflow(jobHeader1, "workflow1_1", bucket, releaseGroupPK: group.PK);
			var workflow1_2 = CreateWorkflow(jobHeader1, "workflow1_2", bucket, ZDateTime.UtcNow.AddDays(-15), releaseGroupPK: group.PK);

			var workflow2_1 = CreateWorkflow(jobHeader2, "workflow2_1", bucket, releaseGroupPK: group.PK);
			var workflow2_2 = CreateWorkflow(jobHeader2, "workflow2_2", bucket, ZDateTime.UtcNow.AddDays(-5), releaseGroupPK: group.PK);

			var workflow3_1 = CreateWorkflow(jobHeader3, "workflow3_1", bucket, releaseGroupPK: group.PK);
			var workflow3_2 = CreateWorkflow(jobHeader3, "workflow3_2", bucket, releaseGroupPK: group.PK);

			var workflow4_1 = CreateWorkflow(jobHeader4, "workflow4_1", unrelatedBucket, releaseGroupPK: group.PK);
			var workflow4_2 = CreateWorkflow(jobHeader4, "workflow4_2", unrelatedBucket, releaseGroupPK: group.PK);

			workflow1_2.MakePrerequisiteOf(workflow1_1);
			workflow2_2.MakePrerequisiteOf(workflow2_1);
			workflow3_2.MakePrerequisiteOf(workflow3_1);

			var task1_1 = CreateTask(workflow1_1, resource1.GS_Code, 60, capability: capability1);
			var task1_2 = CreateTask(workflow1_2, resource1.GS_Code, 60, capability: capability1);

			var task2_1 = CreateTask(workflow2_1, string.Empty, 60, capability: capability2);
			var task2_2 = CreateTask(workflow2_2, string.Empty, 60);

			var task3_1 = CreateTask(workflow3_1, resource3.GS_Code, 60, capability: capability1);
			var task3_2 = CreateTask(workflow3_2, resource3.GS_Code, 60, capability: capability2);

			var task4_1 = CreateTask(workflow4_1, GlbStaff.CurrentUser.GS_Code, 60, capability: capability1);
			var task4_2 = CreateTask(workflow4_2, GlbStaff.CurrentUser.GS_Code, 60);

			return new[] { capability1, capability2 };
		}

		void CreateTestData_SimpleWorkflows()
		{
			var system = CreateSystem("ORG");
			bucket = CreateBucket(system);

			var unrelatedBucket = CreateBucket(system, "unrelatedBucket"); // So that release scheduler section has only tasks in Released sections, preventing loading tasks in other workflows. This allows us to assert that unneeded tasks aren't loaded.
			buffer = CreateBuffer(system);

			LinkComponents(unrelatedBucket, buffer);
			LinkComponents(bucket, unrelatedBucket);

			var resource1 = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var resource2 = CreateStaffInCurrentBranchDept("SAM", "Samwise Gamgee");
			var resource3 = CreateStaffInCurrentBranchDept("BIL", "Bilbo Baggins");

			group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.AddRange(resource1, resource2, resource3);
			var releaseGroup = CreateReleaseGroup(system, group, buffer);

			resource1.DesignateAsCCR(buffer);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();
			var jobHeader4 = CreateJobHeader<OrgHeader>();

			var workflow1_1 = CreateWorkflow(jobHeader1, "workflow1_1", bucket, releaseGroupPK: group.PK);
			var workflow1_2 = CreateWorkflow(jobHeader1, "workflow1_2", buffer, ZDateTime.UtcNow.AddDays(-15), releaseGroupPK: group.PK);
			var workflow2_1 = CreateWorkflow(jobHeader2, "workflow2_1", bucket, releaseGroupPK: group.PK);
			var workflow2_2 = CreateWorkflow(jobHeader2, "workflow2_2", buffer, ZDateTime.UtcNow.AddDays(-5), releaseGroupPK: group.PK);
			var workflow3_1 = CreateWorkflow(jobHeader3, "workflow3_1", bucket, releaseGroupPK: group.PK);
			var workflow3_2 = CreateWorkflow(jobHeader3, "workflow3_2", buffer, releaseGroupPK: group.PK);
			var workflow4_1 = CreateWorkflow(jobHeader4, "workflow4_1", unrelatedBucket, releaseGroupPK: group.PK);
			var workflow4_2 = CreateWorkflow(jobHeader4, "workflow4_2", unrelatedBucket, releaseGroupPK: group.PK);

			workflow1_2.MakePrerequisiteOf(workflow1_1);
			workflow2_2.MakePrerequisiteOf(workflow2_1);
			workflow3_2.MakePrerequisiteOf(workflow3_1);

			var task1_1 = CreateTask(workflow1_1, resource1.GS_Code, 60);
			var task1_2 = CreateTask(workflow1_2, resource1.GS_Code, 60);
			var task2_1 = CreateTask(workflow2_1, resource2.GS_Code, 60);
			var task2_2 = CreateTask(workflow2_2, resource2.GS_Code, 60);
			var task3_1 = CreateTask(workflow3_1, resource3.GS_Code, 60);
			var task3_2 = CreateTask(workflow3_2, resource3.GS_Code, 60);
			var task4_1 = CreateTask(workflow4_1, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working); // Force SetupTasks factory to be retained, held by RoadRunnerDetails. When SetupTasks factory is no longer held, most of these tests are redundant.
			var task4_2 = CreateTask(workflow4_2, GlbStaff.CurrentUser.GS_Code, 60);
		}

		void CreateBufferSection(int expectedChannelCount = 3)
		{
			section = CreateBoardSection(buffer);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			CreateZoneMultiplier(buffer);

			AssertEquals(expectedChannelCount, section.SectionConfiguration.PrimaryAxisChannels.Count);
		}

		void CreateBucketSection()
		{
			section = CreateBoardSection(bucket);
			section.SectionConfiguration.ReleaseGroupPK = group.PK;
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			section.SectionConfiguration.ShowWorkInReleaseGroupOnly = false;

			AssertEquals(3, section.SectionConfiguration.PrimaryAxisChannels.Count);
		}

		void CreateCapabilityBucketSection(IEnumerable<GlbCapability> capabilities)
		{
			section = CreateBoardSection(bucket);
			section.SectionConfiguration.ChannelBy = ChannelTypeList.Codes.Capability;
			section.SectionConfiguration.OverrideChannels = true;

			foreach (var capability in capabilities)
			{
				var channel = section.SectionConfiguration.PrimaryAxisChannels.AddNew();
				channel.MSC_ChannelType = ChannelTypeList.Codes.Capability;
				channel.MSC_ParentID = capability.PK;
			}
		}

		BMComponent bucket, buffer;
		BMBoardSection section;
		GlbGroup group;
	}

	#endregion
}

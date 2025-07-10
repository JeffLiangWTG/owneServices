using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.VisualBoards.GUI.Test
{
	class BoardControlsPanelTest : VisualBoardsTestCase
	{
		public void TestWhenSlideShowOnlyHasOneBoard_ThenShouldShowControlsButInactive()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system, "bucket", offsetMinutes: 0, sequence: 0);

			var board1 = BMSTestHelper.CreateBoard(system, "board1", "board1");

			var section1 = BMSTestHelper.CreateBoardSection(bucket, board1);
			section1.BackgroundColor = "Blue";

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board1, 1));

			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();

			var viewModel = VisualBoardsTestHelper.CreateSlideshowViewModel(slideshow);

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				var controls = form.controlsPanel;

				Assert("Control should be collapsed on startup", !controls.IsExpanded);

				Application.DoEvents();
				controls.Expand();
				Application.DoEvents();

				CombineAssertions("Previous and next buttons should be shown but inactive, PauseResume button and Countdown label should be shown refresh instead of next slide", () =>
				{
					Assert("PreviousButton visible", controls.PreviousButton.Visible);
					AssertEquals("PreviousButton enable", true, controls.PreviousButton.Enabled);
					AssertEquals("PreviousButton tooltip", "Previous Slide", controls.PreviousButton.ToolTipCaption);
					controls.PreviousButton.PerformClick();
					AssertEquals("PreviousButton clicked", "There is only one board on this Slide Show.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					AssertNullOrEmpty("Precondition: LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);

					Assert("NextButton visible", controls.NextButton.Visible);
					AssertEquals("NextButton enable", true, controls.NextButton.Enabled);
					AssertEquals("NextButton tooltip", "Next Slide", controls.NextButton.ToolTipCaption);
					controls.NextButton.PerformClick();
					AssertEquals("NextButton clicked", "There is only one board on this Slide Show.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("PauseResumeButton tooltip - pause", "Pause board refresh", controls.PauseResumeButton.ToolTipCaption);

					var toolTip = ToolTipService.GetToolTip(controls.CountdownLabel);
					AssertEquals("Countdownlabel", "Time to board refresh", toolTip);

					controls.PauseResume(pause: true);
					AssertEquals("PauseResumeButton tooltip - resume", "Resume board refresh", controls.PauseResumeButton.ToolTipCaption);
				});
			}
		}

		public void TestBoardControlsPanel_SingleBoard()
		{
			var viewModel = CreateBoardWithTasks();
			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				var controls = form.controlsPanel;
				Assert("Control should be collapsed on startup", !controls.IsExpanded);
				Application.DoEvents();
				AssertEquals("Control width should be correct for collapsed", ControlDpiScalingHelper.ScaleToCurrentDpiX(3 * BoardControlsPanel.ButtonPadding) + controls.ExpandCollapseButton.Width + controls.CountdownLabel.Width, controls.Width);
				AssertEquals("10:00", controls.CountdownLabel.Text);
				controls.Expand();
				Application.DoEvents();
				bool correctControlsOrder = controls.SearchBox.Right < controls.FilterButton.Left &&
											controls.FilterButton.Right < controls.BoardMeetingButton.Left &&
											controls.BoardMeetingButton.Right < controls.BoardPickerButton.Left &&
											controls.BoardPickerButton.Right < controls.OpenInBrowserButton.Left &&
											controls.OpenInBrowserButton.Right < controls.ConfigButton.Left &&
											controls.ConfigButton.Right < controls.RefreshButton.Left &&
											controls.RefreshButton.Right < controls.PauseResumeButton.Left &&
											controls.PauseResumeButton.Right < controls.CountdownLabel.Left;
				Assert("Controls on panel should be in correct order", correctControlsOrder);
				Assert(controls.SearchBox.Visible);
				Assert(controls.FilterButton.Visible);
				Assert(controls.BoardMeetingButton.Visible);
				Assert(controls.BoardPickerButton.Visible);
				Assert(controls.OpenInBrowserButton.Visible);
				Assert(controls.ConfigButton.Visible);
				Assert(controls.RefreshButton.Visible);
				Assert(controls.PauseResumeButton.Visible);

				int correctWidth = BoardControlsPanel.ButtonPadding +
					controls.ExpandCollapseButton.Width + BoardControlsPanel.ButtonPadding +
					controls.SearchBox.Width + BoardControlsPanel.ButtonPadding +
					controls.FilterButton.Width + BoardControlsPanel.ButtonPadding +
					controls.BoardMeetingButton.Width + BoardControlsPanel.ButtonPadding +
					controls.BoardPickerButton.Width + BoardControlsPanel.ButtonPadding +
					controls.OpenInBrowserButton.Width + BoardControlsPanel.ButtonPadding +
					controls.ConfigButton.Width + BoardControlsPanel.ButtonPadding +
					controls.RefreshButton.Width + BoardControlsPanel.ButtonPadding +
					controls.PreviousButton.Width + BoardControlsPanel.ButtonPadding +
					controls.NextButton.Width + BoardControlsPanel.ButtonPadding +
					controls.PauseResumeButton.Width + BoardControlsPanel.ButtonPadding +
					controls.CountdownLabel.Width + BoardControlsPanel.ButtonPadding;

				AssertEquals("Expanded control should have correct width", correctWidth, controls.Width);
				controls.Collapse(true);
				Application.DoEvents();
				Assert(!controls.SearchBox.Visible);
				Assert(!controls.FilterButton.Visible);
				Assert(!controls.BoardMeetingButton.Visible);
				Assert(!controls.BoardPickerButton.Visible);
				Assert(!controls.OpenInBrowserButton.Visible);
				Assert(!controls.ConfigButton.Visible);
				Assert(!controls.RefreshButton.Visible);
				Assert(!controls.PauseResumeButton.Visible);

				correctWidth = BoardControlsPanel.ButtonPadding +
					controls.ExpandCollapseButton.Width + BoardControlsPanel.ButtonPadding +
					controls.CountdownLabel.Width + BoardControlsPanel.ButtonPadding;
				AssertEquals("Control width should be correct for collapsed", correctWidth, controls.Width);
			}
		}

		public void TestBoardControlsPanel_SingleBoard_ShouldNotShowOpenInBrowserButton()
		{
			var viewModel = CreateBoardWithTasks();

			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				var controls = form.controlsPanel;
				Assert("Control should be collapsed on startup", !controls.IsExpanded);
				Application.DoEvents();

				int expectedWidth = BoardControlsPanel.ButtonPadding + controls.ExpandCollapseButton.Width + BoardControlsPanel.ButtonPadding + controls.CountdownLabel.Width + BoardControlsPanel.ButtonPadding;
				AssertEquals("Control width should be correct for collapsed", expectedWidth, controls.Width);
				AssertEquals("10:00", controls.CountdownLabel.Text);
				controls.Expand();
				Application.DoEvents();
				bool correctControlsOrder = controls.SearchBox.Right < controls.FilterButton.Left &&
											controls.FilterButton.Right < controls.BoardMeetingButton.Left &&
											controls.BoardMeetingButton.Right < controls.BoardPickerButton.Left &&
											controls.BoardPickerButton.Right < controls.ConfigButton.Left &&
											controls.ConfigButton.Right < controls.RefreshButton.Left &&
											controls.RefreshButton.Right < controls.PauseResumeButton.Left &&
											controls.PauseResumeButton.Right < controls.CountdownLabel.Left;
				Assert("Controls on panel should be in correct order", correctControlsOrder);
				Assert(controls.SearchBox.Visible);
				Assert(controls.FilterButton.Visible);
				Assert(controls.BoardMeetingButton.Visible);
				Assert(controls.BoardPickerButton.Visible);
				Assert(controls.ConfigButton.Visible);
				Assert(controls.RefreshButton.Visible);
				Assert(controls.PauseResumeButton.Visible);

				AssertNull("Open in Browser button should not exist", controls.OpenInBrowserButton);

				int correctWidth = BoardControlsPanel.ButtonPadding +
					controls.ExpandCollapseButton.Width + BoardControlsPanel.ButtonPadding +
					controls.SearchBox.Width + BoardControlsPanel.ButtonPadding +
					controls.FilterButton.Width + BoardControlsPanel.ButtonPadding +
					controls.BoardMeetingButton.Width + BoardControlsPanel.ButtonPadding +
					controls.BoardPickerButton.Width + BoardControlsPanel.ButtonPadding +
					controls.ConfigButton.Width + BoardControlsPanel.ButtonPadding +
					controls.RefreshButton.Width + BoardControlsPanel.ButtonPadding +
					controls.PreviousButton.Width + BoardControlsPanel.ButtonPadding +
					controls.NextButton.Width + BoardControlsPanel.ButtonPadding +
					controls.PauseResumeButton.Width + BoardControlsPanel.ButtonPadding +
					controls.CountdownLabel.Width + BoardControlsPanel.ButtonPadding;

				AssertEquals("Expanded control should have correct width", correctWidth, controls.Width);
				controls.Collapse(true);
				Application.DoEvents();
				Assert(!controls.SearchBox.Visible);
				Assert(!controls.FilterButton.Visible);
				Assert(!controls.BoardMeetingButton.Visible);
				Assert(!controls.BoardPickerButton.Visible);
				Assert(!controls.ConfigButton.Visible);
				Assert(!controls.RefreshButton.Visible);
				Assert(!controls.PauseResumeButton.Visible);

				correctWidth = BoardControlsPanel.ButtonPadding + controls.ExpandCollapseButton.Width + BoardControlsPanel.ButtonPadding + controls.CountdownLabel.Width + BoardControlsPanel.ButtonPadding;

				AssertEquals("Control width should be correct for collapsed", correctWidth, controls.Width);
			}
		}

		public void TestBoardControlsPanel_SlideShowBoard()
		{
			var viewModel = CreateSlideShow();
			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				var controls = form.controlsPanel;
				Assert("Control should be collapsed on startup", !controls.IsExpanded);
				Application.DoEvents();

				int correctWidth = BoardControlsPanel.ButtonPadding + controls.ExpandCollapseButton.Width + BoardControlsPanel.ButtonPadding + controls.CountdownLabel.Width + BoardControlsPanel.ButtonPadding;

				AssertEquals("Control width should be correct for collapsed", correctWidth, controls.Width);
				AssertEquals("00:01", controls.CountdownLabel.Text);
				controls.Expand();
				Application.DoEvents();
				bool correctControlsOrder = controls.SearchBox.Right < controls.FilterButton.Left &&
											controls.FilterButton.Right < controls.BoardMeetingButton.Left &&
											controls.BoardMeetingButton.Right < controls.BoardPickerButton.Left &&
											controls.BoardPickerButton.Right < controls.OpenInBrowserButton.Left &&
											controls.OpenInBrowserButton.Right < controls.ConfigButton.Left &&
											controls.ConfigButton.Right < controls.RefreshButton.Left &&
											controls.RefreshButton.Right < controls.PreviousButton.Left &&
											controls.PreviousButton.Right < controls.NextButton.Left &&
											controls.NextButton.Right < controls.PauseResumeButton.Left &&
											controls.PauseResumeButton.Right < controls.CountdownLabel.Left;
				Assert("Controls on panel should be in correct order", correctControlsOrder);
				Assert(controls.SearchBox.Visible);
				Assert(controls.FilterButton.Visible);
				Assert(controls.BoardMeetingButton.Visible);
				Assert(controls.BoardPickerButton.Visible);
				Assert(controls.OpenInBrowserButton.Visible);
				Assert(controls.ConfigButton.Visible);
				Assert(controls.RefreshButton.Visible);
				Assert(controls.PreviousButton.Visible);
				Assert(controls.NextButton.Visible);
				Assert(controls.PauseResumeButton.Visible);

				correctWidth = BoardControlsPanel.ButtonPadding +
					controls.ExpandCollapseButton.Width + BoardControlsPanel.ButtonPadding +
					controls.SearchBox.Width + BoardControlsPanel.ButtonPadding +
					controls.FilterButton.Width + BoardControlsPanel.ButtonPadding +
					controls.BoardMeetingButton.Width + BoardControlsPanel.ButtonPadding +
					controls.BoardPickerButton.Width + BoardControlsPanel.ButtonPadding +
					controls.OpenInBrowserButton.Width + BoardControlsPanel.ButtonPadding +
					controls.ConfigButton.Width + BoardControlsPanel.ButtonPadding +
					controls.RefreshButton.Width + BoardControlsPanel.ButtonPadding +
					controls.PreviousButton.Width + BoardControlsPanel.ButtonPadding +
					controls.NextButton.Width + BoardControlsPanel.ButtonPadding +
					controls.PauseResumeButton.Width + BoardControlsPanel.ButtonPadding +
					controls.CountdownLabel.Width + BoardControlsPanel.ButtonPadding;

				AssertEquals("Expanded control should have correct width", correctWidth, controls.Width);
				controls.Collapse(true);
				Application.DoEvents();
				Assert(!controls.SearchBox.Visible);
				Assert(!controls.FilterButton.Visible);
				Assert(!controls.BoardMeetingButton.Visible);
				Assert(!controls.BoardPickerButton.Visible);
				Assert(!controls.OpenInBrowserButton.Visible);
				Assert(!controls.ConfigButton.Visible);
				Assert(!controls.RefreshButton.Visible);
				Assert(!controls.PreviousButton.Visible);
				Assert(!controls.NextButton.Visible);
				Assert(!controls.PauseResumeButton.Visible);

				correctWidth = BoardControlsPanel.ButtonPadding + controls.ExpandCollapseButton.Width + BoardControlsPanel.ButtonPadding + controls.CountdownLabel.Width + BoardControlsPanel.ButtonPadding;

				AssertEquals("Control width should be correct for collapsed", correctWidth, controls.Width);
			}
		}

		public void TestBoardControlsPanel_FilterButton()
		{
			var viewModel = CreateBoardWithTasks();
			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				var controls = form.controlsPanel;
				Assert("Control should be collapsed on startup", !controls.IsExpanded);
				Application.DoEvents();
				AssertEquals("Control width should be correct for collapsed", ControlDpiScalingHelper.ScaleToCurrentDpiX(3 * BoardControlsPanel.ButtonPadding) + controls.ExpandCollapseButton.Width + controls.CountdownLabel.Width, controls.Width);
				controls.Expand();
				Application.DoEvents();

				var searchControl = controls.SearchBox;
				searchControl.SearchTerm = "owser";
				searchControl.Focus();
				searchControl.OnSearchPerformed(false);

				bool correctControlsOrder = controls.SearchBox.Right < controls.FilterButton.Left &&
											controls.FilterButton.Right < controls.BoardMeetingButton.Left &&
											controls.BoardMeetingButton.Right < controls.BoardPickerButton.Left &&
											controls.BoardPickerButton.Right < controls.OpenInBrowserButton.Left &&
											controls.OpenInBrowserButton.Right < controls.ConfigButton.Left &&
											controls.ConfigButton.Right < controls.RefreshButton.Left &&
											controls.RefreshButton.Right < controls.PauseResumeButton.Left &&
											controls.PauseResumeButton.Right < controls.CountdownLabel.Left;
				Assert("Controls on panel should be in correct order", correctControlsOrder);
				Assert(controls.SearchBox.Visible);
				Assert(controls.FilterButton.Visible);
				Assert(controls.BoardMeetingButton.Visible);
				Assert(controls.BoardPickerButton.Visible);
				Assert(controls.OpenInBrowserButton.Visible);
				Assert(controls.ConfigButton.Visible);
				Assert(controls.RefreshButton.Visible);
				Assert(controls.PauseResumeButton.Visible);

				Assert("Focus should be moved away from search box after search", !controls.SearchBox.ContainsFocus);

				controls.Collapse(true);
				Application.DoEvents();

				Assert(controls.SearchBox.Visible);
				Assert(controls.FilterButton.Visible);
				Assert(!controls.BoardMeetingButton.Visible);
				Assert(!controls.BoardPickerButton.Visible);
				Assert(!controls.OpenInBrowserButton.Visible);
				Assert(!controls.ConfigButton.Visible);
				Assert(!controls.RefreshButton.Visible);
				Assert(!controls.PauseResumeButton.Visible);
				int correctWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding) +
					(controls.SearchBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding)) +
					(controls.FilterButton.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding)) +
					(controls.ExpandCollapseButton.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding)) +
					(controls.CountdownLabel.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding));

				AssertEquals("Collapsed control should have correct width", correctWidth, controls.Width);
				searchControl.OnSearchPerformed(true);
				Application.DoEvents();
				Assert(!controls.SearchBox.Visible);
				Assert(!controls.FilterButton.Visible);
				correctWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding) +
					(controls.ExpandCollapseButton.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding)) +
					(controls.CountdownLabel.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding));

				AssertEquals("Control width should be correct when filter disabled", correctWidth, controls.Width);
			}
		}

		public void TestBoardControlsPanel_BoardMeetingButton()
		{
			var viewModel = CreateBoardWithTasks();
			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				var controls = form.controlsPanel;
				Assert("Control should be collapsed on startup", !controls.IsExpanded);
				Application.DoEvents();

				int expected = ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding) +
					ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding) + controls.ExpandCollapseButton.Width +
					ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding) + controls.CountdownLabel.Width;

				AssertEquals("Control width should be correct for collapsed", expected, controls.Width);
				controls.Expand();
				Application.DoEvents();

				controls.BoardMeetingButton.PerformClick();
				Application.DoEvents();

				controls.Collapse(true);
				Application.DoEvents();

				Assert(!controls.SearchBox.Visible);
				Assert(controls.FilterButton.Visible);
				Assert(controls.BoardMeetingButton.Visible);
				Assert(!controls.BoardPickerButton.Visible);
				Assert(!controls.OpenInBrowserButton.Visible);
				Assert(!controls.ConfigButton.Visible);
				Assert(!controls.RefreshButton.Visible);
				Assert(!controls.PauseResumeButton.Visible);
				int correctWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding) +
					(controls.FilterButton.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding)) +
					(controls.BoardMeetingButton.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding)) +
					(controls.ExpandCollapseButton.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding)) +
					(controls.CountdownLabel.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding));

				AssertEquals("Collapsed control should have correct width", correctWidth, controls.Width);

				controls.Expand();
				Application.DoEvents();
				var searchControl = controls.SearchBox;
				searchControl.SearchTerm = "owser";
				searchControl.OnSearchPerformed(false);
				controls.Collapse(true);
				Application.DoEvents();
				Assert("Search control should be visible", searchControl.Visible);
				controls.BoardMeetingButton.PerformClick();
				Application.DoEvents();
				Assert("Search control should be visible", searchControl.Visible);
				Assert(controls.FilterButton.Visible);
				Assert(!controls.BoardMeetingButton.Visible);
				controls.BoardMeetingButton.PerformClick();
				Application.DoEvents();
				searchControl.OnSearchPerformed(true);
				Application.DoEvents();
				Assert("Search control should not be visible", !searchControl.Visible);
			}
		}

		public void TestBoardControlsPanel_SingleBoardWithNoComponentSections()
		{
			var viewModel = CreateBoardWithModuleSectionOnly();
			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				var controls = form.controlsPanel;
				Assert("Control should be collapsed on startup", !controls.IsExpanded);
				Application.DoEvents();
				int correctWidth = BoardControlsPanel.ButtonPadding + controls.ExpandCollapseButton.Width + BoardControlsPanel.ButtonPadding + controls.CountdownLabel.Width + BoardControlsPanel.ButtonPadding;

				AssertEquals("Control width should be correct for collapsed", correctWidth, controls.Width);
				AssertEquals("10:00", controls.CountdownLabel.Text);
				controls.Expand();
				Application.DoEvents();
				bool correctControlsOrder = controls.BoardPickerButton.Right < controls.OpenInBrowserButton.Left &&
											controls.OpenInBrowserButton.Right < controls.ConfigButton.Left &&
											controls.ConfigButton.Right < controls.RefreshButton.Left &&
											controls.RefreshButton.Right < controls.PauseResumeButton.Left &&
											controls.PauseResumeButton.Right < controls.CountdownLabel.Left;
				Assert("Controls on panel should be in correct order", correctControlsOrder);
				AssertNull(controls.SearchBox);
				Assert(!controls.FilterButton.Visible);
				AssertNull(controls.BoardMeetingButton);
				Assert(controls.BoardPickerButton.Visible);
				Assert(controls.OpenInBrowserButton.Visible);
				Assert(controls.ConfigButton.Visible);
				Assert(controls.RefreshButton.Visible);
				Assert(controls.PauseResumeButton.Visible);
				correctWidth = BoardControlsPanel.ButtonPadding +
					controls.ExpandCollapseButton.Width + BoardControlsPanel.ButtonPadding +
					controls.BoardPickerButton.Width + BoardControlsPanel.ButtonPadding +
					controls.OpenInBrowserButton.Width + BoardControlsPanel.ButtonPadding +
					controls.ConfigButton.Width + BoardControlsPanel.ButtonPadding +
					controls.RefreshButton.Width + BoardControlsPanel.ButtonPadding +
					controls.PreviousButton.Width + BoardControlsPanel.ButtonPadding +
					controls.NextButton.Width + BoardControlsPanel.ButtonPadding +
					controls.PauseResumeButton.Width + BoardControlsPanel.ButtonPadding +
					controls.CountdownLabel.Width + BoardControlsPanel.ButtonPadding;

				AssertEquals("Expanded control should have correct width", correctWidth, controls.Width);
				controls.Collapse(true);
				Application.DoEvents();
				AssertNull(controls.SearchBox);
				Assert(!controls.FilterButton.Visible);
				AssertNull(controls.BoardMeetingButton);
				Assert(!controls.BoardPickerButton.Visible);
				Assert(!controls.OpenInBrowserButton.Visible);
				Assert(!controls.ConfigButton.Visible);
				Assert(!controls.RefreshButton.Visible);
				Assert(!controls.PauseResumeButton.Visible);
				correctWidth = BoardControlsPanel.ButtonPadding +
					controls.ExpandCollapseButton.Width + BoardControlsPanel.ButtonPadding +
					controls.CountdownLabel.Width + BoardControlsPanel.ButtonPadding;

				AssertEquals("Control width should be correct for collapsed", correctWidth, controls.Width);
			}
		}

		public void TestBoardControlsPanel_Slideshow_OneBoardWithComponentSections_OneBoardWithNoComponentSections()
		{
			var viewModel = CreateSlideshowWithModuleSectionBoardAndComponentSectionBoard();
			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				var controls = form.controlsPanel;
				Assert("Control should be collapsed on startup", !controls.IsExpanded);
				Application.DoEvents();
				int correctWidth = BoardControlsPanel.ButtonPadding + controls.ExpandCollapseButton.Width + BoardControlsPanel.ButtonPadding + controls.CountdownLabel.Width + BoardControlsPanel.ButtonPadding;

				AssertEquals("Control width should be correct for collapsed", correctWidth, controls.Width);
				AssertEquals("00:01", controls.CountdownLabel.Text);
				controls.Expand();
				Application.DoEvents();
				bool correctControlsOrder = controls.SearchBox.Right < controls.FilterButton.Left &&
											controls.FilterButton.Right < controls.BoardMeetingButton.Left &&
											controls.BoardMeetingButton.Right < controls.BoardPickerButton.Left &&
											controls.BoardPickerButton.Right < controls.OpenInBrowserButton.Left &&
											controls.OpenInBrowserButton.Right < controls.ConfigButton.Left &&
											controls.ConfigButton.Right < controls.RefreshButton.Left &&
											controls.RefreshButton.Right < controls.PauseResumeButton.Left &&
											controls.PauseResumeButton.Right < controls.CountdownLabel.Left;
				Assert("Controls on panel should be in correct order", correctControlsOrder);
				Assert(controls.SearchBox.Visible);
				Assert(controls.FilterButton.Visible);
				Assert(controls.BoardMeetingButton.Visible);
				Assert(controls.BoardPickerButton.Visible);
				Assert(controls.OpenInBrowserButton.Visible);
				Assert(controls.ConfigButton.Visible);
				Assert(controls.RefreshButton.Visible);
				Assert(controls.PauseResumeButton.Visible);
				correctWidth = BoardControlsPanel.ButtonPadding +
					controls.ExpandCollapseButton.Width + BoardControlsPanel.ButtonPadding +
					controls.SearchBox.Width + BoardControlsPanel.ButtonPadding +
					controls.FilterButton.Width + BoardControlsPanel.ButtonPadding +
					controls.BoardMeetingButton.Width + BoardControlsPanel.ButtonPadding +
					controls.BoardPickerButton.Width + BoardControlsPanel.ButtonPadding +
					controls.OpenInBrowserButton.Width + BoardControlsPanel.ButtonPadding +
					controls.ConfigButton.Width + BoardControlsPanel.ButtonPadding +
					controls.RefreshButton.Width + BoardControlsPanel.ButtonPadding +
					controls.PreviousButton.Width + BoardControlsPanel.ButtonPadding +
					controls.NextButton.Width + BoardControlsPanel.ButtonPadding +
					controls.PauseResumeButton.Width + BoardControlsPanel.ButtonPadding +
					controls.CountdownLabel.Width + BoardControlsPanel.ButtonPadding;

				AssertEquals("Expanded control should have correct width", correctWidth, controls.Width);
				controls.Collapse(true);
				Application.DoEvents();
				Assert(!controls.SearchBox.Visible);
				Assert(!controls.FilterButton.Visible);
				Assert(!controls.BoardMeetingButton.Visible);
				Assert(!controls.BoardPickerButton.Visible);
				Assert(!controls.OpenInBrowserButton.Visible);
				Assert(!controls.ConfigButton.Visible);
				Assert(!controls.RefreshButton.Visible);
				Assert(!controls.PauseResumeButton.Visible);
				correctWidth = BoardControlsPanel.ButtonPadding + controls.ExpandCollapseButton.Width + BoardControlsPanel.ButtonPadding + controls.CountdownLabel.Width + BoardControlsPanel.ButtonPadding;

				AssertEquals("Control width should be correct for collapsed", correctWidth, controls.Width);
			}
		}

		public void TestBoardControlsPanel_Slideshow_AllBoardsWithModuleSections()
		{
			var viewModel = CreateSlideshowWithModuleSectionBoards();
			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				var controls = form.controlsPanel;
				Assert("Control should be collapsed on startup", !controls.IsExpanded);
				Application.DoEvents();

				int correctWidth = BoardControlsPanel.ButtonPadding + controls.ExpandCollapseButton.Width + BoardControlsPanel.ButtonPadding + controls.CountdownLabel.Width + BoardControlsPanel.ButtonPadding;

				AssertEquals("Control width should be correct for collapsed", correctWidth, controls.Width);
				AssertEquals("00:01", controls.CountdownLabel.Text);
				controls.Expand();
				Application.DoEvents();
				bool correctControlsOrder = controls.BoardPickerButton.Right < controls.OpenInBrowserButton.Left &&
											controls.OpenInBrowserButton.Right < controls.ConfigButton.Left &&
											controls.ConfigButton.Right < controls.RefreshButton.Left &&
											controls.RefreshButton.Right < controls.PauseResumeButton.Left &&
											controls.PauseResumeButton.Right < controls.CountdownLabel.Left;
				Assert("Controls on panel should be in correct order", correctControlsOrder);
				AssertNull(controls.SearchBox);
				Assert(!controls.FilterButton.Visible);
				AssertNull(controls.BoardMeetingButton);
				Assert(controls.BoardPickerButton.Visible);
				Assert(controls.OpenInBrowserButton.Visible);
				Assert(controls.ConfigButton.Visible);
				Assert(controls.RefreshButton.Visible);
				Assert(controls.PauseResumeButton.Visible);
				correctWidth = BoardControlsPanel.ButtonPadding +
					controls.ExpandCollapseButton.Width + BoardControlsPanel.ButtonPadding +
					controls.BoardPickerButton.Width + BoardControlsPanel.ButtonPadding +
					controls.OpenInBrowserButton.Width + BoardControlsPanel.ButtonPadding +
					controls.ConfigButton.Width + BoardControlsPanel.ButtonPadding +
					controls.RefreshButton.Width + BoardControlsPanel.ButtonPadding +
					controls.PreviousButton.Width + BoardControlsPanel.ButtonPadding +
					controls.NextButton.Width + BoardControlsPanel.ButtonPadding +
					controls.PauseResumeButton.Width + BoardControlsPanel.ButtonPadding +
					controls.CountdownLabel.Width + BoardControlsPanel.ButtonPadding;

				AssertEquals("Expanded control should have correct width", correctWidth, controls.Width);
				controls.Collapse(true);
				Application.DoEvents();
				AssertNull(controls.SearchBox);
				Assert(!controls.FilterButton.Visible);
				AssertNull(controls.BoardMeetingButton);
				Assert(!controls.BoardPickerButton.Visible);
				Assert(!controls.OpenInBrowserButton.Visible);
				Assert(!controls.ConfigButton.Visible);
				Assert(!controls.RefreshButton.Visible);
				Assert(!controls.PauseResumeButton.Visible);
				correctWidth = BoardControlsPanel.ButtonPadding + controls.ExpandCollapseButton.Width + BoardControlsPanel.ButtonPadding + controls.CountdownLabel.Width + BoardControlsPanel.ButtonPadding;
				AssertEquals("Control width should be correct for collapsed", correctWidth, controls.Width);
			}
		}

		public void TestBoardControlsPanel_CountdownLabel()
		{
			var viewModel = CreateBoardWithTasks();
			viewModel.RefreshSeconds = int.MaxValue;
			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				var controls = form.controlsPanel;
				Assert("Control should be collapsed on startup", !controls.IsExpanded);
				Application.DoEvents();
				int correctWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding) +
					(controls.ExpandCollapseButton.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding)) +
					(controls.CountdownLabel.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(BoardControlsPanel.ButtonPadding));

				AssertEquals("Control width should be correct for collapsed", controls.Width, correctWidth);
				AssertEquals(string.Format("{0}:{1:D2}", (int)TimeSpan.FromSeconds(int.MaxValue).TotalMinutes, TimeSpan.FromSeconds(int.MaxValue).Seconds), controls.CountdownLabel.Text);
				Assert("Control width should be adjusted", controls.CountdownLabel.Width > BoardControlsPanel.countdownWidth);
			}
		}

		public void TestBoardControlsPanel_CountdownLabelFormattingLeadingZeros()
		{
			var viewModel = CreateBoardWithTasks();
			viewModel.RefreshSeconds = 60 * 9 + 9;
			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				var controls = form.controlsPanel;
				Application.DoEvents();
				AssertEquals("09:09", controls.CountdownLabel.Text);
			}
		}

		public void TestBoardControlsPanel_CountdownLabelFormattingTrailingZeros()
		{
			var viewModel = CreateBoardWithTasks();
			viewModel.RefreshSeconds = 60 * 10 + 10;
			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(viewModel))
			{
				form.Show();
				var controls = form.controlsPanel;
				Application.DoEvents();
				AssertEquals("10:10", controls.CountdownLabel.Text);
			}
		}

		public void TestBoardPickerButton_ShouldCacheMenuItems()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var bufferBoard = config.BufferBoard;
			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(bufferBoard)))
			{
				form.Show();
				Application.DoEvents();

				var panel = form.controlsPanel;
				panel.Expand();
				Application.DoEvents();

				var button = panel.BoardPickerButton;

				AssertVisualBoardCommandExecutedCountOnButtonClick("Clicking the button the first time should query the database and load the menu items. SAD!", button, 1);
				var menuItem = button.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Boards not associated with a Release Group");
				menuItem.ShowDropDown();
				AssertEquals(1, menuItem.DropDownItems.Count);

				AssertVisualBoardCommandExecutedCountOnButtonClick("Clicking the button the second time should not query the database because the menu items should be cached. SAD!", button, 0);
				menuItem = button.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Boards not associated with a Release Group");
				menuItem.ShowDropDown();
				AssertEquals(1, menuItem.DropDownItems.Count);

				var bucketBoard = config.BucketBoard;
				Factory.Save();

				form.RefreshNow_ForTest();
				Application.DoEvents();

				AssertVisualBoardCommandExecutedCountOnButtonClick("Refreshing the board should clear the menu item cache, so a new db query should be required in order to reload the menu items. SAD!", button, 1);
				menuItem = button.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == "Boards not associated with a Release Group");
				menuItem.ShowDropDown();
				AssertEquals("The new board should now appear in the list. SAD!", 2, menuItem.DropDownItems.Count);
			}
		}

		public void TestBoardControlsPanel_ExpandAndCollapse()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var bufferBoard = config.BufferBoard;
			Factory.Save();

			using (DisableAsyncBehaviour())
			using (var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(bufferBoard)))
			{
				form.Show();
				Application.DoEvents();

				var panel = form.controlsPanel;
				Assert("Control should be collapsed on startup", !panel.IsExpanded);

				panel.Expand();
				Application.DoEvents();
				Assert("Control should be expanded", panel.IsExpanded);

				panel.Collapse(true);
				Application.DoEvents();
				Assert("Control should be collapsed", !panel.IsExpanded);
			}
		}

		void AssertVisualBoardCommandExecutedCountOnButtonClick(string message, BoardPickerButton button, int expectedExecutions)
		{
			using (TestConnection.TrackExecutedCommands())
			{
				button.PerformClick();
				Application.DoEvents();

				AssertVisualBoardCommandExecutedCount(message, expectedExecutions);
			}
		}

		void AssertVisualBoardCommandExecutedCount(string message, int expectedExecutions)
		{
			var executedMenuCommands = TestConnection.ExecutedCommands.Where(x => x.Contains("VisualBoardMenuItemProvider.GetVisualBoardMenuItems"));
			AssertEquals(message, expectedExecutions, executedMenuCommands.Count());
		}

		BoardSlideshowViewModel CreateBoardWithTasks()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var section = BMSTestHelper.CreateBoardSection(bucket);

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, description: "Yowser");
			var task2 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, description: "Bowser");

			var board1 = BMSTestHelper.CreateBoard(system, "board1", "board1");
			var section1 = BMSTestHelper.CreateBoardSection(bucket, board1);
			section1.BackgroundColor = "Chartreuse";
			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1);

			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();

			return VisualBoardsTestHelper.CreateSlideshowViewModel(slideshow);
		}

		BoardSlideshowViewModel CreateBoardWithModuleSectionOnly()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var board = BMSTestHelper.CreateBoard(system, "board1", "board1");
			var moduleSection = board.Sections.AddNew();
			moduleSection.MS_SectionType = BMConstants.ModuleGridSectionType;
			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board);

			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();

			return VisualBoardsTestHelper.CreateSlideshowViewModel(slideshow);
		}

		BoardSlideshowViewModel CreateSlideShow()
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

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board1, 1), Tuple.Create(board2, 1), Tuple.Create(board3, 3), Tuple.Create(board4, 1));

			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();

			return VisualBoardsTestHelper.CreateSlideshowViewModel(slideshow);
		}

		BoardSlideshowViewModel CreateSlideshowWithModuleSectionBoardAndComponentSectionBoard()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system, "bucket", offsetMinutes: 0, sequence: 0);

			var board1 = BMSTestHelper.CreateBoard(system, "board1", "board1");
			var board2 = BMSTestHelper.CreateBoard(system, "board2", "board2");

			var section1 = BMSTestHelper.CreateBoardSection(bucket, board1);
			var section2 = board2.Sections.AddNew();
			section2.MS_SectionType = BMConstants.ModuleGridSectionType;

			section1.BackgroundColor = "Blue";
			section2.BackgroundColor = "HotPink";

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board1, 1), Tuple.Create(board2, 1));

			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();

			return VisualBoardsTestHelper.CreateSlideshowViewModel(slideshow);
		}

		BoardSlideshowViewModel CreateSlideshowWithModuleSectionBoards()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var board1 = BMSTestHelper.CreateBoard(system, "board1", "board1");
			var board2 = BMSTestHelper.CreateBoard(system, "board2", "board2");

			var section1 = board2.Sections.AddNew();
			section1.MS_SectionType = BMConstants.ModuleGridSectionType;
			var section2 = board2.Sections.AddNew();
			section2.MS_SectionType = BMConstants.ModuleGridSectionType;

			section1.BackgroundColor = "Blue";
			section2.BackgroundColor = "HotPink";

			var slideshow = BMSTestHelper.CreateSlideshow(Factory, Tuple.Create(board1, 1), Tuple.Create(board2, 1));

			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();

			return VisualBoardsTestHelper.CreateSlideshowViewModel(slideshow);
		}
	}
}
